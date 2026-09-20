---
description: Accessibility (WCAG 2.2 AA) and Arabic RTL audit
argument-hint: [scope]
allowed-tools: Read, Grep, Glob, Bash
disable-model-invocation: true
---

Audit $ARGUMENTS for WCAG 2.2 AA and for Arabic RTL quality.

Check, in this order:

1. **Keyboard and focus.** Tab order follows reading order in both directions, visible focus ring on every interactive element, no keyboard trap, skip link, Escape closes overlays, the Ctrl+K palette reachable.
2. **Names and roles.** Every control has an accessible name in both languages, live regions announce asynchronous results, icon-only buttons carry labels, tables have headers and captions.
3. **Contrast and scale.** Light, dark, and tenant brand colors all reach 4.5:1 for text and 3:1 for interface; layout survives 200% text zoom and 320 CSS pixel width; touch targets at least 24 by 24 CSS pixels.
4. **RTL.** Mirrored layout through logical properties, mirrored directional icons, unmirrored logos and media controls, charts and progress read right to left, bidirectional text in inputs, tables, PDFs, and notification bodies.
5. **Arabic language quality.** Numeral system per tenant setting, Arabic search normalization (alef, ya, ta marbuta, tatweel, diacritics), plural categories, no truncation or overflow in Arabic strings, no sentences assembled from concatenated fragments.
6. **Motion.** Reduced-motion fallback on every animation, nothing that flashes more than three times per second.

For each finding give the screen or component, the rule it breaks, the fix, and the automated test that would have caught it.

## Reads first

- `docs/brief/01-master-brief.md` Sections 16 and 17 only.
- `docs/plan/14-design-system-and-ux.md` for tokens, color system, and states.
- `docs/brief/02-appendices/appendix-x-platform-support.md` for the browsers, devices, and screen sizes that must pass.
- `docs/plan/08-web-structure.md` or `docs/plan/09-mobile-structure.md`, whichever owns the scope.
- The component or screen source in scope. Do not load the whole brief.

## Output contract

- `## Scope and what I checked`
- `## Findings` — table: id, screen or component, rule (WCAG criterion or RTL rule), severity (high, medium, low), evidence (file and line), fix, test that locks it
- `## Not checked` — what needs a running browser, a screen reader, or a real device
- `## Smallest fix set` — the high-severity items in dependency order

## Stop conditions

- Stop and ask when the argument names no screen, component, or workspace.
- Stop when the tenant brand palette is undefined: contrast cannot be judged against an unknown color.
- Stop when a fix would change an approved design token. That is an ADR, not an audit fix.