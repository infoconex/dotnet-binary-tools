## Primary issue and intended change

Issue: #<!-- number or explain setup exception -->
Requirements: `FR-###` / `NFR-###` / engineering issue
Scenarios: `FR-###-SC##` / `NFR-###-SC##`

Describe the smallest behavior or engineering improvement delivered.

## Verification evidence

- [ ] Tests were written first for missing/defective behavior, or documented verification-only/setup exception.
- [ ] Relevant positive, negative, boundary and security cases were considered.
- [ ] Actual SDK, operating systems, commands and test counts are recorded below.
- [ ] GitHub Actions run URLs and exact commit SHA are linked; untested work is marked unverified.
- [ ] Scenario traceability was updated or an explicit nonfunctional/setup exception is documented.

**Observed execution and limitations:**

<!-- Link actual runs, logs, counts and exact SHA; never invent execution -->

## Architecture, security and release review

- [ ] Core remains independent of CLI; dependency changes are intentional.
- [ ] New dependencies were reviewed for license, version and vulnerabilities.
- [ ] Public API and JSON schema compatibility were reviewed.
- [ ] Binary inputs are read-only, untrusted and not executed; diagnostic data is safe.
- [ ] Microsoft C# naming and XML documentation checks pass.
- [ ] Native AOT and OS compatibility are documented as verified or not yet verified.
- [ ] Requirements, scenarios, README and ADRs reflect changes.
- [ ] No unrelated framework, SDK or dependency changes.
- [ ] This PR does **not** authorize package publication.

## Owner review

- [ ] Owner completed a documented second-pass review of the exact candidate and accepted remaining risks.
- [ ] Required CI checks passed on the candidate SHA before squash merge.
- [ ] For any release: separate explicit owner GO for promotion, and another for publication.
