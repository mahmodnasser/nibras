// Generate docs/plan/31-business-rules-and-workflows.md from Appendices R and S
// and the build phase column of document 05. Every row is derived, none retyped.
import { fileURLToPath as __toPath } from 'node:url';
import { dirname as __dirOf, resolve as __resolve } from 'node:path';
// Paths resolve from this file, so the script runs from any checkout on Windows or Linux.
const __here = __dirOf(__toPath(import.meta.url)).split(String.fromCharCode(92)).join('/');
const __kit = __resolve(__here, '../..').split(String.fromCharCode(92)).join('/') + '/';
import { readFileSync } from 'node:fs';
import { writeGenerated } from './write-generated.cjs';


const K = __kit;
const R = readFileSync(K + 'docs/brief/02-appendices/appendix-r-workflow-catalog.md', 'utf8');
const S = readFileSync(K + 'docs/brief/02-appendices/appendix-s-business-rules.md', 'utf8');
const C05 = readFileSync(K + 'docs/plan/05-service-catalog.md', 'utf8');

// ---- phase per service from document 05 ------------------------------------
const phase = new Map();
for (const line of C05.split('\n')) {
  const m = /^\| \*\*([A-Za-z.]+)\*\*/.exec(line);
  if (!m) continue;
  const cells = line.split('|').slice(1, -1).map((c) => c.trim());
  const last = cells[cells.length - 1];
  if (/^\d/.test(last) && !phase.has(m[1])) phase.set(m[1], /^\d+/.exec(last)[0]);
}

const pascal = (s) => s.replace(/[^A-Za-z0-9 ]+/g, ' ').split(/\s+/).filter(Boolean)
  .map((w) => w[0].toUpperCase() + w.slice(1)).join('');

