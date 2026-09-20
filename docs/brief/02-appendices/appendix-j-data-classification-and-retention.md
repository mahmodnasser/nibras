# Appendix J. Data Classification and Retention

> Part of `docs/brief/02-appendices/`. Normative. Read with the master brief and the reference architecture.

Master brief Section 20 requires "a data inventory with classification for every field, and a retention schedule per data type with automated deletion or anonymization". Master brief Section 32 sets the retention clocks. This appendix joins the two at field-group level so that a developer can answer, before writing a migration, four questions: what class is this, does it get a column key, may it be cached, and when does it die.

---

## J.1 The five levels

| Level | Meaning | Storage rule | Cache rule | Access log |
|---|---|---|---|---|
| **Public** | Safe outside the tenant | Plain | Cacheable, shared key allowed | no |
| **Internal** | Safe for any signed-in member of the tenant | Plain | Cacheable with the tenant in the key | no |
| **Confidential** | Limited to roles with the permission and the scope | Plain, protected by row-level security | Cacheable only under a per-user key with a short lifetime | On export only |
| **Sensitive** | Special-category data about a child or an employee | Column-encrypted at rest | Never cached | Every read |
| **S — isolation level** | Wellbeing: clinic, counseling, safeguarding, plans | Column-encrypted, separate database `nibras_wellbeing`, separate credentials | Never cached, never projected | Every read, plus an alert on break-glass |

Level S is not "sensitive with more paperwork". It is a **separate blast radius**: the Wellbeing service owns it, no other service holds a copy, Reporting receives counts and never rows, and the Student 360 timeline shows a permitted viewer only that an entry exists unless they hold `wellbeing.*` for that student.

---

## J.2 Field-level classification

Retention cites master brief Section 32. "In Student 360" means the field may appear on the Student 360 page defined in master brief Section 12 item 2, subject to the viewer's permission.

### Student identity, contacts, and guardians

| Entity | Field group | Class | Encrypted at rest | In Student 360 | Cacheable | Retention (Section 32) | Access logged |
|---|---|---|---|---|---|---|---|
| Student | Name, preferred name, section, photo, student number | Internal | no | yes | yes, tenant key | Academic record: 10 years after leaving, then anonymize | no |
| Student | Date of birth, gender, nationality, place of birth | Confidential | no | yes | no | 10 years after leaving | no |
| Student | National identity number, passport number, residence permit | Sensitive | yes | masked, last 4 only | never | 10 years after leaving, then destroy the identifier before archiving | every read |
| Student | Home address, geolocation of residence | Confidential | no | no | no | 10 years after leaving | no |
| Student | Photo and media consent flag | Internal | no | yes | yes, tenant key | Life of the record plus consent history | no |
| Guardian | Name, relationship, preferred language, contact order | Internal | no | yes | yes, tenant key | 10 years after the last linked child leaves | no |
| Guardian | Mobile number, email, workplace | Confidential | no | yes | no | 10 years after the last linked child leaves | no |
| Guardian | National identity number, sponsor identity | Sensitive | yes | masked | never | 10 years, identifier destroyed at archive | every read |
| Custody | Legal custody status, restriction flags | Sensitive | yes | flag only, no text | never | Until leaving plus 3 years, then delete | every read |
| Custody | Court order text, notes, uploaded order | Sensitive | yes | no | never | Until leaving plus 3 years | every read |
| Pickup authorization | Authorized person, photo, PIN hash, validity window | Confidential | PIN hash only | yes | yes, 60-second per-tenant key | Until revoked plus 1 year | On issue and on use |

### Health, care, and safeguarding

