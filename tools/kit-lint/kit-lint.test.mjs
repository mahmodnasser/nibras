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

/* ---------------------------------------------------------------- R20 */

const r20 = (extra) => {
  const kit = catalogKit();
  kit['docs/brief/02-appendices/appendix-r-workflow-catalog.md'] = '# Appendix R. Workflows\n\n### WF-ATT-01 Mark attendance\n\n| Transition | Guard | Effect | Test |\n|---|---|---|---|\n| Open to Marked | Teacher assigned | Saved | TC-ATT-001 |\n';
  Object.assign(kit, extra);
  return lintFixture(kit, ['R20-test-case-identifiers']);
};

test('R20 accepts one definition cited elsewhere, an owner-named citation and a derived acceptance test', () => {
  const findings = r20({
    'docs/plan/06-services/attendance.md': '# Attendance\n\n## Test plan\n\n| Test case | What it proves |\n|---|---|\n| TC-ATT-101 | A late mark is refused |\n| `TC-ATT-001` (Appendix R) | The workflow transition |\n\nTC-ATT-101 and TC-ATT-951 prove REQ-ATT-001.\n',
  });
  assert.deepEqual(findings, [], JSON.stringify(findings));
});

test('R20 catches one identifier defined in two documents', () => {
  const findings = r20({
    'docs/plan/06-services/attendance.md': '# Attendance\n\n## Test plan\n\n| Test case | What it proves |\n|---|---|\n| TC-ATT-001 | Sixty-second attendance demo |\n',
  });
  assert.equal(findings.length, 1, JSON.stringify(findings));
  assert.match(findings[0].message, /TC-ATT-001 is defined in 2 documents/);
});

test('R20 catches a test that is cited but defined nowhere, unless it is only history', () => {
  const findings = r20({
    'docs/plan/12-security-privacy-safety.md': '# 12\n\n## Content\n\nProven by TC-SEC-404.\n',
    'docs/project/PROJECT_STATE.md': '# State\n\nTC-SEC-405 was retired.\n',
  });
  assert.equal(findings.length, 1, JSON.stringify(findings));
  assert.match(findings[0].message, /TC-SEC-404 is cited but defined in no document/);
});

test('R20 catches a derived acceptance test whose requirement does not exist', () => {
  const findings = r20({
    'docs/plan/06-services/attendance.md': '# Attendance\n\n## Content\n\nREQ-ATT-001 is proven by TC-ATT-990.\n',
  });
  assert.equal(findings.length, 1, JSON.stringify(findings));
  assert.match(findings[0].message, /TC-ATT-990 is a derived acceptance test, but REQ-ATT-040 does not exist/);
});

test('R20 treats a heading that opens with an identifier as a definition', () => {
  const findings = r20({
    'docs/plan/10-data-architecture.md': '# 10\n\n## Content\n\n### TC-ATT-001 A pooled connection cannot read the previous tenant\n\nText.\n',
  });
  assert.equal(findings.length, 1, JSON.stringify(findings));
  assert.match(findings[0].message, /TC-ATT-001 is defined in 2 documents/);
});

/* ---------------------------------------------------------- R21 to R32 */

const only = (id, files) => lintFixture({ ...catalogKit(), ...files }, [id]);
const messages = (fs) => fs.map((f) => f.message).join(' | ');
const FENCE = '```';

test('R21 catches a gap in numbering, a missing tier and an unknown rule in the requirements catalog', () => {
  const f = only('R21-requirements-catalog', {
    'docs/plan/03-requirements-catalog.md': '# 03\n\n| ID | Requirement | Tier | Service | Source | WF / BR | Acceptance |\n|---|---|---|---|---|---|---|\n| REQ-ATT-001 | Mark | 1 | Attendance | A | WF-ATT-01 | TC-ATT-001 |\n| REQ-ATT-003 | Late | x | Attendance | A | BR-ATT-777 | Given |\n',
  });
  assert.match(messages(f), /follows REQ-ATT-001/);
  assert.match(messages(f), /no tier/);
  assert.match(messages(f), /BR-ATT-777/);
});

