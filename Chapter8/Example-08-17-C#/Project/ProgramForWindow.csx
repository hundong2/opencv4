// NuGet에서 OpenCvSharp4 라이브러리 참조 (버전: 4.13.0.20260602)
// #r "nuget: OpenCvSharp4, 4.13.0.20260602"
// // Linux x64 환경용 OpenCV 네이티브 런타임 바이너리 참조
// #r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"

#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.Windows, 4.13.0.20260602" // Windows용 런타임

using System;
using System.Collections.Generic;
using OpenCvSharp;
using System.Runtime.CompilerServices;  // CallerFilePath 특성을 사용하기 위해 필요
using System.IO;
using OpenCvSharp.Dnn;

// =====================================================================
// 예제 목표:
// 1) YOLOv7 ONNX 모델로 이미지 내 여러 객체를 동시에 탐지한다.
// 2) 탐지된 객체의 클래스명/신뢰도를 박스로 그린다.
//
// 참고:
// - YOLOv7은 "객체 탐지" 모델이므로 사람+우산이 함께 있으면 둘 다 검출 가능하다.
// - 사용 파일: temp/yolov7.onnx, temp/coco.names
//
// 파일 다운로드 위치:
// 1) yolov7.onnx
//    - YOLOv7 공식 저장소: https://github.com/WongKinYiu/yolov7
//    - ONNX export 예시(공식 저장소 export.py 사용):
//      python export.py --weights yolov7.pt --grid --end2end --simplify --topk-all 100 --iou-thres 0.45 --conf-thres 0.25 --img-size 640 640
//    - 생성된 ONNX 파일명을 yolov7.onnx로 두고 Project/temp 폴더에 배치
//
// 2) coco.names
//    - Darknet 공식 데이터 저장소: https://github.com/pjreddie/darknet/blob/master/data/coco.names
//    - 파일명을 coco.names로 저장해 Project/temp 폴더에 배치
static string ScriptDir([CallerFilePath] string path = "")
    => Path.GetDirectoryName(path)!;

// 현재 스크립트 파일이 위치한 디렉터리 경로(Project 폴더)
static readonly string scriptDir = ScriptDir();
string cfgFile = Path.Combine(scriptDir, "temp/yolov3.cfg");
string darknetModel = Path.Combine(scriptDir, "temp/yolov3.weights");
string[] classNames = File.ReadAllLines(Path.Combine(scriptDir, "temp/coco.names"));

List<string> labels = new List<string>();
List<float> scores = new List<float>();
List<Rect> bboxes = new List<Rect>();

Mat image = new Mat(Path.Combine(scriptDir, "bin/Debug/umbrella.jpg"));
Net net = Net.ReadNetFromDarknet(cfgFile, darknetModel);
Mat inputBlob = CvDnn.BlobFromImage(image, 1/255f, new Size(416, 416), crop:false);

net.SetInput(inputBlob);
var outBlobNames = net.GetUnconnectedOutLayersNames();
var outputBlobs = outBlobNames.Select(toMat => new Mat()).ToArray();

net.Forward(outputBlobs, outBlobNames);
foreach (Mat prob in outputBlobs)
{
    for (int p = 0; p < prob.Rows; p++)
    {
        float confidence = prob.At<float>(p, 4);
        if (confidence > 0.9)
        {
            Cv2.MinMaxLoc(prob.Row(p).ColRange(5, prob.Cols), out _, out _, out _, out Point classNumber);

            int classes = classNumber.X;
            float probability = prob.At<float>(p, classes + 5);

            if (probability > 0.9)
            {
                float centerX = prob.At<float>(p, 0) * image.Width;
                float centerY = prob.At<float>(p, 1) * image.Height;
                float width = prob.At<float>(p, 2) * image.Width;
                float height = prob.At<float>(p, 3) * image.Height;

                labels.Add(classNames[classes]);
                scores.Add(probability);
                bboxes.Add(new Rect((int)centerX - (int)width / 2, (int)centerY - (int)height / 2, (int)width, (int)height));
            }
        }
    }
}

CvDnn.NMSBoxes(bboxes, scores, 0.9f, 0.5f, out int[] indices);

foreach (int i in indices)
{
    Cv2.Rectangle(image, bboxes[i], Scalar.Red, 1);
    Cv2.PutText(image, labels[i], bboxes[i].Location, HersheyFonts.HersheyComplex, 1.0, Scalar.Red);
}

Cv2.ImShow("image", image);
Cv2.WaitKey();
Cv2.DestroyAllWindows();