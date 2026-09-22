# 04. Architecture Overview

> Plan document 04 of 34. Group B. This document is the map: what surrounds the platform, what runs inside it, how the bounded contexts relate, how it is deployed in each of the three modes, what it is built from, and which decisions shaped it. Names come from Appendix L, the diagrams refine reference architecture Section 9, and the numbers come from master brief Sections 19, 21 and 31. The service-by-service detail is in `05-service-catalog.md` and `06-services/`.

**One sentence.** Nibras is a bilingual, multi-tenant school platform built as twenty data-owning services behind one Gateway and two backends-for-frontends, integrating through RabbitMQ events with at most one synchronous hop, each service owning one PostgreSQL database, deployable from one laptop command to a Kubernetes cluster with the same code and the same tenancy rules.

---

## 1. System context

The personas are those of master brief Section 5. Every external system sits behind an adapter interface so that a school can refuse a sub-processor by disabling the feature that uses it (master brief Sections 6.3 and 33).

```mermaid
flowchart LR
    subgraph People["People, master brief Section 5"]
        P1["School owner or director"]
        P2["Principal"]
        P3["Academic coordinator"]
        P4["Teacher and homeroom teacher"]
        P5["Registrar"]
        P6["Accountant"]
        P7["Parent"]
        P8["Student"]
        P9["Nurse and counselor"]
        P10["Platform operator"]
        P11["Partner developer"]
    end
    NIB["Nibras platform: Angular web, Flutter mobile, public API"]
    subgraph External["External systems, each behind an adapter"]
        X1["Push providers: FCM and APNs, behind IPushSender"]
        X2["SMS and WhatsApp providers"]
        X3["Payment gateway, behind IPaymentGateway"]
        X4["E-invoicing plug-ins: ZATCA, JoFotara"]
        X5["Identity providers: OpenID Connect with Google and Microsoft; SAML 2.0 as Tier 2"]
        X6["Ministry and regulatory reporting systems"]
        X7["LTI 1.3 tools and OneRoster consumers"]
        X8["Email relay over SMTP"]
        X9["Online class hosts: Jitsi Meet or LiveKit"]
        X10["Map tiles: OpenStreetMap"]
        X11["Legacy data: Excel and previous systems"]
        X12["Webhook receivers"]
    end
    P1 & P2 & P3 & P4 & P5 & P6 -->|"use the web and mobile workspaces"| NIB
    P7 & P8 & P9 & P10 -->|"use the web and mobile workspaces"| NIB
    P11 -->|"calls the public API through the developer portal"| NIB
    NIB -->|"sends push"| X1
    NIB -->|"sends SMS, urgent lane only by default"| X2
    NIB -->|"redirects to hosted payment pages, receives callbacks"| X3
    NIB -->|"submits invoices through the country plug-in"| X4
    X5 -->|"authenticates staff and guardians"| NIB
    NIB -->|"exports regulatory reports through the country plug-in"| X6
    NIB -->|"launches tools and exports rosters"| X7
    NIB -->|"sends email"| X8
    NIB -->|"embeds a class"| X9
    NIB -->|"loads tiles for transport"| X10
    X11 -->|"imported through Documents jobs"| NIB
    NIB -->|"delivers signed webhooks"| X12
```

| External system | Adapter interface | Owning service | Default implementation | Cost note (master brief Section 6.3) |
|---|---|---|---|---|
| Push providers | `IPushSender` | Notification | Firebase Cloud Messaging, Apple Push Notification service | Free of charge, not open source; the one accepted exception |
| SMS and WhatsApp | channel adapter | Notification | none; email and push fallback | Charged per message by carriers; SMS stays optional |
| Payment gateway | `IPaymentGateway` | Finance | manual and bank transfer | Charged per transaction; card data never touches Nibras (Section 36) |
| E-invoicing | country plug-in | Finance | interface only in Tier 1; ZATCA and JoFotara implementations per target country | Regional plug-in (Section 17) |
| Identity providers | OpenID Connect adapter; SAML 2.0 and SCIM as Tier 2 protocol adapters | Identity | Google and Microsoft in phase 1 (Section 35) | ADR-0008 |
| Ministry systems | country plug-in | Platform (Integrations) with the owning service's export | interface only | Regional plug-in (Section 17) |
| LTI 1.3 tools, OneRoster consumers | standards adapters | Platform (Integrations) | OneRoster 1.2 export and LTI 1.3 platform role in phase 4 (Section 35) | 1EdTech certification is a commercial decision (Section 30) |
| Email relay | MailKit | Notification | Mailpit in development | Delivery at scale is an operating cost |
| Online classes | embed | Academics | Jitsi Meet or LiveKit | none |
| Map tiles | Leaflet and `flutter_map` | Operations | OpenStreetMap | none |
| Legacy data | import job group | Documents | Excel and CSV mapping templates | none |
| Webhook receivers | outgoing webhook contract (Section 35) | Platform (Integrations) | signed POST with retry ladder | none |

---

## 2. Containers

Two views, because one diagram with every service and every piece of infrastructure passes forty nodes. The first shows the request path and the twenty services grouped by tier; the second shows what any service and any worker talks to.

### 2.1 Request path and services by tier

