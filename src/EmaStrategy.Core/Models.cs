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

public readonly record struct Bar(long Index, DateTime TimeUtc, decimal Open, decimal High, decimal Low, decimal Close);

public sealed class EmaStrategyConfig
{
    public int[] EmaPeriods { get; init; } = new[] { 20, 50, 100 };

    // Minimum time between evaluation attempts (debounce).
    public TimeSpan EvaluationInterval { get; init; } = TimeSpan.FromSeconds(30);

    // Warmup bars required before any evaluation.
    public int WarmupBars { get; init; } = 100;

    public int MaxContracts { get; init; } = 1;

    public decimal OrderQuantityContracts { get; init; } = 1;

    // Distance filter to avoid chasing (0 disables).
    public decimal MinDistanceFromFastEmaPoints { get; init; } = 0m;

    public decimal TakeProfitPoints { get; init; } = 10m;
    public decimal StopLossPoints { get; init; } = 10m;

    public EntryFillMode FillMode { get; init; } = EntryFillMode.NextBarOpen;

    // Cooldown after an entry attempt (also acts as "not multiple orders back-to-back").
    public TimeSpan CooldownAfterOrderPlaced { get; init; } = TimeSpan.FromSeconds(30);

    // Cooldown after an exit.
    public TimeSpan CooldownAfterExit { get; init; } = TimeSpan.FromSeconds(30);
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

