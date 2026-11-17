<#
.SYNOPSIS
Packages one already-validated host Native AOT executable for staged review.

.DESCRIPTION
Does not publish, sign, download or upload artifacts. Use the matching x64
host and validate-aot.ps1 first. Unix tar archives preserve executable mode.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('linux-x64', 'win-x64', 'osx-x64')]
    [string]$RuntimeIdentifier
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$hostRid = if ($IsWindows) { 'win-x64' } elseif ($IsLinux) { 'linux-x64' } elseif ($IsMacOS) { 'osx-x64' } else { throw 'Unsupported packaging host.' }
if ($RuntimeIdentifier -ne $hostRid -or [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -ne [System.Runtime.InteropServices.Architecture]::X64) {
    throw "Standalone packaging requires the matching x64 host: $RuntimeIdentifier."
}

$project = Join-Path $root 'src/Infoconex.DotNetBinary.Cli/Infoconex.DotNetBinary.Cli.csproj'
$versionOutput = & dotnet msbuild $project -getProperty:Version
if ($LASTEXITCODE -ne 0) { throw 'Unable to read project version.' }
$version = ([string]($versionOutput | Select-Object -Last 1)).Trim()
if ($version -notmatch '^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?$') { throw 'Invalid package version.' }

$executable = if ($IsWindows) { 'dotnet-binary.exe' } else { 'dotnet-binary' }
$binary = Join-Path $root "artifacts/aot/$RuntimeIdentifier/$executable"
if (-not (Test-Path -LiteralPath $binary -PathType Leaf)) { throw "Native executable missing: $binary" }
$distribution = Join-Path $root 'artifacts/distribution/standalone'
$stage = Join-Path $root "artifacts/distribution/staging/$RuntimeIdentifier/$([guid]::NewGuid())"
New-Item -ItemType Directory -Path $distribution, $stage -Force | Out-Null
Copy-Item -LiteralPath $binary -Destination (Join-Path $stage $executable)
Copy-Item -LiteralPath (Join-Path $root 'LICENSE') -Destination (Join-Path $stage 'LICENSE')

$basename = "dotnet-binary-$version-$RuntimeIdentifier"
if ($IsWindows) {
    $archive = Join-Path $distribution "$basename.zip"
    if (Test-Path -LiteralPath $archive) { throw "Refusing to overwrite archive: $archive" }
    Compress-Archive -LiteralPath (Join-Path $stage $executable), (Join-Path $stage 'LICENSE') -DestinationPath $archive
}
else {
    $stagedExecutable = Join-Path $stage $executable
    & chmod 755 $stagedExecutable
    if ($LASTEXITCODE -ne 0) { throw 'Unable to set native executable mode.' }
    $archive = Join-Path $distribution "$basename.tar.gz"
    if (Test-Path -LiteralPath $archive) { throw "Refusing to overwrite archive: $archive" }
    & tar -czf $archive -C $stage $executable 'LICENSE'
    if ($LASTEXITCODE -ne 0) { throw 'Unable to produce standalone tar archive.' }
}
Write-Host "Staged unpublished Native AOT archive: $archive"
