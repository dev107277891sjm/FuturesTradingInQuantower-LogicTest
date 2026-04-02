namespace EmaStrategy.Core;

public sealed class StrategyEngine
{
    private readonly EmaStrategyConfig _config;
    private readonly EmaSet _emaSet;

    private int _barsSeen;
    private int _evaluationCount;
    private DateTime _nextEvaluationAtUtc;

    private long _tradeIdCounter;

    private Bar? _priorBar;

    private bool _hasPosition;
    private Bias _positionSide;
    private decimal _positionQtyContracts;
    private decimal _entryPrice;
    private DateTime _entryTimeUtc;
    private IReadOnlyDictionary<int, decimal>? _emaAtEntry;

    private int _closedContractsSession;

    private bool _hasPendingOrder;
    private Bias _pendingSide;
    private decimal _pendingQtyContracts;
    private long _pendingTradeId;
    private decimal _pendingEntryPrice;

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
        if (_config.FillMode == EntryFillMode.NextBarOpen && _hasPendingOrder)
            FillPendingOrderAt(bar.TimeUtc);

        _emaSet.Update(bar.Close);
        _barsSeen++;

        var timeUtc = bar.TimeUtc;
        var emaValues = _emaSet.GetValues();
        var price = bar.Close;

        var cloud = EmaCloudLogic.DetermineCloudColor(emaValues, _config.CloudFastPeriod, _config.CloudSlowPeriod);
        var bias = EmaCloudLogic.DetermineBias(emaValues);

        var cooldownActive = timeUtc < _nextEvaluationAtUtc;

        if (_barsSeen < _config.WarmupBars)
        {
            EmitEvaluation(bar, price, cloud, Bias.None, emaValues, cooldownActive,
                "History not loaded (warmup not complete).");
            TryExitAt(bar, price, emaValues);
            _priorBar = bar;
            return;
        }

        if (cooldownActive)
        {
            EmitEvaluation(bar, price, cloud, bias, emaValues, cooldownActive, "Debounce active.");
            TryExitAt(bar, price, emaValues);
            _priorBar = bar;
            return;
        }

        _evaluationCount++;

        // Chasing filter: too close to reference EMA — skip new entries / adds this bar.
        if (_config.MinDistanceFromEmaPoints > 0 &&
            emaValues.TryGetValue(_config.ChasingReferenceEmaPeriod, out var chaseEma))
        {
            var distance = Math.Abs(price - chaseEma);
            if (distance < _config.MinDistanceFromEmaPoints)
            {
                EmitEvaluation(bar, price, cloud, Bias.None, emaValues, false,
                    $"Too close to {_config.ChasingReferenceEmaPeriod} EMA (< {_config.MinDistanceFromEmaPoints} pts); skip entry.");
                TryExitAt(bar, price, emaValues);
                _priorBar = bar;
                _nextEvaluationAtUtc = timeUtc.Add(_config.CooldownAfterOrderPlaced);
                return;
            }
        }

        if (_closedContractsSession >= _config.MaxClosedContractsPerSession)
        {
            EmitEvaluation(bar, price, cloud, bias, emaValues, false, "Max closed contracts this session reached.");
            TryExitAt(bar, price, emaValues);
            _priorBar = bar;
            _nextEvaluationAtUtc = timeUtc.Add(_config.CooldownAfterOrderPlaced);
            return;
        }

        if (_priorBar is null)
        {
            EmitEvaluation(bar, price, cloud, bias, emaValues, false, "No prior candle yet for entry price.");
            TryExitAt(bar, price, emaValues);
            _priorBar = bar;
            _nextEvaluationAtUtc = timeUtc.Add(_config.CooldownAfterOrderPlaced);
            return;
        }

        var priorPrice = GetPriorCandlePrice(_priorBar.Value, _config.EntryPriceField);

