# Control Manifest

> **Engine**: Unity 2017.3.0f3 (C# 4, Mono), PUN Classic 1.88
> **Last Updated**: 2026-10-27
> **Manifest Version**: 2026-10-27
> **ADRs Covered**: ADR-0001, ADR-0002, ADR-0003, ADR-0004, ADR-0005
> **Status**: Active — regenerate with `/create-control-manifest update` when ADRs change

`Manifest Version` is the date this manifest was generated. Stories embed it when
they are written, and `/story-readiness` compares a story's version with this one
to find stories written against old rules. It always equals `Last Updated`: the
same date, for different readers.

This manifest is a programmer's quick reference taken from the Accepted ADRs, the
technical preferences and the engine reference docs. For the reason behind a rule,
read its ADR.

---

## Foundation Layer Rules

*Applies to: engine and language, networking stack, verification and branching*

### Required Patterns

- **Write C# 4 only, as C# scripts; add every new script with its `.meta` file** — source: ADR-0001
- **Never trust a Unity batch-mode exit code; read the log (`compilationhadfailure: False` for every assembly)** — source: ADR-0001
- **In a room, create and remove networked objects with `PhotonNetwork.Instantiate` / `PhotonNetwork.Destroy`; the one exception is local cleanup after leaving the room (`GameManager.OnLeftRoom`)** — source: ADR-0002
- **Shared units (deployed units, dropped cargo) are created by the master with `PhotonNetwork.InstantiateSceneObject`, so they belong to the room** — source: ADR-0002, ADR-0003
- **Give `[PunRPC]` methods only arguments Photon can send; `Tools/check-rpc.sh` must report 0 findings (offline mode doesn't serialize, so it can't catch this)** — source: ADR-0002
- **Offline play goes through the same code paths as online (`PhotonNetwork.offlineMode`, `-offlinePlay`)** — source: ADR-0002
- **Read the Photon App ID only from the command line, `WULFRAM_PHOTON_APPID` or `Assets/Resources/PhotonAppId.local.txt` (gitignored)** — source: ADR-0002
- **Every merge: a fresh-clone `Tools/verify-local.sh` run printing VERIFIED on the exact head commit, archived to `qa-artifacts/gate-runs/<run>` and recorded in `docs/QA-LOG.md`; merge commits only, `gh pr merge --merge --match-head-commit <sha>` after every check run completes** — source: ADR-0005
- **New gameplay behaviour adds or updates a gameplay-test phase (gate 6e) in the same PR; online tests of existing behaviour may be added at any time** — source: ADR-0005
- **A story is done only when it merged with a VERIFIED gate on its head commit and a QA-LOG row** — source: ADR-0005
- **Ask the owner before window-opening gate steps (`--windowed`, `--screenshots`, `--editor`); stop gates only with `Tools/stop-gate.sh`** — source: ADR-0005
- **A failure counts against the code only if it reproduces** — source: ADR-0005

### Forbidden Approaches

- **Never upgrade the engine as a side effect of feature work** — an upgrade breaks binary scenes, PUN Classic and the gate together; it is its own planned step with its own ADR — source: ADR-0001
- **Never use PUN2, UNet, Netcode for GameObjects or another networking stack** — PUN2 needs Unity 2017.4.7+; PUN Classic already works with Photon Cloud — source: ADR-0002
- **Never commit, fully log or place the Photon App ID in a tracked file** — it is a credential — source: ADR-0002
- **Never push to `dev`/`master` directly, use `--admin`, or bypass branch protection** — source: ADR-0005
- **Never treat CI as gameplay verification** — CI can't run Unity; only the local gate builds and plays the game — source: ADR-0005
- **Never start online feature work before L1 is done (local first); online tests of existing behaviour are allowed** — source: ADR-0005

### Performance Guardrails

- **Photon Cloud free tier**: 20 concurrent players (about 10v10) — source: ADR-0002

---

## Core Layer Rules

*Applies to: health and damage, teams, unit ownership, deployment*

### Required Patterns

- **Change health only through `HitPointsManager.TellServerTakeDamage` / `TellServerHealth`; the master clamps it and broadcasts `UpdateHealth` to all** — source: ADR-0003
- **Only the master destroys shared units at 0 health and spawns flak shells and unit death explosions (`GameManager.UnitsHealthUpdated`); a tank's pulse shell and its impact explosion are spawned by the tank's owner (amended 2026-10-27)** — source: ADR-0003
- **Movement is owner-authoritative: only a tank's owner reads input and simulates its rigidbody; other clients keep it kinematic (amended 2026-10-27)** — source: ADR-0003
- **Only the master decides shared outcomes** — source: ADR-0003
- **Compare teams only with `Unit.SameTeam(a, b)`, or compare values that came out of `Unit.NormalizeTeam`; a unit with team `""` belongs to no one** — source: ADR-0003
- **A deploy is a request to the master: `GameManager.DeployUnit(cargoManager, position, rotation)` sends only the tank's view ID; the master refuses a tank that isn't the sender's, an empty hold, a missing prefab or a match that has ended, deploys the cargo actually on board with the tank's team (every deployed unit, whatever the box's colour; amended 2026-10-27), and empties the hold** — source: ADR-0003
- **The placement preview is a local, look-only picture with no network identity** — source: ADR-0003

### Forbidden Approaches

- **Never assign `health` directly or call `TakeDamage` locally** — clients would disagree about a unit's state — source: ADR-0003
- **Never compare raw team strings with `==`, `Equals` or `ToLower()`** — the 2018 data spells teams "blue", "Blue", "BlueTeam" — source: ADR-0003
- **Never move another client's tank; never create a unit other players can damage or use with `PhotonNetwork.Instantiate` on a player's client** — PUN lets only the owner destroy it and deletes a leaving player's objects — source: ADR-0003
- **Never make health owner-authoritative** — scene units have no live owner; two writers per unit — source: ADR-0003
- **Never bulk-rewrite team spellings in binary prefabs or scenes; normalize in code** — source: ADR-0003

---

## Feature Layer Rules

*Applies to: the match flow (win condition, match end, arena reset)*

### Required Patterns

- **Only the master sends `MatchEnded` and `ResetArena`** — source: ADR-0004
- **Derive match state from the scene on every check; never keep it only on the master** — source: ADR-0004
- **Extend `MatchManager` for new match-level rules; don't add a second state machine** — source: ADR-0004
- **Anything that should survive a reset is scene data or re-created by `GameManager.Start`; anything created during a match is a networked object, owned by a client or a room object with `Unit.deployed` set** — source: ADR-0004
- **On reset the master destroys its own match objects before sending `ResetArena`, and each client flushes its destroys (`SendOutgoingCommands`) before `LoadLevel`** — source: ADR-0004
- **Set `MatchManager.ArmWithoutOpponents` only in tests** — source: ADR-0004

### Forbidden Approaches

- **Never add code-only behaviour to a binary scene (e.g. putting `MatchManager` in Playground); add it at runtime** — source: ADR-0004
- **Never reset by rebuilding objects in place; reload the arena** — source: ADR-0004
- **Never reset with `PhotonNetwork.DestroyAll` plus a same-scene `LoadLevel`** — a same-name `LoadLevel` does not reload other clients — source: ADR-0004

### Performance Guardrails

- **MatchManager**: two `FindObjectsOfType` scans per `checkIntervalSeconds` (0.5 s) per client; revisit if the scene grows — source: ADR-0004

---

## Presentation Layer Rules

*Applies to: rendering, audio, UI, VFX*

No Accepted ADR governs this layer yet (architecture review gaps TR-game-concept-010 and TR-win-condition-014/016). The match HUD is a placeholder `OnGUI` overlay (ADR-0004).

---

## Global Rules (All Layers)

### Naming Conventions

| Element | Convention | Example | Source |
|---------|-----------|---------|--------|
| Classes | PascalCase; a MonoBehaviour's class name matches its file name (CI-enforced) | `MatchManager` | `naming.classes` in project.yaml; technical-preferences.md |
| Variables | camelCase | `announceSeconds` | `naming.variables` in project.yaml |
| Signals/Events | PascalCase `On<Event>` | `OnJoinedRoom` | `naming.signals` in project.yaml |
| Files | PascalCase | `MatchManager.cs` | `naming.files` in project.yaml |
| Constants | SCREAMING_SNAKE (**UNVERIFIED**: the 2018 code mixes styles) | `MAX_VIEW_IDS` | `naming.constants` in project.yaml |
| Scenes | PascalCase; `Launcher 1` and `Playground` keep their names (`LoadScene(0)`) | `Playground` | `naming.scenes` in project.yaml |

### Performance Budgets

| Target | Value | Source |
|--------|-------|--------|
| Framerate | TO BE CONFIGURED (none set) | technical-preferences.md |
| Frame budget | TO BE CONFIGURED | technical-preferences.md |
| Draw calls | TO BE CONFIGURED | technical-preferences.md |
| Memory ceiling | TO BE CONFIGURED | technical-preferences.md |

### Approved Libraries / Addons

- Photon PUN Classic 1.88 — approved for networking — source: technical-preferences.md
- Unity UI (UGUI), Unity Standard Assets, the vendored plugins under `Assets/Plugins/` — source: technical-preferences.md

### Forbidden APIs (Unity 2017.3.0f3)

These don't exist in, or don't compile on, the pinned engine:

- C# 6+ syntax: `?.`, `$"..."`, `nameof`, `async`/`await`, tuples, `out var`, expression-bodied members
- URP, HDRP, Shader Graph, SRP Batcher; UI Toolkit; the Input System package; Addressables; DOTS / Entities / Burst
- UnityScript (`.js`) and Boo scripts
- Source: ADR-0001 and `docs/engine-reference/unity/VERSION.md`. (`docs/engine-reference/unity/deprecated-apis.md` describes Unity 6.3 and does not apply to this project.)

### Cross-Cutting Constraints

Forbidden patterns, copied as written in `technical-preferences.md`:

- C# 6+ syntax: `?.`, `$"..."`, `nameof(` - the 2017.3 compiler rejects it (CI fails)
- UnityScript (`.js`) and Boo (`.boo`) scripts - unstable legacy compiler on this machine (CI fails)
- Unity APIs newer than 2017.3 (UI Toolkit, Input System, Addressables, DOTS, URP/HDRP, `async`/`await`)
- `[PunRPC]` parameters PUN Classic can't send (components, GameObjects, `ref`/`out`) - `Tools/check-rpc.sh`
- Calling `HitPointsManager.TakeDamage` directly from client code - go through `TellServerTakeDamage` (only the master applies damage)
- Comparing team strings with `==` - use `Unit.SameTeam`
- Committing the Photon App ID (`Assets/Resources/PhotonAppId.local.txt`) or any webhook URL (CI fails)
- Plain `Destroy` on a `PhotonNetwork.Instantiate`d object - use `PhotonNetwork.Destroy`
