using UnityEngine;

// One 64x64 block of pixels. A plain class: it owns its data and its texture.
public class Chunk
{
    public const int Size = 64;

    // Tier 0 = empty (transparent), tier 1 = black. More tiers come later.
    static readonly Color32[] Palette =
    {
        new Color32(0, 0, 0, 0),
        new Color32(0, 0, 0, 255),
    };

    public readonly Vector2Int Coord;

    readonly byte[] tiers = new byte[Size * Size];
    readonly Color32[] colors = new Color32[Size * Size];
    readonly Texture2D tex;
    bool dirty;

    public Chunk(Vector2Int coord, Transform parent)
    {
        Coord = coord;

        tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels32(colors);
        tex.Apply();

        var go = new GameObject($"Chunk {coord.x},{coord.y}");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(coord.x * Size, coord.y * Size, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, Size, Size), Vector2.zero, 1f);
    }

    // Returns true only if the pixel actually changed (used for coins later).
    public bool SetPixel(int x, int y, byte tier)
    {
        int i = y * Size + x;
        if (tiers[i] == tier) return false;

        tiers[i] = tier;
        colors[i] = Palette[tier];
        dirty = true;
        return true;
    }

    public void Apply()
    {
        if (!dirty) return;
        tex.SetPixels32(colors);
        tex.Apply();
        dirty = false;
    }
}