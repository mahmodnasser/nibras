# Appendix C. Notification Matrix

> Part of `docs/brief/02-appendices/`. Normative. Read with the master brief and the reference architecture.

Urgency: **U** urgent, breaks quiet hours. **N** normal. **D** digest-eligible. Channels are defaults; users adjust within what the school's policy allows.

**Every row names its trigger**, either an integration event from Appendix E or a scheduled job from the job table at the end of Appendix E. A notification with no trigger is a notification nobody can implement, which is what the earlier version left behind for about a quarter of these rows.

**Rules that apply to the whole table.** Urgent messages ignore quiet hours and marketing suppression but still respect a hard bounce. Digest-eligible messages are batched into the recipient's chosen digest unless the recipient has turned digests off. Every channel has a fallback order and a per-tenant SMS credit check before a paid channel is used. Deduplication collapses identical notifications for the same recipient and subject within a five-minute window; this is the default BR-NOT-004 applies, and urgent messages are never deduplicated.

| Event | Trigger | Recipients | Urgency | Default channels |
|---|---|---|---|---|
| Emergency broadcast | `attendance.emergency.broadcast-started.v1` | Selected audience | U | Push, SMS, in-app, email |
| Emergency acknowledgement missing | `attendance.roll-call.completed.v1` | Incident controller | U | Push, in-app |
| Student marked absent | `attendance.student.absent.v1` | Guardians | U | Push, SMS fallback |
| Student late or early dismissal processed | `attendance.dismissal.processed.v1` | Guardians | N | Push |
| Gate pass issued or used | `attendance.gate-pass.issued.v1`, `attendance.gate-pass.used.v1` | Guardians, requester | U | Push |
| Visitor on the watchlist checked in | `attendance.visitor.checked-in.v1` | Security, principal | U | Push |
| Class attendance not marked by cut-off | `attendance.attendance.not-marked.v1` (job: unmarked class reminder) | Teacher, then coordinator | N | Push, in-app |
| Attendance threshold reached | `attendance.threshold.reached.v1` | Guardians, homeroom teacher, counselor | N | Push, email |
| Excuse approved | `attendance.excuse.approved.v1` | Guardians | N | Push |
| Clinic visit or medication given | `wellbeing.clinic-visit.recorded.v1`, `wellbeing.medication.administered.v1` | Guardians | U | Push, SMS fallback |
| Allergy or medical alert updated | `school.student.profile-updated.v1` | Permitted staff of that student | N | In-app |
| Safeguarding concern raised | `wellbeing.safeguarding.concern-raised.v1` | Safeguarding officer | U | Push, email |
| Anonymous concern reported | `communication.concern.reported-anonymously.v1` | Safeguarding officer | U | Push, email |
| Behavior incident, per policy | `behavior.incident.recorded.v1` | Guardians, homeroom teacher | N | Push |
| Consequence assigned | `behavior.consequence.assigned.v1` | Guardians, student | N | Push |
| Badge, award, or house points | `behavior.points.awarded.v1`, `behavior.badge.awarded.v1` | Student, guardians | D | Push |
| Assignment published | `academics.assignment.published.v1` | Students, guardians of young grades | D | Push |
| Assignment due tomorrow | job: assignment due reminder | Students, guardians of young grades | D | Push |
| Assignment graded with feedback | `academics.submission.graded.v1` | Student, guardians | D | Push |
| Homework load ceiling exceeded | `academics.homework-load.exceeded.v1` | Teacher, coordinator | N | In-app |
| Syllabus coverage behind plan | `academics.syllabus-coverage.behind.v1` | Teacher, coordinator | N | In-app |
| Marks overdue for entry | `assessment.marks.overdue.v1` (job: marks overdue check) | Teacher, then coordinator | N | Push, email |
| Marks awaiting moderation or approval | `assessment.marks.awaiting-approval.v1` | Approver | N | Push, in-app |
| Report card published | `assessment.report-cards.published.v1` | Guardians, student | N | Push, email |
| Grade change decided | `assessment.grade-change.approved.v1` | Approver, requester | N | In-app, push |
| Timetable published or changed | `scheduling.timetable.published.v1`, `scheduling.timetable.changed.v1` | Affected teachers, students, guardians | N | Push |
| Exam timetable published | `scheduling.exam-timetable.published.v1` | Students, guardians | N | Push, email |
| Substitution assigned | `scheduling.substitution.assigned.v1` | Substitute teacher | U | Push |
| Period still uncovered 30 minutes before it starts | `notification.notification.requested.v1` (job: uncovered-period escalation, Scheduling, every 5 minutes on school days) | Principal | N | Push, in-app |
| Room booking approved | `scheduling.room-booking.approved.v1` | Requester | N | In-app |
| Meeting booked | `communication.meeting.booked.v1` | Participants | N | Push, email, calendar invite |
| Meeting changed or reminder | `communication.meeting.changed.v1` | Participants | N | Push, email |
| Announcement published | `communication.announcement.published.v1` | Audience | N, or U when the announcement is marked urgent | Push, in-app, email |
| Acknowledgment overdue | `communication.acknowledgment.overdue.v1` (job: acknowledgment chase) | Person, then owner | N | Push, email |
| New message | `communication.message.sent.v1` | Recipient | N | Push |
| Message reported | `communication.message.reported.v1` | Safeguarding officer | U | Push, email |
| Invoice issued | `finance.invoice.issued.v1` | Payer | N | Email, push |
| Payment due, overdue ladder | `finance.invoice.overdue.v1` (job: reminder ladder) | Payer | N | Push, email, SMS optional |
| Payment received | `finance.payment.received.v1` | Payer | N | Push, email with receipt |
| Payment failed | `finance.payment.failed.v1` | Payer | N | Push, email |
| Refund processed | `finance.refund.processed.v1` | Payer | N | Push, email |
| Cheque bounced | `finance.cheque.bounced.v1` | Accountant, payer | N | In-app, email |
| Account restricted or cleared | `finance.account.restricted.v1`, `finance.account.cleared.v1` | Payer, registrar | N | Email, in-app |
| Cashier day close out of balance | `finance.day.closed.v1` | Accountant, principal | N | Email |
| Request submitted, needs information, decided, completed | `requests.request.submitted.v1`, `requests.request.needs-info.v1`, `requests.request.approved.v1`, `requests.request.rejected.v1`, `requests.request.completed.v1` | Requester, approvers | N | Push, in-app |
| Request SLA at risk or breached | `requests.request.sla-breached.v1` | Assignee, then manager | N | Push, email |
| Task assigned | `requests.task.assigned.v1` | Assignee | N | Push, in-app |
| Application stage change, offer made, offer expiring | `admissions.application.stage-changed.v1`, `admissions.offer.made.v1`, `admissions.offer.expired.v1` | Applicant guardian, registrar | N | Email, push |
| Re-enrollment opened, reminder | `admissions.re-enrollment.declined.v1`, job: re-enrollment window | Guardians | N | Push, email |
| Welcome pack after enrolment | `notification.notification.requested.v1` (`RequestNotification` from Admissions on enrolment) | Guardians | N | Email, push |
| Student document expiring | `school.student-document.expiring.v1` (job: document expiry scan) | Guardians, registrar | D | Email, in-app |
| Staff document expiring | `hr.staff-document.expiring.v1` (job: document expiry scan) | Staff member, HR officer | D | Email, in-app |
| Leave decided | `hr.leave.approved.v1`, `hr.leave.cancelled.v1` | Staff member | N | Push |
| Leave balance low | `hr.leave-balance.low.v1` (job: leave balance check) | Staff member | N | In-app |
| Payroll inputs ready | `hr.payroll.inputs-ready.v1` | Accountant | N | Email |
| Early-warning flag raised | `reporting.early-warning.flag-raised.v1` | Homeroom teacher, counselor | N | In-app, email |
| Intervention review due | `wellbeing.intervention.opened.v1` plus its review date | Owner | N | In-app |
| Data quality issue detected | `reporting.data-quality.issue-detected.v1` | Data steward, school administrator | N | In-app |
| Join request pending | `identity.join-request.submitted.v1` | Approver | N | In-app, push |
| Join request approved | `identity.join-request.approved.v1` | Applicant | N | Email, push |
| Invitation sent | `identity.user.invited.v1` | Invitee, on the contact the invitation names | N | Email or SMS, as the invitation names; the message states the 14-day expiry |
| Invitation reminder at day 7 | `notification.notification.requested.v1` (job: joining timeouts, Identity, day 7 of the 14-day validity) | Invitee who has not accepted | N | Email or SMS, as the invitation names |
| One-time code or password reset link | `notification.notification.requested.v1` (`RequestNotification` from Identity for sign-in, verification and reset; from Admissions and Requests for public-form OTP) | The person verifying | U, never deduplicated or digested | SMS or email, as the person chose; never push |
| Role or permission changed | `identity.role.changed.v1`, `identity.permissions.changed.v1` | User, security administrator | N | In-app, email |
| New device login, password or 2FA changed | `identity.login.new-device.v1` | User | U | Email, push |
| Break-glass access used | `identity.break-glass.used.v1` | Principal, security administrator | U | Push, email |
| Access review due | `identity.access-review.due.v1` | Reviewer | N | Email, in-app |
| Sensitive export performed | `documents.sensitive-export.performed.v1` | Principal, security administrator | N | Email |
| Upload failed a virus scan | `documents.file.scan-failed.v1` | Uploader, IT support | N | In-app, email |
| Import or export finished | `documents.import.completed.v1`, `documents.export.completed.v1` | Initiator; administrator on failure | N | In-app, push |
| Document generated | `documents.document.generated.v1` | Requester | N | In-app, push |
| Certificate revoked | `documents.certificate.revoked.v1` | Holder, registrar | N | Email |
| Notification delivery failing repeatedly | `notification.notification.failed.v1` | Platform operators | U | Email, push |
| Audit integrity check failed | `audit.integrity-check.failed.v1` (job: audit integrity verification) | Platform operators, security administrator | U | Email, push |
| Audit export performed | `notification.notification.requested.v1` (`RequestNotification` from Audit when the export job completes) | Principal | N | Email |
| Webhook endpoint failing | `platform.webhook.delivery-failed.v1` | School administrator, integration owner | N | Email, in-app |
| Plan limit approaching, trial ending, tenant invoice due | `platform.limit.approaching.v1`, `platform.trial.ending.v1`, `platform.invoice.due.v1` (job: plan limit and trial check) | School administrator | N | Email, in-app |
| Signup confirmed, tenant provisioned and welcome pack | `platform.tenant.provisioned.v1`; signup confirmation by `notification.notification.requested.v1` (`RequestNotification` from Platform) | Tenant owner, school administrator | N | Email |
| Tenant deletion cooling-off reminder | `platform.tenant.deletion-requested.v1`, then `notification.notification.requested.v1` (job: deletion cooling-off reminder, Platform, daily through the 30-day cooling-off) | Tenant owner | N | Email, in-app |
| API key expiring, sandbox inactive | `notification.notification.requested.v1` (jobs: API key expiry reminder at 14 and 3 days; sandbox lifecycle at 60 and 83 days without a call, Platform) | Key owner, integration owner | N | Email |
| Support ticket SLA escalated | `notification.notification.requested.v1` (job: support SLA, Platform) | Next support level, service owner | N | Email, in-app |
| Data subject request acknowledged, deadline approaching | `notification.notification.requested.v1` (`RequestNotification` from Platform on receipt; job: subject request deadline) | Requester; data protection officer | N | Email |
| Bus delayed, child boarded or dropped *(T2)* | `operations.transport.vehicle-delayed.v1`, `operations.transport.boarding-recorded.v1` | Guardians | N | Push |
| Library item overdue *(T2)* | `operations.library.loan-overdue.v1` (job: library overdue scan) | Borrower, guardians | D | Push |
| Facility ticket raised *(T2)* | `operations.facility.ticket-raised.v1` | Maintenance, requester | N | In-app |
| Stock low *(T2)* | `operations.inventory.stock-low.v1` | Store keeper | D | In-app |
| Complaint received *(T2)* | `operations.frontdesk.complaint-received.v1` | Front desk, principal | N | In-app, email |
| AI injection attempts blocked *(T2)* | `notification.notification.requested.v1` (job: daily digest of blocked attempts, Ai; never the planted text) | School administrator, security administrator | N | Email, in-app |
| Daily or weekly digest | job: digest builder inside Notification | Guardians, students, staff | D | Push, email |

---

## Channel fallback and credit rules

| Situation | Behaviour |
|---|---|
| Push fails or the device is not registered | Fall back to email. For urgent messages only, fall back to SMS after email |
| SMS credits exhausted | Urgent messages still send and the school is charged an overage warning; normal and digest messages fall back to email and in-app |
| Recipient in quiet hours | U delivers; N and D queue until the window opens |
| Recipient hard-bounced on email | Email suppressed until corrected; a data-quality issue is raised |
| Recipient complained about email | Non-urgent email suppressed permanently; urgent still delivers |
| Device has no Google services | Push arrives in-app while the app is open; urgent falls back to SMS and email |
