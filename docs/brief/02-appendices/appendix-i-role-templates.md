# Appendix I. Default Role Templates

> Part of `docs/brief/02-appendices/`. Normative. Read with the master brief and the reference architecture.

Master brief Section 9 names the roles and the authorization model; Section 10.5 describes the interface that edits them. This appendix is the **complete matrix that Section 26 lists as a Phase 0 deliverable**: the twenty-three built-in role templates, what each one holds, what each one must never hold, where it lands after sign-in, and what it can do on a phone.

---

## I.1 How to read this appendix

Permission strings follow Appendix B and the namespace corrections in Appendix L: student, guardian, custody and medical resources live under `school.`, safety lives under `attendance.safety.`, and the old administration namespace is split across `platform.`, `audit.` and the service that owns each concern. Anything written in the old form is a defect.

| Term | Meaning |
|---|---|
| **Template** | A named bundle of permission groups plus a default data scope, seeded per tenant at provisioning |
| **Data scope** | One of: all tenant, campus, stage, department, own sections and groups, own homeroom, own children, self (Appendix B) |
| **Risk** | `normal`, `elevated`, `high` as declared in Appendix B; a high grant needs four-eyes approval |
| **Isolation level S** | Wellbeing records, which no template inherits by default and which Appendix J classifies separately |
| **Five-minute surface** | The mobile home the role sees; master brief Section 12.1 item 34 is the design rule behind it |

Smart defaults (master brief Section 12.1 item 29) pick which of these templates a new tenant receives from country and school type. The wizard shows what it inferred, and the school corrects it before go-live.

---

## I.2 Role sheets

### Purpose, scope, landing, mobile

| Role | Purpose in one line | Default data scope | Default dashboard | Mobile capabilities |
|---|---|---|---|---|
| **Platform Administrator** | Keep every tenant provisioned, healthy and inside its plan | All tenants (platform console only) | Platform operator "Today" (Appendix D) | Read-only health, ticket triage, broadcast to operators; no tenant data on mobile |
| **School Owner / Group Director** | Know whether the school is healthy across campuses | All tenant, all campuses | Owner "Today" with campus comparison | Full read, approvals above the principal's limit, no data entry |
| **School Administrator / Principal** | Run the day and unblock people | All tenant, one campus by default | Principal "Today" | Approvals, morning brief, emergency mode, cover decisions, read Student 360 |
| **Vice Principal** | Cover the principal and own discipline and staffing of the day | Campus | Principal "Today", cover and incident cards first | Same as principal except finance approvals |
| **Academic Coordinator** | Keep teaching and assessment on track | Stage or whole school academics | Academic coordinator "Today" | Lesson-plan review, moderation queue, late-entry chase |
| **Head of Department** | Own one department's curriculum, marks and teachers | Department | Academic coordinator cards filtered to department | Moderation, workload view, observation notes |
| **Registrar** | Accurate records, smooth transfers, clean files | All tenant student records | Registrar "Today" | Document checklist, offer follow-up, read-only records |
| **Admissions Officer** | Move inquiries to enrolled without retyping | Admissions pipeline, campus | Registrar "Today", funnel cards first | Inquiry capture, tour booking, applicant notes |
| **Teacher** | Teach, not administrate | Own sections and groups | Teacher "Today" | Attendance, grade entry, assignment publishing, messages, cover alert — all offline-capable |
| **Homeroom Teacher** | Know every child in the homeroom | Own sections plus own homeroom | Homeroom teacher "Today" | Teacher set plus homeroom timeline, excuse review, guardian contact |
| **Student** | Know what is due and how I am doing | Self | Student "Today" | Timetable, due list, submissions, feedback, badges; offline read |
| **Parent / Guardian** | Know how my child is doing and what I must do | Own children | Parent calm screen (Section 12.1 item 35) | Digest, pay, request, excuse, meeting booking, gate pass |
| **Accountant** | Collect, reconcile and close the day balanced | All tenant finance, one series set | Accountant "Today" | Payment capture, receipt issue, day-close read; no refund approval on mobile |
| **HR Officer** | Staff records, leave, payroll inputs, appraisals | All staff, one campus by default | HR officer "Today" | Leave decisions, document expiry chase, headcount read |
| **Counselor** | Care with confidentiality | Own caseload | Counselor "Today" | Referral triage, follow-up log, intervention steps |
| **Nurse** | Treat, record and tell the right guardian fast | All students, clinic context only | Nurse "Today" | Clinic visit entry, medication round, allergy lookup, offline queue |
| **Special-Needs Coordinator** | Plans that hold up in class and in exams | Students with an active plan | Counselor "Today", plan review cards first | Plan read, accommodation check at exam sitting |
| **Safeguarding Officer** | See the pattern nobody else can see, and act | All tenant, safeguarding context only | Safeguarding queue with concern age and escalation clock | Concern intake, escalation, reported-message review |
| **Librarian** | Circulation that never blocks a class | Library catalogue and loans | Library circulation and overdue cards | Issue, return, reserve, stocktake scan — offline-capable |
| **Transport Coordinator** | Every child on the right bus, every guardian informed | Routes, stops, subscribers | Route roster, no-show and delay cards | Boarding roster, delay broadcast, subscription change |
| **Receptionist / Security** | Control the gate and the front desk | Visitors, gate passes, front desk | Visitors on site, expected pickups, passes to verify | Gate pass verification, visitor check-in, pickup verification — offline-capable |
| **Store Keeper** | Stock that matches the shelf | Inventory and assets | Stock below reorder, requisitions to receive | Receive, issue, count |
| **IT Support** | Accounts and devices work, nothing leaks | Tenant technical objects only | Failed jobs, failed messages, device and account queue | Password reset assist, device enrolment; no student data |

