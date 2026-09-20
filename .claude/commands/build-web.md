---
description: Build the Angular screens for a feature
argument-hint: [feature] [workspace]
disable-model-invocation: true
---

Build the Angular screens for $ARGUMENTS, following `docs/plan/08-web-structure.md`.

Show me the screen structure and the interaction plan **before** writing code.

Requirements:

- Design system components and tokens only. If a component is missing, add it to `libs/ui` with a Storybook story first.
- Every state: loading skeleton, empty, error, offline, processing, no permission.
- Permissions drive what is visible and enabled, and the server enforces them again. Never rely on the interface for security.
- Motion per Section 16.2 of the master brief: native CSS with `animate.enter` and `animate.leave`, view transitions between list and detail, `transform` and `opacity` only, reduced-motion fallback. Never `@angular/animations`.
- English and Arabic, LTR and RTL, light and dark, using logical CSS properties.
- Keyboard-first wherever data entry is heavy; WCAG 2.2 AA throughout.
- A Playwright end-to-end test for the main flow, an axe-core check, and visual snapshots in all four theme and direction combinations.
- Any screen that shows an automated decision follows the because-panel pattern: the reasons are visible and an override carries a reason.

## Reads first

- `docs/plan/08-web-structure.md`, the workspace and feature sections only.
- `docs/brief/01-master-brief.md` Section 16.2 only.
- The service sheet in `docs/plan/06-services/` that owns the data, for endpoints and permissions.
- `.claude/skills/because-panel-pattern/SKILL.md` when the screen shows a score, a ranking, or a suggestion.

## Output contract

- `## Screen structure` — table: route, screen, permission, components used, states
- `## Interaction plan` — the main flow as numbered steps, with the keyboard path
- `## Built` — files added or changed, grouped by feature, `shared`, `core`, `ui`, `data-access`
- `## Tests run` — table: command, result
- `## Snapshots` — the four theme and direction combinations, each pass or fail

## Stop conditions

- Stop and wait after `## Screen structure` and `## Interaction plan` on the first pass.
- Stop before adding a component to `libs/ui` without a Storybook story.
- Stop when the endpoint you need does not exist in the service sheet.
- Stop before adding any npm package: run the license-auditor subagent first.