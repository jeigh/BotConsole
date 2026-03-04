# Copilot Instructions – BotConsole

## Project Overview

This solution simulates a Zwift cyclist ("bot") by reading real power-meter data over ANT+ and writing adjusted wattage back to Zwift via a virtual ANT+ transmitter. A companion agent collects live Zwift player-state data (draft, gradient, speed) from the Zwift API and updates the bot's target wattage in a shared SQLite database.

---

## Solution Structure

| Project | TFM | Role |
|---|---|---|
| `AntSignalBroadcaster.Console` | .NET Framework 4.8.1 | **Entry point.** Runs `Bot`, which reads wattage from SQLite and transmits it over ANT+ via `AntTransmitHelper`. |
| `AntPowerMeterReceiver.Console` | .NET Framework 4.8.1 | Listens on ANT+ for a real bicycle power meter, scales the wattage, and upserts it into SQLite via `AntReceiveHelper`. |
| `AntHelpers` | .NET Framework 4.8.1 | Shared ANT+ wrappers (`AntTransmitHelper`, `AntReceiveHelper`), `PowerData`, and `Constants` (network key, device type). |
| `DataAccess` | .NET Framework 4.8.1 | `SQLiteGateway` (ADO.NET + `System.Data.SQLite`) and the `Rider` DTO. Database path is hard-coded to `C:\sqlite_databases\zwift_info.sqlite`. |
| `ZwiftDataCollectionAgent.Console` | .NET 8 | Authenticates with the Zwift HTTPS API, polls player state in a loop, calculates additional watts from draft/grade/speed, and writes a `Rider` row to SQLite. |
| `ANT_Managed_Library` | .NET Framework 4.0 | Third-party ANT+ managed wrapper (do **not** edit). |
| `UnitTests` | .NET 8 (MSTest) | Unit tests. Currently contains `ProcessTests` with a `MockConfig` stub. |

---

## Key Domain Concepts

- **`Rider`** (`DataAccess.Rider`) — the central data-transfer object shared across all projects. Fields: `RiderId`, `CurrentWatts`, `CurrentCadence`, `MaxIdealOneMinuteWatts` … `MaxIdealOneHourWatts`, `MaxWattsAboveThreshold`.
- **Power-capping logic** (`Bot.ConditionPowerToPreventCattingUp`) — prevents the bot from exceeding Zwift's 20-minute power cap by tracking a rolling 20-minute joule total in a fixed-size queue and applying graduated backoff when remaining joules run low.
- **Wattage scaling** (`FeatureAbstraction.PersistData`) — raw power from the physical power meter is multiplied by `_targetTwentyMinutePower / _ridersHourPower` before being stored, so the bot always targets a configurable FTP ratio.
- **Draft & gradient adjustment** (`ZwiftDataCollection.Console.Process.CalculateAdditionalWatts`) — additional watts are added to a base wattage based on draft position and road gradient received from the Zwift API.
- **`FixedSizeIntQueue`** — a bounded FIFO queue that retains the last 20 minutes of per-tick watt readings and exposes `HistoryCount` for elapsed-time display.

---

## Configuration

Each console project uses its own `BespokeConfig` class:

- **`AntSignalBroadcaster.Console` / `AntPowerMeterReceiver.Console`** — reads `primaryZwiftId` from `App.config` (`ConfigurationManager.AppSettings`).
- **`ZwiftDataCollectionAgent.Console`** — implements `IBespokeConfig` with `zwiftUsername`, `zwiftPassword`, and `zwiftId`. Config source is set at startup via DI/constructor.

> **Never hard-code credentials.** `BespokeConfig` must always source secrets from configuration, not source code.

The SQLite database path (`C:\sqlite_databases\zwift_info.sqlite`) is currently hard-coded in `SQLiteGateway`. Prefer making it configurable via `BespokeConfig` or an environment variable when changing that class.

---

## Coding Conventions

### Language & TFM constraints
- `AntHelpers`, `AntPowerMeterReceiver.Console`, `AntSignalBroadcaster.Console`, and `DataAccess` target **.NET Framework 4.8.1 / C# 7.3**. Use only C# 7.3-compatible syntax in those projects (no records, no `using` declarations, no switch expressions, no `??=`).
- `ZwiftDataCollectionAgent.Console` and `UnitTests` target **.NET 8**. Modern C# is fine there.
- Do **not** change TFM or `<LangVersion>` unless explicitly asked.

### Style
- Follow the existing style per file — no wholesale reformatting.
- `var` only when the type is obvious from the right-hand side.
- Exception messages use plain English strings (no resource files currently in use).
- No unused parameters or methods.

### Error handling
- Do not swallow exceptions silently. The existing `Bot.Run` pattern (log + `break`) is acceptable for the tight hardware loop; elsewhere let exceptions propagate or log and rethrow.
- Throw `ArgumentException` / `InvalidOperationException` for domain violations; avoid throwing base `Exception`.

### ANT+ hardware layer
- `ANT_Managed_Library` is a pre-built third-party assembly. Do **not** modify it.
- Always call `_antHelper.CloseAndDispose()` / `_device.Dispose()` in a `finally` block — the ANT USB stick requires explicit cleanup.
- `AntTransmitHelper` and `AntReceiveHelper` are stateful; treat them as singletons within a process lifetime.

### Data access
- Use `using` blocks for every `SQLiteConnection`, `SQLiteCommand`, and `SQLiteDataReader`.
- Parameterize all queries (`command.Parameters.AddWithValue`); no string concatenation in SQL.

---

## Testing

- Test framework: **MSTest** (`[TestClass]` / `[TestMethod]`).
- Test project: `UnitTests` (targets .NET 8).
- Mirror the class under test: `Bot` → `BotTests`, `Process` → `ProcessTests`.
- Use `MockConfig : IBespokeConfig` (already defined in `ProcessTests.cs`) as the pattern for faking config dependencies.
- Do **not** test ANT+ hardware helpers with unit tests — they require physical USB hardware.
- `Bot.ConditionPowerToPreventCattingUp` and `Process.CalculateAdditionalWatts` are the primary pure-logic targets for unit tests.
- Avoid disk I/O in tests; if an in-memory SQLite DB is needed use `:memory:` as the connection string.