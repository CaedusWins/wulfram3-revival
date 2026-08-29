# Wulfram3 Revival — Project Context

This file is the living state of the Wulfram revival project. It's meant to be maintained continuously as work progresses, and to be enough on its own for a new session (human or Claude) to pick up exactly where things left off if this conversation is ever lost. If you're an AI assistant reading this cold: read this whole file before doing anything else in this repo.

**Companion document:** a designed, visual version of the roadmap/runway lives at https://claude.ai/code/artifact/3b6df1dc-d101-4071-9489-7efd2750ba1a — nice for a human to read, but this file is the source of truth for resuming work.

**Doc map:** three files, three jobs — don't blend them.
- `CLAUDE.md` (this file) — dense, technical, AI-resume source of truth. Decisions, blockers, exact paths.
- `REVIVAL.md` — human-readable narrative/timeline of the revival effort. Read this for the story; read this file for the facts.
- `README.md` — the original 2018 player manual (how to play), untouched except for a pointer at the top to the other two.

**Immediate next action:** M1 (opens & compiles) is done and verified — see Milestone Runway below. What's left on `feature/m1-scene-build-settings` is the actual Build Settings fix: add `Assets/Scenes/Playground.unity` to `EditorBuildSettings.asset`, drop the vendored MHLab `Launcher.unity` demo scene, resolve the duplicate `Launcher.unity` vs `Launcher 1.unity`. Now that Unity CLI access works (see Local Environment), this can likely be done via a custom `-executeMethod` editor script instead of requiring the GUI — worth trying that route first.

## What This Project Is

Two unrelated things share the name "Wulfram" — don't conflate them:

1. **Wulfram II (2002)** — the real, defunct game this whole effort is trying to revive. Built on a proprietary/custom engine, originated as "Shockforce" under Total Entertainment Network in the late 1990s, released independently by Bernt Habermeier's Slurpysoft in 2002. Predates Unity entirely. No source code exists anywhere; it's gone for good.
2. **This repo (`wulfram3`)** — a fan-made *recreation* of Wulfram II, built from scratch in Unity by a small community (2016–2018), unrelated technically to the original game's code. Got to "1.6 alpha" and stalled in January 2018. This is what we're reviving.

## Repo / Fork Topology

- **Upstream (original team, dead since Jan 2018):** `github.com/Wulfram3/wulfram3` — 7 stale open issues, last code push Jan 2018.
- **Sibling repo in that org, unexpectedly active:** `github.com/Wulfram3/wulfram-tools` — created Jan 2026, pushed Mar 2026, then quiet. A Python toolkit that reverse-engineers the *original* Wulfram II's binary asset formats (3D shapes, paletted textures) from a real game install, with a prebuilt Unity import bridge (animated sprite shader + editor auto-importer). Sole author: GitHub user `d4rksh4de` ("Steve") — the same person who did 86 commits on the original 2018 `wulfram3` effort.
  - **Status: parked.** We don't have an original Wulfram II install to extract from, and we decided not to reach out to Steve — building independently. Revisit only if original game files ever surface.
- **Your fork (this repo):** `github.com/CaedusWins/wulfram3-revival` (renamed from `wulfram3` for clarity — fork lineage to `Wulfram3/wulfram3` is tracked by ID on GitHub's side and survived the rename intact) — local path `c:\Development\Wulfram_Development\wulfram3` (local folder name unchanged, only the GitHub repo name changed).
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
- **Unity:** two editors now present.
  - `6000.1.6f1` — Hub-managed, at `C:\Program Files\Unity\Hub\Editor\6000.1.6f1`. Do not open this project with it — see the engine-fork decision above.
  - `2017.3.0f3` — the one this project needs. Installed via a direct download from Unity's official CDN (`https://download.unity3d.com/download_unity/a9f86dcd79df/Windows64EditorInstaller/UnitySetup64-2017.3.0f3.exe`, changeset `a9f86dcd79df`) after Unity Hub's `--headless` CLI proved unworkable (see below). **It landed at `C:\Program Files\Unity\Editor\Unity.exe`, not the Hub-managed path** — an unquoted Windows path with spaces got mangled crossing from bash into the native NSIS installer, so it silently installed to its compiled-in default location instead of `Hub\Editor\2017.3.0f3`. It's fully functional (correct file sizes/dates for this version); Unity Hub's GUI just won't list it since it's outside Hub's managed folder convention. Launch it directly by path, or with `-projectPath` for CLI/batch-mode work, rather than through Hub.
  - Unity Hub's `--headless install` CLI does not work in this environment — `--headless`/`--version` fall through to generic Electron/Node dispatch instead of Unity Hub's own CLI handler. Don't re-attempt it; use the direct-download approach above (Unity's public release API at `https://services.api.unity.com/unity/editor/release/v1/releases?version=<X>` returns the exact installer URL and changeset for any version) if another version is ever needed.
