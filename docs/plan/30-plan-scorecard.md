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

### 2. Summary, round 7 (2026-09-26)

| Group | Completeness | Consistency | Feasibility | Risk honesty | Testability | Distinctiveness | Portability | Lowest | Verdict |
|---|---|---|---|---|---|---|---|---|---|
| A | 4 | 4 | 5 | 4 | 4 | 4 | 5 | 4 | Approved |
| B | 4 | 4 | 5 | 4 | 4 | 4 | 5 | 4 | Approved |
| C | 4 | 4 | 4 | 4 | 4 | 4 | 4 | 4 | Approved |
| D | 4 | 4 | 5 | 5 | 5 | 4 | 5 | 4 | Approved |
| E | 5 | 4 | 5 | 5 | 4 | 4 | 5 | 4 | Approved |
| F | 4 | 4 | 5 | 5 | 5 | 4 | 4 | 4 | Approved |
| **Lowest per axis** | **4** | **4** | **4** | **4** | **4** | **4** | **4** | | |

**Verdict: every group is approved at 4 or better on every axis.** The plan is ready for the product owner's approval, subject to the decisions listed in document 00.

### 3. Score history

Each cell reads round 1 → round 2 → round 7. A group moves to approved only when every axis reaches 4.

| Group | Completeness | Consistency | Feasibility | Risk honesty | Testability | Distinctiveness | Portability |
|---|---|---|---|---|---|---|---|
| A | 3 → 4 → 4 → 4 → 4 → 4 → 4 | 2 → 3 → 4 → 4 → 4 → 4 → 4 | 3 → 3 → 4 → 4 → 4 → 4 → 5 | 3 → 3 → 4 → 4 → 4 → 5 → 4 | 2 → 3 → 4 → 4 → 4 → 4 → 4 | 3 → 3 → 4 → 4 → 4 → 4 → 4 | 4 → 4 → 4 → 4 → 4 → 5 → 5 |
| B | 3 → 4 → 4 → 4 → 4 → 4 → 4 | 2 → 3 → 4 → 4 → 4 → 4 → 4 | 3 → 4 → 4 → 4 → 4 → 4 → 5 | 3 → 4 → 4 → 4 → 4 → 4 → 4 | 3 → 3 → 4 → 4 → 4 → 4 → 4 | 3 → 3 → 4 → 4 → 4 → 4 → 4 | 4 → 4 → 4 → 4 → 4 → 5 → 5 |
| C | 4 → 3 → 3 → 3 → 4 → 4 → 4 | 3 → 3 → 3 → 3 → 4 → 4 → 4 | 4 → 4 → 4 → 4 → 4 → 4 → 4 | 3 → 4 → 4 → 4 → 4 → 4 → 4 | 4 → 3 → 4 → 4 → 4 → 4 → 4 | 3 → 3 → 4 → 4 → 4 → 3 → 4 | 3 → 3 → 4 → 4 → 4 → 4 → 4 |
| D | 4 → 4 → 4 → 4 → 4 → 4 → 4 | 2 → 3 → 4 → 4 → 4 → 4 → 4 | 3 → 4 → 4 → 4 → 4 → 4 → 5 | 3 → 3 → 4 → 4 → 4 → 5 → 5 | 3 → 4 → 4 → 4 → 4 → 4 → 5 | 4 → 4 → 4 → 4 → 4 → 4 → 4 | 4 → 4 → 4 → 4 → 4 → 5 → 5 |
| E | 3 → 4 → 4 → 4 → 4 → 4 → 5 | 2 → 3 → 3 → 4 → 4 → 4 → 4 | 2 → 3 → 4 → 4 → 4 → 5 → 5 | 3 → 3 → 4 → 4 → 4 → 4 → 5 | 3 → 4 → 4 → 4 → 4 → 4 → 4 | 3 → 4 → 4 → 4 → 4 → 4 → 4 | 4 → 4 → 4 → 4 → 4 → 5 → 5 |
| F | 3 → 3 → 3 → 4 → 4 → 4 → 4 | 2 → 3 → 4 → 3 → 4 → 4 → 4 | 3 → 4 → 4 → 4 → 4 → 4 → 5 | 3 → 3 → 4 → 4 → 4 → 5 → 5 | 3 → 3 → 4 → 4 → 4 → 5 → 5 | 3 → 4 → 4 → 4 → 4 → 4 → 4 | 4 → 4 → 4 → 4 → 4 → 4 → 4 |

| Round | Date | Lowest axis score | Axes below 4 | Groups approved |
|---|---|---|---|---|
| 1 | 2026-09-22 | 2 | 32 of 42 | 0 of 6 |
| 2 | 2026-09-26 | 3 | 22 of 42 | 0 of 6 |
| 3 | 2026-09-26 | 3 | 4 of 42 | 3 of 6 |
| 4 | 2026-09-26 | 3 | 3 of 42 | 4 of 6 |
| 5 | 2026-09-26 | 4 | 0 of 42 | 6 of 6 |
| 6 | 2026-09-26 | 3 | 1 of 42 | 5 of 6 |
| 7 | 2026-09-26 | 4 | 0 of 42 | 6 of 6 |

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
| After round 5 | Round-5 gaps, all groups | LTI Assignment and Grade Services built (SL-INT-412, SL-ASM-400, `RecordToolScore`); `capacity-28.mjs` recomputes document 28; document 02 re-checked (capabilities unverified in all ten competitors 24 to 14); web routes for features 15, 33, 36 and 42; saga resume and deliver-twice test ids; RISK-54 carries eight residuals | ADR-0026; brief v9.6 |
| After round 6 | Round-6 gaps; Open Question 30 | The feature 42 class mastery heatmap and its data path in Assessment; derived-test counts aligned between documents 16 and 20; the product owner approved the plan and answered Open Question 30: the billable count is master brief Section 36, owned by Platform; RISK-52 closed | ADR-0027 (Accepted); brief v9.7 |

### 5. Scores by group, round 7

