---
name: flutter-multi-target
description: Flavors, white-label builds, desktop kiosk mode, and devices without Google services. Load before configuring a Flutter flavor, a push or maps integration, a kiosk build, or a release pipeline.
---

# Flutter multi-target

One codebase, four shapes: the shared multi-school application, the white-label school application, the gate and reception kiosk, and mobile web. Each has a different set of assumptions, and the assumptions are where builds break.

## Flavors

| Flavor | Purpose | Differs by |
|---|---|---|
| `dev` | Local against the development stack | Endpoint, logging, demo login helper |
| `shared` | The public multi-school application | Store identity, tenant chooser at sign-in |
| `whitelabel` | One school's branded application | Name, icon, colors, bundle identifier, fixed tenant |
| `kiosk` | Gate and reception, desktop or tablet | No personal session, device-bound credential, wake lock, no sharing |

Rules: brand values come from a build-time configuration file per flavor, never from code branches. The demo login helper exists in `dev` only and must be impossible to compile into a store build. Adding a school is a configuration file and a pipeline entry, never a code change.

## Devices without Google services

A meaningful share of the target market runs devices with no Play Services. Design for it from the start:

- **Push** has two paths: Firebase Cloud Messaging where available, and a foreground-plus-polling fallback with a visible "notifications limited" state elsewhere. Detect at runtime; never assume.
- **Maps** use OpenStreetMap tiles. No Google Maps dependency.
- **Sign-in** never requires a Google account.
- **Location** for transport features degrades to manual stop selection.
- Test on one device without Google services in the release checklist, and say so in the report.

## Kiosk

Wake lock on, system navigation suppressed, no personal data on screen at rest, automatic return to the idle screen after 30 seconds, a device-bound credential rather than a user session, and full function while offline with a queued outbox. A kiosk that shows a child's name to the next visitor in the queue is a privacy incident.

## Worked example

```dart
// lib/config/flavor.dart
enum Flavor { dev, shared, whitelabel, kiosk }

class AppConfig {
  const AppConfig({
    required this.flavor,
    required this.apiBaseUrl,
    required this.fixedTenantId,   // null except for whitelabel and kiosk
    required this.brand,
    required this.pushStrategy,
  });

  final Flavor flavor;
  final String apiBaseUrl;
  final String? fixedTenantId;
  final BrandTokens brand;
  final PushStrategy pushStrategy;

  bool get showsDemoLoginHelper => flavor == Flavor.dev;
  bool get requiresPersonalSession => flavor != Flavor.kiosk;
}
```

```bash
flutter build apk --flavor whitelabel \
  --dart-define-from-file=config/schools/alnoor.json
```

```powershell
flutter build apk --flavor whitelabel `
  --dart-define-from-file=config/schools/alnoor.json
```

Golden tests run per flavor in LTR and RTL. The release checklist records: one device with Google services, one without, one tablet in kiosk mode, and mobile web, each pass or fail, each named.
