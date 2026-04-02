# All Prompt Log

---

## 2026-04-02

**User:** 자 지금부터 프로젝트를 시작할거야. 먼저 해야할 거는 캐릭터 컨트롤이야. 터치 이동으로 만들거고. 플레이어 프리팹은 준비해놨어. 작업 프로세스 짜봐

**Assistant:** 프로젝트 구조 파악 후 터치 이동 방식 확인.
- A. 고정 조이스틱 / B. 다이나믹 조이스틱 / C. 드래그 이동
- 이동 방식: Rigidbody / CharacterController / Transform

**User:** B안으로 갈거야. 리지드 바디는 없고 트랜스폼 이동으로 하자

**Assistant:** 작업 프로세스 제안
- S1. 다이나믹 조이스틱 구현 (DynamicJoystick.cs)
- S2. PlayerController 구현 (PlayerController.cs)
- S3. 카메라 Follow (CameraFollow.cs)
- S4. 씬 셋업 & 테스트

**User:** s1 저렇게 복잡하게 해야하나?

**Assistant:** S1, S2 합치고 단순화. IPointerDownHandler 등 인터페이스 제거.

**User:** 아니아니 구현하는건 냅두고 내 말은 IPointerDownHandler, IDragHandler, IPointerUpHandler이런거 굳이 쓸 필요 있냐는거지

**Assistant:** Input.touches로 직접 처리하는 방식으로 변경.

**User:** 모바일 환경이라고 생각하면 마우스 버튼은 아니겠지?

**Assistant:** Input.touches 단일 터치 처리로 확정.

**User:** ㅇㅇ 시작하기 전에 메모리.엠디에 써져있듯이 앞으로 할 작업내용 플레이어 리퀘스트에 적어놓고 어떻게 할건지도 적어놔

**Assistant:** Player Request.md 생성, S1 진행.
- DynamicJoystick.cs 생성: Input.touches 기반 다이나믹 조이스틱, Transform 이동용 방향벡터 출력

**User:** 앞으로 나와하는 모든 대화 내용을 all prompt.md에 저장해줘. 이거 메모리에 저장해놔. 그리고 s2는 아직 하지마

**Assistant:** 메모리 저장 완료. all prompt.md 생성. S2 대기.
