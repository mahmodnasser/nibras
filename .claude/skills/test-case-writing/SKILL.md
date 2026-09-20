---
name: test-case-writing
description: How to write test cases that would actually catch the failure. Load before writing a test plan, a test case catalog entry, or the tests for a rule or workflow.
---

# Test case writing

A test exists to fail when the product is wrong. A test that cannot fail is worse than no test, because it buys confidence without providing it.

## Identifier and shape

`TC-<AREA>-<NNN>` from the canonical registry, in the format `docs/brief/02-appendices/appendix-v-coverage-matrix.md` defines. Each case states: the requirement or rule it covers, the level it runs at, the precondition, the action, the expected result **including the data**, and the one thing it would catch.

## Choose the level deliberately

| Concern | Level |
|---|---|
| A business rule's arithmetic | Unit, against the domain |
| Authorization and data scope | Integration, against the real endpoint |
| Tenant isolation | Generated integration suite, every endpoint and consumer |
| Message contract | Contract test, publisher and consumer |
| A workflow transition | Integration, driving the state machine |
| A screen flow | One end-to-end test, not twenty |

## Rules

- **Assert the data, not the status code.** `200 OK` with the wrong student's marks passes a status assertion.
- **Every authorization test has a denied twin.** Every validation has a rejected input.
- **Pin the clock, the culture, and the time zone.** A test that passes only in June, or only in a Latin locale, is a scheduled failure.
- **Order is asserted, never assumed.** Without `ORDER BY`, row order is a coincidence.
- **No mock of the thing under test.** Mock the boundary, not the subject.
- **Test the boundary values.** Exactly at the threshold, one below, one above, zero, and empty.

## Worked example

**TC-ATT-014.** Covers REQ-ATT-014 and BR-ATT-002 (a session locks 24 hours after its date).

| Field | Value |
|---|---|
| Level | Integration, real endpoint, real database |
| Precondition | Tenant A, section 4B, session dated 2026-03-02, clock pinned to 2026-03-03 09:00 UTC, teacher with `attendance.student-attendance.mark` |
| Action | `POST /sections/4b/attendance` with one present mark |
| Expected | `409 Conflict`, body error code `ATTENDANCE_SESSION_LOCKED`, and the stored marks are **unchanged**: re-read the session and assert the original three marks with their original values |
| Would catch | A lock check that runs before validation but not before persistence, so the write lands and the error is returned afterwards |

Its twins: `TC-ATT-015` at 2026-03-02 23:59 (accepted, one minute inside the window) and `TC-ATT-016` at exactly 24 hours (rejected, because the boundary is inclusive of the lock). Three cases, because the boundary is where the defect lives.