```mermaid
flowchart LR
    subgraph Clients
        WEB["Angular web, PWA"]
        MOB["Flutter mobile and kiosk"]
        API["Partner API client"]
    end
    EDGE["Edge proxy: Caddy or Traefik, TLS"]
    GW["Gateway: YARP"]
    BW["Bff.Web"]
    BM["Bff.Mobile"]
    subgraph T1["Tier 1, 16 services"]
        IDN["Identity"]
        PLT["Platform"]
        SCH["School"]
        ADM["Admissions"]
        ACA["Academics"]
        ASM["Assessment"]
        SCD["Scheduling"]
        ATT["Attendance"]
        FIN["Finance"]
        COM["Communication"]
        NOT["Notification"]
        RQS["Requests"]
        DOC["Documents"]
        BEH["Behavior"]
        RPT["Reporting"]
        AUD["Audit"]
    end
    subgraph T2["Tier 2, 4 services"]
        WEL["Wellbeing"]
        HR["Hr"]
        OPS["Operations"]
        AI["Ai"]
    end
    WEB & MOB & API --> EDGE --> GW
    GW -->|"screen-shaped calls"| BW & BM
    GW -->|"resource calls, tenant resolved, token validated"| T1
    GW -->|"resource calls"| T2
    BW & BM -->|"read models, Student 360"| RPT
    BW & BM -->|"HTTP reads, never a bypassing write"| SCH
    COM -->|"SignalR hubs"| WEB & MOB
```

### 2.2 What a service and a worker talk to

```mermaid
flowchart LR
    SVC["Service API, any of the 20"]
    WRK["Worker host, any of the 7"]
    MQ[("RabbitMQ: one topic exchange per service, quorum queues")]
    PGB["PgBouncer, transaction pooling"]
    PG[("PostgreSQL: one database per service, row-level security")]
    RC[("redis-cache: HybridCache L2, LFU eviction")]
    RS[("redis-state: backplane, rate limits, idempotency, locks, no eviction")]
    SW[("SeaweedFS: S3-compatible object storage")]
    GB["Gotenberg: HTML to PDF"]
    CV["ClamAV: upload scanning"]
    LLM["Ollama or vLLM, optional, Ai only"]
    subgraph Obs["Observability, standalone tools"]
        OT["OpenTelemetry collector"]
        PR["Prometheus"]
        GF["Grafana"]
        LK["Loki"]
        TP["Tempo"]
        GT["GlitchTip"]
    end
    SVC -->|"publishes through the outbox, consumes through the inbox"| MQ
    MQ -->|"queue per consumer per purpose, urgent and bulk lanes"| WRK
    SVC & WRK --> PGB --> PG
    SVC & WRK --> RC & RS
    WRK -->|"Documents.Worker stores files"| SW
    WRK -->|"Documents.Worker renders"| GB
    WRK -->|"Documents.Worker scans"| CV
    WRK -->|"Ai.Worker embeds and drafts"| LLM
    SVC & WRK -->|"traces, metrics, logs over OTLP"| OT
    OT --> PR & LK & TP
    PR & LK & TP --> GF
    SVC & WRK -->|"errors"| GT
```

| Container | Kind | One per | Notes |
|---|---|---|---|
| Edge proxy | Caddy or Traefik | deployment | TLS with Let's Encrypt, static assets with hashed names and long cache lifetimes, HTTP/2 and HTTP/3 |
| Gateway | YARP | deployment, 3 replicas | Token validation, tenant resolution from domain, subdomain or header, per-source and per-tenant limits, maintenance mode |
| Bff.Web, Bff.Mobile | ASP.NET Core | deployment | No business rules, no database; short-lived Redis cache |
| Service API | ASP.NET Core, Clean Architecture | service, 20 | Its own database, migrations, Dockerfile, OpenAPI, contracts, health probes |
| Worker host | .NET generic host | 7 workers | Same codebase as its service, separate image, scaled by KEDA |
| RabbitMQ | broker | deployment | Exchange `nibras.<service>`, routing key `<service>.<entity>.<event>.v<n>`, dead-letter and parking per consumer |
| PostgreSQL | database | one database per service, 20 | `svc_<service>` user, row-level security forced on every table, monthly partitions for attendance, notifications, messages and audit |
| PgBouncer | pooler | deployment | Transaction pooling; the tenant variable is set with `SET LOCAL` inside each transaction (reference architecture Section 14) |
| redis-cache, redis-state | Redis 8 or Valkey | deployment, two roles | Never mixed, so a cache flood cannot evict a lock (master brief Section 19) |
| SeaweedFS | object storage | deployment | Behind `IFileStorage`; local disk for small installs |
| Gotenberg | renderer | deployment | Bundled Inter, IBM Plex Sans Arabic and Noto Naskh fallback (ADR-0007) |
| ClamAV | scanner | deployment | Standalone GPLv2 container, socket protocol only (master brief Section 6.4) |
| Ollama or vLLM | model server | optional | Off by default; the product is complete without it |
| Observability | OpenTelemetry, Prometheus, Grafana, Loki, Tempo, GlitchTip | deployment | Correlation id from the Gateway through gRPC and RabbitMQ headers to the last consumer |

---

## 3. Bounded-context map

The relationship type on each edge uses the four names the task set fixes. *Published language* means the downstream depends on a versioned contract (`Nibras.Contracts.<Service>`) and nothing else. *Conformist* means the downstream keeps a slim copy in the upstream's shape without translating it. *Customer-supplier* means the downstream negotiates the contract, which is how sagas and command channels behave. *Anti-corruption layer* means the downstream translates before it stores, and never lets the upstream's model in.

### 3.1 Relationships, in full

