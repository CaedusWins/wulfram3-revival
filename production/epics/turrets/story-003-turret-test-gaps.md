# Story 003: Turret test gaps — a second hit kills, splash never exceeds the cap

> **Epic**: Turrets
> **Status**: Ready
> **Layer**: Feature
> **Type**: Integration
> **Estimate**: S
> **Manifest Version**: 2026-10-27
> **Last Updated**:

## Context

**GDD**: `design/gdd/turrets.md` (AC2, AC3)
**Requirement**: `TR-turrets-009`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0005: Verification gate and branch model
**ADR Decision Summary**: Gameplay behaviour is proven by gate 6e phases on a fresh clone of the head commit.
**ADR Version**: 2026-10-26

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: none

**Control Manifest Rules (this layer)**:
- Required: new gameplay behaviour adds or updates a gate 6e phase (ADR-0005)
- Forbidden: none specific
- Guardrail: none

---

## Acceptance Criteria

*From architecture review 3, F8:*

- [ ] AC2: after the first flak hit, a second hit kills the tank (the turret phase waits for it)
- [ ] AC3: every splash amount applied during the gameplay test is logged and none exceeds `splashCap` (30)

---

## Implementation Notes

Extend the existing `turret` phase in `GameplaySmokeTest` to keep the turret firing until a second hit; log
splash amounts through a test hook (or by health deltas minus the 40 direct damage) and assert the cap.

---

## Out of Scope

- Splash online (Required ADR #5)

---

## Files

- `Assets/SmokeTest/GameplaySmokeTest.cs` (checks `turret-second-hit`, `splash-cap`)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- Integration: gate 6e checks `turret-second-hit` and `splash-cap` — must exist and pass

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: None
