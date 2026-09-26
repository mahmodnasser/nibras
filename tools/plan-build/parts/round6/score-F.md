#### Group F

| Axis | Score | Evidence | What would raise it by one |
|---|---|---|---|
| Completeness | 4 | Assignment and Grade Services now has builders: `34:1060` SL-INT-412 "posts each student's score back through LTI Assignment and Grade Services 2.0", `34:1061` SL-ASM-400, and the command in `11-messaging-architecture.md:315`, `platform.md:560`, `assessment.md:471`. Open Question 31 is in 29's question table (`29:59`) and in §3 (`29:173`). Residual issues. The review records of 22, 24, 25, 26 and 27 end at `22:786` "Amended; awaiting the round 5 score", with no round-5 verdict or round-6 row. `34:1390` still says the command and routes "are to be listed in document 11 section 2.5 … which this part does not edit", though they are listed now, in §2.4. | Add the round-5 verdict row to the records of 22 and 24 to 27. Restate `34:1390` as done, citing document 11 §2.4. |
| Consistency | 4 | Round-5 residue closed: `23:742` "SL-FIN-444 builds only the e-invoicing hand-off and the outage queue in phase 3"; `29:37` "depends on Open Questions 3 and 9". A new split on Open Question 9's default: `OPEN_QUESTIONS.md:19` makes the conditional phase 3 build the default, and so does `34:1385` ("the default of Open Question 9 for e-invoicing"). But `23:742` says the plug-ins move to phase 5 "if open question 9 keeps its phase 5 default", and `34:758` says "its default in force puts country implementations in phase 5". Both name the Phase 2 exit review as the decision point, so this is not blocking. Second, `31:91` puts `Nibras.Finance.Domain.Rules.PlanChangeProrationRule` in Finance, while `31:329` says "the rule class is written in the building service" (Platform). | State one Open Question 9 default in 23 open point 7, `34:758` and `OPEN_QUESTIONS.md:19`. Make the Implementation cells of BR-FIN-017 and BR-FIN-018 agree with the `31:329` default. |
| Feasibility | 4 | The ZATCA estimate is qualified: `34:1385` "**Confidence of the ZATCA estimate: low.** … a reasonable upper bound is about twice the estimate … carried by RISK-53". AGS is estimated: `34:1390` "The two slices add 3 slice-days to phase 4". I reran `schedule-34.mjs`: phase 4 still reads "7 to 12", total "61 to 95 weeks", 1,724 days. Residual issues. The ZATCA upper bound (about 18 days) is not in the phase 3 range. 28's prices remain illustrative (RISK-28). | Carry the ZATCA upper bound as a stated phase 3 sensitivity, for example "phase 3 up to N weeks if ZATCA doubles". |
| Risk honesty | 5 | `31:328` "reviewed by the plan editor, not yet by the business-rules-reviewer agent", scored with an owner. `32:324` "that check rests on search snippets only, because every page fetch was refused". `28:507` open point 12 carries the 72.1/72.6 vCPU difference, and the script reports it as carried. | None needed. |
| Testability | 5 | `28:528` "It is performed by the script `tools/plan-build/capacity-28.mjs`". My run printed "160 figures checked, 0 mismatched, 1 carried as open points". `34:766` SL-FIN-452 is "certified against document 23 §8.3 (`TC-INT-037`)", which is defined at `16-annex:868`. AGS is proved by `TC-INT-036` (`platform.md:2365`) and by the deliver-twice tests `assessment.md:1130` TC-ASM-338 and `:1131` TC-ASM-339. | None needed. |
| Distinctiveness | 4 | `32:324` "Fourteen features are unverified in all ten competitors (1, 2, 15, …) down from twenty-four". I recounted the rows (3 yes, 25 partial, 1 no, 14 unverified), and they agree with `32:341`. Snippet sources are labelled (`32:22`, `02:7`). Residual issue: 14 features are still unverified, and the second check is snippet-only. | Resolve PowerSchool, Classter and Classera by trial or full fetch, as `32:324` proposes. |
| Portability | 4 | Unchanged since round 5: `33:254` "reports the container-runtime check as unavailable on that runner rather than passing it"; `33:252` Open Question 14 is pending. The ADR-0026 prefix agrees: `22:43` "`/bff/web/v1/` and `/bff/mobile/v1/`" matches `appendix-n-load-scenarios.md:93` and `:129`. | Settle Open Question 14, or name a Mac host that runs the container leg of TC-PLAT-800. |

