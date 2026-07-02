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

// ImRead 함수는 지정한 경로의 이미지를 Mat 객체로 읽어 온다.
// 이 예제에서는 egg.jpg의 모든 픽셀 색상을 K개의 대표 색상으로 묶는다.
Mat src = Cv2.ImRead(Path.Combine(scriptDir, "bin/Debug/egg.jpg"));

// data는 K-Means에 입력할 학습 데이터이다.
// 이미지의 각 픽셀(B, G, R)을 하나의 3차원 샘플로 사용한다.
Mat data = new Mat();

// Reshape 함수는 Mat의 실제 데이터는 복사하지 않고 행/채널 구조만 바꾼다.
// src는 원래 Height x Width 크기의 3채널 이미지인데,
// K-Means는 "샘플 개수 x 특징 벡터" 형태의 데이터를 받으므로
// (전체 픽셀 수) x 1 형태의 3채널 Mat으로 바꾼다.
//
// Reshape(3, src.Width * src.Height)
// - 3                       : 채널 수를 B, G, R 3채널로 유지
// - src.Width * src.Height  : 행 개수를 전체 픽셀 수로 변경
//
// ConvertTo 함수는 데이터 타입을 변환한다.
// Kmeans 함수는 부동소수점 입력을 사용하므로 CV_32FC3 타입으로 변환한다.
src.Reshape(3, src.Width * src.Height).ConvertTo(data, MatType.CV_32FC3);

// K는 만들 클러스터 개수이다.
// 색상 양자화에서는 최종 이미지에 사용할 대표 색상 개수를 뜻한다.
int K = 7;

// bestLabels : 각 픽셀이 어느 클러스터에 속하는지 저장되는 결과 Mat
// centers    : 각 클러스터의 중심값, 즉 대표 BGR 색상이 저장되는 결과 Mat
Mat bestLabels = new Mat();
Mat centers = new Mat();

// Kmeans 함수는 입력 데이터를 K개의 그룹으로 나눈다.
//
// 매개변수:
// data                       : 입력 샘플. 여기서는 모든 픽셀의 BGR 값
// K                          : 클러스터 개수
// bestLabels                 : 각 샘플이 속한 클러스터 번호를 받을 Mat
// TermCriteria.Both(10, 0.001): 최대 10번 반복하거나 중심 이동량이 0.001 이하이면 종료
// 10                         : 서로 다른 초기 중심으로 K-Means를 10번 시도
// KMeansFlags.RandomCenters  : 초기 중심을 무작위로 선택
// centers                    : 최종 클러스터 중심값을 받을 Mat
//
// 반환값 retval은 각 샘플과 해당 클러스터 중심 사이 거리 제곱의 합이다.
// 값이 작을수록 픽셀들이 각 대표 색상에 더 가깝게 묶였다는 뜻이다.
double retval = Cv2.Kmeans(data, K, bestLabels, TermCriteria.Both(10, 0.001), 10, KMeansFlags.RandomCenters, centers);

// bestLabels는 각 픽셀의 클러스터 번호를 담고 있다.
// Mat<int>로 감싸고 Indexer를 얻으면 배열처럼 빠르게 값을 읽을 수 있다.
Mat<int> bestLabels3b = new Mat<int>(bestLabels);
MatIndexer<int> bestLabelsIndexer = bestLabels3b.GetIndexer();

// centers는 K-Means 계산을 위해 float 타입이므로,
// 실제 이미지 픽셀에 넣을 수 있도록 8비트 unsigned char 3채널 색상으로 변환한다.
centers.ConvertTo(centers, MatType.CV_8UC3);

// centersIndexer[clusterIdx] 형태로 클러스터 대표 색상(Vec3b)을 읽기 위해
// Mat<Vec3b>와 Indexer를 준비한다.
Mat<Vec3b> centers3b = new Mat<Vec3b>(centers);
MatIndexer<Vec3b> centersIndexer = centers3b.GetIndexer();

// idx는 1차원으로 펼쳐진 bestLabels에서 현재 픽셀에 해당하는 라벨 위치를 가리킨다.
int idx = 0;

// dst는 색상 양자화 결과를 저장할 출력 이미지이다.
// 원본과 같은 크기, 같은 8비트 3채널 BGR 형식으로 만든다.
Mat dst = new Mat(new Size(src.Width, src.Height), MatType.CV_8UC3);

// dst도 픽셀 단위로 값을 쓰기 쉽도록 Vec3b Indexer를 준비한다.
Mat<Vec3b> dst3b = new Mat<Vec3b>(dst);
MatIndexer<Vec3b> dstIndexer = dst3b.GetIndexer();

// 원본 이미지의 각 픽셀 위치를 순회하면서,
// 해당 픽셀이 속한 클러스터 번호를 읽고 그 클러스터의 대표 색상으로 바꿔 넣는다.
// 결과적으로 모든 픽셀 색상은 K개의 centers 색상 중 하나가 된다.
for (int y = 0; y < dst.Height; y++)
{
    for (int x = 0; x < dst.Width; x++)
    {
        int clusterIdx = bestLabelsIndexer[idx];
        Vec3b color = centersIndexer[clusterIdx];
        dstIndexer[y, x] = color;
        idx++;
    }
}

// ImShow 함수는 지정한 이름의 창에 이미지를 표시한다.
// WaitKey는 키 입력이 있을 때까지 창을 유지하고,
// DestroyAllWindows는 열린 OpenCV 창을 모두 닫는다.
Cv2.ImShow("dst", dst);
Cv2.WaitKey();
Cv2.DestroyAllWindows();
