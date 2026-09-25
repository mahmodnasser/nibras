# Test-case work for `docs/plan/06-services/platform.md`

## Collisions to resolve here (the owner keeps the identifier)

- **TC-INT-001** at line 2231: "Only tagged operations accept keys and tokens | Generated suite"
  - owner `docs/brief/02-appendices/appendix-w-feature-register.md` line 42: "24 | Open by default: API, webhooks, iCal, standards | "It talks to what we already use" | 1 | 1 surfaces | 2 | Platform"
- **TC-INT-002** at line 2235: "Plug-in kit conformance with the sample | Pipeline"
  - owner `docs/brief/02-appendices/appendix-w-feature-register.md` line 56: "38 | Plug-in kit for regional integrations | "Our partner built the ministry export" | 1 | 1 surfaces | 2 | Platform"
- **TC-DATA-010** at line 2229: "Tier migration shared to dedicated and back, zero lost writes, window under 5 minutes | Load tier"
  - owner `docs/plan/10-data-architecture.md` line 596: "Reference architecture Section 14, tier migration | Shared to dedicated and back without data loss | Zero lost writes, zero cross-tenant rows, read-only window under 5 minutes"
- **TC-PERF-003** at line 2238: "Settings, flags, plan, terminology and branding invalidation per scope | Integration"
  - owner `docs/plan/21-performance-engineering.md` line 326: "Platform settings, flags, plan, terminology, branding | `platform.settings.changed.v1`, `platform.feature-flag.changed.v1`, `platform.plan.changed.v1`, `platform.terminology.changed.v1` | Next read of the named scope mis"
