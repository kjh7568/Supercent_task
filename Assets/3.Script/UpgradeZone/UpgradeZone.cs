using DG.Tweening;
using UnityEngine;

/// <summary>
/// 플레이어가 진입하면 등 뒤 코인을 존으로 날려 업그레이드 비용을 채운다.
/// 요구량 충족 시 MiningController와 StackPoint 용량을 업그레이드.
/// </summary>
public class UpgradeZone : CoinCollectionZone
{
    [Header("Upgrade Settings")]
    [SerializeField] private MiningUpgradeData upgradeData;
    [SerializeField] private int stageIndex = 0;

    [Header("Next Stage")]
    [SerializeField] private GameObject nextZone;

    private MiningController miningController;

    protected override int RequiredCoins => upgradeData.stages[stageIndex].cost;
    protected override bool IsDone => miningController != null && miningController.CurrentStage > stageIndex;

    protected override void OnSetupFromPlayer(Collider player)
    {
        miningController = player.GetComponent<MiningController>();
    }

    protected override bool CanActivate()
    {
        if (miningController == null) return false;
        if (miningController.CurrentStage < stageIndex)
        {
            Debug.Log($"[UpgradeZone] 스테이지 {stageIndex} 먼저 완료 필요");
            return false;
        }
        return true;
    }

    protected override void OnCostMet() => ApplyUpgrade();

    private void ApplyUpgrade()
    {
        var stage = upgradeData.stages[stageIndex];

        miningController?.ApplyUpgrade(stage.miningInterval, stage.miningRadius, stage.maxCarry);
        Debug.Log($"[Upgrade] 스테이지 {stageIndex + 1} 완료 - radius: {stage.miningRadius}, interval: {stage.miningInterval}, maxCarry: {stage.maxCarry}");

        collectedCoins = 0;

        if (nextZone != null)
            DOVirtual.DelayedCall(1f, () => nextZone.SetActive(true));

        gameObject.SetActive(false);
    }
}
