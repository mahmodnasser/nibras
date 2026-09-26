# 31. Business Rules and Workflows, Assigned to Code

## Purpose

Appendix S states what the product computes and Appendix R states how its processes move. This document says **where each one lives in the code, what test proves it, and in which phase it is built.** It is generated from the two appendices and from the build-phase column of `05-service-catalog.md`, so a rule or workflow cannot be added to an appendix and silently miss an owner here.

Saga designs for the multi-service workflows are in `13-workflows-and-sagas.md`. This document does not repeat them.

## Scope

| In scope | Not in scope |
|---|---|
| Every `BR-` identifier in Appendix S, with its owning service, test class, implementation type and phase | The wording of the rules and their worked examples, which Appendix S owns |
| Every `WF-` identifier in Appendix R, with its state type, feature folder and phase | Saga orchestration, which document 13 owns |
| The implementation contract, the test conventions, and the mutation-testing targets | Test infrastructure, which document 16 owns |

## Content

### 1. Summary

| Service | Rules | Workflows | Build phase |
|---|---|---|---|
| Academics | 0 | 1 | 2 |
| Admissions | 6 | 2 | 4 |
| Assessment | 14 | 3 | 2 |
| Attendance | 11 | 2 | 2 |
| Behavior | 0 | 1 | 4 |
| Documents | 0 | 2 | 3 |
| Finance | 20 | 6 | 3 |
| Hr | 0 | 4 | 5 |
| Identity | 9 | 9 | 1 |
| Notification | 7 | 0 | 1 |
| Operations | 0 | 5 | 5 |
| Platform | 9 | 7 | 1 |
| Requests | 6 | 1 | 3 |
| Scheduling | 8 | 0 | 2 |
| School | 1 | 4 | 2 |
| Wellbeing | 4 | 5 | 5 |
| **Total** | **95** | **52** | |

The service table quotes each service's build phase from `05-service-catalog.md`, which quotes master brief Section 28. The Phase column of the rule and workflow tables below is the earliest phase of a `34-work-breakdown.md` slice whose Covers column names the identifier, so a rule a phase 1 slice builds reads phase 1 even when its owning service arrives later; an identifier no slice names falls back to its service's phase.

### 2. Business rules

**How to read the implementation column.** Each rule is one class in the owning service's Domain project, under `Rules/`, named after its test class. A rule that needs data from outside its aggregate receives it as parameters; it never reaches into a repository. Where a rule is enforced by an Application handler rather than a pure domain type, the handler calls the domain rule; the rule still lives in one place.

