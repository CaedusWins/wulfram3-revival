# Story 002: Hover height changes per second, not per frame

> **Epic**: Hover Movement
> **Status**: Ready
> **Layer**: Core
> **Type**: Logic
> **Estimate**: S
> **Manifest Version**: 2026-10-27
> **Last Updated**:

## Context

**GDD**: `design/gdd/hover-movement.md`
**Requirement**: `TR-hover-movement-004`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: N/A — frame-rate independence has no architectural choice to make; the budgets ADR (Required ADR #3) will set frame budgets, not this rule
**ADR Decision Summary**: —
**ADR Version**: unversioned

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: `Time.deltaTime` in `Update`; nothing post-2017.3

**Control Manifest Rules (this layer)**:
- Required: movement is owner-authoritative (ADR-0003)
- Forbidden: no C# 6+ syntax or post-2017.3 APIs (ADR-0001)
- Guardrail: none

---

## Acceptance Criteria

*From GDD `design/gdd/hover-movement.md` (Formulas, Game Feel, Known Issues), scoped to this story:*

- [ ] Holding Shift (or Ctrl) changes `height` by the same amount per second at any frame rate (the 2018 step of 0.01 per frame becomes a per-second rate equal to it at 60 fps, 0.6/s)
- [ ] The `TerrainCollider` is looked up once, not every physics step (review 4, G12)
- [ ] "CanLand true/false" is no longer logged every frame
- [ ] The gameplay gate stays VERIFIED (no other movement change)

---

## Implementation Notes

In `PlayerMovementManager.Update`, scale the ±0.01 step by `Time.deltaTime × 60`, or introduce
`heightChangePerSecond = 0.6f`. Cache the terrain collider in `Start` (refetch if null). Remove the
`Debug.Log` calls in the landing path.

---

## Out of Scope

- Tuning the values (Required ADR #7, tuning path)

---

## Files

- `Assets/Prefabs/PlayerMovementManager.cs`
- `Assets/SmokeTest/GameplaySmokeTest.cs` (a `hover-rate` check: the height change over 1 s with Shift's handler called, independent of `Time.captureFramerate` 30 vs 60)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Logic
**Required evidence**:
- Logic: gate 6e check `hover-rate` in `Assets/SmokeTest/GameplaySmokeTest.cs` — must exist and pass

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: None
