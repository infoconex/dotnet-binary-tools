<#
.SYNOPSIS
Verifies committed NuGet lock files and a package-drift negative control.

.DESCRIPTION
Copies tracked source and its committed lock files into an ignored directory,
verifies a locked solution restore, and proves a changed central package version
is rejected. Normal managed and AOT validation independently run locked restore
on all three supported operating systems.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $IsLinux -or [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -ne [System.Runtime.InteropServices.Architecture]::X64) {
    throw 'The package-drift negative control must run on Linux x64.'
}

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$experiment = Join-Path $root "artifacts/lock-evaluation/$([guid]::NewGuid())"
New-Item -ItemType Directory -Path $experiment -Force | Out-Null

Push-Location $root
try {
    $tracked = @(& git ls-files)
    if ($LASTEXITCODE -ne 0 -or $tracked.Count -eq 0) { throw 'Could not inventory tracked files for isolated lock evaluation.' }
    foreach ($relative in $tracked) {
        $target = Join-Path $experiment $relative
        New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $root $relative) -Destination $target
    }
}
finally { Pop-Location }

Push-Location $experiment
try {
    Write-Host "Checking committed NuGet lock files in isolated copy: $experiment"
    Write-Host "Source revision: $(& git -C $root rev-parse HEAD)"
    & dotnet --info
    if ($LASTEXITCODE -ne 0) { throw "dotnet --info failed with exit code $LASTEXITCODE." }

    $locks = @(Get-ChildItem -Path src, tests -Recurse -File -Filter 'packages.lock.json')
    if ($locks.Count -eq 0) { throw 'No committed packages.lock.json files were copied.' }
    Write-Host "Checking $($locks.Count) committed project lock file(s)."

    & dotnet restore DotNetBinary.slnx --configfile nuget.config --locked-mode
    if ($LASTEXITCODE -ne 0) { throw "Locked solution restore failed with exit code $LASTEXITCODE." }

    $central = 'Directory.Packages.props'
    $original = Get-Content -LiteralPath $central -Raw
    $version = [regex]::Match($original, '(?<=<PackageVersion Include="NUnit" Version=")[^"]+')
    if (-not $version.Success) { throw 'Cannot find the central NUnit version for the negative test.' }
    $replacement = if ($version.Value -eq '4.6.1') { '4.6.0' } else { '4.6.1' }
    $changed = $original.Remove($version.Index, $version.Length).Insert($version.Index, $replacement)
    try {
        Set-Content -LiteralPath $central -Value $changed -NoNewline -Encoding utf8NoBOM
        $driftOutput = & dotnet restore DotNetBinary.slnx --configfile nuget.config --locked-mode 2>&1
        $driftExitCode = $LASTEXITCODE
        $driftOutput | ForEach-Object { Write-Host $_ }
        if ($driftExitCode -eq 0) { throw 'Negative control failed: locked restore accepted a changed direct package version.' }
        if (($driftOutput | Out-String) -notmatch 'NU1004') {
            throw "Negative control failed for an unexpected reason (exit $driftExitCode), not NuGet lock mismatch NU1004."
        }
        Write-Host 'Negative control passed: changed central NuGet version was rejected.'
    }
    finally { Set-Content -LiteralPath $central -Value $original -NoNewline -Encoding utf8NoBOM }

    & dotnet restore DotNetBinary.slnx --configfile nuget.config --locked-mode
    if ($LASTEXITCODE -ne 0) { throw "Locked solution restore failed after resetting the negative control: $LASTEXITCODE." }
    Write-Host 'Lock-file verification complete: committed solution locks are reproducible and reject central package drift.'
}
finally { Pop-Location }
