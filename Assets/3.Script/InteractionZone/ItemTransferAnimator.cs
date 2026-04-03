using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 아이템 오브젝트를 출발지에서 목적지까지 포물선으로 이동시키는 코루틴 유틸.
/// DepositZone(플레이어→존), PickupZone(존→플레이어) 공통 사용.
/// </summary>
public static class ItemTransferAnimator
{
    /// <param name="item">이동할 오브젝트</param>
    /// <param name="target">목적지 월드 좌표</param>
    /// <param name="duration">이동 시간(초)</param>
    /// <param name="arcHeight">포물선 높이</param>
    /// <param name="onComplete">완료 콜백</param>
    public static IEnumerator Transfer(
        GameObject item,
        Vector3 target,
        float duration,
        float arcHeight = 1f,
        Action onComplete = null)
    {
        if (item == null) yield break;

        Vector3 start = item.transform.position;
        float elapsed = 0f;

        item.transform.SetParent(null);

        while (elapsed < duration)
        {
            if (item == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 pos = Vector3.Lerp(start, target, t);
            pos.y += arcHeight * Mathf.Sin(t * Mathf.PI);
            item.transform.position = pos;

            yield return null;
        }

        if (item != null)
            item.transform.position = target;

        onComplete?.Invoke();
    }
}
