[CmdletBinding()]
param(
    [string]$OutputDirectory,
    # One .cs file: the Windows file picker can silently drop part of a large multi-selection.
    [switch]$SingleFile
)

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
if (!$OutputDirectory) {
    $OutputDirectory = Join-Path $workspaceDir ($SingleFile ? 'dist/swing-divergence-identifier-deepcharts-single' : 'dist/swing-divergence-identifier-deepcharts')
}
$outputDir = [IO.Path]::GetFullPath($OutputDirectory)
$insideSources = $outputDir.StartsWith($sourceDir + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)
if ([string]::Equals($outputDir.TrimEnd('\','/'), $sourceDir.TrimEnd('\','/'), [StringComparison]::OrdinalIgnoreCase) -or $insideSources) {
    throw 'Le dossier de sortie doit être distinct des sources.'
}
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
if ($SingleFile) {
    $bundleName = 'SwingDivergenceIdentifierPackage.cs'
    $unexpected = @(Get-ChildItem -LiteralPath $outputDir -Force | Where-Object { $_.PSIsContainer -or $_.Name -ne $bundleName })
    if ($unexpected.Count -gt 0) { throw 'Le dossier de sortie contient des fichiers étrangers. Utilisez un dossier vide.' }
    $globalFile = $sourceFiles | Where-Object Name -eq 'GlobalUsings.cs'
    $out = [Collections.Generic.List[string]]::new()
    $out.Add('// Swing & Divergence Identifier : sources de production réunies pour le formulaire Add package.')
    $out.Add('// Fichier généré par scripts/Export-DeepchartsPackage.ps1 -SingleFile ; ne pas modifier à la main.')
    # Global usings must precede every namespace declaration.
    foreach ($line in [IO.File]::ReadAllLines($globalFile.FullName)) { $out.Add($line) }
    foreach ($file in $sourceFiles | Where-Object Name -ne 'GlobalUsings.cs') {
        $lines = [IO.File]::ReadAllLines($file.FullName)
        $nsIndex = -1
        for ($i = 0; $i -lt $lines.Count; $i++) { if ($lines[$i] -match '^\s*namespace\s+[\w.]+\s*;\s*$') { $nsIndex = $i; break } }
        if ($nsIndex -lt 0) { throw "$($file.Name) : namespace de fichier attendu." }
        $header = $lines[0..$nsIndex]
        $usings = @($header | Where-Object { $_ -match '^\s*using\s' })
        $other = @($header[0..($nsIndex - 1)] | Where-Object { $_.Trim() -ne '' -and $_ -notmatch '^\s*using\s' -and $_ -notmatch '^\s*//' })
        if ($nsIndex -gt 0 -and $other.Count -gt 0) { throw "$($file.Name) : contenu inattendu avant le namespace." }
        $namespace = ($lines[$nsIndex] -replace '^\s*namespace\s+([\w.]+)\s*;\s*$', '$1')
        $out.Add(''); $out.Add("// ---- $($file.Name) ----")
        $out.Add("namespace $namespace"); $out.Add('{')
        # Usings stay scoped to the original file's block.
        foreach ($u in $usings) { $out.Add($u) }
        for ($i = $nsIndex + 1; $i -lt $lines.Count; $i++) { $out.Add($lines[$i]) }
        $out.Add('}')
    }
    $bundlePath = Join-Path $outputDir $bundleName
    [IO.File]::WriteAllLines($bundlePath, $out, [Text.UTF8Encoding]::new($false))
    $bundleSize = (Get-Item -LiteralPath $bundlePath).Length
    Write-Output "Package C# prêt : $bundlePath"
    Write-Output "1 fichier ($($sourceFiles.Count) sources réunies) / $([Math]::Round($bundleSize / 1024, 1)) KB. Sélectionnez ce seul fichier dans Source files."
    return
}
$unexpected = @(Get-ChildItem -LiteralPath $outputDir -Force | Where-Object { $_.PSIsContainer -or $_.Name -notin $sourceFiles.Name })
if ($unexpected.Count -gt 0) { throw 'Le dossier de sortie contient des fichiers étrangers. Utilisez un dossier vide.' }
foreach ($file in $sourceFiles) {
    Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $outputDir $file.Name) -Force
}
Write-Output "Package C# prêt : $outputDir"
Write-Output "$($sourceFiles.Count) fichiers / $([Math]::Round($totalBytes / 1024, 1)) KB. Sélectionnez tous les .cs ensemble dans Source files."
