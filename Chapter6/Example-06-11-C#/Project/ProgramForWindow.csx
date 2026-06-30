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

// dandelion.jpg 이미지를 그레이스케일(1채널)로 읽어온다.
// 모폴로지 연산은 보통 흑백/이진 이미지에서 형태를 분석할 때 많이 사용한다.
Mat src = Cv2.ImRead(Path.Combine(scriptDir, "bin/Debug/dandelion.jpg"), ImreadModes.Grayscale);

// MorphologyEx 결과가 저장될 출력 이미지
Mat dst = new Mat();

// 7x7 크기의 단일 채널 커널을 0으로 초기화한다.
// HitMiss 연산에서 커널은 "찾고 싶은 픽셀 패턴"을 표현하는 구조 요소로 사용된다.
Mat kernel = Mat.Zeros(new Size(7, 7), MatType.CV_8UC1);

// 첫 번째 열 전체를 1로 채운다.
// OpenCvSharp의 Mat 범위 인덱서는 [rowStart, rowEnd, colStart, colEnd] 형태이며,
// rowEnd와 colEnd는 포함되지 않는다.
// 즉, [0, 7, 0, 1]은 0~6행, 0열 영역을 의미한다.
kernel[0, 7, 0, 1] = Mat.Ones(new Size(1, 7), MatType.CV_8UC1);

// 첫 번째 행 전체를 1로 채운다.
// [0, 1, 0, 7]은 0행, 0~6열 영역을 의미한다.
// 위의 첫 번째 열과 합쳐져 좌상단을 기준으로 한 ㄱ/L 모양의 구조 요소가 된다.
kernel[0, 1, 0, 7] = Mat.Ones(new Size(7, 1), MatType.CV_8UC1);

// MorphologyEx는 침식, 팽창, 열림, 닫힘, HitMiss 같은 확장 모폴로지 연산을 수행한다.
// MorphTypes.HitMiss는 커널 모양과 일치하는 픽셀 패턴을 찾아내는 연산이다.
// iterations: 10은 같은 HitMiss 연산을 10번 반복 적용한다는 의미이다.
//
// 주의:
// HitMiss는 일반적으로 0과 255로 구성된 이진 이미지에서 의미가 가장 명확하다.
// src가 단순 그레이스케일 이미지라면 Threshold 등으로 이진화한 뒤 적용하는 편이 결과 해석에 좋다.
Cv2.MorphologyEx(src, dst, MorphTypes.HitMiss, kernel, iterations: 10);

// 결과 이미지 표시
Cv2.ImShow("dst", dst);
Cv2.WaitKey(0);
Cv2.DestroyAllWindows();
