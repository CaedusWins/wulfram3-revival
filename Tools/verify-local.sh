#!/usr/bin/env bash
# Local verification gate: everything "verified" means before a merge upward
# (CLAUDE.md -> "QA & Merge Process"). Needs Unity 2017.3.0f3; run from Git Bash on Windows.
#
#   Tools/verify-local.sh [--project <path>] [--out <dir>] [--windowed] [--screenshots] [--editor] [--online]
#
# Steps - each one fails loudly, and the script exits non-zero if any step failed:
#   1. compile    batch-mode import + compile. ANY compiler failure fails it
#                 ("compilationhadfailure: True" covers C# and UnityScript alike), not only "error CS".
#   2. scenes     WulframSceneCheck.CheckBuildScenes: both build scenes, 0 missing scripts.
#   3. cargo      WulframSceneCheck.CountCargoComponents: exactly 17.
#   4. audit      WulframProjectAudit.Run, compared with Tools/audit-baseline.txt (new findings fail).
#   5. build      Windows 64-bit player: no compiler failure, and no "Script attached ... missing"
#                 warning except the known one ('Sun' in Resources/Terrains/Tron.prefab).
#   6. smoke      the built player with -batchmode -offlineSmokeTest: "SMOKE: PASS".
#   6b.           the same with -offlinePlay: enters through the launcher's real Play button.
#   6c. online   only with --online (needs the Photon App ID + internet): M2 - two clients through the
#                 real Play button on Photon Cloud must share one room, see 2 players + 2 tanks, and
#                 deal damage both ways via the game's network damage path (TellServerTakeDamage).
#                 The second client starts 8s after the first. Each run joins its own room
#                 (-photonRoom gate-<run>-<step>), never the players' default one.
#   6d.           the same with both clients pressing Play at the same instant.
#   7. smoke-win  only with --windowed: the same in a 1280x720 window (it is on screen for ~30s -
#                 don't click it), and probePanelShown=True is required too: only a rendered frame
#                 reaches the body of TargetInfoController.LateUpdate. If the game first shows Unity's
#                 launch dialog ("Wulfram 3 Configuration"), Tools/press-play.ps1 presses "Play!".
#                 Needs an interactive desktop session. --screenshots also saves the game's own
#                 frames to <out>/screenshots (implies --windowed).
#   8. editor     only with --editor: opens the Unity editor (its window shows for ~1-2 min per run)
#                 and runs the smoke test in Play mode with offline play, once opened on the launcher
#                 and once on Playground (Play must start from the launcher either way). Needs
#                 EDITORPLAY PASS and zero exceptions anywhere in the editor log.
#
# Unity's process exit code is not trusted (it can be 0 with compile errors); every step reads its log.
set -u

UNITY="${UNITY:-C:/Program Files/Unity/Editor/Unity.exe}"
HERE="$(cd "$(dirname "$0")" && pwd)"
PROJECT="$(cd "$HERE/.." && pwd)"
OUT=""
WINDOWED=0
SCREENSHOTS=0
EDITOR=0
ONLINE=0
while [ $# -gt 0 ]; do
  case "$1" in
    --project) PROJECT="$(cd "$2" && pwd)"; shift 2 ;;
    --out) OUT="$2"; shift 2 ;;
    --windowed) WINDOWED=1; shift ;;
    --screenshots) WINDOWED=1; SCREENSHOTS=1; shift ;;
    --editor) EDITOR=1; shift ;;
    --online) ONLINE=1; shift ;;
    *) echo "unknown argument: $1"; exit 2 ;;
  esac
done
OUT="${OUT:-${TMPDIR:-/tmp}/wulfram-verify-$(date +%Y%m%d-%H%M%S)}"
mkdir -p "$OUT"
OUT="$(cd "$OUT" && pwd)"

if [ ! -f "$UNITY" ]; then
  echo "Unity not found at $UNITY (set UNITY=...)"
  exit 2
fi
echo "project: $PROJECT"
echo "logs:    $OUT"

failures=0
pass() { echo "PASS  $1"; }
fail() { echo "FAIL  $1"; failures=$((failures + 1)); }
win() { cygpath -w "$1"; }

