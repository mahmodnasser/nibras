# Appendix O. The Fifteen-Minute Demo

> Part of `docs/brief/02-appendices/`. Normative. Read with the master brief and the reference architecture.

Master brief Section 4 says a school owner watching a fifteen-minute demo must immediately understand the value and see things competitors do not have. This is that demo, choreographed minute by minute against the demo data in Appendix H. It is three things at once: the sales script, the golden-path release gate, and the end-to-end regression suite. If a step here cannot be performed, the release does not ship.

**Rules of the script.** One wow moment per minute. Every step names the persona on screen, the feature from Appendix W, the test case that automates it, and the build phase from which it runs as scripted. Two steps run in Arabic with right-to-left layout, because a bilingual product that demos only in English is not believable. One step runs with the network off. One step deliberately shows a permission boundary, because trust sells to school owners more reliably than features do.

**Phases.** The **Phase** column is the phase in `docs/plan/17-roadmap.md` at whose end-of-phase demo the minute first runs exactly as written, because every capability it needs has landed. A phase demo held earlier runs the substitution in "Before a minute's phase" below and says so on screen. **The MVP cut line** is the end of phase 2 plus the phase 3 subset named in that document: the attendance and document request types, the communication capabilities and certificate verification. A minute is never shown as scripted before its phase.

**Features with no minute of their own.** Feature 23 (live, animated interface) is present in every minute and has no step of its own; the four-way visual snapshots behind TC-UX-001 guard it, and the presenter never claims it in words. Eighteen other signature features are demonstrated through the reserve bank below. Feature 39 (calendar-aware scaling) is no longer a signature feature under ADR-0019 and has no step.

**Setup.** Demo tenant "Al-Nibras International School", two campuses, 600 students, mid-term of the current year, one prior year complete. Reset to seed before every run. The demo login helper is compiled out of release builds, so the demo runs on ordinary accounts with known passwords held outside the repository.

---

## Act one: the morning (minutes 1 to 5)

| Min | Persona | What happens | Feature | Test | Phase |
|---|---|---|---|---|---|
| 1 | Principal, phone | Opens the app to the **morning brief**: six approvals, two unmarked classes, one attendance flag, yesterday's collections. Clears all six approvals with one tap each | 25, 1 | TC-RPT-006 | 4 (Reporting). Earlier: substitution |
| 2 | Teacher, phone | Opens **five-minute mode**. The register for 4B is already three-quarters filled from the gate scan and two approved leaves. Confirms four exceptions. Total elapsed on screen: under twenty seconds | 26, 34, 3 | TC-ATT-003 | 2, at the MVP cut line, whose request subset supplies the approved leaves |
| 3 | Teacher, phone, **network off** | Marks the next class with airplane mode on. The pending badge appears. Network comes back; the queue drains; the register updates. Nothing was lost and nothing was claimed complete while queued | 12 | TC-MOB-001 | 2 |
| 4 | Parent, phone, **in Arabic, right to left** | Opens to the **calm screen** with the data-saver profile on, and the presenter says so: one card per child. For the older child, "nothing needs your attention today". For the younger, one absence notice with a one-tap excuse submission | 35, 7, 44 | TC-MOB-004, TC-MOB-005 | 2; the data-saver profile from 4 |
| 5 | Principal, web | An attendance flag opens the **because panel**: the three inputs, the rule that fired, and an override with a reason. Then the **intervention playbook** opens in the same side panel with an owner and a review date | 28, 30, 4 | TC-RPT-008 | 4 (Reporting). Earlier: substitution |

## Act two: the term (minutes 6 to 10)

| Min | Persona | What happens | Feature | Test | Phase |
|---|---|---|---|---|---|
| 6 | Coordinator, web | Opens the timetable, drags one period, sees the conflict highlight in real time, resolves it, publishes. Already-recorded attendance for the week is untouched, and the screen says so | 5 | TC-SCD-001 | 2 |
| 7 | Teacher, web | Mark entry grid: types a column of marks with the keyboard only, pastes a block from a spreadsheet, hits a validation error on an out-of-range mark, fixes it, submits for moderation | 6 | TC-ASM-001 | 2 |
| 8 | Principal, web | Approves the grading period, locks it, and starts the report card batch for a grade. The progress counter runs live over the real-time channel; a report card appears with its QR code | 6 | TC-ASM-001 | 2 |
| 9 | Anyone, public page | Scans the QR code on the printed report card. The public verification page confirms it, in both languages, without a login. Then a revoked certificate is scanned and correctly refuses | 6, 14 | TC-DOC-002 | 2 for the report card; the revoked certificate at the MVP cut line |
| 10 | Accountant, web | Approves an invoice batch previewed before the meeting, shows one parent's **split payer** with a company sponsor, and prints a receipt with the amount in words in Arabic. Recording a payment is reserve step R-15 | 1, 10 | TC-FIN-001 | 3 (Finance). Earlier: substitution |

