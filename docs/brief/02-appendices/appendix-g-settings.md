# Appendix G. Settings Catalog

> Part of `docs/brief/02-appendices/`. Normative. Read with the master brief and the reference architecture.

Each setting has a scope (platform, tenant, campus, role, user), a default, validation, an audit trail, and a description in both languages.

- **General:** languages, default language, numerals, time zone, work week, calendars, currency, terminology overrides, branding and theme
- **Security:** password policy, 2FA requirement per role, session timeout, login methods, SSO providers and domain rules, IP allowlist, export approval rules, retention periods, **high-risk grant approval window** (how long a four-eyes proposal stays open before it expires; default 72 hours), **delegation maximum duration** (default 30 days)
- **Joining:** enabled methods, join codes, default roles, approvers, invitation expiry
- **Academic:** grading schemes, rounding, pass marks, promotion rules, rank visibility, publish windows, comment length, homework ceiling
- **Attendance:** mode (daily or per period), codes, cut-off times, lock window, thresholds and ladder, excuse rules
- **Finance:** tax, numbering series, late fee rules, discount rules, allocation order, payment methods, restriction rules, receipt layout, **pro-rata mode** (by day or by month, for late joiners and leavers; default by day), **rounding mode and decimals per currency** (default half-up, 2 decimals)
- **Communication:** messaging policy, office hours, moderation, translation, acknowledgment reminders
- **Notifications:** channel availability, quiet hours default, digest schedule, SMS credit limits, templates
- **Requests:** enabled types, SLAs, approval chains, fees
- **Safety:** gate pass validity, pickup verification method, visitor policy, emergency templates
- **AI:** enabled features, provider (local by default), usage limits, review requirements
- **Integrations:** API keys, webhooks, payment, SMS, e-invoicing, SSO, LTI tools, device adapters
- **Mobile:** minimum supported version, forced update, feature toggles, white-label flavor values

---