| Rule | Name | Owner | Test class | Implementation | Parameters (Appendix G) | Property-based | Phase |
|---|---|---|---|---|---|---|---|
| `BR-ASM-001` | Weighted category average | Assessment | `WeightedCategoryAverageRulesTests` | `Nibras.Assessment.Domain.Rules.WeightedCategoryAverageRule` | Academic → grading schemes | yes | 2 |
| `BR-ASM-002` | Empty category re-weight | Assessment | `CategoryReweightRulesTests` | `Nibras.Assessment.Domain.Rules.CategoryReweightRule` | Academic → grading schemes | yes | 2 |
| `BR-ASM-003` | Drop the lowest scores | Assessment | `DropLowestRulesTests` | `Nibras.Assessment.Domain.Rules.DropLowestRule` | Academic → grading schemes | yes | 2 |
| `BR-ASM-004` | Best of N | Assessment | `BestOfNRulesTests` | `Nibras.Assessment.Domain.Rules.BestOfNRule` | Academic → grading schemes | yes | 2 |
| `BR-ASM-005` | Mandatory component missing | Assessment | `MandatoryComponentRulesTests` | `Nibras.Assessment.Domain.Rules.MandatoryComponentRule` | Academic → grading schemes | yes | 2 |
| `BR-ASM-006` | Late joiner partial-term re-weighting | Assessment | `LateJoinerReweightRulesTests` | `Nibras.Assessment.Domain.Rules.LateJoinerReweightRule` | Academic → grading schemes | yes | 2 |
| `BR-ASM-007` | Absent versus exempt components | Assessment | `AbsentAndExemptComponentRulesTests` | `Nibras.Assessment.Domain.Rules.AbsentAndExemptComponentRule` | Academic → grading schemes | yes | 2 |
| `BR-ASM-008` | Rounding at a configurable number of decimals | Assessment | `GradeRoundingRulesTests` | `Nibras.Assessment.Domain.Rules.GradeRoundingRule` | Academic → rounding | yes | 2 |
| `BR-ASM-009` | Letter grade boundaries | Assessment | `LetterGradeBoundaryRulesTests` | `Nibras.Assessment.Domain.Rules.LetterGradeBoundaryRule` | Academic → grading schemes | yes | 2 |
| `BR-ASM-010` | GPA scale conversion | Assessment | `GpaConversionRulesTests` | `Nibras.Assessment.Domain.Rules.GpaConversionRule` | Academic → grading schemes | yes | 2 |
| `BR-ASM-011` | Rank and ties | Assessment | `RankRulesTests` | `Nibras.Assessment.Domain.Rules.RankRule` | Academic → rank visibility | yes | 2 |
| `BR-ASM-012` | Promotion eligibility | Assessment | `PromotionEligibilityRulesTests` | `Nibras.Assessment.Domain.Rules.PromotionEligibilityRule` | Academic → pass marks, promotion rules | yes | 2 |
| `BR-ASM-013` | Honors thresholds | Assessment | `HonorsThresholdRulesTests` | `Nibras.Assessment.Domain.Rules.HonorsThresholdRule` | Academic → promotion rules | yes | 2 |
| `BR-ASM-014` | Grade change after lock creates a new version | Assessment | `GradeChangeAfterLockRulesTests` | `Nibras.Assessment.Domain.Rules.GradeChangeAfterLockRule` | Academic → publish windows | no | 2 |
| `BR-ATT-001` | Daily versus per-period derivation | Attendance | `AttendanceDerivationRulesTests` | `Nibras.Attendance.Domain.Rules.AttendanceDerivationRule` | Attendance → mode, codes | no | 2 |
| `BR-ATT-002` | Attendance lock window | Attendance | `AttendanceLockWindowRulesTests` | `Nibras.Attendance.Domain.Rules.AttendanceLockWindowRule` | Attendance → lock window | no | 2 |
| `BR-ATT-003` | Approved leave pre-fills excused | Attendance | `ApprovedLeaveExcuseRulesTests` | `Nibras.Attendance.Domain.Rules.ApprovedLeaveExcuseRule` | Attendance → excuse rules, codes | no | 2 |
| `BR-ATT-004` | Late converts to absent | Attendance | `LateToAbsentRulesTests` | `Nibras.Attendance.Domain.Rules.LateToAbsentRule` | Attendance → cut-off times, codes | no | 2 |
| `BR-ATT-005` | Late accumulation adds a derived absence | Attendance | `LateAccumulationRulesTests` | `Nibras.Attendance.Domain.Rules.LateAccumulationRule` | Attendance → thresholds and ladder | no | 2 |
| `BR-ATT-006` | Consecutive absence threshold | Attendance | `ConsecutiveAbsenceRulesTests` | `Nibras.Attendance.Domain.Rules.ConsecutiveAbsenceRule` | Attendance → thresholds and ladder | no | 2 |
| `BR-ATT-007` | Cumulative absence ladder | Attendance | `CumulativeAbsenceLadderRulesTests` | `Nibras.Attendance.Domain.Rules.CumulativeAbsenceLadderRule` | Attendance → thresholds and ladder | no | 2 |
| `BR-ATT-008` | Attendance percentage denominator | Attendance | `AttendancePercentageRulesTests` | `Nibras.Attendance.Domain.Rules.AttendancePercentageRule` | Attendance → excuse rules | yes | 2 |
| `BR-ATT-009` | Mid-term section move splits the record | Attendance | `SectionMoveAttendanceRulesTests` | `Nibras.Attendance.Domain.Rules.SectionMoveAttendanceRule` | none | no | 2 |
| `BR-ATT-010` | Offline mark arriving after the lock window | Attendance | `OfflineAttendanceSyncRulesTests` | `Nibras.Attendance.Domain.Rules.OfflineAttendanceSyncRule` | Attendance → lock window | no | 2 |
| `BR-ATT-011` | Unmarked class reminder | Attendance | `UnmarkedClassReminderRulesTests` | `Nibras.Attendance.Domain.Rules.UnmarkedClassReminderRule` | Attendance → cut-off times | no | 2 |
| `BR-FIN-001` | Fee plan installment generation | Finance | `InstallmentGenerationRulesTests` | `Nibras.Finance.Domain.Rules.InstallmentGenerationRule` | General → currency | yes | 3 |
| `BR-FIN-002` | Pro-rata by days | Finance | `ProRataByDaysRulesTests` | `Nibras.Finance.Domain.Rules.ProRataByDaysRule` | Finance → pro-rata mode | yes | 3 |
| `BR-FIN-003` | Pro-rata by months | Finance | `ProRataByMonthsRulesTests` | `Nibras.Finance.Domain.Rules.ProRataByMonthsRule` | Finance → pro-rata mode | yes | 3 |
| `BR-FIN-004` | Discount stacking order | Finance | `DiscountStackingRulesTests` | `Nibras.Finance.Domain.Rules.DiscountStackingRule` | Finance → discount rules | yes | 3 |
| `BR-FIN-005` | Sibling discount eligibility | Finance | `SiblingDiscountRulesTests` | `Nibras.Finance.Domain.Rules.SiblingDiscountRule` | Finance → discount rules | yes | 3 |
| `BR-FIN-006` | Scholarship cap | Finance | `ScholarshipCapRulesTests` | `Nibras.Finance.Domain.Rules.ScholarshipCapRule` | Finance → discount rules | yes | 3 |
| `BR-FIN-007` | Late fee accrual and cap | Finance | `LateFeeAccrualRulesTests` | `Nibras.Finance.Domain.Rules.LateFeeAccrualRule` | Finance → late fee rules | yes | 3 |
| `BR-FIN-008` | Payment allocation order | Finance | `PaymentAllocationRulesTests` | `Nibras.Finance.Domain.Rules.PaymentAllocationRule` | Finance → allocation order | yes | 3 |
| `BR-FIN-009` | Overpayment becomes credit | Finance | `OverpaymentCreditRulesTests` | `Nibras.Finance.Domain.Rules.OverpaymentCreditRule` | Finance → allocation order | yes | 3 |
| `BR-FIN-010` | Refund from credit versus from payment | Finance | `RefundSourceRulesTests` | `Nibras.Finance.Domain.Rules.RefundSourceRule` | Finance → payment methods | yes | 3 |
| `BR-FIN-011` | Rounding per currency | Finance | `CurrencyRoundingRulesTests` | `Nibras.Finance.Domain.Rules.CurrencyRoundingRule` | General → currency | yes | 1 |
| `BR-FIN-012` | Tax inclusive versus exclusive per item | Finance | `TaxInclusiveExclusiveRulesTests` | `Nibras.Finance.Domain.Rules.TaxInclusiveExclusiveRule` | Finance → tax | yes | 3 |
| `BR-FIN-013` | Gapless numbering per series under concurrency | Finance | `GaplessNumberingRulesTests` | `Nibras.Finance.Domain.Rules.GaplessNumberingRule` | Finance → numbering series | no | 3 |
| `BR-FIN-014` | Posted documents are immutable | Finance | `PostedDocumentImmutabilityRulesTests` | `Nibras.Finance.Domain.Rules.PostedDocumentImmutabilityRule` | none | yes | 3 |
| `BR-FIN-015` | Cheque bounce reversal and fee | Finance | `ChequeBounceRulesTests` | `Nibras.Finance.Domain.Rules.ChequeBounceRule` | Finance → payment methods, late fee rules | yes | 3 |
| `BR-FIN-016` | Service restriction rules | Finance | `ServiceRestrictionRulesTests` | `Nibras.Finance.Domain.Rules.ServiceRestrictionRule` | Finance → restriction rules | yes | 3 |
| `BR-FIN-017` | Active student definition for SaaS billing | Finance | `ActiveStudentCountRulesTests` | `Nibras.Finance.Domain.Rules.ActiveStudentCountRule` | none | no | 1 |
| `BR-FIN-018` | Proration on a plan change | Finance | `PlanChangeProrationRulesTests` | `Nibras.Finance.Domain.Rules.PlanChangeProrationRule` | none | yes | 1 |
| `BR-FIN-019` | Split payers by percentage | Finance | `SplitPayerRulesTests` | `Nibras.Finance.Domain.Rules.SplitPayerRule` | General → currency | yes | 3 |
| `BR-SCD-001` | Hard versus soft constraints | Scheduling | `TimetableConstraintRulesTests` | `Nibras.Scheduling.Domain.Rules.TimetableConstraintRule` | none | yes | 2 |
| `BR-SCD-002` | Consecutive-period limit | Scheduling | `ConsecutivePeriodRulesTests` | `Nibras.Scheduling.Domain.Rules.ConsecutivePeriodRule` | General → work week | no | 2 |
| `BR-SCD-003` | Part-time availability and weekly load | Scheduling | `PartTimeAvailabilityRulesTests` | `Nibras.Scheduling.Domain.Rules.PartTimeAvailabilityRule` | none | no | 2 |
| `BR-SCD-004` | Travel time between campuses | Scheduling | `CampusTravelTimeRulesTests` | `Nibras.Scheduling.Domain.Rules.CampusTravelTimeRule` | none | no | 2 |
| `BR-SCD-005` | Cover fairness score | Scheduling | `CoverFairnessRulesTests` | `Nibras.Scheduling.Domain.Rules.CoverFairnessRule` | none | yes | 2 |
| `BR-SCD-006` | Publishing does not alter recorded attendance | Scheduling | `TimetablePublishEffectiveDateRulesTests` | `Nibras.Scheduling.Domain.Rules.TimetablePublishEffectiveDateRule` | Attendance → lock window | no | 2 |
| `BR-SCD-007` | Room booking holds and buffers | Scheduling | `RoomBookingRulesTests` | `Nibras.Scheduling.Domain.Rules.RoomBookingRule` | none | no | 2 |
| `BR-ADM-001` | Age eligibility by cut-off date | Admissions | `AgeEligibilityRulesTests` | `Nibras.Admissions.Domain.Rules.AgeEligibilityRule` | General → calendars | no | 4 |
| `BR-ADM-002` | Required documents by grade and nationality | Admissions | `RequiredDocumentRulesTests` | `Nibras.Admissions.Domain.Rules.RequiredDocumentRule` | none | no | 4 |
| `BR-ADM-003` | Seat capacity and override | Admissions | `SeatCapacityRulesTests` | `Nibras.Admissions.Domain.Rules.SeatCapacityRule` | none | no | 4 |
| `BR-ADM-004` | Offer expiry | Admissions | `OfferExpiryRulesTests` | `Nibras.Admissions.Domain.Rules.OfferExpiryRule` | General → time zone | no | 4 |
| `BR-ADM-005` | Waiting-list ranking with sibling priority | Admissions | `WaitingListRankingRulesTests` | `Nibras.Admissions.Domain.Rules.WaitingListRankingRule` | none | yes | 4 |
| `BR-ADM-006` | Duplicate applicant detection | Admissions | `DuplicateApplicantRulesTests` | `Nibras.Admissions.Domain.Rules.DuplicateApplicantRule` | General → languages | no | 4 |
| `BR-IDN-001` | Permission dependency | Identity | `PermissionDependencyRulesTests` | `Nibras.Identity.Domain.Rules.PermissionDependencyRule` | none | no | 1 |
| `BR-IDN-002` | Data-scope evaluation order | Identity | `DataScopeEvaluationRulesTests` | `Nibras.Identity.Domain.Rules.DataScopeEvaluationRule` | none | no | 1 |
| `BR-IDN-003` | Delegation validity window | Identity | `DelegationWindowRulesTests` | `Nibras.Identity.Domain.Rules.DelegationWindowRule` | General → time zone | no | 1 |
| `BR-IDN-004` | Four-eyes for high-risk grants | Identity | `HighRiskGrantRulesTests` | `Nibras.Identity.Domain.Rules.HighRiskGrantRule` | Security → high-risk grant approval window | no | 1 |
| `BR-IDN-005` | The last super administrator cannot be removed | Identity | `LastSuperAdminRulesTests` | `Nibras.Identity.Domain.Rules.LastSuperAdminRule` | none | no | 1 |
| `BR-IDN-006` | Join-method default role | Identity | `JoinMethodDefaultRoleRulesTests` | `Nibras.Identity.Domain.Rules.JoinMethodDefaultRoleRule` | Joining → enabled methods, default roles | no | 1 |
| `BR-IDN-007` | One person linked across tenants | Identity | `CrossTenantLinkingRulesTests` | `Nibras.Identity.Domain.Rules.CrossTenantLinkingRule` | Security → login methods | no | 1 |
| `BR-IDN-008` | Permission version invalidates caches | Identity | `PermissionVersionRulesTests` | `Nibras.Identity.Domain.Rules.PermissionVersionRule` | none | no | 1 |
| `BR-IDN-009` | Impersonation needs consent and a time box | Identity | `ImpersonationRulesTests` | `Nibras.Identity.Domain.Rules.ImpersonationRule` | Security → session timeout | no | 1 |
| `BR-NOT-001` | Urgency versus quiet hours | Notification | `QuietHoursRulesTests` | `Nibras.Notification.Domain.Rules.QuietHoursRule` | Notifications → quiet hours default | no | 1 |
| `BR-NOT-002` | Channel fallback order | Notification | `ChannelFallbackRulesTests` | `Nibras.Notification.Domain.Rules.ChannelFallbackRule` | Notifications → channel availability | no | 1 |
| `BR-NOT-003` | Digest eligibility | Notification | `DigestEligibilityRulesTests` | `Nibras.Notification.Domain.Rules.DigestEligibilityRule` | Notifications → digest schedule | no | 1 |
| `BR-NOT-004` | Deduplication window | Notification | `NotificationDeduplicationRulesTests` | `Nibras.Notification.Domain.Rules.NotificationDeduplicationRule` | Notifications → templates | no | 1 |
| `BR-NOT-005` | SMS credit check before send | Notification | `SmsCreditRulesTests` | `Nibras.Notification.Domain.Rules.SmsCreditRule` | Notifications → SMS credit limits | yes | 1 |
| `BR-NOT-006` | Preference resolution order | Notification | `PreferenceResolutionRulesTests` | `Nibras.Notification.Domain.Rules.PreferenceResolutionRule` | Notifications → channel availability | no | 1 |
| `BR-RQS-001` | Approval chain routing by amount | Requests | `AmountRoutingRulesTests` | `Nibras.Requests.Domain.Rules.AmountRoutingRule` | Requests → approval chains | yes | 3 |
| `BR-RQS-002` | Approval chain routing by duration | Requests | `DurationRoutingRulesTests` | `Nibras.Requests.Domain.Rules.DurationRoutingRule` | Requests → approval chains | no | 3 |
| `BR-RQS-003` | SLA calendars per campus | Requests | `SlaCalendarRulesTests` | `Nibras.Requests.Domain.Rules.SlaCalendarRule` | Requests → SLAs | no | 3 |
| `BR-RQS-004` | Auto-approval conditions | Requests | `AutoApprovalRulesTests` | `Nibras.Requests.Domain.Rules.AutoApprovalRule` | Requests → approval chains | yes | 3 |
| `BR-RQS-005` | Escalation on breach | Requests | `SlaEscalationRulesTests` | `Nibras.Requests.Domain.Rules.SlaEscalationRule` | Requests → SLAs | no | 3 |
| `BR-RQS-006` | Effect execution and compensation | Requests | `RequestEffectSagaRulesTests` | `Nibras.Requests.Domain.Rules.RequestEffectSagaRule` | Requests → fees | no | 3 |
| `BR-WEL-001` | Visibility levels | Wellbeing | `WellbeingVisibilityRulesTests` | `Nibras.Wellbeing.Domain.Rules.WellbeingVisibilityRule` | none | yes | 1 |
| `BR-WEL-002` | Break-glass access | Wellbeing | `BreakGlassRulesTests` | `Nibras.Wellbeing.Domain.Rules.BreakGlassRule` | Security → session timeout | no | 5 |
| `BR-WEL-003` | Events carry no clinical detail | Wellbeing | `WellbeingEventPayloadRulesTests` | `Nibras.Wellbeing.Domain.Rules.WellbeingEventPayloadRule` | none | no | 5 |
| `BR-WEL-004` | Medication authorization and sending home | Wellbeing | `MedicationAuthorizationRulesTests` | `Nibras.Wellbeing.Domain.Rules.MedicationAuthorizationRule` | none | no | 5 |
| `BR-PLT-001` | Soft warn then hard block on plan limits | Platform | `PlanLimitRulesTests` | `Nibras.Platform.Domain.Rules.PlanLimitRule` | none | no | 1 |
| `BR-PLT-002` | What read-only mode allows | Platform | `ReadOnlyModeRulesTests` | `Nibras.Platform.Domain.Rules.ReadOnlyModeRule` | none | no | 1 |
| `BR-PLT-003` | Deletion cooling-off | Platform | `TenantDeletionCoolingOffRulesTests` | `Nibras.Platform.Domain.Rules.TenantDeletionCoolingOffRule` | Security → retention periods | no | 1 |
| `BR-PLT-004` | Data residency pinning at provisioning | Platform | `DataResidencyRulesTests` | `Nibras.Platform.Domain.Rules.DataResidencyRule` | none | no | 1 |
| `BR-PLT-005` | Usage metering counts | Platform | `UsageMeteringRulesTests` | `Nibras.Platform.Domain.Rules.UsageMeteringRule` | none | yes | 1 |
| `BR-PLT-006` | Tenant export completeness | Platform | `TenantExportRulesTests` | `Nibras.Platform.Domain.Rules.TenantExportRule` | Security → export approval rules | no | 1 |
| `BR-L10N-001` | Arabic search normalization | Platform | `ArabicNormalizationRulesTests` | `Nibras.Platform.Domain.Rules.ArabicNormalizationRule` | General → languages | no | 1 |
| `BR-L10N-002` | Numeral rendering | Platform | `NumeralRenderingRulesTests` | `Nibras.Platform.Domain.Rules.NumeralRenderingRule` | General → numerals | no | 1 |
| `BR-L10N-003` | Hijri display, Gregorian source of truth | Scheduling | `HijriDisplayRulesTests` | `Nibras.Scheduling.Domain.Rules.HijriDisplayRule` | General → calendars, time zone | no | 1 |
| `BR-L10N-004` | Amounts in words in both languages | Finance | `AmountInWordsRulesTests` | `Nibras.Finance.Domain.Rules.AmountInWordsRule` | Finance → receipt layout | yes | 3 |
| `BR-L10N-005` | Arabic plural forms | Notification | `ArabicPluralRulesTests` | `Nibras.Notification.Domain.Rules.ArabicPluralRule` | Notifications → templates | no | 1 |
| `BR-L10N-006` | Pinned culture on every host | Platform | `CultureInvarianceRulesTests` | `Nibras.Platform.Domain.Rules.CultureInvarianceRule` | none | no | 1 |
| `BR-L10N-007` | Bilingual names and fallback | School | `BilingualNameRulesTests` | `Nibras.School.Domain.Rules.BilingualNameRule` | General → languages | no | 1 |

