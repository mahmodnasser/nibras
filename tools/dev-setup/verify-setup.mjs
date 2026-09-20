#!/usr/bin/env node
/**
 * Verify a developer machine can build and test Nibras.
 *
 * One Node implementation, run through verify-setup.ps1 or verify-setup.sh.
 * Prints one line per check and exits non-zero if a required check fails.
 *
 * It deliberately checks the things that differ between Windows, Linux and
 * macOS, because those are the failures that waste a first day.
 */

import { execSync } from 'node:child_process';
import { existsSync } from 'node:fs';
import { join } from 'node:path';
import { platform } from 'node:os';

const root = process.argv.find((a, i) => i > 1 && !a.startsWith('-')) || process.cwd();
const os = platform();
const results = [];

function run(cmd) {
  try {
    return execSync(cmd, { stdio: ['ignore', 'pipe', 'ignore'], encoding: 'utf8', timeout: 30000 }).trim();
  } catch {
    return null;
  }
}

function check(name, fn, { required = true, hint = '' } = {}) {
  let ok = false;
  let detail = '';
  try {
    const r = fn();
    ok = Boolean(r && r.ok);
    detail = (r && r.detail) || '';
  } catch (e) {
    detail = e.message;
  }
  results.push({ name, ok, detail, required, hint });
}

const major = (v) => {
  const m = /(\d+)\./.exec(v || '');
  return m ? Number(m[1]) : 0;
};

/* ------------------------------------------------------------------ checks */

check('Node 22 or later', () => {
  const v = process.versions.node;
  return { ok: major(v) >= 22, detail: 'v' + v };
}, { hint: 'The kit tooling and the Angular build both need it.' });

check('.NET SDK 10 or later', () => {
  const v = run('dotnet --version');
  return { ok: major(v) >= 10, detail: v || 'not found' };
}, { hint: 'winget install Microsoft.DotNet.SDK.10, or the apt package.' });

check('Aspire workload', () => {
  const out = run('dotnet workload list');
  const installed = Boolean(out && /aspire/i.test(out));
  return { ok: installed, detail: installed ? 'installed' : 'not installed' };
}, { required: false, hint: 'dotnet workload install aspire' });

check('Git', () => {
  const v = run('git --version');
  return { ok: Boolean(v), detail: v || 'not found' };
});

check('git core.autocrlf is false', () => {
  const v = run('git config --get core.autocrlf');
  const value = (v || 'unset').toLowerCase();
  return { ok: value === 'false' || value === 'input', detail: value };
}, { hint: 'git config --global core.autocrlf false. The repository normalises line endings itself.' });

if (os === 'win32') {
  check('git core.longpaths is true', () => {
    const v = run('git config --get core.longpaths');
    return { ok: (v || '').toLowerCase() === 'true', detail: v || 'unset' };
  }, { required: false, hint: 'git config --global core.longpaths true' });
}

check('A container runtime is reachable', () => {
  const d = run('docker version --format "{{.Server.Version}}"');
  if (d) return { ok: true, detail: 'docker ' + d };
  const p = run('podman version --format "{{.Server.Version}}"');
  if (p) return { ok: true, detail: 'podman ' + p };
  return { ok: false, detail: 'neither docker nor podman responded' };
}, { hint: 'Integration tests need Testcontainers. See docs/dev-setup/ for your operating system.' });

if (os === 'win32') {
  check('DOCKER_HOST is set', () => {
    const v = process.env.DOCKER_HOST;
    return { ok: Boolean(v), detail: v || 'unset' };
  }, { required: false, hint: 'Podman on Windows needs it. See docs/dev-setup/windows.md.' });
}

check('Flutter', () => {
  const v = run('flutter --version');
  return { ok: Boolean(v), detail: v ? v.split('\n')[0] : 'not found' };
}, { required: false, hint: 'Only needed for mobile work.' });

if (os === 'darwin') {
  check('Xcode command line tools', () => {
    const v = run('xcode-select -p');
    return { ok: Boolean(v), detail: v || 'not found' };
  }, { required: false, hint: 'Only needed to build for iOS, which only macOS can do.' });
}

check('Kit lint runs and is clean', () => {
  const lint = join(root, 'tools/kit-lint/kit-lint.mjs');
  if (!existsSync(lint)) return { ok: false, detail: 'kit-lint.mjs not found' };
  try {
    execSync('node "' + lint + '" "' + root + '"', { stdio: ['ignore', 'pipe', 'ignore'], timeout: 120000 });
    return { ok: true, detail: 'clean' };
  } catch {
    return { ok: false, detail: 'reported errors; run it directly to see them' };
  }
});

check('Licence scan runs', () => {
  const scan = join(root, 'tools/license-scan/run.mjs');
  if (!existsSync(scan)) return { ok: false, detail: 'run.mjs not found' };
  try {
    execSync('node "' + scan + '" "' + root + '"', { stdio: ['ignore', 'pipe', 'ignore'], timeout: 60000 });
    return { ok: true, detail: 'clean' };
  } catch {
    return { ok: false, detail: 'reported errors' };
  }
});

/* ------------------------------------------------------------------ report */

const label = { win32: 'Windows', darwin: 'macOS', linux: 'Linux' }[os] || os;
let out = '\nNibras developer setup check on ' + label + '\n\n';
let failed = 0;

for (const r of results) {
  const mark = r.ok ? 'ok   ' : r.required ? 'FAIL ' : 'skip ';
  if (!r.ok && r.required) failed++;
  out += '  ' + mark + r.name.padEnd(34) + ' ' + r.detail + '\n';
  if (!r.ok && r.hint) out += '       ' + r.hint + '\n';
}

out += '\n' + (failed
  ? failed + ' required check(s) failed. See docs/dev-setup/ for this operating system.\n'
  : 'Ready. Start everything with: aspire run (from src/AppHost)\n');

process.stdout.write(out);
process.exit(failed ? 1 : 0);