// ---- rules ------------------------------------------------------------------
const rules = [];
for (const block of S.split(/\n(?=### BR-)/).slice(1)) {
  const h = /^### (BR-[A-Z0-9]+-\d+)\s+(.*)$/m.exec(block);
  const owner = (/\*\*Owner:\*\*\s*([A-Za-z.]+)/.exec(block) || [])[1] || '';
  const params = ((/\*\*Parameters:\*\*\s*([^\n]*)/.exec(block) || [])[1] || 'none').trim();
  const test = (/\*\*Tests:\*\*\s*`([^`]+)`/.exec(block) || [])[1] || '';
  const ruleText = ((/\*\*Rule\.\*\*\s*([^\n]*)/.exec(block) || [])[1] || '');
  if (!h) continue;
  const base = test.replace(/Tests$/, '').replace(/Rules$/, 'Rule');
  const arithmetic = /\b(average|weight|round|pro[- ]?rat|fee|discount|allocat|gpa|percent|tax|\bcap\b|accru|amount|scholarship|refund|credit|prorat|rank|denominator|total|sum|balance|installment|instalment|boundar|score)/i
    .test(h[2] + ' ' + ruleText);
  rules.push({ id: h[1], name: h[2].trim(), owner, params, test, impl: 'Nibras.' + owner + '.Domain.Rules.' + base, arithmetic, phase: phase.get(owner) || '?' });
}

// ---- workflows --------------------------------------------------------------
const flows = [];
for (const block of R.split(/\n(?=### WF-)/).slice(1)) {
  const h = /^### (WF-[A-Z]+-\d+)\s+(.*)$/m.exec(block);
  if (!h) continue;
  const meta = (/^\*\*Owner:\*\*[^\n]*/m.exec(block) || [''])[0];
  const get = (k) => ((new RegExp('\\*\\*' + k + ':\\*\\*\\s*([^·\\n]+)').exec(meta) || [])[1] || '').trim();
  const owner = get('Owner').split(/\s/)[0];
  const name = h[2].trim();
  flows.push({ id: h[1], name, owner, tier: get('Tier'), mobile: get('Mobile'), offline: get('Offline'),
    state: pascal(name) + 'Status', folder: 'Application/Features/' + pascal(name) + '/', phase: phase.get(owner) || '?' });
}

// ---- checks -------------------------------------------------------------------
const ruleNoOwner = rules.filter((r) => !r.owner);
const ruleNoTest = rules.filter((r) => !r.test);
const flowNoOwner = flows.filter((f) => !f.owner);
const unknownPhase = [...new Set([...rules, ...flows].filter((x) => x.phase === '?').map((x) => x.owner))];
const dupTest = rules.map((r) => r.test).filter((t, i, a) => t && a.indexOf(t) !== i);

// ---- document -------------------------------------------------------------------
const L = [];
const p = (s = '') => L.push(s);
p('# 31. Business Rules and Workflows, Assigned to Code');
p();
p('## Purpose');
p();
p('Appendix S states what the product computes and Appendix R states how its processes move. This document says **where each one lives in the code, what test proves it, and in which phase it is built.** It is generated from the two appendices and from the build-phase column of `05-service-catalog.md`, so a rule or workflow cannot be added to an appendix and silently miss an owner here.');
p();
p('Saga designs for the multi-service workflows are in `13-workflows-and-sagas.md`. This document does not repeat them.');
p();
p('## Scope');
p();
p('| In scope | Not in scope |');
p('|---|---|');
p('| Every `BR-` identifier in Appendix S, with its owning service, test class, implementation type and phase | The wording of the rules and their worked examples, which Appendix S owns |');
p('| Every `WF-` identifier in Appendix R, with its state type, feature folder and phase | Saga orchestration, which document 13 owns |');
p('| The implementation contract, the test conventions, and the mutation-testing targets | Test infrastructure, which document 16 owns |');
p();
p('## Content');
p();
p('### 1. Summary');
p();
const byOwner = new Map();
for (const r of rules) { const o = byOwner.get(r.owner) || { r: 0, w: 0 }; o.r++; byOwner.set(r.owner, o); }
for (const f of flows) { const o = byOwner.get(f.owner) || { r: 0, w: 0 }; o.w++; byOwner.set(f.owner, o); }
p('| Service | Rules | Workflows | Build phase |');
p('|---|---|---|---|');
for (const [o, c] of [...byOwner.entries()].sort()) p('| ' + o + ' | ' + c.r + ' | ' + c.w + ' | ' + (phase.get(o) || '?') + ' |');
p('| **Total** | **' + rules.length + '** | **' + flows.length + '** | |');
p();
p('Build phases are quoted from `05-service-catalog.md`, which quotes master brief Section 28.');
p();
p('### 2. Business rules');
p();
p('**How to read the implementation column.** Each rule is one class in the owning service\'s Domain project, under `Rules/`, named after its test class. A rule that needs data from outside its aggregate receives it as parameters; it never reaches into a repository. Where a rule is enforced by an Application handler rather than a pure domain type, the handler calls the domain rule; the rule still lives in one place.');
p();
p('| Rule | Name | Owner | Test class | Implementation | Parameters (Appendix G) | Property-based | Phase |');
p('|---|---|---|---|---|---|---|---|');
for (const r of rules) p('| `' + r.id + '` | ' + r.name + ' | ' + r.owner + ' | `' + r.test + '` | `' + r.impl + '` | ' + r.params.replace(/\|/g, '/') + ' | ' + (r.arithmetic ? 'yes' : 'no') + ' | ' + r.phase + ' |');
p();
p('**Property-based column.** "yes" marks a rule whose statement involves arithmetic: sums, averages, weights, rounding, proration, allocation, caps, percentages, ranks or balances. Those rules get a property-based test in addition to the table-driven one, asserting invariants that no finite example list can cover (for example: allocation never exceeds the payment, rounding is idempotent, a weighted average lies between its minimum and maximum input). The classification is derived from the rule text and is confirmed or corrected by the business-rules-reviewer agent during Group F review.');
p();
p('### 3. Workflows');
p();
p('**How to read the table.** The state type is an enumeration in the owning service\'s Domain project, never a set of booleans. The feature folder holds one sub-folder per transition command, following the anatomy in reference architecture Section 2. Transition tests are the rows of the workflow\'s test table in Appendix R, one test each.');
p();
p('| Workflow | Name | Owner | Tier | Mobile | Offline | State type | Feature folder | Phase |');
p('|---|---|---|---|---|---|---|---|---|');
for (const f of flows) p('| `' + f.id + '` | ' + f.name + ' | ' + f.owner + ' | ' + f.tier + ' | ' + f.mobile + ' | ' + f.offline + ' | `' + f.state + '` | `' + f.folder + '` | ' + f.phase + ' |');
p();
p('### 4. The implementation contract');
p();
p('Quoted from `.claude/rules/rules-and-workflows.md`, which Claude Code applies automatically to any file under a service\'s Domain or Application project:');
p();
p('| Rule | Why |');
p('|---|---|');
p('| A class implementing a business rule names its `BR-` identifier in a comment, and has the test class the rule names | Rule, code and test must be findable from each other or they drift |');
p('| Rules live in the domain, never in an endpoint, a consumer, a component or a database trigger | One rule, one place |');
p('| Money is `decimal` with an explicit currency; rounding happens once, at the point the rule names | The worked examples in Appendix S assume exactly this |');
p('| The order of operations in code matches the order stated in the rule | Where the rule is ambiguous, the rule is fixed, not the code guessed |');
p('| Every workflow is a state machine with an explicit state type; every transition validates state, checks the permission, writes the audit entry, and publishes through the outbox | Master brief Section 14 |');
p('| Every state a human waits in has a timeout and an escalation | Nothing waits forever |');
p('| Concurrent transitions resolve deterministically: first decision wins, the second gets a stable error code naming who decided | Two approvers at once is normal, not an edge case |');
p('| Every terminal state is reachable from every state, directly or through cancellation | Support can always end a stuck item |');
p('| A rule change updates the rule, its examples, its test class, and every screen that quotes it, in one change | Otherwise the product and its documentation disagree |');
p();
p('### 5. How the tests are built');
p();
p('| Test kind | Built from | Named |');
p('|---|---|---|');
p('| Table-driven rule test | Every `Given` line of the rule in Appendix S becomes one row of an `[Theory]` data source | `<TestClass>.<Rule>_<Given>_<Then>` |');
p('| Property-based rule test | The invariants of the rules marked "yes" above | `<TestClass>.Property_<Invariant>` |');
p('| Transition test | Every row of the workflow\'s test table in Appendix R | `<State>_<Transition>_<Expected>`, carrying the row\'s `TC-` identifier |');
p('| Timeout test | Every state with a timeout in Appendix R | `<State>_TimesOut_<Escalation>` |');
p('| Concurrency test | Every transition two actors can take at once | `<Transition>_Concurrent_FirstWins` |');
p();
p('A worked example that does not become a test row is a defect in the test class, and `/simulate-year` reports it.');
p();
p('### 6. Mutation-testing targets');
p();
p('Stryker.NET runs on the classes below and must reach a **mutation score of 80% or better**, per master brief Section 24. These are the domains where a surviving mutant is a wrong grade, a wrong invoice, a wrong promotion or a wrong permission.');
p();
const mutOwners = ['Assessment', 'Finance', 'Identity', 'Attendance', 'School'];
p('| Service | Rules under mutation testing |');
p('|---|---|');
for (const o of mutOwners) {
  const ids = rules.filter((r) => r.owner === o).map((r) => '`' + r.id + '`');
  p('| ' + o + ' | ' + (ids.join(', ') || 'none in Appendix S; promotion and status rules are workflow transitions, tested per transition') + ' |');
}
p();
p('### 7. Coverage check');
p();
p('| Check | Result |');
p('|---|---|');
p('| Rules with no owning service | ' + (ruleNoOwner.length ? ruleNoOwner.map((r) => r.id).join(', ') : 'none') + ' |');
p('| Rules with no named test class | ' + (ruleNoTest.length ? ruleNoTest.map((r) => r.id).join(', ') : 'none') + ' |');
p('| Test class names used by more than one rule | ' + (dupTest.length ? dupTest.join(', ') : 'none') + ' |');
p('| Workflows with no owning service | ' + (flowNoOwner.length ? flowNoOwner.map((f) => f.id).join(', ') : 'none') + ' |');
p('| Owners with no build phase in document 05 | ' + (unknownPhase.length ? unknownPhase.join(', ') : 'none') + ' |');
p('| Rules in Appendix S | ' + rules.length + ' |');
p('| Workflows in Appendix R | ' + flows.length + ' |');
p();
p('## Decisions in force');
p();
p('| Decision | Record |');
p('|---|---|');
p('| Rules live in the Domain project of their owning service, one class per rule | This document, following reference architecture Section 2 |');
p('| Every workflow state is an enumeration, never flags | `.claude/rules/rules-and-workflows.md` |');
p('| Test case identifiers follow `TC-<AREA>-<NNN>` | ADR-0014 |');
p();
p('## Dependencies on other documents');
p();
p('| Document | What this one takes from it |');
p('|---|---|');
p('| Appendix S | Every rule, its owner, parameters and test class |');
p('| Appendix R | Every workflow, its owner, tier, mobile and offline availability |');
p('| `05-service-catalog.md` | The build phase per service |');
p('| `13-workflows-and-sagas.md` | The saga designs this document does not repeat |');
p('| `16-test-strategy.md` | The test infrastructure these tests run on |');
p();
p('## Open points');
p();
p('| Point | Default | Owner | L | I | Score | In the register |');
p('|---|---|---|---|---|---|---|');
p('| The property-based classification is derived from rule text | Confirmed or corrected by the business-rules-reviewer agent in Group F review | Architect | 2 | 2 | 4 | none |');
p('| Promotion eligibility and status changes in School are workflows, not Appendix S rules | Tested per transition; add a rule to Appendix S under a version bump if an arithmetic threshold appears | Architect | 2 | 2 | 4 | none |');
p();
p('## Review record');
p();
p('| Date | Reviewer | Result |');
p('|---|---|---|');
p('| 2026-09-21 | Generated from Appendices R and S | Coverage check above |');
p();
p('## How this document is verified');
p();
p('| Claim | Proof |');
p('|---|---|');
p('| Every rule and workflow has a row | The document is generated from the appendices; the coverage check in Section 7 is recomputed on every regeneration |');
p('| The document is current | Kit-lint rule R23 reruns `gen-31.mjs --check` and fails when Appendices R or S or document 05 changed since it was generated |');
p('| Every rule has a test class that exists in code | Once code exists, an architecture test enumerates `BR-` comments and asserts the named test class exists |');
p('| Every worked example is a test row | `/simulate-year` and the business-rules-reviewer agent compare Appendix S examples with the test data sources |');
p('| Mutation targets are met | Stryker.NET in the pipeline, gated at 80% on the classes in Section 6 |');
p();
writeGenerated(K + 'docs/plan/31-business-rules-and-workflows.md', L.join('\n'));
console.log('rules ' + rules.length + ', workflows ' + flows.length + ', arithmetic ' + rules.filter((r) => r.arithmetic).length);
console.log('no owner ' + ruleNoOwner.length + ', no test ' + ruleNoTest.length + ', dup test ' + dupTest.length + ', wf no owner ' + flowNoOwner.length + ', unknown phase: ' + (unknownPhase.join(',') || 'none'));
console.log('phases found: ' + [...phase.entries()].map(([k, v]) => k + '=' + v).join(' '));
