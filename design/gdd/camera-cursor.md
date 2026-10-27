# Camera & Cursor

> **Status**: Approved (owner, 2026-10-27)
> **Author**: owner + Claude (retrofit of the 2018 `CameraManager` / `CursorVisibilityManager`, 2026-10-27)
> **Last Updated**: 2026-10-27
> **Last Verified**: 2026-10-27
> **Implements Pillar**: Classic core, modern take

## Summary

The camera rides on the player's own tank, in a cockpit view or a chase view (C swaps them). Esc toggles
the cursor: hidden means driving and fighting, visible means using the UI. Dead players watch from an
overhead map.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `Hover Movement, HUD, Death & Respawn`

## Overview

`CameraManager` (on the `PlayerTank` prefab) takes the scene's main camera and parents it to the owner's
tank. `CursorVisibilityManager` (on the HUD canvas) owns the cursor's visibility and lock. Other systems
read `Cursor.visible` to decide whether the player is driving (movement, weapons) or using the UI (the HUD's
clickable panel). This GDD records that behaviour (L1 row 4).

## Player Fantasy

You are in the tank: the cockpit view for precise shots, the chase view to see your surroundings.

## Detailed Design

### Core Rules

1. **Attach:** when the owner's tank starts, the main camera becomes its child at the cockpit position.
2. **Swap (C):** only while the cursor is hidden, C moves the camera between the cockpit position and the
   chase position over `transitionTime` (1 s).
3. **Cursor (Esc):** Esc toggles the cursor. Hidden = locked to the window centre; visible = free. A left
   click re-applies the lock for the current state (so a click back into the window re-captures it).
4. **Driving vs UI:** while the cursor is visible the tank gets no drive, mouse-look or fire input
   (`hover-movement.md`, weapons), and the HUD's clickable panel shows (`hud.md`).
5. **Death:** when the death timer (`destroyDelayWhenDead`, 5 s) runs out, the tank camera is switched off
   and the overhead map camera on, with the cursor shown; respawn restores the tank camera and hides the
   cursor (`win-condition.md`). The camera stays parented to the tank (`CameraManager.Detach` is never
   called).

### States and Transitions

| State | Meaning | Transitions |
|---|---|---|
| Cockpit | Camera at `firstPersonPos` | → Chase on C (cursor hidden), over 1 s |
| Chase | Camera at `thirdPersonPos` | → Cockpit on C (cursor hidden), over 1 s |
| Overhead | Dead: map camera, cursor shown | → Cockpit/Chase at respawn |

| Cursor | Meaning | Transitions |
|---|---|---|
| Hidden (locked) | Driving and fighting | → Visible on Esc, or when the respawn flow starts (5 s after death) |
| Visible (free) | UI | → Hidden on Esc, or at respawn |

### Interactions with Other Systems

- **Hover Movement** (#1) and the weapons (#8, #9): no input while the cursor is visible.
- **HUD** (#20): the clickable panel follows the cursor.
- **Death & Respawn** (#12): overhead camera and cursor.

## Formulas

```text
camera position during a swap = Slerp(current, target, (t − t0) / transitionTime)   until t − t0 ≥ 1 s
cockpit (firstPersonPos) = (0, 0.33, −0.3)     chase (thirdPersonPos) = (0, 1, −2.5)   tank-local
```

## Edge Cases

1. **C with the cursor visible:** ignored.
2. **C pressed mid-swap:** the swap reverses toward the other position from where the camera is.
3. **Alt-tab and back:** the cursor state is kept; a click re-applies the lock.

## Dependencies

- Hover Movement (#1), HUD (#20), Death & Respawn (#12). ADR-0001 (Unity 2017 cursor API).

## Tuning Knobs

| Knob | Value (prefab) | Safe range | Notes |
|---|---|---|---|
| `firstPersonPos` | (0, 0.33, −0.3) | — | cockpit |
| `thirdPersonPos` | (0, 1, −2.5) | — | chase |
| `transitionTime` | 1 s | 0.1–2 | |

## Visual/Audio Requirements

None.

## Game Feel

Swapping should be quick and smooth; the playtest judges whether 1 s is too slow.

## UI Requirements

The clickable HUD panel appears only with the cursor visible (`hud.md`).

## Cross-References

- `docs/L1-LOCAL-PLAYABILITY.md` row 4; `hover-movement.md`; `hud.md`; `win-condition.md`.

## Acceptance Criteria

1. Esc toggles the cursor between hidden (locked) and visible (manual, playtest A3).
2. C swaps cockpit and chase views, only while the cursor is hidden (manual, playtest A4).
3. While the cursor is visible the tank does not drive or fire (manual, playtest A3).
4. 5 s after death the overhead camera and cursor show; at respawn the tank camera returns and the cursor
   hides (gate 6e `respawn`).

## Known Issues

- "ESC was pressed" and "Cursor locked!" are logged on every Esc and left click.

## Open Questions

1. Is a 1 s swap right? Playtest question.
