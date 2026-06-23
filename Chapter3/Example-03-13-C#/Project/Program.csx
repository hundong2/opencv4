#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"
using System;   
using OpenCvSharp;

Mat M = new Mat();
M.Create(MatType.CV_8UC3, new int[] { 480, 640 });
// M.Create(new Size(640, 480), MatType.CV_8UC3);
// M.Create(480, 640, MatType.CV_8UC3);
M.SetTo(new Scalar(255, 0, 0));

// Output ----