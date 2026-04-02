# Job Analysis Report: Quantower EMA Strategy Test System

## 1. Source Inputs
This report is based on:
- Upwork job summary (requesting a C# Quantower futures trading program)
- Upwork chat history (detailed requirements, edge-cases, and deliverables)
- Sample CSV artifacts in `.temp/_srv` (used to infer the desired logging fields)
- Your test-project request (`.temp/test project request.txt`) describing the minimal deliverable: stable EMA-based strategy with logging and CSV export

## 2. Project Objective (What the buyer wants)
The buyer wants a **stable, transparent, EMA-driven trading strategy** implemented in C#, suitable for **Quantower Algo** (extension-style strategy built in Visual Studio).

The core goals emphasized in the conversation:
1. **Fix data flow and runtime stability issues** (especially history/tick-bar readiness).
2. **Generate reliable signals** (BUY/SELL/NO TRADE).
3. **Log everything** (evaluations, decisions, trade attempts/results) to both:
   - console output (for immediate visibility)
   - CSV files (for post-run analysis)
4. **Export historical and live data to CSV** for verification/debugging.

## 3. Key Functional Requirements (from chat)
### 3.1 EMA calculation and bias/cloud logic
The strategy must calculate EMAs with configurable periods (examples given):
- 15/35/50/100 (in your test request example)
- 20/50/100 (in the chat for trade criteria)

The “bias” logic is explicitly described as ordering-based:
- If **EMA(20) > EMA(50) > EMA(100)** => focus on **BUY**
- If **EMA(100) > EMA(50) > EMA(20)** => focus on **SELL**
- Otherwise => **NO TRADE**

Also described: an EMA “cloud” color concept (green/red) reflecting EMA ordering.

### 3.2 Entry/exit criteria
The chat specifies (configurable):
- Entry price choice: prior candle **open** or **close** (and potentially mid/high/low options).
- Take profit: configurable points/$.
- Stop loss: configurable points/$ or linked to EMA/points.
- Optional “distance filter” to avoid chasing:
  - If price is within some points from one of the EMAs, then wait.
- Trade timeframe choice:
  - Either trade off higher timeframe candles (e.g., 5-minute)
  - Or trade off tick bars (e.g., 25,000 ticks tick bars)
- Configurability expectations:
  - Ability to enter/update EMA periods without rewriting code (examples: 20/50/100, or 15/35/50/100)
  - Ability to configure the tick-bar size / tick-aggregation
  - Entry-price selection should be controllable (the chat mentions a dropdown and choosing prior candle OHLC point)

### 3.3 Execution safeguards / risk controls
The buyer wants safeguards like:
- Not placing multiple orders back-to-back
- Not placing buy and short orders at the same time
- Not shorting while already long (and vice-versa)
- Limiting the number of contracts (max contracts both open and closed)
- No trading in specific hours / holidays (mentioned as future requirement)

## 4. Data Flow + Stability Requirements (why their current attempts failed)
From the chat:
- “Programs I tried to run are not generating trade signals or logging trades…”
- “Even something as simple as exporting the most recent 100 tick bar candles is not working.”
- Explicit requirement: **wait for history to load before doing any evaluations or placing any trades.**

Stability measures implied/required:
- Warmup/history readiness gate: do not evaluate until EMA inputs are valid.
- Controlled evaluation timing:
  - After an evaluation, wait 5 seconds / 30 seconds / 10 minutes (configurable).
  - Debounce-like behavior and cooldown minutes left.
- Avoid repeated evaluations spamming order placement logic (debounce/cooldown and “pending order” handling).

## 5. Logging and CSV Export Requirements
The buyer specifically asked for CSV logging of:
1. **Actual trades**:
   - entry and exit data
   - criteria values used
   - reason/metadata
2. **Trade evaluations**:
   - whether an order is filled or not
   - why a trade was not executed
3. Exports for both:
   - historical data
   - live data

### 5.1 Inferred CSV field expectations (from `.temp/_srv` headers)
Sample headers (observed):
- `MNQ_Evaluations_Log.csv` includes fields like:
  - `DatePST, TimePST, EvaluationCount, Price, EMA7, DeviationPct, Bias, Reason`
- `MNQ_Main_Strategy_Logs.csv` includes fields like:
  - `DateTime, EMA12, EMA30, EMA50, CloudColor, Price, NetQty, FilledContracts, PendingContracts, ... Decision, Reason, OrderPlaced`
- `MNQ_Position_Evaluations.csv` includes position/evaluation fields like:
  - `DateOpen, TimeOpenPST, Symbol, PriceOpen, EMA7, EMA12, ... ActionClose, MinutesInTrade`
- `MNQ_TradeLog.csv` includes trade snapshots with EMA values near decision/execution.

The project should therefore ensure that exported logs include:
- timestamps
- evaluation count / cooldown state
- price + EMA values at evaluation
- bias / cloud color
- decision + reason
- trade entry/exit details and criteria snapshots

## 6. Implementation Approach (what was built as the test project)
Because the workspace initially contained no C# solution, the test project is built from scratch to satisfy your “stability + transparency” deliverable:
- `EmaStrategy.Core`
  - EMA calculator for multiple periods
  - EMA ordering => bias/cloud color logic
  - Strategy engine with:
    - warmup gate (history readiness)
    - evaluation interval / debounce
    - pending order logic (fill next bar open or signal bar close)
    - TP/SL exits
    - entry/exit events
- `EmaStrategy.Simulator`
  - Reads `.temp/_srv/MNQ_50k_Bar_History.csv` if rows exist
  - If history contains only a header, generates deterministic synthetic OHLC bars
  - Runs two passes:
    - `historical` pass
    - `live` pass
  - Writes CSV outputs into:
    - `report/exports/historical/*`
    - `report/exports/live/*`
  - Writes console logs for signal-bearing evaluations and trade entry/exit events

## 7. Acceptance Criteria (recommended)
1. Strategy logic runs without exceptions.
2. Warmup/history gating prevents evaluation until EMA inputs are valid.
3. The engine produces deterministic BUY/SELL/NO TRADE signals using EMA ordering.
4. Console logging and CSV logging work for:
   - evaluations (including reasons)
   - trade entries
   - trade exits
5. Historical and live passes both export CSV files.

## 8. Open Questions / Gaps vs Quantower production
Some requirements from the chat were not fully implemented in the simplified test engine:
1. Tick-bar creation / Quantower-specific candle/tick APIs
2. Trading-hours + holiday calendar checks
3. “Average down” / pyramiding logic
4. Order rejection handling (the simulator fills immediately based on OHLC assumptions)
5. Quantower Algo API integration details (this test project focuses on the algorithm core + simulator)
6. Symbol selection and multi-symbol parameter sets (MNQ/NQ/ES/MES)
7. Configurable “entry price source” modes and TP/SL variants beyond fixed points

These should be addressed when wrapping the engine in an actual Quantower strategy adapter.

