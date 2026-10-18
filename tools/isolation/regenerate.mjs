#!/usr/bin/env node
/**
 * Regenerates tests/TenantIsolation.Tests/Generated/ from the endpoints and consumers of every target (document 12
 * part 4.4, document 16 part 3.1), then runs the whole suite once against the result. Review the diff and commit it:
 * the pipeline compares the committed classes with the services and fails on any difference.
 *
 *   node tools/isolation/regenerate.mjs        (or regenerate.ps1, regenerate.sh)
 *
 * Needs the .NET SDK pinned by global.json and a container engine for the suite's PostgreSQL and PgBouncer.
 */

import { spawnSync } from 'node:child_process';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..', '..');
const suite = path.join(root, 'tests', 'TenantIsolation.Tests');
const project = path.join(suite, 'Nibras.TenantIsolation.Tests.csproj');

function dotnet(args, env = {}) {
  console.log(`$ dotnet ${args.join(' ')}`);
  const result = spawnSync('dotnet', args, { cwd: root, stdio: 'inherit', env: { ...process.env, ...env } });
  return result.status ?? 1;
}

// Pass one writes the classes; pass two compiles them and attacks every surface they list.
let status = dotnet(['test', '--project', project, '--filter-class', 'Nibras.TenantIsolation.Tests.Generator.SurfaceCoverageTests'],
  { NIBRAS_ISOLATION_REGENERATE: path.join(suite, 'Generated') });
if (status === 0) status = dotnet(['test', '--project', project]);
process.exitCode = status;
