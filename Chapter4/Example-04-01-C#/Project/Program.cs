using System;
using OpenCvSharp;

namespace Project
{
    class Program
    {
        static void Main(string[] args)
        {
            Mat src = Cv2.ImRead("OpenCV_Logo.png", ImreadModes.ReducedColor2);
            Console.WriteLine(src);
            //Mat [ 369*300*CV_8UC3, IsContinuous=True, IsSubmatrix=False, Ptr=0x5631c2ec1380, Data=0x7f4bc4109040 ]
        }
    }
}