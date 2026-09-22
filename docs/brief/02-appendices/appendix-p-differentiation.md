# Appendix P. Differentiation and Competitive Gap Seed

> Part of `docs/brief/02-appendices/`. Normative as a starting point. Read with the master brief and the reference architecture.

Master brief Section 26 requires a competitive gap analysis in Phase 0. Producing one from memory is how a plan acquires confident errors, so this appendix gives the analysis its structure and its starting claims, and marks every claim as something the plan must verify against current product documentation before `02-competitive-gap-analysis.md` is approved.

**How to use it.** Each claim below is a hypothesis with a date. During Phase 0, check it against the vendor's current public documentation, record the source, and mark it confirmed or corrected. A claim that cannot be verified is written as unverified rather than quietly kept.

**Check of 2026-09-20.** Under ADR-0019 this appendix carries the 21 corrections recorded in `docs/plan/02-competitive-gap-analysis.md`, section "Corrections to Appendix P", which holds the source and check date for each. Where a cell below says "unverified", the check found no public documentation either way; an unverified absence is never written as "no".

---

## P.1 The field

| Product | Shape | Where it is strong | Where a school outgrows it |
|---|---|---|---|
| **openSIS** | Open source (GNU GPL), self-hosted | Free, straightforward student information, long history | Dated interface; no mobile application found (unverified absence); little workflow; multi-school in one installation is claimed, but tenant isolation and row-level security are not documented |
| **Gibbon** | Open source (GNU GPL), self-hosted | Broad module coverage, active community | Interface and mobile, performance at size; finance covers fees and invoicing, with no payroll, ledger or payment gateway found; no application programming interface and no webhooks |
| **Fedena** | Commercial, cloud-hosted on every listed plan; an open-source basic version is available; on-premises unverified | Broad modules, strong in South Asia and the Gulf, competitive price | Depth per module, configurability without code, modern interface |
| **PowerSchool** | Commercial, enterprise | Very deep student information, compliance reporting, ecosystem; a direct competitor in the target region, with a Middle East and Africa edition that has an Arabic, right-to-left interface | Cost, complexity, slow to configure |
| **Classter** | Commercial, hosted; on-premises claimed in vendor marketing, unverified on the product page | Broad, configurable, good integrations, an Arabic language version | Interface density, cost at small scale |
| **Veracross** | Commercial, hosted | Independent schools, one database, strong advancement | Price, fit outside independent schools |
| **ManageBac** | Commercial, hosted | International Baccalaureate curriculum depth, parent experience | Narrow outside its curricula; finance and human resources unverified; Arabic for parents only, not for staff |
| **Blackbaud** | Commercial, enterprise | Fundraising and finance depth | Cost, complexity, integration-heavy |
| **Toddle** | Commercial, hosted | Teaching and learning experience, curriculum planning | Not a full school management system: it covers attendance, timetable, gradebook and reports, and integrates with the school's student information system rather than replacing it; no finance, no Arabic, no offline mode |
| **Classera** | Commercial, hosted | Arabic-first, regional presence, ministry alignment | Configurability; the depth of its finance, human resources and operations suite is not documented publicly and is unverified; its application programming interface is confirmed only by a third-party listing |
| **Ministry-integrated systems** | Mandated, per country | Compliance by definition | Not a school management system; schools run something else alongside |

---

## P.2 Where Nibras is genuinely different

Each row states the claim, who it is aimed at, and the proof that must exist before it may be said out loud.

| Claim | Aimed at | Proof required |
|---|---|---|
| **Arabic and English are equal, not translated.** An Arabic interface is parity: four of the ten products offer one. The difference is the data model: bilingual names, records and documents, Arabic-aware search, Hijri display, and amounts in words in both languages, none of which a competitor documents | Schools in the Gulf and the Levant | Four-way visual snapshots, bilingual PDF baselines, the Arabic normalization rules in Appendix S |
| **Exception-only attendance.** Gate, bus and approved leave pre-fill the register | Teachers, every morning | TC-ATT-003, and the sixty-second measurement in the performance budgets |
| **Everything explains itself.** Any number drills to its records and its rule version; any automated decision shows its reasons and allows an override | School owners and auditors | TC-RPT-007, TC-RPT-008, the rule identifiers in Appendix S |
| **Live in a day.** Smart defaults from country and school type, import with dry run and rollback | The decision maker's biggest fear | TC-PLT-802, TC-DOC-001, the 10,000-row import scenario in Appendix N |
| **Open by default.** Webhooks, iCal, OneRoster, data export always available, and read-only access before suspension. A public API alone is parity: six of the ten products publish one | Schools that have been locked in before | TC-INT-001, and the dunning policy in master brief Section 36 |
| **Permissively licensed, modern, multi-tenant and open source.** openSIS and Gibbon are GNU GPL open source and Fedena has an open-source basic version, so the difference is the licence family and the stack, not open source itself. No licence fee, two named non-open exceptions, every dependency verified | Owners and ministries | The dependency inventory and the licence scan |
| **Works at rung 1.** Every intelligent feature has a deterministic fallback; the product is complete with all models off | Schools without hardware or appetite for artificial intelligence | Appendix W's rung column, and the fallback tests |
| **Safety is a first-class module.** Gate passes, visitor watchlist, emergency mode with reunification, safeguarding with its own retention | Principals and parents | TC-ATT-004, the wellbeing isolation rules |
| **Deploys three ways from one codebase.** Laptop, one server, or Kubernetes | Small schools and groups alike | The three deployment modes, and the appliance drill |
| **A group is one tenant with campuses.** Cross-campus reporting without a cross-tenant query | School groups | The tenancy model in the reference architecture Section 14 |

