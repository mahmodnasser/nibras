# ADR-0026: Appendix N uses the backend-for-frontend route prefix of document 22

- **Status:** Proposed, awaiting product owner confirmation
- **Date:** 2026-09-26
- **Requirement IDs:** REQ-BFF-008

## Context

`docs/plan/22-api-conventions-and-error-catalog.md` fixes the backend-for-frontend prefixes as `/bff/web/v1/` and `/bff/mobile/v1/`, and documents 08 and 09 and the Bff.Web and Bff.Mobile sheets use them. Appendix N still wrote `/bff-mobile/` in the Exercised rows of N-05 (`GET /bff-mobile/home/principal`) and N-08 (`POST /bff-mobile/sync/batch`), and `docs/plan/15-deployment-and-operations.md` copied the N-08 route into alert 28. A load scenario or an alert built from those rows would target a route that does not exist. The round-5 scorecard and the Bff.Mobile sheet's open point 1 recorded the disagreement; only a brief change under a record can close it.

## Decision

**Appendix N names the routes document 22 defines.** N-05 exercises `GET /bff/mobile/v1/home/principal` and N-08 exercises `POST /bff/mobile/v1/sync/batch`. No route, scenario or threshold changes otherwise.

**Brief change under this record (v9.6).** Appendix N, the Exercised rows of N-05 and N-08. All three brief files are bumped to v9.6.

## Alternatives considered

- **Change document 22 to `/bff-mobile/`.** Rejected. Documents 08, 09, 22 and both BFF sheets already agree on `/bff/<client>/v1/`, which carries the version in the path as every other route of document 22 does.

## Consequences

- Document 15's alert 28 and the Bff.Mobile sheet's open point 1 are corrected in the same change.
- Kit-lint R03 keeps the three brief files on one version.
