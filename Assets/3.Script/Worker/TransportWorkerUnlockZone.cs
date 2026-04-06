using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// 플레이어가 진입해 코인을 납부하면 운반 인부를 스폰하는 해금 존.
/// 스폰 시 TransportWorkerController에 PickupZone/DepositZone 참조를 주입한다.
/// </summary>
public class TransportWorkerUnlockZone : InteractionZone
{
    [Header("Unlock Settings")]
    [SerializeField] private int cost = 100;
    [SerializeField] private GameObject workerPrefab;
    [SerializeField] private Transform spawnOrigin;
    [SerializeField] private int workerCount = 3;
    [SerializeField] private float spawnSpacing = 1.2f;
    [SerializeField] private float transferDuration = 0.3f;
    [SerializeField] private float transferInterval = 0.1f;
    [SerializeField] private float arcHeight = 1f;

    [Header("Worker References")]
    [SerializeField] private PickupZone workerPickupZone;
    [SerializeField] private DepositZone workerDepositZone;

    [Header("UI")]
    [SerializeField] private TMP_Text costText;

    private int collectedCoins = 0;
    private bool isUnlocked = false;
    private Coroutine transferCoroutine;
    private Tween _activeTween;
    private StackSystem stackSystem;
    private readonly List<GameObject> spawnedWorkers = new();

    private void Start()
    {
        UpdateCostText();
    }

    private void UpdateCostText()
    {
        if (costText != null)
            costText.text = (cost - collectedCoins).ToString();
    }

    protected override void OnPlayerEnter(Collider player)
    {
        if (isUnlocked)
        {
            Debug.Log("[TransportWorkerUnlockZone] 이미 해금 완료");
            return;
        }

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
        while (IsPlayerInside && !isUnlocked)
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
                    Debug.Log($"[TransportWorkerUnlockZone] 코인 {collectedCoins}/{cost} 납부");
                    if (collectedCoins >= cost)
                        Unlock();
                }
            );

            yield return _activeTween.WaitForCompletion();
            _activeTween = null;

            yield return new WaitForSeconds(transferInterval);
        }

        transferCoroutine = null;
    }

    private void Unlock()
    {
        isUnlocked = true;

        Vector3 origin = spawnOrigin != null ? spawnOrigin.position : transform.position;
        Vector3 right = spawnOrigin != null ? spawnOrigin.right : Vector3.right;

        float totalWidth = (workerCount - 1) * spawnSpacing;
        Vector3 startPos = origin - right * (totalWidth / 2f);

        for (int i = 0; i < workerCount; i++)
        {
            Vector3 pos = startPos + right * (spawnSpacing * i);
            var worker = Instantiate(workerPrefab, pos, spawnOrigin != null ? spawnOrigin.rotation : Quaternion.identity);
            worker.GetComponent<TransportWorkerController>()?.Init(workerPickupZone, workerDepositZone);
            spawnedWorkers.Add(worker);
        }

        Debug.Log($"[TransportWorkerUnlockZone] 해금 완료 - 운반 인부 {workerCount}명 스폰");

        gameObject.SetActive(false);
    }
}
