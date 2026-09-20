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

---

## Platform

Tenant lifecycle events are platform-scoped and carry `tenantId` as the subject rather than the caller's tenant.

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `platform.tenant.provisioning-requested.v1` | every service | `tenantId` | `tenantId`, `planCode`, `region`, `locale`, `countryCode`, `schoolType` |
| `platform.tenant.provisioned.v1` | Identity, School, Notification, Reporting | `tenantId` | `tenantId`, `adminInvitationId`, `completedAt` |
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

## Identity

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `identity.user.invited.v1` | Notification, Audit | `userId` | `invitationId`, `email` or `phone`, `roleCode`, `scope`, `expiresAt` |
| `identity.user.registered.v1` | School, Notification, Reporting | `userId` | `userId`, `joinMethod`, `pendingApproval` |
| `identity.join-request.submitted.v1` | Notification, Requests | `userId` | `joinRequestId`, `audience`, `submittedBy` |
| `identity.join-request.approved.v1` | School, Notification | `userId` | `joinRequestId`, `userId`, `roleCode`, `scope` |
| `identity.user.activated.v1` | every service holding a user copy, Notification | `userId` | `userId`, `roles`, `scope`, `preferredLanguage` |
| `identity.user.deactivated.v1` | every service holding a user copy, Requests | `userId` | `userId`, `reason`, `reassignTo` |
| `identity.role.changed.v1` | every service, Communication | `tenantId` | `roleCode`, `permissionVersion`, `changedBy` |
| `identity.permissions.changed.v1` | every service, Communication | `tenantId` | `permissionVersion`, `affectedUserIds` or `all` |
| `identity.delegation.started.v1` | Requests, Notification | `userId` | `fromUserId`, `toUserId`, `scope`, `until` |
| `identity.delegation.ended.v1` | Requests, Notification | `userId` | `delegationId`, `endedAt` |
| `identity.login.new-device.v1` | Notification, Audit | `userId` | `userId`, `deviceLabel`, `ipHash`, `at` |
| `identity.guardian-link.created.v1` | School, Communication, Finance, Notification | `studentId` | `guardianUserId`, `studentId`, `relationship`, `rights` |
| `identity.access-review.due.v1` | Notification, Requests | `tenantId` | `campaignId`, `reviewerId`, `dueDate` |
| `identity.break-glass.used.v1` | Notification, Audit, Wellbeing | `userId` | `userId`, `resourceType`, `resourceId`, `reason` |

## School

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `school.academic-year.opened.v1` | every academic service | `tenantId` | `academicYearId`, `campusId`, `startsOn`, `endsOn` |
| `school.academic-year.closed.v1` | every academic service, Reporting | `tenantId` | `academicYearId`, `closedBy`, `closedAt` |
| `school.term.started.v1` | Academics, Assessment, Scheduling, Attendance, Finance | `tenantId` | `termId`, `academicYearId`, `startsOn`, `endsOn` |
| `school.section.created.v1` | Academics, Assessment, Scheduling, Attendance, Admissions | `sectionId` | `sectionId`, `gradeLevelId`, `campusId`, `capacity` |
| `school.section.changed.v1` | the same set | `sectionId` | `sectionId`, changed fields |
| `school.student.enrolled.v1` | Academics, Assessment, Attendance, Finance, Communication, Behavior, Operations, Reporting | `studentId` | `studentId`, `studentNumber`, `sectionId`, `campusId`, `enrolledOn`, `namesEnAr` |
| `school.student.section-changed.v1` | Academics, Assessment, Attendance, Behavior, Operations | `studentId` | `studentId`, `fromSectionId`, `toSectionId`, `effectiveOn` |
| `school.student.status-changed.v1` | every service holding a student copy, Ai | `studentId` | `studentId`, `fromStatus`, `toStatus`, `effectiveOn`, `reasonCode` |
| `school.student.promoted.v1` | Academics, Assessment, Finance, Reporting | `studentId` | `studentId`, `fromGradeLevelId`, `toGradeLevelId`, `outcome` |
| `school.student.profile-updated.v1` | services holding a copy | `studentId` | `studentId`, changed field names only |
| `school.student-document.expiring.v1` | Notification, Requests | `studentId` | `studentId`, `documentType`, `expiresOn` |
| `school.guardian.updated.v1` | Communication, Finance, Notification | `studentId` | `guardianId`, `studentIds`, changed field names |
| `school.staff.created.v1` | Identity, Academics, Scheduling, Hr | `staffId` | `staffId`, `employeeNumber`, `departmentId`, `campusIds` |
| `school.staff.left.v1` | Identity, Academics, Scheduling, Requests, Hr | `staffId` | `staffId`, `lastWorkingDay`, `reassignTo` |

