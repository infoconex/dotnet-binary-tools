# Development and issue workflow

## Ordinary changes

1. Start from an approved CAP ID, then define one independently testable FR/NFR or engineering issue and its acceptance scenarios.
2. Branch from current `main` as `issue/<number>-<slug>`. Initial engineering-baseline work uses `setup/engineering-foundation-dotnet11`.
3. Write a failing test for missing/defective functionality, then implement, refactor, and run all affected tests.
4. Update test traceability with **observed** execution evidence and exact SHA.
5. Open a PR to `main`, document risks and limitations, and confirm required CI runs.
6. Owner performs second-pass review, squash merges only after verification, and checks resulting `main` CI.

Create a release branch only when multiple ongoing feature branches or stabilization requirements justify it; this project does not need one at its initial engineering milestone.

## Review ownership

A solo maintainer can configure zero mandatory reviewer approvals while still requiring a PR, passing checks and a documented second-pass owner review. Do not label self-review as independent review.

Promotion to `main` is not authorization to tag or publish. Separate explicit GO is needed for public GitHub Releases, NuGet pushes, and standalone downloads. See [release gates](release-gates.md).
