Set-StrictMode -Version 2.0

function Test-DeepchartSdkDirectory {
    param([string]$Path)
    if ([string]::IsNullOrWhiteSpace($Path)) { return $false }
    return (Test-Path -LiteralPath (Join-Path $Path 'VolSysAPI.dll') -PathType Leaf) -and
        (Test-Path -LiteralPath (Join-Path $Path 'VolumetricaCore.dll') -PathType Leaf)
}

function Read-InstallerSettings {
    param([string]$Path)
    $result = [pscustomobject]@{ DeveloperId = ''; DeepchartDirectory = '' }
    if (!(Test-Path -LiteralPath $Path -PathType Leaf)) { return $result }
    $settings = [System.Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $reader = [System.Xml.XmlReader]::Create($Path, $settings)
    try {
        $document = [System.Xml.XmlDocument]::new()
        $document.XmlResolver = $null
        $document.Load($reader)
        $idNode = $document.SelectSingleNode("//*[local-name()='DeepchartDevId']")
        $directoryNode = $document.SelectSingleNode("//*[local-name()='DeepchartDir']")
        if ($null -ne $idNode) { $result.DeveloperId = $idNode.InnerText.Trim() }
        if ($null -ne $directoryNode) { $result.DeepchartDirectory = $directoryNode.InnerText }
        return $result
    } finally { $reader.Dispose() }
}

function Test-InstallerDeveloperId {
    param([string]$Value)
    return ![string]::IsNullOrWhiteSpace($Value) -and $Value.Trim() -ne 'YOUR-DEV-ID' -and
        $Value.Length -le 256 -and $Value -notmatch '[\x00-\x1f]'
}

function Save-InstallerSettings {
    param([string]$Path, [string]$DeveloperId, [string]$DeepchartDirectory)
    if (!(Test-InstallerDeveloperId $DeveloperId)) { throw 'Developer ID invalide.' }
    $document = [System.Xml.XmlDocument]::new()
    $document.XmlResolver = $null
    if (Test-Path -LiteralPath $Path -PathType Leaf) {
        $readerSettings = [System.Xml.XmlReaderSettings]::new()
        $readerSettings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
        $readerSettings.XmlResolver = $null
        $reader = [System.Xml.XmlReader]::Create($Path, $readerSettings)
        try { $document.Load($reader) } finally { $reader.Dispose() }
    } else { [void]$document.AppendChild($document.CreateElement('Project')) }
    if ($document.DocumentElement.LocalName -ne 'Project') { throw 'Fichier de configuration MSBuild invalide.' }
    $group = $document.SelectSingleNode("/*[local-name()='Project']/*[local-name()='PropertyGroup']")
    if ($null -eq $group) {
        $group = $document.CreateElement('PropertyGroup', $document.DocumentElement.NamespaceURI)
        [void]$document.DocumentElement.AppendChild($group)
    }
    foreach ($item in @(@('DeepchartDir', $DeepchartDirectory), @('DeepchartDevId', $DeveloperId.Trim()))) {
        $node = $document.SelectSingleNode("//*[local-name()='$($item[0])']")
        if ($null -eq $node) {
            $node = $document.CreateElement($item[0], $document.DocumentElement.NamespaceURI)
            [void]$group.AppendChild($node)
        }
        $node.InnerText = $item[1]
    }
    $writerSettings = [System.Xml.XmlWriterSettings]::new()
    $writerSettings.Encoding = [System.Text.UTF8Encoding]::new($false)
    $writerSettings.Indent = $true
    $fullPath = [IO.Path]::GetFullPath($Path)
    $privateDirectory = Join-Path (Split-Path -Parent $fullPath) 'artifacts'
    New-Item -ItemType Directory -Path $privateDirectory -Force | Out-Null
    $temporaryPath = Join-Path $privateDirectory ('installer-config-' + [Guid]::NewGuid().ToString('N') + '.props')
    try {
        $writer = [System.Xml.XmlWriter]::Create($temporaryPath, $writerSettings)
        try { $document.Save($writer) } finally { $writer.Dispose() }
        Move-Item -LiteralPath $temporaryPath -Destination $fullPath -Force
    } finally {
        if (Test-Path -LiteralPath $temporaryPath -PathType Leaf) { Remove-Item -LiteralPath $temporaryPath }
    }
}

function Find-InstallerPowerShell {
    $candidates = @()
    if ($PSVersionTable.PSVersion.Major -ge 7) { $candidates += Join-Path $PSHOME 'pwsh.exe' }
    $command = Get-Command pwsh.exe -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($null -ne $command) { $candidates += $command.Source }
    if ($env:ProgramFiles) { $candidates += Join-Path $env:ProgramFiles 'PowerShell\7\pwsh.exe' }
    if ($env:LOCALAPPDATA) { $candidates += Join-Path $env:LOCALAPPDATA 'Microsoft\PowerShell\7\pwsh.exe' }
    foreach ($candidate in ($candidates | Select-Object -Unique)) {
        if (!(Test-Path -LiteralPath $candidate -PathType Leaf)) { continue }
        $major = & $candidate -NoLogo -NoProfile -Command '$PSVersionTable.PSVersion.Major' 2>$null
        if ($LASTEXITCODE -eq 0 -and [int]$major -ge 7) { return $candidate }
    }
    return $null
}

function Find-InstallerDotNet {
    $candidates = @()
    $command = Get-Command dotnet.exe -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($null -ne $command) { $candidates += $command.Source }
    if ($env:ProgramFiles) { $candidates += Join-Path $env:ProgramFiles 'dotnet\dotnet.exe' }
    foreach ($candidate in ($candidates | Select-Object -Unique)) {
        if (!(Test-Path -LiteralPath $candidate -PathType Leaf)) { continue }
        $sdks = & $candidate --list-sdks 2>$null
        if ($LASTEXITCODE -eq 0 -and @($sdks | Where-Object { $_ -match '^10\.0\.\d+\s' }).Count -gt 0) { return $candidate }
    }
    return $null
}

function Find-InstallerDeepchart {
    param([string]$ConfiguredDirectory)
    $candidates = @($ConfiguredDirectory)
    if ($env:ProgramFiles) { $candidates += Join-Path $env:ProgramFiles 'Volumetrica Trading\Deepchart' }
    if (${env:ProgramFiles(x86)}) { $candidates += Join-Path ${env:ProgramFiles(x86)} 'Volumetrica Trading\Deepchart' }
    if ($env:LOCALAPPDATA) { $candidates += Join-Path $env:LOCALAPPDATA 'Volumetrica Trading\Deepchart' }
    foreach ($process in @(Get-Process -Name Deepchart -ErrorAction SilentlyContinue)) {
        try { if ($process.Path) { $candidates += Split-Path -Parent $process.Path } } catch { }
    }
    foreach ($registryRoot in @('HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*',
        'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*',
        'HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*')) {
        foreach ($app in @(Get-ItemProperty -Path $registryRoot -ErrorAction SilentlyContinue)) {
            if ($app.PSObject.Properties['DisplayName'] -and $app.DisplayName -match 'Deepchart') {
                if ($app.PSObject.Properties['InstallLocation']) { $candidates += $app.InstallLocation }
            }
        }
    }
    foreach ($candidate in ($candidates | Where-Object { ![string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique)) {
        if (Test-DeepchartSdkDirectory $candidate) { return [IO.Path]::GetFullPath($candidate) }
    }
    return $null
}

function Install-InstallerPrerequisite {
    param([ValidateSet('Microsoft.PowerShell', 'Microsoft.DotNet.SDK.10')][string]$PackageId,
        [switch]$NonInteractive, [switch]$AllowInstall)
    if (!$AllowInstall) {
        if ($NonInteractive) { throw "Prerequis absent : $PackageId. Installez-le ou utilisez -InstallPrerequisites." }
        Write-Host "Prerequis Microsoft manquant : $PackageId."
        $answer = Read-Host 'Installer ce prerequis via WinGet ? [O/n]'
        if ($answer -and $answer.Trim() -notmatch '^(o|oui|y|yes)$') { throw 'Installation des prerequis annulee.' }
    }
    $winget = Get-Command winget.exe -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($null -eq $winget) { throw "WinGet absent. Installez $PackageId depuis Microsoft, puis relancez Installer.bat." }
    & $winget.Source install --id $PackageId --exact --source winget --accept-source-agreements --accept-package-agreements --disable-interactivity
    if ($LASTEXITCODE -ne 0) { throw "Installation de $PackageId incomplete (code $LASTEXITCODE). Relancez apres installation manuelle." }
    $env:Path = [Environment]::GetEnvironmentVariable('Path', 'Machine') + ';' +
        [Environment]::GetEnvironmentVariable('Path', 'User') + ';' + $env:Path
}

Export-ModuleMember -Function Test-DeepchartSdkDirectory, Read-InstallerSettings, Test-InstallerDeveloperId,
    Save-InstallerSettings, Find-InstallerPowerShell, Find-InstallerDotNet, Find-InstallerDeepchart, Install-InstallerPrerequisite
