using UnityEngine;

/// <summary>
/// DepositZone / PickupZone 공통 베이스.
/// 플레이어 태그 감지 및 레이아웃 참조를 담당.
/// </summary>
[RequireComponent(typeof(Collider))]
public abstract class InteractionZone : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] protected ZoneStackLayout layout;

    protected bool IsPlayerInside { get; private set; }

    protected virtual void OnPlayerEnter(Collider player) { }
    protected virtual void OnPlayerExit(Collider player) { }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        IsPlayerInside = true;
        OnPlayerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        IsPlayerInside = false;
        OnPlayerExit(other);
    }
}