- **Git identity:** Thomas J. Purdy Jr. (`caedus420@gmail.com`).
- **Remote:** `origin` → `https://github.com/CaedusWins/wulfram3-revival.git` (fetch and push) — updated after the repo rename below; the old `wulfram3.git` URL still redirects via GitHub for now, but don't rely on that indefinitely.
- **`gh` CLI:** installed and already authenticated as `CaedusWins` with `repo` scope — usable for repo-settings changes (default branch, description, etc.), not just git push/pull.
- **GitHub default branch:** changed from `master` to `revival/phase-0-1-bringup` (via `gh api -X PATCH`), specifically so visiting the repo shows active work instead of the untouched original — landing on `master` by default made it look like nothing had happened. Repo description also updated to point at `REVIVAL.md`/`CLAUDE.md` and note `master` is the preserved original.
- **GitHub repo renamed** `wulfram3` → `wulfram3-revival` (via `gh repo rename`), for the same reason — the name itself now signals "this is the revival," not just the description. Fork relationship to `Wulfram3/wulfram3` confirmed intact after rename (GitHub tracks it by internal ID, not name). Local remote URL updated to match.
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
- **M1 — Opens & Compiles: DONE, VERIFIED.** Ran the project through Unity `2017.3.0f3` in batch mode (`-batchmode -quit -nographics -projectPath ... -logFile ...`) twice. First run surfaced 2 real compile errors (see below); second run after fixing them: **all 4 assemblies "Compilation succeeded" (88 warnings, 0 errors), clean `Exiting batchmode successfully now!` exit.** This is the first empirically verified fact in the whole project — everything before this milestone was static analysis. Do NOT open this project in the Unity 6 install — that forces an unplanned, hard-to-reverse project upgrade attempt, which is exactly the engine decision we're deliberately deferring.
  - **Bug caught by the first compile attempt:** the offline-mode fix in `UserController.cs` used C#'s `?.` null-conditional operator, but this project's legacy Mono compiler is capped at C# 4.0 (predates that operator by two language versions — `error CS1644`). Fixed by replacing `Foo?.Invoke(...)` with `if (Foo != null) Foo(...)`. Lesson: don't use C# 5+/6+ syntax anywhere in this codebase — no `?.`, `??` (verify), string interpolation (`$"..."`), `nameof()`, etc. — without confirming it compiles.
  - **Bonus cleanup:** Unity's first import auto-deleted ~35 orphaned `.meta` files for folders that no longer exist on disk (e.g. a `Camera` folder under `Assets/Scripts/` that was long gone) — this is exactly the "Assets/Scripts/* stub" disorganization flagged in the original repo scan, now resolved as a side effect of actually opening the project. Committed separately from the C# fix.
  - **How to re-run this compile check yourself:** `"C:\Program Files\Unity\Editor\Unity.exe" -batchmode -quit -nographics -projectPath "C:\Development\Wulfram_Development\wulfram3" -logFile <path>`, then grep the log for `error CS`, `Compilation failed`/`Compilation succeeded`, and `Exiting batchmode successfully` — don't trust the process exit code alone, it can be 0 even with compile errors present in the log.
- **M2 — First Local Match:** two Unity instances join the same Photon room, both spawn into `Playground`, damage lands both ways, all without `wulfram.com` or any external login server.
- **M3 — Public Alpha:** real Photon backend, closed playtest, repo clean enough for a second contributor.
- **M4 — Full Economy (optional):** SupplyShips, SkyPumps, PowerCells, UpLink. Only if the scope decision above is revisited.

