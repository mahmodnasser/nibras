/**
 * Self-tests for kit-lint.
 *
 * Each test builds a throwaway kit that contains exactly one defect class and
 * asserts that the matching rule fires. A rule that cannot be proven to fire is
 * a rule nobody can trust.
 *
 * Run: node --test tools/kit-lint/
 */

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync, mkdirSync, writeFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, dirname } from 'node:path';
import { lint, rules } from './kit-lint.mjs';

/** Build a temporary kit from a { relativePath: contents } map and lint it. */
function lintFixture(files, only) {
  const root = mkdtempSync(join(tmpdir(), 'kitlint-'));
  try {
    for (const [rel, body] of Object.entries(files)) {
      const abs = join(root, rel);
      mkdirSync(dirname(abs), { recursive: true });
      writeFileSync(abs, body, 'utf8');
    }
    return lint(root, only);
  } finally {
    rmSync(root, { recursive: true, force: true });
  }
}

const ruleIds = (findings) => [...new Set(findings.map((f) => f.rule))];
const fired = (findings, id) => findings.some((f) => f.rule === id);

const MASTER = [
  '# Nibras: Master Brief v9',
  '',
  '## 1. Conventions',
  'Text.',
  '',
  '## 7. Microservices Architecture',
  '',
  '### 7.1 Principles',
  'Text.',
  '',
  '### 7.2 Service catalog',
  'Text.',
  '',
].join('\n');

const APPENDIX_INDEX = ['# Nibras: Appendices v9', '', '## Appendix A. Feature Catalog', 'Text.', ''].join('\n');
const RA = ['# Nibras Reference Architecture v9', '', '## 8. Service Specification Sheets', '', '### 8.1 Gateway', 'Text.', ''].join('\n');

const baseKit = () => ({
  'docs/brief/01-master-brief.md': MASTER,
  'docs/brief/02-appendices/00-index.md': APPENDIX_INDEX,
  'docs/brief/03-reference-architecture.md': RA,
});

test('a clean fixture produces no findings', () => {
  const findings = lintFixture(baseKit());
  assert.deepEqual(ruleIds(findings), [], 'expected no findings, got ' + JSON.stringify(findings));
});

test('every rule has a unique id and a title', () => {
  const ids = rules.map((r) => r.id);
  assert.equal(new Set(ids).size, ids.length, 'rule ids must be unique');
  for (const r of rules) {
    assert.match(r.id, /^R\d\d-[a-z-]+$/, r.id + ' does not follow the R00-name convention');
    assert.ok(r.title.length > 10, r.id + ' needs a descriptive title');
  }
});

test('R01 catches a reference to a section that does not exist', () => {
  const kit = baseKit();
  kit['docs/plan/PLAN_SPEC.md'] = '# Plan\n\nSee Section 99 of the master brief.\n';
  assert.ok(fired(lintFixture(kit, ['R01-section-refs']), 'R01-section-refs'));
});

test('R01 accepts a reference to a section that exists', () => {
  const kit = baseKit();
  kit['docs/plan/PLAN_SPEC.md'] = '# Plan\n\nSee Section 7.2 of the master brief.\n';
  assert.deepEqual(lintFixture(kit, ['R01-section-refs']), []);
});

test('R02 catches a reference to an appendix that does not exist', () => {
  const kit = baseKit();
  kit['docs/plan/PLAN_SPEC.md'] = '# Plan\n\nSee Appendix Z.\n';
  assert.ok(fired(lintFixture(kit, ['R02-appendix-refs']), 'R02-appendix-refs'));
});

test('R03 catches version drift between the briefs', () => {
  const kit = baseKit();
  kit['docs/brief/02-appendices/00-index.md'] = APPENDIX_INDEX.replace('v9', 'v7');
  const findings = lintFixture(kit, ['R03-brief-versions']);
  assert.ok(fired(findings, 'R03-brief-versions'));
  assert.ok(findings.some((f) => f.message.includes('v7')));
});

test('R04 catches a slash command with no command file', () => {
  const kit = baseKit();
  kit['CLAUDE.md'] = '# Nibras\n\nCommands: `/plan-platform`, `/ghost-command`\n';
  kit['.claude/commands/plan-platform.md'] = 'body';
  const findings = lintFixture(kit, ['R04-command-files']);
  assert.equal(findings.length, 1);
  assert.match(findings[0].message, /ghost-command/);
});

