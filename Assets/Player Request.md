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

---

## [003] StackSystem 범용 적재 시스템

**요청 내용**
플레이어가 광석을 채광하거나 가공품을 획득할 때, 해당 오브젝트들이 3D 오브젝트 형태로 손과 등에 쌓이는 운반 시스템 구현. 광물과 가공품이 동시에 사용할 수 있는 확장성 있는 구조.

**구현 방법**
- 데이터: `StackItemConfig` ScriptableObject (아이템 타입, 프리팹)로 아이템 종류 정의
- 적재 앵커: `StackPoint` 컴포넌트를 플레이어 자식 오브젝트(손/등)에 부착, targetType·capacity·stackAxis·itemSpacing 설정
- 타입별 라우팅: 광석 → 등(StackPoint_Back), 가공품 → 손(StackPoint_Hand). StackPoint의 `targetType`으로 구분
- 총괄 관리: `StackSystem` 컴포넌트(플레이어)에서 StackPoint 목록 관리, Add/Remove 처리
- 연동: OreObject.OnPickedUp을 `Action<StackItemConfig>`로 변경, StackSystem이 Start()에서 구독
- 배치 방식: 아이템 인스턴스를 StackPoint 자식으로 붙이고 `anchor + axis * spacing * count` 위치에 배치
- Remove: `Remove(StackItemType type)`으로 타입별 납품 (광석만/가공품만 별도 처리)

**작업 스탭**

| 스탭 | 내용 | 파일 |
|---|---|---|
| S1 | StackItemType enum + StackItemConfig ScriptableObject | StackItemType.cs, StackItemConfig.cs |
| S2 | StackPoint 컴포넌트 — 적재 앵커 단위 관리 (targetType 필드 포함) | StackPoint.cs |
| S3 | StackSystem 컴포넌트 — 플레이어 총괄 관리자 | StackSystem.cs |
| S4 | OreObject 연동 — 이벤트 시그니처 변경 + config 필드 추가 | OreObject.cs |
| S5 | 씬 셋업 — StackPoint 오브젝트 배치, SO 생성, 프리팹 연결 | 에디터 작업 |

**구현 중 변경사항**
- `StackPoint.targetType` 추가: 초기 설계는 순서 기반 라우팅이었으나, 광석=등/가공품=손 분리 요구로 타입 기반 라우팅으로 변경
- `StackSystem.Add()`: 타입 일치 StackPoint만 탐색하도록 변경
- `StackSystem.Remove()`: 파라미터 없던 것을 `Remove(StackItemType type)`으로 변경 — 납품 시 광석/가공품 독립 처리 필요

**BF: 등 꽉 찼을 때 광석 사라지는 문제**

- 문제: 등이 꽉 찬 상태에서 광석 채굴이 완료되면 광석이 사라지면 안 됨 (근본 문제)
- 파생 문제: 9/10 상태에서 2개 동시 채굴 시 둘 다 StartMining() 통과 → 먼저 완료된 게 10번째 슬롯 채우면 나중 완료된 것도 공간 없이 사라짐
- 해결:
  - `OreObject.StartMining()`: StackSystem.HasSpace() 체크 → 공간 없으면 채굴 시작 안 함
  - `OreObject.CompletePickup()`: 완료 시점에도 HasSpace() 재확인 → 공간 없으면 Active 복귀, 광석 유지
  - `StackSystem.HasSpace(StackItemType)` 메서드 추가

---

## [004] InteractionSystem 범용 상호작용 존

**요청 내용**
DepositZone(아이템 투입)과 PickupZone(아이템 수령)으로 구성된 범용 상호작용 존 구현.
가공기(Processor)와 계산대(CashRegister) 두 곳에서 공통으로 재사용 가능한 확장성 있는 구조.

- 가공기: DepositZone(광석 투입) → 가공 시간 → PickupZone(가공품 수령)
- 계산대: DepositZone(가공품 투입) → 즉시 판매 → PickupZone(코인 수령)
- 아이템 이동: 플레이어 진입 트리거 → StackSystem에서 아이템 하나씩 순차적으로 날아옴
- 쌓기 형태: 일자(Linear) 또는 N열 그리드(Grid) — 존마다 독립 설정

**구현 방법**
- 레이아웃 전략 패턴: `ZoneStackLayout` 추상 베이스 → `LinearZoneLayout`(일자), `GridZoneLayout`(N열) 구현체
  - `GetLocalPosition(index)` 반환으로 존이 레이아웃에 무관하게 위치 계산
  - 인스펙터에서 columns, spacing 등 설정 가능
