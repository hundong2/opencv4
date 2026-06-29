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

Mat src1 = Cv2.ImRead(Path.Combine(scriptDir, "bin/Debug/gerbera.jpg"));
Mat dst = new Mat(src1.Size(), MatType.CV_8UC3);

Cv2.Compare(src1, new Scalar(200, 127, 100), dst, CmpTypes.GT);
//Blue,Green, Red의 요솟값이 200,127,100보다 큰 경우 원본 요솟값을 유지하고 아니면 모두 0으로 변경 
//색상이 극단적으로 변한것을 확인 할 수 있다. (색상은 BGR 순서임에 주의)
Cv2.ImShow("dst", dst);
Cv2.WaitKey(0);
Cv2.DestroyAllWindows();