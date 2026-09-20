---
description: Import one school from a legacy system: mapping, dry run, reconciliation
argument-hint: [school or source system]
disable-model-invocation: true
---

Plan and run the import of $ARGUMENTS from its existing system. Nothing is written to the tenant until the dry run reconciles.

1. **Mapping template.** For every entity the school has (students, guardians and custody, staff, sections, subjects, timetable, historical results, attendance history, fee balances, documents), produce a mapping table: source field, target field, transform, required, default when absent, and the rule for a value that does not fit.
2. **Identity and duplicates.** Define the natural key per entity, the duplicate detection rule, and what happens on a suspected duplicate: merge, skip, or hold for a human. A guardian shared by three children is one person, not three.
3. **Dry run.** Import into a scratch tenant with no notifications, no emails, and no payment side effects. Produce counts in and counts out, the rejection report with a reason per row, and the sample of records a school administrator should eyeball.
4. **Reconciliation report.** Row counts per entity, money totals to the last unit, historical grade averages before and after, attendance day counts, and every discrepancy with its cause. A reconciliation that does not balance to the unit is a failed import.
5. **Cutover and rollback.** The order of entities, what runs in a freeze window, how long it takes on this school's volume, and how to undo it.

Never invent a value to make a row pass. A rejected row with a clear reason is worth more than a silently guessed one.

## Reads first

- `docs/plan/10-data-architecture.md` for tenancy, keys, and retention.
- The service sheets in `docs/plan/06-services/` for each entity's invariants and required fields.
- `docs/brief/02-appendices/appendix-f-entities.md`.
- The source system's export sample. Read the data, not the vendor's description of it.

## Output contract

- `## Source inventory` — table: source entity, rows, quality notes
- `## Mapping` — table: source field, target field, transform, required, default, unfit-value rule
- `## Duplicate rules` — table: entity, natural key, detection rule, action
- `## Dry run` — table: entity, in, imported, rejected, held
- `## Rejections` — table: row identifier, reason, suggested correction
- `## Reconciliation` — table: measure, source value, imported value, difference, explanation
- `## Cutover plan` — ordered steps with the freeze window and the rollback

## Stop conditions

- Stop before writing to a live tenant. The dry run comes first, always.
- Stop when money does not reconcile to the unit. Report the gap; never round it away.
- Stop when custody or guardian relationships are ambiguous in the source. A wrong custody record is a child-safety incident.
- Stop when the source export is a sample rather than the whole set, and say which it was.