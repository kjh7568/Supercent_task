using System.Collections;
using UnityEngine;

/// <summary>
/// 계산대 — DepositZone(가공품 투입) → 즉시 판매 → PickupZone(코인 출력).
/// 가공 시간 없이 sellInterval 간격으로 연속 처리.
/// </summary>
public class CashRegister : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DepositZone depositZone;
    [SerializeField] private PickupZone pickupZone;

    [Header("Settings")]
    [SerializeField] private StackItemConfig outputConfig;
    [SerializeField] private float sellInterval = 0.15f;

    private void Start()
    {
        if (depositZone == null || pickupZone == null || outputConfig == null)
        {
            Debug.LogError($"[CashRegister] 필수 레퍼런스가 없습니다. 인스펙터를 확인하세요. - {gameObject.name}");
            return;
        }

        StartCoroutine(SellLoop());
    }

    private IEnumerator SellLoop()
    {
        while (true)
        {
            if (depositZone.Count > 0 && pickupZone.HasSpace)
            {
                var product = depositZone.TakeTopItem();
                Destroy(product);

                var coin = Instantiate(outputConfig.itemPrefab);
                pickupZone.PlaceItem(coin);

                Debug.Log($"[CashRegister] 판매 완료 → Coin 추가 ({pickupZone.Count}) - {gameObject.name}");

                yield return new WaitForSeconds(sellInterval);
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
