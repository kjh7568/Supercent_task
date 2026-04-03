using System.Collections;
using UnityEngine;

/// <summary>
/// 계산대 — 손님이 WaitingInQueue 상태일 때 DepositZone 제품을 손님에게 하나씩 이동.
/// 모든 제품 전달 완료 후 코인을 한번에 PickupZone에 추가.
/// </summary>
public class CashRegister : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DepositZone depositZone;
    [SerializeField] private PickupZone pickupZone;
    [SerializeField] private CustomerQueue customerQueue;

    [Header("Settings")]
    [SerializeField] private StackItemConfig coinConfig;
    [SerializeField] private float transferDuration = 0.3f;
    [SerializeField] private float transferInterval = 0.15f;
    [SerializeField] private float arcHeight = 1f;

    private bool isBusy = false;

    private void Start()
    {
        if (depositZone == null || pickupZone == null || customerQueue == null || coinConfig == null)
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
            if (!isBusy && customerQueue.HasFrontCustomer)
            {
                var customer = customerQueue.FrontCustomer;

                if (customer.State == CustomerState.WaitingInQueue &&
                    depositZone.Count >= customer.Config.demandAmount)
                {
                    StartCoroutine(ProcessPurchase(customer));
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator ProcessPurchase(Customer customer)
    {
        isBusy = true;
        customer.StartPurchasing();

        Debug.Log($"[CashRegister] 구매 시작 — 요구량: {customer.Config.demandAmount}개");

        int soldCount = 0;
        for (int i = 0; i < customer.Config.demandAmount; i++)
        {
            var product = depositZone.TakeTopItem();
            if (product == null) break;

            Vector3 customerPos = customer.transform.position + Vector3.up * 0.5f;

            yield return StartCoroutine(ItemTransferAnimator.Transfer(
                product, customerPos, transferDuration, arcHeight
            ));

            if (product != null)
                Destroy(product);

            soldCount++;
            yield return new WaitForSeconds(transferInterval);
        }

        // 모든 제품 전달 완료 후 코인 한번에 지급
        for (int i = 0; i < soldCount; i++)
        {
            if (!pickupZone.HasSpace) break;
            var coin = Instantiate(coinConfig.itemPrefab);
            pickupZone.PlaceItem(coin);
        }
        Debug.Log($"[CashRegister] 구매 완료 — 코인 {soldCount}개 지급");
        customer.CompletePurchase();
        isBusy = false;
    }
}
