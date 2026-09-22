# Appendix defects found while writing the plan (for ADR-0019 and v9.1)

**Closed.** Applied to the brief under ADR-0019 (v9.1); what changed and what was deliberately not applied is in `docs/project/KIT_V9_1_CHANGES.md`. New brief defects start a new log.

## From document 02
- Appendix P: 21 corrections (PowerSchool regional Arabic edition; API is table stakes; openSIS and Gibbon also open source; Fedena not on-premises; Gibbon has Finance, no API; Classera finance and HR depth). Tier 1 API/OneRoster/iCal recommendation pending product owner.

## From document 09
- Appendix U (U.2, U.10) and Appendix I nurse row imply queued approvals and offline clinic visits; Appendix M forbids both. Plan follows M.

## From document 12
- Appendix R uses school.students.link-guardian and identity.access-review.decide; not in Appendix B (B has school.guardians.link, identity.access-reviews.certify).

## From document 22 and REQ-PLT-032
- Master brief Section 19 says "a limit breach returns Problem Details with Retry-After"; Appendix K defines plan quotas as 402 PLATFORM_PLAN_LIMIT_REACHED. Clarify Section 19: rate limits 429 with Retry-After, plan quotas 402.

## From documents 23 and 24
- Appendix G rounding default "2 decimals" is wrong for JOD (3 decimals). Decimals follow ISO 4217 minor units.
- Cross-script name matching misses long-vowel pairs (يوسف / Yousef).

## From document 11
- Some bindings required by documents 10 and 13 are missing from Appendix E consumer columns.

## From sheets: School, Scheduling, Academics, Admissions
- Appendix E: school.section.created.v1 and school.staff.created.v1 carry no names; no change events for staff, rooms, grade levels, grading periods, holidays, siblings, missing submissions; finance.payment.received.v1 lacks offer/application reference; finance.invoice.overdue.v1 lacks studentId; Appendix R cites six event keys absent from E.
- Appendix B: no permissions for departments, houses, timetable constraints, student groups, rubrics, admission campaigns, age override.
- Appendix K: no codes for configuration-in-use, solver-already-running, grading-period-locked.
- Appendix C: no rows for uncovered-period alert, OTP and welcome messages.
- Reference architecture 8.0: omits gRPC calls to Scheduling that document 10 depends on.

## From sheets: Identity, Platform, Audit, Gateway
- Appendix E: Appendix R names identity.impersonation.started.v1, identity.break-glass.granted.v1, platform.upgrade.started.v1, audit.action.recorded.v1, which E lacks.
- Appendix B: Appendix R uses identity.join-request.approve, school.students.link-guardian, platform.plan.change; B lacks permissions for join codes, template library, modules, tenant export, guardian-safe transparency view.
- Appendix K: no codes for expired invitation, oversized body (413), malformed token.
- Appendix C: no rows for invitations, one-time codes, audit-export notice, platform lifecycle mails.
- Test ids: TC-PLT-002 to 005 mean different things in Appendices R and W.
- Conflicts: tenant-deletion cooling-off 7 vs 30 days; invitation expiry 7 vs 14 days.
- Reference architecture 8.0 lists no synchronous calls for these services, but sheets need School, Platform and Identity query calls.

## From sheets: Attendance, Assessment, Bff.Web, Bff.Mobile
- Appendix E missing: assessment.report-card.generated.v1, assessment.exam-paper.approved.v1, assessment.exam-paper.released.v1, a grading-period change event, an attendance.* event consumed by Requests.
- Appendix R uses audit.action.recorded.v1; correct form is <service>.audit.recorded.v1. No wellbeing.* event routed to Attendance, so WF-ATT-01 and WF-WEL-02 intervention states cannot be driven there.
- Appendix B missing assessment.exam-paper.print (used in R); no permission to read medical excuse detail; none for the principal's nudge.
- Appendix K: no code for appeal outside window, incomplete mark grid, app below minimum version.
- PRODUCT DECISION: Appendix R WF-ATT-01 sends the absence alert 30 minutes after the register closes; REQ-ATT-017 and master brief Section 31 require urgent dispatch within 30 seconds. A grace window avoids alarming parents over a teacher's correction; immediacy is what parents expect. Needs the product owner.

