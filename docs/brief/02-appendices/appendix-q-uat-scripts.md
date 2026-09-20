# Appendix Q. User Acceptance Test Scripts per Role

> Part of `docs/brief/02-appendices/`. Normative. Read with the master brief and the reference architecture.

These scripts are written for the school, not for the test team. A principal, a teacher or an accountant follows them on the demo tenant from Appendix H with no technical help and signs each step pass or fail. Master brief Section 24 makes user acceptance a quality gate; these are the scripts that gate runs.

**Rules for every script.**

| Rule | Detail |
|---|---|
| Test case ids | `TC-<AREA>-<NNN>` with the AREA codes from Appendix L. The same id appears in the service test plan, so a failure here points at one suite |
| Data | Appendix H demo tenants only. One-click reset before each run, so a script is repeatable |
| Language | Every script has at least one step performed with the interface set to Arabic, with the layout mirrored right to left and Arabic-Indic or Western numerals as the tenant's setting dictates |
| Offline | Every script whose role works away from a desk has at least one step performed with the device in airplane mode |
| Boundary | Every script has at least one step where the person deliberately tries something they must not be able to do. The expected result is a refusal, and a script with no refusal step is not accepted |
| Evidence | The tester records the screen and the time; a failed step captures the correlation id from the error, which maps to a code in Appendix K |

---

## Q.1 Principal

| # | Do this | Expect this | Test case |
|---|---|---|---|
| 1 | Sign in on a phone at 07:30 and read the morning brief | Approvals, unmarked attendance, staff absent with cover state, incidents, at-risk students, visitors on site, each card linking to one action | `TC-RPT-001` |
| 2 | Open "unmarked attendance" and tap through to the class | The teacher, the period and a nudge action; no dead end | `TC-ATT-101` |
| 3 | Approve three pending requests from the card without leaving the home screen | Each approval confirms in under 2 seconds and the count drops | `TC-RQS-101` |
| 4 | Switch the interface to Arabic and repeat step 3 | Layout mirrors right to left, the approve button (موافقة) sits on the left, dates read in the tenant's calendar, and nothing is clipped or reversed inside a number | `TC-L10N-101` |
| 5 | Open a student's Student 360 page and read the timeline | Attendance, grades, behavior, fees, communication and documents in one chronological view, filtered to what the principal may see | `TC-SCH-101` |
| 6 | On the same page, try to open the counseling case note | The entry shows as existing but the note does not open; the product does not reveal content | `TC-WEL-101` |
| 7 | Use break-glass on a safeguarding record, giving a reason | Access is granted for a limited window, the reason is recorded, the safeguarding officer is alerted, and the read appears in the access log | `TC-SEC-101` |
| 8 | Open an "explain this number" link on the attendance rate | The records behind the figure and the scheme version that produced it | `TC-RPT-102` |
| 9 | Try to change a mark on a locked report card | Refused with `ASSESSMENT_POST_LOCK_CHANGE_REFUSED`, and the grade-appeal path is offered | `TC-ASM-101` |
| 10 | Start emergency mode from the phone and send a drill broadcast | Roll call by location opens, the broadcast sends on push and SMS, and the "who has not confirmed" list updates live | `TC-ATT-102` |
| 11 | Turn on airplane mode and reopen the morning brief | The last synced brief shows with a clear "as of" time; write actions are visibly disabled rather than failing silently | `TC-MOB-101` |
| 12 | Restore the network | Queued approvals apply once each, and the counts match the server | `TC-MOB-102` |

## Q.2 Teacher

