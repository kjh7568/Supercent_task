using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 구치소. 구매 완료된 죄수의 입소·대기·정원 관리를 담당한다.
/// </summary>
public class Prison : MonoBehaviour
{
    public static Prison Instance { get; private set; }

    [Header("설정")]
    [SerializeField] private int baseCapacity = 20;
    [SerializeField] private Transform entrancePoint;
    [SerializeField] private Transform insidePoint;
    [SerializeField] private float queueSpacing = 1.2f;
    [SerializeField] private Vector3 queueDirection = Vector3.forward;
    [SerializeField] private int maxQueueSize = 5;

    private int capacity;
    private int currentCount;
    private readonly List<Customer> waitingQueue = new List<Customer>();

    public int Capacity => capacity;
    public int CurrentCount => currentCount;
    public bool IsFull => currentCount >= capacity;
    public bool IsQueueFull => waitingQueue.Count >= maxQueueSize;
    public Vector3 EntrancePosition => entrancePoint != null ? entrancePoint.position : transform.position;

    /// <summary>현재 인원 또는 정원이 바뀔 때 발행.</summary>
    public event Action OnCountChanged;

    /// <summary>정원이 처음 가득 찼을 때 발행.</summary>
    public event Action OnFull;

    private void Awake()
    {
        Instance = this;
        capacity = baseCapacity;
    }

    /// <summary>구매 완료된 고객이 입소를 요청한다.</summary>
    public void RequestEntry(Customer customer)
    {
        if (!IsFull)
        {
            Accept(customer);
        }
        else if (!IsQueueFull)
        {
            waitingQueue.Add(customer);
            customer.JoinPrisonQueue(GetQueuePosition(waitingQueue.Count - 1));
            Debug.Log($"[Prison] 정원 초과 → 대기열 {waitingQueue.Count}/{maxQueueSize}번째");
        }
        else
        {
            Debug.LogWarning($"[Prison] 대기열 가득 참({maxQueueSize}명) — 입소 거부");
        }
    }

    /// <summary>업그레이드로 정원을 늘릴 때 호출.</summary>
    public void ExpandCapacity(int newCapacity)
    {
        capacity = newCapacity;
        OnCountChanged?.Invoke();
        ProcessWaitingQueue();
        Debug.Log($"[Prison] 정원 확장 → {capacity}명");
    }

    private void Accept(Customer customer)
    {
        currentCount++;
        SoundManager.Instance?.PlaySound(SoundType.Handcuff, customer.transform.position);
        Vector3 inside = insidePoint != null ? insidePoint.position : transform.position;
        customer.EnterPrison(inside);
        OnCountChanged?.Invoke();
        Debug.Log($"[Prison] 입소 완료 {currentCount}/{capacity}");

        if (IsFull)
        {
            OnFull?.Invoke();
            Debug.Log($"[Prison] 정원 가득 참! {capacity}명");
        }
    }

    private void ProcessWaitingQueue()
    {
        while (waitingQueue.Count > 0 && !IsFull)
        {
            var next = waitingQueue[0];
            waitingQueue.RemoveAt(0);
            RefreshQueuePositions();
            Accept(next);
        }
    }

    private void RefreshQueuePositions()
    {
        for (int i = 0; i < waitingQueue.Count; i++)
            waitingQueue[i].UpdatePrisonQueuePosition(GetQueuePosition(i));
    }

    public Vector3 GetQueuePosition(int index)
    {
        return EntrancePosition + queueDirection * (queueSpacing * (index + 1));
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector3 entrance = entrancePoint != null ? entrancePoint.position : transform.position;
        int previewCount = maxQueueSize;

        // 입구 마커
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(entrance, 0.2f);

        // 대기 슬롯
        for (int i = 0; i < previewCount; i++)
        {
            Vector3 pos = entrance + queueDirection * (queueSpacing * (i + 1));
            bool occupied = i < waitingQueue.Count;

            Gizmos.color = occupied ? Color.red : new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawSphere(pos, 0.2f);

            // 슬롯 간 연결선
            Vector3 prev = i == 0 ? entrance : entrance + queueDirection * (queueSpacing * i);
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawLine(prev, pos);
        }
    }
#endif
}
