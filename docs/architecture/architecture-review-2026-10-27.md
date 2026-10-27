## Architecture Review Report
Date: 2026-10-27
Mode: full
Workflow: standard
Engine: Unity 2017.3.0f3 (C# 4, PUN Classic 1.88)
GDDs Reviewed: 6 (game-concept.md, systems-index.md, win-condition.md, self-destruct.md, turrets.md, cargo.md)
ADRs Reviewed: 5 (ADR-0001 to ADR-0005, all Accepted)
Reviewer: independent (wrote no ADR or GDD); review_mode lean, automation guided, docs.density balanced
Code basis: working tree at `2df3a35` (branch `docs/playtest-sheet`; the only commit after `a426a68` adds a playtest sheet, no code)

<!-- RECEIPT: taken 2026-10-27 over the repository inputs as reviewed. Applied by the owner the same day
     (registry, index, the CHANGES.md section B/C text fixes; TR-win-condition-012 raised to covered; TR-cargo-010 added for the Q2 answer): the
     tr-registry.yaml line below was re-taken after the registry was written ("the receipt is taken last"). -->
Reviewed-Content-Hash: docs/architecture/adr-0001-engine-pin-unity-2017-3.md 31778ecdc8fed61193cf3dc10cc643d1dc453bc2
Reviewed-Content-Hash: docs/architecture/adr-0002-pun-classic-photon-cloud.md 8bfcea7af15cc7768526c8adb31cbb9079a52910
Reviewed-Content-Hash: docs/architecture/adr-0003-master-authority-and-team-identity.md 498b3df3327d1300602e061f8412263183c462ae
Reviewed-Content-Hash: docs/architecture/adr-0004-match-flow-matchmanager.md 55d2a54f4969786d90348579a28d7799dcd3247a
Reviewed-Content-Hash: docs/architecture/adr-0005-verification-gate-and-branch-model.md 97e635cf4623def7ee81dbcc279d0b303d56ada1
Reviewed-Content-Hash: design/gdd/cargo.md 42042dd08b69e66668a070a4ce9bf381c29e3772
Reviewed-Content-Hash: design/gdd/game-concept.md 3a6c4c45e3b8f4d99630b53a2b0e88d25fe670a3
Reviewed-Content-Hash: design/gdd/self-destruct.md 1536dc115c357d35baa77b1e3b0a7062204b155e
Reviewed-Content-Hash: design/gdd/systems-index.md b29ae0c521892fdb779327cfb7c60bfda46e6292
Reviewed-Content-Hash: design/gdd/turrets.md 6d8d357e84f6dc76ace8da0753b98459e18b6dfb
Reviewed-Content-Hash: design/gdd/win-condition.md 3519e3d49717ab7e49b77b696af9f71612715f17
Reviewed-Content-Hash: docs/architecture/tr-registry.yaml 9120ff0fc3555b511be0c1697fa830508aefa500
Reviewed-Content-Hash: docs/architecture/architecture.md ea433ba2b39bdcb58a9057117d9e9a97dbe2ad7d
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
Reviewed-Content-Hash: .claude/docs/technical-preferences.md c096ed9ffcb68c9e11b28edec5966eed643085ba
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
(not stamped, no such file: docs/consistency-failures.md)

---

### Load notes (Phase 1)

- **Freshness check** against `architecture-review-2026-10-26-2.md` (Mode full, Workflow standard, so it covers
  this run): no `UNRESOLVED`, no `GONE`. `NEW`: `cargo.md`, `self-destruct.md`, `turrets.md`, `architecture.md`.
  `CHANGED`: ADR-0002..0005 (the review-2 doc fixes N5/N6, which landed after its receipts), `systems-index.md`,
  `technical-preferences.md`. A new `architecture.md` and a changed `technical-preferences.md` reach every phase,
  so the procedure calls for a full re-run, not a `single-gdd` scope. This is that run.
- Denominators: **N_gdd = 6**, **N_adr = 5**. All 6 GDDs carry `## Summary` (systems-index has an Overview
  instead, read whole as an index). All 5 ADRs carry `## Status`, `## Engine Compatibility`, `## ADR Dependencies`,
  `## GDD Requirements Addressed`. No structural gaps.
- The three new GDDs are short (110-122 lines), so they were read whole; the code they name was read and checked.
- **No ADR's `GDD Requirements Addressed` names self-destruct.md, turrets.md or cargo.md** (all ADRs predate them).
  Coverage below is judged from each ADR's Decision text, and counted only where an Accepted ADR really decides
  the requirement's architectural part.
- `docs/consistency-failures.md`: does not exist. Engine reference: only `VERSION.md` applies to 2017.3; the other
  files describe Unity 6.3 (`VERSION.md:20`). No ADR names a module doc.
- Engine: Unity 2017.3.0f3 (`project.yaml` `engine.version`). Loaded 6 GDDs, 5 ADRs.

### Requirement extraction (Phase 2)

Pre-loaded registry: 28 entries (TR-game-concept-001..011, TR-win-condition-001..017), all active, none
deprecated. Next free IDs: TR-self-destruct-001, TR-turrets-001, TR-cargo-001 (no prior entries, no provisional
baseline rows: `architecture.md` has no Technical Requirements Baseline section).

**Re-check of the 28 existing entries against current GDD text:** `game-concept.md` and `win-condition.md` are
byte-identical to review 2 (same hashes). Every registered requirement still states its GDD's intent, so **no
`revised` edit and no deprecation is proposed**. Code re-checks:
- TR-game-concept-009 (online splash): still true and confirmed in code, `ExplosionPhysicsForce.cs:29,48-53`.
- TR-game-concept-007 (trust): `RequestDeploy` is now validated (`GameManager.cs:456-471`, PR #38), but
  `RequestPickUpCargo`/`RequestDropCargo` take no `PhotonMessageInfo` and never check the sender
  (`GameManager.cs:368-386, 419-436`), `TakeDamage` accepts any amount from any client and
  `SetHealthFromClient` is still unvalidated (`HitPointsManager.cs:15-22, 44-49`). Status stays ⚠️.
- TR-win-condition-001: holds for pads because the `RepairPad` prefab has no team; `Unit.Awake` gives the
  deployer's team **only to team-less prefabs** (`Unit.cs:51-53`). See finding F4 for turrets.
- Stale *text* (not requirement intent) in the unchanged GDDs is listed under GDD accuracy findings below.

New requirements (25): see the matrix. Pure balance numbers were folded into the requirement whose architectural
part they belong to; each GDD's tuning table and test coverage got one entry each, following the
TR-win-condition-015/017 precedent.

---

### Traceability Matrix

| Requirement ID | GDD | System | Requirement (short) | ADR Coverage | Status |
|---|---|---|---|---|---|
| TR-game-concept-001 | game-concept.md | Platform | Windows 64-bit standalone | ADR-0001 (implicit) | ⚠️ Partial |
| TR-game-concept-002 | game-concept.md | Networking | Two-team online, Photon Cloud, 20 CCU | ADR-0002 | ✅ |
| TR-game-concept-003 | game-concept.md | Offline Play | Offline play, same code paths | ADR-0002 | ✅ |
| TR-game-concept-004 | game-concept.md | Networking | App ID out of git | ADR-0002 | ✅ |
| TR-game-concept-005 | game-concept.md | Platform | Unity 2017.3 / C# 4 | ADR-0001 | ✅ |
| TR-game-concept-006 | game-concept.md | All | Playtest stability | ADR-0005 | ✅ |
| TR-game-concept-007 | game-concept.md | Networking | Single authority / RPC trust | ADR-0003 (deploy validated; pick-up, drop, damage amount, `SetHealthFromClient` not) | ⚠️ Partial |
| TR-game-concept-008 | game-concept.md | Networking | Reliable join / late-join state | — (deferred by ADR-0005) | ❌ Gap |
| TR-game-concept-009 | game-concept.md | Explosions | Online splash damage | ADR-0003 (master applies; victim selection undecided) | ⚠️ Partial |
| TR-game-concept-010 | game-concept.md | Presentation | Team colour at 100 m | — | ❌ Gap |
| TR-game-concept-011 | game-concept.md | All | Frame / network budget | — | ❌ Gap |
| TR-win-condition-001 | win-condition.md | Repair Pads | Pad team from deployer | ADR-0004, ADR-0003 | ✅ |
| TR-win-condition-002 | win-condition.md | Death & Respawn | Own-team pads only | ADR-0004, ADR-0003 | ✅ |
| TR-win-condition-003 | win-condition.md | Death & Respawn | Spectate with no pad; no fallback | ADR-0004 | ✅ |
| TR-win-condition-004 | win-condition.md | Win Condition | Elimination rule | ADR-0004, ADR-0003 | ✅ |
| TR-win-condition-005 | win-condition.md | Win Condition | Win / draw | ADR-0004 | ✅ |
| TR-win-condition-006 | win-condition.md | Win Condition | Master-only result | ADR-0003, ADR-0004 | ✅ |
| TR-win-condition-007 | win-condition.md | Win Condition | Arming | ADR-0004 | ✅ |
| TR-win-condition-008 | win-condition.md | Win Condition | Reset in same room | ADR-0004 | ✅ |
| TR-win-condition-009 | win-condition.md | Win Condition | State machine; no damage when Ended | ADR-0004 | ✅ |
| TR-win-condition-010 | win-condition.md | Win Condition | Hand-over re-evaluation | ADR-0004 (untested online) | ✅ |
| TR-win-condition-011 | win-condition.md | Win Condition | Result within 2 s | ADR-0004 | ✅ |
| TR-win-condition-012 | win-condition.md | Win Condition | Emptied team → Waiting | ADR-0004 (now stated, review-2 N5 fix) | ⚠️ Partial (see note) |
| TR-win-condition-013 | win-condition.md | Networking | Late joiner state | ADR-0003/0004 Risks only | ⚠️ Partial |
| TR-win-condition-014 | win-condition.md | HUD | HUD elements | ADR-0004 (OnGUI placeholder) | ⚠️ Partial |
| TR-win-condition-015 | win-condition.md | Win Condition | Tunable knobs | ADR-0004 (tuning path undecided) | ⚠️ Partial |
| TR-win-condition-016 | win-condition.md | Audio | Last-pad sound | — | ❌ Gap |
| TR-win-condition-017 | win-condition.md | Testing | Offline + online test coverage | ADR-0005 | ✅ |
| **TR-self-destruct-001** | self-destruct.md | Self-Destruct | Owner arms own living tank once; effect on all clients | ADR-0002 (`[PunRPC]` to All), ADR-0003 (owner requests) | ✅ (code misses "living", F3) |
| **TR-self-destruct-002** | self-destruct.md | Self-Destruct | Blast hits non-friendly units only (`SameTeam`), falloff formula, once per unit | ADR-0003 (team comparison only through `Unit.SameTeam`; `""` belongs to no one) | ✅ (code is per collider, F6) |
| **TR-self-destruct-003** | self-destruct.md | Self-Destruct | All blast damage via `TellServerTakeDamage`; ignored while Ended | ADR-0003, ADR-0004 (`DamageBlocked`) | ✅ |
| **TR-self-destruct-004** | self-destruct.md | Self-Destruct | Death counts like any other | ADR-0004 (counts from `health > 0` in the scene) | ✅ |
| **TR-self-destruct-005** | self-destruct.md | Self-Destruct / Explosions | Chain death explosion: not team-filtered, capped | ADR-0003 (master spawns explosions); online victim set undecided (F1) | ⚠️ Partial |
| **TR-self-destruct-006** | self-destruct.md | Self-Destruct | `maximumDamage`, `sphereRadius` tunable | — (values are fields on binary tank prefabs; no ADR decides the tuning path) | ❌ Gap |
| **TR-self-destruct-007** | self-destruct.md | Testing | Gate 6e verifies AC 1-4 | ADR-0005 | ✅ (AC4 untested, F8) |
| **TR-turrets-001** | turrets.md | Turrets | Target `teamDetect` via `SameTeam`; never own team | ADR-0003 | ✅ |
| **TR-turrets-002** | turrets.md | Turrets | AI on master only; aim/fire visible to all; survives hand-over | ADR-0003 (master spawns shells) only; no ADR on master-run AI, its sync or hand-over | ⚠️ Partial |
| **TR-turrets-003** | turrets.md | Turrets | Master spawns flak shells; one hit; direct damage via master | ADR-0003 | ✅ |
| **TR-turrets-004** | turrets.md | Explosions | Splash formula + cap 30, master-applied once per body | ADR-0003 (master applies damage) | ✅ (code bypasses the RPC path, F5) |
| **TR-turrets-005** | turrets.md | Explosions | Online splash reaches every unit (instance of TR-game-concept-009) | ADR-0003 (routing only) | ⚠️ Partial |
| **TR-turrets-006** | turrets.md | Turrets | Gun turret hit-scan via master, never own team | ADR-0003 | ✅ (rate/range differ from GDD, F2b) |
| **TR-turrets-007** | turrets.md | Turrets | Room objects: master-created, master-destroyed, survive builder, reset-cleared | ADR-0003, ADR-0004 | ✅ |
| **TR-turrets-008** | turrets.md | Turrets | Six tuning knobs | — (binary prefab fields; `splashCap` lives on the explosion prefab) | ❌ Gap |
| **TR-turrets-009** | turrets.md | Testing | Gate 6e verifies AC 1-4 | ADR-0005 | ✅ (AC2/AC3 not directly tested, F8) |
| **TR-cargo-001** | cargo.md | Cargo | Pick-up/drop are master-applied requests; hold set on all clients | ADR-0003 ("other gameplay outcomes are applied by the master") | ✅ (no sender check, see TR-game-concept-007) |
| **TR-cargo-002** | cargo.md | Cargo | Same box requested twice → exactly one gets it | ADR-0003 (single authority) | ✅ (code race, F7) |
| **TR-cargo-003** | cargo.md | Cargo | Dropped box is a room object, reset-cleared | ADR-0003 ("Dropped cargo is a room object too"), ADR-0004 (sweep) | ✅ |
| **TR-cargo-004** | cargo.md | Deploy | Master-checked deploy, room object, hold emptied | ADR-0003 (deploy-request contract) | ✅ (team wording, F4) |
| **TR-cargo-005** | cargo.md | Deploy | Placement preview is local, no network identity | ADR-0003 ("a local, look-only picture with no network identity") | ✅ |
| **TR-cargo-006** | cargo.md | Cargo | Content = prefab name; legacy contents corrected in code | — (ADR-0003 Alt. 2 sets the "normalize in code" precedent for teams only; `architecture.md` principle 5 is not an ADR) | ⚠️ Partial |
| **TR-cargo-007** | cargo.md | HUD | HUD shows the hold's content | — (no UI ADR; and no code shows it, F9) | ❌ Gap |
| **TR-cargo-008** | cargo.md | Cargo | Pick-up radius tunable 2-10 m | — (hard-coded `5f`, `GameManager.cs:378`) | ❌ Gap |
| **TR-cargo-009** | cargo.md | Testing | Gate 6e verifies AC 1-3 | ADR-0005 | ✅ |

Note on TR-win-condition-012: review 2 rated it ⚠️ because ADR-0004 only implied Playing→Waiting; ADR-0004's
Decision now says it ("When a team's last tank leaves, `Playing` returns to `Waiting` … there is no forfeit
result"). It could be raised to ✅. Kept at ⚠️ in this draft only because the registry and index changes
proposed here are append-only; the upgrade is listed in CHANGES.md for the owner to accept.

### Traceability Summary
Total requirements: **53** (28 existing + 25 new: 7 self-destruct, 9 turrets, 9 cargo)
✅ Covered (Accepted ADR): **34** (existing 17 + new 17)
🟡 Covered (Proposed ADR only): **0**
⚠️ Partial: **11** (existing 7 + new 4)
❌ Gaps: **8** (existing 4 + new 4)

New requirements only: 17 ✅ · 4 ⚠️ · 4 ❌ (self-destruct 5/1/1, turrets 6/2/1, cargo 6/1/2).
"Covered" here means an Accepted ADR decides the requirement's architecture; where the code does not yet
conform, the row carries a finding (F-numbers below). Code conformance is not ADR coverage.

### Coverage Gaps (no ADR exists)
  ❌ TR-game-concept-008 (unchanged, deferred by ADR-0005) · join and late-join state sync · Networking · MEDIUM.
     Note: cargo's unbuffered `SetContent`/`SetPickedUpCargo` RPCs (`GameManager.cs:434`, `CargoManager.cs:36-45`)
     add to this ADR's scope: a late joiner sees dropped boxes with their prefab content and other tanks' holds
     empty, and would decide wrongly if it became master.
  ❌ TR-game-concept-011 (unchanged) · performance and network budgets · Performance · MEDIUM.
  ❌ TR-game-concept-010 (unchanged) · team-colour presentation · Rendering · LOW.
  ❌ TR-win-condition-016 (unchanged) · last-pad sound · Audio · LOW.
  ❌ TR-cargo-007 · HUD shows the hold · Suggested: fold into "/architecture-decision hud-and-ui-architecture"
     (Required ADR #4) · UI · LOW.
  ❌ TR-self-destruct-006, TR-turrets-008, TR-cargo-008 (and ⚠️ TR-win-condition-015) · where balance values
     live and how they are tuned · Suggested: "/architecture-decision balance-values-and-tuning" (a small ADR:
     serialized fields on binary prefabs vs code defaults vs a data file, given principle 5 "code over binary
     assets") · Core/Data · LOW.

---

### Cross-ADR Conflicts

Known conflict-prone areas: none recorded (`docs/consistency-failures.md` does not exist).

**No cross-ADR conflict.** ADR-0002 (Key Interfaces now name `InstantiateSceneObject` for shared units) and
ADR-0003 agree; ADR-0003 and ADR-0004 agree on room objects and the reset sweep; ADR-0005 allows online tests.

**ADR-vs-code and ADR-vs-GDD findings** (none blocking; numbered for CHANGES.md):

**F1 — HIGH for the M3 online playtest — splash damage online (TR-game-concept-009, TR-turrets-005,
TR-self-destruct-005).** Verified: `ExplosionPhysicsForce.Start` collects only bodies `IsControlledByMe`
(`ExplosionPhysicsForce.cs:29`), which for a `Player` tank means `isMine` and otherwise means "I am master"
(`:48-53`); only the master then applies damage (`:37-42`). So the master damages its own tank and non-player
units, and no client ever applies splash to a non-master player's tank. Consequences now that three approved
GDDs rest on it: (a) turrets AC1 ("alive with 30-60 after one flak hit") passes online for the wrong reason, a
remote tank takes 40, not 40 + splash; (b) the pulse cannon's splash and every death explosion behave the same;
(c) the cap (30) is only ever exercised on the master's tank. Explosions & Splash is a **Core**-layer system in
`architecture.md:40,63`, and `architecture.md:113` files the fix as "Should have before the relevant system is
built" although the system is built and MVP. Recommend promoting Required ADR #5 to "Must have before M3".

**F2 — MEDIUM — gun turret code vs GDD, never exercised by a gate.**
(a) Exceptions: `GTManager.ShowFeedback` runs on every client (`GTManager.cs:141`) and dereferences
`currentTarget` (`:111`), which only the master ever sets (`FindTarget` runs only on the master, `:134-140`,
`:256`); non-masters get `shooting = true` from the `SetShooting` RPC (`:93-96`), so each gun shot throws a
`NullReferenceException` on every non-master client. On the master, `CheckTargetOnSight` reads
`objectHit.transform.position` even when the raycast missed (`:196-197`), a `NullReferenceException` whenever a
target is behind cover. (b) GDD rule 4 and the tuning table (`turrets.md:40-41, 89-90`) say damage comes in bursts
of `bulletDamageinHitpoints` at `bulletsPerSecond` within `range` (40). The code applies damage once per
`reloadTime` (default 10 s, `:10, :147`), the hit-scan reaches `scanRadius` (default 10, `:13, :160`) and is cut
at a hard-coded 24.14107 m (`:199`); `range` is unused and `bulletsPerSecond` only paces the laser effect
(`:49, :105`). Prefab overrides cannot be read (binary). No gate phase spawns a gun turret
(`docs/L1-LOCAL-PLAYABILITY.md` row 14: "the gun turret is untested"), and #43 doubled its boxes
(`BlueCargo` → `BlueGunTurret`, `Cargo.cs:21-24`).

**F3 — MEDIUM — self-destruct arms on a dead tank (TR-self-destruct-001).** GDD rule 1 says "own *living*
tank" (`self-destruct.md:33`). `selfDestruct.Update`/`Trigger` check only `photonView.isMine` and `fired`
(`selfDestruct.cs:22, 28, 40`), never health. A dead player (5 s death delay, or spectating with no pad, where
the tank stays where it died) can press V and blast enemies around the wreck. Related: if a tank is killed while
armed and respawns before the effect ends (`destroyDelayWhenDead` 5 s, `PlayerMovementManager.cs:33,111-114`),
the blast fires at the respawn point, next to its own pad, and kills the fresh tank; edge case 4 / Open
Question 1 do not cover that case. Effect length is in a binary prefab; unverified.

**F4 — MEDIUM (design gap + ADR wording) — deployed turrets keep their prefab team.** ADR-0003:73 and
`control-manifest.md:64` say the master "deploys the cargo actually on board with the tank's team". The code
passes the team as instantiation data, but `Unit.Awake` applies it only when the prefab has no team
(`Unit.cs:43-54`), and the gate's `teams` check requires every `Red*`/`Blue*` prefab to carry its own team.
So a red tank that picks up one of the 3 `BlueFlakTurret` or 2 `BlueGunTurret` boxes deploys a **blue** turret
that shoots red tanks. Pick-up has no team check either. Neither turrets.md nor cargo.md says whether this is
intended (2018 colour-coded cargo) or a bug. Question for the owner (Q2); the ADR/manifest sentence should say
"team-less prefabs take the tank's team" whatever the answer.

**F5 — LOW — splash bypasses ADR-0003's damage path.** `ExplosionPhysicsForce.cs:42` calls
`hitpoints.TakeDamage(...)` directly, which ADR-0003's guideline and `control-manifest.md:69` forbid ("Never …
call `TakeDamage` locally"). It is functionally equivalent today (master-only, and `TakeDamage` checks
`isMasterClient` and `DamageBlocked`, `HitPointsManager.cs:18`), but it is the one place the rule is broken, and
it is exactly the code the splash ADR will rewrite.

**F6 — LOW — the self-destruct blast is per collider, not per unit.** `selfDestruct.cs:57-71` iterates
`OverlapSphere` colliders and looks for `HitPointsManager` on each collider's own transform, with no de-duplication
(unlike `ExplosionPhysicsForce.cs:29`, which de-duplicates by rigidbody). A unit whose `HitPointsManager` sits on
a GameObject with two colliders is damaged twice; one whose colliders are all on children is not damaged at all.
Same bug class as the shell double-hit fixed in `96c3ef6` (`PulseShellManager.cs:27-34`). Magnitude depends on
binary prefab colliders; unverified. TR-self-destruct-002 says "once per unit".

**F7 — LOW — double pick-up in one master frame (TR-cargo-002).** The master handles every queued RPC in one
`Update` (`PhotonHandler.cs:152-158`); `RequestPickUpCargo` finds the box with `Physics.OverlapSphere`
(`GameManager.cs:378, 388-411`) and removes it with `PhotonNetwork.Destroy`, which ends in Unity's deferred
`GameObject.Destroy` (`NetworkingPeer.cs:3482`). Two different tanks' requests dispatched in the same frame both
find the box: both holds are filled (cargo duplicated) and the second destroy logs an error. The same tank is
guarded, because `SetPickedUpCargo` to All runs locally at once (`NetworkingPeer.cs:3811-3817`). Fix: mark the
box taken (a flag or a `HashSet<int>` of view IDs) before destroying it.

**F8 — LOW — acceptance criteria the gate does not check.** Turrets AC2 (a second hit kills) is not tested: the
turret phase stops at the first damage (`GameplaySmokeTest.cs:604`); AC3 ("no splash ever exceeds `splashCap`,
logged") is checked only as total damage ≤ 70 (`:621-624`); the phase runs offline on the master's own tank, the
one case where splash works (F1). Self-destruct AC4 (V twice arms once) is not tested; the phase calls `Trigger`
once (`:895`). ADR-0005 requires new gameplay behaviour to come with a gate phase.

**F9 — LOW (GDD accuracy) — "The HUD shows the hold's content (existing)" (`cargo.md:96`) is not true.** No
script reads `CargoManager.pickedUpCargo` for display; the only HUD hook is a TODO (`CargoManager.cs:28-30`).
Players hear the pick-up/drop sounds only. Hence TR-cargo-007 is a gap in both ADR and code.

### ADR Dependency Order
(from `.claude/scripts/adr-dep-graph.sh`: 5 ADRs, no CYCLE, no NO_DEPS_SECTION, no UNFILLED_DEPS)
ORDER: adr-0001, adr-0002, adr-0005, adr-0003, adr-0004
Foundation: 1. ADR-0001 (engine pin)
Depends on Foundation: 2. ADR-0002 (PUN Classic; requires 0001) · 3. ADR-0005 (gate/branches; requires 0001)
Core: 4. ADR-0003 (master authority, team identity, room-owned deployables; requires 0002)
Feature: 5. ADR-0004 (match flow; requires 0002, 0003)
Unresolved dependencies: none; every dependency target is Accepted.

---

### GDD Revision Flags
No GDD revision flags from engine findings: Phase 5 found no HIGH-risk engine finding, and no GDD assumes
behaviour the pinned engine contradicts.

GDD **accuracy** findings from the code checks (not engine flags; recommended as text fixes in a docs PR, not as a
`Needs Revision` status, see CHANGES.md Q4):

| GDD | Text | Reality | Action |
|---|---|---|---|
| turrets.md:40-41, 89-90 | Gun turret: bursts at `bulletsPerSecond` within `range` 40 | Damage once per `reloadTime`, reach `scanRadius` capped at 24.14 m; `range` unused (F2b) | Decide: fix code or GDD |
| turrets.md:110 | AC1 "… (today it dies)" | Fixed in #43 (`dc-gate2`: 100 → 31/34/31) | Drop the parenthesis |
| cargo.md:96 | "The HUD shows the hold's content (existing)" | Not shown (F9) | Say "not yet; needed" |
| win-condition.md:80, 101, 164 | Self-destruct can destroy your own last pad; friendly-fire question open | Decided 2026-10-26: blast hits enemies only (self-destruct.md:21, code `selfDestruct.cs:63`) | Update the Interaction bullet, edge case 3 and Open Question 2 |
| game-concept.md:179, 192, 194 | Open: one flak hit kills; self-destruct friendly fire; `BlueCargo` | All three decided 2026-10-26 (#43) | Mark resolved |
| turrets.md:117-118 / cargo, turrets | Deployed turret team not specified | Prefab team wins (F4) | Owner decision, then state it |

### Engine Compatibility Issues

### Engine Audit Results
Engine: Unity 2017.3.0f3
ADRs with Engine Compatibility section: 5 / 5
Version consistency: all 5 name 2017.3.0f3. No stale versions.
Post-Cutoff APIs: none claimed; none needed.
Deprecated API References: **NOT ASSESSED** — `deprecated-apis.md` describes Unity 6.3, not 2017.3
(`VERSION.md:20`); no deprecated-API list exists for the pinned version. Guard remains ADR-0001 + CI compile.
Module docs consulted: none (no ADR names one; all describe Unity 6.3).

### Engine Specialist Findings
A `unity-specialist` definition exists in `.claude/agents/`, but no such agent type was available to this
reviewer, so the consultation was done inline against the code and the vendored PUN 1.88 source:
- **Confirmed:** `PhotonTargets.All` RPCs execute locally at once on the sender (`NetworkingPeer.cs:3811-3817`);
  the incoming queue is drained in one `Update` (`PhotonHandler.cs:152-158`); network destroy ends in Unity's
  deferred `GameObject.Destroy` (`NetworkingPeer.cs:3482`). Together these produce F7, and they are the same
  deferred-destroy class ADR-0003 already records for double destroys.
- **Unity anti-patterns in new code paths:** per-collider damage without de-duplication (F6);
  `RaycastHit.transform` read after a failed raycast (F2a); `FindObjectOfType` per call in
  `CargoManager.GetGameManager` (`CargoManager.cs:47-49`), harmless at key-press frequency.
- **Unverifiable here:** turret rotation sync to non-masters (needs the binary prefabs' observed components),
  the self-destruct effect length, prefab overrides of `GTManager` fields. All 2017.3-compatible; no C# 6 syntax
  or post-2017.3 API found in the files read.

### Architecture Document Coverage
`docs/architecture/architecture.md` now exists (it did not at review 2), so Phase 6 ran.
- **Every systems-index system (1-26) appears in the layer map.** No orphaned architecture.
- **Stale after the three GDDs:** `architecture.md:8` "GDDs Covered" omits self-destruct, turrets and cargo;
  `:101-102` still says 28 requirements and "18 implemented systems have no GDD" (by the systems index, 16 of the 21 Implemented rows still have none); the ADR audit table's
  "GDD requirements" column predates them.
- **Layer disagreement:** Cargo and Deploy are **Core** in `architecture.md:40-41,64-65` and `cargo.md:14`, but
  the systems index's Dependency Map puts them in the **Feature** layer (`systems-index.md:94`). Explosions &
  Splash is Core in `architecture.md`, Feature in the index.
- **Governing-ADR claims that overstate:** `architecture.md:69` "Turrets … turret AI | ADR-0003": ADR-0003 says
  nothing about where AI runs or how it syncs (TR-turrets-002). `:63` "Explosions & Splash … ADR-0003": ADR-0003
  does not decide who detects splash victims (F1).
- Data flow and API boundaries are not authored at the `standard` tier (by design, `architecture.md:7`); the
  cross-system flows the new GDDs need (blast → health, explosion → health, cargo → deploy) are covered by
  ADR-0003's contracts, except splash victim selection (F1) and master-run AI (TR-turrets-002).

---

### Verdict: CONCERNS

- No cross-ADR conflict; all 5 ADRs Accepted; no requirement rests on a Proposed ADR.
- 25 new requirements registered; 17 covered, 4 partial, 4 gaps. Overall 34 / 11 / 8 of 53.
- CONCERNS: F1 (online splash, a Core-layer system, MVP, now load-bearing for three GDDs) is still only partly
  covered; F2-F4 are MEDIUM code/design findings in systems the GDDs call "Implemented"; four new tuning/HUD gaps.
- NOT ASSESSED line item: deprecated-API audit (no 2017.3 reference). Phase 6 is now assessed.
- Not FAIL: the uncovered Core-layer items (TR-game-concept-008, F1) are partials or owner-deferred gaps
  (review-2 Q2), not newly uncovered Foundation/Core requirements.

### Blocking Issues
None.

### Required ADRs (most foundational first)
1. **Splash damage online** (TR-game-concept-009, TR-turrets-005, TR-self-destruct-005; F1, F5) — promote from
   "Should have" to "Must have before the M3 online playtest". Decide who selects victims (master over all
   bodies, or each owner for its own tank with a master request) and route damage through the ADR-0003 path.
2. Join and late-join state sync (TR-game-concept-008, TR-win-condition-013; add cargo `SetContent` /
   `SetPickedUpCargo` buffering).
3. Performance and network budgets (TR-game-concept-011).
4. RPC trust model (TR-game-concept-007; add pick-up/drop sender checks, damage-amount validation,
   `SetHealthFromClient`, deploy position limits).
5. HUD / UI and presentation (TR-win-condition-014/016, TR-game-concept-010, **TR-cargo-007**).
6. **New, small:** master-run AI and room-object behaviour sync — or an ADR-0003 amendment (TR-turrets-002):
   AI runs on the master, what other clients see (rotation, fire effects), hand-over.
7. **New, small:** balance values and tuning path (TR-self-destruct-006, TR-turrets-008, TR-cargo-008,
   TR-win-condition-015).
8. Amendment, not a new ADR: ADR-0003:73 and control-manifest:64 deploy-team wording (F4).

---

### Handoff (Phase 9)
Immediate actions:
1. `/architecture-decision splash-damage-online` (F1) — before the M3 online playtest.
2. Fix F2 (gun turret NREs, add a gun-turret gate phase) and F3 (living check), both small; decide F4 (Q2).
3. Docs PR: the GDD accuracy table above, `architecture.md` refresh, ADR-0003/manifest wording (CHANGES.md).

Pre-gate checklist:
- ✅ (by owner decision, review 2 Q5) test root = `Tools/verify-local.sh` + in-player smoke tests
  (`technical-preferences.md:49`), in place of `Assets/Tests/EditMode|PlayMode` and `.github/workflows/tests.yml`
  (both absent; `checks.yml` exists).
- ❌ `design/accessibility-requirements.md` — `/ux-design`
- ❌ `design/ux/interaction-patterns.md` — `/ux-design`
`/gate-check` should not be offered until the two UX files exist.
Rerun trigger: re-run `/architecture-review` after each new or amended ADR, and after F2/F3 land.

### Proposed session-state extract (for production/session-state/active.md, not written)
    ## Session Extract — /architecture-review 2026-10-27
    - Verdict: CONCERNS
    - Requirements: 53 total — 34 covered, 11 partial, 8 gaps
    - New TR-IDs registered: 25 (self-destruct 7, turrets 9, cargo 9) — pending owner approval
    - GDD revision flags: None (engine); accuracy fixes: turrets.md, cargo.md, win-condition.md, game-concept.md
    - Top ADR gaps: splash damage online; join/late-join sync; performance & network budgets
    - Report: docs/architecture/architecture-review-2026-10-27.md
