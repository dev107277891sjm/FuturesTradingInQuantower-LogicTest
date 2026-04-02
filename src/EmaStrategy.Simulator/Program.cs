using EmaStrategy.Core;

namespace EmaStrategy.Simulator;

public static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Length > 0 && args[0].Equals("console", StringComparison.OrdinalIgnoreCase))
            return RunConsole();

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
        return 0;
    }

    /// <summary>
    /// Headless run (same defaults as the UI form) for CI/scripts.
    /// Usage: dotnet run --project ... -- console
    /// </summary>
    private static int RunConsole()
    {
        var repoRoot = SimulationService.FindRepoRoot(AppContext.BaseDirectory) ?? Directory.GetCurrentDirectory();

        var options = new SimulatorOptions
        {
            Symbol = "MNQ",
            HistoryCsvPath = "",
            RepoRoot = repoRoot,
            EmaPeriodsText = "20,50,100",
            WarmupBars = 120,
            MaxOpenContracts = 1,
            MaxClosedContractsPerSession = int.MaxValue,
            OrderQuantityContracts = 1m,
            TakeProfitPoints = 10m,
            StopLossPoints = 10m,
            MinDistanceFromEmaPoints = 0m,
            EvaluationIntervalSeconds = 30,
            CooldownAfterOrderSeconds = 30,
            CooldownAfterExitSeconds = 30,
            FillMode = EntryFillMode.NextBarOpen,
            SyntheticBarCount = 600,
            SyntheticStartPrice = 24000m
        };

        SimulationService.Run(options, Console.WriteLine);
        return 0;
    }
}