        if (bias is Bias.Buy or Bias.Sell)
        {
            if (_hasPosition)
            {
                if (_positionSide != bias)
                {
                    EmitEvaluation(bar, price, cloud, bias, emaValues, false,
                        "Opposite bias while in position (no auto-reverse in test engine).");
                }
                else if (_config.AllowAverageDown)
                {
                    var room = _config.MaxOpenContracts - _positionQtyContracts;
                    if (room > 0 && _config.OrderQuantityContracts > 0)
                    {
                        var addQty = Math.Min(_config.OrderQuantityContracts, room);
                        var tid = ++_tradeIdCounter;
                        AddOrEnterPosition(tid, bias, addQty, timeUtc, priorPrice, emaValues);
                        EmitEvaluation(bar, price, cloud, bias, emaValues, false,
                            $"Average down at prior-candle {_config.EntryPriceField} ({priorPrice:0.####}).");
                        _nextEvaluationAtUtc = timeUtc.Add(_config.CooldownAfterOrderPlaced);
                        TryExitAt(bar, price, emaValues);
                        _priorBar = bar;
                        return;
                    }

                    EmitEvaluation(bar, price, cloud, bias, emaValues, false,
                        "Same-side position; max contracts or average-down blocked.");
                }
                else
                {
                    EmitEvaluation(bar, price, cloud, bias, emaValues, false,
                        "Same-side position already open (average down off).");
                }

                TryExitAt(bar, price, emaValues);
                _priorBar = bar;
                _nextEvaluationAtUtc = timeUtc.Add(_config.CooldownAfterOrderPlaced);
                return;
            }

            if (_hasPendingOrder)
            {
                EmitEvaluation(bar, price, cloud, Bias.None, emaValues, false, "Pending order prevents new entry.");
                TryExitAt(bar, price, emaValues);
                _priorBar = bar;
                _nextEvaluationAtUtc = timeUtc.Add(_config.CooldownAfterOrderPlaced);
                return;
            }

            if (_config.OrderQuantityContracts > _config.MaxOpenContracts)
            {
                EmitEvaluation(bar, price, cloud, Bias.None, emaValues, false,
                    "Order qty exceeds max open contracts.");
                TryExitAt(bar, price, emaValues);
                _priorBar = bar;
                _nextEvaluationAtUtc = timeUtc.Add(_config.CooldownAfterOrderPlaced);
                return;
            }

            TryOpenOrAdd(bias, priorPrice, timeUtc, emaValues, _config.OrderQuantityContracts, bar, cloud);
            _priorBar = bar;
            return;
        }

        if (_hasPosition)
            EmitEvaluation(bar, price, cloud, Bias.None, emaValues, false, "In position; waiting for exit rule.");
        else
            EmitEvaluation(bar, price, cloud, Bias.None, emaValues, false,
                bias == Bias.None ? "No trade signal (EMA stack not aligned)." : "No entry.");

