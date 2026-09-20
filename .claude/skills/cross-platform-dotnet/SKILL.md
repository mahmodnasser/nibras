---
name: cross-platform-dotnet
description: Globalization, ICU, time zones, paths, and culture pinning so .NET behaves identically on Windows and Linux. Load before writing file, date, number, or string-comparison code, and before changing a container base image.
---

# Cross-platform .NET

The kit and the product must behave identically on Windows and Linux. Most breakages come from three places: paths, culture, and the container's missing libraries.

## Paths

- Always `Path.Combine` and `Path.Join`. Never string concatenation with a separator, and never a literal `\` or `/` in a path.
- Never assume case-insensitivity. Linux is case-sensitive; two files differing only by case will destroy each other on a Windows checkout.
- Keep repository-relative paths under 200 characters.
- No absolute paths in configuration. Resolve from a content root.
- Compare paths with `StringComparer.OrdinalIgnoreCase` on Windows and `Ordinal` elsewhere only when you must; prefer comparing resolved full paths.

## Culture

Pin it. A machine set to Turkish or Arabic must produce byte-identical output.

```csharp
// Program.cs, before anything formats or parses
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
```

- Machine-facing values (identifiers, routing keys, file names, JSON, logs, database values) use `CultureInfo.InvariantCulture`, always explicitly.
- User-facing values use the **tenant's** culture, resolved per request, never the machine's.
- `ToUpperInvariant()` and `ToLowerInvariant()` for comparison. `ToLower()` in Turkish turns `I` into `ı` and breaks identifier matching.
- Every `string.Equals`, `StartsWith`, `EndsWith`, `IndexOf`, and `Compare` takes an explicit `StringComparison`.
- `decimal.Parse(s, CultureInfo.InvariantCulture)`. A comma is a decimal separator in half the world.
- `DateTimeOffset` on the wire, `TimeProvider` injected, never `DateTime.Now`.

## Containers

- **Never `InvariantGlobalization=true`.** This product sorts Arabic names and formats dates for two calendars.
- Alpine images need `icu-libs` and `tzdata` installed explicitly. Without them, `TimeZoneInfo.FindSystemTimeZoneById` throws at runtime, in production, at a bell-schedule boundary.
- Prefer a Debian-based runtime image unless image size is a measured problem, and record the choice as an ADR either way.

```dockerfile
# Alpine runtime, made correct
RUN apk add --no-cache icu-libs icu-data-full tzdata
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
```

## Worked example

Before, broken on a Turkish-locale Linux host:

```csharp
var key = $"nibras:{tenant}:{service.ToLower()}:{id}";
var due = DateTime.Parse(row["due"]);
var path = baseDir + "\\exports\\" + fileName;
```

After:

```csharp
var key = $"nibras:{tenant}:{service.ToLowerInvariant()}:{id}";
var due = DateTimeOffset.Parse(row["due"], CultureInfo.InvariantCulture);
var path = Path.Combine(baseDir, "exports", fileName);
```

Every tool ships both wrappers over one implementation: `<tool>.ps1` for Windows PowerShell and `<tool>.sh` for bash. The kit lint enforces it.