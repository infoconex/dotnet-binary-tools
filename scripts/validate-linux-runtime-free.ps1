<#
.SYNOPSIS
Smoke-tests the Linux Native AOT executable in a .NET-free container.

.DESCRIPTION
Requires the native linux-x64 executable and a pre-pulled ubuntu:24.04 Docker
image. The container has no network or mounted .NET SDK; Docker provisioning
is performed separately by the caller (including GitHub Actions).
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $IsLinux -or [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -ne [System.Runtime.InteropServices.Architecture]::X64) {
    throw 'Runtime-free Linux validation requires a Linux x64 Docker host.'
}
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$binary = Join-Path $root 'artifacts/aot/linux-x64/dotnet-binary'
if (-not (Test-Path -LiteralPath $binary -PathType Leaf)) { throw "Native executable missing: $binary" }

Write-Host 'Testing Linux AOT in Ubuntu 24.04 without .NET SDK/runtime or network access.'
& docker run --rm --pull never --network none --read-only `
    --volume "${binary}:/app/dotnet-binary:ro" ubuntu:24.04 `
    /bin/sh -ec 'test ! -e /usr/share/dotnet/dotnet && test ! -e /usr/bin/dotnet && /app/dotnet-binary --help && /app/dotnet-binary --version'
if ($LASTEXITCODE -ne 0) { throw "Runtime-free Linux AOT validation failed with exit code $LASTEXITCODE." }
