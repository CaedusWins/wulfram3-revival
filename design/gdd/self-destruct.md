# Self-Destruct

> **Status**: Approved (owner, 2026-10-26)
> **Author**: owner + Claude (retrofit of the 2018 `selfDestruct`; friendly-fire call by the owner, 2026-10-26)
> **Last Updated**: 2026-10-26
> **Last Verified**: 2026-10-26
> **Implements Pillar**: Strategy over kills; Teamwork wins

## Summary

Pressing V arms the tank's self-destruct: after a short effect the tank explodes, killing itself and
damaging **enemy** units within 10 m, more the closer they are. Friendly units take no damage.

> **Quick reference** — Layer: `Feature` · Priority: `MVP` · Key deps: `Health, Teams, Death & Respawn`

## Overview

The 2018 code (`selfDestruct.cs`) damages every unit with hit points in range, whatever its team. The
gameplay test showed the blast destroying the player's own repair pad (L1 row 11), and with the win
condition that lets a player eliminate their own team or wreck a teammate's base. The owner's call
(2026-10-26): the blast hits **enemies only**. It stays a sacrifice play, trading your tank for damage to
an enemy base.

## Player Fantasy

Cornered, out of fuel, next to the enemy's last repair pad: you trade your tank to take it with you. It
should feel like a deliberate last stand, never an accident that costs your own team.

## Detailed Design

### Core Rules

1. **Arming.** V on the player's own living tank arms it once (`selfDestruct.Trigger`); the effect plays on
   every client. Pressing V again while armed does nothing.
2. **Blast.** When the effect ends, every unit with hit points within `sphereRadius` whose team is **not**
   the tank's team takes damage (Formulas). A unit with no team (`""`) counts as not friendly.
3. **Friendly units are untouched:** units where `Unit.SameTeam(unit.team, tank.team)`, including the
   team's pads and turrets and teammates' tanks.
4. **The tank dies:** it takes `maximumDamage`, whatever the blast hit, then the normal death and respawn
   flow runs (`design/gdd/win-condition.md`).
5. **Authority.** Damage goes through `TellServerTakeDamage`, so the master applies it (ADR-0003). Between a
   match's end and the reset, the master ignores it (ADR-0004).

### States and Transitions

| State | Meaning | Transitions |
|---|---|---|
| Idle | Not armed | → Armed on V |
| Armed | Effect playing on all clients | → Blast when the effect ends |
| Blast | Damage applied once; the tank dies | → Idle (reset for the next life) |

### Interactions with Other Systems

- **Health** (#6): damage requests to the master.
- **Teams** (#5): `Unit.SameTeam` decides who is friendly.
- **Death & Respawn** (#12), **Win Condition** (#18): the tank's death counts like any other.

## Formulas

```text
d      = distance from the tank to the unit
damage = ceil(maximumDamage × (sphereRadius − d) / sphereRadius)   for enemy units with d ≤ sphereRadius
damage = 0                                                          for friendly units
```

With the defaults (1000, 10 m), a unit 4 m away takes 600 and one 9 m away takes 100.

## Edge Cases

1. **Next to your own last pad:** the pad survives; your team is not eliminated by your own blast.
2. **A teammate inside the radius:** takes no damage.
3. **Both teams' units in range:** only the enemy's are damaged.
4. **Armed, then killed before the blast:** current behaviour, the blast still fires when the effect ends.
   Kept for now (see Open Questions).
5. **Armed after the match ended:** the master ignores all damage until the reset.

## Dependencies

- Health & Regeneration (#6), Teams (#5), Death & Respawn (#12), Win Condition (#18). ADR-0003.

## Tuning Knobs

| Knob | Default | Range | Notes |
|---|---|---|---|
| `maximumDamage` | 1000 | 100–2000 | at distance 0 |
| `sphereRadius` | 10 m | 3–20 m | blast radius |

## Visual/Audio Requirements

The existing effect (`selfDestructObject` particles), seen by all players during the arming time.

## Game Feel

A short, visible wind-up: enemies who see it can scatter, so it rewards getting close unseen.

## UI Requirements

None beyond the effect.

## Cross-References

- `docs/L1-LOCAL-PLAYABILITY.md` row 11; `design/gdd/win-condition.md` edge case 3 (no longer reachable
  through your own blast).

## Acceptance Criteria

1. An enemy turret 4 m away is damaged or destroyed by the blast (gate 6e `self-destruct`).
2. The player's own repair pad 1.6 m away keeps its health (gate 6e).
3. The tank dies (health 0) and the respawn flow follows.
4. Pressing V twice arms once.

## Open Questions

1. Should a tank killed while armed still explode? (Kept as is.)
