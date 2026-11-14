<#
.SYNOPSIS
Packs the native .NET tool and verifies isolated installation and invocation.

.DESCRIPTION
Requires .NET 11, native AOT toolchain and a matching x64 host. Creates both
the RID-specific Native AOT package and the top-level pointer package without
publishing either to NuGet. Test installation uses a fresh, private tool path.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('linux-x64', 'win-x64', 'osx-x64')]
    [string]$RuntimeIdentifier
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$hostRid = if ($IsWindows) { 'win-x64' } elseif ($IsLinux) { 'linux-x64' } elseif ($IsMacOS) { 'osx-x64' } else { throw 'Unsupported operating system for tool packaging.' }
$hostArchitecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture
if ($RuntimeIdentifier -ne $hostRid -or $hostArchitecture -ne [System.Runtime.InteropServices.Architecture]::X64) {
    throw "Tool packaging requires a matching x64 host: requested $RuntimeIdentifier; host $hostRid ($hostArchitecture)."
}

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Push-Location $root
try {
    $project = 'src/Infoconex.DotNetBinary.Cli/Infoconex.DotNetBinary.Cli.csproj'
    $packageDirectory = Join-Path $root 'artifacts/tool-packages'
    $installationPath = Join-Path $root "artifacts/tool-validation/$RuntimeIdentifier/$([guid]::NewGuid())"
    New-Item -ItemType Directory -Path $packageDirectory, $installationPath -Force | Out-Null

    Write-Host "Source revision: $(& git rev-parse HEAD)"
    Write-Host "PowerShell: $($PSVersionTable.PSVersion); OS: $([System.Runtime.InteropServices.RuntimeInformation]::OSDescription); RID: $RuntimeIdentifier"
    & dotnet --info
    if ($LASTEXITCODE -ne 0) { throw "dotnet --info failed with exit code $LASTEXITCODE." }

    $versionOutput = & dotnet msbuild $project -getProperty:Version
    if ($LASTEXITCODE -ne 0) { throw "Unable to read package version with exit code $LASTEXITCODE." }
    $version = ([string]($versionOutput | Select-Object -Last 1)).Trim()
    if ([string]::IsNullOrWhiteSpace($version)) { throw 'Package version was empty.' }

    & dotnet restore $project -r $RuntimeIdentifier --configfile nuget.config --force-evaluate
    if ($LASTEXITCODE -ne 0) { throw "Native tool restore failed with exit code $LASTEXITCODE." }
    & dotnet pack $project --configuration Release -r $RuntimeIdentifier --no-restore --output $packageDirectory
    if ($LASTEXITCODE -ne 0) { throw "RID-specific AOT tool pack failed with exit code $LASTEXITCODE." }

    & dotnet restore $project --configfile nuget.config --force-evaluate
    if ($LASTEXITCODE -ne 0) { throw "Pointer-package restore failed with exit code $LASTEXITCODE." }
    & dotnet pack $project --configuration Release --no-restore --output $packageDirectory
    if ($LASTEXITCODE -ne 0) { throw "Top-level tool pack failed with exit code $LASTEXITCODE." }

    $topLevel = Join-Path $packageDirectory "Infoconex.DotNetBinary.Tools.$version.nupkg"
    $ridPackage = Join-Path $packageDirectory "Infoconex.DotNetBinary.Tools.$RuntimeIdentifier.$version.nupkg"
    foreach ($package in @($topLevel, $ridPackage)) {
        if (-not (Test-Path -LiteralPath $package -PathType Leaf)) { throw "Expected NuGet tool package missing: $package" }
    }

    Write-Host "Installing $version into fresh tool directory $installationPath"
    & dotnet tool install Infoconex.DotNetBinary.Tools --version $version --tool-path $installationPath --add-source $packageDirectory --ignore-failed-sources
    if ($LASTEXITCODE -ne 0) { throw "Clean local NuGet tool installation failed with exit code $LASTEXITCODE." }

    Write-Host 'Top-level installed tool paths:'
    Get-ChildItem -LiteralPath $installationPath -Force | ForEach-Object { Write-Host "  $($_.Name)" }
    $shimNames = if ($IsWindows) { @('dotnet-binary.exe', 'dotnet-binary.cmd', 'dotnet-binary') } else { @('dotnet-binary') }
    $shims = @($shimNames | ForEach-Object { Join-Path $installationPath $_ } | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf })
    if ($shims.Count -eq 0) { throw "No installed tool entry point found under $installationPath; checked $($shimNames -join ', ')." }
    $shim = $shims[0]
    & (Resolve-Path -LiteralPath $shim).Path --help
    if ($LASTEXITCODE -ne 0) { throw "Installed tool help failed with exit code $LASTEXITCODE." }
    & (Resolve-Path -LiteralPath $shim).Path --version
    if ($LASTEXITCODE -ne 0) { throw "Installed tool version failed with exit code $LASTEXITCODE." }

    $originalPath = $env:PATH
    try {
        $env:PATH = "$installationPath$([System.IO.Path]::PathSeparator)$originalPath"
        & dotnet binary --help
        if ($LASTEXITCODE -ne 0) { throw "dotnet binary subcommand failed with exit code $LASTEXITCODE." }
    }
    finally { $env:PATH = $originalPath }

    Write-Host "Verified pointer and $RuntimeIdentifier packages, clean tool installation, and both entry points."
}
finally { Pop-Location }
