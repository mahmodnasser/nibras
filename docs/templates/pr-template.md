# `<Short imperative title>`

> Copy to `.github/pull_request_template.md`. A box that cannot be honestly ticked stays unticked, with a sentence saying why.

## What and why

`<One paragraph: what changes for a user of the product, and why now.>`

**Requirement IDs** `<REQ-AREA-NNN, ...>` · **ADRs** `<NNNN, or none>` · **Business rules** `<BR-AREA-NNN, or none>`

## How it works

`<Two or three sentences on the approach. Name the alternative you rejected.>`

## What I ran

| Command | Result |
|---|---|
| `<command>` | `<output summary>` |

## Checks

| Check | Status | Note |
|---|---|---|
| Tenant isolation test for every new endpoint and consumer | `<yes, not applicable>` | |
| Permission declared and checked server-side | `<yes, not applicable>` | |
| Events published through the outbox; consumers idempotent | `<yes, not applicable>` | |
| Published contracts unchanged, or a new version added | `<yes, not applicable>` | |
| Query budget test for any new query on a large table | `<yes, not applicable>` | |
| Cache entries name an invalidating event | `<yes, not applicable>` | |
| English and Arabic, LTR and RTL, light and dark | `<yes, not applicable>` | |
| Accessibility: keyboard, names, contrast, reduced motion | `<yes, not applicable>` | |
| Works on Windows and Linux; tools ship both wrappers | `<yes, not applicable>` | |
| New dependency passed the license audit | `<yes, not applicable>` | |
| Project memory updated (state, traceability) | `<yes, not applicable>` | |

## Risk

| What could go wrong | Likelihood | How we would notice | How we would undo it |
|---|---|---|---|

## Screenshots

`<Light LTR, light RTL, dark LTR, dark RTL for any visual change.>`