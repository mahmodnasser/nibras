# 19. Dependency and Licence Inventory

> Plan document for the Nibras platform. Group E. It refines master brief Section 6 (the open-source-only policy, the approved stack, the unavoidable costs and the standalone list) and reference architecture Section 16 (pinned majors); it does not re-derive them. Where this document and the brief disagree, an ADR records the deviation.

**Group** E · **Requirement areas covered** SEC (supply chain and REQ-SEC-012), OPS (images and tools), every area that links a library · **Last updated** 2026-09-26 by the round-3 scorecard remediation (k6 statements made to agree with `allow.json`)

## Purpose

This document is the single place where an engineer, the licence auditor and a reviewer find, for every dependency the brief names, the current stable version, the licence **of that exact version**, where that was read, whether the product links it or runs it standalone, and the verdict against master brief Section 6.1. It also records what changed licence and when, the banned list with the version where each item crossed the line, the unavoidable costs, the scanner allow-list and its four-field rule, the pinning policy and the SBOM and signing policy. The readers are the engineer adding or upgrading a package, the licence auditor at every release, and the reviewer of `Directory.Packages.props`, `package.json` and `pubspec.yaml`.

## Scope

| In scope | Out of scope | Where the out-of-scope item lives |
|---|---|---|
| Every library, image, font, icon set and tool named in master brief Section 6.2 and Section 6.4 | Transitive dependencies of those packages | The CycloneDX SBOM per build (§13) and the licence scan (§11) |
| Current stable version and the licence of that version, with source and date | Which service uses which package | `07-solution-structure.md`, `06-services/<service>.md` |
| Linked or standalone, and the verdict | How a standalone tool is deployed and sized | `15-deployment-and-operations.md` |
| Licence changes and the banned list | Architectural consequences of a major-version change | Reference architecture Section 16; `29-adr-index.md` |
| The unavoidable-cost list | Budget figures for those costs | Master brief Section 30 |
| Scanner allow-list, pinning, update cadence, SBOM and signing | Vulnerability triage and the threat model | `12-security-privacy-safety.md` |
| The generator and API tooling named by documents 08 and 22 | The SAML library for the Tier 2 slice | Chosen when that slice starts (`06-services/identity.md` open point 9) |

---

## 1. How the inventory was built

Master brief Section 6.1 says "verify, do not assume", and rule 5 of `PLAN_SPEC.md` says the licence is read for the exact version from the source. Every row below was read on **2026-09-22** from a machine-readable source for that version, not from memory.

| Ecosystem | Version read from | Licence read from | What the row cites |
|---|---|---|---|
| NuGet | `azuresearch-usnc.nuget.org/query?q=packageid:<id>` (latest stable, prerelease excluded) | The `<license>` element of that version's `.nuspec` at `api.nuget.org/v3-flatcontainer/<id>/<version>/<id>.nuspec`; where the nuspec carries only a licence file, the repository LICENSE file | `https://www.nuget.org/packages/<id>/<version>` |
| npm | `registry.npmjs.org/<package>/latest` | The `license` field of that version's manifest | `https://www.npmjs.com/package/<package>/v/<version>` |
| pub | `pub.dev/api/packages/<package>` | The `license:` tag pub.dev derives from the package's LICENSE file (`pub.dev/api/packages/<package>/score`) | `https://pub.dev/packages/<package>/versions/<version>` |
| Container image, server, tool | The repository's latest release tag (`github.com/<repo>/releases/latest`), or the project's version feed | The repository LICENSE or COPYRIGHT file, read as text where the API reported `NOASSERTION` | The LICENSE file URL |
| Font, icon set | The upstream release tag and the npm packaging | The upstream OFL or licence file and the npm manifest | Both |

**Verdict vocabulary.** `allowed`: satisfies Section 6.1 for linked code. `allowed-standalone`: GPL, AGPL or LGPL software that is run unmodified and never linked, and that has a Section 6.4 row and an `allow.json` entry. `allowed-dynamic-only`: LGPL, allowed only when dynamically referenced. `not-allowed`: fails Section 6.1, or is a GPL or AGPL tool that has no Section 6.4 row yet. `unverified`: the version or licence could not be read from the source; the reason is given.

**Linked or standalone.** *Linked* means the code is compiled into, or loaded by, a product process (a NuGet, npm or pub package in a product build, a font or icon shipped in a bundle). *Tool* means it runs at build or test time and is never shipped. *Standalone* means it runs as a separate process or container reached over a network or socket protocol.

**Freshness rule.** A row is a snapshot. The version actually pinned is the one in the lock file on the day it is added; the auditor re-reads the licence of that pinned version (§12), and this table is refreshed at every release.

---

## 2. .NET packages linked into the product (NuGet)

