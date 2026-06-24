#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"
using System;   
using OpenCvSharp;
using System.Runtime.InteropServices;

Mat m = new Mat(1280, 1920, MatType.CV_8UC3);

Mat coi = m.ExtractChannel(0); //Mat 클래스의 채널을 떼어내어 설정하는 것 
//MatType에 변화가 생기지만 채널만 변경 될 뿐 정밀도에 대한 부분은 유지, 다중 채널 이미지나 배열에서 특정 채널을 추출해서 단일 채널로 반환 
Console.WriteLine(coi);

Cv2.ExtractChannel(m, coi, 0); //ExtractChannel() 메서드로 특정 채널을 추출하여 단일 채널로 반환
Console.WriteLine(coi);
// Output ----
//Mat [ 1280*1920*CV_8UC1, IsContinuous=True, IsSubmatrix=False, Ptr=0x55c1eb296d00, Data=0x55c1eab00cc0 ]