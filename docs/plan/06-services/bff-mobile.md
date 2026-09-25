# Bff.Mobile

Bff.Mobile is the only endpoint the Flutter application talks to (`09-mobile-structure.md`, `12-security-privacy-safety.md` §1.2 NETWORK). It serves the mobile screen shapes (the teacher five-minute home, the parent calm screen, the principal's morning brief), the offline sync contract of Appendix M (the outbox upload with per-action results and the delta pull keyed by the signed delta token), the remote configuration and the per-tenant version policy of master brief Section 37, device registration for push, resumable attachment upload to Documents, the realtime channel while the app is open, and an allow-listed pass-through to the owning services for the single-service mobile screens. It owns no data, publishes no event and holds no business rule: every conflict rule of Appendix M is decided by the owning service, and Bff.Mobile only carries the outcome back to the device in a shape the app can render with the banner rule.

**Group** C · **Requirement areas covered** BFF, MOB, with PERF, SEC and PRV rows that bind this host · **Last updated** 2026-09-21 by the planning session

| Fact | Value | Source |
|---|---|---|
| Application (Appendix L) | **Bff.Mobile**, "Mobile backend-for-frontend" | Appendix L.1 |
| Tier | 1 | Appendix L.1 |
| AREA code | `BFF` (shared with Bff.Web) | Appendix L.1 |
| Database | none; short-lived Redis cache on `redis-cache` and the delta-token store under `state:sync:` in `redis-state` | Appendix L.1, `21-performance-engineering.md` §1.21, §2.2 |
| Exchange | none | Appendix L.1 |
| Images | `nibras/bff-mobile` (api) | Appendix L.1 |
| Worker | none | Appendix L.1 |
| Why the boundary exists | "Release: delta sync and the version policy move with app-store releases, on a different cadence from the web." | `05-service-catalog.md` |
| Synchronous dependencies | "none as gRPC; reads services over HTTP and the Reporting read models" | Reference architecture Section 8, table 8.0 |
| Service level class | Gateway class (master brief Section 31) | Reference architecture Section 8, table 8.0 |
| Scaling profile | "Stateless; the morning sync burst follows the attendance peak" | `05-service-catalog.md` |
| Build phase | 2 (master brief Section 28) | `05-service-catalog.md` |
| Sensitivity | "passes through, stores nothing" | `05-service-catalog.md` |
| Route prefix | `/bff/mobile/v1/`, `bearerAuth` only | `22-api-conventions-and-error-catalog.md` §1.1, §11.3 |

---

## 1. Responsibilities and non-responsibilities

### Responsibilities

| Owns | Detail |
|---|---|
| Version policy | Evaluates `X-Nibras-Client` (`mobile-android/2.3.1`, `mobile-ios/2.3.1`) against the tenant's minimum and recommended versions and returns `block`, `nag` or `ok` (REQ-BFF-005, master brief Section 37) |
| Remote configuration | Feature toggles, session timeout, certificate-pin rotation, low-bandwidth thresholds and white-label flavor values from Appendix G Mobile settings, per flavor and tenant (REQ-MOB-029) |
| Mobile bootstrap and permission refresh | Identity, tenant snapshot, effective permissions with `permissionVersion`, the modes the person may enter, the entity groups their role syncs |
| Mobile role homes | Teacher five-minute home (REQ-BFF-007), parent calm screen and child today (REQ-BFF-008), student today, principal morning brief (N-05), homeroom card, gate-mode day view |
| Outbox upload | `POST /sync/batch`: up to 500 actions or 5 MB, dispatched in `queued_seq` order to the owning services with each action's idempotency key forwarded, per-action results with the Appendix K code and any discarded value for the banner; never sheds a batch (N-08) |
| Delta pull | Per entity group of `09-mobile-structure.md` §3.1, the signed delta token `v1.<tenantId>.<entityGroup>.<checkpointLsn>.<hmac>`, 30-day validity, pages of at most 500 changes and 256 KB, full refresh on an expired or rejected token (REQ-BFF-004, Appendix M.2) |
| Badge counts | Unread count per notification category returned with every sync (REQ-MOB-020) |
| Device registration | Push token register and unregister, passed to Notification; delta-token store cleared on unregister |
| Attachments | Resumable upload sessions forwarded to Documents without buffering (`09-mobile-structure.md` §3.8) |
| Realtime channel | The SignalR connection to Communication's hubs while the app is open, forwarded through this host (`09-mobile-structure.md` §4.3) |
| Pass-through for single-service screens | An allow-list of owning-service operations the app may call, forwarded unchanged with the caller's token and shaped for low bandwidth |
| Low-bandwidth mode | Image variants, sparse fields, text-first payloads and delta-only refresh when the data-saver profile is on (REQ-BFF-009) |
| API versioning towards installed apps | Keeps `/bff/mobile/v1/` working for every app version still at or above any tenant's minimum; a breaking change ships as `v2` beside it |

### Not responsible for

| Does not own | Owner instead | Why the line is here |
|---|---|---|
| Any data, event or conflict decision | The owning service (Attendance for attendance, roll call and gate verification; Assessment for mark drafts; Communication, Requests, Behavior and the others for theirs) | Appendix M.3: "Every rule below is implemented in the owning service" |
| The device's Drift store, outbox and banner rendering | The Flutter application (`09-mobile-structure.md` part 3) | The host returns outcomes; the device renders them |
| Push delivery, quiet hours, channel fallback | Notification | Bff.Mobile registers tokens only |
| File bytes, virus scan, signed download links | Documents | Uploads are forwarded as streams |
| Token issuance, refresh rotation, device-bound kiosk credentials | Identity | Bff.Mobile validates tokens like any service |
| Edge concerns: TLS, tenant resolution, per-address limits | Gateway | The mobile tenant header is accepted only from the mobile client credential (T-GW-01) |
| The web composition | Bff.Web | Different cadence and contract |
| Minimum-version and toggle values | Platform settings (Appendix G, Mobile) | Read, cached and evaluated here |
| App-store listings, signing and white-label builds | Platform operations (master brief Section 37) | Outside the running system |

---

## 2. Requirements covered

| Range or identifier | What it binds here |
|---|---|
| REQ-BFF-004, REQ-BFF-005, REQ-BFF-007, REQ-BFF-008, REQ-BFF-009 | The five Bff.Mobile rows of `03-requirements-catalog.md` |
| REQ-BFF-001, REQ-BFF-002 | No rule, no bypassing write; one call per screen |
| REQ-MOB-020, REQ-MOB-029, REQ-MOB-034 | Badge counts from the server, forced and recommended update with the outbox surviving, a school day under 2 MB |
| REQ-MOB-007, REQ-MOB-010, REQ-MOB-017 | Offline actions replay once through this host; per-student merge decided by Attendance |
| REQ-MOB-008 | Clinic, wellbeing, approvals and payments are online-only: the sync dispatcher refuses those action types |
| REQ-PERF-009, REQ-PERF-010 | Cold start under 3 s needs the home under 24 KB; scale to the 08:00 peak |
| REQ-SEC-003, REQ-SEC-005 | Caller token forwarded; generated isolation and permission suites cover every route |
| REQ-GW-007, REQ-GW-008 | Partial regions when an upstream is down; correlation id forwarded |

---

## 3. Aggregates and entities

None, by design. Bff.Mobile owns no database (Appendix L.1). The delta-token record under `state:sync:{tenant}:{userId}:{deviceId}` in `redis-state` is operational state with a 30-day expiry (`21-performance-engineering.md` §2.2), not a business entity: it holds the token identifiers issued to a device so that unregistering the device or signing out revokes them, and losing it only forces a full refresh. The token's content is carried by the device and verified by HMAC on every pull.

---

## 4. REST API: routes, aggregation endpoints and their upstream calls

All routes are under `/bff/mobile/v1/` on the tenant host, reached through the Gateway, `bearerAuth` only except the two configuration routes. Bff.Mobile declares no permission of its own; **Permission** names the upstream permission that gates the route, and every upstream call runs under the caller's forwarded token. Errors are the eight Appendix K.1 suffixes with the `BFF_` prefix for failures before any upstream is reached; upstream Appendix K codes are passed through verbatim per region or per action. Every response carries `X-Nibras-Min-Version` and `X-Nibras-Update-Policy` so the app can react on any call, not only at bootstrap.

### 4.1 Route table

| Method | Path | Permission (gate) | Request | Response | Errors | Idempotent |
|---|---|---|---|---|---|---|
| GET | `/bff/mobile/v1/config/version` | none (tenant from host or flavor) | `X-Nibras-Client` | `{ minimumVersion, recommendedVersion, policy: block \| nag \| ok, storeUrl }` | `BFF_VALIDATION_FAILED` (unparseable client header) | Safe; `ETag` |
| GET | `/bff/mobile/v1/config/remote` | none before sign-in (flavor values only); signed in for tenant values | `X-Nibras-Client`, flavor | `RemoteConfig`: toggles, session timeout, pins with overlap window, data-saver thresholds, white-label values | none beyond K.1 | Safe; `ETag` |
| GET | `/bff/mobile/v1/me/bootstrap` | signed in | `If-None-Match` | `MobileBootstrap`: user, tenant snapshot, effective permissions and version, permitted modes, entity groups for the role | `BFF_TENANT_MISMATCH`, `BFF_DEPENDENCY_UNAVAILABLE` (Identity down) | Safe |
| GET | `/bff/mobile/v1/me/permissions` | signed in | `If-None-Match` | Effective set, `ETag` = `permissionVersion` | `BFF_DEPENDENCY_UNAVAILABLE` | Safe; `304` |
| POST | `/bff/mobile/v1/me/devices` | signed in | `{ deviceId, platform, pushToken, pushStrategy: fcm \| apns \| none, appVersion }` | 200 device registered | none beyond K.1 | Yes, by `deviceId` |
| DELETE | `/bff/mobile/v1/me/devices/{deviceId}` | signed in (own device) | none | 204; push token unregistered, delta tokens revoked | none beyond K.1 | Yes |
| GET | `/bff/mobile/v1/home/teacher` | `academics.teaching-assignments.view` | `date` | `TeacherFiveMinuteHome`: attendance to mark, a quick note, a quick grade, the cover alert, nothing else | none beyond K.1; regions may be `partial` | Safe |
| GET | `/bff/mobile/v1/home/homeroom` | `attendance.student-attendance.view` (own-homeroom) | `date` | Absent today, excuses to review, flags, birthdays | as above | Safe |
| GET | `/bff/mobile/v1/home/student` | `scheduling.timetable.view` (self) | `date` | Timetable, due list, new feedback | as above | Safe |
| GET | `/bff/mobile/v1/home/parent` | `school.students.view` (own-children) | none | `CalmScreen`: one card per child per day, urgent items that break through, "nothing needs your attention" as a designed state | as above | Safe |
| GET | `/bff/mobile/v1/home/parent/children/{studentId}` | `school.students.view` (own-children) | `date` | `ChildToday` including today's gate pass and attendance | `BFF_NOT_FOUND` (not the caller's child) | Safe |
| GET | `/bff/mobile/v1/home/principal` | `reporting.dashboards.view` | `lens` | `MorningBrief`: approvals, unmarked attendance, staff absences and cover, incidents today, at-risk students, overdue grading, visitors on site | as above | Safe |
| GET | `/bff/mobile/v1/modes/gate/today` | `attendance.safety.gate-passes.verify` | `campusId` | Expected pickups with collector photos, the campus's authorised pickup lists, verification keys, visitors on site | none beyond K.1 | Safe |
| GET | `/bff/mobile/v1/modes/bus/today` | `attendance.student-attendance.mark` plus the Operations transport read | `routeId` | Today's route, stops in order, roster per stop with thumbnails | `BFF_NOT_FOUND` (Operations not deployed for the tenant) | Safe |
| POST | `/bff/mobile/v1/sync/batch` | signed in; each action gated by its upstream permission | `SyncBatch`: up to 500 actions or 5 MB, each `{ idempotencyKey, actionType, batchId, entityId, entityVersion, occurredAt, queuedSeq, payload }` | 200 `SyncBatchResult`: per action `accepted \| merged \| conflict \| review-created \| rejected \| deferred`, `receivedAt`, the first response, the Appendix K code, discarded value and resolving action; unread counts per category | `BFF_VALIDATION_FAILED` (over 500 actions or 5 MB, unknown action type) | Yes: every action by its idempotency key upstream; the batch itself is safe to resend |
| POST | `/bff/mobile/v1/sync/pull` | signed in; each group gated by its upstream view permission | `{ groups: [ { entityGroup, deltaToken or null } ], dataSaver }` | 200 per group: `{ upserts, deletes, discarded, nextToken, hasMore, asOf, fullRefresh }`; unread counts per category | `BFF_VALIDATION_FAILED` on a tampered, wrong-tenant or expired token (the device performs a full refresh) | Safe |
| POST | `/bff/mobile/v1/uploads` | signed in; the owning action's permission applies when the action lands | `{ attachmentId, contentType, sizeBytes }` | 201 `{ uploadUrl, chunkSize }` from Documents | `BFF_VALIDATION_FAILED` (size over the per-action limit) | Yes, by `attachmentId` |
| PATCH | `/bff/mobile/v1/uploads/{attachmentId}` | signed in (own session) | chunk with `Content-Range` | 204 with bytes received | none beyond K.1; Documents codes passed through | Yes, by range |
| HEAD | `/bff/mobile/v1/uploads/{attachmentId}` | signed in (own session) | none | `bytes_uploaded` header for resumption | `BFF_NOT_FOUND` | Safe |
| GET | `/bff/mobile/v1/students/{studentId}/360` | `school.students.view` | `tab` | Student 360 for principals and homeroom teachers, read live, never cached on the device beyond the screen | `BFF_NOT_FOUND` (outside scope, no hint) | Safe |
| GET | `/bff/mobile/v1/students/{studentId}/transparency` | `audit.access-transparency.view` (own-children) | none | Reads by role and time, consents, retention clocks; read live | `BFF_NOT_FOUND` | Safe; never cached |
| GET | `/bff/mobile/v1/hubs/{hub}` | signed in | WebSocket upgrade | Forwarded SignalR connection to Communication's hub (`messaging`, `notifications`, `jobs`, `permissions`) | `BFF_VALIDATION_FAILED` (unknown hub) | not applicable |
| ANY | `/bff/mobile/v1/api/{service}/{**path}` | the forwarded operation's own Appendix B permission | The owning service's request, unchanged | The owning service's response, with data-saver shaping applied to images and sparse fields | `BFF_NOT_FOUND` for an operation not on the mobile allow-list; upstream codes passed through | As the upstream operation |

