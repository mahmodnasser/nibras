# 32. Product Differentiation and Demo

> Plan document for the Nibras platform. Group F. It completes the differentiation matrix of Appendix P section P.4 for the 43 signature features in Appendix W, keeping Appendix W's row 39 so the numbering is stable, and maps the fifteen-minute demo of Appendix O onto the phases and capabilities of `17-roadmap.md`. It refines master brief Section 4 (a school owner must see the value in fifteen minutes), Section 12 (the signature features) and Section 26 (the competitive gap analysis); it does not re-derive them. Competitor facts are quoted from `02-competitive-gap-analysis.md`, whose findings win over Appendix P wherever the two differ (document 02 lists 21 corrections). Where this document and the brief disagree, an ADR records the deviation.

**Group** F · **Requirement areas covered** none directly; this document governs what the product claims to a buyer and how each claim is proven · **Last updated** 2026-09-22 by the platform plan

## Purpose

This document lets a salesperson, a demo presenter and a product owner make only claims the product can prove. For each of the 43 signature features it states who else has something comparable (as found on 2026-09-20, never guessed), the concrete edge, the moment the user would describe, the sixty-second proof, the capability that builds it, and the damage if a competitor catches up. It then states which claims the sales material must stop making, which demo minute exists at which phase, which signature features have no sixty-second proof yet and how each is fixed, and the one-paragraph pitch for the three buyers who sign. The readers are the product owner, the demo director, the domain expert reviewing as a school buyer, and whoever writes the sales sheet.

## Scope

| In scope | Out of scope | Where the out-of-scope item lives |
|---|---|---|
| The P.4 matrix, one row per Appendix W feature | Re-checking competitor facts on the web | `02-competitive-gap-analysis.md` and its re-check before general availability |
| Claims the sales material may and may not make | Pricing and plan tiers | Master brief Section 22, `06-services/platform.md` |
| Each Appendix O minute mapped to a capability and a phase | The capabilities themselves and their weeks | `17-roadmap.md` §4 |
| The demo director's check of every feature against the sixty-second rule | The end-to-end test code behind each test case | `16-test-strategy.md` |
| Three buyer pitches, each with one proof | Marketing copy, brand, website | Outside the kit |
| Rung and autonomy per feature, quoted | Assigning rung and autonomy | `25-ai-and-assist-ladder.md` §2 |