| # | Do this | Expect this | Test case |
|---|---|---|---|
| 1 | Sign in on a phone and read the five-minute home | Next class, attendance to mark, submissions to grade, messages, cover alert, and nothing else | `TC-MOB-201` |
| 2 | Open today's first class and tap "all present" | 25 students marked in one action, with the exceptions still editable | `TC-ATT-201` |
| 3 | Mark two students absent and one late, then save | Saved in under 60 seconds end to end from opening the class | `TC-ATT-202` |
| 4 | Put the device in airplane mode and mark the second class | Marking works, a clear offline badge shows, and the queue count is visible | `TC-ATT-203` |
| 5 | Restore the network | The queue clears, each mark applies exactly once, and no duplicate appears in the register | `TC-MOB-202` |
| 6 | Have a colleague mark one of the same students differently from another device, then sync | Both values with their times are shown and the teacher chooses; nothing is resolved silently | `TC-ATT-204` |
| 7 | Publish an assignment for tomorrow in a class that already has three | The homework ceiling warning appears with the day's load and an alternative date | `TC-ACA-201` |
| 8 | Grade eight submissions in the fast grid using the keyboard only | Focus moves cell to cell, no mouse needed, and each save is immediate | `TC-ACA-202` |
| 9 | Switch the interface to Arabic and enter marks | Numbers stay left to right inside a right-to-left layout, the grid (الدرجات) scrolls from the right, and the comment box accepts mixed Arabic and English | `TC-L10N-201` |
| 10 | Accept a class-wide AI-drafted comment without reading it | The product refuses to publish an unreviewed draft and shows it in review state | `TC-AI-201` |
| 11 | Try to approve your own marks for publication | Refused; approval belongs to the coordinator and the button is not present | `TC-ASM-201` |
| 12 | Try to open another teacher's class register | Refused with a permission message and a link to the coordinator | `TC-SEC-201` |
| 13 | Read a parent message and reply | The reply sends, the read receipt shows, and quiet hours are respected | `TC-COM-201` |

## Q.3 Homeroom Teacher

| # | Do this | Expect this | Test case |
|---|---|---|---|
| 1 | Open the homeroom card on the home screen | Absent today, excuses to review, flags for the class, birthdays | `TC-RPT-201` |
| 2 | Open one absent student and see the pattern | A timeline with the previous absences and the attendance percentage | `TC-ATT-205` |
| 3 | Review an excuse with an uploaded medical note | The note opens through a short-lived link; the medical detail itself stays closed | `TC-ATT-206` |
| 4 | Approve the excuse | The register updates, the guardian is notified, and the audit entry names you | `TC-ATT-207` |
| 5 | Open the early-warning flag on a student | The reasons are listed; there is no bare risk score anywhere on the screen | `TC-RPT-202` |
| 6 | Open an intervention from the flag and assign yourself with a review date | The plan is created with steps and a review reminder | `TC-WEL-201` |
| 7 | In airplane mode, add a note to a student's timeline | The note is saved locally with an offline badge and syncs on reconnect | `TC-MOB-203` |
| 8 | Switch to Arabic and print the class list | The printed class list (كشف الفصل) is right to left, names render correctly, and the photo column is on the right | `TC-L10N-202` |
| 9 | Try to read the counseling case for one of your students | Refused, and the screen does not confirm whether a case exists | `TC-WEL-202` |
| 10 | Contact a guardian from the student page | The correct guardian is offered per the custody rules, and a restricted guardian is not listed | `TC-SCH-201` |
| 11 | Check the allergy alert on a student before a trip | The alert shows live with substance, severity and action | `TC-WEL-203` |

## Q.4 Registrar

