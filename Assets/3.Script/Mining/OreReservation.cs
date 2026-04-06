using System.Collections.Generic;

/// <summary>
/// 인부들이 동일한 광석을 중복 타겟팅하지 않도록 예약을 관리하는 static 레지스트리.
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
}