- 존 베이스: `InteractionZone` (OnTriggerEnter/Exit 추상 베이스, playerTag 기준)
- DepositZone: 플레이어 진입 → StackSystem에서 순차 아이템 이동 수신
- PickupZone: 플레이어 진입 → 존 아이템 순차 StackSystem으로 이동
- 아이템 이동 애니메이션: `ItemTransferAnimator` — 포물선 arc 코루틴, 순차 딜레이
- Processor: DepositZone + PickupZone 조합, 가공 시간 후 결과물 생성
- CashRegister: DepositZone + PickupZone 조합, 투입 즉시 Coin 생성
- StackItemType에 Coin 타입 추가

**작업 스탭**

| 스탭 | 내용 | 파일 |
|---|---|---|
| S1 | Coin 타입 추가 + ZoneStackLayout 전략 + DepositZone 진입 감지 & 더미 배치 검증 | StackItemType.cs, ZoneStackLayout.cs, LinearZoneLayout.cs, GridZoneLayout.cs, InteractionZone.cs, DepositZone.cs |
| S2 | ItemTransferAnimator + DepositZone ↔ StackSystem 실제 아이템 이동 | ItemTransferAnimator.cs, DepositZone.cs |
| S3 | PickupZone 구현 (존 → 플레이어 순차 이동) | PickupZone.cs |
| S4 | Processor (가공기) 구현 | Processor.cs |
| S5 | CashRegister (계산대) 구현 | CashRegister.cs |
| S6 | 씬 셋업 & 통합 테스트 | 에디터 작업 |

---

## [005] CustomerSystem 손님 시스템

**요청 내용**
손님이 타이머 기반으로 스폰되어 계산대 앞에 줄을 서고, 요구 수량만큼 제품이 DepositZone에 쌓이면 제품을 하나씩 받아가며 코인을 지불하는 시스템 구현.

- 줄서기: 계산대 앞 슬롯 자동 계산(시작 위치·방향·간격), 최대 인원 인스펙터 설정
- 이동: Transform.MoveTowards 단순 이동
- 구매 연출: 제품이 손님에게 하나씩 날아가는 ItemTransferAnimator 포물선 이동
- 보상: 제품 1개당 코인 1개 PickupZone에 추가 (coinRewardPerItem = 10)
- 손님 관리: ObjectPool로 재사용

**구현 방법**
- `CustomerConfig` ScriptableObject: 요구 타입·수량, 이동속도, 코인보상 설정값
- `CustomerQueue`: 시작 위치+방향+간격으로 슬롯 위치 자동 계산, 등록/해제/슬롯 갱신 관리. OnDrawGizmos로 씬 미리보기
- `Customer` 상태머신: Idle→MovingToQueue→WaitingInQueue→Purchasing→Leaving
  - UpdateSlotPosition(): 큐 앞으로 당겨질 때 목적지 갱신
  - StartPurchasing() / CompletePurchase(): CashRegister 연동 인터페이스
- `CustomerObjectPool`: pre-instantiate, Get/Return으로 활성화·비활성화 재사용
- `CustomerSpawner`: 코루틴 타이머 기반, 대기열 만원 시 스킵. 스폰 위치 = 퇴장 복귀 위치
- `CashRegister` 수정: 자동 SellLoop 제거 → 손님 WaitingInQueue + 수량 충족 시 구매 루프 실행

**작업 스탭**

| 스탭 | 내용 | 파일 |
|---|---|---|
| S1 | CustomerConfig ScriptableObject + CustomerQueue 대기열 관리자 | CustomerConfig.cs, CustomerQueue.cs |
| S2 | Customer 상태머신 + CustomerObjectPool + CustomerSpawner | Customer.cs, CustomerObjectPool.cs, CustomerSpawner.cs |
| S3 | CashRegister 수정 — 손님 구매 처리 & 제품→손님 이동 연출 | CashRegister.cs |

**구현 후 추가 변경사항**

**BF: 코인 지급 타이밍 수정**
- 기존: 제품 1개 전달마다 코인 1개 지급
- 변경: 모든 제품 전달 완료 후 한번에 일괄 지급
- 이득: 구매 완료 시점 명확화, 중간에 끊기는 경우 코인 과지급 방지

