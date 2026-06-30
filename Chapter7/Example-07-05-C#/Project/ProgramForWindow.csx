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
// 다각형 근사 결과를 원본 위에 그리기 위해 컬러 이미지로 읽는다.
Mat src = Cv2.ImRead(Path.Combine(scriptDir, "bin/Debug/chess.png"));

// gray   : 원본 이미지를 흑백으로 변환한 이미지
// binary : 임계값 처리로 흰색/검은색만 남긴 이진 이미지
// morp   : 모폴로지 닫힘 연산으로 작은 구멍이나 끊어진 부분을 보정한 이미지
// image  : FindContours에 넣기 위해 흑백을 반전한 이미지
// dst    : 근사된 다각형과 꼭짓점을 그릴 결과 이미지
Mat gray = new Mat();
Mat binary = new Mat();
Mat morp = new Mat();
Mat image = new Mat();
Mat dst = src.Clone();

// 3x3 사각형 구조 요소를 만든다.
// MorphTypes.Close에서 흰색 영역을 연결하고 작은 구멍을 메울 때 사용된다.
Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));

// FindContours가 찾은 윤곽선 좌표 배열.
// contours[i]는 i번째 윤곽선이고, contours[i][j]는 해당 윤곽선을 이루는 j번째 점이다.
Point[][] contours;

// 윤곽선의 계층 구조 정보.
// RetrievalModes.Tree를 사용하면 바깥 윤곽선/안쪽 윤곽선 같은 부모-자식 관계가 저장된다.
HierarchyIndex[] hierarchy;

// 1. BGR 컬러 이미지를 그레이스케일로 변환한다.
// 윤곽선 검출은 보통 색상보다 밝기 차이를 기준으로 하기 때문에 1채널 이미지로 줄인다.
Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

// 2. 이진화한다.
// 픽셀값이 230보다 크면 255(흰색), 아니면 0(검은색)이 된다.
// FindContours는 보통 0/255로 나뉜 이진 이미지에서 가장 명확하게 동작한다.
Cv2.Threshold(gray, binary, 230, 255, ThresholdTypes.Binary);

// 3. 닫힘(Close) 연산을 적용한다.
// Close = Dilate(팽창) 후 Erode(침식)
// 작은 빈틈을 메우고 가까운 흰색 영역을 연결해서 윤곽선이 끊기지 않게 돕는다.
// 마지막 인자 2는 이 연산을 2번 반복한다는 의미이다.
Cv2.MorphologyEx(binary, morp, MorphTypes.Close, kernel, new Point(-1, -1), 2);

// 4. 흑백을 반전한다.
// FindContours는 흰색(255) 영역을 객체로 보고 윤곽선을 찾는다.
// 검출하고 싶은 선/영역이 검은색으로 남아 있다면 반전해서 흰색 객체로 만든다.
Cv2.BitwiseNot(morp, image);

// 5. 윤곽선을 찾는다.
// RetrievalModes.Tree      : 모든 윤곽선을 계층 구조까지 포함해 찾는다.
// ApproxTC89KCOS           : 윤곽선을 Teh-Chin 방식으로 1차 근사해 불필요한 점을 줄인다.
// 이후 아래에서 ApproxPolyDP로 한 번 더 다각형 형태로 단순화한다.
Cv2.FindContours(image, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxTC89KCOS);

// 6. 찾은 모든 윤곽선에 대해 다각형 근사를 수행한다.
for (int i = 0; i< contours.Length; i++)
{
    // 현재 윤곽선의 둘레 길이를 계산한다.
    // 두 번째 인자 true는 윤곽선이 닫힌 도형이라는 의미이다.
    double perimeter = Cv2.ArcLength(contours[i], true);

    // Douglas-Peucker 알고리즘의 허용 오차.
    // epsilon이 작으면 원본 윤곽선을 더 많이 보존하고,
    // epsilon이 크면 더 적은 점으로 거칠게 근사한다.
    // 여기서는 윤곽선 둘레의 1%를 허용 오차로 사용한다.
    double epsilon = perimeter * 0.01;

    // 윤곽선을 더 적은 수의 꼭짓점으로 근사한다.
    // 예: 많은 점으로 구성된 사각형 외곽선 -> 꼭짓점 4개에 가까운 다각형
    // 세 번째 인자 true는 시작점과 끝점을 연결한 닫힌 도형으로 처리한다는 의미이다.
    Point[] approx = Cv2.ApproxPolyDP(contours[i], epsilon, true);

    // DrawContours는 Point[][] 형태를 받으므로,
    // 하나의 근사 다각형 approx를 다시 배열로 감싼다.
    Point[][] draw_approx = new Point[][] { approx };

    // 근사된 다각형을 파란색 선으로 그린다.
    // new Scalar(255, 0, 0)은 BGR 기준 파란색이다.
    Cv2.DrawContours(dst, draw_approx, -1, new Scalar(255, 0, 0), 2, LineTypes.AntiAlias);

    // 근사된 다각형의 꼭짓점들을 빨간색 점으로 표시한다.
    // 점 개수를 보면 삼각형, 사각형, 다각형 같은 형태 판단에 활용할 수 있다.
    for (int j = 0; j < approx.Length; j++)
    {
        // new Scalar(0, 0, 255)는 BGR 기준 빨간색이다.
        Cv2.Circle(dst, approx[j], 1, new Scalar(0, 0, 255), 3);
    }
}

// 결과 이미지 표시
Cv2.ImShow("dst", dst);
Cv2.WaitKey(0);
Cv2.DestroyAllWindows();
