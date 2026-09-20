# `WF-<AREA>-<NN>`: `<Workflow name>`

> Copy into `docs/brief/02-appendices/appendix-r-workflow-catalog.md` or `docs/plan/13-workflows-and-sagas.md`. Every transition names a role from the permission catalog and a test case ID.

**Owning service** `<Service>` · **Triggered by** `<actor or event>` · **Requirement IDs** `<REQ-AREA-NNN>`

## States

```mermaid
stateDiagram-v2
    [*] --> <FirstState>: <trigger>
    <FirstState> --> <NextState>: <trigger>
    <NextState> --> [*]
```

| State | Meaning | Who is waiting | Timeout | On timeout |
|---|---|---|---|---|

## Transitions

| From | To | Trigger | Who may (permission) | Validation | Events published | Notifications | Audit entry | Test case ID |
|---|---|---|---|---|---|---|---|---|

## Concurrency

What happens when two people act at once: `<rule>`. The losing actor receives `<SERVICE>_<MEANING>` and sees who decided.

## Reversal

| From terminal state | How it is undone | Who may | What cannot be undone |
|---|---|---|---|

## Calendar behavior

| Aspect | Rule |
|---|---|
| Working days only | `<yes or no>` |
| Holidays excluded from timers | `<yes or no>` |
| Behavior across a term boundary | `<rule>` |
| Behavior across a year rollover | `<rule>` |

## Edge cases covered

| Edge case | Behavior | Test case ID |
|---|---|---|