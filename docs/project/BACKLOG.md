# Backlog

The ordered list of slices, taken from `docs/plan/34-work-breakdown.md` once it exists. A slice is one use case built through every layer it touches, one to three days, one pull request. `docs/templates/work-slice.md` is the template, and the sizing rules are in `PLAN_SPEC.md`.

Nothing is queued yet: the plan has not been written. Group E produces the phases and capabilities, Group F turns them into slices, and the first entries land here.

**Order is by dependency, then by phase.** A slice whose contract is published may start even if the slice that produces it is unfinished, which is what keeps the team from serialising.

| # | Slice | Capability | Phase | Service | Covers | Estimate | Depends on | Status |
|---|---|---|---|---|---|---|---|---|

## Status values

| Status | Meaning |
|---|---|
| Ready | Dependencies satisfied, contract published, someone could start today |
| Blocked | Named dependency not yet available. The blocker is in the Depends on column |
| In progress | One person, one slice. Two in progress for one person means neither is |
| In review | Pull request open, done list complete except the demonstration |
| Done | Every box in the slice's done list ticked, and demonstrated to someone who did not write it |
| Dropped | With a reason, and the requirement it covered reassigned or removed |

## Rules

- A slice enters this list only with its requirement, rule and workflow identifiers filled in. A slice that maps to nothing is scope nobody asked for.
- A slice estimated above three days is split before it is queued, not during.
- A requirement with no slice anywhere in this list is scope nobody will build. `/lint-plan` checks both directions.
- Status changes here, not in someone's head. `/plan-status` reads this file.
