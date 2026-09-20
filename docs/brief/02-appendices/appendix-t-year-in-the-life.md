# Appendix T. A Year in the Life

> Part of `docs/brief/02-appendices/`. Normative. Read with the master brief and the reference architecture.

A feature list cannot tell you whether a product runs a school. A year can. This appendix walks one demo tenant through twelve months and names, month by month, every workflow from Appendix R and every business rule from Appendix S that the month exercises.

It does three jobs at once:

1. **Coverage proof.** `tools/kit-lint` rule R10 fails if any workflow or rule is never exercised here. A process nobody walks through in a whole year is either dead or undiscovered, and both are worth knowing before the build starts.
2. **The end-to-end regression script.** Each month is a test suite that runs against the demo tenant.
3. **The narrative a school recognises.** If a school reads this and says "that is not how our year works", the brief is wrong and it is cheap to find out now.

**The tenant.** Al-Nibras International School from Appendix H: two campuses, 600 students, 60 staff, a completed prior year and a current year. Dates are the northern school year; a southern-hemisphere or Gulf calendar shifts the months without changing the sequence.

**How to run it.** `/simulate-year` walks these months against the plan or a running system and reports anything unexercised. A month passes when every identifier listed under it has a green test.

---

## August, before the year opens

The year is created, campuses and sections are confirmed, staff contracts start, the timetable is solved and published, and the school administrator invites the people who will use the system. A new tenant is provisioned live during this month, so the smart defaults engine and the provisioning saga are exercised before a single child arrives.

**Workflows exercised**

| Workflow | Name |
|---|---|
| `WF-IDN-01` | Invitation or join-code joining |
| `WF-IDN-02` | Parent self-registration and child linking |
| `WF-IDN-03` | Duplicate account merge |
| `WF-IDN-04` | Delegation during absence |
| `WF-IDN-05` | Role change with four-eyes approval |
| `WF-IDN-06` | Offboarding and access revocation |
| `WF-PLT-01` | Tenant signup to live |
| `WF-PLT-02` | Trial conversion and plan change |
| `WF-PLT-03` | Suspension, export, and deletion |
| `WF-SCH-01` | Transfer or withdrawal with clearance |
| `WF-SCH-02` | End of year close and rollover |
| `WF-SCH-03` | Year archival and reopen |
| `WF-SCH-04` | Mid-year campus transfer |

**Rules exercised**

| Rule | Name |
|---|---|
| `BR-IDN-001` | Permission dependency |
| `BR-IDN-002` | Data-scope evaluation order |
| `BR-IDN-003` | Delegation validity window |
| `BR-IDN-004` | Four-eyes for high-risk grants |
| `BR-IDN-005` | The last super administrator cannot be removed |
| `BR-IDN-006` | Join-method default role |
| `BR-IDN-007` | One person linked across tenants |
| `BR-IDN-008` | Permission version invalidates caches |
| `BR-IDN-009` | Impersonation needs consent and a time box |
| `BR-L10N-001` | Arabic search normalization |
| `BR-L10N-002` | Numeral rendering |
| `BR-L10N-003` | Hijri display, Gregorian source of truth |
| `BR-L10N-004` | Amounts in words in both languages |
| `BR-L10N-005` | Arabic plural forms |
| `BR-L10N-006` | Pinned culture on every host |
| `BR-L10N-007` | Bilingual names and fallback |
| `BR-PLT-001` | Soft warn then hard block on plan limits |
| `BR-PLT-002` | What read-only mode allows |
| `BR-PLT-003` | Deletion cooling-off |
| `BR-PLT-004` | Data residency pinning at provisioning |
| `BR-PLT-005` | Usage metering counts |
| `BR-PLT-006` | Tenant export completeness |
| `BR-SCD-001` | Hard versus soft constraints |
| `BR-SCD-002` | Consecutive-period limit |
| `BR-SCD-003` | Part-time availability and weekly load |
| `BR-SCD-004` | Travel time between campuses |
| `BR-SCD-005` | Cover fairness score |
| `BR-SCD-006` | Publishing does not alter recorded attendance |
| `BR-SCD-007` | Room booking holds and buffers |

---

## September, the year opens

Late admissions convert to enrolments, the first registers are marked, coursework begins, the first invoice run goes out, and every family receives its first digest. This is the month where the morning peak is real for the first time and the warm-up job earns its place.

**Workflows exercised**