| Upstream | Downstream | Relationship | Contract |
|---|---|---|---|
| Identity | every service | Published language | Token claims (user, tenant, roles, permission version); `identity.permissions.changed.v1` invalidates the permission cache |
| Platform | every service | Published language | `platform.*` tenant lifecycle, plan, flags, settings, terminology; custom-field definitions (ADR-0009) |
| School | Academics, Assessment, Scheduling, Attendance, Finance, Communication, Requests, Behavior, Wellbeing, Hr, Operations, Admissions | Conformist | `school.*` events and the `nibras.school.v1` gRPC directory; each keeps the slim copy listed in reference architecture Section 8.0 |
| School | Admissions | Customer-supplier | The "offer accepted to enrolled" saga: Admissions is the customer, School supplies the enrolment |
| Academics | Assessment | Customer-supplier | `academics.submission.graded.v1`, `academics.teaching-assignment.changed.v1`; Assessment negotiates what a graded submission carries |
| Academics | Scheduling | Customer-supplier | Teaching assignments as solver constraints |
| Scheduling | Attendance, Academics, Operations | Published language | `scheduling.timetable.published.v1` with a timetable version as the partition key |
| Requests | School, Academics, Assessment, Scheduling, Attendance, Finance, Documents, Hr, Operations, Wellbeing | Customer-supplier | The saga command channel: Requests is the customer of each owning service's effect definition; the owning service supplies the effect and decides nothing less |
| Documents | Assessment, Finance, Admissions, Requests, School, Behavior | Published language | `documents.document.generation-requested.v1` in, `documents.document.generated.v1` out; templates and verification codes |
| Finance | Assessment, Documents, Admissions, Operations | Published language | `finance.account.restricted.v1`, `finance.payment.received.v1`, `finance.invoice.issued.v1` |
| Admissions, Operations, Hr | Finance | Customer-supplier | They request charges (deposits, fines, activity fees, payroll inputs); Finance supplies the posted document |
| Hr | School, Scheduling, Attendance | Customer-supplier | `hr.staff.hired.v1`, `hr.leave.approved.v1`; School and Scheduling supply the record and the cover |
| every service | Notification | Published language | `notification.notification.requested.v1` plus the template catalog in Appendix C; Notification never sees a message body it did not template |
| every service | Reporting | Conformist | Projections conform to each publisher's event shape and are rebuildable from them |
| every service | Audit | Published language | `<service>.audit.recorded.v1` envelope with before and after values |
| Attendance, Behavior, Communication, Reporting | Wellbeing | Anti-corruption layer | Signals (`attendance.threshold.reached.v1`, `behavior.incident.recorded.v1`, `communication.message.reported.v1`, `reporting.early-warning.flag-raised.v1`) become referrals; no clinical content enters from outside, and only identifiers and codes leave |
| every service, the backends-for-frontends | Ai | Anti-corruption layer | Indexes only from events and authorised BFF reads; every embedding carries tenant, data scope and source version; never reads a database (master brief Section 25) |
| Identity | Gateway | Conformist | The Gateway validates tokens exactly as Identity issues them and adds nothing |
| every service, Reporting | Bff.Web, Bff.Mobile | Conformist | The backends-for-frontends compose service APIs and read models into screen shapes without owning a model |
| External standards (OneRoster, LTI, QTI, Open Badges, CASE) | Platform, Academics, Behavior | Anti-corruption layer | Each standard is mapped at the edge by its owning service (ADR-0012); the internal model never adopts the standard's shape |
| External providers (push, SMS, payment, e-invoicing, identity) | Notification, Finance, Identity | Anti-corruption layer | Adapter interfaces of master brief Section 6.3; one adapter replaced per licence or provider change |

### 3.2 Core and academic contexts

```mermaid
flowchart LR
    IDN["Identity"]
    PLT["Platform"]
    SCH["School"]
    ADM["Admissions"]
    ACA["Academics"]
    ASM["Assessment"]
    SCD["Scheduling"]
    ATT["Attendance"]
    FIN["Finance"]
    RQS["Requests"]
    DOC["Documents"]
    IDN -->|"published language: claims, permission version"| SCH & RQS & FIN
    PLT -->|"published language: tenant, settings, flags"| SCH & ADM & FIN
    SCH -->|"conformist: slim copies"| ACA & ASM & SCD & ATT & FIN
    ADM -->|"customer-supplier: enrolment saga"| SCH
    ACA -->|"customer-supplier: graded submissions"| ASM
    ACA -->|"customer-supplier: teaching assignments"| SCD
    SCD -->|"published language: timetable version"| ATT & ACA
    RQS -->|"customer-supplier: saga command"| SCH & ATT & FIN & ASM
    DOC -->|"published language: generation contract"| ASM & FIN & ADM
    FIN -->|"published language: restrictions"| ASM
```

### 3.3 Supporting, sensitive and edge contexts

```mermaid
flowchart LR
    ALL["Every data-owning service"]
    GW["Gateway"]
    BFF["Bff.Web and Bff.Mobile"]
    IDN["Identity"]
    PLT["Platform Integrations"]
    NOT["Notification"]
    RPT["Reporting"]
    AUD["Audit"]
    WEL["Wellbeing"]
    BEH["Behavior"]
    COM["Communication"]
    ATT["Attendance"]
    AI["Ai"]
    EXT["External standards and providers"]
    IDN -->|"conformist: token as issued"| GW
    ALL -->|"conformist: screen composition"| BFF
    RPT -->|"conformist: read models"| BFF
    ALL -->|"published language: notification.requested, Appendix C"| NOT
    ALL -->|"conformist: projections"| RPT
    ALL -->|"published language: audit.recorded"| AUD
    ATT & BEH & COM & RPT -->|"anti-corruption layer: signals become referrals"| WEL
    WEL -->|"published language: identifiers and codes only"| RPT
    ALL & BFF -->|"anti-corruption layer: events and authorised reads only"| AI
    EXT -->|"anti-corruption layer: OneRoster, LTI, webhooks, providers"| PLT
```

---

## 4. Runtime topology

Reference architecture Section 9.1, refined with canonical names and the two Redis roles.

