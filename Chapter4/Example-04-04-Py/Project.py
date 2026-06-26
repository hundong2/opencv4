import cv2

# 이미지를 그레이스케일(흑백) 모드로 읽어 들임
src = cv2.imread("OpenCV_Logo.png", cv2.IMREAD_GRAYSCALE)

# 윈도우 생성: 이름 "src", WINDOW_FREERATIO 플래그로 비율 제한 없이 크기 조절 가능
cv2.namedWindow("src", flags=cv2.WINDOW_FREERATIO)
# 윈도우 크기를 가로 400, 세로 200 픽셀로 설정
cv2.resizeWindow("src", 400, 200)
# 지정한 윈도우에 이미지를 표시
cv2.imshow("src", src)
# 키 입력을 대기 (0 = 무한 대기, 아무 키나 누르면 진행)
cv2.waitKey(0)
# "src" 윈도우를 닫고 메모리 해제
cv2.destroyWindow("src")