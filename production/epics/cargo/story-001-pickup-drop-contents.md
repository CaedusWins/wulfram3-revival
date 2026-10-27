# Story 001: Pick up, drop and box contents

> **Epic**: Cargo
> **Status**: Complete
> **Layer**: Core
> **Type**: Integration
> **Estimate**: M
> **Manifest Version**: 2026-10-27
> **Last Updated**: 2026-10-27
> **Completed**: 2026-10-27 (retrofit: built and fixed before the suite; proven by the gate)

## Context

**GDD**: `design/gdd/cargo.md`
**Requirement**: `TR-cargo-001`, `TR-cargo-003`, `TR-cargo-005`, `TR-cargo-006`, `TR-cargo-009`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0003: Master authority, team identity, room-owned units
**ADR Decision Summary**: Pick-up and drop are requests the master fulfils; a dropped box is a room object the arena reset clears.
**ADR Version**: 2026-10-27

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: none

**Control Manifest Rules (this layer)**:
- Required: shared units are created by the master with `InstantiateSceneObject` (ADR-0003)
- Forbidden: never create a unit other players can use with `PhotonNetwork.Instantiate` on a player's client
- Guardrail: none

---

## Acceptance Criteria

- [x] AC1: Q takes the nearest box within 5 m into an empty hold; the box is removed
- [x] AC2: Z drops a box with the same content at the tank, room-owned
- [x] AC3: all 17 arena boxes name a deployable prefab (`Cargo.CorrectedContent`, #43)

---

## Implementation Notes

Retrofit record: `GameManager.RequestPickUpCargo` / `RequestDropCargo` (view-ID RPCs, 2026-10-20; room-owned drop #38); `Cargo.CorrectedContent` (#43). No change.

---

## Out of Scope

- Story 002: the deployed unit's team; story 003: the same-frame double pick-up; story 004: the hold display

---

## Files

- `Assets/GameManager.cs`, `Assets/CargoManager.cs`, `Assets/BlueFiles/Cargo.cs` (as built)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Integration
**Required evidence**:
- Gate 6e checks `cargo-contents`, `cargo-pickup`, `cargo-drop`: VERIFIED in `pr2-gate` (`008bb95`, 3/3), `docs/QA-LOG.md`

**Status**: [x] Exists and passes

---

## Dependencies

- Depends on: None
- Unlocks: None
