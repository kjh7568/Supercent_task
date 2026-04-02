using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private DynamicJoystick joystick;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 10f;

    private void Update()
    {
        Vector2 dir = joystick.Direction;
        if (dir.sqrMagnitude < 0.01f) return;

        Vector3 moveDir = new Vector3(dir.x, 0f, dir.y);
        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);

        Quaternion targetRot = Quaternion.LookRotation(moveDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
    }
}