# run_unity <log> [extra args...] - one batch-mode editor run on the project
run_unity() {
  local log="$1"
  shift
  "$UNITY" -batchmode -quit -nographics -projectPath "$(win "$PROJECT")" -logFile "$(win "$log")" "$@"
}

# 1. compile
compile_clean() {
  grep -q 'Exiting batchmode successfully' "$1" && ! grep -qE 'compilationhadfailure: True|error CS[0-9]+|BCE[0-9]+' "$1"
}
log="$OUT/1-compile.log"
run_unity "$log"
if compile_clean "$log"; then
  pass "compile"
else
  # In an existing working copy, the first compile after scripts are added or removed runs on
  # Unity's stale file list and fails (e.g. CS0234 for a just-added namespace) before Unity
  # refreshes. A real error fails again; fresh clones never hit this.
  rerun="$OUT/1-compile-rerun.log"
  run_unity "$rerun"
  if compile_clean "$rerun"; then
    pass "compile (first run failed on Unity's stale script list after files were added/removed; rerun clean - see $log)"
  else
    fail "compile - see $log and $rerun"
  fi
fi

# 2. scenes
log="$OUT/2-scenes.log"
run_unity "$log" -executeMethod Wulfram.EditorTools.WulframSceneCheck.CheckBuildScenes
scenes=$(tr -d '\r' < "$log" | grep -c 'missing script references: ')
dirty=$(tr -d '\r' < "$log" | grep 'missing script references: ' | grep -vc 'missing script references: 0$')
if [ "$scenes" -eq 2 ] && [ "$dirty" -eq 0 ]; then
  pass "scenes (2 build scenes, 0 missing scripts)"
else
  fail "scenes ($scenes scene(s) checked, $dirty with missing scripts) - see $log"
fi

# 3. cargo
log="$OUT/3-cargo.log"
run_unity "$log" -executeMethod Wulfram.EditorTools.WulframSceneCheck.CountCargoComponents
if tr -d '\r' < "$log" | grep -q 'total Cargo components in Playground.unity: 17$'; then
  pass "cargo (17)"
else
  fail "cargo - see $log"
fi

# 4. audit
log="$OUT/4-audit.log"
run_unity "$log" -executeMethod Wulfram.EditorTools.WulframProjectAudit.Run
tr -d '\r' < "$log" | grep -E '^AUDIT: (prefab-missing|unresolvable-script) ' | sed 's/^AUDIT: //' | sort -u > "$OUT/4-audit-found.txt"
grep -vE '^(#|$)' "$HERE/audit-baseline.txt" | tr -d '\r' | sort -u > "$OUT/4-audit-baseline.txt"
new=$(comm -13 "$OUT/4-audit-baseline.txt" "$OUT/4-audit-found.txt")
gone=$(comm -23 "$OUT/4-audit-baseline.txt" "$OUT/4-audit-found.txt")
if ! tr -d '\r' < "$log" | grep -q '^AUDIT: done'; then
  fail "audit did not finish - see $log"
elif [ -n "$new" ]; then
  fail "audit - new findings not in Tools/audit-baseline.txt:"
  echo "$new" | sed 's/^/        /'
else
  pass "audit (matches Tools/audit-baseline.txt)"
fi
if [ -n "$gone" ]; then
  echo "NOTE  baseline entries no longer found (fixed? remove them from Tools/audit-baseline.txt):"
  echo "$gone" | sed 's/^/        /'
fi

# 5. build
log="$OUT/5-build.log"
player="$OUT/player/Wulfram3.exe"
run_unity "$log" -buildWindows64Player "$(win "$player")"
unknown_missing=$(grep 'Script attached to' "$log" | grep -vc "Script attached to 'Sun' in scene ''")
if grep -q "Completed 'Build.Player" "$log" && ! grep -q 'compilationhadfailure: True' "$log" &&
   [ "$unknown_missing" -eq 0 ] && [ -f "$player" ]; then
  pass "build"
  built=1
else
  fail "build ($unknown_missing unexpected 'script missing' warning(s)) - see $log"
  built=0
fi

