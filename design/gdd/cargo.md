# Cargo

> **Status**: Approved (owner, 2026-10-26)
> **Author**: owner + Claude (retrofit of the 2018 `CargoManager` / `Cargo`; BlueCargo call by the owner, 2026-10-26)
> **Last Updated**: 2026-10-27
> **Last Verified**: 2026-10-26
> **Implements Pillar**: Strategy over kills; Teamwork wins

## Summary

Cargo boxes lie on the map; a tank picks one up (Q), carries it, drops it (Z) or deploys what it holds
(`,` and a click). A box's content is the name of the unit it deploys.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `Networking, Deploy, Teams`

## Overview

Cargo is how bases get built: every turret and repair pad on the map arrives as cargo. The arena starts
with 17 boxes. Two data bugs make 3 of them useless: two boxes say `'Repair Pad'` (with a space; the prefab
is `RepairPad`), and one says `'BlueCargo'`, which names no prefab. The owner's call (2026-10-26):
**`BlueCargo` becomes a blue gun turret** (`BlueGunTurret`), and the two pad boxes are corrected.

## Player Fantasy

You are the team's builder: you find the turret that will hold the ridge, carry it through fire, and put
it down where it matters.

## Detailed Design

### Core Rules

1. **Pick up (Q):** a tank with an empty hold takes the nearest box within 5 m; the master removes the box
   and fills the hold (`GameManager.RequestPickUpCargo`).
2. **Drop (Z):** the master empties the hold and creates a box with the same content at the tank, as a room
   object that the arena reset clears (`RequestDropCargo`, #38).
3. **Deploy (`,` + click):** a local preview, then a request the master checks and fulfils (Deploy #15,
   ADR-0003).
4. **One box at a time:** a full hold can't pick up.
5. **Content = prefab name** in `Assets/Resources/`; a content with no prefab can't be deployed (the master
   refuses it).
6. **Any tank can carry any box**, and what it deploys fights for the deploying tank's team, whatever the
   box's colour (owner, 2026-10-27, architecture review 3 F4; today the code keeps a `Red*`/`Blue*` prefab's own team, a story).

### States and Transitions

| State | Transitions |
|---|---|
| Box on the ground | → In a hold on pick-up |
| In a hold | → Box on the ground on drop; → Deployed unit on deploy |

### Interactions with Other Systems

- **Deploy** (#15), **Turrets** (#16), **Repair Pads** (#17): what cargo becomes.
- **Networking** (#3): every change goes through the master.

## Formulas

None (pick-up radius 5 m).

## Edge Cases

1. **Two tanks press Q at the same box:** the master gives it to the first request; the second finds no box.
2. **Dropped box in the reset:** removed (room object, `Unit.deployed`).
3. **Carrying cargo when the tank dies:** the hold is kept today (Open Questions).

## Dependencies

- Networking & Match Join (#3), Deploy (#15), Teams (#5). ADR-0002, ADR-0003.

## Tuning Knobs

| Knob | Default | Range | Notes |
|---|---|---|---|
| pick-up radius | 5 m | 2–10 m | `GameManager.FindCargoInRange` |

## Arena Contents (Playground)

| Content | Boxes | After this design |
|---|---|---|
| `RedFlakTurret` | 6 | unchanged |
| `BlueFlakTurret` | 3 | unchanged |
| `RepairPad` | 3 | unchanged |
| `Repair Pad` | 2 | → `RepairPad` (data fix) |
| `BlueGunTurret` | 1 | unchanged |
| `BlueCargo` | 1 | → `BlueGunTurret` (owner, 2026-10-26) |
| `RedPowerCell` | 1 | unchanged (power cells are the M4 economy) |

## Visual/Audio Requirements

Existing box model; pick-up and drop sounds (`CargoManager.pickup` / `dropdown`).

## Game Feel

Carrying should feel like a commitment: you are the target while you hold it.

## UI Requirements

The HUD should show the hold's content (not built: only the pick-up and drop sounds today; `CargoManager` has a TODO). Required ADR #4 (HUD) covers it.

## Cross-References

- `docs/L1-LOCAL-PLAYABILITY.md` rows 12 and 13; CLAUDE.md "Data bug (found 2026-10-20)".

## Acceptance Criteria

1. Every one of the arena's 17 boxes names a prefab in `Assets/Resources/` (gate 6e cargo NOTE line lists none).
2. Pick-up, drop and deploy behave as in rules 1–3 (gate 6e `cargo-pickup`, `cargo-drop`, `deploy`, `deploy-refused`).
3. A dropped box is a room object (gate 6e `cargo-drop` room-owned).

## Open Questions

1. Should carried cargo be destroyed when the tank dies? (The 2018 manual says yes; the code keeps it.)
