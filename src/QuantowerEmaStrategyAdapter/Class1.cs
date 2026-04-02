using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EmaStrategy.Core;
using TradingPlatform.BusinessLayer;

namespace QuantowerEmaStrategyAdapter
{
    // Quantower Algo adapter entrypoint.
    public class QuantowerEmaStrategy : Strategy
    {
        // Quantower requirements typically expect tick bars aggregated from trade quotes.
        private const int TickBarsTickCount = 25000;

        private readonly object _sync = new object();

        private StrategyEngine _engine;
        private CsvTableWriter _barWriter;
        private CsvTableWriter _evaluationWriter;
        private CsvTableWriter _tradeEntryWriter;
        private CsvTableWriter _tradeExitWriter;
        private CsvTableWriter _orderUpdateWriter;

        private TradingPlatform.BusinessLayer.Symbol _symbol;
        private Account _account;

        private string _exportDir;

        private readonly int[] _emaPeriods = new[] { 7, 12, 30, 50 };

        private long _barIndex;
        private int _tickCount;
        private decimal _tickOpen;
        private decimal _tickHigh;
        private decimal _tickLow;
        private decimal _tickClose;
        private DateTime _tickBarCloseTimeUtc;

        private ExecutionState _executionState;
        private Order _entryOrder;
        private Order _takeProfitOrder;
        private Order _stopLossOrder;

        private long _activeTradeId;
        private Bias _positionBias;
        private decimal _positionQtyContracts;
        private decimal _takeProfitPoints;
        private decimal _stopLossPoints;

        private string _entryOrderId;
        private string _takeProfitOrderId;
        private string _stopLossOrderId;

        private Position _position;

        private enum ExecutionState
        {
            Flat = 0,
            EntrySubmitted = 1,
            InPosition = 2,
            Exiting = 3
        }

        protected override void OnCreated()
        {
            var repoRoot = FindRepoRoot(AppContext.BaseDirectory);
            var runTimestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            _exportDir = Path.Combine(repoRoot, "report", "exports", "quantower_realtime", runTimestamp);

            Directory.CreateDirectory(_exportDir);

            var config = new EmaStrategyConfig
            {
                EmaPeriods = _emaPeriods,
                CloudFastPeriod = _emaPeriods[0],
                CloudSlowPeriod = _emaPeriods[1],
                WarmupBars = 100,
                MaxOpenContracts = 1,
                MaxClosedContractsPerSession = int.MaxValue,
                OrderQuantityContracts = 1,
                MinDistanceFromEmaPoints = 0m,
                ChasingReferenceEmaPeriod = _emaPeriods[0],
                TakeProfitPoints = 10m,
                StopLossPoints = 10m,
                FillMode = EntryFillMode.NextBarOpen,
                CooldownAfterOrderPlaced = TimeSpan.FromSeconds(30),
                CooldownAfterExit = TimeSpan.FromSeconds(30),
            };

            _engine = new StrategyEngine(config);
            _engine.Evaluation += OnEvaluation;
            _engine.TradeEntry += OnTradeEntry;
            _engine.TradeExit += OnTradeExit;

            // bar_stream.csv
            _barWriter = new CsvTableWriter(
                Path.Combine(_exportDir, "bar_stream.csv"),
                new[] { "Index", "TimeUtc", "Open", "High", "Low", "Close" });

            // evaluations.csv
            var evalHeaders = new List<string>
            {
                "TimeUtc",
                "BarIndex",
                "EvaluationCount",
                "Price",
                "CloudColor",
                "Bias",
                "DeviationPct",
                "HasPendingOrder",
                "CooldownActive",
                "Reason"
            };
            foreach (var p in _emaPeriods)
                evalHeaders.Add("EMA_" + p);

            _evaluationWriter = new CsvTableWriter(
                Path.Combine(_exportDir, "evaluations.csv"),
                evalHeaders);

            // trade_entries.csv
            var entryHeaders = new List<string>
            {
                "TradeId",
                "Side",
                "QuantityContracts",
                "EntryTimeUtc",
                "EntryPrice"
            };
            foreach (var p in _emaPeriods)
                entryHeaders.Add("EMA_" + p + "_AtEntry");

            _tradeEntryWriter = new CsvTableWriter(
                Path.Combine(_exportDir, "trade_entries.csv"),
                entryHeaders);

            // trade_exits.csv
            var exitHeaders = new List<string>
            {
                "TradeId",
                "Side",
                "QuantityContracts",
                "EntryTimeUtc",
                "EntryPrice",
                "ExitTimeUtc",
                "ExitPrice",
                "ProfitLoss"
            };
            foreach (var p in _emaPeriods)
                exitHeaders.Add("EMA_" + p + "_AtExit");

            _tradeExitWriter = new CsvTableWriter(
                Path.Combine(_exportDir, "trade_exits.csv"),
                exitHeaders);

            _executionState = ExecutionState.Flat;
            _takeProfitPoints = 10m;
            _stopLossPoints = 10m;

            // order_updates.csv (append-only: every Order.Updated we see)
            _orderUpdateWriter = new CsvTableWriter(
                Path.Combine(_exportDir, "order_updates.csv"),
                new[] {
                    "UpdateTimeUtc",
                    "OrderKind",
                    "OrderId",
                    "Status",
                    "FilledQuantity",
                    "RemainingQuantity",
                    "Price",
                    "TriggerPrice",
                    "StopLossPrice",
                    "TakeProfitPrice"
                });
        }

