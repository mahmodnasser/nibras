# 30. Plan Scorecard

## Purpose

This document records how good the plan is, group by group, against the seven-axis rubric in `PLAN_SPEC.md` ("The scorecard"), with quoted evidence for every score. **A group is approved only at 4 or better on every axis; an average never substitutes for a low score.** It exists so the product owner approves the plan on evidence, and so the work left before approval is a named, sized list rather than a feeling. It keeps every scoring round, so the progress is visible and a score cannot quietly improve.

## Scope

| In scope | Owned elsewhere |
|---|---|
| Scores for Groups A to F in every round, with evidence, the verdict per group and the gaps | The rubric: `PLAN_SPEC.md`; the procedure: `/score-plan` and `docs/templates/scorecard.md` |
| What each remediation round changed, and what is left | The changes themselves: the documents, ADR-0019 to ADR-0024 and `docs/project/CHANGELOG.md` |
| | Document 30 does not score itself, and document 00 is written after the last round |

## Content

### 1. Method

In every round each group was scored by an independent reviewer instructed to be adversarial, to quote the document with file and line for every score, and to reserve 5 for work that needs no change. From round 2 on, each reviewer also received the previous round's scorecard for its group and was told not to take the remediation on trust: every earlier gap is marked Closed, Partly closed or Open with evidence. Every reviewer was given the same facts established by tooling (the kit lint and its rules, the generated documents, the requirement and slice counts) and asked to find what tooling cannot: contradictions of meaning, thin sections, unowned unknowns, claims with no test, and defects the remediation itself introduced.

### 2. Summary, round 6 (2026-09-26)

| Group | Completeness | Consistency | Feasibility | Risk honesty | Testability | Distinctiveness | Portability | Lowest | Verdict |
|---|---|---|---|---|---|---|---|---|---|
| A | 4 | 4 | 4 | 5 | 4 | 4 | 5 | 4 | Approved |
| B | 4 | 4 | 4 | 4 | 4 | 4 | 5 | 4 | Approved |
| C | 4 | 4 | 4 | 4 | 4 | 3 | 4 | 3 | Blocked (Distinctiveness) |
| D | 4 | 4 | 4 | 5 | 4 | 4 | 5 | 4 | Approved |
| E | 4 | 4 | 5 | 4 | 4 | 4 | 5 | 4 | Approved |
| F | 4 | 4 | 4 | 5 | 5 | 4 | 4 | 4 | Approved |
| **Lowest per axis** | **4** | **4** | **4** | **4** | **4** | **3** | **4** | | |

**Verdict: 5 of 6 groups approved (A, B, D, E, F).** The groups still blocked, and the axes that block them, are in the table above; Section 6 lists what closes each.

### 3. Score history

Each cell reads round 1 → round 2 → round 6. A group moves to approved only when every axis reaches 4.

| Group | Completeness | Consistency | Feasibility | Risk honesty | Testability | Distinctiveness | Portability |
|---|---|---|---|---|---|---|---|
| A | 3 → 4 → 4 → 4 → 4 → 4 | 2 → 3 → 4 → 4 → 4 → 4 | 3 → 3 → 4 → 4 → 4 → 4 | 3 → 3 → 4 → 4 → 4 → 5 | 2 → 3 → 4 → 4 → 4 → 4 | 3 → 3 → 4 → 4 → 4 → 4 | 4 → 4 → 4 → 4 → 4 → 5 |
| B | 3 → 4 → 4 → 4 → 4 → 4 | 2 → 3 → 4 → 4 → 4 → 4 | 3 → 4 → 4 → 4 → 4 → 4 | 3 → 4 → 4 → 4 → 4 → 4 | 3 → 3 → 4 → 4 → 4 → 4 | 3 → 3 → 4 → 4 → 4 → 4 | 4 → 4 → 4 → 4 → 4 → 5 |
| C | 4 → 3 → 3 → 3 → 4 → 4 | 3 → 3 → 3 → 3 → 4 → 4 | 4 → 4 → 4 → 4 → 4 → 4 | 3 → 4 → 4 → 4 → 4 → 4 | 4 → 3 → 4 → 4 → 4 → 4 | 3 → 3 → 4 → 4 → 4 → 3 | 3 → 3 → 4 → 4 → 4 → 4 |
| D | 4 → 4 → 4 → 4 → 4 → 4 | 2 → 3 → 4 → 4 → 4 → 4 | 3 → 4 → 4 → 4 → 4 → 4 | 3 → 3 → 4 → 4 → 4 → 5 | 3 → 4 → 4 → 4 → 4 → 4 | 4 → 4 → 4 → 4 → 4 → 4 | 4 → 4 → 4 → 4 → 4 → 5 |
| E | 3 → 4 → 4 → 4 → 4 → 4 | 2 → 3 → 3 → 4 → 4 → 4 | 2 → 3 → 4 → 4 → 4 → 5 | 3 → 3 → 4 → 4 → 4 → 4 | 3 → 4 → 4 → 4 → 4 → 4 | 3 → 4 → 4 → 4 → 4 → 4 | 4 → 4 → 4 → 4 → 4 → 5 |
| F | 3 → 3 → 3 → 4 → 4 → 4 | 2 → 3 → 4 → 3 → 4 → 4 | 3 → 4 → 4 → 4 → 4 → 4 | 3 → 3 → 4 → 4 → 4 → 5 | 3 → 3 → 4 → 4 → 4 → 5 | 3 → 4 → 4 → 4 → 4 → 4 | 4 → 4 → 4 → 4 → 4 → 4 |

| Round | Date | Lowest axis score | Axes below 4 | Groups approved |
|---|---|---|---|---|
| 1 | 2026-09-22 | 2 | 32 of 42 | 0 of 6 |
| 2 | 2026-09-26 | 3 | 22 of 42 | 0 of 6 |
| 3 | 2026-09-26 | 3 | 4 of 42 | 3 of 6 |
| 4 | 2026-09-26 | 3 | 3 of 42 | 4 of 6 |
| 5 | 2026-09-26 | 4 | 0 of 42 | 6 of 6 |
| 6 | 2026-09-26 | 3 | 1 of 42 | 5 of 6 |

### 4. What each remediation round changed

