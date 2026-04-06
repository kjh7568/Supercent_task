using UnityEngine;

/// <summary>
/// 구치소 정원이 처음 가득 찼을 때 카메라 이동 알림과 업그레이드 존 활성화를 처리한다.
/// </summary>
public class PrisonFullHandler : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform cameraTarget;

    [Header("Upgrade Zone")]
    [SerializeField] private GameObject upgradeZone;

    [Header("Debug")]
    [SerializeField] private bool debugTriggerCamera;

    private bool triggered = false;

    private void Update()
    {
        if (debugTriggerCamera)
        {
            debugTriggerCamera = false;
            if (cameraTarget != null)
                TutorialCameraController.Instance?.TriggerMove(cameraTarget.position);
        }
    }

    private void Start()
    {
        if (Prison.Instance == null)
        {
            Debug.LogWarning("[PrisonFullHandler] Prison 인스턴스를 찾을 수 없음");
            return;
        }

        Prison.Instance.OnFull += HandleFull;
    }

    private void OnDestroy()
    {
        if (Prison.Instance != null)
            Prison.Instance.OnFull -= HandleFull;
    }

    private void HandleFull()
    {
        if (triggered) return;
        triggered = true;

        if (upgradeZone != null)
            upgradeZone.SetActive(true);

        if (cameraTarget != null)
            TutorialCameraController.Instance?.TriggerMove(cameraTarget.position);

        Debug.Log("[PrisonFullHandler] 정원 가득 참 → 카메라 이동 + 업그레이드 존 활성화");
    }
}
