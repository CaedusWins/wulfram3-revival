# Story 003: Camera and cursor feel sign-off

> **Epic**: Camera & Cursor
> **Status**: Ready
> **Layer**: Core
> **Type**: Visual/Feel
> **Estimate**: S
> **Manifest Version**: 2026-10-27
> **Last Updated**:

## Context

**GDD**: `design/gdd/camera-cursor.md`
**Requirement**: `TR-camera-cursor-001`, `TR-camera-cursor-005`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0001: Engine pin; ADR-0006: HUD, UI and input mode
**ADR Decision Summary**: The camera rides the local tank; Esc switches between driving and UI mode.
**ADR Version**: ADR-0001 2026-10-26; ADR-0006 2026-10-27

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: none

**Control Manifest Rules (this layer)**:
- Required: only Esc and the respawn flow switch the input mode (ADR-0006)
- Forbidden: none specific
- Guardrail: none

---

## Acceptance Criteria

- [ ] AC1: Esc toggles the cursor between hidden (locked) and visible (playtest A3)
- [ ] AC2: C swaps cockpit and chase views, only while the cursor is hidden (playtest A4)
- [ ] AC3: while the cursor is visible the tank does not drive or fire (playtest A3)
- [ ] Open question 1 answered: is a 1 s swap right?

---

## Implementation Notes

No code: the owner's playtest of `builds\wulfram3-dev-7c57587\` (L1 row 4) signs it off.

---

## Out of Scope

- Story 002: the `InputMode` migration

---

## Files

- `production/qa/playtests/playtest-2026-10-27-owner.md`, `production/qa/evidence/camera-feel-evidence.md`

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Visual/Feel
**Required evidence**:
- Visual/Feel: playtest rows A3/A4 + sign-off in `production/qa/evidence/camera-feel-evidence.md`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: None
