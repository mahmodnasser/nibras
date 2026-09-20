---
description: Reproduce, diagnose, and fix a bug with a regression test
argument-hint: [description, where, evidence]
disable-model-invocation: true
---

Bug report: $ARGUMENTS

If the report does not state what happened, what was expected, where (service, screen, tenant, role, language, direction), and the evidence, ask for the missing parts before doing anything else.

Then, in order:

1. **Reproduce it with a failing test.** No fix starts before a red test exists.
2. Find the root cause, not the symptom. Name the commit, design decision, or missing rule that allowed it.
3. Search for the same mistake elsewhere in the repository. A bug that exists once usually exists three times.
4. Fix it, make the test pass, and run every suite the change touches.
5. Report exactly what you ran and what it printed.
6. When the cause is a gap in the brief, the plan, or an assumption, update the project memory and raise an open question or an ADR.

## Reads first

- The failing area's source and its existing tests.
- The service sheet in `docs/plan/06-services/` for the owning service, the business rule section only.
- `docs/brief/01-master-brief.md` Section 15 only, to check whether this is a known edge case that was never implemented.

## Output contract

- `## Reproduction` — the failing test, its name, and its output before the fix
- `## Root cause` — the mechanism, in two or three sentences, with file and line
- `## Same mistake elsewhere` — table: location, same pattern yes or no, action
- `## Fix` — what changed and why this is the smallest correct change
- `## Verification` — table: suite, command, result
- `## Follow-up` — open questions, ADR, or memory updates raised

## Stop conditions

- Stop when you cannot reproduce it. Report what you tried and ask for the missing evidence.
- Stop before widening the fix into a refactor. Propose the refactor separately.
- Stop when the correct behavior is genuinely ambiguous in the brief. Ask rather than choose.
