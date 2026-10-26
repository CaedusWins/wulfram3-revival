#!/usr/bin/env bash
# Stops every verification run on this machine and everything it started - verify-local.sh and
# pair-test.sh shells, the game clients and Unity runs they launched - with their child processes.
# Use this to stop a gate, never just the shell that started it: stopping only that shell left the gate
# running and launching game windows that took the owner's mouse (2026-10-26).
# Never touches a playtest build (builds\) or a Unity editor a gate did not start.
#
#   Tools/stop-gate.sh
HERE="$(cd "$(dirname "$0")" && pwd)"
powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "$(cygpath -w "$HERE/stop-gate-processes.ps1")" -AllGates | tr -d '\r'
