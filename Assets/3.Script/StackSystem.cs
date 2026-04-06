using System;
using System.Collections.Generic;
using UnityEngine;

public class StackSystem : MonoBehaviour
{
    [SerializeField] private List<StackPoint> stackPoints = new();

    public event Action<int> OnCoinCountChanged;

    public int TotalCount
    {
        get
        {
            int count = 0;
            foreach (var sp in stackPoints) count += sp.Count;
            return count;
        }
    }

    private void Start()
    {
        var ores = FindObjectsByType<OreObject>(FindObjectsSortMode.None);
        foreach (var ore in ores)
            ore.OnPickedUp += Add;
    }

    private void OnDestroy()
    {
        var ores = FindObjectsByType<OreObject>(FindObjectsSortMode.None);
        foreach (var ore in ores)
            ore.OnPickedUp -= Add;
    }

    public int GetCount(StackItemType type)
    {
        int count = 0;
        foreach (var sp in stackPoints)
            if (sp.TargetType == type) count += sp.Count;
        return count;
    }

    public bool HasSpace(StackItemType type)
    {
        foreach (var sp in stackPoints)
        {
            if (sp.TargetType == type && sp.HasSpace)
                return true;
        }
        return false;
    }

    public void Add(StackItemConfig config)
    {
        foreach (var sp in stackPoints)
        {
            if (sp.TargetType == config.itemType && sp.HasSpace)
            {
                sp.AddItem(config);
                if (config.itemType == StackItemType.Coin)
                    OnCoinCountChanged?.Invoke(GetCount(StackItemType.Coin));
                return;
            }
        }

        Debug.LogWarning($"[StackSystem] {config.itemType} 타입의 빈 StackPoint가 없습니다.");
    }

    public bool HasItem(StackItemType type)
    {
        foreach (var sp in stackPoints)
            if (sp.TargetType == type && sp.Count > 0) return true;
        return false;
    }

    /// <summary>타입에 해당하는 아이템을 꺼내 반환 (Destroy 없이). InteractionZone 이동 시 사용.</summary>
    public GameObject TakeItem(StackItemType type)
    {
        for (int i = stackPoints.Count - 1; i >= 0; i--)
        {
            if (stackPoints[i].TargetType != type) continue;
            var item = stackPoints[i].RemoveTopItem();
            if (item != null)
            {
                if (type == StackItemType.Coin)
                    OnCoinCountChanged?.Invoke(GetCount(StackItemType.Coin));
                return item;
            }
        }
        return null;
    }

    /// <summary>기존 GameObject를 타입에 맞는 StackPoint에 배치 (PickupZone → 플레이어 이동 시 사용)</summary>
    public bool ReceiveItem(GameObject item, StackItemType type)
    {
        foreach (var sp in stackPoints)
        {
            if (sp.TargetType == type && sp.HasSpace)
            {
                bool result = sp.AddExistingItem(item);
                if (result && type == StackItemType.Coin)
                    OnCoinCountChanged?.Invoke(GetCount(StackItemType.Coin));
                return result;
            }
        }
        return false;
    }

    /// <summary>타입에 해당하는 빈 공간이 있는 StackPoint 반환 (PickupZone parent 설정용)</summary>
    public StackPoint GetStackPoint(StackItemType type)
    {
        foreach (var sp in stackPoints)
            if (sp.TargetType == type && sp.HasSpace) return sp;
        return null;
    }

    public void Remove(StackItemType type)
    {
        for (int i = stackPoints.Count - 1; i >= 0; i--)
        {
            if (stackPoints[i].TargetType != type) continue;

            var item = stackPoints[i].RemoveTopItem();
            if (item != null)
            {
                Destroy(item);
                if (type == StackItemType.Coin)
                    OnCoinCountChanged?.Invoke(GetCount(StackItemType.Coin));
                return;
            }
        }
    }
}