**Route count: 23**, of which the last is the allow-listed pass-through described in 4.4.

### 4.2 Upstream calls behind each aggregation endpoint

Attendance and Assessment paths are quoted from their sheets; other services' resources are named by service and pinned by the consumer pacts in `tests/Nibras.Bff.Mobile.Tests/Contracts/`.

| Endpoint | Upstream calls (service → resource) | Notes |
|---|---|---|
| `config/version`, `config/remote` | Platform → tenant settings, Mobile group (minimum supported version, forced update, feature toggles, white-label flavor values); Platform → flavor registry for the host | Platform-scoped entry per flavor, tenant entry per tenant |
| `me/bootstrap`, `me/permissions` | Identity → user and effective permissions; Platform → tenant snapshot; School → the person's sections or children | as Bff.Web |
| `me/devices` | Notification → register and unregister the device token | Also clears `state:sync:{tenant}:{userId}:{deviceId}` on delete |
| `home/teacher` | Attendance → `GET /api/v1/attendance/sessions` (attendance to mark, next class) and `GET /api/v1/attendance/offline-reviews?mine=true`; Scheduling → cover assigned today; Academics → submissions to grade count; Communication → unread messages count | Four regions only, as REQ-BFF-007 fixes |
| `home/homeroom` | Reporting → absent today and flags; Attendance → `GET /api/v1/attendance/excuses?status=Submitted&sectionId=`; School → birthdays | as Bff.Web |
| `home/student` | Scheduling → today's timetable; Academics → due soon and new feedback | |
| `home/parent` and `children/{id}` | School → linked children; per child in parallel: Reporting → today's status from `student_360`; Attendance → `GET /api/v1/attendance/students/{id}/summary` and today's gate pass from `GET /api/v1/attendance/safety/gate-passes?date=`; Academics → due items; Finance → balance due; Requests → open requests; Communication → unread | Urgent items (absence today, gate pass issued, emergency) break through the calm state |
| `home/principal` | Requests → approvals; Attendance → `GET /api/v1/attendance/sessions/unmarked`, `GET /api/v1/attendance/safety/visitors?status=CheckedIn`; Scheduling → staff absences and cover; Behavior → incidents today; Reporting → at-risk students and overdue grading | N-05 shape |
| `modes/gate/today` | Attendance → `GET /api/v1/attendance/safety/gate-passes?campusId=&date=&status=Issued`, `GET /api/v1/attendance/safety/pickup-persons` for the campus's students, `GET /api/v1/attendance/safety/gate-passes/keys`, `GET /api/v1/attendance/safety/visitors?status=CheckedIn` | Watchlist never included (Open point 3) |
| `modes/bus/today` | Operations → today's route and roster (Tier 2) | Boarding taps upload through `sync/batch` as attendance actions with source `bus` |
| `students/{id}/360`, `transparency` | as Bff.Web section 4.2 | read live |
| `uploads` | Documents → resumable upload session create, chunk, status | Streamed, never buffered in memory beyond one chunk |
| `hubs/{hub}` | Communication → SignalR hub | Forwarded with the caller's token |

