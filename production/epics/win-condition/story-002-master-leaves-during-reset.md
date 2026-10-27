# Story 002: The master leaves during the result countdown

> **Epic**: Win Condition
> **Status**: Ready
> **Layer**: Feature
> **Type**: Integration
> **Estimate**: M
> **Manifest Version**: 2026-10-27
> **Last Updated**:

## Context

**GDD**: `design/gdd/win-condition.md`
**Requirement**: `TR-win-condition-010`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0004: Match flow (MatchManager)
**ADR Decision Summary**: Match state is derived from the scene, so a master hand-over loses nothing; the new master runs the reset.
**ADR Version**: 2026-10-26

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: PUN Classic picks a new master when the master leaves; `autoCleanUpPlayerObjects` removes the leaver's objects

**Control Manifest Rules (this layer)**:
- Required: only the master sends `MatchEnded` and `ResetArena` (ADR-0004)
- Forbidden: none specific
- Guardrail: none

---

## Acceptance Criteria

*From `win-condition.md` (TR-win-condition-010, "untested online") and the playtest sheet warning:*

- [ ] With a result showing, the master's client leaves; the remaining client becomes master and the arena still resets within `announceSeconds` (+ a small margin)
- [ ] After the reset the remaining player respawns on their team with full health and fuel, and the match is Waiting (one team only)
- [ ] 0 errors and 0 exceptions on the remaining client

---

## Implementation Notes

Test first in the online pair (gate 6c, `OfflineSmokeTest.OnlineMatch`): after barrier 6 (result), the master
leaves the room; the other client checks the reset. Fix in `MatchManager` only if the test shows it's needed
(for example, recompute `endedAt` when becoming master).

---

## Out of Scope

- Late-join sync (Required ADR #1)

---

## Files

- `Assets/SmokeTest/OfflineSmokeTest.cs` (an online `handover` check)
- `Assets/MatchManager.cs` (only if needed)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- Integration: gate 6c check `handover` — must exist and pass (`--online --stress 3`)

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: None
