# 27. Compliance and Legal

> Plan document for the Nibras platform. Group F. It refines master brief Section 33 (the compliance map), Section 32 (the retention schedule), Section 20 (privacy and child safety), Section 34 (residency), Section 36 (the promise not to hold data hostage), Section 38 (safeguarding and law-enforcement requests) and the PRV area of Appendix L.3; it does not re-derive them. Consent, data-subject workflows, retention jobs, legal hold, the sub-processor template, the impact assessment outline and the penetration test are owned by `12-security-privacy-safety.md` §10 and §12, and the retention jobs by `10-data-architecture.md` §8; this document cites them and adds only what the legal and contractual side needs. Where this document and the brief disagree, an ADR records the deviation.

**Group** F · **Requirement areas covered** PRV (with `12-security-privacy-safety.md`: REQ-PRV-001 to REQ-PRV-023), PLT (legal documents and residency: REQ-PLT-030, REQ-PLT-033), INT (the e-invoicing plug-in contracts only), UX (the accessibility conformance statement) · **Last updated** 2026-09-22 by the platform plan

## Purpose

This document lets a school's data protection officer, a procurement lawyer, a regulator's questionnaire or an auditor get a precise answer to "what does Nibras do about X, where is it built, and how is it proven" without reading the engineering plan. It turns the Section 33 map into controls with a location and a test, joins the Section 32 schedule to its enforcing jobs, states in one table each what consent, subject rights, sub-processors and the impact assessment look like to a school, sets the legal documents and their acceptance record, writes the child-safety and data-return commitments as contract clauses, fixes residency, defines the e-invoicing plug-in obligations for Saudi Arabia and Jordan, sets the process for law-enforcement and safeguarding requests, and gives the checklist for entering a fourth country. The readers are the product owner, the data protection lead, the customer's legal reviewer, the Finance and Platform service owners, and whoever prepares a SOC 2 or ISO 27001 evidence pack.

## Scope

| In scope | Out of scope | Where the out-of-scope item lives |
|---|---|---|
| The compliance map with controls, locations and tests | Security controls and the threat model themselves | `12-security-privacy-safety.md` §1, §2 |
| Retention schedule joined to jobs, with the hold reference | Job implementation, partitioning, backups | `10-data-architecture.md` §5, §8, §9 |
| One-table summaries of consent, subject rights, sub-processors, impact assessment | The workflows and templates | `12-security-privacy-safety.md` §10.1, §10.2, §10.5, §10.6; WF-PRV-01 |
| Accessibility conformance statement outline | The WCAG criteria and how the design system meets them | `14-design-system-and-ux.md` §11 |
| Terms, privacy policy, data processing agreement, acceptance | Legal document storage and endpoints | `06-services/platform.md` §4.9, §5.10 |
| Child-safety and data-return commitments as clauses | Child-safety controls in the product | `12-security-privacy-safety.md` §7 |
| Residency | Deployment topology per region | `15-deployment-and-operations.md` |
| ZATCA and JoFotara obligations on the plug-in | The plug-in interface and certification | `23-integrations-and-public-api.md` §8.2, §8.3 |
| Law-enforcement and safeguarding request process | Safeguarding workflow inside the school | WF-WEL-04; `12-security-privacy-safety.md` §10.4 |
| Penetration test and disclosure | Scope and process | `12-security-privacy-safety.md` §12 |

---

## Content

### 1. The compliance map

Master brief Section 33 states each framework's status. Each row below names the concrete controls behind "what the product provides", where each lives, and the proof. A control with no test is a claim, and a claim does not belong in a customer answer.

#### 1.1 Status, quoted from Section 33

| Framework | Status |
|---|---|
| GDPR-style baseline | Designed to |
| Saudi PDPL | First target |
| UAE PDPL | First target |
| Jordan PDPL | First target |
| Egypt data protection law | Planned |
| COPPA, FERPA | Adaptable, not claimed |
| SOC 2 Type II | Post-launch roadmap |
| ISO 27001 | Post-launch roadmap |
| WCAG 2.2 AA | Required now |

#### 1.2 Controls per framework

