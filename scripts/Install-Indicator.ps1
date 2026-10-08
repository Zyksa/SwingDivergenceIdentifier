[CmdletBinding()]
param(
    [string]$DeveloperId,
    [string]$DeepchartDirectory,
    [string]$DestinationDirectory,
    [switch]$CheckOnly,
    [switch]$NonInteractive,
    [switch]$InstallPrerequisites
)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'InstallerSupport.psm1') -Force -DisableNameChecking
$projectRoot = Split-Path -Parent $PSScriptRoot
$localConfig = Join-Path $projectRoot 'Deepchart.local.props'
$exitCode = 0
$relaunched = $false
try {
    Write-Host 'Swing & Divergence Identifier - by Zyksa - Beta v0.6' -ForegroundColor Cyan
    if (!(Test-Path -LiteralPath (Join-Path $projectRoot 'src\SwingDivergenceIdentifier.csproj'))) {
        throw 'Dossier incomplet. Extrayez tout le ZIP GitHub avant de lancer Installer.bat.'
    }
    $settings = Read-InstallerSettings $localConfig
    if (!$DeveloperId) { $DeveloperId = $settings.DeveloperId }
    if (!$DeepchartDirectory) { $DeepchartDirectory = Find-InstallerDeepchart $settings.DeepchartDirectory }
    if ($PSVersionTable.PSVersion.Major -lt 7) {
        $powerShell7 = Find-InstallerPowerShell
        if (!$powerShell7) {
            if ($CheckOnly) { throw 'PowerShell 7 absent. Aucune installation effectuee.' }
            Install-InstallerPrerequisite 'Microsoft.PowerShell' -NonInteractive:$NonInteractive -AllowInstall:$InstallPrerequisites
            $powerShell7 = Find-InstallerPowerShell
            if (!$powerShell7) { throw 'PowerShell 7 non trouve. Fermez puis relancez Installer.bat.' }
        }
        $forward = @('-NoLogo', '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $PSCommandPath)
        if ($PSBoundParameters.ContainsKey('DeveloperId')) { $forward += @('-DeveloperId', $DeveloperId) }
        if ($PSBoundParameters.ContainsKey('DeepchartDirectory')) {
            # Windows PowerShell's native argument marshalling treats a trailing
            # backslash before a quote specially. A directory's "\." is equivalent.
            $forwardDirectory = $DeepchartDirectory
            if ($forwardDirectory.EndsWith('\')) { $forwardDirectory += '.' }
            $forward += @('-DeepchartDirectory', $forwardDirectory)
        }
        if ($DestinationDirectory) {
            $forwardDestination = $DestinationDirectory
            if ($forwardDestination.EndsWith('\')) { $forwardDestination += '.' }
            $forward += @('-DestinationDirectory', $forwardDestination)
        }
        if ($CheckOnly) { $forward += '-CheckOnly' }
        if ($NonInteractive) { $forward += '-NonInteractive' }
        if ($InstallPrerequisites) { $forward += '-InstallPrerequisites' }
        $relaunched = $true
        & $powerShell7 @forward
        $exitCode = $LASTEXITCODE
    } else {
        $dotnetPath = Find-InstallerDotNet
        if (!$dotnetPath -and !$CheckOnly) {
            Install-InstallerPrerequisite 'Microsoft.DotNet.SDK.10' -NonInteractive:$NonInteractive -AllowInstall:$InstallPrerequisites
            $dotnetPath = Find-InstallerDotNet
        }
        if (!$dotnetPath) { throw '.NET SDK 10 absent. Le Runtime seul ne permet pas de compiler.' }
        $env:Path = (Split-Path -Parent $dotnetPath) + ';' + $env:Path
        if ($CheckOnly) {
            if (!(Test-DeepchartSdkDirectory $DeepchartDirectory)) { throw 'Dossier Deepcharts/SDK introuvable ou DLL du SDK absentes.' }
            Write-Host 'PowerShell 7 et .NET SDK 10 : OK.'
            Write-Host "SDK Deepcharts : $DeepchartDirectory"
            if (Test-InstallerDeveloperId $DeveloperId) { Write-Host 'Developer ID deja configure (valeur masquee).' }
            else { Write-Host 'Developer ID a saisir lors de la premiere installation.' }
            Write-Host 'Verification terminee. Aucun fichier modifie ni prerequis installe.'
        } else {
            while (!(Test-DeepchartSdkDirectory $DeepchartDirectory)) {
                if ($NonInteractive) { throw 'Indiquez le dossier du SDK avec -DeepchartDirectory.' }
                Write-Host 'Dossier Deepcharts introuvable. Il doit contenir VolSysAPI.dll et VolumetricaCore.dll.'
                $DeepchartDirectory = (Read-Host 'Chemin du dossier Deepcharts').Trim().Trim('"')
                if (!$DeepchartDirectory) { throw 'Installation annulee.' }
            }
            while (!(Test-InstallerDeveloperId $DeveloperId)) {
                if ($NonInteractive) { throw 'Developer ID absent : utilisez -DeveloperId ou configurez Deepchart.local.props.' }
                Write-Host 'Deepcharts charge uniquement une DLL compilee avec votre propre developer ID.'
                Write-Host 'https://docs-indicators.deepcharts.com/hybrid/developer-id'
                $DeveloperId = Read-Host 'Votre developer ID Deepcharts'
                if (!$DeveloperId) { throw 'Installation annulee.' }
            }
            $DeepchartDirectory = [IO.Path]::GetFullPath($DeepchartDirectory)
            $documentsPath = [Environment]::GetFolderPath('MyDocuments')
            $targetDirectory = if ($DestinationDirectory) { [IO.Path]::GetFullPath($DestinationDirectory) }
                else { Join-Path $documentsPath 'Deepchart\Indicators' }
            $targetDll = Join-Path $targetDirectory 'SwingDivergenceIdentifier.dll'
            if (Test-Path -LiteralPath $targetDll) {
                $backupDirectory = Join-Path $projectRoot ('artifacts\installer-backup-' + [Guid]::NewGuid().ToString('N'))
                New-Item -ItemType Directory -Path $backupDirectory -Force | Out-Null
                Copy-Item -LiteralPath $targetDll -Destination (Join-Path $backupDirectory 'SwingDivergenceIdentifier.dll')
                Write-Host "Version precedente sauvegardee : $backupDirectory"
            }
            Save-InstallerSettings $localConfig $DeveloperId $DeepchartDirectory
            Write-Host 'Configuration locale enregistree. Le developer ID reste exclu de Git.'
            Push-Location -LiteralPath $projectRoot
            try {
                & (Join-Path $PSScriptRoot 'Build-Indicator.ps1') -Install -InstallDirectory $targetDirectory
                $compiledDll = Join-Path $projectRoot 'src\bin\Release\net10.0\SwingDivergenceIdentifier.dll'
                if ((Get-FileHash -LiteralPath $compiledDll).Hash -ne (Get-FileHash -LiteralPath $targetDll).Hash) {
                    throw 'La DLL installee ne correspond pas au build. Fermez Deepcharts et relancez.'
                }
            } finally { Pop-Location }
            Write-Host 'Installation reussie !' -ForegroundColor Green
            Write-Host 'Redemarrez Deepcharts : Personal > Swing & Divergence Identifier.'
        }
    }
} catch {
    $exitCode = 1
    Write-Host ('Installation arretee : ' + $_.Exception.Message) -ForegroundColor Red
    Write-Host 'Aucune protection Windows ni regle Defender n''a ete modifiee.'
} finally {
    if (!$relaunched -and !$CheckOnly -and !$NonInteractive) { [void](Read-Host 'Appuyez sur Entree pour fermer') }
}
exit $exitCode
