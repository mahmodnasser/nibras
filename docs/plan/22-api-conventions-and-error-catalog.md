# 22. API Conventions and Error Catalog

> Plan document for the Nibras platform. Group F. It refines master brief Section 19 (the API conventions bullets and the three rate-limit layers), Section 35 (versioning) and Appendix K (the error catalog); it does not re-derive them. Where this document and the brief disagree, an ADR records the deviation.

**Group** F · **Requirement areas covered** API (all of it), SEC (the parts that ride on the API surface), INT (only where the public API reuses these conventions) · **Last updated** 2026-09-20 by the platform plan

## Purpose

This document lets an engineer write an endpoint, a gRPC method or a client that behaves like every other one in the product without reading another service's code: the URL and JSON shape, the one pagination envelope, the one filter and sort grammar, concurrency and idempotency headers, the long-running-operation contract, bulk endpoints, the Problem Details shape with its `code`, the three rate-limit layers and the 429 contract, gRPC metadata and the one-hop rule, how OpenAPI is produced and checked, and how the error catalog in Appendix K is completed and kept stable. The readers are the engineer writing the endpoint, the reviewer holding it to the Spectral rules, and the integrator reading the developer portal.

## Scope

| In scope | Out of scope | Where the out-of-scope item lives |
|---|---|---|
| REST URL, verb, naming and JSON conventions | Which endpoints each service exposes | `06-services/<service>.md` |
| Pagination envelope, filter and sort grammar | Query plans and indexes behind list endpoints | `21-performance-engineering.md` |
| `ETag`, `If-Match`, `Idempotency-Key` semantics | The outbox, inbox and message idempotency | `11-messaging-architecture.md` |
| Long-running operations, bulk endpoints | Worker hosts, job scheduling, KEDA scaling | `15-deployment-and-operations.md` |
| Problem Details, the `code` field, the catalog rules and ranges | The per-service code rows themselves | Appendix K, quoted by each service sheet |
| Rate limiting on the API surface and the 429 contract | Plan quotas as a commercial model | `06-services/platform.md`, master brief Section 22 |
| gRPC package, deadline, metadata and hop conventions | Which gRPC contracts exist and who calls them | `05-service-catalog.md`, `Nibras.Contracts.<Service>` |
| OpenAPI generation, Scalar, Spectral rules, breaking-change detection | Public API keys, webhooks, standards, deprecation timeline | `23-integrations-and-public-api.md` |
| Authentication headers as they appear on the wire | Token issuance, refresh, permission version, client credentials | `12-security-privacy-safety.md` §3 |

---

## 1. REST conventions

Every rule here is enforced by a Spectral rule in §11 or an architecture test in `Nibras.BuildingBlocks.Web`. A rule with no enforcement is a suggestion, and suggestions drift.

### 1.1 URL shape

| Part | Rule | Example |
|---|---|---|
| Prefix | `/api/v{n}/` with an integer major version and nothing else in the prefix | `/api/v1/` |
| Service segment | The lower-case canonical service name from Appendix L | `/api/v1/attendance/` |
| Resource | Kebab-case plural noun; never a verb, never a table name | `/api/v1/attendance/attendance-sessions` |
| Item | The UUID v7 of the resource, never a sequential number, never a natural key | `/api/v1/school/students/018f6a1e-5c2b-7d3e-9a4f-1b2c3d4e5f60` |
| Sub-resource | A plural noun under an item when the child cannot exist without the parent | `/api/v1/finance/invoices/{id}/lines` |
| Action | A kebab-case verb noun **only** as a sub-path of an item when the verb is not CRUD, always `POST` | `/api/v1/assessment/term-results/{id}/publish` |
| Collection action | `bulk` under a collection for per-item batches (§7); `jobs` for long-running operations (§6) | `/api/v1/attendance/attendance-records/bulk` |
| Public API | Same paths, reached through the Gateway on the tenant's domain with an API key; no separate `/public/` prefix | `https://alnoor.nibras.example/api/v1/school/students` |
| Backend-for-frontend | `/bff/web/v1/` and `/bff/mobile/v1/`, screen-shaped, never documented in the public portal | `/bff/web/v1/home/teacher` |
| Depth | At most one sub-resource level below an item; deeper paths are a smell that the child is its own aggregate | `/invoices/{id}/lines` yes; `/invoices/{id}/lines/{lineId}/taxes` no |

The service segment is what lets the Gateway route by prefix and lets the aggregated OpenAPI document stay free of collisions. Two services never share a resource name under the same segment, and the same resource name under two segments means two different things (Attendance `sessions` are not Identity `sessions`).

### 1.2 Verbs

| Verb | Meaning | Request body | Success status | Idempotent | Requires |
|---|---|---|---|---|---|
| `GET` collection | List with the envelope in §2 | none | 200 | yes | permission `<service>.<resource>.view` |
| `GET` item | One resource, full response model for the caller's role | none | 200, or 304 with `If-None-Match` | yes | `.view` |
| `POST` collection | Create | the create model | 201 with `Location` and the created resource | no, unless `Idempotency-Key` is sent (§5) | `.create` |
| `PUT` item | Full replacement of a small resource | the full model | 200 with the resource | yes, given `If-Match` | `.edit` |
| `PATCH` item | Partial update, JSON Merge Patch (`application/merge-patch+json`) | the changed fields | 200 with the resource | yes, given `If-Match` | `.edit` |
| `DELETE` item | Soft delete (`deleted_at`), moves to the recycle bin in `platform.recycle-bin.*` | none | 204 | yes | `.delete` |
| `POST` item action | A domain command that is not CRUD | the command model, may be empty `{}` | 200 with the resource, or 202 with a job (§6) | no, unless `Idempotency-Key` | the elevated action named in Appendix B |
| `HEAD` | Same as `GET` without a body; used for `ETag` probes | none | 200 | yes | `.view` |
| `OPTIONS` | CORS preflight only, handled by the Gateway | none | 204 | yes | none |

Rules that follow: no `GET` ever changes state, so a `GET` may be retried by the Gateway; no `POST` is used where `PUT` or `PATCH` fits; `DELETE` never hard-deletes through the API (purge is a Platform retention job under `platform.retention.*`); a `PATCH` body that is not valid JSON Merge Patch is a 400 `_VALIDATION_FAILED`.

### 1.3 Naming

| Thing | Convention | Right | Wrong |
|---|---|---|---|
| Path segments | kebab-case, plural for collections | `grade-levels`, `attendance-records` | `gradeLevels`, `GradeLevel`, `grade_level` |
| Query parameters | camelCase | `pageSize`, `sort`, `filter[lastName]` | `page_size`, `PageSize` |
| JSON properties | camelCase, ASCII, no leading underscore | `studentNumber`, `nameAr` | `student_number`, `NameAr`, `_links` |
| Enum values | camelCase strings, never integers | `"status": "enrolled"` | `"status": 2`, `"status": "ENROLLED"` |
| Booleans | Positive adjectives, never negated | `isActive`, `hasGuardian` | `isNotActive`, `disabled` |
| Identifiers | `id` on the resource, `<resource>Id` on a reference | `"id": "018f…"`, `"sectionId": "018f…"` | `"studentID"`, `"section": "018f…"` |
| Timestamps | Suffix `At` | `createdAt`, `lockedAt` | `createdDate`, `created` |
| Dates without time | Suffix `Date` or `On` | `dateOfBirth`, `attendanceDate` | `dob`, `date` |
| Money | An object with `amount` and `currency` (§1.4) | `{"amount": "1234.50", "currency": "SAR"}` | `1234.5`, `"SAR 1,234.50"` |
| Bilingual text | An object with `ar` and `en` (`24-localization-and-calendars.md` §2) | `{"ar": "الرياضيات", "en": "Mathematics"}` | `nameAr` and `nameEn` as siblings on the wire |
| Custom headers | `X-Nibras-<Name>` in Train-Case, only where no standard header exists | `X-Nibras-Tenant-Id` | `X-Tenant`, `nibras_tenant` |

### 1.4 JSON rules

| Rule | Value | Why |
|---|---|---|
| Content type | `application/json; charset=utf-8` for bodies; `application/problem+json` for errors; `application/merge-patch+json` for `PATCH` | One parser per shape |
| Property case | camelCase, produced by System.Text.Json source generation with `JsonNamingPolicy.CamelCase` | Master brief Section 19 |
| Timestamps | ISO 8601 with the `Z` suffix and millisecond precision: `2026-09-19T05:30:00.000Z`; `DateTimeOffset` on the wire, `timestamptz` in the database | Time is stored in UTC and displayed in the tenant time zone (master brief Section 19) |
| Local dates | `YYYY-MM-DD` with no time and no zone: `2026-09-19`; `DateOnly` in code, `date` in the database | An attendance date or a date of birth has no instant |
| Times of day | `HH:mm:ss`: `07:45:00`; `TimeOnly` in code | Bell schedules are wall-clock in the campus time zone |
| Durations | ISO 8601 duration: `PT45M` | Never a bare integer whose unit is a guess |
| Enums | Strings, camelCase, with a documented list in OpenAPI; unknown values on input are 400 `_VALIDATION_FAILED`; new values on output are additive and a client keeps an `unknown` branch | Enums as strings (master brief Section 19) |
| Money | `{"amount": "<decimal string>", "currency": "<ISO 4217>"}`; the amount is a string at the currency's minor-unit scale (`"333.334"` for JOD, `"1050.00"` for AED) | BR-FIN-011 forbids binary floating point at any layer, including the payload |
| Numerals | Western Arabic digits `0` to `9` only; a client renders Arabic-Indic digits under BR-L10N-002 | Parsers on the other end |
| Nulls | A property that is null is omitted from responses unless it is a nullable field the client must distinguish from absent; requests may send `null` to clear a field in `PATCH` | JSON Merge Patch semantics |
| Empty collections | `[]`, never null, never omitted | A client can always iterate |
| Identifiers | UUID v7 as a lower-case string with hyphens | Master brief Section 7 |
| Unknown properties on input | Rejected with 400 `_VALIDATION_FAILED` naming the property | A typo in a client is caught at once, not silently dropped |
| Maximum body | 1 MiB for JSON; files go through `Nibras.BuildingBlocks.Files` signed uploads | A large JSON body is a bulk endpoint or an import |
| Response models | Per role, never an entity; sensitive fields per Appendix J are absent, not null, for callers without the field permission | Master brief Section 19: no entity is ever returned from an API |

### 1.5 Standard headers

