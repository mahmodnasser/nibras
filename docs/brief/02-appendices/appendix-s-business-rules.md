# Appendix S. Business Rules Catalog

> Part of `docs/brief/02-appendices/`. Normative. Read with the master brief and the reference architecture.

This appendix makes the product's arithmetic and policy unambiguous. Every rule below is written so that it can be lifted directly into a **table-driven unit test**: the worked examples are the rows of the test table, and the expected values in them are the assertions. A rule whose examples cannot be turned into a passing test is a defect in this appendix, not in the code.

Three conventions hold everywhere.

- **Each rule names one owning service.** The owner is taken from the service registry in Appendix L and nothing else computes the rule. A consumer that re-implements a calculation owned by another service is a defect.
- **Each rule names its parameters.** Parameters are settings from Appendix G. A rule with no parameters is a constant of the product and must not be made configurable without an ADR.
- **Each rule names its test class.** The class lives in the owning service's unit test project, and it is table-driven over the examples given here. Rules that compute money, percentages, weights or rank additionally get **property-based tests**: shares always sum to the total, rounding is applied exactly once, re-weighting preserves the 100% envelope, allocation never creates or destroys value, and a calculation re-run on stored inputs reproduces the stored output.

Rule identifiers follow Appendix L: `BR-<AREA>-<NNN>`, unique across the file, numbered from 001 within each area. The Requests service uses the area code `RQS`.

---

## Index

| ID | Name | Owner | Parameters |
|---|---|---|---|
| BR-ASM-001 | Weighted category average | Assessment | Academic → grading schemes |
| BR-ASM-002 | Empty category re-weight | Assessment | Academic → grading schemes |
| BR-ASM-003 | Drop the lowest scores | Assessment | Academic → grading schemes |
| BR-ASM-004 | Best of N | Assessment | Academic → grading schemes |
| BR-ASM-005 | Mandatory component missing | Assessment | Academic → grading schemes |
| BR-ASM-006 | Late joiner partial-term re-weighting | Assessment | Academic → grading schemes |
| BR-ASM-007 | Absent versus exempt components | Assessment | Academic → grading schemes |
| BR-ASM-008 | Rounding at a configurable number of decimals | Assessment | Academic → rounding |
| BR-ASM-009 | Letter grade boundaries | Assessment | Academic → grading schemes |
| BR-ASM-010 | GPA scale conversion | Assessment | Academic → grading schemes |
| BR-ASM-011 | Rank and ties | Assessment | Academic → rank visibility |
| BR-ASM-012 | Promotion eligibility | Assessment | Academic → pass marks, promotion rules |
| BR-ASM-013 | Honors thresholds | Assessment | Academic → promotion rules |
| BR-ASM-014 | Grade change after lock creates a new version | Assessment | Academic → publish windows |
| BR-ATT-001 | Daily versus per-period derivation | Attendance | Attendance → mode, codes |
| BR-ATT-002 | Attendance lock window | Attendance | Attendance → lock window |
| BR-ATT-003 | Approved leave pre-fills excused | Attendance | Attendance → excuse rules, codes |
| BR-ATT-004 | Late converts to absent | Attendance | Attendance → cut-off times, codes |
| BR-ATT-005 | Late accumulation adds a derived absence | Attendance | Attendance → thresholds and ladder |
| BR-ATT-006 | Consecutive absence threshold | Attendance | Attendance → thresholds and ladder |
| BR-ATT-007 | Cumulative absence ladder | Attendance | Attendance → thresholds and ladder |
| BR-ATT-008 | Attendance percentage denominator | Attendance | Attendance → excuse rules |
| BR-ATT-009 | Mid-term section move splits the record | Attendance | none |
| BR-ATT-010 | Offline mark arriving after the lock window | Attendance | Attendance → lock window |
| BR-ATT-011 | Unmarked class reminder | Attendance | Attendance → cut-off times |
| BR-FIN-001 | Fee plan installment generation | Finance | General → currency |
| BR-FIN-002 | Pro-rata by days | Finance | Finance → pro-rata mode |
| BR-FIN-003 | Pro-rata by months | Finance | Finance → pro-rata mode |
| BR-FIN-004 | Discount stacking order | Finance | Finance → discount rules |
| BR-FIN-005 | Sibling discount eligibility | Finance | Finance → discount rules |
| BR-FIN-006 | Scholarship cap | Finance | Finance → discount rules |
| BR-FIN-007 | Late fee accrual and cap | Finance | Finance → late fee rules |
| BR-FIN-008 | Payment allocation order | Finance | Finance → allocation order |
| BR-FIN-009 | Overpayment becomes credit | Finance | Finance → allocation order |
| BR-FIN-010 | Refund from credit versus from payment | Finance | Finance → payment methods |
| BR-FIN-011 | Rounding per currency | Finance | General → currency |
| BR-FIN-012 | Tax inclusive versus exclusive per item | Finance | Finance → tax |
| BR-FIN-013 | Gapless numbering per series under concurrency | Finance | Finance → numbering series |
| BR-FIN-014 | Posted documents are immutable | Finance | none |
| BR-FIN-015 | Cheque bounce reversal and fee | Finance | Finance → payment methods, late fee rules |
| BR-FIN-016 | Service restriction rules | Finance | Finance → restriction rules |
| BR-FIN-017 | Active student definition for SaaS billing | Finance | none |
| BR-FIN-018 | Proration on a plan change | Finance | none |
| BR-FIN-019 | Split payers by percentage | Finance | General → currency |
| BR-SCD-001 | Hard versus soft constraints | Scheduling | none |
| BR-SCD-002 | Consecutive-period limit | Scheduling | General → work week |
| BR-SCD-003 | Part-time availability and weekly load | Scheduling | none |
| BR-SCD-004 | Travel time between campuses | Scheduling | none |
| BR-SCD-005 | Cover fairness score | Scheduling | none |
| BR-SCD-006 | Publishing does not alter recorded attendance | Scheduling | Attendance → lock window |
| BR-SCD-007 | Room booking holds and buffers | Scheduling | none |
| BR-ADM-001 | Age eligibility by cut-off date | Admissions | General → calendars |
| BR-ADM-002 | Required documents by grade and nationality | Admissions | none |
| BR-ADM-003 | Seat capacity and override | Admissions | none |
| BR-ADM-004 | Offer expiry | Admissions | General → time zone |
| BR-ADM-005 | Waiting-list ranking with sibling priority | Admissions | none |
| BR-ADM-006 | Duplicate applicant detection | Admissions | General → languages |
| BR-IDN-001 | Permission dependency | Identity | none |
| BR-IDN-002 | Data-scope evaluation order | Identity | none |
| BR-IDN-003 | Delegation validity window | Identity | General → time zone |
| BR-IDN-004 | Four-eyes for high-risk grants | Identity | Security → high-risk grant approval window |
| BR-IDN-005 | The last super administrator cannot be removed | Identity | none |
| BR-IDN-006 | Join-method default role | Identity | Joining → enabled methods, default roles |
| BR-IDN-007 | One person linked across tenants | Identity | Security → login methods |
| BR-IDN-008 | Permission version invalidates caches | Identity | none |
| BR-IDN-009 | Impersonation needs consent and a time box | Identity | Security → session timeout |
| BR-NOT-001 | Urgency versus quiet hours | Notification | Notifications → quiet hours default |
| BR-NOT-002 | Channel fallback order | Notification | Notifications → channel availability |
| BR-NOT-003 | Digest eligibility | Notification | Notifications → digest schedule |
| BR-NOT-004 | Deduplication window | Notification | Notifications → templates |
| BR-NOT-005 | SMS credit check before send | Notification | Notifications → SMS credit limits |
| BR-NOT-006 | Preference resolution order | Notification | Notifications → channel availability |
| BR-RQS-001 | Approval chain routing by amount | Requests | Requests → approval chains |
| BR-RQS-002 | Approval chain routing by duration | Requests | Requests → approval chains |
| BR-RQS-003 | SLA calendars per campus | Requests | Requests → SLAs |
| BR-RQS-004 | Auto-approval conditions | Requests | Requests → approval chains |
| BR-RQS-005 | Escalation on breach | Requests | Requests → SLAs |
| BR-RQS-006 | Effect execution and compensation | Requests | Requests → fees |
| BR-WEL-001 | Visibility levels | Wellbeing | none |
| BR-WEL-002 | Break-glass access | Wellbeing | Security → session timeout |
| BR-WEL-003 | Events carry no clinical detail | Wellbeing | none |
| BR-WEL-004 | Medication authorization and sending home | Wellbeing | none |
| BR-PLT-001 | Soft warn then hard block on plan limits | Platform | none |
| BR-PLT-002 | What read-only mode allows | Platform | none |
| BR-PLT-003 | Deletion cooling-off | Platform | Security → retention periods |
| BR-PLT-004 | Data residency pinning at provisioning | Platform | none |
| BR-PLT-005 | Usage metering counts | Platform | none |
| BR-PLT-006 | Tenant export completeness | Platform | Security → export approval rules |
| BR-L10N-001 | Arabic search normalization | Platform | General → languages |
| BR-L10N-002 | Numeral rendering | Platform | General → numerals |
| BR-L10N-003 | Hijri display, Gregorian source of truth | Scheduling | General → calendars, time zone |
| BR-L10N-004 | Amounts in words in both languages | Finance | Finance → receipt layout |
| BR-L10N-005 | Arabic plural forms | Notification | Notifications → templates |
| BR-L10N-006 | Pinned culture on every host | Platform | none |
| BR-L10N-007 | Bilingual names and fallback | School | General → languages |

---

## S.1 Grading and assessment (ASM)

### BR-ASM-001 Weighted category average

**Owner:** Assessment · **Parameters:** Academic → grading schemes

**Tests:** `WeightedCategoryAverageRulesTests`

**Rule.** A subject's grading-period percentage is the sum over categories of the category percentage multiplied by the category weight, where a category percentage is the total earned marks of its counted components divided by the total maximum marks of those components. Category weights in a saved assessment structure must total exactly 100.

**Examples.**

- Given Homework at weight 20 with 18/20, Quizzes at weight 30 with 42/50 and Exam at weight 50 with 76/100, when the term total is computed, then the result is 0.90 × 20 + 0.84 × 30 + 0.76 × 50 = 18.0 + 25.2 + 38.0 = 81.2%.
- Given a single Exam category at weight 100 with 45/60, when the term total is computed, then the result is 45 ÷ 60 = 75.0%.
- Given a structure whose weights are 20, 30 and 40, when the structure is saved, then it is rejected because the weights total 90 and not 100.

**Edge cases.**

- A category with several components must be summed on marks, never averaged on per-component percentages; see BR-ASM-008.
- A component with a maximum mark of zero is invalid and is rejected at structure save, because it would divide by zero.
- Weights are stored on the scheme version used by the result, so editing the structure later never silently changes a published result.

### BR-ASM-002 Empty category re-weight

**Owner:** Assessment · **Parameters:** Academic → grading schemes

**Tests:** `CategoryReweightRulesTests`

**Rule.** A category with no counted components is removed from the calculation and the remaining category weights are re-normalized proportionally so that they again total 100. If every category is empty, the subject result is "Not yet assessed" and no percentage is stored.

**Examples.**

- Given Homework at weight 20 with no components, Quizzes at weight 30 with 42/50 and Exam at weight 50 with 76/100, when the term total is computed, then the remaining weights become 30 ÷ 80 = 37.5 and 50 ÷ 80 = 62.5, and the result is 84 × 0.375 + 76 × 0.625 = 31.5 + 47.5 = 79.0%.
- Given Homework at weight 20 with 10/10, Quizzes at weight 30 with no components and Exam at weight 50 with 40/50, when the term total is computed, then the remaining weights become 20 ÷ 70 and 50 ÷ 70, and the result is 100 × 0.285714 + 80 × 0.714286 = 28.5714 + 57.1429 = 85.71%.
- Given every category empty, when the term total is computed, then the subject result is "Not yet assessed", no percentage, letter or GPA point is stored, and the subject is excluded from the term average.

**Edge cases.**

- A category whose only components are all exempt is empty for this rule; a category whose only components are absent is not empty, because absent counts as zero.
- Re-normalization is applied once over the surviving categories, never iteratively.
- An empty category still appears on the report card with a dash, so a parent can see that it was not dropped silently.

### BR-ASM-003 Drop the lowest scores

**Owner:** Assessment · **Parameters:** Academic → grading schemes

**Tests:** `DropLowestRulesTests`

**Rule.** When a category is configured to drop the N lowest results, the dropped components are the N droppable components with the lowest percentage of their own maximum mark, chosen after exempt components are removed. A component marked mandatory is never droppable.

**Examples.**

- Given a Quizzes category with drop-lowest 1 and results 8/10, 6/10, 9/10 and 7/10, when the category percentage is computed, then 6/10 is dropped and the result is (8 + 9 + 7) ÷ 30 = 24/30 = 80.0%.
- Given results 15/20 (75%), 8/10 (80%) and 9/10 (90%) with drop-lowest 1, when the category percentage is computed, then 15/20 is dropped because it has the lowest percentage, and the result is (8 + 9) ÷ 20 = 17/20 = 85.0%.
- Given results 4/10 marked mandatory, 6/10 and 9/10 with drop-lowest 1, when the category percentage is computed, then 6/10 is dropped because the mandatory component is not droppable, and the result is (4 + 9) ÷ 20 = 13/20 = 65.0%.

**Edge cases.**

- Components carry different maximum marks, so the lowest must be chosen on percentage, not on raw mark.
- When N is greater than or equal to the number of droppable components, all droppable components are dropped and only mandatory components remain; if that leaves nothing, the category is empty and BR-ASM-002 applies.
- A tie for the lowest percentage is broken by the earlier due date, so the calculation is deterministic and reproducible.

### BR-ASM-004 Best of N

**Owner:** Assessment · **Parameters:** Academic → grading schemes

**Tests:** `BestOfNRulesTests`

**Rule.** When a category is configured best-of-N, only the N counted components with the highest percentage of their own maximum mark contribute to the category. When fewer than N counted components exist, all of them contribute.

**Examples.**

- Given best-of-2 and results 7/10, 9/10 and 5/10, when the category percentage is computed, then 9/10 and 7/10 count and the result is 16/20 = 80.0%.
- Given best-of-3 and only two results 12/20 and 18/20, when the category percentage is computed, then both count and the result is 30/40 = 75.0%.
- Given best-of-2 and results 8/10, 8/10 and 6/10, when the category percentage is computed, then the tie at 80% is broken by the earlier due date, and the result is 16/20 = 80.0%.

**Edge cases.**

- Best-of-N and drop-lowest are mutually exclusive on one category; a structure that sets both is rejected at save.
- An absent component has a percentage of zero and is therefore a candidate to be left out, which is the intended behaviour for a makeup-friendly policy.
- Mandatory components always count and are outside the best-of selection.

### BR-ASM-005 Mandatory component missing

**Owner:** Assessment · **Parameters:** Academic → grading schemes

**Tests:** `MandatoryComponentRulesTests`

**Rule.** If a component marked mandatory has no mark and is not exempt, the subject result is Incomplete: no percentage, letter or GPA point is published for that subject and the subject is excluded from the term average and from rank.

**Examples.**

- Given a mandatory Final Exam with no mark and every other component marked, when the term result is calculated, then the subject result is Incomplete and the report card prints "I" for that subject.
- Given the same subject where the Final Exam is later marked 55/100, when recalculation runs, then the Incomplete clears and the subject publishes a percentage.
- Given the mandatory Final Exam set to exempt with the reason "medical", when the term result is calculated, then the component leaves the calculation, the remaining components re-weight under BR-ASM-002 and the subject publishes a percentage.

**Edge cases.**

- An Incomplete subject must not be silently treated as zero; a zero and an Incomplete are different states with different report-card output.
- A student with an Incomplete subject is unranked under BR-ASM-011 and ineligible for honors under BR-ASM-013 until it clears.
- Clearing an Incomplete after the grading period is locked goes through BR-ASM-014 and issues a new report card version.

### BR-ASM-006 Late joiner partial-term re-weighting

**Owner:** Assessment · **Parameters:** Academic → grading schemes

