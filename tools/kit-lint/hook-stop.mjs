/**
 * Invoked by the harness as: node <this file>. Not a standalone entry point,
 * so it carries no shebang and needs no shell wrappers.
 *
 * Stop hook: remind about project memory and consistency at the end of a session.
 *
 * Runs through node so it works identically on Windows and Linux. It only ever
 * prints; it never blocks and never edits anything.
 */

import { execSync } from 'node:child_process';
import { lint } from './kit-lint.mjs';

const run = (cmd) => {
  try {
    return execSync(cmd, { stdio: ['ignore', 'pipe', 'ignore'], encoding: 'utf8', timeout: 15000 }).trim();
  } catch {
    return null;
  }
};

let input = '';
process.stdin.setEncoding('utf8');
process.stdin.on('data', (c) => { input += c; });
process.stdin.on('end', () => {
  const notes = [];

  // Did documentation change without the project memory being updated?
  const changed = run('git status --porcelain');
  if (changed) {
    const lines = changed.split('\n').map((l) => l.slice(3).trim());
    const touchedDocs = lines.some((l) => l.startsWith('docs/') && !l.startsWith('docs/project/'));
    const touchedState = lines.some((l) => l.startsWith('docs/project/PROJECT_STATE.md'));
    const touchedBrief = lines.some((l) => l.startsWith('docs/brief/'));
    const touchedAdr = lines.some((l) => l.startsWith('docs/project/DECISIONS/'));

    if (touchedDocs && !touchedState) {
      notes.push('Documentation changed but docs/project/PROJECT_STATE.md did not. Update it so the next session starts where this one stopped.');
    }
    if (touchedBrief && !touchedAdr) {
      notes.push('docs/brief/ changed without a decision record. CLAUDE.md requires an ADR and a version bump on all three briefs.');
    }
  }

  let errors = [];
  try { errors = lint(process.cwd()).filter((f) => f.severity === 'error'); } catch { /* never fail a stop hook */ }
  if (errors.length) {
    notes.push('kit-lint reports ' + errors.length + ' consistency error(s). Run /lint-plan before handing over.');
  }

  if (notes.length) {
    process.stderr.write('\nBefore you finish:\n' + notes.map((n) => '  - ' + n).join('\n') + '\n');
  }
  process.exit(0);
});
