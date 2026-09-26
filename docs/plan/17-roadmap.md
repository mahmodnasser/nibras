# 17. Roadmap

## Purpose

This document turns the seven phases of master brief Section 28 into **capabilities**: things a named person can do end to end, each one to three weeks, each one demonstrable. `34-work-breakdown.md` then splits every capability into slices of one to three days. Together they answer the question "what does the team build on Monday".

The durations below are ranges for a small, experienced team, per Section 28 and the team shape in Section 29. They are estimates with their uncertainty stated, not commitments. A date is only ever derived from a range and a start, and it stays a range.

## Scope

| In scope | Owned elsewhere |
|---|---|
| Phases, their goals, exit criteria and demos | The phase definitions themselves: master brief Section 28 |
| Every capability with its identifier, services, workflows and requirement scope | Slices within a capability: document 34 |
| Scope of each phase by requirement identifier, computed from document 03 | The requirements themselves: document 03 |
| Parallel streams, dependencies and the critical path | Estimates per slice: document 34 |

## Content

### 1. Phases at a glance

| Phase | Goal | Services | Range | Capabilities | Workflows | Service requirements | Demo at the end |
|---|---|---|---|---|---|---|---|
| **0 Plan** | The documents in `docs/plan/`, approved group by group | none | 3 to 5 weeks | none | none | none | The plan, scored at 4 or better on every axis |
| **1 Foundation** | Everything every later service stands on | Gateway, Bff.Web, Identity, Platform, Notification, Audit, and the Documents rendering pipeline | 14 to 22 weeks | 19 | 14 | 153 plus the cross-cutting obligations | A tenant is provisioned live, its administrator signs in with a second factor, a notification arrives, the audit entry is visible |
| **2 School year loop** | A class is taught, attended, graded and reported, in both languages, on web and phone | School, Scheduling, Attendance, Academics, Assessment, Bff.Mobile | 14 to 21 weeks | 19 | 10 | 177 | Every Appendix O minute whose Phase cell is 2 or earlier, at the MVP cut line: minutes 2, 3, 6, 7, 8 and 9 as scripted and minute 4 without the data-saver profile; from act three, minute 11 (guardian transparency, Appendix W feature 31) and minute 13 (emergency mode, feature 32) as scripted; minute 15 as in phase 1. Minutes 1, 5, 10, 12 and 14 by their "Before a minute's phase" substitutions (TC-RQS-002; TC-RPT-008 against the threshold rule; TC-L10N-202 and TC-L10N-301; a custody-restricted record, TC-SEC-530; reserve step R-01, TC-L10N-311). **The MVP cut line** |
| **3 Money and paperwork** | A fee is invoiced, chased and paid; a request is approved and takes effect; a certificate verifies | Finance, Requests, Communication, Documents | 11 to 17 weeks | 14 | 9 | 103 | Minute 10 of Appendix O now as scripted, so act two runs as written, with every minute the phase 2 demo ran as scripted; minutes 1, 5, 12 and 14 keep their substitutions (TC-RQS-002; TC-RPT-008 against the threshold rule; TC-SEC-530; reserve step R-01, TC-L10N-311) |
| **4 Growth** | An applicant becomes a student; dashboards answer Appendix D; mobile reaches parity | Admissions, Behavior, Reporting | 7 to 11 weeks | 9 | 3 | 61 | Fourteen of the fifteen Appendix O minutes as scripted on a fresh tenant with one-click reset; minute 12 keeps its substitution (TC-SEC-530), because its Phase cell is 5 |
| **5 Extended** | Wellbeing, human resources, operations and assistance, each to the definition of done | Wellbeing, Hr, Operations, Ai | 9 to 14 weeks | 12 | 14 | 67 | Minute 12 as scripted, so all fifteen minutes run as written for the first time; a clinic visit, a substitution from leave, a library loan, and a reviewed draft comment |
| **6 Hardening and launch** | Evidence that every gate in Section 24 holds at scale | all | 6 to 8 weeks | 6 | 2 | the phase 6 obligations below | Restore drill, penetration-test close-out, and the scale-tier load run, each with its record |

The capability and workflow columns are counted from the Section 4 tables: 79 capabilities, and 52 distinct `WF-` identifiers, which is every workflow in `31-business-rules-and-workflows.md`. A workflow is counted once, in the phase of the capability that builds it, which is why WF-INF-01 and WF-INF-03 count in phase 6 and not in phase 1 with the rest of Platform's.

The service requirement counts are computed from `03-requirements-catalog.md` by the owning service's build phase in `05-service-catalog.md` and by the phase column of `20-traceability-matrix.md` where a requirement is scheduled away from its service's phase. They add to 561. The remaining 299 requirements are cross-cutting and are covered in Section 3.

**How the demo column reads Appendix O.** Each Appendix O minute and reserve step carries a Phase cell, the phase from whose end-of-phase demo it runs exactly as written; an earlier phase demo runs the substitution in Appendix O's "Before a minute's phase" and says so on screen. The demo column above is those Phase cells applied phase by phase, and it names every minute a phase shows by substitution. `16-test-strategy.md` part 12.2 applies the same cells to the demo gate, so the two name the same minutes and the same substitutions at every phase; the gate also runs the reserve steps whose phase has shipped and each signature feature's own demo test from the phase that builds the feature. Which requirements, capabilities and slices build each signature feature, with its Appendix W rung, autonomy and demo test, is the "Signature feature trace" of `32-product-differentiation-and-demo.md`; this document cites features by their Appendix W number and does not copy that table.

**Total from start of phase 1 to launch: 61 to 93 weeks** for the team in master brief Section 29, counted in working weeks of five days. With the calendar allowance below, holidays and annual leave both included, that is about 71 to 108 calendar weeks, and the honest reading is "about twenty months, plus or minus five": the low end needs eight engineers building from the first week, the high end is five. The biggest lever on it is the MVP cut line in Section 5, not working faster.

**How the ranges are computed.** Every range above is derived from the slices in `34-work-breakdown.md` by `tools/plan-build/schedule-34.mjs`, not estimated separately:

