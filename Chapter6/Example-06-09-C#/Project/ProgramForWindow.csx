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
Mat src = Cv2.ImRead(Path.Combine(scriptDir, "bin/Debug/dandelion.jpg"), ImreadModes.Grayscale);
Mat dst = new Mat();

//GetStructuringElement() 함수로 커널 생성
Mat kernel= Cv2.GetStructuringElement(MorphShapes.Cross, new Size(7, 7));
//팽창 함수 
Cv2.Dilate(src, dst, kernel, new Point(-1, -1), 3, BorderTypes.Reflect101, new Scalar(0));
//Point(-1, -1) : 커널의 중심 좌표, 3 : 반복 횟수, BorderTypes.Reflect101 : 경계 처리 방식, new Scalar(0) : 경계 처리 시 채워질 값
Cv2.ImShow("dst", dst);
Cv2.WaitKey(0);
Cv2.DestroyAllWindows();