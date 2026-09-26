# 26. Migration and Onboarding Toolkit

> Plan document for the Nibras platform. Group F. It refines master brief Section 23 (the legacy migration toolkit), Section 39 (getting a school live), Section 12 item 9 and Section 12.1 item 29 (go live in a day, the smart defaults engine), WF-DATA-01 and WF-PLT-01 of Appendix R, and scenario N-04 of Appendix N; it does not re-derive them. Provisioning and the smart defaults engine are owned by `06-services/platform.md`; Arabic folding, numerals and Hijri conversion by `24-localization-and-calendars.md`. The import machinery that Documents runs (the `import_jobs` state, the worker job groups, the import endpoints and Saga 9's orchestrator) is specified in `06-services/documents.md`, which this document cites rather than restates; §2 here owns the two-stage staging design across services and the target services' Stage B (open point 4). Where this document and the brief disagree, an ADR records the deviation.

**Group** F · **Requirement areas covered** DOC (the import centre and the migration toolkit: REQ-DOC-011, REQ-DOC-012, REQ-DOC-013, REQ-DOC-016), PLT (onboarding: REQ-PLT-001 to REQ-PLT-004), L10N (import parsing only) · **Last updated** 2026-09-22 by the platform plan

## Purpose

This document makes moving a school from an old system a process and not a project (master brief Section 23). An engineer can build the templates, the staging tables, every validation rule, the dry-run report, the reconciliation report, rollback and the adapters from it; an onboarding lead can run the pre-flight, the wizard, the super-user certification and the go-live day from it. The readers are the Documents service owner (the import job group of `Documents.Worker`), the owners of the target services (School, Scheduling, Finance, Assessment, Identity), the Platform owner for onboarding, and the customer success lead who runs go-live.

## Scope

| In scope | Out of scope | Where the out-of-scope item lives |
|---|---|---|
| Excel templates per entity, columns and validation | The entity definitions themselves | Appendix F, `06-services/school.md`, `06-services/finance.md` |
| Staging, validation, merge, the import identifier | File upload, virus scan, signed URLs | `12-security-privacy-safety.md` §2.13; REQ-DOC-002 |
| Validation rules, including Arabic names, national IDs, Hijri dates, numerals | Folding steps, numeral parsing, Hijri conversion as functions | `24-localization-and-calendars.md` §3, §4.3, §5 |
| Dry-run report, reconciliation report, rollback | The long-running operation contract and progress | `22-api-conventions-and-error-catalog.md` (long-running operations) |
| Adapters for legacy systems and formats | Live integrations, OneRoster as a running feed | `23-integrations-and-public-api.md` §6.2 |
| The onboarding wizard, smart defaults confirmation | The inference table and its engine | `06-services/platform.md` §4.7, §5.1; `.claude/skills/smart-defaults-inference` |
| Provisioning as it affects onboarding, the first administrator | The provisioning saga design | WF-PLT-01; `13-workflows-and-sagas.md` Saga 1; `06-services/platform.md` §8 |
| Super-user certification, pre-flight, go-live checklist | Support tiers and escalation | Master brief Section 39; `06-services/platform.md` §5.9 |
| Throughput of a 10,000-row import | Load scenario definition | Appendix N, N-04 |

---

## Content

### 1. Mapping templates

One Excel workbook per entity, versioned, bilingual headers (English on row 1, Arabic on row 2, data from row 3), with a hidden `_meta` sheet carrying the template code and version so the parser recognises it (WF-DATA-01 `Uploaded` to `Parsed`, `TC-DATA-001`). Dropdown columns use Excel data validation lists generated from the tenant's settings at download time, so the codes offered are the codes the tenant has. Templates are downloaded from the import centre and never from a static file, because codes differ per tenant. Workbooks are produced with ClosedXML (MIT); EPPlus is excluded by the licence rules.

#### 1.1 Template catalogue and load order

Load order follows references: an entity is importable only after what it points to exists.

| Order | Template code | Entity | Target service | Permission to commit (with `documents.imports.commit`) | Natural key for duplicates and updates |
|---|---|---|---|---|---|
| 1 | `sections` | Grade levels and sections for the year | School | `school.sections.create` | Year + grade code + section code |
| 2 | `subjects` | Subjects and their grade mapping | School | `school.subjects.create` | Subject code |
| 3 | `staff` | Staff records, with an optional invitation | School, Identity for invitations | `school.staff.create`, `identity.users.invite` | National ID, else staff number |
| 4 | `students` | Students and enrolment into a section | School | `school.students.create` | National ID, else student number |
| 5 | `guardians` | Guardians and links to students | School | `school.guardians.create`, `school.guardians.link` | National ID, else mobile number + name |
| 6 | `timetable` | Weekly timetable lessons | Scheduling | `scheduling.timetable.edit` | Section + day + period |
| 7 | `fee-plans` | Fee items, structures and plan assignment | Finance | `finance.structures.create`, `finance.plans.assign` | Plan code; student number for assignment |
| 8 | `opening-balances` | Balance brought forward per student account | Finance | `finance.invoices.create` | Student number + as-of date |
| 9 | `historical-results` | Prior-year results per student and subject | Assessment | `assessment.marks.enter` | Student number + year + term + subject |

#### 1.2 `students`

| Column | Required | Type and format | Validation |
|---|---|---|---|
| `student_number` | yes, unless the tenant auto-numbers | Text, up to 20 | Unique in file and tenant |
| `first_name_ar`, `father_name_ar`, `grandfather_name_ar`, `family_name_ar` | first and family yes | Arabic letters, spaces | NFKC; Arabic script only; `PersonName` parts per `24-localization-and-calendars.md` §2.2 |
| `first_name_en`, `family_name_en` | yes when the tenant's primary language is English, else no | Latin letters, spaces, hyphen, apostrophe | Latin script only |
| `gender` | yes | Dropdown `M`, `F` | In list |
| `date_of_birth` | yes | Date; Gregorian or Hijri, §3.5 | Age between 2 and 22 on the year start |
| `nationality` | yes | ISO 3166-1 alpha-2 dropdown | In list |
| `national_id_type` | yes | Dropdown `national`, `iqama`, `emirates-id`, `passport`, `personal-number` | Consistent with nationality and country, §3.4 |
| `national_id` | yes except for `passport` holders without residency | Text | Format per §3.4; unique in file and tenant |
| `passport_number` | when type is `passport` | Text, up to 20 | Alphanumeric |
| `grade_code`, `section_code` | yes | Dropdown from the year's sections | Section exists in file or tenant |
| `enrolment_date` | yes | Date | Inside the academic year |
| `status` | yes | Dropdown `enrolled`, `applicant`, `withdrawn`, `graduated` | In list |
| `medical_alert` | no | Text | Never imported: the column is refused with `sensitiveColumnRefused` and routed to the medical summary form, because Sensitive medical data (Appendix J.2) never passes through Documents staging |
| `photo_consent` | no | Dropdown `yes`, `no` | Default `no` when blank |

#### 1.3 `guardians`

| Column | Required | Type and format | Validation |
|---|---|---|---|
| `guardian_ref` | yes | Text, file-local key | Unique in file |
| `name_ar` parts, `name_en` parts | Arabic first and family yes | As students | As students |
| `relationship` | yes | Dropdown `father`, `mother`, `guardian`, `other` | In list |
| `mobile` | yes | E.164 or local form | Normalised to E.164 with the tenant's country code; Arabic-Indic digits folded |
| `email` | no | Email | RFC 5322 simple form; lower-cased |
| `national_id_type`, `national_id` | yes for citizens and residents | As students | As students |
| `preferred_language` | yes | Dropdown `ar`, `en` | In list |
| `student_numbers` | yes | Semicolon-separated | Every student exists in file or tenant |
| `has_parental_access`, `is_emergency_contact`, `contact_order` | yes | Yes or no, integer 1 to 5 | At least one guardian per student with parental access |
| `custody_restriction` | no | Yes or no | A yes creates a task for the registrar to record the custody detail by hand; custody text is never imported |

#### 1.4 `staff`

| Column | Required | Type and format | Validation |
|---|---|---|---|
| `staff_number` | yes | Text | Unique |
| Name parts in both languages | Arabic first and family yes | As students | As students |
| `national_id_type`, `national_id` | yes | As students | As students |
| `email` | yes when `invite` is yes | Email | Unique among staff in the tenant |
| `mobile` | no | As guardians | As guardians |
| `job_title`, `department` | yes | Text, dropdown | Department exists |
| `role_template` | yes when `invite` is yes | Dropdown from Appendix I templates | In list; high-risk templates refused, granted later through four-eyes |
| `campus_code` | yes | Dropdown | Exists |
| `start_date` | yes | Date | Not in the future beyond the year start |
| `subjects_taught` | no | Semicolon-separated subject codes | Exist |
| `invite` | yes | Yes or no | Yes sends `identity.user.invited.v1` after commit, never during a dry run |
| Salary, bank account | never | not in the template | Hr data is entered in Hr, never through import staging (Appendix J.4) |

#### 1.5 `sections` and `subjects`

| Template | Column | Required | Validation |
|---|---|---|---|
| `sections` | `grade_code`, `grade_name_ar`, `grade_name_en` | yes | Grade code unique per year |
| `sections` | `section_code`, `section_name_ar`, `section_name_en` | yes | Unique per grade |
| `sections` | `capacity` | yes | Integer 1 to 60 |
| `sections` | `homeroom_staff_number` | no | Exists in the staff file or tenant |
| `sections` | `campus_code`, `stream` | campus yes | Exists; stream from the tenant's terminology |
| `subjects` | `subject_code`, `name_ar`, `name_en` | yes | Code unique |
| `subjects` | `grade_codes` | yes | Semicolon-separated; each exists |
| `subjects` | `weekly_periods` | yes | Integer 0 to 15 |
| `subjects` | `grading_scheme_code` | yes | Exists in the tenant's schemes |

#### 1.6 `timetable`

| Column | Required | Validation |
|---|---|---|
| `section_code` | yes | Exists |
| `day` | yes | A working day of the tenant's work week, in English or Arabic day names |
| `period_no` | yes | Inside the tenant's bell schedule |
| `subject_code` | yes | Taught in that grade |
| `staff_number` | yes | Exists; teaches the subject |
| `room_code` | no | Exists |
| Cross-row | | No teacher, room or section double-booked in one slot; conflicts reported with both rows named; the import lands as a draft timetable that a person publishes through `scheduling.timetable.publish` |

#### 1.7 `fee-plans`

| Column | Required | Validation |
|---|---|---|
| `plan_code`, `plan_name_ar`, `plan_name_en` | yes | Code unique |
| `fee_item_code`, `amount` | yes | Item exists or is created from the same file; amount positive, parsed per §3.6, rounded to the currency's minor units |
| `currency` | yes | The tenant's currency; SAR, AED or JOD |
| `tax_treatment` | yes | Dropdown from the confirmed tax setting; never inferred silently |
| `instalments` | yes | Integer 1 to 12 with due dates inside the year |
| `student_numbers` or `grade_codes` | one of them | Exist |

#### 1.8 `opening-balances`

| Column | Required | Validation |
|---|---|---|
| `student_number` | yes | Exists |
| `as_of_date` | yes | One date per file, before the first invoice run |
| `balance` | yes | Signed decimal; positive owed, negative credit |
| `currency` | yes | The tenant's currency |
| `legacy_reference` | yes | Text, kept on the opening document for audit |
| Control total | yes | The `_meta` sheet carries the legacy system's total; the dry run fails when the file sum differs by any amount |

Opening balances become one posted opening document per student, never edited invoices; a correction after commit is a credit note, per master brief Section 36.

#### 1.9 `historical-results`

| Column | Required | Validation |
|---|---|---|
| `student_number` | yes | Exists |
| `academic_year` | yes | A closed prior year, Gregorian or Hijri label |
| `term`, `subject_code` | yes | Term label; subject exists or is mapped to a legacy subject code |
| `mark` or `grade_letter` | one of them | Mark numeric within the scheme range; letter in the scheme's bands |
| `grading_scheme_code` | yes | Exists; imported results carry `source = legacy` and are locked on arrival |
| `comment` | no | Text up to the comment length setting |

Imported results are historical: they feed the transcript and Student 360, never a current-term calculation.

### 2. Staging design

Two stages, because of database-per-service: Documents owns the file and the validation, and each target service owns its own write. Documents never writes another service's database, and no target service trusts a row it has not validated itself.

#### 2.1 Stage A in Documents: parse and validate

```sql
-- nibras_documents: one row per uploaded import
CREATE TABLE import_staging.import_job (
    id                 uuid        PRIMARY KEY,   -- the import identifier, stamped on every row the import writes anywhere
    tenant_id          uuid        NOT NULL,      -- tenant; named filter and row-level security key
    template_code      text        NOT NULL,      -- §1.1 code, e.g. 'students'
    template_version   int         NOT NULL,      -- from the _meta sheet; an unknown version is refused at parse
    source_adapter     text        NOT NULL,      -- §6 adapter code, 'excel-template' for the native templates
    file_id            uuid        NOT NULL,      -- the scanned upload in object storage
    state              text        NOT NULL,      -- WF-DATA-01 state: Uploaded, Parsed, Validated, ErrorsReported, DryRunReady, Committing, Committed, CommitFailed, RolledBack, Sealed
    row_count          int         NOT NULL DEFAULT 0, -- data rows parsed
    error_count        int         NOT NULL DEFAULT 0, -- blocking findings
    warning_count      int         NOT NULL DEFAULT 0, -- non-blocking findings
    dry_run_at         timestamptz NULL,          -- last dry run; a commit needs one less than 24 hours old
    committed_at       timestamptz NULL,          -- set on Committed
    rollback_until     timestamptz NULL,          -- committed_at plus 7 days, the WF-DATA-01 import rollback window (§5.3)
    created_by         uuid        NOT NULL,      -- the administrator who uploaded
    created_at         timestamptz NOT NULL       -- upload time
);

-- nibras_documents: one row per data row, as parsed and normalised
CREATE UNLOGGED TABLE import_staging.import_row (
    job_id             uuid        NOT NULL,      -- import_job.id
    tenant_id          uuid        NOT NULL,      -- tenant, repeated for row-level security
    row_no             int         NOT NULL,      -- row number in the source sheet, as the user sees it
    raw                jsonb       NOT NULL,      -- cell values as read, for the error report
    normalised         jsonb       NULL,          -- values after folding, numeral and date normalisation
    natural_key        text        NULL,          -- §1.1 key after normalisation, for duplicate detection
    match_kind         text        NULL,          -- 'insert', 'update', 'skip' or 'duplicate' once matched against the tenant
    match_target_id    uuid        NULL,          -- the existing record an update or duplicate points to
    PRIMARY KEY (job_id, row_no)
);

-- nibras_documents: findings that make up the dry-run report
CREATE TABLE import_staging.import_finding (
    job_id             uuid        NOT NULL,      -- import_job.id
    tenant_id          uuid        NOT NULL,      -- tenant
    row_no             int         NOT NULL,      -- 0 for file-level findings
    column_name        text        NULL,          -- template column; NULL for row or file findings
    code               text        NOT NULL,      -- validation code from §3.8
    severity           text        NOT NULL,      -- 'error' blocks commit, 'warning' does not
    value_seen         text        NULL,          -- the offending value, masked to the last 4 characters for national IDs
    suggestion         text        NULL,          -- corrected value when one is certain, e.g. a folded name or a converted date
    related_row_no     int         NULL           -- the other row in a duplicate or conflict pair
);
```

National identity and passport numbers are the one Sensitive field group that passes through staging, because they are the duplicate key: in `import_row` they are held as a keyed hash in `natural_key` and as ciphertext under the tenant data key in `normalised`, blanked in `raw`, and masked in every finding and report. `import_row` is unlogged: it is rebuilt from the file on any failure, never needed after `Sealed`, and dropped with the job. The Stage A tables are deleted 30 days after `Sealed` or `RolledBack`; the error report document survives with the Documents retention of its owner record.

#### 2.2 Stage B in the target service: COPY, check, merge

Each target service exposes one internal gRPC method, `ApplyImportBatch`, called by `Documents.Worker` with batches of 1,000 normalised rows and the import identifier. Inside the target service:

| Step | Mechanism |
|---|---|
| 1 Receive | Batch in, with `job_id`, `batch_no` and an idempotency key of both; a repeated batch returns the stored result |
| 2 COPY | Binary `COPY` into a transaction-scoped temporary table shaped like the entity's own staging record |
| 3 Check again | The service re-runs its own domain validation and reference checks against its own data; Documents' validation is a preview, not a trust boundary |
| 4 Capture before values | For rows that update an existing record, the current values are written to `import_before_image` |
| 5 Merge | `INSERT … ON CONFLICT (tenant_id, natural key) DO UPDATE` for updates the dry run allowed; plain `INSERT` for inserts; every written row carries `import_id` |
| 6 Events | Domain events for created entities go through the outbox in the same transaction, marked `origin = import` so consumers can batch (Notification suppresses welcome messages until go-live) |
| 7 Answer | Per-row outcome to Documents; a failed batch rolls back its own transaction and the job enters `CommitFailed` |

```sql
-- In each target service database, e.g. nibras_school: before values for rollback
CREATE TABLE import_before_image (
    import_id      uuid        NOT NULL,  -- the Documents import identifier
    tenant_id      uuid        NOT NULL,  -- tenant; row-level security key
    entity_type    text        NOT NULL,  -- 'student', 'guardian', 'section', …
    entity_id      uuid        NOT NULL,  -- the updated record
    row_version    bigint      NOT NULL,  -- the record's version after the import wrote it; a later edit makes it a conflict on rollback
    before_values  jsonb       NOT NULL,  -- the columns the import changed, as they were
    captured_at    timestamptz NOT NULL,  -- when the before image was taken
    PRIMARY KEY (import_id, entity_type, entity_id)
);
```

Sensitive columns in `before_values` (national identity numbers) are stored encrypted with the tenant data key, as the source column is (Appendix J.5 rule 7). The table is purged when the job is `Sealed`.

#### 2.3 Throughput

Master brief Section 21 and N-04: 10,000 rows validated and committed in under 5 minutes, the dry run in under 2 minutes, streaming with bounded memory. Parsing streams rows with the OpenXML SAX reader; Stage B batches of 1,000 keep each transaction short; the job group runs on the bulk lane of `Documents.Worker` so other tenants see no more than 10 percent p95 degradation.

### 3. Validation rules

Rules run in Stage A in this order; a row collects every finding rather than stopping at the first, so one dry run shows everything wrong with the file.

#### 3.1 File and structure

| Rule | Severity | Code |
|---|---|---|
| Template code and version recognised from `_meta` or by the adapter | error, file | `unknownTemplate` |
| Every required column present; unknown columns reported and ignored | error for missing, warning for unknown | `missingColumn`, `unknownColumn` |
| Row limit per file: 50,000 | error, file | `DOCUMENTS_IMPORT_ROW_LIMIT_EXCEEDED` |
| File passed the ClamAV scan and the type allow-list | error, file | `DOCUMENTS_VIRUS_DETECTED`, `DOCUMENTS_FILE_TYPE_NOT_ALLOWED` |
| Formulas are read as their cached values, never evaluated; macros are refused | error, file | `macroRefused` |
| Encoding for CSV is detected (UTF-8, UTF-16, Windows-1256) and converted to UTF-8 | warning when not UTF-8 | `legacyEncoding` |

#### 3.2 Required fields and types

| Rule | Code |
|---|---|
| A required cell is empty after trimming | `required` |
| A dropdown value is not in the tenant's list; Arabic and English labels are both accepted and mapped to the code | `notInList` |
| Text longer than the column limit | `tooLong` |
| Script check: an Arabic name part contains Latin letters, or the reverse | `wrongScript` |
| A column that would carry Sensitive or level S detail the template does not allow | `sensitiveColumnRefused` |

#### 3.3 Referential checks

| Rule | Code |
|---|---|
| A reference (section, subject, staff, student, campus, room, fee item) exists in the same file, in an earlier committed import of this session, or in the tenant | `referenceMissing` |
| A guardian links to a student in a file not yet committed: allowed when both files are in the same import batch, and ordered by §1.1 | `referenceOrder` (warning) |
| A teacher in the timetable teaches the subject named | `teacherSubjectMismatch` |
| Timetable double-booking of teacher, room or section | `slotConflict` with `related_row_no` |
| Every student has at least one guardian with parental access, checked when the guardians file is dry-run | `noParentalGuardian` (warning, error at go-live pre-check) |
| An opening balance or historical result names a student not enrolled in the tenant | `referenceMissing` |

#### 3.4 National identity numbers

| Country and type | Format | Check | Code on failure |
|---|---|---|---|
| Saudi Arabia, national ID | 10 digits, first digit `1` | Luhn-style check digit over the 10 digits as published for the Saudi identity number | `invalidNationalId` |
| Saudi Arabia, iqama (resident) | 10 digits, first digit `2` | Same check digit | `invalidNationalId` |
| United Arab Emirates, Emirates ID | 15 digits, `784` then 4-digit birth year then 7-digit sequence then 1 check digit; hyphens accepted and stripped | Luhn check digit over 15 digits; birth year consistent with `date_of_birth` within one year | `invalidNationalId`, `idBirthYearMismatch` (warning) |
| Jordan, national number | 10 digits for Jordanian citizens | Format only; no public check-digit algorithm is applied | `invalidNationalId` |
| Jordan, personal number for non-Jordanians | Alphanumeric as issued, up to 12 characters | Format only | `invalidNationalId` |
| Any, passport | 6 to 20 letters and digits | Format only | `invalidPassport` |
| All | Arabic-Indic and extended digits folded before the check; spaces and hyphens removed | | |
| All | Unique within the file and against the tenant, compared on the folded value | | `duplicateNationalId` with `related_row_no` or the existing record |

The algorithms live in `Nibras.BuildingBlocks.Localization` as `NationalIdValidator` per country, selected by the country plug-in, so the fourth country of `27-compliance-and-legal.md` §12 adds a validator and a test, not an import change. Values appear in the error report masked to the last four characters. `TC-DATA-800` proves every row of this table.

#### 3.5 Dates, Hijri-tolerant

| Input seen | Interpretation |
|---|---|
| Excel date cell | Read as a date serial, 1900 system, no culture involved |
| `yyyy-mm-dd` | ISO Gregorian |
| `dd/mm/yyyy` or `dd-mm-yyyy` | Day first, always; the template says so and `mm/dd` is never guessed |
| A year from 1300 to 1500 in any of the forms above | Hijri Umm al-Qura, converted to Gregorian per `24-localization-and-calendars.md` §5 |
| A year suffixed `هـ` or `AH`, or an Arabic Hijri month name (محرم to ذو الحجة) | Hijri, converted |
| Arabic Gregorian month names (يناير, كانون الثاني) or English month names | Gregorian |
| Arabic-Indic digits in any of the above | Folded first |
| A Hijri day 30 in a 29-day month | `invalidHijriDate`, with the suggestion of day 29 or day 1 of the next month; never silently shifted |
| Anything else | `invalidDate` |

Every converted date is shown in the dry-run preview in both calendars, so the registrar can see that 1432-05-10 became 2011-04-14 before committing. The source value is kept in `raw`. `TC-DATA-801` proves every row of this table.

#### 3.6 Numerals and amounts

Numbers are parsed per `24-localization-and-calendars.md` §4.3: Arabic-Indic and extended digits and the Arabic decimal and thousands separators are accepted; mixed digit systems in one value are refused as `mixedNumerals`; a comma is never a decimal separator (`invalidNumber`). Amounts are then rounded to the currency's minor units with the tenant's rounding mode, and a rounding change is a warning (`amountRounded`) showing both values. `TC-DATA-802` proves these rules; the Arabic-Indic digits of a Windows-produced workbook are `TC-PLAT-008` (document 33).

#### 3.7 Duplicate detection

| Layer | Rule | Outcome |
|---|---|---|
| Exact key | Same national ID, or same student or staff number | `duplicateNationalId` or `duplicateKey`: error inside the file; against the tenant, `match_kind = update` when the template allows updates, else error |
| Arabic name | Folded full name (`nibras_ar_fold` steps of `24-localization-and-calendars.md` §3.1, in the same order, via `ArabicNormalizer.Normalize()`) plus date of birth equal | `possibleDuplicate` warning with both rows; "مُحمَّد" and "محمد", "فاطمة" and "فاطمه" fold together, while a dropped hamza as a letter does not |
| Name order | Four-part Arabic names compared with and without the grandfather part | `possibleDuplicate` warning |
| Transliteration | English names compared through the transliteration key of `24-localization-and-calendars.md` §3.5 plus date of birth | `possibleDuplicate` warning; the admissions and import searches are the only places this key applies |
| Guardians | Folded name plus mobile number | `possibleDuplicate` warning; the dry run proposes linking to the existing guardian |
| Siblings | Same family name, same guardian national ID | Informational: grouped in the preview so siblings share guardian records |

A warning never blocks; the dry-run preview shows each pair with "treat as the same person" or "keep both", and the choice is stored on the row before commit (REQ-DOC-013).

#### 3.8 Validation codes

Codes are the `params.fieldErrors[].code` values inside `DOCUMENTS_IMPORT_VALIDATION_FAILED`, following the field-code convention of `22-api-conventions-and-error-catalog.md`: `unknownTemplate`, `missingColumn`, `unknownColumn`, `macroRefused`, `legacyEncoding`, `required`, `notInList`, `tooLong`, `wrongScript`, `sensitiveColumnRefused`, `referenceMissing`, `referenceOrder`, `teacherSubjectMismatch`, `slotConflict`, `noParentalGuardian`, `invalidNationalId`, `idBirthYearMismatch`, `invalidPassport`, `duplicateNationalId`, `duplicateKey`, `possibleDuplicate`, `invalidDate`, `invalidHijriDate`, `mixedNumerals`, `invalidNumber`, `amountRounded`, `controlTotalMismatch`. Each has an Arabic and an English message and a fix, in the Documents language bundle.

### 4. The dry-run error report

A dry run writes nothing to any target service (REQ-DOC-012). It produces two documents through `Documents.Worker` and publishes `documents.document.generated.v1` for each.

#### 4.1 The annotated workbook

The school's own file, returned with three columns added at the right of each sheet and the offending cells highlighted, so the fix happens where the data is.

| Added column | Content |
|---|---|
| `nibras_status` | `ok`, `warning`, `error` |
| `nibras_findings` | One line per finding: column, code, message in the user's language |
| `nibras_suggestion` | The certain correction where one exists, ready to copy |

#### 4.2 The summary

| Section | Content |
|---|---|
| Header | Import identifier, template and version, adapter, file name, uploaded by, dry-run time, valid until (24 hours) |
| Counts | Rows read; would insert; would update; would skip; errors; warnings; per entity |
| Findings by code | Code, count, first 5 row numbers, message, fix |
| Duplicates to decide | Each `possibleDuplicate` pair with both rows side by side |
| Converted values | Hijri dates converted, names folded, numerals normalised, amounts rounded: counts and the first 20 examples |
| Control totals | Opening-balance sum against the `_meta` total; students per section against the legacy count where supplied |
| Next step | "Fix and re-upload" when any error exists; "Commit" otherwise, with the permission it needs |

The machine-readable form is `GET /api/v1/documents/imports/{id}/findings` with the keyset envelope of `22-api-conventions-and-error-catalog.md`; the web preview reads it, not the workbook.

### 5. Commit, reconciliation and rollback

#### 5.1 Commit

| Guard | Source |
|---|---|
| Dry run less than 24 hours old, no errors, every duplicate pair decided | WF-DATA-01 `DryRunReady` to `Committing`, `TC-DATA-004` |
| `documents.imports.commit` plus the target permission of §1.1 | Appendix B |
| A commit with no progress for 10 minutes alerts the platform operator | WF-DATA-01 |
| A failed batch reverses the import as a unit | `TC-DATA-005` |
| On completion `documents.import.completed.v1` with `succeeded`, `failed` and `errorReportId` | Appendix E |

#### 5.2 The reconciliation report

Produced automatically at `Committed`, before anyone is told the import is done.

| Check | Source side | Nibras side | Pass condition |
|---|---|---|---|
| Row counts per entity | File rows minus skips | Target service count of rows carrying `import_id` | Equal |
| Students per section | File | School count per section | Equal, per section |
| Guardian links | File links | School links carrying `import_id` | Equal |
| Opening balances | `_meta` control total | Finance sum of opening documents carrying `import_id` | Equal to the minor unit |
| Fee plan assignments | File | Finance assignments | Equal |
| Historical results | File rows | Assessment rows | Equal, per year and term |
| Timetable | File lessons | Scheduling draft lessons | Equal; conflicts zero |
| Sample | 20 random rows | Read back through each service's API | Every field equal after normalisation |
| Checksum | Hash over normalised rows | Hash over the same fields read back | Equal |

A failed check marks the import `Committed` with a reconciliation warning and opens a Data Quality Center issue through `reporting.data-quality.issue-detected.v1`; the go-live checklist of §10 refuses to proceed while one is open.

#### 5.3 Rollback within the import rollback window

Two windows exist in this kit and they are different things. The **import rollback window** below is 7 days and belongs to one import job. The **tenant deletion cooling-off** is 30 days, belongs to a whole tenant, and is owned by master brief Section 32, BR-PLT-003 and WF-PLT-03, and this document neither sets nor restates it. Neither number is derived from the other, and "cooling-off" on its own always means the 30-day tenant one.

| Rule | Mechanism | Test |
|---|---|---|
| Window | 7 days after `Committed` (WF-DATA-01, which owns the value); after it the job is `Sealed` and before images are purged | `TC-DATA-006` (Appendix R) |
| Permission | `documents.imports.rollback` (elevated) | Appendix B |
| Order | Reverse of §1.1: results, balances, fee plans, timetable, guardians, students, staff, subjects, sections | `ImportRollbackOrderTests` |
| Inserted rows | Deleted by `import_id` in each target service | `TC-DATA-006` (Appendix R) |
| Updated rows | Restored from `import_before_image` when `row_version` still matches | `TC-DATA-006` (Appendix R) |
| Rows touched after the import | Reported as conflicts and left alone | WF-DATA-01 compensation |
| Finance | Opening documents are reversed by a credit note, never deleted, because posted documents are immutable (master brief Section 36) | `ImportRollbackFinanceTests` |
| Invitations | Unaccepted invitations revoked; accepted accounts reported as conflicts | `ImportRollbackIdentityTests` |
| Proof | A checksum of the affected tables equals the pre-import checksum, excluding reported conflicts | N-04 threshold; `TC-DATA-303` |
| Audit | Rollback is audited with the actor and the counts | `documents.audit.recorded.v1` |
| After go-live | A rollback after the tenant is `Live` needs a second approver, because families may already see the data | `DOCUMENTS_IMPORT_ROLLBACK_REQUIRED` explains the state |

### 6. Adapters for legacy systems and formats

An adapter turns a source export into the §1 template rows; everything after it is shared. Adapters are pure functions from file to rows plus a mapping report, live in `Documents.Worker` under `Imports/Adapters/`, and each has a fixture file and a golden output.

| Adapter code | Source | Input the school provides | Entities covered | Notes |
|---|---|---|---|---|
| `excel-template` | Nibras templates of §1 | The filled workbook | All nine | The reference adapter; every other adapter's output is compared against it |
| `csv-mapped` | Any system that exports CSV or XLSX | The export plus a column mapping built once in the mapping screen and saved per tenant | All nine | The mapping screen shows the first 20 rows and lets the user drag source columns to template columns; saved mappings are reused on the next file |
| `oneroster-csv` | OneRoster 1.2 bulk CSV from any compliant system | `orgs.csv`, `academicSessions.csv`, `classes.csv`, `courses.csv`, `users.csv`, `enrollments.csv`, `demographics.csv` | Sections, subjects, staff, students, guardians | Consumes the same 1.2 profile Platform publishes (`23-integrations-and-public-api.md` §6.2) |
| `powerschool-export` | PowerSchool SIS | Students, staff, sections and historical grades exports in the default field lists | Students, guardians, staff, sections, subjects, historical results | Contact fields mapped from the contacts export; guardians deduplicated by §3.7 |
| `openemis-export` | OpenEMIS, used by public schools in Jordan | The institution students, staff and classes Excel reports | Students, staff, sections, subjects | Jordanian national numbers kept; Arabic names split into four parts by the name-part rule |
| `fedena-export` | Fedena school ERP | Student, guardian, employee, batch and finance exports | Students, guardians, staff, sections, fee plans, opening balances | Fee collections summarised into opening balances with a control total |
| `noor-export` | Saudi Noor system reports saved as Excel | The student list and class list reports | Students, sections | Hijri dates converted per §3.5; Noor student numbers kept as a legacy identifier |
| `accounting-balances` | Any accounting package export of receivables by student | Trial balance or aged receivables in CSV or XLSX | Opening balances | Requires the control total; unmatched accounts listed for manual mapping |
| `timetable-xml` | aSc Timetables XML export | The exported XML | Timetable, subjects, rooms | Periods mapped to the tenant bell schedule; conflicts reported, never resolved automatically |

A new adapter is certified like a plug-in (`23-integrations-and-public-api.md` §8.3): fixture, golden output, both cultures, both operating systems. `TC-DATA-803` is the golden-output test every adapter in the table runs.

### 7. The onboarding wizard

The wizard runs in `Onboarding` of WF-PLT-01, after provisioning has finished. It asks two questions and shows everything else as inferred, following `.claude/skills/smart-defaults-inference`.

| Step | Screen | Calls | Done when |
|---|---|---|---|
| 1 | Country and school type | `POST /api/v1/platform/smart-defaults/preview` (`06-services/platform.md` §5.1) | Both chosen |
| 2 | Confirmation screen (§7.1) | Preview result | Every "confirm explicitly" row confirmed by a named person |
| 3 | Apply | `POST /api/v1/platform/smart-defaults/application` with `Idempotency-Key` | Settings written with source `inferred` |
| 4 | Campus, academic year and terms | School | Year opened (`school.academic-year.opened.v1`) |
| 5 | Branding and domain | Platform §5.2 | Logo and colours saved; custom domain optional |
| 6 | Data import | The import centre, §1 to §5, in §1.1 order | Every required template committed and reconciled |
| 7 | Roles and invitations | Identity | Two super-users invited (§9) |
| 8 | Review and go-live booking | Platform onboarding checklist | Go-live date booked with a named contact |

Every step is resumable, carries values forward (WCAG 3.3.7, `14-design-system-and-ux.md` §11) and shows its progress on the onboarding checklist of REQ-PLT-001.

#### 7.1 The confirmation screen

| Column | Content |
|---|---|
| Setting | Name in the tenant's language |
| Inferred value | The value, rendered as it will appear (numerals, calendar) |
| Because | The reason from the engine, for example "Saudi Arabia works Sunday to Thursday", as a Because panel at rung 1 |
| Confidence | High or medium |
| Confirm explicitly | Yes for tax treatment, invoice numbering and the e-invoicing plug-in, always; yes for medium-confidence rows |
| Edit | An inline control on every row |

One "This is correct" button confirms all high-confidence rows; each explicit row needs its own tick, and the confirmation records the person and time as an audit entry. Legal and financial values are never applied silently.

### 8. Provisioning and the first administrator

Provisioning is WF-PLT-01 and Saga 1 of `13-workflows-and-sagas.md`; its steps, compensations and timeouts are binding there and in `06-services/platform.md` §8. What onboarding depends on:

| Point | Rule | Test |
|---|---|---|
| Tenant region fixed at `Validated` | Residency never changes afterwards (`PLATFORM_RESIDENCY_VIOLATION`) | `TC-PLT-101` (Platform sheet) |
| Routes answer `PLATFORM_PROVISIONING_IN_PROGRESS` until `Provisioned` | The wizard shows provisioning progress instead | `TC-GW-014` (Gateway sheet) |
| First administrator invitation | Sent as `identity.user.invited.v1` to the owner contact on `Provisioned`; single use, 7-day expiry, bound to the tenant; accepting it runs WF-IDN-01 and requires a second factor | `TC-PLT-003`, `TC-IDN-006` |
| The seeded `admin` account | Platform scope only (ADR-0003); never the school's administrator and never used for onboarding; safeguards in `12-security-privacy-safety.md` §8 | `TC-SEC-360` (document 12) |
| Failure | A compensated saga leaves no schema, queue or identity; the operator sees the failed step verbatim | `TC-PLT-004`, `TC-PLT-005` |
| `Onboarding` to `Live` | Academic year, campus and the owner account present; the seeded password changed | `TC-PLT-006` (Appendix R) |
| Demo data | Optional at signup; a tenant with demo data must reset it before the first real import, and the import centre refuses a commit into a tenant still holding demo rows | `ImportDemoGuardTests` |

### 9. Super-user certification

Master brief Section 39: two super-users per school complete a short certification in setup, roles and permissions, request types, the mark and report-card cycle, and the audit viewer.

| Module | Hands-on task on the tenant's sandbox | Pass condition |
|---|---|---|
| Setup | Change the bell schedule for Ramadan, add a campus, edit a terminology override | The three changes are visible and audited |
| Roles and permissions | Clone a role template, grant a permission with a data scope, request a high-risk grant and have the other super-user approve it | Four-eyes approval recorded; self-approval refused |
| Request types | Create a leave request type with a two-step approval chain and submit one | Request reaches the second approver |
| Mark and report-card cycle | Enter marks for a section, approve, lock, generate report cards, publish one | Report card verifies by QR |
| Audit viewer | Find who changed a given mark and export the entry with a reason | Export watermarked and reported |

| Rule | Detail |
|---|---|
| Where | The sandbox cloned from the tenant's configuration (Section 12.1 item 41), never live data |
| Format | 90 minutes, guided, in Arabic or English |
| Record | Completion per person on the onboarding checklist in Platform; go-live refuses to book with fewer than two certified super-users |
| Renewal | Offered again after each major release that changes a module |

### 10. Pre-flight and go-live

#### 10.1 Pre-flight with the school

| # | Item | Owner | Evidence |
|---|---|---|---|
| 1 | Legacy system named and the adapter of §6 chosen, or `csv-mapped` agreed | Onboarding lead | Recorded on the checklist |
| 2 | Data clean-up agreed: leavers removed, duplicate families merged, sections for the new year known | School registrar | Clean-up list signed |
| 3 | Templates downloaded from the tenant, not from an old copy | Registrar | Template version in `_meta` |
| 4 | Decisions the wizard will ask for gathered: country, school type, year shape, bell schedule, grading scheme, tax registration, invoice numbering, e-invoicing registration | Principal, accountant | Filled decision sheet |
| 5 | Opening-balance cut-off date and control total from the legacy finance system | Accountant | Control total on the sheet |
| 6 | Two super-users named | Principal | Names on the checklist |
| 7 | Guardian contact coverage: share of students with a mobile number | Registrar | At least 95 percent, or a plan for the rest |
| 8 | Go-live day chosen outside exam weeks, with a named Nibras contact available | Onboarding lead, principal | Booked date |
| 9 | Terms, privacy policy and data processing agreement accepted by the owner | Owner | Acceptance record (`27-compliance-and-legal.md` §5) |

#### 10.2 Go-live checklist

| # | Check | Automated | Blocks go-live |
|---|---|---|---|
| 1 | Every required template committed: sections, subjects, staff, students, guardians | yes | yes |
| 2 | Every reconciliation report passed, no open import issue in the Data Quality Center | yes | yes |
| 3 | No import inside a rollback window with unresolved conflicts | yes | yes |
| 4 | Every student has a guardian with parental access | yes | yes |
| 5 | Smart-defaults explicit rows confirmed; tax and numbering confirmed by a named person | yes | yes |
| 6 | Timetable published, or attendance set to daily mode | yes | yes |
| 7 | Two super-users certified | yes | yes |
| 8 | Email sending domain verified (SPF, DKIM, DMARC, master brief Section 38), or platform-domain sending accepted in writing | yes | no, warning |
| 9 | Fee plans and opening balances reconciled to the control total, when Finance is enabled | yes | yes |
| 10 | E-invoicing plug-in onboarded with the authority, when required in the country | yes | yes for Saudi Arabia and Jordan |
| 11 | Demo data absent | yes | yes |
| 12 | Welcome notifications held since import released in one scheduled batch at go-live | yes | no |
| 13 | Support tier and contact shown in the product | yes | no |
| 14 | A named Nibras contact on call for the day | no | yes |

When every blocking check passes, the tenant moves `Onboarding` to `Live` (WF-PLT-01, `TC-PLT-006`), the held invitations and welcome messages go out, and the first-week health check is scheduled with the customer success lead.

---

## Decisions in force

| Decision | Source | Default if unanswered | Impact if wrong |
|---|---|---|---|
| Two-stage staging: Documents validates, each target service re-validates and writes its own data | This document §2; master brief Section 7 | As stated | A single stage in Documents would write another service's database |
| The import rollback window is 7 days after `Committed`, and is not the 30-day tenant deletion cooling-off | WF-DATA-01 for the 7 days; master brief Section 32 and BR-PLT-003 for the 30 days | As stated | One number used for both strands a school that notices an import mistake in week two, or promises a tenant four weeks of import rollback that no before image survives |
| Dates are day first; a year from 1300 to 1500 is Hijri | This document §3.5 | As stated | A `mm/dd` guess corrupts every birth date with a day under 13 |
| Opening balances post as documents and roll back by credit note | This document §1.8, §5.3; master brief Section 36 | As stated | Editing posted documents breaks the audit and e-invoicing chain |
| Sensitive and level S columns never pass through import staging | This document §1.2, §1.4; Appendix J | As stated | Wellbeing or salary data sits in an unencrypted staging table |
| Welcome messages held until go-live | This document §2.2, §10.2 | Held | Families receive invitations before the school is ready |
| Two certified super-users before go-live | Master brief Section 39; this document §9 | As stated | A school goes live with no one able to fix a permission |

## Dependencies on other documents

| This document assumes | Stated in | Checked on |
|---|---|---|
| WF-DATA-01 states, window and tests | Appendix R | Every lint run |
| WF-PLT-01 and Saga 1 | Appendix R; `13-workflows-and-sagas.md`; `06-services/platform.md` §8 | Group F review |
| Smart defaults endpoints | `06-services/platform.md` §5.1, §5.6 | Group F review |
| Folding, transliteration, numeral parsing, Hijri conversion | `24-localization-and-calendars.md` §3, §4.3, §5 | Group F review |
| The field-code convention inside `_VALIDATION_FAILED` and the list envelope | `22-api-conventions-and-error-catalog.md` | Group F review |
| OneRoster 1.2 profile and plug-in certification | `23-integrations-and-public-api.md` §6.2, §8.3 | Group F review |
| Seeded administrator safeguards | `12-security-privacy-safety.md` §8 | Group F review |
| N-04 thresholds | Appendix N | Nightly load tier |
| The import machinery that builds §2: the import center, `import_jobs`, the Saga 9 handlers, the worker job groups and the import endpoints | `06-services/documents.md` | Group F review, and on every change to either document |

## Open points

| Question | Default | Owner | Impact if the default is wrong | L | I | Score | In the register |
|---|---|---|---|---|---|---|---|
| 1. Which adapters ship in the first release? | `excel-template`, `csv-mapped`, `oneroster-csv` and `noor-export`; the rest by demand from the first schools | Product owner | A school on an unsupported system falls back to `csv-mapped`, which costs a day of mapping | 3 | 2 | 6 | none |
| 2. Is the Saudi identity check digit applied as an error or a warning? | Error, with a documented override for a registrar holding the physical card | Documents owner, with the first Saudi school | A wrong algorithm blocks valid students | 2 | 2 | 4 | none |
| 3. Should historical results older than 3 years be imported at all? | Import what the school provides, locked and marked legacy | Product owner | Large histories slow the first import with no daily use | 2 | 2 | 4 | none |
| 4. Closed: who owns the staging design of §2, now that `06-services/documents.md` exists and specifies the import machinery in full | Split, not moved: the Documents sheet owns everything built inside Documents (the `import_jobs` state, the worker job groups, the endpoints, Saga 9's orchestrator); §2 here owns the two-stage design across services, the target services' Stage B, and the validation rules of §3, and cites the sheet rather than restating it. A difference between the two is a defect in whichever restates the other, found by `plan-consistency-checker` at the Group F review | Architect | A fact later restated in both documents could drift; the Documents sheet already cites this document for the toolkit as a whole (its decision "Document 26 cites this section rather than restating it"), so the sheet stays authoritative for what Documents builds and this document for the cross-service design | 1 | 2 | 2 | none |

> L and I are the likelihood that the default is wrong and the impact if it is, on the 1 to 5 scales of `18-risk-register.md` Section 1. Score is L x I. A point that scores 12 or more names its RISK identifier in document 18; below that, the identifier if one covers it, or `none`. Kit-lint rules R24 and R33 check all of it (ADR-0022). Point 4 is closed and re-scored 1 x 2: `06-services/documents.md` exists and specifies the import machinery itself, and the split in its Default cell gives each document its own half, so the round 2 score of 4 x 2 no longer applies.

## Review record

| Date | Reviewer | Verdict | Blocking items |
|---|---|---|---|
| 2026-09-22 | Group F review, round 1 (independent adversarial scorecard) | Blocked: the group scored below 4 on Completeness, Consistency, Feasibility, Risk honesty, Testability and Distinctiveness | The validation and adapter claims had no test-case identifier (Testability); `TC-PLT-001` meant provisioning here and other things in documents 27 and 32 (Consistency) |
| 2026-09-26 | Group F review, round 2 | Blocked: the group scored below 4 on Completeness, Consistency, Risk honesty and Testability | The test-id collision was closed. Still open: "table-driven tests per §3.4 to §3.6" and "golden-output test per adapter fixture" named no identifier (Testability); a dependency row still said `06-services/documents.md` was "not yet written", and open point 4 waited on it (Completeness) |
| 2026-09-26 | Round 3 remediation | Amended; awaiting the round 3 score | `TC-DATA-800` to `TC-DATA-803` defined in the new test table and cited from §3.4, §3.5, §3.6 and §6; `TC-PLAT-008` and `TC-DOC-327` cited with their owners; the dependency row names what the Documents sheet owns; open point 4 closed with the split of ownership |

## How this document is verified

| Claim | Proof | Where it runs |
|---|---|---|
| Templates parse and unknown columns are reported | `TC-DATA-001` | Every pull request touching the import job group |
| Errors are reported per row and column and nothing is written | `TC-DATA-002` (Appendix R); `TC-DATA-303` (document 10) | Every pull request touching the import job group |
| Dry run of 10,000 rows inside the budget | `TC-DATA-003`; N-04 | Nightly load tier |
| Commit is batched with an import identifier and a stale dry run is refused | `TC-DATA-004` | Every pull request |
| A failed batch reverses the import | `TC-DATA-005` | Every pull request |
| Rollback restores the prior state exactly and reports conflicts | `TC-DATA-006`, `TC-DATA-303` | Every pull request; N-04 checksum nightly |
| Duplicates are detected before commit | `TC-DOC-327` (Documents sheet), with Arabic spelling variants | Every pull request |
| National ID, Hijri and numeral rules | `TC-DATA-800` (§3.4), `TC-DATA-801` (§3.5) and `TC-DATA-802` (§3.6), table-driven under `ar-SA`, `en-US` and `de-DE`; `TC-PLAT-008` (document 33) for Arabic-Indic digits in a workbook produced on Windows | Every pull request touching `Nibras.BuildingBlocks.Localization` or the import job group |
| Adapters produce the reference rows | `TC-DATA-803`, the golden-output test every adapter of §6 runs against its fixture | Every pull request touching an adapter |
| The end-to-end import demo | `TC-DOC-001` (Appendix W item 9) | Demo pipeline |
| Provisioning to live | `TC-PLT-001` to `TC-PLT-006` (Appendix R, WF-PLT-01) | Every pull request touching Platform |
| This document agrees with the catalogs | kit-lint R01, R02, R05, R17, R19 and R30 (a comment on every column of a `CREATE TABLE`); `plan-consistency-checker` compares this document with Appendix R, `06-services/platform.md` and `06-services/documents.md` | kit-lint on every change under `docs/`; the comparison at the Group F review and on every change to any of them |

### Test cases

This document defines the validation and adapter tests below; the workflow transitions are Appendix R's `TC-DATA-001` to `TC-DATA-006` and are not restated. Each validation case is table-driven, runs once under each of `ar-SA`, `en-US` and `de-DE` with the clock pinned, and runs on the ubuntu and windows runners where `Nibras.BuildingBlocks.Localization` is built (document 33 part 4).

| Test case | What it proves | Covers |
|---|---|---|
| TC-DATA-800 | Given a fixture of identifiers for every row of §3.4, each with a valid form and a form with one digit changed, when the country's `NationalIdValidator` checks them under each of the three cultures, then every valid form passes and every changed form fails with `invalidNationalId`, an Emirates ID whose embedded birth year is two years from `date_of_birth` passes with the `idBirthYearMismatch` warning, the same identifier written in Arabic-Indic digits with spaces and hyphens gives the same result as its Western form, two rows sharing a folded identifier give `duplicateNationalId` with `related_row_no`, and every identifier in the error report shows only its last four characters | REQ-DOC-011 |
| TC-DATA-801 | Given one cell per row of §3.5, when the date rules parse them under each of the three cultures, then `1432-05-10` becomes 2011-04-14 and the preview shows both calendars, `03/04/2012` is 3 April 2012 in every culture, a year suffixed `هـ` is converted as Hijri, a Hijri day 30 in a 29-day month is `invalidHijriDate` with the two suggestions and is never shifted, an unparseable value is `invalidDate`, and the source value is kept in `raw` | REQ-DOC-011 |
| TC-DATA-802 | Given amount cells written in Western, Arabic-Indic and extended digits with the Arabic decimal and thousands separators, when §3.6 parses them under each of the three cultures, then each yields the same decimal value, a value mixing two digit systems is `mixedNumerals`, `1,5` is `invalidNumber` and never 1.5, and an amount with more decimals than the currency's minor units is rounded with the tenant's rounding mode and reported as `amountRounded` with both values | REQ-DOC-011, REQ-PLAT-020 |
| TC-DATA-803 | Given the fixture file and committed golden output of each adapter in §6, when the adapter runs under `ar-SA` and `en-US` on the ubuntu and windows runners, then its template rows and mapping report equal the golden output byte for byte, and for the entities it shares with `excel-template` its rows equal what `excel-template` produces from the same data; an adapter with no fixture or no golden output fails the test | REQ-DOC-016 |