| Entity | Field group | Class | Encrypted at rest | In Student 360 | Cacheable | Retention (Section 32) | Access logged |
|---|---|---|---|---|---|---|---|
| Medical summary | Allergy alert (substance, severity, action) | Sensitive, surfaced | yes | yes, as an alert badge | never | Until leaving plus 7 years | every read |
| Medical summary | Chronic conditions, medications held at school, blood group | Sensitive | yes | no | never | Until leaving plus 7 years | every read |
| Clinic visit | Reason, observation, treatment, sent-home decision | **S** | yes | existence only | never | Until leaving plus 7 years, then delete | every read |
| Medication administration | Dose, time, administering nurse, guardian authorization | **S** | yes | existence only | never | Until leaving plus 7 years | every read |
| Counseling case | Referral reason, session notes, risk assessment | **S** | yes | no | never | Until leaving plus 7 years, then delete | every read |
| Safeguarding concern | Concern text, reporter, chronology, external referral | **S** | yes | no | never | Local safeguarding retention rule overrides the default; otherwise until leaving plus 7 years | every read, break-glass alerts the principal |
| IEP and accommodation plan | Diagnosis, goals, accommodations, review outcomes | **S** | yes | accommodation flag only | never | Until leaving plus 7 years | every read |
| Intervention | Owner, plan steps, review dates, outcome | Confidential | no | yes | no | Until leaving plus 3 years | On export |

### Learning, behavior, and attendance

| Entity | Field group | Class | Encrypted at rest | In Student 360 | Cacheable | Retention (Section 32) | Access logged |
|---|---|---|---|---|---|---|---|
| Behavior incident | Category, date, location, points | Confidential | no | yes | no | Until leaving plus 3 years, then anonymize | On export |
| Behavior incident | Narrative, witnesses, sanction rationale | Confidential, restricted | no | permitted roles only | never | Until leaving plus 3 years | every read when `behavior.incidents.view-restricted` is used |
| Recognition | Badge, house points, certificate | Internal | no | yes | yes, tenant key | Life of the portfolio, exportable when the student leaves | no |
| Mark | Raw score, scheme version, entered-by, moderation state | Confidential | no | yes | yes, per-user key, 60 seconds | 10 years after leaving | On export and on post-lock change |
| Report card | Grades, comments, attendance summary, rendered PDF | Confidential | no | yes | rendered PDF only, signed URL, not the body | 10 years after leaving, archive read-only | On generation and on export |
| Transcript | Cumulative result history, verification code | Confidential | no | yes | verification page only | 10 years after leaving | On generation |
| Attendance | Status per session, minutes late, marked-by | Internal | no | yes | yes, tenant key, invalidated by `attendance.attendance.marked.v1` | 7 years, delete by partition | no |
| Attendance | Excuse reason and uploaded evidence | Confidential | no | yes, reason only | no | 7 years, delete by partition | On export |
| Attendance | Medical excuse detail | Sensitive | yes | no | never | 7 years | every read |

### Money, staff, and communication

| Entity | Field group | Class | Encrypted at rest | In Student 360 | Cacheable | Retention (Section 32) | Access logged |
|---|---|---|---|---|---|---|---|
| Invoice | Number, series, line items, totals, currency (SAR, AED, JOD) | Confidential | no | balance only | yes, per-payer key | 10 years, archive read-only | On export |
| Payment | Amount, method, receipt number, cashier, day-close batch | Confidential | no | balance only | no | 10 years, archive read-only | On export |
| Payment | Gateway reference, card scheme, last 4 digits, IBAN of the payer | Sensitive | yes | no | never | 10 years; full card data is never stored, only the gateway token | every read |
| Discount and scholarship | Percentage, sponsor, approval chain | Confidential | no | no | no | 10 years | On approval and on export |
| Staff contract | Job title, grade, start and end dates | Confidential | no | not applicable | yes, tenant key | 10 years after leaving | no |
| Staff contract | Salary, allowances, bank account, payslip lines | Sensitive | yes | not applicable | never | 10 years after leaving | every read, `hr.payroll.view-salary` is high risk |
| Staff document | Licence, visa, certificate, expiry | Confidential | no | not applicable | expiry date only | 10 years after leaving | On export |
| Message | Thread participants, subject, sent time, read receipt | Confidential | no | existence only | no | 2 years, then delete | On oversight read |
| Message | Body and attachments | Confidential | no | no | never | 2 years; a body flagged for safeguarding follows the wellbeing rule | every oversight read, `communication.messages.oversee-messages` is high risk |
| Announcement | Title, body, audience, acknowledgment state | Internal | no | yes | yes, audience key | 2 years | no |
| Uploaded document | File name, type, size, owner, scan verdict | Internal | no | yes | metadata only | Follows the owning record | no |
| Uploaded document | File bytes | Class of the owning record | at rest in object storage, always | through a signed URL of 5 minutes | never | Follows the owning record | On download of a sensitive owner |

