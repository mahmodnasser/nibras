// Validate the work-breakdown parts and, with --write, assemble document 34.
//   node assemble-34.mjs --part A     validate one part (writers run this)
//   node assemble-34.mjs --part C --phase 3   validate one phase of a two-phase part before its second phase exists
//   node assemble-34.mjs              validate all parts that exist
//   node assemble-34.mjs --write      validate all four and write docs/plan/34-work-breakdown.md
//   node assemble-34.mjs --write --partial   write from the parts that exist; missing phases are listed as pending
import { fileURLToPath as __toPath } from 'node:url';
import { dirname as __dirOf, resolve as __resolve } from 'node:path';
// Paths resolve from this file, so the script runs from any checkout on Windows or Linux.
const __here = __dirOf(__toPath(import.meta.url)).split(String.fromCharCode(92)).join('/');
const __kit = __resolve(__here, '../..').split(String.fromCharCode(92)).join('/') + '/';
import { readFileSync, writeFileSync, existsSync } from 'node:fs';

const S = __here + '/parts/';
const K = __kit;
const args = process.argv.slice(2);
const onlyPart = args.includes('--part') ? args[args.indexOf('--part') + 1] : null;
const write = args.includes('--write');
const partial = args.includes('--partial');
const onlyPhase = args.includes('--phase') ? args[args.indexOf('--phase') + 1] : null;

const RANGES = { A: [1, 199], B: [200, 399], C: [400, 599], D: [600, 799] };
const PHASES = { A: ['1'], B: ['2'], C: ['3', '4'], D: ['5', '6'] };
// Which phase of a two-phase part owns its cross-cutting requirements: part C's are
// integrations (phase 3), part D's are infrastructure hardening (phase 6).
const CROSS_CUTTING_PHASE = { A: '1', B: '2', C: '3', D: '6' };

// ---- catalogs ----------------------------------------------------------------
const c03 = readFileSync(K + 'docs/plan/03-requirements-catalog.md', 'utf8');
const REQS = new Set([...c03.matchAll(/^\| (REQ-[A-Z0-9]+-\d{3}) \|/gm)].map((m) => m[1]));
const c31 = readFileSync(K + 'docs/plan/31-business-rules-and-workflows.md', 'utf8');
const BRS = new Set([...c31.matchAll(/`(BR-[A-Z0-9]+-\d{3})`/g)].map((m) => m[1]));
const WFS = new Set([...c31.matchAll(/`(WF-[A-Z]+-\d{2})`/g)].map((m) => m[1]));
const c17 = readFileSync(K + 'docs/plan/17-roadmap.md', 'utf8');
const CAPS = new Map(); // id -> phase
const PHASE_SERVICES = {}; // phase -> services its capabilities name
let curPhase = null;
for (const line of c17.split('\n')) {
  const ph = /^#### Phase (\d):/.exec(line);
  if (ph) curPhase = ph[1];
  const cap = /^\| (CAP-[A-Z0-9]+-\d{2}) \|/.exec(line);
  if (cap && curPhase) {
    CAPS.set(cap[1], curPhase);
    PHASE_SERVICES[curPhase] = PHASE_SERVICES[curPhase] || new Set();
    for (const svc of line.split('|')[3].split(',')) PHASE_SERVICES[curPhase].add(svc.trim());
  }
}
// With --phase, a two-phase part is checked against the requirements of that phase's
// services only; cross-cutting requirements go to CROSS_CUTTING_PHASE.
const scope = (w) => {
  const f = S + 'wb-scope-' + w + '.md';
  if (!existsSync(f)) return new Set();
  const out = new Set();
  for (const line of readFileSync(f, 'utf8').split(/\r?\n/)) {
    const m = /^\| (REQ-[A-Z0-9]+-\d{3}) \|/.exec(line);
    if (!m) continue;
    const svc = line.split('|')[4].trim();
    if (onlyPhase && PHASES[w].includes(onlyPhase) && !(PHASE_SERVICES[onlyPhase].has(svc) || (svc === 'cross-cutting' && CROSS_CUTTING_PHASE[w] === onlyPhase))) continue;
    out.add(m[1]);
  }
  return out;
};

