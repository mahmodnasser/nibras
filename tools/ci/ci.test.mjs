/**
 * Fast self-tests for the pipeline tool: the path filter, the test-project kinds, the coverage merge and
 * thresholds, and the phase gate. No .NET needed; the slow end-to-end proof is ci.violations.e2e.mjs.
 *
 * Run: node --test tools/ci/ci.test.mjs
 */

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync, readFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import path from 'node:path';
import { addWorkingDays, affected, classify, evaluateGate, GATE_ITEMS, generatedArgs, isolationEvidence, STAGES, writeIncident } from './ci.mjs';
import { evaluate, merge, parseCobertura } from './coverage.mjs';

test('TC-INF-114: a change inside one service builds that service only', () => {
  assert.deepEqual(affected(['src/Services/Attendance/Nibras.Attendance.Domain/Session.cs']), ['Attendance']);
  assert.deepEqual(
    affected(['src/Services/Finance/a.cs', 'src/Services/Attendance/b.cs', 'src/Services/Finance/c.cs']),
    ['Attendance', 'Finance'],
  );
});

test('TC-INF-114: a change to a building block, a contract or a root build file builds everything', () => {
  for (const file of [
    'src/BuildingBlocks/Nibras.BuildingBlocks.Web/ETags.cs',
    'src/Contracts/Nibras.Contracts.Shared/MessageEnvelope.cs',
    'src/ServiceDefaults/Nibras.ServiceDefaults/Probes.cs',
    'Directory.Packages.props',
    'Directory.Build.props',
    'global.json',
    'Nibras.sln',
    'tools/ci/ci.mjs',
    '.github/workflows/ci-service.yml',
    'tests/TenantIsolation.Tests/Targets/Canary.cs',
  ]) {
    assert.deepEqual(affected(['src/Services/Attendance/x.cs', file]), ['all'], file);
  }
});

test('TC-INF-114: a change outside code builds nothing, and Windows separators are read', () => {
  assert.deepEqual(affected(['docs/plan/22-api-conventions-and-error-catalog.md', 'README.md', 'tools/kit-lint/kit-lint.mjs']), []);
  assert.deepEqual(affected(['src\\Services\\Library\\Loan.cs']), ['Library']);
  assert.deepEqual(affected([]), []);
});

test('test projects are classified by the names the service template generates', () => {
  assert.equal(classify('src/Services/Attendance/tests/Nibras.Attendance.UnitTests/Nibras.Attendance.UnitTests.csproj'), 'unit');
  assert.equal(classify('src/BuildingBlocks/tests/Nibras.BuildingBlocks.Web.Tests/Nibras.BuildingBlocks.Web.Tests.csproj'), 'unit');
  assert.equal(classify('x/Nibras.Attendance.IntegrationTests.csproj'), 'integration');
  assert.equal(classify('x/Nibras.Attendance.ContractTests.csproj'), 'contract');
  assert.equal(classify('tests/Architecture.Tests/Architecture.Tests.csproj'), 'architecture');
  assert.equal(classify('tests/TenantIsolation.Tests/Nibras.TenantIsolation.Tests.csproj'), 'generated');
  assert.equal(classify('src/Services/Attendance/Nibras.Attendance.Domain/Nibras.Attendance.Domain.csproj'), null);
});

test('the generated suite runs prebuilt in scope all, and a service scope builds it and keeps the generator gate', () => {
  assert.deepEqual(generatedArgs('all'), ['--no-build']);
  assert.deepEqual(generatedArgs('Attendance'), ['-p:RestoreLockedMode=true', '--filter-trait', 'Service=Attendance', '--filter-trait', 'Service=all']);
});

const isolationReport = (failures = []) => ({
  endpointsAttacked: 6,
  consumersAttacked: 1,
  poolSwaps: 1,
  services: [{ service: 'Canary', endpoints: 6, consumers: 1, failed: failures.length }],
  failures,
});

const leak = { service: 'Canary', surface: 'GET /api/v1/canary/pupils', attack: 'foreign rows', detail: "answered 200 carrying tenant B's identifier 018f" };

