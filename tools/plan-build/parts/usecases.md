## academics

Use cases (Application/Features/): 15
- F ManageSubjectOfferings: offerings and electives
- F ManageTeachingAssignments: assignments and load
- F ManageStudentGroups: groups and members
- F ManageCurriculum: units, lessons, outcomes, standards
- F ManageLessonPlans: plans, review, share, coverage
- F AssignmentLifecycle: WF-ACA-01: create, edit, publish, extend, work views
- F ManageRubrics: rubrics
- F SubmitWork: student submission
- F GradeSubmission: grading, bulk offline sync, resubmission, return, accept-late, exempt, similarity
- F ManageResources: resource library
- F ManageQuestionBank: questions, export, QTI
- F ManageQuizzes: quizzes, publish, results, analysis, grading queue
- F TakeQuiz: student attempts
- F LaunchLtiTool: Tier 2 LTI launch
- F GetJob: job resource and cancel
Consumers: 13
- C TenantLifecycleConsumer: tenant rows and settings
- C AcademicYearOpenedConsumer: school.academic-year.opened.v1 and the new partition
- C AcademicYearClosedConsumer: school.academic-year.closed.v1
- C TermStartedConsumer: school.term.started.v1
- C SectionConsumer: school.section.created.v1 and changed
- C StudentEnrolledConsumer: school.student.enrolled.v1
- C StudentSectionChangedConsumer: school.student.section-changed.v1
- C StudentStatusChangedConsumer: school.student.status-changed.v1
- C StudentPromotedConsumer: school.student.promoted.v1
- C StudentProfileUpdatedConsumer: school.student.profile-updated.v1
- C StaffConsumer: school.staff.created.v1 and left
- C TimetableConsumer: scheduling.timetable.published.v1 and changed
- C GradesLockedConsumer: assessment.grades.locked.v1
Jobs: 12
- J AssignmentDueReminderJob: evening reminder
- J AssignmentClosingJob: Published to Missing
- J UngradedEscalationJob: 10 and 15 working days
- J SyllabusCoverageCheckJob: weekly coverage
- J QtiImportJob: QTI import with progress
- J QtiExportJob: QTI export with progress
- J CaseStandardsImportJob: Tier 2 CASE import
- J ItemAnalysisJob: Tier 2 item analysis
- J TimetableFetchJob: timetable copy pages
- J SubmissionPartitionJob: yearly partition
- J ReferenceCopyReconciliationJob: nightly reconciliation
- J UsageFlushJob: usage meters

## admissions

Use cases (Application/Features/): 13
- F PublicInquiry: public inquiry and tour booking
- F ApplicantOtp: OTP challenge and verification
- F PublicApplication: save and resume, documents, submit
- F RespondToOffer: public accept and decline
- F ManageCampaigns: campaign configuration
- F ManageInquiries: staff inquiries, follow-ups, tours
- F InquiryToEnrollment: WF-ADM-01 staff transitions
- F ManageAssessments: assessment slots, assessments, interviews
- F MakeOffer: offers, withdraw, extend, deposit confirmation
- F ManageWaitingList: capacity and waiting list
- F TrackEnrolment: saga strip and retry
- F ReEnrollmentWithFeeSettlementCheck: WF-ADM-02
- F GetJob: job resource and cancel
Consumers: 6
- C TenantLifecycleConsumer: tenant rows, suspension, settings
- C SectionConsumer: school.section.created.v1 and changed
- C PaymentReceivedConsumer: finance.payment.received.v1 deposit and fee matching
- C InvoiceOverdueConsumer: finance.invoice.overdue.v1 for known invoices
- C RequestApprovedConsumer: requests.request.approved.v1 processing badge
- C OfferLetterConsumer: documents.document.generated.v1 for offer letters
Jobs: 14
- J OfferExpiryJob: expiry and automatic promotion
- J OfferReminderJob: day 7 and day 12 reminders
- J NeedsInformationTimeoutJob: 21-day withdrawal
- J WaitlistReconfirmationJob: 30-day reconfirmation
- J ApplicationDocumentExpiryJob: document expiry reminders
- J FollowUpDueJob: follow-up reminders
- J ReEnrollmentWindowJob: reminders and window close
- J BlockedOnFeesEscalationJob: 14 and 30 day escalation
- J SeatUsageRefreshJob: enrolled counts from School
- J GradeLevelSnapshotJob: grade-level copy
- J ReferenceCopyReconciliationJob: nightly reconciliation
- J ApplicantRetentionJob: anonymization of non-enrolled applicants
- J PublicSessionCleanupJob: expired sessions
- J UsageFlushJob: usage meters

## ai

Use cases (Application/Features/): 22
- F SubmitDraft: POST /drafts
- F SubmitTranslation: POST /translations
- F SubmitAsk: POST /asks
- F SubmitQueryPlan: POST /query-plans
- F AssistJobs: read once and cancel
- F AcceptDraft: POST /drafts/{jobId}/accept
- F DismissDraft: POST /drafts/{jobId}/dismiss
- F EffectiveFeatures: GET /features
- F FeatureConfiguration: GET /configuration, PUT /configuration/features/{featureCode}
- F ModelCatalog: GET /models
- F ProviderConfiguration: GET, PUT /configuration/provider and /disable
- F ProviderConsent: POST /configuration/provider/consents and /withdraw
- F IndexStatus: GET /index/status
- F IndexRebuilds: POST /index/rebuilds, GET /index/rebuilds/{id}
- F IndexSource: worker command ai.commands.index-source.v1
- F RebuildIndex: worker command ai.commands.rebuild-index.v1
- F GenerateDraft: worker command ai.commands.draft.v1
- F UsageSummary: GET /usage
- F CallLog: GET /usage/calls
- F ExportUsage: POST /usage/export
- F TenantLifecycle: Saga 1, 2, 10 commands every service handles
- F Jobs: job resource and cancel
Consumers: 5
- C TenantLifecycleConsumer: platform tenant, plan, feature-flag, terminology and custom-field keys
- C SettingsChangedConsumer: platform.settings.changed.v1 for AI and General
- C PermissionsChangedConsumer: identity.role.changed.v1 and identity.permissions.changed.v1 evict retrieval entries
- C DataQualityIssueConsumer: reporting.data-quality.issue-detected.v1 for Ai streams
- C StudentStatusChangedConsumer: school.student.status-changed.v1 purges a leaver's chunks
Jobs: 4
- J JobsRequests: get and cancel records
- J JobsHandler: reads IJobStore
- J JobsValidator: starter or platform.jobs.view
- J JobsEndpoint: /jobs routes

## assessment

