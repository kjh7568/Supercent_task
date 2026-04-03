using UnityEngine;

/// <summary>
/// 단일 축 방향으로 일자 쌓기.
/// 가공기 PickupZone 등 한 줄로 쌓이는 존에 사용.
/// </summary>
public class LinearZoneLayout : ZoneStackLayout
{
    [SerializeField] private Vector3 stackAxis = Vector3.up;
    [SerializeField] private float spacing = 0.3f;
    [SerializeField] private int capacity = 10;

    public override int Capacity => capacity;

    public override Vector3 GetLocalPosition(int index)
    {
        return stackAxis * (spacing * index);
    }
}