        _nextEvaluationAtUtc = timeUtc.Add(_config.CooldownAfterOrderPlaced);
        TryExitAt(bar, price, emaValues);
        _priorBar = bar;
    }

    private void TryOpenOrAdd(
        Bias bias,
        decimal entryPrice,
        DateTime timeUtc,
        IReadOnlyDictionary<int, decimal> emaValues,
        decimal qtyContracts,
        Bar bar,
        CloudColor cloud)
    {
        if (qtyContracts <= 0)
            return;

        var tradeId = ++_tradeIdCounter;
        _pendingEntryPrice = entryPrice;

        if (_config.FillMode == EntryFillMode.SignalBarClose)
        {
            AddOrEnterPosition(tradeId, bias, qtyContracts, timeUtc, entryPrice, emaValues);
            EmitEvaluation(bar, bar.Close, cloud, bias, emaValues, false,
                $"Filled at prior-candle {_config.EntryPriceField} ({entryPrice:0.####}).");
        }
        else
        {
            _hasPendingOrder = true;
            _pendingSide = bias;
            _pendingQtyContracts = qtyContracts;
            _pendingTradeId = tradeId;
            EmitEvaluation(bar, bar.Close, cloud, bias, emaValues, false,
                $"Order placed; fill at prior-candle {_config.EntryPriceField} ({entryPrice:0.####}) on next bar.");
        }

        _nextEvaluationAtUtc = timeUtc.Add(_config.CooldownAfterOrderPlaced);
        TryExitAt(bar, bar.Close, emaValues);
    }

    private void FillPendingOrderAt(DateTime fillTimeUtc)
    {
        if (_pendingQtyContracts <= 0)
        {
            _hasPendingOrder = false;
            return;
        }

        _hasPendingOrder = false;
        var emaValues = _emaSet.GetValues();
        AddOrEnterPosition(_pendingTradeId, _pendingSide, _pendingQtyContracts, fillTimeUtc, _pendingEntryPrice, emaValues);
        _pendingQtyContracts = 0;
    }

    private void AddOrEnterPosition(
        long tradeId,
        Bias side,
        decimal qtyContracts,
        DateTime entryTimeUtc,
        decimal entryPrice,
        IReadOnlyDictionary<int, decimal> emaValues)
    {
        if (_hasPosition && _positionSide == side)
        {
            var totalQty = _positionQtyContracts + qtyContracts;
            _entryPrice = (_entryPrice * _positionQtyContracts + entryPrice * qtyContracts) / totalQty;
            _positionQtyContracts = totalQty;
            _entryTimeUtc = entryTimeUtc;
            _emaAtEntry = emaValues.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            TradeEntry?.Invoke(new TradeEntrySnapshot(
                tradeId,
                side,
                qtyContracts,
                entryTimeUtc,
                entryPrice,
                _emaAtEntry));
            return;
        }

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

    private void TryExitAt(Bar bar, decimal price, IReadOnlyDictionary<int, decimal> emaValues)
    {
        if (!_hasPosition)
            return;

        var tpPts = GetTakeProfitDistancePoints();
        var slPts = GetStopLossDistancePoints();

        if (_positionSide == Bias.Buy)
        {
            var hitTp = price >= _entryPrice + tpPts;
            var hitSl = _config.StopLossMode == StopLossMode.Ema
                ? emaValues.TryGetValue(_config.StopLossAnchorEmaPeriod, out var eSl) && price <= eSl
                : price <= _entryPrice - slPts;

            if (hitTp || hitSl)
                ExitPosition(bar.TimeUtc, price, emaValues);
        }
        else if (_positionSide == Bias.Sell)
        {
            var hitTp = price <= _entryPrice - tpPts;
            var hitSl = _config.StopLossMode == StopLossMode.Ema
                ? emaValues.TryGetValue(_config.StopLossAnchorEmaPeriod, out var eSl) && price >= eSl
                : price >= _entryPrice + slPts;

            if (hitTp || hitSl)
                ExitPosition(bar.TimeUtc, price, emaValues);
        }
    }

    private decimal GetTakeProfitDistancePoints()
    {
        if (_config.TakeProfitMode == TakeProfitMode.Points)
            return _config.TakeProfitPoints;

        var denom = _positionQtyContracts * _config.DollarsPerPointPerContract;
        if (denom <= 0)
            return _config.TakeProfitPoints;
        return _config.TakeProfitDollars / denom;
    }

    private decimal GetStopLossDistancePoints()
    {
        if (_config.StopLossMode == StopLossMode.Points)
            return _config.StopLossPoints;
        if (_config.StopLossMode == StopLossMode.Dollars)
        {
            var denom = _positionQtyContracts * _config.DollarsPerPointPerContract;
            if (denom <= 0)
                return _config.StopLossPoints;
            return _config.StopLossDollars / denom;
        }
        return 0m;
    }

    private void ExitPosition(DateTime exitTimeUtc, decimal exitPrice, IReadOnlyDictionary<int, decimal> emaValues)
    {
        var pointMove = _positionSide == Bias.Buy
            ? (exitPrice - _entryPrice)
            : (_entryPrice - exitPrice);

        var pnlDollars = pointMove * _positionQtyContracts * _config.DollarsPerPointPerContract;

        var tradeId = _tradeIdCounter;

        TradeExit?.Invoke(new TradeExitSnapshot(
            tradeId,
            _positionSide,
            _positionQtyContracts,
            _entryTimeUtc,
            _entryPrice,
            exitTimeUtc,
            exitPrice,
            pnlDollars,
            emaValues.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));

        _closedContractsSession += (int)Math.Min(int.MaxValue - _closedContractsSession, Math.Round(_positionQtyContracts, MidpointRounding.AwayFromZero));

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
        var fast = _config.EmaPeriods.Length > 0 ? _config.EmaPeriods[0] : 20;
        Evaluation?.Invoke(new EvaluationSnapshot(
            bar.TimeUtc,
            bar.Index,
            _evaluationCount,
            price,
            cloudColor,
            bias,
            ComputeDeviationPct(price, emaValues, fast),
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

    private static decimal GetPriorCandlePrice(Bar b, PriorCandlePriceField field)
    {
        return field switch
        {
            PriorCandlePriceField.Open => b.Open,
            PriorCandlePriceField.Close => b.Close,
            PriorCandlePriceField.High => b.High,
            PriorCandlePriceField.Low => b.Low,
            PriorCandlePriceField.Mid => (b.High + b.Low) / 2m,
            _ => b.Close
        };
    }
}
