using UnityEditor;
using UnityEngine;

namespace Wulfram.EditorTools
{
    /// <summary>
    /// M1 fix: EditorBuildSettings.asset (a binary Unity asset, unsafe to hand-edit)
    /// was missing Assets/Scenes/Playground.unity - the actual gameplay arena that
    /// GameManager.cs loads via PhotonNetwork.LoadLevel("Playground") at runtime.
    /// It also carried two problems alongside that: a vendored MHLab PATCH demo
    /// launcher scene that isn't part of this game, and a duplicate launcher scene
    /// ("Launcher 1.unity") sitting next to the real one.
    ///
    /// CORRECTION (verified by building and running the player): "Launcher 1.unity"
    /// is the real launcher, not a duplicate. It holds the LauncherWithLogin
    /// component with the full login/registration UI wired up, and it is the scene
    /// the original team kept editing through Jan 2018 (and had enabled in Build
    /// Settings). "Launcher.unity" is an older name-entry scene whose Launcher
    /// component expects that same UI but has none of it assigned - the built
    /// player threw NullReferenceException in Launcher.Start() when it was scene 0.
    /// GameManager returns to the launcher by index (LoadScene(0)), so the launcher
    /// must stay first.
    ///
    /// Run via Unity's CLI so this can be done safely and repeatably without
    /// touching the binary asset by hand:
    ///   Unity.exe -batchmode -quit -projectPath <path>
    ///     -executeMethod Wulfram.EditorTools.WulframBuildSettings.SetCanonicalScenes
    /// </summary>
    public static class WulframBuildSettings
    {
        public static void SetCanonicalScenes()
        {
            Debug.Log("WulframBuildSettings: current scenes before fix:");
            EditorBuildSettingsScene[] before = EditorBuildSettings.scenes;
            for (int i = 0; i < before.Length; i++)
            {
                Debug.Log("  [" + i + "] enabled=" + before[i].enabled + " path=" + before[i].path);
            }

            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Launcher 1.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Playground.unity", true),
            };

            EditorBuildSettings.scenes = scenes;
            AssetDatabase.SaveAssets();

            Debug.Log("WulframBuildSettings: scenes after fix:");
            EditorBuildSettingsScene[] after = EditorBuildSettings.scenes;
            for (int i = 0; i < after.Length; i++)
            {
                Debug.Log("  [" + i + "] enabled=" + after[i].enabled + " path=" + after[i].path);
            }
        }
    }
}
