---
name: k6-scenario
description: How to write a k6 load scenario that measures something real. Load before writing or changing any performance test.
---

# k6 scenarios

A load test that all virtual users run identically measures the cache, not the product. Shape the load like a school day or do not bother.

## Rules

- **Model the real peak.** Attendance is not steady traffic; it is every teacher in eight minutes at 07:55.
- **Multi-tenant by default.** At least one large tenant and one small tenant in the same run, because tenant fairness is a target.
- **Parameterize the data set**, and run against demo-scale or larger. Results from a tiny data set mislead.
- **Thresholds fail the run.** A scenario with no `thresholds` block reports numbers nobody acts on.
- **Measure the back end too.** Queue depth, consumer lag, database commands per request, and cache hit ratio, alongside the response times.
- **Separate ramp from steady state** in the report. The ramp tells you about cold caches; the steady state tells you about the budget.
- **Never run against a shared environment without asking.**

## Worked example: the 08:00 attendance peak

```javascript
import http from 'k6/http';
import { check } from 'k6';
import { SharedArray } from 'k6/data';

const sections = new SharedArray('sections', () => JSON.parse(open('./sections.json')));

export const options = {
  scenarios: {
    big_tenant_peak: {
      executor: 'ramping-arrival-rate',
      startRate: 5, timeUnit: '1s',
      preAllocatedVUs: 200, maxVUs: 600,
      stages: [
        { target: 120, duration: '2m' },   // 07:55, everyone arrives
        { target: 120, duration: '6m' },   // the marking window
        { target: 5, duration: '2m' },     // 08:05, it is over
      ],
      env: { TENANT: 'large' },
    },
    small_tenant_normal: {                 // fairness probe, runs throughout
      executor: 'constant-arrival-rate',
      rate: 2, timeUnit: '1s',
      duration: '10m', preAllocatedVUs: 20,
      env: { TENANT: 'small' },
    },
  },
  thresholds: {
    'http_req_duration{scenario:small_tenant_normal}': ['p(95)<400'],
    'http_req_duration{scenario:big_tenant_peak}': ['p(95)<800', 'p(99)<2000'],
    'http_req_failed': ['rate<0.001'],
  },
};

export default function () {
  const s = sections[Math.floor(Math.random() * sections.length)];
  const res = http.post(`${__ENV.BASE_URL}/sections/${s.id}/attendance`,
    JSON.stringify({ present: s.students, clientToken: `${__VU}-${__ITER}` }),
    { headers: { 'Content-Type': 'application/json', 'X-Tenant': __ENV.TENANT } });
  check(res, { 'accepted': (r) => r.status === 202 });
}
```

The point of the second scenario is the threshold on it. If the small tenant's p95 degrades while the large tenant runs its peak, tenant fairness has failed, and that is the finding worth having.