```mermaid
flowchart LR
    subgraph Clients
        WEB["Angular web"]
        MOB["Flutter mobile"]
    end
    EDGE["Edge proxy, TLS"]
    GW["Gateway, YARP"]
    BW["Bff.Web"]
    BM["Bff.Mobile"]
    IDN["Identity"]
    SCH["School"]
    ATT["Attendance"]
    FIN["Finance"]
    COM["Communication, SignalR hubs"]
    OTH["The other 15 services"]
    MQ[("RabbitMQ")]
    NW["Notification.Worker"]
    DW["Documents.Worker"]
    RP["Reporting.Projections"]
    OW["Assessment, Scheduling, Finance and Ai workers"]
    AUD["Audit"]
    RPT["Reporting read models"]
    PG[("PostgreSQL, one database per service, through PgBouncer")]
    RC[("redis-cache")]
    RS[("redis-state")]
    SW[("SeaweedFS")]
    GB["Gotenberg"]
    WEB & MOB --> EDGE --> GW
    GW --> BW & BM
    GW --> IDN & SCH & ATT & FIN & COM & OTH
    BW & BM --> RPT
    ATT & FIN & OTH -->|"gRPC, one hop"| SCH
    IDN & SCH & ATT & FIN & COM & OTH <-->|"outbox and inbox"| MQ
    MQ --> NW & DW & RP & OW & AUD
    RP --> RPT
    IDN & SCH & ATT & FIN & COM & OTH & NW & DW & OW --> PG
    IDN & SCH & ATT & FIN & COM & OTH & BW & BM --> RC
    GW & COM & IDN --> RS
    DW --> SW & GB
    COM -->|"SignalR"| WEB & MOB
```

| Path | Route | Rule that governs it |
|---|---|---|
| Request | client, edge proxy, Gateway, service or backend-for-frontend | The Gateway resolves the tenant; a request without a tenant is rejected except on platform endpoints (master brief Section 7.4) |
| Screen composition | Gateway, Bff.Web or Bff.Mobile, several services over HTTP and the Reporting read models | Cross-service screens are served by the backends-for-frontends or Reporting, never by the browser calling ten services (Section 7.3) |
| Synchronous query | service, gRPC, School or Identity | Maximum one hop, with timeouts, retries with jitter, circuit breaker and a cached fallback (Section 7.3) |
| Event | service outbox, RabbitMQ exchange, consumer inbox | At-least-once delivery; every handler safe to run twice; ordered per subject through a `partitionKey` (Section 8) |
| Job | API creates the job record, publishes to the bulk lane, worker runs it, progress over SignalR, result record with output | Long jobs report progress, can be cancelled, and leave an error report (Section 8, item 10) |
| Real time | Communication hubs on the `redis-state` backplane | Messaging, notifications, job progress and live permission refresh; no logout needed after a role change (Section 7.5) |
| File | Documents API, signed URL of 5 minutes, SeaweedFS | Files are scanned by ClamAV before any other job touches them and served only through short-lived signed URLs (Section 20) |

---

## 5. Deployment modes

Master brief Section 7.7 defines three modes and Section 34 gives each its high-availability posture. The code is identical in all three; only the values differ.

| Mode | Started by | PostgreSQL | RabbitMQ | Redis | Storage | Availability posture | Who it is for |
|---|---|---|---|---|---|---|---|
| Developer | `aspire run` or the compose dev profile | one container | one container | one of each role | local disk | none; it is a laptop | Every developer, on Windows 11, Ubuntu 22.04 or later, or macOS 14 or later |
| Single server | the compose `single-server` profile on one Linux host, or the virtual machine appliance on a Windows host (ADR-0016) | one instance, local backups to a second disk and off-site | one node, quorum queues still enabled | `redis-cache` and `redis-state` as separate containers | local disk | Restore from backup is the recovery plan, and the school is told so in writing | A single small school or an on-premises install; no Kubernetes cluster required |
| Scale | Kubernetes with the umbrella Helm chart, Argo CD or Flux, OpenTofu for the infrastructure | operator-managed, one primary and two replicas across zones, synchronous to one | three nodes, quorum queues | Sentinel or cluster, the two roles kept separate | replicated across three volumes | Zone loss survivable; region loss is a disaster-recovery event | The SaaS deployment and large private clouds |

### 5.1 Developer

```mermaid
flowchart TD
    DEV["Developer workstation: Windows 11, Ubuntu or macOS"]
    CMD["aspire run, or the compose dev profile"]
    APP["Aspire AppHost"]
    DASH["Aspire dashboard: logs, traces, metrics"]
    subgraph Proc["Processes started by the AppHost"]
        GW["Gateway and both backends-for-frontends"]
        SVCS["Every service API and worker"]
    end
    subgraph Ctr["Containers: Docker Engine under WSL2 or Podman on Windows, Docker Engine or Podman elsewhere"]
        PG[("postgres and pgbouncer")]
        MQ[("rabbitmq")]
        RD[("redis-cache and redis-state")]
        SW[("seaweedfs")]
        GB["gotenberg"]
        CV["clamav"]
        MP["mailpit"]
    end
    DEV --> CMD --> APP
    APP --> DASH
    APP --> GW & SVCS
    SVCS --> PG & MQ & RD & SW & GB & CV & MP
```

Seeded demo tenants, the `dev-smoke` job proving the one-command start on a Windows runner and a Linux runner, and Docker Desktop not required (reference architecture Section 17).

### 5.2 Single server

```mermaid
flowchart TD
    HOST["One Linux host, or the Linux appliance VM on Hyper-V or VMware"]
    COMP["docker compose, profile single-server"]
    EDGE["Edge proxy: Caddy with Let's Encrypt"]
    subgraph Svc["Service containers"]
        GW["gateway, bff-web, bff-mobile"]
        APIS["20 service APIs"]
        WRK["7 workers"]
    end
    subgraph Infra["Infrastructure containers"]
        PG[("postgres, pgbouncer")]
        MQ[("rabbitmq, one node, quorum queues")]
        RC[("redis-cache")]
        RS[("redis-state")]
        SW[("seaweedfs or local disk")]
        GB["gotenberg"]
        CV["clamav"]
    end
    OBS["Observability profile, optional: prometheus, grafana, loki, tempo"]
    BK["pgBackRest: second disk plus off-site"]
    UPG["Upgrade script with pre-upgrade check and documented rollback"]
    HOST --> COMP --> EDGE --> GW --> APIS
    APIS --> WRK
    APIS & WRK --> PG & MQ & RC & RS & SW & GB & CV
    COMP --> OBS
    PG --> BK
    HOST --> UPG
```