### 4.3 The sync contract

**Upload (`sync/batch`).** The dispatcher walks the actions in `queuedSeq` order, groups consecutive actions of the same type and target (for example the 25 marks of one mark-all-present batch) into one upstream call where the upstream accepts a batch, forwards each action's `idempotencyKey` as the upstream `Idempotency-Key` (per action in a batch body), stamps nothing itself (the owning service stamps `receivedAt`), and maps each upstream outcome to one per-action result. A replayed action returns the upstream's first response (`_IDEMPOTENCY_REPLAY` or `ATTENDANCE_DUPLICATE_MARK`, both 200) and the app stores it in `server_response_json`.

The `actionType` values are the device outbox names of `09-mobile-structure.md` §3.2 (`outbox_actions.action_type`), extended in the same dotted style; they are message names between the app and this host, not Appendix B permissions, and the permission applied is always the upstream operation's.

| `actionType` (device outbox) | Upstream operation | Conflict rule decided upstream (Appendix M.3) |
|---|---|---|
| `attendance.mark` | Attendance `POST /api/v1/attendance/sessions/sync` | Device wins before lock; after lock a review is created; two devices merge per student by `receivedAt` |
| `attendance.edit-after-lock.submit` | Attendance `POST /api/v1/attendance/offline-reviews/{id}/submit` | not a conflict |
| `attendance.excuse.submit` | Attendance `POST /api/v1/attendance/excuses` | Append-only; duplicates within 5 minutes collapse |
| `attendance.check-in` | Attendance `POST /api/v1/attendance/check-ins` | Append-only; duplicate within 1 minute ignored |
| `attendance.gate-pass.verify` | Attendance `POST /api/v1/attendance/safety/gate-passes/verify` with `verifiedOffline` | First verification to reach the server wins; later ones reported with the time |
| `attendance.emergency.acknowledge` | Attendance `POST /api/v1/attendance/safety/emergency/broadcasts/{id}/acknowledgments` | Earliest `occurredAt` wins; duplicates collapse |
| `attendance.roll-call.record` | Attendance `POST /api/v1/attendance/safety/emergency/broadcasts/{id}/roll-call` | as above |
| `attendance.visitor.check-in` | Attendance `POST /api/v1/attendance/safety/visitors/check-in` | Append-only; watchlist matched on arrival |
| `assessment.marks.draft` | Assessment `POST /api/v1/assessment/marks/sync` | Device wins while unapproved; approved or locked returns a proposed grade change |
| `communication.message.send`, `communication.receipt.read`, `communication.policy.acknowledge` | Communication message, receipt and acknowledgment operations | Append-only; earliest `occurredAt` wins for receipts |
| `requests.request.submit`, `requests.task.complete` | Requests request and task operations | Append-only with 5-minute collapse; first completion wins |
| `behavior.points.award`, `behavior.incident.record`, `behavior.note.record` | Behavior operations | Additive or append-only |
| `academics.submission.submit`, `academics.assignment.publish` | Academics operations | Append-only; attachment released first |
| `notification.preference.change` | Notification preference operation | Last `receivedAt` wins |
| `operations.library.*`, `operations.inventory.*`, `operations.transport.boarding` | Operations operations (Tier 2) | Append-only |

Action types for clinic visits, wellbeing records, approvals of any kind and payments are refused by the dispatcher with `BFF_VALIDATION_FAILED` and `params.reason = "onlineOnly"` (Appendix M.1, REQ-MOB-008); the app never queues them, so a refusal here means a defective client.

**Never shed.** When an upstream's circuit is open or it answers 429 or 503, the affected actions return `deferred` with `retryAfter`, and the rest of the batch proceeds; the device keeps deferred actions pending with their original keys (N-08: "the API never sheds a sync batch, it queues it"). The batch route itself is exempt from the per-user rate limit of the Web building block and has its own concurrency limit per device of one batch in flight.

**Pull (`sync/pull`).** One request carries every entity group the device holds. For each group, Bff.Mobile verifies the token's HMAC with the tenant's sync key from the secret store, checks tenant and 30-day age, asks the group's source for changes since the checkpoint, and issues a new token over the source's next checkpoint. A missing, tampered, wrong-tenant or expired token yields `fullRefresh = true` and a snapshot page instead of a delta (Appendix M.4: "the application performs a full refresh for that entity group rather than applying a stale delta").

