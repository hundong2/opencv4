#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"
using System;   
using OpenCvSharp;

var vector1 = new Vec4d(1.0, 2.0, 3.0, 4.0);
var vector2 = new Vec4d(1.0, 2.0, 3.0, 4.0);

Console.WriteLine(vector1.Item0);
Console.WriteLine(vector1[1]);
Console.WriteLine(vector1.Equals(vector2));

// Output ----
// dotnet script Program.csx 
// 1
// 2
// True