# Story 001: Overhead camera and cursor at death, restored at respawn

> **Epic**: Camera & Cursor
> **Status**: Complete
> **Layer**: Core
> **Type**: Integration
> **Estimate**: S
> **Manifest Version**: 2026-10-27
> **Last Updated**: 2026-10-27
> **Completed**: 2026-10-27 (retrofit: built before the suite; proven by the gate)

## Context

**GDD**: `design/gdd/camera-cursor.md`
**Requirement**: `TR-camera-cursor-003`, `TR-camera-cursor-005`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0004: Match flow (respawn flow); ADR-0006: HUD, UI and input mode
**ADR Decision Summary**: The respawn flow shows the overhead map in UI mode and restores the tank camera in driving mode.
**ADR Version**: ADR-0004 2026-10-26; ADR-0006 2026-10-27

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: none

**Control Manifest Rules (this layer)**:
- Required: only Esc and the respawn flow switch the input mode (ADR-0006)
- Forbidden: never write `Cursor.visible` outside `InputMode` (ADR-0006; migration is story 002)
- Guardrail: none

---

## Acceptance Criteria

- [x] AC4: 5 s after death the overhead camera and cursor show; at respawn the tank camera returns and the cursor hides

---

## Implementation Notes

Retrofit record: `GameManager.Respawn` and `RepairPad.OnMouseDown` (#33-#36). No change.

---

## Out of Scope

- Story 002: moving these writes to `InputMode`

---

## Files

- `Assets/GameManager.cs`, `Assets/RepairPad.cs` (as built)
- `Assets/SmokeTest/GameplaySmokeTest.cs` (checks `respawn-view`, `respawn`)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- Gate 6e checks `respawn-view` and `respawn`: VERIFIED in `pr2-gate` (`008bb95`, 3/3), archived in `qa-artifacts/gate-runs/pr2-gate`, `docs/QA-LOG.md`

**Status**: [x] Exists and passes

---

## Dependencies

- Depends on: None
- Unlocks: None
