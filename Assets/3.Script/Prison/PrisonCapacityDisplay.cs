using TMPro;
using UnityEngine;

/// <summary>
/// 구치소 위에 현재인원/전체인원을 월드 스페이스 텍스트로 표시한다.
/// </summary>
public class PrisonCapacityDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshPro capacityText;

    private void Start()
    {
        if (Prison.Instance == null)
        {
            Debug.LogWarning("[PrisonCapacityDisplay] Prison 인스턴스를 찾을 수 없음");
            return;
        }

        Prison.Instance.OnCountChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (Prison.Instance != null)
            Prison.Instance.OnCountChanged -= Refresh;
    }

    private void Refresh()
    {
        capacityText.text = $"{Prison.Instance.CurrentCount}/{Prison.Instance.Capacity}";
        Debug.Log($"[PrisonCapacityDisplay] 갱신 {Prison.Instance.CurrentCount}/{Prison.Instance.Capacity}");
    }
}
