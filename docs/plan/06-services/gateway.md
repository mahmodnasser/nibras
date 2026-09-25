# Gateway

> Service sheet, plan document 06. Group C. It refines reference architecture Section 8.1 and the Gateway row of `05-service-catalog.md`; names come from Appendix L, error codes from Appendix K. The Gateway owns no data and publishes no events, so the entity, event and reference-copy sections say "none, by design" and the REST section describes routes and upstream calls. Where this sheet adds something the brief does not state, the addition is listed under Decisions in force or Open points.

**Group** C · **Requirement areas covered** GW (all 9 rows), plus REQ-INT-006 and REQ-INT-007 · **Last updated** 2026-09-21 by the platform plan

The Gateway is the single public entry point behind the edge proxy. Every request from a browser, the mobile app, an integrator or a public page passes through it once: it resolves the tenant from the custom domain, the subdomain or the header a mobile or API-key caller sends, validates the token locally against Identity's published keys, exchanges an API credential for a short internal token, applies the first layer of rate limiting per source address and per tenant, enforces request size limits, CORS and security headers, answers maintenance mode, adds the correlation id and trace context, routes by path prefix to the owning service or backend-for-frontend, degrades one route without taking the rest down, publishes the deprecation headers of a retiring API version, and serves the aggregated OpenAPI document. It holds no business rule, stores nothing but rate-limit counters, and trusts nothing it did not verify.

| Fact | Value | Source |
|---|---|---|
| Tier | 1 | Appendix L |
| AREA code | `GW` | Appendix L |
| Database | none; `redis-state` for rate-limit counters, read-only entries of `redis-cache` for tenant resolution | Appendix L, `21-performance-engineering.md` §1.21 |
| Exchange | none | Appendix L |
| Images | `nibras/gateway` | Appendix L |
| Worker | none | Appendix L |
| Technology | YARP (MIT) on ASP.NET Core, project `Nibras.Gateway` | `04-architecture-overview.md`, `07-solution-structure.md` §2.1 |
| Build phase | 1 | `05-service-catalog.md` |
| Service level class | Gateway class: 99.9% monthly availability; p95 under 150 ms for token validation at the edge (REQ-GW-009) | Master brief Section 31 |
| Sensitivity | none stored; tokens, keys and bodies pass through and are never logged | Reference architecture Section 8.0 |
| Synchronous dependencies | none as gRPC; HTTP to Identity's token endpoint for API credential exchange and to Platform's tenant resolution on a cache miss | Reference architecture Section 8.0, section 6 |
| Scaling profile | Stateless, scaled on requests per second; 3 replicas as the master brief Section 34 starting point | `05-service-catalog.md` |
| Why the boundary exists | Security level: one enforcement point for token validation, tenant resolution and traffic limits, kept stateless so nothing behind it trusts the network | `05-service-catalog.md` |

---

## 2. Responsibilities

| The Gateway owns | Detail |
|---|---|
| Routing | Path prefix to upstream cluster for the twenty services, both backends-for-frontends, Identity's OpenID Connect paths and Communication's SignalR hubs (section 5.1) |
| Tenant resolution | Custom domain, then subdomain, then the `X-Nibras-Tenant-Id` header for mobile and API-key callers only; a token whose tenant differs is `GATEWAY_TENANT_MISMATCH` (T-GW-01); a request with no tenant is refused except on platform routes (REQ-GW-001, REQ-GW-002) |
| Token validation | Local JWT validation against Identity's key set with overlap (T-GW-02), audience and issuer checks, the seeded-administrator restriction (`12-security-privacy-safety.md` §8 item 3), the revoked-subject mark of a leaver (TC-IDN-056) |
| API credential exchange | `nbk_` and `nbp_` credentials exchanged at Identity for a 5-minute internal token, held 60 s in L1 by key id and permission version, refused in a query string, refused for an operation not tagged `public` (`23-integrations-and-public-api.md` §1 and §2.3) |
| Rate limiting, layer 1 | Per source address and per tenant with the plan's bucket (`22-api-conventions-and-error-catalog.md` §9.1), 429 with `Retry-After` and the `RateLimit-*` headers |
| Request hygiene | Body size limits, CORS, security headers and content security policy (REQ-GW-004), `security.txt` (REQ-SEC-018) |
| Administrative IP allowlist | Routes marked administrative are refused from an address outside the tenant's allowlist (REQ-IDN-047) |
| Maintenance mode | Platform-wide or per tenant, 503 with the maintenance message and `Retry-After`, health routes untouched (REQ-GW-006) |
| Resilience | Per-cluster timeouts, circuit breakers and an honest 503 for one route while the rest keeps working (REQ-GW-007) |
| Correlation | `X-Nibras-Correlation-Id` and `traceparent` issued when absent and echoed on every response (REQ-GW-008) |
| Versioning at the edge | `Deprecation`, `Sunset` and `Link` headers on a deprecated version and 404 with `params.successor` after removal (REQ-INT-006, REQ-INT-007) |
| Aggregated OpenAPI | The merged document of every service, and the `public`-tagged subset for the developer portal (REQ-GW-005) |

### Not responsible for

