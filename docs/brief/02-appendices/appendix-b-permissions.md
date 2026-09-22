# Appendix B. Permission Catalog

> Part of `docs/brief/02-appendices/`. Normative. Read with the master brief and the reference architecture.

**Naming.** `<service>.<resource>.<action>`, for example `assessment.marks.approve`. The service segment is always a real service from Appendix L. Earlier versions used three namespaces that were not services; they are corrected here and the old forms are defects:

| Old | Correct | Why |
|---|---|---|
| `students.*` | `school.students.*` | The School service owns students |
| `safety.*` | `attendance.safety.*` | Safety is part of the Attendance service |
| `admin.*` | `platform.jobs.*`, `platform.failed-messages.*`, `platform.recycle-bin.*`, `platform.retention.*`, `audit.*` | Administration is split across the services that own each concern |

**Standard actions** on every resource unless stated otherwise: `view`, `create`, `edit`, `delete`, `export`. **Special actions** are listed per resource and are the ones that carry risk.

**Every permission carries** a plain-language title and description in English and Arabic, a **risk level**, and the data scopes it supports. The generated permission-matrix test suite reads this catalog, so a permission that is not here cannot be granted and an endpoint that declares one that is not here fails the build.

**Risk levels.** `normal` is routine. `elevated` needs a reason recorded and appears in the audit viewer's default filter. `high` needs **four-eyes approval to grant**, is time-limited by default, and is reviewed in every access-review campaign.

**Data scopes**, in narrowing order: `all-tenant`, `campus`, `stage`, `department`, `own-sections`, `own-homeroom`, `own-children`, `self`. A permission is granted with exactly one scope. The evaluator takes the narrowest scope that applies when a user holds a permission through more than one role.

---

## Platform

| Resource | Standard actions | Special actions | Highest risk |
|---|---|---|---|
| `platform.tenants` | view, create, edit | `provision`, `suspend`, `reactivate`, `delete` (high), `export` (high, reason required, never plan-gated, BR-PLT-006) | high |
| `platform.plans` | view, create, edit | `assign` | normal |
| `platform.subscriptions` | view, edit | `change-plan`, `waive-charge` (elevated) | elevated |
| `platform.feature-flags` | view, edit | `rollout` | normal |
| `platform.modules` | view, edit | `enable`, `disable` (elevated) | elevated |
| `platform.branding` | view, edit | `publish` | normal |
| `platform.settings` | view, edit | `reset-to-default` (elevated) | elevated |
| `platform.terminology` | view, edit | none | normal |
| `platform.custom-fields` | view, create, edit, delete | none | normal |
| `platform.template-library` | view, create, edit | `publish`, `import` | normal |
| `platform.integrations` | view, create, edit, delete | `rotate-secret`, `replay-webhook` (elevated) | elevated |
| `platform.api-keys` | view, create, delete | `reveal-once` (high) | high |
| `platform.jobs` | view | `cancel`, `retry` (elevated), `replay` (elevated) | elevated |
| `platform.failed-messages` | view | `replay`, `discard` (elevated) | elevated |
| `platform.recycle-bin` | view | `restore` (elevated), `purge` (high) | high |
| `platform.retention` | view, edit | `run-now` (high), `place-legal-hold` (high) | high |
| `platform.support` | view, create, edit | `impersonate` (high, consent required, banner shown, fully logged) | high |
| `platform.announcements` | view, create, edit, delete | `publish` | normal |

## Identity

| Resource | Standard actions | Special actions | Highest risk |
|---|---|---|---|
| `identity.users` | view, create, edit, delete, export | `invite`, `activate`, `suspend`, `reset-password`, `force-signout`, `merge` (elevated) | elevated |
| `identity.roles` | view, create, edit, delete | `clone`, `assign-role`, `grant-high-risk` (high) | high |
| `identity.permissions` | view | `explain-effective` | normal |
| `identity.invitations` | view, create, delete | `resend`, `revoke` | normal |
| `identity.join-requests` | view | `approve`, `reject` | normal |
| `identity.join-codes` | view, create, delete | `rotate`, `revoke` | normal |
| `identity.sessions` | view | `revoke` (elevated) | elevated |
| `identity.api-keys` | view, create, delete | none | elevated |
| `identity.access-reviews` | view, create | `certify`, `revoke-access` | elevated |
| `identity.security-policy` | view, edit | none | high |
| `identity.delegation` | view, create, delete | `delegate` | elevated |
| `identity.break-glass` | none | `use` (high, reason required, principal alerted, fully logged) | high |

## School

