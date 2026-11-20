using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    // Screen pixels per world pixel. Whole numbers only, so every pixel stays the same size.
    static readonly int[] Scales = { 1, 2, 3, 4, 5, 6, 8, 10, 12, 16, 20, 24, 32, 48, 64 };

    // Zoom-out limit: once you have painted at least "painted" pixels,
    // you may zoom out to "minScale". These numbers are placeholders to tune.
    static readonly (long painted, int minScale)[] ZoomOutLimits =
    {
        (0, 8),
        (500, 6),
        (2000, 4),
        (10000, 3),
        (50000, 2),
        (250000, 1),
    };

    [SerializeField] Vector2 startPosition = new Vector2(32f, 32f);
    [SerializeField] int startScale = 8;

    Camera cam;
    World world;
    int scaleIndex;
    Vector2 pos;

    int Scale => Scales[scaleIndex];

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        world = FindFirstObjectByType<World>();
        scaleIndex = Mathf.Max(0, System.Array.IndexOf(Scales, startScale));
        pos = startPosition;
        Apply();
    }

    int MinAllowedScale()
    {
        long painted = world != null ? world.PaintedCount : 0;
        int min = ZoomOutLimits[0].minScale;
        foreach (var limit in ZoomOutLimits)
            if (painted >= limit.painted) min = limit.minScale;
        return min;
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 m = mouse.position.ReadValue();
        Vector2 center = new Vector2(Screen.width, Screen.height) * 0.5f;

        // Zoom one step per scroll tick, keeping the point under the cursor fixed
        float scroll = mouse.scroll.ReadValue().y;
        if (scroll != 0f)
        {
            int next = Mathf.Clamp(scaleIndex + (scroll > 0f ? 1 : -1), 0, Scales.Length - 1);
            bool zoomingOut = next < scaleIndex;
            if (next != scaleIndex && !(zoomingOut && Scales[next] < MinAllowedScale()))
            {
                Vector2 under = pos + (m - center) / Scale;
                scaleIndex = next;
                pos = under - (m - center) / Scale;
            }
        }

        // Pan with right or middle mouse drag
        if (mouse.rightButton.isPressed || mouse.middleButton.isPressed)
            pos -= mouse.delta.ReadValue() / Scale;

        Apply();
    }

    void Apply()
    {
        int s = Scale;
        cam.orthographicSize = Screen.height / (2f * s);

        // Snap the camera to the pixel grid (with a half-pixel fix for odd window sizes)
        float fx = (Screen.width % 2) * 0.5f;
        float fy = (Screen.height % 2) * 0.5f;
        float x = (Mathf.Round(pos.x * s - fx) + fx) / s;
        float y = (Mathf.Round(pos.y * s - fy) + fy) / s;
        transform.position = new Vector3(x, y, transform.position.z);
    }
}