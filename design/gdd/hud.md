# HUD

> **Status**: Approved (owner, 2026-10-27)
> **Author**: owner + Claude (retrofit of the 2018 `HUDCanvas` in Playground, read from a scene dump, and `MatchManager`'s match line, 2026-10-27)
> **Last Updated**: 2026-10-27
> **Last Verified**: 2026-10-27
> **Implements Pillar**: Readable Battlefield

## Summary

The in-game overlay: hull and fuel bars, the team player counts, ping and FPS, the target panel, the match
line and result banner, a crosshair, and a Leave Game button when the cursor is shown.

> **Quick reference** — Layer: `Presentation` · Priority: `MVP` · Key deps: `Health, Fuel, Teams, Targeting, Win Condition, Camera & Cursor`

## Overview

The HUD is the `HUDCanvas` in the Playground scene (Unity UI) plus the `OnGUI` lines `MatchManager` added
in #33. This GDD records what is on it (L1 row 17) and the rules each element follows, so the playtest can
check it and Required ADR #4 (HUD architecture: UGUI vs `OnGUI`) has a design to serve. It does not
redesign the HUD.

## Player Fantasy

A glance tells you your state (hull, fuel), the battle's state (team sizes, pads and tanks left) and what
you are aiming at.

## Detailed Design

### Core Rules

| # | Element | Shows | Updates |
|---|---|---|---|
| 1 | Hull bar | own health ÷ max health, as the bar's width | when health changes (`GameManager.SetHullBar`) |
| 2 | Fuel bar | own fuel ÷ max fuel | when fuel changes (`SetFuelBar`) |
| 3 | Team counts | blue and red player counts, with tank icons | on join, leave and team change (`TeamCounter`) |
| 4 | Status line 1 | `Ping:<ms> ~ FPS:<n>` | about every 0.6 s (`StatusBarController`) |
| 5 | Status line 2 | `Server:1 \| Sector:*Map Name*` | never: a 2018 placeholder (Known Issues) |
| 6 | Target panel | target name, type, HP and distance, team | on target change (`TargetInfoController`, Targeting #11) |
| 7 | Crosshair | screen centre | static |
| 8 | Match line (top) | `Red: <n> pads, <n> tanks \| Blue: …` | every frame (`MatchManager`, `win-condition.md`) |
| 9 | Match hint | no pads / enemy pads gone / your pads gone | while Playing (`win-condition.md`) |
| 10 | Result banner | VICTORY / DEFEAT / DRAW and the reset countdown | while Ended |
| 11 | Leave Game button | leaves the room for the launcher | **shown only while the cursor is visible** (`GUIManager`) |
| 12 | Frame art | the HUD frame images | static |

Rules:

1. **Own tank only:** bars show the local player's tank.
2. **Counts are numbers with correct words:** "1 pad", "2 pads" (Known Issues).
3. **Nothing overlaps:** the match line sits clear of the status lines and frame art (Known Issues).
4. **Readable at a glance:** team colours red and blue (game concept, Readable Battlefield).
5. **Chat** (#23) is off (it needs its own Photon Chat app); its panel is out of scope here.

### States and Transitions

| Cursor | HUD |
|---|---|
| Hidden (driving) | all elements but the Leave button |
| Visible (UI) | also the Leave button (clickable panel) |
| Dead (overhead map) | cursor visible: the Leave button shows |

### Interactions with Other Systems

- Health (#6), Fuel (#7), Teams (#5), Targeting (#11), Win Condition (#18), Camera & Cursor (#2), Networking
  (#3: ping, player counts).

## Formulas

```text
bar width = background width × value / max          (LevelController.SetLevel)
FPS       = round(1 / frame time), sampled about every 0.6 s
```

## Edge Cases

1. **Offline:** no server, so ping is meaningless; the team counts should read 1 for your team and 0 for the
   other (not yet checked: the scene saves "1" in both, and `TeamCounter` overwrites them on join).
2. **Dead:** the hull bar reads empty until respawn.
3. **Target leaves the screen:** the target panel never comes back (known bug, L1 row 9).

## Dependencies

- The systems in Interactions. Required ADR #4 (HUD architecture) is not written yet; this GDD is
  technology-neutral.

## Tuning Knobs

| Knob | Value | Notes |
|---|---|---|
| status refresh | ~0.6 s | `StatusBarController` |
| match line font | 14 px; banner 64 px; hint 22 px | `MatchManager.OnGUI` |

## Visual/Audio Requirements

UI click sound in the launcher (`audio.md`); none in-game.

## Game Feel

Information, not decoration: the HUD must never hide the battlefield's centre.

## UI Requirements

This document is the UI requirement.

## Cross-References

- `docs/L1-LOCAL-PLAYABILITY.md` rows 9, 16, 17; `win-condition.md`; `cargo.md` (hold display, TR-cargo-007);
  `camera-cursor.md`; `docs/architecture/architecture.md` Required ADR #4.

## Acceptance Criteria

1. Hull and fuel bars track own health and fuel (manual, playtest A14).
2. Team counts match the players in the room (manual, playtest A14; online B1).
3. Ping and FPS update (manual, playtest A14).
4. The match line and result banner show the right counts and result (gate 6e `win`, 6c `match`; playtest
   A15, B4).
5. With the cursor visible the Leave Game button shows, and clicking it returns to the launcher (manual,
   playtest A17; a gate check comes with the fix).
6. Words agree with numbers ("1 pad") and nothing overlaps (manual, comes with the polish fix).

## Known Issues

- **The Leave Game button never shows:** its panel (`ClickableUI`) is saved inactive in the scene, and the
  `GUIManager` that would show it sits on that same inactive object, so it never runs. Players can only
  leave by closing the game. L1 row 16 is proven only through the test calling `LeaveRoom` directly.
- **Status line 2 is a placeholder** ("Server:1 | Sector:*Map Name*").
- **The match line overlaps the status bar** and says "1 pads".
- **The hold's content isn't shown** (TR-cargo-007).
- **There is no player list** (L1 row 17 named one; `playerInfoPanelPrefab` is never used).
- `PingCounter` (an `OnGUI` ping label) exists in the code but is in no scene.

## Open Questions

1. Status line 2: show the room and map names, or remove it?
2. Is a player list (names per team) wanted for M3? (Not built.)
