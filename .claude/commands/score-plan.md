---
description: Score a plan group against the rubric and approve only at 4 or above
argument-hint: [group A-E, or a plan document path]
allowed-tools: Read, Grep, Glob, Bash
disable-model-invocation: true
---

Score $ARGUMENTS against the rubric using `docs/templates/scorecard.md`. Score each dimension 0 to 5, **with evidence quoted from the document**. A score with no evidence is not a score.

| Dimension | 5 means |
|---|---|
| Completeness | Every item `PLAN_SPEC.md` requires for these documents is present and filled |
| Consistency | Names, identifiers, contracts, and counts agree with the canonical registry and with every other plan document |
| Feasibility | A team of the stated size could build this in the stated time, and the document says how it knows |
| Risk honesty | The hard parts are named as hard, with likelihood, impact, and an owner; nothing is quietly optimistic |
| Testability | Every requirement has an acceptance criterion a test could assert, and a test case identifier |
| Distinctiveness | The signature features are specified sharply enough to be built better than a competitor's version, not merely listed |
| Portability | Windows and Linux, LTR and RTL, web and mobile, and devices without Google services are all addressed where relevant |

Rules:

- **Approve only when every dimension is 4 or above.** One dimension at 3 blocks approval; say which and what would lift it.
- Score what the document says, not what you assume the author meant.
- A 5 is rare. Reserve it for work that needs no change.

## Reads first

- `docs/plan/PLAN_SPEC.md`, the rows for this group.
- The group's plan documents.
- `docs/brief/02-appendices/appendix-l-registry-and-id-codes.md` for the consistency dimension.
- `docs/templates/scorecard.md`.

## Output contract

- `## Scores` — table: dimension, score, evidence (quoted with document and line), what would raise it by one
- `## Verdict` — approved or blocked, with the blocking dimensions named
- `## Gap list` — table: gap, dimension, effort to close, blocking yes or no
- `## Re-score after` — the shortest list of changes that would reach 4 everywhere

## Stop conditions

- Stop and refuse to approve when any dimension is below 4, whatever the average.
- Stop when a document in the group is missing; score the group as incomplete rather than scoring what exists.
- Stop before editing the documents. Scoring and writing are different sessions.