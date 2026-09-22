You are writing part of the work breakdown for the Nibras school-platform plan: the document that turns every capability into **slices** an engineer can pick up on a Monday. Kit root: the repository root; every path below is relative to it.

WRITE EXACTLY ONE FILE:
`tools/plan-build/parts/wb-part-{{W}}.md`
A script validates it and merges it into `docs/plan/34-work-breakdown.md`. Do not edit any other file.

YOUR SCOPE: {{SCOPE}}

## Read, in this order (read only)
1. `docs/plan/PLAN_SPEC.md`, the section "How work is broken down: phases, capabilities, slices" (grep -n "How work is broken down" then read about 60 lines). It holds the sizing rules and a worked example. Obey it.
2. `docs/plan/17-roadmap.md` Section 4, the capability tables for your phases, and Section 7 (dependencies and critical path).
3. Your requirement list: `tools/plan-build/parts/wb-scope-{{W}}.md`. **Every requirement ID in it must appear in the Covers column of at least one of your slices.** This is checked by script.
4. Your services' use cases: `tools/plan-build/parts/usecases.md` (the sections for your services). Each `F` line is a use-case folder under `Application/Features/` in that service's sheet; `C` lines are event consumers; `J` lines are background jobs. Every `F` line must be built by some slice; name it in the Use cases column.
5. `docs/plan/31-business-rules-and-workflows.md` rows for your services (grep "| <Service> |"), for the BR and WF identifiers.
6. Only when a use case's meaning is unclear, grep its name in `docs/plan/06-services/<service>.md`. Do not read whole sheets.

## Output format, exactly
For each capability in your scope, in roadmap order:

```
### CAP-XXX-NN <the capability sentence, copied from the roadmap>

**Phase:** <n> · **Services:** <from the roadmap> · **Demo:** <one sentence: what is shown, on the demo data, when the last slice lands>

| Slice | When it ships, a person can | Service | Days | Covers | Use cases | Depends on |
|---|---|---|---|---|---|---|
| SL-ATT-200 | A teacher's session holds marks for a class, and the lock window refuses late marks | Attendance | 2 | REQ-ATT-001, REQ-ATT-002; BR-ATT-001, BR-ATT-004; WF-ATT-01 | MarkAttendance (domain and persistence) | contract: `school.student.enrolled.v1` |
```

Column rules:
- **Slice**: `SL-<AREA>-<NNN>`, AREA from Appendix L (the area of the requirement it mostly covers). **Your numbers must fall in {{RANGE}}**, sequential per area from the start of your range, never reused.
- **When it ships, a person can**: a behaviour someone can see, never a layer, a table or a technology. "Build the attendance domain" is wrong; "A teacher's session holds marks..." is right.
- **Service**: canonical name from Appendix L, or `BuildingBlocks`, `Web`, `Mobile`, `deploy`, `tools` for platform work.
- **Days**: 1, 2 or 3. Anything larger is two slices.
- **Covers**: `REQ-` ids separated by commas; then `;` and `BR-` ids; then `;` and `WF-` ids, only those that exist. At least one `REQ-` id per slice.
- **Use cases**: the `F`, `C` and `J` names this slice builds, or for foundation work the building block or pipeline item.
- **Depends on**: state the dependency as a **contract** (a routing key, an endpoint, or a building block), not a slice, wherever possible, so streams can start early; otherwise a slice id in your range or `none`.

After all your capabilities, add one section:

```
### Coverage notes for part {{W}}
```
listing any requirement in your scope you judged to need no build work of its own (for example a policy already enforced by a gate), with the slice or gate that satisfies it. Keep that list short and honest; the default is that every requirement is built by a slice.

## Sizing guidance
A capability of 1 to 3 weeks usually needs 4 to 12 slices. A service's reference-data CRUD may be one or two slices for several small use cases together, as long as each slice still ships something a person can do and stays within 3 days. Consumers and jobs usually ride with the slice whose behaviour they complete. Web and mobile screens are slices of their own when they are more than a day's work, tagged `Web` or `Mobile`, depending on the API contract.

## Hard constraints
Never write TODO, TBD, FIXME, "to be decided", "to be determined". Only master-brief sections 1 to 40 and appendices A to X. Cite only identifiers that exist: `CAP-` ids from the roadmap, `REQ-` ids from your scope file, `WF-` and `BR-` ids from document 31, routing keys from Appendix E or document 11. The merge script fails on anything else.

Write the file in several appends, one or two capabilities per append, so partial progress survives an interruption.

When finished, run the validator in dry-run mode and fix every problem it reports for part {{W}}:
`node tools/plan-build/assemble-34.mjs --part {{W}}`

Report in under 120 words: capabilities written, slices, total days, uncovered requirements (should be none), and the validator output.