## Admissions

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `admissions.inquiry.created.v1` | Reporting, Notification | `inquiryId` | `inquiryId`, `source`, `gradeLevelId`, `campusId` |
| `admissions.application.submitted.v1` | Documents, Notification, Reporting | `applicationId` | `applicationId`, `gradeLevelId`, `requiredDocuments` |
| `admissions.application.stage-changed.v1` | Reporting, Notification | `applicationId` | `applicationId`, `fromStage`, `toStage`, `by` |
| `admissions.offer.made.v1` | Finance, Documents, Notification | `applicationId` | `applicationId`, `offerId`, `expiresAt`, `depositAmount`, `currency` |
| `admissions.offer.accepted.v1` | School, Identity, Finance, Documents | `applicationId` | `offerId`, `applicantId`, `sectionId`, `feePlanCode` |
| `admissions.offer.expired.v1` | Notification, Reporting | `applicationId` | `offerId`, `expiredAt`, `waitingListPromotedId` |
| `admissions.re-enrollment.confirmed.v1` | School, Finance, Reporting | `studentId` | `studentId`, `academicYearId`, `confirmedBy` |
| `admissions.re-enrollment.declined.v1` | School, Finance, Reporting, Notification | `studentId` | `studentId`, `academicYearId`, `reasonCode` |

## Academics

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `academics.teaching-assignment.changed.v1` | Assessment, Scheduling, Identity | `staffId` | `staffId`, `sectionId`, `subjectId`, `effectiveOn` |
| `academics.assignment.published.v1` | Notification, Reporting | `sectionId` | `assignmentId`, `sectionId`, `subjectId`, `dueAt`, `maxMark` |
| `academics.submission.received.v1` | Reporting | `assignmentId` | `submissionId`, `assignmentId`, `studentId`, `receivedAt`, `late` |
| `academics.submission.graded.v1` | Assessment, Notification, Reporting | `assignmentId` | `submissionId`, `mark`, `maxMark`, `gradedBy` |
| `academics.lesson-plan.submitted.v1` | Notification, Reporting | `staffId` | `lessonPlanId`, `staffId`, `weekOf`, `status` |
| `academics.homework-load.exceeded.v1` | Notification, Reporting | `sectionId` | `sectionId`, `date`, `assignedMinutes`, `ceilingMinutes` |
| `academics.syllabus-coverage.behind.v1` | Notification, Reporting | `sectionId` | `sectionId`, `subjectId`, `plannedPercent`, `actualPercent` |

## Assessment

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `assessment.marks.entered.v1` | Reporting | `componentId` | `componentId`, `sectionId`, `enteredCount`, `by` |
| `assessment.marks.approved.v1` | Reporting, Notification | `componentId` | `componentId`, `approvedBy`, `at` |
| `assessment.marks.overdue.v1` | Notification | `sectionId` | `sectionId`, `subjectId`, `staffId`, `dueAt`, `escalateTo` |
| `assessment.marks.awaiting-approval.v1` | Notification | `sectionId` | `componentId`, `approverId`, `waitingSince` |
| `assessment.grades.locked.v1` | Academics, Documents, Reporting | `gradingPeriodId` | `gradingPeriodId`, `sectionIds`, `lockedBy` |
| `assessment.report-cards.generation-requested.v1` | Documents | `studentId` | `batchId`, `studentId`, `templateId`, `languages` |
| `assessment.report-cards.published.v1` | Communication, Notification, Reporting | `gradingPeriodId` | `batchId`, `gradingPeriodId`, `publishedTo`, `versionNumber` |
| `assessment.grade-change.approved.v1` | Documents, Notification, Audit | `studentId` | `studentId`, `componentId`, `fromMark`, `toMark`, `reason`, `approvedBy` |

## Scheduling

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `scheduling.timetable.published.v1` | Academics, Attendance, Operations, Notification | `timetableVersionId` | `timetableVersionId`, `academicYearId`, `campusId`, `effectiveFrom` |
| `scheduling.timetable.changed.v1` | Academics, Attendance, Notification | `timetableVersionId` | `timetableVersionId`, `changedEntryIds`, `effectiveFrom` |
| `scheduling.substitution.assigned.v1` | Attendance, Notification, Hr | `staffId` | `substitutionId`, `absentStaffId`, `coverStaffId`, `date`, `periodIds` |
| `scheduling.event.published.v1` | Communication, Notification | `tenantId` | `eventId`, `audience`, `startsAt`, `campusId` |
| `scheduling.room-booking.approved.v1` | Operations, Notification | `roomId` | `bookingId`, `roomId`, `from`, `to`, `bookedBy` |
| `scheduling.exam-timetable.published.v1` | Assessment, Notification | `tenantId` | `examSessionId`, `gradeLevelIds`, `startsOn` |

