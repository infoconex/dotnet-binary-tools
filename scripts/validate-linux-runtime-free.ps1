<#
.SYNOPSIS
Smoke-tests the Linux Native AOT executable in a .NET-free container.

.DESCRIPTION
Requires a native Linux x64/ARM64 executable and a pre-pulled ubuntu:24.04 Docker
image. The container has no network or mounted .NET SDK; Docker provisioning
is performed separately by the caller (including GitHub Actions).
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('linux-x64', 'linux-arm64')]
    [string]$RuntimeIdentifier
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$hostArchitecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture
$architecture = switch ($hostArchitecture.ToString()) { 'X64' { 'x64' } 'Arm64' { 'arm64' } default { throw "Unsupported native architecture: $hostArchitecture" } }
if (-not $IsLinux -or $RuntimeIdentifier -ne "linux-$architecture") {
    throw "Runtime-free Linux validation requires a matching native Linux host: requested $RuntimeIdentifier."
}
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$binary = Join-Path $root "artifacts/aot/$RuntimeIdentifier/dotnet-binary"
if (-not (Test-Path -LiteralPath $binary -PathType Leaf)) { throw "Native executable missing: $binary" }

Write-Host "Testing $RuntimeIdentifier AOT in Ubuntu 24.04 without .NET SDK/runtime or network access."
& docker run --rm --pull never --network none --read-only `
    --volume "${binary}:/app/dotnet-binary:ro" ubuntu:24.04 `
    /bin/sh -ec 'test ! -e /usr/share/dotnet/dotnet && test ! -e /usr/bin/dotnet && /app/dotnet-binary --help && /app/dotnet-binary --version'
if ($LASTEXITCODE -ne 0) { throw "Runtime-free Linux AOT validation failed with exit code $LASTEXITCODE." }
