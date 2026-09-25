# Test-case work for `docs/plan/06-services/documents.md`

## Collisions to resolve here (the owner keeps the identifier)

- **TC-PRV-011** at line 1200: "Workflow | `Requested → Classified`: sensitivity and row count from the actual query"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1904: "Requested to Classified | Requester holds the export permission for those entities | Sensitivity and row count computed from the actual query"
- **TC-PRV-012** at line 1201: "Workflow | `Classified → AutoApproved` below the threshold"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1905: "Classified to AutoApproved | Row count and sensitivity below the threshold | Export proceeds without a second person"
- **TC-PRV-013** at line 1202: "Workflow | `Classified → PendingApproval` for medical, safeguarding or bulk identifiers"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1906: "Classified to PendingApproval | Export includes medical, safeguarding, or bulk identifiers | Approval required before any file is produced"
- **TC-PRV-014** at line 1203: "Workflow, security | `Approved → Generating` only when the approver differs from the requester (T-DOC-04)"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1907: "Approved to Generating | Approver differs from the requester | Job queued with the approval recorded on the manifest"
- **TC-PRV-015** at line 1204: "Workflow, security | `Ready → Downloaded` once, watermarked with the requester"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1908: "Ready to Downloaded | Inside the window and by the requester only | File served once, watermarked with the requester identity"
- **TC-PRV-016** at line 1205: "Workflow | `Ready → LinkExpired`: link dead, temporary file deleted"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1909: "Ready to LinkExpired | Window passed | Link dead and the temporary file deleted"
- **TC-DATA-001** at line 1193: "Workflow | `Uploaded → Parsed`: known template, columns mapped, unknown columns reported"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1943: "Uploaded to Parsed | File matches a known template version | Columns mapped, unknown columns reported"
- **TC-DATA-002** at line 1194: "Workflow | `Parsed → ErrorsReported`: required field empty, row-level report, nothing written"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1944: "Parsed to ErrorsReported | A required field is empty or a reference is missing | Row-level error report produced, nothing written"
- **TC-DATA-003** at line 1195: "Workflow, perf | `Validated → DryRunReady`: 10,000 rows inside the budget"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1945: "Validated to DryRunReady | 10,000 rows evaluated inside the budget | Preview shows inserts, updates, and skips per entity"
- **TC-DATA-004** at line 1196: "Workflow | `DryRunReady → Committing`: preview under 24 h, batched commit with an import identifier"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1946: "DryRunReady to Committing | Dry run less than 24 hours old | Batched commit started with an import identifier"
- **TC-DATA-005** at line 1197: "Workflow, chaos | `Committing → CommitFailed`: import reversed as a unit, no partial data"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1947: "Committing to CommitFailed | A batch fails mid-import | Import reversed as a unit, no partial data remains"
- **TC-DATA-006** at line 1198: "Workflow | `Committed → RolledBack`: created rows removed, updated rows restored, conflicts reported"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1948: "Committed to RolledBack | Rollback requested inside the window | Created rows removed, updated rows restored, conflicts reported"
- **TC-DOC-001** at line 1207: "Feature register | Go live in a day: import with dry run and rollback"
  - owner `docs/brief/02-appendices/appendix-w-feature-register.md` line 27: "9 | Go live in a day: import with dry run and rollback | "We moved in a morning" | 1 | 1 surfaces | 1 | Documents"
- **TC-TST-208** at line 1212: "Integration | English and Arabic PDF snapshots match the committed baselines with the shaping check"
  - owner `docs/plan/16-test-strategy.md` line 720: "Bilingual PDF baselines with the shaping canaries | 9"
- **TC-PLAT-008** at line 1213: "Integration | Arabic-Indic digits normalized before import validation"
  - owner `docs/plan/33-platform-support-and-dev-environments.md` line 230: "Arabic digits in an Excel file produced on Windows | Import reads them as text and rejects the row | An import test in `Documents` with a file containing Arabic-Indic digits, asserting the numerals are normalised before "

## Tests cited but defined nowhere: define them here

- **TC-DOC-360**
  - cited `docs/plan/06-services/documents.md` line 1189: "Existing identifiers are reused; new ones are minted in `TC-DOC-310` to `TC-DOC-360`, a range no document in the kit uses (checked with a search of `docs/` and `.claude/` on 2026-09-21: only `TC-DOC-0"