| Framework | Control | Where it lives (service and document) | Proof |
|---|---|---|---|
| GDPR-style baseline | Lawful basis and consent records, versioned texts | Platform (texts), School (decisions); `12-security-privacy-safety.md` §10.1 | `TC-PRV-061`, `TC-PRV-062`, REQ-PRV-013 |
| GDPR-style baseline | Access, correction, export, erasure | Platform WF-PRV-01, owning services; `12-security-privacy-safety.md` §10.2 | `TC-PRV-001` to `TC-PRV-006`, `TC-PRV-065` |
| GDPR-style baseline | Retention schedule enforced by jobs | Every data-owning service; `10-data-architecture.md` §8; §2 here | `TC-PRV-068` to `TC-PRV-075` |
| GDPR-style baseline | Records of processing | Platform impact assessment pre-fill from the Appendix J inventory | `TC-PRV-080` |
| GDPR-style baseline | Sub-processor list and refusal by feature | Platform `GET /api/v1/platform/sub-processors`; `12-security-privacy-safety.md` §10.5 | `TC-PRV-066`, `TC-PRV-079` |
| GDPR-style baseline | Completable impact assessment | Platform `DpiaRecord`; `12-security-privacy-safety.md` §10.6 | `TC-PRV-080`, REQ-PRV-017 |
| GDPR-style baseline | Data inventory with classification per field | Every service's entity configuration; Appendix J | REQ-PRV-001 architecture test |
| GDPR-style baseline | Breach readiness: tamper-evident audit, access logs on Sensitive reads | Audit; `12-security-privacy-safety.md` §6 | `TC-AUD-101` onwards |
| Saudi PDPL | Residency in region | Platform `region_code` immutable; §8 here | `TC-PLT-101`; REQ-PLT-030 |
| Saudi PDPL | Arabic privacy notices, consent versions | Platform `LegalDocument` per country; §5 here | `TC-PRV-063` (document 12) |
| Saudi PDPL | Transfer outside the Kingdom only with consent and disclosure | Rung 4 AI and any foreign sub-processor behind consent; `25-ai-and-assist-ladder.md` §3.3 | `TC-SEC-323`, `TC-PRV-079` |
| Saudi PDPL | ZATCA e-invoicing plug-in interface | Finance with the `IEInvoicingProvider` plug-in; §9 here | Plug-in conformance suite; `TC-INT-002` |
| UAE PDPL | As Saudi Arabia, with the UAE e-invoicing and reporting plug-ins when the programme is live | Finance, Reporting; §9 here | Plug-in conformance suite |
| Jordan PDPL | As Saudi Arabia, with the JoFotara plug-in interface | Finance; §9 here | Plug-in conformance suite |
| Egypt | Same mechanisms; country plug-in not built | §12 here is the path | None until built |
| COPPA, FERPA | Guardian consent; no advertising, no sale, no behavioural tracking of students | Platform, mobile pipeline; `12-security-privacy-safety.md` §10.1 | `TC-PRV-060`, `TC-PRV-064`, REQ-PRV-016 |
| COPPA, FERPA | Directory-information controls | School; no student search by outsiders | `TC-PRV-022` |
| SOC 2 Type II, ISO 27001 | Access reviews, change management, audit trail, incident process, backup drills | Identity, platform pipeline, Audit, operations; §11 here | Evidence map in §11 |
| WCAG 2.2 AA | Automated and manual verification per release; published statement | Web and mobile; `14-design-system-and-ux.md` §11; §4 here | axe-core in CI, the manual pass per release |

### 2. Retention schedule and the enforcing job

The rows, their default retention, what follows and the legal-hold column are quoted from master brief Section 32. The job name and the services that run it are quoted from `10-data-architecture.md` §8, which owns them; this document names no job of its own and invents no schedule. Tests are from `12-security-privacy-safety.md` §10.3, whose job names are the older ones and are aligned at its next revision (open point 1). Legal hold behaviour for every row is `12-security-privacy-safety.md` §10.4.

| Data | Default retention | Then | Legal hold | Enforcing job and service | Test |
|---|---|---|---|---|---|
| Student academic record (enrollment, results, transcripts) | 10 years after leaving | Archive read-only, then anonymize | Suspends deletion | `LeaverRetentionJob`, School with Academics, Assessment, Requests and Finance for their year partitions; identifiers destroyed first | `TC-PRV-069`, `TC-PRV-075` |
| Attendance records | 7 years | Delete by partition | Suspends deletion | `PartitionMaintenanceJob`, Attendance | `TC-PRV-068` (document 12) |
| Behavior incidents | Until leaving, plus 3 years | Anonymize | Suspends deletion | `LeaverRetentionJob`, Behavior | `TC-PRV-071` (document 12) |
| Wellbeing records (clinic, counseling, safeguarding) | Per country law; default until leaving plus 7 years | Delete; safeguarding concerns follow the local safeguarding retention rule instead | Suspends deletion, and the hold is itself logged | `WellbeingRetentionJob`, Wellbeing, under Wellbeing's own credentials | `TC-PRV-029`, `TC-PRV-070` |
| Financial documents (invoices, payments, credit notes) | 10 years | Archive read-only | Suspends deletion | `FinanceArchiveJob`, Finance | `TC-PRV-072` (document 12) |
| Messages and announcements | 2 years | Delete; content flagged for safeguarding follows the wellbeing rule | Suspends deletion | `PartitionMaintenanceJob`, Communication; flagged rows moved out first | `TC-PRV-068`, `TC-PRV-029` |
| Notification delivery log | 90 days | Delete | no | `PartitionMaintenanceJob`, Notification | `TC-PRV-073` (document 12) |
| Audit entries | 7 years | Detach partition to cold storage | Suspends deletion | `PartitionMaintenanceJob`, Audit; publishes `audit.retention.partition-detached.v1` | `TC-PRV-068` (document 12) |
| Application logs | 30 days | Delete | no | Log store retention policy, observability stack | `TC-PRV-073` (document 12) |
| Metrics | 13 months downsampled | Delete | no | Metrics store retention, observability stack | `TC-PRV-073` (document 12) |
| Traces | 7 days | Delete | no | Trace store retention, observability stack | `TC-PRV-073` (document 12) |
| Backups | 35 days point-in-time plus 12 monthly | Expire | A hold pins the relevant backup set | pgBackRest expiry, operations runbook | `TC-PRV-074` (document 12) |
| Deleted tenant | 30-day cooling-off, export available throughout | Purge, then issue a certificate of deletion | A hold blocks the purge and notifies both parties | `TenantPurgeJob` driven by WF-PLT-03 | `TC-PLT-026`, `TC-PRV-901` |

| What this document adds | Detail |
|---|---|
| Country and contract overrides | A tenant's schedule may lengthen a period for its country or contract through `PATCH /api/v1/platform/retention/schedule` with the basis recorded; shortening below a country minimum is refused with `PLATFORM_SETTING_LOCKED_BY_POLICY` (REQ-PRV-002) |
| Country minimums in force | The country plug-in carries a minimum per row; until a country's counsel sets one, the Section 32 default is the minimum |
| The published position on backups | The customer-facing text says exactly: deletion is immediate in the live system and backup copies age out within 35 days, except where a hold pins them |
| Evidence | The monthly job reports feed the Data Quality Center; the privacy dashboard (REQ-PRV-021) shows the last run per row, which is the evidence an auditor asks for |

