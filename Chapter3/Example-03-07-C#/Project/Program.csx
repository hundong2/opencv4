#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"
using System;   
using OpenCvSharp;

Rect rect1 = new Rect(new Point(0, 0), new Size(640, 480));
Rect rect2 = new Rect(100, 100, 640, 480);

Point pt1 = rect1.Location;
Point pt2 = rect2.Location;
var UnionRect = rect1.Union(rect2);
var IntersectRect = rect1.Intersect(rect2);

var UnionRect2 = rect1 | rect2;
Console.WriteLine($"rect1 | rect2 = {UnionRect2}");

var IntersectRect2 = rect1 & rect2;
Console.WriteLine($"rect1 & rect2 = {IntersectRect2}");

Console.WriteLine($"rect1 == rect2: {rect1 == rect2}");
Console.WriteLine($"rect1 != rect2: {rect1 != rect2}");

Console.WriteLine($"rect1 contains pt1: {rect1.Contains(pt1)}");
Console.WriteLine($"rect2 contains pt2: {rect2.Contains(pt2)}");
Console.WriteLine($"rect1: {rect1}");
Console.WriteLine($"rect2: {rect2}");
Console.WriteLine($"UnionRect: {UnionRect}");
Console.WriteLine($"IntersectRect: {IntersectRect}");
rect1.Inflate(new Size(100, 100)); //직사각형 구조체 팽창 
Console.WriteLine($"rect1 after inflation: {rect1}");
// // Output ----
// rect2: Rect { X = 100, Y = 100, Width = 640, Height = 480, Top = 100, Bottom = 580, Left = 100, Right = 740, Location = Point { X = 100, Y = 100 }, Size = Size { Width = 640, Height = 480 }, TopLeft = Point { X = 100, Y = 100 }, BottomRight = Point { X = 740, Y = 580 } }
// UnionRect: Rect { X = 0, Y = 0, Width = 740, Height = 580, Top = 0, Bottom = 580, Left = 0, Right = 740, Location = Point { X = 0, Y = 0 }, Size = Size { Width = 740, Height = 580 }, TopLeft = Point { X = 0, Y = 0 }, BottomRight = Point { X = 740, Y = 580 } }
// IntersectRect: Rect { X = 100, Y = 100, Width = 540, Height = 380, Top = 100, Bottom = 480, Left = 100, Right = 640, Location = Point { X = 100, Y = 100 }, Size = Size { Width = 540, Height = 380 }, TopLeft = Point { X = 100, Y = 100 }, BottomRight = Point { X = 640, Y = 480 } }
// rect1 after inflation: Rect { X = -100, Y = -100, Width = 840, Height = 680, Top = -100, Bottom = 580, Left = -100, Right = 740, Location = Point { X = -100, Y = -100 }, Size = Size { Width = 840, Height = 680 }, TopLeft = Point { X = -100, Y = -100 }, BottomRight = Point { X = 740, Y = 580 } }