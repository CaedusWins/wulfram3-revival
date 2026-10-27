# Adoption Plan

> **Generated**: 2026-10-26 (a focused `/adopt` pass, run by Claude on the owner's go-ahead)
> **Project phase**: Systems Design (project.stage, set by the Concept gate 2026-10-26; the code is further along than the design docs)
> **Engine**: Unity 2017.3.0f3 / C# 4 (configured 2026-10-26; coherence check: 3 match, 0 differ)
> **Template version**: v1.2.0

Work through these steps in order. Check off each item as you complete it.
Re-run `/adopt` anytime to check remaining gaps.

**Principle: migration, not replacement.** This project already has a working process: the
fresh-clone gate, CI, the feature → revival → dev → master branch model, `CLAUDE.md`,
`docs/L1-LOCAL-PLAYABILITY.md` and `docs/QA-LOG.md`. Each step below brings existing knowledge into
the suite's formats so its skills can read it. Nothing existing is rewritten or removed, and every
step still goes through a feature branch, the gate and a PR.

---

## Step 1: Fix Blocking Gaps

None. The one blocker, an unconfigured engine whose reference pointed at Unity 6.3, was fixed on
2026-10-26: `project.yaml` engine/specialists/naming/commands, `technical-preferences.md`,
`docs/engine-reference/unity/VERSION.md` pinned to 2017.3, and the `CLAUDE.md` import.

- [x] Engine configured and pinned (Unity 2017.3.0f3, C# 4)

---

## Step 2: Fix High-Priority Gaps

### 2a. Game concept (`design/gdd/game-concept.md`)
**Problem**: Concept-level skills (`/design-review`, `/review-all-gdds`, `/create-epics`) look for it; none exists.
**Source material**: `README.md` (the 2018 player manual: two factions, tanks and scouts, cargo, repair pads, PowerCells, SupplyShips) and `CLAUDE.md` "Decisions Already Made" (lean slice first; full economy deferred to M4).
**Fix**: `/design-system retrofit design/gdd/game-concept.md`, built from those sources, with the owner's decisions where the manual and the 2018 code disagree.
**Time**: 1 session
- [x] game-concept.md written and approved (2026-10-26; Visual Identity Anchor: Readable Battlefield; review APPROVED)

### 2b. Systems index (`design/gdd/systems-index.md`)
**Problem**: There's no map of the systems that exist; GDD and story skills key off it.
**Source material**: L1 rows 1–20. Implemented systems: movement/hover, AutoCannon, pulse cannon, fuel, health/regen, targeting, death/respawn and repair pads, self-destruct, cargo/deploy, turrets, teams, HUD, offline play, Photon networking.
**Fix**: `/map-systems`
**Time**: 1 session
- [x] systems-index.md written and approved (2026-10-26; 26 systems)

### 2c. Authoritative project stage
**Fix**: `/gate-check`, one gate at a time. Correction: the stages track design artifacts, not code, so the honest stage is reached gate by gate (Production would need GDDs, ADRs and stories first).
**Time**: 5 min
- [x] `project.stage` = Systems Design (Concept gate PASS 2026-10-26); next gate, Technical Setup, needs the system GDDs (Step 4a)

---

## Step 3: Bootstrap Infrastructure

### 3a. Register existing requirements (creates tr-registry.yaml)
Run `/architecture-review`. It bootstraps the TR registry from the GDDs and ADRs (after Steps 2 and 4b).
**Time**: 1 session
- [x] tr-registry.yaml created (28 TR-IDs; architecture reviews #1 FAIL → fixed, #2 CONCERNS with no blockers)

### 3b. Create control manifest
Run `/create-control-manifest`
**Time**: 30 min
- [x] docs/architecture/control-manifest.md created (from ADR-0001..0005)

### 3c. Create sprint tracking file
Run `/sprint-plan new`. The first sprint is the rest of L1: owner playtest rows 3/4/13/17/18, the three design calls, and the cargo-name fix.
**Time**: 15 min
- [ ] production/sprint-status.yaml created

### 3d. Set authoritative project stage
Covered by 2c.

---

## Step 4: Medium-Priority Gaps

### 4a. Per-system GDDs, retrofitted
`/design-system retrofit design/gdd/<system>.md` for each system in the index. Write the ones
with open design questions first: self-destruct (friendly fire), turrets/flak (one-shot splash),
cargo (what `BlueCargo` deploys).
- [x] GDDs for the systems with open design calls (self-destruct, turrets, cargo: #42, approved 2026-10-26)
- [ ] GDDs for the remaining systems: the first sprint's five done 2026-10-27 (hover-movement, camera-cursor, deploy, hud, audio; owner's choice to retrofit only what the sprint touches); the rest as work reaches them

### 4b. ADRs for decisions already made
`/architecture-decision retrofit` for each decision recorded in `CLAUDE.md`:
- [x] Stay on Unity 2017.3 until a deliberate engine step — ADR-0001 (Accepted)
- [x] PUN Classic 1.88 on Photon Cloud (PUN2 deferred); App ID kept out of git — ADR-0002 (Accepted)
- [x] Local first: L1 before more online work — in ADR-0005 (an ordering rule, not architecture)
- [x] Branch model and merge policy (merge commits, fresh-clone gate, protected dev/master) — ADR-0005 (Accepted)
- [x] Team comparison through `Unit.SameTeam`; damage only through the master (`TellServerTakeDamage`) — ADR-0003 (Accepted)
- [x] Added: match flow (as built in #33, C1 fix #36) — ADR-0004 (Accepted 2026-10-26)

### 4c. L1 as an epic with stories
`/create-epics` and `/create-stories` from `docs/L1-LOCAL-PLAYABILITY.md`. Its 20 rows are already
story-shaped; proven rows become done stories that point to their gate evidence in `docs/QA-LOG.md`.
- [ ] L1 epic + stories

---

## Step 5: Low-Priority Gaps

- [ ] Performance budgets in `technical-preferences.md` (none set; the 2018 game has none)
- [ ] `naming.constants` is marked UNVERIFIED: confirm a convention, or leave it as is

---

## Kept as-is (no migration needed)

`CLAUDE.md` (project facts), `docs/QA-LOG.md` (gate evidence), `Tools/verify-local.sh` (the gate;
`commands.test` points at it), `.github/workflows/checks.yml` (CI), `REVIVAL.md` (the story so far).
