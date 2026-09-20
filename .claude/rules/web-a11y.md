---
paths:
  - "src/Web/**/*.html"
  - "src/Web/**/*.scss"
  - "src/Web/**/*.css"
  - "src/Web/**/*.component.ts"
---

# Web accessibility and RTL rules

- **Logical CSS properties only.** `margin-inline-start`, `padding-inline-end`, `inset-inline-start`, `text-align: start`. Never `left`, `right`, `margin-left`, or `padding-right` for layout.
- Direction comes from `dir` on the document root. No component assumes a direction.
- Directional icons mirror in RTL; logos, media controls, clocks, and checkmarks do not.
- Every control has an accessible name in both languages. Icon-only buttons carry a label, and the icon is `aria-hidden`.
- Focus order follows reading order in both directions. The focus ring is visible on every interactive element. Escape closes every overlay. No keyboard trap.
- Contrast is 4.5:1 for text and 3:1 for interface, verified in light, dark, and the tenant brand palettes.
- The layout survives 200% text zoom and a 320 CSS pixel width with no horizontal page scroll. Touch targets are at least 24 by 24 CSS pixels.
- Asynchronous results are announced in a live region. Errors are programmatically associated with their field.
- Motion uses `transform` and `opacity` only, with `animate.enter` and `animate.leave` and view transitions, and a `prefers-reduced-motion` fallback. **Never `@angular/animations`.**
- No hard-coded user-facing string, and no sentence assembled from concatenated fragments.
- Every screen ships its axe-core check and visual snapshots in four combinations: light LTR, light RTL, dark LTR, dark RTL.