# QA Log

Every verification run that a merge decision rested on, newest last. **Append new rows; never rewrite old ones.** How each step works is in `CLAUDE.md` → "QA & Merge Process"; what L1 still has to prove is in `docs/L1-LOCAL-PLAYABILITY.md`.

## Where the artifacts are

The logs and screenshots are **not in git**. They hold machine details (Unity's license exchange is written into editor logs), and builds contain the Photon App ID. They are kept on the dev machine, outside the repo:

- `C:\Development\Wulfram_Development\qa-artifacts\gate-runs\<run>\` — each run's step logs, screenshots and `SUMMARY.txt` (the gate's printed verdict). Player builds are left out (≈250 MB each; rebuild from the commit).
- `C:\Development\Wulfram_Development\qa-artifacts\one-off-logs\` — single investigations (builds, editor checks, scene dumps).
- `C:\Development\Wulfram_Development\qa-artifacts\scripts\` — one-off test scripts (`room-full-test.sh`; `pair-test.sh <exe> <delay> <out>` runs one online pair with the second client delayed).
- `C:\Development\Wulfram_Development\qa-artifacts\pair-tests\` — results of `pair-test.sh` series.
- Since `feature/qa-dev-stage`, every gate run writes `<out>/SUMMARY.txt` itself.
- `C:\Development\Wulfram_Development\builds\` — play builds shared with testers (contain the App ID: share privately).

The archive was checked for the full Photon App ID (none: logs print only its first 8 characters).

## Gate runs

All on a **fresh sparse clone of the exact commit**, with `Tools/verify-local.sh`. Steps: 1 compile · 2 scenes · 3 cargo · 4 audit · 5 build · 6 smoke · 6b offline play · 6c online pair (8 s apart) · 6d online pair (same instant; from `feature/join-same-room`) · 7 windowed · 8 editor ×2.

| Date | Run | Branch → PR | Commit gated | Flags | Result | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| 2026-10-10 | `op-gate` | `feature/offline-play` → #3 | `dc5899a` | `--windowed --editor` | VERIFIED (10/10) | merged `873d5fd` |
| 2026-10-10 | `m2-gate` | `feature/m2-photon-cloud` → #5 | `6369a20` | `--online --windowed --editor` | VERIFIED (11/11) | merged `c820d62`, tag `m2` |
| 2026-10-10 | `hc-gate` | `feature/hide-chat-when-off` → #7 | `6df90fe` | `--online --screenshots --editor` | VERIFIED (11/11) | merged `d71be99` |
| 2026-10-10 | `pd-gate` | `feature/playground-dirty` | `e12a615` | `--online --screenshots --editor` | NOT VERIFIED (10/11) | 6c failed: the M2 check demanded "exactly 90" and a regenerated tank read higher. Flaky test, not the branch: fixed by #8, which led to the regen bug |
| 2026-10-10 | `tr-gate` | `feature/m2-test-robust` → #8 | `7982476` | `--online --screenshots --editor` | VERIFIED (11/11) | merged `00ce578` |
| 2026-10-10 | `pd-gate2` | `feature/playground-dirty` → #9 | `b3f6f13` | `--online --screenshots --editor` | VERIFIED (11/11) | merged `ac4b125` |
| 2026-10-16 | `rg-gate` | `feature/regen-all-players` → #11 | `60a8320` | `--online --screenshots --editor` | NOT VERIFIED (10/11), then the failed step **3/3 PASS** on rerun | Step 8 (Launcher 1): Unity's periodic **license refresh** fired during Play mode, so the editor reloaded its scripts ("Reloading assemblies due to reload request"), which tripped `ChatDockUI.OnDisable` (null `ChatUI.Instance`). Environmental. Reruns had 0 license updates, 0 reloads, 0 exceptions (`SUMMARY-editor-reruns.txt`). Merged `bcc0228` |
| 2026-10-16 | `js-neg` | `feature/join-same-room` (test only, no fix) | `e408a5d` | `--online` | NOT VERIFIED (9/11), as intended | Negative test: 6c and 6d failed as predicted (see below). Steps 1–6b passed. **Also found:** in 6c, client B's own smoke FAILED for a reason unrelated to rooms. See "Flaky online canary" below |

## Negative tests (test first, then fix)

| Date | Bug | Test | Before the fix | After the fix |
| --- | --- | --- | --- | --- |
| 2026-10-16 | Only the master's tank regenerated | M2 regen check (`53a1e6e`) | FAIL: master 90→91, other client 90→90 | 3/3 PASS, then `rg-gate` |
| 2026-10-16 | Same-instant Play can split players into two rooms | gate 6c/6d with `-photonRoom` (`e408a5d`) | 6c FAIL: the clients shared a random room (`67839fb1-…`), not the one requested. **6d FAIL: started at the same instant, the two clients landed in two different rooms (`802f736c-…` / `484d819e-…`), each seeing only itself.** The race is real and reproducible | not run yet: the branch is **parked** with the other online work (L1 first) |

| 2026-10-16 | Online canary clients aren't in lockstep (flaky 6c) | `qa-artifacts\scripts\pair-test.sh`, the second client delayed by 15/25 s, on a baseline build of `42525b8` | 15 s: B FAIL (own lowest 91, A already gone); 25 s: PASS. Reproduced | Barrier v1: 6/7 pairs PASS. One new failure: A timed out at barrier 3 because B's "done" and B leaving arrived in the same frame, so A never saw B both present and done. Fixed by latching each player's phase. **Barrier v2: 10/10 pairs PASS** (5 at 15 s; 0 timeouts). Logs: `qa-artifacts\pair-tests\` |

## Open QA findings

- **FIXED on `feature/qa-dev-stage` (see the negative-test row above): flaky online canary (2026-10-16, `js-neg` 6c, client B).** The M2 smoke clients run their phases without waiting for each other. Client A started 8 s earlier, finished, and left. B's own tank had already regenerated from 90 to 91 before B began watching ("own lowest 91"), and by B's regen check, A was gone and B's tank was back at 100 ("100->100 healedWithAllPresent=False"). So B FAILED. The regen fix (#11) exposed this: before it, B's tank never healed, so B always saw 90. **It is test timing, not a game bug**, but it makes 6c/6d flaky. Fix (QA work, `feature/qa-dev-stage`): synchronize the phases (each client marks a phase done with a player custom property, and nobody leaves until every player has finished), and start the lowest-health sample when the local tank spawns, not when both players are seen.

## Lessons the runs taught (kept here so they aren't relearned)

- **Unity's license refresh can force a script reload in the editor.** Seen twice: 2026-10-10 (an early editor-Play investigation, which recovered and passed) and 2026-10-16 (`rg-gate`). It happens about once a day (the log names the next check: "Next license update check is after …"). A reload during Play mode fails step 8. Before blaming the code, check the step-8 log for `License updated successfully` just before `Reloading assemblies due to reload request`.
- **"Exactly N" assertions are flaky in a live game** (regeneration, timing). Assert bounds and the lowest/highest value seen.
- **A fresh clone's first editor open recompiles everything**, sometimes twice. `WulframEditorPlayCheck` waits for the editor to settle before pressing Play; keep it that way.
