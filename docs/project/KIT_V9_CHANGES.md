# Kit v9: every finding and where it was fixed

This is the audit trail for the v8 to v9 work. Each row is a defect or gap found by reading v8 end to end, and the place it was addressed. `KIT_LINT_BASELINE.md` holds the lint run against v8 that proves the automated rules detect what they claim to.

**Counts.** 25 contradictions inside the master brief, 20 across the three brief files, 13 drifts between the brief and the reference architecture, 37 production-brief gaps, 30 business and workflow gaps, 15 differentiation gaps, 14 cross-platform gaps, 12 kit mechanics gaps.

---

## A. Contradictions inside the master brief

| # | Finding | Fix |
|---|---|---|
| A1 | Section 7.1 said "about 12 services for Tier 1, 16 to 18 at full scope"; Section 7.2 listed 21 | Appendix L fixes the count at 20 services, 23 deployables, 7 workers. Section 7.1 rewritten. ADR-0002 |
| A2 | Decision 9 reopened the seeded administrator question that Section 10.2 had settled | Decision 9 struck through and settled. ADR-0003 |
| A3 | `Imports.Worker` had no owning service | Folded into `Documents.Worker` as a job group. Section 8 item 9 and Appendix L |
| A4 | Open standards, public API and webhooks had no owning service | Integrations capability inside Platform. Section 7.2, ADR-0012 |
| A5 | Tasks and notification centre had no owner | Requests owns `Task`; Notification owns inbox delivery. ADR-0012 |
| A6 | Forward-only migrations contradicted tested rollback | Rollback means redeploying the previous image; schema is never rolled back. ADR-0010 |
| A7 | Three Angular surfaces, two applications in the tree | Two applications, role workspaces as lazy areas. Section 19 |
| A8 | Mobile crash reporting was "GlitchTip or none" in one place and mandatory in another | Mandatory, opt-out per flavour |
| A9 | "100% open source" headline against two documented non-open exceptions | Subtitle corrected |
| A10 | Zero cross-tenant incidents stated as a measurable outcome | Restated as a control objective with its proof |
| A11 | ImageSharp described as commercial from v2 | Corrected to v3 and later; v2 is Apache-2.0 but unmaintained |
| A12 | MinIO described as discontinued and archived | Corrected to feature-reduced and repositioned |
| A13 | AGPL tools required a justification list that did not exist | New Section 6.4, mirrored in `tools/license-scan/allow.json` |
| A14 | Rate limiting assigned to three different layers with no split | Three layers, three purposes, stated in Section 19 |
| A15 | "No shared business logic" against shared authorization and tenancy blocks | Boundary rule: policy data yes, domain rules never. Architecture test forbids BuildingBlocks to Domain references |
| A16 | AI retrieval required permission filtering but the index had no lawful source | Index built from events and authorized reads, tagged with tenant and scope. Section 25 |
| A17 | Marks in progress never cached, but the grid must be keyboard-speed | Covering index plus batched writes. Section 19 |
| A18 | Mobile accessibility standard unnamed | WCAG 2.2 AA through Flutter semantics plus platform guidelines. Section 21 |
| A19 | Wellbeing "separate database" was weaker than it sounded | Separate database, role and encryption key; isolation level S |
| A20 | The default password is printed in a document that will be committed | Kept as a demo default, with the production seeder refusing it outside Development and the secret scan scoped to `docs/` |
| A21 | .NET 8 remained an option with no statement of what breaks | Fallback list in Section 19, rule by rule |
| A22 | Angular "v22 at the time of writing" | Replaced with a pin-at-start rule and an animation fallback |
| A23 | Phase 0 listed 14 deliverables against 22 plan documents, unmapped | Mapping table in Section 26 |
| A24 | Section 15 ended with "list any more you identify" | 14 further edge cases in Section 19 and Appendix X |
| A25 | Reporting freshness never stated although the UI depends on it | 60 seconds, in Section 31 |

## B. Contradictions across the three brief files