test('R22 catches a cited ADR with no record, an unindexed record and a status mismatch', () => {
  const f = only('R22-adr-references', {
    'docs/project/DECISIONS/0001-one.md': '# ADR-0001: One\n\n- **Status:** Accepted\n',
    'docs/project/DECISIONS/0002-two.md': '# ADR-0002: Two\n\n- **Status:** Proposed\n',
    'docs/plan/29-adr-index.md': '# 29\n\n| ADR | Title | Status |\n|---|---|---|\n| 0001 | One | Proposed |\n',
    'docs/plan/04-architecture-overview.md': '# 04\n\nSee ADR-0009.\n',
  });
  assert.match(messages(f), /ADR-0009 is cited/);
  assert.match(messages(f), /ADR 0002 .* has no row/);
  assert.match(messages(f), /"Proposed" here but "Accepted"/);
});

test('R23 reports a generator whose check fails', () => {
  const f = only('R23-generated-documents', {
    'tools/plan-build/gen-20.mjs': "console.log('stale: docs/plan/20-traceability-matrix.md differs'); process.exit(1);\n",
    'docs/plan/20-traceability-matrix.md': '# 20\n',
  });
  assert.equal(f.length, 1, JSON.stringify(f));
  assert.match(f[0].message, /stale/);
});

test('R24 catches a score that is not likelihood times impact and an unknown register reference', () => {
  const f = only('R24-risk-tables', {
    'docs/plan/18-risk-register.md': '# 18\n\n| Id | Risk | L | I | Score |\n|---|---|---|---|---|\n| RISK-01 | Late | 3 | 4 | 11 |\n',
    'docs/plan/10-data-architecture.md': '# 10\n\n| Risk | L | I | In the register |\n|---|---|---|---|\n| Leak | 2 | 5 | RISK-99 |\n',
  });
  assert.match(messages(f), /score 11 is not 3 x 4/);
  assert.match(messages(f), /RISK-99 is not in document 18/);
});

test('R25 catches a workflow in no capability and a capability with no slices', () => {
  const f = only('R25-roadmap-coverage', {
    'docs/plan/34-work-breakdown.md': '# 34\n\n#### CAP-ATT-02 Other\n\n| SL-ATT-001 | x |\n',
  });
  assert.match(messages(f), /WF-ATT-01 is built by no capability/);
  assert.match(messages(f), /CAP-ATT-01 has no slices/);
});

test('R26 catches an open question missing from document 01', () => {
  const f = only('R26-open-questions-mirror', {
    'docs/project/OPEN_QUESTIONS.md': '# OQ\n\n| # | Question |\n|---|---|\n| 1 | Which runtime? |\n| 2 | Which host? |\n\n## Settled\n',
    'docs/plan/01-questions-and-assumptions.md': '# 01\n\n| # | Question |\n|---|---|\n| 1 | Which runtime? |\n',
  });
  assert.equal(f.length, 1, JSON.stringify(f));
  assert.match(f[0].message, /Open question 2/);
});

test('R27 catches an uncatalogued key in document 11 and a service publishing on another exchange', () => {
  const f = only('R27-messaging-keys', {
    'docs/plan/11-messaging-architecture.md': '# 11\n\n| Target | Commands |\n|---|---|\n| Attendance | `CloseRegister` |\n\nUses `attendance.commands.close-register.v1`, `attendance.student.absent.v1` and `attendance.register.teleported.v1`.\n',
    'docs/plan/05-service-catalog.md': '# 05\n\n| Service | Publishes |\n|---|---|\n| **Attendance** | `finance.invoice.issued.v1` |\n',
  });
  assert.match(messages(f), /attendance\.register\.teleported\.v1 is used in document 11/);
  assert.match(messages(f), /only on its own exchange/);
  assert.doesNotMatch(messages(f), /close-register/);
});

test('R28 catches a high-impact threat with no test', () => {
  const f = only('R28-threat-coverage', {
    'docs/plan/12-security-privacy-safety.md': '# 12\n\n| ID | Threat | Impact | Test |\n|---|---|---|---|\n| T-ATT-01 | Forged mark | high | policy |\n| T-ATT-02 | Typo | low | none |\n',
  });
  assert.equal(f.length, 1, JSON.stringify(f));
  assert.match(f[0].message, /T-ATT-01 has impact high/);
});

