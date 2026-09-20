# Appendix F. Core Entities per Service

> Part of `docs/brief/02-appendices/`. Normative. Read with the master brief and the reference architecture.

- **Identity:** User, Credential, Role, Permission, RoleAssignment (with scope and validity), Delegation, Invitation, JoinRequest, Session, Device, ApiKey, AccessReview
- **Platform:** Tenant, Domain, Plan, Subscription, UsageRecord, FeatureFlag, Branding, TenantInvoice, SupportTicket
- **School:** Campus, Building, Room, AcademicYear, Term, GradingPeriod, Stage, GradeLevel, Section, Subject, Department, House, Student, Guardian, StudentGuardian (rights), Enrollment, StatusChange, StaffMember, CustomFieldDefinition and Value
- **Admissions:** Inquiry, Application, ApplicationStage, Assessment, Interview, Offer, WaitingListEntry, ReEnrollmentCampaign and Response
- **Academics:** TeachingAssignment, StudentGroup, CurriculumUnit, Outcome, LessonPlan, Assignment, Rubric, Submission, Feedback, Resource, Question, Quiz, Attempt
- **Assessment:** AssessmentStructure, Component, Mark, GradeScheme and Version, ModerationRecord, TermResult, ReportCardTemplate, ReportCard and Version, Transcript, GradeChangeRequest
- **Scheduling:** BellSchedule, Period, Constraint, TimetableVersion, TimetableEntry, Substitution, CalendarEvent, RoomBooking, ExamSession, SeatingPlan
- **Attendance and Safety:** AttendanceSession, AttendanceRecord, Excuse, ThresholdRule, PickupPerson, GatePass, Visitor, EmergencyBroadcast, Acknowledgment
- **Finance:** FeeItem, FeeStructure, FeePlan, Installment, Invoice and Line, Payment and Allocation, Refund, CreditNote, Discount, Scholarship, Payer, CashierShift, Series
- **Communication:** Announcement, Audience, Acknowledgment, Conversation, Message, MessagingPolicy, MeetingSlot, Booking, Survey and Response
- **Notification:** Template, Preference, NotificationRequest, Delivery, Digest
- **Requests:** RequestType and Version, FormDefinition, ApprovalChain and Step, Request, ApprovalDecision, SlaPolicy, EffectDefinition
- **Documents:** StoredFile, DocumentTemplate, GeneratedDocument, Certificate, VerificationToken, ImportJob, ExportJob
- **Behavior, Wellbeing, HR, Operations:** as implied by Appendix A, designed in Phase 0
- **Reporting:** projections only; no entity here is a source of truth
- **Audit:** AuditEntry (hash-chained), LoginEvent, AccessLogEntry for sensitive records

---