// ---- parse a part --------------------------------------------------------------
function parse(w) {
  const file = S + 'wb-part-' + w + '.md';
  if (!existsSync(file)) return null;
  const text = readFileSync(file, 'utf8');
  const caps = [];
  let cur = null;
  for (const line of text.split(/\r?\n/)) {
    const h = /^### (CAP-[A-Z0-9]+-\d{2})\b\s*(.*)$/.exec(line);
    if (h) { cur = { id: h[1], title: h[2].trim(), slices: [], raw: [] }; caps.push(cur); }
    if (/^### Coverage notes/.test(line)) cur = null;
    if (cur) cur.raw.push(line);
    const r = /^\| (SL-[A-Z0-9]+-\d{3}) \|/.exec(line);
    if (r && cur) {
      const c = line.split('|').slice(1, -1).map((x) => x.trim());
      cur.slices.push({ id: c[0], what: c[1], service: c[2], days: c[3], covers: c[4] || '', uses: c[5] || '', deps: c[6] || '', cells: c.length });
    }
  }
  const notes = (/### Coverage notes[^\n]*\n([\s\S]*)$/.exec(text) || [])[1] || '';
  return { w, text, caps, notes };
}

// ---- validate -----------------------------------------------------------------
function validate(parts) {
  const problems = [];
  const allSlices = new Map();
  const covered = new Set();
  const capSeen = new Map();
  for (const p of parts) {
    const [lo, hi] = RANGES[p.w];
    for (const cap of p.caps) {
      if (!CAPS.has(cap.id)) problems.push(p.w + ': ' + cap.id + ' is not a roadmap capability');
      else if (!PHASES[p.w].includes(CAPS.get(cap.id))) problems.push(p.w + ': ' + cap.id + ' belongs to phase ' + CAPS.get(cap.id) + ', not this part');
      if (capSeen.has(cap.id)) problems.push(p.w + ': ' + cap.id + ' written twice');
      capSeen.set(cap.id, p.w);
      if (!cap.slices.length) problems.push(p.w + ': ' + cap.id + ' has no slices');
      for (const s of cap.slices) {
        if (s.cells !== 7) problems.push(p.w + ': ' + s.id + ' has ' + s.cells + ' columns, needs 7');
        const n = Number(s.id.split('-')[2]);
        if (n < lo || n > hi) problems.push(p.w + ': ' + s.id + ' is outside the range ' + lo + ' to ' + hi);
        if (allSlices.has(s.id)) problems.push(p.w + ': ' + s.id + ' duplicated');
        allSlices.set(s.id, { ...s, cap: cap.id, part: p.w });
        if (!/^[123]$/.test(s.days)) problems.push(p.w + ': ' + s.id + ' has ' + s.days + ' days; must be 1, 2 or 3');
        const reqs = s.covers.match(/REQ-[A-Z0-9]+-\d{3}/g) || [];
        if (!reqs.length) problems.push(p.w + ': ' + s.id + ' covers no requirement');
        for (const r of reqs) { if (!REQS.has(r)) problems.push(p.w + ': ' + s.id + ' cites unknown ' + r); covered.add(r); }
        for (const b of s.covers.match(/BR-[A-Z0-9]+-\d{3}/g) || []) if (!BRS.has(b)) problems.push(p.w + ': ' + s.id + ' cites unknown ' + b);
        for (const f of s.covers.match(/WF-[A-Z]+-\d{2}/g) || []) if (!WFS.has(f)) problems.push(p.w + ': ' + s.id + ' cites unknown ' + f);
        if (/\b(TODO|TBD|FIXME|to be decided|to be determined)\b/i.test(Object.values(s).join(' '))) problems.push(p.w + ': ' + s.id + ' contains a forbidden placeholder word');
      }
    }
    // Requirements from this part's scope, satisfied either by a slice or named in the coverage notes.
    for (const r of scope(p.w)) if (!covered.has(r) && !p.notes.includes(r)) problems.push(p.w + ': ' + r + ' in scope but covered by no slice and not explained in the coverage notes');
    // Missing capabilities of this part's phases.
    for (const [cap, ph] of CAPS) if (PHASES[p.w].includes(ph) && (!onlyPhase || !PHASES[p.w].includes(onlyPhase) || ph === onlyPhase) && !p.caps.some((c) => c.id === cap)) problems.push(p.w + ': ' + cap + ' (phase ' + ph + ') has no section');
  }
  // Slice-to-slice dependencies must exist.
  for (const s of allSlices.values()) for (const d of s.deps.match(/SL-[A-Z0-9]+-\d{3}/g) || []) if (!allSlices.has(d)) problems.push(s.part + ': ' + s.id + ' depends on unknown ' + d);
  return { problems, allSlices, covered };
}

const partIds = onlyPart ? [onlyPart] : ['A', 'B', 'C', 'D'];
const parts = partIds.map(parse).filter(Boolean);
const { problems, allSlices, covered } = validate(parts);
const days = [...allSlices.values()].reduce((t, s) => t + Number(s.days || 0), 0);
console.log('parts: ' + parts.map((p) => p.w).join(',') + ' | capabilities: ' + parts.reduce((t, p) => t + p.caps.length, 0) + ' | slices: ' + allSlices.size + ' | days: ' + days + ' | requirements covered: ' + covered.size);
console.log('problems: ' + problems.length);
for (const pr of problems.slice(0, 60)) console.log('  ' + pr);
if (problems.length > 60) console.log('  ... ' + (problems.length - 60) + ' more');

if (!write) process.exit(problems.length ? 1 : 0);
if (parts.length !== 4 && !partial) { console.log('NOT WRITTEN: all four parts are needed'); process.exit(1); }
// A note that hands a requirement to another part ("built ... by part D") is only
// a promise; the assembly holds that part to it, so no requirement is lost between parts.
const pending = [];
for (const part of parts) {
  for (const line of part.notes.split('\n')) {
    const target = /by part ([ABCD])\b/.exec(line);
    if (!target) continue;
    for (const r of line.match(/REQ-[A-Z0-9]+-\d{3}/g) || []) {
      const owner = parts.find((x) => x.w === target[1]);
      const ownerDone = owner && !(onlyPhase && PHASES[owner.w].includes(onlyPhase));
      if (!ownerDone && partial) { pending.push(r + ' (handed to part ' + target[1] + ')'); continue; }
      const built = owner && owner.caps.some((c) => c.slices.some((s) => s.covers.includes(r)));
      if (!built) problems.push(part.w + ': ' + r + ' is handed to part ' + target[1] + ', which builds no slice for it');
    }
  }
}
const present = new Set(parts.map((x) => x.w));
// In a partial build only the scopes of the parts that exist must be covered.
const inPlay = partial ? new Set(parts.flatMap((x) => [...scope(x.w)])) : REQS;
const uncoveredAll = [...inPlay].filter((r) => !covered.has(r) && !parts.some((p) => p.notes.includes(r)));
if (problems.length || uncoveredAll.length) { console.log('NOT WRITTEN: ' + problems.length + ' problems, ' + uncoveredAll.length + ' requirements uncovered'); process.exit(1); }

// ---- assemble -------------------------------------------------------------------
const phaseOf = (capId) => CAPS.get(capId);
const perPhase = {};
for (const s of allSlices.values()) {
  const ph = phaseOf(s.cap);
  perPhase[ph] = perPhase[ph] || { caps: new Set(), slices: 0, days: 0 };
  perPhase[ph].caps.add(s.cap); perPhase[ph].slices++; perPhase[ph].days += Number(s.days);
}
const L = [];
const p = (x = '') => L.push(x);
p('# 34. Work Breakdown');
p();
p('## Purpose');
p();
p('This is the document an engineer picks work from. It turns every capability in `17-roadmap.md` into **slices**: one use case, built through every layer it touches, one to three days, one pull request. Every requirement in `03-requirements-catalog.md` is built by at least one slice here, and every slice names the requirements, rules and workflows it covers, so nothing is built that nobody asked for and nothing asked for is left unbuilt.');
p();
p('The sizing rules and the reasons for them are in `PLAN_SPEC.md`, "How work is broken down", and in ADR-0018. The slice template is `docs/templates/work-slice.md`. The ordered queue is `docs/project/BACKLOG.md`, filled from this document with `/plan-tasks`.');
p();
p('## Scope');
p();
p('| In scope | Owned elsewhere |');
p('|---|---|');
p('| Every slice of every capability, with its estimate, coverage and dependencies | The capabilities, phases and critical path: document 17 |');
p('| The definition of done that every slice meets | The coverage matrix behind it: Appendix V |');
p('| Totals per phase, computed from the slices | The phase ranges: master brief Section 28 |');
p();
p('## Content');
p();
p('### 1. How to read this document');
p();
p('| Column | Meaning |');
p('|---|---|');
p('| Slice | `SL-<AREA>-<NNN>`. Numbers 001 to 199 are phase 1, 200 to 399 phase 2, 400 to 599 phases 3 and 4, 600 to 799 phases 5 and 6, so the number tells you the phase |');
p('| When it ships, a person can | The behaviour someone can see when the pull request merges. Never a layer, a table or a technology |');
p('| Days | One, two or three. A larger estimate is two slices not yet separated |');
p('| Covers | The `REQ-`, `BR-` and `WF-` identifiers this slice satisfies. The traceability matrix in document 20 is generated from this column |');
p('| Use cases | The folders under `Application/Features/`, the consumers and the jobs, named exactly as in the service sheet\'s tree |');
p('| Depends on | Stated as a contract wherever possible, so a consuming slice can start as soon as the contract is published rather than when its producer finishes |');
p();
p('**Every slice is done only when all of this is true.** Domain and application logic with unit tests from the Appendix S examples; persistence, migration, tenant filter and row-level security policy; the endpoint with its permission, validation, error codes and OpenAPI; events through the outbox and consumers idempotent through the inbox; integration tests for tenant isolation, the permission matrix and the query budget; the screen in English and Arabic, right to left, with every state, if the slice has one; traceability updated; and a demonstration to someone who did not write it.');
p();
p('### 2. Totals');
p();
p('| Phase | Capabilities | Slices | Slice-days | Roadmap range |');
p('|---|---|---|---|---|');
const ranges = { 1: '8 to 10 weeks', 2: '16 to 20 weeks', 3: '10 to 12 weeks', 4: '8 to 10 weeks', 5: '10 to 14 weeks', 6: '6 to 8 weeks' };
let tc = 0, ts = 0, td = 0;
for (const ph of Object.keys(perPhase).sort()) { const v = perPhase[ph]; tc += v.caps.size; ts += v.slices; td += v.days; p('| ' + ph + ' | ' + v.caps.size + ' | ' + v.slices + ' | ' + v.days + ' | ' + ranges[ph] + ' |'); }
p('| **Total** | **' + tc + '** | **' + ts + '** | **' + td + '** | |');
p();
p('**Reading the slice-days against the ranges.** Slice-days are single-person working days of build effort. With the four streams in document 17 Section 6 working in parallel, and allowing for review, integration and the demonstration at each capability, the calendar time per phase is roughly slice-days divided by the number of engineers in the phase\'s streams, then multiplied by about 1.3. Where that disagrees with the roadmap range by more than a quarter, the roadmap is re-estimated at the end of phase 0, not the slices shortened.');
p();
const phaseNames = { 1: 'Foundation', 2: 'The school year loop', 3: 'Money and paperwork', 4: 'Growth', 5: 'Extended', 6: 'Hardening and launch' };
let sec = 3;
for (const ph of ['1', '2', '3', '4', '5', '6']) {
  p('### ' + sec++ + '. Phase ' + ph + ': ' + phaseNames[ph]);
  p();
  const owner = Object.keys(PHASES).find((w) => PHASES[w].includes(ph));
  if (!present.has(owner) || (onlyPhase && PHASES[owner].includes(onlyPhase) && ph !== onlyPhase && !parts.some((x) => x.caps.some((c) => phaseOf(c.id) === ph)))) {
    const caps = [...CAPS].filter(([, x]) => x === ph).map(([c]) => c);
    p('**Not yet broken down.** This phase is written in the next step of the plan build (part ' + owner + ', slice numbers ' + RANGES[owner].join(' to ') + '). Its capabilities, from document 17: ' + caps.join(', ') + '.');
    p();
    continue;
  }
  for (const part of parts) for (const cap of part.caps) if (phaseOf(cap.id) === ph) {
    const body = cap.raw.slice(1).join('\n').trim();
    p('#### ' + cap.id + ' ' + cap.title);
    p();
    p(body);
    p();
  }
}
p('### ' + sec++ + '. Coverage');
p();
p('| Check | Result |');
p('|---|---|');
p('| Requirements in document 03 | ' + REQS.size + ' |');
if (partial) p('| Requirements in the phases broken down so far | ' + inPlay.size + ' |');
p('| Requirements built by at least one slice | ' + [...REQS].filter((r) => covered.has(r)).length + ' |');
p('| Requirements satisfied by a gate rather than a slice, with the reason | ' + [...inPlay].filter((r) => !covered.has(r)).length + ', listed below |');
if (partial) p('| Requirements handed to a phase not yet broken down | ' + pending.length + ' |');
p('| Capabilities in document 17 with slices | ' + tc + ' of ' + CAPS.size + ' |');
p('| Slices with no requirement | 0 |');
p('| Slices over three days | 0 |');
p();
for (const part of parts) if (part.notes.trim()) { p('**Notes from part ' + part.w + '.**'); p(); p(part.notes.trim()); p(); }
p('## Decisions in force');
p();
p('| Decision | Record |');
p('|---|---|');
p('| Three levels: phase, capability, slice; split by use case, never by layer | ADR-0018 |');
p('| Estimates are day ranges of one to three days, never points | ADR-0018 |');
p('| Dependencies are stated as contracts wherever possible | ADR-0018; document 17 Section 6 |');
p();
p('## Dependencies on other documents');
p();
p('| Document | What this one takes |');
p('|---|---|');
p('| `17-roadmap.md` | Every capability, its phase and its services |');
p('| `03-requirements-catalog.md` | Every requirement identifier |');
p('| `31-business-rules-and-workflows.md` | Rule and workflow ownership |');
p('| `06-services/*.md` | The use-case folders, consumers and jobs each slice builds |');
p('| `20-traceability-matrix.md` | Generated from the Covers column here |');
p();
p('## Open points');
p();
p('| Point | Default | Owner |');
p('|---|---|---|');
p('| Slice-days versus roadmap ranges | Re-estimated at the end of phase 0 with the team actually assembled; the slices are not shortened to fit | Architect and product owner |');
p('| Team size changes the calendar, not the slices | Open Question 24 | Product owner |');
p();
p('## Review record');
p();
p('| Date | Reviewer | Result |');
p('|---|---|---|');
p('| ' + new Date().toISOString().slice(0, 10) + ' | Plan build, assembled and validated by script | ' + ts + ' slices' + (partial ? '; phases ' + [...present].flatMap((w) => PHASES[w]).join(', ') + ' complete, the rest pending' : ', every requirement covered') + ' |');
p();
p('## How this document is verified');
p();
p('| Claim | Proof |');
p('|---|---|');
p('| Every requirement is built by a slice or satisfied by a named gate | The assembly script refuses to write this document otherwise; the Coverage table above is computed, not typed |');
p('| Every identifier exists | `kit-lint` rule R19 over this document |');
p('| No slice exceeds three days or covers nothing | The assembly script |');
p('| Every capability in the roadmap has slices | The assembly script, both directions |');
p('| The estimates were honest | Re-estimated at the end of each phase against the slices actually delivered, recorded in the review record |');
p();
writeFileSync(K + 'docs/plan/34-work-breakdown.md', L.join('\n'), 'utf8');
console.log('WRITTEN docs/plan/34-work-breakdown.md');