**Tests:** `LateJoinerReweightRulesTests`

**Rule.** A student enrolled after a grading period starts is excluded from every component whose due date falls strictly before the enrollment date. Excluded components leave both the numerator and the denominator of their category, and an emptied category is removed and re-weighted under BR-ASM-002.

**Examples.**

- Given quizzes due 2026-09-10, 2026-09-24 and 2026-10-08, a student enrolled 2026-09-20 with marks 7/10 and 9/10 for the last two, when the Quizzes category is computed, then the 10 September quiz is excluded and the category is 16/20 = 80.0%.
- Given the same quizzes and a student enrolled 2026-09-09, when the Quizzes category is computed, then no component is excluded, all three count and the denominator is 30.
- Given a student enrolled 2026-11-01 after every component in the Quizzes category was due, when the term total is computed, then the Quizzes category is empty, it is removed, and the remaining categories re-normalize to total 100.

**Edge cases.**

- A student who leaves mid-term is treated the same way in the opposite direction: components due strictly after the withdrawal date are excluded.
- Re-weighting a late joiner must not push a category weight above 100 or below 0; the property-based test asserts that the surviving weights always total 100.
- Enrollment date, not application date and not the first attendance mark, is the boundary.

### BR-ASM-007 Absent versus exempt components

**Owner:** Assessment · **Parameters:** Academic → grading schemes

**Tests:** `AbsentAndExemptComponentRulesTests`

**Rule.** An absent component counts as zero earned against its full maximum mark until a makeup mark replaces it. An exempt component is removed from both the numerator and the denominator.

**Examples.**

- Given a category of two quizzes with 8/10 and one absent, when the category percentage is computed, then the absent quiz contributes 0/10 and the result is 8/20 = 40.0%.
- Given the same two quizzes where the second is exempt rather than absent, when the category percentage is computed, then the exempt quiz leaves the calculation and the result is 8/10 = 80.0%.
- Given the absent quiz later receiving a makeup mark of 7/10, when recalculation runs, then the result becomes 15/20 = 75.0%.

**Edge cases.**

- Exempt is a decision that needs a reason and is audited; absent is a fact derived from attendance and is not.
- An absent mandatory component is not Incomplete: it is a zero. Incomplete means no mark at all.
- Marking every component of a category exempt empties the category and triggers BR-ASM-002.

### BR-ASM-008 Rounding at a configurable number of decimals

**Owner:** Assessment · **Parameters:** Academic → rounding

**Tests:** `GradeRoundingRulesTests`

**Rule.** Percentages are computed at full decimal precision and rounded exactly once, at the end, half away from zero, to the configured number of decimals. Intermediate category percentages, component percentages and weighted contributions are never rounded.

**Examples.**

- Given a rounding setting of 2 decimals and a raw weighted total of 81.235%, when the result is published, then the published value is 81.24%.
- Given a rounding setting of 0 decimals and a raw weighted total of 74.5%, when the result is published, then the published value is 75%.
- Given a category with components 7/9 and 5/7 and a rounding setting of 2 decimals, when the category percentage is computed, then it is 12 ÷ 16 = 75.00%, and not the average of the rounded component percentages 77.78% and 71.43%, which would give 74.61%.

**Edge cases.**

- Half away from zero and banker's rounding disagree at exactly 0.5 in the last kept digit; this product uses half away from zero everywhere, in the database, the API and the report card renderer.
- The rounding setting applies to display and to the stored published value alike, so the stored value and the printed value can never disagree.
- Rank is computed on the unrounded total under BR-ASM-011, so rounding never creates or removes a tie.

### BR-ASM-009 Letter grade boundaries

**Owner:** Assessment · **Parameters:** Academic → grading schemes

**Tests:** `LetterGradeBoundaryRulesTests`

**Rule.** A letter grade is assigned from the published, rounded percentage using half-open bands of the form lower inclusive to upper exclusive, ordered descending, with the top band closed at 100. The scheme version stored on the result decides the bands.

**Examples.**

- Given the bands A+ 95 to 100, A 90 to 95, B+ 85 to 90, B 80 to 85, C+ 75 to 80, C 70 to 75, D 60 to 70 and F 0 to 60, and a published 89.99%, when the letter is assigned, then it is B+.
- Given the same bands and a published 90.00%, when the letter is assigned, then it is A.
- Given the same bands and a published 100.00%, when the letter is assigned, then it is A+; and given a published 59.99%, then it is F.

**Edge cases.**

- A percentage above 100, which bonus marks can produce, maps to the top band and is recorded with the raw value preserved.
- Bands must cover 0 to 100 with no gap and no overlap; a scheme that leaves a gap is rejected at save.
- The letter is derived from the rounded percentage, so a raw 89.996% rounded to 90.00% at two decimals is an A, not a B+.

### BR-ASM-010 GPA scale conversion

**Owner:** Assessment · **Parameters:** Academic → grading schemes

**Tests:** `GpaConversionRulesTests`

**Rule.** A GPA point comes from the letter grade band, never from re-deriving points out of the percentage. The term GPA is the credit-weighted mean of the subject points, rounded half away from zero to two decimals.

**Examples.**

- Given points A+ = 4.0, A = 3.7, B+ = 3.3 and B = 3.0, and subjects Mathematics (A+, 4 credits), English (B+, 3 credits) and Art (B, 1 credit), when the term GPA is computed, then it is (4.0 × 4 + 3.3 × 3 + 3.0 × 1) ÷ 8 = 28.9 ÷ 8 = 3.6125, published as 3.61.
- Given two subjects of 1 credit each with grades A (3.7) and B (3.0), when the term GPA is computed, then it is 6.7 ÷ 2 = 3.35.
- Given Mathematics (A+, 4 credits) and English at Incomplete (3 credits), when the term GPA is computed, then English is excluded from both numerator and denominator and the GPA is 16.0 ÷ 4 = 4.00.

**Edge cases.**

- A pass or fail subject carries no GPA point and is excluded from the credit denominator, but it still counts for promotion under BR-ASM-012.
- Credits are stored on the subject for the academic year, so changing next year's credits never moves a published GPA.
- The cumulative GPA is the credit-weighted mean over all published terms, not the mean of the term GPAs.

### BR-ASM-011 Rank and ties

**Owner:** Assessment · **Parameters:** Academic → rank visibility

**Tests:** `RankRulesTests`

**Rule.** Rank inside a section or a grade is computed on the unrounded weighted total using standard competition ranking, so equal totals share a rank and the next rank skips the number of tied students. Rank is always stored and is shown only where the rank visibility setting permits.

**Examples.**

- Given unrounded totals 92.4, 88.1, 88.1 and 85.0, when rank is computed, then the ranks are 1, 2, 2 and 4.
- Given two students whose published values are both 88.10% but whose unrounded totals are 88.104 and 88.095, when rank is computed, then the ranks are 2 and 3 and not a tie.
- Given four students with totals 92.4 (complete), 90.0 (Incomplete), 88.1 (complete) and 85.0 (complete), when rank is computed, then the Incomplete student is unranked and does not consume a position, so the ranks are 1, 2 and 3.

**Edge cases.**

- Rank visibility off means the value is computed and stored but is never returned by any API a parent or student can reach.
- A recalculation after a grade change re-ranks the whole cohort, and the previous rank stays on the superseded report card version.
- Grade-level rank and section rank are two independent computations and can disagree; both are stored.

### BR-ASM-012 Promotion eligibility

**Owner:** Assessment · **Parameters:** Academic → pass marks, promotion rules

**Tests:** `PromotionEligibilityRulesTests`

**Rule.** A student is eligible for promotion when all three conditions hold: the year average is at or above the pass mark, the number of subjects below the subject pass mark is at or below the configured maximum, and the year attendance percentage is at or above the promotion attendance floor. Failing any condition records the specific reason.

**Examples.**

- Given a pass mark of 60%, a maximum of 2 failed subjects, an attendance floor of 75%, and a student with a year average of 68.0%, 2 failed subjects and 81.2% attendance, when eligibility is evaluated, then the student is eligible.
- Given the same settings and a student with a year average of 68.0%, 3 failed subjects and 90.0% attendance, when eligibility is evaluated, then the student is not eligible and the reason stored is "failed subject count 3 exceeds the maximum 2".
- Given the same settings and a student with a year average of 59.9%, 0 failed subjects and 95.0% attendance, when eligibility is evaluated, then the student is not eligible and the reason stored is "year average 59.9% is below the pass mark 60%".

**Edge cases.**

- Attendance comes from the Attendance service under BR-ATT-008 and is read as a published figure, never recomputed inside Assessment.
- An Incomplete subject blocks evaluation entirely; eligibility is "pending" until every subject resolves.
- Eligibility is a recommendation. The actual promotion is a student status change and therefore a workflow, never a field edit.

### BR-ASM-013 Honors thresholds

**Owner:** Assessment · **Parameters:** Academic → promotion rules

**Tests:** `HonorsThresholdRulesTests`

**Rule.** Honors bands are evaluated only after the year result is published, against the published value the scheme names (year average or cumulative GPA), using lower-inclusive thresholds. A student carrying a conduct flag configured as disqualifying is excluded from every band.

**Examples.**

- Given bands High Distinction from 95.0%, Distinction from 90.0% and Merit from 85.0%, and a published year average of 90.0%, when honors are evaluated, then the student is listed as Distinction.
- Given the same bands and a published 94.99% that was rounded from a raw 94.994%, when honors are evaluated, then the student is Distinction and not High Distinction, because the published value decides.
- Given the same bands, a published 96.2% and a disqualifying conduct flag recorded on 2027-03-11, when honors are evaluated, then the student appears on no honors list and the exclusion reason is stored on the evaluation.

**Edge cases.**

- Honors lists are recomputed when a grade change issues a new report card version, and a student can leave a list as well as join one.
- A student with any Incomplete subject is not evaluated until it clears.
- Bands are lower-inclusive, so exactly 85.0% is Merit and 84.99% is nothing.

### BR-ASM-014 Grade change after lock creates a new version

**Owner:** Assessment · **Parameters:** Academic → publish windows

**Tests:** `GradeChangeAfterLockRulesTests`

**Rule.** Once a grading period is locked, a mark can change only through an approved grade-change request holding `assessment.marks.change-after-lock`. Publishing the corrected result issues report card version n + 1; version n stays retrievable and immutable.

**Examples.**

- Given a locked term and a published report card version 1 showing Mathematics 74%, when a grade-change request to 79% is approved on 2027-02-03 and republished, then version 2 shows Mathematics 79% and version 1 remains retrievable unchanged.
- Given the same locked term and a teacher attempting a direct mark edit, when the save is submitted, then it is rejected with `ASSESSMENT_MARKS_LOCKED` and no version is created.
- Given an approved change whose effect leaves every published value identical after rounding, for example 74.4% to 74.4%, when republication runs, then no new version is issued and the request closes as "no material change".

**Edge cases.**

- A new version re-runs rank, honors and promotion eligibility for the affected cohort, so a single mark change can move other students' ranks.
- Guardian acknowledgment is reset on the new version; an acknowledgment of version 1 does not carry to version 2.
- The QR verification code resolves to the latest version and states which version it is, so a printed version 1 can be identified as superseded.

---

## S.2 Attendance (ATT)

### BR-ATT-001 Daily versus per-period derivation

**Owner:** Attendance · **Parameters:** Attendance → mode, codes

**Tests:** `AttendanceDerivationRulesTests`

**Rule.** In per-period mode the day status is derived from the period marks: the day is absent when the count of absent periods reaches the configured full-day threshold, otherwise present with the period exceptions recorded. In daily mode a single mark is the day status and no period marks are stored.

**Examples.**

- Given per-period mode, 7 periods a day, a full-day threshold of 4 absent periods and a student absent in 4 periods, when the day is derived, then the day status is absent.
- Given the same settings and a student absent in 3 periods, when the day is derived, then the day status is present with 3 period absences recorded.
- Given daily mode and a teacher opening a period register, when the register loads, then it is read-only, no period marks can be written and the day mark is the only editable value.

**Edge cases.**

- Switching mode mid-year applies from the switch date forward; historical days keep the derivation that produced them and are never recomputed.
- Periods that the bell schedule marks as non-teaching are outside the threshold count.
- A day with no periods scheduled, such as a campus holiday, produces no day record at all rather than a present record.

### BR-ATT-002 Attendance lock window

**Owner:** Attendance · **Parameters:** Attendance → lock window

**Tests:** `AttendanceLockWindowRulesTests`

**Rule.** A session's marks are freely editable until the lock window elapses, measured from the session end time in the campus time zone. After that, an edit requires `attendance.student-attendance.edit-after-lock` plus a reason, and the previous value is kept in history.

**Examples.**

- Given a lock window of 48 hours and a period that ended 2026-10-05 09:40 Asia/Riyadh, when a teacher saves a mark at 2026-10-07 09:39 Asia/Riyadh, then the save succeeds.
- Given the same session, when the teacher saves at 2026-10-07 09:41 Asia/Riyadh, then the save is rejected with `ATTENDANCE_SESSION_LOCKED`.
- Given the same session and a deputy holding `attendance.student-attendance.edit-after-lock` saving at 2026-10-09 12:00 with the reason "paper register reconciliation", then the change is written, the previous value is retained and an audit entry records both values, the actor and the reason.

**Edge cases.**

- The window is measured from session end, not session start and not the day, so a late-afternoon period locks later than a morning one on the same day.
- Workflow effects such as an approved excuse bypass the lock under BR-ATT-003; a human edit never does.
- The campus time zone is used, so a group with campuses in Riyadh and Dubai locks its sessions at different absolute instants.

### BR-ATT-003 Approved leave pre-fills excused

**Owner:** Attendance · **Parameters:** Attendance → excuse rules, codes

**Tests:** `ApprovedLeaveExcuseRulesTests`

**Rule.** Approving a leave or excuse request sets the configured excused code on every attendance session in the requested date range. It overwrites absent and late marks, never overwrites present, and applies even to sessions past the lock window because it is a workflow effect and not a human edit.

**Examples.**

- Given approved sick leave from 2026-11-02 to 2026-11-04 with existing marks absent on 2 November, absent on 3 November and no mark on 4 November, when the approval effect runs, then all three days read excused.
- Given approved leave covering 2026-11-05 where the student was already marked present, when the approval effect runs, then 5 November stays present and a note records the conflict for the registrar.
- Given leave approved on 2026-11-10 for dates whose lock window has already closed, when the approval effect runs, then the excused code is still applied and the audit entry names the approved request as the actor.

**Edge cases.**

- Partial-day leave in per-period mode sets only the periods inside the requested hours, and the day status is re-derived under BR-ATT-001.
- A leave later cancelled restores the previous marks from history rather than setting everything to present.
- Whether excused days count as present for the percentage is a separate setting; see BR-ATT-008.

### BR-ATT-004 Late converts to absent

**Owner:** Attendance · **Parameters:** Attendance → cut-off times, codes

**Tests:** `LateToAbsentRulesTests`

**Rule.** A late mark becomes an absent mark when the recorded arrival time is at or after the session start plus the configured late-to-absent cut-off. The arrival time is kept on the converted record.

**Examples.**

- Given a cut-off of 20 minutes, a period starting 07:45 and an arrival recorded at 08:04, when the mark is saved, then it stays late.
- Given the same cut-off and period and an arrival recorded at 08:05, when the mark is saved, then the mark becomes absent and the arrival time 08:05 is kept on the record.
- Given the cut-off rule switched off in settings and an arrival at 09:30 for a period starting 07:45, when the mark is saved, then it stays late and no conversion happens.

**Edge cases.**

- A converted mark is still an arrival: the gate log, the guardian notification and the late-accumulation counter in BR-ATT-005 all still see a late arrival.
- Conversion runs at save time, so changing the cut-off later does not retroactively convert historical marks.
- A student marked late with no arrival time recorded cannot be converted and is flagged as a data-quality issue.

### BR-ATT-005 Late accumulation adds a derived absence

**Owner:** Attendance · **Parameters:** Attendance → thresholds and ladder

**Tests:** `LateAccumulationRulesTests`

**Rule.** When late accumulation is enabled, every N recorded lates in a grading period add one derived absence to the attendance statistics. The underlying marks are never changed, and the derived absences are recomputed from the live late count.

