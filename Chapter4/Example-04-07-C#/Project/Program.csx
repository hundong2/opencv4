// NuGet에서 OpenCvSharp4 라이브러리 참조 (버전: 4.13.0.20260602)
#r "nuget: OpenCvSharp4, 4.13.0.20260602"
// Linux x64 환경용 OpenCV 네이티브 런타임 바이너리 참조
#r "nuget: OpenCvSharp4.official.runtime.linux-x64, 4.13.0.20260602"

using System;
using OpenCvSharp;
using System.Runtime.CompilerServices;  // CallerFilePath 어트리뷰트 사용에 필요
using System.IO;

// [CallerFilePath] : 컴파일 시점에 현재 스크립트 파일의 전체 경로를 자동으로 주입
// 이를 통해 스크립트가 실행되는 파일의 디렉터리 경로를 런타임에 동적으로 구할 수 있음
string ScriptDir([CallerFilePath] string path = "") 
    => Path.GetDirectoryName(path)!;

// 현재 스크립트 파일이 위치한 디렉터리 경로
string scriptDir = ScriptDir();

// 재생할 동영상 파일의 절대 경로 (스크립트 기준 상대 경로로 지정)
var videoPath = Path.Combine(scriptDir, "bin/Debug/Star.mp4");

// VideoCapture : 동영상 파일 또는 카메라 스트림을 읽기 위한 OpenCV 클래스
// 인자로 파일 경로를 전달하면 해당 동영상 파일을 열어 프레임 단위로 읽을 수 있음
VideoCapture capture = new VideoCapture(videoPath);

// Mat : OpenCV에서 이미지/프레임 데이터를 저장하는 행렬 클래스
// 각 프레임을 담을 빈 행렬 객체 초기화
Mat frame = new Mat();

while(true)
{
    // PosFrames : 현재 재생 위치(프레임 인덱스)
    // FrameCount : 동영상 전체 프레임 수
    // 마지막 프레임에 도달하면 동영상을 처음부터 다시 열어 반복 재생
    if (capture.PosFrames == capture.FrameCount) capture.Open(videoPath);

    // 현재 위치에서 프레임 한 장을 읽어 frame 행렬에 저장
    // 성공 시 true 반환, 스트림 종료 시 false 반환
    capture.Read(frame);

    // "VideoFrame" 이름의 창에 현재 프레임 이미지를 출력
    Cv2.ImShow("VideoFrame", frame);
    
    // WaitKey(33) : 33ms 동안 키 입력 대기 (~30fps에 해당)
    // 'q' 키 입력 시 루프 종료
    if (Cv2.WaitKey(33) == 'q') break;
}

// VideoCapture 리소스 해제 (파일 핸들, 메모리 반환)
capture.Release();

// 열려 있는 모든 OpenCV 창 닫기
Cv2.DestroyAllWindows();