Use cases (Application/Features/): 26
- F ManageStructures: create, edit, publish, copy, delete structures
- F GetStructure: structure for a section and period
- F ManageGradeSchemes: schemes, drafts, versions, assignments
- F GetMarkGrid: the grid read from one covering index, never cached
- F EnterMarks: batch write from the grid at keyboard speed
- F SyncMarkDrafts: offline draft replay with the Appendix M mark rules
- F SubmitComponent: MarkEntry to Validated
- F GetMarkHistory: change history of one mark
- F GetMarkStatus: entry and overdue state per section, and the export
- F ModerateComponent: Validated to Moderated
- F ApproveGradingPeriod: Moderated to Approved
- F PublishAndLock: publish windows, lock and unlock
- F GetResults: student and section results, year results
- F RecalculateResults: recalculation job
- F GetResultAnalysis: distribution, comparisons, cohort, item analysis, heatmap, predicted
- F ManageReportCardTemplates: templates, validation, comment bank
- F ManageReportCardComments: teacher and AI-drafted comments with review
- F StartReportCardBatch: starts Saga 7
- F ManageReportCardBatch: status, retry, cancel, publish
- F GetReportCards: versions, signed document link, acknowledgment, reissue, export
- F IssueTranscript: transcript view and issue
- F ManageGradeChanges: WF-ASM-02 transitions
- F ManageExams: exams, candidates, invigilators, seats, admit cards, makeups
- F ManageExamPaper: WF-ASM-03 transitions
- F SagaCommands: command handlers for other services' sagas
- F TenantLifecycle: Saga 1, 2 and 10 command handlers
Consumers: 17
- C AcademicYearConsumer: school.academic-year.opened.v1 and closed.v1
- C TermStartedConsumer: school.term.started.v1 and the grading-period fetch
- C SectionConsumer: school.section.created.v1 and changed.v1
- C StudentEnrolledConsumer: school.student.enrolled.v1
- C StudentSectionChangedConsumer: school.student.section-changed.v1
- C StudentStatusChangedConsumer: school.student.status-changed.v1
- C StudentPromotedConsumer: school.student.promoted.v1
- C StudentProfileUpdatedConsumer: school.student.profile-updated.v1
- C TeachingAssignmentChangedConsumer: academics.teaching-assignment.changed.v1
- C AccountRestrictionConsumer: finance.account.restricted.v1 and cleared.v1
- C SubmissionGradedConsumer: academics.submission.graded.v1 imports the grade
- C ExamTimetablePublishedConsumer: scheduling.exam-timetable.published.v1
- C RequestApprovedConsumer: requests.request.approved.v1, acknowledged and discarded
- C DocumentGeneratedConsumer: documents.document.generated.v1 per card, transcript and admit card
- C ComputeResultsHandler: assessment.commands.compute-results.v1 in the worker
- C TenantLifecycleConsumers: platform.tenant.* events
- C PlatformContextConsumer: settings, plan, feature-flag and terminology changes
Jobs: 10
- J MarksOverdueCheckJob: daily; publishes assessment.marks.overdue.v1
- J ReproducibilityCheckJob: weekly recomputation sample
- J InvariantAuditJob: nightly domain sample
- J ReferenceCopyReconciliationJob: nightly reconciliation trigger
- J GradeChangeEscalationJob: WF-ASM-02 reminders, escalation, lapse
- J ExamPaperDeadlineJob: WF-ASM-03 deadlines
- J PublishWindowJob: opens and closes publish windows
- J PartitionMaintenanceJob: year list partitions and archive
- J LeaverRetentionJob: 10-year academic record anonymization
- J AssessmentUsageMeterJob: daily assessment.usage.recorded.v1

## attendance

Use cases (Application/Features/): 29
- F MarkAttendance: the worked feature: a teacher marks a class
- F EditAfterLock: elevated correction after the cut-off, permission attendance.student-attendance.edit-after-lock
- F BulkMarkAttendance: whole-section bulk endpoint with per-item results, permission attendance.student-attendance.bulk-mark
- F SubmitExcuse: parent or student submits an excuse with evidence
- F ApproveExcuse: approver decision; publishes attendance.excuse.approved.v1
- F RejectExcuse: (extension) approver refuses an excuse with a reason code
- F ReviewExcuses: (extension) excuse queue and detail for the approver
- F GetSectionRegister: the register a teacher opens: the hottest read, served by a compiled query
- F GetTeacherSessionsToday: (extension) the teacher's sessions of the day (document 21 §3.8 query 1)
- F GetUnmarkedSessions: (extension) principal card and drill-down (REQ-ATT-038)
- F NudgeTeacher: (extension) principal's nudge action
- F GetAttendanceChanges: (extension) delta page that Bff.Mobile wraps in the delta token
- F GetStudentSummary: counts and streaks for the Student 360 card
- F GetStudentRecords: (extension) per-student timeline (REQ-ATT-037)
- F GetThresholdHits: (extension) flags with the reasons that produced them
- F ManageThresholds: create, edit, delete threshold rules
- F MarkStaffAttendance: (extension) staff register and marking (REQ-ATT-023)
- F GetAttendanceReports: (extension) operational reports (REQ-ATT-035)
- F ReviewOfflineMarks: (extension) the pending edit-after-lock and conflict queue
- F RecordCheckIn: (extension) QR, NFC, kiosk and device adapter scans (REQ-ATT-012, REQ-ATT-013)
- F ManagePickupPersons: (extension) the authorised collector list (REQ-ATT-024)
- F IssueGatePass: issue, verify and revoke gate passes
- F VerifyPickupPerson: gate verification of a collector against the authorised list
- F RecordDismissal: (extension) releases, early-leave record and the late-pickup log (REQ-ATT-028, REQ-ATT-031)
- F CheckInVisitor: front-desk check-in and check-out with the watch-list
- F StartEmergencyBroadcast: broadcast, acknowledgements, roll call and reunification
- F SyncOfflineMarks: replays the mobile outbox with the conflict rules of Appendix M
- F RequestEffects: (extension) Saga 6 effect command handlers (document 13 section 4)
- F TenantLifecycle: (extension) Saga 1, 2 and 10 command handlers every data-owning service carries
Consumers: 18
- C StudentEnrolledConsumer: school.student.enrolled.v1 creates the StudentReference
- C StudentSectionChangedConsumer: school.student.section-changed.v1 moves the reference between sections
- C StudentStatusChangedConsumer: school.student.status-changed.v1 retires the reference
- C StudentProfileUpdatedConsumer: (extension) school.student.profile-updated.v1 refreshes names and photo
- C SectionCreatedConsumer: school.section.created.v1 creates the SectionReference
- C SectionChangedConsumer: (extension) school.section.changed.v1 updates the SectionReference
- C AcademicYearConsumer: (extension) school.academic-year.opened.v1 and closed.v1 open and freeze the year
- C TermStartedConsumer: school.term.started.v1 opens the term's marking calendar
- C TimetablePublishedConsumer: scheduling.timetable.published.v1 and scheduling.timetable.changed.v1 rebuild TimetableOfDay
- C SubstitutionAssignedConsumer: scheduling.substitution.assigned.v1 changes who may mark the period
- C LeaveApprovedConsumer: hr.leave.approved.v1 and hr.leave.cancelled.v1 pre-fill staff absence
- C RequestApprovedConsumer: requests.request.approved.v1 dispatches the leave, early dismissal and pickup change effects
- C TransportBoardingRecordedConsumer: operations.transport.boarding-recorded.v1 pre-fills bus presence
- C TenantProvisioningConsumer: (extension) platform.tenant.provisioning-requested.v1
- C TenantStatusConsumer: (extension) platform.tenant.suspended.v1 and reactivated.v1 refuse and reopen writes
- C TenantDeletionConsumer: (extension) platform.tenant.deletion-requested.v1 and deleted.v1
- C SettingsChangedConsumer: (extension) platform.settings.changed.v1 evicts rules and reloads isolation
- C PlatformContextConsumer: (extension) plan, feature-flag and terminology changes evict the tenant context
Jobs: 11
- J UnmarkedClassReminderJob: per period cut-off, per campus time zone; publishes attendance.attendance.not-marked.v1
- J ThresholdEvaluationJob: nightly threshold pass; publishes attendance.threshold.reached.v1
- J AttendanceAgainstTimetableJob: the daily orphan check in both directions (master brief Section 19)
- J PartitionMaintenanceJob: creates next month's partition and detaches per Appendix J
- J ReferenceCopyReconciliationJob: (extension) nightly trigger of ReferenceCopyReconciler per tenant
- J InvariantAuditJob: (extension) nightly sample and term_counters repair (document 21 §11)
- J GatePassExpiryJob: (extension) expires unused passes and their dismissals
- J PresenceSignalCleanupJob: (extension) deletes pre-fill signals older than 7 days
- J VisitorRetentionJob: (extension) 2-year visitor deletion (document 10 part 8)
- J LeaverRetentionJob: (extension) gate passes of leavers after 1 year
- J AttendanceUsageMeterJob: (extension) daily attendance.usage.recorded.v1

