# Epic: Cargo

> **Layer**: Core
> **GDD**: design/gdd/cargo.md
> **Architecture Module**: `CargoManager`, `Cargo`, `GameManager.RequestPickUpCargo` / `RequestDropCargo` (Cargo, #14)
> **Status**: Ready
> **Stories**: Not yet created — run `/create-stories cargo`

## Overview

Tanks pick up a box (Q), drop it (Z) and deploy what it holds. Pick-up and drop are master-decided room objects (ADR-0003). Open work: deployed units take the deployer's team whatever the box's colour (TR-cargo-010, review 3 F4), the same-frame double pick-up (F7), the hold's content on the HUD (TR-cargo-007), and the pick-up radius as a tuning value.

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
| TR-cargo-001 | Pick-up (Q, nearest box within the pick-up radius, empty hold only) and drop (Z) are requests to the master, which applies them and sets the hold's content on every client. | ADR-0003 ✅ |
| TR-cargo-002 | When two tanks request the same box, the master gives it to exactly one; the other finds no box. | ADR-0003 ✅ |
| TR-cargo-003 | A dropped box is a room object created by the master with the same content, kept when the dropper or the master leaves, and removed by the arena reset. | ADR-0003, ADR-0004 ✅ |
| TR-cargo-004 | A deploy is a request the master checks (sender owns the tank, hold not empty, content names a prefab, match not ended) and fulfils by creating the carried unit as a room object and emptying the hold. | ADR-0003 ✅ |
| TR-cargo-005 | The deploy placement preview is local to the deploying client, with no network identity and no live behaviour. | ADR-0003 ✅ |
| TR-cargo-006 | A box's content is a prefab name in Assets/Resources; the legacy contents 'Repair Pad' and 'BlueCargo' are corrected to RepairPad and BlueGunTurret, so all 17 arena boxes are deployable. | ❌ No ADR (partial: ADR-0003 decides teams, not content naming) |
| TR-cargo-007 | The HUD shows the content of the local tank's hold. | ADR-0006 ✅ |
| TR-cargo-008 | The pick-up radius (default 5 m) is tunable within 2-10 m. | ❌ No ADR (Required ADR #7, tuning path) |
| TR-cargo-009 | The gameplay gate (6e cargo-contents, cargo-pickup, cargo-drop, deploy, deploy-refused) verifies acceptance criteria 1-3. | ADR-0005 ✅ |
| TR-cargo-010 | Any tank can carry any box; a deployed unit always takes the deploying tank's team, whatever the box's colour, and a deployed turret targets the other team. | ADR-0003 ✅ |

Requirements covered by Accepted ADRs: **8 / 10**. ADR-0006 was accepted after
architecture review 4, so `docs/architecture/requirements-traceability.md` counts its requirements at the next review.

## Definition of Done

- Every story implemented, reviewed and closed with `/story-done`
- Every acceptance criterion of design/gdd/cargo.md verified
- Logic and Integration stories have passing tests in the test root (the gameplay test, `Assets/SmokeTest/`, run by the gate)
- Visual/Feel and UI stories have retained screenshots in `production/qa/evidence/`
  (a lead sign-off too for Visual/Feel)
