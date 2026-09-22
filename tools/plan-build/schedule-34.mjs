// Compute each phase's calendar range from document 34's slices, so document 17's
// ranges are derived rather than typed.
//   node tools/plan-build/schedule-34.mjs
// Throughput: slice-days x overhead / (builders x 5 days a week), for the smallest and
// largest team in master brief Section 29. Floor: the longest chain of slice-to-slice
// dependencies inside the phase, in working days x overhead, which no headcount shortens.
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';

const here = dirname(fileURLToPath(import.meta.url));
const doc = readFileSync(join(here, '..', '..', 'docs', 'plan', '34-work-breakdown.md'), 'utf8');
const roadmap = readFileSync(join(here, '..', '..', 'docs', 'plan', '17-roadmap.md'), 'utf8');

export const TEAM = { min: 5, max: 8 }; // two to four backend, one to two web, one mobile, one quality engineer
export const OVERHEAD = 1.3; // review, integration and the capability demonstration
// Phase 6 also waits on the calendar, not only on effort: the external penetration test
// window and its retest, and a restore drill in an isolated environment.
export const CALENDAR_FLOOR = { 6: { low: 6, high: 8 } };

const capPhase = new Map();
let ph = null;
for (const l of roadmap.split('\n')) {
  const m = /^#### Phase (\d):/.exec(l); if (m) ph = m[1];
  const c = /^\| (CAP-[A-Z0-9]+-\d{2}) \|/.exec(l); if (c && ph) capPhase.set(c[1], ph);
}
const slices = new Map();
let cap = null;
for (const l of doc.split('\n')) {
  const h = /^#### (CAP-[A-Z0-9]+-\d{2})\b/.exec(l); if (h) cap = h[1];
  const r = /^\| (SL-[A-Z0-9]+-\d{3}) \|/.exec(l);
  if (!r || !cap) continue;
  const c = l.split('|').slice(1, -1).map((x) => x.trim());
  slices.set(r[1], { id: r[1], cap, phase: capPhase.get(cap), days: Number(c[3]), deps: (c[6].match(/SL-[A-Z0-9]+-\d{3}/g) || []) });
}

// Longest dependency chain ending at each slice, within its phase.
const memo = new Map();
const chain = (id, seen = new Set()) => {
  if (memo.has(id)) return memo.get(id);
  if (seen.has(id)) throw new Error('dependency cycle at ' + id);
  seen.add(id);
  const s = slices.get(id);
  let best = 0;
  for (const d of s.deps) { const t = slices.get(d); if (t && t.phase === s.phase) best = Math.max(best, chain(d, seen)); }
  seen.delete(id);
  memo.set(id, best + s.days);
  return best + s.days;
};

export function schedule(capFilter = () => true) {
  const out = {};
  for (const s of slices.values()) {
    if (!capFilter(s.cap)) continue;
    const o = (out[s.phase] ||= { slices: 0, days: 0, chain: 0, caps: new Set() });
    o.slices++; o.days += s.days; o.caps.add(s.cap); o.chain = Math.max(o.chain, chain(s.id));
  }
  for (const [p, o] of Object.entries(out)) {
    const weeks = (builders) => (o.days * OVERHEAD) / (builders * 5);
    const cal = CALENDAR_FLOOR[p] || { low: 0, high: 0 };
    const floor = (o.chain * OVERHEAD) / 5;
    o.low = Math.ceil(Math.max(weeks(TEAM.max), floor, cal.low));
    o.high = Math.ceil(Math.max(weeks(TEAM.min), floor, cal.high));
    o.floor = Math.ceil(floor);
  }
  return out;
}

if (process.argv[1] && /schedule-34\.mjs$/.test(process.argv[1])) {
  const all = schedule();
  let lo = 0, hi = 0;
  console.log('phase  caps slices days  chain-days  floor-wk  range-wk');
  for (const p of Object.keys(all).sort()) {
    const o = all[p]; lo += o.low; hi += o.high;
    console.log(String(p).padEnd(7) + String(o.caps.size).padStart(4) + String(o.slices).padStart(7) + String(o.days).padStart(5) + String(o.chain).padStart(12) + String(o.floor).padStart(10) + ('  ' + o.low + ' to ' + o.high).padStart(10));
  }
  console.log('total  ' + lo + ' to ' + hi + ' weeks');
  const mvpCaps = new Set(['CAP-RQS-01', 'CAP-COM-01', 'CAP-COM-02', 'CAP-DOC-02']);
  const mvp = schedule((c) => ['1', '2'].includes(capPhase.get(c)) || mvpCaps.has(c));
  const m = Object.values(mvp);
  console.log('MVP    caps ' + m.reduce((t, o) => t + o.caps.size, 0) + ', days ' + m.reduce((t, o) => t + o.days, 0) + ', weeks ' + m.reduce((t, o) => t + o.low, 0) + ' to ' + m.reduce((t, o) => t + o.high, 0));
}
