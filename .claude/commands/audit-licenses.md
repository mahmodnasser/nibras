---
description: Full dependency and asset license audit
allowed-tools: Read, Grep, Glob, Bash, WebFetch, WebSearch
disable-model-invocation: true
---

Produce the complete dependency and asset inventory for backend, web, mobile, and infrastructure.

For every item record: package, **exact pinned version**, license **of that version** verified at the source (repository LICENSE file or package metadata, never memory), how you verified it, and whether it is linked into the product or run as a standalone tool.

Then flag:

- Anything that violates Section 6.1 of the master brief.
- Anything whose license changed in a version newer than the pinned one, so a routine upgrade would break the policy.
- Any asset (font, icon set, illustration, animation, sound) with no verified license.
- Anything you could not verify. Write "unverified" and say what blocked you. Never guess.

For each violation propose a compliant replacement, the effort to switch, and what breaks.

Finish by updating `THIRD-PARTY-LICENSES.md` and by confirming the continuous-integration license scan fails on a deliberate test violation. Run the scan as `tools/license-scan/run.ps1` on Windows PowerShell or `tools/license-scan/run.sh` on bash.

## Reads first

- `docs/brief/01-master-brief.md` Sections 6.1, 6.2, and 6.3 only.
- `docs/plan/19-dependency-and-license-inventory.md` when it exists.
- The manifests: `Directory.Packages.props`, the `.csproj` files, `package.json` with its lock file, `pubspec.yaml` with its lock file, and the image tags under `deploy/`.

## Output contract

- `## Inventory` — table: package, version, license, verification source, linked or standalone, verdict
- `## Violations` — table: package, rule broken, replacement, effort, what breaks
- `## Unverified` — table: package, what blocked verification, next step
- `## Upgrade traps` — packages whose newer versions change license
- `## Evidence` — the commands you ran and what they printed

## Stop conditions

- Stop and ask before removing or replacing any dependency. That is a change request, not an audit.
- Stop when a required capability has no compliant implementation. Present options with trade-offs instead of choosing one.
- Stop when a lock file is missing. An unpinned tree cannot be audited.