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

// =====================================================================
// 예제 07-11: 허프 원 변환(Hough Circle Transform)을 이용한 원 검출
// 컬러 볼 이미지에서 원을 검출하고 원본 이미지 위에 검출된 원을 그린다.
// =====================================================================

// ScriptDir 함수는 현재 실행 중인 .csx 파일의 경로를 기준으로
// 예제 이미지가 있는 폴더 위치를 구할 때 사용한다.
// [CallerFilePath] 특성은 컴파일 시점에 호출자의 파일 경로를 자동으로 삽입한다.
string ScriptDir([CallerFilePath] string path = "")
    => Path.GetDirectoryName(path)!;

// 현재 스크립트 파일이 위치한 디렉터리 경로
string scriptDir = ScriptDir();

// Cv2.ImRead(path, flags)
//   - 지정한 경로의 이미지 파일을 읽어 Mat 객체로 반환한다.
//   - flags 생략 시 기본값 ImreadModes.Color (BGR 컬러)로 로드된다.
// src  : 원본 컬러 이미지 (BGR)
// image: 전처리에 사용할 이미지 (그레이스케일로 변환 예정)
// dst  : 검출된 원을 그릴 출력 이미지 (src의 복사본)
Mat src = Cv2.ImRead(Path.Combine(scriptDir, "bin/Debug/colorball.png"));
Mat image = new Mat();
Mat dst = src.Clone();  // src를 복사하여 결과 이미지로 사용

// Cv2.GetStructuringElement(shape, ksize)
//   - 모폴로지 연산(팽창/침식)에 사용할 구조 요소(커널)를 생성한다.
//   - MorphShapes.Rect : 사각형 형태의 구조 요소
//   - new Size(3, 3)   : 커널 크기 3×3
Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));

// Cv2.CvtColor(src, dst, code)
//   - 이미지의 색 공간을 변환한다.
//   - ColorConversionCodes.BGR2GRAY : BGR 컬러 → 그레이스케일
//   - 허프 원 변환은 단채널(그레이스케일) 이미지를 입력으로 사용한다.
Cv2.CvtColor(src, image, ColorConversionCodes.BGR2GRAY);

// Cv2.Dilate(src, dst, kernel, anchor, iterations)
//   - 팽창(Dilation) 연산: 밝은 영역을 확장하여 노이즈 제거 및 경계 강화에 사용한다.
//   - anchor(-1, -1)  : 커널의 중심을 앵커로 사용 (기본값)
//   - iterations(3)   : 반복 횟수 (3회 반복)
Cv2.Dilate(image, image, kernel, new Point(-1, -1), 3);

// Cv2.GaussianBlur(src, dst, ksize, sigmaX, sigmaY, borderType)
//   - 가우시안 블러로 이미지를 부드럽게 하여 원 검출 시 오검출을 줄인다.
//   - ksize(13, 13)             : 블러 커널 크기 (홀수여야 함)
//   - sigmaX, sigmaY(3, 3)      : X/Y 방향 표준 편차 (클수록 더 많이 흐려짐)
//   - BorderTypes.Reflect101    : 경계 픽셀 처리 방식 (대칭 반사)
Cv2.GaussianBlur(image, image, new Size(13, 13), 3, 3, BorderTypes.Reflect101);

// Cv2.Erode(src, dst, kernel, anchor, iterations)
//   - 침식(Erosion) 연산: 어두운 영역을 확장하여 팽창으로 확대된 노이즈를 제거한다.
//   - 팽창 → 블러 → 침식 순서로 전처리하면 원의 경계가 더 선명해진다.
Cv2.Erode(image, image, kernel, new Point(-1, -1), 3);

// Cv2.HoughCircles(image, method, dp, minDist, param1, param2, minRadius, maxRadius)
//   - 허프 원 변환으로 이미지에서 원을 검출하고 CircleSegment 배열로 반환한다.
//   - image     : 단채널 그레이스케일 입력 이미지 (8비트)
//   - method    : 검출 방법 (HoughModes.Gradient: 21HT 기반 기본 방식)
//   - dp(1)     : 해상도 비율 (1=원본 해상도, 2=절반 해상도로 누산)
//   - minDist(100) : 검출된 원의 중심 간 최소 거리 (너무 작으면 중복 검출 발생)
//   - param1(100)  : Canny 엣지 검출기의 상위 임계값 (내부적으로 사용)
//   - param2(35)   : 원 중심 후보의 누산 임계값 (작을수록 더 많은 원 검출, 오검출 증가)
//   - minRadius(0) : 검출할 원의 최소 반지름 (0=제한 없음)
//   - maxRadius(0) : 검출할 원의 최대 반지름 (0=제한 없음)
CircleSegment[] circles = Cv2.HoughCircles(image, HoughModes.Gradient, 1, 100, 100, 35, 0, 0);

// 검출된 원의 개수만큼 반복하며 원본 이미지(dst) 위에 원을 그린다.
for (int i = 0; i < circles.Length; i++)
{
    // CircleSegment.Center : 검출된 원의 중심 좌표 (Point2f)
    // CircleSegment.Radius : 검출된 원의 반지름
    Point center = new Point(circles[i].Center.X, circles[i].Center.Y);

    // Cv2.Circle(img, center, radius, color, thickness)
    //   - 원의 외곽선을 흰색(Scalar.White)으로 두께 3픽셀로 그린다.
    Cv2.Circle(dst, center, (int)circles[i].Radius, Scalar.White, 3);

    //   - 원의 중심점을 AntiqueWhite 색으로 반지름 5픽셀의 채워진 원(Cv2.FILLED)으로 표시한다.
    Cv2.Circle(dst, center, 5, Scalar.AntiqueWhite, Cv2.FILLED);
}

// Cv2.ImShow(windowName, mat) : 지정한 이름의 창에 이미지를 출력한다.
// Cv2.WaitKey(delay)          : delay 밀리초 동안 키 입력을 대기한다. (0=무한 대기)
// Cv2.DestroyAllWindows()     : 열려 있는 모든 OpenCV 창을 닫는다.
Cv2.ImShow("dst", dst);
Cv2.WaitKey(0);
Cv2.DestroyAllWindows();