### 3. Privacy operations, summarised

Each table states what a school or a regulator sees, cites the owning section of `12-security-privacy-safety.md`, and names the one thing this document adds.

#### 3.1 Consent

| Aspect | Position | Owner in `12-security-privacy-safety.md` |
|---|---|---|
| Versioned texts | A consent records the text version, decision, actor and time; a new version re-asks | §10.1 |
| Categories | Photo and media; data sharing with a named sub-processor; external AI provider (rung 4); keyword-flagging disclosure; medication authorization | §10.1 |
| Who consents | A guardian with parental access and no custody restriction; a student above the tenant's configured age for their own settings | §10.1 |
| Withdrawal | Effective within one minute across projections, galleries, indexes and pending exports | §10.1 |
| **This document adds** | **Consent texts are legal documents of kind `consent-text` (§5), drafted per country and reviewed by that country's counsel before publication; a material change to a consent text is a new version and re-asks, a correction of wording is a revision that does not** | |

#### 3.2 Data-subject rights

| Right | Service level | Owner in `12-security-privacy-safety.md` |
|---|---|---|
| Access and copy | Acknowledged within 5 working days, answered within 30 calendar days, through the export workflow (WF-PRV-01) | §10.2 |
| Correction | A request type with its SLA; audited before and after | §10.2 |
| Erasure | 30 calendar days; safeguarding records retained with the reason stated | §10.2 |
| Whole-tenant export and deletion | WF-PLT-03, 30-day cooling-off, certificate of deletion | §10.2 |
| Objection to a sub-processor | Immediate, by disabling the feature | §10.2 |
| **This document adds** | **Role allocation: the school is the controller and answers the subject; Nibras is the processor and assists within 5 working days of the school's request, as clause 7 of the data processing agreement (§5.2). A request that reaches Nibras directly is forwarded to the school within 2 working days and never answered by Nibras itself** | |

#### 3.3 Sub-processors

| Aspect | Position | Owner in `12-security-privacy-safety.md` |
|---|---|---|
| Published list per deployment | Processor, purpose, data by Appendix J class, region, adapter, consent requirement, contract basis, last review | §10.5 |
| Refusal | A school refuses a sub-processor by disabling the feature that uses it | §10.5, master brief Section 33 |
| Consistency | The published list equals the set of enabled adapters | §10.5 (`TC-PRV-079`) |
| **This document adds** | **Change notice: a new sub-processor is announced to every affected tenant 30 days before first use, through the in-product announcement and email to the owner; the school may object in that window by disabling the feature, and the objection is recorded on the tenant. A sub-processor outside the tenant's region is never added to a feature a school has already enabled without that notice** | |

#### 3.4 Data protection impact assessment

| Aspect | Position | Owner in `12-security-privacy-safety.md` |
|---|---|---|
| Template | Eight sections; product pre-fills processing, retention, risks, measures, sub-processors, rights | §10.6 |
| Completion and export | By the school, from the Platform console, as a bilingual PDF | §10.6, REQ-PRV-017 |
| Accuracy | Reflects the modules and sub-processors enabled on the day of export | §10.6 (`TC-PRV-080`) |
| **This document adds** | **Triggers for a new assessment: enabling Wellbeing, enabling any AI rung above 1, enabling a sub-processor outside the region, or turning on keyword flagging. The console prompts the school on each trigger and records whether the assessment was updated; Nibras keeps its own processor-side assessment per deployment, reviewed yearly and after each trigger at platform level** | |

### 4. Accessibility conformance statement

Section 33 requires a statement published per release. The criteria and how they are met are `14-design-system-and-ux.md` §11; this is the statement's outline, generated per release from the CI results and the manual pass, and published in both languages on the status page and inside the product's help.

| Section of the statement | Content | Source |
|---|---|---|
| 1 Identification | Product, release version, date, platforms covered: web, Android application, iOS application, progressive web application | Release record |
| 2 Standard and level | WCAG 2.2 Level AA; for mobile, WCAG 2.2 AA applied through platform semantics plus the Android and iOS accessibility guidelines | Master brief Section 33 |
| 3 Conformance status per platform | Fully conforms, partially conforms, or does not conform, per platform | Result of the rows below |
| 4 Evaluation method | Automated: axe-core over every screen in CI, contrast job, Flutter semantics tests. Manual: keyboard walkthrough, NVDA and Narrator on Windows, VoiceOver on macOS and iOS, TalkBack on Android, in Arabic and English, in both directions | `14-design-system-and-ux.md` §11; `16-test-strategy.md` §8 |
| 5 Criteria table | Each WCAG 2.2 AA criterion: supports, partially supports, does not support, not applicable, with a remark | Generated from the §11 criterion table and the release's results |
| 6 Known limitations | Each gap with the affected screen, the user impact, the workaround, and the release in which it is fixed | Open accessibility defects at release |
| 7 Language and direction | Statement that every screen is verified in Arabic right-to-left and English left-to-right | Four-way snapshots |
| 8 Tenant customisation | Statement that tenant palettes are checked against the contrast matrix before publication, and that uploaded documents and images supplied by the school are the school's responsibility | Branding publish check |
| 9 Feedback and contact | A channel for reporting a barrier, the response target (5 working days), and the escalation path | Support desk |
| 10 Approval | The accessibility lead's name and the date | Release sign-off |

A release with a new "does not support" row on a Tier 1 screen is not published without the product owner's recorded acceptance, because the statement would otherwise fall below the Section 33 status of "required now".

### 5. Terms, privacy policy and data processing agreement

#### 5.1 The documents

