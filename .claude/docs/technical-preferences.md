# Technical Preferences

<!-- project.yaml at the repo root is the machine-readable source of truth for
     engine, specialists, naming, performance, platform, and testing.framework.
     This file is the human-readable LEGACY FALLBACK: agents and skills resolve
     each key from project.yaml first and fall back here only when the
     project.yaml key is absent. /setup-engine dual-writes both.
     Forbidden patterns and allowed libraries are NOT migrated — they live only
     in this file. Populated by /setup-engine; updated as decisions are made. -->
<!-- Configured 2026-10-26 for Wulfram3 (Unity 2017.3 revival). Project facts: CLAUDE.md. -->

## Engine & Language

- **Engine**: Unity 2017.3.0f3 (pinned; `C:/Program Files/Unity/Editor/Unity.exe`)
- **Language**: C# 4.0 (Unity 2017.3 Mono compiler, .NET 3.5-era runtime)
- **Rendering**: Built-in Render Pipeline (URP/HDRP did not exist in 2017.3)
- **Physics**: PhysX (Unity 2017.3 built-in)

## Input & Platform

<!-- Written by /setup-engine. Read by /ux-design, /ux-review, /test-setup, /team-ui, and /dev-story -->
<!-- to scope interaction specs, test helpers, and implementation to the correct input methods. -->

- **Target Platforms**: PC (Windows 64-bit standalone)
- **Input Methods**: Keyboard/Mouse
- **Primary Input**: Keyboard/Mouse (WASD + mouse look; LMB cannon, RMB pulse; Q/Z cargo, ',' deploy, V self-destruct, Tab/T targeting, C camera, Esc cursor)
- **Gamepad Support**: None
- **Touch Support**: None
- **Platform Notes**: Unity's input is read directly with `Input.*` (2017.3 has no Input System package). Unity 2017 can't fake input, so tests call the handlers the input code calls (e.g. `AutoCannon.TryFire`) - see `Assets/SmokeTest/GameplaySmokeTest.cs`.

## Naming Conventions

- **Classes**: PascalCase (a MonoBehaviour's class name must match its file name exactly - CI-enforced)
- **Variables**: camelCase
- **Signals/Events**: PascalCase `On<Event>` (PUN callbacks: `OnJoinedRoom`, `OnPhotonPlayerPropertiesChanged`...)
- **Files**: PascalCase
- **Scenes/Prefabs**: PascalCase; existing `Launcher 1` / `Playground` keep their names (`LoadScene(0)` and scene lists depend on them)
- **Constants**: SCREAMING_SNAKE (UNVERIFIED - the 2018 code mixes styles)

## Performance Budgets

- **Target Framerate**: [TO BE CONFIGURED — none set; the 2018 game has no budget]
- **Frame Budget**: [TO BE CONFIGURED]
- **Draw Calls**: [TO BE CONFIGURED]
- **Memory Ceiling**: [TO BE CONFIGURED]

## Testing

- **Framework**: Project gate `Tools/verify-local.sh` (Unity batch compile/build, scene/cargo/audit checks, `OfflineSmokeTest` and `GameplaySmokeTest` in the built player, online M2 pair, editor Play-mode check) + CI `.github/workflows/checks.yml` (C# 4 guard, script identity, .meta integrity, secret scan, Photon RPC check). No Unity Test Runner tests exist.
- **Minimum Coverage**: every merge passes the full gate on a fresh clone; L1 checklist rows proven per `docs/L1-LOCAL-PLAYABILITY.md`
- **Required Tests**: Balance formulas, gameplay systems, networking (if applicable)

## Forbidden Patterns

<!-- Add patterns that should never appear in this project's codebase -->
- C# 6+ syntax: `?.`, `$"..."`, `nameof(` - the 2017.3 compiler rejects it (CI fails)
- UnityScript (`.js`) and Boo (`.boo`) scripts - unstable legacy compiler on this machine (CI fails)
- Unity APIs newer than 2017.3 (UI Toolkit, Input System, Addressables, DOTS, URP/HDRP, `async`/`await`)
- `[PunRPC]` parameters PUN Classic can't send (components, GameObjects, `ref`/`out`) - `Tools/check-rpc.sh`
- Calling `HitPointsManager.TakeDamage` directly from client code - go through `TellServerTakeDamage` (only the master applies damage)
- Comparing team strings with `==` - use `Unit.SameTeam`
- Committing the Photon App ID (`Assets/Resources/PhotonAppId.local.txt`) or any webhook URL (CI fails)
- Plain `Destroy` on a `PhotonNetwork.Instantiate`d object - use `PhotonNetwork.Destroy`

## Allowed Libraries / Addons

<!-- Add approved third-party dependencies here -->
- Photon PUN Classic 1.88 (networking; PUN2 needs Unity 2017.4.7+, a later engine step)
- Unity UI (UGUI), Unity Standard Assets, the vendored plugins already under `Assets/Plugins/`

## Architecture Decisions Log

<!-- Quick reference linking to full ADRs in docs/architecture/ -->
- [No ADRs yet — use /architecture-decision to create one. Decisions made before adoption are in CLAUDE.md "Decisions Already Made".]

## Engine Specialists

<!-- Written by /setup-engine when engine is configured. -->
<!-- Read by /code-review, /architecture-decision, /architecture-review, and team skills -->
<!-- to know which specialist to spawn for engine-specific validation. -->

- **Primary**: unity-specialist
- **Language/Code Specialist**: unity-specialist (C# 4 / Unity 2017.3)
- **Shader Specialist**: unity-shader-specialist (built-in pipeline shaders)
- **UI Specialist**: unity-ui-specialist (UGUI, not UI Toolkit)
- **Additional Specialists**: none (DOTS / Addressables don't exist in 2017.3); network-programmer for Photon PUN Classic
- **Routing Notes**: every Unity specialist must read `docs/engine-reference/unity/VERSION.md` first - it pins 2017.3

### File Extension Routing

<!-- Skills use this table to select the right specialist per file type. -->
<!-- If a row says [TO BE CONFIGURED], fall back to Primary for that file type. -->

| File Extension / Type | Specialist to Spawn |
|-----------------------|---------------------|
| Game code (primary language) | unity-specialist |
| Shader / material files | unity-shader-specialist |
| UI / screen files | unity-ui-specialist |
| Scene / prefab / level files | unity-specialist |
| Native extension / plugin files | unity-specialist |
| General architecture review | Primary |
