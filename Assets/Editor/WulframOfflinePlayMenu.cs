using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Wulfram.EditorTools
{
    /// <summary>
    /// Editor menu for offline practice play (see Wulfram.OfflinePlay.OfflinePlay):
    ///   Wulfram > Offline Play (no server)       toggle. While on, pressing Play works without
    ///                                            Photon and always starts from the launcher
    ///                                            (playModeStartScene), whatever scene is open;
    ///                                            stopping returns to the scene being edited.
    ///   Wulfram > Play Offline From Launcher     turns it on and presses Play.
    /// The setting is per machine (EditorPrefs), never committed.
    /// </summary>
    [InitializeOnLoad]
    public static class WulframOfflinePlayMenu
    {
        private const string TogglePath = "Wulfram/Offline Play (no server)";
        private const string LauncherScene = "Assets/Scenes/Launcher 1.unity";

        // playModeStartScene doesn't survive an editor restart; re-apply it on every load.
        static WulframOfflinePlayMenu()
        {
            EditorApplication.delayCall += Apply;
        }

        // On when toggled, or when the editor was started with -offlinePlay (as the runtime honors it).
        private static bool IsOn()
        {
            return EditorPrefs.GetBool(Wulfram.OfflinePlay.OfflinePlay.EditorPrefKey, false) ||
                System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-offlinePlay") >= 0;
        }

        private static void Apply()
        {
            EditorSceneManager.playModeStartScene = IsOn() ? AssetDatabase.LoadAssetAtPath<SceneAsset>(LauncherScene) : null;
        }

        [MenuItem(TogglePath, false, 1)]
        private static void Toggle()
        {
            bool on = !IsOn();
            EditorPrefs.SetBool(Wulfram.OfflinePlay.OfflinePlay.EditorPrefKey, on);
            Apply();
            Debug.Log("Wulfram offline play " + (on ? "ON - Play starts from the launcher; no Photon server needed" : "OFF"));
        }

        [MenuItem(TogglePath, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(TogglePath, IsOn());
            return !EditorApplication.isPlaying;
        }

        [MenuItem("Wulfram/Play Offline From Launcher", false, 2)]
        private static void PlayFromLauncher()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }
            EditorPrefs.SetBool(Wulfram.OfflinePlay.OfflinePlay.EditorPrefKey, true);
            Apply();
            EditorApplication.isPlaying = true;
        }
    }
}
