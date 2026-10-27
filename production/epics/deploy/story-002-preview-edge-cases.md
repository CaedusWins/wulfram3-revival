# Story 002: Preview edge cases — aimed at the sky, hold emptied while previewing

> **Epic**: Deploy
> **Status**: Ready
> **Layer**: Core
> **Type**: Logic
> **Estimate**: S
> **Manifest Version**: 2026-10-27
> **Last Updated**:

## Context

**GDD**: `design/gdd/deploy.md` (rule 1, edge cases 6-7)
**Requirement**: `TR-deploy-001`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0003: Master authority, team identity, room-owned units
**ADR Decision Summary**: The placement preview is a local, look-only picture with no network identity.
**ADR Version**: 2026-10-27

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: none

**Control Manifest Rules (this layer)**:
- Required: the placement preview is a local, look-only picture (ADR-0003)
- Forbidden: none specific
- Guardrail: none

---

## Acceptance Criteria

*From `deploy.md` rule 1 and review 4 G5:*

- [ ] The preview follows the tank's deploy point every frame, whatever the camera aims at (today it stops while the camera ray hits nothing)
- [ ] When the hold empties while a preview shows (cargo dropped), the preview is removed
- [ ] Existing deploy checks stay green

---

## Implementation Notes

`GroundPlacementController.MoveCurrentObjectToMouse`: set the position from `placeObject` unconditionally (the
raycast result was never used). In `Update`, remove the preview when `baseUnit` becomes empty.

---

## Out of Scope

- Ground snapping or refusing steep/blocked spots (deploy.md open question 1)

---

## Files

- `Assets/GroundPlacementController.cs`
- `Assets/SmokeTest/GameplaySmokeTest.cs` (checks `deploy-follow`, `deploy-hold-emptied`)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Logic
**Required evidence**:
- Logic: gate 6e checks `deploy-follow` and `deploy-hold-emptied` — must exist and pass

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: None
