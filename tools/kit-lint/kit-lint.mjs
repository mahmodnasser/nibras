#!/usr/bin/env node
/**
 * Nibras kit lint.
 *
 * One Node implementation, run from PowerShell (kit-lint.ps1) or bash (kit-lint.sh).
 * No dependencies. Exit code 0 when there is no error, 1 when there is at least one.
 *
 * Every rule exists because a real defect was found in kit v8.
 * docs/project/KIT_V9_CHANGES.md records the finding each rule protects.
 */

import { readdirSync, readFileSync, statSync } from 'node:fs';
import { join, relative } from 'node:path';
import { spawnSync } from 'node:child_process';

const SKIP_DIRS = new Set(['node_modules', '.git', 'bin', 'obj', 'dist', '.dart_tool', 'build']);
const TEXT_EXT = /\.(md|json|ya?ml|mjs|js|ps1|sh|txt|gitattributes|editorconfig)$/i;

/* ------------------------------------------------------------------ context */

export function collectFiles(root) {
  const out = [];
  const walk = (dir) => {
    let entries;
    try {
      entries = readdirSync(dir, { withFileTypes: true });
    } catch {
      return;
    }
    for (const e of entries.sort((a, b) => a.name.localeCompare(b.name))) {
      const abs = join(dir, e.name);
      if (e.isDirectory()) {
        if (SKIP_DIRS.has(e.name)) continue;
        walk(abs);
      } else if (e.isFile()) {
        out.push(abs);
      }
    }
  };
  walk(root);
  return out;
}

export function buildContext(root) {
  const files = collectFiles(root).map((abs) => {
    const rel = relative(root, abs).split('\\').join('/');
    const readable = TEXT_EXT.test(rel) && statSync(abs).size < 4 * 1024 * 1024;
    const text = readable ? readFileSync(abs, 'utf8') : '';
    return { abs, rel, text, lines: text ? text.split(/\r?\n/) : [] };
  });
  const byRel = new Map(files.map((f) => [f.rel, f]));
  return { root, files, byRel, md: files.filter((f) => f.rel.endsWith('.md')) };
}

/* ------------------------------------------------------------------ helpers */

const finding = (rule, severity, file, line, message) => ({ rule, severity, file, line, message });

export function headings(file) {
  const out = [];
  let fenced = false;
  file.lines.forEach((raw, i) => {
    if (/^\s*```/.test(raw)) {
      fenced = !fenced;
      return;
    }
    if (fenced) return;
    const m = /^(#{1,6})\s+(.*?)\s*$/.exec(raw);
    if (m) out.push({ level: m[1].length, text: m[2], line: i + 1 });
  });
  return out;
}

/** Lines outside fenced code blocks, as [lineNumber, text]. */
export function proseLines(file) {
  const out = [];
  let fenced = false;
  file.lines.forEach((raw, i) => {
    if (/^\s*```/.test(raw)) {
      fenced = !fenced;
      return;
    }
    if (!fenced) out.push([i + 1, raw]);
  });
  return out;
}

export function fencedBlocks(file, lang) {
  const blocks = [];
  let cur = null;
  file.lines.forEach((raw, i) => {
    const m = /^\s*```(\S*)\s*$/.exec(raw);
    if (m) {
      if (cur === null) cur = { lang: m[1], start: i + 1, body: [] };
      else {
        blocks.push(cur);
        cur = null;
      }
      return;
    }
    if (cur) cur.body.push(raw);
  });
  return lang ? blocks.filter((b) => b.lang === lang) : blocks;
}

/** Every markdown table in a file: { header, rows, line }. */
export function tables(file) {
  const out = [];
  let cur = null;
  let fenced = false;
  file.lines.forEach((raw, i) => {
    if (/^\s*```/.test(raw)) {
      fenced = !fenced;
      return;
    }
    if (fenced) return;
    if (!/^\s*\|.*\|\s*$/.test(raw)) {
      if (cur) {
        out.push(cur);
        cur = null;
      }
      return;
    }
    const cells = raw.trim().replace(/^\|/, '').replace(/\|$/, '').split('|').map((c) => c.trim());
    if (!cur) cur = { header: cells, rows: [], line: i + 1 };
    else if (!/^:?-{2,}:?$/.test(cells[0] || '')) cur.rows.push(cells);
  });
  if (cur) out.push(cur);
  return out;
}

const briefFile = (ctx, name) => ctx.byRel.get('docs/brief/' + name);
const appendixFiles = (ctx) => ctx.md.filter((f) => f.rel.startsWith('docs/brief/02-appendices'));

/* -------------------------------------------------------------------- rules */

export const rules = [];
const rule = (id, title, run) => rules.push({ id, title, run });

rule('R01-section-refs', 'Every "Section N" reference resolves to a master-brief heading', (ctx) => {
  const brief = briefFile(ctx, '01-master-brief.md');
  if (!brief) return [];
  const top = new Set();
  const sub = new Set();
  for (const h of headings(brief)) {
    const m = /^(\d+)(?:\.(\d+))?\.?\s/.exec(h.text);
    if (!m) continue;
    if (m[2] === undefined) top.add(m[1]);
    else sub.add(m[1] + '.' + m[2]);
  }
  if (!top.size) return [];
  const parentsWithSubs = new Set([...sub].map((s) => s.split('.')[0]));
  // "reference architecture Section 8.0" is a reference architecture section, not a master-brief one.
  const ra = briefFile(ctx, '03-reference-architecture.md');
  const raTop = new Set();
  const raSub = new Set();
  if (ra) for (const h of headings(ra)) {
    const m = /^(d+)(?:.(d+))?.?s/.exec(h.text);
    if (m) (m[2] === undefined ? raTop : raSub).add(m[2] === undefined ? m[1] : m[1] + '.' + m[2]);
  }
  const out = [];
  for (const f of ctx.md) {
    if (f.rel.startsWith('tools/')) continue;
    for (const [ln, text] of proseLines(f)) {
      for (const m of text.matchAll(/\bSection\s+(\d+)(?:\.(\d+))?/g)) {
        const major = m[1];
        const minor = m[2];
        const before = text.slice(Math.max(0, m.index - 60), m.index);
        if (raTop.size && /reference architecture[^.;]*$/i.test(before)) {
          if (!raTop.has(major)) out.push(finding('R01-section-refs', 'error', f.rel, ln, 'Reference architecture Section ' + major + ' does not exist'));
          else if (minor !== undefined && ![...raSub].some((x) => x.startsWith(major + '.')) === false && !raSub.has(major + '.' + minor)) out.push(finding('R01-section-refs', 'warn', f.rel, ln, 'Reference architecture Section ' + major + '.' + minor + ' has no matching sub-heading'));
          continue;
        }
        if (!top.has(major)) {
          out.push(finding('R01-section-refs', 'error', f.rel, ln, 'Section ' + major + ' does not exist in the master brief'));
          continue;
        }
        if (minor !== undefined && parentsWithSubs.has(major) && !sub.has(major + '.' + minor)) {
          out.push(finding('R01-section-refs', 'warn', f.rel, ln, 'Section ' + major + '.' + minor + ' has no matching sub-heading'));
        }
      }
    }
  }
  return out;
});

rule('R02-appendix-refs', 'Every "Appendix X" reference resolves to an appendix heading', (ctx) => {
  const apps = appendixFiles(ctx);
  if (!apps.length) return [];
  const known = new Set();
  for (const f of apps) {
    for (const h of headings(f)) {
      const m = /^Appendix\s+([A-Z])\b/.exec(h.text);
      if (m) known.add(m[1]);
    }
  }
  if (!known.size) return [];
  const out = [];
  for (const f of ctx.md) {
    if (f.rel.startsWith('tools/')) continue;
    for (const [ln, text] of proseLines(f)) {
      for (const m of text.matchAll(/\bAppendix\s+([A-Z])\b/g)) {
        if (!known.has(m[1])) out.push(finding('R02-appendix-refs', 'error', f.rel, ln, 'Appendix ' + m[1] + ' does not exist'));
      }
    }
  }
  return out;
});

