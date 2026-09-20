# Appendix K. Error Code Catalog

> Part of `docs/brief/02-appendices/`. Normative. Read with the master brief and the reference architecture.

Format `<SERVICE>_<MEANING>` in upper snake case, as fixed by Appendix L. The service part is the long service name in upper case, so the Requests service emits `REQUESTS_*` and never `RQS_*`; `RQS` is an AREA code for identifiers, not an error prefix. Every error response carries the code, a correlation id, the tenant id and, for validation failures, a field list. **Parent-safe** means the code's message may be shown verbatim to a guardian or a student on the mobile app; anything else is shown as a generic message with a reference number, and the detail goes to the log.

---

## K.1 Cross-cutting codes

Every service emits these eight with its own prefix. They are listed once and are not repeated per service below.

| Code suffix | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `_VALIDATION_FAILED` | 400 | A field fails a format, range or required check | Bind the field list to the form, focus the first field, do not retry | yes |
| `_PERMISSION_DENIED` | 403 | The caller lacks the permission or the data scope | Hide the action and refresh the permission version; never retry | yes, as "you do not have access to this" |
| `_TENANT_MISMATCH` | 403 | The subject belongs to another tenant | Sign the session out of the wrong tenant context and report; never retry | no |
| `_NOT_FOUND` | 404 | The entity does not exist, or the caller may not know it exists | Return to the list and refresh | yes |
| `_CONCURRENCY_CONFLICT` | 409 | The row version changed since it was read | Re-read, show what changed, let the user merge; never silently overwrite | yes |
| `_IDEMPOTENCY_REPLAY` | 200 | The same idempotency key was seen before | Treat as success and use the returned original result | yes |
| `_RATE_LIMITED` | 429 | Too many calls for this caller, tenant or endpoint | Back off using `Retry-After` with jitter | yes |
| `_DEPENDENCY_UNAVAILABLE` | 503 | A downstream service, broker, database or provider is down or circuit-open | Queue the work offline if the screen supports it, otherwise retry with backoff | yes, as "temporarily unavailable" |

---

## K.2 Identity

| Code | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `IDENTITY_CREDENTIALS_INVALID` | 401 | Wrong user name or password | Show one generic message; never say which part was wrong | yes |
| `IDENTITY_ACCOUNT_LOCKED` | 423 | Lockout threshold reached | Show the unlock time and the self-service path | yes |
| `IDENTITY_TWO_FACTOR_REQUIRED` | 401 | Password accepted, second factor missing | Open the second-factor step with the enrolled method | yes |
| `IDENTITY_TOKEN_EXPIRED` | 401 | Access token past its 15-minute life | Refresh once, then sign in again | yes |
| `IDENTITY_REFRESH_TOKEN_REUSED` | 401 | A rotated refresh token was presented twice | Destroy the local session, force sign-in, alert the user's devices | no |
| `IDENTITY_PERMISSION_VERSION_STALE` | 409 | The token's permission version is behind the server | Re-fetch the permission set and retry once | yes |
| `IDENTITY_JOIN_CODE_INVALID` | 400 | Join code unknown, expired or already used | Offer to request a new invitation | yes |
| `IDENTITY_GUARDIAN_LINK_UNVERIFIED` | 403 | The guardian is not yet verified against the child | Show the verification step and the school contact | yes |
| `IDENTITY_FOUR_EYES_REQUIRED` | 409 | A high-risk grant needs a second approver | Submit the grant as a proposal and show who can approve | no |
| `IDENTITY_SELF_APPROVAL_REFUSED` | 403 | The proposer tried to approve their own grant | Show the approver list; never offer a bypass | no |
| `IDENTITY_DELEGATION_EXPIRED` | 403 | The delegation that carried this permission has passed its end date | Show who now holds the permission and offer to request a new delegation | yes |
| `IDENTITY_LAST_SUPER_ADMIN` | 409 | The action would remove the last platform super administrator | Refuse, and name the other accounts that could take the role first | no |

## K.3 Platform

