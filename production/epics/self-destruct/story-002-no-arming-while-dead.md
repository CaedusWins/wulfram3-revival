# Story 002: No arming while dead; V twice arms once

> **Epic**: Self-Destruct
> **Status**: Ready
> **Layer**: Feature
> **Type**: Logic
> **Estimate**: S
> **Manifest Version**: 2026-10-27
> **Last Updated**:

## Context

**GDD**: `design/gdd/self-destruct.md` (rule 1, edge case 7, AC4)
**Requirement**: `TR-self-destruct-001`, `TR-self-destruct-007`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0003: Master authority, team identity, room-owned units
**ADR Decision Summary**: Only the owner arms its own tank; the effect is shown to all through an RPC; damage goes through the master.
**ADR Version**: 2026-10-27

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: none

**Control Manifest Rules (this layer)**:
- Required: only the master decides shared outcomes (ADR-0003)
- Forbidden: none specific
- Guardrail: none

---

## Acceptance Criteria

*From `self-destruct.md` rule 1 (a living tank) and AC4 (architecture review 3, F3 and F8):*

- [ ] `Trigger` on a dead or spectating tank does nothing (no effect, no blast)
- [ ] Pressing V twice while armed arms once (one blast, one death)

---

## Implementation Notes

`selfDestruct.Trigger`: return when `HitPointsManager.health <= 0` (or `PlayerMovementManager.isDead`). Test
first: call `Trigger` on the dead tank in the death phase and check no effect; call it twice while armed and
count one blast.

---

## Out of Scope

- Story 003: once per unit
- Open question 1 (a tank killed while armed still explodes): kept as is

---

## Files

- `Assets/selfDestruct.cs`
- `Assets/SmokeTest/GameplaySmokeTest.cs` (checks `self-destruct-dead`, `self-destruct-twice`)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Logic
**Required evidence**:
- Logic: gate 6e checks `self-destruct-dead` and `self-destruct-twice` — must exist and pass

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: None