| Resource | Standard actions | Special actions | Highest risk |
|---|---|---|---|
| `school.profile` | view, edit | none | normal |
| `school.campuses` | view, create, edit, delete | none | normal |
| `school.departments` | view, create, edit, delete | none | normal |
| `school.houses` | view, create, edit, delete | none | normal |
| `school.rooms` | view, create, edit, delete | none | normal |
| `school.academic-years` | view, create, edit | `close-year` (high), `reopen-year` (high) | high |
| `school.terms` | view, create, edit | none | normal |
| `school.grade-levels` | view, create, edit, delete | none | normal |
| `school.sections` | view, create, edit, delete | `balance-formation` | normal |
| `school.subjects` | view, create, edit, delete | none | normal |
| `school.students` | view, create, edit, delete, export | `change-status`, `promote`, `merge` (elevated), `view-sensitive` (elevated), `print-id-cards` | elevated |
| `school.guardians` | view, create, edit, delete, export | `link`, `unlink` (elevated) | elevated |
| `school.custody` | view, edit | none. Sensitive: every read logged | high |
| `school.medical-summary` | view, edit | none. Sensitive: every read logged | high |
| `school.staff` | view, create, edit, delete, export | none | normal |
| `school.numbering` | view, edit | none | elevated |

## Admissions

| Resource | Standard actions | Special actions |
|---|---|---|
| `admissions.campaigns` | view, create, edit, delete | `open`, `close` |
| `admissions.inquiries` | view, create, edit, delete, export | `convert` |
| `admissions.applications` | view, create, edit, export | `decide`, `score`, `override-age` (elevated, reason required, BR-ADM-001) |
| `admissions.assessments` | view, create, edit | `schedule`, `evaluate` |
| `admissions.offers` | view, create, export | `make`, `withdraw`, `extend-expiry` (elevated) |
| `admissions.waiting-list` | view, edit | `promote`, `reorder` (elevated) |
| `admissions.capacity` | view, edit | `override-capacity` (elevated) |
| `admissions.enrollment` | view | `enroll` |
| `admissions.re-enrollment` | view, edit, export | `open-campaign`, `close-campaign` |

## Academics

| Resource | Standard actions | Special actions |
|---|---|---|
| `academics.curriculum` | view, create, edit, delete | `map-standards` |
| `academics.teaching-assignments` | view, create, edit, delete | none |
| `academics.student-groups` | view, create, edit, delete | none |
| `academics.lesson-plans` | view, create, edit, delete | `review`, `approve` |
| `academics.assignments` | view, create, edit, delete | `publish`, `extend-due-date` |
| `academics.rubrics` | view, create, edit, delete | none |
| `academics.submissions` | view, edit | `grade`, `return`, `accept-late` |
| `academics.question-bank` | view, create, edit, delete, export | `import-qti`, `export-qti` |
| `academics.quizzes` | view, create, edit, delete | `publish`, `release-results` |
| `academics.resources` | view, create, edit, delete | none |

## Assessment

| Resource | Standard actions | Special actions | Highest risk |
|---|---|---|---|
| `assessment.structures` | view, create, edit, delete | none | normal |
| `assessment.schemes` | view, create, edit | `version` | elevated |
| `assessment.marks` | view, export | `enter`, `moderate`, `approve`, `publish`, `lock`, `unlock` (high), `change-after-lock` (high) | high |
| `assessment.report-cards` | view, export | `design-template`, `generate`, `publish`, `reissue` (elevated) | elevated |
| `assessment.transcripts` | view, export | `issue` | elevated |
| `assessment.grade-changes` | view, create | `approve`, `reject` | elevated |
| `assessment.exams` | view, create, edit, delete | `seat`, `assign-invigilators`, `approve-paper` (elevated), `print-paper` (elevated) | elevated |

## Scheduling

| Resource | Standard actions | Special actions |
|---|---|---|
| `scheduling.periods` | view, create, edit, delete | none |
| `scheduling.bell-schedules` | view, create, edit, delete | `activate` |
| `scheduling.constraints` | view, create, edit, delete | none |
| `scheduling.timetable` | view, edit, export | `generate`, `publish`, `override-conflict` (elevated), `lock-slot` |
| `scheduling.substitutions` | view, create, edit | `assign`, `accept-cover` |
| `scheduling.calendar` | view, create, edit, delete | `publish` |
| `scheduling.room-bookings` | view, create, edit, delete | `approve` |
| `scheduling.exam-timetable` | view, edit, export | `generate`, `publish` |

## Attendance