| Code | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `PLATFORM_TENANT_SUSPENDED` | 403 | The tenant is in read-only suspension | Switch the app to read-only and show the export path | yes |
| `PLATFORM_PLAN_LIMIT_REACHED` | 402 | Students, storage, SMS credits or AI usage over the plan | Show the limit, the current usage and the upgrade action | no |
| `PLATFORM_FEATURE_DISABLED` | 403 | A feature flag is off for this tenant | Hide the entry point entirely on next load | yes |
| `PLATFORM_PROVISIONING_IN_PROGRESS` | 409 | The tenant is still being created | Poll the provisioning status; do not resubmit | no |
| `PLATFORM_IMPERSONATION_NOT_CONSENTED` | 403 | Impersonation attempted without recorded tenant consent | Show the consent request flow; never proceed | no |
| `PLATFORM_SETTING_LOCKED_BY_POLICY` | 409 | A setting is fixed by the plan or by country policy | Show the value and why it cannot change | no |
| `PLATFORM_WEBHOOK_ENDPOINT_UNREACHABLE` | 502 | An outgoing webhook target failed repeatedly | Show the failure count and the replay action | no |
| `PLATFORM_JOB_REPLAY_REFUSED` | 409 | A failed job is not in a replayable state | Show the job state and the manual path | no |
| `PLATFORM_RESIDENCY_VIOLATION` | 403 | The call would move data outside the tenant's region | Block and report; never retry | no |

## K.4 School

| Code | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `SCHOOL_ACADEMIC_YEAR_CLOSED` | 409 | A write targets a closed year | Show the year state and the reopen request path | yes |
| `SCHOOL_SECTION_CAPACITY_EXCEEDED` | 409 | The section is full | Offer the waiting list or another section | yes |
| `SCHOOL_STUDENT_NUMBER_IN_USE` | 409 | Numbering collision on enrolment | Take the next number from the series and retry once | no |
| `SCHOOL_STUDENT_STATUS_TRANSITION_INVALID` | 409 | The status change is not allowed from the current state | Show the allowed transitions | no |
| `SCHOOL_GUARDIAN_REQUIRED` | 400 | A student record has no contactable guardian | Open the guardian form; block enrolment until fixed | yes |
| `SCHOOL_SENSITIVE_FIELD_ACCESS_DENIED` | 403 | A read of custody or medical fields without `view-sensitive` | Show the masked value and the request-access path | yes |
| `SCHOOL_MERGE_CONFLICT` | 409 | Two student records disagree on a field during merge | Show a side-by-side chooser for the conflicting fields | no |
| `SCHOOL_PROMOTION_BLOCKED` | 409 | Promotion blocked by unpublished results or unsettled clearance | List the blocking items with links | no |
| `SCHOOL_CAMPUS_TRANSFER_BLOCKED` | 409 | The target campus has no seat or no matching stage | Show seat counts by grade | no |

## K.5 Admissions

| Code | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `ADMISSIONS_APPLICATION_STAGE_INVALID` | 409 | The requested stage move is not allowed | Show the current stage and its allowed moves | no |
| `ADMISSIONS_DOCUMENT_MISSING` | 400 | A required document is not uploaded | List the missing documents with upload actions | yes |
| `ADMISSIONS_OFFER_EXPIRED` | 409 | Acceptance arrived after the offer deadline | Show the expiry and the extension request path | yes |
| `ADMISSIONS_SEAT_UNAVAILABLE` | 409 | No seat in the requested grade | Offer the waiting list with the position | yes |
| `ADMISSIONS_CAPACITY_OVERRIDE_REQUIRED` | 403 | Enrolment beyond capacity without the elevated permission | Route to the registrar for an override | no |
| `ADMISSIONS_DUPLICATE_APPLICANT` | 409 | Same child detected in an existing application | Show the match and the merge action | no |
| `ADMISSIONS_ASSESSMENT_NOT_SCHEDULED` | 409 | A decision was attempted before the entrance assessment | Show the scheduling action | yes |
| `ADMISSIONS_REENROLLMENT_BLOCKED_BY_BALANCE` | 409 | Outstanding balance blocks re-enrolment | Show the balance and the pay action | yes |
| `ADMISSIONS_WINDOW_CLOSED` | 409 | The admission window for the year is shut | Show the next window dates | yes |