**Property-based column.** "yes" marks a rule whose statement involves arithmetic: sums, averages, weights, rounding, proration, allocation, caps, percentages, ranks or balances. Those rules get a property-based test in addition to the table-driven one, asserting invariants that no finite example list can cover (for example: allocation never exceeds the payment, rounding is idempotent, a weighted average lies between its minimum and maximum input). The classification is derived from the rule text and is confirmed or corrected by the business-rules-reviewer agent during Group F review.

### 3. Workflows

**How to read the table.** The state type is an enumeration in the owning service's Domain project, never a set of booleans. The feature folder holds one sub-folder per transition command, following the anatomy in reference architecture Section 2. Transition tests are the rows of the workflow's test table in Appendix R, one test each.

| Workflow | Name | Owner | Tier | Mobile | Offline | State type | Feature folder | Phase | Transition tests (Appendix R) |
|---|---|---|---|---|---|---|---|---|---|
| `WF-IDN-01` | Invitation or join-code joining | Identity | 1 | yes | no | `InvitationOrJoinCodeJoiningStatus` | `Application/Features/InvitationOrJoinCodeJoining/` | 1 | TC-IDN-001 to TC-IDN-006 |
| `WF-IDN-02` | Parent self-registration and child linking | Identity | 1 | yes | no | `ParentSelfRegistrationAndChildLinkingStatus` | `Application/Features/ParentSelfRegistrationAndChildLinking/` | 1 | TC-IDN-011 to TC-IDN-016 |
| `WF-IDN-03` | Duplicate account merge | Identity | 1 | no | no | `DuplicateAccountMergeStatus` | `Application/Features/DuplicateAccountMerge/` | 1 | TC-IDN-021 to TC-IDN-026 |
| `WF-IDN-04` | Delegation during absence | Identity | 1 | yes | no | `DelegationDuringAbsenceStatus` | `Application/Features/DelegationDuringAbsence/` | 1 | TC-IDN-031 to TC-IDN-036 |
| `WF-IDN-05` | Role change with four-eyes approval | Identity | 1 | yes | no | `RoleChangeWithFourEyesApprovalStatus` | `Application/Features/RoleChangeWithFourEyesApproval/` | 1 | TC-IDN-041 to TC-IDN-046 |
| `WF-IDN-06` | Offboarding and access revocation | Identity | 1 | no | no | `OffboardingAndAccessRevocationStatus` | `Application/Features/OffboardingAndAccessRevocation/` | 1 | TC-IDN-051 to TC-IDN-056 |
| `WF-SEC-01` | Access review campaign | Identity | 1 | no | no | `AccessReviewCampaignStatus` | `Application/Features/AccessReviewCampaign/` | 1 | TC-SEC-001 to TC-SEC-006 |
| `WF-SEC-02` | Break-glass access | Identity | 1 | no | no | `BreakGlassAccessStatus` | `Application/Features/BreakGlassAccess/` | 1 | TC-SEC-011 to TC-SEC-016 |
| `WF-SEC-03` | Consented impersonation | Identity | 1 | no | no | `ConsentedImpersonationStatus` | `Application/Features/ConsentedImpersonation/` | 1 | TC-SEC-021 to TC-SEC-026 |
| `WF-PLT-01` | Tenant signup to live | Platform | 1 | no | no | `TenantSignupToLiveStatus` | `Application/Features/TenantSignupToLive/` | 1 | TC-PLT-001 to TC-PLT-006 |
| `WF-PLT-02` | Trial conversion and plan change | Platform | 1 | no | no | `TrialConversionAndPlanChangeStatus` | `Application/Features/TrialConversionAndPlanChange/` | 1 | TC-PLT-011 to TC-PLT-016 |
| `WF-PLT-03` | Suspension, export, and deletion | Platform | 1 | no | no | `SuspensionExportAndDeletionStatus` | `Application/Features/SuspensionExportAndDeletion/` | 1 | TC-PLT-021 to TC-PLT-026 |
| `WF-SCH-01` | Transfer or withdrawal with clearance | School | 1 | no | no | `TransferOrWithdrawalWithClearanceStatus` | `Application/Features/TransferOrWithdrawalWithClearance/` | 2 | TC-SCH-001 to TC-SCH-006 |
| `WF-SCH-02` | End of year close and rollover | School | 1 | no | no | `EndOfYearCloseAndRolloverStatus` | `Application/Features/EndOfYearCloseAndRollover/` | 2 | TC-SCH-011 to TC-SCH-016 |
| `WF-SCH-03` | Year archival and reopen | School | 1 | no | no | `YearArchivalAndReopenStatus` | `Application/Features/YearArchivalAndReopen/` | 2 | TC-SCH-021 to TC-SCH-026 |
| `WF-SCH-04` | Mid-year campus transfer | School | 1 | no | no | `MidYearCampusTransferStatus` | `Application/Features/MidYearCampusTransfer/` | 2 | TC-SCH-031 to TC-SCH-036 |
| `WF-ADM-01` | Inquiry to enrollment | Admissions | 1 | yes | no | `InquiryToEnrollmentStatus` | `Application/Features/InquiryToEnrollment/` | 4 | TC-ADM-001 to TC-ADM-006 |
| `WF-ADM-02` | Re-enrollment with fee settlement check | Admissions | 1 | yes | no | `ReEnrollmentWithFeeSettlementCheckStatus` | `Application/Features/ReEnrollmentWithFeeSettlementCheck/` | 4 | TC-ADM-011 to TC-ADM-016 |
| `WF-ACA-01` | Assignment lifecycle | Academics | 1 | yes | yes | `AssignmentLifecycleStatus` | `Application/Features/AssignmentLifecycle/` | 2 | TC-ACA-001 to TC-ACA-006 |
| `WF-ASM-01` | Exam to report card | Assessment | 1 | yes | yes | `ExamToReportCardStatus` | `Application/Features/ExamToReportCard/` | 2 | TC-ASM-001 to TC-ASM-006 |
| `WF-ASM-02` | Grade appeal and post-lock change | Assessment | 1 | yes | no | `GradeAppealAndPostLockChangeStatus` | `Application/Features/GradeAppealAndPostLockChange/` | 2 | TC-ASM-011 to TC-ASM-016 |
| `WF-ASM-03` | Exam paper setting, review, and printing | Assessment | 1 | no | no | `ExamPaperSettingReviewAndPrintingStatus` | `Application/Features/ExamPaperSettingReviewAndPrinting/` | 2 | TC-ASM-021 to TC-ASM-026 |
| `WF-ATT-01` | Daily attendance to intervention | Attendance | 1 | yes | yes | `DailyAttendanceToInterventionStatus` | `Application/Features/DailyAttendanceToIntervention/` | 2 | TC-ATT-001 to TC-ATT-006 |
| `WF-ATT-02` | Early dismissal and gate pickup | Attendance | 1 | yes | no | `EarlyDismissalAndGatePickupStatus` | `Application/Features/EarlyDismissalAndGatePickup/` | 2 | TC-ATT-011 to TC-ATT-016 |
| `WF-FIN-01` | Fee plan to collection and escalation | Finance | 1 | yes | no | `FeePlanToCollectionAndEscalationStatus` | `Application/Features/FeePlanToCollectionAndEscalation/` | 3 | TC-FIN-001 to TC-FIN-006 |
| `WF-FIN-02` | Invoice reversal, credit note, and refund | Finance | 1 | no | no | `InvoiceReversalCreditNoteAndRefundStatus` | `Application/Features/InvoiceReversalCreditNoteAndRefund/` | 3 | TC-FIN-011 to TC-FIN-016 |
| `WF-FIN-03` | Cheque receipt and bounce | Finance | 1 | no | no | `ChequeReceiptAndBounceStatus` | `Application/Features/ChequeReceiptAndBounce/` | 3 | TC-FIN-021 to TC-FIN-026 |
| `WF-FIN-04` | Scholarship award | Finance | 1 | yes | no | `ScholarshipAwardStatus` | `Application/Features/ScholarshipAward/` | 3 | TC-FIN-031 to TC-FIN-036 |
| `WF-FIN-05` | Payer change to sponsor | Finance | 1 | yes | no | `PayerChangeToSponsorStatus` | `Application/Features/PayerChangeToSponsor/` | 3 | TC-FIN-041 to TC-FIN-046 |
| `WF-FIN-06` | Cashier day close | Finance | 1 | no | no | `CashierDayCloseStatus` | `Application/Features/CashierDayClose/` | 3 | TC-FIN-051 to TC-FIN-056 |
| `WF-RQS-01` | Service request lifecycle | Requests | 1 | yes | no | `ServiceRequestLifecycleStatus` | `Application/Features/ServiceRequestLifecycle/` | 2 | TC-RQS-001 to TC-RQS-006 |
| `WF-BEH-01` | Incident to intervention | Behavior | 1 | yes | no | `IncidentToInterventionStatus` | `Application/Features/IncidentToIntervention/` | 4 | TC-BEH-001 to TC-BEH-006 |
| `WF-WEL-01` | Accommodation plan to exam sitting | Wellbeing | 2 | no | no | `AccommodationPlanToExamSittingStatus` | `Application/Features/AccommodationPlanToExamSitting/` | 2 | TC-WEL-001 to TC-WEL-006 |
| `WF-WEL-02` | Clinic visit to sent home | Wellbeing | 2 | yes | no | `ClinicVisitToSentHomeStatus` | `Application/Features/ClinicVisitToSentHome/` | 2 | TC-WEL-011 to TC-WEL-016 |
| `WF-WEL-03` | Medication authorization and administration | Wellbeing | 2 | yes | no | `MedicationAuthorizationAndAdministrationStatus` | `Application/Features/MedicationAuthorizationAndAdministration/` | 5 | TC-WEL-021 to TC-WEL-026 |
| `WF-WEL-04` | Safeguarding concern escalation | Wellbeing | 2 | yes | no | `SafeguardingConcernEscalationStatus` | `Application/Features/SafeguardingConcernEscalation/` | 3 | TC-WEL-031 to TC-WEL-036 |
| `WF-WEL-05` | Daily wellbeing check-in escalation | Wellbeing | 2 | yes | yes | `DailyWellbeingCheckInEscalationStatus` | `Application/Features/DailyWellbeingCheckInEscalation/` | 5 | TC-WEL-041 to TC-WEL-046 |
| `WF-HR-01` | Staff leave to substitution | Hr | 2 | yes | no | `StaffLeaveToSubstitutionStatus` | `Application/Features/StaffLeaveToSubstitution/` | 2 | TC-HR-001 to TC-HR-006 |
| `WF-HR-02` | Staff hiring to onboarding | Hr | 2 | no | no | `StaffHiringToOnboardingStatus` | `Application/Features/StaffHiringToOnboarding/` | 5 | TC-HR-011 to TC-HR-016 |
| `WF-HR-03` | Teaching licence expiry compliance | Hr | 2 | no | no | `TeachingLicenceExpiryComplianceStatus` | `Application/Features/TeachingLicenceExpiryCompliance/` | 5 | TC-HR-021 to TC-HR-026 |
| `WF-HR-04` | Payroll input cycle | Hr | 2 | no | no | `PayrollInputCycleStatus` | `Application/Features/PayrollInputCycle/` | 5 | TC-HR-031 to TC-HR-036 |
| `WF-OPS-01` | Purchase requisition to asset | Operations | 2 | yes | no | `PurchaseRequisitionToAssetStatus` | `Application/Features/PurchaseRequisitionToAsset/` | 3 | TC-OPS-001 to TC-OPS-006 |
| `WF-OPS-02` | Library lending and fines | Operations | 2 | yes | yes | `LibraryLendingAndFinesStatus` | `Application/Features/LibraryLendingAndFines/` | 5 | TC-OPS-011 to TC-OPS-016 |
| `WF-OPS-03` | Transport subscription change | Operations | 2 | yes | no | `TransportSubscriptionChangeStatus` | `Application/Features/TransportSubscriptionChange/` | 5 | TC-OPS-021 to TC-OPS-026 |
| `WF-OPS-04` | Facility booking approval | Operations | 2 | yes | no | `FacilityBookingApprovalStatus` | `Application/Features/FacilityBookingApproval/` | 5 | TC-OPS-031 to TC-OPS-036 |
| `WF-OPS-05` | Safety incident and drill logging | Operations | 2 | yes | yes | `SafetyIncidentAndDrillLoggingStatus` | `Application/Features/SafetyIncidentAndDrillLogging/` | 5 | TC-OPS-041 to TC-OPS-046 |
| `WF-PRV-01` | Data subject access request | Platform | 1 | no | no | `DataSubjectAccessRequestStatus` | `Application/Features/DataSubjectAccessRequest/` | 1 | TC-PRV-001 to TC-PRV-006 |
| `WF-PRV-02` | Sensitive export approval | Documents | 1 | no | no | `SensitiveExportApprovalStatus` | `Application/Features/SensitiveExportApproval/` | 3 | TC-PRV-011 to TC-PRV-016 |
| `WF-DATA-01` | Legacy import with dry run and rollback | Documents | 1 | no | no | `LegacyImportWithDryRunAndRollbackStatus` | `Application/Features/LegacyImportWithDryRunAndRollback/` | 1 | TC-DATA-001 to TC-DATA-006 |
| `WF-INF-01` | On-premises upgrade with rollback | Platform | 1 | no | no | `OnPremisesUpgradeWithRollbackStatus` | `Application/Features/OnPremisesUpgradeWithRollback/` | 6 | TC-INF-001 to TC-INF-006 |
| `WF-INF-02` | Release rollout with canary and rollback | Platform | 1 | no | no | `ReleaseRolloutWithCanaryAndRollbackStatus` | `Application/Features/ReleaseRolloutWithCanaryAndRollback/` | 1 | TC-INF-011 to TC-INF-016 |
| `WF-INF-03` | Restore and failover drill | Platform | 1 | no | no | `RestoreAndFailoverDrillStatus` | `Application/Features/RestoreAndFailoverDrill/` | 6 | TC-INF-021 to TC-INF-026 |

