# Appendix X. Platform Support and Test Matrix

> Part of `docs/brief/02-appendices/`. Normative. Read with the master brief and the reference architecture.

Nibras is developed on Windows, Linux and macOS, runs on Linux servers, and is used on Android, iOS, mobile web and desktop kiosks. Every support claim below names the test that proves it. A claim with no test is removed from this table rather than kept as an intention.

---

## X.1 Supported surfaces

| Surface | Supported | Explicitly not supported |
|---|---|---|
| Developer workstation | Windows 11 with PowerShell 7 or Git Bash; Ubuntu 22.04 and later; macOS 14 and later | Windows PowerShell 5.1 as the only shell; 32-bit hosts |
| Container runtime for developers | Docker Engine under WSL2, Podman, Docker Engine on Linux, Podman or Colima on macOS | Docker Desktop is not required and is not assumed; it is paid for larger companies |
| Server | Linux: Debian, Ubuntu, and the Red Hat family, x86-64 and arm64 | Windows Server. Redis and several dependencies have no supported Windows server builds |
| On-premises on a Windows host | A Linux virtual machine appliance on Hyper-V or VMware, built from `deploy/onprem/` | Native installation on Windows Server |
| Browsers | Chrome, Edge, Firefox, Safari, last two major versions; Samsung Internet current | Internet Explorer; any browser without CSS logical properties |
| Screen readers | NVDA and Narrator on Windows, VoiceOver on macOS and iOS, TalkBack on Android | none claimed beyond these |
| Mobile application | Android 8.0 and later, with and without Google services; iOS 15 and later | Android 7 and below; iOS 14 and below |
| Mobile web | Parent, student, teacher and principal workspaces, installable as a progressive web application | The full administration console on a phone; it is available but not optimised |
| Desktop kiosk | Flutter Windows and Linux builds for gate, front desk and clinic modes | macOS kiosk, until a customer needs it |
| Tablets | Android tablets and iPad for kiosk, bus attendant and nurse modes | none excluded |

---

## X.2 Test matrix

| Test | Linux runner | Windows runner | macOS runner | Device or manual |
|---|---|---|---|---|
| Backend unit tests | every service | `BuildingBlocks`, `Documents`, `Localization` | none | none |
| Backend integration tests with Testcontainers | every service | `BuildingBlocks`, `Documents` | none | none |
| Culture, calendar and time zone inside the built image | yes: `ar-SA`, Um Al Qura, Riyadh, Amman, Dubai | none | none | none |
| Architecture tests | yes | none | none | none |
| Generated permission and tenant-isolation suites | yes | none | none | none |
| Query budget assertions | yes | none | none | none |
| Web unit and lint | yes | none | none | none |
| Web end-to-end, Playwright, four theme and direction combinations | Chromium, Firefox, WebKit | none | none | none |
| Accessibility, axe-core | yes | none | none | Manual screen-reader pass per release |
| Web bundle budgets | yes | none | none | none |
| Mobile analyze, unit and widget tests | yes | none | none | none |
| Mobile golden tests, left-to-right and right-to-left | yes | Windows desktop kiosk goldens | iOS goldens | none |
| Mobile build artefacts | Android APK and app bundle | Windows kiosk MSIX | iOS archive and TestFlight | none |
| Mobile on real devices | none | none | none | Low-end Android 8, Android 14, iPhone SE, iPad, one device without Google services |
| Generated PDF snapshots in both languages | yes | none | none | none |
| Load and soak, k6 | yes | none | none | none |
| Kit lint and its own tests | yes | yes | none | none |
| One-command local start, `dev-smoke` | yes | yes | none | none |
| Appliance build and upgrade | nested virtualisation job | none | none | Quarterly drill on Hyper-V |
| Restore and disaster-recovery drill | yes | none | none | Quarterly, recorded |

**Why Windows appears at all.** Three things break silently when only Linux is tested: path composition, culture-sensitive parsing, and line endings in generated artefacts. `BuildingBlocks`, `Documents` and `Localization` are the projects where those three live, so they run on both. Everything else is Linux, because that is where it will run.

**Why macOS appears at all.** An iOS build cannot be produced anywhere else. The macOS job is path-filtered to `src/Mobile/**` so it does not run on every commit, and its cost is a line in master brief Section 30.

---

## X.3 Cross-platform edge cases

These belong with the edge cases in master brief Section 15. Each has a test or a rule that catches it.

| Edge case | What goes wrong | Caught by |
|---|---|---|
| Two files differing only by case | A Windows commit breaks the Linux checkout, or the reverse | `kit-lint` rule R13, and the same check in the repository pipeline |
| A path longer than 200 characters | A Windows checkout fails entirely | `kit-lint` rule R14 |
| CRLF inside generated SQL or a PDF baseline | Migration checksums and snapshot comparisons drift | `.gitattributes` plus a pipeline check that generated artefacts are byte-identical on both runners |
| An Alpine image without ICU | Arabic collation and the Um Al Qura calendar silently fall back to invariant | The culture test that runs inside the built image |
| A tzdata update mid-year | Bell schedules and cut-off times shift for a region | tzdata pinned per release; a test asserts the expected offsets for three school time zones |
| An ICU update mid-year | Hijri conversions move by a day | ICU pinned per release; a test asserts known Hijri and Gregorian pairs |
| Culture-sensitive parsing of a decimal | `1,5` and `1.5` swap meaning between a Windows developer and a Linux server | Explicit culture in every parse and format; a unit test that runs the money rules under three cultures |
| Arabic digits in an Excel file produced on Windows | Import reads them as text and rejects the row | Import normalises numerals before validation; a test with a file containing Arabic-Indic digits |
| Font shaping differs between emulator and device | Arabic ligatures render correctly in tests and badly on a phone | Fonts bundled with the application and the renderer, and a device pass per release |
| iOS suspends the app for days | Offline queue grows and the sync token expires | Sync on open and on silent push; the token has no expiry shorter than 30 days; a test replays a 30-day-old token |
| Android battery optimisation kills the sync worker | A parent stops receiving anything and does not know why | The app detects the restriction and explains once; urgent messages still arrive by push |
| A device clock is wrong by hours | Offline attendance arrives with a wrong timestamp | The server stamps `receivedAt` and the client's `occurredAt` is kept separately; the conflict rules in Appendix M use the server stamp for ordering |
| Podman names its network differently from Docker | Testcontainers cannot reach the database | Documented configuration for both in `docs/dev-setup/`, and the `dev-smoke` job runs one of each |
| A white-label flavour builds on Linux but its iOS twin does not | Android ships and iOS silently lags a version | The release gate requires both artefacts before a flavour is marked released |

---

## X.4 Repository hygiene rules

These make a mixed-operating-system team possible and are enforced, not advised.

| Rule | Enforced by |
|---|---|
| `* text=auto eol=lf`, `*.ps1 text eol=crlf`, binary patterns for images, fonts and archives | `.gitattributes` |
| `end_of_line = lf` for source | `.editorconfig` |
| No two paths differing only by case | `kit-lint` R13 |
| No repository-relative path over 200 characters | `kit-lint` R14 |
| Every tool entry point ships a `.ps1` and a `.sh` wrapper over one Node implementation | `kit-lint` R15 |
| Every hook invokes `node` with a relative path | `kit-lint` R16 |
| `core.longpaths` enabled on Windows clones | The Windows setup script in `docs/dev-setup/windows.md` |
| Paths composed with `Path.Combine`, never a literal separator | Code review and the portability rule in `.claude/rules/portability.md` |
| Parsing and formatting always specify a culture | The portability rule, plus the money and date rule tests under three cultures |
