`Mat`는 **OpenCV의 핵심 클래스**로, 이미지나 행렬 데이터를 저장하는 다차원 배열입니다.

## Mat 클래스 개요

```csharp
Mat img = new Mat(size, MatType.CV_8UC3);
```

### 주요 특징:
- **다차원 배열**: 이미지, 비디오 프레임, 행렬 데이터 등 저장
- **메모리 자동 관리**: 생성/소멸 시 자동으로 메모리 할당/해제
- **다양한 데이터 타입 지원**: 정수, 실수, 복소수 등

### 생성자 옵션:

```csharp
// 1. 크기와 타입으로 생성
Mat img = new Mat(640, 480, MatType.CV_8UC3);
// 640x480 크기, 8비트 부호없는 정수, 3채널(RGB)

// 2. Size 객체 사용
Size size = new Size(640, 480);
Mat img2 = new Mat(size, MatType.CV_8UC3);

// 3. 초기값 설정
Mat img3 = new Mat(480, 640, MatType.CV_8UC3, new Scalar(0, 0, 0));
// 검은색으로 초기화
```

### MatType 주요 타입:

| 타입 | 설명 |
|------|------|
| `CV_8UC1` | 8비트, 그레이스케일 (1채널) |
| `CV_8UC3` | 8비트, RGB (3채널) |
| `CV_8UC4` | 8비트, RGBA (4채널) |
| `CV_32FC1` | 32비트 실수, 1채널 |

### 주요 메서드:

```csharp
// 크기 정보
img.Size();          // Size 반환
img.Width;           // 너비
img.Height;          // 높이
img.Rows;            // 행 수
img.Cols;            // 열 수

// 채널/타입 정보
img.Channels();      // 채널 수
img.Type();          // MatType 반환
img.Depth();         // 깊이 (데이터 타입)

// 픽셀 접근
img.At<Vec3b>(y, x); // (y,x) 위치 픽셀값 접근
```

### 실제 사용 예:

```csharp
// 빈 이미지 생성
Mat img = new Mat(480, 640, MatType.CV_8UC3, Scalar.All(0));

// 픽셀값 설정 (파란색)
img.Set<Vec3b>(0, 0, new Vec3b(255, 0, 0)); // BGR 순서

// 이미지 저장
Cv2.ImWrite("output.jpg", img);

// 이미지 로드
Mat loaded = Cv2.ImRead("input.jpg");
```

**요약**: Mat은 이미지 처리의 모든 기본이 되는 데이터 구조로, 픽셀을 행렬로 관리합니다.