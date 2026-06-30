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

// 예제 이미지를 컬러로 읽어온다.
// 코너 검출 자체는 밝기 변화만으로 수행하지만,
// 검출된 위치를 노란색/빨간색 점으로 표시하기 위해 원본 컬러 이미지를 사용한다.
Mat src = Cv2.ImRead(Path.Combine(scriptDir, "bin/Debug/dummy.jpg"));

// gray : 코너 검출에 사용할 흑백 이미지
// dst  : 검출 결과를 그릴 출력 이미지
Mat gray = new Mat();
Mat dst = src.Clone();

// BGR 컬러 이미지를 그레이스케일로 변환한다.
// 코너는 색상 자체보다 "밝기가 여러 방향으로 급격히 변하는 지점"이므로,
// 보통 1채널 흑백 이미지에서 검출한다.
Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

// GoodFeaturesToTrack는 추적하기 좋은 특징점, 즉 코너 후보를 찾는 함수이다.
//
// 기초 개념:
// - 평탄한 영역: 어느 방향으로 움직여도 밝기 변화가 거의 없음 → 특징점으로 부적합
// - 선/엣지: 한 방향으로는 밝기 변화가 크지만, 선을 따라 움직이면 변화가 작음
// - 코너: x/y 여러 방향으로 모두 밝기 변화가 큼 → 위치를 다시 찾기 쉬움
//
// 그래서 코너는 객체 추적, 카메라 움직임 추정, 이미지 정합에서 좋은 기준점이 된다.
//
// 매개변수:
// gray        : 입력 흑백 이미지
// 100         : 최대 100개의 코너만 반환
// 0.03        : qualityLevel. 가장 강한 코너 응답의 3% 이상인 점만 사용
// 5           : minDistance. 검출된 코너들 사이의 최소 거리
// null        : mask. null이면 이미지 전체에서 찾음
// 3           : blockSize. 코너 판단에 사용할 주변 영역 크기
// false       : useHarrisDetector. false면 Shi-Tomasi 방식 사용
// 0           : Harris 방식에서 쓰는 k값. 여기서는 false라 거의 영향 없음
Point2f[] corners = Cv2.GoodFeaturesToTrack(gray, 100, 0.03, 5, null, 3, false, 0);

// CornerSubPix는 정수 픽셀 단위로 찾은 코너 위치를 더 정밀하게 보정한다.
//
// GoodFeaturesToTrack 결과는 대략적인 픽셀 위치이다.
// 하지만 실제 코너는 픽셀과 픽셀 사이의 소수점 좌표에 있을 수 있다.
// CornerSubPix는 주변 밝기 패턴을 반복적으로 분석해 코너 위치를 서브픽셀 단위로 개선한다.
//
// 매개변수:
// gray              : 입력 흑백 이미지
// corners           : 보정할 초기 코너 좌표
// new Size(3, 3)    : 검색 윈도우 크기. 코너 주변 3x3 영역을 보며 보정
// new Size(-1, -1)  : zeroZone. 사용하지 않을 중앙 영역 없음
// TermCriteria.Both : 반복 종료 조건. 횟수 또는 오차 기준 중 하나를 만족하면 종료
// (10, 0.03)        : 최대 10번 반복하거나 위치 변화가 0.03보다 작아지면 종료
Point2f[] sub_corners = Cv2.CornerSubPix(gray, corners, new Size(3, 3), new Size(-1, -1), TermCriteria.Both(10, 0.03));

// GoodFeaturesToTrack로 처음 찾은 코너 후보를 노란색 점으로 표시한다.
// Point2f는 소수점 좌표를 가질 수 있지만, Circle로 그릴 때는 정수 Point로 변환한다.
for (int i = 0; i < corners.Length; i++)
{
    Point pt = new Point((int)corners[i].X, (int)corners[i].Y);
    Cv2.Circle(dst, pt, 5, Scalar.Yellow, Cv2.FILLED);
}

// CornerSubPix로 보정한 코너 위치를 빨간색 점으로 표시한다.
// 노란색 점과 빨간색 점이 살짝 어긋나 보이면,
// 서브픽셀 보정으로 위치가 더 정밀하게 이동했다는 의미이다.
for (int i = 0; i < sub_corners.Length; i++)
{
    Point pt = new Point((int)sub_corners[i].X, (int)sub_corners[i].Y);
    Cv2.Circle(dst, pt, 5, Scalar.Red, Cv2.FILLED);
}

// 결과 이미지 표시
Cv2.ImShow("dst", dst);
Cv2.WaitKey(0);
Cv2.DestroyAllWindows();