### 4. The implementation contract

Quoted from `.claude/rules/rules-and-workflows.md`, which Claude Code applies automatically to any file under a service's Domain or Application project:

| Rule | Why |
|---|---|
| A class implementing a business rule names its `BR-` identifier in a comment, and has the test class the rule names | Rule, code and test must be findable from each other or they drift |
| Rules live in the domain, never in an endpoint, a consumer, a component or a database trigger | One rule, one place |
| Money is `decimal` with an explicit currency; rounding happens once, at the point the rule names | The worked examples in Appendix S assume exactly this |
| The order of operations in code matches the order stated in the rule | Where the rule is ambiguous, the rule is fixed, not the code guessed |
| Every workflow is a state machine with an explicit state type; every transition validates state, checks the permission, writes the audit entry, and publishes through the outbox | Master brief Section 14 |
| Every state a human waits in has a timeout and an escalation | Nothing waits forever |
| Concurrent transitions resolve deterministically: first decision wins, the second gets a stable error code naming who decided | Two approvers at once is normal, not an edge case |
| Every terminal state is reachable from every state, directly or through cancellation | Support can always end a stuck item |
| A rule change updates the rule, its examples, its test class, and every screen that quotes it, in one change | Otherwise the product and its documentation disagree |

### 5. How the tests are built

