// Generate docs/plan/20-traceability-matrix.md from documents 03, 17, 31, 34
// and the service sheets' test plans. Every row is derived; none is typed.
import { fileURLToPath as __toPath } from 'node:url';
import { dirname as __dirOf, resolve as __resolve } from 'node:path';
// Paths resolve from this file, so the script runs from any checkout on Windows or Linux.
const __here = __dirOf(__toPath(import.meta.url)).split(String.fromCharCode(92)).join('/');
const __kit = __resolve(__here, '../..').split(String.fromCharCode(92)).join('/') + '/';
import { readFileSync, readdirSync, existsSync } from 'node:fs';
import { writeGenerated } from './write-generated.cjs';
import { buildContext, testCaseOwnership } from '../kit-lint/kit-lint.mjs';


const K = __kit;
const read = (p) => readFileSync(K + p, 'utf8');
if (!existsSync(K + 'docs/plan/34-work-breakdown.md')) { console.log('document 34 does not exist yet'); process.exit(1); }

// ---- requirements ---------------------------------------------------------------
const reqs = [];
for (const l of read('docs/plan/03-requirements-catalog.md').split('\n')) {
  if (!l.startsWith('| REQ-')) continue;
  const c = l.split('|').slice(1, -1).map((x) => x.trim());
  reqs.push({ id: c[0], text: c[1], tier: (/[123]/.exec(c[2]) || ['?'])[0], service: c[3], wfbr: c[5] || '', acceptance: c[6] || '' });
}

// ---- capability phases from 17, slices from 34 ------------------------------------
const capPhase = new Map();
let ph = null;
for (const l of read('docs/plan/17-roadmap.md').split('\n')) {
  const m = /^#### Phase (\d):/.exec(l); if (m) ph = m[1];
  const c = /^\| (CAP-[A-Z0-9]+-\d{2}) \|/.exec(l); if (c && ph) capPhase.set(c[1], ph);
}
const reqSlices = new Map();
const slicePhase = new Map();
let cap = null;
for (const l of read('docs/plan/34-work-breakdown.md').split('\n')) {
  const h = /^#### (CAP-[A-Z0-9]+-\d{2})\b/.exec(l); if (h) cap = h[1];
  const r = /^\| (SL-[A-Z0-9]+-\d{3}) \|/.exec(l);
  if (!r || !cap) continue;
  const cells = l.split('|').slice(1, -1).map((x) => x.trim());
  slicePhase.set(r[1], capPhase.get(cap));
  for (const id of (cells[4] || '').match(/REQ-[A-Z0-9]+-\d{3}/g) || []) {
    if (!reqSlices.has(id)) reqSlices.set(id, []);
    reqSlices.get(id).push(r[1]);
  }
}

