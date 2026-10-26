# ADR-0001: Stay on Unity 2017.3.0f3 and C# 4 until a deliberate engine step

## Status
Accepted

Accepted 2026-10-26 (the owner delegated the call to Claude): decision in force and proven by the gate runs in docs/QA-LOG.md.

## Date
2026-10-26 (retrofit: the decision has stood since the revival began, 2026-10-07; the 2018 project was already on 2017.3)

## Last Verified
2026-10-26

## Decision Makers
Owner; Claude (revival engineering)

## Summary
The revived 2018 Wulfram 3 code base is pinned to the engine it was written for, Unity 2017.3.0f3,
compiled as C# 4 by Mono. Upgrading is a separate, deliberate engine step, not a side effect of feature
work, because every upgrade breaks binary scenes, PUN Classic and the proven verification gate at once.

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Unity 2017.3.0f3 (a9f86dcd79df) |
| **Domain** | Core |
| **Layer** | Foundation |
| **Knowledge Risk** | LOW for the API surface (2017.3 is well inside the model's knowledge); the real risk is the reverse: suggesting APIs from newer Unity versions that don't exist here (`docs/engine-reference/unity/VERSION.md`) |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`; `.claude/docs/technical-preferences.md`; editor log headers of every gate run |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | Every change: `Tools/verify-local.sh` compile step (`compilationhadfailure: False` for all four assemblies) and the CI language checks |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | None |
| **Enables** | ADR-0002 (PUN Classic is only possible on this engine), ADR-0003, ADR-0004, ADR-0005 |
| **Blocks** | None (already in force) |
| **Ordering Note** | A future engine step must supersede this ADR and ADR-0002 together: PUN2 needs Unity 2017.4.7+ |

## Context

### Problem Statement
The project is a revival of a 2018 code base, not a new game. Its scenes and prefabs are binary-serialized
for 2017.3, its networking is PUN Classic 1.88, and its asset-store packages target that era. Any engine
upgrade would touch all of these at once and invalidate the evidence the gate has built up.

### Constraints
- Scenes and prefabs are binary; they can only be inspected through `Assets/Editor/WulframSceneDump.cs`.
- The scripting runtime is the .NET 3.5-era Mono; the compiler is C# 4.
- The verification gate (compile, build, smoke, gameplay, online, editor Play) is proven on 2017.3 only.

### Requirements
- Every commit must compile and pass the gate on the pinned editor.
- Code must not use C# 6+ syntax or post-2017.3 Unity APIs.

## Decision
Pin Unity 2017.3.0f3 at `C:/Program Files/Unity/Editor/Unity.exe` (`project.yaml` `engine:`). All code is
C# 4. An engine upgrade happens only as its own planned step with its own branch, ADR and full gate.

### Architecture Diagram
```text
Unity 2017.3.0f3 ── Mono (C# 4, .NET 3.5 profile)
   ├── Built-in render pipeline, PhysX, UGUI + OnGUI, UnityEngine.Input
   └── PUN Classic 1.88 (Assets/Photon Unity Networking)  → ADR-0002
```

### Key Interfaces
None new; this ADR constrains which interfaces may be used.

### Implementation Guidelines
- Code must never use `?.`, `$"..."`, `nameof`, `async`/`await`, tuples, `out var` or expression-bodied members (CI rejects the first three).
- Code must never use URP/HDRP, UI Toolkit, the Input System package, Addressables, DOTS or other post-2017.3 APIs.
- Scripts must be C#; never UnityScript or Boo.
- Batch-mode exit codes must never be trusted; the gate reads the editor log.
- New scripts must be added with their `.meta` file.

## Alternatives Considered

### Alternative 1: Upgrade to a current LTS now
- **Description**: Move to a modern Unity LTS before reviving gameplay.
- **Pros**: Modern APIs, tooling and platform support; PUN2 or Netcode available.
- **Cons**: Breaks binary scenes, PUN Classic and asset packages together, with no working baseline to compare against.
- **Rejection Reason**: The revival first needs a working, tested game; an upgrade without one has nothing to verify against.

### Alternative 2: Step to Unity 2017.4 LTS only (for PUN2)
- **Description**: The smallest step that unlocks PUN2.
- **Pros**: Small API delta; PUN2 becomes possible.
- **Cons**: Still a scene re-serialization and a networking migration; PUN Classic already works with today's Photon Cloud (M2).
- **Rejection Reason**: Not needed yet; kept as the likely first engine step after the MVP.

## Consequences

### Positive
- The 2018 code compiles and runs unchanged; every gate result is comparable with every other.
- The gate's compile step catches language-level mistakes on every change.

### Negative
- No modern Unity features; the model must actively avoid newer APIs.
- The editor is old and crashes occasionally on this machine (mono crashes on exit, seen in `wc-gate`).

## Risks
- **Newer APIs slip into code.** Mitigation: CI language checks, the compile step, and the "not available" table in `VERSION.md`.
- **The old editor's instability is mistaken for code failures.** Mitigation: a failure only counts against the code if it reproduces (`docs/QA-LOG.md` lessons).

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|------------|-------------|--------------------------|
| game-concept.md | Anti-pillar: "Not a modern-engine remake right now: Unity 2017.3 until a deliberate engine step" | Makes the anti-pillar a binding technical decision |

## Performance Implications
- **CPU**: Unchanged from the 2018 game.
- **Memory**: Unchanged.
- **Load Time**: Unchanged.
- **Network**: See ADR-0002.

## Migration Plan
None: this is the status quo. The future engine step will need its own migration plan (scenes, PUN2, packages).

## Validation Criteria
Every gate run reports the editor version `2017.3.0f3` and a clean compile; CI rejects C# 6+ syntax.

## Related
- ADR-0002 (networking stack tied to this engine)
- `docs/engine-reference/unity/VERSION.md`, `.claude/docs/technical-preferences.md`
