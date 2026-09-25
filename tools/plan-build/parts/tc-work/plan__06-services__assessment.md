# Test-case work for `docs/plan/06-services/assessment.md`

## Collisions to resolve here (the owner keeps the identifier)

- **TC-ASM-001** at line 1040: "Workflow | `MarkEntry → Validated` only when every student has a mark, absence or exemption"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 812: "MarkEntry to Validated | Every enrolled student has a mark, an absence, or an exemption | Entry accepted for moderation"
- **TC-ASM-002** at line 1041: "Workflow | Mark above the maximum refused with the cell highlighted"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 813: "MarkEntry to Validated | A mark exceeds the maximum for the component | Rejected at entry with the offending cell highlighted"
- **TC-ASM-003** at line 1042: "Workflow | `Validated → Moderated` stores the original and the reason"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 814: "Validated to Moderated | Moderator holds `assessment.marks.moderate` for the subject | Adjustment stored with the original value and the reason"
- **TC-ASM-004** at line 1043: "Workflow | `Approved → Locked`: edits refused, grade change offered"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 815: "Approved to Locked | Principal approval recorded for the cohort | Further edits refused, grade change workflow offered instead"
- **TC-ASM-005** at line 1044: "Saga, load | 800 cards inside the batch budget with progress (N-02)"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 816: "Locked to Generated | 800 cards queued in one batch | All rendered inside the batch budget with progress shown"
- **TC-ASM-006** at line 1045: "Saga, chaos | Worker crash after 500 cards resumes with no duplicates"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 817: "Generating to Generating | Worker crashes after 500 cards | Resume produces the remaining 300 with no duplicates"
- **TC-ASM-011** at line 1046: "Workflow | Appeal inside the window creates the review"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 850: "Submitted to UnderReview | Appeal raised inside the appeal window by a linked guardian | Review task created for the subject department"
- **TC-ASM-012** at line 1047: "Workflow | Appeal after the window refused with the dates"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 851: "Submitted to Submitted | Appeal raised after the window closed | Refused with the window dates shown"
- **TC-ASM-013** at line 1048: "Workflow, security | Approver differs from the proposer (T-ASM-01)"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 852: "ChangeProposed to ChangeApproved | Principal differs from the proposing teacher | Change accepted with both identities recorded"
- **TC-ASM-014** at line 1049: "Workflow | Change applied under lock and recalculated"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 853: "ChangeApproved to Applied | Academic year not archived | Mark updated under lock, recalculation triggered"
- **TC-ASM-015** at line 1050: "Workflow | Superseding card rendered, previous retained"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 854: "Applied to Reissued | Superseding card rendered | Previous version retained and marked superseded"
- **TC-ASM-016** at line 1051: "Workflow | GPA and rank recalculated and republished"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 855: "Applied to Applied | Change affects the GPA and ranking | Both recalculated and republished for that cohort"
- **TC-WEL-003** at line 1094: "Integration | Accommodation applied per sitting (REQ-ASM-009)"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 1320: "Published to AppliedToSitting | Exam scheduled for a subject in the plan | Extra time, room, and reader applied to that sitting"

## Tests cited but defined nowhere: define them here

- **TC-ASM-350**
  - cited `docs/plan/06-services/assessment.md` line 1036: "Existing identifiers are reused; new ones are minted in `TC-ASM-301` to `TC-ASM-350`, a range no document in the kit uses (searched on 2026-09-21)."
