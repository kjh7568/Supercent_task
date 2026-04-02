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
