#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"
using System;   
using OpenCvSharp;
using System.Runtime.InteropServices;

// Mat.Eye(): 단위 행렬(Identity Matrix) 생성
// Size(2, 2): 2x2 크기
// CV_8UC2: 8-bit unsigned char, 2채널 (한 픽셀당 2개의 byte 값)
Mat m = Mat.Eye(new Size(2, 2), MatType.CV_8UC2);

// m.Rows: 행렬의 행 개수 (높이)
// m.Cols: 행렬의 열 개수 (너비)
for (int y = 0; y < m.Rows; y++)
{
    for (int x = 0; x < m.Cols; x++)
    {
        // m.Step(): 한 행의 총 바이트 크기 (메모리 stride, 패딩 포함 가능)
        // m.ElemSize(): 한 픽셀의 총 바이트 크기 (2채널이면 2bytes)
        // 오프셋 계산: y행 * stride + x열 * 픽셀크기
        int offset = (int)m.Step() * y + m.ElemSize() * x;
        
        // m.Ptr(0): Mat 데이터의 시작 주소(포인터) 반환
        // Marshal.ReadByte(): 지정된 오프셋 위치에서 1바이트 읽기
        // offset + 0: 첫 번째 채널 위치
        byte i = Marshal.ReadByte(m.Ptr(0), offset + 0);
        
        // 주석 처리된 부분: 두 번째, 세 번째 채널 읽기 (현재 CV_8UC2는 2채널이므로 +2까지만 사용)
        //byte j = Marshal.ReadByte(m.Ptr(0), offset + 1);	// 두 번째 채널
        //byte k = Marshal.ReadByte(m.Ptr(0), offset + 2); 	// 세 번째 채널
        
        Console.WriteLine($"{offset} - ({y}, {x}) : {i}");
    }
}
// Output ----
// 0 - (0, 0) : 1
// 2 - (0, 1) : 0
// 4 - (1, 0) : 0
// 6 - (1, 1) : 1