**Verdict:** Approved

| Round-5 gap | Now | Evidence |
|---|---|---|
| AGS in scope (`23:504`) but no slice, use case or route; REQ-ACA-030 cannot pass | Closed | `34:1060` SL-INT-412, `34:1061` SL-ASM-400, `23:504` "built by SL-INT-412 … and SL-ASM-400 … proved by `TC-INT-036`"; `11:315`, `platform.md:560`, `assessment.md:145` |
| `23:742` open point 7 described the old SL-FIN-444 and put the other country at phase 5 | Closed | `23:742` "SL-FIN-444 builds only the e-invoicing hand-off … Both sets are counted in the phase 3 range". This introduced the Open Question 9 default split noted under Consistency |
| Open Question 31 absent from 29 §1 and §3 | Closed | `29:59` "31 Kindergarten daily-sheet permission"; `29:173` in decisions to come |
| ZATCA 9 slice-days with no confidence note | Closed | `34:1385` "Confidence of the ZATCA estimate: low … carried by RISK-53" |
| TC-PERF-800 manual review step | Closed | `28:528` performed by `capacity-28.mjs`; I ran it: 160 checked, 0 mismatched |
| SL-FIN-452 names no test id | Closed | `34:766` "(`TC-INT-037`)"; `16-annex-test-case-registry.md:868` |
| Property-based classification review not done | Partly closed | `31:158` "reviewed rule by rule … 17 below were corrected, which gives 55" (I recounted 55 yes). The business-rules-reviewer agent pass is still pending (`31:328`) |
| `29:37` tied ADR-0025 to Open Question 3 only | Closed | `29:37` "depends on Open Questions 3 and 9 for the conditional e-invoicing slices" |
| Rules built before their owning service had no footnote | Closed | `31:145` to `31:151` note and table. Residue: the implementation cells at `31:90` and `31:91` disagree with the `31:329` default |
| Twenty-four features unverified in all ten (RISK-31) | Partly closed | `32:324` "Fourteen features … down from twenty-four", snippet-only |
| (Portability "Now" residue) macOS container leg rests on Open Question 14 | Open | `33:254` unchanged; stated honestly with a default |

| Gap | Axis | Effort to close | Blocking |
|---|---|---|---|
| Open Question 9's default is stated two ways. `OPEN_QUESTIONS.md:19` and `34:1385` make the conditional phase 3 build the default; `23:742` and `34:758` say the default keeps the plug-ins in phase 5 | Consistency | 15 minutes | no |
| BR-FIN-017 and BR-FIN-018 implementation cells name `Nibras.Finance.Domain.Rules.*` (`31:90`, `31:91`), while `31:329` says the rule class is written in Platform, the building service | Consistency | 15 minutes (gen-31 review list) | no |
| `34:1390` says the `RecordToolScore` command and routes "are to be listed in document 11 section 2.5 … which this part does not edit"; they are already listed, in §2.4 | Consistency, Completeness | 5 minutes | no |
| Review records of 22, 24, 25, 26 and 27 stop at "awaiting the round 5 score" (for example `22:786`), with no round-5 verdict | Completeness | 10 minutes | no |
| The ZATCA upper bound (about twice, `34:1385`) is not shown as a phase 3 range sensitivity | Feasibility | 15 minutes | no |
| The property-based classification still awaits the business-rules-reviewer agent (`31:328`) | Risk honesty | 1 hour | no |
| Fourteen signature features unverified in all ten competitors, and the second check is snippet-only (`32:324`) | Distinctiveness | 2 days | no |
| The macOS container leg of TC-PLAT-800 is unavailable on the hosted runner until Open Question 14 is decided (`33:254`) | Portability | product-owner decision | no |