The appliance is built from `deploy/onprem/` and carries the compose bundle, a backup volume and the upgrade script; native Windows Server hosting is not supported and the sales material says so (ADR-0016).

### 5.3 Scale

```mermaid
flowchart TD
    TOFU["OpenTofu: networks, cluster, databases, storage, DNS"]
    GIT["Argo CD or Flux: umbrella chart per environment"]
    subgraph NS["Namespace nibras-prod"]
        ING["Ingress edge proxy, TLS"]
        GW["Gateway, HPA on requests"]
        BFF["Bff.Web and Bff.Mobile, HPA"]
        APIS["20 service APIs, HPA on CPU and latency, PodDisruptionBudget, NetworkPolicy"]
        WRK["7 workers, KEDA on queue depth"]
        MIG["Migration jobs, expand and contract (ADR-0010)"]
        PGB["PgBouncer"]
    end
    subgraph Ops["Operator-managed infrastructure"]
        PG[("PostgreSQL: primary plus two replicas across zones, pgBackRest point-in-time recovery")]
        MQ[("RabbitMQ: three nodes, quorum queues")]
        RD[("Redis Sentinel or cluster: cache and state kept separate")]
        SW[("SeaweedFS: three replicated volumes")]
        GB["Gotenberg and ClamAV"]
    end
    SEC["OpenBao secrets, rotation per reference architecture Section 12"]
    OBS["Prometheus, Grafana, Loki, Tempo, GlitchTip"]
    TOFU --> NS & Ops
    GIT --> NS
    ING --> GW --> BFF & APIS
    APIS --> WRK
    MIG --> PGB
    APIS & WRK --> PGB --> PG
    APIS & WRK --> MQ & RD & SW & GB
    NS --> SEC & OBS
```

Rolling or blue-green releases, migrations as jobs, per-service pipelines triggered by path filters, and image, dependency and licence scans on every build (master brief Section 7.7). Calendar-aware scaling warms caches and scales up before first period in each tenant's own time zone (Section 34).

---

## 6. Tenant isolation tiers

Reference architecture Section 14: three tiers, one codebase, because tenancy is enforced in the same building block regardless of tier.

| Tier | What it is | Who gets it | How the code differs | What the plan adds |
|---|---|---|---|---|
| Shared | Every tenant in the same database per service, isolated by `tenant_id`, EF Core named filters and forced row-level security | default | no difference | The pooled-connection test: a connection returned to PgBouncer cannot read the previous tenant's rows, because the variable is set with `SET LOCAL` inside each transaction |
| Dedicated database | One tenant's own database per service, same schema | a plan option, or a regulator's requirement | Platform resolves a different connection string; nothing else changes | Migration between tiers in five steps: provision and migrate, `COPY` per service while the tenant keeps working, re-copy the delta and freeze briefly, final delta and switch the connection string with cache invalidation, unfreeze and keep the old rows for the cooling-off period |
| Dedicated deployment | The whole umbrella chart in its own namespace or cluster | on-premises and private cloud | a different values file | The single-server mode and the appliance are this tier at its smallest |

Two rules that hold in every tier: a school group is one tenant with several campuses, so no cross-tenant query exists anywhere (ADR-0011); and a tenant is pinned to a region at provisioning, so its data and its backups never leave that region (master brief Section 34).

---

## 7. Technology list

Every row is marked "verified from Section 6.2; exact versions pinned in document 19". Section 6.2 records licences at the level of the product family; document 19 verifies the licence of the exact version installed, from the source, and marks anything it cannot verify as unverified. A row whose licence cell is blank in Section 6.2 is marked unverified here rather than guessed.

