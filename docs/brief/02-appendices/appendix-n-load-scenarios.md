# Appendix N. Load and Soak Scenarios

> Part of `docs/brief/02-appendices/`. Normative. Read with the master brief and the reference architecture.

Master brief Section 21 states the scale targets: **500 schools, 500,000 students, 20,000 concurrent users, the 08:00 attendance peak, 800 report cards in under 10 minutes, a 10,000-row import in under 5 minutes.** Master brief Section 19 states the performance budgets that a change may not break. A target nobody measures is a wish, so this appendix is the k6 suite that turns both into a pass or a fail. Every scenario lives in `tests/load/` as a k6 script, runs in the nightly pipeline on the tier it names, and blocks a release when its thresholds fail.

---

## N.1 Data set tiers

| Tier | Size | Purpose | Where it runs | Seeded from |
|---|---|---|---|---|
| **demo** | 600 students, 2 tenants, about 60 staff, one published timetable, one previous year of results | Smoke the scenario itself, prove a script is correct before it is expensive | Every pull request, in CI, 10-minute cap | Appendix H demo data, one-click reset |
| **load** | 50,000 students across 50 tenants, 5,000 staff, 3 time zones, 12 months of attendance partitions | The working tier: every scenario has a load run and its thresholds are the release gate | Nightly, on the performance environment | Generated from the demo shapes, deterministic seed |
| **scale** | 500,000 students across 500 tenants, 45,000 staff, 4 time zones, 3 years of partitions | Prove the Section 21 numbers themselves, and find what only breaks at the real size | Weekly, and before every general-availability release | Generated, with 5 deliberately oversized tenants of 20,000 students each |

A tenant in the load and scale tiers carries a realistic mix: 60 percent single-campus schools under 800 students, 30 percent between 800 and 3,000, 10 percent groups with two to four campuses. Currencies are distributed across SAR, AED and JOD so that rounding, tax treatment and invoice numbering are exercised in all three.

| Scenario | demo | load | scale |
|---|---|---|---|
| N-01 attendance peak | smoke | gate | gate |
| N-02 report card batch | smoke | gate | gate |
| N-03 invoice run | smoke | gate | gate |
| N-04 bulk import | smoke | gate | gate |
| N-05 principal dashboard | smoke | gate | gate |
| N-06 noisy neighbor | — | gate | gate |
| N-07 24-hour soak | — | gate | weekly |
| N-08 mobile sync storm | smoke | gate | gate |
| N-09 emergency fan-out | smoke | gate | gate |
| N-10 admissions and parent-portal surge | smoke | gate | — |
| N-11 cold-cache restart | — | gate | gate |

---

## N.2 Scenario sheets

### N-01 Morning attendance peak across time zones

| Field | Value |
|---|---|
| **Business situation** | Every school's first period starts within a 20-minute window in its own time zone. Teachers mark 25 students each, most from a phone, many over a slow connection. Guardians receive an absence push within a minute and open the app. |
| **Data set** | load, then scale |
| **Virtual users and ramp** | Four waves, one per time zone offset. Each wave: ramp 0 to 5,000 teacher VUs over 4 minutes, hold 12 minutes, ramp down 4 minutes. At scale, waves overlap for 6 minutes at the boundary, peaking at 20,000 concurrent users |
| **Duration** | 90 minutes end to end |
| **Exercised** | `GET /attendance/sessions/today`, `POST /attendance/sessions/{id}/marks` (batch of 25), `GET /scheduling/timetable/today`, the `attendance.student.absent.v1` fan-out, guardian `GET /students/{id}/today` |
| **Pass thresholds** | Mark batch write p95 under 500 ms and p99 under 1,200 ms · timetable read p95 under 80 ms (cached) · attendance read from database p95 under 250 ms · 5 or fewer database commands per mark batch · error rate under 0.1 percent excluding 409 conflicts · absence notification enqueued within 30 s of the mark for p95 · **end to end, one teacher marks one class in under 60 seconds** |
| **Failure means** | The product fails at the only minute that matters. A miss blocks the release, and the fix is a code change, never a threshold change without an ADR |

### N-02 Eight hundred report cards

