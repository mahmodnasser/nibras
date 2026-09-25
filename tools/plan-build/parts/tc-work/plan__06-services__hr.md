# Test-case work for `docs/plan/06-services/hr.md`

## Collisions to resolve here (the owner keeps the identifier)

- **TC-HR-026** at line 1245: "A suspended teacher's eligibility check returns `HR_LICENCE_EXPIRED`, which Bff.Web shows before an assignment | Integration, Hr and Bff.Web"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1597: "Suspended to Suspended | Timetable officer tries to assign a class | Refused with an expired-credential error"

## Tests cited but defined nowhere: define them here

- **TC-HR-803**
  - cited `docs/brief/02-appendices/appendix-q-uat-scripts.md` line 161: "4 | Try to approve leave that exceeds the balance | Refused with the balance shown and unpaid leave offered"
  - cited `docs/plan/03-requirements-catalog.md` line 607: "REQ-HR-004 | Leave types carry balances, accrual, carry-over, an approval chain and a calendar, and leave above the balance is refused with unpaid leave offered | 2 | Hr | Appendix A18 | WF-HR-01"
  - cited `docs/plan/06-services/hr.md` line 176: "`unpaid_split_days` | numeric(6,2) | no default 0"
  - cited `docs/plan/06-services/hr.md` line 352: "POST | `/api/v1/hr/leave-requests` | `hr.leave.create` | `{ staffId (self by default), leaveTypeCode, fromDate, toDate, fromTime, toTime, acceptUnpaidSplit, documentFileId, requestId }` | 201 in `Unde"
- **TC-HR-806**
  - cited `docs/brief/02-appendices/appendix-q-uat-scripts.md` line 165: "8 | Try to change a payroll input after the period locks | Refused; an adjustment in the next period is offered"
  - cited `docs/plan/03-requirements-catalog.md` line 611: "REQ-HR-008 | A payroll input cannot change after the period locks, and an adjustment in the next period is offered | 2 | Hr | Appendix A18 | WF-HR-04"
  - cited `docs/plan/06-services/hr.md` line 384: "POST | `/api/v1/hr/payroll-periods/{id}/inputs` | `hr.payroll.prepare-inputs` and `hr.payroll.view-salary` | manual input line | 201 | by `Idempotency-Key`"
  - cited `docs/plan/20-traceability-matrix.md` line 504: "REQ-HR-008 | A payroll input cannot change after the period locks, and an adjustment in the next period is offered | 2 | Hr | WF-HR-04 | none | 06-services/hr.md | 5 | SL-HR-622, SL-HR-624 | any | Pla"
