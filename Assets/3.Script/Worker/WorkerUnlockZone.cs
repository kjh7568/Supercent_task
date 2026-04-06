using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어가 진입해 코인을 납부하면 채굴 인부를 스폰하는 해금 존.
/// </summary>
public class WorkerUnlockZone : CoinCollectionZone
{
    [Header("Unlock Settings")]
    [SerializeField] private int cost = 100;
    [SerializeField] private GameObject workerPrefab;
    [SerializeField] private Transform spawnOrigin;
    [SerializeField] private int workerCount = 3;
    [SerializeField] private float spawnSpacing = 1.2f;
    [SerializeField] private DepositZone workerDepositZone;

    private bool isUnlocked = false;
    private readonly List<GameObject> spawnedWorkers = new();

    protected override int RequiredCoins => cost;
    protected override bool IsDone => isUnlocked;
    protected override void OnCostMet() => Unlock();

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
            worker.GetComponent<WorkerController>()?.Init(workerDepositZone);
            spawnedWorkers.Add(worker);
        }

        Debug.Log($"[WorkerUnlockZone] 해금 완료 - 인부 {workerCount}명 스폰");
        gameObject.SetActive(false);
    }
}