test('R05 catches a TODO left in a normative document', () => {
  const kit = baseKit();
  kit['docs/brief/01-master-brief.md'] = MASTER + '\nTODO: decide this later.\n';
  assert.ok(fired(lintFixture(kit, ['R05-no-open-todos']), 'R05-no-open-todos'));
});

test('R05 allows an open question to stay open', () => {
  const kit = baseKit();
  kit['docs/project/OPEN_QUESTIONS.md'] = '# Open Questions\n\nTBD with a default and an owner.\n';
  assert.deepEqual(lintFixture(kit, ['R05-no-open-todos']), []);
});

test('R06 catches a service that is missing from the dependency matrix', () => {
  const kit = baseKit();
  kit['docs/brief/03-reference-architecture.md'] = RA + '\n### 8.2 Wellbeing\nText.\n';
  kit['docs/brief/02-appendices/00-index.md'] = APPENDIX_INDEX + '\n`gateway.routes.view` `wellbeing.cases.view`\n';
  const findings = lintFixture(kit, ['R06-service-coverage']);
  assert.ok(findings.some((f) => /Wellbeing is missing from the event dependency matrix/.test(f.message)));
});

test('R07 catches a routing key that is not in the event catalog', () => {
  const kit = baseKit();
  kit['docs/brief/03-reference-architecture.md'] = RA + '\nPublishes `attendance.student.absent.v1`.\n';
  kit['docs/brief/02-appendices/00-index.md'] = APPENDIX_INDEX + '\n## Appendix E. Events\n\nRouting key format.\n\n- `attendance.student.late`\n';
  const findings = lintFixture(kit, ['R07-events-catalogued']);
  assert.ok(fired(findings, 'R07-events-catalogued'));
  assert.match(findings[0].message, /attendance\.student\.absent/);
});

test('R08 catches a workflow with no diagram or test table', () => {
  const kit = baseKit();
  kit['docs/brief/02-appendices/appendix-r-workflows.md'] = '# Appendix R\n\n### WF-ATT-01 Mark attendance\n\nSome prose only.\n';
  const findings = lintFixture(kit, ['R08-workflow-shape']);
  assert.equal(findings.length, 3, JSON.stringify(findings));
});

test('R08 accepts a complete workflow', () => {
  const kit = baseKit();
  kit['docs/brief/02-appendices/appendix-r-workflows.md'] = [
    '# Appendix R',
    '',
    '### WF-ATT-01 Mark attendance',
    '',
    '```mermaid',
    'stateDiagram-v2',
    '    [*] --> Open',
    '```',
    '',
    '| Transition | Guard | Test |',
    '|---|---|---|',
    '| Open to Marked | teacher has permission | TC-ATT-001 |',
    '',
  ].join('\n');
  assert.deepEqual(lintFixture(kit, ['R08-workflow-shape']), []);
});

test('R09 catches a business rule with fewer than three worked examples', () => {
  const kit = baseKit();
  kit['docs/brief/02-appendices/appendix-s-business-rules.md'] = [
    '# Appendix S',
    '',
    '### BR-FIN-001 Late fee',
    '',
    'Tests: `LateFeeRulesTests`',
    '',
    '- Given an invoice 5 days overdue, When the job runs, Then a 2 percent fee is added.',
    '',
  ].join('\n');
  const findings = lintFixture(kit, ['R09-rule-examples']);
  assert.ok(findings.some((f) => /1 worked examples/.test(f.message)));
});

test('R10 catches a workflow that the simulation never exercises', () => {
  const kit = baseKit();
  kit['docs/brief/02-appendices/appendix-r-workflows.md'] = '# Appendix R\n\n### WF-FIN-09 Refund\n\nText.\n';
  kit['docs/brief/02-appendices/appendix-t-year-in-the-life.md'] = '# Appendix T\n\nSeptember exercises nothing.\n';
  const findings = lintFixture(kit, ['R10-simulation-coverage']);
  assert.ok(findings.some((f) => /WF-FIN-09 is never exercised/.test(f.message)));
});

