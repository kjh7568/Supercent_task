using UnityEngine;

/// <summary>
/// 운반 인부의 애니메이션 제어.
/// 매 프레임 이동 속도를 감지해 Animator의 Speed 파라미터를 갱신한다.
/// </summary>
[RequireComponent(typeof(Animator))]
public class TransportWorkerAnimController : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    [SerializeField] private float smoothTime = 0.1f;

    private Animator animator;
    private Vector3 prevPosition;
    private float currentSpeed;
    private float speedVelocity;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        prevPosition = transform.position;
    }

    private void Update()
    {
        float rawSpeed = Vector3.Distance(transform.position, prevPosition) / Time.deltaTime;
        prevPosition = transform.position;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, rawSpeed, ref speedVelocity, smoothTime);
        animator.SetFloat(SpeedHash, currentSpeed);
    }
}
