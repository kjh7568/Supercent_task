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
    [SerializeField] protected Vector3 placementOffset = Vector3.zero;

    protected bool IsPlayerInside { get; private set; }

    protected virtual void OnPlayerEnter(Collider player) { }
    protected virtual void OnPlayerExit(Collider player) { }

    private void OnDrawGizmos()
    {
        if (layout == null) return;

        Gizmos.color = new Color(0f, 1f, 0.4f, 0.6f);
        for (int i = 0; i < layout.Capacity; i++)
        {
            Vector3 worldPos = transform.position + transform.rotation * (placementOffset + layout.GetLocalPosition(i));
            Gizmos.DrawSphere(worldPos, 0.1f);
        }
    }

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
