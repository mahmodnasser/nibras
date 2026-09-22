# Appendix U. Persona Journeys

> Part of `docs/brief/02-appendices/`. Normative. Read with the master brief and the reference architecture.

Master brief Section 5 names eleven personas and the moment success feels like for each. A persona is only useful when it is followed through time, so every persona here has three journeys: a **day**, a **week** and a **year**. Each moment names the surface it happens on, the workflow from Appendix R that carries it, the notifications from Appendix C that arrive, and whether it must work with no connection.

**The five-minute test** is the one task the persona must be able to finish in under five minutes on their worst day, on the device they actually own. It is the acceptance bar for that persona's home screen and the script in Appendix Q that proves it.

**Surfaces.** `web` the Angular application · `mobile` the Flutter application · `console` the platform console · `email` · `print` a generated document. Offline means the Flutter application queues the action locally and syncs later with a visible state, per master brief Section 18.

---

## U.1 School owner or group director

| Rhythm | Moment, in order | Surface | Workflow | Notifications | Offline |
|---|---|---|---|---|---|
| Day | Opens the group health screen before the first campus bell | mobile | — | Daily digest | Last synced figures, read-only |
| Day | Reads collections against target across three campuses in SAR and AED | mobile | — | — | Read-only |
| Day | Clears approvals above the principal's limit, for example a 40,000 SAR write-off | mobile | `WF-FIN-02` | Request SLA at risk | no |
| Day | Reads the incident and safeguarding count per campus, without any record content | mobile | — | — | no |
| Week | Compares campuses on enrolment vs capacity, attendance, results and staff turnover | web | — | — | no |
| Week | Reviews the re-enrollment campaign conversion | web | `WF-ADM-02` | Re-enrollment reminder | no |
| Week | Signs off the weekly cash position and the aging buckets | web | `WF-FIN-06` | Cashier day close out of balance | no |
| Year | Approves next year's fee structure and scholarship budget | web | `WF-FIN-04` | Announcement published | no |
| Year | Approves the year close and the promotion lists per campus | web | `WF-SCH-02` | Report card published | no |
| Year | Reads the inspection self-evaluation and the evidence folder | web, print | — | Acknowledgment overdue | no |

**Five-minute test:** open one screen and know whether every campus is healthy, then clear the approvals waiting on them.
**Wow moment:** the group health screen with campus comparison and one-tap drill to the records behind every figure, from master brief Section 12 item 1 and Section 12.1 item 27.

## U.2 Principal

| Rhythm | Moment, in order | Surface | Workflow | Notifications | Offline |
|---|---|---|---|---|---|
| Day | Reads the morning brief on the way in at 07:30 | mobile | — | Morning brief digest | Yes, last synced brief with an "as of" time |
| Day | Sees three teachers absent and confirms the suggested cover | mobile | `WF-HR-01` | Substitution assigned | no |
| Day | Clears eleven approvals during assembly | mobile | `WF-RQS-01` | Request submitted, SLA at risk | no, an approval needs the current state and permissions (Appendix M) |
| Day | Reads the unmarked-attendance card at the cut-off and nudges two teachers | mobile | `WF-ATT-01` | Class attendance not marked by cut-off | no |
| Day | Handles one behavior incident and one early dismissal | mobile | `WF-BEH-01`, `WF-ATT-02` | Behavior incident, gate pass issued | no |
| Week | Reviews attendance, punctuality, request SLA and syllabus coverage | web | — | Early-warning flag raised | no |
| Week | Runs the staff meeting from the same dashboard, drilling into every figure | web | — | — | no |
| Week | Reads the sensitive-export report and the access review queue | web | `WF-SEC-01` | Sensitive export performed | no |
| Year | Opens the academic year and publishes the calendar and timetable | web | `WF-SCH-02` | Timetable published | no |
| Year | Approves report cards at each term close | web | `WF-ASM-01` | Marks awaiting approval | no |
| Year | Runs one emergency drill with roll call and reunification | mobile | `WF-OPS-05` | Emergency broadcast | Yes, roll call works with no network |

