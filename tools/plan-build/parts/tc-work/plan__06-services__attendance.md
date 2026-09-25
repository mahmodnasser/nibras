# Test-case work for `docs/plan/06-services/attendance.md`

## Collisions to resolve here (the owner keeps the identifier)

- **TC-ATT-001** at line 1121: "Workflow | `Open → Marked`: a teacher holding the assignment saves the register; absentees queued for alerting"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 928: "Open to Marked | Teacher holds a teaching assignment for that session | Register saved, absent students queued for alerting"
- **TC-ATT-002** at line 1122: "Workflow, job | `Open → NotMarked`: grace passed, escalation raised, still markable with a reason (BR-ATT-011)"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 929: "Open to NotMarked | Grace period passed with no submission | Escalation raised, session still markable with a reason"
- **TC-ATT-003** at line 1123: "Workflow, end to end | `Marked → AbsenceAlerted` and the pre-filled register from gate and leave; alert enqueued within 30 s"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 930: "Marked to AbsenceAlerted | Guardian has a verified channel and quiet hours allow it | Alert delivered or deferred, never silently dropped"
- **TC-ATT-004** at line 1124: "Workflow | `ExcuseSubmitted → ExcuseApproved`: day excused, counters recalculated"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 931: "ExcuseSubmitted to ExcuseApproved | Submitted within the excuse window by a linked guardian | Day changed to excused, counters recalculated"
- **TC-ATT-005** at line 1125: "Workflow | `Marked → ThresholdReached`: flag raised with its reasons"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 932: "Marked to ThresholdReached | Absence rate crosses the configured limit | Flag raised with the reasons that produced it"
- **TC-ATT-006** at line 1126: "Workflow | `Marked → Marked` offline conflict: both values shown, no silent overwrite"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 933: "Marked to Marked | Offline register syncs after a server register exists | Conflict shown to the teacher, no silent overwrite"
- **TC-ATT-011** at line 1127: "Workflow (with the Requests stub) | `Requested → UnderReview`: review task raised for the homeroom teacher"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 964: "Requested to UnderReview | Requester is a linked guardian with pickup rights | Review task raised for the homeroom teacher"
- **TC-ATT-012** at line 1128: "Workflow | `UnderReview → Approved`: dismissal created in `Approved` from `requests.request.approved.v1`"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 965: "UnderReview to Approved | Requested time inside school hours and not during an exam | Pass generation queued"
- **TC-ATT-013** at line 1129: "Workflow | `Approved → PassIssued`: collector on the list, code issued with photo"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 966: "Approved to PassIssued | Named collector is on the authorized pickup list | One-time code issued with a photo of the collector"
- **TC-ATT-014** at line 1130: "Workflow | `PassIssued → Verified`: unused code inside the window shows photo and identity"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 967: "PassIssued to Verified | Code unused and inside the validity window | Collector photo and identity shown to the gate officer"
- **TC-ATT-015** at line 1131: "Workflow, security | `PassIssued → PassIssued`: second presentation refused and logged (T-ATT-01)"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 968: "PassIssued to PassIssued | Same code presented a second time | Refused as already used, attempt logged"
- **TC-ATT-016** at line 1132: "Workflow | `Verified → Released`: early-leave attendance written, guardian notified"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 969: "Verified to Released | Gate officer confirms the handover | Early-leave attendance written, guardian notified"

## Tests cited but defined nowhere: define them here

- **TC-ATT-203**
  - cited `docs/brief/02-appendices/appendix-q-uat-scripts.md` line 44: "4 | Put the device in airplane mode and mark the second class | Marking works, a clear offline badge shows, and the queue count is visible"
  - cited `docs/plan/03-requirements-catalog.md` line 350: "REQ-ATT-006 | Attendance marking works offline with a visible offline badge and queue count, and syncs each mark exactly once | 1 | Attendance | Appendix A10; Appendix M | WF-ATT-01"
  - cited `docs/plan/20-traceability-matrix.md` line 293: "REQ-ATT-006 | Attendance marking works offline with a visible offline badge and queue count, and syncs each mark exactly ... | 1 | Attendance | WF-ATT-01 | none | 06-services/attendance.md | 2 | SL-AT"
- **TC-ATT-204**
  - cited `docs/brief/02-appendices/appendix-q-uat-scripts.md` line 46: "6 | Have a colleague mark one of the same students differently from another device, then sync | Both values with their times are shown and the teacher chooses; nothing is resolved silently"
  - cited `docs/plan/03-requirements-catalog.md` line 351: "REQ-ATT-007 | Two devices marking the same student differently surface both values with their times, and nothing resolves silently | 1 | Attendance | Appendix M | WF-ATT-01"
  - cited `docs/plan/20-traceability-matrix.md` line 294: "REQ-ATT-007 | Two devices marking the same student differently surface both values with their times, and nothing resolves... | 1 | Attendance | WF-ATT-01 | none | 06-services/attendance.md | 2 | SL-AT"
- **TC-ATT-207**
  - cited `docs/brief/02-appendices/appendix-q-uat-scripts.md` line 62: "4 | Approve the excuse | The register updates, the guardian is notified, and the audit entry names you"
  - cited `docs/plan/03-requirements-catalog.md` line 360: "REQ-ATT-016 | Approving an excuse updates the register, notifies the guardian, and audits the approver | 1 | Attendance | Appendix A10; Appendix C | WF-ATT-01"
  - cited `docs/plan/06-services/attendance.md` line 1134: "UAT, end to end | All-present marking, 60-second class, offline badge, two-device conflict, timeline, excuse review with closed medical detail, approval audit"
  - cited `docs/plan/20-traceability-matrix.md` line 303: "REQ-ATT-016 | Approving an excuse updates the register, notifies the guardian, and audits the approver | 1 | Attendance | WF-ATT-01 | none | 06-services/attendance.md | 2 | SL-ATT-213 | any | Planned"
- **TC-ATT-360**
  - cited `docs/plan/06-services/attendance.md` line 1117: "Existing identifiers are reused; new ones are minted in `TC-ATT-301` to `TC-ATT-360`, a range no document in the kit uses (checked with a search of `docs/` on 2026-09-21)."
