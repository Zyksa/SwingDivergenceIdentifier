# Contributing

Use .NET SDK 10 and start with the SDK-free engine checks:

```powershell
dotnet run --project tests/StructureChecks.csproj -c Release
```

The production indicator is in `src/`. Pure analysis engines live in
`src/Analysis/`; settings are in `src/Configuration/`, annotations in
`src/Rendering/`, and SDK integration in `src/Runtime/`. `src/GlobalUsings.cs`
provides the common namespaces for both local and source-package builds.
Keep the assembly name and `SwingDivergenceIdentifier` class stable for saved workspaces.

Add regression scenarios for meaningful behavior changes: causal confirmation,
wick rejection, neutral pauses, session boundaries, missing CVD, live alert
deduplication and parameter recalculation. Keep fixture data synthetic.

With the SDK installed, run the adapter suite and build compatibility check:

```powershell
dotnet run --project tests/AdapterChecks/AdapterChecks.csproj -c Release
pwsh -File scripts/Build-Indicator.ps1
```

Follow the [Deepcharts allowed API](https://docs-indicators.deepcharts.com/troubleshooting).
Production indicators must not use filesystem, network, reflection, threads or
process APIs. The Deepcharts source-package validator also rejects `record`
declarations, including `record struct`. Use ordinary classes or structs and
preserve the explicit value comparisons required by the runtime. `yield` state machines can introduce forbidden references even when
the source appears harmless. The compatibility script checks common sensitive
references; Deepcharts' own validator remains authoritative.

GitHub CI runs the SDK-free suite, public-source check and Deepcharts syntax check. Do not add SDK DLLs,
developer IDs, account configuration, private chart screenshots or real trades
to commits. Stage intended changes and run `scripts/Test-PublicSource.ps1` before
submitting a pull request. See [GitHub preparation](docs/GITHUB_PUBLICATION.md).

For fixture regeneration, pass the separate reference implementation explicitly:

```text
python tests/generate_reference.py /path/to/phidias-dxfeed/core/divergence.py
```

Regeneration is optional; the checked-in fixture makes tests self-contained.
State the issue, resulting behavior and validation in pull requests. Contributions
are provided under this project's MIT license.
