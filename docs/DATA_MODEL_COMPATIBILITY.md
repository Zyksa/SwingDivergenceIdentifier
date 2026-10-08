# Data-model compatibility

Deepcharts rejected the source package's 16 `record` declarations, including
`record struct`. Version 1.0.18 uses ordinary `readonly struct` declarations.

The replacement preserves constructor argument order/names and optional defaults,
init-only properties, deconstruction and field-by-field equality. Hash codes
remain consistent with equality; no algorithm relies on their exact numeric value.
Updates copy the struct before setting selected properties, keeping other fields
and the original unchanged. Reference-valued fields retain shallow-copy behavior.

The existing C# `with` expressions apply to ordinary structures too; they do not
require record declarations. [Microsoft language reference](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/with-expression)

Verification: 90 engine checks pass, including the original calculation fixtures,
copy/equality/null/NaN/default-value scenarios, CVD categories and trade snapshots. The SDK adapter suite and
all 30 exported production files compile. The Windows policy previously blocked
adapter execution; no workaround or security-policy change was used.

`Test-DeepchartsSource.ps1` rejects record declarations before build/export and
in CI. This checks the reported restriction, not every rule of the proprietary
platform validator. Final package acceptance and native rendering remain manual.
