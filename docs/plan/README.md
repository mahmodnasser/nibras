# Plan

Empty until `/plan-platform` runs.

`PLAN_SPEC.md` defines the 34 documents that will appear here, in six review groups, with the scorecard each group must pass. `REVIEW_GUIDE.md` is written for the product owner: what to look for per group, five questions to ask, and how to phrase feedback that turns into a change.

Highlights of what lands here:

| Document | What it gives you |
|---|---|
| `05-service-catalog.md` | Every service in one table, with the reason its boundary exists |
| `06-services/<service>.md` | One complete specification sheet per service, including its full folder tree |
| `07-solution-structure.md` | The whole repository, to project level |
| `08-web-structure.md`, `09-mobile-structure.md` | The Angular workspace and the Flutter project, as trees |
| `11-messaging-architecture.md` | RabbitMQ topology and the complete message catalog |
| `15-deployment-and-operations.md` | The deploy tree, the three modes, backup and disaster recovery |
| `20-traceability-matrix.md` | Every requirement to its workflow, rule, test case and phase |
| `21-performance-engineering.md` | Caching map, hot queries, budgets |
| `30-plan-scorecard.md` | The score per group, with evidence |

Run `/lint-plan` after writing anything here, and `/score-plan <group>` before asking for approval. A group is approved at 4 or better on every axis of the rubric.
