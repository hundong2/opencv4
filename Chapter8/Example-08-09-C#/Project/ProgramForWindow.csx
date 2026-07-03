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
using OpenCvSharp.ML;

// =====================================================================
// 예제 08-07: K-최근접 이웃(K-Nearest Neighbors, KNN)을 이용한 Fashion-MNIST 분류
// 28x28 의류 이미지를 784차원 벡터로 읽고, 가까운 학습 샘플을 기준으로 라벨을 예측한다.
// =====================================================================

// ScriptDir 함수는 현재 실행 중인 .csx 파일의 경로를 기준으로
// 예제 이미지가 있는 폴더 위치를 구할 때 사용한다.
// [CallerFilePath] 특성은 컴파일 시점에 호출자의 파일 경로를 자동으로 삽입한다.
string ScriptDir([CallerFilePath] string path = "")
    => Path.GetDirectoryName(path)!;

// 현재 스크립트 파일이 위치한 디렉터리 경로(Project 폴더)
string scriptDir = ScriptDir();

// Fashion-MNIST 데이터는 Project 폴더 안이 아니라 예제 폴더 바로 아래의 temp 폴더에 있다.
// scriptDir의 상위 폴더를 구해서 Example-08-07-C# 폴더 기준 경로를 만든다.
string exampleDir = Directory.GetParent(scriptDir)!.FullName;
string dataDir = Path.Combine(exampleDir, "temp");
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


Tuple<float[], int[]> train = loadTrainData(Path.Combine(dataDir, "train-images-idx3-ubyte"), Path.Combine(dataDir, "train-labels-idx1-ubyte"), 60000);
Tuple<float[], int[]> test = loadTrainData(Path.Combine(dataDir, "t10k-images-idx3-ubyte"), Path.Combine(dataDir, "t10k-labels-idx1-ubyte"), 10000);

// FromPixelData 함수는 C# 배열을 OpenCvSharp Mat으로 감싼다.
//
// train_x/test_x:
// - 행 하나가 이미지 한 장이다.
// - 열 784개는 28x28 픽셀을 펼친 특징 벡터이다.
// - KNN 학습 입력은 float 형식이어야 하므로 CV_32F를 사용한다.
//
// train_y/test_y:
// - 각 이미지의 정답 라벨이다.
// - 정수 클래스 번호를 저장하므로 CV_32S를 사용한다.
Mat train_x = Mat.FromPixelData(60000, 784, MatType.CV_32F, train.Item1);
Mat train_y = Mat.FromPixelData(1, 60000, MatType.CV_32S, train.Item2);
Mat test_x = Mat.FromPixelData(10000, 784, MatType.CV_32F, test.Item1);
Mat test_y = Mat.FromPixelData(1, 10000, MatType.CV_32S, test.Item2);

KNearest knn = KNearest.Create();
knn.Train(train_x, SampleTypes.RowSample, train_y);

int count = 500;
Mat results = new Mat();
Mat neighborResponses = new Mat();
Mat dists = new Mat();
int retval = (int)knn.FindNearest(test_x[0, count, 0, 784], 7, results, neighborResponses, dists);
results.ConvertTo(results, MatType.CV_32S);

for (int i = 0; i < count; ++i)
{
    float[] image_array = new float[784];
    Array.Copy(test.Item1, 784 * i, image_array, 0, 784);
    Mat image = Mat.FromPixelData(28, 28, MatType.CV_32F, image_array);
    image.ConvertTo(image, MatType.CV_8UC1);

    Console.WriteLine($"Index : {i}");
    Console.WriteLine($"예측값 : {label_dict[results.At<int>(i)]}");
    Console.WriteLine($"실젯값 : {label_dict[test_y.At<int>(0, i)]}");
    Cv2.ImShow("image", image);
    Cv2.WaitKey();
}
Cv2.DestroyAllWindows();