**Examples.**

- Given N = 3 and 7 lates recorded in the term, when the statistics are computed, then 2 derived absences are added, because 7 ÷ 3 is 2 with a remainder of 1, and 1 late remains uncounted.
- Given N = 3 and 2 lates recorded, when the statistics are computed, then no derived absence is added.
- Given N = 3, 6 lates recorded and one of them later corrected to present, when the statistics are recomputed, then the derived absences fall from 2 to 1, because 5 ÷ 3 is 1.

**Edge cases.**

- Derived absences count toward the cumulative ladder in BR-ATT-007 only if the setting says so; they never count toward the consecutive run in BR-ATT-006.
- The counter resets at the start of each grading period, so a remainder does not carry across terms.
- The register and the report card must both show the raw late count and the derived absences separately, never a merged figure.

### BR-ATT-006 Consecutive absence threshold

**Owner:** Attendance · **Parameters:** Attendance → thresholds and ladder

**Tests:** `ConsecutiveAbsenceRulesTests`

**Rule.** The consecutive absence counter counts unexcused absent school days in sequence, skips non-school days entirely, and resets to zero on any mark that is not an unexcused absence. Reaching the configured threshold publishes `attendance.threshold.reached.v1` once for that run.

**Examples.**

- Given a threshold of 3, a Sunday-to-Thursday work week and unexcused absences on Monday 2026-10-05, Tuesday 2026-10-06 and Wednesday 2026-10-07, when 7 October is marked, then the run reaches 3 and the threshold event fires on 7 October.
- Given the same threshold and unexcused absences on Wednesday 2026-10-07, Thursday 2026-10-08 and the next school day Sunday 2026-10-11, when 11 October is marked, then the run reaches 3 and the event fires, because Friday 9 October and Saturday 10 October are not school days and do not break the run.
- Given unexcused absences on 2026-10-05 and 2026-10-06 followed by an excused mark on 2026-10-07, when 7 October is marked, then the run resets to 0 and no event fires.

**Edge cases.**

- An absence later converted to excused by BR-ATT-003 retroactively breaks the run, and an already-fired threshold event is compensated by a correction notice rather than deleted.
- The run spans grading periods and academic terms but not academic years.
- Campus holidays and the campus work week come from the campus calendar, so two campuses in one tenant can reach the threshold on different dates.

### BR-ATT-007 Cumulative absence ladder

**Owner:** Attendance · **Parameters:** Attendance → thresholds and ladder

**Tests:** `CumulativeAbsenceLadderRulesTests`

**Rule.** The cumulative unexcused absence count per academic year drives an ordered escalation ladder. Each rung fires exactly once per student per year, at the moment its count is first reached, and firing a higher rung never re-fires a lower one.

**Examples.**

- Given a ladder of 5 (notify guardian), 10 (counselor meeting) and 15 (principal letter), and a student reaching 5 unexcused absences on 2026-11-12, when the count is updated, then the guardian notification fires once on 12 November.
- Given the same student reaching 10 unexcused absences on 2027-01-20, when the count is updated, then the counselor task is created and the guardian rung does not fire again.
- Given a back-dated correction on 2027-02-02 that raises the count from 14 to 16 in a single save, when the ladder is evaluated, then only the rung at 15 fires, exactly once, and the rungs at 5 and 10 stay closed.

**Edge cases.**

- A correction that lowers the count below a fired rung does not un-fire it; it records a correction note so the counselor can close the case.
- Rungs create tasks in the Requests service and notifications in the Notification service; the Attendance service owns only the counting and the event.
- A student who transfers campus mid-year keeps one cumulative count, because the count is per student per academic year and not per section.

### BR-ATT-008 Attendance percentage denominator

**Owner:** Attendance · **Parameters:** Attendance → excuse rules

**Tests:** `AttendancePercentageRulesTests`

**Rule.** Attendance percentage is present-equivalent days divided by enrolled school days. Enrolled school days are the campus school days between the enrollment date and the earlier of today and the withdrawal date. Excused days count as present-equivalent only when the excused-counts-present setting is on.

**Examples.**

- Given 180 school days in the year, a student enrolled from day 1 with 9 unexcused absences and 6 excused days, and the excused-counts-present setting on, when the percentage is computed, then it is (180 − 9) ÷ 180 = 171/180 = 95.00%.
- Given the same student with the excused-counts-present setting off, when the percentage is computed, then it is (180 − 9 − 6) ÷ 180 = 165/180 = 91.67%.
- Given a student enrolled on the 41st school day of the same 180-day year, with 4 unexcused absences since enrolling, when the percentage is computed on the last day of the year, then the denominator is 140 and the percentage is 136/140 = 97.14%.

**Edge cases.**

- The denominator is enrolled school days, never total school days, or a late joiner is punished for days before they existed at the school.
- A withdrawn student's percentage freezes at the withdrawal date and does not decay as the year continues.
- The percentage is rounded half away from zero to two decimals when published, and the unrounded value is what BR-ASM-012 compares against the promotion floor.

### BR-ATT-009 Mid-term section move splits the record

**Owner:** Attendance · **Parameters:** none

**Tests:** `SectionMoveAttendanceRulesTests`

**Rule.** A section move takes effect from a date. Attendance before that date stays attributed to the old section and attendance from that date is attributed to the new one. No existing record is rewritten, reassigned or deleted.

**Examples.**

- Given a move from 4-B to 4-C effective 2027-01-11, when the registers are opened, then the 4-B register for 2027-01-10 still lists the student and the 4-C register for 2027-01-11 lists the student.
- Given a section attendance report for 4-B covering the whole term, when it is generated, then it includes the student's days up to and including 2027-01-10 and no day after.
- Given the student's own percentage over 60 enrolled school days spanning both sections with 3 unexcused absences, when it is computed, then it uses one denominator of 60 and is 57/60 = 95.00%.

**Edge cases.**

- A move backdated to a date that already has marks in the new section is rejected, because it would create two marks for one student on one day.
- Teacher-facing lists are driven by the section membership on the date being marked, not by the current membership.
- The move publishes `school.student.section-changed.v1`; the Attendance service reacts to it and never reads School's tables.

### BR-ATT-010 Offline mark arriving after the lock window

**Owner:** Attendance · **Parameters:** Attendance → lock window

**Tests:** `OfflineAttendanceSyncRulesTests`

**Rule.** A mark captured offline carries its capture timestamp. On sync it is accepted when the capture timestamp falls inside the lock window, even if the sync itself happens after the window closed. It is rejected as a conflict when a different value was written on the server after the capture timestamp.

**Examples.**

- Given a lock window of 48 hours, a mark captured offline at 2026-10-05 08:10 Asia/Riyadh and synced at 2026-10-08 06:00 Asia/Riyadh, when the sync runs, then the mark is accepted because the capture time is inside the window.
- Given the same offline mark and a different value written on the server by the head of year at 2026-10-06 14:00, when the sync runs, then the offline mark is rejected with `ATTENDANCE_OFFLINE_CONFLICT`, queued for human review, and the server value stands.
- Given a mark captured at 2026-10-05 08:10 for a session that was cancelled on 2026-10-04, when the sync runs, then it is rejected with `ATTENDANCE_SESSION_NOT_SCHEDULED` and the device drops it after showing the reason to the teacher.

**Edge cases.**

- The device clock is untrusted: the capture timestamp is accepted only when it is not in the future relative to the server, and a skewed device is asked to resynchronize before it may sync marks.
- Sync is idempotent on the pair of session identifier and student identifier, so a retried upload never creates a second mark.
- A conflict queue entry that nobody resolves is surfaced on the attendance data-quality report rather than expiring silently.

### BR-ATT-011 Unmarked class reminder

**Owner:** Attendance · **Parameters:** Attendance → cut-off times

**Tests:** `UnmarkedClassReminderRulesTests`

**Rule.** A reminder is sent to the assigned teacher when a scheduled session still has no complete register after the configured grace period from the session start. No reminder is sent for a cancelled session, a non-teaching day, or a session whose register is already complete.

**Examples.**

- Given a grace period of 30 minutes and a period starting 09:00 with no marks at all at 09:30, when the reminder job runs, then one reminder is sent to the assigned teacher at 09:30.
- Given the same period where 22 of 24 students are marked at 09:30, when the reminder job runs, then the reminder still fires, because the register is not complete.
- Given the period cancelled at 08:45 for an assembly, when the reminder job runs at 09:30, then no reminder is sent and the session is recorded as cancelled, not unmarked.

**Edge cases.**

- A substitute assigned under BR-SCD-005 becomes the reminder recipient in place of the absent teacher.
- Exactly one reminder is sent per session; escalation to the head of year is a separate, later job with its own interval.
- The job publishes `attendance.attendance.not-marked.v1` and does not send anything itself; delivery is the Notification service's decision under BR-NOT-001.

---

## S.3 Finance (FIN)

### BR-FIN-001 Fee plan installment generation

**Owner:** Finance · **Parameters:** General → currency

**Tests:** `InstallmentGenerationRulesTests`

**Rule.** An installment plan divides the net annual fee by the installment count, truncating each installment down to the currency's minor unit, and adds the whole remainder to the final installment so that the installments sum exactly to the net.

**Examples.**

- Given a net annual fee of SAR 24,000.00 and 10 installments, when the plan is generated, then every installment is SAR 2,400.00 and the installments total SAR 24,000.00.
- Given a net annual fee of SAR 23,000.00 and 3 installments, when the plan is generated, then the installments are SAR 7,666.66, SAR 7,666.66 and SAR 7,666.68, totalling SAR 23,000.00.
- Given a net annual fee of JOD 1,000.000 and 3 installments, when the plan is generated, then the installments are JOD 333.333, JOD 333.333 and JOD 333.334, totalling JOD 1,000.000.

**Edge cases.**

- The property-based test asserts that the installments always sum exactly to the net, for every count from 1 to 12 and every currency scale.
- A mid-year net change regenerates only the unposted installments; posted ones are corrected under BR-FIN-014.
- Due dates come from the plan calendar, not from an even division of the year, so the last installment can fall before the last school day.

### BR-FIN-002 Pro-rata by days

**Owner:** Finance · **Parameters:** Finance → pro-rata mode

**Tests:** `ProRataByDaysRulesTests`

**Rule.** With day-based pro-rata, the charge is the full period fee multiplied by the remaining school days and divided by the total school days in the period, rounded to the currency's minor unit. The remaining count includes the start date itself.

**Examples.**

- Given a term fee of SAR 9,000.00, 60 school days in the term and a student starting on the 16th school day, when the charge is computed, then 45 days remain and the charge is 9,000 × 45 ÷ 60 = SAR 6,750.00.
- Given a term fee of AED 7,500.00, 62 school days in the term and a student starting on the 21st school day, when the charge is computed, then 42 days remain and the charge is 7,500 × 42 ÷ 62 = AED 5,080.65.
- Given a term fee of SAR 9,000.00, 60 school days and a student leaving after the 30th school day, when the charge is recomputed, then the charge is 9,000 × 30 ÷ 60 = SAR 4,500.00 and the balance is credited.

**Edge cases.**

- School days come from the campus calendar, so holidays declared after the charge was computed do not retroactively change a posted invoice.
- A student starting on the last school day is charged one day, never zero, because the start date is included.
- Rounding happens once, on the final amount, not on a per-day rate.

### BR-FIN-003 Pro-rata by months

**Owner:** Finance · **Parameters:** Finance → pro-rata mode

**Tests:** `ProRataByMonthsRulesTests`

**Rule.** With month-based pro-rata, a partial month is charged in full when the start date falls on or before the configured mid-month day, and is not charged at all otherwise. The annual fee is divided by the number of chargeable months the plan declares.

**Examples.**

- Given an annual fee of SAR 24,000.00 over 10 chargeable months from September to June, a mid-month day of 15 and a start on 2026-10-12, when the charge is computed, then October is charged and the total is 24,000 × 9 ÷ 10 = SAR 21,600.00.
- Given the same plan and a start on 2026-10-16, when the charge is computed, then October is not charged and the total is 24,000 × 8 ÷ 10 = SAR 19,200.00.
- Given the same plan and a start on 2026-09-01, when the charge is computed, then all 10 months are charged and the total is SAR 24,000.00.

**Edge cases.**

- The mid-month day is a calendar day number, so February's boundary is the same numeric day as October's.
- A student who leaves mid-month is charged for that month when the leaving date is after the mid-month day, mirroring the joining rule.
- Switching a tenant from month-based to day-based pro-rata applies to new charges only and never recomputes posted ones.

### BR-FIN-004 Discount stacking order

**Owner:** Finance · **Parameters:** Finance → discount rules

**Tests:** `DiscountStackingRulesTests`

**Rule.** Discounts apply to the gross fee in the configured order: fixed-amount discounts first, then percentage discounts on the running balance, then scholarships. The running balance never falls below zero and unused discount value is not carried anywhere.

**Examples.**

- Given a gross tuition of SAR 20,000.00, a fixed book-voucher discount of SAR 1,000.00 and a sibling discount of 10%, when the net is computed, then it is (20,000 − 1,000) × 0.90 = 19,000 × 0.90 = SAR 17,100.00.
- Given the same inputs with the percentage applied before the fixed amount, when the net is computed, then it would be 20,000 × 0.90 − 1,000 = SAR 17,000.00, which is a defect: the configured order gives SAR 17,100.00.
- Given a gross of SAR 5,000.00, a fixed discount of SAR 6,000.00 and a 10% discount, when the net is computed, then the running balance floors at SAR 0.00, the percentage discount computes to SAR 0.00, the net is SAR 0.00 and the unused SAR 1,000.00 is discarded.

**Edge cases.**

- Two percentage discounts compound on the running balance rather than adding: 10% then 10% on SAR 20,000.00 gives SAR 16,200.00, not SAR 16,000.00.
- Each discount line is stored separately on the invoice with its rule identifier, so a parent can see why the net differs from the gross.
- Tax is computed after discounts, on the discounted net, under BR-FIN-012.

### BR-FIN-005 Sibling discount eligibility

**Owner:** Finance · **Parameters:** Finance → discount rules

**Tests:** `SiblingDiscountRulesTests`

**Rule.** The sibling discount counts only enrolled, fee-paying siblings in the same tenant and the same academic year, ordered by date of birth descending, and it applies at the configured rate to the configured child positions. Changes in eligibility apply from the change date forward and never restate posted installments.

**Examples.**

- Given a policy of no discount for children 1 and 2 and 15% from the third child onward, and three enrolled siblings born 2013, 2016 and 2019, when the discount is evaluated, then the 2019-born child receives 15% and the other two receive none.
- Given the eldest withdrawing on 2026-12-01, when the discount is re-evaluated, then only two children remain enrolled, the 15% stops for installments due from 2026-12-01 and already-posted installments are unchanged.
- Given a fourth child enrolling on 2027-01-05, when the discount is re-evaluated, then children three and four both receive 15%, applied to installments due from 2027-01-05.

**Edge cases.**

- A sibling on a full scholarship is still enrolled and still counts for ordering, unless the policy explicitly excludes non-fee-paying siblings.
- Half-siblings and step-siblings count when they share at least one guardian record flagged as a payer, which is the tenant's configured definition.
- Twins share a birth date; the tie is broken by student number so the ordering is deterministic.

### BR-FIN-006 Scholarship cap

**Owner:** Finance · **Parameters:** Finance → discount rules

**Tests:** `ScholarshipCapRulesTests`

**Rule.** A scholarship reduces only the fee items it names and never below zero. The total relief granted per student per academic year is the lower of the scholarship's cap amount and its cap percentage of the named items' gross.

**Examples.**

- Given a gross tuition of SAR 20,000.00 and a scholarship of 50% capped at SAR 8,000.00, when the relief is computed, then it is the lower of SAR 10,000.00 and SAR 8,000.00, so the relief is SAR 8,000.00 and the net is SAR 12,000.00.
- Given a gross tuition of SAR 12,000.00 and the same scholarship, when the relief is computed, then it is the lower of SAR 6,000.00 and SAR 8,000.00, so the relief is SAR 6,000.00 and the net is SAR 6,000.00.
- Given a scholarship naming tuition only, a gross tuition of SAR 20,000.00 and a transport item of SAR 3,000.00, when the relief is computed, then the relief is SAR 8,000.00, transport stays SAR 3,000.00 and the invoice net is 20,000 − 8,000 + 3,000 = SAR 15,000.00.

