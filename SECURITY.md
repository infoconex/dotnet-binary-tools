# Security policy

## Supported versions

.NET Binary Tools is under development. **No version has been released**, and no
NuGet package or standalone binary has been published by this project.

| Version or branch | Security maintenance |
| --- | --- |
| `main` (unreleased development) | Vulnerability reports are accepted and evaluated; fixes are made through reviewed pull requests. |
| Published versions | None exist; no released-version support commitment is in effect. |

The versions of .NET assemblies that the tool will inspect are *inputs*, not
versions of this product covered by a support policy.

## Report a vulnerability privately

Use the repository's [private vulnerability-reporting
form](https://github.com/infoconex/dotnet-binary-tools/security/advisories/new)
**if it is enabled**. Enabling and verifying that GitHub repository feature is
tracked in [Issue #2](https://github.com/infoconex/dotnet-binary-tools/issues/2).
If the private reporting form is unavailable, [contact the
maintainer](https://github.com/infoconex) privately to arrange a reporting
channel before sharing any sensitive details.

Do not disclose a suspected vulnerability, proof of concept, secrets, or
affected private data in a public issue, discussion, or pull request. Include
the affected commit or version, prerequisites, impact, and minimal reproduction
steps through the private channel. Use synthetic test files whenever possible.

The maintainer will evaluate reports and coordinate remediation and disclosure
with the reporter. Response and fix timelines depend on severity and confirmed
reachability; no response-time guarantee is established.

For ordinary defects without a plausible security impact, use a normal GitHub
issue instead.

## System and scope

The intended product is a cross-platform command-line tool for **static**
inspection of compiled .NET binaries, deployment configuration, and associated
metadata, with optional inventory and reporting functions. Its planned
boundaries are described in [the approved
capabilities](docs/capabilities/README.md) and [architecture](docs/requirements/architecture.md).

The tool is not yet a functional binary inspector. The properties below are
security requirements for implementation and review, **not claims that all
controls have been implemented or tested**.

## Threat model and security invariants

- Treat candidate binaries, their metadata, adjacent configuration, directory
  entries, links, and future remote reference data as **untrusted input**.
- Never load, invoke, or execute an inspected candidate assembly to determine
  its properties. Inspection must not trigger its initialization or other code.
- Keep inspection read-only; constrain explicitly requested report output to
  authorized destinations and avoid overwriting unrelated files.
- Bound input parsing, recursive traversal, resource consumption, and output
  size so malformed or adversarial inputs cannot cause unbounded work.
- Preserve correct behavior when filesystem entries change during inspection;
  prevent link traversal, path confusion, or file races from escaping the
  user's authorized scope.
- Do not expose credentials, private file contents, or other sensitive data in
  diagnostics, logs, reports, or error messages.
- Keep runtime/framework statements evidence-based: declared assembly target,
  requested deployment runtime, and actually observed runtime must not be
  conflated. Do not fetch or execute code merely to infer a runtime.
- Keep external adapters outside Domain, with inward-only dependencies as
  required by ARCH-001–ARCH-008. Treat external feeds and package dependencies
  as supply-chain trust boundaries.

## Reportable findings and severity context

Security-relevant examples include unexpected execution of a candidate binary,
arbitrary file access or overwrite, leakage of sensitive data, attacker-driven
resource exhaustion, unsafe link/path handling, or a reachable vulnerable
dependency. Assess severity from the actual affected implementation, realistic
attacker control, reachability, and impact. A hypothetical weakness in a feature
that is not yet implemented must not be presented as a verified exploit.

No broad classes of security findings are excluded or accepted as risks by this
document. Any future exception requires a separately reviewed owner decision
with supporting evidence.
