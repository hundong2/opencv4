#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"
using System;   
using OpenCvSharp;
using System.Runtime.InteropServices;

// SparseMat: 대부분이 0인 행렬에서 0이 아닌 값만 저장하는 희소 행렬
SparseMat sm = new SparseMat(new int[] { 1, 1 }, MatType.CV_32F);

// GetIndexer<T> / Ref<T> : SparseMat의 값을 배열처럼 읽고/쓰는 인덱서
SparseMat.Indexer<Vec3f> indexer = sm.GetIndexer<Vec3f>();
//SparseMat.Indexer<Vec3f> indexer = sm.Ref<Vec3f>();
// GetIndexer()는 인덱서를 반환하고 Ref()는 참조를 반환합니다.
// 일반적으로 GetIndexer()를 사용하는 것이 더 안전합니다.

// (0,0)에 3채널 실수 벡터 저장
indexer[0, 0] = new Vec3f(4, 5, 6);

// Get<T> : 지정한 위치의 값을 그대로 읽습니다.
Console.WriteLine(sm.Get<Vec3f>(0, 0).Item0);
Console.WriteLine(sm.Get<Vec3f>(0, 0).Item1);
Console.WriteLine(sm.Get<Vec3f>(0, 0).Item2);

// Set<T> : 지정한 위치에 값을 직접 저장합니다.
sm.Set<Vec3f>(0, 1, new Vec3f(7, 8, 9));
Console.WriteLine(sm.Get<Vec3f>(0, 1).Item0);

// Find<T> : 값이 존재하는 위치를 찾고, 없으면 null을 반환합니다.
// Value : Find 결과가 존재할 때 실제 저장된 값을 꺼낼 때 사용합니다.
var found = sm.Find<Vec3f>(0, 0);
Console.WriteLine(found.HasValue ? found.Value.Item0 : -1);

var missing = sm.Find<Vec3f>(5, 5);
Console.WriteLine(missing.HasValue ? missing.Value.Item0 : -1);

// Ptr(int i0, bool createMissing, long? hashVal = null)
// 1차원 SparseMat에서 해당 위치의 포인터를 반환합니다.
// createMissing=true 이면 값이 없을 때 새 요소를 만들고 포인터를 돌려줍니다.
SparseMat ptrMat = new SparseMat(new int[] { 8 }, MatType.CV_8UC1);
ptrMat.Ref<byte>()[3] = 42;

IntPtr ptr = ptrMat.Ptr(3, false);
Console.WriteLine(Marshal.ReadByte(ptr));


// Output ----
// 4
// 5
// 6
// 7
// 4
// -1
// 42