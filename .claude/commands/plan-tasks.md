---
description: Break a phase into capabilities and slices an engineer can pick up on Monday
argument-hint: [phase number, or a capability id to split further]
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
disable-model-invocation: true
---

Produce or extend `docs/plan/34-work-breakdown.md` for: $ARGUMENTS

A phase with no slices is an intention. This command turns one into work.

## Reads first

- `docs/plan/PLAN_SPEC.md`, the section "How work is broken down: phases, capabilities, slices". It holds the sizing rules
- `docs/templates/work-slice.md` for the slice format and the worked example
- `docs/plan/17-roadmap.md` for the phase being broken down, its goal and its exit criteria
- `docs/plan/03-requirements-catalog.md` for the requirements in scope for that phase
- `docs/plan/06-services/<service>.md` for each service the phase touches
- Appendix R for the workflow transitions, Appendix S for the rules, Appendix V for the definition of done
- `docs/project/BACKLOG.md` for what is already queued

## What to do

1. **List the capabilities** for the phase. Each is something a named person can do end to end, one to three weeks, demonstrable. Name it as the person's sentence, not as a module.
2. **Split each capability into slices**, one use case each, one to three days, one pull request. Split by use case, never by layer. Use the template.
3. **Fill the Covers line for every slice**: requirement, rule, workflow transition and test case identifiers. A slice that maps to nothing is scope nobody asked for; find the identifier or drop the slice.
4. **State dependencies as contracts**, not as slices, wherever a contract can be published first. Say which slices each one blocks.
5. **Check both directions.** Every requirement in scope for the phase reaches at least one slice, and every slice reaches at least one requirement. List anything that fails, both ways.
6. **Name the demo** at the end of each capability, in one sentence, on the demo data.
7. **Queue the slices** in `docs/project/BACKLOG.md` in dependency order, with status Ready or Blocked.
8. Run the lint and report it.

## Output contract

Your response must contain these headings, in order:

- **Capabilities** — table: id, what a person can do, weeks, services
- **Slices per capability** — the blocks, with estimates
- **Coverage check** — requirements with no slice, and slices with no requirement. Both lists, even when empty
- **Critical path** — the longest dependency chain through the phase, and what it means for the range
- **What I did not split, and why** — anything left at capability level, with the reason
- **Queued** — how many slices entered the backlog, and how many are Ready today

## Stop conditions

Stop and ask rather than guessing when: the phase has no capabilities in the roadmap yet; a capability cannot be split below three days without splitting by layer; a requirement in scope has no owning service; or the phase depends on an open question whose default would change the breakdown materially.

Never invent a requirement identifier to justify a slice. If work seems necessary and no requirement covers it, that is a gap in the requirements catalog, and it is reported rather than papered over.
