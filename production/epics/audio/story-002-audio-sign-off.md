# Story 002: Audio sign-off

> **Epic**: Audio
> **Status**: Ready
> **Layer**: Presentation
> **Type**: Visual/Feel
> **Estimate**: S
> **Manifest Version**: 2026-10-27
> **Last Updated**:

## Context

**GDD**: `design/gdd/audio.md`
**Requirement**: `TR-audio-001`, `TR-audio-002`, `TR-audio-004`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0001: Engine pin, Unity 2017.3; ADR-0003: Master authority (replicated events)
**ADR Decision Summary**: Cues play from the scripts and prefabs that own the events; combat cues are heard by every client through the replicated objects.
**ADR Version**: ADR-0001 2026-10-26; ADR-0003 2026-10-27

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: none

**Control Manifest Rules (this layer)**:
- Required: none specific
- Forbidden: none specific
- Guardrail: none

---

## Acceptance Criteria

- [ ] AC1: cannon fire, pick-up, drop and the launcher click play (playtest A16)
- [ ] AC2: jump, take-off and landing play for the owner (playtest A16)
- [ ] AC3: shells and explosions are heard by both players online (playtest B2)
- [ ] Open question 2 answered: should the player who is hit also hear it?

---

## Implementation Notes

No code: the owner's playtest of `builds\wulfram3-dev-7c57587\` (L1 row 18). Missing sounds (self-destruct,
turrets, match end, taking a hit) become their own stories if the owner wants them.

---

## Out of Scope

- Story 001: the owner's engine loop; the engine for other players (Required ADR #6)

---

## Files

- `production/qa/playtests/playtest-2026-10-27-owner.md`, `production/qa/evidence/audio-evidence.md`

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Visual/Feel
**Required evidence**:
- Visual/Feel: playtest rows A16 and B2 + sign-off in `production/qa/evidence/audio-evidence.md`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: None
