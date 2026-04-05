using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 튜토리얼 카메라 이동 컴포넌트.
/// TriggerMove() 호출 시 CameraFollow를 끄고 지정 위치로 이동,
/// holdDuration초 대기 후 플레이어 추적으로 복귀.
/// </summary>
public class TutorialCameraController : MonoBehaviour
{
    public static TutorialCameraController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private DynamicJoystick joystick;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float holdDuration = 2f;

    [Header("Debug")]
    [SerializeField] private Transform debugLookTarget;
    [SerializeField] private bool debugTrigger;

    private bool isMoving;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (debugTrigger)
        {
            debugTrigger = false;
            if (debugLookTarget != null)
                TriggerMove(debugLookTarget.position, null);
        }
    }

    /// <summary>
    /// 카메라를 targetWorldPos로 이동 후 holdDuration초 대기 뒤 복귀.
    /// onComplete: 복귀 완료 후 콜백 (선택).
    /// </summary>
    public void TriggerMove(Vector3 targetWorldPos, Action onComplete = null)
    {
        if (isMoving) return;
        StartCoroutine(MoveRoutine(targetWorldPos, onComplete));
    }

    private IEnumerator MoveRoutine(Vector3 targetPos, Action onComplete)
    {
        isMoving = true;
        cameraFollow.enabled = false;
        playerController.enabled = false;
        joystick.enabled = false;

        // 목표 위치로 이동
        while (Vector3.Distance(cameraTransform.position, targetPos) > 0.05f)
        {
            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        cameraTransform.position = targetPos;

        // 대기
        yield return new WaitForSeconds(holdDuration);

        // 플레이어 추적 위치로 부드럽게 복귀
        while (Vector3.Distance(cameraTransform.position, cameraFollow.FollowPosition) > 0.05f)
        {
            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position, cameraFollow.FollowPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        cameraFollow.enabled = true;
        playerController.enabled = true;
        joystick.enabled = true;
        isMoving = false;

        onComplete?.Invoke();
        Debug.Log("[TutorialCamera] 카메라 복귀 완료");
    }
}
