#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"
using System;   
using OpenCvSharp;

Vec3d Vector = new Vec3d(1.0, 2.0, 3.0);
Point3d Pt1 = new Vec3d(1.0, 2.0, 3.0);
Point3d Pt2 = Vector;

Console.WriteLine(Pt1);
Console.WriteLine(Pt2);
Console.WriteLine(Pt1.X);

// Output ----
// Point3d { X = 1, Y = 2, Z = 3 }
// Point3d { X = 1, Y = 2, Z = 3 }
// 1