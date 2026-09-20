# ADR-0012: The public API, webhooks and standards belong to the Platform service; Requests owns tasks

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-19
- **Requirement IDs:** REQ-INT-001

## Context

Master brief Section 12 promised a public API, outgoing webhooks, iCal feeds and 1EdTech standards, and Section 10.6 gave the school administrator integrations, API keys and webhooks to manage. No service in the catalog owned any of it. Separately, the Tier 1 tasks and notification centre had no owner either.

## Decision

**Platform gains an Integrations capability**: public API keys and personal access tokens, outgoing webhook endpoints with signing secrets and a delivery log, the developer portal, OneRoster export and the LTI platform role.

Scheduling publishes iCal feeds and Platform exposes the subscription URL. Behavior owns Open Badges. Academics owns QTI.

**Requests owns the Task aggregate**, because a task is almost always the tail of a workflow. Notification owns delivery of the unified inbox.

## Alternatives considered

- **A separate Integrations service.** Rejected for now: it would own no data of its own beyond keys and endpoints, and Section 7.1 requires a written reason for a new service. Revisit if the developer portal grows a product of its own.

## Consequences

- Platform becomes the busiest Tier 1 service by surface area. Its sheet reflects that, and the load is administrative rather than transactional.
- Standards certification has fees, now recorded in master brief Section 30.