**BF: 부분 구매 후 재고 대기**
- 기존: 요구량 전체 충족 시에만 구매 시작, 부족하면 아예 거래 안 함
- 변경: 재고가 1개라도 있으면 구매 시작, 부족분은 재고 채워질 때까지 대기 후 이어서 수령
- 이득: 플레이어가 제품을 조금씩 납품해도 손님이 기다리며 처리 가능

**추가: 스폰 위치 자동 계산 & 기즈모**
- CustomerQueue.GetSpawnPosition(extraSlots): 마지막 슬롯 기준 N칸 뒤 위치 자동 계산
- CustomerSpawner: spawnSlotOffset 인스펙터 설정(기본 5), 빨간 구체 기즈모로 스폰 위치 시각화

**추가: GridZoneLayout 3D 행렬 쌓기**
- 기존: 열(X) + 높이(Y) 2D 구조
- 변경: 열(X) × 행(Z) 레이어 채운 후 높이(Y)로 쌓는 3D 그리드
- rows/rowAxis/rowSpacing 인스펙터 설정 추가

**추가: 아이템 배치 회전 설정**
- ZoneStackLayout에 itemEulerAngles 필드 추가, ItemRotation 프로퍼티 노출
- DepositZone/PickupZone PlaceItem에서 layout.ItemRotation 적용
- 존마다 독립적으로 배치 회전값 설정 가능

**BF: PickupZone 스케일 영향 차단**
- 기존: SetParent(zone)로 아이템 배치 → 존 스케일 상속으로 오브젝트 변형
- 변경: SetParent 제거, position + rotation * localOffset으로 월드 위치 직접 계산
- 이득: 존 스케일과 무관하게 아이템 원본 크기 유지. DepositZone/PickupZone 동일 적용

**추가: StackPoint HoldOffset 트리거 연동**
- useHoldOffset/holdOffset: 아이템 보유 시 StackPoint 위치 오프셋
- holdOffsetTrigger: 다른 StackPoint의 Count를 기준으로 이동 (null이면 자기 자신 기준)
- Update 폴링으로 트리거 StackPoint count 변화 감지
- 활용: 광석 StackPoint를 트리거로 설정 → 광석 들면 코인 StackPoint가 뒤로 이동

---

## [006] MiningSystem — 채굴 구조 리팩토링 & 업그레이드

**요청 내용**
- 기존 OreObject 트리거 기반 채굴 → 플레이어 주도 OverlapSphere 방식으로 변경
- 채광 모드 / 이동 모드 분리: 채광존(MiningZone) 진입 시만 채굴 동작
- 업그레이드: 돈을 지불해 채굴 속도(interval↓), 범위(radius↑), 최대 적재량 증가
- UpgradeZone: 바닥 배치, 플레이어 진입 시 등 뒤 코인 오브젝트가 존으로 날아들며 요구량 채움

**구현 방법**
- OreObject: OnTriggerEnter/Stay/Exit 제거 → TryMine() 메서드로 외부 호출 수신, 히든+리스폰 처리
- MiningZone: 박스 콜라이더 IsTrigger, 플레이어 진입/이탈 → MiningController.SetMiningMode(bool)
- MiningController: miningInterval마다 플레이어 forward 앞에 Physics.OverlapSphere 실행, 가장 가까운 Active 광석에 TryMine() 호출
- MiningUpgradeData: ScriptableObject, 스테이지별 (radius, interval, cost, maxCarry) 테이블
- UpgradeZone: InteractionZone 상속, 진입 시 Coin을 ItemTransferAnimator로 존에 이동, 요구량 충족 시 업그레이드 완료 후 코인 소멸 → MiningController + StackSystem 용량에 스테이지 적용

**작업 스탭**

| 스탭 | 내용 | 파일 |
|---|---|---|
| S1 | OreObject 리팩토링 + MiningZone + MiningController (채굴 동작 통합 검증) | OreObject.cs, MiningZone.cs, MiningController.cs |
| S2 | MiningUpgradeData + UpgradeZone (업그레이드 구매 & 적용 검증) | MiningUpgradeData.cs, UpgradeZone.cs |

**구현 후 추가 변경사항**

**조정: 코인 1개당 10원**
- UpgradeZone에서 코인 1개 소멸 시 collectedCoins += 10 처리

**조정: 존별 단계 분리**
- 기존: 존 하나가 모든 업그레이드 단계를 순서대로 처리
- 변경: UpgradeZone에 stageIndex 필드 추가, 각 존이 담당 단계만 처리
- MiningController에 CurrentStage 프로퍼티 추가, ApplyUpgrade 완료 시 증가
- 진입 시 CurrentStage < stageIndex면 차단, CurrentStage > stageIndex면 이미 완료 처리

