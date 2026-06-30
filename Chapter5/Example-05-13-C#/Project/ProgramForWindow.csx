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
Mat src = Cv2.ImRead(Path.Combine(scriptDir, "bin/Debug/crescent.jpg"));
Mat dst = new Mat(src.Size(), MatType.CV_8UC3);

// ──────────────────────────────────────────────
// Cv2.GaussianBlur – 가우시안 블러
// ──────────────────────────────────────────────
// 시그니처:
//   void Cv2.GaussianBlur(Mat src, Mat dst, Size ksize,
//                         double sigmaX, double sigmaY = 0,
//                         BorderTypes borderType = BorderTypes.Reflect101)
//
// 원리:
//   단순 평균 블러(Box Filter)와 달리, 커널 중심에 가까울수록 높은 가중치를 부여하는
//   가우시안 분포 기반의 커널을 사용한다.
//
//   1D 가우시안 함수:
//     G(x) = exp(-x² / (2σ²))
//
//   2D 커널은 1D를 수평·수직으로 분리 적용(Separable Filter)하여 연산을 최적화한다:
//     K(x,y) = G(x) × G(y)
//
//   예) sigmaX=3, ksize=9×9 의 커널 가중치 분포 (중심이 가장 밝음):
//     ┌────────────────────────────────┐
//     │ 낮음  낮음  낮음  낮음  낮음  │
//     │ 낮음  중간  높음  중간  낮음  │
//     │ 낮음  높음 [최대] 높음  낮음  │  ← 중심(고정점)에 가중치 집중
//     │ 낮음  중간  높음  중간  낮음  │
//     │ 낮음  낮음  낮음  낮음  낮음  │
//     └────────────────────────────────┘
//
// 파라미터:
//   src        : 입력 이미지
//   dst        : 출력 이미지 (src와 동일한 크기·타입)
//   ksize      : 커널 크기. 홀수여야 함 (3,5,7,9,...). 클수록 더 많이 흐려짐
//   sigmaX     : X 방향 표준편차 (σ). 클수록 더 강한 블러. 0이면 ksize로 자동 계산
//   sigmaY     : Y 방향 표준편차. 0이면 sigmaX와 동일하게 설정
//   borderType : 이미지 경계 처리 방식 (아래 참고)
//
// BorderTypes 주요 값:
//   Reflect101 (기본값) : 경계 픽셀을 제외하고 반전 복사  ex) gfedcb|abcdefgh|gfedcba
//   Reflect             : 경계 픽셀 포함 반전 복사        ex) fedcba|abcdefgh|hgfedcb
//   Replicate           : 가장 가장자리 픽셀을 반복       ex) aaaaaa|abcdefgh|hhhhhhh
//   Wrap                : 반대쪽 픽셀로 채움 (타일링)     ex) cdefgh|abcdefgh|abcdefg
//   Isolated            : 커널이 이미지 경계를 벗어나면 0(검정)으로 처리
//                         → 경계 근처 픽셀이 어두워질 수 있음
//
// sigmaX 선택 가이드:
//   - 약한 블러 (노이즈 제거 수준) : sigma ≈ 1.0 ~ 1.5
//   - 중간 블러 (배경 부드럽게)   : sigma ≈ 2.0 ~ 3.0
//   - 강한 블러 (배경 흐리기)     : sigma ≈ 5.0 이상
//
// ksize와 sigma 관계:
//   ksize를 지정하지 않고 0으로 넘기면 sigma로부터 자동 계산:
//   ksize = 2 * round(3 * sigma) + 1  (약 ±3σ 범위)
Cv2.GaussianBlur(src, dst, new Size(9, 9), 3, 3, BorderTypes.Isolated);

Cv2.ImShow("dst", dst);
Cv2.WaitKey(0);
Cv2.DestroyAllWindows();