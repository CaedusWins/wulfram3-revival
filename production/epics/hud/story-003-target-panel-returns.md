# Story 003: The target panel comes back after its target leaves the screen

> **Epic**: HUD
> **Status**: Ready
> **Layer**: Presentation
> **Type**: UI
> **Estimate**: S
> **Manifest Version**: 2026-10-27
> **Last Updated**:

## Context

**GDD**: `design/gdd/hud.md` (element 6, edge case 3)
**Requirement**: `TR-hud-004`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0006: HUD, UI and input mode
**ADR Decision Summary**: HUD elements are Unity UI under `HUDCanvas`.
**ADR Version**: 2026-10-27

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: the panel's `LateUpdate` body runs only on rendered frames (the windowed gate step exercises it)

**Control Manifest Rules (this layer)**:
- Required: HUD elements are Unity UI on the `HUDCanvas` (ADR-0006)
- Forbidden: never add HUD elements with `OnGUI`
- Guardrail: none

---

## Acceptance Criteria

*From L1 row 9 (known bug) and `hud.md`:*

- [ ] With a target selected, turning away and back: the panel hides while the target is off screen and shows again when it is back
- [ ] Tab/T target cycling is unchanged

---

## Implementation Notes

Find why `TargetInfoController` never re-shows (likely it deactivates its own panel and stops updating).
Test in the windowed gate step (step 7 already exercises the panel: `probePanelShown`): add an off-screen and
back-on-screen probe.

---

## Out of Scope

- Story 002: the HUD rework

---

## Files

- `Assets/TargetInfoController.cs`
- `Assets/SmokeTest/OfflineSmokeTest.cs` (the windowed probe)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: UI
**Required evidence**:
- UI: windowed gate step with the probe passing + a retained screenshot of the panel back on screen in `production/qa/evidence/`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: None
