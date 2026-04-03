using System.Collections;
using UnityEngine;

/// <summary>
/// 운반 인부 AI.
/// PickupZone에서 아이템을 손에 하나씩 쌓은 뒤 DepositZone으로 운반한다.
/// MovingToPickup → Collecting → MovingToDeposit → Depositing → 반복
/// </summary>
public class TransportWorkerController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int carryLimit = 5;
    [SerializeField] private float collectInterval = 0.2f;
    [SerializeField] private float depositInterval = 0.2f;
    [SerializeField] private float arcDuration = 0.4f;
    [SerializeField] private float arcHeight = 1.2f;
    [SerializeField] private float stopDistance = 0.8f;

    [Header("Hand Stack")]
    [SerializeField] private StackPoint handStackPoint;

    private enum State { MovingToPickup, Collecting, MovingToDeposit, Depositing }
    private State state = State.MovingToPickup;

    private PickupZone pickupZone;
    private DepositZone depositZone;

    public void Init(PickupZone targetPickupZone, DepositZone targetDepositZone)
    {
        pickupZone = targetPickupZone;
        depositZone = targetDepositZone;
        state = State.MovingToPickup;
        Debug.Log("[TransportWorker] 초기화 완료");
    }

    private void Update()
    {
        if (pickupZone == null || depositZone == null) return;

        switch (state)
        {
            case State.MovingToPickup:
                if (MoveTo(pickupZone.transform.position))
                {
                    state = State.Collecting;
                    StartCoroutine(CollectRoutine());
                }
                break;

            case State.MovingToDeposit:
                if (MoveTo(depositZone.transform.position))
                {
                    state = State.Depositing;
                    StartCoroutine(DepositRoutine());
                }
                break;
        }
    }

    /// <summary>목적지로 이동. 도착 시 true 반환.</summary>
    private bool MoveTo(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        Vector3 dir = target - transform.position;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir.normalized);

        return Vector3.Distance(transform.position, target) <= stopDistance;
    }

    private IEnumerator CollectRoutine()
    {
        Debug.Log("[TransportWorker] 수집 시작");

        while (handStackPoint.Count < carryLimit)
        {
            // 픽업존에 아이템 없으면 대기
            while (pickupZone.Count == 0)
                yield return new WaitForSeconds(0.1f);

            var item = pickupZone.TakeTopItem();
            if (item == null) { yield return new WaitForSeconds(0.1f); continue; }

            Vector3 targetPos = handStackPoint.GetNextItemWorldPosition();

            yield return StartCoroutine(ItemTransferAnimator.Transfer(
                item, targetPos, arcDuration, arcHeight
            ));

            handStackPoint.AddExistingItem(item);

            Debug.Log($"[TransportWorker] 수집 {handStackPoint.Count}/{carryLimit}");

            yield return new WaitForSeconds(collectInterval);
        }

        Debug.Log("[TransportWorker] 상한 도달 → 디포짓존 이동");
        state = State.MovingToDeposit;
    }

    private IEnumerator DepositRoutine()
    {
        Debug.Log("[TransportWorker] 납품 시작");

        while (handStackPoint.Count > 0)
        {
            if (!depositZone.HasSpace)
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            var item = handStackPoint.RemoveTopItem();
            if (item == null) break;

            yield return StartCoroutine(ItemTransferAnimator.Transfer(
                item, depositZone.transform.position, arcDuration, arcHeight
            ));

            depositZone.PlaceItem(item);

            Debug.Log($"[TransportWorker] 납품 {handStackPoint.Count}개 남음");

            yield return new WaitForSeconds(depositInterval);
        }

        Debug.Log("[TransportWorker] 납품 완료 → 픽업존 이동");
        state = State.MovingToPickup;
    }
}
