#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"
using System;   
using OpenCvSharp;

RotatedRect rotatedRect = new RotatedRect(new Point2f(100f, 100f), new Size2f(100, 100), 45f);

Console.WriteLine(rotatedRect.BoundingRect());//회전 직사각형을 포함하는 직사각형
Console.WriteLine(rotatedRect.Points().Length);
Console.WriteLine(rotatedRect.Points()[0]);// Points => 회전된 직사각형을 포함하는 직사각형 
Console.WriteLine(rotatedRect.Center);
Console.WriteLine(rotatedRect.Size);
Console.WriteLine(rotatedRect.Angle);

// Output ----
// Rect { X = 29, Y = 29, Width = 143, Height = 143, Top = 29, Bottom = 172, Left = 29, Right = 172, Location = Point { X = 29, Y = 29 }, Size = Size { Width = 143, Height = 143 }, TopLeft = Point { X = 29, Y = 29 }, BottomRight = Point { X = 172, Y = 172 } }
// 4
// Point2f { X = 29.289322, Y = 100.00001 }
// Point2f { X = 100, Y = 100 }
// Size2f { Width = 100, Height = 100 }
// 45