using System.Collections.Generic;
using UnityEngine;

public class StackPoint : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private StackItemType targetType;
    [SerializeField] private int capacity = 10;
    [SerializeField] private Vector3 stackAxis = Vector3.up;
    [SerializeField] private float itemSpacing = 0.3f;

    [Header("Hold Offset")]
    [SerializeField] private bool useHoldOffset = false;
    [SerializeField] private Vector3 holdOffset = Vector3.zero;
    /// <summary>null이면 자기 자신의 아이템 수 기준. 설정 시 해당 StackPoint의 아이템 수 기준으로 이동.</summary>
    [SerializeField] private StackPoint holdOffsetTrigger;

    public StackItemType TargetType => targetType;
    public bool HasSpace => items.Count < capacity;
    public int Count => items.Count;

    /// <summary>다음 아이템이 배치될 월드 좌표 반환 (포물선 애니메이션 타겟용)</summary>
    public Vector3 GetNextItemWorldPosition()
    {
        Vector3 localOffset = stackAxis * (itemSpacing * items.Count);
        return transform.TransformPoint(localOffset);
    }

    public void SetCapacity(int newCapacity) => capacity = newCapacity;

    private readonly List<GameObject> items = new();
    private Vector3 originalLocalPosition;
    private int lastTriggerCount = -1;

    private void Awake()
    {
        originalLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (!useHoldOffset || holdOffsetTrigger == null) return;

        int current = holdOffsetTrigger.Count;
        if (current == lastTriggerCount) return;

        lastTriggerCount = current;
        transform.localPosition = current > 0
            ? originalLocalPosition + holdOffset
            : originalLocalPosition;
    }

    private void UpdateHoldPosition()
    {
        // holdOffsetTrigger가 있으면 Update에서 처리
        if (!useHoldOffset || holdOffsetTrigger != null) return;

        transform.localPosition = items.Count > 0
            ? originalLocalPosition + holdOffset
            : originalLocalPosition;
    }

    public GameObject AddItem(StackItemConfig config)
    {
        if (!HasSpace) return null;

        GameObject instance = Instantiate(config.itemPrefab, transform);
        instance.transform.localPosition = stackAxis * (itemSpacing * items.Count);
        instance.transform.localRotation = Quaternion.identity;

        items.Add(instance);
        UpdateHoldPosition();
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
        UpdateHoldPosition();
        return true;
    }

    public GameObject RemoveTopItem()
    {
        if (items.Count == 0) return null;

        int lastIndex = items.Count - 1;
        GameObject top = items[lastIndex];
        items.RemoveAt(lastIndex);

        top.transform.SetParent(null);
        UpdateHoldPosition();
        return top;
    }
}
