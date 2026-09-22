# Appendix E. Integration Event Catalog

> Part of `docs/brief/02-appendices/`. Normative. Read with the master brief and the reference architecture.

This is the complete catalog, not a seed. Every event a service publishes appears here with its consumers, its partition key and its payload. An event used anywhere in the kit and missing here is a defect, and `tools/kit-lint` fails on it.

**Routing key.** `<service>.<entity>.<event>.v<n>`, lower case, hyphens inside a word. The service segment is the lower-case canonical service name from Appendix L. Exchange is `nibras.<service>`, topic type, one per publishing service.

**Envelope**, on every message, defined once in `Nibras.Contracts.Shared`:

| Field | Type | Meaning |
|---|---|---|
| `messageId` | uuid v7 | Unique per publication. The inbox key for idempotent consumers |
| `correlationId` | uuid v7 | Constant for the whole user-visible operation, from the Gateway onward |
| `causationId` | uuid v7 | The `messageId` that caused this one. Builds the chain |
| `tenantId` | uuid v7 | Always present, except on platform-scoped events, which say so |
| `userId` | uuid v7 or null | Null when a job or a consumer caused it |
| `occurredAt` | timestamptz | When the fact became true, in UTC, not when it was published |
| `schemaVersion` | int | Matches the `v<n>` in the routing key |
| `partitionKey` | string or null | Set when order matters for one subject. See below |
| `traceparent` | string | W3C trace context, so one trace spans gateway, gRPC and queues |

**Payload rules.** A payload carries identifiers and the few fields a consumer genuinely needs, never a whole entity. It never carries a field classified sensitive in Appendix J: no medical detail, no counseling note, no custody text, no credential, no full payment instrument. A consumer that needs more asks the owning service over gRPC, or subscribes to a different event. This is what makes an event stream safe to store, replay and audit.

**Versioning.** A breaking change publishes `v2` alongside `v1`; both flow until the last consumer moves, and `v1` is retired two minor releases later, tracked in `docs/messages/`. Adding an optional field is not breaking.

**Ordering.** Where `partitionKey` is set, the consumer queue binds through a consistent-hash exchange or runs as a single active consumer, so events for one subject are processed in order. Where it is null, order is not guaranteed and the consumer must not depend on it.

**Cross-cutting events**, published by every service under its own prefix, not repeated per section:

| Event | Publisher | Consumers | Partition key | Payload |
|---|---|---|---|---|
| `<service>.usage.recorded.v1` | every service | Platform | `tenantId` | `meter`, `quantity`, `unit`, `periodStart`, `periodEnd` |
| `<service>.audit.recorded.v1` | every service | Audit | `tenantId` | `actorId`, `action`, `resourceType`, `resourceId`, `before`, `after`, `reason`, `ipHash` |

There is no shared audit routing key. Where a workflow in Appendix R lists an audit entry as a side effect, the entry is the publishing service's own `<service>.audit.recorded.v1` (for example `finance.audit.recorded.v1`), because a service publishes only on its own exchange.

---

## Platform

Tenant lifecycle events are platform-scoped and carry `tenantId` as the subject rather than the caller's tenant.

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `platform.tenant.provisioning-requested.v1` | every service | `tenantId` | `tenantId`, `planCode`, `region`, `locale`, `countryCode`, `schoolType` |
| `platform.tenant.provisioned.v1` | Identity, School, Notification, Documents, Reporting | `tenantId` | `tenantId`, `adminInvitationId`, `completedAt` |
| `platform.tenant.suspended.v1` | every service | `tenantId` | `tenantId`, `reason`, `readOnlyFrom` |
| `platform.tenant.reactivated.v1` | every service | `tenantId` | `tenantId`, `reactivatedAt` |
| `platform.tenant.deletion-requested.v1` | every service | `tenantId` | `tenantId`, `coolingOffEndsAt`, `requestedBy` |
| `platform.tenant.deleted.v1` | every service, Audit | `tenantId` | `tenantId`, `certificateId`, `deletedAt` |
| `platform.plan.changed.v1` | every service | `tenantId` | `planCode`, `previousPlanCode`, `limits`, `effectiveFrom` |
| `platform.feature-flag.changed.v1` | every service | `tenantId` | `flag`, `enabled`, `rolloutPercent` |
| `platform.settings.changed.v1` | every service | `tenantId` | `scope`, `keys`, `changedBy` |
| `platform.terminology.changed.v1` | every service, Web, Mobile | `tenantId` | `overrides` |
| `platform.custom-field.changed.v1` | the owning service, Reporting | `tenantId` | `entityType`, `fieldKey`, `changeType` |
| `platform.limit.approaching.v1` | Notification | `tenantId` | `limit`, `used`, `allowed`, `percent` |
| `platform.trial.ending.v1` | Notification | `tenantId` | `trialEndsAt`, `daysRemaining` |
| `platform.invoice.due.v1` | Notification | `tenantId` | `tenantInvoiceId`, `amount`, `currency`, `dueDate` |
| `platform.webhook.delivery-failed.v1` | Notification | `tenantId` | `endpointId`, `attempts`, `lastStatus` |
| `platform.upgrade.started.v1` | Notification, Reporting | `tenantId` | `upgradeRunId`, `fromVersion`, `toVersion`, `readOnlyFrom`, `windowEndsAt` |

