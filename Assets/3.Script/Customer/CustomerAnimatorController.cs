using UnityEngine;

/// <summary>
/// Animator 파라미터 Speed(float)를 구동:
///   0: Idle, 1 이상: Move
/// Customer의 이동 여부를 매 프레임 감지해 전환한다.
/// </summary>
[RequireComponent(typeof(Animator))]
public class CustomerAnimatorController : MonoBehaviour
{
    [SerializeField] private Customer customer;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private Animator _animator;
    private Vector3 _prevPosition;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _prevPosition = transform.position;
    }

    private void Update()
    {
        float moved = Vector3.Distance(transform.position, _prevPosition);
        float speed = moved / Time.deltaTime;
        _prevPosition = transform.position;

        _animator.SetFloat(SpeedHash, speed);
    }
}
