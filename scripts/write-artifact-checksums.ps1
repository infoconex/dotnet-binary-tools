<#
.SYNOPSIS
Writes a SHA-256 checksum manifest for a directory of generated artifacts.

.DESCRIPTION
Does not publish or sign artifacts. Intended for verification of immutable
outputs; provenance attestation is a separate release-time control.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ArtifactDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$directory = [System.IO.Path]::GetFullPath((Join-Path $root $ArtifactDirectory))
if (-not (Test-Path -LiteralPath $directory -PathType Container)) {
    throw "Artifact directory missing: $directory"
}

$manifest = Join-Path $directory 'SHA256SUMS.txt'
$files = @(Get-ChildItem -LiteralPath $directory -Recurse -File |
    Where-Object { $_.FullName -ne $manifest } | Sort-Object FullName)
if ($files.Count -eq 0) { throw "No artifacts found under $directory" }

$lines = foreach ($file in $files) {
    $relative = [System.IO.Path]::GetRelativePath($directory, $file.FullName).Replace('\', '/')
    $hash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    "$hash  $relative"
}
$lines | Set-Content -LiteralPath $manifest -Encoding utf8NoBOM
Write-Host "Wrote SHA-256 checksums for $($files.Count) artifact(s): $manifest"