## Identity

The contact-point events carry the verified email address or phone number, because Notification delivers to it and has no synchronous dependency on Identity. The address is Confidential, not Sensitive: Notification stores it encrypted and never logs it, the same treatment as the contact on `identity.user.invited.v1`.

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `identity.user.invited.v1` | Notification, Audit, Platform, Admissions | `userId` | `invitationId`, `email` or `phone`, `roleCode`, `scope`, `expiresAt` |
| `identity.user.registered.v1` | School, Notification, Reporting | `userId` | `userId`, `joinMethod`, `pendingApproval` |
| `identity.join-request.submitted.v1` | Notification, Requests | `userId` | `joinRequestId`, `audience`, `submittedBy` |
| `identity.join-request.approved.v1` | School, Notification | `userId` | `joinRequestId`, `userId`, `roleCode`, `scope` |
| `identity.user.activated.v1` | every service holding a user copy, Notification | `userId` | `userId`, `roles`, `scope`, `preferredLanguage` |
| `identity.user.deactivated.v1` | every service holding a user copy, Requests, School, Admissions | `userId` | `userId`, `reason`, `reassignTo` |
| `identity.role.changed.v1` | every service, Communication | `tenantId` | `roleCode`, `permissionVersion`, `changedBy` |
| `identity.permissions.changed.v1` | every service, Communication | `tenantId` | `permissionVersion`, `affectedUserIds` or `all` |
| `identity.delegation.started.v1` | Requests, Notification | `userId` | `fromUserId`, `toUserId`, `scope`, `until` |
| `identity.delegation.ended.v1` | Requests, Notification | `userId` | `delegationId`, `endedAt` |
| `identity.login.new-device.v1` | Notification, Audit | `userId` | `userId`, `deviceLabel`, `ipHash`, `at` |
| `identity.guardian-link.created.v1` | School, Communication, Finance, Notification, Wellbeing, Admissions | `studentId` | `guardianUserId`, `studentId`, `relationship`, `rights` |
| `identity.access-review.due.v1` | Notification, Requests | `tenantId` | `campaignId`, `reviewerId`, `dueDate` |
| `identity.break-glass.granted.v1` | Notification, Audit, Wellbeing | `userId` | `grantId`, `userId`, `resourceType`, `resourceId`, `reason`, `approvedBy`, `expiresAt` |
| `identity.break-glass.used.v1` | Notification, Audit, Wellbeing | `userId` | `userId`, `resourceType`, `resourceId`, `reason` |
| `identity.impersonation.started.v1` | Notification, Audit, Communication | `userId` | `impersonationId`, `operatorId`, `targetUserId`, `reason`, `until` |
| `identity.contact-point.verified.v1` | Notification | `userId` | `userId`, `channel`, `address`, `verifiedAt` |
| `identity.contact-point.removed.v1` | Notification | `userId` | `userId`, `channel`, `removedAt` |

