# Release promotion and publication gates

**Status:** Proposed governance for dotnet-framework, including CI, sole-maintainer reviews, and publication approval.

## Main branch controls

Configure a GitHub ruleset for `main` to require PRs, prohibit force-pushes/deletions, enable squash-only merges and require the *actual* CI job contexts observed in this repository. A sole maintainer may require zero reviewer approvals but must document a second-pass owner review. The current new repository has **no ruleset**; this cannot be fixed by adding a file.

## Change acceptance

- Requirement, scenarios, test coverage, failure cases and traceability are complete for the issue scope.
- CI checks pass for the exact PR candidate SHA and supported OS matrix; failed or missing checks block acceptance.
- Code style, XML API documentation, dependency inventory, security and mandatory ARCH-001–ARCH-008 inward-only architecture reviewed.
- No unsupported claim of Native AOT, clean consumer install, cross-platform compatibility or actual runtime independence.
- Explicit owner GO for merge after second-pass risk review.

## Independent release decisions

Source approval and merging are **not** NuGet publication. The public release needs a separate owner GO for a frozen exact SHA, package ID/version, license, dependency audit, AOT outputs per advertised RID, runtime-free consumer tests, checksums, release notes, and rollback plan.

Never create a public tag, GitHub Release or upload a package on an ordinary PR build. Missing evidence is a STOP.
