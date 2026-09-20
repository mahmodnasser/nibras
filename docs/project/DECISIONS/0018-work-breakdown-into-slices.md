# ADR-0018: Work is broken down into slices of one to three days

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-20
- **Requirement IDs:** REQ-PLAT-005

## Context

Master brief Section 28 sets seven phases with duration ranges, and Section 26 says to "work in small, complete vertical slices". Neither said how small, what a slice contains, or how one is written down. The roadmap document was therefore going to produce phases with requirement identifiers and nothing an engineer could pick up on a Monday morning.

A phase of sixteen to twenty weeks with no decomposition is an intention. The gap showed up as soon as the question "when can we start implementing" was asked, because the honest answer depended on work units that did not exist.

There is a second, quieter risk. Without a stated unit, teams decompose by layer, because layers are the obvious seams in a Clean Architecture codebase. "Build the attendance domain this sprint, the API next sprint" produces nothing demonstrable for weeks and hides integration problems until the end.

## Decision

**Three levels, and no more.** Phase, from master brief Section 28. **Capability** (`CAP-<AREA>-<NN>`), one to three weeks, something a named person can do end to end and that can be demonstrated. **Slice** (`SL-<AREA>-<NNN>`), one to three days, one use case through every layer it touches, one pull request.

**Split by use case, never by layer.** A slice whose name is a layer, a table or a technology has been split the wrong way. The test is whether it can be demonstrated when it is finished.

**A slice is done when** its use case works end to end: domain and application with unit tests built from the rules in Appendix S, persistence with the tenant filter and row-level security policy, the endpoint with its permission and error codes, events through the outbox, the integration tests including tenant isolation and the query budget, the screen in both languages if it has one, every state present, and traceability updated.

**Estimates are day ranges, not points.** Anything above three days is two slices.

**Dependencies are stated as contracts, not as slices.** A consuming slice may start when the contract is published, which is what stops the plan serialising.

**Every slice names its requirement, rule and workflow identifiers**, and `/lint-plan` checks both directions: a slice that maps to nothing is scope nobody asked for, and a requirement with no slice is scope nobody will build.

This lands as `docs/plan/34-work-breakdown.md` in Group F, with `docs/templates/work-slice.md` as the template and `docs/project/BACKLOG.md` as the ordered queue.

## Alternatives considered

- **Story points and velocity.** Rejected. Points are a relative measure that becomes a false absolute the moment anyone divides by velocity to get a date. Day ranges are honest about their own imprecision, and the roadmap already owns dates as ranges.
- **Decompose only to capability level and let engineers split further.** Rejected. It is how layer-splitting happens, and it removes the check that every requirement has somewhere to be built.
- **A four-level hierarchy with epics above capabilities.** Rejected. Phase already plays that role, and a fourth level turns planning into an estimating exercise.
- **Leave the breakdown to a project tracker outside the repository.** Rejected for the plan itself. The slice list must reference requirement, rule and workflow identifiers that live here, and a tracker cannot be linted against them. Exporting to a tracker afterwards is fine and expected.

## Consequences

- Group F gains a document, and it cannot be written until Group E sets the phases and capabilities. The order in `PLAN_SPEC.md` reflects that.
- Every requirement must be reachable from a slice before the plan is approved, which will surface requirements nobody planned to build. That is the point, and it will be uncomfortable the first time.
- Writing a slice takes a few minutes. If it takes longer, the slice is too big, and that friction is deliberate.
- The definition of done per slice repeats the coverage matrix in Appendix V rather than restating it differently. If the two ever disagree, Appendix V wins and the template is the defect.
