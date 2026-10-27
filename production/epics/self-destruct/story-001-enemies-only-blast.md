# Story 001: Enemies-only blast, the tank dies, respawn follows

> **Epic**: Self-Destruct
> **Status**: Complete
> **Layer**: Feature
> **Type**: Logic
> **Estimate**: M
> **Manifest Version**: 2026-10-27
> **Last Updated**: 2026-10-27
> **Completed**: 2026-10-27 (retrofit: owner's call #42, code #43; proven by the gate)

## Context

**GDD**: `design/gdd/self-destruct.md`
**Requirement**: `TR-self-destruct-002`, `TR-self-destruct-003`, `TR-self-destruct-004`, `TR-self-destruct-007`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0003: Master authority, team identity, room-owned units; ADR-0004: Match flow
**ADR Decision Summary**: Damage goes through the master; friendly units are skipped via `Unit.SameTeam`; no damage after a match ends.
**ADR Version**: ADR-0003 2026-10-27; ADR-0004 2026-10-26

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: none

**Control Manifest Rules (this layer)**:
- Required: change health only through `TellServerTakeDamage` (ADR-0003)
- Forbidden: never compare raw team strings
- Guardrail: none

---

## Acceptance Criteria

- [x] AC1: the blast damages a nearby enemy unit and kills the tank
- [x] AC2: own pads, turrets and teammates in range are untouched (only a destroyed enemy's own death splash, as allowed by edge case 6)
- [x] AC3: the tank dies (health 0) and the respawn flow follows

---

## Implementation Notes

Retrofit record: `selfDestruct` (#43). No change.

---

## Out of Scope

- Story 002: no arming while dead + the double-press test (AC4); story 003: once per unit
- Splash online (Required ADR #5)

---

## Files

- `Assets/selfDestruct.cs` (as built)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Logic
**Required evidence**:
- Gate 6e checks `self-destruct`, `self-destruct-friendly`, `respawn-after-self-destruct*`: VERIFIED in `dc-gate2`, `pb-gate`, `pr2-gate`, `docs/QA-LOG.md`

**Status**: [x] Exists and passes

---

## Dependencies

- Depends on: None
- Unlocks: None
