<#
.SYNOPSIS
Assembles and audits the complete unpublished six-RID distribution set.

.DESCRIPTION
Consumes six distinct CI artifacts from matching OS builders. Copies
exactly one top-level pointer, six native NuGet subpackages and six native
standalone archives. Does not install, sign, tag, release or publish anything.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[0-9a-fA-F]{40}$')]
    [string]$SourceSha
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$project = Join-Path $root 'src/Infoconex.DotNetBinary.Cli/Infoconex.DotNetBinary.Cli.csproj'
[xml]$xml = Get-Content -LiteralPath $project -Raw
$version = [string]$xml.Project.PropertyGroup.Version
if ($version -notmatch '^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?$') {
    throw 'Version must be explicitly declared in the CLI project before assembling release-candidate artifacts.'
}

$downloaded = Join-Path $root 'artifacts/collected'
$destination = Join-Path $root 'artifacts/distribution'
$packages = Join-Path $destination 'packages'
$standalone = Join-Path $destination 'standalone'
New-Item -ItemType Directory -Path $packages, $standalone -Force | Out-Null

function Copy-UniqueArtifact([string]$Rid, [string]$Name, [string]$Target) {
    $artifactRoot = Join-Path $downloaded "pre-release-$Rid"
    if (-not (Test-Path -LiteralPath $artifactRoot -PathType Container)) {
        throw "Missing platform artifact for $Rid"
    }
    $matches = @(Get-ChildItem -LiteralPath $artifactRoot -File -Recurse |
        Where-Object { $_.Name -ceq $Name })
    if ($matches.Count -ne 1) { throw "Expected one $Name in $Rid artifact, found $($matches.Count)." }
    $output = Join-Path $Target $Name
    if (Test-Path -LiteralPath $output) { throw "Refusing to replace assembled artifact $Name" }
    Copy-Item -LiteralPath $matches[0].FullName -Destination $output
}

foreach ($rid in @('win-x64', 'win-arm64', 'linux-x64', 'linux-arm64', 'osx-x64', 'osx-arm64')) {
    Copy-UniqueArtifact $rid "Infoconex.DotNetBinary.Tools.$rid.$version.nupkg" $packages
    $extension = if ($rid.StartsWith('win-')) { 'zip' } else { 'tar.gz' }
    Copy-UniqueArtifact $rid "dotnet-binary-$version-$rid.$extension" $standalone
}
# Top-level pointer is independent of the host OS, so select one canonical
# instance after all six native subpackages have been staged.
Copy-UniqueArtifact 'linux-x64' "Infoconex.DotNetBinary.Tools.$version.nupkg" $packages

$manifest = Join-Path $destination 'release-inventory.json'
& python3 (Join-Path $root 'scripts/distribution_inventory.py') --packages $packages --standalone $standalone --version $version --source-sha $SourceSha --output $manifest
if ($LASTEXITCODE -ne 0) { throw "Distribution inventory failed with exit code $LASTEXITCODE." }
& (Join-Path $root 'scripts/write-artifact-checksums.ps1') -ArtifactDirectory 'artifacts/distribution/packages'
& (Join-Path $root 'scripts/write-artifact-checksums.ps1') -ArtifactDirectory 'artifacts/distribution/standalone'
Write-Host "Staged complete unpublished CAP-008 candidate $version for source $SourceSha"
