# Nibras (نبراس): Master Brief v9.2

**The premium open-source school management platform**

**Multi-tenant SaaS · Microservices · Angular · .NET · Flutter · RabbitMQ · open-source stack, two documented exceptions**

> **Document set** (all under `docs/brief/`, all normative):
> 1. `01-master-brief.md` (this file): vision, rules, architecture principles, and method.
> 2. `02-appendices/`: one file per appendix. A to H carry the feature, permission, notification, dashboard, event, entity, settings and demo catalogs. I to X add role templates, data classification, error codes, **the canonical registry (Appendix L)**, offline conflict rules, load scenarios, the demo script, the differentiation matrix, UAT scripts, **the workflow catalog (Appendix R)**, **the business rules catalog (Appendix S)**, the year-in-the-life simulation, persona journeys, the coverage matrix, the feature register, and the platform support matrix.
> 3. `03-reference-architecture.md`: the baseline repository structure, the anatomy of a service, the web and mobile structures, naming conventions, and a specification sheet for every service.
>
> **How this is used.** The repository root contains `CLAUDE.md` and `.claude/commands/`. Planning starts with the `/plan-platform` command, which produces the plan under `docs/plan/` as specified in `docs/plan/PLAN_SPEC.md`. No implementation code is written before that plan is approved. Confirm Section 27 (*Decisions to Confirm*) first.

---

## 1. Document Conventions and Glossary

**Requirement keywords.** MUST and MUST NOT are mandatory. SHOULD is expected unless you give a written reason. MAY is optional. Anything written as a plain statement of what the system does is a MUST.

**Priority.** Tier 1 (core, build first and finish completely), Tier 2 (extended), Tier 3 (optional: design for it, build last).

**Precedence.** My explicit instructions in the conversation, then this master brief, then the appendices, then your assumptions. When two sources conflict, stop and ask.

**Requirement IDs.** In Phase 0, assign every requirement an ID of the form `REQ-<AREA>-<NNN>` (for example `REQ-ATT-014`) and use those IDs in the traceability matrix, commits, tests, and progress reports.

**Glossary**

| Term | Meaning |
|---|---|
| Tenant | One customer organization: a school or a school group. The isolation boundary. |
| Campus | A physical branch of a tenant. A tenant has one or more. |
| Academic year, term, grading period | The year, its divisions, and the windows in which grades are collected and reported |
| Stage, grade level, section | For example: Primary → Grade 4 → Section 6B. Terminology is configurable per tenant. |
| Homeroom | The section a student belongs to for attendance, pastoral care, and the report card |
| Guardian | An adult linked to a student with defined legal and access rights. A **payer** is whoever is billed, and need not be a guardian. |
| Role, permission, data scope | A role is a named bundle of permissions. A data scope limits which records the permissions apply to. |
| Workspace | The role-specific home, navigation, and tools of a user |
| Request type | A configurable template in the Request Center: form, approval chain, SLA, and effect |
| Service, BFF, read model | A microservice; a backend-for-frontend aggregation layer; a query-optimized copy of data built from events |
| Saga | A persisted multi-step process across services, with compensation on failure |
| Building block | A shared technical library. It never contains business logic. |

---

## 2. Your Role

Act as a combined **principal software architect, senior product owner, senior UX designer, and EdTech domain expert** who has seen how real schools operate day to day. You own the completeness and quality of the product, not only the code. If this brief misses something a real school needs, you must find it and raise it.

## 3. Project Parameters

| Parameter | Value |
|---|---|
| Product name | **Nibras** (Arabic: **نبراس**, "a guiding light"). See the brand rules in Section 16.0. |
| Business model | Multi-tenant SaaS sold to schools; also deployable on-premises for a single school or school group |
| School types | K-12, private and public, single and multi-campus, 100 to 10,000+ students |
| Languages | English and Arabic, full RTL, runtime switching, bilingual data entry for names and official documents |
| Calendars and week | Gregorian with optional Hijri display; configurable work week (for example Sunday to Thursday) |
| Backend | ASP.NET Core Web API on **.NET 10 (LTS)**. See the note below. |
| Web | Angular, the latest stable major at project start, pinned in Phase 0; zoneless, signals, standalone components. Verify that the pinned version ships `animate.enter`, `animate.leave` and the View Transitions integration; if it does not, drive the same motion with CSS classes toggled by signals |
| Mobile | Flutter, one codebase for Android and iOS |
| Database and data access | PostgreSQL with **EF Core 10**, tuned under the performance rules in Section 19 |
| Cache | **Redis** through `HybridCache`. See Section 19, *Caching with Redis* |
| Architecture | **Microservices**, database per service, API gateway, event-driven. See Section 7. |
| Messaging | RabbitMQ (mandatory) |
| Administration | Full admin console with user, role, permission, and join management. See Section 10. |
| Default administrator | Username `admin`, initial password `@Like12345`, with the safeguards in Section 10.2 |
| Licensing rule | **Free and open source only.** See Section 6. |
| Deployment | One Docker image per service; Aspire AppHost for local development; Docker Compose single-server mode for small installs; Kubernetes with Helm and KEDA for scale |

> **Note on the .NET version.** The original requirement was .NET 8. Microsoft ends support for .NET 8 on **10 November 2026**, after which it receives no security patches. A new product that stores children's data should not start on a runtime that is about to lose security updates. Target .NET 10 (LTS, supported until November 2028). If .NET 8 is contractually required, use it, and apply the fallback list in Section 19 (*Runtime fallbacks if .NET 8 is chosen*), which names every rule in this brief that depends on .NET 10 and what replaces it. Keep the code free of anything that blocks a one-step retarget.

## 4. Objective and Product Vision

Build a complete, integrated, intelligent school management platform that covers the entire school lifecycle, feels premium from the first screen, and is solid enough for real production use.

Three tests define success:

1. **The demo test.** A school owner watching a 15-minute demo immediately understands the value and sees things competitors do not have.
2. **The Monday-morning test.** A teacher with 30 students and five minutes can do what they need on a phone, without training.
3. **The audit test.** Any sensitive action can be traced, any number on a report can be explained, and no school can ever see another school's data.

**Measurable outcomes the product is designed for:**

| Outcome | Target |
|---|---|
| A teacher marks a class's attendance on a phone | Under 60 seconds |
| A new school goes live with imported data | Within one working day |
| Report card cycle, from marks locked to published | Hours, not days |
| Parents active in the app each week | 70% or more |
| Requests completed within their SLA | 90% or more |
| Fees paid on time after automated reminders | Measurable improvement, reported per school |
| Cross-tenant data incidents | Zero. This is a control objective, not a trend: the tenant isolation suite attacks every endpoint and every consumer on every build, and a single failure blocks the release |

This is a **production-ready platform, not a CRUD application.** Every workflow must be designed end to end, connected to related workflows, and consistent across web, mobile, and API.

---

## 4.1 North Star and Product Principles

**North star.** *Every person in the school knows what matters today, and can act on it in one tap, in their language, even offline.*

Every screen, every notification, and every automation is judged against that sentence. A feature that adds information without adding an action fails it. A feature that works only online fails it for the teacher on a corridor with no signal. A feature that works only in English fails it for most of the people who will use it.

**Six principles, in priority order when they conflict.**

1. **Calm.** One screen answers one question and offers one action. Silence is a designed state: "nothing needs your attention" is a legitimate and valuable answer. Notifications earn their interruption.
2. **Fast.** The budgets in Section 19 are product requirements, not engineering preferences. Attendance for a class takes under a minute on a phone, including the network.
3. **Trustworthy.** Any number can be explained down to the records and the rule version that produced it. Any access to a sensitive record is logged and visible to the people it concerns.
4. **Bilingual by nature.** English and Arabic are equal. Right-to-left is a layout direction, not a translation layer. Names, documents, and reports are correct in both.
5. **Assistive intelligence.** Automation proposes; a human decides anything that touches a child's record, a grade, or a message to a family. Every automated decision shows its reasons. See Section 25.
6. **Open.** Data leaves freely through exports and a public API. Integrations arrive through documented plug-ins. A school is never locked in, and says so in the contract.

**The three tests from Section 4 are the release scorecard.** Before any release, the golden-path demo must pass the demo test, a real teacher task must pass the Monday-morning test, and the audit test must be provable from the audit log. A release that fails one of them is not a release.

---

## 5. Scope, Assumptions, Constraints, and Personas

**In scope:** everything in this brief and in the appendices, for K-12 schools including nursery and kindergarten.

**Explicitly out of scope** (design so that they can be added or integrated later, but do not build them):

- A full general-ledger accounting system. Provide clean accounting exports and an integration interface instead.
- Country-specific payroll tax calculation. Provide payroll inputs, payslip generation from configurable components, and a pluggable calculation interface.
- A SCORM-level course authoring tool. Provide resources, assignments, quizzes, and LTI 1.3 integration with external learning tools.
- Self-hosted video infrastructure beyond embedding Jitsi or LiveKit.
- Hardware firmware. Provide documented APIs and an adapter interface for biometric, RFID, and turnstile devices.
- Higher-education features (credit hours, course registration). Do not make design choices that would block them.

**Assumptions:** schools have unreliable connectivity at times; many parents use low-cost Android phones; many users prefer Arabic; administrators are not technical; data is imported from Excel or a legacy system on day one.

**Constraints:** open-source-only (Section 6); RabbitMQ; the mandated stack; bilingual from the first screen; the data subjects are children, so privacy and safety rules (Section 20) override convenience.

**Personas and what success feels like**

| Persona | Primary job | Success moment |
|---|---|---|
| School owner or director | Know whether the school is healthy | Opens one screen and sees enrollment, collections, attendance, and risks across campuses |
| Principal | Run the day and unblock people | Clears all approvals in five minutes on a phone |
| Academic coordinator | Keep teaching and assessment on track | Sees syllabus coverage and late grading before it becomes a problem |
| Teacher | Teach, not administrate | Attendance in a minute, grading in a fast grid, comments drafted for review |
| Homeroom teacher | Know every child | One timeline per student, and an alert when a pattern changes |
| Registrar | Accurate records, smooth admissions | An application moves from inquiry to enrolled without retyping anything |
| Accountant | Collect and reconcile | Invoices run in bulk, reminders go out alone, the day closes balanced |
| Parent | Know how my child is doing and what I must do | One calm digest, one tap to pay, one tap to request |
| Student | Know what is due and how I am doing | Today's timetable, what is due, and feedback in one place |
| Nurse, counselor | Care with confidentiality | Records only the right people can see, and alerts that reach the right staff |
| Platform operator | Grow and support many schools | Provisions a school in minutes and sees trouble before the school calls |

---

## 6. Open-Source-Only Policy

### 6.1 License rules

- **Allowed for code linked into the product:** MIT, Apache-2.0, BSD, ISC, MPL-2.0, PostgreSQL License. LGPL is allowed only for dynamically referenced libraries.
- **Allowed only as standalone infrastructure, run unmodified and never linked into product code:** GPL and AGPL tools (for example Grafana). List each one and its justification.
- **Not allowed:** commercial licenses, "community" licenses gated by revenue or company size, source-available licenses (BSL, SSPL, RSAL, Elastic License), free tiers of paid products, trial versions.
- **Verify, do not assume.** Several well-known .NET libraries changed to commercial licenses recently. Before adding any dependency, check the license of the **exact version** being installed. Pin versions.
- **Enforce in CI.** Add an automated license scan for NuGet, npm, and pub packages that fails the build on a disallowed license. Maintain a `THIRD-PARTY-LICENSES.md` file.
- **Isolate every external dependency behind an interface** so that a future license change means replacing one adapter, not rewriting the product.

### 6.2 Approved stack

| Concern | Use | License | Do **not** use |
|---|---|---|---|
| Database | PostgreSQL, with `pgvector`, `pg_trgm`, full-text search | PostgreSQL | SQL Server, Oracle |
| ORM | **EF Core 10 with Npgsql** as the data access layer of every service, under the performance rules of Section 19; Dapper only for reporting read models | MIT / PostgreSQL | |
| Bulk operations | `Npgsql` binary COPY, EF Core `ExecuteUpdate/ExecuteDelete` | | EF Core Extensions (Z.EntityFramework), paid |
| Message broker | **RabbitMQ** | MPL-2.0 | |
| Mediator, message bus, outbox, sagas | **Wolverine** (in-process mediator and RabbitMQ transport in one library). Fallback: Rebus, or `RabbitMQ.Client` with a thin in-house bus | MIT | **MassTransit v9+**, **MediatR** (current versions), **NServiceBus**: commercial |
| Object mapping | Mapperly (source generator) or explicit manual mapping | Apache-2.0 | **AutoMapper** (current versions): commercial |
| Validation | FluentValidation | Apache-2.0 | |
| Authentication | ASP.NET Core Identity + **OpenIddict** (OAuth 2.0 / OIDC). Alternative for SSO-heavy customers: Keycloak | Apache-2.0 | **Duende IdentityServer**, Auth0, Okta |
| Cache, SignalR backplane, rate limiting, idempotency, short locks | **Redis 8+** run unmodified as a standalone server under its AGPLv3 option, with the StackExchange.Redis client and **`HybridCache`** (L1 memory + L2 Redis). Valkey is the drop-in fallback. See Section 19, *Caching with Redis* | AGPLv3 standalone / MIT | Redis Enterprise, paid Redis modules, modifying or embedding Redis, managed Redis services that are not free |
| File storage | S3-compatible API via an `IFileStorage` abstraction; **SeaweedFS** as default, local disk for small installs | Apache-2.0 | **MinIO** (the community edition was feature-reduced and repositioned in 2025; treat it as unavailable rather than as a fallback) |
| Scheduled jobs | Quartz.NET (clustered, PostgreSQL job store) | Apache-2.0 | Hangfire Pro |
| Real-time | ASP.NET Core SignalR | MIT | |
| PDF (report cards, certificates, invoices) | **Gotenberg** (Chromium HTML to PDF as a container; correct Arabic shaping and RTL). PDFsharp/MigraDoc for simple documents | MIT | **QuestPDF** (revenue-gated), **iText** (AGPL), IronPDF, Aspose, Syncfusion |
| Excel import and export | ClosedXML; CsvHelper for CSV | MIT / Apache-2.0 | **EPPlus** (commercial), Aspose, GemBox |
| Image processing | SkiaSharp or Magick.NET | MIT / Apache-2.0 | **ImageSharp v3 and later** (Six Labors Split License). Version 2.x is Apache-2.0 but no longer maintained, so it is not a way around the policy |
| QR and barcodes | QRCoder, ZXing.Net | MIT / Apache-2.0 | |
| Multi-tenancy | Finbuckle.MultiTenant, EF Core global query filters, PostgreSQL row-level security as a second barrier | Apache-2.0 | |
| Resilience | Microsoft.Extensions.Resilience | MIT | |
| Feature flags | Microsoft.FeatureManagement | MIT | LaunchDarkly |
| API documentation | Microsoft.AspNetCore.OpenApi + Scalar UI | MIT | |
| API gateway | **YARP** (routing, token validation, rate limiting, tenant resolution) | MIT | Kong Enterprise, Apigee, Azure API Management |
| Edge proxy and TLS | Caddy or Traefik with Let's Encrypt | Apache-2.0 / MIT | |
| Internal synchronous calls | gRPC (`Grpc.AspNetCore`) with Microsoft.Extensions.ServiceDiscovery | Apache-2.0 / MIT | |
| Local orchestration and dev dashboard | **Aspire** (AppHost, service defaults, dashboard) | MIT | |
| Autoscaling | Kubernetes HPA plus **KEDA** (scale workers on RabbitMQ queue depth) | Apache-2.0 | |
| Packaging | Helm charts, one per service plus an umbrella chart | Apache-2.0 | |
| Infrastructure as code and GitOps | **OpenTofu**, Ansible, Argo CD or Flux | MPL-2.0 / GPL tool / Apache-2.0 | Terraform (BSL license) |
| Supply chain | CycloneDX SBOM, cosign image signing, Renovate for updates | Apache-2.0 / AGPL tool | |
| Accessibility and visual tests | axe-core, Playwright snapshots | MPL-2.0 / Apache-2.0 | |
| Mutation testing | Stryker.NET | Apache-2.0 | |
| Error tracking | GlitchTip (self-hosted, Sentry-compatible) | MIT | Sentry SaaS, Firebase Crashlytics |
| Product analytics (staff usage only) | Self-hosted Matomo or PostHog; verify the license of the edition used | | Google Analytics, Mixpanel |
| Connection pooling | PgBouncer | ISC | |
| Contract testing | PactNet | MIT | |
| Service mesh | None at the start. Add Istio or Cilium only if mutual TLS or traffic policy is proven necessary | Apache-2.0 | Linkerd stable builds (paid for larger companies) |
| Email | MailKit; Mailpit for development | MIT | |
| Search | PostgreSQL full-text search with Arabic normalization; Meilisearch only if it proves necessary | | Elasticsearch (license), Algolia |
| Timetable solver | **Google OR-Tools** (CP-SAT, official .NET bindings) | Apache-2.0 | |
| Predictive models | ML.NET (explainable classical models) | MIT | |
| Generative AI | Microsoft.Extensions.AI abstraction; **Ollama** or vLLM serving open-weight models locally; Semantic Kernel if orchestration is needed; `pgvector` for embeddings | MIT / Apache-2.0 | Hard dependency on any paid AI API. A paid provider may exist only as an optional adapter. |
| OCR for uploaded documents | Tesseract (Arabic and English) | Apache-2.0 | |
| Online classes | Jitsi Meet (embed) or LiveKit | Apache-2.0 | Zoom SDK |
| Maps and transport | OpenStreetMap, Leaflet (web), `flutter_map` (mobile) | BSD / ODbL | Google Maps Platform |
| Logging | Serilog to console/OpenTelemetry | Apache-2.0 | **Seq** (paid beyond a single user) |
| Observability | OpenTelemetry, Prometheus, Grafana, Loki, Tempo or Jaeger (standalone tools) | Apache-2.0 / AGPL tools | Datadog, New Relic, Application Insights |
| Secrets | OpenBao, or Kubernetes/Docker secrets | MPL-2.0 | HashiCorp Vault (BSL) |
| Backups | pgBackRest with point-in-time recovery | MIT | |
| Optional BI | Apache Superset | Apache-2.0 | Power BI, Tableau |
| Containers | Docker Engine or Podman | Apache-2.0 | Docker Desktop (paid for larger companies) |
| CI/CD and security | GitHub Actions free tier or Forgejo/Woodpecker; Trivy, Gitleaks, OWASP ZAP, SonarQube Community | | |

