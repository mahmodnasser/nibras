# Test-case work for `docs/plan/06-services/audit.md`

## Collisions to resolve here (the owner keeps the identifier)

- **TC-AUD-001** at line 608: "The audit test: any sensitive action in the Appendix T simulation can be traced and any number explained | End to end"
  - owner `docs/brief/02-appendices/appendix-w-feature-register.md` line 32: "14 | Trust by design: audit, consent, export, erasure | "We can answer any question about any record" | 1 | 1 surfaces | 1 | Audit"
- **TC-AUD-002** at line 609: "Guardian transparency shows reader roles and times, no names, no Wellbeing content | End to end, Appendix O minute 11"
  - owner `docs/brief/02-appendices/appendix-w-feature-register.md` line 49: "31 | Guardian transparency on sensitive access | "I can see who looked at my child's file" | 1 | 1 surfaces | 1 | Audit"
- **TC-SEC-270** at line 611: "An insider edit or delete is refused by the role and caught by the chain | Integration"
  - owner `docs/plan/12-security-privacy-safety.md` line 245: "T-AUD-01 | Entries table | Tampering | An insider with database access edits or deletes an entry | low | high | Append-only role for `svc_audit`, hash chain, nightly `audit.integrity-check.failed.v1`, `AUDIT_IMMUTABLE_RE"
- **TC-SEC-271** at line 612: "A write whose audit event is lost fails a sensitive read in the same transaction | Integration"
  - owner `docs/plan/12-security-privacy-safety.md` line 246: "T-AUD-02 | Audit consumer | Repudiation | A write completes while its audit event is lost | med | high | Outbox on the producer; `AUDIT_WRITE_FAILED` fails a sensitive read in the same transaction (Appendix J rule 8)"
- **TC-SEC-272** at line 613: "The access log shows Wellbeing subjects by existence and record type only | Integration"
  - owner `docs/plan/12-security-privacy-safety.md` line 248: "T-AUD-04 | Access log | Information disclosure | Access log read to learn who is under safeguarding review | low | critical | `audit.access-log.view` is high risk; wellbeing subjects are shown by existence with the recor"
- **TC-SEC-273** at line 614: "A detached partition altered in cold storage fails re-verification on restore | Integration"
  - owner `docs/plan/12-security-privacy-safety.md` line 249: "T-AUD-05 | Cold partitions | Tampering | Detached partition altered in cold storage | low | med | Partition hash recorded on detach; `audit.retention.partition-detached.v1` carries the hash; restore re-verifies"
- **TC-SEC-904** at line 615: "Audit export is refused without the high-risk permission and an approved reason | UAT"
  - owner `docs/plan/12-security-privacy-safety.md` line 247: "T-AUD-03 | Export | Information disclosure | Audit export used to harvest before-and-after values | low | high | `audit.entries.export` is high risk with reason and four-eyes; reported to the principal"
- **TC-PRV-068** at line 617: "Monthly detach of audit partitions to cold storage with a hash | Integration"
  - owner `docs/plan/12-security-privacy-safety.md` line 695: "Attendance, notifications, messages, audit | `PartitionMaintenanceJob` monthly | Each owning service's worker or job host | Detach the month partition; audit partitions go to cold storage with a hash | Data Quality Cente"
- **TC-PERF-016** at line 618: "Audit writes zero cache keys | Integration"
  - owner `docs/plan/21-performance-engineering.md` line 339: "Never-cached audit | Full run of the demo seed through every screen | No key contains a value tagged with a classification above Confidential; Audit and Wellbeing write zero cache keys"