| Round | Theme | What changed | Record |
|---|---|---|---|
| After round 1 | 1. Product-owner decisions | Four conflicting values settled (cooling-off 30 days, invitations 14 days, deduplication 5 minutes, feature 39 to engineering); the open ones recorded as Open Questions 27 to 30 with defaults, scores and register risks | ADR-0019; `OPEN_QUESTIONS.md` |
| After round 1 | 2. Schedule matches the work | Phase ranges derived from document 34 by `schedule-34.mjs`: phase 1 14 to 22 weeks, launch 61 to 93 weeks, MVP 33 to 50 weeks | Document 17 Section 1 |
| After round 1 | 3. Brief corrections | 317 logged brief defects applied; brief v9.1 | ADR-0019 |
| After round 1 | 4. One meaning per fact | Named contradictions removed across the plan and all 23 sheets; the revoked-user mark readable by the Gateway; two Integrations capabilities added | Documents 04 to 34 |
| After round 1 | 5. One test-case registry | Every test defined in exactly one document; kit-lint R20; the registry annex generated | ADR-0020; brief v9.2 |
| After round 1 | 6. Verification claims that run | Twelve kit-lint rules (R21 to R32); every verification row names a rule, a named review or a slice; generators gain `--check` | ADR-0021; brief v9.3 |
| After round 1 | 7. Risk honesty | Every open point scored on the register's scales; 12 or more registered; threats owned; kit-lint R33 | ADR-0022 |
| After round 1 | 8. Signature features on the stage | Every signature feature runs its own demo test in the release gate; kit-lint R34 | ADR-0023; brief v9.4 |
| After round 2 | Round-2 gaps | The "Signature feature trace" in document 32; Platform notes and saga diagrams in the sheets; macOS in dev-smoke; Tier 2 requirements built early listed with reasons and checked by R35; open decisions no longer stated as settled; risk rows of 12 or more registered wherever they appear (R24) | ADR-0024 |
| After round 3 | Round-3 gaps | Document 11 routes `GenerateDocument` from Behavior, Hr, Operations and Scheduling and binds the generated event back; the Bff.Web digital-twin composer; the feature 43 owner; the phase 2 and 3 demo cells rebuilt from Appendix O; the macOS dev-smoke leg stated one way; document 31 lists transition-test ids per workflow | Documents 11, 17, 31 and the sheets |
| After round 4 | Round-4 gaps | `notification.commands` binds every `RequestNotification` sender, and `operations.events` the generated-document event; stale sheet points closed; the LTI 1.3 launch moved from SL-ACA-207 (phase 2) to SL-ACA-405 (phase 4); conditional e-invoicing slices SL-FIN-448 to SL-FIN-452; launch 61 to 95 weeks; Open Question 31 | ADR-0025; brief v9.5 |

### 5. Scores by group, round 6

#### 5.1 Group A: Understanding (01, 02; 00 last)

| Axis | Score | Evidence | What would raise it by one |
|---|---|---|---|
| Completeness | 4 | Document 00 now exists and covers every element it needs: purpose, users, modes (`00:44`), application count (`00:54`), roadmap with the MVP line (`00:69`), top ten (`00:99`), decisions (`00:118`) and verdict (`00:168`). 01 has all 31 questions (`01:5` "Thirty-one questions are open"). 02 is complete, with its second check, which is honestly typed as snippets (`02:7` "All seven fetches were refused by the network egress policy"). There is one stale section. `00:172` says of LTI AGS "no slice builds it", but `34:1060` SL-INT-412 and `34:1061` SL-ASM-400 now build it. | Rewrite `00:172` to name the largest gap still open after round 6, not the AGS gap that round 6 closed. |
| Consistency | 4 | I recomputed every figure 00 quotes against its owning document and all of them match. Examples: `00:38` "79 capabilities, 726 slices and 1,724 slice-days" against `34:43`; USD 195.07/113.72/50.50 against `28:372-374`; the top ten against `18:192-201`; "Thirty more" (1+29) against `18:203`; `00:79` "71 to 110" against `17:38`; the L/I/scores against 01. Q9 now agrees across `01:28`, `34:1359`, `OPEN_QUESTIONS.md:19` and `00:126` (4x3=12). New drift: (1) `00:172` contradicts `00:38`, `17:409` and ADR-0025 about the AGS slices. (2) `01:43` "against Phase 4's 210", but `34:39` gives 215. (3) `00:162` "one decisions workshop of about an hour", taking 27, 28 and 31 in it, but `REVIEW_GUIDE.md:21` says "One 90-minute session" and puts 27 and 28 "in writing". (4) `01:5` "the thirty that `REVIEW_GUIDE.md` counts", but `REVIEW_GUIDE.md:17` "holds **31 open questions**". | Fix these four sentences. In REVIEW_GUIDE's written list, add question 31 so that it agrees with 01's seven. |
| Feasibility | 4 | Every pull-forward is now sized from 34 with the stated formula, and I recomputed each one. `01:59` "SL-INF-608 ... and SL-INF-609 ... 5 slice-days ... about 0.2 weeks with eight builders and 0.3 with five" (18 in all, 0.6 and 0.9). `01:46` "SL-IDN-011 (3 slice-days) ... A yes therefore adds 0 slice-days to Phase 1". `01:28` "14 slice-days ... about 0.5 weeks ... and 0.7 with five". Wellbeing 61, Hr 62, Operations 72 and CAP-INT-01 17 of 27 all match 34. One stale base remains: `01:43` "against Phase 4's 210" (215 now). | Update the Phase 4 base in `01:43` to 215. |
| Risk honesty | 5 | Every question and open point carries L, I, a score and a register link. 00 quotes them unchanged (`00:118` "L, I and Score are quoted from document 01"). Its open points are scored too (`00:210` "Whether the twenty-five Proposed records are confirmed ... 3 \| 4 \| 12 \| RISK-44"). The honest contradictions are stated: `01:49` "**Until the question is decided this default contradicts the rule**"; `01:48` "the plan is not consistent with itself until it is answered". Q9's e-invoicing case now scores as RISK-53 does (`01:28` "4 \| 3 \| 12 \| RISK-23, RISK-53"). | None needed. |
| Testability | 4 | Every row of "Where Nibras is different" now names a defined test: `02:92` "`TC-SEC-393` (document 12 ...)"; `02:93` "`TC-AI-608` ... and `TC-RPT-327`"; `02:95` "`TC-INF-108` ... `TC-TST-218`". The registry defines each of them (`16-annex:132`, `:838`, `:1224`, `:1557`, `:1611`). Residue: `02:95` "the developer mode is proved by `dev-smoke.yml`" names a workflow, not an identifier, although `TC-PLAT-800` and `TC-PLAT-013` exist (`16-annex:1061`, `:1068`). | Cite `TC-PLAT-800` (and `TC-PLAT-013`) for the developer mode at `02:95`. |
| Distinctiveness | 4 | The absence-based standings are relabelled: `02:93` "**Unmatched on the check date,** as a stated principle"; `02:94` "reunification with verified pickup is unmatched on the check date"; `02:95` "**Unmatched on the check date.**". The standings take in the second check's narrower competitor forms and lead with the part competitors lack (`02:161` "the rule version, the guardian's view, and reunification with verified pickup"). One label remains: `02:88` "**Confirmed as a difference in depth,**". It rests on "no product was found that merges gate, bus and approved leave", an absence, against `02:9` "silence is recorded as unverified". | Relabel `02:88` "Unmatched on the check date (difference in depth)" and add it to the re-check list at `02:195`. |
| Portability | 5 | The APK wording is fixed in all three places: `01:32` "a separate APK built from the same source with the no-Google flavour flag"; `02:50` the same; `33:171` "A separate APK per flavour, built from the same source". Runners are named for every platform behaviour: `01:32` "on the ubuntu runner"; `01:129` "`ubuntu-latest`, `windows-latest` and `macos-latest` runners"; `01:58` hosted macOS minutes. 00 names the Windows appliance path (`00:47` "a school with a Windows host runs the Linux virtual machine appliance"). | None needed. |

**Verdict:** Approved

