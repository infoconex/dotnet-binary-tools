<#
.SYNOPSIS
Restores, publishes, and smoke-tests the native CLI for the host's x64 RID.

.DESCRIPTION
Runs without provisioning the SDK or native compiler prerequisites. The host
must match the requested RID; cross-compilation is not considered validation.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('linux-x64', 'win-x64', 'osx-x64')]
    [string]$RuntimeIdentifier
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$hostRid = if ($IsWindows) { 'win-x64' } elseif ($IsLinux) { 'linux-x64' } elseif ($IsMacOS) { 'osx-x64' } else { throw 'Unsupported operating system for Native AOT.' }
$hostArchitecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture
if ($RuntimeIdentifier -ne $hostRid -or $hostArchitecture -ne [System.Runtime.InteropServices.Architecture]::X64) {
    throw "Native AOT validation requires a matching x64 host: requested $RuntimeIdentifier; host $hostRid ($hostArchitecture)."
}

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Push-Location $root
try {
    $project = 'src/Infoconex.DotNetBinary.Cli/Infoconex.DotNetBinary.Cli.csproj'
    $output = Join-Path 'artifacts/aot' $RuntimeIdentifier

    Write-Host "Source revision: $(& git rev-parse HEAD)"
    Write-Host "PowerShell: $($PSVersionTable.PSVersion); OS: $([System.Runtime.InteropServices.RuntimeInformation]::OSDescription); RID: $RuntimeIdentifier"
    & dotnet --info
    if ($LASTEXITCODE -ne 0) { throw "dotnet --info failed with exit code $LASTEXITCODE." }

    Write-Host "Restoring locked dependencies for all RIDs declared by the CLI project (host publish RID: $RuntimeIdentifier)"
    & dotnet restore $project --configfile nuget.config --locked-mode
    if ($LASTEXITCODE -ne 0) { throw "Native AOT restore failed with exit code $LASTEXITCODE." }

    Write-Host "Publishing Native AOT binary to $output"
    & dotnet publish $project -c Release -r $RuntimeIdentifier --configfile nuget.config --no-restore -p:PublishAot=true -o $output
    if ($LASTEXITCODE -ne 0) { throw "Native AOT publish failed with exit code $LASTEXITCODE." }

    $filename = if ($IsWindows) { 'dotnet-binary.exe' } else { 'dotnet-binary' }
    $binary = Join-Path $output $filename
    if (-not (Test-Path -LiteralPath $binary -PathType Leaf)) { throw "Native executable missing: $binary" }

    & (Resolve-Path -LiteralPath $binary).Path --help
    if ($LASTEXITCODE -ne 0) { throw "Native help smoke test failed with exit code $LASTEXITCODE." }
    & (Resolve-Path -LiteralPath $binary).Path --version
    if ($LASTEXITCODE -ne 0) { throw "Native version smoke test failed with exit code $LASTEXITCODE." }

    # CAP-006: Execute the actual Native AOT reporting path, not merely help/version.
    # The published executable is a controlled local candidate: native PE on Windows,
    # non-PE on Unix. Neither outcome requires loading any inspected assembly.
    $binaryPath = (Resolve-Path -LiteralPath $binary).Path
    $jsonReport = & $binaryPath inspect $binaryPath --format json 2>$null | Out-String
    if ($LASTEXITCODE -ne 0) { throw "Native JSON reporting smoke test failed with exit code $LASTEXITCODE." }
    try {
        $report = $jsonReport | ConvertFrom-Json -AsHashtable -ErrorAction Stop
    }
    catch {
        throw 'Native JSON reporting smoke test did not produce one valid JSON document.'
    }
    if ($report.schemaVersion -ne '1.0.0' -or $report.command -ne 'inspect' -or
        $report.candidates.Count -ne 1 -or -not $report.ContainsKey('completion') -or
        -not $report.ContainsKey('summary')) {
        throw 'Native JSON reporting smoke test produced an unexpected CAP-006 contract.'
    }
    Write-Host 'Native AOT JSON reporting contract smoke test passed.'
}
finally {
    Pop-Location
}
