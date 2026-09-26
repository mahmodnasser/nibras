# 24. Localization and Calendars

> Plan document for the Nibras platform. Group F. It refines master brief Section 17 (localization and regional fit), the time and money lines of Section 19, the seven BR-L10N rules of Appendix S, the General group of Appendix G and the ownership of terminology in Appendix L.5; it does not re-derive them. Wire shapes for bilingual text, money and dates are owned by `22-api-conventions-and-error-catalog.md` §1.3 and §1.4 and are cited, not restated. Where this document and the brief disagree, an ADR records the deviation.

**Group** F · **Requirement areas covered** L10N (all of it), UX (right-to-left and bidirectional rules), PLAT (culture and time-zone determinism), FIN (amounts in words and rounding only) · **Last updated** 2026-09-21 by the platform plan

## Purpose

This document lets an engineer add a screen, a document, a notification or a search in Nibras that is correct in Arabic and English on the first attempt, and lets the Arabic reviewer know what they own. It fixes how a string becomes a key and reaches a reviewer, how bilingual data is stored and which language prints on which document, the exact Arabic normalization behind every search with its database objects, where Arabic-Indic numerals appear and where they never do, how Hijri dates are shown without ever being computed with, how time zones, holidays, work weeks and Ramadan timetables behave in Saudi Arabia, the United Arab Emirates and Jordan, how amounts are written in words, how the six Arabic plural categories are selected, the right-to-left rules for every surface, and the culture rule that makes Windows and Linux produce the same bytes. The readers are the web, mobile and backend engineers, the Arabic language owner, and the reviewer running `/audit-a11y-rtl`.

## Scope

| In scope | Out of scope | Where the out-of-scope item lives |
|---|---|---|
| Translation keys, namespaces, context notes, the missing-translation report, reviewer ownership, glossary, terminology overrides | The Angular and Flutter folder layout for `i18n/` and `l10n/` | `08-web-structure.md`, `09-mobile-structure.md` |
| The `LocalizedText` and `PersonName` value objects, which fields are bilingual, which language prints | The wire shape of bilingual text | `22-api-conventions-and-error-catalog.md` §1.3 |
| Arabic normalization, the fold function, generated columns, trigram indexes, transliteration keys | Query budgets and index maintenance per service | `21-performance-engineering.md` |
| Numerals, Hijri display, calendars, time zones, holidays, work week, Ramadan bell schedules | Timetable solving and bell-schedule aggregates | `06-services/scheduling.md` |
| Amounts in words, rounding per currency | Fee, tax, discount and allocation rules | `06-services/finance.md`, Appendix S BR-FIN rules |
| Arabic plural categories in every channel | Notification channel lanes and quiet hours | `06-services/notification.md` |
| Right-to-left rules for layout, icons, charts, documents, notifications, mixed text | Design tokens, the component inventory, the four-way snapshot tooling | `14-design-system-and-ux.md` |
| The culture-pinning rule and the three-culture test | Image checks G1 to G10 and the CI matrix | `33-platform-support-and-dev-environments.md` |
| Machine translation of school messages | | `06-services/communication.md` |

---

## Content

### 1. Translation workflow

Master brief Section 17: every string externalized, no concatenated sentences, correct plurals, keys with context notes, and a missing-translation report in CI. This section turns each clause into a mechanism.

#### 1.1 Where strings live

| Surface | Mechanism | Files | Owner of the files |
|---|---|---|---|
| Angular web and the platform console | Transloco with one scope per feature, lazy-loaded with the feature, plus the MessageFormat transpiler for plurals and selects | `libs/features/<feature>/i18n/en.json` and `ar.json`; shell strings in `apps/<app>/src/i18n/` (`08-web-structure.md`) | The feature team |
| Flutter | `gen-l10n` over ARB files with ICU plural and select syntax | `lib/l10n/app_en.arb` and `app_ar.arb` (`09-mobile-structure.md`) | The mobile team |
| Server-rendered notifications | Notification templates per language and channel, stored in `nibras_notification`, with ICU plural syntax | Seeded from `src/Services/Notification/templates/` | Notification, edited by schools under `notification.templates.edit` |
| Server-rendered documents | Documents templates per language, with labels drawn from the same key catalog as the web | Seeded from `src/Services/Documents/templates/` | Documents, edited under `documents.templates.edit` |
| Error codes | `errors.<CODE>.title` and `errors.<CODE>.message` in both bundles (`22-api-conventions-and-error-catalog.md` §12.4) | Generated key list from Appendix K | The owning service |
| Permission titles and descriptions | Appendix B requires both languages; generated into the `permissions` namespace | Generated from Appendix B | Identity |
| Setting labels and descriptions | Appendix G requires both languages; generated into the `settings` namespace | Generated from Appendix G | Platform |
| Validation codes | `validation.<code>` for the cross-service catalog (`22-api-conventions-and-error-catalog.md` §8) | `docs/api/validation-codes.json` | `BuildingBlocks` |

A string in a template, a component, a widget or a C# file that a person will read, outside these files, is a defect. The ESLint rule `@nibras/no-literal-text`, the Dart analyzer rule `avoid_hardcoded_strings` in `nibras_lints`, and a Roslyn analyzer on `Nibras.*.Api` projects for literal strings passed to user-facing result types enforce it.

#### 1.2 Keys and namespaces

| Rule | Detail | Right | Wrong |
|---|---|---|---|
| Grammar | `<namespace>.<area>.<element>[.<variant>]`, each segment camelCase ASCII, at most five segments | `attendance.session.lockBanner.title` | `Attendance.Session_Lock`, `lock_title` |
| Namespace | The feature scope (`attendance`, `finance`, `requests`), or one of the shared namespaces below | `finance.receipt.amountInWords.label` | `common.receiptAmountWords` |
| Shared namespaces | `common` (buttons and verbs), `enums` (every enum value on the wire), `errors`, `validation`, `permissions`, `settings`, `terms` (terminology, §1.6), `dates` (month and weekday names for both calendars) | `enums.studentStatus.enrolled` | a feature redefining "Save" |
| Meaning, not text | The key names the purpose, never the English wording, so a copy change does not rename the key | `requests.inbox.emptyState.title` | `requests.nothingHereYet` |
| One sentence per key | A sentence is never assembled from fragments; variables are placeholders inside one message | `"{count, plural, one {# student absent} other {# students absent}}"` | `count + " " + t('students') + " " + t('absent')` |
| Placeholders | Named, never positional; the same set in both languages; interpolated values are wrapped in bidi isolates at render (§9.6) | `{studentName}`, `{dueDate}` | `{0}`, `%s` |
| Reuse | A key is reused only when the meaning is identical in both languages; "Close" a dialog and "Close" a term are two keys | `common.dialog.close`, `school.term.close` | one `close` key |
| Removal | A key is deleted in the same change that removes its last use; the report flags unused keys | | |

#### 1.3 Context notes

Every key carries a context note in a sidecar file beside the English bundle (`i18n/context.json` for web features, `@<key>` metadata inside `app_en.arb` for Flutter, a `context` column for server templates). A translator sees the note; the product never does.

| Field | Required | Example |
|---|---|---|
| `note` | yes | "Banner on the attendance register when the session is locked after the cut-off. The teacher can still request an edit." |
| `maxLength` | when the string sits in a fixed-width control | `24` for a button |
| `placeholders` | when the message has any | `{ "count": "number of students, integer" }` |
| `gender` | when the Arabic text addresses or describes a person | `"addressee"`: the reader; the Arabic uses a gender select on `{addresseeGender}` |
| `screen` | yes | The route or document type: `/teacher/attendance/:sessionId` |
| `status` | yes | `draft` or `reviewed` (§1.5) |
| `glossaryTerms` | when the text contains a glossary term | `["term.attendanceSession"]` |

#### 1.4 The missing-translation report

One Node implementation in `tools/i18n-report/` with `.ps1` and `.sh` wrappers (Appendix X), run on every pull request and on every release branch.

| Check | Severity on a pull request | Severity on a release branch |
|---|---|---|
| A key present in one language and absent in the other, across web, Flutter, templates, and the generated `errors`, `permissions`, `settings` and `validation` namespaces | error, fails the build | error |
| An Appendix K code without both `title` and `message` in both languages (`22-api-conventions-and-error-catalog.md` §12.4, `TC-API-030`) | error | error |
| An empty value, or an Arabic value identical to the English one outside the allow-list (brand names, codes, units) | error | error |
| Placeholder sets differ between the two languages | error | error |
| An Arabic plural message missing any of zero, one, two, few, many, other; an English one missing one or other (BR-L10N-005) | error | error |
| A key without a context note | error | error |
| An Arabic value in `draft` status | warning | error: a release carries only reviewed Arabic |
| A glossary term rendered with a forbidden variant (§1.5) | error | error |
| A hard-coded user-facing string found by the three analyzers of §1.1 | error | error |
| A key no code references | warning | warning |
| A value longer than `maxLength` | warning | error |

