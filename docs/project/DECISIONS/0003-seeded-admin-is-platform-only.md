# ADR-0003: The seeded administrator is the platform super administrator only

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-19
- **Requirement IDs:** REQ-IDN-001

## Context

Master brief Section 10.2 already stated that a new tenant never receives a shared password and that its first school administrator arrives through a single-use invitation. Section 27 then reopened the same question as decision 9, offering a per-tenant seeded account as an option.

A settled rule and an open question about that rule cannot both be normative.

## Decision

One seeded account exists, at platform scope, with every safeguard in Section 10.2. No tenant ever receives a seeded account or a shared password. A tenant administrator always arrives by single-use invitation.

The seeder additionally refuses to start when the configured password still equals the documented default and the environment is not Development.

## Alternatives considered

- **A seeded account per tenant.** Rejected: a known username and password per tenant is a standing credential-stuffing target across every customer at once, and it removes the invitation trail that proves who was granted administrator access.

## Consequences

- Provisioning must deliver the invitation reliably, because there is no fallback credential. The provisioning saga therefore treats invitation delivery as a step that can fail and compensate, not as a side effect.
- A tenant that loses every administrator is recovered by the platform operator through the audited break-glass path, not by a seeded login.