## School

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `school.academic-year.opened.v1` | every academic service, Platform | `tenantId` | `academicYearId`, `campusId`, `startsOn`, `endsOn` |
| `school.academic-year.closed.v1` | every academic service, Reporting | `tenantId` | `academicYearId`, `closedBy`, `closedAt` |
| `school.term.started.v1` | Academics, Assessment, Scheduling, Attendance, Finance | `tenantId` | `termId`, `academicYearId`, `startsOn`, `endsOn` |
| `school.section.created.v1` | Academics, Assessment, Scheduling, Attendance, Admissions, Communication, Behavior, Operations, Wellbeing | `sectionId` | `sectionId`, `gradeLevelId`, `campusId`, `capacity`, `nameEn`, `nameAr` |
| `school.section.changed.v1` | the same set | `sectionId` | `sectionId`, changed fields |
| `school.student.enrolled.v1` | Academics, Assessment, Attendance, Finance, Communication, Behavior, Operations, Reporting, Requests, Admissions, Notification | `studentId` | `studentId`, `studentNumber`, `sectionId`, `campusId`, `enrolledOn`, `namesEnAr` |
| `school.student.section-changed.v1` | Academics, Assessment, Attendance, Behavior, Operations, Requests, Notification, Ai | `studentId` | `studentId`, `fromSectionId`, `toSectionId`, `effectiveOn` |
| `school.student.status-changed.v1` | every service holding a student copy, Ai, Admissions | `studentId` | `studentId`, `fromStatus`, `toStatus`, `effectiveOn`, `reasonCode` |
| `school.student.promoted.v1` | Academics, Assessment, Finance, Reporting, Admissions | `studentId` | `studentId`, `fromGradeLevelId`, `toGradeLevelId`, `outcome` |
| `school.student.profile-updated.v1` | services holding a copy, Requests | `studentId` | `studentId`, changed field names only |
| `school.student-document.expiring.v1` | Notification, Requests | `studentId` | `studentId`, `documentType`, `expiresOn` |
| `school.sibling.linked.v1` | Finance, Admissions | `studentId` | `studentId`, `siblingStudentId`, `source` |
| `school.sibling.unlinked.v1` | Finance, Admissions | `studentId` | `studentId`, `siblingStudentId`, `unlinkedAt` |
| `school.guardian.updated.v1` | Communication, Finance, Notification, Wellbeing, Requests, Reporting | `studentId` | `guardianId`, `studentIds`, changed field names |
| `school.staff.created.v1` | Identity, Academics, Scheduling, Hr, Communication, Requests, Notification | `staffId` | `staffId`, `employeeNumber`, `departmentId`, `campusIds`, `namesEnAr` |
| `school.staff.changed.v1` | the `school.staff.created.v1` set | `staffId` | `staffId`, `namesEnAr`, `departmentId`, `campusIds`, changed field names |
| `school.staff.left.v1` | Identity, Academics, Scheduling, Requests, Hr, Communication, Notification | `staffId` | `staffId`, `lastWorkingDay`, `reassignTo` |
| `school.room.changed.v1` | Scheduling | `roomId` | `roomId`, `buildingId`, `campusId`, `kind`, `capacity`, `nameEn`, `nameAr`, `changeType` |
| `school.grade-level.changed.v1` | Admissions | `gradeLevelId` | `gradeLevelId`, `stageId`, `nameEn`, `nameAr`, `changeType` |
| `school.department.changed.v1` | Hr | `departmentId` | `departmentId`, `nameEn`, `nameAr`, `changeType` |
| `school.grading-period.changed.v1` | Assessment | `gradingPeriodId` | `gradingPeriodId`, `termId`, `nameEn`, `nameAr`, `startsOn`, `endsOn`, `lockAt`, `changeType` |
| `school.calendar-day.changed.v1` | Scheduling, Attendance, Requests | `campusId` | `calendarDayId`, `campusId`, `date`, `kind`, `labelEn`, `labelAr`, `changeType` |

## Admissions

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `admissions.inquiry.created.v1` | Reporting, Notification | `inquiryId` | `inquiryId`, `source`, `gradeLevelId`, `campusId` |
| `admissions.application.submitted.v1` | Documents, Notification, Reporting | `applicationId` | `applicationId`, `gradeLevelId`, `requiredDocuments` |
| `admissions.application.stage-changed.v1` | Reporting, Notification | `applicationId` | `applicationId`, `fromStage`, `toStage`, `by` |
| `admissions.offer.made.v1` | Finance, Documents, Notification, Reporting | `applicationId` | `applicationId`, `offerId`, `expiresAt`, `depositAmount`, `currency` |
| `admissions.offer.accepted.v1` | School, Identity, Finance, Documents | `applicationId` | `offerId`, `applicantId`, `sectionId`, `feePlanCode` |
| `admissions.offer.expired.v1` | Notification, Reporting | `applicationId` | `offerId`, `expiredAt`, `waitingListPromotedId` |
| `admissions.re-enrollment.confirmed.v1` | School, Finance, Reporting, Requests | `studentId` | `studentId`, `academicYearId`, `confirmedBy` |
| `admissions.re-enrollment.declined.v1` | School, Finance, Reporting, Notification, Requests | `studentId` | `studentId`, `academicYearId`, `reasonCode` |

## Academics

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `academics.teaching-assignment.changed.v1` | Assessment, Scheduling, Identity | `staffId` | `staffId`, `sectionId`, `subjectId`, `effectiveOn` |
| `academics.assignment.published.v1` | Notification, Reporting | `sectionId` | `assignmentId`, `sectionId`, `subjectId`, `dueAt`, `maxMark` |
| `academics.submission.received.v1` | Reporting | `assignmentId` | `submissionId`, `assignmentId`, `studentId`, `receivedAt`, `late` |
| `academics.submission.graded.v1` | Assessment, Notification, Reporting | `assignmentId` | `submissionId`, `mark`, `maxMark`, `gradedBy` |
| `academics.submission.missing.v1` | Reporting | `assignmentId` | `submissionId`, `assignmentId`, `studentId`, `dueAt`, `markedMissingAt` |
| `academics.lesson-plan.submitted.v1` | Notification, Reporting, Ai | `staffId` | `lessonPlanId`, `staffId`, `weekOf`, `status` |
| `academics.homework-load.exceeded.v1` | Notification, Reporting | `sectionId` | `sectionId`, `date`, `assignedMinutes`, `ceilingMinutes` |
| `academics.syllabus-coverage.behind.v1` | Notification, Reporting | `sectionId` | `sectionId`, `subjectId`, `plannedPercent`, `actualPercent` |

