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

Mat src = Cv2.ImRead(Path.Combine(scriptDir, "bin/Debug/swan.jpg"));
Mat gray = new Mat(src.Size(), MatType.CV_8UC1);
Mat binary = new Mat(src.Size(), MatType.CV_8UC1);

Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

// ──────────────────────────────────────────────
// Cv2.Threshold – 이진화(Thresholding)
// ──────────────────────────────────────────────
// 시그니처:
//   double Cv2.Threshold(Mat src, Mat dst, double thresh, double maxval, ThresholdTypes type)
//
// 파라미터:
//   src    : 입력 이미지 (단일 채널, 보통 그레이스케일)
//   dst    : 출력 이진 이미지
//   thresh : 기준 임계값 (0~255). Otsu/Triangle 방식 사용 시 자동 계산되므로 무시됨
//   maxval : 조건 충족 픽셀에 할당할 최대값 (보통 255 = 흰색)
//   type   : 이진화 방식 (ThresholdTypes 열거형)
//
// 반환값: 실제로 사용된 임계값 (Otsu·Triangle 방식이면 자동 계산값 반환)
//
// ── ThresholdTypes 종류 ──────────────────────────────────────
//
//  Binary        : 픽셀 > thresh  →  maxval,  그 외 → 0
//                  가장 기본적인 이진화. 밝은 영역만 흰색으로.
//                  dst = (src > thresh) ? maxval : 0
//
//  BinaryInv     : 픽셀 > thresh  →  0,       그 외 → maxval
//                  Binary의 반전. 어두운 영역을 흰색으로.
//                  dst = (src > thresh) ? 0 : maxval
//
//  Trunc         : 픽셀 > thresh  →  thresh,  그 외 → 픽셀값 유지
//                  임계값 이상은 임계값으로 잘라냄 (클리핑).
//                  dst = (src > thresh) ? thresh : src
//
//  Tozero        : 픽셀 > thresh  →  픽셀값 유지,  그 외 → 0
//                  임계값 이하는 0으로 지움.
//                  dst = (src > thresh) ? src : 0
//
//  TozeroInv     : 픽셀 > thresh  →  0,       그 외 → 픽셀값 유지
//                  Tozero의 반전. 임계값 이상은 0으로 지움.
//                  dst = (src > thresh) ? 0 : src
//
//  Otsu          : 오츠(Otsu) 알고리즘으로 임계값 자동 결정.
//                  히스토그램을 분석해 클래스 간 분산이 최대가 되는 임계값을 선택.
//                  thresh 파라미터는 무시되며 반환값으로 사용된 임계값을 알 수 있음.
//                  이중 봉우리(bimodal) 히스토그램에서 효과적.
//                  반드시 Binary 또는 BinaryInv 와 OR 조합하여 사용:
//                    ThresholdTypes.Binary | ThresholdTypes.Otsu
//
//  Triangle      : 삼각형(Triangle) 알고리즘으로 임계값 자동 결정.
//                  히스토그램의 최고점과 양 끝을 잇는 삼각형에서 가장 먼 지점을 임계값으로 선택.
//                  단일 봉우리(unimodal) 히스토그램에서 효과적.
//                  반드시 Binary 또는 BinaryInv 와 OR 조합하여 사용:
//                    ThresholdTypes.Binary | ThresholdTypes.Triangle
//
// ── 오츠 알고리즘 원리 ────────────────────────────────────────
//   전체 픽셀을 임계값 T를 기준으로 두 클래스(전경/배경)로 나눌 때
//   클래스 간 분산(between-class variance) σ²_B 를 최대화하는 T를 탐색:
//
//   σ²_B(T) = w₀(T) · w₁(T) · [μ₀(T) - μ₁(T)]²
//     w₀, w₁ : 각 클래스의 픽셀 비율
//     μ₀, μ₁ : 각 클래스의 평균 밝기
//
// ── 이진화 방식 선택 가이드 ──────────────────────────────────
//   조명이 균일하고 임계값 예측 가능  → Binary / BinaryInv (thresh 직접 지정)
//   조명이 불균일하고 임계값 모름     → Otsu (자동)
//   단일 봉우리 히스토그램            → Triangle (자동)
//   지역적 조명 변화가 심함           → AdaptiveThreshold 사용 권장
//
Cv2.Threshold(gray, binary, 127, 255, ThresholdTypes.Otsu);

Cv2.ImShow("binary", binary);
Cv2.WaitKey(0);
Cv2.DestroyAllWindows();