| Field | Value |
|---|---|
| **Business situation** | End of term. A 2,400-student school publishes one grade level of 800 students at once while teachers keep entering marks for the next grade level. |
| **Data set** | load, then scale (10 tenants publishing simultaneously) |
| **Virtual users and ramp** | 1 batch request, plus 400 teacher VUs and 1,500 guardian VUs held flat for the whole run |
| **Duration** | 30 minutes |
| **Exercised** | `POST /assessment/report-cards/batches`, `Assessment.Worker` calculation, `Documents.Worker` PDF rendering, `assessment.report-cards.published.v1` fan-out, guardian PDF download through a signed URL |
| **Pass thresholds** | **800 report cards complete in under 10 minutes** · progress updates at least every 5 seconds · concurrent mark entry p95 stays under 500 ms while the batch runs · worker memory stable, no growth beyond 15 percent across the run · zero duplicate documents on a deliberate worker restart mid-batch · every card reproducible from its stored scheme version |
| **Failure means** | Term close becomes an overnight operation and a school does it by hand. A batch that starves interactive traffic fails even when it finishes in time |

### N-03 Five-thousand-invoice run

| Field | Value |
|---|---|
| **Business situation** | The first of the month. A group of three campuses issues 5,000 invoices in SAR, then the reminder ladder fires and parents pay. |
| **Data set** | load, then scale (20 tenants, staggered by 90 seconds) |
| **Virtual users and ramp** | 1 invoice-run request per tenant, plus 2,000 parent VUs ramping over 10 minutes on the payment path |
| **Duration** | 45 minutes |
| **Exercised** | `POST /finance/invoice-runs`, `Finance.Worker` generation, `finance.invoice.issued.v1`, notification fan-out, `POST /finance/payments`, `GET /finance/invoices/{id}` |
| **Pass thresholds** | 5,000 invoices posted in under 6 minutes · invoice numbering strictly gapless per series with zero duplicates under concurrency · payment write p95 under 500 ms · **day-close reconciliation balances to 0.00 SAR** at the end of the run · a killed worker replays without double-issuing · parent invoice read p95 under 250 ms |
| **Failure means** | Money is wrong, which is the one class of defect a school never forgives. A gap or a duplicate in the number series is an automatic fail regardless of timing |

### N-04 Ten-thousand-row import

| Field | Value |
|---|---|
| **Business situation** | Go-live day. A school uploads students, guardians and enrolments from a legacy export, runs the dry run, reads the error report, fixes 300 rows and commits. |
| **Data set** | demo tenant on a load-tier cluster, so the import competes with 50 busy tenants |
| **Virtual users and ramp** | 1 import, plus the load tier's steady background of 3,000 VUs |
| **Duration** | 25 minutes including the dry run and the commit |
| **Exercised** | `POST /documents/imports` upload, `Documents.Worker` validation job group, binary `COPY` into School, `documents.import.completed.v1`, rollback path |
| **Pass thresholds** | **10,000 rows validated and committed in under 5 minutes**, dry run in under 2 minutes · error report produced for every failing row with its row and column · rollback restores the exact prior state, proven by a checksum · other tenants see no more than 10 percent p95 degradation during the import · import memory bounded, streaming, never the whole sheet in memory |
| **Failure means** | Onboarding takes a week instead of a day, and master brief Section 12 item 9 is not true |

### N-05 Principal dashboard at first light

| Field | Value |
|---|---|
| **Business situation** | 07:40. Every principal in the region opens the "Today" dashboard on a phone before assembly, each card hitting a different read model. |
| **Data set** | load, then scale |
| **Virtual users and ramp** | 500 principal VUs at load, 5,000 at scale, arriving over 6 minutes in a burst shaped like a real morning |
| **Duration** | 20 minutes |
| **Exercised** | `GET /bff-mobile/home/principal` composing approvals, unmarked attendance, staff absences, incidents, at-risk students, overdue grading, visitors on site, from Reporting projections |
| **Pass thresholds** | Composed home p95 under 250 ms and p99 under 600 ms · every card served from cache after the first request, cache hit ratio at or above 95 percent for reference data and permissions · 5 or fewer database commands per card · projection lag under 60 seconds · mobile cold start under 3 s, 60 frames per second on a mid-range Android device |
| **Failure means** | The morning brief arrives after the bell, which removes the reason the principal opens the app at all |

