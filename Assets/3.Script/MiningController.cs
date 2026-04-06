using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 플레이어에 부착. 채광 모드일 때 주기적으로 OverlapSphere를 실행해
/// 가장 가까운 Active 광석에 TryMine()을 호출한다.
/// </summary>
public class MiningController : MonoBehaviour
{
    [Header("Mining Settings")]
    [SerializeField] private float miningInterval = 2f;
    [SerializeField] private float miningRadius = 1.5f;
    [SerializeField] private float sphereOffset = 1f;

    [Header("Pickup Animation")]
    [SerializeField] private float arcDuration = 0.4f;
    [SerializeField] private float arcHeight = 1.5f;

    [Header("References")]
    [SerializeField] private StackPoint oreStackPoint;

    public event Action OnMaxReached;

    private StackSystem stackSystem;
    private bool isMiningMode;
    private Coroutine miningCoroutine;

    public float MiningInterval => miningInterval;
    public float MiningRadius => miningRadius;
    public int CurrentStage { get; private set; } = 0;
    public bool IsMiningMode => isMiningMode;

    private void Awake()
    {
        stackSystem = GetComponent<StackSystem>();
    }

    public void SetMiningMode(bool active)
    {
        isMiningMode = active;

        if (active)
        {
            if (miningCoroutine != null) StopCoroutine(miningCoroutine);
            miningCoroutine = StartCoroutine(MineLoop());
        }
        else
        {
            if (miningCoroutine != null)
            {
                StopCoroutine(miningCoroutine);
                miningCoroutine = null;
            }
        }
    }

    public void ApplyUpgrade(float interval, float radius, int maxCarry)
    {
        miningInterval = interval;
        miningRadius = radius;

        if (oreStackPoint != null)
            oreStackPoint.SetCapacity(maxCarry);

        CurrentStage++;

        if (isMiningMode)
            SetMiningMode(true); // 루프 재시작으로 새 interval 적용
    }

    private IEnumerator MineLoop()
    {
        while (isMiningMode)
        {
            yield return new WaitForSeconds(miningInterval);
            TryMineNearest();
        }
    }

    private void TryMineNearest()
    {
        Vector3 center = transform.position + transform.forward * sphereOffset;
        var hits = Physics.OverlapSphere(center, miningRadius);

        OreObject closest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            var ore = hit.GetComponent<OreObject>();
            if (ore == null || ore.State != OreObject.OreState.Active) continue;

            float dist = Vector3.Distance(transform.position, ore.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = ore;
            }
        }

        if (closest != null)
        {
            if (stackSystem != null && !stackSystem.HasSpace(closest.StackConfig.itemType))
            {
                OnMaxReached?.Invoke();
                Debug.Log("[Mining] 최대 적재량 도달 - 채굴 불가");
            }
            else
            {
                Debug.Log($"[Mining] 채굴 시도 - {closest.gameObject.name} (거리: {minDist:F2})");
                var itemType = closest.StackConfig.itemType;
                var item = closest.TryMine();
                if (item != null)
                {
                    var sp = stackSystem.GetStackPoint(itemType);
                    if (sp != null)
                    {
                        var capturedItem = item;
                        ItemTransferAnimator.Transfer(
                            capturedItem,
                            sp.transform,
                            sp.GetNextItemLocalPosition(),
                            Quaternion.identity,
                            arcDuration,
                            arcHeight,
                            onComplete: () => stackSystem.ReceiveItem(capturedItem, itemType)
                        );
                    }
                }
            }
        }
        else
        {
            Debug.Log("[Mining] 범위 내 광석 없음");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = transform.position + transform.forward * sphereOffset;
        Gizmos.DrawWireSphere(center, miningRadius);
    }
}
