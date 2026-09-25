# Test-case work for `docs/plan/06-services/bff-mobile.md`

## Collisions to resolve here (the owner keeps the identifier)

- **TC-MOB-003** at line 477: "End to end | Teacher five-minute home shows attendance, a quick note, a quick grade, the cover alert and nothing else (REQ-BFF-007)"
  - owner `docs/brief/02-appendices/appendix-w-feature-register.md` line 52: "34 | Teacher five-minute mode | "Four taps and I was teaching" | 1 | 1 surfaces | 1 | Bff.Mobile"
- **TC-MOB-004** at line 478: "End to end | Parent calm screen with one card per child and the designed empty state (REQ-BFF-008)"
  - owner `docs/brief/02-appendices/appendix-w-feature-register.md` line 53: "35 | Parent calm screen | "Nothing needed me today, and it said so" | 1 | 1 surfaces | 1 | Bff.Mobile"
- **TC-MOB-005** at line 479: "End to end | Low-bandwidth mode (REQ-BFF-009)"
  - owner `docs/brief/02-appendices/appendix-w-feature-register.md` line 62: "44 | Low-bandwidth mode | "It worked on my old phone" | 1 | 1 surfaces | 1 | Bff.Mobile"
- **TC-MOB-713** at line 483: "Integration | Version policy blocks, nags and preserves the outbox"
  - owner `docs/plan/09-mobile-structure.md` line 766: "Minimum version enforcement | Bff.Mobile `/config/version` returns `minimumVersion`, `recommendedVersion` and `policy` per tenant (Appendix G, Mobile settings). Below minimum: the app blocks with the upgrade screen and t"
- **TC-MOB-714** at line 484: "Integration | Sign-out unregisters the push token and revokes the delta tokens"
  - owner `docs/plan/09-mobile-structure.md` line 768: "Sign-out and permission loss | Drops the Drift database, clears secure storage for that person, unregisters the push token (`NOTIFICATION_DEVICE_TOKEN_INVALID` is expected afterwards), keeps nothing but the flavor config"