## K.6 Academics

| Code | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `ACADEMICS_SUBMISSION_WINDOW_CLOSED` | 409 | A submission arrives after the deadline | Show the deadline and the late-submission policy | yes |
| `ACADEMICS_SUBMISSION_TOO_LARGE` | 413 | Attachment above the tenant's size limit | Show the limit and offer compression | yes |
| `ACADEMICS_TEACHING_ASSIGNMENT_MISSING` | 403 | The teacher is not assigned to that section and subject | Show the coordinator contact | no |
| `ACADEMICS_HOMEWORK_LOAD_EXCEEDED` | 409 | Publishing would break the day's homework ceiling | Show the day's load and offer another date | no |
| `ACADEMICS_LESSON_PLAN_NOT_REVIEWED` | 409 | Publishing before review where the school requires it | Show the reviewer and the pending state | no |
| `ACADEMICS_QTI_IMPORT_INVALID` | 400 | A QTI 3 package fails schema validation | Show the failing item references | no |
| `ACADEMICS_ONLINE_EXAM_ALREADY_STARTED` | 409 | An edit to a live exam | Block the edit and offer a new version | no |
| `ACADEMICS_CURRICULUM_STANDARD_UNKNOWN` | 400 | A CASE standard reference does not resolve | Show the standards picker | no |
| `ACADEMICS_RESUBMISSION_NOT_ALLOWED` | 409 | Resubmission after the teacher started grading | Show the grading state | yes |

## K.7 Assessment

| Code | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `ASSESSMENT_MARKS_LOCKED` | 409 | Entry after the mark window locked | Offer the grade-change request path | no |
| `ASSESSMENT_MARK_OUT_OF_RANGE` | 400 | A mark exceeds the structure maximum | Highlight the cell and show the maximum | no |
| `ASSESSMENT_MODERATION_REQUIRED` | 409 | Approval attempted before moderation completed | Show the moderation queue position | no |
| `ASSESSMENT_APPROVAL_CHAIN_INCOMPLETE` | 409 | Publishing before every approver signed | List the outstanding approvers | no |
| `ASSESSMENT_SCHEME_VERSION_MISMATCH` | 409 | A recalculation used a different grading scheme version | Recompute with the stored version and show both | no |
| `ASSESSMENT_REPORT_CARD_BATCH_RUNNING` | 409 | A second batch started for the same term | Show progress of the running batch | no |
| `ASSESSMENT_REPORT_CARD_TEMPLATE_INVALID` | 400 | A template placeholder does not resolve | Show the unresolved placeholders | no |
| `ASSESSMENT_POST_LOCK_CHANGE_REFUSED` | 403 | A post-lock change without the high-risk permission | Route to the grade-appeal workflow | no |
| `ASSESSMENT_TRANSCRIPT_NOT_REPRODUCIBLE` | 500 | A closed year recomputes to a different number | Block the document and raise a data-quality issue | no |

## K.8 Scheduling

| Code | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `SCHEDULING_TIMETABLE_CONFLICT` | 409 | Teacher, room or section double-booked | Show both sides of the clash and the resolve action | no |
| `SCHEDULING_SOLVER_INFEASIBLE` | 422 | Constraints cannot all be satisfied | Show the smallest conflicting constraint set | no |
| `SCHEDULING_SOLVER_TIMEOUT` | 504 | The solver exceeded its budget | Offer the best partial solution and a longer run | no |
| `SCHEDULING_PERIOD_OUTSIDE_BELL_SCHEDULE` | 400 | A period falls outside the day's bell schedule | Show the schedule and snap to a valid slot | no |
| `SCHEDULING_ROOM_UNSUITABLE` | 409 | The room lacks a required facility | List rooms that qualify | no |
| `SCHEDULING_SUBSTITUTE_UNAVAILABLE` | 409 | No ranked substitute is free | Widen the criteria or escalate to the coordinator | no |
| `SCHEDULING_PUBLISHED_TIMETABLE_IMMUTABLE` | 409 | A direct edit to a published timetable | Route through the change workflow so parents are notified | no |
| `SCHEDULING_EXAM_CLASH` | 409 | A student has two exams in one slot | Show the affected students and the reschedule action | no |
| `SCHEDULING_BOOKING_OUTSIDE_WINDOW` | 409 | A meeting booked outside the parent booking window | Show the open window | yes |