## audit

Use cases (Application/Features/): 7
- F Entries: search and read the chain
- F Exports: high-risk export
- F LoginHistory: 7-year sign-in history
- F AccessLog: reads of sensitive records
- F Integrity: verification and partitions
- F Ingestion: entries arriving from every service, no HTTP
- F TenantLifecycle: commands from Platform's sagas
Consumers: 8
- C AuditRecordedConsumer: <service>.audit.recorded.v1 from all twenty exchanges, batched per tenant
- C EvidenceConsumer: the ten evidence events of section 7.2
- C TenantDeletedConsumer: platform.tenant.deleted.v1 register and head state
- C TenantProvisioningRequestedConsumer: platform.tenant.provisioning-requested.v1 creates the head
- C TenantLifecycleConsumer: the other platform.* broadcasts recorded as evidence
- C SettingsChangedConsumer: platform.settings.changed.v1 for scope retention
- C PermissionsChangedConsumer: identity.role.changed.v1 and identity.permissions.changed.v1
- C DataQualityIssueConsumer: reporting.data-quality.issue-detected.v1, ignored for non-audit entities
Jobs: 6
- J IntegrityVerificationJob: nightly chain walk
- J PartitionMaintenanceJob: partitions ahead, detach at the retention period with holds honoured
- J AuditExportJob: export files
- J PartitionRestoreJob: restore and re-verify cold partitions
- J IngestionLagWatchJob: readiness and alert on lag
- J UsageMeterJob: daily storage meter

## behavior

Use cases (Application/Features/): 12
- F ManageCategories: categories and ladders
- F IncidentToIntervention: WF-BEH-01 transitions
- F ReadIncidents: incident lists, detail, narrative, export
- F ManageConsequences: consequences and detention sessions
- F ManagePlans: behavior plans and reviews
- F AwardPoints: points, sync, revoke
- F GetLeaderboard: house, section and optional individual totals
- F ManageBadges: badges, awards, Open Badges
- F StudentRecognition: recognition summary and portfolio
- F StudentIncidents: one student's incidents
- F Analytics: behavior analytics
- F TenantLifecycle: Saga 1, 2 and 10 handlers
Consumers: 12
- C StudentEnrolledConsumer: school.student.enrolled.v1
- C StudentSectionChangedConsumer: school.student.section-changed.v1
- C StudentStatusChangedConsumer: school.student.status-changed.v1 starts the retention clock
- C StudentProfileUpdatedConsumer: school.student.profile-updated.v1
- C SectionConsumer: school.section.created.v1 and changed.v1
- C UserConsumer: identity.user.activated.v1 and deactivated.v1, reroutes reviews
- C TenantProvisioningConsumer: platform.tenant.provisioning-requested.v1
- C TenantStatusConsumer: suspension and reactivation
- C TenantDeletionConsumer: deletion requested and deleted
- C SettingsChangedConsumer: platform.settings.changed.v1 for scope behavior
- C PlatformContextConsumer: plan, flags, terminology, custom fields
- C DataQualityIssueConsumer: reporting.data-quality.issue-detected.v1 for Behavior types
Jobs: 10
- J IncidentReviewEscalationJob: review deadlines by severity
- J GuardianNoticeDeadlineJob: 24-hour guardian notice
- J PlanFollowUpJob: review dates and 5-day escalation
- J DetentionOutcomeJob: unrecorded detentions become missed
- J OpenBadgeIssuanceRetryJob: pending credentials
- J LeaverAnonymizationJob: 3 years after leaving
- J ReferenceCopyReconciliationJob: nightly copies
- J InvariantAuditJob: totals against entries
- J PartitionMaintenanceJob: next year's partition
- J BehaviorUsageMeterJob: daily behavior.usage.recorded.v1

## bff-mobile

Use cases (Application/Features/): 11
- F GetVersionPolicy: GET /bff/mobile/v1/config/version
- F GetRemoteConfig: GET /bff/mobile/v1/config/remote
- F GetMobileBootstrap: GET /bff/mobile/v1/me/bootstrap and /me/permissions
- F ManageDevices: POST /me/devices and DELETE /me/devices/{deviceId}
- F GetMobileHome: GET /bff/mobile/v1/home/{role}
- F GetModeDay: GET /bff/mobile/v1/modes/gate/today and /modes/bus/today
- F SyncBatch: POST /bff/mobile/v1/sync/batch
- F SyncPull: POST /bff/mobile/v1/sync/pull
- F Uploads: POST, PATCH, HEAD /bff/mobile/v1/uploads
- F GetStudent360: GET /bff/mobile/v1/students/{id}/360 and /transparency
- F PassThrough: ANY /bff/mobile/v1/api/{service}/{**path} and the hub route
Consumers: 0
Jobs: 0

## bff-web

Use cases (Application/Features/): 12
- F GetBootstrap: GET /bff/web/v1/me/bootstrap
- F GetPermissions: GET /bff/web/v1/me/permissions
- F GetReference: GET /bff/web/v1/me/reference
- F GetCounters: GET /bff/web/v1/me/counters
- F Preferences: GET and PUT /bff/web/v1/me/preferences, a pass-through to Identity
- F GetViewAsRole: GET /bff/web/v1/admin/view-as
- F GetRoleHome: GET /bff/web/v1/home/{role}, one composer per role in Composition/Homes/
- F GetStudent360: GET /bff/web/v1/students/{id}/360 and /360/timeline
- F GetTransparency: GET /bff/web/v1/students/{id}/transparency
- F Search: GET /bff/web/v1/search and POST /search/natural-language
- F GetConsoleJobs: GET /bff/web/v1/console/jobs
- F GetTenantArtefacts: manifest.webmanifest and the two .well-known files
Consumers: 0
Jobs: 0

## communication