#### 5.1 Group A: Understanding (01, 02; 00 last)

| Axis | Score | Evidence | What would raise it by one |
|---|---|---|---|
| Completeness | 4 | Every element PLAN_SPEC asks of 00 is present: modes (`00:44`), the application count (`00:54` "20 data-owning services, 16 Tier 1 and 4 Tier 2"), the roadmap with the MVP (`00:69`, `00:81`), the top ten (`00:99`), the decisions (`00:118`) and the verdict (`00:163`). 01 holds all thirty open questions, five in 1, nine in 2, eight in 3, four in 4 and five in 5, the same set as `OPEN_QUESTIONS.md:11-40`. 02 is complete with both checks. There is one stale section. Section 7 of 00 is current only to round 5 (`00:167` "Verdict after round 5"), but `30:21` holds the round-6 summary. The source list at `00:3` omits documents 08 and 33, which `00:171` and `00:46` cite. | Bring 00 Section 7 up to the latest round that document 30 records, and add 08 and 33 to the list at `00:3`. |
| Consistency | 4 | I checked every figure 00 quotes against its owning document, and all of them match. The requirement counts 860/758/94/8 match `03:53`. The 79/726/1724 and 423/402/215 figures match `34:37-43`, and 858 matches `34:1344`. The costs USD 195.07/113.72/50.50 and the 94% match `28:372-374`. The top ten, the 29 risks at 12 and the five owned by the product owner match `18:178-219`. The ADR counts, 25 Proposed and 2 Accepted, match `29:41`. The ranges 61–95/71–110 and 42/954/33–50 match `17:38` and `17:252`, and the L/I/Score values match 01. The ADR-0027 wording is consistent across 00, 01, `OPEN_QUESTIONS.md:68-72`, `31:90` and `34:225`, and I found no Finance count job and no claim that question 30 is still open. Three drifts remain. (1) `00:167` quotes document 30: "every group is approved at 4 or better on every axis. The plan is ready…". That sentence is no longer in 30, and `30:33` says "5 of 6 groups approved". `00:169` "It took five rounds" contradicts `30:55` (round 6: "5 of 6"). (2) `01:96` says "ADR-0019 is the one record the product owner has Accepted". That is stale after ADR-0027, and `01:7` and `29:41` both say two. (3) The dependency row at `00:203` says 30 was "checked 2026-09-26", but it was not rechecked after round 6. | Restate `00:167-169` from `30:21-55` without the vanished quote. In `01:96`, write "ADR-0019 was the first record the product owner Accepted" (ADR-0027 is the second). |
| Feasibility | 5 | I recomputed every sizing against 34 with the formula stated in 01. Phase 4 is now correct: `01:42` "against Phase 4's 215". Wellbeing 61, Hr 62 and Ops 72 give 2.0/3.2 and 2.3/3.7 weeks. CAP-ACA-03 has 5 slices and 10 slice-days, which gives 0.3/0.5. CAP-INT-01 has 17 of 27 slice-days, which gives 0.6/0.9. SL-INF-608/609 are 5 slice-days (0.2/0.3), and CAP-INF-05 is 18. SL-IDN-011 is 3, SL-PLT-010 is 3 (`01:111`), and SL-FIN-448–452 are 9+5. Every unknown has an owner and a trigger (`01:27` "decided at the Phase 2 exit review"). | None needed. |
| Risk honesty | 4 | Every question and open point carries L, I, a score and a register link. `00:118` says "L, I and Score are quoted from document 01". The contradictions are stated plainly, for example `01:48` "**Until the question is decided this default contradicts the rule**", and RISK-52's closure is stated with its tie-break successor (`00:112`). New optimism: 00 presents the round-5 verdict as the verdict, and `00:171` says only that "the rounds after 5 closed the most material" gaps. It does not say that round 6 blocked Group C on Distinctiveness (`30:27`). The approver's document is therefore rosier than the scorecard it summarises. | State the latest round's verdict per group in 00 Section 7, including any group that round blocked, and what closed it. |
| Testability | 4 | Every row of "Where Nibras is different" cites registry-defined tests. `02:95` now reads "`TC-PLAT-013` (document 33 …) and `TC-PLAT-800` (document 33, its macOS leg)". I checked all 20 TCs cited in 00 to 02, and each appears exactly once in `16-annex-test-case-registry.md`. Residue: 00's central claim is proved only by a manual step (`00:228` "Every quoted number matches its owning document \| The plan review of `REVIEW_GUIDE.md`"). That step missed the stale verdict quote at `00:167`. | Have kit-lint (or a `--check` generator) compare 00's quoted figures and quotations with the owning documents, as `capacity-28.mjs` already does for 28. |
| Distinctiveness | 4 | The last "Confirmed" standing based on an absence has been relabelled: `02:88` "**Unmatched on the check date as a difference in depth,** … absence on a vendor page is not proof, so it is in the re-check". The proofs lead with what competitors lack (`02:161` "the rule version, the guardian's view, and reunification with verified pickup"). The limit is inherent and stated: the second check rests on snippets alone (`02:7` "All seven fetches were refused"), and 117 of 180 cells are still unverified (`02:58`). | Fetch sources 32 to 54 in full and run the scheduled trial re-check, so that "unmatched" rests on more than snippets. |
| Portability | 5 | The no-Google build is described the same way in both places: `01:31` "a separate APK built from the same source with the no-Google flavour flag" on "the ubuntu runner", and `02:50` the same. The runners are named: `01:136` "`ubuntu-latest`, `windows-latest` and `macos-latest` runners". The developer mode has an OS-specific proof: `02:95` "`TC-PLAT-013` … on the ubuntu and windows legs … `TC-PLAT-800` … its macOS leg". The Windows server path is covered at `00:47` "runs the Linux virtual machine appliance for Hyper-V or VMware". | None needed. |

**Verdict:** Approved

