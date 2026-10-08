# Source layout

| Location | Responsibility |
| --- | --- |
| `SwingDivergenceIdentifier.cs` | Registration and indicator lifecycle. |
| `GlobalUsings.cs` | Explicit common namespaces, included in the platform source package. |
| `Analysis/` | Structure, CVD, divergence and execution engines; no SDK dependency. |
| `Structure/` | Confirmed window-pivot detector. |
| `Configuration/` | Public settings, defaults and compatibility migrations. |
| `Rendering/` | Chart annotation creation and removal. |
| `Runtime/` | SDK transactions, live previews, alerts and execution-cache restoration. |

The partial class is `SwingDivergence.SwingDivergenceIdentifier`. The assembly
is `SwingDivergenceIdentifier.dll`. These technical identifiers were renamed
with the project in version 1.0.16; previous workspace references require adding
the renamed indicator again. `SwingDivergenceIdentifier.csproj` references the SDK supplied by the local installation.

For Deepcharts **Add package**, export the production sources with
`scripts/Export-DeepchartsPackage.ps1` from the repository root. Include
`GlobalUsings.cs`; platform compilation does not read this project's MSBuild
configuration. No SDK DLL, project file, test or diagnostic belongs in that upload.