## Assessment

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `assessment.marks.entered.v1` | Reporting | `componentId` | `componentId`, `sectionId`, `enteredCount`, `by` |
| `assessment.marks.approved.v1` | Reporting, Notification | `componentId` | `componentId`, `approvedBy`, `at` |
| `assessment.marks.overdue.v1` | Notification, Reporting | `sectionId` | `sectionId`, `subjectId`, `staffId`, `dueAt`, `escalateTo` |
| `assessment.marks.awaiting-approval.v1` | Notification | `sectionId` | `componentId`, `approverId`, `waitingSince` |
| `assessment.grades.locked.v1` | Academics, Documents, Reporting | `gradingPeriodId` | `gradingPeriodId`, `sectionIds`, `lockedBy` |
| `assessment.report-cards.generation-requested.v1` | Documents | `studentId` | `batchId`, `studentId`, `templateId`, `languages` |
| `assessment.report-card.generated.v1` | Reporting | `studentId` | `reportCardId`, `batchId`, `studentId`, `gradingPeriodId`, `documentId`, `versionNumber` |
| `assessment.report-cards.published.v1` | Communication, Notification, Reporting, Ai | `gradingPeriodId` | `batchId`, `gradingPeriodId`, `publishedTo`, `versionNumber` |
| `assessment.exam-paper.approved.v1` | Notification, Reporting | `examId` | `examPaperId`, `examId`, `approvedBy`, `approvedAt` |
| `assessment.exam-paper.released.v1` | Notification, Reporting | `examId` | `examPaperId`, `examId`, `releasedTo`, `releasedAt` |
| `assessment.grade-change.approved.v1` | Documents, Notification, Audit, Requests | `studentId` | `studentId`, `componentId`, `fromMark`, `toMark`, `reason`, `approvedBy` |

## Scheduling

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `scheduling.timetable.published.v1` | Academics, Attendance, Operations, Notification | `timetableVersionId` | `timetableVersionId`, `academicYearId`, `campusId`, `effectiveFrom` |
| `scheduling.timetable.changed.v1` | Academics, Attendance, Notification, Requests | `timetableVersionId` | `timetableVersionId`, `changedEntryIds`, `effectiveFrom` |
| `scheduling.substitution.assigned.v1` | Attendance, Notification, Hr, Requests, Reporting | `staffId` | `substitutionId`, `absentStaffId`, `coverStaffId`, `date`, `periodIds` |
| `scheduling.event.published.v1` | Communication, Notification | `tenantId` | `eventId`, `audience`, `startsAt`, `campusId` |
| `scheduling.room-booking.approved.v1` | Operations, Notification, Requests | `roomId` | `bookingId`, `roomId`, `from`, `to`, `bookedBy` |
| `scheduling.exam-timetable.published.v1` | Assessment, Notification | `tenantId` | `examSessionId`, `gradeLevelIds`, `startsOn` |

## Attendance

Safety lives inside Attendance, so gate passes, visitors and emergencies carry the `attendance.` prefix. Earlier versions showed a `safety.` prefix that no service owned.

A mark review is a change to a locked register, or an offline conflict, that waits for the holder of the edit-after-lock permission (Appendix M.3). `kind` is `edit-after-lock` or `offline-conflict`; Requests turns the request into a task in the reviewer's inbox and closes it on the resolution.

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `attendance.attendance.marked.v1` | Reporting, Requests, Ai | `sectionId` | `sessionId`, `sectionId`, `date`, `periodId`, `presentCount`, `absentCount` |
| `attendance.student.absent.v1` | Notification, Wellbeing, Reporting | `studentId` | `studentId`, `date`, `periodId`, `code`, `sectionId` |
| `attendance.excuse.approved.v1` | Reporting, Notification, Requests | `studentId` | `excuseId`, `studentId`, `dates`, `code`, `approvedBy` |
| `attendance.threshold.reached.v1` | Wellbeing, Notification, Reporting | `studentId` | `studentId`, `ruleId`, `kind`, `count`, `escalateTo` |
| `attendance.attendance.not-marked.v1` | Notification, Reporting | `sectionId` | `sectionId`, `periodId`, `staffId`, `cutOffAt`, `escalateTo` |
| `attendance.mark-review.requested.v1` | Requests | `sectionId` | `reviewId`, `sessionId`, `sectionId`, `kind`, `studentCount`, `submittedBy`, `approverScope` |
| `attendance.mark-review.resolved.v1` | Requests | `sectionId` | `reviewId`, `sessionId`, `decision`, `resolvedBy`, `resolvedAt` |
| `attendance.dismissal.processed.v1` | Notification, Reporting, Wellbeing | `studentId` | `studentId`, `kind`, `at`, `releasedTo` |
| `attendance.gate-pass.issued.v1` | Notification, Requests | `studentId` | `gatePassId`, `studentId`, `validFrom`, `validUntil`, `issuedBy` |
| `attendance.gate-pass.used.v1` | Notification, Audit | `studentId` | `gatePassId`, `usedAt`, `verifiedBy` |
| `attendance.visitor.checked-in.v1` | Notification, Reporting | `campusId` | `visitorId`, `campusId`, `host`, `at`, `watchlistHit` |
| `attendance.emergency.broadcast-started.v1` | Notification, Communication, Reporting | `campusId` | `broadcastId`, `campusId`, `kind`, `startedBy` |
| `attendance.emergency.acknowledged.v1` | Reporting | `campusId` | `broadcastId`, `userId`, `at`, `location` |
| `attendance.roll-call.completed.v1` | Notification, Reporting | `campusId` | `broadcastId`, `accountedFor`, `missing` |

