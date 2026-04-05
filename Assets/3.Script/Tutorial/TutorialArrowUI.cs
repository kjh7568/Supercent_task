using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 목적지 화살표 UI.
/// 목적지가 화면 안에 있으면 해당 스크린 좌표에 화살표를 표시하고 방향을 가리킴.
/// 화면 밖이면 비활성화 → TutorialTrackingArrowUI가 대신 안내.
/// </summary>
public class TutorialArrowUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform arrowRect;
    [SerializeField] private Transform player;

    [Header("Debug")]
    [SerializeField] private Transform debugTarget;

    private Camera mainCam;
    private Vector3 targetWorldPos;
    private bool hasTarget;

    /// <summary>목적지가 현재 화면 안에 표시 중인지 여부. TutorialTrackingArrowUI가 참조.</summary>
    public bool IsTargetOnScreen { get; private set; }

    private void Awake()
    {
        mainCam = Camera.main;
        arrowRect.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (debugTarget != null)
            SetTarget(debugTarget.position);
    }

    public void SetTarget(Vector3 worldPos)
    {
        targetWorldPos = worldPos;
        hasTarget = true;
    }

    public void ClearTarget()
    {
        hasTarget = false;
        arrowRect.gameObject.SetActive(false);
        IsTargetOnScreen = false;
    }

    private void LateUpdate()
    {
        if (!hasTarget) return;

        Vector3 screenPos = mainCam.WorldToScreenPoint(targetWorldPos);

        bool onScreen = screenPos.z > 0f
            && screenPos.x >= 0f && screenPos.x <= Screen.width
            && screenPos.y >= 0f && screenPos.y <= Screen.height;

        IsTargetOnScreen = onScreen;
        arrowRect.gameObject.SetActive(onScreen);

        if (!onScreen) return;

        // 화살표를 목적지 스크린 좌표에 배치
        arrowRect.position = screenPos;

        // 플레이어 스크린 좌표 → 목적지 방향으로 회전
        Vector3 playerScreenPos = mainCam.WorldToScreenPoint(player.position);
        Vector2 dir = (Vector2)screenPos - (Vector2)playerScreenPos;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        arrowRect.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