| Entity group (`09-mobile-structure.md` §3.1) | Source of changes | Checkpoint carried in the token |
|---|---|---|
| `home` | The role home composition above | Composition time; always a snapshot, at most 24 KB |
| `timetable` | Scheduling → the person's sessions, today plus 14 days back and forward | Scheduling change checkpoint |
| `roster` | School → students per section the person teaches or parents; Attendance → `GET /api/v1/attendance/safety/pickup-persons` for guardians' own children | School and Attendance checkpoints, combined |
| `attendance` | Attendance → `GET /api/v1/attendance/sessions/changes?checkpoint=` | Attendance `nextCheckpoint` |
| `messages` | Communication → threads and announcements of the last 30 days, attachment metadata only | Communication checkpoint |
| `requests` | Requests → the person's requests and approvals listing (read only) | Requests checkpoint |
| `behaviour` | Behavior → points and incidents the person recorded, badges for own children | Behavior checkpoint |
| `documents` | Assessment → `GET /api/v1/assessment/students/{id}/report-cards` metadata; Documents → certificate metadata; bytes are downloaded on demand through the pass-through | Composition time; snapshot |
| `gate_keys` | Attendance → `GET /api/v1/attendance/safety/gate-passes/keys` | Key set version |
| `permissions` | Identity → effective permissions | `permissionVersion` |

### 4.4 The pass-through allow-list

The pass-through exists because the app talks to Bff.Mobile only while most mobile screens read one service. It forwards unchanged, applies data-saver shaping, and refuses anything not on the list, which is generated from the mobile feature list of `09-mobile-structure.md` part 2 and reviewed with every app release.

| Mobile feature (document 09 part 2) | Allowed upstream operations |
|---|---|
| Register, excuse review read, my attendance, gate pass display, pickup persons, visitors, emergency mode | Attendance: the `GET` routes of its sections 4.1, 4.2, 4.7, 4.9 to 4.12, `POST /api/v1/attendance/safety/emergency/broadcasts` (online only, network required), `POST .../reunifications`, `POST /api/v1/attendance/excuses/{id}/approve` and `/reject` (online only) |
| Grades and feedback, mark entry grid, report cards | Assessment: `GET` grid, results, report-card versions and document link; `PUT` grid marks online |
| Timetable and cover | Scheduling: timetable and substitution reads; cover accept or decline online |
| Messages, meetings, announcements, policies | Communication: thread, message and meeting operations |
| Requests, approvals inbox | Requests: request and task operations; approve and reject online |
| Fees and pay | Finance: invoice and receipt reads; payment initiation online only through the provider redirect |
| Clinic, medication, allergy, referrals (nurse and care modes) | Wellbeing: the online-only operations of its sheet; responses carry `Cache-Control: no-store` and are never cached here |
| Assignments and submissions | Academics: assignment reads, submission create |
| Behavior quick note | Behavior: point and incident operations |
| Notification preferences | Notification: preference operations |
| Documents download | Documents: signed download link creation |

### 4.5 Response shaping

| Rule | Detail |
|---|---|
| Regions | Home payloads use the region envelope of Bff.Web (`key`, `status`, `asOf`, `data`, `problem`, `action`) so the seven states render the same way |
| Size | Home under 24 KB after Brotli; delta pages at most 500 changes and 256 KB (`21-performance-engineering.md` §10) |
| Data saver | `dataSaver: true` or the `X-Nibras-Data-Saver: on` header: thumbnails at 96 px instead of 320 px, no avatars in lists, text-first notification detail, `fields=` sparse projection on pass-through reads (REQ-BFF-009) |
| Banner inputs | Every `conflict` or `review-created` result carries `fieldName`, `deviceValue`, `serverValue`, `ruleApplied` and `resolvingAction` exactly as the device's `conflicts` table stores them (`09-mobile-structure.md` §3.2) |
| Badge counts | `unreadCounts` per category (urgent, academic, finance, requests, messages) on every batch and pull response |
| No content in pushes | Unchanged from Notification; the app fetches content after unlock through this host |

### 4.6 Version policy

| Rule | Detail |
|---|---|
| Inputs | `X-Nibras-Client` platform and semantic version; tenant settings (Appendix G, Mobile: minimum supported version, forced update, feature toggles) |
| Outcome | `block` below the tenant minimum; `nag` within one minor version of it; `ok` otherwise (master brief Section 37). The outcome is returned by `config/version` and in two headers on every response |
| What the server never does | Refuse a request because of the app version. Master brief Section 37: "A server change that breaks an older app is forbidden"; the app blocks itself and preserves its outbox, and an action that arrives from a blocked version is still applied by its idempotency key |
| Retiring an API version | `/bff/mobile/v1/` stays while any tenant's minimum version is at or below the last app release that uses it; `v2` ships beside it and `v1` is removed only after every tenant minimum has moved past it, announced through the deprecation headers of `23-integrations-and-public-api.md` |
| Unreachable policy | The app applies its last policy; a fresh install with no policy proceeds (`09-mobile-structure.md` §7) |

---

## 5. gRPC

None, exposed or consumed, by design (`22-api-conventions-and-error-catalog.md` §10.4: the backends-for-frontends call services over HTTP and make no gRPC calls). Upstream HTTP policy:

| Aspect | Value |
|---|---|
| Deadline per upstream call | 400 ms for home regions; 2 s per sync dispatch group; 5 s per delta source page; uploads streamed with no total deadline beyond the Gateway's |
| Retries | One retry on connection failure or 503 for `GET` and for sync dispatch (safe because every action carries its idempotency key) |
| Circuit breaker | Per upstream service; an open breaker returns home regions `unavailable` and sync actions `deferred` |
| Bulkhead | 64 concurrent calls per upstream per instance; one batch in flight per device |

---

## 6. Events published and consumed

None, by design. Bff.Mobile owns no exchange and no queue (`11-messaging-architecture.md` part 1). Silent pushes that trigger a sync are sent by Notification, not here. Cache eviction arrives through the `state:invalidate` pub/sub broadcast of `Nibras.BuildingBlocks.Caching` (`21-performance-engineering.md` §2.6), as for Bff.Web.

---

## 7. Sagas and workflows

None owned. Bff.Mobile carries the offline legs of WF-ATT-01 (marks and the edit-after-lock review), WF-ATT-02 (gate verification in gate mode) and WF-ASM-01 (mark drafts), and the emergency roll call of REQ-ATT-032 to REQ-ATT-034; every transition is taken by the owning service when the action lands.

---

## 8. Local reference copies

None, by design. What looks like a copy lives on the device (the Drift entity groups), and it is refreshed through this host from the owners.

---

## 9. Background jobs

None. Silent-push triggers, digest schedules and the pre-peak warm-up belong to Notification and Platform. The only time-based behaviour here is the 30-day expiry of `state:sync:` records, enforced by Redis.

---

## 10. Permissions, notifications, settings, error codes

| Permissions | Bff.Mobile declares none of its own and holds none. The gate column of section 4.1 and the allow-list of 4.4 name the Appendix B permission of the upstream operation; the generated permission-matrix suite exercises every route and every allow-listed operation with every Appendix I role |
|---|---|

| Notifications | none triggered |
|---|---|

| Settings read (Appendix G) | Where it goes |
|---|---|
| Mobile → minimum supported version, forced update, feature toggles, white-label flavor values | `config/version`, `config/remote`, response headers |
| Security → session timeout, login methods | `config/remote` |
| General → languages, numerals, time zone, calendars, terminology, branding | `me/bootstrap` |
| Notifications → channel availability | `config/remote` (which categories the app registers) |
| Safety → gate pass validity, pickup verification method | `modes/gate/today` |

