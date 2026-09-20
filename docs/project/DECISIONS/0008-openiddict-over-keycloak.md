# ADR-0008: Use ASP.NET Core Identity with OpenIddict rather than Keycloak

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-19
- **Requirement IDs:** REQ-IDN-002

## Context

The Identity service needs a token server, two-factor authentication, passkeys, single sign-on, sessions and devices, invitations, join requests, delegation and access reviews. Both OpenIddict and Keycloak can host the protocol; they differ in where the user model and the join workflows live.

## Decision

Use ASP.NET Core Identity with OpenIddict, in-process in the Identity service. The join workflows, delegation and access reviews are product features with their own screens, and keeping them next to the user model avoids synchronising two sources of truth.

## Alternatives considered

- **Keycloak.** Strong for a customer whose identity provider is already central. It becomes a second user store to reconcile, and the join and delegation flows would straddle two systems. Remains the recommended option for an enterprise deployment, which is why it stays in the approved stack.

## Consequences

- SAML 2.0 and SCIM are not free with OpenIddict and are planned as Tier 2 protocol adapters. Open Question 18 tracks whether a first customer needs them sooner.
- Token signing and encryption keys become ours to rotate. Reference architecture Section 12 owns that, with an overlap window.
