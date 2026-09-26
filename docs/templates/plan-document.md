# `<NN>. <Document title>`

> Copy to `docs/plan/<NN>-<name>.md`. Fill every section. An unanswered point moves to `docs/project/OPEN_QUESTIONS.md` with a default and an owner, and this document states the default as the decision in force.

**Group** `<A | B | C | D | E>` · **Requirement areas covered** `<AREA, AREA>` · **Last updated** `<date>` by `<who>`

## Purpose

One paragraph: what decision this document lets a reader make, and who the reader is.

## Scope

| In scope | Out of scope | Where the out-of-scope item lives |
|---|---|---|

## Content

`<The body of the document. Use tables and Mermaid diagrams wherever they are clearer than prose. Structures are fenced trees with a purpose comment per entry.>`

## Requirements covered

| Requirement ID | What it means here | Acceptance criterion | Test case ID |
|---|---|---|---|

## Decisions in force

| Decision | Source (ADR or open question) | Default if unanswered | Impact if wrong |
|---|---|---|---|

## Dependencies on other documents

| This document assumes | Stated in | Checked on |
|---|---|---|

## Open points

| Question | Default | Owner | Impact if the default is wrong | L | I | Score | In the register |
|---|---|---|---|---|---|---|---|

> L and I are the likelihood that the default is wrong and the impact if it is, on the 1 to 5 scales of `18-risk-register.md` Section 1. Score is L x I. A point that scores 12 or more names its RISK identifier in document 18; below that, write the identifier if one covers it, or `none`. Kit-lint rules R24 and R33 check all of it (ADR-0022). When nothing is open, write one row that starts with None.

## Review record

| Date | Reviewer | Verdict | Blocking items |
|---|---|---|---|