<#
.SYNOPSIS
Evaluates NuGet lock-file reproducibility without changing repository files.

.DESCRIPTION
Copies tracked source into an ignored experiment directory, creates lock files,
verifies locked restore, proves a changed central package version is rejected,
then exercises RID-specific locked restore and AOT publish on Linux x64.
This is an engineering evaluation, not a locked-mode enforcement policy.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $IsLinux -or [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -ne [System.Runtime.InteropServices.Architecture]::X64) {
    throw 'The initial lock-file evaluation must run on Linux x64 with Native AOT prerequisites.'
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
    Write-Host "Evaluating NuGet lock files in isolated copy: $experiment"
    Write-Host "Source revision: $(& git -C $root rev-parse HEAD)"
    & dotnet --info
    if ($LASTEXITCODE -ne 0) { throw "dotnet --info failed with exit code $LASTEXITCODE." }

    & dotnet restore DotNetBinary.slnx --configfile nuget.config -p:RestorePackagesWithLockFile=true --force-evaluate
    if ($LASTEXITCODE -ne 0) { throw "Lock-file generation failed with exit code $LASTEXITCODE." }
    $locks = @(Get-ChildItem -Recurse -File -Filter 'packages.lock.json')
    if ($locks.Count -eq 0) { throw 'NuGet did not generate any packages.lock.json files.' }
    Write-Host "Generated $($locks.Count) lock file(s)."

    & dotnet restore DotNetBinary.slnx --configfile nuget.config --locked-mode
    if ($LASTEXITCODE -ne 0) { throw "Locked solution restore failed with exit code $LASTEXITCODE." }

    $central = 'Directory.Packages.props'
    $original = Get-Content -LiteralPath $central -Raw
    $changed = $original.Replace('Version="4.6.1"', 'Version="4.6.0"')
    if ($changed -eq $original) { throw 'Could not construct the controlled central-version negative test.' }
    try {
        Set-Content -LiteralPath $central -Value $changed -NoNewline -Encoding utf8NoBOM
        & dotnet restore DotNetBinary.slnx --configfile nuget.config --locked-mode
        if ($LASTEXITCODE -eq 0) { throw 'Negative control failed: locked restore accepted a changed direct package version.' }
        Write-Host 'Negative control passed: changed central NuGet version was rejected.'
    }
    finally { Set-Content -LiteralPath $central -Value $original -NoNewline -Encoding utf8NoBOM }

    $project = 'src/Infoconex.DotNetBinary.Cli/Infoconex.DotNetBinary.Cli.csproj'
    $ridResult = & dotnet restore $project -r linux-x64 --configfile nuget.config --locked-mode 2>&1
    $ridExit = $LASTEXITCODE
    $ridResult | ForEach-Object { Write-Host $_ }
    if ($ridExit -ne 0) {
        if (($ridResult | Out-String) -notmatch 'NU1004') {
            throw "RID-specific locked restore failed for an unexpected reason (exit $ridExit)."
        }
        Write-Host 'Observed NU1004: solution-generated lock files do not describe the explicitly requested linux-x64 restore graph.'
        Write-Host 'Re-evaluating locks for the matching RID in the isolated workspace.'
        & dotnet restore $project -r linux-x64 --configfile nuget.config --force-evaluate
        if ($LASTEXITCODE -ne 0) { throw "RID-specific lock regeneration failed with exit code $LASTEXITCODE." }
        & dotnet restore $project -r linux-x64 --configfile nuget.config --locked-mode
        if ($LASTEXITCODE -ne 0) { throw "RID-specific locked restore still failed after regeneration: $LASTEXITCODE." }
    }

    & dotnet publish $project -c Release -r linux-x64 --no-restore -p:PublishAot=true -o artifacts/lock-evaluation/native
    if ($LASTEXITCODE -ne 0) { throw "AOT publish after locked restore failed with exit code $LASTEXITCODE." }
    $binary = 'artifacts/lock-evaluation/native/dotnet-binary'
    if (-not (Test-Path -LiteralPath $binary -PathType Leaf)) { throw 'AOT publish did not produce the native binary.' }
    & (Resolve-Path -LiteralPath $binary).Path --version
    if ($LASTEXITCODE -ne 0) { throw "Locked-restore AOT binary failed with exit code $LASTEXITCODE." }
    Write-Host 'Lock-file evaluation complete: locked solution restore, package-drift negative control, RID-specific restore/regeneration, and AOT publish.'
}
finally { Pop-Location }