Use cases (Application/Features/): 22
- F ManageAnnouncements: list, mine, create, get, patch, delete, withdraw, preview audience
- F PublishAnnouncement: publish and require acknowledgment
- F AcknowledgeAnnouncement: acknowledgment and read
- F AnnouncementAnalytics: read and acknowledgment figures
- F Translate: announcement and message translation
- F ManageNews: news posts and media
- F StartConversation: new direct or group conversation
- F SendMessage: the worked messaging feature
- F ReadMessages: message pages, read receipts, search, close
- F ReportAndBlock: report a message, block and unblock
- F OfficeHoursAndPolicy: own office hours and the policy page
- F Moderate: moderation queue and decisions
- F OverseeMessages: oversight reads
- F SafeguardingExport: high-risk export
- F ReportConcernAnonymously: anonymous concern
- F ManageMeetingSlots: slots and conference days
- F BookMeeting: bookings, moves, cancellations, notes
- F MeetingEffect: Saga 6 BookMeeting and CancelMeeting
- F ManageSurveys: Tier 2 surveys, polls and consent forms
- F ManagePolicies: Tier 2 policies and acknowledgments
- F TenantLifecycle: Saga 1, 2, 10 commands
- F Jobs: job resource and cancel
Consumers: 9
- C TenantLifecycleConsumer: the tenant-lifecycle set and the policy recompilation
- C StudentReferenceConsumer: school.student.enrolled.v1, status-changed.v1, profile-updated.v1
- C GuardianLinkConsumer: school.guardian.updated.v1 and identity.guardian-link.created.v1
- C StaffReferenceConsumer: school.staff.created.v1 and school.staff.left.v1
- C SectionReferenceConsumer: school.section.created.v1 and school.section.changed.v1
- C ReportCardsPublishedConsumer: assessment.report-cards.published.v1 feed item
- C SchoolEventPublishedConsumer: scheduling.event.published.v1 feed item
- C RequestApprovedConsumer: requests.request.approved.v1 status display
- C EmergencyBannerConsumer: attendance.emergency.broadcast-started.v1 on the urgent queue
Jobs: 4
- J JobsRequests: get and cancel records
- J JobsHandler: reads IJobStore
- J JobsValidator: starter or platform.jobs.view
- J JobsEndpoint: /jobs routes

## documents

Use cases (Application/Features/): 20
- F UploadFile: upload, version upload, metadata edit, delete
- F ListFiles: file lists and metadata
- F IssueDownloadUrl: signed URLs and content delivery
- F ManageShareLinks: elevated external sharing
- F ManageFolders: folder tree and access rules
- F ExportFiles: folder or selection archive
- F SearchAndOcr: OCR and full-text search (Tier 2)
- F ManageTemplates: designer: create, draft, preview, publish, retire, delete
- F GenerateDocument: on-demand generation from staff
- F ListGeneratedDocuments: an owner's documents
- F RenderBatches: batch layouts such as ID cards (REQ-DOC-010)
- F ManageCertificates: register, revoke, reissue, export
- F VerifyCertificate: the public verification page
- F LegacyImportWithDryRunAndRollback: WF-DATA-01 transitions started by a person
- F ManageImportMappings: legacy adapters and mappings
- F SensitiveExportApproval: WF-PRV-02 transitions
- F Jobs: the job resource of document 22 §6
- F MemoryBooks: school memory (Tier 2)
- F RequestEffects: Saga commands Documents receives
- F TenantLifecycle: Saga 1, 2 and 10 handlers every data-owning service carries
Consumers: 18
- C TenantProvisioningConsumer: platform.tenant.provisioning-requested.v1
- C TenantProvisionedConsumer: platform.tenant.provisioned.v1 seeds the branding copy
- C TenantStatusConsumer: platform.tenant.suspended.v1 and reactivated.v1
- C TenantDeletionConsumer: platform.tenant.deletion-requested.v1 and deleted.v1
- C PlatformContextConsumer: plan, feature-flag and terminology changes
- C SettingsChangedConsumer: platform.settings.changed.v1 for branding and security scopes
- C CustomFieldChangedConsumer: platform.custom-field.changed.v1 adds placeholder keys
- C DataQualityIssueConsumer: reporting.data-quality.issue-detected.v1 for Documents entity types
- C AccountRestrictionConsumer: finance.account.restricted.v1 and cleared.v1
- C ApplicationSubmittedConsumer: admissions.application.submitted.v1 registers required documents
- C OfferSubjectConsumer: admissions.offer.made.v1 and accepted.v1
- C GradesLockedConsumer: assessment.grades.locked.v1
- C GradeChangeApprovedConsumer: assessment.grade-change.approved.v1 marks supersession
- C InvoiceRunRequestedConsumer: finance.invoice-run.requested.v1 opens a batch counter
- C InvoiceIssuedConsumer: finance.invoice.issued.v1 registers the subject
- C FinanceCorrectionConsumer: finance.refund.processed.v1 and credit-note.issued.v1
- C BadgeAwardedConsumer: behavior.badge.awarded.v1
- C RequestApprovedConsumer: requests.request.approved.v1 shows the pending document
Jobs: 4
- J GetJobQuery: record: job id
- J GetJobHandler: job state from the table and progress from redis-state
- J CancelJobHandler: sets cancelRequested
- J JobEndpoints: GET /jobs/{id}, /jobs/{id}/items, POST /jobs/{id}/cancel

## finance

Use cases (Application/Features/): 20
- F ManageFeeItems: /fee-items list, get, create, patch, delete
- F ManageFeeStructures: /fee-structures including publish and new-version
- F FeePlanToCollectionAndEscalation: WF-FIN-01, one sub-folder per transition command (document 31)
- F InvoiceReversalCreditNoteAndRefund: WF-FIN-02 transition commands
- F ChequeReceiptAndBounce: WF-FIN-03 transition commands
- F ScholarshipAward: WF-FIN-04 transition commands
- F PayerChangeToSponsor: WF-FIN-05 transition commands
- F CashierDayClose: WF-FIN-06 transition commands
- F ManageDiscounts: definitions and assignments
- F Accounts: student account, statements, clearance list
- F Reports: the eight reports, exports, accountant home, reconciliation view
- F AccountingExport: period export and e-invoicing hand-off
- F AssignFeePlan: Saga 3 step 3 command and its compensation
- F AssignNextYearFeePlans: Saga 4 step 6
- F FinanceClearance: Saga 5 step 1
- F RequestFee: Saga 6 step 1
- F RequestEffects: Saga 6 Finance effects
- F ImportBatch: Saga 9 target-service steps for opening balances
- F TenantLifecycle: Saga 1, 2, 10 commands every service handles
- F Jobs: job resource, items and cancel
Consumers: 22
- C TenantLifecycleConsumer: platform tenant, plan, flag, terminology and custom-field keys
- C SettingsChangedConsumer: platform.settings.changed.v1 into ref_settings
- C PermissionCacheConsumer: identity.role.changed.v1 and identity.permissions.changed.v1
- C DataQualityIssueConsumer: reporting.data-quality.issue-detected.v1 for Finance entity types
- C AcademicYearOpenedConsumer: school.academic-year.opened.v1
- C AcademicYearClosedConsumer: school.academic-year.closed.v1
- C TermStartedConsumer: school.term.started.v1
- C StudentEnrolledConsumer: school.student.enrolled.v1; releases held payments
- C StudentStatusChangedConsumer: school.student.status-changed.v1; leaver pro-rata
- C StudentPromotedConsumer: school.student.promoted.v1
- C StudentProfileUpdatedConsumer: school.student.profile-updated.v1
- C GuardianLinkConsumer: school.guardian.updated.v1 and identity.guardian-link.created.v1
- C OfferAcceptedConsumer: admissions.offer.accepted.v1
- C ReEnrollmentConsumer: admissions.re-enrollment.confirmed.v1 and declined.v1
- C OfferMadeConsumer: admissions.offer.made.v1 deposit invoice
- C PayrollInputsReadyConsumer: hr.payroll.inputs-ready.v1
- C TransportSubscriptionChangedConsumer: operations.transport.subscription-changed.v1
- C LibraryFineConsumer: operations.library.loan-overdue.v1
- C ActivityFeeConsumer: operations.activity.enrollment-confirmed.v1
- C ImportCompletedConsumer: documents.import.completed.v1
- C RequestApprovedConsumer: requests.request.approved.v1 status display
- C DocumentGeneratedConsumer: documents.document.generated.v1 on the bulk saga-outcomes queue
Jobs: 4
- J JobsRequests: get, items, cancel records
- J JobsHandler: reads IJobStore
- J JobsValidator: starter or platform.jobs.view
- J JobsEndpoint: /jobs routes

