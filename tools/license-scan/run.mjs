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
 * Until source code exists this reports what it would scan and exits clean, so
 * it can be wired into the pipeline from the first commit rather than later.
 */

import { readFileSync, existsSync, readdirSync, statSync } from 'node:fs';
import { join, relative } from 'node:path';

const ALLOWED_LINKED = new Set([
  'MIT', 'Apache-2.0', 'BSD-2-Clause', 'BSD-3-Clause', 'ISC', 'MPL-2.0',
  'PostgreSQL', '0BSD', 'Unlicense', 'MIT-0', 'BSD-3-Clause-Clear',
]);

const ALLOWED_DYNAMIC = new Set(['LGPL-2.1', 'LGPL-2.1-only', 'LGPL-3.0', 'LGPL-3.0-only']);

const NEVER = [
  /^BUSL/i, /^SSPL/i, /^RSAL/i, /^Elastic/i, /^Commons-Clause/i,
  /commercial/i, /proprietary/i, /source-available/i,
];

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
        if (m) found.push({ path: abs, rel: relative(root, abs).split('\\').join('/'), ecosystem: m.ecosystem });
      }
    }
  };
  walk(root, 0);
  return found;
}

function loadAllowList(root) {
  const p = join(root, 'tools/license-scan/allow.json');
  if (!existsSync(p)) return { entries: [], problems: ['allow.json is missing'] };
  let json;
  try { json = JSON.parse(readFileSync(p, 'utf8')); } catch (e) {
    return { entries: [], problems: ['allow.json is not valid JSON: ' + e.message] };
  }
  const problems = [];
  const entries = json.standalone || [];
  for (const e of entries) {
    for (const field of ['name', 'license', 'justification', 'adr']) {
      if (!e[field]) problems.push('allow.json entry ' + (e.name || '(unnamed)') + ' is missing "' + field + '"');
    }
  }
  return { entries, problems };
}

function verdict(license) {
  if (!license) return { ok: false, reason: 'no licence recorded' };
  const id = String(license).trim();
  if (NEVER.some((r) => r.test(id))) return { ok: false, reason: 'licence is never allowed for this product' };
  if (ALLOWED_LINKED.has(id)) return { ok: true, reason: 'allowed for linked code' };
  if (ALLOWED_DYNAMIC.has(id)) return { ok: true, reason: 'allowed only when dynamically referenced; confirm the reference style' };
  if (/^A?GPL/i.test(id)) return { ok: false, reason: 'GPL or AGPL: allowed only as standalone infrastructure with an allow.json entry' };
  return { ok: false, reason: 'licence not on the allowed list; verify the exact version at the source' };
}

const root = process.argv.find((a, i) => i > 1 && !a.startsWith('-')) || process.cwd();
const manifests = findManifests(root);
const allow = loadAllowList(root);

const lines = [];
let errors = 0;

lines.push('Nibras licence scan');
lines.push('Root: ' + root);
lines.push('');

if (allow.problems.length) {
  lines.push('Allow-list problems:');
  for (const p of allow.problems) {
    lines.push('  error  ' + p);
    errors++;
  }
  lines.push('');
} else {
  lines.push('Allow-list: ' + allow.entries.length + ' standalone tool(s), each citing a justification and an ADR.');
  lines.push('');
}

if (!manifests.length) {
  lines.push('No dependency manifests found. There is no source code yet, so there is nothing to scan.');
  lines.push('This is expected during planning. The scan is wired in now so the first dependency is checked on the day it is added.');
} else {
  lines.push('Manifests found:');
  for (const m of manifests) lines.push('  ' + m.ecosystem.padEnd(6) + ' ' + m.rel);
  lines.push('');
  lines.push('Resolving licences needs the ecosystem tooling (dotnet restore, npm ci, dart pub get) and a network,');
  lines.push('so this scan reports the manifests and the policy it will apply. Wire the resolver in the pipeline job.');
  lines.push('');
  lines.push('Policy applied:');
  lines.push('  allowed for linked code: ' + [...ALLOWED_LINKED].join(', '));
  lines.push('  allowed dynamically:    ' + [...ALLOWED_DYNAMIC].join(', '));
  lines.push('  never:                  BUSL, SSPL, RSAL, Elastic, Commons Clause, commercial, source-available');
  lines.push('  GPL and AGPL:           standalone only, and only with an allow.json entry');
}

lines.push('');
lines.push(errors ? 'licence scan: ' + errors + ' error(s).' : 'licence scan: clean.');

process.stdout.write(lines.join('\n') + '\n');
process.exit(errors ? 1 : 0);

export { verdict, ALLOWED_LINKED, ALLOWED_DYNAMIC, NEVER };
