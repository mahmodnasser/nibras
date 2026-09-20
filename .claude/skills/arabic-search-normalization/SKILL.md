---
name: arabic-search-normalization
description: How to make Arabic name and text search actually find things. Load before building any search, autocomplete, duplicate detection, or index over Arabic text.
---

# Arabic search normalization

A parent types أحمد and the registrar stored احمد. Without normalization the school concludes the product is broken, and they are right.

## What to normalize

| Class | Forms folded to | Why |
|---|---|---|
| Alef | ا | أ إ آ ٱ are typed interchangeably |
| Ya and alef maqsura | ي | ى and ي are regional habits |
| Ta marbuta | ه or dropped, consistently | فاطمة and فاطمه are the same person |
| Tatweel | removed | ـ is decoration, never meaning |
| Diacritics | removed | Harakat are rarely typed |
| Hamza on waw and ya | و and ي | Typing varies |
| Digits | one system for the index | Eastern and Western Arabic numerals are the same number |

Normalize **at index time and at query time with the same function.** Two implementations drift, and the drift is invisible until a parent complains.

Keep the original untouched for display. Normalization is for matching only; a person's name is not stored folded.

## PostgreSQL

Use an unaccent-style dictionary plus an explicit Arabic folding function, and a generated column so the index is always in step:

```sql
CREATE OR REPLACE FUNCTION nibras_ar_fold(t text) RETURNS text
LANGUAGE sql IMMUTABLE PARALLEL SAFE AS $$
  SELECT regexp_replace(
           translate(t, 'أإآٱىةـؤئ', 'اااايه\0وي'),
           '[ً-ْ]', '', 'g')
$$;

ALTER TABLE students
  ADD COLUMN name_ar_folded text
  GENERATED ALWAYS AS (nibras_ar_fold(name_ar)) STORED;

CREATE INDEX ix_students_tenant_name_ar_folded
  ON students (tenant_id, name_ar_folded)
  WHERE deleted_at IS NULL;
```

Query with the same function, never with a raw `LIKE` on the display column:

```sql
SELECT id, name_ar, name_en FROM students
WHERE tenant_id = @tenant
  AND name_ar_folded LIKE nibras_ar_fold(@term) || '%';
```

## Worked example

| Stored | Typed | Folded stored | Folded typed | Match |
|---|---|---|---|---|
| احمد | أحمد | احمد | احمد | yes |
| فاطمة | فاطمه | فاطمه | فاطمه | yes |
| عــلي | علي | علي | علي | yes |
| مُحَمَّد | محمد | محمد | محمد | yes |

Test all four rows, plus one mixed Arabic and Latin name, plus one name entered with Eastern Arabic numerals in the identifier field. Duplicate detection during import uses the same function, or the import will create a second Ahmed.