| Round-6 gap | Now | Evidence |
|---|---|---|
| `00:172` said LTI AGS had "no slice" | Closed | `00:171` "is now built in phase 4 by SL-INT-412 and SL-ASM-400, so REQ-ACA-030's grade return has a slice" |
| `00:162` "workshop of about an hour", with 27/28/31 in it | Closed | `00:161` "one 90-minute decisions session … and written answers for 27, 28 and 31"; matches `REVIEW_GUIDE.md:21`, `:33` |
| `01:5` "thirty that REVIEW_GUIDE counts" vs 31; written list omitted 31 | Closed | `01:5` "Thirty questions are open, the count `REVIEW_GUIDE.md` gives"; `REVIEW_GUIDE.md:17` "**30 open questions**"; `:33` lists 31 among the seven written |
| `01:43` "Phase 4's 210" vs 215 | Closed | `01:42` "against Phase 4's 215"; `34:37` phase 4 = 215 |
| `02:88` "Confirmed as a difference in depth" rested on absence | Closed | `02:88` "**Unmatched on the check date as a difference in depth,** … so it is in the re-check" |
| `02:95` developer mode proved by `dev-smoke.yml` with no TC | Closed | `02:95` "`TC-PLAT-013` (document 33 …) and `TC-PLAT-800` (document 33, its macOS leg)"; both in registry at `16-annex:1069`, `:1076` |
| `00:3` source list omitted 01, 32, Appendix L | Closed (minor residue) | `00:3` "it quotes Appendix L and documents 01, 03, 05, 15, 17, 18, 28, 29, 30, 32 and 34"; 08 and 33, cited at `00:171` and `00:46`, are still missing |

| Gap | Axis | Effort to close | Blocking |
|---|---|---|---|
| `00:167-169` presents the round-5 verdict with a quote that is no longer in document 30 ("every group is approved … ready for the product owner's approval"). It also says "It took five rounds", but `30:21-33`, `:55` and `:310` record round 6 (5 of 6 approved, C blocked on Distinctiveness) | Consistency, Risk honesty, Completeness | 0.25 hour | no |
| `01:96` "ADR-0019 is the one record the product owner has Accepted", which is stale since ADR-0027. `01:7`, `00:131` and `29:41` all say two are Accepted | Consistency | 5 minutes | no |
| `00:228` checks quoted numbers only by a manual review step, which missed the gap above. There is no tooling check of 00 against its owners | Testability | 2 hours | no |
| `00:203` dependency row "The scorecard verdict \| `30-plan-scorecard.md` \| 2026-09-26" was not re-checked after round 6 | Consistency | 5 minutes | no |
| `00:3` source list omits 08 (open point 4, `00:171`) and 33 (`00:46`) | Completeness | 5 minutes | no |
| The second check's 23 sources are snippets only, and 117 of 180 cells remain unverified (`02:58`, `02:195`) | Distinctiveness | 6.5 person-days (scheduled, RISK-31) | no |

#### 5.2 Group B: Requirements and architecture (03, 04, 05, 07)

| Axis | Score | Evidence | What would raise it by one |
|---|---|---|---|
| Completeness | 4 | The round-6 gap is closed: `04:561` "ADR-0026 \| Appendix N names the backend-for-frontend routes" and `04:633` "Section 8 covers ADR-0001 to ADR-0026". ADR-0027 is new and missing, although `04:531` says every decision about how the plan is built "has an ADR". Section 8 has no row for ADR-0027 (Accepted; it moves the billing count to Platform), and `04:633` stops at ADR-0026. | Add an ADR-0027 row to 04 Section 8 (Platform computes BR-FIN-017, with the alternative "Finance keeps the old one-day count"), and make `04:633` read "ADR-0001 to ADR-0027". |
| Consistency | 4 | Two new drifts come from ADR-0027. (1) `04:532` "ADR-0019 is Accepted ... every other record below is Proposed" and `04:614` "Twenty-five of the twenty-six ADRs ... only ADR-0019 is Accepted" disagree with `29:41` "Twenty-seven records: twenty-five Proposed ... two Accepted (0019 and 0027)". (2) Platform's Consumes cell at `05:19` has no `school.student.*` event, and `05:77` leaves Platform out of School's consumers. Yet `11:1154` says it "adds Platform to the consumers of `school.student.enrolled.v1`" for the billing count. Neither drift makes a builder build the wrong thing, because the Platform sheet and document 11 are correct. | Update `04:532`, `04:614` and `04:633` to twenty-seven records with two Accepted (0019, 0027). Add `school.student.enrolled.v1` and `school.student.status-changed.v1` to Platform's Consumes cell in 05. Add Platform to the School row at `05:77`, and classify the School and Platform pair in 5.2 as a reference-copy feed. |
| Feasibility | 5 | The round-6 gap is closed: `07:29` "The team is master brief Section 29's, 'five to eight builders' ... phase 1 ... is 14 to 22 weeks ... '61 to 95 weeks'". This matches `17:25` and `17:38`. The 206 projects are template-generated, "so no builder writes the 206 project files by hand" (`07:29`). Unknowns are owned (`05:258`, `04:613`). | None needed. |
| Risk honesty | 4 | RISK-52 is now consistent: `03:1119` "The conflict is closed, and RISK-52 with it", and `18:147` and `00:112` say it is Closed. The Open points table keeps L, I, score and owner (`03:1115-1120`). One risk row is stale: `04:614` understates what is Accepted ("only ADR-0019 is Accepted"), so the score it carries rests on a count that is out of date. | Correct `04:614` to "twenty-five of the twenty-seven ... ADR-0019 and ADR-0027 Accepted" (the same edit as Consistency). |
| Testability | 4 | The round-6 overclaim is fixed. `05:270` "`TC-TST-740` ... is the static half of the one-hop rule ... The runtime proof of the one-hop rule is `TC-API-072`". `07:195` "checks the static half ... the runtime proof of the rule is `TC-API-072`". The billing count has a test: `03:1119` "`TC-PLT-103` asserts the count", which the Platform sheet defines at `platform.md:2385`. Many rows still hold a Given/When/Then criterion with no TC identifier (for example `03:134`, `03:136`). | Add the Platform-sheet TC identifiers (`TC-PLT-103`, `TC-PLT-128`) to the acceptance cells of REQ-PLT-009 and REQ-PLT-010, or cite them in their Dependencies. |
| Distinctiveness | 4 | The round-6 gap is closed: `04:613` "every feature above rung 1 names its rung 1 fallback in the 'Fallback to rung 1' column of `25-ai-and-assist-ladder.md` section 2", and that column exists (`25:77`). The trace is held once, in document 32 (`04:568`). The group itself shows no signature-feature moment or proof. | Say in 04 which deployment mode or hardware each rung 2 or rung 3 signature feature needs, so the ladder links to the topology. |
| Portability | 5 | `04:63` "Proved in the pipeline by `TC-NOT-610` ... `TC-MOB-988` ... on one device without Google services"; `16:451` lists "one device without Google services". REQ-PLAT-019 to 023 (`03:1105-1109`) pin culture, paths and dual wrappers. No regression since round 6. | None needed. |

