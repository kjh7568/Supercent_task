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
