#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"
using System;   
using OpenCvSharp;
using System.Runtime.InteropServices;

SparseMat sm = new SparseMat(new int[] { 1, 1 }, MatType.CV_8UC3); //2차원 희소 행렬 
sm.Ref<Vec3b>()[99, 1000] = new Vec3b(100, 0, 0);
Console.WriteLine(sm.Find<Vec3b>(99, 1000).Value.Item0);
// Output ----
//100