| Direction | Header | Required | Value | Owner |
|---|---|---|---|---|
| Request | `Authorization` | yes, except public verification pages | `Bearer <access token>` or `Bearer <api key>` | Identity issues; Gateway validates |
| Request | `X-Nibras-Tenant-Id` | mobile and API-key callers only; web callers are resolved by host | Full tenant UUID v7 | Gateway resolves and forwards; a mismatch with the token is 403 `_TENANT_MISMATCH` |
| Request | `Accept-Language` | no | `ar`, `en`, or `ar-SA`; defaults to the user's stored preference, then the tenant default | Used only for server-rendered documents and notification previews; error text is never localized on the server (Appendix K rule 1) |
| Request | `traceparent` | set by the Gateway if absent | W3C trace context | Same value travels through gRPC and the message envelope (Appendix E) |
| Request | `X-Nibras-Correlation-Id` | set by the Gateway if absent | UUID v7 | Echoed on every response and every Problem Details body |
| Request | `Idempotency-Key` | on unsafe `POST` listed in §5 | Client-generated, 1 to 64 characters | `Nibras.BuildingBlocks.Web` |
| Request | `If-Match` | on `PUT`, `PATCH`, `DELETE` of an aggregate | The `ETag` last read | `Nibras.BuildingBlocks.Web` |
| Request | `If-None-Match` | no | The `ETag` last read, for a 304 | Output caching |
| Request | `X-Nibras-Client` | yes from first-party clients | `web/1.42.0`, `mobile-android/2.3.1`, `mobile-ios/2.3.1` | Lets Bff.Mobile apply the version policy from master brief Section 37 |
| Response | `ETag` | on every single-resource `GET`, `PUT`, `PATCH` | Strong, `"<xmin>-<hash>"` (§4) | `Nibras.BuildingBlocks.Web` |
| Response | `Location` | on 201 and 202 | Absolute path of the created resource or the job | endpoint |
| Response | `X-Nibras-Correlation-Id` | always | echoed | Gateway |
| Response | `RateLimit-Limit`, `RateLimit-Remaining`, `RateLimit-Reset` | always on authenticated calls | §9 | Gateway and service limiter |
| Response | `Retry-After` | on 429 and 503 | integer seconds | §9 |
| Response | `Deprecation`, `Sunset`, `Link rel="deprecation"` | on any deprecated version | `23-integrations-and-public-api.md` §5 | Gateway |
| Response | `Idempotency-Replayed` | on a replayed response | `true` | §5 |
| Response | `Content-Language` | on documents and previews | `ar` or `en` | Documents |
| Response | `Cache-Control` | always | `no-store` by default; `private, max-age=<n>` only where the caching map in `21-performance-engineering.md` allows; `public, max-age=300` on QR verification pages | endpoint |

---

## 2. The pagination envelope

There is one envelope. A list endpoint that returns a bare array fails Spectral rule `nibras-list-pagination-envelope`.

### 2.1 Keyset (cursor) lists

Used for every list that can grow with the school: students, guardians, attendance records, marks, messages, notifications, audit entries, deliveries, requests, documents, invoices, payments. Master brief Section 19 requires keyset for these and `Nibras.BuildingBlocks.Persistence` ships the helper.

Request:

| Parameter | Type | Default | Maximum | Rule |
|---|---|---|---|---|
| `pageSize` | integer | 50 | **200**, or the per-endpoint maximum declared in OpenAPI as `x-nibras-max-page-size`, whichever is lower | Above the maximum is clamped, not rejected, and the response says so in `pageSize` |
| `cursor` | string | absent (first page) | 512 characters | Opaque to the client; a cursor from another endpoint, tenant, sort or filter is 400 `_VALIDATION_FAILED` with field `cursor` |
| `sort` | string | the endpoint's documented default, always ending in `id` | 3 fields | §3.3 |
| `filter[...]` | string | none | 10 predicates | §3.1 |
| `q` | string | none | 100 characters | Free-text search on the fields the endpoint declares searchable, normalized under BR-L10N-001 |

Response, exact shape:

```json
{
  "items": [
    { "id": "018f6a1e-5c2b-7d3e-9a4f-1b2c3d4e5f60", "studentNumber": "2026-0418", "name": { "ar": "محمد أحمد الزهراني", "en": "Mohammed Ahmed Alzahrani" } }
  ],
  "pageSize": 50,
  "nextCursor": "eyJ2IjoxLCJrIjpbIkFsemFocmFuaSIsIjAxOGY2YTFlLTVjMmItN2QzZS05YTRmLTFiMmMzZDRlNWY2MCJdLCJzIjoiK2xhc3ROYW1lLCtpZCIsImYiOiI5YzFkIn0.k2Q1",
  "previousCursor": null,
  "hasMore": true,
  "sort": "+lastName,+id",
  "filterHash": "9c1d"
}
```

| Field | Always present | Meaning |
|---|---|---|
| `items` | yes | The page; `[]` on an empty result, never null |
| `pageSize` | yes | The effective page size after clamping |
| `nextCursor` | yes | Cursor for the next page; `null` when `hasMore` is false |
| `previousCursor` | yes | Cursor for the previous page; `null` on the first page |
| `hasMore` | yes | Whether a next page exists; computed by fetching `pageSize + 1` rows |
| `sort` | yes | The normalized sort applied, with explicit `+` and `-` signs |
| `filterHash` | yes | Short hash of the normalized filter; a cursor carries it so a changed filter invalidates the cursor |

There is no `totalCount` on a keyset list. A count over a partitioned attendance table is a full scan, and a total that changes between pages is a lie. A screen that needs a count calls the endpoint's `/count` sibling, which is served from the Reporting read models or a summary table and is documented with its freshness.

**The cursor.** `base64url(JSON { v: 1, k: [<last sort key values>], s: "<sort>", f: "<filterHash>", d: "next" | "prev" })` followed by `.` and an HMAC-SHA256 tag truncated to 8 bytes, keyed by a per-service secret from the secret store. The tag makes the cursor tamper-evident; the `v` lets the shape change; `s` and `f` bind it to its query. Tenant is not in the cursor because the tenant filter is applied by `Nibras.BuildingBlocks.Tenancy` regardless of what a cursor says. Cursors do not expire, but a cursor whose key row was since deleted still resolves because keyset comparison is on values, not on row existence.

**Stability rule.** Every sort ends in `id` so the key is unique and the page boundary is deterministic. The building block appends `+id` if the caller did not.

### 2.2 Offset (page) lists

Used only for small bounded lists: grade levels, sections of a campus, bell-schedule periods, request types, templates, roles, plans. An endpoint declares `x-nibras-pagination: offset` and Spectral checks it is not on a growth table.

| Parameter | Default | Maximum |
|---|---|---|
| `page` | 1 | 1,000 |
| `pageSize` | 50 | 200 |

```json
{ "items": [], "page": 1, "pageSize": 50, "totalCount": 0, "totalPages": 0, "sort": "+sortOrder,+id" }
```

### 2.3 Which one to use

| Signal | Envelope |
|---|---|
| The table is partitioned by month, or grows with students or events | keyset |
| The table is reference data with fewer than 1,000 rows per tenant | offset |
| The screen is an infinite scroll or a mobile sync | keyset |
| The screen is a settings grid with page numbers | offset |
| In doubt | keyset; an offset list is easy to add later, a keyset list is hard to retrofit |

---

## 3. Filter and sort grammar

One grammar, parsed once in `Nibras.BuildingBlocks.Web`, translated to an EF Core expression by the endpoint's declared field map. An endpoint declares which fields are filterable, with which operators, and which are sortable, as OpenAPI extensions (`x-nibras-filterable`, `x-nibras-sortable`); anything undeclared is 400.

### 3.1 Filter grammar

```ebnf
filter-param   = "filter[" field "]" [ "[" operator "]" ] "=" value ;
field          = identifier { "." identifier } ;            (* camelCase, at most 2 levels: "guardian.phone" *)
identifier     = lower-letter { letter | digit } ;
operator       = "eq" | "ne" | "gt" | "gte" | "lt" | "lte"
               | "in" | "nin" | "contains" | "startsWith" | "isNull" | "between" ;
value          = scalar | list | range ;
scalar         = string | number | boolean | date | datetime | uuid ;
list           = scalar { "," scalar } ;                     (* for in, nin; at most 100 members *)
range          = scalar ".." scalar ;                        (* for between, inclusive both ends *)
```

| Rule | Detail |
|---|---|
| Default operator | Absent `[operator]` means `eq` |
| Combining | Several `filter[...]` parameters are combined with AND. There is no OR at the parameter level; a set of values is `in`. A screen that needs OR across fields uses `q` or a saved report |
| Repeating a field | `filter[status]=enrolled&filter[status]=withdrawn` is 400; use `filter[status][in]=enrolled,withdrawn` |
| Types | Values are parsed by the field's declared type with `CultureInfo.InvariantCulture`: dates as `YYYY-MM-DD`, instants as ISO 8601 UTC, numbers with `.` as the decimal separator, booleans as `true` or `false` |
| Strings | `contains` and `startsWith` are case-insensitive; on a bilingual field they run against the folded column under BR-L10N-001, so `filter[name][contains]=فاطمه` matches "فاطمة" |
| `isNull` | Value is `true` or `false` |
| `between` | Inclusive on both ends; `filter[dateOfBirth][between]=2018-01-01..2018-12-31` |
| Escaping | A literal `,` or `..` inside a string value is percent-encoded (`%2C`, `%2E%2E`); the parser decodes after splitting |
| Limits | 10 predicates per request, 100 members in a list, 200 characters per value; beyond that 400 `_VALIDATION_FAILED` |
| Data scope | The caller's data scope (`12-security-privacy-safety.md` §4) is applied **before** the filter, never expressed through it; a filter cannot widen scope, and a filter that names a field the caller may not see is 403 `_PERMISSION_DENIED` |
| Sensitive fields | Fields classified Sensitive in Appendix J are never filterable; Wellbeing exposes no list filters at all |

### 3.2 Filter examples

| Query string | Meaning | Generated predicate |
|---|---|---|
| `filter[status]=enrolled` | Status equals enrolled | `s.status = 'enrolled'` |
| `filter[status][in]=enrolled,suspended` | Either status | `s.status = ANY(ARRAY['enrolled','suspended'])` |
| `filter[gradeLevelId]=018f…&filter[sectionId][isNull]=true` | In the grade, not yet placed | `s.grade_level_id = $1 AND s.section_id IS NULL` |
| `filter[dateOfBirth][between]=2018-01-01..2018-12-31` | Born in 2018 | `s.date_of_birth BETWEEN '2018-01-01' AND '2018-12-31'` |
| `filter[balance.amount][gt]=0&filter[balance.currency]=SAR` | Owes money in SAR | `b.amount > 0 AND b.currency = 'SAR'` |
| `filter[name][contains]=زهراني` | Family name contains, normalized | `s.name_ar_folded LIKE '%' \|\| nibras_ar_fold($1) \|\| '%'` |
| `filter[markedAt][gte]=2026-09-19T00:00:00Z` | Marked since midnight UTC | `r.marked_at >= $1` |
| `filter[guardian.phone][startsWith]=%2B9665` | Guardian phone starts with `+9665` | join to the slim guardian copy, `g.phone LIKE '+9665%'` |
| `filter[status]=enrolled&filter[status]=withdrawn` | Invalid: repeated field | 400 `SCHOOL_VALIDATION_FAILED`, field `filter[status]` |
| `filter[medicalNotes][contains]=asthma` | Invalid: sensitive field | 403 `SCHOOL_PERMISSION_DENIED` |

