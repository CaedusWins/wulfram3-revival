# Audio

> **Status**: Approved (owner, 2026-10-27)
> **Author**: owner + Claude (retrofit of the 2018 sound calls and prefab `AudioSource`s, read from prefab dumps, 2026-10-27)
> **Last Updated**: 2026-10-27
> **Last Verified**: 2026-10-27
> **Implements Pillar**: Readable Battlefield; Classic core, modern take

## Summary

Sound tells you what is happening without looking: your own engine, jumps and landings, cannon fire and
hits, shells and explosions, cargo in and out, and the launcher's clicks. All clips are the classic
Wulfram sounds in `Assets/Wulfsound/`.

> **Quick reference** — Layer: `Presentation` · Priority: `MVP` · Key deps: `Hover Movement, AutoCannon, Pulse Cannon, Explosions, Cargo`

## Overview

There is no audio system as such: each script or prefab plays its own clip through Unity's `AudioSource`.
This GDD lists every cue that exists today (L1 row 18), who hears it, and the gaps, so the playtest can
check them and later audio work starts from a full list.

## Player Fantasy

The battlefield sounds like Wulfram: the clips are the originals.

## Detailed Design

### Core Rules

The cues, as built:

| # | Cue | Clip | Plays when | Who hears it |
|---|---|---|---|---|
| 1 | Engine | `tank.ogg` (volume 0.2) | the tank appears; again at take-off; stops at landing | everyone near the tank (3D) |
| 2 | Jump | `tank_jump3.ogg` | a jump | the owner only |
| 3 | Take-off | `takeoff.ogg` | lifting off from landed | the owner only |
| 4 | Landing | `tank_touchdowna.ogg` | landing | the owner only |
| 5 | Cannon shot | `gunshot4.ogg` | each AutoCannon tracer while firing | every client (firing is synced) |
| 6 | Cannon hit | `hit1.ogg` | a cannon round hits an enemy unit | the shooter only, at the shooter |
| 7 | Pulse / flak shell | `pulse_shot2.ogg` | a shell is spawned | every client (3D, at the shell) |
| 8 | Explosion | `explo1.ogg` | a shell bursts or a unit is destroyed | every client (3D) |
| 9 | Cargo pick-up | `cargup.ogg` | the hold fills | the owner only |
| 10 | Cargo drop / deploy | `cargdn.ogg` | the hold empties (drop or deploy) | the owner only |
| 11 | UI click | launcher `clicksound` | Play in the launcher | the local player |

Rules:

1. **Your own actions always sound** (cues 2–4, 9, 10, 11 are local feedback).
2. **Combat sounds are heard by everyone in range** (cues 1, 5, 7, 8).
3. **Classic clips:** new sounds come from `Assets/Wulfsound/` (88 clips) before anything new is made.

### States and Transitions

None: each cue is a one-shot, except the engine (on while flying, off while landed).

### Interactions with Other Systems

- Hover Movement (#1), AutoCannon (#8), Pulse Cannon (#9), Explosions (#10), Cargo (#14), Deploy (#15),
  Launcher (#21).

## Formulas

None. Volumes are per source (engine 0.2; launcher click 3×).

## Edge Cases

1. **Many shells at once:** each plays its own sound; no limit on voices.
2. **Engine clip ends:** the engine source does not loop, so the engine falls silent after one play of the
   clip (Known Issues).

## Dependencies

- The systems above. ADR-0001 (Unity 2017 audio).

## Tuning Knobs

| Knob | Value | Notes |
|---|---|---|
| engine volume | 0.2 | `PlayerTank` `AudioSource` |
| cue volumes | 1.0 (cargo), 3.0 (launcher click) | in code |

## Visual/Audio Requirements

This document is the audio requirement.

## Game Feel

The engine should hum while you fly and go quiet when you land; a hit should be heard by the one who
landed it and, ideally, by the one who took it.

## UI Requirements

None.

## Cross-References

- `docs/L1-LOCAL-PLAYABILITY.md` row 18; `hover-movement.md`; `win-condition.md` (TR-win-condition-016: no
  match-end sound yet).

## Acceptance Criteria

1. Cannon fire, pick-up, drop and the launcher click play (manual, playtest A16).
2. Jump, take-off and landing play for the owner (manual, playtest A2/A16).
3. Shells and explosions are heard by both players online (manual, playtest B2).

## Known Issues

- **The engine sound doesn't loop** (`Loop = False` on the tank's `AudioSource`): it plays once and stops.
- **No sound** for: self-destruct, turrets firing (the gun turret's line is commented out), the match result,
  a tank taking a hit (only the shooter hears `hit1`).

## Open Questions

1. Should the engine loop while flying? (Recommended: yes.) Playtest question.
2. Should the player who is hit also hear it? Playtest question.
