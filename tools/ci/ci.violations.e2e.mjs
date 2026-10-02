/**
 * TC-INF-114, TC-TST-952 and TC-TST-953 end to end. A throwaway solution is built with the repository's own
 * build rules (Directory.Build.props, the central package versions, global.json, .editorconfig, the banned
 * symbols). It passes the pipeline as it is. Then one deliberate violation per stage is committed to a copy,
 * and each copy must fail at its own stage, with every earlier stage green and every later one skipped.
 *
 * Needs the .NET SDK of global.json and network access to nuget.org. It takes a few minutes, so it runs in
 * ci-service.yml when tools/ci changes, not in the fast kit tests.
 *
 * Run: node --test tools/ci/ci.violations.e2e.mjs (named outside *.test.mjs so the fast kit tests skip it)
 */

import { describe, test, before, after } from 'node:test';
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { cpSync, mkdirSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { runPipeline } from './ci.mjs';

const repo = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..', '..');
const workspace = mkdtempSync(path.join(tmpdir(), 'nibras-ci-'));
const baseline = path.join(workspace, 'baseline');

const DOMAIN = path.join('src', 'Sample', 'Nibras.Sample.Domain');
const TESTS = path.join('src', 'Sample', 'tests', 'Nibras.Sample.Domain.Tests');

const files = {
  [path.join(DOMAIN, 'Nibras.Sample.Domain.csproj')]: '<Project Sdk="Microsoft.NET.Sdk">\n</Project>\n',
  [path.join(DOMAIN, 'Calc.cs')]: [
    'namespace Nibras.Sample.Domain;',
    '',
    'public static class Calc',
    '{',
    '    public static int Add(int a, int b) => a + b;',
    '}',
    '',
  ].join('\n'),
  [path.join(TESTS, 'Nibras.Sample.Domain.Tests.csproj')]: [
    '<Project Sdk="Microsoft.NET.Sdk">',
    '  <ItemGroup>',
    '    <PackageReference Include="xunit.v3" />',
    '  </ItemGroup>',
    '  <ItemGroup>',
    '    <ProjectReference Include="../../Nibras.Sample.Domain/Nibras.Sample.Domain.csproj" />',
    '  </ItemGroup>',
    '</Project>',
    '',
  ].join('\n'),
  [path.join(TESTS, 'CalcTests.cs')]: [
    'using Xunit;',
    '',
    'namespace Nibras.Sample.Domain.Tests;',
    '',
    'public sealed class CalcTests',
    '{',
    '    [Fact]',
    '    public void Add_TwoNumbers_ReturnsTheirSum() => Assert.Equal(5, Calc.Add(2, 3));',
    '}',
    '',
  ].join('\n'),
};

function dotnet(args, cwd) {
  const r = spawnSync('dotnet', args, { cwd, encoding: 'utf8', env: { ...process.env, DOTNET_CLI_TELEMETRY_OPTOUT: '1', DOTNET_NOLOGO: '1' } });
  assert.equal(r.status, 0, `dotnet ${args.join(' ')}\n${r.stdout}${r.stderr}`);
}

before(() => {
  for (const name of ['global.json', 'Directory.Build.props', 'Directory.Packages.props', 'nuget.config', '.editorconfig']) {
    cpSync(path.join(repo, name), path.join(baseline, name));
  }

  cpSync(path.join(repo, 'tools', 'ci', 'BannedSymbols.txt'), path.join(baseline, 'tools', 'ci', 'BannedSymbols.txt'));
  cpSync(path.join(repo, 'tools', 'license-scan', 'allow.json'), path.join(baseline, 'tools', 'license-scan', 'allow.json'));
  for (const [file, content] of Object.entries(files)) {
    mkdirSync(path.dirname(path.join(baseline, file)), { recursive: true });
    writeFileSync(path.join(baseline, file), content);
  }

  dotnet(['new', 'sln', '--name', 'Sample', '--format', 'sln'], baseline);
  dotnet(['sln', 'Sample.sln', 'add', path.join(DOMAIN, 'Nibras.Sample.Domain.csproj'), path.join(TESTS, 'Nibras.Sample.Domain.Tests.csproj')], baseline);
  dotnet(['restore', 'Sample.sln'], baseline); // writes the lock files the locked-mode stage then holds the copies to
});

after(() => {
  if (!process.env.NIBRAS_CI_KEEP) rmSync(workspace, { recursive: true, force: true });
});

/** Copies the baseline, applies one violation, runs the pipeline, and returns the stage statuses by id. */
function runWith(name, mutate) {
  const root = path.join(workspace, name);
  cpSync(baseline, root, { recursive: true });
  mutate(root);
  const results = runPipeline({ root, out: path.join(root, 'artifacts', 'ci'), log: () => {} });
  return { results, status: Object.fromEntries(results.stages.map((s) => [s.id, s.status])), stage: (id) => results.stages.find((s) => s.id === id) };
}

const edit = (root, file, change) => writeFileSync(path.join(root, file), change(readFileSync(path.join(root, file), 'utf8')));

/** The failed stage is `id`; every stage before it passed or did not apply; every stage after it was skipped. */
function failsAt({ results, status }, id) {
  assert.equal(results.passed, false);
  assert.equal(status[id], 'failed', JSON.stringify(status));
  const order = results.stages.map((s) => s.id);
  for (const before of order.slice(0, order.indexOf(id))) assert.ok(['passed', 'not-applicable', 'not-built'].includes(status[before]), `${before} was ${status[before]}`);
  for (const later of order.slice(order.indexOf(id) + 1)) assert.equal(status[later], 'skipped', later);
}

describe('one deliberate violation per stage fails its own stage', { concurrency: 3 }, () => {
  test('TC-INF-114: the untouched sample passes every built stage', () => {
    const { results, status } = runWith('clean', () => {});
    assert.equal(results.passed, true, JSON.stringify(results.stages, null, 2));
    for (const id of ['restore', 'build', 'unit', 'coverage', 'format', 'licence']) assert.equal(status[id], 'passed', id);
    assert.equal(results.coverage.find((c) => c.assembly === 'Nibras.Sample.Domain').line, 100);
  });

  test('TC-INF-114: a package added without its lock file fails the restore', () => {
    failsAt(runWith('lock-drift', (root) => edit(root, path.join(TESTS, 'Nibras.Sample.Domain.Tests.csproj'),
      (s) => s.replace('<PackageReference Include="xunit.v3" />', '<PackageReference Include="xunit.v3" />\n    <PackageReference Include="Shouldly" />'))), 'restore');
  });

  test('TC-INF-114: a compiler warning fails the build', () => {
    const run = runWith('warning', (root) => edit(root, path.join(DOMAIN, 'Calc.cs'),
      (s) => s.replace('    public static int Add', '    public static int Unused()\n    {\n        var unused = 0;\n        return 1;\n    }\n\n    public static int Add')));
    failsAt(run, 'build');
    assert.match(run.stage('build').detail, /CS0219/);
  });

  test('TC-TST-953: a test that sleeps fails the build', () => {
    const run = runWith('sleep', (root) => edit(root, path.join(TESTS, 'CalcTests.cs'),
      (s) => s.replace('    [Fact]', '    [Fact]\n    public void Add_AfterAPause_ReturnsTheirSum()\n    {\n        System.Threading.Thread.Sleep(10);\n        Assert.Equal(5, Calc.Add(2, 3));\n    }\n\n    [Fact]')));
    failsAt(run, 'build');
    assert.match(run.stage('build').detail, /RS0030/);
  });

  test('TC-INF-114: a failing test fails the unit stage', () => {
    failsAt(runWith('failing-test', (root) => edit(root, path.join(TESTS, 'CalcTests.cs'), (s) => s.replace('Assert.Equal(5,', 'Assert.Equal(6,'))), 'unit');
  });

  test('TC-TST-952: an under-covered domain project fails the coverage stage naming the project', () => {
    const run = runWith('under-covered', (root) => edit(root, path.join(DOMAIN, 'Calc.cs'),
      (s) => s.replace('    public static int Add', '    public static int Clamp(int value, int max)\n    {\n        var result = value > max ? max : value;\n        return result;\n    }\n\n    public static int Add')));
    failsAt(run, 'coverage');
    assert.match(run.stage('coverage').detail, /FAIL Nibras\.Sample\.Domain \(domain\): line \d+(\.\d)?%/);
  });

  test('TC-INF-114: an unformatted file fails the format stage', () => {
    failsAt(runWith('unformatted', (root) => edit(root, path.join(DOMAIN, 'Calc.cs'),
      (s) => s.replace('    public static int Add(int a, int b) => a + b;', '        public static int Add(int a, int b) => a + b;'))), 'format');
  });
});