### 3.3 Sort grammar

```ebnf
sort-param = "sort=" sort-field { "," sort-field } ;
sort-field = [ "+" | "-" ] field ;                 (* "+" or absent is ascending, "-" is descending *)
```

| Rule | Detail |
|---|---|
| Fields | At most 3, each declared sortable; `id` is appended automatically as the final tiebreaker |
| Bilingual fields | `sort=name` sorts by the parts of the caller's active language with the fallback of BR-L10N-007 applied, using the database collation for that language (`ar-x-icu` or `en-x-icu`); `sort=name.ar` and `sort=name.en` force a language |
| Nulls | Nulls last on ascending, first on descending, stated explicitly in the generated SQL |
| Money | Sorted by `amount` within one currency; a list mixing currencies sorts by currency code then amount |
| Default | Every list documents its default sort; the response echoes the effective sort with explicit signs |
| Examples | `sort=-markedAt` newest first; `sort=lastName,firstName` alphabetical; `sort=-balance.amount,name` biggest debt first, then name |

### 3.4 Free text `q`

`q` runs the endpoint's declared search: normalized trigram match on the bilingual name columns (`24-localization-and-calendars.md` §3), plus exact match on identifiers such as student number or invoice number. `q` is combined with filters by AND. It never runs a full-text search over free-form notes unless the endpoint says so; wellbeing notes are never searchable across records.

---

## 4. `ETag` and `If-Match`

| Aspect | Rule |
|---|---|
| Source | The PostgreSQL `xmin` of the aggregate root (master brief Section 19 optimistic concurrency), rendered as a strong ETag `"<xmin>-<8-hex hash of id>"`; the hash stops an ETag from one row matching another |
| Where it appears | Every single-resource `GET`, `HEAD`, `PUT`, `PATCH` response, and each item of a bulk response |
| `If-Match` required | `PUT`, `PATCH` and `DELETE` on any aggregate root; missing header is 400 `_VALIDATION_FAILED` with field `If-Match` so the client learns at once rather than overwriting silently |
| `If-Match` mismatch | 409 `_CONCURRENCY_CONFLICT`; the Problem Details carries `params.currentEtag` and, for `PATCH`, `params.changedFields` so the screen can show what changed (Appendix K: re-read, show, let the user merge) |
| `If-Match: *` | Accepted only on `DELETE`, meaning "delete whatever version exists" |
| `If-None-Match` on `GET` | 304 Not Modified with the same `ETag` and no body; the output cache in `Nibras.BuildingBlocks.Web` answers without touching the handler when the entry is cached per the caching map |
| Collections | No ETag; a list is not a versioned resource |
| Weak ETags | Never; the wire format is deterministic through source-generated JSON, so a strong tag is honest |
| Mobile offline | The Flutter outbox stores the ETag it last synced; a conflict on replay follows Appendix M and surfaces as `ATTENDANCE_OFFLINE_CONFLICT` or the entity's equivalent, not as a generic 409 |

---

## 5. `Idempotency-Key`

| Aspect | Rule |
|---|---|
| Required on | `POST` that creates money or facts a retry must not duplicate: payments and payment callbacks (master brief Section 36), coursework submissions, attendance marks and bulk marks, request submissions, message sends, gate-pass issuance, bulk endpoints, and every `POST` the Flutter outbox replays. The endpoint declares `x-nibras-idempotency: required`; Spectral rule `nibras-unsafe-post-idempotency-key` checks the list |
| Optional on | Any other `POST`; when present it is honoured |
| Key format | 1 to 64 characters, `[A-Za-z0-9_-]`; the client generates a UUID v7 per logical operation, not per HTTP attempt |
| Scope | `(tenantId, callerId, method, path template, key)`. The same key from a different user or on a different endpoint is a different operation. `callerId` is the user id, or the API key id for the public API |
| Fingerprint | SHA-256 of the canonical request body plus the `If-Match` header; stored with the key |
| Storage | `redis-state` (no eviction, persisted), key `nibras:{tenant}:idem:{sha256(scope)}`, value `{ fingerprint, status, headers, body, storedAt }`; body capped at 64 KiB, larger responses store a `Location` and the client re-fetches |
| TTL | **24 hours** from first sight; long enough for a phone that lost signal on a school trip, short enough that Redis holds one day of writes. Payment callbacks use 7 days because gateways retry for days |
| First request | The handler runs; the response is stored atomically with the commit through the outbox transaction, so a stored key always corresponds to a committed result |
| In-flight duplicate | A second request with the same key while the first is running gets 409 `_CONCURRENCY_CONFLICT` with `Retry-After: 2`; the client retries and then receives the replay |
| Replay | Same key, same fingerprint: **200** with the original body verbatim, header `Idempotency-Replayed: true`, and the Problem Details-free body; when the original was a Problem Details (a 4xx), the same Problem Details is replayed with its original status. Appendix K row `_IDEMPOTENCY_REPLAY` (200) is the code a client may log; the client treats the reply as success and uses the original result |
| Key reuse with a different payload | 422 is not in the catalog, so this is 400 `_VALIDATION_FAILED` with field `Idempotency-Key` and `params.reason = "fingerprintMismatch"`; the original is not disturbed |
| Redis unavailable | The building block fails closed for endpoints marked `required` (503 `_DEPENDENCY_UNAVAILABLE`, `Retry-After: 5`) and fails open for optional ones, because a duplicate payment is worse than a delayed one |
| Audit | The replay is logged with the correlation ids of both attempts; the audit entry of the original is not duplicated |

---

## 6. The long-running-operation contract

Anything that can take more than 2 seconds at the p95 in Appendix N runs as a job: report-card batches, invoice runs, imports and exports, timetable solving, bulk document generation, tenant provisioning, sensitive exports, projection rebuilds. The contract is implemented once in `Nibras.BuildingBlocks.Jobs`; the job runs in the owning service's worker image.

### 6.1 Starting a job

| Step | Detail |
|---|---|
| Request | `POST /api/v1/<service>/<resource>/{id}/<action>` or `POST /api/v1/<service>/jobs` with `type` and parameters; `Idempotency-Key` required so a double-click starts one batch, which is what `ASSESSMENT_REPORT_CARD_BATCH_RUNNING` and `FINANCE_INVOICE_RUN_IN_PROGRESS` also guard |
| Response | **202 Accepted**, `Location: /api/v1/<service>/jobs/{jobId}`, `Retry-After: 2`, body = the job resource below |
| Concurrency | A second job of the same type on the same subject while one is running is 409 with the service's specific code; the Problem Details carries `params.runningJobId` |
| Permission | The endpoint's own permission starts it; `platform.jobs.view` reads any job in the tenant; `platform.jobs.cancel` and `platform.jobs.replay` are elevated per Appendix B |

### 6.2 The job resource

```json
{
  "id": "018f7c2a-0b1e-7f3a-8d2c-6e5f4a3b2c1d",
  "type": "assessment.report-card-batch",
  "state": "running",
  "subject": { "resourceType": "term", "resourceId": "018f6b…" },
  "progress": { "done": 312, "total": 800, "percent": 39, "messageKey": "jobs.reportCards.rendering", "messageParams": { "section": "4B" } },
  "createdAt": "2026-09-19T06:00:00.000Z",
  "startedAt": "2026-09-19T06:00:01.250Z",
  "completedAt": null,
  "estimatedCompletionAt": "2026-09-19T06:08:30.000Z",
  "result": null,
  "error": null,
  "correlationId": "018f7c2a-0b1d-7e2f-9c3b-1a2b3c4d5e6f",
  "requestedBy": "018f5d…",
  "cancelRequested": false
}
```

| Field | Rule |
|---|---|
| `state` | `queued` → `running` → `succeeded` \| `failed` \| `cancelled`; terminal states never change |
| `progress` | `done` and `total` are counts of the job's own unit (report cards, rows, invoices); `total` may be null until known; `messageKey` is a translation key, never a sentence, so the console renders it in the operator's language |
| `result` | On `succeeded`: `{ "resourceUrl": "/api/v1/documents/export-jobs/018f…/file", "summary": { "succeeded": 800, "failed": 0, "skipped": 0 }, "reportUrl": "…" }`; the per-item report is itself a keyset list at `/jobs/{id}/items` |
| `error` | On `failed`: a full Problem Details object (§8), so the code and correlation id are available without a second call |
| Retention | The job resource is kept **7 days** after completion, its per-item report 30 days, its output file per the retention row in Appendix J for that document type |
| Cancellation | `POST /jobs/{id}/cancel` sets `cancelRequested`; the worker checks the `CancellationToken` between units and finishes in `cancelled` with the partial summary; a job that cannot be cancelled says so in OpenAPI |
| Replay | `POST /jobs/{id}/replay` (elevated) restarts a `failed` job from its checkpoint; `PLATFORM_JOB_REPLAY_REFUSED` when the state does not allow it |

### 6.3 Progress over the real-time channel

| Aspect | Rule |
|---|---|
| Channel | The SignalR hub `/hubs/jobs` behind the Gateway, Redis backplane on `redis-state` |
| Group | `tenant:{tenantId}:job:{jobId}`; a client joins after receiving the 202, the hub checks `platform.jobs.view` or ownership of the job before joining |
| Messages | `jobProgress` with the `progress` object and `state`; `jobCompleted` with the full job resource; at most one `jobProgress` per second per job (the worker coalesces) |
| Polling fallback | `GET` the `Location` honouring `Retry-After` (2 s while queued, 5 s while running); the mobile app polls because it does not hold a hub open in the background |
| Notification | A job started by a user and finishing after they navigated away produces an inbox notification through Notification with a deep link to the result, per the row in Appendix C for that job type |
| Platform console | The process monitor in the platform console reads every service's `/jobs` through Bff.Web and shows them tenant by tenant; it is a view, not a second store |

---

## 7. Bulk endpoints with per-item results

