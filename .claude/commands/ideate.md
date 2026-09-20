---
description: Propose scored product ideas into docs/project/IDEAS.md
argument-hint: [area, role, or problem]
disable-model-invocation: true
---

Propose ideas for $ARGUMENTS and record them in `docs/project/IDEAS.md` using `docs/templates/idea-card.md`.

Produce eight to twelve ideas, then score and cut. Quantity first, judgement second, in that order.

Score every idea 1 to 5 on six axes:

| Axis | 5 means |
|---|---|
| Value | A school would change its buying decision over this |
| Effort | One slice inside an existing service; no new contract, no new store |
| Risk | Nothing about child safety, money, or a published contract can go wrong |
| Distinctiveness | No competitor in this market has it, and copying it would take them a year |
| Privacy fit | It needs no new personal data and no new consent |
| Assist rung | What technology it needs. See below |
| Autonomy level | How far it acts alone. See below |

Two different scales, and they are not the same thing:

- **Assist rung** is *what technology the feature needs*, from master brief Section 25 and Appendix W. 1 deterministic rules and smart defaults, 2 a classical model, 3 a local language model, 4 an optional external provider. The product must be complete at rung 1.
- **Autonomy level** is *how far it acts on its own*. 1 surfaces information the user already had a right to see; 2 suggests with visible reasons and the user decides; 3 drafts for a human to approve before it takes effect; 4 acts inside a written policy, with an audit entry and a one-click undo.

Score both. A rung 1 feature at autonomy 4 is a plain rule acting by itself, which is often the riskiest combination in the product and the easiest to miss.

Never propose anything at **autonomy 4** that touches a grade, a payment, or a message to a parent: master brief Section 25 makes assistance advisory, never authoritative. Never propose anything at **assist rung 3 or 4** without naming what it degrades to when the model is unavailable.

## Reads first

- `docs/brief/01-master-brief.md` Sections 12 and 25 only.
- `docs/brief/02-appendices/appendix-w-feature-register.md` for what is already registered, with its rung and tier.
- `docs/brief/02-appendices/appendix-p-differentiation.md` for what the market already does.
- `docs/project/IDEAS.md` when it exists, so you do not repeat an idea that was already rejected.
- `docs/templates/idea-card.md`.

## Output contract

- `## Ideas` — table: id, idea, one-line promise, value, effort, risk, distinctiveness, privacy fit, assist rung, autonomy level, total
- `## Top three` — an idea card each: the problem, the moment it lands, what it replaces, how it fails safely
- `## Rejected` — table: idea, the axis that killed it, the reason
- `## What would change my ranking` — the facts you would need
- `## Written to` — the lines added to `docs/project/IDEAS.md`

## Stop conditions

- Stop before proposing anything that needs new personal data about a child without saying so in the privacy fit column.
- Stop before proposing anything at autonomy 4 over grades, payments, or parent messages, or at assist rung 3 or 4 with no stated fallback.
- Stop when the area is already covered by an accepted requirement; say which one instead of re-proposing it.