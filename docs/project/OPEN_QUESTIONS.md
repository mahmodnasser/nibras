# Open Questions

Every question has a **default**, so nothing here blocks the plan. Answering changes the default; silence accepts it. The impact column says what it costs to be wrong, which is how to decide what to answer first.

Questions 1, 9 and 13 from v8 are settled and appear at the bottom with the decision record that closed them.

| # | Question | Default assumption | Impact if wrong | Owner | Status |
|---|---|---|---|---|---|
| 1 | .NET 10 or .NET 8? | .NET 10 LTS | Runtime support window, and the fallback list in master brief Section 19 | Product owner | Open |
| 2 | SaaS multi-tenant, on-premises, or both? | Both | Deployment and licensing model, and whether the appliance in `deploy/onprem/` is phase 6 or phase 2 | Product owner | Open |
| 3 | Target countries for the first customers? | Saudi Arabia, United Arab Emirates, Jordan | Tax, e-invoicing plug-ins, regulatory reports, data residency, and which compliance rows in Section 33 are real | Product owner | Open |
| 4 | Which Tier 2 modules matter most for the first release? | None until Tier 1 is complete | Roadmap order after phase 4 | Product owner | Open |
| 5 | Local AI hardware available, or does assist ship later? | Rungs 1 and 2 ship with the product; rung 3 ships later, off by default | Whether the AI service is phase 5 or phase 6. Nothing else, because the product is complete at rung 1 | Product owner | Open |
| 6 | Wolverine, or a thin in-house layer over RabbitMQ.Client? | Wolverine | The messaging building block only. ADR-0004 | Architect | Open |
| 7 | Which services start merged? | Separate, as Appendix L defines them | Service count in phase 1. Merging is a build-time choice that changes no names | Architect | Open |
| 8 | Pricing model and plan limits? | Per active student, with tiers. "Active" is defined in master brief Section 36 | Platform service scope and the metering events | Product owner | Open |
| 9 | Which regional plug-ins first? | Interfaces in Tier 1, implementations per country in phase 5 | Integration work and the partner story | Product owner | Open |
| 10 | Nursery and kindergarten features in the first release? | No. Phase 4 | Daily sheet scope and the admissions age rules | Product owner | Open |
| 11 | Languages beyond English and Arabic? | No, but every string is externalised so a third is configuration | Translation operations and the plural rules | Product owner | Open |
| 12 | Is the name Nibras cleared for trademark, domain and app stores in the target countries? Final taglines? | Use Nibras, keep the name in one configuration value and one token file | Rebranding cost if it must change, and the mobile application identifier | Product owner | Open |
| 13 | OpenIddict with ASP.NET Core Identity, or Keycloak? | OpenIddict. ADR-0008 | The Identity service's shape. Keycloak suits an SSO-heavy customer and changes the service materially | Architect | Open |
| 14 | Is there a Mac build host, or do we buy hosted macOS runner minutes? | Hosted runner minutes, budgeted in master brief Section 30 | iOS cannot be built without one. Android and mobile web are unaffected | Product owner | Open |
| 15 | Hosting provider and regions? | One region to start, chosen with the first customer's residency requirement | Cost per region, and whether residency is a real commitment or a future one | Product owner | Open |
| 16 | Are there Windows hosts among the first on-premises customers? | Yes, served by the Linux virtual machine appliance | Whether `deploy/onprem/` needs a Hyper-V image at phase 2 rather than phase 6 | Product owner | Open |
| 17 | What share of parents use devices without Google services? | Low, but not zero. The fallback in master brief Section 37 covers them | Whether a Huawei push adapter is Tier 2 or Tier 3 | Product owner | Open |
| 18 | Do any target customers require SAML 2.0 or SCIM? | No. OpenID Connect with Google and Microsoft covers phase 1 | Whether enterprise identity moves from Tier 2 into phase 1 | Product owner | Open |
| 19 | Retention periods per country? | The defaults in master brief Section 32 | Storage cost, and a compliance failure if a country mandates longer or shorter | Product owner | Open |
| 20 | Support hours and time zones at launch? | Business hours in the school's time zone, per the tiers in Section 39 | Staffing, and what the contract can promise | Product owner | Open |
| 21 | Which payment gateway first? | Manual and bank transfer by default, with one gateway adapter chosen with the first customer | Per-transaction cost and the reconciliation work | Product owner | Open |
| 22 | Which SMS provider first? | None. Email and push cover everything except the urgent fallback | Per-message cost, and whether the urgent fallback exists at launch | Product owner | Open |
| 23 | Riverpod or flutter_bloc for mobile state? | Riverpod | Mobile structure only. Either satisfies the layering rules | Architect | Open |
| 24 | Team size and shape? | The shape in master brief Section 29: five to eight engineers building slices | Every phase range. `tools/plan-build/schedule-34.mjs` recomputes them from document 34; phase 1 is 14 weeks with eight builders and 22 with five | Product owner | Open |
| 25 | Is there a fixed launch date or an external commitment? | No fixed date; phases are sequenced by dependency | Whether the MVP cut line in Section 28 needs to move | Product owner | Open |
| 26 | Does any first customer require a formal certification, for example SOC 2 or 1EdTech? | No. Compatibility yes, certification only when someone pays for it | Budget and timeline, both material | Product owner | Open |
| 27 | When does a parent receive an absence alert: within 30 seconds of the mark, or 30 minutes after the register closes? | Within 30 seconds, as REQ-ATT-017 and master brief Section 31 require; Appendix R WF-ATT-01 is aligned once decided | A grace window avoids alarming a parent over a teacher's correction; immediacy is what parents expect and what the service levels promise. Changes WF-ATT-01, TC-ATT-003 and the notification lane | Product owner | Open |
| 28 | Do a read-only public API, the OneRoster export and iCal move from Tier 2 into Tier 1? | They stay where the roadmap builds them: iCal in phase 2, the public API and OneRoster in phase 3 under CAP-INT-01, as Tier 2 features | Document 02 found a public API in six of ten competitors; if it is table stakes, a first customer may require it before phase 3. Changes Appendix W row 24 and the MVP cut line | Product owner | Open |

