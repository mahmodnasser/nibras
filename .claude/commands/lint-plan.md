---
description: Run the kit lint and report defects grouped by rule with the minimal fix set
argument-hint: [path, defaults to the repository root]
allowed-tools: Read, Grep, Glob, Bash
disable-model-invocation: true
---

Run the kit lint over $ARGUMENTS, or over the repository root when no path is given.

Run `tools/kit-lint/kit-lint.ps1` on Windows PowerShell or `tools/kit-lint/kit-lint.sh` on bash. Both wrap the same Node implementation, so the findings are identical on either operating system. Use `--json` when you want to count and group precisely.

Then do the part the lint cannot do:

- Group every finding by rule, and inside a rule by file.
- Separate **real defects** from **missing upstream content**. A reference to a document that another workstream has not written yet is a sequencing fact, not a defect in the referring file.
- Find the **minimal fix set**: the smallest number of edits that clears the most findings. Several findings usually share one cause, such as one renamed service or one moved appendix.
- Check the baseline in `docs/project/KIT_LINT_BASELINE.md`. A finding that the baseline records as already known is context, not news.

Propose the fixes. Do not apply them until I agree, unless the fix is a pure typographical correction inside a file the lint itself flagged.

## Reads first

- `docs/project/KIT_LINT_BASELINE.md`.
- `tools/kit-lint/kit-lint.mjs` rule titles only, so you can explain what each rule protects.
- Only the files the lint actually flagged. Nothing else.

## Output contract

- `## Command` — the exact command run and the exit code
- `## Summary` — table: rule, errors, warnings, files affected
- `## Defects` — table: rule, file, line, message, root cause, fix
- `## Not defects` — findings caused by content that does not exist yet, with the workstream that owns it
- `## Minimal fix set` — ordered edits, each with the count of findings it clears
- `## After` — the expected error and warning counts once the fix set is applied

## Stop conditions

- Stop when the lint crashes. Report the rule that crashed and its message; do not work around it.
- Stop before editing any file outside the scope I gave you.
- Stop when clearing a finding would need a change to the canonical registry or to a brief document. Propose an ADR instead.