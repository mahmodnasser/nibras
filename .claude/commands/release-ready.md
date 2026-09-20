---
description: Release readiness check with evidence
argument-hint: [version]
allowed-tools: Read, Grep, Glob, Bash
disable-model-invocation: true
---

Prepare release $ARGUMENTS. Confirm every quality gate in Section 24 of the master brief **with evidence**. A gate with no command output is not confirmed.

Produce:

- Release notes for school administrators in English and Arabic, written for a head teacher, not an engineer. Use `docs/templates/release-notes-en-ar.md`.
- Migration notes and the expand, migrate, contract plan, with the rolling-deployment safety argument.
- Rollback plan, tested at least once on a copy.
- Updated runbooks for every new alert.
- The restore test result for a single tenant, with the wall-clock time it took.
- The golden-path demo result per role, with the time each took.
- Known issues, stated plainly.
- The updated traceability matrix showing what this release completes.

State clearly anything that is not ready. A release note that hides a gap is a defect.

## Reads first

- `docs/brief/01-master-brief.md` Section 24 only.
- `docs/plan/16-test-strategy.md` for the gate definitions and `docs/plan/15-deployment-and-operations.md` for the runbook list.
- `docs/brief/02-appendices/appendix-q-uat-scripts.md` and `appendix-o-demo-script.md`: both are release gates, not optional extras.
- `docs/project/TRACEABILITY.md` and `docs/project/CHANGELOG.md`.

## Output contract

- `## Gates` — table: gate, command run, result, evidence, pass or fail
- `## Release notes` — the English and the Arabic text, both complete
- `## Migration and rollback` — the steps, and the rollback rehearsal result
- `## Restore test` — tenant, size, duration, result
- `## Demo result` — table: role, path, duration, outcome
- `## Known issues` — table: issue, impact, workaround, when it is fixed
- `## Not ready` — the explicit list, or the sentence "Nothing is outstanding" with the evidence for it

## Stop conditions

- Stop when a gate cannot be run in this environment. Mark it not confirmed rather than assumed.
- Stop before publishing release notes that omit a known issue.
- Stop when the restore test has never been run against this schema version.