**Five-minute test:** clear every approval waiting on you, on a phone, before the first bell.
**Wow moment:** emergency mode turning the staff app into roll call, broadcast with acknowledgement and verified reunification in one tap, from master brief Section 12 item 8 and Section 12.1 item 32.

## U.3 Academic coordinator

| Rhythm | Moment, in order | Surface | Workflow | Notifications | Offline |
|---|---|---|---|---|---|
| Day | Opens the lesson plans awaiting review | web | `WF-ACA-01` | Lesson plan submitted | no |
| Day | Checks late mark entry and nudges two teachers | web | `WF-ASM-01` | Marks overdue for entry | no |
| Day | Reviews a homework-load warning for grade 8 Tuesday | web | — | Homework load ceiling exceeded | no |
| Day | Observes one lesson and files the observation on a tablet | mobile | — | — | Yes, the observation form queues |
| Week | Moderates one subject's marks and releases them for approval | web | `WF-ASM-01` | Marks awaiting moderation | no |
| Week | Reads syllabus coverage by subject and the grade distributions | web | — | Syllabus coverage behind plan | no |
| Week | Rebalances two teachers' loads with the workload view | web | `WF-HR-01` | Timetable changed | no |
| Year | Builds the assessment calendar and the exam timetable | web | `WF-ASM-03` | Exam timetable published | no |
| Year | Runs the result analysis and the moderation report at each term | web | `WF-ASM-01` | Report card published | no |
| Year | Builds next year's sections with the balanced class formation | web | `WF-SCH-02` | — | no |

**Five-minute test:** see which subjects are behind and which teachers are late on marks, and act on both from the same screen.
**Wow moment:** balanced class formation that builds next year's sections against gender, ability, behavior, needs and keep-together rules, then lets you drag one child, from master brief Section 12 item 20.

## U.4 Teacher

| Rhythm | Moment, in order | Surface | Workflow | Notifications | Offline |
|---|---|---|---|---|---|
| Day | Opens the five-minute home and reads the next class and the cover alert | mobile | — | Substitution assigned | Yes |
| Day | Marks attendance for first period in under a minute | mobile | `WF-ATT-01` | — | Yes, this is the primary offline case |
| Day | Publishes today's assignment with a due date | mobile | `WF-ACA-01` | Assignment published | Yes, queues |
| Day | Grades yesterday's submissions in the fast grid at break | web, mobile | `WF-ACA-01` | Assignment graded | Yes, grade entry queues |
| Day | Replies to two parent messages within the school's messaging hours | mobile | — | New message | Yes, queues |
| Week | Enters marks for one assessment and submits them for moderation | web | `WF-ASM-01` | Marks overdue for entry | Yes, entry queues |
| Week | Reviews AI-drafted comments and edits every one before submitting | web | `WF-ASM-01` | — | no |
| Week | Accepts one cover class and declines one, with the reason recorded | mobile | `WF-HR-01` | Substitution assigned | no |
| Year | Plans the term's curriculum coverage and submits the plan | web | `WF-ACA-01` | Lesson plan submitted | no |
| Year | Writes report card comments for 90 students across three terms | web | `WF-ASM-01` | Marks awaiting approval | no |
| Year | Attends parent conference day with the booked slots on a phone | mobile | — | Meeting booked, meeting reminder | Yes, the day's schedule |

**Five-minute test:** mark a class, publish one assignment and answer one parent, from a phone, between two lessons.
**Wow moment:** exception-only attendance, where the gate scan, the bus boarding and approved leave pre-fill the register so the class is confirmed in twenty seconds, from master brief Section 12 item 3 and Section 12.1 item 26.

## U.5 Homeroom teacher

