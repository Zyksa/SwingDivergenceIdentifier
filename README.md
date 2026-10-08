# Swing & Divergence Identifier

Market structure, CVD divergences and execution bubbles for Deepcharts desktop.

by Zyksa | Beta v0.6

![MIT License](https://img.shields.io/badge/license-MIT-26a69a?style=flat-square)
![.NET 10](https://img.shields.io/badge/.NET-10-8b5cf6?style=flat-square)
![Version 1.0.25](https://img.shields.io/badge/version-1.0.25-334155?style=flat-square)

[Features](#features) | [Screenshots](#screenshots) | [Installation](#installation) | [Configuration](#configuration) | [Development](#development)

## Features

Swing & Divergence Identifier combines three tools in one chart overlay:

- **Market structure:** alternating swing highs and lows, classified as HH, HL,
  LH and LL, with optional previews of points still forming.
- **CVD divergences:** regular and hidden divergences between price and CVD,
  with lines on price and the CVD panel. RSI and MACD are optional alternatives.
- **Execution bubbles:** translucent buy and sell bubbles filtered by minimum
  volume, with volume labels that become visible as you zoom in.

Visual notifications and configurable sounds signal structural CVD setups as
soon as their conditions hold on a live tick. Early lines are dotted and marked
with `?` until confirmation. One movement triggers at most one alert.

CVD divergences with no confirmed structure anchor use pink lines. Divergences
with at least one confirmed anchor retain EXH/ABS labels and directional colors.
Each group has independent display, opacity, sound and popup settings. Pink
signals are silent by default.

## Screenshots

### Structure Points

Follow alternating highs and lows with HH, HL, LH and LL labels. Confirmed turns
use closed candles. Provisional points are marked with `?`.

![Market structure points](https://i.imgur.com/Dlrk2p6.png)

### Deep Trades

Identify aggressive buy and sell executions with green and purple bubbles.
Filter by minimum volume and reveal volume labels progressively as you zoom in.

![Execution bubbles](https://i.imgur.com/TTAKwU0.png)

### CVD Divergences and Notifications

Compare price extremes with CVD and receive visual or audio notifications as soon
as structural setup conditions are detected, independently of the displayed major structure.

![CVD divergences and notifications](https://i.imgur.com/cwDx3fY.png)

## Installation

### Requirements

- Windows with Deepcharts and its indicator SDK installed.
- .NET SDK 10 and PowerShell 7.
- Your own Deepcharts developer ID.

The SDK files `VolSysAPI.dll` and `VolumetricaCore.dll` come from your Deepcharts
installation and are not included in this repository.

### Install with a double-click

1. Download this repository using **Code > Download ZIP**, then extract the whole archive.
2. Double-click **`Installer.bat`** in the extracted folder.
3. Enter your own Deepcharts developer ID when prompted. Existing local settings are reused.
4. Restart Deepcharts and add **Personal > Swing & Divergence Identifier**.

The installer locates Deepcharts, checks its SDK libraries, builds your personal
DLL and copies it into the Windows Documents indicator folder. A previous DLL is
backed up before replacement. The ID stays in the Git-ignored
`Deepchart.local.props` file.

Missing PowerShell 7 or .NET SDK 10 can be installed through WinGet after an
explicit prompt. Windows may request administrator approval for these Microsoft
packages. If WinGet is unavailable, install the prerequisites manually:
[PowerShell](https://learn.microsoft.com/en-us/powershell/scripting/install/install-powershell-on-windows)
and [.NET SDK](https://learn.microsoft.com/en-us/dotnet/core/install/windows).

To check your machine without installing or changing files:

```powershell
.\Installer.bat -CheckOnly -NonInteractive
```

### Build locally

From the repository root:

```powershell
Copy-Item Deepchart.local.props.example Deepchart.local.props
```

Edit `Deepchart.local.props` with your SDK installation directory and developer
ID. This file is ignored by Git.

```powershell
pwsh -File scripts/Build-Indicator.ps1 -Install
```

The installer copies `SwingDivergenceIdentifier.dll` into
`Documents\Deepchart\Indicators`. Look for **Swing & Divergence Identifier**
in the **Personal** category of the indicator list.

## Configuration

| Setting | Default | Purpose |
| --- | --- | --- |
| Divergence source | CVD | Select CVD or optional RSI/MACD combinations. |
| Reversal confirmation | 2 closed bars | Validate a retreat held across closes; 3 is available. |
| Forming structure points | On | Preview the current provisional swing. |
| CVD-panel lines | On | Draw divergence lines on the selected CVD panel. |
| CVD panel | 2 | Target the first panel below price. |
| CVD indicator ID | 3 | Resolve the area of the selected native CVD; use the panel as fallback. |
| Enable execution bubbles | On | Turn bubbles on or off in the Bubbles settings. |
| Minimum bubble volume | 50 | Filter executions by size. |
| Early CVD detection | On | Draw and notify on eligible live ticks before the candle closes. |
| CVD setup confirmation | Structural divergence | Require the setup filters; optional BOS/sweep adds a condition. |
| Setup structure | Same as chart points | Pair the confirmed and forming points of the selected structure method. |
| Last CVD check | Read-only | Explain detection or rejection in the CVD setup settings. |
| Visual and audio notifications | On | Enable the two channels independently. |
| Pink CVD divergences | On, 50% opacity | Show divergences with no confirmed structure anchor. |
| Pink divergence alerts | Off | Enable their sound and popup separately if needed. |

Divergence pairs are limited to **12 bars**. Advanced settings cover secondary
structure, filtering and notification behavior.

[Complete user guide](docs/USER_GUIDE.fr.md)

## Signal Behavior

Confirmed structure uses closed candles. Forming structure points can move or
disappear. Setup detection uses the same alternating swings as the chart points,
samples CVD on the exact price-pivot candles and requires both price
and CVD lines to remain clear of intervening candle ranges. Weak signals and
missing CVD data are rejected. Divergence display settings do not relax these
alert requirements. A tick snapshot evaluates the active swing of the selected
structure engine without committing provisional points to closed history.
Hiding the structure labels does not stop the setup detector.

Early detection is enabled by default. Eligible setups are drawn on price and
CVD and notified during the open candle. Early lines can move or disappear if
their conditions fail before confirmation. A moving extreme, reappearance or
final confirmation does not repeat the alert for the same leg. Disable early
detection to require closed swings. History loading, recalculation and replay
are silent by default.
The indicator does not submit orders or draw projected entry, stop or target zones.

CVD requires Ask/Bid delta data. The CVD panel must use the same session reset
and unfiltered delta as the indicator. Execution bubbles require actual
transaction data; candle totals cannot reconstruct individual executions.

The execution cache retains up to 20,000 groups through parameter changes.
It is cleared when the indicator instance is removed or Deepcharts restarts.

The trade plot uses the supplied reference settings: size deviation 2.5, minimum
size 0.20, maximum size 25, fill opacity 20-30%, and centered white text at size 12.
Growing aggregates update a single bubble as soon as they pass the volume filter.
SDK aggregates can be selected when the feed provides them; reconstructed sweeps
remain available. Statistical size calibration is an independent implementation,
so pixel-for-pixel equivalence to the proprietary renderer is not established.

## Project Structure

| Location | Purpose |
| --- | --- |
| `src/Analysis/` | Structure, divergence and execution engines. |
| `src/Structure/` | Window-based pivot detection. |
| `src/Configuration/` | Settings and default values. |
| `src/Rendering/` | Chart annotations. |
| `src/Runtime/` | SDK callbacks, live previews, notifications and cache. |
| `scripts/` | Build and source checks. |
| `tests/` | Synthetic regression and SDK adapter checks. |
| `docs/` | Detailed guides and compatibility notes. |

## Development

Run the engine checks without Deepcharts or its SDK:

```powershell
dotnet run --project tests/StructureChecks.csproj -c Release
```

With the SDK installed, run the adapter checks:

```powershell
dotnet run --project tests/AdapterChecks/AdapterChecks.csproj -c Release
```

GitHub CI runs the engine suite and source checks. Adapter tests simulate SDK
calls; native rendering and physical audio require manual validation.

[Contributing](CONTRIBUTING.md) | [Migration guide](docs/MIGRATION.md) | [Data-model compatibility](docs/DATA_MODEL_COMPATIBILITY.md)

## License

Licensed under the [MIT License](LICENSE).
