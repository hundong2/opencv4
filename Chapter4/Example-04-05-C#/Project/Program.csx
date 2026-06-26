#r "nuget: OpenCvSharp4, 4.13.0.20260602"
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"

using System;
using OpenCvSharp;
using System.Runtime.CompilerServices;
using System.IO;

Mat src = new Mat(new Size(500, 500), MatType.CV_8UC3, new Scalar(255, 255, 255));

Cv2.ImShow("draw", src);
// 클로저로 src를 직접 캡처하여 IntPtr 변환 없이 사용
Cv2.SetMouseCallback("draw", (MouseEventTypes @event, int x, int y, MouseEventFlags flags, IntPtr userdata) =>
{
    if (flags == MouseEventFlags.LButton)
    {
        Cv2.Circle(src, new Point(x, y), 10, new Scalar(0, 0, 255), -1);
        Cv2.ImShow("draw", src);
    }
});
Cv2.WaitKey();
Cv2.DestroyAllWindows();

//Mat [ 0*0*CV_8UC1, IsContinuous=False, IsSubmatrix=False, Ptr=0x564e6b57ea10, Data=0x0 ]