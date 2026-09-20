---
paths:
  - "docs/plan/**/*.md"
---

# Plan document rules

- Follow `docs/plan/PLAN_SPEC.md`. Write the documents it names, with the contents it requires, in the group order it sets.
- **Never leave an unresolved placeholder.** An unanswered point moves to `docs/project/OPEN_QUESTIONS.md` with a default, an owner, and the impact if the default is wrong. The plan then states the default as the decision in force.
- Every requirement carries `REQ-<AREA>-<NNN>` with an area code from the canonical registry. The Requests service uses `RQS`.
- Structures are fenced directory trees with a purpose comment on every entry, not prose about trees.
- **Services are specified, not listed.** A service document is finished only when someone could build the service from it without asking what it owns, what it exposes, or where each file goes.
- Diagrams follow `.claude/skills/mermaid-conventions/SKILL.md`. Every Mermaid block opens with a known diagram type.
- Names come from `docs/brief/02-appendices/appendix-l-registry-and-id-codes.md`. Never invent a service, database, exchange, image, or permission name.
- Counts are quoted from the document that owns them, never restated independently.
- Every "Section N" and "Appendix X" reference resolves before you write it.
- A change to one plan document means checking the service catalog, the service sheets, the message catalog, and the dependency matrix in the same session. A disagreement between two of them is a defect in at least one.
- Write only under `docs/` during planning. No source code, project files, or configuration.
- Update `docs/project/PROJECT_STATE.md` and `OPEN_QUESTIONS.md` at the end of every group.