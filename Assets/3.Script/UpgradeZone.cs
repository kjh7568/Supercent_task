using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 플레이어가 진입하면 등 뒤 코인을 존으로 날려 업그레이드 비용을 채운다.
/// 요구량 충족 시 MiningController와 StackPoint 용량을 업그레이드.
/// </summary>
public class UpgradeZone : InteractionZone
{
    [Header("Upgrade Settings")]
    [SerializeField] private MiningUpgradeData upgradeData;
    [SerializeField] private int stageIndex = 0;
    [SerializeField] private float transferDuration = 0.3f;
    [SerializeField] private float transferInterval = 0.1f;
    [SerializeField] private float arcHeight = 1f;

    [Header("Next Stage")]
    [SerializeField] private GameObject nextZone;

    [Header("UI")]
    [SerializeField] private TMP_Text costText;

    private int collectedCoins = 0;
    private Coroutine transferCoroutine;
    private MiningController miningController;
    private StackSystem stackSystem;

    private bool IsAlreadyDone => miningController != null && miningController.CurrentStage > stageIndex;
    private int CurrentCost => upgradeData.stages[stageIndex].cost;

    private void Start()
    {
        UpdateCostText();
    }

    private void UpdateCostText()
    {
        if (costText != null)
            costText.text = (CurrentCost - collectedCoins).ToString();
    }

    protected override void OnPlayerEnter(Collider player)
    {
        miningController = player.GetComponent<MiningController>();
        stackSystem = player.GetComponent<StackSystem>();

        if (IsAlreadyDone)
        {
            Debug.Log($"[UpgradeZone] 스테이지 {stageIndex + 1} 이미 완료");
            return;
        }

        if (miningController.CurrentStage < stageIndex)
        {
            Debug.Log($"[UpgradeZone] 스테이지 {stageIndex} 먼저 완료 필요");
            return;
        }

        if (transferCoroutine != null) StopCoroutine(transferCoroutine);
        transferCoroutine = StartCoroutine(TransferSequence());
    }

    protected override void OnPlayerExit(Collider player)
    {
        if (transferCoroutine != null)
        {
            StopCoroutine(transferCoroutine);
            transferCoroutine = null;
        }
    }

    private IEnumerator TransferSequence()
    {
        while (IsPlayerInside && !IsAlreadyDone)
        {
            if (stackSystem == null || !stackSystem.HasItem(StackItemType.Coin))
            {
                yield return new WaitForSeconds(transferInterval);
                continue;
            }

            var coin = stackSystem.TakeItem(StackItemType.Coin);
            if (coin == null) { yield return new WaitForSeconds(transferInterval); continue; }

            yield return StartCoroutine(ItemTransferAnimator.Transfer(
                coin, transform.position, transferDuration, arcHeight,
                onComplete: null
            ));

            Destroy(coin);
            collectedCoins += 10;
            UpdateCostText();

            Debug.Log($"[UpgradeZone] 코인 {collectedCoins}/{CurrentCost} 납부");

            if (collectedCoins >= CurrentCost)
                ApplyUpgrade();

            yield return new WaitForSeconds(transferInterval);
        }

        transferCoroutine = null;
    }

    private void ApplyUpgrade()
    {
        var stage = upgradeData.stages[stageIndex];

        if (miningController != null)
            miningController.ApplyUpgrade(stage.miningInterval, stage.miningRadius, stage.maxCarry);

        Debug.Log($"[Upgrade] 스테이지 {stageIndex + 1} 완료 - radius: {stage.miningRadius}, interval: {stage.miningInterval}, maxCarry: {stage.maxCarry}");

        collectedCoins = 0;

        if (nextZone != null)
            nextZone.SetActive(true);

        gameObject.SetActive(false);
    }
}