### N-06 Noisy large tenant beside a quiet small one

| Field | Value |
|---|---|
| **Business situation** | A 20,000-student group runs a report-card batch, an invoice run and a 10,000-row import at the same time. A 300-student school two racks away is simply marking attendance. |
| **Data set** | load and scale, using the deliberately oversized tenants |
| **Virtual users and ramp** | Noisy tenant: 3 concurrent bulk jobs plus 4,000 VUs. Quiet tenant: 40 VUs, steady, for the whole run |
| **Duration** | 60 minutes |
| **Exercised** | Everything in N-02, N-03 and N-04 for the noisy tenant; N-01's teacher path for the quiet tenant |
| **Pass thresholds** | **The quiet tenant's p95 never rises more than 10 percent above its solo baseline** and its p99 never doubles · no quiet-tenant request is rate limited by the noisy tenant's traffic · per-tenant worker concurrency caps hold · RabbitMQ per-tenant lane depth for the quiet tenant stays under 100 messages · zero cross-tenant cache keys, asserted by a key audit during the run |
| **Failure means** | Multi-tenancy is a billing arrangement and not an architecture. This is the scenario that proves the per-tenant fairness controls exist rather than being assumed |

### N-07 Twenty-four-hour soak

| Field | Value |
|---|---|
| **Business situation** | A normal school day followed by a normal school night: morning peak, steady teaching hours, dismissal, evening parent traffic, overnight jobs, then the next morning's peak. |
| **Data set** | load nightly, scale weekly |
| **Virtual users and ramp** | A diurnal profile: 8 percent of peak overnight, 100 percent at 08:00 and 13:30, 35 percent in the evening, repeated once |
| **Duration** | 24 hours plus a 2-hour tail to observe recovery |
| **Exercised** | The whole surface: attendance, academics, messaging, finance, requests, plus the nightly reconciliation jobs, retention jobs and projection rebuilds |
| **Pass thresholds** | No memory growth above 10 percent over 24 hours in any service or worker after warm-up · no connection-pool exhaustion · no unbounded queue · dead-letter count under 0.01 percent of published messages and all of it replayable · p95 at hour 24 within 10 percent of p95 at hour 1 · reference-copy reconciliation reports zero unexplained differences · retention jobs detach the expected partitions and nothing else |
| **Failure means** | A leak that is invisible in a 20-minute test takes the platform down on a Thursday. This scenario exists because the second morning peak is where leaks surface |

### N-08 Mobile sync storm after an outage

| Field | Value |
|---|---|
| **Business situation** | A campus loses connectivity for 90 minutes. Teachers keep marking attendance, entering grades and writing notes offline. The network returns and 1,200 devices sync at once, some with conflicts. |
| **Data set** | load, then scale (8 campuses reconnecting within the same 3 minutes) |
| **Virtual users and ramp** | 1,200 device VUs at load and 9,600 at scale, all reconnecting inside a 3-minute window, each holding 40 to 400 queued operations |
| **Duration** | 30 minutes |
| **Exercised** | `POST /bff-mobile/sync/batch` with idempotency keys, delta pull, conflict resolution, `attendance.attendance.marked.v1`, low-bandwidth profile |
| **Pass thresholds** | Every queued operation applied exactly once, proven by idempotency replay returning the original result · zero lost writes · conflicts surfaced to the teacher with both values and times, never silently resolved · median device fully synced in under 45 s, p95 under 3 minutes · the API never sheds a sync batch, it queues it · a device on a 2G-class profile completes without timing out |
| **Failure means** | Offline-first is a claim rather than a feature, and a teacher loses a morning of marks. Silent conflict resolution is an automatic fail even if timings pass |

### N-09 Emergency broadcast fan-out