| Aspect | Rule |
|---|---|
| Path | `POST /api/v1/<service>/<resource>/bulk` |
| Body | `{ "mode": "independent" \| "allOrNothing", "items": [ { "clientReference": "row-1", ...create or update model... } ] }` |
| Size | **500 items** maximum; over 500 is 400 `_VALIDATION_FAILED` with `params.maxItems = 500` and the client uses an import job (§6) through Documents |
| `clientReference` | Required, unique within the request, 1 to 64 characters; it is how a row maps back to a spreadsheet line or an outbox entry |
| `Idempotency-Key` | Required on the whole request; per-item replays are not tracked separately |
| `mode: independent` | Each item is its own unit of work; failures do not affect siblings; the response is **200** even when every item failed, because the bulk request itself succeeded |
| `mode: allOrNothing` | One transaction; the first failure rolls back everything; the response is **200** with every item `status: "rolledBack"` except the failing one, which carries its problem |
| Response | `{ "summary": { "total": 3, "succeeded": 2, "failed": 1, "rolledBack": 0 }, "results": [ { "clientReference": "row-1", "status": "created", "id": "018f…", "etag": "\"4821-a1b2c3d4\"" }, { "clientReference": "row-2", "status": "updated", "id": "…", "etag": "…" }, { "clientReference": "row-3", "status": "failed", "problem": { ...Problem Details... } } ] }` |
| Per-item statuses | `created`, `updated`, `unchanged`, `failed`, `rolledBack`, `skipped` (a duplicate `Idempotency-Key` inside the outbox replay, see `ATTENDANCE_DUPLICATE_MARK`) |
| Per-item `If-Match` | Optional `etag` on an update item; a mismatch fails that item with `_CONCURRENCY_CONFLICT` |
| Ordering | Items are processed in request order; the results array is in the same order |
| Events | One integration event per item, not one per batch, so consumers stay idempotent per subject; a batch-level event exists only where Appendix E lists it |
| Time budget | A bulk request must finish inside the write budget of master brief Section 19 at 500 items on demo-scale data; if it cannot, the endpoint is a job, not a bulk endpoint |
| Data scope | Each item is checked against the caller's scope individually; one out-of-scope item does not fail the request in `independent` mode |

---

## 8. Problem Details

Every error body, from every service and from the Gateway, is RFC 9457 Problem Details with the Nibras extension members. It is produced by the shared middleware in `Nibras.BuildingBlocks.Web` (Appendix K rule 6) and never hand-built in a handler; a handler returns a `Result` with an `Error` and the middleware does the rest.

```json
{
  "type": "urn:nibras:problem:ATTENDANCE_SESSION_LOCKED",
  "title": "Attendance session locked",
  "status": 409,
  "detail": "Session 018f… locked at 2026-09-19T05:15:00Z; marking after the cut-off requires the edit-after-lock request.",
  "instance": "/api/v1/attendance/attendance-sessions/018f…/records",
  "code": "ATTENDANCE_SESSION_LOCKED",
  "correlationId": "018f7c2a-0b1d-7e2f-9c3b-1a2b3c4d5e6f",
  "tenantId": "018f0000-0000-7000-8000-000000000001",
  "parentSafe": false,
  "params": { "lockedAt": "2026-09-19T05:15:00Z", "requestTypeKey": "attendance.edit-after-lock" },
  "errors": null,
  "retryAfterSeconds": null
}
```

| Member | Required | Rule |
|---|---|---|
| `type` | yes | `urn:nibras:problem:<code>`; a URN, not a URL, so no host is baked into clients |
| `title` | yes | The English developer title from Appendix K; never rendered to an end user |
| `status` | yes | Equals the HTTP status |
| `detail` | yes | A developer hint in English; may name identifiers, never a person's name, never a sensitive value (Appendix J); never rendered to an end user (Appendix K rule 1) |
| `instance` | yes | The request path without the query string |
| **`code`** | yes | The stable catalog code `<SERVICE>_<MEANING>`; the client resolves the English and Arabic text from its resource bundle and the Because panel from the code |
| `correlationId` | yes | Same as the response header; shown to guardians as the reference number when `parentSafe` is false |
| `tenantId` | yes, except on platform endpoints | The resolved tenant |
| `parentSafe` | yes | Copied from the catalog row; a client on a guardian or student surface renders a generic message with the reference number when false (Appendix K rule 4) |
| `params` | no | Structured values the client's message template needs (`maxItems`, `lockedAt`, `currentEtag`, `limit`, `used`); keys are camelCase, values are wire-typed like any response; never free text |
| `errors` | on `_VALIDATION_FAILED` | `[ { "field": "dateOfBirth", "code": "dateInFuture", "params": { "max": "2026-09-19" } } ]`; `field` is the JSON path in the request (`items[2].amount.currency`), `code` is a validation code from the shared validation catalog in `Nibras.BuildingBlocks.Application` with its own resource bundle |
| `retryAfterSeconds` | on 429 and 503 | Same value as the `Retry-After` header, so a client that cannot read headers still backs off |
| `traceId` | no | Present in non-production environments only, for the Aspire dashboard |

**Validation catalog.** Field-level validation codes (`required`, `maxLength`, `dateInFuture`, `notE164`, `unknownEnumValue`, `mixedNumerals`) are a separate, cross-service catalog in `Nibras.BuildingBlocks.Application`, because they describe input shape, not domain state. They are listed in `docs/api/validation-codes.json` and follow the same never-removed rule as Appendix K.

**Status mapping for unexpected failures.** An unhandled exception is 500 with code `<SERVICE>_INTERNAL_ERROR`, `parentSafe: false`, no `detail` beyond the correlation id, and an alert; the exception itself goes to the log with the tenant and the handler name. `<SERVICE>_INTERNAL_ERROR` is emitted by the middleware alongside the eight cross-cutting suffixes and is proposed for Appendix K.1 as the ninth (open point 1).

---

## 9. Rate limiting: three layers and the 429 contract

Quoted from master brief Section 19: the Gateway protects the platform from traffic, each service protects a handler from one noisy caller, Platform protects the commercial model. All three read counters from `redis-state`.

### 9.1 The layers

| Layer | Where | Keyed by | Algorithm | Default limit | On breach | Code |
|---|---|---|---|---|---|---|
| 1 Gateway, per source address | YARP middleware in `nibras/gateway` | client IP, or the first trusted `X-Forwarded-For` hop | Sliding window, 1 minute | 600 requests per minute unauthenticated; 3,000 authenticated | 429, before routing; counted per tenant in the abuse dashboard | `GATEWAY_RATE_LIMITED` (the Gateway emits the cross-cutting suffix with its own prefix) |
| 1 Gateway, per tenant | same | `tenantId` from the token or host | Token bucket refilled per minute | By plan: 3,000 per minute (small), 12,000 (standard), 40,000 (scale); burst equal to 10 seconds of refill | 429 | `GATEWAY_RATE_LIMITED` |
| 2 Service, per user and per endpoint | `Nibras.BuildingBlocks.Web` limiter, policy per endpoint group | `(tenantId, userId or apiKeyId, endpoint policy)` | Token bucket | `read`: 300 per minute; `write`: 120 per minute; `sensitive` (exports, sensitive reads): 20 per minute; `auth` (sign-in, code requests): 10 per 5 minutes | 429 | `<SERVICE>_RATE_LIMITED` |
| 2 Service, per API key | same, policy from the key | `apiKeyId` | Token bucket with the key's own limit (`23-integrations-and-public-api.md` §2) | Per plan: 60, 300 or 1,000 per minute per key | 429 | `<SERVICE>_RATE_LIMITED` |
| 3 Platform, plan quotas | Platform, checked by the service through the plan-limits cache and metered by `<service>.usage.recorded.v1` | `tenantId` and meter | Monthly or absolute counters | Jobs per hour, SMS credits, storage GiB, AI calls per month, active students, API calls per day per plan | 402 for a hard quota; a `platform.limit.approaching.v1` event at 80 percent | `PLATFORM_PLAN_LIMIT_REACHED` (402), `NOTIFICATION_SMS_CREDITS_EXHAUSTED` (402); `AI_USAGE_LIMIT_REACHED` at the status Appendix K.21 gives it (below) |

Layer 1 and layer 2 answer "is this caller too fast"; layer 3 answers "has this school used what it paid for". Master brief Section 19 separates them: a rate breach at the Gateway or a service is **429** with a `_RATE_LIMITED` code and `Retry-After`; a plan-quota breach is **402** with `PLATFORM_PLAN_LIMIT_REACHED`, **no** `Retry-After`, and the limit, the current usage and the upgrade action in `params`. A 402 is never retried automatically.

**One exception, owned by Appendix K.21.** `AI_USAGE_LIMIT_REACHED` is not a plain 402. An assist request over the limit answers **200** and degrades to rung 1 with the code as the degradation reason (`25-ai-and-assist-ladder.md` §8 and §10); the **402** is reserved for an administrator's explicit rung 4 request while the plan's rung 4 allowance is spent. This document does not set that status; Appendix K.21 does.

### 9.2 The 429 contract

| Element | Value |
|---|---|
| Status | 429 |
| Body | Problem Details with `code` ending `_RATE_LIMITED`, `parentSafe: true`, `retryAfterSeconds`, `params.limit`, `params.window`, `params.scope` (`address`, `tenant`, `user`, `apiKey`) |
| `Retry-After` | Integer seconds until the bucket has one token, minimum 1, maximum 60 at layers 1 and 2 |
| `RateLimit-Limit` | The policy limit for the window, on every authenticated response, not only on 429 |
| `RateLimit-Remaining` | Tokens left; 0 on a 429 |
| `RateLimit-Reset` | Seconds until the window resets |
| Client behaviour | Back off `Retry-After` plus jitter of 0 to 25 percent (Appendix K row `_RATE_LIMITED`); the generated Angular and Flutter clients do this in their HTTP interceptor and never let a screen retry faster |
| Exemptions | Health probes, the sign-out endpoint, the emergency broadcast acknowledgment endpoint (Attendance), and the Gateway maintenance page; each exemption is listed in `deploy/` configuration and reviewed in the security review |
| Noisy neighbour proof | Appendix N scenario N-06 asserts that a large tenant at its limit does not raise the p95 of a small tenant beyond budget |
| Redis unavailable | Layer 1 and 2 fail open with a metric and an alert (a slow product beats a locked one); layer 3 fails closed for paid meters (SMS, AI) and open for soft ones (jobs) |

### 9.3 The 402 contract for a plan quota

Master brief Section 19 states this shape; it is repeated here only as the wire detail an engineer needs, and no value differs.

| Element | Value |
|---|---|
| Status | 402 |
| Code | `PLATFORM_PLAN_LIMIT_REACHED`, or the meter's own 402 code where Appendix K defines one (`NOTIFICATION_SMS_CREDITS_EXHAUSTED`) |
| `Retry-After` | Never sent. A quota does not refill on a timer the client can wait out, and a client that backs off and retries hammers a limit that will not reset |
| `retryAfterSeconds` | Absent, for the same reason |
| Body | Problem Details carrying the three things Appendix K's row promises, in the `params` names this document owns: `params.limit`, `params.usage` and `params.upgradeAction` (the console route that raises the plan or buys the meter), plus `params.meter`; a meter that resets on a clock adds `params.resetsAt`, as the daily API quota does (`23-integrations-and-public-api.md` §2.5) |
| Client behaviour | Show the limit and the upgrade action; no automatic retry (Appendix K row `PLATFORM_PLAN_LIMIT_REACHED`) |
| Warning before it | `platform.limit.approaching.v1` at 80 percent, so the 402 is not the first the school hears of it |