rule('R03-brief-versions', 'The brief documents carry the same version', (ctx) => {
  const names = ['01-master-brief.md', '02-appendices/00-index.md', '02-appendices.md', '03-reference-architecture.md'];
  const seen = [];
  for (const n of names) {
    const f = briefFile(ctx, n);
    if (!f) continue;
    const h1 = headings(f).find((h) => h.level === 1);
    // Major and minor: v9 and v9.1 are different versions of the brief.
    const m = h1 && /\bv(\d+(?:\.\d+)?)\b/.exec(h1.text);
    seen.push({ rel: f.rel, version: m ? m[1] : null, line: h1 ? h1.line : 1 });
  }
  const versions = new Set(seen.filter((s) => s.version).map((s) => s.version));
  const out = [];
  for (const s of seen) {
    if (!s.version) out.push(finding('R03-brief-versions', 'error', s.rel, s.line, 'Title carries no version (expected "vN")'));
  }
  if (versions.size > 1) {
    const all = [...versions].map((v) => 'v' + v).join(', ');
    for (const s of seen.filter((x) => x.version)) {
      out.push(finding('R03-brief-versions', 'error', s.rel, s.line, 'Version v' + s.version + ' disagrees with the other briefs (' + all + ')'));
    }
  }
  return out;
});

rule('R04-command-files', 'Every slash command named in CLAUDE.md has a file', (ctx) => {
  const c = ctx.byRel.get('CLAUDE.md');
  if (!c) return [];
  const out = [];
  const seen = new Set();
  for (const [ln, text] of proseLines(c)) {
    for (const m of text.matchAll(/`\/([a-z][a-z0-9-]*)`/g)) {
      if (seen.has(m[1])) continue;
      seen.add(m[1]);
      const rel = '.claude/commands/' + m[1] + '.md';
      if (!ctx.byRel.has(rel)) out.push(finding('R04-command-files', 'error', 'CLAUDE.md', ln, '/' + m[1] + ' has no ' + rel));
    }
  }
  return out;
});

rule('R05-no-open-todos', 'No TODO or TBD outside OPEN_QUESTIONS and IDEAS', (ctx) => {
  const allow = /docs\/project\/(OPEN_QUESTIONS|IDEAS|KIT_V9_CHANGES|KIT_LINT_BASELINE)\.md$|^tools\/|^docs\/templates\//;
  const out = [];
  for (const f of ctx.md) {
    if (allow.test(f.rel)) continue;
    for (const [ln, text] of proseLines(f)) {
      const m = /\b(TODO|TBD|FIXME|to be decided|to be determined)\b/i.exec(text);
      if (m) out.push(finding('R05-no-open-todos', 'error', f.rel, ln, '"' + m[1] + '" must move to OPEN_QUESTIONS.md with an owner and a default'));
    }
  }
  return out;
});

/**
 * Gateway and the BFFs own no database and publish no events, so they are
 * exempt from the event and entity catalogs by design, not by oversight.
 */
const EVENTLESS_SERVICES = new Set(['gateway', 'bff', 'bffweb', 'bffmobile']);

rule('R06-service-coverage', 'Every service sheet appears in the permission, event and dependency catalogs', (ctx) => {
  const ra = briefFile(ctx, '03-reference-architecture.md');
  if (!ra) return [];
  const services = [];
  for (const h of headings(ra)) {
    const m = /^8\.\d+\s+(.+?)\s*$/.exec(h.text);
    if (!m) continue;
    for (const part of m[1].split(/\s+and\s+/)) {
      const name = part.replace(/\(.*?\)/g, '').trim();
      if (!name || !/^[A-Z][A-Za-z]+$/.test(name)) continue;
      if (EVENTLESS_SERVICES.has(name.toLowerCase().replace(/[.\s]/g, ''))) continue;
      services.push({ name, line: h.line });
    }
  }
  if (!services.length) return [];
  const appText = appendixFiles(ctx).map((f) => f.text).join('\n');
  const out = [];
  for (const s of services) {
    const key = s.name.toLowerCase();
    if (appText && !new RegExp('\\b' + key + '\\.', 'i').test(appText)) {
      out.push(finding('R06-service-coverage', 'warn', ra.rel, s.line, 'Service ' + s.name + ' has no "' + key + '." permission or event entries in the appendices'));
    }
    if (!new RegExp('\\|\\s*\\*{0,2}' + s.name + '\\*{0,2}\\s*\\|', 'i').test(ra.text)) {
      out.push(finding('R06-service-coverage', 'warn', ra.rel, s.line, 'Service ' + s.name + ' is missing from the event dependency matrix'));
    }
  }
  return out;
});

