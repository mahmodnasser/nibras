# ADR-0022: Every open point is scored, and the serious ones are register risks

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-25
- **Requirement IDs:** REQ-TST-025

## Context

The plan template gave every document an "Open points" table of question, default, owner and impact. The scorecard (`docs/plan/30-plan-scorecard.md`, theme 7) scored risk honesty at 3 in every group, for the same reasons each time:

- open points named an impact but never a likelihood, so nothing distinguished a remote worry from a default that is probably wrong;
- nothing linked an open point to the risk register (document 18), so a serious point could sit in a footnote while the register missed it, and the register's top ten could not see it;
- twelve plan documents had no Open points section at all, although several deferred decisions ("confirm at the Group B review") or stated pending product decisions as settled;
- document 12's threat tables had no owner, no residual risk after the control, and no link to the register.

## Decision

**Every open point is scored on the register's own scales, and a serious one is a register risk.**

- Every plan document and every service sheet has an "Open points" section. Document 01, which is itself the list of open questions, carries the columns on its question tables instead.
- Every open-points table ends with four columns: **L** (likelihood the default in force is wrong) and **I** (impact if it is), both 1 to 5 on the scales of document 18 Section 1; **Score**, L times I; and **In the register**, the RISK identifiers that cover the point, or `none`.
- **A point that scores 12 or more names a RISK in document 18.** Twelve is the register's own line for a dated top-ten action, so an open point serious enough to act on cannot stay outside the register.
- Document 12's threat tables gain **Residual** (what remains after the control), **Owner role** (one of the five roles of document 18) and **In the register**.
- Kit-lint enforces it: R33 (sections present, columns present, 12 or more registered, threat tables owned) and R24 (scales, arithmetic, and every cited RISK existing). The plan-document and service-sheet templates carry the new columns.

## Alternatives considered

- **Move every open point into the register.** Rejected. Most open points are small and local; a register of several hundred rows would bury the twelve that matter.
- **Score open points in words (low, medium, high).** Rejected. Document 18 already scores in numbers with a defined threshold; two scales would need a translation table and would drift.
- **A separate likelihood column without the register link.** Rejected. The defect was not only the missing number but the missing connection: a serious point nobody tracks.

## Consequences

- Every open point states how likely its default is to be wrong. Several defaults that read as settled now show as uncertain, which is the point.
- The register grew by the serious points that had no row, and its heat map and top ten were recomputed.
- Adding an open point costs two numbers and a lookup in document 18.