| Input | Value | Why |
|---|---|---|
| Effort | The phase's slice-days in document 34 | One to three days per slice, one engineer |
| Builders | 5 to 8 | Master brief Section 29: two to four backend, one to two web, one mobile, one quality engineer. **The quality engineer builds slices and is counted**: the test-infrastructure slices of document 34 (SL-TST and SL-PERF, 46 slice-days) and feature slices like any engineer, because every slice carries its own tests in its definition of done. The quality engineer's release work that is not a slice (the device pass, the Appendix Q acceptance run, running the demo gate) is inside the overhead row |
| Overhead | × 1.3 | Review, integration, the demonstration that closes each capability, and the quality engineer's release work above |
| Working weeks | slice-days × 1.3 ÷ (builders × 5) | Eight builders give the low end, five the high end. A week here is five working days |
| Calendar allowance | Stated here, not in the generator | The working weeks become calendar weeks by adding the team's non-working days, which `schedule-34.mjs` does not model: Ramadan, when the working day is customarily shortened in Saudi Arabia, the United Arab Emirates and Jordan (about one working week lost over the month), the Eid al-Fitr and Eid al-Adha holidays and the national days (about two working weeks), so about three weeks a year; and each builder's annual leave, about four working weeks a year taken in rotation, which removes about eight percent of the team's capacity. Together that is about seven weeks of each working year, so a calendar year holds about 45 working weeks. Over the 61 to 93 working weeks to launch the allowance is about ten to fifteen calendar weeks, which gives the 71 to 108 calendar weeks of the headline above. A date is read from a range only after this allowance, and the review at each phase end records the calendar actually lost |
| Dependency floor | The longest chain of slice-to-slice dependencies in the phase, × 1.3 ÷ 5 | No headcount shortens it. It is 4 to 7 weeks in every phase, under the effort figure, so capacity, not sequencing, sets the pace. Dependencies stated as contracts are not in the chain, so it is a lower bound |
| Calendar floor | Phase 6: 6 to 8 weeks | The external penetration test, its retest window and an isolated restore drill take calendar time whatever the effort |

These replace the ranges of master brief Section 28, which were set before any slice existed and put phase 1 at 8 to 10 weeks against 423 slice-days. The brief is corrected to match under ADR-0019. A team outside Section 29's shape changes the builders input and nothing else; the ranges are recomputed, never adjusted by hand. For example, a team whose quality engineer takes no slices has four to seven builders, and the same formula over the same slices gives phase 1 16 to 28 weeks and launch 67 to 115 weeks; that difference is the capacity the quality engineer's slices carry.

### 2. Scope of each phase by requirement identifier

Computed from document 03. A requirement belongs to the phase in which its owning service is built.

| Phase | Area | Count | Identifiers |
|---|---|---|---|
| 1 | IDN | 47 | REQ-IDN-001 to 047 |
| 1 | PLT | 39 | REQ-PLT-001 to 039 |
| 1 | NOT | 19 | REQ-NOT-001 to 019 |
| 1 | INT | 14 | REQ-INT-001 to 013, REQ-INT-017 |
| 1 | GW | 9 | REQ-GW-001 to 009 |
| 1 | AUD | 9 | REQ-AUD-001 to 009 |
| 1 | PRV | 7 | REQ-PRV-002, 010, 012, 014, 017, 018, 021 |
| 1 | BFF | 4 | REQ-BFF-001 to 003, REQ-BFF-006 |
| 1 | MOB, DATA, MSG, PERF | 5 | REQ-MOB-039, 041; REQ-DATA-027; REQ-MSG-024; REQ-PERF-025 |
| 2 | ATT | 38 | REQ-ATT-001 to 038 |
| 2 | SCH | 37 | REQ-SCH-001 to 037 |
| 2 | ASM | 36 | REQ-ASM-001 to 036 |
| 2 | ACA | 30 | REQ-ACA-001 to 030 |
| 2 | SCD | 24 | REQ-SCD-001 to 024 |
| 2 | BFF | 5 | REQ-BFF-004, 005, 007 to 009 |
| 2 | MOB, PRV, PERF | 7 | REQ-MOB-017, 020, 029, 034; REQ-PRV-003, 004; REQ-PERF-024 |
| 3 | FIN | 43 | REQ-FIN-001 to 043 |
| 3 | RQS | 20 | REQ-RQS-001 to 020 |
| 3 | COM | 17 | REQ-COM-001 to 017 |
| 3 | DOC | 17 | REQ-DOC-001 to 017 |
| 3 | PRV, PLAT, TST | 4 | REQ-PRV-007, 008; REQ-PLAT-020; REQ-TST-015 |
| 3 | INT | 2 | REQ-INT-014, REQ-INT-015 |
| 4 | ADM | 26 | REQ-ADM-001 to 026 |
| 4 | RPT | 18 | REQ-RPT-001 to 018 |
| 4 | BEH | 11 | REQ-BEH-001 to 011 |
| 4 | DATA, PRV | 5 | REQ-DATA-023 to 026; REQ-PRV-005 |
| 4 | INT | 1 | REQ-INT-016, counted once in the phase where it starts; LTI 1.3 in phase 4 and QTI 3, Open Badges 3.0 and CASE in phase 5, as document 20's phase column says |
| 5 | AI | 19 | REQ-AI-001 to 019 |
| 5 | OPS | 18 | REQ-OPS-001 to 018 |
| 5 | WEL | 17 | REQ-WEL-001 to 017 |
| 5 | HR | 12 | REQ-HR-001 to 012 |
| 5 | PRV | 1 | REQ-PRV-006 |

### 3. Cross-cutting requirements: established once, binding from then on

299 requirements belong to no single service. Each is **established** in the phase that builds its mechanism, and from that phase on it is part of the definition of done for every slice, in every service. A cross-cutting requirement is never "done" in the sense of finished; it is done when its mechanism exists and a gate enforces it.

| Area | Count | Established in | By which capability | Enforced from then on by |
|---|---|---|---|---|
| SEC | 20 | 1 | CAP-DATA-01, CAP-IDN-01, CAP-IDN-02 | The generated permission and tenant-isolation suites on every build |
| DATA | 23 | 1 | CAP-DATA-01 | Architecture tests and the pooled-connection isolation test |
| MSG | 23 | 1 | CAP-MSG-01 | Contract tests and the deliver-twice test per consumer |
| PERF | 29 | 1 | CAP-PERF-01 | Query budgets in integration tests; the baseline gate |
| API | 27 | 1 | CAP-INF-01 | Spectral rules and breaking-change detection in the pipeline |
| TST | 24 | 1 | CAP-INF-01 | The quality gates in Appendix V |
| L10N | 16 | 1 | CAP-UX-01 | The missing-translation report and the three-culture test |
| UX | 17 | 1 | CAP-UX-01 | Four-way snapshots and axe-core |
| WEB | 11 | 1 | CAP-BFF-01 | Bundle budgets and the web pipeline |
| PLAT | 22 | 1 | CAP-INF-02 | The dev-smoke job on `ubuntu-latest`, `windows-latest` and `macos-latest`, and the lint rules |
| PRV | 10 | 1 | CAP-AUD-01, CAP-PRV-01 | The privacy auditor and the retention jobs |
| INT | 2 | 3 | CAP-INT-01 | Contract tests on the public API |
| MOB | 38 | 2 | CAP-MOB-01 | Goldens in both directions and the offline sync tests |
| INF | 37 | 1 and 6 | CAP-INF-01, CAP-INF-03 in phase 1; CAP-INF-04, CAP-INF-05 in phase 6 | Pipeline stages from phase 1; drills and the appliance from phase 6 |

