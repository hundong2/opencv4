// NuGet에서 OpenCvSharp4 라이브러리 참조 (버전: 4.13.0.20260602)
// #r "nuget: OpenCvSharp4, 4.13.0.20260602"
// // Linux x64 환경용 OpenCV 네이티브 런타임 바이너리 참조
// #r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"

#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.Windows, 4.13.0.20260602" // Windows용

using System;
using OpenCvSharp;
using System.Runtime.CompilerServices;
using System.IO;

string ScriptDir([CallerFilePath] string path = "")
    => Path.GetDirectoryName(path)!;

string scriptDir = ScriptDir();

// 예제 이미지를 흑백으로 읽는다.
// 엣지 검출은 색상보다 밝기 변화에 집중하므로 보통 그레이스케일에서 수행한다.
Mat src = Cv2.ImRead(Path.Combine(scriptDir, "bin/Debug/dummy.jpg"), ImreadModes.Grayscale);

if (src.Empty())
{
    throw new Exception("이미지를 불러오지 못했습니다. bin/Debug/dummy.jpg 경로를 확인하세요.");
}

// 미분 기반 엣지 검출은 노이즈에 민감하다.
// 먼저 약한 가우시안 블러를 적용하면 작은 잡음이 줄어 결과가 더 안정적이다.
Mat blur = new Mat();
Cv2.GaussianBlur(src, blur, new Size(3, 3), 0);

// ──────────────────────────────────────────────
// 1. Scharr 필터
// ──────────────────────────────────────────────
// Scharr는 3x3 Sobel보다 방향별 미분 정확도가 좋은 1차 미분 필터이다.
// X 방향 미분은 세로 경계, Y 방향 미분은 가로 경계를 잘 찾는다.
Mat scharrX = new Mat();
Mat scharrY = new Mat();
Mat absScharrX = new Mat();
Mat absScharrY = new Mat();
Mat scharrEdge = new Mat();

Cv2.Scharr(blur, scharrX, MatType.CV_16S, 1, 0);
Cv2.Scharr(blur, scharrY, MatType.CV_16S, 0, 1);

// 미분 결과에는 음수가 포함될 수 있으므로 화면 표시용으로 절댓값 8비트 이미지로 변환한다.
Cv2.ConvertScaleAbs(scharrX, absScharrX);
Cv2.ConvertScaleAbs(scharrY, absScharrY);

// X/Y 방향 엣지를 반반 섞어 전체 엣지 느낌의 이미지를 만든다.
Cv2.AddWeighted(absScharrX, 0.5, absScharrY, 0.5, 0, scharrEdge);

// ──────────────────────────────────────────────
// 2. Laplacian 필터
// ──────────────────────────────────────────────
// Laplacian은 2차 미분 필터이다.
// 밝기가 증가했다가 감소하는 지점처럼 변화의 방향이 바뀌는 곳을 강하게 반응한다.
Mat laplacian16 = new Mat();
Mat laplacianEdge = new Mat();

Cv2.Laplacian(blur, laplacian16, MatType.CV_16S, ksize: 3);
Cv2.ConvertScaleAbs(laplacian16, laplacianEdge);

// ──────────────────────────────────────────────
// 3. Canny 엣지
// ──────────────────────────────────────────────
// Canny는 블러, 기울기 계산, 얇은 선 추림, 이중 임계값 처리를 조합한 엣지 검출 알고리즘이다.
// threshold1: 약한 엣지 기준, threshold2: 강한 엣지 기준
Mat cannyEdge = new Mat();
Cv2.Canny(blur, cannyEdge, threshold1: 50, threshold2: 150, apertureSize: 3, L2gradient: true);

Cv2.ImShow("src - grayscale", src);
Cv2.ImShow("Scharr - 1st derivative edge", scharrEdge);
Cv2.ImShow("Laplacian - 2nd derivative edge", laplacianEdge);
Cv2.ImShow("Canny - clean binary edge", cannyEdge);

Cv2.WaitKey(0);
Cv2.DestroyAllWindows();
