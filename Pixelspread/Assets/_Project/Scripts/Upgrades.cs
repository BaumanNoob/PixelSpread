using System.Collections.Generic;
using UnityEngine;

public class Upgrades
{
    public const int MaxTapLevel = 5;

    // Cost of going from level n to n+1. Roughly 5x each step, placeholders to tune.
    // The first one is 10 coins: about 10 taps, as the doc asks.
    static readonly long[] TapCosts = { 10, 50, 250, 1250, 6250 };

    static readonly string[] BrushNames = { "Single pixel", "Plus", "3x3", "5x5", "7x7", "9x9" };

    readonly Economy economy;
    readonly Vector2Int[][] brushes = new Vector2Int[MaxTapLevel + 1][];

    public int TapLevel { get; private set; }

    public Upgrades(Economy economy)
    {
        this.economy = economy;
        for (int level = 0; level <= MaxTapLevel; level++)
            brushes[level] = BuildBrush(level);
    }

    public bool CanUpgradeTap => TapLevel < MaxTapLevel;
    public long NextTapCost => TapCosts[TapLevel];
    public string CurrentBrushName => BrushNames[TapLevel];
    public string NextBrushName => BrushNames[Mathf.Min(TapLevel + 1, MaxTapLevel)];

    // Offsets around the tapped pixel that get coloured.
    public Vector2Int[] BrushOffsets => brushes[TapLevel];

    public bool TryBuyTapPower()
    {
        if (!CanUpgradeTap) return false;
        if (!economy.TrySpend(NextTapCost)) return false;
        TapLevel++;
        return true;
    }

    // Level 0: one pixel. Level 1: plus. Level 2+: a filled square (3x3, 5x5, 7x7, 9x9).
    static Vector2Int[] BuildBrush(int level)
    {
        var list = new List<Vector2Int>();

        if (level == 0)
        {
            list.Add(Vector2Int.zero);
        }
        else if (level == 1)
        {
            list.Add(new Vector2Int(0, 0));
            list.Add(new Vector2Int(1, 0));
            list.Add(new Vector2Int(-1, 0));
            list.Add(new Vector2Int(0, 1));
            list.Add(new Vector2Int(0, -1));
        }
        else
        {
            int r = level - 1;
            for (int y = -r; y <= r; y++)
                for (int x = -r; x <= r; x++)
                    list.Add(new Vector2Int(x, y));
        }

        return list.ToArray();
    }
}