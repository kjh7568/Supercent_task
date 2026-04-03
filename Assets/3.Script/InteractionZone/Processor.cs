using System.Collections;
using UnityEngine;

/// <summary>
/// 가공기 — DepositZone(광석 투입) → 가공 시간 → PickupZone(가공품 출력).
/// 1:1 비율로 처리하며, DepositZone에 아이템이 있고 PickupZone에 공간이 있는 동안 연속 가공.
/// </summary>
public class Processor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DepositZone depositZone;
    [SerializeField] private PickupZone pickupZone;

    [Header("Settings")]
    [SerializeField] private StackItemConfig outputConfig;
    [SerializeField] private float processingTime = 2f;

    private void Start()
    {
        if (depositZone == null || pickupZone == null || outputConfig == null)
        {
            Debug.LogError($"[Processor] 필수 레퍼런스가 없습니다. 인스펙터를 확인하세요. - {gameObject.name}");
            return;
        }

        StartCoroutine(ProcessingLoop());
    }

    private IEnumerator ProcessingLoop()
    {
        while (true)
        {
            if (depositZone.Count > 0 && pickupZone.HasSpace)
            {
                var ore = depositZone.TakeTopItem();
                Destroy(ore);

                Debug.Log($"[Processor] 가공 시작 (대기: {processingTime}s) - {gameObject.name}");
                yield return new WaitForSeconds(processingTime);

                var product = Instantiate(outputConfig.itemPrefab);
                pickupZone.PlaceItem(product);

                Debug.Log($"[Processor] 가공 완료 → PickupZone ({pickupZone.Count}/{pickupZone.Count + (pickupZone.HasSpace ? 1 : 0)}) - {gameObject.name}");
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
