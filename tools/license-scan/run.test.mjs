/**
 * Tests for the licence verdict. Each case is a licence shape a real package
 * publishes, so a regression here is a build that would wrongly pass or fail.
 *
 * Run: node --test tools/license-scan/run.test.mjs
 */

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { verdict } from './run.mjs';

const ok = (lic, opts) => assert.equal(verdict(lic, opts).ok, true, lic + ' should be allowed: ' + verdict(lic, opts).reason);
const no = (lic, opts) => assert.equal(verdict(lic, opts).ok, false, lic + ' should be refused');

test('plain permissive identifiers are allowed', () => {
  for (const l of ['MIT', 'Apache-2.0', 'BSD-3-Clause', 'ISC', 'MPL-2.0', 'PostgreSQL']) ok(l);
});

test('a dual licence is allowed when either branch is', () => {
  ok('MS-PL OR Apache-2.0');     // CsvHelper publishes this shape
  ok('MPL-2.0 OR Apache-2.0');   // RabbitMQ.Client publishes this shape
  ok('(MIT OR Apache-2.0)');
});

test('a conjunction is allowed only when every branch is', () => {
  ok('MIT AND BSD-3-Clause');
  no('MIT AND SSPL-1.0');
});

test('source-available and commercial licences are refused, even inside an OR with nothing else', () => {
  no('BUSL-1.1');
  no('SSPL-1.0');
  no('Elastic-2.0');
  no('LicenseRef-Commercial');
});

test('GPL and AGPL are refused for linked code', () => {
  no('GPL-3.0-only');
  no('AGPL-3.0-or-later');
});

test('a WITH exception is judged on its base licence', () => {
  ok('Apache-2.0 WITH LLVM-exception');
  no('GPL-2.0-only WITH Classpath-exception-2.0');
});

test('font licences are allowed as assets and refused as linked code', () => {
  ok('OFL-1.1', { kind: 'asset' });
  no('OFL-1.1', { kind: 'code' });
});

test('LGPL is allowed and flagged as dynamic-only', () => {
  const v = verdict('LGPL-2.1-only');
  assert.equal(v.ok, true);
  assert.equal(v.dynamicOnly, true);
});

test('a licence shipped as a file is refused until an override records its SPDX identifier', () => {
  const pkg = 'Example.Package@1.2.3';
  const refused = verdict('file:LICENSE.txt', { pkg });
  assert.equal(refused.ok, false);
  assert.match(refused.reason, /overrides/);
  ok('file:LICENSE.txt', { pkg, overrides: { [pkg]: 'MIT' } });
});

test('an empty licence is refused', () => {
  no('');
  no(undefined);
});

test('the -only and -or-later suffixes are judged on the licence they spell', () => {
  ok('MPL-2.0-or-later');
  ok('LGPL-3.0-or-later');
  no('AGPL-3.0-only');
});

test('an override must record where its licence was read from', async () => {
  const { mkdtempSync, mkdirSync, writeFileSync, rmSync } = await import('node:fs');
  const { tmpdir } = await import('node:os');
  const { join } = await import('node:path');
  const { loadAllowList } = await import('./run.mjs');
  const root = mkdtempSync(join(tmpdir(), 'licscan-'));
  try {
    mkdirSync(join(root, 'tools/license-scan'), { recursive: true });
    const write = (overrides) => writeFileSync(join(root, 'tools/license-scan/allow.json'), JSON.stringify({ standalone: [], overrides }));
    write({ 'Bogus@35.6.1': 'MIT' });
    assert.ok(loadAllowList(root).problems.some((p) => /source/.test(p)), 'a bare string override must be refused');
    write({ 'Bogus@35.6.1': { spdx: 'MIT', source: 'https://github.com/bchavez/Bogus/blob/master/LICENSE', checked: '2026-09-22' } });
    assert.deepEqual(loadAllowList(root).problems, []);
    write({ 'Some.Pkg@1.0.0': { spdx: 'SSPL-1.0', source: 'https://example.org/LICENSE', checked: '2026-09-22' } });
    assert.ok(loadAllowList(root).problems.some((p) => /not allowed/.test(p)), 'an override cannot launder a banned licence');
  } finally {
    rmSync(root, { recursive: true, force: true });
  }
});
