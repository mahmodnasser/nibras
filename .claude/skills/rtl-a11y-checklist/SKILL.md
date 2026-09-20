---
name: rtl-a11y-checklist
description: The Arabic RTL and WCAG 2.2 AA checklist to run before any screen is called done. Load when building or reviewing a screen, a document template, a notification, or an export.
---

# RTL and accessibility checklist

Run all of it. A screen that passes nine of ten items is not nine-tenths accessible; it is inaccessible to one group of people.

## Layout

- Logical CSS properties only: `margin-inline-start`, `padding-inline-end`, `inset-inline-start`, `text-align: start`. No `left`, `right`, `margin-left`, `padding-right`.
- Direction comes from `dir` on the document root and is never assumed in a component.
- Directional icons mirror (back, next, indent, send, trend). Logos, play controls, clocks, and checkmarks do not.
- Charts, progress bars, timelines, and steppers read right to left in Arabic.
- Tables keep numeric columns aligned to the numeral's own direction, not the paragraph's.

## Text

- No string built from concatenated fragments. Word order differs.
- Plural categories handled as categories, not as singular and plural.
- Bidirectional isolation around identifiers, phone numbers, file names, and money inside Arabic sentences.
- Numeral system comes from the tenant setting. Identifiers, version numbers, and routing keys never localize.
- Arabic strings are longer than the English. Test at the longest string, not the shortest.

## Accessibility

- Every control has an accessible name in both languages; icon-only buttons carry a label.
- Focus order follows reading order in both directions; the focus ring is visible on every interactive element; Escape closes every overlay; no keyboard trap.
- Contrast 4.5:1 for text and 3:1 for interface, in light, dark, and every tenant brand palette.
- The layout survives 200% text zoom and a 320 CSS pixel width without horizontal scrolling.
- Touch targets at least 24 by 24 CSS pixels with adequate spacing.
- Asynchronous results are announced in a live region; errors are associated with their field.
- Every animation has a reduced-motion fallback; nothing flashes more than three times per second.

## Worked example

Before:

```css
.card__action { margin-left: 8px; text-align: left; }
```

```html
<button class="card__action"><svg class="icon-next"></svg></button>
```

After:

```css
.card__action { margin-inline-start: 8px; text-align: start; }
[dir="rtl"] .icon-next { transform: scaleX(-1); }
```

```html
<button class="card__action" [attr.aria-label]="'attendance.next' | translate">
  <svg class="icon-next" aria-hidden="true"></svg>
</button>
```

The Playwright visual snapshot runs in four combinations: light LTR, light RTL, dark LTR, dark RTL. Four, every time.