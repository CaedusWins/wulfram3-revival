<!-- markdownlint-disable-file MD041 -->
## What this changes

<!-- One or two sentences. Link the milestone (M1/M2/...) or the CLAUDE.md item this closes. -->

## Verification (all required before merging upward — see CLAUDE.md → "QA & Merge Process")

- [ ] `checks` workflow is green on this PR (C# 4.0 guard, no UnityScript/Boo, script identity guard, `.meta` integrity, secret scan, required scenes)
- [ ] `Tools/verify-local.sh --windowed` prints **`VERIFIED`** on a **fresh clone of this PR's head commit**. It covers: compile with no compiler failure of any kind; 0 missing scripts in `Launcher 1.unity` / `Playground.unity`; 17 Cargo; audit matches `Tools/audit-baseline.txt`; a clean player build; offline smoke test `SMOKE: PASS` headless and windowed (`probePanelShown=True`)
- [ ] Merge with `gh pr merge --merge --match-head-commit <verified sha>` (merge commit; keep the branch)
- [ ] No credentials, tokens, or private endpoints added (env vars only)
- [ ] `CLAUDE.md` / `REVIVAL.md` updated if this changes state, decisions, or blockers

### Additional proof required for this target branch

- **→ `revival/phase-0-1-bringup`:** the checklist above is sufficient.
- **→ `dev`:** also attach the milestone's end-to-end proof (M1: opens/compiles/scenes clean; M2: two clients in one Photon room, damage lands both ways).
- **→ `master`:** also confirm `dev` has been exercised by real play (closed playtest, M3) with no open blockers.

## Risk / rollback

<!-- What could this break, and how would we revert? "Revert this PR" is a fine answer if true. -->