# 6. smoke (headless)
if [ "$built" -eq 1 ]; then
  log="$OUT/6-smoke.log"
  timeout 300 "$player" -batchmode -offlineSmokeTest -smokeSeconds 10 > "$log" 2>&1
  grep -E '^SMOKE: (scene=|problem)' "$log" | tr -d '\r' | sed 's/^/        /'
  if grep -q '^SMOKE: PASS' "$log"; then
    pass "smoke (headless)"
  else
    fail "smoke (headless) - see $log"
  fi
else
  fail "smoke (headless) - skipped, no build"
fi

# 6b. smoke via the launcher's Play button, offline play mode (-offlinePlay)
if [ "$built" -eq 1 ]; then
  log="$OUT/6b-smoke-offline-play.log"
  timeout 300 "$player" -batchmode -offlinePlay -offlineSmokeTest -smokeSeconds 10 > "$log" 2>&1
  grep -E '^SMOKE: (scene=|problem)' "$log" | tr -d '\r' | sed 's/^/        /'
  if grep -q '^SMOKE: PASS' "$log" && grep -q "entering via the launcher's Play button" "$log"; then
    pass "smoke (offline play, through the Play button)"
  else
    fail "smoke (offline play) - see $log"
  fi
else
  fail "smoke (offline play) - skipped, no build"
fi

# 6c/6d. M2 online (only with --online): two clients through the real Play button on Photon Cloud.
# online_pair <step> <seconds between the two starts> - both clients must PASS, end up in the same
# room, and that room must be the one asked for with -photonRoom. Every run uses its own room name,
# so a gate never meets real players or another gate running at the same time.
online_pair() {
  local step="$1" delay="$2" room logA logB pidA pidB roomA roomB
  room="gate-$RUN_ID-$step"
  logA="$OUT/$step-online-client-A.log"
  logB="$OUT/$step-online-client-B.log"
  timeout 300 "$player" -batchmode -offlineSmokeTest -smokeOnline -smokeExpectPlayers 2 -smokeSeconds 10 -photonRoom "$room" > "$logA" 2>&1 &
  pidA=$!
  [ "$delay" -gt 0 ] && sleep "$delay"
  timeout 300 "$player" -batchmode -offlineSmokeTest -smokeOnline -smokeExpectPlayers 2 -smokeSeconds 10 -photonRoom "$room" > "$logB" 2>&1 &
  pidB=$!
  wait "$pidA"
  wait "$pidB"
  for l in "$logA" "$logB"; do
    grep -E '^(PhotonCloud|Launcher|SMOKE: (damage|problem))' "$l" | tr -d '\r' | sed 's/^/        /'
  done
  roomA=$(grep -oE 'room=[^ ]+' "$logA" | head -1 | tr -d '\r')
  roomB=$(grep -oE 'room=[^ ]+' "$logB" | head -1 | tr -d '\r')
  if grep -q '^PhotonCloud: no Photon App ID' "$logA"; then
    fail "online M2 ($step) - no Photon App ID (Assets/Resources/PhotonAppId.local.txt or WULFRAM_PHOTON_APPID)"
  elif [ -z "$roomA" ] || [ "$roomA" != "$roomB" ]; then
    fail "online M2 ($step) - the two clients are not in one room: '${roomA}' / '${roomB}' (see $logA, $logB)"
  elif [ "$roomA" != "room=$room" ]; then
    fail "online M2 ($step) - shared room is '$roomA', expected 'room=$room' (see $logA)"
  elif grep -q '^SMOKE: PASS' "$logA" && grep -q '^SMOKE: PASS' "$logB"; then
    pass "online M2 ($step): two clients started ${delay}s apart share one Photon Cloud room ($roomA), damage both ways"
  else
    fail "online M2 ($step) - see $logA and $logB"
  fi
}

if [ "$ONLINE" -eq 1 ]; then
  if [ "$built" -eq 1 ]; then
    RUN_ID="$(date +%Y%m%d%H%M%S)-$$"
    # 6c: the second client presses Play 8s after the first (the room already exists).
    online_pair 6c 8
    # 6d: both press Play at the same instant (the room is created while the other is joining).
    online_pair 6d 0
  else
    fail "online M2 - skipped, no build"
  fi