rule('R07-events-catalogued', 'Every routing key used in a service sheet exists in the event catalog', (ctx) => {
  const ra = briefFile(ctx, '03-reference-architecture.md');
  const apps = appendixFiles(ctx);
  if (!ra || !apps.length) return [];
  const catalog = apps.map((f) => f.text).join('\n');
  if (!/routing key/i.test(catalog)) return [];
  const out = [];
  const seen = new Set();
  for (const [ln, text] of proseLines(ra)) {
    for (const m of text.matchAll(/`([a-z][a-z-]*)\.([a-z][a-z-]*)\.([a-z][a-z-]*)\.v\d+`/g)) {
      const key = m[0].replace(/`/g, '');
      if (seen.has(key)) continue;
      seen.add(key);
      if (!new RegExp(m[2] + '\\.' + m[3]).test(catalog)) {
        out.push(finding('R07-events-catalogued', 'error', ra.rel, ln, 'Event ' + key + ' is not in the event catalog'));
      }
    }
  }
  return out;
});

rule('R08-workflow-shape', 'Every workflow has a diagram, a test table and test case IDs', (ctx) => {
  const wf = ctx.md.find((f) => /appendix-r|workflow-catalog/i.test(f.rel));
  if (!wf) return [];
  const hs = headings(wf).filter((h) => /^WF-[A-Z]+-\d+/.test(h.text));
  const out = [];
  hs.forEach((h, i) => {
    const end = i + 1 < hs.length ? hs[i + 1].line : wf.lines.length + 1;
    const body = wf.lines.slice(h.line, end - 1).join('\n');
    const id = /^(WF-[A-Z]+-\d+)/.exec(h.text)[1];
    if (!/stateDiagram-v2/.test(body)) out.push(finding('R08-workflow-shape', 'error', wf.rel, h.line, id + ': no Mermaid stateDiagram-v2'));
    if (!/\|\s*Transition\s*\|/i.test(body)) out.push(finding('R08-workflow-shape', 'error', wf.rel, h.line, id + ': no test table with a Transition column'));
    if (!/TC-[A-Z]+-\d+/.test(body)) out.push(finding('R08-workflow-shape', 'error', wf.rel, h.line, id + ': no test case IDs'));
  });
  return out;
});

rule('R09-rule-examples', 'Every business rule has three worked examples and a test class', (ctx) => {
  const br = ctx.md.find((f) => /appendix-s|business-rules/i.test(f.rel));
  if (!br) return [];
  const hs = headings(br).filter((h) => /^BR-[A-Z]+-\d+/.test(h.text));
  const out = [];
  hs.forEach((h, i) => {
    const end = i + 1 < hs.length ? hs[i + 1].line : br.lines.length + 1;
    const body = br.lines.slice(h.line, end - 1).join('\n');
    const id = /^(BR-[A-Z]+-\d+)/.exec(h.text)[1];
    const given = (body.match(/\bGiven\b/g) || []).length;
    if (given < 3) out.push(finding('R09-rule-examples', 'error', br.rel, h.line, id + ': ' + given + ' worked examples, at least 3 required'));
    if (!/Tests\b/.test(body)) out.push(finding('R09-rule-examples', 'error', br.rel, h.line, id + ': no test class named'));
  });
  return out;
});

rule('R10-simulation-coverage', 'Every workflow and rule is exercised by the year-in-the-life simulation', (ctx) => {
  const sim = ctx.md.find((f) => /appendix-t|year-in-the-life/i.test(f.rel));
  if (!sim) return [];
  const ids = new Set();
  for (const f of ctx.md) {
    if (f === sim) continue;
    if (!/appendix-[rs]|workflow-catalog|business-rules/i.test(f.rel)) continue;
    for (const h of headings(f)) {
      const m = /^((?:WF|BR)-[A-Z]+-\d+)/.exec(h.text);
      if (m) ids.add(m[1]);
    }
  }
  const out = [];
  for (const id of [...ids].sort()) {
    if (!sim.text.includes(id)) out.push(finding('R10-simulation-coverage', 'error', sim.rel, 1, id + ' is never exercised in the simulation'));
  }
  return out;
});

rule('R11-feature-register', 'Every signature feature declares an assist rung, a tier and a moment', (ctx) => {
  const reg = ctx.md.find((f) => /appendix-w|feature-register/i.test(f.rel));
  if (!reg) return [];
  const out = [];
  for (const t of tables(reg)) {
    const head = t.header.map((h) => h.toLowerCase());
    if (!head.some((h) => h.includes('feature'))) continue;
    for (const col of ['rung', 'tier', 'moment']) {
      if (!head.some((h) => h.includes(col))) out.push(finding('R11-feature-register', 'error', reg.rel, t.line, 'Feature table is missing a "' + col + '" column'));
    }
    const rungIdx = head.findIndex((h) => h.includes('rung'));
    const tierIdx = head.findIndex((h) => h.includes('tier'));
    const nameIdx = head.findIndex((h) => h.includes('feature'));
    t.rows.forEach((r, i) => {
      const label = (r[nameIdx] || r[0] || '').replace(/\*/g, '') || 'row ' + (i + 1);
      if (rungIdx >= 0 && !/[1-4]/.test(r[rungIdx] || '')) out.push(finding('R11-feature-register', 'error', reg.rel, t.line + i + 2, '"' + label + '" has no assist rung'));
      if (tierIdx >= 0 && !/[1-3]/.test(r[tierIdx] || '')) out.push(finding('R11-feature-register', 'error', reg.rel, t.line + i + 2, '"' + label + '" has no tier'));
    });
  }
  return out;
});

rule('R12-notification-triggers', 'Every notification row names the event or job that triggers it', (ctx) => {
  const f = ctx.md.find((x) => /appendix-c|notification-matrix/i.test(x.rel));
  if (!f) return [];
  const out = [];
  for (const t of tables(f)) {
    const head = t.header.map((h) => h.toLowerCase());
    if (!head.some((h) => h.includes('recipient'))) continue;
    if (!head.some((h) => h.includes('trigger'))) {
      out.push(finding('R12-notification-triggers', 'error', f.rel, t.line, 'Notification table has no "Trigger" column'));
      continue;
    }
    const idx = head.findIndex((h) => h.includes('trigger'));
    t.rows.forEach((r, i) => {
      const cell = r[idx] || '';
      if (!/[a-z]+\.[a-z-]+\.[a-z-]+|job|schedule/i.test(cell)) {
        out.push(finding('R12-notification-triggers', 'error', f.rel, t.line + i + 2, 'Row "' + r[0] + '" has no event or scheduled job as its trigger'));
      }
    });
  }
  return out;
});

rule('R13-case-collisions', 'No two paths differ only by case', (ctx) => {
  const seen = new Map();
  const out = [];
  for (const f of ctx.files) {
    const key = f.rel.toLowerCase();
    if (seen.has(key) && seen.get(key) !== f.rel) {
      out.push(finding('R13-case-collisions', 'error', f.rel, 1, 'Collides with ' + seen.get(key) + ' when case is ignored'));
    }
    seen.set(key, f.rel);
  }
  return out;
});

rule('R14-path-length', 'Repository-relative paths stay under 200 characters', (ctx) => {
  const out = [];
  for (const f of ctx.files) {
    if (f.rel.length > 200) out.push(finding('R14-path-length', 'error', f.rel, 1, 'Path is ' + f.rel.length + ' characters; Windows checkouts need under 200'));
  }
  return out;
});

rule('R15-script-wrappers', 'Every tool entry point ships a .ps1 and a .sh wrapper', (ctx) => {
  const out = [];
  for (const f of ctx.files) {
    if (!/^tools\/.*\.mjs$/.test(f.rel)) continue;
    if (!/^#!/.test(f.text)) continue;
    const base = f.rel.replace(/\.mjs$/, '');
    for (const ext of ['.ps1', '.sh']) {
      if (!ctx.byRel.has(base + ext)) out.push(finding('R15-script-wrappers', 'error', f.rel, 1, 'Missing ' + base + ext));
    }
  }
  return out;
});

rule('R16-portable-hooks', 'Hooks stay portable across Windows and Linux', (ctx) => {
  const f = ctx.byRel.get('.claude/settings.json');
  if (!f) return [];
  let json;
  try {
    json = JSON.parse(f.text);
  } catch (e) {
    return [finding('R16-portable-hooks', 'error', f.rel, 1, 'Invalid JSON: ' + e.message)];
  }
  const out = [];
  const visit = (node) => {
    if (!node || typeof node !== 'object') return;
    if (Array.isArray(node)) {
      node.forEach(visit);
      return;
    }
    if (typeof node.command === 'string') {
      const cmd = node.command.trim();
      if (!/^node\s/.test(cmd)) {
        out.push(finding('R16-portable-hooks', 'error', f.rel, 1, 'Hook command "' + cmd.slice(0, 60) + '" must invoke node so it runs on Windows and Linux'));
      } else if (/^([A-Za-z]:\\|\/)/.test(cmd.replace(/^node\s+/, ''))) {
        out.push(finding('R16-portable-hooks', 'error', f.rel, 1, 'Hook command uses an absolute path'));
      }
    }
    Object.values(node).forEach(visit);
  };
  visit(json.hooks);
  return out;
});

rule('R17-mermaid-blocks', 'Every Mermaid block declares a known diagram type', (ctx) => {
  const known = /^(graph|flowchart|sequenceDiagram|stateDiagram(-v2)?|erDiagram|classDiagram|journey|gantt|pie|mindmap|timeline|C4Context|C4Container|gitGraph|quadrantChart)\b/;
  const out = [];
  for (const f of ctx.md) {
    for (const b of fencedBlocks(f, 'mermaid')) {
      const first = (b.body.find((l) => l.trim() && !l.trim().startsWith('%%')) || '').trim();
      if (!known.test(first)) {
        out.push(finding('R17-mermaid-blocks', 'error', f.rel, b.start, 'Mermaid block starts with "' + first.slice(0, 40) + '" which is not a known diagram type'));
      }
    }
  }
  return out;
});

rule('R18-tree-comments', 'Directory trees explain every entry', (ctx) => {
  const out = [];
  for (const f of ctx.md) {
    if (!/^docs\/(brief|plan)\//.test(f.rel)) continue;
    for (const b of fencedBlocks(f)) {
      if (b.lang && b.lang !== 'text') continue;
      const treeLines = b.body.filter((l) => /[\u251c\u2514]\u2500\u2500/.test(l));
      if (treeLines.length < 4) continue;
      const bare = treeLines.filter((l) => {
        const after = l.split(/[\u251c\u2514]\u2500\u2500\s*/)[1] || '';
        return after.trim() && !/\S\s{2,}\S/.test(after) && !after.includes('#');
      });
      // Plan trees are specifications, so every entry explains itself; brief trees are
      // illustrations and only warn when most entries are bare.
      if (f.rel.startsWith('docs/plan/') && bare.length) {
        const first = b.body.indexOf(bare[0]);
        out.push(finding('R18-tree-comments', 'error', f.rel, b.start + first + 1, bare.length + ' of ' + treeLines.length + ' tree entries have no purpose comment, the first: ' + bare[0].trim().slice(0, 60)));
      } else if (bare.length > treeLines.length * 0.5) {
        out.push(finding('R18-tree-comments', 'warn', f.rel, b.start, bare.length + ' of ' + treeLines.length + ' tree entries have no purpose comment'));
      }
    }
  }
  return out;
});

/**
 * R19: every identifier a plan document cites exists in the catalog that owns it.
 *
 * Plan documents are written by many hands in parallel; this is the rule that
 * stops one of them citing an event, a requirement or a permission that no
 * catalog defines. A missing identifier may be quoted deliberately, so lines
 * under an "Open points" heading, and lines that say an identifier is missing,
 * are exempt: that is where a defect in a catalog is reported, not hidden.
 */
const PLAN_SKIP = /^docs\/plan\/(PLAN_SPEC|README|REVIEW_GUIDE|WORKFLOWS)\.md$/;
const DISCUSSES_ABSENCE = /\b(not in|missing from|absent from|lacks|lack|proposed|proposal|propose|does not exist|do not exist|not an Appendix|not catalogued|no such|would need|needs an ADR|to be added|add to Appendix)\b/i;
const SERVICE_PREFIXES = ['IDENTITY', 'PLATFORM', 'SCHOOL', 'ADMISSIONS', 'ACADEMICS', 'ASSESSMENT', 'SCHEDULING', 'ATTENDANCE', 'FINANCE', 'COMMUNICATION', 'NOTIFICATION', 'REQUESTS', 'DOCUMENTS', 'BEHAVIOR', 'REPORTING', 'AUDIT', 'WELLBEING', 'HR', 'OPERATIONS', 'AI', 'GATEWAY', 'BFF'];
const SERVICE_NAMESPACES = new Set(SERVICE_PREFIXES.map((s) => s.toLowerCase()));

export function buildCatalogs(ctx) {
  const byName = (re) => ctx.md.find((f) => re.test(f.rel));
  const keyRe = /`([a-z][a-z0-9-]*(?:\.[a-z0-9-]+){2,}\.v\d+)`/g;
  const collectKeys = (f, into) => { if (f) for (const m of f.text.matchAll(keyRe)) into.add(m[1]); };
  const events = new Set();
  collectKeys(byName(/appendix-e-event-catalog/), events);
  const c11 = byName(/^docs\/plan\/11-messaging-architecture\.md$/);
  collectKeys(c11, events);
  // Document 11 defines commands and replies by pattern plus a list of names,
  // e.g. `<target>.commands.<command-kebab>.v<n>` and `BookMeeting`. A derived
  // key is valid only when its name appears in document 11 in PascalCase.
  const messageNames = new Set();
  if (c11) for (const m of c11.text.matchAll(/`([A-Z][A-Za-z0-9]+)`/g)) messageNames.add(m[1]);

  const reqs = new Set();
  const c03 = byName(/^docs\/plan\/03-requirements-catalog\.md$/);
  if (c03) for (const m of c03.text.matchAll(/^\| (REQ-[A-Z0-9]+-\d{3}) \|/gm)) reqs.add(m[1]);

  const headIds = (re, pat) => {
    const out = new Set();
    const f = byName(re);
    if (f) for (const h of headings(f)) { const m = pat.exec(h.text); if (m) out.add(m[1]); }
    return out;
  };
  const wfs = headIds(/appendix-r-workflow-catalog/, /^(WF-[A-Z]+-\d{2})\b/);
  const brs = headIds(/appendix-s-business-rules/, /^(BR-[A-Z0-9]+-\d{3})\b/);

  const caps = new Set();
  const c17 = byName(/^docs\/plan\/17-roadmap\.md$/);
  if (c17) for (const m of c17.text.matchAll(/^\| (CAP-[A-Z0-9]+-\d{2}) \|/gm)) caps.add(m[1]);

  const codes = new Set();
  const suffixes = new Set();
  const k = byName(/appendix-k-error-codes/);
  if (k) for (const m of k.text.matchAll(/^\| `([A-Z_][A-Z0-9_]*)` \|/gm)) {
    if (m[1].startsWith('_')) suffixes.add(m[1]); else codes.add(m[1]);
  }

  const perms = new Set();
  const b = byName(/appendix-b-permissions/);
  if (b) for (const t of tables(b)) {
    for (const r of t.rows) {
      const res = /^`([a-z][a-z0-9.-]+)`$/.exec(r[0] || '');
      if (!res) continue;
      for (const a of (r[1] || '').split(',').map((x) => x.trim()).filter((x) => /^[a-z-]+$/.test(x))) perms.add(res[1] + '.' + a);
      for (const m of (r[2] || '').matchAll(/`([a-z-]+)`/g)) perms.add(res[1] + '.' + m[1]);
    }
  }
  return { events, messageNames, reqs, wfs, brs, caps, codes, suffixes, perms, ready: { events: events.size > 0, reqs: reqs.size > 0, wfs: wfs.size > 0, brs: brs.size > 0, caps: caps.size > 0, codes: codes.size > 0, perms: perms.size > 0 } };
}

rule('R19-plan-identifiers', 'Every identifier a plan document cites exists in the catalog that owns it', (ctx) => {
  const cat = buildCatalogs(ctx);
  const out = [];
  const report = (f, ln, what, id) => out.push(finding('R19-plan-identifiers', 'error', f.rel, ln, what + ' ' + id + ' is not in its catalog'));
  for (const f of ctx.md) {
    if (!f.rel.startsWith('docs/plan/') || PLAN_SKIP.test(f.rel)) continue;
    let section = '';
    let permCols = null;
    let fenced = false;
    f.lines.forEach((line, i) => {
      const ln = i + 1;
      if (/^\s*```/.test(line)) { fenced = !fenced; }
      const h2 = /^##\s+(.*)$/.exec(line);
      if (h2 && !fenced) section = h2[1].trim();
      if (/^open points/i.test(section)) return;
      if (DISCUSSES_ABSENCE.test(line)) return;

      // Track which columns of the current table are permission columns.
      if (/^\s*\|.*\|\s*$/.test(line)) {
        const cells = line.trim().replace(/^\||\|$/g, '').split('|').map((c) => c.trim());
        if (permCols === null) permCols = cells.map((c, idx) => (/permission/i.test(c) ? idx : -1)).filter((x) => x >= 0);
        else if (!/^:?-{2,}/.test(cells[0] || '') && permCols.length && cat.ready.perms) {
          for (const idx of permCols) {
            for (const m of (cells[idx] || '').matchAll(/`([a-z][a-z0-9-]*(?:\.[a-z0-9-]+){2,})`/g)) {
              const p = m[1];
              if (/\.v\d+$/.test(p) || !SERVICE_NAMESPACES.has(p.split('.')[0])) continue;
              if (!cat.perms.has(p)) report(f, ln, 'Permission', p);
            }
          }
        }
      } else {
        permCols = null;
      }

      if (cat.ready.events) {
        for (const m of line.matchAll(/`([a-z][a-z0-9-]*(?:\.[a-z0-9-]+){2,}\.v\d+)`/g)) {
          const key = m[1];
          if (!SERVICE_NAMESPACES.has(key.split('.')[0])) continue;
          if (/\.(usage|audit)\.recorded\.v\d+$/.test(key) && key.split('.').length === 4) continue;
          if (cat.events.has(key)) continue;
          const derived = /^[a-z][a-z0-9-]*\.(commands|replies)\.([a-z0-9-]+)\.v\d+$/.exec(key);
          if (derived) {
            const pascal = derived[2].split('-').map((w) => w.charAt(0).toUpperCase() + w.slice(1)).join('');
            if (cat.messageNames.has(pascal)) continue;
          }
          report(f, ln, 'Routing key', key);
        }
      }
      const idCheck = (re, set, ready, what) => {
        if (!ready) return;
        for (const m of line.matchAll(re)) if (!set.has(m[1])) report(f, ln, what, m[1]);
      };
      idCheck(/\b(REQ-[A-Z0-9]+-\d{3})\b/g, cat.reqs, cat.ready.reqs, 'Requirement');
      idCheck(/\b(WF-[A-Z]+-\d{2})\b/g, cat.wfs, cat.ready.wfs, 'Workflow');
      idCheck(/\b(BR-[A-Z0-9]+-\d{3})\b/g, cat.brs, cat.ready.brs, 'Business rule');
      idCheck(/\b(CAP-[A-Z0-9]+-\d{2})\b/g, cat.caps, cat.ready.caps, 'Capability');
      if (cat.ready.codes) {
        for (const m of line.matchAll(/`([A-Z][A-Z0-9]*_[A-Z0-9_]+)`/g)) {
          const code = m[1];
          const prefix = SERVICE_PREFIXES.find((p) => code.startsWith(p + '_'));
          if (!prefix) continue;
          if (cat.codes.has(code) || cat.suffixes.has(code.slice(prefix.length))) continue;
          report(f, ln, 'Error code', code);
        }
      }
    });
  }
  return out;
});