**Edge cases.**

- The cap is annual, so relief already granted in earlier installments reduces what later installments can receive.
- A scholarship and a sibling discount can both apply; the order in BR-FIN-004 puts the scholarship last, on the already-discounted balance.
- A scholarship revoked mid-year stops future relief and does not claw back posted relief without an explicit, approved reversing document.

### BR-FIN-007 Late fee accrual and cap

**Owner:** Finance · **Parameters:** Finance → late fee rules

**Tests:** `LateFeeAccrualRulesTests`

**Rule.** A late fee accrues on the overdue principal from the end of the grace period, at the configured rate for each started accrual period, rounded to the currency scale on each accrual. Accrual stops permanently once the cumulative late fee reaches the configured cap.

**Examples.**

- Given an installment of SAR 3,000.00 due 2026-10-01, a grace period of 7 days, a rate of 1% per started month and a cap of 10% of principal, when the ledger is evaluated on 2026-11-10, then accrual started 2026-10-08, 2 months have started and the late fee is SAR 60.00.
- Given the same installment still unpaid on 2027-09-10, when the ledger is evaluated, then 12 started months would give SAR 360.00 but the cap of 10% of SAR 3,000.00 stops the late fee at SAR 300.00.
- Given the same installment paid in full on 2026-10-07, when the ledger is evaluated, then no late fee accrues at all, because payment fell inside the 7-day grace period.

**Edge cases.**

- The principal for accrual is the amount still outstanding, so a partial payment reduces future accrual but never refunds accrued fees.
- The cap is per installment, not per account, so a student with three overdue installments can accrue three separate capped fees.
- Accrual is computed by a daily job and is idempotent on the pair of installment and accrual period, so a re-run never double-charges.

### BR-FIN-008 Payment allocation order

**Owner:** Finance · **Parameters:** Finance → allocation order

**Tests:** `PaymentAllocationRulesTests`

**Rule.** A payment with no explicit allocation is applied in the configured order: late fees first, then open invoice lines by due date ascending, then any remainder becomes credit. Allocation never crosses to another payer's invoices.

**Examples.**

- Given open items of a late fee of SAR 60.00, invoice 1 of SAR 3,000.00 due 2026-10-01 and invoice 2 of SAR 3,000.00 due 2026-11-01, when SAR 4,000.00 is received, then SAR 60.00 clears the late fee, SAR 3,000.00 clears invoice 1, SAR 940.00 is applied to invoice 2 and SAR 2,060.00 stays open.
- Given the same open items totalling SAR 6,060.00, when SAR 7,000.00 is received, then every item clears and SAR 940.00 becomes an unapplied credit on the student account.
- Given the same open items and a payer who explicitly allocates SAR 3,000.00 to invoice 2, when the payment is posted, then invoice 2 clears, the late fee and invoice 1 stay open, and the explicit allocation is stored with the user who chose it.

**Edge cases.**

- Two invoices with the same due date are ordered by invoice number ascending, so allocation is deterministic.
- Reallocation of a posted payment is a reversal and a new allocation under BR-FIN-014, never an in-place edit.
- A split-payer account allocates within each payer's own invoices only; see BR-FIN-019.

### BR-FIN-009 Overpayment becomes credit

**Owner:** Finance · **Parameters:** Finance → allocation order

**Tests:** `OverpaymentCreditRulesTests`

**Rule.** An amount received beyond all open items is posted as an unapplied credit on the student account, in the currency it was received in. It is applied automatically to the next posted invoice in the same currency and is never refunded automatically.

**Examples.**

- Given open items of SAR 2,000.00 and a payment of SAR 2,500.00, when the payment is posted, then SAR 2,000.00 is applied and SAR 500.00 becomes an unapplied credit.
- Given that SAR 500.00 credit and a new invoice of SAR 3,000.00 posted on 2027-01-05, when the invoice posts, then SAR 500.00 is applied automatically and SAR 2,500.00 remains due.
- Given an unapplied credit of AED 500.00 and an open item of SAR 300.00 on the same account, when automatic application runs, then nothing is applied, the attempt reports `FINANCE_CURRENCY_MISMATCH`, and the SAR item stays open.

**Edge cases.**

- A credit carries across academic years on the same student and is listed on the statement of account every time.
- Refunding a credit is a deliberate, approved act under BR-FIN-010.
- A credit on a withdrawn student's account blocks the account from closing until it is refunded or written off with approval.

### BR-FIN-010 Refund from credit versus from payment

**Owner:** Finance · **Parameters:** Finance → payment methods

**Tests:** `RefundSourceRulesTests`

**Rule.** Refunding an unapplied credit posts a credit-refund document and needs only the refund approval. Refunding against a settled payment first unapplies that payment, which reopens the invoice lines it had settled, and needs both the refund approval and a recorded reversal reason.

**Examples.**

- Given an unapplied credit of SAR 500.00 and an approved refund of SAR 500.00, when the refund posts, then a credit-refund document of SAR 500.00 is created, the credit falls to SAR 0.00 and no invoice changes.
- Given a settled invoice of SAR 3,000.00 and an approved refund of SAR 3,000.00 against the payment that settled it, when the refund posts, then the payment is unapplied, the invoice reopens with SAR 3,000.00 outstanding and a refund document of SAR 3,000.00 is created.
- Given a partial refund of SAR 1,200.00 against that same settled payment, when the refund posts, then SAR 1,200.00 is unapplied, the invoice reopens with SAR 1,200.00 outstanding and SAR 1,800.00 stays settled.

**Edge cases.**

- A refund against a payment whose method cannot be reversed, such as cash already banked, still posts but is flagged for the cashier's manual payout process.
- Refunds hold the high-risk `finance.refunds.approve` permission and therefore need four-eyes to grant, under BR-IDN-004.
- Reopening an invoice restarts late-fee accrual from the original due date under BR-FIN-007, which is why the reversal reason is mandatory.

### BR-FIN-011 Rounding per currency

**Owner:** Finance · **Parameters:** General → currency

**Tests:** `CurrencyRoundingRulesTests`

**Rule.** Every monetary amount is a decimal stored at the currency's ISO 4217 minor-unit scale: 2 for SAR and AED, 3 for JOD. A computed amount is rounded half away from zero to that scale at the moment it becomes a document line, and never before.

**Examples.**

- Given a line of AED 1,000.00 at a 5% tax rate, when the line is posted, then the tax is AED 50.00 and the gross is AED 1,050.00.
- Given a pro-rata computation producing JOD 333.3335, when the line is posted, then the stored amount is JOD 333.334, because JOD carries three decimals.
- Given a discount of 7.5% on SAR 1,333.33, when the line is posted, then the raw discount 99.99975 rounds to SAR 100.00 and the net line is SAR 1,233.33.

**Edge cases.**

- No monetary value is ever held in a binary floating-point type, at any layer, including the API payload and the report renderer.
- A currency with no minor unit would have scale 0; the three currencies in the first release do not, and adding one requires a configuration entry, never a code branch.
- A sum of rounded lines can differ from the rounding of the raw sum; the invoice total is always the sum of the stored lines, so what is printed is what is owed.

### BR-FIN-012 Tax inclusive versus exclusive per item

**Owner:** Finance · **Parameters:** Finance → tax

**Tests:** `TaxInclusiveExclusiveRulesTests`

**Rule.** Each fee item declares whether its price includes tax. For a tax-exclusive item the tax is price multiplied by the rate. For a tax-inclusive item the net is price divided by one plus the rate and the tax is price minus net. Both results are rounded to the currency scale.

**Examples.**

- Given a tax-exclusive item priced SAR 1,000.00 at a 15% rate, when the line is posted, then the net is SAR 1,000.00, the tax is SAR 150.00 and the gross is SAR 1,150.00.
- Given a tax-inclusive item priced SAR 1,000.00 at a 15% rate, when the line is posted, then the net is 1,000 ÷ 1.15 = SAR 869.57, the tax is SAR 130.43 and the gross is SAR 1,000.00.
- Given a tax-exempt tuition item of SAR 20,000.00 and a tax-exclusive uniform item of SAR 400.00 at 15% on one invoice, when the invoice is posted, then the invoice tax is SAR 60.00 and the invoice gross is SAR 20,460.00.

**Edge cases.**

- Tax is computed on the discounted net, so the discount lines in BR-FIN-004 must be applied before the tax lines.
- An item's inclusive or exclusive flag is copied onto the invoice line at posting, so changing the fee item later never restates a posted invoice.
- The tax rate is stored per line as a value, not as a reference, so a rate change is never retroactive.

### BR-FIN-013 Gapless numbering per series under concurrency

**Owner:** Finance · **Parameters:** Finance → numbering series

**Tests:** `GaplessNumberingRulesTests`

**Rule.** Each document series issues numbers from a single counter, reserved inside the same database transaction that posts the document. A rolled-back post consumes no number, so the issued sequence has neither gaps nor duplicates.

**Examples.**

- Given the series INV-2026 standing at 0417 and two cashiers posting at the same instant, when both transactions commit, then one document is INV-2026-0418 and the other INV-2026-0419, and no number between them is skipped.
- Given a post that fails validation after the number was reserved, when the transaction rolls back, then the counter stays at 0417 and the next successful post takes INV-2026-0418.
- Given the year rolling over on 2027-01-01, when the first receipt of the new year posts, then it is RCT-2027-0001 and the RCT-2026 series stays closed at its last issued number.

**Edge cases.**

- The counter is held per tenant, per campus and per series, because two campuses must be able to issue in parallel without blocking each other.
- Reserving the number outside the posting transaction, for example to show it in the user interface before saving, is the defect this rule exists to prevent; a draft shows no number at all.
- A regulator's audit of the series must be able to walk it from the first number to the last with no missing value, which the property-based test asserts over ten thousand randomized concurrent posts.

### BR-FIN-014 Posted documents are immutable

**Owner:** Finance · **Parameters:** none

**Tests:** `PostedDocumentImmutabilityRulesTests`

**Rule.** A posted invoice, receipt, credit note or refund can never be edited or deleted. A correction posts a reversing document of the same value and the opposite sign, linked to the original, and both stay visible on the statement of account.

**Examples.**

- Given a posted invoice INV-2026-0418 of SAR 3,000.00 carrying the wrong fee item, when the correction is approved, then a credit note CRN-2026-0032 of SAR 3,000.00 is posted, the balance from that pair returns to SAR 0.00 and a new invoice is posted for the correct amount.
- Given an attempt to change the amount on INV-2026-0418 directly, when the request is submitted, then it is rejected with `FINANCE_INVOICE_ALREADY_POSTED` and nothing changes.
- Given a draft invoice that has not been posted, when it is edited, then the edit succeeds and no reversing document is created, because immutability begins at posting.

**Edge cases.**

- A reversing document takes the next number in its own series; it never reuses the original's number.
- The statement of account shows original, reversal and replacement, so the audit trail is readable without opening the audit log.
- Deleting a tenant under BR-PLT-003 removes the data, but until then no financial document is ever removed, including by the recycle bin.

### BR-FIN-015 Cheque bounce reversal and fee

**Owner:** Finance · **Parameters:** Finance → payment methods, late fee rules

**Tests:** `ChequeBounceRulesTests`

**Rule.** Recording a bounce reverses the cheque's payment in full, reopens every invoice line it had settled, and posts a bounce-fee charge at the configured amount. The reversal is dated on the bank advice date, not the original payment date.

**Examples.**

- Given a cheque payment of SAR 6,000.00 dated 2026-11-01 that settled two invoices of SAR 3,000.00 each, and a configured bounce fee of SAR 150.00, when a bounce is recorded on 2026-11-09, then the payment reverses on 2026-11-09, both invoices reopen at SAR 3,000.00 each and a SAR 150.00 charge is posted, leaving SAR 6,150.00 outstanding.
- Given the same bounce with the bounce-fee setting at SAR 0.00, when the bounce is recorded, then no fee line is posted and SAR 6,000.00 is outstanding.
- Given an installment of SAR 3,000.00 originally due 2026-10-01 whose late-fee accrual had stopped when the cheque settled, when the bounce is recorded on 2026-11-09, then accrual resumes from the original due date and the late fee on 2026-11-10 is SAR 60.00 under BR-FIN-007.

**Edge cases.**

- A cheque that settled several students' invoices reopens all of them, and the bounce fee is posted once on the payer, not once per student.
- A second bounce from the same payer can trigger a payment-method restriction, which is a separate, configured rule under BR-FIN-016.
- The reversal never deletes the original payment record; the bounced cheque stays visible with its status.

### BR-FIN-016 Service restriction rules

**Owner:** Finance · **Parameters:** Finance → restriction rules

**Tests:** `ServiceRestrictionRulesTests`

**Rule.** A restriction such as holding a report card applies only when a named, explicit rule matches in full: the overdue amount exceeds the configured threshold, the overdue age exceeds the configured number of days, and the student carries no exemption flag. Every application and every lift is audited.

**Examples.**

- Given the rule "hold the report card when overdue exceeds SAR 2,000.00 for more than 30 days" and a student overdue SAR 2,500.00 since 2026-10-01, when report cards publish on 2026-11-05, then the overdue age is 35 days, the report card is held and the guardian sees the reason.
- Given the same rule and a student overdue SAR 2,500.00 since 2026-10-20, when report cards publish on 2026-11-05, then the overdue age is 16 days, the rule does not match and the report card publishes normally.
- Given the same overdue student carrying a hardship exemption flag recorded on 2026-11-01, when report cards publish on 2026-11-05, then no restriction applies and the exemption is named in the audit entry.

**Edge cases.**

- No restriction may ever block attendance marking, safeguarding records, emergency contact access or the student's physical presence at school.
- Restrictions are published as `finance.account.restricted.v1` and consumed by the restricted service; Finance never reaches into Assessment to hide a report card.
- A payment that clears the threshold publishes `finance.account.cleared.v1`, and the restriction lifts on the next consumption, not on a nightly job.

### BR-FIN-017 Active student definition for SaaS billing

**Owner:** Finance · **Parameters:** none

**Tests:** `ActiveStudentCountRulesTests`

**Rule.** For platform billing, an active student is one whose status was enrolled for at least one day inside the billing month. Each student is counted once per month regardless of section, campus or plan changes. Applicants, withdrawn records and alumni are not counted.

**Examples.**

- Given a tenant with 1,240 enrolled students on 2026-10-01 and 12 more enrolling on 2026-10-28, when the October count is computed, then it is 1,252.
- Given 6 of those students withdrawing on 2026-10-03, when the October count is computed, then it stays 1,252, because each of them was enrolled for at least one day in October.
- Given a student moving from campus A to campus B on 2026-10-15, when the October count is computed, then the student is counted once and the total is unchanged at 1,252.

**Edge cases.**

- A student enrolled and withdrawn on the same day counts for that month; the definition is "at least one day", not "at month end".
- A student re-enrolling in the same month after a withdrawal is still one student, deduplicated on the student identifier.
- The count is a Finance figure and the Platform meter in BR-PLT-005 consumes it; both must agree, and a disagreement fails the reconciliation job.

### BR-FIN-018 Proration on a plan change

**Owner:** Finance · **Parameters:** none

**Tests:** `PlanChangeProrationRulesTests`

**Rule.** Changing a subscription plan mid-cycle credits the unused days of the old plan and charges the remaining days of the new plan, both computed on the actual number of days in that cycle. The day of the change is charged on the new plan.

**Examples.**

- Given a 31-day October cycle, an old plan of SAR 3,100.00, a new plan of SAR 6,200.00 and a change on 2026-10-17, when the proration is computed, then 15 remaining days are credited at 3,100 ÷ 31 = SAR 100.00 per day, giving SAR 1,500.00, and charged at 6,200 ÷ 31 = SAR 200.00 per day, giving SAR 3,000.00, for a net charge of SAR 1,500.00.
- Given the same cycle and a downgrade on 2026-10-17 from SAR 6,200.00 to SAR 3,100.00, when the proration is computed, then the credit is SAR 3,000.00, the charge is SAR 1,500.00 and the net is a credit of SAR 1,500.00 carried to the next invoice rather than refunded.
- Given a change on 2026-10-01, the first day of the cycle, when the proration is computed, then all 31 days are credited at SAR 3,100.00 and all 31 days are charged at SAR 6,200.00, so the net additional charge is SAR 3,100.00 and the cycle costs the full new plan price of SAR 6,200.00.