| Test kind | Built from | Named |
|---|---|---|
| Table-driven rule test | Every `Given` line of the rule in Appendix S becomes one row of an `[Theory]` data source | `<TestClass>.<Rule>_<Given>_<Then>` |
| Property-based rule test | The invariants of the rules marked "yes" above | `<TestClass>.Property_<Invariant>` |
| Transition test | Every row of the workflow's test table in Appendix R | `<State>_<Transition>_<Expected>`, carrying the row's `TC-` identifier |
| Timeout test | Every state with a timeout in Appendix R | `<State>_TimesOut_<Escalation>` |
| Concurrency test | Every transition two actors can take at once | `<Transition>_Concurrent_FirstWins` |

A worked example that does not become a test row is a defect in the test class, and `/simulate-year` reports it.

### 6. Mutation-testing targets

Stryker.NET runs on the classes below and must reach a **mutation score of 80% or better**, per master brief Section 24. These are the domains where a surviving mutant is a wrong grade, a wrong invoice, a wrong promotion or a wrong permission.

| Service | Rules under mutation testing |
|---|---|
| Assessment | `BR-ASM-001`, `BR-ASM-002`, `BR-ASM-003`, `BR-ASM-004`, `BR-ASM-005`, `BR-ASM-006`, `BR-ASM-007`, `BR-ASM-008`, `BR-ASM-009`, `BR-ASM-010`, `BR-ASM-011`, `BR-ASM-012`, `BR-ASM-013`, `BR-ASM-014` |
| Finance | `BR-FIN-001`, `BR-FIN-002`, `BR-FIN-003`, `BR-FIN-004`, `BR-FIN-005`, `BR-FIN-006`, `BR-FIN-007`, `BR-FIN-008`, `BR-FIN-009`, `BR-FIN-010`, `BR-FIN-011`, `BR-FIN-012`, `BR-FIN-013`, `BR-FIN-014`, `BR-FIN-015`, `BR-FIN-016`, `BR-FIN-017`, `BR-FIN-018`, `BR-FIN-019`, `BR-L10N-004` |
| Identity | `BR-IDN-001`, `BR-IDN-002`, `BR-IDN-003`, `BR-IDN-004`, `BR-IDN-005`, `BR-IDN-006`, `BR-IDN-007`, `BR-IDN-008`, `BR-IDN-009` |
| Attendance | `BR-ATT-001`, `BR-ATT-002`, `BR-ATT-003`, `BR-ATT-004`, `BR-ATT-005`, `BR-ATT-006`, `BR-ATT-007`, `BR-ATT-008`, `BR-ATT-009`, `BR-ATT-010`, `BR-ATT-011` |
| School | `BR-L10N-007` |

