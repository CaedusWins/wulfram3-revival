# Wulfram3 Revival — Progress Log

This is the **human-readable narrative** of the revival effort: what's happened, in order, and where things stand. It's meant for a person to skim and get oriented.

- For gameplay/how-to-play (the original 2018 manual, untouched), see [README.md](README.md).
- For the dense technical resume doc — exact decisions, file paths, blockers, working preferences, branch model — see [CLAUDE.md](CLAUDE.md). That file is the authoritative source of truth if this one and it ever disagree.

## Where things stand right now

Nothing has been opened in a real Unity Editor yet. That's the single next step. Everything below is real (forked, branched, committed, pushed) — but unverified against an actual compile until then.

## Timeline

- **2018-01-03** — Original community effort stalls at "1.6 alpha" after the last commit (`21f284f`). Repo goes dormant for ~8 years.
- **2026-08-19** (`1d76eb0`) — Revival begins. Repo forked to `github.com/CaedusWins/wulfram3`, branch model established (`master` / `dev` / `revival/phase-0-1-bringup`), all pushed to GitHub. First real fix landed: the client no longer hangs on the dead `wulfram.com:1337` login backend — it synthesizes a local guest player instead (`OfflineMode`). Also pulled a live Discord webhook URL that had been hardcoded in source, and deleted a couple of confirmed-dead files (`HostGame.cs`, a stray `UnityPlayer.dll`).
- **2026-08-23** (`801123f`) — Scope and engine decisions made explicit (lean slice first, stay on Unity 2017.3.0f3 until a compile is proven, git history bloat cleanup deferred). Milestone runway defined (M0–M4). `CLAUDE.md` created so any future session — human or AI — can resume without re-deriving all of this.
- **2026-08-25** (`3e3c127`, `320d21c`) — Outstanding work split into four independent feature branches (scene/build-settings fix, Photon PUN2 migration, accountless quickplay, git history cleanup) instead of piling onto one branch. This file (`REVIVAL.md`) added.
- **2026-08-25 (later same day)** — Verified the whole repo/branch/doc setup against ground truth in a fresh resume, found and closed one gap (2 commits + all 4 feature branches existed locally only) by pushing everything to GitHub — `origin` now fully mirrors local across all 7 branches. Attempted an unattended Unity `2017.3.0f3` install via Unity Hub's CLI; it didn't work in this environment (see `CLAUDE.md` for why).
- **2026-08-29** — Unity `2017.3.0f3` successfully installed via a direct download from Unity's official CDN instead of Unity Hub's CLI. It landed at `C:\Program Files\Unity\Editor\Unity.exe` rather than the Hub-managed path (a shell quoting issue, not a Unity problem — see `CLAUDE.md`), but it's fully functional. The install blocker is cleared; M1 itself (actually opening and compiling the project) has not started yet.

## What's next

Open the project in Unity `2017.3.0f3` (`C:\Program Files\Unity\Editor\Unity.exe` — do **not** use the Unity 6 install, see `CLAUDE.md`) for the first time, and start on `feature/m1-scene-build-settings`. That's the M1 milestone: prove the project actually opens and compiles. Everything else (Photon migration, quickplay mode, history cleanup) is scoped and waiting on its own branch but not blocking.

## Maintenance note

Update this alongside `CLAUDE.md` whenever a milestone lands or a branch merges — keep the timeline appended, don't rewrite history.
