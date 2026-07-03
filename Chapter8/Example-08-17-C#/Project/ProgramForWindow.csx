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
string modelPath = Path.Combine(scriptDir, "temp/yolov7.onnx");
string classesPath = Path.Combine(scriptDir, "temp/coco.names");
string imagePath = Path.Combine(scriptDir, "bin/Debug/umbrella.jpg");

if (!File.Exists(modelPath))
    throw new FileNotFoundException($"YOLOv7 모델 파일을 찾을 수 없습니다: {modelPath}");
if (!File.Exists(classesPath))
    throw new FileNotFoundException($"COCO 클래스 파일을 찾을 수 없습니다: {classesPath}");
if (!File.Exists(imagePath))
    throw new FileNotFoundException($"입력 이미지를 찾을 수 없습니다: {imagePath}");

string[] classNames = File.ReadAllLines(classesPath);

List<string> labels = new List<string>();
List<float> scores = new List<float>();
List<Rect> bboxes = new List<Rect>();

Mat image = Cv2.ImRead(imagePath);
Net net = CvDnn.ReadNetFromOnnx(modelPath);

// YOLOv7 입력 전처리: 640x640, [0,1] 스케일링, 채널 순서 BGR->RGB 변환(swapRB=true)
const int inputWidth = 640;
const int inputHeight = 640;
Mat inputBlob = CvDnn.BlobFromImage(
    image,
    scalefactor: 1 / 255.0,
    size: new Size(inputWidth, inputHeight),
    mean: Scalar.All(0),
    swapRB: true,
    crop: false);

net.SetInput(inputBlob);

// 대부분의 YOLOv7 ONNX는 단일 출력(예: [1, 25200, 85])을 가진다.
Mat output = net.Forward();
Mat detections = output;

if (output.Dims == 3)
{
    // [1, N, C] -> [N, C] 형태로 변환
    detections = output.Reshape(1, output.Size(1));
}

// 임계값
const float confThreshold = 0.25f;
const float scoreThreshold = 0.25f;
const float nmsThreshold = 0.45f;

for (int r = 0; r < detections.Rows; r++)
{
    float objectness = detections.At<float>(r, 4);
    if (objectness < confThreshold)
        continue;

    // 클래스 점수(5번째 이후) 중 최대값 탐색
    Cv2.MinMaxLoc(
        detections.Row(r).ColRange(5, detections.Cols),
        out _,
        out double maxClassScore,
        out _,
        out Point maxClassLoc);

    float classScore = (float)maxClassScore;
    float confidence = objectness * classScore;
    if (classScore < scoreThreshold || confidence < confThreshold)
        continue;

    int classId = maxClassLoc.X;

    // 출력 좌표는 입력 해상도(640x640) 기준이므로 원본 해상도로 스케일링
    float cx = detections.At<float>(r, 0);
    float cy = detections.At<float>(r, 1);
    float w = detections.At<float>(r, 2);
    float h = detections.At<float>(r, 3);

    int left = (int)((cx - w / 2f) * image.Width / inputWidth);
    int top = (int)((cy - h / 2f) * image.Height / inputHeight);
    int boxWidth = (int)(w * image.Width / inputWidth);
    int boxHeight = (int)(h * image.Height / inputHeight);

    // 박스가 화면을 벗어나지 않도록 보정
    left = Math.Max(0, left);
    top = Math.Max(0, top);
    boxWidth = Math.Min(boxWidth, image.Width - left);
    boxHeight = Math.Min(boxHeight, image.Height - top);

    if (boxWidth <= 0 || boxHeight <= 0)
        continue;

    string className = classId < classNames.Length ? classNames[classId] : $"class_{classId}";
    labels.Add(className);
    scores.Add(confidence);
    bboxes.Add(new Rect(left, top, boxWidth, boxHeight));
}

CvDnn.NMSBoxes(bboxes, scores, confThreshold, nmsThreshold, out int[] indices);

foreach (int i in indices)
{
    Cv2.Rectangle(image, bboxes[i], Scalar.Red, 2);
    Cv2.PutText(
        image,
        $"{labels[i]} {scores[i]:P1}",
        new Point(bboxes[i].X, Math.Max(20, bboxes[i].Y - 5)),
        HersheyFonts.HersheySimplex,
        0.6,
        Scalar.Yellow,
        2);
}

Cv2.ImShow("image", image);
Cv2.WaitKey();
Cv2.DestroyAllWindows();