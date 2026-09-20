# Appendix L. Canonical Registry and ID Codes

> Part of `docs/brief/02-appendices/`. Normative. Read with the master brief and the reference architecture.

This appendix is the **single source of truth for names and identifiers**. Every other document quotes it; none redefines it. When a name here and a name elsewhere disagree, this appendix wins and the other document is a defect. `tools/kit-lint` enforces the parts it can check automatically.

---

## L.1 Service registry

Twenty services own data. Gateway and the two backends-for-frontends own none. Seven worker hosts run inside their owning service and deploy as separate images.

| Service | AREA | Long name used in prose | Tier | Database | Exchange | Images |
|---|---|---|---|---|---|---|
| **Identity** | IDN | Identity and Access | 1 | `nibras_identity` | `nibras.identity` | `nibras/identity-api` |
| **Platform** | PLT | Tenant, Platform, and Integrations | 1 | `nibras_platform` | `nibras.platform` | `nibras/platform-api` |
| **School** | SCH | School Core (SIS) | 1 | `nibras_school` | `nibras.school` | `nibras/school-api` |
| **Admissions** | ADM | Admissions | 1 | `nibras_admissions` | `nibras.admissions` | `nibras/admissions-api` |
| **Academics** | ACA | Academics and Coursework | 1 | `nibras_academics` | `nibras.academics` | `nibras/academics-api` |
| **Assessment** | ASM | Assessment and Reporting | 1 | `nibras_assessment` | `nibras.assessment` | `nibras/assessment-api`, `-worker` |
| **Scheduling** | SCD | Scheduling and Calendar | 1 | `nibras_scheduling` | `nibras.scheduling` | `nibras/scheduling-api`, `-worker` |
| **Attendance** | ATT | Attendance and Safety | 1 | `nibras_attendance` | `nibras.attendance` | `nibras/attendance-api` |
| **Finance** | FIN | Finance | 1 | `nibras_finance` | `nibras.finance` | `nibras/finance-api`, `-worker` |
| **Communication** | COM | Communication | 1 | `nibras_communication` | `nibras.communication` | `nibras/communication-api` |
| **Notification** | NOT | Notification | 1 | `nibras_notification` | `nibras.notification` | `nibras/notification-api`, `-worker` |
| **Requests** | RQS | Requests, Tasks, and Workflow | 1 | `nibras_requests` | `nibras.requests` | `nibras/requests-api` |
| **Documents** | DOC | Documents | 1 | `nibras_documents` | `nibras.documents` | `nibras/documents-api`, `-worker` |
| **Behavior** | BEH | Behavior and Recognition | 1 | `nibras_behavior` | `nibras.behavior` | `nibras/behavior-api` |
| **Reporting** | RPT | Reporting and Analytics | 1 | `nibras_reporting` | `nibras.reporting` | `nibras/reporting-api`, `-projections` |
| **Audit** | AUD | Audit | 1 | `nibras_audit` | `nibras.audit` | `nibras/audit-api` |
| **Wellbeing** | WEL | Student Wellbeing | 2 | `nibras_wellbeing` | `nibras.wellbeing` | `nibras/wellbeing-api` |
| **Hr** | HR | Human Resources | 2 | `nibras_hr` | `nibras.hr` | `nibras/hr-api` |
| **Operations** | OPS | Operations | 2 | `nibras_operations` | `nibras.operations` | `nibras/operations-api` |
| **Ai** | AI | AI Assist | 2 | `nibras_ai` | `nibras.ai` | `nibras/ai-api`, `-worker` |
| **Gateway** | GW | Gateway | 1 | none | none | `nibras/gateway` |
| **Bff.Web** | BFF | Web backend-for-frontend | 1 | none | none | `nibras/bff-web` |
| **Bff.Mobile** | BFF | Mobile backend-for-frontend | 1 | none | none | `nibras/bff-mobile` |

**Counts, stated once.** 20 data-owning services; 16 Tier 1 and 4 Tier 2. With Gateway and the two backends-for-frontends, 23 deployable applications, plus 7 worker images. This replaces the "about 12 for Tier 1, 16 to 18 at full scope" estimate in earlier versions, which never matched the catalog.

**Worker hosts.** `Notification.Worker` (channel lanes and digests), `Documents.Worker` (PDF, imports, exports, virus scan, OCR), `Scheduling.Worker` (OR-Tools solver), `Assessment.Worker` (result calculation and report-card batches), `Finance.Worker` (invoice runs, reminder ladder, statements), `Reporting.Projections`, `Ai.Worker`. There is no standalone `Imports.Worker`: importing is a job group inside `Documents.Worker`.

**First-release merge option (master brief Decision 8).** A smaller first release may merge **Assessment into Academics** and **Behavior into Wellbeing**, and defer **Hr**, **Operations**, and **Ai**. That release runs 14 services. Merging is a build-time choice only: the permission namespaces, routing keys, and database names in this appendix do not change, so splitting later is a deployment change and not a rewrite. Merging requires an ADR that records the date the split is expected.

**Naming rules that follow from this table.** Projects `Nibras.<Service>.<Layer>`. Building blocks `Nibras.BuildingBlocks.<Name>`. Contracts `Nibras.Contracts.<Service>`. Database user `svc_<service>`. Kubernetes namespace `nibras-<env>`. Web packages `@nibras/<lib>`. Metrics prefix `nibras_`, log field `nibras.tenant_id`. Full table: reference architecture Section 7.

