#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"
using System;   
using OpenCvSharp;

Size size = new Size(640, 480);
Mat img = new Mat(size, MatType.CV_8UC3);
Mat img2 = new Mat(img.Size(), MatType.CV_8UC3);

Console.WriteLine($"{size.Width}, {size.Height}");
Console.WriteLine(img.Size());
Console.WriteLine($"{img.Size().Width}, {img.Size().Height}");
Console.WriteLine($"{img.Width}, {img.Height}");
// Output ----
// 640, 480
// Size { Width = 640, Height = 480 }
// 640, 480
// 640, 480