| Round-5 gap | Now | Evidence |
|---|---|---|
| `02:168` "Every difference names a passing test", but three rows cited no TC | Closed | `02:92` "`TC-SEC-393` (document 12"; `02:93` "`TC-AI-608` (Ai sheet" and "`TC-RPT-327`"; `02:95` "`TC-INF-108` (document 15" and "`TC-TST-218` (document 16"; all five are in the registry |
| Three absence-based standings labelled "Confirmed" | Closed | `02:93` "**Unmatched on the check date,** as a stated principle"; `02:94` "reunification ... is unmatched on the check date"; `02:95` "**Unmatched on the check date.**" (`02:88` still says Confirmed; it is a new gap below) |
| Q9: 01 said phase 5 and 3x3, against 34 (phase 3) and 4x3 | Closed | `01:28` "built by the Finance owner's team in Phase 3, as the conditional slices SL-FIN-448 to SL-FIN-450" and "4 \| 3 \| 12"; `34:1359` and `OPEN_QUESTIONS.md:19` agree |
| Section 8 summary listed item 6 under "no ADR of their own" | Closed | `01:154` "**Five carry a Proposed ADR** ... item 6 (the assist ladder, ADR-0015)"; "The remaining eight ... items 2, 3, 4, 5, 10, 11, 12 and 13" |
| Q16 and Q18 pull-forwards unsized | Closed | `01:59` "SL-INF-608 to SL-INF-615, 18 slice-days ... 5 slice-days that move into Phase 1"; `01:46` "SL-IDN-011 (3 slice-days) ... adds 0 slice-days"; recomputed against 34 |
| "The same APK with the no-Google flavour flag" | Closed | `01:32` and `02:50` "a separate APK built from the same source"; `33:171` "A separate APK per flavour" |
| `02:97` Tier cell stated the recommendation as the tier | Closed | `02:118` "2 in force (the default of Open Question 28); 1 for the read-only half only if the product owner accepts" |
| Document 00 not written | Closed (with new drift) | `00:1` "# 00. Executive Summary", written after Group F; its figures match 03, 17, 18, 28, 34 and Appendix L, except the `00:172` AGS sentence and the `00:162` workshop length |

| Gap | Axis | Effort to close | Blocking |
|---|---|---|---|
| `00:172` says LTI Assignment and Grade Services has "no slice" and is "the most material" gap, but `34:1060-1061` (SL-INT-412, SL-ASM-400), `17:409`, ADR-0025 and 00's own count at `00:38` include them | Consistency, Completeness | 0.25 hour | no |
| `00:162` says "one decisions workshop of about an hour" and puts 27, 28 and 31 in it. `REVIEW_GUIDE.md:21` says "One 90-minute session", and `REVIEW_GUIDE.md:29` and `01:5` say those are answered in writing | Consistency | 0.1 hour | no |
| `01:5` "the thirty that `REVIEW_GUIDE.md` counts", but `REVIEW_GUIDE.md:17` counts 31. REVIEW_GUIDE's written list (six) omits question 31, while 01 lists seven | Consistency | 0.1 hour | no |
| `01:43` "against Phase 4's 210 [slice-days]", but `34:39` and `17:409` give 215 after round 6 | Feasibility, Consistency | 5 minutes | no |
| `02:88` "**Confirmed as a difference in depth**" rests on "no product was found", an absence the method at `02:9` records as unverified | Distinctiveness | 5 minutes | no |
| `02:95` proves the developer mode by "`dev-smoke.yml`" with no TC identifier; `TC-PLAT-800` and `TC-PLAT-013` exist | Testability | 5 minutes | no |
| `00:3` lists the documents it quotes as "03, 05, 15, 17, 18, 28, 29, 30 and 34", but it also quotes 01 (L/I/scores), 32 and Appendix L | Completeness | 5 minutes | no |

#### 5.2 Group B: Requirements and architecture (03, 04, 05, 07)

| Axis | Score | Evidence | What would raise it by one |
|---|---|---|---|
| Completeness | 4 | Section 8 now runs to ADR-0025: `04:559` "ADR-0024 \| Every Tier 2 or Tier 3 requirement a phase 1 to 4 slice builds", `04:560` "ADR-0025". But ADR-0026 (Proposed, round 6) is missing, and `04:632` "Section 8 covers ADR-0001 to ADR-0025". 10.3 is now complete: `07:859` "Thirty-three rules ... This table is the single list of architecture rules." | Add an ADR-0026 row to 04 Section 8 (the brief route fix for Appendix N), and make `04:632` read "ADR-0001 to ADR-0026". |
| Consistency | 4 | The round-5 drift is fixed: `16:23` "the thirty-three rules in document 07 part 10.3", which matches `07:859`. `04:64` now names SL-NOT-002 and ADR-0024. The round-6 edits caused new drift. `04:613` "Twenty-four of the twenty-five ADRs" disagrees with `29:40` "Twenty-six records: twenty-five Proposed". `05:270` "`TC-TST-740` (document 07) fails a nested synchronous call" contradicts `07:849` "it cannot see the whole rule". | Make `04:613` read "Twenty-five of the twenty-six" and add 0026. In `05:270`, cite `TC-API-072` as the proof of the nested-call rule, and do the same for "enforces" at `07:193`. |
| Feasibility | 4 | The ownership split of the rules can be built and agrees with document 34: `07:859` "SL-TST-003 writes TC-TST-101 to TC-TST-124 ... SL-DATA-001 adds `TC-PERF-962`", and `34:65` "whose rules are the single list of `07-solution-structure.md` §10.3". Unknowns have owners (`05:257`, `04:609-615`). The group still states no team size. `05:258` says "A small team runs twenty services", with no figure. | Quote the Open Question 24 team-size default and the phase-1 range beside the 206-project count at `07:27` or in 04. |
| Risk honesty | 4 | `03:1119` now says: "Group B is approved with this conflict open: approval of this catalog does not wait for the decision and does not make the action overdue. The ADR is due before SL-PLT-010 starts". The other documents still say otherwise: `18:194` gives the due date as "Group B approval, before any metering slice", as does `00:103`, and the `18:147` trigger is "still open at the Group B approval". | Change the due cells at `18:194` and `00:103` (and the trigger at `18:147`) to "before SL-PLT-010 starts", as 03 and Open Question 30 state. |
| Testability | 4 | The static and runtime proofs are now separate. `07:849` says "static half ... The runtime proof of the rule is `TC-API-072`". `04:329` says the static test "cannot follow a nested call made through an Application port". The five derived rows `07:853-858` match document 20 (`20:618`, `20:619`, `20:772`, `20:780`, `20:801`). One overclaim remains at `05:270` (see Consistency). | Make the `05:270` verification row cite `TC-API-072` as the proof of the one-hop rule. |
| Distinctiveness | 4 | 03 and 04 cite document 32's trace and do not copy it (`03:69`, `04:568` "held once, in the 'Signature feature trace'"). The ladder rule is stated: `04:568` "a feature above it falls back downward to a rung 1 experience". There is still no per-feature fallback in 03 or 04. `04:613` defers it to Appendix W ("every feature above rung 1 names its degraded form in Appendix W"). | For each rung 2 or 3 feature, name its rung-1 fallback, or cite the column of document 32 that holds it. |
| Portability | 5 | `04:63` "Proved in the pipeline by `TC-NOT-610` ... and `TC-MOB-988` ... then on a real device by the per-release device pass on one device without Google services". `16:451` lists "one device without Google services". `07:44` "on ubuntu-latest, windows-latest and macos-latest (REQ-PLAT-001)". `07:852` has a mechanised path rule. | None needed. |

**Verdict:** Approved

