# Ideas

A scored backlog of product ideas that are not yet committed. `/ideate` adds to it, the product-innovator agent scores, and `/retro` folds in what reviews reveal. An idea leaves this file in one of three directions: into Appendix W as a signature feature, into Appendix A as a capability, or into the rejected table with a reason.

**Scoring**, each 1 to 5: value to a named persona, effort inverted so 5 is cheap, risk inverted so 5 is safe, distinctiveness, privacy fit. Total out of 25.

**Two independent scales.** *Assist rung* is what technology it needs: 1 rules, 2 classical model, 3 local language model, 4 external provider. *Autonomy level* is how far it acts alone: 1 surfaces, 2 suggests, 3 drafts, 4 acts. Autonomy 4 is never permitted over a grade, a payment, or a message to a family.

---

## Open ideas

| ID | Idea | Persona | Value | Effort | Risk | Distinct | Privacy | Rung | Autonomy | Total | Status |
|---|---|---|---|---|---|---|---|---|---|---|---|
| I-01 | Parent wallet across fees, cafeteria and trips, with a family view | Parent | 5 | 2 | 3 | 3 | 4 | 1 | 2 | 17 | Parked for Tier 2 |
| I-02 | Substitution marketplace: staff opt in to cover, ranked by fairness | Teacher | 4 | 3 | 4 | 4 | 5 | 1 | 2 | 20 | Candidate for Tier 2 |
| I-03 | Anonymous concern reporting routed to the safeguarding officer | Student | 5 | 4 | 3 | 5 | 3 | 1 | 1 | 20 | **Promoted to Appendix W, feature 26 territory** |
| I-04 | Daily wellbeing check-in with trend escalation | Homeroom teacher | 4 | 4 | 2 | 4 | 2 | 1 | 2 | 16 | Needs a privacy review before it moves |
| I-05 | Timetable what-if simulator the coordinator can share for comment | Coordinator | 4 | 3 | 5 | 4 | 5 | 1 | 1 | 22 | Candidate for Tier 2 |
| I-06 | Fee forecast per family, showing what is due for the rest of the year | Parent | 4 | 4 | 4 | 3 | 5 | 1 | 1 | 21 | Candidate for Tier 1 |
| I-07 | Teacher handover pack when a class changes teacher mid-year | Teacher | 4 | 4 | 4 | 4 | 4 | 1 | 3 | 21 | Candidate for Tier 2 |
| I-08 | Printable offline day pack for a trip: register, contacts, medical flags | Trip lead | 5 | 4 | 3 | 4 | 3 | 1 | 1 | 20 | Candidate for Tier 2. Needs a retention rule for the printout |
| I-09 | Parent-to-parent class contact list, opt-in per parent | Parent | 3 | 4 | 2 | 2 | 2 | 1 | 1 | 13 | Parked. Child-safety review first |
| I-10 | Live occupancy from roll call shown on the campus map | Principal | 3 | 3 | 4 | 4 | 4 | 1 | 1 | 19 | Folded into feature 33 |
| I-11 | Exam paper version control with a secure print request | Coordinator | 4 | 3 | 4 | 3 | 5 | 1 | 2 | 19 | Already a workflow in Appendix R |
| I-12 | School-to-school transfer that moves records directly between tenants | Registrar | 5 | 2 | 2 | 5 | 2 | 1 | 3 | 16 | Parked. Needs consent and residency design first |
| I-13 | Attendance nudge to a parent before the absence becomes chronic | Parent | 4 | 4 | 3 | 3 | 3 | 2 | 2 | 19 | Candidate. Must show its reasons |
| I-14 | Staff wellbeing pulse, aggregated and never individual | HR officer | 3 | 4 | 3 | 3 | 4 | 1 | 1 | 18 | Parked for Tier 2 |
| I-15 | Automatic timetable repair after a room becomes unavailable | Coordinator | 4 | 2 | 3 | 4 | 5 | 1 | 3 | 19 | Candidate for Tier 2 |
| I-16 | Report card comment consistency check across a teacher's set | Teacher | 3 | 3 | 3 | 4 | 4 | 3 | 2 | 17 | Rung 3, so needs a rung 1 fallback before it moves |
| I-17 | Bulk parent meeting scheduling that respects sibling families | Registrar | 4 | 3 | 4 | 3 | 5 | 1 | 2 | 20 | Candidate for Tier 2 |
| I-18 | A read-only board pack generated from live data | School owner | 3 | 4 | 4 | 3 | 4 | 1 | 3 | 19 | Candidate for Tier 2 |
| I-19 | Supply and stock forecast from historical consumption | Store keeper | 2 | 3 | 4 | 2 | 5 | 2 | 2 | 18 | Parked for Tier 3 |
| I-20 | Alumni destination tracking for the school's own reporting | School owner | 2 | 3 | 3 | 2 | 3 | 1 | 1 | 14 | Parked for Tier 3 |

## Rejected, with the reason

| Idea | Reason |
|---|---|
| Engagement scoring of students | Master brief Section 20 forbids behavioural tracking of children. Not a close call |
| Location tracking of children | Vehicles are tracked; children are not |
| An assistant that answers a parent without review | Section 25: nothing reaches a family unreviewed |
| Public student profiles | Child safety |
| Predicted grades shown to families in Tier 1 | A prediction shown to a family becomes a promise. Internal and Tier 2 only |
| Automatic fee restriction with no human step | Withholding a report card from a child is never an automated decision |
| A leaderboard nobody can leave | House points exist; individual public ranking is opt-out by design |

---

## How an idea moves

1. `/ideate <area>` proposes and scores. Anything below 15 stays here unless someone argues for it.
2. The demo director checks it can be shown in under sixty seconds. If it cannot, it is a capability for Appendix A, not a signature feature.
3. `/differentiate <feature>` fills its row in Appendix P: who else has it, our edge, the persona moment, the proof.
4. A promoted idea gains a number in Appendix W with its rung, autonomy and tier, and a row in the demo script.
5. `/retro` after each review adds what the product owner asked for that nobody had thought of, which is usually the best source in this file.
