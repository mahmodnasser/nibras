# Nibras: Project State

**Phase:** Planning complete, awaiting the product owner's approval. All 36 plan documents (00 to 34, with the 16 annex) are written and lint-clean. Scorecard round 5 (document 30): **all six groups approved at 4 on every axis**. Document 00 lists the decisions needed.
**Kit version:** v9, brief v9.5 (ADR-0019, ADR-0020, ADR-0021, ADR-0023, ADR-0025; ADR-0022 and ADR-0024 change only the plan), on `github.com/mahmodnasser/nibras`, branch `main`.
**Last updated:** 2026-09-26, scorecard rounds 4 and 5, remediation round 5, document 00

## Done: complete and lint-clean

| Group | Documents |
|---|---|
| A | 01 questions and assumptions; 02 competitive gap analysis (40 web checks, 31 sources) |
| B | 03 requirements catalog (**860 requirements**, validated by script); 04 architecture overview; 05 service catalog; 07 solution structure |
| C | 06: **all 23 service sheets**, every permission, event, error code and identifier checked against its catalog; 10 data; 11 messaging; 21 performance (130 hot queries) |
| D | 08 web; 09 mobile; 12 security (119 threat rows); 13 workflows and sagas; 14 design system |
| E | 15 deployment and operations; 16 test strategy; **17 roadmap (79 capabilities, MVP cut line)**; 18 risk register; 19 dependency inventory (147 dependencies, 142 verified at source) |
| F | 22 API conventions; 23 integrations; 24 localization; 25 assist ladder; 26 migration toolkit; 27 compliance; 28 capacity and cost; 29 ADR index; 31 rules and workflows assigned to code (generated); 32 differentiation and demo; 33 platform support |

## Done: document 34, the work breakdown (79 capabilities, 724 slices, 1,721 slice-days; 858 requirements built by slices, 2 Tier 3 explained)

| Step | Phases | Slices | State |
|---|---|---|---|
| Part A | 1 Foundation | 001 to 199 | **Done** |
| Part B | 2 The school year loop | 200 to 399 | **Done**. Parts A and B together: 38 capabilities, 353 slices, 825 slice-days, 618 requirements built |
| Part C, phase 3 | 3 Money and paperwork | 400 to 599 | **Done**: 14 capabilities, 135 slices, 321 slice-days, 106 requirements (REQ-INT-016's LTI part handed to phase 4). Validate with `--part C --phase 3` |
| Part C, phase 4 | 4 Growth | 400 to 599, continuing | **Done**: 8 capabilities, 84 slices, 210 slice-days; LTI 1.3 (SL-INT-411) now under CAP-INT-02, added to document 17 in fix 4 |
| Part D, phase 5 | 5 Extended | 600 to 799 | **Done**: 11 capabilities, 105 slices, 252 slice-days; QTI 3, Open Badges 3.0 and CASE now under CAP-INT-03 (fix 4); OPS-014 and OPS-015 (Tier 3) noted, not built |
| Part D, phase 6 | 6 Hardening and launch | 600 to 799, continuing | **Done**: 6 capabilities, 41 slices, 97 slice-days; builds the INF-027..033 and INF-036 handovers |

**How to resume.** Writer prompt: `tools/plan-build/wb-prompt.md` with `{{W}}`, `{{SCOPE}}` and `{{RANGE}}` filled. The writer appends to `tools/plan-build/parts/wb-part-<W>.md`, one capability at a time, so a partial part survives an interruption. Validate with `node tools/plan-build/assemble-34.mjs --part <W>`. Rewrite document 34 with `--write --partial` after each part, and `--write` once all four exist. See `tools/plan-build/README.md`.

## Then: remediation to approval (document 30 Section 5)

| # | Theme | Effort |
|---|---|---|
| 1 | Product-owner decisions: absence-alert timing; public API, OneRoster, iCal in Tier 1; countries, Apple build capacity, certifications | 1 hour of the owner's time |
| 2 | **Done**: document 17's ranges derived from document 34 by `schedule-34.mjs`: phase 1 14 to 22 weeks, launch 61 to 93, MVP 42 capabilities in 33 to 50; RISK-03 recorded as occurred | done |
| 3 | **Done**: ADR-0019 applied 317 brief corrections, all three briefs are v9.1; four value decisions settled (cooling-off 30 days, invitations 14 days, dedupe 5 minutes, feature 39 to engineering); R03 checks minor versions | done |
| 4 | **Done**: contradictions removed across 01-33 and all 23 sheets (113 open points closed or narrowed against ADR-0019); Gateway revoked-mark read added to 21; CAP-INT-02 and CAP-INT-03 added to 17 with their slices moved in 34; 79 capabilities; k6 in allow.json | done |
| 5 | **Done**: ADR-0020 (proposed) and brief v9.2; kit-lint R20 (one definition per test, no undefined citation, derived tests need their requirement) with 5 self-tests; 206 double definitions and 163 undefined citations resolved; registry annex generated (1,566 tests, 334 derived) | done |
| 6 | **Done**: ADR-0021 (proposed) and brief v9.3; 12 new kit-lint rules (R21 to R32) with self-tests, R18 strict for plan trees; generators gained `--check`; about 100 verification rows now name a real rule, a named review step or the building slice | done |
| 7 | **Done**: ADR-0022 (proposed); kit-lint R33; every open point scored on document 18's scales, 12 or more linked to a RISK; 12 Open points sections added; document 12 threats owned and linked; RISK-44 to RISK-53 added, RISK-40 closed; Open Questions 29 and 30 | done |
| 8 | **Done**: ADR-0023 (proposed) and brief v9.4; kit-lint R34; every one of the 43 signature features runs its own demo test in the release gate (SL-TST-006); documents 15, 16 and 32 aligned | done |

Then: rounds 4 and 5. Round 4 approved Group E and left C and F blocked on named gaps; remediation round 5 closed them (document 11 bindings, the Scheduling, Operations, Behavior and Wellbeing sheets, SL-ACA-405 for the LTI launch in phase 4, conditional e-invoicing slices, Open Question 31, ADR-0025 and brief v9.5 with launch at 61 to 95 weeks). Round 5 approved C and F. Document 00 written.

## Next: exactly where to resume

1. **Product-owner approval.** Hold the decisions workshop of document 00 Section 6: Open Questions 29 and 30 first, then 27, 28, 3, 14, 24, 26 and 31, then ADR-0024, ADR-0025 and the Proposed records phase 1 builds on. Record each answer in `OPEN_QUESTIONS.md` and, where it changes the brief, in an ADR with its version bump.
2. **Non-blocking gaps from round 5** (document 30 Section 5, each sized in minutes or hours). The most material: LTI Assignment and Grade Services is in scope in document 23 but no slice builds it, so REQ-ACA-030's grade return needs a phase 4 slice beside SL-ACA-405. Also: document 23 open point 7 still describes the old SL-FIN-444; Open Question 31 is missing from document 29's question table; test ids for the new timetable-PDF route and the Operations and Behavior generated-document consumers; `academics.md` still says document 21 has seven queries.
3. Once approved, start phase 1 with `/build-foundation`.