Output: a JSON file for the pipeline and a table in the pull-request summary grouped by feature, with a count per check. The report is the proof behind the "no English-only screen" line of Appendix Q script Q.10.

#### 1.5 Arabic reviewer ownership and the glossary

| Step | Who | What |
|---|---|---|
| 1 | The engineer adding the string | Writes the key, the English value, the context note and a first Arabic value marked `draft`; never merges an empty Arabic value |
| 2 | The Arabic language owner, a named native speaker assigned by the product owner (open point 3) | Reviews every `draft` value in the feature's pull request or in the weekly batch, corrects it against the glossary and the screen, and sets `reviewed` |
| 3 | The product owner | Settles a disputed term; the decision becomes a glossary entry |
| 4 | The release manager | A release branch cannot be cut while any Arabic value is `draft` (§1.4) |
| 5 | The customer success lead | Collects wording reports from schools; each becomes a glossary change or a string fix within a release |

The terminology glossary is `i18n/glossary/glossary.json` in the repository, semantically versioned, with a changelog. A major version bump means a term changed meaning or a default rendering changed; every string carrying that term is re-reviewed.

| Glossary field | Meaning | Example |
|---|---|---|
| `id` | Stable identifier | `term.guardian` |
| `en` | Default English, singular and plural | `Guardian`, `Guardians` |
| `ar` | Default Arabic, singular, dual, plural | `ولي الأمر`, `وليا الأمر`, `أولياء الأمور` |
| `arGender` | Grammatical gender of the Arabic noun, for agreement of numbers and adjectives | `masculine` |
| `definition` | One sentence in each language | "The adult with legal responsibility for a student" |
| `forbidden` | Variants the report refuses | `["والد الطالب"]` when the school means a guardian of either sex |
| `overridable` | Whether a tenant may override it (§1.6) | `true` |
| `since` | Glossary version that introduced or last changed it | `1.4.0` |

Seed entries include student (طالب), guardian (ولي الأمر), section (شعبة), grade level (الصف), term (الفصل الدراسي), timetable (الجدول الدراسي), marks (الدرجات), attendance (الحضور والغياب), excuse (عذر), request (طلب), approve (موافقة), statement of account (كشف حساب), class list (كشف الفصل), transfer certificate (شهادة نقل), service certificate (شهادة خبرة); the last seven match the Arabic used in Appendix Q.

#### 1.6 Per-tenant terminology overrides

Master brief Section 17: "Grade" or "Year", "Term" or "Semester", "Section" or "Class", "Guardian" or "Parent", applied everywhere including documents. Platform owns the overrides (Appendix L.5, ADR-0009) under `platform.terminology.view` and `platform.terminology.edit`.

| Aspect | Rule |
|---|---|
| What can be overridden | Only glossary terms with `overridable: true`: grade level, term, section, guardian, student, homeroom, campus, subject, report card, fee |
| What an override holds | English singular and plural; Arabic singular, dual, plural and gender. The console refuses an Arabic override without a gender, because number agreement depends on it |
| How strings use terms | Strings never contain the literal term; they reference it with a placeholder `{term.section}` or, where the count matters, `{term.section, count}` resolved to the right form. The report fails a string that contains the default term literally |
| Where it is applied | At render time, on every surface: the Transloco transpiler and the Flutter message lookup substitute from the tenant's terminology snapshot; Documents and Notification substitute from the same snapshot when they render. Stored data never contains a resolved term |
| Snapshot and refresh | Loaded with the session bootstrap from Bff.Web and Bff.Mobile; cached per tenant under the caching map in `21-performance-engineering.md`; invalidated by `platform.terminology.changed.v1`, which re-renders open screens without a reload (`08-web-structure.md` reference store) |
| Documents already issued | A generated PDF keeps the term that was in force when it was generated; a regenerated one uses the current term. A certificate is a record of what was issued |
| Public API and iCal | Labels in feeds follow the override; field names and enum values on the wire never change |
| Test | `TC-L10N-110`: override "Section" to "Class" and "Guardian" to "Parent", then snapshot every screen family, a report card, a receipt and a push notification in both languages |

---

### 2. The bilingual data model

Master brief Section 17: bilingual data, not only a bilingual interface. Three kinds of text exist, and each has one representation.

| Kind | Representation | Examples | Rule |
|---|---|---|---|
| Reference text, expected in both languages | `LocalizedText` | Subject names, section names, fee item names, request type names | Both values collected in the same form, side by side |
| Person names | `PersonName`, parts per language | Students, guardians, staff | BR-L10N-007: display in the active language, fall back to the other and record the gap |
| Authored content, written in one language | Text plus a `language` tag (`ar` or `en`) | Messages, announcements, assignment instructions, excuse reasons, incident descriptions | Stored as written; machine translation is shown beside the original, never saved over it (`06-services/communication.md`) |

#### 2.1 `LocalizedText`

| Aspect | Rule |
|---|---|
| Definition | `public sealed record LocalizedText(string? Ar, string? En)` in `Nibras.BuildingBlocks.Localization`; the wire shape is `{ "ar": …, "en": … }` (`22-api-conventions-and-error-catalog.md` §1.3) and the proto message of the same name (§10.1 there) |
| Invariants | At least one value non-empty when the field is required; each value trimmed, Unicode NFC, within the field's maximum length per language; no line breaks in a name field; bidi control characters stripped from names |
| Resolution | `Resolve(language)` returns the active language's value, or the other with `IsFallback = true`; the UI marks a fallback subtly for administrators and not at all for guardians |
| Storage | Two columns, `<field>_ar` and `<field>_en`, mapped as an EF Core complex type, never a `jsonb` document; each column carries its language's collation (`ar-x-icu`, `en-x-icu`) so `ORDER BY` is correct, and the Arabic column feeds the folded search column of §3 |
| Why not `jsonb` | A collation and a generated search column cannot attach to a key inside a document, and the query budget of a class list sort does not allow an expression index per language |
| Data quality | Reporting counts fallbacks per field and per campus for the data-quality report (BR-L10N-007) |

`22-api-conventions-and-error-catalog.md` and the proto use `LocalizedText`. `07-solution-structure.md` names the same value object `BilingualText`; open point 1 aligns it.

#### 2.2 `PersonName`

| Part | Arabic | English | Notes |
|---|---|---|---|
| First name | required | optional | |
| Father's name | optional, collected by default in the three target countries | optional | |
| Grandfather's name | optional | optional | |
| Family name | required | optional | |
| Display order | first, father, grandfather, family | first, family by default; father's name shown when the school enables it | Master brief Section 17, names in local order with as many parts as the culture uses |
| Short form | first and family | first and family | Used in lists, notifications and badges |

Automatic transliteration is never written into a person's record (BR-L10N-007); the transliteration key of §3.5 is a derived search column, not a name.

#### 2.3 Which fields are bilingual

The entity list is Appendix F; the fields below are the bilingual ones. Services not detailed in Appendix F follow the same three kinds when their sheets are written.

| Service | Entity | `LocalizedText` fields | `PersonName` | Authored with a language tag |
|---|---|---|---|---|
| Platform | Tenant, Branding | school display name, legal name, address, receipt footer, letterhead lines | | |
| Platform | CustomFieldDefinition | label, help text, each option label | | |
| Identity | Role | name, description (custom roles; system roles come from the catalog) | | |
| Identity | User | | display name, linked to the person record where one exists | |
| School | Campus, Building, Room | name, address (campus) | | |
| School | AcademicYear, Term, GradingPeriod | name | | |
| School | Stage, GradeLevel, Section, Department, House | name, short name | | |
| School | Subject | name, short name, description | | |
| School | Student | address | name parts | |
| School | Guardian | address, occupation | name parts | |
| School | StaffMember | job title, address | name parts | |
| Admissions | Application | previous school name | applicant name parts | notes |
| Academics | CurriculumUnit, Outcome | title, description | | |
| Academics | Rubric | criterion titles and level descriptors | | |
| Academics | Assignment, Resource, Question | | | instructions, body (QTI keeps `xml:lang`) |
| Assessment | Component, GradeScheme | name, grade descriptors | | |
| Assessment | ReportCard | comment per subject and the homeroom comment, both languages where the school collects both (master brief Section 17) | | |
| Scheduling | BellSchedule, Period, CalendarEvent | name, title | | |
| Attendance | Excuse | | | reason |
| Finance | FeeItem, Discount, Scholarship | name, description printed on invoices | | |
| Communication | Announcement | | | title and body, with an optional second-language version |
| Communication | Message | | | body |
| Notification | Template | not `LocalizedText`: one template per language and channel, because each language has its own plural forms | | |
| Requests | RequestType, FormDefinition | name, description, every field label and option | | the requester's free-text answers |
| Documents | DocumentTemplate, Certificate | template name, certificate title | | |
| Behavior | Category, Badge | name, criteria | | incident description |

#### 2.4 Which language prints on which document