| Round-5 gap | Now | Evidence |
|---|---|---|
| TC-TST-740 sees only `.Api`; `04:329`/`07:970` overclaim; `TC-API-072` uncited | Closed | `07:849` extends the rule to `.Application`, states "it cannot see the whole rule" and cites "`TC-API-072` (document 22)". `04:329` and `07:975` cite the runtime proof. `05:270` still overclaims, a new minor gap below |
| 10.3 omits the architecture tests 03 requires; SL-TST-003 a second home | Closed | `07:852-858` has rows for `TC-PERF-962`, `TC-PERF-963`, `TC-DATA-951`, `TC-DATA-959` and `TC-MSG-952`. `07:859` "SL-TST-003's pack is this project". `34:65` agrees |
| `16:23`/`:93` "twenty-four rules" against 07 "Twenty-eight" | Closed | `16:23`, `16:93` and `16:778` all say "thirty-three", and 10.3 has 33 rows (`07:821-858`) |
| RISK-52 due "at the Group B approval" makes the action overdue | Partly closed | 03 is fixed: `03:1119` "does not make the action overdue ... due before SL-PLT-010 starts". `18:194` and `00:103` still say "Group B approval, before any metering slice" |
| `04:64` Huawei "when the device share justifies it" against SL-NOT-002 phase 1 | Closed | `04:64` "`34-work-breakdown.md` SL-NOT-002 builds the Huawei adapter in phase 1 today; ADR-0024 (Proposed) lists that row as a Move" |
| `07:986` stale note deferring `GrpcHopRules` location to 22 | Closed | The note is gone. `07:992` "the note deferring the location of `GrpcHopRules` to document 22 dropped" |
| 04 Section 8 leaves out ADR-0024 | Closed | `04:559` has the ADR-0024 row. `04:613` says ADR-0024 and ADR-0025 "are listed because Section 8 covers every decision about how the plan itself is built". ADR-0026 is now missing (new gap) |
| No runner or device lane proves the non-Google push fallback at `04:63` | Closed | `04:63` cites `TC-NOT-610`, `TC-MOB-988` and the device-pass lane. `16:451` includes "one device without Google services" |

| Gap | Axis | Effort to close | Blocking |
|---|---|---|---|
| 04 Section 8 has no ADR-0026 row. `04:613` "Twenty-four of the twenty-five ADRs" and `04:632` "ADR-0001 to ADR-0025" disagree with `29:40` "Twenty-six records: twenty-five Proposed" | Consistency | 20 minutes | no |
| `05:270` says `TC-TST-740` "fails a nested synchronous call", and `07:193` says `GrpcHopRules` "enforces" the one-hop rule. Both contradict `07:849` (static half only; `TC-API-072` is the proof) | Testability | 15 minutes | no |
| RISK-52 due date: `03:1119` "does not make the action overdue ... due before SL-PLT-010 starts"; `18:194` and `00:103` "Group B approval, before any metering slice"; `18:147` trigger "still open at the Group B approval" | Risk honesty | 15 minutes | no |
| No team-size or duration figure appears beside the 206-project structure (`07:27`); `05:258` says only "a small team" | Feasibility | 30 minutes | no |
| Per-feature rung-1 fallback not stated in 03 or 04; `04:613` defers to Appendix W | Distinctiveness | 1 hour | no |

#### 5.3 Group C: Services, data, messaging, performance (06, 10, 11, 21)

| Axis | Score | Evidence | What would raise it by one |
|---|---|---|---|
| Completeness | 4 | The round-5 residues are filled. `behavior.md:192` adds "`portfolio_exports` ... `generated_document_id uuid null` (the book from Documents; the id the export link opens)". `platform.md:235-236` add `LtiLineItem` and `LtiScore`, and `assessment.md:128-129` add `source_tool_id` and `source_score_id` for SL-ASM-400. The one missing piece is feature 42's data path, scored under Distinctiveness | Give Assessment the component-to-outcome link and the section heatmap route (see Distinctiveness) |
| Consistency | 4 | The round-5 drift is fixed. `academics.md:622` says "(eight queries: 1 to 7 ... and 10, the kindergarten daily-sheet class view", which matches doc 21:610. `scheduling.md:314` cites "`notification.events.bulk` (`11-messaging-architecture.md` §2.5)". `RecordToolScore` agrees across `11-messaging-architecture.md:346`, `platform.md:635` and `assessment.md:471`. New drift in round 6: the `bff-mobile.md:595` review row says Appendix N "still write `/bff-mobile/` ... needs a brief ADR". This contradicts `bff-mobile.md:584`, which says "1. Closed ... ADR-0026 (brief v9.6) corrected Appendix N", and the brief, where Appendix N:93 has `/bff/mobile/v1/home/principal` | Add a review row to `bff-mobile.md` that records the ADR-0026 closure, or correct row 595, so that the record matches open point 1 |
| Feasibility | 4 | The consumers can now find their rows. `operations.md:420` says "`subjectId` ... is the row the document is for: the `purchase_orders.id` ... the `safety_records.id`". `behavior.md:335` says the same for "`awards.id` ... `portfolio_exports.id`". The LTI score path is buildable. `11-messaging-architecture.md:346` routes it "through the section 2.3 default binding of `assessment.commands.#` on `nibras.platform`", and replies go to `platform.replies` (doc 11:371). Feature 42 is the exception, scored under Distinctiveness | Close the feature 42 data path |
| Risk honesty | 4 | Open points are narrowed and re-scored honestly. `scheduling.md:1066` says "Still open: the Appendix C row ... \| 2 \| 1 \| 2 \| RISK-43". `bff-mobile.md` open point 5 keeps "**Until decided, this default contradicts the rule that wellbeing data never reaches a device.**" Residue: `08-web-structure.md:858` says the class heatmap endpoint "belongs to the Assessment sheet, not to this document", but the Assessment sheet has no open point for it | Add an Assessment open point for the feature 42 class view and its data path, scored and owned, until the sheet specifies them |
| Testability | 4 | Every round-5 untested surface now has an id. `scheduling.md:991` defines "TC-SCD-135 \| `POST /timetable-versions/{id}/document` ... answers 202". `operations.md:1322-1323` define TC-OPS-624 and TC-OPS-625, and `behavior.md:855-856` define TC-BEH-345 and TC-BEH-346. `scheduling.md:992` defines TC-SCD-136 for the cancellation notice. The new LTI path has `assessment.md:1130` "TC-ASM-338 ... delivered twice with the same `sagaId`" and TC-ASM-339 | Give feature 42's heatmap a sheet test id beyond the Appendix W demo test `TC-ASM-811`. Today no TC in the sheet asserts a heatmap value |
| Distinctiveness | 3 | Signature feature 42 cannot be built from its owning sheet. `03-requirements-catalog.md:311` and `34-work-breakdown.md:603` (SL-ASM-219) require a heatmap "per student and class". `assessment.md:333` offers only "`/api/v1/assessment/students/{id}/standards-heatmap`", and `08-web-structure.md:858` says "A class grid would need one call per student, which Section 9 forbids". Worse, no Assessment entity links a mark or component to a standard or outcome. `assessment.md:89` `components` has no outcome field, and §3.9 keeps no outcome copy. So "Proficiency per standard" has no input | Add a component-to-outcome mapping, for example `components.outcome_ids uuid[]` plus a reference copy of Academics outcomes and `StandardMapping`. Add `GET /sections/{id}/standards-heatmap`, the rung 2 model's inputs, and an integration test |
| Portability | 4 | No-Google citations name their runner. `academics.md:971` has "`TC-MOB-988` (document 20), the no-Google device-pass test of document 33 part 7 \| Device pass, per release". The mobile prefix is now uniform. `bff-mobile.md:560` says "Document 15 and Appendix N use the same prefix since ADR-0026 (brief v9.6)", and the brief's Appendix N:129 has `POST /bff/mobile/v1/sync/batch` | Give the LTI tool-score path (phase 4) a platform-notes row in the Assessment and Platform sheets. The Draft mark's tool name must render in right-to-left in the grid |

