using System.Globalization;

public class Economy
{
    // Coins paid when a pixel becomes this tier. Index = tier (0 = empty).
    // Matches the doc: Black 1, Grey 2, Blue 5, Green 12, Red 30. Placeholder values, tune in playtests.
    static readonly int[] CoinsPerTier = { 0, 1, 2, 5, 12, 30 };

    public long Coins { get; private set; }

    public static int CoinValue(byte tier)
    {
        if (tier < CoinsPerTier.Length) return CoinsPerTier[tier];
        return CoinsPerTier[CoinsPerTier.Length - 1];
    }

    public void Add(long amount)
    {
        if (amount > 0) Coins += amount;
    }

    public bool TrySpend(long cost)
    {
        if (Coins < cost) return false;
        Coins -= cost;
        return true;
    }

    // 1234 -> 1.23K, 5000000 -> 5M, and so on.
    public static string Format(long n)
    {
        if (n < 1000) return n.ToString(CultureInfo.InvariantCulture);

        string[] suffixes = { "", "K", "M", "B", "T", "Q" };
        double v = n;
        int i = 0;
        while (v >= 1000.0 && i < suffixes.Length - 1)
        {
            v /= 1000.0;
            i++;
        }
        return v.ToString("0.##", CultureInfo.InvariantCulture) + suffixes[i];
    }
}