| Document | Language rule | Dates | Numerals |
|---|---|---|---|
| Report card | Both, side by side: Arabic on the right half, English on the left; a tenant may choose single-language Arabic or English per grade level | Both calendars when Hijri display is on | Arabic side per tenant setting; English side Western |
| Transcript | Both, side by side | Both calendars | As above |
| Transfer certificate, enrolment certificate | Both, side by side; names never transliterated automatically (BR-L10N-007) | Both calendars always (BR-L10N-003) | As above |
| Service certificate (Hr) | Both | Both calendars always (`TC-L10N-801`) | As above |
| Receipt | The tenant default language for the body; amount in words in both languages always (BR-L10N-004) | Tenant setting | Tenant setting; the receipt number is an identifier and stays Western |
| Invoice and e-invoice | The tenant default language, plus Arabic where the country's e-invoicing plug-in requires it | Tenant setting | As receipt |
| Statement of account | The payer's preferred language (`TC-L10N-401`) | Payer's preference | Payer's preference |
| Letters and notices to a guardian | The guardian's preferred language | Guardian's preference | Guardian's preference |
| Class list, register, timetable print | The printing user's language (`TC-L10N-202`, `TC-L10N-601`) | User's preference | User's preference |
| Student and staff ID card | Both | Gregorian | Western |
| Ministry export | As the `IMinistryExport` plug-in declares (`23-integrations-and-public-api.md` §8) | As declared | As declared |
| CSV and JSON exports | Column headers in the requesting user's language; values raw | ISO 8601 | Western always (BR-L10N-002) |

---

### 3. Arabic search normalization

BR-L10N-001: normalize identically before indexing and before matching, in the database, and never modify the stored display value. The same function serves search, the `q` parameter and `filter[...][contains]` (`22-api-conventions-and-error-catalog.md` §3.1 and §3.4), and duplicate detection in BR-ADM-006.

#### 3.1 The folding steps

Applied in this order by `nibras_ar_fold`, and mirrored exactly by `ArabicNormalizer.Normalize()` in `Nibras.BuildingBlocks.Localization` for the rare case where application code must compare (the mobile offline search and the import preview).

| Step | Characters | Result | Why |
|---|---|---|---|
| 1 | Tatweel U+0640 (ـ) | removed | Decoration, never meaning |
| 2 | Harakat U+064B to U+065F, superscript alef U+0670, Quranic marks U+06D6 to U+06ED | removed | Rarely typed; "مُحمَّد" matches "محمد" |
| 3 | Alef with hamza above U+0623 (أ), below U+0625 (إ), madda U+0622 (آ), wasla U+0671 (ٱ) | alef U+0627 (ا) | Typed interchangeably |
| 4 | Waw with hamza U+0624 (ؤ) | waw U+0648 (و) | Typing varies |
| 5 | Ya with hamza U+0626 (ئ) | ya U+064A (ي) | Typing varies |
| 6 | Alef maqsura U+0649 (ى), Farsi ya U+06CC (ی) | ya U+064A (ي) | Regional habit and keyboard layouts |
| 7 | Ta marbuta U+0629 (ة) | ha U+0647 (ه) | "فاطمة" matches "فاطمه" |
| 8 | Keheh U+06A9 (ک) | kaf U+0643 (ك) | Keyboard layouts |
| 9 | Arabic-Indic digits U+0660 to U+0669, extended U+06F0 to U+06F9 | `0` to `9` | One digit system in the index |
| 10 | Latin letters | ASCII lower case; accented Latin letters kept as typed | Bilingual fields share one function |
| 11 | Runs of whitespace | one space, trimmed | Name parts are concatenated with spaces |
| Never | Standalone hamza U+0621 (ء), any base letter | kept | Normalization folds letters and never deletes them: "ابرهيم" does not match "إبراهيم" |

Presentation forms (U+FB50 to U+FDFF and U+FE70 to U+FEFF) never reach the database: name inputs apply Unicode NFKC on the server before validation, so a pasted ligature becomes its base letters.

#### 3.2 Database objects

```sql
-- Folds Arabic and Latin text for matching only; the display columns are never modified (BR-L10N-001)
CREATE OR REPLACE FUNCTION nibras_ar_fold(input text)
RETURNS text
LANGUAGE sql IMMUTABLE STRICT PARALLEL SAFE
AS $$
  SELECT btrim(
    regexp_replace(
      lower(
        translate(
          regexp_replace(input, '[ـً-ٰٟۖ-ۭ]', '', 'g'),
          'أإآٱؤئىةیک٠١٢٣٤٥٦٧٨٩۰۱۲۳۴۵۶۷۸۹',
          'ااااوييهيك01234567890123456789'
        ) COLLATE "C"
      ),
      '\s+', ' ', 'g'
    )
  );
$$;

-- Trigram operators and GIN operator classes, installed once per service database by the baseline migration
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Folded full Arabic name, recomputed by PostgreSQL on every insert and update so the index can never lag
ALTER TABLE students
  ADD COLUMN name_ar_folded text
  GENERATED ALWAYS AS (
    nibras_ar_fold(
      coalesce(first_name_ar, '') || ' ' || coalesce(father_name_ar, '') || ' ' ||
      coalesce(grandfather_name_ar, '') || ' ' || coalesce(family_name_ar, '')
    )
  ) STORED;

-- Folded full English name, same function so a Latin query behaves identically
ALTER TABLE students
  ADD COLUMN name_en_folded text
  GENERATED ALWAYS AS (
    nibras_ar_fold(coalesce(first_name_en, '') || ' ' || coalesce(family_name_en, ''))
  ) STORED;

-- Trigram index for contains and similarity matching on the Arabic name, live rows only
CREATE INDEX ix_students_name_ar_folded_trgm
  ON students USING gin (name_ar_folded gin_trgm_ops)
  WHERE deleted_at IS NULL;

-- Trigram index for the English name
CREATE INDEX ix_students_name_en_folded_trgm
  ON students USING gin (name_en_folded gin_trgm_ops)
  WHERE deleted_at IS NULL;

-- B-tree prefix index for autocomplete, led by the tenant so a prefix scan stays inside one school
CREATE INDEX ix_students_tenant_name_ar_folded_prefix
  ON students (tenant_id, name_ar_folded text_pattern_ops)
  WHERE deleted_at IS NULL;
```

The trigram indexes carry no tenant column: the planner combines them with the tenant index by a bitmap AND, and row-level security plus the tenant filter still apply. The same three columns and indexes exist on `guardians` and `staff_members` in School, on `applications` in Admissions, and on the slim student copies that other services search.

Queries, always through the function, never with a raw `LIKE` on a display column:

```sql
-- Autocomplete: prefix match on the folded name inside one tenant, served by the prefix index
SELECT id, first_name_ar, family_name_ar, first_name_en, family_name_en
FROM students
WHERE tenant_id = $1
  AND deleted_at IS NULL
  AND name_ar_folded LIKE nibras_ar_fold($2) || '%'
ORDER BY name_ar_folded, id
LIMIT 20;

-- Contains: the filter[name][contains] and q behaviour of document 22, served by the trigram index
SELECT id, first_name_ar, family_name_ar
FROM students
WHERE tenant_id = $1
  AND deleted_at IS NULL
  AND name_ar_folded LIKE '%' || nibras_ar_fold($2) || '%'
ORDER BY name_ar_folded, id
LIMIT 50;

-- Raises the similarity threshold for this transaction only, so near-misses stay out of a class list
SET LOCAL pg_trgm.similarity_threshold = 0.45;

-- Similarity: tolerant match for duplicate detection and admissions search, ranked by closeness
SELECT id, first_name_ar, family_name_ar, similarity(name_ar_folded, nibras_ar_fold($2)) AS score
FROM students
WHERE tenant_id = $1
  AND deleted_at IS NULL
  AND name_ar_folded % nibras_ar_fold($2)
ORDER BY score DESC, id
LIMIT 20;
```

#### 3.3 Conditions that make the objects correct

| Condition | Why | Check |
|---|---|---|
| The database classifies Arabic letters as alphanumeric (a UTF-8 `LC_CTYPE`, such as `C.UTF-8` on the Debian image) | `pg_trgm` builds trigrams only from characters it considers alphanumeric; under a byte-only classification an Arabic name yields no trigrams and every similarity search silently returns nothing | `TC-L10N-310`: `show_trgm(nibras_ar_fold('محمد'))` returns Arabic trigrams inside the built database image; database creation settings live in `10-data-architecture.md` |
| `nibras_ar_fold` is `IMMUTABLE` and uses only immutable functions | A generated column and an index expression require it; `lower` runs under the `"C"` collation so the result never depends on the database locale | Migration test creates the column on an empty database |
| One function for indexing and querying | Two implementations drift and the drift is invisible (BR-L10N-001 edge case) | `TC-L10N-311`: the C# `ArabicNormalizer` and the SQL function produce identical output over a 5,000-name corpus |
| The Arabic collations `ar-x-icu` and `en-x-icu` exist and are versioned | Display sort (`22-api-conventions-and-error-catalog.md` §3.3) | The culture test inside the image checks the collation version against the release manifest (§10) |

