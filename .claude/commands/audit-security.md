---
description: Security and tenant isolation audit
argument-hint: [scope]
allowed-tools: Read, Grep, Glob, Bash
disable-model-invocation: true
---

Audit $ARGUMENTS against Section 20 of the master brief, OWASP ASVS 5.0 Level 2, and the API Security Top 10. Think like an attacker who is a logged-in parent in tenant A, and like an insider with too much access.

Try, at minimum:

- Swapping identifiers to reach other students, other guardians, and other tenants on **every** endpoint and every consumer.
- Replaying, forging, and reordering messages; consuming an event that names a tenant the caller does not own.
- Escalating privileges through the role editor, data scopes, delegation, and role switching.
- Abusing invitation links, join codes, gate passes, signed URLs, and QR verification pages.
- Bypassing locks on marks, attendance, and posted invoices; grade-change paths that skip the Requests workflow.
- Mass export and the report builder as an exfiltration channel; malicious uploads; the OCR and PDF parsers.
- Prompt injection through the Ai service, including retrieved content that acts as instructions.
- The seeded administrator safeguards in Section 10.2.

## Reads first

- `docs/brief/01-master-brief.md` Sections 20 and 10.2, plus Sections 7.4 and 7.5 for tenancy and authorization.
- `docs/brief/02-appendices/appendix-i-role-templates.md` for the default roles you are trying to escalate between, and `appendix-b-permissions.md` for the permission risk levels.
- `docs/plan/12-security-privacy-safety.md`.
- The service sheet in `docs/plan/06-services/` for the scope, and the endpoints, consumers, and policies themselves.

## Output contract

- `## Threats I attempted` — table: attempt, result (blocked, partial, succeeded), evidence
- `## Findings` — table: id, title, severity (critical, high, medium, low), reproduction steps, impact, fix, regression test name
- `## Isolation coverage` — every endpoint and consumer in scope with yes or no for a tenant isolation test, and the list of gaps
- `## Not checked` — what needs a running environment or credentials
- `## Smallest fix set`

## Stop conditions

- Stop and ask before running anything against a shared or production environment.
- Stop when the permission matrix for the scope is undefined. Guessed permissions produce false findings.
- Stop when a fix would change a published contract or a default role. That needs an ADR first.