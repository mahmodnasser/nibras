# Test-case work for `docs/plan/06-services/gateway.md`

## Collisions to resolve here (the owner keeps the identifier)

- **TC-IDN-056** at line 342: "A leaver presenting a cached token is refused at the Gateway | Integration with Identity"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 286: "Revoked to Revoked | Leaver presents a cached token | Request refused at the gateway"
- **TC-INT-001** at line 343: "Keys and tokens are refused on every operation not tagged `public` | Generated suite"
  - owner `docs/brief/02-appendices/appendix-w-feature-register.md` line 42: "24 | Open by default: API, webhooks, iCal, standards | "It talks to what we already use" | 1 | 1 surfaces | 2 | Platform"
- **TC-SEC-330** at line 336: "A browser-supplied tenant header is refused; the token's tenant must match (T-GW-01) | Integration"
  - owner `docs/plan/12-security-privacy-safety.md` line 300: "T-GW-01 | Tenant resolution | Spoofing | Header-based tenant override from a browser client | med | critical | Tenant header accepted only from the mobile client credential; web resolves from the domain; the token's tena"
- **TC-SEC-039** at line 337: "A token signed with a retired key is refused after the overlap (T-GW-02) | Integration"
  - owner `docs/plan/12-security-privacy-safety.md` line 34: "V9 Self-contained tokens | Signed JWT, algorithm pinning, small claims | OpenIddict signing key with overlap rotation; claims are user, tenant, roles, permission version only (master brief Section 7.5)"
- **TC-SEC-058** at line 338: "One tenant at its limit does not exhaust the platform (T-GW-03); Appendix N scenario N-06 at load | Integration, load"
  - owner `docs/plan/12-security-privacy-safety.md` line 64: "Unrestricted resource consumption | Gateway rate limits per tenant and user; per-key quota on the public API; job concurrency per tenant"
- **TC-SEC-122** at line 339: "The maintenance flag changes only through Platform with the operator role and is audited (T-GW-05) | Integration"
  - owner `docs/plan/12-security-privacy-safety.md` line 102: "T-PLT-06 | Feature flags and settings | Tampering | A setting change silently widens a security policy | low | high | Security-group settings from Appendix G are `elevated`; change history in the audit viewer; `reset-to-"
- **TC-SEC-362** at line 340: "The seeded account's restricted token reaches only the Identity self-service routes in Production | Integration"
  - owner `docs/plan/12-security-privacy-safety.md` line 630: "3 | In Production, all admin functions blocked until the password is changed and 2FA enrolled; console warning while any account still uses a seeded password | Gateway policy for platform routes checks the `seeded-creden"
- **TC-INF-110** at line 345: "One correlation id from the Gateway span to the last consumer | Nightly trace test"
  - owner `docs/plan/15-deployment-and-operations.md` line 820: "Master brief Section 7.6 | OpenTelemetry, correlation id end to end | One correlation id from the Gateway to the last consumer in a trace of the absence alert flow"