#### 3.4 Worked matches

| Stored | Typed | Folded stored | Folded typed | Match |
|---|---|---|---|---|
| احمد | أحمد | احمد | احمد | yes |
| فاطمة | فاطمه | فاطمه | فاطمه | yes |
| عــلي | علي | علي | علي | yes |
| مُحَمَّد | محمد | محمد | محمد | yes |
| مصطفى | مصطفي | مصطفي | مصطفي | yes |
| لؤي | لوي | لوي | لوي | yes |
| إبراهيم | ابرهيم | ابراهيم | ابرهيم | no by equality; by similarity only on the duplicate-detection path |
| Mohammed Alzahrani | mohammed alzahrani | mohammed alzahrani | mohammed alzahrani | yes |

#### 3.5 Transliteration tolerance

Master brief Section 17 asks for tolerant matching of transliterated names: a registrar types "Mohammed" and the record holds only "محمد". A second derived column, `name_translit_key`, holds a consonant skeleton computed from either script by `nibras_translit_key(text)`, an immutable SQL function built on the same pattern as the fold.

| Step | Arabic side | Latin side |
|---|---|---|
| 1 | Apply `nibras_ar_fold` | Lower case |
| 2 | Drop the article ال at the start of each word | Drop `al-`, `al `, `el-`, `el ` at the start of each word, and a fused leading `al` or `el` when the word has at least 7 letters (`Alzahrani`, `Alqahtani`) |
| 3 | Map letters: ب→b, ت ط→t, ث→t, ج→j, ح ه→h, خ→k, د ض ذ→d, ر→r, ز ظ→z, س ص ش→s, غ→j, ف→f, ق ك→k, ل→l, م→m, ن→n, و→w | Map digraphs first: `kh`→k, `th`→t, `dh`→d, `sh`→s, `gh`→j, `ph`→f; then letters: `g`→j, `q` `c`→k, `p`→b, `v`→w, `x`→ks |
| 4 | Drop ا, ي, ع, ء and every remaining letter without a mapping | Drop `a e i o u y` and apostrophes |
| 5 | Collapse doubled letters, remove spaces | Collapse doubled letters, remove spaces |

| Arabic | Latin | Key from Arabic | Key from Latin | Result |
|---|---|---|---|---|
| محمد | Mohammed | mhmd | mhmd | match |
| خالد | Khalid | kld | kld | match |
| عثمان | Othman | tmn | tmn | match |
| ياسر | Yasser | sr | sr | match |
| الزهراني | Al-Zahrani | zhrn | zhrn | match |
| الزهراني | Alzahrani | zhrn | zhrn | match, through the fused-article rule |
| القحطاني | Alqahtani | khtn | khtn | match |
| علوي | Alawi | lw | lw | match; five letters, so the fused rule leaves the Latin `al` alone, and the Arabic has no article |

| Rule | Detail |
|---|---|
| Where it is used | Duplicate detection (BR-ADM-006) and the admissions and directory search when the query script differs from the stored script; the general `q` uses folding and trigrams only (`22-api-conventions-and-error-catalog.md` open point 2) |
| Index | A GIN trigram index on `name_translit_key`, partial on live rows, like §3.2 |
| Never shown | The key is not a name, never displayed, never exported, and never written into a name field (BR-L10N-007) |
| Limits | A skeleton cannot distinguish names that differ only in vowels ("Hamid" and "Hamed" are one key). BR-L10N-001 owns the long-vowel case and states it as a rule, not a defect: the key cannot see و or ي written as a long vowel, so "يوسف" gives `wsf` and "Yousef" gives `sf`, the pair is found through the English name part when the school filled it, and a person confirms every duplicate under BR-ADM-006. Results are ranked by similarity on the display names too |

---

### 4. Numerals

BR-L10N-002: numbers are stored and transmitted as Western Arabic digits; the tenant setting chooses the display system and a user may override it; parsing accepts both systems; identifiers are never converted.

#### 4.1 The setting

| Aspect | Rule |
|---|---|
| Setting | Appendix G General → numerals: `western` or `arabicIndic`, tenant scope, default `western`; user scope override in the profile |
| Applies to | The Arabic interface only; the English interface always renders Western digits |
| Separators | With `arabicIndic`, the decimal separator renders as ٫ (U+066B) and the thousands separator as ٬ (U+066C); with `western` in Arabic, `.` and `,` |
| Implementation | One `NumeralFormatter` per runtime (`Nibras.BuildingBlocks.Localization` in .NET, `@nibras/i18n` in Angular, `nibras_ui` in Flutter): format with the invariant pattern for the value's type, then map digits and separators by table. No runtime relies on a locale's default numbering system, because browsers, ICU versions and the Dart `intl` data disagree on it |
| Direction | A number is always a left-to-right run inside right-to-left text; the formatter wraps it in a left-to-right isolate when it is interpolated into a sentence (§9.6) |

#### 4.2 Where each system renders

| Surface | `arabicIndic` setting, Arabic interface | Always Western |
|---|---|---|
| Screens (web, mobile, console) | Counts, marks, percentages, amounts, dates, times, durations, page numbers, chart axes and data labels | |
| Printed documents | The Arabic side of a bilingual document; single-language Arabic documents | The English side of a bilingual document |
| Push, in-app and e-mail notifications | Counts, amounts, dates in the body | |
| SMS | Counts and amounts in the body | One-time codes, links |
| Input fields | Echoed as typed | Stored and posted Western |
| | | Every identifier: student number, invoice and receipt numbers (`INV-2026-0418`), national identifiers, phone numbers in E.164, IBAN, card last four, verification codes, request numbers, certificate serials |
| | | Every machine surface: API payloads, CSV and JSON exports, OneRoster, iCal, webhook bodies, e-invoice XML, QR payloads, barcodes, logs, database values, URLs, file names |

#### 4.3 Parsing

| Input | Result |
|---|---|
| `1234` or `١٢٣٤` or `۱۲۳۴` | 1234 |
| `87.5`, `87٫5` | 87.5 |
| `1,234.50`, `١٬٢٣٤٫٥٠` | 1234.50 |
| `12٣4` (mixed systems) | Refused with the validation code `mixedNumerals` (`22-api-conventions-and-error-catalog.md` §8), never parsed on a best-effort basis |
| `1.234,50` | Refused as `invalidNumber`; the comma is never a decimal separator on input, whatever the browser locale |

Parsing happens in the client before the value reaches the payload, and again on the server through the invariant culture, so a payload never carries a non-Western digit.

---

### 5. Hijri display with Gregorian as the source of truth

BR-L10N-003: every date is stored, compared, sorted and computed in the proleptic Gregorian calendar; Hijri is a display conversion with the Umm al-Qura calendar; a Hijri date a user enters is converted to Gregorian on save.

#### 5.1 The rules

| Aspect | Rule |
|---|---|
| Conversion | `System.Globalization.UmAlQuraCalendar` on the server. The web and mobile clients do not use their platform's Islamic calendar: they use a month-start table for 1318 to 1500 AH generated at build time from `UmAlQuraCalendar`, published as `@nibras/i18n/umalqura.json` and its Dart twin, so server, browser and phone agree to the day |
| Supported range | 1900-04-30 (1 Muharram 1318) to 2077-11-16 (30 Dhu al-Hijjah 1500), the range of `UmAlQuraCalendar`; a date outside it renders in Gregorian with a note and never throws (BR-L10N-003 edge case) |
| Setting | Appendix G General → calendars: `gregorian`, `gregorianWithHijri` (Gregorian first, Hijri beneath or after) or `hijriWithGregorian`; tenant default, user override |
| What converts | A `DateOnly` in the campus calendar. An instant (`timestamptz`) is first converted to the campus-local date with the campus time zone (§6), then to Hijri; converting a UTC instant directly shows the wrong Hijri day for an event after 21:00 UTC in Riyadh |
| Entry | A Hijri date picker writes a Gregorian `DateOnly` into the model at selection time; the form posts Gregorian only |
| Computation | None in Hijri, ever: age (BR-ADM-001), cut-off dates, due dates, report periods, attendance thresholds and retention all run on Gregorian dates |
| Grouping | A report grouped by month uses Gregorian months, labelled in both calendars (BR-L10N-003); a user asking for "Ramadan 1448" gets the Gregorian range converted at request time, stated on the report |
| Month names | From the `dates` namespace of the glossary in both languages, not from the runtime's culture data, so an ICU update cannot rename a month |
| Printing | Official documents (certificates, transcripts, transfer and service certificates) print both calendars always; other documents follow the setting |
| Format | Arabic: `٨ ربيع الآخر ١٤٤٨ هـ` under `arabicIndic`, `8 ربيع الآخر 1448 هـ` under `western`; English: `8 Rabi al-Akhir 1448 AH`; Gregorian in the same line or beneath, per the setting |

#### 5.2 Known pairs, asserted by the culture test

These pairs come from `UmAlQuraCalendar` and are asserted inside the built image (`33-platform-support-and-dev-environments.md` check G3) and against the generated client table.

