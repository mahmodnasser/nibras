export const meta = {
  name: 'plan-rescore-round-3',
  description: 'Re-score every plan group after the third remediation round, with quoted evidence, against round 2',
  phases: [{ title: 'Score' }],
}
const KIT = 'C:\\Repo\\Nibras\\nibras-platform-kit-v9'
const PARTS = KIT + '\\tools\\plan-build\\parts\\'
const GROUPS = [
  { g: 'A', docs: 'docs/plan/01-questions-and-assumptions.md, docs/plan/02-competitive-gap-analysis.md. Document 00 (executive summary) is written after this scoring by design (PLAN_SPEC "then update 00"); score Completeness on 01 and 02 and say 00 is pending.' },
  { g: 'B', docs: 'docs/plan/03-requirements-catalog.md, 04-architecture-overview.md, 05-service-catalog.md, 07-solution-structure.md' },
  { g: 'C', docs: 'docs/plan/06-services/*.md (23 sheets: read at least identity, finance, wellbeing, attendance, ai, platform and one of gateway/bff-web in depth; grep across all), docs/plan/10-data-architecture.md, 11-messaging-architecture.md, 21-performance-engineering.md' },
  { g: 'D', docs: 'docs/plan/08-web-structure.md, 09-mobile-structure.md, 12-security-privacy-safety.md, 13-workflows-and-sagas.md, 14-design-system-and-ux.md' },
  { g: 'E', docs: 'docs/plan/15-deployment-and-operations.md, 16-test-strategy.md (and 16-annex-test-case-registry.md), 17-roadmap.md, 18-risk-register.md, 19-dependency-and-license-inventory.md, 20-traceability-matrix.md' },
  { g: 'F', docs: 'docs/plan/22-api-conventions-and-error-catalog.md, 23-integrations-and-public-api.md, 24-localization-and-calendars.md, 25-ai-and-assist-ladder.md, 26-migration-and-onboarding-toolkit.md, 27-compliance-and-legal.md, 28-capacity-and-cost-model.md, 29-adr-index.md, 31-business-rules-and-workflows.md, 32-product-differentiation-and-demo.md, 33-platform-support-and-dev-environments.md, 34-work-breakdown.md. Document 30 is the scorecard and is not scored.' },
]
const common = `You are an adversarial plan reviewer for the Nibras school-platform plan, scoring round 3. Kit root: ${KIT}. Read-only: do NOT edit any file except the one output file named below. Windows; use Read/Grep/Glob; write the output with the Write tool.

Read first: docs/plan/PLAN_SPEC.md (the rows for your documents and "## The scorecard"), .claude/commands/score-plan.md (the rules), docs/templates/scorecard.md.

ROUND 2: your group was scored in round 2 as recorded in ${PARTS}round2\\score-{G}.md (read it; round 1 is ${PARTS}round1\\score-{G}.md). Since round 2 a third remediation round was applied to close every round-2 gap (docs/project/PROJECT_STATE.md and CHANGELOG.md; ADR-0024 lists Tier 2 requirements built early; document 32 has a "Signature feature trace"; sheets have Platform notes and Signature features lines; kit-lint gained R35 and a stricter R24). DO NOT TAKE THAT ON TRUST: for every round-2 gap of your group (both the "Round-1 gap / Now" rows not Closed and the "Gap | Axis" rows), check the documents and say Closed, Partly closed or Open, with evidence.

Established by tooling (true now): node tools/kit-lint/kit-lint.mjs . is clean with 35 rules R01-R35, each with self-tests (read their titles in tools/kit-lint/kit-lint.mjs to know what is mechanically checked); every generated document is current; 860 requirements, 858 built by 718 slices in 79 capabilities; every test case defined once; every open point and every risk row scoring 12 or more names a register RISK. Still open for the product owner, stated as such with defaults in force: Open Questions 27, 28, 29, 30 and 3, 14, 26; ADR-0018 and ADR-0020 to ADR-0024 are Proposed. An open decision stated honestly with a default, a score and an owner is not a defect; one stated as settled is.

Rules: score each of the 7 axes (Completeness, Consistency, Feasibility, Risk honesty, Testability, Distinctiveness, Portability) 0 to 5 with evidence QUOTED from a document with file:line (under 25 words per quote). 4 means: everything required is present and correct, with at most minor issues that do not mislead a builder; 5 is rare and means no change needed. Score what is written. Do not praise. Look for NEW defects the remediation introduced. Do not penalise a document for depending on a product-owner decision that is honestly open.

Write your result to the output file in EXACTLY this markdown format and nothing else:

#### Group {G}

| Axis | Score | Evidence | What would raise it by one |
|---|---|---|---|
(7 rows, axes in the order above)

**Verdict:** Approved | Blocked (blocking axes: ...)

| Round-2 gap | Now | Evidence |
|---|---|---|
(one row per round-2 gap of your group)

| Gap | Axis | Effort to close | Blocking |
|---|---|---|---|
(remaining and new gaps, most important first; 0 to 10 rows)

Then reply in under 80 words with the seven scores, the verdict, and how many round-2 gaps are closed.`
phase('Score')
return await parallel(GROUPS.map(x => () => agent(common.split('{G}').join(x.g) + `\n\nYOUR GROUP: ${x.g}. Documents: ${x.docs}\n\nOUTPUT FILE: ${PARTS}score-${x.g}.md`, { label: 'rescore3:' + x.g, phase: 'Score' })))
