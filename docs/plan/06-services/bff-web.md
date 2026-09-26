# Bff.Web

Bff.Web is the web backend-for-frontend: the one place where a screen of the Angular workspaces that needs data from more than one service gets it in one call. It composes role home payloads ("Today" for every role in Appendix D), the Student 360 page from the Reporting read model and the owning services, the session bootstrap (identity, tenant snapshot, effective permissions, landing workspace, reference data), the permission refresh after `identity.permissions.changed.v1`, the command palette and search, the guardian transparency panel, and the platform console's cross-service process monitor. It owns no data, publishes no event, holds no business rule and performs no write that bypasses the owning service (REQ-BFF-001). A single-service screen, such as the attendance register or the mark grid, does not come here: the web workspace calls that service through the Gateway with its generated client (`08-web-structure.md` part 4).

**Group** C · **Requirement areas covered** BFF, with WEB, PERF, SEC and PRV rows that bind this host · **Last updated** 2026-09-26 by the planning session

| Fact | Value | Source |
|---|---|---|
| Application (Appendix L) | **Bff.Web**, "Web backend-for-frontend" | Appendix L.1 |
| Tier | 1 | Appendix L.1 |
| AREA code | `BFF` (shared with Bff.Mobile) | Appendix L.1 |
| Database | none; short-lived Redis cache on `redis-cache` under ACL user `svc_bff_web` | Appendix L.1, `21-performance-engineering.md` §2.3 |
| Exchange | none | Appendix L.1, `11-messaging-architecture.md` part 1 |
| Images | `nibras/bff-web` (api) | Appendix L.1 |
| Worker | none | Appendix L.1 |
| Why the boundary exists | "Release: screen shapes change at the web release cadence and carry no business rules, so they must not force a service release." | `05-service-catalog.md` |
| Synchronous dependencies | "none as gRPC; reads services over HTTP and the Reporting read models" | Reference architecture Section 8, table 8.0 |
| Service level class | Gateway class (master brief Section 31) | Reference architecture Section 8, table 8.0 |
| Scaling profile | "Stateless, scaled on requests per second; role-home payloads cached 15 s L1 and 60 s L2" | `05-service-catalog.md` |
| Build phase | 1 (master brief Section 28) | `05-service-catalog.md` |
| Sensitivity | "passes through, stores nothing" | `05-service-catalog.md`, Appendix J |
| Route prefix | `/bff/web/v1/`, screen-shaped, never in the public developer portal | `22-api-conventions-and-error-catalog.md` §1.1 |

---

## 1. Responsibilities and non-responsibilities

### Responsibilities

