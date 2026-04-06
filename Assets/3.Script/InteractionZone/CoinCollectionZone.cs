using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// 플레이어가 진입해 코인을 납부하면 특정 동작을 수행하는 존의 추상 베이스.
/// TransferSequence(코인 수집 루프), OnPlayerExit를 공통으로 처리한다.
///
/// 서브클래스 구현 의무:
///   - RequiredCoins : 납부 목표 금액
///   - IsDone        : 이미 완료된 상태인지
///   - OnCostMet()   : 목표 달성 시 수행할 동작
///
/// 선택적 오버라이드:
///   - OnSetupFromPlayer() : 플레이어 진입 시 추가 컴포넌트 획득
///   - CanActivate()       : 코루틴 시작 전 추가 조건 검사
/// </summary>
public abstract class CoinCollectionZone : InteractionZone
{
    [Header("Coin Transfer")]
    [SerializeField] private float transferDuration = 0.3f;
    [SerializeField] private float transferInterval = 0.1f;
    [SerializeField] private float arcHeight = 1f;

    [Header("UI")]
    [SerializeField] private TMP_Text costText;

    protected int collectedCoins = 0;
    protected StackSystem stackSystem;

    private Coroutine transferCoroutine;
    private Tween activeTween;

    protected abstract int RequiredCoins { get; }
    protected abstract bool IsDone { get; }
    protected abstract void OnCostMet();

    protected virtual void OnSetupFromPlayer(Collider player) { }
    protected virtual bool CanActivate() => true;

    private void Start()
    {
        UpdateCostText();
    }

    protected override void OnPlayerEnter(Collider player)
    {
        stackSystem = player.GetComponent<StackSystem>();
        OnSetupFromPlayer(player);

        if (IsDone) return;
        if (!CanActivate()) return;

        if (transferCoroutine != null) StopCoroutine(transferCoroutine);
        transferCoroutine = StartCoroutine(TransferSequence());
    }

    protected override void OnPlayerExit(Collider player)
    {
        activeTween?.Kill(complete: true);
        activeTween = null;

        if (transferCoroutine != null)
        {
            StopCoroutine(transferCoroutine);
            transferCoroutine = null;
        }
    }

    private IEnumerator TransferSequence()
    {
        while (IsPlayerInside && !IsDone)
        {
            if (stackSystem == null || !stackSystem.HasItem(StackItemType.Coin))
            {
                yield return new WaitForSeconds(transferInterval);
                continue;
            }

            var coin = stackSystem.TakeItem(StackItemType.Coin);
            if (coin == null) { yield return new WaitForSeconds(transferInterval); continue; }

            activeTween = ItemTransferAnimator.Transfer(
                coin, transform, Vector3.zero, Quaternion.identity, transferDuration, arcHeight,
                onComplete: () =>
                {
                    Destroy(coin);
                    collectedCoins += 10;
                    SoundManager.Instance?.PlaySound(SoundType.UpgradePay, transform.position);
                    UpdateCostText();
                    Debug.Log($"[{GetType().Name}] 코인 {collectedCoins}/{RequiredCoins} 납부");
                    if (collectedCoins >= RequiredCoins)
                        OnCostMet();
                }
            );

            yield return activeTween.WaitForCompletion();
            activeTween = null;

            yield return new WaitForSeconds(transferInterval);
        }

        transferCoroutine = null;
    }

    protected void UpdateCostText()
    {
        if (costText != null)
            costText.text = (RequiredCoins - collectedCoins).ToString();
    }
}
