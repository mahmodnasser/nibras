# Risks

Seeded from master brief Section 40. Reviewed at the end of every phase. A risk with no owner is not being managed.

| # | Risk | Likelihood | Impact | Mitigation | Owner | Status |
|---|---|---|---|---|---|---|
| 1 | A dependency changes licence mid-project | Medium | High | Pinned versions, licence scan in the pipeline, every dependency behind an interface, the Section 6.4 allow-list | Architect | Open |
| 2 | The timetable solver produces schedules a school will not accept | Medium | High | Constraints reviewed with a real school in phase 2, manual refinement with live conflict detection always available, quality score shown | Architect | Open |
| 3 | Arabic document shaping regresses unnoticed | Medium | High | Fonts bundled with the renderer, bilingual snapshot tests against committed baselines | Quality | Open |
| 4 | Offline sync produces conflicts nobody can explain | High | High | Per-entity rules in Appendix M, visible sync state, conflict banner showing both values, a test per rule | Mobile | Open |
| 5 | Twenty services overwhelm a small operations team | High | High | One service template, one pipeline, one dashboard set, the merge option in Appendix L, self-healing operations | Architect | Open |
| 6 | The product name is not cleared for trademark or the app stores | Medium | Medium | The name lives in one configuration value and one token file; clearance is Open Question 12 | Product owner | Open |
| 7 | A .NET 10 library the plan depends on is not ready | Low | Medium | The fallback list in master brief Section 19 names a replacement for each dependency | Architect | Open |
| 8 | Wolverine proves unsuitable for the messaging load | Low | High | Messaging isolated in one building block; Rebus and a thin client layer named as alternatives; contract tests independent of the library | Architect | Open |
| 9 | Data residency multiplies infrastructure cost per region | Medium | Medium | A region is a whole deployment; cost modelled before a region is promised | Product owner | Open |
| 10 | The penetration test finds a tenancy defect late | Low | Severe | Tenant isolation suite on every build from phase 1; a late finding is treated as a gap in the suite and fixed there | Architect | Open |
| 11 | No Apple build capacity when iOS is due | Medium | High | Open Question 14 and a budget line settled before phase 2; Android and mobile web ship independently of it | Product owner | Open |
| 12 | Translation quality in Arabic undermines credibility | Medium | High | A fluent reviewer owns the Arabic string set, the terminology glossary is versioned, the missing-translation report fails the build | Product owner | Open |
