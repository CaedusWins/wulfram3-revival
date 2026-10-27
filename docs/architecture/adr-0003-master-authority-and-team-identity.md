# ADR-0003: Gameplay outcomes decided by the master client; teams compared only through Unit.SameTeam

## Status
Accepted

Accepted 2026-10-26 (the owner delegated the call to Claude): decision in force and proven by the gate runs in docs/QA-LOG.md.

## Date
2026-10-26 (retrofit: master-applied damage is 2018 design; `Unit.SameTeam` added 2026-10-20)

## Last Verified
2026-10-26

## Decision Makers
Owner; Claude (revival engineering)

## Summary
Health changes and other gameplay outcomes are applied by the Photon master client and broadcast to
everyone; other clients only request them. Team membership is compared only through
`Unit.SameTeam`/`Unit.NormalizeTeam`, because the 2018 data spells teams inconsistently.

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Unity 2017.3.0f3 |
| **Domain** | Networking |
| **Layer** | Core |
| **Knowledge Risk** | LOW |
| **References Consulted** | `Assets/Prefabs/HitPointsManager.cs`, `Assets/Unit.cs`, `Assets/GameManager.cs`; gameplay test team audit |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | Gate 6e `teams` and `turret` checks; 6c damage both ways |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | ADR-0002 |
| **Enables** | ADR-0004 |
| **Blocks** | None (already in force) |
| **Ordering Note** | The RPC trust model (M3) will refine this ADR, not replace it |

## Context

### Problem Statement
Without one authority, two clients can disagree about a unit's health and both act on it. Separately,
the 2018 prefabs and scene spell teams as "blue", "Blue" and "BlueTeam", so every exact string
comparison was a latent bug: flak turrets that shoot "BlueTeam" never targeted "blue" tanks (found by
the gameplay test, 2026-10-20).

### Constraints
- No dedicated server (ADR-0002); the master client is the only shared authority.
- Binary prefabs and scenes can't be bulk-edited safely, so data normalization happens in code.

### Requirements
- One consistent health value per unit on every client.
- Every team check gives the same answer whatever the spelling.

## Decision
- Damage goes through `HitPointsManager.TellServerTakeDamage` → `TakeDamage` RPC to the master, which
  clamps and broadcasts `UpdateHealth` to all. Healing on respawn goes through `TellServerHealth`.
- The master destroys non-player units at 0 health (`GameManager.UnitsHealthUpdated`) and spawns shells
  and explosions.
- **Deployed units belong to the room** (amended 2026-10-26, architecture review C1): a deploy is a
  request to the master (`GameManager.DeployUnit` -> `RequestDeploy`), which creates the unit with
  `PhotonNetwork.InstantiateSceneObject`, the deployer's team as instantiation data. PUN only lets an
  object's owner network-destroy it, so a unit owned by the deploying client could never be removed by
  the master; and PUN removes a leaving player's own objects (`autoCleanUpPlayerObjects`, default true),
  so a team's base vanished with its builder.
- **The master checks every deploy request** (amended 2026-10-26, architecture review 2, N1/N4):
  `GameManager.DeployUnit(cargoManager, position, rotation)` sends only the tank's view ID. The master
  refuses a tank that isn't the sender's, an empty hold, a missing prefab, or a match that has ended;
  it deploys the cargo actually on board with the tank's team and empties the hold. Dropped cargo is a
  room object too (N3). The placement preview is a local, look-only picture with no network identity (N2).
- Teams are compared only with `Unit.SameTeam(a, b)`; `Unit.NormalizeTeam` ignores case, spaces and a
  trailing "team". A unit with no team (`""`) belongs to no one.

### Architecture Diagram
```text
any client ──TellServerTakeDamage──> master: TakeDamage (clamp) ──UpdateHealth (All)──> every client
                                       └─ health 0 & not a player → PhotonNetwork.Destroy + explosion
team checks:  Unit.SameTeam(a, b) == (NormalizeTeam(a) == NormalizeTeam(b))
```

