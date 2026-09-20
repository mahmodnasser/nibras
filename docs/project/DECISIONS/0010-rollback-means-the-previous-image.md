# ADR-0010: Rollback means redeploying the previous image; the schema is never rolled back

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-19
- **Requirement IDs:** REQ-INF-001

## Context

Master brief Section 19 required forward-only migrations and Section 23 required a tested rollback. Read together they appeared to contradict each other, and neither said what rollback actually meant.

## Decision

**Rollback is redeploying the previous application image.** A schema migration is never reversed.

This is safe only because every migration is expand and contract: the expand step is deployed and must leave the previous image able to run, and the contract step ships in a later release once every replica is on the new version.

The pipeline enforces it with a test that runs the previous image against the new schema.

## Alternatives considered

- **Down migrations.** Rejected: a down migration that drops a column destroys data written since the deployment, and under a rolling deployment both versions are live at once.
- **Blue-green with two schemas.** Rejected as a default: the operational cost outweighs the benefit for the change sizes this product ships.

## Consequences

- Every schema change is two releases when it removes anything. That discipline is the price of being able to roll back an application in a minute.
- A migration that cannot be made backward compatible needs its own ADR and a maintenance window.
