using System.Globalization;
using EmaStrategy.Core;

namespace EmaStrategy.Simulator;

/// <summary>
/// Runs the EMA strategy simulator passes and writes CSV exports.
/// </summary>
public static class SimulationService
{
    public static string? FindRepoRoot(string startDir)
    {
        var dir = new DirectoryInfo(startDir);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, ".temp");
            if (Directory.Exists(candidate))
                return dir.FullName;

            dir = dir.Parent;
        }
        return null;
    }

    public static string SanitizeFolderName(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return "default";
        var invalid = Path.GetInvalidFileNameChars();
        var s = string.Join("_", symbol.Trim().Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrEmpty(s) ? "default" : s;
    }

    /// <summary>
    /// Resolve history CSV path: if options.HistoryCsvPath is set, use it; else default under repo .temp/_srv/MNQ_50k_Bar_History.csv
    /// </summary>
    public static string ResolveHistoryCsvPath(SimulatorOptions options, string repoRoot)
    {
        if (!string.IsNullOrWhiteSpace(options.HistoryCsvPath))
        {
            var p = options.HistoryCsvPath.Trim();
            if (Path.IsPathRooted(p))
                return p;
            return Path.GetFullPath(Path.Combine(repoRoot, p));
        }

        return Path.Combine(repoRoot, ".temp", "_srv", "MNQ_50k_Bar_History.csv");
    }

    public static void Run(SimulatorOptions options, Action<string> log)
    {
        var repoRoot = options.RepoRoot ?? FindRepoRoot(AppContext.BaseDirectory) ?? Directory.GetCurrentDirectory();
        var historyCsv = ResolveHistoryCsvPath(options, repoRoot);

        var bars = LoadBarsFromHistoryCsv(historyCsv);
        if (bars.Count == 0)
        {
            log("No bar history rows found; generating synthetic bars.");
            bars = GenerateSyntheticBars(
                options.SyntheticBarCount,
                options.SyntheticStartPrice,
                DateTime.UtcNow.AddMinutes(-options.SyntheticBarCount));
        }
        else
        {
            log($"Loaded {bars.Count} bars from file.");
        }

        var outputBase = Path.Combine(repoRoot, "report", "exports", "simulator", SanitizeFolderName(options.Symbol));
        Directory.CreateDirectory(outputBase);

        var config = options.ToConfig();

        var liveDir = Path.Combine(outputBase, "live");
        RunPass("LIVE", bars, config, liveDir, exportBars: true, log);

        log($"Exports written to: {liveDir}");
    }

    private static void RunPass(
        string passName,
        IReadOnlyList<Bar> bars,
        EmaStrategyConfig config,
        string outputDir,
        bool exportBars,
        Action<string> log)
    {
        Directory.CreateDirectory(outputDir);

        if (exportBars)
            ExportBars(bars, Path.Combine(outputDir, "bar_stream.csv"));

        var periods = config.EmaPeriods.Distinct().OrderBy(p => p).ToArray();
        log($"[{passName}] EMA periods: {string.Join(", ", periods)} (count={periods.Length})");

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
        evalHeaders.AddRange(periods.Select(p => $"EMA_{p}"));
        var expectedEvalCols = evalHeaders.Count;

        using var evalWriter = new CsvTableWriter(
            Path.Combine(outputDir, "evaluations.csv"),
            evalHeaders);

        using var entryWriter = new CsvTableWriter(
            Path.Combine(outputDir, "trade_entries.csv"),
            new[]
            {
                "TradeId",
                "Side",
                "QuantityContracts",
                "EntryTimeUtc",
                "EntryPrice"
            }.Concat(periods.Select(p => $"EMA_{p}_AtEntry")).ToArray());

        using var exitWriter = new CsvTableWriter(
            Path.Combine(outputDir, "trade_exits.csv"),
            new[]
            {
                "TradeId",
                "Side",
                "QuantityContracts",
                "EntryTimeUtc",
                "EntryPrice",
                "ExitTimeUtc",
                "ExitPrice",
                "ProfitLoss"
            }.Concat(periods.Select(p => $"EMA_{p}_AtExit")).ToArray());

        var engine = new StrategyEngine(config);

        engine.Evaluation += snapshot =>
        {
            var row = new List<object?>
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
            foreach (var p in periods)
                row.Add(snapshot.EmaValues.TryGetValue(p, out var v) ? v : 0m);
            if (row.Count != expectedEvalCols)
                log($"[{passName}] WARNING: evaluations row count mismatch. row={row.Count}, expected={expectedEvalCols}");
            evalWriter.WriteRow(row);

            if (snapshot.Bias != Bias.None)
            {
                log($"[{passName}] {snapshot.TimeUtc:HH:mm:ss} idx={snapshot.BarIndex} price={snapshot.Price} bias={snapshot.Bias} cloud={snapshot.CloudColor} reason={snapshot.Reason}");
            }
        };

        engine.TradeEntry += entry =>
        {
            var row = new List<object?>
            {
                entry.TradeId,
                entry.Side,
                entry.QuantityContracts,
                entry.EntryTimeUtc,
                entry.EntryPrice
            };
            foreach (var p in periods)
                row.Add(entry.EmaAtEntry.TryGetValue(p, out var v) ? v : 0m);
            entryWriter.WriteRow(row);
            log($"[{passName}] TRADE ENTRY id={entry.TradeId} side={entry.Side} qty={entry.QuantityContracts} entry={entry.EntryPrice}");
        };

        engine.TradeExit += exit =>
        {
            var row = new List<object?>
            {
                exit.TradeId,
                exit.Side,
                exit.QuantityContracts,
                exit.EntryTimeUtc,
                exit.EntryPrice,
                exit.ExitTimeUtc,
                exit.ExitPrice,
                exit.ProfitLoss
            };
            foreach (var p in periods)
                row.Add(exit.EmaAtExit.TryGetValue(p, out var v) ? v : 0m);
            exitWriter.WriteRow(row);
            log($"[{passName}] TRADE EXIT id={exit.TradeId} side={exit.Side} exit={exit.ExitPrice} pnl={exit.ProfitLoss}");
        };

        foreach (var bar in bars)
            engine.ProcessBar(bar);
    }

    private static void ExportBars(IReadOnlyList<Bar> bars, string outputPath)
    {
        using var writer = new StreamWriter(outputPath, append: false);
        writer.WriteLine("Index,TimeUtc,Open,High,Low,Close");

        foreach (var b in bars)
        {
            writer.WriteLine(string.Join(",",
                b.Index,
                b.TimeUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                b.Open.ToString(CultureInfo.InvariantCulture),
                b.High.ToString(CultureInfo.InvariantCulture),
                b.Low.ToString(CultureInfo.InvariantCulture),
                b.Close.ToString(CultureInfo.InvariantCulture)));
        }
    }

    private static List<Bar> LoadBarsFromHistoryCsv(string path)
    {
        try
        {
            if (!File.Exists(path))
                return new List<Bar>();

            var lines = File.ReadAllLines(path);
            if (lines.Length < 2)
                return new List<Bar>();

            var header = lines[0].Split(',').Select(h => h.Trim()).ToArray();
            var col = header
                .Select((name, idx) => (name, idx))
                .ToDictionary(x => x.name, x => x.idx, StringComparer.OrdinalIgnoreCase);

            if (!col.TryGetValue("CloseTime", out var closeTimeIdx) ||
                !col.TryGetValue("Open", out var openIdx) ||
                !col.TryGetValue("High", out var highIdx) ||
                !col.TryGetValue("Low", out var lowIdx) ||
                !col.TryGetValue("Close", out var closeIdx))
            {
                return new List<Bar>();
            }

            var bars = new List<Bar>();
            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(',');
                if (parts.Length <= Math.Max(Math.Max(closeTimeIdx, openIdx), Math.Max(Math.Max(highIdx, lowIdx), closeIdx)))
                    continue;

                if (!DateTime.TryParse(parts[closeTimeIdx], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var timeUtc))
                    continue;

                if (!decimal.TryParse(parts[openIdx], NumberStyles.Any, CultureInfo.InvariantCulture, out var open))
                    continue;
                if (!decimal.TryParse(parts[highIdx], NumberStyles.Any, CultureInfo.InvariantCulture, out var high))
                    continue;
                if (!decimal.TryParse(parts[lowIdx], NumberStyles.Any, CultureInfo.InvariantCulture, out var low))
                    continue;
                if (!decimal.TryParse(parts[closeIdx], NumberStyles.Any, CultureInfo.InvariantCulture, out var close))
                    continue;

                bars.Add(new Bar(bars.Count, timeUtc, open, high, low, close));
            }

            return bars;
        }
        catch
        {
            return new List<Bar>();
        }
    }

    private static List<Bar> GenerateSyntheticBars(int count, decimal startPrice, DateTime startTimeUtc)
    {
        var bars = new List<Bar>(count);
        var rand = new Random(12345);

        decimal price = startPrice;
        var dt = startTimeUtc;

        for (var i = 0; i < count; i++)
        {
            var delta = (decimal)(rand.NextDouble() - 0.5) * 20m;
            var open = price;
            var close = price + delta;
            var high = Math.Max(open, close) + (decimal)rand.NextDouble() * 5m;
            var low = Math.Min(open, close) - (decimal)rand.NextDouble() * 5m;

            bars.Add(new Bar(i, dt, open, high, low, close));

            price = close;
            dt = dt.AddMinutes(1);
        }

        return bars;
    }
}
