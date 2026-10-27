# Epic: Audio

> **Layer**: Presentation
> **GDD**: design/gdd/audio.md
> **Architecture Module**: the `AudioSource` cues in `PlayerMovementManager`, `AutoCannon`, `CargoManager`, the shell and explosion prefabs (Audio, #22)
> **Status**: Ready
> **Stories**: Not yet created — run `/create-stories audio`

## Overview

Every sound is a classic Wulfram clip played by the script or prefab that owns the event. The owner's engine now loops (#50). Open work: other players' engine sound (needs the landed state synced, Required ADR #6), sounds that don't exist yet (self-destruct, turrets, match end, taking a hit), and evidence beyond the playtest.

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0001: Engine pin, Unity 2017.3 | Stay on Unity 2017.3.0f3 and C# 4 until a deliberate engine step | LOW |
| ADR-0003: Master authority, team identity, room-owned units | The master decides shared outcomes; teams via Unit.SameTeam; deployed units are room objects; owner-run movement | LOW |
| ADR-0005: Verification gate and branch model | Fresh-clone gate on the head commit; gameplay test phases; local first | LOW |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-audio-001 | Combat cues (engine, cannon shot per tracer, shell spawn, explosion) play as 3D sounds on every client in range, driven by networked objects or synced state, not by the owner's client alone. | ❌ No ADR (partial: ADR-0002/0003 decide replication only; no audio ADR) |
| TR-audio-002 | Local feedback cues play for the owner only: jump, take-off, landing, cargo pick-up and drop (when the hold's content changes), the cannon hit (for the shooter), and the launcher click. | ADR-0001, ADR-0003 ✅ |
| TR-audio-003 | The engine sound plays continuously while the tank hovers and stops while it is landed, on every client that hears the tank. | ❌ No ADR (Required ADR #6, landed state sync) |
| TR-audio-004 | Acceptance criteria 1-3 are verified by the manual playtest (A2, A16, B2). | ❌ No ADR (partial: ADR-0005 decides the gate, not playtest evidence) |

Requirements covered by Accepted ADRs: **1 / 4**. ADR-0006 was accepted after
architecture review 4, so `docs/architecture/requirements-traceability.md` counts its requirements at the next review.

## Definition of Done

- Every story implemented, reviewed and closed with `/story-done`
- Every acceptance criterion of design/gdd/audio.md verified
- Logic and Integration stories have passing tests in the test root (the gameplay test, `Assets/SmokeTest/`, run by the gate)
- Visual/Feel and UI stories have retained screenshots in `production/qa/evidence/`
  (a lead sign-off too for Visual/Feel)