**Verdict:** Blocked (blocking axes: Distinctiveness)

| Round-5 gap | Now | Evidence |
|---|---|---|
| New routes and consumers have no test ids (timetable PDF route, Operations `DocumentGeneratedConsumer`, Behavior portfolio path) | Closed | `scheduling.md:314` cites TC-SCD-135, defined at `scheduling.md:991`. `operations.md:439` cites TC-OPS-624 and TC-OPS-625, defined at 1322-1323. `behavior.md:355` cites TC-BEH-345 and TC-BEH-346, defined at 855-856 |
| `behavior.md:352` has no column for the portfolio book's `documentId` | Closed | `behavior.md:192` adds `portfolio_exports.generated_document_id`. `behavior.md:355` writes it with "`status` set to `Generated`". `behavior.md:721` adds it to the tree's `RecognitionConfiguration.cs` |
| No sheet says that `subjectId` is the award, purchase-order or safety-record id | Closed | `behavior.md:290` has "`subjectId` the new `awards.id`". `behavior.md:294` covers `portfolio_exports`. `operations.md:312` has "`subjectId` the `purchase_orders.id`", and `operations.md:342` covers `safety_records.id` |
| `academics.md:622` counts seven queries in doc 21 §3.5 | Closed | `academics.md:622` now reads "(eight queries ... and 10 ... with `ix_daily_sheets_section_date` and `ix_daily_sheet_entries_sheet`, never cached)" |
| The BR-SCD-007 cancellation notice stays in-app with no mechanism | Closed | `scheduling.md:600` says "`PublishTimetableHandler` writes one command per auto-cancelled booking to the outbox in the publish transaction". `scheduling.md:992` defines TC-SCD-136. Open point 9 is narrowed to the Appendix C row at `scheduling.md:1066` |
| `scheduling.md:314` cites doc 11 §2.3 for the ready notice | Closed | `scheduling.md:314` now reads "consumes `documents.document.generated.v1` on `notification.events.bulk` (`11-messaging-architecture.md` §2.5)" |

| Gap | Axis | Effort to close | Blocking |
|---|---|---|---|
| Feature 42 (mastery and next step) has no data path in the Assessment sheet. No component or mark links to an outcome or standard (`assessment.md:89`, §3.9), and only a per-student route exists (`assessment.md:333`). REQ-ASM-033 and SL-ASM-219 require "per student and class", and document 08:858 hands the class endpoint back to this sheet | Distinctiveness | 2 hours | yes |
| The Assessment sheet has no open point for the feature 42 class view that document 08:858 says belongs to it | Risk honesty | 15 minutes | no |
| The `bff-mobile.md:595` review row still says Appendix N writes `/bff-mobile/` and "needs a brief ADR", which contradicts open point 1 (`bff-mobile.md:584`, closed by ADR-0026) and Appendix N:93 and :129 | Consistency | 5 minutes | no |
| No sheet test asserts heatmap content. Feature 42 relies only on the Appendix W demo test `TC-ASM-811` | Testability | 20 minutes | no |
| The LTI tool-score path (SL-INT-412 and SL-ASM-400) has no platform-notes row (right-to-left tool name in the grid, the phase 4 runner) in the Assessment or Platform sheet | Portability | 15 minutes | no |

#### 5.4 Group D: Web, mobile, security, workflows, design (08, 09, 12, 13, 14)

| Axis | Score | Evidence | What would raise it by one |
|---|---|---|---|
| Completeness | 4 | The round-5 hole is filled: `08-web-structure.md:847` "### 7.10 The screens of features 15, 33, 36 and 42", with endpoints, seven states and slices (`:852-856`), 196 rows recounted (`:802`, verified: 196). What remains is stated as an open point with a default: `08-web-structure.md:978` features "17, 19 and 38". One new thin spot: the class view of feature 42 has no endpoint, and no document owns it. `08-web-structure.md:858` "A class grid would need … a section-level endpoint. That endpoint belongs to the Assessment sheet", but `06-services/assessment.md` has no such open point | Record the section-level standards-heatmap endpoint as an open point, or add it, in `06-services/assessment.md`. Or narrow SL-ASM-219 and Appendix W feature 42 to per-student for now |
| Consistency | 4 | Round-5 drift is fixed: `14-design-system-and-ux.md:16` "a named review step (ADR-0021), not a pipeline check"; `06-services/bff-web.md:594` "Closed 2026-09-26: documents 08 and 12 now use the `/bff/web/v1/` prefix". New drift between editors: `12-security-privacy-safety.md:839` "RISK-54's text still names seven threats and seven tests", also at `:338`, but `18-risk-register.md:129` already names T-SCH-06 and `TC-SEC-132` (eight). `34-work-breakdown.md:603` SL-ASM-219 "a standards heatmap per student and class" disagrees with `08-web-structure.md:858` "The Assessment sheet exposes the heatmap per student only" | Update `12-security-privacy-safety.md:338` and `:839` to say that RISK-54 now carries all eight threats. Make SL-ASM-219's outcome and the Assessment sheet agree on the class heatmap |
| Feasibility | 4 | Every §7.10 call exists in its owning sheet: `06-services/platform.md:328` `/api/v1/platform/demo/reset`; `06-services/documents.md:480-482` memory-books; `06-services/bff-web.md:118` `/bff/web/v1/campus/digital-twin`; `06-services/scheduling.md:365` `POST /room-bookings`; slices SL-PLT-006, SL-ASM-219, SL-DOC-410, SL-OPS-624 and SL-OPS-626 exist in 34. Residual: SL-ASM-219 promises a class heatmap that no endpoint serves (`34-work-breakdown.md:603` against `06-services/assessment.md:333`) | Give the class heatmap an endpoint and a slice owner, or remove "and class" from SL-ASM-219 |
| Risk honesty | 5 | T-SCH-06 is rescored: `12-security-privacy-safety.md:329` "re-verifying both records' guardians after the merge is a person's act"; `:839` "Nine critical-impact child-safety threats keep a `med` residual … eight under RISK-54 … T-WEL-02 … under RISK-24". The top risk is still stated as contested (T-WEL-09 → RISK-47). White-label iOS capacity has a figure with an approver (`09-mobile-structure.md:719`, "A sixth white-label school … is a budget change the product owner approves") | None needed |
| Testability | 4 | The deliver-twice and run-twice rows now have identifiers: `13-workflows-and-sagas.md:185-186` "TC-PLT-783", "TC-PLT-784"; `:246`, `:308`, `:434`, `:493`, `:554`, `:670-671`. Each is registered once in `16-annex-test-case-registry.md` (checked). The worker-killed resume rows still carry method names only: `13-workflows-and-sagas.md:187` "`WorkerKilledMidFanOut_Resumes_NoDuplicates`", `:309` | Give the worker-killed resume scenarios TC identifiers, or say in 13 §1 that those rows are test-method names covered by the deliver-twice TCs |
| Distinctiveness | 4 | The signature screens are now designed to build level. `08-web-structure.md:856` "Rooms and counts only, never a child's name or id (REQ-OPS-016)"; `:854` "the fallback that Appendix W feature 42 degrades to". But feature 42's defining view is reduced: `08-web-structure.md:858` "Appendix W describes feature 42 as a heatmap 'per class' … the web screen shows one student at a time" | Deliver the per-class heatmap that Appendix W describes, once the section-level endpoint exists |
| Portability | 5 | All three round-2 items are closed. `09-mobile-structure.md:224` "Connectivity is not the reason … a quiz is a set of QTI 3 items … the plan builds one QTI renderer"; `:645` "**Trigger, measured:** … more than 10 percent"; `:719` "at most **240 iOS builds** … **6,000 minutes, 480 USD a month**", which agrees with `28-capacity-and-cost-model.md:390` | None needed |

