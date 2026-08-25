# Wulfram3 Revival — Project Context

This file is the living state of the Wulfram revival project. It's meant to be maintained continuously as work progresses, and to be enough on its own for a new session (human or Claude) to pick up exactly where things left off if this conversation is ever lost. If you're an AI assistant reading this cold: read this whole file before doing anything else in this repo.

**Companion document:** a designed, visual version of the roadmap/runway lives at https://claude.ai/code/artifact/3b6df1dc-d101-4071-9489-7efd2750ba1a — nice for a human to read, but this file is the source of truth for resuming work.

**Doc map:** three files, three jobs — don't blend them.
- `CLAUDE.md` (this file) — dense, technical, AI-resume source of truth. Decisions, blockers, exact paths.
- `REVIVAL.md` — human-readable narrative/timeline of the revival effort. Read this for the story; read this file for the facts.
- `README.md` — the original 2018 player manual (how to play), untouched except for a pointer at the top to the other two.

**Immediate next action:** install Unity `2017.3.0f3` (Unity Hub → Installs → Archive), open this project for the first time, then work `feature/m1-scene-build-settings`. Nothing else is unblocked until that happens. **Note:** an attempt to install it unattended via Unity Hub's `--headless` CLI failed in this environment — the flags fell through to generic Electron/Node dispatch instead of Unity Hub's own CLI handler (e.g. `--version` alone printed Electron's bundled Node version, not Unity Hub's). Don't re-attempt that path without a new idea; do it manually via the Unity Hub GUI.

## What This Project Is

Two unrelated things share the name "Wulfram" — don't conflate them:

1. **Wulfram II (2002)** — the real, defunct game this whole effort is trying to revive. Built on a proprietary/custom engine, originated as "Shockforce" under Total Entertainment Network in the late 1990s, released independently by Bernt Habermeier's Slurpysoft in 2002. Predates Unity entirely. No source code exists anywhere; it's gone for good.
2. **This repo (`wulfram3`)** — a fan-made *recreation* of Wulfram II, built from scratch in Unity by a small community (2016–2018), unrelated technically to the original game's code. Got to "1.6 alpha" and stalled in January 2018. This is what we're reviving.

## Repo / Fork Topology

- **Upstream (original team, dead since Jan 2018):** `github.com/Wulfram3/wulfram3` — 7 stale open issues, last code push Jan 2018.
- **Sibling repo in that org, unexpectedly active:** `github.com/Wulfram3/wulfram-tools` — created Jan 2026, pushed Mar 2026, then quiet. A Python toolkit that reverse-engineers the *original* Wulfram II's binary asset formats (3D shapes, paletted textures) from a real game install, with a prebuilt Unity import bridge (animated sprite shader + editor auto-importer). Sole author: GitHub user `d4rksh4de` ("Steve") — the same person who did 86 commits on the original 2018 `wulfram3` effort.
  - **Status: parked.** We don't have an original Wulfram II install to extract from, and we decided not to reach out to Steve — building independently. Revisit only if original game files ever surface.
- **Your fork (this repo):** `github.com/CaedusWins/wulfram3` — local path `c:\Development\Wulfram_Development\wulfram3`.
- **Unmerged upstream branch worth knowing about:** `Wulfram3/wulfram3` has a branch `Wulfram-Alpha-v1.6.0` with 6 commits never merged to master — chat, auto-login-to-chat, player-database fields, red-powercell cargo detection, PC zoning, fuel balancing. Not yet pulled into this fork. Worth cherry-picking later.

## Branch Model (set up and pushed to GitHub)

- **`master`** = prod. Stable, deployable, currently untouched at the original `21f284f` ("fixed size of gui", Jan 2018). Nothing merges here without explicit sign-off.
- **`dev`** = integration/testing. Currently just mirrors `master` — nothing merged in yet, waiting on M1 (see below) before the feature branch merges in.
- **`revival/phase-0-1-bringup`** = active integration branch for the revival effort, pushed to origin. Feature branches (below) merge back into this one; this is what eventually merges into `dev`.

**Working style:** create branches/commit/edit files locally without asking each time. Confirm before pushing to GitHub or merging into `dev`/`master` — but push feature-branch work at reasonable checkpoints (not after every micro-edit) since "publish as we go" has been explicitly requested.

