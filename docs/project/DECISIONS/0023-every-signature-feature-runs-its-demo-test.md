# ADR-0023: Every signature feature runs its own demo test in the release gate

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-26
- **Requirement IDs:** REQ-TST-022

## Context

Master brief Section 4 promises that a school owner watching the fifteen-minute demo sees things competitors do not have, and Appendix W gives every signature feature a sixty-second demo test. The scorecard (`docs/plan/30-plan-scorecard.md`, theme 8) found that the promise was not held by anything: 23 of 44 features had no minute of the demo, feature 39 could not be shown at all, and several phase 2 minutes needed capabilities from later phases.

ADR-0019 answered most of that: feature 39 moved to engineering capabilities, every minute states the phase it runs from with a substitution for earlier phases, and a reserve bank of one-minute steps covers the features without a minute. What remained was the proof. Appendix O says the tests of every step run in the end-to-end suite on every release, but for nine features the step that shows them ran a different test from the feature's own demo test in Appendix W, so the feature's proof was defined and never gated. Feature 3, "sixty-second attendance", was shown in minute 2, whose test was the pre-filled register's, not the sixty-second measurement.

## Decision

**Every signature feature is shown by a demo step whose Test cell runs the feature's own Appendix W demo test, and the release gate runs every step.**

- Each of the 43 signature features appears in the Feature column of a minute or a reserve step of Appendix O, and that step's Test cell names the feature's demo test from Appendix W. A feature with no step of its own (feature 23, the live interface, present in every minute) has its demo test named in Appendix O.
- Every step states the phase it runs from. At each phase exit and every release, the demo gate runs the tests of every step whose phase has shipped; a feature's demo test runs from the phase that builds the feature (document 17), even when its step runs earlier.
- **Kit-lint rule R34** enforces the first two points. The demo-director agent reviews the choreography itself.

**Brief change under this record (v9.4).** Appendix O's Test cells for minutes 1, 2, 4, 5, 10, 11, 13 and 15 gain the demo tests of the features they show: TC-RPT-001, TC-ATT-810, TC-MOB-003, TC-NOT-001, TC-RPT-003, TC-WEL-810, TC-PLT-805, TC-AUD-001, TC-ATT-811 and TC-PLT-801. All three brief files are bumped to v9.4.

## Alternatives considered

- **Point Appendix W's Demo column at whatever test the step already runs.** Rejected. The demo tests in Appendix W measure the feature's moment ("done before the bell stopped"); a step's other test proves something else, and the feature's claim would go unmeasured.
- **Give every feature its own minute.** Rejected. The demo is fifteen minutes by design; the reserve bank exists so a presenter can swap in the feature a buyer asks about.

## Consequences

- The release gate grows by ten tests, all already defined.
- A new signature feature cannot be added without a demo step that runs its test; R34 fails the lint otherwise.
- Documents 15, 16 and 32 state the gate the same way.
