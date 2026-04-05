using UnityEngine;

/// <summary>
/// 추적 화살표 (2D 스프라이트 월드 오브젝트).
/// 목적지가 화면 밖으로 나가면 플레이어 주변 월드 공간에서 목적지 방향을 가리킴.
/// 타겟 정보는 TutorialArrowUI에서 직접 참조.
/// </summary>
public class TutorialTrackingArrowUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform arrowTransform;
    [SerializeField] private Transform player;
    [SerializeField] private TutorialArrowUI destinationArrow;

    [Header("Settings")]
    [SerializeField] private float radius = 1.5f;
    [SerializeField] private bool flipVertical;

    private void Awake()
    {
        if (arrowTransform != null)
            arrowTransform.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (destinationArrow == null || !destinationArrow.HasTarget)
        {
            arrowTransform.gameObject.SetActive(false);
            return;
        }

        // 목적지가 화면 안에 있으면 숨김
        if (destinationArrow.IsTargetOnScreen)
        {
            arrowTransform.gameObject.SetActive(false);
            return;
        }

        arrowTransform.gameObject.SetActive(true);

        // XZ 평면에서 플레이어 → 타겟 방향
        Vector3 toTarget = destinationArrow.TargetWorldPos - player.position;
        toTarget.y = 0f;
        Vector3 dir = toTarget.normalized;

        // 플레이어 주변 월드 공간에 배치 (Y는 0.1 고정)
        Vector3 pos = player.position + dir * radius;
        pos.y = 0.1f;
        arrowTransform.position = pos;

        // 데스티네이션 방향으로 Y축 회전 + 90도 틸트
        Quaternion rot = Quaternion.LookRotation(dir) * Quaternion.Euler(90f, 0f, 0f);
        if (flipVertical)
            rot *= Quaternion.Euler(180f, 0f, 0f);
        arrowTransform.rotation = rot;
    }
}