/* ---------------------------------------------------------------- R20 */

/**
 * Test-case identifiers (ADR-0014) are proof, so each one must mean one thing.
 * A test is *defined* where a table cell holds that identifier and nothing else,
 * in the first column or in a column headed as a test (Appendix W's "Demo"
 * column included). Everywhere else the identifier is *cited*. A cell that
 * names an owner, such as "`TC-ATT-003` (Appendix R)", is a citation by design.
 *
 * Errors: a test defined in more than one document; a test cited but defined
 * nowhere; a derived acceptance test (950 to 999, document 20) whose
 * requirement does not exist. Documents that only report, review or generate
 * (documents 02, 03, 20, 30, 34, the registry annex of 16, Appendices O, P, Q,
 * V, and docs/project) never define, and the stale citations in docs/project
 * and document 30 are history, not defects.
 */
const TC_RE = /TC-[A-Z0-9]+-\d{3}/g;
const TC_LONE = /^`?(TC-[A-Z0-9]+-\d{3})`?$/;
const TC_TEST_HEADER = /^(test case|test|tc|identifier|test case id|test id|demo)$/i;
const TC_CITE_ONLY = /^docs\/(project\/|plan\/(02|03|20|30|34)-|plan\/16-annex-|brief\/02-appendices\/appendix-[opqv]-)/;
const TC_HISTORY = /^docs\/(project\/|plan\/30-)/;
const TC_SCOPE = /^docs\/(brief|plan|project)\//;