**조정: 업그레이드 완료 시 존 비활성화**
- ApplyUpgrade() 완료 후 gameObject.SetActive(false)로 존 자동 비활성화

---

## [007] WorkerSystem 인부 시스템

**요청 내용**
인부 3명을 해금 후 광산에서 자동으로 광물을 채굴하는 시스템.
- 해금: UpgradeZone처럼 바닥 존 위에 서서 코인 납부 → 완료 시 인부 3명 활성화
- 인부는 씬에 미리 배치, SetActive(true)로 활성화
- 이동: MoveTowards 직선 이동 (광산 평탄 지형)
- 채굴 방식: 광산 내 전범위에서 가장 가까운 Active + 미예약 광석으로 이동 → 도달 시 채굴
- 채굴 속도: 플레이어 기본 miningInterval (2f)
- 광석 예약: 인부가 타겟 지정 시 다른 인부 접근 불가 (OreReservation 시스템)
- 채굴 완료: 광석 오브젝트가 ItemTransferAnimator 포물선으로 DepositZone으로 날아감

**구현 방법**
- WorkerUnlockZone: UpgradeZone 구조 참고, 코인 납부 완료 시 workerPrefab을 spawnOrigin 기준 1열 중앙 정렬로 Instantiate
- OreReservation: static Dictionary로 광석 예약/해제 관리
- WorkerController: Idle→Moving→Mining 상태머신, 전범위 FindNearestOre(), MoveTowards 이동, 도달 시 채굴
- 채굴 완료 시 ItemTransferAnimator로 광석→DepositZone 포물선 이동 후 DepositZone.PlaceItem()

**작업 스탭**

| 스탭 | 내용 | 파일 |
|---|---|---|
| S1 | WorkerUnlockZone - 코인 납부 → 인부 3명 프리팹 스폰 (1열 중앙 정렬) | WorkerUnlockZone.cs |
| S2 | OreReservation + WorkerController - 광석 예약 + 이동/채굴 FSM | OreReservation.cs, WorkerController.cs |
| S3 | 채굴 완료 처리 - 광석 포물선 이동 → DepositZone.PlaceItem() | WorkerController.cs, OreObject.cs |

---

## [008] TransportWorkerSystem 운반 인부 시스템

**요청 내용**
가공기 PickupZone의 생산품을 계산대 DepositZone으로 운반하는 인부 시스템.
- 해금: WorkerUnlockZone 패턴 동일 (코인 납부 → 인부 스폰)
- 이동: MoveTowards 직선 이동
- 수집: PickupZone에서 아이템 하나씩 포물선으로 손에 쌓임, 상한 찰 때까지 대기
- 운반: 상한 차면 DepositZone으로 이동
- 납품: DepositZone에 아이템 하나씩 포물선으로 내려놓음
- 시각: 손에 StackPoint로 아이템 쌓임

**구현 방법**
- TransportWorkerUnlockZone: WorkerUnlockZone 구조 동일, Init(pickupZone, depositZone) 주입
- TransportWorkerController: MovingToPickup→Collecting→MovingToDeposit→Depositing FSM
  - Collecting: PickupZone.TakeTopItem() 순차 호출, ItemTransferAnimator로 손 StackPoint에 쌓음, 상한 전이면 대기
  - Depositing: 손 StackPoint에서 하나씩 꺼내 ItemTransferAnimator로 DepositZone에 전달
- 손 StackPoint: 인부 자식 오브젝트에 부착, 인스펙터에서 방향/간격 설정

**작업 스탭**

| 스탭 | 내용 | 파일 |
|---|---|---|
| S1 | TransportWorkerUnlockZone - 코인 납부 → 운반 인부 스폰 | TransportWorkerUnlockZone.cs |
| S2 | TransportWorkerController - 운반 FSM + 포물선 수집/납품 + 손 StackPoint | TransportWorkerController.cs |

---

## [009] UISystem HUD

**요청 내용**
4가지 HUD UI 구현. 모두 Screen Space Overlay + WorldToScreenPoint 좌표변환 방식.

