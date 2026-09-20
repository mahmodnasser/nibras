# ADR-0014: Every requirement, rule and workflow is proven by an identified test case

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-19
- **Requirement IDs:** REQ-TST-001

## Context

The brief required a traceability matrix and a definition of done but never defined what a test case looked like or which proof each kind of artefact owed. In practice that means the traceability matrix has a column nobody can fill honestly.

## Decision

Introduce `TC-<AREA>-<NNN>` with a fixed Given, When, Then format and a Covers line naming the requirement, workflow and rule it proves.

Appendix V holds the coverage matrix: one row per artefact kind, naming the proof, the level and the gate. Master brief Section 24 states it as a contract.

The traceability matrix gains Workflow, Rule, Test case and Platform columns. A row with an empty test case is an unfinished requirement, and the lint refuses it.

## Alternatives considered

- **Rely on coverage percentages.** Rejected: coverage measures lines executed, not behaviour proven, and it cannot tell you that a business rule is right.

## Consequences

- Writing the test case identifier before the test is extra work at the start of every slice. It is also the only thing that makes the traceability matrix true rather than decorative.