export function testCaseOwnership(ctx) {
  const defs = new Map();
  const cites = new Map();
  const add = (m, id, x) => { if (!m.has(id)) m.set(id, []); m.get(id).push(x); };
  for (const f of ctx.md) {
    if (!TC_SCOPE.test(f.rel)) continue;
    const citeOnly = TC_CITE_ONLY.test(f.rel);
    let header = null;
    let fenced = false;
    let section = '';
    f.lines.forEach((line, i) => {
      if (/^\s*```/.test(line)) { fenced = !fenced; return; }
      // A heading that opens with an identifier, "### TC-DATA-001 A pooled ...", defines that test,
      // including inside a fenced Given, When, Then block, which is how some documents write one out.
      const head = /^#{2,6}\s+`?(TC-[A-Z0-9]+-\d{3})`?\s+(.*)$/.exec(line);
      if (head && !citeOnly) { header = null; add(defs, head[1], { file: f.rel, line: i + 1, section, exempt: false, text: head[2].trim() }); return; }
      if (fenced) return;
      const h2 = /^##\s+(.*)$/.exec(line);
      if (h2) section = h2[1].trim();
      // Only the Open points and Review record sections are exempt: a test's own assertion often says
      // something is absent, so the phrasing exemption of R19 would hide real citations here.
      const where = { file: f.rel, line: i + 1, section, exempt: /^(open points|review record)/i.test(section) };
      if (!/^\s*\|.*\|\s*$/.test(line)) {
        header = null;
        for (const m of line.match(TC_RE) || []) add(cites, m, { ...where, text: line.trim().slice(0, 200) });
        return;
      }
      const cells = line.trim().replace(/^\|/, '').replace(/\|$/, '').split('|').map((c) => c.trim());
      if (/^:?-{2,}:?$/.test(cells[0] || '')) return;
      const next = f.lines[i + 1] || '';
      if (header === null && /^\s*\|\s*:?-{2,}/.test(next)) { header = cells; return; }
      cells.forEach((cell, ci) => {
        const ids = cell.match(TC_RE) || [];
        if (!ids.length) return;
        const lone = TC_LONE.exec(cell);
        const defining = !citeOnly && lone && (ci === 0 || TC_TEST_HEADER.test(((header && header[ci]) || '').replace(/`/g, '')));
        const text = cells.filter((_, k) => k !== ci).join(' | ').replace(/\s+/g, ' ').trim();
        for (const id of ids) add(defining ? defs : cites, id, { ...where, text });
      });
    });
  }
  const derivedReq = (id) => {
    const [, area, num] = id.split('-');
    const n = Number(num);
    return n >= 950 && n <= 999 ? 'REQ-' + area + '-' + String(n - 950).padStart(3, '0') : null;
  };
  const collisions = [...defs].filter(([, v]) => new Set(v.map((x) => x.file)).size > 1).map(([id, v]) => ({ id, defs: v }));
  const undefinedIds = [...cites.keys()].filter((id) => !defs.has(id) && !derivedReq(id));
  return { defs, cites, collisions, undefinedIds, derivedReq };
}

rule('R20-test-case-identifiers', 'Every test-case identifier is defined in exactly one document', (ctx) => {
  const out = [];
  const own = testCaseOwnership(ctx);
  const reqs = buildCatalogs(ctx).reqs;
  for (const c of own.collisions) {
    const files = [...new Set(c.defs.map((d) => d.file))];
    const first = c.defs[0];
    out.push(finding('R20-test-case-identifiers', 'error', first.file, first.line, c.id + ' is defined in ' + files.length + ' documents: ' + files.join(', ') + '. Keep one definition and cite it elsewhere, or renumber one'));
  }
  for (const id of own.undefinedIds) {
    const live = own.cites.get(id).filter((c) => !c.exempt && !TC_HISTORY.test(c.file));
    if (live.length) out.push(finding('R20-test-case-identifiers', 'error', live[0].file, live[0].line, id + ' is cited but defined in no document'));
  }
  if (reqs.size) {
    const seen = new Set();
    for (const [id, list] of [...own.cites, ...own.defs]) {
      const req = own.derivedReq(id);
      if (!req || reqs.has(req) || seen.has(id)) continue;
      seen.add(id);
      const live = list.filter((c) => !c.exempt && !TC_HISTORY.test(c.file));
      if (live.length) out.push(finding('R20-test-case-identifiers', 'error', live[0].file, live[0].line, id + ' is a derived acceptance test, but ' + req + ' does not exist'));
    }
  }
  return out;
});

/* ------------------------------------------------------------ R21 to R32 */

/*
 * Plan-integrity rules (scorecard theme 6). Each one exists because a plan
 * document's "How this document is verified" table named a check that nobody
 * ran. They read the plan's own tables, so a rule is silent on a kit that does
 * not contain the document it checks.
 */
const planDoc = (ctx, num) => ctx.md.find((f) => f.rel.startsWith('docs/plan/' + num + '-'));
const splitCells = (line) => line.trim().replace(/^\|/, '').replace(/\|$/, '').split('|').map((c) => c.trim());

/** Tables outside fences, with the line number of every row. */
export function linedTables(file) {
  const out = [];
  let cur = null;
  let fenced = false;
  file.lines.forEach((raw, i) => {
    if (/^\s*```/.test(raw)) { fenced = !fenced; if (cur) { out.push(cur); cur = null; } return; }
    if (fenced) return;
    if (!/^\s*\|.*\|\s*$/.test(raw)) { if (cur) { out.push(cur); cur = null; } return; }
    const cells = splitCells(raw);
    if (!cur) { cur = { header: cells, line: i + 1, rows: [] }; return; }
    if (/^:?-{2,}:?$/.test(cells[0] || '')) return;
    cur.rows.push({ cells, line: i + 1 });
  });
  if (cur) out.push(cur);
  return out;
}
const col = (t, re) => t.header.findIndex((h) => re.test(h.replace(/[`*]/g, '').trim()));

/** Sections of a file by heading text: returns the lines between a heading matching `re` and the next heading of the same or higher level. */
function sectionLines(file, re) {
  const hs = headings(file);
  const h = hs.find((x) => re.test(x.text));
  if (!h) return null;
  const next = hs.find((x) => x.line > h.line && x.level <= h.level);
  return { start: h.line, lines: file.lines.slice(h.line, next ? next.line - 1 : file.lines.length) };
}

