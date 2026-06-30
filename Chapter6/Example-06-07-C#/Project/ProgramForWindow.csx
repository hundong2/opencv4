// NuGet에서 OpenCvSharp4 라이브러리 참조 (버전: 4.13.0.20260602)
// #r "nuget: OpenCvSharp4, 4.13.0.20260602"
// // Linux x64 환경용 OpenCV 네이티브 런타임 바이너리 참조
// #r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"

#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.Windows, 4.13.0.20260602" // ✅ Windows용

using System;
using OpenCvSharp;
using System.Runtime.CompilerServices;  // CallerFilePath 어트리뷰트 사용에 필요
using System.IO;

string ScriptDir([CallerFilePath] string path = "") 
    => Path.GetDirectoryName(path)!;

// 현재 스크립트 파일이 위치한 디렉터리 경로
string scriptDir = ScriptDir();
Mat src = Cv2.ImRead(Path.Combine(scriptDir, "bin/Debug/clouds.jpg"));
Mat dst = new Mat();

List<Point2f> src_pts = new List<Point2f>()
{
    new Point2f(0.0f, 0.0f),
    new Point2f(0.0f, src.Height),
    new Point2f(src.Width, src.Height)
};

List<Point2f> dst_pts = new List<Point2f>()
{
    new Point2f(300.0f, 300.0f),
    new Point2f(300.0f, src.Height),
    new Point2f(src.Width-400.0f, src.Height-200.0f)
};

Mat M = Cv2.GetAffineTransform(src_pts, dst_pts);

Cv2.WarpAffine(
    src, dst, M, new Size(src.Width, src.Height),
    borderValue: new Scalar(127, 127, 127, 0)
);

Cv2.ImShow("dst", dst);
Cv2.WaitKey(0);
Cv2.DestroyAllWindows();