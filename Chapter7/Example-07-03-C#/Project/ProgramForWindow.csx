// NuGet에서 OpenCvSharp4 라이브러리 참조 (버전: 4.13.0.20260602)
// #r "nuget: OpenCvSharp4, 4.13.0.20260602"
// // Linux x64 환경용 OpenCV 네이티브 런타임 바이너리 참조
// #r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"

#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.Windows, 4.13.0.20260602" // ✅ Windows용

using System;
using OpenCvSharp;
using System.Runtime.CompilerServices;  // CallerFilePath 어트리뷰트 사용에 필요
using System.IO;

string ScriptDir([CallerFilePath] string path = "") 
    => Path.GetDirectoryName(path)!;

// 현재 스크립트 파일이 위치한 디렉터리 경로
string scriptDir = ScriptDir();

// 체스판 이미지를 읽어온다.
// FindContours는 보통 흑백 이진 이미지에서 동작하지만,
// 윤곽선을 원본 위에 그리기 위해 원본 컬러 이미지도 함께 보관한다.
Mat src = Cv2.ImRead(Path.Combine(scriptDir, "bin/Debug/chess.png"));

// gray   : 원본 이미지를 흑백으로 변환한 이미지
// binary : 임계값 처리로 흰색/검은색만 남긴 이진 이미지
// morp   : 모폴로지 연산으로 작은 구멍이나 끊어진 부분을 보정한 이미지
// image  : FindContours에 넣기 위해 흑백을 반전한 이미지
// dst    : 검출된 윤곽선과 점을 그릴 결과 이미지
Mat gray = new Mat();
Mat binary = new Mat();
Mat morp = new Mat();
Mat image = new Mat();
Mat dst = src.Clone();

// 3x3 사각형 구조 요소를 만든다.
// MorphTypes.Close 연산에서 주변 픽셀을 어느 범위까지 볼지 결정한다.
Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));

// FindContours가 찾아낸 윤곽선 좌표들이 저장될 배열.
// contours[i]는 i번째 윤곽선이고, contours[i][j]는 그 윤곽선을 이루는 j번째 점이다.
Point[][] contours;

// 윤곽선 사이의 계층 구조 정보가 저장된다.
// 예: 바깥 윤곽선 안에 구멍 윤곽선이 있는 경우 부모/자식 관계를 표현한다.
HierarchyIndex[] hierarchy;

// 1. 컬러 이미지를 그레이스케일로 변환한다.
// 윤곽선 검출은 색상보다 밝기 차이를 기준으로 하는 경우가 많으므로,
// BGR 3채널 이미지를 1채널 밝기 이미지로 줄인다.
Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

// 2. 임계값 처리로 이진 이미지를 만든다.
// gray 픽셀값이 230보다 크면 255(흰색), 아니면 0(검은색)이 된다.
// 체스판처럼 밝은 배경/어두운 선이 분명한 이미지에서 형태를 분리하기 위한 전처리이다.
Cv2.Threshold(gray, binary, 230, 255, ThresholdTypes.Binary);

// 3. 닫힘(Close) 연산을 적용한다.
// Close = Dilate(팽창) 후 Erode(침식)
// 작은 구멍을 메우고, 가까이 있는 흰색 영역을 연결하는 데 유용하다.
// iterations: 2이므로 닫힘 연산을 2번 반복한다.
Cv2.MorphologyEx(binary, morp, MorphTypes.Close, kernel, new Point(-1, -1), 2);

// 4. 흰색과 검은색을 반전한다.
// OpenCV의 FindContours는 일반적으로 흰색(255) 영역을 객체로 보고 윤곽선을 찾는다.
// 현재 이미지에서 검출하고 싶은 선/영역이 검은색이면 반전해서 흰색 객체로 만든다.
Cv2.BitwiseNot(morp, image);

// 5. 윤곽선을 찾는다.
// image              : 입력 이진 이미지. 보통 0과 255로 구성되어야 한다.
// contours           : 검출된 윤곽선 좌표 배열
// hierarchy          : 윤곽선의 부모/자식 관계 정보
// RetrievalModes.Tree: 모든 윤곽선을 계층 구조까지 포함해 찾는다.
// ApproxTC89KCOS     : Teh-Chin 알고리즘으로 윤곽선을 더 적은 점으로 근사한다.
Cv2.FindContours(image, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxTC89KCOS);

// 6. 검출된 모든 윤곽선을 결과 이미지에 그린다.
// contourIdx: -1은 모든 윤곽선을 그린다는 의미이다.
// color     : new Scalar(255, 0, 0)은 BGR 기준 파란색이다.
// thickness : 2픽셀 두께
// maxLevel  : 3단계 깊이의 계층 윤곽선까지 그림
Cv2.DrawContours(dst, contours, -1, new Scalar(255, 0, 0), 2, LineTypes.AntiAlias, hierarchy, 3);

// 7. 각 윤곽선을 이루는 점들을 빨간색 원으로 표시한다.
// DrawContours는 윤곽선 전체를 선으로 그려주고,
// Circle은 실제로 윤곽선이 어떤 점들의 집합으로 저장되는지 보여준다.
for (int i = 0; i< contours.Length; i++)
{
    for (int j = 0; j < contours[i].Length; j++)
    {
        // BGR 기준 new Scalar(0, 0, 255)는 빨간색이다.
        Cv2.Circle(dst, contours[i][j], 1, new Scalar(0, 0, 255), 3);
    }
}

// 결과 이미지 표시
Cv2.ImShow("dst", dst);
Cv2.WaitKey(0);
Cv2.DestroyAllWindows();