**Verdict:** Approved

| Round-6 gap | Now | Evidence |
|---|---|---|
| 04 Section 8 has no ADR-0026 row; `04:613`/`04:632` counts stop at twenty-five | Closed | `04:561` has the ADR-0026 row; `04:614` "Twenty-five of the twenty-six"; `04:633` "ADR-0001 to ADR-0026". ADR-0027 then left these lines stale again (new gap below) |
| `05:270` and `07:193` claim `TC-TST-740`/`GrpcHopRules` enforce the one-hop rule | Closed | `05:270` "is the static half ... The runtime proof of the one-hop rule is `TC-API-072`"; `07:195` "checks the static half of that rule" |
| RISK-52 due date "Group B approval" in 18 and 00 | Closed | `18:272` "due before SL-PLT-010 starts ... its trigger reads the same". The risk was then Closed by ADR-0027 (`18:147`, `00:112`, `03:1119`) |
| No team size or duration beside the 206-project count | Closed | `07:29` "five to eight builders ... '61 to 95 weeks'", matching `17:38` |
| Per-feature rung-1 fallback not stated or cited | Closed | `04:613` cites the "Fallback to rung 1" column of `25-ai-and-assist-ladder.md` section 2 (`25:77`) |

| Gap | Axis | Effort to close | Blocking |
|---|---|---|---|
| ADR-0027 not reflected in 04. `04:532` and `04:614` say only ADR-0019 is Accepted, `04:633` covers only up to ADR-0026, and Section 8 has no ADR-0027 row. `29:41` says "two Accepted (0019 and 0027)" | Consistency, Completeness | 20 minutes | no |
| 05 Platform row's Consumes cell (`05:19`) and School's consumer list (`05:77`) leave out the `school.student.enrolled.v1` and `status-changed.v1` subscription that ADR-0027 added (`11:371`, `11:1154`). The School and Platform cycle in 5.2 is still classified only as broadcast and telemetry | Consistency | 30 minutes | no |
| REQ-PLT-009 and REQ-PLT-010 (`03:135-136`) carry Given/When/Then criteria with no TC identifier, although `TC-PLT-103` and `TC-PLT-128` exist | Testability | 10 minutes | no |
| 04 does not link signature-feature rungs 2 and 3 to deployment modes and hardware | Distinctiveness | 45 minutes | no |

#### 5.3 Group C: Services, data, messaging, performance (06, 10, 11, 21)

| Axis | Score | Evidence | What would raise it by one |
|---|---|---|---|
| Completeness | 4 | The feature 42 data path is now in the sheet. `assessment.md:91` says "`component_outcomes` ... the evidence link of Appendix W feature 42", and `assessment.md:351` adds "`/api/v1/assessment/sections/{id}/standards-heatmap`". ADR-0027 is carried into Platform: `platform.md:780` "`ref_student_enrolment_periods` (phase 1, every tenant)". Two residues remain. The replication map `10-data-architecture.md:288` lists only "Platform \| Usage counters" and has no enrolment-period copy. `assessment.md:687` admits "document 21 §3.6 has no feature 42 query yet" | Add the Platform `ref_student_enrolment_periods` row to doc 10 §6 and the feature 42 query and index to doc 21 §3.6 |
| Consistency | 4 | ADR-0027 is applied without stale text. `finance.md:61` says "Platform also computes the billable active-student count (BR-FIN-017)", and `finance.md:1697` records "Closed 2026-09-26 by ADR-0027". `11-messaging-architecture.md:597` declares the Platform binding "ADR-0027 (Platform computes BR-FIN-017 from its own copy)". No group document still calls Open Question 30 open. New minor drift: `platform.md:2335` says "the nine rule test classes", but `platform.md:2358` lists eleven, including `ActiveStudentCountRulesTests`. `assessment.md:360` says teachers hold `academics.curriculum.view` under G07, while `academics.md:568` gives it only to "Coordinator, head of department" | Change "nine" to "eleven" in `platform.md:2335`. Add teachers (own-sections) to the `academics.md:568` `curriculum.view` row, or state the Appendix I G07 grant there |
| Feasibility | 4 | The billing count can now be built from Platform's own data. `platform.md:794` "`TenantBillingRunJob` ... computes the active-student count with `ActiveStudentCountRule` ... from `ref_student_enrolment_periods`", fed by `11-messaging-architecture.md:371` `platform.reference-copies`. Feature 42 has bounded inputs: `assessment.md:687` "2 commands ... p95 80 ms". The model inputs are listed at `assessment.md:359` | Put the feature 42 query in doc 21 so its budget is load-tested with the rest |
| Risk honesty | 4 | The remaining feature 42 unknowns are scored and owned. `assessment.md:1257` "Appendix E has no Academics curriculum event ... the mastery rule has no Appendix S identifier \| ... \| 2 \| 2 \| 4". The billing risk is updated: `platform.md:2450` "BR-FIN-017 computed only by Platform (ADR-0027) ... \| RISK-52 (closed by ADR-0027)" | Obtain the BR identifier and the a proposed Academics outcome-changed event (not yet in Appendix E) decision through an ADR, or register open point 8 in document 18 |
| Testability | 4 | Heatmap values now have sheet tests. `assessment.md:1175` TC-ASM-340 works through "O1 `percent` 68.000000 (17 of 25)", and `assessment.md:1177` TC-ASM-342 checks "`meanPercent` 67.666667 and `levelCounts` [1, 1, 0, 1]". Both are arithmetically correct. The billing tests `platform.md:2408-2412` (TC-PLT-126 to 130) all check out. One expected value is wrong: `assessment.md:1176` "After the teacher submits A with 18, O1 reads 76.000000". By the §4.5 formula this is (18 + 4×0.5) ÷ (20 + 10×0.5) = 80.000000 | Correct TC-ASM-341 to 80.000000 (level 3 is unchanged) |
| Distinctiveness | 4 | Feature 42 can now be built better than a plain grade grid. `assessment.md:353` "`GetStandardsHeatmapHandler` computes the heatmap on read, per student and per outcome". The rung 2 ranker has five named inputs and a raw-heatmap fallback (`assessment.md:359`). `08-web-structure.md:858` "the first render is two calls ... no per-student fan-out remains". What is still missing: the mastery rule has no BR identifier (`assessment.md:1257`) | Give the mastery computation an Appendix S rule and a named rule test class |
| Portability | 4 | The LTI path now has platform notes in both sheets with runners. `assessment.md:1195` "`ci-web.yml` Playwright on Chromium, Firefox and WebKit in the four theme and direction combinations", backed by TC-ASM-345. `platform.md:2428` covers "invariant culture, and `timestamp` as an ISO 8601 instant compared in UTC". `08-web-structure.md:858` "columns mirror in right to left, and outcome codes are bidi-isolated" | Add a TC-ASM id that asserts the heatmap's right-to-left column mirroring and its bidi-isolated outcome codes. Today only the doc 08 prose covers it |

