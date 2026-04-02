namespace EmaStrategy.Core;

public sealed class StrategyEngine
{
    private readonly EmaStrategyConfig _config;
    private readonly EmaSet _emaSet;

    private int _barsSeen;
    private int _evaluationCount;
    private DateTime _nextEvaluationAtUtc;

    private long _tradeIdCounter;

    private bool _hasPosition;
    private Bias _positionSide;
    private decimal _positionQtyContracts;
    private decimal _entryPrice;
    private DateTime _entryTimeUtc;
    private IReadOnlyDictionary<int, decimal>? _emaAtEntry;

    private bool _hasPendingOrder;
    private Bias _pendingSide;
    private decimal _pendingQtyContracts;
    private DateTime _pendingPlacedAtUtc;

    public event Action<EvaluationSnapshot>? Evaluation;
    public event Action<TradeEntrySnapshot>? TradeEntry;
    public event Action<TradeExitSnapshot>? TradeExit;

    public StrategyEngine(EmaStrategyConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _emaSet = new EmaSet(_config.EmaPeriods);

        _barsSeen = 0;
        _evaluationCount = 0;
        _nextEvaluationAtUtc = DateTime.MinValue;

        _tradeIdCounter = 0;
    }

    public void ProcessBar(Bar bar)
    {
        // If an entry was placed on a previous bar and we want to fill at the next bar open,
        // we consume it at the start of the current bar.
        if (_config.FillMode == EntryFillMode.NextBarOpen && _hasPendingOrder)
        {
            FillPendingOrderAt(bar.TimeUtc, bar.Open);
        }

        // Update EMA values using the latest close.
        _emaSet.Update(bar.Close);
        _barsSeen++;

        var timeUtc = bar.TimeUtc;
        var emaValues = _emaSet.GetValues();
        var price = bar.Close;

        var cloud = EmaCloudLogic.DetermineCloudColor(emaValues);
        var bias = EmaCloudLogic.DetermineBias(emaValues);

        var cooldownActive =
            timeUtc < _nextEvaluationAtUtc;

        if (_barsSeen < _config.WarmupBars)
        {
            EmitEvaluation(bar, price, cloud, Bias.None, emaValues, cooldownActive,
                "History not loaded (warmup not complete).");

            // Even when in warmup, we should still allow exits if a position exists
            // (useful in sim runs); but by design we don't open trades until warmup is done.
            TryExitAt(bar, price, emaValues);
            return;
        }

        if (cooldownActive)
        {
            EmitEvaluation(bar, price, cloud, bias, emaValues, cooldownActive,
                "Debounce active.");
            TryExitAt(bar, price, emaValues);
            return;
        }

        _evaluationCount++;

        var deviationPct = ComputeDeviationPct(price, emaValues, _config.EmaPeriods[0]);

        // Apply distance filter to avoid chasing.
        if (_config.MinDistanceFromFastEmaPoints > 0)
        {
            var fastPeriod = _config.EmaPeriods[0];
            if (emaValues.TryGetValue(fastPeriod, out var fastEma))
            {
                var distance = Math.Abs(price - fastEma);
                if (distance < _config.MinDistanceFromFastEmaPoints)
                {
                    EmitEvaluation(bar, price, cloud, Bias.None, emaValues, false,
                        $"Distance from fast EMA too small (< {_config.MinDistanceFromFastEmaPoints} points).");
                    TryExitAt(bar, price, emaValues);
                    _nextEvaluationAtUtc = timeUtc.Add(_config.CooldownAfterOrderPlaced);
                    return;
                }
            }
        }

        // Decide whether we can enter given current position state.
        if (!_hasPosition && !_hasPendingOrder && bias is Bias.Buy or Bias.Sell)
        {
            var allowed = CanEnterSide(bias);
            if (!allowed)
            {
                EmitEvaluation(bar, price, cloud, Bias.None, emaValues, false,
                    "Entry blocked by position rules.");
                TryExitAt(bar, price, emaValues);
                _nextEvaluationAtUtc = timeUtc.Add(_config.CooldownAfterOrderPlaced);
                return;
            }

            if (_positionSide == bias)
            {
                // Same side already open; in this simplified engine we don't pyramid.
                EmitEvaluation(bar, price, cloud, bias, emaValues, false,
                    "Same-side position already open (no pyramid).");
                TryExitAt(bar, price, emaValues);
                _nextEvaluationAtUtc = timeUtc.Add(_config.CooldownAfterOrderPlaced);
                return;
            }

            if (_config.MaxContracts > 0 && _config.OrderQuantityContracts > _config.MaxContracts)
            {
                EmitEvaluation(bar, price, cloud, Bias.None, emaValues, false,
                    "Max contracts reached (configured quantity exceeds max).");
                TryExitAt(bar, price, emaValues);
                _nextEvaluationAtUtc = timeUtc.Add(_config.CooldownAfterOrderPlaced);
                return;
            }

            // Place order (fill either immediately or at next bar open).
            var tradeId = ++_tradeIdCounter;
            _hasPendingOrder = true;
            _pendingSide = bias;
            _pendingQtyContracts = _config.OrderQuantityContracts;
            _pendingPlacedAtUtc = timeUtc;

            if (_config.FillMode == EntryFillMode.SignalBarClose)
            {
                // Immediate fill at bar close using the "signal bar" price.
                _hasPendingOrder = false;
                EnterPosition(tradeId, bias, _pendingQtyContracts, timeUtc, price, emaValues);
                EmitEvaluation(bar, price, cloud, bias, emaValues, false,
                    "Order filled (signal bar close).");
            }
            else
            {
                EmitEvaluation(bar, price, cloud, bias, emaValues, false,
                    "Order placed (will fill on next bar open).");
            }

            if (_config.FillMode == EntryFillMode.NextBarOpen)
            {
                _nextEvaluationAtUtc = timeUtc.Add(_config.CooldownAfterOrderPlaced);
            }
            else
            {
                _nextEvaluationAtUtc = timeUtc.Add(_config.CooldownAfterOrderPlaced);
            }

            // Exit logic still applies on the same bar (e.g., if tp/sl is tiny).
            TryExitAt(bar, price, emaValues);
            return;
        }

        // If we're flat but bias is None, or we're already in position, log it.
        if (_hasPosition)
        {
            EmitEvaluation(bar, price, cloud, Bias.None, emaValues, false,
                "In position; waiting for tp/sl or exit rule.");
        }
        else
        {
            EmitEvaluation(bar, price, cloud, Bias.None, emaValues, false,
                bias == Bias.None ? "No trade signal (EMA ordering not met)." : "Pending order state prevents entry.");
        }

        _nextEvaluationAtUtc = timeUtc.Add(_config.CooldownAfterOrderPlaced);
        TryExitAt(bar, price, emaValues);
    }

