# ADR-0015: Every intelligent feature declares an assist rung and an autonomy level, and explains itself

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-19
- **Requirement IDs:** REQ-AI-001

## Context

The brief treated AI as one capability that is on or off. Real features differ in what technology they need and in how far they act alone, and conflating the two produces a product that stops working when a model is unavailable.

## Decision

**Assist rung** says what technology a feature needs: 1 deterministic rules, 2 a classical model, 3 a local language model, 4 an optional external provider. The product must be complete and sellable at rung 1, and every feature above it names what it degrades to.

**Autonomy level** says how far it acts alone: 1 surfaces, 2 suggests, 3 drafts, 4 acts. Autonomy 4 is never permitted over a grade, a payment, or a message to a family.

**The Because panel** is required on every automated decision a person can see or is affected by: the reasons in plain language, the rung, and an override that records who and why.

Appendix W records both numbers per feature and the lint enforces their presence.

## Alternatives considered

- **One combined score.** Rejected: it hides the risky combination of a simple rule acting by itself.
- **No explanation requirement.** Rejected: master brief Section 4 makes explainability a product test, not a nicety.

## Consequences

- Every feature that automates anything carries interface work for the Because panel. That is the point: it is what makes the automation acceptable in a school.