**Verdict:** Approved

| Round-5 gap | Now | Evidence |
|---|---|---|
| Signature features 15, 33, 36 and 42 had no web route or Section 7 row | Closed | `08-web-structure.md:303,315,382` routes; `:652-752` rows; `:847-858` §7.10 design. Endpoints were checked against the Platform, Assessment, Documents, Bff.Web and Scheduling sheets. It raised a new gap: the class heatmap (see below) |
| 14 contradicted itself on the inventory check (`:16` against `:875`) | Closed | `14-design-system-and-ux.md:16` "a named review step (ADR-0021), not a pipeline check"; `:875`, `:918` agree |
| The bff-web and bff-mobile sheets said 08 and 09 still used the old BFF prefix | Closed | `06-services/bff-web.md:569` "documents 08 and 12 use the same prefix", `:594` open point 1 Closed; `06-services/bff-mobile.md:560` "document 09 uses the same prefix" |
| Portability: the quiz reason, the Huawei threshold, the white-label iOS bound | Closed | `09-mobile-structure.md:224` QTI 3 renderer reason; `:645` 10 percent over 30 days; `:719` 5 flavors, 240 builds, 480 USD, which matches `28-capacity-and-cost-model.md:390` |
| 12 open point 10 counted seven; T-WEL-02 left out; T-SCH-06 scored `low` | Closed | `12-security-privacy-safety.md:324-336` nine rows including T-SCH-06 and T-WEL-02; `:839` eight under RISK-54 and one under RISK-24; `18-risk-register.md:129` names T-SCH-06. A stale sentence is left behind (see below) |
| Saga deliver-twice and compensate-twice rows had method names, not TC ids | Closed | `13-workflows-and-sagas.md:185-186,246,308,434,493,554,670-671,737`; each defined once in `16-annex-test-case-registry.md`; `:844` lists them |

| Gap | Axis | Effort to close | Blocking |
|---|---|---|---|
| Feature 42's class heatmap has no endpoint and no owner. SL-ASM-219 promises "per student and class" (`34-work-breakdown.md:603`). 08 says the Assessment sheet holds the gap (`08-web-structure.md:982`), but `06-services/assessment.md` has no such point | Consistency | 1 hour (open point), or 1 day (endpoint) | no |
| 12 says RISK-54 "still names seven threats" (`12-security-privacy-safety.md:338,839`). `18-risk-register.md:129,271` and `00-executive-summary.md:108` already carry eight | Consistency | 15 minutes | no |
| The worker-killed resume scenarios have method names, not TC ids (`13-workflows-and-sagas.md:187,309`) | Testability | 1 hour | no |
| Signature features 17, 19 and 38 still have no web route (open point, default and owner stated, `08-web-structure.md:978`) | Completeness | 1 day | no |

#### 5.5 Group E: Operations, testing, roadmap, risk, dependencies, traceability (15 to 20)

| Axis | Score | Evidence | What would raise it by one |
|---|---|---|---|
| Completeness | 4 | Every PLAN_SPEC item for 15 to 20 is present. The round-5 thin item is filled: `20-traceability-matrix.md:889` REQ-PLAT-004 now reads "Hyper-V and VMware: the appliance images built in the nested-virtualisation job of `release.yml`". AGS is now built and named: `17-roadmap.md:230` "built by SL-INT-412 ... and SL-ASM-400 ... proved by `TC-INT-036`". Minor: the matrix has no writer for a test that twenty documents cite. `16-annex-test-case-registry.md:1870` defines TC-MOB-988 as REQ-MOB-038's test, but `20-traceability-matrix.md:719` gives REQ-MOB-038 only "TC-BFF-761". | Put TC-MOB-988 (the five-device pass) in REQ-MOB-038's Test cell in document 20, so SL-MOB-202 writes it under 16:81. |
| Consistency | 4 | The round-5 drifts are closed: `20-traceability-matrix.md:227` REQ-ACA-030 phase "4 \| SL-ACA-405, SL-INT-412, SL-ASM-400" agrees with `17-roadmap.md:342`, and the phase 1 demo cell names minute 15 (`17-roadmap.md:25`). New minor drift between two generated Group E documents: `16-annex-test-case-registry.md:21` "Derived acceptance tests ... 329" against `20-traceability-matrix.md:26` "327" and `16-test-strategy.md:81` "327". The two extra are TC-MOB-988 and TC-ATT-964. Section 2's own example is wrong: `20-traceability-matrix.md:30` "the acceptance test of REQ-ATT-014 is TC-ATT-964", while `:301` gives it "TC-ATT-004". The "Last updated" lines are stale: `15-deployment-and-operations.md:5` "by the round-4 scorecard remediation" although `:889` records a round-6 amendment. | Make gen-20 and gen-tc-registry count derived tests the same way (resolve TC-MOB-988 and TC-ATT-964), pick an example in 20 Section 2 that no sheet test covers, and refresh the "Last updated" lines of 15, 16 and 18. |
| Feasibility | 5 | The ranges recompute. `17-roadmap.md:230` "three slice-days to phase 4 (212 to 215) ... the low end moves from 6.89 to 6.99 ... high end from 11.02 to 11.18". 215 × 1.3 ÷ 40 = 6.99 and ÷ 25 = 11.18 check out. The margin is stated: "One more phase 4 slice-day would take the low end to 8 weeks". The launch no longer precedes its prerequisite: `34-work-breakdown.md:1059` SL-ACA-405 depends on "SL-INT-411". The totals agree with 00 (`00-executive-summary.md:38` "726 slices and 1,724 slice-days") and 34:42. | None needed. |
| Risk honesty | 4 | RISK-04 now matches 15 open point 6: `18-risk-register.md:80` "run by the mobile engineer's Mac at each phase demo until this question settles". RISK-53 carries the estimate's low confidence and a re-estimate (`18-risk-register.md:148` "the ZATCA estimate of 9 slice-days is of low confidence"), and the bands still add up (`:182` "the four bands add to 62"). Minor: RISK-53 opens with "Open question 9 puts each country's e-invoicing plug-in in phase 5", but the realigned OQ9 default builds a VAT-registered first customer's plug-in in phase 3 (`OPEN_QUESTIONS.md:19` "which the Finance owner's team builds in phase 3"). | Reword RISK-53's opening to OQ9's default: phase 5 generally, and phase 3 for the country of a VAT-registered first customer. |
| Testability | 4 | Every requirement has a test (`20-traceability-matrix.md:25` "Requirements with a test identifier \| 860"), and AGS has a concrete case (`16-annex-test-case-registry.md:867` TC-INT-036 "Given a tool registered with grade access"). Minor: two derived tests in the registry have no slice to write them. `16-annex-test-case-registry.md:1751` TC-ATT-964 and `:1870` TC-MOB-988 are owned "by their requirement", but document 20 does not assign either to a slice, so neither falls under 16:81 "written by the slice that document 20's Slices column names". | Assign both in document 20, or retire TC-ATT-964 and repoint the TC-MOB-988 citations to TC-BFF-761 plus the device pass. |
| Distinctiveness | 4 | Signature features still run in the demo gate: `17-roadmap.md:26` "minute 11 (guardian transparency, Appendix W feature 31) and minute 13 (emergency mode, feature 32) as scripted". Not 5: agreement between the demo minutes in 17 and 16 part 12.2 is still only a review step, not a lint rule (35 rules, none comparing them). | Add a kit-lint rule that the minute sets in 17 Section 1 and in 16's Demo script row match at each phase. |
| Portability | 5 | The PLAT cells name real platforms. `20-traceability-matrix.md:890` "Chromium, Firefox and WebKit under Playwright on ubuntu-latest; real Chrome, Edge, Firefox, Safari and Samsung Internet on the device pass". `:891` "NVDA and Narrator on Windows, VoiceOver on macOS and iOS, TalkBack on Android", and `:892` "Windows 11 kiosk (MSIX ...) and Ubuntu 22.04 or later kiosk". macOS is stated as a partial proof, and the VMware drill stays. | None needed. |

