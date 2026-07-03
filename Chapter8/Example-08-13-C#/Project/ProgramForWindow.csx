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

// =====================================================================
// 예제 08-13: HOG 특징 + SVM을 이용한 Fashion-MNIST 분류
// 28x28 의류 이미지를 그대로 쓰지 않고, 모양/방향 정보를 담은 HOG 특징으로 변환한 뒤
// SVM 모델로 의류 클래스를 예측한다.
// =====================================================================

// ScriptDir 함수는 현재 실행 중인 .csx 파일의 경로를 기준으로
// 예제 이미지가 있는 폴더 위치를 구할 때 사용한다.
// [CallerFilePath] 특성은 컴파일 시점에 호출자의 파일 경로를 자동으로 삽입한다.
string ScriptDir([CallerFilePath] string path = "")
    => Path.GetDirectoryName(path)!;

// 현재 스크립트 파일이 위치한 디렉터리 경로(Project 폴더)
string scriptDir = ScriptDir();

// Fashion-MNIST 데이터는 Project 폴더 안이 아니라 예제 폴더 바로 아래의 temp 폴더에 있다.
// scriptDir의 상위 폴더를 구해서 Example-08-13-C# 폴더 기준 경로를 만든다.
string exampleDir = Directory.GetParent(scriptDir)!.FullName;
string dataDir = Path.Combine(exampleDir, "temp");

// Fashion-MNIST 라벨 번호를 사람이 읽을 수 있는 클래스 이름으로 매핑한다.
// 예: 모델이 7을 예측하면 "Sneaker" 클래스로 해석할 수 있다.
static Dictionary<int, string> label_dict = new Dictionary<int, string>()
{
    { 0, "T-shirt/top" },
    { 1, "Trouser" },
    { 2, "Pullover" },
    { 3, "Dress" },
    { 4, "Coat" },
    { 5, "Sandal" },
    { 6, "Shirt" },
    { 7, "Sneaker" },
    { 8, "Bag" },
    { 9, "Ankle boot" }
};

// Fashion-MNIST의 이미지/라벨 바이너리 파일을 읽어 학습 또는 테스트 데이터로 변환한다.
//
// image_path : 이미지 데이터 파일 경로
// label_path : 라벨 데이터 파일 경로
// length     : 읽을 이미지 개수
//
// 반환값:
// - float[] : 모든 이미지 픽셀 값. 이미지 1장은 28x28 = 784개의 값으로 저장된다.
// - int[]   : 각 이미지의 정답 라벨. Fashion-MNIST는 0~9 범주의 의류 클래스를 사용한다.
static Tuple<float[], int[]> loadTrainData(string image_path, string label_path, int length)
{
    using (FileStream image_data = new FileStream(image_path, FileMode.Open))
    using (FileStream label_data = new FileStream(label_path, FileMode.Open))
    using (BinaryReader image_binary = new BinaryReader(image_data))
    using (BinaryReader label_binary = new BinaryReader(label_data))
    {
        // IDX 파일의 앞부분에는 magic number, 개수, 행/열 크기 같은 헤더 정보가 있다.
        // 이 예제에서는 파일 구조를 이미 알고 있으므로 이미지 헤더 16바이트,
        // 라벨 헤더 8바이트를 건너뛴 뒤 실제 데이터만 읽는다.
        image_binary.ReadBytes(16);
        label_binary.ReadBytes(8);

        // image 배열은 length개의 이미지를 한 줄로 이어 붙인 형태이다.
        // di번째 이미지의 i번째 픽셀은 image[di * 784 + i]에 저장된다.
        float[] image = new float[length * 784];
        int[] label = new int[length];

        for (int di = 0; di < length; ++di)
        {
            for (int i = 0; i < 784; ++i)
            {
                // Fashion-MNIST 픽셀은 0~255 범위의 byte 값이다.
                // OpenCV ML 학습 입력은 float Mat을 사용하므로 float로 저장한다.
                byte img = image_binary.ReadByte();
                image[di * 784 + i] = (float)img;
            }

            // 라벨 파일에는 각 이미지의 정답 클래스 번호가 1바이트씩 저장되어 있다.
            byte lb = label_binary.ReadByte();
            label[di] = (int)lb;
        }
        return new Tuple<float[], int[]>(image, label);
    }
}

