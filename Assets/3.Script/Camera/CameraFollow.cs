using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -7f);

    public Vector3 FollowPosition => target.position + offset;

    private void LateUpdate()
    {
        transform.position = target.position + offset;
    }
}
