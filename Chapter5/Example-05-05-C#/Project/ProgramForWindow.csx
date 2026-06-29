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
// 1. 이미지 로드 및 결과 Mat 초기화
// ──────────────────────────────────────────────
Mat src = Cv2.ImRead(Path.Combine(scriptDir, "bin/Debug/tomato.jpg"));
Mat hsv       = new Mat(src.Size(), MatType.CV_8UC3);
// lower_red : Hue 0~5  (빨강 색상환 시작 구간) 마스크를 담을 버퍼
// upper_red : Hue 170~179 (빨강 색상환 끝 구간) 마스크를 담을 버퍼
// added_red : 두 마스크를 합산한 최종 빨강 마스크
Mat lower_red = new Mat(src.Size(), MatType.CV_8UC3);
Mat upper_red = new Mat(src.Size(), MatType.CV_8UC3);
Mat added_red = new Mat(src.Size(), MatType.CV_8UC3);
Mat dst       = new Mat(src.Size(), MatType.CV_8UC3);

// ──────────────────────────────────────────────
// 2. BGR → HSV 변환
// ──────────────────────────────────────────────
// 빨강 검출 시 BGR을 직접 쓰면 R·G·B 세 채널 조건을 모두 설정해야 하고
// 조명 변화에도 매우 취약하다.
// HSV로 변환하면 H(색상) 범위만 지정해도 특정 색을 안정적으로 검출할 수 있다.
Cv2.CvtColor(src, hsv, ColorConversionCodes.BGR2HSV);

// ──────────────────────────────────────────────
// 3. InRange – 빨강 Hue 범위 마스크 생성 (두 구간)
// ──────────────────────────────────────────────
// 빨강이 색상환 양 끝(0° 근처, 360°=0° 근처)에 걸쳐 있기 때문에
// 하나의 InRange로 표현할 수 없어 두 구간으로 나눠 처리한다.
//
//  색상환(OpenCV H 0~179):
//  [빨강↓]  오렌지  노랑  초록  하늘  파랑  보라  [빨강↑]
//    0~5    8~20  22~38 45~75        100~130      170~179
//
// Scalar(H_min, S_min, V_min) ~ Scalar(H_max, S_max, V_max)
//   S ≥ 100 : 채도 조건 → 무채색(회색·흰색) 제거
//   V ≥ 100 : 명도 조건 → 너무 어두운 영역 제거
//
// 결과: 조건 만족 픽셀 = 255(흰색), 나머지 = 0(검정) 인 단일 채널 마스크
//       (CV_8UC1 형태로 내부 저장됨, Mat 타입이 CV_8UC3이어도 InRange는 8UC1로 출력)
Cv2.InRange(hsv, new Scalar(0,   100, 100), new Scalar(5,   255, 255), lower_red);
Cv2.InRange(hsv, new Scalar(170, 100, 100), new Scalar(179, 255, 255), upper_red);

// ──────────────────────────────────────────────
// 4. AddWeighted – 두 마스크 합산
// ──────────────────────────────────────────────
// AddWeighted(src1, alpha, src2, beta, gamma, dst):
//   dst = src1 * alpha + src2 * beta + gamma
//
//   alpha = 1.0, beta = 1.0, gamma = 0.0 이므로
//   added_red = lower_red * 1.0 + upper_red * 1.0 + 0.0
//             = lower_red + upper_red  (픽셀값 합산)
//
// lower_red와 upper_red는 각각 0 또는 255이며 두 영역은 겹치지 않으므로
// 합산 결과도 0 또는 255가 된다. (OR 마스크와 동일한 효과)
//
// ※ Cv2.BitwiseOr(lower_red, upper_red, added_red) 로도 동일하게 구현 가능
Cv2.AddWeighted(lower_red, 1.0, upper_red, 1.0, 0.0, added_red);

// ──────────────────────────────────────────────
// 5. BitwiseAnd – 마스크 적용으로 빨강 영역만 추출
// ──────────────────────────────────────────────
// BitwiseAnd(src1, src2, dst, mask):
//   mask[i] == 255 → dst[i] = src1[i] AND src2[i]
//   mask[i] == 0   → dst[i] = 0 (검정)
//
// src1 = src2 = hsv 이므로 hsv AND hsv = hsv (값 그대로)
// 결과적으로 added_red가 255인 픽셀(빨강 영역)만 HSV 값을 유지하고
// 나머지는 모두 0(검정)으로 채워진다.
Cv2.BitwiseAnd(hsv, hsv, dst, added_red);

// ──────────────────────────────────────────────
// 6. HSV → BGR 역변환 후 표시
// ──────────────────────────────────────────────
// ImShow는 BGR 포맷으로 이미지를 렌더링한다.
// HSV 값을 BGR로 변환하지 않으면 H·S·V가 B·G·R로 오해석되어 색이 전혀 다르게 표시된다.
Cv2.CvtColor(dst, dst, ColorConversionCodes.HSV2BGR);

Cv2.ImShow("dst", dst);
Cv2.WaitKey(0);
Cv2.DestroyAllWindows();