// HogCompute 함수는 여러 장의 28x28 이미지를 HOG 특징 벡터로 변환한다.
// HOG(Histogram of Oriented Gradients)는 픽셀 밝기 자체보다
// "어느 방향의 경계선이 얼마나 있는지"를 요약하는 특징이다.
// 옷의 윤곽, 소매 방향, 신발 가장자리 같은 모양 정보를 SVM이 다루기 쉬운 숫자로 바꾼다고 보면 된다.
static float[] HogCompute(float[] images)
{
    // HOGDescriptor는 HOG 특징을 계산하는 도구이다.
    //
    // new Size(28, 28) : 입력 이미지 한 장의 크기
    // new Size(8, 8)   : 블록 크기. 여러 셀을 묶어 정규화하는 영역
    // new Size(4, 4)   : 블록이 이동하는 간격
    // new Size(4, 4)   : 셀 크기. 방향 히스토그램을 계산하는 작은 영역
    // 9                : 방향을 9개 구간으로 나누어 히스토그램 생성
    // HistogramNormType.L2Hys : 밝기 차이에 덜 흔들리도록 히스토그램을 정규화하는 방식
    HOGDescriptor hog = new HOGDescriptor(new Size(28, 28), new Size(8, 8), new Size(4, 4), new Size(4, 4), 9, 1, -1, HistogramNormType.L2Hys, 0.2, true, 28);

    // descriptor에는 이미지 한 장마다 계산된 HOG 특징 배열이 차례대로 들어간다.
    List<float[]> descriptor = new List<float[]>();

    // images는 모든 이미지를 1차원 배열로 이어 붙인 형태이다.
    // 이미지 한 장은 784개 픽셀(28x28)이므로 images.Length / 784번 반복한다.
    for (int num = 0; num < images.Length / 784; num++)
    {
        // 전체 배열에서 현재 이미지 한 장에 해당하는 784개 픽셀만 잘라낸다.
        float[] image_array = new float[784];
        Array.Copy(images, 784 * num, image_array, 0, 784);

        // Mat.FromPixelData는 C# 배열을 OpenCV Mat 데이터로 감싼다.
        // 여기서는 784개 float 값을 28행 x 28열의 단일 채널 이미지로 해석한다.
        //
        // 참고: 최신 OpenCvSharp에서는 배열을 넘기는 new Mat(rows, cols, type, array) 생성자가
        // 직접 접근 불가일 수 있으므로 FromPixelData를 사용하는 편이 안전하다.
        Mat image = Mat.FromPixelData(28, 28, MatType.CV_32F, image_array);

        // HOGDescriptor.Compute는 보통 8비트 단일 채널 이미지를 입력으로 쓰므로
        // 0~255 float 픽셀값을 CV_8UC1 형식으로 변환한다.
        image.ConvertTo(image, MatType.CV_8UC1);

        // Compute 함수는 현재 이미지의 HOG 특징 벡터를 계산한다.
        descriptor.Add(hog.Compute(image));
    }

    // descriptor는 List<float[]> 형태라서 이미지별 특징이 나뉘어 있다.
    // SVM에 넣기 위해 모든 특징 배열을 하나의 긴 float[]로 평평하게 펼친다.
    List<float> flatten_descriptor = (from list in descriptor from item in list select item).ToList();
    return flatten_descriptor.ToArray();
}

// 학습 데이터 60,000장과 테스트 데이터 10,000장을 읽는다.
// temp 폴더에는 Fashion-MNIST의 IDX 원본 파일이 있어야 한다.
Tuple<float[], int[]> train = loadTrainData(Path.Combine(dataDir, "train-images-idx3-ubyte"), Path.Combine(dataDir, "train-labels-idx1-ubyte"), 60000);
Tuple<float[], int[]> test = loadTrainData(Path.Combine(dataDir, "t10k-images-idx3-ubyte"), Path.Combine(dataDir, "t10k-labels-idx1-ubyte"), 10000);

// 원본 픽셀 배열을 바로 SVM에 넣지 않고, 먼저 HOG 특징 배열로 변환한다.
// train_descriptor/test_descriptor는 이미지별 HOG 특징을 모두 이어 붙인 1차원 배열이다.
float[] train_descriptor = HogCompute(train.Item1);
float[] test_descriptor = HogCompute(test.Item1);