## gateway

Use cases (Application/Features/): 0
Consumers: 0
Jobs: 0

## hr

Use cases (Application/Features/): 28
- F ManageStaffFiles: /staff-files list, get, create, patch
- F ExportStaffFiles: /staff-files/export
- F TeachingEligibility: the check Bff.Web makes before an assignment
- F QualificationsAndDevelopment: qualifications and PD records
- F TeachingLicenceExpiryCompliance: WF-HR-03, one sub-folder per human transition
- F ManageContracts: contract drafts, submit, document, signed copy
- F ApproveContract: four-eyes approval and ending
- F PayTerms: salary, components and bank account
- F ManageLeaveTypes: /leave-types
- F LeaveBalances: balances, ledger, adjustments
- F LeaveCalendar: calendar and staff on leave
- F StaffLeaveToSubstitution: WF-HR-01, one sub-folder per transition command
- F OvertimeEntries: /overtime
- F AdvancesAndLoans: /advances
- F SalaryComponents: /salary-components
- F PayrollInputCycle: WF-HR-04, one sub-folder per transition command
- F Payslips: generate and read
- F AppraisalCycles: cycles
- F Appraisals: goals, self-review, feedback, improvement plan
- F RecordObservation: classroom observations and rubrics
- F FinalizeAppraisal: finalize and publish
- F StaffHiringToOnboarding: WF-HR-02, one sub-folder per transition command
- F Offboarding: offboarding cases and clearance
- F HrHome: the HR officer's Today
- F Workload: cover and leave totals
- F ImportBatches: Saga 9 target commands
- F TenantLifecycle: Saga 1, 2, 10 commands every service handles
- F Jobs: job resource and cancel
Consumers: 10
- C TenantLifecycleConsumer: platform tenant, plan, flag, terminology and custom-field keys
- C SettingsChangedConsumer: platform.settings.changed.v1
- C PermissionsChangedConsumer: identity.role.changed.v1 and identity.permissions.changed.v1
- C DataQualityIssueConsumer: reporting.data-quality.issue-detected.v1
- C StaffCreatedConsumer: school.staff.created.v1; no-op for an Hr hire
- C StaffLeftConsumer: school.staff.left.v1; offboarding and contract end
- C SubstitutionAssignedConsumer: scheduling.substitution.assigned.v1
- C RequestApprovedConsumer: requests.request.approved.v1 for HR types without a command
- C ImportCompletedConsumer: documents.import.completed.v1
- C DocumentGeneratedConsumer: documents.document.generated.v1 once bound
Jobs: 4
- J JobsRequests: get and cancel records
- J JobsHandler: reads IJobStore
- J JobsValidator: starter or platform.jobs.view
- J JobsEndpoint: /jobs routes

## identity

Use cases (Application/Features/): 15
- F Authentication: OpenID Connect endpoints and the hosted sign-in steps (section 5.1)
- F Me: the signed-in user's own account (section 5.2)
- F Users: administration of accounts (section 5.3)
- F Roles: roles and the permission catalog (section 5.4)
- F RoleChangeWithFourEyesApproval: WF-IDN-05, state RoleChangeWithFourEyesApprovalStatus (section 5.5)
- F DelegationDuringAbsence: WF-IDN-04, state DelegationDuringAbsenceStatus (section 5.6)
- F InvitationOrJoinCodeJoining: WF-IDN-01, state InvitationOrJoinCodeJoiningStatus (section 5.7)
- F ParentSelfRegistrationAndChildLinking: WF-IDN-02, state ParentSelfRegistrationAndChildLinkingStatus (section 5.8)
- F DuplicateAccountMerge: WF-IDN-03, state DuplicateAccountMergeStatus (section 5.9)
- F OffboardingAndAccessRevocation: WF-IDN-06, state OffboardingAndAccessRevocationStatus (section 5.9)
- F AccessReviewCampaign: WF-SEC-01, state AccessReviewCampaignStatus (section 5.10)
- F BreakGlassAccess: WF-SEC-02, state BreakGlassAccessStatus (section 5.11)
- F ConsentedImpersonation: WF-SEC-03, state ConsentedImpersonationStatus (section 5.11)
- F SecurityPolicy: effective policy, SSO connections and SCIM (section 5.12)
- F TenantLifecycle: commands from Platform's sagas and the failed-message console, no HTTP
Consumers: 18
- C TenantProvisioningRequestedConsumer: platform.tenant.provisioning-requested.v1 runs ProvisionTenant
- C TenantProvisionedConsumer: platform.tenant.provisioned.v1 marks the tenant active
- C TenantSuspendedConsumer: platform.tenant.suspended.v1 limits sign-in and key write scopes
- C TenantReactivatedConsumer: platform.tenant.reactivated.v1 restores the tenant and evicts the connection override
- C TenantDeletionRequestedConsumer: platform.tenant.deletion-requested.v1 records the cooling-off end
- C TenantDeletedConsumer: platform.tenant.deleted.v1 verifies deletion and disables orphaned persons
- C PlanChangedConsumer: platform.plan.changed.v1 refreshes entitlements for SSO and key counts
- C FeatureFlagChangedConsumer: platform.feature-flag.changed.v1 evicts the tenant context
- C SettingsChangedConsumer: platform.settings.changed.v1 for scopes security and joining
- C TerminologyChangedConsumer: platform.terminology.changed.v1 evicts terminology-bound labels
- C CustomFieldChangedConsumer: platform.custom-field.changed.v1 for entityType user only
- C DataQualityIssueConsumer: reporting.data-quality.issue-detected.v1 raises merge candidates and copy replays
- C StaffCreatedConsumer: school.staff.created.v1 upserts ref_staff and invites when Hr is absent
- C StaffLeftConsumer: school.staff.left.v1 starts offboarding
- C StaffHiredConsumer: hr.staff.hired.v1 invites the new starter
- C TeachingAssignmentChangedConsumer: academics.teaching-assignment.changed.v1 refreshes the own-sections anchor
- C OfferAcceptedConsumer: admissions.offer.accepted.v1 records the expected application
- C RequestApprovedConsumer: requests.request.approved.v1 shows the role-change effect as being applied
Jobs: 16
- J TokenSweepJob: hourly expiry of tokens, invitations and expected applications
- J CredentialHistoryJob: daily trim of password history to five
- J LoginEventPartitionJob: monthly partitions for login_events
- J JoiningTimeoutsJob: WF-IDN-01 reminders, escalation and expiry
- J GuardianClaimTimeoutsJob: WF-IDN-02 reminders, escalation and clearing
- J DelegationWindowJob: WF-IDN-04 activation and end
- J GrantProposalTimeoutsJob: WF-IDN-05 reminders, escalation and expiry
- J AccessReviewSchedulerJob: WF-SEC-01 opening, reminders, overdue handling
- J BreakGlassExpiryJob: WF-SEC-02 hard expiry and late-review suspension
- J ImpersonationExpiryJob: WF-SEC-03 consent lapse and session cap
- J MergeWindowJob: WF-IDN-03 confirmation and dismissal
- J OffboardingDeadlineJob: WF-IDN-06 escalation
- J ApiKeyUsageFlushJob: last-used writes once per key per minute
- J PermissionWarmupJob: pre-peak load of active staff permission sets
- J ReferenceCopyReconciliationJob: nightly checksums of the copies
- J UsageMeterJob: daily accounts-active meter

