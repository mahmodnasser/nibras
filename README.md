# Nibras (نبراس): Claude Code Planning Kit v9

Everything Claude Code needs to produce a professional plan for **Nibras**, a multi-tenant K-12 school management platform, and then to build it. Angular, ASP.NET Core on .NET 10, Flutter, PostgreSQL, Redis, RabbitMQ, open source only, English and Arabic with full right-to-left from the first screen.

The plan will show **every service and every structure**: a service catalog, a specification sheet per service with its folder tree, and the full repository, Angular, Flutter and deployment trees.

---

## What is in this kit

```
CLAUDE.md                          project instructions, loaded every session
.gitattributes .editorconfig       line endings and paths that survive Windows and Linux
.claude/
  commands/       35               /plan-platform, /build-*, /audit-*, /lint-plan, /score-plan ...
  agents/         15               architecture, security, privacy, messaging, portability, rules,
                                   test strategy, localization, domain, innovation, demo reviewers
  skills/         17               reusable know-how: service sheets, EF Core performance, saga
                                   design, Arabic normalization, cross-platform .NET, and more
  rules/          14               path-scoped rules for backend, web, mobile, contracts, tests,
                                   deployment, messaging, portability and the brief itself
  settings.json                    permissions, plus hooks that lint documents and guard commits
docs/
  brief/
    INDEX.md                       every section in one line, with file sizes
    READING_MAP.md                 what each command must read, with a reading budget
    01-master-brief.md             vision, rules, architecture, method, delivery (40 sections)
    02-appendices/   25 files      A to X: features, permissions, notifications, events, entities,
                                   settings, demo data, role templates, classification, error codes,
                                   the canonical registry, offline rules, load scenarios, the demo,
                                   differentiation, acceptance scripts, 52 workflows, 95 business
                                   rules, a year simulation, journeys, coverage, features, platforms
    03-reference-architecture.md   structures, 22 service sheets, delivery, secrets, backup,
                                   isolation, environments, versions, platform support (17 sections)
  plan/
    PLAN_SPEC.md                   the 34 plan documents, in six review groups, with a scorecard
    REVIEW_GUIDE.md                what to look for per group, and the decisions workshop agenda
    06-services/                   one specification sheet per service will be written here
  project/                         state, open questions, traceability, backlog, risks, ideas,
                                   decision records, and the v8 to v9 findings map
  templates/       21              service sheet, ADR, runbook, threat model, test case, and more
  dev-setup/                       Windows, Linux, macOS, troubleshooting
tools/
  kit-lint/                        18 consistency rules, 24 self-tests, both shell wrappers
  license-scan/                    open-source policy enforcement and the standalone allow-list
  dev-setup/                       verify a developer machine, on any operating system
```

---

## Getting started

### Windows

```powershell
git config --global core.autocrlf false
git config --global core.longpaths true
git init
tools\dev-setup\verify-setup.ps1
claude
```

### Linux or macOS

```bash
git config --global core.autocrlf false
git init
tools/dev-setup/verify-setup.sh
claude
```

`verify-setup` checks the toolchain, the container runtime and the Git configuration, and tells you exactly what is missing. `docs/dev-setup/` has the install steps per operating system.

### Then

1. Open `docs/project/OPEN_QUESTIONS.md`. Answer what you can. **Every question has a default, so nothing blocks the plan.**
2. Confirm master brief Section 27 in one 90-minute workshop. `docs/plan/REVIEW_GUIDE.md` has the agenda and the sign-off record.
3. Run **`/plan-platform`**. Claude Code reads the brief sections the task needs, tells you what it understood, then writes the plan under `docs/plan/` in six groups and stops for your review after each one.
4. Review with `docs/plan/REVIEW_GUIDE.md`. It gives five questions per group and shows how to phrase feedback that turns into a change.
5. When a group is approved, continue with `/plan-platform B`, `C`, and so on. To deepen one service later: `/plan-service Attendance`.
6. Once every group is approved: `/build-foundation`, then `/build-service <name>`, `/build-web`, `/build-mobile`.
7. Before each release: `/review-architecture`, `/audit-security`, `/audit-privacy`, `/audit-licenses`, `/audit-ux`, `/audit-a11y-rtl`, `/audit-portability`, `/verify-performance`, `/simulate-year`, `/release-ready`.
8. In every new session: `/resume`.

---

## Where to look in the finished plan

| You want to see | Open |
|---|---|
| Every service in one table, and why each exists | `docs/plan/05-service-catalog.md` |
| One service in full: API, events, entities, folder tree | `docs/plan/06-services/<service>.md` |
| The whole repository structure | `docs/plan/07-solution-structure.md` |
| The Angular structure and screen inventory | `docs/plan/08-web-structure.md` |
| The Flutter structure and offline design | `docs/plan/09-mobile-structure.md` |
| RabbitMQ topology and the message catalog | `docs/plan/11-messaging-architecture.md` |
| Deployment, backup, disaster recovery | `docs/plan/15-deployment-and-operations.md` |
| Caching, hot queries, performance budgets | `docs/plan/21-performance-engineering.md` |
| Phases, scope, exit criteria | `docs/plan/17-roadmap.md` |

## Where to look in the brief

| You want to know | Open |
|---|---|
| What a service is called, and who owns what | Appendix L. Read this first |
| How a process actually runs, step by step | Appendix R, 52 state machines |
| What a number should be, with worked examples | Appendix S, 95 rules |
| What the product does in a school year | Appendix T |
| What makes it worth switching to | Appendix W and Appendix P |
| What proves any of it works | Appendix V |
| Whether it runs on your machine or your phone | Appendix X |

---

## What changed since v8

v8 was a strong brief with three files that quietly disagreed with each other and a build the documents never described. v9 fixes that and extends it.

- **Reconciled.** All three brief files are v9 and consistent. 25 contradictions inside the master brief, 20 across the files, and 13 drifts from the reference architecture were found and fixed. Every one is listed in `docs/project/KIT_V9_CHANGES.md`.
- **Specified the business.** 52 workflows as state machines with a test per transition, and 95 business rules with worked examples that become the unit tests. A year-in-the-life simulation proves nothing was forgotten.
- **Filled the gaps.** The master brief went from 27 to 40 sections, adding delivery, governance, budget, service levels, retention, compliance, operations topology, the public API contract, billing, mobile release, deliverability, support and risk. The appendices went from 8 to 24.
- **Made it provable.** Every requirement, rule and workflow maps to a test case identifier. `tools/kit-lint` enforces the links and has its own tests. It runs clean on this kit and reports v8's defects when pointed at v8.
- **Made it portable.** Windows, Linux, Android, iOS, mobile web and desktop kiosk, each with the test that proves the claim. One Node implementation per tool, with both shell wrappers.
- **Made it distinctive.** A north star, twenty more signature features, a differentiation matrix, and a fifteen-minute demo where every claim has a sixty-second proof.

---

## Notes

- `CLAUDE.md` is deliberately short. The brief is large, so `INDEX.md` and `READING_MAP.md` let a session read 30 KB instead of 790 KB.
- The plan takes several sessions. `docs/project/` keeps each session consistent with the last, and `/resume` reads it for you.
- The seeded administrator is `admin`, from configuration, with the safeguards in master brief Section 10.2. It is the platform super administrator only; a school's first administrator always arrives by single-use invitation. Override both values in production.
- Run `node tools/kit-lint/kit-lint.mjs .` after editing anything under `docs/`. A hook does it for you, and it never blocks.