Phase 1 therefore carries far more than its 153 service requirements. That is why it is the largest phase by effort, 423 slice-days, and why its range of 14 to 22 weeks is the widest. A larger team helps less here than anywhere, because the building blocks come first and everything waits on them.

### 4. Capabilities

Identifiers are `CAP-<AREA>-<NN>` with area codes from Appendix L. Each is named as the sentence a person would say when it works. Workflows are from Appendix R; rule ownership is in document 31.

#### Phase 1: Foundation

| Capability | What a person can do | Services | Workflows | Weeks |
|---|---|---|---|---|
| CAP-INF-01 | An engineer creates a service from the template, and it passes the whole pipeline on the first commit | BuildingBlocks, tools | none | 2 to 3 |
| CAP-INF-02 | A developer on Windows or Linux starts the whole stack with one command | AppHost, deploy/compose | none | 1 |
| CAP-DATA-01 | Tenant A can never read tenant B's data, and a test proves it on every build | BuildingBlocks | none | 2 |
| CAP-MSG-01 | An event survives a worker crash and is processed exactly once | BuildingBlocks | none | 2 |
| CAP-PERF-01 | A handler that exceeds its query budget fails its test; cached reads are tenant-safe | BuildingBlocks | none | 1 to 2 |
| CAP-GW-01 | Every request enters through one front door that knows the tenant and limits abuse | Gateway | none | 1 |
| CAP-IDN-01 | A person signs in with a second factor, and the seeded administrator is safe | Identity | none | 2 |
| CAP-IDN-02 | An administrator decides who can do what, and the change applies without anyone signing out | Identity | WF-IDN-05 | 2 to 3 |
| CAP-IDN-03 | Staff, parents and students join the school | Identity | WF-IDN-01, WF-IDN-02, WF-IDN-03 | 2 |
| CAP-IDN-04 | Access is governed over time: delegation, offboarding, reviews, break-glass, consented impersonation | Identity | WF-IDN-04, WF-IDN-06, WF-SEC-01, WF-SEC-02, WF-SEC-03 | 2 to 3 |
| CAP-PLT-01 | A platform operator provisions a school in minutes, with sensible defaults already set | Platform | WF-PLT-01 | 2 to 3 |
| CAP-PLT-02 | A school's plan, limits, flags, settings and lifecycle are managed without a developer | Platform | WF-PLT-02, WF-PLT-03 | 2 |
| CAP-NOT-01 | A person receives the right message on the right channel, at the right time, in their language | Notification | none | 2 |
| CAP-AUD-01 | Every sensitive action is traceable, and the trail cannot be quietly edited | Audit | none | 1 to 2 |
| CAP-DOC-01 | A document renders correctly in Arabic and English, every time | Documents (rendering only) | none | 1 to 2 |
| CAP-UX-01 | The design system exists in both languages, both directions, both themes, on web and mobile | Web, Mobile | none | 3 |
| CAP-BFF-01 | The platform console and the school console know who you are and show only what you may do | Bff.Web, Web | none | 2 |
| CAP-PRV-01 | A data subject request can be answered from the services that exist | Platform, Audit | WF-PRV-01 | 1 |
| CAP-INF-03 | A release rolls out by canary, migrates safely, and rolls back by redeploying the previous image | tools, deploy | WF-INF-02 | 1 to 2 |

**Exit criteria.** Every capability above demonstrated to someone who did not build it; the generated tenant-isolation and permission suites green; the pipeline enforcing licence scan, secret scan, budgets and snapshots; `dev-smoke` green on `ubuntu-latest`, `windows-latest` and `macos-latest`; Appendix V gates met for the six services built; minute 15 and reserve step R-18 of Appendix O pass, the steps whose Phase cell is 1.

#### Phase 2: The school year loop (the MVP cut line)

| Capability | What a person can do | Services | Workflows | Weeks |
|---|---|---|---|---|
| CAP-SCH-01 | A school sets up its campuses, years, terms, grades, sections, subjects and houses | School | none | 2 |
| CAP-SCH-02 | Students, guardians and staff are on record, bilingual, with custody and access rights | School | none | 2 to 3 |
| CAP-SCH-03 | A student changes section, campus, or leaves, and every service follows | School | WF-SCH-01, WF-SCH-04 | 2 |
| CAP-SCH-04 | The year closes, students are promoted, and the structure rolls forward | School | WF-SCH-02, WF-SCH-03 | 2 |
| CAP-SCD-01 | A coordinator builds, refines and publishes the timetable | Scheduling | none | 3 |
| CAP-SCD-02 | When a teacher is absent, fair cover is arranged in minutes | Scheduling | none | 1 to 2 |
| CAP-SCD-03 | The calendar, events and room bookings are managed, and staff subscribe by iCal | Scheduling | none | 1 to 2 |
| CAP-ATT-01 | A teacher takes attendance for a class in under a minute, on a phone, offline | Attendance | WF-ATT-01 (marking) | 2 to 3 |
| CAP-ATT-02 | Absences are excused, thresholds escalate, and an intervention opens | Attendance | WF-ATT-01 (escalation) | 2 |
| CAP-ATT-03 | Children leave safely: pickup persons, gate passes, early dismissal, visitors | Attendance | WF-ATT-02 | 2 |
| CAP-ATT-04 | In an emergency, every person is accounted for and reunified | Attendance | none | 1 to 2 |
| CAP-ACA-01 | Teachers plan lessons and set work, and nobody overloads a class | Academics | WF-ACA-01 | 2 |
| CAP-ACA-02 | Students submit work and receive feedback | Academics | WF-ACA-01 | 2 |
| CAP-ASM-01 | Marks are entered at keyboard speed, moderated and locked | Assessment | none | 2 to 3 |
| CAP-ASM-02 | Report cards are generated in a batch, published, and verified by QR | Assessment, Documents | WF-ASM-01 | 2 to 3 |
| CAP-ASM-03 | A grade can be appealed and changed after lock, with a new report-card version | Assessment | WF-ASM-02 | 1 |
| CAP-ASM-04 | Exams are scheduled, seated and invigilated, and papers are set and reviewed securely | Assessment | WF-ASM-03 | 2 |
| CAP-MOB-01 | The teacher app works offline and resolves conflicts visibly | Bff.Mobile, Mobile | none | 3 |
| CAP-MOB-02 | A parent follows every child on one calm screen | Bff.Mobile, Mobile | none | 2 |

**Exit criteria.** A class is taught, attended, graded and reported end to end in both languages on web and phone; every Appendix O minute whose Phase cell is 2 or earlier passes as scripted, as the phase 2 row of Section 1 states them, minutes 11 and 13 of act three among them, and minutes 1, 5, 10, 12 and 14 pass by their substitutions, with the demo gate running each substitution's test (`16-test-strategy.md` part 12.2); the 08:00 attendance-peak scenario from Appendix N passes at the load tier.

#### Phase 3: Money and paperwork