## notification

Use cases (Application/Features/): 14
- F ManageTemplates: /templates list, get, create, patch, delete
- F PublishTemplate: publish with plural validation
- F PreviewAndTestTemplate: preview and send-test
- F ManagePreferences: /preferences/me and support view
- F RegisterDevice: push registration and removal
- F RegisterContactEndpoint: verified address from an Identity proof
- F Inbox: feed, counts, read, read-all, snooze
- F DeliveryLog: deliveries, request status, export, resend
- F ManageSuppressions: list and lift
- F ManageChannels: channels, sender domains, credits
- F ReceiveProviderCallback: receipts, bounces, complaints
- F RequestNotification: the command other services send
- F TenantLifecycle: Saga 1, 2, 10 commands
- F Jobs: job resource and cancel
Consumers: 24
- C TenantLifecycleConsumer: the tenant-lifecycle set and platform.tenant.provisioned.v1
- C UserCopyConsumer: identity.user.activated.v1
- C RecipientLinkConsumer: identity.guardian-link.created.v1 and school.guardian.updated.v1
- C PlatformEventsConsumer: platform limit, trial, invoice and webhook keys
- C IdentityEventsConsumer: invitations, registrations, join requests, delegations, access reviews, new device, break-glass
- C SchoolEventsConsumer: student document expiry
- C AdmissionsEventsConsumer: inquiry, application, offer, re-enrolment keys
- C AcademicsEventsConsumer: assignments, submissions, lesson plans, homework load, syllabus
- C AssessmentEventsConsumer: marks, report cards, grade changes
- C SchedulingEventsConsumer: timetables, substitutions, events, bookings, exam timetables
- C AttendanceEventsConsumer: absence, excuses, thresholds, dismissal, gate passes, visitors, emergency, roll call
- C FinanceEventsConsumer: payments, cheques, refunds, restrictions
- C FinanceBulkConsumer: invoices issued and overdue on the bulk queue
- C CommunicationEventsConsumer: announcements, acknowledgments, meetings, reported messages, anonymous concerns
- C RequestsEventsConsumer: request states, SLA breaches, task assigned
- C DocumentsEventsConsumer: revocations, imports, exports, sensitive exports, scan failures
- C DocumentGeneratedConsumer: documents.document.generated.v1 on the bulk queue
- C BehaviorEventsConsumer: incidents, consequences, points, badges
- C ReportingEventsConsumer: early-warning flags and data-quality issues
- C AuditEventsConsumer: integrity-check failures
- C WellbeingEventsConsumer: clinic, medication, safeguarding, referrals, interventions; category codes only
- C HrEventsConsumer: hires, leave, balance, documents, payroll, appraisal
- C OperationsEventsConsumer: transport, library, facilities, stock, complaints, activities
- C DeliveryFailureConsumer: notification.notification.failed.v1 to platform operators
Jobs: 4
- J JobsRequests: get and cancel records
- J JobsHandler: reads IJobStore
- J JobsValidator: starter or platform.jobs.view
- J JobsEndpoint: /jobs routes

## operations

Use cases (Application/Features/): 9
- F Library: schema library
- F Transport: schema transport
- F Inventory: schema inventory
- F Facilities: schema facilities
- F FrontDesk: schema frontdesk
- F Activities: schema activities
- F Clearance: Saga 5 step 2 across library, inventory and activities
- F TenantLifecycle: Saga 1, 2, 10 commands every service handles
- F Jobs: job resource and cancel
Consumers: 12
- C TenantLifecycleConsumer: platform tenant, plan, flag, terminology and custom-field keys
- C SettingsChangedConsumer: platform.settings.changed.v1
- C PermissionsChangedConsumer: identity.role.changed.v1 and identity.permissions.changed.v1
- C DataQualityIssueConsumer: reporting.data-quality.issue-detected.v1
- C StudentReferenceConsumer: school.student.enrolled.v1 and school.student.section-changed.v1
- C StudentStatusChangedConsumer: school.student.status-changed.v1; subscriptions, holds, places
- C SectionReferenceConsumer: school.section.created.v1 and school.section.changed.v1
- C UserReferenceConsumer: identity.user.activated.v1 and identity.user.deactivated.v1
- C TimetablePublishedConsumer: scheduling.timetable.published.v1
- C RoomBookingApprovedConsumer: scheduling.room-booking.approved.v1 creates the room FacilityBooking
- C PaymentReceivedConsumer: finance.payment.received.v1
- C RequestApprovedConsumer: requests.request.approved.v1 by type code
Jobs: 4
- J JobsRequests: get and cancel records
- J JobsHandler: reads IJobStore
- J JobsValidator: starter or platform.jobs.view
- J JobsEndpoint: /jobs routes

## platform

Use cases (Application/Features/): 30
- F TenantSignupToLive: WF-PLT-01, state TenantSignupToLiveStatus (section 5.1)
- F Tenants: operator tenant administration (section 5.1)
- F SuspensionExportAndDeletion: WF-PLT-03, state SuspensionExportAndDeletionStatus (section 5.1)
- F TierMigration: Saga 10 console actions (section 5.1)
- F Domains: domains (section 5.2)
- F Branding: branding and white-label (section 5.2)
- F Plans: plan catalog (section 5.3)
- F TrialConversionAndPlanChange: WF-PLT-02, state TrialConversionAndPlanChangeStatus (section 5.3)
- F TenantInvoices: platform billing (section 5.3)
- F Usage: usage and limits (section 5.4)
- F FeatureFlags: flags and modules (section 5.5)
- F Settings: settings catalog and values (section 5.6)
- F Terminology: terminology (section 5.6)
- F CustomFields: custom-field definitions (section 5.6)
- F ConfigurationAsCode: export, diff and import (section 5.6)
- F TemplateLibrary: global library and exchange (section 5.7)
- F Announcements: announcements, maintenance, release notes (section 5.8)
- F Support: support desk and customer success (section 5.9)
- F Legal: legal documents and acceptance (section 5.10)
- F PrivacyAdministration: DPIA, dashboard, retention, holds, recycle-bin policy (section 5.10)
- F DataSubjectAccessRequest: WF-PRV-01, state DataSubjectAccessRequestStatus (section 5.10)
- F OperationsConsole: jobs, sagas, failed messages, health board (section 5.11)
- F OnPremisesUpgradeWithRollback: WF-INF-01, state OnPremisesUpgradeWithRollbackStatus (section 5.12)
- F ReleaseRolloutWithCanaryAndRollback: WF-INF-02, state ReleaseRolloutWithCanaryAndRollbackStatus (section 5.12)
- F RestoreAndFailoverDrill: WF-INF-03, state RestoreAndFailoverDrillStatus (section 5.12)
- F ApiKeys: tenant API key console (section 5.13)
- F Webhooks: outgoing webhooks (section 5.13, document 23 §4)
- F OneRoster: OneRoster 1.2 provider, phase 4 (section 5.13)
- F Lti: LTI 1.3 platform, phase 4 (section 5.13)
- F Providers: plug-in selection and configuration (section 5.13)
Consumers: 15
- C UsageRecordedConsumer: <service>.usage.recorded.v1 from every exchange
- C PermissionsChangedConsumer: identity.role.changed.v1 and identity.permissions.changed.v1
- C DataQualityIssueConsumer: reporting.data-quality.issue-detected.v1 for entities owned here
- C NotificationOutcomeConsumer: notification.notification.delivered.v1 and failed.v1 for deliverability
- C IntegrityCheckFailedConsumer: audit.integrity-check.failed.v1 raises the alarm and a Sev1 ticket
- C AiIndexRebuiltConsumer: ai.index.rebuild-completed.v1 for the console
- C ProjectionRebuildCompletedConsumer: reporting.projection.rebuild-completed.v1, Saga 1 step 6
- C AuditPartitionDetachedConsumer: audit.retention.partition-detached.v1, Saga 2 step 8
- C OwnerInvitedConsumer: identity.user.invited.v1, Saga 1 step 3
- C AcademicYearOpenedConsumer: school.academic-year.opened.v1, Saga 1 step 4
- C ExportCompletedConsumer: documents.export.completed.v1 for Saga 2, WF-PRV-01 and tenant export
- C DocumentGeneratedConsumer: documents.document.generated.v1 for certificates, letters and PDFs
- C RequestApprovedConsumer: requests.request.approved.v1 for the subject-request effect
- C WebhookFanOutConsumer: the 54 eligible keys of document 23 §4.9
- C OneRosterCopyConsumer: School and Academics events for the OneRoster copies
Jobs: 22
- J PlanLimitAndTrialCheckJob: daily limits and trial reminders
- J TenantBillingRunJob: invoices on each billing date
- J DunningJob: 7, 14, 30 days and read-only
- J UsageAggregationJob: daily roll-up to usage_monthly
- J UsageRecountJob: monthly recount through every service
- J PrePeakWarmupJob: pre-peak cache warm-up per tenant time zone
- J HealthScoreJob: adoption metrics and churn-risk flags
- J SupportSlaJob: SLA clocks and escalation
- J WebhookChallengeJob: verification attempts
- J WebhookEndpointStateJob: failing and disabled rules, overlap ends
- J PartitionMaintenanceJob: usage and attempt partitions
- J ApiKeyExpiryReminderJob: 14 and 3 days before key expiry
- J ApiKeyRetentionJob: per-key usage history purge
- J SandboxLifecycleJob: idle notices and deletion
- J SubjectRequestDeadlineJob: WF-PRV-01 clocks
- J DomainVerificationJob: DNS and certificate status
- J MaintenanceWindowJob: window boundaries
- J FailedMessageIndexJob: refresh of the parking index
- J FailedMessageAutoReplayJob: transient replays after recovery
- J SagaDeadlineJob: overdue and stuck saga alerts
- J DeletionCoolingOffReminderJob: daily owner reminder
- J OneRosterReconciliationJob: nightly roster copy checksums

