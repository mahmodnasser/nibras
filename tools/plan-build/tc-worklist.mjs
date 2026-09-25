// Turn kit-lint R20's findings into a work list per document, for resolving
// test-case identifier collisions and giving every cited test a definition.
//   node tools/plan-build/tc-worklist.mjs            summary
//   node tools/plan-build/tc-worklist.mjs --write    also writes parts/tc-work-<doc>.md
// The owner of a test is chosen by precedence: Appendix R, Appendix W, the
// service sheet of the identifier's area, the cross-cutting plan document of
// the area, then any other document.
import { writeFileSync, mkdirSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';
import { buildContext, testCaseOwnership } from '../kit-lint/kit-lint.mjs';

const here = dirname(fileURLToPath(import.meta.url));
const root = join(here, '..', '..');
const ctx = buildContext(root);
const own = testCaseOwnership(ctx);

const SHEET = { IDN: 'identity', PLT: 'platform', SCH: 'school', ADM: 'admissions', ACA: 'academics', ASM: 'assessment', SCD: 'scheduling', ATT: 'attendance', FIN: 'finance', COM: 'communication', NOT: 'notification', RQS: 'requests', DOC: 'documents', BEH: 'behavior', RPT: 'reporting', AUD: 'audit', WEL: 'wellbeing', HR: 'hr', OPS: 'operations', AI: 'ai', GW: 'gateway' };
const CROSS = { SEC: '12-security-privacy-safety', PRV: '12-security-privacy-safety', PERF: '21-performance-engineering', API: '22-api-conventions-and-error-catalog', INT: '23-integrations-and-public-api', L10N: '24-localization-and-calendars', MOB: '09-mobile-structure', WEB: '08-web-structure', UX: '14-design-system-and-ux', DATA: '10-data-architecture', MSG: '11-messaging-architecture', INF: '15-deployment-and-operations', TST: '16-test-strategy', PLAT: '33-platform-support-and-dev-environments', BFF: '06-services/bff-web' };
export const naturalOwner = (id) => {
  const area = id.split('-')[1];
  if (SHEET[area]) return 'docs/plan/06-services/' + SHEET[area] + '.md';
  if (CROSS[area]) return 'docs/plan/' + CROSS[area] + '.md';
  return 'docs/plan/16-test-strategy.md';
};
const rank = (id, file) => {
  if (/appendix-r-/.test(file)) return 0;
  if (/appendix-w-/.test(file)) return 1;
  if (file === naturalOwner(id)) return 2;
  if (/^docs\/brief\//.test(file)) return 3;
  return 4;
};

const work = new Map(); // file -> { resolve: [], define: [] }
const bucket = (f) => { if (!work.has(f)) work.set(f, { resolve: [], define: [] }); return work.get(f); };
for (const c of own.collisions) {
  const files = [...new Set(c.defs.map((d) => d.file))].sort((a, b) => rank(c.id, a) - rank(c.id, b));
  const owner = files[0];
  const ownerDef = c.defs.find((d) => d.file === owner);
  for (const f of files.slice(1)) {
    for (const d of c.defs.filter((x) => x.file === f)) bucket(f).resolve.push({ id: c.id, line: d.line, here: d.text, owner, ownerLine: ownerDef.line, ownerText: ownerDef.text });
  }
}
for (const id of own.undefinedIds) {
  const live = own.cites.get(id).filter((c) => !c.exempt && !/^docs\/(project\/|plan\/30-)/.test(c.file));
  if (!live.length) continue;
  bucket(naturalOwner(id)).define.push({ id, citedBy: live.map((c) => ({ file: c.file, line: c.line, text: c.text })) });
}

const brief = [...work.keys()].filter((f) => f.startsWith('docs/brief/'));
console.log('documents with work: ' + work.size + (brief.length ? ' | brief documents needing change: ' + brief.join(', ') : ''));
for (const [f, w] of [...work].sort((a, b) => (b[1].resolve.length + b[1].define.length) - (a[1].resolve.length + a[1].define.length))) console.log(String(w.resolve.length).padStart(4) + ' resolve ' + String(w.define.length).padStart(4) + ' define  ' + f);

if (process.argv.includes('--write')) {
  mkdirSync(join(here, 'parts', 'tc-work'), { recursive: true });
  for (const [f, w] of work) {
    const name = f.replace(/^docs\//, '').replace(/[\/]/g, '__');
    const L = ['# Test-case work for `' + f + '`', ''];
    if (w.resolve.length) {
      L.push('## Collisions to resolve here (the owner keeps the identifier)', '');
      for (const r of w.resolve) L.push('- **' + r.id + '** at line ' + r.line + ': "' + r.here.slice(0, 220) + '"', '  - owner `' + r.owner + '` line ' + r.ownerLine + ': "' + r.ownerText.slice(0, 220) + '"');
      L.push('');
    }
    if (w.define.length) {
      L.push('## Tests cited but defined nowhere: define them here', '');
      for (const d of w.define) { L.push('- **' + d.id + '**'); for (const c of d.citedBy.slice(0, 4)) L.push('  - cited `' + c.file + '` line ' + c.line + ': "' + c.text.slice(0, 200) + '"'); }
      L.push('');
    }
    writeFileSync(join(here, 'parts', 'tc-work', name), L.join('\n'), 'utf8');
  }
  console.log('written to tools/plan-build/parts/tc-work/');
}
