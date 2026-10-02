#!/usr/bin/env node
/**
 * The service pipeline of document 15 part 4.1, as one Node implementation that `ci-service.yml` and an
 * engineer's machine run the same way (Appendix X: `ci.ps1` and `ci.sh` wrap this file).
 *
 *   node tools/ci/ci.mjs affected --base <git ref> [--head <ref>] [--files a,b] [--github-output]
 *       Which scopes a change builds: `all` when a building block, a contract or a root build file changed,
 *       otherwise one scope per service under src/Services/<Service>/, and none for a change outside code.
 *
 *   node tools/ci/ci.mjs run [--scope all|<Service>] [--root <repo>] [--out <dir>]
 *       Runs the stages in their fixed order, cheap and deterministic first. The first failure stops the run;
 *       every later stage is recorded as skipped. Writes <out>/results.json, which the phase gate reads.
 *
 *   node tools/ci/ci.mjs gate --phase <1-6> <results.json>...
 *       The phase gate of document 16 part 12 over the same results: every gate item must have passing
 *       evidence. Missing evidence is not a pass (part 12.2), so the gate stays shut until it exists.
 *
 * No dependencies beyond Node 22 and the .NET SDK pinned by global.json.
 */

import { spawnSync } from 'node:child_process';
import { existsSync, mkdirSync, readdirSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { evaluate, merge, parseCobertura } from './coverage.mjs';

const here = path.dirname(fileURLToPath(import.meta.url));

/** A change under one of these builds every scope: they are shared by every service (master brief Section 29). */
const SHARED = [/^src\/BuildingBlocks\//, /^src\/Contracts\//, /^src\/ServiceDefaults\//, /^Directory\.(Build|Packages)\.props$/,
  /^global\.json$/, /^nuget\.config$/, /^[^/]+\.sln$/, /^\.editorconfig$/, /^tools\/ci\//, /^\.github\/workflows\/ci-service\.yml$/];

const SERVICE = /^src\/Services\/([A-Z][A-Za-z0-9]*)\//;

/** The scopes a set of changed paths builds. Paths use forward slashes, relative to the repository root. */
export function affected(files) {
  const scopes = new Set();
  for (const raw of files) {
    const file = raw.replaceAll('\\', '/').replace(/^\.\//, '');
    if (SHARED.some((re) => re.test(file))) return ['all'];
    const service = SERVICE.exec(file);
    if (service) scopes.add(service[1]);
  }
  return [...scopes].sort();
}

/** The stages of document 15 part 4.1, in order. `owner` names the slice that builds a stage not built yet. */
export const STAGES = [
  { id: 'restore', title: 'Restore, locked mode' },
  { id: 'build', title: 'Build, warnings and analyzers as errors (the lint)' },
  { id: 'unit', title: 'Unit tests with coverage' },
  { id: 'coverage', title: 'Coverage thresholds (document 16 part 15)' },
  { id: 'architecture', title: 'Architecture tests' },
  { id: 'format', title: 'Format check' },
  { id: 'integration', title: 'Integration tests' },
  { id: 'contract', title: 'Contract tests' },
  { id: 'generated', title: 'Generated permission and tenant-isolation suites', owner: 'SL-DATA-003' },
  { id: 'query-budget', title: 'Query-budget assertions', owner: 'SL-PERF-001' },
  { id: 'licence', title: 'Licence scan' },
  { id: 'vulnerability', title: 'Dependency and container vulnerability scan', owner: 'SL-SEC-001' },
  { id: 'secret', title: 'Secret scan', owner: 'SL-SEC-001' },
  { id: 'image', title: 'Publish the image, culture test in the image, SBOM, signature', owner: 'SL-SEC-001 and SL-INF-003' },
];

const IGNORED_DIRS = new Set(['bin', 'obj', 'node_modules', '.git', 'artifacts', 'TestResults']);

function findFiles(dir, test, found = []) {
  if (!existsSync(dir)) return found;
  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    if (entry.isDirectory()) {
      if (!IGNORED_DIRS.has(entry.name)) findFiles(path.join(dir, entry.name), test, found);
    } else if (test(entry.name)) {
      found.push(path.join(dir, entry.name));
    }
  }
  return found;
}

/**
 * Test projects by kind, from the names the service template generates: `*.IntegrationTests`, `*.ContractTests`,
 * `Architecture.Tests` or `*.ArchitectureTests`; `*.UnitTests` and a building block's `*.Tests` are unit tests.
 */
export function classify(projectPath) {
  const name = path.basename(projectPath, '.csproj');
  if (name.endsWith('.IntegrationTests')) return 'integration';
  if (name.endsWith('.ContractTests')) return 'contract';
  if (name === 'Architecture.Tests' || name.endsWith('.ArchitectureTests')) return 'architecture';
  return name.endsWith('Tests') ? 'unit' : null;
}

function scopeRoot(root, scope) {
  return scope === 'all' ? path.join(root, 'src') : path.join(root, 'src', 'Services', scope);
}

function run(command, args, cwd, log) {
  log(`$ ${command} ${args.join(' ')}`);
  const started = Date.now();
  const result = spawnSync(command, args, { cwd, encoding: 'utf8', maxBuffer: 256 * 1024 * 1024, env: { ...process.env, DOTNET_CLI_TELEMETRY_OPTOUT: '1', DOTNET_NOLOGO: '1' } });
  const output = `${result.stdout ?? ''}${result.stderr ?? ''}`;
  return { ok: result.status === 0, status: result.status, output, ms: Date.now() - started, error: result.error };
}

/** The last lines of a failed command, enough for the reader of a red job to see why. */
const tail = (text, lines = 25) => text.trimEnd().split(/\r?\n/).slice(-lines).join('\n');

/** Runs the pipeline for one scope. Returns the results object it also writes to <out>/results.json. */
export function runPipeline({ root, scope = 'all', out, log = console.log }) {
  const solution = readdirSync(root).filter((f) => f.endsWith('.sln') || f.endsWith('.slnx'));
  if (solution.length !== 1) throw new Error(`Expected one solution file at ${root}, found ${solution.length}.`);
  const base = scopeRoot(root, scope);
  const target = scope === 'all' ? path.join(root, solution[0]) : null;
  const projects = scope === 'all' ? [] : findFiles(base, (f) => f.endsWith('.csproj'));
  if (scope !== 'all' && projects.length === 0) throw new Error(`No projects under ${path.relative(root, base)} for scope ${scope}.`);
  const targets = target ? [target] : projects;
  const tests = findFiles(base, (f) => f.endsWith('.csproj')).filter((p) => classify(p) !== null).sort();
  const byKind = (kind) => tests.filter((p) => classify(p) === kind);

  const coverageDir = path.join(out, 'coverage');
  rmSync(out, { recursive: true, force: true });
  mkdirSync(coverageDir, { recursive: true });

  const results = { scope, startedAt: new Date().toISOString(), commit: gitHead(root), stages: [], coverage: [], evidence: {} };
  let failed = false;

  const dotnetEach = (args, list) => {
    let output = '';
    let ms = 0;
    for (const item of list) {
      const r = run('dotnet', [...args.slice(0, 1), item, ...args.slice(1)], root, log);
      output += r.output;
      ms += r.ms;
      if (!r.ok) return { ok: false, output, ms };
    }
    return { ok: true, output, ms };
  };

  const testProjects = (list, withCoverage) => {
    let output = '';
    let ms = 0;
    for (const project of list) {
      const name = path.basename(project, '.csproj');
      const extra = withCoverage
        ? ['--coverlet', '--coverlet-output-format', 'cobertura', '--coverlet-include', '[Nibras.*]*', '--coverlet-file-prefix', name, '--results-directory', coverageDir]
        : [];
      const r = run('dotnet', ['test', '--project', project, '--no-build', ...extra], root, log);
      output += r.output;
      ms += r.ms;
      if (!r.ok) return { ok: false, output, ms };
    }
    return { ok: true, output, ms };
  };

  const stageRunners = {
    restore: () => dotnetEach(['restore', '--locked-mode'], targets),
    build: () => dotnetEach(['build', '--no-restore', '-warnaserror'], targets),
    unit: () => (byKind('unit').length ? testProjects(byKind('unit'), true) : { na: 'no unit test project in this scope' }),
    coverage: () => {
      const reports = findFiles(coverageDir, (f) => f.endsWith('.xml') && f.includes('cobertura'));
      if (reports.length === 0) return { ok: false, output: 'The unit stage produced no coverage report.' };
      results.coverage = evaluate(merge(reports.map((r) => parseCobertura(readFileSync(r, 'utf8')))));
      const failures = results.coverage.filter((c) => !c.passed);
      const lines = results.coverage.map((c) => `${c.passed ? 'ok  ' : 'FAIL'} ${c.assembly} (${c.layer}): line ${c.line}%, branch ${c.branch}%${c.failures.length ? ' — ' + c.failures.join('; ') : ''}`);
      return { ok: failures.length === 0, output: lines.join('\n') };
    },
    architecture: () => (byKind('architecture').length ? testProjects(byKind('architecture'), false) : { na: 'no architecture test project yet; SL-TST-003 builds tests/Architecture.Tests' }),
    format: () => dotnetEach(['format', '--verify-no-changes', '--no-restore', '--verbosity', 'minimal'], targets),
    integration: () => (byKind('integration').length ? testProjects(byKind('integration'), false) : { na: 'no *.IntegrationTests project in this scope' }),
    // The generated OpenAPI and Problem Details contract tests join these projects with SL-API-005.
    contract: () => (byKind('contract').length ? testProjects(byKind('contract'), false) : { na: 'no *.ContractTests project in this scope' }),
    licence: () => run(process.execPath, [path.join(here, '..', 'license-scan', 'run.mjs')], root, log),
  };

  for (const stage of STAGES) {
    const record = { id: stage.id, title: stage.title, status: 'skipped', ms: 0, detail: '' };
    results.stages.push(record);
    if (failed) {
      record.detail = 'an earlier stage failed';
      continue;
    }

    const runner = stageRunners[stage.id];
    if (!runner) {
      record.status = 'not-built';
      record.detail = `built by ${stage.owner}`;
      log(`-- ${stage.title}: not built yet (${stage.owner})`);
      continue;
    }

    log(`== ${stage.title}`);
    const r = runner();
    record.ms = r.ms ?? 0;
    if (r.na) {
      record.status = 'not-applicable';
      record.detail = r.na;
      log(`   not applicable: ${r.na}`);
      continue;
    }

    record.status = r.ok ? 'passed' : 'failed';
    record.detail = r.ok ? (stage.id === 'coverage' ? r.output : '') : tail(r.output || String(r.error ?? 'failed'));
    log(`   ${record.status}${record.detail ? '\n' + record.detail : ''}`);
    if (!r.ok) failed = true;
  }

  results.finishedAt = new Date().toISOString();
  results.passed = !failed;
  const status = (id) => results.stages.find((s) => s.id === id).status;
  const testsGreen = ['unit', 'architecture', 'integration', 'contract'].every((id) => ['passed', 'not-applicable'].includes(status(id)))
    && ['restore', 'build'].every((id) => status(id) === 'passed');
  results.evidence = {
    tests: { status: testsGreen ? 'passed' : 'failed', detail: `scope ${scope}` },
    coverage: { status: status('coverage') === 'passed' ? 'passed' : status('coverage') === 'failed' ? 'failed' : 'missing' },
    licence: { status: status('licence') === 'passed' ? 'passed' : status('licence') === 'failed' ? 'failed' : 'missing' },
  };
  writeFileSync(path.join(out, 'results.json'), JSON.stringify(results, null, 2) + '\n');
  return results;
}

function gitHead(root) {
  const r = spawnSync('git', ['rev-parse', 'HEAD'], { cwd: root, encoding: 'utf8' });
  return r.status === 0 ? r.stdout.trim() : null;
}

/**
 * The gate items of document 16 part 12.1, each with the evidence key a results file carries. Items whose
 * subject does not exist before a phase are not applicable there (part 12.2); everything else needs a pass.
 */
export const GATE_ITEMS = [
  { id: 'tests', title: 'All tests green' },
  { id: 'coverage', title: 'Coverage thresholds met' },
  { id: 'mutation', title: 'Mutation score met on rule-heavy classes' },
  { id: 'vulnerabilities', title: 'Zero high or critical vulnerabilities' },
  { id: 'licence', title: 'Licence scan clean' },
  { id: 'accessibility', title: 'Accessibility clean' },
  { id: 'performance', title: 'Performance budgets met' },
  { id: 'isolation', title: 'Tenant isolation suite green' },
  { id: 'traceability', title: 'Traceability updated' },
  { id: 'documentation', title: 'Documentation and project memory updated' },
  { id: 'demo', title: 'Demo script passes' },
];

/**
 * Evaluates the gate of one phase over any number of results files. A failure in any file fails the item; an
 * item no file reports is missing; a vulnerability item with a high or critical finding fails whatever its
 * status says. The phase closes only when every applicable item passed.
 */
export function evaluateGate(phase, resultsList) {
  if (!Number.isInteger(phase) || phase < 1 || phase > 6) throw new Error('The phase is 1 to 6.');
  const items = GATE_ITEMS.map((item) => {
    const reports = resultsList.map((r) => r.evidence?.[item.id]).filter(Boolean);
    let status = 'missing';
    const details = [];
    for (const e of reports) {
      if (item.id === 'vulnerabilities' && ((e.high ?? 0) > 0 || (e.critical ?? 0) > 0)) {
        status = 'failed';
        details.push(`${e.critical ?? 0} critical and ${e.high ?? 0} high open`);
        continue;
      }

      if (e.status === 'failed') {
        status = 'failed';
        if (e.detail) details.push(e.detail);
      } else if (e.status === 'passed' && status === 'missing') {
        status = 'passed';
      }
    }

    if (resultsList.some((r) => r.passed === false) && item.id === 'tests' && status !== 'failed') status = 'failed';
    return { ...item, status, detail: details.join('; ') || (status === 'missing' ? 'no evidence; missing is not a pass' : '') };
  });
  return { phase, closed: items.every((i) => i.status === 'passed'), items };
}

function readOption(args, name) {
  const i = args.indexOf(name);
  return i >= 0 ? args[i + 1] : undefined;
}

function main(argv) {
  const [command, ...args] = argv;
  if (command === 'affected') {
    let files;
    if (args.includes('--files')) {
      files = readOption(args, '--files').split(',').filter(Boolean);
    } else {
      const baseRef = readOption(args, '--base');
      if (!baseRef) throw new Error('affected needs --base <ref> or --files <list>.');
      const headRef = readOption(args, '--head') ?? 'HEAD';
      const diff = spawnSync('git', ['diff', '--name-only', `${baseRef}...${headRef}`], { encoding: 'utf8' });
      if (diff.status !== 0) {
        // A shallow clone or a first push has no merge base: build everything rather than guess.
        files = ['Directory.Build.props'];
        console.error(`git diff failed (${diff.stderr.trim()}); building every scope.`);
      } else {
        files = diff.stdout.split(/\r?\n/).filter(Boolean);
      }
    }

    const scopes = affected(files);
    console.log(JSON.stringify(scopes));
    if (args.includes('--github-output') && process.env.GITHUB_OUTPUT) {
      writeFileSync(process.env.GITHUB_OUTPUT, `scopes=${JSON.stringify(scopes)}\n`, { flag: 'a' });
    }

    return 0;
  }

  if (command === 'run') {
    const root = path.resolve(readOption(args, '--root') ?? path.join(here, '..', '..'));
    const out = path.resolve(readOption(args, '--out') ?? path.join(root, 'artifacts', 'ci'));
    const results = runPipeline({ root, scope: readOption(args, '--scope') ?? 'all', out });
    console.log(`\npipeline ${results.passed ? 'passed' : 'FAILED'}: ${path.join(out, 'results.json')}`);
    return results.passed ? 0 : 1;
  }

  if (command === 'gate') {
    const phase = Number(readOption(args, '--phase'));
    const files = args.filter((a, i) => a !== '--phase' && args[i - 1] !== '--phase');
    if (files.length === 0) throw new Error('gate needs at least one results.json.');
    const gate = evaluateGate(phase, files.map((f) => JSON.parse(readFileSync(f, 'utf8'))));
    for (const item of gate.items) console.log(`${item.status.padEnd(8)} ${item.title}${item.detail ? ` — ${item.detail}` : ''}`);
    console.log(`\nphase ${phase} gate: ${gate.closed ? 'closed' : 'NOT closed'}`);
    return gate.closed ? 0 : 1;
  }

  console.error('usage: ci.mjs affected --base <ref> | run [--scope all|<Service>] [--root <dir>] [--out <dir>] | gate --phase <n> <results.json>...');
  return 2;
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  try {
    process.exitCode = main(process.argv.slice(2));
  } catch (error) {
    console.error(error.message);
    process.exitCode = 2;
  }
}

