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
Cv2.AdaptiveThreshold(gray, binary, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 25, 5);
//AdaptiveThresholdTypes.MeanC : blocksize영역의 모든 픽셀에 평균 가중치를 적용
//AdaptiveThresholdTypes.GaussianC : blocksize영역의 모든 픽셀에 가우시안 가중치를 적용
Cv2.ImShow("binary", binary);
Cv2.WaitKey(0);
Cv2.DestroyAllWindows();