| # | Do this | Expect this | Test case |
|---|---|---|---|
| 1 | Open the registrar home | New inquiries, applications per stage, documents missing, offers expiring, seats left | `TC-ADM-301` |
| 2 | Convert an inquiry to an application | No field is retyped; the inquiry data carries forward | `TC-ADM-302` |
| 3 | Upload a birth certificate for the applicant | The file is scanned, accepted, and the missing-document count drops | `TC-DOC-301` |
| 4 | Make an offer with a 7-day expiry | The guardian receives it by email and push, and the expiry clock is visible | `TC-ADM-303` |
| 5 | Try to enroll the applicant into a full section | Refused with the seat count shown and the waiting list offered | `TC-ADM-304` |
| 6 | Ask a colleague with the override permission to place the student | The override records a reason and the enrolment completes | `TC-ADM-305` |
| 7 | Run a 10,000-row import dry run from the Excel template | A row-and-column error report downloads and nothing is written | `TC-DATA-301` |
| 8 | Fix 12 rows and commit the import | Completes in under 5 minutes with a summary of created and updated records | `TC-DATA-302` |
| 9 | Roll back the import | The prior state is restored exactly and the rollback is audited | `TC-DATA-303` |
| 10 | Switch to Arabic and issue a transfer certificate | The transfer certificate (شهادة نقل) renders right to left, the school name and student name are correct in both scripts, and the QR verification code resolves | `TC-L10N-301` |
| 11 | Open the public verification page from the QR code | The certificate is confirmed as issued, with no personal data beyond what the school chose to show | `TC-DOC-302` |
| 12 | Try to post a payment against the student's invoice | Refused; the registrar does not hold finance permissions | `TC-SEC-301` |
| 13 | Try to export the student list with national identity numbers | The export requires approval and a reason, is watermarked, and notifies the principal | `TC-PRV-301` |

## Q.5 Accountant

| # | Do this | Expect this | Test case |
|---|---|---|---|
| 1 | Open the accountant home | Payments to reconcile, refunds to approve, cheques due, today's overdue ladder | `TC-FIN-401` |
| 2 | Run an invoice batch for one grade level | Progress is visible, and the invoices post with a gapless number series | `TC-FIN-402` |
| 3 | Record a cash payment of 2,750.00 SAR against an invoice | The receipt issues, the balance updates, and the guardian is notified | `TC-FIN-403` |
| 4 | Try to record 3,200.00 SAR against the same 2,750.00 SAR invoice | Refused with the overpayment message, and posting the excess as credit is offered | `TC-FIN-404` |
| 5 | Record a payment in AED against a SAR invoice series | Refused with the currency mismatch message and the invoice currency shown | `TC-FIN-405` |
| 6 | Mark a cheque as bounced | The invoice reopens, the payer and the ladder update, and both parties are notified | `TC-FIN-406` |
| 7 | Request a refund of 1,200.00 SAR | Held for approval; the accountant cannot approve their own refund | `TC-FIN-407` |
| 8 | Have the principal approve the refund | The refund posts and both approvals appear in the audit entry | `TC-FIN-408` |
| 9 | Close the cashier day with a deliberate 25.00 SAR discrepancy | The close is blocked and the documents that differ are listed by name | `TC-FIN-409` |
| 10 | Correct the discrepancy and close | The day balances to 0.00 SAR and the close is recorded | `TC-FIN-410` |
| 11 | Switch to Arabic and print a statement for a guardian | The statement (كشف حساب) renders right to left, currency symbol on the correct side, totals unchanged | `TC-L10N-401` |
| 12 | Try to change a student's grade level to clear a fee dispute | Refused; the accountant holds no student status permission | `TC-SEC-401` |
| 13 | Export the aging report | The export runs, is logged, and matches the on-screen totals exactly | `TC-FIN-411` |

## Q.6 Parent