| The Gateway does not own | Owner | How the Gateway relates to it |
|---|---|---|
| Issuing tokens, keys, permissions and data scopes | Identity | Validates locally; exchanges API credentials at Identity's token endpoint |
| Authorization of a request to a resource | Each service, through `Nibras.BuildingBlocks.Authorization` | Checks authentication, tenant and the public tag only; the permission check happens in the service |
| Tenants, domains, plans and maintenance windows | Platform | Reads Platform's cache entries; asks Platform's resolution route on a miss |
| Per-user and per-endpoint limits (layer 2), plan quotas (layer 3) | Each service, Platform | Never counts them |
| Screen composition | Bff.Web, Bff.Mobile | Routes `/bff/web/v1/` and `/bff/mobile/v1/` to them |
| Public TLS, HTTP/3, static web assets, bot protection challenge pages | The edge proxy and CDN (`15-deployment-and-operations.md`) | Receives the forwarded request with the first trusted `X-Forwarded-For` hop |
| SignalR presence and messages | Communication | Proxies WebSocket upgrades on `/hubs/` |
| The developer portal pages | The Platform web workspace | Serves only the OpenAPI documents the portal renders |

---

## 3. Requirements covered

| Range or identifier | What it binds here |
|---|---|
| REQ-GW-001 to REQ-GW-009 | Routing, tenant resolution, missing tenant, rate limits, size limits and headers, aggregated OpenAPI, maintenance, graceful degradation, correlation id, validation latency |
| REQ-INT-006, REQ-INT-007 | Version support for 12 months after the successor and the deprecation headers |
| REQ-SEC-005 | The tenant-isolation suite's header and token swaps are refused here first |
| REQ-SEC-018 | `security.txt` is published |
| REQ-IDN-047 | Administrative IP allowlist enforced at the edge |
| REQ-PERF-031 | Brotli, HTTP/2 to upstreams, pooled connections |

---

## 4. Aggregates and entities

None, by design: the Gateway owns no database (Appendix L), so the only state it keeps are rate-limit counters in `redis-state`, which expire with their window.

---

## 5. Routes, upstream calls and the Gateway's own endpoints

### 5.1 Routes

Every route carries the metadata generated from the aggregated OpenAPI at start-up: `public` tag, administrative flag, deprecation date and sunset, anonymous or authenticated, and the rate-limit class. A path that matches no route is `GATEWAY_NOT_FOUND`.

