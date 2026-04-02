# Quantower EMA Strategy Test System (C#)

This repository contains a small, testable C# implementation of an EMA-based strategy core plus a simulator harness that exports:
- evaluation logs (bias/cloud color, reasons)
- trade entries
- trade exits (TP/SL-based)
- the bar stream used for the run

The intent is to support the Upwork/Quantower request: **stability + transparency + CSV logging** before integrating with Quantower Algo APIs.

## Quick start

### 1) Run unit tests (from repository root)
```powershell
dotnet test QuantowerEmaStrategyTestSystem.sln -c Release
```

### 2) Run the simulator (exports CSV)

**Windows UI (default)** — configure symbol label, EMA periods, max contracts, TP/SL, cooldowns, fill mode, and more:

```powershell
dotnet run --project src/EmaStrategy.Simulator/EmaStrategy.Simulator.csproj -c Release
```

**Headless console** (useful for scripts/CI; uses `SimulatorOptions` defaults with a small explicit subset in code—see `src/EmaStrategy.Simulator/Program.cs`):

```powershell
dotnet run --project src/EmaStrategy.Simulator/EmaStrategy.Simulator.csproj -c Release -- console
```

Run these commands from the **repository root** (where `QuantowerEmaStrategyTestSystem.sln` lives).

CSV outputs are written under:

`report/exports/simulator/<symbol>/live/`

Typical files: `bar_stream.csv`, `evaluations.csv`, `trade_entries.csv`, `trade_exits.csv`.

The **symbol** field is a label used for the output folder (for example `MNQ`); it does not connect to a live broker. Bar data is loaded from the default repo file `.temp/_srv/MNQ_50k_Bar_History.csv` when present (columns `CloseTime`, `Open`, `High`, `Low`, `Close`). If that file is missing or yields no rows, the simulator generates synthetic bars (counts and start price from **Synthetic** fields in the UI, or from `SimulatorOptions` when using console mode).

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
- **Signals**: BUY/SELL/NO TRADE from stacked EMA ordering; **cloud color** in logs uses the fast vs slow EMA pair (see `EmaCloudLogic`)
- **Logging**: evaluations + trade entry/exit exported to CSV and printed to console
- **Stability**: warmup/history gating in the strategy engine

## Configuration

- **Simulator UI**: run the simulator project without arguments. On startup, controls are filled from `new SimulatorOptions()` so defaults match `SimulatorOptions` / `EmaStrategyConfig`. Logic lives in `SimulatorOptions`, `SimulationService`, and `MainForm`.
- **Console mode**: when you pass `console`, `Program` invokes a private `RunConsole` method in `src/EmaStrategy.Simulator/Program.cs` that builds a `SimulatorOptions` instance (explicit fields plus class defaults for the rest).

The simulator does **not** load a `.env` file today; `.env` is listed in `.gitignore` for possible future use. `.temp/` and `report/exports/` are ignored by git (see `.gitignore`).

