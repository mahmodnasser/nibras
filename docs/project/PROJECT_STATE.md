# Nibras: Project State

**Phase:** **Phase 1, Foundation, in progress.** The plan was approved by the product owner on 2026-09-26 (scorecard round 7: all six groups approved at 4 or better on every axis). Slice SL-INF-001 is built and green; the next slices follow the critical path of document 34.
**Kit version:** v9, brief v9.7 (ADR-0019, ADR-0020, ADR-0021, ADR-0023, ADR-0025, ADR-0026, ADR-0027; ADR-0022 and ADR-0024 change only the plan). Accepted records: ADR-0019 and ADR-0027. On `github.com/mahmodnasser/nibras`, branch `main`.
**Last updated:** 2026-09-27, SL-API-001 built

## Phase 1 build log

**Standing instruction from the product owner (2026-09-27):** commit and push to `main` after each green slice, without asking each time. A slice is green only when its build has no warnings, its tests pass, `kit-lint` is clean and the kit tests pass.

| Slice | Requirements | State | Evidence |
|---|---|---|---|
| SL-INF-001 | REQ-INF-014, REQ-INF-017, REQ-INF-018 (REQ-DATA-001 is proven by TC-DATA-951 in SL-TST-003) | **Built, green** | Repository root (`global.json` SDK 10.0.401 with Microsoft.Testing.Platform, `Directory.Build.props` with warnings as errors, central package management, `nuget.config`, `Nibras.sln`); `Nibras.BuildingBlocks.Domain` (13 tests), `Nibras.BuildingBlocks.Observability` (20 tests, TC-INF-111), `Nibras.ServiceDefaults` (probes, graceful shutdown, resilience, service discovery), `Nibras.Contracts.Shared`; the service template `tools/templates/service/` (`new-service.mjs` with `.ps1` and `.sh`) whose test generates a service and passes its unit, integration (TC-INF-964) and contract (TC-TST-112) tests: `node --test tools/templates/service/template.test.mjs` 5 of 5. Demonstrated on Linux: a generated service started as a process answered `/health/live`, `/health/ready` and `/health/startup` with 200, echoed `X-Nibras-Correlation-Id`, exported traces and metrics to an OpenTelemetry Collector 0.161.0 as `nibras-probe`, and shut down gracefully on SIGTERM. Windows unverified in this environment |
| SL-DATA-001 | REQ-DATA-006, REQ-DATA-007, REQ-PERF-011, REQ-PERF-012, REQ-PERF-018 (partly: 409 in SL-API-004), REQ-DATA-003 (partly: row-level security in SL-DATA-002) | **Built, green** | `Nibras.BuildingBlocks.Tenancy` (`TenantId`, `ITenantContext`, `[PlatformScoped]`, telemetry tenant label; 4 tests) and `Nibras.BuildingBlocks.Persistence` (`NibrasDbContext` pooled base with named `Tenant` and `SoftDelete` filters, key `(tenant_id, id)`, audit and soft-delete columns, `xmin`, snake_case; `AuditColumnsInterceptor`, `SoftDeleteInterceptor`, `AddNibrasDbContext`; 9 tests on PostgreSQL 18.6 through Testcontainers: TC-DATA-640, TC-PERF-961, TC-PERF-962, TC-DATA-008, TC-PERF-968). `dotnet test --solution Nibras.sln` 46 of 46. `EFCore.NamingConventions` 10.0.1 added to document 19. The template generates its DbContext and row-level security policy from SL-DATA-002, when the migration can be proven with row-level security enabled |
| SL-DATA-002 | REQ-DATA-002, REQ-DATA-003 (now complete), REQ-DATA-004; REQ-DATA-017 and REQ-SEC-010 in part | **Built, green** | `RowLevelSecurity` (policy SQL per table and per model, `EnableTenantRowLevelSecurity` for migrations), `DatabaseBootstrap` (roles `svc_`/`mig_`, database, schema, privileges), `TenantTransactionInterceptor` and `InTenantTransactionAsync`; `NibrasDbContext` now requires the service schema. 5 tests through PgBouncer 1.25.2 (`edoburu/pgbouncer:v1.25.2-p0`, test only) in transaction mode with one server connection: TC-DATA-643, TC-DATA-953, TC-DATA-952, TC-SEC-960 (database part), TC-PLAT-015. Solution 51 of 51. Deferred: the template's generated DbContext, `policies.sql` and per-service row-level security test arrive with the migration bundle (SL-DATA-005), which is what proves a generated schema migrates with row-level security on; pool sizing (TC-DATA-967) with the load tier |
| SL-API-001 | REQ-API-002, REQ-API-003, REQ-API-004, REQ-API-009, REQ-SEC-017; REQ-API-001 and REQ-PERF-031 in part | **Built, green** | `Nibras.BuildingBlocks.Web`: `NibrasJson` (camelCase, camelCase string enums, UTC `Z` timestamps with milliseconds, ISO 8601 durations, unknown properties refused, HTML-sensitive characters escaped), `ErrorCatalog` (Appendix K.1 per prefix plus `_INTERNAL_ERROR`), `NibrasProblemDetails` and `ToProblem(Result)`, exception and body-less-status handlers, FluentValidation filter, `ETags` and `ETagFilter` (strong `"<xmin>-<hash>"`, 304), Brotli, output cache policy `nibras-public`, default `Cache-Control: no-store`, `MapNibrasApi(version)` and a startup check of the URL shape. 54 tests: TC-API-951, TC-API-952, TC-API-953, TC-API-954, TC-API-031, TC-SEC-967, TC-PERF-981. The template now wires the block and its generated service passes a TC-API-031 test. Solution 105 of 105; template 5 of 5. `FluentValidation` 12.1.1 pinned (already in document 19). Open points: Spectral plural check, OpenAPI and the status-only-assertion analyzer (SL-API-005); `_INTERNAL_ERROR` awaits Appendix K.1 (document 22 open point 1); the 1 MiB body limit and HTTP/2 and HTTP/3 are edge concerns |