| # | Do this | Expect this | Test case |
|---|---|---|---|
| 1 | Sign in on a low-cost Android phone and open the calm screen | One card per child, and "nothing needs your attention" as a designed state when that is true | `TC-MOB-501` |
| 2 | Open the older child's card | Today's status, what is due, fees due, requests, unread messages | `TC-RPT-501` |
| 3 | Pay an outstanding 450.00 AED balance in one flow | Payment succeeds, the receipt arrives by email and push, and the balance is zero | `TC-FIN-501` |
| 4 | Submit an early-dismissal request for 13:00 | The request appears with its approver and its expected decision time | `TC-RQS-501` |
| 5 | Receive the approval and open the gate pass | A one-time QR code with its validity window | `TC-ATT-501` |
| 6 | Switch the app to Arabic | Every screen mirrors, the calm state reads «لا يوجد ما يتطلب انتباهك», the school's own terminology is used, and a school message sent in English shows a translation with the original one tap away | `TC-L10N-501` |
| 7 | Set quiet hours from 21:00 to 06:30 and send yourself a normal-urgency test | It arrives after 06:30, not during the night | `TC-NOT-501` |
| 8 | Have the school send an emergency broadcast during quiet hours | It breaks through, and acknowledging it is one tap | `TC-NOT-502` |
| 9 | Turn on airplane mode and open the child's timetable and latest report card | Both open from the local copy with an "as of" time | `TC-MOB-502` |
| 10 | Try to open another family's child by editing the address bar | Refused, and the screen gives no hint that the child exists | `TC-SEC-501` |
| 11 | Open the transparency panel | Which roles read the child's sensitive records and when, the consents on file, and the retention clock per category | `TC-PRV-501` |
| 12 | Withdraw photo consent | The flag changes immediately and a later attempt to publish a photo of the child is blocked | `TC-PRV-502` |

## Q.7 Student

| # | Do this | Expect this | Test case |
|---|---|---|---|
| 1 | Sign in and open "Today" | Timetable, what is due, new feedback, announcements | `TC-MOB-601` |
| 2 | Open an assignment and submit a file | Upload completes, the submission time is recorded, and the state changes | `TC-ACA-601` |
| 3 | Try to resubmit after the teacher has started grading | Refused with a plain message and the grading state shown | `TC-ACA-602` |
| 4 | Read feedback on a graded submission | Mark, comment and rubric, with the scheme the mark came from | `TC-ASM-601` |
| 5 | Open the badges and portfolio page | Badges, house points and selected work across years | `TC-BEH-601` |
| 6 | Switch to Arabic | The timetable (الجدول الدراسي) mirrors, period order reads right to left, and the subject names use the school's terminology | `TC-L10N-601` |
| 7 | Turn on airplane mode and open the timetable and due list | Both open from the local copy; submitting queues with a visible badge | `TC-MOB-602` |
| 8 | Try to message another student | Blocked by default with a plain explanation; the school can turn it on | `TC-COM-601` |
| 9 | Try to open a classmate's grades | Refused, with no hint of the other record | `TC-SEC-601` |
| 10 | Report an inappropriate message in a thread | The report reaches the safeguarding officer and the thread locks pending review | `TC-COM-602` |

## Q.8 Counselor

| # | Do this | Expect this | Test case |
|---|---|---|---|
| 1 | Open the counselor home | Referrals, follow-ups due, caseload, intervention reviews | `TC-WEL-701` |
| 2 | Accept a referral raised by a homeroom teacher | The case opens with the referral reason and the referrer named | `TC-WEL-702` |
| 3 | Record a session note | Saved to the Wellbeing service, and the read is logged from the moment it is saved | `TC-WEL-703` |
| 4 | Sign in as a teacher on another device and search the student | The case does not appear in search, in Student 360 or in any report | `TC-WEL-704` |
| 5 | Open an intervention playbook and assign steps with review dates | Owner, steps, dates and outcome fields are present, and reminders are scheduled | `TC-WEL-705` |
| 6 | Try to open a clinic visit record for the same student | Refused; clinic records belong to the nurse's context | `TC-WEL-706` |
| 7 | Try to export the caseload to a spreadsheet | Refused without approval; the export needs a reason and notifies the principal | `TC-PRV-701` |
| 8 | Switch to Arabic and record a session note (ملاحظة الجلسة) in Arabic | The text saves and redisplays correctly, mixed with an English term, without reordering | `TC-L10N-701` |
| 9 | Close the case with an outcome | The case closes, the flag clears, and the closure is audited | `TC-WEL-707` |
| 10 | Check the guardian transparency panel as the parent | The counselor's reads are listed by role and time, without the note content | `TC-PRV-702` |

## Q.9 HR Officer