**How to read "who else has it".** Values are those of document 02: **yes** (a vendor page states it), **partial** (a narrower form exists), **no** (the vendor says so, or the product's shape rules it out), **unverified** (no page fetched on 2026-09-20 states it either way). Bracketed numbers are document 02's source footnotes. A feature that document 02's ten-capability matrix does not cover is **unverified** unless its field table names a comparable module, in which case that module is quoted. Unverified is not "no": a sales claim built on an unverified cell says "we found no competitor documenting it", never "nobody has it".

---

## 1. The completed differentiation matrix

Columns follow Appendix P section P.4, with two added: the rung and autonomy from `25-ai-and-assist-ladder.md` §2 (rung 1 at autonomy 1 unless stated), and the capability from `17-roadmap.md` §4 with its phase. **Proof** is the Appendix O minute and its test case; where the feature has no minute, the cell says "not in the fifteen-minute script" and gives the feature's own Appendix W test case. **Risk** is Low, Medium or High for the damage if a competitor matches the feature before launch, with the reason.

### 1.1 Tier 1 features

| # | Feature | Who else has it (document 02) | Our edge | Persona moment | Rung / autonomy | Sixty-second proof | Capability (phase) | Risk if matched |
|---|---|---|---|---|---|---|---|---|
| 1 | Today dashboards where every card leads to an action | unverified in all ten | Every card carries its action button, so the approval, the unmarked class or the overdue invoice is completed on the card, not in a module | "I can see what needs me and do it here" | 1 / 1 | Minute 1 (TC-RPT-006), minute 14 (TC-RPT-007); own test TC-RPT-001 | CAP-RPT-01 (4) | Medium: dashboards are universal; the action-per-card discipline is easy to copy shallowly |
| 2 | Student 360 timeline filtered by the viewer's permissions | unverified in all ten; Veracross markets a "single-record architecture" [16] | One timeline per child where each viewer sees only the entries their role and data scope allow | "The whole child, on one screen" | 1 / 1 | Not in the fifteen-minute script; TC-RPT-002 | CAP-SCH-02 (2), CAP-BFF-01 (1) | Medium: Veracross's single record is the closest match |
| 3 | Sixty-second attendance | partial: Fedena's mobile application marks attendance [31]; openSIS [1] and Toddle [25] have attendance; speed is documented by none | A section's register is taken on a phone in under sixty seconds, measured against the performance budget in master brief Section 19 | "Done before the bell stopped" | 1 / 1 | Minute 2 (TC-ATT-003); own test TC-ATT-001 | CAP-ATT-01 (2) | Low: speed is a budget enforced by a test, which a competitor has to rebuild for |
| 4 | Early warning with explanation and an intervention | unverified in all ten | The flag names its factors and their weights and opens a playbook with an owner; with the model off it falls back to named threshold rules | "I know who is slipping and what to do" | 2 (degrades to rule thresholds) / 2 | Minute 5 (TC-RPT-008); own test TC-RPT-003 | CAP-RPT-03 (4); thresholds in CAP-ATT-02 (2) | Medium: predictive flags are common in enterprise student systems; the explanation and fallback are not documented by any |
| 5 | Smart timetable with live conflict detection | partial: Fedena automatic timetable, Ultimate tier [7]; openSIS scheduling [1]; PowerSchool "two decades of scheduling" [10]; Toddle timetable [25]; live conflict highlighting unverified | A dragged period shows its conflicts as it moves, and publishing leaves recorded attendance untouched and says so | "A term's work in an afternoon" | 1 / 1 | Minute 6 (TC-SCD-001) | CAP-SCD-01 (2) | High: timetabling is where PowerSchool and Fedena are strongest; compete on the publish-safety message, not on the solver |
| 6 | Report Card Studio with QR verification | partial: report cards and transcripts in openSIS [1]; progress reports in Toddle [25]; QR verification unverified in all ten | Keyboard-speed mark entry, a live batch counter, and a QR on every card that a public page verifies in both languages without a login | "Eight hundred report cards, verified" | 1, comments at 3 / 3 for comments | Minutes 7, 8 (TC-ASM-810), minute 9 (TC-DOC-002) | CAP-ASM-01, CAP-ASM-02 (2); CAP-DOC-02 (3) | Medium: report cards are parity; public QR verification is cheap to copy |
| 7 | Parent experience that respects attention | partial: Toddle family communication [25]; PowerSchool mobile application for grades and progress [10]; digesting and quiet hours unverified | Notifications are merged into one calm message per child per window, in the parent's language, with quiet hours | "One calm message, not twelve" | 1 / 1 | Minute 4 (TC-MOB-004); own test TC-NOT-001 | CAP-NOT-01 (1), CAP-MOB-02 (2) | Medium: every vendor has a parent app; restraint is a design choice anyone can make |
| 8 | Safety and dismissal with gate passes | unverified in all ten | Pickup persons, gate passes and early dismissal are verified at the gate, and the record shows who collected the child | "I know who collected the child" | 1 / 1 | Minute 13 (TC-ATT-813), reunification step; own test TC-ATT-811 | CAP-ATT-03 (2) | Medium: dismissal is a known product category outside school management systems |
| 9 | Go live in a day: import with dry run and rollback | partial: openSIS bulk import [1]; Fedena performs onboarding data and configuration as a vendor service on every plan [9]; dry run and rollback unverified | The school imports its own data with a dry run that shows every problem row, and a rollback if the result is wrong, without a services engagement | "We moved in a morning" | 1 / 1 | Not in the fifteen-minute script; TC-DOC-001 | CAP-DOC-03 (3) | High: Fedena's human onboarding competes directly; the claim must be elapsed time and self-service |
| 10 | Configurable without code | partial: Fedena form builder, Premium tier [7]; configurability depth undocumented for all | A school changes fee plans, grading scales, workflows and forms itself, and every change is versioned | "We changed it ourselves" | 1 / 1 | Minute 10 (TC-FIN-001), split payer; own test TC-PLT-001 | CAP-PLT-02 (1) | Medium: every vendor claims configurability; only a live change proves it |
| 11 | Command palette and natural-language search | unverified in all ten | One keystroke reaches any permitted screen or record, with Arabic-aware matching; natural language only at rung 3 | "I typed what I wanted" | 1, natural language at 3 / 1 | Not in the fifteen-minute script; TC-WEB-001 | CAP-BFF-01 (1); natural language in CAP-AI-01 (5) | Low: easy to copy, but Arabic normalization under it is not |
| 12 | Offline-first mobile | no: Toddle requires "3G+" [25]; unverified for the other nine | Attendance is marked with no network, queued visibly, and never claimed complete while queued | "The corridor has no signal and it still worked" | 1 / 1 | Minute 3 (TC-MOB-001) | CAP-MOB-01, CAP-ATT-01 (2) | Low: offline sync with visible conflict rules is expensive to retrofit |
| 13 | Data quality center | unverified in all ten | Wrong or missing records surface themselves with the rule that found them and a fix link | "The list was wrong and the system told us" | 1 / 1 | Not in the fifteen-minute script; TC-RPT-004 | CAP-RPT-03 (4) | Low |
| 14 | Trust by design: audit, consent, export, erasure | partial: Fedena audit module, Ultimate tier [7]; consent, export and erasure unverified | An append-only audit trail, recorded consents, a full export for a guardian and erasure under retention rules, all in the product | "We can answer any question about any record" | 1 / 1 | Minute 9 (TC-DOC-002), minute 11 (TC-AUD-002), minute 12 (TC-SEC-530); own test TC-AUD-001 | CAP-AUD-01, CAP-PRV-01 (1) | Medium: audit logs are common; the guardian-facing half is not |
| 15 | Sales-ready demo mode with one-click reset | unverified in all ten | The demo tenant resets to its seed in one click, and the demo runs on ordinary accounts | "Show it to the next school in ten minutes" | 1 / 1 | Minute 15 (TC-PLT-003); own test TC-PLT-002 | CAP-PLT-01 (1); one-click reset is a phase 4 exit criterion | Low: internal sales tooling, invisible to buyers |
| 17 | Photo and media consent enforced at publishing | unverified in all ten | Publishing a photo checks each tagged child's media consent and blocks the post, instead of relying on staff memory | "That child's photo cannot be posted" | 1 / 1 | Not in the fifteen-minute script; TC-COM-001 | CAP-COM-01 (3) | Low |
| 23 | Live, animated, modern interface | unverified in all ten (subjective; no page states it) | Real-time updates, motion and both directions and themes on every screen, from one design system | "It does not feel like school software" | 1 / 1 | Not in the fifteen-minute script as its own step; TC-UX-001 | CAP-UX-01 (1) | High: the most easily matched claim on the list; never lead with it |
| 25 | Morning brief per role | unverified in all ten | Each role opens to its day assembled from every service, with fixed bilingual sentences at rung 1 | "I knew my day before I arrived" | 1, phrasing at 3 / 1 | Minute 1 (TC-RPT-006) | CAP-RPT-01 (4) | Medium: a digest is easy to copy shallowly |
| 26 | Exception-only attendance | partial: Fedena biometric capture [8]; register pre-fill unverified for all | Gate scan, bus boarding and approved leave are merged into the register before the teacher opens it, with conflict rules | "The register was already half filled" | 1 / 4 (pre-fill, undone by one tap) | Minute 2 (TC-ATT-003) | CAP-ATT-01 (2), approved leave through CAP-RQS-01 (3, MVP subset) | Low: the three-source merge and its conflict rules are the moat, not gate capture |
| 27 | Explain this number | unverified in all ten; document 02 found no page for any product describing it | Every figure drills to the records and the rule version that produced it | "I could show the parent exactly why" | 1 / 1 | Minute 14 (TC-RPT-007) | CAP-RPT-01 (4) | Medium: easy to imitate shallowly, hard to imitate completely |
| 28 | Because panel on every automated action | unverified in all ten; Toddle markets AI drafting with no stated fallback [25] | Every automated action shows its rule identifier, inputs and an override with a recorded reason | "It told me why, and I could disagree" | 1 / 1 | Minute 5 (TC-RPT-008) | CAP-RPT-03 (4), CAP-AI-02 (5) | Medium |
| 29 | Smart defaults engine at onboarding | unverified in all ten; Fedena's answer is vendor staff [9] | Country and school type infer the configuration areas, and the screen shows what was inferred and why | "It already knew how our year works" | 1 / 3 | Minute 15 (TC-PLT-003) | CAP-PLT-01 (1) | Medium: competes with a human service, not a wizard |
| 30 | Intervention playbooks | unverified in all ten | A flag becomes a plan from a library with an owner and a review date, proposed and never auto-assigned | "A flag became a plan with an owner" | 1 / 3 | Minute 5 (TC-RPT-008); own test TC-WEL-001 | CAP-ATT-02 (2), CAP-RPT-03 (4) | Low |
| 31 | Guardian transparency on sensitive access | unverified in all ten; document 02 found no guardian-visible access log | A parent sees which roles read their child's sensitive records and when, with consents and the retention clock | "I can see who looked at my child's file" | 1 / 1 | Minute 11 (TC-AUD-002) | CAP-AUD-01, CAP-PRV-01 (1) | Low: it needs an access log most products do not keep |
| 32 | Emergency mode with reunification | partial: Blackbaud Emergency Bulletin, broadcast only [23]; Gibbon evacuation absence reports [3]; roll call and reunification found nowhere | Roll call by location, streamed acknowledgements, the shrinking "not accounted for" list and verified pickup in one flow, offline-capable | "Every child accounted for in four minutes" | 1 / 1 | Minute 13 (TC-ATT-813) | CAP-ATT-04 (2) | Medium: broadcast is matched; reunification is not |
| 34 | Teacher five-minute mode | unverified in all ten | The teacher's phone opens to the current class with four actions and nothing else | "Four taps and I was teaching" | 1 / 1 | Minute 2 (TC-ATT-003); own test TC-MOB-003 | CAP-MOB-01 (2) | Medium: a focused mode is easy to copy |
| 35 | Parent calm screen | unverified in all ten | One card per child that says plainly when nothing needs the parent | "Nothing needed me today, and it said so" | 1 / 1 | Minute 4 (TC-MOB-004) | CAP-MOB-02 (2) | Medium |
| 39 | Calendar-aware scaling — **no longer a signature feature (ADR-0019)**; an engineering capability in Appendix A, A1, still built and still measured | unverified in all ten (PowerSchool and Classter are hosted on Azure [10], [13]; scaling policy undocumented) | Capacity is raised ahead of the 08:00 attendance peak from the school calendar, not after the load arrives | None, as Appendix W row 39 now records | 1 / 1 | None. It has no minute and no reserve step; Appendix N scenarios N-01 and N-11 measure it | CAP-PERF-02 (6) | Low: invisible to buyers; a reliability property |
| 40 | Self-healing operations | unverified in all ten | Failed messages are replayed inside a written policy, with an audit entry and a note to the operator | "It fixed itself and told us what it did" | 1 / 4 | Not in the fifteen-minute script; TC-INF-002 | CAP-MSG-01 (1), CAP-INF-04 (6) | Low |
| 41 | Configuration as code | unverified in all ten | A settings change is exported, reviewed and applied as a versioned file, with a diff before it goes live | "We reviewed the change before it went live" | 1 / 1 | Not in the fifteen-minute script; TC-PLT-804 | CAP-PLT-02 (1) | Low |
| 44 | Low-bandwidth mode | no: Toddle requires "3G+" [25]; no page mentions a data-saver mode; unverified for the other nine | A data-saver profile for the phones and plans parents actually own | "It worked on my old phone" | 1 / 1 | Not in the fifteen-minute script; TC-MOB-005 | CAP-MOB-03 (4) | Low |

### 1.2 Tier 2 features

| # | Feature | Who else has it (document 02) | Our edge | Persona moment | Rung / autonomy | Sixty-second proof | Capability (phase) | Risk if matched |
|---|---|---|---|---|---|---|---|---|
| 16 | White-label mobile apps | yes: Fedena branded iOS and Android applications as an add-on [7] | Branding per school from one codebase and one release train | "It is our school's app" | 1 / 1 | Not in the fifteen-minute script; TC-MOB-002 | CAP-MOB-03 (4), CAP-PLT-02 (1) | High: already matched; parity, not an edge |
| 18 | Student portfolio and recognition | partial: Toddle portfolios [25] | Achievements, recognition and work samples collected per student and exportable by the family | "Everything they achieved, theirs to keep" | 1 / 1 | Not in the fifteen-minute script; TC-BEH-001 | CAP-BEH-01 (4) | High: Toddle is strong here |
| 19 | Kindergarten daily sheet | unverified in all ten | Meals, naps and activities recorded in taps and sent as one daily sheet per child | "I know how the day went" | 1 / 1 | Not in the fifteen-minute script; TC-ACA-001 | CAP-ACA-03 (4) | Medium: dedicated nursery apps exist outside the ten |
| 20 | Balanced class formation | unverified in all ten | Classes balanced on gender, ability, needs and separations in one pass, then adjusted by hand with the balance shown live | "Fair classes in one pass, then adjusted by hand" | 1 / 1 | Not in the fifteen-minute script; TC-SCH-810 | CAP-SCH-04 (2) | Low |
| 21 | Inspection and accreditation readiness | partial: Toddle accreditation workflows [25] | Evidence is tagged to inspection criteria as it is created, so the folder fills itself | "The evidence folder filled itself" | 1 / 1 | Not in the fifteen-minute script; TC-RPT-005 | CAP-RPT-02 (4) | High: Toddle leads on accreditation for IB schools |
| 22 | Policy and handbook acknowledgment | partial: Classter lists Protocols and Signatures modules [14]; acknowledgment proof unverified | Every recipient's acknowledgment is recorded against the policy version, with a chase for the missing ones | "Everyone signed, and we can prove it" | 1 / 1 | Not in the fifteen-minute script; TC-COM-002 | CAP-COM-01 (3) | Medium |
| 24 | Open by default: API, webhooks, iCal, standards | yes for an API: PowerSchool [10], Classter [13], Veracross [16], ManageBac [20], Blackbaud [22], Toddle [25]; partial: Fedena [8], Classera [29]; webhooks: no for Gibbon [6], unverified for the rest; OneRoster: Veracross [17], Toddle [26] | Outgoing webhooks, iCal, OneRoster and a full export that stay available read-only before suspension; the API alone is parity | "It talks to what we already use" | 1 / 1 | Not in the fifteen-minute script; TC-INT-001 | CAP-INT-01 (3), iCal in CAP-SCD-03 (2) | High for the API (already matched); Low for webhooks and read-only access |
| 33 | Campus digital twin | unverified in all ten | A live floor plan of rooms, occupancy and bookings, drawn from the timetable and bookings | "I saw the empty rooms at a glance" | 1 / 1 | Not in the fifteen-minute script; TC-OPS-001 | CAP-OPS-03 (5), bookings in CAP-SCD-03 (2) | Low |
| 36 | School memory: portfolio and yearbook | partial: Toddle portfolios [25]; a generated yearbook unverified | The yearbook is assembled from the year's consented photos, events and recognition, and rendered in both languages | "The yearbook made itself" | 1 / 1 | Not in the fifteen-minute script; TC-DOC-002 (see open point 3) | CAP-BEH-01 (4), CAP-DOC-01 (1) | Low |
| 37 | Template exchange between schools | unverified in all ten; Classter's marketplace [13] is for integrations, not templates | A school publishes a timetable, report card or policy template and another starts from it | "We started from someone else's good work" | 1 / 1 | Not in the fifteen-minute script; TC-PLT-803 | CAP-PLT-02 (1); no capability names the exchange (open point 2) | Medium |
| 38 | Plug-in kit for regional integrations | yes: PowerSchool Plugin System [10], Classter 40+ integrations and marketplace [13]; partial: Toddle 50+ integrations [26], Veracross 200+ partners [16] | A partner builds a ministry export or a GPS adapter against a published contract, without a Nibras release | "Our partner built the ministry export" | 1 / 1 | Not in the fifteen-minute script; TC-INT-002 | CAP-INT-01 (3) | High: already matched by the two strongest regional incumbents |
| 42 | Mastery and next step | partial: ManageBac standards across IB, IGCSE, AP and 600+ others [19]; Toddle curriculum maps and assessment [25]; next-step suggestion unverified | A standards heatmap per class with a suggested next teaching step, and the heatmap alone when the model is off | "I saw the gap and what to teach next" | 2 (degrades to the raw heatmap) / 2 | Not in the fifteen-minute script; TC-ASM-002 | CAP-ASM-01 (2); the suggestion through CAP-AI-01 (5) | High: ManageBac and Toddle own curriculum depth |
| 43 | Workload balance for staff | unverified in all ten | Teaching load, cover and duties per teacher with a flag before overload, staff data only | "Someone noticed before I burned out" | 2 (degrades to load totals) / 2 | Not in the fifteen-minute script; TC-HR-810 | CAP-SCD-02 (2), CAP-HR-02 (5) | Low |

**Totals.** 44 rows: 31 at Tier 1 and 13 at Tier 2, matching Appendix W row for row. Appendix W keeps row 39 but marks it as moved — under ADR-0019 calendar-aware scaling is an engineering capability in Appendix A, A1 — so the numbering is stable at 44 while **43 are signature features**. Appendix P section P.4 drops the row entirely; this document follows Appendix W and keeps it, marked, so a reader looking for feature 39 finds where it went. Of those 43, twenty-one have a minute in Appendix O and twenty-two do not (§4). Against document 02: 3 features are **yes** somewhere else (16, 38, and 24 for its API half), 14 are **partial** (3, 5, 6, 7, 9, 10, 14, 18, 21, 22, 26, 32, 36, 42), 2 are **no** on the one product that documents it (12 and 44, Toddle) and unverified on the other nine, and 24 are **unverified** in all ten. The five features document 02 and RISK-31 in `18-risk-register.md` name as undocumented by PowerSchool (26, 27, 29, 31, 32) all have a minute.

---

## 2. Positioning forced by document 02

Document 02 changed three of the claims Appendix P section P.2 made. The sales material states the corrected claim or nothing.

### 2.1 What is parity, and what is the edge

| Topic | What document 02 found | Parity (say it, do not lead with it) | The edge (lead with it, and prove it) | Proof |
|---|---|---|---|---|
| Arabic | PowerSchool (with right-to-left) [10], Classter [13], Classera [27] and Fedena [7] offer an Arabic interface; ManageBac offers Arabic to parents only [21]. None documents bilingual records, Arabic-aware search or Hijri | An Arabic, right-to-left interface | Bilingual data (every name and title stored in both languages), Arabic-aware search that finds a name however it was typed, Hijri and Gregorian calendars on every date, right-to-left printed documents with amounts in words | TC-L10N-202 (right-to-left class list), TC-L10N-401 (Arabic statement, currency placement), TC-L10N-801 (Hijri and Gregorian on a certificate), TC-L10N-311 (normalizer agreement over 5,000 names); minutes 4, 10 and 11 |
| Public API | Six of ten products publish one [10], [13], [16], [20], [22], [25], plus partial evidence for Fedena and Classera | A documented REST API with OAuth | Outgoing webhooks (no competitor documents them), iCal, OneRoster export, and read-only data access that survives suspension for non-payment | TC-INT-001 |
| Open source | openSIS [1] and Gibbon [4] are GNU GPL; Fedena ships an open-source basic version [9] | "Open source" | A permissive licence (master brief Section 6), a modern multi-tenant stack with row-level isolation, a public API and a mobile application, none of which openSIS or Gibbon offers together: Gibbon has no general-purpose API [5], openSIS has an undocumented `/api` folder [1], neither has a mobile application | The licence scan in `19-dependency-and-license-inventory.md`; TC-SEC-101 and TC-SEC-501 for isolation |
| Emergency | Blackbaud has a built-in broadcast [23]; Gibbon prints evacuation absence reports [3] | Emergency broadcast | Roll call by location, acknowledgement tracking and reunification with verified pickup in one offline-capable flow | Minute 13, TC-ATT-813, with reunification as the climax |
| Exception-only attendance | Fedena integrates biometric capture [8] | Capturing attendance from a device | Merging gate, bus and approved leave into the register before the teacher opens it, with conflict rules | Minute 2, TC-ATT-003 |
| Onboarding | Fedena performs onboarding and training as a vendor service on every plan [9] | Help getting started | Self-service elapsed time: inferred defaults the school can see and change, and an import with dry run and rollback | Minute 15 (TC-PLT-003); TC-DOC-001 |
| Deployment | openSIS and Gibbon self-host only; every commercial product fetched is cloud-only on its product page; Classter's on-premises option is a blog snippet [15] | Hosting | Laptop, single server and Kubernetes from one codebase | The appliance drill, CAP-INF-05 (6) |

### 2.2 Claims the sales material must stop making

| Stop saying | Why | Say instead |
|---|---|---|
| "The only school platform built for Arabic" or "fully Arabic" as a differentiator | Four products offer an Arabic interface; PowerSchool markets a Middle East and Africa edition with Saudi Arabian and Emirati references [10] | "Bilingual data, not a translated interface", followed by the search and Hijri proof |
| "Open API" or "open by default" as the headline difference | Six of ten products publish an API, and two more have partial evidence | "Webhooks, iCal, OneRoster, and your data stays readable even if you stop paying" |
| "The open-source school platform" or "unlike the others, we are open source" | openSIS and Gibbon are open source, and Fedena has an open-source edition | "Permissively licensed, modern, multi-tenant, with an API and a mobile app" |
| "Competitors have no emergency features" | Blackbaud has an Emergency Bulletin [23] | "Nobody we checked documents roll call and reunification with verified pickup" |
| "Nobody pre-fills attendance" | Fedena automates capture from biometric devices [8] | "We found no product that merges gate, bus and leave into the register before the teacher opens it" |
| "PowerSchool is weak outside North America" | Its regional edition is Arabic, right-to-left and referenced in the Gulf [10] | Name the five PowerSchool-undocumented proofs (features 26, 27, 29, 31, 32) and show them |
| "No competitor works offline" | Only Toddle is verified as online-only [25]; nine are unverified | "Toddle requires a 3G connection; we mark attendance with the network off", then minute 3 |
| "openSIS has no multi-tenancy", "Gibbon has limited finance" | openSIS claims multiple institutions in one installation [1]; Gibbon has a Finance module for fees and invoices [3] | "Tenant isolation with row-level security that openSIS does not document"; do not raise Gibbon's finance |
| Any "nobody else has X" built on an unverified cell | Unverified is silence on a marketing page, not absence | "As of our check on 2026-09-20, no competitor documents X", until the trial re-check in document 02 resolves it |
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
| 4 | 35, 7 | CAP-MOB-02, CAP-NOT-01; the excuse through CAP-ATT-02 | 2 | Full | As scripted, in Arabic |
| 5 | 28, 30, 4 | Threshold rule and opened intervention in CAP-ATT-02 (2); the Because panel and the playbook library in CAP-RPT-03 (4); the general panel contract in CAP-AI-02 (5) | 4 (5 for every automated action) | Partial | The attendance threshold flag shows its rule identifier and inputs and opens an intervention with an owner (CAP-ATT-02); no playbook library and no early-warning model |
| 6 | 5 | CAP-SCD-01 | 2 | Full | As scripted |
| 7 | 6 | CAP-ASM-01 | 2 | Full | As scripted |
| 8 | 6 | CAP-ASM-02 | 2 | Full | As scripted |
| 9 | 6, 14 | CAP-ASM-02 for the report-card QR; CAP-DOC-02 for the revoked certificate | 3 (in the MVP) | Full | As scripted |
| 10 | 1, 10 | CAP-FIN-01, CAP-FIN-02, CAP-FIN-05; the Arabic receipt through CAP-DOC-01 | 3 (outside the MVP) | Absent | Replaced by the bilingual-documents minute: an Arabic right-to-left class list (TC-L10N-202) and a transfer certificate in Arabic with its QR (TC-L10N-301), through CAP-DOC-01 and CAP-DOC-02 |
| 11 | 31, 14 | CAP-AUD-01, CAP-PRV-01; the guardian surface through CAP-BFF-01 | 1 (guardians on record from CAP-SCH-02, phase 2) | Full | As scripted, in Arabic |
| 12 | 14 | CAP-IDN-02, CAP-AUD-01; the wellbeing record in CAP-WEL-02 | 5 | Partial | The second refusal uses a custody-restricted student record (CAP-SCH-02) in place of a wellbeing record |
| 13 | 32, 8 | CAP-ATT-04, CAP-ATT-03 | 2 | Full | As scripted, ending on the verified pickup |
| 14 | 27, 1 | CAP-RPT-01; collections against target through CAP-FIN-02 | 4 | Absent | Replaced by reserve step R-01 (Arabic-aware search, §4.3) |
| 15 | 29, 15 | CAP-PLT-01, CAP-IDN-03 | 1 | Full | As scripted |

**At the MVP cut line:** ten minutes full (2, 3, 4, 6, 7, 8, 9, 11, 13, 15), three partial (1, 5, 12), two replaced (10, 14). Of the five PowerSchool-undocumented proofs named by RISK-31 (features 26, 27, 29, 31, 32), four are demonstrable at the MVP; feature 27 (minute 14) waits for phase 4.

### 3.2 Which minutes each phase can show

| End of phase | Minutes full | Minutes partial or replaced | What `17-roadmap.md` §1 promises for that phase demo | Gap |
|---|---|---|---|---|
| 1 | 15 | all others not yet demonstrable | A tenant provisioned live, a second-factor sign-in, a notification, an audit entry | None: minute 15 matches |
| 2 | 2, 3, 4, 6, 7, 8, 11, 13, 15; minute 9 for the report card only | 1, 5, 12 partial; 9 without the revoked certificate; 10 and 14 replaced | Acts one and two of Appendix O | Minutes 1, 5 and 10 cannot run as scripted: Reporting is phase 4 and Finance is phase 3 (open point 4) |
| 3 | adds 9 in full and 10 | 1, 5, 12 partial; 14 replaced | Act three of Appendix O | Minute 12 needs Wellbeing (phase 5) and minute 14 needs Reporting (phase 4) |
| 4 | adds 1, 5 and 14 | 12 partial | The full script on a fresh tenant with one-click reset | Minute 12's wellbeing refusal still needs phase 5; the custody-restricted substitute keeps the minute honest |
| 5 | all fifteen | none | A clinic visit, a substitution, a library loan, a reviewed draft | None |

---

## 4. The demo director's check

Appendix W admits a feature only if it can be shown on the demo data in under a minute, and Appendix O says a feature with no row fails the demo director's review. This section applies both rules to the register as it stands.

### 4.1 Minutes at risk of running past sixty seconds

| Min | Why it overruns | Fix | Proven by |
|---|---|---|---|
| 5 | Three features on two screens: the Because panel, an override with a reason, then the playbook | Open the playbook in the same side panel as the Because panel; state the override in one sentence without typing a reason; budget 30 seconds each for panel and playbook | TC-RPT-008 with a duration assertion of 60 seconds from flag to assigned owner |
| 10 | Five actions: batch preview, approval, split payer, payment, receipt | Run the batch preview before the meeting and show only its approval, the split payer and the Arabic receipt with the amount in words; move "record a payment" to reserve step R-15 | TC-FIN-001 split so the in-meeting part carries its own duration assertion; "record a payment" at R-15 is TC-FIN-551 |
| 10 | Feature 10, configurable without code, is claimed by a split-payer step that shows no configuration | Prove feature 10 in minute 15 instead: the administrator changes one inferred default and the change is versioned | TC-PLT-001 folded into minute 15 |
| 11 | The full data export is a long-running operation | Show the request accepted with its progress, then open an export prepared at the start of the meeting | TC-AUD-002 |
| 13 | Roll call waits on real acknowledgements, and document 02 requires reunification to be the climax, not the broadcast | Seed the demo tenant so most acknowledgements arrive within ten seconds of the trigger; spend the second half of the minute on the verified pickup | TC-ATT-813 with the reunification step asserted last |
| 15 | CAP-PLT-01 promises provisioning "in minutes", which is longer than the minute | Show the saga's first steps live, then switch to a tenant provisioned at the start of the meeting, as minute 8 does for the batch; the smart-defaults screen is the part that must be live | TC-PLT-003 with the provisioning step timed separately from the defaults screen |

### 4.2 Signature features with no demo step

Twenty-two of the 43 signature features have no minute. Each gets one of two fixes: **fold** into an existing minute at no more than ten added seconds; or **reserve**, a sixty-second step in the reserve bank (§4.3) that the presenter swaps in for a specific buyer and that runs in the end-to-end suite like the fifteen minutes. A third route, **demote** — moving a feature from Appendix W to Appendix A by ADR because it cannot be shown to a buyer on demo data — was used once, for feature 39, and is listed below for the record.

| # | Feature | Fix | How |
|---|---|---|---|
| 2 | Student 360 timeline | Reserve R-02 | Principal and teacher open the same child; the teacher's timeline shows only the entries the teacher's role may see (TC-RPT-002) |
| 9 | Go live in a day | Reserve R-03 | Dry run of a 600-row student file shows three problem rows; import; roll back (TC-DOC-001) |
| 11 | Command palette | Fold into minute 12 | The teacher reaches the other teacher's class through the palette, which is where the refusal appears (TC-WEB-001) |
| 13 | Data quality center | Reserve R-04 | Two guardians with the same national identifier surface with the rule that found them and a merge link (TC-RPT-004) |
| 16 | White-label mobile apps | Reserve R-05 | Two phones side by side with two schools' branding, one build (TC-MOB-002); presented as parity |
| 17 | Photo consent at publishing | Reserve R-06 | A class photo with one child lacking media consent is blocked at publish (TC-COM-001) |
| 18 | Student portfolio and recognition | Reserve R-07 | A recognition added by a teacher appears in the student's portfolio and in the family export (TC-BEH-001) |
| 19 | Kindergarten daily sheet | Reserve R-08 | Five taps on a tablet produce the daily sheet on the parent's phone (TC-ACA-001) |
| 20 | Balanced class formation | Reserve R-09 | One pass forms four balanced sections; one drag shows the balance change live (TC-SCH-810) |
| 21 | Inspection readiness | Reserve R-10 | The evidence folder for one criterion, already filled from the term's records (TC-RPT-005) |
| 22 | Policy acknowledgment | Reserve R-11 | A handbook sent to staff; the missing acknowledgments are listed and chased (TC-COM-002) |
| 23 | Live, animated interface | Fold into every minute | No step of its own; the four-way snapshots behind TC-UX-001 guard it, and the presenter never claims it in words |
| 24 | Open by default | Reserve R-12 | A webhook fires to a request inspector when attendance is marked, and a staff calendar updates by iCal (TC-INT-001) |
| 33 | Campus digital twin | Reserve R-13 | The floor plan shows free rooms for period 3 and books one (TC-OPS-001) |
| 36 | School memory: yearbook | Reserve R-14 | The yearbook draft for a grade, built from consented media only (TC-DOC-002 until open point 3 gives it its own test) |
| 37 | Template exchange | Fold into minute 15 | The new tenant starts its report-card template from the exchange (TC-PLT-803), about five seconds |
| 38 | Plug-in kit | Reserve R-16, IT lead only | A sample ministry-export plug-in installed and run on the demo tenant (TC-INT-002) |
| 39 | Calendar-aware scaling | **Demoted (ADR-0019)** | Not showable on demo data in a meeting, so it left Appendix W's signature list for the engineering capabilities in Appendix A, A1. It has no minute and no reserve step; the IT lead is shown the recorded 08:00 peak run measured by Appendix N scenarios N-01 and N-11. It is listed here for the record and is not one of the twenty-two above |
| 40 | Self-healing operations | Reserve R-17, IT lead only | A worker is stopped, the queue backs up, the replay runs inside policy and the operator note appears (TC-INF-002) |
| 41 | Configuration as code | Reserve R-18, IT lead only | A settings export, a one-line diff, review, apply (TC-PLT-804) |
| 42 | Mastery and next step | Reserve R-19 | The standards heatmap for a class, with the rung 2 suggestion switched off to show the fallback (TC-ASM-002) |
| 43 | Workload balance | Reserve R-20 | A cover request shows the least-loaded eligible teacher and the load totals behind it (TC-HR-810) |
| 44 | Low-bandwidth mode | Fold into minute 4 | Minute 4 runs with the data-saver profile on and says so (TC-MOB-005) |

### 4.3 The reserve bank

The reserve steps above plus the two below keep every signature feature demonstrable without lengthening the fifteen minutes. A buyer-specific demo swaps at most two reserve steps in for minutes of the same act.

| Step | What it shows | Test | Available from phase |
|---|---|---|---|
| R-01 | Arabic-aware search: a guardian's name typed with and without hamza or taa marbuta finds the same record, which is the "bilingual data" claim of §2.1 made visible | TC-L10N-311 | 2 (CAP-SCH-02, CAP-BFF-01) |
| R-15 | Record a payment and allocate it across two invoices | TC-FIN-551 | 3 (CAP-FIN-02) |
| R-02 to R-14, R-16 to R-20 | As listed in §4.2 | The feature's own Appendix W test | The phase of the feature's capability in §1 |

**Result of the check.** Six scripted-minute problems need a fix (§4.1); four features fold into existing minutes and eighteen go to the reserve bank, which accounts for all twenty-two signature features without a minute. Feature 39 was demoted under ADR-0019 and is no longer a signature feature, so once the fixes land every signature feature in Appendix W has a sixty-second route.

---

## 5. The pitch

### 5.1 School owner

You are choosing the system your families will judge you by for the next ten years. Every serious product in the region can show you an Arabic screen; we show you a school where every child's record exists in both languages, where a parent can see who looked at their child's file, and where every number on your dashboard opens to the records and the rule behind it. It runs on your own server or in the cloud from the same code, under a licence that does not lock you in, and your data stays readable even if you ever stop paying. **Proof: minute 11 (TC-AUD-002), a parent in Arabic seeing who read their child's sensitive records and downloading the full export.**

### 5.2 Principal

Your mornings go on chasing registers, approvals and the one child nobody noticed. Here the register is three-quarters filled before the teacher opens it, from the gate, the bus and the leave you already approved; the teacher confirms the exceptions in under twenty seconds, even with no signal in the corridor; and when a child starts slipping, the flag tells you why and turns into a plan with an owner. In an emergency every child is accounted for and handed over to a verified adult. **Proof: minute 2 (TC-ATT-003), a pre-filled register confirmed on a phone in under twenty seconds.**

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
| Phases and the MVP cut line | `17-roadmap.md` §1 and §5 | Phases 1 and 2 plus the named phase 3 subset | A buyer is promised a minute that does not exist at their start date |
| Rung and autonomy per feature | `25-ai-and-assist-ladder.md` §2 | As quoted in §1 | The matrix contradicts the ladder |

## Dependencies on other documents

| This document assumes | Stated in | Checked on |
|---|---|---|
| Competitor facts, sources and the re-check before general availability | `02-competitive-gap-analysis.md` | Every change to document 02 |
| The 43 signature features with moment, rung, autonomy, tier, owner and demo test, in 44 rows with row 39 marked as moved | Appendix W | Every lint run |
| The fifteen minutes, their features and test cases | Appendix O | Every release, through the end-to-end suite |
| The P.4 column set | Appendix P section P.4 | Group F review |
| Capabilities, phases and the MVP cut line | `17-roadmap.md` §1, §4, §5 | Every lint run (R19) |
| Rung and autonomy per feature | `25-ai-and-assist-ladder.md` §2 | Group F review |
| RISK-31 and its mitigation | `18-risk-register.md` | Group F review |
| Localization tests TC-L10N-202, TC-L10N-301, TC-L10N-401, TC-L10N-801, TC-L10N-311 | Appendix Q, `24-localization-and-calendars.md` | Group F review |
| The demo tenant and its seed | Appendix H | Every release |

## Open points

| Question | Default | Owner | Impact if the default is wrong |
|---|---|---|---|
| 1. Should the reserve bank of §4.3 become a table in Appendix O, so the reserve steps are part of the release gate by the brief rather than by this document? | Yes, by ADR with the next brief version; until then the reserve tests run in the end-to-end suite on the same schedule as the fifteen minutes | Demo director, then the product owner | Reserve steps rot between releases and fail in front of the buyer they were chosen for |
| 2. Feature 15 (demo mode reset) and feature 37 (template exchange) have no capability of their own in `17-roadmap.md` §4; reset is only a phase 4 exit criterion | Carry feature 15 under CAP-PLT-01 and feature 37 under CAP-PLT-02; propose two capabilities in the next roadmap revision | Product owner | Nobody plans the weeks for them, and the minute 15 fold for feature 37 has nothing to show |
| 3. Appendix O minute 9 cites TC-DOC-002 for QR verification, while Appendix W assigns TC-DOC-002 to feature 36 (the yearbook) | Minute 9 keeps TC-DOC-002; feature 36 receives a new test identifier by ADR amending Appendix W | Demo director | Two behaviours share one test case, and the yearbook passes because verification passes |
| 4. `17-roadmap.md` §1 promises acts one and two at phase 2 and act three at phase 3, but minutes 1, 5, 10, 12 and 14 need Reporting (phase 4), Finance (phase 3) or Wellbeing (phase 5) | Phase demos use the MVP substitutions of §3.1; the roadmap's demo column is amended to say "acts one and two with the substitutions of document 32 §3.1" | Product owner, with the roadmap owner | A phase is declared failed, or passed on a minute that did not run |
| 5. The split of feature 24 into a Tier 1 read-only API and Tier 2 webhooks, recommended by document 02, awaits an ADR. This is **Open Question 28** in `docs/project/OPEN_QUESTIONS.md` | The register's default: they stay where the roadmap builds them, as Tier 2 features. The IT-lead pitch names webhooks, iCal and OneRoster, which land in CAP-INT-01 (phase 3) and CAP-SCD-03 (phase 2) either way | Product owner, as an ADR | An IT lead at an MVP-stage evaluation asks for the API and it is phase 3 |
| 6. Features 16 and 38 are already matched (Fedena branded apps; PowerSchool and Classter plug-ins and marketplace) | Keep them in Appendix W as parity features and never present them as differences | Product owner | The sales sheet claims a difference a buyer can disprove |
| 7. Twenty-four features are unverified in all ten competitors | Resolve for PowerSchool, Classter and Classera by trial or scripted sales conversation in the document 02 re-check before general availability, and update §1 under the same footnote discipline | Domain expert agent | A "we found no competitor documenting it" claim turns out to be matched in a product nobody trialled |

## Review record

| Date | Reviewer | Verdict | Blocking items |
|---|---|---|---|
| 2026-09-22 | Group F review pending | Draft | none recorded yet |

## How this document is verified

| Claim | Proof | Where it runs |
|---|---|---|
| Every Appendix W feature has exactly one matrix row with every P.4 column filled | `demo-director` counts 44 rows across §1.1 and §1.2 against Appendix W, which likewise keeps row 39 marked as moved, and finds no empty P.4 cell (kit-lint R11 checks Appendix W's own register, not this matrix) | Group F review; every change to Appendix W or §1 |
| Every "who else has it" value is traceable to document 02 | Each yes, partial and no carries a document 02 footnote number; every other cell reads unverified | Domain expert review as a school buyer |
| Every sixty-second proof exists | Each minute's test case runs in the end-to-end suite against the demo tenant on every release (Appendix O); each reserve step's test runs on the same schedule (open point 1) | Every release |
| Each minute fits in sixty seconds | The duration assertions named in §4.1, and a timed dry run by the demo director before each phase demo | Every phase demo |
| Every capability cited exists in the roadmap | `tools/kit-lint` rule R19 against `17-roadmap.md` §4 | Every change under `docs/` |
| The MVP mapping matches the roadmap | `plan-consistency-checker` compares the phase §3 gives each capability with `17-roadmap.md` §5; kit-lint R19 fails on a `CAP-` identifier in §3 that document 17 does not define | The comparison on every change to either document; kit-lint on every change under `docs/` |
| No banned claim reaches the sales material | The sales sheet is reviewed against §2.2 before each release | Every release |
| This document agrees with the catalogs | `node tools/kit-lint/kit-lint.mjs .` for section and appendix references, identifiers and open markers | Every change under `docs/` |
