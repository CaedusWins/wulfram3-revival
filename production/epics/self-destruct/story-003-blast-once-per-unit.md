# Story 003: The blast hits each unit once, not once per collider

> **Epic**: Self-Destruct
> **Status**: Ready
> **Layer**: Feature
> **Type**: Logic
> **Estimate**: S
> **Manifest Version**: 2026-10-27
> **Last Updated**:

## Context

**GDD**: `design/gdd/self-destruct.md` (rule 2, Formulas)
**Requirement**: `TR-self-destruct-002`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0003: Master authority, team identity, room-owned units
**ADR Decision Summary**: Damage goes through `TellServerTakeDamage`; teams are compared with `Unit.SameTeam`.
**ADR Version**: 2026-10-27

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: `Physics.OverlapSphere` returns one entry per collider

**Control Manifest Rules (this layer)**:
- Required: change health only through `TellServerTakeDamage` (ADR-0003)
- Forbidden: never call `TakeDamage` locally
- Guardrail: none

---

## Acceptance Criteria

*From `self-destruct.md` Formulas ("once per unit"; architecture review 3, F6):*

- [ ] A unit with several colliders inside the radius takes the formula's damage once
- [ ] The existing `self-destruct` and `self-destruct-friendly` checks stay green

---

## Implementation Notes

In `selfDestruct.doDamage`, collect each `HitPointsManager` (or `attachedRigidbody`) once before applying
damage, as `ExplosionPhysicsForce` does. Test: a multi-collider enemy unit (the flak turret) loses exactly
`ceil(maximumDamage × (r − d) / r)` (capped at its health).

---

## Out of Scope

- Splash online (Required ADR #5)

---

## Files

- `Assets/selfDestruct.cs`
- `Assets/SmokeTest/GameplaySmokeTest.cs` (a `self-destruct-once` check)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Logic
**Required evidence**:
- Logic: gate 6e check `self-destruct-once` — must exist and pass

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: None
