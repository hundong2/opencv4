#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"
using System;   
using OpenCvSharp;
using System.Runtime.InteropServices;

// 원본 이미지 Mat 생성
// 1280행 x 1920열, 3채널 컬러 이미지(CV_8UC3)
Mat m = new Mat(1280, 1920, MatType.CV_8UC3);

// new Mat(m, Rect(...))
// 원본 Mat m에서 사각형 영역을 잘라서 ROI(관심 영역) Mat을 만듭니다.
// Rect(x, y, width, height) 형태이므로,
// x=300, y=300 위치에서 가로 100, 세로 100 영역을 선택합니다.
Mat roi1 = new Mat(m, new Rect(300, 300, 100, 100));

// 인덱서 방식으로 ROI를 얻는 예제입니다.
// m[y1, y2, x1, x2] 형태로 행 범위와 열 범위를 지정합니다.
// 아래 코드는 0행부터 99행, 0열부터 99열까지의 100x100 영역을 잘라냅니다.
Mat roi2 = m[0, 100, 0, 100];

// SubMat(startRow, endRow, startCol, endCol)
// 지정한 범위의 부분 행렬을 반환합니다.
// 아래 코드는 100행~299행, 200열~299열을 잘라낸 200x100 ROI입니다.
// 주의: endRow/endCol는 포함하지 않는 범위로 해석됩니다.
Mat roi3 = m.SubMat(100, 300, 200, 300);

// 원본 Mat과 ROI Mat의 정보 출력
// IsSubmatrix=True 이면 원본 메모리를 공유하는 부분 행렬이라는 뜻입니다.
Console.WriteLine(m);
Console.WriteLine(roi1);
Console.WriteLine(roi2);
Console.WriteLine(roi3);

// Output ----
// m : 원본 전체 행렬
// - 1280*1920*CV_8UC3 -> 1280행 x 1920열, 3채널 컬러 이미지
// - IsContinuous=True -> 메모리가 한 덩어리로 연속 저장됨
// - IsSubmatrix=False -> 원본 행렬 자체
// - Ptr / Data -> 내부 포인터와 실제 데이터 시작 주소
// Mat [ 1280*1920*CV_8UC3, IsContinuous=True, IsSubmatrix=False, Ptr=0x557195b06320, Data=0x557195fc7680 ]
// roi1 : Rect(300, 300, 100, 100)로 잘라낸 ROI
// - 100*100*CV_8UC3 -> 100x100 크기의 부분 행렬
// - IsContinuous=False -> 원본에서 일부만 잘라왔기 때문에 연속 메모리가 아님
// - IsSubmatrix=True -> 원본 m의 일부를 참조하는 ROI
// Mat [ 100*100*CV_8UC3, IsContinuous=False, IsSubmatrix=True, Ptr=0x557195afcdc0, Data=0x55719616d804 ]
// roi2 : m[0, 100, 0, 100]로 잘라낸 ROI
// - 행 0~99, 열 0~99 영역
// - 원본의 좌상단부터 잘라서 Data 주소가 원본 Data와 같아 보일 수 있음
// - 역시 부분 행렬이므로 IsSubmatrix=True
// Mat [ 100*100*CV_8UC3, IsContinuous=False, IsSubmatrix=True, Ptr=0x557195afbfd0, Data=0x557195fc7680 ]
// roi3 : m.SubMat(100, 300, 200, 300)으로 잘라낸 ROI
// - 행 100~299, 열 200~299 영역
// - 크기는 200x100
// - 중간 영역이므로 Data 시작 주소가 원본과 다름
// - 부분 행렬이라 IsSubmatrix=True
// Mat [ 200*100*CV_8UC3, IsContinuous=False, IsSubmatrix=True, Ptr=0x557195afecd0, Data=0x5571960542d8 ]