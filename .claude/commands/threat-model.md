---
description: STRIDE threat model for one service, with abuse cases and tested controls
argument-hint: [service]
allowed-tools: Read, Grep, Glob, Bash
disable-model-invocation: true
---

Build the threat model for the $ARGUMENTS service using `docs/templates/threat-model.md`.

1. **Draw the boundaries.** Trust boundaries, entry points (REST, gRPC, message consumers, jobs, file uploads, webhooks), data stores, and the actors who reach each one.
2. **STRIDE per entry point.** Spoofing, Tampering, Repudiation, Information disclosure, Denial of service, Elevation of privilege. One row per threat, never one row per category.
3. **Abuse cases.** Write them as a person, not a category: a parent in tenant A reaching a child in tenant B; a teacher unlocking a posted mark; a dismissed staff member whose session still works; a guardian without custody requesting a transcript; a bulk export used as exfiltration; a vendor integration key reused across tenants.
4. **Controls.** Every threat gets a named control, and **every control maps to a test case identifier** in the `TC-<AREA>-<NNN>` format. A control with no test is an intention.
5. **Residual risk.** What remains after the controls, who accepted it, and when it is reviewed.

This is a child-data platform. A threat to a student's location, custody arrangement, health, or wellbeing record is critical by default, whatever its likelihood.

## Reads first

- `docs/brief/01-master-brief.md` Section 20 only.
- `docs/plan/06-services/<service>.md`: entry points, events, permissions, and data sensitivity.
- `docs/brief/02-appendices/appendix-j-data-classification-and-retention.md` for what is sensitive, and `appendix-i-role-templates.md` for who holds what.
- `docs/brief/02-appendices/appendix-l-registry-and-id-codes.md` for the area code used in the test identifiers.
- `docs/templates/threat-model.md`.

## Output contract

- `## Boundaries` — a Mermaid `flowchart` of actors, entry points, stores, and trust boundaries
- `## STRIDE` — table: id, entry point, category, threat, likelihood, impact, control, test case ID
- `## Abuse cases` — table: id, actor, goal, path, control, test case ID
- `## Residual risk` — table: risk, why it remains, accepted by, review date
- `## Test gaps` — every control with no test case identifier

## Stop conditions

- Stop when the service sheet lists no permissions. A threat model over undefined authorization is fiction.
- Stop before accepting a residual risk yourself. Residual risk is accepted by a person, not by a model.
- Stop when a control needs a dependency that is not in the approved inventory.