# Test-case work for `docs/plan/06-services/behavior.md`

## Collisions to resolve here (the owner keeps the identifier)

- **TC-BEH-001** at line 804: "Workflow | `Recorded → UnderReview`: recorder in scope, case routed to the head of year for that section"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1279: "Recorded to UnderReview | Recorder taught or supervised the student that day | Case routed to the head of year for that section"
- **TC-BEH-002** at line 805: "Workflow | `UnderReview → ActionDecided`: action within the decider's authority recorded with its category"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1280: "UnderReview to ActionDecided | Action within the recorder authority level | Sanction or support recorded with its category"
- **TC-BEH-003** at line 806: "Workflow | `ActionDecided → GuardianNotified`: notice in the guardian's preferred language"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1281: "ActionDecided to GuardianNotified | Guardian has a verified channel | Notification sent in the guardian preferred language"
- **TC-BEH-004** at line 807: "Workflow | `GuardianNotified → PlanOpened`: plan with a named owner and a review date"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1282: "GuardianNotified to PlanOpened | Severity or repetition meets the plan threshold | Plan created with a named owner and a review date"
- **TC-BEH-005** at line 808: "Workflow, security | `UnderReview → Dismissed`: points reversed, correction sent, signal recalculated (T-BEH-03)"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1283: "UnderReview to Dismissed | Evidence does not support the record | Points reversed, correction sent, signal recalculated"
- **TC-BEH-006** at line 809: "Workflow | `FollowUpDue → Closed`: outcome recorded, timeline entry visible on Student 360"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1284: "FollowUpDue to Closed | Outcome and evidence recorded | Intervention closed, timeline entry visible on Student 360"

## Tests cited but defined nowhere: define them here

- **TC-BEH-360**
  - cited `docs/plan/06-services/behavior.md` line 800: "Existing identifiers are reused; new ones are minted in `TC-BEH-310` to `TC-BEH-360`, a range no document in the kit uses (checked with a search of `docs/` and `.claude/` on 2026-09-21: existing BEH i"
