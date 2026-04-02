using System.Collections.Generic;
using UnityEngine;

public class StackSystem : MonoBehaviour
{
    [SerializeField] private List<StackPoint> stackPoints = new();

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

    public void Add(StackItemConfig config)
    {
        foreach (var sp in stackPoints)
        {
            if (sp.TargetType == config.itemType && sp.HasSpace)
            {
                sp.AddItem(config);
                return;
            }
        }

        Debug.LogWarning($"[StackSystem] {config.itemType} 타입의 빈 StackPoint가 없습니다.");
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
                return;
            }
        }
    }
}
