using UnityEngine;

/// <summary>
/// 플레이어가 진입하면 MiningController의 채광 모드를 ON/OFF하는 존.
/// BoxCollider (IsTrigger) 필요.
/// </summary>
public class MiningZone : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        var mc = other.GetComponent<MiningController>();
        if (mc == null) return;
        mc.SetMiningMode(true);
        Debug.Log("[MiningZone] 채광 모드 ON");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        var mc = other.GetComponent<MiningController>();
        if (mc == null) return;
        mc.SetMiningMode(false);
        Debug.Log("[MiningZone] 채광 모드 OFF");
    }
}