| Gregorian | Umm al-Qura | Why it is in the test |
|---|---|---|
| 1900-04-30 | 1 Muharram 1318 | Lower bound of the supported range |
| 2026-02-18 | 1 Ramadan 1447 | A Ramadan start, which drives the Ramadan bell schedule prompt (§6.4) |
| 2026-03-20 | 1 Shawwal 1447 | A month boundary after a 30-day or 29-day month |
| 2026-06-16 | 1 Muharram 1448 | A Hijri year boundary inside a Gregorian school year |
| 2026-09-19 | 8 Rabi al-Akhir 1448 | The date used in the BR-L10N-003 examples |
| 2027-02-08 | 1 Ramadan 1448 | The next Ramadan, eleven days earlier in the Gregorian year |
| 2077-11-16 | 30 Dhu al-Hijjah 1500 | Upper bound of the supported range |

#### 5.3 Month-boundary cases

| Case | Behaviour |
|---|---|
| Umm al-Qura and moon sighting disagree by a day | The displayed Hijri date is Umm al-Qura, the civil calendar of Saudi Arabia; religious holidays that depend on sighting are entered by the school as Gregorian dates (§6.3), never computed from the Hijri date |
| A user enters the 30th of a 29-day month (Rabi al-Awwal 1448 has 29 days) | The picker does not offer it; a typed value is refused with the validation code `dateInvalid` |
| A Hijri date of birth outside the range | Refused with `dateOutOfRange`; the form offers Gregorian entry |
| A date range spanning a Hijri month boundary | Stored and filtered by Gregorian range; the label shows both calendars for both ends |
| The year rolls over mid-term | Academic years are Gregorian; a Hijri year boundary (for example 1 Muharram 1448 on 2026-06-16) has no effect on any computation |
| An authority adjusts a future Umm al-Qura month | Picked up only by a deliberate update of the .NET runtime and a regenerated client table in a release; the culture test pairs change in the same pull request with a note |

#### 5.4 The ICU pin

| Aspect | Rule |
|---|---|
| What ICU affects here | Collation (`ar-x-icu` in PostgreSQL and culture-aware sorting in .NET), culture data for `ar-SA` formatting patterns, and time-zone names; the Umm al-Qura conversion itself comes from the managed `UmAlQuraCalendar` table |
| Pin | The ICU package version inside every service image and the PostgreSQL image is pinned per release and recorded in the release manifest (`33-platform-support-and-dev-environments.md` check G6); a bump is its own pull request with the collation-version check and the culture test |
| Collation version drift | After an ICU bump, PostgreSQL reports a collation version mismatch; the release runbook reindexes the affected indexes before the new image takes traffic |
| Never | `InvariantGlobalization=true`, an Alpine image without `icu-libs` and `icu-data-full`, or a client that formats Hijri with its own platform calendar |

---

### 6. Calendars and time

Master brief Section 17 and Section 19: time in UTC for storage, the school time zone for display, a configurable work week, holidays per campus, Ramadan bell schedules.

#### 6.1 Types and storage

| Value | .NET type | PostgreSQL type | Wire (`22-api-conventions-and-error-catalog.md` §1.4) | Zone |
|---|---|---|---|---|
| An instant (marked at, paid at, sent at) | `DateTimeOffset` from the injected `TimeProvider` | `timestamptz` | ISO 8601 with `Z` | UTC |
| A school day (attendance date, due date, date of birth) | `DateOnly` | `date` | `YYYY-MM-DD` | none; interpreted in the campus zone |
| A wall-clock time (bell, cut-off) | `TimeOnly` | `time` | `HH:mm:ss` | the campus zone |
| A campus time zone | `string` IANA id | `text` | IANA id | |

`DateTime.Now`, `DateTime.UtcNow` and `DateTime.Today` are banned by analyzer in every project; tests control time through `FakeTimeProvider`.

#### 6.2 Time zones

| Aspect | Rule |
|---|---|
| Tenant zone | Appendix G General → time zone, IANA id, default from the country at provisioning |
| Campus zone | Each campus stores its own zone, defaulting to the tenant zone; a group with a campus in Dubai and one in Riyadh has two |
| Resolution | `TimeZoneInfo.FindSystemTimeZoneById` with the IANA id on every operating system; Windows ids are never stored |
| Target countries | `Asia/Riyadh` UTC+03:00, `Asia/Amman` UTC+03:00, `Asia/Dubai` UTC+04:00 |
| Daylight saving | None of the three observes it today; Jordan adopted permanent UTC+03:00 in 2022. The code still converts through the zone rules and never through a fixed offset, because a future change arrives as a tzdata update, not a code change |
| Pinned tzdata | The tzdata package in every image is pinned per release (check G6); the culture test asserts the three offsets on a January and a July date (check G4) |
| Jobs | Every scheduled job that means "at 07:30 at school" runs per campus in the campus zone (the unmarked-class reminder, the pre-peak warm-up, the attendance cut-off lock) |
| Day boundary | "Today" for a campus is the campus-local date of the injected clock; an attendance mark at 23:30 UTC is already tomorrow in Riyadh and in Dubai |

#### 6.3 Work week and holidays

| Aspect | Rule |
|---|---|
| Work week | Appendix G General → work week, a set of weekdays with optional half days, tenant default and campus override. Seeds: Sunday to Thursday for Saudi Arabia and Jordan; Monday to Friday with a Friday half day for the United Arab Emirates; confirmed by the school during onboarding |
| Holidays | `CalendarEvent` rows of type holiday in Scheduling, per campus or tenant-wide, as Gregorian date ranges; Attendance opens no session on a holiday, Finance skips holidays when a due date rule says "working day" |
| National holidays | Fixed-date national days seeded per country as suggestions (Saudi Founding Day 22 February, Saudi National Day 23 September, Jordan Independence Day 25 May, UAE National Day 2 December); the school confirms them each year |
| Religious holidays | Eid al-Fitr and Eid al-Adha depend on sighting, so they are entered by the school, with a suggestion computed from Umm al-Qura and marked as a suggestion |
| Change mid-year | A holiday added after timetables and fee schedules exist raises `scheduling.event.published.v1`; consumers recompute only future dates |

#### 6.4 Ramadan bell schedules

| Aspect | Rule |
|---|---|
| Model | A `BellSchedule` has a validity date range and a priority; a Ramadan schedule is an ordinary bell schedule with shorter periods and a later start, valid for the Gregorian dates the school enters |
| Prompt | Thirty days before the Umm al-Qura start of Ramadan (for example 2027-01-09 for Ramadan 1448), Scheduling raises a task to the timetable owner to confirm the dates and the schedule; the dates are the school's, because the start is announced by sighting |
| Effect | Attendance cut-offs, period reminders and the timetable view switch on the first date and back after the last, per campus; no timetable re-solve is needed because periods map one to one |
| Test | `TC-L10N-620`: a Ramadan schedule valid 2027-02-08 to 2027-03-09 changes the cut-off on 2027-02-08 in `Asia/Riyadh` and in `Asia/Dubai` at the right local time |

---

### 7. Money

Master brief Section 17: multi-currency, configurable decimals and rounding, amounts in words in both languages on receipts. Section 19: money is a decimal with a currency. The wire shape is a decimal string plus an ISO 4217 code (`22-api-conventions-and-error-catalog.md` §1.4, BR-FIN-011).

#### 7.1 Currencies of the target countries

| Code | Minor unit digits | English major, minor | Arabic major (gender), minor (gender) | Arabic short symbol |
|---|---|---|---|---|
| SAR | 2 | Saudi riyal, halala | ريال سعودي (masculine), هللة (feminine) | ر.س |
| AED | 2 | UAE dirham, fils | درهم إماراتي (masculine), فلس (masculine) | د.إ |
| JOD | 3 | Jordanian dinar, fils | دينار أردني (masculine), فلس (masculine) | د.أ |

#### 7.2 Rounding

| Rule | Detail |
|---|---|
| Scale | The ISO 4217 minor unit of the currency, which is what Appendix G Finance → "rounding mode and decimals per currency" now sets: 2 for SAR and AED, 3 for JOD. The §7.1 table restates those three values; Appendix G and the Appendix S BR-FIN money-scale rule own them, and a currency added later takes its own minor unit, never a flat 2 |
| Mode | Half-up by default (Appendix G), applied as half away from zero so a refund rounds symmetrically: SAR 10.005 becomes 10.01 and SAR −10.005 becomes −10.01; JOD 12.3455 becomes 12.346 |
| Implementation | `decimal.Round(value, scale, MidpointRounding.AwayFromZero)` named explicitly; the .NET default is banker's rounding, which would turn SAR 10.005 into 10.00, so an analyzer forbids `Math.Round` and `decimal.Round` without a `MidpointRounding` argument |
| Where rounding happens | Where Finance's rules say (per line, per installment, allocation remainders); this document fixes only how a value is rounded, never when |
| Binary floating point | Never, at any layer (BR-FIN-011) |

