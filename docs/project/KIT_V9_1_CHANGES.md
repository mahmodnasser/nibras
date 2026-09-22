# Kit v9.1 changes: every brief correction under ADR-0019

One bullet per change, with the finding or service sheet it came from. The findings were logged while the plan was written (`tools/plan-build/parts/brief-findings.md`); the product owner approved applying them and settled four conflicting values (ADR-0019). Items deliberately not applied are listed with their reason; most are plan-side follow-ups for scorecard themes 4 and 5.

## Master brief and reference architecture

- docs/brief/01-master-brief.md Section 19, Backend, rate-limiting bullet: split "A limit breach returns Problem Details with Retry-After" into rate limits (429, `<SERVICE>_RATE_LIMITED`, `Retry-After`) and plan quotas (402 `PLATFORM_PLAN_LIMIT_REACHED`, no `Retry-After`, limit, usage and upgrade action) (source: brief-findings "From document 22 and REQ-PLT-032"; Appendix K.1 and K.3)
- docs/brief/01-master-brief.md Section 6.4 table: added the k6 row (AGPLv3; load, soak and performance-budget tests; standalone binary in CI and test environments only, never linked, never shipped) (source: brief-findings "From document 19")
- docs/brief/01-master-brief.md Section 6.4, after the table: added the sentence that Forgejo (GPLv3 since 9.0) and Matomo (GPLv3) need a row and an allow-list entry only if chosen (source: brief-findings "From document 19")
- docs/brief/01-master-brief.md Section 10.4 table, Invitation row: "expiring link" now "expires after 14 days, with a reminder to the invitee at day 7" (source: product-owner decision; brief-findings "From sheets: Identity, Platform, Audit, Gateway" conflict 7 vs 14 days)
- docs/brief/01-master-brief.md Section 12.1 row 39: calendar-aware scaling marked "Moved to engineering capabilities (ADR-0019)", still built and measured, number kept so 40 to 44 keep theirs; moment column "none" (source: product-owner decision; brief-findings "From document 32")
- docs/brief/01-master-brief.md Section 12.1 intro: "These twenty" now says nineteen of the twenty numbers are signature features since item 39 moved (source: product-owner decision; brief-findings "From document 32")
- docs/brief/01-master-brief.md Section 28 intro: ranges for phases 1 to 6 stated as derived from docs/plan/34-work-breakdown.md by tools/plan-build/schedule-34.mjs for the Section 29 team (5 to 8 builders, 1.3 overhead), phase 6 calendar floor, recomputed never hand-adjusted (source: brief-findings "From sheet: Ai", last bullet; docs/plan/17-roadmap.md Section 1)
- docs/brief/01-master-brief.md Section 28 table, Range column: phase 1 8-10 to 14-22, phase 2 16-20 to 14-21, phase 3 10-12 to 11-17, phase 4 8-10 to 7-11, phase 5 10-14 to 9-14; phase 6 unchanged at 6 to 8 weeks (source: same)
- docs/brief/01-master-brief.md Section 28, after the table: added "Total from the start of phase 1 to launch: 61 to 93 weeks" (source: same; docs/plan/17-roadmap.md Section 1)
- docs/brief/01-master-brief.md Section 28, MVP cut line paragraph: added "42 capabilities and 33 to 50 weeks from the start of phase 1", derived by the same script, pointing to docs/plan/17-roadmap.md Section 5 (source: same; docs/plan/17-roadmap.md Section 5)
- docs/brief/01-master-brief.md Section 32, Deleted tenant row: verified 30-day cooling-off, no change (source: product-owner decision)
- docs/brief/03-reference-architecture.md Section 8.0 table, Bff.Web/Bff.Mobile row: "none as gRPC"; Bff.Web submits assist jobs to Ai over REST and serves Ai's three internal routes (source: docs/plan/06-services/ai.md open point 1 and section 6)
- docs/brief/03-reference-architecture.md Section 8.0 table, Identity row: Platform `Settings`, `Retention.ListActiveHolds`, `Tenants.Checksum`; School staff checksum and guardian eligibility check (source: docs/plan/06-services/identity.md section 6.2 and open point 5)
- docs/brief/03-reference-architecture.md Section 8.0 table, Platform row: Identity `ApiKeyAdministration`, `PermissionLookup.GetRoleRisk`; School student and staff directory for OneRoster; job only every service's `Usage.Recount` (source: docs/plan/06-services/platform.md section 6.2 and open point 7)
- docs/brief/03-reference-architecture.md Section 8.0 table, Academics row: added Scheduling `Timetables` (published version entries, nightly checksum) (source: docs/plan/06-services/academics.md section 6.2 and open point 3; brief-findings "Reference architecture 8.0: omits gRPC calls to Scheduling")
- docs/brief/03-reference-architecture.md Section 8.0 table, Scheduling row: School staff, structure and student directory (rooms, calendar days, sections, exam-seating candidates); job only Academics `TeachingAssignments.Checksum`, Hr `Leave.Checksum` (source: docs/plan/06-services/scheduling.md section 6.2 and open point 3; hr.md section 6)
- docs/brief/03-reference-architecture.md Section 8.0 table, Attendance row: added Scheduling `Timetables` (campus day, published version); job only Hr `Leave.Checksum` (source: docs/plan/06-services/attendance.md open point 1; scheduling.md and hr.md section 6 caller lists)
- docs/brief/03-reference-architecture.md Section 8.0 table, Operations row: added job only Scheduling `Timetables.Checksum` (source: docs/plan/06-services/operations.md section 6 and open point 8)
- docs/brief/03-reference-architecture.md Section 8.0 table, Ai row: "none as gRPC; reads through Bff.Web's three internal routes over REST, from its jobs only" (source: docs/plan/06-services/ai.md section 6 and open point 1)
- docs/brief/03-reference-architecture.md Section 8.0, after the table: added "Calls every service makes" (Platform `Tenants.GetTenantContext`, `Settings.GetSettings`, `Retention.ListActiveHolds`; Identity `PermissionLookup.GetEffectivePermissions`; owners' `Checksum`/`ListSnapshotPage`; Communication `CheckPermission`) (source: docs/plan/06-services/platform.md and identity.md section 6.1)
- docs/brief/03-reference-architecture.md Section 8.0, after the table: defined "job only" and stated the one-hop rule once (no synchronous call from inside a handler serving one; `GrpcHopRules`) (source: identity.md and platform.md section 6.2 hop statements)
- docs/brief/03-reference-architecture.md Section 8.0, after the table: added "Ai over REST, through Bff.Web" with the three routes exactly as the Ai sheet proposes (`GET /bff/web/v1/internal/ai/sources/{sourceEntity}?cursor=`, `POST /bff/web/v1/internal/ai/sources/authorize`, `POST /bff/web/v1/internal/ai/tools/{toolName}`), model calls answer 202 (source: docs/plan/06-services/ai.md open point 1, sections 5 and 6)
- docs/brief/03-reference-architecture.md Section 16 table, Redis row: Valkey 8 now Valkey 9.1 (current 9.1.2); added the rule that Redis is never pinned below 8.0 (source: brief-findings "From document 19")

Not applied, deliberately:
- Absence-alert timing (30 minutes vs 30 seconds; master brief Section 31): still open with the product owner.
- Moving public API, OneRoster and iCal to Tier 1 (master brief Section 13): still open with the product owner.
- k6 entry in tools/license-scan/allow.json: outside the files this editor owns.
- Job-only calls named by sheets for services outside the task list (Audit to Platform, Documents to Platform, Reporting snapshots, Wellbeing `Counts` and `AllergyAlerts`, Finance reconciliation methods): not added to table 8.0 rows; the task scoped 8.0 to School, Platform, Identity, Scheduling, Hr, Academics and Ai. The generic "Calls every service makes" paragraph covers the Platform settings and retention reads, but the Reporting, Wellbeing, Documents branding and Finance calls still need rows.
- Identity's additional local copies (staff directory, teaching assignments; identity.md) in the 8.0 middle column: not in the task's scope.
- Section 12.1 heading "Twenty more that decide whether a school switches": left as is (heading kept; the intro sentence carries the change).

## Appendix E: events

- appendix-e-event-catalog.md cross-cutting events: added a paragraph saying there is no shared audit routing key; an audit entry named in Appendix R as a workflow side effect is the publishing service's own `<service>.audit.recorded.v1` (source: brief-findings Attendance/Assessment and Finance/Requests items; Finance, School, Attendance, Assessment sheets)
- appendix-e-event-catalog.md Platform, `platform.upgrade.started.v1`: new row, consumers Notification and Reporting (source: brief-findings Identity/Platform/Audit; platform.md open point 2)
- appendix-e-event-catalog.md Platform, `platform.tenant.provisioned.v1`: added Documents as a consumer, for the branding copy (source: doc 11 section 2.6)
- appendix-e-event-catalog.md Identity: added a paragraph on the contact-point events: the address is Confidential, Notification stores it encrypted and never logs it (source: notification.md open point 1)
- appendix-e-event-catalog.md Identity, `identity.break-glass.granted.v1`: new row (source: brief-findings Identity/Platform/Audit; identity.md open point 2)
- appendix-e-event-catalog.md Identity, `identity.impersonation.started.v1`: new row. Communication is a consumer so its hub can show the web shell banner (source: brief-findings; identity.md open point 2; doc 08 ShellStore)
- appendix-e-event-catalog.md Identity, `identity.contact-point.verified.v1` and `identity.contact-point.removed.v1`: new rows, consumer Notification. No sheet had a name, so the names follow Appendix L and use the Identity `ContactPoint` value object (source: brief-findings Finance/Requests/Notification; notification.md open point 1)
- appendix-e-event-catalog.md Identity, `identity.user.invited.v1`: added Platform and Admissions as consumers (source: doc 11 section 2.6; doc 13 sagas 1 and 3)
- appendix-e-event-catalog.md Identity, `identity.user.deactivated.v1`: added School and Admissions (source: doc 11 section 2.6; doc 13 sagas 3 and 5)
- appendix-e-event-catalog.md Identity, `identity.guardian-link.created.v1`: added Wellbeing and Admissions (source: doc 11 section 2.6; doc 10 section 6; doc 13 saga 3)
- appendix-e-event-catalog.md School, `school.academic-year.opened.v1`: added Platform (source: doc 11 section 2.6; doc 13 saga 1)
- appendix-e-event-catalog.md School, `school.section.created.v1`: added `nameEn` and `nameAr`, and added Communication, Behavior, Operations and Wellbeing as consumers. `school.section.changed.v1` keeps "the same set" (source: brief-findings School/Scheduling/Academics/Admissions; school.md open point 2; doc 11 section 2.6)
- appendix-e-event-catalog.md School, `school.student.enrolled.v1`: added Requests, Admissions and Notification (source: doc 11 section 2.6; notification.md open point 2)
- appendix-e-event-catalog.md School, `school.student.section-changed.v1`: added Requests, Notification and Ai (source: doc 11 section 2.6 saga outcomes; notification.md open point 2; brief-findings Ai; ai.md open point 2)
- appendix-e-event-catalog.md School, `school.student.status-changed.v1`: added Admissions (source: doc 11 section 2.6)
- appendix-e-event-catalog.md School, `school.student.promoted.v1`: added Admissions (source: admissions.md open point 6)
- appendix-e-event-catalog.md School, `school.student.profile-updated.v1`: added Requests (source: doc 11 section 2.6 saga outcomes)
- appendix-e-event-catalog.md School, `school.sibling.linked.v1`: new row, consumers Finance and Admissions (source: school.md open point 2; REQ-SCH-018; BR-ADM-005)
- appendix-e-event-catalog.md School, `school.sibling.unlinked.v1`: new row. No sheet had a name, so the name follows Appendix L; it is the reverse of the link, needed so that a removed link ends the sibling discount (source: brief-findings "siblings"; BR-FIN-005)
- appendix-e-event-catalog.md School, `school.guardian.updated.v1`: added Wellbeing, Requests and Reporting (source: doc 11 section 2.6; reporting.md open point 7 / doc 10 part 7.1)
- appendix-e-event-catalog.md School, `school.staff.created.v1`: added `namesEnAr`, and added Communication, Requests and Notification (source: brief-findings; school.md open point 2; doc 11 section 2.6; notification.md open point 2)
- appendix-e-event-catalog.md School, `school.staff.changed.v1`: new row with the `school.staff.created.v1` consumer set (source: school.md open point 2)
- appendix-e-event-catalog.md School, `school.staff.left.v1`: added Communication and Notification (source: doc 11 section 2.6; notification.md open point 2)
- appendix-e-event-catalog.md School, `school.room.changed.v1`: new row, consumer Scheduling (source: school.md open point 2; doc 10 open point 1; REQ-SCH-002)
- appendix-e-event-catalog.md School, `school.grade-level.changed.v1`: new row, consumer Admissions (source: school.md open point 2; doc 10 open point 1)
- appendix-e-event-catalog.md School, `school.department.changed.v1`: new row, consumer Hr (source: doc 10 open point 1; hr.md reference copies)
- appendix-e-event-catalog.md School, `school.grading-period.changed.v1`: new row, consumer Assessment. This is the grading-period change event for both the School and the Assessment findings (source: school.md open point 2; assessment.md open point 2; doc 10 open point 1)
- appendix-e-event-catalog.md School, `school.calendar-day.changed.v1` (holidays and non-teaching days): new row, consumers Scheduling, Attendance and Requests (source: school.md open point 2; requests.md open point 3; scheduling.md holiday copy)
- appendix-e-event-catalog.md Admissions, `admissions.offer.made.v1`: added Reporting (source: reporting.md open point 7 / doc 10 part 7.1)
- appendix-e-event-catalog.md Admissions, `admissions.re-enrollment.confirmed.v1` and `admissions.re-enrollment.declined.v1`: added Requests (source: doc 11 section 2.6 saga outcomes)
- appendix-e-event-catalog.md Academics, `academics.submission.missing.v1`: new row, consumer Reporting (source: brief-findings "missing submissions"; academics.md open point 2)
- appendix-e-event-catalog.md Academics, `academics.lesson-plan.submitted.v1`: added Ai (source: brief-findings Ai; ai.md open point 2; doc 25 section 4.2)
- appendix-e-event-catalog.md Assessment, `assessment.marks.overdue.v1`: added Reporting (source: reporting.md open point 7 / doc 10 part 7.1)
- appendix-e-event-catalog.md Assessment, `assessment.report-card.generated.v1`: new row, consumer Reporting (source: brief-findings Attendance/Assessment; assessment.md open point 1)
- appendix-e-event-catalog.md Assessment, `assessment.report-cards.published.v1`: added Ai (source: ai.md open point 2; doc 25 section 4.2)
- appendix-e-event-catalog.md Assessment, `assessment.exam-paper.approved.v1` and `assessment.exam-paper.released.v1`: new rows, consumers Notification and Reporting, partition key `examId`; the payload never carries paper content (source: brief-findings; assessment.md open point 1)
- appendix-e-event-catalog.md Assessment, `assessment.grade-change.approved.v1`: added Requests (source: doc 11 section 2.6 saga outcomes)
- appendix-e-event-catalog.md Scheduling, `scheduling.timetable.changed.v1` and `scheduling.room-booking.approved.v1`: added Requests (source: doc 11 section 2.6 saga outcomes)
- appendix-e-event-catalog.md Scheduling, `scheduling.substitution.assigned.v1`: added Requests and Reporting (source: doc 11 section 2.6; reporting.md open point 7 / doc 10 part 7.1)
- appendix-e-event-catalog.md Attendance: added a paragraph defining a mark review (source: attendance.md open point 8; Appendix M.3)
- appendix-e-event-catalog.md Attendance, `attendance.mark-review.requested.v1` and `attendance.mark-review.resolved.v1`: new rows, consumer Requests. These are the attendance events Requests consumes. No sheet had a name, so the names follow Appendix L (source: brief-findings "an attendance.* event consumed by Requests"; attendance.md open point 8)
- appendix-e-event-catalog.md Attendance, `attendance.attendance.marked.v1`: added Requests and Ai (source: doc 11 section 2.6 saga outcomes; ai.md open point 2; doc 25 section 4.2)
- appendix-e-event-catalog.md Attendance, `attendance.excuse.approved.v1` and `attendance.gate-pass.issued.v1`: added Requests (source: doc 11 section 2.6 saga outcomes)
- appendix-e-event-catalog.md Attendance, `attendance.attendance.not-marked.v1`: added Reporting (source: reporting.md open point 7 / doc 10 part 7.1)
- appendix-e-event-catalog.md Attendance, `attendance.dismissal.processed.v1`: added Wellbeing, so the WF-WEL-02 `Released` state can be automated (source: wellbeing.md open point 5)
- appendix-e-event-catalog.md Finance, `finance.payment.received.v1`: added the optional `sourceRefs` field, with a paragraph defining it (source: brief-findings School/Scheduling/Academics/Admissions and Ai; admissions.md open point 4)
- appendix-e-event-catalog.md Finance, `finance.invoice.overdue.v1`: added `studentId`, which is null for a pre-enrolment invoice (source: brief-findings; admissions.md open point 6)
- appendix-e-event-catalog.md Finance, `RaiseApplicationFee` command (`finance.commands.raise-application-fee.v1`, sent on `nibras.admissions`): added a one-row command table after the Finance events (source: brief-findings Ai, last item; admissions.md open point 4; SL-ADM-406). Document 11 also needs the command in its catalog, which is outside this file
- appendix-e-event-catalog.md Finance, `finance.scholarship.awarded.v1`, `finance.payer.changed.v1`, `finance.cash-session.closed.v1`, `finance.deposit.recorded.v1`: new rows (source: brief-findings Finance/Requests/Notification; finance.md open point 6)
- appendix-e-event-catalog.md Finance, `finance.fee-plan.assigned.v1`: added Admissions, School and Requests (source: doc 11 section 2.6)
- appendix-e-event-catalog.md Finance, `finance.invoice.issued.v1` and `finance.refund.processed.v1`: added Requests (source: doc 11 section 2.6 saga outcomes)
- appendix-e-event-catalog.md Finance, `finance.credit-note.issued.v1`: added Admissions and Requests (source: doc 11 section 2.6)
- appendix-e-event-catalog.md Finance, `finance.account.restricted.v1`: added Admissions and Reporting (source: admissions.md open point 6; reporting.md open point 7)
- appendix-e-event-catalog.md Finance, `finance.account.cleared.v1`: added Admissions, School and Reporting (source: admissions.md open point 6; doc 11 section 2.6; reporting.md open point 7)
- appendix-e-event-catalog.md Communication, `communication.announcement.published.v1`: added Ai (source: ai.md open point 2; doc 25 section 4.2)
- appendix-e-event-catalog.md Communication, `communication.meeting.booked.v1` and `communication.meeting.changed.v1`: added Requests (source: doc 11 section 2.6 saga outcomes)
- appendix-e-event-catalog.md Requests, `requests.request.reassigned.v1`, `requests.request.withdrawn.v1`, `requests.request.expired.v1`: new rows, consumers Notification and Reporting (source: brief-findings Finance/Requests/Notification; requests.md open point 1)
- appendix-e-event-catalog.md Documents, `documents.certificate.revoked.v1`: added Admissions, School and Requests (source: doc 11 section 2.6)
- appendix-e-event-catalog.md Documents, `documents.export.completed.v1`: added Platform, Requests and Reporting (source: doc 11 section 2.6; reporting.md open point 7)
- appendix-e-event-catalog.md Behavior, `behavior.incident.recorded.v1`: added Ai; restricted incidents are skipped under doc 25 (source: ai.md open point 2; doc 25 section 4.2)
- appendix-e-event-catalog.md Audit, `audit.integrity-check.failed.v1`: added Reporting (source: reporting.md open point 7)
- appendix-e-event-catalog.md Wellbeing: added a paragraph on the three events Attendance consumes. They carry no category, symptom or reason, and Attendance discards an intervention event that has no `sourceRuleId` (source: brief-findings Attendance/Assessment; attendance.md open point 3)
- appendix-e-event-catalog.md Wellbeing, `wellbeing.intervention.opened.v1`: added Attendance, and added the optional `sourceRuleId` (source: attendance.md open point 3; WF-ATT-01)
- appendix-e-event-catalog.md Wellbeing, `wellbeing.intervention.closed.v1`: added Attendance, and added `studentId` so the payload matches its partition key (source: attendance.md open point 3; WF-ATT-01)
- appendix-e-event-catalog.md Wellbeing, `wellbeing.clinic-visit.collection-arranged.v1`: new row, consumer Attendance, with no clinical field. No sheet had a name, so the name follows Appendix L and the WF-WEL-02 `CollectionArranged` state (source: brief-findings; attendance.md open point 3; wellbeing.md `/collection` endpoint)
- appendix-e-event-catalog.md Hr, `hr.leave.approved.v1` and `hr.leave.cancelled.v1`: added Requests (source: doc 11 section 2.6 saga outcomes)
- appendix-e-event-catalog.md Operations, `operations.transport.subscription-changed.v1`: added Requests (source: doc 11 section 2.6 saga outcomes)
- appendix-e-event-catalog.md scheduled jobs table: added "Missing work at closing" (Academics, every 5 minutes, per academics.md `AssignmentClosingJob`) and "Request expiry" (Requests, hourly, per requests.md `RequestExpiryJob`) as publishers of the new events (source: academics.md; requests.md)

Not applied, deliberately:
- The "six event keys absent from E" that Appendix R cites for School, Scheduling, Academics and Admissions: `school.academic-year.archived.v1`, `school.academic-year.reopened.v1`, `reporting.snapshot.sealed.v1`, the shared audit key, `operations.loan.returned.v1` and `operations.transport-subscription.changed.v1`. school.md open point 4 and reporting.md open point 9 resolve these by correcting Appendix R to the catalogued names, not by adding keys to E, and Appendix R is outside this file.
- A shared audit key: not added. The per-service `<service>.audit.recorded.v1` already existed and is now stated as the only form. Replacing the shared key in Appendix R is for the Appendix R editor.
- The other Appendix R keys named in the Identity, Platform, Documents, Hr, Operations and Wellbeing sheets were not added, because the task and the log name only the three listed. These include `identity.impersonation.ended.v1`, `identity.break-glass.expired.v1`, `platform.upgrade.completed.v1` and `platform.upgrade.rolled-back.v1`. Note: without `identity.impersonation.ended.v1`, the web banner cannot be turned off by an event.
- `requests.request.effect-failed.v1` and a cancelled event, both proposed in requests.md open point 1: not in the log's list (the log names withdrawn and expired only).
- `finance.fee-plan.changed.v1` (doc 10 open point 1): admissions.md open point 2 removes the fee-plan name copy in favour of codes only, so no consumer needs the event.
- Product-owner decisions (30-day cooling-off, 14-day invitation expiry with a day-7 reminder, 5-minute dedupe, feature 39): Appendix E holds only `coolingOffEndsAt` and `expiresAt` values that are computed elsewhere, so nothing here changes. Absence-alert timing and Tier 1 API: left open as instructed.

## Appendices B and I: permissions and role templates

- docs/brief/02-appendices/appendix-b-permissions.md Platform, `platform.tenants`: added special action `export` (high, reason required, never plan-gated, BR-PLT-006) (source: brief-findings "Identity, Platform, Audit, Gateway" tenant export; platform.md open point 5). No sheet proposed a name; follows Appendix L format.
- docs/brief/02-appendices/appendix-b-permissions.md Platform: new resource `platform.modules` (view, edit; `enable`, `disable` elevated) (source: findings, modules; platform.md open point 5). Name follows the sheet's `/platform/modules` route; Appendix L format.
- docs/brief/02-appendices/appendix-b-permissions.md Platform: new resource `platform.template-library` (view, create, edit; `publish`, `import`) (source: findings, template library; platform.md §5.7 `/platform/template-library`, open point 5).
- docs/brief/02-appendices/appendix-b-permissions.md Platform, `platform.jobs`: added `replay` (elevated) (source: platform.md open point 5 and 22-api-conventions §6.1, which already cite `platform.jobs.replay`; not itself a logged line, added so the plan's existing string resolves).
- docs/brief/02-appendices/appendix-b-permissions.md Identity: new resource `identity.join-codes` (view, create, delete; `rotate`, `revoke`) (source: findings, join codes; identity.md Decisions in force). Appendix L format; no sheet name existed.
- docs/brief/02-appendices/appendix-b-permissions.md School: new resources `school.departments` and `school.houses` (view, create, edit, delete) (source: findings, departments and houses; school.md open point 1).
- docs/brief/02-appendices/appendix-b-permissions.md Admissions: new resource `admissions.campaigns` (view, create, edit, delete; `open`, `close`) (source: findings, admission campaigns; admissions.md open point 1).
- docs/brief/02-appendices/appendix-b-permissions.md Admissions, `admissions.applications`: added `override-age` (elevated, reason required, BR-ADM-001) (source: findings, age override; admissions.md open point 1 and `/applications/{id}/age-override` route).
- docs/brief/02-appendices/appendix-b-permissions.md Academics: new resources `academics.student-groups` and `academics.rubrics` (view, create, edit, delete) (source: findings, student groups and rubrics; academics.md open point 1).
- docs/brief/02-appendices/appendix-b-permissions.md Assessment, `assessment.exams`: added `print-paper` (elevated) beside the existing `approve-paper` (source: findings "Appendix B missing assessment.exam-paper.print"; assessment.md open point 5). Chose B's existing resource over a new `assessment.exam-paper` resource. **Appendix R WF (Approved to PrintRequested guard, TC-ASM-024) must switch `assessment.exam-paper.print` to `assessment.exams.print-paper`.**
- docs/brief/02-appendices/appendix-b-permissions.md Scheduling: new resource `scheduling.constraints` (view, create, edit, delete) (source: findings, timetable constraints; scheduling.md open point 1).
- docs/brief/02-appendices/appendix-b-permissions.md Attendance, `attendance.student-attendance`: added `nudge` (source: findings, principal's nudge; attendance.md open point 6, which names the `nudge` action).
- docs/brief/02-appendices/appendix-b-permissions.md Attendance, `attendance.excuses`: added `view-medical-detail` (high, every read logged); row risk now high (source: findings, medical excuse detail; attendance.md open point 4).
- docs/brief/02-appendices/appendix-b-permissions.md Finance: new resource `finance.payers` (view, create, edit; `view-bank-details` high) (source: findings, finance.payers; finance.md open point 2, exact names taken from the sheet).
- docs/brief/02-appendices/appendix-b-permissions.md Communication: new resource `communication.concerns` (view, create; create never records the reporter, view high, reason required, logged) (source: findings, anonymous concern; communication.md open point 4 and `/anonymous-concerns` routes).
- docs/brief/02-appendices/appendix-b-permissions.md Requests: new resource `requests.duty-rosters` (view, create, edit, delete; `publish`, `assign`) (source: findings, duty roster; requests.md open point 7 and `/requests/duty-rosters` route).
- docs/brief/02-appendices/appendix-b-permissions.md Audit: new resource `audit.access-transparency` (view, normal, own-children or self scope only, roles and times, never reader names) (source: findings, guardian-safe transparency view; audit.md open point 2). The audit sheet and 08-web-structure should switch from `audit.access-log.view` to this.
- docs/brief/02-appendices/appendix-b-permissions.md "Rules the catalog enforces" rule 5: added `attendance.excuses.view-medical-detail`, `finance.payers.view-bank-details`, `communication.concerns.view` to the read-logged list (source: follows from the three new sensitive actions).
- docs/brief/02-appendices/appendix-b-permissions.md Notification and Ai sections: unchanged; the notification.* and ai.* defects are grants, applied in Appendix I (source: findings "no role granted notification.*", "no role template grants ai.*").
- docs/brief/02-appendices/appendix-b-permissions.md (no edit, instruction to Appendix R editor): **Appendix R must switch** `school.students.link-guardian` to `school.guardians.link`; `identity.access-review.decide` to `identity.access-reviews.certify`; `identity.join-request.approve` to `identity.join-requests.approve`; `platform.plan.change` to `platform.subscriptions.change-plan`; `assessment.exam-paper.print` to `assessment.exams.print-paper` (source: findings doc 12 and "Identity, Platform, Audit, Gateway"; identity.md open point 6, school.md open point 11, platform.md open point 3). B keeps its existing names per Appendix L (one name per fact).
- docs/brief/02-appendices/appendix-i-role-templates.md I.2 purpose table, Nurse mobile capabilities: removed "offline queue"; clinic entry, medication round and allergy lookup only while connected, nothing queued or stored on device (source: findings "From document 09"; Appendix M rows "Record a clinic visit: No").
- docs/brief/02-appendices/appendix-i-role-templates.md I.2 holds table, Nurse: G20 marked online only; added `attendance.excuses.view-medical-detail` (high, four-eyes grant); must-not adds `ai.*` (source: attendance.md open point 4; Appendix M).
- docs/brief/02-appendices/appendix-i-role-templates.md I.2 holds, Platform Administrator: added G25 for platform library notification templates (source: notification.md §11.1).
- docs/brief/02-appendices/appendix-i-role-templates.md I.2 holds, School Owner: added G26 (use) and `platform.tenants.export` (high, four-eyes) (source: BR-PLT-006; platform.md open point 5; ai.md §11.1).
- docs/brief/02-appendices/appendix-i-role-templates.md I.2 holds, Principal: added G25, G26; must-not adds `communication.concerns.view`, `attendance.excuses.view-medical-detail` (source: notification.md §11.1, ai.md §11.1, communication.md open point 4).
- docs/brief/02-appendices/appendix-i-role-templates.md I.2 holds, Vice Principal: G26 for use only, no `ai.configuration.*` (source: ai.md §11.1).
- docs/brief/02-appendices/appendix-i-role-templates.md I.2 holds, Academic Coordinator: G09 including `assessment.exams.print-paper`; G26 (use) (source: assessment.md open point 5; ai.md §11.1).
- docs/brief/02-appendices/appendix-i-role-templates.md I.2 holds, Head of Department: G26 (use); `school.departments.view` and `.edit` in department scope (source: school.md open point 1 impact; ai.md §11.1).
- docs/brief/02-appendices/appendix-i-role-templates.md I.2 holds, Registrar, HR Officer, Teacher: G26 (use) (source: ai.md §11.1).
- docs/brief/02-appendices/appendix-i-role-templates.md I.2 holds, Admissions Officer: G06 without `admissions.applications.override-age`, G26 (use); must-not adds `admissions.applications.override-age` (source: admissions.md open point 1, BR-ADM-001).
- docs/brief/02-appendices/appendix-i-role-templates.md I.2 holds, Homeroom Teacher must-not: added `attendance.excuses.view-medical-detail` (source: attendance.md open point 4).
- docs/brief/02-appendices/appendix-i-role-templates.md I.2 holds, Parent / Guardian: added `audit.access-transparency.view` (own children) (source: audit.md open point 2).
- docs/brief/02-appendices/appendix-i-role-templates.md I.2 holds, Accountant: G14 including `finance.payers.*`; `finance.payers.view-bank-details` only with four-eyes; G26 (use) (source: finance.md open point 2).
- docs/brief/02-appendices/appendix-i-role-templates.md I.2 holds, Counselor and Special-Needs Coordinator must-not: added `ai.*` (source: ai index refuses Sensitive and S data, 12-security TC-PRV-044).
- docs/brief/02-appendices/appendix-i-role-templates.md I.2 holds, Safeguarding Officer: added `communication.concerns.view` (high, reason, logged); must-not adds `ai.*` (source: communication.md open point 4 and §5.4).
- docs/brief/02-appendices/appendix-i-role-templates.md I.2 holds, IT Support: added `platform.jobs.replay`, G25 (`notification.delivery-log.*`, `notification.channels.*`), `ai.usage.view` (source: notification.md §11.1; platform.md open point 5).
- docs/brief/02-appendices/appendix-i-role-templates.md I.3 groups: new permissions added to representative lists of G01, G02, G03, G06, G07, G09, G10, G12, G14, G15, G16, G17 (source: the Appendix B additions above).
- docs/brief/02-appendices/appendix-i-role-templates.md I.3 groups: new groups G25 Notification and G26 Assist (Ai) (source: findings "no role granted notification.*", "no role template grants ai.*"; notification.md open point 4; ai.md open point 6). **Downstream: docs/plan/12-security-privacy-safety.md §4.3 and §5, 16-test-strategy.md, audit.md and gateway.md say "G01 to G24"; they must become G01 to G26.**
- docs/brief/02-appendices/appendix-i-role-templates.md I.4: added legend paragraph for G25 and G26 cells and a third matrix (Role by G25, G26) for all twenty-three templates (source: as above).
- docs/brief/02-appendices/appendix-i-role-templates.md I.5: new rule 8, every tenant template holds `notification.preferences.view`/`.edit` in self scope and `communication.concerns.create`; only the Safeguarding Officer holds `communication.concerns.view`. Former rule 8 renumbered 9 (no document cites I.5 rule 8) (source: notification.md open point 4; communication.md open point 4).
- NOT APPLIED: Appendix B `notification.inbox` resource. notification.md open point 4 defaults the inbox to `notification.preferences.*` in self scope; granted that instead of inventing a resource.
- NOT APPLIED: other gaps named only in sheet open points, not in the log (documents.files.edit, documents.imports.edit, behavior.badges.export, reporting.early-warning.override, reporting.governance.*, hr.payroll approve, identity.me.*, operations requisitions, kindergarten daily sheets, online classes, scheduling travel times as a separate resource). Outside the logged defects; left for a later ADR.
- NOT APPLIED: product-owner decisions (cooling-off 30 days, invitation 14 days, dedupe 5 minutes, feature 39) do not touch Appendix B or I. Absence-alert timing and Tier 1 API left open as instructed.

## Appendices K, C, G, J and F: error codes, notifications, settings, retention, entities

- appendix-k-error-codes.md K.2 Identity: added `IDENTITY_TOKEN_INVALID` (401) for a malformed, unverifiable or foreign token, raised by Identity and the Gateway (source: brief-findings "sheets: Identity, Platform, Audit, Gateway"; gateway.md open point 5; no sheet proposed a name, so Appendix L format `<SERVICE>_<MEANING>` was followed)
- appendix-k-error-codes.md K.2 Identity: added `IDENTITY_INVITATION_EXPIRED` (410), 14-day validity, revoked or already accepted (source: same finding; identity.md open point 8; no sheet name, Appendix L format; 14 days per product-owner decision)
- appendix-k-error-codes.md K.4 School: added `SCHOOL_CONFIGURATION_IN_USE` (409) (source: finding "sheets: School, Scheduling, Academics, Admissions"; name from school.md open point 8)
- appendix-k-error-codes.md K.6 Academics: added `ACADEMICS_GRADING_PERIOD_LOCKED` (409) (source: same finding; name from academics.md open point 6)
- appendix-k-error-codes.md K.7 Assessment: added `ASSESSMENT_APPEAL_WINDOW_CLOSED` (409, parent-safe) (source: finding "sheets: Attendance, Assessment, Bff.Web, Bff.Mobile"; assessment.md open point 4; no sheet name, Appendix L format)
- appendix-k-error-codes.md K.7 Assessment: added `ASSESSMENT_MARK_GRID_INCOMPLETE` (400) (source: same finding; assessment.md open point 4; no sheet name, Appendix L format)
- appendix-k-error-codes.md K.8 Scheduling: added `SCHEDULING_SOLVER_RUNNING` (409) (source: finding "sheets: School, Scheduling, Academics, Admissions"; name from scheduling.md open point 4)
- appendix-k-error-codes.md K.21 AI Assist, row `AI_USAGE_LIMIT_REACHED`: HTTP changed from 402 to "200 on assist requests; 402 only on a rung 4 request". Assist requests degrade to rung 1 with the code as the degradation reason (document 25 §8 and §10). The 402 remains only for an administrator's explicit request to route a feature to the external provider (rung 4) while the plan's rung 4 allowance is spent: in the Ai sheet this is `PUT /api/v1/ai/configuration/features/{featureCode}` with `preferExternal: true`, or activating the provider through `PUT /api/v1/ai/configuration/provider` (source: finding "sheet: Ai"; ai.md section 5 degradation rule, section 11.4, open point 3)
- appendix-k-error-codes.md new section K.23 Gateway and backends-for-frontends, placed after K.22 so that existing references to "K.22 rule N" stay valid: added `GATEWAY_BODY_TOO_LARGE` (413) (source: finding "sheets: Identity, Platform, Audit, Gateway"; gateway.md open point 2; no sheet name, Appendix L format)
- appendix-k-error-codes.md K.23: added `BFF_APP_VERSION_BELOW_MINIMUM` (403, parent-safe) (source: finding "sheets: Attendance, Assessment, Bff.Web, Bff.Mobile"; bff-mobile.md open point 3; `BFF_` prefix from bff-mobile.md section 5 and document 22 §12.3; no sheet name, Appendix L format)

- appendix-c-notifications.md rules paragraph: deduplication stays five minutes; added that this is the default BR-NOT-004 applies and that urgent messages are never deduplicated (source: finding "sheets: Finance, Requests, Notification, Communication"; notification.md decision row and `dedup_window_minutes`; product-owner decision 5 minutes)
- appendix-c-notifications.md row "Period still uncovered 30 minutes before it starts" after "Substitution assigned" (source: finding "sheets: School, Scheduling, Academics, Admissions"; scheduling.md `UncoveredPeriodEscalationJob`, open point 9)
- appendix-c-notifications.md row "Welcome pack after enrolment" (source: same finding; admissions.md section notifications, open point 10)
- appendix-c-notifications.md row "Invitation sent", trigger `identity.user.invited.v1`, states the 14-day expiry (source: finding "sheets: Identity, Platform, Audit, Gateway"; identity.md open point 1; product-owner decision 14 days)
- appendix-c-notifications.md row "Invitation reminder at day 7" (source: same finding; identity.md `JoiningTimeoutsJob`; product-owner decision day-7 reminder)
- appendix-c-notifications.md row "One-time code or password reset link", U, never deduplicated, covers Identity sign-in and reset plus Admissions and Requests public-form OTP (source: findings "School, Scheduling, Academics, Admissions" (OTP) and "Identity, Platform, Audit, Gateway" (one-time codes); identity.md open point 1, admissions.md open point 10, requests.md open point 5)
- appendix-c-notifications.md row "Audit export performed" to the principal, N, email (source: finding "Identity, Platform, Audit, Gateway"; audit.md `AuditExportJob`, open point 1)
- appendix-c-notifications.md five platform lifecycle rows: "Signup confirmed, tenant provisioned and welcome pack", "Tenant deletion cooling-off reminder" (30-day cooling-off), "API key expiring, sandbox inactive", "Support ticket SLA escalated", "Data subject request acknowledged, deadline approaching" (source: same finding; platform.md section 11.2, jobs `DeletionCoolingOffReminderJob`, `ApiKeyExpiryReminderJob`, `SandboxLifecycleJob`, `SupportSlaJob`, `SubjectRequestDeadlineJob`, open point 1; product-owner decision 30 days)
- appendix-c-notifications.md row "AI injection attempts blocked *(T2)*", daily digest, never the planted text (source: finding "sheet: Ai"; row name from ai.md open point 5)

- appendix-g-settings.md Finance, rounding: decimals now follow ISO 4217 minor units (JOD 3, SAR and AED 2) instead of "2 decimals" (source: findings "documents 23 and 24" and "Finance, Requests, Notification, Communication")
- appendix-g-settings.md Joining, invitation expiry: default 14 days with a reminder at day 7 (source: finding "Identity, Platform, Audit, Gateway" conflict 7 vs 14 days; product-owner decision)
- appendix-g-settings.md AI: added template version per feature, model tag per feature, rung 4 field groups, with defaults (source: finding "sheet: Ai"; ai.md section 4.6, 4.7, 11.3 and open point 4; document 25 §3.3, §6.3)

- appendix-j-data-classification-and-retention.md J.2 Platform, identity, and evidence, row "Token / Invitation link...": invitation retention 7 days changed to 14 days (source: finding "Identity, Platform, Audit, Gateway"; product-owner decision)
- appendix-j-data-classification-and-retention.md row "Tenant": already states the 30-day cooling-off; no change needed

- appendix-f-entities.md: added the Ai line: IndexedChunk (table `ai_index.embedding_chunk`, the document 25 name), SourceCheckpoint, PurgedSubject, IndexRebuild, AssistJob and Payload, FeatureConfiguration, ProviderConfiguration, ProviderConsent, CallLogEntry, UsagePeriodTotal, ModelRelease (source: finding "sheet: Ai"; names from ai.md section 4 and open point 9)

- Appendix C preamble says a trigger is an Appendix E event or a job from E's job table. The new rows name jobs that E's job table does not list yet (uncovered-period escalation, joining timeouts, deletion cooling-off reminder, API key expiry reminder, sandbox lifecycle, support SLA, subject request deadline, the Ai daily digest). Every row's published key, `notification.notification.requested.v1`, is in E. The E job table belongs to another editor.
- Document 11 must add `nibras.ai`, `nibras.scheduling`, `nibras.identity`, `nibras.audit` as `RequestNotification` senders (the finding's "nibras.ai cannot send RequestNotification" part). That is plan-level and not in my files.
- `AI_DISABLED_FOR_TENANT` stays 403, as the Ai sheet default keeps it.
- `AI_MODEL_UNAVAILABLE` still says 503. The Ai sheet sends it as a 200 degradation reason. No logged defect covers it, so I did not change it.
- BR-NOT-004's 10-minute example is in Appendix S, which is not mine. Appendix C keeps 5 minutes.
- Plan follow-ups: gateway.md (use `GATEWAY_BODY_TOO_LARGE` and `IDENTITY_TOKEN_INVALID`), bff-mobile.md open point 3 (the code now exists; the sheet default was no refusal code), identity.md (`IDENTITY_INVITATION_EXPIRED`, invitation expiry default 7 to 14 days in section 11.3), school/academics/scheduling/assessment sheets (adopt the new codes), document 22 §12.3 counts (Identity 14, School 10, Academics 10, Assessment 11, Scheduling 10; Gateway and Bff gain one each).
- Absence-alert timing and moving the public API, OneRoster and iCal to Tier 1 are still open. These files do not touch either one. Signature feature 39 does not appear in these files.

## Appendices O, P, A and W: demo script, differentiation, feature catalog, feature register

- appendix-w-feature-register.md intro: autonomy level added to the list of things every feature declares (source: brief-findings "From documents 25, 26, 27"; master brief Section 25)
- appendix-w-feature-register.md new paragraph "Feature 39 is no longer a signature feature": moved to engineering capabilities in Appendix A, A1, still built and measured by Appendix N N-01 and N-11, row kept so numbering is stable (source: product-owner decision; brief-findings "From document 32"; doc 32 open point 5)
- appendix-w-feature-register.md new paragraph "Autonomy levels": defines levels 1 surfaces, 2 suggests, 3 drafts, 4 acts, copied from docs/plan/25-ai-and-assist-ladder.md section 1.2 (source: brief-findings "From documents 25, 26, 27")
- appendix-w-feature-register.md new paragraph "Test identifiers": records the renumbering below (source: brief-findings "From sheets: Identity, Platform, Audit, Gateway" and "From document 32")
- appendix-w-feature-register.md feature table: new Autonomy column after Rung; values from doc 25 section 2.1: 4 = 2 suggests, 6 = 3 drafts, 26 = 4 acts (pre-fill only), 29 = 3 drafts, 30 = 3 drafts, 40 = 4 acts, 42 = 2 suggests, 43 = 2 suggests, all other rows 1 surfaces (doc 25: "the remaining 33 rows ... are rung 1 at autonomy 1") (source: docs/plan/25-ai-and-assist-ladder.md section 2.1)
- appendix-w-feature-register.md row 15: TC-PLT-002 -> TC-PLT-801 (source: test-id collision with Appendix R WF-PLT-01; platform sheet open point 6)
- appendix-w-feature-register.md row 29: TC-PLT-003 -> TC-PLT-802 (same source)
- appendix-w-feature-register.md row 37: TC-PLT-004 -> TC-PLT-803 (same source)
- appendix-w-feature-register.md row 41: TC-PLT-005 -> TC-PLT-804 (same source)
- appendix-w-feature-register.md row 36: TC-DOC-002 -> TC-DOC-801; Appendix O keeps TC-DOC-002 for QR verification (source: brief-findings "From document 32"; doc 32 open point 3)
- Proof the new ids were unused: grep -rIl -E "TC-PLT-80[1-4]|TC-DOC-801" over C:\Repo\Nibras (the whole kit plus its parent) returned no file before the edit. Existing TC-PLT ids stop at 001-006, 011-016, 021-026, 101-125, 901-904, 951-989; TC-DOC ids at 001, 002, 301-360, 957
- appendix-w-feature-register.md row 39: feature text marked "moved to engineering capabilities (ADR-0019)", moment replaced with a pointer to Appendix A, A1, demo cell now "None; measured by Appendix N scenarios N-01 and N-11" (TC-INF-001 dropped from the row); rung, autonomy, tier kept so kit-lint R11 still passes (source: product-owner decision)


- appendix-o-demo-script.md "Rules of the script": every step now also names the phase from which it runs as scripted (source: brief-findings "From document 32"; doc 32 section 3)
- appendix-o-demo-script.md new "Phases" paragraph: defines the Phase column against docs/plan/17-roadmap.md phase demos and the MVP cut line (source: doc 17 sections 1, 4 and 5; doc 32 open point 4)
- appendix-o-demo-script.md new "Features with no minute of their own" paragraph: feature 23 folded into every minute (TC-UX-001), eighteen features in the reserve bank, feature 39 has no step (source: doc 32 section 4.2; product-owner decision)
- appendix-o-demo-script.md all three act tables: new Phase column. Minutes 1, 5, 14 run from phase 4 (Reporting), minute 10 from phase 3 (Finance), minute 12 from phase 5 (Wellbeing), each marked "Earlier: substitution"; minute 2 and the revoked certificate of minute 9 at the MVP cut line; minute 15 from phase 1; the rest from phase 2 (source: doc 32 sections 3.1 and 3.2; doc 17 section 4)
- appendix-o-demo-script.md minute 4: feature 44 folded in (data-saver profile on and said), test TC-MOB-005 added, data-saver from phase 4 (CAP-MOB-03) (source: doc 32 section 4.2)
- appendix-o-demo-script.md minute 5: playbook opens in the same side panel as the because panel (source: doc 32 section 4.1)
- appendix-o-demo-script.md minute 10: invoice batch previewed before the meeting; "record a payment" moved to reserve step R-15 (source: doc 32 section 4.1)
- appendix-o-demo-script.md minute 11: data export shown as accepted with progress, then a pre-prepared export opened (source: doc 32 section 4.1)
- appendix-o-demo-script.md minute 12: feature 11 folded in (command palette reaches the other teacher's class), test TC-WEB-001 added (source: doc 32 section 4.2)
- appendix-o-demo-script.md minute 13: ends on reunification with verified pickup (source: doc 32 section 4.1; doc 02 recommendation)
- appendix-o-demo-script.md minute 15: feature 37 folded in (template exchange, TC-PLT-803); smart defaults test TC-PLT-003 -> TC-PLT-802; TC-PLT-003 kept in its Appendix R meaning (provisioning completes, owner invitation sent); live provisioning switches to a pre-provisioned tenant; invitation valid 14 days with a reminder at day 7 (source: doc 32 sections 4.1 and 4.2; test-id collision; product-owner decision on invitation expiry)
- appendix-o-demo-script.md note under act three: states the kept meanings of TC-PLT-003 and TC-DOC-002 and the new TC-DOC-801 (source: test-id collisions)
- appendix-o-demo-script.md new section "Before a minute's phase": substitutions for minutes 1 (request approvals, TC-RQS-002), 5 (threshold flag and intervention, TC-RPT-008), 10 (bilingual-documents minute, TC-L10N-202, TC-L10N-301), 12 (custody-restricted record, TC-SEC-001), 14 (reserve R-01, TC-L10N-311) (source: doc 32 section 3.1 and open point 4)
- appendix-o-demo-script.md new section "The reserve bank": R-01 to R-20 with persona, step, feature, test and phase; release-gated; at most two swaps per demo in the same act; R-16 to R-18 IT lead only; feature 15 covered by the pre-run reset and TC-PLT-801 (source: doc 32 sections 4.2 and 4.3 and open point 1; phases from doc 32 section 1 capability column)
- appendix-o-demo-script.md "How this appendix is verified": reserve steps run on every release; a signature feature needs a minute or a reserve step (source: doc 32 open point 1)


- appendix-p-differentiation.md new "Check of 2026-09-20" paragraph: points to doc 02 "Corrections to Appendix P" for sources and dates; unverified absence never written as "no" (source: doc 02)
- appendix-p-differentiation.md P.1 openSIS: "no real multi-tenancy" reworded to multi-school claimed, isolation undocumented (correction 1) (source: doc 02)
- appendix-p-differentiation.md P.1 openSIS: "limited mobile" -> "no mobile application found (unverified absence)"; shape gains GNU GPL (correction 2) (source: doc 02)
- appendix-p-differentiation.md P.1 Gibbon: "limited finance" -> fees and invoicing, no payroll, ledger or gateway found (correction 3) (source: doc 02)
- appendix-p-differentiation.md P.1 Gibbon: added "no application programming interface and no webhooks"; shape gains GNU GPL (correction 4) (source: doc 02)
- appendix-p-differentiation.md P.1 Fedena shape: "hosted or on-premises" -> cloud-hosted on every listed plan, open-source basic version, on-premises unverified (correction 5) (source: doc 02)
- appendix-p-differentiation.md P.1 PowerSchool: "weaker outside North America" removed; regional competitor with Arabic right-to-left edition added to strengths (correction 6) (source: doc 02)
- appendix-p-differentiation.md P.1 Classter shape: on-premises claimed in marketing, unverified (correction 7) (source: doc 02)
- appendix-p-differentiation.md P.1 Classter strengths: Arabic language version added (correction 8) (source: doc 02)
- appendix-p-differentiation.md P.1 ManageBac: finance and HR unverified; Arabic for parents only (correction 9) (source: doc 02)
- appendix-p-differentiation.md P.1 Toddle: kept "not a full SMS", added what it covers and no finance, Arabic or offline (correction 10) (source: doc 02)
- appendix-p-differentiation.md P.1 Classera: finance and operations weakness marked unverified; API confirmed only by third-party listing (correction 11) (source: doc 02)
- appendix-p-differentiation.md P.2 "Open source, honestly" -> "Permissively licensed, modern, multi-tenant and open source", with openSIS, Gibbon and Fedena named (correction 12) (source: doc 02)
- appendix-p-differentiation.md P.2 "Open by default": leads with webhooks, iCal, OneRoster and read-only before suspension; API alone is parity, SIX of ten products (doc 02's corrections table says seven but lists six footnotes and its matrix supports six) (correction 13) (source: doc 02 capability matrix)
- appendix-p-differentiation.md P.2 "Arabic and English are equal": states the interface is parity (four of ten) and the data model is the difference (correction 14) (source: doc 02)
- appendix-p-differentiation.md P.3 cafeteria and wallet: who has it -> unverified (correction 15) (source: doc 02)
- appendix-p-differentiation.md P.3 marketplace: Classter confirmed, Veracross and Toddle partner listings, PowerSchool unverified (correction 16) (source: doc 02)
- appendix-p-differentiation.md P.3 alumni: Fedena and Classera added as confirmed, Veracross and Blackbaud unverified (correction 17) (source: doc 02)
- appendix-p-differentiation.md P.4 row 26: "several have gate integration" -> Fedena biometric, others unverified (correction 18) (source: doc 02)
- appendix-p-differentiation.md P.4 row 32: -> broadcast built into Blackbaud; roll call and reunification unmatched (correction 19) (source: doc 02)
- appendix-p-differentiation.md P.4 row 31: kept "very rare", marked unverified absence pending a trial (correction 20) (source: doc 02)
- appendix-p-differentiation.md P.4 row 44: kept "rare", marked unverified absence with the Toddle 3G note (correction 21) (source: doc 02)
- appendix-p-differentiation.md P.4 row 44 proof: "Not in the fifteen-minute script" -> "Minute 4, TC-MOB-005" (source: the minute 4 fold in Appendix O)
- appendix-p-differentiation.md P.2 "Live in a day" and P.4 row 29: TC-PLT-003 -> TC-PLT-802 (source: test-id renumbering)
- appendix-p-differentiation.md P.4 intro: feature 39 has no row, now an engineering capability in Appendix A (source: product-owner decision)


- appendix-a-feature-catalog.md A1 tenancy bullet: "a cooling-off period" -> "a 30-day cooling-off period during which the export stays available" (source: product-owner decision; master brief Section 32 table "30-day cooling-off")
- appendix-a-feature-catalog.md A1 new bullet: calendar-aware scaling as an engineering capability, formerly signature feature 39, master brief Section 34, measured by Appendix N N-01 and N-11 (source: product-owner decision)
- appendix-a-feature-catalog.md A14 digests bullet: deduplication defined as same template, recipient and subject within a 5-minute window (BR-NOT-004) (source: product-owner decision; brief-findings "From sheets: Finance, Requests, Notification, Communication")


- Tier 1 split of feature 24 (public API, OneRoster, iCal): still open for the product owner; W row 24 stays Tier 2 and P.2 does not promise a Tier 1 API.
- Absence-alert timing (30 minutes vs 30 seconds): still open; none of these four files states it.
- Doc 02 correction 6's second half (add a PowerSchool risk to master brief Section 40): outside these files; for the coordinator or the master-brief editor.
- Doc 32 section 4.1, moving feature 10 (configurable without code) from minute 10 to minute 15: not in the defect log, and it would put a fourth feature into minute 15. Minute 10 still lists feature 10.
- Invitation expiry: set only where minute 15 mentions the invitation. Appendix A has no invitation text to change.


- TC-PLT-001 also collides: Appendix R uses it for WF-PLT-01 Requested to Validated, and W row 10 (configurable without code) uses it too. Not renumbered, because the task named only 002 to 005.
- TC-INF-001 and TC-INF-002 collide with Appendix R's upgrade workflow (R line about "Scheduled to PreChecked" uses TC-INF-001). TC-INF-001 was removed from W row 39 by the move. W row 40 still uses TC-INF-002; not checked against R and not renumbered.
- The Documents sheet (docs/plan/06-services/documents.md, open point 10) makes the opposite choice: TC-DOC-002 for school memory and TC-DOC-302 for verification. The brief now keeps TC-DOC-002 for verification (Appendix O) and uses TC-DOC-801 for the yearbook, as the task and doc 32 open point 3 say. The sheet needs aligning.
- The Platform sheet (section 11 test table, open point 6) and docs 03, 08 and 20 still cite TC-PLT-002 to 005 in the Appendix W meaning; they need TC-PLT-801 to 804.
- docs/plan/30-plan-scorecard.md notes that TC-INT-001 tests refusal of untagged operations, not webhooks. P.2 "Open by default" and O's R-12 still cite TC-INT-001 as the webhook proof, as W does.

## Appendices R, S, T, M and U: workflows, rules, year in the life, offline rules, journeys

Permissions (the Appendix B names that the BI log says to use):
- appendix-r-workflow-catalog.md WF-IDN-01, PendingApproval to Approved (TC-IDN-004): `identity.join-request.approve` changed to `identity.join-requests.approve` (source: brief-findings "Identity, Platform, Audit, Gateway"; adr19-BI; identity.md open point 6)
- appendix-r-workflow-catalog.md WF-IDN-02, MatchProposed to LinkApproved (TC-IDN-014): `school.students.link-guardian` changed to `school.guardians.link` (source: brief-findings "From document 12"; adr19-BI; school.md open point 11)
- appendix-r-workflow-catalog.md WF-SEC-01, Opened to InProgress (TC-SEC-002): `identity.access-review.decide` changed to `identity.access-reviews.certify` (source: brief-findings "From document 12"; adr19-BI)
- appendix-r-workflow-catalog.md WF-PLT-02, Requested to LimitChecked (TC-PLT-012): `platform.plan.change` changed to `platform.subscriptions.change-plan` (source: brief-findings; adr19-BI; platform.md open point 3)
- appendix-r-workflow-catalog.md WF-ASM-03, Approved to PrintRequested (TC-ASM-024): `assessment.exam-paper.print` changed to `assessment.exams.print-paper` (source: brief-findings "Attendance, Assessment"; adr19-BI; assessment.md open point 5)
- appendix-r-workflow-catalog.md WF-WEL-02, Arrived to Assessed (TC-WEL-011): `wellbeing.clinic-visit.record` changed to `wellbeing.clinic-visits.create`. This defect was not in the log, but lint on Appendix B showed the name did not exist (source: wellbeing.md section 3 invariant 1 and the `POST /clinic-visits` row)

Audit key:
- appendix-r-workflow-catalog.md, every workflow's side effects (42 occurrences): `audit.action.recorded.v1` changed to the owning service's `<service>.audit.recorded.v1`. Examples: `identity.`, `platform.`, `school.`, `admissions.`, `assessment.`, `attendance.`, `finance.`, `requests.`, `behavior.`, `wellbeing.`, `hr.`, `operations.`, `documents.`. WF-SEC-02 and WF-SEC-03 say "each acting service's `<service>.audit.recorded.v1`" for actions taken inside the elevated or impersonated session, because those actions happen in other services (source: brief-findings "Attendance, Assessment" and "Finance, Requests"; Appendix E cross-cutting paragraph, adr19-E)

Event keys missing from Appendix E, switched to catalogued keys or to the owner's audit key, using the name each sheet uses:
- appendix-r-workflow-catalog.md WF-IDN-04 and WF-HR-01 side effects: `identity.delegation.activated.v1` changed to `identity.delegation.started.v1` (source: identity.md open point 2; hr.md open point 10)
- appendix-r-workflow-catalog.md WF-SEC-01 side effects: `identity.access-review.opened.v1` changed to `identity.access-review.due.v1` sent to each reviewer. `identity.access-review.certified.v1` removed; the opening and the certification are now recorded as `identity.audit.recorded.v1` (source: identity.md open point 2)
- appendix-r-workflow-catalog.md WF-SEC-02 side effects: `identity.break-glass.expired.v1` removed. Added `identity.break-glass.used.v1` for each record opened under the grant, and `identity.audit.recorded.v1` for the expiry and the revocation (source: identity.md open point 2; `identity.break-glass.granted.v1` is now in E per adr19-E)
- appendix-r-workflow-catalog.md WF-SEC-03 side effects: `identity.impersonation.ended.v1` replaced by `identity.audit.recorded.v1` for the end of the session (source: identity.md open point 2; adr19-E did not add the ended key)
- appendix-r-workflow-catalog.md WF-SCH-01 side effects: `operations.loan.returned.v1` replaced by `operations.audit.recorded.v1` for each library loan returned or charged (source: school.md open point 4; operations.md open point 6)
- appendix-r-workflow-catalog.md WF-SCH-03 side effects: `school.academic-year.archived.v1` and `school.academic-year.reopened.v1` replaced by `school.audit.recorded.v1`. `reporting.snapshot.sealed.v1` replaced by `reporting.audit.recorded.v1`, with Reporting taking the seal on `school.academic-year.closed.v1` (source: school.md open point 4; reporting.md open point 9)
- appendix-r-workflow-catalog.md WF-SCH-04 and WF-OPS-03 side effects: `operations.transport-subscription.changed.v1` changed to `operations.transport.subscription-changed.v1` (source: school.md open point 4; operations.md open point 6)
- appendix-r-workflow-catalog.md WF-WEL-01 side effects: `wellbeing.accommodation-plan.published.v1` and `assessment.exam-accommodation.applied.v1` replaced by `wellbeing.audit.recorded.v1`. The arrangement reaches Assessment as a flag and codes through the Requests exam-accommodation effect (source: wellbeing.md open point 1 and the WF-WEL-01 participation row)
- appendix-r-workflow-catalog.md WF-WEL-03 side effects: `wellbeing.medication-authorization.approved.v1` and `wellbeing.medication.missed.v1` replaced by `wellbeing.audit.recorded.v1` for the authorization, every administration, and every missed dose. The notification to the guardian is kept (source: wellbeing.md open point 1)
- appendix-r-workflow-catalog.md WF-WEL-04 side effects: `wellbeing.safeguarding-concern.raised.v1` changed to `wellbeing.safeguarding.concern-raised.v1`, the Appendix E name (source: wellbeing.md open point 1)
- appendix-r-workflow-catalog.md WF-WEL-05 side effects: `wellbeing.check-in.recorded.v1` and `wellbeing.check-in.flagged.v1` removed; `wellbeing.audit.recorded.v1` now covers every flag raised and every read (source: wellbeing.md open point 1)
- appendix-r-workflow-catalog.md WF-HR-03 side effects: `hr.document.expiring.v1` changed to `hr.staff-document.expiring.v1` at each warning threshold. `hr.document.expired.v1` replaced by `hr.audit.recorded.v1` for the expiry, the suspension and the restoration (source: hr.md open point 5)
- appendix-r-workflow-catalog.md WF-HR-04 side effects: `hr.payroll-period.frozen.v1` and `hr.payroll-input.exported.v1` replaced by `hr.payroll.inputs-ready.v1` when the inputs are assembled, plus `hr.audit.recorded.v1` for the freeze and the export (source: hr.md open point 10 and section 5.6)
- appendix-r-workflow-catalog.md WF-OPS-01 side effects: the requisition, purchase-order, goods-received and asset-registered keys replaced by `operations.audit.recorded.v1` (source: operations.md open point 6)
- appendix-r-workflow-catalog.md WF-OPS-02 side effects: `operations.loan.issued.v1` changed to `operations.library.loan-recorded.v1`, and `operations.loan.overdue.v1` to `operations.library.loan-overdue.v1`. The returned and lost keys were replaced by `operations.audit.recorded.v1` (source: operations.md open point 6 and the library row)
- appendix-r-workflow-catalog.md WF-OPS-03 side effects: `operations.route-manifest.updated.v1` replaced by `operations.audit.recorded.v1` (source: operations.md open point 6)
- appendix-r-workflow-catalog.md WF-OPS-04 side effects: `operations.maintenance-ticket.created.v1` changed to `operations.facility.ticket-raised.v1`. `operations.facility-booking.confirmed.v1` replaced by `operations.audit.recorded.v1` (source: operations.md open point 6 and the facilities row)
- appendix-r-workflow-catalog.md WF-OPS-05 side effects: the safety-incident, drill and corrective-action keys replaced by `operations.audit.recorded.v1` (source: operations.md open point 6)
- appendix-r-workflow-catalog.md WF-PRV-01 side effects: `platform.subject-request.received.v1` and `.completed.v1` replaced by `platform.audit.recorded.v1` for receipt and completion (source: platform.md open point 2)
- appendix-r-workflow-catalog.md WF-PRV-02 side effects: `documents.export.requested.v1` replaced by `documents.audit.recorded.v1` for every transition (source: documents.md open point 3)
- appendix-r-workflow-catalog.md WF-DATA-01 side effects: `documents.import.started.v1` removed. `documents.import.rolled-back.v1` replaced by a second `documents.import.completed.v1` with zero rows succeeded on rollback, and `documents.audit.recorded.v1` for every transition (source: documents.md open point 3)
- appendix-r-workflow-catalog.md WF-INF-01 side effects: `platform.upgrade.completed.v1`, `platform.upgrade.rolled-back.v1` and `platform.backup.verified.v1` replaced by `platform.audit.recorded.v1`. `platform.upgrade.started.v1` kept, now in E (source: platform.md open point 2; adr19-E)
- appendix-r-workflow-catalog.md WF-INF-02 side effects: `platform.release.deployed.v1` and `platform.release.rolled-back.v1` replaced by `platform.audit.recorded.v1` (source: platform.md open point 2)
- appendix-r-workflow-catalog.md WF-INF-03 side effects: `platform.restore-drill.completed.v1`, `platform.failover.executed.v1` and `platform.backup.verified.v1` replaced by `platform.audit.recorded.v1` (source: platform.md open point 2)

Values:
- appendix-r-workflow-catalog.md WF-PLT-03 Timeouts and escalation: the deletion cooling-off period changed from 7 days to 30 days. The daily reminder to the owner is kept, matching the Appendix C row and `DeletionCoolingOffReminderJob` (source: product-owner decision; brief-findings conflict 7 vs 30 days)
- appendix-r-workflow-catalog.md WF-IDN-01 Timeouts and escalation: checked and left unchanged. It already says the invitation expires after 14 days with one reminder at day 7 (source: product-owner decision)
- appendix-r-workflow-catalog.md: no test ids changed. A diff of every TC-id before and after is identical (the W renumbering keeps R's ids)

- appendix-s-business-rules.md BR-NOT-004 Rule: added "The window defaults to 5 minutes" (source: product-owner decision; brief-findings "Finance, Requests, Notification"; Appendix C)
- appendix-s-business-rules.md BR-NOT-004 examples: the window is now 5 minutes. Example 1 is 14:02 and 14:05, suppressed with a count of 2. Example 2 is 14:08, which is 6 minutes after the first delivery and so outside the window; it is delivered. Example 3 is 14:02 and 14:05 with different subject keys, so both are delivered (source: same)
- appendix-s-business-rules.md BR-PLT-003: checked and left unchanged. It already uses 30 days, and 2027-01-10 plus 30 days is 2027-02-09 (source: product-owner decision)
- appendix-s-business-rules.md BR-L10N-001 examples: added a fourth example with the long-vowel pair "يوسف" and "Yousef". The record matches through the English name part; with the English part empty, the cross-script key does not match (`wsf` against `sf`) (source: brief-findings "documents 23 and 24"; docs/plan/24-localization-and-calendars.md section 3 limits row and open point 3)
- appendix-s-business-rules.md BR-L10N-001 edge cases: added that cross-script matching uses a consonant key. The key is used only for duplicate detection and for admissions and directory search. It cannot see long vowels written as و or ي, so those pairs rely on the English name part, and a person confirms every duplicate under BR-ADM-006 (source: same)
- appendix-s-business-rules.md BR-FIN money-scale rule (the rule stating "2 for SAR and AED, 3 for JOD"): now names the scale as the ISO 4217 minor unit. JOD 3 was already there (source: brief-findings "documents 23 and 24"; Appendix G per adr19-KCGJF)

- appendix-m-offline-conflict-rules.md M.1 table: added the row "Record a medication administration | No". This makes explicit what U.10 had implied the other way; it follows the rule that wellbeing data is never stored on a device (source: brief-findings "From document 09"; Appendix I nurse row per adr19-BI)

- appendix-u-persona-journeys.md U.2 Day "Clears eleven approvals during assembly": Offline changed from "Queues, applies on reconnect" to "no, an approval needs the current state and permissions (Appendix M)" (source: brief-findings "From document 09"; M.1 "Approve anything: No")
- appendix-u-persona-journeys.md U.10 Day "Opens the care home": Offline changed from "Yes, the day's schedule" to "no, care data never reaches the device (Appendix M)" (source: same; M.1 "View a wellbeing record: No")
- appendix-u-persona-journeys.md U.10 Day "Records a clinic visit": Offline changed from "Queues, with the record held encrypted on device" to "no, a clinic visit is never recorded offline (Appendix M)" (source: same; M.1 "Record a clinic visit: No")
- appendix-u-persona-journeys.md U.10 Day "Administers an authorized medication": Offline changed from "Queues" to "no, signed at the moment it happens while connected (Appendix M)" (source: same; new M.1 row)

- appendix-t-year-in-the-life.md: no change needed. T lists only identifiers and names; no WF or BR was added, removed or renamed, and T states no value that changed. kit-lint R10 still passes (52 of 52 workflows, 95 of 95 rules)

- WF-ATT-01 absence-alert timing (30 minutes vs 30 seconds): open product decision; untouched. U.8 "absence alert within a minute" also left as is for the same reason.
- Tier 1 public API, OneRoster and iCal: open product decision; none of these files states it.
- Feature 39 (calendar-aware scaling): none of these five files mentions it; nothing to change.

- WF-WEL-05 is marked Offline yes, and TC-WEL-046 is an offline answer syncing. This conflicts with Appendix M ("Wellbeing data is never stored on a device"). wellbeing.md open point 8 keeps a write-only outbox exception pending the privacy officer. Left unchanged because it is an open decision, not a logged defect.
- WF-WEL-05 still names `reporting.early-warning.flag-raised.v1` "when the pattern joins other signals". reporting.md open point 8 says Reporting may hold no check-in data and does nothing for WF-WEL-05. Not logged; left unchanged.
- Consumers lose several transitions that are now audit-only: impersonation end (the web banner), year archive and reopen (Reporting "superseded" marks), and licence expiry and suspension (Academics and Scheduling). The sheets propose Appendix E keys for these (identity.md op 2, school.md op 4, hr.md op 5). Adding them is Appendix E work for a later ADR.
- Plan sheets and docs that still cite the old R names need to be aligned: identity, school, platform, assessment, wellbeing, hr, operations, documents, reporting open points.

## Follow-up: remaining test-id collisions, the job table, the PowerSchool risk

- Appendix W row 10 (Configurable without code): demo test TC-PLT-001 renumbered to TC-PLT-805. TC-PLT-001 keeps its Appendix R meaning (WF-PLT-01 Requested to Validated). A whole-repo grep (docs and tools) found no TC-PLT-805 before this change.
- Appendix W row 40 (Self-healing operations): demo test TC-INF-002 renumbered to TC-INF-801. Appendix R uses TC-INF-002 for WF-INF-01 PreChecked to Aborted. A whole-repo grep found no TC-INF-8xx before this change. W cites no other TC-INF id.
- Appendix W "Test identifiers" paragraph: now lists features 10 and 40 among the renumbered ones and states that TC-PLT-001 to TC-PLT-005 and TC-INF-001 to TC-INF-006 keep their Appendix R meaning.
- Appendix O, reserve R-17 (feature 40): TC-INF-002 changed to TC-INF-801. Appendix O cites no other renumbered id. Appendix P cites neither id, so it was not changed.
- Appendix E jobs table: added Uncovered-period escalation (`UncoveredPeriodEscalationJob`, Scheduling, every 5 minutes from 06:00 to the last period on school days).
- Appendix E jobs table: added Joining timeouts (`JoiningTimeoutsJob`, Identity, every 15 minutes).
- Appendix E jobs table: added Deletion cooling-off reminder (`DeletionCoolingOffReminderJob`, Platform, daily 09:00 per tenant time zone).
- Appendix E jobs table: added API key expiry reminder (`ApiKeyExpiryReminderJob`, Platform, daily 08:00 per tenant time zone).
- Appendix E jobs table: added Sandbox lifecycle (`SandboxLifecycleJob`, Platform, daily 04:00 UTC).
- Appendix E jobs table: added Support SLA (`SupportSlaJob`, Platform, every 5 minutes).
- Appendix E jobs table: added Subject request deadline (`SubjectRequestDeadlineJob`, Platform, daily 08:00 per tenant time zone).
- Appendix E jobs table: added Daily digest of blocked attempts (Ai, daily per tenant). The Ai sheet names no job class, so the Appendix C name and the daily cadence it implies are used.
- Appendix E jobs table: added Digest builder (`DigestSchedulerJob`, Notification, every 15 minutes).
- Appendix E jobs table: added Re-enrollment window (`ReEnrollmentWindowJob`, Admissions, daily 08:00 campus time). This is the job that actually sends re-enrollment reminders.
- Master brief Section 40: added risk 13. PowerSchool's Middle East and Africa Arabic edition narrows the Arabic-first difference. Rated Medium likelihood and High impact. Owned by the product owner. The mitigation comes from docs/plan/02-competitive-gap-analysis.md: sell bilingual data rather than a translated interface, show the sixty-second proofs for features 26, 27, 29, 31 and 32, and resolve the unverified PowerSchool cells before general availability.
- Master brief Section 40 intro: "These twelve" changed to "These thirteen".
- kit-lint: `node tools/kit-lint/kit-lint.mjs .` prints "kit-lint: clean."


- Appendix C row "Re-enrollment opened, reminder" names "job: reminder ladder". That is the Finance job, not Admissions' `ReEnrollmentWindowJob`. The row should be relabelled.
- docs/plan/32-product-differentiation-and-demo.md still cites TC-PLT-001 (feature 10) and TC-INF-002 (feature 40) with the old Appendix W meaning.
- docs/plan/18-risk-register.md line 50 still says "The twelve risks in master brief Section 40". It needs the new risk 13.
- appendix-c-notifications.md row "Re-enrollment opened, reminder": trigger job relabelled from the Finance reminder ladder to Admissions' re-enrollment window job (source: adr19-LEFT follow-up)

