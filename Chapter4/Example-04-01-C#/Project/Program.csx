#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"

using System;
using OpenCvSharp;

Mat src = Cv2.ImRead("./OpenCV_Logo.png", ImreadModes.ReducedColor2);
Console.WriteLine(src);