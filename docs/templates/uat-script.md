# User acceptance test: `<feature>` for `<role>`

> Copy into `docs/brief/02-appendices/appendix-q-uat-scripts.md`. The tester is a school staff member, not an engineer. Every step is something they would really do, on the seeded demo tenant.

**Role** `<role>` · **Tenant** `<demo tenant>` · **Language and direction** `<English LTR | Arabic RTL>` · **Device** `<desktop | tablet | phone>`

## Before you start

| Precondition | How to get there |
|---|---|
| Signed in as | `<demo user>` |
| Data state | `<what must already exist>` |

## Steps

| # | What you do | What you should see | Pass or fail | Notes |
|---|---|---|---|---|
| 1 | `<action in the tester's words>` | `<observable result, including the data>` | | |

## Things we expect you to try to break

| # | Try this | What should happen |
|---|---|---|
| 1 | `<act without permission>` | `<clear refusal, no data shown>` |
| 2 | `<submit incomplete data>` | `<field-level error in your language>` |
| 3 | `<do it twice quickly>` | `<one result, not two>` |
| 4 | `<switch to the other language mid-task>` | `<the task continues, layout mirrors correctly>` |

## Result

| Question | Answer |
|---|---|
| Did you finish the task without help | `<yes or no>` |
| How long it took | `<minutes>` |
| Would you use this instead of your current method | `<yes or no, and why>` |
| What confused you | `<free text>` |

## Defects raised

| # | Step | What happened | What you expected | Severity |
|---|---|---|---|---|