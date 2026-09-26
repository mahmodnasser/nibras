# ADR-0024: Some Tier 2 requirements are built before Tier 1 is complete

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-26
- **Requirement IDs:** REQ-TST-025; REQ-IDN-010, REQ-MOB-039, REQ-NOT-002, REQ-PLT-023, REQ-PLT-039, REQ-SCH-026, REQ-SCH-036, REQ-ATT-013, REQ-ACA-007, REQ-ACA-017, REQ-ACA-019, REQ-ACA-023, REQ-ACA-024, REQ-ACA-025, REQ-ACA-026, REQ-ACA-027, REQ-ACA-028, REQ-ACA-030, REQ-ASM-010, REQ-ASM-011, REQ-ASM-012, REQ-ASM-031, REQ-ASM-033, REQ-COM-015, REQ-COM-016, REQ-DOC-009, REQ-DOC-017, REQ-FIN-033, REQ-FIN-034, REQ-RQS-019, REQ-BEH-006, REQ-BEH-008, REQ-RPT-013, REQ-RPT-016, REQ-RPT-017

## Context

Master brief Section 13 says "Tier 1 must be complete and polished before Tier 2 begins", and that Tier 3 is anticipated in the data model and built last. Phases 1 to 4 of `docs/plan/17-roadmap.md` build Tier 1; phase 5 is the extended scope.

The round-2 scorecard (Group E, risk honesty) found that the plan does not keep that rule and does not say so. Document 34 covers Tier 2 requirements in phases 1 and 2 (REQ-IDN-010, REQ-ACA-007, REQ-ASM-010 among them) with no record, although RISK-02's own mitigation in `docs/plan/18-risk-register.md` says "a slice whose requirement is Tier 2 cannot enter a Tier 1 phase without an ADR", and the top-ten action that was to enforce it was never built.

Counted from `docs/plan/03-requirements-catalog.md` (the tier column) and the phase 1 to 4 sections of `docs/plan/34-work-breakdown.md` (the Covers column), 35 Tier 2 or Tier 3 requirements are built in phases 1 to 4. The slices that build nothing but Tier 2 or Tier 3 work come to 59 slice-days (5 in phase 1, 24 in phase 2, 12 in phase 3, 18 in phase 4), 37 of them inside the MVP.

Part of the pull is the brief itself. Appendix O, normative since brief v9.4, gives every demo step a Phase cell, and eleven signature features that Appendix W marks Tier 2 (16, 18, 19, 20, 21, 22, 24, 36, 37, 38 and 42) are shown by steps whose Phase cell is 1 to 4; minute 14's Tier 1 feature 27 also rests on the Tier 3 group view of REQ-RPT-013. The two brief rules disagree for those features, and nothing in the plan said which one wins.

## Decision

**Every Tier 2 or Tier 3 requirement that a phase 1 to 4 slice builds is listed, with the reason it is built early. `docs/plan/17-roadmap.md` carries the same list in its table "Requirements built ahead of their tier", and kit-lint R35 fails when a requirement is built early without a row there, or when a row names a requirement no longer built early.**

A reason is one of four kinds, the same four document 17 uses:

- **Demo.** An Appendix O step whose Phase cell is 1 to 4 shows the feature. For signature features the Appendix O Phase cell wins over Section 13, because Appendix O is the release gate (ADR-0023).
- **Shared.** A Tier 1 requirement's mechanism carries it in the same slice.
- **Locality.** Its service is being built in that phase and nothing else needs it yet. Kept for now; the first candidates to move if the MVP must come sooner.
- **Move.** No reason stands. This record proposes the move; document 34 keeps building it where it is until the product owner accepts.

