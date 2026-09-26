# 32. Product Differentiation and Demo

> Plan document for the Nibras platform. Group F. It completes the differentiation matrix of Appendix P section P.4 for the 43 signature features in Appendix W, keeping Appendix W's row 39 so the numbering is stable, and maps the fifteen-minute demo of Appendix O onto the phases and capabilities of `17-roadmap.md`. It refines master brief Section 4 (a school owner must see the value in fifteen minutes), Section 12 (the signature features) and Section 26 (the competitive gap analysis); it does not re-derive them. Competitor facts are quoted from `02-competitive-gap-analysis.md`, whose findings win over Appendix P wherever the two differ (document 02 lists 21 corrections). Where this document and the brief disagree, an ADR records the deviation.

**Group** F · **Requirement areas covered** none directly; this document governs what the product claims to a buyer and how each claim is proven · **Last updated** 2026-09-26 by the platform plan

## Purpose

This document lets a salesperson, a demo presenter and a product owner make only claims the product can prove. For each of the 43 signature features it states who else has something comparable (as found on 2026-09-20 and in the second check on 2026-09-26, never guessed), the concrete edge, the moment the user would describe, the sixty-second proof, the capability that builds it, and the damage if a competitor catches up. It then states which claims the sales material must stop making, which demo minute exists at which phase, which minute or reserve step shows each signature feature and runs its demo test in the release gate, and the one-paragraph pitch for the three buyers who sign. The readers are the product owner, the demo director, the domain expert reviewing as a school buyer, and whoever writes the sales sheet.

## Scope

| In scope | Out of scope | Where the out-of-scope item lives |
|---|---|---|
| The P.4 matrix, one row per Appendix W feature | Re-checking competitor facts on the web | `02-competitive-gap-analysis.md` and its re-check before general availability |
| Claims the sales material may and may not make | Pricing and plan tiers | Master brief Section 22, `06-services/platform.md` |
| Each Appendix O minute mapped to a capability and a phase | The capabilities themselves and their weeks | `17-roadmap.md` §4 |
| The demo director's check of every feature against the sixty-second rule | The end-to-end test code behind each test case | `16-test-strategy.md` |
| Three buyer pitches, each with one proof | Marketing copy, brand, website | Outside the kit |
| Rung and autonomy per feature, quoted | Assigning rung and autonomy | `25-ai-and-assist-ladder.md` §2 |