rule('R21-requirements-catalog', 'The requirements catalog is well formed', (ctx) => {
  const f = planDoc(ctx, '03');
  if (!f) return [];
  const cat = buildCatalogs(ctx);
  const out = [];
  const seen = new Set();
  const last = new Map();
  f.lines.forEach((line, i) => {
    if (!line.startsWith('| REQ-')) return;
    const ln = i + 1;
    const c = splitCells(line);
    const bad = (msg) => out.push(finding('R21-requirements-catalog', 'error', f.rel, ln, msg));
    if (c.length !== 7) bad(c[0] + ' has ' + c.length + ' columns; the catalog has 7');
    const m = /^REQ-([A-Z0-9]+)-(\d{3})$/.exec(c[0]);
    if (!m) { bad('Malformed requirement identifier ' + c[0]); return; }
    if (seen.has(c[0])) bad(c[0] + ' appears twice');
    seen.add(c[0]);
    const n = Number(m[2]);
    const prev = last.get(m[1]) || 0;
    if (n !== prev + 1) bad(c[0] + ' follows ' + (prev ? 'REQ-' + m[1] + '-' + String(prev).padStart(3, '0') : 'nothing') + '; numbers run without gaps per area');
    last.set(m[1], n);
    if (!/^[123]$/.test((c[2] || '').replace(/\D/g, ''))) bad(c[0] + ' has no tier of 1, 2 or 3');
    if (!(c[6] || '').trim()) bad(c[0] + ' has no acceptance criterion');
    if (cat.ready.wfs) for (const w of (c[5] || '').match(/WF-[A-Z]+-\d{2}/g) || []) if (!cat.wfs.has(w)) bad(c[0] + ' cites ' + w + ', which Appendix R does not define');
    if (cat.ready.brs) for (const b of (c[5] || '').match(/BR-[A-Z0-9]+-\d{3}/g) || []) if (!cat.brs.has(b)) bad(c[0] + ' cites ' + b + ', which Appendix S does not define');
  });
  return out;
});

rule('R22-adr-references', 'Every ADR cited exists, and document 29 indexes every record once with its status', (ctx) => {
  const records = new Map();
  for (const f of ctx.md) {
    const m = /^docs\/project\/DECISIONS\/(\d{4})-.*\.md$/.exec(f.rel);
    if (!m || m[1] === '0000') continue;
    const status = (/^- \*\*Status:\*\*\s*(\w+)/m.exec(f.text) || [])[1] || '';
    records.set(m[1], { file: f, status });
  }
  if (!records.size) return [];
  const out = [];
  for (const f of ctx.md) {
    if (!/^(docs\/|CLAUDE\.md$|README\.md$)/.test(f.rel) || /^docs\/project\/(CHANGELOG|KIT_V9)/.test(f.rel)) continue;
    f.lines.forEach((line, i) => {
      for (const m of line.matchAll(/\bADR[- ](\d{4})\b/g)) {
        if (m[1] !== '0000' && !records.has(m[1]) && !DISCUSSES_ABSENCE.test(line)) out.push(finding('R22-adr-references', 'error', f.rel, i + 1, 'ADR-' + m[1] + ' is cited but docs/project/DECISIONS has no record ' + m[1]));
      }
    });
  }
  const idx = planDoc(ctx, '29');
  if (!idx) return out;
  const indexed = new Map();
  for (const t of linedTables(idx)) {
    if (!/^ADR$/i.test(t.header[0] || '')) continue;
    const sc = col(t, /^status$/i);
    for (const r of t.rows) {
      if (!/^\d{4}$/.test(r.cells[0])) continue;
      if (r.cells.length !== t.header.length) out.push(finding('R22-adr-references', 'error', idx.rel, r.line, 'ADR ' + r.cells[0] + ' row has ' + r.cells.length + ' cells; the index has ' + t.header.length + ' columns'));
      if (indexed.has(r.cells[0])) out.push(finding('R22-adr-references', 'error', idx.rel, r.line, 'ADR ' + r.cells[0] + ' is indexed twice'));
      indexed.set(r.cells[0], { line: r.line, status: sc >= 0 ? r.cells[sc] || '' : '' });
    }
  }
  if (!indexed.size) return out;
  for (const [n, rec] of records) {
    const row = indexed.get(n);
    if (!row) { out.push(finding('R22-adr-references', 'error', idx.rel, 1, 'ADR ' + n + ' (' + rec.file.rel + ') has no row in the index')); continue; }
    if (rec.status && !row.status.toLowerCase().startsWith(rec.status.toLowerCase())) out.push(finding('R22-adr-references', 'error', idx.rel, row.line, 'ADR ' + n + ' is "' + row.status + '" here but "' + rec.status + '" in its record'));
  }
  for (const [n, row] of indexed) if (!records.has(n)) out.push(finding('R22-adr-references', 'error', idx.rel, row.line, 'ADR ' + n + ' is indexed but has no record in docs/project/DECISIONS'));
  return out;
});

/*
 * R23 runs the plan generators in --check mode. They compare what they would
 * write with what is on disk, ignoring review-record dates and line endings.
 * It is the slow rule, so the post-edit hook skips it; /lint-plan runs it.
 */
const GENERATORS = [
  ['tools/plan-build/gen-31.mjs', 'docs/plan/31-business-rules-and-workflows.md'],
  ['tools/plan-build/assemble-34.mjs', 'docs/plan/34-work-breakdown.md', '--write'],
  ['tools/plan-build/gen-20.mjs', 'docs/plan/20-traceability-matrix.md'],
  ['tools/plan-build/gen-tc-registry.mjs', 'docs/plan/16-annex-test-case-registry.md'],
  ['tools/plan-build/build-30.cjs', 'docs/plan/30-plan-scorecard.md'],
  ['tools/plan-build/schedule-34.mjs', 'docs/plan/17-roadmap.md'],
];
rule('R23-generated-documents', 'Every generated plan document matches what its generator produces today', (ctx) => {
  const out = [];
  for (const [script, doc, extra] of GENERATORS) {
    if (!ctx.byRel.has(script) || !ctx.byRel.has(doc)) continue;
    const args = [join(ctx.root, script), ...(extra ? [extra] : []), '--check'];
    const r = spawnSync(process.execPath, args, { cwd: ctx.root, encoding: 'utf8' });
    if (r.status !== 0) {
      const why = ((r.stdout || '') + (r.stderr || '')).trim().split(/\r?\n/).filter(Boolean).slice(-1)[0] || 'exit ' + r.status;
      out.push(finding('R23-generated-documents', 'error', doc, 1, 'Stale or failing: ' + script + ' --check says "' + why.slice(0, 200) + '"'));
    }
  }
  return out;
});

rule('R24-risk-tables', 'Every risk row scores likelihood times impact on the 1 to 5 scale, and every register reference exists', (ctx) => {
  const reg = planDoc(ctx, '18');
  const riskIds = new Set();
  if (reg) for (const m of reg.text.matchAll(/^\| (RISK-\d{2}) \|/gm)) riskIds.add(m[1]);
  const out = [];
  for (const f of ctx.md) {
    if (!f.rel.startsWith('docs/plan/') || PLAN_SKIP.test(f.rel)) continue;
    for (const t of linedTables(f)) {
      const li = col(t, /^(L|Likelihood)$/);
      const ii = col(t, /^(I|Impact)$/);
      if (li < 0 || ii < 0) continue;
      const si = col(t, /^Score$/i);
      const ri = col(t, /^In the register$/i);
      for (const r of t.rows) {
        const L = r.cells[li];
        const I = r.cells[ii];
        if (!/^\d$/.test(L) || !/^\d$/.test(I)) continue; // word-scaled tables (document 12) are not register rows
        const bad = (msg) => out.push(finding('R24-risk-tables', 'error', f.rel, r.line, (r.cells[0] || '').slice(0, 40) + ': ' + msg));
        if (+L < 1 || +L > 5 || +I < 1 || +I > 5) bad('likelihood and impact run from 1 to 5');
        if (si >= 0 && Number(r.cells[si]) !== +L * +I) bad('score ' + r.cells[si] + ' is not ' + L + ' x ' + I + ' = ' + (+L * +I));
        if (ri >= 0 && riskIds.size) for (const id of r.cells[ri].match(/RISK-\d{2}/g) || []) if (!riskIds.has(id)) bad(id + ' is not in document 18');
      }
    }
  }
  return out;
});

