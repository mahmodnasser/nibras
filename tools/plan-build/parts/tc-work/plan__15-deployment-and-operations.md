# Test-case work for `docs/plan/15-deployment-and-operations.md`

## Collisions to resolve here (the owner keeps the identifier)

- **TC-INF-025** at line 516: "Real-event path rehearsal | Yearly | WF-INF-03 with the real-event flag against staging, traffic switched and switched back | Staging | Architect"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 2061: "Verified to TrafficSwitched | Real-event flag set and verification passed | Traffic moved, tenants notified, status page updated"
- **TC-DATA-020** at line 514: "Single-tenant restore under load | Quarterly | As above on the load tier with 49 busy tenants; p95 of the others must not move by more than 10% | Test, load tier | Platform engineer"
  - owner `docs/plan/10-data-architecture.md` line 600: "Reference architecture Section 13; WF-INF-03 | Single-tenant restore without touching others | Quarterly drill passes with the ordering in Section 9"

## Tests cited but defined nowhere: define them here

- **TC-INF-108**
  - cited `docs/plan/03-requirements-catalog.md` line 1012: "REQ-INF-002 | A single-server Docker Compose profile runs all services on one machine, so a small school never needs a Kubernetes cluster | 1 | cross-cutting | Master brief Section 7.7; Section 34 | n"
  - cited `docs/plan/15-deployment-and-operations.md` line 816: "Master brief Section 7.7; reference architecture Section 6 | Three modes, one tree | Each mode starts from the documented command and passes `healthcheck.sh`"
  - cited `docs/plan/15-deployment-and-operations.md` line 885: "All three modes start and report ready | Every pull request; nightly"
  - cited `docs/plan/20-traceability-matrix.md` line 825: "REQ-INF-002 | A single-server Docker Compose profile runs all services on one machine, so a small school never needs a Ku... | 1 | cross-cutting | none | none | 15 | 1 | SL-PLAT-004 | any | Planned"
- **TC-INF-114**
  - cited `docs/plan/03-requirements-catalog.md` line 1018: "REQ-INF-008 | Per-service pipelines trigger on path filters and run build, test, lint, format, architecture tests, licence scan, vulnerability, container and secret scans in a fixed order | 1 | cross-"
  - cited `docs/plan/15-deployment-and-operations.md` line 886: "Every `ci-service.yml` stage fails on its condition | Once per release of the pipeline"
  - cited `docs/plan/20-traceability-matrix.md` line 831: "REQ-INF-008 | Per-service pipelines trigger on path filters and run build, test, lint, format, architecture tests, licenc... | 1 | cross-cutting | none | none | 15 | 1 | SL-TST-001 | any | Planned"
- **TC-INF-101**
  - cited `docs/plan/03-requirements-catalog.md` line 1019: "REQ-INF-009 | An image is built once and promoted by tag through test, staging and production, never rebuilt | 1 | cross-cutting | Master brief Section 23 | none"
  - cited `docs/plan/15-deployment-and-operations.md` line 289: "Built once, promoted by tag | The image digest built on merge is the digest that reaches production; `release.yml` re-tags, never rebuilds"
  - cited `docs/plan/15-deployment-and-operations.md` line 817: "Reference architecture Section 11 | Stages, ordering, promotion by tag, supply chain | Every stage in part 4.1 present and failing on its condition; digest equality across environments"
  - cited `docs/plan/15-deployment-and-operations.md` line 861: "`16-test-strategy.md`, which registers them | Group E review"
- **TC-INF-103**
  - cited `docs/plan/03-requirements-catalog.md` line 1020: "REQ-INF-010 | The cluster admission controller refuses an unsigned image or one signed by an unknown key | 1 | cross-cutting | Master brief Section 20 | none"
  - cited `docs/plan/15-deployment-and-operations.md` line 291: "Signature | cosign; the admission controller refuses an unsigned image or one signed by an unknown key"
  - cited `docs/plan/15-deployment-and-operations.md` line 817: "Reference architecture Section 11 | Stages, ordering, promotion by tag, supply chain | Every stage in part 4.1 present and failing on its condition; digest equality across environments"
  - cited `docs/plan/15-deployment-and-operations.md` line 887: "Built once, promoted by tag, signed, with an SBOM | Every release"
- **TC-INF-104**
  - cited `docs/plan/03-requirements-catalog.md` line 1027: "REQ-INF-017 | Every service emits OpenTelemetry, and the operators have a service map, per-service SLO dashboards, centralized logs and alerts | 1 | cross-cutting | Master brief Section 7.6 | none"
  - cited `docs/plan/15-deployment-and-operations.md` line 392: "Review | A dashboard change is a pull request with a screenshot"
  - cited `docs/plan/20-traceability-matrix.md` line 840: "REQ-INF-017 | Every service emits OpenTelemetry, and the operators have a service map, per-service SLO dashboards, centra... | 1 | cross-cutting | none | none | 15 | 1 | SL-INF-001, SL-INF-007 | any |"
- **TC-INF-102**
  - cited `docs/plan/15-deployment-and-operations.md` line 290: "SBOM | CycloneDX per image, attached as an OCI referrer, kept for the life of the image"
  - cited `docs/plan/15-deployment-and-operations.md` line 817: "Reference architecture Section 11 | Stages, ordering, promotion by tag, supply chain | Every stage in part 4.1 present and failing on its condition; digest equality across environments"
- **TC-INF-105**
  - cited `docs/plan/15-deployment-and-operations.md` line 543: "Contents | Operating system, container engine, `deploy/compose/` with the single-server and edge files, `deploy/onprem/upgrade/`, `deploy/onprem/firstboot/`, the observability profile off by default, "
  - cited `docs/plan/15-deployment-and-operations.md` line 826: "Appendix X; ADR-0016; WF-INF-01 | Appliance build, upgrade, rollback, drill | Quarterly Hyper-V drill recorded"
  - cited `docs/plan/15-deployment-and-operations.md` line 897: "Appliance builds, upgrades, rolls back, restores | Per release; quarterly"
  - cited `docs/plan/34-work-breakdown.md` line 1267: "SL-INF-608 | A release manager gets, from every release, a Hyper-V `.vhdx` and a VMware `.ova` appliance built by `release.yml` in a nested-virtualisation job: a minimal Debian guest with system, data"
- **TC-INF-106**
  - cited `docs/plan/15-deployment-and-operations.md` line 545: "Offline bundle | An upgrade is a versioned bundle: images as archives, the migration bundle, checksums, SBOMs, release notes in both languages, and the supported upgrade path (from which versions) | `"
  - cited `docs/plan/15-deployment-and-operations.md` line 826: "Appendix X; ADR-0016; WF-INF-01 | Appliance build, upgrade, rollback, drill | Quarterly Hyper-V drill recorded"
  - cited `docs/plan/15-deployment-and-operations.md` line 897: "Appliance builds, upgrades, rolls back, restores | Per release; quarterly"
  - cited `docs/plan/34-work-breakdown.md` line 1269: "SL-INF-610 | An operator applies a versioned offline upgrade bundle with no network access, and the pre-upgrade check refuses to start unless the version path, the absence of irreversible migrations, "
