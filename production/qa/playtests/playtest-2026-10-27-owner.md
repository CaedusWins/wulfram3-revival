# Playtest Report

> Session sheet for the owner, filled in ahead of time (`/playtest-report new` template plus the
> L1 manual checks). Write in the blanks, as roughly as you like; afterwards
> `/playtest-report analyze production/qa/playtests/playtest-2026-10-27-owner.md` turns it into the
> structured report and routes each finding (bug, balance, design change, polish).

## Session Info
- **Date**: 2026-10-27
- **Build**: `builds\wulfram3-dev-7c57587\` (`dev` @ `7c57587`, byte-identical to the `pr2-gate` player; replaces `2e5de05` after the #50 fixes)
- **Duration**:
- **Tester**: owner
- **Platform**: PC (Windows)
- **Input Method**: KB+M
- **Session Type**: Targeted test (L1 manual rows) + free play

## Test Focus
The L1 rows only a person can check (`docs/L1-LOCAL-PLAYABILITY.md`: 3, 4, 13-rotate, 17, 18), and
how the changes since the last build feel: win condition, own-team respawn, self-destruct, flak, cargo.

### Part A — offline (`Play Wulfram (offline).bat`)

Mark each **P** (pass), **F** (fail, say what happened) or **?** (unsure).

| # | L1 row | Check | P/F/? | Notes |
|---|--------|-------|-------|-------|
| A1 | 3 | WASD drives; mouse looks; the tank follows the terrain without falling through | | |
| A2 | 3 | Shift raises hover / takes off; Left Ctrl lowers / lands; Space jumps | | |
| A3 | 4 | Esc toggles the cursor (hidden = drive, visible = UI) | | |
| A4 | 4 | C swaps the camera position (only while the cursor is hidden) | | |
| A5 | 5 | Left mouse: laser line and sound; fuel bar drops while firing, refills after | | |
| A6 | 6 | Right mouse: slow pulse shell, one per charge | | |
| A7 | 9 | Tab cycles targets; T targets what is straight ahead; the info panel shows | | |
| A8 | 12 | Q picks up a box within 5 m; Z drops it | | |
| A9 | 13 | `,` shows the placement preview; **each wheel notch turns it a little (10°) and it then holds still**; click deploys | | |
| A10 | 13 | The preview does nothing on its own (no firing, no targeting); with Esc (cursor shown) a click doesn't deploy | | |
| A11 | 14 | The arena has no turrets of its own: pick up a box holding `RedFlakTurret` (6 of the 17 do) and deploy it; it stays red, so drive about 20 m away and it fires at you. **One hit does not kill a full tank**. (This build still keeps the box's colour; the owner's rule that a deployed unit fights for the deployer is a story to come) | | |
| A12 | 11 | V self-destructs: nearby enemies take damage, **your own pad/turrets don't** | | |
| A13 | 10 | Dead: overhead map + cursor; clicking a **red** pad does nothing; clicking your blue pad respawns you | | |
| A14 | 17 | HUD: hull and fuel bars move correctly; ping, FPS, team counts, target panel readable; the minimap (bottom right) works | | |
| A15 | 17 | Match line at the top: counts look right (known: overlaps the status bar, says "1 pads") | | |
| A16 | 18 | Sounds: cannon, pickup/drop, UI clicks; jump, take-off and landing; the engine hums while flying and stops when landed | | |
| A17 | 16 | Press Esc: the **Leave Game** button shows; it returns to the launcher; Play again works | | |

### Part B — online (`Play Wulfram (online).bat` twice, a few seconds apart)

| # | Check | P/F/? | Notes |
|---|-------|-------|-------|
| B1 | Both windows join; one tank per team | | |
| B2 | Each side can see and shoot the other; damage shows on both; shells and explosions are heard in both windows | | |
| B3 | A turret deployed in one window appears in the other, on the right team | | |
| B4 | Destroy the enemy's pads, then its tank: VICTORY in one window, DEFEAT in the other | | |
| B5 | About 15 s later both windows reset to a fresh arena and play on | | |

**Don't** close the first window during the 15 s countdown, unless you want to test that on
purpose: a master hand-over mid-reset is untested.

## First Impressions (First 5 minutes)
- **Understood the goal?**
- **Understood the controls?**
- **Emotional response**:
- **Notes**:

## Gameplay Flow
### What worked well
-

### Pain points
-

### Confusion points
-

### Moments of delight
-

## Bugs Encountered
| # | Description | Severity | Reproducible |
|---|-------------|----------|-------------|
| | | | |

## Feature-Specific Feedback
### Mouse feel (driving and aiming)
- **Suggestions**:

### Match end and reset
- **Understood purpose?**
- **Suggestions**:

### Building (cargo, deploy, turrets, pads)
- **Found engaging?**
- **Suggestions**:

## Owner Decisions (answer if you have a view)
- Should carried cargo be destroyed when your tank dies? (today it survives)
- SAM turret: give it AI, or take it out of the cargo boxes?
- HUD polish (overlap, "1 pads") now, or later?

## Overall Assessment
- **Would play again?**
- **Difficulty**:
- **Pacing**:

## Top 3 Priorities from this session
1.
2.
3.