### What each role holds and what it must never hold

| Role | Holds (permission groups, I.3) | Must **not** hold |
|---|---|---|
| **Platform Administrator** | G01, G24, plus `platform.support.impersonate` under consent | Any tenant academic, finance, behavior or wellbeing permission; `wellbeing.*`; `assessment.marks.enter`; silent impersonation |
| **School Owner / Group Director** | G03 (view), G14 (view), G15 (approve only above the principal limit), G23, G24 (view) | `assessment.marks.enter`, `attendance.student-attendance.mark`, `wellbeing.*`, `identity.roles.grant-high-risk` |
| **School Administrator / Principal** | G02, G03, G04, G06, G07, G09, G10, G12, G13, G16, G17, G18, G19, G21 (view), G22 (view), G23, G24 (view) | `wellbeing.safeguarding.view` by default, `hr.payroll.view-salary`, `platform.tenants.provision`, `audit.entries.export` |
| **Vice Principal** | Principal set minus G14, G15; adds `scheduling.substitutions.assign` | `finance.*` beyond view, `hr.payroll.view-salary`, `wellbeing.*` |
| **Academic Coordinator** | G07, G08 (view), G09, G10 (view), G12 (view), G23 | `assessment.marks.unlock`, `finance.*`, `school.custody.view`, `identity.roles.assign-role` |
| **Head of Department** | G07 (department), G08 (view), G09 (moderate only), G23 (department) | `assessment.report-cards.publish`, `attendance.thresholds.edit`, `hr.*` |
| **Registrar** | G04, G05 (`view-sensitive` only, logged), G06, G18, G23 (records) | `finance.payments.record`, `assessment.marks.enter`, `wellbeing.*`, `documents.exports.export-sensitive` without approval |
| **Admissions Officer** | G06, G04 (view of applicants only) | `school.students.change-status` for enrolled students, `finance.discounts.approve-above-limit`, G05 |
| **Teacher** | G07 (own), G08, G11, G16 (own classes), G17 (submit), G19 (record) | `assessment.marks.approve`, `assessment.marks.unlock`, `attendance.student-attendance.edit-after-lock`, `school.medical-summary.view` beyond the allergy alert, `finance.*` |
| **Homeroom Teacher** | Teacher set plus G12 (own homeroom), G04 (view, own homeroom), G23 (own homeroom) | `assessment.marks.approve`, `wellbeing.counseling-cases.view`, `school.custody.view` |
| **Student** | G07 (self), G16 (self, policy-bounded), G23 (self) | Any permission over another student; `communication.messages.create` to other students when the tenant keeps the default off |
| **Parent / Guardian** | G04 (own children, view), G14 (own invoices), G16 (own threads), G17 (submit), G23 (own children) | Any permission over another family's child; `behavior.incidents.view` for other students; staff-side anything |
| **Accountant** | G14, G15 (`post`, `close-day`; `refund` and `write-off` only with four-eyes), G18 (finance templates), G23 (finance) | `assessment.*`, `school.students.change-status`, `wellbeing.*`, `hr.payroll.view-salary` |
| **HR Officer** | G21, G18 (staff documents), G23 (staff) | `school.students.*`, `assessment.*`, `finance.invoices.post`, `wellbeing.*` |
| **Counselor** | G20 (counseling and interventions), G19 (view-restricted), G04 (view), G23 (caseload) | `wellbeing.clinic-visits.edit`, `wellbeing.break-glass.use`, `finance.*` |
| **Nurse** | G20 (clinic, medications, allergy), G04 (view) | `wellbeing.counseling-cases.view`, `wellbeing.safeguarding.view`, `assessment.*` |
| **Special-Needs Coordinator** | G20 (education-plans, interventions), G07 (view), G09 (accommodation flag only) | `wellbeing.clinic-visits.view`, `wellbeing.safeguarding.view`, `assessment.marks.enter` |
| **Safeguarding Officer** | G20 (safeguarding and `wellbeing.break-glass.use`, high and logged), `communication.messages.oversee-messages` (high, logged) | `assessment.*`, `finance.*`, `hr.*`, `identity.roles.grant-high-risk` |
| **Librarian** | G22 (library), G04 (view name and section only) | `school.guardians.view`, `finance.payments.record` beyond library fines, `assessment.*` |
| **Transport Coordinator** | G22 (transport), G04 (view of subscribers), G13 (boarding events) | `school.medical-summary.view`, `attendance.student-attendance.mark`, `finance.refunds.approve` |
| **Receptionist / Security** | G13, G04 (photo, name, section, authorized pickups) | `school.custody.view` text, `behavior.incidents.view`, `assessment.*`, `finance.*` |
| **Store Keeper** | G22 (inventory, assets) | `school.students.*`, `finance.payments.record`, `hr.*` |
| **IT Support** | G02 (`reset-password`, `force-signout`, device objects), `platform.jobs.retry`, `platform.failed-messages.replay` | `identity.roles.grant-high-risk`, `school.students.view`, `assessment.*`, `finance.*`, `wellbeing.*`, `audit.entries.export` |