## K.9 Attendance

| Code | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `ATTENDANCE_SESSION_LOCKED` | 409 | Marking after the session cut-off | Offer the edit-after-lock request | no |
| `ATTENDANCE_SESSION_NOT_SCHEDULED` | 409 | Marking a session the timetable does not have | Refresh the timetable and raise a data-quality issue | no |
| `ATTENDANCE_DUPLICATE_MARK` | 200 | An offline queue replays the same mark | Accept as success and keep the server value | yes |
| `ATTENDANCE_OFFLINE_CONFLICT` | 409 | Two devices marked the same student differently | Show both values with times and let the teacher choose | no |
| `ATTENDANCE_STUDENT_NOT_IN_SECTION` | 400 | The student moved section since the device synced | Refresh the roster and drop the stale entry | no |
| `ATTENDANCE_EXCUSE_EVIDENCE_REQUIRED` | 400 | An excuse type that needs a document has none | Open the upload step | yes |
| `ATTENDANCE_EXCUSE_WINDOW_PASSED` | 409 | An excuse submitted after the allowed days | Show the window and the appeal path | yes |
| `ATTENDANCE_GATE_PASS_INVALID` | 403 | QR or PIN unknown, expired or already used | Show the manual verification path at the desk | yes |
| `ATTENDANCE_PICKUP_PERSON_NOT_AUTHORIZED` | 403 | The person at the gate is not on the list | Hold the child and call the guardian; never release | yes |
| `ATTENDANCE_BROADCAST_NOT_PERMITTED` | 403 | Emergency broadcast without the high-risk permission | Route to the principal or the safety officer | no |

## K.10 Finance

| Code | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `FINANCE_INVOICE_ALREADY_POSTED` | 409 | An edit to a posted invoice | Offer a credit note instead | yes |
| `FINANCE_PAYMENT_EXCEEDS_BALANCE` | 400 | Payment above the outstanding amount, for example 3,200.00 SAR against 2,750.00 SAR | Offer to post the excess as credit on account | yes |
| `FINANCE_CURRENCY_MISMATCH` | 400 | A payment in AED against an invoice series in SAR | Block and show the invoice currency | yes |
| `FINANCE_DAY_ALREADY_CLOSED` | 409 | A posting dated into a closed cashier day | Post to the next open day with a note | no |
| `FINANCE_DAY_CLOSE_OUT_OF_BALANCE` | 409 | The day-close reconciliation does not balance | List the documents that differ; block the close | no |
| `FINANCE_REFUND_APPROVAL_REQUIRED` | 403 | A refund without the high-risk permission | Route to the approver with the amount and reason | no |
| `FINANCE_WRITE_OFF_APPROVAL_REQUIRED` | 403 | A write-off without the high-risk permission | Route to the approver | no |
| `FINANCE_CHEQUE_ALREADY_BOUNCED` | 409 | A second bounce entry for one cheque | Show the existing bounce record | no |
| `FINANCE_GATEWAY_DECLINED` | 402 | The payment provider declined the card | Show the provider's plain reason and offer another method | yes |
| `FINANCE_INVOICE_RUN_IN_PROGRESS` | 409 | A second run for the same term and plan | Show the running batch and its progress | no |

## K.11 Communication

