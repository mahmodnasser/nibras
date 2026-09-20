# Traceability

Generated during planning as `docs/plan/20-traceability-matrix.md`, then maintained here through the build.

Area codes come from Appendix L. Identifier formats are `REQ-<AREA>-<NNN>`, `WF-<AREA>-<NN>`, `BR-<AREA>-<NNN>`, `TC-<AREA>-<NNN>`.

**A row with an empty Test case column is an unfinished requirement**, whatever the code says. `/lint-plan` refuses a matrix with a gap, and master brief Section 24 makes the same point as a quality gate.

The Platform column names a runner or device class from Appendix X, and is `any` unless the requirement exists because of an operating system or device difference.

| Requirement | Summary | Tier | Service | Workflow | Rule | Plan document | Phase | Test case | Platform | Status |
|---|---|---|---|---|---|---|---|---|---|---|