| Resource | Standard actions | Special actions | Highest risk |
|---|---|---|---|
| `attendance.student-attendance` | view, export | `mark`, `edit-after-lock` (elevated), `bulk-mark`, `nudge` | elevated |
| `attendance.staff-attendance` | view, export | `mark`, `edit-after-lock` (elevated) | elevated |
| `attendance.excuses` | view, create | `approve`, `reject`, `view-medical-detail` (high). Sensitive: every read logged | high |
| `attendance.thresholds` | view, create, edit, delete | none | normal |
| `attendance.safety.pickup-persons` | view, create, edit, delete | `verify` | elevated |
| `attendance.safety.gate-passes` | view | `issue`, `verify`, `revoke` | elevated |
| `attendance.safety.visitors` | view, create, edit, export | `check-in`, `check-out`, `manage-watchlist` (high) | high |
| `attendance.safety.emergency` | view | `broadcast` (high), `run-roll-call`, `reunify` | high |

## Finance

| Resource | Standard actions | Special actions | Highest risk |
|---|---|---|---|
| `finance.fee-items` | view, create, edit, delete | none | normal |
| `finance.structures` | view, create, edit, delete | none | normal |
| `finance.plans` | view, create, edit | `assign`, `change-mid-year` (elevated) | elevated |
| `finance.payers` | view, create, edit | `view-bank-details` (high) | high |
| `finance.invoices` | view, create, export | `run-batch`, `post` (elevated), `reverse` (high) | high |
| `finance.payments` | view, create, export | `record`, `allocate`, `mark-bounced` | elevated |
| `finance.refunds` | view, create | `approve` (high) | high |
| `finance.credit-notes` | view, create | `issue` (elevated) | elevated |
| `finance.write-offs` | view, create | `approve` (high) | high |
| `finance.discounts` | view, create, edit, delete | `approve-above-limit` (elevated) | elevated |
| `finance.scholarships` | view, create, edit | `award` (elevated) | elevated |
| `finance.restrictions` | view, edit | `apply`, `lift` (elevated) | elevated |
| `finance.cashier` | view | `open-shift`, `close-day` | elevated |
| `finance.reports` | view, export | none | normal |

## Communication

| Resource | Standard actions | Special actions | Highest risk |
|---|---|---|---|
| `communication.announcements` | view, create, edit, delete | `publish`, `require-acknowledgment` | normal |
| `communication.news` | view, create, edit, delete | `publish` | normal |
| `communication.messages` | view, create | `moderate`, `oversee-messages` (high, every read logged), `export-for-safeguarding` (high) | high |
| `communication.concerns` | view, create | none. `create` never records the reporter; `view` is high, reason required, every read logged | high |
| `communication.meetings` | view, create, edit, delete | `open-slots`, `book-on-behalf` | normal |
| `communication.surveys` | view, create, edit, delete, export | `publish`, `close` | normal |
| `communication.policies` | view, create, edit | `publish`, `chase-acknowledgment` | normal |

## Notification

| Resource | Standard actions | Special actions |
|---|---|---|
| `notification.templates` | view, create, edit, delete | `send-test`, `publish` |
| `notification.preferences` | view, edit | none |
| `notification.delivery-log` | view, export | `resend` |
| `notification.channels` | view, edit | `enable`, `disable` |

## Requests

| Resource | Standard actions | Special actions | Highest risk |
|---|---|---|---|
| `requests.types` | view, create, edit, delete | `design`, `publish-version` | elevated |
| `requests.forms` | view, create, edit, delete | none | normal |
| `requests.chains` | view, create, edit, delete | none | elevated |
| `requests.requests` | view, create, export | `submit-on-behalf`, `approve`, `reject`, `reassign`, `override` (elevated), `withdraw` | elevated |
| `requests.tasks` | view, create, edit | `assign`, `complete` | normal |
| `requests.duty-rosters` | view, create, edit, delete | `publish`, `assign` | normal |
| `requests.sla` | view, edit | none | normal |

## Documents

| Resource | Standard actions | Special actions | Highest risk |
|---|---|---|---|
| `documents.files` | view, create, delete, export | `download`, `share-link` (elevated) | elevated |
| `documents.templates` | view, create, edit, delete | `publish` | normal |
| `documents.certificates` | view, create, export | `generate`, `revoke` (elevated) | elevated |
| `documents.imports` | view, create | `dry-run`, `commit`, `rollback` (elevated) | elevated |
| `documents.exports` | view, create | `export-sensitive` (high, reason required, watermarked, principal notified) | high |

## Behavior

| Resource | Standard actions | Special actions |
|---|---|---|
| `behavior.categories` | view, create, edit, delete | none |
| `behavior.incidents` | view, create, edit, export | `view-restricted` (elevated), `assign-consequence` |
| `behavior.points` | view, create | `award`, `revoke` |
| `behavior.badges` | view, create, edit | `award`, `export-open-badge` |