## Finance

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `finance.fee-plan.assigned.v1` | Reporting, Admissions, School, Requests | `studentId` | `studentId`, `feePlanCode`, `academicYearId`, `total`, `currency` |
| `finance.invoice-run.requested.v1` | Documents, Reporting | `tenantId` | `runId`, `scope`, `requestedBy`, `expectedCount` |
| `finance.invoice.issued.v1` | Documents, Notification, Reporting, Requests | `invoiceId` | `invoiceId`, `series`, `number`, `studentId`, `payerId`, `amount`, `currency`, `dueDate` |
| `finance.payment.received.v1` | Admissions, Operations, Notification, Reporting | `invoiceId` | `paymentId`, `invoiceIds`, `amount`, `currency`, `method`, `gatewayRef`, `sourceRefs` |
| `finance.payment.failed.v1` | Notification | `invoiceId` | `attemptId`, `invoiceId`, `reasonCode` |
| `finance.cheque.bounced.v1` | Notification, Reporting | `invoiceId` | `paymentId`, `chequeNumber`, `bouncedOn`, `feeApplied` |
| `finance.refund.processed.v1` | Notification, Documents, Reporting, Requests | `invoiceId` | `refundId`, `amount`, `currency`, `approvedBy` |
| `finance.credit-note.issued.v1` | Documents, Reporting, Admissions, Requests | `invoiceId` | `creditNoteId`, `invoiceId`, `amount`, `reason` |
| `finance.invoice.overdue.v1` | Admissions, Notification, Reporting | `invoiceId` | `invoiceId`, `studentId`, `daysOverdue`, `amountOutstanding`, `ladderStep` |
| `finance.account.restricted.v1` | Assessment, Documents, Notification, Admissions, Reporting | `studentId` | `studentId`, `restrictions`, `policyId`, `appliedBy` |
| `finance.account.cleared.v1` | Assessment, Documents, Notification, Admissions, School, Reporting | `studentId` | `studentId`, `clearedAt` |
| `finance.day.closed.v1` | Reporting, Audit | `campusId` | `shiftId`, `campusId`, `date`, `declaredTotal`, `countedTotal`, `difference` |
| `finance.scholarship.awarded.v1` | Notification, Reporting | `studentId` | `awardId`, `studentId`, `schemeCode`, `academicYearId`, `amount`, `currency`, `approvedBy` |
| `finance.payer.changed.v1` | Notification, Reporting | `studentId` | `studentId`, `fromPayerId`, `toPayerId`, `sharePercent`, `effectiveFrom` |
| `finance.cash-session.closed.v1` | Reporting, Audit | `campusId` | `cashSessionId`, `campusId`, `cashierId`, `declaredTotal`, `countedTotal`, `difference`, `closedAt` |
| `finance.deposit.recorded.v1` | Reporting, Audit | `campusId` | `depositId`, `campusId`, `cashSessionIds`, `amount`, `currency`, `depositSlipNumber`, `depositedOn` |

`sourceRefs` on `finance.payment.received.v1` is optional and lists the business records a paid invoice was raised for, as `kind` and `id` pairs, where `kind` is `offer` or `application` and `id` is the `offerId` or `applicationId`. It lets Admissions match a deposit or an application fee without reading Finance. `studentId` on `finance.invoice.overdue.v1` is null for an invoice raised before the applicant became a student. Both are optional additions and not breaking.

**Command Finance accepts from Admissions.** Commands are catalogued in `docs/plan/11-messaging-architecture.md`; this one is named here because its outcome depends on the `sourceRefs` field above.

| Command | Routing key | Sent on | Payload | Outcome |
|---|---|---|---|---|
| `RaiseApplicationFee` | `finance.commands.raise-application-fee.v1` | `nibras.admissions` | `applicationId`, `applicantId`, `campaignId`, `amount`, `currency`, `dueDate` | `finance.invoice.issued.v1`; when paid, `finance.payment.received.v1` with `sourceRefs` naming the application |

