#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"
using System;   
using OpenCvSharp;

// Mat.Eye(): 단위 행렬(Identity Matrix) 생성
// 대각선(0,0), (1,1), (2,2)에만 1을 가지고 나머지는 0
// Size(3, 3): 3x3 크기
// CV_64FC3: 64-bit float(double), 3채널(3개의 double 값)
Mat m = Mat.Eye(new Size(3, 3), MatType.CV_64FC3);

// At<T>(y, x): (y, x) 위치의 픽셀을 T 타입으로 접근
// double 타입으로 접근 -> 첫 번째 채널 값만 반환 (1.0)
Console.WriteLine(m.At<double>(0, 0));

// Vec3d 타입으로 접근 -> 3개 채널을 벡터로 반환
// (0,0) 위치: (1.0, 0, 0) -> Item0(첫 채널) = 1.0
Console.WriteLine(m.At<Vec3d>(0, 0).Item0);

// (1,1) 위치: (1.0, 0, 0) -> Item1(두 번째 채널) = 0
// Mat.Eye()는 대각선 요소마다 같은 값 (1,0,0)을 모두 저장 (대각선별로 다르지 않음)
Console.WriteLine(m.At<Vec3d>(1, 1).Item1);

// (2,2) 위치: (1.0, 0, 0) -> Item2(세 번째 채널) = 0
// 따라서 모든 대각선에서 첫 채널만 1, 나머지는 0
Console.WriteLine(m.At<Vec3d>(2, 2).Item2);

// Point3d 타입으로 접근 -> 3개 채널을 Point3d(X, Y, Z)로 반환  
// (2,2) 위치: X=1.0, Y=0, Z=0 (Vec3d의 Item0→X, Item1→Y, Item2→Z)
Console.WriteLine(m.At<Point3d>(2, 2));

// long 타입으로 접근 -> double을 long으로 재해석(메모리 비트 그대로)
// 1.0(double)의 비트 패턴을 long으로 읽음 (의도된 사용 아님, 메모리 구조 확인용)
Console.WriteLine(m.At<long>(2, 2));

// Output ----
// 1
// 1
// 0
// 0
// Point3d { X = 1, Y = 0, Z = 0 }
// 4607182418800017408