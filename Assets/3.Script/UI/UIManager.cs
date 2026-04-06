using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Screen Space Overlay HUD 관리자.
/// - 소지금액 표시 (StackSystem 이벤트 구독)
/// - 손님별 요구수치 + 진행 바 (WorldToScreenPoint 위치 추적)
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Money")]
    [SerializeField] private TextMeshProUGUI moneyText;

    [Header("Customer HUD")]
    [SerializeField] private CustomerHUDElement customerHUDPrefab;
    [SerializeField] private float customerHeadOffset = 2f;

    [Header("MAX")]
    [SerializeField] private Image maxImage;
    [SerializeField] private float maxFadeDuration = 1.5f;
    [SerializeField] private float maxTorsoOffset = 1f;
    [SerializeField] private float maxRiseAmount = 50f;

    private StackSystem playerStackSystem;
    private MiningController miningController;
    private Transform playerTransform;
    private Camera mainCamera;
    private readonly List<CustomerHUDElement> activeCustomerHUDs = new();
    private Coroutine maxFadeCoroutine;
    private float maxScreenRiseOffset;

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
        mainCamera = Camera.main;

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

        playerTransform = player.transform;
        miningController = player.GetComponent<MiningController>();
        if (miningController != null)
            miningController.OnMaxReached += ShowMax;

        if (maxImage != null)
            maxImage.gameObject.SetActive(false);

        Debug.Log("[UIManager] 초기화 완료");
    }

    private void OnDestroy()
    {
        if (playerStackSystem != null)
            playerStackSystem.OnCoinCountChanged -= UpdateMoneyDisplay;
        if (miningController != null)
            miningController.OnMaxReached -= ShowMax;
    }

    // ── Money ─────────────────────────────────────────────

    private void UpdateMoneyDisplay(int coinCount)
    {
        if (moneyText != null)
        {
            moneyText.text = (coinCount * CoinValue).ToString();
            Debug.Log($"[UIManager] 소지금액 갱신: {coinCount * CoinValue}");
        }
    }

    // ── Customer HUD ───────────────────────────────────────

    public void RegisterCustomerHUD(Customer customer)
    {
        if (customerHUDPrefab == null) return;

        var element = Instantiate(customerHUDPrefab, transform);
        element.Bind(customer);
        activeCustomerHUDs.Add(element);
        Debug.Log($"[UIManager] 손님 HUD 등록 (요구량: {customer.DemandAmount})");
    }

    public void UnregisterCustomerHUD(Customer customer)
    {
        var element = activeCustomerHUDs.Find(e => e.TrackedCustomer == customer);
        if (element == null) return;

        activeCustomerHUDs.Remove(element);
        Destroy(element.gameObject);
        Debug.Log("[UIManager] 손님 HUD 해제");
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;

        foreach (var hud in activeCustomerHUDs)
        {
            if (hud == null || hud.TrackedCustomer == null) continue;

            Vector3 worldPos = hud.TrackedCustomer.transform.position + Vector3.up * customerHeadOffset;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

            bool behindCamera = screenPos.z < 0f;
            hud.gameObject.SetActive(!behindCamera && hud.TrackedCustomer.ShowHUD);
            if (!behindCamera)
            {
                hud.SetScreenPosition(screenPos);
                hud.RefreshFill();
            }
        }

        if (maxImage != null && maxImage.gameObject.activeSelf && playerTransform != null)
        {
            Vector3 worldPos = playerTransform.position + Vector3.up * maxTorsoOffset;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            screenPos.y += maxScreenRiseOffset;
            maxImage.rectTransform.position = screenPos;
        }
    }

    // ── MAX Image ──────────────────────────────────────────

    private void ShowMax()
    {
        if (maxImage == null) return;

        if (maxFadeCoroutine != null)
            StopCoroutine(maxFadeCoroutine);

        maxImage.gameObject.SetActive(true);
        maxScreenRiseOffset = 0f;
        SetMaxAlpha(1f);
        maxFadeCoroutine = StartCoroutine(FadeOutMax());
        Debug.Log("[UIManager] MAX 표시");
    }

    private IEnumerator FadeOutMax()
    {
        float elapsed = 0f;
        while (elapsed < maxFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / maxFadeDuration;
            SetMaxAlpha(1f - t);
            maxScreenRiseOffset = t * maxRiseAmount;
            yield return null;
        }
        SetMaxAlpha(0f);
        maxImage.gameObject.SetActive(false);
        maxFadeCoroutine = null;
    }

    private void SetMaxAlpha(float alpha)
    {
        Color c = maxImage.color;
        c.a = alpha;
        maxImage.color = c;
    }
}