#### 7.3 Display

| Language | Numeral setting | SAR 1234.5 | JOD 1250.75 |
|---|---|---|---|
| English | any | SAR 1,234.50 | JOD 1,250.750 |
| Arabic | `western` | 1,234.50 ر.س | 1,250.750 د.أ |
| Arabic | `arabicIndic` | ١٬٢٣٤٫٥٠ ر.س | ١٬٢٥٠٫٧٥٠ د.أ |

The amount is always shown at the currency scale, the symbol follows the number in Arabic and the code precedes it in English, and the amount and symbol sit in one left-to-right isolate so the symbol never jumps to the far side of a right-to-left line (`TC-L10N-401`).

#### 7.4 Amounts in words

BR-L10N-004: generated from the stored decimal, never from the formatted string; the minor unit is always stated, even when zero; the phrase ends with "only". The Arabic generator is table-driven per currency because number words agree in gender and case with the currency noun.

Arabic noun form by count, applied to the last two digits of the major and of the minor amount separately:

| Count ending | Noun form | Number form | Example with ريال (masculine) | Example with هللة (feminine) |
|---|---|---|---|---|
| 0 | singular, after صفر | صفر | صفر ريال | صفر هللة |
| 1 | singular, then the number as an adjective | واحد or واحدة, agreeing | ريال واحد | هللة واحدة |
| 2 | dual | none | ريالان | هللتان |
| 3 to 10 | plural, genitive | opposite gender to the noun | ثلاثة ريالات | ثلاث هللات |
| 11 to 99 | singular, accusative | 11 and 12 agree with the noun; in 13 to 19 the unit takes the opposite gender and the ten agrees; tens do not change | أحد عشر ريالًا، خمسون ريالًا | إحدى عشرة هللةً، خمسون هللةً |
| 100, 1000 and their multiples with 00 at the end | singular, genitive | | مئة ريال، ألفا ريال | مئة هللة |

Worked examples, which are also the fixed vectors of `AmountInWordsRulesTests`:

| Amount | English | Arabic |
|---|---|---|
| SAR 1,234.50 | One thousand two hundred thirty-four Saudi riyals and fifty halalas only | فقط ألف ومئتان وأربعة وثلاثون ريالًا سعوديًا وخمسون هللةً لا غير |
| SAR 2,000.00 | Two thousand Saudi riyals and zero halalas only | فقط ألفا ريال سعودي وصفر هللة لا غير |
| SAR 1.00 | One Saudi riyal and zero halalas only | فقط ريال سعودي واحد وصفر هللة لا غير |
| SAR −150.00 (refund receipt) | Minus one hundred fifty Saudi riyals and zero halalas only | فقط سالب مئة وخمسون ريالًا سعوديًا وصفر هللة لا غير |
| JOD 1,250.750 | One thousand two hundred fifty Jordanian dinars and seven hundred fifty fils only | فقط ألف ومئتان وخمسون دينارًا أردنيًا وسبعمئة وخمسون فلسًا لا غير |
| JOD 45.005 | Forty-five Jordanian dinars and five fils only | فقط خمسة وأربعون دينارًا أردنيًا وخمسة فلوس لا غير |

| Rule | Detail |
|---|---|
| Source | The stored decimal at currency scale; the numeral setting never changes the words |
| Numerals inside the words | None; the phrase is words only, so it survives any numeral setting and any printer |
| Sign | An explicit word ("Minus", "سالب") rather than a minus character that can be lost in printing |
| Review | Every Arabic vector is approved by the Arabic language owner (§1.5) before the generator ships; a new currency adds a row to §7.1 and six vectors |
| Range | Up to 999,999,999,999 in the major unit; above that the receipt prints digits with a note, which no school fee reaches |

---

### 8. The six Arabic plural categories

BR-L10N-005: Arabic messages select among zero, one, two, few, many and other from the numeric value; English among one and other; a template that supplies fewer categories than its language requires cannot be published. The selection rules are the CLDR plural rules, taken from the CLDR release pinned with ICU (§5.4) and generated into one table shared by the three runtimes.

| Category | Rule on the integer n | Values | Example (students absent) |
|---|---|---|---|
| zero | n = 0 | 0 | لا يوجد طلاب غائبون |
| one | n = 1 | 1 | طالب واحد غائب |
| two | n = 2 | 2 | طالبان غائبان |
| few | n mod 100 is 3 to 10 | 3 to 10, 103 to 110, 203 to 210 | ٣ طلاب غائبون |
| many | n mod 100 is 11 to 99 | 11 to 99, 111 to 199 | ١١ طالبًا غائبًا |
| other | everything else, including fractions | 100, 101, 102, 200, 1000, 2.5 | ١٠٠ طالب غائب |

The same message as it is written in `ar.json`, an ARB file or a Notification template:

```text
{count, plural,
  zero {لا يوجد طلاب غائبون}
  one {طالب واحد غائب}
  two {طالبان غائبان}
  few {# طلاب غائبون}
  many {# طالبًا غائبًا}
  other {# طالب غائب}}
```

| Rule | Detail |
|---|---|
| English | `one` and `other` required; `zero` allowed and encouraged where "0 students" reads badly (BR-L10N-005 edge case) |
| Selection | Always from the number; never by concatenating a number and a noun (§1.2) |
| The `#` | Formatted by the `NumeralFormatter` (§4), so it honours the numeral setting |
| Gender | A message that addresses a person also selects on gender (`{addresseeGender, select, female {…} male {…} other {…}}`); `other` is a neutral wording, never the masculine by default |
| Channels | In-app text, push, e-mail subjects and bodies, SMS bodies and printed documents use the same rules; Notification validates each template once for every channel (BR-L10N-005 edge case) |
| Validation | The missing-translation report (§1.4) and Notification's template publish check both refuse a missing category and name it |
| Test | `ArabicPluralRulesTests` asserts 0, 1, 2, 3, 10, 11, 99, 100, 101, 102, 103 and 111 for Arabic, 1 and 100 for English, in all three runtimes from the shared table |

---

### 9. Right-to-left rules

Master brief Section 17: mirrored layouts, icons, charts, steppers and swipe directions; bidirectional text handled in inputs, tables, PDFs and notifications. The design-system side (tokens, components, the four-way snapshot job) is in `14-design-system-and-ux.md`; this section is the rule list every surface obeys.

#### 9.1 Layout

| Rule | Web | Mobile |
|---|---|---|
| Direction source | `dir` on `<html>` from the active language, set before first paint; the Angular CDK `Directionality` follows it | `Directionality` from the active locale at the app root |
| Spacing and position | Logical properties only (`margin-inline-start`, `inset-inline-end`, `padding-block`); a physical `left` or `right` in a stylesheet fails the Stylelint rule | `EdgeInsetsDirectional`, `AlignmentDirectional`, `PositionedDirectional`; the physical variants fail the lint rule in `nibras_lints` |
| Reading order | DOM order equals reading order in both directions; no CSS `order` or `row-reverse` to fake mirroring | Widget order unchanged; `Row` follows `Directionality` |
| Steppers, breadcrumbs, pagination | Flow from the start edge; "next" points toward the end edge | Same |
| Swipe | Swipe toward the start edge means back; swipe actions on list items mirror | Same, including the `Dismissible` directions |
| Tables | Columns flow from the start edge; numeric columns stay end-aligned in both directions; the frozen first column is on the start side | Same in the data grid |
| Keyboard | Arrow keys follow visual direction: in right-to-left, the left arrow moves to the next item in a horizontal list | Not applicable |
| Motion | Slide and view transitions travel toward the end edge for "forward"; the motion tokens take a direction sign | Same |

#### 9.2 Icons

| Mirror | Never mirror |
|---|---|
| Arrows and chevrons that mean back, forward, next, previous, expand sideways | Check marks, close, plus, minus |
| Send, reply, forward (message) | Play, pause, record and other media controls |
| Undo and redo | Clocks, clock-direction refresh icons, circular progress |
| Lists with bullets on one side, indent and outdent, text alignment | Logos, brand marks, flags |
| A person walking or a vehicle moving (direction of travel) | Search magnifier, question mark, a slash within a symbol |
| Sidebar and panel open or close | Charts inside icons that depict data |

Icons in `@nibras/ui` and `nibras_ui` declare `mirrorInRtl: true | false`; an icon without the flag fails the component build.

#### 9.3 Charts

| Element | Rule in right-to-left |
|---|---|
| Time axis | Runs from the right (earliest) to the left (latest) |
| Category axis | First category at the right |
| Value axis | On the right side |
| Bars | Horizontal bars grow from right to left |
| Legend and title | Aligned to the start edge (right) |
| Numbers | Tick labels and data labels are left-to-right runs with the numeral setting applied (§4) |
| Tooltips | Open toward the start side when space allows; content follows §9.6 |
| Not mirrored | Pie and donut order stays clockwise from twelve o'clock; a line's shape is never flipped vertically |

#### 9.4 Documents

