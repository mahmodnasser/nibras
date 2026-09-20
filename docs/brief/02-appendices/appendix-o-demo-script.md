# Appendix O. The Fifteen-Minute Demo

> Part of `docs/brief/02-appendices/`. Normative. Read with the master brief and the reference architecture.

Master brief Section 4 says a school owner watching a fifteen-minute demo must immediately understand the value and see things competitors do not have. This is that demo, choreographed minute by minute against the demo data in Appendix H. It is three things at once: the sales script, the golden-path release gate, and the end-to-end regression suite. If a step here cannot be performed, the release does not ship.

**Rules of the script.** One wow moment per minute. Every step names the persona on screen, the feature from Appendix W, and the test case that automates it. Two steps run in Arabic with right-to-left layout, because a bilingual product that demos only in English is not believable. One step runs with the network off. One step deliberately shows a permission boundary, because trust sells to school owners more reliably than features do.

**Setup.** Demo tenant "Al-Nibras International School", two campuses, 600 students, mid-term of the current year, one prior year complete. Reset to seed before every run. The demo login helper is compiled out of release builds, so the demo runs on ordinary accounts with known passwords held outside the repository.

---

## Act one: the morning (minutes 1 to 5)

| Min | Persona | What happens | Feature | Test |
|---|---|---|---|---|
| 1 | Principal, phone | Opens the app to the **morning brief**: six approvals, two unmarked classes, one attendance flag, yesterday's collections. Clears all six approvals with one tap each | 25, 1 | TC-RPT-006 |
| 2 | Teacher, phone | Opens **five-minute mode**. The register for 4B is already three-quarters filled from the gate scan and two approved leaves. Confirms four exceptions. Total elapsed on screen: under twenty seconds | 26, 34, 3 | TC-ATT-003 |
| 3 | Teacher, phone, **network off** | Marks the next class with airplane mode on. The pending badge appears. Network comes back; the queue drains; the register updates. Nothing was lost and nothing was claimed complete while queued | 12 | TC-MOB-001 |
| 4 | Parent, phone, **in Arabic, right to left** | Opens to the **calm screen**: one card per child. For the older child, "nothing needs your attention today". For the younger, one absence notice with a one-tap excuse submission | 35, 7 | TC-MOB-004 |
| 5 | Principal, web | An attendance flag opens the **because panel**: the three inputs, the rule that fired, and an override with a reason. Then the **intervention playbook** opens with an owner and a review date | 28, 30, 4 | TC-RPT-008 |

## Act two: the term (minutes 6 to 10)

| Min | Persona | What happens | Feature | Test |
|---|---|---|---|---|
| 6 | Coordinator, web | Opens the timetable, drags one period, sees the conflict highlight in real time, resolves it, publishes. Already-recorded attendance for the week is untouched, and the screen says so | 5 | TC-SCD-001 |
| 7 | Teacher, web | Mark entry grid: types a column of marks with the keyboard only, pastes a block from a spreadsheet, hits a validation error on an out-of-range mark, fixes it, submits for moderation | 6 | TC-ASM-001 |
| 8 | Principal, web | Approves the grading period, locks it, and starts the report card batch for a grade. The progress counter runs live over the real-time channel; a report card appears with its QR code | 6 | TC-ASM-001 |
| 9 | Anyone, public page | Scans the QR code on the printed report card. The public verification page confirms it, in both languages, without a login. Then a revoked certificate is scanned and correctly refuses | 6, 14 | TC-DOC-002 |
| 10 | Accountant, web | Runs an invoice batch with preview and approval, shows one parent's **split payer** with a company sponsor, records a payment, prints a receipt with the amount in words in Arabic | 1, 10 | TC-FIN-001 |

## Act three: trust, safety, and the close (minutes 11 to 15)

| Min | Persona | What happens | Feature | Test |
|---|---|---|---|---|
| 11 | Parent, web, **in Arabic** | Opens **guardian transparency**: which roles read their child's sensitive records and when, the consents they have given, and the retention clock. Downloads their child's full data export | 31, 14 | TC-AUD-002 |
| 12 | Teacher, web, **permission boundary** | Tries to open another teacher's class and the wellbeing record of a student they do not support. Both refuse with a clear message, and both attempts appear in the audit viewer seconds later | 14 | TC-SEC-001 |
| 13 | Security officer, tablet | Triggers **emergency mode**. The staff app switches to roll call by location, acknowledgements stream in, the "not yet accounted for" list shrinks, and reunification verifies one pickup | 32, 8 | TC-ATT-004 |
| 14 | School owner, web | Opens the cross-campus dashboard: enrolment against capacity, collections against target, attendance, and the risk list. Clicks a number and uses **explain this number** to drill to the records behind it | 27, 1 | TC-RPT-007 |
| 15 | Platform operator, web | Provisions a brand-new tenant live. The provisioning saga reports progress step by step. The **smart defaults engine** shows what it inferred from country and school type, and the new school's first administrator invitation is sent | 29, 15 | TC-PLT-003 |

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

Every row's test case runs in the end-to-end suite against the demo tenant on every release. `/demo-script` regenerates the presenter notes from this table, so the script and the tests cannot drift apart. A signature feature in Appendix W with no row here fails the demo director's review and is not a signature feature.
