# Reading Map

The brief set is around 790 KB. No task needs all of it. This map says what each command must read before it acts, so a session loads roughly 20 to 40 KB instead of everything.

**How to use it.** Find your command. Read the "Always" column completely, not by skimming. Read the "If it applies" column when the condition is true. Use `INDEX.md` to locate a section, and re-read rather than remember.

**Two documents are worth reading once, in full, before anything else:** Appendix L, the canonical registry, which is short and makes every other name make sense; and `docs/project/PROJECT_STATE.md`, which says where the work actually stands.

---

## Planning

| Command | Always | If it applies |
|---|---|---|
| `/plan-platform` | `PLAN_SPEC.md`, Appendix L, master brief 1, 4, 4.1, 7, 26, 28, and the sections for the group being written | The whole of a brief file only when writing Group B |
| `/plan-service <service>` | Appendix L, that service's sheet in reference architecture 8, its modules in Appendix A, its rows in B, C, E, F, G, K, its workflows in R and rules in S | Appendix J when it holds sensitive data; Appendix M when it works offline |
| `/plan-review <group>` | `PLAN_SPEC.md` scorecard, the documents in that group, Appendix L | The appendices those documents draw from |
| `/score-plan <group>` | `PLAN_SPEC.md` scorecard, the documents being scored | |
| `/lint-plan` | Nothing. It reads the files itself and reports | |
| `/plan-status` | `PROJECT_STATE.md`, `TRACEABILITY.md`, `RISKS.md`, `OPEN_QUESTIONS.md` | |
| `/decide <topic>` | `docs/templates/adr.md`, `OPEN_QUESTIONS.md`, the sections the decision touches | Existing ADRs that it supersedes |
| `/ideate <area>` | Appendix W, Appendix P, master brief 4.1 and 25 | Appendix A for the area |
| `/differentiate <feature>` | Appendix P, Appendix W, Appendix O | |
| `/retro <group>` | `PROJECT_STATE.md`, the group's documents, `IDEAS.md` | |

## Building

| Command | Always | If it applies |
|---|---|---|
| `/build-foundation` | Master brief 26, reference architecture 1, 2, 3, 11, 12, `07-solution-structure.md`, `17-roadmap.md` | Reference architecture 6 and 13 when touching deployment |
| `/build-service <service>` | That service's plan sheet, Appendix L, master brief 19 and 20, its rules in S and workflows in R, its permissions in B, its events in E, its error codes in K | Appendix J and master brief 32 when it holds sensitive data |
| `/build-web <feature>` | `08-web-structure.md`, master brief 16, 19 web rules, Appendix U for the persona, the screens in Appendix A | Appendix Q for the acceptance script |
| `/build-mobile <feature>` | `09-mobile-structure.md`, master brief 18, Appendix M, Appendix X mobile rows | Appendix U for the persona |
| `/fix-bug` | The failing area's rules in S and workflows in R | The service sheet |
| `/change-request` | Appendix L, the affected service sheets, `20-traceability-matrix.md` | |
| `/migrate-legacy <source>` | Master brief 23 and 39, `26-migration-and-onboarding-toolkit.md`, Appendix H | |
| `/onboard-tenant` | Master brief 7.4, 10.4, 39, workflow WF-PLT-01, Appendix G | |

## Checking

| Command | Always | If it applies |
|---|---|---|
| `/review-architecture` | Master brief 7, 8, 19, reference architecture 2, 3, 8, 10 | The service sheets under review |
| `/audit-security <scope>` | Master brief 20, Appendix B, Appendix J, the threat model for the service | Appendix I for role coverage |
| `/audit-privacy <scope>` | Master brief 20 and 32, Appendix J, Appendix L section on Wellbeing | Master brief 33 for the country |
| `/audit-messaging <service>` | Master brief 8, Appendix E, reference architecture 3 and 10 | The service sheet |
| `/audit-licenses` | Master brief 6, including 6.4, `tools/license-scan/allow.json` | |
| `/audit-ux <scope>` | Master brief 16, Appendix U, Appendix D | Appendix O when a signature feature is involved |
| `/audit-a11y-rtl <scope>` | Master brief 16, 17, 21, Appendix X browser and screen-reader rows | |
| `/audit-portability <scope>` | Appendix X, master brief 19 container rules, `.claude/rules/portability.md` | |
| `/threat-model <service>` | Master brief 20, the service sheet, Appendix J | |
| `/verify-performance <scope>` | Master brief 19 budgets and 21 targets, Appendix N | The service caching table and hot queries |
| `/optimize-service <service>` | Master brief 19, the service sheet, `21-performance-engineering.md` | |
| `/simulate-year [month]` | Appendix T for the month, plus the workflows and rules it names | |
| `/demo-script [role]` | Appendix O, Appendix H, Appendix W | |
| `/runbook <alert>` | Master brief 31, reference architecture 13, `docs/templates/runbook.md` | |
| `/release-ready <version>` | Master brief 24 and its coverage matrix, Appendix V, Appendix O | Reference architecture 13 for the restore evidence |
| `/verify-setup` | `docs/dev-setup/` for the current operating system, Appendix X | |
| `/resume` | `PROJECT_STATE.md`, `OPEN_QUESTIONS.md`, `RISKS.md`, the last `CHANGELOG.md` entry | |

---

## Reading budget

| Task shape | Target |
|---|---|
| One service sheet | Under 40 KB |
| One audit | Under 30 KB |
| One plan group | Under 60 KB, read across the session rather than at once |
| A status or lint command | Under 10 KB |

If a task seems to need more than this, it is probably two tasks. Split it, because a session that has read 200 KB has stopped reading carefully.
