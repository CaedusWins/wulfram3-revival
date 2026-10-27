# Story 003: Gate check that the tank hovers

> **Epic**: Hover Movement
> **Status**: Ready
> **Layer**: Core
> **Type**: Integration
> **Estimate**: S
> **Manifest Version**: 2026-10-27
> **Last Updated**:

## Context

**GDD**: `design/gdd/hover-movement.md`
**Requirement**: `TR-hover-movement-006`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0005: Verification gate and branch model
**ADR Decision Summary**: Gameplay behaviour is proven by a gate 6e phase on a fresh clone of the head commit.
**ADR Version**: 2026-10-26

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: none

**Control Manifest Rules (this layer)**:
- Required: new gameplay behaviour adds or updates a gameplay-test phase (gate 6e) in the same PR (ADR-0005)
- Forbidden: never treat CI as gameplay verification (ADR-0005)
- Guardrail: none

---

## Acceptance Criteria

*From GDD `design/gdd/hover-movement.md` AC2 (review 4, G10: the gate drives onto a box but doesn't assert the hover):*

- [ ] The settled tank's height above the terrain below it is within a band around its hover height, checked over at least 2 s
- [ ] The tank never goes below the terrain during the gameplay test

---

## Implementation Notes

After the existing settle loop in `GameplaySmokeTest.Start`, raycast down to the `TerrainCollider` (as
`PlayerMovementManager.FixedUpdate` does) and record the gap for 2 s; log min/max. Pick the band from a
first run's log (test first), then assert it.

---

## Out of Scope

- Story 002: the hover height rate

---

## Files

- `Assets/SmokeTest/GameplaySmokeTest.cs` (a `hover` check)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- Integration: gate 6e check `hover` in `Assets/SmokeTest/GameplaySmokeTest.cs`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: None
