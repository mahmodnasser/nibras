# Appendix W. Signature Feature Register

> Part of `docs/brief/02-appendices/`. Normative. Read with the master brief and the reference architecture.

Master brief Sections 12 and 12.1 name 44 signature features. This register is the machine-checkable version: every feature declares the **persona moment** it creates, the **assist rung** it runs at, its **autonomy level**, its **tier**, the **service** that owns it, and the **demo test case** that proves it can be shown. `tools/kit-lint` fails when a row is missing a rung, a tier or a moment, because a feature without those three is an aspiration.

**Feature 39 is no longer a signature feature.** Under ADR-0019, calendar-aware scaling moved to the engineering capabilities in Appendix A, section A1, because it cannot be shown on demo data in a meeting. It is still built and still measured, by Appendix N scenarios N-01 and N-11. Its row stays below, marked as moved, so the numbers of the other 43 features do not change.

**Assist rungs** come from master brief Section 25. Rung 1 is deterministic rules and smart defaults and is always available. Rung 2 is a classical model. Rung 3 is a local language model. Rung 4 is an optional external provider. A feature listed at rung 2 or above states what it degrades to.

**Autonomy levels** are the second, separate scale in master brief Section 25, and each feature's level is copied from `docs/plan/25-ai-and-assist-ladder.md` section 2. Level 1 surfaces information the person already had a right to see. Level 2 suggests: it ranks or flags with visible reasons, and the person decides. Level 3 drafts text or a plan that a person approves before it takes effect. Level 4 acts inside a written policy, with an audit entry and a one-click undo, and never over a grade, a payment or a message to a family. The rung says what technology a feature needs; the autonomy level says how far it acts on its own.

**Test identifiers.** Under ADR-0019 the demo test cases for features 10, 15, 29, 36, 37, 40 and 41 were renumbered to TC-PLT-805, TC-PLT-801, TC-PLT-802, TC-DOC-801, TC-PLT-803, TC-INF-801 and TC-PLT-804. TC-PLT-001 to TC-PLT-005 keep their Appendix R meaning (the WF-PLT-01 transitions), TC-INF-001 to TC-INF-006 keep their Appendix R meaning (the WF-INF-01 transitions), and TC-DOC-002 keeps its Appendix O meaning (QR verification, minute 9). Under ADR-0020 the demo test cases for features 3, 6, 8, 18, 19, 20, 26, 30, 32, 33, 42 and 43, which reused identifiers Appendix R gives to workflow transitions, moved to the 810 range: TC-ATT-810, TC-ASM-810, TC-ATT-811, TC-BEH-810, TC-ACA-810, TC-SCH-810, TC-ATT-812, TC-WEL-810, TC-ATT-813, TC-OPS-810, TC-ASM-811 and TC-HR-810. Appendix R keeps TC-ATT-001 to 004, TC-ASM-001 and 002, and the other 001 identifiers. Kit-lint rule R20 now refuses an identifier defined in two documents.

**The sixty-second rule.** A feature belongs here only if it can be demonstrated on the demo data in under a minute. Everything else is a capability in Appendix A. The demo director agent enforces this before a feature is accepted into the register.

