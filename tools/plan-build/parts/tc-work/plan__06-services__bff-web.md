# Test-case work for `docs/plan/06-services/bff-web.md`

## Collisions to resolve here (the owner keeps the identifier)

- **TC-RPT-002** at line 430: "End to end | Student 360 timeline filtered by the viewer's permissions (REQ-BFF-003)"
  - owner `docs/brief/02-appendices/appendix-w-feature-register.md` line 20: "2 | Student 360 timeline filtered by the viewer's permissions | "The whole child, on one screen" | 1 | 1 surfaces | 1 | Bff.Web"
- **TC-WEB-001** at line 431: "End to end | Command palette and natural-language search limited by permissions (REQ-BFF-006)"
  - owner `docs/brief/02-appendices/appendix-w-feature-register.md` line 29: "11 | Command palette and natural-language search | "I typed what I wanted" | 1, natural language at 3 | 1 surfaces | 1 | Bff.Web"
- **TC-RPT-201** at line 432: "UAT | Homeroom home card shows absent today, excuses to review, flags, birthdays"
  - owner `docs/plan/06-services/reporting.md` line 853: "UAT | Homeroom card: absent today, excuses, flags, birthdays"
- **TC-SEC-331** at line 433: "Security | T-GW-04: no field the user may not see"
  - owner `docs/plan/12-security-privacy-safety.md` line 303: "T-GW-04 | Bff.Web aggregation | Information disclosure | The backend composes a screen with fields the user may not see | med | high | The backend forwards the caller's token; it holds no permissions of its own and store"

## Tests cited but defined nowhere: define them here

- **TC-BFF-160**
  - cited `docs/plan/06-services/bff-mobile.md` line 473: "New identifiers are minted in `TC-BFF-101` to `TC-BFF-160` (Bff.Web uses `TC-BFF-001` to `TC-BFF-060`); no document in the kit used a `TC-BFF-` identifier before these sheets (searched on 2026-09-21)."
- **TC-BFF-060**
  - cited `docs/plan/06-services/bff-mobile.md` line 473: "New identifiers are minted in `TC-BFF-101` to `TC-BFF-160` (Bff.Web uses `TC-BFF-001` to `TC-BFF-060`); no document in the kit used a `TC-BFF-` identifier before these sheets (searched on 2026-09-21)."
  - cited `docs/plan/06-services/bff-web.md` line 426: "New identifiers are minted in `TC-BFF-001` to `TC-BFF-060` for Bff.Web (Bff.Mobile uses `TC-BFF-101` onward); no document in the kit used a `TC-BFF-` identifier before these sheets (searched on 2026-0"
