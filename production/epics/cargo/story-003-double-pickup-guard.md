# Story 003: One box, one tank: guard the same-frame double pick-up

> **Epic**: Cargo
> **Status**: Ready
> **Layer**: Core
> **Type**: Logic
> **Estimate**: S
> **Manifest Version**: 2026-10-27
> **Last Updated**:

## Context

**GDD**: `design/gdd/cargo.md`
**Requirement**: `TR-cargo-002`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0003: Master authority, team identity, room-owned units
**ADR Decision Summary**: The master decides shared outcomes: one box can be taken by one tank.
**ADR Version**: 2026-10-27

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: PUN drains its incoming queue in one Update and defers `PhotonNetwork.Destroy` (architecture review 3, F7)

**Control Manifest Rules (this layer)**:
- Required: only the master decides shared outcomes (ADR-0003)
- Forbidden: never call local `Destroy` on a networked object in a room
- Guardrail: none

---

## Acceptance Criteria

- [ ] Two pick-up requests for the same box handled in the same master frame: only the first tank gets the cargo; the second hold stays empty
- [ ] Single pick-ups behave as before (`cargo-pickup` stays green)

---

## Implementation Notes

In `GameManager.RequestPickUpCargo`, mark the box as taken (a flag on `Cargo`) before `PhotonNetwork.Destroy`,
and skip a box already taken. Test: call the master-side handler twice in one frame for two holds (offline
the master is local) and check one hold.

---

## Out of Scope

- Sender checks on pick-up/drop (the RPC trust ADR, Required ADR #2)

---

## Files

- `Assets/GameManager.cs`, `Assets/BlueFiles/Cargo.cs`
- `Assets/SmokeTest/GameplaySmokeTest.cs` (a `cargo-once` check)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Logic
**Required evidence**:
- Logic: gate 6e check `cargo-once` — must exist and pass

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: None
