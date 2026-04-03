using UnityEngine;

public enum CustomerState
{
    Idle,
    MovingToQueue,
    WaitingInQueue,
    Purchasing,
    Leaving
}

/// <summary>
/// 손님 개체. 상태머신 기반으로 이동·대기·구매·퇴장을 처리한다.
/// </summary>
public class Customer : MonoBehaviour
{
    [SerializeField] private float arrivalThreshold = 0.05f;

    private CustomerConfig config;
    private CustomerQueue queue;
    private CustomerObjectPool pool;
    private Vector3 targetPosition;
    private Vector3 leavePosition;

    public CustomerState State { get; private set; } = CustomerState.Idle;
    public CustomerConfig Config => config;

    public void Initialize(
        CustomerConfig customerConfig,
        CustomerQueue customerQueue,
        CustomerObjectPool customerPool,
        Vector3 slotPosition,
        Vector3 exitPosition)
    {
        config = customerConfig;
        queue = customerQueue;
        pool = customerPool;
        targetPosition = slotPosition;
        leavePosition = exitPosition;

        State = CustomerState.MovingToQueue;
        Debug.Log($"[Customer] 스폰 → MovingToQueue (슬롯: {slotPosition})");
    }

    private void Update()
    {
        switch (State)
        {
            case CustomerState.MovingToQueue:
                MoveTowards(targetPosition);
                if (HasArrived(targetPosition))
                {
                    State = CustomerState.WaitingInQueue;
                    Debug.Log($"[Customer] 슬롯 도착 → WaitingInQueue");
                }
                break;

            case CustomerState.Leaving:
                MoveTowards(leavePosition);
                if (HasArrived(leavePosition))
                {
                    Debug.Log($"[Customer] 퇴장 완료 → 풀 반환");
                    pool.Return(this);
                }
                break;
        }
    }

    /// <summary>큐 앞으로 당겨질 때 이동 목적지 갱신.</summary>
    public void UpdateSlotPosition(Vector3 position)
    {
        targetPosition = position;
        if (State == CustomerState.WaitingInQueue)
            State = CustomerState.MovingToQueue;

        Debug.Log($"[Customer] 슬롯 갱신 → MovingToQueue ({position})");
    }

    /// <summary>CashRegister에서 구매 시작 시 호출.</summary>
    public void StartPurchasing()
    {
        State = CustomerState.Purchasing;
        Debug.Log($"[Customer] 구매 시작 → Purchasing");
    }

    /// <summary>CashRegister에서 구매 완료 시 호출. 손님이 퇴장한다.</summary>
    public void CompletePurchase()
    {
        queue.Dequeue();
        State = CustomerState.Leaving;
        transform.SetParent(null);
        Debug.Log($"[Customer] 구매 완료 → Leaving");
    }

    private void MoveTowards(Vector3 target)
    {
        Vector3 flatTarget = new Vector3(target.x, transform.position.y, target.z);
        transform.position = Vector3.MoveTowards(transform.position, flatTarget, config.moveSpeed * Time.deltaTime);
    }

    private bool HasArrived(Vector3 target)
    {
        Vector3 flatTarget = new Vector3(target.x, transform.position.y, target.z);
        return Vector3.Distance(transform.position, flatTarget) <= arrivalThreshold;
    }
}