## Communication

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `communication.announcement.published.v1` | Notification, Reporting, Ai | `tenantId` | `announcementId`, `audience`, `requiresAcknowledgment`, `expiresAt` |
| `communication.acknowledgment.recorded.v1` | Reporting | `announcementId` | `announcementId`, `userId`, `at` |
| `communication.acknowledgment.overdue.v1` | Notification | `announcementId` | `announcementId`, `userId`, `dueAt`, `escalateTo` |
| `communication.message.sent.v1` | Reporting | `threadId` | `messageId`, `threadId`, `fromRole`, `toRole`, `hasAttachment` |
| `communication.message.reported.v1` | Wellbeing, Notification, Audit | `threadId` | `messageId`, `reportedBy`, `reasonCode` |
| `communication.meeting.booked.v1` | Notification, Scheduling, Requests | `staffId` | `meetingId`, `staffId`, `guardianId`, `studentId`, `startsAt` |
| `communication.meeting.changed.v1` | Notification, Requests | `staffId` | `meetingId`, `previousStartsAt`, `startsAt`, `reason` |
| `communication.concern.reported-anonymously.v1` | Wellbeing, Notification | `campusId` | `concernId`, `campusId`, `category`, `submittedAt` |

## Notification

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `notification.notification.requested.v1` | Notification workers | `userId` | `notificationId`, `templateCode`, `recipients`, `urgency`, `language` |
| `notification.notification.delivered.v1` | Platform, Reporting | `userId` | `notificationId`, `channel`, `deliveredAt` |
| `notification.notification.failed.v1` | Platform, Reporting, Notification | `userId` | `notificationId`, `channel`, `reasonCode`, `willRetry` |
| `notification.channel.suppressed.v1` | Reporting | `userId` | `userId`, `channel`, `reason`, `permanent` |

## Requests

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `requests.request.submitted.v1` | Notification, Reporting | `requestId` | `requestId`, `typeCode`, `submittedBy`, `onBehalfOf`, `slaDueAt` |
| `requests.request.needs-info.v1` | Notification | `requestId` | `requestId`, `askedBy`, `question` |
| `requests.request.approved.v1` | the owning service of the effect, Notification, Reporting | `requestId` | `requestId`, `typeCode`, `effect`, `subjectId`, `approvedBy` |
| `requests.request.rejected.v1` | Notification, Reporting | `requestId` | `requestId`, `reason`, `rejectedBy` |
| `requests.request.completed.v1` | Notification, Reporting | `requestId` | `requestId`, `outcome`, `completedAt` |
| `requests.request.sla-breached.v1` | Notification, Reporting | `requestId` | `requestId`, `assigneeId`, `breachedAt`, `escalateTo` |
| `requests.request.reassigned.v1` | Notification, Reporting | `requestId` | `requestId`, `fromAssigneeId`, `toAssigneeId`, `reason`, `reassignedBy` |
| `requests.request.withdrawn.v1` | Notification, Reporting | `requestId` | `requestId`, `typeCode`, `withdrawnBy`, `withdrawnAt` |
| `requests.request.expired.v1` | Notification, Reporting | `requestId` | `requestId`, `typeCode`, `stepKey`, `expiredAt` |
| `requests.task.assigned.v1` | Notification, Reporting | `userId` | `taskId`, `assigneeId`, `sourceRequestId`, `dueAt` |
| `requests.task.completed.v1` | Reporting | `userId` | `taskId`, `completedBy`, `at` |

## Documents

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `documents.document.generation-requested.v1` | Documents workers | `subjectId` | `jobId`, `templateId`, `subjectId`, `language`, `requestedBy` |
| `documents.document.generated.v1` | the requesting service, Notification | `subjectId` | `jobId`, `documentId`, `subjectId`, `sizeBytes`, `verificationCode` |
| `documents.certificate.revoked.v1` | Notification, Reporting, Admissions, School, Requests | `subjectId` | `certificateId`, `reason`, `revokedBy` |
| `documents.import.completed.v1` | the target service, Notification, Reporting | `jobId` | `jobId`, `entityType`, `succeeded`, `failed`, `errorReportId` |
| `documents.export.completed.v1` | Notification, Audit, Platform, Requests, Reporting | `jobId` | `jobId`, `entityType`, `rowCount`, `requestedBy`, `reason` |
| `documents.sensitive-export.performed.v1` | Notification, Audit | `jobId` | `jobId`, `entityType`, `rowCount`, `requestedBy`, `reason`, `watermark` |
| `documents.file.scan-failed.v1` | Notification, Audit | `fileId` | `fileId`, `uploadedBy`, `verdict` |

