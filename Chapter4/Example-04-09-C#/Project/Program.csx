// NuGet에서 OpenCvSharp4 라이브러리 참조 (버전: 4.13.0.20260602)
#r "nuget: OpenCvSharp4, 4.13.0.20260602"
// Linux x64 환경용 OpenCV 네이티브 런타임 바이너리 참조
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"

using System;
using OpenCvSharp;
using System.Runtime.CompilerServices;  // CallerFilePath 어트리뷰트 사용에 필요
using System.IO;

VideoCapture capture = new VideoCapture(0);
Mat frame = new Mat();
capture.Set(VideoCaptureProperties.FrameWidth, 640);
capture.Set(VideoCaptureProperties.FrameHeight, 480);

while(true)
{
    if (capture.IsOpened() == true)
    {
        capture.Read(frame);
        Cv2.ImShow("VideoFrame", frame);
        if (Cv2.WaitKey(33) == 'q') break;
    }
}

capture.Release();
Cv2.DestroyAllWindows();