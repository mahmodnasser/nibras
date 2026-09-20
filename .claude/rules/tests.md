---
paths:
  - "tests/**/*"
  - "src/**/*.Tests/**/*"
  - "src/Web/**/*.spec.ts"
  - "src/Mobile/test/**/*"
  - "docs/plan/16-test-strategy.md"
---

# Test rules

- Every test names the requirement or rule it covers: a `TC-<AREA>-<NNN>` identifier in the test name or an attribute. A test that covers nothing traceable is deleted or given a purpose.
- **Assert the data, not the status code.** A `200 OK` carrying another tenant's student passes a status assertion.
- Every authorization test has a denied twin. Every validation test has a rejected input. Every boundary is tested at the value, one below, and one above.
- Tenant isolation and permission matrix suites are **generated** from the endpoint and consumer registries, so they cannot fall behind. A new endpoint with no generated isolation test fails the build.
- Every business rule has its named test class, and every worked example in the rule is a case in that class.
- Integration tests use Testcontainers with a real PostgreSQL, Redis, and RabbitMQ. No in-memory database substitutes for a query that has to be correct.
- Pin the clock through the injected `TimeProvider`, pin the culture to invariant, and pin the time zone. A test that passes only in June or only in a Latin locale is a scheduled failure.
- Never assert row order without an `ORDER BY`.
- Hot paths carry a query-budget assertion using the command-counting interceptor. A handler whose command count grows with the roster fails.
- Never mock the subject under test. Mock the boundary.
- A flaky test is disabled with a linked issue on the same day, never retried into green.