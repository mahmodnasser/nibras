# Nibras: Appendices v9.1

**Status: normative.** Every item in these appendices is a requirement unless marked *(T2)* for Tier 2 or *(T3)* for Tier 3. Unmarked items are Tier 1. Read together with `01-master-brief.md` and `03-reference-architecture.md`, which carry the same version. If an appendix and the master brief conflict, the master brief wins **and you must report the conflict** rather than picking one quietly.

In v8 the appendices were one file, one version behind the set they claimed to be normative with, and half of what a build needs was missing. They are now one file per appendix, versioned with the rest, and extended from eight appendices to twenty-four. ADR-0013 records the split.

---

## Contents

| # | Appendix | File | What it settles |
|---|---|---|---|
| A | Feature Catalog per Module | `appendix-a-feature-catalog.md` | What every module does, with its business rules, across 24 modules |
| B | Permission Catalog | `appendix-b-permissions.md` | Every permission, its risk level and its data scopes. Generates the permission test suite |
| C | Notification Matrix | `appendix-c-notifications.md` | Every notification, **its trigger**, recipients, urgency and channels |
| D | Dashboards, Reports, and KPIs per Role | `appendix-d-dashboards-and-reports.md` | What each role sees and the report library |
| E | Integration Event Catalog | `appendix-e-event-catalog.md` | Every event with publisher, consumers, partition key and payload. Complete, not a seed |
| F | Core Entities per Service | `appendix-f-entities.md` | The aggregates and entities each service owns |
| G | Settings Catalog | `appendix-g-settings.md` | Every configurable setting, its scope and its default |
| H | Demo Data Specification | `appendix-h-demo-data.md` | The deterministic demo tenants, and the load and scale tiers |
| I | Default Role Templates | `appendix-i-role-templates.md` | The built-in roles as a complete permission matrix |
| J | Data Classification and Retention | `appendix-j-data-classification-and-retention.md` | Field-level classification, encryption, cacheability and retention |
| K | Error Code Catalog | `appendix-k-error-codes.md` | Stable error codes per service, and what a client should do with each |
| L | **Canonical Registry and ID Codes** | `appendix-l-registry-and-id-codes.md` | **Service names, area codes, identifier formats, ownership decisions. Nothing redefines this** |
| M | Offline Behaviour and Conflict Rules | `appendix-m-offline-conflict-rules.md` | What works offline, the sync contract, and the conflict rule per entity |
| N | Load and Soak Scenarios | `appendix-n-load-scenarios.md` | The k6 scenarios that prove the scale targets, and the data tiers |
| O | The Fifteen-Minute Demo | `appendix-o-demo-script.md` | The demo, the golden path, and the release gate, minute by minute |
| P | Differentiation and Competitive Gap Seed | `appendix-p-differentiation.md` | The field, where this product differs, and what it lacks |
| Q | UAT Scripts per Role | `appendix-q-uat-scripts.md` | Plain-language acceptance scripts a school person can run |
| R | **Workflow Catalog** | `appendix-r-workflow-catalog.md` | **52 business processes as state machines, each with a test per transition** |
| S | **Business Rules Catalog** | `appendix-s-business-rules.md` | **95 rules with worked examples that become the unit tests** |
| T | Year in the Life | `appendix-t-year-in-the-life.md` | A month-by-month simulation that exercises every workflow and rule |
| U | Persona Journeys | `appendix-u-persona-journeys.md` | Day, week and year for each persona, and the moment that wins them |
| V | Coverage Matrix and Test Case Format | `appendix-v-coverage-matrix.md` | What proves what, and how a test case is written |
| W | Signature Feature Register | `appendix-w-feature-register.md` | All 44 signature features with their persona moment, assist rung and tier |
| X | Platform Support and Test Matrix | `appendix-x-platform-support.md` | Windows, Linux, Android, iOS, mobile web, kiosk, and the test for each claim |

---

## How the appendices relate to the plan

| Appendix | Feeds |
|---|---|
| A, D | `03-requirements-catalog.md`, the service sheets |
| B, I, J | `12-security-privacy-safety.md`, the generated permission suite |
| C, E | `11-messaging-architecture.md`, the service sheets |
| F | `10-data-architecture.md`, the service sheets |
| G | Every service sheet's settings section |
| H, N, V | `16-test-strategy.md` |
| K | `22-api-conventions-and-error-catalog.md` |
| L | `05-service-catalog.md`, `07-solution-structure.md`, and every identifier everywhere |
| M | `09-mobile-structure.md` |
| O, P, W | `32-product-differentiation-and-demo.md`, `02-competitive-gap-analysis.md` |
| Q | `16-test-strategy.md`, the acceptance scripts |
| R, S | `13-workflows-and-sagas.md`, `31-business-rules-and-workflows.md` |
| T, U | `17-roadmap.md`, `14-design-system-and-ux.md` |
| X | `33-platform-support-and-dev-environments.md`, `15-deployment-and-operations.md` |

---

## Reading order for someone new

1. **L** first, always. Ten minutes, and every name in every other document makes sense.
2. **A** for what the product does, skimmed by module heading.
3. **R** and **S** for how it actually behaves. These two are where the product stops being a list of features.
4. **B** and **J** before touching anything that reads a student record.
5. **O** to see the whole product in fifteen minutes.
6. The rest on demand, guided by `docs/brief/READING_MAP.md`.