test('TC-SEC-955: the isolation evidence counts what was attacked, a failure names the surface, and missing is not a pass', () => {
  const passed = isolationEvidence('passed', isolationReport());
  assert.equal(passed.status, 'passed');
  assert.equal(passed.endpoints, 6);
  assert.match(passed.detail, /^6 endpoints and 1 consumers attacked across 1 services \(Canary\), 1 pooled connections swapped$/);

  const failed = isolationEvidence('failed', isolationReport([leak]));
  assert.equal(failed.status, 'failed');
  assert.equal(failed.detail, "Canary GET /api/v1/canary/pupils: answered 200 carrying tenant B's identifier 018f");
  assert.equal(evaluateGate(1, [{ passed: false, evidence: { isolation: failed } }]).items.find((i) => i.id === 'isolation').status, 'failed');

  assert.equal(isolationEvidence('failed', null).detail, 'the tenant-isolation suite failed before it reported a verdict');
  assert.equal(isolationEvidence('passed', null).status, 'missing');
  assert.equal(isolationEvidence('not-applicable', null).status, 'missing');
});

test('TC-SEC-970: the incident review is due five working days later, in a Sunday-to-Thursday week', () => {
  assert.equal(addWorkingDays(new Date('2026-10-15T09:00:00Z'), 5), '2026-10-22'); // a Thursday: Friday and Saturday are skipped
  assert.equal(addWorkingDays(new Date('2026-10-19T23:30:00Z'), 5), '2026-10-26'); // a Monday
  assert.equal(addWorkingDays(new Date('2026-10-16T09:00:00Z'), 1), '2026-10-18'); // a Friday: the next working day is Sunday
});