| Rule | Detail |
|---|---|
| Renderer | HTML templates rendered to PDF by the Documents worker through Gotenberg; direction by `dir` and `lang` on each region, never by reordering text |
| Fonts | IBM Plex Sans Arabic with a Noto Naskh fallback for Arabic, Inter for Latin, present in the Documents image and the Gotenberg container (`33-platform-support-and-dev-environments.md` check G7); shaping by the browser engine, so ligatures and joining are correct |
| Bilingual layout | Side-by-side documents put Arabic on the right half and English on the left; stacked documents put the tenant's primary language first |
| Tables | Arabic tables flow right to left; the photo column of a class list is on the right (`TC-L10N-202`) |
| Numbers, dates, amounts | As §4, §5 and §7; identifiers left to right in an isolate |
| QR and verification | Codes and URLs left to right; the caption in the document language |
| Headers and page numbers | Page numbers in the document's numeral system at the start-side corner of the footer |

#### 9.5 Notifications

| Channel | Rule |
|---|---|
| Push | Title and body in the recipient's language; interpolated names, numbers and identifiers in first-strong isolates (§9.6); no Sensitive content on the lock screen (master brief Section 18) |
| SMS | The Arabic body starts with an Arabic word so every handset infers right to left; identifiers and one-time codes inside left-to-right isolates; the part count is computed on the UCS-2 length |
| E-mail | `dir` and `lang` on the `<html>` element and on each bilingual block; plain-text part with the same isolates |
| In-app inbox | Rendered by the client with the rules of §9.1 and §9.6 |
| Mixed-language content | A message written in English and received by an Arabic reader keeps its own direction inside the Arabic chrome; the translation is shown beside it with the original one tap away (`TC-L10N-501`) |

#### 9.6 Mixed-direction text

| Situation | Rule |
|---|---|
| A value interpolated into a sentence (a name, a number, a class code, a date) | Wrapped in first-strong isolate U+2068 and pop directional isolate U+2069 by the message formatter on every runtime, so "طالب {name} غائب" renders correctly for an English name and "{name} is absent" for an Arabic one |
| User-generated text displayed (messages, comments, names) | `dir="auto"` on the element, or `<bdi>`; Flutter uses `Bidi.detectRtlDirectionality` from the first strong character to set the text direction |
| Inputs for free text | `dir="auto"` so the caret and alignment follow what is typed; the comment box accepts mixed Arabic and English (`TC-L10N-201`) |
| Inputs for fixed-direction values | E-mail, URL, phone in E.164, IBAN, national identifier, codes: `dir="ltr"` always, aligned to the end edge in right-to-left layouts |
| Numbers inside Arabic text | A left-to-right run; never reversed; the minus sign and percent sign stay attached (`٨٧٫٥٪`) |
| Parentheses and punctuation at run ends | Isolates prevent a trailing parenthesis from jumping sides |
| Stored text | Bidi control characters are stripped from names and identifiers on input; formatting isolates are added at render and never stored |
| Tables and exports | CSV exports carry no control characters; a spreadsheet opened in Arabic Excel shows the values in their natural direction |

---

### 10. Culture pinning and the three-culture test

BR-L10N-006: every culture-sensitive operation names its culture; the invariant culture for parsing, formatting and comparing stored values; the request culture for display only; no result may depend on the host operating system, its ICU version or the machine's locale.

#### 10.1 The rule in code

| Where | Rule |
|---|---|
| Process start | Every host sets `CultureInfo.DefaultThreadCurrentCulture` and `DefaultThreadCurrentUICulture` to `CultureInfo.InvariantCulture` before anything formats or parses (`AddNibrasLocalization()`) |
| Display | Only through `ICultureContext`, resolved per request from the user's preference, then the tenant default; used by Documents, Notification and the few server-rendered texts, never for stored or transmitted values |
| Parse and format | Every `Parse`, `TryParse` and `ToString` on numbers and dates names a culture; the analyzer (check G10) fails the build otherwise |
| Comparison | Every `string.Equals`, `StartsWith`, `EndsWith`, `IndexOf` and `Compare` names a `StringComparison`; security decisions (permission names, tenant ids, routing keys, cache keys) are ordinal; case folding for identifiers uses `ToUpperInvariant` and `ToLowerInvariant` |
| Sorting for display | By the request culture's `CompareInfo` or by the database collation per language; the two are never assumed to agree (BR-L10N-006 edge case) |
| Time | `TimeProvider` injected; IANA zone ids; no local time of the host |
| Containers | Globalization on, ICU and tzdata present and pinned (§5.4, §6.2) |
| Clients | Angular and Flutter never read the device locale for machine values; generated API clients serialize with invariant formats |

#### 10.2 The three-culture test

| Aspect | Rule |
|---|---|
| Cultures | `ar-SA`, `en-US` and `de-DE`: Arabic for the product's own culture, English for the default developer machine, German because it swaps the decimal and thousands separators |
| Mechanism | `UseCultureAttribute` from `Nibras.BuildingBlocks.Testing` runs the decorated test once under each culture as the thread's current culture and restores it afterwards (`16-test-strategy.md`) |
| Required on | Every money, date, numeral, normalization and plural test, every BR-L10N rule test class, every property-based test over amounts, and every plug-in's conformance suite (`23-integrations-and-public-api.md` §8.3) |
| Pass condition | Byte-identical output across the three cultures for every stored or transmitted value; display output equal to the expected string for the culture under test |
| Runners | Linux and Windows in the pipeline matrix (TC-PLAT-007) |
| Inside the image | A separate test class runs in the built service image: `ar-SA` resolves and is not invariant, `UmAlQuraCalendar` returns the §5.2 pairs, the three time zones return the §6.2 offsets, and the ICU and tzdata versions equal the release manifest (checks G1 to G4 and G6) |

---

## Decisions in force

| Decision | Source | Default if unanswered | Impact if wrong |
|---|---|---|---|
| Bilingual reference text is `LocalizedText` stored as two columns with per-language collations | This document §2.1 | Two columns | A `jsonb` document cannot carry a collation or a generated search column, and every sort becomes an expression |
| Person names have parts per language and never an automatic transliteration | This document §2.2; BR-L10N-007 | As stated | A transliterated name written into a record becomes the official spelling on a certificate |
| One fold function in SQL, mirrored in C#, used by search, filters and duplicate detection | This document §3; BR-L10N-001 | As stated | Search and deduplication disagree and a second Ahmed is created on import |
| Trigram search requires a database character classification that treats Arabic as letters | This document §3.3 | `C.UTF-8` on the Debian image | Every similarity search on an Arabic name returns nothing, silently |
| Numerals default to `western`; identifiers and machine surfaces are always Western | This document §4; BR-L10N-002 | `western` | A school expecting Arabic-Indic digits changes one setting; the reverse default would break every copied number in a spreadsheet |
| Clients convert Hijri with a table generated from `UmAlQuraCalendar`, not their platform calendar | This document §5.1 | Generated table | A browser and a phone show different Hijri days for the same date |
| No computation in Hijri, ever | This document §5; BR-L10N-003 | As stated | A one-day disagreement between authorities changes an age check or a due date |
| Time zones by IANA id per campus; no fixed offsets | This document §6.2 | As stated | The next tzdata change in the region shifts every bell by an hour |
| Rounding is half away from zero at the currency's minor-unit scale, always named in code | This document §7.2; Appendix G | As stated | Banker's rounding silently changes receipts by one halala |
| The scale is the ISO 4217 minor unit per currency, not a flat 2 decimals: SAR and AED 2, JOD 3 | Appendix G Finance rounding row; the Appendix S BR-FIN money-scale rule | As stated | JOD amounts lose a fils on every invoice line |
| The cross-script consonant key cannot see و or ي as a long vowel; "يوسف" and "Yousef" are matched through the English name part, and a person confirms every duplicate | BR-L10N-001 rule and edge cases; BR-ADM-006 | As stated | Either registrars create duplicates unseen, or a looser key merges unrelated families |
| Amounts in words are table-driven per currency and reviewed by the Arabic language owner | This document §7.4; BR-L10N-004 | As stated | A generic number-to-words function gets Arabic gender agreement wrong on every receipt |
| Plural rules come from one generated CLDR table shared by all runtimes | This document §8 | As stated | Web, phone and SMS pick different Arabic forms for the same count |
| A release carries only reviewed Arabic | This document §1.4 and §1.5 | As stated | Draft machine Arabic reaches a ministry-facing certificate |

## Dependencies on other documents

