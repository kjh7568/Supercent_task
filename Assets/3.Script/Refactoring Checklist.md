# 리팩토링 체크리스트

> 기준 문서: `Refactoring guideline.md`  
> 분석 대상: `Assets/3.Script` 전체 (.cs 46개)  
> 작성일: 2026-04-06

---

## 우선순위 기준
- **P1** : 구조적 문제 (중복·책임 위반) — 버그 위험 또는 확장 불가
- **P2** : 품질 개선 (함수 분리·네이밍) — 가독성·유지보수성 저하
- **P3** : 최적화 (성능·이벤트 구조) — 현재 문제 없으나 개선 여지

---

## 섹션 0. MonoBehaviour 제거 가능 후보

> 가이드라인 §8·§9 원칙: 순수 로직 클래스는 MonoBehaviour에 의존하지 않도록 설계

| 번호 | 파일 | 판단 | 근거 | 완료 |
|------|------|------|------|------|
| M-1 | `TutorialSteps.cs` (6개 클래스) | ✅ 이미 완료 | MonoBehaviour 상속 없음. 순수 C# 클래스로 구현됨 | [x] |
| M-2 | `GameTutorial.cs` | ❌ 제거 불가 | `[SerializeField]`로 씬 레퍼런스 연결 + `Start()` 사용. MonoBehaviour 필수 | [x] |
| M-3 | `OreReservation.cs` | ✅ 이미 완료 | 이미 `static class`로 구현됨 | [x] |
| M-4 | `ItemTransferAnimator.cs` | ✅ 이미 완료 | 이미 `static class`로 구현됨 | [x] |
| M-5 | `ZoneStackLayout.cs` (추상 클래스) | ❌ 제거 불가 | `[SerializeField]` + 자식 클래스(Linear/Grid)가 인스펙터 노출 컴포넌트로 사용됨 | [x] |
| M-6 | `TransportWorkerAnimController.cs` | ❌ 제거 불가 | `Update()`에서 `transform.position` 매 프레임 접근. Unity 라이프사이클 필수 | [x] |
| M-7 | `PlayerAnimationController.cs` | ❌ 제거 불가 | `Update()`, `Awake()`, `[SerializeField]` 모두 사용. Unity 라이프사이클 필수 | [x] |

---

## 섹션 1. 중복 로직 제거 (가이드라인 §2, §3, §5)

### P1-1. TransferSequence 코루틴 중복 — 4개 클래스 ✅

동일한 "코인 수집 후 스폰" 흐름이 4곳에 복사됨.

- [x] `UpgradeZone.cs` — `TransferSequence()` (~35줄)
- [x] `PrisonUpgradeZone.cs` — `TransferSequence()` (~35줄)
- [x] `WorkerUnlockZone.cs` — `TransferSequence()` (~35줄)
- [x] `TransportWorkerUnlockZone.cs` — `TransferSequence()` (~35줄)

**목표**: 공통 추상 클래스 `CoinCollectionZone` 추출. 4개 클래스는 스폰 콜백만 오버라이드.

**어떻게 구현했는지**: `InteractionZone`을 상속하는 `CoinCollectionZone` 추상 클래스 신규 생성. 공통 필드(`transferDuration`, `transferInterval`, `arcHeight`, `costText`)와 공통 메서드(`OnPlayerEnter/Exit`, `TransferSequence`, `UpdateCostText`, `Start`)를 베이스로 이동. 서브클래스는 추상 멤버 3개(`RequiredCoins`, `IsDone`, `OnCostMet()`)만 구현. UpgradeZone처럼 추가 조건이 필요한 경우 `OnSetupFromPlayer()`, `CanActivate()` 선택 오버라이드로 처리.

**결과**: 4개 클래스 각각 ~120줄 → ~45줄. 중복 코드 ~100줄 제거. 향후 코인 수집 로직 수정 시 `CoinCollectionZone` 한 곳만 수정하면 됨.

---

### P1-2. 광석 탐색 로직 중복 ✅

- [x] `MiningController.TryMineNearest()` 내 광석 탐색 로직
- [x] `WorkerController.FindNearestOre()` — 동일 구조

**목표**: `OreReservation` 또는 별도 `OreSearchUtil` 정적 유틸로 추출.

**어떻게 구현했는지**: `OreReservation`에 두 개의 정적 메서드 추가. `FindNearest(Vector3, Collider[])` — 플레이어용, 예약 무시. `FindNearestUnreserved(Vector3, OreObject[])` — 인부용, 예약된 광석 제외. 두 메서드는 호출 대상 컬렉션 타입과 예약 체크 여부만 다르고 핵심 루프는 동일한 구조.

**결과**: `MiningController.TryMineNearest()` 탐색 루프 12줄 → 1줄. `WorkerController.FindNearestOre()` 17줄 → 1줄(식 본문 메서드). 탐색 조건 변경 시 `OreReservation` 한 곳만 수정.

---

## 섹션 2. 단일 책임 원칙 위반 (가이드라인 §1, §6, §9)

### P1-3. UIManager — 3가지 책임 혼재 ⏸ 보류

`UIManager.cs`가 다음 3가지를 동시에 담당:
1. 소지 금액 표시
2. 손님 HUD 등록/해제/갱신
3. MAX 이미지 표시