| # | Do this | Expect this | Test case |
|---|---|---|---|
| 1 | Open the HR home | Leave to approve, documents expiring, probation ending, vacancies | `TC-HR-801` |
| 2 | Approve a teacher's three-day leave | Approval records, and the substitution step is offered before the approval completes | `TC-HR-802` |
| 3 | Accept the top-ranked substitute suggestion | The cover is assigned, the substitute is notified urgently, and the timetable shows it | `TC-SCD-801` |
| 4 | Try to approve leave that exceeds the balance | Refused with the balance shown and unpaid leave offered | `TC-HR-803` |
| 5 | Open a staff member with an expired teaching licence | The assignment is blocked, HR and the principal are notified, and the document upload is offered | `TC-HR-804` |
| 6 | Try to open the salary section without the salary permission | The section is not present at all, not merely disabled | `TC-SEC-801` |
| 7 | Prepare payroll inputs for the month | Components resolve from the contract, and the accountant is notified when they are ready | `TC-HR-805` |
| 8 | Try to change a payroll input after the period locks | Refused; an adjustment in the next period is offered | `TC-HR-806` |
| 9 | Switch to Arabic and generate a service certificate (شهادة خبرة) | Right-to-left document, correct Hijri and Gregorian dates, correct staff name in both scripts | `TC-L10N-801` |
| 10 | Run the document expiry report | Every expiring document with its owner and an upload action | `TC-HR-807` |
| 11 | Try to open a student's record | Refused; HR holds no student permissions | `TC-SEC-802` |

## Q.10 Platform Administrator

| # | Do this | Expect this | Test case |
|---|---|---|---|
| 1 | Open the platform console | Provisioning in progress, failing services, queue alarms, tickets at risk, trials ending | `TC-PLT-901` |
| 2 | Provision a new tenant for a school in Jordan with the currency JOD | The wizard infers work week, calendar, numerals, tax treatment, terminology and role templates, and shows exactly what it inferred | `TC-PLT-902` |
| 3 | Correct one inferred setting and complete provisioning | The tenant is live with demo data in minutes and the administrator invitation is sent | `TC-PLT-903` |
| 4 | Try to open a student record in that tenant | Refused; platform staff hold no tenant data permissions | `TC-SEC-901` |
| 5 | Request impersonation of the school administrator | Blocked until the tenant records consent | `TC-SEC-902` |
| 6 | After consent, impersonate | A visible banner shows throughout, every action is audited as impersonation, and the session expires on its own | `TC-SEC-903` |
| 7 | Replay a failed message from the failed-message queue | The consumer applies it once; a second replay is an idempotent no-change | `TC-MSG-901` |
| 8 | Suspend a tenant for non-payment | The tenant goes read-only, export stays available, and the school is told what changed | `TC-PLT-904` |
| 9 | Request deletion of a trial tenant | A 30-day cooling-off starts with export available, and a certificate of deletion issues at the end | `TC-PRV-901` |
| 10 | Place a legal hold on a suspended tenant and retry the purge | The purge is blocked, both parties are notified, and the hold itself is logged | `TC-PRV-902` |
| 11 | Switch the console to Arabic | The console (لوحة التحكم) mirrors fully; no operator screen is English-only | `TC-L10N-901` |
| 12 | Open the audit integrity check | The hash chain verifies, and a deliberately tampered test row is reported as broken | `TC-AUD-901` |
| 13 | Try to export a tenant's audit log | Refused without the high-risk permission and an approved reason | `TC-SEC-904` |

---

## Q.11 Acceptance

| Condition | Meaning |
|---|---|
| A script passes | Every step passed, including every refusal step. A refusal step that succeeded instead of refusing is a security defect, not a test failure |
| A script fails | The run stops at the failed step, the correlation id is recorded, and the matching Appendix K code names the owning service |
| Sign-off | One named person per role signs their own script. A sign-off by the build team is not acceptance |
| Regression | Every script runs again before each release on the demo tier, and the Arabic, offline and boundary steps are never skipped for time |
| Traceability | Each test case id here appears in `TRACEABILITY.md` against the requirement it proves, so a requirement with no accepted script is not done |
