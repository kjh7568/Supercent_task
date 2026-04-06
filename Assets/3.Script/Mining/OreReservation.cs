using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인부들이 동일한 광석을 중복 타겟팅하지 않도록 예약을 관리하는 static 레지스트리.
/// 광석 탐색 유틸(FindNearest)도 여기서 제공한다.
/// </summary>
public static class OreReservation
{
    private static readonly Dictionary<OreObject, WorkerController> reservations = new();

    /// <summary>광석 예약 시도. 이미 예약된 경우 false 반환.</summary>
    public static bool TryReserve(OreObject ore, WorkerController worker)
    {
        if (ore == null || reservations.ContainsKey(ore)) return false;
        reservations[ore] = worker;
        return true;
    }

    /// <summary>광석 예약 해제.</summary>
    public static void Release(OreObject ore)
    {
        if (ore != null)
            reservations.Remove(ore);
    }

    public static bool IsReserved(OreObject ore)
        => ore != null && reservations.ContainsKey(ore);

    /// <summary>
    /// Collider 배열에서 from에 가장 가까운 Active 광석을 반환.
    /// 플레이어 채굴처럼 예약 여부를 무시하는 경우에 사용.
    /// </summary>
    public static OreObject FindNearest(Vector3 from, Collider[] hits)
    {
        OreObject nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            var ore = hit.GetComponent<OreObject>();
            if (ore == null || ore.State != OreObject.OreState.Active) continue;

            float dist = Vector3.Distance(from, ore.transform.position);
            if (dist < minDist) { minDist = dist; nearest = ore; }
        }

        return nearest;
    }

    /// <summary>
    /// OreObject 배열에서 from에 가장 가까운 Active·미예약 광석을 반환.
    /// 인부 AI처럼 예약된 광석을 건너뛰어야 하는 경우에 사용.
    /// </summary>
    public static OreObject FindNearestUnreserved(Vector3 from, OreObject[] candidates)
    {
        OreObject nearest = null;
        float minDist = float.MaxValue;

        foreach (var ore in candidates)
        {
            if (ore == null || ore.State != OreObject.OreState.Active) continue;
            if (IsReserved(ore)) continue;

            float dist = Vector3.Distance(from, ore.transform.position);
            if (dist < minDist) { minDist = dist; nearest = ore; }
        }

        return nearest;
    }
}
