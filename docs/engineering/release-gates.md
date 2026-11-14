# Release promotion and publication gates

This document specifies evidence required before promotion; current workstream
progress is tracked only in [Issue #2](https://github.com/infoconex/dotnet-binary-tools/issues/2).
Approval to merge source is distinct from approval to distribute packages.

## Main branch controls

The `main` ruleset is managed in GitHub settings: require PRs, block force
pushes and branch deletion, and require the seven existing OS-specific CI job
contexts. The sole maintainer can use zero mandatory approving reviews while
still performing a documented second-pass review. Verify the actual effective
settings and contexts immediately before merge; source files do not enable
those repository protections.

## Change acceptance

- Requirement, scenarios, test coverage, failure cases and traceability are complete for the issue scope.
- Applicable CI checks pass for the exact PR candidate SHA. Documentation-only changes intentionally omit expensive managed/NUnit/AOT steps without losing the required matrix contexts; omitted validation is not proof of execution. A release candidate requires explicitly executed full-matrix verification. Failed or missing applicable checks block acceptance.
- Code style, XML API documentation, dependency inventory, security and mandatory ARCH-001–ARCH-008 inward-only architecture reviewed.
- No unsupported claim of Native AOT, clean consumer install, cross-platform compatibility or actual runtime independence.
- Explicit owner GO for merge after second-pass risk review.

## Independent release verification

Keep the following evidence together for a frozen, reviewed candidate SHA:

| Gate | Evidence for owner review |
| --- | --- |
| Identity | Source SHA, exact SDK/PowerShell/compiler versions, host OS, package ID and version, license and [changelog](../../CHANGELOG.md) entry. |
| Specifications | All selected CAP/FR/NFR/ARCH/scenario acceptance criteria mapped to observed tests; no drafts recorded as passing. |
| Validation | Explicit full engineering, managed NUnit/coverage/style, CodeQL/security, dependency review and three-RID AOT runs on the exact SHA. |
| NuGet tool | Top-level pointer and matching-version RID-specific `.nupkg` archives for all three x64 OSes; clean local install and both CLI entry points per OS. |
| Standalone AOT | Three native outputs and SHA-256 manifests; successful `--help`, `--version` and meaningful behavior tests in consumer environments **without .NET SDK or runtime**. |
| Security | NuGet audit/Dependabot/CodeQL and secret protections verified; dependency inventory, advisories and security exceptions reviewed. |
| Supply chain | Final package/binary checksums, a verified dependency SBOM, build provenance attestation tied to immutable digests and SHA, publication permissions and chain of custody. |
| Operational | Version naming, download/install instructions, release notes, rollback and revocation plan, and independent owner decision. |

For a first release, evaluate GitHub's dependency-graph **SPDX SBOM export**
against actual runtime/package dependencies; do not represent an incomplete
repository dependency graph as a complete binary SBOM. Use SHA-256 for each
immutable downloadable artifact, and consider GitHub artifact attestations
with GitHub Actions `id-token: write`/`attestations: write` in a separately
authorized release workflow. These are release-time deliverables, not
authorization to attach public provenance or distribute preview binaries now.
Retain the source hash, packaging workflow run, artifact digests and signing
identity for later verification.

## Promotion criteria

1. All gates above contain observed evidence from the frozen SHA; failures,
   omissions and unsupported platforms are explicit STOP conditions.
2. The owner explicitly approves the merge independently of the release.
3. A separate owner **GO for publication** approves the immutable inputs,
   release identifier, registry destinations, credentials and release notes.
4. Publish all RID-specific native tool packages first, then the matching
   top-level pointer package. Verify installation from the public source only
   after publication authorization. Publish standalone archives and attestations
   with identical recorded digests.
5. If a post-publication fault is discovered, stop promotion, communicate the
   affected version and use the documented rollback/revocation plan. NuGet
   versions are immutable; a corrected package requires a new version.

Never create a tag, GitHub Release, public download or NuGet package publication
from an ordinary PR build. Missing evidence is a STOP.
