[CmdletBinding()]
param([string]$OutputPath)

$ErrorActionPreference = 'Stop'
$workspaceDir = Split-Path -Parent $PSScriptRoot
& (Join-Path $PSScriptRoot 'Test-PublicSource.ps1')
& git -C $workspaceDir diff --quiet
if ($LASTEXITCODE -ne 0) { throw 'Indexez les modifications relues avant export : git add <fichiers-publics>.' }

if (!$OutputPath) { $OutputPath = Join-Path $workspaceDir 'dist/swing-divergence-identifier-source.zip' }
$archivePath = [IO.Path]::GetFullPath($OutputPath)
if ([IO.Path]::GetExtension($archivePath) -ne '.zip') { throw "L'export doit être un fichier .zip." }
New-Item -ItemType Directory -Path (Split-Path -Parent $archivePath) -Force | Out-Null
$tree = & git -C $workspaceDir write-tree
if ($LASTEXITCODE -ne 0) { throw "Impossible de créer l'instantané de l'index." }
& git -C $workspaceDir archive --format=zip --prefix=swing-divergence-identifier/ "--output=$archivePath" $tree
if ($LASTEXITCODE -ne 0) { throw "L'export source a échoué." }
Write-Output "Archive source préparée : $archivePath"
