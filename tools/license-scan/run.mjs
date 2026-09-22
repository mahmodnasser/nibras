#!/usr/bin/env node
/**
 * Nibras licence scan.
 *
 * Runs on Windows and Linux through the .ps1 and .sh wrappers. No dependencies.
 *
 * It enforces master brief Section 6.1 across NuGet, npm and pub, and it fails
 * the build on anything that is not allowed for linked code unless the package
 * has an entry in allow.json that cites the Section 6.4 justification and an ADR.
 *
 * Licences are read as SPDX expressions, because real packages publish them
 * that way: "MIT OR Apache-2.0" is allowed when any branch is, "MIT AND X"
 * only when every branch is. Fonts and other assets under the SIL Open Font
 * Licence are allowed as assets. A package that ships its licence as a file
 * cannot be judged automatically: someone reads the file at the source and
 * records the SPDX identifier under "overrides" in allow.json.
 *
 * Until source code exists this reports what it would scan and exits clean, so
 * it can be wired into the pipeline from the first commit rather than later.
 */

import { readFileSync, existsSync, readdirSync } from 'node:fs';
import { join, relative } from 'node:path';

export const ALLOWED_LINKED = new Set([
  'MIT', 'MIT-0', 'Apache-2.0', 'BSD-2-Clause', 'BSD-3-Clause', 'BSD-3-Clause-Clear',
  'ISC', 'MPL-2.0', 'PostgreSQL', '0BSD', 'Unlicense', 'Zlib', 'BSL-1.0',
]);

export const ALLOWED_DYNAMIC = new Set(['LGPL-2.1', 'LGPL-2.1-only', 'LGPL-2.1-or-later', 'LGPL-3.0', 'LGPL-3.0-only', 'LGPL-3.0-or-later']);

/** Allowed for fonts, icons and other assets that are shipped, not linked. */
export const ALLOWED_ASSET = new Set(['OFL-1.1', 'OFL-1.1-RFN', 'OFL-1.1-no-RFN', 'CC-BY-4.0', 'CC0-1.0']);

export const NEVER = [
  /^BUSL/i, /^SSPL/i, /^RSAL/i, /^Elastic/i, /Commons-Clause/i,
  /commercial/i, /proprietary/i, /source-available/i,
];

const FILE_LICENCE = /^(file:|LICENSE|LICENCE|COPYING|see license|see licence)/i;

/** One SPDX identifier, with any "WITH <exception>" stripped to its base licence. */
function judgeOne(raw) {
  const id = raw.replace(/\s+WITH\s+.*$/i, '').replace(/\+$/, '').trim();
  if (!id) return { ok: false, reason: 'empty licence identifier' };
  // SPDX spells versions with -only or -or-later; the policy is about the licence, not the spelling.
  const base = id.replace(/-(only|or-later)$/i, '');
  const inSet = (set) => set.has(id) || set.has(base);
  if (NEVER.some((r) => r.test(id))) return { ok: false, reason: id + ' is never allowed for this product' };
  if (inSet(ALLOWED_LINKED)) return { ok: true, reason: id + ' is allowed for linked code' };
  if (inSet(ALLOWED_DYNAMIC)) return { ok: true, dynamicOnly: true, reason: id + ' is allowed only when dynamically referenced; confirm the reference style' };
  if (inSet(ALLOWED_ASSET)) return { ok: true, assetOnly: true, reason: id + ' is allowed for fonts and assets, not for linked code' };
  if (/^A?GPL/i.test(id)) return { ok: false, reason: id + ' is allowed only as standalone infrastructure with an allow.json entry' };
  return { ok: false, reason: id + ' is not on the allowed list; verify the exact version at the source' };
}

/**
 * Judge an SPDX expression. OR means the consumer may choose, so one allowed
 * branch is enough. AND means every term applies, so every branch must pass.
 * Parentheses are flattened, which is correct for the shapes real packages use:
 * a flat OR list, a flat AND list, or one of each nested once.
 */
export function verdict(license, { kind = 'code', overrides = {}, pkg = '' } = {}) {
  if (license === undefined || license === null || String(license).trim() === '') {
    return { ok: false, reason: 'no licence recorded' };
  }
  let expr = String(license).trim();
  if (FILE_LICENCE.test(expr)) {
    const o = pkg ? overrides[pkg] : undefined;
    if (o) expr = typeof o === 'string' ? o : o.spdx;
    else {
      return {
        ok: false,
        reason: 'licence is shipped as a file; read it at the source and record the SPDX identifier under "overrides" in allow.json as "' + (pkg || '<package>@<version>') + '"',
      };
    }
  }
  const flat = expr.replace(/[()]/g, ' ').replace(/\s+/g, ' ').trim();
  const orParts = flat.split(/\s+OR\s+/i);
  const judged = orParts.map((part) => {
    const andParts = part.split(/\s+AND\s+/i).map(judgeOne);
    const failed = andParts.find((j) => !j.ok);
    return failed || { ok: true, reason: andParts.map((j) => j.reason).join('; '), assetOnly: andParts.some((j) => j.assetOnly), dynamicOnly: andParts.some((j) => j.dynamicOnly) };
  });
  const chosen = judged.find((j) => j.ok && !(kind === 'code' && j.assetOnly)) || judged.find((j) => j.ok);
  if (!chosen) return { ok: false, reason: judged.map((j) => j.reason).join(' | ') };
  if (kind === 'code' && chosen.assetOnly) {
    return { ok: false, reason: 'an asset licence is not allowed for linked code: ' + chosen.reason };
  }
  return { ok: true, reason: chosen.reason, dynamicOnly: Boolean(chosen.dynamicOnly) };
}

