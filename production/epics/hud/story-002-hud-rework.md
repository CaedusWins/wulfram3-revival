# Story 002: HUD rework — match line in Unity UI, no overlap, words agree with numbers

> **Epic**: HUD
> **Status**: Ready
> **Layer**: Presentation
> **Type**: UI
> **Estimate**: M
> **Manifest Version**: 2026-10-27
> **Last Updated**:

## Context

**GDD**: `design/gdd/hud.md` (rules 2-3, elements 8-10)
**Requirement**: `TR-hud-006`, `TR-win-condition-014`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0006: HUD, UI and input mode
**ADR Decision Summary**: HUD elements are Unity UI under `HUDCanvas`, created in code where the binary scene would need editing; the `OnGUI` match line migrates in this rework.
**ADR Version**: 2026-10-27

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: UGUI `Text` created at runtime; `CanvasScaler` on `HUDCanvas`

**Control Manifest Rules (this layer)**:
- Required: HUD elements are Unity UI on the `HUDCanvas` (ADR-0006)
- Forbidden: never add HUD elements with `OnGUI` (this story removes the one exception)
- Guardrail: none

---

## Acceptance Criteria

- [ ] The match line, hint and result banner are Unity UI under `HUDCanvas`; `MatchManager` has no `OnGUI`
- [ ] Words agree with numbers ("1 pad", "2 pads", "1 tank")
- [ ] Nothing overlaps: the match line sits clear of the status line and frame art (screenshot at 1280x720)
- [ ] The `win` phases and online `match` checks stay green (they read the state, not the drawing)

---

## Implementation Notes

Create the texts in code under `HUDCanvas` (as #50 activates `ClickableUI` in code); drive them from
`MatchManager`'s existing state. A gate check reads the texts (e.g. "Red: 1 pad, 0 tanks").

---

## Out of Scope

- Story 003: the target panel; cargo story 004: the hold display; status line 2 (open question)

---

## Files

- `Assets/MatchManager.cs`, a new `Assets/MatchHud.cs` (+ `.meta`) if separate
- `Assets/SmokeTest/GameplaySmokeTest.cs` (a `match-hud` check)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: UI
**Required evidence**:
- UI: retained screenshots (gate `--screenshots`: arena, result banner) in `production/qa/evidence/`, plus gate 6e `match-hud`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: `production/epics/camera-cursor/story-002-inputmode-migration.md`
- Unlocks: None
