using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 인부 AI. 광산 내 전범위에서 가장 가까운 미예약 광석을 찾아 이동 후 채굴한다.
/// Idle → Moving → Mining → Idle 루프.
/// </summary>
public class WorkerController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float miningInterval = 2f;
    [SerializeField] private float stopDistance = 0.8f;

    [Header("Deposit")]
    [SerializeField] private float arcDuration = 0.5f;
    [SerializeField] private float arcHeight = 1.5f;

    private DepositZone depositZone;

    private enum State { Idle, Moving, Mining }
    private State state = State.Idle;

    private OreObject target;
    private OreObject[] allOres;

    public void Init(DepositZone targetDepositZone)
    {
        depositZone = targetDepositZone;
        allOres = FindObjectsOfType<OreObject>();
        Debug.Log($"[Worker] 초기화 - 광석 {allOres.Length}개 감지");
    }

    private void Update()
    {
        switch (state)
        {
            case State.Idle:
                TryFindTarget();
                break;
            case State.Moving:
                MoveToTarget();
                break;
        }
    }

    private void TryFindTarget()
    {
        var nearest = FindNearestOre();
        if (nearest == null) return;
        if (!OreReservation.TryReserve(nearest, this)) return;

        target = nearest;
        state = State.Moving;
        Debug.Log($"[Worker] 타겟 지정 → {target.gameObject.name}");
    }

    private void MoveToTarget()
    {
        // 타겟이 채굴되거나 없어진 경우 초기화
        if (target == null || target.State != OreObject.OreState.Active)
        {
            OreReservation.Release(target);
            target = null;
            state = State.Idle;
            return;
        }

        Vector3 targetPos = target.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        Vector3 dir = (targetPos - transform.position);
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir.normalized);

        if (Vector3.Distance(transform.position, targetPos) <= stopDistance)
        {
            state = State.Mining;
            StartCoroutine(MineRoutine());
        }
    }

    private IEnumerator MineRoutine()
    {
        Debug.Log($"[Worker] 채굴 시작 - {target?.gameObject.name} ({miningInterval}s)");
        yield return new WaitForSeconds(miningInterval);

        if (target != null && target.State == OreObject.OreState.Active)
            OnMineComplete();
        else
            ReleasAndIdle();
    }

    protected virtual void OnMineComplete()
    {
        StartCoroutine(TransferRoutine());
    }

    private IEnumerator TransferRoutine()
    {
        var config = target.StackConfig;
        var item = Instantiate(config.itemPrefab, target.transform.position, Quaternion.identity);

        target.MineByWorker(); // 광석 즉시 히든 + 리스폰 타이머 시작

        if (depositZone != null)
        {
            yield return ItemTransferAnimator.Transfer(
                item, depositZone.transform, Vector3.zero, Quaternion.identity, arcDuration, arcHeight
            ).WaitForCompletion();

            if (depositZone.HasSpace)
                depositZone.PlaceItem(item);
            else
                Destroy(item);

            Debug.Log($"[Worker] 광석 DepositZone 전달 완료");
        }
        else
        {
            Destroy(item);
            Debug.LogWarning("[Worker] DepositZone 미연결 - 아이템 제거");
        }

        ReleasAndIdle();
    }

    protected void ReleasAndIdle()
    {
        OreReservation.Release(target);
        target = null;
        state = State.Idle;
    }

    private OreObject FindNearestOre()
    {
        OreObject nearest = null;
        float minDist = float.MaxValue;

        foreach (var ore in allOres)
        {
            if (ore == null) continue;
            if (ore.State != OreObject.OreState.Active) continue;
            if (OreReservation.IsReserved(ore)) continue;

            float dist = Vector3.Distance(transform.position, ore.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = ore;
            }
        }

        return nearest;
    }
}
