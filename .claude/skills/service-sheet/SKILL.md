---
name: service-sheet
description: How to write a complete service specification sheet for docs/plan/06-services/. Load before writing or reviewing any service sheet, and before judging whether one is finished.
---

# Service sheet

A sheet is finished when a senior engineer who has never seen this project could build the service from it without asking a question. Fourteen sections, in this order, from `docs/templates/service-sheet.md`.

## The test that matters

For each section ask: **could someone implement this without guessing?** A table with a column left empty is a guess waiting to happen. Write the default and name its owner instead.

## The four sections people get wrong

1. **Non-responsibilities.** Most boundary disputes come from what a service does *not* own. Write that column first; it is harder and more useful than the responsibilities column.
2. **The folder tree.** A real fenced tree, to file level for one example feature and to folder level for the rest, with a purpose comment per entry.
3. **The caching table.** Every row needs an invalidating event. A row with only a lifetime is a stale-data bug with a date on it.
4. **Hot queries.** Each query names its index, its expected row count, its pagination style, and its query budget per handler.

## Worked example: Attendance

**Responsibilities and non-responsibilities**

| Owns | Does not own |
|---|---|
| Attendance sessions, marks, excuses, thresholds, gate passes | The student record (School), the timetable (Scheduling), the message to the parent (Notification) |

**REST row**

| Method | Path | Permission | Request | Response | Errors | Idempotent |
|---|---|---|---|---|---|---|
| POST | `/sections/{id}/attendance` | `attendance.student-attendance.mark` | `MarkAttendanceRequest` | `202 Accepted` | `ATTENDANCE_SESSION_LOCKED`, `ATTENDANCE_OUT_OF_WINDOW` | Yes, by session id plus client token |

**Caching row**

| Data | Key | Tags | L1 | L2 | Invalidated by | Never cached |
|---|---|---|---|---|---|---|
| Section roster for marking | `nibras:{tenant}:attendance:roster:{sectionId}:v1` | `tenant`, `section` | 30 s | 10 min | `school.enrollment.changed.v1`, `scheduling.section.updated.v1` | Excuse reasons, medical notes |

**Hot query row**

| Query | Index | Rows | Pagination | Budget |
|---|---|---|---|---|
| Unmarked sessions today for a teacher | `(tenant_id, teacher_id, session_date) WHERE deleted_at IS NULL` | under 12 | none needed | 1 command, p95 under 40 ms |

**Events**

| Direction | Routing key | Payload | Ordering key |
|---|---|---|---|
| Publishes | `attendance.student.absent.v1` | student, section, date, reason class | student id |
| Consumes | `school.enrollment.changed.v1` | student, section, effective date | student id |

Cross-check the sheet against the service catalog, the message catalog, and the dependency matrix before calling it done. A disagreement is a defect in at least one of the four.