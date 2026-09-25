# ADR-0020: Every test case is defined in exactly one document

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-25
- **Requirement IDs:** REQ-TST-001, REQ-TST-009, REQ-TST-025

## Context

ADR-0014 made `TC-<AREA>-<NNN>` the unit of proof: every requirement, rule and workflow is proven by an identified test case. It did not say where a test case is defined, and the kit grew 1,876 distinct identifiers across the brief and the plan. The scorecard (`docs/plan/30-plan-scorecard.md`, theme 5) then found what that allows:

- the same identifier meaning two different tests (Appendix W's demo test for feature 3, "sixty-second attendance", was TC-ATT-001, which Appendix R gives to the "Open to Marked" transition of WF-ATT-01);
- about 165 identifiers cited as proof and defined nowhere, so the proof they promise cannot be found;
- hundreds of tests restated in a second document, where a later edit to one copy silently diverges from the other.

A test identifier that can mean two things proves neither, and the traceability matrix (document 20) inherits the ambiguity.

## Decision

**A test case is defined in exactly one document, and cited everywhere else.**

- *Defined* means a table cell holds the identifier and nothing else, in the first column or in a column headed Test, Test case, TC, Identifier, Test case ID or Test id (Appendix W's Demo column included). *Cited* means any other appearance. A cell that names its owner, such as "`TC-ATT-003` (Appendix R)", is a citation by design.
- When two documents want the same test, the owner is chosen by precedence: Appendix R (workflow transitions), then Appendix W (demo proofs), then the service sheet of the identifier's area, then the area's cross-cutting plan document, then any other document. The other document cites it.
- When two documents use one identifier for **different** tests, the lower-precedence one is renumbered.
- Derived acceptance tests keep the rule of document 20: `TC-<AREA>-(950 + requirement number)`, owned by the requirement in document 03 and never minted elsewhere.
- **kit-lint rule R20** enforces all of it: an identifier defined in two documents, an identifier cited but defined nowhere, and a derived test whose requirement does not exist are errors.
- The registry of every test case, with its owner, what it proves and who cites it, is generated into `docs/plan/16-annex-test-case-registry.md` from the same code R20 runs, so the list and the rule cannot disagree.

**Brief change under this record (v9.2).** Twelve Appendix W demo tests reused Appendix R identifiers for different tests. Appendix R keeps its identifiers; W's move to the 810 range, and Appendices O and P, which cite W's meaning through feature numbers, follow: TC-ATT-001 to 004 to TC-ATT-810 to 813, TC-ASM-001 and 002 to TC-ASM-810 and 811, and TC-ACA-001, TC-BEH-001, TC-HR-001, TC-OPS-001, TC-SCH-001 and TC-WEL-001 to the same area's 810. Two further brief edits follow from the plan-side work: Appendix O cites document 08's renumbered audit-viewer test (TC-SEC-001 to TC-SEC-530, minute 12) and document 32's reserve-step payment test (TC-FIN-001 to TC-FIN-551, R-15); and Appendix Q step 12 no longer promises queued approvals, because Appendix M keeps approval decisions online (the same alignment ADR-0019 made in Appendix U). All three brief files are bumped to v9.2.

## Alternatives considered

- **A central list of test cases, typed by hand.** Rejected. It would be a fourth copy of every test and the first thing to drift. Generating it from the documents that define the tests keeps one source.
- **Let restatements stand and compare them by meaning.** Rejected. No lint can compare meaning, so the check would be a review step that is skipped under pressure. One definition removes the question.
- **Renumber Appendix R instead of W.** Rejected. Appendix R's identifiers are cited by every service sheet's transition tests and by document 20; W's demo identifiers are cited in three appendices and one plan document.

## Consequences

- A document that relies on another document's test writes the owner beside the identifier. That costs a few characters and tells the reader where the test is specified.
- Every plan document with a test table carries definitions for the tests it cites that nobody else defines; about 165 were added in this change.
- A new test is registered by writing it once, in its owner's table. R20 fails the lint if anyone mints the same identifier again.
- The product owner may reject the brief change; the plan-side changes stand either way, and W would then need another way to separate its demo tests from R's.
