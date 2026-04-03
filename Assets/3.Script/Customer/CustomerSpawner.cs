using System.Collections;
using UnityEngine;

/// <summary>
/// 타이머 기반으로 손님을 스폰한다.
/// 스폰/퇴장 위치 = 대기열 마지막 슬롯 기준 spawnSlotOffset칸 뒤.
/// </summary>
public class CustomerSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CustomerObjectPool pool;
    [SerializeField] private CustomerQueue queue;
    [SerializeField] private CustomerConfig config;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int spawnSlotOffset = 5;

    private void Start()
    {
        if (pool == null || queue == null || config == null)
        {
            Debug.LogError($"[CustomerSpawner] 필수 레퍼런스가 없습니다. 인스펙터를 확인하세요.");
            return;
        }

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            TrySpawn();
        }
    }

    private void TrySpawn()
    {
        if (queue.IsFull)
        {
            Debug.Log($"[CustomerSpawner] 대기열 만원, 스폰 스킵");
            return;
        }

        Vector3 spawnPos = queue.GetSpawnPosition(spawnSlotOffset);

        var customer = pool.Get();
        customer.transform.position = spawnPos;
        customer.transform.SetParent(null);

        if (!queue.TryEnqueue(customer, out var slotPos))
        {
            pool.Return(customer);
            return;
        }

        customer.Initialize(config, queue, pool, slotPos, spawnPos);
        Debug.Log($"[CustomerSpawner] 손님 스폰 ({spawnPos}) → 슬롯 {slotPos}");
    }

    private void OnDrawGizmos()
    {
        if (queue == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(queue.GetSpawnPosition(spawnSlotOffset), 0.4f);
    }
}
