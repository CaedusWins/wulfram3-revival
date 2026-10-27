# Epic: Hover Movement

> **Layer**: Core
> **GDD**: design/gdd/hover-movement.md
> **Architecture Module**: `PlayerMovementManager` (Hover Movement, systems index #1)
> **Status**: Ready
> **Stories**: 3 stories (see Stories)

## Overview

The hovertank floats over the terrain, drives with WASD, aims its hull with the mouse, lands and takes off with Shift/Ctrl and jumps for fuel. The behaviour exists and is owner-run (ADR-0003); this epic covers making it frame-rate independent, tuning it after the playtest, and the gate evidence for hovering that the gameplay test doesn't assert yet.

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | [Movement feel sign-off](story-001-movement-feel-sign-off.md) | Visual/Feel | Ready | ADR-0001, ADR-0006 |
| 002 | [Hover height changes per second, not per frame](story-002-hover-height-per-second.md) | Logic | Ready | N/A |
| 003 | [Gate check that the tank hovers](story-003-hover-gate-check.md) | Integration | Ready | ADR-0005 |

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0001: Engine pin, Unity 2017.3 | Stay on Unity 2017.3.0f3 and C# 4 until a deliberate engine step | LOW |
| ADR-0003: Master authority, team identity, room-owned units | The master decides shared outcomes; teams via Unit.SameTeam; deployed units are room objects; owner-run movement | LOW |
| ADR-0004: Match flow (MatchManager) | Master-decided result from the scene, reset by scene reload, respawn flow | LOW |
| ADR-0006: HUD, UI and input mode | InputMode owns driving vs UI; HUD elements in Unity UI, OnGUI for debug only | LOW |
| ADR-0005: Verification gate and branch model | Fresh-clone gate on the head commit; gameplay test phases; local first | LOW |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-hover-movement-001 | Movement is owner-authoritative: only the owning client reads the tank's input and simulates its rigidbody; every other client keeps the tank kinematic and shows the pose PUN's view sync delivers. | ADR-0003 ✅ |
| TR-hover-movement-002 | Hover, drive, jump, landing and take-off run on the owner's client as rigidbody forces in the physics step, using a downward ray against the TerrainCollider only (reach 2 x height; units, pads and boxes are not ground); landing makes the body kinematic, snapped to the slope, and take-off makes it simulated again. | ADR-0001 ✅ |
| TR-hover-movement-003 | The tank takes movement input (drive, mouse look, hover height, jump) only while it is the owner's living tank and the cursor is hidden (input mode: TR-camera-cursor-002), with no mouse look while landed; a dead tank starts the respawn flow destroyDelayWhenDead after death. | ADR-0006, ADR-0004 ✅ |
| TR-hover-movement-004 | Driving, aiming and hover-height changes do not depend on the frame rate (the hover height changes per second, not per rendered frame). | ❌ No ADR (Required ADR #3, performance budgets) |
| TR-hover-movement-005 | sensitivityX/Y (0.5-5), the pitch limits (30-89 degrees each way), fuelPerJump (0-100), timeBetweenJumps (0.5-10 s) and destroyDelayWhenDead (1-10 s) are tunable within their documented ranges. | ❌ No ADR (Required ADR #7, tuning path) |
| TR-hover-movement-006 | The gameplay gate (6e cargo drive-onto-box, respawn) verifies acceptance criteria 2 and 5; criteria 1, 3 and 4 are verified by the manual playtest (A1, A2). | ADR-0005 ✅ |

Requirements covered by Accepted ADRs: **4 / 6**. ADR-0006 was accepted after
architecture review 4, so `docs/architecture/requirements-traceability.md` counts its requirements at the next review.

## Definition of Done

- Every story implemented, reviewed and closed with `/story-done`
- Every acceptance criterion of design/gdd/hover-movement.md verified
- Logic and Integration stories have passing tests in the test root (the gameplay test, `Assets/SmokeTest/`, run by the gate)
- Visual/Feel and UI stories have retained screenshots in `production/qa/evidence/`
  (a lead sign-off too for Visual/Feel)