## What's Already Fixed

On `revival/phase-0-1-bringup` (commit `1d76eb0` unless noted):
- **`Assets/InternalApis/Implementations/UserController.cs`** — added `public static bool OfflineMode = true` (the `wulfram.com:1337` socket.io backend is dead). `LoginUser`/`RegisterUser` now synthesize a local guest player instead of hanging on a dead connection. Also fixed a pre-existing bug where `GetUsername()` compared the username against the literal string `"null"` instead of testing for a real null/empty value — that bug produced a null player name on every launch regardless of backend status, online or off.
- **`Assets/InternalApis/Implementations/DiscordApi.cs`** — removed a hardcoded, live Discord webhook URL that was committed to source. Now reads `WULFRAM_DISCORD_WEBHOOK_URL` from the environment and silently no-ops if unset. **Still needed from the user:** the old webhook should be revoked/regenerated on Discord's side — removing it from source code doesn't invalidate the credential itself.
- **Deleted `Assets/HostGame.cs`** — dead UNET matchmaking leftover, confirmed unreferenced, unrelated to the actual Photon networking path.
- **Deleted root-level `UnityPlayer.dll`** — stray 22MB binary sitting outside `Assets/` and outside `windows/`, not referenced by the project.

On `feature/m1-scene-build-settings` (commits `5400b5a`, `b3e4717`):
- **Fixed the C# 4.0 incompatibility** in `UserController.cs` that the first real compile attempt caught (`?.` → explicit null checks — see M1 in Milestone Runway for detail).
- **Removed ~35 orphaned `.meta` files** for ghost folders, auto-flagged by Unity's own asset database on first import.
- **Fixed `EditorBuildSettings.asset`** via a new permanent editor utility, `Assets/Editor/WulframBuildSettings.cs` (`SetCanonicalScenes()`, runnable through `-executeMethod`). **Correction to earlier notes:** `Playground.unity` was never actually missing from Build Settings — that claim traced back to the original repo scan and was never re-verified against the real binary asset until this script printed the actual contents. What was actually wrong: the canonical `Assets/Scenes/Launcher.unity` was **disabled**, while a duplicate, `Launcher 1.unity`, was the one enabled/active — meaning a build would have launched into the wrong launcher scene. Fixed to a clean 2-entry list: `Launcher.unity` + `Playground.unity`, both enabled. The MHLab PATCH demo scene and the `Launcher 1.unity` duplicate are excluded from Build Settings now (files themselves untouched, not deleted).

## What's Still Broken / Not Yet Done

