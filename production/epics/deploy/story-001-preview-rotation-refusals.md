# Story 001: Preview, rotation, refusals and the living/cursor rules

> **Epic**: Deploy
> **Status**: Complete
> **Layer**: Core
> **Type**: Logic
> **Estimate**: M
> **Manifest Version**: 2026-10-27
> **Last Updated**: 2026-10-27
> **Completed**: 2026-10-27 (retrofit: #36, #38, #43 and #50; proven by the gate)

## Context

**GDD**: `design/gdd/deploy.md`
**Requirement**: `TR-deploy-001`, `TR-deploy-002`, `TR-deploy-003` (client part), `TR-deploy-005`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0003: Master authority, team identity, room-owned units; ADR-0006: HUD, UI and input mode
**ADR Decision Summary**: A deploy is a request the master checks and fulfils as a room object; the preview is a local, look-only picture; a click in UI mode is a UI click.
**ADR Version**: ADR-0003 2026-10-27; ADR-0006 2026-10-27

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: none

**Control Manifest Rules (this layer)**:
- Required: a deploy is a request to the master via `GameManager.DeployUnit` (ADR-0003)
- Forbidden: never create a shared unit with `PhotonNetwork.Instantiate` on a player's client
- Guardrail: none

---

## Acceptance Criteria

- [x] AC1: `,` with cargo shows an inert preview; `,` again removes it
- [x] AC2: a click deploys the carried unit for the deployer's team as a room object and empties the hold
- [x] AC3: the master refuses an empty hold, a missing prefab and an ended match
- [x] AC4: each wheel notch turns the preview 10°, it holds still, a new preview starts unrotated (#50)
- [x] AC5: a click with the cursor visible or from a dead tank deploys nothing; a dead tank's preview is removed (#50, client side)

---

## Implementation Notes

Retrofit record. No change.

---

## Out of Scope

- Story 002: the edge cases (aimed at the sky; hold emptied while previewing)
- The master-side living check (Required ADR #2, RPC trust; owner deferred it)

---

## Files

- `Assets/GroundPlacementController.cs`, `Assets/GameManager.cs` (as built)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Logic
**Required evidence**:
- Gate 6e checks `deploy`, `deploy-team`, `deploy-refused`, `deploy-rotate`, `deploy-cursor`, `deploy-dead`; online 6c `match`: VERIFIED in `pb-gate` (`5a3282b`) and `pr2-gate` (`008bb95`), test-first run `pb-test`, `docs/QA-LOG.md`

**Status**: [x] Exists and passes

---

## Dependencies

- Depends on: None
- Unlocks: None