**How to read "who else has it".** Values are those of document 02: **yes** (a vendor page states it), **partial** (a narrower form exists), **no** (the vendor says so, or the product's shape rules it out), **unverified** (no page checked on 2026-09-20 or 2026-09-26 states it either way). Bracketed numbers are document 02's source footnotes; sources 32 to 54 are search snippets of the 2026-09-26 check, whose page fetches were all refused by the checking session's egress policy. A feature that document 02's capability matrix does not cover is **unverified** unless document 02's field table names a comparable module or its "Second check of the signature features" records one, in which case that module is quoted. Unverified is not "no": a sales claim built on an unverified cell says "we found no competitor documenting it", never "nobody has it".

---

## 1. The completed differentiation matrix

Columns follow Appendix P section P.4, with two added: the rung and autonomy from `25-ai-and-assist-ladder.md` §2 (rung 1 at autonomy 1 unless stated), and the capability from `17-roadmap.md` §4 with its phase. **Sixty-second proof** is the Appendix O minute or reserve step that shows the feature, and the feature's own Appendix W demo test, which that step's Test cell runs (ADR-0023); a second minute that also shows the feature is named after it. **Risk** is Low, Medium or High for the damage if a competitor matches the feature before launch, with the reason.

### 1.1 Tier 1 features

| # | Feature | Who else has it (document 02) | Our edge | Persona moment | Rung / autonomy | Sixty-second proof | Capability (phase) | Risk if matched |
|---|---|---|---|---|---|---|---|---|
| 1 | Today dashboards where every card leads to an action | unverified in all ten | Every card carries its action button, so the approval, the unmarked class or the overdue invoice is completed on the card, not in a module | "I can see what needs me and do it here" | 1 / 1 | Minute 1, TC-RPT-001; also minutes 10 and 14 | CAP-RPT-01 (4) | Medium: dashboards are universal; the action-per-card discipline is easy to copy shallowly |
| 2 | Student 360 timeline filtered by the viewer's permissions | unverified in all ten; Veracross markets a "single-record architecture" [16] | One timeline per child where each viewer sees only the entries their role and data scope allow | "The whole child, on one screen" | 1 / 1 | Reserve R-02, TC-RPT-002 | CAP-SCH-02 (2), CAP-BFF-01 (1) | Medium: Veracross's single record is the closest match |
| 3 | Sixty-second attendance | partial: Fedena's mobile application marks attendance [31]; openSIS [1] and Toddle [25] have attendance; speed is documented by none | A section's register is taken on a phone in under sixty seconds, measured against the performance budget in master brief Section 19 | "Done before the bell stopped" | 1 / 1 | Minute 2, TC-ATT-810 | CAP-ATT-01 (2) | Low: speed is a budget enforced by a test, which a competitor has to rebuild for |
| 4 | Early warning with explanation and an intervention | partial: PowerSchool Risk Analysis and Early Warning System, predictive risk groups with named factors and thresholds a district can override [53]; an intervention with an owner and a fallback with the model off unverified | The flag names its factors and their weights and opens a playbook with an owner; with the model off it falls back to named threshold rules | "I know who is slipping and what to do" | 2 (degrades to rule thresholds) / 2 | Minute 5, TC-RPT-003 | CAP-RPT-03 (4); thresholds in CAP-ATT-02 (2) | Medium: predictive flags are common in enterprise student systems; the explanation and fallback are not documented by any |
| 5 | Smart timetable with live conflict detection | partial: Fedena automatic timetable, Ultimate tier [7]; openSIS scheduling [1]; PowerSchool "two decades of scheduling" [10]; Toddle timetable [25]; live conflict highlighting unverified | A dragged period shows its conflicts as it moves, and publishing leaves recorded attendance untouched and says so | "A term's work in an afternoon" | 1 / 1 | Minute 6, TC-SCD-001 | CAP-SCD-01 (2) | High: timetabling is where PowerSchool and Fedena are strongest; compete on the publish-safety message, not on the solver |
| 6 | Report Card Studio with QR verification | partial: report cards and transcripts in openSIS [1]; progress reports in Toddle [25]; QR verification unverified in all ten | Keyboard-speed mark entry, a live batch counter, and a QR on every card that a public page verifies in both languages without a login | "Eight hundred report cards, verified" | 1, comments at 3 / 3 for comments | Minutes 7 and 8, TC-ASM-810; also minute 9, whose QR verification is TC-DOC-002 | CAP-ASM-01, CAP-ASM-02 (2); CAP-DOC-02 (3) | Medium: report cards are parity; public QR verification is cheap to copy |
| 7 | Parent experience that respects attention | partial: Toddle family communication [25]; PowerSchool mobile application for grades and progress [10]; digesting and quiet hours unverified | Notifications are merged into one calm message per child per window, in the parent's language, with quiet hours | "One calm message, not twelve" | 1 / 1 | Minute 4, TC-NOT-001 | CAP-NOT-01 (1), CAP-MOB-02 (2) | Medium: every vendor has a parent app; restraint is a design choice anyone can make |
| 8 | Safety and dismissal with gate passes | partial: Veracross help articles on managing student dismissals and an early-dismissal location log, titles only [51]; verification of the collecting adult at the gate unverified | Pickup persons, gate passes and early dismissal are verified at the gate, and the record shows who collected the child | "I know who collected the child" | 1 / 1 | Minute 13, TC-ATT-811 | CAP-ATT-03 (2) | Medium: dismissal is a known product category outside school management systems |
| 9 | Go live in a day: import with dry run and rollback | partial: openSIS bulk import [1]; Fedena performs onboarding data and configuration as a vendor service on every plan [9]; dry run and rollback unverified | The school imports its own data with a dry run that shows every problem row, and a rollback if the result is wrong, without a services engagement | "We moved in a morning" | 1 / 1 | Reserve R-03, TC-DOC-001 | CAP-DOC-03 (3) | High: Fedena's human onboarding competes directly; the claim must be elapsed time and self-service |
| 10 | Configurable without code | partial: Fedena form builder, Premium tier [7]; configurability depth undocumented for all | A school changes fee plans, grading scales, workflows and forms itself, and every change is versioned | "We changed it ourselves" | 1 / 1 | Minute 10, TC-PLT-805; also reserve R-15 | CAP-PLT-02 (1) | Medium: every vendor claims configurability; only a live change proves it |
| 11 | Command palette and natural-language search | partial: Veracross Axiom Launchpad Search reaches people, queries, reports and pages from one bar [46]; Arabic-aware matching and natural language unverified | One keystroke reaches any permitted screen or record, with Arabic-aware matching; natural language only at rung 3 | "I typed what I wanted" | 1, natural language at 3 / 1 | Minute 12, TC-WEB-001 | CAP-BFF-01 (1); natural language in CAP-AI-01 (5) | Low: easy to copy, but Arabic normalization under it is not |
| 12 | Offline-first mobile | partial: PowerSchool's SIS Teacher Offline Mode takes attendance offline, on Windows and macOS, not on a phone [39]; no: Toddle requires "3G+" [25]; unverified for the other eight | Attendance is marked with no network, queued visibly, and never claimed complete while queued | "The corridor has no signal and it still worked" | 1 / 1 | Minute 3, TC-MOB-001 | CAP-MOB-01, CAP-ATT-01 (2) | Low: offline sync with visible conflict rules is expensive to retrofit |
| 13 | Data quality center | partial: PowerSchool data validation rules and missing or incomplete records reports [47]; the rule and a fix link on each finding unverified | Wrong or missing records surface themselves with the rule that found them and a fix link | "The list was wrong and the system told us" | 1 / 1 | Reserve R-04, TC-RPT-004 | CAP-RPT-03 (4) | Low |
| 14 | Trust by design: audit, consent, export, erasure | partial: Fedena audit module, Ultimate tier [7]; Classter audit trails, export of a student's data and erasure on request, from a vendor blog [34]; a guardian-facing export and consent record unverified | An append-only audit trail, recorded consents, a full export for a guardian and erasure under retention rules, all in the product | "We can answer any question about any record" | 1 / 1 | Minute 11, TC-AUD-001; also minutes 9 and 12 | CAP-AUD-01, CAP-PRV-01 (1) | Medium: audit logs are common; the guardian-facing half is not |
| 15 | Sales-ready demo mode with one-click reset | unverified in all ten | The demo tenant resets to its seed in one click, and the demo runs on ordinary accounts | "Show it to the next school in ten minutes" | 1 / 1 | Minute 15, TC-PLT-801; the same reset runs before every demo | CAP-PLT-01 (1), slice SL-PLT-006; one-click reset on a fresh tenant is also a phase 4 exit criterion | Low: internal sales tooling, invisible to buyers |
| 17 | Photo and media consent enforced at publishing | unverified in all ten | Publishing a photo checks each tagged child's media consent and blocks the post, instead of relying on staff memory | "That child's photo cannot be posted" | 1 / 1 | Reserve R-06, TC-COM-001 | CAP-COM-01 (3) | Low |
| 23 | Live, animated, modern interface | unverified in all ten (subjective; no page states it) | Real-time updates, motion and both directions and themes on every screen, from one design system | "It does not feel like school software" | 1 / 1 | No step of its own: present in every minute, and guarded by TC-UX-001, which Appendix O names | CAP-UX-01 (1) | High: the most easily matched claim on the list; never lead with it |
| 25 | Morning brief per role | partial: Blackbaud "My Day" shows a faculty member's daily obligations [49]; a brief for every role assembled from every service unverified | Each role opens to its day assembled from every service, with fixed bilingual sentences at rung 1 | "I knew my day before I arrived" | 1, phrasing at 3 / 1 | Minute 1, TC-RPT-006 | CAP-RPT-01 (4) | Medium: a digest is easy to copy shallowly |
| 26 | Exception-only attendance | partial: Fedena biometric capture [8]; register pre-fill unverified for all | Gate scan, bus boarding and approved leave are merged into the register before the teacher opens it, with conflict rules | "The register was already half filled" | 1 / 4 (pre-fill, undone by one tap) | Minute 2, TC-ATT-812 | CAP-ATT-01 (2), approved leave through CAP-RQS-01 (3, MVP subset) | Low: the three-source merge and its conflict rules are the moat, not gate capture |
| 27 | Explain this number | partial: PowerSchool Analytics and Insights drills from a metric to its student list [32]; no product found showing the rule version | Every figure drills to the records and the rule version that produced it | "I could show the parent exactly why" | 1 / 1 | Minute 14, TC-RPT-007 | CAP-RPT-01 (4) | Medium: easy to imitate shallowly, hard to imitate completely |
| 28 | Because panel on every automated action | unverified in all ten; Toddle markets AI drafting with no stated fallback [25] | Every automated action shows its rule identifier, inputs and an override with a recorded reason | "It told me why, and I could disagree" | 1 / 1 | Minute 5, TC-RPT-008 | CAP-RPT-03 (4), CAP-AI-02 (5) | Medium |
| 29 | Smart defaults engine at onboarding | unverified in all ten; Fedena's answer is vendor staff [9] | Country and school type infer the configuration areas, and the screen shows what was inferred and why | "It already knew how our year works" | 1 / 3 | Minute 15, TC-PLT-802 | CAP-PLT-01 (1) | Medium: competes with a human service, not a wizard |
| 30 | Intervention playbooks | unverified in all ten | A flag becomes a plan from a library with an owner and a review date, proposed and never auto-assigned | "A flag became a plan with an owner" | 1 / 3 | Minute 5, TC-WEL-810, from phase 5 | CAP-ATT-02 (2), CAP-RPT-03 (4); the playbook library in CAP-WEL-02 (5) | Low |
| 31 | Guardian transparency on sensitive access | partial: staff-side access logs in PowerSchool Special Programs [33] and Classter [34]; no product found showing the log to the guardian | A parent sees which roles read their child's sensitive records and when, with consents and the retention clock | "I can see who looked at my child's file" | 1 / 1 | Minute 11, TC-AUD-002 | CAP-AUD-01, CAP-PRV-01 (1) | Low: it needs an access log most products do not keep |
| 32 | Emergency mode with reunification | partial: emergency broadcast in Blackbaud [23], PowerSchool (SchoolMessenger) [35] and Classter [36]; Veracross integrates Ruvna, a third-party building-attendance platform [37]; Gibbon evacuation absence reports [3]; reunification with verified pickup found nowhere | Roll call by location, streamed acknowledgements, the shrinking "not accounted for" list and verified pickup in one flow, offline-capable | "Every child accounted for in four minutes" | 1 / 1 | Minute 13, TC-ATT-813 | CAP-ATT-04 (2) | Medium: broadcast is matched; reunification is not |
| 34 | Teacher five-minute mode | unverified in all ten | The teacher's phone opens to the current class with four actions and nothing else | "Four taps and I was teaching" | 1 / 1 | Minute 2, TC-MOB-003 | CAP-MOB-01 (2) | Medium: a focused mode is easy to copy |
| 35 | Parent calm screen | unverified in all ten | One card per child that says plainly when nothing needs the parent | "Nothing needed me today, and it said so" | 1 / 1 | Minute 4, TC-MOB-004 | CAP-MOB-02 (2) | Medium |
| 39 | Calendar-aware scaling — **no longer a signature feature (ADR-0019)**; an engineering capability in Appendix A, A1, still built and still measured | unverified in all ten (PowerSchool and Classter are hosted on Azure [10], [13]; scaling policy undocumented) | Capacity is raised ahead of the 08:00 attendance peak from the school calendar, not after the load arrives | None, as Appendix W row 39 now records | 1 / 1 | None. It has no minute and no reserve step; Appendix N scenarios N-01 and N-11 measure it | CAP-PERF-02 (6) | Low: invisible to buyers; a reliability property |
| 40 | Self-healing operations | unverified in all ten | Failed messages are replayed inside a written policy, with an audit entry and a note to the operator | "It fixed itself and told us what it did" | 1 / 4 | Reserve R-17, TC-INF-801 | CAP-MSG-01 (1), CAP-INF-04 (6) | Low |
| 41 | Configuration as code | unverified in all ten | A settings change is exported, reviewed and applied as a versioned file, with a diff before it goes live | "We reviewed the change before it went live" | 1 / 1 | Reserve R-18, TC-PLT-804 | CAP-PLT-02 (1) | Low |
| 44 | Low-bandwidth mode | no: Toddle requires "3G+" [25]; no page mentions a data-saver mode; unverified for the other nine | A data-saver profile for the phones and plans parents actually own | "It worked on my old phone" | 1 / 1 | Minute 4, TC-MOB-005, from phase 4 | CAP-MOB-03 (4) | Low |

### 1.2 Tier 2 features

| # | Feature | Who else has it (document 02) | Our edge | Persona moment | Rung / autonomy | Sixty-second proof | Capability (phase) | Risk if matched |
|---|---|---|---|---|---|---|---|---|
| 16 | White-label mobile apps | yes: Fedena branded iOS and Android applications as an add-on [7] | Branding per school from one codebase and one release train | "It is our school's app" | 1 / 1 | Reserve R-05, TC-MOB-002 | CAP-MOB-03 (4), CAP-PLT-02 (1) | High: already matched; parity, not an edge |
| 18 | Student portfolio and recognition | partial: Toddle portfolios [25] | Achievements, recognition and work samples collected per student and exportable by the family | "Everything they achieved, theirs to keep" | 1 / 1 | Reserve R-07, TC-BEH-810 | CAP-BEH-01 (4) | High: Toddle is strong here |
| 19 | Kindergarten daily sheet | partial: Toddle Play, the early-years product, has daily reports and parent messaging [52]; meals and naps unverified | Meals, naps and activities recorded in taps and sent as one daily sheet per child | "I know how the day went" | 1 / 1 | Reserve R-08, TC-ACA-810 | CAP-ACA-03 (4) | Medium: dedicated nursery apps exist outside the ten |
| 20 | Balanced class formation | partial: PowerSchool partners with Class Solver, a separate product that balances classes on ability, behaviour, maturity and gender with pairings and separations [48] | Classes balanced on gender, ability, needs and separations in one pass, then adjusted by hand with the balance shown live | "Fair classes in one pass, then adjusted by hand" | 1 / 1 | Reserve R-09, TC-SCH-810 | CAP-SCH-04 (2) | Low |
| 21 | Inspection and accreditation readiness | partial: Toddle accreditation workflows [25] | Evidence is tagged to inspection criteria as it is created, so the folder fills itself | "The evidence folder filled itself" | 1 / 1 | Reserve R-10, TC-RPT-005 | CAP-RPT-02 (4) | High: Toddle leads on accreditation for IB schools |
| 22 | Policy and handbook acknowledgment | partial: Classter lists Protocols and Signatures modules [14]; acknowledgment proof unverified | Every recipient's acknowledgment is recorded against the policy version, with a chase for the missing ones | "Everyone signed, and we can prove it" | 1 / 1 | Reserve R-11, TC-COM-002 | CAP-COM-01 (3) | Medium |
| 24 | Open by default: API, webhooks, iCal, standards | yes for an API: PowerSchool [10], Classter [13], Veracross [16], ManageBac [20], Blackbaud [22], Toddle [25]; partial: Fedena [8], Classera [29]; webhooks: yes for PowerSchool, data-event notifications to third-party applications [40], no for Gibbon [6], unverified for the rest; OneRoster: Veracross [17], Toddle [26] | Outgoing webhooks, iCal, OneRoster and a full export that stay available read-only before suspension; the API alone is parity | "It talks to what we already use" | 1 / 1 | Reserve R-12, TC-INT-001 | CAP-INT-01 (3), iCal in CAP-SCD-03 (2) | High for the API and webhooks (both matched by PowerSchool); Low for read-only access before suspension |
| 33 | Campus digital twin | unverified in all ten | A live floor plan of rooms, occupancy and bookings, drawn from the timetable and bookings | "I saw the empty rooms at a glance" | 1 / 1 | Reserve R-13, TC-OPS-810 | CAP-OPS-03 (5), bookings in CAP-SCD-03 (2) | Low |
| 36 | School memory: portfolio and yearbook | partial: Toddle portfolios [25]; a generated yearbook unverified | The yearbook is assembled from the year's consented photos, events and recognition, and rendered in both languages | "The yearbook made itself" | 1 / 1 | Reserve R-14, TC-DOC-801 | CAP-BEH-01 (4), CAP-DOC-01 (1) | Low |
| 37 | Template exchange between schools | unverified in all ten; Classter's marketplace [13] is for integrations, not templates | A school publishes a timetable, report card or policy template and another starts from it | "We started from someone else's good work" | 1 / 1 | Minute 15, TC-PLT-803 | CAP-PLT-02 (1), slice SL-PLT-022 | Medium |
| 38 | Plug-in kit for regional integrations | yes: PowerSchool Plugin System [10], Classter 40+ integrations and marketplace [13]; partial: Toddle 50+ integrations [26], Veracross 200+ partners [16] | A partner builds a ministry export or a GPS adapter against a published contract, without a Nibras release | "Our partner built the ministry export" | 1 / 1 | Reserve R-16, TC-INT-002 | CAP-INT-01 (3) | High: already matched by the two strongest regional incumbents |
| 42 | Mastery and next step | partial: ManageBac standards across IB, IGCSE, AP and 600+ others [19]; Toddle curriculum maps and assessment [25]; next-step suggestion unverified | A standards heatmap per class with a suggested next teaching step, and the heatmap alone when the model is off | "I saw the gap and what to teach next" | 2 (degrades to the raw heatmap) / 2 | Reserve R-19, TC-ASM-811 | CAP-ASM-01 (2); the suggestion through CAP-AI-01 (5) | High: ManageBac and Toddle own curriculum depth |
| 43 | Workload balance for staff | partial: Classter Staff Workload shows each teacher's daily load and availability for substitution and compares contract and timetable hours [50]; a flag before overload unverified | Teaching load, cover and duties per teacher with a flag before overload, staff data only | "Someone noticed before I burned out" | 2 (degrades to load totals) / 2 | Reserve R-20, TC-HR-810 | CAP-SCD-02 (2), CAP-HR-02 (5) | Low |

**Totals.** 44 rows: 31 at Tier 1 and 13 at Tier 2, matching Appendix W row for row. Appendix W keeps row 39 but marks it as moved — under ADR-0019 calendar-aware scaling is an engineering capability in Appendix A, A1 — so the numbering is stable at 44 while **43 are signature features**. Appendix P section P.4 drops the row entirely; this document follows Appendix W and keeps it, marked, so a reader looking for feature 39 finds where it went. Of those 43, counted from the Feature cells of Appendix O: twenty-four are shown in a minute, eighteen only in a reserve step, and one, feature 23, is present in every minute with no step of its own and guarded by TC-UX-001, which Appendix O names (§4). Every one of the 43 runs its own Appendix W demo test in the release gate from the phase that builds it, and kit-lint R34 fails the lint when a feature loses its step or its test (ADR-0023). Against document 02, after its second check on 2026-09-26: 3 features are **yes** somewhere else (16, 38, and 24 for its API and webhook half), 25 are **partial** (3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 18, 19, 20, 21, 22, 25, 26, 27, 31, 32, 36, 42, 43), 1 is **no** on the one product that documents it (44, Toddle) and unverified on the other nine, and 14 are **unverified** in all ten (1, 2, 15, 17, 23, 28, 29, 30, 33, 34, 35, 37, 40, 41). Before the second check the counts were 3, 14, 2 and 24; the second check found a narrower form for ten of the twenty-four (4, 8, 11, 13, 19, 20, 25, 27, 31, 43) and moved feature 12 from no to partial (PowerSchool's desktop offline application). The five features document 02 and RISK-31 in `18-risk-register.md` name as undocumented by PowerSchool (26, 27, 29, 31, 32) all have a minute; the second check found narrower PowerSchool forms of 27 (a drill to the student list), 31 (a staff-side access log in a separate product) and 32 (emergency broadcast), so those minutes lead with the rule version, the guardian's view and reunification with verified pickup.

### Signature feature trace

One row per signature feature, from the moment a buyer is promised it to the slice an engineer picks up. This is the one table that traces the 43 features end to end; other documents cite a feature by its Appendix W number and point here rather than copying the rows. **Rung / autonomy** is quoted from Appendix W. **Requirements** are the rows of `03-requirements-catalog.md` whose Source cites the feature's item of master brief Section 12, or whose acceptance test is the feature's demo test. **Capabilities** and **Slices** are found through those requirements: every slice of `34-work-breakdown.md` whose Covers cell names one of them, and the capability it sits under, with the phase `17-roadmap.md` §4 gives that capability. **Appendix O step** is the minute or reserve step whose Test cell runs the demo test, with the phase in its Appendix O row. **Demo test** is Appendix W's. The §1.1 and §1.2 Capability column names the capability the sixty-second proof depends on; this table lists every capability that builds a requirement of the feature, so it can name more. Row 39 is absent because feature 39 is no longer a signature feature (ADR-0019).

| # | Feature | Rung / autonomy | Requirements (document 03) | Capabilities (phase) | Slices (document 34) | Appendix O step | Demo test |
|---|---|---|---|---|---|---|---|
| 1 | Today dashboards where every card leads to an action | 1 / 1 surfaces | REQ-ATT-038, REQ-RPT-002, REQ-UX-006 | CAP-BFF-01 (1), CAP-UX-01 (1), CAP-ATT-01 (2), CAP-MOB-03 (4), CAP-RPT-01 (4) | SL-ATT-206, SL-BFF-002, SL-MOB-401, SL-MOB-410, SL-RPT-406, SL-RPT-411, SL-RPT-412, SL-UX-006 | Minute 1 (phase 4; the substitution before it) | TC-RPT-001 |
| 2 | Student 360 timeline filtered by the viewer's permissions | 1 / 1 surfaces | REQ-SCH-035, REQ-BFF-003 | CAP-BFF-01 (1), CAP-MOB-02 (2), CAP-SCH-02 (2), CAP-RPT-01 (4) | SL-BFF-005, SL-BFF-207, SL-MOB-215, SL-RPT-410, SL-SCH-220 | R-02 (phase 2) | TC-RPT-002 |
| 3 | Sixty-second attendance | 1 / 1 surfaces | REQ-ATT-003 | CAP-ATT-01 (2) | SL-ATT-200, SL-ATT-202, SL-ATT-204, SL-ATT-207 | Minute 2 (phase 2) | TC-ATT-810 |
| 4 | Early warning with explanation and an intervention | 2, degrades to rule thresholds / 2 suggests | REQ-ACA-016, REQ-RPT-007, REQ-RPT-008, REQ-RPT-009 | CAP-ACA-02 (2), CAP-MOB-03 (4), CAP-RPT-03 (4) | SL-ACA-212, SL-MOB-410, SL-RPT-422, SL-RPT-423, SL-RPT-424, SL-RPT-426 | Minute 5 (phase 4; the substitution before it) | TC-RPT-003 |
| 5 | Smart timetable with live conflict detection | 1 / 1 surfaces | REQ-SCD-003, REQ-SCD-007, REQ-SCD-008 | CAP-SCD-01 (2) | SL-SCD-203, SL-SCD-205, SL-SCD-207, SL-SCD-210, SL-SCD-211 | Minute 6 (phase 2) | TC-SCD-001 |
| 6 | Report Card Studio with QR verification | 1, comments at 3 / 3 drafts | REQ-ASM-024, REQ-ASM-029, REQ-DOC-005, REQ-AI-011 | CAP-ASM-02 (2), CAP-MOB-02 (2), CAP-DOC-02 (3), CAP-MOB-03 (4), CAP-AI-01 (5) | SL-AI-605, SL-AI-613, SL-ASM-211, SL-ASM-214, SL-ASM-215, SL-ASM-220, SL-DOC-404, SL-DOC-405, SL-DOC-413, SL-MOB-215, SL-MOB-406 | Minutes 7 and 8 (phase 2) | TC-ASM-810 |
| 7 | Parent experience that respects attention | 1 / 1 surfaces | REQ-COM-011, REQ-NOT-007, REQ-NOT-008 | CAP-NOT-01 (1), CAP-COM-02 (3) | SL-COM-416, SL-COM-421, SL-NOT-003 | Minute 4 (phase 2) | TC-NOT-001 |
| 8 | Safety and dismissal with gate passes | 1 / 1 surfaces | REQ-ATT-024, REQ-ATT-032 | CAP-ATT-03 (2), CAP-ATT-04 (2) | SL-ATT-220, SL-ATT-226, SL-ATT-227, SL-ATT-228, SL-ATT-229, SL-ATT-230 | Minute 13 (phase 2) | TC-ATT-811 |
| 9 | Go live in a day: import with dry run and rollback | 1 / 1 surfaces | REQ-SCH-011, REQ-DOC-009, REQ-DOC-011, REQ-DOC-012, REQ-DOC-013 | CAP-SCH-01 (2), CAP-DOC-02 (3), CAP-DOC-03 (3) | SL-DOC-408, SL-DOC-413, SL-DOC-414, SL-DOC-415, SL-DOC-416, SL-DOC-417, SL-DOC-418, SL-DOC-419, SL-DOC-423, SL-SCH-206, SL-SCH-208 | R-03 (phase 3) | TC-DOC-001 |
| 10 | Configurable without code | 1 / 1 surfaces | REQ-PLT-025, REQ-ADM-003, REQ-RQS-001, REQ-RQS-002 | CAP-PLT-02 (1), CAP-COM-01 (3), CAP-RQS-01 (3), CAP-ADM-01 (4) | SL-ADM-400, SL-ADM-403, SL-ADM-417, SL-COM-405, SL-IDN-037, SL-PLT-015, SL-RQS-400, SL-RQS-402, SL-RQS-415 | Minute 10 (phase 3; the substitution before it) | TC-PLT-805 |
| 11 | Command palette and natural-language search | 1, natural language at 3 / 1 surfaces | REQ-AI-014, REQ-BFF-006 | CAP-BFF-01 (1), CAP-AI-01 (5) | SL-AI-608, SL-AI-612, SL-AI-613, SL-BFF-004 | Minute 12 (phase 5; the substitution before it) | TC-WEB-001 |
| 12 | Offline-first mobile | 1 / 1 surfaces | REQ-MOB-007 | CAP-ATT-01 (2), CAP-MOB-01 (2), CAP-ACA-03 (4), CAP-MOB-03 (4) | SL-ACA-401, SL-ACA-403, SL-ASM-230, SL-ATT-205, SL-MOB-208, SL-MOB-400, SL-MOB-404, SL-MOB-409 | Minute 3 (phase 2) | TC-MOB-001 |
| 13 | Data quality center | 1 / 1 surfaces | REQ-SCH-027, REQ-RPT-014 | CAP-SCH-02 (2), CAP-RPT-03 (4) | SL-RPT-425, SL-RPT-426, SL-SCH-219, SL-SCH-222 | R-04 (phase 4) | TC-RPT-004 |
| 14 | Trust by design: audit, consent, export, erasure | 1 / 1 surfaces | REQ-AUD-002, REQ-AUD-009, REQ-PRV-021 | CAP-AUD-01 (1), CAP-PRV-01 (1) | SL-AUD-001, SL-AUD-003, SL-AUD-006, SL-PRV-004, SL-PRV-007 | Minute 11 (phase 2) | TC-AUD-001 |
| 15 | Sales-ready demo mode with one-click reset | 1 / 1 surfaces | REQ-PLT-028 | CAP-PLT-01 (1) | SL-PLT-006 | Minute 15 (phase 1) | TC-PLT-801 |
| 16 | White-label mobile apps | 1 / 1 surfaces | REQ-MOB-001 | CAP-MOB-01 (2) | SL-MOB-202 | R-05 (phase 4) | TC-MOB-002 |
| 17 | Photo and media consent enforced at publishing | 1 / 1 surfaces | REQ-SCH-034, REQ-COM-004 | CAP-SCH-02 (2), CAP-COM-01 (3) | SL-COM-404, SL-COM-407, SL-SCH-216 | R-06 (phase 3) | TC-COM-001 |
| 18 | Student portfolio and recognition | 1 / 1 surfaces | REQ-BEH-008 | CAP-BEH-01 (4), CAP-MOB-03 (4) | SL-BEH-409, SL-BEH-413, SL-MOB-409 | R-07 (phase 4) | TC-BEH-810 |
| 19 | Kindergarten daily sheet | 1 / 1 surfaces | REQ-ACA-028 | CAP-ACA-01 (2), CAP-ACA-03 (4) | SL-ACA-209, SL-ACA-400, SL-ACA-401, SL-ACA-402, SL-ACA-403, SL-ACA-404 | R-08 (phase 4) | TC-ACA-810 |
| 20 | Balanced class formation | 1 / 1 surfaces | REQ-SCH-026 | CAP-SCH-04 (2) | SL-SCH-236 | R-09 (phase 2) | TC-SCH-810 |
| 21 | Inspection and accreditation readiness | 1 / 1 surfaces | REQ-RPT-016 | CAP-RPT-02 (4) | SL-RPT-418, SL-RPT-421 | R-10 (phase 4) | TC-RPT-005 |
| 22 | Policy and handbook acknowledgment | 1 / 1 surfaces | REQ-COM-016 | CAP-COM-01 (3), CAP-MOB-03 (4) | SL-COM-406, SL-COM-407, SL-MOB-404 | R-11 (phase 3) | TC-COM-002 |
| 23 | Live, animated, modern interface | 1 / 1 surfaces | REQ-UX-008 | CAP-UX-01 (1) | SL-UX-004 | None of its own: present in every minute, guarded by its demo test, which Appendix O names | TC-UX-001 |
| 24 | Open by default: API, webhooks, iCal, standards | 1 / 1 surfaces | REQ-INT-001, REQ-INT-015 | CAP-PLT-02 (1), CAP-INT-01 (3) | SL-INT-002, SL-INT-407, SL-INT-408, SL-INT-409, SL-INT-410 | R-12 (phase 3; the iCal half from phase 2) | TC-INT-001 |
| 25 | Morning brief per role | 1, phrasing at 3 / 1 surfaces | REQ-RPT-003 | CAP-RPT-01 (4) | SL-RPT-407, SL-RPT-412 | Minute 1 (phase 4; the substitution before it) | TC-RPT-006 |
| 26 | Exception-only attendance | 1 / 4 acts, the pre-fill only | REQ-ATT-005 | CAP-ATT-01 (2) | SL-ATT-208 | Minute 2 (phase 2) | TC-ATT-812 |
| 27 | Explain this number | 1 / 1 surfaces | REQ-RPT-010 | CAP-MOB-03 (4), CAP-RPT-01 (4) | SL-MOB-410, SL-RPT-408, SL-RPT-412 | Minute 14 (phase 4; the substitution before it) | TC-RPT-007 |
| 28 | Because panel on every automated action | 1 / 1 surfaces | REQ-RPT-011 | CAP-MOB-03 (4), CAP-RPT-01 (4), CAP-RPT-03 (4) | SL-MOB-410, SL-RPT-408, SL-RPT-412, SL-RPT-423, SL-RPT-424, SL-RPT-426 | Minute 5 (phase 4; the substitution before it) | TC-RPT-008 |
| 29 | Smart defaults engine at onboarding | 1 / 3 drafts | REQ-PLT-003 | CAP-PLT-01 (1) | SL-PLT-005 | Minute 15 (phase 1) | TC-PLT-802 |
| 30 | Intervention playbooks | 1 / 3 drafts | REQ-WEL-011 | CAP-WEL-02 (5) | SL-WEL-613, SL-WEL-618 | Minute 5 (phase 4; its demo test from phase 5) | TC-WEL-810 |
| 31 | Guardian transparency on sensitive access | 1 / 1 surfaces | REQ-AUD-007 | CAP-PRV-01 (1) | SL-AUD-007 | Minute 11 (phase 2) | TC-AUD-002 |
| 32 | Emergency mode with reunification | 1 / 1 surfaces | REQ-ATT-032 | CAP-ATT-04 (2) | SL-ATT-227, SL-ATT-228, SL-ATT-229, SL-ATT-230 | Minute 13 (phase 2) | TC-ATT-813 |
| 33 | Campus digital twin | 1 / 1 surfaces | REQ-OPS-016 | CAP-OPS-03 (5) | SL-OPS-623, SL-OPS-624, SL-OPS-626 | R-13 (phase 5) | TC-OPS-810 |
| 34 | Teacher five-minute mode | 1 / 1 surfaces | REQ-BFF-007 | CAP-MOB-01 (2) | SL-BFF-205, SL-MOB-207 | Minute 2 (phase 2) | TC-MOB-003 |
| 35 | Parent calm screen | 1 / 1 surfaces | REQ-BFF-008 | CAP-MOB-02 (2), CAP-ACA-03 (4), CAP-MOB-03 (4) | SL-ACA-402, SL-BFF-206, SL-MOB-214, SL-MOB-401 | Minute 4 (phase 2) | TC-MOB-004 |
| 36 | School memory: portfolio and yearbook | 1 / 1 surfaces | REQ-DOC-017 | CAP-DOC-02 (3) | SL-DOC-410 | R-14 (phase 4) | TC-DOC-801 |
| 37 | Template exchange between schools | 1 / 1 surfaces | REQ-PLT-023 | CAP-PLT-02 (1) | SL-PLT-022 | Minute 15 (phase 1) | TC-PLT-803 |
| 38 | Plug-in kit for regional integrations | 1 / 1 surfaces | REQ-INT-017 | CAP-PLT-02 (1) | SL-INT-004 | R-16 (phase 3) | TC-INT-002 |
| 40 | Self-healing operations | 1 / 4 acts | REQ-PLT-018, REQ-PLT-021, REQ-PLT-037 | CAP-PLT-02 (1), CAP-INF-06 (6) | SL-PLT-019, SL-PLT-021, SL-PLT-023, SL-PLT-601 | R-17 (phase 6) | TC-INF-801 |
| 41 | Configuration as code | 1 / 1 surfaces | REQ-PLT-027 | CAP-PLT-02 (1) | SL-PLT-016 | R-18 (phase 1) | TC-PLT-804 |
| 42 | Mastery and next step | 2, degrades to the raw heatmap / 2 suggests | REQ-ASM-033 | CAP-ASM-02 (2) | SL-ASM-219 | R-19 (phase 2 for the heatmap; the suggestion from phase 5) | TC-ASM-811 |
| 43 | Workload balance for staff | 2, degrades to load totals / 2 suggests | REQ-HR-012 | CAP-HR-02 (5) | SL-HR-616, SL-HR-617 | R-20 (phase 5) | TC-HR-810 |
| 44 | Low-bandwidth mode | 1 / 1 surfaces | REQ-BFF-009 | CAP-MOB-02 (2) | SL-BFF-208, SL-MOB-218 | Minute 4 (phase 2; the data-saver profile from phase 4) | TC-MOB-005 |

**Reading the trace.** Forty-three rows. Every feature reaches at least one requirement, one capability and one slice, and every one has a step whose Test cell runs its demo test except feature 23, which Appendix O names and guards in every minute. Where a feature's step runs in an earlier phase than a capability in its row (features 2, 6, 7, 10, 12, 22, 30 and 35), the step shows what the earlier phase has built and the later capability extends the feature to another surface or service; for feature 30 the demo test itself joins the gate only in phase 5, as §3.2 says. Features 15 and 37, which open point 1 once said had no capability, are built in phase 1 by SL-PLT-006 and SL-PLT-022.

---

## 2. Positioning forced by document 02

Document 02 changed three of the claims Appendix P section P.2 made. The sales material states the corrected claim or nothing.

### 2.1 What is parity, and what is the edge

| Topic | What document 02 found | Parity (say it, do not lead with it) | The edge (lead with it, and prove it) | Proof |
|---|---|---|---|---|
| Arabic | PowerSchool (with right-to-left) [10], Classter [13], Classera [27] and Fedena [7] offer an Arabic interface; ManageBac offers Arabic to parents only [21]. None documents bilingual records, Arabic-aware search or Hijri | An Arabic, right-to-left interface | Bilingual data (every name and title stored in both languages), Arabic-aware search that finds a name however it was typed, Hijri and Gregorian calendars on every date, right-to-left printed documents with amounts in words | TC-L10N-202 (right-to-left class list), TC-L10N-401 (Arabic statement, currency placement), TC-L10N-801 (Hijri and Gregorian on a certificate), TC-L10N-311 (normalizer agreement over 5,000 names); minutes 4, 10 and 11 |
| Public API | Six of ten products publish one [10], [13], [16], [20], [22], [25], plus partial evidence for Fedena and Classera | A documented REST API with OAuth | iCal, OneRoster export, and read-only data access that survives suspension for non-payment; outgoing webhooks are parity, since PowerSchool documents data-event notifications to third-party applications [40] | TC-INT-001 |
| Open source | openSIS [1] and Gibbon [4] are GNU GPL; Fedena ships an open-source basic version [9] | "Open source" | A permissive licence (master brief Section 6), a modern multi-tenant stack with row-level isolation, a public API and a mobile application, none of which openSIS or Gibbon offers together: Gibbon has no general-purpose API [5], openSIS has an undocumented `/api` folder [1], neither has a mobile application | The licence scan in `19-dependency-and-license-inventory.md`; TC-SEC-101 and TC-SEC-501 for isolation |
| Emergency | Blackbaud [23], PowerSchool [35] and Classter [36] broadcast; Veracross integrates Ruvna for building attendance [37]; Gibbon prints evacuation absence reports [3] | Emergency broadcast; roll call through a third-party integration | Roll call by location, acknowledgement tracking and reunification with verified pickup in one offline-capable flow | Minute 13, TC-ATT-813, with reunification as the climax |
| Exception-only attendance | Fedena integrates biometric capture [8] | Capturing attendance from a device | Merging gate, bus and approved leave into the register before the teacher opens it, with conflict rules | Minute 2, TC-ATT-812 |
| Onboarding | Fedena performs onboarding and training as a vendor service on every plan [9] | Help getting started | Self-service elapsed time: inferred defaults the school can see and change, and an import with dry run and rollback | Minute 15 (TC-PLT-802); reserve R-03 (TC-DOC-001) |
| Deployment | openSIS and Gibbon self-host only; every commercial product fetched is cloud-only on its product page; Classter's on-premises option is a blog snippet [15] | Hosting | Laptop, single server and Kubernetes from one codebase | The appliance drill, CAP-INF-05 (6) |

### 2.2 Claims the sales material must stop making

| Stop saying | Why | Say instead |
|---|---|---|
| "The only school platform built for Arabic" or "fully Arabic" as a differentiator | Four products offer an Arabic interface; PowerSchool markets a Middle East and Africa edition with Saudi Arabian and Emirati references [10] | "Bilingual data, not a translated interface", followed by the search and Hijri proof |
| "Open API" or "open by default" as the headline difference | Six of ten products publish an API, and two more have partial evidence; PowerSchool also sends data-event notifications [40] | "iCal, OneRoster, and your data stays readable even if you stop paying"; name webhooks in the feature list, not as a difference |
| "The open-source school platform" or "unlike the others, we are open source" | openSIS and Gibbon are open source, and Fedena has an open-source edition | "Permissively licensed, modern, multi-tenant, with an API and a mobile app" |
| "Competitors have no emergency features" | Blackbaud [23], PowerSchool [35] and Classter [36] broadcast, and Veracross schools can add Ruvna for roll call [37] | "Nobody we checked documents reunification with verified pickup in the product" |
| "Nobody pre-fills attendance" | Fedena automates capture from biometric devices [8] | "We found no product that merges gate, bus and leave into the register before the teacher opens it" |
| "PowerSchool is weak outside North America" | Its regional edition is Arabic, right-to-left and referenced in the Gulf [10] | Show the five proofs (features 26, 27, 29, 31, 32), leading with what PowerSchool does not document: the three-source register merge, the rule version behind a figure, inferred defaults, the guardian's view of the access log, and reunification with verified pickup [32], [33], [35] |
| "No competitor works offline" | Only Toddle is verified as online-only [25]; PowerSchool has a desktop offline application [39]; eight are unverified | "Toddle requires a 3G connection; we mark attendance with the network off", then minute 3 |
| "openSIS has no multi-tenancy", "Gibbon has limited finance" | openSIS claims multiple institutions in one installation [1]; Gibbon has a Finance module for fees and invoices [3] | "Tenant isolation with row-level security that openSIS does not document"; do not raise Gibbon's finance |
| Any "nobody else has X" built on an unverified cell | Unverified is silence on a marketing page, not absence | "As of our check on 2026-09-26, no competitor documents X", until the trial re-check in document 02 resolves it |
| "Live, animated, modern" as a lead claim | Subjective and the easiest thing on the list to match | Let the demo show it; never claim it in writing |
| White-label apps or the plug-in kit as differences | Fedena sells branded apps [7]; PowerSchool and Classter have plug-in systems and marketplaces [10], [13] | Mention as parity in a feature list |

---

## 3. The demo mapped to the roadmap

Each Appendix O minute is listed with the capability that makes it demonstrable, the phase that capability lands in (`17-roadmap.md` §4), and whether the minute exists at the MVP cut line (`17-roadmap.md` §5: phases 1 and 2, plus CAP-RQS-01 for the attendance and document request types, CAP-COM-01, CAP-COM-02 and CAP-DOC-02). **Full** means every action in the minute works; **partial** means the minute runs with the substitution shown; **absent** means the minute is replaced.

### 3.1 Minute by minute

| Min | Features | Capability that makes it demonstrable | Latest phase needed | At the MVP cut line | What the presenter shows at the MVP |
|---|---|---|---|---|---|
| 1 | 25, 1 | CAP-RPT-01 (morning brief, cards); approvals through CAP-RQS-01 and the inbox in CAP-RQS-02 | 4 | Partial | The principal clears the pending attendance and document requests from the phone (CAP-RQS-01); no morning brief, because Reporting is phase 4 |
| 2 | 26, 34, 3 | CAP-ATT-01, CAP-MOB-01; gate scan through CAP-ATT-03; approved leave through CAP-RQS-01 | 3 (MVP subset) | Full | As scripted |
| 3 | 12 | CAP-MOB-01, CAP-ATT-01 | 2 | Full | As scripted |
| 4 | 35, 7, 44 | CAP-MOB-02, CAP-NOT-01; the excuse through CAP-ATT-02; the data-saver profile through CAP-MOB-03 (4) | 2; the data-saver profile 4 | Partial | As scripted, in Arabic, without the data-saver profile |
| 5 | 28, 30, 4 | Threshold rule and opened intervention in CAP-ATT-02 (2); the Because panel and the playbook library in CAP-RPT-03 (4); the general panel contract in CAP-AI-02 (5); the playbooks' own test TC-WEL-810 through CAP-WEL-02 (5) | 4 (5 for every automated action and for TC-WEL-810) | Partial | The attendance threshold flag shows its rule identifier and inputs and opens an intervention with an owner (CAP-ATT-02); no playbook library and no early-warning model |
| 6 | 5 | CAP-SCD-01 | 2 | Full | As scripted |
| 7 | 6 | CAP-ASM-01 | 2 | Full | As scripted |
| 8 | 6 | CAP-ASM-02 | 2 | Full | As scripted |
| 9 | 6, 14 | CAP-ASM-02 for the report-card QR; CAP-DOC-02 for the revoked certificate | 3 (in the MVP) | Full | As scripted |
| 10 | 1, 10 | CAP-FIN-01, CAP-FIN-02, CAP-FIN-05; the Arabic receipt through CAP-DOC-01 | 3 (outside the MVP) | Absent | Replaced by the bilingual-documents minute: an Arabic right-to-left class list (TC-L10N-202) and a transfer certificate in Arabic with its QR (TC-L10N-301), through CAP-DOC-01 and CAP-DOC-02 |
| 11 | 31, 14 | CAP-AUD-01, CAP-PRV-01; the guardian surface through CAP-BFF-01 | 1 (guardians on record from CAP-SCH-02, phase 2) | Full | As scripted, in Arabic |
| 12 | 14, 11 | CAP-IDN-02, CAP-AUD-01; the command palette through CAP-BFF-01; the wellbeing record in CAP-WEL-02 | 5 | Partial | The second refusal uses a custody-restricted student record (CAP-SCH-02) in place of a wellbeing record |
| 13 | 32, 8 | CAP-ATT-04, CAP-ATT-03 | 2 | Full | As scripted, ending on the verified pickup |
| 14 | 27, 1 | CAP-RPT-01; collections against target through CAP-FIN-02 | 4 | Absent | Replaced by reserve step R-01 (Arabic-aware search, §4.3) |
| 15 | 29, 15, 37 | CAP-PLT-01 (the reset is SL-PLT-006), CAP-IDN-03; the template exchange is SL-PLT-022 under CAP-PLT-02 | 1 | Full | As scripted |

**At the MVP cut line:** nine minutes full (2, 3, 6, 7, 8, 9, 11, 13, 15), four partial (1, 4, 5, 12), two replaced (10, 14). Of the five PowerSchool-undocumented proofs named by RISK-31 (features 26, 27, 29, 31, 32), four are demonstrable at the MVP; feature 27 (minute 14) waits for phase 4.

### 3.2 Which minutes each phase can show

| End of phase | Minutes full | Minutes partial or replaced | What `17-roadmap.md` §1 promises for that phase demo | Gap |
|---|---|---|---|---|
| 1 | 15 | all others not yet demonstrable | A tenant provisioned live, a second-factor sign-in, a notification, an audit entry | None: minute 15 matches |
| 2 | 2, 3, 6, 7, 8, 11, 13, 15; minute 9 for the report card only | 1, 4, 5, 12 partial; 9 without the revoked certificate; 10 and 14 replaced | Acts one and two of Appendix O | Minutes 1, 5 and 10 cannot run as scripted: Reporting is phase 4 and Finance is phase 3. Each runs the substitution in Appendix O "Before a minute's phase", and the demo gate runs that substitution's test (`16-test-strategy.md` part 12.2) |
| 3 | adds 9 in full and 10 | 1, 4, 5, 12 partial; 14 replaced | Act three of Appendix O | Minute 12 needs Wellbeing (phase 5) and minute 14 needs Reporting (phase 4) |
| 4 | adds 1, 4, 5 and 14 | 12 partial | The full script on a fresh tenant with one-click reset | Minute 12's wellbeing refusal still needs phase 5; the custody-restricted substitute keeps the minute honest |
| 5 | all fifteen | none | A clinic visit, a substitution, a library loan, a reviewed draft | None; TC-WEL-810 joins minute 5 in the demo gate |

The reserve steps follow the same rule: each joins the demo gate from the phase in its Appendix O row, the last, R-17, in phase 6.

---

## 4. The demo director's check

Appendix W admits a feature only if it can be shown on the demo data in under a minute, and Appendix O says a feature with neither a minute nor a reserve step fails the demo director's review. This section applies both rules to the register as it stands. Since ADR-0023 the second rule is mechanical: kit-lint R34 fails when a signature feature is not shown by a minute or reserve step whose Test cell runs its Appendix W demo test, and the `demo-director` agent reviews the choreography, the sixty seconds, the clicks and the seed data.

### 4.1 Minutes at risk of running past sixty seconds

| Min | Why it overruns | Fix | Proven by |
|---|---|---|---|
| 5 | Three features on two screens: the Because panel, an override with a reason, then the playbook | Open the playbook in the same side panel as the Because panel; state the override in one sentence without typing a reason; budget 30 seconds each for panel and playbook | TC-RPT-008 with a duration assertion of 60 seconds from flag to assigned owner |
| 10 | Five actions: batch preview, approval, split payer, payment, receipt | Run the batch preview before the meeting and show only its approval, the split payer and the Arabic receipt with the amount in words; move "record a payment" to reserve step R-15 | TC-FIN-001 split so the in-meeting part carries its own duration assertion; "record a payment" at R-15 is TC-FIN-551 |
| 10 | Feature 10, configurable without code, is claimed by a split-payer step that shows no configuration | Minute 10 keeps feature 10 and its Test cell runs the feature's own demo test, so the claim is measured rather than implied by the split payer | TC-PLT-805 in minute 10 (ADR-0023) |
| 11 | The full data export is a long-running operation | Show the request accepted with its progress, then open an export prepared at the start of the meeting | TC-AUD-002 |
| 13 | Roll call waits on real acknowledgements, and document 02 requires reunification to be the climax, not the broadcast | Seed the demo tenant so most acknowledgements arrive within ten seconds of the trigger; spend the second half of the minute on the verified pickup | TC-ATT-813 with the reunification step asserted last |
| 15 | CAP-PLT-01 promises provisioning "in minutes", which is longer than the minute | Show the saga's first steps live, then switch to a tenant provisioned at the start of the meeting, as minute 8 does for the batch; the smart-defaults screen is the part that must be live | TC-PLT-003 for the provisioning steps, timed separately from TC-PLT-802 for the defaults screen |

### 4.2 Signature features with no minute

Nineteen of the 43 signature features have no minute of their own. Eighteen are shown by a sixty-second step of the reserve bank (§4.3) whose Test cell runs the feature's own Appendix W demo test; the presenter swaps the step in for a specific buyer, and the demo gate runs it on every release like the fifteen minutes. The nineteenth, feature 23, is present in every minute. Three features this check once had to fold into a minute are now in the Feature cells of Appendix O and are counted with the twenty-four shown in a minute: 11 in minute 12, 37 in minute 15 and 44 in minute 4. A third route, **demote**, moving a feature from Appendix W to Appendix A by ADR because it cannot be shown to a buyer on demo data, was used once, for feature 39, and is listed below for the record.

| # | Feature | Step | How |
|---|---|---|---|
| 2 | Student 360 timeline | Reserve R-02 | Principal and teacher open the same child; the teacher's timeline shows only the entries the teacher's role may see (TC-RPT-002) |
| 9 | Go live in a day | Reserve R-03 | Dry run of a 600-row student file shows three problem rows; import; roll back (TC-DOC-001) |
| 13 | Data quality center | Reserve R-04 | Two guardians with the same national identifier surface with the rule that found them and a merge link (TC-RPT-004) |
| 16 | White-label mobile apps | Reserve R-05 | Two phones side by side with two schools' branding, one build (TC-MOB-002); presented as parity |
| 17 | Photo consent at publishing | Reserve R-06 | A class photo with one child lacking media consent is blocked at publish (TC-COM-001) |
| 18 | Student portfolio and recognition | Reserve R-07 | A recognition added by a teacher appears in the student's portfolio and in the family export (TC-BEH-810) |
| 19 | Kindergarten daily sheet | Reserve R-08 | Five taps on a tablet produce the daily sheet on the parent's phone (TC-ACA-810) |
| 20 | Balanced class formation | Reserve R-09 | One pass forms four balanced sections; one drag shows the balance change live (TC-SCH-810) |
| 21 | Inspection readiness | Reserve R-10 | The evidence folder for one criterion, already filled from the term's records (TC-RPT-005) |
| 22 | Policy acknowledgment | Reserve R-11 | A handbook sent to staff; the missing acknowledgments are listed and chased (TC-COM-002) |
| 23 | Live, animated interface | None; present in every minute | No step of its own; the four-way snapshots behind TC-UX-001, which Appendix O names, guard it, and the presenter never claims it in words |
| 24 | Open by default | Reserve R-12 | A webhook fires to a request inspector when attendance is marked, and a staff calendar updates by iCal (TC-INT-001) |
| 33 | Campus digital twin | Reserve R-13 | The floor plan shows free rooms for period 3 and books one (TC-OPS-810) |
| 36 | School memory: yearbook | Reserve R-14 | The yearbook draft for a grade, built from consented media only (TC-DOC-801) |
| 38 | Plug-in kit | Reserve R-16, IT lead only | A sample ministry-export plug-in installed and run on the demo tenant (TC-INT-002) |
| 39 | Calendar-aware scaling | **Demoted (ADR-0019)** | Not showable on demo data in a meeting, so it left Appendix W's signature list for the engineering capabilities in Appendix A, A1. It has no minute and no reserve step; the IT lead is shown the recorded 08:00 peak run measured by Appendix N scenarios N-01 and N-11. It is listed here for the record and is not one of the nineteen above |
| 40 | Self-healing operations | Reserve R-17, IT lead only | A worker is stopped, the queue backs up, the replay runs inside policy and the operator note appears (TC-INF-801) |
| 41 | Configuration as code | Reserve R-18, IT lead only | A settings export, a one-line diff, review, apply (TC-PLT-804) |
| 42 | Mastery and next step | Reserve R-19 | The standards heatmap for a class, with the rung 2 suggestion switched off to show the fallback (TC-ASM-811) |
| 43 | Workload balance | Reserve R-20 | A cover request shows the least-loaded eligible teacher and the load totals behind it (TC-HR-810) |

### 4.3 The reserve bank

The reserve bank is the table "The reserve bank" in Appendix O, R-01 to R-20, with the persona, the feature, the test and the phase of each step; this document does not restate it. Eighteen steps show the features of §4.2 and run their Appendix W demo tests. The other two run tests this document defines:

| Step | What it shows | Test | Runs from phase |
|---|---|---|---|
| R-01 | Arabic-aware search: a guardian's name typed with and without hamza or taa marbuta finds the same record, which is the "bilingual data" claim of §2.1 made visible | TC-L10N-311 | 2 (CAP-SCH-02, CAP-BFF-01) |
| R-15 | Record a payment and allocate it across two invoices, for feature 10, which minute 10 already shows | TC-FIN-551 | 3 (CAP-FIN-02) |

A buyer-specific demo swaps at most two reserve steps in for minutes of the same act. Each step joins the demo gate from the phase in its Appendix O row, and a feature's own demo test from the phase that builds the feature (`16-test-strategy.md` part 12.2).

**Result of the check.** Appendix O now scripts the fixes of the six scripted-minute problems in §4.1. Of the 43 signature features, twenty-four are shown in a minute, eighteen only in a reserve step, and feature 23 in every minute, guarded by TC-UX-001; feature 39 is no longer a signature feature (ADR-0019). Every step's Test cell runs the demo test of each feature it shows, and the demo gate runs every step whose phase has shipped at each phase exit and on every release, so every one of the 43 runs its demo test in the release gate from the phase that builds it (ADR-0023). Kit-lint R34 holds this on every change to Appendix O or Appendix W.

---

## 5. The pitch

### 5.1 School owner

You are choosing the system your families will judge you by for the next ten years. Every serious product in the region can show you an Arabic screen; we show you a school where every child's record exists in both languages, where a parent can see who looked at their child's file, and where every number on your dashboard opens to the records and the rule behind it. It runs on your own server or in the cloud from the same code, under a licence that does not lock you in, and your data stays readable even if you ever stop paying. **Proof: minute 11 (TC-AUD-002), a parent in Arabic seeing who read their child's sensitive records and downloading the full export.**

### 5.2 Principal

Your mornings go on chasing registers, approvals and the one child nobody noticed. Here the register is three-quarters filled before the teacher opens it, from the gate, the bus and the leave you already approved; the teacher confirms the exceptions in under twenty seconds, even with no signal in the corridor; and when a child starts slipping, the flag tells you why and turns into a plan with an owner. In an emergency every child is accounted for and handed over to a verified adult. **Proof: minute 2 (TC-ATT-812), a pre-filled register confirmed on a phone in under twenty seconds.**

### 5.3 IT lead

You need to know what happens on a bad day. Every tenant is isolated by row-level security and proven by a test on every build; every sensitive read and every refusal lands in an audit trail that cannot be quietly edited; the same code runs on a laptop, one server or Kubernetes, with a Windows appliance for on-premises upgrades; outgoing webhooks, iCal and OneRoster connect it to what you already run; and every component is under a permissive open-source licence. Nothing in the product needs a language model to work. **Proof: minute 12 (TC-SEC-530), a teacher's two out-of-scope requests refused with a clear message and visible in the audit viewer seconds later.**

---

## Decisions in force

| Decision | Source | Value | What breaks if it is ignored |
|---|---|---|---|
| Document 02's competitor facts win over Appendix P | `02-competitive-gap-analysis.md` "Corrections to Appendix P" | 21 corrections | Sales material repeats a claim a buyer can disprove on a vendor page |
| Unverified is never written as "no" | Document 02 method | As stated | A "nobody has it" claim collapses in the first competitive meeting |
| A claim is not spoken until its test passes | Appendix P section P.2 and document 02 | Every edge in §1 names a test case | The demo shows something the release gate never checked |
| The demo sells at rung 1; nothing at rung 3 or 4 is shown | Appendix O "What the demo deliberately does not do"; master brief Section 25 | As stated | A demo that needs a model fails in the school's meeting room |
| The sixty-second rule for every signature feature | Appendix W | As stated | Features accumulate that cannot be shown, and the register stops meaning anything |
| Feature 39 (calendar-aware scaling) is an engineering capability, not a signature feature | ADR-0019; Appendix W's "Feature 39 is no longer a signature feature" paragraph; Appendix A section A1 | 43 signature features; row 39 kept in Appendix W and in §1.1 so the numbering is stable | The demo script carries a feature that breaks its own sixty-second rule, and the signature count drifts from Appendix W |
| Every signature feature runs its own demo test in the release gate | ADR-0023; Appendix O; kit-lint R34; `16-test-strategy.md` part 12.2 | 24 features shown in a minute, 18 only in a reserve step, feature 23 guarded by TC-UX-001; the demo gate runs every step whose phase has shipped | A feature is claimed on stage and never measured, and a phase exit passes on a step that did not run |
| Phases and the MVP cut line | `17-roadmap.md` §1 and §5 | Phases 1 and 2 plus the named phase 3 subset | A buyer is promised a minute that does not exist at their start date |
| Rung and autonomy per feature | `25-ai-and-assist-ladder.md` §2 | As quoted in §1 | The matrix contradicts the ladder |

## Dependencies on other documents

| This document assumes | Stated in | Checked on |
|---|---|---|
| Competitor facts, sources and the re-check before general availability | `02-competitive-gap-analysis.md` | Every change to document 02 |
| The 43 signature features with moment, rung, autonomy, tier, owner and demo test, in 44 rows with row 39 marked as moved | Appendix W | Every lint run |
| The fifteen minutes, the substitutions before a minute's phase and the reserve bank, with their features, test cases and phases | Appendix O | Every lint run (R34); every phase exit and release, through the demo gate |
| The demo gate at each phase exit and release | `16-test-strategy.md` part 12.2; `15-deployment-and-operations.md` part 3 | Group E review |
| The P.4 column set | Appendix P section P.4 | Group F review |
| Capabilities, phases and the MVP cut line | `17-roadmap.md` §1, §4, §5 | Every lint run (R19) |
| Rung and autonomy per feature | `25-ai-and-assist-ladder.md` §2 | Group F review |
| RISK-31 and its mitigation | `18-risk-register.md` | Group F review |
| Localization tests TC-L10N-202, TC-L10N-301, TC-L10N-401, TC-L10N-801, TC-L10N-311 | Appendix Q, `24-localization-and-calendars.md` | Group F review |
| The demo tenant and its seed | Appendix H | Every release |
| The requirements behind each signature feature, their slices and the capability each slice sits under | `03-requirements-catalog.md`, `34-work-breakdown.md`, `17-roadmap.md` §4 | Every lint run (R19); the trace is re-derived at the Group F review and on every change to those documents |

## Open points

| Question | Default | Owner | Impact if the default is wrong | L | I | Score | In the register |
|---|---|---|---|---|---|---|---|
| 1. Closed: do features 15 (demo mode reset) and 37 (template exchange) have work planned? `34-work-breakdown.md` builds both in phase 1: SL-PLT-006 (REQ-PLT-028) under CAP-PLT-01 and SL-PLT-022 (REQ-PLT-023) under CAP-PLT-02, so each has a capability, a slice and its days | As built: feature 15 by SL-PLT-006 and feature 37 by SL-PLT-022, shown in minute 15 from phase 1; the "Signature feature trace" above carries both rows | Product owner | Only if either slice is cut from phase 1: minute 15 would then lose a step, which the demo gate reports at the phase 1 exit | 1 | 2 | 2 | none |
| 2. The split of feature 24 into a Tier 1 read-only API and Tier 2 webhooks, recommended by document 02, awaits an ADR. This is **Open Question 28** in `docs/project/OPEN_QUESTIONS.md` | The register's default: they stay where the roadmap builds them, as Tier 2 features. The IT-lead pitch names webhooks, iCal and OneRoster, which land in CAP-INT-01 (phase 3) and CAP-SCD-03 (phase 2) either way | Product owner, as an ADR | An IT lead at an MVP-stage evaluation asks for the API and it is phase 3 | 3 | 3 | 9 | RISK-42 |
| 3. Features 16 and 38 are already matched (Fedena branded apps; PowerSchool and Classter plug-ins and marketplace) | Keep them in Appendix W as parity features and never present them as differences | Product owner | The sales sheet claims a difference a buyer can disprove | 1 | 2 | 2 | RISK-31 |
| 4. Fourteen features are unverified in all ten competitors (1, 2, 15, 17, 23, 28, 29, 30, 33, 34, 35, 37, 40, 41), down from twenty-four after document 02's second check on 2026-09-26; that check rests on search snippets only, because every page fetch was refused by the checking session's egress policy | Resolve for PowerSchool, Classter and Classera by trial or scripted sales conversation in the document 02 re-check before general availability, fetch sources 32 to 54 in full, and update §1 under the same footnote discipline | Product owner, who runs the trials; the domain expert agent re-fetches the sources and drafts the cells | A "we found no competitor documenting it" claim turns out to be matched in a product nobody trialled | 3 | 3 | 9 | RISK-31 |

> L and I are the likelihood that the default is wrong and the impact if it is, on the 1 to 5 scales of `18-risk-register.md` Section 1. Score is L x I. A point that scores 12 or more names its RISK identifier in document 18; below that, the identifier if one covers it, or `none`. Kit-lint rules R24 and R33 check all of it (ADR-0022).

**Closed by ADR-0019 and ADR-0023 (brief v9.4).** Three points are no longer open. The reserve bank is a table of Appendix O, so its steps are part of the release gate by the brief, and R34 checks that each runs the demo test of the feature it shows (former point 1). Feature 36 has its own demo test, TC-DOC-801, and TC-DOC-002 stays minute 9's QR verification test (former point 3). Every Appendix O step states the phase it runs from, and "Before a minute's phase" names the substitution and test an earlier phase demo runs, so a phase exit gates exactly the steps whose phase has shipped (`16-test-strategy.md` part 12.2); the phase demo column of `17-roadmap.md` §1 is read through those Phase cells (former point 4). The remaining points are renumbered.

## Review record

| Date | Reviewer | Verdict | Blocking items |
|---|---|---|---|
| 2026-09-22 | Group F review, round 1 (independent adversarial scorecard) | Blocked: the group scored below 4 on Completeness, Consistency, Feasibility, Risk honesty, Testability and Distinctiveness | 23 of 44 features relied on reserve steps outside the release gate, feature 39 had no proof, and phase 2 minutes 1, 5 and 10 could not run as scripted (Distinctiveness); `TC-PLT-001` and `TC-DOC-002` each meant two things (Consistency, Testability) |
| 2026-09-26 | Scorecard remediation, theme 8 | Amended under ADR-0023: every sixty-second proof names the Appendix O step and the feature's own demo test; the counts recounted from Appendix O (24 in a minute, 18 only in a reserve step, feature 23 by TC-UX-001, feature 39 moved); §4.3 cites Appendix O's reserve bank; the former open points 1, 3 and 4 closed | none |
| 2026-09-26 | Group F review, round 2 | Blocked: the group scored below 4 on Completeness, Consistency, Risk honesty and Testability; Distinctiveness, the axis this document carries, scored 4 | The round 1 items were closed. Still open: open point 1 said features 15 and 37 had no capability although document 34 builds them in phase 1 (SL-PLT-006, SL-PLT-022) (Consistency); no signature feature carried its rung, requirements and slices in one place for documents 02 and 03 to cite |
| 2026-09-26 | Round 3 remediation | Amended; awaiting the round 3 score | The "Signature feature trace" table added: 43 rows with rung and autonomy, requirements, capabilities, slices, Appendix O step and demo test; open point 1 closed against SL-PLT-006 and SL-PLT-022; the §1 and §3.1 cells for features 15 and 37 name their slices |
| 2026-09-26 | Group F review, round 3 | Blocked: the group scored below 4 on Completeness, because document 31 listed no transition-test ids per workflow; Distinctiveness, the axis this document carries, scored 4 | None of the group's blocking gaps was in this document; twenty-four features unverified in all ten competitors (RISK-31, not blocking), and this record stopped at "awaiting the round 3 score" |
| 2026-09-26 | Group F review, round 4 | Blocked: the group scored below 4 on Consistency, because SL-ACA-207 in document 34 built `LaunchLtiTool` in phase 2 against the default of document 17; Distinctiveness scored 4 | None in this document; the document 02 re-check for PowerSchool, Classter and Classera still open (RISK-31, not blocking) |
| 2026-09-26 | Round-4 scorecard, Group F, then remediation round 5 | Amended; awaiting the round 5 score | The round 3 and round 4 verdicts recorded above; nothing else in this document changed |
| 2026-09-26 | Round-5 scorecard, remediation round 6 | Amended; awaiting the next score | RISK-31 residue: §1 now quotes document 02's second check (46 web calls, 23 snippet-only sources 32 to 54, every fetch refused by the egress policy). Ten features left "unverified in all ten" (4, 8, 11, 13, 19, 20, 25, 27, 31, 43) and feature 12 moved to partial; the totals are 3 yes, 25 partial, 1 no and 14 unverified in all ten. Webhooks are parity (PowerSchool), and the positioning and "stop saying" rows for the API, emergency, PowerSchool and offline follow. Open: the trial re-check and a full fetch of the snippet sources |

## How this document is verified

| Claim | Proof | Where it runs |
|---|---|---|
| Every Appendix W feature has exactly one matrix row with every P.4 column filled | The `demo-director` agent counts 44 rows across §1.1 and §1.2 against Appendix W, which likewise keeps row 39 marked as moved, and finds no empty P.4 cell (kit-lint R11 checks Appendix W's own register, not this matrix) | Group F review; every change to Appendix W or §1 |
| Every "who else has it" value is traceable to document 02 | Each yes, partial and no carries a document 02 footnote number; every other cell reads unverified | Domain expert review as a school buyer |
| Every signature feature is shown by a minute or reserve step whose Test cell runs its own demo test, and every step states its phase | Kit-lint R34 over Appendix W and Appendix O; feature 23's TC-UX-001 must be named in Appendix O | Every change under `docs/` |
| Every sixty-second proof runs | The demo gate runs the test cases of every Appendix O step whose phase has shipped, minutes and reserve bank, against the demo tenant, each feature's demo test from the phase that builds it (`16-test-strategy.md` part 12.2, REQ-TST-022); `15-deployment-and-operations.md` part 3 makes it promotion evidence | Every phase exit and release |
| Each minute fits in sixty seconds | The duration assertions named in §4.1, and the `demo-director` agent's review of the choreography with a timed dry run before each phase demo | Every phase demo |
| Every capability cited exists in the roadmap | `tools/kit-lint` rule R19 against `17-roadmap.md` §4 | Every change under `docs/` |
| The "Signature feature trace" is complete and current | kit-lint R19 fails on any requirement, capability or slice in it that documents 03, 17 or 34 do not define. That each row lists every requirement whose Source cites the feature's Section 12 item or names its demo test, and every slice whose Covers names one of them, is a review step: `plan-consistency-checker` re-derives the rows from documents 03, 17 and 34 and Appendices O and W at the Group F review and on every change to any of them, and a difference is a defect in this table | kit-lint on every change under `docs/`; the re-derivation at the Group F review |
| The MVP mapping matches the roadmap | `plan-consistency-checker` compares the phase §3 gives each capability with `17-roadmap.md` §5; kit-lint R19 fails on a `CAP-` identifier in §3 that document 17 does not define | The comparison on every change to either document; kit-lint on every change under `docs/` |
| No banned claim reaches the sales material | The sales sheet is reviewed against §2.2 before each release | Every release |
| This document agrees with the catalogs | `node tools/kit-lint/kit-lint.mjs .` for section and appendix references, identifiers and open markers | Every change under `docs/` |