**Edge cases.**

- February and the 31-day months produce different daily rates; the divisor is always the actual day count of that cycle, never 30.
- A plan change on the last day of the cycle credits and charges one day each, which is correct and must not be special-cased to zero.
- An annual cycle uses the same rule with the actual day count, so a leap year divides by 366.

### BR-FIN-019 Split payers by percentage

**Owner:** Finance · **Parameters:** General → currency

**Tests:** `SplitPayerRulesTests`

**Rule.** When a student has split payers, each posted charge is divided by the payer percentages, which must total exactly 100. Every share except the largest is rounded to the currency scale, and the largest payer's share is the charge minus the sum of the others, so the shares always sum exactly to the charge.

**Examples.**

- Given payers A at 60% and B at 40% and a charge of SAR 3,000.00, when the charge is split, then A is invoiced SAR 1,800.00 and B is invoiced SAR 1,200.00.
- Given payers A at 70% and B at 30% and a charge of SAR 1,000.05, when the charge is split, then B's share rounds from 300.015 to SAR 300.02, A takes the remainder of SAR 700.03, and the two invoices sum to exactly SAR 1,000.05; rounding A independently would give SAR 700.04 and a total of SAR 1,000.06, which is a defect.
- Given payers A at 60% and B at 45%, when the split is saved, then it is rejected because the percentages total 105 and not 100.

**Edge cases.**

- A tie for the largest share is broken by payer order on the student record, so the remainder always lands deterministically.
- Each payer receives their own invoice, their own statement and their own payment link; a payment by one payer never settles the other's invoice.
- Changing the split applies to charges posted after the change; posted invoices are corrected under BR-FIN-014 if the split was wrong.

---

## S.4 Timetable and scheduling (SCD)

### BR-SCD-001 Hard versus soft constraints

**Owner:** Scheduling · **Parameters:** none

**Tests:** `TimetableConstraintRulesTests`

**Rule.** Hard constraints — teacher double-booking, room double-booking, section double-booking, room capacity and declared teacher unavailability — make a timetable invalid and block publishing. Soft constraints never block; they only reduce the quality score.

**Examples.**

- Given a draft in which teacher T is assigned to 4-B and 5-A in the same period on Sunday, when publishing is attempted, then it is blocked with `SCHEDULING_TIMETABLE_CONFLICT`.
- Given a draft in which 5-A has Science in three periods on one day, against a soft spread preference that costs 3 points for each extra same-day period, when the quality score is computed, then the penalty is 6 points, the score falls from 92 to 86 and publishing still succeeds.
- Given a draft placing 32 students in a room of capacity 30, when publishing is attempted, then it is blocked, and it proceeds only with `scheduling.timetable.override-conflict` and a recorded reason.

**Edge cases.**

- A soft constraint must never silently become hard through a high penalty; the two lists are explicit and separate.
- An override records which hard constraint was broken, by whom and why, and appears on the timetable version history.
- The quality score is stored on the version, so two candidate timetables can be compared after the fact.

### BR-SCD-002 Consecutive-period limit

**Owner:** Scheduling · **Parameters:** General → work week

**Tests:** `ConsecutivePeriodRulesTests`

**Rule.** A teacher may not exceed the configured number of consecutive teaching periods without a free period. The run is counted within one day, and a period the bell schedule marks as a break is ignored rather than treated as a free period.

**Examples.**

- Given a limit of 4 consecutive periods and a teacher assigned to periods 1, 2, 3 and 4 on Monday, when the placement is validated, then it is accepted.
- Given the same teacher also assigned to period 5 on Monday, when the placement is validated, then it is rejected, because 5 consecutive periods exceed the limit of 4.
- Given the teacher assigned to periods 1, 2 and 3, a marked break between periods 3 and 4, and periods 4 and 5, when the placement is validated, then the run counts as 5 teaching periods and is rejected, because a marked break is ignored and is not a free period.

**Edge cases.**

- The limit is per teacher and can be overridden per teacher in the staff record; the tenant value is the default.
- Cover assignments under BR-SCD-005 count toward the run, so a substitution can push a teacher over the limit and must be validated the same way.
- The run resets at the end of the school day and never spans two days.

### BR-SCD-003 Part-time availability and weekly load

**Owner:** Scheduling · **Parameters:** none

**Tests:** `PartTimeAvailabilityRulesTests`

**Rule.** A part-time teacher's declared availability windows are hard constraints: any placement whose period is not entirely inside a window is rejected. The teacher's maximum weekly load is enforced against the count of assigned teaching periods.

**Examples.**

- Given a teacher available Sunday to Tuesday from 07:30 to 12:00 only, when a Wednesday period is placed, then the placement is rejected.
- Given the same teacher and a Sunday period running 11:30 to 12:20, when it is placed, then it is rejected, because the period ends after 12:00 and must be entirely inside the window.
- Given a maximum weekly load of 12 periods with 12 already assigned, when a thirteenth period is placed, then it is rejected with `SCHEDULING_TIMETABLE_CONFLICT`.

**Edge cases.**

- Availability is stored per academic year with effective dates, so a change of contract mid-year does not invalidate the published first term.
- An exam invigilation slot counts toward availability but not toward teaching load; the two counters are separate.
- A window that a leave request later removes triggers substitution, not silent reassignment.

### BR-SCD-004 Travel time between campuses

**Owner:** Scheduling · **Parameters:** none

**Tests:** `CampusTravelTimeRulesTests`

**Rule.** A teacher shared across campuses needs at least the configured travel time between the end of a period on one campus and the start of the next period on another. The gap is measured end to start, and consecutive periods on the same campus need no gap.

**Examples.**

- Given a travel time of 30 minutes between campus A and campus B, a period on A ending 10:10 and a period on B starting 10:45, when the placement is validated, then it is accepted, because the gap is 35 minutes.
- Given the same campuses and a period on B starting 10:35, when the placement is validated, then it is rejected, because the gap is 25 minutes and less than 30.
- Given two consecutive periods both on campus A with no gap between them, when the placement is validated, then it is accepted, because no travel is required.

**Edge cases.**

- Travel time is directional and is configured per ordered campus pair, because traffic is not symmetric.
- A campus pair with no configured travel time is treated as unreachable within a school day, not as zero minutes.
- A substitution that would break travel time is filtered out of the suggestions in BR-SCD-005 before ranking.

### BR-SCD-005 Cover fairness score

**Owner:** Scheduling · **Parameters:** none

**Tests:** `CoverFairnessRulesTests`

**Rule.** Substitution candidates are first filtered to teachers who are free in the period, qualified for the subject, inside their daily load and inside their travel-time constraints, then ranked ascending by a fairness score equal to the teacher's cover minutes this term divided by the term average.

**Examples.**

- Given a term average of 120 cover minutes and qualified free candidates at 60, 180 and 0 minutes, when the suggestions are ranked, then the order is the teacher at 0 (score 0.00), the teacher at 60 (score 0.50) and the teacher at 180 (score 1.50).
- Given that the teacher at 0 cover minutes is not free in that period, when the suggestions are ranked, then that teacher is filtered out and the top suggestion is the teacher at 60 minutes.
- Given two qualified free teachers both at 60 cover minutes, when the suggestions are ranked, then the one with fewer teaching periods that day ranks first, and if that also ties, the one whose last cover was longer ago.

**Edge cases.**

- A term average of zero, at the start of a term when nobody has covered anything, makes every score zero, and the tie-break ordering alone decides.
- Accepting a suggestion updates the cover minutes immediately, so the next suggestion in the same morning already reflects it.
- The score is advisory: a scheduler may pick any candidate from the filtered list, and the override is recorded.

### BR-SCD-006 Publishing does not alter recorded attendance

**Owner:** Scheduling · **Parameters:** Attendance → lock window

**Tests:** `TimetablePublishEffectiveDateRulesTests`

**Rule.** A published timetable version applies from its effective date forward. Sessions on or before that date keep the version that generated them, and attendance already recorded is never rewritten, reassigned or deleted.

**Examples.**

- Given attendance recorded for 4-B period 3 on 2027-01-08 and a new timetable version published with effect from 2027-01-11, when the publish completes, then the 8 January record keeps the old version's subject and teacher.
- Given the new version removing period 3 from 4-B, when the publish completes, then no period 3 session is generated from 2027-01-11 onward and every historical period 3 record stays readable.
- Given an attempt to publish with an effective date of 2027-01-05 while attendance already exists on 2027-01-06, when the publish is submitted, then it is rejected and the message states that the earliest allowed effective date is 2027-01-07.

**Edge cases.**

- Sessions between the effective date and today are regenerated only where no attendance exists; a partially marked week is split at the first marked session.
- The timetable version identifier is stamped on every generated session, so any historical register can name the version it came from.
- Publishing emits `scheduling.timetable.published.v1`; Attendance reacts to it and never queries Scheduling's tables.

### BR-SCD-007 Room booking holds and buffers

**Owner:** Scheduling · **Parameters:** none

**Tests:** `RoomBookingRulesTests`

**Rule.** An ad-hoc room booking is granted only when the room is free for the whole requested interval extended by the configured setup and teardown buffers. A timetabled lesson always outranks an ad-hoc booking.

**Examples.**

- Given a setup and teardown buffer of 15 minutes, a request for 13:00 to 14:00 and a lesson in that room ending at 12:50, when the booking is evaluated, then the effective hold is 12:45 to 14:15, it overlaps the lesson and the booking is rejected.
- Given the same request where the last lesson in that room ends at 12:40, when the booking is evaluated, then the booking is granted for 13:00 to 14:00 and the hold 12:45 to 14:15 is recorded.
- Given a timetable version published later that places a lesson at 13:20 in that room, when the publish completes, then the ad-hoc booking is cancelled automatically, the requester is notified and the cancellation names the timetable version.

**Edge cases.**

- Buffers are properties of the room, not of the booking, because a science laboratory needs more turnaround than a meeting room.
- Two bookings whose holds overlap but whose requested intervals do not are still a conflict; the hold is what is reserved.
- An automatically cancelled booking is never silently dropped: the requester gets a notification with the reason and a suggested alternative room.

---

## S.5 Admissions (ADM)

### BR-ADM-001 Age eligibility by cut-off date

**Owner:** Admissions · **Parameters:** General → calendars

**Tests:** `AgeEligibilityRulesTests`

**Rule.** An applicant is eligible for a grade when their age in whole years on that grade's cut-off date falls between the grade's minimum and maximum, both inclusive. Age is computed from the Gregorian date of birth under BR-L10N-003.

**Examples.**

- Given Grade 1 with a cut-off of 2027-09-01, a minimum age of 6 and a maximum of 7, and a date of birth of 2021-03-14, when eligibility is evaluated, then the age on the cut-off is 6 and the applicant is eligible.
- Given the same grade and a date of birth of 2021-09-02, when eligibility is evaluated, then the applicant turns 6 one day after the cut-off, the age is 5 and the applicant is not eligible.
- Given the same grade and a date of birth of 2021-09-01, when eligibility is evaluated, then the applicant turns 6 exactly on the cut-off, the age is 6 and the applicant is eligible.

**Edge cases.**

- A 29 February birth date is aged on 28 February in a non-leap year, so the rule never throws on an invalid date.
- An age override needs a named permission and a reason and is recorded on the application, so an inspector can see every exception.
- A Hijri date of birth entered by a parent is converted to Gregorian on save and the Gregorian value is what this rule uses.

### BR-ADM-002 Required documents by grade and nationality

**Owner:** Admissions · **Parameters:** none

**Tests:** `RequiredDocumentRulesTests`

**Rule.** The required-document set for an application is the union of the documents the applied grade requires and the documents the applicant's nationality group requires. An application cannot be submitted while any required document is missing, and a document whose expiry date has passed counts as missing.

**Examples.**

- Given Grade 1 requiring a birth certificate and a vaccination record, and the non-citizen resident group additionally requiring a residency permit and a passport copy, when a Jordanian resident applies to Grade 1 in Riyadh, then 4 documents are required.
- Given that applicant uploading 3 of those 4 documents, when submission is attempted, then it is blocked with `ADMISSIONS_DOCUMENT_MISSING` and the missing document is named.
- Given a passport copy uploaded with an expiry of 2026-08-31 and a submission attempted on 2026-09-19, when submission is validated, then the passport counts as missing and submission is blocked.

**Edge cases.**

- A document that expires after submission but before enrollment triggers a reminder rather than invalidating the application.
- Changing the applied grade recomputes the required set, and documents already uploaded that remain required are kept.
- A conditional offer may waive a document explicitly; the waiver is named on the offer and expires with it.

### BR-ADM-003 Seat capacity and override

**Owner:** Admissions · **Parameters:** none

**Tests:** `SeatCapacityRulesTests`

**Rule.** An offer cannot be issued when outstanding offers plus enrolled students for the grade already reach the grade's seat capacity. An override needs `admissions.capacity.override-capacity` and a recorded reason, and the resulting headcount is written to the audit trail.

**Examples.**

- Given Grade 3 with a capacity of 75, 68 enrolled students and 7 outstanding offers, when a new offer is attempted, then it is blocked with `ADMISSIONS_CAPACITY_OVERRIDE_REQUIRED`, because 68 + 7 = 75.
- Given one of those offers expiring on 2026-11-01, when a new offer is attempted on 2026-11-02, then outstanding offers are 6, the total is 74 and the offer is issued.
- Given a registrar holding the capacity-override permission issuing an offer at 75 with the reason "sibling of an enrolled student", when the offer is issued, then the headcount is recorded as 76 and the override appears in the audit trail.

**Edge cases.**

- Capacity is per grade and per campus; section capacity is a separate, later constraint applied at section assignment.
- A declined offer releases its seat immediately, and the waiting list in BR-ADM-005 is consulted on the same transaction.
- Capacity checks run inside the offer transaction, so two registrars issuing simultaneously cannot both take the last seat.

### BR-ADM-004 Offer expiry

**Owner:** Admissions · **Parameters:** General → time zone

**Tests:** `OfferExpiryRulesTests`

**Rule.** An offer expires at the end of the last day of its validity period, counted in calendar days from the issue date in the campus time zone. An expired offer cannot be accepted and has already released its held seat. An extension needs an approval and sets a new expiry date.

**Examples.**

- Given a validity of 14 days and an offer issued 2026-11-03 at 10:00 Asia/Riyadh, when the expiry is computed, then it is 2026-11-17 at 23:59:59 Asia/Riyadh.
- Given acceptance attempted at 2026-11-18 at 00:05 Asia/Riyadh, when the acceptance is submitted, then it is rejected with `ADMISSIONS_OFFER_EXPIRED` and the seat has already been released.
- Given an approved extension of 7 days granted on 2026-11-16, when the expiry is recomputed, then it is 2026-11-24 at 23:59:59 Asia/Riyadh and the seat stays held.

**Edge cases.**

- Expiry is a scheduled job plus a check at acceptance time, so an offer is never acceptable just because the job has not run yet.
- Expiring publishes `admissions.offer.expired.v1`, which both releases capacity and advances the waiting list.
- A deposit already paid against an expired offer becomes an unapplied credit under BR-FIN-009 and is never forfeited automatically.

### BR-ADM-005 Waiting-list ranking with sibling priority

**Owner:** Admissions · **Parameters:** none

**Tests:** `WaitingListRankingRulesTests`

**Rule.** The waiting list is ordered by priority band first — staff child, then sibling of an enrolled student, then general — and within a band by the application completion timestamp ascending. The assessment score breaks a timestamp tie.

**Examples.**

- Given a general applicant completed 2026-10-01 09:00, a sibling applicant completed 2026-10-05 11:00 and a staff child completed 2026-10-09 15:00, when the list is ranked, then the order is the staff child, then the sibling, then the general applicant.
- Given two sibling applicants both completed 2026-10-05 at 11:00 with assessment scores of 78 and 84, when the list is ranked, then the applicant scoring 84 ranks ahead.
- Given a seat opening on 2026-11-02, when the list is processed, then the top-ranked applicant receives an automatic offer and every remaining applicant moves up one rank.

