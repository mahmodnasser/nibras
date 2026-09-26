// Generate docs/plan/31-business-rules-and-workflows.md from Appendices R and S,
// the slices of document 34 (whose phases are the capability phases of document 17)
// and the build phase column of document 05. Every row is derived, none retyped.
import { fileURLToPath as __toPath } from 'node:url';
import { dirname as __dirOf, resolve as __resolve } from 'node:path';
// Paths resolve from this file, so the script runs from any checkout on Windows or Linux.
const __here = __dirOf(__toPath(import.meta.url)).split(String.fromCharCode(92)).join('/');
const __kit = __resolve(__here, '../..').split(String.fromCharCode(92)).join('/') + '/';
import { readFileSync, existsSync } from 'node:fs';
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

// ---- build phase per identifier from document 34 --------------------------------
// A rule or workflow is built in the earliest phase of a document 34 slice whose
// Covers column names it; the owning service's phase in document 05 is the
// fallback for an identifier no slice names.
const built = new Map();
// The first slice that names each identifier, and the first slice of the owning service
// that names it, so a workflow started ahead of its owner can say so (Section 3 note).
const firstSlice = new Map();
const firstOwnerSlice = new Map();
const D34 = K + 'docs/plan/34-work-breakdown.md';
if (existsSync(D34)) {
  let ph = null;
  for (const line of readFileSync(D34, 'utf8').split(/\r?\n/)) {
    const h = /^### \d+\. Phase (\d)/.exec(line);
    if (h) ph = h[1];
    if (!ph || !/^\| SL-/.test(line)) continue;
    const cells = line.split('|');
    const covers = cells[5] || '';
    const slice = { id: cells[1].trim(), service: cells[3].trim(), ph };
    for (const m of covers.matchAll(/(BR-[A-Z0-9]+-\d{3}|WF-[A-Z]+-\d{2})/g)) {
      if (!built.has(m[1]) || +ph < +built.get(m[1])) { built.set(m[1], ph); firstSlice.set(m[1], slice); }
      const own = firstOwnerSlice.get(m[1]) || new Map();
      if (!own.has(slice.service) || +ph < +own.get(slice.service).ph) own.set(slice.service, slice);
      firstOwnerSlice.set(m[1], own);
    }
  }
}
const phaseOf = (id, owner) => built.get(id) || phase.get(owner) || '?';
// Rules whose default is contested by an open product-owner decision. Their rows and the
// mutation targets carry the mark, so no reader takes them as settled. Empty since
// ADR-0027 (Accepted 2026-09-26) decided Open Question 30 and closed RISK-52.
const CONTESTED = {};
// Rules a product-owner decision settled after a conflict, stated under Section 2 so the
// reader sees the decision rather than the old conflict.
const DECIDED = {
  'BR-FIN-017': 'decided by ADR-0027 (Accepted by the product owner on 2026-09-26, Open Question 30, brief v9.7): a tenant is billed on the students enrolled on the billing date, prorated by day from a mid-month enrollment and counted for the month in which they leave, as master brief Section 36 and REQ-PLT-009 state. Platform owns and computes it (`34-work-breakdown.md` SL-PLT-010), Finance does not, and BR-PLT-005\'s active-student meter is this count; RISK-52 is Closed',
};

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
  rules.push({ id: h[1], name: h[2].trim(), owner, params, test, impl: 'Nibras.' + owner + '.Domain.Rules.' + base, arithmetic, derived: arithmetic, phase: phaseOf(h[1], owner) });
}
// ---- the property-based classification review (remediation round 6, 2026-09-26) -------
// The keyword match above is a first pass. Each of the rules below was read in full in
// Appendix S and its classification corrected against the criterion of Appendix V
// ("money, dates and weighted averages") and of Section 2: a rule is "yes" when it
// computes, counts or compares a number, an amount, a date or a duration over inputs a
// generator can vary. Every rule not listed was read and its derived value confirmed.
const CLASSIFICATION_REVIEW = {
  'BR-ATT-001': [true, 'derives the day status from a count of absent periods against a threshold'],
  'BR-ATT-005': [true, 'adds one derived absence per N lates, a floor division recomputed from the live count'],
  'BR-ATT-006': [true, 'counts consecutive school days, skipping non-school days and resetting, and fires once'],
  'BR-ATT-007': [true, 'counts cumulative absences per year against an ordered ladder whose rungs fire once each'],
  'BR-FIN-017': [true, 'counts students enrolled on the billing date and prorates a mid-month joiner by day, with the rounding Appendix S states (ADR-0027, see below)'],
  'BR-SCD-002': [true, 'counts a run of consecutive periods within a day, with breaks ignored'],
  'BR-SCD-003': [true, 'checks interval containment in availability windows and a weekly period count against a maximum'],
  'BR-SCD-004': [true, 'measures the gap between periods on two campuses against a travel time'],
  'BR-SCD-007': [true, 'extends a booking interval by setup and teardown buffers and tests the overlap'],
  'BR-ADM-001': [true, 'computes age in whole years on a cut-off date, leap days included'],
  'BR-ADM-003': [true, 'compares outstanding offers plus enrolled students with the seat capacity'],
  'BR-ADM-004': [true, 'counts calendar days from the issue date in the campus time zone to the expiry instant'],
  'BR-RQS-002': [true, 'counts working days on the campus calendar, holidays excluded, before the bands apply'],
  'BR-RQS-003': [true, 'measures working hours on a campus calendar with pauses; the Requests sheet already plans its property tests'],
  'BR-L10N-003': [true, 'converts between the Umm al-Qura and Gregorian calendars, a date computation with a round trip to hold'],
  'BR-L10N-005': [true, 'selects an Arabic plural category from the number by modular arithmetic'],
  'BR-WEL-001': [false, 'matched on "summary" in its text; it is an audience rule with no arithmetic, and its tests are the access tests of the Wellbeing sheet'],
};
for (const r of rules) if (CLASSIFICATION_REVIEW[r.id]) r.arithmetic = CLASSIFICATION_REVIEW[r.id][0];