| Workflow | Name |
|---|---|
| `WF-ACA-01` | Assignment lifecycle |
| `WF-ADM-01` | Inquiry to enrollment |
| `WF-ADM-02` | Re-enrollment with fee settlement check |
| `WF-ATT-01` | Daily attendance to intervention |
| `WF-ATT-02` | Early dismissal and gate pickup |
| `WF-FIN-01` | Fee plan to collection and escalation |
| `WF-FIN-02` | Invoice reversal, credit note, and refund |
| `WF-FIN-03` | Cheque receipt and bounce |
| `WF-FIN-04` | Scholarship award |
| `WF-FIN-05` | Payer change to sponsor |
| `WF-FIN-06` | Cashier day close |

**Rules exercised**

| Rule | Name |
|---|---|
| `BR-ADM-001` | Age eligibility by cut-off date |
| `BR-ADM-002` | Required documents by grade and nationality |
| `BR-ADM-003` | Seat capacity and override |
| `BR-ADM-004` | Offer expiry |
| `BR-ADM-005` | Waiting-list ranking with sibling priority |
| `BR-ADM-006` | Duplicate applicant detection |
| `BR-ATT-001` | Daily versus per-period derivation |
| `BR-ATT-002` | Attendance lock window |
| `BR-ATT-003` | Approved leave pre-fills excused |
| `BR-ATT-004` | Late converts to absent |
| `BR-ATT-005` | Late accumulation adds a derived absence |
| `BR-ATT-006` | Consecutive absence threshold |
| `BR-ATT-007` | Cumulative absence ladder |
| `BR-ATT-008` | Attendance percentage denominator |
| `BR-ATT-009` | Mid-term section move splits the record |
| `BR-ATT-010` | Offline mark arriving after the lock window |
| `BR-ATT-011` | Unmarked class reminder |
| `BR-FIN-001` | Fee plan installment generation |
| `BR-FIN-002` | Pro-rata by days |
| `BR-FIN-003` | Pro-rata by months |
| `BR-FIN-004` | Discount stacking order |
| `BR-FIN-005` | Sibling discount eligibility |
| `BR-FIN-006` | Scholarship cap |
| `BR-FIN-007` | Late fee accrual and cap |
| `BR-FIN-008` | Payment allocation order |
| `BR-FIN-009` | Overpayment becomes credit |
| `BR-FIN-010` | Refund from credit versus from payment |
| `BR-FIN-011` | Rounding per currency |
| `BR-FIN-012` | Tax inclusive versus exclusive per item |
| `BR-FIN-013` | Gapless numbering per series under concurrency |
| `BR-FIN-014` | Posted documents are immutable |
| `BR-FIN-015` | Cheque bounce reversal and fee |
| `BR-FIN-016` | Service restriction rules |
| `BR-FIN-017` | Active student definition for SaaS billing |
| `BR-FIN-018` | Proration on a plan change |
| `BR-FIN-019` | Split payers by percentage |
| `BR-NOT-001` | Urgency versus quiet hours |
| `BR-NOT-002` | Channel fallback order |
| `BR-NOT-003` | Digest eligibility |
| `BR-NOT-004` | Deduplication window |
| `BR-NOT-005` | SMS credit check before send |
| `BR-NOT-006` | Preference resolution order |

---

## October, the term settles

First assessments are entered and moderated, the Request Center carries its first real volume of leave and document requests, and behaviour records begin to accumulate. Approval chains and SLA escalation are exercised under load rather than in a demo.

**Workflows exercised**

| Workflow | Name |
|---|---|
| `WF-ASM-01` | Exam to report card |
| `WF-ASM-02` | Grade appeal and post-lock change |
| `WF-ASM-03` | Exam paper setting, review, and printing |
| `WF-BEH-01` | Incident to intervention |
| `WF-RQS-01` | Service request lifecycle |

**Rules exercised**

| Rule | Name |
|---|---|
| `BR-ASM-001` | Weighted category average |
| `BR-ASM-002` | Empty category re-weight |
| `BR-ASM-003` | Drop the lowest scores |
| `BR-ASM-004` | Best of N |
| `BR-ASM-005` | Mandatory component missing |
| `BR-ASM-006` | Late joiner partial-term re-weighting |
| `BR-ASM-007` | Absent versus exempt components |
| `BR-ASM-008` | Rounding at a configurable number of decimals |
| `BR-ASM-009` | Letter grade boundaries |
| `BR-ASM-010` | GPA scale conversion |
| `BR-ASM-011` | Rank and ties |
| `BR-ASM-012` | Promotion eligibility |
| `BR-ASM-013` | Honors thresholds |
| `BR-ASM-014` | Grade change after lock creates a new version |
| `BR-RQS-001` | Approval chain routing by amount |
| `BR-RQS-002` | Approval chain routing by duration |
| `BR-RQS-003` | SLA calendars per campus |
| `BR-RQS-004` | Auto-approval conditions |
| `BR-RQS-005` | Escalation on breach |
| `BR-RQS-006` | Effect execution and compensation |