1. 소지금액 표시: 코인 픽업 시 HUD에 소지금액(코인 수 × 10) 텍스트 갱신
2. 손님 요구 수치: 손님 머리 위 WorldToScreenPoint로 요구량 숫자 표시
3. 손님 충족 진행 바: 요구량 대비 현재 전달량을 초록 바(Slider)로 항상 표시
4. MAX 이미지: 광석 최대 적재 상태에서 채굴 시도 시 플레이어 몸통 위치에 이미지 표시 후 페이드아웃, 재시도 시 재표시

**구현 방법**
- UI 구조: Screen Space Overlay Canvas 단일 사용, 모든 HUD 요소 통합 관리
- 소지금액: StackSystem에 Coin 개수 변화 이벤트 추가, UIManager에서 구독 → TextMeshPro 갱신
- 손님 UI: CustomerHUDElement (숫자 텍스트 + Slider 바 세트), UIManager가 활성 손님 추적 + WorldToScreenPoint 위치 갱신
- 손님 진행도: CashRegister가 아이템 전달 시 Customer에 delivered 업데이트 → CustomerHUDElement가 반영
- MAX 이미지: MiningController에서 풀 상태 채굴 시도 시 이벤트 발생, UIManager가 WorldToScreenPoint(플레이어 torso) 위치로 이미지 표시 후 페이드아웃 코루틴

**작업 스탭**

| 스탭 | 내용 | 파일 |
|---|---|---|
| S1 | HUD Canvas + 소지금액 표시 | UIManager.cs 생성, StackSystem.cs 수정 |
| S2 | 손님 요구 수치 + 진행 바 | CustomerHUDElement.cs 생성, UIManager.cs 수정, Customer.cs 수정, CashRegister.cs 수정 |
| S3 | MAX 이미지 페이드아웃 + 위로 이동 | UIManager.cs 수정, MiningController.cs 수정 |

**구현 후 추가 변경사항**

**추가: MAX 이미지 상승 연출**
- 페이드아웃 진행도(t)에 비례해 스크린 Y 오프셋 증가 (maxRiseAmount, 기본 50px)
- LateUpdate에서 WorldToScreenPoint 결과에 maxScreenRiseOffset 가산
- maxRiseAmount 인스펙터 설정으로 조정 가능

---

## [BF] 카메라 대각선 시점 이후 조이스틱 방향 불일치

**요청 내용**
카메라 시점을 탑뷰(정면 하향)에서 대각선(isometric-like)으로 변경한 이후, 조이스틱 위쪽 방향이 월드 +Z와 고정 매핑되어 화면 방향과 어긋나는 문제 수정.

**구현 방법**
- `PlayerController.cs` 수정
- 기존: `moveDir = new Vector3(dir.x, 0f, dir.y)` (월드 고정 방향)
- 변경: `Camera.main`의 forward/right를 XZ 평면에 투영 후 정규화 → 조이스틱 입력과 조합
- `camForward = Camera.main.transform.forward`, y=0 정규화
- `camRight = Camera.main.transform.right`, y=0 정규화
- `moveDir = camRight * dir.x + camForward * dir.y`
- 카메라 각도가 바뀌어도 조이스틱 위쪽 = 화면 위쪽(카메라 forward 방향) 자동 유지
- 에디터 설정 불필요 (`Camera.main` 자동 참조)

---

## [011] 존 TMP 비용 표시

**요청 내용**
UpgradeZone, WorkerUnlockZone, TransportWorkerUnlockZone에 하위 자식으로 붙인 TextMeshPro 오브젝트에 현재 남은 요구 비용을 실시간으로 표시.

**구현 방법**
- 세 존 각각에 `[SerializeField] TMP_Text costText` 필드 추가 (Header: UI)
- `Start()`에서 초기 비용 표시
- 코인 납부 시마다 `(cost - collectedCoins)` 계산 후 텍스트 갱신
- 인스펙터에서 자식 TMP 오브젝트를 costText 슬롯에 연결

---

## [010] 플레이어 애니메이션 컨트롤러

**요청 내용**
플레이어 이동(Run/Idle)과 곡괭이질(Mine) 2가지 애니메이션 구동.

**구현 방법**
- `PlayerAnimationController.cs` 생성 (플레이어에 부착)
- Animator 파라미터: `Speed`(float) — 조이스틱 방향 크기, `IsMining`(bool) — MiningController.IsMiningMode 상태
- `MiningController`에 `IsMiningMode` 프로퍼티 추가
- Animator Controller 에디터 설정: Idle/Run/Mine 3개 State, Speed/IsMining 파라미터, Any State→Mine 트랜지션 (Can Transition To Self = false)

---

## [013] CarryInertia 관성 시스템

