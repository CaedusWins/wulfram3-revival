# Presses "Play!" on Unity's launch dialog ("<Product> Configuration": resolution/quality picker)
# for one player process, the way a player would. Used by verify-local.sh for windowed smoke runs:
# when the dialog is shown, the game waits there and the smoke test never starts. Sends the
# button's WM_COMMAND straight to the dialog, so it needs no mouse, keyboard or window focus.
#
#   powershell -NoProfile -ExecutionPolicy Bypass -File Tools/press-play.ps1 -ProcessId <pid> [-TimeoutSeconds 40]
#
# Prints what it did and returns as soon as the dialog was pressed, the game window appeared without
# a dialog, or the process exited.
param(
    [Parameter(Mandatory = $true)][uint32]$ProcessId,
    [int]$TimeoutSeconds = 40
)

Add-Type @'
using System; using System.Text; using System.Runtime.InteropServices;
public static class UnityLaunchDialog {
    delegate bool EnumProc(IntPtr h, IntPtr l);
    [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc f, IntPtr l);
    [DllImport("user32.dll")] static extern bool EnumChildWindows(IntPtr p, EnumProc f, IntPtr l);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr h);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetWindowText(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll")] static extern int GetDlgCtrlID(IntPtr h);
    [DllImport("user32.dll")] static extern bool PostMessage(IntPtr h, uint m, IntPtr w, IntPtr l);

    static string Text(IntPtr h) { StringBuilder s = new StringBuilder(256); GetWindowText(h, s, 256); return s.ToString(); }

    // "pressed", "game" (a visible window without the dialog), or "" (nothing visible yet).
    public static string Poll(uint pid) {
        IntPtr dialog = IntPtr.Zero;
        bool game = false;
        EnumWindows(delegate(IntPtr h, IntPtr l) {
            uint p;
            GetWindowThreadProcessId(h, out p);
            if (p != pid || !IsWindowVisible(h)) return true;
            string title = Text(h);
            if (title.EndsWith(" Configuration")) { dialog = h; return false; }
            if (title.Length > 0) game = true;
            return true;
        }, IntPtr.Zero);
        if (dialog == IntPtr.Zero) return game ? "game" : "";

        IntPtr play = IntPtr.Zero;
        EnumChildWindows(dialog, delegate(IntPtr h, IntPtr l) {
            if (Text(h) == "Play!") { play = h; return false; }
            return true;
        }, IntPtr.Zero);
        if (play == IntPtr.Zero) return "";
        // WM_COMMAND with BN_CLICKED (high word 0) and the button's control id.
        PostMessage(dialog, 0x0111, (IntPtr)(GetDlgCtrlID(play) & 0xFFFF), play);
        return "pressed";
    }
}
'@

$deadline = (Get-Date).AddSeconds($TimeoutSeconds)
while ((Get-Date) -lt $deadline) {
    if (-not (Get-Process -Id $ProcessId -ErrorAction SilentlyContinue)) {
        Write-Output "press-play: the player exited"
        exit 0
    }
    $state = [UnityLaunchDialog]::Poll($ProcessId)
    if ($state -eq "pressed") {
        Write-Output "press-play: launch dialog shown - pressed Play!"
        exit 0
    }
    if ($state -eq "game") {
        Write-Output "press-play: no launch dialog"
        exit 0
    }
    Start-Sleep -Milliseconds 500
}
Write-Output "press-play: neither the launch dialog nor a game window appeared within $TimeoutSeconds s"
exit 1
