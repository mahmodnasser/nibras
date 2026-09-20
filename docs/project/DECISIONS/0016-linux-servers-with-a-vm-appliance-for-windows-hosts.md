# ADR-0016: Servers are Linux only; a Windows host is served by a virtual machine appliance

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-19
- **Requirement IDs:** REQ-PLAT-003

## Context

Schools buying an on-premises installation often have a Windows server and no Linux administration. Several dependencies, Redis among them, have no supported Windows server build, so a native Windows installation would be an unsupported configuration sold as a supported one.

## Decision

All product images are Linux. On-premises on a Windows host is supported as a **Linux virtual machine appliance** for Hyper-V or VMware, built from `deploy/onprem/`, carrying the compose bundle, a backup volume and an upgrade script with a pre-upgrade check and a documented rollback.

Native Windows Server hosting is explicitly not supported, and the sales material says so.

## Alternatives considered

- **Support Windows Server natively.** Rejected: the dependency set makes it unsupportable, and discovering that during an incident at a school is the worst possible time.
- **Refuse Windows-host customers.** Rejected: the appliance costs little and opens a real segment.

## Consequences

- The appliance is an artefact to build, test and version. Its build and upgrade run in the release pipeline and in the quarterly drill, so it does not rot.
- Developers still work on Windows. That is a separate concern, covered by the setup guides and the dev-smoke job.