- **17 objects in Playground.unity need the `Cargo` component re-attached — in progress, stuck on a real quirk.** Full story: `WulframSceneCheck` found 18 missing script references in `Playground.unity` (0 in `Launcher.unity`). 16 were on objects named `Cargo`/`Cargo (N)`, all broken at the same component slot; the 17th pattern-matched too (plain `Cargo`); the 18th (`RedBase/RML/Turret_SAM`) matched no class anywhere in the codebase.
  - **Phase 1 (removing the broken slots): DONE, verified, committed** (`24b8448`, on `feature/m1-scene-build-settings`). Root cause of an early false-start: the affected objects were `PrefabType.PrefabInstance`, and Unity's (pre-2018.3) prefab override system was silently reverting raw `SerializedObject` structural edits on save unless the instance was disconnected first (`PrefabUtility.DisconnectPrefabInstance`) *before* creating any `SerializedObject` for it — creating the `SerializedObject` first, then disconnecting, left it referencing a stale pre-disconnect snapshot, which was the actual bug in the very first attempt. Fixed, independently re-verified twice in fresh processes: 0 missing scripts in either scene.
  - **Phase 2 (re-attaching `Cargo` to the 17 objects): NOT done, uncommitted, reverted back to the Phase 1 checkpoint each time. Conclusion: finish this by hand in the Unity GUI, not via more CLI automation.** `Com.Wulfram3.Cargo` (`Assets/BlueFiles/cargo.cs`) is confirmed the correct class to attach — `GameManager.cs` calls `GetComponent<Cargo>()` on these objects directly, and it's a real, currently-compiling class. The problem the whole time was persistence, not correctness, and four different automated approaches were tried, each independently re-verified in fresh processes rather than trusted on the log's own claims:
    1. All 17 in one process, one `SaveScene` at the end, no dirty-marking: **1 of 17** persisted.
    2. Same, with `EditorUtility.SetDirty`/`EditorSceneManager.MarkSceneDirty` added per object: **8 of 17**.
    3. One separate Unity CLI process per object (17 launches, each doing the minimum possible work): **9 of 17** — ruling out in-process batching as the cause, since these were now fully isolated processes.
    4. Same, plus `AssetDatabase.ImportAsset(path, ForceUpdate)` before opening the scene and a delay between launches (testing a filesystem-cache-staleness theory): **10 of 17** on the first pass — then a **retry round on just the 7 still-failing objects made things worse, not better** (missing-script count went from 7 up to 12), meaning this specific mitigation appears able to actively regress previously-successful fixes on other objects, not just fail to add new ones.
    - Which objects succeed doesn't correlate with name, numeric suffix, or processing order in any run - the same handful (`Blue FT Cargo`, `Red PC Cargo`, `Cargo (3)`, `RP Cargo`, `Cargo (12)`, `Cargo (14)`) succeeded consistently across multiple different attempts, which argues against pure randomness, but no explanation for *why* those specific ones was found. Given attempt 4 actively regressed prior progress, **stop here — do not keep iterating on CLI-only fixes for this specific step.**
    - The forced-reimport code from attempt 4 has been removed from `WulframFixMissingScripts.cs` (left as a commented warning, not deleted silently) so it isn't accidentally reused.
    - **To finish:** open the project normally in the Unity Editor (not batch mode), open `Playground.unity`, and for each of the 17 objects (`Blue FT Cargo`, `Cargo`, `RP Cargo`, `Cargo (3)`, `Red PC Cargo`, `Cargo (5)` through `Cargo (16)`) drag `Assets/BlueFiles/cargo.cs` onto it in the Inspector (or Add Component → search "Cargo"), then save the scene. `WulframSceneCheck.CheckBuildScenes` (via `-executeMethod`) is still there afterward to independently confirm 0 missing scripts remain, and `WulframSceneCheck.CountCargoComponents` to confirm exactly 17 (not more, not fewer).
  - `Assets/Editor/WulframSceneCheck.cs`, `WulframFixMissingScripts.cs`, `WulframPrefabCheck.cs` are all committed and reusable for whichever approach comes next — no need to rewrite them, just call their methods differently (per-object, or not at all if going the manual GUI route).
- **Confirm the fixed Build Settings actually work end to end** — the fix itself is verified (before/after logged, zero new compile errors), but nobody has pressed Play or done a build yet to confirm the game boots into the right launcher and loads Playground correctly at runtime.
- **Photon networking** — `PhotonServerSettings.asset` points at a dead AWS IP (`18.218.55.176`), no working App ID exists. Needs a fresh Photon Cloud app, and a PUN Classic → PUN2 migration (use Photon's official converter tool, don't hand-port).
- **No real account/stats backend** — `OfflineMode` is a stopgap for local testing, not a long-term replacement for the dead `wulfram.com:1337` service. Current leaning: ship an accountless "quick play" mode first rather than building a replacement backend immediately.

## Working Preferences

- Hands-off for routine local dev/git work (branch creation, commits, file edits) — don't ask permission for each step.
- Confirm before anything that leaves the machine: pushing to GitHub, merging into `dev` or `master`.
- Push feature-branch work to GitHub at reasonable checkpoints as we go.
- (This preference is also saved in Claude's cross-session memory for this project folder, but restate it if starting a session rooted somewhere else.)

## Related But Separate Project — Do Not Conflate

There is a second, entirely unrelated project: a WordPress prototype site for "Pine Liquors" (a real business in Pine, Colorado) at `c:\Development\web-dev-exploration`. Different repo, different git history, different session. It is not part of the Wulfram revival and shares nothing technically — mentioned here only so it's never confused with this project.

## Maintenance Note

Update this file whenever a milestone is hit, a decision is made or changed, or a new fact is discovered about the project (upstream activity, new blockers, etc.) — the goal is that this file is never more than one work session out of date.