**Verdict:** Approved

| Round-5 gap | Now | Evidence |
|---|---|---|
| 20's Platform column gives REQ-PLAT-004 to 007 the generic PLAT string of CI runners | Closed | `20-traceability-matrix.md:889` to `:892` name Hyper-V/VMware, the browser engines, the screen readers and the kiosk operating systems; the rule is stated at `:42` |
| 17:340 calls the phase 4 launch half of REQ-ACA-030 the default in force, while 20:227 and 34:547 build `LaunchLtiTool` in phase 2 | Closed | `20-traceability-matrix.md:227` phase "4 \| SL-ACA-405, SL-INT-412, SL-ASM-400"; `34-work-breakdown.md:1059` SL-ACA-405 `LaunchLtiTool`; `17-roadmap.md:342` agrees |
| 17:26 "minute 15 as in phase 1", but the phase 1 demo cell does not name minute 15 | Closed | `17-roadmap.md:25` "minute 15 of Appendix O (a report-card template started from the template exchange, Appendix W feature 37)" |
| RISK-04's mitigation (18:80) says the macOS leg falls back to the build host, unlike 15 open point 6 | Closed | `18-risk-register.md:80` "run by the mobile engineer's Mac at each phase demo until this question settles, and moves to the Mac build host only if it settles on one" |
| (Now: Partly closed) round-1 row: 20 Platform column, macOS, VMware drill | Closed | As the first row: the PLAT cells are real; macOS partial proof and VMware drill unchanged |
| (Now: Open) 20's PLAT Platform cells, PLAT-004/005/006 | Closed | `20-traceability-matrix.md:889`, `:890`, `:891` as above |

