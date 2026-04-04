using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private DynamicJoystick joystick;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 10f;

    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        Vector2 dir = joystick.Direction;
        if (dir.sqrMagnitude < 0.01f) return;

        // 카메라 기준 forward/right를 XZ 평면에 투영
        Vector3 camForward = _cam.transform.forward;
        Vector3 camRight   = _cam.transform.right;
        camForward.y = 0f; camForward.Normalize();
        camRight.y   = 0f; camRight.Normalize();

        Vector3 moveDir = camRight * dir.x + camForward * dir.y;
        transform.Translate(moveDir * (moveSpeed * Time.deltaTime), Space.World);

        Quaternion targetRot = Quaternion.LookRotation(moveDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
    }
}