## Behavior

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `behavior.incident.recorded.v1` | Wellbeing, Notification, Reporting, Ai | `studentId` | `incidentId`, `studentIds`, `categoryCode`, `severity`, `recordedBy`, `restricted` |
| `behavior.points.awarded.v1` | Notification, Reporting | `studentId` | `studentId`, `points`, `categoryCode`, `houseId` |
| `behavior.badge.awarded.v1` | Notification, Documents, Reporting | `studentId` | `studentId`, `badgeCode`, `awardedBy` |
| `behavior.consequence.assigned.v1` | Notification, Reporting | `studentId` | `incidentId`, `studentId`, `consequenceCode`, `scheduledFor` |

## Reporting

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `reporting.early-warning.flag-raised.v1` | Wellbeing, Notification | `studentId` | `studentId`, `indicatorCode`, `score`, `factors`, `assistRung` |
| `reporting.early-warning.flag-cleared.v1` | Wellbeing, Notification | `studentId` | `studentId`, `indicatorCode`, `clearedAt`, `reason` |
| `reporting.data-quality.issue-detected.v1` | the owning service, Notification | `tenantId` | `ruleCode`, `entityType`, `affectedCount`, `severity` |
| `reporting.projection.rebuild-completed.v1` | Platform | `tenantId` | `projection`, `fromCheckpoint`, `toCheckpoint`, `duration` |

## Audit

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `audit.integrity-check.failed.v1` | Platform, Notification, Reporting | `tenantId` | `fromEntryId`, `toEntryId`, `detectedAt` |
| `audit.retention.partition-detached.v1` | Platform | `tenantId` | `partition`, `rowCount`, `movedTo` |

## Wellbeing

Wellbeing events carry identifiers, a category code and a timestamp. They never carry clinical detail, a note, a diagnosis or a medication name. A consumer that legitimately needs more asks Wellbeing with a permission that is checked and logged.

Attendance consumes three Wellbeing events to drive WF-ATT-01 and WF-WEL-02: the intervention events move a student's attendance case to `InterventionOpened` and `InterventionClosed`, and `wellbeing.clinic-visit.collection-arranged.v1` prepares the gate pass for the sent-home student. None of the three carries a category, a symptom or a reason; `sourceRuleId` is the Attendance threshold rule that led to the intervention, or null; Attendance discards an intervention event whose `sourceRuleId` is null and keeps nothing of it.

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `wellbeing.referral.created.v1` | Notification, Reporting | `studentId` | `referralId`, `studentId`, `categoryCode`, `urgency`, `createdAt` |
| `wellbeing.intervention.opened.v1` | Reporting, Notification, Attendance | `studentId` | `interventionId`, `studentId`, `ownerId`, `reviewDate`, `sourceRuleId` |
| `wellbeing.intervention.closed.v1` | Reporting, Attendance | `studentId` | `interventionId`, `studentId`, `outcomeCode`, `closedAt` |
| `wellbeing.clinic-visit.recorded.v1` | Notification | `studentId` | `visitId`, `studentId`, `categoryCode`, `guardianNotified`, `at` |
| `wellbeing.clinic-visit.collection-arranged.v1` | Attendance | `studentId` | `visitId`, `studentId`, `pickupPersonId`, `arrangedAt` |
| `wellbeing.medication.administered.v1` | Notification | `studentId` | `logId`, `studentId`, `authorizationId`, `at`, `administeredBy` |
| `wellbeing.safeguarding.concern-raised.v1` | Notification (safeguarding officer only) | `studentId` | `concernId`, `urgency`, `raisedAt` |

## Hr

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `hr.staff.hired.v1` | Identity, School, Notification | `staffId` | `staffId`, `startsOn`, `roleCode`, `departmentId`, `campusIds` |
| `hr.leave.approved.v1` | Scheduling, Attendance, Notification, Requests | `staffId` | `leaveId`, `staffId`, `fromDate`, `toDate`, `leaveTypeCode` |
| `hr.leave.cancelled.v1` | Scheduling, Attendance, Notification, Requests | `staffId` | `leaveId`, `cancelledAt`, `reason` |
| `hr.leave-balance.low.v1` | Notification | `staffId` | `staffId`, `leaveTypeCode`, `remainingDays` |
| `hr.staff-document.expiring.v1` | Notification, Requests | `staffId` | `staffId`, `documentType`, `expiresOn` |
| `hr.payroll.inputs-ready.v1` | Finance, Notification | `tenantId` | `periodId`, `staffCount`, `preparedBy` |
| `hr.appraisal.completed.v1` | Reporting, Notification | `staffId` | `appraisalId`, `staffId`, `cycleId`, `outcomeCode` |

## Operations

