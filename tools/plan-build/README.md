# Plan build

The scripts that generated and validate the plan documents that are not written by hand. Every path resolves from the script's own location, so they run from any checkout on Windows or Linux with Node 22.

| Script | Produces | Run |
|---|---|---|
| `assemble-03.mjs` | `docs/plan/03-requirements-catalog.md`, validated | `node tools/plan-build/assemble-03.mjs` |
| `gen-31.mjs` | `docs/plan/31-business-rules-and-workflows.md` | `node tools/plan-build/gen-31.mjs` |
| `extract-usecases.mjs` | `parts/usecases.md`: every use case, consumer and job in the service sheets | `node tools/plan-build/extract-usecases.mjs` |
| `assemble-34.mjs` | `docs/plan/34-work-breakdown.md` from `parts/wb-part-*.md` | `--part C` validates one part; `--write` needs all four; `--write --partial` writes the phases done so far |
| `build-30.cjs` | `docs/plan/30-plan-scorecard.md` from the six group scorecards in `parts/score-<group>.md` | `node tools/plan-build/build-30.cjs` |
| `schedule-34.mjs` | The phase ranges and MVP figure in document 17 Section 1, from document 34 slice-days and the Section 29 team | `node tools/plan-build/schedule-34.mjs` |
| `gen-tc-registry.mjs` | `docs/plan/16-annex-test-case-registry.md`: every test case, its one owner, what it proves, who cites it (ADR-0020) | `node tools/plan-build/gen-tc-registry.mjs` |
| `tc-worklist.mjs` | Per-document work lists for kit-lint R20 findings, in `parts/tc-work/` | `node tools/plan-build/tc-worklist.mjs --write` |
| `gen-20.mjs` | `docs/plan/20-traceability-matrix.md` from 03, 17, 31, 34 and the sheets | `node tools/plan-build/gen-20.mjs`, once document 34 is complete |

`wb-prompt.md` is the writer prompt for one work-breakdown part; `parts/wb-scope-<part>.md` is that part's requirement list. `parts/brief-findings.md` logs the brief defects found while planning, to be applied under ADR-0019.