| Kind (`LegalDocument.kind`, `06-services/platform.md` §4.9) | Party accepting | When accepted | Per country |
|---|---|---|---|
| `terms` | Tenant owner on behalf of the school | At signup, before WF-PLT-01 leaves `Requested`; again on a material change | Yes: governing law and courts per country |
| `dpa` | Tenant owner | With the terms; again on a material change | Yes: the country's processor obligations |
| `privacy-policy` | Every user | At first sign-in; again on a material change | Yes |
| `privacy-notice` | Guardians and students, in the product | At first sign-in and on change; always reachable from the profile | Yes, per country plug-in |
| `consent-text` | Guardians, per category | When the category first applies; again on a new version | Yes |

#### 5.2 The data processing agreement, by clause

| # | Clause | Content |
|---|---|---|
| 1 | Roles | The school is the controller; Nibras is the processor; the school's instructions are the product configuration and written requests |
| 2 | Subject matter | The data categories of Appendix J for the modules enabled; the data subjects of `12-security-privacy-safety.md` §10.6 section 3 |
| 3 | Duration | The subscription plus the 30-day cooling-off after a deletion request, then the certificate of deletion |
| 4 | Confidentiality | Staff access under least privilege, second factor, logged; no engineer database access to tenant data outside break-glass (`12-security-privacy-safety.md` §3.5) |
| 5 | Security measures | The ASVS level, encryption, isolation level S, audit chain, backups and drills, as the annex; changes may only strengthen them |
| 6 | Sub-processors | General authorisation for the published list; 30-day notice of additions with the right to object by disabling the feature (§3.3) |
| 7 | Assistance | With subject requests within 5 working days of the school's request; with impact assessments through the product template |
| 8 | Breach notification | Notice to the school's named contact without undue delay and no later than 48 hours after Nibras confirms a breach affecting the tenant, with what is known, and updates as facts are established |
| 9 | Residency and transfers | §8: data and backups stay in the tenant's region; any transfer only through a feature the school enabled with consent and disclosure |
| 10 | Return and deletion | §7 clause verbatim |
| 11 | Audit | The SOC 2 or ISO evidence pack once available (§11); before then the penetration test summary and this document's §1 with test results; an on-site audit on reasonable notice for Enterprise and on-premises plans |
| 12 | Law-enforcement requests | §10: the school is told unless the law forbids it, and only the minimum is produced |
| 13 | Child safety | §6 clauses by reference |

#### 5.3 Recorded acceptance

| Rule | Mechanism | Test |
|---|---|---|
| Every acceptance is a row | `LegalAcceptance`: document, version, user or tenant, time, hashed address (`06-services/platform.md` §4.9) through `POST /api/v1/platform/legal-acceptances` | `TC-PLT-113` (Platform sheet) |
| Publishing a version asks once | `POST /api/v1/platform/legal-documents/{documentId}/publish`; the next sign-in shows the new version in the user's language with a summary of changes | `TC-PLT-113` (Platform sheet); `TC-PRV-800` |
| Material change notice | Terms and DPA changes are announced 30 days before they take effect; the owner may export and leave in that window under §7 | Review step: at every publish of a `terms` or `dpa` version, the data protection lead compares the Platform announcement record's date with the version's effective date, refuses the publish when fewer than 30 days separate them, and records the check in the release notes |
| Arabic and English are equal | Both bodies are published together; neither is a translation of record unless the country's law requires one, in which case the country plug-in names it | `TC-PRV-063` (document 12) |
| Nothing is used before acceptance | A user without acceptance of the current privacy policy reaches only the acceptance screen and sign-out | `TC-PRV-800` |
| Evidence export | `GET /api/v1/platform/legal-acceptances` with the keyset envelope, exportable by the owner | `TC-PRV-801` |

### 6. Child-safety commitments as contract clauses

These clauses sit in the terms (§5.1) and are referenced by the data processing agreement clause 13. Each one names the product control that makes it true, so the contract cannot promise more than the product does.

| # | Clause | Product control | Proof |
|---|---|---|---|
| CS-1 | Nibras shows no advertising to any user and sells no data, in any form, to anyone | No advertising or data-broker sub-processor exists; the sub-processor list is the evidence | `TC-PRV-060` |
| CS-2 | Nibras does not track the behaviour of students for analytics, engagement scoring or profiling; product analytics covers staff and administrator usage only and is self-hosted | Analytics disabled for student and guardian sessions; no third-party tracking SDK in the mobile build | `TC-PRV-064` |
| CS-3 | Student-to-student messaging is off unless the school turns it on, and the school decides who may message whom and when | Messaging policy per school | `TC-PRV-021`, `TC-SEC-210` |
| CS-4 | No student has a public profile, and no one outside the school can search for a student | No anonymous route returns a student | `TC-PRV-022` |
| CS-5 | The location of a child is never tracked; only vehicles are | No child position field exists | `TC-SEC-310`, REQ-PRV-020 |
| CS-6 | No automated process profiles a child in a way the guardian or the school cannot see and challenge; every flag shows its reasons and can be overridden | Because panel (`25-ai-and-assist-ladder.md` §7); guardian transparency panel | `TC-PRV-027` |
| CS-7 | Nothing generated by AI reaches a family without a person approving it | Review enforced at autonomy 3 | `TC-AI-201` |
| CS-8 | A child's photo is not published without the recorded media consent, and withdrawal takes effect across the product | Consent enforced at publishing | `TC-PRV-502`, `TC-PRV-026` |
| CS-9 | Every conversation offers report and block, and a report reaches the safeguarding officer within 15 minutes | Report lock and routing | `TC-PRV-023`, `TC-PRV-024` |
| CS-10 | Oversight access by the safeguarding officer is itself logged and reviewable by the principal | Access entries on every oversight read | `TC-PRV-025` |
| CS-11 | Content flagged for safeguarding is never deleted by a routine retention job while a case is open | Retention jobs skip flagged rows and holds | `TC-PRV-029` |
| CS-12 | Wellbeing records never leave the Wellbeing service, never reach a device cache, and are never used to train or prompt any model | Isolation level S; the AI index excludes them | `TC-SEC-322`, `TC-SEC-280` |
| CS-13 | A guardian can see which roles read their child's sensitive records and when, and how long each category is kept | Guardian transparency panel | `TC-PRV-501`, `TC-PRV-702` |