fi

# stop_player <bash pid> - end a player started in the background
stop_player() {
  local winpid
  winpid=$(cat "/proc/$1/winpid" 2>/dev/null)
  if [ -n "$winpid" ]; then
    taskkill //F //PID "$winpid" > /dev/null 2>&1
  else
    kill "$1" 2>/dev/null
  fi
  wait "$1" 2>/dev/null
}

# 7. smoke (windowed)
if [ "$WINDOWED" -eq 1 ]; then
  if [ "$built" -eq 1 ]; then
    shots=()
    if [ "$SCREENSHOTS" -eq 1 ]; then
      mkdir -p "$OUT/screenshots"
      shots=(-smokeScreenshots "$(win "$OUT/screenshots")")
    fi
    # The windowed player may first show Unity's launch dialog ("Wulfram 3 Configuration") and
    # wait for "Play!" - press-play.ps1 presses it, as a player would.
    log="$OUT/7-smoke-windowed.log"
    "$player" -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -offlineSmokeTest -smokeSeconds 6 ${shots[@]+"${shots[@]}"} > "$log" 2>&1 &
    pid=$!
    # Read the Windows PID only after a moment: read right after launch it named another process
    # (press-play then reported "neither the launch dialog nor a game window appeared").
    sleep 2
    winpid=$(cat "/proc/$pid/winpid" 2>/dev/null)
    powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(win "$HERE/press-play.ps1")" -ProcessId "$winpid" -TimeoutSeconds 40 | tr -d '\r' | sed 's/^/        /'
    waited=0
    while [ "$waited" -lt 45 ] && ! grep -q '^SMOKE: starting' "$log" && kill -0 "$pid" 2>/dev/null; do
      sleep 1
      waited=$((waited + 1))
    done
    waited=0
    while [ "$waited" -lt 180 ] && grep -q '^SMOKE: starting' "$log" && kill -0 "$pid" 2>/dev/null; do
      sleep 1
      waited=$((waited + 1))
    done
    if kill -0 "$pid" 2>/dev/null; then
      stop_player "$pid"
    fi
    grep -E '^SMOKE: (scene=|problem)' "$log" | tr -d '\r' | sed 's/^/        /'
    if grep -q '^SMOKE: PASS' "$log" && grep -q 'probePanelShown=True' "$log"; then
      pass "smoke (windowed, target panel exercised)"
    elif ! grep -q '^SMOKE: starting' "$log"; then
      fail "smoke (windowed) - the player never reached the smoke test - see $log"
    else
      fail "smoke (windowed) - see $log"
    fi
  else
    fail "smoke (windowed) - skipped, no build"
  fi
fi

# 8. editor Play mode (only with --editor): offline play from the launcher scene and from Playground
if [ "$EDITOR" -eq 1 ]; then
  for start in "Assets/Scenes/Launcher 1.unity" "Assets/Scenes/Playground.unity"; do
    name=$(basename "$start" .unity | tr ' ' '-')
    log="$OUT/8-editor-play-$name.log"
    timeout 600 "$UNITY" -projectPath "$(win "$PROJECT")" -logFile "$(win "$log")" -offlinePlay -offlineSmokeTest -smokeSeconds 8 \
      -executeMethod Wulfram.EditorTools.WulframEditorPlayCheck.Run -scenePath "$start"
    tr -d '\r' < "$log" | grep -E '^(EDITORPLAY: result|SMOKE: (scene=|problem))' | sed 's/^/        /'
    exceptions=$(tr -d '\r' < "$log" | grep -cE '^[A-Za-z.]*Exception')
    if tr -d '\r' < "$log" | grep -q '^EDITORPLAY: result PASS' && [ "$exceptions" -eq 0 ]; then
      pass "editor Play mode, opened on $name ($exceptions exceptions in the editor log)"
    else
      fail "editor Play mode, opened on $name ($exceptions exceptions in the editor log) - see $log"
    fi
  done
fi

echo
if [ "$failures" -eq 0 ]; then
  echo "VERIFIED - all steps passed"
  exit 0
fi
echo "NOT VERIFIED - $failures step(s) failed"
exit 1
