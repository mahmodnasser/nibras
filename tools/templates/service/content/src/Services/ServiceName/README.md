# ServiceName service

The anatomy of reference architecture Section 2. The specification of this service is its sheet in
`docs/plan/06-services/`; this file points to it and never restates it.

| | |
|---|---|
| Area | `AREACODE` |
| Database | `nibras_servicename` |
| Exchange | `nibras.servicename` |
| Api image | `nibras/servicename-api` |
| Error prefix | `SERVICENAME_` |
| Permission namespace | `servicename.` |

## Purpose and owned data

The service sheet, sections 1 to 3.

## API

The service sheet, section 5. The committed OpenAPI document is generated at build.

## Events

The service sheet, sections 6 and 7, and `src/Contracts/Nibras.Contracts.ServiceName/`.

## How to run

`dotnet run --project Nibras.ServiceName.Api`, or `aspire run` from `src/AppHost` for the whole stack. The probes
answer on `/health/live`, `/health/ready` and `/health/startup`.

## Runbook links

`docs/runbooks/`, one runbook per alert of this service.