| Capability | What a person can do | Services | Workflows | Weeks |
|---|---|---|---|---|
| CAP-FIN-01 | An accountant structures fees and runs invoices in bulk, with gapless numbering and tax | Finance | WF-FIN-01 (invoicing) | 3 |
| CAP-FIN-02 | Payments are recorded, allocated and receipted, online or at the cashier | Finance | WF-FIN-01 (collection) | 2 |
| CAP-FIN-03 | Overdue fees are chased fairly, and restrictions apply only by explicit policy | Finance | WF-FIN-01 (escalation) | 1 to 2 |
| CAP-FIN-04 | Mistakes are corrected by reversal: credit notes, refunds, bounced cheques | Finance | WF-FIN-02, WF-FIN-03 | 2 |
| CAP-FIN-05 | Scholarships, discounts and sponsors are handled without spreadsheets | Finance | WF-FIN-04, WF-FIN-05 | 1 to 2 |
| CAP-FIN-06 | The cashier closes the day balanced, and the books export cleanly | Finance | WF-FIN-06 | 1 to 2 |
| CAP-RQS-01 | Anyone submits a request that is routed, approved on a phone, and takes effect automatically | Requests | WF-RQS-01 | 3 |
| CAP-RQS-02 | Everyone has one inbox of tasks, approvals and mentions | Requests, Notification | none | 1 |
| CAP-COM-01 | The school announces, and it knows who acknowledged | Communication | none | 1 to 2 |
| CAP-COM-02 | Parents and staff message safely within the school's policy, and concerns reach the right person | Communication | none | 2 |
| CAP-COM-03 | Parents book meetings, including whole conference days | Communication | none | 1 |
| CAP-DOC-02 | Certificates and letters are issued, verified by QR, and revocable | Documents | none | 1 to 2 |
| CAP-DOC-03 | A school moves in with its own data in a day, with a dry run and a rollback | Documents | WF-DATA-01, WF-PRV-02 | 2 to 3 |
| CAP-INT-01 | Other systems connect: public API, webhooks, iCal | Platform | none | 2 |

**Exit criteria.** Minute 10 of Appendix O now passes as scripted, with every minute that passed as scripted in phase 2, and minutes 1, 5, 12 and 14 pass by their substitutions, minute 14 by reserve step R-01, as the phase 3 row of Section 1 states; the 5,000-invoice and 10,000-row import scenarios from Appendix N pass at the load tier; the finance daily balance job reconciles on the demo data.

#### Phase 4: Growth

| Capability | What a person can do | Services | Workflows | Weeks |
|---|---|---|---|---|
| CAP-ADM-01 | An inquiry becomes an enrolled student without anyone retyping anything | Admissions | WF-ADM-01 | 3 |
| CAP-ADM-02 | Families re-enrol, and outstanding balances are settled first | Admissions | WF-ADM-02 | 1 to 2 |
| CAP-BEH-01 | Behaviour is recorded, recognised and followed up | Behavior | WF-BEH-01 | 2 |
| CAP-RPT-01 | Every role opens a dashboard that answers its questions, and any number can be explained | Reporting, Bff.Web | none | 3 |
| CAP-RPT-02 | Reports are built, scheduled and exported without a developer | Reporting | none | 2 |
| CAP-RPT-03 | Early warning leads to a plan with an owner, and data-quality problems surface themselves | Reporting | none | 2 |
| CAP-MOB-03 | The mobile app reaches the parity matrix in document 09 | Bff.Mobile, Mobile | none | 2 to 3 |
| CAP-ACA-03 | Nursery and kindergarten families receive the daily sheet | Academics | none | 1 to 2 |
| CAP-INT-02 | A teacher launches a registered external learning tool from a section, and the tool reads only the roster its privacy setting allows | Platform | none | 1 |

**Exit criteria.** Every Appendix O minute whose Phase cell is 4 or earlier passes as scripted on a freshly provisioned tenant with one-click reset, which is fourteen of the fifteen; minute 12 passes by its substitution until phase 5; the principal-dashboard scenario from Appendix N passes; an LTI 1.3 launch with deep linking and names and roles passes on the demo tenant.

#### Phase 5: Extended

| Capability | What a person can do | Services | Workflows | Weeks |
|---|---|---|---|---|
| CAP-WEL-01 | The nurse cares for a child and the family is told, without clinical detail leaving the service | Wellbeing | WF-WEL-02, WF-WEL-03 | 2 |
| CAP-WEL-02 | A safeguarding concern reaches the officer and is handled to an outcome | Wellbeing | WF-WEL-04, WF-WEL-05 | 2 |
| CAP-WEL-03 | A special-needs plan reaches the classroom and the exam room | Wellbeing | WF-WEL-01 | 2 |
| CAP-HR-01 | Staff are hired onto file, and licence expiry is caught in time | Hr | WF-HR-02, WF-HR-03 | 2 |
| CAP-HR-02 | Approved leave arranges its own cover | Hr, Scheduling | WF-HR-01 | 1 |
| CAP-HR-03 | Payroll inputs are prepared and exported | Hr | WF-HR-04 | 1 to 2 |
| CAP-OPS-01 | The library lends and the store issues, with fines and stock checks | Operations | WF-OPS-01, WF-OPS-02 | 2 |
| CAP-OPS-02 | Transport routes, boarding and delays reach parents | Operations | WF-OPS-03 | 2 |
| CAP-OPS-03 | Facilities, front desk, complaints and drills are managed | Operations | WF-OPS-04, WF-OPS-05 | 2 |
| CAP-AI-01 | A teacher gets a reviewed draft, a translation or an answer, and the school can switch it all off | Ai | none | 3 |
| CAP-AI-02 | Every automated decision explains itself and can be overridden | Ai, Reporting | none | 1 |
| CAP-INT-03 | A school exchanges question banks, curriculum standards and badge credentials with other systems through the 1EdTech formats | Academics, Behavior | none | 1 to 2 |

**Exit criteria.** Each Tier 2 module meets the definition of done in master brief Section 26; the product is fully usable with every assist rung above 1 switched off; minute 12 of Appendix O passes as scripted, so all fifteen minutes and reserve steps R-13 and R-20 run as written; the standards checklist of REQ-INT-016 passes a QTI 3 round trip, an Open Badges 3.0 credential verified by an independent verifier, and a CASE 1.0 conformance run of the CASE import built in phase 2.

CAP-INT-02 and CAP-INT-03 carry the REQ-INT-016 standards work that the Platform service sheet and the requirement itself time to phases 4 and 5. `34-work-breakdown.md` lists SL-INT-411 under CAP-INT-02 and SL-INT-600 to SL-INT-602 under CAP-INT-03; moving them there from CAP-ACA-03 and CAP-AI-01 changed no days, services or dependencies, so the phase ranges in Section 1 are unaffected.

