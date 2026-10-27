# Architecture Traceability Index
Last Updated: 2026-10-27 (architecture review 4)
Engine: Unity 2017.3.0f3

## Coverage Summary
- Total requirements: 81 (review 3 as applied: 54; +27 new in review 4: hover-movement 6, camera-cursor 5, deploy 5, hud 7, audio 4)
- Covered: 46 (57%) by Accepted ADRs
- Partial: 16
- Gaps: 19
- "Covered" means an Accepted ADR decides the requirement's architecture. Rows marked "(code: F-n)" have a code
  finding in `docs/architecture/architecture-review-2026-10-27.md`, rows marked "(G-n)" one in
  `docs/architecture/architecture-review-2026-10-27-2.md`; code conformance is not ADR coverage.

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
| TR-hover-movement-001 | hover-movement.md | Owner-authoritative movement; others kinematic via PUN view sync | ADR-0003 (amended 2026-10-27: owner-authoritative movement), ADR-0002 (view sync) | ✅ Covered |
| TR-hover-movement-002 | hover-movement.md | Rigidbody hover/drive/jump/land in the physics step; terrain-only ray | ADR-0001 (PhysX, `UnityEngine.Input`; local only) | ✅ |
| TR-hover-movement-003 | hover-movement.md | Input only for the owner's living tank with the cursor hidden; death timer | ADR-0004 (respawn flow); input mode undecided (TR-camera-cursor-002, G1) | ⚠️ Partial |
| TR-hover-movement-004 | hover-movement.md | Frame-rate independence | — (code: hover height per frame) | ❌ Gap |
| TR-hover-movement-005 | hover-movement.md | Five tuning knobs | — | ❌ Gap |
| TR-hover-movement-006 | hover-movement.md | Gate 6e (AC2, AC5) + playtest (AC1, 3, 4) | ADR-0005 (AC2 gate evidence weak, G10) | ✅ |
| TR-camera-cursor-001 | camera-cursor.md | Main camera driven only by the local tank; cockpit/chase swap | ADR-0001, ADR-0002 (`isMine`); local presentation | ✅ |
| TR-camera-cursor-002 | camera-cursor.md | One input mode (hidden = drive, visible = UI) followed by all gameplay input and the clickable UI | — (G1) | ❌ Gap |
| TR-camera-cursor-003 | camera-cursor.md | Overhead camera + cursor at the respawn flow; restored at respawn | ADR-0004 (respawn flow only; G7 timing) | ⚠️ Partial |
| TR-camera-cursor-004 | camera-cursor.md | `transitionTime`, positions tunable | — | ❌ Gap |
| TR-camera-cursor-005 | camera-cursor.md | Gate 6e respawn (AC4) + playtest A3/A4 | ADR-0005 | ✅ |
| TR-deploy-001 | deploy.md | Local preview at the deploy point, follows the tank; cancel keeps cargo | ADR-0003 (N2 preview) (G5a) | ✅ |
| TR-deploy-002 | deploy.md | 10° per wheel notch, still between notches; request carries rotation | ADR-0003 (`DeployUnit(…, position, rotation)`) (code: spun; fixed in #50, gate 6e `deploy-rotate`) | ✅ |
| TR-deploy-003 | deploy.md | No deploy with the cursor visible or from a dead tank | ADR-0003 check list lacks a living check (G4; client check fixed in #50, master check deferred to Required ADR #2); cursor via TR-camera-cursor-002 | ⚠️ Partial |
| TR-deploy-004 | deploy.md | Rotation step 5-45° tunable | — | ❌ Gap |
| TR-deploy-005 | deploy.md | Gate 6e deploy, deploy-team, deploy-refused, 6c match; AC4/5 checks with the fix | ADR-0005 | ✅ |
| TR-hud-001 | hud.md | Own-tank hull and fuel bars | — (data: ADR-0003; display: Required ADR #4) | ❌ Gap |
| TR-hud-002 | hud.md | Team player counts, correct offline | — (data: ADR-0002; G11) | ❌ Gap |
| TR-hud-003 | hud.md | Ping and FPS every ~0.6 s | — | ❌ Gap |
| TR-hud-004 | hud.md | Target panel; recovers on re-entry | — (L1 row 9 bug) | ❌ Gap |
| TR-hud-005 | hud.md | Leave Game shows with the cursor visible; leaves to the launcher | ADR-0002 (leave path); visibility via TR-camera-cursor-002 | ⚠️ Partial |
| TR-hud-006 | hud.md | Plurals, no overlap, crosshair, centre clear, red/blue | — | ❌ Gap |
| TR-hud-007 | hud.md | Playtest A14/A17; gates 6e win, 6c match | ADR-0005 | ✅ |
| TR-audio-001 | audio.md | Combat cues 3D on every client in range | ADR-0002/0003 (replication only; no audio ADR) | ⚠️ Partial |
| TR-audio-002 | audio.md | Owner-only feedback cues | ADR-0001; cargo cues on ADR-0003's `SetPickedUpCargo` | ✅ |
| TR-audio-003 | audio.md | Engine hums while hovering, silent when landed, on every client | — (landed state not synced, G8) | ❌ Gap |
| TR-audio-004 | audio.md | Playtest only (A2, A16, B2) | ADR-0005 decides the gate, not the playtest (G14) | ⚠️ Partial |

## Known Gaps
- TR-game-concept-008: join and late-join state sync ADR (Core; deferred by ADR-0005). Add cargo's unbuffered
  `SetContent` / `SetPickedUpCargo` RPCs to its scope.
- TR-game-concept-011: performance and network budgets ADR (include PUN's 999 scene view-ID limit). Add
  TR-hover-movement-004 (frame-rate independence) as a control-manifest rule from it, and review 4 G12.
- TR-game-concept-010: team-colour presentation (with the HUD/UI ADR).
- TR-win-condition-016: audio cue (with a presentation ADR).
- TR-cargo-007, TR-hud-001, -002, -003, -004, -006: HUD display (Required ADR #4, HUD and UI architecture).
- TR-camera-cursor-002: the input mode (driving vs UI); widen Required ADR #4 to "HUD, UI and input mode" (review 4
  G1). TR-hover-movement-003, TR-deploy-003 and TR-hud-005 are partial until it exists.
- TR-audio-003: the landed state must reach other clients for the engine sound (with Required ADR #6, room-object
  and presentation state sync, or a small audio ADR).
- TR-self-destruct-006, TR-turrets-008, TR-cargo-008, TR-hover-movement-005, TR-camera-cursor-004, TR-deploy-004
  (and partial TR-win-condition-015): a small "balance values and tuning path" ADR.
- Partials needing an ADR, not just code: TR-game-concept-009 / TR-turrets-005 / TR-self-destruct-005 (splash
  damage online, Required ADR #5, Must have before M3); TR-turrets-002 (master-run AI and its sync);
  TR-hover-movement-001 (ADR-0003 amendment: owner-authoritative movement, review 4 G2); TR-deploy-003 (ADR-0003
  amendment: the master refuses a dead tank's deploy, review 4 G4).
- Structural: 11 of the 21 implemented systems (systems-index rows marked Implemented) still have no GDD, so they
  have no requirements to trace.

## Superseded Requirements
None. The six GDDs registered before review 4 changed only in text that review 3 proposed; their 54 entries are
reused as they are. Requirements in the five new GDDs that an earlier entry already states (TR-cargo-004, -005,
-007, -010; TR-win-condition-014, -016; TR-game-concept-010) are referenced, not registered again.

## History
| Date | Covered | Partial | Gaps | Notes |
|---|---|---|---|---|
| 2026-10-26 | 7 (+9 Proposed) | 8 | 4 | Review 1: FAIL (C1) |
| 2026-10-26 | 17 | 7 | 4 | Review 2: CONCERNS (C1 resolved, ADR-0004 Accepted) |
| 2026-10-27 | 34 | 11 | 8 | Review 3: CONCERNS (+25 TR-IDs for self-destruct, turrets, cargo; no ADR change) |
| 2026-10-27 | 36 | 10 | 8 | Owner, applying review 3: TR-win-condition-012 raised to covered (ADR-0004); TR-cargo-010 added (deploy team, ADR-0003 amendment) |
| 2026-10-27 | 45 | 17 | 19 | Review 4: CONCERNS (+27 TR-IDs for hover-movement, camera-cursor, deploy, hud, audio; no ADR change) |
| 2026-10-27 | 46 | 16 | 19 | Owner, applying review 4: ADR-0003 amended (owner-authoritative movement, owner-spawned pulse shells); TR-hover-movement-001 covered |