// FromPixelData 함수는 기존 C# 배열을 OpenCvSharp Mat으로 감싼다.
//
// Mat.FromPixelData와 new Mat의 차이:
// - new Mat(rows, cols, type)             : 비어 있는 새 Mat 메모리를 만든다.
// - Mat.FromPixelData(rows, cols, type, array) : 이미 존재하는 C# 배열 데이터를 Mat처럼 보게 한다.
// - 과거 예제의 new Mat(rows, cols, type, array)는 OpenCvSharp 버전에 따라 접근 불가일 수 있다.
//   배열로부터 Mat을 만들 때는 FromPixelData를 쓰는 방식이 현재 코드에서 더 명확하고 안전하다.
//
// train_x/test_x:
// - 행 하나가 이미지 한 장이다.
// - 열 개수는 이미지 한 장에서 나온 HOG 특징 개수이다.
// - SVM 학습 입력은 float 형식이어야 하므로 CV_32F를 사용한다.
//
// train_y/test_y:
// - 각 이미지의 정답 라벨이다.
// - 정수 클래스 번호를 저장하므로 CV_32S를 사용한다.
Mat train_x = Mat.FromPixelData(60000, train_descriptor.Length / 60000, MatType.CV_32F, train_descriptor);
Mat train_y = Mat.FromPixelData(1, 60000, MatType.CV_32S, train.Item2);
Mat test_x = Mat.FromPixelData(10000, test_descriptor.Length / 10000, MatType.CV_32F, test_descriptor);
Mat test_y = Mat.FromPixelData(1, 10000, MatType.CV_32S, test.Item2);

// SVM.Create 함수는 OpenCV ML의 SVM 분류기 객체를 만든다.
// 이번 예제에서는 HOG 특징을 입력으로 받아 의류 클래스를 분류한다.
SVM svm = SVM.Create();

// CSvc는 여러 클래스 분류에 사용하는 SVM 타입이다.
svm.Type = SVM.Types.CSvc;

// Rbf는 Radial Basis Function 커널이다.
// 직선으로 나누기 어려운 특징 공간에서도 부드러운 비선형 경계를 만들 수 있다.
svm.KernelType = SVM.KernelTypes.Rbf;

// Gamma는 RBF 커널에서 한 학습 샘플의 영향 범위를 조절한다.
// 값이 커질수록 가까운 샘플의 영향이 강해지고 경계가 더 복잡해질 수 있다.
svm.Gamma = 0.5;

// C는 오분류 허용 정도와 경계 여유 사이의 균형을 조절한다.
// 값이 클수록 학습 데이터를 더 엄격하게 맞추려는 성향이 강해진다.
svm.C = 0.5;

// Train 함수는 HOG 특징과 정답 라벨로 SVM 모델을 학습한다.
// RowSample은 train_x의 각 행이 하나의 이미지 샘플이라는 뜻이다.
svm.Train(train_x, SampleTypes.RowSample, train_y);

// 전체 테스트 데이터 중 앞쪽 count개만 평가한다.
int count = 500;

// results에는 각 테스트 이미지에 대해 SVM이 예측한 라벨이 저장된다.
Mat results = new Mat();

// Predict 함수는 학습된 SVM 모델로 입력 샘플의 클래스를 예측한다.
// test_x[0, count, 0, test_x.Width]는 0행부터 count행 전까지,
// 전체 HOG 특징 열을 선택한다는 뜻이다.
svm.Predict(test_x[0, count, 0, test_x.Width], results);

// Predict 결과는 float 형태로 나올 수 있으므로,
// 정답 라벨 Mat과 비교하기 쉽도록 32비트 정수로 변환한다.
results.ConvertTo(results, MatType.CV_32S);

Mat matches = new Mat();

// Compare 함수는 예측 라벨과 정답 라벨이 같은 위치를 255, 다른 위치를 0으로 표시한다.
// test_y는 1 x 10000 형태이므로 앞 count개의 라벨을 잘라낸 뒤 T()로 전치해
// results와 같은 count x 1 형태로 맞춘다.
Cv2.Compare(results, test_y[0, 1, 0, count].T(), matches, CmpTypes.EQ);

// CountNonZero 함수는 matches에서 0이 아닌 값, 즉 정답을 맞힌 개수를 센다.
// 맞힌 개수 / 평가 개수 * 100으로 정확도(%)를 출력한다.
Console.WriteLine((float)Cv2.CountNonZero(matches) / count * 100);
