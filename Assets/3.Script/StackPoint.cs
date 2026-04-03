using System.Collections.Generic;
using UnityEngine;

public class StackPoint : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private StackItemType targetType;
    [SerializeField] private int capacity = 10;
    [SerializeField] private Vector3 stackAxis = Vector3.up;
    [SerializeField] private float itemSpacing = 0.3f;

    public StackItemType TargetType => targetType;
    public bool HasSpace => items.Count < capacity;
    public int Count => items.Count;

    private readonly List<GameObject> items = new();

    public GameObject AddItem(StackItemConfig config)
    {
        if (!HasSpace) return null;

        GameObject instance = Instantiate(config.itemPrefab, transform);
        instance.transform.localPosition = stackAxis * (itemSpacing * items.Count);
        instance.transform.localRotation = Quaternion.identity;

        items.Add(instance);
        return instance;
    }

    /// <summary>기존 GameObject를 StackPoint에 수용 (PickupZone → 플레이어 이동 시 사용)</summary>
    public bool AddExistingItem(GameObject item)
    {
        if (!HasSpace || item == null) return false;

        item.transform.SetParent(transform);
        item.transform.localPosition = stackAxis * (itemSpacing * items.Count);
        item.transform.localRotation = Quaternion.identity;
        items.Add(item);
        return true;
    }

    public GameObject RemoveTopItem()
    {
        if (items.Count == 0) return null;

        int lastIndex = items.Count - 1;
        GameObject top = items[lastIndex];
        items.RemoveAt(lastIndex);

        top.transform.SetParent(null);
        return top;
    }
}