| Error codes | HTTP | When |
|---|---|---|
| `BFF_VALIDATION_FAILED` | 400 | Batch over 500 actions or 5 MB, unknown or online-only action type, tampered or expired delta token (REQ-BFF-004), unparseable client header |
| `BFF_PERMISSION_DENIED` | 403 | Route gate refused before any upstream call |
| `BFF_TENANT_MISMATCH` | 403 | Tenant header or delta token tenant differs from the token tenant |
| `BFF_NOT_FOUND` | 404 | Operation not on the pass-through allow-list; child or student outside scope; unknown upload session |
| `BFF_CONCURRENCY_CONFLICT` | 409 | A second batch from the same device while one is in flight (`Retry-After: 2`) |
| `BFF_IDEMPOTENCY_REPLAY` | 200 | Not raised by the host itself; upstream replays pass through per action |
| `BFF_RATE_LIMITED` | 429 | Per-user limits on pass-through and home routes; never on `sync/batch` |
| `BFF_DEPENDENCY_UNAVAILABLE` | 503 | Only for bootstrap without Identity; sync actions defer instead |
| `BFF_APP_VERSION_BELOW_MINIMUM` | 403 | Never raised here: master brief Section 37 forbids refusing a request because of the app's version. Appendix K.23 defines it with the client action this sheet's `block` outcome already produces, an upgrade screen that keeps the outbox; the policy travels in `config/version` and the two response headers instead |

---

## 11. Caching and hot queries

The entries are `21-performance-engineering.md` §1.21: role home payload per user (15 s and 60 s), bootstrap per user and permission version (60 s and 30 min), remote configuration and version policy per flavor (platform-scoped, 60 s and 1 h), and the delta-token store in `redis-state` (30 days, a store and not a cache). What this sheet adds:

| Data | Key | Tags | L1 | L2 | Invalidated by | Never cached |
|---|---|---|---|---|---|---|
| Tenant version policy | `nibras:{tenant}:bff:version-policy:v1` | `tenant` | 60 s | 1 h ± 10% | `state:invalidate` broadcast for tag `tenant` after `platform.settings.changed.v1` with the Mobile scope | Nothing |
| Gate-mode day view per campus | `nibras:{tenant}:bff:gate-day:{campusId}:{date}:v1` | `tenant`, `campus` | 15 s | 60 s ± 10% | Lifetime; broadcast for tag `campus` from Attendance on pass issue, use and revoke | Watchlist, visitor identity references, pass codes |
| Delta source checkpoint per group and person | none; the checkpoint travels in the token | not applicable | none | none | not applicable | Delta pages are never cached, they are per person and per checkpoint |