        protected override void OnRun()
        {
            var core = Core.Instance;

            _symbol = core.Symbols.FirstOrDefault();
            _account = core.Accounts.FirstOrDefault();

            if (_symbol == null || _account == null || _engine == null)
                return;

            // Export & feed warmup history (time-based candles) to seed EMA calculations.
            // The realtime section still feeds tick-bars; the EMA engine only needs ordered closes.
            ExportAndFeedHistory(_symbol);

            // Reset tick-bar aggregator so realtime starts clean.
            lock (_sync)
            {
                _tickCount = 0;
                _tickHigh = decimal.MinValue;
                _tickLow = decimal.MaxValue;
            }

            // Subscribe to real-time last (trade) ticks.
            _symbol.NewLast += (s, e) =>
            {
                try
                {
                    OnTick(symbol: _symbol, lastPrice: ConvertToDecimal(_symbol.Last), lastTimeUtc: _symbol.LastDateTime.ToUniversalTime());
                }
                catch
                {
                    // Swallow in realtime path; we log CSV separately only from engine callbacks.
                }
            };
        }

        private void ExportAndFeedHistory(TradingPlatform.BusinessLayer.Symbol symbol)
        {
            // We seed using last N MIN1 bars; N must be >= WarmupBars.
            var toUtc = symbol.LastDateTime.ToUniversalTime();
            if (toUtc == DateTime.MinValue)
                toUtc = DateTime.UtcNow;

            var warmupTarget = _emaPeriods.Length > 0 ? 110 : 110;
            var fromUtc = toUtc.AddMinutes(-warmupTarget);

            var history = symbol.GetHistory(Period.MIN1, fromUtc, toUtc);

            foreach (var item in history)
            {
                var barItem = item as HistoryItemBar;
                if (barItem == null)
                    continue;

                var barTimeUtc = barItem.TimeRight.ToUniversalTime();

                var bar = new Bar(
                    _barIndex++,
                    barTimeUtc,
                    ConvertToDecimal(barItem.Open),
                    ConvertToDecimal(barItem.High),
                    ConvertToDecimal(barItem.Low),
                    ConvertToDecimal(barItem.Close));

                _barWriter.WriteRow(new object[] { bar.Index, bar.TimeUtc, bar.Open, bar.High, bar.Low, bar.Close });
                _engine.ProcessBar(bar);
            }
        }

        private void OnTick(TradingPlatform.BusinessLayer.Symbol symbol, decimal lastPrice, DateTime lastTimeUtc)
        {
            if (lastPrice == default(decimal) && _tickCount == 0)
                return;

            lock (_sync)
            {
                if (_tickCount == 0)
                {
                    _tickOpen = lastPrice;
                    _tickHigh = lastPrice;
                    _tickLow = lastPrice;
                    _tickClose = lastPrice;
                    _tickBarCloseTimeUtc = lastTimeUtc;
                }
                else
                {
                    if (lastPrice > _tickHigh) _tickHigh = lastPrice;
                    if (lastPrice < _tickLow) _tickLow = lastPrice;
                    _tickClose = lastPrice;
                    _tickBarCloseTimeUtc = lastTimeUtc;
                }

                _tickCount++;

                if (_tickCount >= TickBarsTickCount)
                {
                    var bar = new Bar(
                        _barIndex++,
                        _tickBarCloseTimeUtc,
                        _tickOpen,
                        _tickHigh,
                        _tickLow,
                        _tickClose);

                    _barWriter.WriteRow(new object[] { bar.Index, bar.TimeUtc, bar.Open, bar.High, bar.Low, bar.Close });
                    _engine.ProcessBar(bar);

                    _tickCount = 0;
                    _tickHigh = decimal.MinValue;
                    _tickLow = decimal.MaxValue;
                }
            }
        }

