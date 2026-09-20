---
description: Provision and verify a new tenant end to end, then hand it over with a go-live checklist
argument-hint: [school name] [country] [school type]
allowed-tools: Read, Grep, Glob, Bash
disable-model-invocation: true
---

Onboard the tenant described by: $ARGUMENTS

This command runs the real provisioning saga against a running stack. It is not a simulation. If the stack is not running, say so and stop rather than pretending.

## Reads first

- Master brief Section 7.4 (tenant provisioning saga), Section 10.4 (how people join), Section 39 (training and go-live)
- Appendix R, workflow `WF-PLT-01` tenant provisioning, and `WF-PLT-02` trial and plan change
- Appendix G, the settings the smart defaults engine infers
- `docs/plan/26-migration-and-onboarding-toolkit.md` when it exists

## What to do

1. **Pre-flight.** Confirm the inputs the wizard needs: legal name in both languages, country, school type, campuses, academic year dates, work week, currency, first administrator's contact. Report anything missing and stop if it is material.
2. **Show what smart defaults inferred** from country and school type before applying them: work week, calendar, numerals, tax treatment, e-invoicing plug-in, terminology, bell schedule, request templates, role templates. Ask for corrections. Never apply an inference silently.
3. **Run provisioning.** Start the saga and report each step as it completes: tenant record, per-service seed, branding, administrator invitation, optional demo data. If a step fails, report the compensation that ran and the state the tenant is in.
4. **Verify the tenant is usable**, with evidence for each: the administrator invitation was delivered; sign-in works; the tenant sees no other tenant's data; a notification reaches a test recipient; an audit entry exists for the provisioning.
5. **Import check.** If data files were supplied, run the import dry run, report the error report, and stop for a decision before committing.
6. **Hand over.** Produce the go-live checklist: super-user certification status, outstanding configuration, the support tier, and the date the school expects to be live.

## Output contract

Your response must contain these headings, in order:

- **Inputs and what is missing**
- **Inferred defaults, for confirmation**
- **Provisioning result** (step by step, with what you ran)
- **Verification evidence** (each check, and its actual result)
- **Import dry run** (or "not requested")
- **Go-live checklist**
- **Open items and owners**

## Stop conditions

Stop and ask rather than guessing when: the stack is not running; the country is not one with a configured plug-in set; the school type does not match a known template; a provisioning step fails and its compensation did not fully clean up; or the supplied import data fails validation on more than five percent of rows.

Never create a tenant in production without an explicit instruction naming the environment.