| Requirement | Tier | Slices | Phase | Kind | Reason |
|---|---|---|---|---|---|
| REQ-IDN-010 | 2 | SL-IDN-011 | 1 | Shared | Master brief Section 35 designs Identity so SAML 2.0 and SCIM are a protocol adapter; building the adapter beside the phase 1 sign-in pipeline proves that seam |
| REQ-MOB-039 | 2 | SL-NOT-002 | 1 | Move | Master brief Section 37 builds the Huawei adapter when the device share justifies it (open question 17); SL-NOT-002 keeps the `IPushSender` seam, FCM, APNs and the in-app fallback of Tier 1 REQ-NOT-003 |
| REQ-NOT-002 | 3 | SL-NOT-004 | 1 | Move | The channel interface and the SMS adapter are Tier 1; the WhatsApp adapter is a second implementation with no phase 1 need |
| REQ-PLT-023 | 2 | SL-PLT-022 | 1 | Demo | Minute 15 (phase 1) uses the template exchange, feature 37; it shares the library and review step of Tier 1 REQ-PLT-022 |
| REQ-PLT-039 | 3 | SL-PLT-025 | 1 | Move, in part | The tenant's owning partner stays in the phase 1 tenant model (the Tier 3 data-model duty); the partner console, commissions and white-label options move to phase 5 |
| REQ-SCH-026 | 2 | SL-SCH-236 | 2 | Demo | Reserve step R-09 (phase 2) shows balanced class formation, feature 20, at year close |
| REQ-SCH-036 | 3 | SL-SCH-237 | 2 | Move, in part | Year close needs the alumni status and profile for graduates; the directory and events move to phase 5 |
| REQ-ATT-013 | 2 | SL-ATT-223 | 2 | Shared | Tier 1 REQ-ATT-012 builds the device integration interface; the third-party adapters plug into it, and minute 2's gate-scan pre-fill (feature 26) runs through it |
| REQ-ACA-007 | 2 | SL-ACA-203 | 2 | Demo | Reserve step R-19 (phase 2) shows the standards heatmap, feature 42, over standards that arrive by CASE import. Built once; SL-INT-601 extends it in phase 5 |
| REQ-ACA-017 | 2 | SL-ACA-214 | 2 | Locality | The similarity note sits on the submission feedback of Tier 1 REQ-ACA-018 |
| REQ-ACA-019 | 2 | SL-ACA-214 | 2 | Locality | Audio feedback is one more attachment on the graded submission of Tier 1 REQ-ACA-018 |
| REQ-ACA-023 | 2 | SL-ACA-216 | 2 | Locality | The question bank is an Academics aggregate built while Academics is open |
| REQ-ACA-024 | 2 | SL-ACA-217 | 2 | Locality | Quizzes run on the question bank and grade into the Tier 1 gradebook |
| REQ-ACA-025 | 2 | SL-ACA-217 | 2 | Locality | Item analysis is computed from the same quiz attempts |
| REQ-ACA-026 | 2 | SL-ACA-216 | 2 | Locality | QTI 3 import and export are built once here; SL-INT-600 adds the per-item report and round trip in phase 5 |
| REQ-ACA-027 | 2 | SL-ACA-218 | 2 | Locality | Participation feeds Attendance while both are built; document 34 already moves the slice to phase 5 if its Appendix B permission has not landed |
| REQ-ACA-028 | 2 | SL-ACA-209, SL-ACA-400 to SL-ACA-404 | 2, 4 | Demo | Reserve step R-08 (phase 4) shows the daily sheet, feature 19 |
| REQ-ACA-030 | 2 | SL-ACA-405 | 4 | Shared, after a move | An LTI 1.3 launch needs the registration, login and keys SL-INT-411 builds in phase 4 for Tier 1 REQ-INT-016, so the launch could not run in phase 2 and document 34 has already moved it: SL-ACA-405 builds it under CAP-INT-02 beside SL-INT-411, and SL-ACA-207 keeps the Tier 1 resource library only |
| REQ-ASM-010 | 2 | SL-ASM-227, SL-ASM-228, SL-ASM-229 | 2 | Shared | The exam session of Tier 1 REQ-ASM-009 carries its papers |
| REQ-ASM-011 | 2 | SL-ASM-227 | 2 | Shared | The refusal, audit and alert guard the paper workflow |
| REQ-ASM-012 | 2 | SL-ASM-228 | 2 | Shared | Sealing and the print count close the paper workflow against REQ-ASM-009's candidates |
| REQ-ASM-031 | 2 | SL-ASM-219 | 2 | Locality | Predicted grades come from the same mastery data as the heatmap, never shown to a family |
| REQ-ASM-033 | 2 | SL-ASM-219 | 2 | Demo | Reserve step R-19 (phase 2 for the heatmap) shows feature 42 |
| REQ-COM-015 | 2 | SL-COM-405, SL-COM-407, SL-COM-408 | 3 | Shared | The Tier 1 form builder of REQ-RQS-002 produces consent forms and surveys; publishing them with an OTP signature sits on top |
| REQ-COM-016 | 2 | SL-COM-406, SL-COM-407, SL-MOB-404 | 3, 4 | Demo | Reserve step R-11 (phase 3) shows handbook acknowledgment, feature 22 |
| REQ-DOC-009 | 2 | SL-DOC-408, SL-DOC-413 | 3 | Locality | OCR indexing sits in the document library of Tier 1 REQ-DOC-008 |
| REQ-DOC-017 | 2 | SL-DOC-410 | 3 | Demo | Reserve step R-14 (phase 4) shows the yearbook, feature 36 |
| REQ-FIN-033 | 2 | SL-FIN-445 | 3 | Locality | Expenses and budgets sit on Finance's financial periods; the requisition half already waits for phase 5 |
| REQ-FIN-034 | 3 | SL-FIN-443 | 3 | Shared | Nothing Tier 3 is built: no general ledger is kept, and the Tier 1 accounting export of REQ-FIN-032 serves the requirement |
| REQ-RQS-019 | 2 | SL-RQS-421, SL-RQS-422, SL-RQS-423 | 3 | Locality | Duty rosters use the task and approval surfaces Requests builds in phase 3 |
| REQ-BEH-006 | 2 | SL-BEH-408 | 4 | Shared | The portfolio export carries each badge as an Open Badges 3.0 credential, built once; SL-INT-602 adds conformance in phase 5 |
| REQ-BEH-008 | 2 | SL-BEH-409, SL-BEH-413, SL-MOB-409 | 4 | Demo | Reserve step R-07 (phase 4) shows the portfolio, feature 18 |
| REQ-RPT-013 | 3 | SL-RPT-409, SL-RPT-412 | 4 | Demo | Minute 14 (phase 4) opens the cross-campus dashboard, feature 27, inside one tenant (ADR-0011) |
| REQ-RPT-016 | 2 | SL-RPT-418, SL-RPT-421 | 4 | Demo | Reserve step R-10 (phase 4) shows the inspection evidence folder, feature 21 |
| REQ-RPT-017 | 2 | SL-RPT-419, SL-RPT-421 | 4 | Shared | The policies and minutes are the evidence R-10's folders link to |

