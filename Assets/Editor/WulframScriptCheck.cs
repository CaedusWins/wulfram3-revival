using UnityEditor;
using UnityEngine;

namespace Wulfram.EditorTools
{
    /// <summary>
    /// Root-cause check for the Cargo missing-script problem. A component in a
    /// scene/prefab is saved as a reference to a MonoScript asset (by GUID),
    /// and Unity resolves that MonoScript back to a class by matching the
    /// class name against the file name. If that lookup fails, MonoScript.GetClass()
    /// returns null and every component pointing at that script loads as
    /// "missing" - even though the class compiles fine and AddComponent<T>()
    /// works in memory.
    ///
    /// Run via:
    ///   Unity.exe -batchmode -quit -projectPath <path>
    ///     -executeMethod Wulfram.EditorTools.WulframScriptCheck.CheckCargoScript
    /// </summary>
    public static class WulframScriptCheck
    {
        public static void CheckCargoScript()
        {
            ReportScript("Assets/BlueFiles/cargo.cs");
            ReportScript("Assets/BlueFiles/Cargo.cs");

            MonoScript fromType = null;
            MonoScript[] all = Resources.FindObjectsOfTypeAll<MonoScript>();
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].GetClass() == typeof(Com.Wulfram3.Cargo))
                {
                    fromType = all[i];
                    break;
                }
            }
            Debug.Log("WulframScriptCheck: MonoScript resolving to Com.Wulfram3.Cargo: " +
                (fromType == null ? "NONE" : AssetDatabase.GetAssetPath(fromType)));

            ReportPrefab("Assets/Resources/Cargo.prefab");
            Debug.Log("WulframScriptCheck: done");
        }

        // Dumps whatever Unity still has serialized for each missing-script
        // component in Playground.unity (the script reference and any saved
        // field names/values), so an unresolvable component can be identified
        // by its data even though its class is gone.
        //
        // Run via:
        //   -executeMethod Wulfram.EditorTools.WulframScriptCheck.DumpMissingScripts
        public static void DumpMissingScripts()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Playground.unity",
                UnityEditor.SceneManagement.OpenSceneMode.Single);
            GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < all.Length; i++)
            {
                GameObject go = all[i];
                if (EditorUtility.IsPersistent(go) || !go.scene.IsValid())
                {
                    continue;
                }

                SerializedProperty components = new SerializedObject(go).FindProperty("m_Component");
                for (int c = 0; c < components.arraySize; c++)
                {
                    SerializedProperty slot = components.GetArrayElementAtIndex(c).FindPropertyRelative("component");
                    if (slot.objectReferenceValue != null)
                    {
                        continue;
                    }

                    int id = slot.objectReferenceInstanceIDValue;
                    Object broken = EditorUtility.InstanceIDToObject(id);
                    Debug.Log("WulframScriptCheck: missing component on '" + go.name + "' slot " + c +
                        ", instanceID " + id + ", object " + (broken == null ? "NULL" : broken.GetType().FullName));
                    if (broken == null)
                    {
                        continue;
                    }

                    SerializedObject so = new SerializedObject(broken);
                    SerializedProperty it = so.GetIterator();
                    bool enter = true;
                    while (it.Next(enter))
                    {
                        enter = false;
                        string value = it.propertyType == SerializedPropertyType.ObjectReference
                            ? (it.objectReferenceValue == null
                                ? "null (instanceID " + it.objectReferenceInstanceIDValue + ")"
                                : it.objectReferenceValue.name + " [" + AssetDatabase.GetAssetPath(it.objectReferenceValue) + "]")
                            : it.propertyType.ToString();
                        Debug.Log("WulframScriptCheck:   field " + it.propertyPath + " = " + value);
                    }
                }
            }
            Debug.Log("WulframScriptCheck: DumpMissingScripts done");
        }

        // Where does RedBase/RML/Turret_SAM's missing script live - in the
        // scene only, or in the RML.prefab asset it was instantiated from?
        //
        // Run via:
        //   -executeMethod Wulfram.EditorTools.WulframScriptCheck.CheckTurretSam
        public static void CheckTurretSam()
        {
            ReportPrefab("Assets/redfiles/redmisslelauncher/RML.prefab");

            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Playground.unity",
                UnityEditor.SceneManagement.OpenSceneMode.Single);
            GameObject sam = GameObject.Find("RedBase/RML/Turret_SAM");
            if (sam == null)
            {
                Debug.Log("WulframScriptCheck: RedBase/RML/Turret_SAM not found in scene");
                return;
            }

            Object parent = PrefabUtility.GetPrefabParent(sam);
            Debug.Log("WulframScriptCheck: Turret_SAM PrefabType=" + PrefabUtility.GetPrefabType(sam) +
                ", prefab source=" + (parent == null ? "none" : AssetDatabase.GetAssetPath(parent)) +
                ", prefab root=" + PrefabUtility.FindPrefabRoot(sam).name);
        }

        private static void ReportScript(string path)
        {
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (script == null)
            {
                Debug.Log("WulframScriptCheck: " + path + " - no MonoScript asset at this path");
                return;
            }

            System.Type cls = script.GetClass();
            Debug.Log("WulframScriptCheck: " + path + " - guid " + AssetDatabase.AssetPathToGUID(path) +
                ", GetClass() = " + (cls == null ? "NULL" : cls.FullName));
        }

        private static void ReportPrefab(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.Log("WulframScriptCheck: " + path + " - not found");
                return;
            }

            Component[] components = prefab.GetComponentsInChildren<Component>(true);
            int missing = 0;
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    missing++;
                }
                else
                {
                    Debug.Log("WulframScriptCheck: " + path + " has " + components[i].GetType().FullName +
                        " on " + components[i].gameObject.name);
                }
            }
            Debug.Log("WulframScriptCheck: " + path + " - missing script references: " + missing);
        }
    }
}