---

## P.3 What competitors have that this brief does not

This is the half of a gap analysis that teams skip. Each row has a recommendation, and none of them is "add it later" without a tier.

| Gap | Who has it | Recommendation |
|---|---|---|
| Fundraising and advancement | Blackbaud, Veracross | Tier 3. Provide the donor and campaign data model hooks; do not build it |
| Deep North American compliance reporting | PowerSchool | Out of scope until a customer needs it. The regional plug-in interface is the answer, not a built-in |
| Curriculum-specific depth, for example the International Baccalaureate | ManageBac, Toddle | Tier 2 through CASE standards mapping and configurable assessment structures rather than a curriculum-specific module |
| An app and integration marketplace | Classter (confirmed); Veracross and Toddle (partner listings); PowerSchool (unverified) | Tier 2. The plug-in kit and template exchange are the first two steps |
| Cafeteria and prepaid wallet | Unverified: not found on Fedena's or Classter's module pages | Tier 3, in Operations, with the parent wallet as the Tier 2 precursor |
| Alumni and community management | Fedena and Classera (confirmed); Veracross and Blackbaud (unverified) | Tier 3, as a student status plus a profile in School |
| Learning management depth: paths, adaptive content, SCORM | Dedicated learning platforms | Out of scope by decision. Integrate through LTI 1.3 rather than compete |
| Bus GPS tracking with live parent view | Fedena and regional products | Tier 3 in Operations. The delay notification at Tier 2 covers most of the value |
| Formal certification against 1EdTech standards | PowerSchool, Classter | Compatibility in Tier 2, certification only when a customer pays for it. The fee is in master brief Section 30 |
| A mature partner and reseller channel | Fedena, Classter | Tier 3, after the plug-in kit exists |
| Twenty years of edge cases in fee structures | Fedena, Classter | Closed deliberately by Appendix S, which writes the rules down with worked examples instead of discovering them in production |

---

## P.4 The differentiation matrix, per signature feature

`/differentiate` fills one row at a time and writes it here. Feature 39 (calendar-aware scaling) has no row: under ADR-0019 it is an engineering capability in Appendix A, not a signature feature. The columns are fixed so the finished matrix can be read as a sales sheet and as a review artefact.

| Column | Meaning |
|---|---|
| Feature | The number and name from Appendix W |
| Who else has it | Products with a comparable capability, with the source checked |
| Our edge | The specific difference, not an adjective |
| Persona moment | The sentence the user would say |
| Sixty-second proof | The demo step from Appendix O and its test case |
| Risk if we are wrong | What happens if a competitor matches it before launch |

**Seed rows**, to be completed during Phase 0:

| Feature | Who else has it | Our edge | Persona moment | Proof | Risk if wrong |
|---|---|---|---|---|---|
| 26 Exception-only attendance | Partial: Fedena integrates biometric capture; others unverified; none found that pre-fills the register from it | Three sources merged into the register before the teacher opens it, with the conflict rules to make it safe | "The register was already half filled" | Minute 2, TC-ATT-003 | Low. The integration depth is the moat, not the idea |
| 27 Explain this number | Rare. Most show a number and a report | Every figure drills to records and the scheme version that produced it | "I showed the parent exactly why" | Minute 14, TC-RPT-007 | Medium. Easy to imitate shallowly, hard to imitate completely |
| 29 Smart defaults engine | Rare. Most have a setup wizard with empty fields | Country plus school type infers eleven configuration areas and shows what it inferred | "It already knew how our year works" | Minute 15, TC-PLT-802 | Medium |
| 31 Guardian transparency | Very rare: no product found with a guardian-visible access log; an unverified absence pending a product trial | A parent sees which roles read their child's sensitive records | "I can see who looked at my child's file" | Minute 11, TC-AUD-002 | Low. It requires an access log most products do not keep |
| 32 Emergency mode with reunification | Partial: broadcast is built into Blackbaud; roll call and reunification unmatched | Built in, offline-capable, with acknowledgement tracking | "Every child accounted for in four minutes" | Minute 13, TC-ATT-004 | Medium |
| 44 Low-bandwidth mode | Rare: no product page mentions a data-saver mode, and Toddle requires a 3G or better connection; an unverified absence | A data-saver profile designed for the phones parents actually own | "It worked on my old phone" | Minute 4, TC-MOB-005 | Low |

## How this appendix is verified

Before `02-competitive-gap-analysis.md` is approved, every claim in P.1 and P.3 carries a source and a check date, every row in P.2 names a passing test, and every seed row in P.4 is either completed or removed. The domain expert agent reviews it as a school buyer would, and the demo director confirms each sixty-second proof actually fits in sixty seconds.
