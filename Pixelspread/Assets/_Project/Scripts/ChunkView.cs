using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class ChunkView : MonoBehaviour
{
    const int Size = 64;

    Texture2D tex;
    Camera cam;

    void Start()
    {
        cam = Camera.main;

        tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        var pixels = new Color32[Size * Size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(255, 255, 255, 255);
        tex.SetPixels32(pixels);
        tex.Apply();

        GetComponent<SpriteRenderer>().sprite =
            Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 1f);
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;

        Vector2 p = mouse.position.ReadValue();
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(p.x, p.y, -cam.transform.position.z));
        Vector3 local = transform.InverseTransformPoint(world);

        int x = Mathf.FloorToInt(local.x + Size / 2f);
        int y = Mathf.FloorToInt(local.y + Size / 2f);
        if (x < 0 || x >= Size || y < 0 || y >= Size) return;

        tex.SetPixel(x, y, Color.black);
        tex.Apply();
    }
}