| # | Finding | Fix |
|---|---|---|
| B1 | Appendices were v7 inside a v8 set | All three are v9; lint rule R03 enforces equality |
| B2 | Settings, terminology and custom fields had three different owners | Platform owns definitions, services own values. ADR-0009 |
| B3 | Permission namespaces `students`, `safety`, `admin` were not services | Renamed to the owning services. Appendix B and L |
| B4 | The dependency matrix omitted 8 publishers and 8 consumers, contradicting 12 sheets | Regenerated from the sheets. Reference architecture Section 10 |
| B5 | Ghost event `report-card.generated` | Removed; Assessment consumes `documents.document.generated.v1` |
| B6 | `usage.recorded` listed as published by Platform, which consumes it | Cross-cutting events section; every service publishes under its own prefix |
| B7 | `settings.changed` relied on for cache invalidation but absent from the catalog | Added, with terminology and custom-field events |
| B8 | `Acknowledgment` named two different aggregates in two services | `AnnouncementAcknowledgment` and `BroadcastAcknowledgment` |
| B9 | `document.expiring` collided between HR and student documents | Qualified per service |
| B10 | Behavior and Wellbeing events were one bullet | Split, with the no-clinical-detail payload rule |
| B11 | Operations had no events at all, yet notifications depended on them | Full `operations.*` group |
| B12 | About 12 notification rows had no event or job to trigger them | Every row now names its trigger; 12 events and a job table added |
| B13 | `Sync deps` and `Local copies` were defined in the legend and populated almost nowhere | Section 8.0 cross-service table for all 20 services |
| B14 | Entity lists disagreed between Appendix F and the sheets | Reconciled |
| B15 | Four services had no entity list at all | Written |
| B16 | Tier 3 sub-modules had no owning service | Homes table in Appendix L |
| B17 | Service names differed between the brief and the reference architecture | One canonical table. ADR-0002 |
| B18 | `docs/plan/02-services/` referenced where `06-services/` was meant | Corrected |
| B19 | A directory tree in the reference architecture was unclosed | Corrected |
| B20 | Cache key example used a truncated tenant identifier | Full UUID, stated as a rule |

## C. Reference architecture drift

| # | Finding | Fix |
|---|---|---|
| C1 | No continuous integration or delivery anywhere | New Section 11, with migration ordering and promotion |
| C2 | No secrets, keys or rotation | New Section 12, with an inventory and cadences |
| C3 | No backup, restore or disaster recovery | New Section 13, including the single-tenant restore procedure |
| C4 | No tenant isolation model despite three commercial tiers | New Section 14, with the row-level security contract and tier migration |
| C5 | No environment inventory | New Section 15, including per-pull-request previews |
| C6 | No pinned versions | New Section 16 |
| C7 | No platform support statement | New Section 17 and Appendix X |
| C8 | No on-premises bundle despite the commitment to one | `deploy/onprem/` and ADR-0016 |
| C9 | Helm infrastructure list omitted pooling, operators, certificates and secrets | Section 6 completed |
| C10 | Observability containers named only by filename | Listed |
| C11 | Workers missing from the anatomy and topology | Added, and reconciled with Appendix L |
| C12 | Message envelope fields never listed | Listed in Section 3 and Appendix E |
| C13 | No service level or data classification per service | Both added to the Section 8.0 table |

## D. Production-brief gaps, now Sections 28 to 40

Timeline, team and governance, budget, service levels, retention schedule, compliance map, high availability and residency, public API and webhook contract, payment and billing semantics, mobile release operations, deliverability and safeguarding operations, support tiers, risk register. Each has defaults so nothing blocks. Also added: north star and principles (4.1), the assist ladder and autonomy scale (25), the coverage matrix (24), data integrity jobs, the .NET 8 fallback list, and container globalization rules (19).

## E. Business and workflow gaps

| Finding | Fix |
|---|---|
| 18 of roughly 50 school processes were named, none specified | Appendix R: 52 workflows as state machines, 312 test cases |
| No business rule catalog; arithmetic left to discovery | Appendix S: 95 rules with worked examples and named test classes |
| No way to know whether the product covers a school year | Appendix T: a year-in-the-life simulation, lint-enforced to exercise every workflow and rule |
| No acceptance criteria format, no test case identifiers | Appendix V and `TC-<AREA>-<NNN>`. ADR-0014 |
| No persona journeys beyond a success sentence | Appendix U |
| No offline conflict rules, explicitly deferred | Appendix M, per entity, with tests |
| No role templates despite being required in Phase 0 | Appendix I |
| No data classification or retention values | Appendix J and Section 32 |
| No error codes | Appendix K |
| No load scenarios or data tiers | Appendix N |
| No acceptance scripts | Appendix Q |
| Integrity of eventually consistent data never checked | Reconciliation jobs in Section 19 |

