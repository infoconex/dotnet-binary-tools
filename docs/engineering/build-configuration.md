# Repository-wide .NET build configuration

Modern .NET has several optional repository-level hooks; use the ones that solve a concrete problem rather than generating every possible file.

## Central files in use

| File | Owner or consumer | Purpose |
| --- | --- | --- |
| `DotNetBinary.slnx` | .NET CLI and IDE | Single XML-format solution containing separate Domain, Application, Infrastructure, CLI and their test projects |
| `global.json` | .NET SDK resolver | Selects the .NET 11 SDK baseline with feature-band roll-forward |
| `.editorconfig` | C# analyzers and editors | Code style, whitespace and enforceable naming rules |
| `.gitattributes` | Git | Normalize tracked text in the index and enforce LF checkout for cross-platform text; reserve CRLF for Windows command scripts |
| `Directory.Build.props` | MSBuild, early import | Shared target framework, nullable analysis, warnings-as-errors, build determinism and NuGet auditing |
| `Directory.Build.targets` | MSBuild, late import | XML documentation on production projects only, after test-project properties are known |
| `Directory.Packages.props` | NuGet Central Package Management | Enables `ManagePackageVersionsCentrally` and pins all direct package versions |
| `nuget.config` | NuGet restore/audit | Explicit approved feeds, clears inherited package sources, disabled-source settings, source mappings and audit sources |

## Git line-ending policy

The root `.editorconfig` defines editor behavior; `.gitattributes` governs Git's index normalization and checkout across Windows, Linux and macOS. Source code, scripts, documentation, and XML/JSON/YAML configuration use LF. Future `.bat` and `.cmd` scripts use CRLF in the working tree. Binary file patterns are explicitly excluded from conversion.

The engineering workflow checks Git's indexed newline classification, effective attributes and checkout representation via `python3 scripts/validate_line_endings.py`. This makes line-ending consistency a versioned CI requirement, independent of each developer's `core.autocrlf` setting.

If existing tracked text needs normalization after changing the policy, run:

```shell
git add --renormalize .
git diff --cached --stat
git diff --cached --check
```

Review staged changes before committing; avoid broad formatting changes unrelated to EOL normalization. A successful CI EOL gate confirms the checked-out tracked files and index already meet the policy.

## Change-aware CI and NuGet dependency caching

All three GitHub Actions workflows still trigger on pull requests targeting `main`, pushes to `main`, and manual dispatch. A shared, tested `scripts/ci_change_scope.py` classifier evaluates the changed paths; checks are gated at **job or step level**, not with workflow-level `paths` filters. This preserves check results for branch rules: documentation-only PRs do not build .NET or run NUnit/AOT jobs, while source and build configuration changes do. A change-detection failure fails CI instead of silently skipping checks.

| Change type | Documentation validation | Managed build/NUnit | Native AOT | Relevant engineering gates |
| --- | --- | --- | --- | --- |
| Documentation (`docs/**`, `README.md`, `AGENTS.md`, GitHub Markdown) | Yes | No | No | Scenario/test metadata where applicable; line endings |
| Production source (`src/**`) | No, unless docs also change | Yes | Yes | Project dependency/architecture; line endings |
| NUnit tests (`tests/**`) | No, unless docs also change | Yes | No | Scenario metadata and relevant project references |
| Global build/NuGet/SDK configuration | No, unless docs also change | Yes | Yes | Architecture and line endings |
| Specific workflow/validator changes | Only if documentation validator changes | As relevant to the changed workflow | As relevant to the changed workflow | Applicable validator |
| Manual dispatch | Yes | Yes | Yes | All |

The two .NET workflows use the existing pinned `actions/setup-dotnet` action's NuGet **global-packages** cache. Keys hash `Directory.Packages.props`, shared build properties/targets, `global.json`, `nuget.config` and all project files. The Native AOT workflow also includes its workflow definition in its key so its RID-specific dependency cache is distinct from the managed cache. Host operating systems are separated by the action's cache-key calculation.

No new lock-file policy is introduced just for caching. Restores still execute and perform the configured NuGet audit; cached packages are a download optimization, **not** proof of dependency consistency. We do not cache `bin`, `obj`, compilation outputs, toolchains or entire build environments. AOT compiler prerequisites are verified on runners and installed only if missing.

For a release candidate or any audit requiring full verification, use manual workflow dispatch to force the complete documentation, managed build/NUnit and Native AOT checks on the exact selected commit; do not treat an intentionally skipped job as evidence of execution.

## NuGet source isolation

The root `nuget.config` clears inherited settings in four sections: `packageSources`, `disabledPackageSources`, `packageSourceMapping`, and `auditSources`. It then adds nuget.org as the sole approved package source and audit endpoint.

This matters when a developer has globally disabled nuget.org or configured unrelated enterprise feeds and source mapping. Without clearing each relevant section, user-specific settings can influence repository restores. In CI, restore explicitly supplies the configuration:

```shell
dotnet restore DotNetBinary.slnx --configfile nuget.config --force-evaluate
```

A developer restoring from the solution root also gets this root NuGet configuration through standard NuGet discovery; explicit `--configfile` makes the source set unambiguous.

## Central Package Management

`ManagePackageVersionsCentrally` is declared in `Directory.Packages.props` alongside the `PackageVersion` items. Project files use versionless `PackageReference` declarations. `Directory.Build.props` does not duplicate the CPM activation setting.

## Optional features not enabled

- `Directory.Solution.props` and `Directory.Solution.targets`: solution-scoped MSBuild customization is unnecessary without solution-specific targets.
- `Directory.Build.rsp`: global MSBuild command-line switches are not needed; shared properties and CI commands are explicit.
- `.globalconfig`: unnecessary while the current analyzer and severity rules fit in `.editorconfig`.
- `.config/dotnet-tools.json`: local tool manifest can be added when the project relies on repository-scoped development tools.
- `packages.lock.json`: evaluate locked transitive dependency graphs before release where needed. Consider runtime-specific restore and Native AOT tool packaging when adopting lock files.

Adding files without a requirement would increase maintenance and the risk of settings overriding one another. The present baseline is centralized without unnecessary duplication.

## Further references

- [Microsoft: Customize the build by directory](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory)
- [Microsoft: NuGet Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/Central-Package-Management)
- [Microsoft: nuget.config reference](https://learn.microsoft.com/en-us/nuget/reference/nuget-config-file)
- [Microsoft: lock files and SDK selection](https://learn.microsoft.com/en-us/dotnet/core/install/upgrade)
