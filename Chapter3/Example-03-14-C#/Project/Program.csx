#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"
using System;   
using OpenCvSharp;

IList<int> sizes = new List<int>() { 480, 640 };
Mat m = new Mat(sizes, MatType.CV_8UC3);

// Output ----