### Key Interfaces
- `HitPointsManager.TellServerTakeDamage(int)`, `TellServerHealth(int)`, RPCs `TakeDamage`, `UpdateHealth`, `SetHealthFromClient`.
- `Unit.NormalizeTeam(string)`, `Unit.SameTeam(string, string)`.

### Implementation Guidelines
- Code must never change `health` directly or call `TakeDamage` locally; it must call `TellServerTakeDamage`/`TellServerHealth`.
- Code must never compare raw team strings with `==`, `Equals` or `ToLower()`; it must use `Unit.SameTeam`, or compare only values that came out of `Unit.NormalizeTeam` (as `MatchManager` does).
- Only the master may destroy shared units or decide shared outcomes (see ADR-0004 for match results).
- Shared units (anything another player can damage or use) must be created by the master as room objects (`PhotonNetwork.InstantiateSceneObject`), never with `PhotonNetwork.Instantiate` on a player's client.

## Alternatives Considered

### Alternative 1: Owner-authoritative health
- **Description**: Each object's owner applies its own damage.
- **Pros**: Lower latency for the owner.
- **Cons**: Scene units have no live owner; two writers per unit; easier cheating.
- **Rejection Reason**: The master already owns scene units and the 2018 code was built this way.

### Alternative 2: Fix team spellings in the data
- **Description**: Rewrite every prefab and scene to canonical names.
- **Pros**: Clean data.
- **Cons**: Binary assets; risky bulk edits; new data could reintroduce variants.
- **Rejection Reason**: Normalizing in one function covers old and new data; `WulframFixUnitTeams` exists for targeted fixes.

## Consequences

### Positive
- One source of truth for health; the online test sees identical values on both clients.
- Team bugs are fixed in one place (turrets fire at the right team since PR #23).

### Negative
- Hits land one round trip later on non-master clients.
- The master is trusted; a modified client as master could cheat (M3 trust model).

## Risks
- **Late joiners see other units' health as 0 until it next changes** (the initial `SetHealth` RPC is not buffered). Mitigation: tracked for the online work; noted in `design/gdd/win-condition.md`.
- **Master hand-over mid-action.** Mitigation: state that decides outcomes is recomputed from the scene (ADR-0004).
- **A second hit on a unit already at 0 health** makes the master call `PhotonNetwork.Destroy` again in the same frame (Unity's `Destroy` is deferred; `UpdateHealth` to All runs locally at once), which can send a duplicate destroy or log an error (`GameManager.UnitsHealthUpdated`). Pre-existing; mitigation: guard against destroying twice when next touched.
- **`SetHealthFromClient` accepts any value from any client** (`HitPointsManager`), so a modified client can heal itself. Mitigation: the M3 RPC trust model must validate it (respawn heal only while dead).
- (Engine specialist review, 2026-10-26: no blocking findings.)

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|------------|-------------|--------------------------|
| systems-index.md | #5 Teams; #6 Health ("damage only via master") | Defines both rules |
| win-condition.md | Core rule 6: only the master evaluates and announces the result | Same authority model |

## Performance Implications
- **CPU**: Negligible.
- **Memory**: None.
- **Load Time**: None.
- **Network**: Two messages per hit (request + broadcast).

## Migration Plan
Gameplay team checks (turrets, targeting, damage) were moved to `Unit.SameTeam` in PR #23. **One gap
remains (found while writing this ADR):** `Unit.Start` maps `team.ToLower()` to `unitTeam`
(`PunTeams.Team`), which the target info panel and player name colours read. A unit spelled
"BlueTeam" therefore shows as team `none` (grey) there. The fix is to switch on `NormalizeTeam(team)`;
**fixed 2026-10-26** with the C1 change (`Unit.Start` now switches on `NormalizeTeam`). The gameplay test's
`team.ToLower() == "red"` checks only read the player tank's canonical team and are harmless, but should
use `SameTeam` when next touched.

## Validation Criteria
Gate 6e `teams` (every Red*/Blue* prefab on its team, turrets shoot the other) and `turret`; 6c damage both ways with `master=True/False` on each side.

## Related
- ADR-0002, ADR-0004
- `docs/L1-LOCAL-PLAYABILITY.md` rows 8 and 14