| This document assumes | Stated in | Checked on |
|---|---|---|
| The seven BR-L10N rules and their test classes | Appendix S | Every lint run |
| The General group of settings and the Finance rounding setting | Appendix G | Group F review |
| Terminology ownership by Platform and `platform.terminology.changed.v1` | Appendix L.5, Appendix E | Every lint run |
| Wire shapes for `LocalizedText`, money, dates and the validation catalog | `22-api-conventions-and-error-catalog.md` §1.3, §1.4, §8 | Group F review |
| Folder layout for `i18n/` and `l10n/`, the reference store | `08-web-structure.md`, `09-mobile-structure.md` | Group F review |
| The name of the value object in `Nibras.BuildingBlocks.Localization` | `07-solution-structure.md` | Open point 1 |
| Database creation settings, collations and the migration bundle | `10-data-architecture.md` | Group C review |
| Four-way snapshots, icon flags, motion direction tokens | `14-design-system-and-ux.md` | Group F review |
| `UseCultureAttribute` and the test data tiers | `16-test-strategy.md` | Group E review |
| Image checks G1 to G10 and the pinned ICU and tzdata versions | `33-platform-support-and-dev-environments.md` | Group F review |
| Plug-in conformance under three cultures, iCal and OneRoster labels | `23-integrations-and-public-api.md` | Group F review |

## Open points

| Question | Default | Owner | Impact if the default is wrong | L | I | Score | In the register |
|---|---|---|---|---|---|---|---|
| 1. `07-solution-structure.md` names the bilingual value object `BilingualText`; this document and `22-api-conventions-and-error-catalog.md` use `LocalizedText` | `LocalizedText` everywhere, matching the wire and the proto; document 07 is aligned in its next revision | Architect | Two names for one type in the building blocks and the contracts | 2 | 1 | 2 | none |
| 2. Transliteration keys on the general `q` search | Only on duplicate detection and admissions and directory search, as `22-api-conventions-and-error-catalog.md` open point 2 | Architect, with the School service owner | Default-on gives surprising matches in a class list; default-off misses cross-script queries in the directory | 3 | 2 | 6 | none |
| 3. Who is the Arabic language owner | A named native speaker assigned by the product owner before phase 1 ends, with a deputy | Product owner | Draft Arabic accumulates and blocks the first release branch | 3 | 3 | 9 | RISK-05 |
| 4. Numeral default for new tenants | `western`, the school changes it in onboarding | Product owner | A school that expects Arabic-Indic digits sees Western digits until it changes the setting | 2 | 1 | 2 | none |

> L and I are the likelihood that the default is wrong and the impact if it is, on the 1 to 5 scales of `18-risk-register.md` Section 1. Score is L x I. A point that scores 12 or more names its RISK identifier in document 18; below that, the identifier if one covers it, or `none`. Kit-lint rules R24 and R33 check all of it (ADR-0022).

Two points that stood here in v9 are settled by the v9.1 brief and now sit in Decisions in force: the currency scale (Appendix G) and the long-vowel limit of the consonant key (BR-L10N-001).

## Review record

| Date | Reviewer | Verdict | Blocking items |
|---|---|---|---|
| 2026-09-22 | Group F review, round 1 (independent adversarial scorecard) | Blocked: the group scored below 4 on Completeness, Consistency, Feasibility, Risk honesty, Testability and Distinctiveness | None of the group's blocking gaps was in this document |
| 2026-09-26 | Group F review, round 2 | Blocked: the group scored below 4 on Completeness, Consistency, Risk honesty and Testability | This document was labelled Group D and its dependency checks pointed at Group B, D and E reviews for documents that belong to Groups D and F (Consistency) |
| 2026-09-26 | Round 3 remediation | Amended; awaiting the round 3 score | Relabelled Group F; the checks on Appendix G and on documents 08, 09, 14, 22, 23 and 33 now happen at the Group F review |
| 2026-09-26 | Group F review, round 3 | Blocked: the group scored below 4 on Completeness, because document 31 listed no transition-test ids per workflow | None of the group's blocking gaps was in this document; this record stopped at "awaiting the round 3 score" (Completeness, not blocking) |
| 2026-09-26 | Group F review, round 4 | Blocked: the group scored below 4 on Consistency, because SL-ACA-207 in document 34 built `LaunchLtiTool` in phase 2 against the default of document 17 | None in this document; this record still recorded no round-3 verdict (Completeness, not blocking) |
| 2026-09-26 | Round-4 scorecard, Group F, then remediation round 5 | Amended; awaiting the round 5 score | The round 3 and round 4 verdicts recorded above; nothing else in this document changed |

## How this document is verified

| Claim | Proof | Where it runs |
|---|---|---|
| Every screen and component is correct in both directions and both themes | The four-way snapshots (light LTR, light RTL, dark LTR, dark RTL) of every `States`, `Directions` and `Themes` story and every screen family, with the longest Arabic string (`14-design-system-and-ux.md`); Flutter goldens in LTR and RTL; `TC-L10N-101`, `TC-L10N-201`, `TC-L10N-601`, `TC-L10N-901` | Every pull request touching `@nibras/ui`, `nibras_ui` or a feature |
| Documents render correctly in both languages | Bilingual PDF baselines: a report card, a transcript, a transfer certificate, a service certificate, a receipt in SAR and in JOD, a statement and a class list, rendered in the Documents image and compared pixel by pixel within a tolerance (check G7); `TC-L10N-202`, `TC-L10N-301`, `TC-L10N-401`, `TC-L10N-801` | Every pull request touching Documents templates; nightly for all |
| No missing, empty, draft or placeholder-mismatched translation, no hard-coded string, no missing plural category | The missing-translation report (§1.4) across web, Flutter, templates and the generated namespaces | Every pull request; release branches with the stricter severities |
| Terminology overrides apply everywhere | `TC-L10N-110` (§1.6) | Nightly |
| Search normalization, the fold function and the trigram configuration | `ArabicNormalizationRulesTests` for BR-L10N-001; `TC-L10N-310` (`show_trgm` returns Arabic trigrams inside the database image); `TC-L10N-311` (C# and SQL agree over 5,000 names); the §3.4 and §3.5 tables as fixed cases | Every pull request in `BuildingBlocks` and School; the database image test nightly |
| Numerals render and parse as specified, identifiers never convert | `NumeralRenderingRulesTests` for BR-L10N-002, in .NET, Angular and Flutter | Every pull request |
| Hijri display with Gregorian as the source of truth | `HijriDisplayRulesTests` for BR-L10N-003 with the §5.2 pairs and the §5.3 cases | Every pull request; the pairs again inside the image |
| Amounts in words | `AmountInWordsRulesTests` for BR-L10N-004 with the §7.4 vectors, approved by the Arabic language owner | Every pull request in Finance and `BuildingBlocks` |
| Plural categories | `ArabicPluralRulesTests` for BR-L10N-005 with the values of §8, in all three runtimes | Every pull request |
| Culture pinning | `CultureInvarianceRulesTests` for BR-L10N-006 under the three cultures; the analyzer (G10); TC-PLAT-007 on Linux and Windows | Every pull request |
| Bilingual names and fallback | `BilingualNameRulesTests` for BR-L10N-007; the data-quality fallback count in Reporting | Every pull request in School and Reporting |
| The culture test inside the image | Checks G1 to G4 and G6 run inside each built service image: `ar-SA` resolves, `UmAlQuraCalendar` returns the §5.2 pairs, `Asia/Riyadh`, `Asia/Amman` and `Asia/Dubai` return the §6.2 offsets, ICU and tzdata versions equal the release manifest (TC-PLAT-004, TC-PLAT-005, TC-PLAT-006) | Every image build |
| Ramadan bell schedules and time zones | `TC-L10N-620` (§6.4) | Nightly |
| This document agrees with the catalogs | kit-lint R01, R02 and R17 for section and appendix references and Mermaid types, R30 for a comment on every column of a `CREATE TABLE`, and R31 for database and image names against Appendix L; `plan-consistency-checker` with `rtl-localization-reviewer` compares the rest, project and exchange names included, with `08-web-structure.md`, `14-design-system-and-ux.md`, `22-api-conventions-and-error-catalog.md`, `23-integrations-and-public-api.md` and `33-platform-support-and-dev-environments.md` | kit-lint on every change under `docs/`; the comparison at the Group F review and on every change to any of those documents |

### Test cases

This document defines the localization tests below; the other `TC-L10N-*` cases in the table above are defined by the documents that own them.

| Test case | What it proves | Covers |
|---|---|---|
| TC-L10N-110 | Given a tenant that overrides "Section" to "Class" and "Guardian" to "Parent", when every screen family, a report card, a receipt and a push notification are snapshotted in English and Arabic, then every one shows the override and none shows the default term | REQ-L10N-014 |
| TC-L10N-310 | Given the Nibras database image, when `show_trgm(nibras_ar_fold('محمد'))` runs inside it, then it returns Arabic trigrams, so a similarity search on an Arabic name can match | REQ-L10N-009, BR-L10N-001 |
| TC-L10N-620 | Given a Ramadan bell schedule valid 2027-02-08 to 2027-03-09, when the clock reaches 2027-02-08 in `Asia/Riyadh` and in `Asia/Dubai`, then the attendance cut-off switches to the Ramadan schedule at the right local time on each campus, and back after 2027-03-09 | REQ-L10N-011, REQ-SCD-023 |
| TC-L10N-901 | Given the operator console switched to Arabic, when every operator and administrator screen is opened, then each mirrors fully right to left and none is English-only | REQ-L10N-016 |