## Act three: trust, safety, and the close (minutes 11 to 15)

| Min | Persona | What happens | Feature | Test | Phase |
|---|---|---|---|---|---|
| 11 | Parent, web, **in Arabic** | Opens **guardian transparency**: which roles read their child's sensitive records and when, the consents they have given, and the retention clock. Requests their child's full data export, sees it accepted with its progress, and opens an export prepared at the start of the meeting | 31, 14 | TC-AUD-002 | 2 |
| 12 | Teacher, web, **permission boundary** | Uses the **command palette** to reach another teacher's class, then tries to open the wellbeing record of a student they do not support. Both refuse with a clear message, and both attempts appear in the audit viewer seconds later | 14, 11 | TC-SEC-001, TC-WEB-001 | 5 (Wellbeing). Earlier: substitution |
| 13 | Security officer, tablet | Triggers **emergency mode**. The staff app switches to roll call by location, acknowledgements stream in, the "not yet accounted for" list shrinks, and the minute ends on reunification verifying one pickup | 32, 8 | TC-ATT-004 | 2 |
| 14 | School owner, web | Opens the cross-campus dashboard: enrolment against capacity, collections against target, attendance, and the risk list. Clicks a number and uses **explain this number** to drill to the records behind it | 27, 1 | TC-RPT-007 | 4 (Reporting). Earlier: substitution |
| 15 | Platform operator, web | Provisions a brand-new tenant live. The provisioning saga reports its first steps, then the presenter switches to a tenant provisioned at the start of the meeting. The **smart defaults engine** shows what it inferred from country and school type, the new school's report-card template is started from the **template exchange**, and the first administrator invitation is sent, valid for 14 days with a reminder at day 7 | 29, 15, 37 | TC-PLT-802, TC-PLT-803, TC-PLT-003 | 1 |

TC-PLT-003 in minute 15 keeps its Appendix R meaning: provisioning completes and the owner invitation is sent. TC-DOC-002 in minute 9 is the QR verification test; the yearbook test of feature 36 is TC-DOC-801.

---

## Before a minute's phase

Five minutes need capabilities that land after the phase demos of `docs/plan/17-roadmap.md` would first show them. Until the phase in their row, a phase demo runs the substitution below, names it as a substitution, and does not claim the scripted feature.

| Min | Runs as scripted from | What is missing earlier | Substitution shown earlier | Test |
|---|---|---|---|---|
| 1 | Phase 4 | The morning brief and the dashboard cards (Reporting) | At the MVP cut line and in phase 3: the principal clears the pending attendance and document requests from the phone. No morning brief is shown | TC-RQS-002 |
| 5 | Phase 4 | The because panel and the playbook library (Reporting); the general panel contract for every automated action follows in phase 5 | Phases 2 and 3: the attendance threshold flag shows its rule identifier and inputs and opens an intervention with an owner. No playbook library and no early-warning model | TC-RPT-008, run against the threshold rule |
| 10 | Phase 3 | Invoicing, payments and sponsors (Finance) | Phase 2 and the MVP cut line: the bilingual-documents minute, an Arabic right-to-left class list and a transfer certificate in Arabic with its QR code | TC-L10N-202, TC-L10N-301 |
| 12 | Phase 5 | The wellbeing record (Wellbeing) | Phases 2 to 4: the second refusal uses a custody-restricted student record in place of a wellbeing record | TC-SEC-001 |
| 14 | Phase 4 | The cross-campus dashboard and explain this number (Reporting) | Phases 2 and 3: reserve step R-01, Arabic-aware search | TC-L10N-311 |

---

## The reserve bank

Every signature feature without a minute has a sixty-second reserve step. A presenter swaps at most two reserve steps into a demo, each in place of a minute of the same act, for a buyer who needs to see that feature. Reserve steps are part of the release gate: each test runs in the end-to-end suite against the demo tenant on every release, exactly as the fifteen minutes do. A reserve step is never shown before the phase in its row.