**요청 내용**
플레이어 이동 시 등/손에 들고 있는 오브젝트 스택 전체가 관성을 받는 느낌 구현.
- 이동 반대 방향으로 localPosition 오프셋 (밀리는 느낌)
- 이동 방향으로 localRotation 틸트 (기울어지는 느낌)
- 멈추면 원래 위치/회전으로 스프링 복귀
- StackPoint 단위로 적용

**구현 방법**
- `CarryInertia.cs` 신규 컴포넌트, StackPoint 오브젝트에 부착
- 플레이어 이동 velocity(위치 델타) 추적
- position 오프셋: 이동 반대 방향으로 localPosition 가산, maxOffset 클램프
- rotation 틸트: velocity 기준 forward/right 성분으로 localEulerAngles 조정
- Lerp(returnSpeed)로 매 프레임 복귀

**작업 스탭**

| 스탭 | 내용 | 파일 |
|---|---|---|
| S1 | CarryInertia - localRotation 기반 관성 기울기 (베이스 고정, 위로 갈수록 변위 증가) | CarryInertia.cs 생성 |

**구현 후 변경사항**

**버그픽스: position → rotation 기반으로 전면 교체**
- 원인 1: localPosition 전체 이동 방식은 스택 균일 이동 → 베이스 고정 불가
- 원인 2: CarryInertia가 localPosition 덮어씌워 StackPoint의 holdOffset 무시됨
- 해결: localRotation만 수정. StackPoint 피벗 기준 회전 → 베이스 고정, 위로 갈수록 변위 자연 증가. holdOffset(localPosition) 충돌 없음.

**버그: Humanoid 전환 후에도 애니메이션 미동작**

- 발생 상황: char01.fbx를 Generic → Humanoid로 변경 후 Apply했으나 모델이 여전히 애니메이션 동작 안 함
- 원인 분석:
  1. 기존 Player 프리팹을 FBX에서 꺼낸 뒤 **Unpack Completely** 했기 때문에 FBX와 연결이 완전히 끊어진 상태
  2. FBX 임포트 설정을 바꿔도 이미 언팩된 프리팹에는 변경사항이 전혀 반영되지 않음
  3. Humanoid 전환으로 생성된 Avatar가 Animator 컴포넌트에 연결되지 않은 상태
- 해결 과정:
  1. char01.fbx → Import Settings → Rig → Humanoid → Apply
  2. Configure... 에서 뼈 매핑 초록불 확인
  3. Player 오브젝트의 Animator 컴포넌트 → Avatar 필드에 `char01Avatar` 연결
  4. 정상 동작 확인

---

## [014] TutorialSystem 튜토리얼

**요청 내용**
튜토리얼 흐름 안내 시스템. 목적지 화살표 UI + 추적 화살표 UI + 카메라 연출 + 업그레이드 존 활성화 연동.

**구현 방법**
- 구조: `ITutorialStep` 인터페이스(OnEnter/OnExit/IsComplete) + `TutorialManager` (순서형 스텝 리스트)
- 목적지 화살표: Canvas Image 컴포넌트, 월드 좌표 → 스크린 좌표 변환, 화면 안에 있을 때 표시
- 추적 화살표: Canvas Image 컴포넌트, 목적지가 화면 밖으로 나가면 활성화, 플레이어 주변 회전
- 카메라 이동: 지정 위치로 부드럽게 이동 → N초 대기 → 플레이어 추적 복귀 (Cinemachine 미사용)
- 업그레이드 존: 기본 비활성화, 첫 돈 획득 이벤트 감지 시 특정 존 SetActive(true)

**작업 스탭**

| 스탭 | 내용 | 파일 |
|---|---|---|
| S1 | TutorialManager + ITutorialStep 뼈대 | TutorialManager.cs, ITutorialStep.cs |
| S2 | 목적지 화살표 UI | TutorialArrowUI.cs |
| S3 | 추적 화살표 UI | TutorialTrackingArrowUI.cs |
| S4 | 카메라 이동 컴포넌트 | TutorialCameraController.cs |
| S5 | 업그레이드 존 활성화 연동 | TutorialManager.cs 수정, UpgradeZone 관련 |
| S6 | 실제 튜토리얼 스텝 구성 | 튜토리얼 흐름 확정 후 진행 |

---

## [015] 구치소(Prison) 시스템