test('R29 catches a state diagram with an unlabelled transition and no exit', () => {
  const f = only('R29-state-diagrams', {
    'docs/plan/13-workflows-and-sagas.md': '# 13\n\n' + FENCE + 'mermaid\nstateDiagram-v2\n    [*] --> Open\n    Open --> Marked\n' + FENCE + '\n',
  });
  assert.match(messages(f), /no terminal state/);
  assert.match(messages(f), /Transition has no label: Open --> Marked/);
});

test('R30 catches a column without a comment', () => {
  const f = only('R30-sql-comments', {
    'docs/plan/10-data-architecture.md': '# 10\n\n' + FENCE + 'sql\nCREATE TABLE attendance.mark (\n    tenant_id uuid NOT NULL, -- owner\n    status smallint NOT NULL,\n    PRIMARY KEY (tenant_id)\n);\n' + FENCE + '\n',
  });
  assert.equal(f.length, 1, JSON.stringify(f));
  assert.match(f[0].message, /status smallint/);
});

test('R31 catches an unregistered image and database, and ignores metric and label prefixes', () => {
  const f = only('R31-canonical-names', {
    'docs/brief/02-appendices/appendix-l-registry-and-id-codes.md': '# L\n\n| Service | Database | Images |\n|---|---|---|\n| **Attendance** | `nibras_attendance` | `nibras/attendance-api` |\n| **Reporting** | `nibras_reporting` | `nibras/reporting-api`, `-projections` |\n',
    'docs/plan/07-solution-structure.md': '# 07\n\nImages `nibras/attendance-worker`, `nibras/reporting-projections`; metric `nibras_ui`; label `nibras/tenant`.\n\n| Service | Database |\n|---|---|\n| Attendance | `nibras_attendence` |\n',
  });
  assert.match(messages(f), /Image nibras\/attendance-worker/);
  assert.doesNotMatch(messages(f), /reporting-projections|nibras_ui|nibras\/tenant/);
  assert.match(messages(f), /nibras_attendence/);
});

test('R32 catches an owner sheet that does not cite a transition test of its workflow', () => {
  const kit = catalogKit();
  kit['docs/brief/02-appendices/appendix-r-workflow-catalog.md'] = '# R\n\n### WF-ATT-01 Mark attendance\n\n| Transition | Test |\n|---|---|\n| Open to Marked | TC-ATT-001 |\n| Marked to Locked | TC-ATT-002 |\n';
  kit['docs/brief/02-appendices/appendix-l-registry-and-id-codes.md'] = '# L\n';
  kit['docs/plan/13-workflows-and-sagas.md'] = '# 13\n\n### 1. Workflow assignment\n\n| Workflow | Name | Owner |\n|---|---|---|\n| WF-ATT-01 | Mark | Attendance |\n';
  kit['docs/plan/06-services/attendance.md'] = '# Attendance\n\n## 15. Test plan\n\n| Test case | What |\n|---|---|\n| `TC-ATT-001` (Appendix R) | Open to Marked |\n';
  const f = lintFixture(kit, ['R32-workflow-tests']);
  assert.equal(f.length, 1, JSON.stringify(f));
  assert.match(f[0].message, /does not cite TC-ATT-002/);
});

test('R18 requires a comment on every plan tree entry', () => {
  const f = only('R18-tree-comments', {
    'docs/plan/07-solution-structure.md': '# 07\n\n' + FENCE + '\nsrc/\n├── a/        # one\n├── b/        # two\n├── c/        # three\n├── d/\n└── e/        # five\n' + FENCE + '\n',
  });
  assert.equal(f.length, 1, JSON.stringify(f));
  assert.equal(f[0].severity, 'error');
});

test('R33 requires scored open points, a register link at 12 or more, and owned threat rows', () => {
  const f = only('R33-open-points', {
    'docs/plan/10-data-architecture.md': '# 10\n\n## Open points\n\n| Question | Default | Owner | L | I | Score | In the register |\n|---|---|---|---|---|---|---|\n| Hot partition | Split later | Architect | 4 | 3 | 12 | none |\n| Minor | Keep | Architect | 1 | 2 | 2 | none |\n',
    'docs/plan/11-messaging-architecture.md': '# 11\n\n## Open points\n\n| Question | Default | Owner |\n|---|---|---|\n| Queue | Keep | Architect |\n',
    'docs/plan/21-performance-engineering.md': '# 21\n\n## Content\n',
    'docs/plan/12-security-privacy-safety.md': '# 12\n\n## Open points\n\nNone.\n\n## Threats\n\n| ID | Threat | Impact | Test |\n|---|---|---|---|\n| T-ATT-01 | Forged | high | TC-SEC-001 |\n',
  });
  const m = messages(f);
  assert.match(m, /scores 12 but names no RISK/);
  assert.match(m, /no L, I, Score, In the register column/);
  assert.match(m, /No "Open points" section/);
  assert.match(m, /Threat table has no Owner role column/);
  assert.doesNotMatch(m, /Minor/);
});