### Feature branches (created off `revival/phase-0-1-bringup`, pushed to origin)

One branch per outstanding work item from "What's Still Broken" below, so each can be picked up and merged independently instead of piling everything onto one branch:

- **`feature/m1-scene-build-settings`** — the M1 blocker: fix `EditorBuildSettings.asset` (add `Playground.unity`, drop the vendored MHLab `Launcher.unity` demo scene, resolve duplicate `Launcher.unity` vs `Launcher 1.unity`). Must be done inside a real Unity 2017.3.0f3 editor, not by hand-editing the binary asset.
- **`feature/m2-photon-pun2-migration`** — replace the dead Photon AWS endpoint/App ID with a fresh Photon Cloud app, migrate PUN Classic → PUN2 via Photon's official converter tool.
- **`feature/accountless-quickplay`** — ship an accountless "quick play" mode as the long-term answer to the dead `wulfram.com:1337` backend (current leaning per the "No real account/stats backend" note below), rather than building a replacement backend.
- **`feature/git-history-cleanup`** — rewrite history to drop the ~2.5GB of committed `windows/` build binaries and old WebGL builds. Deferred/low priority per the decision below; branch exists so the work is scoped whenever it's picked up.

**Isolation caveat — read before working two branches "at once":** these branches isolate *history* from each other (commits on one don't touch another until merged), but a single working directory can only have one branch checked out at a time — checking out a branch overwrites the working tree with that branch's files. That's a real problem specifically for `feature/m1-scene-build-settings`, because it requires the Unity Editor to have the project open, and Unity regenerates its `Library/` cache on every branch switch (slow, and occasionally flaky). If you want to genuinely work M1 in the Unity Editor while also editing code for another feature branch at the same time, use `git worktree add ../wulfram3-m1 feature/m1-scene-build-settings` (and similarly for others) to get separate working directories on the same repo/history instead of switching branches in place. Not set up yet — ask if you want it.

## Local Environment

- **OS:** Windows 11. Repo lives at `c:\Development\Wulfram_Development\wulfram3`.
- **Unity:** Unity Hub installed at `C:\Program Files\Unity Hub`; only editor version currently installed is `6000.1.6f1` (`C:\Program Files\Unity\Hub\Editor\6000.1.6f1`) — **not** the `2017.3.0f3` this project needs. Must be installed via Unity Hub → Installs → Archive before M1 can start.
- **Git identity:** Thomas J. Purdy Jr. (`caedus420@gmail.com`).
- **Remote:** `origin` → `https://github.com/CaedusWins/wulfram3.git` (fetch and push).
- **`WULFRAM_DISCORD_WEBHOOK_URL`:** referenced by `DiscordApi.cs` (see "What's Already Fixed" below); whether it's set in this machine's environment hasn't been verified this session — check before relying on Discord integration working locally.

## Session Transcript Locations (fallback only)

This file is the intended resume mechanism and should be sufficient on its own. If something is ever missing from it, the raw conversation history is preserved locally and can be searched or replayed:
- Transcripts: `C:\Users\Caedu\.claude\projects\c--Development-Wulfram-Development\*.jsonl` (one file per past session, newest reflect the most recent work).
- Full replay: `claude --resume <session-id>` run from this repo's directory.
- As of 2026-08-25, three sessions exist there, spanning 2026-08-19 through 2026-08-25 — covering the fork/branch setup, the offline-bringup fix, the `CLAUDE.md` creation, and the feature-branch scaffolding documented above.

## Decisions Already Made (don't re-litigate these without new information)

- **Scope: lean slice first.** Ship what's already in the repo (team tank/scout combat) rather than building the README's full persistent-world economy (SupplyShips, SkyPumps, PowerCells, UpLink strategy layer) up front. The full economy is deferred to M4, not cancelled.
- **Engine: stay on Unity 2017.3.0f3** until M1 (below) proves the project actually compiles. No engine upgrade decision can be made honestly before that — upgrading blind on an unverified codebase is a guess, not a plan.
- **Git history bloat (~2.5GB from committed `windows/` build binaries and old WebGL builds):** not rewritten yet. Deferred — not urgent while it's just one person working on the fork.

## Milestone Runway

- **M0 — Repo Untangled: DONE.** Fork confirmed, branch model set up (master/dev/revival branch), all three pushed to GitHub.
- **M1 — Opens & Compiles: NEXT, NOT DONE.** Nobody has opened this project in a real Unity Editor yet. Everything documented here is static analysis, not a confirmed compile. **Blocker:** only Unity `6000.1.6f1` is installed on the dev machine, not `2017.3.0f3`. Do NOT open this project in Unity 6 — that forces an unplanned, hard-to-reverse project upgrade attempt, which is exactly the engine decision we're deliberately deferring. Install Unity `2017.3.0f3` via Unity Hub → Installs → Archive first.
- **M2 — First Local Match:** two Unity instances join the same Photon room, both spawn into `Playground`, damage lands both ways, all without `wulfram.com` or any external login server.
- **M3 — Public Alpha:** real Photon backend, closed playtest, repo clean enough for a second contributor.
- **M4 — Full Economy (optional):** SupplyShips, SkyPumps, PowerCells, UpLink. Only if the scope decision above is revisited.

## What's Already Fixed (on `revival/phase-0-1-bringup`, commit `1d76eb0`)

- **`Assets/InternalApis/Implementations/UserController.cs`** — added `public static bool OfflineMode = true` (the `wulfram.com:1337` socket.io backend is dead). `LoginUser`/`RegisterUser` now synthesize a local guest player instead of hanging on a dead connection. Also fixed a pre-existing bug where `GetUsername()` compared the username against the literal string `"null"` instead of testing for a real null/empty value — that bug produced a null player name on every launch regardless of backend status, online or off.
- **`Assets/InternalApis/Implementations/DiscordApi.cs`** — removed a hardcoded, live Discord webhook URL that was committed to source. Now reads `WULFRAM_DISCORD_WEBHOOK_URL` from the environment and silently no-ops if unset. **Still needed from the user:** the old webhook should be revoked/regenerated on Discord's side — removing it from source code doesn't invalidate the credential itself.
- **Deleted `Assets/HostGame.cs`** — dead UNET matchmaking leftover, confirmed unreferenced, unrelated to the actual Photon networking path.
- **Deleted root-level `UnityPlayer.dll`** — stray 22MB binary sitting outside `Assets/` and outside `windows/`, not referenced by the project.

## What's Still Broken / Not Yet Done

- **`ProjectSettings/EditorBuildSettings.asset`** — `Assets/Scenes/Playground.unity` (the actual arena `GameManager.cs` loads at runtime) is still missing from Build Settings. This file is Unity's binary serialization format, not text/YAML — do not hand-edit bytes; it needs to be fixed inside a real Unity 2017.3.0f3 editor. Also needs: removing the vendored `Assets/MHLab/PATCH/.../Launcher.unity` demo scene from the build list, and resolving the duplicate `Assets/Scenes/Launcher.unity` vs `Launcher 1.unity`.
- **Photon networking** — `PhotonServerSettings.asset` points at a dead AWS IP (`18.218.55.176`), no working App ID exists. Needs a fresh Photon Cloud app, and a PUN Classic → PUN2 migration (use Photon's official converter tool, don't hand-port).
- **No real account/stats backend** — `OfflineMode` is a stopgap for local testing, not a long-term replacement for the dead `wulfram.com:1337` service. Current leaning: ship an accountless "quick play" mode first rather than building a replacement backend immediately.
- **Nothing has been compiled or run yet.** See M1 above — this is the single biggest open unknown in the whole project.

## Working Preferences

- Hands-off for routine local dev/git work (branch creation, commits, file edits) — don't ask permission for each step.
- Confirm before anything that leaves the machine: pushing to GitHub, merging into `dev` or `master`.
- Push feature-branch work to GitHub at reasonable checkpoints as we go.
- (This preference is also saved in Claude's cross-session memory for this project folder, but restate it if starting a session rooted somewhere else.)

## Related But Separate Project — Do Not Conflate

There is a second, entirely unrelated project: a WordPress prototype site for "Pine Liquors" (a real business in Pine, Colorado) at `c:\Development\web-dev-exploration`. Different repo, different git history, different session. It is not part of the Wulfram revival and shares nothing technically — mentioned here only so it's never confused with this project.

## Maintenance Note

Update this file whenever a milestone is hit, a decision is made or changed, or a new fact is discovered about the project (upstream activity, new blockers, etc.) — the goal is that this file is never more than one work session out of date.
