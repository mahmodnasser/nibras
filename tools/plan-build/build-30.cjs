// Assemble docs/plan/30-plan-scorecard.md from the group scorecards of every
// scoring round: parts/round1/, parts/round2/ ... and the current round in parts/.
const fs = require('fs');
const path = require('path');
const { writeGenerated } = require('./write-generated.cjs');
// Paths resolve from this file, so the script runs from any checkout on Windows or Linux.
const SP = path.join(__dirname, 'parts') + path.sep;
const OUT = path.join(__dirname, '..', '..', 'docs', 'plan', '30-plan-scorecard.md');
const AXES = ['Completeness', 'Consistency', 'Feasibility', 'Risk honesty', 'Testability', 'Distinctiveness', 'Portability'];
const G = ['A', 'B', 'C', 'D', 'E', 'F'];
// The date each round was scored. The current round is the last one.
const ROUND_DATES = ['2026-09-22', '2026-09-26', '2026-09-26'];

const readRound = (dir) => {
  const scores = {};
  const bodies = {};
  for (const g of G) {
    const f = dir + 'score-' + g + '.md';
    if (!fs.existsSync(f)) return null;
    const t = fs.readFileSync(f, 'utf8').replace(/\r/g, '');
    bodies[g] = t.replace(/^#### Group [A-F]\s*\n/, '').trim();
    scores[g] = {};
    for (const a of AXES) {
      const m = new RegExp('^\\| ' + a + ' \\| (\\d)', 'm').exec(t);
      if (!m) throw new Error(dir + ' ' + g + ' missing ' + a);
      scores[g][a] = Number(m[1]);
    }
  }
  return { scores, bodies };
};
const rounds = [];
for (let n = 1; fs.existsSync(SP + 'round' + n); n++) rounds.push(readRound(SP + 'round' + n + path.sep));
const current = readRound(SP);
const archived = rounds.length && JSON.stringify(rounds[rounds.length - 1].scores) === JSON.stringify(current.scores) && rounds[rounds.length - 1].bodies.A === current.bodies.A;
if (!archived) rounds.push(current);
const last = rounds[rounds.length - 1];
const R = rounds.length;
const blocked = (g) => AXES.filter((a) => last.scores[g][a] < 4);
const approvedGroups = G.filter((g) => !blocked(g).length);

const L = [];
const p = (x = '') => L.push(x);
p('# 30. Plan Scorecard');
p();
p('## Purpose');
p();
p('This document records how good the plan is, group by group, against the seven-axis rubric in `PLAN_SPEC.md` ("The scorecard"), with quoted evidence for every score. **A group is approved only at 4 or better on every axis; an average never substitutes for a low score.** It exists so the product owner approves the plan on evidence, and so the work left before approval is a named, sized list rather than a feeling. It keeps every scoring round, so the progress is visible and a score cannot quietly improve.');
p();
p('## Scope');
p();
p('| In scope | Owned elsewhere |');
p('|---|---|');
p('| Scores for Groups A to F in every round, with evidence, the verdict per group and the gaps | The rubric: `PLAN_SPEC.md`; the procedure: `/score-plan` and `docs/templates/scorecard.md` |');
p('| What each remediation round changed, and what is left | The changes themselves: the documents, ADR-0019 to ADR-0024 and `docs/project/CHANGELOG.md` |');
p('| | Document 30 does not score itself, and document 00 is written after the last round |');
p();
p('## Content');
p();
p('### 1. Method');
p();
p('In every round each group was scored by an independent reviewer instructed to be adversarial, to quote the document with file and line for every score, and to reserve 5 for work that needs no change. From round 2 on, each reviewer also received the previous round\'s scorecard for its group and was told not to take the remediation on trust: every earlier gap is marked Closed, Partly closed or Open with evidence. Every reviewer was given the same facts established by tooling (the kit lint and its rules, the generated documents, the requirement and slice counts) and asked to find what tooling cannot: contradictions of meaning, thin sections, unowned unknowns, claims with no test, and defects the remediation itself introduced.');
p();
p('### 2. Summary, round ' + R + ' (' + ROUND_DATES[R - 1] + ')');
p();
p('| Group | ' + AXES.join(' | ') + ' | Lowest | Verdict |');
p('|---|' + AXES.map(() => '---|').join('') + '---|---|');
for (const g of G) {
  const v = AXES.map((a) => last.scores[g][a]);
  const b = blocked(g);
  p('| ' + g + ' | ' + v.join(' | ') + ' | ' + Math.min(...v) + ' | ' + (b.length ? 'Blocked (' + b.join(', ') + ')' : 'Approved') + ' |');
}
const axisMin = AXES.map((a) => Math.min(...G.map((g) => last.scores[g][a])));
p('| **Lowest per axis** | ' + axisMin.map((x) => '**' + x + '**').join(' | ') + ' | | |');
p();
p(approvedGroups.length === G.length
  ? '**Verdict: every group is approved at 4 or better on every axis.** The plan is ready for the product owner\'s approval, subject to the decisions listed in document 00.'
  : '**Verdict: ' + approvedGroups.length + ' of 6 groups approved' + (approvedGroups.length ? ' (' + approvedGroups.join(', ') + ')' : '') + '.** The groups still blocked, and the axes that block them, are in the table above; Section 6 lists what closes each.');
p();
p('### 3. Score history');
p();
p('Each cell reads round 1 → round 2' + (R > 2 ? ' → round ' + R : '') + '. A group moves to approved only when every axis reaches 4.');
p();
p('| Group | ' + AXES.join(' | ') + ' |');
p('|---|' + AXES.map(() => '---|').join(''));
for (const g of G) p('| ' + g + ' | ' + AXES.map((a) => rounds.map((r) => r.scores[g][a]).join(' → ')).join(' | ') + ' |');
p();
p('| Round | Date | Lowest axis score | Axes below 4 | Groups approved |');
p('|---|---|---|---|---|');
rounds.forEach((r, k) => {
  const below = G.reduce((t, g) => t + AXES.filter((a) => r.scores[g][a] < 4).length, 0);
  const ok = G.filter((g) => AXES.every((a) => r.scores[g][a] >= 4)).length;
  p('| ' + (k + 1) + ' | ' + ROUND_DATES[k] + ' | ' + Math.min(...G.flatMap((g) => AXES.map((a) => r.scores[g][a]))) + ' | ' + below + ' of 42 | ' + ok + ' of 6 |');
});
p();
p('### 4. What each remediation round changed');
p();
p('| Round | Theme | What changed | Record |');
p('|---|---|---|---|');
p('| After round 1 | 1. Product-owner decisions | Four conflicting values settled (cooling-off 30 days, invitations 14 days, deduplication 5 minutes, feature 39 to engineering); the open ones recorded as Open Questions 27 to 30 with defaults, scores and register risks | ADR-0019; `OPEN_QUESTIONS.md` |');
p('| After round 1 | 2. Schedule matches the work | Phase ranges derived from document 34 by `schedule-34.mjs`: phase 1 14 to 22 weeks, launch 61 to 93 weeks, MVP 33 to 50 weeks | Document 17 Section 1 |');
p('| After round 1 | 3. Brief corrections | 317 logged brief defects applied; brief v9.1 | ADR-0019 |');
p('| After round 1 | 4. One meaning per fact | Named contradictions removed across the plan and all 23 sheets; the revoked-user mark readable by the Gateway; two Integrations capabilities added | Documents 04 to 34 |');
p('| After round 1 | 5. One test-case registry | Every test defined in exactly one document; kit-lint R20; the registry annex generated | ADR-0020; brief v9.2 |');
p('| After round 1 | 6. Verification claims that run | Twelve kit-lint rules (R21 to R32); every verification row names a rule, a named review or a slice; generators gain `--check` | ADR-0021; brief v9.3 |');
p('| After round 1 | 7. Risk honesty | Every open point scored on the register\'s scales; 12 or more registered; threats owned; kit-lint R33 | ADR-0022 |');
p('| After round 1 | 8. Signature features on the stage | Every signature feature runs its own demo test in the release gate; kit-lint R34 | ADR-0023; brief v9.4 |');
p('| After round 2 | Round-2 gaps | The "Signature feature trace" in document 32; Platform notes and saga diagrams in the sheets; macOS in dev-smoke; Tier 2 requirements built early listed with reasons and checked by R35; open decisions no longer stated as settled; risk rows of 12 or more registered wherever they appear (R24) | ADR-0024 |');
p();
p('### 5. Scores by group, round ' + R);
p();
const names = { A: 'Understanding (01, 02; 00 last)', B: 'Requirements and architecture (03, 04, 05, 07)', C: 'Services, data, messaging, performance (06, 10, 11, 21)', D: 'Web, mobile, security, workflows, design (08, 09, 12, 13, 14)', E: 'Operations, testing, roadmap, risk, dependencies, traceability (15 to 20)', F: 'Conventions to work breakdown (22 to 29, 31 to 34)' };
let n = 1;
for (const g of G) {
  p('#### 5.' + n++ + ' Group ' + g + ': ' + names[g]);
  p();
  p(last.bodies[g]);
  p();
}
p('### 6. Round 1 findings re-checked by hand');
p();
p('The five most consequential round-1 findings were checked by hand before the first remediation, and all were real. Each is closed: the API count (document 02 now says six of ten), the Gateway\'s read of the revoked-user mark (document 21 Section 2.3), k6 (master brief Section 6.4 and `allow.json`), the phase 1 range (document 17 Section 1) and the AI index name (`ai_index.embedding_chunk` everywhere).');
p();
p('## Decisions in force');
p();
p('| Decision | Record |');
p('|---|---|');
p('| Approve only at 4 or better on every axis | `PLAN_SPEC.md`, "The scorecard" |');
p('| Scores need quoted evidence; a score without evidence is not a score | `.claude/commands/score-plan.md` |');
p('| Every round is kept; a later round checks every earlier gap | This document |');
p();
p('## Dependencies on other documents');
p();
p('| Document | What this one takes |');
p('|---|---|');
p('| `PLAN_SPEC.md` | The rubric and the approval rule |');
p('| Every plan document in Groups A to F | The evidence |');
p('| `docs/project/CHANGELOG.md`, ADR-0019 to ADR-0024 | What each remediation changed |');
p();
p('## Open points');
p();
p('| Point | Default | Owner | L | I | Score | In the register |');
p('|---|---|---|---|---|---|---|');
p('| The product-owner decisions the plan states as open (Open Questions 27 to 30, 3, 14, 26) and the Proposed ADRs | The defaults in force until decided; document 00 lists them | Product owner | 3 | 4 | 12 | RISK-44 |');
p();
p('## Review record');
p();
p('| Date | Reviewer | Result |');
p('|---|---|---|');
rounds.forEach((r, k) => {
  const ok = G.filter((g) => AXES.every((a) => r.scores[g][a] >= 4));
  p('| ' + ROUND_DATES[k] + ' | Round ' + (k + 1) + ': six independent adversarial reviewers, one per group | ' + (ok.length === 6 ? 'All groups approved' : ok.length + ' of 6 groups approved' + (ok.length ? ' (' + ok.join(', ') + ')' : '')) + '; lowest axis score ' + Math.min(...G.flatMap((g) => AXES.map((a) => r.scores[g][a]))) + ' |');
});
p();
p('## How this document is verified');
p();
p('| Claim | Proof |');
p('|---|---|');
p('| Every score has evidence | Each row of Section 5 quotes a document with file and line; a row without one is invalid |');
p('| The summary and history tables match the group scores | Computed from the group scorecards of every round by `build-30.cjs`, not typed |');
p('| The verdict follows the rule | A group with any axis below 4 is shown blocked; the script applies the rule |');
p('| The document is current | Kit-lint rule R23 reruns `build-30.cjs --check` and fails when a group scorecard changed since this document was built |');
p();
writeGenerated(OUT, L.join('\n'));
console.log('rounds ' + R + '; approved ' + approvedGroups.length + ' of 6; lowest per axis ' + axisMin.join(','));
