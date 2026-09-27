# Traceability

Generated during planning as `docs/plan/20-traceability-matrix.md`, then maintained here through the build.

Area codes come from Appendix L. Identifier formats are `REQ-<AREA>-<NNN>`, `WF-<AREA>-<NN>`, `BR-<AREA>-<NNN>`, `TC-<AREA>-<NNN>`.

**A row with an empty Test case column is an unfinished requirement**, whatever the code says. `/lint-plan` refuses a matrix with a gap, and master brief Section 24 makes the same point as a quality gate.

The Platform column names a runner or device class from Appendix X, and is `any` unless the requirement exists because of an operating system or device difference.

| Requirement | Summary | Tier | Service | Workflow | Rule | Plan document | Phase | Test case | Platform | Status |
|---|---|---|---|---|---|---|---|---|---|---|
| REQ-INF-014 | Liveness, readiness and startup probes, timeouts, retries with backoff and jitter, circuit breakers, graceful shutdown | 1 | cross-cutting (ServiceDefaults) | none | none | 07, 15, 34 SL-INF-001 | 1 | TC-INF-964 | any | Partly built: the three probes, readiness failing on an unhealthy dependency while liveness passes, readiness failing while the host stops, the 30-second shutdown timeout, and the standard HttpClient resilience handler (timeout, retry with backoff and jitter, circuit breaker); TC-INF-964 passes in the generated service. Bulkheads and queue backpressure join with the messaging slices (SL-MSG) and SL-INF-004 |
| REQ-INF-017 | Every service emits OpenTelemetry; service map, SLO dashboards, logs and alerts | 1 | cross-cutting (Observability block) | none | none | 07, 15, 34 SL-INF-001 | 1 | TC-INF-104 | any | Partly built: traces, metrics and logs over OTLP, verified against an OpenTelemetry Collector 0.161.0 on Linux. TC-INF-104 (dashboards and runbook panels) comes with SL-INF-007 |
| REQ-INF-018 | Metrics use the `nibras_` prefix and carry the tenant label | 1 | cross-cutting (Observability block) | none | none | 07, 15, 34 SL-INF-001 | 1 | TC-INF-111 | any | Built: instrument names are refused unless `nibras_` lower snake case, and the tenant label `nibras.tenant_id` is on the request-duration metric, the span and the log scope when a tenant is resolved; TC-INF-111 passes. The tenant source is supplied by the Tenancy block (SL-DATA-001) |