| Code | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `COMMUNICATION_RECIPIENT_NOT_ALLOWED` | 403 | The tenant's messaging policy forbids this pair | Show who the sender may message | yes |
| `COMMUNICATION_QUIET_HOURS` | 409 | A non-urgent message inside quiet hours | Offer to schedule for the next allowed time | yes |
| `COMMUNICATION_ATTACHMENT_REJECTED` | 400 | Type, size or scan verdict rejected the file | Show the allowed types and the size limit | yes |
| `COMMUNICATION_THREAD_CLOSED` | 409 | A reply to a closed thread | Offer to open a new thread | yes |
| `COMMUNICATION_MESSAGE_REPORTED_LOCK` | 423 | The thread is locked pending safeguarding review | Show the neutral notice; no detail | yes |
| `COMMUNICATION_TRANSLATION_UNAVAILABLE` | 503 | Machine translation is down | Show the original and mark translation unavailable | yes |
| `COMMUNICATION_ACKNOWLEDGMENT_ALREADY_RECORDED` | 200 | The same acknowledgment arrives twice | Treat as success | yes |
| `COMMUNICATION_AUDIENCE_EMPTY` | 400 | An announcement resolves to no recipients | Show the audience builder with the filter that emptied it | no |
| `COMMUNICATION_OVERSIGHT_REASON_REQUIRED` | 400 | Oversight read attempted with no reason recorded | Require the reason field before the read | no |

## K.12 Notification

| Code | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `NOTIFICATION_TEMPLATE_NOT_FOUND` | 404 | The template key or language variant is missing | Fall back to the default language and raise a defect | no |
| `NOTIFICATION_CHANNEL_DISABLED` | 409 | The recipient turned that channel off | Use the next channel in the fallback ladder | yes |
| `NOTIFICATION_NO_REACHABLE_CHANNEL` | 422 | No channel remains for this recipient | Surface in the Data Quality Center as a missing contact | no |
| `NOTIFICATION_DEVICE_TOKEN_INVALID` | 410 | The push token is no longer registered | Delete the token and prompt re-registration on next sign-in | no |
| `NOTIFICATION_PROVIDER_REJECTED` | 502 | SMS or email provider rejected the payload | Retry on the next provider in the lane | no |
| `NOTIFICATION_SMS_CREDITS_EXHAUSTED` | 402 | The tenant's SMS credit is zero | Fall back to push and email; alert the administrator | no |
| `NOTIFICATION_URGENT_OVERRIDE_REFUSED` | 403 | A non-urgent template tried to break quiet hours | Send at the next allowed time | no |
| `NOTIFICATION_DIGEST_WINDOW_MISSED` | 200 | A digest-eligible item arrived after the digest was built | Carry the item to the next digest | yes |
| `NOTIFICATION_PREFERENCE_CONFLICT` | 409 | A user preference contradicts a school policy | Apply the policy and explain which setting won | yes |

## K.13 Requests

| Code | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `REQUESTS_TYPE_NOT_AVAILABLE` | 403 | The request type is not offered to this role or campus | Show the types the person may raise | yes |
| `REQUESTS_REQUIRED_FIELD_MISSING` | 400 | A designed field marked required is empty | Bind to the dynamic form and focus the field | yes |
| `REQUESTS_TRANSITION_NOT_ALLOWED` | 409 | The state move is not in the state machine | Show the allowed actions for the current state | no |
| `REQUESTS_APPROVER_UNAVAILABLE` | 409 | The assigned approver is absent with no delegate | Escalate per the request type and show who now holds it | no |
| `REQUESTS_ALREADY_DECIDED` | 409 | A second decision on a decided request | Show the decision, the decider and the time | yes |
| `REQUESTS_SLA_BREACHED` | 200 | The SLA clock passed while the request was open | Show the breach badge and the escalation target | no |
| `REQUESTS_ON_BEHALF_NOT_PERMITTED` | 403 | Submitting for another person without the permission | Show the person's own submission path | no |
| `REQUESTS_ATTACHMENT_SCAN_PENDING` | 409 | A decision attempted while a file is still being scanned | Poll and enable the decision when the scan clears | yes |
| `REQUESTS_DUPLICATE_OPEN_REQUEST` | 409 | The same subject already has an open request of this type | Open the existing request | yes |

## K.14 Documents

