using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wulfram.EditorTools
{
    // Read-only diagnostic: is the reason the Cargo fix silently reverts on
    // save that these objects are prefab instances, where Unity's older
    // (pre-nested-prefab) override system may not track raw SerializedObject
    // structural edits the same way it does on plain scene objects?
    public static class WulframPrefabCheck
    {
        public static void CheckPrefabStatus()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Playground.unity", OpenSceneMode.Single);
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject go = roots[i];
                if (go.name.ToLower().Contains("cargo"))
                {
                    PrefabType type = PrefabUtility.GetPrefabType(go);
                    Debug.Log("WulframPrefabCheck: " + go.name + " -> PrefabType=" + type);
                }
            }
        }
    }
}