## From sheets: Finance, Requests, Notification, Communication
- Appendix R names events E lacks: finance.scholarship.awarded, finance.payer.changed, finance.cash-session.closed, finance.deposit.recorded, requests.request.reassigned, audit.action.recorded.
- Appendix B/I: no finance.payers resource, no duty-roster or anonymous-concern permission, no role granted notification.*.
- Appendix E: no contact-address events for Notification; no request withdrawn/expired events.
- Conflicts: REQ-PLT-009 vs BR-FIN-017; Appendix G rounding ignores JOD 3 decimals; BR-NOT-004 dedupe window 10 min vs Appendix C 5 min.

## From documents 25, 26, 27
- Appendix W has no autonomy column, although master brief Section 25 says both numbers appear there. (Introduced during the v9 kit build.) Document 25 assigns autonomy levels; copy them into Appendix W.
- Retention job names differ between document 10 section 8 and document 12 section 10.3; document 27 treats document 10 as authoritative. Plan-level fix: align 12 to 10.

## From document 19
- k6 (AGPL-3.0) is named in Section 6.2 but missing from Section 6.4 and allow.json. Forgejo (GPL-3.0 since v9.0) and Matomo (GPL-3.0) need rows only if chosen.
- Reference architecture Section 16 pins Valkey 8; current is 9.1.2. Redis must never be pinned below 8.0 (7.4 has no AGPL option).
- NetArchTest: no release since 2021, no licence field in package metadata; watch item.
- Shouldly moved BSD-2-Clause to BSD-3-Clause (still allowed). MinIO repository archived.
- Scanner bugs (fixed in tools): SPDX OR expressions, OFL-1.1 fonts, licence-as-file packages.

## From document 32
- 23 of 44 signature features have no Appendix O minute. Four fit existing minutes (11, 23, 37, 44); eighteen go to a reserve bank of one-minute steps. Feature 39 (calendar-aware scaling) cannot be shown on demo data; proposed demotion from signature list.
- Appendix O minutes 1, 5, 10, 12, 14 need capabilities from later phases than document 17's phase demos promise; align document 17 demo claims or the script.
- TC-DOC-002 means different things in Appendix O and Appendix W.

## From sheet: Ai
- Appendix E: Ai consumes only school.student.status-changed.v1; missing from source change events and school.student.section-changed.v1, so re-index on change (Section 25) is impossible. Sheet uses a 15-minute pull via Bff.Web meanwhile.
- Appendix K AI_USAGE_LIMIT_REACHED is 402; document 25 says fall back to rung 1 instead. Sheet follows 25.
- Appendix G lacks settings for template version, model tag, rung 4 field groups.
- Appendix C / document 11: no notification row for blocked prompt-injection admin alert; nibras.ai cannot send RequestNotification.
- Appendix I: no role template grants ai.* permissions. Appendix F: no Ai entities.
- Documents 10 and 21 name the index table embeddings; document 25 names it embedding_chunk. Plan-level fix: align 10 and 21 to 25.
- Document 25 wants gRPC to Ai; RA 8.0 gives Ai no synchronous dependency and Bff.Web has no internal Ai routes. Sheet uses REST, proposes three Bff.Web routes.
- Document 25 has Reporting, Assessment, Hr publish ai.* events, breaking document 11's own-exchange rule. Plan-level fix: they publish <service>.usage.recorded.v1 with an ai meter instead.
- Document 17 has no Integrations capability in phase 4 (LTI 1.3) or phase 5 (QTI 3, Open Badges 3.0, CASE), although the Platform sheet and REQ-INT-016 time them there. Document 34 places SL-INT-411 under CAP-ACA-03; phase 5 places the standards slices under the nearest capability. Fix: add CAP-INT-02 (phase 4) and CAP-INT-03 (phase 5) to document 17.
- OneRoster: document 23 and the Platform sheet say phase 4; document 34 builds it in phase 3 under CAP-INT-01. Align with the product owner's Tier 1 decision on the public API.
- Admissions open point 4: no command or event asks Finance to raise an application fee, and finance.payment.received.v1 carries no sourceRefs. Document 34 SL-ADM-406 uses Finance's invoice endpoints. Fix: add the command and the field to Appendix E and document 11.
- Master brief Section 28 phase ranges (phase 1 8 to 10 weeks, and the rest) predate document 34. Document 17 Section 1 now derives them from slice-days (tools/plan-build/schedule-34.mjs): 14 to 22, 14 to 21, 11 to 17, 7 to 11, 9 to 14, 6 to 8 weeks; launch 61 to 93 weeks; MVP 42 capabilities, 33 to 50 weeks. Fix: replace Section 28's ranges and MVP figure with these under ADR-0019.
