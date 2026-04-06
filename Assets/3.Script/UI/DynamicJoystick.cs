using UnityEngine;
using UnityEngine.UI;

public class DynamicJoystick : MonoBehaviour
{
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;
    [SerializeField] private float handleRange = 100f;

    public Vector2 Direction { get; private set; }

    private int _touchId = -1;
    private Camera _uiCamera;

    private void Awake()
    {
        var canvas = GetComponentInParent<Canvas>();
        _uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        background.gameObject.SetActive(false);
        handle.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.touchCount == 0)
        {
            if (_touchId != -1)
                ResetJoystick();
            return;
        }

        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began && _touchId == -1)
            {
                _touchId = touch.fingerId;
                ActivateJoystick(touch.position);
            }
            else if (touch.fingerId == _touchId)
            {
                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    MoveHandle(touch.position);
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    ResetJoystick();
            }
        }
    }

    private void ActivateJoystick(Vector2 screenPos)
    {
        background.gameObject.SetActive(true);
        handle.gameObject.SetActive(true);
        background.position = screenPos;
        handle.position = screenPos;
    }

    private void MoveHandle(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background, screenPos, _uiCamera, out Vector2 localPoint);

        if (localPoint.magnitude > handleRange)
            localPoint = localPoint.normalized * handleRange;

        handle.position = background.TransformPoint(localPoint);
        Direction = localPoint / handleRange;
    }

    private void ResetJoystick()
    {
        _touchId = -1;
        Direction = Vector2.zero;
        background.gameObject.SetActive(false);
        handle.gameObject.SetActive(false);
    }
}
