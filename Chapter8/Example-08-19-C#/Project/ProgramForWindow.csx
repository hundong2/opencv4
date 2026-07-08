// NuGet에서 OpenCvSharp4 라이브러리 참조 (버전: 4.13.0.20260602)
// #r "nuget: OpenCvSharp4, 4.13.0.20260602"
// // Linux x64 환경용 OpenCV 네이티브 런타임 바이너리 참조
// #r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"

#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.Windows, 4.13.0.20260602" // Windows용 런타임

using System;
using OpenCvSharp;
using System.Runtime.CompilerServices;  // CallerFilePath 특성을 사용하기 위해 필요
using System.IO;
using OpenCvSharp.Dnn;

// ---------------------------------------------------------------------
// 이 파일을 처음 보는 분을 위한 빠른 용어 정리
// - Mat: OpenCV의 이미지/행렬 자료형
// - Net: DNN(딥러닝) 모델 객체
// - Blob: DNN 입력 형식으로 변환된 텐서(보통 NCHW)
// - out 키워드: 함수가 결과를 "반환값 외에" 추가로 돌려줄 때 사용
// ---------------------------------------------------------------------

// ---------------------------------------------------------------------
// 학습 가이드(기초 -> 중급 -> 고급)
// 1) 경로/파일 로딩: 모델과 라벨 파일을 안전하게 읽는 법
// 2) 전처리(BlobFromImage): 이미지 -> 모델 입력 텐서
// 3) 추론(Forward): 모델이 예측한 raw 텐서 받기
// 4) 후처리: confidence 필터링, 좌표 복원, 라벨 시각화
// 5) 확장: threshold 튜닝, 성능 최적화, 평가 지표 적용
// ---------------------------------------------------------------------

// =====================================================================
// 예제 목표:
// 1) TensorFlow SSD 모델(.pb + .pbtxt)로 이미지 내 객체를 탐지한다.
// 2) 탐지된 객체의 클래스명/신뢰도를 박스로 그린다.
//
// 파일 구성 의미:
// 1) frozen_inference_graph.pb
//    - 학습된 가중치가 포함된 TensorFlow 그래프(추론 모델 본체)
// 2) graph.pbtxt
//    - 그래프 구조를 텍스트로 정의한 설정 파일
// 3) labelmap.txt
//    - 클래스 id를 사람이 읽을 수 있는 이름으로 변환하는 매핑 파일

// [CallerFilePath]를 사용하면 이 메서드를 호출한 소스 파일의 전체 경로가 자동 전달된다.
static string ScriptDir([CallerFilePath] string path = "")
    => Path.GetDirectoryName(path)!;
// 위의 => 문법은 "표현식 본문 메서드"다.
// 즉 { return Path.GetDirectoryName(path)!; } 와 같은 의미다.
// ! (null-forgiving)는 "null이 아님을 개발자가 알고 있다"는 표시다.

// 현재 스크립트 파일이 위치한 디렉터리 경로(Project 폴더)
static readonly string scriptDir = ScriptDir();

// Path.Combine은 OS에 맞는 경로 구분자를 자동으로 처리한다.
string config = Path.Combine(scriptDir, "temp/graph.pbtxt");
string model = Path.Combine(scriptDir, "temp/frozen_inference_graph.pb");
string labelsPath = Path.Combine(scriptDir, "temp/labelmap.txt");
string imagePath = Path.Combine(scriptDir, "bin/Debug/umbrella.jpg");

// 파일 존재 여부를 먼저 확인하면 런타임 중간 에러보다 원인 파악이 쉽다.
if (!File.Exists(config))
    throw new FileNotFoundException($"TensorFlow config 파일을 찾을 수 없습니다: {config}");
if (!File.Exists(model))
    throw new FileNotFoundException($"TensorFlow model 파일을 찾을 수 없습니다: {model}");
if (!File.Exists(labelsPath))
    throw new FileNotFoundException($"라벨 매핑 파일을 찾을 수 없습니다: {labelsPath}");
if (!File.Exists(imagePath))
    throw new FileNotFoundException($"입력 이미지를 찾을 수 없습니다: {imagePath}");

// labelmap.txt를 한 줄씩 읽어 class id -> class name 매핑 배열로 사용한다.
string[] classNames = File.ReadAllLines(labelsPath);

// Mat 생성자에 경로를 넣으면 이미지를 메모리(Mat)로 로드한다.
Mat image = new Mat(imagePath);

// ReadNetFromTensorflow(model, config):
// - model(.pb): 가중치/그래프 데이터
// - config(.pbtxt): 그래프 구조/입출력 정보
// OpenCV DNN은 이 둘을 조합해 Net 객체를 만든다.
Net net = Net.ReadNetFromTensorflow(model, config);

