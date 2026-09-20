# ADR-0009: Platform owns settings, terminology and custom-field definitions; services own the values

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-19
- **Requirement IDs:** REQ-PLT-002

## Context

Three documents each gave these a different owner. The reference architecture put Setting, CustomFieldDefinition and Terminology in Platform. Appendix F put CustomFieldDefinition in School. Appendix B put terminology and custom fields under the school namespace and settings under an admin namespace that was not a service.

## Decision

**Platform owns the definitions**: tenant settings, terminology overrides, and custom-field definitions, including their validation and their audit trail. It publishes settings.changed, terminology.changed and custom-field.changed.

**Each service owns the values** stored on its own entities. School stores a custom-field value on a student; Finance stores one on an invoice. No service stores another service values.

Permissions move to the platform namespace accordingly.

## Alternatives considered

- **School owns everything.** Rejected: settings apply to every service, and a non-academic tenant setting has no business living in the student information service.
- **Each service owns its own definitions.** Rejected: an administrator would configure the same terminology in twenty places.

## Consequences

- Every service consumes platform.custom-field.changed and keeps a local copy of the definitions it needs, which is one more reference copy to reconcile nightly.
- A definition change is eventually consistent across services, so the interface shows a brief processing state rather than pretending otherwise.
