#### Group F

| Axis | Score | Evidence | What would raise it by one |
|---|---|---|---|
| Completeness | 4 | The round-6 residue is closed. The review records of 22 and 24 to 27 now carry the round-6 verdict, for example `22:788` "Group F review, round 6 \| Approved with minor gaps". `34:1466` says `RecordToolScore` "is listed in document 11 section 2.4". Residual gap: `34:1352` "Three product-owner decisions are open" has no row for Open Question 31, although that question gates SL-ACA-209 and SL-ACA-400 to 404 (`OPEN_QUESTIONS.md:40`, `34:1396`). | Add Open Question 31 as a fourth row of the `34:1352` table: default, slices, decider and score 9. |
| Consistency | 4 | ADR-0027 is carried through the group. `31:90` "Platform \| `ActiveStudentCountRulesTests` \| `Nibras.Platform.Domain.Rules.ActiveStudentCountRule`" matches Appendix S `:62`. `34:225` "computed here by Platform and nowhere else". `29:41` "Twenty-seven records: twenty-five Proposed … two Accepted (0019 and 0027)". `29:59` shows Open Question 30 as decided. `OPEN_QUESTIONS.md` has 30 rows marked Open. No stale Finance count remains. The Open Question 9 default reads the same in `23:742`, `34:758` and `OPEN_QUESTIONS.md:19`. Residue: `34:1352` names three open decisions that change slices but leaves out Open Question 31, which is itself open (`29:60`). | Same fix as for Completeness. |
| Feasibility | 5 | `34:1384` "at twice the estimate the three ZATCA slices take 18 slice-days … phase 3 reads 12 to 18 weeks … the launch total 62 to 95". I ran `schedule-34.mjs`: phase 3 has 335 days and reads "11 to 18", total "61 to 95". 344 × 1.3 ÷ 40 rounds up to 12, so the sensitivity holds. 28's illustrative prices are owned by RISK-28. | None needed. |
| Risk honesty | 5 | `31:326` "reviewed by the plan editor, not yet by the business-rules-reviewer agent", with owner Architect and score 2. `32:324` "that check rests on search snippets only". RISK-52 is closed by a decision, not by deletion (`18:71` "two Closed (RISK-40, and RISK-52 by ADR-0027)"). | None needed. |
| Testability | 5 | Tooling confirms `capacity-28.mjs` at 160 figures and 0 mismatched. BR-FIN-017 keeps its test class and property flag (`31:90` "`ActiveStudentCountRulesTests` … yes"). `31:164` ties the property reason to "the rounding Appendix S states (ADR-0027)". REQ-PLT-009 maps to `TC-PLT-103, TC-PLT-126, TC-PLT-128` (`20:104`). | None needed. |
| Distinctiveness | 4 | `32:324` "Fourteen features are unverified in all ten competitors (1, 2, 15, … 41) … that check rests on search snippets only". This is unchanged since round 6. Every feature row still names a demo minute and a test, for example `32:34` "Minute 1, TC-RPT-001". | Resolve PowerSchool, Classter and Classera by trial or by fetching the full pages. |
| Portability | 4 | Unchanged: `33:254` "reports the container-runtime check as unavailable on that runner rather than passing it", which rests on Open Question 14 (`OPEN_QUESTIONS.md:24`, Open, with a default and an owner). | Settle Open Question 14, or name a Mac host that runs the container leg of TC-PLAT-800. |

**Verdict:** Approved

| Round-6 gap | Now | Evidence |
|---|---|---|
| Open Question 9's default was stated two ways (`23:742` and `34:758` against `OPEN_QUESTIONS.md:19` and `34:1385`) | Closed | `23:742` "except the e-invoicing plug-in for the country of a VAT-registered first customer, which the Finance owner's team builds in phase 3". `34:758` and `34:1455` say the same. |
| Implementation cells of BR-FIN-017 and BR-FIN-018 named Finance, against the `31:329` Platform default | Closed | `31:90` and `31:91` name `Nibras.Platform.Domain.Rules.*`. Appendix S `:62` and `:63` give both rules the owner Platform (ADR-0027). |
| `34:1390` said `RecordToolScore` "is to be listed in document 11 section 2.5 … which this part does not edit" | Closed | The phrase is gone from 34. `34:1466` "says `RecordToolScore` is listed in document 11 section 2.4". |
| Review records of 22 and 24 to 27 stopped at "awaiting the round 5 score" | Closed | `22:788`, `24:816`, `25:532`, `26:584` and `27:423` each read "Group F review, round 6 \| Approved with minor gaps", followed by a round-7 row. |
| ZATCA upper bound was not shown as a phase 3 sensitivity | Closed | `34:1384` "Sensitivity of the phase 3 range … 12 to 18 weeks instead of 11 to 18". I checked the figure against the `schedule-34.mjs` formula. |
| Property-based classification awaits the business-rules-reviewer agent | Open | `31:326` "not yet by the business-rules-reviewer agent". The point is scored and owned, so it is not a defect. |
| Fourteen features unverified in all ten, and the check is snippet-only | Open | `32:324` is unchanged: "Fourteen features … snippet only". |
| macOS container leg of TC-PLAT-800 rests on Open Question 14 | Open | `33:254` is unchanged. It is stated honestly, with a default and an owner. |

| Gap | Axis | Effort to close | Blocking |
|---|---|---|---|
| `34:1352` says "Three product-owner decisions are open" that change slices, and its table leaves out Open Question 31. That question is open (`OPEN_QUESTIONS.md:40`, `29:60`) and gates SL-ACA-209 and SL-ACA-400 to 404. The slices do state the gate (`34:551`, `34:1396`). | Completeness, Consistency | 10 minutes | no |
| The property-based classification still awaits the business-rules-reviewer agent (`31:326`) | Risk honesty | 1 hour | no |
| Fourteen signature features are unverified in all ten competitors, and the second check is snippet-only (`32:324`) | Distinctiveness | 2 days | no |
| The macOS container leg of TC-PLAT-800 cannot run on the hosted runner until Open Question 14 is decided (`33:254`) | Portability | product-owner decision | no |
