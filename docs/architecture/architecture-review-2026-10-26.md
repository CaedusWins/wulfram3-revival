## Architecture Review Report
Date: 2026-10-26
Mode: full
Workflow: standard
Engine: Unity 2017.3.0f3 (C# 4, PUN Classic 1.88)
GDDs Reviewed: 3 (game-concept.md, systems-index.md, win-condition.md)
ADRs Reviewed: 5 (ADR-0001 to ADR-0005)
Reviewer: independent (did not write any ADR); review_mode lean, rigor standard, automation guided

<!-- RECEIPT: the hashes below were taken on 2026-10-26 BEFORE any write. If the owner applies
     tr-registry.proposed.yaml, re-run the Phase 8 hash command after that write and replace the
     tr-registry.yaml line (the procedure takes the receipt last). -->
Reviewed-Content-Hash: docs/architecture/adr-0001-engine-pin-unity-2017-3.md 31778ecdc8fed61193cf3dc10cc643d1dc453bc2
Reviewed-Content-Hash: docs/architecture/adr-0002-pun-classic-photon-cloud.md adc6ceacc7759b70cf2c3a06009869c0d475335e
Reviewed-Content-Hash: docs/architecture/adr-0003-master-authority-and-team-identity.md 998066222e42197ba01c7461790b80623533b246
Reviewed-Content-Hash: docs/architecture/adr-0004-match-flow-matchmanager.md 58192928b715191599bf773c7d0d9da310527a76
Reviewed-Content-Hash: docs/architecture/adr-0005-verification-gate-and-branch-model.md b2ce79737fff9e534e910c7c9137315e4c993dd1
Reviewed-Content-Hash: design/gdd/game-concept.md 3a6c4c45e3b8f4d99630b53a2b0e88d25fe670a3
Reviewed-Content-Hash: design/gdd/systems-index.md a3cc68399445e0c0d27ad49c52fbf2586b07f6e8
Reviewed-Content-Hash: design/gdd/win-condition.md cea1cbdf89435ecd68422049748bf9c27963a765
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
Reviewed-Content-Hash: .claude/docs/technical-preferences.md 95a8117e5a71ec4f169ef3bdd1ca377d7e354e59
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

- Freshness check: `RECEIPT: NONE`. No prior review report exists, so this is the first full review.
- Loaded 3 GDDs and 5 ADRs. Engine: Unity 2017.3.0f3. All 5 ADRs carry `## Status`, `## Engine Compatibility`,
  `## ADR Dependencies` and `## GDD Requirements Addressed`, so there are no structural gaps in the ADRs.
- `win-condition.md` is the only system GDD. `game-concept.md` is a concept doc, and `systems-index.md` is an index.
  **18 of the 26 systems are marked "Implemented" with no GDD** (rows 1-11, 13-17, 20-22), so they have
  no requirements to trace. The coverage below covers only what has been designed, which is a small part of the game.
- `docs/architecture/architecture.md`: does not exist. `docs/consistency-failures.md`: does not exist.
- Engine reference: only `VERSION.md` applies. `breaking-changes.md`, `deprecated-apis.md` and `modules/` describe
  Unity 6.3 and **do not apply** (VERSION.md says so). No ADR names a module doc.
- Requirement IDs: `tr-registry.yaml` was empty and `architecture.md` has no baseline, so all IDs are new.
  `systems-index.md` is used as the system list and is not a requirement source.

---

### Traceability Matrix

| Requirement ID | GDD | Requirement (short) | ADR Coverage | Status |
|---|---|---|---|---|
| TR-game-concept-001 | game-concept.md | Windows 64-bit standalone | ADR-0001 (implicit: no platform decision stated; build target only in project.yaml) | ⚠️ Partial |
| TR-game-concept-002 | game-concept.md | Two-team online on Photon Cloud, 20 CCU | ADR-0002 | ✅ |
| TR-game-concept-003 | game-concept.md | Offline play on the same code paths | ADR-0002 | ✅ |
| TR-game-concept-004 | game-concept.md | App ID out of git | ADR-0002 | ✅ |
| TR-game-concept-005 | game-concept.md | Stay on Unity 2017.3 / C# 4 | ADR-0001 | ✅ |
| TR-game-concept-006 | game-concept.md | Stable enough for a closed playtest | ADR-0005 | ✅ |
| TR-game-concept-007 | game-concept.md | Single authority / RPC trust model | ADR-0003 (master authority; trust model deferred to M3; `SetHealthFromClient` open) | ⚠️ Partial |
| TR-game-concept-008 | game-concept.md | Reliable join, including simultaneous and mid-match | none (join race "parked"; late-join health is listed only as a Risk in ADR-0003/0004) | ❌ Gap |
| TR-game-concept-009 | game-concept.md | Splash damage applied correctly online | ADR-0003 routes damage, but not the splash path, which is a known online bug | ⚠️ Partial |
| TR-game-concept-010 | game-concept.md | Team colour readable at 100 m (built-in RP) | none | ❌ Gap |
| TR-game-concept-011 | game-concept.md | Frame and network budget for about 10v10 | none (technical-preferences budgets `[TO BE CONFIGURED]`; ADR-0002 "not measured yet") | ❌ Gap |
| TR-win-condition-001 | win-condition.md | Pads have a team; deployed pad takes deployer's team | ADR-0004 | 🟡 Proposed |
| TR-win-condition-002 | win-condition.md | Respawn only at own-team pads | ADR-0004 (+ ADR-0003 SameTeam) | 🟡 Proposed |
| TR-win-condition-003 | win-condition.md | No pad: spectate; respawn within 2 s of a new pad; no centre fallback | ADR-0004 | 🟡 Proposed |
| TR-win-condition-004 | win-condition.md | Elimination = 0 pads and 0 living tanks | ADR-0004, **broken online by Conflict C1** | ⚠️ Partial |
| TR-win-condition-005 | win-condition.md | Win / draw evaluation | ADR-0004 | 🟡 Proposed |
| TR-win-condition-006 | win-condition.md | Master-only evaluation, RPC to all | ADR-0003 (Accepted, lists it explicitly) + ADR-0004 | ✅ |
| TR-win-condition-007 | win-condition.md | Arming rule; offline can arm | ADR-0004 | 🟡 Proposed |
| TR-win-condition-008 | win-condition.md | Reset in the same room | ADR-0004 (as-built difference: spawn point, not pad; documented) | 🟡 Proposed |
| TR-win-condition-009 | win-condition.md | 4-state machine; no damage while Ended | ADR-0004 | 🟡 Proposed |
| TR-win-condition-010 | win-condition.md | Hand-over re-evaluates from the scene | ADR-0004 | 🟡 Proposed |
| TR-win-condition-011 | win-condition.md | Result on all clients within 2 s | ADR-0004 (0.5 s checks) | 🟡 Proposed |
| TR-win-condition-012 | win-condition.md | Team emptied → Waiting, no forfeit | ADR-0004 decides Waiting→Playing only; Playing→Waiting is not stated | ⚠️ Partial |
| TR-win-condition-013 | win-condition.md | Late joiner sees correct state | ADR-0003/0004 list it as a Risk and defer it; no decision | ⚠️ Partial |
| TR-win-condition-014 | win-condition.md | HUD counts, overlay, banner, countdown | ADR-0004 calls the OnGUI HUD a placeholder; no UI architecture ADR (UGUI vs OnGUI) | ⚠️ Partial |
| TR-win-condition-015 | win-condition.md | Tuning knobs within ranges | ADR-0004 adds MatchManager at runtime (no inspector); how the knobs are tuned is undecided | ⚠️ Partial |
| TR-win-condition-016 | win-condition.md | Last-pad sound cue | none (no audio ADR; no asset) | ❌ Gap |
| TR-win-condition-017 | win-condition.md | Offline + online two-client test coverage | ADR-0005 (Accepted: each new behaviour adds a gate phase) | ✅ |

### Traceability Summary
Total requirements: 28
✅ Covered (Accepted ADR): 7
🟡 Covered (Proposed ADR-0004 only): 9
⚠️ Partial: 8
❌ Gaps: 4

(ADR-0004 is the sole coverage for 9 of the 17 win-condition requirements, and it is `Proposed`.
The route out is `/architecture-decision accept ADR-0004`, after its online two-client test and after Conflict C1 is fixed.)

### Coverage Gaps (no ADR exists)
  ❌ TR-game-concept-008: game-concept.md → Networking & Match Join → reliable join (simultaneous, mid-match, late-join state)
     Suggested ADR: "/architecture-decision join-and-late-join-state-sync" (buffered/initial health, cached scene-destroy handling, join race)
     Domain: Networking · Engine Risk: MEDIUM (PUN Classic buffering/caching behaviour)
  ❌ TR-game-concept-011: game-concept.md → (cross-cutting) → frame and network budget for a 20-player match
     Suggested ADR: "/architecture-decision performance-and-network-budgets"
     Domain: Performance · Engine Risk: MEDIUM (Photon Cloud message-rate limits are not measured)
  ❌ TR-game-concept-010: game-concept.md → Rendering/Teams → team colour readable at 100 m
     Suggested ADR: "/architecture-decision team-colour-presentation" (data-driven from `Unit.team`, built-in RP)
     Domain: Rendering · Engine Risk: LOW
  ❌ TR-win-condition-016: win-condition.md → Audio → last-pad cue
     Suggested ADR: none of its own; fold into an audio/UI presentation ADR. Domain: Audio · Engine Risk: LOW

---

### Cross-ADR Conflicts

Known conflict-prone areas: none recorded (`docs/consistency-failures.md` does not exist).

## Conflict C1: ADR-0003 vs ADR-0004 (and ADR-0002's destroy rule) — 🔴 HIGH, blocking for the online win condition
Type: State / Integration (who may network-destroy a client-deployed unit)
ADR-0003 claims: "The master destroys non-player units at 0 health (`GameManager.UnitsHealthUpdated`)"
  and "Only the master may destroy shared units".
ADR-0004 claims (Constraints): "PUN only lets the owner (or the master, for scene objects and objects of
  players who left) network-destroy an object". Its elimination count relies on "own-team RepairPads that
  exist (the master destroys a pad at 0 health)". Its reset has **each client** destroy the objects it owns.