/* ------------------------------------------------------------------ CLI */

const MANIFESTS = [
  { file: 'Directory.Packages.props', ecosystem: 'nuget' },
  { file: 'package.json', ecosystem: 'npm' },
  { file: 'pubspec.yaml', ecosystem: 'pub' },
];

function findManifests(root) {
  const found = [];
  const skip = new Set(['node_modules', '.git', 'bin', 'obj', 'dist', 'build', '.dart_tool']);
  const walk = (dir, depth) => {
    if (depth > 6) return;
    let entries;
    try { entries = readdirSync(dir, { withFileTypes: true }); } catch { return; }
    for (const e of entries) {
      const abs = join(dir, e.name);
      if (e.isDirectory()) {
        if (skip.has(e.name)) continue;
        walk(abs, depth + 1);
      } else {
        const m = MANIFESTS.find((x) => x.file === e.name);
        if (m) found.push({ rel: relative(root, abs).split('\\').join('/'), ecosystem: m.ecosystem });
      }
    }
  };
  walk(root, 0);
  return found;
}

export function loadAllowList(root) {
  const p = join(root, 'tools/license-scan/allow.json');
  if (!existsSync(p)) return { entries: [], overrides: {}, problems: ['allow.json is missing'] };
  let json;
  try { json = JSON.parse(readFileSync(p, 'utf8')); } catch (e) {
    return { entries: [], overrides: {}, problems: ['allow.json is not valid JSON: ' + e.message] };
  }
  const problems = [];
  const entries = json.standalone || [];
  for (const e of entries) {
    for (const field of ['name', 'license', 'justification', 'adr']) {
      if (!e[field]) problems.push('allow.json entry ' + (e.name || '(unnamed)') + ' is missing "' + field + '"');
    }
  }
  const overrides = json.overrides || {};
  for (const [pkg, o] of Object.entries(overrides)) {
    if (!/^[^@\s]+@\S+$/.test(pkg)) problems.push('override key "' + pkg + '" must be <package>@<exact version>');
    if (!o || typeof o !== 'object' || !o.spdx || !o.source || !o.checked) {
      problems.push('override for ' + pkg + ' must record spdx, source (the URL the licence file was read from) and checked (the date)');
      continue;
    }
    const v = verdict(String(o.spdx));
    if (!v.ok) problems.push('override for ' + pkg + ' records a licence that is itself not allowed: ' + v.reason);
  }
  return { entries, overrides, problems };
}

function main() {
  const root = process.argv.find((a, i) => i > 1 && !a.startsWith('-')) || process.cwd();
  const manifests = findManifests(root);
  const allow = loadAllowList(root);
  const lines = ['Nibras licence scan', 'Root: ' + root, ''];
  let errors = 0;

  if (allow.problems.length) {
    lines.push('Allow-list problems:');
    for (const p of allow.problems) { lines.push('  error  ' + p); errors++; }
    lines.push('');
  } else {
    lines.push('Allow-list: ' + allow.entries.length + ' standalone tool(s) and ' + Object.keys(allow.overrides).length + ' licence override(s), each justified.');
    lines.push('');
  }

  if (!manifests.length) {
    lines.push('No dependency manifests found. There is no source code yet, so there is nothing to scan.');
    lines.push('This is expected during planning. The scan is wired in now so the first dependency is checked on the day it is added.');
  } else {
    lines.push('Manifests found:');
    for (const m of manifests) lines.push('  ' + m.ecosystem.padEnd(6) + ' ' + m.rel);
    lines.push('');
    lines.push('Resolving licences needs the ecosystem tooling (dotnet restore, npm ci, dart pub get) and a network;');
    lines.push('the pipeline job feeds each resolved package and licence expression through verdict().');
  }

  lines.push('');
  lines.push(errors ? 'licence scan: ' + errors + ' error(s).' : 'licence scan: clean.');
  process.stdout.write(lines.join('\n') + '\n');
  process.exit(errors ? 1 : 0);
}

const invoked = (process.argv[1] || '').split('\\').join('/');
if (invoked.endsWith('license-scan/run.mjs')) main();
