# Architecture Traceability Index
Last Updated: 2026-10-26 (architecture review 2)
Engine: Unity 2017.3.0f3

## Coverage Summary
- Total requirements: 28
- Covered: 17 (61%) by Accepted ADRs (review 1: 7 covered + 9 on Proposed ADR-0004)
- Partial: 7
- Gaps: 4

## Full Matrix
| TR-ID | GDD | Requirement | ADR | Status |
|---|---|---|---|---|
| TR-game-concept-001 | game-concept.md | Windows 64-bit standalone | ADR-0001 (implicit) | ⚠️ Partial |
| TR-game-concept-002 | game-concept.md | Two-team online, Photon Cloud, 20 CCU | ADR-0002 | ✅ |
| TR-game-concept-003 | game-concept.md | Offline play, same code paths | ADR-0002 | ✅ |
| TR-game-concept-004 | game-concept.md | App ID out of git | ADR-0002 | ✅ |
| TR-game-concept-005 | game-concept.md | Unity 2017.3 / C# 4 | ADR-0001 | ✅ |
| TR-game-concept-006 | game-concept.md | Playtest stability | ADR-0005 | ✅ |
| TR-game-concept-007 | game-concept.md | Single authority / RPC trust | ADR-0003 (trust model deferred; `RequestDeploy` unvalidated) | ⚠️ Partial |
| TR-game-concept-008 | game-concept.md | Reliable join / late-join state | — (parked by ADR-0005) | ❌ Gap |
| TR-game-concept-009 | game-concept.md | Online splash damage | ADR-0003 (routing only) | ⚠️ Partial |
| TR-game-concept-010 | game-concept.md | Team colour at 100 m | — | ❌ Gap |
| TR-game-concept-011 | game-concept.md | Frame/network budget | — | ❌ Gap |
| TR-win-condition-001 | win-condition.md | Pad team from deployer | ADR-0004, ADR-0003 (room object, team as instantiation data) | ✅ |
| TR-win-condition-002 | win-condition.md | Own-team pads only | ADR-0004, ADR-0003 | ✅ |
| TR-win-condition-003 | win-condition.md | Spectate with no pad; no fallback | ADR-0004 | ✅ |
| TR-win-condition-004 | win-condition.md | Elimination rule | ADR-0004, ADR-0003 (C1 resolved, gate 6c `ro-gate2`) | ✅ |
| TR-win-condition-005 | win-condition.md | Win / draw | ADR-0004 | ✅ |
| TR-win-condition-006 | win-condition.md | Master-only result | ADR-0003, ADR-0004 | ✅ |
| TR-win-condition-007 | win-condition.md | Arming | ADR-0004 | ✅ |
| TR-win-condition-008 | win-condition.md | Reset in same room | ADR-0004 | ✅ |
| TR-win-condition-009 | win-condition.md | State machine; no damage when Ended | ADR-0004 | ✅ |
| TR-win-condition-010 | win-condition.md | Hand-over re-evaluation | ADR-0004 (untested online) | ✅ |
| TR-win-condition-011 | win-condition.md | Result within 2 s | ADR-0004 | ✅ |
| TR-win-condition-012 | win-condition.md | Emptied team → Waiting | ADR-0004 (implied, not stated) | ⚠️ Partial |
| TR-win-condition-013 | win-condition.md | Late joiner state | ADR-0003/0004 Risks only | ⚠️ Partial |
| TR-win-condition-014 | win-condition.md | HUD elements | ADR-0004 (placeholder) | ⚠️ Partial |
| TR-win-condition-015 | win-condition.md | Tunable knobs | ADR-0004 (undecided) | ⚠️ Partial |
| TR-win-condition-016 | win-condition.md | Last-pad sound | — | ❌ Gap |
| TR-win-condition-017 | win-condition.md | Offline + online test coverage | ADR-0005 (gates 6c `match`, 6e `win`) | ✅ |

## Known Gaps
- TR-game-concept-008: join and late-join state sync ADR (Core layer; deferred by ADR-0005)
- TR-game-concept-011: performance and network budgets ADR (include PUN's 999 scene view-ID limit)
- TR-game-concept-010: team-colour presentation (with a UI/presentation ADR)
- TR-win-condition-016: audio cue (with a presentation ADR)
- Structural: 18 implemented systems have no GDD, so they have no requirements to trace.

## Superseded Requirements
None.

## History
| Date | Covered | Partial | Gaps | Notes |
|---|---|---|---|---|
| 2026-10-26 | 7 (+9 Proposed) | 8 | 4 | Review 1: FAIL (C1) |
| 2026-10-26 | 17 | 7 | 4 | Review 2: CONCERNS (C1 resolved, ADR-0004 Accepted) |
