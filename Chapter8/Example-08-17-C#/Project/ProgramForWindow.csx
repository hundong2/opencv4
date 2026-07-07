// NuGet에서 OpenCvSharp4 라이브러리 참조 (버전: 4.13.0.20260602)
// #r "nuget: OpenCvSharp4, 4.13.0.20260602"
// // Linux x64 환경용 OpenCV 네이티브 런타임 바이너리 참조
// #r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"

#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.Windows, 4.13.0.20260602" // Windows용 런타임

// C# 스크립트(.csx)에서는 #r 지시어로 NuGet 패키지를 즉시 참조할 수 있다.
using System;
using System.Collections.Generic;
using OpenCvSharp;
using System.Runtime.CompilerServices;  // CallerFilePath 특성을 사용하기 위해 필요
using System.IO;
using OpenCvSharp.Dnn;

// ---------------------------------------------------------------------
// 이 파일을 처음 보는 분을 위한 빠른 용어 정리
// - Mat: OpenCV의 이미지/행렬 자료형
// - Net: DNN(딥러닝) 모델 객체
// - Rect: 사각형 박스 (x, y, width, height)
// - Blob: DNN 입력 형식으로 변환된 텐서(보통 NCHW)
// - out 키워드: 함수가 결과를 "반환값 외에" 추가로 돌려줄 때 사용
// ---------------------------------------------------------------------

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
// [CallerFilePath]를 사용하면 이 메서드를 호출한 소스 파일의 전체 경로가 자동 전달된다.
static string ScriptDir([CallerFilePath] string path = "")
    => Path.GetDirectoryName(path)!;
// 위의 => 문법은 "표현식 본문 메서드"다.
// 즉 { return Path.GetDirectoryName(path)!; } 와 같은 의미다.
// ! (null-forgiving)는 "null이 아님을 개발자가 알고 있다"는 표시다.

// 현재 스크립트 파일이 위치한 디렉터리 경로(Project 폴더)
static readonly string scriptDir = ScriptDir();
// Path.Combine은 OS에 맞는 경로 구분자를 자동으로 처리한다.
string cfgFile = Path.Combine(scriptDir, "temp/yolov3.cfg");
string darknetModel = Path.Combine(scriptDir, "temp/yolov3.weights");
// coco.names를 한 줄씩 읽어 클래스 인덱스(0~79)를 클래스 이름 문자열로 매핑한다.
string[] classNames = File.ReadAllLines(Path.Combine(scriptDir, "temp/coco.names"));
// ReadAllLines 결과 타입은 string[] (문자열 배열)이며, classNames[0]처럼 인덱스로 접근한다.

// 탐지 결과를 누적할 리스트: 라벨/점수/바운딩 박스
List<string> labels = new List<string>();
List<float> scores = new List<float>();
List<Rect> bboxes = new List<Rect>();
// List<T>는 크기가 자동으로 늘어나는 컬렉션이다.
// Add(...)로 요소를 계속 추가한다.

Mat image = new Mat(Path.Combine(scriptDir, "bin/Debug/umbrella.jpg"));
// Darknet 형식(cfg + weights)으로 네트워크 로드
Net net = Net.ReadNetFromDarknet(cfgFile, darknetModel);
// BlobFromImage: 이미지 정규화 및 리사이즈로 DNN 입력 텐서(NCHW) 생성
Mat inputBlob = CvDnn.BlobFromImage(image, 1/255f, new Size(416, 416), crop:false);
// 1/255f 의 f는 float 리터럴 의미다. (double이 아니라 float)
// Size(416,416)은 모델이 기대하는 입력 해상도다.
// crop:false는 비율 유지 크롭 대신 리사이즈를 우선한다.

net.SetInput(inputBlob);
// YOLO의 출력은 여러 스케일의 출력 레이어로 구성되므로 모든 출력 레이어 이름을 가져온다.
var outBlobNames = net.GetUnconnectedOutLayersNames();
var outputBlobs = outBlobNames.Select(toMat => new Mat()).ToArray();
// var는 "컴파일러가 타입을 추론"한다는 뜻이다.
// 여기서 outBlobNames는 문자열 이름 목록, outputBlobs는 Mat 배열로 추론된다.
// Select(...).ToArray()는 이름 개수만큼 빈 Mat 객체를 만들어 배열로 만든다.

