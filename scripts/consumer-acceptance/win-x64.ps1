<#
.SYNOPSIS
Issue #4 independent acceptance on a clean Windows x64 consumer.
.DESCRIPTION
Runs with inbox Windows PowerShell 5.1; no SDK, PowerShell 7 or extra runtime is installed.
Creates separate raw stdout, stderr, command, exit-code, and host inventory logs.
The inspected synthetic file is inert and is never loaded or executed.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$ArtifactZip,
    [Parameter(Mandatory = $true)][string]$EvidenceDirectory,
    [Parameter(Mandatory = $true)][string]$ProvenanceFile
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$sourceSha = '6dffde08e6a9853c68555c1e751bcd63f082e0bd'
$expectedArtifactSha = '7b83dd06cc7bc513b13ca0dbd92ccaf2425c087c0d7e8c347b7769c9e1e9af7a'
$expectedStandaloneSha = '0b8204ec74f7c1120c51cc2472c246e5827a8ef60c9987e0086a1728c45fea49'
$expectedBinarySha = '972c77903a325c0925c2a0556ecddb329a5410e733373dd5a5d70ea24dee0fd0'
$archiveName = 'dotnet-binary-0.1.0-preview.1-win-x64.zip'

if (-not [Environment]::Is64BitOperatingSystem -or -not [Environment]::Is64BitProcess -or
    -not [Environment]::OSVersion.Platform.Equals([PlatformID]::Win32NT)) {
    throw 'Required: real Windows x64 with a 64-bit Windows PowerShell process.'
}
if (-not (Test-Path -LiteralPath $ArtifactZip -PathType Leaf) -or
    -not (Test-Path -LiteralPath $ProvenanceFile -PathType Leaf) -or
    (Test-Path -LiteralPath $EvidenceDirectory)) {
    throw 'Artifact ZIP and provenance note must exist; evidence directory must not exist.'
}

$artifact = (Resolve-Path -LiteralPath $ArtifactZip).Path
$provenance = (Resolve-Path -LiteralPath $ProvenanceFile).Path
$evidence = [IO.Path]::GetFullPath($EvidenceDirectory)
New-Item -ItemType Directory -Path $evidence -Force | Out-Null
Copy-Item -LiteralPath $provenance -Destination (Join-Path $evidence 'provenance.txt')
@(
    "Source SHA: $sourceSha"
    "Artifact: $artifact"
    "UTC start: $([DateTime]::UtcNow.ToString('o'))"
) | Set-Content -LiteralPath (Join-Path $evidence 'context.txt') -Encoding UTF8

# Both OS-native inventory and independently recorded image provenance are required.
# Windows PowerShell's Windows .NET Framework is NOT the modern .NET shared runtime.
$missingRuntime = $true
$inventory = New-Object System.Collections.Generic.List[string]
$inventory.Add('Requirement: Windows x64, no modern .NET SDK or shared runtime.')
$inventory.Add("OS: $((Get-CimInstance Win32_OperatingSystem).Caption) / build $([Environment]::OSVersion.Version)")
$inventory.Add("CPU: $((Get-CimInstance Win32_Processor | Select-Object -First 1).Name)")
$inventory.Add("64-bit OS/process: $([Environment]::Is64BitOperatingSystem) / $([Environment]::Is64BitProcess)")
$inventory.Add("Windows PowerShell: $($PSVersionTable.PSVersion)")
$dotnetCommands = @(Get-Command dotnet -All -ErrorAction SilentlyContinue)
if ($dotnetCommands.Count -gt 0) {
    $missingRuntime = $false
    foreach ($command in $dotnetCommands) { $inventory.Add("PRESENT on PATH: $($command.Source)") }
}
else { $inventory.Add('dotnet on PATH: absent') }

$paths = @(
    (Join-Path $env:ProgramFiles 'dotnet'),
    (Join-Path ${env:ProgramFiles(x86)} 'dotnet'),
    (Join-Path $env:USERPROFILE '.dotnet'),
    (Join-Path $env:LOCALAPPDATA 'Microsoft\dotnet')
)
foreach ($path in $paths) {
    if (Test-Path -LiteralPath $path) {
        $missingRuntime = $false
        $inventory.Add("PRESENT directory: $path")
    }
    else { $inventory.Add("absent directory: $path") }
}
$registryPaths = @(
    'HKLM:\SOFTWARE\dotnet\Setup\InstalledVersions\x64',
    'HKLM:\SOFTWARE\dotnet\Setup\InstalledVersions\x86',
    'HKLM:\SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x86',
    'HKCU:\SOFTWARE\dotnet\Setup\InstalledVersions\x64',
    'HKCU:\SOFTWARE\dotnet\Setup\InstalledVersions\x86'
)
foreach ($path in $registryPaths) {
    if (Test-Path -LiteralPath $path) {
        $missingRuntime = $false
        $inventory.Add("PRESENT registry key: $path")
    }
    else { $inventory.Add("absent registry key: $path") }
}
$inventory.Add("Clean-host inventory result: $missingRuntime")
$inventory | Set-Content -LiteralPath (Join-Path $evidence 'environment.txt') -Encoding UTF8
if (-not $missingRuntime) {
    'FAIL: .NET runtime/SDK absence not established; no executable run.' |
        Set-Content -LiteralPath (Join-Path $evidence 'summary.txt') -Encoding UTF8
    throw 'Consumer host is not demonstrably SDK/runtime-free.'
}

