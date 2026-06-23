#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"
using System;   
using OpenCvSharp;

Point Pt1 = new Point(1, 2);
Point Pt2 = new Point(3, 2);

Console.WriteLine(Pt1.DistanceTo(Pt2));//거리
Console.WriteLine(Pt1.DotProduct(Pt2));//내적
Console.WriteLine(Pt1.CrossProduct(Pt2));//외적
Console.WriteLine(Pt1 + Pt2);//합
Console.WriteLine(Pt1 - Pt2);//차
Console.WriteLine(Pt1 == Pt2);//동등 비교
Console.WriteLine(Pt1 * 0.5);//스칼라 곱

// Output ----
// 2
// 7
// -4
// Point { X = 4, Y = 4 }
// Point { X = -2, Y = 0 }
// False
// Point { X = 0, Y = 1 }