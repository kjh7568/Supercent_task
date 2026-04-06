using UnityEngine;

/// <summary>
/// 구치소 정원 확장 업그레이드 존.
/// 플레이어가 진입해 코인을 납부하면 Prison 정원을 늘린다.
/// 정원이 가득 찼을 때 Prison이 활성화한다 (초기 비활성).
/// </summary>
public class PrisonUpgradeZone : CoinCollectionZone
{
    [Header("Upgrade Settings")]
    [SerializeField] private PrisonUpgradeData upgradeData;
    [SerializeField] private int stageIndex = 0;

    [Header("Next Stage")]
    [SerializeField] private GameObject nextZone;

    private bool isDone = false;

    protected override int RequiredCoins => upgradeData.stages[stageIndex].cost;
    protected override bool IsDone => isDone;
    protected override void OnCostMet() => ApplyUpgrade();

    private void ApplyUpgrade()
    {
        isDone = true;
        var stage = upgradeData.stages[stageIndex];

        Prison.Instance.ExpandCapacity(stage.newCapacity);
        Debug.Log($"[PrisonUpgradeZone] 업그레이드 완료 - 정원: {stage.newCapacity}");

        collectedCoins = 0;

        if (nextZone != null)
            nextZone.SetActive(true);

        gameObject.SetActive(false);
    }
}