Evidence (code, read-only):
  - `Assets/GroundPlacementController.cs:160`: a deployed unit is created with `PhotonNetwork.Instantiate`
    by the deploying client, so that client owns it. No `TransferOwnership` call exists anywhere outside the PUN demos.
  - `Assets/GameManager.cs:201-204`: the master calls `PhotonNetwork.Destroy` on any non-player unit at 0 health.
  - `Assets/Photon Unity Networking/Plugins/PhotonNetwork/NetworkingPeer.cs:3421-3426`: `RemoveInstantiatedGO`
    refuses when `!viewZero.isMine` ("Client is neither owner nor masterClient taking over for owner who left").
    `PhotonView.cs:241`: `isMine` = owner == local player, or (owner inactive and master).
  - `Assets/RepairPad.cs:48-59`: `Count(team)` and `FindNearestOwn` count every `RepairPad` object, with no health check.
Impact: online, when a **non-master** client deployed the pad, a destroyed pad (or turret) is never removed. It
  stays in the scene at 0 health on every client and still counts in `RepairPad.Count`, so that team **can never be
  eliminated and the match never ends**. Dead players can also still respawn at it through `FindNearestOwn`. The
  master also spawns an explosion on every further hit. Offline play and the master's own deploys are not affected,
  which is why gate 6e passes. The ADR-0003/0004 "engine specialist review: no blocking findings" notes missed this.