| Concern | Technology | Licence (Section 6.2) | Verification |
|---|---|---|---|
| Runtime | .NET 10 (LTS), ASP.NET Core, EF Core 10 with Npgsql | MIT / PostgreSQL | verified from Section 6.2; exact versions pinned in document 19 |
| Database | PostgreSQL 16 or later with `pgvector`, `pg_trgm`, full-text search | PostgreSQL | verified from Section 6.2; exact versions pinned in document 19 |
| Bulk operations | Npgsql binary COPY, EF Core `ExecuteUpdate` and `ExecuteDelete` | part of the rows above | verified from Section 6.2; exact versions pinned in document 19 |
| Message broker | RabbitMQ 4 | MPL-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Mediator, bus, outbox, sagas | Wolverine (ADR-0004) | MIT | verified from Section 6.2; exact versions pinned in document 19 |
| Object mapping | Mapperly, or explicit manual mapping | Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Validation | FluentValidation | Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Authentication | ASP.NET Core Identity with OpenIddict (ADR-0008); Keycloak as the SSO-heavy alternative | Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Cache, backplane, limits, locks | Redis 8 standalone under AGPLv3 (ADR-0005), StackExchange.Redis, `HybridCache` (ADR-0006); Valkey as fallback | AGPLv3 standalone / MIT | verified from Section 6.2; exact versions pinned in document 19 |
| File storage | SeaweedFS behind `IFileStorage`; local disk for small installs | Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Scheduled jobs | Quartz.NET, clustered, PostgreSQL job store | Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Real time | ASP.NET Core SignalR | MIT | verified from Section 6.2; exact versions pinned in document 19 |
| PDF | Gotenberg (ADR-0007); PDFsharp or MigraDoc for simple documents | MIT | verified from Section 6.2; exact versions pinned in document 19 |
| Excel and CSV | ClosedXML, CsvHelper | MIT / Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Image processing | SkiaSharp or Magick.NET | MIT / Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| QR and barcodes | QRCoder, ZXing.Net | MIT / Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Multi-tenancy | Finbuckle.MultiTenant, EF Core global query filters, PostgreSQL row-level security | Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Resilience | Microsoft.Extensions.Resilience | MIT | verified from Section 6.2; exact versions pinned in document 19 |
| Feature flags | Microsoft.FeatureManagement | MIT | verified from Section 6.2; exact versions pinned in document 19 |
| API documentation | Microsoft.AspNetCore.OpenApi with Scalar UI | MIT | verified from Section 6.2; exact versions pinned in document 19 |
| API gateway | YARP | MIT | verified from Section 6.2; exact versions pinned in document 19 |
| Edge proxy and TLS | Caddy or Traefik with Let's Encrypt | Apache-2.0 / MIT | verified from Section 6.2; exact versions pinned in document 19 |
| Internal synchronous calls | gRPC (`Grpc.AspNetCore`), Microsoft.Extensions.ServiceDiscovery | Apache-2.0 / MIT | verified from Section 6.2; exact versions pinned in document 19 |
| Local orchestration | Aspire (AppHost, service defaults, dashboard) | MIT | verified from Section 6.2; exact versions pinned in document 19 |
| Autoscaling | Kubernetes HPA plus KEDA | Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Packaging | Helm, one chart per service plus an umbrella chart | Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Infrastructure as code and GitOps | OpenTofu, Ansible (GPL tool, build time only), Argo CD or Flux | MPL-2.0 / GPL tool / Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Supply chain | CycloneDX SBOM, cosign, Renovate (AGPL tool, CI only) | Apache-2.0 / AGPL tool | verified from Section 6.2; exact versions pinned in document 19 |
| Accessibility and visual tests | axe-core, Playwright snapshots | MPL-2.0 / Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Mutation testing | Stryker.NET | Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Error tracking | GlitchTip, self-hosted | MIT | verified from Section 6.2; exact versions pinned in document 19 |
| Product analytics, staff usage only | Matomo or PostHog, self-hosted | unverified: Section 6.2 requires the licence of the edition used to be verified | unverified until document 19 |
| Connection pooling | PgBouncer | ISC | verified from Section 6.2; exact versions pinned in document 19 |
| Contract testing | PactNet | MIT | verified from Section 6.2; exact versions pinned in document 19 |
| Service mesh | none at the start; Istio or Cilium only if proven necessary | Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Email | MailKit; Mailpit for development | MIT | verified from Section 6.2; exact versions pinned in document 19 |
| Search | PostgreSQL full-text search with Arabic normalization; Meilisearch only if proven necessary | unverified: Section 6.2 gives no licence for Meilisearch | unverified until document 19 |
| Timetable solver | Google OR-Tools, CP-SAT, official .NET bindings | Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Predictive models | ML.NET | MIT | verified from Section 6.2; exact versions pinned in document 19 |
| Generative AI | Microsoft.Extensions.AI, Ollama or vLLM, Semantic Kernel if needed, `pgvector` | MIT / Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| OCR | Tesseract, Arabic and English | Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Online classes | Jitsi Meet embed or LiveKit | Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Maps | OpenStreetMap, Leaflet, `flutter_map` | BSD / ODbL | verified from Section 6.2; exact versions pinned in document 19 |
| Logging | Serilog to console and OpenTelemetry | Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Observability | OpenTelemetry, Prometheus, Grafana, Loki, Tempo or Jaeger, as standalone tools | Apache-2.0 / AGPL tools | verified from Section 6.2; exact versions pinned in document 19 |
| Upload scanning | ClamAV, standalone container | GPLv2 standalone (Section 6.4) | verified from Section 6.2; exact versions pinned in document 19 |
| Secrets | OpenBao, or Kubernetes and Docker secrets | MPL-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Backups | pgBackRest with point-in-time recovery | MIT | verified from Section 6.2; exact versions pinned in document 19 |
| Optional BI | Apache Superset | Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Containers | Docker Engine or Podman | Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| CI, CD and security scanning | GitHub Actions free tier or Forgejo with Woodpecker; Trivy, Gitleaks, OWASP ZAP, SonarQube Community | unverified: Section 6.2 lists no licence for this row | unverified until document 19 |
| Web | Angular latest stable at project start, zoneless, signals, standalone; Angular Material and CDK; native CSS animations with `animate.enter` and `animate.leave` plus View Transitions; Tailwind CSS; NgRx SignalStore; Transloco; Apache ECharts via `ngx-echarts`; Tiptap or Quill; generated OpenAPI client; Playwright | MIT | verified from Section 6.2; exact versions pinned in document 19 |
| Mobile | Flutter latest stable at project start; Riverpod or `flutter_bloc`; `go_router`; Dio; Drift; `flutter_secure_storage`; `local_auth`; `fl_chart`; `mobile_scanner`; `flutter_map`; `flutter_animate`; `lottie` | BSD / MIT | verified from Section 6.2; exact versions pinned in document 19 |
| Fonts and icons | Inter, IBM Plex Sans Arabic; Lucide or Material Symbols | SIL Open Font License; ISC / Apache-2.0 | verified from Section 6.2; exact versions pinned in document 19 |
| Testing | xUnit, NSubstitute, Shouldly or AwesomeAssertions, Bogus, Testcontainers, BenchmarkDotNet, NetArchTest, Playwright, k6 | as listed per package in document 19 | verified from Section 6.2; exact versions pinned in document 19 |
| Tooling | Node 22 LTS or later, one implementation with `.ps1` and `.sh` wrappers (ADR-0017) | MIT | verified from Section 6.2; exact versions pinned in document 19 |

**Explicitly excluded** by Section 6.2, so that nobody adds them by habit: MassTransit v9 and later, MediatR current versions, NServiceBus, AutoMapper current versions, Duende IdentityServer, MinIO, QuestPDF, iText, EPPlus, ImageSharp v3 and later, FluentAssertions v8 and later, Terraform, HashiCorp Vault, Seq, Docker Desktop, AG Grid Enterprise, FullCalendar premium plug-ins, Syncfusion in any form, and any hard dependency on a paid AI API.

