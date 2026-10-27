# Epic: Turrets

> **Layer**: Feature
> **GDD**: design/gdd/turrets.md
> **Architecture Module**: `FlakTurretManager`, `GTManager` (Turrets, #16)
> **Status**: Ready
> **Stories**: 3 stories (see Stories)

## Overview

Deployed flak and gun turrets defend a team's ground; one flak hit does at most 70. Open work: the gun turret's errors and its rate/range against the GDD (owner: the GDD is the design; review 3 F2), the direct `TakeDamage` in splash (F5), test gaps for AC2/AC3 (F8), master-run AI sync (Required ADR #6) and splash online (Required ADR #5).

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | [Flak turrets find, aim, fire; one hit does at most 70](story-001-flak-turret-as-built.md) | Logic | Complete | ADR-0003 |
| 002 | [Gun turret — no errors, the GDD's rate and range, a gate phase](story-002-gun-turret-fix.md) | Logic | Ready | ADR-0003 |
| 003 | [Turret test gaps — a second hit kills, splash never exceeds the cap](story-003-turret-test-gaps.md) | Integration | Ready | ADR-0005 |

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0003: Master authority, team identity, room-owned units | The master decides shared outcomes; teams via Unit.SameTeam; deployed units are room objects; owner-run movement | LOW |
| ADR-0004: Match flow (MatchManager) | Master-decided result from the scene, reset by scene reload, respawn flow | LOW |
| ADR-0005: Verification gate and branch model | Fresh-clone gate on the head commit; gameplay test phases; local first | LOW |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-turrets-001 | A turret targets only tanks (tag Player) whose team is its teamDetect, compared with Unit.SameTeam, and never its own team. | ADR-0003 ✅ |
| TR-turrets-002 | Turret AI (scan, turn, fire) runs only on the master; every client sees the turret's aim and fire, and a master hand-over continues the AI on the new master. | ❌ No ADR (Required ADR #6, master-run AI sync) |
| TR-turrets-003 | Flak shells are spawned by the master (GameManager.SpawnFlakShell); a shell hits once, applying directHitpointsDamage through the master and then one impact explosion. | ADR-0003 ✅ |
| TR-turrets-004 | Explosion splash is min(splashCap, floor(3 x explosionForce / d)) per unit, applied once per body by the master; the cap (30) applies to flak, pulse and death explosions alike, so one flak hit does at most 70. | ADR-0003 ✅ |
| TR-turrets-005 | Online, explosion splash damages every unit in range whichever client controls it, not only bodies the master controls (the turret-level instance of TR-game-concept-009). | ❌ No ADR (Required ADR #5, splash damage online) |
| TR-turrets-006 | Gun turret hit-scan damage (bulletDamageinHitpoints at bulletsPerSecond within range) is applied through the master and never to the turret's own team. | ADR-0003 ✅ |
| TR-turrets-007 | Turrets are room objects: created by the master from cargo, destroyed by the master at 0 health, kept when their builder leaves, and cleared by the arena reset. | ADR-0003, ADR-0004 ✅ |
| TR-turrets-008 | splashCap (0-100), flak reloadTime (2-10 s), flak scanRadius (50-500), flak shell directHitpointsDamage (10-100), gun bulletDamageinHitpoints (1-20) and gun range (10-100) are tunable within their documented ranges. | ❌ No ADR (Required ADR #7, tuning path) |
| TR-turrets-009 | The gameplay gate (6e turret, teams) verifies acceptance criteria 1-4. | ADR-0005 ✅ |

Requirements covered by Accepted ADRs: **6 / 9**. ADR-0006 was accepted after
architecture review 4, so `docs/architecture/requirements-traceability.md` counts its requirements at the next review.

## Definition of Done

- Every story implemented, reviewed and closed with `/story-done`
- Every acceptance criterion of design/gdd/turrets.md verified
- Logic and Integration stories have passing tests in the test root (the gameplay test, `Assets/SmokeTest/`, run by the gate)
- Visual/Feel and UI stories have retained screenshots in `production/qa/evidence/`
  (a lead sign-off too for Visual/Feel)
