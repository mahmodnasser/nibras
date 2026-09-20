# Kit lint baseline: v8

Run on 2026-09-19T02:27Z against `nibras-platform-kit-v8` with the v9 lint.
It proves the lint detects the defects v9 exists to fix. v9 must finish at zero errors.

```

R03-brief-versions  The brief documents carry the same version
  error docs/brief/01-master-brief.md:1  Version v8 disagrees with the other briefs (v8, v7)
  error docs/brief/02-appendices.md:1  Version v7 disagrees with the other briefs (v8, v7)
  error docs/brief/03-reference-architecture.md:1  Version v8 disagrees with the other briefs (v8, v7)

R05-no-open-todos  No TODO or TBD outside OPEN_QUESTIONS and IDEAS
  error CLAUDE.md:35  "TODO" must move to OPEN_QUESTIONS.md with an owner and a default
  error docs/brief/01-master-brief.md:979  "TODO" must move to OPEN_QUESTIONS.md with an owner and a default
  error docs/plan/PLAN_SPEC.md:5  "to be decided" must move to OPEN_QUESTIONS.md with an owner and a default

R06-service-coverage  Every service sheet appears in the permission, event and dependency catalogs
  warn  docs/brief/03-reference-architecture.md:285  Service Identity has no "identity." permission or event entries in the appendices
  warn  docs/brief/03-reference-architecture.md:293  Service Platform has no "platform." permission or event entries in the appendices
  warn  docs/brief/03-reference-architecture.md:299  Service School has no "school." permission or event entries in the appendices
  warn  docs/brief/03-reference-architecture.md:306  Service Admissions has no "admissions." permission or event entries in the appendices
  warn  docs/brief/03-reference-architecture.md:316  Service Assessment has no "assessment." permission or event entries in the appendices
  warn  docs/brief/03-reference-architecture.md:321  Service Scheduling has no "scheduling." permission or event entries in the appendices
  warn  docs/brief/03-reference-architecture.md:331  Service Finance has no "finance." permission or event entries in the appendices
  warn  docs/brief/03-reference-architecture.md:336  Service Communication has no "communication." permission or event entries in the appendices
  warn  docs/brief/03-reference-architecture.md:336  Service Communication is missing from the event dependency matrix
  warn  docs/brief/03-reference-architecture.md:346  Service Requests has no "requests." permission or event entries in the appendices
  warn  docs/brief/03-reference-architecture.md:351  Service Documents has no "documents." permission or event entries in the appendices
  warn  docs/brief/03-reference-architecture.md:356  Service Behavior has no "behavior." permission or event entries in the appendices
  warn  docs/brief/03-reference-architecture.md:356  Service Behavior is missing from the event dependency matrix
  warn  docs/brief/03-reference-architecture.md:360  Service Wellbeing has no "wellbeing." permission or event entries in the appendices
  warn  docs/brief/03-reference-architecture.md:360  Service Wellbeing is missing from the event dependency matrix
  warn  docs/brief/03-reference-architecture.md:364  Service Hr has no "hr." permission or event entries in the appendices
  warn  docs/brief/03-reference-architecture.md:368  Service Operations has no "operations." permission or event entries in the appendices
  warn  docs/brief/03-reference-architecture.md:368  Service Operations is missing from the event dependency matrix
  warn  docs/brief/03-reference-architecture.md:371  Service Reporting has no "reporting." permission or event entries in the appendices
  warn  docs/brief/03-reference-architecture.md:376  Service Ai has no "ai." permission or event entries in the appendices
  warn  docs/brief/03-reference-architecture.md:376  Service Ai is missing from the event dependency matrix

kit-lint: 6 error(s), 21 warning(s) across 3 rule(s).
```
