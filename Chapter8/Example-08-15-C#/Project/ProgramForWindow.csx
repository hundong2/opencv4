// NuGet에서 OpenCvSharp4 라이브러리 참조 (버전: 4.13.0.20260602)
// #r "nuget: OpenCvSharp4, 4.13.0.20260602"
// // Linux x64 환경용 OpenCV 네이티브 런타임 바이너리 참조
// #r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"

#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.Windows, 4.13.0.20260602" // Windows용 런타임

using System;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using System.Runtime.CompilerServices;  // CallerFilePath 특성을 사용하기 위해 필요
using System.IO;
using OpenCvSharp.ML;
using OpenCvSharp.Dnn;

// =====================================================================
// 예제 목표:
// 1) GoogLeNet(ImageNet 1000-class) 분류 모델을 로드한다.
// 2) umbrella.jpg를 분류해 가장 확률이 높은 클래스 1개를 출력한다.
//
// 중요: 이 코드는 "객체 탐지"가 아니라 "이미지 분류" 예제다.
// 따라서 사람+우산처럼 여러 객체가 있어도 모델은 최종적으로 상위 1개 클래스를 선택한다.
static string ScriptDir([CallerFilePath] string path = "")
    => Path.GetDirectoryName(path)!;

// 현재 스크립트 파일이 위치한 디렉터리 경로(Project 폴더)
static readonly string scriptDir = ScriptDir();
// prototxt: 네트워크 구조 정의 파일
string prototxt = Path.Combine(scriptDir, "temp/bvlc_googlenet.prototxt");
// caffemodel: 학습된 가중치 파일
string caffeModel = Path.Combine(scriptDir, "temp/bvlc_googlenet.caffemodel");
// classNames: ImageNet 클래스 이름(인덱스 기반) 목록
string[] classNames = File.ReadAllLines(Path.Combine(scriptDir, "temp/bvlc_googlenet.txt"));

Mat image = Cv2.ImRead(Path.Combine(scriptDir, "bin/Debug/umbrella.jpg"));
Net net = Net.ReadNetFromCaffe(prototxt, caffeModel);
// BlobFromImage:
// - 이미지를 224x224로 리사이즈
// - 채널 평균값(B,G,R)=(104,117,123) 보정
// - 분류 모델 입력 텐서 형태로 변환
Mat inputBlob = CvDnn.BlobFromImage(image, 1, new Size(224, 224), new Scalar(104, 117, 123));

// 입력 텐서를 네트워크에 넣고, softmax 결과(prob)를 얻는다.
net.SetInput(inputBlob);
Mat outputBlobs = net.Forward("prob");

// MinMaxLoc으로 1000개 클래스 중 최대 확률의 인덱스를 찾는다.
Cv2.MinMaxLoc(outputBlobs, out _, out double classProb, out _, out Point classNumber);
// classNumber.X: 최고 확률 클래스 인덱스
// classProb    : 해당 클래스 확률
// 예) 사람도 함께 있더라도 umbrella 특징이 더 강하면 umbrella가 1위로 출력될 수 있다.
Console.WriteLine($"Class Number : {classNumber.X}");
Console.WriteLine($"Class Name : {classNames[classNumber.X]}");
Console.WriteLine($"Probability : {classProb:P2}");