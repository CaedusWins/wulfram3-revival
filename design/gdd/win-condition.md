# Win Condition

> **Status**: Approved (owner, 2026-10-26)
> **Author**: owner + Claude (decisions by the owner, 2026-10-26)
> **Last Updated**: 2026-10-26
> **Last Verified**: 2026-10-26
> **Implements Pillar**: Strategy over kills; Teamwork wins

## Summary

A match ends when one team has **no repair pads and no living tanks**; the other team wins. Repair
pads are the only way back into the fight, so they are the real objective.

> **Quick reference** — Layer: `Feature` · Priority: `MVP` · Key deps: `Teams, Repair Pads, Death & Respawn, Deploy, Networking`

## Overview

Wulfram 3's 2018 code never ends a match: there is no win or lose check anywhere in its history (the
only "EndGame" in the repo is Photon's demo code). The manual (`README.md`) defines the rule:
*"whoever destroys all the enemy Tanks, Scouts, and the RepairPads that let them build more, wins
the game"*, and *"you will be given another vehicle as long as there is a RepairPad to build it for
you."* The 2018
team's intended spawn flow (upstream issues #7 "Initial spawn is not Repair Pad based" and #13
"Hitting play needs to bring you to map screen") makes that rule work: you only ever enter the game
from one of your team's repair pads. This system adds the rule, removes today's no-pad fallback
(respawn at the arena centre), makes repair pads belong to a team, and adds a match-end announcement
followed by a new match.

## Player Fantasy

Every tank on your team is protecting the pads that bring your teammates back. Destroying the enemy's
last pad turns their dead into spectators, and the push to finish their remaining tanks is the climax
of the match. A team that loses its pads can still come back, by carrying in cargo and deploying a
new pad, if it has a living tank to do it.

## Detailed Design

### Core Rules

1. **Teams own pads.** Every repair pad has a team (`Unit.team`, canonical `red`/`blue`). A pad
   deployed from cargo takes the **deploying tank's** team (today `GroundPlacementController` instantiates the prefab as-is, so the pad keeps whatever
   team the prefab was saved with).