---

## 10. gRPC conventions

gRPC is the only synchronous call between services (master brief Section 7.3). It exists for queries that truly need fresh data: the School directory, the Identity permission lookup, the live allergy read in Wellbeing. Everything else is an event.

### 10.1 Package and service naming

| Thing | Convention | Example |
|---|---|---|
| Package | `nibras.<service>.v<n>` (reference architecture Section 7) | `nibras.school.v1` |
| File | `Nibras.Contracts.<Service>/Grpc/<service>.proto`; one file per package version | `Nibras.Contracts.School/Grpc/school.proto` |
| Service | PascalCase noun naming the capability, not the table | `StudentDirectory`, `PermissionLookup`, `AllergyAlerts` |
| Method | PascalCase verb noun; `Get` for one, `List` for many, `Check` for a boolean | `GetStudent`, `ListStudentsBySection`, `CheckPermission` |
| Messages | `<Method>Request`, `<Method>Response`; fields snake_case in proto, generated to PascalCase | `GetStudentRequest { string student_id = 1; }` |
| Timestamps | `google.protobuf.Timestamp`, UTC | |
| Dates | `string` in `YYYY-MM-DD`, because proto has no date type and `Date` well-known types differ by generator | |
| Money | `message Money { string amount = 1; string currency = 2; }` in `Nibras.Contracts.Shared`, same rule as JSON | |
| Enums | First value `<NAME>_UNSPECIFIED = 0`; never reuse a number | `STUDENT_STATUS_UNSPECIFIED = 0` |
| Bilingual text | `message LocalizedText { string ar = 1; string en = 2; }` in `Nibras.Contracts.Shared` | |
| Forbidden | `google.protobuf.Any`, `oneof` for versioning, streaming for anything but exports, a field carrying a sensitive value per Appendix J | |
| Style and compatibility | `buf lint` with the `DEFAULT` category plus `FIELD_LOWER_SNAKE_CASE`, and `buf breaking` against the last released tag on every pull request touching `src/Contracts/` | |

### 10.2 Deadlines, retries and circuit breaking

