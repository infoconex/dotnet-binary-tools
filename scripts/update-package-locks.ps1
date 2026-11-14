<#
.SYNOPSIS
Explicitly updates NuGet lock files for the common solution restore graph.

.DESCRIPTION
Use this script only when intentionally refreshing dependency locks. It does
not target an individual runtime identifier. Ordinary CI validation must use
locked mode without force evaluation once the lock files are adopted.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path

Push-Location $root
try {
    Write-Host "Source revision: $(& git rev-parse HEAD)"
    Write-Host "PowerShell: $($PSVersionTable.PSVersion); OS: $([System.Runtime.InteropServices.RuntimeInformation]::OSDescription)"
    & dotnet --info
    if ($LASTEXITCODE -ne 0) { throw "dotnet --info failed with exit code $LASTEXITCODE." }

    & dotnet restore DotNetBinary.slnx --configfile nuget.config --use-lock-file --force-evaluate
    if ($LASTEXITCODE -ne 0) { throw "Dependency lock update failed with exit code $LASTEXITCODE." }

    $locks = @(Get-ChildItem -Path src, tests -Recurse -File -Filter 'packages.lock.json')
    if ($locks.Count -eq 0) { throw 'NuGet did not produce any packages.lock.json files.' }
    Write-Host "Generated or updated $($locks.Count) project lock file(s)."
    foreach ($lock in $locks | Sort-Object FullName) {
        $relative = [System.IO.Path]::GetRelativePath($root, $lock.FullName)
        Write-Host "  $relative"
    }
}
finally { Pop-Location }