| Owns | Detail |
|---|---|
| Session bootstrap | One call at sign-in and tenant switch: identity, tenant snapshot (branding, terminology, numerals, calendars, feature flags, plan limits the screens need), effective permissions with `permissionVersion`, landing workspace in Appendix I order, reference slices, product name (`14-design-system-and-ux.md`) |
| Permission refresh | The effective set with `ETag` for `PermissionStore.refresh()` after the hub's `permissions.changed` message (`08-web-structure.md` §3.2); the "view as role" preview set |
| Role home payloads | One composed payload per role "Today" (Appendix D), each card answering one question and linking to one action, each card's data filtered by the caller's permissions and scope |
| Student 360 composition | Header, tabs, and one chronological timeline across attendance, grades, behavior, health visits, fees, communication, documents and interventions, filtered by the viewer's permissions, with "exists, no access" for what the viewer may not read (REQ-BFF-003) |
| Command palette and search | Federated search across the services the caller may read, merged with the permitted navigation and action entries; natural-language search through Ai when the tenant enables it (REQ-BFF-006) |
| Ai's front door and read path | The web client's only road to Ai (the `ai/{path}` pass-through and the natural-language routes, each answering with Ai's 202 job), and the three internal routes Ai's jobs read through: the source feed, the source re-check and the read-only tools (reference architecture Section 8.0; section 4.1) |
| Guardian transparency panel | Who read the child's sensitive records and when, consents, retention clocks, composed from Audit and Platform (`12-security-privacy-safety.md` §7) |
| Campus digital twin (feature 33) | Operations' floor plans and tickets, Scheduling's room holds and Attendance's present counts, joined by room for the principal's floor plan and weekly heatmap. It shows rooms and counts, never a child (REQ-OPS-016, SL-OPS-624; section 4.1) |
| Shell counters | Unread messages, unread inbox, open tasks and pending approvals for the shell badges |
| Platform console process monitor | Every service's `/jobs` list for a tenant, read through one call (`22-api-conventions-and-error-catalog.md` §6.3) |
| Per-tenant web artefacts | `manifest.webmanifest` with the tenant's white-label name, icon and theme colour, and the App Links and Universal Links association files per tenant host (`09-mobile-structure.md` §4.2) |
| Response shaping | Screen-shaped DTOs under 32 KB after Brotli, `ETag` and `304`, the seven-state model's `partial` flag per region, `asOf` time per region, Appendix K codes passed through unchanged |
| Caching of composed payloads | The two entries of `21-performance-engineering.md` §1.21 plus the additions in section 10, all under `Nibras.BuildingBlocks.Caching` |

### Not responsible for

| Does not own | Owner instead | Why the line is here |
|---|---|---|
| Any data, any database, any event | Every data-owning service | Appendix L; a BFF that stored data would become a twenty-first service without a boundary |
| Any business rule, including permission decisions | The owning service; Identity for permissions | The BFF forwards the caller's token and never widens or narrows what the service returns except to drop a region the caller cannot see (T-GW-04) |
| Writes | The owning service, called by the web client through the Gateway | REQ-BFF-001; the one forwarded write, preferences, is a pass-through to Identity (Open point 3) |
| Single-service screens (register, mark grid, invoices, approvals inbox, timetable editor) | The owning service's API through the Gateway | `08-web-structure.md` part 4: one generated client per service |
| Dashboards, projections, "explain this number", Because panels | Reporting | The BFF composes Reporting's answers; it computes no aggregate |
| Realtime delivery (permission refresh, job progress, feature-store patches) | Communication (SignalR hubs) | The BFF answers the follow-up read only |
| Token validation, tenant resolution, edge rate limits, CORS, security headers | Gateway | The BFF trusts the Gateway's forwarded tenant and re-validates the token locally as every service does |
| The mobile application's composition and sync | Bff.Mobile | Different release cadence and a different contract |
| Settings, terminology and branding values | Platform | Read for the bootstrap only |

---

## 2. Requirements covered

| Range or identifier | What it binds here |
|---|---|
| REQ-BFF-001, REQ-BFF-002, REQ-BFF-003, REQ-BFF-006 | The four Bff.Web rows of `03-requirements-catalog.md` |
| REQ-RPT-004 | The homeroom home card: absent today, excuses to review, flags for the class, birthdays (composed here from Reporting and Attendance) |
| REQ-GW-007 | A stopped upstream degrades its card to an honest unavailable state; the rest of the screen renders |
| REQ-GW-008 | The correlation id is forwarded on every upstream call |
| REQ-WEB-005 | Error and offline handling depend on the Problem Details and `partial` flags this host returns |
| REQ-SEC-003, REQ-SEC-004, REQ-SEC-005 | The caller's token is forwarded; no permission of its own; generated isolation and permission suites cover every BFF route |
| REQ-PERF-003, REQ-PERF-010 | Composed reads within the Gateway-class budget; composed home p95 under 250 ms (Appendix N, N-05, `15-deployment-and-operations.md`) |
| REQ-PERF-026 | With Redis down, composition still answers from upstream calls |
| REQ-L10N-001, REQ-L10N-008, REQ-L10N-010, REQ-L10N-014 | Bootstrap carries language, numerals, time zone, Hijri display and terminology overrides |
| REQ-API-018 | The natural-language search route and the Ai pass-through answer 202 with Ai's assist job, so no request waits on a model |
| REQ-AI-006, REQ-AI-014 | The source re-check and the read tools run under the caller's token, and a natural-language plan is executed here over the owners' list endpoints, never by Ai |
| REQ-OPS-016 | The campus digital twin (feature 33) is composed here from Operations, Scheduling and Attendance (SL-OPS-624); Operations owns the requirement and its floor plans |

---

## 3. Aggregates and entities

None, by design. Bff.Web owns no database (Appendix L.1); every field it returns belongs to the service that produced it, and the only state it keeps is the cache of section 10, which is rebuildable and expendable. Its DTOs are screen shapes, listed in section 4 and generated into `libs/data-access/generated/bff-web/` from `openapi/bff-web.v1.json` (`08-web-structure.md` part 4).

---

## 4. REST API: routes, aggregation endpoints and their upstream calls

All routes are under `/bff/web/v1/`, reached through the Gateway on the tenant host, `bearerAuth` only (`22-api-conventions-and-error-catalog.md` §11.3), except the three internal routes under `/bff/web/v1/internal/ai/`, which the Gateway never routes and only Ai's jobs call in-cluster (the Ai routes table after section 4.1). **Permission** is the upstream permission that gates the screen; Bff.Web declares no permission of its own and forwards the caller's token, so every upstream call is authorised by the owning service against the caller's effective set and scope. A card whose upstream refuses returns that region as `noPermission` rather than failing the page. Errors are the eight Appendix K.1 suffixes with the `BFF_` prefix for failures before any upstream is reached (`22-api-conventions-and-error-catalog.md` part 8); an upstream Appendix K code is passed through verbatim inside the region's `problem` object and never rewritten. Every route is a `GET` except where marked, is safe, and supports `ETag` and `If-None-Match`.

### 4.1 Route table

| Method | Path | Permission (gate) | Request | Response | Errors | Idempotent |
|---|---|---|---|---|---|---|
| GET | `/bff/web/v1/me/bootstrap` | signed in | `If-None-Match` | `Bootstrap`: user, tenant snapshot, effective permissions and version, landing workspace, reference slices, product name | `BFF_TENANT_MISMATCH`, `BFF_DEPENDENCY_UNAVAILABLE` (Identity down; nothing can render without permissions) | Safe |
| GET | `/bff/web/v1/me/permissions` | signed in | `If-None-Match` | `EffectivePermissions` with `ETag` = `permissionVersion` | `BFF_DEPENDENCY_UNAVAILABLE` | Safe; `304` when unchanged |
| GET | `/bff/web/v1/me/reference?slices=` | signed in | slices: terminology, sections, grade-levels, subjects, bell-schedule, calendar | `ReferenceSlices` | `BFF_VALIDATION_FAILED` (unknown slice) | Safe |
| GET | `/bff/web/v1/me/counters` | signed in | none | Unread messages, unread inbox, open tasks, pending approvals | none beyond K.1 | Safe; 15 s L1 |
| GET | `/bff/web/v1/me/preferences` | signed in (self) | none | Saved views, table columns, density, pinned quick actions | `BFF_DEPENDENCY_UNAVAILABLE` | Safe |
| PUT | `/bff/web/v1/me/preferences` | signed in (self) | `Preferences`, `If-Match` | 200 `Preferences` | `BFF_CONCURRENCY_CONFLICT` passed from Identity | Yes, by `If-Match` |
| GET | `/bff/web/v1/admin/view-as?roleCode=&userId=` | `identity.permissions.explain-effective` | query | Effective set of the role or user, for the preview overlay only | none beyond K.1 | Safe; never cached |
| GET | `/bff/web/v1/home/teacher` | `academics.teaching-assignments.view` | `date` (default today in the campus time zone) | `TeacherHome` | none beyond K.1; regions may be `partial` | Safe |
| GET | `/bff/web/v1/home/homeroom` | `attendance.student-attendance.view` (own-homeroom) | `date` | `HomeroomHome` | as above | Safe |
| GET | `/bff/web/v1/home/student` | `scheduling.timetable.view` (self) | `date` | `StudentHome` | as above | Safe |
| GET | `/bff/web/v1/home/parent` | `school.students.view` (own-children) | none | `ParentCalmScreen`: one card per child | as above | Safe |
| GET | `/bff/web/v1/home/parent/children/{studentId}` | `school.students.view` (own-children) | `date` | `ChildToday` | `BFF_NOT_FOUND` when the child is not the caller's | Safe |
| GET | `/bff/web/v1/home/principal?lens=` | `reporting.dashboards.view` | `lens` = principal, vice-principal, owner, coordinator, department (card order only; Appendix I) | `PrincipalHome`: morning brief and Today | as above | Safe |
| GET | `/bff/web/v1/home/registrar` | `admissions.applications.view` | none | `RegistrarHome` | as above | Safe |
| GET | `/bff/web/v1/home/accountant` | `finance.payments.view` | `date` | `AccountantHome` | as above | Safe |
| GET | `/bff/web/v1/home/hr` | `hr.leave.view` | none | `HrHome` | as above | Safe |
| GET | `/bff/web/v1/home/care` | `wellbeing.interventions.view` | `date` | `CareHome`: counts only, never case content | as above | Safe; never cached beyond counts |
| GET | `/bff/web/v1/home/front-desk` | `operations.frontdesk.view` | `date` | `FrontDeskHome` | as above | Safe |
| GET | `/bff/web/v1/home/admin` | `platform.settings.view` | none | `AdminHome`: setup checklist, usage against plan, data-quality summary | as above | Safe |
| GET | `/bff/web/v1/home/operator` | `platform.tenants.view` (platform scope) | none | `OperatorToday` (platform console) | as above | Safe |
| GET | `/bff/web/v1/students/{studentId}/360` | `school.students.view` | `tab` | `Student360`: header, tab list with counts and access flags, first timeline page | `BFF_NOT_FOUND` (student outside scope, no hint it exists) | Safe |
| GET | `/bff/web/v1/students/{studentId}/360/timeline?cursor=&kinds=&from=&to=` | `school.students.view` | query, keyset cursor | Timeline page, newest first, page cap 50 | `BFF_VALIDATION_FAILED` (bad cursor) | Safe |
| GET | `/bff/web/v1/students/{studentId}/transparency` | `audit.access-transparency.view` (own-children) | none | Reads of sensitive records by role and time, consents, retention clocks | `BFF_NOT_FOUND` | Safe; never cached |
| GET | `/bff/web/v1/campus/digital-twin?campusId=&date=` | `operations.facilities.view` | `campusId`; `date` (default today in the campus time zone) sets the day for live occupancy and the week for the heatmap | `DigitalTwin`: floor plans with room polygons. Per room, it carries the timetabled or booked use now, the present count from today's roll call, open tickets, and a utilisation heatmap over the week, which lays out Scheduling's holds per room and period as held or free. Bff.Web sums nothing. It shows rooms and counts, never a student name or id (REQ-OPS-016, feature 33) | none beyond K.1; the occupancy and heatmap regions may be `partial` | Safe; never cached here (Operations caches the floor plans) |
| GET | `/bff/web/v1/search?q=&kinds=&limit=` | signed in; each result kind gated by its upstream view permission | `q` at least 2 characters, Arabic-normalized upstream | `SearchResults` grouped by kind plus permitted navigation and action entries | `BFF_VALIDATION_FAILED` | Safe; never cached |
| POST | `/bff/web/v1/search/natural-language` | `ai.assistant.use` | `{ question, language }`, `Idempotency-Key` | **202** `{ jobId, location }`: the assist job Ai returns for `POST /api/v1/ai/query-plans` with `featureCode = nl-search` (REQ-API-018; no request waits on a model). When Ai answers 200 degraded at rung 1, or Ai is not deployed, the route answers **200** with keyword `SearchResults` from the `search` route, `degraded: true` and the `degradedReason` | `AI_DISABLED_FOR_TENANT` (403, the client hides the entry point), `AI_LANGUAGE_UNSUPPORTED`, `BFF_VALIDATION_FAILED` (question over 200 characters) | By `Idempotency-Key`, forwarded to Ai |
| GET | `/bff/web/v1/search/natural-language/{jobId}` | `ai.assistant.view` | none | **202** with the job status while it runs; then **200** `NaturalLanguageResults`: Bff.Web runs the job's `QueryPlan` over the owning services' list endpoints under the caller's token, so every row is authorized by its owner, and returns the rows grouped by kind with the plan's filters as the Because explanation and any `AI_SCOPE_VIOLATION_BLOCKED` warning of a plan narrowed to scope | `AI_NOT_FOUND` passed through (another user's job, a second read, or after 15 minutes) | Safe; the result is readable once, as Ai's is |
| any | `/bff/web/v1/ai/{path}` | the Appendix B permission Ai's own route declares (`06-services/ai.md` §5) | as Ai's route | as Ai's route, unchanged: drafts, translations and asks answer 202 with the assist job, configuration and usage answer as Ai's section 5 says | Ai's `AI_*` codes passed through unchanged | As Ai's route; `Idempotency-Key` and `If-Match` forwarded |
| GET | `/bff/web/v1/internal/ai/sources/{sourceEntity}?cursor=` | **Internal**: Ai's client-credentials service token (`12-security-privacy-safety.md` §3.3) whose scope names this source entity; no user | `cursor` from the last page, or none for a full read | A page of at most 200 records of the entity changed since the cursor, each with its source service, id, `sourceVersion`, data class, required permission and scope tags (campus, sections, students) and the text fields document 25 §4.2 lists for it; the next cursor; the last page also carries the entity's record count and highest `sourceVersion` for Ai's nightly reconciliation | `BFF_PERMISSION_DENIED` for an entity the credential does not name, and always for message bodies, Wellbeing and any Sensitive or level S field (T-AI-03); `BFF_VALIDATION_FAILED` (bad cursor, unknown entity) | Safe |
| POST | `/bff/web/v1/internal/ai/sources/authorize` | **Internal**: the caller's token, forwarded by Ai's job | `{ sources: [{ sourceService, sourceEntity, sourceId }] }`, at most 8 | `{ permitted: [...] }`: the subset the caller may read now, each confirmed by the owning service's own read under the caller's token (document 25 §4.4 step 5) | `BFF_VALIDATION_FAILED` (more than 8, unknown entity); an upstream refusal drops the source, never fails the call | Safe |
| POST | `/bff/web/v1/internal/ai/tools/{toolName}` | **Internal**: the caller's token, forwarded by Ai's job | the tool's arguments | The result of the one read-only GET operation `toolName` maps to, called on the owning service under the caller's token | `BFF_NOT_FOUND` (a tool name outside the map), `BFF_PERMISSION_DENIED` (a mapping to any operation other than `GET`, refused before a call); the owner's code passed through | Safe |
| GET | `/bff/web/v1/console/jobs?tenantId=&state=` | `platform.jobs.view` | query | Jobs from every service that runs them, merged and ordered by age | none beyond K.1; a silent service shows as `partial` | Safe |
| GET | `/bff/web/v1/manifest.webmanifest` | none (public, per tenant host) | host | Web manifest with the tenant's white-label name, icons and theme colour | `BFF_NOT_FOUND` for an unknown host | Safe; `Cache-Control: max-age=3600` |
| GET | `/.well-known/assetlinks.json` | none (public, per tenant host; Gateway maps the path to Bff.Web) | host | Android App Links statement for the shared app and every white-label package | `BFF_NOT_FOUND` | Safe |
| GET | `/.well-known/apple-app-site-association` | none (public, per tenant host) | host | Universal Links association | `BFF_NOT_FOUND` | Safe |

**Route count: 35**, counted from the table: 32 reached through the Gateway (the `/bff/web/v1/ai/{path}` pass-through counts once, whatever Ai path it forwards) and 3 internal routes that only Ai's jobs call.

**The Ai routes.** Reference architecture Section 8.0 gives Bff.Web two jobs for Ai, and the rows above are both:

| Kind | Routes | Who calls | Rule |
|---|---|---|---|
| Assist submission | `/bff/web/v1/ai/{path}` and the two natural-language search routes | The web client through the Gateway | Ai is never routed from outside (`06-services/gateway.md` §5.1), so the web client reaches Ai's section 5 only through this host. The pass-through forwards the caller's token, the tenant and the correlation id unchanged and adds nothing: Ai authorizes, meters and degrades. Every model call answers 202 with an assist job, which the client polls on the job route |
| Internal routes for Ai's jobs | `GET /bff/web/v1/internal/ai/sources/{sourceEntity}?cursor=`, `POST /bff/web/v1/internal/ai/sources/authorize`, `POST /bff/web/v1/internal/ai/tools/{toolName}` | `ai-api` and `ai-worker` only, in-cluster | The Gateway never routes `/bff/web/v1/internal/` (`TC-GW-751`) and a network policy admits only the two Ai workloads. The source feed runs under Ai's service credential, limited per source entity; the re-check and the tools run under the caller's token that Ai's job holds in memory. Each is one hop from an Ai job to Bff.Web, answered from the owning service's own API, and none is made inside a request Ai is serving (`05-service-catalog.md`, "Ai over REST, through Bff.Web") |
### 4.2 Upstream calls behind each aggregation endpoint

Every composition fans out in parallel with a per-call deadline (section 5), forwards the caller's bearer token, `X-Nibras-Tenant-Id`, the correlation id and `traceparent`, and assembles the response even when some calls fail. Attendance and Assessment paths, and the Operations and Scheduling paths of the digital twin, are quoted from their sheets; paths of the other services are pinned by the consumer pacts in `tests/Nibras.Bff.Web.Tests/Contracts/` against each service's published OpenAPI, and are named here by service and resource.

| Endpoint | Upstream calls (service → resource) | Source of truth for the card |
|---|---|---|
| `me/bootstrap` | Identity → current user and effective permissions with `permissionVersion`; Platform → tenant settings snapshot, branding, terminology, feature flags, plan limits; School → sections, grade levels, subjects of the caller's campus; Scheduling → active bell schedule | Identity for permissions; Platform for tenant; School and Scheduling for reference |
| `me/permissions` | Identity → effective permissions (served from the shared permission cache entry `nibras:{tenant}:identity:permissions:*`, read-only ACL) | Identity |
| `me/reference` | Platform, School, Scheduling as in bootstrap, only the named slices | as bootstrap |
| `me/counters` | Communication → unread message count; Notification → unread inbox count per category; Requests → open tasks and pending approvals for the caller | each owner |
| `me/preferences` | Identity → user preferences (Open point 3) | Identity |
| `admin/view-as` | Identity → explain-effective for the named role or user | Identity |
| `home/teacher` | Scheduling → the caller's timetable today and substitutions assigned; Attendance → `GET /api/v1/attendance/sessions` (attendance to mark) and `GET /api/v1/attendance/offline-reviews?mine=true` (pending edit-after-lock items); Academics → submissions to grade; Communication → unread messages and today's meetings; Assessment → `GET /api/v1/assessment/sections/{id}/mark-status` per section taught; Reporting → class averages, missing work, attendance per class, grading turnaround | Live services for the action cards; Reporting for indicators |
| `home/homeroom` | Reporting → absent today and flags for the class (early-warning), class overview per student; Attendance → `GET /api/v1/attendance/excuses?status=Submitted&sectionId=` (excuses to review); School → birthdays in the homeroom | Reporting and Attendance (REQ-RPT-004) |
| `home/student` | Scheduling → today's timetable; Academics → due soon and new feedback; Communication → announcements; Reporting → grades trend, attendance, badges | as named |
| `home/parent` | School → the caller's linked children; per child in parallel: Reporting → today's status, attendance, recent grades, behavior points (from `student_360`); Academics → what is due; Finance → fees due and balance; Requests → open requests; Communication → unread messages; Scheduling → upcoming events | as named; "nothing needs your attention" is a designed state when every card is empty (Master brief Section 12.1 item 35) |
| `home/parent/children/{id}` | The parent calls for one child plus Attendance → `GET /api/v1/attendance/students/{id}/summary` and today's gate pass (`GET /api/v1/attendance/safety/gate-passes?date=` filtered to the child) | as named |
| `home/principal` | Requests → approvals waiting; Attendance → `GET /api/v1/attendance/sessions/unmarked` and `GET /api/v1/attendance/safety/visitors?status=CheckedIn`; Scheduling → staff absences and cover; Behavior → incidents today; Reporting → at-risk students (early-warning), overdue grading (`staffing_facts`), attendance rate, punctuality, behavior trend, request SLA, fee collection, syllabus coverage; Hr → staff absences where the tenant runs Hr | Live services for action cards (freshness under 60 s matters at 07:40); Reporting for indicators (N-05) |
| `home/registrar` | Admissions → new inquiries, applications per stage, offers expiring, seats left; Documents → documents missing; Reporting → funnel conversion, time in stage, re-enrollment rate | as named |
| `home/accountant` | Finance → payments to reconcile, refunds to approve, cheques due, overdue ladder today; Reporting → collections, aging, discounts, forecast (`finance_balance_facts`) | as named |
| `home/hr` | Hr → leave to approve, documents expiring, probation ending, vacancies; Reporting → headcount, absence rate, turnover | as named |
| `home/care` | Wellbeing → referrals count, follow-ups due count, visits today count, medication schedule count; Reporting → caseload and outcome aggregates | Wellbeing counts only; no case field ever crosses this host |
| `home/front-desk` | Attendance → `GET /api/v1/attendance/safety/visitors?status=CheckedIn`, `GET /api/v1/attendance/safety/gate-passes?date=&status=Issued` (expected pickups, passes to verify); Operations → front-desk enquiries and complaints | as named |
| `home/admin` | Platform → setup checklist state and usage against plan; Reporting → data-quality summary | as named |
| `home/operator` | Platform → provisioning in progress, trials ending, tickets at risk; Reporting → `platform_health` projection (failing services, queue alarms, failed messages) | platform-scoped |
| `students/{id}/360` | School → student header (name, photo reference, section, status; never custody text); Reporting → `student_360` read model (timeline and counts); for each tab the owning service's permission check through a `HEAD` or count read: Attendance `GET /api/v1/attendance/students/{id}/summary`, Assessment `GET /api/v1/assessment/students/{id}/term-results`, Behavior, Finance, Communication, Documents, Wellbeing (existence and count only) | Reporting for the timeline; the owning service decides whether the viewer may open each tab |
| `students/{id}/360/timeline` | Reporting → `student_360` timeline page by keyset; entries of kinds the viewer cannot read are replaced by an "exists, no access" marker with no content | Reporting |
| `students/{id}/transparency` | Audit → the guardian transparency facts for the child's sensitive records (role, time, category; never reader names, never content); Platform → consents and retention clocks per category | Audit, Platform |
| `campus/digital-twin` | Operations → `GET /api/v1/operations/facilities/floor-plans?campusId=` (plans, room polygons, open tickets per room); Scheduling → `GET /api/v1/scheduling/rooms/availability?campusId=&from=&to=` for the day and for the week (room holds from the timetable and bookings, the heatmap source); Attendance → `GET /api/v1/attendance/reports/daily-register?campusId=&date=` (register per section and period, from which only the present count per section and period is kept; no student row leaves the composer) | Operations for geometry and tickets; Scheduling for which section or booking holds a room; Attendance for how many are present. The composer joins them by room and section and computes nothing else (SL-OPS-624) |
| `search` | School → students, guardians, staff; Requests → requests; Documents → files the caller may read; Academics → assignments; plus the static navigation manifest filtered by the effective set | each owner; results are the union of what each owner returned for this caller |
| `search/natural-language` (POST) | Ai → `POST /api/v1/ai/query-plans` with `featureCode = nl-search` under the caller's token; on a degraded answer or with Ai absent, the `search` fan-out above instead | Ai for the job; the owners for any keyword fallback |
| `search/natural-language/{jobId}` (GET) | Ai → `GET /api/v1/ai/assist-jobs/{jobId}`; once it holds a `QueryPlan`, each list endpoint the plan names, on its owning service, in parallel under the caller's token, at most one call per service | The owning services for every row; Ai only for the plan |
| `ai/{path}` | Ai → `/api/v1/ai/{path}`, the same method and body, 2 s deadline (every model call answers 202 at once) | Ai |
| `internal/ai/sources/{sourceEntity}` | The one owning service of the entity (Communication for announcements, Assessment for report-card comments and marks summaries, Academics for lesson plans, Attendance for weekly attendance summaries, Behavior for behavior categories without restricted narratives; document 25 §4.2) → its list endpoint filtered by change since the cursor, under Ai's service token; 10 s per page of 200 | The owning service |
| `internal/ai/sources/authorize` | Each distinct owning service among the at most 8 sources → its own read of the source under the caller's token, in parallel; 800 ms for the whole call | The owning services; a refusal or a timeout drops the source |
| `internal/ai/tools/{toolName}` | The one owning service the tool maps to → its `GET` operation under the caller's token; 2 s per call, and Ai makes at most 3 per job | The owning service |
| `console/jobs` | The `/jobs` resource of Assessment, Documents, Scheduling, Finance, Reporting, Platform (document 22 §6) | each owner |
| `manifest.webmanifest`, `.well-known/*` | Platform → branding and the white-label flavor registry for the host | Platform |

### 4.3 Response shaping

| Rule | Detail |
|---|---|
| One region per card | Every home payload is `{ asOf, regions: [{ key, status: ready \| empty \| partial \| noPermission \| unavailable, asOf, data, problem, action }] }`, so the client binds each region to the seven states of `14-design-system-and-ux.md` |
| No hint of what is hidden | A region the caller cannot see is omitted when its existence is itself sensitive (Wellbeing under Appendix K.22 rule 5) and returned as `noPermission` otherwise |
| Counts, not rows, for sensitive sources | Wellbeing regions carry counts; Finance regions for guardians carry balances; nothing Sensitive or level S crosses the host |
| Size | Every composed payload under 32 KB after Brotli (`21-performance-engineering.md` §10); the Student 360 first page carries at most 20 timeline entries |
| Localization | Labels are keys, never sentences; numbers and dates in ISO form; the client renders them per tenant numerals and calendar |
| Links | Each card's `action` is a route from the navigation manifest with the permission it needs, so a card never links to a screen the caller cannot open |
| Freshness | Each region carries its own `asOf`; Reporting regions carry the projection checkpoint time and a `rebuilding` flag during a rebuild (`10-data-architecture.md` §7.3) |
| Read your writes | A request carrying `Nibras-Expect-Message` forwards it to Reporting, which reads the primary once when the replica lags (`10-data-architecture.md` §7.4) |

---

## 5. gRPC

None, exposed or consumed, by design: "Bff.Web and Bff.Mobile call services over HTTP, fan out in parallel, and are not counted as a hop because they own no data and make no gRPC calls" (`22-api-conventions-and-error-catalog.md` §10.4).

Upstream HTTP policy, stated here because it replaces the gRPC deadlines of a service sheet:

| Aspect | Value |
|---|---|
| Deadline per upstream call | 400 ms for home and bootstrap regions, 800 ms for Student 360, search and the digital twin, 2 s for `console/jobs` and the `ai/{path}` pass-through; bounded by the Gateway's 30-second request timeout. The internal Ai routes carry Ai's own budgets (`06-services/ai.md` §6): 10 s per source-feed page of 200, 800 ms for a re-check of up to 8 sources, 2 s per tool call |
| Retries | One retry on connection failure or 503 for `GET`, with 50 to 100 ms jitter; none for the preferences `PUT` |
| Circuit breaker | Per upstream service through the resilience pipeline of `Nibras.BuildingBlocks.Web`: opens at 50 percent failures over 30 s with at least 20 calls, half-open after 15 s; an open breaker returns the region as `unavailable` at once |
| Bulkhead | At most 64 concurrent calls per upstream per instance, so one slow service cannot exhaust the host |
| Total composition budget | Home p95 under 250 ms, p99 under 600 ms (N-05); a region that misses its deadline is returned `partial` with the rest of the page |

---

## 6. Events published and consumed

None, by design. Bff.Web owns no exchange and no queue (`11-messaging-architecture.md` part 1): it publishes nothing and consumes no RabbitMQ event. The event-driven invalidation of its cache entries in `21-performance-engineering.md` §1.21 reaches it as the `state:invalidate` pub/sub broadcast of `Nibras.BuildingBlocks.Caching` (§2.6), which its ACL user may subscribe to (§2.3); section 10 lists which broadcast evicts which entry.

---

## 7. Sagas and workflows

None owned; no workflow identifier in Appendix R names Bff.Web. It composes the read side of several: WF-ATT-01 (unmarked attendance and excuses to review on the principal and homeroom homes), WF-ASM-01 (overdue grading and batch progress through `console/jobs`), WF-RQS-01 (approvals and tasks counters). It never drives a transition.

---

## 8. Local reference copies

None, by design. Reference slices in the bootstrap are read from their owners per request and cached under section 10; nothing is copied, stored or reconciled here.

---

## 9. Background jobs

None. Bff.Web runs no Quartz.NET job. The pre-peak warm-up of the entries it reads (tenant context, permissions of staff with a lesson today, the day's timetable) is Platform's `PrePeakWarmUp` job (`21-performance-engineering.md` §9); the BFF's own home entries warm naturally on the first request because their lifetime is 60 s.

---

## 10. Permissions, notifications, settings, error codes

| Permissions | Bff.Web declares none of its own and holds none (T-GW-04). The gate column of section 4.1 names the Appendix B permission the web route guard and the upstream both check; the generated permission-matrix suite exercises every BFF route with every Appendix I role and asserts that each region's status matches what the upstream allows |
|---|---|

| Notifications | none triggered; Bff.Web publishes nothing |
|---|---|

| Settings read (Appendix G) | Where it goes |
|---|---|
| General → languages, default language, numerals, time zone, work week, calendars, currency, terminology overrides, branding and theme | Bootstrap tenant snapshot |
| Security → session timeout | Bootstrap, for the client's idle timer |
| AI → enabled features | Bootstrap feature list; gates `search/natural-language` |
| Integrations → the white-label flavor values | `manifest.webmanifest` and `.well-known` files |

| Error codes | HTTP | When |
|---|---|---|
| `BFF_VALIDATION_FAILED` | 400 | Bad query parameter, unknown slice, bad cursor |
| `BFF_PERMISSION_DENIED` | 403 | Route gate refused before any upstream call (a route the caller's effective set cannot reach at all); on the internal Ai routes, a source entity the service credential does not name, or a tool mapped to anything but a `GET` |
| `BFF_TENANT_MISMATCH` | 403 | Token tenant differs from the forwarded tenant |
| `BFF_NOT_FOUND` | 404 | Unknown host for per-tenant artefacts; a student or child outside scope; a tool name outside `AiToolMap` |
| `BFF_CONCURRENCY_CONFLICT` | 409 | Preferences `If-Match` failure passed through |
| `BFF_IDEMPOTENCY_REPLAY` | 200 | Not raised by a route today; reserved by the shared middleware |
| `BFF_RATE_LIMITED` | 429 | Per-user limit of the Web building block (for example search at 10 per second) |
| `BFF_DEPENDENCY_UNAVAILABLE` | 503 | Only when a region the page cannot render without is unavailable (Identity on bootstrap); otherwise regions degrade to `unavailable` |

Upstream codes, for example `ATTENDANCE_PERMISSION_DENIED` or `AI_DISABLED_FOR_TENANT`, pass through unchanged inside the region's `problem` (Appendix K.22 rule 1).

---

## 11. Caching and hot queries

The entries are `21-performance-engineering.md` §1.21 (role home payload per user, 15 s L1 and 60 s L2; navigation and permission bootstrap per user and permission version, 60 s and 30 min); they are not repeated. What this sheet adds:

| Data | Key | Tags | L1 | L2 | Invalidated by | Never cached |
|---|---|---|---|---|---|---|
| Reference slices per campus | `nibras:{tenant}:bff:reference:{campusId}:{slice}:v1` | `tenant`, `campus` | 60 s | 30 min ± 10% | `state:invalidate` broadcast for tag `tenant` after `platform.settings.changed.v1`, `platform.terminology.changed.v1`, `school.section.changed.v1` in the owners | Nothing |
| Shell counters per user | `nibras:{tenant}:bff:counters:{userId}:v1` | `tenant`, `user` | 15 s | none (L1 only) | Lifetime only | Nothing |
| Student 360 header per viewer and student | `nibras:{tenant}:bff:s360-header:{viewerId}:{studentId}:v1` | `tenant`, `user`, `student` | 15 s | 60 s ± 10% | Broadcast for tag `student` after `school.student.profile-updated.v1`, `school.student.section-changed.v1`, `school.student.status-changed.v1` | Custody, medical summary, wellbeing content; the timeline is never cached here (Reporting owns its freshness) |
| Per-tenant web manifest and association files (platform-scoped, per host) | `nibras:platform:bff:web-manifest:{host}:v1` | none | 5 min | 1 h ± 10% | Branding publish handler in Platform evicts by key through the broadcast | Nothing |

**Never cached in Bff.Web:** search results, natural-language answers, the digital twin (its occupancy is live; Operations caches the floor plans), the transparency panel, the view-as-role set, Student 360 timeline pages, Wellbeing content beyond counts, any response carrying a signed URL. Every key carries the full tenant UUID and, for anything Confidential, the `userId` (§1 conventions); a composed payload is cached per user, never shared across users, because the permission that shaped it is part of its identity.

**Hot paths and budgets.** Bff.Web runs no query. Its budgets are the composition budgets: home p95 under 250 ms and p99 under 600 ms with a cache hit ratio at or above 95 percent after the first request (N-05); first screen of every workspace at most two Bff.Web calls (`21-performance-engineering.md` §10); each home at most one upstream call per card and at most 5 database commands per card upstream (`16-test-strategy.md` query budgets for "Dashboard card from a read model"). The metric is `nibras_bff_compose_duration_seconds` by screen with upstream failures and circuit states per service (`15-deployment-and-operations.md`).

---

## 12. Security

The threat row is `12-security-privacy-safety.md` §2.21 T-GW-04 (the backend composes a screen with fields the user may not see; control: the backend forwards the caller's token, holds no permissions of its own and stores nothing; test `TC-SEC-331`). The guardian transparency panel is the child-safety control of §7 of that document (`TC-PRV-501`, `TC-PRV-702`).

| Data class (Appendix J) | Handling here |
|---|---|
| Public | Web manifest and association files, cached by host |
| Internal | Reference slices, cached with the tenant in the key |
| Confidential | Home regions, Student 360 header, counters: per-user keys, L2 at most 60 s (Appendix J level Confidential) |
| Sensitive | Never composed: custody text, medical detail, payment instruments, excuse medical detail, gate-pass material. A tab that would show them links to the owning screen, which reads live with its own access log |
| S (Wellbeing) | Counts and existence only; never cached, never logged, never in a timeline entry's content |

| Never | What |
|---|---|
| Cached | The items listed under "Never cached" in section 11 |
| Logged | Response bodies, search text, natural-language questions, student names; logs carry route, status, upstream timings, tenant id and correlation id |
| Sent to the browser | Any field the upstream did not return for this caller; the view-as-role preview never carries data, only the permission set |

The BFF runs under its own client-credentials identity only for the platform-scoped per-host artefacts; every tenant data call uses the caller's forwarded token so that the owning service's authorization, scope and row-level security apply unchanged. The one exception is the internal source feed, which forwards Ai's own service token, scoped per source entity, and never widens it: the owning service answers that token only for the entity it names, and never with message bodies, Wellbeing or a Sensitive or level S field (T-AI-03 in `12-security-privacy-safety.md` §2.20).

---

## 13. Folder and file tree

Document 07 §2.1 gives this host one project and one test project and forbids any reference to a `Nibras.<Service>.*` project (rule `Hosts_NeverReference_ServiceProjects`, TC-TST-114). The tree follows the Attendance anatomy where it applies: one folder per use case under `Features/` named after the routes of section 4, the caching and permission-free security folders, and the test project; the folders that exist only for data owners are listed as absent, the way document 07 lists `Sagas/` for Attendance.

```text
src/Bff.Web/                                                      web backend-for-frontend, image nibras/bff-web
├── Nibras.Bff.Web/                                               role home payloads, Student 360 composition from Reporting, navigation and permission bootstrap
│   ├── Nibras.Bff.Web.csproj                                     references Nibras.ServiceDefaults, Nibras.BuildingBlocks.* and Nibras.Contracts.Shared only
│   ├── README.md                                                 purpose, routes, upstream map, budgets, runbook links
│   ├── Program.cs                                                composition root: ServiceDefaults, Web, Caching, Observability, upstream clients, endpoints, probes
│   ├── Features/                                                 one folder per aggregation endpoint: Query, Handler (composer), Validator, Endpoint
│   │   ├── GetBootstrap/                                         GET /bff/web/v1/me/bootstrap
│   │   │   ├── GetBootstrapQuery.cs                              record: caller, tenant, If-None-Match
│   │   │   ├── GetBootstrapHandler.cs                            parallel Identity, Platform, School, Scheduling reads; landing workspace in Appendix I order
│   │   │   ├── GetBootstrapValidator.cs                          tenant header equals token tenant
│   │   │   └── GetBootstrapEndpoint.cs                           route, ETag, 304
│   │   ├── GetPermissions/                                       GET /bff/web/v1/me/permissions
│   │   │   ├── GetPermissionsQuery.cs                            record: caller, held ETag
│   │   │   ├── GetPermissionsHandler.cs                          reads the shared permission cache entry, falls back to Identity
│   │   │   ├── GetPermissionsValidator.cs                        ETag format
│   │   │   └── GetPermissionsEndpoint.cs                         route, ETag equals permissionVersion
│   │   ├── GetReference/                                         GET /bff/web/v1/me/reference
│   │   │   ├── GetReferenceQuery.cs                              record: slices
│   │   │   ├── GetReferenceHandler.cs                            per-slice owner reads with the reference cache entry
│   │   │   ├── GetReferenceValidator.cs                          known slice names
│   │   │   └── GetReferenceEndpoint.cs                           route
│   │   ├── GetCounters/                                          GET /bff/web/v1/me/counters
│   │   │   ├── GetCountersQuery.cs                               record: caller
│   │   │   ├── GetCountersHandler.cs                             Communication, Notification, Requests counts in parallel
│   │   │   ├── GetCountersValidator.cs                           none beyond the caller
│   │   │   └── GetCountersEndpoint.cs                            route
│   │   ├── Preferences/                                          GET and PUT /bff/web/v1/me/preferences, a pass-through to Identity
│   │   │   ├── PreferencesQuery.cs                               record: caller, or the new preferences with If-Match
│   │   │   ├── PreferencesHandler.cs                             forwards unchanged; no field is interpreted here
│   │   │   ├── PreferencesValidator.cs                           size cap 16 KB
│   │   │   └── PreferencesEndpoint.cs                            routes
│   │   ├── GetViewAsRole/                                        GET /bff/web/v1/admin/view-as
│   │   │   ├── GetViewAsRoleQuery.cs                             record: role code or user id
│   │   │   ├── GetViewAsRoleHandler.cs                           Identity explain-effective; never cached
│   │   │   ├── GetViewAsRoleValidator.cs                         exactly one of role or user
│   │   │   └── GetViewAsRoleEndpoint.cs                          route
│   │   ├── GetRoleHome/                                          GET /bff/web/v1/home/{role}, one composer per role in Composition/Homes/
│   │   │   ├── GetRoleHomeQuery.cs                               record: role, lens, date, child id
│   │   │   ├── GetRoleHomeHandler.cs                             picks the composer, reads and writes the home cache entry
│   │   │   ├── GetRoleHomeValidator.cs                           role known, date within 7 days of today, child is the caller's
│   │   │   └── GetRoleHomeEndpoint.cs                            the thirteen home routes
│   │   ├── GetStudent360/                                        GET /bff/web/v1/students/{id}/360 and /360/timeline
│   │   │   ├── GetStudent360Query.cs                             record: student id, tab, cursor, kinds, range
│   │   │   ├── GetStudent360Handler.cs                           header, tab access flags and the Reporting timeline page
│   │   │   ├── GetStudent360Validator.cs                         cursor shape, kinds known, range at most one year
│   │   │   └── GetStudent360Endpoint.cs                          routes
│   │   ├── GetTransparency/                                      GET /bff/web/v1/students/{id}/transparency
│   │   │   ├── GetTransparencyQuery.cs                           record: student id
│   │   │   ├── GetTransparencyHandler.cs                         Audit access log and Platform consents and clocks; never cached
│   │   │   ├── GetTransparencyValidator.cs                       caller is a guardian of the student
│   │   │   └── GetTransparencyEndpoint.cs                        route
│   │   ├── GetDigitalTwin/                                       GET /bff/web/v1/campus/digital-twin (feature 33, SL-OPS-624)
│   │   │   ├── GetDigitalTwinQuery.cs                            record: campus id, date
│   │   │   ├── GetDigitalTwinHandler.cs                          parallel Operations, Scheduling and Attendance reads; hands them to the composer
│   │   │   ├── GetDigitalTwinValidator.cs                        campus in the caller's scope, date within 7 days of today
│   │   │   └── GetDigitalTwinEndpoint.cs                         route; never cached
│   │   ├── Search/                                               GET /bff/web/v1/search
│   │   │   ├── SearchQuery.cs                                    record: text, kinds, limit
│   │   │   ├── SearchHandler.cs                                  parallel owner searches, merged by kind, plus permitted manifest entries
│   │   │   ├── SearchValidator.cs                                length 2 to 200, kinds known
│   │   │   └── SearchEndpoint.cs                                 route, per-user rate limit
│   │   ├── NaturalLanguageSearch/                                POST /search/natural-language and GET /search/natural-language/{jobId}
│   │   │   ├── NaturalLanguageSearchRequests.cs                  records: question and language, or job id
│   │   │   ├── NaturalLanguageSearchHandler.cs                   submits the nl-search query plan to Ai (202); runs a finished plan over the owners' list endpoints; keyword fallback when degraded
│   │   │   ├── NaturalLanguageSearchValidator.cs                 question 2 to 200 characters, language ar or en
│   │   │   └── NaturalLanguageSearchEndpoint.cs                  routes, per-user rate limit, Idempotency-Key forwarded
│   │   ├── AiPassThrough/                                        any method on /bff/web/v1/ai/{path}
│   │   │   ├── AiPassThroughRequest.cs                           record: method, path, body, forwarded headers
│   │   │   ├── AiPassThroughHandler.cs                           forwards to Ai section 5 unchanged under the caller's token; adds and removes nothing
│   │   │   ├── AiPassThroughValidator.cs                         path stays under /api/v1/ai/ after normalisation, no internal route reachable
│   │   │   └── AiPassThroughEndpoint.cs                          route; 2 s deadline, 202 passed through
│   │   ├── AiSourceFeed/                                         GET /bff/web/v1/internal/ai/sources/{sourceEntity}
│   │   │   ├── AiSourceFeedQuery.cs                              record: source entity, cursor
│   │   │   ├── AiSourceFeedHandler.cs                            one owner list read by change since the cursor, pages of 200, count and highest version on the last page
│   │   │   ├── AiSourceFeedValidator.cs                          service token names this entity; entity is one of the five of document 25 §4.2
│   │   │   └── AiSourceFeedEndpoint.cs                           internal route, Ai service credential only
│   │   ├── AiSourceAuthorize/                                    POST /bff/web/v1/internal/ai/sources/authorize
│   │   │   ├── AiSourceAuthorizeCommand.cs                       record: at most 8 sources
│   │   │   ├── AiSourceAuthorizeHandler.cs                       each owner's own read under the caller's token, in parallel; refusal or timeout drops the source
│   │   │   ├── AiSourceAuthorizeValidator.cs                     at most 8, known entities, a user token
│   │   │   └── AiSourceAuthorizeEndpoint.cs                      internal route, 800 ms budget
│   │   ├── AiTools/                                              POST /bff/web/v1/internal/ai/tools/{toolName}
│   │   │   ├── AiToolCommand.cs                                  record: tool name, arguments
│   │   │   ├── AiToolHandler.cs                                  calls the one GET operation the tool maps to, under the caller's token
│   │   │   ├── AiToolValidator.cs                                tool in AiToolMap, mapped operation is a GET, arguments match its query parameters
│   │   │   └── AiToolEndpoint.cs                                 internal route, 2 s deadline
│   │   ├── AiToolMap.cs                                          tool name to owning-service GET operation; Ai's ToolCatalog offers only these names
│   │   ├── GetConsoleJobs/                                       GET /bff/web/v1/console/jobs
│   │   │   ├── GetConsoleJobsQuery.cs                            record: tenant id, state
│   │   │   ├── GetConsoleJobsHandler.cs                          reads every job-running service's /jobs, merged by age
│   │   │   ├── GetConsoleJobsValidator.cs                        platform scope
│   │   │   └── GetConsoleJobsEndpoint.cs                         route
│   │   └── GetTenantArtefacts/                                   manifest.webmanifest and the two .well-known files
│   │       ├── GetTenantArtefactsQuery.cs                        record: host, artefact kind
│   │       ├── GetTenantArtefactsHandler.cs                      Platform branding and flavor registry, platform-scoped cache
│   │       ├── GetTenantArtefactsValidator.cs                    host resolves to a tenant
│   │       └── GetTenantArtefactsEndpoint.cs                     three public routes
│   ├── Composition/                                              the screen composers; no business rule, only fan-out, filtering and shaping
│   │   ├── Homes/                                                one composer per role home of Appendix D
│   │   │   ├── TeacherHomeComposer.cs                            next class, attendance to mark, pending reviews, submissions, messages, meetings, cover, indicators
│   │   │   ├── HomeroomHomeComposer.cs                           absent today, excuses to review, class flags, birthdays (REQ-RPT-004)
│   │   │   ├── StudentHomeComposer.cs                            timetable, due soon, new feedback, announcements, indicators
│   │   │   ├── ParentHomeComposer.cs                             calm screen: one card per child and the designed empty state
│   │   │   ├── ChildTodayComposer.cs                             one child's day including the gate pass card
│   │   │   ├── PrincipalHomeComposer.cs                          morning brief and Today, card order per lens
│   │   │   ├── RegistrarHomeComposer.cs                          inquiries, stages, documents missing, offers, seats, funnel
│   │   │   ├── AccountantHomeComposer.cs                         reconcile, refunds, cheques, ladder, collections, aging
│   │   │   ├── HrHomeComposer.cs                                 leave, expiring documents, probation, vacancies, headcount
│   │   │   ├── CareHomeComposer.cs                               counts only from Wellbeing and Reporting
│   │   │   ├── FrontDeskHomeComposer.cs                          visitors on site, expected pickups, passes to verify, enquiries
│   │   │   ├── AdminHomeComposer.cs                              setup checklist, usage against plan, data-quality summary
│   │   │   └── OperatorTodayComposer.cs                          platform-scoped operator cards
│   │   ├── Regions/                                              shared region shaping
│   │   │   ├── Region.cs                                         the region envelope: key, status, asOf, data, problem, action
│   │   │   ├── RegionStatusMapper.cs                             upstream outcome to ready, empty, partial, noPermission, unavailable
│   │   │   └── ActionLinkResolver.cs                             card action from the navigation manifest with its permission
│   │   ├── DigitalTwin/                                          campus digital twin composition
│   │   │   └── DigitalTwinComposer.cs                            joins floor plans, room holds and present counts by room and section; drops every student row
│   │   ├── Student360/                                           Student 360 composition
│   │   │   ├── TabAccessResolver.cs                              per-tab access flags from the owning services' answers
│   │   │   └── TimelineRedactor.cs                               replaces entries the viewer cannot read with "exists, no access"
│   │   └── Navigation/                                           landing and manifest helpers
│   │       ├── LandingWorkspaceResolver.cs                       first visible workspace in Appendix I order unless the person chose another
│   │       └── NavigationManifest.cs                             the server copy of workspaces.manifest entries used for search and card actions
│   ├── Upstream/                                                 typed HTTP clients, one per service, generated from each service's OpenAPI
│   │   ├── Generated/                                            output of the OpenAPI generator, read-only, one folder per service (twenty)
│   │   ├── UpstreamRegistry.cs                                   base addresses from service discovery, deadlines per endpoint class
│   │   └── FanOut.cs                                             parallel execution with per-call deadline, bulkhead and partial results
│   ├── Resilience/                                               upstream policies from the Web building block
│   │   └── UpstreamPolicies.cs                                   retry once on GET, circuit breaker per upstream, bulkhead of 64
│   ├── Security/                                                 no permissions of its own; propagation only
│   │   ├── TokenForwardingHandler.cs                             forwards the caller's bearer token unchanged
│   │   ├── TenantPropagationHandler.cs                           forwards X-Nibras-Tenant-Id and refuses a mismatch with BFF_TENANT_MISMATCH
│   │   ├── InternalAiCallerPolicy.cs                             internal Ai routes: Ai's service token for the feed, a user token for the re-check and tools, in-cluster callers only
│   │   └── CorrelationHandler.cs                                 forwards the correlation id, traceparent and Nibras-Expect-Message
│   ├── Caching/                                                  what this host caches and what evicts it
│   │   ├── BffWebCacheKeys.cs                                    keys and tags of document 21 §1.21 and section 11
│   │   └── InvalidationSubscriber.cs                             state:invalidate subscriber that evicts home, bootstrap, reference and header entries by tag
│   ├── Contracts/                                                screen-shaped response DTOs published as openapi/bff-web.v1.json
│   │   ├── BootstrapDto.cs                                       bootstrap payload
│   │   ├── HomeDtos.cs                                           one DTO per role home
│   │   ├── Student360Dtos.cs                                     header, tabs, timeline page
│   │   ├── SearchDtos.cs                                         grouped search results and natural-language results with the plan as Because
│   │   └── InternalAiDtos.cs                                     source feed page, re-check answer, tool result; internal, not in the public OpenAPI groups
│   ├── Observability/                                            metrics beyond ServiceDefaults
│   │   └── ComposeMetrics.cs                                     nibras_bff_compose_duration_seconds by screen, upstream failures and circuit states
│   ├── Persistence/                                              absent by design: no database (Appendix L), so the template does not create this folder here
│   ├── Messaging/                                                absent by design: no exchange and no queue (document 11 part 1), so the template does not create this folder here
│   ├── Grpc/                                                     absent by design: no gRPC exposed or consumed (document 22 §10.4), so the template does not create this folder here
│   ├── appsettings.json                                          upstream deadlines, bulkhead sizes, payload cap; no secrets
│   ├── appsettings.Development.json                              Aspire development values
│   └── Dockerfile                                                Debian-based aspnet image, non-root, read-only root filesystem, ICU present, TZ=UTC
└── tests/Nibras.Bff.Web.Tests/                                   composition tests against stubbed service clients
    ├── Nibras.Bff.Web.Tests.csproj                               references the host, the Testing block and PactNet
    ├── Composition/                                              one test class per composer with stubbed upstreams
    │   ├── TeacherHomeComposerTests.cs                           every region, every upstream failure mode
    │   ├── PrincipalHomeComposerTests.cs                         lens ordering, unmarked attendance and visitors from Attendance
    │   ├── ParentHomeComposerTests.cs                            one card per child, the designed empty state
    │   ├── Student360ComposerTests.cs                            redaction and tab access flags per role
    │   ├── DigitalTwinComposerTests.cs                           counts per room, tickets on the plan, no student field, partial regions (TC-BFF-027)
    │   └── HomeComposersTests.cs                                 the remaining homes, table-driven
    ├── Contracts/                                                consumer pacts, one per upstream service (twenty)
    │   └── UpstreamPactTests.cs                                  pins every upstream path and field the composers read
    ├── Payload/                                                  size and shape
    │   └── PayloadSizeTests.cs                                   every composed example under 32 KB after Brotli
    ├── Security/                                                 no widening, no leak
    │   ├── InternalAiRouteTests.cs                               feed scope per entity, re-check drops refused sources, tools refuse non-GET mappings
    │   ├── TokenForwardingTests.cs                               the caller's token reaches every upstream unchanged
    │   └── NoLeakTests.cs                                        no Sensitive or level-S field in any response; logs carry no body
    ├── Caching/                                                  keys and invalidation
    │   └── BffCacheTests.cs                                      tenant and user in every key, permission version in bootstrap, broadcast eviction
    └── Resilience/                                               degradation
        └── PartialRegionTests.cs                                 a stopped upstream returns partial or unavailable regions and the page still renders
```

---

## 14. Test plan

New identifiers are minted from `TC-BFF-001` upward for Bff.Web, numbers 001 to 060 (Bff.Mobile uses `TC-BFF-101` onward); no document in the kit used a `TC-BFF-` identifier before these sheets (searched on 2026-09-21).

| Test case | Level | What it proves |
|---|---|---|
| `TC-RPT-002` (Appendix W) | End to end | Student 360 timeline filtered by the viewer's permissions (REQ-BFF-003) |
| `TC-WEB-001` (Appendix W) | End to end | Command palette and natural-language search limited by permissions (REQ-BFF-006) |
| `TC-RPT-201` (Reporting sheet) | UAT | Homeroom home card shows absent today, excuses to review, flags, birthdays |
| `TC-SEC-331` (document 12) | Security | T-GW-04: no field the user may not see |
| TC-PRV-501, TC-PRV-702 | Privacy | Transparency panel lists reads by role and time without content |
| TC-TST-114 | Architecture | `Nibras.Bff.Web` references no `Nibras.<S>.*` assembly |
| TC-BFF-001 | Architecture | No `Bff.*` project references a Domain project or a database driver (REQ-BFF-001) |
| TC-BFF-002 | End to end | Student 360 page loads with at most 2 browser API calls (REQ-BFF-002) |
| TC-BFF-003 | Integration | Bootstrap returns identity, tenant snapshot, effective permissions and landing workspace in one call; `304` on an unchanged version |
| TC-BFF-004 | Integration | Permission refresh after `identity.permissions.changed.v1` returns the new set within 2 s of the broadcast, no sign-out |
| TC-BFF-005 | Integration | View-as-role returns only a permission set, never data, and is never cached |
| TC-BFF-006 | Composition | Every role home composes each Appendix D card for its role |
| TC-BFF-007 | Composition | A card whose upstream answers 403 is `noPermission`; Wellbeing regions are omitted where existence is sensitive |
| TC-BFF-008 | Resilience | One upstream stopped: its regions `unavailable`, the page renders, p95 unaffected (REQ-GW-007) |
| TC-BFF-009 | Resilience | Identity stopped: bootstrap returns `BFF_DEPENDENCY_UNAVAILABLE`, not a partial permission set |
| TC-BFF-010 | Performance | Composed principal home p95 under 250 ms, p99 under 600 ms, hit ratio at or above 95 percent (N-05) |
| TC-BFF-011 | Contract | Every composed payload example under 32 KB after Brotli |
| TC-BFF-012 | Contract | Twenty consumer pacts verified by the upstream providers |
| TC-BFF-013 | Security | Tenant header differing from the token tenant refused with `BFF_TENANT_MISMATCH` |
| TC-BFF-014 | Isolation | Generated tenant-isolation attack on every BFF route with another tenant's student and child ids returns no data and no hint |
| TC-BFF-015 | Permission matrix | Every route with every Appendix I role: region statuses equal what the upstreams allow |
| TC-BFF-016 | Caching | Every key carries the full tenant UUID and the user id; bootstrap keys carry the permission version |
| TC-BFF-017 | Caching | `state:invalidate` for a tag evicts the matching L1 and L2 entries on every instance within 2 s |
| TC-BFF-018 | Caching | Redis down: every route still answers from upstream calls (REQ-PERF-026) |
| TC-BFF-019 | Security | No Sensitive or level-S field in any response across the fixture corpus; logs contain no response body or search text |
| TC-BFF-020 | Integration | Read your writes: a home request with `Nibras-Expect-Message` shows the teacher's just-marked attendance |
| TC-BFF-021 | Integration | Parent calm screen shows "nothing needs your attention" when every card is empty, and an urgent item breaks through |
| TC-BFF-022 | Integration | Search results never include a kind the caller cannot view; rate limit returns `BFF_RATE_LIMITED` |
| TC-BFF-023 | Integration | Console jobs merges every job-running service and marks a silent one `partial` |
| TC-BFF-024 | Integration | Per-host manifest and association files carry the tenant's white-label values; unknown host is `BFF_NOT_FOUND` |
| TC-BFF-025 | Integration | The correlation id and `traceparent` reach every upstream span of one composition (REQ-GW-008) |
| TC-BFF-026 | Localization | Bootstrap carries numerals, calendar, time zone and terminology; labels are keys only |
| TC-BFF-027 | Composition | The digital twin for a campus of 3 floors, with stubbed Operations, Scheduling and Attendance answers, shows each room's timetabled use, the present count for its section and period, and its open tickets. The week's heatmap marks each room and period held or free exactly as Scheduling returned it. No student name or id appears anywhere in the payload. With Attendance stopped, the occupancy region is `partial` and the plan and tickets still render (REQ-OPS-016, SL-OPS-624; the end-to-end demo test is `TC-OPS-810`, Operations sheet) |
| TC-BFF-750 | Integration | The source feed, called in-cluster with Ai's service token scoped to announcements, returns pages of at most 200 announcements with their versions and scope tags and a next cursor; the same token asking for report-card comments, message bodies or a Wellbeing entity gets `BFF_PERMISSION_DENIED` and no upstream call is made; the same path from outside is refused at the Gateway (`TC-GW-751`, Gateway sheet) |
| TC-BFF-751 | Integration | The source re-check under a teacher's token for 8 sources, 2 of them outside the teacher's sections, returns the 6 permitted ids within 800 ms; an owning service stopped mid-call drops its sources rather than failing the call |
| TC-BFF-752 | Integration | A tool call whose name maps to a `GET` list returns that list under the caller's token; an unknown tool name returns `BFF_NOT_FOUND`; a map entry pointing at a `POST` is refused with `BFF_PERMISSION_DENIED` before any upstream call |
| TC-BFF-753 | End to end | Natural-language search returns 202 with Ai's job; polling the job route returns the rows of the plan's list calls, each authorized by its owner under the caller's token, and a second read returns `AI_NOT_FOUND`; with Ai stopped or answering degraded, the same POST returns 200 keyword results with `degraded: true` (REQ-BFF-006) |
| TC-BFF-754 | Contract | With the process culture set to `ar-SA`, every composed payload example filled from the Arabic demo tenant stays under 32 KB after Brotli, and its dates, numbers and `asOf` values are ISO 8601 with Latin digits, byte-identical to the invariant-culture run; the client renders them in the tenant's numerals and calendar (REQ-PLAT-019, REQ-L10N-008) |

### Platform notes

| Concern | What holds here | Proof and runner |
|---|---|---|
| Runner | The host's composition, contract and security tests run on the Linux runner (`ubuntu-latest`) in `ci-service.yml`; Bff.Web is not among the projects document 33 part 4 runs on Windows (`BuildingBlocks`, `Documents`, `Localization`), and the Web building block it composes with is tested there. `dev-smoke` starts the host on `ubuntu-latest`, `windows-latest` and `macos-latest` | Document 33 part 4 |
| Culture-sensitive values | Every value this host returns is machine-facing: labels are keys, dates and numbers are ISO 8601 and invariant, so a host process running under any culture returns the same bytes, and the client renders them in the tenant's numerals, calendar and time zone from the bootstrap | `TC-BFF-754`, `TC-BFF-026` on the Linux runner |
| Right-to-left and Arabic payloads | The host composes no sentence and sets no direction; the web client mirrors (document 08). Arabic payloads are the larger ones, so the 32 KB budget is proven on Arabic examples, which the Spectral rule `nibras-example-arabic` of document 22 requires for every schema with bilingual text or a person's name | `TC-BFF-754`, `TC-BFF-011` |
| Arabic search | Search text is passed to the owners unchanged; each owner folds it with `nibras_ar_fold` (document 24), so this host never normalises Arabic itself and cannot disagree with the owner | `TC-BFF-022`; `TC-L10N-310` (document 24) |
| Devices without Google services | Not relevant to this host: it serves the web client only, and the mobile app talks to Bff.Mobile | Not applicable |

**Signature features.** This host owns Appendix W features 2 (Student 360 timeline filtered by the viewer's permissions) and 11 (command palette and natural-language search, natural language at rung 3 through the routes above), and composes the web side of 1 (Today dashboards), 25 (morning brief per role, on the principal home), 31 (guardian transparency panel) and 33 (campus digital twin, Operations' feature, through `campus/digital-twin`). Each is cited by its Appendix W number; its moment, rung, autonomy, the requirements and slices that build it, its Appendix O step and its demo test are held once, in the "Signature feature trace" table of `32-product-differentiation-and-demo.md`, and are not copied here.

---

## 15. Scaling, partitioning and risks

| Concern | Design | Trigger to revisit |
|---|---|---|
| Replicas | Stateless; 2 replicas as this sheet's starting point, HPA on requests per second and p95 composition time (document 21 §5 carries no BFF row) | p95 home above 200 ms at the N-05 load |
| Partitioning | None; no data | not applicable |
| Upstream fan-out | At most one call per card, parallel, deadline per class, bulkhead per upstream | A home needing more than 10 upstream calls, which means Reporting lacks a projection |
| Cache | Per-user entries with short lifetimes; hit ratio monitored per screen | Hit ratio under 95 percent at 07:45 in any band |

Likelihood (L) and impact (I) use the 1 to 5 scales of `18-risk-register.md` Section 1; Score is L times I, and a row scoring 12 or more names the register risk that covers it.

| Risk | L | I | Score | Mitigation | Owner | In the register |
|---|---|---|---|---|---|---|
| A composed screen shows a field the viewer may not see | 3 | 4 | 12 | Token forwarding, no own permissions, redaction by upstream answer, TC-SEC-331, TC-BFF-019 | Security owner | RISK-20 |
| An internal Ai route widens what Ai may read: the feed answers an entity its credential does not name, or a tool reaches a write | 2 | 5 | 10 | The Gateway never routes `/bff/web/v1/internal/`, a network policy admits only the Ai workloads, the feed checks the entity against the service token, tools map only to `GET`; TC-BFF-750, TC-BFF-752, `TC-GW-751` | Security owner | RISK-38 |
| A business rule creeps into a composer | 3 | 3 | 9 | Architecture test TC-BFF-001, review checklist: composers only fan out, filter and shape | Architect | none |
| One slow service makes every home slow | 2 | 4 | 8 | Deadlines, breaker, bulkhead, partial regions, TC-BFF-008 | Performance owner | none |
| Stale home after an action | 3 | 2 | 6 | 15 s L1, broadcast eviction, read-your-writes header | Web lead | RISK-13 |
| Contract drift between BFF and a service | 3 | 3 | 9 | Twenty consumer pacts gate both sides, TC-BFF-012 | Tech lead | none |

---

## Decisions in force

| Decision | Source | Default if unanswered | Impact if wrong |
|---|---|---|---|
| Route prefix is `/bff/web/v1/` | `22-api-conventions-and-error-catalog.md` §1.1 and Spectral rule `nibras-path-version-prefix`; documents 08 and 12 use the same prefix | As stated | The generated web client and the Gateway route table would disagree; the Spectral rule fails such a build |
| Single-service screens call their service through the Gateway; Bff.Web serves only compositions | `08-web-structure.md` part 4 | As stated | Routing every screen through the BFF would couple web releases to it and double the hop count |
| Reporting read models are read through the Reporting API, not a database connection | Appendix L (no database), reference architecture Section 8 table 8.0 ("reads ... the Reporting read models"); `10-data-architecture.md` §7.4, which now says the same | As stated | A direct connection would make the BFF a reader of another service's database |
| Bff.Web is the web client's only road to Ai: model calls through the `/bff/web/v1/ai/{path}` pass-through and the natural-language routes, each answering 202 with Ai's assist job; and Ai's jobs read through the three internal routes | Reference architecture Section 8.0 ("Ai over REST, through Bff.Web"); `05-service-catalog.md`; `06-services/ai.md` §5 and §6 | As stated | Without the pass-through the drafting panel and the AI settings screens have no path, because the Gateway never routes `/api/v1/ai/`; without the internal routes Ai cannot index or re-check a source |
| Cache eviction arrives through the `state:invalidate` broadcast, not a queue | `11-messaging-architecture.md` part 1, `21-performance-engineering.md` §2.3 and §2.6 | As stated | A queue would make the BFF a consumer with an inbox and an exchange binding it does not own |
| Every composed payload is cached per user | Appendix J level Confidential; `21-performance-engineering.md` §1 conventions | As stated | A shared payload would leak one role's view to another |

## Dependencies on other documents

| This document assumes | Stated in | Checked on |
|---|---|---|
| Routes, screens and the realtime refresh flow | `08-web-structure.md` parts 2, 3, 4, 5, 7 | Group D review |
| Upstream resources of Attendance and Assessment | `06-services/attendance.md`, `06-services/assessment.md` | Group C review |
| Upstream resources of the other eighteen services | their sheets in `06-services/`, pinned by the consumer pacts | Group C review |
| Cache entries, payload and composition budgets | `21-performance-engineering.md` §1.21, §2, §10; Appendix N N-05 | Group C review |
| Threat row and child-safety control | `12-security-privacy-safety.md` §2.21 and §7 | Group D review |
| Projection catalog and lag awareness | `10-data-architecture.md` §7 | Group C review |
| Card list per role | Appendix D, Appendix I | every lint run |
| Ai's routes, jobs, source streams and the three internal routes it calls | `06-services/ai.md` §5, §6, §9; `25-ai-and-assist-ladder.md` §4.2, §4.4; reference architecture Section 8.0 | Group C review |
| The Gateway never routes `/bff/web/v1/internal/` | `06-services/gateway.md` §5.1 | `TC-GW-751` |

## Open points

| Question | Default | Owner | Impact if the default is wrong | L | I | Score | In the register |
|---|---|---|---|---|---|---|---|
| 1. `08-web-structure.md` §3.2 and §5 and `12-security-privacy-safety.md` §3.4 named `/api/v1/bff-web/...` where document 22 and its Spectral rule `nibras-path-version-prefix` require `/bff/web/v1/` | `/bff/web/v1/` everywhere | Web lead | Closed 2026-09-26: documents 08 and 12 now use the `/bff/web/v1/` prefix (both call `GET /bff/web/v1/me/permissions`), as this sheet and document 22 do | 1 | 1 | 1 | none |
| 2. `10-data-architecture.md` §7.4 had both backends-for-frontends reading the Reporting replica through a connection string | Read through the Reporting API; the replica routing and the `Nibras-Expect-Message` check run inside Reporting | Data architect | Closed 2026-09-26: document 10 §7.4 now says the Reporting API alone holds `Reporting:ReadReplica` and the BFF forwards `Nibras-Expect-Message` to it | 1 | 1 | 1 | none |
| 3. Saved views, table columns, density and pinned actions "persist per person through Bff.Web" (`08-web-structure.md` §3.1), but Bff.Web stores nothing and no Appendix B permission covers user preferences | Stored as user preferences in Identity (which owns `User`), forwarded unchanged, self-scoped. ADR-0019 did not add an `identity.me.*` resource to Appendix B, so the two preference routes keep declaring the `self` scope and no permission, as the Identity sheet's Decisions in force say | Identity lead | If Identity declines, the preferences stay browser-local and do not follow the person across devices | 2 | 1 | 2 | none |
| 4. `21-performance-engineering.md` §1.21 lists routing keys as the invalidators of the BFF entries; the BFF consumes no event | The publisher of each key broadcasts the tag on `state:invalidate` after commit, and each BFF instance evicts its own entries under its own ACL user | Performance owner | Closed 2026-09-26: document 21 §1.21 now names the broadcaster per key (Requests, Notification, Attendance, Identity, Platform) and says the BFF evicts under `svc_bff_web`; without a broadcast an entry still lives only 60 seconds | 1 | 2 | 2 | RISK-13 |

## Review record

| Date | Reviewer | Outcome |
|---|---|---|
| 2026-09-21 | drafted | awaiting Group C review |
| 2026-09-26 | Round-5 scorecard, remediation round 6 | Checked against documents 08 and 12: both use `/bff/web/v1/` (`GET /bff/web/v1/me/bootstrap` and `/me/permissions`), so the Decisions in force row names them as agreeing and open point 1 is closed; neither document 08 nor 12 still writes `/api/v1/bff-web/`, which survives only in open point 1's history and in scorecard quotations. Earlier remediation rounds changed this sheet without adding a row; this row records it. Awaiting Group C re-review |

## How this document is verified

| Claim | Proof | Where it runs |
|---|---|---|
| Owns no data, no event, no rule | TC-TST-114 and TC-BFF-001 architecture tests; no `Persistence/` or `Messaging/` folder in the host | `tests/Architecture.Tests`, lint of the tree at review |
| Every gate permission exists in Appendix B | `/lint-plan` permission cross-check; TC-BFF-015 once code exists | Lint, pipeline |
| Every route appears in the OpenAPI document with `bearerAuth` only and the `/bff/web/v1/` prefix | Spectral rules `nibras-path-version-prefix` and `nibras-security-declared` | `ci-service.yml` for the host |
| Composition budgets hold | TC-BFF-010 in N-05 and `nibras_bff_compose_duration_seconds` alerts | Load tier, production dashboards |
| No field leaks | TC-SEC-331, TC-BFF-019 | Security suite |
| The three internal Ai routes read only what their credential allows and are unreachable from outside | TC-BFF-750, TC-BFF-751, TC-BFF-752; `TC-GW-751` (Gateway sheet) | Integration suite on `ubuntu-latest`, every pull request touching the host or Ai |
| Natural-language search answers with Ai's 202 job and degrades to keyword search | TC-BFF-753 | End-to-end suite, once SL-BFF-004 and CAP-AI-01 exist |
| Payloads do not depend on the host culture | TC-BFF-754 | Contract suite on `ubuntu-latest` |
| Every tree entry has a purpose comment | `tools/kit-lint` rule R18 | Lint |