## reporting

Use cases (Application/Features/): 15
- F GetTodayCards: the role Today dashboard
- F GetIndicators: key indicators, cohorts and the group view
- F GetMorningBrief: the brief, the homeroom card and preferences
- F ExplainFigure: explain this number
- F GetStudent360: the Student 360 read model
- F ManageReports: builder: create, edit, delete, share, schedule
- F RunReport: library runs, preview, fields
- F ExportReport: exports and regulatory runs through Documents
- F EarlyWarning: flags and the Because panel
- F DataQuality: the Data Quality Center
- F Projections: freshness and the rebuild command
- F Jobs: the job resource
- F OpenData: the open data API
- F Governance: Tier 2 records
- F TenantLifecycle: Saga 1, 2 and 10 handlers
Consumers: 2
- C TenantLifecycleConsumer: platform tenant keys
- C SettingsChangedConsumer: platform.settings.changed.v1 and custom fields
Jobs: 2
- J GetJobHandler: job state and progress
- J JobEndpoints: GET /jobs/{id}, POST /jobs/{id}/cancel

## requests

Use cases (Application/Features/): 16
- F DesignRequestTypes: /request-types list, available, create, patch, draft version, retire, delete
- F PublishRequestTypeVersion: publish a version
- F SeedRequestCatalog: Section 11.2 templates
- F ManageForms: /forms list, create, draft, publish, versions, preview, delete
- F ManageApprovalChains: /approval-chains including simulate
- F ManageSla: /sla-policies and /sla-calendars
- F ServiceRequestLifecycle: WF-RQS-01, one sub-folder per transition command (document 31)
- F RequestViews: list, get, timeline, board, export, summary document
- F RequestComments: comments and internal notes
- F CalendarImpact: leave overlap preview
- F RequestAnalytics: REQ-RQS-010
- F ManageTasks: personal and assigned tasks
- F CompleteTask: first completion wins
- F DutyRosters: Tier 2 rosters and swaps
- F TenantLifecycle: Saga 1, 2, 10 commands every service handles
- F Jobs: job resource and cancel
Consumers: 12
- C TenantLifecycleConsumer: platform tenant, plan, flag, terminology and custom-field keys
- C SettingsChangedConsumer: platform.settings.changed.v1
- C ApproverRoleConsumer: identity.role.changed.v1 and identity.permissions.changed.v1
- C DataQualityIssueConsumer: reporting.data-quality.issue-detected.v1
- C UserLifecycleConsumer: identity.user.activated.v1 and identity.user.deactivated.v1
- C DelegationConsumer: identity.delegation.started.v1 and identity.delegation.ended.v1
- C StudentReferenceConsumer: school.student.enrolled.v1 and school.student.status-changed.v1
- C StaffReferenceConsumer: school.staff.created.v1 and school.staff.left.v1; leaver reassignment
- C JoinRequestTaskConsumer: identity.join-request.submitted.v1
- C AccessReviewTaskConsumer: identity.access-review.due.v1
- C DocumentExpiryTaskConsumer: school.student-document.expiring.v1 and hr.staff-document.expiring.v1
- C ComplaintReceivedConsumer: operations.frontdesk.complaint-received.v1
Jobs: 4
- J JobsRequests: get and cancel records
- J JobsHandler: reads IJobStore
- J JobsValidator: starter or platform.jobs.view
- J JobsEndpoint: /jobs routes

## scheduling

Use cases (Application/Features/): 16
- F ManageBellSchedules: bell schedules, periods, special assignments
- F ManageConstraints: constraints, travel times, lesson requirements
- F ManageTimetableVersions: versions, copies, comparison, delete
- F GenerateTimetable: starts a solve
- F EditTimetableEntries: place, move, delete, lock, unlock, conflicts
- F PublishTimetable: publish and change sets
- F ExportTimetable: CSV export
- F GetTimetableViews: section, staff, room and campus-day views
- F ManageIcalFeeds: subscriptions and the feed
- F ManageStaffAbsences: manual absences
- F AssignSubstitution: suggestions, assign, accept, release, notes, fairness
- F ManageCalendar: calendar events and RSVPs
- F ManageRoomBookings: bookings and availability
- F ManageExamTimetable: sessions, generation, publish, seating, invigilators, export
- F CopyTimetableSkeleton: Saga 4 step 7 and its compensation
- F GetJob: job resource and cancel
Consumers: 13
- C TenantLifecycleConsumer: tenant rows, read-only, cache eviction
- C AcademicYearOpenedConsumer: school.academic-year.opened.v1
- C AcademicYearClosedConsumer: school.academic-year.closed.v1 archives versions
- C TermStartedConsumer: school.term.started.v1 opens the cover ledger term
- C SectionCreatedConsumer: school.section.created.v1
- C SectionChangedConsumer: school.section.changed.v1
- C StaffCreatedConsumer: school.staff.created.v1
- C StaffLeftConsumer: school.staff.left.v1 uncovers future entries
- C TeachingAssignmentChangedConsumer: academics.teaching-assignment.changed.v1
- C LeaveApprovedConsumer: hr.leave.approved.v1 creates absences and suggestions
- C LeaveCancelledConsumer: hr.leave.cancelled.v1 releases covers
- C MeetingBookedConsumer: communication.meeting.booked.v1 blocks the slot
- C RequestApprovedConsumer: requests.request.approved.v1 processing badge
Jobs: 6
- J DailyCoverPlanningJob: 05:30 cover planning
- J UncoveredPeriodEscalationJob: 30-minute principal alert
- J SkeletonCopyJob: rollover skeleton copy with progress
- J ReferenceCopyReconciliationJob: nightly reconciliation
- J RoomAndCalendarSnapshotJob: nightly room and holiday snapshot
- J UsageFlushJob: usage meters