**Verdict:** Approved

| Round-6 gap | Now | Evidence |
|---|---|---|
| Feature 42 has no data path in the Assessment sheet (no component-to-outcome link, no class route) | Closed | `assessment.md:91` `component_outcomes`, `assessment.md:301` "each component with its `outcomes[]`", `assessment.md:351` the section route, the §4.5 computation at 353-360, and the query row at 687 |
| Assessment has no open point for the feature 42 class view | Closed | `assessment.md:1257` open point 8 is scored 2/2/4 and owned by "Assessment lead, with the Academics lead for the event". `08-web-structure.md:982` records the closure |
| `bff-mobile.md:595` review row contradicts open point 1 | Closed | `bff-mobile.md:596` "The row above is superseded. Open point 1 is closed by ADR-0026 (brief v9.6) ... No further brief ADR is needed" |
| No sheet test asserts heatmap content | Closed (one wrong expected value) | TC-ASM-340 to 344 at `assessment.md:1175-1179`. TC-ASM-341's 76.000000 should read 80.000000 |
| LTI tool-score path has no platform-notes row in Assessment or Platform | Closed | `assessment.md:1195` with TC-ASM-345 at 1180, and `platform.md:2428` with the review row at 2514 |

| Gap | Axis | Effort to close | Blocking |
|---|---|---|---|
| TC-ASM-341 expects O1 = 76.000000 after A is submitted with 18. The §4.5 formula gives (18 + 2) ÷ 25 = 80.000000 (`assessment.md:1176`) | Testability | 5 minutes | no |
| Doc 10 §6 replication map has no Platform `ref_student_enrolment_periods` row, although doc 11 §2.6 cites doc 10 §6 for it (`10-data-architecture.md:288`, `11-messaging-architecture.md:597`) | Completeness | 10 minutes | no |
| Doc 21 §3.6 has no feature 42 section-heatmap query or `ix_component_outcomes_section`, as `assessment.md:687` admits | Completeness | 15 minutes | no |
| `platform.md:2335` says "nine rule test classes", but `platform.md:2358` lists eleven after ADR-0027 | Consistency | 2 minutes | no |
| `academics.md:568` gives `academics.curriculum.view` only to the coordinator and head of department. The feature 42 labels depend on teachers holding it (`assessment.md:360`), so the Academics table would leave teachers with unlabeled columns | Consistency | 5 minutes | no |
| The mastery rule has no Appendix S identifier, and there is no Academics outcome event (open point 8, `assessment.md:1257`) | Distinctiveness | ADR, 1 hour | no |

#### 5.4 Group D: Web, mobile, security, workflows, design (08, 09, 12, 13, 14)

