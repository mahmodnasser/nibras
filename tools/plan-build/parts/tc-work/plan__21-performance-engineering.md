# Test-case work for `docs/plan/21-performance-engineering.md`

## Tests cited but defined nowhere: define them here

- **TC-PERF-023**
  - cited `docs/plan/06-services/attendance.md` line 568: "`PartitionMaintenanceJob` | Monthly, first Sunday 02:00 deployment time | Creates partitions three months ahead for `attendance_records`, `attendance_sessions`, `attendance_record_changes`, `gate_pass"
  - cited `docs/plan/06-services/attendance.md` line 1182: "Load, nightly | Detach under traffic, plans after detach, interrupted detach"
  - cited `docs/plan/21-performance-engineering.md` line 1017: "Interrupted detach | Kills the job between the two transactions of the concurrent detach | The next run finds the partition in the `DETACH PENDING` state and completes it with `ALTER TABLE ... DETACH "
  - cited `docs/plan/21-performance-engineering.md` line 1307: "Partition detach is safe under traffic and plans keep pruning | Weekly on the load tier; nightly on the demo tier for the interrupted case"
- **TC-PERF-021**
  - cited `docs/plan/06-services/attendance.md` line 1182: "Load, nightly | Detach under traffic, plans after detach, interrupted detach"
  - cited `docs/plan/21-performance-engineering.md` line 1015: "Detach under traffic | On the load tier, `DETACH PARTITION CONCURRENTLY` of the oldest `attendance_records` month while a 10-minute N-01-shaped wave runs against the same database | No client waits on"
  - cited `docs/plan/21-performance-engineering.md` line 1019: "**What a detach costs.** Figures are estimates for the scale tier, replaced by the `TC-PERF-021` measurement recorded in `docs/perf/partition-detach.md`."
  - cited `docs/plan/21-performance-engineering.md` line 1307: "Partition detach is safe under traffic and plans keep pruning | Weekly on the load tier; nightly on the demo tier for the interrupted case"
- **TC-PERF-022**
  - cited `docs/plan/21-performance-engineering.md` line 1016: "Plans after detach | Re-runs the hot-query captures of the service whose table was detached | Every plan still prunes; planning time still under 2 ms | Same run"
- **TC-PERF-024**
  - cited `docs/plan/21-performance-engineering.md` line 1260: "Protocol-level prepared statements off in the first release | Section 5; master brief Section 19 "verify prepared-statement compatibility" | Off"
- **TC-PERF-025**
  - cited `docs/plan/21-performance-engineering.md` line 1091: "Interleaved tenants at pool size 2 | PgBouncer with the table's settings but `default_pool_size = 2`, 50 tenants, 2,000 interleaved requests with random delays, reads and `ExecuteUpdateAsync` writes |"
  - cited `docs/plan/21-performance-engineering.md` line 1250: "The PgBouncer pools saturate at the peak, or transaction mode breaks under row-level security and prepared statements | 3 | 4 | Architect | RISK-14"
  - cited `docs/plan/21-performance-engineering.md` line 1306: "Pooling cannot leak a tenant | Every pull request touching persistence; nightly for all services"
- **TC-PERF-026**
  - cited `docs/plan/21-performance-engineering.md` line 1092: "Interrupted transaction | A request killed after `set_config` and before commit, then the next request on the same server connection | The next transaction sees no tenant until it sets its own; PgBoun"
  - cited `docs/plan/21-performance-engineering.md` line 1250: "The PgBouncer pools saturate at the peak, or transaction mode breaks under row-level security and prepared statements | 3 | 4 | Architect | RISK-14"
  - cited `docs/plan/21-performance-engineering.md` line 1306: "Pooling cannot leak a tenant | Every pull request touching persistence; nightly for all services"