**Edge cases.**

- The completion timestamp is when the application became complete, including documents, not when it was started, so an early but incomplete application does not hold a place.
- Losing eligibility for a band, for example a sibling who withdraws, re-ranks the applicant into the general band from that date, and the applicant is notified.
- An applicant who declines an automatic offer leaves the list unless they ask to be reinstated, which restores their original completion timestamp.

### BR-ADM-006 Duplicate applicant detection

**Owner:** Admissions · **Parameters:** General → languages

**Tests:** `DuplicateApplicantRulesTests`

**Rule.** An incoming application is flagged as a possible duplicate when its normalized name matches an existing applicant or student and either the date of birth or the national identifier also matches, or when the national identifier matches on its own. A flagged application cannot be converted to an enrollment until a registrar resolves the flag.

**Examples.**

- Given an existing student named "محمد أحمد الزهراني" born 2018-04-02 and an application for "محمّد احمد الزهرانى" born 2018-04-02, when the check runs, then the normalized names match under BR-L10N-001, the dates of birth match and the application is flagged.
- Given an application whose national identifier matches an enrolled student but whose name is completely different, when the check runs, then the application is still flagged with `ADMISSIONS_DUPLICATE_APPLICANT`, because an identifier match alone is enough.
- Given a registrar resolving the flag on 2026-10-12 with the reason "twin sibling, not a duplicate", when the conversion is retried, then it proceeds and the resolution is stored with the registrar's name and the timestamp.

**Edge cases.**

- Twins share a surname and a date of birth and are the most common false positive, which is why resolution is a human decision and never automatic.
- The check runs again at conversion time, because a duplicate can be created between submission and enrollment.
- Merging two records is a separate, audited operation in the School service and never happens inside Admissions.

---

## S.6 Identity and permissions (IDN)

### BR-IDN-001 Permission dependency

**Owner:** Identity · **Parameters:** none

**Tests:** `PermissionDependencyRulesTests`

**Rule.** Every action permission declares the permissions it depends on, and a role must hold all of them. Editing always requires viewing on the same resource. A role saved without a declared dependency is rejected, and removing a dependency from a role that still holds the dependent action is also rejected.

**Examples.**

- Given a role holding `school.students.edit` and not `school.students.view`, when the role is saved, then it is rejected and the missing dependency `school.students.view` is named.
- Given the same role saved holding both permissions, when a later edit removes `school.students.view`, then the removal is rejected while `school.students.edit` is still present.
- Given a role holding `school.students.delete` and `school.students.view` but not `school.students.edit`, when the role is saved, then it is rejected, because delete depends on edit, and the chain delete to edit to view is resolved transitively.

**Edge cases.**

- Dependencies are declared in the permission catalog in Appendix B, not inferred from the permission name, so `export` depending on `view` is explicit.
- A dependency chain is resolved transitively and checked for cycles at startup; a cycle fails the service's start-up validation.
- Granting a permission through a delegation still requires the dependencies at the moment of use, under BR-IDN-003.

### BR-IDN-002 Data-scope evaluation order

**Owner:** Identity · **Parameters:** none

**Tests:** `DataScopeEvaluationRulesTests`

**Rule.** Authorization is evaluated in a fixed order: tenant, then campus, then the role's data scope, then row-level security in the database. A denial at any stage ends the evaluation. Several scopes on one role form a union of rows; scopes from different stages intersect, so a scope can only ever narrow.

**Examples.**

- Given a teacher whose scope is "own sections and groups", holding 4-B and 5-A, when the teacher queries section 6-C, then no rows are returned and a direct fetch of a 6-C student returns 403.
- Given the same teacher also granted the scope "own children" for one enrolled child in 6-C, when the teacher queries 6-C, then only that child's rows are visible and the rest of 6-C is not.
- Given a head of year scoped to campus Riyadh-North with the role scope "all tenant", when a section on campus Riyadh-South is queried, then no rows are returned, because the campus stage has already narrowed the result.

**Edge cases.**

- Row-level security is the second barrier, not the first; a bug in the application scope must still produce no rows, and the tenancy tests assert exactly that.
- An empty scope means no rows, never all rows; a role saved with an action permission and no scope is rejected.
- The evaluation order is identical in every service, and a service that reorders it is a defect.

### BR-IDN-003 Delegation validity window

**Owner:** Identity · **Parameters:** General → time zone

**Tests:** `DelegationWindowRulesTests`

**Rule.** A delegation grants the delegate the delegator's named permissions only between its start and end instants in the campus time zone. It can never grant a permission the delegator does not hold at the moment of use, and it is not transitive.

**Examples.**

- Given a delegation from the principal to the deputy from 2027-03-01 00:00 to 2027-03-10 23:59 Asia/Riyadh, when the deputy approves a request at 2027-03-05 09:00, then the approval is accepted and records both the deputy and the principal.
- Given the same delegation and an approval attempted at 2027-03-11 08:00 Asia/Riyadh, when it is submitted, then it is rejected, because the delegation window closed at 23:59 on 10 March.
- Given the principal losing `finance.refunds.approve` on 2027-03-04, when the deputy attempts to use it on 2027-03-05, then it is rejected, because a delegation can never exceed the delegator's live permissions.

**Edge cases.**

- A delegate cannot re-delegate; an attempt to create a delegation from a delegated permission is rejected at save.
- Revoking a delegation takes effect immediately and bumps the delegate's permission version under BR-IDN-008.
- Every action taken under a delegation names both identities in the audit trail, so "who approved this" always has two answers and both are recorded.

### BR-IDN-004 Four-eyes for high-risk grants

**Owner:** Identity · **Parameters:** Security → high-risk grant approval window

**Tests:** `HighRiskGrantRulesTests`

**Rule.** A permission marked high risk cannot be granted by one person. The grant is created as pending and takes effect only when a second administrator, who is neither the requester nor the subject, approves it inside the configured approval window.

**Examples.**

- Given `identity.roles.grant-high-risk` marked high risk, when administrator A grants it to user U, then the grant is pending and confers nothing.
- Given administrator B, who is neither A nor U, approving that pending grant on the same day, when the approval is recorded, then the grant becomes active and both A and B are stored on it.
- Given administrator A attempting to approve their own request, or user U attempting to approve a grant to themselves, when the approval is submitted, then it is rejected with `IDENTITY_SELF_APPROVAL_REFUSED`, the grant stays pending, and an unapproved grant expires after the configured window of 7 days.

**Edge cases.**

- The high-risk list comes from the permission catalog and includes at least the grant, unlock, refund, write-off, break-glass, impersonate and sensitive-export permissions.
- A tenant with only one administrator cannot grant a high-risk permission at all, which is deliberate: the fix is a second administrator, not a bypass.
- Expiry of a pending grant is recorded so that a repeated pattern of unapproved requests is visible to an access review.

### BR-IDN-005 The last super administrator cannot be removed

**Owner:** Identity · **Parameters:** none

**Tests:** `LastSuperAdminRulesTests`

**Rule.** A tenant must always have at least one active account holding the super administrator role. Removing that role from, deactivating, or deleting the last such account is rejected.

**Examples.**

- Given a tenant with exactly one active super administrator, when that role is removed from the account, then the operation is rejected, because it would leave the tenant with no active super administrator.
- Given a tenant with two active super administrators, when the role is removed from one of them, then the removal succeeds and the second account remains.
- Given a tenant with two super administrators where one account is suspended, when the role is removed from the remaining active account, then it is rejected, because a suspended account does not count as active.

**Edge cases.**

- The check runs inside the same transaction as the change, so two simultaneous removals cannot both succeed.
- The seeded administrator is subject to this rule like any other account; it has no exemption.
- Platform support cannot bypass this rule through impersonation, because impersonation acts as a tenant user and is bound by the same check.

### BR-IDN-006 Join-method default role

**Owner:** Identity · **Parameters:** Joining → enabled methods, default roles

**Tests:** `JoinMethodDefaultRoleRulesTests`

**Rule.** A person who joins through a join method receives exactly the default role configured for that method, at the campus the method names, and nothing else. A default role may never hold a high-risk permission. Every assignment records the method and the code used.

**Examples.**

- Given a parent join code for campus Riyadh-North whose default role is Guardian, when a parent joins with that code, then the account holds Guardian scoped to Riyadh-North and holds no other role.
- Given an attempt to set a join method's default role to a role holding `identity.roles.grant-high-risk`, when the setting is saved, then it is rejected and the offending permission is named.
- Given a staff invitation whose default role is Teacher with an approver configured, when the invitee registers, then the account is created inactive and becomes active holding Teacher only after the approver accepts.

**Edge cases.**

- A join code that has expired or reached its use limit creates nothing; the person sees a clear message rather than an account with no role.
- Changing a method's default role never changes roles already assigned through it, and the audit trail keeps the role that was granted at the time.
- Joining twice with two codes produces one account with two role assignments, not two accounts, deduplicated on the verified contact.

### BR-IDN-007 One person linked across tenants

**Owner:** Identity · **Parameters:** Security → login methods

**Tests:** `CrossTenantLinkingRulesTests`

**Rule.** One person may hold accounts in several tenants behind one set of credentials. Every session and every token carries exactly one tenant, permissions never cross tenants, and switching tenants issues a new token rather than widening the current one.

**Examples.**

- Given a person employed as a teacher in tenant A and registered as a parent in tenant B, when they sign in and select tenant A, then the token carries tenant A only and a request for tenant B data returns 403.
- Given the same person switching to tenant B, when the switch completes, then a new token carrying tenant B is issued and the tenant A token is not reused or widened.
- Given the person being deactivated in tenant A on 2027-04-01, when they sign in on 2027-04-02, then sign-in still succeeds and only tenant B is selectable.

**Edge cases.**

- The tenant list on the sign-in screen must not leak tenant names to someone who failed authentication; it is returned only after credentials are verified.
- Cache keys carry the full tenant identifier under the scheme in Appendix L, so a cached permission set can never be read by the wrong tenant.
- Deleting the person in one tenant never deletes the shared credential; the credential dies only with the last linked account.

### BR-IDN-008 Permission version invalidates caches

**Owner:** Identity · **Parameters:** none

**Tests:** `PermissionVersionRulesTests`

**Rule.** Every subject's effective permission set carries a version that increases whenever a role, a role assignment, a scope or a delegation changes for that subject. A token or cache entry holding an older version is stale, and the permission set is re-read before the request is authorized.

**Examples.**

- Given a cached permission set at version 7 and a role change that raises the subject's version to 8, when the next request arrives, then the stale entry raises `IDENTITY_PERMISSION_VERSION_STALE` internally, the set is re-read at version 8 and the version 7 entry is discarded.
- Given a token issued at version 7 presented after the version reached 8, when the request is authorized, then it is authorized against version 8, so a permission removed at version 8 is not honoured.
- Given a role change that affects a different subject, when this subject's next request arrives, then this subject's version is still 7 and the cached set is used without a re-read.

**Edge cases.**

- The version is per subject and per tenant, so a change in one tenant never invalidates the same person's cached set in another.
- A version that goes backwards, for example after a restore, is treated as a change and forces a re-read.
- Revocation must be effective within one request, not within a cache time-to-live, which is exactly why the version and not the expiry governs freshness.

### BR-IDN-009 Impersonation needs consent and a time box

**Owner:** Identity · **Parameters:** Security → session timeout

**Tests:** `ImpersonationRulesTests`

**Rule.** A platform operator may act as a tenant user only inside an impersonation session that the tenant has consented to. The session is time-boxed, shows a persistent banner on every screen, and writes every action to the audit trail attributed to the operator and the impersonated user together.

**Examples.**

- Given a consented impersonation window of 60 minutes starting 2026-10-06 11:00 Asia/Riyadh, when the operator acts at 11:59, then the action is allowed and recorded against both identities.
- Given the same session, when the operator acts at 12:01, then the action is rejected because the session has already closed.
- Given a tenant that has not recorded consent, when an impersonation session is started, then it is refused with `PLATFORM_IMPERSONATION_NOT_CONSENTED` and the refused attempt is itself written to the audit trail.

**Edge cases.**

- Consent is per incident and expires; a blanket, permanent consent is not offered.
- Impersonation can never read wellbeing records, which need the break-glass path in BR-WEL-002 and a tenant staff member.
- Ending the session early revokes the token immediately rather than letting it run to the end of the window.

---

## S.7 Notifications (NOT)

### BR-NOT-001 Urgency versus quiet hours

**Owner:** Notification · **Parameters:** Notifications → quiet hours default

**Tests:** `QuietHoursRulesTests`

**Rule.** A notification whose template is marked urgent is delivered immediately on every enabled channel, regardless of quiet hours. A non-urgent notification created inside quiet hours is held and released at the end of quiet hours, evaluated in the recipient's own time zone.

**Examples.**

- Given quiet hours of 21:00 to 07:00 Asia/Riyadh and a non-urgent homework notification created at 22:15 Asia/Riyadh, when delivery is scheduled, then it is held and delivered at 07:00 the next morning.
- Given an emergency broadcast marked urgent created at 22:15 Asia/Riyadh, when delivery is scheduled, then it is delivered at 22:15 on in-app, push and SMS.
- Given a recipient in Asia/Riyadh with quiet hours 21:00 to 07:00 and a non-urgent notification created at 21:30 Asia/Dubai, which is 20:30 Asia/Riyadh, when delivery is scheduled, then it is delivered immediately, because quiet hours are evaluated in the recipient's time zone and not the sender's.

**Edge cases.**

- A held notification whose subject becomes stale before release, such as a cancelled event, is dropped at release time rather than delivered late.
- Urgency is a property of the template, approved once, not a flag a sender can set per message, so it cannot be abused to bypass quiet hours.
- Quiet hours that span midnight are the normal case and must not be implemented as a simple start-less-than-end comparison.

### BR-NOT-002 Channel fallback order

**Owner:** Notification · **Parameters:** Notifications → channel availability

**Tests:** `ChannelFallbackRulesTests`

**Rule.** A notification is attempted on channels in the configured fallback order and stops at the first channel that reports delivery. A channel is skipped when the recipient disabled it, when the address is missing, or when the channel is unavailable for the tenant.

**Examples.**

- Given the order push, then email, then SMS, and a push that reports delivered, when the send completes, then no email and no SMS are sent and the log holds one attempt.
- Given a recipient with no push token and a valid email address, when the send runs, then push is skipped, email is sent and delivered, and SMS is not attempted.
- Given push failing, email bouncing and SMS enabled with credit available, when the send runs, then SMS is attempted and the delivery log holds three attempts with their individual outcomes.

**Edge cases.**

- In-app is always the last resort and is never skipped, so a notification always exists somewhere the recipient can find it.
- A channel that reports acceptance but not delivery, which is normal for SMS, counts as delivered for fallback purposes and is reconciled later by the delivery receipt.
- Fallback stops at the first delivery, so an urgent broadcast that must reach every channel is configured as a fan-out, not as a fallback chain.

### BR-NOT-003 Digest eligibility

**Owner:** Notification · **Parameters:** Notifications → digest schedule

**Tests:** `DigestEligibilityRulesTests`

**Rule.** A notification joins a digest only when its category is digestible, the recipient has chosen a digest for that category, and the notification is not urgent. A notification that joins a digest is not delivered individually.

**Examples.**

- Given a daily digest at 17:00 and 6 digestible notifications created between 08:00 and 16:00, when the digest runs, then one digest is delivered at 17:00 listing 6 items and no individual message was sent.
- Given an urgent absence alert created at 09:12 among those, when the digest runs, then the alert was already delivered immediately at 09:12 and the digest lists the remaining 5 items.
- Given a recipient with no digest preference for that category, when each notification is created, then each one is delivered individually as it arrives.

**Edge cases.**

- An empty digest is not sent at all; silence is better than a message saying there is nothing to report.
- A digest that would exceed the channel's size limit is split, and each part states which part it is.
- Changing the digest preference mid-window applies from the change forward; items already queued for the current window still go in it.

