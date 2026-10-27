# Architecture Traceability Index
Last Updated: 2026-10-27 (architecture review 3)
Engine: Unity 2017.3.0f3

## Coverage Summary
- Total requirements: 54 (review 2: 28; +25 new in review 3: self-destruct 7, turrets 9, cargo 9; +1 when it was applied: TR-cargo-010)
- Covered: 36 (67%) by Accepted ADRs
- Partial: 10
- Gaps: 8
- "Covered" means an Accepted ADR decides the requirement's architecture. Rows marked "(code: F-n)" have a code
  finding in `docs/architecture/architecture-review-2026-10-27.md`; code conformance is not ADR coverage.

## Full Matrix
| TR-ID | GDD | Requirement | ADR | Status |
|---|---|---|---|---|
| TR-game-concept-001 | game-concept.md | Windows 64-bit standalone | ADR-0001 (implicit) | ⚠️ Partial |
| TR-game-concept-002 | game-concept.md | Two-team online, Photon Cloud, 20 CCU | ADR-0002 | ✅ |
| TR-game-concept-003 | game-concept.md | Offline play, same code paths | ADR-0002 | ✅ |
| TR-game-concept-004 | game-concept.md | App ID out of git | ADR-0002 | ✅ |
| TR-game-concept-005 | game-concept.md | Unity 2017.3 / C# 4 | ADR-0001 | ✅ |
| TR-game-concept-006 | game-concept.md | Playtest stability | ADR-0005 | ✅ |
| TR-game-concept-007 | game-concept.md | Single authority / RPC trust | ADR-0003 (deploy validated since #38; pick-up, drop, damage amount, `SetHealthFromClient` not) | ⚠️ Partial |
| TR-game-concept-008 | game-concept.md | Reliable join / late-join state | — (parked by ADR-0005) | ❌ Gap |
| TR-game-concept-009 | game-concept.md | Online splash damage | ADR-0003 (master applies; victim selection undecided) (code: F1) | ⚠️ Partial |
| TR-game-concept-010 | game-concept.md | Team colour at 100 m | — | ❌ Gap |
| TR-game-concept-011 | game-concept.md | Frame/network budget | — | ❌ Gap |
| TR-win-condition-001 | win-condition.md | Pad team from deployer | ADR-0004, ADR-0003 | ✅ |
| TR-win-condition-002 | win-condition.md | Own-team pads only | ADR-0004, ADR-0003 | ✅ |
| TR-win-condition-003 | win-condition.md | Spectate with no pad; no fallback | ADR-0004 | ✅ |
| TR-win-condition-004 | win-condition.md | Elimination rule | ADR-0004, ADR-0003 | ✅ |
| TR-win-condition-005 | win-condition.md | Win / draw | ADR-0004 | ✅ |
| TR-win-condition-006 | win-condition.md | Master-only result | ADR-0003, ADR-0004 | ✅ |
| TR-win-condition-007 | win-condition.md | Arming | ADR-0004 | ✅ |
| TR-win-condition-008 | win-condition.md | Reset in same room | ADR-0004 | ✅ |
| TR-win-condition-009 | win-condition.md | State machine; no damage when Ended | ADR-0004 | ✅ |
| TR-win-condition-010 | win-condition.md | Hand-over re-evaluation | ADR-0004 (untested online) | ✅ |
| TR-win-condition-011 | win-condition.md | Result within 2 s | ADR-0004 | ✅ |
| TR-win-condition-012 | win-condition.md | Emptied team → Waiting | ADR-0004 (its Decision states Playing → Waiting; raised to ✅ by the owner after review 3) | ✅ Covered |
| TR-win-condition-013 | win-condition.md | Late joiner state | ADR-0003/0004 Risks only | ⚠️ Partial |
| TR-win-condition-014 | win-condition.md | HUD elements | ADR-0004 (placeholder) | ⚠️ Partial |
| TR-win-condition-015 | win-condition.md | Tunable knobs | ADR-0004 (tuning path undecided) | ⚠️ Partial |
| TR-win-condition-016 | win-condition.md | Last-pad sound | — | ❌ Gap |
| TR-win-condition-017 | win-condition.md | Offline + online test coverage | ADR-0005 (gates 6c `match`, 6e `win`) | ✅ |
| TR-self-destruct-001 | self-destruct.md | Owner arms own living tank once; effect on all clients | ADR-0002, ADR-0003 (code: F3, no living check) | ✅ |
| TR-self-destruct-002 | self-destruct.md | Blast hits non-friendly units only, falloff, once per unit | ADR-0003 (`Unit.SameTeam`) (code: F6, per collider) | ✅ |
| TR-self-destruct-003 | self-destruct.md | Blast damage via master; ignored while Ended | ADR-0003, ADR-0004 | ✅ |
| TR-self-destruct-004 | self-destruct.md | Death counts like any other | ADR-0004 | ✅ |
| TR-self-destruct-005 | self-destruct.md | Chain death explosion: not team-filtered, capped | ADR-0003 (routing only) (code: F1) | ⚠️ Partial |
| TR-self-destruct-006 | self-destruct.md | `maximumDamage`, `sphereRadius` tunable | — | ❌ Gap |
| TR-self-destruct-007 | self-destruct.md | Gate 6e verifies AC 1-4 | ADR-0005 (AC4 untested, F8) | ✅ |
| TR-turrets-001 | turrets.md | Targets `teamDetect` via `SameTeam`; never own team | ADR-0003 | ✅ |
| TR-turrets-002 | turrets.md | AI on master; aim/fire visible to all; hand-over | ADR-0003 (shells only) (code: F2a) | ⚠️ Partial |
| TR-turrets-003 | turrets.md | Master spawns flak shells; one hit; direct damage via master | ADR-0003 | ✅ |
| TR-turrets-004 | turrets.md | Splash formula, cap 30, master-applied once per body | ADR-0003 (code: F5, direct `TakeDamage`) | ✅ |
| TR-turrets-005 | turrets.md | Online splash reaches every unit | ADR-0003 (routing only) (code: F1) | ⚠️ Partial |
| TR-turrets-006 | turrets.md | Gun turret hit-scan via master, never own team | ADR-0003 (code/GDD: F2b rate and range) | ✅ |
| TR-turrets-007 | turrets.md | Turrets are room objects (create, destroy, survive builder, reset) | ADR-0003, ADR-0004 | ✅ |
| TR-turrets-008 | turrets.md | Six tuning knobs | — | ❌ Gap |
| TR-turrets-009 | turrets.md | Gate 6e verifies AC 1-4 | ADR-0005 (AC2/AC3 not directly tested, F8) | ✅ |
| TR-cargo-001 | cargo.md | Pick-up/drop are master-applied requests | ADR-0003 (no sender check: TR-game-concept-007) | ✅ |
| TR-cargo-002 | cargo.md | Same box twice → exactly one gets it | ADR-0003 (code: F7, same-frame race) | ✅ |
| TR-cargo-003 | cargo.md | Dropped box is a room object, reset-cleared | ADR-0003, ADR-0004 | ✅ |
| TR-cargo-004 | cargo.md | Master-checked deploy, room object, hold emptied | ADR-0003 (team wording, F4) | ✅ |
| TR-cargo-005 | cargo.md | Placement preview local, no network identity | ADR-0003 | ✅ |
| TR-cargo-006 | cargo.md | Content = prefab name; legacy contents corrected in code | — (ADR-0003 precedent for teams only) | ⚠️ Partial |
| TR-cargo-007 | cargo.md | HUD shows the hold's content | — (and not in code, F9) | ❌ Gap |
| TR-cargo-008 | cargo.md | Pick-up radius tunable 2-10 m | — (hard-coded 5 m) | ❌ Gap |
| TR-cargo-009 | cargo.md | Gate 6e verifies AC 1-3 | ADR-0005 | ✅ |
| TR-cargo-010 | cargo.md | Deployed unit takes the deployer's team, whatever the box colour | ADR-0003 (amended 2026-10-27; code: F4, a story) | ✅ |

## Known Gaps
- TR-game-concept-008: join and late-join state sync ADR (Core; deferred by ADR-0005). Add cargo's unbuffered
  `SetContent` / `SetPickedUpCargo` RPCs to its scope.
- TR-game-concept-011: performance and network budgets ADR (include PUN's 999 scene view-ID limit).
- TR-game-concept-010: team-colour presentation (with a UI/presentation ADR).
- TR-win-condition-016: audio cue (with a presentation ADR).
- TR-cargo-007: HUD hold display (with the HUD/UI ADR, Required ADR #4).
- TR-self-destruct-006, TR-turrets-008, TR-cargo-008 (and partial TR-win-condition-015): a small
  "balance values and tuning path" ADR.
- Partials needing an ADR, not just code: TR-game-concept-009 / TR-turrets-005 / TR-self-destruct-005
  (splash damage online, Required ADR #5, proposed "Must have"); TR-turrets-002 (master-run AI and its sync).
- Structural: 16 of the 21 implemented systems (systems-index rows marked Implemented) still have no GDD, so they have no requirements to trace.

## Superseded Requirements
None. The game-concept and win-condition GDDs are unchanged since review 2; their 28 entries are reused as they are.
Stale *text* in those GDDs (self-destruct friendly fire, flak balance, `BlueCargo` open questions) changes no
registered requirement.

## History
| Date | Covered | Partial | Gaps | Notes |
|---|---|---|---|---|
| 2026-10-26 | 7 (+9 Proposed) | 8 | 4 | Review 1: FAIL (C1) |
| 2026-10-26 | 17 | 7 | 4 | Review 2: CONCERNS (C1 resolved, ADR-0004 Accepted) |
| 2026-10-27 | 34 | 11 | 8 | Review 3: CONCERNS (+25 TR-IDs for self-destruct, turrets, cargo; no ADR change) |
| 2026-10-27 | 36 | 10 | 8 | Owner, applying review 3: TR-win-condition-012 raised to covered (ADR-0004); TR-cargo-010 added (deploy team, ADR-0003 amendment) |
