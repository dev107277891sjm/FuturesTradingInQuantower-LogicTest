using EmaStrategy.Core;

namespace EmaStrategy.Tests;

public class UnitTest1
{
    [Fact]
    public void EmaSet_UpdatesWithClassicFormula()
    {
        // For period=3, k=2/(3+1)=0.5
        var ema = new EmaSet(new[] { 3 });

        ema.Update(1m); // seed = 1
        ema.Update(2m);
        ema.Update(3m);
        ema.Update(4m);

        // Expected:
        // ema1=1
        // ema2=2*0.5 + 1*0.5 = 1.5
        // ema3=3*0.5 + 1.5*0.5 = 2.25
        // ema4=4*0.5 + 2.25*0.5 = 3.125
        var values = ema.GetValues();
        Assert.True(values.TryGetValue(3, out var v));
        Assert.Equal(3.125m, v);
    }

    [Fact]
    public void EmaCloudLogic_BiasAndCloudAreIndependent()
    {
        var buyMap = new Dictionary<int, decimal>
        {
            [20] = 5m,
            [50] = 4m,
            [100] = 3m
        };
        Assert.Equal(Bias.Buy, EmaCloudLogic.DetermineBias(buyMap));
        Assert.Equal(CloudColor.Green, EmaCloudLogic.DetermineCloudColor(buyMap, 20, 50));

        var sellMap = new Dictionary<int, decimal>
        {
            [20] = 3m,
            [50] = 4m,
            [100] = 5m
        };
        Assert.Equal(Bias.Sell, EmaCloudLogic.DetermineBias(sellMap));
        Assert.Equal(CloudColor.Red, EmaCloudLogic.DetermineCloudColor(sellMap, 20, 50));

        var noneMap = new Dictionary<int, decimal>
        {
            [20] = 4m,
            [50] = 3m,
            [100] = 3.5m
        };
        Assert.Equal(Bias.None, EmaCloudLogic.DetermineBias(noneMap));
        // Cloud is only 20 vs 50: 4 > 3 → green even though 20/50/100 are not stacked.
        Assert.Equal(CloudColor.Green, EmaCloudLogic.DetermineCloudColor(noneMap, 20, 50));
    }

    [Fact]
    public void EmaCloudLogic_CloudGrayWhenFastEqualsSlow()
    {
        var m = new Dictionary<int, decimal> { [20] = 4m, [50] = 4m, [100] = 3m };
        Assert.Equal(CloudColor.Gray, EmaCloudLogic.DetermineCloudColor(m, 20, 50));
    }
}
