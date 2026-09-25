# Test-case work for `docs/plan/06-services/school.md`

## Collisions to resolve here (the owner keeps the identifier)

- **TC-DATA-006** at line 1156: "A corrupted consumer copy is repaired from `ListSnapshotPage` and reported | Nightly"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1948: "Committed to RolledBack | Rollback requested inside the window | Created rows removed, updated rows restored, conflicts reported"
- **TC-WEL-203** at line 1155: "Medical summary encrypted and every read logged (REQ-SCH-016) | Integration"
  - owner `docs/plan/06-services/wellbeing.md` line 958: "UAT | Allergy alert shown live with substance, severity and action before a trip"
- **TC-PERF-004** at line 1157: "Section change evicts both rosters and the student entry | Integration"
  - owner `docs/plan/21-performance-engineering.md` line 327: "School student, roster, guardians | `school.student.section-changed.v1`, `school.guardian.updated.v1` | Old and new section rosters both miss; the student entry misses"