**Each standard's importer is built once.** The CASE import is built in phase 2, by SL-ACA-203 for REQ-ACA-007 under CAP-ACA-01, because the standards heatmap of Appendix W feature 42, shown by reserve step R-19 from phase 2, maps standards that arrive by that import. SL-INT-601 in phase 5 does not build a second importer: it extends the same `CaseStandardsImportJob` with the provider-URL fetch and the re-import change list that the REQ-INT-016 checklist needs, and the phase 5 exit runs the checklist against it. QTI 3 follows the same rule: the question bank's import and export are built in phase 2 by SL-ACA-216 (REQ-ACA-026), and SL-INT-600 adds the per-item report and the round trip in phase 5. The Open Badges credential is built in phase 4 by SL-BEH-408 (REQ-BEH-006), and SL-INT-602 adds the conformance items. Document 34 words SL-INT-600 and SL-INT-601 as extensions of those slices, each ending "no second importer is built", and the days do not change. Why each of these is built before its tier is in "Requirements built ahead of their tier" below.

#### Phase 6: Hardening and launch

| Capability | What a person can do | Services | Workflows | Weeks |
|---|---|---|---|---|
| CAP-INF-04 | The platform survives failure, and a school can be restored alone | all | WF-INF-03 | 2 |
| CAP-INF-05 | An on-premises school upgrades safely on a Windows host through the appliance | deploy/onprem | WF-INF-01 | 2 |
| CAP-SEC-01 | An independent penetration test finds nothing left open | all | none | 2 to 3 |
| CAP-PERF-02 | The scale targets of master brief Section 21 hold at the scale tier | all | none | 2 |
| CAP-UX-02 | Accessibility is confirmed by a manual pass and a published conformance statement | Web, Mobile | none | 1 |
| CAP-INF-06 | A school is supported: runbooks exercised, status page live, support tiers staffed | Platform, deploy | none | 1 to 2 |

**Exit criteria.** Every gate in master brief Section 24 green with evidence; penetration-test findings closed or accepted with an owner and a date; one quarterly restore drill recorded with elapsed times.

Three workflows owned by Platform are scheduled by capability rather than by their service's phase: WF-INF-02 in phase 1 because releases start then, and WF-INF-01 and WF-INF-03 in phase 6 because they need a running product to drill against. Document 31 lists each workflow at its owning service's phase; this section is authoritative for timing.

### 5. The MVP cut line

The first paying school needs **phases 1 and 2 complete**, plus from phase 3 only what term one uses: **CAP-RQS-01** limited to the attendance and document request types, **CAP-COM-01** and **CAP-COM-02**, and **CAP-DOC-02**. That is **42 capabilities, 954 slice-days, and 33 to 50 weeks from the start of phase 1**, computed the same way as Section 1. At the middle of the team range, with the calendar allowance of Section 1, that is about eleven months to the first paying school.

| In the MVP | Not in the MVP, and why |
|---|---|
| Sign-in, roles, joining, provisioning, notifications, audit | |
| School structure, students, guardians, staff, transfers, year close | |
| Timetable, cover, calendar | |
| Attendance, excuses, dismissal, emergency mode | |
| Coursework, marks, report cards, exams | |
| Teacher and parent mobile, offline | |
| Announcements, messaging, certificates, two request types | |
| | Finance: a school on its existing billing system for the first term is common, and fees are the highest-risk module to rush |
| | Admissions: runs once a year; the first customer arrives already enrolled |
| | Behavior, Reporting dashboards: valuable, not blocking a school day |
| | Wellbeing, Hr, Operations, Ai: Tier 2 |

A school that needs Finance in term one is a phase 3 customer, and saying so in the first meeting is cheaper than saying it in month nine.

### 6. Parallel streams

With the team in master brief Section 29, work runs in four streams. A stream is a set of people, not a service.

| Stream | Phase 1 | Phase 2 | Phase 3 | Phase 4 | Phase 5 |
|---|---|---|---|---|---|
| Platform backend | CAP-INF-01, CAP-DATA-01, CAP-MSG-01, CAP-PERF-01, CAP-GW-01 | CAP-SCH-01 to 04 | CAP-RQS-01, 02 | CAP-RPT-01 to 03 | CAP-AI-01, 02 |
| Domain backend | CAP-IDN-01 to 04, CAP-PLT-01, 02, CAP-AUD-01, CAP-PRV-01 | CAP-SCD-01 to 03, CAP-ATT-01 to 04, CAP-ACA-01, 02, CAP-ASM-01 to 04 | CAP-FIN-01 to 06, CAP-COM-01 to 03, CAP-DOC-02, 03, CAP-INT-01 | CAP-ADM-01, 02, CAP-BEH-01, CAP-INT-02 | CAP-WEL-01 to 03, CAP-HR-01 to 03, CAP-OPS-01 to 03, CAP-INT-03 |
| Web | CAP-UX-01, CAP-BFF-01 | screens for every phase 2 capability | screens for every phase 3 capability | screens and dashboards | screens |
| Mobile and delivery | CAP-INF-02, CAP-INF-03, CAP-NOT-01, CAP-DOC-01 | CAP-MOB-01, 02 | mobile screens for phase 3 | CAP-MOB-03, CAP-ACA-03 | mobile screens |

Web and mobile start each capability as soon as its API contract is published, not when its backend is finished. That contract-first rule is what makes four streams possible without four times the waiting.

### 7. Dependencies and the critical path

```mermaid
flowchart LR
    INF01["CAP-INF-01<br/>template and pipeline"] --> DATA01["CAP-DATA-01<br/>tenancy"]
    INF01 --> MSG01["CAP-MSG-01<br/>messaging"]
    DATA01 --> IDN01["CAP-IDN-01<br/>sign-in"]
    IDN01 --> IDN02["CAP-IDN-02<br/>permissions"]
    MSG01 --> PLT01["CAP-PLT-01<br/>provisioning"]
    IDN02 --> PLT01
    PLT01 --> SCH01["CAP-SCH-01<br/>school structure"]
    SCH01 --> SCH02["CAP-SCH-02<br/>students and staff"]
    SCH02 --> SCD01["CAP-SCD-01<br/>timetable"]
    SCD01 --> ATT01["CAP-ATT-01<br/>attendance"]
    SCH02 --> ASM01["CAP-ASM-01<br/>marks"]
    DOC01["CAP-DOC-01<br/>rendering"] --> ASM02["CAP-ASM-02<br/>report cards"]
    ASM01 --> ASM02
    ATT01 --> MVP(["MVP demo"])
    ASM02 --> MVP
    ASM02 --> FIN01["CAP-FIN-01<br/>invoicing"]
```

**The critical path** runs template, tenancy, sign-in, permissions, provisioning, school structure, students, then splits into timetable to attendance and marks to report cards. Everything in phase 2 waits for CAP-SCH-02, because every academic service keeps a reference copy of School's students. That makes CAP-SCH-02 the single most important capability to finish on time, and it is why School is built by the platform stream, which has just finished the building blocks and knows them best.