2. **You respawn only at your own team's pads.** The overhead respawn map offers only pads where
   `Unit.SameTeam(pad.team, player.team)`; clicking an enemy or neutral pad does nothing. (Today any
   pad works, including the enemy's.)
3. **No pad, no respawn.** If a dead player's team has no pad, the player stays on the overhead map as
   a spectator. They respawn automatically as soon as their team has a pad again (a teammate deployed
   one). **The arena-centre fallback (`GameManager.Respawn` → `RepairPad.Spawn(…, Vector3.zero)`) is
   removed.**
4. **Elimination.** A team is **eliminated** when it has **zero repair pads and zero living tanks**.
   A tank counts as living while its health is above 0 (what `PlayerMovementManager.isDead` is computed from). Scouts count as tanks once
   they exist (they don't spawn yet).
5. **Victory.** When exactly one team is eliminated, the other team wins. If both are eliminated in
   the same check (e.g. by a self-destruct), the match is a **draw**.
6. **Authority.** Only the master client evaluates the rule and announces the result (an RPC to all),
   the same authority model as damage (`TellServerTakeDamage`).
7. **Start condition.** The rule only arms once both teams have at least one player, so a lone
   player in an empty room isn't instantly "eliminated" or victorious. Offline play arms it too, so
   the gameplay test can exercise it.
8. **Next match.** After the result is announced, the arena resets about 15 s later in the same room:
   destroyed base structures are restored to their starting state, deployed units and dropped cargo
   are cleared, every player respawns at their team's starting pad with full health and fuel, and the
   rule re-arms.
9. **No time limit** (MVP). Matches end only by elimination; revisit after playtests if they drag.

### States and Transitions

| State | Meaning | Transitions |
|---|---|---|
| **Waiting** | Fewer than one player per team | → Playing when both teams have ≥ 1 player |
| **Playing** | The rule is checked (every `checkIntervalSeconds` on every client for the HUD; only the master acts on it) | → Ended when one or both teams are eliminated; → Waiting if a team's last player leaves |
| **Ended** | Banner shown to everyone (Victory / Defeat / Draw); input still allowed but damage ignored | → Resetting after the announcement time |
| **Resetting** | The master restores the arena and respawns everyone | → Playing (or Waiting if a team is now empty) |

### Interactions with Other Systems

- **Repair Pads / Deploy**: pads get the deployer's team; the master tracks the pad count per team (pad created or destroyed).
- **Death & Respawn**: respawning goes through the own-team-pad filter; spectating when no pad exists.
- **Teams**: all comparisons go through `Unit.SameTeam`; teams come from `PunTeams` for players and from `Unit.team` for structures.
- **Self-Destruct**: can destroy your own team's last pad (currently allowed; the friendly-fire question is open). That can eliminate your own team.
- **Networking**: the master evaluates; a master hand-over re-evaluates from the current state (nothing is cached only on the old master).
- **HUD**: the banner, plus a per-team count of pads and living tanks.

## Formulas

Elimination, evaluated on the master for each team `T`:

```text
padsAlive(T)  = count of RepairPad units with SameTeam(team, T) still in the arena (the master removes a pad at 0 health)
tanksAlive(T) = count of player tanks on T with health > 0
eliminated(T) = armed AND padsAlive(T) == 0 AND tanksAlive(T) == 0
```

Result: `red` wins if `eliminated(blue) && !eliminated(red)`; `blue` wins if the reverse; a draw if
both are eliminated.

## Edge Cases

1. **Last pad destroyed while teammates are mid-respawn countdown (the 5 s `destroyDelayWhenDead`):** they become spectators; they're dead, so they don't count as living.
2. **A team deploys a new pad while it has no pads:** spectators respawn immediately at it; the team is no longer close to elimination.
3. **The last living tank self-destructs, destroying its own last pad:** the team is eliminated, and it's a draw if the other team was also eliminated in that blast.
4. **A player disconnects:** their tank is removed, so it no longer counts as living. If that empties a team, the match returns to Waiting (no winner by forfeit in the MVP).
5. **Master hand-over mid-match:** the new master re-evaluates from the scene; the result is not lost.
6. **A neutral (team-less) pad, e.g. legacy data:** belongs to no team, so it is never offered for respawn.
7. **A pad destroyed during Resetting:** ignored; the reset restores the starting pads.
8. **Two players click the same pad at once:** both respawn (pads aren't exclusive).

## Dependencies

- **Teams** (systems index #5) — team of players and structures; `Unit.SameTeam`.
- **Repair Pads** (#17) and **Deploy** (#15) — pad ownership and deployment.
- **Death & Respawn** (#12) — the respawn path changes (team filter, no fallback, spectating).
- **Networking** (#3) — master authority, RPC broadcast.
- **HUD** (#20) — banner and counts.

## Tuning Knobs

| Knob | Default | Range | Notes |
|---|---|---|---|
| `announceSeconds` | 15 | 5–60 | Time from the result to the reset |
| `checkIntervalSeconds` | 0.5 | 0.2–5 | How often the rule is checked (keeps the 2 s result criterion) |
| `minPlayersPerTeam` | 1 | 1–10 | Arms the rule |
| `timeLimitMinutes` | none | — | Not in MVP (owner, 2026-10-26) |

## Visual/Audio Requirements

A full-width Victory / Defeat / Draw banner in the team colour (Readable Battlefield: the team colour
is the loudest thing on screen); a distinct sound for the last enemy pad destroyed.

## Game Feel

The moment the enemy's last pad dies should feel decisive: the banner text "Enemy repair pads
destroyed — finish them!" (or the defensive equivalent for the losing team) turns the match into its
final push.

## UI Requirements

- HUD: per team, pads remaining and tanks alive.
- Spectator overlay when there is no pad: "No repair pads — waiting for your team".
- The end-of-match banner and a reset countdown.

## Cross-References

- `README.md` (the 2018 manual) — the source rule.
- Upstream issues #7 and #13 — the spawn-from-pad intent; #18 — player report from Jan 2018.
- `design/gdd/game-concept.md` — the MVP includes this system.
- `docs/L1-LOCAL-PLAYABILITY.md` rows 10, 11, 13 — respawn, self-destruct and deploy behaviour this changes.

## Acceptance Criteria

1. With both teams armed, destroying a team's last pad and then its last living tank shows **Victory** to the other team and **Defeat** to that team within 2 s, on every client.
2. With no own-team pad, a dead player is **not** respawned (after 30 s they're still dead and spectating), and the arena-centre fallback never fires.
3. Clicking an enemy pad on the respawn map does not respawn the player; clicking an own-team pad does.
4. A pad deployed from cargo has the deploying tank's team (`Unit.team` == the player's team).
5. Deploying a pad while a team has none respawns that team's spectators within 2 s.
6. When both teams are eliminated in the same check, everyone sees **Draw**.
7. `announceSeconds` after the result, the arena is reset (starting pads restored, deployed units cleared) and every player is alive at their team's starting pad with full health and fuel.
8. A lone player in a room (one team empty) never sees a result.
9. The offline gameplay test covers 1–5 and 7 (one team scripted as the enemy), and the online M2 test covers 1 across two clients.

## Open Questions

1. Should scouts count as "tanks" for elimination once they spawn? (Assumed yes.)
2. The self-destruct friendly-fire call (open since 2026-10-20) affects edge case 3.
3. Does the reset rebuild the starting turrets as well as the pads? (Assumed: the whole starting base.)

## Implementation Notes (2026-10-26, `feature/win-condition`)

- `Assets/MatchManager.cs`, added at runtime by `GameManager.Start` (the scenes are binary). The reset
  reloads the arena scene on every client, so the whole starting base comes back (open question 3).
- Covered by the offline gameplay test (gate step 6e): acceptance criteria 2–8. Criteria 1 and 9 are
  also checked online by gate 6c `match` (two clients; added with the C1 fix, #36).
- Differences from this document, kept small on purpose:
  - after a reset, tanks start at their team's spawn point (`GameManager.spawnPoints*`), next to
    the base, rather than on the pad itself;
  - the HUD is a plain `OnGUI` overlay; the sound for the last enemy pad is not in yet (no asset).
- Known limits: a player who joins after a reset is still sent the room's cached destroy events
  for scene objects destroyed earlier (PUN caches them), and remote units' health reads 0 for a late
  joiner until it next changes. If such a player becomes master while a team has no pads, they
  could misjudge that team's living tanks. Both come with the online work.
