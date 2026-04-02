# Player Request

---

## [001] 캐릭터 터치 이동 컨트롤

**요청 내용**
다이나믹 조이스틱 방식의 터치 이동 구현. 모바일 환경 기준.

**구현 방법**
- 입력: `Input.touches` 사용 (마우스 없음, 단일 터치)
- 이동: `Transform.Translate` (Rigidbody 없음)
- 회전: 이동 방향으로 `Quaternion.LookRotation`

**작업 스탭**

| 스탭 | 내용 | 파일 |
|---|---|---|
| S1 | 다이나믹 조이스틱 구현 | DynamicJoystick.cs |
| S2 | PlayerController 구현 | PlayerController.cs |
| S3 | 카메라 Follow | CameraFollow.cs |
| S4 | 씬 셋업 & 테스트 | - |
