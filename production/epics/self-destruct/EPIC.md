# Epic: Self-Destruct

> **Layer**: Feature
> **GDD**: design/gdd/self-destruct.md
> **Architecture Module**: `selfDestruct` (Self-Destruct, #13)
> **Status**: Ready
> **Stories**: 3 stories (see Stories)

## Overview

V arms a blast that damages enemies only (#43) and kills the tank. Open work from architecture review 3: arming while dead (F3), the blast applied per collider instead of per unit (F6), the untested double-press (F8), and splash damage online (Required ADR #5).

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | [Enemies-only blast, the tank dies, respawn follows](story-001-enemies-only-blast.md) | Logic | Complete | ADR-0003, ADR-0004 |
| 002 | [No arming while dead; V twice arms once](story-002-no-arming-while-dead.md) | Logic | Ready | ADR-0003 |
| 003 | [The blast hits each unit once, not once per collider](story-003-blast-once-per-unit.md) | Logic | Ready | ADR-0003 |

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0002: PUN Classic on Photon Cloud | PUN Classic 1.88; offline play through the same code paths | LOW |
| ADR-0003: Master authority, team identity, room-owned units | The master decides shared outcomes; teams via Unit.SameTeam; deployed units are room objects; owner-run movement | LOW |
| ADR-0004: Match flow (MatchManager) | Master-decided result from the scene, reset by scene reload, respawn flow | LOW |
| ADR-0005: Verification gate and branch model | Fresh-clone gate on the head commit; gameplay test phases; local first | LOW |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-self-destruct-001 | Only the owning client arms the self-destruct (V), on its own living tank, at most once per life; repeat presses while armed do nothing, and the arming effect is shown on every client. | ADR-0002, ADR-0003 ✅ |
| TR-self-destruct-002 | The blast damages only units within sphereRadius whose team is not the tank's (Unit.SameTeam; a team-less unit counts as not friendly), by ceil(maximumDamage x (sphereRadius - d) / sphereRadius), once per unit; friendly tanks, pads and turrets take no blast damage. | ADR-0003 ✅ |
| TR-self-destruct-003 | All blast damage, including the tank's own lethal maximumDamage, is requested from the master through TellServerTakeDamage, and the master ignores it between a match's end and the reset. | ADR-0003, ADR-0004 ✅ |
| TR-self-destruct-004 | A self-destructed tank dies through the normal death and respawn flow and counts for the win condition like any other death. | ADR-0004 ✅ |
| TR-self-destruct-005 | A unit the blast destroys produces its own death explosion (Explosions & Splash), whose splash is not team-filtered and is capped at splashCap; it is separate from the blast. | ❌ No ADR (Required ADR #5, splash damage online) |
| TR-self-destruct-006 | maximumDamage (100-2000) and sphereRadius (3-20 m) are tunable within their documented ranges. | ❌ No ADR (Required ADR #7, tuning path) |
| TR-self-destruct-007 | The gameplay gate (6e self-destruct, self-destruct-friendly) verifies acceptance criteria 1-4. | ADR-0005 ✅ |

Requirements covered by Accepted ADRs: **5 / 7**. ADR-0006 was accepted after
architecture review 4, so `docs/architecture/requirements-traceability.md` counts its requirements at the next review.

## Definition of Done

- Every story implemented, reviewed and closed with `/story-done`
- Every acceptance criterion of design/gdd/self-destruct.md verified
- Logic and Integration stories have passing tests in the test root (the gameplay test, `Assets/SmokeTest/`, run by the gate)
- Visual/Feel and UI stories have retained screenshots in `production/qa/evidence/`
  (a lead sign-off too for Visual/Feel)