---

## I.3 Permission groups

| Code | Group | Representative permissions | Highest risk inside |
|---|---|---|---|
| G01 | Platform administration | `platform.tenants.provision`, `platform.tenants.suspend`, `platform.plans.edit`, `platform.feature-flags.edit` | high |
| G02 | Identity and access | `identity.users.invite`, `identity.roles.assign-role`, `identity.sessions.revoke`, `identity.users.reset-password` | high (`identity.roles.grant-high-risk`) |
| G03 | School configuration | `school.profile.edit`, `school.campuses.create`, `school.academic-years.close-year`, `school.numbering.edit` | high (`close-year`, `reopen-year`) |
| G04 | Student records | `school.students.view`, `school.students.edit`, `school.guardians.edit`, `school.students.print-id-cards` | elevated |
| G05 | Sensitive student fields | `school.students.view-sensitive`, `school.custody.view`, `school.medical-summary.view` | elevated, always logged |
| G06 | Admissions | `admissions.applications.decide`, `admissions.offers.make`, `admissions.enrollment.enroll`, `admissions.capacity.override-capacity` | elevated |
| G07 | Academics and coursework | `academics.teaching-assignments.edit`, `academics.lesson-plans.review`, `academics.assignments.publish`, `academics.question-bank.edit` | normal |
| G08 | Mark entry | `assessment.marks.enter`, `assessment.marks.view`, `assessment.structures.view` | normal |
| G09 | Mark approval and publishing | `assessment.marks.moderate`, `assessment.marks.approve`, `assessment.report-cards.publish`, `assessment.marks.lock` | high (`unlock`, `change-after-lock`) |
| G10 | Scheduling and cover | `scheduling.timetable.generate`, `scheduling.timetable.publish`, `scheduling.substitutions.assign`, `scheduling.room-bookings.approve` | elevated (`override-conflict`) |
| G11 | Attendance marking | `attendance.student-attendance.mark`, `attendance.student-attendance.view` | normal |
| G12 | Attendance oversight | `attendance.excuses.approve`, `attendance.thresholds.edit`, `attendance.student-attendance.edit-after-lock` | elevated |
| G13 | Safety and dismissal | `attendance.safety.gate-passes.issue`, `attendance.safety.gate-passes.verify`, `attendance.safety.visitors.check-in`, `attendance.safety.emergency.broadcast` | high (`broadcast`) |
| G14 | Finance operations | `finance.invoices.create`, `finance.payments.record`, `finance.cashier.close-day`, `finance.reports.view` | normal to elevated |
| G15 | Finance approvals | `finance.refunds.approve`, `finance.write-offs.approve`, `finance.discounts.approve-above-limit` | high |
| G16 | Communication | `communication.announcements.publish`, `communication.messages.create`, `communication.surveys.publish` | high (`oversee-messages`) |
| G17 | Requests and approvals | `requests.requests.create`, `requests.requests.approve`, `requests.types.design`, `requests.requests.reassign` | elevated (`override`) |
| G18 | Documents and transfers | `documents.certificates.generate`, `documents.imports.commit`, `documents.exports.create`, `documents.templates.edit` | high (`export-sensitive`) |
| G19 | Behavior and recognition | `behavior.incidents.create`, `behavior.points.award`, `behavior.badges.award`, `behavior.incidents.view-restricted` | elevated |
| G20 | Wellbeing (isolation level S) | `wellbeing.clinic-visits.create`, `wellbeing.counseling-cases.view`, `wellbeing.safeguarding.create`, `wellbeing.education-plans.edit` | high (`break-glass`) |
| G21 | Human resources | `hr.staff-files.edit`, `hr.leave.approve`, `hr.appraisals.edit`, `hr.vacancies.edit` | high (`view-salary`) |
| G22 | Operations modules | `operations.library.issue`, `operations.transport.edit`, `operations.inventory.receive`, `operations.frontdesk.edit` | normal |
| G23 | Reporting and analytics | `reporting.dashboards.view`, `reporting.reports.view`, `reporting.reports.edit`, `reporting.reports.schedule` | normal |
| G24 | Audit and compliance | `audit.entries.view`, `audit.login-history.view`, `audit.integrity.verify` | high (`audit.entries.export`) |

