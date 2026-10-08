[CmdletBinding()]
param(
    [string]$DeepchartDevId,
    [string]$DeepchartDir,
    [string]$InstallDirectory,
    [switch]$Install,
    [switch]$PublicBuild
)

$ErrorActionPreference = 'Stop'
if ($PublicBuild -and ($Install -or $DeepchartDevId)) {
    throw 'PublicBuild ne peut pas être installé ni inclure un developer ID personnel.'
}
$workspaceDir = Split-Path -Parent $PSScriptRoot
$projectFile = Join-Path $workspaceDir 'src\SwingDivergenceIdentifier.csproj'
& (Join-Path $PSScriptRoot 'Test-DeepchartsSource.ps1')
$buildArgs = @('build', $projectFile, '-c', 'Release')
if ($PublicBuild) {
    $publicOutput = Join-Path $workspaceDir 'dist/public'
    $buildArgs += '-p:PublicBuild=true'
    $buildArgs += "-p:OutputPath=$publicOutput/"
}
if ($DeepchartDevId) { $buildArgs += "-p:DeepchartDevId=$DeepchartDevId" }
if ($DeepchartDir) { $buildArgs += "-p:DeepchartDir=$DeepchartDir" }
& dotnet @buildArgs
if ($LASTEXITCODE -ne 0) { throw 'Compilation échouée. Aucune installation effectuée.' }

$builtDll = Join-Path $workspaceDir 'src\bin\Release\net10.0\SwingDivergenceIdentifier.dll'
if ($PublicBuild) { $builtDll = Join-Path $publicOutput 'SwingDivergenceIdentifier.dll' }
if (!(Test-Path -LiteralPath $builtDll)) { throw "DLL introuvable : $builtDll" }
& (Join-Path $PSScriptRoot 'Test-IndicatorCompatibility.ps1') -Path $builtDll
if ($Install) {
    # Resolve the same settings as the build, including Deepchart.local.props.
    $propertyArgs = @('msbuild', $projectFile, '-nologo', '-getProperty:DeepchartDevId')
    if ($DeepchartDevId) { $propertyArgs += "-p:DeepchartDevId=$DeepchartDevId" }
    $resolvedDevId = & dotnet @propertyArgs
    if ($LASTEXITCODE -ne 0) { throw 'Impossible de lire DeepchartDevId.' }
    if ([string]::IsNullOrWhiteSpace($resolvedDevId) -or $resolvedDevId.Trim() -eq 'YOUR-DEV-ID') {
        throw 'Installation impossible : renseignez votre DeepchartDevId pour que Deepcharts charge la DLL.'
    }
    & (Join-Path $PSScriptRoot 'Test-LocalIndicatorMetadata.ps1') -Path $builtDll -ExpectedDeveloperId $resolvedDevId.Trim()

    $documentsDir = [Environment]::GetFolderPath('MyDocuments')
    $indicatorDir = if ($InstallDirectory) { [IO.Path]::GetFullPath($InstallDirectory) }
        else { Join-Path $documentsDir 'Deepchart\Indicators' }
    New-Item -ItemType Directory -Path $indicatorDir -Force | Out-Null
    Copy-Item -LiteralPath $builtDll -Destination (Join-Path $indicatorDir 'SwingDivergenceIdentifier.dll') -Force
    # Replace the prior project identity only after the renamed binary is installed.
    $previousDll = Join-Path $indicatorDir 'DivIndicator.dll'
    if (Test-Path -LiteralPath $previousDll) {
        $backupDir = Join-Path $workspaceDir ('artifacts/previous-project-identity-' + [Guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
        Copy-Item -LiteralPath $previousDll -Destination (Join-Path $backupDir 'DivIndicator.dll') -Force
        Remove-Item -LiteralPath $previousDll
        Write-Output "Ancienne identité sauvegardée dans $backupDir. Réajoutez l'indicateur renommé aux graphiques concernés."
    }
    Write-Output "Indicateur installé dans $indicatorDir. Dans Deepcharts : Personal > Swing & Divergence Identifier."
} else {
    Write-Output "DLL compilée : $builtDll"
}
