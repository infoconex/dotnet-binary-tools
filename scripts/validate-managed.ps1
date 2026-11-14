<#
.SYNOPSIS
Restores, builds, and tests the managed solution without provisioning tools.

.DESCRIPTION
Runs the same managed Release validation locally and in CI. The optional style
gate is enabled on Linux CI; NUnit coverage is collected separately.
#>
[CmdletBinding()]
param(
    [switch]$VerifyStyle
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path

Push-Location $root
try {
    Write-Host "Source revision: $(& git rev-parse HEAD)"
    Write-Host "PowerShell: $($PSVersionTable.PSVersion); OS: $([System.Runtime.InteropServices.RuntimeInformation]::OSDescription)"
    & dotnet --info
    if ($LASTEXITCODE -ne 0) { throw "dotnet --info failed with exit code $LASTEXITCODE." }

    Write-Host 'Restoring solution with NuGet audit and dependency reevaluation'
    & dotnet restore DotNetBinary.slnx --configfile nuget.config --force-evaluate
    if ($LASTEXITCODE -ne 0) { throw "Managed restore failed with exit code $LASTEXITCODE." }

    Write-Host 'Building Release configuration'
    & dotnet build DotNetBinary.slnx --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Managed build failed with exit code $LASTEXITCODE." }

    Write-Host 'Running NUnit suites'
    & dotnet test DotNetBinary.slnx --configuration Release --no-build --no-restore --logger trx
    if ($LASTEXITCODE -ne 0) { throw "Managed NUnit tests failed with exit code $LASTEXITCODE." }

    if ($VerifyStyle) {
        Write-Host 'Verifying enforced Microsoft naming style'
        & dotnet format style DotNetBinary.slnx --no-restore --verify-no-changes --diagnostics IDE1006 --severity error --verbosity normal
        if ($LASTEXITCODE -ne 0) { throw "Managed style verification failed with exit code $LASTEXITCODE." }
    }
}
finally {
    Pop-Location
}
