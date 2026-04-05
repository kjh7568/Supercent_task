using UnityEngine;

/// <summary>
/// Animator 파라미터 2개를 구동:
///   Speed(float)   - 0: 대기, 1: 이동
///   IsMining(bool) - true: 채굴(곡괭이질) 상태
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private DynamicJoystick joystick;
    [SerializeField] private MiningController miningController;

    private static readonly int SpeedHash    = Animator.StringToHash("Speed");
    private static readonly int IsMiningHash = Animator.StringToHash("IsMining");

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        float speed = joystick.Direction.magnitude;
        bool isMining = miningController != null && miningController.IsMiningMode;

        _animator.SetFloat(SpeedHash, speed);
        _animator.SetBool(IsMiningHash, isMining);
    }
}