function Assert-Checksum {
    param([string]$Path, [string]$Expected, [string]$Label)
    $actual = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
    Add-Content -LiteralPath (Join-Path $evidence 'checksums.txt') -Encoding UTF8 -Value @(
        "$Label expected: $Expected", "$Label actual:   $actual"
    )
    if ($actual -ne $Expected) { throw "FAIL: $Label SHA-256 mismatch; do not execute." }
}

Assert-Checksum -Path $artifact -Expected $expectedArtifactSha -Label 'workflow ZIP'
$work = Join-Path $evidence 'work'
Expand-Archive -LiteralPath $artifact -DestinationPath $work
$nativeDirectory = Join-Path $work 'aot\win-x64'
$manifest = Join-Path $nativeDirectory 'SHA256SUMS.txt'
foreach ($line in Get-Content -LiteralPath $manifest) {
    if ($line -match '^([a-fA-F0-9]{64})  ([^\\/]+)$') {
        $hash = $Matches[1].ToLowerInvariant()
        $name = $Matches[2]
    }
    else {
        throw "FAIL: Malformed or unsafe native checksum entry; do not execute: $line"
    }
    if ($name -eq '.' -or $name -eq '..') {
        throw "FAIL: Unsafe native checksum entry; do not execute: $name"
    }
    Assert-Checksum -Path (Join-Path $nativeDirectory $name) -Expected $hash -Label $name
}
$standalone = Join-Path $work "distribution\standalone\$archiveName"
Assert-Checksum -Path $standalone -Expected $expectedStandaloneSha -Label 'standalone ZIP'
$unpacked = Join-Path $work 'standalone'
Expand-Archive -LiteralPath $standalone -DestinationPath $unpacked
$binary = Join-Path $unpacked 'dotnet-binary.exe'
Assert-Checksum -Path $binary -Expected $expectedBinarySha -Label 'native executable'

$fixtureDirectory = Join-Path $work 'fixtures'
New-Item -ItemType Directory -Path $fixtureDirectory | Out-Null
$fixture = Join-Path $fixtureDirectory 'synthetic.exe'
'Inert, deliberately non-PE test fixture for static inspection.' |
    Set-Content -LiteralPath $fixture -Encoding Ascii
$cases = Join-Path $evidence 'cases'
New-Item -ItemType Directory -Path $cases | Out-Null
$script:failures = 0
"Issue #4 win-x64 consumer scenarios, UTC $([DateTime]::UtcNow.ToString('o'))" |
    Set-Content -LiteralPath (Join-Path $evidence 'summary.txt') -Encoding UTF8