| Axis | Score | Evidence | What would raise it by one |
|---|---|---|---|
| Completeness | 4 | Feature 42 now has its class view, built from the owning sheet: `08-web-structure.md:854` "`GET /api/v1/assessment/sections/{id}/standards-heatmap?subjectId=&gradingPeriodId=` (Assessment sheet §4.5 …)". Section 7 recounts to 196 rows (`:802`, verified: 196). One gap is still open, with a default and an owner: `08-web-structure.md:978` "Where do the web screens of signature features 17, 19 and 38 live". Feature 19 is also gated on Open Question 31 | Give features 17, 19 and 38 routes in Section 2 and Section 7 rows, or state which of them is mobile-only. For 19, add it once OQ 31 is answered |
| Consistency | 4 | The round-6 drift is gone. `12-security-privacy-safety.md:338` "its text names all eight threats, including T-SCH-06", which matches `18-risk-register.md:129` and `:198`. `34-work-breakdown.md:603` "per student and class" now matches `06-services/assessment.md:351`. No stale ADR-0027 or OQ-30 text in 08, 09, 12, 13 or 14 (grep found no match for RISK-52, BR-FIN-017/018, OQ 30 or the billing count). Minor paraphrase drift: `12-security-privacy-safety.md:836` "SOC 2 or ISO 27001", but `OPEN_QUESTIONS.md:36` "SOC 2 or 1EdTech". Also, `34-work-breakdown.md:603` names "GetResultAnalysis (heatmap, predicted)", while `assessment.md:353` names `GetStandardsHeatmapHandler` | Quote OQ 26 as `OPEN_QUESTIONS.md` words it. Make SL-ASM-219's deliverable name `GetStandardsHeatmap` |
| Feasibility | 5 | The class heatmap now has an endpoint, an index and a budget: `06-services/assessment.md:687` "2 commands … p95 80 ms; the ranker adds under 10 ms". `08-web-structure.md:858` "the first render is two calls … within Section 9, and no per-student fan-out remains". What is left open has an owner and a default (`assessment.md:1257`: no outcome event, a teacher without `academics.curriculum.view` sees ids) | None needed |
| Risk honesty | 5 | `12-security-privacy-safety.md:839` "Nine critical-impact child-safety threats keep a `med` residual … eight under RISK-54 … T-WEL-02 … under RISK-24", with likelihood 3, impact 5, score 15 and the product owner as owner. T-WEL-09 and RISK-47 are still called contested (`09-mobile-structure.md:223` "**Contested.**"). OQ 23 says its record is missing: `09-mobile-structure.md:969` "with the record Group D raises … not yet written" | None needed |
| Testability | 5 | Every worker-killed row now has a TC: `13-workflows-and-sagas.md:187` "TC-PLT-786", `:247` TC-PLT-787, `:309` TC-ADM-781, `:435`, `:497`, `:603`, `:668`. Each is defined once in `16-annex-test-case-registry.md` (checked: 1 hit each). `:845` "A killed worker loses and duplicates nothing" maps to all of them. The class heatmap is asserted by `assessment.md:1177` TC-ASM-342, which gives exact values and a keyset page check | None needed |
| Distinctiveness | 4 | Feature 42 is now the view Appendix W describes: `08-web-structure.md:654` "`heatmap` (class grid of students by outcomes with a class row, columns mirror in RTL)", with a Because panel, the rung 2 fallback and demo test `TC-ASM-811`. Signature features 17, 19 and 38 still have no web moment (`08-web-structure.md:982` "now carries only features 17, 19 and 38") | Design the web screens of 17 and 38 to the §7.10 level. Do the same for 19 once OQ 31 lands |
| Portability | 5 | No change since round 6, and it still holds: `09-mobile-structure.md:645` Huawei trigger "more than 10 percent"; `:719` "240 iOS builds"; `:224` the QTI 3 renderer reason. The new heatmap mirrors in RTL and bidi-isolates outcome codes (`08-web-structure.md:858`) | None needed |

**Verdict:** Approved

| Round-6 gap | Now | Evidence |
|---|---|---|
| Feature 42's class heatmap had no endpoint and no owner. SL-ASM-219 promised "per student and class" | Closed | `06-services/assessment.md:351` `GET /api/v1/assessment/sections/{id}/standards-heatmap`, `:353` mastery computation, `:687` hot query, `:1177` TC-ASM-342. `08-web-structure.md:854,858` calls it. `34-work-breakdown.md:603` now agrees |
| 12 said RISK-54 "still names seven threats" (`:338`, `:839`) | Closed | `12-security-privacy-safety.md:338` "its text names all eight threats"; `:839` "RISK-54's text in document 18 now names all eight"; `18-risk-register.md:129,198` agree |
| The worker-killed resume scenarios had method names, not TC ids | Closed | `13-workflows-and-sagas.md:187,247,309,435,497,603,668` each cite a TC. Each is defined once in the 16 annex. `:845` lists them |
| Signature features 17, 19 and 38 had no web route | Open (stated with a default and an owner) | `08-web-structure.md:978` open point with default, owner Architect, score 6. Feature 19 is additionally gated by OQ 31 (`OPEN_QUESTIONS.md:40`) |

| Gap | Axis | Effort to close | Blocking |
|---|---|---|---|
| Signature features 17, 19 and 38 have no web route or §7.10 design. This is an open point with a default (`08-web-structure.md:978`), and 19 is gated on OQ 31 | Completeness, Distinctiveness | 1 day (17 and 38); 19 after OQ 31 | no |
| SL-ASM-219's deliverable names `GetResultAnalysis (heatmap, predicted)` (`34-work-breakdown.md:603`), but the handler is `GetStandardsHeatmapHandler` (`06-services/assessment.md:353`) | Consistency | 10 minutes | no |
| OQ 26 is paraphrased as "SOC 2 or ISO 27001" (`12-security-privacy-safety.md:836`), but the register says "SOC 2 or 1EdTech" (`OPEN_QUESTIONS.md:36`) | Consistency | 5 minutes | no |

#### 5.5 Group E: Operations, testing, roadmap, risk, dependencies, traceability (15 to 20)

