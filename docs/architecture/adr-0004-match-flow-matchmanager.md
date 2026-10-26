# ADR-0004: Match flow: a runtime-added MatchManager, master-decided result, reset by scene reload

## Status
Proposed

To be accepted once an online two-client test shows a match result and the arena reset on both clients (GDD acceptance criterion 9). The offline evidence is complete (gate 6e, `wc-gate2`).

## Date
2026-10-26 (retrofit: implemented in PR #33 the same day, before this ADR was written)

## Last Verified
2026-10-26

## Decision Makers
Owner (design: `design/gdd/win-condition.md`); Claude (implementation)

## Summary
The win condition needs a match state machine that every client shows but only one decides, and a way
to start a new match in the same room. `MatchManager` is added at runtime to the GameManager object; the
master evaluates elimination and sends `MatchEnded`, then `ResetArena`, which makes every client destroy
its own network objects and reload the arena scene.

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Unity 2017.3.0f3 |
| **Domain** | Networking |
| **Layer** | Feature |
| **Knowledge Risk** | LOW (PUN Classic source read directly for `LoadLevel`, `DestroyAll`, scene-view caching) |
| **References Consulted** | `PhotonNetwork.LoadLevel`, `NetworkingPeer.SetLevelInPropsIfSynced`, `NetworkingPeer.RemoveInstantiatedGO` / `ServerCleanInstantiateAndDestroy`, `PhotonView.isSceneView` / `instantiationData` |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | Gate 6e `win` phases (offline); an online two-client match end and reset (not yet written) |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | ADR-0002, ADR-0003 |
| **Enables** | Team & Vehicle Selection (#19) and any later match-level rule (time limit, scoring) |
| **Blocks** | None (implemented) |
| **Ordering Note** | Written after implementation; accepting it records the as-built design |

## Context

### Problem Statement
The 2018 game never ended a match. The approved GDD requires elimination (no pads and no living tanks),
a result banner on every client, no damage until the reset, and a fresh match in the same room about
15 s later, with players keeping their teams.

### Constraints
- Scenes are binary: adding a component to the scene is a risky asset edit.
- No server: the master client is the only authority (ADR-0003).
- PUN only lets the owner (or the master, for scene objects and objects of players who left) network-destroy an object.
- `PhotonNetwork.LoadLevel` with the same scene name does not re-sync other clients (`SetLevelInPropsIfSynced` sees no change).
- Destroyed scene objects leave a globally cached destroy event in the room.

### Requirements
- Result decided once, by one client, and survive a master hand-over.
- Reset restores the whole starting base and removes everything created during the match.
- Testable offline with one player.

## Decision
- `GameManager.Start` adds `MatchManager` to its own GameObject, so its RPCs travel on the GameManager's PhotonView (it also refreshes the RPC component cache; see Risks). No scene edit.
- States: `Waiting` → `Playing` (both teams have ≥ `minPlayersPerTeam` tanks) → `Ended` → `Resetting`. Every client computes Waiting/Playing and the counts locally for its HUD; only the master sends `MatchEnded(int result)` (to All).
- Counts come from the scene each check (every 0.5 s): own-team `RepairPad`s that exist (the master destroys a pad at 0 health), and tanks whose `HitPointsManager.health > 0`. No result is cached only on the master, so a new master re-evaluates.
- `announceSeconds` after `Ended`, the master sends `ResetArena` (to All). The master first network-destroys its own match objects (units deployed this match, which are room objects marked `Unit.deployed`, and anything else it created), then sends the RPC, so every client processes those destroys before it reloads (a destroy that arrives after a client's reload finds nothing, and PUN logs "Ev Destroy Failed" as an error). Each client then network-destroys every root object it owns that is not a loaded scene object, and calls `PhotonNetwork.LoadLevel` on the current scene (amended 2026-10-26, architecture review C1). `GameManager.Start` spawns a new tank, keeping the player's existing team.
- `MatchManager.DamageBlocked` makes the master ignore `TakeDamage` while `Ended`/`Resetting`.
- `MatchManager.ArmWithoutOpponents` (static) arms the rule with one team, for the offline test only.

### Architecture Diagram
```text
GameManager GO (scene, PhotonView) ── AddComponent ──> MatchManager
 every 0.5 s on each client: Count() → Waiting | Playing (HUD)
 master only:  eliminated(T) = pads(T)==0 && tanksAlive(T)==0
               └─ MatchEnded(result) ─All─> Ended (banner, DamageBlocked)
               └─ after announceSeconds: ResetArena ─All─> each client:
                     PhotonNetwork.Destroy(own non-scene roots); PhotonNetwork.LoadLevel(scene)
                     → GameManager.Start: new tank, same team; new MatchManager (Waiting)
```

### Key Interfaces
- `MatchManager.Instance`, `State`, `Result {None, RedWins, BlueWins, Draw}`, `ResultFor(team)`, `DamageBlocked`, `SecondsToReset`.
- RPCs `MatchEnded(int)`, `ResetArena()` on the GameManager's PhotonView.
- `RepairPad.Count(team)`, `RepairPad.FindNearestOwn(player)`, `RepairPad.BuildsFor(player)`.
- `GameManager.WaitingForPad` (spectating with no own-team pad).

### Implementation Guidelines
- Only the master must send `MatchEnded` and `ResetArena`.
- Match state must be derived from the scene, never kept only on the master.
- Anything that should survive a reset must be scene data or re-created by `GameManager.Start`; anything created during a match must be a networked object: owned by a client, or a room object the master created with `Unit.deployed` set (ADR-0003).
- Code must never set `ArmWithoutOpponents` outside tests.
- New match-level rules must extend `MatchManager`, not add a second state machine.

## Alternatives Considered

### Alternative 1: Put MatchManager in the scene
- **Description**: Add the component to Playground in the editor.
- **Pros**: Inspector-tunable.
- **Cons**: A binary scene edit for a code-only behaviour; harder to review.
- **Rejection Reason**: Runtime `AddComponent` keeps the change in reviewable code.

### Alternative 2: Reset in place (respawn objects without reloading)
- **Description**: Re-instantiate destroyed scene units and remove match objects by hand.
- **Pros**: No scene load; no message-queue pause.
- **Cons**: Must know and rebuild every starting object; scene views can't be re-created with their IDs.
- **Rejection Reason**: A reload restores the whole base exactly.

### Alternative 3: Master calls `PhotonNetwork.DestroyAll` + `LoadLevel`
- **Description**: One call clears every instantiated object for everyone.
- **Pros**: Simple.
- **Cons**: `LoadLevel` with the same scene name doesn't reload other clients (`SetLevelInPropsIfSynced` returns early when the room property already names the active scene).
- **Rejection Reason**: Every client must reload, so the reset is an RPC to all with a local reload on each; per-client destroy follows from PUN's ownership rules.

## Consequences

### Positive
- A complete match loop with no scene edits; offline-testable end to end (victory, reset, draw).
- The whole starting base returns exactly.

### Negative
- A reset is a full scene load for everyone (a short hitch).
- `OnGUI` HUD is a placeholder for a designed UI.

## Risks
- **Late joiners after a reset** still receive cached destroy events for scene objects destroyed earlier, and read remote health as 0. Mitigation: the online work; documented in the GDD.
- **The online path is only covered by "no false result" so far.** Mitigation: an online two-client match-end test is the next story.
- **Messages arriving during the load.** Mitigation: `LoadLevel` pauses the message queue until the scene is loaded (PUN).
- **Pausing also stops sending** (`PhotonHandler` skips `SendOutgoingCommands` while paused; only acks go out), so a client's destroy events leave after its own load finishes. Order is kept. If the master drops during its load, nobody receives `ResetArena`; mitigation: a client that becomes master resets on its own clock once `announceSeconds` have passed.
- **The explicit destroy is load-bearing**: tanks are DontDestroyOnLoad, so without it the old tank survives the reload. `GameManager.Start` relies on the destroyed tank reading as null.
- **Remote players' old tanks linger** on a reloaded client until their owners' destroy events arrive, reading health 0; only the reloaded pads keep a new master from a false result in that window. Mitigation: the online match-end test must cover a master hand-over during reset.
- **The destroy sweep skips inactive objects** (`FindObjectsOfType` has no include-inactive overload in 2017.3). An inactive owned network object would only be removed locally, and its cached Instantiate would spawn a ghost for late joiners. Mitigation: no networked object is deactivated today; keep it that way or sweep with `Resources.FindObjectsOfTypeAll`.
- **`OnGUI` allocates two `GUIStyle`s and calls `GetComponent` per GUI event** (GC churn). Mitigation: cache them when the HUD is designed properly.
- `RefreshRpcMonoBehaviourCache` after `AddComponent` is not required today (`UseRpcMonoBehaviourCache` is off, so PUN rebuilds the list on every RPC); it is kept so turning the cache on can't break the RPCs.
- (Engine specialist review, 2026-10-26: no blocking findings; every PUN behaviour claim above verified against the PUN 1.88 source.)

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|------------|-------------|--------------------------|
| win-condition.md | Core rules 4–8; States and Transitions; Formulas | MatchManager states, counts and RPCs |
| win-condition.md | Rule 2/3: own-team pads only, no centre fallback | `RepairPad.BuildsFor`, `GameManager.Respawn`/`Update` |
| win-condition.md | Rule 1: deployed pad takes the deployer's team | Instantiation data read in `Unit.Awake` |
| win-condition.md | Edge case 5: master hand-over | Scene-derived state; clock-based reset |

## Performance Implications
- **CPU**: `FindObjectsOfType` twice per 0.5 s per client; negligible at this scene size.
- **Memory**: Negligible.
- **Load Time**: One arena load per match end.
- **Network**: Two RPCs per match, plus each client's destroy events on reset.

## Migration Plan
Implemented in PR #33 (`104704f`). The arena-centre respawn fallback was removed.

## Validation Criteria
Gate 6e: `match-armed`, `victory` (≤ 2 s), `match-over-no-damage`, `reset`, `draw`, `-lone-player`, `-no-pad`, `-new-pad`, `deploy-team` (all PASS, `wc-gate2` 3/3). To add: online two-client victory and reset.

## Related
- ADR-0002, ADR-0003
- `design/gdd/win-condition.md`
