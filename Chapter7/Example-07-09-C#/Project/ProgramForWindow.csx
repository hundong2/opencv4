// NuGet에서 OpenCvSharp4 라이브러리 참조 (버전: 4.13.0.20260602)
// #r "nuget: OpenCvSharp4, 4.13.0.20260602"
// // Linux x64 환경용 OpenCV 네이티브 런타임 바이너리 참조
// #r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"

#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.Windows, 4.13.0.20260602" // Windows용 런타임

using System;
using OpenCvSharp;
using System.Runtime.CompilerServices;  // CallerFilePath 특성을 사용하기 위해 필요
using System.IO;

// ScriptDir 함수는 현재 실행 중인 .csx 파일의 경로를 기준으로
// 예제 이미지가 있는 폴더 위치를 구할 때 사용한다.
string ScriptDir([CallerFilePath] string path = "")
    => Path.GetDirectoryName(path)!;

// 현재 스크립트 파일이 위치한 디렉터리 경로
string scriptDir = ScriptDir();

// ImRead 함수는 지정한 경로의 이미지를 Mat 객체로 읽어 온다.
// 이 예제는 카드 이미지에서 직선을 검출한 뒤 원본 컬러 이미지 위에 그려서 확인한다.
Mat src = Cv2.ImRead(Path.Combine(scriptDir, "bin/Debug/card.jpg"));

// gray   : 흑백 변환 이미지
// binary : 임계값으로 흑백화한 이미지
// morp   : 팽창/침식 연산으로 형태를 정리한 이미지
// canny  : Canny 에지 검출 결과
// dst    : 검출한 직선을 그려 넣을 출력 이미지
Mat gray = new Mat();
Mat binary = new Mat();
Mat morp = new Mat();
Mat canny = new Mat();
Mat dst = src.Clone();

// GetStructuringElement 함수는 모폴로지 연산에 사용할 커널을 만든다.
// Rect 모양의 3x3 커널은 주변 8방향 픽셀을 함께 보며 팽창/침식을 수행하게 한다.
Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));

// CvtColor 함수는 이미지의 색상 공간을 변환한다.
// 직선 검출 전처리는 색상보다 밝기 정보가 중요하므로 BGR 컬러 이미지를 그레이스케일로 바꾼다.
Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

// Threshold 함수는 픽셀 밝기를 기준으로 이진 이미지를 만든다.
// 여기서는 밝기가 150보다 크면 255(흰색), 아니면 0(검은색)으로 바꾼다.
Cv2.Threshold(gray, binary, 150, 255, ThresholdTypes.Binary);

// Dilate 함수는 흰색 영역을 확장한다.
// 작은 끊김이나 틈을 메워 이후 직선 후보가 더 이어져 보이도록 만든다.
// anchor가 (-1, -1)이면 커널의 중심점을 기준으로 사용한다.
Cv2.Dilate(binary, morp, kernel, new Point(-1, -1));

// Erode 함수는 흰색 영역을 줄인다.
// 팽창 후 두꺼워진 영역이나 작은 잡음을 깎아 형태를 정리한다.
// 마지막 인수 3은 침식 연산을 3번 반복한다는 뜻이다.
Cv2.Erode(morp, morp, kernel, new Point(-1, -1), 3);

// 다시 Dilate를 적용해 침식으로 얇아진 주요 영역을 적당히 복원한다.
// 마지막 인수 2는 팽창 연산을 2번 반복한다는 뜻이다.
Cv2.Dilate(morp, morp, kernel, new Point(-1, -1), 2);

// Canny 함수는 이미지에서 에지를 검출한다.
// 앞에서 정리한 이진/모폴로지 결과를 입력으로 사용해 카드 테두리 같은 경계선을 뽑는다.
// 0, 0은 하위/상위 임계값이고, 3은 Sobel 커널 크기(apertureSize)이다.
Cv2.Canny(morp, canny, 0, 0, 3);

// HoughLinesP 함수는 확률적 허프 변환으로 직선 "선분"을 검출한다.
//
// 매개변수:
// canny      : 입력 에지 이미지
// 1          : rho. 거리 해상도이며 1픽셀 단위로 누적 공간을 계산
// Cv2.PI/180 : theta. 각도 해상도이며 1도 단위
// 140        : threshold. 직선으로 인정하기 위한 최소 누적 투표 수
// 50         : minLineLength. 검출할 선분의 최소 길이
// 10         : maxLineGap. 같은 직선으로 이어 붙일 수 있는 선분 사이 최대 간격
LineSegmentPoint[] lines = Cv2.HoughLinesP(canny, 1, Cv2.PI / 180, 140, 50, 10);

// Line 함수는 이미지 위에 선분을 그린다.
// HoughLinesP가 반환한 각 선분의 시작점(P1)과 끝점(P2)을 노란색 두께 2로 표시한다.
for (int i = 0; i < lines.Length; i++)
{
    Cv2.Line(dst, lines[i].P1, lines[i].P2, Scalar.Yellow, 2);
}

// ImShow 함수는 지정한 이름의 창에 이미지를 표시한다.
// WaitKey(0)는 키 입력이 있을 때까지 창을 유지하고,
// DestroyAllWindows는 열린 OpenCV 창을 모두 닫는다.
Cv2.ImShow("dst", dst);
Cv2.WaitKey(0);
Cv2.DestroyAllWindows();
