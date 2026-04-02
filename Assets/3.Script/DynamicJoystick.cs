using UnityEngine;
using UnityEngine.UI;

public class DynamicJoystick : MonoBehaviour
{
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;
    [SerializeField] private float handleRange = 100f;

    public Vector2 Direction { get; private set; }

    private int _touchId = -1;
    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        background.gameObject.SetActive(false);
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
        background.position = screenPos;
        handle.anchoredPosition = Vector2.zero;
    }

    private void MoveHandle(Vector2 screenPos)
    {
        Vector2 delta = screenPos - (Vector2)background.position;
        float scaleFactor = _canvas.scaleFactor;
        Vector2 localDelta = delta / scaleFactor;

        if (localDelta.magnitude > handleRange)
            localDelta = localDelta.normalized * handleRange;

        handle.anchoredPosition = localDelta;
        Direction = localDelta / handleRange;
    }

    private void ResetJoystick()
    {
        _touchId = -1;
        Direction = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
        background.gameObject.SetActive(false);
    }
}
