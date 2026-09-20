---
paths:
  - "src/Mobile/**/*"
---

# Mobile rules

- Structure per `docs/plan/09-mobile-structure.md`: each feature has `data/`, `domain/`, `presentation/`. No business rules in widgets.
- Offline-first where the plan says so: Drift storage, outbox queue, visible sync state, conflict rules per entity.
- Secure storage for tokens, biometric unlock, no sensitive data in logs or lock-screen notifications.
- Shared design tokens with the web. Implicit animations, Hero, `flutter_animate`; respect the reduce-motion setting.
- No hard-coded strings. Arabic RTL and dynamic text size on every screen. Golden tests in LTR and RTL for key screens.
- No Firebase beyond Cloud Messaging. No advertising or tracking SDKs. No Syncfusion widgets.
