# ADR-0025: Master brief Section 28's ranges are re-derived after remediation round 5

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-26
- **Requirement IDs:** REQ-TST-025

## Context

Master brief Section 28 states that the phase ranges "are derived from the slice-days in `docs/plan/34-work-breakdown.md` by `tools/plan-build/schedule-34.mjs` ... not estimated separately", and that they "are recomputed, never adjusted by hand". ADR-0019 set them from 718 slices and 1,705 slice-days.

The round-4 scorecard (`docs/plan/30-plan-scorecard.md`) blocked Group F because SL-ACA-207 built the LTI 1.3 launch in phase 2, before the registration, login and keys that SL-INT-411 builds in phase 4. It also found that SL-FIN-444, a 3-day slice, carried the first e-invoicing plug-in with no estimate. Remediation round 5 split the launch into SL-ACA-405 under CAP-INT-02 (phase 4) and added the conditional e-invoicing slices SL-FIN-448 to SL-FIN-452 (phase 3) for the default countries of Open Question 3. Document 34 now holds 79 capabilities, 724 slices and 1,721 slice-days, and `schedule-34.mjs` gives phase 3 11 to 18 weeks, phase 4 7 to 12 weeks and a total of 61 to 95 weeks. The brief still said 11 to 17, 7 to 11 and 61 to 93, so the brief and the plan disagreed.

## Decision

**The brief quotes the ranges the generator produces from document 34 as it stands.** Section 28's phase 3 range becomes 11 to 18 weeks, phase 4 becomes 7 to 12 weeks, and the total from the start of phase 1 to launch becomes 61 to 95 weeks. Phases 1, 2, 5 and 6 and the MVP cut line (42 capabilities, 33 to 50 weeks) are unchanged.

**Brief change under this record (v9.5).** Master brief Section 28: the phase 3 and phase 4 Range cells and the total sentence. All three brief files are bumped to v9.5; no other brief text changes.

## Alternatives considered

- **Keep the old ranges in the brief until phase 0 closes.** Rejected. The brief says the ranges are derived, never adjusted; a brief that quotes a figure the generator no longer produces is the defect RISK-43 describes.
- **Absorb the new slices into the overhead factor.** Rejected. It is an adjustment by hand, and it hides the e-invoicing work that Open Question 3 makes conditional.

## Consequences

- `docs/plan/17-roadmap.md` Section 1, `docs/plan/00-executive-summary.md` and `docs/project/PROJECT_STATE.md` quote 61 to 95 weeks.
- If Open Question 3 removes a country, its conditional slices leave document 34 and the ranges are recomputed again under a new record.
- Revisit when document 34's slice-days change by any amount that moves a range.