| Dependency | Why | Mitigation |
|---|---|---|
| Every phase 2 service waits for School's student events | Reference copies are built from `school.student.enrolled.v1` and friends | Publish School's event contracts in week 1 of phase 2 and seed the demo data through them, so consumers start before School's screens exist |
| Report cards need the rendering pipeline | Arabic shaping is the hardest part of document generation | CAP-DOC-01 is built in phase 1 deliberately, a phase early |
| Finance needs report-card restriction hooks and Admissions needs Finance | `finance.account.restricted.v1` is consumed by Assessment; `finance.payment.received.v1` by Admissions | Contracts published in phase 2; consumers tolerate the producer's absence |
| Hr leave needs Scheduling substitutions | `hr.leave.approved.v1` drives cover | Scheduling's cover consumer is built in phase 2 against the contract, before Hr exists |
| iOS release needs Apple build capacity | Open Question 14 | Settled before phase 2; Android and mobile web ship independently |

### Requirements built ahead of their tier

Master brief Section 13 says Tier 1 is complete and polished before Tier 2 begins, and Tier 3 is designed for and built last. Phases 1 to 4 build Tier 1, yet 35 Tier 2 or Tier 3 requirements of `03-requirements-catalog.md` are covered by a phase 1 to 4 slice of `34-work-breakdown.md`, counted from those two documents by kit-lint R35. Each is listed here with its reason, and ADR-0024 (Proposed) asks the product owner to accept the list; RISK-02 in `18-risk-register.md` is the risk this table manages. A reason is one of four kinds:

- **Demo.** An Appendix O step whose Phase cell falls in phases 1 to 4 shows the feature, so the brief's own demo schedule needs it built. Features are cited by Appendix W number; the requirements, slices and demo test of each are in the "Signature feature trace" of `32-product-differentiation-and-demo.md`.
- **Shared.** A Tier 1 requirement's mechanism carries it in the same slice, so building it later would reopen finished Tier 1 work.
- **Locality.** Its service is being built in that phase and nothing else needs it yet; deferring it reopens the service later. The product owner may move these without harm to any Tier 1 promise.
- **Move.** No reason stands against the brief. ADR-0024 proposes moving it, and the change is logged for document 34; until the product owner decides, document 34 still builds it where the row says. The one exception is REQ-ACA-030, whose launch half cannot run in phase 2 and moves now, as its row says.