function Invoke-Case {
    param([string]$Name, [int]$ExpectedExitCode, [string[]]$Arguments)
    $caseDirectory = Join-Path $cases $Name
    New-Item -ItemType Directory -Path $caseDirectory | Out-Null
    # Store the exact argument vector as JSON; Start-Process joins the tokens.
    @{ executable = $binary; arguments = $Arguments; startUtc = [DateTime]::UtcNow.ToString('o') } |
        ConvertTo-Json -Depth 3 | Set-Content -LiteralPath (Join-Path $caseDirectory 'command.json') -Encoding UTF8
    $quoted = @($Arguments | ForEach-Object {
        if ($_ -match '[\s"]') { '"' + $_ + '"' } else { $_ }
    })
    $stdout = Join-Path $caseDirectory 'stdout.txt'
    $stderr = Join-Path $caseDirectory 'stderr.txt'
    $actualExitCode = 'not-started'
    $process = $null
    try {
        $process = Start-Process -FilePath $binary -ArgumentList ($quoted -join ' ') -NoNewWindow `
            -RedirectStandardOutput $stdout -RedirectStandardError $stderr -PassThru
        if (-not $process.WaitForExit(30000)) {
            $process.Kill()
            $actualExitCode = 'timed-out'
            throw "Timed out: $Name"
        }
        $actualExitCode = $process.ExitCode
    }
    catch {
        $_.ToString() | Set-Content -LiteralPath (Join-Path $caseDirectory 'execution-error.txt') -Encoding UTF8
    }
    finally { if ($null -ne $process) { $process.Dispose() } }
    "$actualExitCode" | Set-Content -LiteralPath (Join-Path $caseDirectory 'exit-code.txt') -Encoding Ascii
    "UTC end: $([DateTime]::UtcNow.ToString('o'))" |
        Set-Content -LiteralPath (Join-Path $caseDirectory 'end-utc.txt') -Encoding UTF8
    if ($actualExitCode -eq $ExpectedExitCode) {
        "PASS $Name expected=$ExpectedExitCode actual=$actualExitCode" |
            Add-Content -LiteralPath (Join-Path $evidence 'summary.txt') -Encoding UTF8
    }
    else {
        "FAIL $Name expected=$ExpectedExitCode actual=$actualExitCode" |
            Add-Content -LiteralPath (Join-Path $evidence 'summary.txt') -Encoding UTF8
        $script:failures++
    }
}

Invoke-Case -Name 'help' -ExpectedExitCode 0 -Arguments @('--help')
Invoke-Case -Name 'version' -ExpectedExitCode 0 -Arguments @('--version')
Invoke-Case -Name 'inspect' -ExpectedExitCode 0 -Arguments @('inspect', $fixture, '--format', 'json', '--sha256')
Invoke-Case -Name 'scan' -ExpectedExitCode 0 -Arguments @('scan', $fixtureDirectory, '--format', 'json', '--max-candidates', '1')
Invoke-Case -Name 'invalid-arity' -ExpectedExitCode 2 -Arguments @('inspect')
$versionPath = Join-Path $cases 'version\stdout.txt'
$version = if (Test-Path -LiteralPath $versionPath) { Get-Content -LiteralPath $versionPath -Raw } else { '' }
if (-not $version.Contains("0.1.0-preview.1+$sourceSha")) {
    'FAIL: Version string does not match the frozen source SHA.' |
        Add-Content -LiteralPath (Join-Path $evidence 'summary.txt') -Encoding UTF8
    $script:failures++
}
$helpPath = Join-Path $cases 'help\stdout.txt'
$helpText = if (Test-Path -LiteralPath $helpPath) { Get-Content -LiteralPath $helpPath -Raw } else { '' }
if ($helpText -notmatch '(?i)usage|commands|options') {
    'FAIL: Help output lacks recognizable usage/commands.' |
        Add-Content -LiteralPath (Join-Path $evidence 'summary.txt') -Encoding UTF8
    $script:failures++
}
foreach ($name in @('inspect', 'scan')) {
    try {
        $json = Get-Content -LiteralPath (Join-Path $cases "$name\stdout.txt") -Raw |
            ConvertFrom-Json -ErrorAction Stop
        if ($json.schemaVersion -ne '1.0.0' -or $json.command -ne $name -or
            $null -eq $json.candidates) {
            throw 'Invalid report schema, command or candidate list.'
        }
    }
    catch {
        "FAIL: $name did not produce the expected v1 JSON report: $($_.Exception.Message)" |
            Add-Content -LiteralPath (Join-Path $evidence 'summary.txt') -Encoding UTF8
        $script:failures++
    }
}
$invalidStdoutPath = Join-Path $cases 'invalid-arity\stdout.txt'
$invalidStderrPath = Join-Path $cases 'invalid-arity\stderr.txt'
$invalidStdout = if (Test-Path -LiteralPath $invalidStdoutPath) { Get-Content -LiteralPath $invalidStdoutPath -Raw } else { '' }
$invalidStderr = if (Test-Path -LiteralPath $invalidStderrPath) { Get-Content -LiteralPath $invalidStderrPath -Raw } else { '' }
if ($invalidStdout.Length -ne 0 -or $invalidStderr -notmatch 'exactly one') {
    'FAIL: Invalid-arity stdout/stderr do not match the CLI contract.' |
        Add-Content -LiteralPath (Join-Path $evidence 'summary.txt') -Encoding UTF8
    $script:failures++
}
"UTC finish: $([DateTime]::UtcNow.ToString('o'))" |
    Add-Content -LiteralPath (Join-Path $evidence 'summary.txt') -Encoding UTF8
"Failed checks: $script:failures" |
    Add-Content -LiteralPath (Join-Path $evidence 'summary.txt') -Encoding UTF8
if ($script:failures -ne 0) { throw "FAIL: $script:failures scenario checks; inspect $evidence" }
Write-Host "PASS: All Windows x64 cases completed; preserve the directory: $evidence"