| Path prefix | Upstream cluster | Authentication | Tenant | Layer-1 policy | Notes |
|---|---|---|---|---|---|
| `/connect/`, `/.well-known/openid-configuration`, `/.well-known/jwks` | `identity-api` | none at the Gateway | resolved from host when present | per address, 600 per minute unauthenticated | Sign-in, token and discovery |
| `/api/v1/identity/auth/`, `/api/v1/identity/public/`, `/api/v1/identity/saml/` | `identity-api` | anonymous | from host | per address, the tighter `auth` profile | Credential and registration routes |
| `/api/v1/identity/` | `identity-api` | token or personal token (only the caller's own tokens, document 23 §1) | required | per tenant | |
| `/api/v1/platform/signups`, `/api/v1/platform/public/`, `/api/v1/platform/legal-documents/current`, `/api/v1/platform/payment-callbacks/`, `/api/v1/platform/lti/jwks`, `/api/v1/platform/lti/authorizations`, `/api/v1/platform/lti/deep-linking-returns` | `platform-api` | anonymous; signups behind the edge bot challenge | from host or none (platform route) | per address | |
| `/api/v1/platform/tenant-resolution/` | not routed | never public | none | none | Internal route, reachable only in-cluster from the Gateway itself |
| `/api/v1/platform/` | `platform-api` | token; tenant keys only on operations tagged `public` | required, except operations marked platform-scoped | per tenant | Operator routes need a platform-tenant token |
| `/api/v1/school/`, `/api/v1/admissions/`, `/api/v1/academics/`, `/api/v1/assessment/`, `/api/v1/scheduling/`, `/api/v1/attendance/`, `/api/v1/finance/`, `/api/v1/communication/`, `/api/v1/notification/`, `/api/v1/requests/`, `/api/v1/documents/`, `/api/v1/behavior/`, `/api/v1/reporting/`, `/api/v1/audit/`, `/api/v1/hr/`, `/api/v1/operations/` | `<service>-api`, one cluster per service | token; keys and personal tokens only on `public` operations | required | per tenant | Public verification and feed routes of Documents and Scheduling are anonymous by their route metadata |
| `/api/v1/wellbeing/` | `wellbeing-api` | first-party token only; keys and personal tokens always refused | required | per tenant | Never public (document 23 §1) |
| `/api/v1/ai/` | not routed | none | none | none | Ai is reached only through the backends-for-frontends in-cluster (`07-solution-structure.md` §2.4) |
| `/bff/web/v1/` | `bff-web` | first-party web token | required | per tenant | Never documented in the portal |
| `/bff/mobile/v1/` | `bff-mobile` | first-party mobile token with `X-Nibras-Tenant-Id` | required | per tenant; the sync route is exempt from the burst cap per `15-deployment-and-operations.md` | |
| `/hubs/` | `communication-api` | token in the access-token query parameter only for the WebSocket upgrade, as SignalR requires, never logged | required | per tenant, connection rate | WebSocket upgrade with long idle timeout |
| `/api/v{n}/…` for a retired `n` | not routed | none | none | none | 404 `GATEWAY_NOT_FOUND` with `params.reason = "versionRetired"` and `params.successor` |

### 5.2 Request pipeline, in order

| # | Step | Refusal |
|---|---|---|
| 1 | Correlation id and `traceparent` issued if absent | none |
| 2 | Maintenance check, platform-wide then per tenant, skipped for `/health/` | 503 `GATEWAY_DEPENDENCY_UNAVAILABLE` with `params.reason = "maintenance"` and `Retry-After` to the window end |
| 3 | Body size limit per route class (1 MiB JSON, 64 KiB on anonymous routes) | 413 `GATEWAY_BODY_TOO_LARGE` with the route's limit in `params` (Appendix K.23) |
| 4 | Tenant resolution: host through the Platform entry, then the header for mobile and credential callers only | 400 `GATEWAY_VALIDATION_FAILED` (`tenantUnresolved`); 409 `PLATFORM_PROVISIONING_IN_PROGRESS` while Saga 1 runs |
| 5 | Layer 1 rate limit per source address, then per tenant with the plan's bucket | 429 `GATEWAY_RATE_LIMITED` with `Retry-After` |
| 6 | Credential presented in a query string | 400 `GATEWAY_VALIDATION_FAILED` and the key flagged for rotation (document 23 §2.3) |
| 7 | API credential exchange, or JWT validation: signature against the key set, lifetime, issuer, audience | 401 `IDENTITY_TOKEN_EXPIRED` for an expired token, so the client refreshes once; 401 `IDENTITY_TOKEN_INVALID` for a malformed token, a failed signature, or a foreign issuer, audience or tenant, so the client discards the session and signs in again (Appendix K.2) |
| 8 | Token tenant equals the resolved tenant; `test` key only on a sandbox and `live` never on one | 403 `GATEWAY_TENANT_MISMATCH` |
| 9 | Revoked-subject mark for the token's subject | 401 `IDENTITY_TOKEN_EXPIRED`, forcing a refresh that Identity refuses |
| 10 | Audience restriction of a seeded-credential token, key or token on an untagged operation, key on a Wellbeing route | 403 `GATEWAY_PERMISSION_DENIED` |
| 11 | Administrative route from an address outside the tenant's allowlist | 403 `GATEWAY_PERMISSION_DENIED` with `params.reason = "ipNotAllowed"`, audited through the upstream's audit path on the next allowed request of that tenant (Open point 2) |
| 12 | Forward with `X-Nibras-Tenant-Id`, the internal token, correlation and trace headers; strip hop-by-hop and client-supplied internal headers | none |
| 13 | Per-cluster timeout (30 s default, 5 s for `/connect/token`), circuit breaker per cluster | 503 `GATEWAY_DEPENDENCY_UNAVAILABLE` with `Retry-After` for that route only |
| 14 | Response: security headers, `RateLimit-*`, deprecation headers, `X-Nibras-Correlation-Id`, Brotli | none |

### 5.3 Upstream calls the Gateway itself makes

| Call | Target | When | Timeout | Fallback |
|---|---|---|---|---|
| `GET /.well-known/jwks` | Identity | At start-up, every hour, and on an unknown `kid` (at most once per 30 s) | 2 s | The cached key set (`nibras:platform:identity:jwks:current:v1`, 5 min L1, 1 h L2) |
| `POST /connect/token` with the token-exchange grant, client `svc-gateway` | Identity | First use of an API credential, then after the 60 s L1 entry lapses or is evicted | 2 s | 503 `GATEWAY_DEPENDENCY_UNAVAILABLE`; never serve a stale exchange past 60 s |
| `GET /api/v1/platform/tenant-resolution/{host}` with the Gateway service token | Platform | Cache miss on `nibras:platform:platform:tenant-by-host:{host}:v1` | 2 s | Negative cache 10 s; unknown host refused with `GATEWAY_NOT_FOUND` |
| `GET /openapi/v1.json` of each routed service | Every service except Ai | At start-up and every 10 minutes | 5 s | The last merged document stays in memory |

### 5.4 The Gateway's own endpoints

| Method | Path | Permission | Request | Response | Errors | Idempotent |
|---|---|---|---|---|---|---|
| GET | `/openapi/v1.json` | first-party token | none | the aggregated document of every routed service, one group per service (REQ-GW-005) | `GATEWAY_PERMISSION_DENIED` without a first-party token | safe, `ETag` |
| GET | `/openapi/public/v1.json` | none, public | none | only operations tagged `public`, the developer portal's reference (document 23 §3.1) | none | safe, `public, max-age=300` |
| GET | `/docs` | first-party token | none | Scalar over the aggregated document | `GATEWAY_PERMISSION_DENIED` | safe |
| GET | `/.well-known/security.txt` | none, public | none | contact, policy and expiry per `12-security-privacy-safety.md` §12.2 | none | safe |
| GET | `/health/live`, `/health/ready`, `/health/startup` | none, cluster network only | none | 200 or 503; `ready` requires the key set and the route table | none | safe |

---

## 6. gRPC

None, by design: the Gateway makes no gRPC call and exposes none (`22-api-conventions-and-error-catalog.md` §10.4), so it can never add a synchronous hop.

---

## 7. Events published and consumed

None, by design: the Gateway owns no data, so it publishes nothing and binds no queue (Appendix L). The one push it reacts to is the `state:invalidate` channel of `Nibras.BuildingBlocks.Caching` in `redis-state` (`21-performance-engineering.md` §2.6), which evicts its L1 entries for tenant resolution, maintenance state, the key set and API credential exchanges within 2 seconds; that channel is a cache mechanism, not an integration event.

---

## 8. Sagas and workflows

The Gateway owns no workflow and takes part in no saga. It is the enforcement point that three workflow rows rely on: WF-IDN-06 `Revoked to Revoked` (a leaver's cached token refused at the gateway, TC-IDN-056), WF-PLT-01 step 7 (the routing entry published by Platform, before which the Gateway answers `PLATFORM_PROVISIONING_IN_PROGRESS`), and WF-INF-02 (the canary share, applied by the deployment layer in front of the Gateway's clusters, not by route rules).

---

## 9. Local reference copies

None, by design: the Gateway reads Platform's and Identity's cache entries through `Nibras.BuildingBlocks.Caching` with read-only Redis permissions and keeps nothing it would have to reconcile.

---

## 10. Background work

The Gateway runs no scheduled job. It runs four hosted services inside the process:

| Hosted service | Cadence | What it does | Progress |
|---|---|---|---|
| `KeySetRefresher` | hourly and on an unknown `kid` | Refreshes Identity's signing key set with the overlap window | none |
| `OpenApiAggregator` | start-up and every 10 minutes | Merges the services' documents, rebuilds route metadata (public tag, administrative flag, deprecation, rate class) | none |
| `InvalidationSubscriber` | continuous | Listens on `state:invalidate` and drops matching L1 entries | none |
| `CircuitStatePublisher` | every 10 s | Exposes per-cluster breaker state as metrics for the health board | none |

---

## 11. Permissions, notifications, settings, error codes

### 11.1 Permissions

The Gateway declares no Appendix B permission and checks none: permission checks run in the services. It enforces only authentication, the tenant match, the `public` tag for credentials, the Wellbeing exclusion, the seeded-credential audience restriction and the administrative IP allowlist.

### 11.2 Notifications

None: the Gateway triggers no Appendix C row. Its alerts (`GatewayErrorRateHigh`, `ServiceUnavailable`, `CalendarScaleUpMissed`) are operational alerts in `15-deployment-and-operations.md`, not notifications.

### 11.3 Settings read (Appendix G)

| Group → setting | Read from | Used for |
|---|---|---|
| Security → IP allowlist | Platform settings entry for scope `security` | Step 11 of the pipeline |
| Maintenance windows (Platform's announcements, not an Appendix G group) | Platform maintenance entries | Step 2 |
| Plan rate-limit bucket (per plan in `22-api-conventions-and-error-catalog.md` §9.1) | The tenant resolution entry | Step 5 |

### 11.4 Error codes (Appendix K and `22-api-conventions-and-error-catalog.md` §12.3)

| Code | HTTP | Raised here when |
|---|---|---|
| `GATEWAY_VALIDATION_FAILED` | 400 | Tenant unresolved, credential in a query string |
| `GATEWAY_BODY_TOO_LARGE` | 413 | Body over the route class limit (step 3) |
| `GATEWAY_PERMISSION_DENIED` | 403 | Credential on an untagged operation, key on Wellbeing, seeded-credential restriction, IP allowlist, aggregated document without a first-party token |
| `GATEWAY_TENANT_MISMATCH` | 403 | Header or token tenant differs from the resolved tenant; `test` key on a live tenant or `live` key on a sandbox |
| `GATEWAY_NOT_FOUND` | 404 | No route; unknown host; retired API version |
| `GATEWAY_RATE_LIMITED` | 429 | Layer 1 per address or per tenant |
| `GATEWAY_DEPENDENCY_UNAVAILABLE` | 503 | Maintenance, an open circuit, Identity or Platform unreachable on a cold cache |
| `GATEWAY_CONCURRENCY_CONFLICT`, `GATEWAY_IDEMPOTENCY_REPLAY` | 409, 200 | Never raised here; listed because the shared middleware generates all eight suffixes |
| `PLATFORM_PROVISIONING_IN_PROGRESS` | 409 | Tenant routes while Saga 1 runs |
| `IDENTITY_TOKEN_EXPIRED` | 401 | Expired token or revoked-subject mark |
| `IDENTITY_TOKEN_INVALID` | 401 | Malformed token, failed signature, foreign issuer or audience |

---

## 12. Caching and hot paths

The caching rows are `21-performance-engineering.md` §1.21 and the Redis prefixes and ACL for `svc_gateway` are §2.2 and §2.3; both are binding. Service-specific additions:

| Data | Key | Tags | L1 | L2 | Invalidated by | Never cached |
|---|---|---|---|---|---|---|
| API credential exchange result | in-process only, keyed by `key_id` and `perm_ver` | `apikey:{keyId}` | 60 s | none | `state:invalidate` on revocation (document 23 §2.6) | the credential secret |
| Identity key set | `nibras:platform:identity:jwks:current:v1` (Identity's entry, read-only) | none | 5 min | 1 h ± 10% | Key rotation handler; an unknown `kid` | private key material |
| Maintenance state | Platform's maintenance entries (read-only) | `tenant` | 10 s | 5 min | `state:invalidate` from `MaintenanceWindowJob` | nothing |
| Revoked-subject mark | `state:revoked:{tenant}:{userId}` in `redis-state` (Identity writes) | none | 5 s | the mark's own 15-minute expiry | Expiry | nothing |
| Route metadata | in-process | none | until the next aggregation | none | `OpenApiAggregator` | nothing |

| Hot path | Budget |
|---|---|
| Authenticated request with warm caches | 0 network calls before forwarding except one `redis-state` rate-limit increment and one revoked-mark read; p95 added latency under 5 ms |
| Token validation at the edge | p95 under 150 ms end to end including the upstream (REQ-GW-009 acceptance in the Appendix N peak scenario), under 2 ms for the validation itself |
| First API credential call | one Identity exchange, p95 under 30 ms |

---

## 13. Security

The threat table is `12-security-privacy-safety.md` §2.21 (T-GW-01 to T-GW-05).

| Data class | Handling |
|---|---|
| Tokens, API credentials, cookies | Passed through or exchanged; never logged, never cached beyond the 60-second exchange result, never forwarded to an upstream that did not need them (the internal token replaces the credential) |
| Bodies | Never read beyond the size check; never logged |
| Client addresses | Used for layer 1 and the allowlist; logged only as a prefix hash |

| Never | What |
|---|---|
| Logged | `Authorization`, cookies, the SignalR access-token query parameter, bodies, credential-bearing query strings, full client addresses |
| Trusted | `X-Nibras-Tenant-Id` from a browser client (T-GW-01); any `X-Forwarded-*` hop not from the edge proxy; client-supplied internal headers, which are stripped |
| Forwarded | An API credential to a service; the service sees only the internal 5-minute token |

Security headers on every response: `Strict-Transport-Security` with a one-year max age, a content security policy per front end generated from `08-web-structure.md`, `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`, `Permissions-Policy` denying geolocation, camera and microphone except the routes that declare them, `frame-ancestors 'none'` except the LTI launch route. CORS allows the tenant's verified hosts and the platform console origin only.

---

## 14. Folder and file tree

```text
src/Gateway/                                 the single public entry point; one host project and one test project, no Domain, Application or Infrastructure (07-solution-structure.md §2.1)
├── Nibras.Gateway/                          the YARP host, image nibras/gateway
│   ├── Program.cs                           composition root: ServiceDefaults, YARP, the pipeline of section 5.2 in order, probes; no business logic
│   ├── Nibras.Gateway.csproj                references Nibras.ServiceDefaults, Nibras.BuildingBlocks.Web, .Caching, .Observability and Nibras.Contracts.Shared only
│   ├── Routing/                             routes, clusters and route metadata
│   │   ├── RouteTable.cs                    path prefixes of section 5.1 to clusters, one cluster per service and backend-for-frontend
│   │   ├── ClusterConfiguration.cs          per-cluster timeouts, HTTP/2 pools, health probes and service discovery names
│   │   ├── RouteMetadata.cs                 public tag, administrative flag, deprecation and sunset, anonymous flag, rate class per operation
│   │   ├── RouteMetadataProvider.cs         builds RouteMetadata from the aggregated OpenAPI extensions x-nibras-tier, x-nibras-admin, x-nibras-deprecated-sunset
│   │   └── BlockedPrefixes.cs               prefixes never routed from outside: /api/v1/ai/ and the internal tenant-resolution route
│   ├── Pipeline/                            one middleware per step of section 5.2, registered in order
│   │   ├── CorrelationMiddleware.cs         step 1: X-Nibras-Correlation-Id and traceparent issued when absent
│   │   ├── MaintenanceMiddleware.cs         step 2: platform-wide then per-tenant maintenance, health routes skipped
│   │   ├── BodySizeLimitMiddleware.cs       step 3: 1 MiB JSON, 64 KiB anonymous; 413 with bodyTooLarge
│   │   ├── TenantResolutionMiddleware.cs    step 4: host, then header for mobile and credential callers only; provisioning check
│   │   ├── EdgeRateLimitMiddleware.cs       step 5: sliding window per address, token bucket per tenant, RateLimit-* headers
│   │   ├── CredentialLocationMiddleware.cs  step 6: refuses a credential in a query string and flags the key
│   │   ├── AuthenticationMiddleware.cs      step 7: API credential exchange or local JWT validation
│   │   ├── TenantMatchMiddleware.cs         step 8: token tenant equals resolved tenant; test and live key environments
│   │   ├── RevokedSubjectMiddleware.cs      step 9: the state:revoked mark written by Identity
│   │   ├── AccessPolicyMiddleware.cs        step 10: seeded-credential audience, public tag for credentials, Wellbeing exclusion
│   │   ├── AdminAllowlistMiddleware.cs      step 11: administrative routes against the tenant's IP allowlist
│   │   ├── ForwardingTransforms.cs          step 12: X-Nibras-Tenant-Id, internal token, trace headers; strips client-supplied internal headers
│   │   └── ResponseHeadersMiddleware.cs     step 14: security headers, deprecation headers, correlation echo, Brotli
│   ├── Authentication/                      token and credential handling
│   │   ├── JwtValidation.cs                 signature against the key set, lifetime, issuer, audience; no network call on the hot path
│   │   ├── KeySetCache.cs                   Identity's key set from the shared cache entry with the unknown-kid refresh limit
│   │   ├── ApiCredentialExchanger.cs        token-exchange grant at Identity with the svc-gateway client, 2 s timeout
│   │   ├── ExchangeCache.cs                 60 s in-process cache by key id and permission version, evicted by tag apikey:{keyId}
│   │   └── CredentialFormat.cs              nbk_ and nbp_ prefixes, live and test environments, key id parsing
│   ├── Tenancy/                             tenant resolution
│   │   ├── TenantResolver.cs                verified host through the Platform entry, then Platform's resolution route on a miss, negative cache 10 s
│   │   └── TenantResolutionResult.cs        tenant id, status, maintenance state, plan bucket size
│   ├── RateLimiting/                        layer 1 of 22-api-conventions-and-error-catalog.md §9.1
│   │   ├── AddressLimiter.cs                sliding window per client address or first trusted X-Forwarded-For hop
│   │   ├── TenantLimiter.cs                 token bucket per tenant sized by plan, burst of 10 seconds of refill
│   │   ├── RateLimitStore.cs                counters under state:ratelimit:{tenant}:edge in redis-state; fail open authenticated, closed anonymous
│   │   └── Exemptions.cs                    health, sign-out, emergency acknowledgment, maintenance page, per the deploy configuration list
│   ├── Security/                            headers, CORS and allowlist
│   │   ├── SecurityHeaders.cs               HSTS, CSP per front end, nosniff, referrer and permissions policies, frame-ancestors rules
│   │   ├── CorsPolicy.cs                    verified tenant hosts and the platform console origin only
│   │   ├── AdminAllowlist.cs                reads the Security settings entry and matches the client address prefix
│   │   └── SecurityTxtEndpoint.cs           serves /.well-known/security.txt
│   ├── Resilience/                          per-cluster protection
│   │   ├── ClusterCircuitBreakers.cs        breaker per cluster, 503 with Retry-After for that route only
│   │   └── CircuitStatePublisher.cs         breaker state as metrics every 10 s for the health board
│   ├── Versioning/                          the deprecation timeline at the edge
│   │   ├── DeprecationHeaders.cs            Deprecation, Sunset and Link on deprecated operations
│   │   └── RetiredVersionHandler.cs         404 GATEWAY_NOT_FOUND with versionRetired and the successor after the sunset
│   ├── OpenApi/                             aggregated documents
│   │   ├── OpenApiAggregator.cs             fetches every routed service's document at start-up and every 10 minutes and merges them
│   │   ├── PublicDocumentFilter.cs          keeps only operations tagged public for the developer portal
│   │   └── OpenApiEndpoints.cs              /openapi/v1.json, /openapi/public/v1.json and /docs with Scalar
│   ├── Invalidation/                        cache eviction pushed by other services
│   │   └── InvalidationSubscriber.cs        listens on state:invalidate and drops matching L1 entries within 2 s
│   ├── Errors/                              Problem Details at the edge
│   │   └── GatewayProblemDetails.cs         GATEWAY_* codes from the shared middleware, params.reason values of section 5.2
│   ├── Logging/                             what is never written
│   │   └── RedactionPolicy.cs               Authorization, cookies, hub access tokens, bodies and credential query strings never logged; addresses as prefix hashes
│   ├── appsettings.json                     non-secret defaults: timeouts, body limits, rate-limit defaults, cluster names
│   ├── appsettings.Development.json         Aspire and compose cluster addresses
│   └── Dockerfile                           Debian-based aspnet image, non-root, read-only root filesystem, ICU and tzdata present, TZ=UTC
└── tests/                                   the Gateway's own suite
    └── Nibras.Gateway.Tests/                routing, tenant resolution, limits and security tests against fake upstreams
        ├── Routing/                         every prefix to its cluster, blocked prefixes, retired versions
        ├── Tenancy/                         domain, subdomain, header rules, provisioning state, mismatch
        ├── Authentication/                  JWT validation, key rotation overlap, credential exchange and its cache eviction, revoked-subject mark
        ├── RateLimiting/                    per address and per tenant limits, 429 contract, Redis failure modes
        ├── Security/                        headers, CORS, allowlist, header stripping, security.txt, log redaction capture
        ├── Resilience/                      one upstream down while others answer, breaker timings
        ├── OpenApi/                         aggregation of every service and the public filter
        └── Nibras.Gateway.Tests.csproj      references the Gateway project and Nibras.BuildingBlocks.Testing
```

---

## 15. Test plan

| Test case | What it proves | Level |
|---|---|---|
| `TC-SEC-330` (document 12) | A browser-supplied tenant header is refused; the token's tenant must match (T-GW-01) | Integration |
| `TC-SEC-039` (document 12) | A token signed with a retired key is refused after the overlap (T-GW-02) | Integration |
| `TC-SEC-058` (document 12) | One tenant at its limit does not exhaust the platform (T-GW-03); Appendix N scenario N-06 at load | Integration, load |
| `TC-SEC-122` (document 12) | The maintenance flag changes only through Platform with the operator role and is audited (T-GW-05) | Integration |
| `TC-SEC-362` (document 12) | The seeded account's restricted token reaches only the Identity self-service routes in Production | Integration |
| TC-SEC-056 | Header swap and token swap of the tenant-isolation suite are refused at the Gateway | Generated suite |
| `TC-IDN-056` (Appendix R) | A leaver presenting a cached token is refused at the Gateway | Integration with Identity |
| TC-INT-660 | Keys and tokens are refused on every operation not tagged `public` | Generated suite |
| TC-INT-020 | Deprecated version headers, then `GATEWAY_NOT_FOUND` with the successor after the sunset | Integration with the fake clock |
| `TC-INF-110` (document 15) | One correlation id from the Gateway span to the last consumer | Nightly trace test |
| TC-GW-001 | Each of the routed prefixes reaches its cluster; `/api/v1/ai/` and `/api/v1/platform/tenant-resolution/` are not routable from outside | Integration |
| TC-GW-002 | Tenant resolved by custom domain, by subdomain and by header for mobile and credential callers; the header is ignored for web clients | Integration |
| TC-GW-003 | A tenant route with no resolvable tenant returns 400 `GATEWAY_VALIDATION_FAILED`; a platform route proceeds (REQ-GW-002) | Integration |
| TC-GW-004 | The 1,001st request over a tenant limit of 1,000 per minute returns 429 with `Retry-After`; another tenant is unaffected (REQ-GW-003) | Integration |
| TC-GW-005 | Security headers present on every response; a 30 MB body on a 20 MB limit returns 413 (REQ-GW-004) | Integration |
| TC-GW-006 | The aggregated document lists one group per routed service and the public document only `public` operations (REQ-GW-005) | Integration |
| TC-GW-007 | Maintenance for tenant A returns 503 to A while B and every health route answer (REQ-GW-006) | Integration |
| TC-GW-008 | With Behavior stopped, attendance routes work and behavior routes return 503 `GATEWAY_DEPENDENCY_UNAVAILABLE` (REQ-GW-007) | Integration, chaos |
| TC-GW-009 | Token validation p95 under 150 ms in the Appendix N peak scenario (REQ-GW-009) | Load |
| TC-GW-010 | A credential in a query string is refused and flagged; a `test` key on a live tenant is `GATEWAY_TENANT_MISMATCH` | Integration |
| TC-GW-011 | A revoked key stops working within 5 s through the invalidation channel, with no stale exchange beyond 60 s | Integration with Identity |
| TC-GW-012 | A key or personal token on any Wellbeing route is refused | Integration |
| TC-GW-013 | An administrative route from an address outside the allowlist is refused with `ipNotAllowed` | Integration |
| TC-GW-014 | A tenant route during Saga 1 returns `PLATFORM_PROVISIONING_IN_PROGRESS` until the routing entry is published | Integration with Platform |
| TC-GW-015 | `redis-state` down: layer 1 fails open for authenticated callers and closed for anonymous public routes | Chaos |
| TC-GW-016 | Client-supplied `X-Nibras-*` internal headers are stripped before forwarding | Integration |
| TC-GW-017 | No log line contains an `Authorization` header, a cookie, a body or a credential query string under the full test run | Integration, log capture |
| TC-GW-018 | `security.txt` is served and not expired | Integration |

---

## 16. Scaling, partitioning and risks

| Concern | Design | Trigger to revisit |
|---|---|---|
| Profile | Stateless request routing; CPU for TLS to upstreams and JWT validation, one Redis round trip per request | CPU above 60% at peak |
| Replicas | 3 as the starting point (master brief Section 34); the calendar-aware KEDA cron raises the minimum before each time-zone band's first period; HPA on requests per second | `CalendarScaleUpMissed` alert |
| Partitioning | None; rate-limit counters are keyed by tenant and window in `redis-state` | `redis-state` CPU above 50% from rate limiting |
| Connections | HTTP/2 multiplexed pools per cluster; WebSocket upgrades on `/hubs/` counted separately | Pool exhaustion on the hub cluster |

| Risk | Likelihood | Impact | Mitigation | Owner |
|---|---|---|---|---|
| A tenant spoofed by header or domain | med | critical | Header accepted only from mobile and credential callers, token tenant must match, verified hosts only (T-GW-01) | Security lead |
| The Gateway becomes a single point of failure | low | critical | 3 or more replicas across zones, no state, health-routed by the edge proxy | Platform operations |
| A cold cache after a Redis loss floods Platform and Identity | med | med | Negative caching, request coalescing per host and per key, degraded rate-limit profile (`21-performance-engineering.md` §1.22) | Platform operations |
| A noisy tenant degrades others | med | med | Per-tenant token bucket from the plan, N-06 scenario | Platform operations |
| Business logic creeps into the Gateway | med | med | Architecture rule `Hosts_NeverReference_ServiceProjects`; review rule that the pipeline of section 5.2 is the whole behaviour | Architect |

---

## Decisions in force

| Decision | Source | Default if unanswered | Impact if wrong |
|---|---|---|---|
| API credential exchange uses Identity's token endpoint over HTTP with the token-exchange grant, not gRPC | Reference architecture Section 8.0 gives the Gateway no gRPC dependency; `23-integrations-and-public-api.md` §2.3 | As stated | A gRPC call would make the Gateway a hop in the synchronous graph |
| The seeded administrator is held back by a token audience restricted to Identity's self-service routes, which the Gateway enforces | `12-security-privacy-safety.md` §8 item 3; REQ-IDN-002 keeps permission lists out of tokens | As stated | A claim listing permissions would contradict REQ-IDN-002 |
| A leaver's token is refused through Identity's revoked-subject mark in `redis-state` | Appendix R WF-IDN-06, TC-IDN-056 | As stated | Without it an access token lives up to 15 minutes after offboarding |
| Route metadata (public tag, administrative flag, deprecation) comes from the aggregated OpenAPI, not from hand-written Gateway configuration | `22-api-conventions-and-error-catalog.md` §11, `23-integrations-and-public-api.md` §1 | As stated | Hand-written rules drift from the services' own declarations |
| Ai is never routed from outside | `07-solution-structure.md` §2.4 | As stated | Exposing Ai would let a client bypass the backends-for-frontends' scope filtering |

## Dependencies on other documents

| This sheet assumes | Stated in | Checked on |
|---|---|---|
| Name, image, no database, no exchange | Appendix L | every lint run |
| Cross-cutting error suffixes and the Gateway prefix | Appendix K.1, `22-api-conventions-and-error-catalog.md` §12.3 | the generated contract suite |
| Headers, rate-limit layers and the 429 contract | `22-api-conventions-and-error-catalog.md` §1.5, §9 | Group C review |
| Credential exchange, public tag, deprecation timeline | `23-integrations-and-public-api.md` §1, §2.3, §5 | Group D review |
| Cache entries read and Redis ACL | `21-performance-engineering.md` §1.21, §2.2, §2.3, §2.6 | Group C review |
| Threats | `12-security-privacy-safety.md` §2.21 | Group D review |
| Edge proxy, alerts, calendar scaling | `15-deployment-and-operations.md` | Group E review |

## Open points

**Closed by ADR-0019 (brief v9.1).** Appendix K.23 now carries `GATEWAY_BODY_TOO_LARGE` (413), so step 3 of section 5.2 raises that code instead of a `GATEWAY_VALIDATION_FAILED` with `params.reason`, and REQ-GW-004's 413 has a code of its own. Appendix K.2 now carries `IDENTITY_TOKEN_INVALID` (401) for a malformed, unverifiable or foreign token, so step 7 tells a client to sign in again rather than to refresh a token that was never valid. The two points those codes answered are gone from the table below and the points that remain are renumbered.

| # | Question | Default | Owner | Impact if the default is wrong |
|---|---|---|---|---|
| 1 | `21-performance-engineering.md` §2.3 gives `svc_gateway` read access only to tenant resolution and its rate-limit prefix; this sheet also reads the key set, maintenance entries, the security settings entry and the revoked-subject mark | Extend the ACL with read-only access to those four keys | Architect, update to document 21 | Without it the Gateway cannot enforce maintenance, the allowlist or the leaver rule |
| 2 | The Gateway has no audit channel of its own for refused allowlist attempts (REQ-IDN-047 asks that the attempt is logged) | A structured security log line with the tenant and address prefix, exported to the log store and counted on the abuse dashboard; not an audit-chain entry | Security lead | An auditor looking only at the audit viewer does not see refused administrative attempts |
| 3 | "Administrative route" needs a definition the OpenAPI can carry | Operations declaring any permission in Appendix I groups G01, G02, G03 or G24 are marked `x-nibras-admin` by the generator | Architect | A route missing the flag is not protected by the allowlist |

## Review record

| Date | Reviewer | Outcome |
|---|---|---|
| 2026-09-21 | drafted | awaiting Group C review |

## How this document is verified

| Claim | Proof | Where it runs |
|---|---|---|
| No business logic, no data, no service reference | Architecture test `Hosts_NeverReference_ServiceProjects` and the absence of any `DbContext` | `tests/Architecture.Tests` |
| Every error code used exists in Appendix K or is a K.1 suffix | `/lint-plan` code check | Lint |
| Every tree entry has a purpose comment | `tools/kit-lint` rule R18 | Lint |
| The routes match the services | `TC-GW-001` and `TC-GW-006` against the aggregated document | Integration |
| The service can be built from this sheet | Group C review against the service-sheet skill | Review |