### 7. The data-never-held-hostage clause

Master brief Section 36 says the sentence is in the contract. The clause, as it appears in the terms in both languages:

> **Your data is yours at every stage.** Export of all of your school's data stays available at every stage of your subscription, including when your account is past due, read-only, suspended, or scheduled for deletion. We will never withhold your data, or access to its export, to force a payment. After a deletion request, you have a 30-day cooling-off period during which the export remains available and the deletion can be cancelled; after it, we purge your data and issue a certificate of deletion. Backup copies age out within 35 days of deletion, except where a legal hold requires otherwise, and we will tell you if one does.

| Stage | Export | Mechanism | Test |
|---|---|---|---|
| Active | Available | WF-PLT-03 export rung; configuration as code for settings | `TC-PLT-023` (Appendix R) |
| Past due, reminders at 7, 14 and 30 days | Available | Dunning never touches export | REQ-PLT-013 acceptance test |
| Read-only at 30 days past due | Available | Read-only blocks writes, never exports | `TC-PLT-022` (Appendix R) |
| Suspended | Available to the owner | Export route exempt from suspension; the owner keeps sign-in | `TC-PLT-021`, `TC-PLT-903` |
| Deletion requested, 30-day cooling-off | Available throughout, cancellable | WF-PLT-03 with BR-PLT-003 | `TC-PLT-024`, `TC-PLT-025` |
| Purged | Not available; certificate issued | `TenantPurgeJob` | `TC-PLT-026`, `TC-PRV-901` |

| Export content | Format |
|---|---|
| Every entity the tenant owns, per service | CSV per entity plus JSON with identifiers preserved, completeness checked under BR-PLT-006 |
| Documents and media | Original files with a manifest |
| Configuration | The configuration-as-code JSON (Section 12.1 item 41) |
| Audit entries | JSON with the hash chain and the anchors, verifiable offline |
| Encryption | A key the tenant can be handed (Appendix J.5 rule 7) |

### 8. Residency

Quoted from master brief Section 34: a region is a whole deployment. A tenant is pinned to a region at provisioning and its data never leaves: no cross-region replication of tenant data, and backups stay in region. Moving a tenant between regions is an export and import, scheduled with the school.

| Control | Where it lives | Proof |
|---|---|---|
| Region chosen at signup and immutable after `Validated` | Platform `Tenant.region_code`; BR-PLT-004; `PLATFORM_RESIDENCY_VIOLATION` | `TC-PLT-101`; REQ-PLT-030 |
| Databases, object storage, queues and caches of a region run only in that region | `15-deployment-and-operations.md` per deployment | Deployment review per region |
| Backups stay in region | Backup target per region (`10-data-architecture.md` §9) | `TC-PRV-074` |
| Staging copies are anonymized and stay in region | Master brief Section 34 environments table; REQ-PRV-023 | Staging refresh test |
| Sub-processors in region, or disclosed and consented | Sub-processor list region column (§3.3) | `TC-PRV-079` |
| Rung 4 AI discloses what left and where | `25-ai-and-assist-ladder.md` §3.3 | `TC-SEC-323` |
| Operator access from outside the region | Only through consented impersonation with a banner, fully logged; no data copied out | `12-security-privacy-safety.md` §3.6 |
| Moving regions | Export, then import into a new tenant in the target region, then deletion with a certificate in the old one; a scheduled project, never a replication | WF-PLT-03 plus this document's migration path in `26-migration-and-onboarding-toolkit.md` |

| Region | Serves | Data protection law | E-invoicing |
|---|---|---|---|
| Saudi Arabia | Schools in the Kingdom | Saudi PDPL | ZATCA plug-in (§9.1) |
| United Arab Emirates | Schools in the UAE | UAE PDPL | UAE plug-in when the programme is live |
| Jordan | Schools in Jordan | Jordan PDPL | JoFotara plug-in (§9.2) |
| On-premises | One school or group, in its own infrastructure | The school's country | The country's plug-in |

### 9. E-invoicing plug-in interfaces

The interface is `IEInvoicingProvider` in `23-integrations-and-public-api.md` §8.2, certified per §8.3 of that document. Nibras keeps numbering, totals, tax and the posted document; the plug-in never recomputes a total or changes a posted document. What each country requires of the plug-in:

#### 9.1 ZATCA, Saudi Arabia

| Obligation | Plug-in behaviour | Interface method |
|---|---|---|
| Device onboarding with the authority, compliance then production credentials | Onboarding with the one-time code the taxpayer obtains from the authority's portal; credentials stored encrypted in `ProviderConfiguration` | `OnboardAsync` |
| Standard tax invoices (to a business, such as a corporate sponsor) are cleared before they are issued | Submit for clearance; the invoice is issued to the payer only after the cleared response returns | `SubmitInvoiceAsync` |
| Simplified tax invoices (to a parent) are reported within 24 hours of issue | Report after issue; a failure is retried and an invoice still unreported at 20 hours raises an alert to the accountant | `SubmitInvoiceAsync`, `GetSubmissionStatusAsync` |
| Credit notes follow the same model as the invoice they correct | Submit the credit note, referencing the original | `SubmitCreditNoteAsync` |
| Invoice hash chain and counter | The plug-in computes the hash and stamp over the document Nibras passes, in Nibras' numbering order; Finance serialises submission per numbering series | `SubmitInvoiceAsync` |
| QR code on the invoice | Payload returned to Documents for printing | `BuildQrPayloadAsync` |
| Specification version | Pinned in `Describe()`; a specification change is a new plug-in version and re-certification | `Describe` |

