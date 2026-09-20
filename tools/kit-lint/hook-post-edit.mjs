/**
 * Invoked by the harness as: node <this file>. Not a standalone entry point,
 * so it carries no shebang and needs no shell wrappers.
 *
 * PostToolUse hook: lint the kit after an edit under docs/.
 *
 * Runs through node so it works identically on Windows and Linux. It never
 * blocks an edit; it reports, because a hook that blocks mid-task is a hook
 * people disable. Silent when the edit did not touch documentation.
 */

import { lint, formatFindings } from './kit-lint.mjs';

let input = '';
process.stdin.setEncoding('utf8');
process.stdin.on('data', (c) => { input += c; });
process.stdin.on('end', () => {
  let payload = {};
  try { payload = JSON.parse(input || '{}'); } catch { /* a hook must not fail on unexpected input */ }

  const path = String(
    payload?.tool_input?.file_path ||
    payload?.tool_input?.path ||
    ''
  ).split('\\').join('/');

  // Only documentation drift is this hook's business.
  if (!/\/docs\/|^docs\//.test(path) && !/CLAUDE\.md$/.test(path)) {
    process.exit(0);
  }

  let findings = [];
  try { findings = lint(process.cwd()); } catch { process.exit(0); }

  const errors = findings.filter((f) => f.severity === 'error');
  if (!errors.length) process.exit(0);

  process.stderr.write(
    '\nkit-lint found ' + errors.length + ' consistency error(s) after that edit:\n' +
    formatFindings(errors) +
    '\nFix these before ending the session, or run /lint-plan for the minimal fix set.\n'
  );
  process.exit(0);
});
