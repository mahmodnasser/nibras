---
paths:
  - "src/Services/**/Domain/**/*.cs"
  - "src/Services/**/Application/**/*.cs"
  - "src/Services/**/Workflows/**/*.cs"
---

# Business rule and workflow implementation rules

- **A handler that implements a business rule names its `BR-<AREA>-<NNN>` identifier in a comment on the class or method**, and has the test class the rule names. Rule, code, and test are found from each other or they will drift.
- Rules live in the domain, never in an endpoint, a consumer, a component, or a database trigger. One rule, one place.
- Money is `decimal` with an explicit currency. Percentages state whether they are fractions or whole numbers in the type or the name. Rounding happens once, at the point the rule names, and the code comments say which point.
- The order of operations in the code matches the order stated in the rule. Where the rule is ambiguous, stop and get the rule fixed rather than choosing an order.
- Every workflow is a state machine with an explicit state type, not a set of boolean flags. Every transition validates the current state, checks the permission, writes the audit entry, and publishes its event through the outbox.
- Every state a human waits in has a timeout with a defined escalation. Holidays are excluded from the clock where the workflow says so.
- Concurrent transitions resolve deterministically: first decision wins, second returns a stable error code naming who decided.
- Every terminal state is reachable from every state, directly or through cancellation, so support can always end a stuck item.
- Reversal is designed, not improvised. A transition that cannot be reversed says so and is ordered after everything that can.
- A rule change means updating the rule document, the worked examples, the test class, and every screen that quotes the rule, in the same change.