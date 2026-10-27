# Wulfram 3 (Revival) — Master Architecture

## Document Status
- Version: 1
- Last Updated: 2026-10-27 (architecture review 3)
- Engine: Unity 2017.3.0f3 (C# 4, Mono), PUN Classic 1.88 on Photon Cloud
- Workflow tier: `standard` (system layer map + critical ADR list; module ownership, data flow and API boundaries are left to the ADRs)
- GDDs Covered: game-concept.md, systems-index.md (26 systems), win-condition.md, self-destruct.md, turrets.md, cargo.md, hover-movement.md, camera-cursor.md, deploy.md, hud.md, audio.md
- ADRs Referenced: ADR-0001, ADR-0002, ADR-0003, ADR-0004, ADR-0005, ADR-0006 (all Accepted)
- Retrofitted: the architecture describes the 2018 code base as it is today, plus the decisions the ADRs record
- TD-ARCHITECTURE: 2026-10-26 — skipped — Lean mode
- LP-FEASIBILITY: 2026-10-26 — skipped — Lean mode

## Engine Knowledge Gap Summary

The usual gap is reversed here (`docs/engine-reference/unity/VERSION.md`): Unity 2017.3 is well inside the
model's training, so no domain is HIGH or MEDIUM risk for missing knowledge. The real risk is suggesting
APIs from *newer* Unity versions that don't exist in 2017.3. ADR-0001 and the control manifest's
Forbidden APIs list guard it, and the compile step catches what slips through. The `breaking-changes.md`,
`deprecated-apis.md`, `current-best-practices.md` and `modules/` files describe Unity 6.3 and do not apply.

| Domain | Risk | Notes |
|--------|------|-------|
| Networking (PUN Classic 1.88) | LOW, verify against source | A less common library; behaviour claims are checked against the vendored source in `Assets/Photon Unity Networking/Plugins/PhotonNetwork` (ADR-0004 did this) |
| Physics, Input, UI, Rendering, Audio | LOW | 2017.3 APIs only (`UnityEngine.Input`, UGUI/`OnGUI`, built-in pipeline) |

## System Layer Map

```text
┌───────────────────────────────────────────────────────────────────────────┐
│ PRESENTATION   Team & Vehicle Selection (19) · HUD (20) · Launcher/Login  │
│                (21) · Audio (22) · Chat (23) · Minimap / Big Map (25)      │
├───────────────────────────────────────────────────────────────────────────┤
│ FEATURE        Targeting (11) · Death & Respawn (12) · Self-Destruct (13)  │
│                · Turrets (16) · Repair Pads (17) · Win Condition (18)      │
│                · Scouts (24) · Economy (26, deferred to M4)                │
├───────────────────────────────────────────────────────────────────────────┤
│ CORE           Hover Movement (1) · Camera & Cursor (2) · Teams (5)        │
│                · Health & Regeneration (6) · Fuel (7) · AutoCannon (8)     │
│                · Pulse Cannon (9) · Explosions & Splash (10) · Cargo (14)  │
│                · Deploy (15)                                               │
├───────────────────────────────────────────────────────────────────────────┤
│ FOUNDATION     Networking & Match Join (3) · Offline Play (4)              │
│                (+ the verification gate, ADR-0005: tooling, not a system)  │
├───────────────────────────────────────────────────────────────────────────┤
│ PLATFORM       Unity 2017.3 · PUN Classic 1.88 · Photon Cloud · Windows 64 │
└───────────────────────────────────────────────────────────────────────────┘
```

Numbers are the rows of `design/gdd/systems-index.md`.

| Layer | System | Main code | Owns | Governing ADRs |
|-------|--------|-----------|------|----------------|
| Foundation | Networking & Match Join (3) | `LauncherWithLogin`, `GameManager` (join, spawn), `PhotonCloudSettings` | room connection, App ID resolution, scene sync | ADR-0002 |
| Foundation | Offline Play (4) | `OfflinePlay` | `PhotonNetwork.offlineMode` entry | ADR-0002 |
| Core | Hover Movement (1) | `PlayerMovementManager` | the local tank's motion, death timer | ADR-0001; ADR-0003 (owner-authoritative movement) |
| Core | Camera & Cursor (2) | `CameraManager`, `CursorVisibilityManager` | the main camera's parent and view; the input mode (driving vs UI) is shared: Required ADR #4 | ADR-0001 |
| Core | Teams (5) | `Unit` (`SameTeam`, `NormalizeTeam`), PunTeams | team identity of players and units | ADR-0003 |
| Core | Health & Regeneration (6) | `HitPointsManager`, `HealthRegenerator` | unit health (master-written) | ADR-0003 |
| Core | Fuel (7) | `FuelManager` | the tank's fuel pool | — |
| Core | AutoCannon (8) | `AutoCannon` | hit-scan fire, fuel cost | ADR-0003 |
| Core | Pulse Cannon (9) | `PlayerMovementManager.TryFirePulse`, `PulseShellManager` | pulse shell spawn (owner); damage via the master | ADR-0003 |
| Core | Explosions & Splash (10) | `ExplosionPhysicsForce` | splash damage | ADR-0003 (damage routing; victim selection: Required ADR #5) |
| Core | Cargo (14) | `CargoManager`, `Cargo`, `GameManager.RequestPickUpCargo` / `RequestDropCargo` | hold contents; dropped boxes are room objects | ADR-0003 |
| Core | Deploy (15) | `GroundPlacementController`, `GameManager.DeployUnit` / `RequestDeploy` | local preview; master-checked room-owned units | ADR-0003 |
| Feature | Targeting (11) | `TargetController`, `TargetInfoController` | the local target | ADR-0003 (teams) |
| Feature | Death & Respawn (12) | `GameManager.Respawn`/`Update`, `RepairPad` | own-team respawn, spectating with no pad | ADR-0004 |
| Feature | Self-Destruct (13) | `selfDestruct` | the blast | ADR-0003 |
| Feature | Turrets (16) | `FlakTurretManager`, `GTManager` | turret AI | ADR-0003 (teams, master-applied damage; master-run AI: Required ADR #6) |
| Feature | Repair Pads (17) | `RepairPad` | pad team, respawn point | ADR-0003, ADR-0004 |
| Feature | Win Condition (18) | `MatchManager` | match state (master-decided), reset | ADR-0004 |
| Feature | Scouts (24) | prefabs only | — | — |
| Feature | Economy (26) | not built (M4) | — | — |
| Presentation | Team & Vehicle Selection (19) | not built | — | — |
| Presentation | HUD (20) | `GUIManager`, `StatusBarController`, `TeamCounter`, `TargetInfoController`, `PlayerInfoController` (name tags), `MatchManager.OnGUI` (`PingCounter` is in no scene) | screen display only | — |
| Presentation | Launcher / Login (21) | `LauncherWithLogin` | launcher UI | ADR-0002 |
| Presentation | Audio (22) | audio sources on prefabs | — | — |
| Presentation | Chat (23) | Photon Chat UI (off) | — | — |
| Presentation | Minimap / Big Map (25) | not built (a map asset, `Assets/kolmich/KGFMapSystem`, is vendored; its use is unverified) | — | — |

Layering rule: a layer may call down, never up. Upward notifications go through PUN RPCs and Unity
callbacks (`OnPhotonPlayerPropertiesChanged`, health updates through `GameManager.UnitsHealthUpdated`),
which is how the 2018 code already works.

## Module Ownership, Data Flow, API Boundaries

Not authored at the `standard` tier. The binding contracts that exist are recorded in the ADRs and in
`docs/registry/architecture.yaml`: unit health (master), match state (`MatchManager`), the damage and heal
RPCs, the deploy request, the match-result and reset RPCs, and team comparison.

## ADR Audit

| ADR | Layer | Status | Engine compatibility | GDD requirements |
|-----|-------|--------|----------------------|------------------|
| ADR-0001 engine pin | Foundation | Accepted | present | game-concept |
| ADR-0002 PUN Classic on Photon Cloud | Foundation | Accepted | present | game-concept, systems index #3/#4 |
| ADR-0003 master authority, teams, room-owned units | Core | Accepted | present | systems index #5/#6, win-condition |
| ADR-0004 match flow | Feature | Accepted | present | win-condition |
| ADR-0005 gate, branches, local first | Foundation | Accepted | present | game-concept |
| ADR-0006 HUD, UI and input mode | Core / Presentation | Accepted | present | camera-cursor, hover-movement, deploy, hud, win-condition, cargo |

Traceability: 81 requirements, 46 covered, 16 partial, 19 gaps (`docs/architecture/requirements-traceability.md`,
architecture review 4 as applied by the owner: TR-hover-movement-001 covered by the ADR-0003 amendment). 11
implemented systems have no GDD yet, so they have no requirements to trace.

## Required ADRs

**Must have before more Foundation/Core code:**
1. **Join and late-join state sync** — the same-instant join race (fixed 2026-10-27: one named room via `JoinOrCreateRoom`, gate 6d), late joiners reading remote health as 0, cached destroy events for scene objects (TR-game-concept-008, TR-win-condition-013), and cargo content and hold state for late joiners (`SetContent`, `SetPickedUpCargo` are unbuffered). Deferred by ADR-0005's local-first rule until L1 is done.
2. **RPC trust model** — which client requests the master validates (`SetHealthFromClient`, damage, pickups), and how (TR-game-concept-007): pick-up/drop sender checks, damage-amount checks, deploy position limits. M3.
3. **Performance and network budgets** — frame budget, Photon message rate for about 10v10, the 999 scene view IDs, frame-rate independence (TR-game-concept-011, TR-hover-movement-004, review N7).

**Must have before the M3 online playtest** (raised by the owner after architecture review 3):
5. **Splash damage online** — who selects splash victims and how damage is routed (TR-game-concept-009, TR-turrets-005, TR-self-destruct-005). `ExplosionPhysicsForce` only collects bodies the client controls and only the master applies damage, so no client ever splashes a non-master player's tank; it also calls `TakeDamage` directly (review 3 F1, F5). Explosions & Splash is Core, MVP, and three approved GDDs depend on it.

**Should have before the relevant system is built:**
4. ~~**HUD, UI and input mode**~~ **Written: ADR-0006, Accepted 2026-10-27** (the traceability index picks it up at the next architecture review) — one owner for the input mode (driving vs UI, `Cursor.visible` today: three writers, six readers); UGUI vs `OnGUI`, the match banner, team colour presentation, the hold's content (TR-camera-cursor-002, TR-hud-001..006, TR-win-condition-014, TR-game-concept-010, TR-cargo-007). Owner, 2026-10-27: write it next, before the epics and stories.
6. **Master-run AI and room-object behaviour sync** (TR-turrets-002, TR-audio-003) — AI runs on the master; what non-masters see (including a tank's landed state, for its engine sound); hand-over. Planned as an ADR-0003 amendment.
7. **Balance values and tuning path** (TR-self-destruct-006, TR-turrets-008, TR-cargo-008, TR-win-condition-015, TR-hover-movement-005, TR-camera-cursor-004, TR-deploy-004) — where balance values live and how they are tuned; a short ADR before the balance pass after the playtest.

**Can defer to implementation:**
8. Audio cues (TR-win-condition-016, TR-audio-001), Chat (needs its own Photon Chat app), Economy (M4).

## Architecture Principles

1. **One authority:** the master client decides every shared outcome; other clients request (ADR-0003).
2. **Shared things belong to the room:** units other players can damage or use are room objects created by the master (ADR-0003).
3. **State comes from the scene:** match-level state is recomputed from what exists, so a master hand-over loses nothing (ADR-0004).
4. **One code path:** offline play runs the same code as online (ADR-0002); behaviour that crosses clients is still proven online (gate 6c).
5. **Code over binary assets:** behaviour is added in reviewable code (e.g. `MatchManager` added at runtime), not by editing binary scenes (ADR-0004).
6. **Pinned engine:** Unity 2017.3 and C# 4 until a deliberate engine step (ADR-0001).

## Open Questions

- When does the engine step (Unity 2017.4 LTS for PUN2) happen? After the MVP playtest (ADR-0001).
- Should Scouts (24) be built before Team & Vehicle Selection (19), which they depend on?