| Code | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `DOCUMENTS_FILE_TYPE_NOT_ALLOWED` | 400 | The upload type is outside the allow list | Show the allowed types | yes |
| `DOCUMENTS_VIRUS_DETECTED` | 422 | ClamAV flagged the upload | Delete the file, notify the uploader, alert the administrator | yes, as "this file could not be accepted" |
| `DOCUMENTS_IMPORT_VALIDATION_FAILED` | 422 | A dry run found errors in the sheet | Download the error report keyed by row and column | no |
| `DOCUMENTS_IMPORT_ROLLBACK_REQUIRED` | 409 | A commit failed partway | Offer the rollback action and show what was applied | no |
| `DOCUMENTS_IMPORT_ROW_LIMIT_EXCEEDED` | 413 | The sheet exceeds the tenant's row limit | Show the limit and offer splitting | no |
| `DOCUMENTS_TEMPLATE_PLACEHOLDER_UNRESOLVED` | 400 | A merge field has no source value | List the unresolved placeholders | no |
| `DOCUMENTS_CERTIFICATE_REVOKED` | 410 | A verification page opened for a revoked certificate | Show the revocation date and the issuing school | yes |
| `DOCUMENTS_SENSITIVE_EXPORT_APPROVAL_REQUIRED` | 403 | A sensitive export without approval | Route to the approval workflow with a reason field | no |
| `DOCUMENTS_SIGNED_URL_EXPIRED` | 403 | A download link older than 5 minutes | Request a fresh link automatically | yes |
| `DOCUMENTS_OCR_CONFIDENCE_LOW` | 200 | OCR capture below the confidence floor | Show the fields for human confirmation | no |

## K.15 Behavior

| Code | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `BEHAVIOR_CATEGORY_NOT_CONFIGURED` | 400 | The tenant has no category for this incident type | Show the category picker and the admin path | no |
| `BEHAVIOR_INCIDENT_WINDOW_PASSED` | 409 | An incident logged outside the allowed backdating window | Show the window and the override path | no |
| `BEHAVIOR_RESTRICTED_NARRATIVE_DENIED` | 403 | Reading the narrative without `view-restricted` | Show the category and date only | yes |
| `BEHAVIOR_POINTS_LIMIT_EXCEEDED` | 409 | Points above the daily ceiling for that teacher | Show the ceiling and the remaining balance | no |
| `BEHAVIOR_BADGE_ALREADY_AWARDED` | 200 | The same badge awarded twice | Treat as success and show the original date | yes |
| `BEHAVIOR_SANCTION_REQUIRES_APPROVAL` | 409 | A sanction above the role's limit | Route to the vice principal | no |
| `BEHAVIOR_STUDENT_NOT_IN_SCOPE` | 403 | Logging against a student outside the actor's scope | Show the homeroom teacher contact | no |
| `BEHAVIOR_OPEN_BADGE_ISSUANCE_FAILED` | 502 | Open Badges 3.0 issuance failed at the verifier | Keep the award, retry issuance, show a pending badge state | yes |
| `BEHAVIOR_INCIDENT_LINKED_TO_CONCERN` | 403 | The incident is attached to a safeguarding concern | Show nothing beyond existence; route to the safeguarding officer | no |

## K.16 Reporting

| Code | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `REPORTING_PROJECTION_STALE` | 200 | The read model is behind its source beyond the freshness budget | Show the "as of" time and offer a live recount | yes |
| `REPORTING_REPORT_TOO_LARGE` | 413 | The result set exceeds the interactive limit | Offer the scheduled export instead | no |
| `REPORTING_AGGREGATE_TOO_SMALL` | 403 | A breakdown would identify fewer than 10 students | Widen the grouping; never show the cell | no |
| `REPORTING_FILTER_OUT_OF_SCOPE` | 403 | A filter reaches outside the actor's data scope | Reset the filter to the actor's scope | no |
| `REPORTING_SCHEDULE_CONFLICT` | 409 | A schedule collides with the nightly rebuild window | Offer the next free window | no |
| `REPORTING_EXPORT_FORMAT_UNSUPPORTED` | 400 | An unsupported export format was requested | Show the supported formats | no |
| `REPORTING_EARLY_WARNING_MODEL_UNAVAILABLE` | 503 | The explainable model is not loaded | Fall back to the transparent rule set and label it | no |
| `REPORTING_EXPLANATION_UNAVAILABLE` | 422 | A number cannot be traced to its records | Block the figure; a figure without a drill-down is a defect | no |
| `REPORTING_DATA_QUALITY_BLOCK` | 409 | A report ran against a data set with open blocking issues | Show the issues with their fix actions | no |

## K.17 Audit

