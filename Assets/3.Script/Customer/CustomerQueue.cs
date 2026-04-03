using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 계산대 앞 대기열 관리자.
/// 시작 위치·방향·간격으로 슬롯 위치를 자동 계산한다.
/// </summary>
public class CustomerQueue : MonoBehaviour
{
    [Header("Queue Settings")]
    [SerializeField] private int maxSize = 5;
    [SerializeField] private float slotSpacing = 1.2f;
    [SerializeField] private Vector3 queueDirection = Vector3.back; // 계산대 반대 방향

    private readonly List<Customer> queue = new();

    public int Count => queue.Count;
    public bool IsFull => queue.Count >= maxSize;
    public bool HasFrontCustomer => queue.Count > 0;
    public Customer FrontCustomer => queue.Count > 0 ? queue[0] : null;

    private Vector3 GetSlotPosition(int index)
        => transform.position + queueDirection.normalized * (slotSpacing * index);

    /// <summary>대기열 마지막 슬롯 기준으로 extraSlots만큼 더 뒤쪽 위치 반환. 스폰/퇴장 위치 계산용.</summary>
    public Vector3 GetSpawnPosition(int extraSlots)
        => GetSlotPosition(maxSize - 1 + extraSlots);

    /// <summary>
    /// 손님을 대기열 맨 뒤에 등록하고 배정된 슬롯 위치를 반환.
    /// 슬롯이 꽉 찼으면 false 반환.
    /// </summary>
    public bool TryEnqueue(Customer customer, out Vector3 slotPosition)
    {
        slotPosition = Vector3.zero;

        if (IsFull)
        {
            Debug.Log($"[CustomerQueue] 대기열이 꽉 찼습니다 ({queue.Count}/{maxSize})");
            return false;
        }

        queue.Add(customer);
        slotPosition = GetSlotPosition(queue.Count - 1);

        Debug.Log($"[CustomerQueue] 손님 등록 ({queue.Count}/{maxSize}) 슬롯: {queue.Count - 1}");
        return true;
    }

    /// <summary>
    /// 맨 앞 손님을 대기열에서 제거하고 나머지 손님의 이동 목적지를 갱신.
    /// </summary>
    public void Dequeue()
    {
        if (queue.Count == 0) return;

        queue.RemoveAt(0);
        Debug.Log($"[CustomerQueue] 손님 퇴장, 남은 대기: {queue.Count}명");

        for (int i = 0; i < queue.Count; i++)
            queue[i].UpdateSlotPosition(GetSlotPosition(i));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < maxSize; i++)
        {
            Gizmos.DrawWireSphere(GetSlotPosition(i), 0.3f);
        }
    }
}
