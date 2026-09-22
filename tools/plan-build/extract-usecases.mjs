// Extract each service's use cases, consumers and jobs from the file trees in
// its sheet: the direct children of Application/Features/, Consumers/ and Jobs/.
// The first Features/ block in a sheet is the production one; test trees repeat
// the name later and are skipped.
import { fileURLToPath as __toPath } from 'node:url';
import { dirname as __dirOf, resolve as __resolve } from 'node:path';
// Paths resolve from this file, so the script runs from any checkout on Windows or Linux.
const __here = __dirOf(__toPath(import.meta.url)).split(String.fromCharCode(92)).join('/');
const __kit = __resolve(__here, '../..').split(String.fromCharCode(92)).join('/') + '/';
import { readFileSync, readdirSync, writeFileSync } from 'node:fs';

const DIR = __kit + 'docs/plan/06-services/';
const OUT = __here + '/parts/usecases.md';

/** Column of the first letter of a tree entry's name, or -1 if not a tree line. */
const nameCol = (line) => {
  const m = /[├└]──\s/.exec(line);
  return m ? m.index + m[0].length : -1;
};
const entry = (line) => {
  const c = nameCol(line);
  if (c < 0) return null;
  const rest = line.slice(c);
  const m = /^(\S+)\s*(.*)$/.exec(rest);
  return m ? { name: m[1], comment: m[2].trim(), col: c } : null;
};

function children(lines, folderRe, isProduction) {
  const out = [];
  for (let i = 0; i < lines.length; i++) {
    const e = entry(lines[i]);
    if (!e || !folderRe.test(e.name)) continue;
    if (!isProduction(lines, i)) continue;
    // Children sit exactly one level deeper (4 columns in these trees).
    const childCol = e.col + 4;
    for (let j = i + 1; j < lines.length; j++) {
      const c = entry(lines[j]);
      if (!c) { if (!/^\s*[│ ]*$/.test(lines[j])) break; else continue; }
      if (c.col <= e.col) break;
      if (c.col === childCol && /\/$/.test(c.name)) out.push({ name: c.name.replace(/\/$/, ''), comment: c.comment });
      if (c.col === childCol && /\.cs$/.test(c.name)) out.push({ name: c.name.replace(/\.cs$/, ''), comment: c.comment });
    }
    return out; // first matching block only
  }
  return out;
}

// A block belongs to the production tree when the nearest preceding project
// line does not name a test project.
const production = (lines, i) => {
  for (let k = i; k >= 0 && k > i - 400; k--) {
    if (/Tests?\/|\.Tests\b|tests\//.test(lines[k]) && /[├└]──/.test(lines[k]) && entry(lines[k]) && entry(lines[k]).col < entry(lines[i]).col) return false;
    if (/\.Application\/|Application\//.test(lines[k]) && entry(lines[k]) && entry(lines[k]).col < entry(lines[i]).col) return true;
  }
  return true;
};

const md = [];
const totals = [];
for (const file of readdirSync(DIR).filter((f) => f.endsWith('.md')).sort()) {
  const lines = readFileSync(DIR + file, 'utf8').split(/\r?\n/);
  const svc = file.replace(/\.md$/, '');
  const feats = children(lines, /^Features\/$/, production);
  const cons = children(lines, /^Consumers\/$/, production);
  const jobs = children(lines, /^Jobs\/$/, () => true);
  totals.push([svc, feats.length, cons.length, jobs.length]);
  md.push('## ' + svc);
  md.push('');
  md.push('Use cases (Application/Features/): ' + feats.length);
  for (const f of feats) md.push('- F ' + f.name + ': ' + f.comment.slice(0, 150));
  md.push('Consumers: ' + cons.length);
  for (const f of cons) md.push('- C ' + f.name + ': ' + f.comment.slice(0, 110));
  md.push('Jobs: ' + jobs.length);
  for (const f of jobs) md.push('- J ' + f.name + ': ' + f.comment.slice(0, 110));
  md.push('');
}
writeFileSync(OUT, md.join('\n'), 'utf8');
console.log('service'.padEnd(16) + 'feat  cons  jobs');
let tf = 0, tc = 0, tj = 0;
for (const [s, f, c, j] of totals) { tf += f; tc += c; tj += j; console.log(s.padEnd(16) + String(f).padStart(4) + String(c).padStart(6) + String(j).padStart(6)); }
console.log('TOTAL'.padEnd(16) + String(tf).padStart(4) + String(tc).padStart(6) + String(tj).padStart(6));
