#!/usr/bin/env node
// Recompute the capacity and cost figures of docs/plan/28-capacity-and-cost-model.md from
// the inputs the document states, and fail on any printed figure that does not match.
// This is TC-PERF-800. Node 22, no dependencies; runs on Windows and Linux.
//   node tools/plan-build/capacity-28.mjs            check, exit 1 on a mismatch
//   node tools/plan-build/capacity-28.mjs --verbose  also list every figure that matched
//
// Inputs are read from the document itself: the unit prices of part 4.1, the month and
// node packing of part 1, the replica tables of parts 2.3 and 2.4, the storage tables of
// part 2.8, the database volumes of part 2.6, and the drivers written in the Working and
// Formula cells of parts 4.3, 4.4 and 5. Printed figures are read from the same places and
// from the sentences that quote them. Money is compared to the cent, volumes and vCPU to
// one decimal, and figures the document calls "about" to the precision it prints.
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';

const here = dirname(fileURLToPath(import.meta.url));
const docPath = join(here, '..', '..', 'docs', 'plan', '28-capacity-and-cost-model.md');
const doc = readFileSync(docPath, 'utf8').replace(/\r\n/g, '\n');
const verbose = process.argv.includes('--verbose');

// ---- reading helpers ------------------------------------------------------------------
const fail = (msg) => { console.error('capacity-28: cannot read the document: ' + msg); process.exit(2); };
// The text under a heading, up to the next heading of the same or a higher level.
function section(title) {
  const lines = doc.split('\n');
  const i = lines.findIndex((l) => /^#{2,4} /.test(l) && l.includes(title));
  if (i < 0) fail('no heading containing "' + title + '"');
  const level = /^#+/.exec(lines[i])[0].length;
  let j = i + 1;
  while (j < lines.length && !(new RegExp('^#{1,' + level + '} ').test(lines[j]))) j++;
  return lines.slice(i + 1, j).join('\n');
}
// Rows of the first table in `text` whose header row starts with `head`, as arrays of cells.
function table(text, head) {
  const lines = text.split('\n');
  const i = lines.findIndex((l) => l.startsWith('| ' + head));
  if (i < 0) fail('no table headed "' + head + '"');
  const rows = [];
  for (let j = i + 2; j < lines.length && lines[j].startsWith('|'); j++) rows.push(lines[j].split('|').slice(1, -1).map((c) => c.trim()));
  return rows;
}
const row = (rows, label) => {
  const want = label.replace(/\*\*/g, '');
  const r = rows.find((c) => c[0].replace(/\*\*/g, '') === want) || rows.find((c) => c[0].replace(/\*\*/g, '').startsWith(want));
  if (!r) fail('no row "' + label + '"');
  return r;
};
// Numbers in a string, commas removed; `num` is the first one.
const nums = (s) => [...String(s).replace(/(\d),(\d{3})/g, '$1$2').matchAll(/-?\d+(?:\.\d+)?/g)].map((m) => Number(m[0]));
const num = (s) => { const n = nums(s); if (!n.length) fail('no number in "' + s + '"'); return n[0]; };
const grab = (text, re, what) => { const m = re.exec(text); if (!m) fail('cannot find ' + what); return m; };

// ---- comparison ---------------------------------------------------------------------------
// Half away from zero at `d` decimals, with a small allowance for binary floating point.
const round = (x, d) => { const f = 10 ** d; return Math.sign(x) * Math.round(Math.abs(x) * f + 1e-7) / f; };
const results = [];
const check = (what, printed, computed, decimals) => {
  const ok = round(computed, decimals) === round(printed, decimals);
  results.push({ what, printed, computed: round(computed, decimals), ok });
};
// A difference the document carries as an open point: reported, not failed, while the
// open point that explains it is still in the document.
const known = [];
const carried = (what, printed, computed, decimals, openPoint) => {
  if (round(computed, decimals) === round(printed, decimals)) { check(what, printed, computed, decimals); return; }
  const op = new RegExp('^\\| ' + openPoint + '\\. ', 'm').test(doc);
  known.push({ what, printed, computed: round(computed, decimals), openPoint, op });
};

// ---- inputs: part 1 and part 4.1 ------------------------------------------------------------
const p1 = section('1. Inputs and conventions');
const month = row(table(p1, 'Input'), 'Month')[1];
const [hMonth, , hPeakPerDay, hPeak, hOff] = nums(month); // 730 hours; 22 days; 4 h; 88; 642
const schoolDays = nums(month)[1];
check('part 1: peak hours = school days x hours a day', hPeak, schoolDays * hPeakPerDay, 0);
check('part 1: off-peak hours = month - peak', hOff, hMonth - hPeak, 0);
const packing = row(table(p1, 'Input'), 'Node packing')[1];
// "An 8 vCPU / 32 GB node offers about 7.5 vCPU and 28 GB allocatable; ... 75 percent ..., so 5.6 vCPU and 21 GB"
const [, , allocCpu, allocMem, packPct, vcpuPerNode, memPerNode] = nums(packing);
check('part 1: vCPU packed per node', vcpuPerNode, allocCpu * packPct / 100, 1);
check('part 1: memory packed per node', memPerNode, allocMem * packPct / 100, 0);

const prices = table(section('4.1 Illustrative unit prices'), 'Unit');
const price = (label) => num(row(prices, label)[1]);
const pNode = price('General-purpose node');
const pNodeHour = pNode / hMonth;
const pDbLoad = price('Memory-optimised database node, 8 vCPU');
const pDbScale = price('Memory-optimised database node, 32 vCPU');
const pSsd = price('SSD block storage');
const pStd = price('Standard block storage');
const pObj = price('Object storage');
const pEgress = price('Egress');
const pLb = price('Load balancer');
const pCp = price('Kubernetes control plane');
const pStatus = price('Status page and DNS');
const pHostSmall = price('Rented dedicated host, small band');
const pDomain = price('Domain');

// ---- part 2.3 and 2.4: replicas and vCPU --------------------------------------------------------
const s23 = section('2.3 Scale: replicas per service class');
const apis = table(s23, 'Application').map((c) => {
  const count = c[0].includes(',') ? c[0].split(/,| and /).map((x) => x.trim()).filter(Boolean).length : 1;
  const [cpu, mem] = nums(c[2]);
  return { name: c[0], count, cpu, mem, peak: num(c[3]), off: num(c[4]), scale: num(c[6]) };
});
const workers = table(s23, 'Worker deployment').map((c) => ({ name: c[0], cpu: num(c[1]), peak: num(c[2]), off: num(c[3]), scale: num(c[4]) }));
const sum = (xs, f) => xs.reduce((t, x) => t + f(x), 0);
const apiReplicas = (k) => sum(apis, (a) => a.count * a[k]);
const apiCpu = (k) => sum(apis, (a) => a.count * a[k] * a.cpu);
const wReplicas = (k) => sum(workers, (w) => w[k]);
const wCpu = (k) => sum(workers, (w) => w[k] * w.cpu);

// Platform components: vCPU per tier, read as "<n> replicas|nodes ... <v> vCPU".
const comps = table(section('2.4 Scale: platform components'), 'Component');
function compCpu(cell, loadCell) {
  if (/^Same$/.test(cell)) cell = loadCell;
  const c = cell.replace(/(\d),(\d{3})/g, '$1$2');
  let m;
  if ((m = /^(\d+) masters \((\d+(?:\.\d+)?) vCPU\), (\d+) volume servers \((\d+(?:\.\d+)?) vCPU/.exec(c))) return m[1] * m[2] + m[3] * m[4];
  if ((m = /^3 masters, (\d+) volume servers \((\d+(?:\.\d+)?) vCPU/.exec(c))) { const lm = /^(\d+) masters \((\d+(?:\.\d+)?) vCPU\)/.exec(loadCell); return lm[1] * lm[2] + m[1] * m[2]; }
  if ((m = /^Sentinel, 1 primary and 2 replicas.*?(\d+(?:\.\d+)?) vCPU each/.exec(c))) return 3 * m[1];
  if ((m = /^Same topology, (\d+(?:\.\d+)?) vCPU each/.exec(c))) return 3 * m[1];
  if ((m = /^(\d+(?:\.\d+)?) vCPU each/.exec(c))) return 3 * m[1];
  if ((m = /^(\d+) per role, (\d+(?:\.\d+)?) vCPU/.exec(c))) return 2 * m[1] * m[2]; // two roles
  if ((m = /^(\d+) (?:nodes|replicas), (\d+(?:\.\d+)?) vCPU/.exec(c))) return m[1] * m[2];
  if ((m = /^(\d+(?:\.\d+)?) vCPU \/ [\d.]+ GB in total/.exec(c))) return Number(m[1]);
  if ((m = /^(\d+(?:\.\d+)?) vCPU \/ [\d.]+ GB$/.exec(c))) return Number(m[1]);
  if ((m = /^(\d+) replicas(?:;|$)/.exec(c))) { const per = /(\d+(?:\.\d+)?) vCPU/.exec(loadCell); return m[1] * per[1]; }
  fail('cannot read the vCPU of component cell "' + cell + '"');
}
const compLoad = sum(comps, (c) => compCpu(c[1], c[1]));
const compScale = sum(comps, (c) => compCpu(c[2], c[1]));

const s25 = table(section('2.5 Scale: totals and node count'), 'State');
const nodesFor = (cpu, mem) => Math.ceil(Math.max(cpu / vcpuPerNode, mem / memPerNode) - 1e-9);
function state(label, api, wk, cpu, openPoint) {
  const r = row(s25, label);
  const put = openPoint ? (w, p, c, d) => carried(w, p, c, d, openPoint) : check;
  put('part 2.5 ' + label + ': API replicas', num(r[1]), api, 0);
  put('part 2.5 ' + label + ': worker replicas', num(r[2]), wk, 0);
  put('part 2.5 ' + label + ': vCPU requested', num(r[3]), cpu, 1);
  // Memory: part 2.4 does not give every component's memory, so the printed figure is used
  // for the binding constraint and the node count, and the vCPU is recomputed.
  const mem = num(r[4]);
  check('part 2.5 ' + label + ': nodes', num(r[5]), nodesFor(cpu, mem), 0);
  const binding = cpu / vcpuPerNode >= mem / memPerNode ? 'CPU' : 'Memory';
  results.push({ what: 'part 2.5 ' + label + ': binding constraint', printed: r[6], computed: binding, ok: r[6] === binding });
  return nodesFor(cpu, mem);
}
const nLoadPeak = state('Load tier, peak window', apiReplicas('peak'), wReplicas('peak'), apiCpu('peak') + wCpu('peak') + compLoad);
const nLoadOff = state('Load tier, off peak', apiReplicas('off'), wReplicas('off'), apiCpu('off') + wCpu('off') + compLoad);
const nScalePeak = state('Scale tier, peak window', apiReplicas('scale'), wReplicas('scale'), apiCpu('scale') + wCpu('scale') + compScale);
// Part 2.3 gives off-peak minima for the load tier only; open point 12 records that the
// scale-tier off-peak row is recomputed here with the load-tier peak minima.
const nScaleOff = state('Scale tier, off peak', apiReplicas('peak'), wReplicas('off'), apiCpu('peak') + wCpu('off') + compScale, 12);

// ---- part 2.8: storage per student ----------------------------------------------------------------
const s28 = section('2.8 Storage per student per year');
const remaining = num(grab(s28, /uses \*\*(\d+) years\*\* of average remaining enrolment/, 'the average remaining enrolment')[1]);
const multiplier = (retention) => {
  const r = retention.replace(/\(.*?\)/g, '');
  let m;
  if ((m = /(\d+) years after leaving/.exec(r)) || (m = /leaving plus (\d+) years/.exec(r))) return remaining + Number(m[1]);
  if ((m = /(\d+) days/.exec(r))) return Number(m[1]) / 360;
  if ((m = /(\d+) years/.exec(r))) return Number(m[1]);
  if (/Life of the record/.test(r)) return 1;
  fail('cannot read the retention "' + retention + '"');
};
function storage(head, label) {
  const rows = table(s28.slice(s28.indexOf(label)), head);
  // Year one holds each class's annual volume, except a class kept for less than a year,
  // which holds only its retained share (the notification log: 1.00 MB x 0.25).
  let written = 0, steady = 0;
  for (const r of rows) {
    if (/^\*\*Total/.test(r[0])) {
      check('part 2.8 ' + label.replace(/\*\*/g, '') + ' total held in year one (MB)', num(r[1]), written, 2);
      check('part 2.8 ' + label.replace(/\*\*/g, '') + ' total at steady state (MB)', num(r[4]), steady, 2);
      return { year1: num(r[1]), steady: num(r[4]) };
    }
    const w = num(r[1]);
    const mult = multiplier(r[2]);
    check('part 2.8 ' + label.replace(/\*\*/g, '') + ' ' + r[0] + ': multiplier', num(r[3]), mult, 2);
    check('part 2.8 ' + label.replace(/\*\*/g, '') + ' ' + r[0] + ': steady state', num(r[4]), w * mult, 2);
    written += w * Math.min(1, mult); steady += w * mult;
  }
  fail('no total row in ' + label);
}
const db = storage('Data class', '**Database, per student.**');
const obj = storage('Content', '**Object storage, per student.**');
const media = grab(s28, /\*\*Coursework and portfolio media are (\d+) percent/, 'the media share')[1];
const objRows = table(s28.slice(s28.indexOf('**Object storage, per student.**')), 'Content');
const mediaMb = num(row(objRows, 'Coursework submissions')[4]) + num(row(objRows, 'Portfolio')[4]);
check('part 2.8: coursework and portfolio share of object storage (percent)', Number(media), mediaMb / obj.steady * 100, 0);

const GB = 1000; // MB per GB, the convention of the worked examples
const s26 = table(section('2.6 Scale: database'), 'Element');
check('part 2.6: load-tier data used at steady state (GB)', num(/\((\d+) GB used\)/.exec(row(s26, 'Data volume per instance, steady state')[1])[1]), 50000 * db.steady / GB, 0);
check('part 2.6: scale-tier data used at steady state (TB)', num(/\(([\d.]+) TB used\)/.exec(row(s26, 'Data volume per instance, steady state')[2])[1]), 500000 * db.steady / GB / 1000, 1);
const volTb = (cell) => { const m = /([\d,.]+) (GB|TB) SSD/.exec(cell); return num(m[1]) * (m[2] === 'TB' ? 1000 : 1); };
const volLoad1 = volTb(row(s26, 'Data volume per instance, year one')[1]);
const volLoadS = volTb(row(s26, 'Data volume per instance, steady state')[1]);
const volScaleS = volTb(row(s26, 'Data volume per instance, steady state')[2]);

// ---- part 4.2 and the egress driver -----------------------------------------------------------------
const s42 = section('4.2 The formula per mode');
const egressMb = num(grab(s42, /uses `e = (\d+) MB per student-month`/, 'the egress driver')[1]);
const backups = (S, sDb, sObj) => (4.5 * S * sDb / GB + 1.15 * S * sObj / GB) * pObj;
const lines = (text) => table(text, 'Line');
const money = (cell) => num(cell.replace(/\*\*/g, ''));

// ---- part 4.3: worked example A ---------------------------------------------------------------------------
const s43 = section('4.3 Worked example A');
const SA = num(grab(s43, /^\| Egress \| (\d+) ×/m, 'example A students')[1]);
const A = lines(s43);
const aLines = { compute: [pHostSmall, pHostSmall], database: [0, 0], storage: [0, 0], egress: [SA * egressMb / GB * pEgress, SA * egressMb / GB * pEgress], observability: [0, 0], backups: [backups(SA, db.year1, obj.year1), backups(SA, db.steady, obj.steady)], domain: [pDomain, pDomain] };
const labelsA = { compute: 'Compute', database: 'Database', storage: 'Storage', egress: 'Egress', observability: 'Observability', backups: 'Backups', domain: 'Domain and certificate' };
const totalA = [0, 0];
for (const [k, label] of Object.entries(labelsA)) for (const i of [0, 1]) {
  const printed = money(row(A, label)[2 + i]);
  check('part 4.3 example A ' + label + (i ? ', steady state' : ', year one'), printed, aLines[k][i], 2);
  totalA[i] += round(aLines[k][i], 2);
}
check('part 4.3 example A: database in year one (GB)', num(grab(s43, /database ([\d.]+) GB in year one/, 'A database year one')[1]), SA * db.year1 / GB, 1);
check('part 4.3 example A: database at steady state (GB)', num(grab(s43, /and ([\d.]+) GB at steady state/, 'A database steady')[1]), SA * db.steady / GB, 1);
for (const i of [0, 1]) {
  const tag = i ? ', steady state' : ', year one';
  check('part 4.3 example A total' + tag, money(row(A, '**Total**')[2 + i]), totalA[i], 2);
  check('part 4.3 example A per 1,000 students' + tag, money(row(A, '**Per 1,000 students**')[2 + i]), totalA[i] / SA * 1000, 2);
  check('part 4.3 example A per student per year' + tag, money(row(A, 'Per student per year')[2 + i]), totalA[i] * 12 / SA, 2);
}
const variant = grab(s43, /USD ([\d,]+) over (\d+) months plus USD (\d+) a month of power, compute is USD ([\d.]+) and the steady-state total per 1,000 students is USD ([\d.]+)/, 'the owned-host variant');
const vCompute = num(variant[1]) / num(variant[2]) + num(variant[3]);
check('part 4.3 owned-host variant: compute', num(variant[4]), vCompute, 2);
check('part 4.3 owned-host variant: per 1,000 students, steady state', num(variant[5]), (totalA[1] - round(aLines.compute[1], 2) + round(vCompute, 2)) / SA * 1000, 2);

// ---- part 4.4: worked example B -----------------------------------------------------------------------------
const s44 = section('4.4 Worked example B');
const B = lines(s44);
const SB = num(grab(s44, /^\| Egress \| ([\d,]+) ×/m, 'example B students')[1]);
const obsB = grab(row(B, 'Observability')[1], /(\d+) nodes × \d+ \+ ([\d,]+) GB/, 'example B observability');
const computeB = (nLoadPeak * hPeak + nLoadOff * hOff) * pNodeHour;
const bLines = {
  'Compute': [computeB, computeB],
  'Database, instances': [3 * pDbLoad, 3 * pDbLoad],
  'Database, volumes': [3 * volLoad1 * pSsd, 3 * volLoadS * pSsd],
  'Storage': [SB * obj.year1 / GB * 3 * pStd, SB * obj.steady / GB * 3 * pStd],
  'Egress': [SB * egressMb / GB * pEgress, SB * egressMb / GB * pEgress],
  'Observability': [num(obsB[1]) * pNode + num(obsB[2]) * pSsd, num(obsB[1]) * pNode + num(obsB[2]) * pSsd],
  'Backups': [backups(SB, db.year1, obj.year1), backups(SB, db.steady, obj.steady)],
  'Network': [2 * pLb + pCp + pStatus, 2 * pLb + pCp + pStatus],
  'Cold standby': [pCp, pCp],
};
const totalB = [0, 0];
for (const [label, v] of Object.entries(bLines)) for (const i of [0, 1]) {
  check('part 4.4 example B ' + label + (i ? ', steady state' : ', year one'), money(row(B, label)[2 + i]), v[i], 2);
  totalB[i] += round(v[i], 2);
}
const bk = grab(row(B, 'Backups')[1], /= ([\d,.]+) GB; steady .*= ([\d,.]+) GB/, 'example B backup volumes');
check('part 4.4 example B backup volume, year one (GB)', num(bk[1]), 4.5 * SB * db.year1 / GB + 1.15 * SB * obj.year1 / GB, 1);
check('part 4.4 example B backup volume, steady state (GB)', num(bk[2]), 4.5 * SB * db.steady / GB + 1.15 * SB * obj.steady / GB, 0);
for (const i of [0, 1]) {
  const tag = i ? ', steady state' : ', year one';
  check('part 4.4 example B total' + tag, money(row(B, '**Total**')[2 + i]), totalB[i], 2);
  check('part 4.4 example B per 1,000 students' + tag, money(row(B, '**Per 1,000 students**')[2 + i]), totalB[i] / SB * 1000, 2);
  check('part 4.4 example B per student per year' + tag, money(row(B, 'Per student per year')[2 + i]), totalB[i] * 12 / SB, 2);
}
const allDayB = grab(s44, /compute would be (\d+) × \d+ h × \d+ ÷ \d+ = USD ([\d,]+), so calendar-aware scaling saves USD (\d+)/, 'example B all-day compute');
check('part 4.4 all-day compute nodes are the peak nodes', num(allDayB[1]), nLoadPeak, 0);
check('part 4.4 all-day compute', num(allDayB[2]), nLoadPeak * hMonth * pNodeHour, 2);
check('part 4.4 calendar-aware saving', num(allDayB[3]), nLoadPeak * hMonth * pNodeHour - computeB, 0);

// ---- part 4.5: comparison and the scale-tier indication ----------------------------------------------------------
const s45 = section('4.5 Comparison');
const C = table(s45, 'Deployment');
const cA = row(C, 'A:'), cB = row(C, 'B:'), cS = row(C, 'Scale-tier region');
check('part 4.5 A per 1,000, year one', num(cA[1]), totalA[0] / SA * 1000, 2);
check('part 4.5 A per 1,000, steady state', num(cA[2]), totalA[1] / SA * 1000, 2);
check('part 4.5 A: the host share (percent)', num(/\((\d+)%\)/.exec(cA[3])[1]), round(aLines.compute[1], 2) / totalA[1] * 100, 0);
check('part 4.5 B per 1,000, year one', num(cB[1]), totalB[0] / SB * 1000, 2);
check('part 4.5 B per 1,000, steady state', num(cB[2]), totalB[1] / SB * 1000, 2);
const shares = nums(cB[3]);
check('part 4.5 B: compute share (percent)', shares[0], round(computeB, 2) / totalB[1] * 100, 0);
check('part 4.5 B: database share (percent)', shares[1], (3 * pDbLoad + 3 * volLoadS * pSsd) / totalB[1] * 100, 0);
check('part 4.5 B: storage share (percent)', shares[2], round(bLines.Storage[1], 2) / totalB[1] * 100, 0);

const SS = num(grab(cS[0], /([\d,]+) students/, 'scale-tier students')[1]);
const ind = grab(s45, /compute USD ([\d,]+) \((\d+) and (\d+) nodes\), database USD ([\d,]+) plus USD ([\d,]+) of (\d+) TB volumes, backups USD ([\d,]+), egress USD ([\d,]+), observability USD ([\d,]+) \((\w+) nodes and (\d+) TB\), network and standby USD (\d+)/, 'the scale-tier indication');
const words = { two: 2, three: 3, four: 4, five: 5, six: 6 };
check('part 4.5 indication: peak nodes are part 2.5\'s', num(ind[2]), nScalePeak, 0);
check('part 4.5 indication: off-peak nodes are part 2.5\'s', num(ind[3]), nScaleOff, 0);
const sLines = {
  compute: (nScalePeak * hPeak + nScaleOff * hOff) * pNodeHour,
  dbInstances: 3 * pDbScale,
  dbVolumes: 3 * num(ind[6]) * 1000 * pSsd,
  storage: SS * obj.steady / GB * 3 * pStd,
  backups: backups(SS, db.steady, obj.steady),
  egress: SS * egressMb / GB * pEgress,
  observability: words[ind[10]] * pNode + num(ind[11]) * 1000 * pSsd,
  networkStandby: 2 * pLb + pCp + pStatus + pCp,
};
check('part 4.5 indication: database volumes are part 2.6\'s', num(ind[6]) * 1000, volScaleS, 0);
check('part 4.5 indication: compute (about)', num(ind[1]), sLines.compute, 0);
check('part 4.5 indication: database instances', num(ind[4]), sLines.dbInstances, 0);
check('part 4.5 indication: database volumes', num(ind[5]), sLines.dbVolumes, 0);
check('part 4.5 indication: backups (about)', num(ind[7]), sLines.backups, 0);
check('part 4.5 indication: egress', num(ind[8]), sLines.egress, 0);
check('part 4.5 indication: observability', num(ind[9]), sLines.observability, 0);
check('part 4.5 indication: network and standby', num(ind[12]), sLines.networkStandby, 0);
const sTotal = Object.values(sLines).reduce((t, x) => t + x, 0);
const storageCell = grab(cS[3], /Storage \((\d+)%\): ([\d,]+) GB × 3 × [\d.]+ = USD ([\d,]+) of about USD ([\d,]+)/, 'the scale-tier storage cell');
check('part 4.5 indication: object storage volume (GB)', num(storageCell[2]), SS * obj.steady / GB, 0);
check('part 4.5 indication: storage line', num(storageCell[3]), sLines.storage, 0);
check('part 4.5 indication: total (about, to USD 10)', num(storageCell[4]), sTotal, -1);
check('part 4.5 indication: storage share (percent)', num(storageCell[1]), sLines.storage / sTotal * 100, 0);
check('part 4.5 indication: per 1,000 students (about)', num(/about USD ([\d.]+)/.exec(cS[2])[1]), sTotal / SS * 1000, 2);
const allDayS = grab(s45, /Against USD ([\d,]+) had peak minimums run all day, calendar-aware scaling saves USD ([\d,]+) a month at the scale tier, (\d+) percent of compute/, 'the scale-tier saving');
check('part 4.5 indication: all-day compute', num(allDayS[1]), nScalePeak * hMonth * pNodeHour, 0);
check('part 4.5 indication: saving (to USD 100)', num(allDayS[2]), nScalePeak * hMonth * pNodeHour - sLines.compute, -2);
check('part 4.5 indication: saving as a share of all-day compute (percent)', num(allDayS[3]), (nScalePeak * hMonth * pNodeHour - sLines.compute) / (nScalePeak * hMonth * pNodeHour) * 100, 0);
const s25text = section('2.5 Scale: totals and node count');
const cmp25 = grab(s25text, /difference between USD ([\d,]+) and USD ([\d,]+) a month in compute/, 'the part 2.5 compute comparison');
check('part 2.5: scale-tier compute with the peak window (to USD 10)', num(cmp25[1]), sLines.compute, -1);
check('part 2.5: scale-tier compute with peak minimums all day', num(cmp25[2]), nScalePeak * hMonth * pNodeHour, 0);
const peakAdd = grab(s25text, /the peak adds (\w+) nodes for (\d+) hours a month instead of (\d+)/, 'the part 2.5 peak nodes');
check('part 2.5: nodes the scale-tier peak adds', { nine: 9, eight: 8, ten: 10 }[peakAdd[1]] ?? num(peakAdd[1]), nScalePeak - nScaleOff, 0);

// ---- part 5: costs that are not infrastructure ---------------------------------------------------------------------
const s5 = section('5. Costs that are not infrastructure');
const P = table(s5, 'Line (Section 30)');
const sms = grab(row(P, 'SMS and WhatsApp')[2], /S × ([\d.]+) × (\d+)% × (\d+) × p_sms/, 'the SMS formula');
const pSms = num(row(P, 'SMS and WhatsApp')[3]);
const smsPer = (S) => S * num(sms[1]) * num(sms[2]) / 100 * num(sms[3]) * pSms;
check('part 5 SMS, example A', num(row(P, 'SMS and WhatsApp')[4]), smsPer(SA), 2);
check('part 5 SMS, example B', num(row(P, 'SMS and WhatsApp')[5]), smsPer(SB), 2);
check('part 5 SMS per 1,000 students', num(/USD (\d+) per 1,000/.exec(row(P, 'SMS and WhatsApp')[6])[1]), smsPer(1000), 2);
const em = grab(row(P, 'Email at scale')[2], /S × ([\d.]+) × (\d+) × p_email/, 'the email formula');
const pEmail = num(row(P, 'Email at scale')[3]) / 1000;
const emPer = (S) => S * num(em[1]) * num(em[2]) * pEmail;
check('part 5 email, example A', num(row(P, 'Email at scale')[4]), emPer(SA), 2);
check('part 5 email, example B', num(row(P, 'Email at scale')[5]), emPer(SB), 2);
check('part 5 email per 1,000 students', num(/USD (\d+) per 1,000/.exec(row(P, 'Email at scale')[6])[1]), emPer(1000), 2);
check('part 5 app stores: Apple fee a month', num(/[\d.]+/.exec(row(P, 'App stores')[5])[0]), num(row(P, 'App stores')[3]) / 12, 2);
const apple = grab(row(P, 'Apple build capacity')[5], /(\d+) builds × (\d+) min × ([\d.]+) = (\d+)/, 'the Apple build line');
check('part 5 Apple build capacity per flavour', num(apple[4]), num(apple[1]) * num(apple[2]) * num(apple[3]), 2);
const ai = row(P, 'AI hardware');
const aiA = grab(ai[3], /16 GB GPU ([\d,]+) once over (\d+) months; 48 GB GPU node ([\d,]+) per month/, 'the AI unit prices');
check('part 5 AI hardware, example A', num(ai[4]), num(aiA[1]) / num(aiA[2]), 2);
const aiB = grab(ai[5], /([\d,]+) × (\d+) h ÷ (\d+) = ([\d.]+)/, 'the AI scale-mode working');
check('part 5 AI hardware, example B', num(aiB[4]), num(aiA[3]) * num(aiB[2]) / hMonth, 2);
const pen = row(P, 'Penetration test');
check('part 5 penetration test a month', num(pen[5]), num(pen[3]) / 12, 2);
const ppl = row(P, 'People');
const fte = grab(ppl[5], /([\d.]+) to ([\d.]+) FTE: ([\d,]+) to ([\d,]+)/, 'the people line');
check('part 5 people, low', num(fte[3]), num(fte[1]) * num(ppl[3]), 2);
check('part 5 people, high', num(fte[4]), num(fte[2]) * num(ppl[3]), 2);
const para = grab(s5, /\(SMS USD (\d+), email USD (\d+), and rung 3 at USD ([\d.]+) in scale mode\) add about USD (\d+) to USD (\d+)/, 'the variable-lines sentence');
check('part 5 rung 3 per 1,000 students in scale mode', num(para[3]), num(aiB[4]) / SB * 1000, 2);
check('part 5 variable lines, low', num(para[4]), smsPer(1000) + emPer(1000), 0);
check('part 5 variable lines, high', num(para[5]), smsPer(1000) + emPer(1000) + num(aiB[4]) / SB * 1000, 0);

// ---- report ---------------------------------------------------------------------------------------------------------------
const bad = results.filter((r) => !r.ok);
if (verbose) for (const r of results.filter((x) => x.ok)) console.log('  ok    ' + r.what + ': ' + r.printed);
for (const r of bad) console.log('  FAIL  ' + r.what + ': printed ' + r.printed + ', recomputed ' + r.computed);
for (const k of known) console.log('  ' + (k.op ? 'known ' : 'FAIL  ') + k.what + ': printed ' + k.printed + ', recomputed ' + k.computed + (k.op ? ' (carried as open point ' + k.openPoint + ')' : ' (open point ' + k.openPoint + ' that carried it is gone)'));
const knownBad = known.filter((k) => !k.op);
console.log('capacity-28: ' + results.length + ' figures checked, ' + bad.length + ' mismatched, ' + known.length + ' carried as open points' + (knownBad.length ? ', ' + knownBad.length + ' no longer carried' : ''));
process.exit(bad.length || knownBad.length ? 1 : 0);
