namespace EmaStrategy.Core;

public static class EmaCloudLogic
{
    public static CloudColor DetermineCloudColor(IReadOnlyDictionary<int, decimal> emaValues)
    {
        var periods = emaValues.Keys.OrderBy(p => p).ToArray();
        if (periods.Length < 3)
            return CloudColor.Gray;

        var fast = emaValues[periods[0]];
        var mid = emaValues[periods[1]];
        var slow = emaValues[periods[periods.Length - 1]];

        if (fast > mid && mid > slow)
            return CloudColor.Green;
        if (fast < mid && mid < slow)
            return CloudColor.Red;

        return CloudColor.Gray;
    }

    public static Bias DetermineBias(IReadOnlyDictionary<int, decimal> emaValues)
    {
        // Periods are sorted by length (fast -> slow). Then:
        // - Buy: EMA_fast > EMA_mid > ... > EMA_slow (strictly decreasing with period length)
        // - Sell: EMA_fast < EMA_mid < ... < EMA_slow (strictly increasing with period length)
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