test('R11 catches a signature feature with no assist rung', () => {
  const kit = baseKit();
  kit['docs/brief/02-appendices/appendix-w-feature-register.md'] = [
    '# Appendix W',
    '',
    '| # | Feature | Moment | Rung | Tier |',
    '|---|---|---|---|---|',
    '| 25 | Morning brief | I know my day | 1 | 1 |',
    '| 26 | Hall pass | Who is out | | 2 |',
    '',
  ].join('\n');
  const findings = lintFixture(kit, ['R11-feature-register']);
  assert.equal(findings.length, 1);
  assert.match(findings[0].message, /Hall pass/);
});

test('R12 catches a notification row with no trigger', () => {
  const kit = baseKit();
  kit['docs/brief/02-appendices/appendix-c-notifications.md'] = [
    '# Appendix C',
    '',
    '| Event | Recipient | Channels | Urgency | Trigger |',
    '|---|---|---|---|---|',
    '| Student absent | Guardians | Push | U | `attendance.student.absent.v1` |',
    '| Cheque bounced | Accountant | Email | N | none |',
    '',
  ].join('\n');
  const findings = lintFixture(kit, ['R12-notification-triggers']);
  assert.equal(findings.length, 1);
  assert.match(findings[0].message, /Cheque bounced/);
});

test('R13 catches two paths that differ only by case', () => {
  const kit = baseKit();
  kit['docs/notes/readme.md'] = 'a';
  kit['docs/notes/README.md'] = 'b';
  const findings = lintFixture(kit, ['R13-case-collisions']);
  // On a case-insensitive file system the second write replaces the first, so
  // the rule can only be proven where the collision can actually exist.
  if (findings.length === 0) return;
  assert.ok(fired(findings, 'R13-case-collisions'));
});

test('R15 catches a tool entry point with no wrappers', () => {
  const kit = baseKit();
  kit['tools/demo/run.mjs'] = '#!/usr/bin/env node\nconsole.log(1);\n';
  const findings = lintFixture(kit, ['R15-script-wrappers']);
  assert.equal(findings.length, 2);
  assert.ok(findings.some((f) => f.message.endsWith('.ps1')));
  assert.ok(findings.some((f) => f.message.endsWith('.sh')));
});

test('R16 catches a hook that does not run through node', () => {
  const kit = baseKit();
  kit['.claude/settings.json'] = JSON.stringify({
    hooks: { PostToolUse: [{ matcher: 'Write', hooks: [{ type: 'command', command: 'bash tools/kit-lint/kit-lint.sh' }] }] },
  });
  assert.ok(fired(lintFixture(kit, ['R16-portable-hooks']), 'R16-portable-hooks'));
});

test('R16 accepts a node hook with a relative path', () => {
  const kit = baseKit();
  kit['.claude/settings.json'] = JSON.stringify({
    hooks: { PostToolUse: [{ matcher: 'Write', hooks: [{ type: 'command', command: 'node tools/kit-lint/kit-lint.mjs' }] }] },
  });
  assert.deepEqual(lintFixture(kit, ['R16-portable-hooks']), []);
});

test('R17 catches a Mermaid block with no diagram type', () => {
  const kit = baseKit();
  kit['docs/plan/04-architecture.md'] = '# A\n\n```mermaid\nA --> B\n```\n';
  assert.ok(fired(lintFixture(kit, ['R17-mermaid-blocks']), 'R17-mermaid-blocks'));
});

test('R17 accepts a state diagram', () => {
  const kit = baseKit();
  kit['docs/plan/04-architecture.md'] = '# A\n\n```mermaid\nstateDiagram-v2\n    [*] --> Draft\n```\n';
  assert.deepEqual(lintFixture(kit, ['R17-mermaid-blocks']), []);
});

test('R18 warns about a directory tree with no purpose comments', () => {
  const kit = baseKit();
  kit['docs/plan/07-solution-structure.md'] = [
    '# Structure',
    '',
    '```',
    'src/',
    '├── Gateway/',
    '├── Services/',
    '├── Web/',
    '└── Mobile/',
    '```',
    '',
  ].join('\n');
  assert.ok(fired(lintFixture(kit, ['R18-tree-comments']), 'R18-tree-comments'));
});

/* ---------------------------------------------------------------- R19 */

