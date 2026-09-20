# Hot queries: `<Service>`

> Copy into section 13 of the service sheet. Every query names its index and its budget, and every budget has a test.

## Queries

| # | Query | Handler | Index | Expected rows | Pagination | Commands | p95 budget |
|---|---|---|---|---|---|---|---|
| 1 | `<what it answers>` | `<Handler>` | `(tenant_id, <cols>) WHERE deleted_at IS NULL` | `<count>` | `<keyset | none>` | `<n>` | `<ms>` |

## Indexes created

| Index | Table | Columns | Partial predicate | Migration |
|---|---|---|---|---|

`tenant_id` is the first column of every index. Soft-deleted rows are excluded by a partial predicate.

## Measurements

| Query | Data set | Plan summary | Rows read | Buffers | Measured p95 | Verdict |
|---|---|---|---|---|---|---|

Measured on `<demo-scale or larger data set>`, captured with `EXPLAIN (ANALYZE, BUFFERS)`.

## Growth

| Query | Grows with | At `<10x>` volume | Mitigation |
|---|---|---|---|

## Budget tests

| Handler | Assertion | Test case ID |
|---|---|---|
| `<Handler>` | Command count `<= n>` and p95 `<= ms>` | `TC-<AREA>-<NNN>` |