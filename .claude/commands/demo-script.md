---
description: Produce or check the fifteen-minute demo for one role against the demo data
argument-hint: [role]
disable-model-invocation: true
---

Produce the fifteen-minute demo for the $ARGUMENTS role, or check the existing one, against the demo data in Appendix H.

Rules that make a demo work:

1. **Open on a problem, not a login screen.** The first thirty seconds show the role's real morning: what needs attention now.
2. **Every step is timed and every step is real.** Use the seeded tenant. No screen that needs data the seed does not contain. If a step needs data that is missing, that is a demo-data defect, and you report it.
3. **Three signature moments**, each under sixty seconds, each something a competitor cannot show. Name them explicitly.
4. **Show the second language.** One switch to Arabic with RTL, on a screen where the mirroring is obviously correct.
5. **Show the explanation.** Any automated decision in the demo shows why it decided that, and how a human overrides it with a reason.
6. **Plan the recovery.** For each step, what to say when it fails live, and which step can be cut when time runs short.
7. **End on the ask.** What the viewer is supposed to do next.

Budget: fifteen minutes with two minutes of slack. If the script exceeds thirteen minutes of content, cut a step rather than speeding up.

## Reads first

- `docs/brief/02-appendices/appendix-o-demo-script.md` in full: the demo already exists there, minute by minute.
- `docs/brief/02-appendices/appendix-h-demo-data.md` for what the seeded tenants actually contain.
- `docs/brief/02-appendices/appendix-w-feature-register.md` for the signature features and their assist rungs.
- The screen inventory for this role in `docs/plan/08-web-structure.md` or `docs/plan/09-mobile-structure.md`.

## Output contract

- `## Role and promise` — who is watching and what they should believe at the end
- `## Script` — table: step, minute, screen, what you do, what you say, the point it proves
- `## Signature moments` — table: moment, seconds, why a competitor cannot show this
- `## Demo data needed` — table: step, data required, present in the seed yes or no
- `## Failure plan` — table: step, what can break, what you say, the fallback
- `## Cut list` — steps in the order they get cut, with the total time saved

## Stop conditions

- Stop when a step needs data the seed does not have. Report it as a demo-data defect rather than improvising.
- Stop when a signature moment takes over sixty seconds. Redesign the moment or drop it.
- Stop when the role has no defined permission set: a demo that shows a screen the role cannot open is a lie.
