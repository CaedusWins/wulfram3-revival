<!-- markdownlint-disable-file MD041 -->
## What this changes

<!-- One or two sentences. Link the milestone (M1/M2/...) or the CLAUDE.md item this closes. -->

## Verification (all required before merging upward — see CLAUDE.md → "QA & Merge Process")

- [ ] `checks` workflow is green on this PR (C# 4.0 guard, script identity guard, secret scan, required scenes)
- [ ] Local Unity batch-mode compile: **0 errors** (`Compilation succeeded`, `Exiting batchmode successfully`)
- [ ] `WulframSceneCheck.CheckBuildScenes`: **0 missing script references** in `Launcher 1.unity` and `Playground.unity`
- [ ] Built player offline smoke test: `Wulfram3.exe -batchmode -offlineSmokeTest` prints **`SMOKE: PASS`** (Playground loads, player spawns, 0 missing scripts, 0 exceptions)
- [ ] No credentials, tokens, or private endpoints added (env vars only)
- [ ] `CLAUDE.md` / `REVIVAL.md` updated if this changes state, decisions, or blockers

### Additional proof required for this target branch

- **→ `revival/phase-0-1-bringup`:** the checklist above is sufficient.
- **→ `dev`:** also attach the milestone's end-to-end proof (M1: opens/compiles/scenes clean; M2: two clients in one Photon room, damage lands both ways).
- **→ `master`:** also confirm `dev` has been exercised by real play (closed playtest, M3) with no open blockers.

## Risk / rollback

<!-- What could this break, and how would we revert? "Revert this PR" is a fine answer if true. -->
