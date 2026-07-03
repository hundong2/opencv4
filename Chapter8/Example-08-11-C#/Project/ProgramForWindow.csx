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
using OpenCvSharp.ML;

// =====================================================================
// 예제 08-11: SVM(Support Vector Machine)을 이용한 Fashion-MNIST 분류
// 28x28 의류 이미지를 784차원 벡터로 읽고, SVM 모델로 의류 클래스를 예측한다.
// =====================================================================

// ScriptDir 함수는 현재 실행 중인 .csx 파일의 경로를 기준으로
// 예제 이미지가 있는 폴더 위치를 구할 때 사용한다.
// [CallerFilePath] 특성은 컴파일 시점에 호출자의 파일 경로를 자동으로 삽입한다.
string ScriptDir([CallerFilePath] string path = "")
    => Path.GetDirectoryName(path)!;

// 현재 스크립트 파일이 위치한 디렉터리 경로(Project 폴더)
string scriptDir = ScriptDir();

// Fashion-MNIST 데이터는 Project 폴더 안이 아니라 예제 폴더 바로 아래의 temp 폴더에 있다.
// scriptDir의 상위 폴더를 구해서 Example-08-11-C# 폴더 기준 경로를 만든다.
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

// 학습 데이터 60,000장과 테스트 데이터 10,000장을 읽는다.
// temp 폴더에는 Fashion-MNIST의 IDX 원본 파일이 있어야 한다.
Tuple<float[], int[]> train = loadTrainData(Path.Combine(dataDir, "train-images-idx3-ubyte"), Path.Combine(dataDir, "train-labels-idx1-ubyte"), 60000);
Tuple<float[], int[]> test = loadTrainData(Path.Combine(dataDir, "t10k-images-idx3-ubyte"), Path.Combine(dataDir, "t10k-labels-idx1-ubyte"), 10000);

// FromPixelData 함수는 C# 배열을 OpenCvSharp Mat으로 감싼다.
//
// train_x/test_x:
// - 행 하나가 이미지 한 장이다.
// - 열 784개는 28x28 픽셀을 펼친 특징 벡터이다.
// - SVM 학습 입력은 float 형식이어야 하므로 CV_32F를 사용한다.
//
// train_y/test_y:
// - 각 이미지의 정답 라벨이다.
// - 정수 클래스 번호를 저장하므로 CV_32S를 사용한다.
Mat train_x = Mat.FromPixelData(60000, 784, MatType.CV_32F, train.Item1);
Mat train_y = Mat.FromPixelData(1, 60000, MatType.CV_32S, train.Item2);
Mat test_x = Mat.FromPixelData(10000, 784, MatType.CV_32F, test.Item1);
Mat test_y = Mat.FromPixelData(1, 10000, MatType.CV_32S, test.Item2);

// SVM.Create 함수는 OpenCV ML의 SVM 분류기 객체를 만든다.
// SVM은 클래스들을 잘 나누는 결정 경계(decision boundary)를 학습하는 알고리즘이다.
SVM svm = SVM.Create();

// Type은 SVM이 풀 문제의 종류를 지정한다.
// CSvc는 C-Support Vector Classification으로, 여러 클래스 분류에 사용하는 기본 타입이다.
svm.Type = SVM.Types.CSvc;

// KernelType은 데이터를 어떤 방식의 경계로 나눌지 정한다.
// Poly는 다항식 커널이며, 직선으로 분리하기 어려운 데이터를 곡선 경계로 나눌 수 있게 한다.
svm.KernelType = SVM.KernelTypes.Poly;

// Degree는 다항식 커널의 차수이다.
// 3이면 3차 다항식 형태의 결정 경계를 사용한다.
svm.Degree = 3;

// Gamma는 커널 함수에서 샘플 간 영향 범위를 조절하는 값이다.
// 값이 커지면 가까운 샘플의 영향이 상대적으로 강해져 경계가 더 복잡해질 수 있다.
svm.Gamma = 5.0;

// C는 오분류 허용 정도와 경계 여유(margin) 사이의 균형을 조절한다.
// 값이 클수록 학습 데이터를 틀리지 않게 맞추려는 성향이 강해진다.
svm.C = 3.0;

// Coef0는 다항식/시그모이드 커널에서 사용하는 상수항이다.
// 여기서는 추가 상수항 없이 0으로 둔다.
svm.Coef0 = 0;

// Train 함수는 학습 샘플과 정답 라벨로 SVM 모델을 학습한다.
// SampleTypes.RowSample은 train_x의 각 행이 하나의 이미지 샘플이라는 뜻이다.
svm.Train(train_x, SampleTypes.RowSample, train_y);

// 전체 테스트 데이터 중 앞쪽 count개만 평가한다.
// SVM 예측 결과를 빠르게 확인하기 위해 10,000개 전체가 아니라 500개만 사용한다.
int count = 500;

// results에는 각 테스트 이미지에 대해 SVM이 예측한 라벨이 저장된다.
Mat results = new Mat();

// Predict 함수는 학습된 SVM 모델로 입력 샘플의 클래스를 예측한다.
// test_x[0, count, 0, 784]는 테스트 데이터에서 0행부터 count행 전까지,
// 전체 784개 픽셀 특징을 선택한다는 뜻이다.
svm.Predict(test_x[0, count, 0, 784], results);

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
