# Story 001: Flak turrets find, aim, fire; one hit does at most 70

> **Epic**: Turrets
> **Status**: Complete
> **Layer**: Feature
> **Type**: Logic
> **Estimate**: M
> **Manifest Version**: 2026-10-27
> **Last Updated**: 2026-10-27
> **Completed**: 2026-10-27 (retrofit: team fix 2026-10-20, splash cap and shell-hit-once #43; proven by the gate)

## Context

**GDD**: `design/gdd/turrets.md`
**Requirement**: `TR-turrets-001`, `TR-turrets-003`, `TR-turrets-007`, `TR-turrets-009` (AC1)
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0003: Master authority, team identity, room-owned units
**ADR Decision Summary**: Turrets target the other team via `Unit.SameTeam`; damage through the master; turrets are room objects.
**ADR Version**: 2026-10-27

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: none

**Control Manifest Rules (this layer)**:
- Required: compare teams only with `Unit.SameTeam` (ADR-0003)
- Forbidden: never compare raw team strings
- Guardrail: none

---

## Acceptance Criteria

- [x] AC1: a full-health tank hit once by a flak shell is alive with 30-70 health
- [x] Every Red*/Blue* unit prefab is on its team and turrets shoot the other team (team audit)

---

## Implementation Notes

Retrofit record. No change.

---

## Out of Scope

- Story 002: the gun turret; story 003: AC2/AC3 test gaps
- Master-run AI sync (Required ADR #6); splash online (Required ADR #5)

---

## Files

- `Assets/FlakTurretManager.cs`, `Assets/ExplosionPhysicsForce.cs`, `Assets/Prefabs/PulseShellManager.cs` (as built)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Logic
**Required evidence**:
- Gate 6e checks `turret` and `teams`: VERIFIED in `dc-gate2`, `pb-gate`, `pr2-gate`, `docs/QA-LOG.md`

**Status**: [x] Exists and passes

---

## Dependencies

- Depends on: None
- Unlocks: None
