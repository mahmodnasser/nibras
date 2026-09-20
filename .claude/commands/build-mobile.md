---
description: Build the Flutter screens for a feature
argument-hint: [feature] [role]
disable-model-invocation: true
---

Build the Flutter screens for $ARGUMENTS, following `docs/plan/09-mobile-structure.md`.

Requirements:

- Design tokens shared with the web; no locally invented colors, spacings, or durations.
- Offline behavior and conflict rules per Section 18 of the master brief, with a visible sync state and a conflict resolution the user understands.
- Deep links from notifications land on the exact record, and degrade to a sensible screen when permission is missing.
- Every state is designed: loading, empty, error, offline, processing, no permission.
- Motion through implicit animations, `Hero`, and `flutter_animate`, with a reduced-motion fallback.
- English and Arabic with RTL, dynamic text size, and screen-reader labels on every control.
- Tests: widget tests, golden tests in LTR and RTL, and one integration test for the main flow.
- Flavors build cleanly, including the white-label flavor and a device without Google services.

State plainly which parts work offline and how each sync conflict is resolved.

## Reads first

- `docs/plan/09-mobile-structure.md`, the feature's section only.
- `docs/brief/01-master-brief.md` Section 18 only.
- `docs/brief/02-appendices/appendix-m-offline-conflict-rules.md`: the offline contract and the conflict rule per entity. Do not invent a conflict rule that is already written there.
- The service sheet in `docs/plan/06-services/` that owns the data, for its API table and permissions.
- `.claude/skills/flutter-multi-target/SKILL.md` before touching flavors or device targets.

## Output contract

- `## Screen plan` — table: screen, role, permission, states, offline behavior
- `## Conflict rules` — table: entity, conflict, resolution, who is told
- `## Built` — files added or changed, grouped by `data/`, `domain/`, `presentation/`
- `## Tests run` — command and result for `flutter analyze` and `flutter test`
- `## Parity` — what the web has here that mobile does not, and why

## Stop conditions

- Stop and ask before inventing an endpoint. If the service sheet has no endpoint for this screen, that is a plan gap.
- Stop when the feature has no offline decision recorded. Offline is a product decision, not a default.
- Stop before adding any package: run the license-auditor subagent first.