| # | Feature | Moment | Rung | Autonomy | Tier | Owner | Demo |
|---|---|---|---|---|---|---|---|
| 1 | Today dashboards where every card leads to an action | "I can see what needs me and do it here" | 1 | 1 surfaces | 1 | Reporting | TC-RPT-001 |
| 2 | Student 360 timeline filtered by the viewer's permissions | "The whole child, on one screen" | 1 | 1 surfaces | 1 | Bff.Web | TC-RPT-002 |
| 3 | Sixty-second attendance | "Done before the bell stopped" | 1 | 1 surfaces | 1 | Attendance | TC-ATT-810 |
| 4 | Early warning with explanation and an intervention | "I know who is slipping and what to do" | 2, degrades to rule thresholds | 2 suggests | 1 | Reporting | TC-RPT-003 |
| 5 | Smart timetable with live conflict detection | "A term's work in an afternoon" | 1 | 1 surfaces | 1 | Scheduling | TC-SCD-001 |
| 6 | Report Card Studio with QR verification | "Eight hundred report cards, verified" | 1, comments at 3 | 3 drafts | 1 | Assessment | TC-ASM-810 |
| 7 | Parent experience that respects attention | "One calm message, not twelve" | 1 | 1 surfaces | 1 | Notification | TC-NOT-001 |
| 8 | Safety and dismissal with gate passes | "I know who collected the child" | 1 | 1 surfaces | 1 | Attendance | TC-ATT-811 |
| 9 | Go live in a day: import with dry run and rollback | "We moved in a morning" | 1 | 1 surfaces | 1 | Documents | TC-DOC-001 |
| 10 | Configurable without code | "We changed it ourselves" | 1 | 1 surfaces | 1 | Platform | TC-PLT-805 |
| 11 | Command palette and natural-language search | "I typed what I wanted" | 1, natural language at 3 | 1 surfaces | 1 | Bff.Web | TC-WEB-001 |
| 12 | Offline-first mobile | "The corridor has no signal and it still worked" | 1 | 1 surfaces | 1 | Attendance | TC-MOB-001 |
| 13 | Data quality center | "The list was wrong and the system told us" | 1 | 1 surfaces | 1 | Reporting | TC-RPT-004 |
| 14 | Trust by design: audit, consent, export, erasure | "We can answer any question about any record" | 1 | 1 surfaces | 1 | Audit | TC-AUD-001 |
| 15 | Sales-ready demo mode with one-click reset | "Show it to the next school in ten minutes" | 1 | 1 surfaces | 1 | Platform | TC-PLT-801 |
| 16 | White-label mobile apps | "It is our school's app" | 1 | 1 surfaces | 2 | Platform | TC-MOB-002 |
| 17 | Photo and media consent enforced at publishing | "That child's photo cannot be posted" | 1 | 1 surfaces | 1 | Communication | TC-COM-001 |
| 18 | Student portfolio and recognition | "Everything they achieved, theirs to keep" | 1 | 1 surfaces | 2 | Behavior | TC-BEH-810 |
| 19 | Kindergarten daily sheet | "I know how the day went" | 1 | 1 surfaces | 2 | Academics | TC-ACA-810 |
| 20 | Balanced class formation | "Fair classes in one pass, then adjusted by hand" | 1 | 1 surfaces | 2 | School | TC-SCH-810 |
| 21 | Inspection and accreditation readiness | "The evidence folder filled itself" | 1 | 1 surfaces | 2 | Reporting | TC-RPT-005 |
| 22 | Policy and handbook acknowledgment | "Everyone signed, and we can prove it" | 1 | 1 surfaces | 2 | Communication | TC-COM-002 |
| 23 | Live, animated, modern interface | "It does not feel like school software" | 1 | 1 surfaces | 1 | Web | TC-UX-001 |
| 24 | Open by default: API, webhooks, iCal, standards | "It talks to what we already use" | 1 | 1 surfaces | 2 | Platform | TC-INT-001 |
| 25 | Morning brief per role | "I knew my day before I arrived" | 1, phrasing at 3 | 1 surfaces | 1 | Reporting | TC-RPT-006 |
| 26 | Exception-only attendance | "The register was already half filled" | 1 | 4 acts, the pre-fill only | 1 | Attendance | TC-ATT-812 |
| 27 | Explain this number | "I could show the parent exactly why" | 1 | 1 surfaces | 1 | Reporting | TC-RPT-007 |
| 28 | Because panel on every automated action | "It told me why, and I could disagree" | 1 | 1 surfaces | 1 | Reporting | TC-RPT-008 |
| 29 | Smart defaults engine at onboarding | "It already knew how our year works" | 1 | 3 drafts | 1 | Platform | TC-PLT-802 |
| 30 | Intervention playbooks | "A flag became a plan with an owner" | 1 | 3 drafts | 1 | Wellbeing | TC-WEL-810 |
| 31 | Guardian transparency on sensitive access | "I can see who looked at my child's file" | 1 | 1 surfaces | 1 | Audit | TC-AUD-002 |
| 32 | Emergency mode with reunification | "Every child accounted for in four minutes" | 1 | 1 surfaces | 1 | Attendance | TC-ATT-813 |
| 33 | Campus digital twin | "I saw the empty rooms at a glance" | 1 | 1 surfaces | 2 | Operations | TC-OPS-810 |
| 34 | Teacher five-minute mode | "Four taps and I was teaching" | 1 | 1 surfaces | 1 | Bff.Mobile | TC-MOB-003 |
| 35 | Parent calm screen | "Nothing needed me today, and it said so" | 1 | 1 surfaces | 1 | Bff.Mobile | TC-MOB-004 |
| 36 | School memory: portfolio and yearbook | "The yearbook made itself" | 1 | 1 surfaces | 2 | Documents | TC-DOC-801 |
| 37 | Template exchange between schools | "We started from someone else's good work" | 1 | 1 surfaces | 2 | Platform | TC-PLT-803 |
| 38 | Plug-in kit for regional integrations | "Our partner built the ministry export" | 1 | 1 surfaces | 2 | Platform | TC-INT-002 |
| 39 | Calendar-aware scaling: moved to engineering capabilities (ADR-0019) | None. No longer a signature feature; built and measured as an engineering capability in Appendix A, A1 | 1 | 1 surfaces | 1 | Platform | None; measured by Appendix N scenarios N-01 and N-11 |
| 40 | Self-healing operations | "It fixed itself and told us what it did" | 1 | 4 acts | 1 | Platform | TC-INF-801 |
| 41 | Configuration as code | "We reviewed the change before it went live" | 1 | 1 surfaces | 1 | Platform | TC-PLT-804 |
| 42 | Mastery and next step | "I saw the gap and what to teach next" | 2, degrades to the raw heatmap | 2 suggests | 2 | Assessment | TC-ASM-811 |
| 43 | Workload balance for staff | "Someone noticed before I burned out" | 2, degrades to load totals | 2 suggests | 2 | Hr | TC-HR-810 |
| 44 | Low-bandwidth mode | "It worked on my old phone" | 1 | 1 surfaces | 1 | Bff.Mobile | TC-MOB-005 |

