// The service template's own test (document 07, part 9; SL-INF-001). It generates a throwaway service into a
// temporary copy of the build inputs and proves the generated service builds, starts, answers its probes
// and passes its three generated test projects. Run: node --test tools/templates/service/template.test.mjs

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { cpSync, existsSync, mkdtempSync, readFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { newService, parseArgs, readRegistry } from './new-service.mjs';

const here = dirname(fileURLToPath(import.meta.url));
const kitRoot = resolve(here, '..', '..', '..');

function scratchRoot() {
  const root = mkdtempSync(join(tmpdir(), 'nibras-template-test-'));
  for (const file of ['global.json', 'nuget.config', 'Directory.Build.props', 'Directory.Packages.props', '.editorconfig', join('tools', 'ci', 'BannedSymbols.txt')]) {
    cpSync(join(kitRoot, file), join(root, file));
  }
  // Every building block, so a block that starts depending on another never breaks this test; their tests stay behind.
  const skipBuildOutput = (src) => !['bin', 'obj'].includes(basename(src));
  cpSync(join(kitRoot, 'src', 'BuildingBlocks'), join(root, 'src', 'BuildingBlocks'), {
    recursive: true,
    filter: (src) => skipBuildOutput(src) && src !== join(kitRoot, 'src', 'BuildingBlocks', 'tests'),
  });
  for (const dir of [
    join('src', 'ServiceDefaults', 'Nibras.ServiceDefaults'),
    join('src', 'Contracts', 'Nibras.Contracts.Shared'),
  ]) {
    cpSync(join(kitRoot, dir), join(root, dir), { recursive: true, filter: skipBuildOutput });
  }
  return root;
}

function dotnetTest(root, project) {
  const result = spawnSync('dotnet', ['test', '--project', project], { cwd: root, encoding: 'utf8' });
  return { status: result.status, output: `${result.stdout}\n${result.stderr}` };
}

test('the registry lists the twenty data-owning services of Appendix L', () => {
  const registry = readRegistry();
  assert.equal(registry.size, 20);
  assert.equal(registry.get('Attendance'), 'ATT');
  assert.equal(registry.get('Requests'), 'RQS');
  assert.equal(registry.has('Gateway'), false);
});

test('a name outside Appendix L is refused without --scratch', () => {
  assert.throws(() => newService(parseArgs(['--name', 'Canteen', '--area', 'CAN'])), /not a data-owning service in Appendix L/);
});

test('an area that disagrees with Appendix L is refused', () => {
  assert.throws(() => newService(parseArgs(['--name', 'Attendance', '--area', 'ATD'])), /gives Attendance the area ATT/);
});

test('malformed arguments are refused with the usage', () => {
  assert.throws(() => parseArgs(['--name', 'attendance', '--area', 'ATT']), /PascalCase/);
  assert.throws(() => parseArgs(['--name', 'Attendance', '--area', 'at']), /AREA code/);
  assert.throws(() => parseArgs(['--name', 'Attendance', '--area', 'ATT', '--bogus']), /Unknown argument/);
});

test('a generated service builds, starts, answers its probes and passes its generated tests', { timeout: 900_000 }, () => {
  const root = scratchRoot();
  try {
    const result = newService({ ...parseArgs(['--name', 'Probe', '--area', 'PRB', '--root', root, '--no-sln']), scratch: true });

    assert.equal(result.projects.length, 8, 'contract, four layers, three test projects');
    const readme = readFileSync(join(root, 'src', 'Services', 'Probe', 'README.md'), 'utf8');
    assert.match(readme, /`nibras_probe`/);
    assert.match(readme, /`nibras\.probe`/);
    assert.match(readme, /`PROBE_`/);
    assert.match(readme, /`PRB`/);
    const codes = readFileSync(join(root, 'src', 'Contracts', 'Nibras.Contracts.Probe', 'ErrorCodes', 'ProbeErrorCodes.cs'), 'utf8');
    assert.match(codes, /Prefix = "PROBE_"/);

    for (const project of ['UnitTests', 'IntegrationTests', 'ContractTests']) {
      const path = join('src', 'Services', 'Probe', 'tests', `Nibras.Probe.${project}`);
      const run = dotnetTest(root, path);
      assert.equal(run.status, 0, `${project} failed:\n${run.output}`);
      assert.match(run.output, /failed: 0/);
    }

    assert.throws(() => newService({ ...parseArgs(['--name', 'Probe', '--area', 'PRB', '--root', root, '--no-sln']), scratch: true }), /already exists/);
  } finally {
    rmSync(root, { recursive: true, force: true });
  }
  assert.equal(existsSync(root), false);
});