| Code | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `AUDIT_WRITE_FAILED` | 500 | The audit entry could not be written | Fail the originating action; never proceed unlogged | no |
| `AUDIT_CHAIN_BROKEN` | 500 | The hash chain does not verify | Freeze exports, alert platform operators, raise an incident | no |
| `AUDIT_EXPORT_APPROVAL_REQUIRED` | 403 | Export without the high-risk permission | Route to the approver and record the reason | no |
| `AUDIT_QUERY_RANGE_TOO_WIDE` | 413 | The query spans more partitions than allowed | Narrow the date range or schedule the export | no |
| `AUDIT_PARTITION_ARCHIVED` | 409 | The range is in cold storage | Offer the restore request with its lead time | no |
| `AUDIT_REASON_REQUIRED` | 400 | A logged read with no reason supplied | Require the reason before the read | no |
| `AUDIT_ACTOR_UNRESOLVED` | 422 | An entry references a deleted actor | Show the retained actor reference, never a blank | no |
| `AUDIT_IMMUTABLE_RECORD` | 409 | Any attempt to edit or delete an entry | Refuse; the log is append-only | no |
| `AUDIT_ACCESS_REVIEW_OVERDUE` | 200 | An access review campaign passed its due date | Show the overdue campaign to the security administrator | no |

## K.18 Wellbeing

Every code here is written so that the message can be shown without revealing that a record exists.

| Code | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `WELLBEING_ACCESS_DENIED` | 404 | Any access without isolation-level access, deliberately reported as not found | Show nothing; never confirm existence | yes, as a plain not-found |
| `WELLBEING_BREAK_GLASS_REASON_REQUIRED` | 400 | Emergency access with no reason | Require the reason; the access alerts the principal | no |
| `WELLBEING_BREAK_GLASS_EXPIRED` | 403 | The emergency window closed | Require a new break-glass with a new reason | no |
| `WELLBEING_MEDICATION_NOT_AUTHORIZED` | 403 | Administration without a guardian authorization on file | Block, show the authorization request | no |
| `WELLBEING_MEDICATION_DOSE_WINDOW` | 409 | A dose outside the authorized window | Block and require a nurse override with a reason | no |
| `WELLBEING_ALLERGY_ALERT_UNAVAILABLE` | 503 | The live allergy read failed | Block the dependent action and escalate; never serve a cached value | no |
| `WELLBEING_CONCERN_ESCALATION_REQUIRED` | 409 | A concern closed without the required escalation step | Show the escalation step and its owner | no |
| `WELLBEING_CASE_ALREADY_CLOSED` | 409 | An edit to a closed case | Offer a new entry that references the closed case | no |
| `WELLBEING_PLAN_ACCOMMODATION_NOT_APPLIED` | 409 | An exam sitting without an active accommodation | Block the sitting and alert the coordinator | no |

## K.19 Human Resources

| Code | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `HR_LEAVE_BALANCE_INSUFFICIENT` | 409 | The request exceeds the remaining balance | Show the balance and the unpaid-leave option | no |
| `HR_LEAVE_OVERLAPS_EXISTING` | 409 | Dates overlap an approved leave | Show the overlapping record | no |
| `HR_SUBSTITUTION_NOT_ARRANGED` | 409 | Leave approval before cover is arranged | Open the substitute suggestions | no |
| `HR_CONTRACT_EXPIRED` | 403 | An action by a staff member with a lapsed contract | Block and route to HR | no |
| `HR_LICENCE_EXPIRED` | 403 | Teaching with an expired licence where the country requires one | Block the assignment and notify HR and the principal | no |
| `HR_SALARY_ACCESS_DENIED` | 403 | Salary read without `hr.payroll.view-salary` | Hide the section entirely | no |
| `HR_PAYROLL_PERIOD_LOCKED` | 409 | An input change after the period locked | Offer an adjustment in the next period | no |
| `HR_APPRAISAL_CYCLE_CLOSED` | 409 | An appraisal edit after the cycle closed | Show the cycle dates | no |
| `HR_DOCUMENT_EXPIRING_BLOCK` | 409 | A required staff document is past its expiry | List the documents with upload actions | no |