| Gap | Axis | Effort to close | Blocking |
|---|---|---|---|
| TC-MOB-988 is defined in the registry as REQ-MOB-038's derived test and cited by 20 documents, but 20 gives REQ-MOB-038 only TC-BFF-761, so no slice is charged with writing it | Testability | 20 minutes (a test-plan row or override so gen-20 carries it; regenerate 20 and the annex) | no |
| The registry counts 329 derived acceptance tests and 20 and 16:81 count 327 (TC-MOB-988 and TC-ATT-964 are the difference) | Consistency | 20 minutes (align the two generators' definitions) | no |
| 20 Section 2's example says REQ-ATT-014's acceptance test is TC-ATT-964, while its row gives TC-ATT-004 | Consistency | 5 minutes | no |
| RISK-53 opens with "Open question 9 puts each country's e-invoicing plug-in in phase 5", which is short of the realigned OQ9 default (a VAT-registered first customer's plug-in in phase 3) | Risk honesty | 5 minutes | no |
| The "Last updated" lines of 15, 16 and 18 name rounds 3 and 4 though their change logs record later amendments | Consistency | 5 minutes | no |
| No kit-lint rule checks that the demo minutes in 17 Section 1 match 16 part 12.2 | Distinctiveness | 1 hour | no |

#### 5.6 Group F: Conventions to work breakdown (22 to 29, 31 to 34)

| Axis | Score | Evidence | What would raise it by one |
|---|---|---|---|
| Completeness | 4 | Assignment and Grade Services now has builders: `34:1060` SL-INT-412 "posts each student's score back through LTI Assignment and Grade Services 2.0", `34:1061` SL-ASM-400, and the command in `11-messaging-architecture.md:315`, `platform.md:560`, `assessment.md:471`. Open Question 31 is in 29's question table (`29:59`) and in §3 (`29:173`). Residual issues. The review records of 22, 24, 25, 26 and 27 end at `22:786` "Amended; awaiting the round 5 score", with no round-5 verdict or round-6 row. `34:1390` still says the command and routes "are to be listed in document 11 section 2.5 … which this part does not edit", though they are listed now, in §2.4. | Add the round-5 verdict row to the records of 22 and 24 to 27. Restate `34:1390` as done, citing document 11 §2.4. |
| Consistency | 4 | Round-5 residue closed: `23:742` "SL-FIN-444 builds only the e-invoicing hand-off and the outage queue in phase 3"; `29:37` "depends on Open Questions 3 and 9". A new split on Open Question 9's default: `OPEN_QUESTIONS.md:19` makes the conditional phase 3 build the default, and so does `34:1385` ("the default of Open Question 9 for e-invoicing"). But `23:742` says the plug-ins move to phase 5 "if open question 9 keeps its phase 5 default", and `34:758` says "its default in force puts country implementations in phase 5". Both name the Phase 2 exit review as the decision point, so this is not blocking. Second, `31:91` puts `Nibras.Finance.Domain.Rules.PlanChangeProrationRule` in Finance, while `31:329` says "the rule class is written in the building service" (Platform). | State one Open Question 9 default in 23 open point 7, `34:758` and `OPEN_QUESTIONS.md:19`. Make the Implementation cells of BR-FIN-017 and BR-FIN-018 agree with the `31:329` default. |
| Feasibility | 4 | The ZATCA estimate is qualified: `34:1385` "**Confidence of the ZATCA estimate: low.** … a reasonable upper bound is about twice the estimate … carried by RISK-53". AGS is estimated: `34:1390` "The two slices add 3 slice-days to phase 4". I reran `schedule-34.mjs`: phase 4 still reads "7 to 12", total "61 to 95 weeks", 1,724 days. Residual issues. The ZATCA upper bound (about 18 days) is not in the phase 3 range. 28's prices remain illustrative (RISK-28). | Carry the ZATCA upper bound as a stated phase 3 sensitivity, for example "phase 3 up to N weeks if ZATCA doubles". |
| Risk honesty | 5 | `31:328` "reviewed by the plan editor, not yet by the business-rules-reviewer agent", scored with an owner. `32:324` "that check rests on search snippets only, because every page fetch was refused". `28:507` open point 12 carries the 72.1/72.6 vCPU difference, and the script reports it as carried. | None needed. |
| Testability | 5 | `28:528` "It is performed by the script `tools/plan-build/capacity-28.mjs`". My run printed "160 figures checked, 0 mismatched, 1 carried as open points". `34:766` SL-FIN-452 is "certified against document 23 §8.3 (`TC-INT-037`)", which is defined at `16-annex:868`. AGS is proved by `TC-INT-036` (`platform.md:2365`) and by the deliver-twice tests `assessment.md:1130` TC-ASM-338 and `:1131` TC-ASM-339. | None needed. |
| Distinctiveness | 4 | `32:324` "Fourteen features are unverified in all ten competitors (1, 2, 15, …) down from twenty-four". I recounted the rows (3 yes, 25 partial, 1 no, 14 unverified), and they agree with `32:341`. Snippet sources are labelled (`32:22`, `02:7`). Residual issue: 14 features are still unverified, and the second check is snippet-only. | Resolve PowerSchool, Classter and Classera by trial or full fetch, as `32:324` proposes. |
| Portability | 4 | Unchanged since round 5: `33:254` "reports the container-runtime check as unavailable on that runner rather than passing it"; `33:252` Open Question 14 is pending. The ADR-0026 prefix agrees: `22:43` "`/bff/web/v1/` and `/bff/mobile/v1/`" matches `appendix-n-load-scenarios.md:93` and `:129`. | Settle Open Question 14, or name a Mac host that runs the container leg of TC-PLAT-800. |

**Verdict:** Approved

| Round-5 gap | Now | Evidence |
|---|---|---|
| AGS in scope (`23:504`) but no slice, use case or route; REQ-ACA-030 cannot pass | Closed | `34:1060` SL-INT-412, `34:1061` SL-ASM-400, `23:504` "built by SL-INT-412 … and SL-ASM-400 … proved by `TC-INT-036`"; `11:315`, `platform.md:560`, `assessment.md:145` |
| `23:742` open point 7 described the old SL-FIN-444 and put the other country at phase 5 | Closed | `23:742` "SL-FIN-444 builds only the e-invoicing hand-off … Both sets are counted in the phase 3 range". This introduced the Open Question 9 default split noted under Consistency |
| Open Question 31 absent from 29 §1 and §3 | Closed | `29:59` "31 Kindergarten daily-sheet permission"; `29:173` in decisions to come |
| ZATCA 9 slice-days with no confidence note | Closed | `34:1385` "Confidence of the ZATCA estimate: low … carried by RISK-53" |
| TC-PERF-800 manual review step | Closed | `28:528` performed by `capacity-28.mjs`; I ran it: 160 checked, 0 mismatched |
| SL-FIN-452 names no test id | Closed | `34:766` "(`TC-INT-037`)"; `16-annex-test-case-registry.md:868` |
| Property-based classification review not done | Partly closed | `31:158` "reviewed rule by rule … 17 below were corrected, which gives 55" (I recounted 55 yes). The business-rules-reviewer agent pass is still pending (`31:328`) |
| `29:37` tied ADR-0025 to Open Question 3 only | Closed | `29:37` "depends on Open Questions 3 and 9 for the conditional e-invoicing slices" |
| Rules built before their owning service had no footnote | Closed | `31:145` to `31:151` note and table. Residue: the implementation cells at `31:90` and `31:91` disagree with the `31:329` default |
| Twenty-four features unverified in all ten (RISK-31) | Partly closed | `32:324` "Fourteen features … down from twenty-four", snippet-only |
| (Portability "Now" residue) macOS container leg rests on Open Question 14 | Open | `33:254` unchanged; stated honestly with a default |

| Gap | Axis | Effort to close | Blocking |
|---|---|---|---|
| Open Question 9's default is stated two ways. `OPEN_QUESTIONS.md:19` and `34:1385` make the conditional phase 3 build the default; `23:742` and `34:758` say the default keeps the plug-ins in phase 5 | Consistency | 15 minutes | no |
| BR-FIN-017 and BR-FIN-018 implementation cells name `Nibras.Finance.Domain.Rules.*` (`31:90`, `31:91`), while `31:329` says the rule class is written in Platform, the building service | Consistency | 15 minutes (gen-31 review list) | no |
| `34:1390` says the `RecordToolScore` command and routes "are to be listed in document 11 section 2.5 … which this part does not edit"; they are already listed, in §2.4 | Consistency, Completeness | 5 minutes | no |
| Review records of 22, 24, 25, 26 and 27 stop at "awaiting the round 5 score" (for example `22:786`), with no round-5 verdict | Completeness | 10 minutes | no |
| The ZATCA upper bound (about twice, `34:1385`) is not shown as a phase 3 range sensitivity | Feasibility | 15 minutes | no |
| The property-based classification still awaits the business-rules-reviewer agent (`31:328`) | Risk honesty | 1 hour | no |
| Fourteen signature features unverified in all ten competitors, and the second check is snippet-only (`32:324`) | Distinctiveness | 2 days | no |
| The macOS container leg of TC-PLAT-800 is unavailable on the hosted runner until Open Question 14 is decided (`33:254`) | Portability | product-owner decision | no |

### 6. Round 1 findings re-checked by hand

The five most consequential round-1 findings were checked by hand before the first remediation, and all were real. Each is closed: the API count (document 02 now says six of ten), the Gateway's read of the revoked-user mark (document 21 Section 2.3), k6 (master brief Section 6.4 and `allow.json`), the phase 1 range (document 17 Section 1) and the AI index name (`ai_index.embedding_chunk` everywhere).

## Decisions in force

| Decision | Record |
|---|---|
| Approve only at 4 or better on every axis | `PLAN_SPEC.md`, "The scorecard" |
| Scores need quoted evidence; a score without evidence is not a score | `.claude/commands/score-plan.md` |
| Every round is kept; a later round checks every earlier gap | This document |

## Dependencies on other documents

| Document | What this one takes |
|---|---|
| `PLAN_SPEC.md` | The rubric and the approval rule |
| Every plan document in Groups A to F | The evidence |
| `docs/project/CHANGELOG.md`, ADR-0019 to ADR-0024 | What each remediation changed |

## Open points

| Point | Default | Owner | L | I | Score | In the register |
|---|---|---|---|---|---|---|
| The product-owner decisions the plan states as open (Open Questions 27 to 30, 3, 14, 26) and the Proposed ADRs | The defaults in force until decided; document 00 lists them | Product owner | 3 | 4 | 12 | RISK-44 |

## Review record

| Date | Reviewer | Result |
|---|---|---|
| 2026-09-22 | Round 1: six independent adversarial reviewers, one per group | 0 of 6 groups approved; lowest axis score 2 |
| 2026-09-26 | Round 2: six independent adversarial reviewers, one per group | 0 of 6 groups approved; lowest axis score 3 |
| 2026-09-26 | Round 3: six independent adversarial reviewers, one per group | 3 of 6 groups approved (A, B, D); lowest axis score 3 |
| 2026-09-26 | Round 4: six independent adversarial reviewers, one per group | 4 of 6 groups approved (A, B, D, E); lowest axis score 3 |
| 2026-09-26 | Round 5: six independent adversarial reviewers, one per group | All groups approved; lowest axis score 4 |
| 2026-09-26 | Round 6: six independent adversarial reviewers, one per group | 5 of 6 groups approved (A, B, D, E, F); lowest axis score 3 |

## How this document is verified

| Claim | Proof |
|---|---|
| Every score has evidence | Each row of Section 5 quotes a document with file and line; a row without one is invalid |
| The summary and history tables match the group scores | Computed from the group scorecards of every round by `build-30.cjs`, not typed |
| The verdict follows the rule | A group with any axis below 4 is shown blocked; the script applies the rule |
| The document is current | Kit-lint rule R23 reruns `build-30.cjs --check` and fails when a group scorecard changed since this document was built |
