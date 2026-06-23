#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"

using System;
using OpenCvSharp;

// OpenCvSharp Mat 실무 Top 10 예제
// 각 섹션은 자주 쓰는 함수 1개(또는 1그룹)를 짧게 보여줍니다.

// ---------------------------------------------------------------------
// 1) At<T>, Set<T> : 특정 좌표의 픽셀 값을 읽고/쓰기
// ---------------------------------------------------------------------
var pixelMat = new Mat(2, 2, MatType.CV_8UC3, Scalar.All(0));
pixelMat.Set(0, 0, new Vec3b(10, 20, 30)); // (y=0, x=0)에 BGR 값 저장
var p00 = pixelMat.At<Vec3b>(0, 0);         // 같은 위치의 값을 읽기
Console.WriteLine($"1) At/Set      -> (0,0) BGR = ({p00.Item0}, {p00.Item1}, {p00.Item2})");

// ---------------------------------------------------------------------
// 2) Clone, CopyTo : 깊은 복사(원본과 독립), 대상 Mat으로 복사
// ---------------------------------------------------------------------
var original = new Mat(2, 2, MatType.CV_8UC1, Scalar.All(5));
var cloned = original.Clone();               // 깊은 복사
var copied = new Mat();
original.CopyTo(copied);                     // copied에 내용 복사
original.Set<byte>(0, 0, 99);                // 원본만 변경
Console.WriteLine($"2) Clone/CopyTo -> original={original.At<byte>(0,0)}, clone={cloned.At<byte>(0,0)}, copied={copied.At<byte>(0,0)}");

// ---------------------------------------------------------------------
// 3) ConvertTo : 타입 변환 + 스케일/시프트 적용
// ---------------------------------------------------------------------
var srcU8 = new Mat(1, 3, MatType.CV_8UC1);
srcU8.Set<byte>(0, 0, 10);
srcU8.Set<byte>(0, 1, 20);
srcU8.Set<byte>(0, 2, 30);
var dstF32 = new Mat();
srcU8.ConvertTo(dstF32, MatType.CV_32FC1, 1.0 / 255.0); // 0~255 -> 0~1 범위
Console.WriteLine($"3) ConvertTo    -> {dstF32.At<float>(0,0):F4}, {dstF32.At<float>(0,1):F4}, {dstF32.At<float>(0,2):F4}");

// ---------------------------------------------------------------------
// 4) Cv2.Add/Subtract/Multiply : 기본 사칙 연산
// ---------------------------------------------------------------------
var a = new Mat(1, 3, MatType.CV_32FC1, Scalar.All(2));
var b = new Mat(1, 3, MatType.CV_32FC1, Scalar.All(3));
var add = new Mat();
var sub = new Mat();
var mul = new Mat();
Cv2.Add(a, b, add);
Cv2.Subtract(a, b, sub);
Cv2.Multiply(a, b, mul);
Console.WriteLine($"4) Add/Sub/Mul  -> add={add.At<float>(0,0)}, sub={sub.At<float>(0,0)}, mul={mul.At<float>(0,0)}");

// ---------------------------------------------------------------------
// 5) Cv2.Resize : 영상 크기 변경
// ---------------------------------------------------------------------
var small = new Mat(2, 2, MatType.CV_8UC1, Scalar.All(100));
var large = new Mat();
Cv2.Resize(small, large, new Size(4, 4), interpolation: InterpolationFlags.Nearest);
Console.WriteLine($"5) Resize       -> {small.Rows}x{small.Cols} -> {large.Rows}x{large.Cols}");

// ---------------------------------------------------------------------
// 6) Cv2.Split, Cv2.Merge : 채널 분리/병합
// ---------------------------------------------------------------------
var color = new Mat(1, 1, MatType.CV_8UC3, new Scalar(10, 20, 30)); // B=10,G=20,R=30
Mat[] channels = Cv2.Split(color);
var merged = new Mat();
Cv2.Merge(channels, merged);
var mergedPix = merged.At<Vec3b>(0, 0);
Console.WriteLine($"6) Split/Merge  -> ch0={channels[0].At<byte>(0,0)}, ch1={channels[1].At<byte>(0,0)}, ch2={channels[2].At<byte>(0,0)}, merged=({mergedPix.Item0},{mergedPix.Item1},{mergedPix.Item2})");

// ---------------------------------------------------------------------
// 7) SubMat(ROI) : 관심 영역만 잘라서 처리
// ---------------------------------------------------------------------
var canvas = new Mat(4, 4, MatType.CV_8UC1, Scalar.All(0));
var roi = canvas.SubMat(1, 3, 1, 3); // [row 1..2], [col 1..2] 2x2 영역
roi.SetTo(Scalar.All(255));          // ROI만 흰색으로 변경
Console.WriteLine($"7) SubMat ROI    -> center={canvas.At<byte>(1,1)}, corner={canvas.At<byte>(0,0)}");

// ---------------------------------------------------------------------
// 8) IsContinuous, Step, ElemSize : 메모리 레이아웃 확인
// ---------------------------------------------------------------------
var mem = new Mat(3, 3, MatType.CV_8UC3);
Console.WriteLine($"8) Memory layout -> isContinuous={mem.IsContinuous()}, step={mem.Step()}, elemSize={mem.ElemSize()}");

// ---------------------------------------------------------------------
// 9) T(), Inv() : 전치/역행렬
// ---------------------------------------------------------------------
var m2 = new Mat(2, 2, MatType.CV_64FC1);
m2.Set<double>(0, 0, 4);
m2.Set<double>(0, 1, 7);
m2.Set<double>(1, 0, 2);
m2.Set<double>(1, 1, 6);
var mt = m2.T().ToMat();
var inv = m2.Inv().ToMat();
Console.WriteLine($"9) T/Inv         -> T(0,1)={mt.At<double>(0,1)}, Inv(0,0)={inv.At<double>(0,0):F4}");

// ---------------------------------------------------------------------
// 10) Cv2.Normalize : 값 범위 정규화 (min-max)
// ---------------------------------------------------------------------
var raw = new Mat(1, 3, MatType.CV_32FC1);
raw.Set<float>(0, 0, 10f);
raw.Set<float>(0, 1, 20f);
raw.Set<float>(0, 2, 30f);
var norm = new Mat();
Cv2.Normalize(raw, norm, 0, 1, NormTypes.MinMax);
Console.WriteLine($"10) Normalize    -> {norm.At<float>(0,0):F2}, {norm.At<float>(0,1):F2}, {norm.At<float>(0,2):F2}");