---

## I.4 Roles by permission group

`F` full · `V` view only · `A` approve or decide only · `S` scoped to the role's own children, classes or caseload · `—` not held · `4` held but every grant needs four-eyes.

| Role | G01 | G02 | G03 | G04 | G05 | G06 | G07 | G08 | G09 | G10 | G11 | G12 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Platform Administrator | F | V | — | — | — | — | — | — | — | — | — | — |
| School Owner | — | V | V | V | — | V | V | V | — | V | — | V |
| Principal | — | F | F | F | 4 | F | F | V | F | F | — | F |
| Vice Principal | — | V | V | F | 4 | V | F | V | A | F | — | F |
| Academic Coordinator | — | — | V | V | — | V | F | V | F | V | — | V |
| Head of Department | — | — | — | V | — | — | S | V | A | V | — | — |
| Registrar | — | — | V | F | 4 | F | V | — | — | V | — | V |
| Admissions Officer | — | — | — | S | — | F | — | — | — | — | — | — |
| Teacher | — | — | — | S | — | — | S | S | — | V | S | — |
| Homeroom Teacher | — | — | — | S | — | — | S | S | — | V | S | S |
| Student | — | — | — | S | — | — | S | — | — | S | — | — |
| Parent / Guardian | — | — | — | S | — | S | S | — | — | S | — | — |
| Accountant | — | — | — | V | — | V | — | — | — | — | — | — |
| HR Officer | — | V | — | — | — | — | — | — | — | V | — | — |
| Counselor | — | — | — | V | 4 | — | V | — | — | — | — | V |
| Nurse | — | — | — | V | 4 | — | — | — | — | — | — | V |
| Special-Needs Coordinator | — | — | — | V | 4 | — | V | — | A | — | — | V |
| Safeguarding Officer | — | — | — | V | 4 | — | — | — | — | — | — | V |
| Librarian | — | — | — | V | — | — | — | — | — | — | — | — |
| Transport Coordinator | — | — | — | S | — | — | — | — | — | — | — | V |
| Receptionist / Security | — | — | — | V | — | — | — | — | — | — | — | V |
| Store Keeper | — | — | — | — | — | — | — | — | — | — | — | — |
| IT Support | — | S | — | — | — | — | — | — | — | — | — | — |