### BR-NOT-004 Deduplication window

**Owner:** Notification · **Parameters:** Notifications → templates

**Tests:** `NotificationDeduplicationRulesTests`

**Rule.** Two notifications sharing the same template, recipient and subject key inside the deduplication window collapse into one. The window defaults to 5 minutes. The first is delivered and every later duplicate is recorded as suppressed with a running count.

**Examples.**

- Given a deduplication window of 5 minutes and two "assignment updated" notifications for the same assignment to the same parent at 14:02 and 14:05, when the second arrives, then one message was delivered at 14:02 and the second is suppressed with a count of 2.
- Given the second arriving at 14:08 instead, when it is processed, then it is 6 minutes after the first delivery, outside the 5-minute window, and is delivered as a separate notification.
- Given two notifications for two different assignments at 14:02 and 14:05, when they are processed, then the subject keys differ and both are delivered.

**Edge cases.**

- The window is measured from the first delivery, not from the previous suppression, so a stream of updates cannot extend the window indefinitely.
- The suppressed count is shown in the in-app item, so the recipient knows the assignment changed three times and not once.
- Deduplication never applies to urgent templates, because two emergencies are two emergencies.

### BR-NOT-005 SMS credit check before send

**Owner:** Notification · **Parameters:** Notifications → SMS credit limits

**Tests:** `SmsCreditRulesTests`

**Rule.** An SMS is attempted only when the tenant's remaining credit covers the message's segment count. When it does not, the send is not attempted at all, the notification falls to the next channel in the order, and a low-credit alert goes to the finance administrator.

**Examples.**

- Given 100 remaining credits and a message of 2 segments, when the SMS is sent, then 2 credits are consumed and 98 remain.
- Given 1 remaining credit and a message of 2 segments, when the send is evaluated, then no SMS is attempted, the send records `NOTIFICATION_SMS_CREDITS_EXHAUSTED`, the notification falls through to email and 1 credit still remains.
- Given an Arabic message of 75 characters, when the segments are counted, then it is 2 segments, because Arabic is encoded in the 70-character Unicode segment, and 2 credits are consumed.

**Edge cases.**

- Segment counting is done on the rendered message including the tenant's sender prefix, because the prefix can push a message over a segment boundary.
- Credits are reserved before the send and released if the gateway rejects the message, so a failed send never consumes credit.
- An emergency broadcast is allowed to go into a configured negative credit buffer, and the overdraft is invoiced; nothing else may.

### BR-NOT-006 Preference resolution order

**Owner:** Notification · **Parameters:** Notifications → channel availability

**Tests:** `PreferenceResolutionRulesTests`

**Rule.** A recipient's channel preference is resolved most specific first: per child and category, then per category, then the account default, then the tenant default. The first level that names the channel wins. A channel disabled at tenant level overrides every personal preference.

**Examples.**

- Given a parent who disabled SMS for child A's attendance category while leaving SMS on at account level, when an attendance alert about child A is sent, then no SMS goes out, and when an attendance alert about child B is sent, then SMS goes out.
- Given a parent with no preference recorded at any level, when a notification is sent, then the tenant default of in-app and email applies.
- Given the tenant disabling SMS entirely on 2026-10-01, when any notification is sent after that date, then no SMS goes to any recipient, whatever their personal preference says.

**Edge cases.**

- A preference that names no channel at all is invalid; in-app cannot be switched off, so there is always one delivery target.
- Resolution is computed per notification, not cached per user, because the category and the child both participate.
- A guardian with children in two campuses can have different preferences per child, and the campus never overrides the child-level choice.

---

## S.8 Requests and workflow (RQS)

### BR-RQS-001 Approval chain routing by amount

**Owner:** Requests · **Parameters:** Requests → approval chains

**Tests:** `AmountRoutingRulesTests`

**Rule.** The request's decisive amount selects the highest required approver from ordered bands, using lower-inclusive and upper-exclusive boundaries. The chain then runs every step from the first approver up to that one, in order, and no step is skipped.

**Examples.**

- Given bands of up to SAR 5,000.00 for the head of department, above SAR 5,000.00 up to SAR 20,000.00 for the finance manager, and above SAR 20,000.00 for the principal, and a purchase request of SAR 5,000.00, when the chain is built, then it has one step: the head of department.
- Given a purchase request of SAR 5,000.01, when the chain is built, then it has two steps: the head of department, then the finance manager.
- Given a purchase request of SAR 42,000.00, when the chain is built, then it has three steps, and the principal cannot approve before the finance manager has approved.

**Edge cases.**

- Raising the amount after the chain started re-evaluates the chain and adds the missing steps without discarding approvals already given at lower steps.
- Lowering the amount never removes an approval already given; the extra approval is simply recorded.
- Bands must cover zero to unbounded with no gap; a configuration with a gap is rejected at save.

### BR-RQS-002 Approval chain routing by duration

**Owner:** Requests · **Parameters:** Requests → approval chains

**Tests:** `DurationRoutingRulesTests`

**Rule.** A leave request routes on the number of requested working days counted on the campus work calendar. Non-working days and campus holidays are excluded from the count before the bands are applied.

**Examples.**

- Given bands of 1 to 3 working days for the line manager, 4 to 10 for the principal and above 10 for the principal and then HR, and leave from Sunday 2026-11-01 to Tuesday 2026-11-03 on a Sunday-to-Thursday week, when the chain is built, then the duration is 3 working days and the approver is the line manager.
- Given leave from Thursday 2026-11-05 to Sunday 2026-11-08 on the same work week, when the chain is built, then Friday 6 November and Saturday 7 November are excluded, the duration is 2 working days and the approver is the line manager.
- Given leave of 12 working days, when the chain is built, then it has two steps, the principal and then HR, and both must approve.

**Edge cases.**

- A half day counts as 0.5 working days and the bands are compared on the decimal value, so 3.5 days routes to the principal, not the line manager.
- A campus holiday declared after submission recounts the duration and can shorten the chain; approvals already given stay.
- Two campuses in one tenant can route the same calendar range differently, because each uses its own work calendar.

### BR-RQS-003 SLA calendars per campus

**Owner:** Requests · **Parameters:** Requests → SLAs

**Tests:** `SlaCalendarRulesTests`

**Rule.** An SLA is measured in working hours on the campus's own service calendar. The clock starts when the request enters the step, pauses outside working hours and on campus holidays, and pauses while the request is waiting on the requester.

**Examples.**

- Given a campus working 07:30 to 15:30 Sunday to Thursday, an SLA of 8 working hours and a request entering the step on Thursday 2026-11-05 at 14:30, when the breach point is computed, then 1 working hour elapses on Thursday and the breach point is Sunday 2026-11-08 at 14:30.
- Given that same request sent back to the requester on Sunday 2026-11-08 at 09:00 and answered on Monday 2026-11-09 at 09:00, when the breach point is recomputed, then 8 working hours were paused, 5.5 working hours remain and the breach point moves to Monday 2026-11-09 at 14:30.
- Given the same request and 2026-11-08 declared a campus holiday, when the breach point is recomputed, then Sunday is skipped entirely and the breach point moves to Monday 2026-11-09 at 14:30.

**Edge cases.**

- The clock is per step, not per request, so a request that visits three approvers has three SLAs and three possible breaches.
- Pausing on "waiting for the requester" must be bounded, or a request can sit forever; an unanswered information request auto-closes after its own configured period.
- Daylight saving is not observed in the first-release countries, but the calculation still works on the named time zone rather than on a fixed offset.

### BR-RQS-004 Auto-approval conditions

**Owner:** Requests · **Parameters:** Requests → approval chains

**Tests:** `AutoApprovalRulesTests`

**Rule.** A request auto-approves only when every configured condition holds at once: the request type allows auto-approval, the decisive value falls inside the auto-approval band, the requester has no open policy breach, and any required balance covers the request. The decision is recorded as a system decision naming the conditions that matched.

**Examples.**

- Given a leave type that auto-approves up to 1 working day when the balance covers it, and a teacher with 4.5 days of balance requesting 1 working day, when the request is submitted, then it auto-approves and the decision records "system, inside band, balance 4.5".
- Given the same teacher requesting 2 working days, when the request is submitted, then it does not auto-approve and it routes to the line manager.
- Given the same teacher with 0.5 days of balance requesting 1 working day, when the request is submitted, then it does not auto-approve even though the duration is inside the band, and it routes to the line manager.

**Edge cases.**

- Auto-approval still runs the effects in BR-RQS-006; it removes the human step, not the consequences.
- A request type that allows auto-approval and has no band configured never auto-approves, because an absent band is not an infinite band.
- Auto-approval is visible in the request timeline as a decision by the system, never as a decision by the requester.

### BR-RQS-005 Escalation on breach

**Owner:** Requests · **Parameters:** Requests → SLAs

**Tests:** `SlaEscalationRulesTests`

**Rule.** When a step's SLA breaches, the request escalates once to the configured escalation target and stays assigned to the current approver. It re-escalates only after the configured re-escalation interval. Escalation never changes who is allowed to approve.

**Examples.**

- Given an 8-working-hour SLA that breaches at 2026-11-08 14:30 and an escalation target of the principal, when the breach is detected, then one escalation notification goes to the principal at 14:30 and the step stays assigned to the current approver.
- Given a re-escalation interval of 8 working hours and the request still open, when the interval elapses, then a second escalation fires at 2026-11-09 14:30 and not before.
- Given the approver acting at 2026-11-09 10:00, when the decision is recorded, then no second escalation fires and the escalation record closes with the resolution time.

**Edge cases.**

- The escalation target may be a role rather than a person, so a vacant post does not swallow the escalation.
- A breach publishes `requests.request.sla-breached.v1` once per breach, and the Notification service decides delivery under BR-NOT-001.
- If the escalation target is also the current approver, one notification is sent and it is labelled as an escalation, so the metric is still counted.

### BR-RQS-006 Effect execution and compensation

**Owner:** Requests · **Parameters:** Requests → fees

**Tests:** `RequestEffectSagaRulesTests`

**Rule.** A request's effects run as an ordered saga after the final approval. Every effect is idempotent on the request identifier and declares a compensation. If any effect still fails after its retries, the completed effects are compensated in reverse order and the request returns to the approved state with the failure recorded.

**Examples.**

- Given a transfer-certificate request whose effects are "post a SAR 250.00 fee", "generate the certificate" and "set the student status to transferred", when certificate generation fails after 3 retries, then the SAR 250.00 fee is reversed by a credit note, the status is not changed and the request shows "effects failed".
- Given that same request retried successfully on 2027-05-04, when the saga completes, then the fee posts once, the certificate is generated once and the status changes once, because every effect is idempotent on the request identifier.
- Given the effect message delivered twice by the broker, when the second delivery is consumed, then it is a no-op and no second SAR 250.00 fee is posted.

**Edge cases.**

- Compensation is not rollback: a posted fee is reversed by a credit note under BR-FIN-014, never deleted.
- An effect with no possible compensation, such as an email already sent, is ordered last so that it runs only after everything reversible has succeeded.
- A saga stuck in compensation raises an operator alert and appears in the failed-message console; it never resolves itself silently.

---

## S.9 Wellbeing (WEL)

### BR-WEL-001 Visibility levels

**Owner:** Wellbeing · **Parameters:** none

**Tests:** `WellbeingVisibilityRulesTests`

**Rule.** Every wellbeing record carries exactly one of four visibility levels: clinic only, named care team, teaching staff summary, and guardian visible. A reader sees a record only when they hold the wellbeing permission and belong to that level's audience. Visibility never widens by inheritance from the student record.

**Examples.**

- Given a counseling note at "named care team" and a class teacher who is not on that care team, when the teacher opens the student's profile, then the note is invisible and it appears in no search result and on no timeline.
- Given an allergy record at "teaching staff summary", when any teacher assigned to that student opens the profile, then the allergy badge and the emergency instruction are shown and no symptoms, treatment notes or attachments are shown.
- Given a clinic visit at "clinic only" and a guardian opening the parent application, when the profile loads, then the guardian sees only that a clinic visit occurred on that date, if the tenant enables visit notices, and never the record itself.

**Edge cases.**

- A record cannot be moved to a wider level without a named permission and a reason, and the change is audited.
- Student 360 and global search exclude wellbeing records entirely unless the reader holds the specific permission, so absence of a result is not evidence of absence of a record.
- An attachment inherits its record's level and is served through the same check, never through a shareable direct link.

### BR-WEL-002 Break-glass access

**Owner:** Wellbeing · **Parameters:** Security → session timeout

**Tests:** `BreakGlassRulesTests`

**Rule.** A staff member holding `wellbeing.break-glass.use` may open a restricted record without belonging to its audience, only by declaring an emergency reason. The access is time-boxed to the configured window, notifies the record owner and the safeguarding lead immediately, and is reviewed afterwards.

**Examples.**

- Given a break-glass window of 30 minutes and a nurse opening a restricted record at 2026-12-02 08:10 with the reason "anaphylaxis in progress", when the access starts, then it ends at 08:40 and both the record owner and the safeguarding lead are notified at 08:10.
- Given a further view attempted at 08:41, when the request arrives, then it is denied with `WELLBEING_BREAK_GLASS_EXPIRED` and a new break-glass declaration is required.
- Given the review on 2026-12-05 finding the access unjustified, when the review is recorded, then the finding is stored against the staff member and an access-review task is raised; the access entry itself is never deleted from the audit trail.

**Edge cases.**

- Break-glass is high risk, so granting the permission itself needs four-eyes under BR-IDN-004.
- The declared reason is free text and is treated as evidence, so it is retained under the safeguarding retention period and not the general one.
- A platform operator can never break glass, because impersonation under BR-IDN-009 cannot reach wellbeing records at all.

### BR-WEL-003 Events carry no clinical detail

**Owner:** Wellbeing · **Parameters:** none

**Tests:** `WellbeingEventPayloadRulesTests`

**Rule.** Events the Wellbeing service publishes carry identifiers, a category and a severity and nothing else. No symptom, diagnosis, medication name, note text or attachment ever leaves the service on the message bus or in a projection.

**Examples.**

- Given a clinic visit recording "asthma attack, salbutamol administered", when `wellbeing.clinic-visit.recorded.v1` is published, then the payload carries the tenant identifier, the student identifier, the category "clinic-visit" and the severity "high", and no clinical text of any kind.
- Given a subscriber that needs the detail, when it tries to read it, then it must call the Wellbeing service with the reader's own permissions, and that call is logged like any other access.
- Given the Reporting service projecting wellbeing data, when a wellbeing report is generated, then it holds counts by category and severity only, so it can state "3 clinic visits in November 2026" and can never state why.

**Edge cases.**

- The payload schema is enforced by a contract test, so adding a field with clinical content fails the build rather than the audit.
- Notification templates for wellbeing events are written to say what happened without saying what is wrong, because a push preview appears on a lock screen.
- The wellbeing database is separate and field-encrypted, so a projection cannot obtain the detail even by reading tables directly.

### BR-WEL-004 Medication authorization and sending home

**Owner:** Wellbeing · **Parameters:** none

**Tests:** `MedicationAuthorizationRulesTests`

**Rule.** Medication is administered only against a live guardian authorization naming the medication and the exact dose. A student is sent home only after a guardian with pickup rights records consent, or the school's defined escalation is exhausted and the escalation is recorded.

**Examples.**

- Given a guardian authorization for paracetamol 250 mg valid to 2027-06-30, when a dose of 250 mg is administered on 2027-01-14, then it is allowed and recorded with the administering nurse, the time and the dose.
- Given a requested dose of 500 mg against that same authorization, when administration is attempted, then it is blocked, because the dose does not match the authorization.
- Given an authorization that expired on 2026-12-31 and a request on 2027-01-14, when administration is attempted, then it is blocked with `WELLBEING_MEDICATION_NOT_AUTHORIZED` and the guardian is contacted for a new authorization.

**Edge cases.**

- An emergency medication such as an adrenaline auto-injector is authorized as a standing instruction and is administered first, with the notification following immediately.
- A guardian without pickup rights cannot consent to sending a student home, even if they are the one who answers the telephone.
- Every attempt, including a blocked one, is recorded, because the pattern of blocked attempts is itself safeguarding information.

