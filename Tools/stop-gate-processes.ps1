<#
Stops the processes a verification run started, with their child processes (taskkill /T), so a test
run never outlives itself: added 2026-10-26 after a stopped gate kept launching game windows that took
the owner's mouse.

  stop-gate-processes.ps1 -OutDir <run's --out folder> -ProjectDir <project the run tests>
      This run only (verify-local.sh calls it when it ends, however it ends): players started from
      OutDir (Wulfram3.exe, and the `timeout` wrappers around them) and Unity runs on ProjectDir that a
      gate starts (batch mode, or the editor Play-mode check).

  stop-gate-processes.ps1 -AllGates
      Every verification run on this machine (Tools/stop-gate.sh): verify-local.sh and pair-test.sh
      shells, players run from a verify-*/build_* scratch folder or a C:\Development\wulfram-gates
      clone, and Unity batch / Play-check runs.

Never stopped: a player under a builds\ folder (the owner's playtest builds) or a Unity editor a gate
did not start (no -batchmode, no WulframEditorPlayCheck).
#>
param(
    [string]$OutDir,
    [string]$ProjectDir,
    [switch]$AllGates
)

function Norm([string]$s) {
    if (-not $s) { return $null }
    return $s.Replace('/', '\').TrimEnd('\').ToLowerInvariant()
}

$out = Norm $OutDir
$project = Norm $ProjectDir
$hits = @()
foreach ($p in Get-CimInstance Win32_Process) {
    if (-not $p.CommandLine) { continue }
    $cmd = $p.CommandLine.Replace('/', '\').ToLowerInvariant()
    # The MSYS form of a path (\c\development\...) next to the Windows form (c:\development\...).
    $cmdMsys = $cmd -replace '([a-z]):\\', '\$1\'
    $name = $p.Name.ToLowerInvariant()
    if ($cmd.Contains('\builds\')) { continue }
    $gateUnity = ($name -eq 'unity.exe') -and ($cmd.Contains('-batchmode') -or $cmd.Contains('wulframeditorplaycheck'))
    $match = $false
    if ($out -or $project) {
        $outMsys = if ($out) { $out -replace '^([a-z]):\\', '\$1\' } else { $null }
        $namesOut = ($out -and ($cmd.Contains($out) -or $cmdMsys.Contains($out) -or $cmd.Contains($outMsys)))
        $namesProject = ($project -and ($cmd.Contains($project) -or $cmdMsys.Contains($project)))
        if (($name -eq 'wulfram3.exe' -or $name -eq 'timeout.exe') -and $namesOut) { $match = $true }
        if ($gateUnity -and $namesProject) { $match = $true }
    }
    if ($AllGates) {
        if ($name -eq 'bash.exe' -and ($cmd -match 'verify-local\.sh|pair-test\.sh')) { $match = $true }
        if (($name -eq 'wulfram3.exe' -or $name -eq 'timeout.exe') -and ($cmd -match '\\verify-|\\build_|wulfram-gates')) { $match = $true }
        if ($gateUnity) { $match = $true }
    }
    if ($match -and $p.ProcessId -ne $PID) { $hits += $p }
}

foreach ($h in $hits) {
    & taskkill.exe /PID $h.ProcessId /T /F 2>$null | Out-Null
}
if ($hits.Count -gt 0) {
    "stopped $($hits.Count) process(es): " + (($hits | ForEach-Object { "$($_.Name) $($_.ProcessId)" }) -join ', ')
} else {
    "nothing to stop"
}