---

## What each rung owes the person using it

| Rung | Must show | Must offer | Must survive |
|---|---|---|---|
| 1 | The rule identifier and the inputs that produced the result | A manual override with a recorded reason where the result is consequential | Everything. Rung 1 has no external dependency |
| 2 | The contributing factors and their relative weight, never a bare score | The same override, plus a way to mark the prediction wrong so the model is reviewed | Model unavailable: fall back to the threshold rule and say so |
| 3 | The sources the draft drew on, and that a person must review it | Edit before send, always. Nothing generated reaches a family unreviewed | Model unavailable: an empty field with a template and guidance |
| 4 | Everything rung 3 owes, plus exactly what left the school's infrastructure and to whom | Tenant-level consent before the first call, revocable at any time | Provider unavailable or consent withdrawn: fall back to rung 3, then rung 1 |

## Features deliberately not in this register

Naming what was considered and rejected is as useful as naming what was built.

| Considered | Decision | Reason |
|---|---|---|
| Behavioural tracking of students for engagement scoring | Rejected | Master brief Section 20 forbids it. Children are not a funnel |
| Location tracking of children | Rejected | Vehicles are tracked, children are not. Section 20 |
| Automatic grade prediction shown to parents | Rejected for Tier 1 | A predicted grade shown to a family becomes a promise. Predicted bands stay internal and Tier 2 |
| An AI that answers a parent directly without review | Rejected | Section 25: nothing reaches a family unreviewed |
| Public student profiles or a student directory | Rejected | Section 20 child safety |
| Gamified leaderboards that cannot be opted out of | Rejected | House points exist; public ranking of individuals is opt-out by design |
