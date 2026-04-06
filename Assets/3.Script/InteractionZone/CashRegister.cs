using System.Collections;
using DG.Tweening;
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
                    depositZone.Count > 0 && !depositZone.IsTransferring &&
                    (Prison.Instance == null || !Prison.Instance.IsQueueFull))
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


        int soldCount = 0;
        int remaining = customer.Config.demandAmount;

        while (remaining > 0)
        {
            // 제품이 채워질 때까지 대기
            if (depositZone.Count == 0)
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            var product = depositZone.TakeTopItem();
            if (product == null) continue;

            yield return ItemTransferAnimator.Transfer(
                product, customer.transform, Vector3.up * 0.5f, Quaternion.identity, transferDuration, arcHeight
            ).WaitForCompletion();

            if (product != null)
                Destroy(product);

            customer.AddDelivered();
            soldCount++;
            remaining--;
            yield return new WaitForSeconds(transferInterval);
        }

        // 모든 제품 전달 완료 후 코인 동시 발사
        int coinsToSend = Mathf.Min(soldCount, pickupZone.RemainingCapacity);
        int baseIndex = pickupZone.Count;
        Vector3 spawnPos = customer.transform.position + Vector3.up * 0.5f;

        if (coinsToSend > 0)
            SoundManager.Instance?.PlaySound(SoundType.PayMoney, spawnPos);

        for (int i = 0; i < coinsToSend; i++)
        {
            int slotIndex = baseIndex + i;
            var coin = Instantiate(coinConfig.itemPrefab, spawnPos, Quaternion.identity);
            Vector3 worldTargetPos = pickupZone.GetItemWorldPositionAt(slotIndex);
            Vector3 localTargetPos = pickupZone.GetItemLocalPositionAt(slotIndex);
            var capturedCoin = coin;

            ItemTransferAnimator.Transfer(
                capturedCoin,
                pickupZone.transform,
                localTargetPos,
                Quaternion.identity,
                transferDuration,
                arcHeight,
                onComplete: () =>
                {
                    if (capturedCoin != null)
                    {
                        capturedCoin.transform.SetParent(null);
                        capturedCoin.transform.position = worldTargetPos;
                        pickupZone.AddPlacedItem(capturedCoin);
                    }
                }
            );
        }

        customer.CompletePurchase();
        isBusy = false;
    }
}
