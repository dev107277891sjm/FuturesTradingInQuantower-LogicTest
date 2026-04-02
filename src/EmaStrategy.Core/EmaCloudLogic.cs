namespace EmaStrategy.Core;

public static class EmaCloudLogic
{
    /// <summary>Cloud color from fast vs slow EMA only (e.g. 20 vs 50). Gray if missing or equal.</summary>
    public static CloudColor DetermineCloudColor(
        IReadOnlyDictionary<int, decimal> emaValues,
        int cloudFastPeriod,
        int cloudSlowPeriod)
    {
        if (!emaValues.TryGetValue(cloudFastPeriod, out var fast) ||
            !emaValues.TryGetValue(cloudSlowPeriod, out var slow))
            return CloudColor.Gray;

        if (fast > slow)
            return CloudColor.Green;
        if (fast < slow)
            return CloudColor.Red;
        return CloudColor.Gray;
    }

    public static Bias DetermineBias(IReadOnlyDictionary<int, decimal> emaValues)
    {
        // Periods sorted ascending (20, 50, 100):
        // Buy: EMA20 > EMA50 > EMA100
        // Sell: EMA20 < EMA50 < EMA100  (equivalently 100 > 50 > 20)
        var periods = emaValues.Keys.OrderBy(p => p).ToArray();
        if (periods.Length < 3)
            return Bias.None;

        var orderedEma = periods.Select(p => emaValues[p]).ToArray();

        var isBuy = true;
        for (var i = 1; i < orderedEma.Length; i++)
        {
            if (!(orderedEma[i - 1] > orderedEma[i]))
            {
                isBuy = false;
                break;
            }
        }
        if (isBuy)
            return Bias.Buy;

        var isSell = true;
        for (var i = 1; i < orderedEma.Length; i++)
        {
            if (!(orderedEma[i - 1] < orderedEma[i]))
            {
                isSell = false;
                break;
            }
        }
        return isSell ? Bias.Sell : Bias.None;
    }
}