---

## L.2 Identifier schemes

| Identifier | Format | Example | Lives in |
|---|---|---|---|
| Requirement | `REQ-<AREA>-<NNN>` | `REQ-ATT-014` | `docs/plan/03-requirements-catalog.md` |
| Workflow | `WF-<AREA>-<NN>` | `WF-ATT-03` | Appendix R |
| Business rule | `BR-<AREA>-<NNN>` | `BR-FIN-007` | Appendix S |
| Test case | `TC-<AREA>-<NNN>` | `TC-ATT-001` | Appendix Q and the service test plans |
| Error code | `<SERVICE>_<MEANING>` | `ATTENDANCE_SESSION_LOCKED` | Appendix K |
| Permission | `<service>.<resource>.<action>` | `attendance.student-attendance.mark` | Appendix B |
| Routing key | `<service>.<entity>.<event>.v<n>` | `attendance.student.absent.v1` | Appendix E |
| Cache key | `nibras:{tenant}:{service}:{entity}:{id}:v{n}` | `nibras:018f…:scheduling:timetable:sec-4b:v2` | Master brief Section 19 |
| ADR | `NNNN-kebab-title.md` | `0009-platform-owns-settings.md` | `docs/project/DECISIONS/` |

`{tenant}` in a cache key is the **full tenant UUID v7**, never a shortened hash. A truncated tenant id collides, and a cache collision across tenants is the one failure this product cannot have.

The Requests service uses AREA code **RQS**, not REQ, so that a requirement about the Requests service reads `REQ-RQS-001` and never `REQ-REQ-001`.

---

## L.3 Cross-cutting AREA codes

Requirements that belong to no single service still need an area.

| AREA | Covers | Owning plan document |
|---|---|---|
| `SEC` | Security controls, threat model, authorization enforcement | `12-security-privacy-safety.md` |
| `PRV` | Privacy, consent, retention, data subject rights | `12-security-privacy-safety.md`, `27-compliance-and-legal.md` |
| `PERF` | Performance budgets, caching, query budgets | `21-performance-engineering.md` |
| `L10N` | Localization, RTL, calendars, numerals, terminology | `24-localization-and-calendars.md` |
| `UX` | Design system, motion, accessibility, states | `14-design-system-and-ux.md` |
| `WEB` | Angular workspace and screens | `08-web-structure.md` |
| `MOB` | Flutter application, offline, device modes | `09-mobile-structure.md` |
| `API` | REST and gRPC conventions, error catalog, versioning | `22-api-conventions-and-error-catalog.md` |
| `INT` | Public API, webhooks, standards, plug-ins | `23-integrations-and-public-api.md` |
| `DATA` | Data architecture, tenancy, partitioning, retention | `10-data-architecture.md` |
| `MSG` | Messaging topology, contracts, delivery guarantees | `11-messaging-architecture.md` |
| `INF` | Deployment, environments, backup, disaster recovery | `15-deployment-and-operations.md` |
| `TST` | Test strategy, coverage matrix, quality gates | `16-test-strategy.md` |
| `PLAT` | Cross-platform support: operating systems, browsers, devices | `33-platform-support-and-dev-environments.md` |

---

## L.4 Permission namespaces

Namespaces equal service names in lower case. Three namespaces in earlier versions were not services and are corrected here; anything still using the old form is a defect.

| Old namespace | Correct namespace | Reason |
|---|---|---|
| `students.*` | `school.students.*` | Students are owned by the School service |
| `safety.*` | `attendance.safety.*` | Safety lives inside the Attendance service |
| `admin.*` | `platform.jobs.*`, `platform.failed-messages.*`, `platform.recycle-bin.*`, `platform.retention.*`, `audit.*` | Administration is split across the services that own each concern |

---

## L.5 Ownership decisions that earlier versions left ambiguous

| Concern | Owner | Note |
|---|---|---|
| Tenant settings, terminology, custom-field **definitions** | **Platform** | Every service owns the custom-field **values** stored on its own entities. ADR-0009 |
| Tasks and the unified inbox | **Requests** owns the `Task` aggregate; **Notification** owns inbox delivery | ADR-0012 |
| Public API keys, outgoing webhooks, OneRoster, LTI, developer portal | **Platform**, as the Integrations capability | ADR-0012 |
| iCal feeds | **Scheduling** publishes them; Platform exposes the subscription URL | |
| Open Badges | **Behavior** | |
| QTI import and export | **Academics** | |
| Cafeteria, boarding | **Operations**, as separate schemas | Tier 3 |
| Alumni | **School**, as a student status plus `AlumniProfile` | Tier 3 |
| Live vehicle tracking | **Operations**, `transport` schema | Tier 3 |
| General ledger | A separate optional `Ledger` service, not in the v1 tree | Tier 3 |
| WhatsApp delivery | **Notification**, as a channel adapter | Tier 3 |

---

## L.6 A school group

A **school group is one tenant with several campuses**. Group-level reporting is campus aggregation inside that tenant, and it needs no cross-tenant query. An owner who insists on separate tenants per school gets consolidated reporting through the Reporting service's group view, which is Tier 3 and explicitly opt-in per tenant. ADR-0011.

This settles the two framings that earlier versions used interchangeably and is the reason the tenancy model never needs a cross-tenant join.
