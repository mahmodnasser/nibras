# Scorecard: `<plan group or document>`

> Score 0 to 5 per dimension, each with evidence quoted from the document. Approve only when every dimension is 4 or above. An average does not substitute for a low score.

**Scored** `<date>` by `<who>` · **Covers** `<documents>`

## Scores

| Dimension | Score | Evidence (document:line, quoted) | What would raise it by one |
|---|---|---|---|
| Completeness | `<0-5>` | `<quote>` | `<change>` |
| Consistency | `<0-5>` | `<quote>` | `<change>` |
| Feasibility | `<0-5>` | `<quote>` | `<change>` |
| Risk honesty | `<0-5>` | `<quote>` | `<change>` |
| Testability | `<0-5>` | `<quote>` | `<change>` |
| Distinctiveness | `<0-5>` | `<quote>` | `<change>` |
| Portability | `<0-5>` | `<quote>` | `<change>` |

What each dimension means at 5:

| Dimension | 5 means |
|---|---|
| Completeness | Every item the plan specification requires is present and filled |
| Consistency | Names, identifiers, contracts, and counts agree with the canonical registry and every other document |
| Feasibility | A team of the stated size could build this in the stated time, and the document says how it knows |
| Risk honesty | The hard parts are named as hard, with likelihood, impact, and an owner |
| Testability | Every requirement has an acceptance criterion a test could assert, and a test case ID |
| Distinctiveness | Signature features are specified sharply enough to be built better than a competitor's version |
| Portability | Windows and Linux, LTR and RTL, web and mobile, and devices without Google services are addressed |

## Verdict

**`<Approved | Blocked>`** — blocking dimensions: `<list, or none>`

## Gaps

| Gap | Dimension | Effort to close | Blocking |
|---|---|---|---|

## Re-score after

| # | Change | Dimension it lifts |
|---|---|---|