    private bool CanEnterSide(Bias bias)
    {
        // In this simplified engine, we only allow entering when we're flat.
        // (A more complete engine could support reversal/hedging.)
        if (_hasPosition)
            return false;

        if (_hasPendingOrder)
            return false;

        if (bias is Bias.Buy or Bias.Sell)
            return true;

        return false;
    }

    private void EnterPosition(
        long tradeId,
        Bias side,
        decimal qtyContracts,
        DateTime entryTimeUtc,
        decimal entryPrice,
        IReadOnlyDictionary<int, decimal> emaValues)
    {
        _hasPosition = true;
        _positionSide = side;
        _positionQtyContracts = qtyContracts;
        _entryTimeUtc = entryTimeUtc;
        _entryPrice = entryPrice;
        _emaAtEntry = emaValues.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        TradeEntry?.Invoke(new TradeEntrySnapshot(
            tradeId,
            side,
            qtyContracts,
            entryTimeUtc,
            entryPrice,
            _emaAtEntry));
    }

    private void FillPendingOrderAt(DateTime fillTimeUtc, decimal fillPrice)
    {
        // If we fill pending orders at the next bar open, the trade ID is already incremented when placed.
        // For simplicity, we derive "last trade id" from the counter without tracking it separately.
        if (_pendingQtyContracts <= 0)
        {
            _hasPendingOrder = false;
            return;
        }

        var tradeId = _tradeIdCounter;
        _hasPendingOrder = false;
        EnterPosition(tradeId, _pendingSide, _pendingQtyContracts, fillTimeUtc, fillPrice, _emaSet.GetValues());
        _pendingQtyContracts = 0;
    }

    private void TryExitAt(Bar bar, decimal price, IReadOnlyDictionary<int, decimal> emaValues)
    {
        if (!_hasPosition)
            return;

        var tp = _config.TakeProfitPoints;
        var sl = _config.StopLossPoints;

        if (_positionSide == Bias.Buy)
        {
            var takeProfitLevel = _entryPrice + tp;
            var stopLossLevel = _entryPrice - sl;

            if (price >= takeProfitLevel || price <= stopLossLevel)
            {
                ExitPosition(bar.TimeUtc, price, emaValues);
            }
        }
        else if (_positionSide == Bias.Sell)
        {
            var takeProfitLevel = _entryPrice - tp;
            var stopLossLevel = _entryPrice + sl;

            if (price <= takeProfitLevel || price >= stopLossLevel)
            {
                ExitPosition(bar.TimeUtc, price, emaValues);
            }
        }
    }

    private void ExitPosition(DateTime exitTimeUtc, decimal exitPrice, IReadOnlyDictionary<int, decimal> emaValues)
    {
        // Profit/Loss in "points" (not dollarized).
        var pnl = _positionSide == Bias.Buy
            ? (exitPrice - _entryPrice) * _positionQtyContracts
            : (_entryPrice - exitPrice) * _positionQtyContracts;

        var tradeId = _tradeIdCounter;

        TradeExit?.Invoke(new TradeExitSnapshot(
            tradeId,
            _positionSide,
            _positionQtyContracts,
            _entryTimeUtc,
            _entryPrice,
            exitTimeUtc,
            exitPrice,
            pnl,
            emaValues.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));

        _hasPosition = false;
        _positionQtyContracts = 0;
        _entryPrice = 0;
        _entryTimeUtc = default;
        _emaAtEntry = null;

        _nextEvaluationAtUtc = exitTimeUtc.Add(_config.CooldownAfterExit);
    }

    private void EmitEvaluation(
        Bar bar,
        decimal price,
        CloudColor cloudColor,
        Bias bias,
        IReadOnlyDictionary<int, decimal> emaValues,
        bool cooldownActive,
        string reason)
    {
        Evaluation?.Invoke(new EvaluationSnapshot(
            bar.TimeUtc,
            bar.Index,
            _evaluationCount,
            price,
            cloudColor,
            bias,
            ComputeDeviationPct(price, emaValues, _config.EmaPeriods[0]),
            emaValues.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            _hasPendingOrder,
            cooldownActive,
            reason));
    }

    private static decimal ComputeDeviationPct(decimal price, IReadOnlyDictionary<int, decimal> emaValues, int fastPeriod)
    {
        if (!emaValues.TryGetValue(fastPeriod, out var fastEma) || fastEma == 0)
            return 0m;

        var diff = price - fastEma;
        return diff / fastEma;
    }
}