## K.20 Operations

| Code | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `OPERATIONS_ITEM_ALREADY_ON_LOAN` | 409 | A library item is out | Offer the reservation queue | yes |
| `OPERATIONS_BORROWER_LIMIT_REACHED` | 409 | The borrower holds the maximum items | Show current loans and due dates | yes |
| `OPERATIONS_FINE_OUTSTANDING` | 409 | Borrowing blocked by an unpaid fine, for example 15.00 JOD | Show the fine and the pay action | yes |
| `OPERATIONS_ROUTE_CAPACITY_EXCEEDED` | 409 | A transport subscription beyond seat capacity | Offer another route or the waiting list | yes |
| `OPERATIONS_STOP_NOT_ON_ROUTE` | 400 | The requested stop is not served | Show the served stops on a map | yes |
| `OPERATIONS_STOCK_INSUFFICIENT` | 409 | An issue exceeds stock on hand | Show the quantity available | no |
| `OPERATIONS_ASSET_ALREADY_ASSIGNED` | 409 | An asset assigned to two holders | Show the current holder | no |
| `OPERATIONS_FACILITY_BOOKING_CONFLICT` | 409 | The facility is booked in that slot | Show the next free slot | no |
| `OPERATIONS_VISITOR_BLOCKLISTED` | 403 | A visitor on the tenant's blocklist | Hold at the desk and alert security; no detail on screen | no |

## K.21 AI Assist

| Code | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `AI_DISABLED_FOR_TENANT` | 403 | The tenant has AI assist switched off | Hide the assist entry points | yes |
| `AI_USAGE_LIMIT_REACHED` | 402 | The plan's AI allowance is spent | Show usage and the upgrade path | no |
| `AI_MODEL_UNAVAILABLE` | 503 | The model host is down | Fall back to the manual path; never block the task | yes |
| `AI_OUTPUT_REQUIRES_REVIEW` | 200 | A draft is returned that a human must approve | Show the draft in review state; never auto-publish | no |
| `AI_PROMPT_INJECTION_BLOCKED` | 422 | Untrusted content tried to steer the assistant | Discard the output, log the attempt, notify the administrator | no |
| `AI_SCOPE_VIOLATION_BLOCKED` | 403 | A natural-language query reached outside the actor's scope | Return the in-scope answer only | no |
| `AI_SENSITIVE_CONTEXT_REFUSED` | 403 | The request would read level-S data | Refuse and explain that wellbeing records are never used | no |
| `AI_LANGUAGE_UNSUPPORTED` | 422 | A request in a language the assist does not serve | Offer English or Arabic | yes |
| `AI_EXPLANATION_REQUIRED` | 422 | A suggestion arrived without its reasons | Discard it; a bare score is never shown | no |

---

## K.22 Rules

1. **The code is the contract; the message is not.** The server sends the code, a correlation id and structured parameters. The client looks up the English and Arabic text from its own resource bundle and renders it right-to-left when the locale is Arabic. A server message string is a developer hint, is never rendered to an end user, and is never translated on the server.
2. **A code is never removed, only deprecated.** A retired code keeps its row here, marked deprecated with the release that retired it and the code that replaced it, and the server may keep emitting it for one major version. Removing a row breaks every client that still maps it, including mobile builds a family has not updated.
3. **A code is never reused for a different meaning.** A new meaning gets a new code even when the old one is free.
4. **Parent-safe is a property of the code, not of the caller.** A code marked not parent-safe is rendered to guardians and students as a generic message with the correlation id, whatever screen it reaches.
5. **Not-found hides existence where existence is the secret.** Wellbeing returns `WELLBEING_ACCESS_DENIED` with 404 rather than 403, because a 403 would confirm that a record exists. Every other service returns 403 for a permission failure so that the user can be told to ask for access.
6. **Cross-cutting codes are generated, not hand-written.** The eight suffixes in K.1 are emitted by the shared problem-details middleware in `Nibras.BuildingBlocks`, so every service reports them identically and no service invents a local variant.
7. **Every code carries a test.** The authorization and validation tests generated from Appendix B assert the exact code, not the status alone, so a refactor that changes a code fails the build rather than a client.
