# `TC-<AREA>-<NNN>`: `<What it proves>`

> Copy into `docs/brief/02-appendices/appendix-v-coverage-matrix.md` or the service test plan. A test exists to fail when the product is wrong.

| Field | Value |
|---|---|
| Covers | `<REQ-AREA-NNN, BR-AREA-NNN, WF-AREA-NN>` |
| Level | `<unit | integration | contract | end-to-end | generated suite>` |
| Owning service | `<Service>` |
| Automated in | `<test class and method>` |

## Precondition

| Item | Value |
|---|---|
| Tenant | `<tenant>` |
| Actor and permission | `<role>`, `<service>.<resource>.<action>` |
| Data state | `<exact records that must exist>` |
| Clock pinned to | `<timestamp>` |
| Culture and time zone | `<invariant or tenant culture>`, `<zone>` |

## Action

`<The single action under test, with its exact input.>`

## Expected

| Assertion | Expected value |
|---|---|
| Status or outcome | `<value>` |
| Data assertion | `<the records, re-read and compared field by field>` |
| Side effects | `<events published, notifications sent, audit entry>` |
| Side effects that must NOT happen | `<what must not be written or sent>` |

## What this would catch

`<The specific defect this test exists to detect. If you cannot name one, the test is decoration.>`

## Twins

| Related case | Why it exists |
|---|---|
| `TC-<AREA>-<NNN>` | The denied twin, or the value one unit the other side of the boundary |
