using UnityEngine;

public enum CustomerState
{
    Idle,
    MovingToQueue,
    WaitingInQueue,
    Purchasing,
    Leaving,
    MovingToPrison,      // 구매 완료 후 구치소 입구로 이동 중
    WaitingForPrison,    // 구치소 앞 대기열에서 대기 중
    InPrison             // 입소 완료
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
    public int Delivered { get; private set; }
    public bool ShowHUD { get; set; } = false;

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

        Delivered = 0;
        State = CustomerState.MovingToQueue;
        UIManager.Instance?.RegisterCustomerHUD(this);
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

            case CustomerState.MovingToPrison:
                MoveTowards(targetPosition);
                if (HasArrived(targetPosition))
                    Prison.Instance.RequestEntry(this);
                break;

            case CustomerState.WaitingForPrison:
                if (!HasArrived(targetPosition))
                    MoveTowards(targetPosition);
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

    public void AddDelivered()
    {
        Delivered++;
    }

    /// <summary>CashRegister에서 구매 완료 시 호출. 구치소가 있으면 구치소로, 없으면 퇴장한다.</summary>
    public void CompletePurchase()
    {
        UIManager.Instance?.UnregisterCustomerHUD(this);
        queue.Dequeue();
        transform.SetParent(null);

        if (Prison.Instance != null)
        {
            targetPosition = Prison.Instance.EntrancePosition;
            State = CustomerState.MovingToPrison;
            Debug.Log($"[Customer] 구매 완료 → MovingToPrison");
        }
        else
        {
            State = CustomerState.Leaving;
            Debug.Log($"[Customer] 구매 완료 → Leaving (구치소 없음)");
        }
    }

    /// <summary>Prison이 대기열 자리를 배정할 때 호출.</summary>
    public void JoinPrisonQueue(Vector3 waitPosition)
    {
        targetPosition = waitPosition;
        State = CustomerState.WaitingForPrison;
        Debug.Log($"[Customer] 구치소 대기열 합류");
    }

    /// <summary>대기열 앞으로 당겨질 때 호출.</summary>
    public void UpdatePrisonQueuePosition(Vector3 newPosition)
    {
        targetPosition = newPosition;
    }

    /// <summary>Prison이 입소를 승인할 때 호출.</summary>
    public void EnterPrison()
    {
        State = CustomerState.InPrison;
        Debug.Log($"[Customer] 구치소 입소 완료 → 풀 반환");
        pool.Return(this);
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
