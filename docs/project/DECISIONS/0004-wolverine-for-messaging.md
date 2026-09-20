# ADR-0004: Use Wolverine for the mediator, transport, outbox and sagas

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-19
- **Requirement IDs:** REQ-MSG-001

## Context

The product needs an in-process mediator, a RabbitMQ transport, a transactional outbox, an inbox for idempotent consumers, and saga persistence. MediatR, MassTransit v9 and NServiceBus are excluded by the licence policy in Section 6.1.

## Decision

Use Wolverine, which covers all five concerns under MIT. Keep it behind `Nibras.BuildingBlocks.Messaging` so that services depend on the building block and not on the library.

## Alternatives considered

- **Rebus plus a hand-written outbox.** Viable, more code to own, fewer features. Kept as the named alternative.
- **RabbitMQ.Client with a thin in-house bus.** Most control, most code, and the outbox and saga machinery would be ours to get right.

## Consequences

- A single library covers a lot of surface area, so risk 8 in the register owns the possibility that it does not scale. The mitigation is real: contract tests are written against the message shapes rather than the library, and the building-block boundary means a replacement touches one project.
