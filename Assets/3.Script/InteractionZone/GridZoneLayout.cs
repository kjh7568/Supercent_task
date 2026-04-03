using UnityEngine;

/// <summary>
/// 열(X) × 행(Z) 레이어를 채운 뒤 위(Y)로 쌓는 3D 그리드.
/// 코인 픽업존 등 바닥에 행렬로 쌓이는 존에 사용.
/// </summary>
public class GridZoneLayout : ZoneStackLayout
{
    [SerializeField] private int columns = 3;
    [SerializeField] private int rows = 3;
    [SerializeField] private Vector3 colAxis = Vector3.right;
    [SerializeField] private Vector3 rowAxis = Vector3.forward;
    [SerializeField] private Vector3 heightAxis = Vector3.up;
    [SerializeField] private float colSpacing = 0.4f;
    [SerializeField] private float rowSpacing = 0.4f;
    [SerializeField] private float heightSpacing = 0.4f;
    [SerializeField] private int capacity = 30;

    public override int Capacity => capacity;

    public override Vector3 GetLocalPosition(int index)
    {
        int itemsPerLayer = columns * rows;
        int layer = index / itemsPerLayer;
        int remainder = index % itemsPerLayer;
        int col = remainder % columns;
        int row = remainder / columns;

        return colAxis * (col * colSpacing)
             + rowAxis * (row * rowSpacing)
             + heightAxis * (layer * heightSpacing);
    }
}