| Rhythm | Moment, in order | Surface | Workflow | Notifications | Offline |
|---|---|---|---|---|---|
| Day | Opens the homeroom card: absent today, excuses, flags, birthdays | mobile | `WF-ATT-01` | Student marked absent | Yes, read |
| Day | Reviews and approves two excuses with uploaded notes | mobile | `WF-ATT-01` | Excuse approved | no |
| Day | Adds a timeline note about a student who seemed withdrawn | mobile | — | — | Yes, queues |
| Day | Reads an early-warning flag and its stated reasons | mobile | — | Early-warning flag raised | no |
| Week | Opens an intervention playbook for one student, with owner and review date | web | `WF-WEL-05` | Intervention review due | no |
| Week | Reads the class overview: attendance, behavior and performance per student | web | — | Attendance threshold reached | no |
| Week | Calls two guardians, using the contact order the custody rules allow | mobile | — | — | no |
| Year | Hands over the homeroom file at year end with every open intervention | web | `WF-SCH-02` | — | no |
| Year | Prepares and attends two parent conference rounds | web, mobile | — | Meeting booked | Yes, the day's schedule |
| Year | Writes the homeroom comment on each report card | web | `WF-ASM-01` | Report card published | no |

**Five-minute test:** know which child in your homeroom changed this week, and why, without opening five screens.
**Wow moment:** the early-warning flag that always shows why it was raised and opens a real intervention with an owner and a follow-up date, from master brief Section 12 item 4 and Section 12.1 item 30.

## U.6 Registrar

| Rhythm | Moment, in order | Surface | Workflow | Notifications | Offline |
|---|---|---|---|---|---|
| Day | Opens new inquiries and converts three to applications | web | `WF-ADM-01` | Inquiry created | no |
| Day | Chases four applications with missing documents | web | `WF-ADM-01` | Student document expiring | no |
| Day | Issues two offers with their expiry clocks | web | `WF-ADM-01` | Offer made | no |
| Day | Enrols one accepted student and issues the identity card | web, print | `WF-ADM-01` | Application stage change | no |
| Week | Reviews the funnel: time in stage, conversion, seats left per grade | web | — | Offer expiring | no |
| Week | Processes one transfer out with clearance across finance and library | web | `WF-SCH-01` | Account restricted or cleared | no |
| Week | Runs the data quality center and fixes missing guardian contacts | web | — | Data quality issue detected | no |
| Year | Runs the legacy import at go-live with dry run, error report and rollback | web | `WF-DATA-01` | Import finished or failed | no |
| Year | Opens and closes the re-enrollment campaign | web | `WF-ADM-02` | Re-enrollment opened | no |
| Year | Runs promotion and the year rollover after results are published | web | `WF-SCH-02` | Report card published | no |

**Five-minute test:** move one applicant from inquiry to enrolled without retyping a single field.
**Wow moment:** go-live in a day, with downloadable templates, a dry-run preview, a row-level error report and a rollback that actually restores, from master brief Section 12 item 9.

## U.7 Accountant

| Rhythm | Moment, in order | Surface | Workflow | Notifications | Offline |
|---|---|---|---|---|---|
| Day | Opens the finance home: payments to reconcile, cheques due, today's ladder | web | `WF-FIN-01` | Payment received | no |
| Day | Records counter payments and issues receipts, in SAR and in AED | web | `WF-FIN-01` | Payment received | no |
| Day | Marks one cheque bounced and reopens the invoice | web | `WF-FIN-03` | Cheque bounced | no |
| Day | Closes the cashier day, balanced to 0.00 SAR | web | `WF-FIN-06` | Cashier day close out of balance | no |
| Week | Runs the invoice batch for the term and watches the progress | web | `WF-FIN-01` | Invoice issued | no |
| Week | Sends the reminder ladder and reviews the aging buckets | web | `WF-FIN-01` | Payment due, overdue ladder | no |
| Week | Submits two refunds and one write-off for four-eyes approval | web | `WF-FIN-02` | Refund processed | no |
| Year | Builds the fee structure and assigns plans for the new year | web | `WF-FIN-01` | Announcement published | no |
| Year | Awards scholarships and applies sponsor payer changes | web | `WF-FIN-04`, `WF-FIN-05` | Announcement published | no |
| Year | Produces the annual collection, discount and tax reports | web, print | — | — | no |

