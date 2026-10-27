# ADR-0006: HUD, UI and Input Mode

## Status
Accepted

Accepted 2026-10-27 by the owner (`/architecture-decision accept`). Its only dependency, ADR-0001, is
Accepted; no story was blocked on it yet. The migration to `InputMode` and the HUD rework are stories.

## Date
2026-10-27

## Last Verified
2026-10-27

## Decision Makers
Owner (approach: one `InputMode` helper, 2026-10-27); Claude (draft). Raised by architecture review 4, G1.

## Summary
Nothing owns the switch between driving and using the UI: three scripts write `Cursor.visible` and six read
it as "is the player driving?". One small static `InputMode` class becomes the only writer and the one place
to read the mode, and the HUD's rendering rule is fixed: Unity UI (the `HUDCanvas`) for the HUD, `OnGUI`
only for test and debug overlays, with the existing `OnGUI` match line migrated when the HUD is reworked.

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Unity 2017.3.0f3 |
| **Domain** | Input / UI |
| **Layer** | Core (input mode); Presentation (HUD) |
| **Knowledge Risk** | LOW (`Cursor.visible`, `Cursor.lockState`, UGUI and `OnGUI` are unchanged 2017.3 APIs, already used by the game) |
| **References Consulted** | `docs/engine-reference/unity/VERSION.md`; the game's own `CursorVisibilityManager`, `GUIManager`, `GameManager.Respawn`, `RepairPad.OnMouseDown`, `MatchManager.OnGUI` |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | Gate 6e: `leave-button`, `deploy-cursor`, `respawn` (cursor shown on the map, hidden after respawn), `death` (dead tank can't fire); playtest A3/A4 |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | ADR-0001 (Accepted: engine pin) |
| **Enables** | The HUD rework (UGUI match line, plurals, overlap), Team & Vehicle Selection (19) |
| **Blocks** | Stories that change who shows or hides the cursor, or that add HUD elements |
| **Ordering Note** | The migration to `InputMode` is one story, covered by the existing gate checks; behaviour must not change |

## Context

### Problem Statement
The game has two input modes: **driving** (cursor hidden and locked: movement, mouse look, weapons) and
**UI** (cursor visible and free: the Leave button, the overhead map, pad clicks). The mode is
`Cursor.visible` itself:

- **Writers (3 + 1 leftover):** `CursorVisibilityManager` (Esc toggle, start), `GameManager.Respawn` (shows
  it on the overhead map), `RepairPad.OnMouseDown` (hides it after respawn); `PlayerMove.cs` also writes it
  but is on neither the tank prefab nor Playground (2018 leftover).
