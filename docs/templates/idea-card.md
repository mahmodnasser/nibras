# Idea: `<name>`

> Copy into `docs/project/IDEAS.md`. An accepted idea then earns a row in `docs/brief/02-appendices/appendix-w-feature-register.md`. An idea card that cannot name the moment it lands is not ready to be scored.

**Proposed** `<date>` by `<who>` · **Area** `<service or role>` · **Status** `<proposed | accepted | rejected | parked>`

## The promise

`<One sentence a school would repeat to another school.>`

## The problem it solves

| Question | Answer |
|---|---|
| Who has this problem | `<role>` |
| How often | `<frequency>` |
| What they do today | `<current workaround>` |
| What it costs them | `<time, errors, or trust>` |

## The moment it lands

`<A named person, a day in the school year, what they were doing, what changed. Not a feature description.>`

## Scores

| Axis | Score 1-5 | Reason |
|---|---|---|
| Value | `<n>` | `<why a school would change its buying decision, or not>` |
| Effort | `<n>` | `<one slice inside one service, or a new contract and a new store>` |
| Risk | `<n>` | `<what could go wrong for a child, for money, or for a contract>` |
| Distinctiveness | `<n>` | `<what competitors do today>` |
| Privacy fit | `<n>` | `<new personal data or consent needed, or none>` |
| Assist rung | `<1-4>` | `<rules, classical model, local language model, or external provider>` |
| Autonomy level | `<1-4>` | `<surfaces, suggests, drafts, or acts>` |
| **Total** | `<sum>` | |

Two different scales, and they are not the same thing:

- **Assist rung** is *what technology the feature needs*, from master brief Section 25 and Appendix W. 1 deterministic rules and smart defaults, 2 a classical model, 3 a local language model, 4 an optional external provider. The product must be complete at rung 1.
- **Autonomy level** is *how far it acts on its own*. 1 surfaces information the user already had a right to see; 2 suggests with visible reasons and the user decides; 3 drafts for a human to approve before it takes effect; 4 acts inside a written policy, with an audit entry and a one-click undo.

Score both. A rung 1 feature at autonomy 4 is a plain rule acting by itself, which is often the riskiest combination in the product and the easiest to miss.

Autonomy 4 is never permitted over a grade, a payment, or a message to a parent.

## How it fails safely

`<What happens when the inference is wrong, the data is missing, or the feature is switched off.>`

## Sixty-second demo

| Second | Screen | Action | What the viewer sees |
|---|---|---|---|

## Decision

| Verdict | Reason | Revisit when |
|---|---|---|