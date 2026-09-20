---
description: Children's data privacy audit: classification, retention, consent, access logging, export and erasure
argument-hint: [service or "all"]
allowed-tools: Read, Grep, Glob, Bash
disable-model-invocation: true
---

Audit $ARGUMENTS for children's data protection. The subject is a minor who did not choose to be in this system. Convenience never wins here.

Check all six:

1. **Classification.** Every field holding personal data is classified: ordinary, sensitive (health, wellbeing, counseling, special needs, custody, behavior incidents, biometric, location), or credential. Anything unclassified is treated as sensitive until it is classified.
2. **Retention.** Every classified store has a retention period, a trigger that starts the clock, an action at the end (delete, anonymize, archive), and a job that actually performs it. A retention policy with no job is a document, not a control.
3. **Consent.** Photo and media consent, data-sharing consent, and any optional processing each have a recorded grant, a recorded withdrawal, a timestamp, and an actor. Withdrawal takes effect everywhere, including cached copies, exports already generated, and published galleries.
4. **Access logging.** Every read of a sensitive record writes an audit entry with who, when, which student, and why the caller was allowed. Sensitive reads are logged, not only writes.
5. **Export.** A guardian or a school can export one student's complete record in a portable format, including only that student, with siblings and other children removed from shared items.
6. **Erasure.** Deleting a student removes or anonymizes data across every service, including message bodies, gallery tags, cached projections, search indexes, backups policy, and the audit trail's own personal fields. Name what legitimately survives erasure and why.

Custody is a first-class rule: a guardian without custody is not a guardian for access purposes.

## Reads first

- `docs/brief/01-master-brief.md` Section 20 only.
- `docs/brief/02-appendices/appendix-j-data-classification-and-retention.md`: the normative field-level classification, encryption, cacheability, and retention. Start here.
- `docs/brief/02-appendices/appendix-f-entities.md` for the fields in scope, and `appendix-i-role-templates.md` for who can reach them.
- `docs/plan/12-security-privacy-safety.md` and `docs/plan/10-data-architecture.md` for the plan's version of the same.
- `.claude/skills/because-panel-pattern/SKILL.md` where an automated decision touches a child's record.

## Output contract

- `## Classification` — table: service, entity, field, class, justification
- `## Retention` — table: store, period, clock trigger, end action, job that runs it, verified yes or no
- `## Consent` — table: consent type, granted where, withdrawn where, propagation path, gaps
- `## Access logging` — table: sensitive read path, logged yes or no, fields captured
- `## Export and erasure` — table: service, export path, erasure path, what survives, why
- `## Findings` — table: id, issue, severity, fix, test case ID
- `## Not checked`

## Stop conditions

- Stop when a field's sensitivity is genuinely ambiguous. Classify it as sensitive, flag it, and ask.
- Stop before proposing to shorten any retention period below a legal minimum you cannot verify.
- Stop when erasure would break a financial or audit record that must legally survive. Report the conflict rather than resolving it.