---
description: Independent review of one plan group using the reviewer subagents
argument-hint: [group A-E, or a plan document path]
allowed-tools: Read, Grep, Glob, Bash
disable-model-invocation: true
---

Review plan group $ARGUMENTS as an independent reviewer. You did not write it. Assume it contains mistakes and find them.

Dispatch the reviewer subagents that match the group's content, in parallel:

| Group | Subagents to run |
|---|---|
| A (00, 01, 02) | domain-expert, product-innovator, plan-consistency-checker |
| B (03, 04, 05, 07) | architecture-reviewer, plan-consistency-checker, domain-expert |
| C (06, 10, 11, 21) | architecture-reviewer, messaging-reviewer, performance-reviewer, business-rules-reviewer |
| D (08, 09, 12, 13, 14) | ux-reviewer, rtl-localization-reviewer, security-auditor, privacy-auditor, portability-reviewer |
| E (15 to 20) | test-strategist, portability-reviewer, plan-consistency-checker, license-auditor |

Then reconcile. A finding two subagents raise independently is almost certainly real. A finding only one raises needs evidence before you repeat it. Drop anything that is a matter of taste.

Close with a verdict: approve, approve with fixes, or return for rework. Say which.

## Reads first

- `docs/plan/PLAN_SPEC.md`, the rows for the documents in this group.
- The group's plan documents themselves.
- `docs/brief/02-appendices/appendix-l-registry-and-id-codes.md` for names and identifier formats.

## Output contract

- `## Defects` — table: id, document, section, defect, severity (blocker, major, minor), evidence, fix, raised by
- `## Agreement` — which findings more than one subagent raised
- `## Coverage` — table: `PLAN_SPEC.md` requirement for this group, present yes or no, where
- `## Verdict` — approve, approve with fixes, or return for rework, with the reason
- `## Blockers only` — the shortest list that would change the verdict

## Stop conditions

- Stop when the group's documents do not all exist. Review is not a substitute for writing.
- Stop before editing a plan document. This command reviews; `/plan-platform` writes.
- Stop when two plan documents contradict each other on a fact you need. Report the contradiction as a blocker.