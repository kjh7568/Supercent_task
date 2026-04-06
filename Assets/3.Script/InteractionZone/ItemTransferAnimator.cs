using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 아이템 오브젝트를 출발지에서 목적지까지 포물선으로 이동시키는 DOTween 유틸.
/// DepositZone(플레이어→존), PickupZone(존→플레이어) 공통 사용.
/// </summary>
public static class ItemTransferAnimator
{
    /// <param name="item">이동할 오브젝트</param>
    /// <param name="targetParent">목적지 부모 Transform (이동 시작 시 즉시 SetParent)</param>
    /// <param name="localTargetPos">목적지 localPosition</param>
    /// <param name="localTargetRot">목적지 localRotation</param>
    /// <param name="duration">이동 시간(초)</param>
    /// <param name="arcHeight">포물선 높이</param>
    /// <param name="onComplete">완료 콜백</param>
    public static Tween Transfer(
        GameObject item,
        Transform targetParent,
        Vector3 localTargetPos,
        Quaternion localTargetRot,
        float duration,
        float arcHeight = 1f,
        Action onComplete = null)
    {
        if (item == null) return null;

        Vector3 worldStart = item.transform.position;
        item.transform.SetParent(targetParent, worldPositionStays: true);

        float progress = 0f;

        var moveTween = DOTween.To(
            () => progress,
            t =>
            {
                progress = t;
                // 매 프레임 목적지 world 좌표 재계산 → 플레이어 이동 중에도 목적지 추적
                Vector3 worldTarget = targetParent.TransformPoint(localTargetPos);
                Vector3 pos = Vector3.Lerp(worldStart, worldTarget, t);
                pos.y += arcHeight * Mathf.Sin(t * Mathf.PI);
                item.transform.position = pos;
            },
            1f,
            duration
        ).SetEase(Ease.Linear);

        var rotTween = item.transform
            .DOLocalRotate(localTargetRot.eulerAngles, duration)
            .SetEase(Ease.Linear);

        var seq = DOTween.Sequence();
        seq.Join(moveTween);
        seq.Join(rotTween);
        seq.OnComplete(() =>
        {
            if (item != null)
                item.transform.localPosition = localTargetPos;
            onComplete?.Invoke();
        });

        return seq;
    }
}
