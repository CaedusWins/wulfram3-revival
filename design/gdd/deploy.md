# Deploy

> **Status**: Approved (owner, 2026-10-27)
> **Author**: owner + Claude (retrofit of the 2018 `GroundPlacementController`; room-owned deploys #38; rotation rule by the owner, 2026-10-27)
> **Last Updated**: 2026-10-27
> **Last Verified**: 2026-10-27
> **Implements Pillar**: Teamwork wins; Strategy over kills

## Summary

With cargo on board, `,` shows a ghost of the carried unit in front of the tank, the mouse wheel turns it,
and a left click asks the master to build it there for the tank's team. `,` again cancels.

> **Quick reference** — Layer: `Core` · Priority: `MVP` · Key deps: `Cargo, Teams, Networking`

## Overview

Deploy turns a box in the hold into a turret or a repair pad on the ground. The preview is this client's
own picture (meshes in a ghost material, no scripts, no network identity, #38 / ADR-0003); the real unit is
a room object the master creates after checking the request. This GDD records that behaviour (L1 row 13)
and sets the rotation rule the 2018 code got wrong.

## Player Fantasy

You chose the spot under fire: the turret goes down exactly where and how you placed it.

## Detailed Design

### Core Rules

1. **Show (`,`):** with a non-empty hold, `,` shows a preview of the carried unit (`Resources/<content>`) at
   the tank's deploy point: about 5.8 m straight ahead, at the tank's height. It moves with the tank; its
   facing is the tank's facing when it was shown, plus the wheel turns. A content with no prefab shows
   nothing.
2. **Rotate (mouse wheel):** **each wheel notch turns the preview 10° about the vertical**; it stays put
   between notches. A new preview starts unrotated. *(Owner's rule, 2026-10-27; the 2018 code spun it,
   fixed in `GroundPlacementController.RotateNotches`, gate 6e `deploy-rotate`.)*
3. **Deploy (left click):** sends the preview's position and rotation to the master
   (`GameManager.DeployUnit`) and removes the preview. The master refuses a tank that isn't the sender's, an
   empty hold, a missing prefab or an ended match; otherwise it creates the carried unit as a room object
   **for the deploying tank's team, whatever the box's colour** (`cargo.md` rule 6, ADR-0003) and empties
   the hold for everyone.
4. **Cancel (`,` again):** removes the preview; the hold keeps its cargo.
5. **Only the owner** acts on its own tank's keys; other clients never see the preview.
6. **Only a living tank with a hidden cursor deploys:** clicks with the cursor visible are UI clicks, and a
   dead tank's preview is removed. *(The 2018 code checked neither; fixed on the deploying client, gate 6e
   `deploy-cursor`, `deploy-dead`. The master doesn't check it yet: Required ADR #2, RPC trust.)*

### States and Transitions

| State | Meaning | Transitions |
|---|---|---|
| Idle | No preview | → Previewing on `,` with cargo on board |
| Previewing | Ghost at the deploy point | → Idle on `,` (cancel), on a click (request sent), or at death |
| Requested | The master decides | the unit appears for everyone and the hold empties, or nothing changes (refused) |

### Interactions with Other Systems

- **Cargo** (#14): the hold's content names the prefab.
- **Teams** (#5): the unit takes the deployer's team. **Turrets** (#16), **Repair Pads** (#17): what gets built.
- **Win Condition** (#18): deployed units are cleared by the arena reset; no deploys after a match ends.

## Formulas

```text
deploy point = tank position + tank rotation × (0, 0, 4.38 × tank scale 1.32)   ≈ 5.8 m ahead
preview yaw  = 10° × wheel notches since the preview was shown
```

## Edge Cases

1. **Click while the cannon is ready:** the same left click also fires the AutoCannon (one burst).
2. **Deploy point inside a hill or another unit:** no placement check today; the unit is created there.
3. **Deploy point in the air** (tank hovering high or pitched up): the unit is created there.
4. **Two players deploy at once:** each request is checked separately; both units are built.
5. **Match ends with a preview showing:** the click is refused by the master; the hold keeps its cargo.
6. **Aimed at the sky:** the preview stops following the tank (it moves only while the camera's ray hits
   something), and a click deploys it where it was left (review 4, G5; not fixed).
7. **Hold emptied while previewing** (cargo dropped): `,` can't cancel the preview any more; a click sends
   a request the master refuses (empty hold) and removes it (review 4, G5; not fixed).

## Dependencies

- Cargo (#14), Teams (#5), Networking (#3). ADR-0003 (master-checked, room-owned deploys), ADR-0004 (no
  deploys after the match ends).

## Tuning Knobs

| Knob | Value | Safe range | Notes |
|---|---|---|---|
| `newObjectHotkey` | `,` | — | |
| rotation step | 10° per notch | 5–45° | |
| deploy point | 4.38 tank-local units ahead | — | a child object on the tank prefab |
| `PreviewMaterial` | `GhostMaterialBlue` | — | |

## Visual/Audio Requirements

The ghost must read clearly as "not real" at a glance. The drop sound plays when the hold empties
(`audio.md`).

## Game Feel

Rotating should be precise: one notch, one step.

## UI Requirements

None beyond the preview (the hold's content on the HUD is `cargo.md` / `hud.md`).

## Cross-References

- `docs/L1-LOCAL-PLAYABILITY.md` row 13; `cargo.md`; `turrets.md`; `win-condition.md`; ADR-0003.

## Acceptance Criteria

1. `,` with cargo shows an inert preview; `,` again removes it (gate 6e `deploy`).
2. A click deploys the carried unit for the deployer's team as a room object and empties the hold (gate 6e
   `deploy`, `deploy-team`; online gate 6c `match`).
3. The master refuses an empty hold, a missing prefab and an ended match (gate 6e `deploy-refused`).
4. Each wheel notch turns the preview 10°, and it holds still between notches; a new preview starts
   unrotated (gate 6e `deploy-rotate`; the real wheel: playtest A9).
5. A click with the cursor visible, or from a dead tank, deploys nothing, and a dead tank's preview is
   removed (gate 6e `deploy-cursor`, `deploy-dead`).

## Known Issues

- **Fixed 2026-10-27 (`feature/playtest-blockers`):** the preview spun after one notch (the running total
  was applied every frame), and a visible cursor or a dead tank could deploy.
- **The master doesn't refuse a dead tank's deploy request** (only the deploying client checks): Required
  ADR #2 (RPC trust).
- **A turret deployed from the other team's box keeps the box's colour** (rule 3; review 3 F4).

## Open Questions

1. Should the deploy point snap to the ground (and refuse steep or blocked spots)? Today it doesn't.