// ---- test cases: from 03's acceptance column, and from the sheets' test plans ------
const reqTests = new Map();
const addTest = (req, tc) => { if (!reqTests.has(req)) reqTests.set(req, new Set()); reqTests.get(req).add(tc); };
for (const r of reqs) for (const tc of r.acceptance.match(/TC-[A-Z0-9]+-\d{3}/g) || []) addTest(r.id, tc);
for (const f of readdirSync(K + 'docs/plan/06-services').filter((x) => x.endsWith('.md'))) {
  for (const l of read('docs/plan/06-services/' + f).split('\n')) {
    if (!/^\| `?TC-[A-Z0-9]+-\d{3}/.test(l)) continue;
    const tcs = l.match(/TC-[A-Z0-9]+-\d{3}/g) || [];
    for (const req of l.match(/REQ-[A-Z0-9]+-\d{3}/g) || []) for (const tc of tcs.slice(0, 1)) addTest(req, tc);
  }
}
// A requirement's derived acceptance test is TC-<AREA>-(950 + requirement number). It is
// assigned when no source names a test for the requirement, and also when another plan
// document already cites that identifier (a derived test named beside a sheet's test), so
// this document and the registry (gen-tc-registry.mjs) count the same derived tests: an
// identifier in the 950 to 999 range cited outside the history documents (30 and
// docs/project) and outside this document and the annex.
const own = testCaseOwnership(buildContext(K));
const TC_HISTORY = /^docs\/(project\/|plan\/30-)/;
const SELF = /^docs\/plan\/(20-|16-annex-)/;
const citedElsewhere = new Set();
for (const [id, list] of own.cites) {
  if (!own.derivedReq(id)) continue;
  if (list.some((c) => !TC_HISTORY.test(c.file) && !SELF.test(c.file))) citedElsewhere.add(id);
}
const fromSources = reqs.filter((r) => reqTests.has(r.id)).length;
let derived = 0;
let derivedBeside = 0;
const derivedOnly = [];
for (const r of reqs) {
  const [, area, num] = r.id.split('-');
  const n = Number(num);
  const id = 'TC-' + area + '-' + (950 + n);
  const sourced = reqTests.has(r.id);
  if (sourced && !citedElsewhere.has(id)) continue;
  if (n > 49) throw new Error(r.id + ' exceeds the derived range; extend the rule');
  addTest(r.id, id);
  derived++;
  if (sourced) derivedBeside++; else derivedOnly.push([r.id, id]);
}

// ---- plan document and phase per requirement ----------------------------------------
const sheet = (svc) => '06-services/' + svc.toLowerCase().replace('.', '-') + '.md';
const areaDoc = { SEC: '12', PRV: '12, 27', PERF: '21', L10N: '24', UX: '14', WEB: '08', MOB: '09', API: '22', INT: '23', DATA: '10', MSG: '11', INF: '15', TST: '16', PLAT: '33' };
const svcPhase = { Gateway: '1', 'Bff.Web': '1', 'Bff.Mobile': '2', Identity: '1', Platform: '1', School: '2', Admissions: '4', Academics: '2', Assessment: '2', Scheduling: '2', Attendance: '2', Finance: '3', Communication: '3', Notification: '1', Requests: '3', Documents: '3', Behavior: '4', Reporting: '4', Audit: '1', Wellbeing: '5', Hr: '5', Operations: '5', Ai: '5' };
// The Platform column names the runners or devices the requirement's tests name in
// their definitions; otherwise the area's default, from document 33's runner matrix.
const RUNNERS = [['ubuntu-latest', /ubuntu-latest/i], ['windows-latest', /windows-latest/i], ['macos-latest', /macos-latest/i], ['Android', /\bAndroid\b/], ['iOS', /\biOS\b/], ['Chromium', /Chromium/], ['Firefox', /Firefox/], ['WebKit', /WebKit/], ['device farm', /device (farm|pass)/i]];
// Requirements whose tests run on something other than a CI runner: the platform is
// named from document 33, because their test definitions name none the pattern finds.
const PLATFORM_OVERRIDE = {
  'REQ-PLAT-004': 'Hyper-V and VMware: the appliance images built in the nested-virtualisation job of `release.yml`, and the quarterly drill on real Hyper-V (document 33 part 8)',
  'REQ-PLAT-005': 'Chromium, Firefox and WebKit under Playwright on ubuntu-latest; real Chrome, Edge, Firefox, Safari and Samsung Internet on the device pass (document 33 parts 1 and 4)',
  'REQ-PLAT-006': 'NVDA and Narrator on Windows, VoiceOver on macOS and iOS, TalkBack on Android, in the manual screen-reader pass of each release (document 33 part 1)',
  'REQ-PLAT-007': 'Windows 11 kiosk (MSIX, built on windows-latest) and Ubuntu 22.04 or later kiosk (Linux bundle, built on ubuntu-latest) (document 33 part 7)',
};
const platformOf = (area, tests, req) => {
  if (PLATFORM_OVERRIDE[req]) return PLATFORM_OVERRIDE[req];
  const text = tests.map((t) => (own.defs.get(t) || []).map((d) => d.text).join(' ')).join(' ');
  const named = RUNNERS.filter(([, re]) => re.test(text)).map(([n]) => n);
  if (named.length) return named.join(', ');
  return ({ PLAT: 'ubuntu-latest and windows-latest; dev-smoke also macos-latest (document 33 part 4)', MOB: 'Android, iOS', WEB: 'Chromium, Firefox, WebKit' }[area] || 'any');
};

const rows = [];
let noSlice = 0;
for (const r of reqs) {
  const area = r.id.split('-')[1];
  const slices = reqSlices.get(r.id) || [];
  if (!slices.length) noSlice++;
  const phases = [...new Set(slices.map((s) => slicePhase.get(s)).filter(Boolean))].sort();
  const phase = phases.length ? phases.join(', ') : (svcPhase[r.service] || '1');
  const doc = r.service === 'cross-cutting' ? (areaDoc[area] || '03') : sheet(r.service);
  const wf = (r.wfbr.match(/WF-[A-Z]+-\d{2}/g) || []).join(', ') || 'none';
  const br = (r.wfbr.match(/BR-[A-Z0-9]+-\d{3}/g) || []).join(', ') || 'none';
  const summary = r.text.replace(/\|/g, '/').replace(/\s+/g, ' ');
  rows.push('| ' + r.id + ' | ' + (summary.length > 110 ? summary.slice(0, 107) + '...' : summary) + ' | ' + r.tier + ' | ' + r.service + ' | ' + wf + ' | ' + br + ' | ' + doc + ' | ' + phase + ' | ' + (slices.join(', ') || 'gate, see document 34') + ' | ' + [...reqTests.get(r.id)].join(', ') + ' | ' + platformOf(area, [...reqTests.get(r.id)], r.id) + ' | Planned |');
}

const L = [];
const p = (x = '') => L.push(x);
p('# 20. Traceability Matrix');
p();
p('## Purpose');
p();
p('One row per requirement, from its identifier to the service that owns it, the workflow and rule it depends on, the plan document that specifies it, the phase and slices that build it, and the test that proves it. **No requirement is unmapped and no test cell is empty.** This document is generated from documents 03, 17, 31 and 34 and from the service sheets\' test plans; editing it by hand is a defect. `docs/project/TRACEABILITY.md` continues it through the build, adding status.');
p();
p('## Scope');
p();
p('| In scope | Owned elsewhere |');
p('|---|---|');
p('| The mapping for every requirement in document 03 | The requirement text: document 03 |');
p('| The test identifier that proves each requirement | The test itself: the service sheets and document 16 |');
p('| The slices that build each requirement | The slices: document 34 |');
p();
p('## Content');
p();
p('### 1. Summary');
p();
p('| Measure | Value |');
p('|---|---|');
p('| Requirements | ' + reqs.length + ' |');
p('| Requirements built by at least one slice | ' + (reqs.length - noSlice) + ' |');
p('| Requirements satisfied by a gate named in document 34 rather than a slice | ' + noSlice + ' |');
p('| Requirements with a test identifier | ' + reqs.length + ' |');
p('| Of which an identifier comes from document 03 or a service sheet\'s test plan | ' + fromSources + ' |');
p('| Of which an identifier is a derived acceptance test (Section 2) | ' + derived + (derivedBeside ? ', ' + derivedBeside + ' of them beside a test from a test plan, because another document already cites the derived test' : '') + ' |');
p();
p('### 2. Derived acceptance tests');
p();
p('Every requirement in document 03 has an acceptance criterion: either an existing test identifier or a Given, When, Then line with a concrete number. Where it is the latter and no service sheet\'s test plan names a test for it yet, the requirement\'s acceptance test takes the identifier **`TC-<AREA>-(950 + requirement number)`**, so the acceptance test of ' + derivedOnly[0][0] + ' is ' + derivedOnly[0][1] + ', specified by that requirement\'s Given, When, Then line. A requirement that has a test from a test plan also carries its derived test when another plan document already cites that derived identifier, so a cited derived test always has a slice that writes it; the registry in `16-annex-test-case-registry.md` counts the same derived tests. No other test in the kit uses the 950 to 999 range, and no area has more than 49 requirements, so the rule is collision-free. When the slice that builds the requirement writes the test, it uses this identifier and the matrix stays true without an edit.');
p();
p('### 3. Columns');
p();
p('| Column | Source |');
p('|---|---|');
p('| Requirement, Summary, Tier, Service | Document 03 |');
p('| Workflow, Rule | Document 03\'s Workflow / Rule column, whose identifiers document 31 assigns to code |');
p('| Plan document | The owning service\'s sheet, or for a cross-cutting requirement the document Appendix L section L.3 names for its area |');
p('| Phase | The phases of the capabilities whose slices build it, from documents 34 and 17 |');
p('| Slices | Document 34\'s Covers column |');
p('| Test case | Document 03\'s acceptance column, the service sheets\' test plans, or the derived acceptance test of Section 2 |');
p('| Platform | The runners or devices named in the definitions of the tests of the requirement (ubuntu-latest, windows-latest, macos-latest, Android, iOS, the browser engines, the device pass); for REQ-PLAT-004 to REQ-PLAT-007, whose tests run on hypervisors, browsers, screen readers and kiosks rather than a CI runner, the platforms document 33 names for them; otherwise the default of its area from document 33 part 4, or `any` |');
p('| Status | `Planned` until the build begins; `docs/project/TRACEABILITY.md` carries it forward |');
p();
p('### 4. The matrix');
p();
p('| Requirement | Summary | Tier | Service | Workflow | Rule | Plan document | Phase | Slices | Test case | Platform | Status |');
p('|---|---|---|---|---|---|---|---|---|---|---|---|');
for (const r of rows) p(r);
p();
p('## Decisions in force');
p();
p('| Decision | Record |');
p('|---|---|');
p('| Test case identifiers follow `TC-<AREA>-<NNN>` with a Covers line | ADR-0014 |');
p('| Derived acceptance tests use the 950 to 999 range | This document, Section 2 |');
p('| This document is generated, never edited by hand | This document |');
p();
p('## Dependencies on other documents');
p();
p('| Document | What this one takes |');
p('|---|---|');
p('| `03-requirements-catalog.md` | Every requirement, its tier, service, workflow, rule and acceptance |');
p('| `17-roadmap.md` | The phase of every capability |');
p('| `34-work-breakdown.md` | The slices that build every requirement |');
p('| `06-services/*.md` | The test plans |');
p();
p('## Open points');
p();
p('| Point | Default | Owner | L | I | Score | In the register |');
p('|---|---|---|---|---|---|---|');
p('| Areas that grow beyond 49 requirements overflow the derived range | Extend the rule into the next free range and regenerate; the generator refuses to run until then | Architect | 1 | 2 | 2 | none |');
p();
p('## Review record');
p();
p('| Date | Reviewer | Result |');
p('|---|---|---|');
p('| 2026-09-26 | Round-6 scorecard, remediation round 7 | Amended at the generator: a requirement with a test from a test plan also carries its derived test when another plan document cites it (REQ-MOB-038 carries TC-MOB-988), so this document and the registry count the same derived tests; the Section 2 example names a requirement whose acceptance test is its derived test |');
p('| ' + new Date().toISOString().slice(0, 10) + ' | Generated | ' + reqs.length + ' requirements, every one mapped to a test |');
p();
p('## How this document is verified');
p();
p('| Claim | Proof |');
p('|---|---|');
p('| No requirement is unmapped | Generated from document 03 row by row; the summary counts are computed |');
p('| Every identifier exists | `kit-lint` rule R19 over this document |');
p('| No test cell is empty | The generator assigns a derived acceptance test to any requirement without one |');
p('| The matrix is current | Kit-lint rule R23 reruns `gen-20.mjs --check` and fails when the matrix differs from what documents 03, 17, 31, 34 and the sheets produce today |');
p();
writeGenerated(K + 'docs/plan/20-traceability-matrix.md', L.join('\n'));
console.log('requirements ' + reqs.length + ', built by slices ' + (reqs.length - noSlice) + ', gates ' + noSlice + ', tests from sources ' + fromSources + ', derived ' + derived);
