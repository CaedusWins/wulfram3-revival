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

        // Lists every unassigned object-reference field on each Launcher component
        // in a launcher scene - Launcher.Start() dereferences several of them.
        // Defaults to Launcher.unity; pass -scenePath to check another scene.
        //
        // Run via:
        //   -executeMethod Wulfram.EditorTools.WulframSceneCheck.CheckLauncherReferences [-scenePath "Assets/Scenes/Launcher 1.unity"]
        public static void CheckLauncherReferences()
        {
            string path = "Assets/Scenes/Launcher.unity";
            string[] args = System.Environment.GetCommandLineArgs();
            for (int a = 0; a < args.Length - 1; a++)
            {
                if (args[a] == "-scenePath")
                {
                    path = args[a + 1];
                }
            }

            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            // Launcher (Launcher.unity) and LauncherWithLogin (Launcher 1.unity)
            // are near-identical classes with the same fields.
            System.Collections.Generic.List<MonoBehaviour> all = new System.Collections.Generic.List<MonoBehaviour>();
            all.AddRange(Object.FindObjectsOfType<Com.Wulfram3.Launcher>());
            all.AddRange(Object.FindObjectsOfType<Com.Wulfram3.LauncherWithLogin>());
            Debug.Log("WulframSceneCheck: Launcher components in " + path + ": " + all.Count);
            for (int i = 0; i < all.Count; i++)
            {
                SerializedProperty it = new SerializedObject(all[i]).GetIterator();
                bool enter = true;
                while (it.NextVisible(enter))
                {
                    enter = false;
                    if (it.propertyType == SerializedPropertyType.ObjectReference && it.propertyPath != "m_Script")
                    {
                        Debug.Log("WulframSceneCheck: Launcher on '" + all[i].gameObject.name + "' field " + it.propertyPath +
                            " = " + (it.objectReferenceValue == null ? "UNASSIGNED" : it.objectReferenceValue.name));
                    }
                }
            }
        }

        // Lists every MonoBehaviour (and missing script) in a scene, with its
        // hierarchy path. Pass -scenePath; defaults to Launcher.unity.
        //
        // Run via:
        //   -executeMethod Wulfram.EditorTools.WulframSceneCheck.ListSceneScripts -scenePath "Assets/Scenes/Launcher 1.unity"
        public static void ListSceneScripts()
        {
            string path = "Assets/Scenes/Launcher.unity";
            string[] args = System.Environment.GetCommandLineArgs();
            for (int a = 0; a < args.Length - 1; a++)
            {
                if (args[a] == "-scenePath")
                {
                    path = args[a + 1];
                }
            }

            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            GameObject[] roots = scene.GetRootGameObjects();
            int missing = 0;
            for (int r = 0; r < roots.Length; r++)
            {
                missing += ReportMissingScripts(roots[r], roots[r].name);
                MonoBehaviour[] behaviours = roots[r].GetComponentsInChildren<MonoBehaviour>(true);
                for (int b = 0; b < behaviours.Length; b++)
                {
                    string ns = behaviours[b] == null ? null : behaviours[b].GetType().Namespace;
                    if (behaviours[b] != null && (ns == null || !ns.StartsWith("UnityEngine")))
                    {
                        Debug.Log("WulframSceneCheck: " + path + " script " + behaviours[b].GetType().FullName +
                            " on " + PathOf(behaviours[b].transform) + (behaviours[b].gameObject.activeSelf ? "" : " [inactive]"));
                    }
                }
            }
            Debug.Log("WulframSceneCheck: " + path + " - root objects: " + roots.Length + ", missing script references: " + missing);
        }

        private static string PathOf(Transform t)
        {
            string p = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                p = t.name + "/" + p;
            }
            return p;
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
