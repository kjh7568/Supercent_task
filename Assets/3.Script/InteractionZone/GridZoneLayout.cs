using UnityEngine;

/// <summary>
/// N열 그리드로 가로 먼저 채운 뒤 위로 쌓기.
/// 가공기 DepositZone 등 3줄로 쌓이는 존에 사용.
/// </summary>
public class GridZoneLayout : ZoneStackLayout
{
    [SerializeField] private int columns = 3;
    [SerializeField] private Vector3 colAxis = Vector3.right;
    [SerializeField] private Vector3 heightAxis = Vector3.up;
    [SerializeField] private float colSpacing = 0.4f;
    [SerializeField] private float heightSpacing = 0.4f;
    [SerializeField] private int capacity = 30;

    public override int Capacity => capacity;

    public override Vector3 GetLocalPosition(int index)
    {
        int col = index % columns;
        int row = index / columns;
        return colAxis * (col * colSpacing) + heightAxis * (row * heightSpacing);
    }
}
