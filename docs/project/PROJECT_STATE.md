# Nibras: Project State

**Phase:** Planning, in its final stretch. 32 of 35 plan documents are complete and lint-clean; document 34 is half built (phases 1 and 2).
**Kit version:** v9, on `github.com/mahmodnasser/nibras`, branch `main`. The plan documents are committed.
**Last updated:** 2026-09-22, by the plan build

## Done: complete and lint-clean

| Group | Documents |
|---|---|
| A | 01 questions and assumptions; 02 competitive gap analysis (40 web checks, 31 sources) |
| B | 03 requirements catalog (**860 requirements**, validated by script); 04 architecture overview; 05 service catalog; 07 solution structure |
| C | 06: **all 23 service sheets**, every permission, event, error code and identifier checked against its catalog; 10 data; 11 messaging; 21 performance (130 hot queries) |
| D | 08 web; 09 mobile; 12 security (119 threat rows); 13 workflows and sagas; 14 design system |
| E | 15 deployment and operations; 16 test strategy; **17 roadmap (77 capabilities, MVP cut line)**; 18 risk register; 19 dependency inventory (147 dependencies, 142 verified at source) |
| F | 22 API conventions; 23 integrations; 24 localization; 25 assist ladder; 26 migration toolkit; 27 compliance; 28 capacity and cost; 29 ADR index; 31 rules and workflows assigned to code (generated); 32 differentiation and demo; 33 platform support |

## In progress: document 34, the work breakdown, phase by phase

| Step | Phases | Slices | State |
|---|---|---|---|
| Part A | 1 Foundation | 001 to 199 | **Done** |
| Part B | 2 The school year loop | 200 to 399 | **Done**. Parts A and B together: 38 capabilities, 353 slices, 825 slice-days, 618 requirements built |
| Part C, phase 3 | 3 Money and paperwork | 400 to 599 | **Done**: 14 capabilities, 135 slices, 321 slice-days, 106 requirements (REQ-INT-016's LTI part handed to phase 4). Validate with `--part C --phase 3` |
| Part C, phase 4 | 4 Growth | 400 to 599, continuing | Next. 8 capabilities, 60 requirements (Admissions, Behavior, Reporting), appended to `wb-part-C.md` |
| Part D | 5 Extended; 6 Hardening and launch | 600 to 799 | After C. 17 capabilities; also builds INF-027..033 and INF-036, handed over by part A |

**How to resume.** Writer prompt: `tools/plan-build/wb-prompt.md` with `{{W}}`, `{{SCOPE}}` and `{{RANGE}}` filled. The writer appends to `tools/plan-build/parts/wb-part-<W>.md`, one capability at a time, so a partial part survives an interruption. Validate with `node tools/plan-build/assemble-34.mjs --part <W>`. Rewrite document 34 with `--write --partial` after each part, and `--write` once all four exist. See `tools/plan-build/README.md`.

## Then

1. **20** traceability matrix: `node tools/plan-build/gen-20.mjs` once document 34 is complete.
2. **30** plan scorecard, then **00** executive summary.
3. The brief corrections found while planning (about 70 appendix defects, logged in `tools/plan-build/parts/brief-findings.md`), applied under ADR-0019 with a version bump to v9.1, so the plan's "Open points" can close.
4. Final lint, commit, push.

## Tooling added during the plan build

- `kit-lint` rule **R19**: every requirement, workflow, rule, capability, routing key, error code and permission a plan document cites must exist in its catalog. 28 self-tests pass.
- `tools/license-scan` parses SPDX expressions, treats fonts as assets, and requires every licence override to record its source. 12 tests pass.
- `tools/plan-build/`: the generators and validators for documents 03, 20, 31 and 34.

## Decisions waiting for the product owner

| Decision | Why it matters |
|---|---|
| Absence alert timing: the workflow catalog delays a parent's absence alert 30 minutes after the register closes; the requirements and service levels demand it within 30 seconds | A grace window avoids alarming parents over a teacher's correction; immediacy is what parents expect. The plan follows the requirement until decided |
| Move a read-only public API, OneRoster export and iCal from Tier 2 into Tier 1 | Document 02 found a public API is table stakes in 7 of 10 competitors |
| Target countries, Apple build capacity, and any required certification | Open Questions 3, 14 and 26; each changes cost or schedule |

## Plan progress

| Group | Status | Approved by owner |
|---|---|---|
| A | 01, 02 done; 00 last | No |
| B | All done | No |
| C | All done | No |
| D | All done | No |
| E | All done except 20 | No |
| F | All done except 30; 34 phases 1 and 2 done | No |

## Session protocol

Read this file and `OPEN_QUESTIONS.md` first, or run `/resume`. Run `/lint-plan` and `/score-plan` before stopping. Update this file and `TRACEABILITY.md` at the end of every session.
