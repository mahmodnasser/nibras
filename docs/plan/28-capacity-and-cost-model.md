# 28. Capacity and Cost Model

> Plan document for the Nibras platform. Group F. It refines master brief Section 21 (the scale targets), Section 30 (the cost categories and their drivers), Section 32 (the retention schedule that multiplies storage) and Section 34 (the high-availability posture per mode and the capacity starting points); it does not re-derive them. Deployment modes, alerts and the warm-up operation are owned by `15-deployment-and-operations.md`; Redis sizing, pools and budgets by `21-performance-engineering.md`; worker scaling rules by `11-messaging-architecture.md` §7; rung 3 hardware by `25-ai-and-assist-ladder.md` §9. Those are cited, not restated. Where this document and the brief disagree, an ADR records the deviation.

**Group** F · **Requirement areas covered** PERF (capacity and the scale targets), INF (sizing per mode, scaling thresholds, FinOps) · **Last updated** 2026-09-22 by the platform plan

## Purpose

This document answers three questions with numbers a product owner can put in a price list and an engineer can put in a values file: how big each deployment mode is, when it grows, and what it costs per 1,000 students. It turns master brief Section 34's capacity starting points into a sizing table per mode, adds the scaling thresholds that move a deployment between those sizes, gives the cost of each mode as a formula with its drivers, works two examples end to end, lists the costs that are not infrastructure, names the Appendix N scenario that replaces each starting value with a measurement, and fixes the labels and the monthly review that keep the model honest after launch. The readers are the product owner pricing a plan, the platform engineer writing `values-prod.yaml` and the appliance's first-boot minimums, and the architect deciding when a number measured in phase 6 changes a size.

## Scope

| In scope | Out of scope | Where the out-of-scope item lives |
|---|---|---|
| Replicas per service class per mode, peak and off-peak | What each service does and which worker job groups exist | `05-service-catalog.md` |
| Node pools, database instances, RabbitMQ nodes, SeaweedFS volumes, single-server host bands | The operational path of each mode, backups, drills, the appliance lifecycle | `15-deployment-and-operations.md` §2, §7, §8 |
| PgBouncer and Redis sizing as capacity inputs | The pool settings, prepared-statement rule, Redis roles, key prefixes and memory table themselves | `21-performance-engineering.md` §2.4 and §5 |
| HPA thresholds per service class | KEDA queues, target depths and bounds per worker | `11-messaging-architecture.md` §7 |
| Storage per student per year with the Section 32 multiplier | The retention jobs and the partition detach mechanics | `10-data-architecture.md`; `21-performance-engineering.md` §4 |
| The cost formula per mode and two worked examples | Plan prices and commercial tiers | Master brief Section 22; the product owner |
| Non-infrastructure cost lines from Section 30 with drivers | Team shape and governance | Master brief Section 29 |
| The Appendix N scenario that proves each capacity number | Scenario scripts, thresholds and tiers | Appendix N; `21-performance-engineering.md` §8 |
| FinOps labels and the monthly review | Dashboards as code in general | `15-deployment-and-operations.md` §5.6 |

**Every unit price in this document is illustrative, to be replaced by the product owner's provider quotes.** The hosting provider and region are open question 15; until a quote exists, the prices below exist only so that the formulas can be exercised and the two examples can be checked for arithmetic.

---

## Content

### 1. Inputs and conventions

| Input | Value used here | Owner of the value |
|---|---|---|
| Scale targets | 500 schools, 500,000 students, 20,000 concurrent users | Master brief Section 21 |
| Data tiers | demo 600 students and 2 tenants; load 50,000 students across 50 tenants; scale 500,000 students across 500 tenants | Appendix N, N.1 |
| Capacity starting points | Gateway 3 replicas; Attendance 4 at the morning peak and 2 otherwise; Notification workers 2 to 20 by KEDA; Reporting projections 2; about 4 PgBouncer server connections per replica | Master brief Section 34 |
| API replicas at peak per service, pool sizes, `max_connections` 500 (scale) and 250 (single server) | Quoted, not restated | `21-performance-engineering.md` §5 |
| `redis-cache` and `redis-state` memory per tier | Quoted, not restated | `21-performance-engineering.md` §2.4 |
| Worker min, max and target depth | Quoted, not restated | `11-messaging-architecture.md` §7 |
| Peak window per time-zone band | Earliest first period minus 30 minutes to latest first period plus 20 minutes; roughly 4 hours per band per school day | `15-deployment-and-operations.md` §11 |
| Service classes | Gateway, Identity, Platform; read-heavy; write-heavy; Notification; workers; backends-for-frontends | Master brief Section 31; `15-deployment-and-operations.md` §5.1 |
| Month | 730 hours; 22 school days; peak window 4 hours per school day, so 88 peak hours and 642 off-peak hours | This document |
| Node packing | An 8 vCPU / 32 GB node offers about 7.5 vCPU and 28 GB allocatable; the model packs to 75 percent of allocatable, so 5.6 vCPU and 21 GB of requests per node | This document; replaced by the N-07 scale run |

