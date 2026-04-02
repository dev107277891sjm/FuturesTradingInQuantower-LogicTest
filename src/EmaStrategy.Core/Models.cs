namespace EmaStrategy.Core;

public enum Bias
{
    None = 0,
    Buy = 1,
    Sell = 2
}

public enum CloudColor
{
    Gray = 0,
    Green = 1,
    Red = 2
}

public enum EntryFillMode
{
    NextBarOpen = 0,
    SignalBarClose = 1
}

/// <summary>Which price from the prior completed candle to use for entry.</summary>
public enum PriorCandlePriceField
{
    Open = 0,
    Close = 1,
    High = 2,
    Low = 3,
    Mid = 4
}

public enum TakeProfitMode
{
    Points = 0,
    Dollars = 1
}

public enum StopLossMode
{
    Points = 0,
    Dollars = 1,
    Ema = 2
}

public readonly record struct Bar(long Index, DateTime TimeUtc, decimal Open, decimal High, decimal Low, decimal Close);

public sealed class EmaStrategyConfig
{
    /// <summary>Must include 20, 50, 100 for the client strategy (defaults).</summary>
    public int[] EmaPeriods { get; init; } = new[] { 20, 50, 100 };

    public int CloudFastPeriod { get; init; } = 20;

    public int CloudSlowPeriod { get; init; } = 50;

    public TimeSpan EvaluationInterval { get; init; } = TimeSpan.FromSeconds(30);

    public int WarmupBars { get; init; } = 100;

    /// <summary>Max contracts in open position (gross).</summary>
    public int MaxOpenContracts { get; init; } = 1;

    /// <summary>Max cumulative contracts closed in the session (stops new entries when reached).</summary>
    public int MaxClosedContractsPerSession { get; init; } = int.MaxValue;

    public decimal OrderQuantityContracts { get; init; } = 1m;

    public PriorCandlePriceField EntryPriceField { get; init; } = PriorCandlePriceField.Close;

    /// <summary>EMA period used for chasing distance (e.g. 20).</summary>
    public int ChasingReferenceEmaPeriod { get; init; } = 20;

    /// <summary>Minimum distance from reference EMA in points; 0 = off.</summary>
    public decimal MinDistanceFromEmaPoints { get; init; } = 0m;

    public TakeProfitMode TakeProfitMode { get; init; } = TakeProfitMode.Points;

    public decimal TakeProfitPoints { get; init; } = 10m;

    /// <summary>Profit target in dollars (per position leg); converted using DollarsPerPointPerContract.</summary>
    public decimal TakeProfitDollars { get; init; } = 20m;

    public StopLossMode StopLossMode { get; init; } = StopLossMode.Points;

    public decimal StopLossPoints { get; init; } = 10m;

    public decimal StopLossDollars { get; init; } = 20m;

    /// <summary>When StopLossMode is Ema, exit when price crosses this EMA (e.g. 50).</summary>
    public int StopLossAnchorEmaPeriod { get; init; } = 50;

    /// <summary>e.g. MNQ ≈ $2 per index point per contract.</summary>
    public decimal DollarsPerPointPerContract { get; init; } = 2m;

    public EntryFillMode FillMode { get; init; } = EntryFillMode.NextBarOpen;

    public TimeSpan CooldownAfterOrderPlaced { get; init; } = TimeSpan.FromSeconds(30);

    public TimeSpan CooldownAfterExit { get; init; } = TimeSpan.FromSeconds(30);

    public bool AllowAverageDown { get; init; } = false;
}

public sealed record EvaluationSnapshot(
    DateTime TimeUtc,
    long BarIndex,
    int EvaluationCount,
    decimal Price,
    CloudColor CloudColor,
    Bias Bias,
    decimal DeviationPct,
    IReadOnlyDictionary<int, decimal> EmaValues,
    bool HasPendingOrder,
    bool CooldownActive,
    string Reason);

public sealed record TradeSnapshot(
    long TradeId,
    Bias Side,
    decimal QuantityContracts,
    DateTime EntryTimeUtc,
    decimal EntryPrice,
    DateTime ExitTimeUtc,
    decimal ExitPrice,
    decimal ProfitLoss,
    IReadOnlyDictionary<int, decimal> EmaAtEntry);

public sealed record TradeEntrySnapshot(
    long TradeId,
    Bias Side,
    decimal QuantityContracts,
    DateTime EntryTimeUtc,
    decimal EntryPrice,
    IReadOnlyDictionary<int, decimal> EmaAtEntry);

public sealed record TradeExitSnapshot(
    long TradeId,
    Bias Side,
    decimal QuantityContracts,
    DateTime EntryTimeUtc,
    decimal EntryPrice,
    DateTime ExitTimeUtc,
    decimal ExitPrice,
    decimal ProfitLoss,
    IReadOnlyDictionary<int, decimal> EmaAtExit);