**One build per standard.** The CASE, QTI 3 and Open Badges 3.0 importers or exporters are each built once, in the phase their service is built (SL-ACA-203, SL-ACA-216, SL-BEH-408), and extended in phase 5 for the REQ-INT-016 checklist (SL-INT-601, SL-INT-600, SL-INT-602), never built a second time.

**Enforcement.** Kit-lint R35 keeps the list in document 17 complete and current. The kinds and reasons are reviewed by the architect with the product owner at each phase-end review, when RISK-02 is re-scored. REQ-TST-025 applies: a plan that builds scope ahead of its tier without saying so does not score 4 on risk honesty.

## Alternatives considered

- **Move every Tier 2 and Tier 3 requirement out of phases 1 to 4.** Rejected. It takes 59 slice-days out, but it also takes out the Tier 2 signature features that Appendix O shows from phases 1 to 4, so the phase demos and the demo gate of ADR-0023 would fail, and the Shared rows would reopen finished Tier 1 slices in phase 5.
- **Keep everything where it is and record nothing.** Rejected. It is what the scorecard found: RISK-02's mitigation was stated and not kept, and a reader of the roadmap could not tell scope creep from a decision.
- **Re-tier the Demo requirements to Tier 1 in document 03.** Rejected for now. The tiers come from Appendix A and master brief Section 13; changing them is a brief change with a version bump, which the product owner may still prefer to this record.

## Consequences

- Document 17 carries the list, and R35 fails the lint the day a slice starts building a Tier 2 or Tier 3 requirement in phases 1 to 4 without a row.
- If this record is accepted, document 34 moves the Move rows (REQ-MOB-039, REQ-NOT-002, and the named parts of REQ-PLT-039 and REQ-SCH-036), and document 17's table and ranges are recomputed; until then they stay as built. REQ-ACA-030 has already moved to phase 4 (SL-ACA-405) whatever the decision, because its launch cannot run before SL-INT-411.
- The Locality rows are the first to move if the MVP must come sooner; the product owner can move any of them without breaking a Tier 1 promise or a demo step.
- Revisit when Appendix O changes a Phase cell, when document 03 changes a tier, or when a first customer asks for a Tier 2 module in term one (RISK-32).
