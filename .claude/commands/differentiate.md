---
description: Fill the differentiation matrix row for one feature
argument-hint: [feature]
disable-model-invocation: true
---

Fill the differentiation row for $ARGUMENTS using `docs/templates/wow-moment.md` and `docs/templates/decision-matrix.md`.

Answer four questions, honestly. A row that concludes "we are better at everything" is a row nobody believes.

1. **Competitors.** Name three established products and what each actually does for this feature today. Describe their behavior, not their marketing. If a competitor does it well, say so.
2. **Our edge.** One sentence naming the difference, and the mechanism that produces it. "Better user experience" is not an edge. "The risk flag opens an intervention workflow with an owner and a follow-up date, so the flag cannot be ignored" is.
3. **Persona moment.** The exact moment a named person notices the difference: who, what day of the school year, what they were doing, what they felt before and after. Not a feature list. A moment.
4. **Sixty-second demo proof.** The precise screens and clicks that prove the edge in under sixty seconds, using the seeded demo data. Time it. If it cannot be shown in sixty seconds, the edge is not yet real, and you say so.

Close by stating what a competitor would have to build to erase this edge, and how long that would take them.

## Reads first

- `docs/brief/02-appendices/appendix-p-differentiation.md`: the differentiation matrix this command fills a row of.
- `docs/brief/02-appendices/appendix-w-feature-register.md` for this feature's register entry.
- `docs/plan/02-competitive-gap-analysis.md` when it exists.
- `docs/brief/02-appendices/appendix-h-demo-data.md` for what the demo can actually show.
- The owning service sheet in `docs/plan/06-services/` for what is really built.

## Output contract

- `## Feature` — name, owning service, tier
- `## Competitors` — table: product, what it does today, how well, evidence
- `## Our edge` — one sentence, plus the mechanism that produces it
- `## Persona moment` — the named person, the date in the school year, before and after
- `## Sixty-second proof` — table: second, screen, action, what the viewer sees
- `## Defensibility` — what a competitor must build to erase this, and how long

## Stop conditions

- Stop when the sixty-second proof runs over sixty seconds. Report the feature as not yet distinctive.
- Stop when you cannot name three real competitors with real behavior. Say what you could not verify.
- Stop when the edge depends on data the seeded demo does not contain.
