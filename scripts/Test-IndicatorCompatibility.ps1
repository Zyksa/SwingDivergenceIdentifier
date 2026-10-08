[CmdletBinding()]
param([string]$Path = (Join-Path $PSScriptRoot '../src/bin/Release/net10.0/SwingDivergenceIdentifier.dll'))

$ErrorActionPreference = 'Stop'
# Read metadata only. This check never executes the indicator or loads its assembly.
$checkedPath = (Resolve-Path -LiteralPath $Path).Path
$inputStream = [IO.File]::OpenRead($checkedPath)
$peReader = [Reflection.PortableExecutable.PEReader]::new($inputStream)
try {
    $metadata = [Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($peReader)
    $disallowed = @()
    foreach ($handle in $metadata.TypeReferences) {
        $type = $metadata.GetTypeReference($handle)
        $name = $metadata.GetString($type.Namespace) + '.' + $metadata.GetString($type.Name)
        # Permit the metadata already present in the working indicator baseline,
        # including DeepchartDevId; do not exempt every Assembly*Attribute.
        # IEnumerator is referenced by ordinary generic foreach.
        if ($name -match '^System\.(Environment$|Type$|RuntimeTypeHandle$|Runtime\.CompilerServices\.RuntimeHelpers$|IO\.|Net\.|Reflection\.(?!Assembly(Company|Configuration|FileVersion|InformationalVersion|Metadata|Product|Title|Version)Attribute$)|Threading\.|Runtime\.InteropServices\.|Diagnostics\.Process$|Collections\.IEnumerable$)') {
            $disallowed += $name
        }
    }
    if ($disallowed.Count -gt 0) {
        throw ('References incompatibles avec les usages autorises par Deepcharts : ' + (($disallowed | Sort-Object -Unique) -join ', '))
    }
    Write-Output 'Controle des references sensibles : OK (metadonnees uniquement).'
} finally {
    $peReader.Dispose()
    $inputStream.Dispose()
}
