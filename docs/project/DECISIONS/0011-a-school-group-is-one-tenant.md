# ADR-0011: A school group is one tenant with several campuses

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-19
- **Requirement IDs:** REQ-DATA-001

## Context

The brief used two framings interchangeably. Section 9 described a group director with a cross-campus view, implying one tenant. Tier 2 described consolidated reporting across the schools of one owner, implying several tenants. The tenancy model cannot satisfy both.

## Decision

**A school group is one tenant with several campuses.** Group reporting is campus aggregation inside that tenant and needs no cross-tenant query.

An owner who insists on separate tenants gets consolidated reporting through the Reporting service group view, which is Tier 3, opt-in per tenant, and built on exports rather than on a cross-tenant join.

## Alternatives considered

- **Several tenants with a group layer.** Rejected: every query in every service would need a second scope above the tenant, and the isolation guarantee that the whole product rests on would become conditional.

## Consequences

- A group that later splits into independent schools needs a tenant split, which is an export and import per school. The tier-migration procedure in reference architecture Section 14 is the same machinery.
- Campus becomes a first-class scope in the permission model, which Appendix B already reflects.
