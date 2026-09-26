# Open Questions

Every question has a **default**, so nothing here blocks the plan. Answering changes the default; silence accepts it. The impact column says what it costs to be wrong, which is how to decide what to answer first.

Thirty questions are open. `docs/plan/01-questions-and-assumptions.md` restates each one with its likelihood, impact, score and RISK identifier in `docs/plan/18-risk-register.md`, and kit-lint rule R26 fails when the two files disagree on which questions are open.

Six questions from v8 were closed in v9 by decision records that are still **Proposed, awaiting product owner confirmation**. They appear at the bottom as defaults in force, not as settled answers, until the product owner accepts each record. The four value decisions of ADR-0019, which the product owner Accepted on 2026-09-22, are settled, and so is question 30 (the billing count), which the product owner answered on 2026-09-26 by accepting ADR-0027; it is listed under "Settled in v9.7" at the bottom.

| # | Question | Default assumption | Impact if wrong | Owner | Status |
|---|---|---|---|---|---|
| 1 | .NET 10 or .NET 8? | .NET 10 LTS | Runtime support window, and the fallback list in master brief Section 19 | Product owner | Open |
| 2 | SaaS multi-tenant, on-premises, or both? | Both | Deployment and licensing model, and whether the appliance in `deploy/onprem/` is phase 6 or phase 2 | Product owner | Open |
| 3 | Target countries for the first customers? | Saudi Arabia, United Arab Emirates, Jordan | Tax, e-invoicing plug-ins, regulatory reports, data residency, and which compliance rows in Section 33 are real. RISK-23 | Product owner | Open |
| 4 | Which Tier 2 modules matter most for the first release? | None until Tier 1 is complete | Roadmap order after phase 4 | Product owner | Open |
| 5 | Local AI hardware available, or does assist ship later? | Rungs 1 and 2 ship with the product; rung 3 ships later, off by default | Whether the AI service is phase 5 or phase 6. Nothing else, because the product is complete at rung 1 | Product owner | Open |
| 6 | Wolverine, or a thin in-house layer over RabbitMQ.Client? | Wolverine | The messaging building block only. ADR-0004 | Architect | Open |
| 7 | Which services start merged? | Separate, as Appendix L defines them | Service count in phase 1. Merging is a build-time choice that changes no names | Architect | Open |
| 8 | Pricing model and plan limits? | Per active student, with tiers. "Active" is defined in master brief Section 36 | Platform service scope and the metering events | Product owner | Open |
| 9 | Which regional plug-ins first? | Interfaces in Tier 1, implementations per country in phase 5, except the e-invoicing plug-in for the country of a VAT-registered first customer, which the Finance owner's team builds in phase 3 as the conditional slices SL-FIN-448 to SL-FIN-450 (ZATCA, 9 slice-days) and SL-FIN-451 and SL-FIN-452 (JoFotara, 5 slice-days), counted in phase 3 by document 34; a set with no such customer moves to phase 5 | Integration work and the partner story. With no VAT-registered first customer in Saudi Arabia or Jordan, phase 3 carries 14 slice-days it does not need; a plug-in the default does not build (a ministry report, the United Arab Emirates) moves into phase 3 without a specification. Decided for e-invoicing at the phase 2 exit review. Scored 12 (4 x 3). RISK-23, RISK-53 | Product owner | Open |
| 10 | Nursery and kindergarten features in the first release? | No. Phase 4 | Daily sheet scope and the admissions age rules | Product owner | Open |
| 11 | Languages beyond English and Arabic? | No, but every string is externalised so a third is configuration | Translation operations and the plural rules | Product owner | Open |
| 12 | Is the name Nibras cleared for trademark, domain and app stores in the target countries? Final taglines? | Use Nibras, keep the name in one configuration value and one token file | Rebranding cost if it must change, and the mobile application identifier | Product owner | Open |
| 13 | OpenIddict with ASP.NET Core Identity, or Keycloak? | OpenIddict. ADR-0008 | The Identity service's shape. Keycloak suits an SSO-heavy customer and changes the service materially | Architect | Open |
| 14 | Is there a Mac build host, or do we buy hosted macOS runner minutes? | Hosted runner minutes, budgeted in master brief Section 30 | iOS cannot be built without one. Android and mobile web are unaffected. RISK-04 | Product owner | Open |
| 15 | Hosting provider and regions? | One region to start, chosen with the first customer's residency requirement | Cost per region, and whether residency is a real commitment or a future one | Product owner | Open |
| 16 | Are there Windows hosts among the first on-premises customers? | Yes, served by the Linux virtual machine appliance | Whether `deploy/onprem/` needs a Hyper-V image at phase 2 rather than phase 6: 5 slice-days (SL-INF-608, SL-INF-609) move into phase 1 for an install, 18 (all of CAP-INF-05) for installs and upgrades | Product owner | Open |
| 17 | What share of parents use devices without Google services? | Low, but not zero. The fallback in master brief Section 37 covers them | Whether a Huawei push adapter is Tier 2 or Tier 3 | Product owner | Open |
| 18 | Do any target customers require SAML 2.0 or SCIM? | No. OpenID Connect with Google and Microsoft covers phase 1 | Whether enterprise identity is a phase 1 release condition. SL-IDN-011 (3 slice-days) already builds the SAML and SCIM adapters in phase 1 (ADR-0024, Shared), so a yes adds no slice-days; it may reopen Keycloak (question 13) | Product owner | Open |
| 19 | Retention periods per country? | The defaults in master brief Section 32 | Storage cost, and a compliance failure if a country mandates longer or shorter | Product owner | Open |
| 20 | Support hours and time zones at launch? | Business hours in the school's time zone, per the tiers in Section 39 | Staffing, and what the contract can promise | Product owner | Open |
| 21 | Which payment gateway first? | Manual and bank transfer by default, with one gateway adapter chosen with the first customer | Per-transaction cost and the reconciliation work | Product owner | Open |
| 22 | Which SMS provider first? | None. Email and push cover everything except the urgent fallback | Per-message cost, and whether the urgent fallback exists at launch | Product owner | Open |
| 23 | Riverpod or flutter_bloc for mobile state? | Riverpod | Mobile structure only. Either satisfies the layering rules | Architect | Open |
| 24 | Team size and shape? | The shape in master brief Section 29: five to eight engineers building slices | Every phase range, phase 1 included. `tools/plan-build/schedule-34.mjs` recomputes them from document 34; phase 1 is 14 weeks with eight builders and 22 with five. RISK-06, RISK-35 | Product owner | Open |
| 25 | Is there a fixed launch date or an external commitment? | No fixed date; phases are sequenced by dependency | Whether the MVP cut line in Section 28 needs to move | Product owner | Open |
| 26 | Does any first customer require a formal certification, for example SOC 2 or 1EdTech? | No. Compatibility yes, certification only when someone pays for it | Budget and timeline, both material. Compatibility lands with OneRoster in phase 3, LTI in phase 4, and QTI, Open Badges and CASE exchange in phase 5; no RISK, because the score is 6 | Product owner | Open |
| 27 | When does a parent receive an absence alert: within 30 seconds of the mark, or 30 minutes after the register closes? | Within 30 seconds, as REQ-ATT-017 and master brief Section 31 require, and as SL-ATT-203 builds it; Appendix R WF-ATT-01 is aligned once decided | A grace window avoids alarming a parent over a teacher's correction; immediacy is what parents expect and what the service levels promise. Changes WF-ATT-01, TC-ATT-003 and the notification lane. Answer in writing before the first phase 2 attendance slice. RISK-41 | Product owner | Open |
| 28 | Do a read-only public API, the OneRoster export and iCal move from Tier 2 into Tier 1? | They stay where the roadmap builds them: iCal in phase 2, the public API and OneRoster in phase 3 under CAP-INT-01, with feature 24 at Tier 2 in Appendix W; document 03 already lists REQ-INT-014 and REQ-INT-015 at Tier 1, a disagreement the answer settles | Document 02 found a public API in six of ten competitors; if it is table stakes, a first customer may require it before phase 3. Changes Appendix W row 24 and the MVP cut line. Pulling the read-only API, iCal and OneRoster forward moves 17 slice-days of CAP-INT-01. Answer in writing before the MVP cut line is confirmed. RISK-42 | Product owner | Open |
| 29 | May a student's level S check-in answer wait in the device's encrypted outbox until the next sync, or must the check-in be online only? | The default in force is what the plan builds: the encrypted outbox (Appendix R WF-WEL-05, SL-WEL-619), in which the answer code waits on the phone until sync, write-only. Until decided, this default contradicts the rule that wellbeing data never reaches a device (master brief Section 20, Appendix M.1). Recommended answer: online only, which changes SL-WEL-619 and WF-WEL-05 | A student without connectivity cannot check in, or a level S answer rests on a shared or lost device. RISK-47, the highest-scoring risk in the register (20). On the decisions workshop agenda; due before the first WF-WEL-05 slice, at the phase 4 exit at the latest | Privacy officer, then the product owner | Open |
| 31 | Does Appendix B gain a kindergarten daily-sheet resource (view, create, edit), and Appendix C a daily-sheet row, so that feature 19 (REQ-ACA-028) can ship? | Yes, as proposed in the Academics sheet open point 1: the amendment is made under an ADR with a version bump of the three briefs. Until it lands, the daily sheet is specified in full (`06-services/academics.md` sections 4.11 and 5.10) but names no permission string, its folder is not deployed, and the guardian notice goes through `RequestNotification` | Feature 19 is gated on the amendment. If it has not landed by phase 2, SL-ACA-209 and SL-ACA-400 to SL-ACA-404 move to phase 5, and the phase 4 reserve demo step R-08 (`TC-ACA-810`) cannot run, which qualifies a signature-feature promise. Answered in writing, before phase 2 starts; no RISK, because the score is 9 | Product owner | Open |

