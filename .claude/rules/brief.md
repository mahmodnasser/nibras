---
paths:
  - "docs/brief/**/*.md"
---

# Brief rules

- **The brief is normative and is not edited casually.** Any change to `docs/brief/` requires an ADR in `docs/project/DECISIONS/` that states what changed, why, and what it invalidates downstream. Write the ADR first, then the edit.
- `docs/brief/02-appendices/appendix-l-registry-and-id-codes.md` is the single source of truth for names and identifiers. Nothing else redefines a name. When another document disagrees with it, that document is the defect.
- Counts are stated once, in the appendix that owns them, and quoted everywhere else. A service count written in three places will disagree in two of them within a month.
- The three brief documents carry the same version in their titles. Changing one means changing all three.
- Every "Section N" and "Appendix X" reference must resolve. Check before you write it; the kit lint checks after.
- A change to the brief means checking, in the same session: the plan documents it invalidates, `docs/project/TRACEABILITY.md`, and `docs/project/OPEN_QUESTIONS.md`.
- Never soften a requirement to match what was built. If the product cannot meet the brief, that is an ADR recording the deviation, not a quiet edit.
- Never leave an unanswered point inside the brief. It moves to `docs/project/OPEN_QUESTIONS.md` with a default and an owner.