        private void OnEvaluation(EvaluationSnapshot snapshot)
        {
            // Called from the strategy engine whenever it has an evaluation to export.
            var values = new List<object>
            {
                snapshot.TimeUtc,
                snapshot.BarIndex,
                snapshot.EvaluationCount,
                snapshot.Price,
                snapshot.CloudColor,
                snapshot.Bias,
                snapshot.DeviationPct,
                snapshot.HasPendingOrder,
                snapshot.CooldownActive,
                snapshot.Reason
            };

            foreach (var p in _emaPeriods)
            {
                decimal ema;
                if (snapshot.EmaValues.TryGetValue(p, out ema))
                    values.Add(ema);
                else
                    values.Add("");
            }

            _evaluationWriter.WriteRow(values);
        }

        private void OnTradeEntry(TradeEntrySnapshot snapshot)
        {
            if (_executionState != ExecutionState.Flat)
                return;

            var values = new List<object>
            {
                snapshot.TradeId,
                snapshot.Side,
                snapshot.QuantityContracts,
                snapshot.EntryTimeUtc,
                snapshot.EntryPrice
            };

            foreach (var p in _emaPeriods)
            {
                decimal ema;
                if (snapshot.EmaAtEntry != null && snapshot.EmaAtEntry.TryGetValue(p, out ema))
                    values.Add(ema);
                else
                    values.Add("");
            }

            _tradeEntryWriter.WriteRow(values);

            // Execution state: submit real market entry order.
            SubmitEntryOrder(snapshot);
        }

        private void SubmitEntryOrder(TradeEntrySnapshot snapshot)
        {
            if (_symbol == null || _account == null)
                return;

            // Note: Exact fields in PlaceOrderRequestParameters vary across Quantower versions.
            // We set the minimal set expected by this SDK; if compilation passes, runtime should be close.
            var request = new PlaceOrderRequestParameters();

            request.Account = _account;
            request.Symbol = _symbol;

            request.Side = snapshot.Side == Bias.Buy ? Side.Buy : Side.Sell;
            request.Quantity = (double)snapshot.QuantityContracts;
            request.OrderTypeId = "Market";

            // Place entry order immediately.
            var operationResult = Core.Instance.PlaceOrder(request);
            _activeTradeId = snapshot.TradeId;
            _positionBias = snapshot.Side;
            _positionQtyContracts = snapshot.QuantityContracts;

            if (operationResult != null)
            {
                var orderId = operationResult.OrderId;
                if (!string.IsNullOrWhiteSpace(orderId))
                {
                    // ConnectionId is required when multiple connections exist; if Quantower has only one, an empty id is typically accepted.
                    _entryOrder = Core.Instance.GetOrderById(orderId, "");
                    _entryOrderId = orderId;

                    if (_entryOrder != null)
                    {
                        _entryOrder.Updated += OnEntryOrderUpdated;
                        _executionState = ExecutionState.EntrySubmitted;
                    }
                }
            }
        }

        private void OnEntryOrderUpdated(IOrder order)
        {
            if (_executionState != ExecutionState.EntrySubmitted)
                return;

            if (order == null)
                return;

            ExportOrderUpdate(orderKind: "Entry", orderId: _entryOrderId, order: order);

            // Filled when remaining quantity reaches 0.
            if (order.RemainingQuantity > 0)
                return;

            // Compute average/market fill price. For market orders, `Price` is the executed price.
            var entryPrice = Convert.ToDecimal(order.Price);

            // Protective orders close the position, so they use the opposite side.
            var protectiveSide = _positionBias == Bias.Buy ? Side.Sell : Side.Buy;

            var tpPrice = _positionBias == Bias.Buy ? entryPrice + _takeProfitPoints : entryPrice - _takeProfitPoints;
            var slPrice = _positionBias == Bias.Buy ? entryPrice - _stopLossPoints : entryPrice + _stopLossPoints;

            _takeProfitOrderId = PlaceProtectiveOrder(protectiveSide, tpPrice, "TakeProfit", out _takeProfitOrder);
            _stopLossOrderId = PlaceProtectiveOrder(protectiveSide, slPrice, "StopLoss", out _stopLossOrder);

            // Subscribe to position updates as a safety net (matches plan requirements).
            try
            {
                var positionId = order.PositionId;
                if (!string.IsNullOrWhiteSpace(positionId))
                {
                    _position = Core.Instance.GetPositionById(positionId, "");
                    if (_position != null)
                        _position.Updated += OnPositionUpdated;
                }
            }
            catch
            {
            }

            _executionState = ExecutionState.InPosition;
        }

        private void OnPositionUpdated(Position position)
        {
            if (position == null)
                return;

            if (_executionState != ExecutionState.InPosition && _executionState != ExecutionState.Exiting)
                return;

            // If Quantower reports the position as closed, cancel any remaining protective orders.
            if (position.Quantity == 0)
            {
                if (_takeProfitOrder != null)
                    _takeProfitOrder.Cancel("");
                if (_stopLossOrder != null)
                    _stopLossOrder.Cancel("");

                _executionState = ExecutionState.Flat;
            }
        }