**Build environment notes.** The .NET 10.0.401 SDK is taken from the `mcr.microsoft.com/dotnet/sdk:10.0` image because the SDK download host is not reachable from the cloud environment; NuGet restores normally. `xunit.v3` 4.x runs on Microsoft.Testing.Platform (`global.json` `test.runner`), so `Microsoft.NET.Test.Sdk` and `xunit.runner.visualstudio` are not used; `dotnet test --solution Nibras.sln` runs every suite. Six packages the slice needed were added to document 19 with the product owner's approval (2026-09-27).

**Deferred from SL-INF-001, each to the slice that owns it.** The template's AppHost registration (SL-PLAT-003), Compose entry (SL-PLAT-004), Helm chart and dashboard and alert files (their deployment slices), the `--worker`, `--grpc` and `--sagas` flags (the first slice that needs each), and the DbContext with its row-level security test (SL-DATA-001, SL-DATA-002). Document 34 names the block `Nibras.BuildingBlocks.Hosting` for SL-INF-001; document 07 has no such block and puts the host defaults in `Nibras.ServiceDefaults`, which is what was built (document 07 owns the structure).

## Done: complete and lint-clean

| Group | Documents |
|---|---|
| A | 01 questions and assumptions; 02 competitive gap analysis (40 web checks, 31 sources) |
| B | 03 requirements catalog (**860 requirements**, validated by script); 04 architecture overview; 05 service catalog; 07 solution structure |
| C | 06: **all 23 service sheets**, every permission, event, error code and identifier checked against its catalog; 10 data; 11 messaging; 21 performance (130 hot queries) |
| D | 08 web; 09 mobile; 12 security (119 threat rows); 13 workflows and sagas; 14 design system |
| E | 15 deployment and operations; 16 test strategy; **17 roadmap (79 capabilities, MVP cut line)**; 18 risk register; 19 dependency inventory (147 dependencies, 142 verified at source) |
| F | 22 API conventions; 23 integrations; 24 localization; 25 assist ladder; 26 migration toolkit; 27 compliance; 28 capacity and cost; 29 ADR index; 31 rules and workflows assigned to code (generated); 32 differentiation and demo; 33 platform support |

## Done: document 34, the work breakdown (79 capabilities, 726 slices, 1,724 slice-days; 858 requirements built by slices, 2 Tier 3 explained)