Resolution options:
  1. The master decides and the owner executes: the master sends a `DestroyUnit` RPC to the owner (or to All, and
     the `isMine` holder destroys it). Amend ADR-0003's wording to "master decides, owner network-destroys".
  2. Make deployables scene-owned: deploy through `PhotonNetwork.InstantiateSceneObject` from the master (an RPC
     request to the master), so the master owns them. Note that ADR-0004's reset rule ("anything created during a
     match must be a networked object owned by a client") must then change to master-destroys-scene-instantiated.
  3. Defensive in addition to 1 or 2: `RepairPad.Count` and `FindNearestOwn` ignore pads whose health is 0 or below.

## Conflict C2: ADR-0005 (local first) vs ADR-0004 (acceptance needs an online test) — 🟡 LOW, ordering only
Type: Dependency / ordering
ADR-0005: online feature work waits for L1, and online gate steps stay as a regression canary.
ADR-0004: stays Proposed until an online two-client match-end-and-reset test exists.
Impact: the win condition's acceptance (and every requirement marked 🟡) is held behind work that ADR-0005 defers.
  It is not a contradiction, but no ADR says whether the online match-end test counts as "online feature work".
Resolution options: 1. Treat the online match-end test as an allowed regression canary under ADR-0005 (one line in ADR-0005).
  2. Accept ADR-0004 on offline evidence and track the online test as a story.

### Minor consistency notes (not conflicts)
- `docs/registry/architecture.yaml` `match_state.interface` names `MatchManager.Instance.state / result`, while
  ADR-0004 Key Interfaces lists `State`, `Result` (the enum types). The code has the fields `state`/`result`
  (`Assets/MatchManager.cs:31-32`). ADR-0004 should name the fields.
