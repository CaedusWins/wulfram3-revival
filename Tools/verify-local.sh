#!/usr/bin/env bash
# Local verification gate: everything "verified" means before a merge upward
# (CLAUDE.md -> "QA & Merge Process"). Needs Unity 2017.3.0f3; run from Git Bash on Windows.
#
#   Tools/verify-local.sh [--project <path>] [--out <dir>] [--windowed]
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
#   7. smoke-win  only with --windowed: the same in a small window (it opens on screen for ~20s -
#                 don't click it), and probePanelShown=True is required too: only a rendered frame
#                 reaches the body of TargetInfoController.LateUpdate. Run the script from an
#                 interactive desktop session: launched from a background job, the windowed player
#                 hangs at startup (right after "<RI> Input initialized.").
#
# Unity's process exit code is not trusted (it can be 0 with compile errors); every step reads its log.
set -u

UNITY="${UNITY:-C:/Program Files/Unity/Editor/Unity.exe}"
HERE="$(cd "$(dirname "$0")" && pwd)"
PROJECT="$(cd "$HERE/.." && pwd)"
OUT=""
WINDOWED=0
while [ $# -gt 0 ]; do
  case "$1" in
    --project) PROJECT="$(cd "$2" && pwd)"; shift 2 ;;
    --out) OUT="$2"; shift 2 ;;
    --windowed) WINDOWED=1; shift ;;
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
log="$OUT/1-compile.log"
run_unity "$log"
if grep -q 'Exiting batchmode successfully' "$log" && ! grep -qE 'compilationhadfailure: True|error CS[0-9]+|BCE[0-9]+' "$log"; then
  pass "compile"
else
  fail "compile - see $log"
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

# 7. smoke (windowed)
if [ "$WINDOWED" -eq 1 ]; then
  if [ "$built" -eq 1 ]; then
    log="$OUT/7-smoke-windowed.log"
    timeout 180 "$player" -screen-fullscreen 0 -screen-width 640 -screen-height 360 -offlineSmokeTest -smokeSeconds 6 > "$log" 2>&1
    grep -E '^SMOKE: (scene=|problem)' "$log" | tr -d '\r' | sed 's/^/        /'
    if grep -q '^SMOKE: PASS' "$log" && grep -q 'probePanelShown=True' "$log"; then
      pass "smoke (windowed, target panel exercised)"
    elif ! grep -q '^SMOKE: starting' "$log"; then
      fail "smoke (windowed) - the player never started; run from an interactive desktop session, not a background job - see $log"
    else
      fail "smoke (windowed) - see $log"
    fi
  else
    fail "smoke (windowed) - skipped, no build"
  fi
fi

echo
if [ "$failures" -eq 0 ]; then
  echo "VERIFIED - all steps passed"
  exit 0
fi
echo "NOT VERIFIED - $failures step(s) failed"
exit 1
