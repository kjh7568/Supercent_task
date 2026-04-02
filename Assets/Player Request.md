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

---

## [002] OreObject 광석 채굴 컴포넌트

**요청 내용**
광석 오브젝트 컴포넌트 구현. 플레이어가 닿으면 바로 캐지는 게 아니라 채굴 시간(miningDuration)이 필요하고, 수집 후 사라졌다가 리스폰 타이머(respawnTime) 이후 재활성화.

**구현 방법**
- 충돌: `OnTriggerEnter/Stay/Exit` (플레이어 태그 기준)
- 채굴: OnTriggerStay에서 miningTimer 누적 → miningDuration 도달 시 픽업 완료
- 중단: OnTriggerExit → miningTimer 리셋
- 리스폰: 픽업 완료 후 Renderer & Collider 비활성화, Coroutine으로 respawnTime 후 재활성화
- 픽업 이벤트: `Action OnPickedUp`으로 외부 시스템(인벤토리 등) 연동 지원

**작업 스탭**

| 스탭 | 내용 | 파일 |
|---|---|---|
| S1 | OreObject 상태 관리 + 채굴 타이머 + 리스폰 로직 | OreObject.cs |
| S2 | 플레이어 충돌 감지 & 픽업 이벤트 발동 | OreObject.cs |
