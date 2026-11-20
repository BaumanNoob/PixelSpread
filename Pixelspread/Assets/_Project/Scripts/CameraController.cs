using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] float zoomSpeed = 0.1f;
    [SerializeField] float minSize = 2f;
    [SerializeField] float maxSize = 200f;

    Camera cam;
    Vector3 dragOrigin;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    Vector3 MouseWorld(Mouse mouse)
    {
        Vector2 p = mouse.position.ReadValue();
        return cam.ScreenToWorldPoint(new Vector3(p.x, p.y, -transform.position.z));
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        // Zoom with the scroll wheel
        float scroll = mouse.scroll.ReadValue().y;
        if (scroll != 0f)
        {
            float size = cam.orthographicSize * (1f - Mathf.Sign(scroll) * zoomSpeed);
            cam.orthographicSize = Mathf.Clamp(size, minSize, maxSize);
        }

        // Pan by dragging with the right or middle mouse button
        bool panStart = mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame;
        bool panning = mouse.rightButton.isPressed || mouse.middleButton.isPressed;

        if (panStart)
        {
            dragOrigin = MouseWorld(mouse);
        }
        else if (panning)
        {
            transform.position += dragOrigin - MouseWorld(mouse);
        }
    }
}