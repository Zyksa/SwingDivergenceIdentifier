[CmdletBinding()]
param([string]$SourceDirectory = (Join-Path $PSScriptRoot '../src'))

$ErrorActionPreference = 'Stop'
$sourceDir = (Resolve-Path -LiteralPath $SourceDirectory).Path
$files = @(Get-ChildItem -LiteralPath $sourceDir -Recurse -File -Filter '*.cs' |
    Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' })
if ($files.Count -eq 0) { throw 'Aucune source C# à contrôler.' }
$recordDeclaration = '(?m)^\s*(?:(?:public|internal|private|protected|sealed|abstract|readonly|partial)\s+)*record\b'
$failures = @()
foreach ($file in $files) {
    $content = [IO.File]::ReadAllText($file.FullName)
    if ($content -match $recordDeclaration) { $failures += $file.Name }
}
if ($failures.Count -gt 0) {
    throw ('Déclarations record refusées par le compilateur de packages Deepcharts : ' + ($failures -join ', '))
}
Write-Output "Sources Deepcharts : $($files.Count) fichiers, aucune déclaration record. Ce contrôle ne remplace pas le validateur de la plateforme."
