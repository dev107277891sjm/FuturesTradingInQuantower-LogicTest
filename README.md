# Quantower EMA Strategy Test System (C#)

This repository contains a small, testable C# implementation of an EMA-based strategy core plus a simulator harness that exports:
- evaluation logs (bias/cloud color, reasons)
- trade entries
- trade exits (TP/SL-based)
- the bar stream used for the run

The intent is to support the Upwork/Quantower request: **stability + transparency + CSV logging** before integrating with Quantower Algo APIs.

## Quick start

### 1) Run unit tests
```powershell
dotnet test QuantowerEmaStrategyTestSystem.sln -c Release
```

### 2) Run the simulator (exports CSV)

**Windows UI (default)** — configure symbol label, EMA periods, max contracts, TP/SL, cooldowns, fill mode, and more:

```powershell
dotnet run --project src/EmaStrategy.Simulator/EmaStrategy.Simulator.csproj -c Release
```

**Headless console** (same defaults as before; useful for scripts/CI):

```powershell
dotnet run --project src/EmaStrategy.Simulator/EmaStrategy.Simulator.csproj -c Release -- console
```

CSV outputs are written under:

`report/exports/simulator/<symbol>/live/*`

The **symbol** field is a label used for the output folder (for example `MNQ`); it does not connect to a live broker. Bar data is loaded from the default repo file `.temp/_srv/MNQ_50k_Bar_History.csv` when present (columns `CloseTime,Open,High,Low,Close`). If that file is missing or yields no rows, the simulator generates synthetic bars using the synthetic settings in the UI.

## Quantower Real-time Adapter

This project also includes a Quantower Algo adapter that drives `EmaStrategy.Core` using real-time market data and exports CSV logs from inside Quantower.

### Build
```powershell
dotnet build QuantowerEmaStrategyTestSystem.sln -c Release
```

The adapter assembly is produced under:
- `src/QuantowerEmaStrategyAdapter/bin/Release/net9.0/QuantowerEmaStrategyAdapter.dll`

### Output files
When the strategy runs inside Quantower, CSVs are written to:
- `report/exports/quantower_realtime/<run_timestamp>/`

Generated files:
- `bar_stream.csv`
- `evaluations.csv`
- `trade_entries.csv`
- `trade_exits.csv`
- `order_updates.csv`

### How it feeds the EMA engine
- Historical warmup is seeded from `Symbol.GetHistory(Period.MIN1, ...)`.
- Realtime updates come from `Symbol.NewLast`.
- Realtime data is aggregated into tick-bars of `25,000` trade ticks per bar and then fed to `EmaStrategy.Core` (`StrategyEngine.ProcessBar`).

## How it maps to the job requirements

- **EMA calculation**: configurable EMA periods
- **Signals**: BUY/SELL/NO TRADE based on EMA ordering (EMA cloud)
- **Logging**: evaluations + trade entry/exit exported to CSV and printed to console
- **Stability**: warmup/history gating in the strategy engine

## Configuration

- **Simulator UI**: run the simulator project without arguments; all parameters are edited on the form. Logic lives in `SimulatorOptions`, `SimulationService`, and `MainForm`.
- **Console mode**: defaults are in `Program.RunConsole()` (used when you pass `console` as shown above).

If you want to override behavior locally via environment variables, create a `.env` file at the repo root. Note: `.env` is not yet wired into the simulator UI; it is reserved for future use.

`.temp/` is treated as input/artifact storage and is ignored by git via `.gitignore`.

