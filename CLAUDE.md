# Nibras (نبراس)

Nibras is a multi-tenant, microservices school management platform. Angular web, ASP.NET Core on .NET 10, Flutter mobile, PostgreSQL with EF Core, Redis, RabbitMQ, open source only.

## Source of truth, in precedence order

1. The product owner's explicit instruction in this conversation.
2. `docs/brief/01-master-brief.md` (v9.3): vision, rules, method. 40 sections.
3. `docs/brief/02-appendices/` (v9.3): one file per appendix, A to X. **Appendix L is the canonical registry** of service names, identifier formats and ownership decisions. Nothing redefines it.
4. `docs/brief/03-reference-architecture.md` (v9.3): structures, service sheets, topology, delivery, operations. 17 sections.
5. `docs/plan/PLAN_SPEC.md`: what the plan must contain. `docs/plan/`: the approved plan.
6. Your own assumptions, stated out loud.

When two sources conflict, stop and ask. All three brief files carry the same version; a mismatch is a defect.

These files are large. **Do not hold them in memory.** `docs/brief/INDEX.md` lists every section and appendix in one line each, and `docs/brief/READING_MAP.md` says which of them a given command needs. Read the sections for the task, then re-read when unsure.

At the start of every session read `docs/project/PROJECT_STATE.md` and `docs/project/OPEN_QUESTIONS.md` first, or just run `/resume`.

## Current phase

Planning. **No implementation code until the plan in `docs/plan/` is approved by the product owner.** Start with `/plan-platform`.

## Non-negotiable rules

- **Naming.** Product name Nibras. Solution `Nibras.sln`, projects `Nibras.<Service>.<Layer>`, databases `nibras_<service>`, exchanges `nibras.<service>`, images `nibras/<service>-<kind>`. Service names, area codes and identifier formats: **Appendix L**. Naming table: reference architecture Section 7. The display name lives in one configuration value; never hard-code it.
- **Open source only.** Allowed for linked code: MIT, Apache-2.0, BSD, ISC, MPL-2.0, PostgreSQL. Standalone AGPL and GPL tools are listed with their justification in master brief Section 6.4 and nowhere else. Never MediatR, AutoMapper, MassTransit v9+, Duende IdentityServer, QuestPDF, EPPlus, ImageSharp v3+, FluentAssertions v8+, MinIO, Redis Enterprise, Terraform, or any paid UI kit. If a need cannot be met with an allowed licence, stop and present options.
- **Microservices discipline.** Database per service. No shared business logic. No reading another service's data. Asynchronous first through RabbitMQ with outbox and idempotent consumers. At most one synchronous gRPC hop. Master brief Sections 7 and 8.
- **Every service has the same shape.** Follow the anatomy in reference architecture Section 2 and use the service template. Deviations need an ADR.
- **Fast by design.** EF Core: pooled contexts, named `Tenant` and `SoftDelete` filters, `AsNoTracking` with `Select` to DTOs, keyset pagination, `ExecuteUpdate` and `ExecuteDelete`, binary `COPY` for bulk, no lazy loading, no N+1. Caching through `Nibras.BuildingBlocks.Caching` only, tenant in every key, invalidation by event and tag. Budgets: master brief Section 19. Breaking a budget fails the pipeline.
- **Tenancy and authorization are enforced on the server in every service**, with row-level security as a second barrier, and covered by generated tests. Master brief Sections 7.4, 7.5, 9, 20.
- **Seeded administrator** `admin` from configuration, with every safeguard in master brief Section 10.2. Platform scope only. Never log it, never put it in client code.
- **Two languages from the first screen:** English and Arabic with full right-to-left. No hard-coded strings.
- **Children's data.** Privacy and safeguarding rules in Section 20 override convenience. Wellbeing data never leaves its service, never reaches a device, never enters a cache.
- **Angular:** native CSS animations with `animate.enter` and `animate.leave`, and view transitions. Never `@angular/animations`.
- **Cross-platform.** The kit and the product run on Windows and Linux. No hard-coded path separators, no culture-sensitive parsing without an explicit culture, every tool ships a `.ps1` and a `.sh` wrapper over one Node implementation. Appendix X and `.claude/rules/portability.md`.

## Working rules

- Work in small, complete vertical slices. Never deliver broad, shallow, half-finished work. No placeholder markers and no mock data in finished modules.
- State which requirement identifiers a change covers. Update `TRACEABILITY.md` and `PROJECT_STATE.md` when you finish.
- **Never claim something works unless you ran it.** Say what you ran and what happened. Label anything unverified.
- Every business rule has an identifier in Appendix S and a named test class; every workflow has a state machine in Appendix R and a test per transition.
- Do not invent library APIs. Check the documentation of the pinned version.
- Ask before choosing when the brief is ambiguous. Record significant decisions as ADRs in `docs/project/DECISIONS/`.
- **Never edit anything under `docs/brief/` without an ADR and a version bump on all three briefs.**
- **Never run `git commit`, `git push`, merge, tag, or amend unless the user explicitly asks for it in that message.**
- Prefer the simplest design that meets the requirement. Every abstraction must justify itself.

## Checking your work

Run the kit lint before ending any session that touched `docs/`:

```bash
node tools/kit-lint/kit-lint.mjs .
```

On Windows PowerShell, `tools/kit-lint/kit-lint.ps1`. On bash, `tools/kit-lint/kit-lint.sh`. It must exit clean. `/lint-plan` does the same and explains what to fix.

## Commands once code exists

- Run everything locally: `aspire run` from `src/AppHost`, or `docker compose -f deploy/compose/docker-compose.yml --profile dev up`
- Backend tests `dotnet test`; web `npm run lint && npm test` in `src/Web`; mobile `flutter analyze && flutter test` in `src/Mobile`
- Licence scan: `node tools/license-scan/run.mjs` (must pass before any dependency change is accepted)
- Developer setup per operating system: `docs/dev-setup/`

## Slash commands

Planning: `/plan-platform`, `/plan-service`, `/plan-tasks`, `/lint-plan`, `/plan-review`, `/score-plan`, `/plan-status`, `/decide`, `/ideate`, `/differentiate`, `/retro`.
Building: `/build-foundation`, `/build-service`, `/build-web`, `/build-mobile`, `/fix-bug`, `/change-request`, `/migrate-legacy`, `/verify-setup`.
Checking: `/review-architecture`, `/audit-security`, `/audit-privacy`, `/audit-messaging`, `/audit-licenses`, `/audit-ux`, `/audit-a11y-rtl`, `/audit-portability`, `/threat-model`, `/verify-performance`, `/optimize-service`, `/simulate-year`, `/demo-script`, `/runbook`, `/release-ready`, `/onboard-tenant`, `/resume`.