---

## November, the first report cycle

Marks are locked, report cards generate in a batch, parents acknowledge them, and the first grade appeal arrives. Document generation, QR verification and the reissue path all run for real.

---

## December, mid-year pressure

Overdue fees reach the escalation ladder, the first restriction policy applies and is lifted, wellbeing interventions open from early-warning flags, and staff take leave that needs cover. The month where the product either holds together across services or does not.

**Workflows exercised**

| Workflow | Name |
|---|---|
| `WF-HR-01` | Staff leave to substitution |
| `WF-HR-02` | Staff hiring to onboarding |
| `WF-HR-03` | Teaching licence expiry compliance |
| `WF-HR-04` | Payroll input cycle |
| `WF-WEL-01` | Accommodation plan to exam sitting |
| `WF-WEL-02` | Clinic visit to sent home |
| `WF-WEL-03` | Medication authorization and administration |
| `WF-WEL-04` | Safeguarding concern escalation |
| `WF-WEL-05` | Daily wellbeing check-in escalation |

**Rules exercised**

| Rule | Name |
|---|---|
| `BR-WEL-001` | Visibility levels |
| `BR-WEL-002` | Break-glass access |
| `BR-WEL-003` | Events carry no clinical detail |
| `BR-WEL-004` | Medication authorization and sending home |

---

## January, the second term begins

A mid-year timetable change publishes without disturbing recorded attendance, a student transfers between campuses, and transport and library operations run their routine cycles.

**Workflows exercised**

| Workflow | Name |
|---|---|
| `WF-OPS-01` | Purchase requisition to asset |
| `WF-OPS-02` | Library lending and fines |
| `WF-OPS-03` | Transport subscription change |
| `WF-OPS-04` | Facility booking approval |
| `WF-OPS-05` | Safety incident and drill logging |

---

## February, the audit month

An access review campaign runs, a data subject access request arrives, a sensitive export is requested and approved, break-glass access is used once and reviewed, and the data quality center is worked through. Nothing here is a feature demonstration; it is the audit test from master brief Section 4, on a real data set.

**Workflows exercised**

| Workflow | Name |
|---|---|
| `WF-DATA-01` | Legacy import with dry run and rollback |
| `WF-PRV-01` | Data subject access request |
| `WF-PRV-02` | Sensitive export approval |
| `WF-SEC-01` | Access review campaign |
| `WF-SEC-02` | Break-glass access |
| `WF-SEC-03` | Consented impersonation |

---

## March, safety and inspection

A drill runs in emergency mode with reunification, a safeguarding concern is raised and escalated, and the school assembles its inspection evidence pack.

---

## April, admissions for next year

The re-enrolment campaign opens, offers go out for next year, deposits are collected, and a family with an outstanding balance meets the re-enrolment block and then resolves it.

---

## May, examinations

The exam timetable publishes, seating and invigilation are assigned, accommodations flow from education plans into the exam room, and papers move through the setter and reviewer workflow.

---

## June, results and year end

Final results calculate, promotion decisions are made, transcripts and leaving certificates issue, withdrawal clearance runs for students who are leaving, and the financial year reconciles.

---

## July, close and carry forward

The year closes and becomes read-only, structures roll forward, an on-premises customer upgrades with a backup and a tested rollback, the quarterly restore drill runs, and a tenant that is leaving exports and is deleted with a certificate.

**Workflows exercised**

| Workflow | Name |
|---|---|
| `WF-INF-01` | On-premises upgrade with rollback |
| `WF-INF-02` | Release rollout with canary and rollback |
| `WF-INF-03` | Restore and failover drill |

---

## Coverage summary

| | Count |
|---|---|
| Workflows in Appendix R | 52 |
| Workflows exercised here | 52 |
| Rules in Appendix S | 95 |
| Rules exercised here | 95 |

Both pairs must be equal. `kit-lint` R10 enforces it, so this table cannot quietly drift out of date.

## How this appendix is verified

`/simulate-year` runs the months in order against the demo tenant and reports, per month, which identifiers have a passing test and which do not. Before a release, every month must be green. The run is recorded with its date in `docs/project/CHANGELOG.md`, because a simulation nobody ran proves nothing.
