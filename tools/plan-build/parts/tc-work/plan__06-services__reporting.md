# Test-case work for `docs/plan/06-services/reporting.md`

## Collisions to resolve here (the owner keeps the identifier)

- **TC-SCH-026** at line 861: "Integration | A historical report rerun after a structure change gives the same numbers from the sealed snapshot"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 618: "Archived to Archived | Historical report rerun after the structure changed | Same numbers as the original run"
- **TC-RPT-001** at line 844: "UAT, feature register | Morning brief and Today cards, each card with one action"
  - owner `docs/brief/02-appendices/appendix-w-feature-register.md` line 19: "1 | Today dashboards where every card leads to an action | "I can see what needs me and do it here" | 1 | 1 surfaces | 1 | Reporting"
- **TC-RPT-002** at line 845: "Feature register | Student 360 timeline filtered by the viewer's permissions"
  - owner `docs/brief/02-appendices/appendix-w-feature-register.md` line 20: "2 | Student 360 timeline filtered by the viewer's permissions | "The whole child, on one screen" | 1 | 1 surfaces | 1 | Bff.Web"
- **TC-RPT-003** at line 846: "Feature register | Early warning with explanation and an intervention; degrades to rules"
  - owner `docs/brief/02-appendices/appendix-w-feature-register.md` line 22: "4 | Early warning with explanation and an intervention | "I know who is slipping and what to do" | 2, degrades to rule thresholds | 2 suggests | 1 | Reporting"
- **TC-RPT-004** at line 847: "Feature register | Data Quality Center with fix actions"
  - owner `docs/brief/02-appendices/appendix-w-feature-register.md` line 31: "13 | Data quality center | "The list was wrong and the system told us" | 1 | 1 surfaces | 1 | Reporting"
- **TC-RPT-005** at line 848: "Feature register | Inspection readiness evidence folder (Tier 2)"
  - owner `docs/brief/02-appendices/appendix-w-feature-register.md` line 39: "21 | Inspection and accreditation readiness | "The evidence folder filled itself" | 1 | 1 surfaces | 2 | Reporting"
- **TC-RPT-006** at line 849: "Feature register | Morning brief per role before the day starts"
  - owner `docs/brief/02-appendices/appendix-w-feature-register.md` line 43: "25 | Morning brief per role | "I knew my day before I arrived" | 1, phrasing at 3 | 1 surfaces | 1 | Reporting"
- **TC-RPT-007** at line 850: "Feature register | Explain this number to the records"
  - owner `docs/brief/02-appendices/appendix-w-feature-register.md` line 45: "27 | Explain this number | "I could show the parent exactly why" | 1 | 1 surfaces | 1 | Reporting"
- **TC-RPT-008** at line 851: "Feature register | Because panel with reasons, rung and a reasoned override"
  - owner `docs/brief/02-appendices/appendix-w-feature-register.md` line 46: "28 | Because panel on every automated action | "It told me why, and I could disagree" | 1 | 1 surfaces | 1 | Reporting"
- **TC-WEL-201** at line 856: "UAT | Intervention opened from a flag with owner and review date"
  - owner `docs/plan/06-services/wellbeing.md` line 956: "UAT | Intervention opened from a flag with owner and review date"
- **TC-DATA-013** at line 859: "Integration | Replay paused: reads routed to the primary and the alert raised"
  - owner `docs/plan/10-data-architecture.md` line 598: "Master brief Section 19, read replica | Replica-lag awareness | Reads fall back to the primary when lag exceeds 30 s"

## Tests cited but defined nowhere: define them here

- **TC-RPT-360**
  - cited `docs/plan/06-services/reporting.md` line 840: "Existing identifiers are reused; new ones are minted in `TC-RPT-310` to `TC-RPT-360`, a range no document in the kit uses (checked with a search of `docs/` and `.claude/` on 2026-09-21: existing RPT i"
