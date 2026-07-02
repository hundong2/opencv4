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

// =====================================================================
// 예제 08-01: K-평균 클러스터링(K-Means Clustering)을 이용한 색상 양자화
// 이미지를 K개의 색상으로 줄여서 표현한다.
// =====================================================================

// ScriptDir 함수는 현재 실행 중인 .csx 파일의 경로를 기준으로
// 예제 이미지가 있는 폴더 위치를 구할 때 사용한다.
// [CallerFilePath] 특성은 컴파일 시점에 호출자의 파일 경로를 자동으로 삽입한다.
string ScriptDir([CallerFilePath] string path = "")
    => Path.GetDirectoryName(path)!;

// 현재 스크립트 파일이 위치한 디렉터리 경로
string scriptDir = ScriptDir();

static Tuple<float[], int[]> loadTrainData(string image_path, string label_path, int length)
{
    using (FileStream image_data = new FileStream(image_path, FileMode.Open))
    using (FileStream label_data = new FileStream(label_path, FileMode.Open))
    using (BinaryReader image_binary = new BinaryReader(image_data))
    using (BinaryReader label_binary = new BinaryReader(label_data))
    {
        image_binary.ReadBytes(16);
        label_binary.ReadBytes(8);

        float[] image = new float[length * 784];
        int[] label = new int[length];

        for (int di = 0; di < length; ++di)
        {
            for (int i = 0; i < 784; ++i)
            {
                byte img = image_binary.ReadByte();
                image[di * 784 + i] = (float)img;
            }
            byte lb = label_binary.ReadByte();
            label[di] = (int)lb;
        }
        return new Tuple<float[], int[]>(image, label);
    }
}

Tuple<float[], int[]> train = loadTrainData(Path.Combine(scriptDir, "fashion-mnist/train-images-idx3-ubyte"), Path.Combine(scriptDir, "fashion-mnist/train-labels-idx1-ubyte"), 60000);
Tuple<float[], int[]> test = loadTrainData(Path.Combine(scriptDir, "fashion-mnist/t10k-images-idx3-ubyte"), Path.Combine(scriptDir, "fashion-mnist/t10k-labels-idx1-ubyte"), 10000);

Mat train_x = new Mat(60000, 784, MatType.CV_32F, train.Item1);
Mat train_y = new Mat(1, 60000, MatType.CV_32S, train.Item2);
Mat test_x = new Mat(10000, 784, MatType.CV_32F, test.Item1);
Mat test_y = new Mat(1, 10000, MatType.CV_32S, test.Item2);