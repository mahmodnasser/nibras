# Glossary

Master brief Section 1 holds the product vocabulary. This file adds the terms the kit itself uses.

| Term | Meaning |
|---|---|
| Tenant | One customer organisation, a school or a school group. The isolation boundary |
| School group | **One tenant with several campuses.** Not several tenants. ADR-0011 |
| Campus | A physical branch of a tenant |
| Isolation tier | Shared database, dedicated database, or dedicated deployment. The same code serves all three |
| Isolation level S | The stricter handling Wellbeing data receives: its own database, its own database role, its own encryption key |
| Assist rung | How intelligent a feature is. 1 rules, 2 classical model, 3 local language model, 4 external provider. Master brief Section 25 |
| Because panel | The explanation shown beside any automated decision, with an override that records a reason |
| Signature feature | A feature with a named persona moment that can be demonstrated in under sixty seconds. Appendix W |
| Partition key | The envelope field that makes events about one subject arrive in order |
| Local reference copy | A slim read-only copy of another service's data, kept current by events and reconciled nightly |
| Permission version | The counter carried in a token that makes a permission change take effect everywhere within seconds |
| Break-glass | Emergency access to a restricted record. Needs a reason, alerts the principal, always logged |
| Delta token | The signed checkpoint a mobile device sends to receive only what changed since last time |
| Golden path | The fifteen-minute demo in Appendix O, which is also the release gate |
| Residency region | A whole deployment. A tenant is pinned to one at provisioning and its data never leaves it |
| Kit lint | `tools/kit-lint`, which proves the documents agree with each other |
| Rung 1 complete | The property that the product is fully usable with every model switched off |
