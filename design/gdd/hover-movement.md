# Hover Movement

> **Status**: Approved (owner, 2026-10-27)
> **Author**: owner + Claude (retrofit of the 2018 `PlayerMovementManager`; values read from the `PlayerTank` prefab, 2026-10-27)
> **Last Updated**: 2026-10-27
> **Last Verified**: 2026-10-27
> **Implements Pillar**: Classic core, modern take

## Summary

The hovertank floats a set height above the terrain, drives with WASD, aims the whole hull with the mouse,
raises or lowers its hover height (Shift / Left Ctrl) to take off or land, and jumps (Space) for fuel.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `Physics, Input, Fuel`; reads the input mode (driving vs UI, Required ADR #4)

## Overview

Movement is the 2018 `PlayerMovementManager` on the `PlayerTank` prefab. The tank is a rigidbody: a hover
force pushes it up from the terrain below it, WASD pushes it along its own axes, and the mouse turns the
hull. Only the owner's client simulates its tank; other clients see it kinematic, moved by the network.
This GDD records the existing behaviour (L1 row 3) so the owner's playtest can judge its feel against a
written rule.

## Player Fantasy

A heavy hovercraft: momentum, a little float over bumps, and the choice to settle on the ground (a stable
gun platform) or lift off and move.

## Detailed Design

### Core Rules

1. **Drive (WASD):** a force along the tank's own forward/right axes, `driveForce × axis` (Formulas), applied
   every physics step while the cursor is hidden. With the cursor visible the tank gets no drive input.
2. **Hover:** a ray straight down from the tank to the terrain (`TerrainCollider` only, reach `2 × height`);
   while it hits, an upward force grows as the gap closes (Formulas). Units, pads and boxes are not terrain:
   the tank hovers over the ground under them.
3. **Mouse look:** mouse X turns the hull (yaw), mouse Y pitches it within ±60°, at `sensitivity` 1.5. The
   whole tank turns, and the guns with it. No mouse look while landed.
4. **Hover height (Shift / Left Ctrl):** Shift raises `height` toward 1.0, Ctrl lowers it toward 0. At 0 the
   tank asks to land.
5. **Land:** allowed when `height ≤ 0.3` and the terrain is within 0.3 below. The tank snaps flat onto the
   terrain's slope, stops being simulated (kinematic), plays the landing sound and stops the engine sound.
6. **Take off:** Shift while landed: the tank is simulated again, plays the take-off sound and restarts the
   engine sound.
7. **Jump (Space or Keypad 0):** costs `fuelPerJump` (20) and pushes the tank up along its own up axis;
   at most one jump per `timeBetweenJumps` (3 s). Refused without the fuel.
8. **Death:** at 0 health the tank takes no input; after `destroyDelayWhenDead` (5 s) the respawn flow starts
   (`win-condition.md`): the overhead map, then a pad click or a new pad.
9. **Authority:** the owner's client moves its own tank (ADR-0003, amended 2026-10-27); others see
   it through PUN's view sync.

### States and Transitions

| State | Meaning | Transitions |
|---|---|---|
| Hovering | Simulated, floating at `height` | → Landing when `height` reaches 0; → Dead at 0 health |
| Landing requested | `height` 0, waiting for ground within 0.3 | → Landed when it can land; → Hovering on Shift |
| Landed | Kinematic, flat on the slope, no mouse look | → Hovering on Shift (take off); → Dead at 0 health |
| Dead | No input | → Hovering at respawn (`Reset`, `win-condition.md`) |

### Interactions with Other Systems

- **Fuel** (#7): jumps spend fuel. **Pulse Cannon** (#9): fired from this script (right mouse), recoil pushes
  the tank back.
- **Camera & Cursor** (#2): a visible cursor stops driving and mouse look.
- **Death & Respawn** (#12): the death timer and `Reset` live here.

## Formulas

```text
drive force (per physics step)  = 50 × 0.1 × (axisX, 0, axisZ)        axis ∈ [-1, 1]  → at most 5 per axis
hover force (per physics step)  = min(4 / max(d − height, 0.01), 20)   d = distance to the terrain below
height change (per frame)       = ±0.01 while Shift / Ctrl is held      height ∈ [0, 1], default 0.5
jump                            = 700 along the tank's up axis, 20 fuel, once per 3 s
```

The height change is per rendered frame, so it is faster at a higher frame rate (Known Issues).

## Edge Cases

1. **Over a unit or a pad:** the hover ray ignores everything but terrain; the tank collides with the object
   but hovers relative to the ground below it.
2. **Off the terrain's edge:** no terrain below, no hover force: the tank falls.
3. **Landing on a steep slope:** the tank snaps to the slope's angle.
4. **Cursor shown mid-drive:** the tank stops taking input and its rotation is frozen until the cursor hides.
5. **Jump while landed:** the jump force acts on a kinematic body: nothing happens, the fuel is spent.

## Dependencies

- Physics (Unity rigidbody), Input (Unity 2017 `Input`), Fuel (#7). ADR-0001 (engine), ADR-0003 (owner moves
  its own tank).

## Tuning Knobs

| Knob | Value (prefab) | Safe range | Notes |
|---|---|---|---|
| `sensitivityX` / `sensitivityY` | 1.5 | 0.5–5 | mouse look; the playtest judges it |
| `minimumY` / `maximumY` | −60 / 60 | ±30–±89 | pitch limits |
| `fuelPerJump` | 20 | 0–100 | |
| `timeBetweenJumps` | 3 s | 0.5–10 | |
| `destroyDelayWhenDead` | 5 s | 1–10 | |
| jump force, drive force, hover ceiling | 700, 50 × 0.1, 20 | — | constants in code today |

## Visual/Audio Requirements

Jump, take-off and landing sounds, and an engine sound (`audio.md`).

## Game Feel

Weighty but responsive: the playtest (L1 row 3) is the measure. Driving, aiming and the hover height should
not depend on the frame rate.

## UI Requirements

None (fuel shows on the HUD, `hud.md`).

## Cross-References

- `docs/L1-LOCAL-PLAYABILITY.md` row 3; `design/gdd/win-condition.md` (death and respawn); `audio.md`.

## Acceptance Criteria

1. WASD moves the tank; a visible cursor stops it (manual, playtest A1).
2. The tank hovers above the terrain without falling through it (manual, playtest A1; gate 6e drives the
   tank onto a box but doesn't assert the hover).
3. Shift raises the hover and takes off a landed tank; Ctrl lowers it and lands on ground within 0.3
   (manual, playtest A2).
4. Space jumps for 20 fuel, at most once per 3 s, and is refused without fuel (manual, playtest A2).
5. A dead tank takes no input; after 5 s the overhead map shows, and it respawns when its player clicks an
   own-team pad, or by itself when a pad appears (gate 6e `respawn`).

## Known Issues

- **Hover height changes per frame**, not per second: Shift/Ctrl act faster at a higher frame rate.
- **"CanLand false" is logged every frame** while a landing is requested and not possible.

## Open Questions

1. Should a landed tank still aim with the mouse? (Today it can't.) Playtest question.
2. Is the mouse sensitivity right? Playtest question.
