using UnityEngine;

/// <summary>
/// 첫 코인 획득 시 지정된 업그레이드 존들을 활성화하는 일회성 컴포넌트.
/// </summary>
public class TutorialUpgradeZoneActivator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StackSystem stackSystem;

    [Header("Activate On First Coin")]
    [SerializeField] private GameObject[] upgradeZones;

    private void Start()
    {
        stackSystem.OnCoinCountChanged += OnCoinCountChanged;
    }

    private void OnCoinCountChanged(int count)
    {
        if (count <= 0) return;

        foreach (var zone in upgradeZones)
        {
            if (zone != null)
                zone.SetActive(true);
        }

        Debug.Log("[TutorialActivator] 첫 코인 획득 → 업그레이드 존 활성화");
        stackSystem.OnCoinCountChanged -= OnCoinCountChanged;
        enabled = false;
    }
}
