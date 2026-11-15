# Repository security automation

Security automation complements code review, dependency audits and untrusted
binary input design; it does not establish that a build is free of vulnerabilities.
GitHub-managed account settings are verified separately from committed files.

## CodeQL choice: advanced C# analysis with a manual build

The repository uses [advanced CodeQL setup](../../.github/workflows/codeql.yml)
with a C# manual-build database on an Ubuntu runner. CodeQL initializes before
the shared managed validation script runs. That script restores, builds and
tests the .NET 11 solution, capturing the compilation CodeQL needs for
build-aware analysis. The workflow analyzes and uploads results on PRs to
`main`, pushes to `main`, a weekly schedule and manual dispatch.

GitHub's [C# default setup](https://docs.github.com/en/code-security/reference/code-scanning/codeql/build-options-for-compiled-languages)
uses `none` build mode. It is simpler and avoids build orchestration, but does
not analyze generated code and provides less control over the exact build.
Advanced setup with a manual build reuses our local validation entry point and
captures compiled source while preserving the existing three-OS managed and
Native AOT gates. CodeQL's single Linux run does not replace those gates.

Only one CodeQL setup should be active. If GitHub default setup is already
enabled, an administrator must switch to advanced setup under **Settings →
Advanced Security → CodeQL analysis** before expecting uploads from this
workflow. [GitHub documents the switch](https://docs.github.com/en/code-security/how-tos/find-and-fix-code-vulnerabilities/configure-code-scanning/configuring-advanced-setup-for-code-scanning).
The build-aware analysis is verified only when the CodeQL job completes and
its analysis appears under **Security → Code scanning** on the actual commit.

## GitHub-managed controls to verify

These settings are not established by adding configuration files. A repository
administrator should inspect **Settings → Advanced Security** (GitHub may label
the section **Code security**) and confirm:

1. **Dependabot alerts** are enabled; review **Security → Dependabot alerts**.
2. **Dependabot security updates** are enabled; review generated advisory PRs
   when alerts affect a dependency. Grouping rules are in
   [dependabot.yml](../../.github/dependabot.yml); they do not by themselves
   enable alerts or updates.
3. **Secret scanning** is enabled with alert notifications; inspect the
   **Security → Secret scanning** alert view. Review access to alerts.
4. **Push protection** is enabled for the repository. Review the allowed bypass
   behavior, notification recipients and any bypass events. A repository
   setting or policy view—not an untested dummy credential—is the evidence.
5. **Private vulnerability reporting** is available via the Security tab if
   the owner wants to accept private security reports. `SECURITY.md` must not
   be treated as confirmation that this feature is active.

For each item, retain the actual GitHub setting or alert-page observation and
date in [Issue #2](https://github.com/infoconex/dotnet-binary-tools/issues/2).
Do not mark these controls enabled merely because the repository is public or
because Dependency Review or Dependabot configuration is committed.

## Dependency updates

The [Dependabot policy](../../.github/dependabot.yml) groups compatible weekly
minor and patch updates for NuGet and GitHub Actions, preserves individual
major-version updates, and groups compatible security updates without automatic
merge. Security remediation should not wait for the weekly version schedule.
The [Dependency Review workflow](../../.github/workflows/dependency-review.yml)
blocks dependency changes introducing high or critical known vulnerabilities;
it does not replace alert monitoring for dependencies already on `main`.
