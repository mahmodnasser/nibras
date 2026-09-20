# `BR-<AREA>-<NNN>`: `<Rule stated as one sentence>`

> Copy into `docs/brief/02-appendices/appendix-s-business-rules.md`. At least three worked examples, including a boundary and an awkward case. Recompute every example before review.

**Owning service** `<Service>` · **Requirement IDs** `<REQ-AREA-NNN>` · **Test class** `<NameTests>`

## Statement

`<One sentence, active voice, naming who or what the rule applies to.>`

## Inputs

| Input | Type | Unit | Source | Notes |
|---|---|---|---|---|
| `<name>` | `<decimal | int | percentage>` | `<currency | points | days>` | `<where it comes from>` | `<fraction or whole number>` |

## Computation

| Step | Operation | Applies to |
|---|---|---|
| 1 | `<operation>` | `<which lines>` |

**Order of operations:** `<state it explicitly; a different order gives a different answer>`

**Rounding:** `<where, to how many places, which direction, per line or per total>`

## Worked examples

| # | Given | Computation | Expected |
|---|---|---|---|
| 1 | `<ordinary case>` | `<working>` | `<result>` |
| 2 | `<boundary case>` | `<working>` | `<result>` |
| 3 | `<awkward case: rounding or scope>` | `<working>` | `<result>` |

Example 2 exists to pin `<the boundary>`. Example 3 exists to pin `<the rounding point or the scope>`.

## Rejected inputs

| Input | Error code | Message shown |
|---|---|---|

## Interactions

| Interacts with | How | Order |
|---|---|---|

## Where this rule is quoted

| Surface | Must show the same number |
|---|---|