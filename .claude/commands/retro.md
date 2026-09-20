---
description: Capture what the product owner changed in a review and fold it back into the kit
argument-hint: [review or group just reviewed]
disable-model-invocation: true
---

The product owner has just reviewed $ARGUMENTS and changed things. Capture what changed and make sure the same correction is never needed twice.

1. **List the corrections.** Every change the owner made or asked for, in their words, with what the document said before.
2. **Classify each one:**
   - *Preference* — the owner wants it a particular way. It becomes a default in a template or a rule.
   - *Missing knowledge* — the kit did not know a fact about schools. It becomes an open question answered, or a line in the glossary.
   - *Defect* — the kit produced something wrong. It becomes a lint rule, a stop condition, or an agent check.
   - *Scope change* — the product is different now. It becomes an ADR.
3. **Fold each one back into exactly one place.** A correction that lives in two places will rot in one of them.
4. **Say what you changed and where**, so the next session inherits the correction instead of repeating the mistake.
5. **Name the signal you missed.** What in the document should have told you the owner would object?

## Reads first

- The reviewed documents, and the owner's comments.
- `docs/project/OPEN_QUESTIONS.md` and `docs/project/PROJECT_STATE.md`.
- The template, rule, or command file each correction belongs in. Only those.

## Output contract

- `## Corrections` — table: id, what the owner changed, what it said before, class (preference, knowledge, defect, scope)
- `## Folded back` — table: correction, destination file, exact change made, how it prevents a repeat
- `## New open questions` — table: question, default, owner, impact if wrong
- `## ADRs needed` — table: scope change, ADR title to write
- `## Signals I missed` — table: correction, what should have warned me, the check that would catch it next time

## Stop conditions

- Stop before changing `CLAUDE.md` or any brief document. Propose the change and let me make it.
- Stop when a correction contradicts an accepted ADR. That is a superseding decision, not a retro item.
- Stop when a correction is really two corrections. Split it before folding it back.