| Requirement | Tier | Slices | Phase | Why it is built early |
|---|---|---|---|---|
| REQ-IDN-010 | 2 | SL-IDN-011 | 1 | Shared. Master brief Section 35 designs Identity so that SAML 2.0 and SCIM are "a protocol adapter and not a redesign"; building the one adapter while the sign-in pipeline of CAP-IDN-01 is built is what proves that seam, and ministry and large-group buyers ask for it at procurement |
| REQ-MOB-039 | 2 | SL-NOT-002 | 1 | Move. Master brief Section 37 builds the Huawei adapter "when the device share justifies it". SL-NOT-002 needs only the `IPushSender` seam, FCM, APNs and the in-app fallback for devices without Google services (Tier 1, REQ-NOT-003); the adapter itself waits for the device share that open question 17 asks about, as document 09 records |
| REQ-NOT-002 | 3 | SL-NOT-004 | 1 | Move. The channel adapter interface and the SMS adapter in SL-NOT-004 are Tier 1; the WhatsApp adapter is a second implementation of the same interface with no phase 1 need, and Tier 3 is built last |
| REQ-PLT-023 | 2 | SL-PLT-022 | 1 | Demo. Minute 15 (Phase cell 1) starts a report-card template from the template exchange, Appendix W feature 37; the exchange shares the global library and review step that the Tier 1 REQ-PLT-022 builds in the same slice |
| REQ-PLT-039 | 3 | SL-PLT-025 | 1 | Move, in part. The tenant record carries its owning partner from provisioning in phase 1, so no tenant row migrates later, which is the Tier 3 "design for it in the data model". The partner console, commissions and white-label options have no phase 1 need and move to phase 5 |
| REQ-SCH-026 | 2 | SL-SCH-236 | 2 | Demo. Reserve step R-09 (Phase cell 2) shows balanced class formation, Appendix W feature 20, and year close in CAP-SCH-04 is where next year's sections are formed |
| REQ-SCH-036 | 3 | SL-SCH-237 | 2 | Move, in part. Year close in CAP-SCH-04 must give graduates a status, and the alumni status with its `AlumniProfile` is that status. The alumni directory and events have no phase 2 need and move to phase 5 |
| REQ-ATT-013 | 2 | SL-ATT-223 | 2 | Shared. The Tier 1 REQ-ATT-012 builds the device integration interface for QR, NFC and kiosk check-in in the same slice; the biometric, RFID and turnstile adapters plug into it. The gate-scan pre-fill of minute 2 (Appendix W feature 26) runs through the same interface |
| REQ-ACA-007 | 2 | SL-ACA-203 | 2 | Demo. Reserve step R-19 (Phase cell 2 for the heatmap) shows the standards heatmap, Appendix W feature 42, over standards that arrive by the CASE import. Built once here; SL-INT-601 extends it in phase 5 (Section 4, phase 5) |
| REQ-ACA-017 | 2 | SL-ACA-214 | 2 | Locality. The similarity note sits on the submission and feedback that CAP-ACA-02 builds for the Tier 1 REQ-ACA-018; no demo step or Tier 1 flow needs it |
| REQ-ACA-019 | 2 | SL-ACA-214 | 2 | Locality. Audio feedback is one more attachment on the graded submission of the Tier 1 REQ-ACA-018; no demo step or Tier 1 flow needs it |
| REQ-ACA-023 | 2 | SL-ACA-216 | 2 | Locality. The question bank is an Academics aggregate built while Academics is open; the quiz player that uses it is the one stated exception to mobile parity in document 09 |
| REQ-ACA-024 | 2 | SL-ACA-217 | 2 | Locality. Online quizzes run on the question bank above and grade into the Tier 1 gradebook of CAP-ACA-02 |
| REQ-ACA-025 | 2 | SL-ACA-217 | 2 | Locality. Item analysis is computed from the quiz attempts of the same slice |
| REQ-ACA-026 | 2 | SL-ACA-216 | 2 | Locality. QTI 3 import and export of the question bank are built once here; SL-INT-600 adds the per-item report and the round trip for the REQ-INT-016 checklist in phase 5 (Section 4, phase 5) |
| REQ-ACA-027 | 2 | SL-ACA-218 | 2 | Locality, conditional. Online classes feed participation into Attendance while both are built in phase 2; document 34 already moves SL-ACA-218 to phase 5 if the Appendix B permission amendment it waits on has not landed by phase 2 |
| REQ-ACA-028 | 2 | SL-ACA-209, SL-ACA-400, SL-ACA-401, SL-ACA-402, SL-ACA-403, SL-ACA-404 | 2, 4 | Demo. Reserve step R-08 (Phase cell 4) shows the kindergarten daily sheet, Appendix W feature 19; SL-ACA-209 builds the sheet record in phase 2 and CAP-ACA-03 delivers it to guardians in phase 4. Both move to phase 5 together if the daily-sheet permission has not landed, as document 34 records |
| REQ-ACA-030 | 2 | SL-ACA-207 | 2; the launch half 4 | Move, and unlike the other Move rows it is the default in force now rather than waiting on ADR-0024, because the launch cannot be built where it stands. An LTI 1.3 launch needs the tool registration, third-party login and signing keys that SL-INT-411 builds in phase 4 (CAP-INT-02), so a phase 2 launch cannot run before then. The launch half of SL-ACA-207 (`LaunchLtiTool`) is built beside SL-INT-411 in phase 4, and in phase 2 SL-ACA-207 delivers the Tier 1 resource library (REQ-ACA-022) only; no phase 2 or 3 demo or exit criterion claims a launch, and the phase 4 exit runs the first. Logged for document 34, whose SL-ACA-207 row still names `LaunchLtiTool` in phase 2 |
| REQ-ASM-010 | 2 | SL-ASM-227, SL-ASM-228, SL-ASM-229 | 2 | Shared. The exam session that the Tier 1 REQ-ASM-009 schedules, seats and prints admit cards for carries its papers; SL-ASM-229's exam screens serve both |
| REQ-ASM-011 | 2 | SL-ASM-227 | 2 | Shared. The refusal, audit and alert on an exam paper are the guard on the paper workflow above; the paper cannot ship without them |
| REQ-ASM-012 | 2 | SL-ASM-228 | 2 | Shared. Sealing and the fixed print count close the paper workflow above against the registered candidates of REQ-ASM-009 |
| REQ-ASM-031 | 2 | SL-ASM-219 | 2 | Locality. Predicted grades are computed from the same mastery data as the heatmap below, and are never shown to a family |
| REQ-ASM-033 | 2 | SL-ASM-219 | 2 | Demo. Reserve step R-19 (Phase cell 2 for the heatmap, 5 for the suggestion) shows the standards heatmap, Appendix W feature 42, at rung 2 degrading to the raw heatmap |
| REQ-COM-015 | 2 | SL-COM-405, SL-COM-407, SL-COM-408 | 3 | Shared. The Tier 1 form builder of REQ-RQS-002 produces "consent forms and surveys"; publishing them to an audience with an OTP-confirmed signature is the Tier 2 step on top, built in the same slice |
| REQ-COM-016 | 2 | SL-COM-406, SL-COM-407, SL-MOB-404 | 3, 4 | Demo. Reserve step R-11 (Phase cell 3) shows policy and handbook acknowledgment, Appendix W feature 22 |
| REQ-DOC-009 | 2 | SL-DOC-408, SL-DOC-413 | 3 | Locality. OCR indexing sits in the Documents library that the Tier 1 REQ-DOC-008 builds in phase 3; no demo step or Tier 1 flow needs it |
| REQ-DOC-017 | 2 | SL-DOC-410 | 3 | Demo. Reserve step R-14 (Phase cell 4) shows the yearbook draft, Appendix W feature 36, built from Documents in phase 3 for the phase 4 demo |
| REQ-FIN-033 | 2 | SL-FIN-445 | 3 | Locality. Expenses and budgets sit on the financial periods Finance builds in phase 3; the requisition commitment already waits for Operations' WF-OPS-01 in phase 5, as document 34 records |
| REQ-FIN-034 | 3 | SL-FIN-443 | 3 | Shared. Nothing Tier 3 is built: the requirement says no general ledger is kept, and the Tier 1 accounting export of REQ-FIN-032 in the same slice is what serves it |
| REQ-RQS-019 | 2 | SL-RQS-421, SL-RQS-422, SL-RQS-423 | 3 | Locality. Duty rosters with swaps use the task and approval surfaces Requests builds in phase 3 (CAP-RQS-02); exam invigilation duties follow the phase 2 exams. No demo step or Tier 1 flow needs it |
| REQ-BEH-006 | 2 | SL-BEH-408 | 4 | Shared. The portfolio export of REQ-BEH-008 carries each badge, and the credential is built once in the Open Badges 3.0 format; SL-INT-602 adds the conformance items in phase 5 (Section 4, phase 5) |
| REQ-BEH-008 | 2 | SL-BEH-409, SL-BEH-413, SL-MOB-409 | 4 | Demo. Reserve step R-07 (Phase cell 4) shows a recognition reaching the portfolio and the family export, Appendix W feature 18 |
| REQ-RPT-013 | 3 | SL-RPT-409, SL-RPT-412 | 4 | Demo. Minute 14 (Phase cell 4) opens the cross-campus dashboard, Appendix W feature 27, which is this group view across the campuses of one tenant (ADR-0011) |
| REQ-RPT-016 | 2 | SL-RPT-418, SL-RPT-421 | 4 | Demo. Reserve step R-10 (Phase cell 4) shows the evidence folder for an inspection criterion, Appendix W feature 21 |
| REQ-RPT-017 | 2 | SL-RPT-419, SL-RPT-421 | 4 | Shared. The policies library and the minutes with tracked actions are the evidence R-10's folders link to (REQ-RPT-016, above), and SL-RPT-421's screens serve both |

**What deferral would save.** The slices above that build nothing but Tier 2 or Tier 3 work come to 57 slice-days in phases 1 to 4 (5 in phase 1, 24 in phase 2, 12 in phase 3, 16 in phase 4), 37 of them inside the MVP of Section 5, computed from document 34. The rows marked Move are the part ADR-0024 proposes to take out; the Locality rows are the next candidates if the MVP must come sooner.

## Decisions in force

| Decision | Record |
|---|---|
| Phases and the MVP cut line | Master brief Section 28 |
| Phase ranges derived from document 34's slice-days, not estimated | This document, Section 1; brief Section 28 corrected under ADR-0019 |
| Capabilities and slices, split by use case never by layer | ADR-0018 |
| Contract-first dependencies between streams | ADR-0018, `docs/plan/PLAN_SPEC.md` |
| Workflow timing is by capability, overriding the service phase where stated | This document, Section 4 |
| Every Tier 2 or Tier 3 requirement a phase 1 to 4 slice builds is listed with its reason; the product owner has not yet accepted the list | ADR-0024, Proposed; "Requirements built ahead of their tier" |
| Each standard's importer (CASE, QTI 3, Open Badges 3.0) is built once, in the phase its service is built, and extended for the REQ-INT-016 checklist in phase 5 | This document, Section 4, phase 5 |
| The quality engineer builds slices and is counted among the builders; the calendar allowance for Ramadan, holidays and annual leave is stated and included in the headline, not modelled by the generator | This document, Section 1 |

## Dependencies on other documents