- `.claude/docs/technical-preferences.md` "Architecture Decisions Log" still says "No ADRs yet". It is stale.
- ADR-0003's open Migration-Plan gap (`Unit.Start` switches on `team.ToLower()`, `Assets/Unit.cs:51`) is
  itself a `raw_team_string_comparison` forbidden-pattern violation in the codebase. It is correctly recorded, but still open.

### ADR Dependency Order
(from `.claude/scripts/adr-dep-graph.sh`: 5 ADRs, no CYCLE, no NO_DEPS_SECTION, no UNFILLED_DEPS)
Foundation (no dependencies):
  1. ADR-0001: Engine pin, Unity 2017.3 / C# 4
Depends on Foundation:
  2. ADR-0002: PUN Classic on Photon Cloud (requires ADR-0001)
  3. ADR-0005: Verification gate and branch model (requires ADR-0001)
Core:
  4. ADR-0003: Master authority and team identity (requires ADR-0002)
Feature:
  5. ADR-0004: Match flow / MatchManager (requires ADR-0002, ADR-0003). **Proposed**. Nothing depends on it yet;
     ADR-0004 "Enables" Team & Vehicle Selection (#19), which has no ADR.
Unresolved dependencies: none. Every dependency target is Accepted.

---

### GDD Revision Flags

| GDD | Assumption | Reality (from ADR/engine source) | Action |
|-----|-----------|--------------------------------------|--------|
| win-condition.md (Formulas) | "padsAlive(T) … (the master removes a pad at 0 health)" | PUN Classic refuses a master network-destroy of a pad owned by an active non-master client (Conflict C1) | Fix the architecture or code (C1, option 1 or 2) **rather than** the design. If the GDD is touched, it only needs to define padsAlive as pads with health > 0. Recommended: do not mark the GDD `Needs Revision`; see QUESTIONS.md Q2 |

Proposed systems-index change (only if the owner chooses to flag the GDD):
- current: `| 18 | Win Condition (MatchManager) | Gameplay | MVP | Implemented (offline-proven, gate 6e win; online two-client check to come) | [win-condition.md](win-condition.md) | … |`
- proposed: `| 18 | Win Condition (MatchManager) | Gameplay | MVP | Needs Revision | [win-condition.md](win-condition.md) | … |`

### Engine Compatibility Issues

### Engine Audit Results
Engine: Unity 2017.3.0f3
ADRs with Engine Compatibility section: 5 / 5
Version consistency: all 5 name 2017.3.0f3. No stale versions.
Post-Cutoff APIs: none claimed by any ADR. This is correct for a 2017 engine.
Deprecated API References: **check not applicable.** `docs/engine-reference/unity/deprecated-apis.md` describes
  Unity 6.3, not 2017.3 (VERSION.md). No deprecated-API reference exists for the pinned version, so this audit leg
  is effectively NOT ASSESSED. The real guard is ADR-0001's "not available" list plus CI/compile.
Module docs consulted: none (no ADR names one, and all module docs are for Unity 6.3).

### Engine Specialist Findings
The `unity-specialist` agent type is not available in this session, so the consultation was done inline by this
reviewer against the vendored PUN 1.88 source and the game code:
- **Confirmed** ADR-0004's PUN claims: `PhotonNetwork.LoadLevel` pauses the message queue (`PhotonNetwork.cs:3271`).
  `SetLevelInPropsIfSynced` returns early for the same scene name (`NetworkingPeer.cs:4612-4621`). The 2017.3
  `FindObjectsOfType` has no include-inactive overload.
- **New, HIGH:** Conflict C1 (master network-destroy of client-owned deployables fails). The earlier specialist sign-offs
  in ADR-0003 and ADR-0004 should be treated as incomplete.
- No post-2017.3 API or C# 6+ assumption was found in any ADR.

### Architecture Document Coverage
**Architecture document coverage: NOT ASSESSED — no docs/architecture/architecture.md.** Workflow `standard`
expects one ("architecture doc + critical ADRs"). Systems in `systems-index.md` that no ADR mentions: Hover Movement (#1),
Camera (#2), Fuel (#7), weapons (#8-10), Targeting (#11), Cargo (#14), Turrets (#16), HUD (#20), Audio (#22),
Team & Vehicle Selection (#19). No orphaned architecture was found.

---

### Verdict: FAIL

- One blocking cross-ADR conflict (C1). As written, ADR-0003 and ADR-0004 together cannot end an online match
  when a pad deployed by a non-master client is destroyed. The MVP's one designed system fails online. The evidence
  is from code and PUN source and has not been reproduced in a run, but the PUN refusal path is unconditional.
- If C1 is fixed, the verdict falls to **CONCERNS**: 9 requirements rest only on the Proposed ADR-0004, there are 8
  partials and 4 gaps, and Phase 6 and the deprecated-API audit are NOT ASSESSED.

### Blocking Issues (must resolve before PASS)
1. Resolve C1. Amend ADR-0003 (who executes a destroy) and ADR-0004 (pad-count rule, reset ownership rule), fix the
   code, and add an online gate phase where a non-master deploys a pad that is then destroyed.
2. Accept ADR-0004 (after 1 and its online two-client test), or settle C2.
3. Create `docs/architecture/architecture.md` (`/create-architecture`), so Phase 6 can be assessed.

### Required ADRs (most foundational first)
1. Join and late-join state sync (TR-game-concept-008, TR-win-condition-013). Networking, Foundation/Core.
2. Performance and network budgets (TR-game-concept-011). Cross-cutting. Also fills `performance_budgets` in the registry.
3. RPC trust model (TR-game-concept-007), which ADR-0003 already names as the M3 refinement.
4. HUD / UI architecture: UGUI vs OnGUI, team-colour presentation (TR-win-condition-014, TR-game-concept-010).
5. Later: GDDs for the 18 implemented-but-undesigned systems, then their ADRs (Teams/Health/Deploy first).

---

### Handoff (Phase 9)
Immediate actions: (1) fix C1 and amend ADR-0003/0004; (2) `/create-architecture`; (3) `/architecture-decision` the join/late-join ADR.
Pre-gate checklist:
- ❌ `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`: run `/test-setup`. The project's real test path is `Tools/verify-local.sh`
  plus the in-player smoke tests. Decide whether this item applies (QUESTIONS.md Q6).
- ❌ `.github/workflows/tests.yml`: `/test-setup` (only `checks.yml` exists)
- ❌ `design/accessibility-requirements.md`: `/ux-design`
- ❌ `design/ux/interaction-patterns.md`: `/ux-design`
`/gate-check` should not be offered until these are resolved.
Rerun trigger: re-run `/architecture-review` after each new or amended ADR.

### Proposed session-state extract (for production/session-state/active.md, not written)
    ## Session Extract — /architecture-review 2026-10-26
    - Verdict: FAIL (CONCERNS once C1 is resolved)
    - Requirements: 28 total — 7 covered, 9 covered-proposed, 8 partial, 4 gaps
    - New TR-IDs registered: 28 (pending owner approval)
    - GDD revision flags: win-condition.md (recommended: fix code/ADR, not GDD)
    - Top ADR gaps: join/late-join sync; performance & network budgets; RPC trust model
    - Report: docs/architecture/architecture-review-2026-10-26.md

---

### Owner decisions (2026-10-26, relayed by the authoring session)

The review was run by an independent agent; the questions in its QUESTIONS.md were put to the owner:

| # | Question | Decision |
|---|----------|----------|
| Q1 | What to write | All three: this report, `requirements-traceability.md`, `tr-registry.yaml` (28 TR-IDs) |
| Q2 | Flag `win-condition.md` Needs Revision | No: the design stands; the code and ADRs change |
| Q3 | Resolve C1 | **Room-owned deployables**: a deploy asks the master, which creates the unit with `PhotonNetwork.InstantiateSceneObject` (team as instantiation data), so the master can destroy it. ADR-0003 and ADR-0004 get amended. The "ignore pads at 0 health" guard is **not** added: late joiners read remote health as 0 (ADR-0003 Risks), so the guard would stop them respawning at healthy pads |
| Q4 | TR-ID slugs | GDD file stem (`TR-win-condition-NNN`) |
| Q5 | game-concept.md as a source | Yes |
| Q6 | Next step | Fix C1 with an online gate phase, then re-run this review |
| Q7 | C2 | The online match-end test is an allowed regression test under ADR-0005 (line added there) |

**Correction to C1's analysis (found while relaying):** the report assumes "if the owner leaves the room,
the master inherits the objects". In this project `PhotonNetwork.autoCleanUpPlayerObjects` is left at its
default, `true` (`PhotonNetwork.cs:579`), so PUN **destroys** a leaving player's objects for everyone: a
team's deployed pads and turrets vanish when their builder disconnects. Room-owned deployables (Q3) fix
this as well, which is why that option was chosen over "owner destroys on request".
