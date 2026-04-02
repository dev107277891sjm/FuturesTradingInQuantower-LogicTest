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
```powershell
dotnet run --project src/EmaStrategy.Simulator/EmaStrategy.Simulator.csproj -c Release
```

CSV outputs are written to:
`report/exports/historical/*` and `report/exports/live/*`

## How it maps to the job requirements

- **EMA calculation**: configurable EMA periods
- **Signals**: BUY/SELL/NO TRADE based on EMA ordering (EMA cloud)
- **Logging**: evaluations + trade entry/exit exported to CSV and printed to console
- **Stability**: warmup/history gating in the strategy engine

## Configuration

Default simulator parameters are defined in `src/EmaStrategy.Simulator/Program.cs`.

If you want to override behavior locally via environment variables, create a `.env` file at the repo root. Note: the simulator currently uses hardcoded defaults; `.env` is provided for future parameterization.

`.temp/` is treated as input/artifact storage and is ignored by git via `.gitignore`.