---

## 8. Key decisions

Every architecture-shaping decision has an ADR in `docs/project/DECISIONS/`. All are proposed and awaiting product owner confirmation at the date of this document; `29-adr-index.md` tracks status.

| ADR | Decision | Alternatives considered | Why the alternative lost |
|---|---|---|---|
| ADR-0001 | .NET 10 (LTS) rather than .NET 8 | .NET 8, with the fallback list in master brief Section 19 | Support for .NET 8 ends on 10 November 2026; a product holding children's data should not start on a runtime about to lose security patches (Section 3) |
| ADR-0002 | Twenty data-owning services, Assessment separate from Academics and Behavior separate from Wellbeing | Keep the estimate and let the catalog drift; merge the two pairs permanently; split Operations into six | Drift produced three different counts; the merges couple two scaling profiles and two sensitivity models; six sub-domains that deploy together are one service with six schemas |
| ADR-0003 | The seeded administrator is the platform super administrator only | A seeded account per tenant | A known credential per tenant is a standing credential-stuffing target and erases the invitation trail |
| ADR-0004 | Wolverine for mediator, transport, outbox and sagas, behind `Nibras.BuildingBlocks.Messaging` | Rebus plus a hand-written outbox; `RabbitMQ.Client` with a thin in-house bus | More code to own, and the outbox and saga machinery would be ours to get right |
| ADR-0005 | Redis 8 as unmodified standalone infrastructure under AGPLv3, Valkey as the drop-in fallback | Valkey as the default; avoid Redis entirely | Valkey is equally acceptable and kept ready; avoiding Redis would need a different answer for cache, backplane, limits and idempotency each |
| ADR-0006 | All caching through `HybridCache` behind `Nibras.BuildingBlocks.Caching`, tenant on every key and tag | Direct StackExchange.Redis calls per service; `IDistributedCache` only | Twenty chances to forget the tenant prefix; no stampede protection or tag invalidation for the morning peak |
| ADR-0007 | PDFs rendered from HTML by Gotenberg, fonts bundled, bilingual snapshot tests | A .NET PDF library; a self-operated headless browser | Licence and weaker Arabic shaping; Gotenberg is the maintained headless browser already |
| ADR-0008 | ASP.NET Core Identity with OpenIddict, in-process in Identity | Keycloak | A second user store to reconcile, and join and delegation flows would straddle two systems; Keycloak stays the recommended enterprise option |
| ADR-0009 | Platform owns settings, terminology and custom-field definitions; each service owns the values on its entities | School owns everything; each service owns its own definitions | Settings apply to every service; an administrator would configure the same terminology in twenty places |
| ADR-0010 | Rollback means redeploying the previous image; the schema is never rolled back; migrations are expand and contract | Down migrations; blue-green with two schemas | A down migration destroys data written since the deployment; two schemas cost more than the change sizes justify |
| ADR-0011 | A school group is one tenant with several campuses | Several tenants with a group layer | Every query would need a second scope above the tenant and the isolation guarantee would become conditional |
| ADR-0012 | Public API, webhooks and standards live in Platform as the Integrations capability; Requests owns the `Task` aggregate | A separate Integrations service | It would own no data beyond keys and endpoints, and Section 7.1 requires a written reason for a new service |
| ADR-0013 | Appendices split into one file each | Keep one file; split by theme | Unreadable and unmergeable at the v9 size; every existing reference is by letter |
| ADR-0014 | Every requirement, rule and workflow is proven by an identified test case (`TC-<AREA>-<NNN>`) | Rely on coverage percentages | Coverage measures lines executed, not behaviour proven |
| ADR-0015 | Every intelligent feature declares an assist rung and an autonomy level, and explains itself through the Because panel | One combined score; no explanation requirement | A combined score hides a simple rule acting by itself; master brief Section 4 makes explainability a product test |
| ADR-0016 | Servers are Linux only; a Windows host runs the Linux virtual machine appliance | Support Windows Server natively; refuse Windows-host customers | The dependency set makes native Windows unsupportable; the appliance costs little and opens a real segment |
| ADR-0017 | Kit tooling is one Node implementation with PowerShell and bash wrappers | Write each script twice; require WSL on Windows | Two implementations drift; WSL is an unreasonable requirement for a kit whose job is to be read |

Decisions this document relies on that have no ADR yet, and which Group B must either confirm as brief facts or turn into one: the two additional cycle classes in `05-service-catalog.md` Section 5 (the job reply channel and the orchestrator sagas outside Requests), and the phase placement of Ai, which follows master brief Section 27 decision 6.

---

## 9. The assist ladder

Every automated or suggested behaviour in Nibras declares an assist rung, which says what technology it needs, and an autonomy level, which says how far it acts alone, and the two are independent (master brief Section 25, ADR-0015). Rung 1 is deterministic rules and smart defaults, always on, owing the rule identifier and its inputs; rung 2 is a classical ML.NET model on ordinary hardware, on by tenant choice, owing its contributing factors and weights; rung 3 is a local open-weight language model, off by default and needing a capable machine, owing its sources and a human review step; rung 4 is an external provider behind an adapter, off by default, never required, needing explicit per-tenant consent, and owing everything rung 3 owes plus what left the school's infrastructure. Autonomy runs from 1, surfaces, through 2, suggests, and 3, drafts, to 4, acts, and autonomy 4 is never permitted over a grade, a payment or a message to a family. The product is complete and sellable at rung 1; a feature above it falls back downward to a rung 1 experience and never sideways to an error or a silent wrong answer; every automated decision a person can see shows its reasons, its rung and an override in the Because panel; every rung above 1 has a per-tenant, per-feature off switch; retrieval filters by the caller's data scope before it ranks; and the Ai service never reads another service's database, indexing only from events and authorised reads through the backends-for-frontends, which is what lets it be a Tier 2 service that the rest of the platform never depends on. The per-feature rung assignment is in Appendix W and the full design is document 25.