### Safety, transport, library, and the school day

| Entity | Field group | Class | Encrypted at rest | In Student 360 | Cacheable | Retention (Section 32) | Access logged |
|---|---|---|---|---|---|---|---|
| Gate pass | QR or PIN material, validity window, issuer | Confidential | hashed | yes, as an event | never | Until leaving plus 1 year with the attendance partitions | On issue and on use |
| Visitor | Name, identity document reference, host, photo | Confidential | identity reference encrypted | no | no | 2 years, then delete | On watchlist match |
| Emergency roll call | Location, headcount, unaccounted names, reunification record | Confidential | no | yes, as an event | no | 7 years with the attendance partitions | On export |
| Timetable | Sections, teachers, rooms, periods | Internal | no | yes | yes, tenant key, invalidated by `scheduling.timetable.published.v1` | Life of the academic year plus 10 years | no |
| Staff attendance | Status per day, minutes late, marked-by | Confidential | no | not applicable | no | 7 years, delete by partition | On export |
| Transport subscription | Route, stop, seat, boarding events | Confidential | no | yes | yes, route key for the roster | Until the subscription ends plus 2 years | no |
| Vehicle location | Coordinates, speed, timestamps | Internal | no | never; location is tracked for vehicles, never for children | yes, 30-second key | 90 days, then delete | no |
| Library loan | Item, borrower, dates, fine in JOD, AED or SAR | Internal | no | yes | no | Until leaving plus 2 years | no |
| Activity and trip | Participants, consent state, medical flags carried for the day | Confidential | the carried medical flag is read live and never copied | yes | no | Until leaving plus 3 years | On the medical flag read |
| Kindergarten daily sheet | Meals, naps, mood, activities, note to guardians | Confidential | no | yes | no | Until leaving plus 3 years | no |

### Platform, identity, and evidence

| Entity | Field group | Class | Encrypted at rest | In Student 360 | Cacheable | Retention (Section 32) | Access logged |
|---|---|---|---|---|---|---|---|
| Audit entry | Actor, tenant, action, entity, before and after values, hash chain | Confidential | values encrypted when the source field is sensitive | no | never | 7 years, detach partition to cold storage | Export is high risk and reported to the principal |
| Login history | Time, device, address, result | Confidential | no | no | no | 7 years with the audit partitions | no |
| Notification delivery log | Channel, template, recipient reference, status | Internal | no | no | no | 90 days, then delete | no |
| Consent record | Consent text version, decision, actor, timestamp | Confidential | no | yes, as the consent panel | yes, per-student key | Life of the record plus 7 years | On change |
| Credential | Password hash, salt, algorithm parameters | Sensitive | hash only, never reversible | no | never | Until replaced; history of the last 5 kept for reuse checks | On change |
| Credential | Two-factor secret, recovery codes, passkey public key | Sensitive | yes | no | never | Until removed | On change |
| Token | Refresh token handle, device binding, permission version | Sensitive | hashed handle | no | Redis only, with the token lifetime as the expiry | Access token 15 minutes, refresh token 30 days or until rotated | On new-device use |
| Token | Invitation link, gate-pass QR, signed download URL | Confidential | hashed | no | never | Invitation 7 days, gate pass its validity window, signed URL 5 minutes | On use |
| API key | Key hash, scopes, owner | Sensitive | hash only | no | never | Until revoked plus 1 year of usage history | On use |
| Tenant | Plan, limits, health score, usage counters | Internal | no | no | yes, tenant key | Life of the tenant; deleted tenant purged after the 30-day cooling-off | no |

---

## J.3 Where each classified entity lives

