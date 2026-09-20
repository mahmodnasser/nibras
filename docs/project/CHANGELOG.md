# Changelog

## v9 kit, 2026-09-19

The kit was reconciled, extended, and made self-checking. Every finding behind these changes is listed in `KIT_V9_CHANGES.md`.

**Reconciled.** All three brief files are v9 and agree with each other. Service names, ownership and identifier formats are fixed in Appendix L and quoted everywhere else rather than restated. The event dependency matrix was regenerated from the service sheets and now covers all twenty services; the eight publishers and eight consumers it silently omitted are back. Permission namespaces that were not services (`students`, `safety`, `admin`) were corrected to the services that own them. A ghost event was removed, two colliding entity names were renamed, and the settings ownership that three documents each claimed differently was settled on Platform.

**Extended.** The master brief grew from 27 to 40 sections, adding the delivery plan, governance and decision rights, the budget model, service levels, the retention schedule, the compliance map, operations topology, the public API and webhook contract, billing semantics, mobile release operations, deliverability and safeguarding operations, support tiers, and a risk register. It also gained a north star, an assist ladder, twenty more signature features, a coverage matrix, data-integrity jobs, a .NET 8 fallback list, and the container rules that make Arabic work in a Linux image.

The appendices grew from eight to twenty-four: role templates, data classification and retention, error codes, the canonical registry, offline conflict rules, load scenarios, the fifteen-minute demo, differentiation, acceptance scripts, 52 workflows, 95 business rules, a year-in-the-life simulation, persona journeys, the coverage matrix, the feature register, and the platform support matrix.

The reference architecture grew from 10 to 17 sections, adding continuous integration and delivery, secrets and rotation, backup and single-tenant restore and disaster recovery, the tenant isolation model with tier migration, environments, pinned versions, and the platform support matrix.

**Made self-checking.** `tools/kit-lint` carries 18 rules and 24 of its own tests, runs on Windows and Linux, and exits clean on this kit. It is what stops the documents drifting apart again. `tools/license-scan` is wired in with the standalone-tool allow-list before the first dependency exists. Both ship a PowerShell and a bash wrapper over one Node implementation.

**Made portable.** Windows, Linux, Android, iOS, mobile web and desktop kiosk each carry a named test. Line endings, path length, case collisions, container globalization and culture pinning became enforced rules rather than advice.
