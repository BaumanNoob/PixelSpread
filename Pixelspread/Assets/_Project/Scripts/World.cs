using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class World : MonoBehaviour
{
    readonly Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();
    readonly HashSet<Chunk> touched = new HashSet<Chunk>();
    Camera cam;
    GameHud hud;

    public long PaintedCount { get; private set; }
    public Economy Economy { get; private set; }
    public Upgrades Upgrades { get; private set; }

    void Awake()
    {
        Economy = new Economy();
        Upgrades = new Upgrades(Economy);
    }

    void Start()
    {
        cam = Camera.main;

        hud = FindAnyObjectByType<GameHud>();
        if (hud == null) hud = gameObject.AddComponent<GameHud>();
        hud.Init(this);
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;

        Vector2 p = mouse.position.ReadValue();
        if (hud != null && hud.IsPointerOver(p)) return;

        Vector3 w = cam.ScreenToWorldPoint(new Vector3(p.x, p.y, -cam.transform.position.z));
        PaintBrush(Mathf.FloorToInt(w.x), Mathf.FloorToInt(w.y), 1);
    }

    // Paints the current brush shape around a world pixel. Returns how many pixels changed.
    public int PaintBrush(int cx, int cy, byte tier)
    {
        int changedCount = 0;

        foreach (Vector2Int o in Upgrades.BrushOffsets)
            if (SetPixel(cx + o.x, cy + o.y, tier)) changedCount++;

        FlushTouched();
        return changedCount;
    }

    // World pixel coordinates in, chunk created on demand.
    // Returns true if the pixel actually changed.
    public bool PaintPixel(int px, int py, byte tier)
    {
        bool changed = SetPixel(px, py, tier);
        FlushTouched();
        return changed;
    }

    bool SetPixel(int px, int py, byte tier)
    {
        var coord = new Vector2Int(FloorDiv(px, Chunk.Size), FloorDiv(py, Chunk.Size));

        if (!chunks.TryGetValue(coord, out Chunk chunk))
        {
            chunk = new Chunk(coord, transform);
            chunks[coord] = chunk;
        }

        bool changed = chunk.SetPixel(px - coord.x * Chunk.Size, py - coord.y * Chunk.Size, tier);
        if (changed)
        {
            PaintedCount++;
            Economy.Add(Economy.CoinValue(tier));
            touched.Add(chunk);
        }
        return changed;
    }

    // Upload each changed chunk's texture once, not once per pixel.
    void FlushTouched()
    {
        foreach (Chunk c in touched) c.Apply();
        touched.Clear();
    }

    // Integer division that rounds down, so negative coordinates work.
    static int FloorDiv(int a, int b)
    {
        int q = a / b;
        if (a % b != 0 && a < 0) q--;
        return q;
    }
}