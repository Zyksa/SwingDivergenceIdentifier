[CmdletBinding()]
param([string]$OutputDirectory)

$ErrorActionPreference = 'Stop'
$workspaceDir = Split-Path -Parent $PSScriptRoot
$sourceDir = Join-Path $workspaceDir 'src'
& (Join-Path $PSScriptRoot 'Test-DeepchartsSource.ps1') -SourceDirectory $sourceDir
$sourceFiles = @(Get-ChildItem -LiteralPath $sourceDir -Recurse -File -Filter '*.cs' |
    Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } | Sort-Object Name)
if ($sourceFiles.Count -eq 0) { throw 'Aucune source de production trouvée.' }
if (!($sourceFiles.Name -contains 'GlobalUsings.cs')) { throw 'GlobalUsings.cs doit être inclus dans le package.' }
$duplicates = @($sourceFiles | Group-Object Name | Where-Object Count -gt 1)
if ($duplicates.Count -gt 0) { throw 'Noms de fichiers identiques : export plat impossible.' }
$totalBytes = ($sourceFiles | Measure-Object Length -Sum).Sum
if ($sourceFiles.Count -gt 50 -or $totalBytes -gt 2048 * 1024) {
    throw 'Le package dépasse la limite du formulaire : 50 fichiers / 2048 KB.'
}
if (!$OutputDirectory) { $OutputDirectory = Join-Path $workspaceDir 'dist/swing-divergence-identifier-deepcharts' }
$outputDir = [IO.Path]::GetFullPath($OutputDirectory)
$insideSources = $outputDir.StartsWith($sourceDir + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)
if ([string]::Equals($outputDir.TrimEnd('\','/'), $sourceDir.TrimEnd('\','/'), [StringComparison]::OrdinalIgnoreCase) -or $insideSources) {
    throw 'Le dossier de sortie doit être distinct des sources.'
}
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
$unexpected = @(Get-ChildItem -LiteralPath $outputDir -Force | Where-Object { $_.PSIsContainer -or $_.Name -notin $sourceFiles.Name })
if ($unexpected.Count -gt 0) { throw 'Le dossier de sortie contient des fichiers étrangers. Utilisez un dossier vide.' }
foreach ($file in $sourceFiles) {
    Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $outputDir $file.Name) -Force
}
Write-Output "Package C# prêt : $outputDir"
Write-Output "$($sourceFiles.Count) fichiers / $([Math]::Round($totalBytes / 1024, 1)) KB. Sélectionnez tous les .cs ensemble dans Source files."
