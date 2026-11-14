<#
.SYNOPSIS
Collects cross-platform NUnit coverage using Microsoft's built-in VSTest collector.

.DESCRIPTION
Requires an already restored and built Release solution. Collects Cobertura XML
without depending on GitHub Actions. Does not provision the .NET SDK or test tools.
#>
[CmdletBinding()]
param(
    [ValidateNotNullOrEmpty()]
    [string]$OutputDirectory = 'artifacts/coverage'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path

Push-Location $root
try {
    $output = [System.IO.Path]::GetFullPath((Join-Path $root $OutputDirectory))
    New-Item -ItemType Directory -Path $output -Force | Out-Null

    Write-Host "Collecting NUnit coverage in $output"
    & dotnet test DotNetBinary.slnx --configuration Release --no-build --no-restore `
        --collect 'Code Coverage;Format=cobertura' --settings coverage.runsettings `
        --results-directory (Join-Path $output 'raw') --logger trx

    if ($LASTEXITCODE -ne 0) {
        throw "Coverage test run failed with exit code $LASTEXITCODE."
    }

    $reports = @(Get-ChildItem -Path $output -Recurse -File -Filter 'coverage.cobertura.xml')
    if ($reports.Count -eq 0) {
        throw "The coverage collector did not produce a Cobertura report under $output."
    }

    Write-Host "Collected $($reports.Count) Cobertura report(s):"
    foreach ($report in $reports) {
        Write-Host "  $($report.FullName)"
    }
}
finally {
    Pop-Location
}
