using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 손님 한 명에 대응하는 HUD 요소.
/// 요구 수치 텍스트 + 녹색 진행 바를 표시한다.
/// UIManager가 생성/위치 갱신/파괴를 담당한다.
/// </summary>
public class CustomerHUDElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI demandText;
    [SerializeField] private Image fillImage;

    public Customer TrackedCustomer { get; private set; }

    public void Bind(Customer customer)
    {
        TrackedCustomer = customer;
        demandText.text = customer.Config.demandAmount.ToString();
        SetFill(0f);
    }

    public void RefreshFill()
    {
        if (TrackedCustomer == null) return;
        float ratio = TrackedCustomer.Config.demandAmount > 0
            ? (float)TrackedCustomer.Delivered / TrackedCustomer.Config.demandAmount
            : 0f;
        SetFill(ratio);
    }

    public void SetScreenPosition(Vector3 screenPos)
    {
        transform.position = screenPos;
    }

    private void SetFill(float ratio)
    {
        if (fillImage != null)
            fillImage.fillAmount = Mathf.Clamp01(ratio);
    }
}