**Status of every number.** A number in this document is one of three things, and the table that carries it says which: **quoted** (owned elsewhere and cited), **starting point** (master brief Section 34's words: "to be replaced by phase 6 load-test evidence"), or **estimate** (derived here, with its derivation, and replaced by the scenario named in part 6). Nothing is a measurement yet; CAP-PERF-02 in phase 6 is where the measurements arrive.

---

### 2. Sizing per deployment mode

This is the "part 2" that `15-deployment-and-operations.md` §2 and §8 cite for sizing and for the appliance's first-boot minimum.

#### 2.1 Developer

| Element | Size | Status |
|---|---|---|
| Machine | 8 cores, 32 GB memory recommended, 50 GB free disk; Windows 11, Ubuntu 22.04 or macOS 14 | Estimate |
| Applications | 23 applications and 7 workers as processes under the Aspire AppHost, one instance each | Quoted, `15-deployment-and-operations.md` §2.1 |
| Containers | postgres, pgbouncer, rabbitmq, redis-cache, redis-state, seaweedfs, gotenberg, clamav, mailpit, one each | Quoted, same |
| Redis | 256 MB per role | Quoted, `21-performance-engineering.md` §2.4 |
| The `ai` profile | Adds the 16 GB laptop floor for CPU inference | Quoted, `25-ai-and-assist-ladder.md` §9 |
| Cost | The developer's machine; not in the cost model | |

#### 2.2 Single server: host bands

One Linux host, or the appliance on Hyper-V or VMware, runs every application once. A school is placed in a band by its enrolled students; the first-boot wizard refuses a host below the small band's minimum (`15-deployment-and-operations.md` §8).

| Band | Students | vCPU | Memory | Data disk (SSD) | Backup disk | Observability profile | Status |
|---|---|---|---|---|---|---|---|
| Small (the refusal floor) | up to 800 | 8 | 32 GB | 500 GB | 1 TB | Off; alerts by email | Starting point |
| Medium | 800 to 3,000 | 16 | 64 GB | 1 TB | 2 TB | On | Starting point |
| Large | 3,000 to 6,000 | 24 | 96 GB | 2 TB | 4 TB | On | Starting point |
| Above large | over 6,000, or a group with more than three campuses | Scale mode as a dedicated deployment (`15-deployment-and-operations.md` §2.3) | | | | | Decision |

Memory budget of the small band, which is why 32 GB is the floor:

| Consumer | Memory | Basis |
|---|---|---|
| 23 applications and 7 workers, one replica each | about 9 GB | about 300 MB resident per .NET host at idle, estimate |
| PostgreSQL, 20 databases | 8 GB | `shared_buffers` 4 GB plus working memory, estimate |
| RabbitMQ, one node with quorum queues | 1 GB | Estimate |
| `redis-cache` and `redis-state` | 0.25 GB and 0.5 GB `maxmemory` | Scaled down from the demo column of `21-performance-engineering.md` §2.4; the idempotency store of a 600-student school holds about 50,000 writes a day at 1 KB |
| ClamAV | 3 GB | Signature database in memory |
| Gotenberg, SeaweedFS, Caddy | 2.5 GB | Estimate |
| Operating system and page cache headroom | 7 GB | Remainder |

Within a single server:

| Element | Size | Status |
|---|---|---|
| Replicas | One per application; Attendance one; `notification-worker-urgent` and `notification-worker-bulk` one each; workers that scale from zero in `11-messaging-architecture.md` §7 stay at zero until a job arrives | Starting point |
| PgBouncer | Every value in `21-performance-engineering.md` §5 halved; `max_connections` 250 | Quoted |
| RabbitMQ | One node, quorum queues still enabled | Quoted, master brief Section 34 |
| Redis | Two containers, TLS, separate AOF volume for `redis-state` | Quoted, `21-performance-engineering.md` §2.4 |
| Storage | Local disk; object storage on SeaweedFS or local disk | Quoted, master brief Section 34 |
| Recovery | Restore from backup, told to the school in writing | Quoted, master brief Section 34 |

#### 2.3 Scale: replicas per service class

Two sizes are given: the **load-tier region** (50 tenants, 50,000 students, Appendix N's load tier) and the **scale-tier region** (500 tenants, 500,000 students). "Peak minimum" applies inside a band's peak window through the KEDA cron trigger of `15-deployment-and-operations.md` §11; "off-peak minimum" applies at every other hour; HPA scales between the minimum and the maximum on the thresholds in part 3. The minimum of 2 off peak is not an efficiency choice: master brief Section 34 makes zone loss survivable, and one replica cannot survive the loss of its zone.

| Application | Class | vCPU / memory request per replica | Peak minimum (load tier) | Off-peak minimum | HPA maximum | Expected at the scale-tier peak | Status |
|---|---|---|---|---|---|---|---|
| Gateway | Edge | 1 / 1 GB | 3 | 3, one per zone | 12 | 9 | Starting point (Section 34) |
| Identity | Edge | 0.5 / 0.75 GB | 3 | 2 | 8 | 6 | Starting point (`21-performance-engineering.md` §5) |
| Platform | Edge | 0.5 / 0.75 GB | 2 | 2 | 4 | 3 | Starting point |
| School | Read-heavy | 0.5 / 0.75 GB | 3 | 2 | 8 | 6 | Starting point |
| Scheduling, Academics, Reporting | Read-heavy | 0.5 / 0.75 GB | 2 each | 2 each | 6 each | 4 each | Starting point |
| Attendance | Write-heavy | 1 / 1 GB | 4 | 2 | 16 | 12 | Starting point (Section 34) |
| Assessment | Write-heavy | 0.5 / 1 GB | 2 | 2 | 8 | 6 | Starting point |
| Finance | Write-heavy | 0.5 / 1 GB | 2 | 2 | 6 | 4 | Starting point |
| Bff.Mobile | Backend-for-frontend | 0.5 / 0.75 GB | 3 | 2 | 12 | 10 | Estimate |
| Bff.Web | Backend-for-frontend | 0.5 / 0.75 GB | 2 | 2 | 6 | 4 | Estimate |
| Admissions, Communication, Notification, Requests, Documents, Behavior, Audit, Wellbeing, Hr, Operations, Ai (API hosts) | Other | 0.25 / 0.5 GB | 2 each | 2 each | 4 each | 3 each | Starting point |

Workers follow `11-messaging-architecture.md` §7 for bounds; the table adds the request per replica and the replica count the cost model assumes.

| Worker deployment | vCPU / memory request per replica | Assumed in the peak window (load tier) | Assumed off peak | Assumed at the scale-tier peak | Bounds |
|---|---|---|---|---|---|
| `notification-worker-urgent` | 0.5 / 0.5 GB | 6, the band cron value | 2 | 12 | Quoted |
| `notification-worker-bulk` | 0.5 / 0.5 GB | 2 | 2 | 6 | Quoted |
| `documents-worker` | 1 / 1.5 GB | 1 | 1 | 4 | Quoted |
| `scheduling-worker` | 2 / 2 GB | 0 | 0 | 1 | Quoted |
| `assessment-worker` | 1 / 1 GB | 1 | 1 | 3 | Quoted |
| `finance-worker` | 0.5 / 1 GB | 1 | 1 | 2 | Quoted |
| `reporting-projections` | 0.5 / 1 GB | 2 | 2 | 6 | Quoted, 2 is the Section 34 starting point |
| `ai-worker` | 0.5 / 1 GB, GPU separate | 0 | 0 | 1 | Quoted |

Bulk bursts (invoice runs, report-card batches, imports) are placed off peak by `11-messaging-architecture.md` §5 and last minutes; they are absorbed by the 25 percent packing headroom and the cluster autoscaler rather than priced as standing replicas.

#### 2.4 Scale: platform components in the application pool

| Component | Load-tier region | Scale-tier region | Status |
|---|---|---|---|
| RabbitMQ | 3 nodes, 2 vCPU / 4 GB each, quorum queues | 3 nodes, 4 vCPU / 8 GB each | Starting point; node count quoted from Section 34 |
| `redis-cache` | Sentinel, 1 primary and 2 replicas, `maxmemory` per `21-performance-engineering.md` §2.4, 0.5 vCPU each | Same topology, 1 vCPU each; cluster mode past the §2.4 line | Quoted memory; estimated CPU |
| `redis-state` | Sentinel, 1 primary and 2 replicas, 1 vCPU each | 2 vCPU each | Quoted memory; estimated CPU |
| Sentinels | 3 per role, 0.1 vCPU | Same | Quoted |
| PgBouncer | 2 replicas, 0.5 vCPU; `max_client_conn` 2,000 | 4 replicas; `max_client_conn` 4,000, because HPA maxima multiply client connections at 20 per replica while server connections stay capped by `max_db_connections` | Estimate |
| SeaweedFS | 3 masters (0.25 vCPU), 3 volume servers (1 vCPU / 2 GB) | 3 masters, 6 volume servers (1 vCPU / 4 GB) | Starting point; replication across three volumes quoted from Section 34 |
| Gotenberg | 2 replicas, 1 vCPU / 1 GB | 6 replicas | Estimate; N-02 decides |
| ClamAV | 2 replicas, 0.5 vCPU / 3 GB | 4 replicas | Estimate; N-10 upload scan under 20 s decides |
| OpenBao | 3 replicas, 0.25 vCPU | Same | Estimate |
| Ingress | 2 replicas, 0.5 vCPU | 4 replicas | Estimate |
| Operators (KEDA, External Secrets, the PostgreSQL operator, Argo CD or Flux) | 1 vCPU / 2 GB in total | 2 vCPU / 4 GB | Estimate |

#### 2.5 Scale: totals and node count

Computed from the two tables above with the packing rule in part 1.

| State | API replicas | Worker replicas | vCPU requested | Memory requested | 8 vCPU / 32 GB nodes | Binding constraint |
|---|---|---|---|---|---|---|
| Load tier, peak window | 52 | 13 | 53.1 | 94 GB | 10 | CPU |
| Load tier, off peak | 47 | 9 | 47.6 | 88 GB | 9 | CPU |
| Scale tier, peak window | 105 | 35 | 120.4 | 233 GB | 22 | CPU |
| Scale tier, off peak | 52 | 9 | 72.1 | 169 GB | 13 | CPU |

**What this says about calendar-aware scaling.** At the load tier, the zone-survivable floor of 2 replicas dominates and the peak window adds one node: calendar-aware scaling is a latency feature there, not a cost feature. At the scale tier the peak adds nine nodes for 88 hours a month instead of 730, and it is the difference between USD 3,380 and USD 5,280 a month in compute at the illustrative node price (part 4.3). This is the pricing `15-deployment-and-operations.md` §11 asks for.

#### 2.6 Scale: database

One PostgreSQL cluster holds the 20 service databases (master brief Section 34: operator-managed, one primary and two replicas across zones, synchronous to one). Wellbeing keeps its own PgBouncer user and pool on the same cluster (`21-performance-engineering.md` §5).

| Element | Load-tier region | Scale-tier region | Status |
|---|---|---|---|
| Instances | 3 × 8 vCPU / 64 GB | 3 × 32 vCPU / 256 GB | Starting point |
| Data volume per instance, year one | 500 GB SSD | 2 TB SSD | Estimate from part 2.8 (2.23 MB per student) |
| Data volume per instance, steady state | 1 TB SSD (423 GB used) | 8 TB SSD (4.2 TB used) | Estimate from part 2.8 (8.46 MB per student) |
| `max_connections` | 500 | 500 | Quoted, `21-performance-engineering.md` §5 |
| Split into a second cluster | Not expected | When the primary exceeds 60 percent CPU at the N-01 peak on three consecutive runs, or a volume passes 4.5 TB used; the first candidates to move are the month-partitioned write-heavy databases (`nibras_attendance`, `nibras_notification`, `nibras_audit`) | Decision by ADR on N-01 and N-07 scale-run evidence |
| Reporting replica | Reporting reads from a replica through `nibras_reporting_ro` | Same | Quoted, `21-performance-engineering.md` §6 |

#### 2.7 Scale: Redis, pools and RabbitMQ, cited

| Component | Where the size lives | What this document adds |
|---|---|---|
| `redis-cache` `maxmemory` 256 MB, 512 MB, 3 GB by tier | `21-performance-engineering.md` §2.4 | The container limit (768 MB, 4 GB) is the memory request in part 2.4 |
| `redis-state` `maxmemory` 256 MB, 3 GB, 8 GB by tier | `21-performance-engineering.md` §2.4 | The idempotency lifetime cut to 12 hours is the first lever before hardware, as §2.4 records |
| PgBouncer pools, sum of `max_db_connections` 432 | `21-performance-engineering.md` §5 | HPA maxima in part 2.3 raise client connections only; `max_client_conn` in part 2.4 is sized for them |
| RabbitMQ lanes and per-tenant fairness | `11-messaging-architecture.md` | Node sizes in part 2.4 |

#### 2.8 Storage per student per year, with the Section 32 multiplier

Stored volume at steady state follows from Little's law: what is kept equals what is written each year times the average number of years it is kept. The average for a row kept "until leaving plus N years" is the average remaining enrolment plus N; this model uses **6 years** of average remaining enrolment (a 12-year stay, uniform intake), which is open point 2. Steady state is reached after the longest retention horizon; year one holds only the annual volume.

**Database, per student.**

| Data class | Written per student per year | Retention (Section 32; Appendix J) | Multiplier | Steady state |
|---|---|---|---|---|
| Attendance records, excuses, roll calls | 0.25 MB (about 1,500 rows with indexes) | 7 years, delete by partition | 7 | 1.75 MB |
| Academic record: marks, results, report-card rows, transcripts | 0.10 MB | 10 years after leaving | 16 | 1.60 MB |
| Audit entries | 0.30 MB | 7 years, then detached to cold storage | 7 | 2.10 MB |
| Notification delivery log | 1.00 MB | 90 days | 0.25 | 0.25 MB |
| Messages and announcements | 0.20 MB | 2 years | 2 | 0.40 MB |
| Financial documents | 0.03 MB | 10 years | 10 | 0.30 MB |
| Behaviour, interventions, kindergarten sheets | 0.06 MB | Until leaving plus 3 years | 9 | 0.54 MB |
| Wellbeing records | 0.04 MB | Until leaving plus 7 years | 13 | 0.52 MB |
| Live state that does not accumulate: School structure, Scheduling, Requests, reference copies, Reporting projections | 1.00 MB flat | Life of the record | 1 | 1.00 MB |
| **Total** | **2.23 MB in year one** | | | **8.46 MB** |

**Object storage, per student.** Uploaded files follow the retention of their owning record (Appendix J).

| Content | Written per student per year | Retention of the owning record | Multiplier | Steady state |
|---|---|---|---|---|
| Report-card and transcript PDFs | 0.6 MB | 10 years after leaving | 16 | 9.6 MB |
| Certificates and letters | 0.2 MB | 10 years after leaving | 16 | 3.2 MB |
| Coursework submissions | 8.0 MB | No Appendix J row names them; priced at until leaving plus 3 years (open point 3) | 9 | 72.0 MB |
| Message and announcement attachments, per-student share | 4.0 MB | 2 years | 2 | 8.0 MB |
| Portfolio, recognition and school-memory media | 5.0 MB | Life of the portfolio; priced at until leaving plus 3 years | 9 | 45.0 MB |
| Attendance excuse evidence | 0.5 MB | 7 years | 7 | 3.5 MB |
| Wellbeing uploads | 0.2 MB | Until leaving plus 7 years | 13 | 2.6 MB |
| **Total** | **18.5 MB in year one** | | | **143.9 MB** |

**Physical multipliers on top of the logical figures.**

| Copy | Multiplier | Basis |
|---|---|---|
| Database, scale mode | × 3 instances | Primary and two replicas (Section 34) |
| Database backups | × 4.5 of the database size, in object storage | pgBackRest weekly full kept 35 days plus 12 monthly: 17 full copies at about 4 to 1 compression is 4.25, plus 35 days of WAL at about 0.25 |
| Object storage, scale mode | × 3 on SeaweedFS volumes | Three replicated volumes (Section 34) |
| Object storage versioned copy | × 1.15 in provider object storage | One nightly versioned copy plus 15 percent version overhead |
| Cold archive after year 7 | under 0.05 MB per student per year | Detached audit and attendance partitions as Parquet at about 12 to 1 (`21-performance-engineering.md` §4); below one cent per 1,000 students per month at the illustrative price, and not carried further |
| Logs, metrics, traces | 30 days, 13 months downsampled, 7 days | Section 32; priced inside the observability line, not per student |

**Coursework and portfolio media are 81 percent of steady-state object storage.** A school that keeps them for less time, or a plan that caps them, changes the storage line more than any other lever; the storage limit per plan in master brief Section 22 is where that choice is sold.

---

### 3. Scaling thresholds

#### 3.1 HPA per service class

HPA scales API hosts between the minima and maxima of part 2.3. Every class scales on CPU; the latency classes also scale on their own latency indicator through the Prometheus adapter, set at 80 percent of the Section 31 budget so that scaling starts before the budget is breached. A latency trigger is useless when the bottleneck is the database: more replicas add client connections but not server connections, so a latency scale-out that does not move p95 within two evaluations is a `PgBouncerPoolSaturated` or a query problem, not a replica problem.

| Service class | Applications | CPU target | Latency trigger (p95 over 2 min) | Scale-up step | Scale-down stabilisation | Budget it protects |
|---|---|---|---|---|---|---|
| Edge | Gateway, Identity, Platform | 60% | `nibras_identity_token_validation_duration_seconds` over 120 ms; Gateway request duration over 120 ms | +100% or +2 replicas per 30 s | 600 s | 150 ms token validation (Section 31) |
| Read-heavy | School, Scheduling, Academics, Reporting | 65% | Database read over 200 ms, or cached read over 64 ms | +100% per 30 s | 300 s | 250 ms and 80 ms (Section 31) |
| Write-heavy | Attendance, Assessment, Finance | 60% | Write over 400 ms (`nibras_<service>_write_duration_seconds`) | +100% or +4 replicas per 30 s; Attendance +4 | 900 s inside a peak window, 300 s outside | 500 ms write (Section 31); `AttendancePeakLatency` |
| Backends-for-frontends | Bff.Mobile, Bff.Web | 60% | `nibras_bff_compose_duration_seconds` over 200 ms | +100% per 30 s | 600 s | Composed home 250 ms (N-05) |
| Other API hosts | Admissions, Communication, Notification, Requests, Documents, Behavior, Audit, Wellbeing, Hr, Operations, Ai | 70% | none | +1 replica per 60 s | 300 s | Class budget via `ApiLatencyBudgetBreach` |

#### 3.2 KEDA per worker

Quoted from `11-messaging-architecture.md` §7, which owns the queues and the numbers; this table adds only the scenario that proves each bound (part 6).

| Worker deployment | Target depth per replica | Min to max | Scenario that proves the maximum |
|---|---|---|---|
| `notification-worker-urgent` | 20 messages | 2 to 20; 6 in a band's peak window | N-09 push within 30 s at p95 |
| `notification-worker-bulk` | 500 messages | 2 to 20 | N-01 absence fan-out; N-07 |
| `documents-worker` | per queue: render 20, scan 10, import 5, export 2, OCR 10 | 1 to 12 | N-04 import in under 5 minutes; N-10 scan in under 20 s |
| `scheduling-worker` | 1 solve | 0 to 4 | No Appendix N scenario; the solve time is a unit benchmark (open point 5) |
| `assessment-worker` | results 5, report cards 200 | 1 to 8 | N-02, 800 cards in under 10 minutes |
| `finance-worker` | invoice runs 4, reminders 10, statements 4 | 1 to 6 | N-03, 5,000 invoices in under 6 minutes |
| `reporting-projections` | 2,000 ready messages over the shards, or 30 s lag | 2 to 8 | N-05 projection lag under 60 s; N-07 |
| `ai-worker` | embeddings 50, drafts 2 | 0 to 4 | Rung 3 throughput in `25-ai-and-assist-ladder.md` §9 |

#### 3.3 Thresholds for everything that does not autoscale

| Component | Threshold | Action | Alert (`15-deployment-and-operations.md` §5.7) |
|---|---|---|---|
| Cluster node pool | Pods pending for over 30 s | Cluster autoscaler adds a node | `CalendarScaleUpMissed` if the peak minimum is not met by first period minus 15 minutes |
| PostgreSQL primary CPU | Over 60% at the N-01 peak on three consecutive school days | Next instance size, then the split in part 2.6 | `ApiLatencyBudgetBreach`, `PgBouncerPoolSaturated` |
| PostgreSQL volume | 70% used | Expand the volume in the next maintenance window | `PostgresDiskHigh` at 80%, `PostgresDiskCritical` at 90% |
| `redis-cache` | Evictions over 1% of sets for 30 min | Raise `maxmemory` within the container limit, then the limit | `RedisCacheEvictionsHigh` |
| `redis-state` | 70% of `maxmemory` | Shorten idempotency lifetime per `21-performance-engineering.md` §2.4, then raise the limit | `RedisStateMemoryHigh` at 80% |
| RabbitMQ | Memory or disk alarm, or a lane not back to baseline 10 minutes after a burst | Next node size; never a fourth node without an ADR | `RabbitMqNodeDown`, `WorkerQueueNotDraining` |
| SeaweedFS | Volume servers 70% full | Add a volume server | `ObjectStorageReplicationDegraded` covers replica loss, not fullness; fullness is on the `cost.json` board |
| Single-server host | Enrolment reaches 90% of the band ceiling, or `HostDiskCritical` | Move the school to the next band at the next upgrade window | `HostDiskCritical` |
| Rung 3 hardware | Draft p95 over 8 s at the single-server target | Add a GPU node, or leave the feature at rung 1 | `AiDegradedToRungOne` |

---

### 4. The cost model

#### 4.1 Illustrative unit prices

**Illustrative, to be replaced by the product owner's provider quotes.** Every figure in parts 4 and 5 is computed from this table and nothing else, so replacing a row re-prices both worked examples.

| Unit | Illustrative price (USD) | Used by |
|---|---|---|
| General-purpose node, 8 vCPU / 32 GB | 240 per month, so 240 ÷ 730 per node-hour (about 0.329; the worked examples use the exact quotient) | Application pool, observability pool |
| Memory-optimised database node, 8 vCPU / 64 GB | 360 per month | PostgreSQL, load-tier region |
| Memory-optimised database node, 32 vCPU / 256 GB | 1,440 per month | PostgreSQL, scale-tier region |
| SSD block storage | 0.10 per GB-month | Database volumes, observability volumes |
| Standard block storage | 0.04 per GB-month | SeaweedFS volume servers |
| Object storage | 0.02 per GB-month | Backups, versioned copy, cold archive, off-site target |
| Egress | 0.08 per GB | Traffic leaving the region to parents and staff |
| Load balancer | 25 per month each | Scale mode, two per region |
| Kubernetes control plane | 75 per month per cluster | Scale mode, and the cold standby region |
| Status page and DNS, hosted outside the platform | 20 per month | Scale mode (master brief Section 31) |
| Rented dedicated host, small band | 110 per month | Single server |
| Rented dedicated host, medium and large bands | 200 and 320 per month | Single server |
| Domain | 1 per month | Single server |

#### 4.2 The formula per mode

Symbols: `S` students; `s_db` and `s_obj` the per-student storage of part 2.8 (year one or steady state); `e` egress per student-month; `N_peak` and `N_off` nodes from part 2.5; `h_peak` 88 and `h_off` 642 hours; `p_*` the unit prices of part 4.1.

**Single-server mode**, one school per host:

| Line | Formula | Driver |
|---|---|---|
| Compute | `p_host(band)` | The band of part 2.2, set by enrolled students |
| Database | 0; on the host's data disk | Students times the Section 32 multiplier, which must stay under 70% of the data disk |
| Storage | 0; on the host's data disk | As above, for object storage |
| Egress | `S × e × p_egress` | Guardians active in the app; often included by the host provider |
| Observability | 0 in the small band; on the same host otherwise | The profile is off by default on the small band |
| Backups | `(4.5 × S × s_db + 1.15 × S × s_obj) × p_obj` | Off-site target chosen at first boot |
| Domain and certificate | `p_domain`; certificates are automatic | One per school |

**Scale mode**, one region:

| Line | Formula | Driver |
|---|---|---|
| Compute | `(N_peak × h_peak + N_off × h_off) × p_node_hour` | Peak concurrency per band; replicas per service class; the calendar-aware peak window |
| Database | `3 × p_dbnode(size) + 3 × V_db × p_ssd` | Students times the Section 32 multiplier (`V_db` from part 2.6); the N-01 peak for the instance size |
| Storage | `S × s_obj × 3 × p_std` | Students, documents and media per student per year, times Section 32 retention, times three replicas |
| Egress | `S × e × p_egress` | Guardians active in the app, report-card and media downloads |
| Observability | `N_obs × p_node + V_obs × p_ssd` | Replicas (series), requests (logs), and the retention of 30 days, 13 months and 7 days (Section 32) |
| Backups | `(4.5 × S × s_db + 1.15 × S × s_obj) × p_obj` | Database size; object storage size |
| Network | `2 × p_lb + p_control_plane + p_status` | Per region; residency makes this per region, not per fleet (RISK-28) |
| Cold standby | `p_control_plane` | Cold per `15-deployment-and-operations.md` §7; a warm standby would add the compute and database lines again |

**Cost per 1,000 students** is `C ÷ S × 1,000` for the month. **Egress** uses `e = 75 MB per student-month`: 1.5 guardians per student at about 40 MB each of app traffic and PDF downloads, plus 15 MB of staff traffic per student; an estimate replaced by the first term's measurement (open point 9).

#### 4.3 Worked example A: a 600-student school, single-server mode

One campus, small band (part 2.2), observability profile off, rung 3 off. Storage: database 1.3 GB in year one and 5.1 GB at steady state; objects 11 GB and 86 GB; both inside the 500 GB data disk.

| Line | Working | Year one (USD/month) | Steady state (USD/month) |
|---|---|---|---|
| Compute | Rented small-band host | 110.00 | 110.00 |
| Database | On the host | 0.00 | 0.00 |
| Storage | On the host | 0.00 | 0.00 |
| Egress | 600 × 75 MB = 45 GB × 0.08 | 3.60 | 3.60 |
| Observability | Profile off; alerts by email | 0.00 | 0.00 |
| Backups | Year one: 4.5 × 1.3 GB + 1.15 × 11.1 GB = 18.8 GB; steady: 4.5 × 5.1 GB + 1.15 × 86.3 GB = 122.1 GB; × 0.02 | 0.38 | 2.44 |
| Domain and certificate | | 1.00 | 1.00 |
| **Total** | | **114.98** | **117.04** |
| **Per 1,000 students** | Total ÷ 0.6 | **191.63** | **195.07** |
| Per student per year | Total × 12 ÷ 600 | 2.30 | 2.34 |

Variant: a school that owns the appliance host replaces the compute line with the purchase price over its life plus power. At an illustrative USD 3,500 over 48 months plus USD 15 a month of power, compute is USD 87.92 and the steady-state total per 1,000 students is USD 158.27. The host dominates either way: a single server is paid for by the machine, not by the students, and a school well below its band ceiling pays for headroom it does not use.

#### 4.4 Worked example B: a 50-school region at 50,000 students, scale mode

The load-tier region of part 2: 50 tenants, three time-zone bands, rung 3 off.

| Line | Working | Year one (USD/month) | Steady state (USD/month) |
|---|---|---|---|
| Compute | (10 nodes × 88 h + 9 nodes × 642 h) × 240 ÷ 730 | 2,188.93 | 2,188.93 |
| Database, instances | 3 × 360 | 1,080.00 | 1,080.00 |
| Database, volumes | Year one 3 × 500 GB; steady 3 × 1,000 GB; × 0.10 | 150.00 | 300.00 |
| Storage | Year one 925 GB; steady 7,195 GB; × 3 replicas × 0.04 | 111.00 | 863.40 |
| Egress | 50,000 × 75 MB = 3,750 GB × 0.08 | 300.00 | 300.00 |
| Observability | 2 nodes × 240 + 500 GB × 0.10 | 530.00 | 530.00 |
| Backups | Year one 4.5 × 111.5 GB + 1.15 × 925 GB = 1,565.5 GB; steady 4.5 × 423 GB + 1.15 × 7,195 GB = 10,178 GB; × 0.02 | 31.31 | 203.56 |
| Network | 2 × 25 + 75 + 20 | 145.00 | 145.00 |
| Cold standby | 75 | 75.00 | 75.00 |
| **Total** | | **4,611.24** | **5,685.89** |
| **Per 1,000 students** | Total ÷ 50 | **92.22** | **113.72** |
| Per student per year | Total × 12 ÷ 50,000 | 1.11 | 1.36 |

Had the peak minimums run all day, compute would be 10 × 730 h × 240 ÷ 730 = USD 2,400, so calendar-aware scaling saves USD 211 a month at this size (part 2.5 explains why it is small here).

#### 4.5 Comparison and the scale-tier indication

| Deployment | Per 1,000 students, year one | Per 1,000 students, steady state | Largest line at steady state |
|---|---|---|---|
| A: 600 students, single server | USD 191.63 | USD 195.07 | The host (94%) |
| B: 50,000 students, scale mode | USD 92.22 | USD 113.72 | Compute (38%), then database instances and volumes (24%) and storage (15%) |
| Scale-tier region, 500,000 students (indication only, not a worked example) | not computed | about USD 50.50 | Storage (34%): 71,950 GB × 3 × 0.04 = USD 8,634 of about USD 25,250 |

The indication uses the scale-tier rows of part 2: compute USD 3,380 (22 and 13 nodes), database USD 4,320 plus USD 2,400 of 8 TB volumes, backups USD 2,036, egress USD 3,000, observability USD 1,260 (four nodes and 3 TB), network and standby USD 220 (the part 4.2 formula: two load balancers, the control plane, the status page and the standby control plane). Against USD 5,280 had peak minimums run all day, calendar-aware scaling saves USD 1,900 a month at the scale tier, 36 percent of compute.

Two conclusions for the price list. First, a single server costs roughly twice as much per student as a shared region, so the on-premises price is set by the host band, not by the per-student rate. Second, at every size storage is the line that grows without anyone deciding to grow it (master brief Section 30), and at the scale tier it becomes the largest line; the storage limit per plan (master brief Section 22) is a cost control, not only a commercial term.

---

### 5. Costs that are not infrastructure

The categories and drivers are master brief Section 30's. Prices are **illustrative, to be replaced by the product owner's provider quotes**; the example columns use the two worked examples' student counts.

| Line (Section 30) | Driver | Formula | Illustrative unit price | Example A (USD/month) | Example B (USD/month) | Notes |
|---|---|---|---|---|---|---|
| Push notifications | none | 0 | Free | 0 | 0 | Firebase Cloud Messaging and the Apple Push Notification service behind `IPushSender`; free but not open source (master brief Section 6.3) |
| App stores | Accounts: one shared app, plus one per white-label school | Apple fee ÷ 12 per account, plus the Google one-off fee once per account | Apple Developer Program 99 per year; Google Play 25 once | 0 unless white-label; 8.25 if it is | 8.25 for the shared app | White-label accounts are the school's own (master brief Section 37) and the fee is passed through |
| Apple build capacity | iOS builds per month × minutes per build × flavours | Hosted runner minutes × price, or one Mac host once | Hosted macOS minute 0.08; Mac build host 1,000 once | Fleet cost | 40 builds × 25 min × 0.08 = 80 per flavour | Open question 14; RISK-04; a fleet line, not per school |
| SMS and WhatsApp | Guardians without push × messages per month × price per country | `S × 1.5 × 10% × 8 × p_sms` | 0.04 per SMS | 28.80 | 2,400.00 | USD 48 per 1,000 students; sold as SMS credits (master brief Section 22); the urgent-only fallback keeps it optional |
| Payment gateway | Value collected online × rate | `V_online × r` | 2.5% of value | Pass-through | Pass-through | Optional; the default is manual and bank transfer (master brief Section 36) |
| Email at scale | Messages above the self-hosted relay's free tier | `S × 1.5 × 20 × p_email` | 0.10 per 1,000 messages | 1.80 | 150.00 | USD 3 per 1,000 students; plus the sending-domain work of master brief Section 38 |
| AI hardware | Rung 3 enabled per deployment | Single server: GPU price ÷ life; scale: GPU node-hours in school hours | 16 GB GPU 1,500 once over 36 months; 48 GB GPU node 1,600 per month | 41.67 if rung 3 is on | 1,600 × 176 h ÷ 730 = 385.75 if rung 3 is on | Sizes from `25-ai-and-assist-ladder.md` §9; zero when rung 3 is off, and everything degrades to rung 1 |
| Penetration test | Once before general availability, annually after | Test price ÷ 12 across the fleet | 20,000 per test | Covered by the release, not the school | 1,666.67 if the region is the whole fleet | Master brief Section 20; CAP-SEC-01 |
| Standards conformance | A customer requiring 1EdTech certification | Membership plus per-standard certification | 1EdTech's quote at the time; no figure is modelled | 0 | 0 until a customer pays | Compatibility is built; certification only when paid for (master brief Section 30) |
| People | The team in master brief Section 29 | FTE × loaded cost per FTE-month | 7,000 per FTE-month | Fleet cost | 7.5 to 10.5 FTE: 52,500 to 73,500 | The largest line by a wide margin: roughly ten times example B's infrastructure |

**Per 1,000 students, the variable non-infrastructure lines** (SMS USD 48, email USD 3, and rung 3 at USD 7.72 in scale mode) add about USD 51 to USD 59 to the infrastructure figures of part 4.5. They are sold as metered limits (SMS credits, AI usage per Appendix G) rather than folded into the per-student price, so a school that never sends an SMS does not pay for one.

---

### 6. Which Appendix N scenario proves which capacity number

Every starting point and estimate above is replaced by a measurement from the scenario named here, at the tier named here, in phase 6 (CAP-PERF-02) at the latest. A replacement that raises a budget needs an ADR (Appendix N, N.3 rule 2); a replacement that only changes a size is recorded in this document's review record with the run's tier, seed and commit (N.3 rule 4).

| Capacity number | Current value and status | Scenario and tier | What is measured | How the replacement is derived |
|---|---|---|---|---|
| Gateway replicas | 3, starting point | N-01 at scale; N-11 | Requests per second per replica at 60% CPU inside the peak; p95 at the edge | Peak requests ÷ per-replica capacity, rounded up, never below 3 (one per zone) |
| Attendance peak and off-peak replicas | 4 and 2, starting points | N-01 at load and scale; N-08 | Mark-batch writes per replica at p95 under 500 ms; the sync-storm burst | As above, with the N-08 burst as the maximum |
| Identity, School and other API peak minimums | Part 2.3, starting points | N-01, N-05, N-10 | CPU and p95 per class during each scenario | The class latency trigger in part 3.1 never fires at the minimum on a normal day |
| Bff.Mobile and Bff.Web | Part 2.3, estimates | N-05 at load and scale | Composed home p95 under 250 ms; cold start | Replicas at which p95 holds with 20% CPU headroom |
| Notification worker bounds | 2 to 20, starting point | N-09; N-01 fan-out; N-07 | Push p95 within 30 s; lane depth back to baseline in 10 minutes | Replicas at the 42,000-recipient N-09 burst, plus one |
| Reporting projections | 2, starting point | N-05; N-07 | Projection lag under 60 s | Shards with lag over 30 s at the N-05 burst |
| Assessment, Finance and Documents worker maxima | 8, 6 and 12, quoted | N-02, N-03, N-04 | 800 cards in 10 minutes; 5,000 invoices in 6 minutes; 10,000 rows in 5 minutes, while interactive p95 holds | Replicas at which the batch meets its time without the concurrent p95 moving |
| PgBouncer pool sizes and `max_db_connections` | `21-performance-engineering.md` §5, starting point | N-01, N-06, N-07 | Clients waiting for a server connection; pool use at the peak; exhaustion over 24 hours | Peak server connections in use plus 25%, per pool; the sum stays under `max_connections` minus 68 |
| `redis-cache` and `redis-state` memory | `21-performance-engineering.md` §2.4, estimate | N-07 at scale; N-08 for idempotency; N-11 | `used_memory` peak per role; evictions; keys per prefix | Peak plus 33%, as `maxmemory` at 75% of the container |
| RabbitMQ node size | Part 2.4, starting point | N-09, N-03, N-07 | Memory and disk alarms; quorum-queue memory per lane | Peak memory plus 50% headroom |
| Database instance size | Part 2.6, starting point | N-01 at scale; N-06; N-07 | Primary CPU, buffer-cache hit ratio, replica lag at the peak | Smallest size at which primary CPU stays under 60% at the N-01 peak |
| Storage per student | Part 2.8, estimate | N-07 at scale, whose tier carries 3 years of partitions | `pg_database_size` per service database and SeaweedFS bytes per tenant, divided by students and years | Measured annual volume per data class; the multipliers stay Section 32's |
| Node packing (5.6 vCPU and 21 GB per node) | Part 1, estimate | N-07 at scale | Requested versus used CPU and memory per node over 24 hours | Allocatable × the highest safe utilisation observed without evictions |
| Warm-up cost | `21-performance-engineering.md` §9, estimate | N-11; N-01 across four bands | Commands and bytes per tenant; time to warm a band | Recorded as measured |
| Single-server bands | Part 2.2, starting point | No Appendix N scenario runs on a single server | See open point 4 | N-01 and N-02 scripts against one appliance per band |
| Egress per student | 75 MB per student-month, estimate | None; a production measurement | Egress bytes per tenant per month in the first term | Median across tenants, by tenant tier |

---

### 7. FinOps labels and the monthly review

#### 7.1 Labels

Every Kubernetes object in the umbrella chart and every cloud resource created by OpenTofu carries these keys, as labels and as provider tags respectively. A resource without them fails the chart's lint and the OpenTofu plan check, because an unlabelled resource is a cost nobody owns.

| Key | Values | Purpose |
|---|---|---|
| `app.kubernetes.io/name` | `<service>-<kind>`, matching the image name in Appendix L (for example `attendance-api`, `notification-worker`) | Cost per service and per image |
| `app.kubernetes.io/component` | `api`, `worker`, `platform`, `database`, `observability` | Cost per layer |
| `app.kubernetes.io/part-of` | `nibras` | Separates product cost from anything else in the account |
| `nibras/service-class` | `edge`, `read-heavy`, `write-heavy`, `notification`, `worker`, `bff`, `other`, `infrastructure` | Joins cost to the thresholds of part 3.1 |
| `nibras/cost-line` | `compute`, `database`, `storage`, `egress`, `observability`, `backups`, `network`, `standby` | Joins the bill to the formula of part 4.2 line by line |
| `nibras/deployment` | `shared`, `dedicated` | Separates the SaaS region from dedicated deployments |
| `nibras/tenant` | The tenant identifier, on a dedicated deployment's namespace and resources only | A dedicated deployment is billed to one tenant; shared resources never carry a tenant |
| `nibras/region` | The region code | Residency makes cost per region the unit (RISK-28) |
| `nibras/environment` | `production`, `staging`, `test`, `preview` | Keeps preview and load-tier spend visible and separate |

**Allocating shared cost to tenants.** Shared resources carry no tenant label. Cost per tenant is allocated by driver: compute and egress by the tenant's share of requests, read from the `tenant_tier` label on request metrics (`15-deployment-and-operations.md` §5.5); database and storage by the tenant's share of bytes; the rest by students. The board shows allocation by `tenant_tier` (`small`, `medium`, `group`, `oversized`); per-tenant figures appear only in the monthly export, which keeps metric cardinality inside §5.5's guardrails.

**Where it is computed.** Prometheus recording rules multiply each pod's requests by the unit prices of part 4.1, held in one ConfigMap next to the rules, and feed the `cost.json` board named in `15-deployment-and-operations.md` §5.6. Storage and egress come from the provider bill, imported monthly. No new dependency is added for this (open point 8).

#### 7.2 The monthly review

| Element | Content |
|---|---|
| When | First working week of each month, and within a week of any provider price change |
| Who | Architect (chair), product owner, platform engineer |
| Inputs | The provider bill by `nibras/cost-line`; the `cost.json` board; this document's part 4 figures; storage growth per student against part 2.8; requested versus used CPU and memory per service class; peak versus off-peak node-hours per band; the non-infrastructure lines of part 5 (SMS volume, macOS minutes, GPU hours) |
| Measures | Cost per 1,000 students per mode and per region; the share of each cost line; idle ratio (requested but unused CPU) per service class; storage per student per data class; cost per `tenant_tier` |
| Actions and their triggers | Lower a replica's request when used CPU stays under 40% of it for 30 days; re-price the plan list when cost per 1,000 students moves more than 15% for two consecutive months; open an ADR when a measurement replaces a starting point in part 6; raise a storage-limit review when storage per student exceeds part 2.8 by 25% |
| Output | A dated record under `docs/perf/finops/<yyyy-mm>.md`: the measures, the actions with an owner and a date, and any change to this document |
| Escalation | A cost line growing faster than students for three months becomes a risk in `18-risk-register.md` |

---

## Decisions in force

| Decision | Source | Default if unanswered | Impact if wrong |
|---|---|---|---|
| Section 34's capacity values are starting points, replaced only by the scenarios in part 6 | Master brief Section 34; this document part 6 | As listed in part 2 | A size set by opinion is either idle money or a missed morning peak |
| Off-peak minimum of 2 replicas per API, 3 for Gateway | Master brief Section 34 (zone loss survivable); this document part 2.3 | 2 and 3 | One replica turns a zone loss into an outage |
| One PostgreSQL cluster for all 20 databases until the split trigger | This document part 2.6 | One cluster | Splitting early doubles the database line; splitting late makes the primary the bottleneck at the peak |
| Single-server bands with a refusal floor of 8 vCPU / 32 GB | This document part 2.2; `15-deployment-and-operations.md` §8 | As stated | A smaller host swaps under ClamAV and PostgreSQL together and fails the morning |
| Storage priced at steady state with Little's law and the Section 32 multipliers | This document part 2.8 | 6 years average remaining enrolment | The price list understates the cost that grows every year |
| Cold standby region | `15-deployment-and-operations.md` §7 | Cold | A warm standby doubles the compute and database lines |
| Unit prices are illustrative until the provider quote | Open question 15 | Part 4.1 | Every absolute figure moves; the formulas and the ratios hold |
| Shared cost is allocated to tenants by driver, never by a tenant label on shared resources | This document part 7.1 | By driver | A tenant label on shared resources breaks the cardinality guardrails and still misallocates |

## Dependencies on other documents

| This document assumes | Stated in | Checked on |
|---|---|---|
| The three deployment modes, the appliance and the cold standby | `15-deployment-and-operations.md` §2, §7, §8 | Group F review |
| The alert names used in part 3.3 | `15-deployment-and-operations.md` §5.7 | Group F review |
| Peak windows, bands and the KEDA cron trigger | `15-deployment-and-operations.md` §11 | Group F review |
| API replicas at peak, pools, `max_connections` | `21-performance-engineering.md` §5 | Group F review |
| Redis memory per tier and the idempotency lever | `21-performance-engineering.md` §2.4 | Group F review |
| Worker queues, target depths and bounds | `11-messaging-architecture.md` §7 | Group F review |
| Rung 3 hardware and its cost basis | `25-ai-and-assist-ladder.md` §9 | Group F review |
| Retention per data class | Master brief Section 32; Appendix J | Every lint run |
| Scenario names, tiers and thresholds | Appendix N | Every lint run |
| CAP-PERF-02 and CAP-SEC-01 in phase 6 | `17-roadmap.md` §4 | Group F review |

## Open points

| Question | Default | Owner | Impact if the default is wrong | L | I | Score | In the register |
|---|---|---|---|---|---|---|---|
| 1. Hosting provider, regions and real unit prices (open question 15) | The illustrative prices of part 4.1 | Product owner | Every absolute figure moves; the per-1,000 ratios between modes largely hold | 5 | 2 | 10 | RISK-28 |
| 2. Average remaining enrolment used by the retention multiplier | 6 years | Product owner, with the domain expert | At 4 years the academic multiplier falls from 16 to 14; at 8 it rises to 18 | 3 | 1 | 3 | none |
| 3. Retention of coursework submissions, which Appendix J does not name | Until leaving plus 3 years; proposed for Appendix J by ADR | Product owner, with the privacy auditor | Coursework is half of steady-state object storage; at 10 years after leaving its line nearly doubles | 3 | 2 | 6 | none |
| 4. No Appendix N scenario runs on a single server | Run the N-01 and N-02 scripts against one appliance per band in phase 6 as recorded evidence, not a release gate | Architect | A band floor that is too small fails a school's first morning with nobody having measured it | 3 | 3 | 9 | none |
| 5. The timetable solver has no load scenario | Solve time for a 3,000-student timetable recorded per release as a benchmark | Scheduling service owner | The `scheduling-worker` maximum of 4 is unproven | 3 | 2 | 6 | RISK-10 |
| 6. Erasure coding on SeaweedFS for warm volumes instead of three replicas | Three replicas (master brief Section 34) | Architect | At the scale tier storage is 34% of cost; erasure coding would cut the storage line by about half, at a rebuild-time cost | 2 | 2 | 4 | none |
| 7. Price list for owned appliance hosts | The price list quotes the rented band; an owned host is a variant (part 4.3) | Product owner | A school comparing a purchase against the rented figure sees a higher price than it pays | 2 | 1 | 2 | none |
| 8. A dedicated FinOps tool | Prometheus recording rules and a price ConfigMap; no new dependency | Architect, after the licence auditor if a tool is proposed | Allocation stays approximate for shared nodes | 2 | 1 | 2 | none |
| 9. Egress of 75 MB per student-month | As stated, measured in the first term | Platform engineer | Egress is 5% of example B; an error of two times moves the total by 5% | 3 | 1 | 3 | none |
| 10. Open question 14: a Mac build host, or hosted macOS runner minutes? Part 5 prices the Apple build line | The recorded default: hosted runner minutes, budgeted in master brief Section 30, at 80 USD a month per flavour in part 5 | Product owner | With no budget line or host, iOS is not built when phase 2 needs it; Android and mobile web are unaffected | 3 | 4 | 12 | RISK-04 |
| 11. Open question 22: which SMS provider first? Part 5 prices SMS at USD 48 per 1,000 students a month, sold as credits | The recorded default: none; email and push cover everything except the urgent fallback | Product owner | Without a provider the urgent fallback does not exist at launch; with one, SMS is the largest per-student line of part 5 and moves with each country's rate | 3 | 3 | 9 | RISK-30 |
| 12. The scale-tier off-peak row of part 2.5 (52 API replicas, 9 worker replicas, 72.1 vCPU) does not recompute from part 2.3, which gives off-peak minima only for the load tier; the load-tier peak minima with the off-peak workers give 72.6 vCPU | Kept as printed; the architect states the scale-tier off-peak replica set in part 2.3 at the next revision | Architect | None on the node count: 72.1 and 72.6 vCPU both need 13 nodes at 5.6 vCPU per node, so the compute line of part 4.5 does not move | 3 | 1 | 3 | none |

> L and I are the likelihood that the default is wrong and the impact if it is, on the 1 to 5 scales of `18-risk-register.md` Section 1. Score is L x I. A point that scores 12 or more names its RISK identifier in document 18; below that, the identifier if one covers it, or `none`. Kit-lint rules R24 and R33 check all of it (ADR-0022). Point 1 scores likelihood 5 because the prices are illustrative by construction; its impact stays 2 because replacing part 4.1 re-prices every table from the same formulas.

## Review record

| Date | Reviewer | Verdict | Blocking items |
|---|---|---|---|
| 2026-09-22 | Group F review, round 1 (independent adversarial scorecard) | Blocked: the group scored below 4 on Completeness, Consistency, Feasibility, Risk honesty, Testability and Distinctiveness | The arithmetic and the calendar-aware scaling claims had no test-case identifier (Testability) |
| 2026-09-26 | Group F review, round 2 | Blocked: the group scored below 4 on Completeness, Consistency, Risk honesty and Testability | Unchanged: "a reviewer re-running any row gets the same figure" named no identifier, and the document contained no `TC-` identifier at all (Testability) |
| 2026-09-26 | Round 3 remediation, `TC-PERF-800` run by hand | Amended; awaiting the round 3 score | Every figure of parts 2.5, 2.8, 4.3, 4.4, 4.5 and 5 was recomputed. Three differences were found and corrected: example B's compute working said × 0.329 while its figure used 240 ÷ 730; example B's year-one backup volume read 1,565.6 GB for 1,565.5 GB; the scale-tier indication carried USD 270 for network and standby where the part 4.2 formula gives USD 220, so its total is about USD 25,250 and USD 50.50 per 1,000 students. Every other figure reproduced to the cent except the scale-tier off-peak row of part 2.5 (open point 12) |

## How this document is verified

| Claim | Proof | Where it runs |
|---|---|---|
| The arithmetic of parts 2.5, 2.8, 4.3, 4.4, 4.5 and 5 | `TC-PERF-800`, defined below: every figure recomputes from the unit prices of part 4.1, the formulas of part 4.2 and the tables of part 2, to the cent. It is a review step, run by `performance-reviewer`, until the product builds a recomputation script; the round 3 run is in the review record | Group F review; again at every change to part 2, part 4 or part 5 |
| Every starting point is replaced by evidence | Each row of part 6 names its scenario and tier; the phase 6 exit (CAP-PERF-02) is not met while a row still reads "starting point" or "estimate" | Phase 6 exit review |
| The sizes hold at the scale targets of master brief Section 21 | N-01, N-05, N-06, N-07 and N-11 at the scale tier on production-shaped infrastructure (Appendix N, N.3 rule 5) | Weekly, and before every general-availability release |
| Calendar-aware scaling saves what part 2.5 claims | Node-hours per band from the cluster autoscaler against the 88 and 642 hour split; `CalendarScaleUpMissed` never fires | Monthly review, part 7.2 |
| Storage per student matches part 2.8 | Measured bytes per data class per student from N-07 at scale, then from production each month | N-07 weekly; monthly review |
| Every resource carries the FinOps labels | Chart lint and the OpenTofu plan check refuse an unlabelled resource | Every pull request touching `deploy/` |
| This document agrees with the catalogs | kit-lint R01, R02, R05 and R17 for section and appendix references, forbidden words and Mermaid types; `plan-consistency-checker` with `performance-reviewer` compares this document with documents 11, 15, 21 and 25 | kit-lint on every change under `docs/`; the comparison at the Group F review and on every change to any of them |

### Test cases

This document defines the recomputation check below. Until a script performs it, it is a review step with an identifier: `performance-reviewer` runs it at every group review and at every change to part 2, part 4 or part 5, and records the result in the review record.

| Test case | What it proves | Covers |
|---|---|---|
| TC-PERF-800 | Given the unit prices of part 4.1 (the node-hour as 240 ÷ 730, unrounded), the formulas of part 4.2 and the tables of part 2, when every figure of parts 2.5, 2.8, 4.3, 4.4, 4.5 and 5 is recomputed, then each replica, vCPU and node count of part 2.5, each total and steady state of part 2.8, and each line, total, per-1,000 and per-student-year figure of parts 4.3 to 5 equals the printed value to the cent (to one decimal for volumes and vCPU), and any difference is either corrected or carried as an open point before the review closes | none |
