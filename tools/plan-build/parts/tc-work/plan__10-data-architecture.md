# Test-case work for `docs/plan/10-data-architecture.md`

## Collisions to resolve here (the owner keeps the identifier)

- **TC-DATA-004** at line 592: "Master brief Section 19, DbContext and model | Pooled context with per-lease accessor; named filters | A context leased without a tenant throws on first query; `IgnoreQueryFilters()` fails the build"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1946: "DryRunReady to Committing | Dry run less than 24 hours old | Batched commit started with an import identifier"
- **TC-DATA-005** at line 593: "Master brief Section 19, PostgreSQL | Partition by month; `tenant_id` first; partial indexes | Every partitioned table has next-three-months partitions and a detach job"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1947: "Committing to CommitFailed | A batch fails mid-import | Import reversed as a unit, no partial data remains"
- **TC-DATA-006** at line 594: "Master brief Section 19, Data integrity and reconciliation | Nightly reference-copy reconciliation | A deliberately corrupted copy is repaired and reported within one run"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1948: "Committed to RolledBack | Rollback requested inside the window | Created rows removed, updated rows restored, conflicts reported"

## Tests cited but defined nowhere: define them here

- **TC-DATA-302**
  - cited `docs/brief/02-appendices/appendix-q-uat-scripts.md` line 82: "8 | Fix 12 rows and commit the import | Completes in under 5 minutes with a summary of created and updated records"
  - cited `docs/plan/03-requirements-catalog.md` line 517: "REQ-DOC-011 | The import center provides templates, validation, a dry run with a row-and-column error report, commit, and rollback, and a 10,000-row import completes in under 5 minutes | 1 | Documents"
  - cited `docs/plan/20-traceability-matrix.md` line 435: "REQ-DOC-011 | The import center provides templates, validation, a dry run with a row-and-column error report, commit, and... | 1 | Documents | WF-DATA-01 | none | 06-services/documents.md | 3 | SL-DOC"
- **TC-DATA-303**
  - cited `docs/brief/02-appendices/appendix-q-uat-scripts.md` line 83: "9 | Roll back the import | The prior state is restored exactly and the rollback is audited"
  - cited `docs/plan/03-requirements-catalog.md` line 518: "REQ-DOC-012 | An import dry run writes nothing, and a rollback restores the prior state exactly and is audited | 1 | Documents | Master brief Section 12 item 9 | WF-DATA-01"
  - cited `docs/plan/06-services/documents.md` line 1199: "UAT | Dry run from the Excel template, commit in under 5 minutes, exact rollback (Appendix Q)"
  - cited `docs/plan/20-traceability-matrix.md` line 436: "REQ-DOC-012 | An import dry run writes nothing, and a rollback restores the prior state exactly and is audited | 1 | Documents | WF-DATA-01 | none | 06-services/documents.md | 3 | SL-DOC-416, SL-DOC-4"
- **TC-DATA-014**
  - cited `docs/plan/10-data-architecture.md` line 583: "A retention job detaches or drops a partition that a legal hold covers, and the evidence is gone | 2 | 5 | Data architect | none yet; Data architect proposes it at the Group E review"
  - cited `docs/plan/10-data-architecture.md` line 599: "Master brief Section 32; Appendix J.5 rule 3 | A job per retention row, honouring legal holds | Every row in Section 8 has a job that runs in the soak and detaches only the expected partitions"
  - cited `docs/plan/10-data-architecture.md` line 659: "Every retention row has a job | Nightly"