---

## S.10 Tenancy and platform (PLT)

### BR-PLT-001 Soft warn then hard block on plan limits

**Owner:** Platform · **Parameters:** none

**Tests:** `PlanLimitRulesTests`

**Rule.** A metered limit warns at the configured soft threshold, warns again at 100%, and hard-blocks only the action that would exceed the limit, and only after the configured grace period from first reaching 100%. Reads, exports and reports are never blocked.

**Examples.**

- Given a plan limit of 1,200 active students, a soft threshold of 90% and a grace period of 14 days, when the count reaches 1,080, then a soft warning fires.
- Given the count reaching 1,200 on 2026-10-10, when the limit is evaluated, then a second warning fires and enrollments still succeed until the end of 2026-10-24.
- Given an enrollment attempted on 2026-10-25 with the count still at 1,200, when it is submitted, then it is blocked with `PLATFORM_PLAN_LIMIT_REACHED`, while every read, export and report continues to work.

**Edge cases.**

- The grace period restarts only when usage falls back below the limit, so a tenant cannot reset it by oscillating.
- Blocking is per metered action; hitting the student limit never blocks marking attendance for the students already enrolled.
- An upgrade takes effect immediately and clears the block in the same transaction, without waiting for the next billing cycle.

### BR-PLT-002 What read-only mode allows

**Owner:** Platform · **Parameters:** none

**Tests:** `ReadOnlyModeRulesTests`

**Rule.** A suspended tenant runs in read-only mode: sign-in, reads, exports and payment of platform invoices all work. Every write that changes tenant data is rejected, and no scheduled job that writes tenant data runs.

**Examples.**

- Given a tenant suspended on 2026-11-01, when a teacher saves attendance, then the save is rejected with `PLATFORM_TENANT_SUSPENDED`.
- Given the same suspended tenant, when an administrator exports the student list, then the export runs and completes normally.
- Given the nightly invoice run scheduled for that tenant, when the scheduler reaches it, then the run is skipped and recorded as skipped with the reason, and it executes on the first run after reactivation.

**Edge cases.**

- Paying the platform invoice is the one write that must work, because it is the path out of suspension.
- Safeguarding and emergency contact reads always work, because a suspension is a commercial event and never a safety event.
- Reactivation replays nothing automatically except the skipped billing job; skipped notifications are dropped rather than delivered late.

### BR-PLT-003 Deletion cooling-off

**Owner:** Platform · **Parameters:** Security → retention periods

**Tests:** `TenantDeletionCoolingOffRulesTests`

**Rule.** A tenant deletion request starts a cooling-off period during which the tenant is read-only and recoverable by the tenant owner alone. After the period, the deletion saga runs across every data-owning service and ends with a certificate of deletion.

**Examples.**

- Given a cooling-off period of 30 days and a deletion requested on 2027-01-10, when the request is accepted, then the tenant becomes read-only on 10 January and irreversible deletion starts on 2027-02-09.
- Given the owner cancelling on 2027-02-05, when the cancellation is recorded, then the tenant returns to its previous plan and state and nothing has been deleted.
- Given the saga running on 2027-02-09 with one service failing to confirm, when the failure is detected, then the saga retries that service, withholds the certificate and raises an operator alert; the certificate is issued only when every service has confirmed.

**Edge cases.**

- Export stays available throughout the cooling-off period under BR-PLT-006, so nobody loses data by hesitating.
- The certificate names each service, the row counts removed and the completion time, and is retained after the tenant is gone.
- Backups are purged on their own schedule after the certificate, and the certificate states the date on which the last backup copy expires.

### BR-PLT-004 Data residency pinning at provisioning

**Owner:** Platform · **Parameters:** none

**Tests:** `DataResidencyRulesTests`

**Rule.** A tenant's data region is chosen at provisioning and is immutable thereafter. Every database, object store, cache and backup for that tenant stays in that region. Moving a tenant means a new tenant in the target region plus an export and an import, never an in-place change.

**Examples.**

- Given a tenant provisioned in the Saudi Arabia region on 2026-09-01, when its storage is inspected, then its PostgreSQL database, its object storage bucket, its cache and its backups are all in that region and no row is written elsewhere.
- Given a request to change that tenant's region to the United Arab Emirates, when it is submitted, then it is rejected, and the supported path offered is provisioning a new tenant in that region and migrating by export and import.
- Given a misconfigured service attempting a cross-region read for that tenant, when the connection is opened, then it is rejected at the connection layer with `PLATFORM_RESIDENCY_VIOLATION` and the attempt is recorded.

**Edge cases.**

- A school group spanning two countries either accepts one region for the whole tenant or runs two tenants; a tenant is never split across regions.
- Platform-level data such as the tenant directory holds no tenant content and is therefore not subject to the pin, which is stated explicitly so that nobody assumes otherwise.
- Disaster recovery copies stay inside the region, which constrains the recovery design and is recorded as such.

### BR-PLT-005 Usage metering counts

**Owner:** Platform · **Parameters:** none

**Tests:** `UsageMeteringRulesTests`

**Rule.** Usage meters are computed once per day from the owning service's own data and stored as an immutable daily fact per tenant and meter. A month's billable figure is the aggregate the plan names: the maximum daily value for headcount meters and the sum for event meters.

**Examples.**

- Given daily active-student counts of 1,240 on 1 to 27 October 2026 and 1,252 on 28 to 31 October 2026, when the October headcount meter is aggregated, then it is the maximum, 1,252.
- Given SMS segment counts of 400, 350 and 610 on three days of October 2026 and none on the other days, when the October SMS meter is aggregated, then it is the sum, 1,360.
- Given a back-dated correction on 2026-11-03 changing the 28 October count to 1,248, when the meter is recomputed, then the 28 October fact is superseded by a corrected fact, the October headcount becomes 1,248 and the correction is audited.

**Edge cases.**

- A daily fact is never updated in place; a correction writes a new fact that supersedes the old one, so the billing history is reproducible.
- A day with no data is a zero fact, not a missing fact, so a gap in the job's execution is visible rather than silently ignored.
- The headcount meter must reconcile with the Finance figure in BR-FIN-017; a mismatch fails the nightly reconciliation and blocks invoicing until it is explained.

### BR-PLT-006 Tenant export completeness

**Owner:** Platform · **Parameters:** Security → export approval rules

**Tests:** `TenantExportRulesTests`

**Rule.** A tenant can always export its data, including while suspended and during the deletion cooling-off period. The export covers every data-owning service and completes with a manifest naming each service, its row counts and a checksum.

**Examples.**

- Given a tenant on the smallest plan requesting an export, when the export runs, then it completes, because export is never metered or plan-gated.
- Given a suspended tenant requesting an export on 2026-11-02, when the export runs, then it completes and the manifest lists all 20 data-owning services.
- Given one service failing to produce its part, when the export finishes, then it is marked incomplete, no manifest is issued, and the failure names the service so the export can be retried.

**Edge cases.**

- Sensitive exports still require the export approval configured for the tenant; the right to export is not a bypass of the tenant's own controls.
- Wellbeing data is exported in its own encrypted part with its own access record, so it is never mixed into the general archive.
- The checksum lets the tenant prove the archive is the one the platform produced, which is what makes the certificate of deletion meaningful.

---

## S.11 Localization (L10N)

### BR-L10N-001 Arabic search normalization

**Owner:** Platform · **Parameters:** General → languages

**Tests:** `ArabicNormalizationRulesTests`

**Rule.** Arabic text is normalized identically before indexing and before matching: the alef forms أ, إ, آ and ٱ fold to ا, alef maqsura ى folds to ي, ta marbuta ة folds to ه, tatweel is removed, and diacritics are stripped. The stored display value is never modified.

**Examples.**

- Given a stored name of "مُحمَّد" and a query of "محمد", when the search runs, then the record matches, because diacritics are stripped on both sides, and the name still displays as "مُحمَّد".
- Given a stored name of "فاطمة" and a query of "فاطمه", when the search runs, then the record matches, because ta marbuta folds to ه.
- Given a stored name of "إبراهيم" and a query of "ابراهيم", when the search runs, then the record matches; and given a query of "ابرهيم", then it does not match, because normalization folds letters and never deletes them.
- Given a student stored as "يوسف" with the English name part "Yousef", and a Latin query of "Yousef" in the admissions or directory search, when the search runs, then the record matches through the English name part; and given the same student with the English name part empty, then the cross-script key does not match, because و written as a long vowel keeps a `w` in the Arabic key (`wsf`) that the Latin spelling drops (`sf`).

**Edge cases.**

- Normalization runs in the database so that the index and the query agree; doing it only in application code produces a search that silently misses rows.
- The same normalization applies to the duplicate detection in BR-ADM-006, so search and deduplication cannot disagree.
- Latin text passes through unchanged apart from case folding, so a bilingual name field works with one index.
- A query in one script against a name stored in the other is matched on a consonant key derived from both scripts, used only by duplicate detection and the admissions and directory search. The key cannot see long vowels written as و or ي in Arabic, so pairs such as "يوسف" and "Yousef" rely on the English name part, and a person confirms every duplicate under BR-ADM-006.

### BR-L10N-002 Numeral rendering

**Owner:** Platform · **Parameters:** General → numerals

**Tests:** `NumeralRenderingRulesTests`

**Rule.** Numbers are stored and transmitted as Western Arabic digits. The numeral system used for display comes from the tenant numeral setting and can be overridden per user. It affects rendering only: parsing accepts both systems, and identifiers are never converted.

**Examples.**

- Given the numeral setting Arabic-Indic and a mark of 87.5, when the mark is displayed, then it renders as "٨٧٫٥" and the stored value remains 87.5.
- Given a user typing "١٢٣٤" into an amount field, when the form is submitted, then it parses to 1234 and posts as 1234.
- Given the invoice number INV-2026-0418 with the Arabic-Indic setting on, when the receipt is rendered, then the number appears unchanged as INV-2026-0418, because identifiers are never converted.

**Edge cases.**

- A CSV or JSON export always uses Western Arabic digits with the invariant decimal separator, whatever the display setting says, so downstream systems parse it.
- The Arabic decimal separator ٫ and thousands separator ٬ are rendering characters and never reach the parser.
- A mixed-numeral input such as "12٣4" is rejected with a clear message rather than parsed on a best-effort basis.

### BR-L10N-003 Hijri display, Gregorian source of truth

**Owner:** Scheduling · **Parameters:** General → calendars, time zone

**Tests:** `HijriDisplayRulesTests`

**Rule.** Every date is stored, compared, sorted and computed in the proleptic Gregorian calendar with a named time zone. Hijri is a display conversion using the Umm al-Qura calendar, and a Hijri date a user enters is converted to Gregorian at save time.

**Examples.**

- Given a stored attendance date of 2026-09-19 and a user with Hijri display on, when the register renders, then the screen shows the Umm al-Qura equivalent while the stored value, the sort order and every comparison still use 2026-09-19.
- Given a parent typing a Hijri date of birth into the admission form, when the form is saved, then it is converted to a Gregorian date and the Gregorian value is what the age check in BR-ADM-001 uses.
- Given a report grouped by month with Hijri display on, when it is generated, then the grouping still uses Gregorian months, so 2026-09-01 to 2026-09-30 forms one group, and the group label shows both calendars.

**Edge cases.**

- Umm al-Qura is defined only over a bounded range of years; a date outside that range renders in Gregorian with a note rather than throwing.
- A Hijri month boundary can differ by one day between authorities, which is exactly why no computation is ever done in Hijri.
- Both calendars appear together on printed documents, so a certificate is readable to a ministry and to a parent alike.

### BR-L10N-004 Amounts in words in both languages

**Owner:** Finance · **Parameters:** Finance → receipt layout

**Tests:** `AmountInWordsRulesTests`

**Rule.** A receipt prints the amount in words in English and in Arabic, generated from the decimal value and the currency's minor-unit name. The minor unit is always stated, even when it is zero, and the phrase ends with the word "only".

**Examples.**

- Given SAR 1,234.50, when the receipt renders, then the English words are "One thousand two hundred thirty-four Saudi riyals and fifty halalas only" and the Arabic words state the same value with the same minor unit.
- Given SAR 2,000.00, when the receipt renders, then the English words are "Two thousand Saudi riyals and zero halalas only", because the minor unit is always stated.
- Given JOD 45.005, when the receipt renders, then the minor unit carries three decimals and the English words are "Forty-five Jordanian dinars and five fils only".

**Edge cases.**

- The words are generated from the stored decimal, never from the formatted string, so a display numeral setting cannot change them.
- Arabic number words agree in gender and case with the currency noun, which is why the generator is table-driven per currency and not a generic number-to-words function.
- A negative amount, which appears on a refund receipt, is rendered with an explicit word for the sign rather than a minus character that can be lost in printing.

### BR-L10N-005 Arabic plural forms

**Owner:** Notification · **Parameters:** Notifications → templates

**Tests:** `ArabicPluralRulesTests`

**Rule.** Arabic messages select among the six plural categories zero, one, two, few, many and other from the numeric value, while English selects among one and other. A template that supplies fewer categories than its language requires fails validation and cannot be published.

**Examples.**

- Given the counts 0, 1, 2, 3 and 11 in an Arabic message, when the category is selected, then it is zero, one, two, few and many respectively.
- Given the count 100 in an Arabic message and in the English message, when the category is selected, then both select other.
- Given an Arabic template that defines only one and other, when it is published, then publication is rejected and the missing categories zero, two, few and many are named in the error.

**Edge cases.**

- Category selection is driven by the count, never by string concatenation of a number and a noun, which is the defect this rule prevents.
- A template variable that can be zero must have a zero form written deliberately, because "0 students" reads badly in both languages.
- The same six categories apply to in-app text, push text, email subject lines and SMS bodies, so the validation runs once per template and covers every channel.

### BR-L10N-006 Pinned culture on every host

**Owner:** Platform · **Parameters:** none

**Tests:** `CultureInvarianceRulesTests`

**Rule.** Every culture-sensitive operation names its culture explicitly: the invariant culture for parsing, formatting and comparing stored values, and the request culture for display only. No result may depend on the host operating system, its ICU version, or the machine's locale.

**Examples.**

- Given the amount 1234.5 formatted for storage or for an integration payload, when it is written, then it is "1234.5" on a Windows host and on a Linux host alike, because the invariant culture is named.
- Given a Turkish display culture and an identifier compared case-insensitively, when the comparison runs, then ordinal comparison is used and "INVOICE" matches "invoice" identically on both hosts, because the Turkish dotless-i rule never reaches identifier comparison.
- Given the date 2026-09-19 persisted to the database or to a file, when it is written, then it is "2026-09-19" on both hosts, and a test asserting a machine-locale date format is itself the defect.

**Edge cases.**

- Sorting a list for display uses the request culture and can legitimately differ from the database's collation; the two are never assumed to agree.
- A test that passes on a developer's Windows machine and fails in the Linux pipeline is almost always an unpinned culture, so the build pins the culture explicitly in the test host too.
- String comparison for security decisions, such as permission names and tenant identifiers, is always ordinal, never culture-aware.

### BR-L10N-007 Bilingual names and fallback

**Owner:** School · **Parameters:** General → languages

**Tests:** `BilingualNameRulesTests`

**Rule.** A person carries name parts in both languages where the school collects them. A screen shows the name in the active language, and when that language's parts are empty it falls back to the other language and marks the value as a fallback for data-quality reporting.

**Examples.**

- Given a student with Arabic name parts filled and English parts empty, when an English screen renders the name, then it shows the Arabic name and the record is counted as an English-name gap in the data-quality report.
- Given a student with both languages filled, when the screens render, then the English screen shows the English name, the Arabic screen shows the Arabic name and no gap is recorded.
- Given a certificate template printing both names side by side, when it is generated, then the Arabic side renders right to left, the English side renders left to right on the same page, and neither name is transliterated automatically.

**Edge cases.**

- Name parts are stored in culturally correct parts, so a four-part Arabic name is never squeezed into a first and last name pair.
- Sorting a class list uses the active language's parts with the fallback applied, so the order is stable rather than dependent on which records happen to be complete.
- Automatic transliteration is never written into the record; it may be offered as a suggestion that a human accepts.
