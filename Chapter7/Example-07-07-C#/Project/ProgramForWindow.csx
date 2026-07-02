// NuGet에서 OpenCvSharp4 라이브러리 참조 (버전: 4.13.0.20260602)
// #r "nuget: OpenCvSharp4, 4.13.0.20260602"
// // Linux x64 환경용 OpenCV 네이티브 바이너리 참조
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
// 여기서는 코너 검출 결과를 색상 점으로 표시해야 하므로 컬러 원본 이미지를 유지한다.
Mat src = Cv2.ImRead(Path.Combine(scriptDir, "bin/Debug/dummy.jpg"));

// gray : 코너 검출에 사용할 흑백 이미지
// dst  : 검출 결과를 그려 넣을 출력 이미지
Mat gray = new Mat();
Mat dst = src.Clone();

// CvtColor 함수는 이미지의 색상 공간을 변환한다.
// GoodFeaturesToTrack은 밝기 변화로 코너를 찾으므로 BGR 컬러 이미지를
// 1채널 그레이스케일 이미지로 변환해서 사용한다.
Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

// GoodFeaturesToTrack 함수는 추적하기 좋은 특징점, 즉 코너 후보를 찾는다.
//
// 기본 개념:
// - 평탄한 영역: 어느 방향으로 움직여도 밝기 변화가 거의 없어 특징점으로 부적합
// - 에지: 한 방향으로는 밝기 변화가 크지만 다른 방향으로는 변화가 작음
// - 코너: x/y 여러 방향 모두에서 밝기 변화가 커서 다시 찾기 쉬운 위치
//
// 매개변수:
// gray        : 입력 흑백 이미지
// 100         : 최대 100개의 코너만 반환
// 0.03        : qualityLevel. 가장 강한 코너 응답의 3% 이상인 점만 사용
// 5           : minDistance. 검출된 코너 사이의 최소 거리
// null        : mask. null이면 이미지 전체에서 찾음
// 3           : blockSize. 코너 판단에 사용할 주변 영역 크기
// false       : useHarrisDetector. false면 Shi-Tomasi 방식 사용
// 0           : Harris 방식에서 쓰는 k값. 여기서는 false이므로 영향 없음
Point2f[] corners = Cv2.GoodFeaturesToTrack(gray, 100, 0.03, 5, null, 3, false, 0);

// CornerSubPix 함수는 정수 픽셀 단위로 찾은 코너 위치를 더 정밀하게 보정한다.
// GoodFeaturesToTrack 결과는 대략적인 픽셀 위치이지만, 실제 코너는 픽셀과 픽셀 사이의
// 소수점 좌표에 있을 수 있다. CornerSubPix는 주변 밝기 패턴을 반복 분석해
// 서브픽셀 단위의 더 정확한 좌표를 계산한다.
//
// 매개변수:
// gray              : 입력 흑백 이미지
// corners           : 보정할 초기 코너 좌표
// new Size(3, 3)    : 검색 윈도우 크기. 코너 주변 3x3 영역을 보며 보정
// new Size(-1, -1)  : zeroZone. 사용하지 않을 중앙 영역 없음
// TermCriteria.Both : 반복 종료 조건. 횟수 또는 오차 기준 중 하나를 만족하면 종료
// (10, 0.03)        : 최대 10번 반복하거나 위치 변화가 0.03보다 작아지면 종료
Point2f[] sub_corners = Cv2.CornerSubPix(gray, corners, new Size(3, 3), new Size(-1, -1), TermCriteria.Both(10, 0.03));

// Circle 함수는 이미지 위에 원을 그린다.
// 먼저 GoodFeaturesToTrack으로 찾은 원래 코너 후보를 노란색 원으로 표시한다.
// Point2f는 소수점 좌표를 가지지만, 화면에 그릴 때는 정수 Point로 변환한다.
for (int i = 0; i < corners.Length; i++)
{
    Point pt = new Point((int)corners[i].X, (int)corners[i].Y);
    Cv2.Circle(dst, pt, 5, Scalar.Yellow, Cv2.FILLED);
}

// CornerSubPix로 보정한 코너 위치를 빨간색 원으로 표시한다.
// 노란색 원과 빨간색 원이 조금 어긋나 보이면 서브픽셀 보정으로
// 좌표가 더 정밀한 위치로 이동했다는 뜻이다.
for (int i = 0; i < sub_corners.Length; i++)
{
    Point pt = new Point((int)sub_corners[i].X, (int)sub_corners[i].Y);
    Cv2.Circle(dst, pt, 5, Scalar.Red, Cv2.FILLED);
}

// ImShow 함수는 지정한 이름의 창에 이미지를 표시한다.
// WaitKey(0)는 키 입력이 있을 때까지 창을 유지하고,
// DestroyAllWindows는 열린 OpenCV 창을 모두 닫는다.
Cv2.ImShow("dst", dst);
Cv2.WaitKey(0);
Cv2.DestroyAllWindows();