| Step | Persona | What happens | Feature | Test | Phase |
|---|---|---|---|---|---|
| R-01 | Registrar, web, **in Arabic** | Types a guardian's name with and without hamza and with and without taa marbuta; each spelling finds the same record | none (bilingual data) | TC-L10N-311 | 2 |
| R-02 | Principal and teacher, web | Both open the same child's timeline; the teacher's shows only the entries the teacher's role may see | 2 | TC-RPT-002 | 2 |
| R-03 | Administrator, web | A dry run of a 600-row student file shows three problem rows; the file is imported, then rolled back | 9 | TC-DOC-001 | 3 |
| R-04 | Registrar, web | Two guardians with the same national identifier surface with the rule that found them and a merge link | 13 | TC-RPT-004 | 4 |
| R-05 | Parent, two phones | Two schools' branded apps side by side, from one build. Presented as parity, not as a difference | 16 | TC-MOB-002 | 4 |
| R-06 | Teacher, web | A class photo with one child lacking media consent is blocked at publishing | 17 | TC-COM-001 | 3 |
| R-07 | Teacher, web, then parent, phone | A recognition added by the teacher appears in the student's portfolio and in the family export | 18 | TC-BEH-001 | 4 |
| R-08 | Kindergarten teacher, tablet, then parent, phone | Five taps on the tablet produce the daily sheet on the parent's phone | 19 | TC-ACA-001 | 4 |
| R-09 | Coordinator, web | One pass forms four balanced sections; one drag shows the balance change live | 20 | TC-SCH-001 | 2 |
| R-10 | Principal, web | The evidence folder for one inspection criterion, already filled from the term's records | 21 | TC-RPT-005 | 4 |
| R-11 | Principal, web | A handbook is sent to staff; the missing acknowledgments are listed and chased | 22 | TC-COM-002 | 3 |
| R-12 | IT lead, web | A webhook fires to a request inspector when attendance is marked, and a staff calendar updates by iCal | 24 | TC-INT-001 | 3; the iCal half from 2 |
| R-13 | Administrator, web | The campus floor plan shows free rooms for period 3, and one is booked | 33 | TC-OPS-001 | 5 |
| R-14 | Principal, web | The yearbook draft for a grade, built from consented media only | 36 | TC-DOC-801 | 4 |
| R-15 | Accountant, web | A payment is recorded and allocated across two invoices, and the receipt is printed | 10 | TC-FIN-001 | 3 |
| R-16 | IT lead, web | A sample ministry-export plug-in is installed and run on the demo tenant | 38 | TC-INT-002 | 3 |
| R-17 | IT lead, web | A worker is stopped, the queue backs up, the replay runs inside the replay policy, and the operator note appears | 40 | TC-INF-801 | 6 |
| R-18 | IT lead, web | A settings export, a one-line diff, review, and apply | 41 | TC-PLT-804 | 1 |
| R-19 | Teacher, web | The standards heatmap for a class, with the rung 2 suggestion switched off to show the fallback | 42 | TC-ASM-002 | 2 for the heatmap; the suggestion from 5 |
| R-20 | Coordinator, web | A cover request shows the least-loaded eligible teacher and the load totals behind it | 43 | TC-HR-001 | 5 |

R-16, R-17 and R-18 are shown to an IT lead only. Feature 15 (demo mode with one-click reset) is exercised before every run by the reset in the setup above and is tested by TC-PLT-801.

---

## What the demo deliberately does not do

| Not shown | Why |
|---|---|
| Every module | Fifteen minutes shows depth in the daily loop. Breadth is the feature catalog, and a breadth demo convinces nobody |
| Anything at rung 3 or 4 | The product must sell at rung 1. A demo that needs a language model available is a demo that fails in a school's meeting room |
| Configuration screens, beyond minute 15 | Administrators are convinced by the outcome, not by the settings page |
| A perfect data set | The demo tenant has a student with an outstanding balance, one at-risk student, and a bounced cheque, because a spotless demo reads as a mock-up |

## Failure handling during a live demo

Each step has a fallback that keeps the story moving. The presenter never debugs in front of a customer.

| If this fails | Do this |
|---|---|
| Network is unavailable | Run the whole of act one, which is the offline story anyway, and pick up act two on reconnect |
| The report card batch is slow | Show the progress counter, then switch to a batch generated before the meeting |
| Push notification does not arrive | Show the in-app inbox, which carries the same message, and say plainly that push depends on the device |
| Any step errors | Move on, note it, and follow up. A visible error handled calmly damages credibility far less than five minutes of silence |

## How this appendix is verified

Every row's test case, in the fifteen minutes and in the reserve bank, runs in the end-to-end suite against the demo tenant on every release. `/demo-script` regenerates the presenter notes from these tables, so the script and the tests cannot drift apart. A signature feature in Appendix W with neither a minute nor a reserve step here fails the demo director's review and is not a signature feature.
