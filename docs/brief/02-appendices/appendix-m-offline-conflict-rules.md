# Appendix M. Offline Behaviour and Conflict Rules

> Part of `docs/brief/02-appendices/`. Normative. Read with the master brief and the reference architecture.

Master brief Section 18 requires offline-first behaviour with "defined conflict rules per entity" and never defined them. Offline sync is the highest-risk unspecified area in a school product, because the failure is silent: a teacher marks attendance in a corridor, the register looks right on the phone and wrong in the office, and nobody finds out until a parent calls. This appendix removes the ambiguity.

---

## M.1 What works offline, and what does not

| Capability | Offline | Why |
|---|---|---|
| View today's timetable | Yes, from cache | Pre-loaded at sync |
| Mark attendance for a class | Yes, queued | The reason offline exists |
| Enter marks for a component | Yes, queued, draft only | Drafts are local until submitted; approval never happens offline |
| Read messages and announcements | Yes, last 30 days | Cached |
| Send a message | Yes, queued | Delivered on reconnect, with a visible pending state |
| Submit an excuse or a request with an attachment | Yes, queued | The attachment uploads on reconnect |
| Record a behaviour point or incident | Yes, queued | |
| Record a clinic visit | **No** | Wellbeing data is never stored on a device |
| View a wellbeing record | **No** | Same rule |
| Approve anything | **No** | Approval needs the current permission version and the current state |
| Issue or verify a gate pass | Verify yes, issue no | Verification uses a signed payload that works offline; issuing needs server state |
| Emergency roll call | Yes, queued, and it says so | Safety work must not stop for a network |
| Make a payment | **No** | Payment goes to the gateway, never through a queue |

**The device cache holds nothing classified sensitive in Appendix J.** No custody text, no medical detail, no counseling note, no payment instrument, no credential beyond the token in secure storage.

---

## M.2 The sync contract

**Delta token.** An opaque string the server issues with every sync response: `v1.<tenantId>.<entityGroup>.<checkpointLsn>.<hmac>`. The client sends it back to get everything that changed since. It is signed so a tampered token is rejected, and it is valid for 30 days, which is the longest an app may sit unopened before a full refresh is required.

**Outbox on the device.** Every offline action is queued with a client-generated UUID v7 as its **idempotency key**, the `occurredAt` from the device clock, and the entity version the device held. The server stamps `receivedAt` on arrival. Ordering uses `receivedAt`, never the device clock, because device clocks are wrong often enough to matter.

**Replay is safe.** The server treats the idempotency key as an inbox key. A queued action delivered twice produces one result and the second returns the first response.

**Visible state, always.** Every queued item shows as pending on the screen that created it. The application never pretends an action is complete when it is queued. A sync failure is shown, with a retry and a plain explanation of what has not reached the school.

**Payload limits.** One sync request carries at most 5 MB or 500 queued actions, whichever comes first, and continues with the next batch. Attachments upload separately with resumable transfer, and the action they belong to stays pending until its attachment lands.

---

## M.3 Conflict rules, per entity

A conflict is when the server state changed after the device read it and before the queued action arrived. Every rule below is implemented in the owning service and has a test.

| Entity | Rule | If the rule discards the device value |
|---|---|---|
| **Attendance record, before the lock window closes** | Device wins. The teacher in the room is the authority | The previous server value is kept in the change history with both values |
| **Attendance record, after the lock window closed** | Server wins. The device action becomes a pending **edit-after-lock request** with the original timestamp | The teacher is told, and the request appears in their tasks with one tap to submit |
| **Attendance, two devices for the same session** | Last `receivedAt` wins per student, not per session, so two teachers marking different halves both succeed | Per-student merge means neither loses work |
| **Mark, draft** | Device wins while the component is unapproved | Change history records both |
| **Mark, component approved or locked** | Server wins. The device value is offered as a grade-change request | The teacher sees a conflict banner showing both values side by side |
| **Message** | Append-only. Never a conflict | not applicable |
| **Excuse or request submission** | Append-only. A duplicate submission within five minutes for the same subject and type is collapsed | The user sees one request |
| **Behaviour incident** | Append-only | not applicable |
| **Behaviour points** | Additive. Two devices awarding points both apply | not applicable |
| **Student profile edit from a device** | Server wins on any field the school changed meanwhile; the device change becomes a change request | The person is told which fields were kept |
| **Emergency acknowledgement or roll call** | Append-only, and the earliest `occurredAt` wins for "when were you accounted for" | Duplicates collapse |
| **Gate pass verification** | Append-only. The pass is single-use, so the first verification to reach the server wins and later ones are reported | The second verifier is told the pass was already used, with the time |
| **Read receipt** | Earliest `occurredAt` wins | Silent |
| **Notification preference change** | Last `receivedAt` wins | Silent |
| **Task completion** | First completion wins; a second is ignored | Silent |

**The banner rule.** Whenever a rule discards a value a person typed, the application shows it. It names the field, shows both values, says which was kept and why, and offers the one action that resolves it. Silently discarding a teacher's work is the failure this appendix exists to prevent.

---

## M.4 Worked examples

- **Given** a teacher marks 4B present offline at 08:05 and the office marks one student absent at 08:10, **when** the phone syncs at 08:40 and the lock window closes at 09:00, **then** the teacher's values win per student, the office change to that one student is overwritten, and both values appear in the change history with who and when.
- **Given** the same teacher syncs at 09:30 after the 09:00 lock, **then** nothing is overwritten, an edit-after-lock request is created carrying the original 08:05 timestamps, and the teacher is shown one task to submit it.
- **Given** two devices each queue an emergency acknowledgement for the same staff member at 10:02:14 and 10:02:51, **when** both arrive, **then** one acknowledgement exists with `occurredAt` 10:02:14, and the roll-call count is not double-counted.
- **Given** a phone has been closed for 45 days, **when** it opens, **then** the delta token has expired, the application performs a full refresh for that entity group rather than applying a stale delta, and the queued outbox is still sent because its idempotency keys do not expire.

---

## M.5 Tests every conflict rule owes

| Test | Proves |
|---|---|
| Deliver the same queued action twice | One result, second returns the first response |
| Queue on device A and change on the server, then sync | The rule in M.3 applies and the banner shows |
| Queue on two devices for the same session | Per-student merge, no lost work |
| Sync after the lock window | A request is created, nothing is overwritten |
| Sync with a 45-day-old token | Full refresh, outbox still delivered |
| Device clock set two hours fast | Ordering uses `receivedAt`; `occurredAt` is preserved for display only |
| Attachment upload interrupted | The action stays pending; retry resumes rather than duplicating |
| Airplane mode for a full school day, then reconnect | Everything queued arrives, in order, once |
