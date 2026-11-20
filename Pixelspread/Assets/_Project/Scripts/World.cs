using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class World : MonoBehaviour
{
    readonly Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();
    Camera cam;
        public long PaintedCount { get; private set; }

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;

        Vector2 p = mouse.position.ReadValue();
        Vector3 w = cam.ScreenToWorldPoint(new Vector3(p.x, p.y, -cam.transform.position.z));
        PaintPixel(Mathf.FloorToInt(w.x), Mathf.FloorToInt(w.y), 1);
    }

    // World pixel coordinates in, chunk created on demand.
    public bool PaintPixel(int px, int py, byte tier)
    {
        var coord = new Vector2Int(FloorDiv(px, Chunk.Size), FloorDiv(py, Chunk.Size));

        if (!chunks.TryGetValue(coord, out Chunk chunk))
        {
            chunk = new Chunk(coord, transform);
            chunks[coord] = chunk;
        }

        bool changed = chunk.SetPixel(px - coord.x * Chunk.Size, py - coord.y * Chunk.Size, tier);
        if (changed) PaintedCount++;
        chunk.Apply();
    }

    // Integer division that rounds down, so negative coordinates work.
    static int FloorDiv(int a, int b)
    {
        int q = a / b;
        if (a % b != 0 && a < 0) q--;
        return q;
    }
}