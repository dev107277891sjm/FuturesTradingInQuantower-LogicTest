#nullable disable

using EmaStrategy.Core;

namespace EmaStrategy.Simulator;

/// <summary>
/// Simulator options UI. Layout and controls are in <see cref="MainForm.Designer.cs"/> for Visual Studio WinForms designer.
/// </summary>
public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        ApplyStartupDefaults();
    }

    /// <summary>
    /// Sets every control from <see cref="SimulatorOptions"/> defaults so the initial UI matches what a run would use
    /// (same source as <c>new SimulatorOptions()</c> / <see cref="SimulatorOptions.ToConfig"/>).
    /// </summary>
    private void ApplyStartupDefaults()
    {
        ApplySimulatorOptionsToUi(new SimulatorOptions());
    }

    private void ApplySimulatorOptionsToUi(SimulatorOptions o)
    {
        var symIdx = _cmbSymbol.FindStringExact(o.Symbol);
        _cmbSymbol.SelectedIndex = symIdx >= 0 ? symIdx : 0;

        _txtEmaPeriods.Text = o.EmaPeriodsText;

        SetNudInt(_numWarmup, o.WarmupBars);
        SetNudInt(_numMaxOpen, o.MaxOpenContracts);

        if (o.MaxClosedContractsPerSession >= 10_000_000)
            _numMaxClosed.Value = _numMaxClosed.Maximum;
        else
            SetNudInt(_numMaxClosed, o.MaxClosedContractsPerSession);

        SetNudDecimal(_numOrderQty, o.OrderQuantityContracts);

        var priorIdx = (int)o.EntryPriceField;
        if (priorIdx >= 0 && priorIdx < _cmbPriorField.Items.Count)
            _cmbPriorField.SelectedIndex = priorIdx;

        SetNudInt(_numChaseEma, o.ChasingReferenceEmaPeriod);
        SetNudDecimal(_numMinDist, o.MinDistanceFromEmaPoints);

        _cmbTpMode.SelectedIndex = o.TakeProfitMode == TakeProfitMode.Points ? 0 : 1;
        SetNudDecimal(_numTpPts, o.TakeProfitPoints);
        SetNudDecimal(_numTpUsd, o.TakeProfitDollars);

        _cmbSlMode.SelectedIndex = o.StopLossMode switch
        {
            StopLossMode.Points => 0,
            StopLossMode.Dollars => 1,
            _ => 2
        };
        SetNudDecimal(_numSlPts, o.StopLossPoints);
        SetNudDecimal(_numSlUsd, o.StopLossDollars);
        SetNudInt(_numSlAnchorEma, o.StopLossAnchorEmaPeriod);
        SetNudDecimal(_numDollarsPerPoint, o.DollarsPerPointPerContract);

        SetNudInt(_numEvalSec, o.EvaluationIntervalSeconds);
        SetNudInt(_numCdOrder, o.CooldownAfterOrderSeconds);
        SetNudInt(_numCdExit, o.CooldownAfterExitSeconds);

        _cmbFill.SelectedIndex = o.FillMode == EntryFillMode.NextBarOpen ? 0 : 1;

        _chkAvgDown.Checked = o.AllowAverageDown;

        SetNudInt(_numSyntheticCount, o.SyntheticBarCount);
        SetNudDecimal(_numSyntheticPrice, o.SyntheticStartPrice);
    }

    private static void SetNudInt(NumericUpDown n, int value)
    {
        var clamped = Math.Max((int)n.Minimum, Math.Min((int)n.Maximum, value));
        n.Value = clamped;
    }

    private static void SetNudDecimal(NumericUpDown n, decimal value)
    {
        var clamped = Math.Max(n.Minimum, Math.Min(n.Maximum, value));
        n.Value = clamped;
    }

    private async void OnRunClick(object sender, EventArgs e)
    {
        _btnRun.Enabled = false;
        _txtLog.Clear();
        try
        {
            var fill = _cmbFill.SelectedIndex == 0 ? EntryFillMode.NextBarOpen : EntryFillMode.SignalBarClose;
            var prior = (PriorCandlePriceField)_cmbPriorField.SelectedIndex;
            var tpMode = _cmbTpMode.SelectedIndex == 0 ? TakeProfitMode.Points : TakeProfitMode.Dollars;
            var slMode = _cmbSlMode.SelectedIndex switch
            {
                0 => StopLossMode.Points,
                1 => StopLossMode.Dollars,
                _ => StopLossMode.Ema
            };

            var maxClosed = (int)_numMaxClosed.Value;
            if (maxClosed >= 10_000_000)
                maxClosed = int.MaxValue;

            var repoRoot = SimulationService.FindRepoRoot(AppContext.BaseDirectory) ?? Directory.GetCurrentDirectory();
            var options = new SimulatorOptions
            {
                Symbol = _cmbSymbol.SelectedItem?.ToString() ?? "MNQ",
                RepoRoot = repoRoot,
                HistoryCsvPath = "",
                EmaPeriodsText = _txtEmaPeriods.Text.Trim(),
                WarmupBars = (int)_numWarmup.Value,
                MaxOpenContracts = (int)_numMaxOpen.Value,
                MaxClosedContractsPerSession = maxClosed,
                OrderQuantityContracts = _numOrderQty.Value,
                EntryPriceField = prior,
                ChasingReferenceEmaPeriod = (int)_numChaseEma.Value,
                MinDistanceFromEmaPoints = _numMinDist.Value,
                TakeProfitMode = tpMode,
                TakeProfitPoints = _numTpPts.Value,
                TakeProfitDollars = _numTpUsd.Value,
                StopLossMode = slMode,
                StopLossPoints = _numSlPts.Value,
                StopLossDollars = _numSlUsd.Value,
                StopLossAnchorEmaPeriod = (int)_numSlAnchorEma.Value,
                DollarsPerPointPerContract = _numDollarsPerPoint.Value,
                EvaluationIntervalSeconds = (int)_numEvalSec.Value,
                CooldownAfterOrderSeconds = (int)_numCdOrder.Value,
                CooldownAfterExitSeconds = (int)_numCdExit.Value,
                FillMode = fill,
                AllowAverageDown = _chkAvgDown.Checked,
                SyntheticBarCount = (int)_numSyntheticCount.Value,
                SyntheticStartPrice = _numSyntheticPrice.Value
            };

            await Task.Run(() =>
            {
                SimulationService.Run(options, line =>
                {
                    if (IsDisposed)
                        return;
                    BeginInvoke(() => AppendLog(line));
                });
            });
        }
        catch (Exception ex)
        {
            AppendLog("ERROR: " + ex.Message);
        }
        finally
        {
            if (!IsDisposed)
                _btnRun.Enabled = true;
        }
    }

    private void AppendLog(string line)
    {
        if (InvokeRequired)
        {
            BeginInvoke(AppendLog, line);
            return;
        }

        _txtLog.AppendText(line + Environment.NewLine);
    }
}
