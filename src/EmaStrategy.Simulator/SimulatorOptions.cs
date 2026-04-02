using EmaStrategy.Core;

namespace EmaStrategy.Simulator;

/// <summary>
/// User-configurable options for the offline simulator (UI or console).
/// </summary>
public sealed class SimulatorOptions
{
    /// <summary>Display name used for output subfolder (e.g. MNQ). Not a live instrument subscription.</summary>
    public string Symbol { get; init; } = "MNQ";

    /// <summary>Repository root; used to resolve the default bar-history CSV when <see cref="HistoryCsvPath"/> is empty.</summary>
    public string? RepoRoot { get; init; }

    /// <summary>Optional path to bar history CSV (CloseTime, Open, High, Low, Close). Empty uses default under repo <c>.temp/_srv/MNQ_50k_Bar_History.csv</c>.</summary>
    public string HistoryCsvPath { get; init; } = "";

    /// <summary>Comma-separated EMA periods, e.g. "20,50,100".</summary>
    public string EmaPeriodsText { get; init; } = "20,50,100";

    public int CloudFastPeriod { get; init; } = 20;

    public int CloudSlowPeriod { get; init; } = 50;

    public int WarmupBars { get; init; } = 120;

    public int MaxOpenContracts { get; init; } = 1;

    public int MaxClosedContractsPerSession { get; init; } = int.MaxValue;

    public decimal OrderQuantityContracts { get; init; } = 1m;

    public PriorCandlePriceField EntryPriceField { get; init; } = PriorCandlePriceField.Close;

    public int ChasingReferenceEmaPeriod { get; init; } = 20;

    public decimal MinDistanceFromEmaPoints { get; init; } = 0m;

    public TakeProfitMode TakeProfitMode { get; init; } = TakeProfitMode.Points;

    public decimal TakeProfitPoints { get; init; } = 10m;

    public decimal TakeProfitDollars { get; init; } = 20m;

    public StopLossMode StopLossMode { get; init; } = StopLossMode.Points;

    public decimal StopLossPoints { get; init; } = 10m;

    public decimal StopLossDollars { get; init; } = 20m;

    public int StopLossAnchorEmaPeriod { get; init; } = 50;

    public decimal DollarsPerPointPerContract { get; init; } = 2m;

    public int EvaluationIntervalSeconds { get; init; } = 30;

    public int CooldownAfterOrderSeconds { get; init; } = 30;

    public int CooldownAfterExitSeconds { get; init; } = 30;

    public EntryFillMode FillMode { get; init; } = EntryFillMode.NextBarOpen;

    public bool AllowAverageDown { get; init; } = false;

    /// <summary>When history CSV is empty or missing, generate this many synthetic bars.</summary>
    public int SyntheticBarCount { get; init; } = 600;

    public decimal SyntheticStartPrice { get; init; } = 24000m;

    public int[] ParseEmaPeriods()
    {
        var parts = EmaPeriodsText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var list = new List<int>();
        foreach (var p in parts)
        {
            if (int.TryParse(p, out var n) && n > 0)
                list.Add(n);
        }
        return list.Count > 0 ? list.ToArray() : new[] { 20, 50, 100 };
    }

    public EmaStrategyConfig ToConfig()
    {
        return new EmaStrategyConfig
        {
            EmaPeriods = ParseEmaPeriods(),
            CloudFastPeriod = CloudFastPeriod,
            CloudSlowPeriod = CloudSlowPeriod,
            WarmupBars = WarmupBars,
            EvaluationInterval = TimeSpan.FromSeconds(EvaluationIntervalSeconds),
            MaxOpenContracts = MaxOpenContracts,
            MaxClosedContractsPerSession = MaxClosedContractsPerSession,
            OrderQuantityContracts = OrderQuantityContracts,
            EntryPriceField = EntryPriceField,
            ChasingReferenceEmaPeriod = ChasingReferenceEmaPeriod,
            MinDistanceFromEmaPoints = MinDistanceFromEmaPoints,
            TakeProfitMode = TakeProfitMode,
            TakeProfitPoints = TakeProfitPoints,
            TakeProfitDollars = TakeProfitDollars,
            StopLossMode = StopLossMode,
            StopLossPoints = StopLossPoints,
            StopLossDollars = StopLossDollars,
            StopLossAnchorEmaPeriod = StopLossAnchorEmaPeriod,
            DollarsPerPointPerContract = DollarsPerPointPerContract,
            FillMode = FillMode,
            CooldownAfterOrderPlaced = TimeSpan.FromSeconds(CooldownAfterOrderSeconds),
            CooldownAfterExit = TimeSpan.FromSeconds(CooldownAfterExitSeconds),
            AllowAverageDown = AllowAverageDown
        };
    }
}