---

## Settled in v9

| Was | Decision | Record |
|---|---|---|
| Is the seeded `admin` account also created per tenant? | No. Platform super administrator only. A tenant's first administrator always arrives by single-use invitation | ADR-0003 |
| Is a school group one tenant or several? | One tenant with several campuses. Cross-tenant consolidation is Tier 3 | ADR-0011 |
| Who owns settings, terminology and custom-field definitions? | Platform owns the definitions; each service owns the values on its own entities | ADR-0009 |
| Are Assessment and Behavior separate services? | Yes, for load and sensitivity, with a named merge option for a smaller first release | ADR-0002 |
| Who owns the public API, webhooks and standards? | Platform, as the Integrations capability | ADR-0012 |
| What does rollback mean? | Redeploy the previous image. The schema is never rolled back | ADR-0010 |

## Settled in v9.1

| Was | Decision | Record |
|---|---|---|
| Tenant-deletion cooling-off: 7 days (Appendix R, document 13) or 30 days (Section 32, REQ-PLT-007, BR-PLT-003, Appendix J)? | 30 days, export available throughout | ADR-0019 |
| Invitation link validity: 7 days (Appendix J) or 14 days (Appendix R)? | 14 days, with a reminder at day 7 | ADR-0019 |
| Notification deduplication window: 10 minutes (BR-NOT-004) or 5 minutes (Appendix C)? | 5 minutes; urgent templates are never deduplicated | ADR-0019 |
| Does calendar-aware scaling (feature 39) stay a signature feature? | No. It is built and measured as an engineering capability, and leaves the signature list and the demo script | ADR-0019 |
