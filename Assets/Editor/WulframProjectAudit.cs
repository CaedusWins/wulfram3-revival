using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wulfram.EditorTools
{
    /// <summary>
    /// Read-only, project-wide audit. Nothing is saved. Every line is prefixed "AUDIT:".
    ///
    ///   1. Missing scripts in EVERY prefab in the project, not just the build scenes.
    ///      Resources/ prefabs are flagged because they always ship in a build.
    ///   2. Scripts Unity cannot resolve to a class (MonoScript.GetClass() == null) that
    ///      look like components - the bug class that broke all 17 Cargo objects.
    ///   3. Every Unit in Playground.unity and in every prefab, with the components
    ///      TargetInfoController.LateUpdate dereferences on a selected target
    ///      (HitPointsManager, a MeshRenderer in children, PhotonView for tanks/scouts).
    ///
    /// Run via:
    ///   Unity.exe -batchmode -quit -projectPath <path>
    ///     -executeMethod Wulfram.EditorTools.WulframProjectAudit.Run
    /// </summary>
    public static class WulframProjectAudit
    {
        public static void Run()
        {
            AuditPrefabMissingScripts();
            AuditUnresolvableScripts();
            AuditDragTransforms();
            AuditUnits();
            Debug.Log("AUDIT: done");
        }

        // DragTransform was ported from UnityScript to C# keeping the .js GUID; every prefab
        // that used it must now resolve to the C# class with its serialized color intact.
        private static void AuditDragTransforms()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets" });
            int found = 0;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                DragTransform[] drags = prefab.GetComponentsInChildren<DragTransform>(true);
                for (int d = 0; d < drags.Length; d++)
                {
                    found++;
                    Debug.Log("AUDIT: drag-transform " + path + " :: " + HierarchyPath(drags[d].transform) +
                        " mouseOverColor=" + drags[d].mouseOverColor +
                        " renderer=" + (drags[d].GetComponent<Renderer>() != null));
                }
            }
            Debug.Log("AUDIT: DragTransform components in prefabs " + found);
        }

        private static void AuditPrefabMissingScripts()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets" });
            int withMissing = 0;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                int missing = 0;
                Component[] all = prefab.GetComponentsInChildren<Component>(true);
                for (int c = 0; c < all.Length; c++)
                {
                    if (all[c] == null)
                    {
                        missing++;
                    }
                }

                if (missing > 0)
                {
                    withMissing++;
                    bool shipped = path.Contains("/Resources/");
                    Debug.Log("AUDIT: prefab-missing " + missing + " " + (shipped ? "[RESOURCES - SHIPS] " : "") + path);
                }
            }
            Debug.Log("AUDIT: prefabs scanned " + guids.Length + ", with missing scripts " + withMissing);
        }

        private static void AuditUnresolvableScripts()
        {
            string[] guids = AssetDatabase.FindAssets("t:MonoScript", new string[] { "Assets" });
            int flagged = 0;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!path.EndsWith(".cs") && !path.EndsWith(".js"))
                {
                    continue;
                }

                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script == null || script.GetClass() != null)
                {
                    continue;
                }

                string text = script.text;
                if (text.Contains("MonoBehaviour") || text.Contains("Behaviour") || text.Contains("ScriptableObject"))
                {
                    flagged++;
                    Debug.Log("AUDIT: unresolvable-script " + path);
                }
            }
            Debug.Log("AUDIT: scripts scanned " + guids.Length + ", component-like scripts with no resolvable class " + flagged);
        }

        private static void AuditUnits()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets" });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                Com.Wulfram3.Unit[] units = prefab.GetComponentsInChildren<Com.Wulfram3.Unit>(true);
                for (int u = 0; u < units.Length; u++)
                {
                    ReportUnit("prefab " + path, units[u]);
                }
            }

            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Playground.unity", OpenSceneMode.Single);
            GameObject[] roots = scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
            {
                Com.Wulfram3.Unit[] units = roots[r].GetComponentsInChildren<Com.Wulfram3.Unit>(true);
                for (int u = 0; u < units.Length; u++)
                {
                    ReportUnit("scene Playground", units[u]);
                }
            }
        }

        private static void ReportUnit(string where, Com.Wulfram3.Unit unit)
        {
            GameObject go = unit.gameObject;
            MeshRenderer renderer = go.GetComponentInChildren<MeshRenderer>(true);
            List<string> problems = new List<string>();
            if (go.GetComponent<Com.Wulfram3.HitPointsManager>() == null)
            {
                problems.Add("no HitPointsManager");
            }
            if (renderer == null)
            {
                problems.Add("no MeshRenderer in children");
            }
            string kind = unit.name == null ? "" : unit.name.ToLower();
            if ((kind == "tank" || kind == "scout") && go.GetComponent<PhotonView>() == null)
            {
                problems.Add("tank/scout without PhotonView");
            }

            Debug.Log("AUDIT: unit " + where + " :: " + HierarchyPath(go.transform) +
                " [name=" + unit.name + " team=" + unit.team + "]" +
                (go.GetComponent<Com.Wulfram3.TargetController>() != null ? " [has TargetController]" : "") +
                (problems.Count == 0 ? " OK" : " PROBLEM: " + string.Join(", ", problems.ToArray())));
        }

        private static string HierarchyPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
    }
}
