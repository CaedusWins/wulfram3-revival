using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wulfram.EditorTools
{
    /// <summary>
    /// Sanity check before trusting the M1 Build Settings fix: open each scene
    /// in EditorBuildSettings and report anything that looks broken (missing
    /// scripts, broken component references). This is Edit-mode only - it does
    /// NOT enter Play mode, because Play mode would trigger real Awake/Start
    /// logic (including Photon connecting to a live network endpoint), which
    /// isn't safe to run unattended in a batch-mode CLI session with no way to
    /// interrupt a hang.
    ///
    /// Run via:
    ///   Unity.exe -batchmode -quit -projectPath <path>
    ///     -executeMethod Wulfram.EditorTools.WulframSceneCheck.CheckBuildScenes
    /// </summary>
    public static class WulframSceneCheck
    {
        public static void CheckBuildScenes()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            Debug.Log("WulframSceneCheck: checking " + scenes.Length + " scene(s) from Build Settings");

            for (int i = 0; i < scenes.Length; i++)
            {
                string path = scenes[i].path;
                Debug.Log("WulframSceneCheck: opening " + path);

                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                if (!scene.IsValid())
                {
                    Debug.LogError("WulframSceneCheck: FAILED TO OPEN " + path);
                    continue;
                }

                int missingScriptCount = 0;
                GameObject[] roots = scene.GetRootGameObjects();
                for (int r = 0; r < roots.Length; r++)
                {
                    missingScriptCount += ReportMissingScripts(roots[r], roots[r].name);
                }

                Debug.Log("WulframSceneCheck: " + path + " - root objects: " + roots.Length +
                    ", missing script references: " + missingScriptCount);
            }

            Debug.Log("WulframSceneCheck: done");
        }

        public static void CountCargoComponents()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Playground.unity", OpenSceneMode.Single);
            Com.Wulfram3.Cargo[] all = Object.FindObjectsOfType<Com.Wulfram3.Cargo>();
            Debug.Log("WulframSceneCheck: total Cargo components in Playground.unity: " + all.Length);
            for (int i = 0; i < all.Length; i++)
            {
                Debug.Log("WulframSceneCheck: Cargo component on " + all[i].gameObject.name);
            }
        }

        private static int ReportMissingScripts(GameObject go, string hierarchyPath)
        {
            int count = 0;
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    count++;
                    Debug.Log("WulframSceneCheck: MISSING SCRIPT at " + hierarchyPath + " (component index " + i + ")");
                }
            }

            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                Transform child = t.GetChild(i);
                count += ReportMissingScripts(child.gameObject, hierarchyPath + "/" + child.name);
            }

            return count;
        }
    }
}
