# Brief Index

The three brief files are large. This index gives every section in one line so you can jump straight to what a task needs. `READING_MAP.md` says which sections each command requires.

**Never read a whole brief file to answer a question.** Find the section here, read it completely, and re-read when unsure.

| File | Size |
|---|---|
| `01-master-brief.md` | 156 KB |
| `02-appendices/` (25 files) | 579 KB |
| `03-reference-architecture.md` | 54 KB |

---

## 01-master-brief.md

| Section | Covers |
|---|---|
| **1. Document Conventions and Glossary** | Requirement keywords, tiers, precedence, identifier formats, glossary |
| **2. Your Role** | The role the reader is asked to play |
| **3. Project Parameters** | The pinned stack and every headline parameter |
| **4. Objective and Product Vision** | Vision, the three success tests, measurable outcomes |
| **4.1 North Star and Product Principles** | North star and the six product principles |
| **5. Scope, Assumptions, Constraints, and Personas** | Scope, what is deliberately out, assumptions, constraints, personas |
| **6. Open-Source-Only Policy** | Licence rules, the approved stack, unavoidable costs, the standalone AGPL list |
| **7. Microservices Architecture** | Microservice principles, the service catalog, communication, tenancy, authorization, resilience, deployment modes |
| **8. RabbitMQ Messaging Design** | RabbitMQ topology, outbox and inbox, lanes, fairness, ordering, envelope, workers |
| **9. User Roles and Authorization** | Roles and the authorization model |
| **10. Admin Console and Access Management** | The two consoles, first run, user management, joining, roles interface, what an administrator controls |
| **11. Request Center and Approval Engine** | The Request Center engine and the request catalog |
| **12. Signature Features: What Makes This Product Special** | The 24 original signature features |
| **13. Functional Scope** | Functional scope by tier |
| **14. End-to-End Workflows** | The end-to-end workflows |
| **15. Edge Cases You Must Handle** | Edge cases that must be handled |
| **16. Design, Motion, and User Experience** | Brand, design language, motion, experience principles |
| **17. Localization and Regional Fit** | Localization and regional fit |
| **18. Mobile Application Requirements** | Mobile requirements |
| **19. Architecture and Engineering Standards** | Engineering standards, performance, caching, integrity, runtime fallbacks, container rules |
| **20. Security, Privacy, and Child Safety** | Security, privacy, child safety |
| **21. Non-Functional Requirements** | Non-functional requirements and scale targets |
| **22. SaaS Commercial Layer and Customer Success** | The commercial layer and customer success |
| **23. Operations and Release Management** | Operations and release management |
| **24. Test Strategy and Quality Gates** | Test strategy, quality gates, the coverage matrix |
| **25. AI Principles** | AI principles and the assist ladder |
| **26. Working Method** | Working method, project memory, response protocol, definition of done |
| **27. Decisions to Confirm Before Starting** | Decisions to confirm before starting |
| **28. Delivery Plan** | Delivery plan, phases, the MVP cut line |
| **29. Team, Governance, and Decision Rights** | Team, governance, decision rights |
| **30. Budget and Cost Model** | Budget and cost model |
| **31. Service Levels** | Service levels, error budgets, severities |
| **32. Retention and Lifecycle Schedule** | Retention and lifecycle schedule |
| **33. Compliance Map** | Compliance map |
| **34. Platform Operations and Topology** | Platform operations, high availability, residency, environments, capacity |
| **35. Public API, Webhooks, and Integrations** | Public API, webhooks, integrations, deprecation |
| **36. Payments and Billing Semantics** | Payments and billing semantics |
| **37. Mobile Release Operations** | Mobile release operations |
| **38. Deliverability and Safeguarding Operations** | Deliverability and safeguarding operations |
| **39. Training, Onboarding, and Support** | Training, onboarding, support tiers |
| **40. Risk Register** | Risk register |

---

## 02-appendices/

Full list with one line each: `02-appendices/00-index.md`. Appendix **L** is the canonical registry and is the one to read first.

| File | Lines |
|---|---|
| `00-index.md` | 69 |
| `appendix-a-feature-catalog.md` | 229 |
| `appendix-b-permissions.md` | 284 |
| `appendix-c-notifications.md` | 103 |
| `appendix-d-dashboards-and-reports.md` | 25 |
| `appendix-e-event-catalog.md` | 325 |
| `appendix-f-entities.md` | 23 |
| `appendix-g-settings.md` | 22 |
| `appendix-h-demo-data.md` | 18 |
| `appendix-i-role-templates.md` | 184 |
| `appendix-j-data-classification-and-retention.md` | 167 |
| `appendix-k-error-codes.md` | 323 |
| `appendix-l-registry-and-id-codes.md` | 127 |
| `appendix-m-offline-conflict-rules.md` | 92 |
| `appendix-n-load-scenarios.md` | 179 |
| `appendix-o-demo-script.md` | 68 |
| `appendix-p-differentiation.md` | 95 |
| `appendix-q-uat-scripts.md` | 199 |
| `appendix-r-workflow-catalog.md` | 2063 |
| `appendix-s-business-rules.md` | 2060 |
| `appendix-t-year-in-the-life.md` | 307 |
| `appendix-u-persona-journeys.md` | 226 |
| `appendix-v-coverage-matrix.md` | 125 |
| `appendix-w-feature-register.md` | 81 |
| `appendix-x-platform-support.md` | 95 |

---

## 03-reference-architecture.md

| Section | Covers |
|---|---|
| **1. Repository Structure** | The repository tree |
| **2. Anatomy of a Service** | The anatomy of a service, with Attendance as the worked example |
| **3. Building Blocks and Contracts** | Building blocks and the contracts layout |
| **4. Web Structure (Angular)** | The Angular workspace |
| **5. Mobile Structure (Flutter)** | The Flutter project |
| **6. Deployment Structure** | Compose, Helm, OpenTofu, GitOps, on-premises |
| **7. Naming Conventions** | Naming conventions |
| **8. Service Specification Sheets** | A specification sheet per service, and the cross-service table |
| **9. Reference Diagrams** | Topology, sequence and saga diagrams |
| **10. Event Dependency Matrix** | The event dependency matrix |
| **11. Continuous Integration and Delivery** | Continuous integration and delivery |
| **12. Secrets, Keys, and Rotation** | Secrets, keys and rotation |
| **13. Backup, Restore, and Disaster Recovery** | Backup, single-tenant restore, disaster recovery |
| **14. Tenant Isolation Model and Tier Migration** | The tenant isolation model and tier migration |
| **15. Environments** | Environments |
| **16. Pinned Technology Versions** | Pinned technology versions |
| **17. Platform Support Matrix** | The platform support matrix |