**Never cached in Bff.Mobile:** delta pages, sync results, uploads, pass-through responses (the owning service's `ETag` is forwarded instead), Student 360, the transparency panel, anything from Wellbeing, anything carrying a signed URL or a gate-pass code.

**Hot paths and budgets.** No query runs here. The budgets are Appendix N N-08 (1,200 devices reconnecting in 3 minutes at load, 9,600 at scale; every queued operation applied exactly once; median device synced under 45 s, p95 under 3 minutes; never shed), N-05 (principal morning brief p95 under 250 ms, p99 under 600 ms), the 24 KB home and 256 KB delta page (`21-performance-engineering.md` §10), and a school day of teacher use under 2 MB (TC-MOB-715). The alert is `MobileSyncRejections`: the batch route rejecting over 1 percent of batches for 5 minutes (`15-deployment-and-operations.md`).

---

## 12. Security

Threat rows: `12-security-privacy-safety.md` §2.21 T-GW-01 (tenant header accepted only from the mobile client credential) and T-GW-04 (no field the user may not see), and the MASVS NETWORK row of §1.2 (Bff.Mobile is the only endpoint the app talks to; optional certificate pinning with rotation through `config/remote`).

| Data class (Appendix J) | Handling here |
|---|---|
| Public | Flavor configuration and gate verification public keys |
| Internal | Timetable, reference data, permissions |
| Confidential | Home regions, delta pages, roster and pickup lists, sync results: per-user, never stored beyond the short cache entries of section 11 |
| Sensitive | Never forwarded into any entity group; passes through only on online-only pass-through reads with `no-store` (for example a guardian's own gate-pass code) |
| S (Wellbeing) | Online-only pass-through with `no-store`; never in a delta, a home region beyond counts, a log or a cache |

| Never | What |
|---|---|
| Cached | The items listed under "Never cached" in section 11 |
| Logged | Request and response bodies, action payloads, delta tokens, push tokens; logs carry route, action type, result status, Appendix K code, tenant id and correlation id |
| Sent to a device | Anything at Sensitive or level S in a synced entity group (Appendix M.1: "The device cache holds nothing classified sensitive in Appendix J"); the watchlist; another family's data on a personal device |

The delta token is signed with a per-tenant HMAC key held in the secret store and rotated with an overlap window; a token signed with a retired key is treated as expired and forces a full refresh, never an error the person must handle. Kiosk devices authenticate with the device-bound credential issued by Identity, scoped to `attendance.student-attendance.mark` (kiosk source) and `attendance.safety.visitors.check-in` (`09-mobile-structure.md` part 6).

---

## 13. Folder and file tree

Document 07 §2.1 gives this host one project and one test project; the tree follows the Attendance anatomy where it applies, lists the data-owner folders as absent, and is to file level.

```text
src/Bff.Mobile/                                                   mobile backend-for-frontend, image nibras/bff-mobile
├── Nibras.Bff.Mobile/                                            delta sync endpoints keyed by change token, remote configuration, version policy
│   ├── Nibras.Bff.Mobile.csproj                                  references Nibras.ServiceDefaults, Nibras.BuildingBlocks.* and Nibras.Contracts.Shared only
│   ├── README.md                                                 purpose, routes, sync contract, allow-list, budgets, runbook links
│   ├── Program.cs                                                composition root: ServiceDefaults, Web, Caching, upstream clients, YARP forwarder, endpoints, probes
│   ├── Features/                                                 one folder per route group: Query or Command, Handler, Validator, Endpoint
│   │   ├── GetVersionPolicy/                                     GET /bff/mobile/v1/config/version
│   │   │   ├── GetVersionPolicyQuery.cs                          record: client platform and version, tenant
│   │   │   ├── GetVersionPolicyHandler.cs                        compares with the tenant minimum and recommended versions
│   │   │   ├── GetVersionPolicyValidator.cs                      X-Nibras-Client format
│   │   │   └── GetVersionPolicyEndpoint.cs                       route, ETag
│   │   ├── GetRemoteConfig/                                      GET /bff/mobile/v1/config/remote
│   │   │   ├── GetRemoteConfigQuery.cs                           record: flavor, tenant, signed-in flag
│   │   │   ├── GetRemoteConfigHandler.cs                         flavor values platform-scoped, tenant toggles and pins per tenant
│   │   │   ├── GetRemoteConfigValidator.cs                       known flavor
│   │   │   └── GetRemoteConfigEndpoint.cs                        route, ETag
│   │   ├── GetMobileBootstrap/                                   GET /bff/mobile/v1/me/bootstrap and /me/permissions
│   │   │   ├── GetMobileBootstrapQuery.cs                        record: caller, If-None-Match
│   │   │   ├── GetMobileBootstrapHandler.cs                      Identity, Platform, School reads; permitted modes and entity groups for the role
│   │   │   ├── GetMobileBootstrapValidator.cs                    tenant header equals token tenant
│   │   │   └── GetMobileBootstrapEndpoint.cs                     routes
│   │   ├── ManageDevices/                                        POST /me/devices and DELETE /me/devices/{deviceId}
│   │   │   ├── ManageDeviceCommand.cs                            record: register or unregister with device id and push token
│   │   │   ├── ManageDeviceHandler.cs                            forwards to Notification; clears the state:sync records on unregister
│   │   │   ├── ManageDeviceValidator.cs                          push strategy fcm, apns or none
│   │   │   └── ManageDeviceEndpoint.cs                           routes
│   │   ├── GetMobileHome/                                        GET /bff/mobile/v1/home/{role}
│   │   │   ├── GetMobileHomeQuery.cs                             record: role, lens, date, child id, data saver
│   │   │   ├── GetMobileHomeHandler.cs                           picks the composer, reads and writes the home cache entry
│   │   │   ├── GetMobileHomeValidator.cs                         role known, child is the caller's
│   │   │   └── GetMobileHomeEndpoint.cs                          the six home routes
│   │   ├── GetModeDay/                                           GET /bff/mobile/v1/modes/gate/today and /modes/bus/today
│   │   │   ├── GetModeDayQuery.cs                                record: mode, campus or route, date
│   │   │   ├── GetModeDayHandler.cs                              gate: passes, pickup lists, keys, visitors; bus: route and roster
│   │   │   ├── GetModeDayValidator.cs                            caller holds the mode's permission
│   │   │   └── GetModeDayEndpoint.cs                             routes
│   │   ├── SyncBatch/                                            POST /bff/mobile/v1/sync/batch
│   │   │   ├── SyncBatchCommand.cs                               record: up to 500 actions or 5 MB in queuedSeq order
│   │   │   ├── SyncBatchHandler.cs                               dispatches in order, groups per upstream, maps outcomes, defers on an open breaker
│   │   │   ├── SyncBatchValidator.cs                             limits, known and offline-allowed action types, 64 KB per action
│   │   │   └── SyncBatchEndpoint.cs                              route, one batch in flight per device
│   │   ├── SyncPull/                                             POST /bff/mobile/v1/sync/pull
│   │   │   ├── SyncPullCommand.cs                                record: entity groups with tokens, data saver
│   │   │   ├── SyncPullHandler.cs                                verifies tokens, asks each source for changes or a snapshot, issues new tokens
│   │   │   ├── SyncPullValidator.cs                              known entity groups
│   │   │   └── SyncPullEndpoint.cs                               route
│   │   ├── Uploads/                                              POST, PATCH, HEAD /bff/mobile/v1/uploads
│   │   │   ├── UploadCommand.cs                                  record: create, chunk or status for an attachment id
│   │   │   ├── UploadHandler.cs                                  streams to the Documents resumable session, never buffers beyond one chunk
│   │   │   ├── UploadValidator.cs                                content type allow-list, size within the per-action limit
│   │   │   └── UploadEndpoint.cs                                 routes
│   │   ├── GetStudent360/                                        GET /bff/mobile/v1/students/{id}/360 and /transparency
│   │   │   ├── GetStudent360Query.cs                             record: student id, tab, or transparency
│   │   │   ├── GetStudent360Handler.cs                           same composition as Bff.Web, read live, no-store
│   │   │   ├── GetStudent360Validator.cs                         scope
│   │   │   └── GetStudent360Endpoint.cs                          routes
│   │   └── PassThrough/                                          ANY /bff/mobile/v1/api/{service}/{**path} and the hub route
│   │       ├── PassThroughAllowList.cs                           the mobile operation allow-list of section 4.4, generated from document 09 part 2
│   │       ├── PassThroughHandler.cs                             YARP forwarding with the caller's token and data-saver shaping
│   │       ├── HubForwarder.cs                                   WebSocket forwarding to Communication's hubs
│   │       └── PassThroughEndpoint.cs                            routes
│   ├── Sync/                                                     the sync contract, independent of any feature
│   │   ├── DeltaToken.cs                                         value object: v1.<tenantId>.<entityGroup>.<checkpointLsn>.<hmac>, 30-day validity
│   │   ├── DeltaTokenSigner.cs                                   HMAC with the per-tenant key from the secret store, key rotation with overlap
│   │   ├── DeltaTokenStore.cs                                    state:sync:{tenant}:{userId}:{deviceId} records for revocation
│   │   ├── EntityGroupRegistry.cs                                the ten entity groups and their change sources
│   │   ├── ActionDispatcher.cs                                   actionType to upstream operation map of section 4.3
│   │   ├── OutcomeMapper.cs                                      upstream result and Appendix K code to accepted, merged, conflict, review-created, rejected, deferred
│   │   └── BannerFields.cs                                       fieldName, deviceValue, serverValue, ruleApplied, resolvingAction for the device conflicts table
│   ├── Composition/                                              the mobile screen composers; fan-out, filtering, shaping only
│   │   ├── TeacherFiveMinuteComposer.cs                          attendance to mark, quick note, quick grade, cover alert
│   │   ├── HomeroomComposer.cs                                   absent today, excuses to review, flags, birthdays
│   │   ├── StudentTodayComposer.cs                               timetable, due list, new feedback
│   │   ├── CalmScreenComposer.cs                                 one card per child, break-through urgent items, the designed empty state
│   │   ├── ChildTodayComposer.cs                                 one child's day with gate pass and attendance
│   │   ├── MorningBriefComposer.cs                               principal cards of Appendix D
│   │   ├── GateDayComposer.cs                                    gate-mode day view without watchlist or codes
│   │   ├── BusDayComposer.cs                                     route and roster for the bus attendant
│   │   └── Region.cs                                             the region envelope shared with Bff.Web's shape
│   ├── VersionPolicy/                                            version evaluation
│   │   ├── ClientVersion.cs                                      value object parsed from X-Nibras-Client
│   │   ├── VersionPolicyEvaluator.cs                             block below minimum, nag within one minor, ok otherwise
│   │   └── VersionHeadersMiddleware.cs                           X-Nibras-Min-Version and X-Nibras-Update-Policy on every response
│   ├── Shaping/                                                  low-bandwidth mode
│   │   ├── DataSaverProfile.cs                                   thresholds from remote configuration
│   │   └── ImageVariantRewriter.cs                               rewrites image references to the thumbnail variant
│   ├── Upstream/                                                 typed HTTP clients, one per service, generated from each service's OpenAPI
│   │   ├── Generated/                                            generator output, read-only, one folder per service (twenty)
│   │   ├── UpstreamRegistry.cs                                   base addresses from service discovery, deadlines per endpoint class
│   │   └── FanOut.cs                                             parallel execution with deadline, bulkhead and partial results
│   ├── Resilience/                                               upstream policies from the Web building block
│   │   └── UpstreamPolicies.cs                                   retry once, breaker per upstream, bulkhead, per-device batch lock
│   ├── Security/                                                 propagation only
│   │   ├── TokenForwardingHandler.cs                             forwards the caller's bearer token unchanged
│   │   ├── TenantPropagationHandler.cs                           accepts the tenant header from the mobile client credential only (T-GW-01)
│   │   └── CorrelationHandler.cs                                 forwards the correlation id and traceparent
│   ├── Caching/                                                  what this host caches and what evicts it
│   │   ├── BffMobileCacheKeys.cs                                 keys and tags of document 21 §1.21 and section 11
│   │   └── InvalidationSubscriber.cs                             state:invalidate subscriber
│   ├── Contracts/                                                DTOs published as the Bff.Mobile OpenAPI document the Flutter clients are generated from
│   │   ├── SyncDtos.cs                                           batch, action result, pull request and page
│   │   ├── HomeDtos.cs                                           one DTO per mobile home
│   │   └── ConfigDtos.cs                                         version policy and remote configuration
│   ├── Observability/                                            metrics beyond ServiceDefaults
│   │   └── SyncMetrics.cs                                        batches, actions by result, deferrals, rejections, pull pages, compose duration
│   ├── Persistence/                                              absent by design: no database (Appendix L), so the template does not create this folder here
│   ├── Messaging/                                                absent by design: no exchange and no queue (document 11 part 1), so the template does not create this folder here
│   ├── Grpc/                                                     absent by design: no gRPC exposed or consumed (document 22 §10.4), so the template does not create this folder here
│   ├── appsettings.json                                          deadlines, bulkheads, batch limits, data-saver defaults; no secrets
│   ├── appsettings.Development.json                              Aspire development values
│   └── Dockerfile                                                Debian-based aspnet image, non-root, read-only root filesystem, ICU present, TZ=UTC
└── tests/Nibras.Bff.Mobile.Tests/                                sync-token, delta and version-policy tests
    ├── Nibras.Bff.Mobile.Tests.csproj                            references the host, the Testing block and PactNet
    ├── Sync/                                                     the sync contract
    │   ├── DeltaTokenTests.cs                                    signature, tenant binding, 30-day expiry, key rotation, tamper rejection
    │   ├── SyncBatchTests.cs                                     order, grouping, per-action results, replay returns the first response
    │   ├── NeverShedTests.cs                                     open breaker defers actions and proceeds with the rest
    │   ├── OnlineOnlyRefusalTests.cs                             clinic, wellbeing, approval and payment action types refused
    │   └── SyncPullTests.cs                                      delta and full-refresh paths per entity group, page limits
    ├── VersionPolicy/                                            master brief Section 37
    │   └── VersionPolicyTests.cs                                 2.2.0 blocked and 2.3.5 nagged under a 2.4.0 minimum; headers on every response
    ├── Composition/                                              composers with stubbed upstreams
    │   ├── TeacherFiveMinuteComposerTests.cs                     four regions and nothing else
    │   ├── CalmScreenComposerTests.cs                            one card per child, designed empty state, break-through
    │   └── MobileHomeComposersTests.cs                           the remaining homes and mode views, table-driven
    ├── PassThrough/                                              allow-list
    │   └── AllowListTests.cs                                     operations off the list refused, on the list forwarded unchanged
    ├── Contracts/                                                consumer pacts, one per upstream service (twenty)
    │   └── UpstreamPactTests.cs                                  pins every upstream path and field the host reads
    ├── Payload/                                                  size budgets
    │   └── PayloadSizeTests.cs                                   home under 24 KB, delta page under 256 KB after Brotli
    ├── Security/                                                 no widening, no leak
    │   ├── TokenForwardingTests.cs                               caller token reaches every upstream unchanged
    │   └── NoSensitiveInSyncTests.cs                             no Sensitive or level-S field in any entity group page
    └── Caching/                                                  keys and invalidation
        └── BffMobileCacheTests.cs                                tenant and user in every key; version policy evicted on the settings broadcast
```

---

## 14. Test plan

New identifiers are minted from `TC-BFF-101` upward, numbers 101 to 160 (Bff.Web uses numbers 001 to 060); no document in the kit used a `TC-BFF-` identifier before these sheets (searched on 2026-09-21).

| Test case | Level | What it proves |
|---|---|---|
| `TC-MOB-003` (Appendix W) | End to end | Teacher five-minute home shows attendance, a quick note, a quick grade, the cover alert and nothing else (REQ-BFF-007) |
| `TC-MOB-004` (Appendix W) | End to end | Parent calm screen with one card per child and the designed empty state (REQ-BFF-008) |
| `TC-MOB-005` (Appendix W) | End to end | Low-bandwidth mode (REQ-BFF-009) |
| TC-MOB-101 | UAT | Last synced morning brief with its as-of time offline |
| TC-MOB-502 | UAT | Report card opens from the local copy offline |
| TC-MOB-701 to TC-MOB-710 | Integration through this host | The Appendix M.5 tests end to end: replay, conflict banner, two devices, after lock, 45-day token, clock skew, interrupted upload, full day offline |
| `TC-MOB-713` (document 09) | Integration | Version policy blocks, nags and preserves the outbox |
| `TC-MOB-714` (document 09) | Integration | Sign-out unregisters the push token and revokes the delta tokens |
| TC-MOB-715 | Integration | A school day of teacher use under 2 MB |
| TC-SEC-044 to TC-SEC-047 | Security | Pinning rotation, lock-screen, logs, permission version on mobile |
| TC-BFF-101 | Integration | A token from 10 minutes ago returns only changes since; a tampered token returns 400 `BFF_VALIDATION_FAILED` (REQ-BFF-004) |
| TC-BFF-102 | Integration | Minimum 2.4.0: app 2.2.0 gets `block`, 2.3.5 gets `nag` and proceeds (REQ-BFF-005) |
| TC-BFF-103 | Integration | A request from a blocked version is still applied by its idempotency key; the server never refuses on version |
| TC-BFF-104 | Integration | `sync/batch` preserves `queuedSeq` order across grouping; "mark absent then late" lands in that order |
| TC-BFF-105 | Integration | 25 marks of one batch travel as one Attendance call and return 25 per-action results |
| TC-BFF-106 | Integration | Every action delivered twice returns the first response and changes state once |
| TC-BFF-107 | Resilience | Attendance circuit open: attendance actions `deferred`, message actions accepted, batch 200 (N-08 never shed) |
| TC-BFF-108 | Integration | Online-only action types refused with `params.reason = "onlineOnly"` (REQ-MOB-008) |
| TC-BFF-109 | Integration | Conflict results carry the five banner fields exactly as the device stores them |
| TC-BFF-110 | Integration | Delta page limits of 500 changes and 256 KB; `hasMore` loops; the token is issued over the last page only |
| TC-BFF-111 | Integration | Wrong-tenant token forces a full refresh and never mixes tenants |
| TC-BFF-112 | Integration | Unread counts per category on every batch and pull (REQ-MOB-020) |
| TC-BFF-113 | Integration | Resumable upload resumes from `bytes_uploaded` without duplicating |
| TC-BFF-114 | Integration | Pass-through refuses an operation off the allow-list with `BFF_NOT_FOUND` and forwards allowed ones unchanged |
| TC-BFF-115 | Integration | Wellbeing pass-through responses carry `no-store` and never enter a cache or an entity group |
| TC-BFF-116 | Security | Tenant header accepted only with the mobile client credential (T-GW-01) |
| TC-BFF-117 | Isolation | Generated tenant-isolation attack on every route, every entity group and every action type |
| TC-BFF-118 | Permission matrix | Every route and allow-listed operation with every Appendix I role |
| TC-BFF-119 | Performance | N-08: 1,200 devices reconnect in 3 minutes, every operation applied once, median synced under 45 s, p95 under 3 minutes |
| TC-BFF-120 | Performance | N-05: morning brief p95 under 250 ms, p99 under 600 ms |
| TC-BFF-121 | Contract | Home under 24 KB and delta page under 256 KB after Brotli |
| TC-BFF-122 | Contract | Twenty consumer pacts verified by the upstream providers |
| TC-BFF-123 | Security | No Sensitive or level-S field in any entity group page across the fixture corpus; logs carry no body, token or push token |
| TC-BFF-124 | Integration | Gate-mode day view carries no watchlist, no identity reference and no pass code |
| TC-BFF-125 | Caching | Tenant and user in every key; version policy evicted by the settings broadcast within 2 s |
| TC-BFF-126 | Integration | Delta-token key rotation: a token signed with the retired key forces a full refresh, not an error |
| TC-BFF-127 | Architecture | `Nibras.Bff.Mobile` references no `Nibras.<S>.*` assembly (with TC-TST-114) |

---

## 15. Scaling, partitioning and risks

| Concern | Design | Trigger to revisit |
|---|---|---|
| Replicas | Stateless; 2 replicas off peak and 4 during each band's peak window, raised by the calendar KEDA cron trigger (`15-deployment-and-operations.md` part 11) with HPA above; the counts are this sheet's starting point, document 21 §5 carries no BFF row | p95 batch time above 2 s in N-08 |
| Partitioning | None; no data | not applicable |
| Sync storm | One batch in flight per device, grouping per upstream, deferral instead of shedding, silent-push jitter set by Notification | Deferral rate above 5 percent for 5 minutes |
| Upstream fan-out | Delta pull asks at most one source per entity group; homes at most one call per region | A group needing two sources on every pull |

| Risk | Likelihood | Impact | Mitigation | Owner |
|---|---|---|---|---|
| A teacher's offline work lost or applied twice | med | high | Idempotency keys forwarded per action, ordered dispatch, deferral, TC-BFF-104 to TC-BFF-107, TC-MOB-701 to TC-MOB-710 | Mobile lead |
| A silent conflict | low | high | Conflict fields mapped verbatim for the banner rule, TC-BFF-109 | Mobile lead with Attendance lead |
| Sensitive data reaches a device | low | critical | Entity groups exclude Sensitive and S by construction, `no-store` pass-through, TC-BFF-115, TC-BFF-123, TC-MOB-704 | Privacy officer |
| An old app breaks after a server change | med | high | Version policy, `v1` kept while any tenant minimum needs it, pacts against the Bff.Mobile OpenAPI | Tech lead |
| Delta token forged or replayed across tenants | low | high | HMAC per tenant, tenant check, 30-day expiry, revocation on unregister | Security owner |

---

## Decisions in force

| Decision | Source | Default if unanswered | Impact if wrong |
|---|---|---|---|
| Route prefix `/bff/mobile/v1/` | `22-api-conventions-and-error-catalog.md` §1.1 and Spectral rule | As stated | Documents 09, 15 and Appendix N write `/api/mobile`, `/config/version` and `/bff-mobile/...`; Open point 1 |
| The app talks to Bff.Mobile only, so single-service screens use an allow-listed pass-through | `09-mobile-structure.md` (Dio against Bff.Mobile only); `12-security-privacy-safety.md` §1.2 | As stated | A per-screen composed endpoint for every mobile screen would multiply routes without adding value |
| Conflict rules are decided by the owning service; Bff.Mobile maps outcomes | Appendix M.3 | As stated | A rule here would be business logic in a BFF (REQ-BFF-001) |
| The server never refuses a request because of the app version | Master brief Section 37 | As stated | A refusal would strand queued work on a device that cannot yet upgrade |
| Sync deferral instead of 429 or 503 on the batch route | Appendix N N-08 ("never sheds a sync batch, it queues it") | As stated | Shedding would force the device into exponential backoff at the worst moment |

## Dependencies on other documents

| This document assumes | Stated in | Checked on |
|---|---|---|
| Entity groups, outbox record, delta token, sync engine, modes, version check | `09-mobile-structure.md` parts 2, 3, 5, 6, 7 | Group D review |
| Offline behaviour and conflict rules | Appendix M | every lint run |
| Upstream operations of Attendance and Assessment | `06-services/attendance.md`, `06-services/assessment.md` | Group C review |
| Upstream operations of the other eighteen services | their sheets, pinned by consumer pacts | Group C review |
| Cache entries, `redis-state` prefixes, payload budgets | `21-performance-engineering.md` §1.21, §2.2, §10 | Group C review |
| Load scenarios N-05 and N-08, the `MobileSyncRejections` alert | Appendix N, `15-deployment-and-operations.md` | Group E review |
| Threat rows | `12-security-privacy-safety.md` §1.2, §2.21 | Group D review |

## Open points

**Closed by ADR-0019 (brief v9.1).** Appendix K.23 now carries `BFF_APP_VERSION_BELOW_MINIMUM` (403, parent-safe), so the missing code this sheet reported exists. The sheet's behaviour does not change: master brief Section 37 still forbids refusing a request because of the app's version, so section 11.4 lists the code as one Bff.Mobile never raises, and the `block` outcome keeps travelling in `config/version` and the response headers. The remaining points are renumbered.

| Question | Default | Owner | Impact if the default is wrong |
|---|---|---|---|
| 1. `09-mobile-structure.md` §5.2 uses `apiBaseUrl .../api/mobile` and `/config/version`; Appendix N and `15-deployment-and-operations.md` write `/bff-mobile/home/principal` and `/bff-mobile/sync/batch`; document 22 requires `/bff/mobile/v1/` | `/bff/mobile/v1/` everywhere; the flavor file's `apiBaseUrl` becomes `https://<host>/bff/mobile/v1` and `minimumVersionPolicyUrl` becomes `/bff/mobile/v1/config/version` | Mobile lead | The Gateway route and the app's base URL disagree on the first build |
| 2. Only Attendance names a change feed for its entity group; the other sources (Scheduling, School, Communication, Requests, Behavior, Identity) do not yet name one | Each source exposes `GET .../changes?checkpoint=` in its sheet; until it does, its group is served as a snapshot with `ETag`, which is correct but costs data | Tech lead, per service sheet | Teacher data usage rises towards the 2 MB budget (TC-MOB-715) |
| 3. `09-mobile-structure.md` §2.7 matches visitors against "the cached list" in gate mode; the Attendance sheet never sends the watchlist to a device | Offline check-ins queue and are matched on the server at sync; gate mode shows "watchlist not checked offline" on the pending row | Security owner with the mobile lead | A device-held watchlist would put names and instructions on a device that can be lost |
| 4. Bus attendant mode needs an Operations transport read permission that Appendix B lists only as `operations.transport.view` without a mode scope | `operations.transport.view` scoped to the attendant's route through the device session | Operations lead | An attendant could read other routes' rosters |

## Review record

| Date | Reviewer | Outcome |
|---|---|---|
| 2026-09-21 | drafted | awaiting Group C review |

## How this document is verified

| Claim | Proof | Where it runs |
|---|---|---|
| Owns no data, no event, no rule | TC-BFF-127 and TC-TST-114; no `Persistence/` or `Messaging/` folder | `tests/Architecture.Tests`, review |
| The sync contract matches Appendix M and document 09 | TC-BFF-101 to TC-BFF-113 and TC-MOB-701 to TC-MOB-710 | Host test project, device pass |
| Never sheds a sync batch | TC-BFF-107, TC-BFF-119 and the `MobileSyncRejections` alert | Load tier, production |
| Version policy per master brief Section 37 | TC-BFF-102, TC-BFF-103, TC-MOB-713 | Host tests, device pass |
| Every gate permission exists in Appendix B | `/lint-plan` cross-check; TC-BFF-118 once code exists | Lint, pipeline |
| Every tree entry has a purpose comment | `tools/kit-lint` rule R18 | Lint |
