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

### P1-3. UIManager — 3가지 책임 혼재

`UIManager.cs`가 다음 3가지를 동시에 담당:
1. 소지 금액 표시
2. 손님 HUD 등록/해제/갱신
3. MAX 이미지 표시

- [ ] `MoneyDisplay` 클래스 분리 (소지 금액)
- [ ] `CustomerHUDManager` 클래스 분리 (HUD 등록/갱신/위치)
- [ ] `MaxImageController` 클래스 분리 (MAX 표시 페이드)
- [ ] `UIManager`는 세 클래스를 조합하는 얇은 진입점으로 유지하거나 제거

---

### P1-4. Customer.cs — 초기화 매개변수 과다

- [ ] `Customer.Initialize(config, queue, cashRegister, prison, soundManager)` — 5개 매개변수  
  **목표**: `CustomerInitData` 데이터 클래스로 묶어 매개변수 1개로 압축 (가이드라인 §7: 4개 이상 → 객체화)

---

## 섹션 3. 함수 길이 초과 (가이드라인 §7: 30줄 이상 분리)

| 번호 | 파일 | 함수명 | 현재 길이 | 분리 방향 | 완료 |
|------|------|--------|----------|----------|------|
| F-1 | `CashRegister.cs` | `ProcessPurchase()` | ~72줄 | 제품 전달 로직 / 코인 발사 로직 분리 | [ ] |
| F-2 | `MiningController.cs` | `TryMineNearest()` | ~59줄 | `FindBestTarget()` / `ExecuteMine()` 분리 | [ ] |
| F-3 | `PickupZone.cs` | `TransferSequence()` | ~55줄 | 이동 조건 검사 / 단일 아이템 이동 분리 | [ ] |
| F-4 | `DepositZone.cs` | `TransferSequence()` | ~44줄 | 아이템 꺼내기 / 포물선 전송 분리 | [ ] |
| F-5 | `TutorialCameraController.cs` | `MoveRoutine()` | ~35줄 | 이동 단계 / 복귀 단계 분리 | [ ] |

---

## 섹션 4. 조건문 중첩 3단계 이상 (가이드라인 §7: 3단계 → 구조 개선)

| 번호 | 파일 | 위치 | 문제 | 개선 방향 | 완료 |
|------|------|------|------|----------|------|
| C-1 | `PickupZone.cs` | `TransferSequence()` | `while > if > yield` 3단계 중첩 | 조기 return / 보조 함수 추출로 평탄화 | [ ] |
| C-2 | `Prison.cs` | `RequestEntry()` | 다중 `if-else` 중첩 체인 | Guard clause 패턴으로 평탄화 | [ ] |

---

## 섹션 5. Update / LateUpdate 최적화 (가이드라인 §9, §12)

| 번호 | 파일 | 위치 | 문제 | 개선 방향 | 완료 |
|------|------|------|------|----------|------|
| U-1 | `UIManager.cs` | `LateUpdate()` | 매 프레임 모든 손님 HUD 위치 순회 | 손님 이동 이벤트 기반으로 갱신 | [ ] |
| U-2 | `PrisonFullHandler.cs` | `Update()` | `debugTriggerCamera` 플래그 매 프레임 확인 | 디버그 코드 `#if UNITY_EDITOR`로 격리 | [ ] |
| U-3 | `TutorialCameraController.cs` | `Update()` | `debugTrigger` 플래그 매 프레임 확인 | 디버그 코드 `#if UNITY_EDITOR`로 격리 | [ ] |
| U-4 | `Customer.cs` | `Update()` | switch 문으로 전체 상태 매 프레임 평가 | 상태 진입 시 1회 실행되는 구조로 개선 고려 | [ ] |

---

## 섹션 6. 이벤트 구독 해제 누락 (가이드라인 §11)

| 번호 | 파일 | 문제 | 개선 방향 | 완료 |
|------|------|------|----------|------|
| E-1 | `PrisonCapacityDisplay.cs` | `OnDestroy()`에서 `Prison.Instance` null 체크 없이 구독 해제 | `Prison.Instance?.OnCountChanged -= ...` 형태로 방어 | [ ] |
| E-2 | `TutorialUpgradeZoneActivator.cs` | `Start()`에서 구독 (권장: `Awake()`) | 라이프사이클 일관성을 위해 `Awake()`로 이동 | [ ] |

---

## 섹션 7. 데이터-로직 혼합 (가이드라인 §10)

| 번호 | 파일 | 문제 | 개선 방향 | 완료 |
|------|------|------|----------|------|
| D-1 | `MiningUpgradeData.cs` | 업그레이드 수치 데이터가 MonoBehaviour 안에 존재 | `ScriptableObject` 또는 순수 데이터 클래스(`[Serializable]`)로 전환 | [ ] |
| D-2 | `PrisonUpgradeData.cs` | 동일 | 동일 | [ ] |

---

## 섹션 8. 네이밍 규칙 (가이드라인 §14)

| 번호 | 파일 | 대상 | 문제 | 수정안 | 완료 |
|------|------|------|------|--------|------|
| N-1 | `MiningController.cs` | 지역 변수 `itemType` | 명사지만 맥락 불분명 | `acquiredItemType` 또는 `minedItemType` | [ ] |
| N-2 | 전체 | bool 변수 검토 | `is/has/can` 접두어 없는 bool 변수 여부 전체 확인 필요 | `isRunning`, `hasTarget`, `canMine` 형태로 통일 | [ ] |

---

## 섹션 9. TutorialSteps 구조 통합 (가이드라인 §2, §3)

- [ ] 6개 `Step_*` 클래스가 동일한 패턴(타겟 저장 → OnEnter에서 arrow 표시 → OnExit에서 arrow 숨김) 반복
- **목표**: `TutorialStepBase` 추상 클래스 또는 제네릭 기반 클래스 도입으로 공통 코드 제거

---

## 진행 현황

| 섹션 | 항목 수 | 완료 |
|------|--------|------|
| §0 MonoBehaviour 제거 | 7 | 7 (3 완료 / 4 제거불가) |
| §1 중복 로직 | 6 | 6 |
| §2 단일 책임 | 4 | 0 |
| §3 함수 길이 | 5 | 0 |
| §4 조건문 중첩 | 2 | 0 |
| §5 Update 최적화 | 4 | 0 |
| §6 이벤트 구독 | 2 | 0 |
| §7 데이터-로직 | 2 | 0 |
| §8 네이밍 | 2 | 0 |
| §9 Tutorial 통합 | 1 | 0 |
| **합계** | **35** | **0** |
