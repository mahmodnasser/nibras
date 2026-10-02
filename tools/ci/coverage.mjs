// Coverage gate of document 16 part 15. Every test project writes a Cobertura report through coverlet;
// one building block is exercised by several test projects, so the reports are merged line by line before
// any threshold is applied. A line counts as covered when any report hit it; a branch point keeps the best
// covered-of-total any report saw.

import { readFileSync } from 'node:fs';
import path from 'node:path';

/** The thresholds, by layer. A layer with no threshold is reported, not gated (document 16 part 15). */
export const THRESHOLDS = [
  { layer: 'domain', test: (a) => /^Nibras\.[A-Za-z0-9]+\.Domain$/.test(a) && !a.startsWith('Nibras.BuildingBlocks.'), line: 90, branch: 85 },
  { layer: 'application', test: (a) => /^Nibras\.[A-Za-z0-9]+\.Application$/.test(a), line: 80, branch: null },
  { layer: 'building-block', test: (a) => a.startsWith('Nibras.BuildingBlocks.') && !a.startsWith('Nibras.BuildingBlocks.Testing'), line: 90, branch: null },
];

const attr = (tag, name) => {
  const m = new RegExp(`\\s${name}="([^"]*)"`).exec(tag);
  return m ? m[1] : undefined;
};

/** Parses one Cobertura document into Map<assembly, Map<"file:line", {hits, covered, total}>>. */
export function parseCobertura(xml) {
  const result = new Map();
  // Each report names its files relative to its own <source> roots; resolve them so that two reports agree on a file.
  const sources = [...xml.matchAll(/<source>([^<]*)<\/source>/g)].map((m) => m[1].replaceAll('\\', '/'));
  const resolve = (file) => {
    const normal = file.replaceAll('\\', '/');
    if (path.posix.isAbsolute(normal) || /^[A-Za-z]:\//.test(normal) || sources.length === 0) return normal;
    return path.posix.join(sources[0], normal);
  };
  const packageRe = /<package\b[^>]*>([\s\S]*?)<\/package>/g;
  for (const pkg of xml.matchAll(packageRe)) {
    const open = pkg[0].slice(0, pkg[0].indexOf('>') + 1);
    const assembly = attr(open, 'name');
    const lines = result.get(assembly) ?? new Map();
    for (const cls of pkg[1].matchAll(/<class\b[^>]*>[\s\S]*?<\/class>/g)) {
      const clsOpen = cls[0].slice(0, cls[0].indexOf('>') + 1);
      const file = resolve(attr(clsOpen, 'filename') ?? '');
      // Method-level <lines> repeat the class-level ones; reading every <line> and keying by file and number dedupes them.
      for (const line of cls[0].matchAll(/<line\b[^>]*\/?>/g)) {
        const number = attr(line[0], 'number');
        const hits = Number(attr(line[0], 'hits') ?? 0);
        const condition = /\((\d+)\/(\d+)\)/.exec(attr(line[0], 'condition-coverage') ?? '');
        const key = `${file}:${number}`;
        const prev = lines.get(key);
        const covered = condition ? Number(condition[1]) : 0;
        const total = condition ? Number(condition[2]) : 0;
        lines.set(key, {
          hits: Math.max(prev?.hits ?? 0, hits),
          covered: Math.max(prev?.covered ?? 0, covered),
          total: Math.max(prev?.total ?? 0, total),
        });
      }
    }
    result.set(assembly, lines);
  }
  return result;
}

/** Merges parsed reports: the union of hits per line, the best branch coverage per line. */
export function merge(reports) {
  const merged = new Map();
  for (const report of reports) {
    for (const [assembly, lines] of report) {
      const target = merged.get(assembly) ?? new Map();
      for (const [key, value] of lines) {
        const prev = target.get(key);
        target.set(key, prev
          ? { hits: Math.max(prev.hits, value.hits), covered: Math.max(prev.covered, value.covered), total: Math.max(prev.total, value.total) }
          : { ...value });
      }
      merged.set(assembly, target);
    }
  }
  return merged;
}

const percent = (part, whole) => (whole === 0 ? 100 : Math.floor((part / whole) * 1000) / 10);

/**
 * One row per assembly: its layer, line and branch percentages (rounded down to one decimal, so 89.99 is
 * 89.9 and fails 90), the thresholds that apply, and whether it passes. Assemblies are sorted by name.
 */
export function evaluate(merged, thresholds = THRESHOLDS) {
  const rows = [];
  for (const [assembly, lines] of [...merged].sort(([a], [b]) => (a < b ? -1 : a > b ? 1 : 0))) {
    let hit = 0;
    let covered = 0;
    let total = 0;
    for (const value of lines.values()) {
      if (value.hits > 0) hit++;
      covered += value.covered;
      total += value.total;
    }
    const rule = thresholds.find((t) => t.test(assembly));
    const line = percent(hit, lines.size);
    const branch = percent(covered, total);
    const failures = [];
    if (rule && line < rule.line) failures.push(`line ${line}% is under ${rule.line}%`);
    if (rule && rule.branch !== null && branch < rule.branch) failures.push(`branch ${branch}% is under ${rule.branch}%`);
    rows.push({
      assembly,
      layer: rule?.layer ?? 'reported',
      lines: lines.size,
      line,
      branch,
      lineThreshold: rule?.line ?? null,
      branchThreshold: rule?.branch ?? null,
      passed: failures.length === 0,
      failures,
    });
  }
  return rows;
}

export function fromFiles(paths, thresholds = THRESHOLDS) {
  return evaluate(merge(paths.map((p) => parseCobertura(readFileSync(p, 'utf8')))), thresholds);
}
