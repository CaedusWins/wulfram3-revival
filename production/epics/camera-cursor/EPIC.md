# Epic: Camera & Cursor

> **Layer**: Core
> **GDD**: design/gdd/camera-cursor.md
> **Architecture Module**: `CameraManager`, `CursorVisibilityManager`, `InputMode` (Camera & Cursor, #2)
> **Status**: Ready
> **Stories**: Not yet created — run `/create-stories camera-cursor`

## Overview

The camera rides the owner's tank in a cockpit or chase view (C swaps), and the input mode switches between driving and UI (Esc). ADR-0006 gives the input mode one owner, `InputMode`; this epic covers that migration and the camera's tuning.

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0001: Engine pin, Unity 2017.3 | Stay on Unity 2017.3.0f3 and C# 4 until a deliberate engine step | LOW |
| ADR-0002: PUN Classic on Photon Cloud | PUN Classic 1.88; offline play through the same code paths | LOW |
| ADR-0004: Match flow (MatchManager) | Master-decided result from the scene, reset by scene reload, respawn flow | LOW |
| ADR-0006: HUD, UI and input mode | InputMode owns driving vs UI; HUD elements in Unity UI, OnGUI for debug only | LOW |
| ADR-0005: Verification gate and branch model | Fresh-clone gate on the head commit; gameplay test phases; local first | LOW |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-camera-cursor-001 | Only the local player's tank drives the scene's main camera: it is parented to that tank at start in the cockpit position, and C swaps the cockpit and chase positions over transitionTime only while the cursor is hidden. | ADR-0001, ADR-0002 ✅ |
| TR-camera-cursor-002 | One input mode for the local player: cursor hidden and locked means driving and fighting, cursor visible and free means UI; Esc toggles it, a left click re-applies the lock, the death and respawn flow sets it, and every gameplay input (drive, look, fire, jump, deploy, camera swap) and the clickable UI follow it. | ADR-0006 ✅ |
| TR-camera-cursor-003 | When the respawn flow starts, the overhead map camera replaces the tank camera with the cursor shown; respawn restores the tank camera and hides the cursor. | ADR-0004, ADR-0006 ✅ |
| TR-camera-cursor-004 | transitionTime (0.1-2 s) is tunable within its documented range, and the cockpit and chase positions are tunable values. | ❌ No ADR (Required ADR #7, tuning path) |
| TR-camera-cursor-005 | The gameplay gate (6e respawn) verifies acceptance criterion 4; criteria 1-3 are verified by the manual playtest (A3, A4). | ADR-0005 ✅ |

Requirements covered by Accepted ADRs: **4 / 5**. ADR-0006 was accepted after
architecture review 4, so `docs/architecture/requirements-traceability.md` counts its requirements at the next review.

## Definition of Done

- Every story implemented, reviewed and closed with `/story-done`
- Every acceptance criterion of design/gdd/camera-cursor.md verified
- Logic and Integration stories have passing tests in the test root (the gameplay test, `Assets/SmokeTest/`, run by the gate)
- Visual/Feel and UI stories have retained screenshots in `production/qa/evidence/`
  (a lead sign-off too for Visual/Feel)
