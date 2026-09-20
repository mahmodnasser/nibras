# Multi-agent patterns for writing the plan

Some parts of the plan are naturally parallel. This file documents the patterns that work and the one rule that makes them safe. **Nothing here runs automatically.** These are patterns to use when you ask for them.

---

## The rule that makes parallel work safe

**Each worker owns a disjoint set of files, and no two workers may write the same file.** Everything else follows from that. A worker that needs a fact another worker is producing reads it from the canonical source, Appendix L, rather than inventing its own.

The second rule, learned the hard way during the v9 build: **a parallel worker will invent a definition if the canonical one is not written down first.** Write the registry before you fan out, and tell every worker to obey it by name.

---

## Pattern 1: service sheets, fanned out

Group C writes one sheet per service, 20 of them, and they are genuinely independent.

| Step | What happens |
|---|---|
| 1 | Write `05-service-catalog.md` first. It is the shared fact every sheet depends on |
| 2 | Fan out: one worker per service, each owning exactly `docs/plan/06-services/<service>.md` |
| 3 | Each worker reads: Appendix L, that service's sheet in reference architecture Section 8, its modules in Appendix A, its rows in B, C, E, F, G, K, its workflows in R and rules in S |
| 4 | Merge pass: run `/lint-plan`, then the plan-consistency-checker agent across all 20 sheets |
| 5 | Fix disagreements in **every** affected document, not just the one that looks wrong |

A sheet that disagrees with the message catalog or the dependency matrix is a defect, not a difference of opinion.

## Pattern 2: reviews, in parallel

`/plan-review <group>` runs several reviewers over the same documents at once. They only read, so there is no write conflict.

| Reviewer | Looks for |
|---|---|
| architecture-reviewer | Boundaries, layering, distributed-monolith signs |
| security-auditor | Tenancy, authorization, abuse cases |
| privacy-auditor | Children's data, retention, consent, access logging |
| messaging-reviewer | Outbox, inbox, idempotency, ordering, versioning |
| business-rules-reviewer | Recomputes every worked example and reports arithmetic errors |
| test-strategist | Whether each claim has a test |
| rtl-localization-reviewer | Arabic, right-to-left, calendars, numerals |
| portability-reviewer | Windows, Linux, mobile, kiosk |
| domain-expert | What a real school would find missing |

Collect the findings into one table, severity first, then decide. Reviewers disagree sometimes, and that is useful.

## Pattern 3: appendix or catalog work

Large catalogs (workflows, rules, error codes, load scenarios) are one file each and fan out cleanly. Give each worker the exact structural contract the lint enforces, so the merge is a lint run rather than a rewrite.

---

## What not to parallelise

| Do not fan out | Why |
|---|---|
| The service catalog itself | Everything else depends on it. One author, one pass |
| Appendix L | It is the definition of shared truth. Two authors produce two truths |
| The traceability matrix | It is assembled from everything else and must be written last |
| Anything touching `docs/brief/` | Brief changes need an ADR and a version bump on all three files |
| A group that has not had its predecessor approved | You would be building on a draft |

---

## After any parallel run

1. `/lint-plan`. Cross-file rules are exactly what parallel work breaks.
2. `/score-plan <group>` on the merged result, not on the pieces.
3. Update `PROJECT_STATE.md` once, at the end, by one author.
4. Record anything a worker reported as a conflict. During the v9 build, two real defects surfaced that way and neither would have been found by reading.