| Package | Concern (Section 6.2) | Version | Licence of that version | Source | Checked | Linked | Verdict |
|---|---|---|---|---|---|---|---|
| Microsoft.EntityFrameworkCore | ORM | 10.0.12 | MIT | https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/10.0.12 | 2026-09-22 | Linked | allowed |
| Npgsql | ORM, binary COPY | 10.0.3 | PostgreSQL | https://www.nuget.org/packages/Npgsql/10.0.3 | 2026-09-22 | Linked | allowed |
| Npgsql.EntityFrameworkCore.PostgreSQL | ORM provider | 10.0.3 | PostgreSQL | https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL/10.0.3 | 2026-09-22 | Linked | allowed |
| Dapper | Reporting read models | 2.1.86 | Apache-2.0 | https://www.nuget.org/packages/Dapper/2.1.86 | 2026-09-22 | Linked | allowed |
| WolverineFx | Mediator, bus, outbox, sagas | 6.39.1 | MIT | https://www.nuget.org/packages/WolverineFx/6.39.1 | 2026-09-22 | Linked | allowed |
| Rebus | Fallback bus | 8.9.4 | MIT | https://www.nuget.org/packages/Rebus/8.9.4 | 2026-09-22 | Linked (fallback only) | allowed |
| RabbitMQ.Client | Fallback thin bus, transport | 7.2.2 | Apache-2.0 OR MPL-2.0 | https://www.nuget.org/packages/RabbitMQ.Client/7.2.2 | 2026-09-22 | Linked | allowed (Apache-2.0 elected) |
| Riok.Mapperly | Object mapping | 4.3.1 | Apache-2.0 | https://www.nuget.org/packages/Riok.Mapperly/4.3.1 | 2026-09-22 | Linked (source generator) | allowed |
| FluentValidation | Validation | 12.1.1 | Apache-2.0 | https://www.nuget.org/packages/FluentValidation/12.1.1 | 2026-09-22 | Linked | allowed |
| OpenIddict.AspNetCore | Authentication | 7.7.1 | Apache-2.0 | https://www.nuget.org/packages/OpenIddict.AspNetCore/7.7.1 | 2026-09-22 | Linked | allowed |
| OpenIddict.EntityFrameworkCore | Authentication store | 7.7.1 | Apache-2.0 | https://www.nuget.org/packages/OpenIddict.EntityFrameworkCore/7.7.1 | 2026-09-22 | Linked | allowed |
| StackExchange.Redis | Cache client | 3.3.0 | MIT | https://www.nuget.org/packages/StackExchange.Redis/3.3.0 | 2026-09-22 | Linked | allowed |
| Microsoft.Extensions.Caching.Hybrid | `HybridCache` | 10.10.0 | MIT | https://www.nuget.org/packages/Microsoft.Extensions.Caching.Hybrid/10.10.0 | 2026-09-22 | Linked | allowed |
| Quartz | Scheduled jobs | 4.1.1 | Apache-2.0 | https://www.nuget.org/packages/Quartz/4.1.1 | 2026-09-22 | Linked | allowed |
| Quartz.Serialization.SystemTextJson | PostgreSQL job store serialisation | 4.1.1 | Apache-2.0 | https://www.nuget.org/packages/Quartz.Serialization.SystemTextJson/4.1.1 | 2026-09-22 | Linked | allowed |
| PDFsharp | Simple PDF | 6.2.4 | MIT | https://www.nuget.org/packages/PDFsharp/6.2.4 | 2026-09-22 | Linked | allowed |
| PDFsharp-MigraDoc | Simple PDF layout | 6.2.4 | MIT | https://www.nuget.org/packages/PDFsharp-MigraDoc/6.2.4 | 2026-09-22 | Linked | allowed |
| ClosedXML | Excel import and export | 0.105.1 | MIT | https://www.nuget.org/packages/ClosedXML/0.105.1 | 2026-09-22 | Linked | allowed |
| CsvHelper | CSV | 33.1.0 | MS-PL OR Apache-2.0 | https://www.nuget.org/packages/CsvHelper/33.1.0 | 2026-09-22 | Linked | allowed (Apache-2.0 elected) |
| SkiaSharp | Image processing | 4.152.1 | MIT | https://www.nuget.org/packages/SkiaSharp/4.152.1 | 2026-09-22 | Linked | allowed |
| Magick.NET-Q8-AnyCPU | Image processing (alternative) | 14.17.1 | Apache-2.0 | https://www.nuget.org/packages/Magick.NET-Q8-AnyCPU/14.17.1 | 2026-09-22 | Linked | allowed |
| QRCoder | QR codes | 1.8.0 | MIT | https://www.nuget.org/packages/QRCoder/1.8.0 | 2026-09-22 | Linked | allowed |
| ZXing.Net | Barcodes | 0.16.11 | Apache-2.0 | https://www.nuget.org/packages/ZXing.Net/0.16.11 | 2026-09-22 | Linked | allowed |
| Finbuckle.MultiTenant.AspNetCore | Multi-tenancy | 10.1.4 | Apache-2.0 | https://www.nuget.org/packages/Finbuckle.MultiTenant.AspNetCore/10.1.4 | 2026-09-22 | Linked | allowed |
| Microsoft.Extensions.Resilience | Resilience | 10.10.0 | MIT | https://www.nuget.org/packages/Microsoft.Extensions.Resilience/10.10.0 | 2026-09-22 | Linked | allowed |
| Microsoft.FeatureManagement.AspNetCore | Feature flags | 4.7.0 | MIT | https://www.nuget.org/packages/Microsoft.FeatureManagement.AspNetCore/4.7.0 | 2026-09-22 | Linked | allowed |
| Microsoft.AspNetCore.OpenApi | API documentation | 10.0.12 | MIT | https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi/10.0.12 | 2026-09-22 | Linked | allowed |
| Scalar.AspNetCore | API reference UI | 2.17.7 | MIT | https://www.nuget.org/packages/Scalar.AspNetCore/2.17.7 | 2026-09-22 | Linked | allowed |
| Yarp.ReverseProxy | API gateway | 2.3.0 | MIT | https://www.nuget.org/packages/Yarp.ReverseProxy/2.3.0 | 2026-09-22 | Linked | allowed |
| Grpc.AspNetCore | Internal synchronous calls | 2.84.0 | Apache-2.0 | https://www.nuget.org/packages/Grpc.AspNetCore/2.84.0 | 2026-09-22 | Linked | allowed |
| Microsoft.Extensions.ServiceDiscovery | Service discovery | 10.10.0 | MIT | https://www.nuget.org/packages/Microsoft.Extensions.ServiceDiscovery/10.10.0 | 2026-09-22 | Linked | allowed |
| Aspire.Hosting.AppHost | Local orchestration | 13.5.4 | MIT | https://www.nuget.org/packages/Aspire.Hosting.AppHost/13.5.4 | 2026-09-22 | Tool (development host) | allowed |
| MailKit | Email | 4.18.0 | MIT | https://www.nuget.org/packages/MailKit/4.18.0 | 2026-09-22 | Linked | allowed |
| Google.OrTools | Timetable solver | 9.15.6755 | Apache-2.0 | https://www.nuget.org/packages/Google.OrTools/9.15.6755 | 2026-09-22 | Linked (native runtime included) | allowed |
| Microsoft.ML | Predictive models | 5.0.0 | MIT | https://www.nuget.org/packages/Microsoft.ML/5.0.0 | 2026-09-22 | Linked | allowed |
| Microsoft.Extensions.AI | Generative AI abstraction | 10.10.0 | MIT | https://www.nuget.org/packages/Microsoft.Extensions.AI/10.10.0 | 2026-09-22 | Linked | allowed |
| Microsoft.SemanticKernel | Orchestration, only if needed | 1.80.1 | MIT | https://www.nuget.org/packages/Microsoft.SemanticKernel/1.80.1 | 2026-09-22 | Linked (conditional) | allowed |
| Serilog | Logging | 4.4.0 | Apache-2.0 | https://www.nuget.org/packages/Serilog/4.4.0 | 2026-09-22 | Linked | allowed |
| Serilog.AspNetCore | Logging host integration | 10.0.0 | Apache-2.0 | https://www.nuget.org/packages/Serilog.AspNetCore/10.0.0 | 2026-09-22 | Linked | allowed |
| OpenTelemetry | Observability SDK | 1.19.1 | Apache-2.0 | https://www.nuget.org/packages/OpenTelemetry/1.19.1 | 2026-09-22 | Linked | allowed |
| OpenTelemetry.Extensions.Hosting | Observability host integration | 1.19.1 | Apache-2.0 | https://www.nuget.org/packages/OpenTelemetry.Extensions.Hosting/1.19.1 | 2026-09-22 | Linked | allowed |

ASP.NET Core Identity and SignalR ship in the .NET 10 shared framework (MIT, `https://github.com/dotnet/dotnet`) and are versioned with it; they carry no separate package row. Magick.NET bundles native ImageMagick under the ImageMagick licence and SkiaSharp bundles native Skia under BSD-3-Clause; the SBOM records both native components, and their licences were not separately re-read on 2026-09-22 (status `unverified` for the native parts only).

## 3. .NET test, build and supply-chain tools (NuGet)

