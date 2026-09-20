# ADR-0001: Target .NET 10 (LTS) instead of .NET 8

- **Status:** Proposed (awaiting product owner confirmation, Open Question 1)
- **Date:** 2026-09-19

## Context
The original requirement named .NET 8. Microsoft ends support for .NET 8 and .NET 9 on 10 November 2026. .NET 10 is the current LTS, supported until November 2028. The product stores children's data and must receive security patches.

## Decision
Target .NET 10.

## Alternatives considered
- .NET 8: matches the original request, but loses security updates weeks after the project starts.
- .NET 9: same end-of-support date as .NET 8.

## Consequences
All libraries must support .NET 10. If a contract forces .NET 8, keep the code free of anything that blocks a one-step retarget.