rule('R25-roadmap-coverage', 'Every workflow is assigned once and built by a capability, and every capability has slices', (ctx) => {
  const cat = buildCatalogs(ctx);
  const out = [];
  const d13 = planDoc(ctx, '13');
  if (d13 && cat.ready.wfs) {
    const assigned = new Map();
    const sec = sectionLines(d13, /^1\.\s+Workflow assignment/i);
    if (sec) sec.lines.forEach((l, k) => { const m = /^\| (WF-[A-Z]+-\d{2}) \|/.exec(l); if (m) { if (assigned.has(m[1])) out.push(finding('R25-roadmap-coverage', 'error', d13.rel, sec.start + k + 1, m[1] + ' is assigned twice')); assigned.set(m[1], true); } });
    if (sec) for (const w of cat.wfs) if (!assigned.has(w)) out.push(finding('R25-roadmap-coverage', 'error', d13.rel, sec.start, w + ' has no row in the workflow assignment table'));
  }
  const d17 = planDoc(ctx, '17');
  if (d17 && cat.ready.wfs) {
    const inCaps = new Set();
    for (const l of d17.lines) if (/^\| CAP-[A-Z0-9]+-\d{2} \|/.test(l)) for (const m of l.matchAll(/WF-[A-Z]+-\d{2}/g)) inCaps.add(m[0]);
    for (const w of cat.wfs) if (!inCaps.has(w)) out.push(finding('R25-roadmap-coverage', 'error', d17.rel, 1, w + ' is built by no capability in Section 4'));
  }
  const d34 = planDoc(ctx, '34');
  if (d34 && cat.ready.caps) {
    const sliced = new Set();
    let cap = null;
    for (const l of d34.lines) { const h = /^#### (CAP-[A-Z0-9]+-\d{2})\b/.exec(l); if (h) cap = h[1]; else if (/^#{1,4} /.test(l)) cap = null; if (cap && /^\| SL-/.test(l)) sliced.add(cap); }
    for (const c of cat.caps) if (!sliced.has(c)) out.push(finding('R25-roadmap-coverage', 'error', d34.rel, 1, c + ' has no slices'));
  }
  return out;
});

rule('R26-open-questions-mirror', 'Document 01 and OPEN_QUESTIONS.md carry the same open questions', (ctx) => {
  const oq = ctx.byRel.get('docs/project/OPEN_QUESTIONS.md');
  const d01 = planDoc(ctx, '01');
  if (!oq || !d01) return [];
  const nums = (f, stop) => {
    const s = new Set();
    for (const l of f.lines) { if (stop && stop.test(l)) break; const m = /^\| (\d{1,3}) \| [^|]*\?/.exec(l); if (m) s.add(m[1]); }
    return s;
  };
  const a = nums(oq, /^## Settled/);
  const b = nums(d01);
  const out = [];
  for (const n of a) if (!b.has(n)) out.push(finding('R26-open-questions-mirror', 'error', d01.rel, 1, 'Open question ' + n + ' is in OPEN_QUESTIONS.md but not in document 01'));
  for (const n of b) if (!a.has(n)) out.push(finding('R26-open-questions-mirror', 'error', oq.rel, 1, 'Question ' + n + ' is in document 01 but not open in OPEN_QUESTIONS.md'));
  return out;
});

rule('R27-messaging-keys', 'Document 11 publishes only catalogued keys, and every service publishes on its own exchange', (ctx) => {
  const out = [];
  const e = ctx.md.find((f) => /appendix-e-event-catalog/.test(f.rel));
  const d11 = planDoc(ctx, '11');
  if (e && d11) {
    const inE = new Set([...e.text.matchAll(/`([a-z][a-z0-9-]*(?:\.[a-z0-9-]+){2,}\.v\d+)`/g)].map((m) => m[1]));
    const names = new Set([...d11.text.matchAll(/`([A-Z][A-Za-z0-9]+)`/g)].map((m) => m[1]));
    d11.lines.forEach((line, i) => {
      if (DISCUSSES_ABSENCE.test(line)) return;
      for (const m of line.matchAll(/`([a-z][a-z0-9-]*(?:\.[a-z0-9-]+){2,}\.v\d+)`/g)) {
        const key = m[1];
        if (!SERVICE_NAMESPACES.has(key.split('.')[0]) || inE.has(key)) continue;
        if (/\.(usage|audit)\.recorded\.v\d+$/.test(key) && key.split('.').length === 4) continue;
        const d = /^[a-z][a-z0-9-]*\.(commands|replies)\.([a-z0-9-]+)\.v\d+$/.exec(key);
        if (d && names.has(d[2].split('-').map((w) => w[0].toUpperCase() + w.slice(1)).join(''))) continue;
        out.push(finding('R27-messaging-keys', 'error', d11.rel, i + 1, key + ' is used in document 11 but is not in Appendix E and matches no command or reply document 11 names'));
      }
    });
  }
  const d05 = planDoc(ctx, '05');
  if (d05) {
    for (const t of linedTables(d05)) {
      const pi = col(t, /^Publishes$/i);
      if (pi < 0 || !/^Service$/i.test(t.header[0])) continue;
      for (const r of t.rows) {
        const svc = r.cells[0].replace(/[*`]/g, '').trim().toLowerCase();
        if (!SERVICE_NAMESPACES.has(svc)) continue;
        for (const m of r.cells[pi].matchAll(/`([a-z][a-z0-9-]*(?:\.[a-z0-9-]+){2,}\.v\d+)`/g)) {
          if (m[1].split('.')[0] !== svc) out.push(finding('R27-messaging-keys', 'error', d05.rel, r.line, r.cells[0].replace(/[*`]/g, '') + ' lists ' + m[1] + ' as published, but a service publishes only on its own exchange'));
        }
      }
    }
  }
  return out;
});

rule('R28-threat-coverage', 'Every high or critical threat names the test that proves its control', (ctx) => {
  const d12 = planDoc(ctx, '12');
  if (!d12) return [];
  const out = [];
  for (const t of linedTables(d12)) {
    const ii = col(t, /^Impact$/i);
    const ti = col(t, /^Test$/i);
    if (ii < 0 || ti < 0) continue;
    for (const r of t.rows) {
      if (!/^(high|critical)$/i.test(r.cells[ii] || '')) continue;
      if (!/TC-[A-Z0-9]+-\d{3}|SL-[A-Z0-9]+-\d{3}/.test(r.cells[ti] || '')) out.push(finding('R28-threat-coverage', 'error', d12.rel, r.line, (r.cells[0] || '') + ' has impact ' + r.cells[ii] + ' but its Test cell names no test case or slice'));
    }
  }
  return out;
});

rule('R29-state-diagrams', 'Every workflow state diagram has an exit and a label on every transition', (ctx) => {
  const out = [];
  for (const f of ctx.md) {
    if (!/appendix-r-workflow-catalog|^docs\/plan\/13-/.test(f.rel)) continue;
    for (const b of fencedBlocks(f, 'mermaid')) {
      if (!/^\s*stateDiagram-v2/.test(b.body.find((l) => l.trim()) || '')) continue;
      if (!b.body.some((l) => /-->\s*\[\*\]/.test(l))) out.push(finding('R29-state-diagrams', 'error', f.rel, b.start, 'State diagram has no terminal state (--> [*])'));
      b.body.forEach((l, k) => {
        if (!/-->/.test(l) || /^\s*\[\*\]\s*-->/.test(l) || /-->\s*\[\*\]/.test(l)) return;
        if (!/-->\s*[^:]+:\s*\S/.test(l)) out.push(finding('R29-state-diagrams', 'error', f.rel, b.start + k + 1, 'Transition has no label: ' + l.trim().slice(0, 80)));
      });
    }
  }
  return out;
});

