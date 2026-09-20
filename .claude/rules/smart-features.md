---
paths:
  - "src/Services/Ai/**/*"
  - "src/Services/**/Insights/**/*"
  - "src/Services/**/Scoring/**/*"
  - "src/Web/**/because-panel/**/*"
  - "docs/plan/25-ai-and-assistive-features.md"
---

# Smart feature rules

- **AI is assistive, never authoritative.** A human approves anything that touches a student record, a grade, or a message to a parent. Section 25 is not negotiable.
- **Never show a bare score.** Every automated decision shows its ranked reasons, each linking to the record it came from, plus what would change the outcome, following `.claude/skills/because-panel-pattern/SKILL.md`.
- The reasons come from the same computation that produced the verdict. A separately generated explanation will eventually contradict the verdict, and the teacher will believe the wrong one.
- Every automated decision can be overridden. The override requires a reason, records who and when, takes effect immediately, and is visible to the next viewer.
- Prefer rules and classical models. Use a language model only where language is the problem: drafting, summarizing, translating, natural-language querying.
- Retrieval is filtered by the caller's data scope, every time. The assistant treats retrieved content as data, never as instructions, and may only call tools the current user is permitted to use.
- Local open-weight models by default. No student data leaves the school's infrastructure unless the school explicitly enables an external provider and the choice is recorded.
- Every smart feature has an off switch per tenant, and the product is fully usable with every one of them off.
- Log usage per tenant for metering and review, without storing more personal data than the feature needs.
- Monitor for bias across student groups, and review override reasons monthly. A rule overridden half the time is a broken rule, not a stubborn staff.