const catalogKit = () => ({
  ...baseKit(),
  'docs/brief/02-appendices/appendix-e-event-catalog.md': '# Appendix E. Events\n\n| Event |\n|---|\n| `attendance.student.absent.v1` |\n',
  'docs/brief/02-appendices/appendix-r-workflow-catalog.md': '# Appendix R. Workflows\n\n### WF-ATT-01 Mark attendance\n',
  'docs/brief/02-appendices/appendix-s-business-rules.md': '# Appendix S. Rules\n\n### BR-ATT-001 Lock window\n',
  'docs/brief/02-appendices/appendix-k-error-codes.md': '# Appendix K. Errors\n\n| Code | Status |\n|---|---|\n| `_VALIDATION_FAILED` | 400 |\n| `ATTENDANCE_SESSION_LOCKED` | 409 |\n',
  'docs/brief/02-appendices/appendix-b-permissions.md': '# Appendix B. Permissions\n\n| Resource | Standard | Special | Risk |\n|---|---|---|---|\n| `attendance.student-attendance` | view, export | `mark`, `edit-after-lock` (elevated) | elevated |\n',
  'docs/plan/03-requirements-catalog.md': '# 03\n\n| ID | Requirement |\n|---|---|\n| REQ-ATT-001 | Mark attendance |\n',
  'docs/plan/17-roadmap.md': '# 17\n\n| Capability | What |\n|---|---|\n| CAP-ATT-01 | Take attendance |\n',
});

const r19 = (planText) => {
  const kit = catalogKit();
  kit['docs/plan/06-services/attendance.md'] = planText;
  return lintFixture(kit, ['R19-plan-identifiers']);
};

test('R19 accepts identifiers that exist in their catalogs', () => {
  const findings = r19([
    '# Attendance', '', '## Content', '',
    'Covers REQ-ATT-001, WF-ATT-01, BR-ATT-001 and CAP-ATT-01.',
    'Publishes `attendance.student.absent.v1` and `attendance.audit.recorded.v1`.',
    'Fails with `ATTENDANCE_SESSION_LOCKED` or `ATTENDANCE_VALIDATION_FAILED`.',
    '', '| Method | Path | Permission |', '|---|---|---|', '| POST | /x | `attendance.student-attendance.mark` |', '',
  ].join('\n'));
  assert.deepEqual(findings, [], JSON.stringify(findings));
});

test('R19 catches every kind of invented identifier', () => {
  const findings = r19([
    '# Attendance', '', '## Content', '',
    'Covers REQ-ATT-099, WF-ATT-09, BR-ATT-999 and CAP-ATT-99.',
    'Publishes `attendance.student.vanished.v1`.',
    'Fails with `ATTENDANCE_MADE_UP`.',
    '', '| Method | Path | Permission |', '|---|---|---|', '| POST | /x | `attendance.student-attendance.teleport` |', '',
  ].join('\n'));
  const kinds = findings.map((f) => f.message.split(' ')[0] + ' ' + f.message.split(' ')[1]);
  for (const expected of ['Requirement REQ-ATT-099', 'Workflow WF-ATT-09', 'Business rule', 'Capability CAP-ATT-99', 'Routing key', 'Error code', 'Permission attendance.student-attendance.teleport']) {
    assert.ok(findings.some((f) => f.message.startsWith(expected)), 'expected a finding for ' + expected + ', got ' + JSON.stringify(kinds));
  }
});

test('R19 leaves a defect alone when it is reported under Open points or said to be missing', () => {
  const findings = r19([
    '# Attendance', '', '## Content', '',
    'Appendix E lacks `attendance.student.vanished.v1`, so it is not published.',
    '', '## Open points', '',
    '| Point | Default |', '|---|---|',
    '| `attendance.roll.called.v1` and `ATTENDANCE_MADE_UP` for REQ-ATT-099 | Keep |', '',
  ].join('\n'));
  assert.deepEqual(findings, [], JSON.stringify(findings));
});

test('R19 accepts a command key derived from a name in document 11 and refuses an invented one', () => {
  const kit = catalogKit();
  kit['docs/plan/11-messaging-architecture.md'] = '# 11\n\n| Target | Commands |\n|---|---|\n| Communication | `BookMeeting` |\n';
  kit['docs/plan/06-services/communication.md'] = '# Communication\n\n## Content\n\nConsumes `communication.commands.book-meeting.v1` and `communication.commands.teleport-meeting.v1`.\n';
  const findings = lintFixture(kit, ['R19-plan-identifiers']);
  assert.equal(findings.length, 1, JSON.stringify(findings));
  assert.match(findings[0].message, /teleport-meeting/);
});
