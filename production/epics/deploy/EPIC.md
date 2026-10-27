# Epic: Deploy

> **Layer**: Core
> **GDD**: design/gdd/deploy.md
> **Architecture Module**: `GroundPlacementController`, `GameManager.DeployUnit` / `RequestDeploy` (Deploy, #15)
> **Status**: Ready
> **Stories**: 3 stories (see Stories)

## Overview

With cargo on board, `,` shows a local preview, the wheel turns it 10° per notch and a click asks the master to build the unit for the deployer's team. The rotation and the living/cursor rules were fixed in #50; open work is the deploy edge cases (aiming at the sky, the hold emptied while previewing), the master-side living check (with the RPC trust ADR) and the rotation step as a tuning value.

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | [Preview, rotation, refusals and the living/cursor rules](story-001-preview-rotation-refusals.md) | Logic | Complete | ADR-0003, ADR-0006 |
| 002 | [Preview edge cases — aimed at the sky, hold emptied while previewing](story-002-preview-edge-cases.md) | Logic | Ready | ADR-0003 |
| 003 | [Mouse-wheel rotation feel sign-off](story-003-wheel-feel-sign-off.md) | Visual/Feel | Ready | ADR-0003 |

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0003: Master authority, team identity, room-owned units | The master decides shared outcomes; teams via Unit.SameTeam; deployed units are room objects; owner-run movement | LOW |
| ADR-0004: Match flow (MatchManager) | Master-decided result from the scene, reset by scene reload, respawn flow | LOW |
| ADR-0006: HUD, UI and input mode | InputMode owns driving vs UI; HUD elements in Unity UI, OnGUI for debug only | LOW |
| ADR-0005: Verification gate and branch model | Fresh-clone gate on the head commit; gameplay test phases; local first | LOW |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-deploy-001 | With cargo on board, the deploy key shows a local preview of the carried unit (Resources/<content>; nothing for a content with no prefab) at the tank's deploy point, following the tank and facing the tank's facing when shown plus the wheel turns; the key again removes it and the hold keeps its cargo (preview identity: TR-cargo-005). | ADR-0003 ✅ |
| TR-deploy-002 | Each mouse-wheel notch turns the preview exactly 10 degrees about the vertical, it holds still between notches, a new preview starts unrotated, and the deploy request carries the preview's position and rotation (master checks: TR-cargo-004; team: TR-cargo-010). | ADR-0003 ✅ |
| TR-deploy-003 | Only the owner's living tank with the cursor hidden deploys: a click with the cursor visible is a UI click and deploys nothing, and a dead tank's preview is removed and its click deploys nothing. | ❌ No ADR (partial: ADR-0006 decides the cursor rule; the master-side living check waits for Required ADR #2) |
| TR-deploy-004 | The rotation step (5-45 degrees per notch) is tunable within its documented range. | ❌ No ADR (Required ADR #7, tuning path) |
| TR-deploy-005 | The gameplay gate (6e deploy, deploy-team, deploy-refused; 6c match) verifies acceptance criteria 1-3; gate checks for criteria 4 and 5 come with the rotation and living/cursor fixes. | ADR-0005 ✅ |

Requirements covered by Accepted ADRs: **3 / 5**. ADR-0006 was accepted after
architecture review 4, so `docs/architecture/requirements-traceability.md` counts its requirements at the next review.

## Definition of Done

- Every story implemented, reviewed and closed with `/story-done`
- Every acceptance criterion of design/gdd/deploy.md verified
- Logic and Integration stories have passing tests in the test root (the gameplay test, `Assets/SmokeTest/`, run by the gate)
- Visual/Feel and UI stories have retained screenshots in `production/qa/evidence/`
  (a lead sign-off too for Visual/Feel)
