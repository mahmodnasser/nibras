# Changelog

## v9.4 brief, 2026-09-26

Every signature feature runs its own demo test in the release gate (ADR-0023, proposed). Kit-lint R34 requires each of the 43 signature features to be shown by an Appendix O minute or reserve step whose Test cell runs the feature's Appendix W demo test; Appendix O's minutes 1, 2, 4, 5, 10, 11, 13 and 15 gained ten demo tests. Documents 15, 16 and 32 state the demo gate the same way: every step whose phase has shipped runs at each phase exit and release, through SL-TST-006. Document 32 recounted: 24 features shown in a minute, 18 only in a reserve step, feature 23 guarded by TC-UX-001, feature 39 moved to engineering.

## Plan risk honesty, 2026-09-25

Every open point is scored, and the serious ones are register risks (ADR-0022, proposed; no brief change).

**Kit-lint.** R33: every plan document and service sheet has an Open points section whose table carries likelihood, impact, score and register columns; a point that scores 12 or more names a RISK in document 18; document 12's threat tables name an owner and a register link. R24 checks the scales, the arithmetic and that every cited RISK exists. 47 self-tests.

**Plan.** About 300 open points scored across 57 documents; 12 documents gained an Open points section; document 12's 131 threat rows gained residual risk, owner role and register link. The register gained RISK-44 to RISK-53 and closed RISK-40; its heat map, top ten and owner tables were recomputed. The top risk (RISK-47, 20) is the Wellbeing check-in answer that waits on the device against the no-device rule, now Open Question 29; the billing-count conflict (RISK-52, 16) is Open Question 30.

## v9.3 brief, 2026-09-25

Every verification claim names a check that runs (ADR-0021, proposed). Of 167 verification rows that named a kit check, 67 were true; the rest named the wrong rule, a check nobody had built, or no one at all.

**Kit-lint.** Twelve new rules, each with a self-test: R21 requirements catalog, R22 ADR references and index, R23 generated documents current, R24 risk arithmetic, R25 roadmap coverage, R26 open-questions mirror, R27 messaging keys, R28 threat coverage, R29 state diagrams, R30 SQL comments, R31 canonical names, R32 workflow tests. R18 now requires a comment on every plan tree entry; R01 resolves reference-architecture sections against the reference architecture. 46 self-tests.

**Generators.** Every plan generator writes through `tools/plan-build/write-generated.cjs` and has a `--check` mode; regenerating no longer moves a review date when nothing else changed.

**Plan.** About 100 verification rows rewritten across 46 documents to name a real rule, a named review step (who, what, when) or the slice that builds a product check; document 03 gained its verification section; 37 tree entries gained comments; document 11 lists its 16 worker-job commands. **Brief.** One unlabelled transition in Appendix R (WF-FIN-04) labelled.

## v9.2 brief, 2026-09-25

Every test case is defined in exactly one document (ADR-0020, proposed). Kit-lint rule R20 refuses an identifier defined twice, a citation with no definition, and a derived acceptance test without its requirement; `docs/plan/16-annex-test-case-registry.md` lists all 1,566 tests and 334 derived acceptance tests with their owner, meaning and citations, generated from the same code.

**Brief.** Appendix W's twelve demo tests that reused Appendix R identifiers moved to the 810 range, with Appendices O and P following; Appendix O follows two plan-side renumberings; Appendix Q step 12 follows Appendix M on approvals.

**Plan.** 206 double definitions and 163 undefined citations resolved across 38 documents: restatements now name their owner, a handful of genuinely different tests were renumbered, and missing definitions were written. Document 10's pooled-connection tests no longer reuse the import workflow's identifiers.

## v9.1 brief, 2026-09-22

The brief was corrected from the defects the plan found while it was written, under ADR-0019. All three brief files are v9.1. Every change and its source is listed in `KIT_V9_1_CHANGES.md` (317 entries).

**Catalogs completed.** Appendix E gained the events, command, fields, bindings and scheduled jobs the service sheets need; Appendix B the missing permissions, granted in Appendix I through two new groups; Appendix K nine error codes; Appendix C thirteen notification rows. Appendix R now cites only catalogued names.

**Values settled by the product owner.** Tenant-deletion cooling-off 30 days; invitations 14 days with a reminder at day 7; notification deduplication 5 minutes; calendar-aware scaling moved from the signature list to engineering capabilities.

**Honesty fixes.** Section 28's phase ranges are derived from the work breakdown (phase 1 14 to 22 weeks, launch 61 to 93); k6 has its Section 6.4 row; rate limits and plan quotas return different statuses; Appendix W has its autonomy column; colliding test identifiers are renumbered; Appendix P's competitor facts are corrected.

**Lint.** Rule R03 now compares minor versions.

## v9 kit, 2026-09-19

The kit was reconciled, extended, and made self-checking. Every finding behind these changes is listed in `KIT_V9_CHANGES.md`.

**Reconciled.** All three brief files are v9 and agree with each other. Service names, ownership and identifier formats are fixed in Appendix L and quoted everywhere else rather than restated. The event dependency matrix was regenerated from the service sheets and now covers all twenty services; the eight publishers and eight consumers it silently omitted are back. Permission namespaces that were not services (`students`, `safety`, `admin`) were corrected to the services that own them. A ghost event was removed, two colliding entity names were renamed, and the settings ownership that three documents each claimed differently was settled on Platform.

**Extended.** The master brief grew from 27 to 40 sections, adding the delivery plan, governance and decision rights, the budget model, service levels, the retention schedule, the compliance map, operations topology, the public API and webhook contract, billing semantics, mobile release operations, deliverability and safeguarding operations, support tiers, and a risk register. It also gained a north star, an assist ladder, twenty more signature features, a coverage matrix, data-integrity jobs, a .NET 8 fallback list, and the container rules that make Arabic work in a Linux image.

The appendices grew from eight to twenty-four: role templates, data classification and retention, error codes, the canonical registry, offline conflict rules, load scenarios, the fifteen-minute demo, differentiation, acceptance scripts, 52 workflows, 95 business rules, a year-in-the-life simulation, persona journeys, the coverage matrix, the feature register, and the platform support matrix.

The reference architecture grew from 10 to 17 sections, adding continuous integration and delivery, secrets and rotation, backup and single-tenant restore and disaster recovery, the tenant isolation model with tier migration, environments, pinned versions, and the platform support matrix.

**Made self-checking.** `tools/kit-lint` carries 18 rules and 24 of its own tests, runs on Windows and Linux, and exits clean on this kit. It is what stops the documents drifting apart again. `tools/license-scan` is wired in with the standalone-tool allow-list before the first dependency exists. Both ship a PowerShell and a bash wrapper over one Node implementation.

**Made portable.** Windows, Linux, Android, iOS, mobile web and desktop kiosk each carry a named test. Line endings, path length, case collisions, container globalization and culture pinning became enforced rules rather than advice.
