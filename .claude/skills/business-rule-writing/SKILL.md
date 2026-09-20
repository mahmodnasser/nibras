---
name: business-rule-writing
description: How to write a business rule with worked examples that survive review. Load before writing or changing any BR entry, fee calculation, grading scheme, GPA, threshold, or discount.
---

# Business rule writing

Every rule in this product eventually becomes an invoice a parent disputes or a grade a student appeals. Write it so the arithmetic can be checked by someone who did not write it.

## Required shape

`BR-<AREA>-<NNN>` from the canonical registry, written into `docs/brief/02-appendices/appendix-s-business-rules.md`, then:

1. **Statement.** One sentence, in the active voice, naming who the rule applies to.
2. **Inputs.** Each with its type, unit, and source, including currency and whether a percentage is a fraction or a whole number.
3. **The computation, step by step**, with the **order of operations stated explicitly**, because discount-before-tax and tax-before-discount give different answers.
4. **Rounding.** Where it happens, to how many places, in which direction, and whether it is per line or per total. Money rounds once, at a named point.
5. **At least three worked examples**, and they must include a boundary and an awkward case, not three comfortable ones.
6. **Rejected inputs.** What the rule refuses, and the error code it returns.
7. **The named test class**, so the rule and its test cannot drift apart.

## Worked example

**BR-FIN-007. Sibling discount applies to tuition only, after scholarship and before tax.**

Inputs: tuition (decimal, tenant currency), scholarship (whole-number percentage of tuition), sibling rank (integer, 1 for the eldest enrolled), tax rate (whole-number percentage).

Order: scholarship, then sibling discount, then tax. Sibling discount is 0% for rank 1, 10% for rank 2, 15% for rank 3 and beyond. It never applies to transport, activity, or uniform lines.

Rounding: each line rounds half away from zero to two places **after** its own discounts and **before** tax; tax rounds once per invoice.

| Given | Compute | Expected |
|---|---|---|
| Tuition 10,000; scholarship 0%; rank 2; tax 15% | 10,000 → 9,000 after 10% → tax 1,350 | 10,350 |
| Tuition 10,000; scholarship 20%; rank 3; tax 15% | 8,000 → 6,800 after 15% → tax 1,020 | 7,820 |
| Tuition 3,333.33; scholarship 10%; rank 2; tax 5% | 2,999.997 → rounds to 3,000.00 → 2,700.00 → tax 135.00 | 2,835.00 |
| Tuition 10,000 plus transport 2,000; rank 2; tax 15% | Discount touches tuition only: 9,000 + 2,000 = 11,000 → tax 1,650 | 12,650 |

The third example exists to pin the rounding point; the fourth exists to pin the scope. Both would be wrong under a plausible misreading, which is why they are in the rule and not left to the test.

Rejected: a negative tuition returns `FINANCE_INVALID_AMOUNT`; a sibling rank below 1 returns `FINANCE_INVALID_SIBLING_RANK`.

Tests: `SiblingDiscountTests`.