# Test-case work for `docs/plan/06-services/finance.md`

## Tests cited but defined nowhere: define them here

- **TC-FIN-404**
  - cited `docs/brief/02-appendices/appendix-q-uat-scripts.md` line 96: "4 | Try to record 3,200.00 SAR against the same 2,750.00 SAR invoice | Refused with the overpayment message, and posting the excess as credit is offered"
  - cited `docs/plan/03-requirements-catalog.md` line 400: "REQ-FIN-013 | Payments allocate by the configured order, and an overpayment is refused unless posted as credit | 1 | Finance | Appendix A15 | BR-FIN-008; BR-FIN-009"
  - cited `docs/plan/20-traceability-matrix.md` line 338: "REQ-FIN-013 | Payments allocate by the configured order, and an overpayment is refused unless posted as credit | 1 | Finance | none | BR-FIN-008, BR-FIN-009 | 06-services/finance.md | 3 | SL-FIN-412, "
- **TC-FIN-405**
  - cited `docs/brief/02-appendices/appendix-q-uat-scripts.md` line 97: "5 | Record a payment in AED against a SAR invoice series | Refused with the currency mismatch message and the invoice currency shown"
  - cited `docs/plan/03-requirements-catalog.md` line 401: "REQ-FIN-014 | A payment in a currency different from the invoice series is refused with the invoice currency shown | 1 | Finance | Appendix A15; Master brief Section 17 | BR-FIN-011"
  - cited `docs/plan/20-traceability-matrix.md` line 339: "REQ-FIN-014 | A payment in a currency different from the invoice series is refused with the invoice currency shown | 1 | Finance | none | BR-FIN-011 | 06-services/finance.md | 3 | SL-FIN-412, SL-FIN-4"
- **TC-FIN-408**
  - cited `docs/brief/02-appendices/appendix-q-uat-scripts.md` line 100: "8 | Have the principal approve the refund | The refund posts and both approvals appear in the audit entry"
  - cited `docs/plan/03-requirements-catalog.md` line 402: "REQ-FIN-015 | A refund needs an approver who differs from the requester and within limit, and returns to the original instrument | 1 | Finance | Appendix A15 | WF-FIN-02; BR-FIN-010"
  - cited `docs/plan/06-services/finance.md` line 563: "POST | `/api/v1/finance/refunds/{id}/approve` | `finance.refunds.approve` | `{ note }` | `FINANCE_REFUND_APPROVAL_REQUIRED` (self-approval or above limit) | `Idempotency-Key` required"
  - cited `docs/plan/20-traceability-matrix.md` line 340: "REQ-FIN-015 | A refund needs an approver who differs from the requester and within limit, and returns to the original ins... | 1 | Finance | WF-FIN-02 | BR-FIN-010 | 06-services/finance.md | 3 | SL-FI"
- **TC-FIN-410**
  - cited `docs/brief/02-appendices/appendix-q-uat-scripts.md` line 102: "10 | Correct the discrepancy and close | The day balances to 0.00 SAR and the close is recorded"
  - cited `docs/plan/03-requirements-catalog.md` line 416: "REQ-FIN-029 | A balanced day closes to 0.00, records a deposit slip reference, and cannot be edited afterwards | 1 | Finance | Appendix A15 | WF-FIN-06"
  - cited `docs/plan/06-services/finance.md` line 594: "POST | `/api/v1/finance/day-closes` | `finance.cashier.close-day` | `{ campusId, businessDate }` | `FINANCE_DAY_CLOSE_OUT_OF_BALANCE` listing the documents (TC-FIN-409), `FINANCE_DAY_ALREADY_CLOSED` |"
  - cited `docs/plan/20-traceability-matrix.md` line 354: "REQ-FIN-029 | A balanced day closes to 0.00, records a deposit slip reference, and cannot be edited afterwards | 1 | Finance | WF-FIN-06 | none | 06-services/finance.md | 3 | SL-FIN-441, SL-FIN-447 | "