## Attendance

Safety lives inside Attendance, so gate passes, visitors and emergencies carry the `attendance.` prefix. Earlier versions showed a `safety.` prefix that no service owned.

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `attendance.attendance.marked.v1` | Reporting | `sectionId` | `sessionId`, `sectionId`, `date`, `periodId`, `presentCount`, `absentCount` |
| `attendance.student.absent.v1` | Notification, Wellbeing, Reporting | `studentId` | `studentId`, `date`, `periodId`, `code`, `sectionId` |
| `attendance.excuse.approved.v1` | Reporting, Notification | `studentId` | `excuseId`, `studentId`, `dates`, `code`, `approvedBy` |
| `attendance.threshold.reached.v1` | Wellbeing, Notification, Reporting | `studentId` | `studentId`, `ruleId`, `kind`, `count`, `escalateTo` |
| `attendance.attendance.not-marked.v1` | Notification | `sectionId` | `sectionId`, `periodId`, `staffId`, `cutOffAt`, `escalateTo` |
| `attendance.dismissal.processed.v1` | Notification, Reporting | `studentId` | `studentId`, `kind`, `at`, `releasedTo` |
| `attendance.gate-pass.issued.v1` | Notification | `studentId` | `gatePassId`, `studentId`, `validFrom`, `validUntil`, `issuedBy` |
| `attendance.gate-pass.used.v1` | Notification, Audit | `studentId` | `gatePassId`, `usedAt`, `verifiedBy` |
| `attendance.visitor.checked-in.v1` | Notification, Reporting | `campusId` | `visitorId`, `campusId`, `host`, `at`, `watchlistHit` |
| `attendance.emergency.broadcast-started.v1` | Notification, Communication, Reporting | `campusId` | `broadcastId`, `campusId`, `kind`, `startedBy` |
| `attendance.emergency.acknowledged.v1` | Reporting | `campusId` | `broadcastId`, `userId`, `at`, `location` |
| `attendance.roll-call.completed.v1` | Notification, Reporting | `campusId` | `broadcastId`, `accountedFor`, `missing` |

## Finance

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `finance.fee-plan.assigned.v1` | Reporting | `studentId` | `studentId`, `feePlanCode`, `academicYearId`, `total`, `currency` |
| `finance.invoice-run.requested.v1` | Documents, Reporting | `tenantId` | `runId`, `scope`, `requestedBy`, `expectedCount` |
| `finance.invoice.issued.v1` | Documents, Notification, Reporting | `invoiceId` | `invoiceId`, `series`, `number`, `studentId`, `payerId`, `amount`, `currency`, `dueDate` |
| `finance.payment.received.v1` | Admissions, Operations, Notification, Reporting | `invoiceId` | `paymentId`, `invoiceIds`, `amount`, `currency`, `method`, `gatewayRef` |
| `finance.payment.failed.v1` | Notification | `invoiceId` | `attemptId`, `invoiceId`, `reasonCode` |
| `finance.cheque.bounced.v1` | Notification, Reporting | `invoiceId` | `paymentId`, `chequeNumber`, `bouncedOn`, `feeApplied` |
| `finance.refund.processed.v1` | Notification, Documents, Reporting | `invoiceId` | `refundId`, `amount`, `currency`, `approvedBy` |
| `finance.credit-note.issued.v1` | Documents, Reporting | `invoiceId` | `creditNoteId`, `invoiceId`, `amount`, `reason` |
| `finance.invoice.overdue.v1` | Admissions, Notification, Reporting | `invoiceId` | `invoiceId`, `daysOverdue`, `amountOutstanding`, `ladderStep` |
| `finance.account.restricted.v1` | Assessment, Documents, Notification | `studentId` | `studentId`, `restrictions`, `policyId`, `appliedBy` |
| `finance.account.cleared.v1` | Assessment, Documents, Notification | `studentId` | `studentId`, `clearedAt` |
| `finance.day.closed.v1` | Reporting, Audit | `campusId` | `shiftId`, `campusId`, `date`, `declaredTotal`, `countedTotal`, `difference` |