---

## 10. Quality attributes

Numbers are quoted from master brief Sections 19 (performance budgets), 21 (non-functional requirements) and 31 (service levels). A change that breaks a budget fails the pipeline, and raising a budget needs an ADR (Section 19).

| Attribute | Target | Source | Proven by |
|---|---|---|---|
| Availability, Gateway, Identity, Platform | 99.9% monthly; about 43 minutes of error budget | Section 31 | Per-service SLO dashboard and alert; the pipeline fails a release that regresses it |
| Availability, every other service class | 99.9% | Section 31 | As above |
| Latency, token validation at the edge | p95 under 150 ms | Section 31 | k6 scenario in Appendix N |
| Latency, read-heavy services from cache | p95 under 80 ms at the service | Sections 19 and 31 | k6, plus the command-counting interceptor in integration tests |
| Latency, read-heavy services from the database | p95 under 250 ms | Sections 19 and 31 | k6 |
| Latency, write-heavy services | p95 under 500 ms | Sections 19 and 31 | k6 |
| Freshness, Reporting read models | within 60 seconds of the event | Section 31 | Projection lag metric and alert |
| Notification dispatch | urgent within 30 seconds of the event; bulk within 15 minutes | Section 31 | Queue-age metric per lane |
| Worker recovery | queue depth back to baseline within 10 minutes of a burst | Section 31 | KEDA scaling test in the Appendix N scenarios |
| Database commands per request | 5 or fewer typical; above 10 needs an ADR | Section 19 | Command-counting interceptor fails the test |
| Single SQL command | p95 under 50 ms on demo-scale data; none above 200 ms without an ADR | Section 19 | Slow-query interceptor |
| Cache hit ratio, reference data, settings, permissions | 95% or higher during school hours | Section 19 | Redis hit-ratio dashboard; a falling ratio is an alert |
| Web vitals on a mid-range phone | LCP under 2.5 s, INP under 200 ms, CLS under 0.1 | Section 19 | Playwright with mobile budgets |
| Mobile | cold start under 3 s, 60 frames per second on a mid-range Android device | Section 19 | Device pass per release |
| Teacher marks attendance for one class | under 60 seconds end to end including network | Section 19 | Appendix Q acceptance script and the Appendix O demo |
| Scale | 500 schools, 500,000 students, 20,000 concurrent users; the 8:00 a.m. attendance peak; 800 report cards in under 10 minutes; a 10,000-row import in under 5 minutes | Section 21 | Appendix N load scenarios in phase 6 |
| Recovery | RPO 15 minutes or better, RTO 4 hours or better; point-in-time recovery | Section 21 | Quarterly restore drill (reference architecture Section 13) |
| Security | OWASP ASVS 5.0 Level 2 and MASVS 2; 2FA; lockout; rate limiting; CSP; ClamAV on uploads; column encryption for sensitive fields | Sections 20 and 21 | Generated permission and isolation suites; independent penetration test before general availability |
| Privacy | minimization, consent records, access logs for sensitive records, retention jobs, export and erasure, data residency per tenant | Section 21, Appendix J | Retention job reports in the Data Quality Center; the guardian transparency panel |
| Auditability | who, what, when, where, before and after, append-only, hash-chained | Section 21 | Nightly integrity verification publishing `audit.integrity-check.failed.v1` on failure |
| Accessibility | WCAG 2.2 AA on web and mobile | Section 21 | axe-core in CI; manual pass per release with TalkBack, VoiceOver, NVDA and Narrator |
| Severity response | Sev1 acknowledged in 15 minutes and mitigated in 4 hours; Sev2 in 30 minutes and 1 business day | Section 31 | Incident drill in phase 6; a cross-tenant exposure is always Sev1 |
| Maintainability | a new developer productive within two days | Section 21 | The `dev-smoke` job and the onboarding checklist in document 33 |

---

## How this document is verified

| Claim | Proof |
|---|---|
| Every name in every diagram is a canonical name from Appendix L | `kit-lint` canonical-name checks; Group B review reads each diagram against the Appendix L table |
| Every Mermaid block opens with a known diagram type and stays under forty nodes | `kit-lint` rule R17; the reviewer counts nodes in the two container views, which are the largest |
| The context diagram's external systems each map to an adapter interface | The table in Section 1; document 23 names the interface per integration and document 19 lists the adapter packages |
| The bounded-context relationships match the dependency matrix | Each edge in Section 3 corresponds to a row in `05-service-catalog.md` Section 3 or Section 5; `/lint-plan` cross-checks the pairs |
| The runtime topology refines reference architecture Section 9.1 without contradicting it | Group B review diff against Section 9.1; document 15 draws the same topology per environment |
| The three deployment modes agree with master brief Sections 7.7 and 34 and reference architecture Section 6 | Group B review; the compose profiles and umbrella chart in document 15 name the same containers |
| The isolation tiers and the migration steps match reference architecture Section 14 | Group B review; the pooled-connection isolation test in document 16 proves the shared tier; the tier-migration drill in document 15 proves the move |
| Every licence in Section 7 is what Section 6.2 states, and every unverified row is marked | Group B review; document 19 verifies exact versions from the source and the licence scan in CI fails on a disallowed licence |
| Every ADR in Section 8 exists with the alternatives stated | `29-adr-index.md` lists them; `/lint-plan` fails on an ADR number referenced here with no file in `docs/project/DECISIONS/` |
| Every number in Section 10 is a quotation from Sections 19, 21 or 31 | Group B review diff; each number is tied to the test, scenario or drill in its last column, which documents 16 and 28 carry as identified test cases |
| The assist ladder paragraph agrees with master brief Section 25 and ADR-0015 | Group B review; document 25 and Appendix W carry the per-feature assignment the paragraph refers to |