**Five-minute test:** run a batch of invoices, send the reminders, and close the day balanced, without a spreadsheet.
**Wow moment:** the day close that refuses to balance silently and names the exact documents that differ, from master brief Section 12 item 13 and Section 12.1 item 27.

## U.8 Parent or guardian

| Rhythm | Moment, in order | Surface | Workflow | Notifications | Offline |
|---|---|---|---|---|---|
| Day | Opens the calm screen: one card per child, often saying nothing needs attention | mobile | — | Daily digest | Yes, last synced |
| Day | Receives an absence alert within a minute and replies with an excuse | mobile | `WF-ATT-01` | Student marked absent | Excuse queues |
| Day | Reads the clinic note that the child was seen and sent back to class | mobile | `WF-WEL-02` | Clinic visit or medication given | no |
| Day | Pays a 450.00 AED balance in one tap | mobile | `WF-FIN-01` | Invoice issued, payment received | no |
| Week | Reads the weekly digest per child instead of a stream of alerts | mobile, email | — | Weekly digest | Yes, read |
| Week | Requests an early dismissal and collects the gate pass | mobile | `WF-ATT-02` | Gate pass issued | Pass shows offline |
| Week | Books a conference slot with the homeroom teacher | mobile | — | Meeting booked | no |
| Year | Confirms re-enrollment and settles the deposit | mobile | `WF-ADM-02` | Re-enrollment opened | no |
| Year | Acknowledges the school handbook and the media consent | mobile | — | Acknowledgment overdue | no |
| Year | Downloads the report card and the year's portfolio | mobile, print | `WF-ASM-01` | Report card published | Yes, once downloaded |

**Five-minute test:** know how each child is doing today and clear everything the school needs from you, in one sitting, on a cheap phone.
**Wow moment:** the transparency panel that shows which roles read the child's sensitive records and when, next to the consents and the retention clock, from master brief Section 12 item 14 and Section 12.1 item 31.

## U.9 Student

| Rhythm | Moment, in order | Surface | Workflow | Notifications | Offline |
|---|---|---|---|---|---|
| Day | Opens today: timetable, what is due, new feedback | mobile | — | Assignment due tomorrow | Yes |
| Day | Submits an assignment from the phone | mobile | `WF-ACA-01` | — | Queues, sends on reconnect |
| Day | Reads feedback with the rubric and the scheme behind the mark | mobile | `WF-ACA-01` | Assignment graded | Yes, read |
| Day | Sees house points awarded in the afternoon | mobile | `WF-BEH-01` | Badge, award, or house points | Yes, read |
| Week | Checks the grades trend and the attendance percentage | mobile | — | — | Yes, read |
| Week | Sits an online quiz in class | web | `WF-ACA-01` | — | no |
| Week | Books a counseling appointment through a private request | mobile | `WF-RQS-01` | Request submitted | no |
| Year | Sits term exams with the accommodations on the plan applied | web, print | `WF-WEL-01` | Exam timetable published | no |
| Year | Receives the report card and the badges of the year | mobile | `WF-ASM-01` | Report card published | Yes, once downloaded |
| Year | Exports the portfolio when leaving the school | web | `WF-SCH-01` | — | no |

**Five-minute test:** know what is due, submit it, and read the feedback, without asking anyone.
**Wow moment:** the portfolio and recognition that follows them across years and leaves with them when they go, from master brief Section 12 item 18 and Section 12.1 item 36.

## U.10 Nurse and counselor

