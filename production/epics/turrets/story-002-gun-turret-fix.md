# Story 002: Gun turret — no errors, the GDD's rate and range, a gate phase

> **Epic**: Turrets
> **Status**: Ready
> **Layer**: Feature
> **Type**: Logic
> **Estimate**: M
> **Manifest Version**: 2026-10-27
> **Last Updated**:

## Context

**GDD**: `design/gdd/turrets.md` (rule 4, Known Issues)
**Requirement**: `TR-turrets-006`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0003: Master authority, team identity, room-owned units
**ADR Decision Summary**: Damage is applied through the master; teams via `Unit.SameTeam`.
**ADR Version**: 2026-10-27

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: none

**Control Manifest Rules (this layer)**:
- Required: change health only through `TellServerTakeDamage` (ADR-0003)
- Forbidden: never compare raw team strings
- Guardrail: none

---

## Acceptance Criteria

*From `turrets.md` rule 4 (the owner chose the GDD as the design, 2026-10-27; architecture review 3, F2):*

- [ ] A gun turret fires bursts of `bulletDamageinHitpoints` at `bulletsPerSecond` at an enemy tank within `range` (40)
- [ ] No exception on the master after a missed raycast, and none on non-master clients (`ShowFeedback` no longer reads a target only the master knows)
- [ ] It never damages its own team
- [ ] A gate 6e phase runs a real gun turret against the tank (offline), and gate 6c shows 0 errors with one online

---

## Implementation Notes

`GTManager`: check the raycast result before reading `objectHit.transform`; drop the hard-coded 24.14 m cut-off
and use `range`; fire at `bulletsPerSecond`; send the target position with `SetShooting` (or guard a null
target) so non-masters can draw the tracer. Test first: spawn `BlueGunTurret` against a red target (or a
red tank stand-in) and log damage per second and range.

---

## Out of Scope

- Master-run AI sync (Required ADR #6)

---

## Files

- `Assets/GTManager.cs`
- `Assets/SmokeTest/GameplaySmokeTest.cs` (a `gun-turret` phase)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Logic
**Required evidence**:
- Logic: gate 6e phase `gun-turret` — must exist and pass; online 6c with 0 errors

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: None
