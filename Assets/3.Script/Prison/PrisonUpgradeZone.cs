using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// 구치소 정원 확장 업그레이드 존.
/// 플레이어가 진입해 코인을 납부하면 Prison 정원을 늘리고 건물 스케일을 변경한다.
/// 정원이 가득 찼을 때 Prison이 활성화한다 (초기 비활성).
/// </summary>
public class PrisonUpgradeZone : InteractionZone
{
    [Header("Upgrade Settings")]
    [SerializeField] private PrisonUpgradeData upgradeData;
    [SerializeField] private int stageIndex = 0;
    [SerializeField] private float transferDuration = 0.3f;
    [SerializeField] private float transferInterval = 0.1f;
    [SerializeField] private float arcHeight = 1f;

    [Header("Next Stage")]
    [SerializeField] private GameObject nextZone;

    [Header("UI")]
    [SerializeField] private TMP_Text costText;

    private int collectedCoins = 0;
    private bool isDone = false;
    private Coroutine transferCoroutine;
    private Tween _activeTween;
    private StackSystem stackSystem;

    private int CurrentCost => upgradeData.stages[stageIndex].cost;

    private void Start()
    {
        UpdateCostText();
    }

    protected override void OnPlayerEnter(Collider player)
    {
        if (isDone) return;

        stackSystem = player.GetComponent<StackSystem>();

        if (transferCoroutine != null) StopCoroutine(transferCoroutine);
        transferCoroutine = StartCoroutine(TransferSequence());
    }

    protected override void OnPlayerExit(Collider player)
    {
        _activeTween?.Kill(complete: true);
        _activeTween = null;

        if (transferCoroutine != null)
        {
            StopCoroutine(transferCoroutine);
            transferCoroutine = null;
        }
    }

    private IEnumerator TransferSequence()
    {
        while (IsPlayerInside && !isDone)
        {
            if (stackSystem == null || !stackSystem.HasItem(StackItemType.Coin))
            {
                yield return new WaitForSeconds(transferInterval);
                continue;
            }

            var coin = stackSystem.TakeItem(StackItemType.Coin);
            if (coin == null) { yield return new WaitForSeconds(transferInterval); continue; }

            _activeTween = ItemTransferAnimator.Transfer(
                coin, transform, Vector3.zero, Quaternion.identity, transferDuration, arcHeight,
                onComplete: () =>
                {
                    Destroy(coin);
                    collectedCoins += 10;
                    SoundManager.Instance?.PlaySound(SoundType.UpgradePay, transform.position);
                    UpdateCostText();
                    Debug.Log($"[PrisonUpgradeZone] 코인 {collectedCoins}/{CurrentCost} 납부");
                    if (collectedCoins >= CurrentCost)
                        ApplyUpgrade();
                }
            );

            yield return _activeTween.WaitForCompletion();
            _activeTween = null;

            yield return new WaitForSeconds(transferInterval);
        }

        transferCoroutine = null;
    }

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

    private void UpdateCostText()
    {
        if (costText != null)
            costText.text = (CurrentCost - collectedCoins).ToString();
    }
}
