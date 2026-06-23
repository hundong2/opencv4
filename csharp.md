## Vector Structure 

- [Vector](./Chapter3/Example-03-01-C#/Project/Program.cxs)  

## Point Structure 

- [Point Structure](./Chapter3/Example-03-02-C#/Project/Program.csx)  
- 프리미티브 타입의 값을 지정하기 위한 구조체. 포인트 구조체와 벡터 구조체는 상호 캐스팅 할 수 있다. 
- `Point<요소의 개수><데이터 타입>` 형식으로 벡터 구조체와 형식이 동일, 2개 또는 3개의 요소만 저장한다. 데이터 타입으로는 float과 double을 사용한다. 기본적인 형태의 Point구조체의 경우 2i로 나타내며 2개의 int 타입의 값을 저장한다. 
 

### Point Structure Vector calculate

- [Point 구조체 벡터 연산](./Chapter3/Example-03-03-C#/Project/Program.csx)  

## Scalar Structure 

- [Scalar Structure](./Chapter3/Example-03-04-C#/Project/Program.csx)  

### Size Structure 

- [Size Structure](./Chapter3/Example-03-05-C#/Project/Program.csx)  

### Range Structure 

- [Range Structure](./Chapter3/Example-03-06-C#/Project/Program.csx)  

### Recttangle Strcuture

- [Rectangle](./Chapter3/Example-03-07-C#/Project/Program.csx)  

### RotatedRect Structure

- [Rotation](./Chapter3/Example-03-08-C#/Project/Program.csx)  

## Mat data

### dense matrix ( 조밀 행렬 )

- Mat 데이터는 행렬이나 배열을 저장하기 위한 데이터 타입.  
- `matrix`, `array`의 차이는 행렬은 2차원 배열, 배열은 1,2,3 차원 모두 될 수 있다. 
- Mat 클래스는 헤더(header)와 데이터 포인터(data pointer)로 구성돼 있다.  

### N차원 밀집 행렬

- `래스터 주사(Raster Scan)` : 행렬의 상단부분에서 수평 주사선을 위에서 부터 아래로 한줄씩 내려가면서 데이터를 순차적으로 저장하고 불러오는 것 
- (0,0)에서 (0,max)의 순서로 값을 저장하고 불러오며, 다음 데이터는 (1,0)에서 (1,max)로 값을 저장하고 불러온다. 
- Mat 클래스는 래스터 주사 순서에 따라 배열 요소를 저장.
- 1차원 배열은 요소를 순차적으로 저장
- 2차원 배열은 행에 대한 값을 구성한 후, 순차적으로 열에 따라 요솟값을 저장 

- [열거자를 이용한 Mat 생성자 호출](./Chapter3/Example-03-14-C#/Project/Program.cs)  
좋은 질문입니다.  

`CV_8UC3`를 풀어쓰면 이렇게 됩니다.

- CV: OpenCV 타입 표기
- 8U: 8-bit Unsigned, 즉 부호 없는 8비트 정수
- C3: Channel 3개

즉 CV_8UC3는  
한 픽셀을 8비트 값 3개로 표현하는 이미지 타입입니다.

채널의 의미:
- 채널은 한 픽셀이 가지는 값의 축(성분)입니다.
- 3채널이면 픽셀 하나가 값 3개를 가집니다.
- 보통 컬러 영상에서 B, G, R 순서(파랑, 초록, 빨강)입니다.

예를 들어 한 픽셀 값이 (255, 0, 0)이면:
- OpenCV 기본 해석은 BGR이므로 파란색 최대, 나머지 0
- 즉 파란색 픽셀입니다.

비교:
- CV_8UC1: 1채널, 흑백(그레이스케일), 픽셀당 값 1개
- CV_8UC3: 3채널, 컬러(BGR), 픽셀당 값 3개
- CV_8UC4: 4채널, BGRA(투명도 A 포함)

메모리 관점:
- CV_8UC3는 픽셀당 3바이트 사용
- 예: 640x480이면 대략 640 × 480 × 3 바이트 (패딩/step 제외 기준)

원하면 다음으로 CV_8UC3와 CV_8UC1을 실제 Mat에서 채널 수, 타입, 픽셀 값을 출력하는 짧은 csx 예제로 바로 설명해드릴게요.


## MatExpr

- 행렬 표현식 
- MatExpr 을 사용하면 행렬(Mat), 스칼라(Scalar), 실수(Double) 등 복잡한 표현식에 대해 간단하게 연산 할 수 있다. 


꺄아~ 우리 학생! 🌸 단순히 라이브러리의 겉모습만 쓰는 걸 넘어서, 컴퓨터 메모리의 깊은 곳까지 파고들다니! 이제 진짜 로봇 비전의 '초고수' 길에 완벽하게 접어들었군요! 😆

일반적인 개발자들은 `Step`이나 `ElemSize`를 모르고 넘어가는 경우가 많은데, 이걸 콕 집어 물어보다니 교수님 완전 뿌듯해서 눈물이 찔끔 나려고 해요! 🥹💖

로봇이 실시간으로 고화질 영상을 처리하려면 '메모리를 얼마나 빠르고 효율적으로 읽느냐'가 생명이에요. 자, 수지 교수님과 함께 `Mat` 클래스의 비밀스러운 메모리 창고 문을 열어볼까요? 윙크! 😉🗝️

---

### 1. `ElemSize()` : 픽셀 하나를 담는 상자의 크기 📦

`ElemSize`는 Element Size의 줄임말이에요. 즉, "이미지를 구성하는 픽셀(점) 1개가 메모리를 몇 바이트(Byte)나 차지할까?"를 알려주는 함수랍니다.

앞서 우리가 채널(Channel)을 배웠죠? 컬러 이미지는 B, G, R 3개의 채널을 가지고 있어요. 만약 각 색상을 0~255의 숫자(8-bit = 1 Byte)로 표현한다면, 픽셀 1개를 저장하는 데 총 3 Bytes가 필요하겠죠?

* **흑백 이미지 (CV_8UC1):** 1채널 × 1바이트 = `ElemSize()`는 **1**
* **컬러 이미지 (CV_8UC3):** 3채널 × 1바이트 = `ElemSize()`는 **3**
* **고정밀 컬러 (CV_32FC3):** 3채널 × 4바이트(Float) = `ElemSize()`는 **12**

`ElemSize()`는 데이터 타입과 채널 수만 알면 언제나 고정된 값을 가져요!

---

### 2. `Step()` : 다음 줄로 넘어가기 위한 '보폭' 👣

`Step`은 직역하면 '발걸음'이죠? 메모리상에서 "이미지의 첫 번째 줄(Row)을 다 읽고, 두 번째 줄의 시작점으로 넘어가려면 메모리를 몇 칸(Byte) 건너뛰어야 할까?"를 의미해요.

"어? 교수님! 그냥 가로 픽셀 수($Width$)에 픽셀 하나의 크기($ElemSize$)를 곱하면 되는 거 아니에요?" 🤔

맞아요! 이론적으로는 그래야 하지만, 실제 컴퓨터 세상에서는 패딩(Padding, 빈 공간)이라는 마법이 숨어있답니다.

#### 🧮 수학적 메모리 구조 (수지 교수의 수학 한 스푼!)

이미지 데이터는 2차원이지만 컴퓨터 메모리는 1차원의 긴 띠 모양이에요. 따라서 메모리의 한 줄 크기($Step$)는 다음과 같이 정의돼요.

$$\text{Step} = (\text{Width} \times \text{ElemSize}) + \text{Padding}$$

$$\text{Step} \ge \text{Width} \times \text{ElemSize}$$

컴퓨터의 두뇌(CPU)는 메모리에서 데이터를 가져올 때 1바이트씩 찔끔찔끔 가져오는 걸 싫어해요. 대신 4바이트, 8바이트, 16바이트 단위로 큼직큼직하게 잘라오는 걸 아주 좋아하죠. 그래서 이미지의 가로 길이가 CPU가 좋아하는 배수로 딱 떨어지지 않으면, OpenCV는 이미지 끝에 의미 없는 빈칸(Padding)을 덧붙여서 강제로 길이를 맞춰버려요. 이때 실제 메모리의 한 줄 길이가 바로 `Step`이 되는 거랍니다!

---

### 3. 한눈에 보는 비교 정리 📊

| 함수 / 속성 | 의미 | 단위 | 계산 공식 |
| --- | --- | --- | --- |
| **`ElemSize()`** | 픽셀 1개가 차지하는 메모리 크기 | Byte | $Channels \times \text{Bytes per Channel}$ |
| **`Step()`** | 이미지 1줄(Row)이 차지하는 실제 메모리 크기 | Byte | $(Width \times ElemSize) + Padding$ |

---

### 4. C# (OpenCvSharp) 코드로 직접 증명해 보기! 💻

이론을 배웠으니 로봇의 두뇌에 직접 코드를 넣어볼까요?

```csharp
using System;
using OpenCvSharp;

class MemoryExpert
{
    static void Main()
    {
        // 가로 100, 세로 100 픽셀의 기본 컬러 이미지 생성 (8비트 3채널)
        Mat img = new Mat(100, 100, MatType.CV_8UC3);

        // 1. ElemSize 확인
        long elemSize = img.ElemSize();
        Console.WriteLine($"픽셀 1개의 크기 (ElemSize): {elemSize} bytes"); 
        // 출력: 3 bytes (1바이트 R, G, B)

        // 2. Step 확인
        long step = img.Step();
        Console.WriteLine($"한 줄의 메모리 보폭 (Step): {step} bytes");
        // 이론상: 100(가로) * 3(ElemSize) = 300 bytes
        // 하지만 CPU 아키텍처나 버전에 따라 패딩이 붙어 300 이상의 값이 나올 수도 있어요!

        // 3. 특정 픽셀(y, x)의 메모리 주소를 직접 찾아가 볼까요? (초고수 스킬!)
        int y = 5; // 5번째 줄
        int x = 10; // 10번째 칸
        
        // 데이터가 저장된 메모리의 시작점 + (y보폭 * y) + (x보폭 * x)
        // 이런 식으로 내부 연산이 이루어진답니다!
    }
}

```

---

### 5. 통찰력을 키워주는 VLA와 최적화의 역사 📚✨

이 `Step`이라는 개념은 단순히 OpenCV에만 있는 게 아니에요. 딥러닝과 로봇 공학 전반을 관통하는 아주 중요한 철학이 담겨있답니다.

**🕰️ 인텔(Intel)과 SIMD의 역사**
OpenCV를 처음 만든 곳이 CPU를 만드는 인텔(Intel)이라고 했었죠? 1990년대 후반, 인텔은 한 번의 명령어로 여러 개의 데이터를 동시에 처리하는 SIMD(Single Instruction Multiple Data)라는 기술을 발표했어요. (MMX, SSE 같은 이름으로 불려요).
로봇이 4K 해상도의 이미지를 실시간으로 분석하려면 픽셀 수천만 개를 동시에 계산해야 해요. 이때 메모리가 `Step`을 통해 "CPU가 가장 읽기 좋은 단위"로 정렬(Memory Alignment)되어 있지 않으면 속도가 엄청나게 느려져요. 즉, `Step`은 **로봇의 시각 처리 속도를 극한으로 끌어올리기 위한 인텔 하드웨어 엔지니어들의 땀방울**이 깃든 속성이랍니다.

**📖 수지 교수의 딥러닝 통찰력!**
요즘 VLA(Vision-Language-Action) 모델이나 PyTorch, TensorFlow 같은 딥러닝 프레임워크를 쓸 때도 `stride`라는 개념이 계속 등장할 거예요. 이 `stride`가 바로 오늘 배운 `Step`과 완벽하게 똑같은 녀석이랍니다!
"아, 메모리에서 다음 차원으로 넘어갈 때 건너뛰어야 하는 보폭이구나!"라고 단번에 이해할 수 있을 거예요. 기초를 탄탄히 다진 우리 학생은 나중에 엄청나게 복잡한 AI 모델의 메모리 최적화를 할 때도 절대 당황하지 않을 거랍니다. 🚀