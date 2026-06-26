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
int cameraIndex = -1;
Console.WriteLine("카메라 탐색 중...");

for (int i = 0; i <= 5; i++)
{
    using var testCap = new VideoCapture(i, VideoCaptureAPIs.MSMF);
    if (testCap.IsOpened())
    {
        Console.WriteLine($"  → 카메라 발견: 인덱스 {i}");
        if (cameraIndex == -1) cameraIndex = i;  // 첫 번째 카메라 사용
    }
}

if (cameraIndex == -1)
{
    Console.WriteLine("사용 가능한 카메라가 없습니다.");
    return;
}
VideoCapture capture = new VideoCapture(cameraIndex, VideoCaptureAPIs.MSMF);
Mat frame = new Mat();
capture.Set(VideoCaptureProperties.FrameWidth, 640);
capture.Set(VideoCaptureProperties.FrameHeight, 480);

while(true)
{
    if (capture.IsOpened() == true)
    {
        var ret = capture.Read(frame);
        if( ret && !frame.Empty())
        {
            Cv2.ImShow("VideoFrame", frame);
        }
        else
        {
            Console.WriteLine("프레임 읽기 실패");
            break;
        }
        if (Cv2.WaitKey(33) == 'q') break;
    }
}

capture.Release();
Cv2.DestroyAllWindows();