**요청 내용**
제품을 구매한 고객(죄수)이 구치소로 이동해 입소하는 시스템 구현.
- 정원 20명 (업그레이드로 증가), 정원 초과 시 줄 서서 대기
- 현재인원/전체인원 월드 스페이스 오브젝트로 표시
- 정원이 차면 TutorialCameraController 재사용해 카메라 이동 알림 + 업그레이드 존 활성화
- 업그레이드 시 건물 스케일 시각 효과

**구현 방법**
- S1: `Customer.cs` 상태 머신에 `MovingToPrison`, `WaitingForPrison`, `InPrison` 추가. `Prison.cs` 생성 — 수용 인원 관리, 입소 큐, 입소 처리.
- S2: `PrisonCapacityDisplay.cs` — 구치소 위 TextMeshPro 월드 스페이스로 현재/전체 인원 표시. Prison 이벤트 구독.
- S3: `PrisonUpgradeData.cs` (ScriptableObject, 단계별 비용·정원 인스펙터 할당, BuildingScale은 건물이 길어지기만 해서 제외) + `PrisonUpgradeZone.cs` (UpgradeZone 패턴 재활용, 초기 비활성).
- S4: `PrisonFullHandler.cs` — Prison.OnFull 구독, 최초 1회만 트리거. TutorialCameraController.TriggerMove() 호출 + PrisonUpgradeZone 활성화. debugTriggerCamera 인스펙터 토글로 카메라 위치 수동 확인 가능.

---

## [020] 리팩토링 - PickupZone 내 아이템 생성 시 즉시 이동 버그

**요청 내용**
플레이어가 PickupZone 안에 있는 상태에서 Processor가 아이템을 생성해도 플레이어 스택으로 이동하지 않는 버그 수정.

**구현 방법**
- `PickupZone.TransferSequence`: while 조건을 `IsPlayerInside`로 변경. 아이템 없거나 공간 없으면 0.1s 폴링 대기 후 재시도.

---

## [019] 리팩토링 - 광석 채굴 이동 연출

**요청 내용**
광석 채굴 시 등 뒤에 바로 생성되던 것을 광석 위치에서 스폰 후 StackPoint까지 날아오는 연출로 변경.

**구현 방법**
- `OreObject.cs`: OnPickedUp 이벤트 제거. TryMine() → 광석 위치에서 item Instantiate 후 반환
- `StackSystem.cs`: OnPickedUp 구독/Add(config) 제거
- `MiningController.cs`: TryMineNearest()에서 TryMine() 반환 item을 ItemTransferAnimator.Transfer로 StackPoint까지 이동. arcDuration/arcHeight 인스펙터 필드 추가

---

## [018] 리팩토링 - 맨 앞 손님만 말풍선 표시

**요청 내용**
손님 HUD(말풍선)를 대기열 맨 앞 손님에게만 표시.

**구현 방법**
- `Customer.cs`: ShowHUD 프로퍼티 추가 (기본값 false)
- `CustomerQueue.cs`: RefreshHUDVisibility() 추가 — index 0만 ShowHUD = true. TryEnqueue/Dequeue에서 호출
- `UIManager.cs`: LateUpdate에서 ShowHUD 체크 추가

---

## [022] 감옥 시스템 개선

**요청 내용**
- 감옥 대기열 기즈모 시각화
- 큐 사이즈 5 고정, 꽉 차면 결제 차단
- 죄수가 감옥 내부로 실제로 걸어 들어가는 연출

**구현 방법**
- `Prison.cs`: OnDrawGizmos 추가 (입구/슬롯/연결선), maxQueueSize=5, IsQueueFull 프로퍼티, insidePoint 필드, RequestEntry에서 큐 풀이면 거부
- `CashRegister.cs`: SellLoop에 !Prison.Instance.IsQueueFull 조건 추가
- `CustomerState`: EnteringPrison 상태 추가
- `Customer.cs`: EnterPrison(insidePosition) 시 EnteringPrison 상태로 내부 이동, 도착 시 InPrison 전환 (풀 반환 없음)

---

## [021] 운반 인부 애니메이션 컨트롤러

**요청 내용**
운반 인부에 애니메이션 추가. Speed 프로퍼티로 정지/이동 상태 제어.

**구현 방법**
- `TransportWorkerAnimController.cs` 신규 생성
- 매 프레임 현재 위치와 이전 위치 차이로 rawSpeed 계산
- SmoothDamp로 부드럽게 보간 후 Animator.SetFloat("Speed", currentSpeed) 전달
- Animator Controller에 Speed 파라미터 기반 Idle↔Move 트랜지션 구성