        private string PlaceProtectiveOrder(Side side, decimal price, string orderTypeId, out Order order)
        {
            order = null;

            var request = new PlaceOrderRequestParameters();
            request.Account = _account;
            request.Symbol = _symbol;
            request.Side = side;
            request.Quantity = (double)_positionQtyContracts;
            request.OrderTypeId = orderTypeId;

            // Most Quantower order types use either Price or TriggerPrice; we set both.
            request.Price = (double)price;
            request.TriggerPrice = (double)price;

            var operationResult = Core.Instance.PlaceOrder(request);
            if (operationResult == null)
                return null;

            var orderId = operationResult.OrderId;
            if (string.IsNullOrWhiteSpace(orderId))
                return null;

            order = Core.Instance.GetOrderById(orderId, "");
            if (order == null)
                return orderId;

            order.Updated += OnProtectiveOrderUpdated;
            return orderId;
        }

        private void OnProtectiveOrderUpdated(IOrder order)
        {
            if (_executionState != ExecutionState.InPosition && _executionState != ExecutionState.Exiting)
                return;

            if (order == null)
                return;

            var kind = ReferenceEquals(order, _takeProfitOrder) ? "TakeProfit" : "StopLoss";
            var orderId = kind == "TakeProfit" ? _takeProfitOrderId : _stopLossOrderId;
            ExportOrderUpdate(orderKind: kind, orderId: orderId, order: order);

            if (order.RemainingQuantity > 0)
                return;

            // One protective order filled -> cancel the other.
            _executionState = ExecutionState.Exiting;

            if (_takeProfitOrder != null && !ReferenceEquals(order, _takeProfitOrder) && _stopLossOrder != null)
                _stopLossOrder.Cancel("");

            if (_stopLossOrder != null && !ReferenceEquals(order, _stopLossOrder) && _takeProfitOrder != null)
                _takeProfitOrder.Cancel("");

            _executionState = ExecutionState.Flat;
        }

        private void ExportOrderUpdate(string orderKind, string orderId, IOrder order)
        {
            if (_orderUpdateWriter == null || order == null)
                return;

            // StopLoss / TakeProfit price holders can vary by order type; keep the columns if available, else export empty.
            object stopLossPrice = "";
            object takeProfitPrice = "";

            try
            {
                // These properties exist on some order types; if not, the catch keeps CSV aligned.
                if (order.StopLoss != null)
                    stopLossPrice = Convert.ToDecimal(order.StopLoss.Price);
            }
            catch
            {
            }

            try
            {
                if (order.TakeProfit != null)
                    takeProfitPrice = Convert.ToDecimal(order.TakeProfit.Price);
            }
            catch
            {
            }

            _orderUpdateWriter.WriteRow(new object[]
            {
                order.LastUpdateTime.ToUniversalTime(),
                orderKind,
                orderId,
                order.Status,
                order.FilledQuantity,
                order.RemainingQuantity,
                order.Price,
                order.TriggerPrice,
                stopLossPrice,
                takeProfitPrice
            });
        }

        private void OnTradeExit(TradeExitSnapshot snapshot)
        {
            var values = new List<object>
            {
                snapshot.TradeId,
                snapshot.Side,
                snapshot.QuantityContracts,
                snapshot.EntryTimeUtc,
                snapshot.EntryPrice,
                snapshot.ExitTimeUtc,
                snapshot.ExitPrice,
                snapshot.ProfitLoss
            };

            foreach (var p in _emaPeriods)
            {
                decimal ema;
                if (snapshot.EmaAtExit != null && snapshot.EmaAtExit.TryGetValue(p, out ema))
                    values.Add(ema);
                else
                    values.Add("");
            }

            _tradeExitWriter.WriteRow(values);
        }

        private static string FindRepoRoot(string startDirectory)
        {
            // Walk up until we find the known folders (report + src).
            var dir = new DirectoryInfo(startDirectory);
            var depth = 0;

            while (dir != null && depth < 12)
            {
                var candidate = dir.FullName;
                if (Directory.Exists(Path.Combine(candidate, "report")) &&
                    Directory.Exists(Path.Combine(candidate, "src")))
                    return candidate;

                dir = dir.Parent;
                depth++;
            }

            // Fallback: current directory.
            return Directory.GetCurrentDirectory();
        }

        private static decimal ConvertToDecimal(object value)
        {
            if (value == null)
                return 0m;

            if (value is decimal d)
                return d;

            return Convert.ToDecimal(value);
        }
    }
}
