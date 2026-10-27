## Architecture Review Report
Date: 2026-10-27
Mode: full
Workflow: standard
Engine: Unity 2017.3.0f3 (C# 4, PUN Classic 1.88)
GDDs Reviewed: 11 (game-concept, systems-index, win-condition, self-destruct, turrets, cargo, and new: hover-movement, camera-cursor, deploy, hud, audio)
ADRs Reviewed: 5 (ADR-0001 to ADR-0005, all Accepted)
Reviewer: independent (wrote no ADR or GDD); review_mode lean, automation guided, docs.density balanced
Code basis: frozen clone at `b1ac8e4` (merge of PR #49, the five GDDs). No code has changed since review 3's basis `2df3a35`; the commits in between touch only docs.

<!-- RECEIPT: taken 2026-10-27 over the repository inputs as reviewed (clone at b1ac8e4). Applied by the owner the
     same day, after PR #50 (which fixed G6 and the client part of G4, the Leave button and the owner's engine
     loop): registry and index as drafted, TR-hover-movement-001 raised to covered by the B1 amendment, B2 and
     B4 applied, B3 deferred to Required ADR #2 (owner), the engine sync deferred to an audio story (owner),
     G9 resolved from the PlayerTank prefab dump (name tags are live). The tr-registry.yaml line was re-taken
     after the registry was written ("the receipt is taken last"). -->
Reviewed-Content-Hash: docs/architecture/adr-0001-engine-pin-unity-2017-3.md 31778ecdc8fed61193cf3dc10cc643d1dc453bc2
Reviewed-Content-Hash: docs/architecture/adr-0002-pun-classic-photon-cloud.md 8bfcea7af15cc7768526c8adb31cbb9079a52910
Reviewed-Content-Hash: docs/architecture/adr-0003-master-authority-and-team-identity.md ce995032251082eb9ec1e286be56c33e8db4823a
Reviewed-Content-Hash: docs/architecture/adr-0004-match-flow-matchmanager.md 55d2a54f4969786d90348579a28d7799dcd3247a
Reviewed-Content-Hash: docs/architecture/adr-0005-verification-gate-and-branch-model.md 97e635cf4623def7ee81dbcc279d0b303d56ada1
Reviewed-Content-Hash: design/gdd/audio.md 0960563b4d860c23c78aaeccd3ac4a110d685b59
Reviewed-Content-Hash: design/gdd/camera-cursor.md 893660d755e6a85cc14d8f9c9a1c66d3f8954206
Reviewed-Content-Hash: design/gdd/cargo.md 9cc07f5dbdc44b76ea1ef8cc974151bfb6f014c6
Reviewed-Content-Hash: design/gdd/deploy.md c0406e330fab5b829b1089e824f5d458c5042d99
Reviewed-Content-Hash: design/gdd/game-concept.md b102ab708d437d876c0a0fb383064fabb8592e65
Reviewed-Content-Hash: design/gdd/hover-movement.md da3d6c9d833d99aad2390fe510bbe125492c07c2
Reviewed-Content-Hash: design/gdd/hud.md 35cc9ec0460e91ab24e92748fab572feaae35e4f
Reviewed-Content-Hash: design/gdd/self-destruct.md c3ef71fd95d7658fdfb9046d29140484c4e74d17
Reviewed-Content-Hash: design/gdd/systems-index.md cf1a3188d2c2037d0c924da9917ba5ecd5b768f4
Reviewed-Content-Hash: design/gdd/turrets.md bb5d5493d16ae76edfebf17653a0e6d8efa33202
Reviewed-Content-Hash: design/gdd/win-condition.md a6169e0b0d36aeda107996ae3f11376d08e4c953
Reviewed-Content-Hash: docs/architecture/tr-registry.yaml 303b8f870414f5ca2cc5219aa43c64542ad2ff14
Reviewed-Content-Hash: docs/architecture/architecture.md 3bae7d28660118965239397eaa7f76f507a14580
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

- **Freshness check** against `architecture-review-2026-10-27.md` (Mode full, Workflow standard, so it covers this
  run): no `UNRESOLVED`, no `GONE`. `NEW`: the five GDDs. `CHANGED`: ADR-0003, `architecture.md`, cargo,
  game-concept, self-destruct, systems-index, turrets, win-condition. `UNCHANGED`: `tr-registry.yaml`, the engine
  reference, `project.yaml`, `technical-preferences.md`, this skill. A changed `architecture.md` reaches every
  phase, so the procedure calls for a full re-run, not a `single-gdd` scope. This is that run.
  The `git diff 2df3a35 a2b1ea7` shows that every `CHANGED` file except systems-index is the owner applying
  review 3's own CHANGES (ADR-0003 F4 amendment, `architecture.md` refresh, GDD text fixes). Each was re-read and
  checked against review 3's proposals; none adds or removes a requirement.
- Denominators: **N_gdd = 11**, **N_adr = 5**. All 10 system GDDs carry `## Summary`; the five new ones carry
  `## Detailed Design`, `## Formulas`, `## Dependencies`, `## Tuning Knobs`, `## Acceptance Criteria`. They are
  short (116-139 lines), so they were read in full. All 5 ADRs carry `## Status`, `## Engine Compatibility`,
  `## ADR Dependencies`, `## GDD Requirements Addressed`. No structural gaps.
- **No ADR's `GDD Requirements Addressed` names any of the five new GDDs.** Coverage is judged from each ADR's
  Decision text, and counted only where an Accepted ADR really decides the requirement's architecture.
- `docs/consistency-failures.md`: does not exist. Engine reference: only `VERSION.md` applies to 2017.3
  (`VERSION.md:19-21`); the other files describe Unity 6.3. No ADR names a module doc.
- Loaded 11 GDDs, 5 ADRs, engine Unity 2017.3.0f3.

### Requirement extraction (Phase 2)

Pre-loaded registry: 54 entries, all active. Next free IDs: `TR-hover-movement-001`, `TR-camera-cursor-001`,
`TR-deploy-001`, `TR-hud-001`, `TR-audio-001` (no prior entries for these slugs, no provisional baseline rows:
`architecture.md` has no Technical Requirements Baseline section).

**The 54 existing entries:** no GDD wording behind them changed in intent (review 3's text fixes only), so **no
`revised` edit, no deprecation, no supersession**. The draft registry keeps lines 1-486 byte-identical, except the
`version:` header field (3 → 4), as the procedure requires. Registry blob of the existing part:
`9120ff0f…` before and after (checked with `git hash-object`).

**Overlap handled by reference, not duplication:**

| New GDD statement | Already registered as | Handling |
|---|---|---|
| deploy.md rule 3: the master's checks, room object, empties the hold | TR-cargo-004 | referenced from TR-deploy-002 |
| deploy.md rule 3: team "whatever the box's colour" | TR-cargo-010 | referenced from TR-deploy-002 |
| deploy.md overview: preview has no network identity | TR-cargo-005 | referenced from TR-deploy-001 |
| deploy.md: no deploys after a match ends | TR-cargo-004 ("match not ended") | not re-registered |
| hud.md rows 8-10: match line, hint, result banner, countdown | TR-win-condition-014 | referenced from TR-hud-006 |
| hud.md Known Issues: hold not shown | TR-cargo-007 | referenced from TR-hud-006 |
| hud.md rule 4: team colours | TR-game-concept-010 | referenced from TR-hud-006 |
| audio.md: no match-result sound | TR-win-condition-016 | not re-registered |
| hover-movement.md rule 8 / camera-cursor.md rule 5: respawn flow | TR-win-condition-002/003 | only the new parts (death timer, camera/cursor hand-over) registered |

Only statements that imply an architectural decision were extracted. Pure feel values (forces, positions) are
folded into the requirement they belong to; each GDD's tuning table and test plan got one entry each, following
the TR-win-condition-015/017 precedent. Three statements of the GDDs are **open questions** and were not
registered as requirements: deploy ground-snapping (deploy.md OQ1), the hit-taken sound (audio.md OQ2), the
player list (hud.md OQ2). TR-audio-003 rests on audio.md's Game Feel line while its Open Question 1 is still open:
see Q5.

**New requirements: 27** — hover-movement 6, camera-cursor 5, deploy 5, hud 7, audio 4. The three new owner
rules are TR-deploy-002 (10° per notch), TR-deploy-003 (no deploy with the cursor visible or from a dead tank),
TR-hud-005 (Leave Game shows with the cursor).

---

### Traceability Matrix

Existing 54 rows: unchanged from `docs/architecture/requirements-traceability.md` (36 ✅, 10 ⚠️, 8 ❌; no code has
changed since review 3). They are repeated in the draft index. New rows:

| Requirement ID | GDD | System | Requirement (short) | ADR Coverage | Status |
|---|---|---|---|---|---|
| **TR-hover-movement-001** | hover-movement.md | Hover Movement | Owner-authoritative movement; others kinematic via PUN view sync | ADR-0002 provides the view sync; **no ADR decides movement authority** (the GDD's ADR-0003 citation is wrong, G2) | ⚠️ Partial |
| **TR-hover-movement-002** | hover-movement.md | Hover Movement | Rigidbody hover/drive/jump/land in the physics step; terrain-only ray | ADR-0001 (PhysX, `UnityEngine.Input` on the pinned engine); local to the owner, no shared state | ✅ |
| **TR-hover-movement-003** | hover-movement.md | Hover Movement | Input only for the owner's living tank with the cursor hidden; death timer starts respawn | ADR-0004 (respawn flow); input mode undecided (TR-camera-cursor-002, G1) | ⚠️ Partial |
| **TR-hover-movement-004** | hover-movement.md | Hover Movement | Frame-rate independence | — (no ADR or manifest rule; code violates it, `PlayerMovementManager.cs:161,169`) | ❌ Gap |
| **TR-hover-movement-005** | hover-movement.md | Hover Movement | Five tuning knobs | — (Required ADR #7) | ❌ Gap |
| **TR-hover-movement-006** | hover-movement.md | Testing | Gate 6e (drive onto box, respawn) for AC2/AC5; playtest for AC1/3/4 | ADR-0005 (AC2's gate evidence is weak, G10) | ✅ |
| **TR-camera-cursor-001** | camera-cursor.md | Camera & Cursor | Main camera driven only by the local tank; cockpit/chase swap with cursor hidden | ADR-0001 (built-in camera), ADR-0002 (`photonView.isMine`); local presentation | ✅ |
| **TR-camera-cursor-002** | camera-cursor.md | Camera & Cursor / Input | One input mode (hidden+locked = drive, visible+free = UI) that every gameplay input and the clickable UI follow | — (G1: a global `Cursor.visible` written by three scripts, read by five; no ADR) | ❌ Gap |
| **TR-camera-cursor-003** | camera-cursor.md | Camera & Cursor | Overhead camera + cursor at the respawn flow; tank camera + hidden cursor at respawn | ADR-0004 decides the respawn flow (`GameManager.Respawn`, `RepairPad`), not the camera/cursor hand-over (G7 timing) | ⚠️ Partial |
| **TR-camera-cursor-004** | camera-cursor.md | Camera & Cursor | `transitionTime`, positions tunable | — (Required ADR #7) | ❌ Gap |
| **TR-camera-cursor-005** | camera-cursor.md | Testing | Gate 6e respawn for AC4; playtest A3/A4 | ADR-0005 | ✅ |
| **TR-deploy-001** | deploy.md | Deploy | Local preview at the deploy point; follows the tank; cancel keeps cargo | ADR-0003 (local, look-only preview, N2) (code: G5a, preview freezes when aimed at the sky) | ✅ |
| **TR-deploy-002** | deploy.md | Deploy | **10° per wheel notch**, still between notches, new preview unrotated; request carries rotation | ADR-0003 (`DeployUnit(cargoManager, position, rotation)`) (code: the preview spins, a story; G6 engine note) | ✅ |
| **TR-deploy-003** | deploy.md | Deploy | **No deploy with the cursor visible or from a dead tank** | ADR-0003's master check list has no "living tank" check (G4); cursor part rests on TR-camera-cursor-002 (G1) | ⚠️ Partial |
| **TR-deploy-004** | deploy.md | Deploy | Rotation step 5-45° tunable | — (Required ADR #7) | ❌ Gap |
| **TR-deploy-005** | deploy.md | Testing | Gate 6e deploy, deploy-team, deploy-refused, 6c match; AC4/5 checks with the fix | ADR-0005 | ✅ |
| **TR-hud-001** | hud.md | HUD | Own-tank hull and fuel bars | — (data from ADR-0003's `UpdateHealth`; display undecided, Required ADR #4) | ❌ Gap |
| **TR-hud-002** | hud.md | HUD | Team player counts; correct offline | — (data from PUN player properties, ADR-0002; display undecided) (code: G11) | ❌ Gap |
| **TR-hud-003** | hud.md | HUD | Ping and FPS every ~0.6 s | — | ❌ Gap |
| **TR-hud-004** | hud.md | HUD | Target panel; recovers when the target re-enters the screen | — (Targeting has no GDD; L1 row 9 bug) | ❌ Gap |
| **TR-hud-005** | hud.md | HUD | **Leave Game shows with the cursor visible**; leaves to the launcher | ADR-0002 (`OnLeftRoom` cleanup guideline) covers the leave path; visibility rests on TR-camera-cursor-002 (G1) | ⚠️ Partial |
| **TR-hud-006** | hud.md | HUD | Plural words, no overlap, crosshair, centre clear, red/blue | — (Required ADR #4; refs TR-game-concept-010, TR-win-condition-014, TR-cargo-007) | ❌ Gap |
| **TR-hud-007** | hud.md | Testing | Playtest A14/A17; gates 6e win, 6c match | ADR-0005 | ✅ |
| **TR-audio-001** | audio.md | Audio | Combat cues 3D on every client in range | ADR-0002/0003 replicate the objects and the firing state; no ADR decides audio (voices, 3D settings) | ⚠️ Partial |
| **TR-audio-002** | audio.md | Audio | Owner-only feedback cues | ADR-0001 (local `AudioSource`); cargo cues ride on the master's `SetPickedUpCargo` (ADR-0003) | ✅ |
| **TR-audio-003** | audio.md | Audio | Engine hums while hovering, silent when landed, on every client | — (the landed state is never sent to other clients, G8) | ❌ Gap |
| **TR-audio-004** | audio.md | Testing | Playtest only (A2, A16, B2) | ADR-0005 decides the gate, not the manual playtest; the cited rows don't list these cues (G14) | ⚠️ Partial |

### Traceability Summary
Total requirements: **81** (54 existing + 27 new)
✅ Covered (Accepted ADR): **45** (existing 36 + new 9)
🟡 Covered (Proposed ADR only): **0**
⚠️ Partial: **17** (existing 10 + new 7)
❌ Gaps: **19** (existing 8 + new 11)

New only, per system (✅ / ⚠️ / ❌): hover-movement 2/2/2 · camera-cursor 2/1/2 · deploy 3/1/1 · hud 1/1/5 ·
audio 1/2/1 · total 9/7/11.

"Covered" means an Accepted ADR decides the requirement's architecture. Where local, single-client behaviour needs
nothing beyond the pinned engine's APIs, ADR-0001 is counted (TR-hover-movement-002, TR-camera-cursor-001,
TR-audio-002). HUD requirements are not counted under ADR-0001: it names both UGUI and `OnGUI`, and choosing
between them is Required ADR #4. Code conformance is not ADR coverage; code findings are separate (G-numbers).

### Coverage Gaps (no ADR exists)
New in this review:
  ❌ TR-camera-cursor-002 · Camera & Cursor → **input mode** (driving vs UI) · Suggested: widen Required ADR #4 to
     "/architecture-decision hud-ui-and-input-mode" · Domain: Input/UI · Engine Risk: LOW (2017.3 `Cursor` API).
     Core layer; three of the owner's new rules depend on it (G1).
  ❌ TR-hud-001, -002, -003, -004, -006 · HUD → display elements · Suggested: Required ADR #4 (UGUI vs `OnGUI`,
     how systems feed the HUD: today `GameManager.SetHullBar`/`SetFuelBar` push into scene objects it holds,
     `TeamCounter` listens to PUN callbacks, `MatchManager` draws with `OnGUI`) · UI · LOW.
  ❌ TR-hover-movement-004 · frame-rate independence · Suggested: a rule in the control manifest from Required
     ADR #3 (performance and budgets): "per-frame changes scale with `Time.deltaTime`" · Core · LOW.
  ❌ TR-hover-movement-005, TR-camera-cursor-004, TR-deploy-004 · tuning · fold into Required ADR #7 (balance
     values and tuning path) with TR-self-destruct-006, TR-turrets-008, TR-cargo-008 · LOW.
  ❌ TR-audio-003 · engine sound on remote clients · needs the landed state replicated (a PhotonView-observed field
     or an RPC) · Suggested: Required ADR #6 (room-object/presentation state sync) or a small audio ADR · LOW.
Unchanged from review 3: TR-game-concept-008, -010, -011, TR-win-condition-016, TR-cargo-007, TR-self-destruct-006,
TR-turrets-008, TR-cargo-008.

---

### Cross-ADR Conflicts

Known conflict-prone areas: none recorded (`docs/consistency-failures.md` does not exist).

**No cross-ADR conflict.** The only ADR change since review 3 is ADR-0003's F4 amendment
(`adr-0003-…md:75-78`: every deployed unit takes the deploying tank's team). It agrees with ADR-0004's GDD table
("Rule 1: deployed pad takes the deployer's team | instantiation data read in `Unit.Awake`") and with
`control-manifest.md:64`.

**ADR-vs-code, ADR-vs-GDD and code findings** (none blocking; numbered G to keep them apart from review 3's F1-F9;
F1-F9 still stand, no code has changed):

**G1 — MEDIUM — the input mode is an undecided Core contract (TR-camera-cursor-002; TR-hover-movement-003,
TR-deploy-003, TR-hud-005 rest on it).** `Cursor.visible` is used as a global "driving vs UI" flag. Writers:
`CursorVisibilityManager.cs:11,18`, `GameManager.cs:333-334` (`Respawn`), `RepairPad.cs:82-83` (`Spawn`), plus the
legacy `PlayerMove.cs:42-43`. Readers: `PlayerMovementManager.cs:121,236,268`, `AutoCannon.cs:109`,
`CameraManager.cs:35`, `GUIManager.cs:11,23`. The deploy click ignores it (`GroundPlacementController.cs:162-168`),
which is the bug the owner's new rule fixes. The design docs also have a dependency cycle through it:
`systems-index.md:27,89` says Camera & Cursor depends on Hover Movement, while `hover-movement.md:14` lists Camera &
Cursor as a key dependency. Recommendation: Required ADR #4 decides one owner of the input mode (a small static
`InputMode` that the cursor manager, the death/respawn flow and the HUD set, and that gameplay reads), so the deploy,
Leave-button and movement fixes all read the same rule; that also breaks the cycle.

**G2 — MEDIUM — owner-authoritative movement is not decided by any ADR (TR-hover-movement-001).**
`hover-movement.md:50-51` ("ADR-0003 leaves movement with the owner") and `:91-92` cite ADR-0003, but ADR-0003's
Decision (`adr-0003-…md:59-81`) says nothing about movement, and its Summary (`:18-19`) says "Health changes and
other gameplay outcomes are applied by the Photon master client … other clients only request them." The code makes
movement owner-authoritative (`PlayerMovementManager.cs:60-63` makes non-owners kinematic; input and forces run only
`if photonView.isMine`, `:118, :259`). This is the right model for a hovertank on PUN, and it is a trust surface
(speed, teleport) that Required ADR #2 must bound. Recommendation: amend ADR-0003 with one bullet, "A tank's pose is
owner-authoritative, synced by its PhotonView; the RPC trust model (Required ADR #2) bounds it", and then the GDD
citation becomes true.

**G3 — LOW (ADR accuracy) — pulse shells and their explosions are spawned by the shooter, not the master.**
ADR-0003 Decision `:62-63`: "The master … spawns shells and explosions"; `architecture.md:62`: Pulse Cannon "shell
spawn (master)". Code: `PlayerMovementManager.cs:253` calls `PhotonNetwork.Instantiate` for the pulse shell on the
firing client; `PulseShellManager.cs:15,33,45` has that client simulate the collision and instantiate the explosion.
The master RPC `GameManager.SpawnPulseShell` exists (`GameManager.cs:160-167`) but its call is commented out
(`PlayerMovementManager.cs:252`). Damage still goes through `TellServerTakeDamage` (`PulseShellManager.cs:40`), so the
functional impact is small: hit detection is the shooter's, and a shell in flight vanishes if its shooter leaves.
Recommendation: amend the ADR-0003 sentence ("flak shells and unit death explosions are spawned by the master; a
tank's pulse shells are spawned by its owner, whose client detects the hit and requests the damage") and
`architecture.md:62`, or move the spawn to the master. Pulse Cannon has no GDD yet, so no TR is affected.

**G4 — MEDIUM — the "no deploy from a dead tank" rule is client-side only (TR-deploy-003).** `deploy.md:45-46` puts
the rule on the client (a dead tank's preview is removed). The master's checks (ADR-0003 `:70-74`,
`GameManager.cs:457-468`) are sender, empty hold, missing prefab and ended match. There is no health check. The
client gap is real, not hypothetical: the cursor stays hidden for `destroyDelayWhenDead` (5 s) after death, because
`GameManager.Respawn` shows it only when the death timer runs out (`PlayerMovementManager.cs:108-115`,
`GameManager.cs:331-334`). So during those 5 s a dead player can still click with a hidden cursor. Recommendation:
the master also refuses a tank whose `HitPointsManager.health <= 0` (one condition in `RequestDeploy`), recorded as an
ADR-0003 amendment next to the F4 one; the client removes the preview at death. Owner question Q1.

**G5 — LOW — deploy behaviour the GDD does not describe.** (a) The preview moves only while a ray from the mouse
position (the screen centre, since the cursor is locked) hits a collider (`GroundPlacementController.cs:128-135`):
aimed at the sky, it stays where it was while the tank drives on, which contradicts `deploy.md:32` ("It moves with
the tank"). (b) If the hold empties while previewing (Z drop), `,` no longer removes the preview, because
`TogglePlacement` needs a non-empty hold (`:75`). The next click then sends a request that the master refuses (empty
hold, `GameManager.cs:466`) and removes the preview. Recommendation: in the rotation/living fix story, place the
preview at the deploy point every frame and end it when the hold empties; add both cases to deploy.md's Edge Cases.

**G6 — LOW (engine note) — implementing "one notch, one step" on 2017.3.** `Input.mouseScrollDelta.y` is ±1 per notch
on a standard Windows wheel, but fractional on precision touchpads and high-resolution wheels. The bug is
`mouseWheelRotation += delta` followed by `Rotate(…, mouseWheelRotation * 10f)` every frame (`:143-144`), never reset
(`:16`). The fix should rotate by `delta × 10°` once per frame and reset per preview. If exact 10° steps are wanted on
fractional devices, accumulate the delta and step on whole notches. The `Debug.Log` every frame (`:142`) goes with it.

**G7 — LOW (GDD accuracy) — camera and respawn timing.** `camera-cursor.md:39` ("Death: the camera is detached …")
and `:52` ("→ Visible … at death"): `CameraManager.Detach` (`CameraManager.cs:28-30`) is never called. At the end of
the death timer the main camera stays parented to the tank and is disabled (`GameManager.cs:331-332`). The overhead
view and cursor come `destroyDelayWhenDead` after death, not at death. `hover-movement.md:129` (AC5, "respawns after
5 s"): after 5 s the overhead map shows; the player respawns by clicking an own-team pad (`RepairPad.cs:18-29`) or
automatically when a pad appears while waiting (`GameManager.cs:338-358`), as playtest A13 says. Text fixes.

**G8 — LOW (GDD accuracy) — the engine sound on other clients.** `audio.md:35` (cue 1: "stops at landing; restarts
at take-off; everyone near the tank"). `Land` and `TakeOff` run only on the owner (`PlayerMovementManager.cs:215,228`,
called from owner-only code), and `isLanded` is never sent to other clients. Remote clients hear only the spawn play,
which ends after one pass of the clip (`Loop = False`). Making the engine loop (OQ1) would make remote landed tanks hum
for ever. Hence TR-audio-003 is a gap: it needs the landed state synced.

**G9 — LOW (GDD accuracy, unverifiable part) — "`playerInfoPanelPrefab` is never used" (`hud.md:130`).**
`PlayerInfoController.cs:11-17` instantiates it for every other player's tank: a floating name label in team colour
with the master marked (`PlayerInfoPanelController.cs:28-49`, `GameManager.cs:243-312`). Whether the component is on
the tank prefabs cannot be read here (binary prefabs). If it is, the HUD has a 13th element (name tags over enemy and
friendly tanks) that bears on TR-game-concept-010 (team colour at 100 m). Owner question Q4.

**G10 — LOW — hover AC2's gate evidence is weak.** `hover-movement.md:125` cites "gate 6e drives onto a box". The
cargo phase teleports the tank 2 m above a box and waits 2 s (`GameplaySmokeTest.cs:350-360`); it asserts nothing
about hovering. AC2 is in effect manual (playtest A1). The procedure accepts this (ADR-0005 needs gate phases for
*new* behaviour); the GDD should not claim more.

**G11 — LOW (unverified) — team counts may lag.** `TeamCounter` (`TeamCounter.cs:21-45`) reads
`PunTeams.PlayersPerTeam` inside the same `OnPhotonPlayerPropertiesChanged` broadcast that `PunTeams` uses to rebuild
it; PUN's message order between scripts is not defined. Its `OnJoinedRoom` never fires, because the HUD's scene loads
after the join. Counts are refreshed by the team property that `GameManager.Start` sets (`:135,142`). If playtest
A14/B1 sees a count one behind, call `PunTeams.UpdateTeamsNow()` first, as `GameManager.Start:121` does.

**G12 — LOW (performance) — `FindObjectOfType<TerrainCollider>()` every physics step** (`PlayerMovementManager.cs:278`,
and `:196, :203`), and `Debug.Log("CanLand false")` every frame while a landing waits (`:183`, a GDD Known Issue). It
also assumes one terrain. For Required ADR #3; cache it in `Start` (it already is, at `:66`, and then overwritten).

**G13 — LOW (playtest sheet vs owner rule).** `production/qa/playtests/playtest-2026-10-27-owner.md:37` (A11) tells the
tester to deploy a `RedFlakTurret` box "it stays red … it fires at you". That is the F4 bug the owner ruled against
(TR-cargo-010). It is fine for this build, but the row must change when the F4 story lands. A14 asks for a "player
list" that hud.md says doesn't exist.

**G14 — LOW — audio ACs cite playtest rows that don't list the cues.** `audio.md` AC2 cites A2/A16 and AC3 cites B2.
A2 is "Shift raises hover … Space jumps" and A16 is "Sounds: cannon, pickup/drop, UI clicks", so neither names jump,
take-off or landing sounds. B2 is "see and shoot the other". Add the cues to the sheet.

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
No GDD revision flags from engine findings. Phase 5 found no HIGH-risk engine finding, and no new GDD assumes
behaviour that the pinned engine contradicts (the 2017.3 `Cursor`, `Input.mouseScrollDelta`, `AudioSource`, UGUI and
`OnGUI` APIs the GDDs rely on all exist).

GDD **accuracy** findings (text fixes in a docs PR, not a `Needs Revision` status; the same treatment the owner
chose for review 3, Q4):

| GDD | Text | Reality | Action |
|---|---|---|---|
| hover-movement.md:50-51, 91-92 | "ADR-0003 leaves movement with the owner" | ADR-0003 says nothing on movement (G2) | Keep after the ADR-0003 amendment; until then "owner-authoritative (code; ADR pending)" |
| hover-movement.md:129 | AC5 "respawns after 5 s" | Overhead map after 5 s, respawn on a pad click or a new pad (G7) | Reword |
| hover-movement.md:125 | AC2 "gate 6e drives onto a box" | The gate places the tank; no hover assertion (G10) | "manual (A1); the gate only exercises it" |
| camera-cursor.md:39, 52 | "camera is detached", cursor visible "at death" | Camera disabled, not detached; switch after `destroyDelayWhenDead` (G7) | Reword |
| deploy.md:32 | "It moves with the tank" | Only while the screen-centre ray hits a collider (G5a) | Add edge case, or fix with the story |
| deploy.md Edge Cases | — | Dropping cargo while previewing (G5b) | Add edge case |
| audio.md:35 | Engine "stops at landing … everyone near" | Stop/restart only on the owner's client (G8) | Reword; note TR-audio-003 needs sync |
| audio.md AC2/AC3 | cites A2/A16, B2 | Rows don't list those cues (G14) | Add the cues to the playtest sheet |
| hud.md:130 | "`playerInfoPanelPrefab` is never used" | Used by `PlayerInfoController` (G9) | Owner check (Q4) |

### Engine Compatibility Issues

### Engine Audit Results
Engine: Unity 2017.3.0f3
ADRs with Engine Compatibility section: 5 / 5
Version consistency: all 5 name 2017.3.0f3. No stale versions.
Post-Cutoff APIs: none claimed; none needed by the new GDDs.
Deprecated API References: **NOT ASSESSED**. `deprecated-apis.md` describes Unity 6.3, not 2017.3
(`VERSION.md:19-21`), and no deprecated-API list exists for the pinned version. The guard remains ADR-0001 + the CI
compile step.
Module docs consulted: none (no ADR names one; all describe Unity 6.3).

### Engine Specialist Findings
No `unity-specialist` agent type was available to this reviewer, so the consultation was done inline against the
code, as in review 3:
- **Confirmed for the new GDDs:** `Cursor.visible` / `Cursor.lockState` (2017.3) behave as the GDDs say. With
  `CursorLockMode.Locked`, the standalone player hides and centres the cursor, so the screen-centre raycast in
  `GroundPlacementController.cs:128` is a crosshair ray (G5a). `Input.mouseScrollDelta` exists in 2017.3 (G6).
  `AudioSource.PlayClipAtPoint` creates a temporary 3D source on the local client only, so cues 2-4 and 6 are local
  by construction, as audio.md says.
- **Unity anti-patterns in the reviewed paths:** a scene search every physics step (G12); per-frame `Debug.Log`
  (`GroundPlacementController.cs:142`, `PlayerMovementManager.cs:180,183`); per-frame accumulation applied as an
  absolute delta (G6); `Vector3.Slerp` on positions for the camera swap (`CameraManager.cs:46`) gives a curved,
  ease-out path that ends well before `transitionTime`. That is harmless, but the GDD's "over 1 s" is nominal.
- **Unverifiable here:** prefab values the GDDs took from dumps (jump fuel 20, which overrides the code default 30
  at `PlayerMovementManager.cs:31`; the deploy point; audio clips and volumes; the HUD layout). They are accepted as
  given: no code contradicts them.

### Architecture Document Coverage
`docs/architecture/architecture.md` exists; Phase 6 ran.
- **Every systems-index system (1-26) appears in the layer map**; no orphaned architecture. The five new GDDs' layers
  agree with it (hover-movement, camera-cursor, deploy: Core; hud, audio: Presentation).
- **Stale after the five GDDs:** `:8` "GDDs Covered" omits them. `:101-103` says 54 requirements and "16 implemented
  systems have no GDD"; by the systems index, 11 of the 21 Implemented rows now have none.
- **Governing-ADR claims that overstate or are wrong:** `:56` Hover Movement "ADR-0001" omits that movement authority
  is undecided (G2). `:62` Pulse Cannon "shell spawn (master)" is false (G3). `:57` Camera & Cursor "owns … the main
  camera's parent and mode": the cursor mode is written by three scripts in two other systems (G1). `:75` HUD lists
  `PingCounter`, which hud.md says is in no scene.
- **Layer disagreement (new):** `systems-index.md:84` puts Audio in the Foundation layer ("Audio — cosmetic"), while
  `architecture.md:32` and `audio.md:15` put it in Presentation. `systems-index.md:27,89` makes Camera & Cursor depend
  on Hover Movement, while `hover-movement.md:14` makes Hover Movement depend on Camera & Cursor (G1).
- **Required ADR list:** #4 should add the input mode and the HUD element set (TR-camera-cursor-002, TR-hud-*). #8
  ("Audio cues … can defer") should name TR-audio-001/003. #7 should add the three new tuning TRs.

---

### Verdict: CONCERNS

- No cross-ADR conflict; all 5 ADRs Accepted; no requirement rests on a Proposed ADR.
- 27 new requirements registered: 9 covered, 7 partial, 11 gaps. Overall 45 / 17 / 19 of 81.
- CONCERNS, for these reasons:
  - The input mode (G1) is an undecided Core-layer contract, and three of the owner's new rules depend on it.
  - Owner-authoritative movement (G2) is undecided, and a GDD cites an ADR for it that doesn't decide it.
  - The dead-tank deploy rule is client-only (G4).
  - The HUD has no architecture (5 of 7 HUD requirements are gaps).
- Not FAIL. The new Core-layer gaps are of two kinds: tuning or coding-standard gaps (frame-rate independence,
  tuning knobs), and an implicit contract that works today (the input mode). None blocks the stories now planned (the
  deploy rotation, the deploy living/cursor check, the Leave button), each of which can be written against the
  current `Cursor.visible` convention. Review 3's F1 (online splash) still stands as the most important open
  Core item for M3.
- NOT ASSESSED line item: deprecated-API audit (no 2017.3 reference).

### Blocking Issues
None.

### Required ADRs (most foundational first)
1. **Splash damage online** (unchanged from review 3; Must have before the M3 online playtest).
2. Join and late-join state sync (TR-game-concept-008, TR-win-condition-013).
3. RPC trust model (TR-game-concept-007). **Add:** bounds on owner-authoritative movement (G2) and the deploy
   living check if it stays client-side (G4).
4. Performance and network budgets (TR-game-concept-011). **Add:** TR-hover-movement-004 as a manifest rule, G12.
5. **HUD, UI and input mode** (widened Required ADR #4): UGUI vs `OnGUI`; how systems feed the HUD; **one owner of
   the input mode** (TR-camera-cursor-002); TR-hud-001..006, TR-win-condition-014, TR-cargo-007,
   TR-game-concept-010. Move it from "Should have" to "before the HUD/Leave-button fix stories", because three new
   owner rules read the input mode.
6. Master-run AI and room-object/presentation state sync (TR-turrets-002). **Add:** TR-audio-003 (landed state for
   remote engine sound).
7. Balance values and tuning path. **Add:** TR-hover-movement-005, TR-camera-cursor-004, TR-deploy-004.
8. Amendments, not new ADRs: ADR-0003 movement authority (G2), pulse-shell spawn wording (G3), deploy living
   check (G4).

---

### Handoff (Phase 9)
Immediate actions:
1. Amend ADR-0003 (G2, G3, G4): three short bullets, the owner's call (Q1-Q3). Then TR-hover-movement-001 and
   TR-deploy-003 become ✅.
2. `/architecture-decision hud-ui-and-input-mode` (widened Required ADR #4), before the Leave-button and deploy
   cursor fixes, so all of them read one input mode.
3. Docs PR: the GDD accuracy table above, `architecture.md` refresh, `systems-index.md` layer/dependency fixes,
   playtest sheet rows (CHANGES.md).

Pre-gate checklist:
- ✅ (by owner decision, review 2 Q5) test root = `Tools/verify-local.sh` + in-player smoke tests, in place of
  `Assets/Tests/EditMode|PlayMode` and `.github/workflows/tests.yml` (both absent; `checks.yml` exists).
- ❌ `design/accessibility-requirements.md` — `/ux-design`
- ❌ `design/ux/interaction-patterns.md` — `/ux-design` (more pressing now: hud.md and camera-cursor.md define
  interaction patterns, such as Esc for the cursor and the Leave button, that this file should hold).
`/gate-check` should not be offered until the two UX files exist.
Rerun trigger: re-run `/architecture-review` after the ADR-0003 amendment and after Required ADR #4 is written.

### Proposed session-state extract (for production/session-state/active.md, not written)
    ## Session Extract — /architecture-review 2026-10-27 (review 4)
    - Verdict: CONCERNS
    - Requirements: 81 total — 45 covered, 17 partial, 19 gaps
    - New TR-IDs registered: 27 (hover-movement 6, camera-cursor 5, deploy 5, hud 7, audio 4) — pending owner approval
    - GDD revision flags: None (engine); accuracy fixes: hover-movement.md, camera-cursor.md, deploy.md, audio.md, hud.md
    - Top ADR gaps: HUD/UI and input mode; movement authority (ADR-0003 amendment); splash damage online (review 3)
    - Report: docs/architecture/architecture-review-2026-10-27-2.md
