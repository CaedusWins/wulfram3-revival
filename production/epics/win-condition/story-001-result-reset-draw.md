# Story 001: Result, reset, draw and own-team respawn

> **Epic**: Win Condition
> **Status**: Complete
> **Layer**: Feature
> **Type**: Integration
> **Estimate**: L
> **Manifest Version**: 2026-10-27
> **Last Updated**: 2026-10-27
> **Completed**: 2026-10-27 (retrofit: #33-#36; proven offline and online by the gate)

## Context

**GDD**: `design/gdd/win-condition.md`
**Requirement**: `TR-win-condition-001` … `TR-win-condition-012`, `TR-win-condition-017`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0004: Match flow (MatchManager); ADR-0003: Master authority, team identity, room-owned units
**ADR Decision Summary**: The master decides the result from what exists in the scene, sends `MatchEnded`, and resets by scene reload; respawn only at own-team pads.
**ADR Version**: ADR-0004 2026-10-26; ADR-0003 2026-10-27

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: none

**Control Manifest Rules (this layer)**:
- Required: only the master sends `MatchEnded` and `ResetArena` (ADR-0004)
- Forbidden: never compare raw team strings (ADR-0003)
- Guardrail: none

---

## Acceptance Criteria

- [x] A team with no pads and no living tanks loses; both at once is a draw
- [x] No damage between the result and the reset; the arena resets about `announceSeconds` later
- [x] Respawn only at own-team pads; with none, the player waits on the map until one is deployed
- [x] Online, both clients see the result and the reset

---

## Implementation Notes

Retrofit record: `MatchManager`, `RepairPad`, `GameManager.Respawn` (#33-#36). No change.

---

## Out of Scope

- Story 002: master hand-over during a reset
- Late-join sync (Required ADR #1); the match-end sound (no audio ADR); the HUD line's move to Unity UI (HUD story 002)

---

## Files

- `Assets/MatchManager.cs`, `Assets/RepairPad.cs`, `Assets/GameManager.cs` (as built)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- Gate 6e `win` phases (`match-armed`, `victory`, `match-over-no-damage`, `reset`, `draw`) and respawn checks; gate 6c `match` online: VERIFIED in `wc-gate2`, `ro-gate2`, `pr-gate`, `pb-gate`, `pr2-gate`, `docs/QA-LOG.md`

**Status**: [x] Exists and passes

---

## Dependencies

- Depends on: None
- Unlocks: None