- [ ] ~~`MoneyDisplay` 클래스 분리 (소지 금액)~~
- [ ] ~~`CustomerHUDManager` 클래스 분리 (HUD 등록/갱신/위치)~~
- [ ] ~~`MaxImageController` 클래스 분리 (MAX 표시 페이드)~~
- [ ] ~~`UIManager`는 세 클래스를 조합하는 얇은 진입점으로 유지하거나 제거~~

**보류 사유**: 전체 189줄로 관리 가능한 크기. 주석(`// ── Money ──` 등)으로 섹션이 이미 명확히 구분됨. 각 기능의 외부 호출 지점이 각각 1곳뿐이라 재사용 이점 없음. 분리 시 씬 컴포넌트 3개 연결 + 싱글톤 3개로 오히려 복잡도 증가. 가이드라인 §2 통합 기준("사용 지점이 한 곳뿐", "추상화하면 오히려 복잡해진다") 해당.

---

## 섹션 3. 함수 길이 초과 (가이드라인 §7: 30줄 이상 분리)

| 번호 | 파일 | 함수명 | 현재 길이 | 분리 방향 | 완료 |
|------|------|--------|----------|----------|------|
| F-1 | `CashRegister.cs` | `ProcessPurchase()` | ~72줄 | 코인 발사 블록을 `SpawnCoins()` 메서드로 추출 (yield 없는 독립 단계) | [x] |
| F-2 | `MiningController.cs` | `TryMineNearest()` | ~44줄 | 채굴 실행 블록을 `ExecuteMine(ore)` 메서드로 추출 (탐색·판단 vs 실행 분리) | [x] |

**목표**: 각 함수 내 두 단계의 책임을 이름이 있는 메서드로 분리.

**어떻게 구현했는지**:
- F-1: `ProcessPurchase`에서 yield 없는 코인 발사 블록(30줄)을 `SpawnCoins(customer, soldCount)`로 추출. 호출부는 한 줄로 교체.
- F-2: `TryMineNearest`의 else 블록을 `ExecuteMine(OreObject ore)`로 추출. 기존 중첩 if-else를 guard clause 패턴으로 평탄화.

**결과**: `ProcessPurchase` 72줄 → 38줄. `TryMineNearest` 44줄 → 17줄 + `ExecuteMine` 20줄. 각 메서드가 단일 책임을 가짐.

---

## 섹션 4. 이벤트 구독 해제 누락 (가이드라인 §11)

| 번호 | 파일 | 문제 | 개선 방향 | 완료 |
|------|------|------|----------|------|
| E-1 | `PrisonCapacityDisplay.cs` | `OnDestroy()`에서 `Prison.Instance` null 체크 보강 | `if (Prison.Instance != null)` 명시적 null 가드로 방어 | [x] |
| E-2 | `TutorialUpgradeZoneActivator.cs` | `Start()`에서 구독 (권장: `Awake()`) | 라이프사이클 일관성을 위해 `Awake()`로 이동 | [x] |

**목표**: 이벤트 구독 해제 시점의 null 안전성 확보 및 구독 등록 라이프사이클 일관성 통일.

**어떻게 구현했는지**:
- E-1: `OnDestroy()`에 `if (Prison.Instance != null)` null 가드 명시. C#에서 `?.` 연산자는 이벤트 `-=`에 사용 불가(컴파일 에러)이므로 if 형태 유지.
- E-2: `TutorialUpgradeZoneActivator`의 구독 등록 메서드를 `Start()` → `Awake()`로 변경. `stackSystem`은 `[SerializeField]` 참조라 Awake 시점에 이미 할당되어 있어 안전.

**결과**:
- E-1: `OnDestroy`에 null 가드 추가. 씬 전환 시 Prison 싱글톤이 먼저 파괴되는 경우 NullReference 방지.
- E-2: 씬 로드 초기 단계(Awake)에서 구독 등록이 완료되어 다른 컴포넌트의 Start 실행 전 발생하는 코인 이벤트도 누락 없이 수신 가능.

---

## 섹션 5. TutorialSteps 구조 통합 ⏸ 보류 (가이드라인 §2, §3)

- [ ] 6개 `Step_*` 클래스가 동일한 패턴(타겟 저장 → OnEnter에서 arrow 표시 → OnExit에서 arrow 숨김) 반복
- **목표**: `TutorialStepBase` 추상 클래스 또는 제네릭 기반 클래스 도입으로 공통 코드 제거

**보류 사유**: 각 클래스가 이미 12줄 내외로 짧고, `OnEnter`/`OnExit`가 1줄짜리라 베이스 추출 시 체감 이득이 적음. `Step_CameraMove`는 패턴이 달라 베이스 상속 대상 제외 필요 — 분리 시 일관성 오히려 저하.

---

## 진행 현황

| 섹션 | 항목 수   | 완료                |
|------|--------|-------------------|
| §0 MonoBehaviour 제거 | 7      | 7 (3 완료 / 4 제거불가) |
| §1 중복 로직 | 2      | 2                 |
| §2 단일 책임 | 1      | 1 (보류)            |
| §3 함수 길이 | 2      | 2                 |
| §4 이벤트 구독 | 2      | 2                 |
| §5 Tutorial 통합 | 1      | 1 (보류)            |
| **합계** | **15** | **13 완료 / 2 보류**  |
