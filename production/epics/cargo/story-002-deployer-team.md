# Story 002: A deployed unit fights for the deployer, whatever the box's colour

> **Epic**: Cargo
> **Status**: Ready
> **Layer**: Core
> **Type**: Logic
> **Estimate**: M
> **Manifest Version**: 2026-10-27
> **Last Updated**:

## Context

**GDD**: `design/gdd/cargo.md` (rule 6); `design/gdd/turrets.md` (rule 1)
**Requirement**: `TR-cargo-010`
*(Requirement text lives in `docs/architecture/tr-registry.yaml` — read fresh at review time)*

**ADR Governing Implementation**: ADR-0003: Master authority, team identity, room-owned units
**ADR Decision Summary**: The master deploys the cargo on board with the tank's team, for every deployed unit whatever the box's colour (amended 2026-10-27), and a turret targets the team that is not its own.
**ADR Version**: 2026-10-27

**Engine**: Unity 2017.3.0f3 | **Risk**: LOW
**Engine Notes**: `PhotonView.instantiationData` is available in `Awake`

**Control Manifest Rules (this layer)**:
- Required: compare teams only with `Unit.SameTeam` / `Unit.NormalizeTeam` (ADR-0003)
- Forbidden: never bulk-rewrite team spellings in binary prefabs; normalize in code
- Guardrail: none

---

## Acceptance Criteria

*From the owner's call (architecture review 3, Q2) and `cargo.md` rule 6:*

- [ ] A blue tank deploying a `RedFlakTurret` box gets a turret on team blue that targets red tanks
- [ ] A red tank deploying a `BlueFlakTurret` or `BlueGunTurret` box gets a red turret that targets blue
- [ ] Team-less prefabs (`RepairPad`) still take the deployer's team (`deploy-team` stays green)
- [ ] The arena's own units (not deployed) keep their prefab teams (`teams` audit stays green)

---

## Implementation Notes

`Unit.Awake`: for a deployed unit (string instantiation data) always take the data's team, not only when the
prefab's is empty. Turrets: derive `teamDetect` as the other team after the team is set (flak and gun
turret managers), or set it from the same data. Write the check first and see it fail on today's code.
Update playtest sheet row A11 for the next build (review 4, G13).

---

## Out of Scope

- Blocking pick-up of the other team's cargo (not chosen)

---

## Files

- `Assets/Unit.cs`, `Assets/FlakTurretManager.cs`, `Assets/GTManager.cs`
- `Assets/SmokeTest/GameplaySmokeTest.cs` (a `deploy-enemy-box` check)

---

## QA Test Cases

*N/A — no qa-lead specs at this tier; implement against the Acceptance Criteria above*

---

## Test Evidence

**Story Type**: Logic
**Required evidence**:
- Logic: gate 6e check `deploy-enemy-box` — must exist and pass; test-first run recorded in `docs/QA-LOG.md`

**Status**: [ ] Not yet created

---

## Dependencies

- Depends on: None
- Unlocks: None