---

## Proposed in v9: default in force until the product owner accepts the record

| Was | Default in force | Record |
|---|---|---|
| Is the seeded `admin` account also created per tenant? | No. Platform super administrator only. A tenant's first administrator always arrives by single-use invitation | ADR-0003, Proposed |
| Is a school group one tenant or several? | One tenant with several campuses. Cross-tenant consolidation is Tier 3 | ADR-0011, Proposed |
| Who owns settings, terminology and custom-field definitions? | Platform owns the definitions; each service owns the values on its own entities | ADR-0009, Proposed |
| Are Assessment and Behavior separate services? | Yes, for load and sensitivity, with a named merge option for a smaller first release | ADR-0002, Proposed |
| Who owns the public API, webhooks and standards? | Platform, as the Integrations capability | ADR-0012, Proposed |
| What does rollback mean? | Redeploy the previous image. The schema is never rolled back | ADR-0010, Proposed |

## Settled in v9.1

ADR-0019 is Accepted, so these four are settled.

| Was | Decision | Record |
|---|---|---|
| Tenant-deletion cooling-off: 7 days (Appendix R, document 13) or 30 days (Section 32, REQ-PLT-007, BR-PLT-003, Appendix J)? | 30 days, export available throughout | ADR-0019 |
| Invitation link validity: 7 days (Appendix J) or 14 days (Appendix R)? | 14 days, with a reminder at day 7 | ADR-0019 |
| Notification deduplication window: 10 minutes (BR-NOT-004) or 5 minutes (Appendix C)? | 5 minutes; urgent templates are never deduplicated | ADR-0019 |
| Does calendar-aware scaling (feature 39) stay a signature feature? | No. It is built and measured as an engineering capability, and leaves the signature list and the demo script | ADR-0019 |

## Settled in v9.7

The product owner answered question 30 on 2026-09-26 with the recommended answer and Accepted ADR-0027, which closes it and RISK-52.

| Was | Decision | Record |
|---|---|---|
| Question 30. Which count bills a tenant: students enrolled on the billing date, prorated by day (master brief Section 36, REQ-PLT-009), or any student enrolled for at least one day of the month (the old BR-FIN-017)? | Students enrolled on the billing date, prorated by day from a mid-month enrollment, and counted for the month in which they leave, as master brief Section 36 states. BR-FIN-017 is rewritten to that definition and owned by Platform, which computes the count (SL-PLT-010); BR-PLT-005's active-student meter is that count; BR-FIN-018, also owned by Platform, applies a downgrade at the next renewal. Brief v9.7 | ADR-0027, Accepted |
