# Story 001: The owner's engine sound loops

> **Epic**: Audio
> **Status**: Complete
> **Layer**: Presentation
> **Type**: Logic
> **Estimate**: S
> **Manifest Version**: 2026-10-27
> **Last Updated**: 2026-10-27
> **Completed**: 2026-10-27 (#50; proven by the gate)

## Context

**GDD**: `design/gdd/audio.md` (cue 1)
**Requirement**: `TR-audio-002`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0001: Engine pin, Unity 2017.3
**ADR Decision Summary**: Unity 2017.3 `AudioSource`; owner-only feedback cues play locally.
**ADR Version**: 2026-10-26

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: none

**Control Manifest Rules (this layer)**:
- Required: none specific
- Forbidden: no post-2017.3 APIs (ADR-0001)
- Guardrail: none

---

## Acceptance Criteria

- [x] The owner's engine `AudioSource` (`tank.ogg`) loops
- [ ] It hums while flying and stops when landed, by ear (owner, playtest A16)

---

## Implementation Notes

Retrofit record: `PlayerMovementManager.Start` (#50). Other players' engines (a landed-state sync) are not
in scope: Required ADR #6, an audio story after the playtest (owner's call).

---

## Out of Scope

- Syncing the engine for other players (TR-audio-003)

---

## Files

- `Assets/Prefabs/PlayerMovementManager.cs` (as built)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Logic
**Required evidence**:
- Gate 6e check `engine-loop`: test-first `pb-test` (loop=False), VERIFIED in `pb-gate` and `pr2-gate`, `docs/QA-LOG.md`

**Status**: [x] Exists and passes

---

## Dependencies

- Depends on: None
- Unlocks: None
