# Nibras: Project State

**Phase:** Planning. 34 of 35 plan documents are written and lint-clean; only 00 (executive summary) is left. **The scorecard (document 30) blocks every group**: coverage is complete, but consistency, feasibility and testability are at 2. About 14 to 15 days of remediation in eight themes stand between the plan and approval.
**Kit version:** v9, brief v9.3 (ADR-0019, ADR-0020, ADR-0021; ADR-0022 changes only the plan), on `github.com/mahmodnasser/nibras`, branch `main`. Fix 7 is not yet committed.
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
| 8 | Every signature feature with a release-gated demo step | 1.5 days |

Then re-score with `/score-plan` (`node tools/plan-build/build-30.cjs` rebuilds document 30 from `parts/score-*.md`), write **00**, final lint, commit, push.

## Tooling added during the plan build

- `kit-lint` rule **R19**: every requirement, workflow, rule, capability, routing key, error code and permission a plan document cites must exist in its catalog. Rule **R20**: every test case is defined in exactly one document and every cited test exists (ADR-0020). Rules **R21 to R32** make the plan's verification claims true (ADR-0021). 46 self-tests pass.
- `tools/license-scan` parses SPDX expressions, treats fonts as assets, and requires every licence override to record its source. 12 tests pass.
- `tools/plan-build/`: the generators and validators for documents 03, 20, 30, 31, 34 and the test-case registry annex, and the schedule derivation for document 17.

## Decisions waiting for the product owner

| Decision | Why it matters |
|---|---|
| Absence alert timing: the workflow catalog delays a parent's absence alert 30 minutes after the register closes; the requirements and service levels demand it within 30 seconds | A grace window avoids alarming parents over a teacher's correction; immediacy is what parents expect. The plan follows the requirement until decided |
| Move a read-only public API, OneRoster export and iCal from Tier 2 into Tier 1 | Document 02 found a public API is table stakes in 6 of 10 competitors (Open Question 28) |
| Target countries, Apple build capacity, and any required certification | Open Questions 3, 14 and 26; each changes cost or schedule |
| Wellbeing check-in: may a level S answer wait on the device until sync? | Open Question 29, RISK-47, the top risk (20): the plan builds the outbox, which contradicts the no-device rule. Recommended: online only |
| Which count bills a tenant? | Open Question 30, RISK-52 (16): the brief and REQ-PLT-009 disagree with BR-FIN-017 |

## Plan progress

| Group | Status | Approved by owner |
|---|---|---|
| A | 01, 02 done; 00 last; blocked at scoring | No |
| B | All done; blocked at scoring | No |
| C | All done; blocked at scoring | No |
| D | All done; blocked at scoring | No |
| E | All done; blocked at scoring | No |
| F | All written; blocked at scoring (document 30) | No |

## Session protocol

Read this file and `OPEN_QUESTIONS.md` first, or run `/resume`. Run `/lint-plan` and `/score-plan` before stopping. Update this file and `TRACEABILITY.md` at the end of every session.
