<#
.SYNOPSIS
Packs the native .NET tool and verifies isolated installation and invocation.

.DESCRIPTION
Requires .NET 11, native AOT toolchain and a matching x64/ARM64 host. Creates both
the RID-specific Native AOT package and the top-level pointer package without
publishing either to NuGet. Test installation uses a fresh, private tool path.
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
$hostRid = if ($IsWindows) { "win-$architecture" } elseif ($IsLinux) { "linux-$architecture" } elseif ($IsMacOS) { "osx-$architecture" } else { throw 'Unsupported operating system for tool packaging.' }
if ($RuntimeIdentifier -ne $hostRid) {
    throw "Tool packaging requires a matching host: requested $RuntimeIdentifier; host $hostRid ($hostArchitecture)."
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

    Write-Host 'Restoring committed multi-RID dependency locks before RID-specific packaging'
    & dotnet restore $project --configfile nuget.config --locked-mode
    if ($LASTEXITCODE -ne 0) { throw "Multi-RID tool restore failed with exit code $LASTEXITCODE." }
    & dotnet pack $project --configuration Release -r $RuntimeIdentifier --no-restore --output $packageDirectory
    if ($LASTEXITCODE -ne 0) { throw "RID-specific AOT tool pack failed with exit code $LASTEXITCODE." }

    & dotnet pack $project --configuration Release --no-restore --output $packageDirectory
    if ($LASTEXITCODE -ne 0) { throw "Top-level tool pack failed with exit code $LASTEXITCODE." }

    $topLevel = Join-Path $packageDirectory "Infoconex.DotNetBinary.Tools.$version.nupkg"
    $ridPackage = Join-Path $packageDirectory "Infoconex.DotNetBinary.Tools.$RuntimeIdentifier.$version.nupkg"
    foreach ($package in @($topLevel, $ridPackage)) {
        if (-not (Test-Path -LiteralPath $package -PathType Leaf)) { throw "Expected NuGet tool package missing: $package" }
    }

    # The tool and its RID packages must come from our unpublished local feed.
    # An empty NuGet cache can expose missing Microsoft SDK host packs on ARM64
    # runners. Permit only those Microsoft host packs from nuget.org, never our
    # tool ID, so this still proves the tool is installed from staged packages.
    $nugetConfig = Join-Path $installationPath 'nuget.config'
    $escapedSource = [System.Security.SecurityElement]::Escape($packageDirectory)
    @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="staged" value="$escapedSource" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="staged">
      <package pattern="Infoconex.DotNetBinary.Tools" />
      <package pattern="Infoconex.DotNetBinary.Tools.*" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="Microsoft.NETCore.App.Host.*" />
    </packageSource>
  </packageSourceMapping>
  <auditSources><clear /></auditSources>
</configuration>
"@ | Set-Content -LiteralPath $nugetConfig -Encoding utf8NoBOM
    $previousPackages = $env:NUGET_PACKAGES
    try {
        $env:NUGET_PACKAGES = Join-Path $installationPath 'nuget-cache'
        Write-Host "Installing $version with staged-only tool packages and NuGet.org restricted to Microsoft SDK host packs into $installationPath"
        & dotnet tool install Infoconex.DotNetBinary.Tools --version $version --tool-path $installationPath --configfile $nugetConfig --no-cache
        if ($LASTEXITCODE -ne 0) { throw "Isolated staged NuGet tool installation failed with exit code $LASTEXITCODE." }
    }
    finally { $env:NUGET_PACKAGES = $previousPackages }

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

    # Validate identical feature behavior for the same synthetic fixtures.
    # Compare semantic content; exclude only the per-execution report timestamp.
    $native = Join-Path $root "artifacts/aot/$RuntimeIdentifier/dotnet-binary"
    if ($IsWindows) { $native = "$native.exe" }
    if (-not (Test-Path -LiteralPath $native -PathType Leaf)) {
        throw 'Standalone Native AOT executable is required for distribution parity.'
    }
    $fixtures = Join-Path $installationPath 'fixtures'
    New-Item -ItemType Directory -Path $fixtures -Force | Out-Null
    $sample = Join-Path $fixtures 'synthetic.exe'
    [System.IO.File]::WriteAllBytes($sample, [byte[]](0x4d, 0x5a, 0x00, 0x00))

    foreach ($arguments in @(
        @('inspect', $sample, '--format', 'json', '--sha256'),
        @('scan', $fixtures, '--format', 'json', '--max-candidates', '1')
    )) {
        $directOutput = & $native @arguments 2>$null | Out-String
        if ($LASTEXITCODE -ne 0) { throw 'Standalone distribution parity command failed.' }
        $installedOutput = & (Resolve-Path -LiteralPath $shim).Path @arguments 2>$null | Out-String
        if ($LASTEXITCODE -ne 0) { throw 'Installed tool distribution parity command failed.' }
        $direct = $directOutput | ConvertFrom-Json -AsHashtable -ErrorAction Stop
        $installed = $installedOutput | ConvertFrom-Json -AsHashtable -ErrorAction Stop
        foreach ($report in @($direct, $installed)) { [void]$report.Remove('generatedAtUtc') }
        if (($direct | ConvertTo-Json -Depth 100 -Compress) -cne ($installed | ConvertTo-Json -Depth 100 -Compress)) {
            throw 'Standalone and installed tool disagree on semantic JSON report contract.'
        }
    }

    Write-Host "Verified pointer and $RuntimeIdentifier packages, isolated tool installation, both entry points and inspect/scan parity."
}
finally { Pop-Location }
