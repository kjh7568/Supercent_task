using TMPro;
using UnityEngine;

/// <summary>
/// Screen Space Overlay HUD 관리자.
/// 플레이어의 StackSystem 이벤트를 구독해 소지금액을 갱신한다.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Money")]
    [SerializeField] private TextMeshProUGUI moneyText;

    private StackSystem playerStackSystem;

    private const int CoinValue = 10;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[UIManager] Player 태그 오브젝트를 찾을 수 없습니다.");
            return;
        }

        playerStackSystem = player.GetComponent<StackSystem>();
        if (playerStackSystem == null)
        {
            Debug.LogError("[UIManager] Player에 StackSystem이 없습니다.");
            return;
        }

        playerStackSystem.OnCoinCountChanged += UpdateMoneyDisplay;
        UpdateMoneyDisplay(playerStackSystem.GetCount(StackItemType.Coin));

        Debug.Log("[UIManager] 초기화 완료");
    }

    private void OnDestroy()
    {
        if (playerStackSystem != null)
            playerStackSystem.OnCoinCountChanged -= UpdateMoneyDisplay;
    }

    private void UpdateMoneyDisplay(int coinCount)
    {
        if (moneyText != null)
        {
            moneyText.text = (coinCount * CoinValue).ToString();
            Debug.Log($"[UIManager] 소지금액 갱신: {coinCount * CoinValue}");
        }
    }
}
