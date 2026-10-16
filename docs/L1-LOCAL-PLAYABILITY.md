# L1 — Local Playability

**Decision (2026-10-16, project owner):** before any more online development, the game must play end to end **locally**, on one machine, with no server, in both a built player and the Unity editor. Every step happens on its own siloed branch and gets tested before it moves up: `feature/*` → `revival/phase-0-1-bringup` → `dev` → (much later) `master`.

**Why local first:** online play multiplies the unknowns (network, timing, master-client authority, Photon limits). A bug found while playing online could be a gameplay bug or a network bug. Proving the game itself first means any later online failure is a network problem by elimination.

**One caveat to keep in mind:** PUN offline mode runs every RPC locally *without serializing its arguments*. Some code that works offline therefore cannot work online. Suspected example: the cargo RPCs pass a `CargoManager` component as an RPC argument, and Photon cannot send components. L1 includes a static check for this bug class (no online work needed), and the gate keeps its existing online steps (6c/6d) as a **regression canary** only: no new online features until L1 is done.

## Scope

"Plays perfectly" means **every feature the 2018 code actually implements works**, without exceptions or errors, through the real controls. It does *not* mean the full game described in `README.md`: Wulfram3 stopped as an early alpha ("1.6 alpha"). The manual's features that were never built are listed at the end. They are roadmap input, not L1 bugs.

## Checklist

Status: **proven** = covered by an automated gate step that passes today · **untested** = exists in code, never exercised · **suspect** = code review found a likely bug · **known bug** = recorded in `CLAUDE.md` → Still Broken.

| # | Area | What "works" means | Controls / code | Proof planned | Status |
| --- | --- | --- | --- | --- | --- |
| 1 | Boot + launcher | Launcher shows, Play (guest) enters the arena | `LauncherWithLogin` | auto (6b, 7, 8) | proven |
| 2 | Spawn | Solo player spawns as blue tank at a spawn point; HUD appears | `GameManager.Start` | auto | proven |
| 3 | Driving | WASD move, mouse look, Shift raises hover height / takes off, Left Ctrl lowers / lands, Space (or Keypad 0) jumps; terrain collision | `PlayerMovementManager` | manual + auto (physics outcome) | untested |
| 4 | Camera + cursor | C swaps the camera position (only while the cursor is hidden); Esc toggles the cursor (hidden = drive, visible = UI) | `CameraManager`, `CursorVisibilityManager` | manual | untested |
| 5 | AutoCannon | Left mouse fires (script default 10/s), costs fuel per shot (default 1), raycast hit damages a unit with hit points, laser line and sound show | `AutoCannon` | auto (hit → health drop) + manual (feel) | untested (M2 test calls the damage RPC directly, not the cannon) |
| 6 | PulseCannon | Right mouse (tanks only) fires a slow shell that does 40 direct damage | `PlayerMovementManager.CmdFirePulseShell`, `PulseShellManager` | auto + manual | untested |
| 7 | Fuel | Drains with firing, regenerates (script default 1/s), fuel bar follows | `FuelManager`, `GameManager.SetFuelBar` | auto + manual | untested |
| 8 | Health | Damage lowers the hull bar; regenerates 0.5/s through the master | `HitPointsManager`, `HealthRegenerator` | auto | proven online (regen, damage); hull bar untested |
| 9 | Targeting | Tab cycles targets; T targets the unit straight ahead (raycast, 300 m); target info panel | `TargetController`, `TargetInfoController` | auto (Tab) | proven (Tab); **known bug**: panel never returns after its target leaves the screen |
| 10 | Death → respawn | At 0 health: overhead camera + cursor; clicking a repair pad respawns at full health and fuel; with no pad, one is spawned | `PlayerMovementManager.Update`, `GameManager.Respawn`, `RepairPad` | auto (kill → respawn) + manual (click) | untested |
| 11 | Self-destruct | V plays the effect, then splash damage around and kills self; respawn works after | `selfDestruct` | auto + manual | untested |
| 12 | Cargo | Q picks up a cargo box within 5 m, Z drops it with its content | `CargoManager`, `GameManager.RequestPickUpCargo` / `RequestDropCargo` | auto | untested; **suspect** online (component passed as RPC argument) |
| 13 | Deploy cargo | `,` places the carried unit (cargo content = prefab name), mouse wheel rotates, click releases | `GroundPlacementController` | auto + manual | untested; logs `mouseScrollDelta` every frame while placing |
| 14 | Turrets | Flak turrets find enemy-team tanks in range, turn, fire flak shells (master only) | `FlakTurretManager` | auto | untested; SAM turret has **no AI** (known) |
| 15 | Unit destruction | A non-player unit at 0 health explodes and is removed | `GameManager.UnitsHealthUpdated` | auto | untested |
| 16 | Leave + return | Leaving returns to the launcher; Play works again | `GameManager.OnLeftRoom` | auto | proven |
| 17 | HUD | Hull + fuel bars, ping, FPS, player list, team counter readable and correct | various | manual | untested |
| 18 | Audio | Cannon, pickup/drop, UI click sounds play | various | manual | untested |
| 19 | Editor parity | Everything above also works in editor Play mode (Wulfram → Offline Play) | `WulframEditorPlayCheck` | auto (boot path) + manual | partly proven |
| 20 | Offline-hidden network bugs | No RPC passes an argument type Photon cannot serialize | static check (CI) | auto | not built yet |

**L1 is done when:** every row is *proven* by an automated gate step and/or a signed-off manual playtest pass, and every bug found is fixed or written down as a deliberate known issue. The verdicts are recorded in `docs/QA-LOG.md`.

## How it gets proven

1. **Automated gameplay smoke** (offline, built player + editor). Like the M2 damage test, each phase calls the same method the control handler calls (Unity 2017's `Input` class can't be fed fake keyboard/mouse input) and checks the outcome: health, fuel, objects spawned or destroyed, camera state. One phase per branch, written **test first**: run it against the current code to see what it catches, then fix.
2. **Manual playtest checklist** (the owner, on a build from the verified commit): rows 3, 4, 13, 17, 18 and the feel of everything else. One session per verified build, recorded in `docs/QA-LOG.md` with the build commit.
3. **Static RPC-argument check** in CI (row 20). It fails on `photonView.RPC(...)` calls whose arguments include a component or GameObject.

## Not in the 2018 code (from the manual; roadmap input, not L1)

No win condition (destroy all enemy vehicles and RepairPads); no team/vehicle selection screen; no minimap or BigMap (`m`); no RefuelPads; landing on a RepairPad doesn't heal; PowerCells are a prefab only (no power/neutral logic); no extra weapons (Piercer, Thumper, Hunter missiles, mines…); no SupplyShips, SkyPumps or orbital strikes; the UpLink (`BlueFiles/Uplink.cs`) is a pick-up/drop stub with no orders; nothing spawns scouts, so the scout RepairBeam can't be used; no bots, so solo play's only opponents are the red base's units; chat is off (separate Photon Chat app needed).
