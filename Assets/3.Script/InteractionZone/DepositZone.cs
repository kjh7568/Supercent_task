using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어가 진입하면 아이템을 받아 쌓는 존.
/// S2: 더미 큐브로 레이아웃 배치를 인게임에서 확인.
/// </summary>
public class DepositZone : InteractionZone
{
    [Header("S2 Debug - 더미 배치 테스트")]
    [SerializeField] private bool spawnDummyOnEnter = true;
    [SerializeField] private int dummyCount = 5;

    private readonly List<GameObject> placedItems = new();

    public int Count => placedItems.Count;
    public bool HasSpace => layout != null && placedItems.Count < layout.Capacity;

    protected override void OnPlayerEnter(Collider player)
    {
        Debug.Log($"[DepositZone] 플레이어 진입 감지 - {gameObject.name}");

        if (spawnDummyOnEnter)
            SpawnDummies();
    }

    protected override void OnPlayerExit(Collider player)
    {
        Debug.Log($"[DepositZone] 플레이어 이탈 - {gameObject.name}");

        if (spawnDummyOnEnter)
            ClearDummies();
    }

    private void SpawnDummies()
    {
        if (layout == null)
        {
            Debug.LogWarning("[DepositZone] Layout이 없습니다. ZoneStackLayout 컴포넌트를 연결하세요.");
            return;
        }

        for (int i = 0; i < dummyCount && i < layout.Capacity; i++)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(transform);
            cube.transform.localPosition = layout.GetLocalPosition(i);
            cube.transform.localScale = Vector3.one * 0.25f;
            Destroy(cube.GetComponent<Collider>());
            placedItems.Add(cube);
        }

        Debug.Log($"[DepositZone] 더미 {placedItems.Count}개 배치 완료 (레이아웃: {layout.GetType().Name})");
    }

    private void ClearDummies()
    {
        foreach (var item in placedItems)
            if (item != null) Destroy(item);
        placedItems.Clear();
    }

    /// <summary>아이템 오브젝트를 레이아웃 위치에 배치 (S4 이후 실제 이동에서 사용)</summary>
    public void PlaceItem(GameObject item)
    {
        if (!HasSpace) return;

        int index = placedItems.Count;
        item.transform.SetParent(transform);
        item.transform.localPosition = layout.GetLocalPosition(index);
        item.transform.localRotation = Quaternion.identity;
        placedItems.Add(item);

        Debug.Log($"[DepositZone] 아이템 배치 ({index + 1}/{layout.Capacity}) - {gameObject.name}");
    }

    /// <summary>맨 위 아이템 제거 후 반환</summary>
    public GameObject TakeTopItem()
    {
        if (placedItems.Count == 0) return null;

        int last = placedItems.Count - 1;
        var item = placedItems[last];
        placedItems.RemoveAt(last);
        item.transform.SetParent(null);
        return item;
    }
}