| Role | G13 | G14 | G15 | G16 | G17 | G18 | G19 | G20 | G21 | G22 | G23 | G24 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Platform Administrator | — | — | — | S | F | V | — | — | — | — | V | F |
| School Owner | V | V | 4 | V | A | V | V | — | V | V | F | V |
| Principal | F | V | — | F | F | F | F | — | V | V | F | V |
| Vice Principal | F | — | — | F | F | V | F | — | V | V | F | V |
| Academic Coordinator | — | — | — | S | A | V | V | — | — | — | F | — |
| Head of Department | — | — | — | S | A | — | V | — | — | — | S | — |
| Registrar | V | V | — | S | F | F | V | — | — | — | S | — |
| Admissions Officer | — | V | — | S | S | V | — | — | — | — | S | — |
| Teacher | — | — | — | S | S | — | S | — | — | — | S | — |
| Homeroom Teacher | V | — | — | S | S | — | S | — | — | — | S | — |
| Student | — | S | — | S | S | — | — | — | — | S | S | — |
| Parent / Guardian | S | S | — | S | S | V | S | — | — | S | S | — |
| Accountant | — | F | 4 | S | A | F | — | — | — | — | S | — |
| HR Officer | — | — | — | S | A | F | — | — | F | — | S | — |
| Counselor | — | — | — | S | S | — | V | F | — | — | S | — |
| Nurse | V | — | — | S | S | — | — | S | — | — | S | — |
| Special-Needs Coordinator | — | — | — | S | S | — | V | S | — | — | S | — |
| Safeguarding Officer | V | — | — | 4 | F | — | F | 4 | — | — | S | V |
| Librarian | — | S | — | S | S | — | — | S | — | F | S | — |
| Transport Coordinator | F | — | — | S | S | — | — | — | — | F | S | — |
| Receptionist / Security | F | — | — | S | S | — | — | — | — | S | — | — |
| Store Keeper | — | — | — | — | S | — | — | — | — | F | S | — |
| IT Support | — | — | — | — | S | — | — | — | — | — | — | — |

---

## I.5 Rules that govern these templates

1. **System templates are locked and clonable.** The twenty-three templates above cannot be edited in place. A school that wants "Principal without finance oversight" clones the template, renames it, and edits the clone. The clone records its parent and the platform release it was cloned from, so an upgrade can tell the school which parent permissions changed under it.
2. **A locked template is still upgradable.** When a release adds a permission to a service, the locked template gains it only if the release notes say so, and the tenant sees the change in the in-app changelog before it applies.
3. **High-risk grants need four-eyes.** Every cell marked `4`, and every permission Appendix B marks `high`, is granted through `WF-IDN-05` in Appendix R: one person proposes, a second with `identity.roles.grant-high-risk` approves, both are named in the audit entry, and the grant may carry an expiry. A self-grant is refused even when the actor holds both permissions.
4. **Delegation is time-boxed, never permanent.** `WF-IDN-04` lends a template to a covering person with a start and end instant. The delegation ends on its own; there is no "remember to remove it" step.
5. **A role change takes effect within seconds.** Each user carries a permission version. Identity publishes `identity.permissions.changed.v1` on every grant, revocation, delegation start and delegation end; Gateway and every service read the version from the token and revalidate against the cached permission set, whose cache entry is tagged by user and invalidated by that event. The contract is: **a revoked permission stops working within 5 seconds of the revocation, on web and on mobile, without a sign-out.** A mobile client that is offline holds no more than its last-synced permission set and refuses every write that its cached set does not allow, then revalidates on reconnect.
6. **Scope is enforced with the permission, not after it.** `S` in the matrix is a server-side predicate, applied in the query, and backed by row-level security as the second barrier (master brief Section 7.4).
7. **Isolation level S is opt-in only.** No template inherits G20 by holding a broader role. A principal who needs a safeguarding record uses break-glass (`WF-SEC-02`), states a reason, and the access alerts the safeguarding officer and is itself logged.
8. **Every template ships with its own tests.** The permission matrix generates authorization tests (master brief Section 20): for each role and each endpoint, one test that the allowed call succeeds and one that the forbidden call returns the permission-denied code from Appendix K.