#### 9.2 JoFotara, Jordan

| Obligation | Plug-in behaviour | Interface method |
|---|---|---|
| Registration of the taxpayer's client credentials with the national system | Credentials stored encrypted; validated before first use | `ValidateConfigurationAsync`, `OnboardAsync` |
| Each invoice and credit note submitted to the national system | Submit after posting; the returned reference and QR stored on the document | `SubmitInvoiceAsync`, `SubmitCreditNoteAsync` |
| QR code printed on the invoice | Payload from the system's response | `BuildQrPayloadAsync` |
| Status of a submission | Polled until accepted or rejected; a rejection raises a task to the accountant with the reason | `GetSubmissionStatusAsync` |
| Specification version | Pinned in `Describe()` | `Describe` |

#### 9.3 Rules common to both

| Rule | Detail |
|---|---|
| Idempotent on invoice id | A resubmission returns the stored result, never a second submission |
| Posted documents are immutable | A rejection is corrected by a credit note and a new invoice, never by editing (master brief Section 36) |
| Offline tolerance | A submission queue on the Finance worker survives an authority outage; issued simplified invoices are reported when it returns, inside the reporting window where possible, and the accountant is told when it is not |
| Evidence | Every submission and response kept with the financial document for its 10-year retention |
| Explicit setup | The plug-in is proposed by the smart defaults engine and confirmed by a named person, never switched on silently (`26-migration-and-onboarding-toolkit.md` §7.1) |

### 10. Law-enforcement and safeguarding requests

Master brief Section 38: requests from law enforcement are answered by a named person against a written process, never by an engineer with database access, and are recorded. The runbook is `law-enforcement-request.md` (`12-security-privacy-safety.md` §10.4); the steps it must contain:

| Step | Rule | Record |
|---|---|---|
| 1 Receive | Only through the published legal channel; a request to an engineer, support agent or sales contact is forwarded unanswered | Request logged on receipt |
| 2 Verify | Authenticity of the requesting authority and its jurisdiction; a foreign authority is referred to the channels of the tenant's region | Verification note |
| 3 Legal basis | The named person (the data protection lead, with counsel) confirms the order is valid and specific; a broad or informal request is refused or narrowed | Basis recorded |
| 4 Tell the school | The school, as controller, is told unless the order lawfully forbids it; where forbidden, the reason is recorded and the school is told when the restriction ends | Notice record |
| 5 Minimum data | Only the records the order names, for the subjects and period it names | Scope recorded |
| 6 Produce | Through the WF-PRV-01 export tooling with the data protection lead as the proxy subject; never a database query by an engineer | Export job with reason and watermark |
| 7 Second review | A second named person reviews the package before release | Reviewer recorded |
| 8 Hold | Where the order requires preservation, a legal hold on the subject or case (`platform.retention.place-legal-hold`) | Hold record, itself audited |
| 9 Close and report | The request is closed with what was produced; counts are published in a yearly transparency report without identifying anyone | Closure record; transparency report |

| Safeguarding request | Rule | Source |
|---|---|---|
| Flagged message or anonymous report | Reaches the safeguarding officer within 15 minutes; acknowledged within one working day; an urgent concern pages the officer and the principal | Master brief Section 38; WF-WEL-04 |
| Retention | Flagged content follows the wellbeing rule of Section 32 and is never deleted by a routine job while a case is open | `TC-PRV-029` |
| Legal hold | Freezes everything connected to a named student or case; the hold itself is audited and alerts the safeguarding officer | `12-security-privacy-safety.md` §10.4 |
| A request from a child-protection authority | Handled by the school as controller through its safeguarding officer; Nibras assists only through the steps above when the request reaches Nibras | This section |
| Data subject access request | Acknowledged within 5 working days and answered within 30 calendar days through the export workflow | Master brief Section 38; §3.2 |

Proof: `TC-PRV-078`, REQ-PRV-019.

### 11. Evidence base for SOC 2 Type II and ISO 27001

Section 33 places both on the post-launch roadmap and says the product is built to produce the evidence. The audit is a business decision; the evidence must exist from the first release.

| Control area | Evidence the product produces | Source |
|---|---|---|
| Access reviews | Quarterly export of role grants with high-risk grants and their approvers | Identity; `12-security-privacy-safety.md` §4, §5 |
| Change management | Pull request reviews, pipeline results, release records, canary and rollback outcomes | WF-INF-02; `06-services/platform.md` §5.12 |
| Audit trail | Hash-chained audit with integrity verification runs | Audit; `TC-AUD-101` onwards |
| Incident management | Incident records and blameless postmortems | Master brief Section 23; `15-deployment-and-operations.md` |
| Backup and recovery | Quarterly restore and disaster-recovery drill results | WF-INF-03; `10-data-architecture.md` §9 |
| Vulnerability management | Dependency and image scans per build, penetration test report, disclosure log | `12-security-privacy-safety.md` §11, §12 |
| Supplier management | Sub-processor list with review dates | §3.3 |
| Data retention | Retention job reports per row | §2 |
| Privacy | Subject request log with service levels met | WF-PRV-01 |

### 12. Checklist for adding a fourth country

A new country is data, plug-ins and legal review, not new architecture (master brief Section 33, and the smart-defaults rule that a country is a settings row and a test). Egypt is the planned next case.

