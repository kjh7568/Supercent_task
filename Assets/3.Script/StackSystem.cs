using System.Collections.Generic;
using UnityEngine;

public class StackSystem : MonoBehaviour
{
    [Header("Stack Points (앞 = 먼저 채움)")]
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
            if (sp.HasSpace)
            {
                sp.AddItem(config);
                return;
            }
        }

        Debug.LogWarning("[StackSystem] 모든 StackPoint가 가득 찼습니다.");
    }

    public void Remove()
    {
        for (int i = stackPoints.Count - 1; i >= 0; i--)
        {
            var item = stackPoints[i].RemoveTopItem();
            if (item != null)
            {
                Destroy(item);
                return;
            }
        }
    }
}
