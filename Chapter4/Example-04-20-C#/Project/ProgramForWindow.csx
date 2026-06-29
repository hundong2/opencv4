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
VideoCapture capture = new VideoCapture(Path.Combine(scriptDir, "bin/Debug/Star.mp4"));
Mat frame = new Mat(new Size(capture.FrameWidth, capture.FrameHeight), MatType.CV_8UC3);
VideoWriter videoWriter = new VideoWriter();
bool isWrite = false;

while (true)
{
    if (capture.PosFrames == capture.FrameCount)
    {
        capture.Open(Path.Combine(scriptDir, "bin/Debug/Star.mp4"));
        if (!capture.IsOpened()) break;
    }

    bool ret = capture.Read(frame);
    if (!ret || frame.Empty()) continue;

    Cv2.ImShow("VideoFrame", frame);

    int key = Cv2.WaitKey(33);
    // Ctrl + D 또는 Alt + D
    // 운영체제 별로 다를 수 있습니다.
    if (key == 4)
    {
        videoWriter.Open(Path.Combine(scriptDir, "bin/Debug/Video.avi"), FourCC.XVID, 30, frame.Size(), true);
        isWrite = true;

    }
    // Ctrl + X 또는 Alt + X
    // 운영체제 별로 다를 수 있습니다.
    else if (key == 24)
    {
        videoWriter.Release();
        isWrite = false;
    }
    else if (key == 'q') break;

    if (isWrite == true) videoWriter.Write(frame);
}

videoWriter.Release();
capture.Release();
Cv2.DestroyAllWindows();