test('TC-SEC-970: a run that caught an exposure writes an open Sev1 record and a job summary naming every failure', () => {
  const out = mkdtempSync(path.join(tmpdir(), 'nibras-incident-'));
  const summary = path.join(out, 'summary.md');
  const previous = process.env.GITHUB_STEP_SUMMARY;
  process.env.GITHUB_STEP_SUMMARY = summary;
  try {
    const { path: file, record } = writeIncident(out, { commit: '0123456789abcdef', scope: 'all' }, isolationReport([leak]), new Date('2026-10-19T08:00:00Z'));

    assert.equal(path.dirname(file), path.join(out, 'incidents'));
    const saved = JSON.parse(readFileSync(file, 'utf8'));
    assert.deepEqual(saved, record);
    assert.equal(saved.id, 'INC-20261019T080000-0123456');
    assert.equal(saved.severity, 'Sev1');
    assert.equal(saved.status, 'open');
    assert.equal(saved.reviewDue, '2026-10-26');
    assert.equal(saved.reviewWindow, '5 working days, Sunday to Thursday, on UTC dates');
    assert.deepEqual(saved.failures, [leak]);
    const text = readFileSync(summary, 'utf8');
    assert.match(text, /^## Sev1 INC-20261019T080000-0123456: Cross-tenant exposure caught by the tenant-isolation suite/);
    assert.match(text, /Incident review due 2026-10-26/);
    assert.match(text, /\| Canary \| `GET \/api\/v1\/canary\/pupils` \| foreign rows \|/);
  } finally {
    if (previous === undefined) delete process.env.GITHUB_STEP_SUMMARY;
    else process.env.GITHUB_STEP_SUMMARY = previous;
    rmSync(out, { recursive: true, force: true });
  }
});

test('TC-INF-114: the stages keep the fixed order of document 15 part 4.1', () => {
  assert.deepEqual(STAGES.map((s) => s.id), [
    'restore', 'build', 'unit', 'coverage', 'architecture', 'format', 'integration', 'contract',
    'generated', 'query-budget', 'licence', 'vulnerability', 'secret', 'image',
  ]);
});

/** A Cobertura document for one assembly whose file has `hit` covered lines out of `total`. */
function cobertura(assembly, source, file, hits, branches = []) {
  const lines = hits.map((h, i) => {
    const branch = branches[i];
    return branch
      ? `<line number="${i + 1}" hits="${h}" branch="True" condition-coverage="50% (${branch[0]}/${branch[1]})" />`
      : `<line number="${i + 1}" hits="${h}" branch="False" />`;
  }).join('');
  return `<?xml version="1.0"?><coverage><sources><source>${source}</source></sources><packages>`
    + `<package name="${assembly}"><classes><class name="C" filename="${file}"><methods><method name="M"><lines>${lines}</lines></method></methods>`
    + `<lines>${lines}</lines></class></classes></package></packages></coverage>`;
}

const ones = (n, hit) => Array.from({ length: n }, (_, i) => (i < hit ? 1 : 0));

test('TC-TST-952: two reports of one assembly under different source roots merge into one set of lines', () => {
  const a = parseCobertura(cobertura('Nibras.BuildingBlocks.Web', '/repo/src/BuildingBlocks/', 'Nibras.BuildingBlocks.Web/ETags.cs', [1, 0, 0, 0]));
  const b = parseCobertura(cobertura('Nibras.BuildingBlocks.Web', '/', 'repo/src/BuildingBlocks/Nibras.BuildingBlocks.Web/ETags.cs', [0, 1, 1, 1]));

  const [row] = evaluate(merge([a, b]));

  assert.equal(row.lines, 4);
  assert.equal(row.line, 100);
  assert.equal(row.passed, true);
});

test('TC-TST-952: a domain project at 88 percent fails naming the project, at 90 it passes, and branches count too', () => {
  const at = (hit, branches) => evaluate(merge([parseCobertura(cobertura('Nibras.Attendance.Domain', '/r/', 'Session.cs', ones(100, hit), branches))]))[0];

  const under = at(88);
  assert.equal(under.passed, false);
  assert.equal(under.assembly, 'Nibras.Attendance.Domain');
  assert.match(under.failures[0], /line 88% is under 90%/);
  assert.equal(at(89).passed, false);
  assert.equal(at(90).passed, true);
  assert.equal(at(91).passed, true);

  const thinBranches = at(100, [[4, 5], [4, 5]]); // 8 of 10 branches: 80 percent, under the domain's 85
  assert.equal(thinBranches.passed, false);
  assert.match(thinBranches.failures[0], /branch 80% is under 85%/);
});

test('TC-TST-952: application is held to 80, a building block to 90, and infrastructure is reported only', () => {
  const row = (assembly, hit) => evaluate(merge([parseCobertura(cobertura(assembly, '/r/', 'F.cs', ones(100, hit)))]))[0];

  assert.equal(row('Nibras.Attendance.Application', 79).passed, false);
  assert.equal(row('Nibras.Attendance.Application', 80).passed, true);
  assert.equal(row('Nibras.BuildingBlocks.Caching', 89).passed, false);
  assert.equal(row('Nibras.BuildingBlocks.Caching', 90).passed, true);
  const infra = row('Nibras.Attendance.Infrastructure', 10);
  assert.equal(infra.layer, 'reported');
  assert.equal(infra.passed, true);
});

const allPassed = () => Object.fromEntries(GATE_ITEMS.map((i) => [i.id, { status: 'passed' }]));

test('TC-TST-973: one high vulnerability open at the phase 3 gate keeps the phase from closing', () => {
  const evidence = allPassed();
  evidence.vulnerabilities = { status: 'passed', high: 1, critical: 0 };

  const gate = evaluateGate(3, [{ passed: true, evidence }]);

  assert.equal(gate.closed, false);
  const vulnerabilities = gate.items.find((i) => i.id === 'vulnerabilities');
  assert.equal(vulnerabilities.status, 'failed');
  assert.match(vulnerabilities.detail, /1 high open/);
});

test('TC-TST-973: every item with passing evidence closes the phase; a missing item never does', () => {
  assert.equal(evaluateGate(1, [{ passed: true, evidence: allPassed() }]).closed, true);

  const evidence = allPassed();
  delete evidence.accessibility;
  const gate = evaluateGate(1, [{ passed: true, evidence }]);
  assert.equal(gate.closed, false);
  assert.equal(gate.items.find((i) => i.id === 'accessibility').status, 'missing');
});

test('TC-TST-973: the gate reads several results files, and one failing scope fails the item', () => {
  const service = { passed: true, evidence: allPassed() };
  const other = { passed: false, evidence: { tests: { status: 'failed', detail: 'scope Finance' }, coverage: { status: 'passed' } } };

  const gate = evaluateGate(2, [service, other]);

  assert.equal(gate.closed, false);
  assert.equal(gate.items.find((i) => i.id === 'tests').status, 'failed');
  assert.equal(gate.items.find((i) => i.id === 'coverage').status, 'passed');
  assert.throws(() => evaluateGate(7, [service]), /1 to 6/);
});
