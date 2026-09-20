---
description: UX, design, and motion review and polish
argument-hint: [workspace or screens]
allowed-tools: Read, Grep, Glob, Bash
disable-model-invocation: true
---

Review $ARGUMENTS as a senior product designer who has used five competing school products and found them all joyless.

Check:

- Does each dashboard card answer one question and lead to one action? Delete any chart nobody can act on.
- Visual hierarchy, spacing, type scale, and design-system consistency.
- Every state: loading skeleton, empty, error, offline, processing, no permission, and first run before any data exists.
- Microcopy in English and Arabic, written naturally in each, never as a translated fragment.
- Motion that explains rather than decorates, inside the duration and easing tokens, at 60 frames per second, with a reduced-motion fallback, and without `@angular/animations`.
- The teacher's five-minute test: can a teacher mark a class and enter marks without training?
- The parent's one-glance test: does the parent learn the one thing that matters in under five seconds?
- The golden-path demo against the demo data in Appendix H.

List issues by severity with a concrete before and after for each, then implement the ones I agree to.

## Reads first

- `docs/brief/01-master-brief.md` Sections 16 and 12 only.
- `docs/brief/02-appendices/appendix-o-demo-script.md` for the golden path, and `appendix-h-demo-data.md` for the data behind it.
- `docs/brief/02-appendices/appendix-u-persona-journeys.md` for the journey the screen belongs to.
- `docs/plan/14-design-system-and-ux.md` and the screen inventory for the workspace in scope.

## Output contract

- `## What I reviewed`
- `## Findings` — table: id, screen, issue, severity, before, after, effort
- `## States audit` — table: screen, loading, empty, error, offline, processing, no-permission, first-run, each marked present or missing
- `## Golden-path result` — where the demo path stalls, counted in steps
- `## Proposed changes` — ordered, with the ones needing my approval marked

## Stop conditions

- Stop and ask before changing a design token, a component contract, or the information architecture.
- Stop when the screen has no defined role and permission set. UX cannot be judged without knowing who sees it.
- Stop when Arabic copy does not exist yet. Review the English and say the Arabic pass is pending.