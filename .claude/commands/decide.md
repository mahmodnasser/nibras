---
description: Record an architecture decision as an ADR and update the open questions
argument-hint: [decision title or open question number]
disable-model-invocation: true
---

Record the decision about $ARGUMENTS as an architecture decision record.

1. Find the next free number in `docs/project/DECISIONS/`. Name the file `NNNN-kebab-title.md`, four digits, lower case, no spaces.
2. Fill `docs/templates/adr.md`. Every heading is filled. An ADR with an empty "Alternatives considered" is not a decision, it is a preference.
3. **Alternatives are real.** Give at least two, each with what it would have cost and why it lost. If there was genuinely only one option, say so and explain what forced it.
4. **Consequences are honest.** State what becomes harder, not only what becomes easier, and name what must be revisited and when.
5. Link the requirement IDs this decision constrains.
6. Update `docs/project/OPEN_QUESTIONS.md`: close the question this answers, or add the new question this decision creates.
7. Update every plan document that now contradicts the decision, and list what you changed.

## Reads first

- `docs/project/DECISIONS/0000-adr-template.md` and the titles of the existing ADRs, to check this is not already decided or superseded.
- `docs/project/OPEN_QUESTIONS.md`.
- `.claude/skills/adr/SKILL.md`.
- Only the plan sections the decision touches.

## Output contract

- `## ADR written` — path, number, title, status
- `## Decision` — one paragraph a new engineer could act on
- `## Alternatives` — table: option, cost, why it lost
- `## Consequences` — table: consequence, easier or harder, revisit when
- `## Open questions updated` — table: question number, closed or added, text
- `## Documents that now need changing` — table: document, what contradicts, change made or proposed

## Stop conditions

- Stop when an existing ADR already decides this. Propose superseding it explicitly instead of writing a second one.
- Stop when the decision contradicts a non-negotiable rule in `CLAUDE.md`. Bring it to me first.
- Stop when you cannot name a real alternative. That usually means the question is not yet understood.