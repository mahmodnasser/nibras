# ADR-0027: A tenant is billed on students enrolled, prorated by day, as master brief Section 36 states

- **Status:** Accepted by the product owner, 2026-09-26 (Open Question 30, the recommended answer)
- **Date:** 2026-09-26
- **Requirement IDs:** REQ-PLT-009

## Context

Master brief Section 36 defines the student a school pays for: "A student counted on the billing date with a status of enrolled ... A student who joins mid-month is prorated by day from the enrollment date; a student who leaves is counted for the month in which they leave. This definition is in the contract, because every other definition produces an argument." REQ-PLT-009 restates it with a worked example.

Appendix S said three different things. BR-FIN-017 counted every student enrolled for at least one day of the month, with no proration, and made the count a Finance figure. BR-PLT-005 billed the maximum daily headcount of the month. BR-FIN-018 prorated a downgrade mid-cycle, where Section 36 says "A downgrade takes effect at the next renewal". Open Question 30 and RISK-52 (16) recorded the conflict; SL-PLT-010 in phase 1 could not start until it was settled. The product owner chose the recommended answer, Section 36, on 2026-09-26.

## Decision

**The billable active-student count is the Section 36 definition, computed by Platform.**

- BR-FIN-017 is rewritten to Section 36 with its computation, rounding and worked examples, and its owner becomes Platform, the service that bills tenants (REQ-PLT-009). Finance, which bills parents, does not compute it. The identifier is kept so that every citation stays valid.
- BR-PLT-005's active-student meter is the BR-FIN-017 count; the maximum-daily aggregate stays for the other headcount meters.
- BR-FIN-018's owner becomes Platform, and a downgrade takes effect at the next renewal with no proration, as Section 36 states; an upgrade still prorates by day from the day of the change.

**Brief change under this record (v9.7).** Appendix S: the index rows and bodies of BR-FIN-017 and BR-FIN-018, and BR-PLT-005's rule, examples and last edge case. All three brief files are bumped to v9.7.

## Alternatives considered

- **Keep BR-FIN-017, "any student enrolled for one day".** Rejected by the product owner. It charges a school for a whole month for a student who joined on the 30th, which is the argument Section 36 was written to prevent.
- **Renumber the two rules to BR-PLT.** Rejected. The identifiers are cited in documents 03, 20, 31 and 34 and in the sheets; renumbering changes nothing a builder does and breaks every citation.

## Consequences

- Open Question 30 is closed and RISK-52 closes with it.
- SL-PLT-010 builds the count and the meter on Platform; the Finance sheet's count job is removed, and the plan documents that cited the conflict (01, 03, 18, 31, 34, the Finance and Platform sheets) are aligned in the same change.
- The rule is the contract text, so a school can check its invoice by hand from the examples.
