# ADR-0021: Every verification claim names a check that runs

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-25
- **Requirement IDs:** REQ-TST-025

## Context

Every plan document ends with "How this document is verified", a table of claims and their proof. The scorecard (`docs/plan/30-plan-scorecard.md`, theme 6) found that many proofs named checks that did not exist. A reviewer then classified the 167 rows that named a kit check:

- 67 were true as written;
- 23 named the wrong rule (most said R07, which never reads `docs/plan/`, where R19 does the work);
- 48 relied on checks nobody had built: generated documents being current, risk scores being likelihood times impact, every workflow being assigned and built, the ADR index matching the records, and tools such as `tools/threat-model-lint` that were never planned;
- 20 needed human judgement and named no human;
- 9 relied on product artefacts without saying which slice builds them.

`/lint-plan` runs kit-lint and explains its findings; it performs no other check. A proof that says "/lint-plan checks X" is only true if a kit-lint rule checks X.

## Decision

**A verification row names one of three kinds of proof, and only those:**

1. **A kit-lint rule that checks the claim as its code is written.** Twelve rules were added so the mechanical claims are true: R21 (requirements catalog), R22 (ADR references and the index), R23 (generated documents are current, by running each generator in `--check` mode), R24 (risk arithmetic and register references), R25 (every workflow assigned once and built, every capability sliced), R26 (open questions mirrored in document 01), R27 (document 11's keys catalogued; publishers on their own exchange), R28 (every high or critical threat has a test), R29 (state diagrams have a terminal state and labelled transitions), R30 (SQL columns commented), R31 (database and image names registered), R32 (the owning sheet cites every transition test). R18 now requires a comment on every plan tree entry, and R01 resolves "reference architecture Section N" against the reference architecture. Each rule has a self-test that proves it fires.
2. **A named review step:** who (a role or a kit agent), what they compare, and when.
3. **A product artefact built by a named slice** in document 34, at a path in document 07.

**Generators check themselves.** Every plan generator writes through `tools/plan-build/write-generated.cjs`, which compares with the file on disk ignoring review-record dates and line endings. `--check` fails on a difference; a normal run leaves the file alone when only the date would change, so regenerating does not move the date of a review nobody repeated.

**R23 is skipped by the post-edit hook**, because a generated document is expected to lag while its sources are being edited; the full lint and `/lint-plan` run it.

**Brief change under this record (v9.3).** Rule R29 found one unlabelled transition in Appendix R (WF-FIN-04, `Renewed --> Active`); it is labelled "new period starts on the same terms". All three brief files are bumped to v9.3.

## Alternatives considered

- **Delete the unverifiable rows.** Rejected. The claims are true requirements of a good plan; the defect was the missing check, not the claim.
- **Restate every mechanical claim as a review step.** Rejected. A review step is skipped under pressure; a rule is not. Judgement stays with people, arithmetic goes to the lint.
- **Build every candidate rule the reviewer proposed.** Rejected for three: tree anatomy per service, verbatim quotations of brief tables, and the full messaging graph (bindings, queue names, cycles) need markers or judgement that a lint would fake. They are named review steps.

## Consequences

- `node tools/kit-lint/kit-lint.mjs .` takes a few seconds longer, because R23 runs six generators.
- A document edit that makes a generated document stale fails the full lint until the generator is rerun. That is the point.
- The product owner may reject the brief change; the label is cosmetic and the kit rules stand either way.
