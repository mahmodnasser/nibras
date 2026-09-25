# Test-case work for `docs/plan/06-services/wellbeing.md`

## Tests cited but defined nowhere: define them here

- **TC-WEL-704**
  - cited `docs/brief/02-appendices/appendix-q-uat-scripts.md` line 146: "4 | Sign in as a teacher on another device and search the student | The case does not appear in search, in Student 360 or in any report"
  - cited `docs/plan/03-requirements-catalog.md` line 593: "REQ-WEL-012 | Wellbeing records never appear in general search, Student 360 or any report without a specific permission, and the product does not confirm whether a case exists | 2 | Wellbeing | Append"
  - cited `docs/plan/06-services/wellbeing.md` line 1083: "Existence is never confirmed | Integration and UAT"
  - cited `docs/plan/20-traceability-matrix.md` line 491: "REQ-WEL-012 | Wellbeing records never appear in general search, Student 360 or any report without a specific permission, ... | 2 | Wellbeing | none | BR-WEL-001 | 06-services/wellbeing.md | 5 | SL-WEL"
- **TC-WEL-707**
  - cited `docs/brief/02-appendices/appendix-q-uat-scripts.md` line 151: "9 | Close the case with an outcome | The case closes, the flag clears, and the closure is audited"
  - cited `docs/plan/03-requirements-catalog.md` line 597: "REQ-WEL-016 | Closing a case records an outcome, clears the flag, and is audited | 2 | Wellbeing | Appendix A17 | WF-WEL-04"
  - cited `docs/plan/06-services/wellbeing.md` line 310: "POST | `/api/v1/wellbeing/counseling-cases/{id}/close` | `wellbeing.counseling-cases.edit` | `{ outcomeCode }` | `WELLBEING_CASE_ALREADY_CLOSED` | Yes, state-guarded"
  - cited `docs/plan/06-services/wellbeing.md` line 959: "UAT | Counsellor home, referral acceptance, logged session note, absence from search and Student 360, playbook, clinic record refused to the counsellor, closure audited"
- **TC-WEL-360**
  - cited `docs/plan/06-services/wellbeing.md` line 946: "Existing identifiers are reused; new ones are minted in `TC-WEL-310` to `TC-WEL-360`, a range no document in the kit uses (checked with a search of `docs/` and `.claude/` on 2026-09-21: existing WEL i"