| Step | Phases | Slices | State |
|---|---|---|---|
| Part A | 1 Foundation | 001 to 199 | **Done** |
| Part B | 2 The school year loop | 200 to 399 | **Done**. Parts A and B together: 38 capabilities, 353 slices, 825 slice-days, 618 requirements built |
| Part C, phase 3 | 3 Money and paperwork | 400 to 599 | **Done**: 14 capabilities, 135 slices, 321 slice-days, 106 requirements (REQ-INT-016's LTI part handed to phase 4). Validate with `--part C --phase 3` |
| Part C, phase 4 | 4 Growth | 400 to 599, continuing | **Done**: 8 capabilities, 84 slices, 210 slice-days; LTI 1.3 (SL-INT-411) now under CAP-INT-02, added to document 17 in fix 4 |
| Part D, phase 5 | 5 Extended | 600 to 799 | **Done**: 11 capabilities, 105 slices, 252 slice-days; QTI 3, Open Badges 3.0 and CASE now under CAP-INT-03 (fix 4); OPS-014 and OPS-015 (Tier 3) noted, not built |
| Part D, phase 6 | 6 Hardening and launch | 600 to 799, continuing | **Done**: 6 capabilities, 41 slices, 97 slice-days; builds the INF-027..033 and INF-036 handovers |

**How to resume.** Writer prompt: `tools/plan-build/wb-prompt.md` with `{{W}}`, `{{SCOPE}}` and `{{RANGE}}` filled. The writer appends to `tools/plan-build/parts/wb-part-<W>.md`, one capability at a time, so a partial part survives an interruption. Validate with `node tools/plan-build/assemble-34.mjs --part <W>`. Rewrite document 34 with `--write --partial` after each part, and `--write` once all four exist. See `tools/plan-build/README.md`.

## Then: remediation to approval (document 30 Section 5)

| # | Theme | Effort |
|---|---|---|
| 1 | Product-owner decisions: absence-alert timing; public API, OneRoster, iCal in Tier 1; countries, Apple build capacity, certifications | 1 hour of the owner's time |
| 2 | **Done**: document 17's ranges derived from document 34 by `schedule-34.mjs`: phase 1 14 to 22 weeks, launch 61 to 93, MVP 42 capabilities in 33 to 50; RISK-03 recorded as occurred | done |
| 3 | **Done**: ADR-0019 applied 317 brief corrections, all three briefs are v9.1; four value decisions settled (cooling-off 30 days, invitations 14 days, dedupe 5 minutes, feature 39 to engineering); R03 checks minor versions | done |
| 4 | **Done**: contradictions removed across 01-33 and all 23 sheets (113 open points closed or narrowed against ADR-0019); Gateway revoked-mark read added to 21; CAP-INT-02 and CAP-INT-03 added to 17 with their slices moved in 34; 79 capabilities; k6 in allow.json | done |
| 5 | **Done**: ADR-0020 (proposed) and brief v9.2; kit-lint R20 (one definition per test, no undefined citation, derived tests need their requirement) with 5 self-tests; 206 double definitions and 163 undefined citations resolved; registry annex generated (1,566 tests, 334 derived) | done |
| 6 | **Done**: ADR-0021 (proposed) and brief v9.3; 12 new kit-lint rules (R21 to R32) with self-tests, R18 strict for plan trees; generators gained `--check`; about 100 verification rows now name a real rule, a named review step or the building slice | done |
| 7 | **Done**: ADR-0022 (proposed); kit-lint R33; every open point scored on document 18's scales, 12 or more linked to a RISK; 12 Open points sections added; document 12 threats owned and linked; RISK-44 to RISK-53 added, RISK-40 closed; Open Questions 29 and 30 | done |
| 8 | **Done**: ADR-0023 (proposed) and brief v9.4; kit-lint R34; every one of the 43 signature features runs its own demo test in the release gate (SL-TST-006); documents 15, 16 and 32 aligned | done |

Then: rounds 4 and 5. Round 4 approved Group E and left C and F blocked on named gaps; remediation round 5 closed them (document 11 bindings, the Scheduling, Operations, Behavior and Wellbeing sheets, SL-ACA-405 for the LTI launch in phase 4, conditional e-invoicing slices, Open Question 31, ADR-0025 and brief v9.5 with launch at 61 to 95 weeks). Round 5 approved C and F. Document 00 written.

Then: remediation round 6 closed the round-5 non-blocking gaps across all six groups (LTI grade return built, the capacity recomputation script, document 02 re-checked, ADR-0026 and brief v9.6); round 6 re-scored every group.

## Next: exactly where to resume

1. **Design review against the reference dashboards** the product owner named (Edudash by wowtheme7, EduMin by dexignlab, Akademi). The hosts are blocked by the environment's network policy; once they are allowed (or screenshots are supplied), review them, extend the design preview (https://claude.ai/artifact/J82rJXAFiDfiWLFcUhbGaQ) to every role dashboard in both directions and all widths, and bring any new features into the plan as a change (ADR where the brief changes).
2. **Start phase 1 with `/build-foundation`** once the product owner confirms after the design review.
3. Alongside phase 1, hold the decisions workshop of document 00 Section 6 for the questions still open: 29 first, then 27, 28, 3 with 9, 14, 24, 26 and 31, then ADR-0024 to ADR-0026 and the Proposed records phase 1 builds on. Record each answer in `OPEN_QUESTIONS.md` and, where it changes the brief, in an ADR with its version bump.
4. The non-blocking gaps round 7 still lists are in document 30 Section 5; none changes a phase range or the MVP.
