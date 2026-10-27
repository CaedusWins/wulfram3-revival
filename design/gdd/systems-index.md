# Systems Index: Wulfram 3 (Revival)

> **Status**: Approved (owner, 2026-10-26)
> **Created**: 2026-10-26
> **Last Updated**: 2026-10-27
> **Source Concept**: design/gdd/game-concept.md

---

## Overview

A team hovertank game whose core loop is fight → carry cargo → deploy turrets and repair pads →
defend or push, inside a match won by destroying the enemy's tanks and repair pads. The 2018 code
already implements most of the combat and building systems; the L1 gameplay test proves them
offline (`docs/L1-LOCAL-PLAYABILITY.md`). The MVP gaps are a **win condition** and a **team/vehicle
selection** flow. Retrofitted from the existing code (adoption plan step 2b): "Implemented" means
the code exists and its L1 evidence is listed; design docs come with the GDD retrofits (adoption
step 4a).

---

## Systems Enumeration

| # | System Name | Category | Priority | Status | Design Doc | Depends On |
|---|-------------|----------|----------|--------|------------|------------|
| 1 | Hover Movement (`PlayerMovementManager`) | Core | MVP | Implemented (feel: manual playtest, L1 row 3) | [hover-movement.md](hover-movement.md) | Physics, Input |
| 2 | Camera & Cursor (`CameraManager`, `CursorVisibilityManager`) | Core | MVP | Implemented (manual, row 4) | [camera-cursor.md](camera-cursor.md) | Hover Movement |
| 3 | Networking & Match Join (PUN Classic, `LauncherWithLogin`, `GameManager`) | Core | MVP | Implemented (M2 online test; the same-instant join race fixed 2026-10-27: one named room via `JoinOrCreateRoom`, gate 6d) | — | — |
| 4 | Offline Play (`OfflinePlay`) | Core | MVP | Implemented (L1 rows 1/2/16) | — | Networking |
| 5 | Teams (`Unit`, `Unit.SameTeam`, PunTeams) | Core | MVP | Implemented (row 14 team audit) | — | Networking |
| 6 | Health & Regeneration (`HitPointsManager`, `HealthRegenerator`) | Gameplay | MVP | Implemented (row 8; damage only via master) | — | Networking |
| 7 | Fuel (`FuelManager`) | Gameplay | MVP | Implemented (row 7) | — | — |
| 8 | AutoCannon (`AutoCannon`) | Gameplay | MVP | Implemented (row 5) | — | Fuel, Health, Teams |
| 9 | Pulse Cannon (`TryFirePulse`, `PulseShellManager`) | Gameplay | MVP | Implemented (row 6) | — | Fuel, Health |
| 10 | Explosions & Splash (`ExplosionPhysicsForce`) | Gameplay | MVP | Implemented; balance question (uncapped splash) and suspected online bug | — | Health |
| 11 | Targeting (`TargetController`, `TargetInfoController`) | Gameplay | MVP | Implemented (row 9; panel bug known) | — | Teams |
| 12 | Death & Respawn (`GameManager.Respawn`, `RepairPad`) | Gameplay | MVP | Implemented (row 10); own-team pads only, no-pad wait | [win-condition.md](win-condition.md) | Health, Repair Pads |
| 13 | Self-Destruct (`selfDestruct`) | Gameplay | MVP | Implemented (row 11); blast hits enemies only (#43) | [self-destruct.md](self-destruct.md) | Health, Death & Respawn |
| 14 | Cargo (`CargoManager`, `Cargo`) | Gameplay | MVP | Implemented (row 12); all 17 boxes deployable: BlueCargo → BlueGunTurret, Repair Pad fixed (#43) | [cargo.md](cargo.md) | Networking |
| 15 | Deploy / Base Building (`GroundPlacementController`) | Gameplay | MVP | Implemented (row 13); preview rotation and dead/cursor rules fixed 2026-10-27 | [deploy.md](deploy.md) | Cargo, Teams |
| 16 | Turrets (`FlakTurretManager`, `GTManager`) | Gameplay | MVP | Implemented (row 14); splash cap 30, shells hit once (#43); SAM turret has no AI | [turrets.md](turrets.md) | Teams, Health |
| 17 | Repair Pads (respawn points) | Gameplay | MVP | Implemented (row 10) | — | Deploy, Teams |
| 18 | Win Condition (`MatchManager`) | Gameplay | MVP | Implemented (gate 6e `win` offline; gate 6c `match` online, two clients; ADR-0004 Accepted) | [win-condition.md](win-condition.md) | Teams, Repair Pads, Death & Respawn |
| 19 | Team & Vehicle Selection (inferred) | UI | Vertical Slice | Not Started (spawning assigns teams automatically today) | — | Teams, Networking |
| 20 | HUD (`GUIManager`, `StatusBarController`, `TeamCounter`, `PingCounter`) | UI | MVP | Implemented (manual, row 17); Leave button fixed 2026-10-27 | [hud.md](hud.md) | Health, Fuel |
| 21 | Launcher / Login (`LauncherWithLogin`) | UI | MVP | Implemented (guest login; 2018 account backend dead) | — | Networking |
| 22 | Audio (cannon, pickup/drop, UI clicks) | Audio | MVP | Implemented (manual, row 18); own engine loops (fixed 2026-10-27) | [audio.md](audio.md) | — |
| 23 | Chat (Photon Chat UI) | Meta | Alpha | Off (needs its own Photon Chat app) | — | Networking |
| 24 | Scouts (`BlueScout`/`RedScout` prefabs, repair beam) | Gameplay | Alpha | Prefabs exist; nothing spawns them | — | Team & Vehicle Selection |
| 25 | Minimap / Big Map (inferred) | UI | Alpha | A minimap exists (the KGF map system on the HUD, seen in the `pr2-gate` screenshots, 2026-10-27; not yet documented); no Big Map | — | Teams |
| 26 | Economy: SupplyShips, SkyPumps, PowerCells, UpLink | Economy | Full Vision | Not Started (deferred to M4; PowerCell prefab only, UpLink a stub) | — | Cargo, Deploy, Teams |

---

## Categories

| Category | Description | Systems here |
|----------|-------------|--------------|
| **Core** | Foundation everything depends on | 1–5 |
| **Gameplay** | The systems that make the game fun | 6–18, 24 |
| **Economy** | Resource creation and consumption | 26 |
| **UI** | Player-facing displays | 19, 20, 21, 25 |
| **Audio** | Sound | 22 |
| **Meta** | Outside the core loop | 23 |

---

## Priority Tiers

| Tier | Definition | Target Milestone | Design Urgency |
|------|------------|------------------|----------------|
| **MVP** | Required for the core loop (lean team combat online) | M3 closed playtest | Design FIRST: the win condition (18) |
| **Vertical Slice** | One complete, polished match experience | M3 | Design SECOND: team & vehicle selection (19) |
| **Alpha** | All features present in rough form | after M3 | Design THIRD: chat, scouts, minimap |
| **Full Vision** | The economy layer | M4 | As needed |

---

## Dependency Map

### Foundation Layer (no dependencies)
1. Networking & Match Join — every other system syncs through it
2. Fuel — self-contained resource

### Core Layer (depends on foundation)
1. Hover Movement — depends on: Physics, Input
2. Offline Play, Teams, Health & Regeneration — depend on: Networking
3. Camera & Cursor — depends on: Hover Movement (it rides the tank); the input mode it owns is shared with
   movement, weapons and the HUD (Required ADR #4)
4. AutoCannon, Pulse Cannon, Explosions & Splash — depend on: Fuel, Health, Teams
5. Cargo → Deploy — depend on: Networking, Teams

### Feature Layer (depends on core)
1. Targeting — depends on: Teams
2. Turrets, Repair Pads — depend on: Deploy, Teams, Health
3. Death & Respawn, Self-Destruct — depend on: Health, Repair Pads
4. **Win Condition** — depends on: Teams, Repair Pads, Death & Respawn

(Layers match `docs/architecture/architecture.md`; weapons, explosions, cargo and deploy moved to Core,
architecture review 3.)

### Presentation Layer (depends on features)
1. HUD, Launcher, Team & Vehicle Selection, Minimap — depend on the systems they show
2. Chat — depends on: Networking
3. Audio — cosmetic; plays the cues of the systems above it (as in `architecture.md` and `audio.md`)
