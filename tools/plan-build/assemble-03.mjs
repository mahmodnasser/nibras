// Assemble the requirements catalog from staged chunks, taking each area from
// exactly one source, then validate every row before writing anything.
import { fileURLToPath as __toPath } from 'node:url';
import { dirname as __dirOf, resolve as __resolve } from 'node:path';
// Paths resolve from this file, so the script runs from any checkout on Windows or Linux.
const __here = __dirOf(__toPath(import.meta.url)).split(String.fromCharCode(92)).join('/');
const __kit = __resolve(__here, '../..').split(String.fromCharCode(92)).join('/') + '/';
import { readFileSync, writeFileSync } from 'node:fs';

const S = __here + '/parts/';
const K = __kit;
const target = K + 'docs/plan/03-requirements-catalog.md';
const write = process.argv.includes('--write');

/** Split a chunk into { area: sectionText } keyed by the "## AREA" heading. */
function sections(text) {
  const out = new Map();
  const parts = text.split(/\n(?=## [A-Z0-9]+ )/);
  for (const p of parts) {
    const m = /^## ([A-Z0-9]+) /.exec(p.trimStart());
    if (m) out.set(m[1], p.trim() + '\n');
  }
  return out;
}

// Which source supplies which area. One source per area, never two.
const plan = [
  ['chunk2.md', ['ASM', 'SCD', 'ATT', 'FIN']],
  ['req-part3.md', ['COM', 'NOT', 'RQS', 'DOC', 'BEH']],
  ['req-part4.md', ['RPT', 'AUD', 'WEL', 'HR', 'OPS', 'AI', 'GW', 'BFF']],
  ['req-part5.md', ['SEC', 'PRV', 'PERF', 'L10N', 'UX', 'WEB']],
  ['req-part6.md', ['MOB', 'API', 'INT', 'DATA', 'MSG', 'INF', 'TST', 'PLAT']],
];

let doc = readFileSync(target, 'utf8');
const present = new Set([...doc.matchAll(/^## ([A-Z0-9]+) /gm)].map((m) => m[1]));
let appended = [];
for (const [file, areas] of plan) {
  let text;
  try { text = readFileSync(S + file, 'utf8'); } catch { continue; }
  const secs = sections(text);
  for (const a of areas) {
    if (present.has(a)) continue;
    if (!secs.has(a)) { console.log('MISSING in ' + file + ': ' + a); continue; }
    doc = doc.replace(/\s*$/, '\n\n') + secs.get(a);
    present.add(a);
    appended.push(a);
  }
}

// ---- validation -----------------------------------------------------------
const wfIds = new Set(readFileSync(K + 'docs/brief/02-appendices/appendix-r-workflow-catalog.md', 'utf8').match(/WF-[A-Z]+-\d+/g));
const brIds = new Set(readFileSync(K + 'docs/brief/02-appendices/appendix-s-business-rules.md', 'utf8').match(/BR-[A-Z0-9]+-\d+/g));
const rows = doc.split('\n').filter((l) => l.startsWith('| REQ-'));
const problems = [];
const seen = new Set();
const lastPerArea = new Map();
const counts = new Map();
for (const r of rows) {
  const cells = r.split('|').slice(1, -1).map((c) => c.trim());
  if (cells.length !== 7) problems.push('columns=' + cells.length + ': ' + r.slice(0, 60));
  const [id, , tier] = cells;
  const m = /^REQ-([A-Z0-9]+)-(\d{3})$/.exec(id);
  if (!m) { problems.push('bad id ' + id); continue; }
  if (seen.has(id)) problems.push('duplicate ' + id);
  seen.add(id);
  const n = Number(m[2]);
  const prev = lastPerArea.get(m[1]) || 0;
  if (n !== prev + 1) problems.push('sequence ' + id + ' after ' + prev);
  lastPerArea.set(m[1], n);
  const c = counts.get(m[1]) || { 1: 0, 2: 0, 3: 0 };
  const t = (/[123]/.exec(tier) || ['?'])[0];
  if (t === '?') problems.push('no tier ' + id);
  else c[t]++;
  counts.set(m[1], c);
  for (const ref of (cells[5] || '').match(/WF-[A-Z]+-\d+/g) || []) if (!wfIds.has(ref)) problems.push(id + ' cites unknown ' + ref);
  for (const ref of (cells[5] || '').match(/BR-[A-Z0-9]+-\d+/g) || []) if (!brIds.has(ref)) problems.push(id + ' cites unknown ' + ref);
}
const forbidden = /\b(TODO|TBD|FIXME|to be decided|to be determined)\b/i;
doc.split('\n').forEach((l, i) => { if (forbidden.test(l)) problems.push('forbidden word line ' + (i + 1)); });

console.log('appended: ' + (appended.join(' ') || 'nothing'));
console.log('rows: ' + rows.length + ', areas: ' + counts.size);
console.log('problems: ' + problems.length);
for (const p of problems.slice(0, 40)) console.log('  ' + p);

// ---- summary, once every area is present ----------------------------------
const order = ['IDN','PLT','SCH','ADM','ACA','ASM','SCD','ATT','FIN','COM','NOT','RQS','DOC','BEH','RPT','AUD','WEL','HR','OPS','AI','GW','BFF','SEC','PRV','PERF','L10N','UX','WEB','MOB','API','INT','DATA','MSG','INF','TST','PLAT'];
const missing = order.filter((a) => !counts.has(a));
console.log('areas still missing: ' + (missing.join(' ') || 'none'));
if (!missing.length && doc.includes('<<SUMMARY>>')) {
  const tot = { 1: 0, 2: 0, 3: 0 };
  let t = '| Area | Tier 1 | Tier 2 | Tier 3 | Total |\n|---|---|---|---|---|\n';
  for (const a of order) {
    const c = counts.get(a);
    tot[1] += c[1]; tot[2] += c[2]; tot[3] += c[3];
    t += '| ' + a + ' | ' + c[1] + ' | ' + c[2] + ' | ' + c[3] + ' | ' + (c[1] + c[2] + c[3]) + ' |\n';
  }
  t += '| **Total** | **' + tot[1] + '** | **' + tot[2] + '** | **' + tot[3] + '** | **' + (tot[1] + tot[2] + tot[3]) + '** |';
  doc = doc.replace('<<SUMMARY>>', t);
  console.log('summary written: ' + (tot[1] + tot[2] + tot[3]) + ' requirements');
}

if (write && problems.length === 0) { writeFileSync(target, doc, 'utf8'); console.log('WRITTEN'); }
else if (write) console.log('NOT WRITTEN: fix problems first');
