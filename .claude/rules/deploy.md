---
paths:
  - "deploy/**/*"
  - ".github/workflows/**/*"
  - "**/Dockerfile"
  - "**/Dockerfile.*"
  - "docs/plan/15-deployment-and-operations.md"
---

# Deployment rules

- Three deployment modes stay working at all times: single server with Docker Compose, Kubernetes, and the local Aspire development stack. A change that only works in one of them is incomplete.
- Images are named `nibras/<service>-<kind>` and tagged with an immutable version. Never deploy `latest`.
- **Base images ship ICU and time zone data.** An Alpine image adds `icu-libs` and `tzdata` explicitly. `InvariantGlobalization` is never enabled: this product sorts Arabic and formats two calendars.
- No secret in an image, a compose file, a manifest, or a pipeline log. Secrets come from the platform's secret store and are referenced, never inlined.
- Migrations are expand, migrate, contract, and safe under a rolling deployment. A migration that requires downtime is an ADR with a scheduled window.
- Every pipeline runs, and fails on: build, unit tests, integration tests, the license scan, the kit lint, the accessibility check, and the performance budget checks.
- Health, readiness, and startup probes on every service. Readiness means dependencies are reachable; health means the process is alive. They are not the same endpoint.
- Every alert named in the operations plan has a runbook file before the alert is enabled.
- Resource requests and limits on every workload. An unlimited workload starves the rest of the node at 08:00.
- Backups are verified by an actual single-tenant restore, timed, at least once per release. An untested backup is not a backup.
- Terraform is not permitted. Infrastructure definitions use the compliant tooling recorded in the dependency inventory.