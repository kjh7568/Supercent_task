using UnityEngine;

/// <summary>
/// 목적지 화살표 (2D 스프라이트 월드 오브젝트).
/// 목적지가 화면 안에 있으면 타겟 월드 위치 위에 배치, 카메라를 향해 빌보드.
/// 화면 밖이면 비활성화 → TutorialTrackingArrowUI가 대신 안내.
/// </summary>
public class TutorialArrowUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform arrowTransform;
    [SerializeField] private Transform player;

    [Header("Settings")]
    [SerializeField] private float heightOffset = 1.5f;
    [SerializeField] private bool flipVertical;

    [Header("Debug")]
    [SerializeField] private Transform debugTarget;

    private Camera mainCam;
    private Vector3 targetWorldPos;
    private bool hasTarget;

    public bool IsTargetOnScreen { get; private set; }
    public bool HasTarget => hasTarget;
    public Vector3 TargetWorldPos => targetWorldPos;

    private void Awake()
    {
        mainCam = Camera.main;
        if (arrowTransform != null)
            arrowTransform.gameObject.SetActive(false);
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
        arrowTransform.gameObject.SetActive(false);
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
        arrowTransform.gameObject.SetActive(onScreen);

        if (!onScreen) return;

        // 타겟 월드 위치 위에 배치
        arrowTransform.position = targetWorldPos + Vector3.up * heightOffset;

        // 플레이어를 향해 Y축만 회전
        Vector3 toPlayer = player.position - arrowTransform.position;
        toPlayer.y = 0f;
        if (toPlayer != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(toPlayer);
            if (flipVertical)
                rot *= Quaternion.Euler(180f, 0f, 0f);
            arrowTransform.rotation = rot;
        }
    }
}
