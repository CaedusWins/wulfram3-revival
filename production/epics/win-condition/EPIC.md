# Epic: Win Condition

> **Layer**: Feature
> **GDD**: design/gdd/win-condition.md
> **Architecture Module**: `MatchManager`, `RepairPad`, `GameManager.Respawn` (Death & Respawn #12, Repair Pads #17, Win Condition #18)
> **Status**: Ready
> **Stories**: 2 stories (see Stories)

## Overview

A team wins by destroying every enemy repair pad and tank; the master decides, everyone sees VICTORY/DEFEAT/DRAW and the arena resets (#33-#36). Built and gated online. Open work: late-join sync (Required ADR #1), master hand-over during a reset (untested), the match line moving to Unity UI (with the HUD epic), tuning, and a match-end sound.

## Stories

| # | Story | Type | Status | ADR |
|---|-------|------|--------|-----|
| 001 | [Result, reset, draw and own-team respawn](story-001-result-reset-draw.md) | Integration | Complete | ADR-0003, ADR-0004 |
| 002 | [The master leaves during the result countdown](story-002-master-leaves-during-reset.md) | Integration | Ready | ADR-0004 |

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
| TR-win-condition-001 | Every repair pad has a team; a pad deployed from cargo takes the deploying tank's team. | ADR-0004, ADR-0003 ✅ |
| TR-win-condition-002 | A player respawns only at own-team pads (Unit.SameTeam); enemy and team-less pads are never offered. | ADR-0004, ADR-0003 ✅ |
| TR-win-condition-003 | With no own-team pad a dead player spectates and respawns within 2 s of the team regaining a pad; the arena-centre fallback is removed. | ADR-0004, ADR-0003 ✅ |
| TR-win-condition-004 | A team is eliminated when it has zero repair pads and zero tanks with health above 0 (scouts count as tanks). | ADR-0004, ADR-0003 ✅ |
| TR-win-condition-005 | One team eliminated means the other wins; both eliminated in the same check is a draw. | ADR-0004, ADR-0003 ✅ |
| TR-win-condition-006 | Only the master client evaluates the rule and announces the result by RPC to all, as with damage. | ADR-0004, ADR-0003 ✅ |
| TR-win-condition-007 | The rule arms only when both teams have at least minPlayersPerTeam players; offline play can arm it for tests. | ADR-0004, ADR-0003 ✅ |
| TR-win-condition-008 | announceSeconds after the result the arena resets in the same room: starting base restored, deployed units and cargo cleared, everyone respawned on their team with full health and fuel. | ADR-0004, ADR-0003 ✅ |
| TR-win-condition-009 | Match states Waiting, Playing, Ended, Resetting; damage is ignored while Ended. | ADR-0004, ADR-0003 ✅ |
| TR-win-condition-010 | A master hand-over re-evaluates from the current scene; no match state is cached only on the old master. | ADR-0004, ADR-0003 ✅ |
| TR-win-condition-011 | The result is shown on every client within 2 s of the elimination (checked every checkIntervalSeconds). | ADR-0004, ADR-0003 ✅ |
| TR-win-condition-012 | If a disconnect empties a team the match returns to Waiting; there is no forfeit win. | ADR-0004, ADR-0003 ✅ |
| TR-win-condition-013 | A player who joins after a reset or mid-match sees correct unit health and scene state. | ❌ No ADR (Required ADR #1, join and late-join sync) |
| TR-win-condition-014 | The HUD shows per-team pads and living tanks, the spectator overlay, the result banner in team colour and the reset countdown. | ADR-0006 ✅ |
| TR-win-condition-015 | announceSeconds, checkIntervalSeconds and minPlayersPerTeam are tunable within their documented ranges. | ❌ No ADR (Required ADR #7, tuning path) |
| TR-win-condition-016 | A distinct sound plays when the last enemy pad is destroyed. | ❌ No ADR (no audio ADR) |
| TR-win-condition-017 | The offline gameplay test covers acceptance criteria 1-5 and 7; an online two-client test covers criterion 1. | ADR-0005 ✅ |

Requirements covered by Accepted ADRs: **14 / 17**. ADR-0006 was accepted after
architecture review 4, so `docs/architecture/requirements-traceability.md` counts its requirements at the next review.

## Definition of Done

- Every story implemented, reviewed and closed with `/story-done`
- Every acceptance criterion of design/gdd/win-condition.md verified
- Logic and Integration stories have passing tests in the test root (the gameplay test, `Assets/SmokeTest/`, run by the gate)
- Visual/Feel and UI stories have retained screenshots in `production/qa/evidence/`
  (a lead sign-off too for Visual/Feel)