### 7. Coverage check

| Check | Result |
|---|---|
| Rules with no owning service | none |
| Rules with no named test class | none |
| Test class names used by more than one rule | none |
| Workflows with no owning service | none |
| Workflows with no transition test in Appendix R | none |
| Owners with no build phase in document 05 | none |
| Rules in Appendix S | 95 |
| Workflows in Appendix R | 52 |

## Decisions in force

| Decision | Record |
|---|---|
| Rules live in the Domain project of their owning service, one class per rule | This document, following reference architecture Section 2 |
| Every workflow state is an enumeration, never flags | `.claude/rules/rules-and-workflows.md` |
| Test case identifiers follow `TC-<AREA>-<NNN>` | ADR-0014 |

## Dependencies on other documents

| Document | What this one takes from it |
|---|---|
| Appendix S | Every rule, its owner, parameters and test class |
| Appendix R | Every workflow, its owner, tier, mobile and offline availability |
| `05-service-catalog.md` | The build phase per service |
| `13-workflows-and-sagas.md` | The saga designs this document does not repeat |
| `16-test-strategy.md` | The test infrastructure these tests run on |

## Open points

| Point | Default | Owner | L | I | Score | In the register |
|---|---|---|---|---|---|---|
| The property-based classification is derived from rule text | Confirmed or corrected by the business-rules-reviewer agent in Group F review | Architect | 2 | 2 | 4 | none |
| Promotion eligibility and status changes in School are workflows, not Appendix S rules | Tested per transition; add a rule to Appendix S under a version bump if an arithmetic threshold appears | Architect | 2 | 2 | 4 | none |

## Review record

| Date | Reviewer | Result |
|---|---|---|
| 2026-09-21 | Generated from Appendices R and S | Coverage check above |

## How this document is verified

| Claim | Proof |
|---|---|
| Every rule and workflow has a row | The document is generated from the appendices; the coverage check in Section 7 is recomputed on every regeneration |
| The document is current | Kit-lint rule R23 reruns `gen-31.mjs --check` and fails when Appendices R or S or document 05 changed since it was generated |
| Every rule has a test class that exists in code | Once code exists, an architecture test enumerates `BR-` comments and asserts the named test class exists |
| Every worked example is a test row | `/simulate-year` and the business-rules-reviewer agent compare Appendix S examples with the test data sources |
| Mutation targets are met | Stryker.NET in the pipeline, gated at 80% on the classes in Section 6 |