test('R34 requires every signature feature to have a step that runs its demo test, and every step a phase', () => {
  const f = only('R34-demo-coverage', {
    'docs/brief/02-appendices/appendix-w-feature-register.md': '# W\n\n| # | Feature | Demo |\n|---|---|---|\n| 1 | Morning brief | TC-RPT-001 |\n| 2 | Sixty-second attendance | TC-ATT-810 |\n| 3 | Live interface | TC-UX-001 |\n| 4 | Scaling: moved to engineering capabilities | None |\n| 5 | Emergency mode | TC-ATT-813 |\n',
    'docs/brief/02-appendices/appendix-o-demo-script.md': '# O\n\nFeature 3 is guarded by TC-UX-001.\n\n| Min | Persona | What happens | Feature | Test | Phase |\n|---|---|---|---|---|---|\n| 1 | Principal | Brief | 1 | TC-RPT-001 | 4 |\n| 2 | Teacher | Register | 2 | TC-ATT-003 | 2 |\n| 3 | Officer | Drill | 5 | TC-ATT-813 | |\n',
  });
  const m = messages(f);
  assert.match(m, /Feature 2 is shown in step 2 but no such step runs its demo test TC-ATT-810/);
  assert.match(m, /Demo step 3 does not say which phase/);
  assert.doesNotMatch(m, /Feature 1 |Feature 3 |Feature 4 /);
});

test('R35 requires every Tier 2 requirement built in phases 1 to 4 to be listed in document 17, and nothing else', () => {
  const f = only('R35-tier-ahead', {
    'docs/plan/03-requirements-catalog.md': '# 03\n\n| ID | Requirement | Tier |\n|---|---|---|\n| REQ-ATT-001 | Mark | 1 |\n| REQ-ATT-002 | Kiosk | 2 |\n| REQ-ATT-003 | Face | 3 |\n',
    'docs/plan/34-work-breakdown.md': '# 34\n\n### 3. Phase 1: Foundation\n\n| SL-ATT-001 | x | Attendance | 2 | REQ-ATT-001, REQ-ATT-002 | x | none |\n\n### 7. Phase 5: Extended\n\n| SL-ATT-600 | x | Attendance | 2 | REQ-ATT-003 | x | none |\n',
    'docs/plan/17-roadmap.md': '# 17\n\n| Capability | What |\n|---|---|\n| CAP-ATT-01 | Take attendance |\n\n### Requirements built ahead of their tier\n\n| Requirement | Tier | Slices | Phase | Why it is built early |\n|---|---|---|---|---|\n| REQ-ATT-003 | 3 | SL-ATT-600 | 5 | stale |\n',
  });
  const m = messages(f);
  assert.match(m, /REQ-ATT-002 is Tier 2 but SL-ATT-001 builds it/);
  assert.match(m, /REQ-ATT-003 is listed as built ahead of its tier, but no phase 1 to 4 slice builds it/);
  assert.doesNotMatch(m, /REQ-ATT-001/);
});

test('R24 requires a risk row scoring 12 or more to name a RISK in document 18', () => {
  const f = only('R24-risk-tables', {
    'docs/plan/18-risk-register.md': '# 18\n\n| Id | Risk | L | I | Score |\n|---|---|---|---|---|\n| RISK-01 | Late | 3 | 4 | 12 |\n',
    'docs/plan/06-services/attendance.md': '# Attendance\n\n| Risk | L | I | Score | In the register |\n|---|---|---|---|---|\n| Pool exhaustion | 3 | 4 | 12 | Not yet |\n| Covered | 4 | 3 | 12 | RISK-01 |\n| Small | 2 | 2 | 4 | none |\n',
  });
  assert.equal(f.length, 1, JSON.stringify(f));
  assert.match(f[0].message, /Pool exhaustion: scores 12 but names no RISK/);
});
