---
paths:
  - "src/Web/**/*"
---

# Web rules

- Structure per `docs/plan/08-web-structure.md`. Features never import from each other, only from `shared`, `core`, `ui`, and `data-access`.
- Standalone components, signals, SignalStore, zoneless or OnPush. No business rules in components. Generated API clients only.
- Use design-system components and tokens only. Missing component: add it to `libs/ui` with a Storybook story first.
- Animations: native CSS with `animate.enter` and `animate.leave`, view transitions, `transform` and `opacity` only, reduced-motion fallback. **Never `@angular/animations`.**
- Every screen: loading skeleton, empty, error, offline, processing, no-permission. Permission directive and guards mirror the server; never rely on them for security.
- No hard-coded strings. English and Arabic, logical CSS properties for RTL, light and dark.
- Open source only: no AG Grid Enterprise, Kendo, Syncfusion, DevExpress, Highcharts, or FullCalendar premium plugins.
