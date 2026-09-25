# Test-case work for `docs/plan/26-migration-and-onboarding-toolkit.md`

## Collisions to resolve here (the owner keeps the identifier)

- **TC-PLT-001** at line 470: "Tenant region fixed at `Validated` | Residency never changes afterwards (`PLATFORM_RESIDENCY_VIOLATION`)"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 427: "Requested to Validated | Subdomain free and plan exists | Tenant record created in Requested state only"
- **TC-PLT-002** at line 471: "Routes answer `PLATFORM_PROVISIONING_IN_PROGRESS` until `Provisioned` | The wizard shows provisioning progress instead"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 428: "Validated to Provisioning | Operator or signup token authorised | Saga started with a correlation identifier"
- **TC-PLT-006** at line 473: "The seeded `admin` account | Platform scope only (ADR-0003); never the school's administrator and never used for onboarding; safeguards in `12-security-privacy-safety.md` §8"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 432: "Onboarding to Live | Academic year, campus, and owner account present | Tenant opened to all users, seeded password forced to change"
- **TC-PLT-006** at line 475: "`Onboarding` to `Live` | Academic year, campus and the owner account present; the seeded password changed"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 432: "Onboarding to Live | Academic year, campus, and owner account present | Tenant opened to all users, seeded password forced to change"
- **TC-DATA-006** at line 404: "Window | 7 days after `Committed` (WF-DATA-01, which owns the value); after it the job is `Sealed` and before images are purged"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1948: "Committed to RolledBack | Rollback requested inside the window | Created rows removed, updated rows restored, conflicts reported"
- **TC-DATA-006** at line 407: "Inserted rows | Deleted by `import_id` in each target service"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1948: "Committed to RolledBack | Rollback requested inside the window | Created rows removed, updated rows restored, conflicts reported"
- **TC-DATA-006** at line 408: "Updated rows | Restored from `import_before_image` when `row_version` still matches"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1948: "Committed to RolledBack | Rollback requested inside the window | Created rows removed, updated rows restored, conflicts reported"
