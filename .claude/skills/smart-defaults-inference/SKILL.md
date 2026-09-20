---
name: smart-defaults-inference
description: How country and school type infer calendar, work week, numerals, tax, terminology, and bell schedule so onboarding takes minutes. Load when designing onboarding, tenant settings, or any setup wizard.
---

# Smart defaults inference

"Go live in a day" fails on the setup wizard, not on the import. A school should answer two questions and see a working configuration it can correct, not forty questions it must answer before seeing anything.

## The two questions

**Country** and **school type** (national curriculum, international, bilingual, religious, nursery). From those two, infer everything below and **show the inferences on one screen for correction**.

| Setting | Inferred from | Example |
|---|---|---|
| Work week | Country | Sunday to Thursday, or Monday to Friday |
| First day of the week | Country | Sunday, Saturday, or Monday |
| Calendar systems shown | Country and school type | Gregorian primary with Hijri secondary |
| Numerals | Country and language | Eastern Arabic or Western Arabic digits |
| Academic year shape | Country and school type | Three terms starting in September, or two semesters starting in August |
| Holidays preloaded | Country | National holidays plus the religious calendar |
| Bell schedule | School type and country | 7 periods of 45 minutes, with a Ramadan variant of 35 minutes |
| Terminology | Country and school type | "Grade" or "Year" or "Form"; "Section" or "Class" or "Homeroom" |
| Grading scheme | Country and school type | Percentage with letter bands, or 1 to 20, or standards-based |
| Tax | Country | Value-added tax at the national rate, or none |
| Currency | Country | With the correct minor-unit count |
| Document numbering | Country | Invoice numbering that satisfies the local requirement |

## Rules

- **Inference is a starting point, never a lock.** Every inferred value is editable, and the screen says it was inferred.
- **Show the inference, do not hide it.** A school that discovers in November that its week was wrong will not forgive the product.
- **Never infer anything legal or financial silently.** Tax rate and invoice numbering are confirmed explicitly, by a named person, with the confirmation recorded.
- **The inference table is data, not code.** A new country is a settings row and a test, not a release.
- **Every inferred default has a test** asserting the whole inferred set for that country and school type, so a change to one row cannot quietly move another.

## Worked example

A user picks **Saudi Arabia** and **bilingual private school**:

| Setting | Inferred | Confidence | Confirm explicitly |
|---|---|---|---|
| Work week | Sunday to Thursday | High | No |
| First day | Sunday | High | No |
| Calendars | Gregorian primary, Hijri secondary | High | No |
| Numerals | Western Arabic digits, Arabic interface | Medium | Yes, shown on the review screen |
| Year shape | Two semesters from August | Medium | Yes |
| Bell schedule | 7 periods of 45 minutes, Ramadan variant 35 minutes | Medium | Yes |
| Terminology | Grade, Section, Guardian | High | No |
| Tax | Value-added tax at the national rate | High | **Yes, always** |
| Currency | Saudi riyal, 2 minor units | High | No |

The wizard shows nine inferred rows on one screen with a single "This is correct" button and an edit control on each row. The school sees a working configuration in under a minute, then corrects the two rows that are actually unusual about them.
