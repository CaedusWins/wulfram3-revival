# Epic: HUD

> **Layer**: Presentation
> **GDD**: design/gdd/hud.md
> **Architecture Module**: `HUDCanvas` scripts (`GUIManager`, `StatusBarController`, `TeamCounter`, `TargetInfoController`), `PlayerInfoController`, `MatchManager.OnGUI` (HUD, #20)
> **Status**: Ready
> **Stories**: Not yet created — run `/create-stories hud`

## Overview

The overlay: hull and fuel bars, team counts, ping/FPS, the target panel, name tags, the match line and the Leave button (fixed in #50). ADR-0006 sets Unity UI for HUD elements. Open work: the HUD rework (the match line to Unity UI, no overlap, "1 pad"), the target panel that never returns (L1 row 9), status line 2, and the hold display (with Cargo).

## Governing ADRs

| ADR | Decision Summary | Engine Risk |
|-----|------------------|-------------|
| ADR-0006: HUD, UI and input mode | InputMode owns driving vs UI; HUD elements in Unity UI, OnGUI for debug only | LOW |
| ADR-0002: PUN Classic on Photon Cloud | PUN Classic 1.88; offline play through the same code paths | LOW |
| ADR-0003: Master authority, team identity, room-owned units | The master decides shared outcomes; teams via Unit.SameTeam; deployed units are room objects; owner-run movement | LOW |
| ADR-0005: Verification gate and branch model | Fresh-clone gate on the head commit; gameplay test phases; local first | LOW |

## GDD Requirements

| TR-ID | Requirement | ADR Coverage |
|-------|-------------|--------------|
| TR-hud-001 | The HUD shows the local player's own tank only: a hull bar (health / max health) and a fuel bar (fuel / max fuel), updated when either changes, the hull bar empty while dead. | ADR-0006, ADR-0003 ✅ |
| TR-hud-002 | The HUD shows each team's player count, updated on join, leave and team change, and correct offline (1 for your team, 0 for the other). | ADR-0006, ADR-0002 ✅ |
| TR-hud-003 | The HUD shows ping and FPS, refreshed about every 0.6 s. | ADR-0006 ✅ |
| TR-hud-004 | The HUD shows the current target's name, type, health, distance and team when the target changes, and recovers when the target leaves and re-enters the screen. | ADR-0006 ✅ |
| TR-hud-005 | The Leave Game button shows only while the cursor is visible (including while dead on the overhead map), and clicking it leaves the room and returns to the launcher. | ADR-0006, ADR-0002 ✅ |
| TR-hud-006 | HUD text and layout: counts read with the correct singular or plural, no element overlaps another, the crosshair marks the screen centre, nothing hides the battlefield's centre, and team colours are red and blue (team-colour readability: TR-game-concept-010; match line and banner: TR-win-condition-014; hold: TR-cargo-007). | ADR-0006 ✅ |
| TR-hud-007 | Acceptance criteria 1-3, 5 and 6 are verified by the manual playtest (A14, A17) and criterion 4 by gates 6e win and 6c match; gate checks for criteria 5 and 6 come with their fixes. | ADR-0005 ✅ |

Requirements covered by Accepted ADRs: **7 / 7**. ADR-0006 was accepted after
architecture review 4, so `docs/architecture/requirements-traceability.md` counts its requirements at the next review.

## Definition of Done

- Every story implemented, reviewed and closed with `/story-done`
- Every acceptance criterion of design/gdd/hud.md verified
- Logic and Integration stories have passing tests in the test root (the gameplay test, `Assets/SmokeTest/`, run by the gate)
- Visual/Feel and UI stories have retained screenshots in `production/qa/evidence/`
  (a lead sign-off too for Visual/Feel)
