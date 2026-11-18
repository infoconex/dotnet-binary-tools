<#
.SYNOPSIS
Restores, publishes, and smoke-tests the native CLI for its x64/ARM64 host RID.

.DESCRIPTION
Runs without provisioning the SDK or native compiler prerequisites. The host
must match the requested RID; cross-compilation is not considered validation.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('linux-x64', 'linux-arm64', 'win-x64', 'win-arm64', 'osx-x64', 'osx-arm64')]
    [string]$RuntimeIdentifier
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$hostArchitecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture
$architecture = switch ($hostArchitecture.ToString()) { 'X64' { 'x64' } 'Arm64' { 'arm64' } default { throw "Unsupported native architecture: $hostArchitecture" } }
$hostRid = if ($IsWindows) { "win-$architecture" } elseif ($IsLinux) { "linux-$architecture" } elseif ($IsMacOS) { "osx-$architecture" } else { throw 'Unsupported operating system for Native AOT.' }
if ($RuntimeIdentifier -ne $hostRid) {
    throw "Native AOT validation requires a matching host: requested $RuntimeIdentifier; host $hostRid ($hostArchitecture)."
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
    # CAP-007: check real option binding, a scan operation, invalid syntax and exit-code discipline.
    $hashJson = & $binaryPath inspect $binaryPath --format json --sha256 2>$null | Out-String
    if ($LASTEXITCODE -ne 0) { throw 'Native opt-in SHA-256 inspection failed.' }
    $hashReport = $hashJson | ConvertFrom-Json -AsHashtable -ErrorAction Stop
    if ($hashReport.candidates.Count -ne 1 -or $hashReport.candidates[0].sha256 -notmatch '^[0-9a-f]{64}$') {
        throw 'Native opt-in SHA-256 did not propagate to the inspection service.'
    }

    $scanJson = & $binaryPath scan (Split-Path -Parent $binaryPath) --format json --max-candidates 1 2>$null | Out-String
    if ($LASTEXITCODE -ne 0) { throw 'Native directory scan failed.' }
    $scanReport = $scanJson | ConvertFrom-Json -AsHashtable -ErrorAction Stop
    if ($scanReport.schemaVersion -ne '1.0.0' -or $scanReport.command -ne 'scan' -or $scanReport.inputs.Count -ne 1) {
        throw 'Native scan did not produce the expected command envelope.'
    }

    # Isolate the intentional nonzero exit so it cannot become this script's own
    # $LASTEXITCODE on successful completion (pwsh would then fail the CI step).
    $invalidStart = [System.Diagnostics.ProcessStartInfo]::new($binaryPath)
    $invalidStart.ArgumentList.Add('inspect')
    $invalidStart.RedirectStandardOutput = $true
    $invalidStart.RedirectStandardError = $true
    $invalidProcess = [System.Diagnostics.Process]::Start($invalidStart)
    try {
        $invalidStdout = $invalidProcess.StandardOutput.ReadToEnd()
        $invalidStderr = $invalidProcess.StandardError.ReadToEnd()
        if (-not $invalidProcess.WaitForExit(30000)) {
            $invalidProcess.Kill()
            throw 'Native invalid-arity smoke test exceeded 30 seconds.'
        }
        if ($invalidProcess.ExitCode -ne 2 -or $invalidStdout.Length -ne 0 -or
            $invalidStderr -notmatch 'exactly one') {
            throw 'Native CLI did not reject invalid arity with the expected exit code and stderr-only diagnostic.'
        }
    }
    finally {
        $invalidProcess.Dispose()
    }
    Write-Host 'Native AOT reporting, scan, SHA-256 option, and invalid-arity smoke tests passed.'
}
finally {
    Pop-Location
}
