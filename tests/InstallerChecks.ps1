[CmdletBinding()]
param([switch]$Integration)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
Import-Module (Join-Path $projectRoot 'scripts/InstallerSupport.psm1') -Force -DisableNameChecking
$testRoot = Join-Path $projectRoot ('artifacts/installer-checks-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testRoot | Out-Null
$passed = 0
function Assert-Installer([bool]$Condition, [string]$Message) { if (!$Condition) { throw $Message } }
function Test-Case([string]$Name, [scriptblock]$Action) { & $Action; $script:passed++; Write-Output "OK - $Name" }

Test-Case 'XML special characters and saved ID round-trip without command interpolation' {
    $path = Join-Path $testRoot 'Deepchart.local.props'
    $exampleId = 'test-id-&-quote-"-dollar-$-semicolon-;'
    $exampleDirectory = Join-Path $testRoot 'SDK & space'
    Save-InstallerSettings $path $exampleId $exampleDirectory
    $read = Read-InstallerSettings $path
    Assert-Installer ($read.DeveloperId -ceq $exampleId -and $read.DeepchartDirectory -ceq $exampleDirectory) 'XML escaping lost a value.'
}
Test-Case 'Existing MSBuild namespace and unrelated properties survive' {
    $path = Join-Path $testRoot 'existing.props'
    [IO.File]::WriteAllText($path, '<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003"><PropertyGroup><Keep>yes</Keep><DeepchartDevId>old</DeepchartDevId></PropertyGroup></Project>')
    Save-InstallerSettings $path 'new-id' 'sdk-folder'
    [xml]$document = [IO.File]::ReadAllText($path)
    Assert-Installer ($document.SelectSingleNode("//*[local-name()='Keep']").InnerText -eq 'yes') 'Unrelated property removed.'
    Assert-Installer ((Read-InstallerSettings $path).DeveloperId -eq 'new-id') 'Namespaced ID not saved.'
}
Test-Case 'Invalid IDs and external XML entities are rejected' {
    Assert-Installer (!(Test-InstallerDeveloperId '') -and !(Test-InstallerDeveloperId 'YOUR-DEV-ID') -and
        !(Test-InstallerDeveloperId "test`nother")) 'Invalid ID accepted.'
    $path = Join-Path $testRoot 'external.props'
    [IO.File]::WriteAllText($path, '<!DOCTYPE Project [<!ENTITY sample SYSTEM "file:///not-a-config">]><Project><PropertyGroup><DeepchartDevId>&sample;</DeepchartDevId></PropertyGroup></Project>')
    $rejected = $false
    try { Read-InstallerSettings $path | Out-Null } catch { $rejected = $true }
    Assert-Installer $rejected 'External entity accepted.'
}
Test-Case 'SDK directory requires both libraries' {
    $sdk = Join-Path $testRoot 'SDK & space'
    New-Item -ItemType Directory -Path $sdk -Force | Out-Null
    [IO.File]::WriteAllText((Join-Path $sdk 'VolSysAPI.dll'), '')
    Assert-Installer (!(Test-DeepchartSdkDirectory $sdk)) 'Incomplete SDK accepted.'
    [IO.File]::WriteAllText((Join-Path $sdk 'VolumetricaCore.dll'), '')
    Assert-Installer (Test-DeepchartSdkDirectory $sdk) 'Complete SDK directory refused.'
}
Test-Case 'Every installer script parses as PowerShell' {
    foreach ($name in @('Install-Indicator.ps1','InstallerSupport.psm1','Build-Indicator.ps1')) {
        $parseErrors = $null; $tokens = $null
        [void][System.Management.Automation.Language.Parser]::ParseFile((Join-Path $projectRoot "scripts/$name"), [ref]$tokens, [ref]$parseErrors)
        Assert-Installer ($parseErrors.Count -eq 0) "Parser error in $name."
    }
}
Test-Case 'Noninteractive prerequisite handling does not install packages without opt-in' {
    $rejected = $false
    try { Install-InstallerPrerequisite 'Microsoft.DotNet.SDK.10' -NonInteractive } catch { $rejected = $true }
    Assert-Installer $rejected 'Prerequisite install unexpectedly started.'
}

if ($Integration) {
    $realDll = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Deepchart/Indicators/SwingDivergenceIdentifier.dll'
    $realHash = if (Test-Path -LiteralPath $realDll) { (Get-FileHash -LiteralPath $realDll).Hash } else { $null }
    $sdk = Find-InstallerDeepchart ''
    Assert-Installer (Test-DeepchartSdkDirectory $sdk) 'Integration test requires the installed Deepcharts SDK.'
    $sandbox = Join-Path $testRoot 'Extracted ZIP & spaces'
    New-Item -ItemType Directory -Path $sandbox | Out-Null
    Copy-Item -LiteralPath (Join-Path $projectRoot 'Installer.bat') -Destination $sandbox
    Copy-Item -LiteralPath (Join-Path $projectRoot 'global.json') -Destination $sandbox
    New-Item -ItemType Directory -Path (Join-Path $sandbox 'src'), (Join-Path $sandbox 'scripts') | Out-Null
    $sourceRoot = Join-Path $projectRoot 'src'
    foreach ($file in @(Get-ChildItem -LiteralPath $sourceRoot -File -Recurse | Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' -and $_.Extension -in @('.cs','.csproj') })) {
        $relative = $file.FullName.Substring($sourceRoot.Length + 1)
        $destination = Join-Path (Join-Path $sandbox 'src') $relative
        New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
        Copy-Item -LiteralPath $file.FullName -Destination $destination
    }
    foreach ($file in @(Get-ChildItem -LiteralPath (Join-Path $projectRoot 'scripts') -File -Filter '*.ps*')) {
        Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $sandbox 'scripts')
    }
    $config = Join-Path $sandbox 'Deepchart.local.props'
    Save-InstallerSettings $config '00000000-0000-0000-0000-000000000123' $sdk
    Test-Case 'ZIP launcher preflight: spaces, ampersand, Windows PowerShell 5 and no writes' {
        $before = (Get-FileHash -LiteralPath $config).Hash
        & (Join-Path $sandbox 'Installer.bat') -CheckOnly -NonInteractive
        Assert-Installer ($LASTEXITCODE -eq 0) 'Batch preflight failed.'
        Assert-Installer ((Get-FileHash -LiteralPath $config).Hash -eq $before) 'Preflight changed the config.'
    }
    Test-Case 'Complete source build and isolated installation, without touching real indicators' {
        $destination = Join-Path $sandbox 'Fake Documents & folder'
        & (Join-Path $sandbox 'Installer.bat') -NonInteractive -DestinationDirectory $destination
        Assert-Installer ($LASTEXITCODE -eq 0) 'Isolated installation failed.'
        $target = Join-Path $destination 'SwingDivergenceIdentifier.dll'
        $built = Join-Path $sandbox 'src/bin/Release/net10.0/SwingDivergenceIdentifier.dll'
        Assert-Installer ((Get-FileHash -LiteralPath $target).Hash -eq (Get-FileHash -LiteralPath $built).Hash) 'Installed DLL differs.'
        if ($realHash) { Assert-Installer ((Get-FileHash -LiteralPath $realDll).Hash -eq $realHash) 'Real indicator was modified by the test.' }
    }
}
Write-Output "$passed installer checks passed. Fixtures retained in artifacts."
