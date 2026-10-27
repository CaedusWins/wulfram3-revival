# Story 003: Mouse-wheel rotation feel sign-off

> **Epic**: Deploy
> **Status**: Ready
> **Layer**: Core
> **Type**: Visual/Feel
> **Estimate**: S
> **Manifest Version**: 2026-10-27
> **Last Updated**:

## Context

**GDD**: `design/gdd/deploy.md`
**Requirement**: `TR-deploy-002`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0003: Master authority, team identity, room-owned units
**ADR Decision Summary**: The deploy request carries the preview's position and rotation.
**ADR Version**: 2026-10-27

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: some touchpads report fractional notches (review 4, G6)

**Control Manifest Rules (this layer)**:
- Required: none specific
- Forbidden: none specific
- Guardrail: none

---

## Acceptance Criteria

- [ ] With a real mouse wheel, one notch turns the preview a visible step and it holds still (playtest A9)
- [ ] The deployed unit faces the way the preview did
- [ ] Game feel: "one notch, one step" feels precise (deploy.md Game Feel)

---

## Implementation Notes

No code: the owner's playtest of `builds\wulfram3-dev-7c57587\` (L1 row 13, the real wheel).

---

## Out of Scope

- Story 002: the preview edge cases

---

## Files

- `production/qa/playtests/playtest-2026-10-27-owner.md`, `production/qa/evidence/deploy-wheel-evidence.md`

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Visual/Feel
**Required evidence**:
- Visual/Feel: playtest row A9 + sign-off in `production/qa/evidence/deploy-wheel-evidence.md`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: None
