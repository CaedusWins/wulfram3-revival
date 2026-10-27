# Story 001: Leave Game button shows with the cursor

> **Epic**: HUD
> **Status**: Complete
> **Layer**: Presentation
> **Type**: UI
> **Estimate**: S
> **Manifest Version**: 2026-10-27
> **Last Updated**: 2026-10-27
> **Completed**: 2026-10-27 (#50; proven by the gate)

## Context

**GDD**: `design/gdd/hud.md` (element 11)
**Requirement**: `TR-hud-005`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0006: HUD, UI and input mode; ADR-0002: PUN Classic on Photon Cloud
**ADR Decision Summary**: Clickable HUD panels show only in UI mode; leaving the room returns to the launcher.
**ADR Version**: ADR-0006 2026-10-27; ADR-0002 2026-10-26

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: none

**Control Manifest Rules (this layer)**:
- Required: clickable HUD panels show only in UI mode (ADR-0006)
- Forbidden: never add HUD elements with `OnGUI`
- Guardrail: none

---

## Acceptance Criteria

- [x] AC5: with the cursor visible the Leave Game button shows (hidden while driving) and is wired to `GameManager.LeaveRoom`
- [ ] The real click returns to the launcher (owner, playtest A17)

---

## Implementation Notes

Retrofit record: `GameManager.ShowClickableUI` (#50). The real click is confirmed by the playtest.

---

## Out of Scope

- Story 002: the HUD rework

---

## Files

- `Assets/GameManager.cs` (as built)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: UI
**Required evidence**:
- Gate 6e check `leave-button`: test-first `pb-test` (failed on 2018 code), VERIFIED in `pb-gate` and `pr2-gate`; leaving itself: gate 6b; `docs/QA-LOG.md`

**Status**: [x] Exists and passes

---

## Dependencies

- Depends on: None
- Unlocks: None