| Rhythm | Moment, in order | Surface | Workflow | Notifications | Offline |
|---|---|---|---|---|---|
| Day | Opens the care home: visits today, medication schedule, follow-ups due | mobile | `WF-WEL-02` | Intervention review due | no, care data never reaches the device (Appendix M) |
| Day | Records a clinic visit and notifies the guardian in the same action | mobile | `WF-WEL-02` | Clinic visit recorded | no, a clinic visit is never recorded offline (Appendix M) |
| Day | Administers an authorized medication inside its window | mobile | `WF-WEL-03` | Medication given | no, signed at the moment it happens while connected (Appendix M) |
| Day | Checks an allergy alert before a trip, read live and never from a cache | mobile | — | Allergy or medical alert updated | no, this one always needs the network |
| Day | Triages one referral from a homeroom teacher into a case | web | `WF-WEL-04` | Referral created | no |
| Week | Runs counseling sessions and records notes in the isolated store | web | `WF-WEL-04` | — | no |
| Week | Escalates one safeguarding concern with the chronology attached | web | `WF-WEL-04` | Safeguarding concern raised | no |
| Week | Reviews accommodation plans before the week's assessments | web | `WF-WEL-01` | Intervention review due | no |
| Year | Runs the annual medical data collection and consent refresh | web | — | Acknowledgment overdue | no |
| Year | Reviews every open case and plan before the year closes | web | `WF-WEL-04` | — | no |
| Year | Hands over active plans and concerns under the retention rules | web | `WF-PRV-01` | — | no |

**Five-minute test:** record a visit, tell the right guardian, and be certain nobody else can read the note.
**Wow moment:** care records that live in their own isolated store, where a principal needs break-glass with a reason and the family can see that it happened, from master brief Section 12 item 14 and Section 12.1 item 31.

## U.11 Platform operator

| Rhythm | Moment, in order | Surface | Workflow | Notifications | Offline |
|---|---|---|---|---|---|
| Day | Opens the console: provisioning, failing services, queue alarms, tickets at risk | console | — | Failed messages above threshold | no |
| Day | Provisions a school in Jordan in JOD and watches the wizard's inferred settings | console | `WF-PLT-01` | Tenant provisioned | no |
| Day | Replays a batch of dead letters once their transient cause clears | console | `WF-INF-02` | Failed messages above threshold | no |
| Day | Answers two tickets, one with consented impersonation and a visible banner | console | `WF-SEC-03` | Join request pending | no |
| Week | Reviews tenant health scores and opens tickets before the schools call | console | — | Plan limit approaching | no |
| Week | Ships a canary release and publishes the plain-language "what changed" per tenant | console | `WF-INF-02` | Announcement published | no |
| Week | Reviews the access review campaign for platform staff | console | `WF-SEC-01` | Role or permission changed | no |
| Year | Runs the restore and failover drill and records the result | console | `WF-INF-03` | Service degraded | no |
| Year | Processes trial conversions, plan changes and two tenant deletions with certificates | console | `WF-PLT-02`, `WF-PLT-03` | Trial ending, invoice due | no |
| Year | Runs the annual penetration test remediation and the audit integrity report | console | — | Sensitive export performed | no |

**Five-minute test:** provision a school and hand its administrator a working tenant with demo data, without touching a database.
**Wow moment:** self-healing operations, where dead letters replay themselves, the tenant health score raises the ticket before the school notices, and every release explains itself per tenant, from master brief Section 12.1 item 40.

---

## U.12 How these journeys are used

| Use | Rule |
|---|---|
| Design | A screen that no journey moment reaches is not in the first release. A moment with no screen is a gap, not a nice-to-have |
| Offline scope | The Flutter application implements exactly the moments marked offline here. Anything else may fail cleanly when there is no network |
| Notifications | Every notification named here exists in Appendix C with its trigger. A journey that expects a message Appendix C does not send is a defect in one of the two |
| Acceptance | The five-minute test of each persona is the acceptance bar for that role's home screen and appears as a timed step in Appendix Q |
| Demo | The wow moment of each persona is what the sixty-second demo for that role shows. A wow moment that cannot be demonstrated on the Appendix H data is not a wow moment |
| Change | Adding a persona moment means checking the workflow in Appendix R, the permissions in Appendix B, and the notification in Appendix C in the same session |
