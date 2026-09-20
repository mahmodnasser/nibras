# Review Guide for the Product Owner

The plan arrives in six groups. You approve each one before the next is written, which is what stops a large mistake from being discovered at the end. This guide says what to look for, what questions to ask, and what approval means.

You do not need to read every word. You need to answer one question per group: **would a competent engineer build the right product from this?**

---

## What approval means

Approval is a written note in `docs/project/PROJECT_STATE.md` naming the group, the date, and any conditions. A verbal yes is a signal to write that note, not a substitute for it. Approving a group means the next group may be written on top of it; it does not mean the group can never change, but a change after approval needs a decision record.

---

## Before Group A: the decisions workshop

Master brief Section 27 lists the decisions, and `docs/project/OPEN_QUESTIONS.md` holds 26 questions, each with a default. **Every default is safe enough to proceed on**, so nothing here blocks. Answering changes the default; silence accepts it.

One 90-minute session covers it:

| Minutes | Topic | Questions |
|---|---|---|
| 0 to 15 | Commercial shape | 2 SaaS or on-premises, 8 pricing, 25 launch date, 26 certifications |
| 15 to 35 | Market | 3 countries, 9 regional plug-ins, 11 languages, 12 the name, 15 hosting and regions |
| 35 to 55 | Scope | 4 Tier 2 priorities, 10 nursery, 5 assist rungs, 18 enterprise identity |
| 55 to 75 | Delivery | 24 team, 14 Apple build capacity, 16 Windows hosts, 20 support hours |
| 75 to 90 | Technical, for information | 1 runtime, 6 messaging, 7 merging, 13 identity, 23 mobile state |

The last block is the architect's to decide; it is on the agenda so you know what was chosen and why, not so you choose it.

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
