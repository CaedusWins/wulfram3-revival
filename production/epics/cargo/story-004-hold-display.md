# Story 004: Show the hold's content on the HUD

> **Epic**: Cargo
> **Status**: Ready
> **Layer**: Core
> **Type**: UI
> **Estimate**: S
> **Manifest Version**: 2026-10-27
> **Last Updated**:

## Context

**GDD**: `design/gdd/cargo.md` (UI Requirements); `design/gdd/hud.md`
**Requirement**: `TR-cargo-007`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0006: HUD, UI and input mode
**ADR Decision Summary**: HUD elements are Unity UI under `HUDCanvas`, created in code where the binary scene would otherwise need editing; `OnGUI` is for debug only.
**ADR Version**: 2026-10-27

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: UGUI `Text` created at runtime under the canvas

**Control Manifest Rules (this layer)**:
- Required: HUD elements are Unity UI on the `HUDCanvas` (ADR-0006)
- Forbidden: never add HUD elements with `OnGUI`
- Guardrail: none

---

## Acceptance Criteria

- [ ] With cargo on board the HUD shows the content's readable name (e.g. "Cargo: Flak Turret"); empty shows nothing or "Cargo: —"
- [ ] It updates on pick-up, drop and deploy, for the local player only
- [ ] It does not overlap other HUD elements (screenshot)

---

## Implementation Notes

A small component on the tank (owner only) or a HUD script that reads `CargoManager.pickedUpCargo` for the
local tank and sets a `Text` it creates under `HUDCanvas`. A gate check reads the text after pick-up/drop.

---

## Out of Scope

- HUD story 002: the HUD rework (layout of the existing elements)

---

## Files

- `Assets/CargoHoldDisplay.cs` (+ `.meta`) or an addition to `Assets/CargoManager.cs`
- `Assets/SmokeTest/GameplaySmokeTest.cs` (a `hold-display` check)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: UI
**Required evidence**:
- UI: a retained screenshot with cargo on board in `production/qa/evidence/` (gate `--screenshots`), plus gate 6e `hold-display`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: None
