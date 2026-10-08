[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$workspaceDir = Split-Path -Parent $PSScriptRoot
$files = @(& git -C $workspaceDir -c core.quotepath=false ls-files)
if ($LASTEXITCODE -ne 0 -or $files.Count -eq 0) { throw 'Initialisez Git et indexez les sources publiques avant ce contrôle.' }

$privateId = $null
$localProps = Join-Path $workspaceDir 'Deepchart.local.props'
if (Test-Path -LiteralPath $localProps) {
    [xml]$localSettings = Get-Content -LiteralPath $localProps -Raw
    $privateId = [string]$localSettings.Project.PropertyGroup.DeepchartDevId
    if ($privateId -eq 'YOUR-DEV-ID') { $privateId = $null }
}

$failures = @()
foreach ($file in $files) {
    if ($file -match '(^|/)(bin|obj|artifacts|dist|\.git|\.vs)/|(^|/)Deepchart\.local\.props$|(^|/)\.env($|\.)|\.(dll|exe|pdb|log|pem|key|pfx|p12)$') {
        $failures += "Fichier privé ou binaire indexé : $file"
        continue
    }
    $content = (& git -C $workspaceDir show ":$file") -join "`n"
    if ($LASTEXITCODE -ne 0) { throw "Lecture de l'index impossible : $file" }
    if ($privateId -and $content.Contains($privateId)) { $failures += "Identifiant personnel trouvé dans : $file" }
    if ($content -match '(?i)[a-z]:[\\/]{1,2}Users[\\/]{1,2}') { $failures += "Chemin de compte Windows trouvé dans : $file" }
    if ($content -match '\b(ghp_[a-zA-Z0-9]{30,}|github_pat_[a-zA-Z0-9_]{30,}|sk-(proj-)?[a-zA-Z0-9_-]{30,}|AKIA[0-9A-Z]{16})\b') {
        $failures += "Motif de secret trouvé dans : $file"
    }
}
if ($failures.Count -gt 0) { throw ($failures -join "`n") }
Write-Output "Sources publiques vérifiées : $($files.Count) fichiers. Aucun identifiant personnel ni chemin de compte détecté."