- **Readers (6):** `PlayerMovementManager` (drive, look, pulse), `AutoCannon` (fire), `CameraManager` (C swap),
  `GUIManager` (clickable panel), `GroundPlacementController` (deploy click, #50), and the writers themselves.

Each new feature has to know this convention, and any new writer can break the others (for example the
lock mode is applied only by `CursorVisibilityManager`, so the other two writers leave the lock as it was).
Three of the owner's 2026-10-27 rules (deploy, Leave button, HUD) depend on it.

The HUD also mixes two UI systems: the `HUDCanvas` (Unity UI, binary scene) and `MatchManager.OnGUI` (the
match line and result banner, added in code in #33), which overlap on screen.

### Constraints
- Unity 2017.3 and C# 4 (ADR-0001); binary scenes are edited only when unavoidable (architecture principle 5).
- No behaviour change in the migration: the gate's cursor checks must pass unchanged.
- The gameplay test sets `Cursor.visible` directly to simulate the mode; the helper must keep that working.

### Requirements
- One owner of the input mode, readable everywhere (TR-camera-cursor-002).
- Hidden cursor ⇔ driving; visible ⇔ UI; the lock follows the mode (TR-camera-cursor-002, TR-hud-005,
  TR-deploy-003, TR-hover-movement-003).
- A rendering rule for HUD elements (TR-hud-001..006, TR-win-condition-014, TR-cargo-007).

## Decision

1. **`InputMode` (static class, `Com.Wulfram3`)** owns the mode:
   - `InputMode.Driving` — `true` when the cursor is hidden. Every gameplay reader uses it instead of
     `!Cursor.visible`.
   - `InputMode.SetUI(bool ui)` — the only writer: sets `Cursor.visible` and `Cursor.lockState`
     (`None` for UI, `Locked` for driving) together.
   - `InputMode.Toggle()` — what Esc does.
   It wraps `Cursor.visible` rather than keeping its own flag, so the state can never disagree with the
   real cursor, and the gameplay test's direct `Cursor.visible` writes keep working.
2. **Who switches the mode:** Esc (`CursorVisibilityManager`, via `Toggle`); the respawn flow (UI on the
   overhead map, driving after respawn). Nothing else.
3. **HUD rendering:** HUD elements are Unity UI on the `HUDCanvas` (or created in code under it, since the
   scene is binary). `OnGUI` is allowed only for test and debug overlays. The existing `MatchManager.OnGUI`
   match line and banner move to Unity UI in the HUD rework story, which also fixes the overlap and plurals
   (hud.md rules 2 and 3).
4. **Clickable HUD panels** show only in UI mode (`GUIManager`, as today).

### Architecture Diagram

```text
 Esc ──► CursorVisibilityManager ─┐
 death (overhead map) ─► GameManager.Respawn ─┼─► InputMode.SetUI(bool) ──► Cursor.visible + lockState
 respawn ─► RepairPad / GameManager ──────────┘                │
                                                               ▼
   InputMode.Driving ◄── PlayerMovementManager · AutoCannon · CameraManager · GroundPlacementController
   UI mode           ◄── GUIManager (clickable panels)
```

### Key Interfaces

```csharp
namespace Com.Wulfram3 {
    public static class InputMode {
        public static bool Driving { get { return !Cursor.visible; } }
        public static void SetUI(bool ui) {
            Cursor.visible = ui;
            Cursor.lockState = ui ? CursorLockMode.None : CursorLockMode.Locked;
        }
        public static void Toggle() { SetUI(Driving); }
    }
}
```

### Implementation Guidelines
- Replace each `Cursor.visible = x` in game code with `InputMode.SetUI(x)`, and each gameplay read of
  `Cursor.visible` with `InputMode.Driving` (negated as needed). `Tools`-style search: no `Cursor.visible`
  outside `InputMode` and the smoke tests.
- Delete `PlayerMove.cs` only after confirming in the editor that no prefab uses it (binary assets).
- New HUD elements: Unity UI under `HUDCanvas`, created in code where the scene would otherwise need editing.

## Alternatives Considered

### Alternative 1: Keep `Cursor.visible` as the rule, documented only
- **Description**: Write the convention down; name `CursorVisibilityManager` the toggle owner; no code change.
- **Pros**: No work.
- **Cons**: Nothing stops a new writer; the lock mode stays inconsistent across writers.
- **Rejection Reason**: The owner chose one owner in code (2026-10-27).

### Alternative 2: An `InputModeManager` component with events
- **Description**: A scene component keeping its own state and raising `OnModeChanged`.
- **Pros**: Listeners react to changes instead of polling.
- **Cons**: Needs a scene object (binary scene, or created at runtime), a second source of truth that can
  disagree with the real cursor, and rewiring every reader; no reader needs events today.
- **Rejection Reason**: More moving parts than a 4-member static wrapper.

## Consequences

### Positive
- One place decides the mode and the lock; new features read one property.
- The HUD has one rendering rule, which ends the `OnGUI`/canvas overlap at the next rework.

### Negative
- A migration story touching about eight files (behaviour unchanged).
- Until the HUD rework, `MatchManager.OnGUI` remains an accepted exception.

## Risks
- **The migration changes behaviour by accident** (for example the lock on respawn): mitigated by the
  existing gate checks (`respawn`, `leave-button`, `deploy-cursor`, `death`) and the windowed/editor gate.
- **Batch mode has no real cursor**: `Cursor.visible` still reads back what was set (the respawn checks rely
  on it today), so the gate can verify the mode.

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|------------|-------------|--------------------------|
| camera-cursor.md | TR-camera-cursor-002: one input mode followed by all gameplay input and the clickable UI | `InputMode` is the one owner; readers use `InputMode.Driving` |
| hover-movement.md | TR-hover-movement-003: input only with the cursor hidden | `InputMode.Driving` |
| deploy.md | TR-deploy-003: no deploy with the cursor visible | `InputMode.Driving` in `Release` |
| hud.md | TR-hud-005: the Leave button shows with the cursor | UI mode shows clickable panels (`GUIManager`) |
| hud.md | TR-hud-001..004, TR-hud-006: HUD display | Unity UI under `HUDCanvas`; `OnGUI` migrated in the rework |
| win-condition.md | TR-win-condition-014: match line and banner | Moves from `OnGUI` to Unity UI in the HUD rework |
| cargo.md | TR-cargo-007: the hold's content on the HUD | A Unity UI element under `HUDCanvas` |

## Performance Implications
- **CPU**: none measurable (a property read per input check).
- **Memory**: none.
- **Load Time**: none.
- **Network**: none (the input mode is local).

## Migration Plan
One story: add `InputMode`, move the writers and readers, run the full gate. The HUD rework (Unity UI match
line, plurals, overlap, hold display) is a separate story governed by decision 3.

## Validation Criteria
- No `Cursor.visible` in game code outside `InputMode` (search), and the gate's cursor checks pass unchanged.
- The HUD rework story shows the match line without overlapping the status line (playtest, screenshot).

## Related
- ADR-0001 (engine pin); architecture review 4 (G1); `design/gdd/camera-cursor.md`, `hud.md`, `deploy.md`,
  `hover-movement.md`; `docs/architecture/architecture.md` Required ADR #4.