## school

Use cases (Application/Features/): 35
- F ManageSchoolProfile: profile and marks, section 5.1
- F SetupChecklist: wizard checklist and sample data
- F ManageNumberingFormats: student and staff numbering
- F ManageCampuses: campuses and buildings
- F ManageRooms: rooms
- F ManageAcademicYears: years and set-current
- F ManageTerms: terms, grading periods, calendar days
- F ManageStructure: stages, grade levels, subjects, departments, houses
- F ManageSections: sections and capacity
- F GetSectionRoster: the roster read
- F BalancedSectionFormation: Tier 2 formation
- F SearchStudents: student list and search
- F GetStudent: student profile read
- F CreateStudent: direct creation with enrolment
- F UpdateStudentProfile: profile edits, photo, consent, documents, notes, siblings, alumni
- F DeleteStudent: deletion of a record created in error
- F ReadSensitiveStudentData: identifiers, custody, medical summary
- F ChangeStudentStatus: status transitions outside the sagas
- F ChangeSection: section change, REST and effect command
- F BulkStudentOperations: bulk section, status and promotion
- F PrintIdCards: ID card batch
- F ImportStudentPhotos: photo upload by file name
- F ExportStudents: streamed exports of students, guardians, staff
- F MergeStudents: duplicates and merge
- F ManageGuardians: guardians, links, emergency contacts
- F ManageStaff: staff profiles and departure
- F TransferOrWithdrawalWithClearance: WF-SCH-01 transitions
- F MidYearCampusTransfer: WF-SCH-04 transitions
- F EndOfYearCloseAndRollover: WF-SCH-02 human transitions
- F YearArchivalAndReopen: WF-SCH-03 transitions
- F EnrolStudent: Saga 3 step 2 command and its compensation
- F UpdateGuardianDetails: Saga 6 effect after parent request approval
- F ImportBatch: Saga 9 target-service steps
- F OpenFirstAcademicYear: Saga 1 step 4
- F GetJob: job resource, items and cancel
Consumers: 10
- C TenantLifecycleConsumer: provisioning, suspension, settings, custom fields, data-quality flags
- C UserRegisteredConsumer: identity.user.registered.v1 duplicate-person check
- C JoinRequestApprovedConsumer: identity.join-request.approved.v1 sets user links
- C GuardianLinkCreatedConsumer: identity.guardian-link.created.v1 verifies the link
- C OfferAcceptedConsumer: admissions.offer.accepted.v1 pre-reserves a number
- C ReEnrollmentConfirmedConsumer: admissions.re-enrollment.confirmed.v1 return intent
- C ReEnrollmentDeclinedConsumer: admissions.re-enrollment.declined.v1 return intent
- C StaffHiredConsumer: hr.staff.hired.v1 creates the staff record
- C ImportCompletedConsumer: documents.import.completed.v1 seals and evicts
- C RequestApprovedConsumer: requests.request.approved.v1 processing badge
Jobs: 16
- J DocumentExpiryScanJob: weekly expiry scan
- J TermStartJob: daily term start
- J RolloverApplyJob: Saga 4 batch of 200 with checkpoint
- J RolloverStallMonitorJob: 15-minute stall alert
- J RolloverDecisionEscalationJob: 14-day decision escalation
- J ClearanceEscalationJob: 10-day and 60-day clearance rules
- J YearArchivalJob: snapshot and freeze
- J ReopenWindowCloseJob: closes reopen windows
- J CampusTransferEffectiveJob: effective dates, lapses, reminders
- J BulkStudentOperationJob: bulk section, status, promotion
- J IdCardBatchJob: ID card generation fan-out
- J PhotoImportJob: photos by file name
- J SectionFormationJob: Tier 2 balancing
- J SampleDataJob: demo data through normal commands
- J StudentRetentionJob: REQ-PRV-003 retention
- J UsageFlushJob: usage meters

## wellbeing

Use cases (Application/Features/): 11
- F ClinicVisitToSentHome: WF-WEL-02 transitions
- F MedicalAlerts: allergy badge, immunizations, screenings
- F MedicationAuthorizationAndAdministration: WF-WEL-03 transitions
- F Counseling: referrals and cases
- F SafeguardingConcernEscalation: WF-WEL-04 transitions
- F AccommodationPlanToExamSitting: WF-WEL-01 transitions
- F Interventions: interventions and playbooks
- F DailyWellbeingCheckInEscalation: WF-WEL-05 transitions
- F Access: break-glass and visibility
- F RequestEffects: Saga 6 commands
- F TenantLifecycle: Saga 1, 2 and 10 handlers
Consumers: 14
- C StudentStatusChangedConsumer: school.student.status-changed.v1 for students with a record
- C StudentProfileUpdatedConsumer: school.student.profile-updated.v1 for students with a record
- C GuardianLinkConsumer: school.guardian.updated.v1 and identity.guardian-link.created.v1
- C SectionConsumer: school.section.created.v1 and changed.v1
- C MessageReportedConsumer: communication.message.reported.v1 opens a concern
- C AnonymousConcernConsumer: communication.concern.reported-anonymously.v1, no reporter stored
- C OperatorBreakGlassConsumer: identity.break-glass.used.v1, operator refused and logged
- C AttendanceThresholdConsumer: attendance.threshold.reached.v1 suggests an intervention
- C StudentAbsentConsumer: attendance.student.absent.v1 for watched students only
- C BehaviorIncidentConsumer: behavior.incident.recorded.v1 for serious or restricted incidents
- C EarlyWarningFlagConsumer: reporting.early-warning.flag-raised.v1 suggests an intervention
- C EarlyWarningClearedConsumer: reporting.early-warning.flag-cleared.v1 withdraws a suggestion
- C RequestApprovedConsumer: requests.request.approved.v1 for medication and accommodation types
- C TenantLifecycleConsumers: the lifecycle set of document 11 §2.3
Jobs: 15
- J SafeguardingTriageEscalationJob: triage deadlines
- J MonitoredConcernReviewJob: 14-day reviews
- J GuardianContactEscalationJob: 15-minute contact and 60-minute collection
- J MedicationScheduleJob: daily dose list
- J MissedDoseJob: missed doses and alerts
- J MedicationAuthorizationExpiryJob: one-term expiry
- J ConsentChaseJob: 7 and 14 days
- J EducationPlanReviewJob: plan review dates
- J CheckInWindowCloseJob: skips and silence flags
- J CheckInFlagEscalationJob: 2-hour and 30-minute escalations
- J BreakGlassExpiryJob: window end and review task
- J WellbeingRetentionJob: retention with holds and key rotation
- J ReferenceCopyReconciliationJob: nightly copies
- J TenantKeyRewrapJob: quarterly re-wrap
- J WellbeingUsageMeterJob: daily counts
