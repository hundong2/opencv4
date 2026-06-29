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

// ──────────────────────────────────────────────
// 1. 이미지 로드
// ──────────────────────────────────────────────
// ImRead는 기본적으로 BGR(Blue-Green-Red) 채널 순서로 이미지를 로드한다.
// 일반적인 RGB와 채널 순서가 반대임에 주의.
Mat src = Cv2.ImRead(Path.Combine(scriptDir, "bin/Debug/tomato.jpg"));
Mat hsv = new Mat(src.Size(), MatType.CV_8UC3);
Mat dst = new Mat(src.Size(), MatType.CV_8UC3);

// ──────────────────────────────────────────────
// 2. BGR → HSV 색상 공간 변환
// ──────────────────────────────────────────────
// BGR로는 "오렌지색"을 검출하기 어렵다. 예를 들어 조명이 밝아지면 R·G·B 값이
// 동시에 변하여 임계값 설정이 복잡해진다.
//
// HSV 색상 공간은 색을 세 가지 축으로 분리한다:
//   H (Hue,      색상) : 0 ~ 179  → 색의 종류 (빨강=0, 초록=60, 파랑=120)
//   S (Saturation, 채도) : 0 ~ 255  → 색의 선명도 (0=무채색, 255=순수한 색)
//   V (Value,    명도) : 0 ~ 255  → 밝기 (0=검정, 255=최대 밝기)
//
// 핵심 장점: 조명이 변해도 H(색상) 값은 크게 바뀌지 않으므로
//           특정 색 검출 시 H 범위만 지정하면 된다.
//
// ※ OpenCV의 H 범위는 0°~360°를 절반으로 압축한 0~179 사용
//   (일반 색상환 각도를 2로 나눈 값)
Cv2.CvtColor(src, hsv, ColorConversionCodes.BGR2HSV);

// ──────────────────────────────────────────────
// 3. 채널 분리 (Split)
// ──────────────────────────────────────────────
// HSV 3채널 이미지를 H, S, V 세 개의 단일 채널 Mat으로 분리한다.
//   HSV[0] → H 채널 (색상)
//   HSV[1] → S 채널 (채도)
//   HSV[2] → V 채널 (명도)
//
// 여기서는 색상(H) 채널만으로 오렌지 영역을 검출할 것이므로 HSV[0]을 사용한다.
Mat[] HSV = Cv2.Split(hsv);

// ──────────────────────────────────────────────
// 4. InRange – 오렌지색 Hue 범위 마스크 생성
// ──────────────────────────────────────────────
// InRange(src, lowerBound, upperBound, dst):
//   픽셀값이 [lowerBound, upperBound] 범위 안에 있으면 255(흰색),
//   범위 밖이면 0(검정)으로 채워진 바이너리 마스크를 생성한다.
//
// 오렌지색의 Hue 범위: 약 8 ~ 20 (색상환 기준 16° ~ 40°)
//   빨강(0) ─ 오렌지(8~20) ─ 노랑(30) ─ 초록(60) ─ 파랑(120)
//
// 결과 H_orange: 오렌지 영역=255, 나머지=0 인 단일채널 마스크
Mat H_orange = new Mat(src.Size(), MatType.CV_8UC1);
Cv2.InRange(HSV[0], new Scalar(8), new Scalar(20), H_orange);

// ──────────────────────────────────────────────
// 5. BitwiseAnd – 마스크 적용
// ──────────────────────────────────────────────
// BitwiseAnd(src1, src2, dst, mask):
//   mask가 255(흰색)인 픽셀만 src1 & src2 비트 AND 연산 결과를 dst에 복사하고,
//   mask가 0인 픽셀은 dst를 0(검정)으로 채운다.
//
// 여기서 src1=src2=hsv 이므로 hsv AND hsv = hsv 그대로이며,
// 실질적으로 "오렌지 영역만 원본 HSV 값을 유지하고 나머지는 검정으로 지운다"는 의미다.
//
// 수식: dst[i] = (H_orange[i] == 255) ? hsv[i] : 0
Cv2.BitwiseAnd(hsv, hsv, dst, H_orange);

// ──────────────────────────────────────────────
// 6. HSV → BGR 역변환
// ──────────────────────────────────────────────
// ImShow는 BGR 포맷으로 이미지를 표시한다.
// 마스크가 적용된 HSV 결과를 다시 BGR로 변환해야 원래 색상으로 화면에 출력된다.
// 변환하지 않으면 H·S·V 값이 B·G·R로 잘못 해석되어 엉뚱한 색이 표시된다.
Cv2.CvtColor(dst, dst, ColorConversionCodes.HSV2BGR);

Cv2.ImShow("Orange", dst);
Cv2.WaitKey(0);
Cv2.DestroyAllWindows();