// Forward 수행 결과가 outputBlobs에 채워진다.
net.Forward(outputBlobs, outBlobNames);
foreach (Mat prob in outputBlobs)
{
    // foreach는 컬렉션의 요소를 처음부터 끝까지 순회하는 문법이다.
    // prob 하나는 특정 출력 레이어의 결과 행렬이다.
    for (int p = 0; p < prob.Rows; p++)
    {
        // for(초기값; 조건; 증감)은 인덱스 기반 반복문이다.
        // prob.Rows는 행 개수(=후보 박스 개수)다.

        // prob[p,4]: objectness score (해당 박스에 객체가 존재할 확률)
        float confidence = prob.At<float>(p, 4);
        // At<float>(row,col)은 해당 좌표의 값을 float로 읽는 함수다.
        if (confidence > 0.9)
        {
            // prob[p,5..]: 클래스별 점수 중 최대값의 인덱스(=예측 클래스 id)를 찾는다.
            Cv2.MinMaxLoc(prob.Row(p).ColRange(5, prob.Cols), out _, out _, out _, out Point classNumber);
            // MinMaxLoc의 out 인자 설명:
            // - out _ : 필요 없는 결과는 _ 로 버린다(최소값/최대값 좌표 등).
            // - out Point classNumber : 최대값 위치를 Point로 받는다.
            //   여기서는 X가 "가장 큰 클래스 점수의 인덱스" 역할을 한다.

            int classes = classNumber.X;
            // classes+5 위치에 해당 클래스의 점수가 있다.
            float probability = prob.At<float>(p, classes + 5);

            if (probability > 0.9)
            {
                // YOLO 출력 좌표는 (centerX, centerY, width, height)이며 0~1 정규화 값이다.
                float centerX = prob.At<float>(p, 0) * image.Width;
                float centerY = prob.At<float>(p, 1) * image.Height;
                float width = prob.At<float>(p, 2) * image.Width;
                float height = prob.At<float>(p, 3) * image.Height;

                labels.Add(classNames[classes]);
                scores.Add(probability);
                // 중심좌표 기반 박스를 OpenCV Rect(좌상단 x,y,width,height) 형식으로 변환
                bboxes.Add(new Rect((int)centerX - (int)width / 2, (int)centerY - (int)height / 2, (int)width, (int)height));
                // (int) 캐스팅은 실수(float)를 정수(int)로 바꾸는 문법이다.
            }
        }
    }
}

// NMS(Non-Maximum Suppression): 겹치는 박스 중 신뢰도 높은 박스만 남긴다.
CvDnn.NMSBoxes(bboxes, scores, 0.9f, 0.5f, out int[] indices);
// NMSBoxes 파라미터:
// 1) bboxes: 후보 박스 목록
// 2) scores: 후보 박스의 신뢰도 목록(인덱스가 bboxes와 반드시 일치해야 함)
// 3) 0.9f: score threshold (이 값 미만 박스는 버림)
// 4) 0.5f: NMS IoU threshold (겹침이 이 값보다 크면 중복으로 간주)
// 5) out indices: 최종 채택된 박스 인덱스 배열

foreach (int i in indices)
{
    // 최종 선택된 박스/라벨을 화면에 시각화
    Cv2.Rectangle(image, bboxes[i], Scalar.Red, 1);
    Cv2.PutText(image, labels[i], bboxes[i].Location, HersheyFonts.HersheyComplex, 1.0, Scalar.Red);
    // Rectangle: 이미지에 박스 그리기
    // PutText: 텍스트(클래스명) 쓰기
}

Cv2.ImShow("image", image);
Cv2.WaitKey();
Cv2.DestroyAllWindows();
// ImShow: 창에 이미지 표시
// WaitKey: 키 입력 대기(창이 바로 닫히지 않게 유지)
// DestroyAllWindows: 열린 OpenCV 창 정리