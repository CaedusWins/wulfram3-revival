# Story 001: Movement feel sign-off

> **Epic**: Hover Movement
> **Status**: Ready
> **Layer**: Core
> **Type**: Visual/Feel
> **Estimate**: S
> **Manifest Version**: 2026-10-27
> **Last Updated**:

## Context

**GDD**: `design/gdd/hover-movement.md`
**Requirement**: `TR-hover-movement-002`, `TR-hover-movement-003`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0001: Engine pin, Unity 2017.3; ADR-0006: HUD, UI and input mode
**ADR Decision Summary**: Movement uses Unity 2017's rigidbody physics and `Input`; input reaches the tank only in driving mode (cursor hidden).
**ADR Version**: ADR-0001 2026-10-26; ADR-0006 2026-10-27

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: none (existing behaviour; no new API)

**Control Manifest Rules (this layer)**:
- Required: movement is owner-authoritative: only a tank's owner reads input and simulates its rigidbody (ADR-0003)
- Forbidden: never write `Cursor.visible` outside `InputMode` (ADR-0006)
- Guardrail: none

---

## Acceptance Criteria

*From GDD `design/gdd/hover-movement.md`, scoped to this story:*

- [ ] AC1: WASD moves the tank; a visible cursor stops it (playtest A1)
- [ ] AC3: Shift raises the hover and takes off a landed tank; Ctrl lowers it and lands on ground within 0.3 (playtest A2)
- [ ] AC4: Space jumps for 20 fuel, at most once per 3 s, and is refused without fuel (playtest A2)

---

## Implementation Notes

No code: the owner plays `builds\wulfram3-dev-7c57587\` with `production/qa/playtests/playtest-2026-10-27-owner.md`
rows A1/A2 and signs off the feel (L1 row 3). Any finding goes through `/playtest-report analyze` and becomes
its own bug or story.

---

## Out of Scope

- Story 002: frame-rate independence of the hover height
- Story 003: a gate check that the tank hovers

---

## Files

- `production/qa/playtests/playtest-2026-10-27-owner.md` (filled in by the owner)
- `production/qa/evidence/movement-feel-evidence.md`

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Visual/Feel
**Required evidence**:
- Visual/Feel: the owner's filled-in playtest rows A1/A2 + sign-off in `production/qa/evidence/movement-feel-evidence.md`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: None