## F. Differentiation gaps

No north star, no differentiation matrix, no demo choreography, no smart defaults, no explainability pattern, intelligence not tiered. Fixed by Section 4.1, Appendix P, Appendix O, features 25 to 44 in Section 12.1, the Because panel and the assist ladder in Section 25, and Appendix W which records rung and tier per feature. ADR-0015.

## G. Cross-platform gaps

| Finding | Fix |
|---|---|
| Docker Desktop banned with no Windows developer path | `docs/dev-setup/windows.md`, Podman and WSL2 both documented |
| Kit scripts were bash only | One Node implementation with `.ps1` and `.sh` wrappers. ADR-0017, lint R15 |
| Hooks would not have run on Windows | All hooks invoke node with a relative path. Lint R16 |
| Linux containers lacked ICU and tzdata guidance for Arabic | Container rules in Section 19, with a culture test inside the image |
| No Windows on-premises path | Linux virtual machine appliance. ADR-0016 |
| iOS needs macOS with no cost line or owner | Budget line in Section 30, Open Question 14, risk 11 |
| No plan for devices without Google services | Push fallback in Section 37 and Appendix C |
| No mobile web or progressive web app parity | Stated in Appendix X and `08-web-structure.md` |
| No line ending, case or path length rules | `.gitattributes`, `.editorconfig`, lint R13 and R14 |
| Culture-sensitive parsing unaddressed | Culture pinning rule, tested under three cultures |
| Desktop kiosk targets unstated | Flutter Windows and Linux builds, Appendix X |
| No developer setup verification | `tools/dev-setup/verify-setup` with both wrappers and `/verify-setup` |
| No cross-platform test matrix | Appendix X section 2 |
| No cross-platform edge cases | Appendix X section 3 |

## H. Kit mechanics gaps

| Finding | Fix |
|---|---|
| No consistency checking of any kind | `tools/kit-lint`: 18 rules, 24 of its own tests, clean on this kit |
| No templates | 21 in `docs/templates/` |
| Commands had no output contract or stop conditions | All 35 commands restructured, with a reads-first list |
| No privacy, messaging, portability, rules or demo reviewers | 10 new agents, 15 in total |
| No reusable know-how | 17 skills |
| No path rules for contracts, tests, deployment, messaging or portability | 10 new rules, 14 in total |
| No ADRs for decisions the kit had already made | 16 new ADRs |
| No reading guide for 790 KB of brief | `INDEX.md` and `READING_MAP.md` with a reading budget |
| No product owner review guidance | `docs/plan/REVIEW_GUIDE.md` with a decisions workshop agenda and sign-off |
| No ideas backlog or retrospective loop | `IDEAS.md`, `/ideate`, `/retro` |
| Project memory files were stubs, one with a stray artefact | All rewritten |
| Licence scan referenced a script that did not exist | `tools/license-scan` with the allow-list |

---

## Found during the v9 work itself

Two defects were introduced and caught by the tooling rather than by review, which is the point of having it.

| Finding | Caught by | Fix |
|---|---|---|
| New events added to the dependency matrix were not in the event catalog | Lint rule R07 | Appendix E rewritten as a complete catalog |
| A parallel worker defined the assist ladder as an autonomy scale, conflicting with Section 25 | The worker reported it | Two named scales: assist rung for technology, autonomy level for how far it acts. Both now canonical. ADR-0015 |
| Hook scripts carried shebangs but no wrappers | Lint rule R15 | Shebangs removed; they are invoked as `node <file>`, never directly |
| The brief index collapsed sub-section numbers into their parent | Manual check of the generated output | Generator matches a sub-section before a top-level one |
| The first packaged archive used backslash path separators and would not extract on Linux | Extracting the archive and linting the copy | Repackaged with bsdtar; the check is now part of the release steps |
