# Story 002: Move the input mode to InputMode

> **Epic**: Camera & Cursor
> **Status**: Ready
> **Layer**: Core
> **Type**: Integration
> **Estimate**: M
> **Manifest Version**: 2026-10-27
> **Last Updated**:

## Context

**GDD**: `design/gdd/camera-cursor.md`
**Requirement**: `TR-camera-cursor-002` (also serves `TR-hover-movement-003`, `TR-deploy-003`, `TR-hud-005`)
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0006: HUD, UI and input mode
**ADR Decision Summary**: A static `InputMode` class owns driving vs UI: `InputMode.Driving` to read, `SetUI(bool)` / `Toggle()` to write (cursor visibility and lock together); only Esc and the respawn flow switch it.
**ADR Version**: 2026-10-27

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: `Cursor.visible` / `Cursor.lockState` (2017.3); batch mode reads back what was set

**Control Manifest Rules (this layer)**:
- Required: read the input mode only through `InputMode.Driving`; change it only through `InputMode.SetUI` / `Toggle` (ADR-0006)
- Forbidden: never write `Cursor.visible` or `Cursor.lockState` elsewhere (the gameplay test excepted)
- Guardrail: none

---

## Acceptance Criteria

*From GDD `design/gdd/camera-cursor.md` rules 3-5 and ADR-0006:*

- [ ] `InputMode` exists as in ADR-0006 Key Interfaces (new script with its `.meta`)
- [ ] No `Cursor.visible` / `Cursor.lockState` in game code outside `InputMode` (search proves it; `PlayerMove.cs` handled per ADR-0006)
- [ ] Behaviour unchanged: gate checks `respawn-view`, `respawn`, `death`, `leave-button`, `deploy-cursor` pass, plus the windowed and editor steps
- [ ] The respawn writers now also set the lock (they used to leave it as it was)

---

## Implementation Notes

From ADR-0006: replace each `Cursor.visible = x` with `InputMode.SetUI(x)` (writers: `CursorVisibilityManager`,
`GameManager.Respawn`, `RepairPad.OnMouseDown`) and each gameplay read with `InputMode.Driving`
(`PlayerMovementManager`, `AutoCannon`, `CameraManager`, `GroundPlacementController`, `GUIManager`). Delete
`PlayerMove.cs` only after confirming in the editor that no prefab uses it.

---

## Out of Scope

- HUD story 002: the HUD rework (Unity UI match line)

---

## Files

- `Assets/InputMode.cs` (+ `.meta`), `Assets/Prefabs/CursorVisibilityManager.cs`, `Assets/GameManager.cs`, `Assets/RepairPad.cs`, `Assets/Prefabs/PlayerMovementManager.cs`, `Assets/AutoCannon.cs`, `Assets/Prefabs/CameraManager.cs`, `Assets/GroundPlacementController.cs`, `Assets/GUIManager.cs`

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- Integration: the existing gate 6e cursor checks and gate steps 7 (windowed) and 8 (editor) VERIFIED on the head commit

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: `production/epics/hud/story-002-hud-rework.md`