| Aspect | Rule |
|---|---|
| Deadline | The caller always sets one. Default **2 seconds** for a lookup, 5 seconds for a directory page, 30 seconds for a reconciliation checksum. A method's deadline is declared in the proto as the option `(nibras.deadline_ms)` and the generated client uses it |
| Propagation | The remaining deadline from the inbound HTTP request (the Gateway's 30-second request timeout) bounds any gRPC deadline; a call cannot outlive its request |
| Retries | Only on `UNAVAILABLE` and `DEADLINE_EXCEEDED`, only for methods marked idempotent (every `Get`, `List`, `Check`), 2 retries with exponential backoff 100 ms and 400 ms plus 0 to 50 percent jitter, through the gRPC retry policy on the channel |
| Circuit breaker | Per channel through the resilience pipeline in `Nibras.BuildingBlocks.Web`: opens after 50 percent failures in a 30-second window with at least 20 calls, half-open after 15 seconds |
| Fallback | Declared per call site: the local reference copy (Finance falls back to its slim student copy), the L1 cache (permission lookup falls back to the last known permission set for at most 60 seconds), or refuse (Wellbeing allergy read never falls back, `WELLBEING_ALLERGY_ALERT_UNAVAILABLE`) |
| Channels | One pooled channel per target service per process, service discovery through `Microsoft.Extensions.ServiceDiscovery`, TLS inside the cluster |
| Load | A gRPC method has a query budget like any handler; the command-counting interceptor applies |

### 10.3 Metadata

| Key | Required | Value | Set by | Checked by |
|---|---|---|---|---|
| `authorization` | yes | `Bearer <service token>` from client credentials (`12-security-privacy-safety.md` §3.3), 5-minute life, scope list from the registry | the calling service | the callee's interceptor validates locally against the published keys |
| `nibras-tenant-id` | yes, except platform-scoped calls that say so | Full tenant UUID v7 | `Nibras.BuildingBlocks.Tenancy` from the ambient tenant | the callee sets its tenant context from it and refuses a mismatch with the token's tenant claim: `PERMISSION_DENIED` with code `_TENANT_MISMATCH` |
| `nibras-correlation-id` | yes | The inbound correlation id | `Nibras.BuildingBlocks.Observability` | logged and forwarded to any event the callee publishes |
| `traceparent`, `tracestate` | yes | W3C trace context | OpenTelemetry propagator | |
| `nibras-user-id` | no | The acting user when the call is on behalf of a user | caller | used for audit only; never for authorization, which is by service scope |
| `nibras-hop` | yes | `1` | the caller's interceptor | §10.4 |
| `nibras-client` | yes | `<service>/<version>` | caller | metrics and the deprecation calendar |

### 10.4 The one-hop rule

Master brief Section 7.3: maximum one synchronous hop. The rule is enforced, not hoped for.

| Mechanism | Detail |
|---|---|
| Outbound interceptor | Refuses to open a gRPC call when the current activity already carries `nibras-hop: 1` from an inbound gRPC call; throws `InvalidOperationException` in development and returns `FAILED_PRECONDITION` with code `<SERVICE>_HOP_LIMIT_EXCEEDED` in production |
| Inbound interceptor | Sets the hop marker on the request activity so any nested call is caught |
| Architecture test | `GrpcHopRules`, one of the architecture rules of `07-solution-structure.md` §10.3 in `tests/Architecture.Tests/Rules/`; document 07 owns the rule and its test-case identifier, and this document only states what it enforces: it fails when a gRPC service implementation class references a gRPC client type |
| Consequence | A service that needs data two hops away keeps a reference copy (master brief Section 7.3) or subscribes to an event; the reconciliation job in `10-data-architecture.md` keeps the copy honest |
| Backend-for-frontend | Bff.Web and Bff.Mobile call services over HTTP, fan out in parallel, and are not counted as a hop because they own no data and make no gRPC calls |

### 10.5 Status mapping

| gRPC status | Meaning here | When the callee returns it | HTTP equivalent if surfaced to a client |
|---|---|---|---|
| `OK` | success | | 200 |
| `INVALID_ARGUMENT` | bad request | validation failed; `code` in trailers | 400 |
| `UNAUTHENTICATED` | no or bad token | | 401 |
| `PERMISSION_DENIED` | scope or tenant mismatch | `_PERMISSION_DENIED`, `_TENANT_MISMATCH` | 403 |
| `NOT_FOUND` | not found, or hidden | including Wellbeing's deliberate not-found | 404 |
| `ABORTED` | concurrency conflict | `_CONCURRENCY_CONFLICT` | 409 |
| `FAILED_PRECONDITION` | domain state refuses | any service-specific 409 code; the hop-limit code | 409 |
| `RESOURCE_EXHAUSTED` | rate limited | `_RATE_LIMITED` | 429 |
| `UNAVAILABLE` | dependency down, circuit open | `_DEPENDENCY_UNAVAILABLE` | 503 |
| `DEADLINE_EXCEEDED` | deadline passed | | 504 |
| `INTERNAL` | unhandled | `<SERVICE>_INTERNAL_ERROR` | 500 |

The catalog `code` travels in the trailer `nibras-error-code`, with `nibras-error-params` as JSON, so a gRPC failure surfaces to a REST client as the same Problem Details it would have produced locally.

---

## 11. OpenAPI, Scalar and contract-first

### 11.1 Generation and publication

| Aspect | Rule |
|---|---|
| Generator | `Microsoft.AspNetCore.OpenApi` (MIT) from the Minimal API endpoint definitions, with document transformers from `Nibras.BuildingBlocks.Web` that add the Problem Details responses, the `x-nibras-*` extensions and the security schemes |
| Per service | `/openapi/v1.json` (and `/openapi/v2.json` while two versions are supported), OpenAPI 3.1, served by the service in every environment |
| Aggregated | The Gateway merges every service document into `/openapi/v1.json` on the tenant's domain, keeping the service tag prefix; the aggregated document is the API inventory (`12-security-privacy-safety.md` §1, "an undocumented route fails the build") |
| Scalar | `Scalar.AspNetCore` (MIT) at `/scalar/v1` on every service in Development and Staging, and on the Gateway in the developer portal for Production behind the developer-portal sign-in; never on a bare Production service |
| Committed spec | Each service commits `docs/api/<service>.v1.json` generated at build; the pull request fails when the committed file differs from the generated one (`nibras-spec-drift` check) |
| Generated clients | The Angular workspace and the Flutter app consume the aggregated document through the generator pinned in `19-dependency-and-license-inventory.md`; a hand-written HTTP call in a feature is a lint failure (master brief Section 19, "generated API clients only") |
| Contract tests | PactNet consumer contracts from the web and mobile clients run against each provider on every provider pull request (master brief Section 19) |
| Extensions | `x-nibras-permission` (the permission the endpoint declares, Appendix B rule 4), `x-nibras-pagination` (`keyset` \| `offset` \| `none`), `x-nibras-max-page-size`, `x-nibras-filterable` (map of field to operator list), `x-nibras-sortable`, `x-nibras-idempotency` (`required` \| `optional`), `x-nibras-parent-safe-codes`, `x-nibras-deprecated-sunset`, `x-nibras-tier` (1, 2, 3) |

### 11.2 Document rules

| Rule | Detail |
|---|---|
| `operationId` | `<service>_<resource>_<action>` in snake_case: `attendance_attendance_records_bulk_create`; unique across the aggregated document |
| Tags | One tag per resource, prefixed with the service: `Attendance / Attendance sessions` |
| Summaries and descriptions | English, one sentence, imperative: "Mark attendance for a session." |
| Responses | Every operation declares its success response and each Problem Details response it can produce, with the `code` enum listed per status; the eight cross-cutting codes are added by the transformer |
| Examples | Every request and response schema has at least one example, and any schema containing `LocalizedText` or a person's name has an example with Arabic text, so a generated client is tested against right-to-left data from the first build |
| Schemas | Named `<Resource><Create \| Update \| Response \| Summary>`; no schema named after an entity class; no `additionalProperties: true` |
| Security | `bearerAuth` (JWT) and `apiKeyAuth` (Bearer API key) schemes; each operation lists which it accepts; BFF documents list `bearerAuth` only |
| Servers | Relative (`/`) so the same document is valid on every tenant domain |

### 11.3 Spectral rules

The ruleset lives in `tools/api-lint/nibras.spectral.yaml` with `.ps1` and `.sh` wrappers over one Node entry point (Appendix X, ADR-0017), extends `spectral:oas`, and runs on every committed spec. Severity `error` fails the build.

| Rule | Severity | What it checks |
|---|---|---|
| `nibras-path-version-prefix` | error | Every path starts with `/api/v[0-9]+/<service>/` or `/bff/(web\|mobile)/v[0-9]+/`, service from the Appendix L list |
| `nibras-path-kebab-case` | error | Every static path segment matches `^[a-z][a-z0-9]*(-[a-z0-9]+)*$` |
| `nibras-path-plural-nouns` | error | The resource segment ends in `s`, `es` or is in the small irrelevant-plural allow list (`staff`); action segments are only allowed after `{id}` with `POST` |
| `nibras-path-depth` | error | At most one sub-resource level under an item |
| `nibras-operation-id-format` | error | `operationId` matches `^[a-z]+_[a-z_]+_[a-z_]+$` and starts with the path's service segment |
| `nibras-operation-permission` | error | Every operation outside `/health` and the public verification page carries `x-nibras-permission` naming a permission of the form `<service>.<resource>.<action>` |
| `nibras-json-camel-case` | error | Every property name in every schema matches `^[a-z][A-Za-z0-9]*$` |
| `nibras-enum-string` | error | Every `enum` is of `type: string` and every value is camelCase |
| `nibras-datetime-utc` | error | Every `format: date-time` property description states UTC, and the example ends in `Z`; every `format: date` example is `YYYY-MM-DD` |
| `nibras-money-object` | error | No property named `amount`, `price`, `total`, `balance` or ending in `Amount` is numeric; each references `Money` |
| `nibras-localized-text-object` | error | No pair of properties `<x>Ar` and `<x>En` on one schema; bilingual text references `LocalizedText` |
| `nibras-list-pagination-envelope` | error | Every `GET` collection response is `PagedResult` or `KeysetResult`; no top-level array responses |
| `nibras-list-max-page-size` | error | Every keyset or offset list declares `x-nibras-max-page-size` between 1 and 200 |
| `nibras-keyset-on-growth-tables` | error | A path whose resource is in the growth list (students, attendance-records, marks, messages, notifications, audit-entries, deliveries, requests, invoices, payments, documents) declares `x-nibras-pagination: keyset` |
| `nibras-filter-declared` | error | An operation with `filter[...]` or `sort` parameters declares `x-nibras-filterable` and `x-nibras-sortable` |
| `nibras-problem-responses` | error | Every operation declares `application/problem+json` for 400, 401, 403 and 429, and 404 on item paths, 409 on writes |
| `nibras-problem-code-enum` | error | Every Problem Details response lists the `code` enum, and every listed code exists in `docs/api/error-codes.json` (generated from Appendix K) |
| `nibras-write-if-match` | error | Every `PUT`, `PATCH` and `DELETE` on an item path declares the `If-Match` header parameter as required and a 409 response |
| `nibras-unsafe-post-idempotency-key` | error | Every `POST` in the required list (payments, submissions, attendance, requests, messages, gate passes, bulk, jobs) declares `Idempotency-Key` as required |
| `nibras-bulk-shape` | error | Every `/bulk` operation has the `BulkRequest` body, the `BulkResult` response and `maxItems: 500` |
| `nibras-lro-202-shape` | error | Every 202 response carries `Location` and returns the `Job` schema |
| `nibras-no-entity-schema` | error | No schema name ends in `Entity`, and no schema carries `deletedAt`, `rowVersion`, `xmin`, `passwordHash` or any Appendix J credential field |
| `nibras-no-sensitive-in-list` | error | A list item schema (`*Summary`) contains none of the Appendix J sensitive field names |
| `nibras-example-arabic` | warn | Every schema with `LocalizedText` or a person name has an example containing Arabic characters |
| `nibras-deprecated-has-sunset` | error | Every `deprecated: true` operation carries `x-nibras-deprecated-sunset` as a date and a description naming the replacement |
| `nibras-tag-per-resource` | error | Every operation has exactly one tag of the form `<Service> / <Resource>` |
| `nibras-security-declared` | error | Every operation declares a security requirement; BFF documents allow only `bearerAuth` |
| `nibras-no-query-pii` | error | No query parameter is named `nationalId`, `passportNumber`, `phone`, `email` or `iban` (privacy rule: never PII in URLs) |

Twenty-eight rules; the first twelve are the minimum the plan requires, the rest are the ones that have caught a defect in a comparable product.

### 11.4 Breaking-change detection

| Aspect | Rule |
|---|---|
| Tool | `oasdiff` (Apache-2.0) in `tools/api-lint/`, comparing the pull request's generated document against the last released document for the same major version, stored under `docs/api/released/<service>.v<n>.json` and updated by the release pipeline |
| Breaking, fails the build | Removing a path or operation; removing or renaming a property; changing a property type or format; making an optional request property required; adding a required request property; removing an enum value on input; removing a response status; narrowing `x-nibras-max-page-size`; removing a filterable or sortable field; removing a `code` from a response enum; changing `x-nibras-permission` to a stricter permission |
| Additive, allowed | Adding a path or operation; adding an optional request property; adding a response property; adding an enum value on output (clients keep an `unknown` branch); adding a `code` (with its Appendix K row); widening a page size; adding a filterable field |
| Escape hatch | A breaking change ships as `/api/v{n+1}/` for the affected service, with the old version kept for 12 months per master brief Section 35 and the headers in `23-integrations-and-public-api.md` §5; the pull request references the ADR that approved the new major |
| Messages | `buf breaking` for gRPC (§10.1); the message catalog in `11-messaging-architecture.md` for events |
| Mobile | An additional check runs the previous released Flutter contract tests against the new provider, because a family's phone may be two versions behind (master brief Section 37) |

---

## 12. Error catalog

### 12.1 What Appendix K owns and what this document adds

Appendix K is the catalog. This document does not copy its per-service rows; it fixes the shape (§8), the cross-cutting codes every service emits, the namespace ranges, and the lifecycle rules. Each service sheet in `06-services/<service>.md` quotes its own section of Appendix K (K.2 for Identity through K.21 for Ai) and adds nothing that is not there; a code used in code and absent from Appendix K is a defect the kit lint reports once `docs/api/error-codes.json` is generated from the appendix.

### 12.2 Cross-cutting codes, quoted from Appendix K.1

Every service emits these eight with its own prefix, generated by the shared middleware and never hand-written (Appendix K rule 6).

| Code suffix | HTTP | When it happens | What the client does | Parent-safe |
|---|---|---|---|---|
| `_VALIDATION_FAILED` | 400 | A field fails a format, range or required check | Bind the field list to the form, focus the first field, do not retry | yes |
| `_PERMISSION_DENIED` | 403 | The caller lacks the permission or the data scope | Hide the action and refresh the permission version; never retry | yes, as "you do not have access to this" |
| `_TENANT_MISMATCH` | 403 | The subject belongs to another tenant | Sign the session out of the wrong tenant context and report; never retry | no |
| `_NOT_FOUND` | 404 | The entity does not exist, or the caller may not know it exists | Return to the list and refresh | yes |
| `_CONCURRENCY_CONFLICT` | 409 | The row version changed since it was read | Re-read, show what changed, let the user merge; never silently overwrite | yes |
| `_IDEMPOTENCY_REPLAY` | 200 | The same idempotency key was seen before | Treat as success and use the returned original result | yes |
| `_RATE_LIMITED` | 429 | Too many calls for this caller, tenant or endpoint | Back off using `Retry-After` with jitter | yes |
| `_DEPENDENCY_UNAVAILABLE` | 503 | A downstream service, broker, database or provider is down or circuit-open | Queue the work offline if the screen supports it, otherwise retry with backoff | yes, as "temporarily unavailable" |

The Gateway emits the same suffixes with the prefix `GATEWAY_`, and the two backends-for-frontends with `BFF_`, for failures that happen before a service is reached (an unroutable tenant, a token that fails validation, an address limit). This document proposes one addition, `_INTERNAL_ERROR` (500), as open point 1.

### 12.3 Namespace ranges

Codes are strings, so the ranges are prefixes and status classes rather than numbers.

| Prefix | Emitted by | Appendix K section | Count today |
|---|---|---|---|
| `IDENTITY_` | Identity | K.2 | 14 |
| `PLATFORM_` | Platform | K.3 | 9 |
| `SCHOOL_` | School | K.4 | 10 |
| `ADMISSIONS_` | Admissions | K.5 | 9 |
| `ACADEMICS_` | Academics | K.6 | 10 |
| `ASSESSMENT_` | Assessment | K.7 | 11 |
| `SCHEDULING_` | Scheduling | K.8 | 10 |
| `ATTENDANCE_` | Attendance | K.9 | 10 |
| `FINANCE_` | Finance | K.10 | 10 |
| `COMMUNICATION_` | Communication | K.11 | 9 |
| `NOTIFICATION_` | Notification | K.12 | 9 |
| `REQUESTS_` | Requests | K.13 | 9 |
| `DOCUMENTS_` | Documents | K.14 | 10 |
| `BEHAVIOR_` | Behavior | K.15 | 9 |
| `REPORTING_` | Reporting | K.16 | 9 |
| `AUDIT_` | Audit | K.17 | 9 |
| `WELLBEING_` | Wellbeing | K.18 | 9 |
| `HR_` | Hr | K.19 | 9 |
| `OPERATIONS_` | Operations | K.20 | 9 |
| `AI_` | Ai | K.21 | 9 |
| `GATEWAY_`, `BFF_` | Gateway, Bff.Web, Bff.Mobile | K.1 suffixes, plus their own section K.23 | 9 each: the eight cross-cutting suffixes, plus `GATEWAY_BODY_TOO_LARGE` (413) and `BFF_APP_VERSION_BELOW_MINIMUM` (403) |

Counts are read from Appendix K, which owns them; they are not maintained here. K.23 is numbered after K.22 so that every existing reference to "K.22 rule N" still resolves.

The prefix is the **long service name in upper case** (Appendix K header): the Requests service emits `REQUESTS_*`, never `RQS_*`, because `RQS` is an AREA code for identifiers and not an error prefix (Appendix L).

Status classes a code may use, and what each means to a client:

| Status | Class | Codes may use it when | Client default |
|---|---|---|---|
| 200 | informational success | The call succeeded and the client should know something (`_IDEMPOTENCY_REPLAY`, `ATTENDANCE_DUPLICATE_MARK`, `REPORTING_PROJECTION_STALE`, `AI_OUTPUT_REQUIRES_REVIEW`) | Continue, show the note |
| 400 | input | Shape, format, required, unknown reference | Fix the form; no retry |
| 401 | authentication | Token missing, expired, second factor pending | Refresh once, then sign in |
| 402 | commercial | A plan quota or a provider decline | Show the limit, the usage and the upgrade action, or the decline; no retry, no `Retry-After` (§9.3) |
| 403 | authorization | Permission, scope, consent, policy | Hide the action; request access |
| 404 | existence | Not found, or existence is the secret (`WELLBEING_ACCESS_DENIED`) | Return to the list |
| 409 | state | Concurrency, workflow state, window closed, duplicate | Re-read and show the allowed actions |
| 410 | gone | Revoked certificate, dead device token | Remove the reference |
| 413 | size | Too many rows, too wide a range, too large a file | Split or schedule |
| 422 | semantic | Well-formed but unprocessable: infeasible solver, virus, prompt injection, no reachable channel | Show the diagnosis; no retry |
| 423 | locked | Account or thread locked pending a human | Show the unlock path |
| 429 | rate | Layer 1 or 2 limit | Back off `Retry-After` |
| 500 | fault | Internal error, audit write failed, chain broken | Show the reference number; alert fires |
| 502 | provider | A provider or webhook target rejected | Retry on the next provider or later |
| 503 | unavailable | Dependency down, circuit open, model host down | Queue offline or retry with backoff |
| 504 | timeout | Solver or gRPC deadline | Offer the partial result or a longer run |

No code uses 201, 204, 301, 302, 405, 406, 415 or 418; those statuses are produced by the framework for framework reasons and carry no catalog code.

### 12.4 Lifecycle rules

| Rule | Detail | Enforcement |
|---|---|---|
| Never removed, only deprecated | Appendix K rule 2: a retired code keeps its row, marked deprecated with the release that retired it and the code that replaced it; the server may keep emitting it for one major version; removing a row breaks mobile builds a family has not updated | `docs/api/error-codes.json` is generated from Appendix K with a `deprecated` object `{ since, replacedBy }`; the generator refuses to run if a code present in the previous release's file is absent |
| Never reused | Appendix K rule 3: a new meaning gets a new code even when the old one is free | Same generator: a code whose `meaning` text changed is a build failure unless the row is new |
| Every code has text in both languages | The Angular and Flutter resource bundles carry `errors.<CODE>.title` and `errors.<CODE>.message` in `en` and `ar` | `TC-API-030`: the missing-translation report in `24-localization-and-calendars.md` §1 treats a catalog code without both texts as an error, not a warning |
| Every code has a test | Appendix K rule 7: generated authorization and validation tests assert the exact code | `TC-API-031`: a test that asserts a status without a code is flagged by an analyzer |
| Parent-safe is a property of the code | Appendix K rule 4 | The generated JSON carries `parentSafe`; the client bundles read it; `TC-API-032` snapshots the guardian rendering of every non-parent-safe code as the generic message plus reference |
| Per-service ownership | A service may add a code only by amending its Appendix K section through the brief-change rule (an ADR and a version bump) | The kit lint compares codes referenced under `docs/plan/06-services/` and `src/` against the generated file |
| Server messages are hints | Appendix K rule 1: `title` and `detail` are English developer text, never shown to an end user, never translated on the server | `TC-API-033`: the web and mobile clients never render `detail`; a Playwright assertion looks for it in the DOM and fails when found |

### 12.5 Versioning of the API surface, in one table

Master brief Section 35 owns the policy; `23-integrations-and-public-api.md` §5 owns the headers and the timeline. Quoted here so an engineer reading only this document knows the shape.

| Element | Rule |
|---|---|
| Version in the path | `/api/v{n}/`, integer, per service; the Gateway routes each major to the image that serves it |
| Support window | 12 months after the successor ships |
| Deprecated version responses | `Deprecation`, `Sunset` and `Link rel="deprecation"` headers on every response; listed in the developer portal with the removal date |
| Within a major | Only additive changes (§11.4) |
| Message contracts | `v<n>` in the routing key; old schema retired two minor releases after the last consumer moves (Appendix E) |
| gRPC | `nibras.<service>.v<n>` package; a new major is a new package beside the old one |

---

## Decisions in force

| Decision | Source | Default if unanswered | Impact if wrong |
|---|---|---|---|
| Money on the wire is a string amount plus a currency code, never a JSON number | This document §1.4; BR-FIN-011 | String | A JSON number is parsed as a double by every JavaScript client, and JOD 333.334 does not survive |
| Bilingual text on the wire is `{ar, en}`, not sibling properties | This document §1.3; `24-localization-and-calendars.md` §2 | Object | Sibling properties multiply every schema and every generated client type by two |
| Keyset lists carry no `totalCount` | This document §2.1 | No count; a `/count` sibling from the read models | A count over a month-partitioned table becomes the slowest query in the product |
| Cursors are signed and bound to sort and filter | This document §2.1 | Signed | A cursor from one query replayed on another produces a silent gap or duplicate |
| A missing `If-Match` on a write is 400, not 428 | This document §4 | 400 with field `If-Match` | 428 has no catalog code; adding a status class needs an Appendix K change |
| `Idempotency-Key` TTL is 24 hours, 7 days for payment callbacks | This document §5 | 24 h and 7 d | Shorter loses offline replays after a school trip; longer grows `redis-state` beyond a day of writes |
| Bulk maximum is 500 items; more is an import job | This document §7 | 500 | Larger batches breach the write budget; smaller ones make the mobile outbox chatty |
| Quota breaches are 402 with the limit, the usage and the upgrade action and no `Retry-After`; rate breaches are 429 with `Retry-After` | Master brief Section 19; this document §9.2 and §9.3; Appendix K | As stated | A client that retries a 402 hammers a limit that will not reset |
| `AI_USAGE_LIMIT_REACHED` is the one meter that is not a plain 402: 200 with a degradation to rung 1 on an assist request, 402 only on an explicit rung 4 request | Appendix K.21, which owns the status; `25-ai-and-assist-ladder.md` §8 | As stated | A school that has spent its AI allowance sees errors where the ladder promises a working rung 1 fallback |
| The hop rule is enforced by interceptor and architecture test | This document §10.4 | Enforced | Two hops silently become three and the p95 budget is gone |
| Spectral plus `oasdiff` gate every spec change | This document §11 | Both | A breaking change reaches a phone that cannot update |
| Problem `type` is a URN, not a URL | This document §8 | `urn:nibras:problem:<code>` | A URL bakes a host into every mobile build |
| `<SERVICE>_INTERNAL_ERROR` is emitted by the middleware | This document §8, open point 1 | Emitted, proposed for Appendix K.1 | Without it a 500 has no code and no test can assert it |

## Dependencies on other documents

| This document assumes | Stated in | Checked on |
|---|---|---|
| Service names, prefixes, permission namespaces | Appendix L, Appendix B | Every lint run |
| The per-service code rows | Appendix K | Every lint run once `docs/api/error-codes.json` is generated |
| The eight cross-cutting codes, the rules, and the Gateway and backend-for-frontend codes | Appendix K.1, K.22 and K.23 | Group F review |
| Token issuance, permission version, client credentials, key rotation | `12-security-privacy-safety.md` §3 and §9 | Group F review |
| The caching map that decides `Cache-Control` and the L1 fallback | `21-performance-engineering.md` | Group C review |
| The outbox transaction that stores an idempotency result | `11-messaging-architecture.md` | Group C review |
| Public API keys, webhooks, deprecation headers | `23-integrations-and-public-api.md` | Group F review |
| `LocalizedText`, normalization, numerals | `24-localization-and-calendars.md` | Group F review |
| The generated client tool and the versions of Scalar, Spectral and `oasdiff` | `19-dependency-and-license-inventory.md` | Group E review |
| Load scenario N-06 for the noisy-neighbour proof | Appendix N; `16-test-strategy.md` | Group E review |

## Open points

| Question | Default | Owner | Impact if the default is wrong | L | I | Score | In the register |
|---|---|---|---|---|---|---|---|
| 1. Add `_INTERNAL_ERROR` (500) as the ninth cross-cutting suffix in Appendix K.1? | Emit it from the middleware now; amend Appendix K.1 by ADR with the next brief version | Architect | Without the row, the never-removed rule cannot protect it and no generated test asserts it | 2 | 1 | 2 | none |
| 2. Should `q` free-text search also cover the transliteration key in `24-localization-and-calendars.md` §3 by default, or only on the duplicate-detection endpoints? | Only on duplicate detection and the admissions search; the general `q` uses folding and trigram | Architect, with the School service owner | Default-on transliteration produces surprising matches in a class list of 30; default-off misses "Mohammed" for "محمد" in the directory | 3 | 2 | 6 | none |
| 3. Per-plan Gateway limits (3,000, 12,000, 40,000 per minute) and per-key limits (60, 300, 1,000) are starting values | As stated, revised by the N-01 and N-06 runs | Architect, with the product owner for the plan tiers | A limit set too low makes the morning attendance peak a 429 storm; too high defeats the noisy-neighbour protection | 3 | 3 | 9 | RISK-19 |
| 4. Does the public API expose `/count` siblings, or only first-party clients? | First-party only until the read-only public API tiering in `02-competitive-gap-analysis.md` is decided by ADR | Product owner | Integrators paginate without knowing the total, which OneRoster consumers expect | 2 | 2 | 4 | RISK-42 |

## Review record

| Date | Reviewer | Verdict | Blocking items |
|---|---|---|---|
| 2026-09-22 | Group F review, round 1 (independent adversarial scorecard) | Blocked: the group scored below 4 on Completeness, Consistency, Feasibility, Risk honesty, Testability and Distinctiveness | None of the group's blocking gaps was in this document |
| 2026-09-26 | Group F review, round 2 | Blocked: the group scored below 4 on Completeness, Consistency, Risk honesty and Testability | This document was labelled Group D and its dependency checks pointed at a Group D review (Consistency) |
| 2026-09-26 | Round 3 remediation | Amended; awaiting the round 3 score | Relabelled Group F with every dependency check at the Group F review; §10.4 places `GrpcHopRules` with the architecture rules of `07-solution-structure.md` §10.3 instead of the Testing block |

## How this document is verified

| Claim | Proof | Where it runs |
|---|---|---|
| Every endpoint follows the URL, naming, JSON and header rules | The 28 Spectral rules in §11.3 over every committed `docs/api/<service>.v<n>.json`; `nibras-spec-drift` compares committed to generated | Every pull request |
| Every list uses the one envelope and its maximum page size | `TC-API-001`: a generated test per list endpoint requests `pageSize=1000` and asserts clamping to the declared maximum and the envelope shape; `TC-API-002`: a keyset walk over 1,001 seeded rows returns each row once with no gaps, in every declared sort, forwards and backwards | Every pull request per service |
| A cursor from another sort, filter or endpoint is refused | `TC-API-003` | Every pull request |
| The filter and sort grammar parses and refuses as specified | `TC-API-010` to `TC-API-014` in `Nibras.BuildingBlocks.Web.Tests`: every operator, escaping, limits, sensitive-field refusal, data scope applied before filter | Every pull request touching `BuildingBlocks` |
| `If-Match` is required and a mismatch is `_CONCURRENCY_CONFLICT` with the current ETag | `TC-API-020`: two clients read, both write, second gets 409 with `params.currentEtag`; `TC-API-021`: a missing header is 400 with field `If-Match` | Every pull request per service, generated from the OpenAPI extension |
| `Idempotency-Key` replays, refuses fingerprint mismatch, fails closed for required endpoints | `TC-API-022` to `TC-API-025` against Redis through Testcontainers, including a Redis outage | Every pull request touching `BuildingBlocks`; nightly for payment endpoints in Finance |
| The long-running-operation contract | `TC-API-040`: a report-card batch returns 202 with `Location`, progress arrives on the hub at most once per second, the job resource reaches `succeeded` with a result URL; `TC-API-041`: cancellation ends in `cancelled` with a partial summary | Nightly against Assessment and Finance workers |
| Bulk endpoints return per-item results and respect 500 | `TC-API-050`: 500 attendance marks with 3 invalid rows in `independent` mode; `TC-API-051`: `allOrNothing` rolls back; `TC-API-052`: 501 items is 400 | Every pull request per service with a bulk endpoint |
| Problem Details shape and the `code` field | `TC-API-030` to `TC-API-033` in §12.4; a snapshot test per catalog code asserts the exact JSON shape and that `detail` never reaches the DOM | Every pull request |
| The three rate-limit layers and the 429 contract | `TC-API-060`: Gateway address and tenant limits with `Retry-After` and `RateLimit-*`; `TC-API-061`: service per-user and per-key buckets; `TC-API-062`: a plan quota is 402 and never 429; N-06 noisy neighbour keeps the small tenant inside budget | Every pull request for the Gateway and `BuildingBlocks`; nightly load tier for N-06 |
| gRPC metadata, deadlines and the one-hop rule | `TC-API-070`: a call without `nibras-tenant-id` is refused; `TC-API-071`: a tenant mismatch between metadata and token is `_TENANT_MISMATCH`; `TC-API-072`: a nested gRPC call is refused with the hop code; the `GrpcHopRules` architecture test (`07-solution-structure.md` §10.3); `buf lint` and `buf breaking` on `src/Contracts/` | Every pull request |
| Breaking changes are caught | `oasdiff` against `docs/api/released/` in the product pipeline, the gate SL-INT-402 builds; when SL-INT-402 is accepted, `architecture-reviewer` confirms that a seeded breaking change fails the gate | Every pull request once SL-INT-402 is built; the seeded-change check at its acceptance |
| Error codes are never removed and never reused | The generator of `docs/api/error-codes.json` compares against the previous release's file; both language bundles carry every code | Every pull request touching Appendix K or the bundles |
| This document agrees with the catalogs | kit-lint R01, R02 and R17 for section and appendix references and Mermaid types, R19 for service-prefixed error codes (Appendix K) and routing keys, and R31 for database and image names against Appendix L; `plan-consistency-checker` compares the rest, project and exchange names included, with `05-service-catalog.md`, `12-security-privacy-safety.md`, `23-integrations-and-public-api.md` and `24-localization-and-calendars.md` | kit-lint on every change under `docs/`; the comparison at the Group F review and on every change to any of those documents |

### Test cases

This document defines the API convention tests below; the claims above and `23-integrations-and-public-api.md` cite them. Every case runs against `Nibras.BuildingBlocks` or a real service endpoint with the clock pinned and `CultureInfo.InvariantCulture`.

| Test case | What it proves | Covers |
|---|---|---|
| TC-API-001 | Given a list endpoint whose declared maximum is 200, when a client requests `pageSize=1000`, then the response is 200 rather than 400 with `pageSize` 200 and the full envelope, and a request without `pageSize` returns 50 | REQ-API-007 |
| TC-API-002 | Given 1,001 seeded rows, when a client walks a keyset list forwards and backwards at `pageSize` 50 in every declared sort, then each row appears exactly once with no gap, every page carries `items`, `pageSize`, `nextCursor`, `previousCursor`, `hasMore`, `sort` and `filterHash`, and no page carries a total count | REQ-API-006 |
| TC-API-003 | Given a `nextCursor` issued for `sort=+lastName,+id` on the students list, when it is sent with another sort, another filter or to another endpoint, then each request is 400 `_VALIDATION_FAILED` with field `cursor` and returns no rows | REQ-API-006 |
| TC-API-010 | Given a list endpoint that declares its filterable and sortable fields, when each of the 12 operators of §3.1 is sent and `sort=lastName` is requested, then each predicate returns exactly the expected seeded rows, `between` is inclusive at both ends, an undeclared field is 400, and the response echoes the sort as `+lastName,+id` | REQ-API-008 |
| TC-API-011 | Given a string value holding a literal comma sent as `%2C` in `filter[name][in]`, when the grammar parses it, then it is one list member, not two, and a percent-encoded `%2E%2E` inside a `between` bound is not read as the range separator | REQ-API-008 |
| TC-API-012 | Given the §3.1 limits, when a request carries 10 predicates, a 100-member list and a 200-character value, then it is accepted, and when any one carries 11, 101 or 201, then it is 400 `_VALIDATION_FAILED` | REQ-API-008 |
| TC-API-013 | Given a field classified Sensitive in Appendix J, when a client sends `filter[medicalNotes][contains]=asthma`, then it is 403 `SCHOOL_PERMISSION_DENIED` and no rows are read, and every Wellbeing list refuses every filter | REQ-API-008 |
| TC-API-014 | Given a teacher whose data scope is section 4B, when the teacher filters the students list by the section id of 5A, then the response holds 0 rows, because the scope is applied before the filter, and a filter naming a field the teacher may not see is 403 `_PERMISSION_DENIED` | REQ-API-008 |
| TC-API-020 | Given two clients that read the same student at the same `ETag`, when both send `PATCH` with that `If-Match`, then the first is 200 and the second is 409 `_CONCURRENCY_CONFLICT` with `params.currentEtag` equal to the first write's `ETag`, and the stored row holds only the first change | REQ-API-014 |
| TC-API-021 | Given an aggregate root, when `PUT`, `PATCH` or `DELETE` arrives without `If-Match`, then each is 400 `_VALIDATION_FAILED` with field `If-Match` and the stored row is unchanged | REQ-API-015 |
| TC-API-022 | Given an attendance mark `POST` with an `Idempotency-Key` that succeeded, when the same key and body are sent 23 hours later, then the reply is 200 with the original body verbatim and `Idempotency-Replayed: true`, one record exists, and a payment-callback key still replays after 6 days | REQ-API-016 |
| TC-API-023 | Given a stored `Idempotency-Key`, when the same key arrives with a different body, then it is 400 `_VALIDATION_FAILED` with field `Idempotency-Key` and `params.reason` `fingerprintMismatch`, and the original stored result is unchanged | REQ-API-016 |
| TC-API-024 | Given a request with an `Idempotency-Key` still running, when a second request with the same key arrives, then it is 409 `_CONCURRENCY_CONFLICT` with `Retry-After: 2`, and the retry after completion receives the replay | REQ-API-016 |
| TC-API-025 | Given `redis-state` stopped through Testcontainers, when a `POST` reaches an endpoint marked `x-nibras-idempotency: required`, then it is 503 `_DEPENDENCY_UNAVAILABLE` with `Retry-After: 5` and nothing is written, while an endpoint where the key is optional proceeds | REQ-API-016 |
| TC-API-030 | Given the generated `docs/api/error-codes.json`, when the missing-translation report runs over the Angular and Flutter bundles, then every catalog code has `errors.<CODE>.title` and `errors.<CODE>.message` in `en` and `ar`, and deleting one Arabic message makes the report fail with an error naming the code | REQ-API-011 |
| TC-API-031 | Given every catalog code, when the per-code snapshot test runs, then each error body is Problem Details with `type` `urn:nibras:problem:<code>`, the exact `code` and a `correlationId` equal to the response header, and a test that asserts a status without a code is flagged by the analyzer | REQ-API-009 |
| TC-API-032 | Given a guardian surface, when each code with `parentSafe: false` is rendered, then the snapshot shows the generic message with the reference number and none of the code's own text | REQ-API-012 |
| TC-API-033 | Given an error whose `detail` names a session identifier, when the web and mobile clients render it, then the message comes from the bundle for the code in the active language and the Playwright assertion finds 0 occurrences of the `detail` text in the DOM | REQ-API-010 |
| TC-API-040 | Given a report-card batch of 800 cards, when it is started, then the reply is 202 with `Location` of the job resource, at most one `jobProgress` per second arrives on `/hubs/jobs`, and the job reaches `succeeded` with a result URL | REQ-API-018 |
| TC-API-041 | Given a running job at 312 of 800 units, when `POST /jobs/{id}/cancel` is sent, then the job ends in `cancelled` with a partial summary counting the units already done, and its state never changes again | REQ-API-019 |
| TC-API-050 | Given 500 attendance marks of which 3 are invalid, sent in `independent` mode, then the reply is 200 with a summary of 497 succeeded and 3 failed, each failed item carries its Problem Details, and the results follow request order | REQ-API-017 |
| TC-API-051 | Given 500 items of which 1 is invalid, sent in `allOrNothing` mode, then the reply is 200 with 499 items `rolledBack` and the failing item carrying its problem, and nothing is stored | REQ-API-017 |
| TC-API-052 | Given a bulk request of 501 items, then it is 400 `_VALIDATION_FAILED` with `params.maxItems` 500 and no item is processed | REQ-API-017 |
| TC-API-060 | Given the Gateway limit of 600 unauthenticated requests per minute per address, when the 601st arrives inside the minute, then it is 429 `GATEWAY_RATE_LIMITED` with `Retry-After` between 1 and 60 and `RateLimit-Remaining: 0`, the tenant bucket refuses the same way, and every authenticated response before it carried `RateLimit-Limit`, `RateLimit-Remaining` and `RateLimit-Reset` | REQ-API-020 |
| TC-API-061 | Given the service `write` policy of 120 per minute per user, when the 121st write arrives, then it is 429 `<SERVICE>_RATE_LIMITED` with `params.scope` `user`, and an API key over its own per-minute limit is 429 with `params.scope` `apiKey` | REQ-API-020 |
| TC-API-062 | Given a tenant at its plan quota, when one more metered call arrives, then it is 402 `PLATFORM_PLAN_LIMIT_REACHED` with `params.limit`, `params.usage` and `params.upgradeAction`, no `Retry-After`, and never 429 | none |
| TC-API-070 | Given a gRPC call to School without `nibras-tenant-id` on a method that is not platform-scoped, when it arrives, then the callee refuses it before the handler runs and reads no data | REQ-API-024 |
| TC-API-071 | Given a gRPC call whose `nibras-tenant-id` differs from the service token's tenant claim, when it arrives, then it is `PERMISSION_DENIED` with code `_TENANT_MISMATCH` and reads no data | REQ-API-024 |
| TC-API-072 | Given an inbound gRPC call carrying `nibras-hop: 1`, when its handler opens a second gRPC call, then the call is refused with `FAILED_PRECONDITION` and `<SERVICE>_HOP_LIMIT_EXCEEDED`, and the second callee receives nothing | REQ-API-023 |
