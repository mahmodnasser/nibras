# Review Guide for the Product Owner

The plan arrives in six groups. You approve each one before the next is written, which is what stops a large mistake from being discovered at the end. This guide says what to look for, what questions to ask, and what approval means.

You do not need to read every word. You need to answer one question per group: **would a competent engineer build the right product from this?**

---

## What approval means

Approval is a written note in `docs/project/PROJECT_STATE.md` naming the group, the date, and any conditions. A verbal yes is a signal to write that note, not a substitute for it. Approving a group means the next group may be written on top of it; it does not mean the group can never change, but a change after approval needs a decision record.

---

## Before Group A: the decisions workshop

Master brief Section 27 lists the decisions, and `docs/project/OPEN_QUESTIONS.md` holds **30 open questions**, each with a default. Questions 27 to 31 were added during the scorecard remediation and **four of the five are still open**: 27 absence-alert timing, 28 the Tier 1 public API, OneRoster and iCal, 29 whether a student's Wellbeing check-in answer may wait on the phone, and 31 the Appendix B permission amendment the kindergarten daily sheet waits on. Question 30, which student count bills a school, was decided by the product owner on 2026-09-26 with the recommended answer (master brief Section 36) and ADR-0027, Accepted. `docs/plan/01-questions-and-assumptions.md` gives each question its impact, likelihood, score and risk.

**Every default lets the plan proceed, but one of them breaks a rule of the brief until you decide it.** Question 29's default is what the plan builds today, an encrypted outbox on the phone that holds the student's answer code until it syncs. That contradicts the rule that wellbeing data never reaches a device. The recommended answer is online only. It is RISK-47, the highest-scoring risk in the register, so it is on the agenda, and the privacy officer takes it with you. Answering any question changes the default; silence accepts it.

One 90-minute session covers twenty-three questions:

| Minutes | Topic | Questions |
|---|---|---|
| 0 to 15 | Commercial shape | 2 SaaS or on-premises, 8 pricing, 25 launch date, 26 certifications |
| 15 to 35 | Market | 3 countries, 9 regional plug-ins, 11 languages, 12 the name, 15 hosting and regions |
| 35 to 55 | Scope | 4 Tier 2 priorities, 10 nursery, 5 assist rungs, 18 enterprise identity, **29 the Wellbeing check-in on the phone, with the privacy officer** |
| 55 to 75 | Delivery | 24 team, 14 Apple build capacity, 16 Windows hosts, 20 support hours |
| 75 to 90 | Technical, for information | 1 runtime, 6 messaging, 7 merging, 13 identity, 23 mobile state |

The last block is for information. Questions 6, 7, 13 and 23 are the architect's to decide. Question 1 is yours, on the architect's recommendation. They are on the agenda so you know what was chosen and why.

**Seven questions are answered in writing, not at the workshop:** 17 devices without Google services, 19 retention per country, 21 payment gateway, 22 SMS provider, 27 absence-alert timing, 28 the Tier 1 public API and 31 the kindergarten daily-sheet permission amendment. You answer them in `OPEN_QUESTIONS.md` before you write the Group A approval note. Question 27 must be answered before the first phase 2 attendance slice at the latest, question 28 before the MVP cut line is confirmed, and question 31 before Phase 2 starts.

**Decision records awaiting you.** Every record in `docs/project/DECISIONS/` except ADR-0019 and ADR-0027 is still Proposed, awaiting your confirmation. The plan builds on each one as a default in force; `01-questions-and-assumptions.md` section 6.1 lists the six that closed questions from v8. The workshop is the place to accept or overturn them, and each acceptance is written in the record's Status line.

Record the answers in `OPEN_QUESTIONS.md`, and sign off at the bottom of this file.

---

## Group by group

### Group A: summary, questions, competitive picture

**What to look for.** Does the executive summary describe the product you actually want to sell? Is the competitive analysis honest about what other products do better?

**Five questions.**
1. Is anything in the summary a surprise to me?
2. Which of the gaps in the competitive analysis would lose us a deal?
3. Are the assumptions the plan made in my absence ones I would have made?
4. Is the phase roadmap fast enough to matter and slow enough to be true?
5. What would I cut to ship three months sooner?

### Group B: requirements, architecture, service catalog, repository structure

**What to look for.** Every requirement traceable to something in the brief; a service catalog where each boundary has a reason; a repository structure a new engineer could navigate.

**Five questions.**
1. Is any requirement here something I never asked for?
2. Is anything I asked for missing? Check the traceability matrix for my top ten features.
3. Does each service exist for a reason I would repeat to an investor?
4. What happens to the plan if we merge services for the first release?
5. Which requirement is the riskiest, and does the plan say so?

### Group C: every service sheet, data, messaging, performance

The largest group and the one an engineer will actually build from. You are not checking the detail; you are checking the shape.

**Five questions.**
1. Pick two services at random. Could someone build them from these sheets without asking me anything?
2. Does the data a parent can see match what I would promise a parent?
3. Does anything cross a boundary I care about, for example wellbeing data appearing where it should not?
4. Are the performance numbers ones I would be happy to see in a contract?
5. What is the plan for the morning peak, in one sentence?

### Group D: web, mobile, security, workflows, design

**What to look for.** Screens that match the personas; workflows that match how a school actually runs; security that survives a hard question.

**Five questions.**
1. Walk me through a teacher's Monday morning on this design. Does it take five minutes?
2. Show me the Arabic version of the three screens I care most about.
3. Which workflow here does not match how schools I know actually work?
4. If a parent asked who has seen their child's medical note, can we answer?
5. What can a determined insider do that they should not?

### Group E: operations, testing, roadmap, risk, dependencies, traceability

**What to look for.** Honest risk, real gates, and a traceability matrix with no empty test cells.

**Five questions.**
1. Which risk would actually stop us, and who owns it?
2. What does the plan say we will not do?
3. If the database was lost at noon, what do we lose and how long until we are back?
4. Are there requirements with no test? Show me the empty cells.
5. What does this cost to run for a thousand students?

### Group F: conventions, integrations, localization, assist, migration, compliance, capacity, platform

**What to look for.** The parts that get skipped and then hurt: deprecation policy, compliance, cost, and what happens on a Windows host or an old phone.

**Five questions.**
1. If we change the public API, how long do integrators have?
2. Which compliance claim could we not defend in an audit today?
3. What does the product do when every model is switched off?
4. What is the oldest phone we support, and have we tried it?
5. Does the demo in Appendix O still show the product I want to sell?

---

## How to give feedback that can be acted on

| Instead of | Say |
|---|---|
| "This feels too complex" | "Service X and service Y look like one thing to me. Why are they two?" |
| "Make it faster" | "Attendance must be under a minute on a three-year-old Android. Is it?" |
| "I do not like this screen" | "A parent opening this should see one thing first. Right now they see four" |
| "Add more detail" | "Section 3 does not tell me what happens when the payment fails" |
| "This is wrong" | "This contradicts what we agreed about groups being one tenant" |

A reference to a specific section, requirement identifier or persona turns into a change. An adjective turns into a guess.

---

## Sign-off record

| Group | Date | Approved by | Conditions |
|---|---|---|---|
| Decisions workshop | | | |
| A | | | |
| B | | | |
| C | | | |
| D | | | |
| E | | | |
| F | | | |
