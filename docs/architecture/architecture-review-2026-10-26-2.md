## Architecture Review Report
Date: 2026-10-26 (second review of the day)
Mode: full
Workflow: standard
Engine: Unity 2017.3.0f3 (C# 4, PUN Classic 1.88)
GDDs Reviewed: 3 (game-concept.md, systems-index.md, win-condition.md)
ADRs Reviewed: 5 (ADR-0001 to ADR-0005)
Reviewer: independent (did not write any ADR or the C1 fix); review_mode lean, rigor standard, automation guided
Code basis: `313fc62` (Merge PR #37, after PR #36 `3218158`)

<!-- RECEIPT: taken 2026-10-26 after the review and before any write. No TR-registry change is proposed,
     so these lines stand as written if only this report and the traceability index are written. -->
Reviewed-Content-Hash: docs/architecture/adr-0001-engine-pin-unity-2017-3.md 31778ecdc8fed61193cf3dc10cc643d1dc453bc2
Reviewed-Content-Hash: docs/architecture/adr-0002-pun-classic-photon-cloud.md adc6ceacc7759b70cf2c3a06009869c0d475335e
Reviewed-Content-Hash: docs/architecture/adr-0003-master-authority-and-team-identity.md e86228a014b6a70a423f27dcd83a30816ce28c43
Reviewed-Content-Hash: docs/architecture/adr-0004-match-flow-matchmanager.md 4909fac1d479d442619bc054e13015b4071a51a1
Reviewed-Content-Hash: docs/architecture/adr-0005-verification-gate-and-branch-model.md 7ab445bf49b9c5d5aa74082ced38d5017001aa66
Reviewed-Content-Hash: design/gdd/game-concept.md 3a6c4c45e3b8f4d99630b53a2b0e88d25fe670a3
Reviewed-Content-Hash: design/gdd/systems-index.md a8c3d73d5471b1e18eec633f1e2ac0eeec88731c
Reviewed-Content-Hash: design/gdd/win-condition.md 3519e3d49717ab7e49b77b696af9f71612715f17
Reviewed-Content-Hash: docs/architecture/tr-registry.yaml e1b84248da2de0b249ba3d123e1c9efac9ed4439
Reviewed-Content-Hash: docs/engine-reference/unity/PLUGINS.md 8d99063a63269d629be11e9e00e9f460188b689a
Reviewed-Content-Hash: docs/engine-reference/unity/VERSION.md ba94db8b4f876bba132f086ff98845ca3a25aaa5
Reviewed-Content-Hash: docs/engine-reference/unity/breaking-changes.md fb4b916b76b87bf474a4152ce8c64f7c02e7b747
Reviewed-Content-Hash: docs/engine-reference/unity/current-best-practices.md 764d12ee314452d4d814380074ea157d0984ec37
Reviewed-Content-Hash: docs/engine-reference/unity/deprecated-apis.md a8216efd06ba1b7f2ae3bb16ec8b6d3afe902eb8
Reviewed-Content-Hash: docs/engine-reference/unity/modules/animation.md e0601eeba72565d71e1dd2a8bb267eb966d10a5f
Reviewed-Content-Hash: docs/engine-reference/unity/modules/audio.md 56c04c15cb9a356b87a27df5d0fcf67f18db9302
Reviewed-Content-Hash: docs/engine-reference/unity/modules/input.md 1b64a2381167d17c1b796d4d5f93a7e1fcc651e6
Reviewed-Content-Hash: docs/engine-reference/unity/modules/navigation.md 484a867a2fe1acf86b2071f7950b10afaab8ffab
Reviewed-Content-Hash: docs/engine-reference/unity/modules/networking.md ab0ada43362406731784044326eed68fd25aa3b1
Reviewed-Content-Hash: docs/engine-reference/unity/modules/physics.md 191e8d8d502363a99e80670942d251ef819a3976
Reviewed-Content-Hash: docs/engine-reference/unity/modules/rendering.md 292081320361b18c12715f7ed11dae2a38ba5f2d
Reviewed-Content-Hash: docs/engine-reference/unity/modules/ui.md 87de2ff30ce1c8d9499b6bcf084947201852e6ab
Reviewed-Content-Hash: project.yaml 5b2ad3dfb12f044ca9a7c347ed334404bb252fee
Reviewed-Content-Hash: .claude/docs/technical-preferences.md 151ddbdad07920c7f5b48107ba4d259b8643fab3
Reviewed-Content-Hash: .claude/skills/architecture-review/SKILL.md 1a9d28aadbcbd26b5fc6b8ed1c64d73ae82b7b73
Reviewed-Content-Hash: .claude/skills/architecture-review/references/1-load.md 9a65759ebdbb2cb1f9744775bfed18cb25e76347
Reviewed-Content-Hash: .claude/skills/architecture-review/references/2-extract-requirements.md d08ea0b0e9ab9760b0d8bfe0cd9115f0bc40f8e2
Reviewed-Content-Hash: .claude/skills/architecture-review/references/3-traceability.md bb7b6b65c5e5e5b8869fa692413aa831615dede7
Reviewed-Content-Hash: .claude/skills/architecture-review/references/4-conflicts.md 0d30f8173b23248a32523a365b606dd1ce9642ab
Reviewed-Content-Hash: .claude/skills/architecture-review/references/5-engine.md 318ecbc1361d430badaf5dd8ce4cfc6ab0dff0ca
Reviewed-Content-Hash: .claude/skills/architecture-review/references/6-document-coverage.md dcce78482ec2ce60048989b6f9d1f797dd0a9a0e
Reviewed-Content-Hash: .claude/skills/architecture-review/references/7-report.md a007fd66bdac87812dc272e4b14cd231174e979d
Reviewed-Content-Hash: .claude/skills/architecture-review/references/8-write.md bf6d8946fc7b9e19eda7f65503a5c8f593a0d2b1
Reviewed-Content-Hash: .claude/skills/architecture-review/references/9-handoff.md fd5f681f4c3517c6a2e388c6d78075d82dded22e
(not stamped, no such file: docs/architecture/architecture.md, docs/consistency-failures.md)

---

### Load notes (Phase 1)

- **Freshness check** against `architecture-review-2026-10-26.md` (Mode full, Workflow standard, so it covers this run):
  no `UNRESOLVED`, no `GONE`. `CHANGED`: ADR-0003, ADR-0004, ADR-0005, `systems-index.md`, `win-condition.md`,
  `.claude/docs/technical-preferences.md`. A changed non-GDD/ADR input (`technical-preferences.md`) means the
  procedure recommends a full re-run, not a `single-gdd` scope. This run is that full re-run.
- Loaded 3 GDDs (N_gdd = 3) and 5 ADRs (N_adr = 5). All 5 ADRs carry `## Status`, `## Engine Compatibility`,
  `## ADR Dependencies`, `## GDD Requirements Addressed`. No structural gaps.
- Status: **all 5 ADRs are now Accepted** (ADR-0004 was Proposed at the first review).
- `docs/architecture/architecture.md`: still does not exist. `docs/consistency-failures.md`: does not exist.
- Engine reference: only `VERSION.md` applies (the rest describes Unity 6.3). No ADR names a module doc.
- TR registry: 28 IDs, `registry.sh validate` clean (0 duplicate, 0 unknown, 0 path_missing, 0 status, 0 layout).
  The GDD changes since the first review are in `win-condition.md` Implementation Notes and the systems-index
  status column only; **no requirement was added, removed or reworded**, so every ID is reused unchanged and no
  registry change is proposed (no `tr-registry.proposed.yaml`).

---

### Traceability Matrix

| Requirement ID | GDD | Requirement (short) | ADR Coverage | Status | Change vs review 1 |
|---|---|---|---|---|---|
| TR-game-concept-001 | game-concept.md | Windows 64-bit standalone | ADR-0001 (implicit) | ⚠️ Partial | — |
| TR-game-concept-002 | game-concept.md | Two-team online, Photon Cloud, 20 CCU | ADR-0002 | ✅ | — |
| TR-game-concept-003 | game-concept.md | Offline play, same code paths | ADR-0002 | ✅ | — |
| TR-game-concept-004 | game-concept.md | App ID out of git | ADR-0002 | ✅ | — |
| TR-game-concept-005 | game-concept.md | Unity 2017.3 / C# 4 | ADR-0001 | ✅ | — |
| TR-game-concept-006 | game-concept.md | Playtest stability | ADR-0005 | ✅ | — |
| TR-game-concept-007 | game-concept.md | Single authority / RPC trust | ADR-0003 (trust model deferred to M3; `RequestDeploy` adds a new unvalidated request, finding N1) | ⚠️ Partial | — |
| TR-game-concept-008 | game-concept.md | Reliable join / late-join state | none (join fix parked by ADR-0005) | ❌ Gap | — |
| TR-game-concept-009 | game-concept.md | Online splash damage | ADR-0003 (routing only) | ⚠️ Partial | — |
| TR-game-concept-010 | game-concept.md | Team colour at 100 m | none | ❌ Gap | — |
| TR-game-concept-011 | game-concept.md | Frame / network budget | none | ❌ Gap | — |
| TR-win-condition-001 | win-condition.md | Deployed pad takes deployer's team | ADR-0004 + ADR-0003 (room object, team as instantiation data) | ✅ | 🟡 → ✅ |
| TR-win-condition-002 | win-condition.md | Own-team pads only | ADR-0004, ADR-0003 | ✅ | 🟡 → ✅ |
| TR-win-condition-003 | win-condition.md | Spectate with no pad; no fallback | ADR-0004 | ✅ | 🟡 → ✅ |
| TR-win-condition-004 | win-condition.md | Elimination rule | ADR-0004 + ADR-0003 (C1 resolved) | ✅ | ⚠️ → ✅ |
| TR-win-condition-005 | win-condition.md | Win / draw | ADR-0004 | ✅ | 🟡 → ✅ |
| TR-win-condition-006 | win-condition.md | Master-only result | ADR-0003, ADR-0004 | ✅ | — |
| TR-win-condition-007 | win-condition.md | Arming | ADR-0004 | ✅ | 🟡 → ✅ |
| TR-win-condition-008 | win-condition.md | Reset in same room | ADR-0004 (amended reset ordering) | ✅ | 🟡 → ✅ |
| TR-win-condition-009 | win-condition.md | State machine; no damage when Ended | ADR-0004 | ✅ | 🟡 → ✅ |
| TR-win-condition-010 | win-condition.md | Hand-over re-evaluation | ADR-0004 (decided; not yet tested online, see N5) | ✅ | 🟡 → ✅ |
| TR-win-condition-011 | win-condition.md | Result within 2 s | ADR-0004 (6c: 0.3-0.7 s) | ✅ | 🟡 → ✅ |
| TR-win-condition-012 | win-condition.md | Emptied team → Waiting | ADR-0004 (Waiting→Playing stated; Playing→Waiting only implied by "computes Waiting/Playing locally"; code does it, `MatchManager.cs:88-89`) | ⚠️ Partial | — |
| TR-win-condition-013 | win-condition.md | Late joiner state | ADR-0003/0004 Risks only | ⚠️ Partial | — |
| TR-win-condition-014 | win-condition.md | HUD elements | ADR-0004 (OnGUI placeholder; no UI ADR) | ⚠️ Partial | — |
| TR-win-condition-015 | win-condition.md | Tunable knobs | ADR-0004 (runtime AddComponent; tuning path undecided) | ⚠️ Partial | — |
| TR-win-condition-016 | win-condition.md | Last-pad sound | none | ❌ Gap | — |
| TR-win-condition-017 | win-condition.md | Offline + online test coverage | ADR-0005 (+ gate 6c `match`, 6e `win`) | ✅ | — |

### Traceability Summary
Total requirements: 28 (same 28 IDs as review 1; none new, none retired)
✅ Covered (Accepted ADR): **17** (review 1: 7)
🟡 Covered (Proposed ADR only): **0** (review 1: 9)
⚠️ Partial: **7** (review 1: 8)
❌ Gaps: **4** (review 1: 4)

### Coverage Gaps (no ADR exists)
  ❌ TR-game-concept-008: game-concept.md → Networking & Match Join → reliable join, simultaneous and mid-match
     Suggested ADR: "/architecture-decision join-and-late-join-state-sync" · Domain: Networking · Engine Risk: MEDIUM
     Note: Networking & Match Join is a **Core**-layer system. It is not graded as a FAIL-level critical gap only
     because ADR-0005 (Accepted) deliberately parks the join fix behind L1. See QUESTIONS.md Q2.
  ❌ TR-game-concept-011: frame and network budget · Suggested ADR: "performance-and-network-budgets" · Domain:
     Performance · Engine Risk: MEDIUM (also: room objects share PUN's 999 scene view IDs, see N7)
  ❌ TR-game-concept-010: team colour readable at 100 m · Suggested ADR: "team-colour-presentation" · Rendering · LOW
  ❌ TR-win-condition-016: last-pad sound · fold into an audio/UI presentation ADR · Audio · LOW

---

### Conflict C1 re-check (the first review's blocking conflict): RESOLVED

Verified independently against the code and the PUN 1.88 source:

1. **Deploys create room objects on the master.** `GroundPlacementController.Release` calls
   `GameManager.DeployUnit` (`Assets/GroundPlacementController.cs:157-158`), which sends `RequestDeploy` to the
   master only (`Assets/GameManager.cs:444-446`); the master calls
   `PhotonNetwork.InstantiateSceneObject(unitName, …, new object[] { team })` (`Assets/GameManager.cs:449-454`).
   No other gameplay path network-instantiates a deployable unit (grep of `PhotonNetwork.Instantiate(` outside
   PUN/demo/test code: tanks, shells, explosions, dropped cargo, and the placement preview, see N2).
2. **The master owns them.** `InstantiateSceneObject` allocates IDs with `AllocateViewID(0)` (`PhotonNetwork.cs:2389-2413`,
   `2593`), so `ownerId = viewID / MAX_VIEW_IDS = 0` (`PhotonView.cs:171`). `isOwnerActive` is false for owner 0
   (`PhotonView.cs:221`), so `isMine` is true on the master (`PhotonView.cs:241`), and the refusal in
   `RemoveInstantiatedGO` (`NetworkingPeer.cs:3421-3426`) no longer fires for `GameManager.UnitsHealthUpdated`
   (`Assets/GameManager.cs:201-204`). A new master after a hand-over inherits the same right.
3. **They survive their builder leaving.** `DestroyPlayerObjects` removes only views whose `CreatorActorNr` is the
   leaving actor (`NetworkingPeer.cs:3357-3363`); room objects have creator 0. Their Instantiate event is cached
   with `AddToRoomCacheGlobal` (`NetworkingPeer.cs:3149`), not tied to an actor, and is removed on destroy by
   `ServerCleanInstantiateAndDestroy` with `TargetActors = {0}` (`NetworkingPeer.cs:3494`); runtime-instantiated
   objects do not cache a Destroy event (`NetworkingPeer.cs:3501-3507`), so no ghost for late joiners.
4. **Reset removes them.** `Unit.Awake` marks a unit with string instantiation data as `deployed`
   (`Assets/Unit.cs:46-55`; the data is available in `Awake` either way, `PhotonView.cs:101-113`). The sweep
   counts `!view.isSceneView || unit.deployed` objects the client owns (`Assets/MatchManager.cs:177-185`), so the
   master removes deployed room objects, and runs it **before** sending `ResetArena` (`MatchManager.cs:72-79`),
   guarded against a second run (`MatchManager.cs:167-175`). Non-masters skip room objects (`isMine` false).
5. **Gate evidence.** `docs/QA-LOG.md` rows `ro-gate` (C1 phase passed, but ~230 `RayViewer` NREs after reset,
   reproduced, fixed in `1d24750`) and `ro-gate2` (`1d24750`, `--online --stress 3`, VERIFIED, 3/3, 0 exceptions,
   0 errors). The online phase (`Assets/SmokeTest/OfflineSmokeTest.cs:229-345`) has the **non-master** request
   lethal damage on its own deployed pad and asserts the master removed it, then asserts VICTORY/DEFEAT and a
   clean reset (one starting pad per team, zero deployed units).

ADR-0003 (Decision bullet "Deployed units belong to the room", guideline "Shared units … must be created by the
master as room objects") and ADR-0004 (reset ordering, guideline "owned by a client, or a room object the master
created with `Unit.deployed` set") now agree with each other and with the code. The first review's resolution
option 2 was implemented; option 3 (ignore 0-health pads) was rejected by the owner with a sound reason.

### Cross-ADR Conflicts

Known conflict-prone areas: none recorded (`docs/consistency-failures.md` does not exist).

**No blocking conflict remains.** C1: resolved (above). C2 (ADR-0005 local-first vs ADR-0004 online test):
resolved, ADR-0005 Ordering Note and Decision now allow online tests of existing behaviour.

New findings from the C1 fix and its surroundings (none blocking):

**N1 — MEDIUM — `RequestDeploy` trusts the request entirely (TR-game-concept-007).**
`Assets/GameManager.cs:448-454` instantiates whatever prefab name and team string any client sends, with no
check that the sender carries that cargo, that the name is a deployable, that the team is the sender's, or that
the match is `Playing`. Before #36 a cheating client could already `PhotonNetwork.Instantiate` anything, but the
object was its own and vanished when it left; now the master creates a **permanent room object** for it, e.g. a
`RepairPad` for its team anywhere (blocks elimination) or a `RedTank`/`PlayerTank` (a `PlayerMovementManager`
the master would own and `MatchManager.Count` would count). This is within ADR-0003's deferred M3 trust model,
so not a conflict, but it is a new, cheap-to-close surface: the master can check `info.sender`'s
`CargoManager.pickedUpCargo == unitName`, a deployable whitelist, the sender's team, and clear the hold itself.
That also closes N4.

**N2 — MEDIUM — the placement preview still breaks ADR-0003's new guideline (residual C1 class).**
`Assets/GroundPlacementController.cs:85` creates the preview with `PhotonNetwork.Instantiate(baseUnit, …)` on the
deploying client, i.e. a client-owned network copy of the real unit prefab, and `MoveCurrentObjectToMouse` enables
every script on it each frame (`GroundPlacementController.cs:107-108`). ADR-0003 now says shared units are
"never [created] with `PhotonNetwork.Instantiate` on a player's client". If the preview carries a damageable
unit (`HitPointsManager`), the master's destroy at 0 health is refused exactly as in C1
(`GameManager.cs:203` → `NetworkingPeer.cs:3421-3426`, an error log plus one explosion per further hit).
A `RepairPad` preview has team `""`, so it never counts for elimination or respawn; a turret preview from a
prefab with its own team (e.g. `BlueFlakTurret`) is a live, armed turret owned by the client while the preview
shows. Not observed in a gate (no phase damages a preview). Fix: make the preview local-only (plain
`Instantiate` with the PhotonView, `HitPointsManager` and AI removed) or show a non-networked ghost mesh.

**N3 — LOW (latent) — the reset sweep is keyed on `Unit.deployed`, and dropped cargo is master-actor owned.**
`MatchManager.cs:180` sweeps a room object only if it has a `Unit` with `deployed` set. Any future room object
without a `Unit` (or one created with `InstantiateSceneObject` without string instantiation data) survives the
reset sweep: Unity unloads it locally on `LoadLevel`, but its globally cached Instantiate stays in the room and
spawns a ghost for every late joiner. Separately, `GameManager.RequestDropCargo` creates dropped cargo with
`PhotonNetwork.Instantiate` on the master (`GameManager.cs:431`): it is owned by the master's actor, so with
`autoCleanUpPlayerObjects` true (`PhotonNetwork.cs:579`) it vanishes for everyone when the master leaves, the
same "base vanishes with its builder" defect #36 fixed for units, and it is something another player can use
(ADR-0003's guideline wording). Moving cargo to room objects would hit the sweep-key problem above. Suggest
ADR-0004 state the sweep rule as "room objects created at runtime" (e.g. a marker component or a runtime
`isRuntimeInstantiated`-style check) rather than `Unit.deployed`.

**N4 — LOW — deploy requests that cross a reset.** A `RequestDeploy` sent shortly before the client receives
`ResetArena` reaches the master after the master's reload (its queue is paused during `LoadLevel`), so the unit
appears in the new match while the cargo was already spent (`GroundPlacementController.cs:158-161`). A deploy
during `Ended` is accepted and then swept. Closed by N1's server-side checks plus a state check.

**N5 — LOW (documentation) — ADR-0004 contradicts itself after acceptance.** Status says the online two-client
test exists, but `Engine Compatibility` "Verification Required" still says "(not yet written)" (line 33), Risks
still says "The online path is only covered by 'no false result' so far … next story" (line 127), and Validation
Criteria ends "To add: online two-client victory and reset." (line 156). Line 131 asks the online test to cover
a **master hand-over during reset**; the 6c `match` phase does not, so TR-win-condition-010's hand-over path
remains decided but untested online. Key Interfaces still names the types `State`, `Result` where the code and
`docs/registry/architecture.yaml` use the fields `state` / `result` (first-review minor note, still open).

**N6 — LOW (registry/wording) — the new stance is not registered.** ADR-0003's "shared units are master-created
room objects" constrains every future deployable, but `docs/registry/architecture.yaml` has no entry for it
(no forbidden pattern for client-side `PhotonNetwork.Instantiate` of shared units; `api_decisions.networking`
lists only `Instantiate/Destroy`), the forbidden-patterns list in `technical-preferences.md` lacks it, and ADR-0002
Key Interfaces still says "`PhotonNetwork.Instantiate` / `PhotonNetwork.Destroy` for every networked object"
(line 74). Not a contradiction (`InstantiateSceneObject` is a PhotonNetwork API), but `/architecture-decision`
reads the registry to catch conflicts and will not see this one.

**N7 — LOW (capacity) — room objects share PUN's scene view-ID space.** `AllocateViewID(0)` hands out sub-IDs
1-999 shared with the scene's static PhotonViews (`PhotonNetwork.cs:2393-2413`); when exhausted,
`InstantiateSceneObject` throws/logs and the deploy is lost. IDs are freed on destroy and on reset, so this only
matters for very large bases; record it in the budgets ADR (TR-game-concept-011).

Resolved first-review minor notes: `technical-preferences.md` ADR log is current; `Unit.Start` now switches on
`NormalizeTeam` (`Assets/Unit.cs:60`).

### ADR Dependency Order
(from `.claude/scripts/adr-dep-graph.sh`: 5 ADRs, no CYCLE, no NO_DEPS_SECTION, no UNFILLED_DEPS)
ORDER: adr-0001, adr-0002, adr-0005, adr-0003, adr-0004
Foundation: 1. ADR-0001 (engine pin)
Depends on Foundation: 2. ADR-0002 (PUN Classic; requires 0001) · 3. ADR-0005 (gate/branches; requires 0001)
Core: 4. ADR-0003 (master authority, team identity, room-owned deployables; requires 0002)
Feature: 5. ADR-0004 (match flow; requires 0002, 0003)
Unresolved dependencies: none. Every dependency target is Accepted.

---

### GDD Revision Flags
None. The C1 fix made the GDD's Formulas line ("the master removes a pad at 0 health") true online; the owner's
decision at review 1 (do not flag `win-condition.md`) is borne out. No systems-index change proposed.

### Engine Compatibility Issues

### Engine Audit Results
Engine: Unity 2017.3.0f3
ADRs with Engine Compatibility section: 5 / 5
Version consistency: all 5 name 2017.3.0f3. No stale versions.
Post-Cutoff APIs: none claimed; none needed.
Deprecated API References: **NOT ASSESSED** — `deprecated-apis.md` describes Unity 6.3, not 2017.3 (VERSION.md);
  no deprecated-API reference exists for the pinned version. Guard remains ADR-0001 + CI compile.
Module docs consulted: none (no ADR names one; all describe Unity 6.3).

### Engine Specialist Findings
No `unity-specialist` agent type is available in this session, so the consultation was done inline against the
vendored PUN 1.88 source:
- **Confirmed** ADR-0003's new PUN claims: only the owner (or master for owner-0 / owner-left views) may
  network-destroy (`PhotonView.cs:221,241`, `NetworkingPeer.cs:3421`); `autoCleanUpPlayerObjects` defaults true
  (`PhotonNetwork.cs:579`) and removes a leaver's created objects (`NetworkingPeer.cs:3357-3368`).
- **Confirmed** ADR-0004's amended reset reasoning: `LoadLevel` pauses the queue; `SendOutgoingCommands` before it
  flushes the destroys and `ResetArena` (`MatchManager.cs:159-163`).
- `instantiationData` is readable in `Unit.Awake` whatever the component order (`PhotonView.cs:101-113`,
  `NetworkingPeer.cs:3280-3297`).
- New engine-level notes: N3 (cached global Instantiate of an unswept room object ghosts late joiners), N7 (shared
  scene view-ID space). No post-2017.3 API or C# 6+ use found in the changed code.

### Architecture Document Coverage
**Architecture document coverage: NOT ASSESSED — no docs/architecture/architecture.md.** Unchanged since review 1.
Systems no ADR mentions: Hover Movement (#1), Camera (#2), Fuel (#7), weapons (#8-10), Targeting (#11), Cargo
(#14), Turrets (#16), HUD (#20), Audio (#22), Team & Vehicle Selection (#19). Deploy (#15) is now partly covered
by ADR-0003's room-object rule. No orphaned architecture.

---

### Verdict: CONCERNS

- C1 is resolved, with code, PUN-source and online-gate evidence; C2 is resolved. No blocking cross-ADR conflict.
- Every ADR is Accepted, so no requirement rests on a Proposed ADR. Coverage rose from 7 to 17 of 28.
- CONCERNS remain: 7 partials and 4 gaps (one, TR-game-concept-008, in a Core-layer system, deliberately deferred
  by ADR-0005); new MEDIUM findings N1 and N2; Phase 6 and the deprecated-API audit are **NOT ASSESSED** line items,
  which alone keep this full-mode review from PASS.

### Blocking Issues
None.

### Required ADRs (most foundational first)
1. Join and late-join state sync (TR-game-concept-008, TR-win-condition-013). Networking, Core.
2. Performance and network budgets (TR-game-concept-011; include N7). Cross-cutting.
3. RPC trust model (TR-game-concept-007; include `RequestDeploy`, N1, and `SetHealthFromClient`).
4. HUD / UI and presentation: UGUI vs OnGUI, team colour, last-pad cue (TR-win-condition-014/016, TR-game-concept-010).
5. Small amendments, not new ADRs: ADR-0004 stale lines and sweep rule (N3, N5); ADR-0002 Key Interfaces and the
   architecture registry entry (N6); state Playing→Waiting explicitly in ADR-0004 (TR-win-condition-012).

---

### Handoff (Phase 9)
Immediate actions: (1) fix N2 and N1 (both small, on the deploy path just changed); (2) `/create-architecture` so
Phase 6 can run; (3) `/architecture-decision join-and-late-join-state-sync`.
Pre-gate checklist:
- ❌ `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/` — `/test-setup` (or record that `Tools/verify-local.sh` + in-player smoke tests are the test root; QUESTIONS.md Q5)
- ❌ `.github/workflows/tests.yml` — `/test-setup` (only `checks.yml` exists)
- ❌ `design/accessibility-requirements.md` — `/ux-design`
- ❌ `design/ux/interaction-patterns.md` — `/ux-design`
`/gate-check` should not be offered until these are resolved.
Rerun trigger: re-run `/architecture-review` after each new or amended ADR.

### Proposed session-state extract (for production/session-state/active.md, not written)
    ## Session Extract — /architecture-review 2026-10-26 (2)
    - Verdict: CONCERNS
    - Requirements: 28 total — 17 covered, 7 partial, 4 gaps
    - New TR-IDs registered: None
    - GDD revision flags: None
    - Top ADR gaps: join/late-join sync; performance & network budgets; RPC trust model
    - Report: docs/architecture/architecture-review-2026-10-26-2.md

---

### Owner decisions (2026-10-26, relayed by the authoring session)

| # | Question | Decision |
|---|----------|----------|
| Q1 | What to write | This report and the updated traceability index (no registry change was needed) |
| Q2 | Reliable-join gap as a FAIL? | No: ADR-0005 defers it on purpose; one line added to ADR-0005 saying so |
| Q3/Q4 | N2 and N1 | Fixed now, together with N3 and N4: PR #38 (`dh-gate` VERIFIED, online 3/3, 0 errors) |
| Q5 | Test setup | `Tools/verify-local.sh` plus the in-player smoke tests recorded as the project's test root (`technical-preferences.md`, Testing) |
| Q6 | Small doc fixes | Done in this docs PR: ADR-0004 stale lines and Playing→Waiting (N5, TR-win-condition-012); ADR-0002/0003 and the registry carry room-owned deploys and the deploy-request contract (N6) |
| Q7 | Next step | `/create-control-manifest`, then `/sprint-plan`; `/create-architecture` and the join/late-join ADR go into the sprint backlog |

The Reviewed-Content-Hash receipts above were taken before #38 and these doc fixes; the next review re-stamps them.
N7 (shared scene view IDs) and a master hand-over during a reset remain open, as recorded in ADR-0004's Risks.