A classification is only enforceable when one service owns the field. Service names are from Appendix L, and no other service holds a copy of a sensitive or level-S field.

| Owning service | Database | Sensitive field groups it owns | Copies allowed elsewhere |
|---|---|---|---|
| School | `nibras_school` | Student and guardian identity numbers, custody, medical summary | Name, section and student number only, as a reference copy reconciled nightly |
| Wellbeing | `nibras_wellbeing` | Every level-S group: clinic, medication, counseling, safeguarding, education plans | None. Reporting receives counts, never rows |
| Identity | `nibras_identity` | Password hashes, second-factor secrets, tokens, API keys | None |
| Finance | `nibras_finance` | Gateway references, payer bank details | Balance figures only, in Reporting projections |
| Hr | `nibras_hr` | Salary, allowances, bank accounts, payslip lines | Headcount and absence aggregates only |
| Communication | `nibras_communication` | Message bodies and attachments | None; Notification holds a template reference, never the body |
| Documents | `nibras_documents` | File bytes in object storage, with the owner's class | None |
| Audit | `nibras_audit` | Before and after values of sensitive changes | None; the audit log is the copy of record |
| Attendance | `nibras_attendance` | Medical excuse detail, gate-pass material, visitor identity references | Attendance status only |

---

## J.4 Never cached, under any key

These never enter Redis, never enter a `HybridCache` local tier, never appear in a Reporting projection, and never appear in a log line or an exception message.

| Never cached | Why |
|---|---|
| Every field at level **S** | Separate blast radius; a cache is a copy outside the Wellbeing database |
| National identity, passport, residence permit, sponsor identity | Re-identification in a shared store |
| Medical summary detail, medical excuse detail | Special-category child data |
| Custody status text and court orders | Safety of a child, not only privacy |
| Message and announcement bodies with a safeguarding flag | Follows the wellbeing rule |
| Salary, allowances, bank accounts, payslip lines | Employment data with a separate access-review cycle |
| Gateway references, IBANs, card data of any kind | Payment scope; only the gateway token is ever held |
| Password hashes, two-factor secrets, recovery codes, API key material | Credential material |
| Audit before-and-after values | The audit log is the evidence, not a read model |

The allergy alert is the single deliberate exception in spirit and not in mechanism: it is surfaced prominently to permitted staff, and it is still read live from Wellbeing on every view, never cached, with every read logged. Convenience does not earn a cache entry here.

---

## J.5 Rules

1. **A field with no classification cannot ship.** Every new column arrives with its level, its encryption decision, its cache decision and its retention row in the same pull request as the migration. The architecture test fails a migration whose entity configuration carries no classification attribute, and the reviewer treats a missing row here as a defect, not as documentation debt.
2. **Classification travels with the copy.** A projection, an export, a report and a mobile cache inherit the level of the source field. A confidential field does not become internal by being aggregated unless the aggregate is at least 10 students wide.
3. **Retention is a job, not a policy.** Every row in J.2 maps to a scheduled deletion or anonymization job that reports to the Data Quality Center. Attendance, notifications, messages and audit detach partitions rather than delete rows (Section 32).
4. **A legal hold suspends deletion and is itself logged.** The hold names who set it, when, and which subject. The Wellbeing hold additionally alerts the safeguarding officer.
5. **Deletion is immediate in the live system and backups age out within 35 days** (Section 32). The product says exactly that to the school and to the guardian, and never more.
6. **The guardian can see the clock.** Master brief Section 12.1 item 31 requires that a parent sees which roles read their child's sensitive records, when, and how long each category is kept. The retention column in J.2 is the source for that panel, so a change here changes what families are told.
7. **Encryption keys are per tenant and rotatable.** A sensitive column uses a tenant data key wrapped by a service key; rotation re-wraps without rewriting rows, and a tenant export uses a key the tenant can be handed.
8. **Access logging is a write, not a best effort.** A read of a sensitive or level-S field writes an audit entry in the same transaction boundary as the read handler; if the audit write fails, the read fails. Master brief Section 21 makes audit records append-only, and this is what makes the guardian transparency panel true rather than approximate.
