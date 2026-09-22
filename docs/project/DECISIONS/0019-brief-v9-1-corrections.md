# ADR-0019: The brief is corrected to v9.1 from the defects the plan found

- **Status:** Accepted by the product owner, 2026-09-22 (approval to apply the corrections, and the four value decisions below)
- **Date:** 2026-09-22
- **Requirement IDs:** REQ-PLT-007, REQ-ATT-017, REQ-INT-016, REQ-PLAT-005

## Context

Writing the plan exercised every catalog in the brief, and the brief did not hold up everywhere. The service sheets needed events Appendix E did not list, permissions Appendix B did not have, error codes Appendix K did not define, and notification rows Appendix C lacked. Appendix R's workflows cited event keys and permissions that no catalog contained. Four values contradicted each other between appendices. Master brief Section 28 put phase 1 at 8 to 10 weeks, and document 34 then measured it at 423 slice-days. k6, an AGPL tool every load gate depends on, had no Section 6.4 row. Appendix W had no autonomy column although Section 25 says it does. Test identifiers collided between Appendices R, O and W.

Each defect was logged as it was found (`tools/plan-build/parts/brief-findings.md`) and the plan worked around it with a stated default. The scorecard (`docs/plan/30-plan-scorecard.md`) made the cost visible: consistency scored 2 in five of six groups, and much of that was the brief, not the plan. The rule in CLAUDE.md is that the brief changes only under an ADR with all three files bumped together, so the corrections are applied here as one record rather than piecemeal.

## Decision

**Apply every logged defect to the brief, and bump all three brief files to v9.1.** The corrections take the names the service sheets already use, so the brief and the plan converge rather than meeting in the middle. Where no sheet proposed a name, the name follows Appendix L's format and the change list says so.

**The product owner settled four conflicting values:**

| Question | Decision | Changed |
|---|---|---|
| Tenant-deletion cooling-off: 7 days or 30 days | **30 days**, with the export available throughout | Appendix R; document 13 follows |
| Invitation link validity: 7 days or 14 days | **14 days**, with a reminder at day 7 | Appendix J, master brief Section 10.4; the Identity sheet default follows |
| Notification deduplication window: 10 or 5 minutes | **5 minutes**; urgent templates are never deduplicated | BR-NOT-004 and its recomputed examples |
| Calendar-aware scaling (feature 39) as a signature feature | **Moved to engineering capabilities**: still built and measured, off the signature list and the demo script | Section 12.1, Appendices A, O, P, W |

**What changed, by area** (the full list, 317 entries with sources, is `docs/project/KIT_V9_1_CHANGES.md`):

| Area | Changes |
|---|---|
| Master brief | Section 6.4 gains the k6 row (standalone, CI only, never linked); Section 19 separates rate limits (429 with `Retry-After`) from plan quotas (402 `PLATFORM_PLAN_LIMIT_REACHED`); Section 28 carries the phase ranges derived from document 34 (phase 1 14 to 22 weeks, launch 61 to 93 weeks, MVP 42 capabilities in 33 to 50 weeks); Section 40 gains the PowerSchool regional-edition risk |
| Reference architecture | Section 8.0 lists the synchronous query calls the sheets need, within the one-hop rule, and the three Ai routes through Bff.Web; Section 16 pins Valkey 9.1 and never Redis below 8.0 |
| Appendix E | 27 events, the `RaiseApplicationFee` command, `sourceRefs` on the payment event, about 30 consumer bindings, ten scheduled jobs; `<service>.audit.recorded.v1` is the only audit key; Wellbeing events carry no clinical field |
| Appendices B and I | The missing permissions; two new permission groups (Notification, Assist) granted in the role templates; the nurse row is online-only as Appendix M requires |
| Appendices K, C, G, J, F | Nine error codes; thirteen notification rows; currency decimals follow ISO 4217 (JOD has 3); the Ai settings and entities (`embedding_chunk`); AI usage limits fall back to rung 1 |
| Appendices O, P, A, W | W gains the autonomy column; colliding test identifiers in W renumbered into the 800 range; O states the phase each minute can run from and gains a reserve bank of one-minute steps; P's competitor facts corrected (six of ten publish an API) |
| Appendices R, S, M, U | R cites only catalogued events and permissions; BR-NOT-004 recomputed; a long-vowel cross-script example in BR-L10N-001; U no longer implies offline approvals or clinic visits |
| Kit lint | Rule R03 compares major and minor versions, so a v9 and v9.1 mix is caught |

**Still open, deliberately not decided here:** absence-alert timing (Open Question 27) and moving the public API, OneRoster and iCal into Tier 1 (Open Question 28).

## Alternatives considered

- **Leave the brief at v9 and let the plan carry the workarounds.** Rejected. Every workaround is a place where the plan and its source disagree, and the scorecard counts each one against consistency. An engineer reading the brief would build the wrong thing.
- **Correct each appendix under its own ADR as the defect was found.** Rejected. It would have meant a dozen version bumps during planning, and defects in one appendix often depend on another (Appendix R's names depend on B and E), so they have to land together.
- **Let the plan's names win without reading the brief's.** Rejected where the brief already had a sound name: Appendix R switched to Appendix B's existing permission names instead of B gaining near-duplicates.

## Consequences

- The plan now cites the brief's names directly. Plan documents that still use a pre-v9.1 name, a renumbered test identifier (TC-PLT-801 to 805, TC-DOC-801, TC-INF-801) or a superseded value are listed in `KIT_V9_1_CHANGES.md` and are corrected under scorecard themes 4 and 5.
- `tools/plan-build/parts/brief-findings.md` is closed as a log; new brief defects start a new log and, when they matter, a new record.
- Master brief Section 28 now depends on document 34 through `tools/plan-build/schedule-34.mjs`. When the slices or the team change, the ranges are recomputed and the brief is bumped again.
- k6 is allowed as a standalone CI tool. Forgejo and Matomo need their own rows only if they are chosen.
