# Nibras: Project State

**Phase:** Planning, final stretch. All 35 plan documents except 00 are written and lint-clean. Scorecard round 3 (document 30): Groups A, B and D approved; C, E and F were blocked on a few named gaps, which round 4 fixes closed (see "Next" below). Left: re-score C, E and F (round 4), write 00, push.
**Kit version:** v9, brief v9.4 (ADR-0019, ADR-0020, ADR-0021, ADR-0023; ADR-0022 changes only the plan), on `github.com/mahmodnasser/nibras`, branch `main`. Round-3 fixes and round-4 fixes are in the working tree; see "Next".
**Last updated:** 2026-09-25, scorecard remediation themes 1 to 5

## Done: complete and lint-clean

| Group | Documents |
|---|---|
| A | 01 questions and assumptions; 02 competitive gap analysis (40 web checks, 31 sources) |
| B | 03 requirements catalog (**860 requirements**, validated by script); 04 architecture overview; 05 service catalog; 07 solution structure |
| C | 06: **all 23 service sheets**, every permission, event, error code and identifier checked against its catalog; 10 data; 11 messaging; 21 performance (130 hot queries) |
| D | 08 web; 09 mobile; 12 security (119 threat rows); 13 workflows and sagas; 14 design system |
| E | 15 deployment and operations; 16 test strategy; **17 roadmap (79 capabilities, MVP cut line)**; 18 risk register; 19 dependency inventory (147 dependencies, 142 verified at source) |
| F | 22 API conventions; 23 integrations; 24 localization; 25 assist ladder; 26 migration toolkit; 27 compliance; 28 capacity and cost; 29 ADR index; 31 rules and workflows assigned to code (generated); 32 differentiation and demo; 33 platform support |

## Done: document 34, the work breakdown (79 capabilities, 718 slices, 1,705 slice-days; 858 requirements built by slices, 2 Tier 3 explained)

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

Then re-score with `/score-plan` (`node tools/plan-build/build-30.cjs` rebuilds document 30 from `parts/score-*.md`), write **00**, final lint, commit, push.

## Next: exactly where to resume

1. **Round 4 re-score, Groups C, E and F only.** Groups A, B and D were approved in round 3 and their documents are unchanged since. Run the round-3 reviewer workflow, `tools/plan-build/rescore-workflow.js` (a Workflow tool script: limit GROUPS to C, E and F and change "round2" to "round3" and "round 3" to "round 4"), giving each reviewer `tools/plan-build/parts/round3/score-<G>.md` as the previous round. Each writes `tools/plan-build/parts/score-<G>.md`. Copy `parts/round3/score-A.md`, `-B.md` and `-D.md` into `parts/` unchanged first, so the round is complete, then create `parts/round4/` after scoring.
2. **Rebuild document 30:** `node tools/plan-build/build-30.cjs` (it reads every `parts/roundN/` and the current `parts/`; add the round-4 date to `ROUND_DATES`).
3. **Write document 00** (`docs/plan/00-executive-summary.md`, PLAN_SPEC row 00): what is built and for whom; the three deployment modes (developer, single-server Compose including the Linux VM appliance for Windows hosts, scale mode on Kubernetes); 23 deployable applications from Appendix L (20 data-owning services, the Gateway and two BFFs) and seven worker images; the phase ranges and MVP line quoted from document 17 Section 1 and Section 5; the top ten risks quoted from document 18 Section 5; the decisions needed: Open Questions 27 to 30 first (29 is the top risk), then 3, 14, 24, 26, and the Proposed ADRs 0001 to 0018 and 0020 to 0024; the scorecard verdict from document 30. Every number quoted from its owning document; the template sections and an Open points table (ADR-0022).
4. Regenerate in order (34, 31, 20, 30, then the 16 annex last), run `node tools/kit-lint/kit-lint.mjs .` and `node --test tools/kit-lint/kit-lint.test.mjs tools/license-scan/run.test.mjs`, then ask the product owner before committing and pushing.

Small non-blocking leftovers noted by the round-4 editors: `06-services/behavior.md` consumer row for its `behavior.events` queue; the daily-sheet permission (Appendix B amendment) as an open question; document 15 and Appendix N still write `/bff-mobile/` (Appendix N is brief: next ADR); document 34's SL-ACA-207 still places the LTI launch in phase 2 while document 17 says phase 4.