## Communication

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `communication.announcement.published.v1` | Notification, Reporting | `tenantId` | `announcementId`, `audience`, `requiresAcknowledgment`, `expiresAt` |
| `communication.acknowledgment.recorded.v1` | Reporting | `announcementId` | `announcementId`, `userId`, `at` |
| `communication.acknowledgment.overdue.v1` | Notification | `announcementId` | `announcementId`, `userId`, `dueAt`, `escalateTo` |
| `communication.message.sent.v1` | Reporting | `threadId` | `messageId`, `threadId`, `fromRole`, `toRole`, `hasAttachment` |
| `communication.message.reported.v1` | Wellbeing, Notification, Audit | `threadId` | `messageId`, `reportedBy`, `reasonCode` |
| `communication.meeting.booked.v1` | Notification, Scheduling | `staffId` | `meetingId`, `staffId`, `guardianId`, `studentId`, `startsAt` |
| `communication.meeting.changed.v1` | Notification | `staffId` | `meetingId`, `previousStartsAt`, `startsAt`, `reason` |
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
| `requests.task.assigned.v1` | Notification, Reporting | `userId` | `taskId`, `assigneeId`, `sourceRequestId`, `dueAt` |
| `requests.task.completed.v1` | Reporting | `userId` | `taskId`, `completedBy`, `at` |

## Documents

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `documents.document.generation-requested.v1` | Documents workers | `subjectId` | `jobId`, `templateId`, `subjectId`, `language`, `requestedBy` |
| `documents.document.generated.v1` | the requesting service, Notification | `subjectId` | `jobId`, `documentId`, `subjectId`, `sizeBytes`, `verificationCode` |
| `documents.certificate.revoked.v1` | Notification, Reporting | `subjectId` | `certificateId`, `reason`, `revokedBy` |
| `documents.import.completed.v1` | the target service, Notification, Reporting | `jobId` | `jobId`, `entityType`, `succeeded`, `failed`, `errorReportId` |
| `documents.export.completed.v1` | Notification, Audit | `jobId` | `jobId`, `entityType`, `rowCount`, `requestedBy`, `reason` |
| `documents.sensitive-export.performed.v1` | Notification, Audit | `jobId` | `jobId`, `entityType`, `rowCount`, `requestedBy`, `reason`, `watermark` |
| `documents.file.scan-failed.v1` | Notification, Audit | `fileId` | `fileId`, `uploadedBy`, `verdict` |

## Behavior

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `behavior.incident.recorded.v1` | Wellbeing, Notification, Reporting | `studentId` | `incidentId`, `studentIds`, `categoryCode`, `severity`, `recordedBy`, `restricted` |
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
| `audit.integrity-check.failed.v1` | Platform, Notification | `tenantId` | `fromEntryId`, `toEntryId`, `detectedAt` |
| `audit.retention.partition-detached.v1` | Platform | `tenantId` | `partition`, `rowCount`, `movedTo` |

## Wellbeing

Wellbeing events carry identifiers, a category code and a timestamp. They never carry clinical detail, a note, a diagnosis or a medication name. A consumer that legitimately needs more asks Wellbeing with a permission that is checked and logged.

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `wellbeing.referral.created.v1` | Notification, Reporting | `studentId` | `referralId`, `studentId`, `categoryCode`, `urgency`, `createdAt` |
| `wellbeing.intervention.opened.v1` | Reporting, Notification | `studentId` | `interventionId`, `studentId`, `ownerId`, `reviewDate` |
| `wellbeing.intervention.closed.v1` | Reporting | `studentId` | `interventionId`, `outcomeCode`, `closedAt` |
| `wellbeing.clinic-visit.recorded.v1` | Notification | `studentId` | `visitId`, `studentId`, `categoryCode`, `guardianNotified`, `at` |
| `wellbeing.medication.administered.v1` | Notification | `studentId` | `logId`, `studentId`, `authorizationId`, `at`, `administeredBy` |
| `wellbeing.safeguarding.concern-raised.v1` | Notification (safeguarding officer only) | `studentId` | `concernId`, `urgency`, `raisedAt` |

## Hr

| Event | Consumers | Partition key | Payload |
|---|---|---|---|
| `hr.staff.hired.v1` | Identity, School, Notification | `staffId` | `staffId`, `startsOn`, `roleCode`, `departmentId`, `campusIds` |
| `hr.leave.approved.v1` | Scheduling, Attendance, Notification | `staffId` | `leaveId`, `staffId`, `fromDate`, `toDate`, `leaveTypeCode` |
| `hr.leave.cancelled.v1` | Scheduling, Attendance, Notification | `staffId` | `leaveId`, `cancelledAt`, `reason` |
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
| `operations.transport.subscription-changed.v1` | Finance, Notification | `studentId` | `studentId`, `routeId`, `stopId`, `effectiveFrom` |
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