| Document | What this one takes |
|---|---|
| `03-requirements-catalog.md` | Every requirement identifier and its owning service |
| `05-service-catalog.md` | The build phase per service |
| `31-business-rules-and-workflows.md` | Workflow ownership |
| `34-work-breakdown.md` | The slices inside every capability |
| Appendix N, Appendix O | The load scenarios, and the demo steps with the Phase cells and substitutions that close each phase |
| `16-test-strategy.md` part 12.2 | The demo gate per phase, read from the same Phase cells |
| `32-product-differentiation-and-demo.md` | The "Signature feature trace", cited by Appendix W feature number rather than copied |
| `docs/project/DECISIONS/0024-tier-2-built-early.md` | The decision record behind "Requirements built ahead of their tier" |

## Open points

| Point | Default | Owner | L | I | Score | In the register |
|---|---|---|---|---|---|---|
| Whether a read-only public API, OneRoster export and iCal move to Tier 1, per document 02 | Stays in phase 3 as CAP-INT-01, where document 34 builds it, except iCal which is already in phase 2 through CAP-SCD-03. This is the current default, not a decision | Product owner, Open Question 28, then an ADR | 3 | 3 | 9 | RISK-42 |
| Whether Finance is needed by the first customer in term one | No; it is outside the MVP | Product owner, Open Question 25 | 3 | 3 | 9 | RISK-32 |
| Team size and shape | Master brief Section 29, five to eight builders. Every phase range in Section 1 assumes it: phase 1 is 14 weeks with eight builders and 22 with five. By the same formula a team of four stretches phases 1 and 2 by about six weeks each over the five-builder figure and phases 3 to 5 by three to four weeks, while phase 6 stays inside its calendar floor. A different team changes the builders input of `schedule-34.mjs` and the ranges are recomputed | Product owner, Open Question 24 | 3 | 4 | 12 | RISK-03, RISK-06 |
| Whether the product owner accepts ADR-0024: the 35 requirements built ahead of their tier, the Move rows taken out of phases 1 to 4, and the Locality rows kept | Built as listed; document 34 is unchanged until the decision, and the Move rows are logged for it, except the launch half of REQ-ACA-030, which is built in phase 4 beside SL-INT-411 whatever the decision because it cannot run earlier. If the default is wrong, phases 1 to 4 carry up to 57 slice-days of Tier 2 and Tier 3 work, 37 of them inside the MVP | Product owner, with the architect | 4 | 3 | 12 | RISK-02 |
| Whether the calendar allowance of Section 1 is right for the team's actual calendar | About three weeks a year for Ramadan's shorter working day and the Eid and national holidays, plus about four weeks of annual leave per builder in rotation, about seven in all and inside the headline of Section 1; if wrong, phase dates read from the ranges slip by the difference, which the phase-end review records | Product owner | 3 | 2 | 6 | none |

## Review record

| Date | Reviewer | Result |
|---|---|---|
| 2026-09-21 | Plan build | Written from Section 28 and computed from documents 03, 05 and 31 |
| 2026-09-22 | Scorecard remediation, theme 2 | Ranges re-derived from document 34: phase 1 from 8 to 10 to 14 to 22 weeks, total from 58 to 74 to 61 to 93 weeks, MVP from about 38 capabilities and 30 to 36 weeks to 42 and 33 to 50 |
| 2026-09-22 | Scorecard remediation, theme 4 | CAP-INT-02 (phase 4) and CAP-INT-03 (phase 5) added for the REQ-INT-016 standards work; capability and workflow columns in Section 1 recounted from the Section 4 tables (phase 1 workflows 16 to 14, phase 6 "0 new" to 2); the Section 2 INT rows split across phases 1, 3 and 4 to match document 20's phase column, which moves the phase totals to 153, 103 and 61 with 561 unchanged |
| 2026-09-26 | Round-2 scorecard, Group E, then remediation round 3 | The group was blocked on consistency, feasibility and risk honesty. Amended: the phase demos and exit criteria restated from Appendix O's Phase cells, naming each substitution (phase 4 is fourteen minutes, not the full script); the stale "156" corrected to 153; the CASE, QTI 3 and Open Badges importers each built once; the quality engineer stated as a builder with the capacity shown, and the calendar allowance stated; `macos-latest` added to the dev-smoke runners; "Requirements built ahead of their tier" added with ADR-0024 and kit-lint R35 |
| 2026-09-26 | Round-3 scorecard, Group E, then remediation round 4 | The group was blocked on consistency. Amended: the phase 2 and 3 demo cells and exit criteria rebuilt from Appendix O's Phase cells, so minutes 11 and 13 (Phase cell 2) run in phase 2 and minutes 12 and 14 are shown there by their substitutions, as `16-test-strategy.md` part 12.2 says; annual leave added to the calendar allowance and the headline, now 71 to 108 calendar weeks, "about twenty months, plus or minus five", and the MVP reading eleven months; the launch half of REQ-ACA-030 moved to phase 4 as the default in force; the stale "Document 34 is asked to" sentence restated |

## How this document is verified

| Claim | Proof |
|---|---|
| Every service requirement is in exactly one phase | The Section 2 table is computed from document 03 by service phase; its counts add to 561, and with the 299 cross-cutting requirements to the 860 in document 03 |
| Every workflow has a capability | kit-lint R25 fails when a `WF-` heading of Appendix R, the catalog document 31 is generated from, appears in no `CAP-` row of Section 4; kit-lint R19 fails when a Section 4 row cites a `WF-` identifier Appendix R does not define |
| Every capability is broken into slices | kit-lint R25 fails when a `CAP-` row here has no `SL-` row under its `#### CAP-` heading in document 34. CAP-INT-02 holds SL-INT-411 and CAP-INT-03 holds SL-INT-600 to SL-INT-602 |
| Every requirement reaches a slice | Document 34 and document 20 together; a requirement with no slice is scope nobody will build |
| The ranges match the work | kit-lint R23 runs `tools/plan-build/schedule-34.mjs --check`, which recomputes the ranges from the slices in document 34 and fails when a Section 1 phase row, the launch total or the MVP line differs from what it computes |
| The ranges stay honest | Re-computed at the end of each phase from the slices actually delivered and the team actually present, and recorded in the review record, with the calendar actually lost to Ramadan, holidays and leave beside the allowance of Section 1 |
| Every Tier 2 or Tier 3 requirement built in phases 1 to 4 is listed with a reason, and nothing else is | Kit-lint R35 reads the tiers from document 03 and the phase sections of document 34, and fails when such a requirement is missing from "Requirements built ahead of their tier" or when a listed one is no longer built early, on every lint run |
| The phase demos and exit criteria match Appendix O's Phase cells | Kit-lint R34 checks, on every lint run, that every Appendix O step states its phase; that the Section 1 demo column and the exit criteria name the same minutes and substitutions is a review step by the `demo-director` agent before each phase demo, reading them against Appendix O and `16-test-strategy.md` part 12.2 |
