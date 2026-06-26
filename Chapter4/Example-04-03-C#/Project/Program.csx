#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"

using System;
using OpenCvSharp;
using System.Runtime.CompilerServices;
using System.IO;

string ScriptDir([CallerFilePath] string path = "") 
    => Path.GetDirectoryName(path)!;
string scriptDir = ScriptDir();
Mat src = Cv2.ImRead(Path.Combine(scriptDir, "OpenCV_Logo.png"), ImreadModes.ReducedColor2);

Cv2.NamedWindow("src", WindowFlags.GuiExpanded);
Cv2.SetWindowProperty("src", WindowPropertyFlags.Fullscreen, 0);
Cv2.ImShow("src", src);
Cv2.WaitKey(0);
Cv2.DestroyWindow("src");

//Mat [ 0*0*CV_8UC1, IsContinuous=False, IsSubmatrix=False, Ptr=0x564e6b57ea10, Data=0x0 ]