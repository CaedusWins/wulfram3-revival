# ADR-0002: PUN Classic 1.88 on Photon Cloud, App ID kept out of git

## Status
Accepted

Accepted 2026-10-26 (the owner delegated the call to Claude): decision in force and proven by the gate runs in docs/QA-LOG.md.

## Date
2026-10-26 (retrofit: decided and proven with M2, 2026-10-10)

## Last Verified
2026-10-26

## Decision Makers
Owner; Claude (revival engineering)

## Summary
Online play uses the PUN Classic 1.88 client already in the 2018 code, connected to the owner's free
Photon Cloud app (20 CCU, region `us`), instead of the dead 2018 endpoint or a migration to PUN2. The
App ID never enters git: it is read from an environment variable or a gitignored local file.

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Unity 2017.3.0f3 |
| **Domain** | Networking |
| **Layer** | Foundation |
| **Knowledge Risk** | LOW (PUN Classic 1.88 is a 2017–2018 library; its source is in `Assets/Photon Unity Networking`) |
| **References Consulted** | PUN Classic source (`PhotonNetwork.cs`, `NetworkingPeer.cs`, `PhotonView.cs`); `Assets/Networking/PhotonCloudSettings.cs`; `CLAUDE.md` M2 notes |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | Gate step 6c (`--online`): two clients in one Photon Cloud room, damage both ways, regeneration |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | ADR-0001 |
| **Enables** | ADR-0003, ADR-0004 |
| **Blocks** | None (already in force) |
| **Ordering Note** | PUN2 needs Unity 2017.4.7+; migrating supersedes this ADR together with ADR-0001 |

## Context

### Problem Statement
The 2018 game connected to a self-hosted Photon endpoint and a login backend that no longer exist. The
revival needed working online play without rewriting the networking layer.

### Constraints
- PUN2 requires a newer engine (ADR-0001).
- The free Photon tier allows 20 concurrent players.
- The App ID is a credential: builds contain it, so it must stay out of the public repository and builds
  are shared privately.

### Requirements
- Two or more players in one room, synchronized tanks, damage and units.
- Offline practice play with no server (PUN offline mode).
- No secret in git history.

## Decision
Keep PUN Classic 1.88 and point it at Photon Cloud. `PhotonCloudSettings` resolves the App ID from, in
order, the command line, `WULFRAM_PHOTON_APPID`, then `Assets/Resources/PhotonAppId.local.txt` (gitignored).
Offline play uses `PhotonNetwork.offlineMode` through the same code paths (`-offlinePlay`).

### Architecture Diagram
```text
Launcher (LauncherWithLogin) ──Connect──> Photon Cloud name server → master → game server (room)
        │                                          ▲
        └─ -offlinePlay ──> PhotonNetwork.offlineMode (same RPC/Instantiate paths, no server)
App ID: args → env WULFRAM_PHOTON_APPID → Resources/PhotonAppId.local.txt (gitignored)
```

### Key Interfaces
- `PhotonNetwork.Instantiate` / `PhotonNetwork.Destroy` for every networked object. Shared units (deployed units, dropped cargo) are created by the master with `PhotonNetwork.InstantiateSceneObject`, so they belong to the room (ADR-0003).
- `[PunRPC]` methods invoked through `photonView.RPC`.
- `PhotonCloudSettings.AppIdResource` = `"PhotonAppId.local"`.

### Implementation Guidelines
- The App ID must never be committed, logged in full, or placed in a tracked file.
- Networked objects must be created and removed with `PhotonNetwork.Instantiate`/`Destroy`, never plain `Instantiate`/`Destroy`, while in a room. The one exception is local cleanup after leaving the room (`GameManager.OnLeftRoom` destroys the DontDestroyOnLoad tank, which offline mode doesn't clean up).
- RPC arguments must be types Photon can serialize; offline mode doesn't serialize, so `Tools/check-rpc.sh` (CI) is the guard.
- Code must never depend on PUN2-only APIs.

## Alternatives Considered

### Alternative 1: Migrate to PUN2 now
- **Description**: Run Photon's converter and move to PUN2.
- **Pros**: Maintained library, newer features.
- **Cons**: Needs an engine step first (ADR-0001); large diff with no gameplay benefit yet.
- **Rejection Reason**: PUN Classic already works with today's Photon Cloud (proven by M2).

### Alternative 2: Self-hosted Photon Server or another stack
- **Description**: Host a server, or rewrite on Mirror/Netcode.
- **Pros**: Full control, server authority possible.
- **Cons**: Hosting cost and operations; a rewrite of every networked system.
- **Rejection Reason**: Out of proportion for a fan revival's MVP.

## Consequences

### Positive
- Online play works with no networking rewrite; offline play shares the same code.
- No credential in the repository.

### Negative
- Clients trust each other's RPCs (no server authority beyond the master client); an RPC trust model is M3 work.
- Builds contain the App ID and must be shared privately.
- The 20-CCU free tier caps team size (about 10v10).

## Risks
- **Offline mode hides serialization bugs.** Mitigation: `Tools/check-rpc.sh` in CI (0 findings) and the online gate step.
- **Photon retires PUN Classic support.** Mitigation: the PUN2 step stays planned after an engine step.

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|------------|-------------|--------------------------|
| game-concept.md | Online multiplayer, two teams, Photon Cloud free tier; offline practice | Defines the stack and the offline path |
| systems-index.md | #3 Networking & Match Join, #4 Offline Play | Records the chosen implementation |

## Performance Implications
- **CPU**: Negligible.
- **Memory**: Negligible.
- **Load Time**: Room join is a network round trip (about 100 ms ping in region `us`).
- **Network**: Photon Cloud message limits apply; not measured yet.

## Migration Plan
None now (status quo since M2).

## Validation Criteria
Gate step 6c passes 3/3 under `--stress 3` (last: `wc-gate2`, 2026-10-26); `git log -p` contains no App ID.

## Related
- ADR-0001, ADR-0003, ADR-0004
- `docs/QA-LOG.md` (online runs), `CLAUDE.md` (M2)