| Axis | Score | Evidence | What would raise it by one |
|---|---|---|---|
| Completeness | 5 | Every PLAN_SPEC item for 15 to 20 is present, and the round-6 thin item is filled: `20-traceability-matrix.md:719` REQ-MOB-038 "SL-MOB-202 \| TC-BFF-761, TC-MOB-988". `20-traceability-matrix.md:24` "Requirements with a test identifier \| 860". The PLAT cells still name real platforms (`:889` to `:892`). | None needed. |
| Consistency | 4 | The round-6 drifts are closed: `16-annex-test-case-registry.md:21` "Derived acceptance tests ... 328", `20-traceability-matrix.md:26` "328", `16-test-strategy.md:81` "Each of the 328". I recounted 328 in both. The ADR-0027 change is carried: `18-risk-register.md:71` "sixty-one open and two Closed (RISK-40, and RISK-52 by ADR-0027)". New stale mention: `18-risk-register.md:86` RISK-43 lists versions "from v9.1 to v9.6", and its signal reads "the current one (v9.6 today)", but the brief is v9.7 under ADR-0027, and the `:273` change log did not sweep it. | Bring RISK-43 up to v9.7: add "v9.7 ADR-0027" to its list and change the signal to "v9.7 today". |
| Feasibility | 5 | The ranges still recompute. `17-roadmap.md:231` says "the low end moves from 6.89 to 6.99 ... One more phase 4 slice-day would take the low end to 8 weeks". The new ZATCA sensitivity is computed, not asserted: `17-roadmap.md:51` "The ZATCA slices SL-FIN-448 to SL-FIN-450 (9 slice-days ...) are the one estimate of low confidence". The totals agree with `00-executive-summary.md:38` "726 slices and 1,724". | None needed. |
| Risk honesty | 5 | RISK-52 was closed and not quietly dropped: `18-risk-register.md:165` "RISK-52 (4 x 4, closed by ADR-0027 on 2026-09-26)". The bands add up (`:182` "the four bands add to 61"; I recounted 10+29+20+2), and the top ten is re-ranked by its own rule (`:203` "RISK-48 ... entered at rank 10"). RISK-53 now opens with OQ9's default: `:148` "builds the e-invoicing plug-in in phase 3 only for the country of a VAT-registered first customer". | None needed. |
| Testability | 4 | The derived-test gap is closed: 16:81 says each derived test "is written by the slice that document 20's Slices column names", and TC-MOB-988 is now in 20:719. TC-ATT-964 is retired from the registry. Minor new gap from ADR-0027: `16-test-strategy.md:394` scopes fee mutation testing to "`Nibras.Finance.Domain/Fees/` ... `/Proration/`" and "The seventeen arithmetic rules". The Platform-owned billing rules BR-FIN-017/018 (`31-...:90-91`, `Nibras.Platform.Domain.Rules.*`) are therefore under no mutation threshold. | Add `Nibras.Platform.Domain/Rules/` (ActiveStudentCountRule, PlanChangeProrationRule) to the mutation-testing domain table in 16 §6.3. |
| Distinctiveness | 4 | Signature features still run in the demo gate: `17-roadmap.md:26` "minute 11 (guardian transparency, Appendix W feature 31) and minute 13 (emergency mode, feature 32) as scripted"; `16-test-strategy.md:642` gives the same minutes per phase. Not 5: no kit-lint rule compares the two (`kit-lint.mjs` R34 checks Appendix O and W only). | Add a kit-lint rule that the minute sets in 17 Section 1 and in 16's Demo script row match at each phase. |
| Portability | 5 | `20-traceability-matrix.md:890` "Chromium, Firefox and WebKit under Playwright on ubuntu-latest; real Chrome, Edge, Firefox, Safari and Samsung Internet on the device pass". `20:719` REQ-MOB-038 "Android 8 and later including devices without Google services" with platform "Android, iOS". `15-deployment-and-operations.md:878` states the macOS leg's missing nested virtualisation. | None needed. |

**Verdict:** Approved

| Round-6 gap | Now | Evidence |
|---|---|---|
| TC-MOB-988 is defined as REQ-MOB-038's derived test but not carried by 20, so no slice writes it | Closed | `20-traceability-matrix.md:719` "SL-MOB-202 \| TC-BFF-761, TC-MOB-988"; the rule is stated at `:30` "also carries its derived test when another plan document already cites that derived identifier" |
| Registry 329 against 20 and 16:81 327 derived tests | Closed | `16-annex:21` "328", `20:26` "328", `16-test-strategy.md:81` "328"; I recounted 328 rows in 20 and 328 in annex §4; TC-ATT-964 appears nowhere |
| 20 Section 2's example names TC-ATT-964 for REQ-ATT-014 | Closed | `20:30` "the acceptance test of REQ-IDN-001 is TC-IDN-951", matching the row at `20:49` "SL-IDN-001, SL-IDN-012 \| TC-IDN-951" |
| RISK-53 opens by putting every e-invoicing plug-in in phase 5 | Closed | `18-risk-register.md:148` "builds the e-invoicing plug-in in phase 3 only for the country of a VAT-registered first customer" |
| "Last updated" lines of 15, 16, 18 are stale | Closed | `15:5` "remediation round 7"; `16-test-strategy.md:5` "remediation round 7"; `18:5` "after open question 30 was decided (ADR-0027 ...)" |
| No kit-lint rule checks the demo minutes of 17 Section 1 against 16 part 12.2 | Open | No such rule in `tools/kit-lint/kit-lint.mjs` (R34 reads Appendices O and W only). The two agree today by hand (`17:25-29` against `16-test-strategy.md:642`) |

| Gap | Axis | Effort to close | Blocking |
|---|---|---|---|
| RISK-43 still says the brief runs "v9.1 to v9.6" and its signal reads "(v9.6 today)", while the brief is v9.7 under ADR-0027 (`18-risk-register.md:86`) | Consistency | 5 minutes | no |
| Mutation testing covers fees only in `Nibras.Finance.Domain` (`16-test-strategy.md:394`); the billing rules BR-FIN-017/018, now in `Nibras.Platform.Domain.Rules`, have no mutation threshold | Testability | 10 minutes | no |
| No kit-lint rule checks that the demo minutes in 17 Section 1 match 16 part 12.2 | Distinctiveness | 1 hour | no |

#### 5.6 Group F: Conventions to work breakdown (22 to 29, 31 to 34)

