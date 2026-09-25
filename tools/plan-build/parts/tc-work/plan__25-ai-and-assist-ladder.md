# Test-case work for `docs/plan/25-ai-and-assist-ladder.md`

## Collisions to resolve here (the owner keeps the identifier)

- **TC-PLT-026** at line 255: "`platform.tenant.deleted.v1` | Delete every row for the tenant"
  - owner `docs/brief/02-appendices/appendix-r-workflow-catalog.md` line 506: "DeletionExecuted to Certified | All service schemas and files removed | Certificate issued with scope, timestamp, and operator"
- **TC-SEC-320** at line 278: "Retrieved content is data, never instructions | Templates place retrieved text inside a delimited, labelled data block; the system message states that the block carries no instructions; a tool call requested from inside "
  - owner `docs/plan/12-security-privacy-safety.md` line 287: "T-AI-01 | Assistant prompt | Tampering | Prompt injection through a retrieved document or message body | high | high | Retrieved content is data never instructions; tool calls limited to the caller's permissions; `AI_PRO"
- **TC-SEC-321** at line 252: "`identity.permissions.changed.v1` | Nothing in the index; permissions apply at query time from the caller's token and are never baked into rows beyond the source's required permission"
  - owner `docs/plan/12-security-privacy-safety.md` line 288: "T-AI-02 | Retrieval | Information disclosure | Natural-language query reaches rows outside the caller's scope | med | high | Every embedding carries tenant, scope and source version; filter before rank; `AI_SCOPE_VIOLATI"
- **TC-SEC-321** at line 279: "Tools limited to the caller's permissions | The function list offered to the model is built per request from the caller's token; each tool executes under that token through `POST /bff/web/v1/internal/ai/tools/{toolName}`"
  - owner `docs/plan/12-security-privacy-safety.md` line 288: "T-AI-02 | Retrieval | Information disclosure | Natural-language query reaches rows outside the caller's scope | med | high | Every embedding carries tenant, scope and source version; filter before rank; `AI_SCOPE_VIOLATI"
- **TC-SEC-322** at line 282: "Level S never in context | Prompt assembly rejects any slot tagged Sensitive or S | `AI_SENSITIVE_CONTEXT_REFUSED`"
  - owner `docs/plan/12-security-privacy-safety.md` line 289: "T-AI-03 | Indexing | Information disclosure | Level-S content indexed | low | critical | Ai never consumes Wellbeing events with detail (BR-WEL-003) and refuses level-S context with `AI_SENSITIVE_CONTEXT_REFUSED`"
- **TC-SEC-325** at line 250: "`school.student.status-changed.v1` to withdrawn | Delete every chunk whose `scope_student_ids` contains the student, in the consumer's transaction"
  - owner `docs/plan/12-security-privacy-safety.md` line 292: "T-AI-06 | Withdrawn student | Information disclosure | Embeddings survive after withdrawal | med | high | `school.student.status-changed.v1` to withdrawn purges the index"