| Field | Value |
|---|---|
| **Business situation** | A campus raises an emergency. One broadcast must reach 3,000 guardians and 200 staff on push, SMS, in-app and email, with acknowledgement tracking and a live "who has not confirmed" view. |
| **Data set** | load, then scale (10 campuses broadcasting within the same minute, 42,000 recipients) |
| **Virtual users and ramp** | 1 broadcast per campus, plus recipient VUs acknowledging over 10 minutes on a long-tail curve |
| **Duration** | 20 minutes |
| **Exercised** | `POST /attendance/safety/broadcasts`, `attendance.emergency.broadcast-started.v1`, `Notification.Worker` channel lanes, provider adapters, `attendance.emergency.acknowledged.v1`, the live acknowledgement view |
| **Pass thresholds** | p95 recipient receives a push within 30 s of the broadcast and p99 within 90 s · SMS fallback dispatched within 120 s for recipients with no push token · the acknowledgement view lags reality by under 5 s · urgent messages bypass quiet hours and digests · a provider returning 500 for 100 percent of calls must not delay the other channels · zero duplicate deliveries to one recipient |
| **Failure means** | The safety feature in master brief Section 12 item 8 cannot be sold or, worse, is believed and is not true |

### N-10 Admissions and parent-portal surge

| Field | Value |
|---|---|
| **Business situation** | The admissions window opens at 09:00 and re-enrollment reminders go out the same morning. Guardians who are not signed in arrive together on the public application form and the payment page. |
| **Data set** | load |
| **Virtual users and ramp** | 6,000 anonymous VUs over 5 minutes, 40 percent converting to an authenticated session, plus 2,000 re-enrollment confirmations |
| **Duration** | 30 minutes |
| **Exercised** | Public application submit, file upload with scanning, `admissions.application.submitted.v1`, sign-up and guardian linking, `POST /finance/payments` for the deposit in AED |
| **Pass thresholds** | Public form submit p95 under 500 ms · upload and scan under 20 s for a 5 MB file · sign-up write p95 under 500 ms · rate limiting sheds abusive traffic without shedding real applicants, measured as under 0.5 percent false positives · seat capacity never oversubscribed under concurrency, proven by a post-run count |
| **Failure means** | A school loses applications on the one morning its funnel matters, and capacity is oversold by a race |

### N-11 Cold-cache restart under load

| Field | Value |
|---|---|
| **Business situation** | A rolling release lands at 07:55 or Redis fails over. Every cache is empty five minutes before the attendance peak. |
| **Data set** | load, then scale |
| **Virtual users and ramp** | N-01's first wave, with Redis flushed at minute 3 and one service replaced at minute 5 |
| **Duration** | 25 minutes |
| **Exercised** | The N-01 surface plus cache warm-up, the calendar-aware pre-warm from master brief Section 12.1 item 39, circuit breakers, graceful degradation |
| **Pass thresholds** | No error rate above 1 percent during the 60 seconds after the flush · p95 recovers under 250 ms within 3 minutes · no stampede: a single origin request per key, proven by counting database commands per cold key · the rolling replacement drops zero in-flight requests · with Redis fully down, the product degrades to database reads and keeps serving, at a p95 under 1,000 ms |
| **Failure means** | Every deployment becomes a maintenance window, and the platform cannot ship during a school term |

---

## N.3 How a scenario passes

1. **Thresholds are k6 thresholds, not a report read by a human.** Each scenario declares its budgets as `thresholds` in the script; k6 exits non-zero and the pipeline fails. There is no step where someone decides whether the numbers look acceptable.
2. **Budgets come from one place.** The latency, command-count and cache-ratio numbers above are the Section 19 budgets. When Section 19 changes, these scenarios change with it, and raising a budget needs an ADR.
3. **Correctness counts as a threshold.** Gapless invoice numbers, exactly-once sync, reproducible report cards, a balanced day close and zero cross-tenant cache keys are asserted after the run. A fast run that is wrong is a failed run.
4. **Every run records its tier, seed and commit**, so a regression can be bisected. A comparison against a different tier is not evidence.
5. **The scale tier runs against production-shaped infrastructure**, with the same partitioning, the same connection limits and the same per-tenant worker caps. A scale result from an oversized database proves nothing about the real one.
6. **A failed nightly blocks merges to the release branch** until either the code is fixed or an ADR records the accepted change, naming who accepted it.
