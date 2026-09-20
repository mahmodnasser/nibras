---
paths:
  - "tools/**/*"
  - "**/*.ps1"
  - "**/*.sh"
  - "**/Dockerfile"
  - "**/Dockerfile.*"
  - ".gitattributes"
  - ".editorconfig"
---

# Portability rules

- **Every tool entry point ships both wrappers**: `<tool>.ps1` for Windows PowerShell and `<tool>.sh` for bash, over **one** implementation. Two implementations drift, and the drift shows up in one operating system's build only.
- **No hard-coded path separator.** Use the language's path join. No literal `\` or `/` inside a path string, and no absolute path in configuration.
- **No case-only path collisions.** Two files differing only by letter case coexist on Linux and destroy each other on Windows and macOS.
- Repository-relative paths stay under 200 characters.
- **No culture-sensitive parsing or formatting without an explicit culture.** Machine-facing values use invariant culture, always stated. User-facing values use the tenant's culture, never the machine's. `ToUpperInvariant` and `ToLowerInvariant` for comparison; an explicit `StringComparison` on every comparison.
- **No Alpine base image without `icu-libs` and `tzdata`**, and `InvariantGlobalization` is never enabled anywhere.
- `.gitattributes` normalizes text and keeps shell scripts at `lf`. A shell script checked out with carriage returns fails with an unreadable error.
- Scripts detect their own shell rather than assuming one, and fail with a clear message on an unsupported one.
- Mobile targets never assume Google services, a specific biometric, granted storage permission, or a screen wider than 360 logical pixels. Each has a stated fallback.
- Any change here is verified on the operating system you are on, and the report says which one, and which targets therefore stayed unverified.