// ---- workflows --------------------------------------------------------------
const flows = [];
for (const block of R.split(/\n(?=### WF-)/).slice(1)) {
  const h = /^### (WF-[A-Z]+-\d+)\s+(.*)$/m.exec(block);
  if (!h) continue;
  const meta = (/^\*\*Owner:\*\*[^\n]*/m.exec(block) || [''])[0];
  const get = (k) => ((new RegExp('\\*\\*' + k + ':\\*\\*\\s*([^·\\n]+)').exec(meta) || [])[1] || '').trim();
  const owner = get('Owner').split(/\s/)[0];
  const name = h[2].trim();
  // Transition tests: the Test column of the workflow's transition table in Appendix R.
  const tests = [];
  let testCol = -1;
  for (const line of block.split(/\r?\n/)) {
    if (!/^\|/.test(line)) { testCol = -1; continue; }
    const cells = line.split('|').slice(1, -1).map((c) => c.trim());
    if (/^Transition$/i.test(cells[0])) { testCol = cells.findIndex((c) => /^Test$/i.test(c)); continue; }
    if (testCol >= 0 && !/^:?-{2,}/.test(cells[0])) for (const m of (cells[testCol] || '').match(/TC-[A-Z0-9]+-\d{3}/g) || []) if (!tests.includes(m)) tests.push(m);
  }
  flows.push({ id: h[1], name, owner, tier: get('Tier'), mobile: get('Mobile'), offline: get('Offline'), tests,
    state: pascal(name) + 'Status', folder: 'Application/Features/' + pascal(name) + '/', phase: phaseOf(h[1], owner) });
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
p('Appendix S states what the product computes and Appendix R states how its processes move. This document says **where each one lives in the code, what test proves it, and in which phase it is built.** It is generated from the two appendices, from the slices of `34-work-breakdown.md`, whose phases are the capability phases of `17-roadmap.md` and give the Phase column of every rule and workflow, and from the build-phase column of `05-service-catalog.md`, which gives the service table of Section 1 and the phase of an identifier no slice names. So a rule or workflow cannot be added to an appendix and silently miss an owner here.');
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
p('The service table quotes each service\'s build phase from `05-service-catalog.md`, which quotes master brief Section 28. The Phase column of the rule and workflow tables below is the earliest phase of a `34-work-breakdown.md` slice whose Covers column names the identifier, so a rule a phase 1 slice builds reads phase 1 even when its owning service arrives later; an identifier no slice names falls back to its service\'s phase.');
p();
p('### 2. Business rules');
p();
p('**How to read the implementation column.** Each rule is one class in the owning service\'s Domain project, under `Rules/`, named after its test class. A rule that needs data from outside its aggregate receives it as parameters; it never reaches into a repository. Where a rule is enforced by an Application handler rather than a pure domain type, the handler calls the domain rule; the rule still lives in one place.');
p();
p('| Rule | Name | Owner | Test class | Implementation | Parameters (Appendix G) | Property-based | Phase |');
p('|---|---|---|---|---|---|---|---|');
// A rule first named by a slice of another service before its owner is built, marked as
// the workflows of Section 3 are.
const earlyRules = rules.filter((r) => phase.has(r.owner) && /^\d$/.test(r.phase) && +r.phase < +phase.get(r.owner));
for (const r of rules) p('| `' + r.id + '` | ' + r.name + (CONTESTED[r.id] ? ' (**contested**: ' + CONTESTED[r.id] + ')' : '') + ' | ' + r.owner + ' | `' + r.test + '` | `' + r.impl + '` | ' + r.params.replace(/\|/g, '/') + ' | ' + (r.arithmetic ? 'yes' : 'no') + ' | ' + r.phase + (earlyRules.includes(r) ? ' (owner ' + phase.get(r.owner) + ', note below)' : '') + ' |');
p();
if (earlyRules.length) {
  p('**Rules whose Phase precedes their owning service.** The Phase column is the earliest `34-work-breakdown.md` slice that names the rule, and for the rows below that slice belongs to another service or to a building block, which applies the rule on its own side ahead of the owner, as the slice describes. The rule class named in the Implementation column, in the owner\'s Domain project, with its test class and, for Sections 5 and 6, its property-based and mutation tests, is built in the owner\'s phase, from the slice in the last column. Where the last column reads "none in document 34", no slice of the owning service names the rule, so the rule is built only by the first slice, outside the owner Appendix S gives it; that is a disagreement between Appendix S and document 34, recorded here and in Open points rather than resolved:');
  p();
  p('| Rule | Owner, and its build phase in document 05 | First slice naming it, phase and service | Owner\'s first slice naming it |');
  p('|---|---|---|---|');
  for (const r of earlyRules) {
    const s = firstSlice.get(r.id);
    const o = (firstOwnerSlice.get(r.id) || new Map()).get(r.owner);
    p('| `' + r.id + '` | ' + r.owner + ', phase ' + phase.get(r.owner) + ' | ' + (s ? s.id + ', phase ' + s.ph + ', ' + s.service : 'none') + ' | ' + (o ? o.id + ', phase ' + o.ph : 'none in document 34') + ' |');
  }
  p();
}
for (const [id, why] of Object.entries(CONTESTED)) if (rules.some((r) => r.id === id)) p('**`' + id + '` is contested, not settled.** It is ' + why + '. `34-work-breakdown.md` SL-PLT-010 builds the billing-date count as the default in force, and the rule stays in conflict with it until the product owner decides; its test class and mutation target (Section 6) stand for whichever count the decision keeps.');
for (const [id, why] of Object.entries(DECIDED)) if (rules.some((r) => r.id === id)) p('**`' + id + '` is settled.** It is ' + why + '.');
p();
p('**Property-based column.** "yes" marks a rule whose statement involves arithmetic: sums, averages, weights, rounding, proration, allocation, caps, percentages, ranks or balances. Those rules get a property-based test in addition to the table-driven one, asserting invariants that no finite example list can cover (for example: allocation never exceeds the payment, rounding is idempotent, a weighted average lies between its minimum and maximum input). The classification was first derived from the rule text by a keyword match, and then reviewed rule by rule at remediation round 6 (2026-09-26): all ' + rules.length + ' rules were read in full in Appendix S against the criterion of Appendix V, money, dates and weighted averages, read as any rule that computes, counts or compares a number, an amount, a date or a duration. ' + (rules.length - Object.keys(CLASSIFICATION_REVIEW).length) + ' derived values were confirmed and the ' + Object.keys(CLASSIFICATION_REVIEW).length + ' below were corrected, which gives ' + rules.filter((r) => r.arithmetic).length + ' rules marked "yes" where the keyword match gave ' + rules.filter((r) => r.derived).length + '. The review was made by the plan editor; the business-rules-reviewer agent confirms it at the next Group F review.');
p();
p('| Rule | Derived | Reviewed | Why |');
p('|---|---|---|---|');
for (const r of rules) if (CLASSIFICATION_REVIEW[r.id]) p('| `' + r.id + '` | ' + (r.derived ? 'yes' : 'no') + ' | ' + (r.arithmetic ? 'yes' : 'no') + ' | ' + CLASSIFICATION_REVIEW[r.id][1][0].toUpperCase() + CLASSIFICATION_REVIEW[r.id][1].slice(1) + ' |');
p();
p('### 3. Workflows');
p();
p('**How to read the table.** The state type is an enumeration in the owning service\'s Domain project, never a set of booleans. The feature folder holds one sub-folder per transition command, following the anatomy in reference architecture Section 2. Transition tests are the rows of the workflow\'s test table in Appendix R, one test each.');
p();
// Consecutive identifiers of one area collapse into a range: TC-ATT-001 to TC-ATT-006.
const ranges = (ids) => {
  const out = [];
  for (let i = 0; i < ids.length; i++) {
    const [, area, num] = /^TC-([A-Z0-9]+)-(\d{3})$/.exec(ids[i]);
    let j = i;
    while (j + 1 < ids.length && ids[j + 1] === 'TC-' + area + '-' + String(Number(/\d{3}$/.exec(ids[j])[0]) + 1).padStart(3, '0')) j++;
    out.push(j - i >= 2 ? ids[i] + ' to ' + ids[j] : ids.slice(i, j + 1).join(', '));
    i = j;
  }
  return out.join(', ');
};
p('| Workflow | Name | Owner | Tier | Mobile | Offline | State type | Feature folder | Phase | Transition tests (Appendix R) |');
p('|---|---|---|---|---|---|---|---|---|---|');
// A workflow first named by a slice of another service before its owner is built.
const early = flows.filter((f) => phase.has(f.owner) && /^\d$/.test(f.phase) && +f.phase < +phase.get(f.owner));
for (const f of flows) p('| `' + f.id + '` | ' + f.name + ' | ' + f.owner + ' | ' + f.tier + ' | ' + f.mobile + ' | ' + f.offline + ' | `' + f.state + '` | `' + f.folder + '` | ' + f.phase + (early.includes(f) ? ' (owner ' + phase.get(f.owner) + ', note below)' : '') + ' | ' + (f.tests.length ? ranges(f.tests) : 'none') + ' |');
p();
if (early.length) {
  p('**Workflows whose Phase precedes their owning service.** The Phase column is the earliest `34-work-breakdown.md` slice that names the workflow, and for the rows below that slice belongs to another service, which builds its own side of the workflow ahead of the owner. The owner\'s state machine, its state type and its transition tests are built in the owner\'s phase, from the slice in the last column:');
  p();
  p('| Workflow | Owner, and its build phase in document 05 | First slice naming it, phase and service | Owner\'s first slice naming it |');
  p('|---|---|---|---|');
  for (const f of early) {
    const s = firstSlice.get(f.id);
    const o = (firstOwnerSlice.get(f.id) || new Map()).get(f.owner);
    p('| `' + f.id + '` | ' + f.owner + ', phase ' + phase.get(f.owner) + ' | ' + (s ? s.id + ', phase ' + s.ph + ', ' + s.service : 'none') + ' | ' + (o ? o.id + ', phase ' + o.ph : 'none in document 34') + ' |');
  }
  p();
}
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
  const ids = rules.filter((r) => r.owner === o).map((r) => '`' + r.id + '`' + (CONTESTED[r.id] ? ' (contested, Open Question 30, RISK-52; see Section 2)' : ''));
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
p('| Workflows with no transition test in Appendix R | ' + (flows.filter((f) => !f.tests.length).map((f) => f.id).join(', ') || 'none') + ' |');
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
p('| `34-work-breakdown.md` | The Phase column of Sections 2 and 3: the earliest slice whose Covers column names each identifier |');
p('| `17-roadmap.md` | Through document 34, the capability phase each of those slices is built in |');
p('| `05-service-catalog.md` | The build phase per service in Section 1, the owner phase in the Section 3 note, and the phase of an identifier no slice names |');
p('| `13-workflows-and-sagas.md` | The saga designs this document does not repeat |');
p('| `16-test-strategy.md` | The test infrastructure these tests run on |');
p();
p('## Open points');
p();
p('| Point | Default | Owner | L | I | Score | In the register |');
p('|---|---|---|---|---|---|---|');
p('| The property-based classification was reviewed by the plan editor, not yet by the business-rules-reviewer agent | The reviewed classification of Section 2 stands: every rule read in full at remediation round 6, ' + Object.keys(CLASSIFICATION_REVIEW).length + ' derived values corrected with their reasons. The business-rules-reviewer agent confirms or corrects it at the next Group F review, and a later correction goes into the review list of `gen-31.mjs` with its reason | Architect | 1 | 2 | 2 | none |');
const orphanRules = earlyRules.filter((r) => !(firstOwnerSlice.get(r.id) || new Map()).get(r.owner));
if (orphanRules.length) p('| ' + orphanRules.map((r) => '`' + r.id + '`').join(' and ') + ' are owned by ' + [...new Set(orphanRules.map((r) => r.owner))].join(', ') + ' in Appendix S but built only by ' + [...new Set(orphanRules.map((r) => firstSlice.get(r.id).service))].join(', ') + ' slices in document 34 (Section 2 note) | The slices of document 34 stand: the rule class is written in the building service, which is the service that applies it, and Appendix S\'s owner is corrected under a record with a brief version bump, as ADR-0027 did for `BR-FIN-017` and `BR-FIN-018` | Architect | 3 | 2 | 6 | none |');
p('| Promotion eligibility and status changes in School are workflows, not Appendix S rules | Tested per transition; add a rule to Appendix S under a version bump if an arithmetic threshold appears | Architect | 2 | 2 | 4 | none |');
p();
p('## Review record');
p();
p('| Date | Reviewer | Result |');
p('|---|---|---|');
p('| 2026-09-21 | Generated from Appendices R and S | Coverage check above |');
p('| 2026-09-22 | Round-1 scorecard, Group F | Blocked on Completeness, Consistency, Feasibility, Risk honesty, Testability and Distinctiveness; for this document, Section 3 listed no transition-test ids per workflow |');
p('| 2026-09-26 | Round-2 scorecard, Group F, then remediation round 3 | Blocked on Completeness, Consistency, Risk honesty and Testability; the transition-test ids still missing |');
p('| 2026-09-26 | Round-3 scorecard, Group F, then remediation round 4 | Blocked on Completeness, because Section 3 still listed no transition-test ids; the transition-test column was generated from Appendix R\'s test tables in remediation round 4 |');
p('| 2026-09-26 | Round-4 scorecard, Group F, then remediation round 5 | Blocked on Consistency, by document 34\'s SL-ACA-207, not by this document; the transition-test column was found complete. Amended: the Purpose, Dependencies and verification rows name document 34, and through it document 17, as the source of the Phase column; the seven workflows whose Phase precedes their owning service (WF-RQS-01, WF-WEL-01, WF-WEL-02, WF-WEL-04, WF-HR-01, WF-OPS-01 and WF-DATA-01) are marked with the owner\'s phase and explained in a note under Section 3; `BR-FIN-017` is marked contested pending Open Question 30 (RISK-52) in Sections 2 and 6; the property-based open point records that its review has not yet been done |');
p('| 2026-09-26 | Round-5 scorecard, remediation round 6 | Group F was approved with minor gaps; for this document, the property-based review was still undone and rules built before their owning service had no note. Amended: the property-based classification reviewed rule by rule, ' + Object.keys(CLASSIFICATION_REVIEW).length + ' values corrected with reasons in Section 2 (' + rules.filter((r) => r.derived).length + ' "yes" derived, ' + rules.filter((r) => r.arithmetic).length + ' after review), and the open point now waits only on the reviewer agent\'s confirmation; ' + earlyRules.length + ' rules whose Phase precedes their owning service (' + earlyRules.map((r) => r.id).join(', ') + ') are marked with the owner\'s phase and explained in a note under Section 2, as the workflows are under Section 3' + (orphanRules.length ? ', and the ' + orphanRules.length + ' of them no slice of their owner names (' + orphanRules.map((r) => r.id).join(', ') + ') are a new open point' : '') + ' |');
p('| 2026-09-26 | Round-6 scorecard, remediation round 7 | Group F was approved with minor gaps; for this document, the implementation cells of `BR-FIN-017` and `BR-FIN-018` named Finance while the open points said Platform builds them. Closed at the source: Appendix S now gives both rules to Platform (ADR-0027), so their owner and implementation cells name `Nibras.Platform.Domain.Rules` and the open point on rules built only by another service no longer arises |');
p('| 2026-09-26 | Open Question 30 decided (ADR-0027) | Amended: `BR-FIN-017` is no longer marked contested in Sections 2 and 6; Section 2 states the decision (the master brief Section 36 count, owned and computed by Platform, brief v9.7) and its property-based reason follows the rewritten rule; RISK-52 is Closed |');
p();
p('## How this document is verified');
p();
p('| Claim | Proof |');
p('|---|---|');
p('| Every rule and workflow has a row | The document is generated from the appendices; the coverage check in Section 7 is recomputed on every regeneration |');
p('| The document is current | Kit-lint rule R23 reruns `gen-31.mjs --check` and fails when Appendices R or S, document 34 (and through it document 17) or document 05 changed since it was generated |');
p('| Every rule has a test class that exists in code | Once code exists, an architecture test enumerates `BR-` comments and asserts the named test class exists |');
p('| Every worked example is a test row | `/simulate-year` and the business-rules-reviewer agent compare Appendix S examples with the test data sources |');
p('| Mutation targets are met | Stryker.NET in the pipeline, gated at 80% on the classes in Section 6 |');
p();
writeGenerated(K + 'docs/plan/31-business-rules-and-workflows.md', L.join('\n'));
console.log('rules ' + rules.length + ', workflows ' + flows.length + ', arithmetic ' + rules.filter((r) => r.arithmetic).length);
console.log('no owner ' + ruleNoOwner.length + ', no test ' + ruleNoTest.length + ', dup test ' + dupTest.length + ', wf no owner ' + flowNoOwner.length + ', unknown phase: ' + (unknownPhase.join(',') || 'none'));
console.log('phases found: ' + [...phase.entries()].map(([k, v]) => k + '=' + v).join(' '));