// BlobFromImage(image, scalefactor, size, mean, swapRB, crop)
// - scalefactor=1: 픽셀 스케일링 없음(모델에 따라 1/255가 필요할 수 있음)
// - size=(300,300): SSD MobileNet 계열에서 자주 쓰는 입력 크기
// - mean 미지정: 기본 0
// - swapRB=true: BGR(OpenCV 기본) -> RGB(모델 기대 형식) 변환
// - crop=false: 강제 크롭 없이 리사이즈
Mat inputBlob = CvDnn.BlobFromImage(image, 1, new Size(300, 300), swapRB: true, crop: false);

// SetInput: 생성한 Blob를 네트워크 입력 버퍼에 설정
net.SetInput(inputBlob);

// Forward: 추론 실행 후 출력 텐서를 Mat로 반환
Mat outputBlobs = net.Forward();

// TensorFlow SSD 출력은 보통 [1, 1, N, 7] 형식이다.
// 7개 값 의미(일반적인 SSD DetectionOutput):
// [0]=image_id, [1]=class_id, [2]=confidence, [3]=x1, [4]=y1, [5]=x2, [6]=y2
// 아래 FromPixelData는 [N, 7] 2D 형태로 보기 쉽게 재해석한다.
// outputBlobs.Ptr(0)은 실제 데이터 포인터이며, 복사 없이 같은 메모리를 참조한다.
Mat prob = Mat.FromPixelData(outputBlobs.Size(2), outputBlobs.Size(3), MatType.CV_32F, outputBlobs.Ptr(0));

// for(초기값; 조건; 증감)은 인덱스 기반 반복문이다.
// 여기서는 후보 박스 N개를 순회한다.
for (int p = 0; p < prob.Rows; p++)
{
    // At<float>(row, col)은 해당 좌표의 값을 float로 읽는다.
    // confidence는 "이 탐지가 맞다"는 신뢰도(0~1)
    float confidence = prob.At<float>(p, 2);

    // threshold를 높이면 오검출은 줄고, 미검출은 늘 수 있다.
    // 학습 시작값: 0.4~0.6, 데모에서 엄격 필터: 0.8~0.9
    if (confidence > 0.9)
    {
        // class_id는 float로 들어오는 경우가 많아서 int로 캐스팅한다.
        int classes = (int)prob.At<float>(p, 1);

        // 라벨 범위를 벗어나는 예외 상황을 방지하는 안전 코드
        string label = (classes >= 0 && classes < classNames.Length)
            ? classNames[classes]
            : $"class_{classes}";

        // x1,y1,x2,y2는 0~1 정규화 좌표이므로 원본 이미지 크기로 복원한다.
        int x1 = (int)(prob.At<float>(p, 3) * image.Width);
        int y1 = (int)(prob.At<float>(p, 4) * image.Height);
        int x2 = (int)(prob.At<float>(p, 5) * image.Width);
        int y2 = (int)(prob.At<float>(p, 6) * image.Height);

        // Math.Clamp: 좌표를 유효 범위 안으로 고정(경계 밖 접근 방지)
        x1 = Math.Clamp(x1, 0, image.Width - 1);
        y1 = Math.Clamp(y1, 0, image.Height - 1);
        x2 = Math.Clamp(x2, 0, image.Width - 1);
        y2 = Math.Clamp(y2, 0, image.Height - 1);

        // 좌표가 뒤집힌 경우(드물지만 가능) 교정
        if (x2 < x1) (x1, x2) = (x2, x1);
        if (y2 < y1) (y1, y2) = (y2, y1);

        // Rectangle: 좌상단/우하단 좌표로 박스 시각화
        Cv2.Rectangle(image, new Point(x1, y1), new Point(x2, y2), new Scalar(0, 0, 255), 2);

        // PutText: 라벨 + 확률 표시
        // P1은 퍼센트 한 자리 포맷(예: 93.1%)
        string text = $"{label} {confidence:P1}";
        Cv2.PutText(
            image,
            text,
            new Point(x1, Math.Max(20, y1)),
            HersheyFonts.HersheyComplex,
            0.8,
            Scalar.Red,
            2);
    }
}

// ImShow/WaitKey/DestroyAllWindows는 OpenCV 기본 시각화 루틴
// - ImShow: 창 열기
// - WaitKey: 키 입력 대기(없으면 창이 즉시 닫힘)
// - DestroyAllWindows: 열린 창 정리
Cv2.ImShow("image", image);
Cv2.WaitKey();
Cv2.DestroyAllWindows();

// ---------------------------------------------------------------------
// 전문가 단계 확장 포인트
// 1) 임계값 동적화: 클래스별 confidence threshold 다르게 적용
// 2) 후처리 확장: SSD 출력에 대해 IoU 기반 NMS 추가 적용
// 3) 성능 최적화: net.SetPreferableBackend/SetPreferableTarget으로 CPU/GPU 선택
// 4) 배치 처리: 다중 이미지 루프에서 Blob 재사용으로 할당 비용 감소
// 5) 정량 평가: precision/recall, mAP 기준으로 threshold 체계적 튜닝
// ---------------------------------------------------------------------