| Package | Concern | Version | Licence of that version | Source | Checked | Linked | Verdict |
|---|---|---|---|---|---|---|---|
| xunit.v3 | Unit tests | 4.0.1 | Apache-2.0 | https://www.nuget.org/packages/xunit.v3/4.0.1 | 2026-09-22 | Tool (test projects) | allowed |
| NSubstitute | Test doubles | 6.2.0 | BSD-3-Clause | https://www.nuget.org/packages/NSubstitute/6.2.0 | 2026-09-22 | Tool | allowed |
| Shouldly | Assertions | 4.3.0 | BSD-3-Clause | https://www.nuget.org/packages/Shouldly/4.3.0 | 2026-09-22 | Tool | allowed |
| AwesomeAssertions | Assertions (alternative) | 9.6.0 | Apache-2.0 | https://www.nuget.org/packages/AwesomeAssertions/9.6.0 | 2026-09-22 | Tool | allowed |
| FsCheck | Property-based tests for the arithmetic rules (Appendix V; `16-test-strategy.md` §6.2 makes it the default). Not in Section 6.2's Testing list, so the choice needs an ADR: open point 7 | 3.4.0 | BSD-3-Clause (`<license type="expression">` in the nuspec) | https://www.nuget.org/packages/FsCheck/3.4.0 | 2026-09-22 | Tool (test projects) | allowed |
| FsCheck.Xunit | xUnit integration for the above; pins `FsCheck` to the same version | 3.4.0 | BSD-3-Clause (`<license type="expression">` in the nuspec) | https://www.nuget.org/packages/FsCheck.Xunit/3.4.0 | 2026-09-22 | Tool (test projects) | allowed |
| Bogus | Test data | 35.6.5 | MIT (nuspec carries a licence file; text read at https://github.com/bchavez/Bogus/blob/master/LICENSE) | https://www.nuget.org/packages/Bogus/35.6.5 | 2026-09-22 | Tool | allowed |
| Testcontainers | Integration tests | 4.15.0 | MIT | https://www.nuget.org/packages/Testcontainers/4.15.0 | 2026-09-22 | Tool | allowed |
| Testcontainers.PostgreSql | Integration tests (PostgreSQL module; RabbitMQ and Redis modules share the version) | 4.15.0 | MIT | https://www.nuget.org/packages/Testcontainers.PostgreSql/4.15.0 | 2026-09-22 | Tool | allowed |
| BenchmarkDotNet | Benchmarks | 0.15.8 | MIT | https://www.nuget.org/packages/BenchmarkDotNet/0.15.8 | 2026-09-22 | Tool | allowed |
| NetArchTest.Rules | Architecture rules | 1.3.2 (published 2021-05-23) | MIT (nuspec has no licence element; text read at https://github.com/BenMorris/NetArchTest/blob/master/LICENSE) | https://www.nuget.org/packages/NetArchTest.Rules/1.3.2 | 2026-09-22 | Tool | allowed |
| TngTech.ArchUnitNET | Architecture rules (not in Section 6.2; recorded as the replacement if NetArchTest is retired, open point 3) | 0.13.4 | Apache-2.0 | https://www.nuget.org/packages/TngTech.ArchUnitNET/0.13.4 | 2026-09-22 | Tool | allowed |
| PactNet | Contract testing | 5.0.1 | MIT | https://www.nuget.org/packages/PactNet/5.0.1 | 2026-09-22 | Tool | allowed |
| dotnet-stryker | Mutation testing | 5.0.0 | Apache-2.0 (nuspec carries a licence file; text read at https://github.com/stryker-mutator/stryker-net/blob/master/LICENSE) | https://www.nuget.org/packages/dotnet-stryker/5.0.0 | 2026-09-22 | Tool | allowed |
| CycloneDX (dotnet tool) | SBOM | 6.2.0 | Apache-2.0 | https://www.nuget.org/packages/CycloneDX/6.2.0 | 2026-09-22 | Tool | allowed |

## 4. Web packages (npm)

| Package | Concern (Section 6.2, Angular paragraph) | Version | Licence of that version | Source | Checked | Linked | Verdict |
|---|---|---|---|---|---|---|---|
| @angular/core | Framework | 22.1.7 | MIT | https://www.npmjs.com/package/@angular/core/v/22.1.7 | 2026-09-22 | Linked | allowed |
| @angular/material | Component base | 22.1.7 | MIT | https://www.npmjs.com/package/@angular/material/v/22.1.7 | 2026-09-22 | Linked | allowed |
| @angular/cdk | Component base, timetable drag and drop | 22.1.7 | MIT | https://www.npmjs.com/package/@angular/cdk/v/22.1.7 | 2026-09-22 | Linked | allowed |
| @angular/animations | Excluded by Section 6.2 (deprecated) | 22.1.7 | MIT | https://www.npmjs.com/package/@angular/animations/v/22.1.7 | 2026-09-22 | Not installed | allowed by licence; excluded by Section 6.2 |
| tailwindcss | Layout | 4.3.3 | MIT | https://www.npmjs.com/package/tailwindcss/v/4.3.3 | 2026-09-22 | Linked (build-time CSS) | allowed |
| @ngrx/signals | SignalStore | 22.0.1 | MIT | https://www.npmjs.com/package/@ngrx/signals/v/22.0.1 | 2026-09-22 | Linked | allowed |
| @jsverse/transloco | i18n | 8.4.0 | MIT | https://www.npmjs.com/package/@jsverse/transloco/v/8.4.0 | 2026-09-22 | Linked | allowed |
| echarts | Charts | 6.1.0 | Apache-2.0 | https://www.npmjs.com/package/echarts/v/6.1.0 | 2026-09-22 | Linked | allowed |
| ngx-echarts | Charts wrapper | 22.0.0 | MIT | https://www.npmjs.com/package/ngx-echarts/v/22.0.0 | 2026-09-22 | Linked | allowed |
| @tiptap/core | Rich text | 3.31.3 | MIT | https://www.npmjs.com/package/@tiptap/core/v/3.31.3 | 2026-09-22 | Linked | allowed (open-source core and extensions only; Tiptap Pro extensions and its hosted collaboration are not used) |
| quill | Rich text (alternative) | 2.0.3 | BSD-3-Clause | https://www.npmjs.com/package/quill/v/2.0.3 | 2026-09-22 | Linked | allowed |
| leaflet | Maps (web) | 1.9.4 | BSD-2-Clause | https://www.npmjs.com/package/leaflet/v/1.9.4 | 2026-09-22 | Linked | allowed |
| lucide-angular | Icons | 1.0.0 | ISC | https://www.npmjs.com/package/lucide-angular/v/1.0.0 | 2026-09-22 | Linked | allowed |
| @openapitools/openapi-generator-cli | Typed API client generator (`08-web-structure.md`) | 2.41.0 | Apache-2.0 | https://www.npmjs.com/package/@openapitools/openapi-generator-cli/v/2.41.0 | 2026-09-22 | Tool (the generated code is ours) | allowed |
| @scalar/api-reference | Developer portal reference UI | 1.70.0 | MIT | https://www.npmjs.com/package/@scalar/api-reference/v/1.70.0 | 2026-09-22 | Linked (portal) | allowed |
| @stoplight/spectral-cli | OpenAPI lint (`22-api-conventions-and-error-catalog.md`) | 6.16.3 | Apache-2.0 | https://www.npmjs.com/package/@stoplight/spectral-cli/v/6.16.3 | 2026-09-22 | Tool | allowed |
| @playwright/test | End-to-end and visual tests | 1.63.0 | Apache-2.0 | https://www.npmjs.com/package/@playwright/test/v/1.63.0 | 2026-09-22 | Tool | allowed |
| axe-core | Accessibility tests | 4.13.0 | MPL-2.0 | https://www.npmjs.com/package/axe-core/v/4.13.0 | 2026-09-22 | Tool | allowed |
| @axe-core/playwright | Accessibility tests in Playwright | 4.13.0 | MPL-2.0 | https://www.npmjs.com/package/@axe-core/playwright/v/4.13.0 | 2026-09-22 | Tool | allowed |
| @cyclonedx/cyclonedx-npm | SBOM | 6.0.1 | Apache-2.0 | https://www.npmjs.com/package/@cyclonedx/cyclonedx-npm/v/6.0.1 | 2026-09-22 | Tool | allowed |

## 5. Mobile packages (pub)

| Package | Concern (Section 6.2, Flutter paragraph) | Version | Licence of that version | Source | Checked | Linked | Verdict |
|---|---|---|---|---|---|---|---|
| flutter_riverpod | State | 3.4.3 | MIT | https://pub.dev/packages/flutter_riverpod/versions/3.4.3 | 2026-09-22 | Linked | allowed |
| flutter_bloc | State (alternative) | 9.1.1 | MIT | https://pub.dev/packages/flutter_bloc/versions/9.1.1 | 2026-09-22 | Linked | allowed |
| go_router | Routing | 18.0.1 | BSD-3-Clause | https://pub.dev/packages/go_router/versions/18.0.1 | 2026-09-22 | Linked | allowed |
| dio | HTTP | 5.11.1 | MIT | https://pub.dev/packages/dio/versions/5.11.1 | 2026-09-22 | Linked | allowed |
| drift | Offline storage (SQLite) | 2.35.0 | MIT | https://pub.dev/packages/drift/versions/2.35.0 | 2026-09-22 | Linked | allowed |
| flutter_secure_storage | Token storage | 11.2.0 | BSD-3-Clause | https://pub.dev/packages/flutter_secure_storage/versions/11.2.0 | 2026-09-22 | Linked | allowed |
| local_auth | Biometrics | 3.0.2 | BSD-3-Clause | https://pub.dev/packages/local_auth/versions/3.0.2 | 2026-09-22 | Linked | allowed |
| fl_chart | Charts | 1.2.0 | MIT | https://pub.dev/packages/fl_chart/versions/1.2.0 | 2026-09-22 | Linked | allowed |
| mobile_scanner | QR scanning | 7.4.2 | BSD-3-Clause | https://pub.dev/packages/mobile_scanner/versions/7.4.2 | 2026-09-22 | Linked | allowed |
| flutter_map | Maps (mobile) | 8.3.2 | BSD-3-Clause | https://pub.dev/packages/flutter_map/versions/8.3.2 | 2026-09-22 | Linked | allowed (the tile server's usage policy is a separate obligation, not a licence) |
| flutter_animate | Motion | 4.5.2 (published 2024-11-25) | BSD-3-Clause | https://pub.dev/packages/flutter_animate/versions/4.5.2 | 2026-09-22 | Linked | allowed |
| lottie | Illustrative animation | 3.6.1 | MIT | https://pub.dev/packages/lottie/versions/3.6.1 | 2026-09-22 | Linked | allowed |
| rive | Runtime, only if assets already exist | 0.14.11 | MIT | https://pub.dev/packages/rive/versions/0.14.11 | 2026-09-22 | Linked (conditional) | allowed (the Rive editor is a commercial hosted product and is not a dependency) |

## 6. Fonts, icons and illustrations

| Asset | Concern | Version | Licence | Source | Checked | Linked | Verdict |
|---|---|---|---|---|---|---|---|
| Inter | Latin UI font | Upstream v4.1; `@fontsource/inter` 5.3.0 | OFL-1.1 | https://github.com/rsms/inter/blob/master/LICENSE.txt ; https://www.npmjs.com/package/@fontsource/inter/v/5.3.0 | 2026-09-22 | Linked (bundled asset) | allowed |
| IBM Plex Sans Arabic | Arabic UI font | `@fontsource/ibm-plex-sans-arabic` 5.3.0; the upstream per-family release number was not read | OFL-1.1 | https://github.com/IBM/plex/blob/master/LICENSE.txt ; https://www.npmjs.com/package/@fontsource/ibm-plex-sans-arabic/v/5.3.0 | 2026-09-22 | Linked (bundled asset) | allowed (upstream family version unverified) |
| Noto Naskh Arabic | Arabic reading font for long text and PDFs | Upstream NotoNaskhArabic-v2.021; `@fontsource/noto-naskh-arabic` 5.3.0 | OFL-1.1 | https://github.com/notofonts/arabic/blob/main/OFL.txt ; https://www.npmjs.com/package/@fontsource/noto-naskh-arabic/v/5.3.0 | 2026-09-22 | Linked (bundled asset, and installed in the Gotenberg image) | allowed |
| Lucide | Icons | `lucide` 1.47.0; `lucide-angular` 1.0.0 | ISC | https://github.com/lucide-icons/lucide/blob/main/LICENSE ; https://www.npmjs.com/package/lucide/v/1.47.0 | 2026-09-22 | Linked | allowed |
| Material Symbols | Icons (alternative) | Upstream 4.0.0; `material-symbols` 0.47.4 (third-party packaging) | Apache-2.0 | https://github.com/google/material-design-icons/blob/master/LICENSE ; https://www.npmjs.com/package/material-symbols/v/0.47.4 | 2026-09-22 | Linked | allowed |
| Illustrations | Empty states, onboarding | Original SVG | Owned by the project under the commissioning contract | The contract, filed with the brand assets | 2026-09-22 | Linked | allowed once the contract assigns rights; any openly licensed set gets its own row here before use |

Section 6.2 names the SIL Open Font License for fonts, so OFL-1.1 fonts are `allowed` even though OFL-1.1 is not in the Section 6.1 list for code. The scanner applies this as an asset rule (§11.3).

## 7. Infrastructure images and standalone servers

| Component | Concern | Version (latest stable) | Licence of that version | Source | Checked | Linked or standalone | Verdict |
|---|---|---|---|---|---|---|---|
| PostgreSQL | Database | 18.6 (current major; 16.15 and 17.11 also supported; all released 2026-08-13) | PostgreSQL | https://github.com/postgres/postgres/blob/master/COPYRIGHT ; https://www.postgresql.org/versions.json | 2026-09-22 | Standalone | allowed |
| pgvector | Embeddings | 0.8.6 (latest tag) | PostgreSQL | https://github.com/pgvector/pgvector/blob/master/LICENSE | 2026-09-22 | Standalone (database extension) | allowed |
| pg_trgm, full-text search | Search | Ships with PostgreSQL 18.6 | PostgreSQL | https://github.com/postgres/postgres/blob/master/COPYRIGHT | 2026-09-22 | Standalone | allowed |
| PgBouncer | Connection pooling | 1.25.2 | ISC | https://github.com/pgbouncer/pgbouncer/blob/master/COPYRIGHT | 2026-09-22 | Standalone | allowed |
| pgBackRest | Backups | 2.59.1 | MIT | https://github.com/pgbackrest/pgbackrest/blob/main/LICENSE | 2026-09-22 | Standalone | allowed |
| RabbitMQ | Message broker | 4.3.6 | MPL-2.0 (server and tier 1 plug-ins; some OCF files Apache-2.0) | https://github.com/rabbitmq/rabbitmq-server/blob/main/LICENSE | 2026-09-22 | Standalone | allowed |
| Redis | Cache, backplane, rate limits, locks | 8.10.2 | RSALv2 OR SSPLv1 OR AGPLv3; used under AGPLv3 | https://github.com/redis/redis/blob/unstable/LICENSE.txt | 2026-09-22 | Standalone | allowed-standalone |
| Valkey | Cache fallback | 9.1.2 | BSD-3-Clause | https://github.com/valkey-io/valkey/blob/unstable/COPYING | 2026-09-22 | Standalone | allowed |
| SeaweedFS | File storage | 4.47 | Apache-2.0 | https://github.com/seaweedfs/seaweedfs/blob/master/LICENSE | 2026-09-22 | Standalone | allowed |
| Gotenberg | PDF rendering | 8.37.0 | MIT | https://github.com/gotenberg/gotenberg/blob/main/LICENSE | 2026-09-22 | Standalone | allowed |
| ClamAV | Upload scanning | 1.5.4 | GPL-2.0 | https://github.com/Cisco-Talos/clamav/blob/main/COPYING.txt | 2026-09-22 | Standalone | allowed-standalone |
| Tesseract | OCR | 5.5.3 | Apache-2.0 | https://github.com/tesseract-ocr/tesseract/blob/main/LICENSE | 2026-09-22 | Standalone (CLI inside a worker image) | allowed (the Arabic and English trained-data files carry their own licence: unverified) |
| Ollama | Local model serving | 0.34.2 | MIT | https://github.com/ollama/ollama/blob/main/LICENSE | 2026-09-22 | Standalone | allowed (each open-weight model's licence is recorded per model in `25-ai-and-assist-ladder.md`) |
| vLLM | Local model serving (alternative) | 0.29.0 | Apache-2.0 | https://github.com/vllm-project/vllm/blob/main/LICENSE | 2026-09-22 | Standalone | allowed |
| Grafana | Dashboards | 13.2.2 | AGPL-3.0 | https://github.com/grafana/grafana/blob/main/LICENSE | 2026-09-22 | Standalone | allowed-standalone |
| Loki | Log storage | 3.7.8 | AGPL-3.0 | https://github.com/grafana/loki/blob/main/LICENSE | 2026-09-22 | Standalone | allowed-standalone |
| Tempo | Trace storage | 3.0.3 | AGPL-3.0 | https://github.com/grafana/tempo/blob/main/LICENSE | 2026-09-22 | Standalone | allowed-standalone |
| Jaeger | Trace storage (alternative) | 2.21.0 | Apache-2.0 | https://github.com/jaegertracing/jaeger/blob/main/LICENSE | 2026-09-22 | Standalone | allowed |
| Prometheus | Metrics | 3.14.0 | Apache-2.0 | https://github.com/prometheus/prometheus/blob/main/LICENSE | 2026-09-22 | Standalone | allowed |
| OpenTelemetry Collector | Telemetry pipeline | 0.161.0 | Apache-2.0 | https://github.com/open-telemetry/opentelemetry-collector-releases/blob/main/LICENSE | 2026-09-22 | Standalone | allowed |
| GlitchTip | Error tracking | 6.2.6 (latest backend tag) | MIT | https://gitlab.com/glitchtip/glitchtip-backend/-/blob/master/LICENSE | 2026-09-22 | Standalone | allowed |
| KEDA | Queue-depth autoscaling | 2.20.2 | Apache-2.0 | https://github.com/kedacore/keda/blob/main/LICENSE | 2026-09-22 | Standalone (cluster operator) | allowed |
| OpenBao | Secrets | 2.6.2 | MPL-2.0 | https://github.com/openbao/openbao/blob/main/LICENSE | 2026-09-22 | Standalone | allowed |
| Caddy | Edge proxy and TLS | 2.11.4 | Apache-2.0 | https://github.com/caddyserver/caddy/blob/master/LICENSE | 2026-09-22 | Standalone | allowed |
| Traefik | Edge proxy (alternative) | 3.7.13 | MIT | https://github.com/traefik/traefik/blob/master/LICENSE.md | 2026-09-22 | Standalone | allowed |
| Keycloak | Alternative for SSO-heavy customers | 26.7.4 | Apache-2.0 | https://github.com/keycloak/keycloak/blob/main/LICENSE.txt | 2026-09-22 | Standalone | allowed |
| Jitsi Meet | Online classes (embed) | stable/jitsi-meet_11248 | Apache-2.0 | https://github.com/jitsi/jitsi-meet/blob/master/LICENSE | 2026-09-22 | Standalone | allowed |
| LiveKit | Online classes (alternative) | 1.13.7 | Apache-2.0 | https://github.com/livekit/livekit/blob/master/LICENSE | 2026-09-22 | Standalone | allowed |
| Apache Superset | Optional BI | 6.1.0 | Apache-2.0 | https://github.com/apache/superset/blob/master/LICENSE.txt | 2026-09-22 | Standalone | allowed |
| Mailpit | Development mail catcher | 1.31.2 | MIT | https://github.com/axllent/mailpit/blob/develop/LICENSE | 2026-09-22 | Standalone (development only) | allowed |
| Meilisearch | Search, only if it proves necessary | 1.54.0 | Not a single SPDX licence: the LICENSE file splits Community and Enterprise editions | https://github.com/meilisearch/meilisearch/blob/main/LICENSE | 2026-09-22 | Standalone (conditional) | unverified (the edition split was not read in full; decided by ADR before first use) |
| Matomo | Staff product analytics | 5.13.0 | GPL-3.0 | https://github.com/matomo-org/matomo/blob/5.x-dev/LICENSE | 2026-09-22 | Standalone | not-allowed until a Section 6.4 row and ADR exist (open point 1) |
| PostHog | Staff product analytics (alternative) | Release tags track the desktop app; the server version was not read | MIT outside `ee/`; `ee/` under its own licence | https://github.com/PostHog/posthog/blob/master/LICENSE | 2026-09-22 | Standalone | unverified (Section 6.2 already requires verifying the edition used) |
| .NET runtime image (`mcr.microsoft.com/dotnet/aspnet`) | Service base image | Tracks .NET 10.0.12 | MIT (image sources) | https://github.com/dotnet/dotnet-docker/blob/main/LICENSE | 2026-09-22 | Standalone (base image) | allowed (operating-system package licences come from the SBOM) |
| Docker Engine (Moby) | Container runtime | 29.8.1 | Apache-2.0 | https://github.com/moby/moby/blob/master/LICENSE | 2026-09-22 | Standalone | allowed |
| Podman | Container runtime (alternative) | Repository moved organisation; the version was not read | Apache-2.0 | https://github.com/containers/podman/blob/main/LICENSE | 2026-09-22 | Standalone | allowed (version unverified) |
| Istio; Cilium | Service mesh, only if proven necessary | 1.31.1; 1.20.2 | Apache-2.0 | https://github.com/istio/istio/blob/master/LICENSE ; https://github.com/cilium/cilium/blob/main/LICENSE | 2026-09-22 | Standalone (conditional) | allowed |

## 8. Delivery, security and continuous-integration tools

| Tool | Concern | Version | Licence of that version | Source | Checked | Linked or standalone | Verdict |
|---|---|---|---|---|---|---|---|
| OpenTofu | Infrastructure as code | 1.12.6 | MPL-2.0 | https://github.com/opentofu/opentofu/blob/main/LICENSE | 2026-09-22 | Tool | allowed |
| Ansible (ansible-core) | On-premises provisioning | 2.21.4 | GPL-3.0 | https://github.com/ansible/ansible/blob/devel/COPYING | 2026-09-22 | Tool | allowed-standalone |
| Argo CD | GitOps | 3.5.3 | Apache-2.0 | https://github.com/argoproj/argo-cd/blob/master/LICENSE | 2026-09-22 | Standalone | allowed |
| Flux | GitOps (alternative) | 2.9.5 | Apache-2.0 | https://github.com/fluxcd/flux2/blob/main/LICENSE | 2026-09-22 | Standalone | allowed |
| Helm | Packaging | 4.3.0 | Apache-2.0 | https://github.com/helm/helm/blob/main/LICENSE | 2026-09-22 | Tool | allowed |
| cosign | Image signing | 3.1.3 | Apache-2.0 | https://github.com/sigstore/cosign/blob/main/LICENSE | 2026-09-22 | Tool | allowed |
| Renovate | Dependency updates | 44.106.0 | AGPL-3.0-only | https://github.com/renovatebot/renovate/blob/main/license ; https://www.npmjs.com/package/renovate/v/44.106.0 | 2026-09-22 | Tool | allowed-standalone |
| Trivy | Vulnerability scan | 0.74.0 | Apache-2.0 | https://github.com/aquasecurity/trivy/blob/main/LICENSE | 2026-09-22 | Tool | allowed |
| Gitleaks | Secret scan | 8.30.1 | MIT | https://github.com/gitleaks/gitleaks/blob/master/LICENSE | 2026-09-22 | Tool | allowed |
| OWASP ZAP | Dynamic security scan | 2.17.0 | Apache-2.0 | https://github.com/zaproxy/zaproxy/blob/main/LICENSE | 2026-09-22 | Tool | allowed |
| SonarQube Community | Static analysis | 26.9.0.129388 | LGPL-3.0 | https://github.com/SonarSource/sonarqube/blob/master/LICENSE.txt | 2026-09-22 | Standalone (never linked) | allowed-standalone (LGPL run as a server; Section 6.4 covers GPL and AGPL only, but the scanner still needs an exception entry if its image is ever scanned) |
| k6 | Load tests (Section 6.2, Testing paragraph; Section 6.4 row since brief v9.1) | 2.3.0 | AGPL-3.0 | https://github.com/grafana/k6/blob/master/LICENSE.md | 2026-09-22 | Tool (load-test binary in CI and test environments, never linked, never shipped) | allowed-standalone: master brief Section 6.4 row and the `allow.json` entry with ADR `0019-brief-v9-1-corrections` (§11.1; open point 1 closed) |
| oasdiff | Breaking-change detection | 1.32.1 | Apache-2.0 | https://github.com/oasdiff/oasdiff/blob/main/LICENSE | 2026-09-22 | Tool | allowed |
| buf | gRPC lint and breaking checks | 1.73.0 | Apache-2.0 | https://github.com/bufbuild/buf/blob/main/LICENSE | 2026-09-22 | Tool | allowed |
| Forgejo | Self-hosted forge (alternative to GitHub) | 16.0.5 | GPL-3.0-or-later (since v9.0) | https://codeberg.org/forgejo/forgejo/src/branch/forgejo/LICENSE ; https://forgejo.org/2024-08-gpl/ | 2026-09-22 | Standalone | not-allowed until a Section 6.4 row and ADR exist (open point 1) |
| Woodpecker CI | Self-hosted CI (alternative) | 3.18.1 | Apache-2.0 | https://github.com/woodpecker-ci/woodpecker/blob/main/LICENSE | 2026-09-22 | Standalone | allowed |
| GitHub Actions | CI (free tier) | Hosted service | Free tier of a hosted service, not a licence | n/a | 2026-09-22 | Service | named by Section 6.2; see open point 5 |

**Count.** 149 rows across §2 to §8. 144 are verified for both version and licence from the source for that version. 5 rows carry `unverified` in part: the IBM Plex Sans Arabic upstream family version, Tesseract trained data, Meilisearch (edition split), PostHog (edition and server version), and the Podman version. The native Skia and ImageMagick components bundled by SkiaSharp and Magick.NET are also unverified (§2 note). Nothing is guessed.

---

## 9. Flagged items

### 9.1 Items whose licence or status differs from Section 6.2, or changed recently

| Item | What Section 6.2 or 6.4 says | What the source says on 2026-09-22 | Version where it changed | Source | Consequence |
|---|---|---|---|---|---|
| k6 | Named in the Testing paragraph with no licence; brief v9.1 added the Section 6.4 row under ADR-0019 (load, soak and performance-budget tests; a standalone binary in CI and test environments only) | AGPL-3.0 | Current 2.3.0 | https://github.com/grafana/k6/blob/master/LICENSE.md | The Section 6.4 row and the `allow.json` entry (ADR `0019-brief-v9-1-corrections`) both exist, so the load-test jobs pass the scan. Open point 1, closed |
| Forgejo | Named in the CI row with no licence | GPL-3.0-or-later | 9.0 (older releases MIT) | https://forgejo.org/2024-08-gpl/ | If Forgejo is chosen it needs what k6 now has: a Section 6.4 row, an `allow.json` entry and an ADR. Open point 1 |
| Matomo | Named for staff analytics, "verify the license" | GPL-3.0 | Current 5.13.0 | https://github.com/matomo-org/matomo/blob/5.x-dev/LICENSE | If Matomo is chosen it needs what k6 now has: a Section 6.4 row, an `allow.json` entry and an ADR. Open point 1 |
| SonarQube Community | Named in the CI row with no licence | LGPL-3.0 | Current 26.9.0.129388 | https://github.com/SonarSource/sonarqube/blob/master/LICENSE.txt | Standalone use is fine; recorded so nobody links its libraries |
| Valkey | Reference architecture Section 16 pins Valkey 9.1 since v9.1 (ADR-0019), and never Redis below 8.0 | Current is 9.1.2, still BSD-3-Clause | 9.x minor | https://github.com/valkey-io/valkey | Major-version drift from the pinned table, not a licence change. Open point 4 |
| Redis | 6.4 row: Redis 8, AGPLv3 option | Tri-licence RSALv2 OR SSPLv1 OR AGPLv3 from 8.0; 7.2 and earlier BSD-3-Clause; the 7.4 line has no AGPL option | 7.4 (RSALv2 or SSPLv1 only), 8.0 (AGPLv3 added) | https://github.com/redis/redis/blob/unstable/LICENSE.txt | Never pin a 7.4.x image. `TC-SEC-394` below |
| Shouldly | Named with no licence | BSD-3-Clause | 4.3.0 (4.0.0 to 4.2.x were BSD-2-Clause) | https://www.nuget.org/packages/Shouldly/4.3.0 | Both allowed; recorded because the licence changed |
| CsvHelper | MIT / Apache-2.0 | `MS-PL OR Apache-2.0` | Current 33.1.0 | https://www.nuget.org/packages/CsvHelper/33.1.0 | Allowed by electing Apache-2.0; the scanner must understand `OR` (§11.3) |
| RabbitMQ.Client | Not named; used by the fallback bus and transport | `Apache-2.0 OR MPL-2.0` | Current 7.2.2 | https://www.nuget.org/packages/RabbitMQ.Client/7.2.2 | Allowed; scanner must understand `OR` |
| NetArchTest.Rules | Named for architecture rules | MIT, but the nuspec carries no licence element and the last release is 1.3.2 of 2021-05-23 | 1.3.2 | https://github.com/BenMorris/NetArchTest | The scanner reads "no licence recorded" and fails; maintenance risk. Open point 3 |
| Bogus, dotnet-stryker | Named | MIT and Apache-2.0 respectively, carried as a licence file rather than an SPDX expression | Current versions | §3 | The scanner needs an override map for file-licensed packages (§11.3) |
| MinIO | Banned, "treat as unavailable" | Repository archived (read-only); last release RELEASE.2025-10-15T17-29-55Z; AGPL-3.0 | Maintenance mode December 2025; archived 2026 | https://github.com/minio/minio ; https://github.com/minio/minio/issues/21714 | Confirms the ban; SeaweedFS stays the default |
| flutter_animate | Named | BSD-3-Clause; last release 4.5.2 on 2024-11-25 | n/a | https://pub.dev/packages/flutter_animate | Maintenance watch only; built-in implicit and Hero animations are the fallback Section 6.2 already names |
| OFL-1.1 fonts | Section 6.2 names the SIL Open Font License | OFL-1.1 | n/a | §6 | The first scanner version rejected OFL-1.1 today (§11.3) |

### 9.2 The banned list, with the version where each crossed the line

| Item | Last acceptable version and licence | First offending version and licence | Current version on 2026-09-22 | Source | Replacement named in Section 6.2 |
|---|---|---|---|---|---|
| MassTransit | 8.x, Apache-2.0 | 9.0.0, commercial (`https://massient.com/license`) | 9.2.2 | https://www.nuget.org/packages/MassTransit/9.0.0 | Wolverine; fallback Rebus or `RabbitMQ.Client` with a thin in-house bus |
| MediatR | 12.x, Apache-2.0 | 13.0.0, dual Reciprocal Public License 1.5 or commercial (Lucky Penny Software) | 14.2.0 | https://www.nuget.org/packages/MediatR/13.0.0 ; https://github.com/LuckyPennySoftware/MediatR/blob/main/LICENSE.md | Wolverine's in-process mediator |
| AutoMapper | 14.x, MIT | 15.0.0, dual Reciprocal Public License 1.5 or commercial (Lucky Penny Software) | 16.2.0 | https://www.nuget.org/packages/AutoMapper/15.0.0 | Mapperly or explicit manual mapping |
| FluentAssertions | 7.x, Apache-2.0 | 8.0.0, Xceed community licence for non-commercial use, commercial otherwise | 8.11.0 | https://www.nuget.org/packages/FluentAssertions/8.0.0 ; https://github.com/fluentassertions/fluentassertions/blob/main/LICENSE | Shouldly or AwesomeAssertions |
| SixLabors.ImageSharp | 2.x, Apache-2.0 (unmaintained, so not a way around the policy) | 3.0.0, Six Labors Split License 1.0 | 4.1.2 | https://www.nuget.org/packages/SixLabors.ImageSharp/3.0.0 ; https://github.com/SixLabors/ImageSharp/blob/main/LICENSE | SkiaSharp or Magick.NET |
| QuestPDF | 2022.12.15, MIT | 2023.4.0, QuestPDF Community licence (revenue-gated) or commercial | 2026.9.0 | https://www.nuget.org/packages/QuestPDF/2023.4.0 ; https://github.com/QuestPDF/QuestPDF/blob/main/LICENSE.md | Gotenberg; PDFsharp and MigraDoc for simple documents |
| EPPlus | 4.5.3.3, LGPL-3.0 | 5.0.3 (first stable 5.x), PolyForm Noncommercial 1.0.0 or commercial | 8.7.0 | https://www.nuget.org/packages/EPPlus/5.0.3 ; https://github.com/EPPlusSoftware/EPPlus/blob/develop8/license.md | ClosedXML; CsvHelper for CSV |
| Duende.IdentityServer | None: IdentityServer4 (Apache-2.0) was a different package line | 5.0.0 (first Duende package), Duende commercial licence | 8.0.8 | https://www.nuget.org/packages/Duende.IdentityServer/5.0.0 ; https://github.com/DuendeSoftware/products/blob/main/LICENSE | ASP.NET Core Identity with OpenIddict; Keycloak for SSO-heavy customers |
| MinIO | Community releases, AGPL-3.0 | Console removed from the community edition May 2025; binaries and images stopped October 2025; repository archived 2026 | RELEASE.2025-10-15T17-29-55Z (archived) | https://github.com/minio/minio ; https://github.com/minio/minio/issues/21714 | SeaweedFS; local disk for small installs |
| Terraform | 1.5.x, MPL-2.0 | 1.6.0, Business Source License | 1.16.3 | https://github.com/hashicorp/terraform/blob/main/LICENSE | OpenTofu |
| HashiCorp Vault (banned in the Secrets row) | 1.14.x, MPL-2.0 | 1.15.0, Business Source License | 2.1.1 | https://github.com/hashicorp/vault/blob/main/LICENSE | OpenBao |
| Redis Enterprise and paid Redis modules | Never open source | Commercial | n/a | Section 6.2 | Redis 8 under AGPLv3, standalone; Valkey |
| Z.EntityFramework.Extensions.EFCore | Never open source | Commercial (`https://zzzprojects.com/license-agreement/`) | 10.105.8.1 | https://www.nuget.org/packages/Z.EntityFramework.Extensions.EFCore | `Npgsql` binary COPY; EF Core `ExecuteUpdate` and `ExecuteDelete` |

The last-acceptable versions for MassTransit, MediatR, AutoMapper and FluentAssertions come from reading the first and last nuspec of every major line on nuget.org on 2026-09-22; the Grafana, Loki and Tempo move from Apache-2.0 to AGPL-3.0 was announced in April 2021 (https://grafana.com/blog/grafana-loki-tempo-relicensing-to-agplv3/) and is already covered by their Section 6.4 rows.

---

## 10. Unavoidable costs (master brief Section 6.3)

| Cost | Nature | Adapter that isolates it | Default when switched off |
|---|---|---|---|
| Firebase Cloud Messaging, Apple Push Notification service | Free of charge, not open source; the one accepted exception | `IPushSender` | In-app inbox and email |
| Apple Developer Program, Google Play Console | Account fees | Release pipeline in `15-deployment-and-operations.md` | Web and PWA only |
| SMS and WhatsApp | Per-message carrier charges | SMS and WhatsApp adapters behind the notification sender | Email and push fallback; SMS stays optional |
| Online payments | Per-transaction gateway fees | `IPaymentGateway` | Manual and bank-transfer implementation |
| Email delivery at scale | Relay or provider charges | MailKit through a configurable SMTP relay | The school's own SMTP server |
| Servers, domains | Hosting and registration | Deployment modes in `15-deployment-and-operations.md` | On-premises install |
| AI hardware | A capable machine for a local model | `Microsoft.Extensions.AI` behind the assist ladder (`25-ai-and-assist-ladder.md`) | AI features degrade to their non-AI path |

---

## 11. Scanner allow-list and the four-field rule

### 11.1 The allow-list, quoted from `tools/license-scan/allow.json`

| name | license | justification | adr |
|---|---|---|---|
| redis | AGPL-3.0 | Cache, real-time backplane, rate limits and locks. Run as an unmodified server image; only the MIT StackExchange.Redis client is linked. Valkey is a drop-in fallback. | 0005-redis-agpl-standalone-valkey-fallback |
| grafana | AGPL-3.0 | Dashboards. Separate container, no product code links to it, dashboards are JSON in this repository. | 0005-redis-agpl-standalone-valkey-fallback |
| loki | AGPL-3.0 | Log storage. Separate container, reached over HTTP only. | 0005-redis-agpl-standalone-valkey-fallback |
| tempo | AGPL-3.0 | Trace storage. Separate container, reached over OTLP only. | 0005-redis-agpl-standalone-valkey-fallback |
| clamav | GPL-2.0 | Upload scanning. Separate container, reached over its socket protocol. | 0005-redis-agpl-standalone-valkey-fallback |
| ansible | GPL-3.0 | On-premises provisioning. A build tool, never shipped inside a product image. | 0005-redis-agpl-standalone-valkey-fallback |
| renovate | AGPL-3.0 | Dependency update pull requests. A continuous integration tool, never shipped. | 0005-redis-agpl-standalone-valkey-fallback |
| k6 | AGPL-3.0 | Load and soak tests. A continuous integration tool run as its own binary against a deployed environment; no product code links to it and the scenarios are JavaScript in this repository. Master brief Section 6.4 carries the row since v9.1. | 0019-brief-v9-1-corrections |

The file's own note reads: "Nothing linked into product code may appear here."

Eight entries, quoted as the file stands on 2026-09-26. The k6 entry matches the Section 6.4 row brief v9.1 added under ADR-0019, which closed open point 1. This table is not edited ahead of the file.

### 11.2 The four-field rule

Master brief Section 6.4: every entry names the tool, the licence, the Section 6.4 row that justifies it, and the ADR. `run.mjs` enforces it by checking `name`, `license`, `justification` and `adr` on every entry and failing the scan when any is empty. Adding an entry is a reviewed change, and the licence auditor re-verifies the whole list at every release.

### 11.3 Scanner cases found here, and how they were fixed

The first version of `tools/license-scan/run.mjs` compared a licence string against fixed sets, which would have failed four legitimate package shapes. All four were fixed on 2026-09-22 and each has a test in `tools/license-scan/run.test.mjs` (12 tests, passing).

| Case | Packages affected | Fix now in the scanner | Test |
|---|---|---|---|
| SPDX `OR` expressions | CsvHelper, RabbitMQ.Client | The expression is parsed; `OR` passes when any alternative is allowed, `AND` only when every term is | "a dual licence is allowed when either branch is"; "a conjunction is allowed only when every branch is" |
| OFL-1.1 | `@fontsource/*` fonts | An asset rule: OFL-1.1 and the Creative Commons asset licences pass for `kind: "asset"` and are refused for linked code, per master brief Section 6.2 | "font licences are allowed as assets and refused as linked code" |
| Licence carried as a file | Bogus, dotnet-stryker, NetArchTest.Rules | Refused with a message naming the fix: record the SPDX identifier under `overrides` in `tools/license-scan/allow.json`, keyed `<package>@<exact version>`. An override whose licence is itself not allowed fails the scan | "a licence shipped as a file is refused until an override records its SPDX identifier" |
| `-only` and `-or-later` spellings | renovate (`AGPL-3.0-only`), LGPL packages | Suffixes are normalised before comparison, so the policy judges the licence, not its spelling | "the -only and -or-later suffixes are judged on the licence they spell" |
| Tools not yet listed | Forgejo or Matomo if chosen (k6 is now listed in both places, §11.1) | Correctly refused until listed in master brief Section 6.4 **and** `allow.json` | Open point 1 |

---

## 12. Pinning and update policy

| Rule | Where it lives | How it is enforced |
|---|---|---|
| Exact versions only, one place per ecosystem: `Directory.Packages.props` with central package management for NuGet, `package.json` plus `package-lock.json` for npm, `pubspec.yaml` plus `pubspec.lock` for pub | Repository root and each workspace | Restore in locked mode; `TC-SEC-380` fails a restore without the lock file (`12-security-privacy-safety.md`) |
| No floating ranges, no `*`, no `latest` image tags; images pinned by tag and digest | Helm values and compose files | Trivy config check and a review rule |
| Majors follow reference architecture Section 16; changing a pinned major needs an ADR | `29-adr-index.md` | Review |
| Renovate groups non-security updates weekly, one pull request per ecosystem group (.NET runtime and EF Core together, Angular together, Flutter together, images together) | `renovate.json` | Renovate schedule |
| Security updates are opened immediately, outside the weekly group | `renovate.json` vulnerability alerts | Renovate |
| Anything rated critical is merged and deployed within 48 hours | Reference architecture Section 11, supply chain | Tracked as an incident with the 48-hour clock |
| Before any version change, the licence of the new exact version is re-read at the source and this document is updated in the same pull request | This document | The licence auditor agent and the scan in §11 |
| A package whose licence changes in a newer version is capped below the first offending version in `Directory.Packages.props` with a comment naming this document | `Directory.Packages.props` | Renovate `allowedVersions` rule per capped package |

## 13. SBOM and signing policy

| Rule | Detail | Where it runs |
|---|---|---|
| Every image carries a CycloneDX SBOM | Generated by the CycloneDX .NET tool (6.2.0) and `@cyclonedx/cyclonedx-npm` (6.0.1) for application components, plus an image scan for operating-system packages; attached to the image as an attestation | Every build |
| Every image is signed with cosign (3.1.3) | Keyless signing in CI, or a key held in OpenBao for on-premises builds | Every build |
| The cluster admission controller refuses an unsigned image | Admission policy in the platform chart | Every deploy |
| Target SLSA build level 2 at launch | Provenance attestation alongside the signature | Every release |
| `THIRD-PARTY-LICENSES.md` is generated from the SBOMs, not written by hand | Lists every component, version and licence, including fonts and native components | Every release |
| Vulnerability and licence scans on every build | Trivy for images and dependencies; the licence scan in §11 | Every pull request and every build |

---

## Decisions in force

| Decision | Source | State | Why |
|---|---|---|---|
| Only MIT, Apache-2.0, BSD, ISC, MPL-2.0 and PostgreSQL for linked code; LGPL dynamic only | Master brief Section 6.1 | In force | A licence change should mean one adapter, not a rewrite |
| GPL and AGPL only as standalone tools, each listed in Section 6.4 with an `allow.json` entry and an ADR | Master brief Section 6.4; ADR `0005-redis-agpl-standalone-valkey-fallback` | In force | Keeps copyleft at arm's length and visible |
| OFL-1.1 is allowed for fonts only | Master brief Section 6.2 | In force; the scanner applies it as an asset rule (§11.3) | Fonts are assets, not code |
| Where a package offers `A OR B`, the allowed alternative is elected and recorded | This document §9.1 | Proposed | CsvHelper and RabbitMQ.Client would otherwise block the scan |
| k6 runs as a standalone load-test tool in CI and test environments, never linked and never shipped | Master brief Section 6.4, row added in brief v9.1 under ADR-0019 | In force: the Section 6.4 row and the `allow.json` entry with ADR `0019-brief-v9-1-corrections` (§11.1) | Every load gate in documents 15, 16 and 17 runs on it |
| FsCheck is the property-based library for the arithmetic rules, pinned at 3.4.0 | `16-test-strategy.md` §6.2; this document §3 | Proposed, pending the ADR in open point 7 | Appendix V requires a property-based test and Section 6.2 names no library |
| Redis is never pinned below 8.0 | This document §9.1 | Proposed | 7.4 has no AGPL option |
| Capped packages carry a Renovate `allowedVersions` rule | This document §12 | Proposed | An automated update must never cross a licence change |

## Dependencies on other documents

| This document assumes | Stated in | Checked on |
|---|---|---|
| The approved stack, banned list, costs and standalone list | Master brief Section 6 | Every change to Section 6 |
| Pinned majors | Reference architecture Section 16 | Group E review |
| Supply-chain controls and the 48-hour critical rule | Reference architecture Section 11; master brief Section 20 | Group E review |
| Lock-file enforcement `TC-SEC-380` and the flutter secure storage pin | `12-security-privacy-safety.md` | Group E review |
| Which property-based library the arithmetic rules use, and which load tool the gates use | `16-test-strategy.md` §6.2 and part 10 | Group E review |
| The generator, Scalar, Spectral, `oasdiff` and buf usage | `08-web-structure.md`; `22-api-conventions-and-error-catalog.md` | Group D review |
| The pub packages used by the app | `09-mobile-structure.md` | Group B review |
| Model licences for Ollama and vLLM | `25-ai-and-assist-ladder.md` | Group D review |
| Deployment of standalone tools | `15-deployment-and-operations.md` | Group E review |
| Who approves a new dependency | `29-adr-index.md` | Every new dependency |

## Open points

| Question | Default | Owner | Impact if the default is wrong | L | I | Score | In the register |
|---|---|---|---|---|---|---|---|
| 1. Closed: k6 (AGPL-3.0) has its Section 6.4 row since brief v9.1 (ADR-0019) and its `allow.json` entry with the four fields. Forgejo (GPL-3.0 since 9.0) and Matomo (GPL-3.0) would need both if chosen | Both are recorded; the licence scan passes with the entry in place | Architect, with the licence auditor | Without the entry the first load-test job fails the scan, and every load gate in documents 15, 16 and 17 waits on it | 1 | 3 | 3 | RISK-40 |
| 2. The scanner mishandled `OR` expressions, OFL-1.1, file-carried licences and the `-only` suffix (§11.3) | **Resolved 2026-09-22.** All four fixed in `tools/license-scan/run.mjs`; overrides live in `allow.json` and each must record `spdx`, `source` and `checked`, and cannot launder a banned licence. 12 tests in `run.test.mjs` pass | Tech lead | Closed | 1 | 3 | 3 | RISK-26 |
| 3. NetArchTest.Rules has had no release since 2021-05-23 and publishes no licence metadata | Keep it; add an override entry. If it breaks on .NET 10, replace it with TngTech.ArchUnitNET 0.13.4 (Apache-2.0), which is not named in Section 6.2 and needs an ADR | Tech lead | Architecture rules stop running, which removes a guard on service boundaries | 3 | 2 | 6 | RISK-11 |
| 4. Closed by ADR-0019: reference architecture Section 16 pins Valkey 9.1 (current 9.1.2). The fallback suite runs against it | Run the fallback suite against Valkey 9.1.2 and amend Section 16 by ADR if it passes | Architect | The fallback silently drifts from the version the integration suite proves | 2 | 3 | 6 | RISK-26 |
| 5. GitHub Actions is a free tier of a hosted service, which Section 6.1 lists as not allowed, while Section 6.2 names it | Keep it as named; Woodpecker CI (Apache-2.0) is the replacement if the free tier changes | Product owner | A pricing change on the free tier stops delivery | 2 | 3 | 6 | RISK-26 |
| 6. Meilisearch and PostHog editions, Tesseract trained data, native ImageMagick and Skia, the IBM Plex Sans Arabic family version and the Podman version are `unverified` | Verify each at the source before first use; nothing in phase 1 depends on them except the fonts and native image libraries, which are verified before the first release | Licence auditor | An unverified component reaches a release | 2 | 3 | 6 | RISK-26 |
| 7. FsCheck is the default property-based library in `16-test-strategy.md` §6.2 and is named by `13-workflows-and-sagas.md`, but master brief Section 6.2's Testing list names no property-based library | Pin FsCheck 3.4.0 and `FsCheck.Xunit` 3.4.0 (§3, both BSD-3-Clause) and record the choice by ADR, as TngTech.ArchUnitNET's row would need. CsCheck and Hedgehog stay named alternatives with no row until one is chosen | Tech lead | A test-only dependency enters the build without the ADR that Section 6.2 additions require | 2 | 1 | 2 | none |

No verified item forces a replacement today. Every banned item in §9.2 already has its Section 6.2 replacement in the stack.

## Review record

| Date | Reviewer | Verdict | Blocking items |
|---|---|---|---|
| 2026-09-22 | Group E review pending | Draft | none recorded yet |
| 2026-09-22 | Scorecard remediation, theme 4 | Amended: FsCheck 3.4.0 and `FsCheck.Xunit` 3.4.0 added to §3 from each nuspec (row count 147 to 149); k6 re-verdicted `allowed-standalone` on the Section 6.4 row brief v9.1 added under ADR-0019; the §11.3 scanner test count corrected from 11 to 12 | The k6 `allow.json` entry, open point 1 (closed since) |
| 2026-09-26 | Round-2 scorecard, Group E, remediation round 3 | Amended: every k6 statement (§8, §9.1, §11.1, §11.3, Decisions in force) now agrees with the file, which has eight entries including k6 under ADR `0019-brief-v9-1-corrections`; §11.1 re-quoted with the k6 row | none |

## How this document is verified

| Claim | Proof | Where it runs |
|---|---|---|
| Every row's licence is that of the exact version | The licence auditor re-reads each pinned version at the source listed and compares it to this table | Every release; every pull request that changes a manifest |
| No disallowed licence reaches a build | `tools/license-scan/run.mjs` (in the kit today with its own `run.test.mjs`; its path and the `license-scan.yml` job are in document 07, and SL-SEC-001 builds the product pipeline step) over NuGet, npm and pub with the fixes recorded in §11.3; `TC-SEC-393` (REQ-SEC-012) seeds a BSL package and an `allow.json` entry without `adr` and expects both to fail | Every pull request |
| No banned package is referenced | `TC-SEC-395`: a deny-list check over `Directory.Packages.props`, `package.json` and `pubspec.yaml` for every package in §9.2 | Every pull request |
| No Redis image below 8.0 | `TC-SEC-394`: the image tags in Helm values and compose files are parsed and a `redis` tag below 8.0 fails | Every pull request touching deployment files |
| Every image is signed and carries an SBOM | Admission refusal of an unsigned test image in the staging cluster | Every deploy to staging |
| This document agrees with the catalogs | kit-lint R01, R02 and R05 for section and appendix references and forbidden words; `plan-consistency-checker` with `license-auditor` compares this document with master brief Section 6, reference architecture Section 16 and `12-security-privacy-safety.md` | kit-lint on every change under `docs/`; the comparison at the Group E review and on every change to any of those documents |
