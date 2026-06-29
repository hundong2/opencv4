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
Mat one = new Mat(Path.Combine(scriptDir, "bin/Debug/one.jpg"));
Mat two = new Mat(Path.Combine(scriptDir, "bin/Debug/two.jpg"));
Mat three = new Mat(Path.Combine(scriptDir, "bin/Debug/three.jpg"));
Mat four = new Mat(Path.Combine(scriptDir, "bin/Debug/four.jpg"));

Mat left = new Mat();
Mat right = new Mat();
Mat dst = new Mat();

Cv2.VConcat(new Mat[] { one, three }, left); //수직
Cv2.VConcat(new Mat[] { two, four }, right); //수직
Cv2.HConcat(new Mat[] { left, right }, dst); //수평

Cv2.ImShow("dst", dst); 
Cv2.WaitKey();
Cv2.DestroyAllWindows();