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