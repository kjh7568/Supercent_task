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
    [SerializeField] private float queueSpacing = 1.2f;
    [SerializeField] private Vector3 queueDirection = Vector3.back;

    private int capacity;
    private int currentCount;
    private readonly List<Customer> waitingQueue = new List<Customer>();

    public int Capacity => capacity;
    public int CurrentCount => currentCount;
    public bool IsFull => currentCount >= capacity;
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
        else
        {
            waitingQueue.Add(customer);
            customer.JoinPrisonQueue(GetQueuePosition(waitingQueue.Count - 1));
            Debug.Log($"[Prison] 정원 초과 → 대기열 {waitingQueue.Count}번째");
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
        customer.EnterPrison();
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
}
