---
name: because-panel-pattern
description: The pattern for showing why an automated decision was made and allowing a reasoned override. Load whenever a screen shows a score, ranking, flag, suggestion, or automatic assignment.
---

# The because panel

Section 25 makes AI assistive, never authoritative, and Section 12 requires that a risk flag always shows why. This is the pattern that satisfies both, and it applies to plain rules as much as to models. A rule-based score is just as opaque to a teacher as a neural network.

## The contract

Every automated decision shown to a user carries four things:

1. **The verdict**, in plain language. "At risk of falling behind in mathematics", not "0.82".
2. **The reasons**, ranked, each a fact the user can verify, each linking to the record it came from. Three to five. A list of twelve is not an explanation.
3. **The counterfactual.** What would change this. "Two more attended lessons this week would clear this flag."
4. **The override**, which requires a reason, records who and when, and takes effect immediately and visibly.

## Rules

- **Never show a bare score**, and never show a percentage as if it were a measurement.
- **Reasons come from the same computation that produced the verdict.** A separately generated explanation is a story, not a reason, and it will eventually contradict the verdict.
- **The override is not a dismissal.** The reason is stored, is visible to the next viewer, and feeds the review of the rule itself.
- **Absence of data is a reason too.** "No behavior records this term" belongs in the panel, because it tells the teacher how much to trust it.
- **The panel is bilingual and screen-reader friendly.** Reasons are a list, not a paragraph of generated text.

## Worked example

> **Layla is flagged for follow-up in Mathematics** *(flagged 2 days ago, by the early-warning rule)*
>
> **Because**
> 1. Missed 4 of the last 10 mathematics lessons — *view attendance*
> 2. Last two assessment scores fell from 72% to 51% — *view marks*
> 3. Two assignments not submitted this term — *view coursework*
> 4. No behavior records this term, so behavior did not affect this flag
>
> **This would clear if** attendance returns to 8 of 10 lessons and one outstanding assignment is submitted.
>
> **Buttons:** `Start intervention` · `Not a concern (reason required)` · `Who can see this`

The override dialog asks "Why is this not a concern?" with a free-text field and three common reasons. The stored reason appears to the next teacher who opens the flag, and the monthly rule review reads every override reason, because a rule that is overridden half the time is a broken rule.