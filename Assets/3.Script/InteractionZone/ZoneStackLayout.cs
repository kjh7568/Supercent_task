using UnityEngine;

/// <summary>
/// 존 내부 아이템 쌓기 위치를 결정하는 전략 베이스.
/// LinearZoneLayout(일자), GridZoneLayout(N열 그리드) 중 하나를 붙여 사용.
/// </summary>
public abstract class ZoneStackLayout : MonoBehaviour
{
    [Header("Item Rotation")]
    [SerializeField] private Vector3 itemEulerAngles = Vector3.zero;

    public abstract int Capacity { get; }
    public Quaternion ItemRotation => Quaternion.Euler(itemEulerAngles);

    /// <summary>index번째 아이템의 localPosition 반환</summary>
    public abstract Vector3 GetLocalPosition(int index);
}
