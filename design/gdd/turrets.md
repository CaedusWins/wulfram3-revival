# Turrets

> **Status**: Approved (owner, 2026-10-26)
> **Author**: owner + Claude (retrofit of the 2018 `FlakTurretManager` / `GTManager`; flak balance call by the owner, 2026-10-26)
> **Last Updated**: 2026-10-27
> **Last Verified**: 2026-10-26
> **Implements Pillar**: Strategy over kills; Teamwork wins

## Summary

Deployable turrets defend a team's ground: flak turrets lob explosive shells at enemy tanks from long
range, gun turrets fire rapid short-range bursts. One flak hit does at most 70 damage, so a full-health
tank survives one hit and dies to two.

> **Quick reference** — Layer: `Feature` · Priority: `MVP` · Key deps: `Teams, Health, Explosions & Splash, Deploy`

## Overview

Turrets are the defensive half of base building: carried in as cargo, deployed (Deploy #15, room-owned
since #36), and left to guard repair pads. The 2018 flak shell did 40 direct damage plus an uncapped splash
(`ExplosionPhysicsForce`: `3 × force / distance`), so a direct hit killed a full tank. The owner's call
(2026-10-26): **cap the splash at 30**. The SAM turret prefab has no AI and is out of scope.

## Player Fantasy

A turret line makes the enemy think: attack it head-on and lose tanks, or flank it, or send someone to
destroy it. Turrets reward the team that built them without deciding fights alone.

## Detailed Design

### Core Rules

1. **Targets.** A turret targets tanks (tag `Player`) whose team is its `teamDetect`, compared with
   `Unit.SameTeam` (ADR-0003). It ignores its own team.
   A deployed turret belongs to the deploying tank's team, whatever the box's colour, and targets the
   other team (owner, 2026-10-27, architecture review 3 F4).
2. **Flak turret** (`FlakTurretManager`): scans every `scanInterval` within `scanRadius`, turns at
   `turnSpeed`, and fires a flak shell every `reloadTime` while the target is in sight. Shells are spawned by
   the master (`GameManager.SpawnFlakShell`).
3. **Flak shell:** `directHitpointsDamage` to the unit it hits, then an impact explosion whose splash
   damages nearby units (Formulas). The splash is capped.
4. **Gun turret** (`GTManager`): hit-scan bursts of `bulletDamageinHitpoints` at `bulletsPerSecond` within
   `range`.
5. **Authority.** All damage goes through the master (ADR-0003); turrets are room objects the master can
   destroy (ADR-0003, #36). Splash damage online is a known bug (Known Issues).

### States and Transitions

| State | Meaning | Transitions |
|---|---|---|
| Scanning | No target | → Tracking when an enemy tank is within range |
| Tracking | Turning toward the target | → Firing when on target and reloaded; → Scanning when the target is lost |
| Firing | One shot, then reload | → Tracking |
| Destroyed | Health 0, removed by the master | — |

### Interactions with Other Systems

- **Explosions & Splash** (#10): the splash formula and its cap live in `ExplosionPhysicsForce`, so the cap
  also applies to pulse shells and to the explosion of a destroyed unit.
- **Deploy** (#15), **Cargo** (#14): turrets arrive as cargo.
- **Teams** (#5), **Health** (#6).

## Formulas

```text
splash(d) = min(splashCap, floor(3 × explosionForce / d))     explosionForce = 4, splashCap = 30
flak hit  = directHitpointsDamage (40) + splash(d at impact)   ≤ 70
```

A direct hit on a full-health tank (100) leaves it at 30 or more; a second hit kills it.

## Edge Cases

1. **Splash at distance ~0** (shell bursting on the hull): capped at 30, never unbounded.
2. **Several turrets on one tank:** damage stacks; a tank under two flak turrets can die to one volley.
3. **Turret of no team (`""`):** targets no one useful (its `teamDetect` decides; prefabs all have one).
4. **Pulse shells** share the cap: a pulse direct hit is at most 40 + 30.

## Dependencies

- Teams (#5), Health (#6), Explosions & Splash (#10), Cargo (#14), Deploy (#15). ADR-0003.

## Tuning Knobs

| Knob | Default | Range | Notes |
|---|---|---|---|
| `splashCap` | 30 | 0–100 | new; on `ExplosionPhysicsForce` |
| flak `reloadTime` | 5 s | 2–10 | |
| flak `scanRadius` | 300 | 50–500 | |
| flak shell `directHitpointsDamage` | 40 | 10–100 | `PulseShellManager` on the flak shell |
| gun `bulletDamageinHitpoints` | 5 | 1–20 | |
| gun `range` | 40 | 10–100 | |

## Visual/Audio Requirements

Existing models and effects. Team colour must read at 100 m (game concept, Readable Battlefield).

## Game Feel

Flak should feel heavy and slow to reload: a window to push between shots.

## UI Requirements

None new.

## Cross-References

- `docs/L1-LOCAL-PLAYABILITY.md` row 14; CLAUDE.md "Balance question (2026-10-20)".

## Acceptance Criteria

1. A full-health tank hit once by a flak shell is alive with 30–60 health (gate 6e `turret`).
2. A second hit kills it.
3. No splash ever exceeds `splashCap` (logged by the test).
4. Turrets never target their own team (gate 6e `teams`, `turret`).

## Known Issues

- **The gun turret's code doesn't match this design yet** (architecture review 3, F2; the owner chose this
  GDD as the design, 2026-10-27): `GTManager` damages once per `reloadTime` instead of `bulletsPerSecond`,
  reaches `scanRadius` with a hard-coded 24.14 m cut-off instead of `range` (unused), throws on non-master
  clients (`ShowFeedback` reads a target only the master sets) and after a missed raycast. No gate phase has
  run a gun turret yet. Fix and gate phase: a story.
- **A deployed turret keeps its box's colour today** (`Unit.Awake`; review 3, F4): the rule above is the
  design; the code change is a story.
- **Splash damage online:** `ExplosionPhysicsForce` only damages bodies the master controls, so online the
  splash hits only the master's own tank. Fixing it is required ADR #5 in `docs/architecture/architecture.md`.

## Open Questions

1. Is the SAM turret (no AI) cut, or built later?
