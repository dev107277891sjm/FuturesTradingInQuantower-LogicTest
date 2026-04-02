using System.Globalization;
using EmaStrategy.Core;

namespace EmaStrategy.Simulator;

public static class Program
{
    public static int Main(string[] args)
    {
        var startDir = AppContext.BaseDirectory;
        var repoRoot = FindRepoRoot(startDir) ?? Directory.GetCurrentDirectory();

        // Inputs are the sample CSVs inside the workspace .temp folder.
        var historyCsv = Path.Combine(repoRoot, ".temp", "_srv", "MNQ_50k_Bar_History.csv");

        var bars = LoadBarsFromHistoryCsv(historyCsv);
        if (bars.Count == 0)
        {
            Console.WriteLine("No bar history rows found in sample CSV; generating synthetic bars for the test run.");
            bars = GenerateSyntheticBars(count: 600, startPrice: 24000m, startTimeUtc: DateTime.UtcNow.AddMinutes(-600));
        }

        var outputBase = Path.Combine(repoRoot, "report", "exports");
        Directory.CreateDirectory(outputBase);

        var config = new EmaStrategyConfig
        {
            EmaPeriods = new[] { 20, 50, 100 },
            WarmupBars = 120,
            EvaluationInterval = TimeSpan.FromSeconds(30),
            MaxContracts = 1,
            OrderQuantityContracts = 1,
            MinDistanceFromFastEmaPoints = 0m,
            TakeProfitPoints = 10m,
            StopLossPoints = 10m,
            FillMode = EntryFillMode.NextBarOpen,
            CooldownAfterOrderPlaced = TimeSpan.FromSeconds(30),
            CooldownAfterExit = TimeSpan.FromSeconds(30)
        };

        // Run historical pass.
        var historicalDir = Path.Combine(outputBase, "historical");
        RunPass(passName: "HISTORICAL", bars, config, historicalDir, exportBars: true);

        // Run live pass.
        var liveDir = Path.Combine(outputBase, "live");
        RunPass(passName: "LIVE", bars, config, liveDir, exportBars: true);

        Console.WriteLine($"Exports written to: {outputBase}");
        return 0;
    }

    private static string? FindRepoRoot(string startDir)
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

    private static void RunPass(
        string passName,
        IReadOnlyList<Bar> bars,
        EmaStrategyConfig config,
        string outputDir,
        bool exportBars)
    {
        Directory.CreateDirectory(outputDir);

        if (exportBars)
            ExportBars(bars, Path.Combine(outputDir, "bar_stream.csv"));

        var periods = config.EmaPeriods.Distinct().OrderBy(p => p).ToArray();
        Console.WriteLine($"[{passName}] EMA periods: {string.Join(", ", periods)} (count={periods.Length})");

        // Evaluation log (per evaluation attempt).
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
            {
                Console.WriteLine($"[{passName}] WARNING: evaluations row count mismatch. row={row.Count}, expected={expectedEvalCols}, periodsCount={periods.Length}");
            }
            evalWriter.WriteRow(row);

            // Keep console output readable: print only signal-worthy evaluations.
            if (snapshot.Bias != Bias.None)
            {
                Console.WriteLine($"[{passName}] {snapshot.TimeUtc:HH:mm:ss} idx={snapshot.BarIndex} price={snapshot.Price} bias={snapshot.Bias} cloud={snapshot.CloudColor} reason={snapshot.Reason}");
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

            Console.WriteLine($"[{passName}] TRADE ENTRY id={entry.TradeId} side={entry.Side} qty={entry.QuantityContracts} entry={entry.EntryPrice}");
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

            Console.WriteLine($"[{passName}] TRADE EXIT id={exit.TradeId} side={exit.Side} exit={exit.ExitPrice} pnl={exit.ProfitLoss}");
        };

        foreach (var bar in bars)
        {
            engine.ProcessBar(bar);
        }
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
            // Create a small random-walk OHLC bar.
            var delta = (decimal)(rand.NextDouble() - 0.5) * 20m; // +/-10 points
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