rule('R30-sql-comments', 'Every column in a plan CREATE TABLE explains itself in a comment', (ctx) => {
  const out = [];
  for (const f of ctx.md) {
    if (!f.rel.startsWith('docs/plan/')) continue;
    for (const b of fencedBlocks(f, 'sql')) {
      let inTable = false;
      b.body.forEach((l, k) => {
        if (/create\s+table/i.test(l)) { inTable = true; return; }
        if (!inTable) return;
        if (/^\s*\)\s*(partition|;|$)/i.test(l) || /^\s*\);/.test(l)) { inTable = false; return; }
        const t = l.trim();
        if (!t || t.startsWith('--') || /^(constraint|primary|unique|foreign|check|exclude|like)\b/i.test(t)) return;
        if (/^"?[a-z_][a-z0-9_]*"?\s+[a-z]/i.test(t) && !t.includes('--')) out.push(finding('R30-sql-comments', 'error', f.rel, b.start + k + 1, 'Column without a comment: ' + t.slice(0, 70)));
      });
    }
  }
  return out;
});

rule('R31-canonical-names', 'Every database and image name a plan document uses is registered in Appendix L', (ctx) => {
  const l = ctx.md.find((f) => /appendix-l-registry/.test(f.rel));
  if (!l) return [];
  const dbs = new Set([...l.text.matchAll(/`(nibras_[a-z0-9_]+)`/g)].map((m) => m[1]));
  // Appendix L writes a service's further images as a shorthand suffix: `nibras/reporting-api`, `-projections`.
  const images = new Set();
  for (const line of l.lines) {
    const full = [...line.matchAll(/`(nibras\/[a-z0-9-]+)`/g)].map((m) => m[1]);
    for (const x of full) images.add(x);
    if (!full.length) continue;
    const base = full[0].replace(/-[a-z0-9]+$/, '');
    for (const m of line.matchAll(/`(-[a-z0-9-]+)`/g)) images.add(base + m[1]);
  }
  if (!dbs.size) return [];
  // Metric names share the `nibras_` prefix and label keys the `nibras/` prefix, so only a
  // name that starts with a service, or a cell in a Database column, is a database or image.
  const serviceLed = (x) => SERVICE_NAMESPACES.has(x.split(/[-_]/)[0]);
  const out = [];
  for (const f of ctx.md) {
    if (!f.rel.startsWith('docs/plan/') || PLAN_SKIP.test(f.rel)) continue;
    for (const t of linedTables(f)) {
      const di = col(t, /^Database$/i);
      if (di < 0) continue;
      for (const r of t.rows) for (const m of (r.cells[di] || '').matchAll(/`(nibras_[a-z0-9_]+)`/g)) if (!dbs.has(m[1])) out.push(finding('R31-canonical-names', 'error', f.rel, r.line, 'Database ' + m[1] + ' is not in Appendix L'));
    }
    f.lines.forEach((line, i) => {
      if (DISCUSSES_ABSENCE.test(line)) return;
      for (const m of line.matchAll(/`nibras_([a-z0-9]+)`/g)) if (serviceLed(m[1]) && !dbs.has('nibras_' + m[1])) out.push(finding('R31-canonical-names', 'error', f.rel, i + 1, 'Database nibras_' + m[1] + ' is not in Appendix L'));
      for (const m of line.matchAll(/`nibras\/([a-z0-9-]+)`/g)) if (serviceLed(m[1]) && !images.has('nibras/' + m[1])) out.push(finding('R31-canonical-names', 'error', f.rel, i + 1, 'Image nibras/' + m[1] + ' is not in Appendix L'));
    });
  }
  return out;
});

rule('R32-workflow-tests', 'The owning service sheet cites every transition test of its workflows', (ctx) => {
  const r = ctx.md.find((f) => /appendix-r-workflow-catalog/.test(f.rel));
  const d13 = planDoc(ctx, '13');
  const l = ctx.md.find((f) => /appendix-l-registry/.test(f.rel));
  if (!r || !d13 || !l) return [];
  const owner = new Map();
  for (const line of d13.lines) { const m = /^\| (WF-[A-Z]+-\d{2}) \|[^|]*\| ([A-Za-z.]+) \|/.exec(line); if (m) owner.set(m[1], m[2]); }
  const tests = new Map();
  const hs = headings(r).filter((h) => /^WF-[A-Z]+-\d{2}\b/.test(h.text));
  hs.forEach((h, k) => {
    const end = k + 1 < hs.length ? hs[k + 1].line - 1 : r.lines.length;
    tests.set(/^(WF-[A-Z]+-\d{2})/.exec(h.text)[1], new Set(r.lines.slice(h.line, end).join('\n').match(/TC-[A-Z0-9]+-\d{3}/g) || []));
  });
  const expand = (text) => {
    const s = new Set(text.match(/TC-[A-Z0-9]+-\d{3}/g) || []);
    for (const m of text.matchAll(/TC-([A-Z0-9]+)-(\d{3})`?\s*(?:to|through|–|-)\s*`?TC-\1-(\d{3})/g)) for (let n = +m[2]; n <= +m[3]; n++) s.add('TC-' + m[1] + '-' + String(n).padStart(3, '0'));
    return s;
  };
  const out = [];
  for (const [wf, svc] of owner) {
    const sheet = ctx.byRel.get('docs/plan/06-services/' + svc.toLowerCase().replace('.', '-') + '.md');
    if (!sheet || !tests.has(wf)) continue;
    const plan = sectionLines(sheet, /test plan/i);
    if (!plan) { out.push(finding('R32-workflow-tests', 'error', sheet.rel, 1, 'No "Test plan" section, so ' + wf + ' has no transition tests here')); continue; }
    const cited = expand(plan.lines.join('\n'));
    const missing = [...tests.get(wf)].filter((t) => !cited.has(t));
    if (missing.length) out.push(finding('R32-workflow-tests', 'error', sheet.rel, plan.start, wf + ': the test plan does not cite ' + missing.join(', ')));
  }
  return out;
});

/* ---------------------------------------------------------------------- run */

export function lint(root, only) {
  const ctx = buildContext(root);
  const selected = only ? rules.filter((r) => only.includes(r.id)) : rules;
  const findings = [];
  for (const r of selected) {
    try {
      findings.push(...r.run(ctx));
    } catch (e) {
      findings.push(finding(r.id, 'error', '(lint)', 1, 'Rule crashed: ' + e.message));
    }
  }
  return findings;
}

export function formatFindings(findings) {
  if (!findings.length) return 'kit-lint: clean.\n';
  const byRule = new Map();
  for (const f of findings) {
    if (!byRule.has(f.rule)) byRule.set(f.rule, []);
    byRule.get(f.rule).push(f);
  }
  let out = '';
  for (const [ruleId, list] of [...byRule.entries()].sort()) {
    const meta = rules.find((r) => r.id === ruleId);
    out += '\n' + ruleId + '  ' + (meta ? meta.title : '') + '\n';
    for (const f of list.slice(0, 40)) out += '  ' + f.severity.padEnd(5) + ' ' + f.file + ':' + f.line + '  ' + f.message + '\n';
    if (list.length > 40) out += '  ... ' + (list.length - 40) + ' more\n';
  }
  const errors = findings.filter((f) => f.severity === 'error').length;
  out += '\nkit-lint: ' + errors + ' error(s), ' + (findings.length - errors) + ' warning(s) across ' + byRule.size + ' rule(s).\n';
  return out;
}

const invoked = (process.argv[1] || '').split('\\').join('/');
if (invoked.endsWith('kit-lint.mjs')) {
  const args = process.argv.slice(2);
  const root = args.find((a) => !a.startsWith('-')) || process.cwd();
  const findings = lint(root);
  process.stdout.write(args.includes('--json') ? JSON.stringify(findings, null, 2) + '\n' : formatFindings(findings));
  process.exit(findings.some((f) => f.severity === 'error') ? 1 : 0);
}