**Web (Angular):** Angular Material and CDK (MIT) as the component base, **native CSS animations with `animate.enter` and `animate.leave` plus the View Transitions API** (the `@angular/animations` package is deprecated and scheduled for removal, so do not use it), Tailwind CSS for layout, NgRx SignalStore for state, Transloco for i18n, Apache ECharts via `ngx-echarts` for charts, Tiptap or Quill for rich text, typed API client generated from OpenAPI, Playwright for end-to-end tests. **Build the timetable grid in-house with CDK drag and drop.** Do not use: AG Grid Enterprise, Kendo UI, Syncfusion, DevExpress, Highcharts, FullCalendar Premium plugins (resource and timeline views are paid), paid PrimeNG templates.

**Mobile (Flutter):** Riverpod or `flutter_bloc`, `go_router`, Dio, **Drift** (SQLite) for offline storage, `flutter_secure_storage`, `local_auth` for biometrics, `fl_chart`, `mobile_scanner` for QR, `flutter_map`, `flutter_animate` and the built-in implicit and Hero animations for motion, `lottie` for illustrative animations, and the Rive runtime only if animation assets already exist (the runtime is MIT, but the Rive editor is a commercial hosted product, so prefer code-driven motion). Do not use Syncfusion Flutter widgets.

**Fonts, icons, illustrations:** Inter and IBM Plex Sans Arabic (SIL Open Font License), Lucide or Material Symbols icons (ISC / Apache-2.0), and original SVG illustrations or an openly licensed set. Verify the license of every asset, not only of code.

**Testing:** xUnit, NSubstitute, Shouldly or AwesomeAssertions, Bogus, Testcontainers (PostgreSQL, RabbitMQ, Redis), BenchmarkDotNet, NetArchTest for architecture rules, Playwright, k6 for load tests. Do not use FluentAssertions v8+ (commercial).

### 6.3 Unavoidable costs (be honest, do not hide them)

Open source removes license fees, not all costs. Design each of these as a pluggable adapter that can be switched off, and list them clearly in Phase 0:

- **Push notifications:** Firebase Cloud Messaging and Apple Push Notification service are free of charge but not open source. iOS offers no open alternative. This is the one accepted exception, isolated behind `IPushSender`.
- **App stores:** Apple Developer Program and Google Play have account fees.
- **SMS and WhatsApp:** always charged per message by carriers. Provide the adapter interface and an email/push fallback; SMS stays optional.
- **Online payments:** gateways charge per transaction. Provide `IPaymentGateway` with a manual/bank-transfer implementation by default.
- **Email delivery at scale, servers, domains, and AI hardware** (a local LLM needs a capable machine; AI features must degrade gracefully when it is absent).

### 6.4 Standalone AGPL and GPL tools, listed with their justification

Section 6.1 allows GPL and AGPL software **only as standalone infrastructure, run unmodified, never linked into product code**, and requires each one to be listed. This is that list. Nothing may be added to it without an ADR.

| Tool | Licence | Why it is allowed | How it is kept at arm's length |
|---|---|---|---|
| Redis 8 | AGPLv3 (its OSI-approved option) | Cache, backplane, rate limits, locks | Run as an unmodified server image. Only the MIT-licensed StackExchange.Redis client is linked. Valkey (BSD-3) is a drop-in fallback and the integration suite runs against both, so the choice is reversible by changing an image tag |
| Grafana | AGPLv3 | Dashboards | Separate container, no product code links to it, dashboards are JSON in this repository |
| Loki, Tempo | AGPLv3 | Log and trace storage | Separate containers, reached over OTLP and HTTP only |
| ClamAV | GPLv2 | Upload scanning | Separate container, reached over its socket protocol |
| Ansible | GPLv3 | On-premises provisioning | A build tool, never shipped inside a product image |
| Renovate | AGPLv3 | Dependency update pull requests | A CI tool, never shipped |
| k6 | AGPLv3 | Load, soak and performance-budget tests (Sections 21 and 24) | A standalone load-test binary run in CI and in test environments only; scenarios are scripts it executes, it is never linked into product code and never shipped inside a product image |

Forgejo (GPLv3 since version 9.0) and Matomo (GPLv3) appear in Section 6.2 as options. Each needs a row here, and an allow-list entry, only if it is chosen.

**Scanner exception process.** A licence scanner reads package metadata and will flag Redis and these tools regardless of how they are used. The allow-list lives in `tools/license-scan/allow.json`, each entry naming the tool, the licence, the row of this table that justifies it, and the ADR. An entry without those four fields fails the scan. Adding an entry is a reviewed change like any other, and the licence auditor re-verifies the whole list at every release.

**What this list does not cover.** Anything linked into product code, which must satisfy Section 6.1 on its own, and the two non-open exceptions in Section 6.3 (Firebase Cloud Messaging and the Apple Push Notification service), which are not open source at all and are isolated behind `IPushSender`.

---

## 7. Microservices Architecture

### 7.1 Principles

- **Split by bounded context, never by entity.** The catalog is fixed at **20 data-owning services**, 16 of them Tier 1, plus the Gateway and two backends-for-frontends: 23 deployable applications and 7 worker images. Appendix L is the registry and the only place these names and counts are defined. Do not create a new service without a written reason: independent scaling, independent release, a different security level, or a different team. A smaller first release may merge Assessment into Academics and Behavior into Wellbeing and defer Hr, Operations and Ai, which is 14 services; merging is a build-time choice that leaves namespaces, routing keys and database names unchanged, so the split later is a deployment change.
- **Avoid the distributed monolith.** No shared database. No shared business-logic libraries. No chains of synchronous calls. If two services must always deploy together, they are one service.
- Every service has Clean Architecture inside, **its own PostgreSQL database and migrations**, its own Dockerfile, tests, health checks, OpenAPI document, message contracts, and README, and is **independently deployable and scalable.**
- **One mono-repo.** Shared `BuildingBlocks` libraries contain technical concerns only: authentication, tenancy, messaging, outbox, logging, result types, test helpers. Never domain logic.
- A **`dotnet new` service template** so that every service has the same structure, conventions, and pipeline. Consistency is what keeps a microservices codebase readable.

### 7.2 Service catalog

Names in this table are canonical and come from **Appendix L**. The long name is what prose uses; the short name is what projects, databases, exchanges and images use.

| Service | Long name | Tier | Owns |
|---|---|---|---|
| **Gateway** | Gateway (YARP) | 1 | Single public entry point: routing, token validation, tenant resolution from domain or subdomain, per-IP and per-tenant rate limiting, request limits, CORS, security headers, aggregated API docs, maintenance mode |
| **Bff.Web**, **Bff.Mobile** | Backends-for-frontends | 1 | Screen-shaped aggregation (dashboards, Student 360 from Reporting), response shaping per client, mobile delta-sync endpoints, mobile version policy. No business rules, no writes that bypass the owning service |
| **Identity** | Identity and Access | 1 | Users, login, OpenIddict token server, 2FA and passkeys, SSO, sessions and devices, roles, permissions, data scopes, invitations, join requests, delegation, access reviews, API keys and personal access tokens |
| **Platform** | Tenant, Platform, and Integrations | 1 | Tenants, plans, subscriptions, limits, usage metering, feature flags, branding, domains, tenant settings, terminology, custom-field definitions, provisioning, support desk, and the Integrations capability: public API keys, outgoing webhooks, OneRoster, LTI, developer portal |
| **School** | School Core (SIS) | 1 | School structure, campuses, academic years and terms, students, guardians, staff profiles, classes and sections, enrollment records, promotion, alumni |
| **Admissions** | Admissions | 1 | Inquiries, CRM pipeline, applications, assessments, offers, waiting lists, re-enrollment |
| **Academics** | Academics and Coursework | 1 | Curricula, lesson plans, coursework, assignments, submissions, question bank, quizzes, QTI import and export |
| **Assessment** | Assessment and Reporting | 1 | Assessment structures, gradebook, grading schemes with versions, moderation, report cards with versions, transcripts, grade-change requests |
| **Scheduling** | Scheduling and Calendar | 1 | Periods, bell schedules, rooms, timetable, OR-Tools solver worker, substitutions, calendar, events, room booking, iCal feeds |
| **Attendance** | Attendance and Safety | 1 | Student and staff attendance, excuses, thresholds, pickup persons, gate passes, dismissal, visitors, emergency broadcast and reunification |
| **Finance** | Finance | 1 | Fee structures, plans, invoices, payments, refunds, credit notes, discounts, scholarships, statements, cashier, accounting export |
| **Communication** | Communication | 1 | Announcements, news feed, messaging, meetings, surveys, policy acknowledgment, SignalR hubs |
| **Notification** | Notification | 1 | Channels, templates, preferences, quiet hours, digests, delivery log, channel workers, the unified inbox |
| **Requests** | Requests, Tasks, and Workflow | 1 | Request Center, approval engine, form builder, SLA and escalation, and the `Task` aggregate behind personal and assigned to-dos. See Section 11 |
| **Documents** | Documents | 1 | File storage, document templates, PDF generation through Gotenberg, certificates, QR verification, import and export jobs, virus scanning, OCR |
| **Behavior** | Behavior and Recognition | 1 | Behavior categories, incidents, points, houses, awards, badges, Open Badges export |
| **Reporting** | Reporting and Analytics | 1 | Read models built from events, dashboards, report builder, data quality center, early-warning models (ML.NET). Owns no source data |
| **Audit** | Audit | 1 | Append-only hash-chained audit store fed by audit events from every service, with search, export and integrity verification |
| **Wellbeing** | Student Wellbeing | 2 | Health clinic, counseling, special needs, interventions. Highest sensitivity: its own database, its own database role, its own encryption key, encrypted columns, every read logged |
| **Hr** | Human Resources | 2 | Contracts, leave, staff evaluation, payroll inputs, recruitment, professional development |
| **Operations** | Operations | 2 | Library, transport, inventory, facilities, front desk, activities. One service with a schema per sub-domain so that a later split is mechanical |
| **Ai** | AI Assist | 2 | Model gateway, embeddings, retrieval filtered by the caller's data scope, assistant, drafting. Off by default per tenant |

**Assessment and Behavior are separate services** from Academics and Wellbeing. The reason is load and sensitivity, not size: mark entry and report-card batches have a different scaling profile from coursework, and behavior records are parent-visible while wellbeing records are not. ADR-0002 records this, with the merge option above.

### 7.3 Communication and data

- **Asynchronous first.** Services integrate through RabbitMQ events (Section 8). Synchronous calls are the exception: internal **gRPC**, only for queries that truly need fresh data, with timeouts, retries with jitter, circuit breakers, and a cached fallback. **Maximum one synchronous hop.**
- **Local reference copies.** A service that needs another service's data keeps a slim read-only copy updated by events (for example, Finance keeps student id, name, grade, and status). It never queries another service's database.
- **Sagas for multi-service workflows:** enrollment, withdrawal clearance, year-end rollover, tenant provisioning, tenant deletion, and request fulfilment. Each saga has persisted state, timeouts, and **compensating actions**, and is visible in an admin process monitor.
- **No distributed transactions.** Consistency between services is eventual, and the UI must show honest "processing" states instead of pretending otherwise.
- **Cross-service screens** are served by the BFFs or by the Reporting read models, never by the browser calling ten services.
- **Identifiers** are UUID version 7, generated by the owning service. **Contracts** are versioned; consumer-driven contract tests (PactNet) protect them.

### 7.4 Multi-tenancy across services

- The gateway resolves the tenant (custom domain, subdomain, or header for mobile), and the tenant id travels in the token, in gRPC metadata, and in every message envelope. A request without a tenant is rejected, except on platform endpoints.
- Every service enforces tenancy itself: `TenantId` on every row, EF Core global filters, and PostgreSQL row-level security as a second barrier. Cache keys, file paths, logs, metrics, and traces all carry the tenant.
- **Isolation tiers per plan:** shared databases (default), dedicated databases for one tenant, or a fully dedicated deployment (on-premises or private cloud). The code is identical in all three.
- **Tenant provisioning is a saga:** create the tenant, seed defaults in each service, create the school administrator invitation, apply branding, load optional demo data, and report progress live to the platform console.
- Per-tenant rate limits, quotas, and job concurrency so that one school cannot degrade the others.

### 7.5 Authorization across services

- Identity and Access is the single source of truth for roles, permissions, and data scopes. Tokens stay small: user, tenant, roles, and a **permission version**.
- Each service enforces permissions locally through a shared authorization library that loads the user's effective permissions from Redis, keyed by that version.
- When an administrator changes a role, a `PermissionsChanged` event invalidates caches everywhere within seconds, and SignalR tells open clients to refresh menus and guards. **No logout is required.**
- Service-to-service calls use client credentials with narrowly scoped permissions. No service trusts a caller only because it is inside the network.

### 7.6 Resilience, observability, and scale

- Timeouts everywhere, retries with backoff and jitter, circuit breakers, bulkheads, idempotent handlers, liveness, readiness, and startup probes, graceful shutdown, and backpressure on queues.
- OpenTelemetry in every service. One correlation id from the gateway through gRPC and RabbitMQ to the last consumer. Service map, per-service SLO dashboards, centralized logs, and alerts.
- Stateless services that scale horizontally. HPA on CPU and latency for APIs; **KEDA on queue depth for workers.** PgBouncer pooling, read replicas for Reporting, and time or tenant partitioning for the largest tables (attendance, notifications, audit).
- Static web assets behind a CDN or the edge proxy with long cache lifetimes and hashed filenames.

### 7.7 Deployment modes

1. **Developer:** `aspire run` starts every service and dependency with one command and opens the dashboard with logs, traces, and metrics.
2. **Single server:** a Docker Compose profile that runs all services on one machine, for a single small school or an on-premises install. A small school must not need a Kubernetes cluster.
3. **Scale:** Kubernetes with Helm, rolling or blue-green releases, migrations as jobs, per-service pipelines triggered by path filters, and image, dependency, and license scans on every build.

---

## 8. RabbitMQ Messaging Design

RabbitMQ is the backbone for everything asynchronous. Design it deliberately; do not just "add a queue".

**Use RabbitMQ for:**

