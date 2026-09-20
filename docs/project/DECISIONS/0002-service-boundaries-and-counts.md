# ADR-0002: Fix the service catalog at twenty services, with Assessment and Behavior separate

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-19
- **Requirement IDs:** REQ-PLAT-001

## Context

Master brief Section 7.1 asked for "about 12 services for Tier 1 and 16 to 18 at full scope" while Section 7.2 listed 21 entries and the reference architecture specified 22 sheets. Three documents, three answers, and no way for a reader to know which was binding.

Separately, the master brief placed Assessment inside Academics and Behavior inside Student Wellbeing, while the reference architecture split both out and justified it in a note that was never an ADR.

## Decision

The catalog is fixed at **20 data-owning services**, 16 Tier 1 and 4 Tier 2, plus the Gateway and two backends-for-frontends: 23 deployable applications and 7 worker images. Appendix L is the registry and the only place these names and counts are defined.

**Assessment is separate from Academics** because mark entry and report-card batches have a different load profile from coursework, and their peak is the sharpest write burst in the product.

**Behavior is separate from Wellbeing** because behaviour records are parent-visible and wellbeing records are not. Merging them would force one sensitivity model onto two different audiences.

A smaller first release may merge Assessment into Academics and Behavior into Wellbeing and defer Hr, Operations and Ai, which is 14 services. Merging is a build-time choice: namespaces, routing keys and database names do not change, so a later split is a deployment change rather than a rewrite.

## Alternatives considered

- **Keep the estimate and let the catalog drift.** Rejected: it is what produced three different answers in the first place.
- **Merge Assessment and Behavior permanently.** Rejected: it couples two different scaling profiles and two different sensitivity models, and unpicking it later is a data migration rather than a deployment.
- **Split further, for example Operations into six.** Rejected: master brief Section 7.1 requires a written reason per service, and six sub-domains that deploy together are one service with six schemas.

## Consequences

- Twenty services is a real operational load for a small team. Risk 5 in the register owns it, and the mitigations are one template, one pipeline, one dashboard set and the merge option.
- Every document that names a service now quotes Appendix L. The lint enforces it, so the drift cannot return silently.