Operations publishes under one prefix with the sub-domain as the entity, which is how a later split into separate services stays mechanical.

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `operations.transport.boarding-recorded.v1` | Attendance, Notification, Reporting | `studentId` | `studentId`, `routeId`, `stopId`, `direction`, `at` |
| `operations.transport.vehicle-delayed.v1` | Notification | `routeId` | `routeId`, `delayMinutes`, `reasonCode`, `estimatedArrival` |
| `operations.transport.subscription-changed.v1` | Finance, Notification, Requests | `studentId` | `studentId`, `routeId`, `stopId`, `effectiveFrom` |
| `operations.library.loan-recorded.v1` | Reporting | `studentId` | `loanId`, `studentId`, `copyId`, `dueOn` |
| `operations.library.loan-overdue.v1` | Finance, Notification | `studentId` | `loanId`, `studentId`, `daysOverdue`, `fineAmount`, `currency` |
| `operations.facility.ticket-raised.v1` | Notification, Reporting | `campusId` | `ticketId`, `roomId`, `priority`, `raisedBy` |
| `operations.facility.ticket-closed.v1` | Reporting | `campusId` | `ticketId`, `closedBy`, `cost`, `currency` |
| `operations.inventory.stock-low.v1` | Notification | `campusId` | `itemId`, `onHand`, `reorderLevel` |
| `operations.frontdesk.complaint-received.v1` | Requests, Notification | `campusId` | `complaintId`, `categoryCode`, `slaDueAt` |
| `operations.activity.enrollment-confirmed.v1` | Finance, Notification | `studentId` | `studentId`, `activityId`, `feeAmount`, `currency` |

## Ai

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `ai.usage.recorded.v1` | Platform | `tenantId` | `featureCode`, `rung`, `tokensOrUnits`, `at` |
| `ai.index.rebuild-completed.v1` | Platform, Reporting | `tenantId` | `scope`, `documentCount`, `duration` |
| `ai.suggestion.rejected.v1` | Reporting | `tenantId` | `featureCode`, `rung`, `reasonCode` |

---

## Events that exist only as scheduled jobs

Some notifications in Appendix C are not caused by a state change but by a clock. These are Quartz jobs inside the owning service, which then publish the event above. Listing them stops anyone hunting for a publisher that does not exist.

| Job | Service | Schedule | Publishes |
|---|---|---|---|
| Unmarked class reminder | Attendance | per period cut-off, per campus time zone | `attendance.attendance.not-marked.v1` |
| Assignment due reminder | Academics | daily, evening before | `notification.notification.requested.v1` |
| Missing work at closing | Academics | every 5 minutes, for assignments past their closing time | `academics.submission.missing.v1` |
| Request expiry | Requests | hourly | `requests.request.expired.v1` |
| Marks overdue check | Assessment | daily | `assessment.marks.overdue.v1` |
| Acknowledgment chase | Communication | daily | `communication.acknowledgment.overdue.v1` |
| Reminder ladder | Finance | daily | `finance.invoice.overdue.v1` |
| Document expiry scan | School, Hr | weekly | `school.student-document.expiring.v1`, `hr.staff-document.expiring.v1` |
| Leave balance check | Hr | monthly | `hr.leave-balance.low.v1` |
| Plan limit and trial check | Platform | daily | `platform.limit.approaching.v1`, `platform.trial.ending.v1` |
| Library overdue scan | Operations | daily | `operations.library.loan-overdue.v1` |
| Audit integrity verification | Audit | nightly | `audit.integrity-check.failed.v1` on failure |
| Reference-copy reconciliation | every service holding a copy | nightly | `reporting.data-quality.issue-detected.v1` on a mismatch |
| Pre-peak cache warm-up | Platform | before first period, per tenant time zone | none; it warms caches |
| Uncovered-period escalation (`UncoveredPeriodEscalationJob`) | Scheduling | every 5 minutes from 06:00 to the last period on school days | `notification.notification.requested.v1` |
| Joining timeouts (`JoiningTimeoutsJob`) | Identity | every 15 minutes | `notification.notification.requested.v1` for the day-7 invitation reminder |
| Deletion cooling-off reminder (`DeletionCoolingOffReminderJob`) | Platform | daily 09:00 per tenant time zone, through the 30-day cooling-off | `notification.notification.requested.v1` |
| API key expiry reminder (`ApiKeyExpiryReminderJob`) | Platform | daily 08:00 per tenant time zone | `notification.notification.requested.v1` at 14 and 3 days before expiry |
| Sandbox lifecycle (`SandboxLifecycleJob`) | Platform | daily 04:00 UTC | `notification.notification.requested.v1` at 60 and 83 days without a call |
| Support SLA (`SupportSlaJob`) | Platform | every 5 minutes | `notification.notification.requested.v1` to the next support level |
| Subject request deadline (`SubjectRequestDeadlineJob`) | Platform | daily 08:00 per tenant time zone | `notification.notification.requested.v1` |
| Daily digest of blocked attempts | Ai | daily, per tenant | `notification.notification.requested.v1`, never with the planted text |
| Re-enrollment window (`ReEnrollmentWindowJob`) | Admissions | daily 08:00 campus time | `notification.notification.requested.v1` at days 7 and 14 of the window |
| Digest builder (`DigestSchedulerJob`) | Notification | every 15 minutes, for users whose digest is due in their time zone | none; it builds each due digest inside Notification |
