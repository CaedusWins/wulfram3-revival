# Story 004: HUD sign-off

> **Epic**: HUD
> **Status**: Ready
> **Layer**: Presentation
> **Type**: UI
> **Estimate**: S
> **Manifest Version**: 2026-10-27
> **Last Updated**:

## Context

**GDD**: `design/gdd/hud.md`
**Requirement**: `TR-hud-001`, `TR-hud-002`, `TR-hud-003`, `TR-hud-007`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0006: HUD, UI and input mode
**ADR Decision Summary**: HUD elements are Unity UI under `HUDCanvas`; clickable panels only in UI mode.
**ADR Version**: 2026-10-27

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: none

**Control Manifest Rules (this layer)**:
- Required: HUD elements are Unity UI on the `HUDCanvas` (ADR-0006)
- Forbidden: none specific
- Guardrail: none

---

## Acceptance Criteria

- [ ] AC1: hull and fuel bars track own health and fuel (playtest A14)
- [ ] AC2: team counts match the players in the room, offline and online (playtest A14, B1)
- [ ] AC3: ping and FPS update (playtest A14)
- [ ] The minimap (KGF map system, seen in the `pr2-gate` screenshots) works and is documented in `hud.md`

---

## Implementation Notes

No code unless the playtest finds a bug: the owner's playtest of `builds\wulfram3-dev-7c57587\` (L1 row 17)
plus the gate's retained screenshots. Add the minimap to `hud.md` (element 14) from what the playtest shows.

---

## Out of Scope

- Story 002: the HUD rework; story 003: the target panel

---

## Files

- `production/qa/playtests/playtest-2026-10-27-owner.md`, `production/qa/evidence/hud-evidence.md`, `design/gdd/hud.md`

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: UI
**Required evidence**:
- UI: retained screenshots (`qa-artifacts/gate-runs/pr2-gate/screenshots`, copied to `production/qa/evidence/`) + playtest rows A14/A15

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: None
