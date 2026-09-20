---
description: Write the operations runbook for one alert
argument-hint: [alert name]
allowed-tools: Read, Grep, Glob, Bash
disable-model-invocation: true
---

Write the runbook for the $ARGUMENTS alert using `docs/templates/runbook.md`.

The reader is on call at 03:00, is not the author of the service, and has no context. Write for that person.

Every runbook contains:

1. **What fired and what it means in product terms.** Not "queue depth above 5000" but "absence alerts to parents are more than ten minutes late".
2. **Who is affected.** Which tenants, which roles, which children. Whether anything is silently wrong as opposed to visibly broken.
3. **First three checks**, each a copy-and-paste command with the expected healthy output beside it. Give the Windows PowerShell form and the bash form where they differ.
4. **Diagnosis tree.** A small decision path from the checks to the likely causes, ordered by how often each occurs.
5. **Mitigation** for each cause, marked reversible or not, with the blast radius of each action.
6. **What never to do during this alert.** Replaying a dead-letter queue that sends parent messages is the classic example.
7. **Escalation.** Who, after how long, and what to hand them.
8. **After.** The follow-up ticket, the monitoring gap this exposed, and the test that would have caught it.

## Reads first

- `docs/plan/15-deployment-and-operations.md`, the alert list and service-level objectives.
- `docs/plan/06-services/<service>.md` for the owning service's jobs, consumers, and dependencies.
- `.claude/skills/runbook/SKILL.md` and `docs/templates/runbook.md`.

## Output contract

- `## Alert` — name, threshold, what it means in product terms, severity
- `## Impact` — table: who, what they experience, silent or visible
- `## First checks` — table: check, command (PowerShell and bash), healthy output
- `## Diagnosis` — a Mermaid `flowchart` from symptom to cause
- `## Mitigations` — table: cause, action, reversible, blast radius
- `## Never do` — the explicit list
- `## Escalation` — table: after, to whom, with what
- `## Follow-up` — the ticket, the monitoring gap, the missing test

## Stop conditions

- Stop when the alert has no defined threshold. A runbook for an undefined alert cannot be written.
- Stop when you would have to guess at a command. Write only commands that exist in this repository.
- Stop before documenting a mitigation that sends messages to parents without a human confirmation step.