---

## [023] SoundManager 사운드 시스템

**요청 내용**
게임 전반 효과음을 관리하는 SoundManager 구현. BGM 없음, 효과음 8종.
1. 광석 캐는 소리 (플레이어 + 광부)
2. 광석/제품을 디포짓에 두는 소리
3. 픽업존에 아이템이 나타나는 소리
4. 픽업존에서 아이템 집는 소리
5. 죄수가 수갑 가져가는 소리
6. 돈 내는 소리
7. 플레이어가 돈 집는 소리
8. 업그레이드에 돈 넣는 소리

**구현 방법**
- AudioClip: Inspector 빈 슬롯 (나중에 연결)
- 동시 재생: AudioSource 풀 10개로 겹침 허용
- 3D 사운드: 월드 위치 기반 거리 감쇠
- `SoundType` enum으로 사운드 타입 정의
- `SoundManager.PlaySound(SoundType, Vector3)` 단일 API

**작업 스탭**

| 스탭 | 내용 | 파일 |
|---|---|---|
| S1 | SoundType enum + SoundManager 코어 (AudioSource 풀 + PlaySound) | SoundType.cs, SoundManager.cs |
| S2 | 채굴 사운드 연결 (플레이어 + 광부) | MiningController.cs, WorkerController.cs |
| S3 | DepositZone / PickupZone 사운드 연결 | DepositZone.cs, PickupZone.cs |
| S4 | Prison / CashRegister / 전체 업그레이드 존 사운드 연결 | Prison.cs, CashRegister.cs, UpgradeZone.cs, WorkerUnlockZone.cs, TransportWorkerUnlockZone.cs, PrisonUpgradeZone.cs |

---

## [017] 리팩토링 - 죄수 코인 발사 연출

**요청 내용**
죄수 구매 완료 시 코인이 즉시 배치되던 것을 죄수 위치에서 PickupZone으로 동시에 날아오는 연출로 교체.

**구현 방법**
- `PickupZone.cs`: RemainingCapacity, GetItemLocalPositionAt(index), GetItemWorldPositionAt(index), AddPlacedItem(item) 추가
- `CashRegister.cs`: 기존 즉시 PlaceItem 루프 → soldCount개 코인 죄수 위치에서 동시 Instantiate + ItemTransferAnimator.Transfer 동시 발사. onComplete에서 SetParent(null) + world position 확정 + AddPlacedItem 등록

---

## [016] 리팩토링 - DepositZone/PickupZone 이동 버그 수정

**요청 내용**
DepositZone/PickupZone에서 아이템 이동 중 존을 나오면 아이템이 공중에 멈추는 버그 수정.
마지막 프레임에서 회전/위치가 갑자기 적용되는 문제도 함께 수정.

**구현 방법**
- `ItemTransferAnimator.cs` DOTween 기반으로 교체: parent 설정 후 localPosition(arc 포함) + localRotation 동시 트윈
- `PickupZone.cs`: 이동 시작 시 아이템 parent = 플레이어 StackPoint → 플레이어 이동 중에도 목적지 추적
- `DepositZone.cs`: 이동 시작 시 아이템 parent = 존 transform → 이탈 시 고아 방지
- 이탈 시 DOTween Kill → 아이템이 parent 아래 멈춤 (고아 없음), 즉시 확정 처리
- UpgradeZone / WorkerUnlockZone / TransportWorkerUnlockZone / PrisonUpgradeZone: 동일 _activeTween 패턴 적용. collectedCoins 증가·Destroy·ApplyUpgrade 체크를 모두 onComplete 안으로 이동 → 이탈 시에도 납부 확정
- DepositZone onComplete에서 SetParent(null) + world position/rotation 직접 설정 → DepositZone scale 상속 및 위치 오차 제거

---

## [012] 업그레이드 존 다음 단계 연결

**요청 내용**
같은 위치에 여러 단계 UpgradeZone이 겹쳐있을 때, 현재 단계 완료 시 다음 단계 존을 활성화.

**구현 방법**
- `UpgradeZone.cs`에 `[SerializeField] GameObject nextZone` 필드 추가 (Header: Next Stage)
- `ApplyUpgrade()` 완료 시 `nextZone.SetActive(true)` 호출 후 자신 비활성화
- 인스펙터에서 1단계 존의 Next Zone 슬롯에 2단계 존 오브젝트 연결, 2단계는 미리 비활성화 상태로 설정
