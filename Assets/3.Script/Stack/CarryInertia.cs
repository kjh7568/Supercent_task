using UnityEngine;

/// <summary>
/// StackPoint에 부착. 플레이어 이동 시 스택이 반대 방향으로 기울어지는 관성 효과.
/// localRotation만 수정 → StackPoint의 holdOffset(localPosition) 시스템과 충돌 없음.
/// 피벗(StackPoint 위치) 기준 회전이므로 베이스는 고정, 위로 갈수록 변위 증가.
/// </summary>
public class CarryInertia : MonoBehaviour
{
    [Header("Inertia")]
    [SerializeField] private float maxTiltAngle = 15f;
    [SerializeField] private float inertiaStrength = 80f;
    [SerializeField] private float returnSpeed = 8f;

    private Quaternion _originalLocalRot;
    private Vector3 _prevWorldPos;
    private Vector2 _tiltAngle; // x: 좌우, y: 전후

    private void Awake()
    {
        _originalLocalRot = transform.localRotation;
    }

    private void Start()
    {
        Transform root = transform.parent;
        _prevWorldPos = root != null ? root.position : transform.position;
    }

    private void LateUpdate()
    {
        Transform root = transform.parent;
        if (root == null) return;

        // 이번 프레임 이동 벡터 → 로컬 공간 변환
        Vector3 worldDelta = root.position - _prevWorldPos;
        _prevWorldPos = root.position;

        Vector3 localDelta = root.InverseTransformDirection(worldDelta);

        // 이동 반대 방향으로 기울기 누적
        _tiltAngle -= new Vector2(localDelta.x, localDelta.z) * inertiaStrength;

        // 최대 각도 클램프
        if (_tiltAngle.magnitude > maxTiltAngle)
            _tiltAngle = _tiltAngle.normalized * maxTiltAngle;

        // 원위치로 복귀
        _tiltAngle = Vector2.Lerp(_tiltAngle, Vector2.zero, returnSpeed * Time.deltaTime);

        // 회전 적용: y(전후) → X축 회전, x(좌우) → Z축 회전
        Quaternion tilt = Quaternion.Euler(_tiltAngle.y, 0f, -_tiltAngle.x);
        transform.localRotation = _originalLocalRot * tilt;
    }
}
