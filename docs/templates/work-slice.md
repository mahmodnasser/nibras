# Slice template

One slice is one use case, built through every layer it touches, finished in one to three days, delivered as one pull request. Copy this block into `docs/plan/34-work-breakdown.md` under its capability.

If filling this in takes longer than a few minutes, the slice is too big. Split it by use case, never by layer.

---

```
### SL-<AREA>-<NNN> <What a person can do when this is finished>

**Capability:** CAP-<AREA>-<NN> · **Phase:** <0-6> · **Service:** <from Appendix L> · **Estimate:** <1-3> days

**Covers**
- Requirements: REQ-<AREA>-<NNN>, ...
- Rules: BR-<AREA>-<NNN>, ...          (the worked examples become the unit tests)
- Workflow transitions: WF-<AREA>-<NN> <state> to <state>, ...
- Test cases: TC-<AREA>-<NNN>, ...

**Depends on:** <a published contract, not another slice, wherever possible>
**Blocks:** <slices that cannot start until this contract exists>

**Done when**
- [ ] Domain and application logic, unit tests from the rules above
- [ ] Persistence, migration, tenant filter, row-level security policy
- [ ] Endpoint with its permission declared, validation, error codes, OpenAPI
- [ ] Events published through the outbox; consumers idempotent through the inbox
- [ ] Integration tests: tenant isolation, permission matrix, query budget
- [ ] Screen or mobile surface, if this slice has one, English and Arabic, right to left
- [ ] Every state: loading, empty, error, offline, processing, no permission
- [ ] Traceability updated; the identifiers above all point at each other
- [ ] Demonstrated to someone who did not write it

**Not in this slice:** <the nearest thing a reader would assume is included, and is not>
```

---

## How to size one

| Signal | What it means |
|---|---|
| The name is a layer, a table, or a technology | Not a slice. Rename it as something a person can do, then re-split |
| The estimate is over three days | Two slices that have not been separated |
| It cannot be demonstrated when finished | Split the wrong way |
| It has no requirement, rule or workflow identifier | Scope nobody asked for. Find the identifier or drop it |
| It touches more than one service | Usually two slices, one per service, joined by a published contract |
| Everything is a checkbox and nothing is a behaviour | The done list was copied without thinking about this use case |

## Worked example

```
### SL-ATT-003 A teacher submits a class register and it is stored

**Capability:** CAP-ATT-01 · **Phase:** 2 · **Service:** Attendance · **Estimate:** 2 days

**Covers**
- Requirements: REQ-ATT-002, REQ-ATT-003
- Rules: BR-ATT-001 daily versus per-period derivation, BR-ATT-004 lock window
- Workflow transitions: WF-ATT-01 Open to Marked, Open to Locked
- Test cases: TC-ATT-003, TC-ATT-004, TC-ATT-009

**Depends on:** the session aggregate and its contract from SL-ATT-001
**Blocks:** SL-ATT-005 register screen, SL-ATT-006 mobile register

**Done when**
- [x] Handler with the lock-window rule, unit tests from all three worked examples in BR-ATT-004
- [x] Migration with the covering index on (tenant_id, section_id, date), row-level security policy
- [x] POST /api/v1/attendance/sessions declaring attendance.student-attendance.mark, Problem Details with ATTENDANCE_SESSION_LOCKED
- [x] attendance.attendance.marked.v1 through the outbox, partition key sectionId
- [x] Integration tests: another tenant is refused, a teacher without the permission is refused, the handler stays under five database commands
- [ ] No screen in this slice
- [ ] Traceability updated

**Not in this slice:** the register screen, offline queueing, and the unmarked-class reminder. Each is its own slice.
```

Two days, one pull request, one thing a person can do, and every claim in it points at something that can be checked.