| # | Item | Owner | Evidence before the first school goes live |
|---|---|---|---|
| 1 | Legal analysis of the country's data protection law: lawful bases, consent age, subject-request deadlines, breach notification period, transfer rules, special-category rules for children | Data protection lead with local counsel | Written memo; §1.2 row added for the country |
| 2 | Residency decision: whether the law requires in-country hosting, and the region that serves the country | Architect, product owner | A region row in §8; deployment plan in `15-deployment-and-operations.md` |
| 3 | Retention minimums per Section 32 row, and the local safeguarding retention rule | Local counsel | Country minimums in the country plug-in; §2 overrides tested |
| 4 | Legal documents: terms with governing law, data processing agreement, privacy policy, privacy notice, consent texts, in Arabic and English and any required official language | Legal | Published `LegalDocument` versions for the country |
| 5 | Smart defaults row: work week, first day, calendars, numerals, year shape, holidays, bell schedule, terminology, grading scheme, currency and minor units, tax treatment | Platform owner | Inference table row and the whole-set test for each school type |
| 6 | National identity validator | Documents owner | `NationalIdValidator` for the country with a table-driven test (`26-migration-and-onboarding-toolkit.md` §3.4) |
| 7 | E-invoicing plug-in, if the country has a programme | Finance owner | Certified `IEInvoicingProvider` (§9, `23-integrations-and-public-api.md` §8.3) |
| 8 | Ministry export, if schools must report | Reporting owner | Certified `IMinistryExport` |
| 9 | Payment gateway and SMS provider available in the country | Finance, Notification owners | Certified `IPaymentGateway` and `ISmsSender`; sub-processor rows |
| 10 | Sub-processor list for the region | Data protection lead | Published list; `TC-PRV-079` passes for the deployment |
| 11 | Localization: glossary review for local terminology, date and number conventions, any new locale | Arabic reviewer, localization owner | `24-localization-and-calendars.md` glossary entries; culture tests under the new locale |
| 12 | Accessibility statement and any local accessibility law | Accessibility lead | Statement for the country (§4) |
| 13 | Support hours in the country's time zone, and the status page | Customer success | Support tier schedule |
| 14 | Impact assessment pre-fill for the country's lawful bases | Platform owner | Template section 1 per country |
| 15 | Demo data variant for sales | Product owner | Appendix H variant with local names and calendar |
| 16 | Regulator contact and breach notification route | Data protection lead | Runbook entry |

---

## Decisions in force

| Decision | Source | Default if unanswered | Impact if wrong |
|---|---|---|---|
| The school is the controller and Nibras the processor | This document §3.2, §5.2 | As stated | Nibras would answer subjects directly and override the school's decisions |
| Retention job names follow `10-data-architecture.md` §8 | This document §2 | As stated; see open point 1 | Two documents name different jobs for one row |
| 30-day notice for new sub-processors and material legal changes | This document §3.3, §5.3 | 30 days | Shorter notice leaves schools no time to object; longer delays needed providers |
| Breach notice to the school within 48 hours of confirmation | This document §5.2 clause 8 | 48 hours | Longer than a country's statutory deadline puts the school in breach |
| The data-return clause is verbatim in the terms | Master brief Section 36; this document §7 | As stated | A weaker contract promises less than the product does, which invites dispute |
| Child-safety commitments are contract clauses bound to controls | This document §6 | As stated | A clause without a control is an unkeepable promise |
| Law-enforcement requests are answered only by the named person through export tooling | Master brief Section 38; this document §10 | As stated | An engineer with database access becomes the disclosure path |
| E-invoicing specifications are pinned per plug-in version | This document §9; `23-integrations-and-public-api.md` §8 | As stated | An authority change silently breaks submissions |

## Dependencies on other documents

| This document assumes | Stated in | Checked on |
|---|---|---|
| Consent, subject rights, retention tests, legal hold, sub-processor template, impact assessment outline | `12-security-privacy-safety.md` §10 | Group F review |
| Child-safety controls | `12-security-privacy-safety.md` §7 | Group F review |
| Penetration test and disclosure | `12-security-privacy-safety.md` §12 | Group F review |
| Retention jobs and backups | `10-data-architecture.md` §8, §9 | Group F review |
| Legal documents, acceptances, holds, subject requests, retention schedule endpoints | `06-services/platform.md` §4.9, §5.10 | Group F review |
| E-invoicing interface and certification | `23-integrations-and-public-api.md` §8.2, §8.3 | Group F review |
| WCAG criteria and verification | `14-design-system-and-ux.md` §11; `16-test-strategy.md` §8 | Group F review |
| Rung 4 disclosure and consent | `25-ai-and-assist-ladder.md` §3.3 | Group F review |
| National identity validators and the migration path between regions | `26-migration-and-onboarding-toolkit.md` §3.4, §6 | Group F review |
| Workflows WF-PLT-03, WF-PRV-01, WF-WEL-04 | Appendix R | Every lint run |

## Open points

