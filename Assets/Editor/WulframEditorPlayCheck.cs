using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Wulfram.EditorTools
{
    /// <summary>
    /// Runs the offline smoke test inside the Unity editor's Play mode, the way a developer tests,
    /// then closes the editor. Opens the editor window (not batch mode - Play mode needs it):
    ///
    ///   Unity.exe -projectPath <path> -logFile <log> -offlinePlay -offlineSmokeTest -smokeSeconds 8
    ///     -executeMethod Wulfram.EditorTools.WulframEditorPlayCheck.Run [-scenePath "Assets/Scenes/Playground.unity"]
    ///
    /// Starts from the launcher scene unless -scenePath says otherwise. Waits until the editor has
    /// settled before pressing Play: entering Play mode right at launch let a startup task (Unity
    /// Connect / package refresh) trigger an assembly reload mid-play, which broke Photon Chat UI
    /// (thousands of NullReferenceExceptions). A reload during Play mode now fails the check.
    /// Prints "EDITORPLAY: result PASS|FAIL|TIMEOUT|..." and exits 0 on PASS, non-zero otherwise.
    /// Inert in any editor session not started with -executeMethod ...WulframEditorPlayCheck.Run.
    /// </summary>
    [InitializeOnLoad]
    public static class WulframEditorPlayCheck
    {
        private const int SettleSeconds = 20;
        private const int TimeoutSeconds = 420;
        private static int result = -1;
        private static string failure = "";

        // Per-process keys: domain reloads (including the one entering Play mode) wipe static state,
        // and a key from another editor session never applies.
        private static string Key(string name)
        {
            return "Wulfram.EditorPlayCheck." + System.Diagnostics.Process.GetCurrentProcess().Id + "." + name;
        }

        static WulframEditorPlayCheck()
        {
            if (!Requested())
            {
                return;
            }
            if (EditorPrefs.GetBool(Key("PlayPressed"), false) && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // One reload is expected: entering Play mode. A second one is a reload mid-play.
                int reloads = EditorPrefs.GetInt(Key("PlayReloads"), 0) + 1;
                EditorPrefs.SetInt(Key("PlayReloads"), reloads);
                if (reloads > 1)
                {
                    result = 1;
                    failure = " (assembly reload during Play mode)";
                }
            }
            Application.logMessageReceived += OnLog;
            EditorApplication.update += Watch;
        }

        public static void Run()
        {
            string scene = "Assets/Scenes/Launcher 1.unity";
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-scenePath")
                {
                    scene = args[i + 1];
                }
            }
            EditorSceneManager.OpenScene(scene, OpenSceneMode.Single);
            Debug.Log("EDITORPLAY: opened " + scene + "; pressing Play once the editor has settled");
        }

        private static bool Requested()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-executeMethod" && args[i + 1] == "Wulfram.EditorTools.WulframEditorPlayCheck.Run")
                {
                    return true;
                }
            }
            return false;
        }

        private static void OnLog(string message, string stackTrace, LogType type)
        {
            if (message.StartsWith("SMOKE: PASS") && result < 0)
            {
                result = 0;
            }
            else if (message.StartsWith("SMOKE: FAIL"))
            {
                result = 1;
            }
        }

        private static void Watch()
        {
            double age = (DateTime.Now - System.Diagnostics.Process.GetCurrentProcess().StartTime).TotalSeconds;
            bool playPressed = EditorPrefs.GetBool(Key("PlayPressed"), false);

            if (result < 0 && !playPressed)
            {
                if (age >= SettleSeconds && !EditorApplication.isCompiling && !EditorApplication.isUpdating &&
                    !EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    EditorPrefs.SetBool(Key("PlayPressed"), true);
                    Debug.Log("EDITORPLAY: editor settled after " + Math.Round(age) + "s - entering Play mode");
                    EditorApplication.isPlaying = true;
                }
                if (age < TimeoutSeconds)
                {
                    return;
                }
            }

            if (result < 0 && playPressed && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                result = 1;
                failure = " (Play mode ended before the smoke test finished)";
            }

            if (result < 0 && age < TimeoutSeconds)
            {
                return;
            }

            int code = result < 0 ? 2 : result;
            Debug.Log("EDITORPLAY: result " + (code == 0 ? "PASS" : code == 1 ? "FAIL" + failure : "TIMEOUT"));
            EditorApplication.update -= Watch;
            EditorPrefs.DeleteKey(Key("PlayPressed"));
            EditorPrefs.DeleteKey(Key("PlayReloads"));
            EditorApplication.isPlaying = false;
            EditorApplication.Exit(code);
        }
    }
}
