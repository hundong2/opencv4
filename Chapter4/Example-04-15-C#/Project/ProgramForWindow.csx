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
int value = 0;

Cv2.NamedWindow("Palette");
Cv2.CreateTrackbar("Color", "Palette", ref value, 255);

while (true)
{
    int pixel = Cv2.GetTrackbarPos("Color", "Palette");
    Mat src = new Mat(new Size(500, 500), MatType.CV_8UC3, new Scalar(pixel, value, value));

    Cv2.ImShow("Palette", src);
    if (Cv2.WaitKey(33) == 'q') break;
}

Cv2.DestroyAllWindows();