- Integration events between services (a service never touches another service's database)
- Notification fan-out to email, push, SMS, and in-app channels
- Heavy or bulk work: report card batches, invoice generation runs, bulk imports, year-end promotion, timetable solving, AI tasks, large exports
- Outgoing webhooks, audit and analytics projections, scheduled reminders triggered by Quartz.NET

**Do not use RabbitMQ for:** queries, or commands where the user is waiting for an immediate result. Those stay in-process through the mediator, or use gRPC when another service owns the data.

**Required design:**

1. **Transactional outbox** on every publish, and an **inbox table for idempotent consumers.** Delivery is at-least-once; every handler must be safe to run twice.
2. **Topology:** one topic exchange per service (`nibras.academics`, `nibras.attendance`, `nibras.finance`, ...). Routing keys follow `<module>.<entity>.<event>.v<version>`. One queue per consumer per purpose. **Quorum queues** for durability.
3. **Failure handling:** bounded retries with exponential backoff, then a dead-letter exchange and a parking-lot queue per consumer, with an admin screen to inspect, replay, or discard failed messages.
4. **Priority lanes:** separate queues and workers for urgent traffic (emergency broadcast, OTP, absence alert) and bulk traffic (digests, report batches), so that a bulk job can never delay an urgent message.
5. **Tenant fairness:** one large school's bulk job must not starve other tenants. Apply per-tenant concurrency limits or partitioning.
6. **Ordering where it matters** (for example, events for the same student): consistent-hash exchange or single active consumer.
7. **Standard envelope:** `messageId`, `correlationId`, `causationId`, `tenantId`, `userId`, `occurredAt`, `schemaVersion`. Contracts live in a shared, versioned contracts project; breaking changes publish a new version alongside the old one.
8. **Reliability settings:** publisher confirms, manual acknowledgements, tuned prefetch, graceful shutdown that finishes in-flight messages.
9. **Separate worker hosts** inside the owning service, scaled independently by KEDA on queue depth: `Notification.Worker` (channel lanes and digests), `Documents.Worker` (PDF, imports, exports, virus scan, OCR), `Scheduling.Worker` (OR-Tools), `Assessment.Worker` (result calculation and report-card batches), `Finance.Worker` (invoice runs, reminders, statements), `Reporting.Projections`, and `Ai.Worker`. There is no standalone imports worker: importing is a job group inside `Documents.Worker`, because it shares the same storage, scanning and error-report machinery.
10. **Progress feedback:** long jobs report progress over SignalR ("Generating report cards: 412 of 800"), can be cancelled, and leave a result record with downloadable output and an error report.
11. **Observability:** trace context propagated through message headers, RabbitMQ Prometheus plugin, alerts on queue depth, consumer count, and dead-letter growth.
12. **Testing:** consumer tests with Testcontainers, contract tests for message schemas, and a chaos test that kills a worker mid-batch and proves nothing is lost or duplicated.

---

## 9. User Roles and Authorization

| Role | Scope |
|---|---|
| **Platform Administrator** | Tenants, plans and subscriptions, feature flags, platform health, support tools |
| **School Owner / Group Director** | Cross-campus view, finance and performance oversight |
| **School Administrator / Principal** | Full control of one school |
| **Vice Principal / Academic Coordinator / Head of Department** | Curricula, timetables, exams, moderation, teacher oversight |
| **Registrar / Admissions Officer** | Inquiries, applications, enrollment, records, transfers |
| **Teacher** and **Homeroom Teacher** | Classes, attendance, coursework, grades, communication; homeroom adds whole-class oversight |
| **Student** | Own timetable, coursework, grades, attendance, resources |
| **Parent / Guardian** | All linked children, communication, fees, approvals, meetings |
| **Accountant / Finance Officer** | Fees, invoices, payments, refunds, expenses, reports |
| **HR Officer** | Staff records, leave, payroll inputs, appraisals |
| **Counselor, Nurse, Special-Needs Coordinator** | Restricted, sensitive records |
| **Librarian, Transport Coordinator, Receptionist / Security, Store Keeper, IT Support** | Their own modules |

For every role define: responsibilities, dashboard, workflows and actions, data scope, notifications, and mobile capabilities.

**Authorization model:**

- Permission-based. Roles are editable bundles of granular permissions, per module and action: **view, create, edit, delete, approve, publish, export, manage.**
- Custom roles, per-user overrides, time-limited delegation (a vice principal covers for an absent principal), and **data scopes** (own classes, own department, own campus, own children).
- One person can hold several roles (a teacher who is also a parent) with a clean role switcher.
- **Field-level protection** for sensitive data (medical, counseling, special needs, custody notes), with access logging and a "break-glass" emergency access that requires a reason and alerts the principal.
- Guardian-level restrictions: custody and legal limits on who may view data, receive messages, or pick up a child.
- Server-side enforcement on every endpoint; the UI only reflects it. Automated tests must prove that tenant A cannot read tenant B, and that a parent cannot read another family's child.
- Support impersonation by platform staff only with tenant consent, a visible banner, and a full audit trail.

---

## 10. Admin Console and Access Management

The administrator must have **full, visible control** of the system without ever needing a developer. Build two consoles on the same design system.

### 10.1 The two consoles

- **Platform Console** (the SaaS operator): tenants, provisioning progress, plans and limits, feature flags per tenant, platform announcements, health of every service, queue and job dashboards, tenant usage analytics, school billing records, support tickets, consent-based impersonation, release notes, a global template library, maintenance mode, tenant export and deletion.
- **School Admin Console** (each tenant): everything in Sections 10.3 to 10.6.

Every other role gets its own **workspace**, not a cut-down admin screen: a teacher workspace (my day, my classes, attendance, grading queue, messages), a student workspace, a parent workspace with a child switcher, and workspaces for finance, registrar, HR, and the supporting roles. Navigation, home screen, and quick actions are generated from the user's permissions.

### 10.2 First run and the default administrator

- On first start, seed one **super administrator**: username **`admin`**, initial password **`@Like12345`**.
- Read both values from configuration (`Seed:AdminUsername`, `Seed:AdminPassword`), with these values as the development and demo defaults, so that production can override them without a code change.
- **Safeguards, all mandatory:**
  - Force a password change on the first login; the default password can never be set again for that account.
  - In the Production environment, block all admin functions until the password has been changed and 2FA has been enrolled, and show a warning in the platform console while any account still uses a seeded password.
  - The seeder is idempotent: it never recreates the account and never resets an existing password.
  - **The seeder refuses to start** when the configured password still equals the documented default and the environment is not Development. The documented default appears in this brief and in demo configuration only; the repository secret scan allows it under `docs/` and fails on it anywhere else.
  - Store only a salted hash. Never log the password, never place it in client code, tests output, or API responses.
  - Lockout and rate limiting apply to this account like any other. Provide a documented **break-glass recovery command** (CLI, server access required) in case the super administrator is locked out.
  - The last super administrator cannot be deleted, deactivated, or stripped of the role.
- A new tenant never receives a shared password: its first school administrator joins through a single-use invitation link.

### 10.3 User management

List with search, filters, and saved views; create, invite, bulk import, activate, suspend, deactivate; reset password and force sign-out; active sessions and devices; login history; merge duplicate accounts; link and unlink guardians and students; assign roles, scopes, campuses, and departments; bulk actions; export.

### 10.4 How people join

| Method | Flow |
|---|---|
| **Invitation** | Admin invites by email or SMS with a pre-assigned role and scope. Single-use link that expires after 14 days, with a reminder to the invitee at day 7. Resend and revoke. |
| **Join code or QR** | The school publishes a code per audience (staff, parents, students). The person registers, and lands in an approval queue. |
| **Parent self-registration** | The parent enters the student code plus a verification detail. Auto-match when it is certain, otherwise manual review. One parent account links to several children, including children in different schools on the platform. |
| **Bulk import** | Accounts are created from Excel with activation links, a dry run, and an error report. |
| **SSO just-in-time** | First login through Google or Microsoft creates the account, with a default role chosen by email-domain rules, optionally pending approval. |
| **From admissions** | Accepting an applicant creates the student and guardian accounts automatically. |
| **From HR** | Hiring a staff member creates the account and starts an onboarding checklist. |

**Join request workflow:** Submitted → identity verified → reviewed → approved with role and data scope, or rejected with a reason → activated → welcome tour. Pending users see a clear waiting screen. Administrators set the default role, approver, and expiry per join method. Leaving is a workflow too: offboarding revokes access immediately, reassigns ownership of classes and open tasks, and keeps history.

### 10.5 Roles and permissions interface

- Built-in role templates (Administrator, Principal, Teacher, Homeroom Teacher, Student, Parent, Accountant, Registrar, and the rest of Section 9). System roles are locked but can be **cloned** into custom roles.
- A **permission matrix** of modules by actions, with search, grouping, bulk toggles, plain-language descriptions of every permission, and dependency hints (edit requires view).
- Data scope picker (all, campus, department, own classes, own children), field-level toggles for sensitive data, and per-request-type approval rights.
- **"View as role" preview**, an **effective-permissions explainer** for any user ("why can this person see this?"), role comparison, and a full change history.
- Time-limited role assignments, delegation during absence, **four-eyes approval** for granting high-risk permissions, and periodic **access review campaigns** where managers confirm or remove access.
- Changes apply live, within seconds, without signing users out.

### 10.6 What the school administrator controls

School profile and campuses; modules on and off; **theme and branding editor with live preview**; academic configuration; grading schemes; request types and approval chains (Section 11); custom fields; form builder; notification and document templates; numbering formats; integrations, API keys, and webhooks; imports and exports; audit viewer; security policy (password rules, 2FA enforcement per role, session timeout, optional IP allowlist for admin access); background job monitor; failed-message console; data quality center; **recycle bin** with restore; usage against plan limits.

---

## 11. Request Center and Approval Engine

One engine handles every request in the school. It replaces paper forms, scattered emails, and hallway conversations, and it is a major part of what makes the product feel complete.

### 11.1 Engine capabilities

- **Request type designer (no code):** form fields from the form builder, required attachments, who may submit (roles and scopes), optional request fee, SLA target, assignment rule, approval chain, output document template, notifications, and the **effect on approval.**
- **Approval chains:** sequential, parallel, any-of or all-of, conditional routing (for example, amount above a limit goes to the principal; more than three days of leave goes to the vice principal), auto-approval rules, delegation, and escalation when the SLA is breached.
- **States:** Draft → Submitted → Under review → Needs information → Approved or Rejected → In progress → Completed; plus Cancelled, Withdrawn, and Expired. Every transition is audited and published as an event.
- **Effects on approval, executed automatically through a saga:** approved student leave marks attendance as excused; an approved transport change updates the route and notifies the driver; an approved certificate is generated as a PDF with a QR code and delivered; an approved fee plan regenerates the installments; approved staff leave triggers the substitution workflow.
- **Inboxes:** "My requests" and "My approvals" on web and mobile, one-tap approve or reject with a reason, bulk approval, comments thread, internal notes hidden from the requester, reminders, and a satisfaction rating on completion.
- **Analytics:** volume by type, turnaround time, SLA compliance, bottleneck approvers, and rejection reasons.

### 11.2 Request catalog (ship these as ready templates; all editable)

**Parents and students**

- *Attendance and safety:* absence or leave, late arrival, early dismissal, change of authorized pickup persons, one-time gate pass
- *Documents:* enrollment certificate, transcript, report card copy, conduct certificate, transfer certificate, fee statement, official letter ("to whom it may concern"), ID card replacement
- *Academic:* section change, elective or subject change, grade review or appeal, makeup or re-sit exam, exam accommodation, extra support or tutoring, club or activity enrollment, trip consent
- *Finance:* installment plan, payment extension, discount or sibling discount, scholarship or financial aid, refund, payer change (company or sponsor)
- *Transport:* subscribe, cancel, change route or stop, temporary change
- *Health and care:* medication administration authorization, allergy or dietary update, counseling appointment (confidential), special-needs assessment
- *Records:* update address, phone, or guardian details, with verification before the record changes
- *Meetings:* teacher, homeroom teacher, counselor, or principal
- *Lifecycle:* re-enrollment, withdrawal or transfer out (starts the clearance workflow)
- *Other:* complaint, suggestion, lost and found, uniform or book order, locker, account or technical support

**Staff**

- *Leave:* configurable leave types (annual, sick, emergency, maternity and paternity, unpaid, pilgrimage, study), hourly permission, remote-work day
- *Teaching:* period swap, substitution, schedule change, grade change after lock, exam paper printing, field trip or event proposal, student referral to counselor, nurse, or discipline
- *Resources:* room or facility booking, equipment, purchase requisition, maintenance ticket, IT support
- *HR:* salary certificate or HR letter, expense reimbursement, salary advance, overtime, training or professional development, document update, resignation

**Administration**

- Access or role request, temporary permission elevation, data export, data deletion, bulk data change, fee waiver above limit, budget approval, plan or module change

---

## 12. Signature Features: What Makes This Product Special

These are the differentiators. They must be excellent, not just present.

1. **"Today" dashboards, not vanity dashboards.** Every role opens to a prioritized list of what needs attention now: unmarked attendance, pending approvals, overdue grading, students to follow up. **Every insight links to an action.** No chart exists unless someone can act on it.
2. **Student 360.** One page and one chronological timeline per student: attendance, grades, behavior, health visits, fees, communication, documents, interventions, filtered by the viewer's permissions.
3. **Sixty-second attendance.** Seating-chart or list view, one tap to mark all present and then flag exceptions, works offline on mobile and syncs later. Optional QR or NFC check-in at the gate.
4. **Early-warning with explanation and follow-through.** Combine attendance, grades, behavior, and missing work into a risk indicator that always shows **why** the student was flagged. A flag opens an **intervention workflow**: assign owner, plan, follow-up dates, outcome. Use explainable ML.NET models or transparent rules. Never show a bare score.
5. **Smart timetable.** Constraint-based automatic generation with OR-Tools (teacher availability, room types, subject weekly loads, consecutive-period rules, part-time staff, shared teachers across campuses), manual drag-and-drop refinement with live conflict detection, what-if scenarios, and **one-tap substitute suggestions** ranked by availability, subject, and workload fairness.
6. **Report Card Studio.** Per-school template designer, bilingual output, several grading schemes, teacher comment bank plus AI-drafted comments that teachers must review, moderation and approval chain, batch generation through RabbitMQ with progress, and a **QR verification code** on every official document that opens a public verification page.
7. **Parent experience that respects attention.** A calm daily or weekly **digest** per child instead of notification spam, urgent alerts that break through, per-channel preferences, quiet hours, read receipts, and automatic translation of school messages between Arabic and English with the original always available.
8. **Safety and dismissal.** Authorized pickup lists with photo, one-time QR or PIN gate passes, early-dismissal requests with approval, visitor management, and **emergency broadcast** with acknowledgement tracking and a live "who has not confirmed" view.
9. **Go live in a day.** Onboarding wizard, downloadable Excel templates, import with validation, **dry-run preview, error report, and rollback**, duplicate detection, and OCR-assisted data capture from scanned documents.
10. **Configurable without code.** Custom fields per entity, a form builder (admission forms, consent forms, surveys), a configurable multi-step **approval workflow engine**, document and numbering templates, and notification templates. This is what lets one product fit very different schools.
11. **Command palette and natural-language search.** Ctrl+K to jump anywhere or run any action; administrators can ask questions in plain language ("students absent more than five days this month in grade 7") with results limited by their permissions.
12. **Offline-first mobile.** Attendance, timetable, grade entry, and messages work without a connection, with a visible sync state and clear conflict handling.
13. **Data quality center.** Surfaces missing guardian contacts, duplicate students, classes without teachers, unpublished grades, and other gaps, each with a fix action.
14. **Trust by design.** Full audit trail with before and after values, consent tracking, data export and deletion per student, retention policies, and a privacy dashboard for the school.
15. **Sales-ready demo mode.** A rich, realistic demo school in both languages, one-click reset, and a guided tour per role.
16. **White-label mobile apps.** Build flavors produce a school-branded app (name, icon, colors) from the same Flutter codebase, alongside the shared multi-school app.
17. **Photo and media consent.** A per-student consent flag controls whether the child may appear in galleries and news posts; tagging a student without consent blocks publishing.
18. **Student portfolio and recognition.** Achievements, badges, house points, certificates, and selected work collected across years, exportable when the student leaves.
19. **Kindergarten daily sheet.** For early years: meals, naps, mood, activities, photos (with consent), and a daily note to parents, entered in seconds.
20. **Balanced class formation.** Build next year's sections automatically while balancing gender, ability, behavior, special needs, and keep-together or keep-apart rules, then adjust by drag and drop.
21. **Inspection and accreditation readiness.** Self-evaluation against a configurable framework, improvement plans, KPIs, and an evidence folder that fills itself from system data.
22. **Policy and handbook acknowledgment.** Publish a policy, require staff or parents to acknowledge it with a recorded signature, and chase those who have not.
23. **Live, animated, modern interface.** A motion system and design language (Section 16) that make the product feel fast and crafted rather than like a legacy ERP.
24. **Open by default.** Public REST API, outgoing webhooks, iCal calendar feeds, SSO through OpenID Connect (Google and Microsoft accounts), and the open education standards from 1EdTech: **OneRoster 1.2** for rostering, **LTI 1.3** for external learning tools, **QTI 3** for question bank import and export, **CASE** for curriculum standards, and **Open Badges 3.0** for verifiable digital badges and certificates.

---

### 12.1 Twenty more that decide whether a school switches

Items 1 to 24 above are the product's substance. These twenty numbers, nineteen of them signature features since item 39 moved to engineering capabilities, are the moments that make someone choose it over a system that already works well enough. Each one names the person it serves, the **assist rung** it runs at (Section 25), and its tier. Appendix W is the register, Appendix P compares each against the market, and Appendix O is the demo script that has to show them.

| # | Feature | The moment | Rung | Tier |
|---|---|---|---|---|
| 25 | **Morning brief per role** assembled before the day starts: the principal's approvals, the teacher's cover and gaps, the parent's one line per child, the accountant's overnight payments | "I knew my day before I arrived" | 1, phrasing at 3 | 1 |
| 26 | **Exception-only attendance**: the gate scan, the bus boarding, and approved leave pre-fill the register, so the teacher confirms exceptions instead of marking a class | Attendance in twenty seconds | 1 | 1 |
| 27 | **Explain this number**: every figure on every dashboard, report card and statement drills to the records and the scheme version that produced it | The audit test, on screen | 1 | 1 |
| 28 | **Because panel**: every automated action shows its reasons and offers an override that records who and why | Automation people trust | 1 to 2 | 1 |
| 29 | **Smart defaults engine**: country and school type infer the work week, calendar, numerals, tax treatment, e-invoicing plug-in, terminology, bell schedule, request templates and role templates, and the wizard shows exactly what it inferred so it can be corrected | Live in a day | 1 | 1 |
| 30 | **Intervention playbooks**: an early-warning flag opens a plan with an owner, steps, review dates and an outcome, from a library the school can edit | A warning becomes an action | 1 to 2 | 1 |
| 31 | **Guardian transparency**: a parent sees which roles read their child's sensitive records and when, alongside their consents and the retention clock | Families trust the system | 1 | 1 |
| 32 | **Emergency mode**: one tap turns the staff app into roll call by location, broadcast with acknowledgement tracking, and reunification with verified pickup at assembly points | Safety when it counts | 1 | 1 |
| 33 | **Campus digital twin**: rooms, live occupancy from timetable and roll call, utilisation heatmap, and maintenance tickets on a floor plan. Rooms are tracked, never children | See the whole school | 1 | 2 |
| 34 | **Teacher five-minute mode**: a mobile home with attendance, a quick note, a quick grade and the cover alert, and nothing else | The Monday-morning test | 1 | 1 |
| 35 | **Parent calm screen**: one card per child per day, with "nothing needs your attention" as a designed state and urgent items that break through | Attention respected | 1 | 1 |
| 36 | **School memory**: the year's portfolio and yearbook assembled from consented media, achievements and comments, exportable per student when they leave | A keepsake that costs no staff time | 1 | 2 |
| 37 | **Template exchange**: schools opt in to share request types, report-card templates and rubrics between tenants, with attribution and a review step | Every school starts further along | 1 | 2 |
| 38 | **Plug-in kit**: contracts, a sample implementation and a certification checklist for regional integrations (e-invoicing, ministry export, payment, SMS) | Partners extend it without us | 1 | 2 |
| 39 | Moved to engineering capabilities (ADR-0019). **Calendar-aware scaling** (caches warm and workers scale up before each school's first period in its own time zone, and scale down after dismissal) cannot be demonstrated on the demo data, so it fails the rule below. It is still built and measured as an engineering capability; the number is kept so that items 40 to 44 keep theirs | none | 1 | 1 |
| 40 | **Self-healing operations**: dead letters replay automatically once their transient cause clears, the tenant health score raises a ticket before the school calls, and each release publishes a plain-language "what changed" per tenant | Fewer incidents, fewer calls | 1 | 1 |
| 41 | **Configuration as code**: a tenant's settings, roles, request types and templates export as versioned JSON, which can be reviewed, backed up, cloned to a sandbox and restored | Governance without a consultant | 1 | 1 |
| 42 | **Mastery and next step**: a standards heatmap per student and class, with a suggested next step drawn from the curriculum mapping | Teaching insight, not another report | 1 to 2 | 2 |
| 43 | **Workload balance**: teaching load, cover fairness and grading turnaround surface strain to coordinators. Staff analytics only, never students | The school looks after its people | 1 to 2 | 2 |
| 44 | **Low-bandwidth mode**: a data-saver profile with compressed images, delta sync and text-first notifications for families on slow connections and cheap phones | It works on the phone people actually own | 1 | 1 |

**The rule that keeps this list honest.** A feature earns a number here only when a persona can name the moment it saves them time, worry or money, **and** it can be demonstrated in under sixty seconds on the demo data. Anything that cannot be demonstrated is a feature in Appendix A, not a reason to switch.

## 13. Functional Scope

**Tier 1 must be complete and polished before Tier 2 begins.** Tier 3 must be anticipated in the data model and extension points, and built last.

> **Appendix A is normative.** The lists below are summaries. The full feature catalog per module, with business rules, is in `02-appendices.md`, and every item there is a requirement.

### Tier 1: Core

1. **Platform and tenancy:** tenant provisioning, plans and module toggles, usage limits, white-label branding, custom domain, tenant export and deletion, platform announcements, health and usage monitoring.
2. **School setup:** profile, campuses, academic years, terms, grading periods, work week, holidays, grading schemes, numbering formats, onboarding wizard.
3. **Identity, access, and admin console:** everything in Section 10: login, SSO, two-factor authentication, invitations, join codes and approval queue, password policy, sessions and devices, roles and the permission matrix, data scopes, delegation, access reviews, parent-to-student linking, role switching.
4. **Request Center:** the engine and the full catalog in Section 11.
5. **Tasks and notification center:** personal and assigned to-dos generated by workflows, due dates, reminders, and one inbox for notifications, approvals, and mentions.
6. **Admissions CRM and enrollment:** inquiries and leads, school tours, online application, configurable review pipeline, assessments and interviews, document collection, decisions, offer and deposit, waiting lists, enrollment, section assignment, sibling detection, **annual re-enrollment.**
7. **Student information:** profile with bilingual names, guardians and custody, emergency contacts, medical summary, documents with expiry, siblings, academic history, status changes, house/group membership, bulk promotion, ID cards.
8. **Academic structure:** stages, grade levels, sections, subjects, electives and student groups, curricula, subject-to-grade mapping, teaching assignments, class capacity.
9. **Staff:** profiles, qualifications, contracts, departments, workload, documents with expiry.
10. **Timetable and calendar:** periods and bell schedules (including special days and Ramadan timings), rooms, the smart timetable described above, substitutions, school calendar, events, exam calendar, iCal feeds.
11. **Attendance:** daily and per-period, staff attendance, late and early leave, excuses with approval, absence alerts, thresholds and escalation, reports and regulatory formats.
12. **Coursework:** lesson plans and syllabus coverage, assignments with attachments and rubrics, online submission, late rules, grading, feedback, resubmission, **homework load view** so students are not overloaded on one day.
13. **Assessment and grading:** assessment structures and weights, exam scheduling, seating and invigilation, accommodations, mark entry with validation and keyboard-speed grids, absent and makeup handling, moderation, approval, publishing, locking, grade change requests with audit, percentage and letter and descriptive and standards-based schemes, GPA, ranking (optional), report cards, transcripts.
14. **Communication:** targeted announcements, school news feed, one-to-one and group messaging with moderation and office hours, parent-teacher meeting booking, read receipts, translation.
15. **Notifications:** in-app, email, push, SMS adapter, templates per language, preferences, quiet hours, digests, delivery log, all dispatched through RabbitMQ.
16. **Fees and finance:** fee structures, installment plans, discounts, scholarships, sibling discounts, invoice runs, tax/VAT, multi-currency, online and manual payments, receipts, refunds, credit notes, overpayments, late fees, reminders and escalation, statements of account, cashier day-close, revenue and aging reports, pluggable e-invoicing and accounting export.
17. **Behavior:** positive and negative records, points and house system, incidents, actions, parent notification, behavior plans.
18. **Reports and analytics:** role dashboards, report library, custom report builder with saved views, scheduled delivery, PDF and Excel export, branded official documents, regulatory report templates per country as plug-ins.
19. **Documents and certificates:** templates, generation, QR verification, central document storage with access control.
20. **Administration:** audit log, login history, data import and export, data quality center, background job monitor, failed-message console, settings, in-app help and changelog.

### Tier 2: Extended

- **Learning:** shared resources, online quizzes and exams with a question bank, auto-grading, randomization, time limits; online classes via Jitsi or LiveKit
- **Health clinic:** visits, medication, vaccinations, allergies alerts to relevant staff, parent notification
- **Counseling and special needs:** confidential case notes, referrals, individual education plans, accommodations that flow into exams
- **HR:** recruitment, leave and approvals, staff attendance, payroll inputs and payslips, appraisals, classroom observations, professional development
- **Library:** catalog, barcode lending, reservations, fines
- **Transport:** routes, stops, vehicles, drivers, student assignment, boarding attendance, fees, parent notifications
- **Activities and trips:** clubs, teams, events, trips with consent forms and payment, competitions and awards
- **Front desk:** visitors, gate passes, calls log, parent complaints and requests as a helpdesk with SLA tracking
- **Surveys and forms**, **polls and student council elections**, **facility booking and maintenance tickets**, **inventory, assets, uniform and book sales**
- **Media gallery** with consent checks, **student portfolio and badges**, **lost and found**, **parent volunteering and event sign-ups**
- **Group-level management:** consolidated reporting, shared staff, and shared policies across the schools of one owner

### Tier 3: Optional

Cafeteria with prepaid wallet, hostel or boarding, alumni, live bus tracking on OpenStreetMap, accounting general ledger, procurement, public school website and CMS, biometric or RFID device integration.

---

## 14. End-to-End Workflows

Design each as a state machine with **states, transitions, responsible roles, validations, notifications, audit entries, and messages published to RabbitMQ.** Provide a diagram for each in Phase 0.

1. Inquiry → application → review → assessment → decision → deposit → enrollment → accounts created → welcome pack
2. Re-enrollment for next year: invitation → parent confirmation → fee settlement check → seat reserved or released
3. Daily attendance: mark → absence alert → excuse → approval → thresholds → escalation → intervention
4. Assignment: create → notify → submit → late handling → grade → feedback → gradebook
5. Exam to report card: schedule → mark entry → validation → moderation → approval → lock → batch generation → publish → grade change request
6. Fees: plan → invoice run → reminders → payment → receipt → overdue escalation → service restrictions per school policy → reporting
7. Teacher absence: leave request → approval → substitute suggestion → timetable update → notifications
8. Early dismissal and pickup: parent request → approval → gate pass → verification at gate → log
9. Incident to intervention: record → notify → action → plan → follow-up → outcome
10. Transfer or withdrawal: request → clearance (fees, library, assets) → documents → archive
11. End of year: finalize results → promote, retain, or graduate → archive year → roll over structure, fees, and timetable skeleton
12. Tenant lifecycle: signup → provisioning saga → onboarding → live → plan change → suspension → export → deletion
13. Joining: invitation or join code → registration → verification → approval with role and scope → activation → welcome tour
14. Offboarding: trigger → immediate access revocation → reassignment of classes, tasks, and approvals → history retained
15. Service request: submit → route → approve or reject or ask for information → automatic effect → document delivery → rating
16. Role change: request → four-eyes approval for high-risk permissions → live propagation → audit entry
17. Staff hiring: vacancy → applicants → interview → offer → contract → account and onboarding checklist
18. Purchase: requisition → budget check → approval → order → receipt → inventory update

---

## 15. Edge Cases You Must Handle

- Student moves section or campus mid-term: grades, attendance, fees, and timetable history
- Several children per parent across grades; guardians with different legal access; a guardian who loses access by court order
- A teacher who is also a parent in the same school; a staff member at two campuses
- Grade correction after publishing; report card regeneration and versioning
- Timetable change mid-term and its effect on attendance already recorded
- Partial payments, overpayments, refunds, discounts after invoicing, a payer who is not a guardian (company or sponsor), fee plan change mid-year
- Staff member leaves mid-year: reassign classes, keep history, revoke access immediately
- Different grading schemes by stage; electives and mixed-grade groups; students repeating a year; late joiners with partial-term weights
- Time zones, week start, Hijri display, Ramadan schedules, holidays per campus
- Arabic and English names, search that tolerates Arabic spelling variants and diacritics, mixed-direction text in PDFs
- Soft delete and archiving; historical reports must stay reproducible after structures change
- Concurrent edits (optimistic concurrency), offline mobile edits that conflict on sync
- Duplicate or out-of-order messages, a worker crash mid-batch, a bulk job cancelled halfway
- Very large schools: 800 report cards, 5,000 invoices, 10,000-row imports without timeouts
- A tenant exceeding plan limits; a suspended tenant; a tenant requesting full data deletion

- A service is down: the gateway degrades gracefully, the rest of the product keeps working, and queued work completes when the service returns
- Events arrive before the local reference copy exists (a payment for a student the Finance service has not seen yet)
- A saga fails halfway: compensation runs, the administrator sees exactly what happened, and nothing is left half-applied
- A permission is revoked while the user has a screen open, or an approver leaves while requests are waiting for them
- The same person joins twice through different methods; a parent with children in two schools on the platform
- A contract change is deployed to one service before its consumers are updated

List any more you identify.

---

## 16. Design, Motion, and User Experience

The interface must look and feel like a modern consumer product, not an ERP. Deliver the design system **before** feature screens.

### 16.0 Brand: Nibras (نبراس)

- **Name:** Nibras in Latin script, نبراس in Arabic. Always written as a single capitalized word in English. Never translated, abbreviated, or written in all capitals in running text.
- **Meaning and idea:** a lamp that guides. The product makes the state of the school clear and shows each person what to do next. The "Today" dashboards, the early-warning explanations, and the calm parent digest are the brand promise in practice.
- **Taglines:** English: "The guiding light for modern schools." Arabic: "نبراس المدرسة الحديثة". Both are placeholders for the product owner to confirm.
- **Logo direction:** a simple light or lamp mark that works at app-icon size, with a bilingual lockup (Latin and Arabic wordmarks together) and single-script variants. Deliver it as original SVG artwork; do not use stock marks.
- **Color and tone:** one confident primary color with a warm light accent that reflects the name, generated into accessible tints by the token system. Voice is clear, calm, and respectful in both languages.
- **White-label rule:** a school's own name, logo, and colors replace the Nibras brand inside its tenant and its branded mobile app. "Powered by Nibras" appears only where the plan allows and the school has not purchased its removal. The Nibras brand is always used on the marketing site, the Platform Console, the shared multi-school mobile app, system emails from the platform, and documentation.
- **Technical naming:** solution `Nibras.sln`, root namespace `Nibras`, and the prefixes defined in the reference architecture, Section 7.
- **Not yet cleared:** the name has not been checked for trademarks, domains, or app-store availability. Keep the product name in one configuration value and one token file so that it can be changed in a single place.

### 16.1 Design language

- Clean and spacious: generous white space, a clear type scale, soft depth (subtle shadows and layered surfaces), rounded corners, and one confident accent color per school.
- **Bento-style dashboards:** modular cards of different sizes, each answering one question and linking to one action.
- **Design tokens** (color, type, spacing, radius, elevation, motion) defined once and consumed by both Angular and Flutter. Per-school theming from a single brand color, with automatic generation of accessible tints and an automatic contrast check.
- Light and dark themes, comfortable and compact density modes, and full RTL mirroring, including icons and charts that imply direction.
- Typography: Inter for Latin and IBM Plex Sans Arabic for Arabic, tuned so that mixed-language screens look intentional.
- A documented component library with live examples (Storybook for Angular, Widgetbook for Flutter).

### 16.2 Motion system

Motion must explain, never decorate. Define it as tokens and reuse it everywhere.

- **Durations:** about 100 ms for feedback, 200 ms for small transitions, 300 to 400 ms for page and panel transitions. **Easing:** one standard curve, one for entering, one for leaving.
- **Web:** native CSS transitions and keyframes with Angular's `animate.enter` and `animate.leave`, route changes through the **View Transitions API** (`withViewTransitions`), shared-element transitions from a list row to its detail page. Never use the deprecated `@angular/animations` package.
- **Mobile:** implicit animations, Hero transitions, `flutter_animate` for choreography, staggered list entrances, pull-to-refresh, swipe actions, and light haptic feedback on key actions.
- **Patterns to implement:** skeleton loading that morphs into content; staggered card entrance on dashboards; number count-up on KPIs; animated chart drawing and smooth data updates; the attendance tap that ripples and settles; drag-and-drop timetable with a lifted card and live conflict highlighting; progress rings for long jobs; toast and snackbar slide-ins; a brief, tasteful celebration when a student earns a badge or a setup checklist completes; animated empty states.
- **Rules:** animate only `transform` and `opacity`; hold 60 frames per second on a mid-range phone; never block input while animating; never delay a frequent action for the sake of an animation; **honor `prefers-reduced-motion` and the mobile accessibility setting** with a calm fallback.

### 16.3 Experience principles

- **Speed of work:** keyboard-first grids for marks and attendance, bulk actions, inline edit, saved filters, recently visited, favorites, command palette, undo instead of confirm where safe, autosave for long forms.
- **Every state designed:** loading, empty, error with a way forward, offline, partial data, "processing" for eventual consistency, and no-permission.
- **Perceived performance:** optimistic updates, instant navigation, prefetching, virtual scrolling, and background processing with progress for anything slow.
- **Accessible and inclusive:** WCAG 2.2 AA, scalable text, screen-reader labels, focus management, color-blind-safe status colors that never rely on color alone.
- **Mobile:** role-specific home, deep-linked push notifications, biometric unlock, camera and document scan upload, home-screen quick actions, a child switcher for parents, and home-screen widgets for today's timetable.
- **Guidance:** first-run tours per role, contextual help, setup checklists, and "what's new".
- **Tone:** clear, human microcopy in both languages, written by someone fluent, never machine-literal.

---

## 17. Localization and Regional Fit

- **Two first-class languages, more by configuration.** Every string externalized; no concatenated sentences; correct pluralization in Arabic (six forms) and English; a translation workflow with keys, context notes, and a missing-translation report in CI.
- **Bilingual data, not only a bilingual interface:** names, addresses, subject names, report card comments, and official documents hold Arabic and English values, with a rule per document for which to print.
- **RTL done properly:** mirrored layouts, icons, charts, steppers, and swipe directions; bidirectional text handled in inputs, tables, PDFs, and notifications; numerals configurable (Western or Arabic-Indic).
- **Arabic-aware search and sorting:** normalize alef, hamza, ta marbuta, and diacritics; correct collation; tolerant matching of transliterated names.
- **Calendars and time:** Gregorian with optional Hijri display and printing; configurable work week and weekend; holidays per campus; Ramadan bell schedules; school time zone for display, UTC for storage.
- **Money:** multi-currency, configurable decimals and rounding, tax or VAT per fee item, amounts in words in both languages on receipts.
- **Terminology overrides per tenant:** "Grade" or "Year", "Term" or "Semester", "Section" or "Class", "Guardian" or "Parent", applied everywhere including documents.
- **Regional plug-ins** (interfaces in Tier 1, implementations per target country): e-invoicing (for example ZATCA in Saudi Arabia or JoFotara in Jordan), ministry and regulatory reports, national ID validation, local payment gateways, local SMS providers, and country grading rules.
- **Formats:** phone numbers in E.164 with country picker, addresses by country template, names in the local order with as many parts as the culture uses (first, father, grandfather, family).

---

## 18. Mobile Application Requirements

- **One Flutter app, role-based,** plus white-label build flavors per school. Special modes on the same codebase: **gate and security mode** (scan passes, visitor check-in), **bus attendant mode** (boarding attendance, route list), **nurse mode**, and a **kiosk mode** for self check-in on a tablet.
- **Feature parity matrix** in Phase 0: for every feature, state whether it is web only, mobile only, or both, and why. Parents and students must be able to do everything on mobile. Teachers must be able to do all daily tasks on mobile. Heavy configuration stays on the web.
- **Offline-first** for attendance, timetable, grade entry, daily sheets, messages, and gate scanning, with Drift storage, an outbox queue, background sync, visible sync status, and defined conflict rules per entity.
- **Notifications:** channels and categories (urgent, academic, finance, requests, messages), deep links to the exact screen, actionable notifications (approve, acknowledge), badge counts, quiet hours, and per-child settings for parents.
- **Security** aligned with OWASP MASVS 2: secure storage, biometric unlock, session timeout, screenshot protection on sensitive screens, root and jailbreak advisory, optional certificate pinning, no sensitive data in logs or notifications shown on the lock screen.
- **Store compliance:** in-app account deletion request, accurate privacy labels, no third-party advertising or tracking SDKs, age-appropriate design, and store listings in both languages.
- **Lifecycle:** forced and recommended update prompts and remote configuration served by our own backend, environment flavors, crash and error reporting through GlitchTip, and staged rollouts.
- **Quality:** phones and tablets, portrait and landscape where useful, dynamic text size, screen readers, 60 frames per second on a mid-range Android device, cold start under 3 seconds, small install size, and low data usage with image compression before upload.
- **Delight:** home-screen widgets (today's timetable, next due item), quick actions, haptics, camera and document scanner for submissions and documents, QR scanner, and calendar export.

---

## 19. Architecture and Engineering Standards

### Backend

- **Microservices as defined in Section 7**, with **Clean Architecture inside every service**: Domain, Application, Infrastructure, and API layers, dependencies pointing inward, enforced by NetArchTest. Prefer vertical slices inside the Application layer so that one feature lives in one folder.
- **Readable by design:** one service template, one naming convention, one way to write a handler, an endpoint, a consumer, and a test. Small files, explicit names, no clever abstractions. A developer who knows one service can read them all.
- **SOLID** and patterns only where they earn their place: CQRS through the mediator, domain events, Specification, Strategy (grading schemes, fee rules, notification channels), State (workflows), Outbox/Inbox, Unit of Work through EF Core, Result pattern for expected failures.
- Rich domain model for rule-heavy areas (grading, fees, promotion, timetable). Plain CRUD for reference data. Do not force DDD ceremony onto simple tables.
- REST API with OpenAPI, URL versioning, Problem Details, cursor or page pagination, filtering and sorting conventions, ETags and optimistic concurrency, idempotency keys for payments and submissions.
- **Rate limiting has three layers and each owns a different question.** The Gateway limits per source address and per tenant, and protects the platform from traffic. Each service limits per user and per endpoint through `Nibras.BuildingBlocks.Web`, and protects a handler from one noisy caller. The Platform service enforces plan quotas (jobs, SMS credits, storage, AI usage), and protects the commercial model. All three read counters from `redis-state`. A rate-limit breach at the Gateway or a service returns Problem Details with status 429, the `<SERVICE>_RATE_LIMITED` code and `Retry-After`. A plan quota breach is not a rate limit: it returns status 402 with `PLATFORM_PLAN_LIMIT_REACHED` (Appendix K), no `Retry-After`, and the limit, the current usage and the upgrade action.
- **Multi-tenancy:** as defined in Section 7.4, implemented once in the shared tenancy building block and applied identically in every service.
- **Where the building-block boundary runs.** A building block may evaluate *policy data* that the platform owns: which tenant a request belongs to, which permissions a token carries, which cache key a tenant gets. It may never evaluate *domain rules*: what a grade is worth, when a fee is late, who may collect a child. An architecture test forbids any reference from `Nibras.BuildingBlocks.*` to any `*.Domain` project, which is what keeps this line honest.
- Cross-cutting pipeline: validation, authorization, tenant resolution, transaction, audit, logging, performance timing.
- Time handled through an injected clock, stored in UTC, displayed per school time zone. Money stored as decimal with currency.
- Configuration by environment, secrets outside the repository, health and readiness endpoints, graceful shutdown.

### Web and Mobile

- Angular: one workspace with separate lazy-loaded applications or areas for the Platform Console, the School Admin Console, and the role workspaces, sharing one UI library; standalone components, signals, zoneless change detection, strict TypeScript, generated API client, permission directive and route guards driven by server permissions, global error and offline handling.
- Flutter: presentation, domain, and data layers; repository pattern over Drift cache and API; offline queue with retry and conflict resolution; environment flavors; crash reporting through a self-hosted open-source option (for example GlitchTip) or none.

### Repository structure

```
/src
  /BuildingBlocks        technical libraries only (tenancy, auth, messaging, outbox, results, testing)
  /Contracts             versioned message and gRPC contracts, one package per service
  /Gateway               YARP
  /Bff.Web  /Bff.Mobile
  /Services/<Name>       <Name>.Domain, .Application, .Infrastructure, .Api, .Worker, .Tests
  /Web                   Angular workspace: apps (platform-console, school) and libs (ui, data-access, features)
  /Mobile                Flutter app: core, features, flavors
  /AppHost               Aspire orchestration
/deploy                  compose, helm, opentofu
/docs                    architecture, adr, runbooks, api, messages, user-guides, project (state files)
/tools                   service template, seeders, license scan, scripts
```

### Conventions (apply everywhere, enforce with analyzers and CI)

- **C#:** nullable reference types on, warnings as errors, `.editorconfig` and analyzers, file-scoped namespaces, async all the way with `CancellationToken`, no static state, no logic in controllers or endpoints, injected clock and ID generator, no PII in logs.
- **API:** plural nouns, kebab-case paths, `/api/v{n}/`, camelCase JSON, ISO 8601 UTC timestamps, enums as strings, one pagination envelope, one filter and sort syntax, Problem Details with a stable **error code** from a central catalog, messages localized on the client from the code, `ETag` and `If-Match` for updates, `Idempotency-Key` for unsafe retries, consistent bulk endpoints with per-item results.
- **Database:** snake_case, one schema owner per service, UUID v7 keys, `tenant_id` first in composite indexes, audit columns (`created_at/by`, `updated_at/by`), soft delete with `deleted_at/by`, optimistic concurrency token, foreign keys only inside a service, expand-and-contract migrations that are safe under rolling deployment.
- **Angular:** strict mode, feature-first folders, smart and presentational components, signals and SignalStore, `OnPush` or zoneless, no business rules in components, typed forms, generated API clients only, lint and format in CI.
- **Flutter:** feature-first folders, immutable state, repositories hide data sources, no business rules in widgets, `very_good_analysis`-level lints, golden tests for key screens in LTR and RTL.
- **Git:** trunk-based with short-lived branches, Conventional Commits that reference requirement IDs, a pull request template with the definition of done, protected main branch, required checks.

### Performance engineering: EF Core and PostgreSQL

Speed is a feature of Nibras, designed in from the first service, not tuned at the end. **EF Core is the data access layer for every service.** These rules are mandatory.

**DbContext and model**

- Register contexts with **DbContext pooling**. A pooled context keeps state between leases, so the tenant is never stored in a field set at construction: resolve it per lease through a scoped tenant accessor and reset it when the context is returned.
- Use **EF Core 10 named query filters**: one named `Tenant` filter and one named `SoftDelete` filter per entity. `IgnoreQueryFilters()` without names is forbidden outside platform-level code, enforced by an analyzer rule or an architecture test. Ignoring the `Tenant` filter requires a platform permission and an audit entry.
- Use a **compiled model** for services with large models, to cut startup time. Never run migrations at application startup: ship migration bundles and run them as deployment jobs.
- No lazy loading, anywhere. No entity is ever returned from an API.

**Reads**

- Read handlers use `AsNoTracking()` and **project straight to DTOs with `Select`**. Load only the columns the screen needs.
- **Keyset (cursor) pagination** for any list that can grow large (students, attendance, messages, audit, notifications). Offset pagination only for small bounded lists. Every list endpoint has a maximum page size.
- More than one collection `Include`: use `AsSplitQuery()` or separate queries to avoid a Cartesian explosion. Prefer projections to `Include`.
- **Compiled queries** (`EF.CompileAsyncQuery`) for the hottest paths: attendance register, timetable of the day, permission lookup, dashboard cards.
- Stream large exports with `IAsyncEnumerable`; never materialize them in memory.
- Dashboards and heavy reports read from the **Reporting read models**, summary tables, or materialized views refreshed by events, and may use Dapper or raw SQL. They never aggregate transactional tables on demand.

**Writes**

- Rely on EF Core batching for normal saves. For set-based changes use **`ExecuteUpdateAsync` and `ExecuteDeleteAsync`**, which run one SQL statement without loading entities (bulk status changes, promotion, reminders marked as sent, retention jobs).
- For imports and other inserts above a few hundred rows use **Npgsql binary `COPY`** into a staging table, validate there, then merge. The 10,000-row import target depends on this.
- Optimistic concurrency with the PostgreSQL `xmin` system column. Keep aggregates small so a write touches few rows.

**PostgreSQL**

- Every multi-tenant index starts with `tenant_id`. Use covering indexes (`INCLUDE`) for hot list queries, **partial indexes** `WHERE deleted_at IS NULL` for soft-deleted tables, GIN indexes for full-text, trigram, and `jsonb` search.
- **Partition by month** the largest tables (attendance records, notifications and deliveries, audit entries, messages) and detach old partitions according to the retention schedule.
- UUID version 7 keys keep index inserts local. Avoid wide rows in hot tables; move large text and `jsonb` to side tables.
- **Row-level security must stay index-friendly:** the policy is a plain equality on `tenant_id` against `current_setting('app.tenant_id')`. With PgBouncer in transaction mode, set the variable with `SET LOCAL` inside each transaction, never per session. An integration test must prove that a connection reused from the pool cannot see the previous tenant.
- PgBouncer in transaction mode in front of every database; verify prepared-statement compatibility for the versions in use. Size pools from load tests, not by guessing.
- Read replica for Reporting, with replica-lag awareness in the read model.
- `pg_stat_statements` enabled everywhere. Autovacuum tuned for the high-churn tables.

**Proof, not belief**

- A **command-counting interceptor** in integration tests fails a test when a handler exceeds its query budget. This is how N+1 queries are caught before production.
- A slow-query interceptor logs any command above a threshold with the tenant and the handler name, and exports a metric.
- The plan lists the **top queries per service with their indexes**, and each one gets `EXPLAIN (ANALYZE, BUFFERS)` evidence on demo-scale data before its service is declared done.
- BenchmarkDotNet micro-benchmarks for rule-heavy hot paths (grade calculation, fee allocation, permission evaluation).

### Caching with Redis

**Redis is the cache and the shared in-memory store of the platform.**

- **Licensing note.** Redis 8 and later is available under AGPLv3, an OSI-approved open-source license, alongside its two source-available options. Nibras runs Redis **as an unmodified standalone server** under the AGPLv3 option, which the policy in Section 6.1 allows, and links only the MIT-licensed StackExchange.Redis client. Do not modify Redis, do not embed it, and do not use Redis Enterprise or any paid Redis module. Valkey (BSD-3) speaks the same protocol and works with the same client: keep it as a **drop-in fallback** and run the integration suite against both, so that the choice can be reversed by changing a container image.
- **`HybridCache` is the caching API in every service** (`Microsoft.Extensions.Caching.Hybrid`, MIT): a fast in-process L1, Redis as the distributed L2, **stampede protection** so that one cold key causes one database query and not hundreds, and **tag-based invalidation**. Services never call Redis directly for caching.
- All access goes through the `Nibras.BuildingBlocks.Caching` wrapper, which **adds the tenant to every key and tag automatically**. A cache entry without a tenant is impossible by construction, except for explicitly platform-scoped entries. A test proves that tenant A can never read tenant B's entry.

**Keys, lifetimes, and invalidation**

- Key format: `nibras:{tenant}:{service}:{entity}:{id}:v{n}`. Tags: `tenant:{id}`, `student:{id}`, `section:{id}`, `timetable:{version}`, and similar. Bump `v{n}` when a cached shape changes, so that deployments never read stale shapes.
- **Cache-aside only.** The database is always the source of truth. No write-behind.
- **Event-driven invalidation:** the consumer of a change event removes entries by tag. Time-to-live is the safety net, not the mechanism. Add random jitter to every TTL to avoid synchronized expiry.
- L1 entries on other instances are not evicted by a remote change, so keep **L1 lifetimes short (seconds to a minute)** for mutable data, and broadcast invalidation for the few entries where that is not enough (permissions, feature flags, branding) over Redis pub/sub or the existing `permissions.changed` and `settings.changed` events. FusionCache (MIT) is an acceptable alternative implementation behind the same abstraction if a built-in backplane proves necessary.
- Short **negative caching** for "not found" on hot lookups, to stop repeated misses from reaching the database.

| Data | L1 | L2 (Redis) | Invalidated by |
|---|---|---|---|
| Effective permissions per user (keyed by permission version) | 60 s | 30 min | `permissions.changed`, `role.changed`, broadcast |
| Tenant settings, branding, feature flags, terminology, plan limits | 60 s | 1 h | `settings.changed`, `feature-flag.changed`, `plan.changed`, broadcast |
| Reference data: grade levels, sections, subjects, periods, codes, grading schemes | 5 min | 6 h | the owning `*.changed` event |
| Published timetable per section, teacher, and room | 5 min | 12 h | `timetable.published`, `timetable.changed`, `substitution.assigned` |
| Slim student and staff directory entries | 30 s | 15 min | `student.*`, `staff.*` |
| Role home and dashboard payloads | 15 s | 60 s | TTL, plus targeted tags for counters |
| Notification templates, document templates, request type definitions | 5 min | 6 h | their `*.changed` events |
| Public QR verification results, public school pages | none | output cache 5 min | `certificate.revoked`, publish events |

- **Never cached in Redis:** wellbeing records, counseling notes, medical details, custody information, passwords, tokens beyond their purpose, payment details, and marks while they are being entered. Financial balances are read from the database; at most a few seconds of L1.
- **Marks in progress stay fast without a cache.** The mark-entry grid reads one covering index on `(tenant_id, section_id, component_id)` and writes back in batches through `ExecuteUpdateAsync`, so a keyboard-speed grid never depends on a cache that could serve another teacher a stale number. Published results are cached; drafts are not.
- **Warm before the peak.** A scheduled job pre-loads tenant settings, permissions of active staff, and the day's timetable shortly before the first period in each school's time zone, so that the 8:00 a.m. attendance peak hits a warm cache.

**Other uses of Redis** (each with its own key prefix): SignalR backplane, rate limiting, idempotency keys, one-time codes and short-lived tokens, long-job progress, presence, ASP.NET Core output caching for public endpoints, and short distributed locks. A Redis lock is a performance optimization, never a correctness guarantee: anything that must be exactly-once relies on database constraints, the inbox, or PostgreSQL advisory locks.

**Operating Redis**

- **Two logical deployments:** `redis-cache` with `maxmemory` and `allkeys-lfu` eviction and no persistence, and `redis-state` (backplane, locks, rate limits, idempotency) with `noeviction` and persistence. Mixing them lets a cache flood evict a lock.
- TLS, ACL users per service restricted to their key prefix, no dangerous commands exposed, Sentinel or cluster mode for high availability in the scale deployment, a single instance in the single-server mode.
- Compact serialization (System.Text.Json source generation or MessagePack), a payload size limit, compression for large values, and no unbounded collections under one key.
- Dashboards and alerts for hit ratio, latency, evictions, memory, and connection count. A falling hit ratio is an alert.
- **Graceful degradation:** if Redis is unavailable, a circuit breaker opens, services fall back to L1 and the database, and the product gets slower but keeps working. A resilience test proves it.

### Fast APIs, web, and mobile

- **API:** Minimal APIs, System.Text.Json source generation, Brotli compression, HTTP/2 and HTTP/3 at the edge, `ETag` and `304 Not Modified` for cacheable reads, output caching for public endpoints, sparse payloads shaped by the BFFs so that a screen needs one or two calls and not ten, server garbage collection and sensible container limits, pooled `HttpClient` and gRPC channels, and no synchronous-over-asynchronous code.
- **Web:** route-level lazy loading, `@defer` for below-the-fold blocks, zoneless change detection with signals, virtual scrolling, `NgOptimizedImage`, preloading of likely next routes, hashed static assets with long cache lifetimes behind the edge proxy or a CDN, an Angular service worker for the application shell, optimistic updates, and **bundle budgets enforced in CI.**
- **Mobile:** render from the local Drift cache first and refresh in the background, delta sync by change token, image caching and compression before upload, heavy parsing in isolates, `const` widgets and lazy list builders, deferred initialization so that cold start stays under 3 seconds.

### Performance budgets (enforced in CI and in load tests)

| Budget | Target |
|---|---|
| API read, served from cache | p95 under 80 ms at the service |
| API read, from the database | p95 under 250 ms |
| API write | p95 under 500 ms |
| Database commands per request | 5 or fewer for a typical request; any handler above 10 needs an ADR |
| Single SQL command | p95 under 50 ms on demo-scale data; none above 200 ms without an ADR |
| Cache hit ratio for reference data, settings, and permissions | 95% or higher during school hours |
| Web: Largest Contentful Paint, Interaction to Next Paint, Cumulative Layout Shift | under 2.5 s, under 200 ms, under 0.1 on a mid-range phone |
| Web initial bundle per application | budget set in Phase 0 and enforced by the Angular build |
| Mobile cold start and frame rate | under 3 s, 60 frames per second on a mid-range Android device |
| Teacher marks attendance for one class, end to end | under 60 seconds including network |

A change that breaks a budget fails the pipeline. Raising a budget requires an ADR.

### Data integrity and reconciliation

Eventual consistency is honest only when something checks it. Each of these runs on a schedule, reports to the Data Quality Center, and raises an alert when it finds a difference it cannot explain.

- **Aggregate invariants.** Every aggregate in Appendix F states its invariants, and they are enforced in the domain, not in the database alone. An invariant that only a database constraint enforces is a rule nobody can read.
- **Reference-copy reconciliation.** Nightly, each service that keeps a local copy of School data compares a checksum of its copy against School over gRPC, for the tenants it serves. A mismatch repairs itself by replaying from the source and raises a data-quality issue. This is what makes "a service never reads another service's database" safe rather than hopeful.
- **Finance daily balance.** At day close, the sum of postings must equal invoices minus payments minus credits minus write-offs, per tenant and per series. A difference blocks the close and names the documents involved.
- **Attendance against timetable.** A daily job lists sessions that the published timetable expected and attendance never received, and the reverse. This is how a mid-term timetable change that orphaned a class gets found the same day.
- **Historical reproducibility.** Every calculation stores the version of the scheme that produced it. A report for a closed year recomputes to the same number after the school has changed its grading scheme, because it recomputes with the version that was in force. A report that cannot be reproduced is a defect.

### Runtime fallbacks if .NET 8 is chosen

Decision 1 defaults to .NET 10. If .NET 8 is imposed, these rules change and nothing else does. Record the choice as an ADR and keep this list with it.

| Rule in this brief | Depends on | Fallback on .NET 8 |
|---|---|---|
| Named `Tenant` and `SoftDelete` query filters | EF Core 10 named filters | One combined global filter per entity, plus an architecture test that forbids `IgnoreQueryFilters()` outside platform code. The tenant filter can then only be bypassed wholesale, so the platform-permission-and-audit requirement becomes stricter, not looser |
| `HybridCache` as the caching API | .NET 9 and later | FusionCache (MIT) behind the same `Nibras.BuildingBlocks.Caching` surface, with its Redis backplane for L1 invalidation |
| Compiled model for large services | EF Core 8 supports it | No change |
| `ExecuteUpdateAsync` and `ExecuteDeleteAsync` | EF Core 7 and later | No change |
| Keyset pagination, `AsNoTracking`, projections | Any supported EF Core | No change |
| Support window | .NET 8 loses support in November 2026 | The product ships on a runtime without security patches, which Section 20 forbids for children's data. This is the reason the default is .NET 10 |

### Container and runtime rules that make Arabic work everywhere

These are correctness requirements, not deployment preferences. Arabic shaping, the Hijri calendar and school time zones all depend on them, and all three fail silently when they are wrong.

- **Globalization stays on.** `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT` is false in every image. A CI test inside the built image asserts that `ar-SA` resolves, that `UmAlQuraCalendar` is available, and that `Asia/Riyadh`, `Asia/Amman` and `Asia/Dubai` resolve to the expected offsets.
- **Base images.** Debian-based `aspnet` images by default. If a service uses Alpine it must install `icu-libs` and `tzdata` explicitly and carry the native assets SkiaSharp needs; the same CI test then runs against that image.
- **ICU and tzdata are pinned** per release and bumped deliberately, because a silent ICU change alters Hijri conversions and a silent tzdata change alters bell schedules.
- **Fonts ship with the renderer.** The Documents image and the Gotenberg container bundle Inter, IBM Plex Sans Arabic and a Noto Naskh fallback. A PDF snapshot test in both languages compares against committed baselines, which is the only way Arabic shaping regressions get caught before a parent sees them.
- **Containers run non-root** with a read-only root filesystem, a `tmpfs` for temporary files, and `TZ=UTC`.
- **Paths and cultures in code.** Compose paths with `Path.Combine`, never with a literal separator. Parse and format with an explicit culture; `CultureInfo.InvariantCulture` for anything stored or transmitted, the user's culture only for display. A developer's machine culture must never change a result, which is what makes the Windows and Linux test runs comparable.

### Quality and Delivery

- Unit tests for domain and application logic, integration tests against real PostgreSQL, RabbitMQ, and Redis through Testcontainers, contract tests for APIs and messages, consumer-driven contract tests with PactNet, Playwright end-to-end tests for the workflows in Section 14, Flutter widget and integration tests for critical flows, k6 load tests for the scenarios in Section 21, and resilience tests that stop a service or a worker during a workflow.
- One command starts everything locally (`aspire run`, with an equivalent `docker compose up`): gateway, BFFs, all services and workers, web, PostgreSQL, RabbitMQ, Redis, SeaweedFS, Gotenberg, Mailpit, Ollama (optional profile), observability stack.
- Per-service CI pipelines triggered by path filters: build, test, lint, format check, architecture tests, **license scan**, dependency vulnerability scan, container scan, secret scan.
- Zero-downtime deployment, forward-only reviewed migrations, seed and demo data commands, backup and **tested restore** procedure.
- Documentation: architecture overview, Architecture Decision Records, module guides, API guide, message catalog, runbooks, developer onboarding guide, administrator manual, and end-user help in both languages.

---

## 20. Security, Privacy, and Child Safety

**Standards:** OWASP **ASVS 5.0 Level 2** for web and APIs, OWASP **MASVS 2** for mobile, OWASP Top 10 and API Security Top 10 as minimum checklists.

**Threat model.** In Phase 0, produce a STRIDE threat model per service and a list of abuse cases with their controls. At minimum: cross-tenant access; a parent reading another family's child by changing an ID; privilege escalation through the role editor; stolen invitation or gate-pass links; mass export by an insider; grade or payment tampering; malicious file uploads; forged or replayed messages on RabbitMQ; prompt injection against the AI assistant; takeover of an administrator account.

**Controls**

- Object-level authorization on every resource access, not only on endpoints. Automated tests generated from the permission matrix.
- **Tamper-evident audit log** (hash-chained, append-only). Sensitive exports require a reason, are watermarked with the requester, rate limited, and reported to the principal.
- Files: type and size validation, ClamAV scan, stored outside the web root, served only through short-lived signed URLs, safe content disposition.
- Secrets outside the repository with rotation; encryption in transit everywhere, including RabbitMQ and PostgreSQL; column encryption for sensitive fields; encrypted backups.
- Least privilege in infrastructure: a database user per service, a RabbitMQ user and permissions per service, Kubernetes network policies, non-root containers with read-only file systems.
- Supply chain: pinned dependencies, SBOM, signed images, vulnerability and license scans on every build, automated update pull requests.
- Accounts: 2FA (TOTP and passkeys), enforced per role; new-device login alerts; session and device management; lockout and rate limits; breached-password check against an offline list.
- An independent penetration test before general availability, a `security.txt`, and a vulnerability disclosure process.

**Privacy**

- A data inventory with classification (public, internal, confidential, sensitive) for every field, and a retention schedule per data type with automated deletion or anonymization.
- Guardian consent management with versioned consent texts; access, correction, export, and erasure workflows; data residency per tenant; a published sub-processor list; privacy notices inside the product.
- **No advertising, no sale of data, no behavioral tracking of students.** Product analytics covers staff and administrator usage only and is self-hosted.
- A data protection impact assessment template that a school can complete and export.
- Designed to adapt to GDPR-style laws and to the data protection laws of each target country.

**Child safety and safeguarding**

- Messaging policy per school: who may message whom, during which hours. **Student-to-student messaging is off by default.** No public profiles. No student search by outsiders.
- Report and block in every conversation; attachment scanning; optional, transparent keyword flagging that alerts the safeguarding officer.
- A **safeguarding officer role** with oversight access that is itself logged and reviewable.
- Photo and media consent enforced at publishing time (Section 12).
- Location is tracked for vehicles, never for children.
- AI never profiles a child in ways a guardian or the school cannot see and challenge.

---

## 21. Non-Functional Requirements

| Area | Requirement |
|---|---|
| **Security** | OWASP ASVS 5.0 Level 2 and MASVS 2 as the targets (Section 20); short-lived access tokens with rotating refresh tokens; 2FA; lockout; rate limiting; secure headers and CSP; input validation and output encoding; upload validation with type checking and ClamAV scanning; encryption in transit and for sensitive columns at rest; no secrets in code; dependency scanning |
| **Privacy** | Data about minors: minimization, consent records, purpose limitation, access logs for sensitive records, retention rules, export and erasure, configurable data residency; designed to be adaptable to GDPR-style and local regulations |
| **Performance** | The budgets in Section 19 (*Performance budgets*) are binding: cached reads p95 under 80 ms, database reads under 250 ms, writes under 500 ms, five or fewer database commands per typical request, 95% cache hit ratio for reference data, Core Web Vitals within target. No N+1 queries, proven by the command-counting interceptor |
| **Scale targets** | 500 schools, 500,000 students, 20,000 concurrent users; the 8:00 a.m. attendance peak; 800 report cards generated in under 10 minutes; 10,000-row import in under 5 minutes |
| **Reliability** | 99.9% availability target; no message loss; idempotent operations; retries with backoff; circuit breakers on external services; graceful degradation when AI, SMS, or payment providers are down |
| **Recovery** | Point-in-time recovery; RPO 15 minutes or better, RTO 4 hours or better; restore drills documented |
| **Auditability** | Who, what, when, where, before and after values, for every sensitive action; audit records are append-only |
| **Accessibility** | WCAG 2.2 AA on web, verified by axe-core in CI. On mobile, WCAG 2.2 AA applied through Flutter semantics, plus the Android and iOS accessibility guidelines, verified by semantics tests and a manual pass per release with TalkBack and VoiceOver |
| **Flexibility** | New request types, roles, custom fields, forms, templates, and approval chains without code; new modules as new services without touching existing ones; every external provider replaceable behind an interface |
| **Maintainability** | Enforced conventions, architecture tests, ADRs, a new developer productive within two days |

---

## 22. SaaS Commercial Layer and Customer Success

- **Acquisition:** public marketing site, pricing page, and **self-service signup** that provisions a trial tenant with demo data in minutes, followed by a guided onboarding checklist and a conversion flow.
- **Plans:** priced per active student with tiers; modules as add-ons; limits on students, storage, SMS credits, and AI usage; monthly and annual terms; coupons; multiple currencies; trials and grace periods.
- **Tenant billing:** invoices to schools, payment recording through the manual or gateway adapter, reminders and dunning, and a fair suspension policy: **read-only mode first, and data export always available. A school's data is never held hostage.**
- **Usage metering:** services publish usage events; the Platform service aggregates them for limits, billing, and the tenant health score.
- **Customer success:** in-app help and support widget feeding a platform ticket desk, knowledge base in both languages, CSAT and NPS surveys, a **tenant health score** from adoption metrics, lifecycle emails, a public status page, an in-app changelog, and a feature-request board with voting.
- **Partners (Tier 3):** reseller accounts that manage their own tenants, with commissions and white-label options.
- **Legal:** versioned terms of service, privacy policy, and data processing agreement, with recorded acceptance per tenant and per user.

---

## 23. Operations and Release Management

- **Environments:** development, test, staging with production-like anonymized data, and production. Everything is defined as code (OpenTofu, Helm) and deployed through GitOps.
- **Releases:** trunk-based development, semantic versions per service, feature flags for incomplete work, canary or blue-green rollout, **backward-compatible migrations (expand, migrate, contract)**, tested rollback, release notes generated from commits.
- **Reliability practice:** SLOs and error budgets per service, actionable alerts only, runbooks for every alert, an incident process with blameless postmortems, and planned maintenance with tenant notification.
- **Backup and recovery:** encrypted backups, point-in-time recovery, **restore of a single tenant** without affecting others, and quarterly restore and disaster-recovery drills with recorded results.
- **Capacity and cost:** a model of infrastructure cost per 1,000 students for each deployment mode, load-test evidence behind it, and log and metric retention policies.
- **Legacy migration toolkit:** mapping templates, a staging area, validation and reconciliation reports, repeatable dry runs, and adapters for common export formats, so that moving a school from an old system is a process and not a project.
- **On-premises customers:** versioned offline install and upgrade bundles, a pre-upgrade check, automatic backup before upgrade, and a documented rollback.

---

## 24. Test Strategy and Quality Gates

- **Pyramid:** many fast unit tests on domain and application logic; integration tests per service against real PostgreSQL, RabbitMQ, and Redis through Testcontainers; contract tests for every API and message; a small number of end-to-end tests for the workflows in Section 14.
- **Generated security tests:** from the permission matrix, generate tests proving each role can and cannot do exactly what the matrix says; a **tenant isolation suite** that attacks every endpoint and consumer with another tenant's identifiers.
- **Interface tests:** Playwright end-to-end, visual regression snapshots in light, dark, LTR, and RTL; axe-core accessibility checks; Flutter widget, golden, and integration tests.
- **Non-functional tests:** k6 load tests for the scale targets, soak tests, resilience tests that stop services and workers mid-workflow, restore tests, and performance budgets enforced in CI for bundle size and key API latencies.
- **Test data:** builders and Bogus generators, a deterministic demo seed, and anonymization for staging.
- **Mutation testing** with Stryker.NET on rule-heavy domains (grading, fees, promotion, permissions).
- **Acceptance:** a UAT script per role written in plain language, and a **golden-path demo script** that must pass before any release.
- **Quality gates per phase:** all tests green, coverage thresholds on domain and application layers, zero high or critical vulnerabilities, license scan clean, accessibility checks clean, performance budgets met, documentation and traceability updated.

### The coverage matrix: what proves what

Every artefact this product produces has one named proof. Nothing is "done" on the strength of a reading. Appendix V holds the full matrix and the test-case format; this is the contract.

| Artefact | What proves it | Where it runs |
|---|---|---|
| Requirement `REQ-<AREA>-<NNN>` | At least one acceptance test `TC-<AREA>-<NNN>` written as Given, When, Then | Integration or end-to-end |
| Business rule `BR-<AREA>-<NNN>` | A table-driven unit test built from the rule's worked examples, plus a property-based test for arithmetic rules, plus a mutation score of 80% or better on that class | Service unit tests |
| Workflow `WF-<AREA>-<NN>` | One test per transition and one per failure and compensation path; sagas add a timeout test | Integration tests with Testcontainers |
| Endpoint | Generated permission-matrix test, tenant-isolation attack test, Problem Details contract test, query-budget assertion | Generated suites and service integration |
| Event | Schema contract test, deliver-twice idempotency test, ordering test on the partition key | Contract tests and service integration |
| Cache entry | Invalidation test driven by the real event, tenant-key isolation test, Redis-down fallback test | Service integration |
| Hot query | Committed `EXPLAIN (ANALYZE, BUFFERS)` evidence on demo-scale data, plus the budget assertion | Service integration and `docs/perf/` |
| Web screen | Playwright happy path, axe-core check, visual snapshots in light, dark, left-to-right and right-to-left, and a story for every state | Web pipeline |
| Mobile screen | Widget test, golden test in both directions, offline-sync test | Mobile pipeline |
| Generated PDF | Snapshot in English and Arabic against committed baselines, with a shaping check | Documents integration |
| Migration | SQL linter, plus a test that the previous image runs against the new schema | Pipeline |
| Signature feature | The demo test case in Appendix O, run against the demo data | End-to-end |
| Runbook | Executed in a game day within ninety days of being written | Operations calendar |
| Platform behaviour | A test on that operating system or device class, per the matrix in Appendix X | The matching runner or device |
| Plan document | The scorecard at 4 or better on every axis, and a clean `kit-lint` run | Review |

**Unit testing standards.** Arrange, act, assert. One behaviour per test. Names read `Method_State_Expected`. Data comes from builders and seeded generators, never from shared mutable fixtures. The clock and the identifier generator are injected; no test sleeps. Domain tests touch no infrastructure. Any rule that handles text is tested with Arabic and English data. Assertions pin the culture explicitly rather than inheriting the machine's, so a Windows run and a Linux run compare like for like. Coverage thresholds are 90% on domain and 80% on application layers, enforced in the pipeline. A flaky test is quarantined with an owner and a 48-hour deadline, never re-run until it passes.

---

## 25. AI Principles

- AI is **assistive, never authoritative.** A human approves anything that touches a student record, a grade, or a message to a parent.
- Runs on **local open-weight models** by default. No student data leaves the school's infrastructure unless the school explicitly enables an external provider.
- Respects permissions and tenant isolation exactly like any other feature. Retrieval is always filtered by the caller's data scope.
- **The AI service never reads another service's database.** Its index is built from integration events and from reads it is authorized to make through the backends-for-frontends. Every embedding carries the tenant id, the data scope of its source record, and the source version, so retrieval filters before it ranks. A change event re-indexes the record; a `school.student.status-changed.v1` to withdrawn purges it.
- Explainable: every prediction shows its contributing factors. Monitor for bias across student groups.
- Every AI feature has an off switch per tenant, and the product is fully usable with all of them off.
- The assistant treats retrieved content as data, never as instructions, to resist prompt injection, and it can only call tools that the current user is permitted to use.
- Log AI usage per tenant for metering and review, without storing more personal data than the feature needs.
- Prefer rules and classical models when they do the job. Use a language model only where language is the problem: drafting comments, summarizing, translating, natural-language querying, help assistant.

### The assist ladder

"AI" is not one thing, and treating it as one is how products become unusable when a model is unavailable. Every automated or suggested behaviour in Nibras declares which **rung** it runs at and what it falls back to. The product is complete and sellable at rung 1.

| Rung | What it is | Availability | Explanation it owes | Examples |
|---|---|---|---|---|
| **1** | Deterministic rules and smart defaults | Always on, no extra infrastructure | The rule id (`BR-…`) and the inputs | Exception-only attendance, fee calculations, escalation ladders, smart defaults at onboarding, morning brief assembly, cover suggestions |
| **2** | Classical models (ML.NET), explainable by construction | On by tenant choice, runs on ordinary hardware | The contributing factors and their weights | Early-warning flags, workload strain, anomaly hints on marks and attendance |
| **3** | A local open-weight language model | Off by default, needs a capable machine | The sources it drew on, and a human review step | Comment drafting, translation, summarising, natural-language query, the help assistant |
| **4** | An external provider behind an adapter | Off by default, never required, explicit per-tenant consent | Everything rung 3 owes, plus what left the school's infrastructure | Only where a school chooses it, and never for a rung 1 or 2 job |

**Autonomy is a second, separate scale.** The rung says what technology a feature needs. Autonomy says how far it acts on its own, and the two are independent.

| Autonomy | What it does | Where it is allowed |
|---|---|---|
| 1 Surfaces | Shows information the person already had a right to see, gathered in one place | Anywhere |
| 2 Suggests | Ranks or flags with visible reasons; the person decides | Anywhere, with the Because panel |
| 3 Drafts | Produces text or a plan that a person approves before it takes effect | Anywhere, with review enforced in the interface |
| 4 Acts | Performs a bounded action inside a written policy, with an audit entry and a one-click undo | **Never** over a grade, a payment, or a message to a family. Elsewhere only with a policy the school configured |

A rung 1 feature at autonomy 4 is a plain rule acting by itself. That combination is common, useful and the easiest to ship without noticing, which is why both numbers appear in Appendix W and in every idea card.

**Rules that apply at every rung.**

- **Fall back downward, never sideways.** If the language model is unavailable, a rung 3 feature degrades to a rung 1 experience (a template, a blank field with guidance), never to an error and never to a silent wrong answer.
- **The Because panel.** Any automated decision a person can see or is affected by shows its reasons in plain language, names the rung, and offers an override that records who overrode it and why. A score with no explanation is never shown.
- **Humans decide.** Nothing at any rung writes to a student record, a grade, or a message to a family without a person approving it.
- **Off switches are per tenant and per feature**, and the product is fully usable with every one of them off. A school that disables all four rungs above 1 loses suggestions, not function.
- **Retrieval respects the caller.** Filtering by data scope happens before ranking, not after. See the indexing rule above.
- **Metering and review.** Usage is logged per tenant for limits and review, holding no more personal data than the feature needs.

---

## 26. Working Method

Work in phases. **Do not write implementation code until Phase 0 is approved.**

### Phase 0 deliverables (your first response, split across several messages if needed)

1. Clarifying questions, and the assumption you will use for each if unanswered
2. **The plan documents defined in `docs/plan/PLAN_SPEC.md`**, which include the service catalog, a specification sheet per service, and the full repository, service, web, mobile, and deployment structures
3. **Competitive gap analysis:** compare this scope with established products (for example openSIS, Gibbon, Fedena, PowerSchool, Classter, Veracross) and list anything they offer that this brief lacks
4. Complete module and feature map
5. Role and permission matrix
6. Workflow state diagrams for Section 14, and the saga designs with their compensations
7. Solution architecture: service boundaries with the reason for each, context map, synchronous and asynchronous dependencies, repository and folder structure for services, building blocks, workers, web, and mobile, and the service template
8. The design system: tokens, type scale, color system, motion tokens, core components, and high-fidelity designs for the key screens of each workspace, in light, dark, LTR, and RTL
9. The admin console specification: permission catalog, role templates, join flows, and the request catalog with default approval chains
10. Data model per service, and the reference data each service replicates
11. **Message catalog:** every event and command, publisher, consumers, routing key, and RabbitMQ topology
12. Dependency list with the verified license of each exact version, plus the unavoidable-cost list
13. Risk register and the roadmap, phase by phase
14. **Traceability matrix:** every requirement in this document mapped to a module, a phase, and a test. Nothing may be left unmapped.

**Where each Phase 0 deliverable lands.** The fourteen deliverables above are not fourteen documents. This is the mapping, and `docs/plan/PLAN_SPEC.md` is the authority on what each document must contain.

| Phase 0 deliverable | Plan document |
|---|---|
| 1. Clarifying questions and assumptions | `01-questions-and-assumptions.md`, `docs/project/OPEN_QUESTIONS.md` |
| 2. The plan documents themselves | All of `docs/plan/`, per `PLAN_SPEC.md` |
| 3. Competitive gap analysis | `02-competitive-gap-analysis.md`, seeded from Appendix P |
| 4. Module and feature map | `03-requirements-catalog.md`, from Appendix A |
| 5. Role and permission matrix | `12-security-privacy-safety.md`, from Appendices B and I |
| 6. Workflow state diagrams and sagas | `13-workflows-and-sagas.md`, from Appendix R |
| 7. Solution architecture and structures | `04-architecture-overview.md`, `05-service-catalog.md`, `07-solution-structure.md` |
| 8. Design system and key screens | `14-design-system-and-ux.md` |
| 9. Admin console specification | `12-security-privacy-safety.md` and the Identity and Platform service sheets |
| 10. Data model per service and reference data | `10-data-architecture.md` and each `06-services/<service>.md` |
| 11. Message catalog and topology | `11-messaging-architecture.md`, from Appendix E |
| 12. Dependency and licence inventory | `19-dependency-and-license-inventory.md` |
| 13. Risk register and roadmap | `17-roadmap.md`, `18-risk-register.md` |
| 14. Traceability matrix | `20-traceability-matrix.md`, with the Workflow, Rule, Test case and Platform columns |

### Phase 1 onward

Start with the platform foundation: building blocks, service template, Gateway, Identity and Access with the admin console and seeded administrator, Tenant and Platform, Notification, Audit, the design system, and the pipeline. Then build vertically, one service at a time: domain, API, messages, workers, web UI, mobile UI, tests, seed and demo data, documentation. Finish and verify a module before starting the next. At the end of each phase, report: what was delivered, what was deferred and why, the updated traceability matrix, and known issues.

### Project memory (so that quality survives long work and new sessions)

Maintain these files in `/docs/project/` and treat them as the source of truth. Read them at the start of every session and update them at the end:

- `PROJECT_STATE.md`: current phase, what is done, what is in progress, what is next
- `TRACEABILITY.md`: requirement ID → service → phase → tests → status
- `BACKLOG.md`: ordered work items with requirement IDs
- `DECISIONS/`: one Architecture Decision Record per significant decision
- `RISKS.md`, `GLOSSARY.md`, `CHANGELOG.md`, and `OPEN_QUESTIONS.md`

### Response protocol

- Begin each response by stating what you are delivering and which requirement IDs it covers. End with status, open questions, and the proposed next step.
- **Never claim that something works, passes, or is complete unless you actually ran it.** Label anything unverified as unverified.
- Do not invent library APIs. Check the documentation of the pinned version. If you are unsure, say so.
- When a task is too large for one response, deliver a smaller complete vertical slice. Never deliver a broad, shallow, half-finished one.
- Before presenting work, run the **self-review checklist:** requirements covered; performance budgets met, query count checked, cache keys tenant-scoped and invalidation wired; permissions, tenancy, and audit applied; events published and consumers idempotent; all UI states present; both languages and RTL; tests written and run; licenses checked; documentation and project memory updated.

### Definition of done for every module

- Workflows run end to end on web, and on mobile where relevant
- Permissions, data scopes, and tenant isolation enforced and covered by tests
- Validation, errors, loading, empty, and offline states complete
- Notifications, audit entries, and integration events wired and idempotent
- English and Arabic with RTL complete; PDFs verified in both languages
- Import and export available where the module holds bulk data
- Tests pass; license scan passes; demo data exists; documentation updated

### Rules

- No placeholder markers, no "implement later" comments, no mock data in finished modules, no silently skipped requirements
- If a requirement cannot be met with an allowed license, stop and present the options; never add a paid or restricted dependency silently
- Keep API, web, and mobile consistent in naming, rules, and behavior
- State assumptions and trade-offs explicitly; record significant decisions as ADRs
- When this brief is ambiguous or contradictory, ask before building
- Prefer the simplest design that satisfies the requirement. Every abstraction must justify itself.

---

## 27. Decisions to Confirm Before Starting

1. .NET 10 (the default in this brief) or .NET 8. Choosing .NET 8 triggers the fallback list in Section 19
2. SaaS multi-tenant, on-premises, or both
3. Languages beyond English and Arabic
4. Target countries (affects tax, e-invoicing, regulatory reports, and data residency)
5. Which Tier 2 modules matter most to the first customers
6. Whether local AI hardware will be available, or AI ships later
7. Wolverine as the messaging and mediator library, or a thinner in-house layer over `RabbitMQ.Client`
8. The service boundaries in Section 7.2, and which services may start merged and split later
9. ~~Whether the seeded `admin` account is also created per tenant.~~ **Settled in this version:** the seeded account is the platform super administrator only. A tenant's first school administrator always arrives through a single-use invitation, which Section 10.2 already required. ADR-0003
10. Pricing model and plan limits for the SaaS layer, or whether the first release is sold per installation
11. Which regional plug-ins are needed first (e-invoicing, ministry reports, payment gateway, SMS provider)
12. Whether nursery and kindergarten features are needed in the first release
13. Trademark, domain, and app-store clearance for the name **Nibras** in the target countries, and the final taglines

---

## 28. Delivery Plan

Durations are ranges for a small, experienced team working continuously. They are estimates, not commitments. The ranges for phases 1 to 6 are derived from the slice-days in `docs/plan/34-work-breakdown.md` by `tools/plan-build/schedule-34.mjs` for the team in Section 29 (five to eight builders, with a 1.3 overhead factor), not estimated separately; phase 6 also has a calendar floor set by the penetration test, its retest window and the restore drill. The roadmap in `docs/plan/17-roadmap.md` shows the derivation and the requirement identifiers each phase closes. A team outside Section 29's shape changes the builders input, and the ranges are recomputed, never adjusted by hand.

| Phase | Goal | Services touched | Range | Exit criteria |
|---|---|---|---|---|
| **0 Plan** | The documents in `docs/plan/`, approved group by group | none | 3 to 5 weeks | Every group scores 4 or better on the rubric; `kit-lint` clean; Section 27 decisions signed off |
| **1 Foundation** | Building blocks, service template, pipeline, Gateway, Identity, Platform, Notification, Audit, design system, demo tenant | 6 | 14 to 22 weeks | A person can sign in, a tenant can be provisioned by the saga, a notification arrives, an audit entry is written, all proven by tests that ran |
| **2 The school year loop** | School, Scheduling, Attendance, Academics, Assessment, and the web and mobile screens for them | 5 | 14 to 21 weeks | A class is taught, attended, graded and reported end to end, in both languages, on web and phone. **This is the MVP cut line** |
| **3 Money and paperwork** | Finance, Requests, Communication, Documents | 4 | 11 to 17 weeks | A fee is invoiced, chased and paid; a request is approved and takes effect; a certificate is issued and verifies by QR |
| **4 Growth** | Admissions, Behavior, Reporting, mobile parity, nursery and kindergarten | 4 | 7 to 11 weeks | An applicant becomes an enrolled student without retyping; dashboards answer the questions in Appendix D |
| **5 Extended** | Wellbeing, Hr, Operations, and the Tier 2 features the first customers asked for | 3 | 9 to 14 weeks | Each module meets the definition of done in Section 26 |
| **6 Hardening and launch** | Load, soak, chaos, restore and disaster-recovery drills, penetration test, accessibility pass, documentation, on-premises bundle | all | 6 to 8 weeks | Every quality gate in Section 24 green with evidence; penetration-test findings closed or accepted with an owner |

**Total from the start of phase 1 to launch: 61 to 93 weeks** for the team in Section 29, derived the same way. The low end assumes eight builders from the first week, the high end five.

**The MVP cut line.** The first paying school needs phases 0 to 2 plus the parts of phase 3 it uses in term one. Concretely: sign-in and roles, school setup, students and guardians, timetable, attendance, coursework, marks and report cards, announcements and messaging, notifications, the Request Center with the attendance and document request types, and the audit log. That is **42 capabilities and 33 to 50 weeks from the start of phase 1**, derived from document 34 by the same script (`docs/plan/17-roadmap.md` Section 5 lists the capabilities). Admissions, Behavior, Wellbeing, Hr, Operations and Ai are explicitly not in it. A school that needs one of them is a phase 4 or 5 customer, and saying so early is cheaper than saying it late.

**Demo milestones.** End of phase 1: provisioning and sign-in. End of phase 2: the fifteen-minute demo in Appendix O, acts one and two. End of phase 3: act three. End of phase 4: the full script on the demo tenant with one-click reset.

**Critical path.** School blocks Academics, Scheduling and Attendance, because they all keep a reference copy of its data. Assessment blocks report cards, which block the phase 2 demo. Documents blocks every certificate and PDF, so it starts in phase 3 but its PDF pipeline is built in phase 1 as part of the foundation. Identity and Platform block everything and are therefore phase 1 in full, not in part.

---

## 29. Team, Governance, and Decision Rights

**Assumed shape.** One product owner, one architect, two to four backend engineers, one to two web engineers, one mobile engineer, one quality engineer, and part-time platform engineering. A smaller team is possible and lengthens phases roughly in proportion; a larger one does not shorten phase 1, which is sequential by nature. If the team differs materially from this, revisit Section 28 before the roadmap is approved.

**Who decides what.**

| Decision | Proposes | Decides | Records |
|---|---|---|---|
| Scope, tier, priority | Architect or engineer | Product owner | `BACKLOG.md`, requirement identifiers |
| Architecture, technology, boundaries | Architect | Architect, with the product owner informed when cost or scope moves | ADR in `docs/project/DECISIONS/` |
| Anything that changes `docs/brief/` | Anyone | Product owner | ADR plus a version bump on the brief |
| Raising a performance budget | Engineer | Architect | ADR |
| Adding a dependency | Engineer | Architect, after the licence auditor | `19-dependency-and-license-inventory.md` |
| Accepting a security finding without a fix | Engineer | Product owner and architect together | `RISKS.md` with an owner and a date |
| Release go or no-go | Quality engineer | Product owner | Release notes and the gate evidence |

**Cadence.** A weekly decision review that closes open questions and approves ADRs. A demo at the end of every phase, to the product owner, on the demo tenant. A retrospective after each phase that feeds `docs/project/IDEAS.md` and the kit's own defaults.

**Code ownership.** Each service names an owner who reviews changes to its contracts and its sheet. Building blocks and `src/Contracts` are owned by the architect, because a careless change there reaches every service.

**"Approved by the product owner"** means a written note in `PROJECT_STATE.md` naming the group, the date and any conditions. A verbal yes in a meeting is a signal to write that note, not a substitute for it.

**On-call** begins at the first paying customer, not at general availability. Until then, alerts route to the team channel during working hours and the status page carries an honest support window.

---

## 30. Budget and Cost Model

Open source removes licence fees. It does not remove cost, and a brief that hides cost is not honest. Figures are filled by the product owner; the categories and the drivers are fixed here.

| Category | Driver | Notes |
|---|---|---|
| Infrastructure, scale mode | Students, peak concurrency, retention | Model cost per 1,000 students; the number comes from the phase 6 load tests, not from a guess |
| Infrastructure, single-server mode | One machine per school | The on-premises price list depends on it |
| Object storage and backups | Documents and media per student per year, times the retention schedule in Section 32 | Grows every year and never shrinks on its own |
| Push notifications | Free, but not open source | Firebase Cloud Messaging and the Apple Push Notification service, behind `IPushSender` |
| App stores | Annual Apple Developer Program fee, one-off Google Play fee, per white-label account | See Section 37 for who owns the accounts |
| **Apple build capacity** | Either hosted macOS runner minutes or one Mac used as a build host | An iOS build cannot be produced on Linux or Windows. This is a hard dependency, and it is an open question, not an assumption |
| SMS and WhatsApp | Per message, per country | Always optional, always behind an adapter, with an email and push fallback |
| Payment gateway | Per transaction | Optional; the default implementation is manual and bank transfer |
| Email at scale | Per message above the free tiers of a self-hosted relay | Plus the sending domain work in Section 38 |
| AI hardware | One capable machine per deployment that enables rung 3 | Everything degrades to rung 1 without it |
| Penetration test | Once before general availability, annually after | Section 20 |
| Standards conformance | 1EdTech membership and certification for OneRoster, LTI and QTI | Only when a customer requires the certification rather than the compatibility |
| People | The team in Section 29 | The largest line by a wide margin |

---

## 31. Service Levels

**Targets per service.** Each service publishes these as a dashboard and an alert, and the pipeline fails a release that regresses one.

| Service class | Availability | Latency | Freshness |
|---|---|---|---|
| Gateway, Identity, Platform | 99.9% | p95 under 150 ms at the edge for token validation | not applicable |
| Read-heavy services (School, Scheduling, Academics, Reporting) | 99.9% | p95 under 250 ms from the database, under 80 ms from cache | Reporting read models within 60 seconds of the event |
| Write-heavy services (Attendance, Assessment, Finance) | 99.9% | p95 under 500 ms for writes | not applicable |
| Notification | 99.9% | urgent messages dispatched within 30 seconds of the event; bulk within 15 minutes | not applicable |
| Workers | no availability target | queue depth returns to baseline within 10 minutes of a burst | not applicable |

**Error budget.** 99.9% monthly allows about 43 minutes. Spending half of it in a month freezes non-essential change for that service until the following month, and the freeze is lifted by the architect, not by the calendar.

**Severity and response.**

| Severity | Meaning | Response target | Who |
|---|---|---|---|
| Sev1 | Data loss, cross-tenant exposure, or a whole tenant down | Acknowledge 15 minutes, mitigate 4 hours | On-call plus architect; the product owner informed immediately |
| Sev2 | A core workflow broken for many users, no workaround | Acknowledge 30 minutes, mitigate 1 business day | On-call |
| Sev3 | Degraded or a workaround exists | Next business day | Service owner |
| Sev4 | Cosmetic or a single-user issue | Backlog | Service owner |

A cross-tenant exposure is always Sev1 and always triggers an incident review, even when the exposure was caught by a test rather than by a person.

**Customer-facing commitments.** Availability and support targets per plan live in the contract and on the status page. The status page is public, reports per-service state, and publishes incident notes; it is hosted outside the platform so that it survives an outage of the platform.

---

## 32. Retention and Lifecycle Schedule

Defaults below apply unless a tenant's country or contract requires otherwise, in which case the tenant's schedule overrides and the difference is recorded. Every row is enforced by a scheduled job, not by a policy document.

| Data | Default retention | Then | Legal hold |
|---|---|---|---|
| Student academic record (enrollment, results, transcripts) | 10 years after leaving | Archive read-only, then anonymize | Suspends deletion |
| Attendance records | 7 years | Delete by partition | Suspends deletion |
| Behavior incidents | Until leaving, plus 3 years | Anonymize | Suspends deletion |
| Wellbeing records (clinic, counseling, safeguarding) | Per country law; default until leaving plus 7 years | Delete; safeguarding concerns follow the local safeguarding retention rule instead | Suspends deletion, and the hold is itself logged |
| Financial documents (invoices, payments, credit notes) | 10 years | Archive read-only | Suspends deletion |
| Messages and announcements | 2 years | Delete; content flagged for safeguarding follows the wellbeing rule | Suspends deletion |
| Notification delivery log | 90 days | Delete | no |
| Audit entries | 7 years | Detach partition to cold storage | Suspends deletion |
| Application logs | 30 days | Delete | no |
| Metrics | 13 months downsampled | Delete | no |
| Traces | 7 days | Delete | no |
| Backups | 35 days point-in-time plus 12 monthly | Expire | A hold pins the relevant backup set |
| Deleted tenant | 30-day cooling-off, export available throughout | Purge, then issue a certificate of deletion | A hold blocks the purge and notifies both parties |

**Partitions and retention are the same mechanism.** Attendance, notifications, messages and audit are partitioned by month; retention detaches partitions rather than deleting rows, which is why these numbers are affordable at scale.

**Deletion and backups.** A record deleted today still exists in backups until those backups expire. The published position is exactly that: deletion is immediate in the live system, and backup copies age out within 35 days, except where a hold pins them. Promising anything else would be untrue.

---

## 33. Compliance Map

The product is designed to be adapted per country rather than certified once. This map states what is assumed, what is designed for, and what is deliberately deferred.

| Framework | Status | What the product provides |
|---|---|---|
| GDPR-style baseline | Designed to | Lawful basis and consent records, data subject access, correction, export and erasure workflows, retention schedule, processing records, sub-processor list, a completable data protection impact assessment |
| Saudi PDPL | First target | Data residency in region, Arabic privacy notices, consent versions, ZATCA e-invoicing plug-in interface |
| UAE PDPL | First target | As above, with the local e-invoicing and reporting plug-ins |
| Jordan PDPL | First target | As above, with the JoFotara plug-in interface |
| Egypt data protection law | Planned | Same mechanisms, country plug-in not built |
| COPPA, FERPA | Adaptable, not claimed | Guardian consent, no advertising, no sale of data, no behavioural tracking of students, directory-information controls. A United States launch needs a legal review, not new architecture |
| SOC 2 Type II | Post-launch roadmap | Access reviews, change management, audit trail, incident process and backup drills are built to produce the evidence; the audit itself is a business decision |
| ISO 27001 | Post-launch roadmap | Same evidence base |
| WCAG 2.2 AA | Required now | Automated and manual verification per release; an accessibility conformance statement is published per release |

**Sub-processors.** A published list per deployment naming each processor, what it processes, and where. A school can refuse a sub-processor by disabling the feature that uses it, which is the reason each one sits behind an adapter.

---

## 34. Platform Operations and Topology

**High availability by deployment mode.**

| Mode | PostgreSQL | RabbitMQ | Redis | Storage | Availability posture |
|---|---|---|---|---|---|
| Developer | one container | one container | one of each role | local disk | none; it is a laptop |
| Single server | one instance, local backups to a second disk and off-site | one node, quorum queues still enabled | `redis-cache` and `redis-state` as separate containers | local disk | Restore from backup is the recovery plan, and the school is told so in writing |
| Scale | operator-managed, one primary and two replicas across availability zones, synchronous to one | three nodes, quorum queues | Sentinel or cluster, the two roles kept separate | replicated across three volumes | Zone loss survivable; region loss is a disaster-recovery event |

**Data residency.** A region is a whole deployment. A tenant is pinned to a region at provisioning and its data never leaves: no cross-region replication of tenant data, and backups stay in region. Moving a tenant between regions is an export and import, scheduled with the school. This keeps residency simple enough to be true.

**Environments.**

| Environment | Data | Who deploys | Notes |
|---|---|---|---|
| Development | seeded demo | anyone | `aspire run` or the compose dev profile |
| Preview, per pull request | seeded demo | pipeline | Created on open, destroyed on merge; the reason reviewers can click rather than imagine |
| Test | seeded plus generated load data | pipeline | Where the generated permission and isolation suites run at size |
| Staging | production-like, anonymized nightly | pipeline | Anonymization replaces names, contacts, identifiers and media; it never copies live media |
| Production | live | pipeline, with approval | Canary or blue-green, migrations as jobs |

**Capacity starting points**, to be replaced by phase 6 load-test evidence: Gateway 3 replicas; Attendance 4 replicas at the morning peak and 2 otherwise; Notification workers scaled by KEDA between 2 and 20 on queue depth; Reporting projections 2; PgBouncer transaction pooling sized at roughly 4 server connections per service replica; attendance and audit partitioned monthly from day one rather than retrofitted.

**Calendar-aware scaling.** Warm-up and scale-up run on each tenant's own school calendar and time zone, before first period, and scale down after dismissal. A school in Riyadh and a school in Amman peak an hour apart, and treating that as one global peak wastes capacity at night and misses it in the morning.

---

## 35. Public API, Webhooks, and Integrations

**Credentials.** API keys belong to a tenant, personal access tokens belong to a user and inherit that user's permissions and data scope. Both carry explicit scopes, an expiry, a per-key rate limit and quota, and a last-used timestamp. Both can be revoked instantly, and revocation propagates with the permission version.

**Versioning and deprecation.** REST paths carry `/api/v{n}/`. A version is supported for **12 months** after its successor ships. Responses on a deprecated version carry `Deprecation` and `Sunset` headers and appear in the developer portal with the removal date. Message contracts follow the same idea: a new schema version publishes alongside the old one, and the old one is retired **two minor releases** after the last consumer stops using it, which the message catalog tracks.

**Webhook contract.** Delivery is a POST with a JSON body carrying the standard envelope. Each request is signed `X-Nibras-Signature: sha256=<hmac>` over the timestamp and body with the endpoint's secret, and carries `X-Nibras-Timestamp`; a receiver rejects anything older than five minutes to stop replay. Retries are five attempts with exponential backoff and jitter, then the endpoint is marked failing and the school is notified. Every attempt is in a delivery log that the school can inspect and replay. A new endpoint is verified with a challenge request before it receives events. Secrets rotate with an overlap window so rotation is not an outage.

**Standards, by phase.** iCal feeds and the public REST API in phase 3. OneRoster 1.2 export and LTI 1.3 as a platform in phase 4. QTI 3 import and export with the question bank in phase 5. Open Badges 3.0 with Behavior in phase 5. CASE import with curriculum mapping in phase 5. Conformance certification is a commercial decision separate from compatibility, and its fees are in Section 30.

**Enterprise identity.** OpenID Connect with Google and Microsoft in phase 1. SAML 2.0 and SCIM provisioning are Tier 2, driven by ministry and large-group demand; the Identity service is designed so that adding them is a protocol adapter and not a redesign.

---

## 36. Payments and Billing Semantics

**Card data never touches Nibras.** Payment pages are hosted fields or a redirect to the gateway. The product stores a gateway reference, the last four digits and the brand, never a full number. This keeps the platform in the smallest possible scope for card-industry compliance, and that position is stated to schools rather than implied.

**Idempotency and reconciliation.** Every payment callback carries an idempotency key and is safe to receive twice. A daily reconciliation compares gateway settlements to recorded payments and reports differences by document. Chargebacks reverse through a credit note and never by editing a posted document.

**What "active student" means for SaaS billing.** A student counted on the billing date with a status of enrolled. Applicants, withdrawn, graduated and alumni are not counted. A student who joins mid-month is prorated by day from the enrollment date; a student who leaves is counted for the month in which they leave. This definition is in the contract, because every other definition produces an argument.

**Plan changes.** An upgrade takes effect immediately and prorates by day. A downgrade takes effect at the next renewal, and limits that are already exceeded show a warning rather than deleting anything. Taxes on the platform's own invoices follow the seller's country, separately from the tax a school charges its parents.

**Dunning, and the promise not to hold data hostage.** Reminders at 7, 14 and 30 days past due. At 30 days the tenant becomes read-only. Export stays available at every stage, including read-only and deletion. A school's data is never withheld to force a payment, and that sentence is in the contract too.

---

## 37. Mobile Release Operations

**Supported platforms.** Android 8 and later, including devices without Google services. iOS 15 and later. Tablets for kiosk, bus attendant and nurse modes. Mobile web covers the parent, student, teacher and principal workspaces for anyone who will not install an app.

**Push, honestly.** Firebase Cloud Messaging on Android and the Apple Push Notification service on iOS, both behind `IPushSender`. On a device without Google services, push falls back to the in-app real-time channel while the app is open, plus email and, for urgent messages only, SMS. A Huawei adapter is a Tier 2 plug-in, built when the device share justifies it.

**Distribution.** Google Play, the App Store, AppGallery where it matters, and a signed APK channel for a white-label school that distributes internally. The shared multi-school app is published by the platform. **A white-label app is published under the school's own developer accounts**, with the platform building and submitting on the school's behalf under a written agreement. The school owns the listing, the certificates and the push credentials; the platform holds them in escrow under that agreement. Settling this before the first white-label sale avoids an ownership argument after it.

**Versions and upgrades.** A minimum supported version is configured per tenant. Below it the app blocks with a clear upgrade screen; within one minor version of it the app nags but allows. Store review time is assumed at up to three days for iOS and one for Android, so anything that must reach phones by a date is submitted a week early. A server change that breaks an older app is forbidden; the API versioning rules in Section 35 are how that is guaranteed.

**Background behaviour.** iOS does not guarantee background execution, so the app syncs on open and on silent push, and never promises background sync. Android battery optimisation is detected and the app explains, once, why exempting it helps. Neither platform is asked to do something it does not support.

---

## 38. Deliverability and Safeguarding Operations

**Email that arrives.** Each tenant sends from its own subdomain with SPF, DKIM and DMARC records generated during provisioning and verified before the first send; until verification, mail goes out from the platform domain with the school's display name. Bounces and complaints are processed automatically: a hard bounce suspends the address and raises a data-quality issue, and a complaint suppresses non-urgent mail to that address permanently. Urgent messages ignore marketing suppression but still respect a hard bounce, because a bouncing address delivers nothing either way. Every non-urgent email carries an unsubscribe route that does not disable urgent school communication.

**Safeguarding operations.** A flagged message or an anonymous report reaches the safeguarding officer within 15 minutes and is acknowledged within one working day; a concern marked urgent pages the officer and the principal. Flagged content is retained per the wellbeing rule in Section 32 and is never deleted by a routine retention job while a case is open. A legal hold freezes everything connected to a named student or case, and the hold itself is audited. Requests from law enforcement are answered by a named person against a written process, never by an engineer with database access, and are recorded. A data subject access request is acknowledged within 5 working days and answered within 30 calendar days, using the export workflow rather than a manual extract.

---

## 39. Training, Onboarding, and Support

**Getting a school live.** A pre-flight with the school covers data clean-up, the required Excel templates, and the decisions the wizard will ask for. Import runs as a dry run first, with an error report and a rollback, and only then for real. Two super-users per school complete a short certification: setup, roles and permissions, request types, the mark and report-card cycle, and the audit viewer. Go-live is a scheduled day with someone available, not an email.

**Support tiers.**

| Plan | Hours | First response | Resolution target for Sev2 |
|---|---|---|---|
| Standard | Business hours, school time zone | 1 business day | 5 business days |
| Professional | Business hours, extended | 4 business hours | 2 business days |
| Enterprise or on-premises | Extended, with a named contact | 1 business hour | 1 business day |

Escalation runs support, then service owner, then architect, then product owner, with the timings above. Every school knows which tier it has, because the tier is on the status page and in the product.

---

## 40. Risk Register

The full register with review dates lives in `docs/project/RISKS.md`. These thirteen are the ones that shape the plan.

| # | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| 1 | A dependency changes licence mid-project | Medium | High | Pinned versions, licence scan in the pipeline, every external dependency behind an interface, Section 6.4 exception list |
| 2 | The timetable solver produces schedules a school will not accept | Medium | High | Constraint set reviewed with a real school in phase 2, manual refinement with live conflict detection always available, quality score shown |
| 3 | Arabic PDF shaping regresses unnoticed | Medium | High | Fonts bundled with the renderer, bilingual snapshot tests against committed baselines |
| 4 | Offline sync produces conflicts nobody can explain | High | High | Per-entity conflict rules in Appendix M, visible sync state, conflict banner with both values, tests per rule |
| 5 | Twenty services overwhelm a small operations team | High | High | One service template, one pipeline, one dashboard set, the merge option in Appendix L, self-healing operations |
| 6 | The product name is not clear for trademark or app stores | Medium | Medium | Name held in one configuration value and one token file; clearance is an open question with an owner |
| 7 | A .NET 10 library the plan depends on is not ready | Low | Medium | Fallback list in Section 19; every choice has a named alternative |
| 8 | Wolverine proves unsuitable for the messaging load | Low | High | Messaging isolated in one building block; Rebus and a thin client layer are the named alternatives; contract tests are independent of the library |
| 9 | Data residency multiplies infrastructure cost per region | Medium | Medium | Region is a whole deployment; cost per region modelled before a region is promised |
| 10 | The penetration test finds a tenancy defect late | Low | Severe | Tenant isolation suite on every build from phase 1, so a late finding would be a gap in the suite and is treated as such |
| 11 | No Apple build capacity when iOS is due | Medium | High | Named as an open question and a budget line before phase 2; Android and mobile web ship independently of it |
| 12 | Translation quality in Arabic undermines credibility | Medium | High | A fluent reviewer owns the Arabic string set, a terminology glossary is versioned, and the missing-translation report fails the build |
| 13 | PowerSchool's Middle East and Africa edition, fully available in Arabic with right-to-left orientation and Saudi and Emirati references, is chosen on brand and narrows the Arabic-first difference to parity | Medium | High | Owned by the product owner. The claim is sold as bilingual data, not a translated interface: bilingual records, Arabic-aware search, Hijri display and amounts in words shown in the demo, with the sixty-second proofs for features 26, 27, 29, 31 and 32 that PowerSchool does not document; the unverified PowerSchool cells in `docs/plan/02-competitive-gap-analysis.md` are resolved by trial or sales conversation before general availability |