| Axis | Score | Evidence | What would raise it by one |
|---|---|---|---|
| Completeness | 4 | The round-6 residue is closed. The review records of 22 and 24 to 27 now carry the round-6 verdict, for example `22:788` "Group F review, round 6 \| Approved with minor gaps". `34:1466` says `RecordToolScore` "is listed in document 11 section 2.4". Residual gap: `34:1352` "Three product-owner decisions are open" has no row for Open Question 31, although that question gates SL-ACA-209 and SL-ACA-400 to 404 (`OPEN_QUESTIONS.md:40`, `34:1396`). | Add Open Question 31 as a fourth row of the `34:1352` table: default, slices, decider and score 9. |
| Consistency | 4 | ADR-0027 is carried through the group. `31:90` "Platform \| `ActiveStudentCountRulesTests` \| `Nibras.Platform.Domain.Rules.ActiveStudentCountRule`" matches Appendix S `:62`. `34:225` "computed here by Platform and nowhere else". `29:41` "Twenty-seven records: twenty-five Proposed … two Accepted (0019 and 0027)". `29:59` shows Open Question 30 as decided. `OPEN_QUESTIONS.md` has 30 rows marked Open. No stale Finance count remains. The Open Question 9 default reads the same in `23:742`, `34:758` and `OPEN_QUESTIONS.md:19`. Residue: `34:1352` names three open decisions that change slices but leaves out Open Question 31, which is itself open (`29:60`). | Same fix as for Completeness. |
| Feasibility | 5 | `34:1384` "at twice the estimate the three ZATCA slices take 18 slice-days … phase 3 reads 12 to 18 weeks … the launch total 62 to 95". I ran `schedule-34.mjs`: phase 3 has 335 days and reads "11 to 18", total "61 to 95". 344 × 1.3 ÷ 40 rounds up to 12, so the sensitivity holds. 28's illustrative prices are owned by RISK-28. | None needed. |
| Risk honesty | 5 | `31:326` "reviewed by the plan editor, not yet by the business-rules-reviewer agent", with owner Architect and score 2. `32:324` "that check rests on search snippets only". RISK-52 is closed by a decision, not by deletion (`18:71` "two Closed (RISK-40, and RISK-52 by ADR-0027)"). | None needed. |
| Testability | 5 | Tooling confirms `capacity-28.mjs` at 160 figures and 0 mismatched. BR-FIN-017 keeps its test class and property flag (`31:90` "`ActiveStudentCountRulesTests` … yes"). `31:164` ties the property reason to "the rounding Appendix S states (ADR-0027)". REQ-PLT-009 maps to `TC-PLT-103, TC-PLT-126, TC-PLT-128` (`20:104`). | None needed. |
| Distinctiveness | 4 | `32:324` "Fourteen features are unverified in all ten competitors (1, 2, 15, … 41) … that check rests on search snippets only". This is unchanged since round 6. Every feature row still names a demo minute and a test, for example `32:34` "Minute 1, TC-RPT-001". | Resolve PowerSchool, Classter and Classera by trial or by fetching the full pages. |
| Portability | 4 | Unchanged: `33:254` "reports the container-runtime check as unavailable on that runner rather than passing it", which rests on Open Question 14 (`OPEN_QUESTIONS.md:24`, Open, with a default and an owner). | Settle Open Question 14, or name a Mac host that runs the container leg of TC-PLAT-800. |

**Verdict:** Approved

| Round-6 gap | Now | Evidence |
|---|---|---|
| Open Question 9's default was stated two ways (`23:742` and `34:758` against `OPEN_QUESTIONS.md:19` and `34:1385`) | Closed | `23:742` "except the e-invoicing plug-in for the country of a VAT-registered first customer, which the Finance owner's team builds in phase 3". `34:758` and `34:1455` say the same. |
| Implementation cells of BR-FIN-017 and BR-FIN-018 named Finance, against the `31:329` Platform default | Closed | `31:90` and `31:91` name `Nibras.Platform.Domain.Rules.*`. Appendix S `:62` and `:63` give both rules the owner Platform (ADR-0027). |
| `34:1390` said `RecordToolScore` "is to be listed in document 11 section 2.5 … which this part does not edit" | Closed | The phrase is gone from 34. `34:1466` "says `RecordToolScore` is listed in document 11 section 2.4". |
| Review records of 22 and 24 to 27 stopped at "awaiting the round 5 score" | Closed | `22:788`, `24:816`, `25:532`, `26:584` and `27:423` each read "Group F review, round 6 \| Approved with minor gaps", followed by a round-7 row. |
| ZATCA upper bound was not shown as a phase 3 sensitivity | Closed | `34:1384` "Sensitivity of the phase 3 range … 12 to 18 weeks instead of 11 to 18". I checked the figure against the `schedule-34.mjs` formula. |
| Property-based classification awaits the business-rules-reviewer agent | Open | `31:326` "not yet by the business-rules-reviewer agent". The point is scored and owned, so it is not a defect. |
| Fourteen features unverified in all ten, and the check is snippet-only | Open | `32:324` is unchanged: "Fourteen features … snippet only". |
| macOS container leg of TC-PLAT-800 rests on Open Question 14 | Open | `33:254` is unchanged. It is stated honestly, with a default and an owner. |

| Gap | Axis | Effort to close | Blocking |
|---|---|---|---|
| `34:1352` says "Three product-owner decisions are open" that change slices, and its table leaves out Open Question 31. That question is open (`OPEN_QUESTIONS.md:40`, `29:60`) and gates SL-ACA-209 and SL-ACA-400 to 404. The slices do state the gate (`34:551`, `34:1396`). | Completeness, Consistency | 10 minutes | no |
| The property-based classification still awaits the business-rules-reviewer agent (`31:326`) | Risk honesty | 1 hour | no |
| Fourteen signature features are unverified in all ten competitors, and the second check is snippet-only (`32:324`) | Distinctiveness | 2 days | no |
| The macOS container leg of TC-PLAT-800 cannot run on the hosted runner until Open Question 14 is decided (`33:254`) | Portability | product-owner decision | no |

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
| 2026-09-26 | Round 7: six independent adversarial reviewers, one per group | All groups approved; lowest axis score 4 |

## How this document is verified

| Claim | Proof |
|---|---|
| Every score has evidence | Each row of Section 5 quotes a document with file and line; a row without one is invalid |
| The summary and history tables match the group scores | Computed from the group scorecards of every round by `build-30.cjs`, not typed |
| The verdict follows the rule | A group with any axis below 4 is shown blocked; the script applies the rule |
| The document is current | Kit-lint rule R23 reruns `build-30.cjs --check` and fails when a group scorecard changed since this document was built |