## Reporting

| Resource | Standard actions | Special actions |
|---|---|---|
| `reporting.dashboards` | view | none |
| `reporting.reports` | view, create, edit, delete, export | `share`, `schedule` |
| `reporting.early-warning` | view | `explain`, `open-intervention` |
| `reporting.data-quality` | view | `resolve`, `run-rules` |
| `reporting.projections` | view | `rebuild` (elevated) |

## Audit

| Resource | Standard actions | Special actions | Highest risk |
|---|---|---|---|
| `audit.entries` | view | `export` (high, reason required) | high |
| `audit.login-history` | view, export | none | elevated |
| `audit.access-log` | view | none. Reads of sensitive records | high |
| `audit.integrity` | view | `verify` | elevated |
| `audit.access-transparency` | view | none. Roles and times of reads of the holder's own or own children's records, never reader names; granted in the own-children or self scope only | normal |

## Wellbeing

Every resource here is sensitive. Every read is logged, and none appears in general search or in Student 360 without an explicit permission.

| Resource | Standard actions | Special actions | Highest risk |
|---|---|---|---|
| `wellbeing.clinic-visits` | view, create, edit | `notify-guardian`, `send-home` | high |
| `wellbeing.medications` | view, create | `authorize`, `administer` | high |
| `wellbeing.counseling-cases` | view, create, edit | none | high |
| `wellbeing.safeguarding` | view, create, edit | `escalate`, `close` | high |
| `wellbeing.education-plans` | view, create, edit | `approve`, `apply-accommodation` | elevated |
| `wellbeing.interventions` | view, create, edit | `open`, `close` | elevated |
| `wellbeing.break-glass` | none | `use` (high, reason required, principal alerted) | high |

## Hr

| Resource | Standard actions | Special actions | Highest risk |
|---|---|---|---|
| `hr.staff-files` | view, create, edit, export | none | elevated |
| `hr.contracts` | view, create, edit | `approve` | high |
| `hr.leave` | view, create, edit | `approve`, `reject`, `adjust-balance` (elevated) | elevated |
| `hr.payroll` | view, edit, export | `view-salary` (high), `prepare-inputs`, `generate-payslips` | high |
| `hr.appraisals` | view, create, edit | `observe`, `finalize` | elevated |
| `hr.vacancies` | view, create, edit, delete | `publish`, `make-offer` | elevated |

## Operations

| Resource | Standard actions | Special actions |
|---|---|---|
| `operations.library` | view, create, edit, delete, export | `issue`, `receive`, `waive-fine` (elevated) |
| `operations.transport` | view, create, edit, delete, export | `assign-student`, `record-boarding`, `notify-delay` |
| `operations.inventory` | view, create, edit, delete, export | `issue`, `receive`, `stock-take` |
| `operations.facilities` | view, create, edit | `approve`, `close-ticket` |
| `operations.frontdesk` | view, create, edit, export | `check-in`, `resolve-complaint` |
| `operations.activities` | view, create, edit, delete | `enroll`, `collect-consent` |

## Ai

| Resource | Standard actions | Special actions |
|---|---|---|
| `ai.assistant` | view | `use` |
| `ai.drafting` | view | `use`, `accept-draft` |
| `ai.configuration` | view, edit | `configure`, `set-provider` (elevated) |
| `ai.usage` | view, export | none |

---

## Rules the catalog enforces

1. **Dependency.** `edit` requires `view`; `approve` requires `view`; `delete` requires `edit`. The interface shows the dependency and grants it automatically rather than failing later.
2. **Four-eyes.** Granting any `high` permission needs a second approver who is not the requester. The grant is time-limited unless someone explicitly makes it permanent, and that choice is audited.
3. **Live application.** A role change publishes `identity.permissions.changed.v1`, which invalidates the permission cache everywhere within seconds and tells open clients to refresh. Nobody signs out.
4. **Server-side only.** The interface reflects permissions; it never enforces them. Every endpoint declares its permission, and the generated suite proves each role can do exactly what this catalog says and nothing more.
5. **Sensitive resources log reads.** `school.custody`, `school.medical-summary`, `attendance.excuses.view-medical-detail`, `finance.payers.view-bank-details`, `communication.concerns.view`, every `wellbeing.*`, `audit.access-log` and `communication.messages` under oversight write an access-log entry on read, not only on change.
6. **No wildcard grants.** There is no "all permissions" permission. The platform super administrator holds an explicit set, which is why it can be reviewed.