| Question | Default | Owner | Impact if the default is wrong | L | I | Score | In the register |
|---|---|---|---|---|---|---|---|
| 1. `10-data-architecture.md` §8 and `12-security-privacy-safety.md` §10.3 name the retention jobs differently (`LeaverRetentionJob` against `retention-anonymize-leavers`, and so on) | Class names from `10-data-architecture.md` §8 are canonical; §10.3 of document 12 is aligned at its next revision, keeping its test identifiers | Architect | Two names for one job confuse the evidence pack and the runbooks | 2 | 1 | 2 | none |
| 2. Is 48 hours the right processor breach notice for all three first countries? | 48 hours, shortened per country by the country plug-in if a law requires it | Data protection lead with counsel | A statutory deadline shorter than the contract leaves the school exposed | 2 | 4 | 8 | RISK-23 |
| 3. Which language version of the terms prevails in a dispute? | Neither, unless a country's law names one, in which case the country plug-in records it | Legal | A court applies the other text | 2 | 3 | 6 | none |
| 4. Publish a yearly transparency report on law-enforcement requests? | Yes, counts only | Product owner | Without it, schools cannot verify clause 12 | 2 | 1 | 2 | none |
| 5. Open question 19: retention periods per country. §2 applies master brief Section 32's periods and marks the rows that vary by country law | The recorded default: the Section 32 periods, held as configuration so a country plug-in can override them | Product owner, with the data protection lead | A country that mandates a longer or shorter period for a data class puts every school there out of compliance until the override ships, and a longer period grows the storage line of `28-capacity-and-cost-model.md` part 2.8 | 3 | 3 | 9 | RISK-23 |
| 6. Open question 26: does any first customer require a formal certification such as SOC 2 Type II or ISO 27001? §1.1 places both on the post-launch roadmap and §11 keeps their evidence base | The recorded default: no; compatibility yes, certification only when a customer pays for it, with the penetration-test summary and §1 offered in its place (§5.2 clause 11) | Product owner | A school group or procurement office that makes certification a contract condition is lost, because a Type II report needs an observation period that cannot be shortened | 2 | 4 | 8 | none |
| 7. Open question 3: target countries for the first customers. §1 and §9 cover Saudi Arabia, the United Arab Emirates and Jordan, and §12 is the checklist for any other | The recorded default: those three | Product owner | A fourth country before launch runs the whole §12 checklist, counsel review of every legal document included, inside a delivery phase | 2 | 3 | 6 | RISK-23 |

> L and I are the likelihood that the default is wrong and the impact if it is, on the 1 to 5 scales of `18-risk-register.md` Section 1. Score is L x I. A point that scores 12 or more names its RISK identifier in document 18; below that, the identifier if one covers it, or `none`. Kit-lint rules R24 and R33 check all of it (ADR-0022).

## Review record

| Date | Reviewer | Verdict | Blocking items |
|---|---|---|---|
| 2026-09-22 | Group F review, round 1 (independent adversarial scorecard) | Blocked: the group scored below 4 on Completeness, Consistency, Feasibility, Risk honesty, Testability and Distinctiveness | `TC-PLT-001` meant residency here and other things in documents 26 and 32 (Consistency, Testability) |
| 2026-09-26 | Group F review, round 2 | Blocked: the group scored below 4 on Completeness, Consistency, Risk honesty and Testability | The collision was closed ("Residency holds" cites `TC-PLT-101`). Still open: "Legal acceptance is recorded and enforced" named a requirement's acceptance test, not a test (Testability) |
| 2026-09-26 | Round 3 remediation | Amended; awaiting the round 3 score | §5.3 and the legal-acceptance row cite `TC-PLT-113` (Platform sheet) and the new `TC-PRV-800` and `TC-PRV-801`; the 30-day notice and the counsel review of the legal text are named review steps |

## How this document is verified

| Claim | Proof | Where it runs |
|---|---|---|
| Every Section 32 row has a job that runs | `TC-PRV-068` to `TC-PRV-075`, `TC-PLT-026`; job reports on the privacy dashboard | Nightly, and every pull request touching a retention job |
| Holds suspend deletion and pin backups | `TC-PRV-076`, `TC-PRV-077`, `TC-PRV-074`, `TC-PRV-902` | Every pull request touching retention |
| Consent, subject rights, sub-processors and impact assessment behave as summarised | The tests cited in §3 | Per `12-security-privacy-safety.md` |
| Legal acceptance is recorded and enforced | `TC-PLT-113` (Platform sheet) records one acceptance per version; `TC-PRV-800` proves the sign-in gate and `TC-PRV-801` the evidence export, both defined below; the 30-day material-change notice is the data protection lead's review step at every `terms` or `dpa` publish (§5.3); the legal text itself is reviewed by counsel for each country before its first publish and on every material change, and the data protection lead records that review in the release notes of the publish | Every pull request touching Platform or Identity sign-in; the two reviews at each publish |
| Export is available at every stage | `TC-PLT-021` to `TC-PLT-026`, `TC-PLT-903` | Every pull request touching Platform |
| Residency holds | `TC-PLT-101`; deployment review per region | Every pull request touching Platform; per region at release |
| Child-safety clauses are true | The tests in §6 | Every pull request touching the owning services |
| The accessibility statement is generated from real results | Release pipeline stage that assembles §4 from CI and the manual pass | Every release |
| E-invoicing plug-ins meet §9 | Conformance suite in `Nibras.Plugins.Testing` per plug-in version | Plug-in certification |
| This document agrees with the catalogs | kit-lint R01, R02, R05, R17 and R19; `plan-consistency-checker` with `privacy-auditor` compares this document with `12-security-privacy-safety.md`, `10-data-architecture.md` and `06-services/platform.md` | kit-lint on every change under `docs/`; the comparison at the Group F review and on every change to any of them |

### Test cases

This document defines the two legal-acceptance tests below; recording one acceptance per version is `TC-PLT-113` in the Platform sheet and is not restated. Both run against Identity sign-in and the Platform legal endpoints with the clock pinned.

| Test case | What it proves | Covers |
|---|---|---|
| TC-PRV-800 | Given a guardian who accepted privacy policy version 2, when version 3 is published and the guardian signs in, then every request other than the acceptance screen, the acceptance call and sign-out is refused until version 3 is accepted, the acceptance screen shows version 3 in the guardian's language with its summary of changes, and after acceptance the next request succeeds and no second prompt appears for version 3 | REQ-PLT-033 |
| TC-PRV-801 | Given 3 users who accepted privacy policy version 1 and 2 users who accepted version 2, when the tenant owner calls `GET /api/v1/platform/legal-acceptances` with a page size of 2, then the keyset walk returns the 5 rows exactly once each, every row carries the document, version, user or tenant, time and hashed address and none carries a clear address, and a caller who is not the owner is refused | REQ-PLT-033 |
