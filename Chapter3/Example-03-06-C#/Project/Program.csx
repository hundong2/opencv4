#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"
using System;   
using OpenCvSharp;

OpenCvSharp.Range range = new OpenCvSharp.Range(0, 100);
Console.WriteLine($"{range.Start}, {range.End}");
// Output ----
// 0, 100