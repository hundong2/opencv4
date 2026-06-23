#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"
using System;   
using OpenCvSharp;

Scalar s1 = new Scalar(255, 127);
Scalar s2 = Scalar.Yellow;
Scalar s3 = Scalar.All(99);

Console.WriteLine(s1);
Console.WriteLine(s2); //BGRA 순서 
Console.WriteLine(s3);
// Output ----
// Scalar { Val0 = 255, Val1 = 127, Val2 = 0, Val3 = 0 }
// Scalar { Val0 = 0, Val1 = 255, Val2 = 255, Val3 = 0 }
// Scalar { Val0 = 99, Val1 = 99, Val2 = 99, Val3 = 99 }