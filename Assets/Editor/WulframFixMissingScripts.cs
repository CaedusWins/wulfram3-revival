using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wulfram.EditorTools
{
    /// <summary>
    /// Fixes the 18 missing-script references WulframSceneCheck found in
    /// Playground.unity. Evidence-based, not a guess:
    ///
    /// - 16 of the 18 are on objects named "Cargo"/"Cargo (N)"/"*Cargo", all
    ///   broken at the same component slot - consistent with one shared
    ///   prefab losing its script link (a stale GUID from the file
    ///   reorganization, most likely). GameManager.cs itself calls
    ///   GetComponent<Cargo>() on these objects, and Com.Wulfram3.Cargo
    ///   (Assets/BlueFiles/cargo.cs) is a real, currently-compiling class -
    ///   so re-attaching it is restoring known-intended behavior, not
    ///   inventing it. Cargo's only field ("content") is set at runtime via
    ///   a PunRPC, not hand-configured per instance, so a fresh component
    ///   with default values is exactly what these objects should have.
    ///
    /// - The 18th, RedBase/RML/Turret_SAM, doesn't match any class anywhere
    ///   in this codebase (searched for "SAM" and any Turret class). There is
    ///   nothing to restore, so this one is only cleaned, not replaced -
    ///   logged clearly so it isn't silently lost.
    ///
    /// Uses SerializedObject on GameObject's internal "m_Component" array to
    /// strip null entries, rather than GameObjectUtility's newer
    /// missing-script helpers, since we already hit one 2017.3 API gap today
    /// (GetMonoBehavioursWithMissingScriptCount) and this pattern is old and
    /// stable enough to trust without re-verifying against this exact version.
    /// </summary>
    public static class WulframFixMissingScripts
    {
        // Phase 1: remove the broken/missing component slots and save.
        // Run this in its own Unity process, then run AddCargoComponents in a
        // SEPARATE process afterward that reopens the scene fresh from disk.
        // (A first attempt that did both phases in one process, one frame,
        // corrupted 5 objects - mixing a low-level SerializedObject structural
        // edit with a native AddComponent call in the same step desynced the
        // two representations. Two clean, separate passes avoids that.)
        public static void RemoveMissingOnly()
        {
            string path = "Assets/Scenes/Playground.unity";
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            int removedFrom = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
            {
                removedFrom += RemoveRecursive(roots[r], roots[r].name);
            }

            EditorSceneManager.SaveScene(scene);
            Debug.Log("WulframFixMissingScripts: RemoveMissingOnly done - objects cleaned: " + removedFrom);
        }

        // Phase 2: reopen the now-clean scene and attach Cargo to every
        // object whose name matches the pattern we confirmed was affected.
        public static void AddCargoComponents()
        {
            string path = "Assets/Scenes/Playground.unity";
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            int cargoFixed = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
            {
                cargoFixed += AddCargoRecursive(roots[r], roots[r].name);
            }

            EditorSceneManager.SaveScene(scene);
            Debug.Log("WulframFixMissingScripts: AddCargoComponents done - Cargo attached: " + cargoFixed);
        }

        // Phase 2, one-object-per-process variant: the batched version (all 17
        // in one process, one -executeMethod call) only reliably persisted a
        // subset (1 of 17, then 8 of 17 after adding dirty-marking - see
        // CLAUDE.md for the full story). Doing exactly one object per fresh
        // Unity process sidesteps whatever that in-process batching issue is,
        // since each process does the minimum possible work before saving.
        //
        // Target object name comes from a custom CLI arg:
        //   -executeMethod Wulfram.EditorTools.WulframFixMissingScripts.AddCargoToOne -cargoTarget "Cargo (5)"
        public static void AddCargoToOne()
        {
            string target = null;
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-cargoTarget")
                {
                    target = args[i + 1];
                    break;
                }
            }

            if (string.IsNullOrEmpty(target))
            {
                Debug.LogError("WulframFixMissingScripts: AddCargoToOne - no -cargoTarget argument given");
                return;
            }

            // NOTE: an earlier version of this method called
            // AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate)
            // here, on the theory that stale AssetDatabase caching between rapid
            // process launches explained why only some objects' changes
            // persisted. It didn't fix that, and in one run appeared to actively
            // REGRESS previously-successful fixes on other objects (missing-script
            // count went from 7 back up to 12 after a retry round that used this).
            // Deliberately removed - do not re-add without new evidence, and treat
            // this whole one-process-per-object approach as unreliable for this
            // task. See CLAUDE.md for the full account and the decision to finish
            // the remaining objects by hand in the Unity GUI instead.
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Playground.unity", OpenSceneMode.Single);
            GameObject[] roots = scene.GetRootGameObjects();
            GameObject found = null;
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == target)
                {
                    found = roots[i];
                    break;
                }
            }

            if (found == null)
            {
                Debug.LogError("WulframFixMissingScripts: AddCargoToOne - no root object named '" + target + "' found");
                return;
            }

            if (found.GetComponent<Com.Wulfram3.Cargo>() != null)
            {
                Debug.Log("WulframFixMissingScripts: AddCargoToOne - '" + target + "' already has Cargo, skipping");
                return;
            }

            found.AddComponent<Com.Wulfram3.Cargo>();
            EditorUtility.SetDirty(found);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("WulframFixMissingScripts: AddCargoToOne - attached Cargo to '" + target + "'");
        }

        private static int RemoveRecursive(GameObject go, string hierarchyPath)
        {
            int count = 0;
            if (RemoveMissingScripts(go))
            {
                count = 1;
                if (!go.name.ToLower().Contains("cargo"))
                {
                    Debug.LogWarning("WulframFixMissingScripts: removed missing script at " + hierarchyPath +
                        " - no matching class found in codebase, left without a replacement. Needs manual investigation.");
                }
            }

            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                Transform child = t.GetChild(i);
                count += RemoveRecursive(child.gameObject, hierarchyPath + "/" + child.name);
            }

            return count;
        }

        private static int AddCargoRecursive(GameObject go, string hierarchyPath)
        {
            int count = 0;
            // go.name only - NOT hierarchyPath. A first attempt matched the full
            // accumulated path, which meant every child of a Cargo object (e.g.
            // "Blue FT Cargo/Box") also matched "contains cargo" and wrongly got
            // the component too (51 attached instead of the correct 17). The
            // original missing-script report only ever listed root object names,
            // never their children - confirming only the root objects need this.
            if (go.name.ToLower().Contains("cargo") && go.GetComponent<Com.Wulfram3.Cargo>() == null)
            {
                go.AddComponent<Com.Wulfram3.Cargo>();
                // Same lesson as RemoveMissingScripts: a native mutation in this
                // batch-mode/-executeMethod context (no normal Editor update loop
                // pumping between calls) needs an explicit dirty mark or
                // EditorSceneManager.SaveScene silently drops it for most objects.
                EditorUtility.SetDirty(go);
                EditorSceneManager.MarkSceneDirty(go.scene);
                count = 1;
                Debug.Log("WulframFixMissingScripts: attached Cargo to " + hierarchyPath);
            }

            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                Transform child = t.GetChild(i);
                count += AddCargoRecursive(child.gameObject, hierarchyPath + "/" + child.name);
            }

            return count;
        }

        // Removes null ("missing script") entries from a GameObject's component
        // list via SerializedObject. Deliberately avoids DeleteArrayElementAtIndex
        // on individual null entries - that method's behavior differs for an
        // element that starts non-null (first call nulls it, second removes it,
        // per Unity's documented quirk) versus one already null going in, and
        // guessing wrong here means silently deleting a live component instead
        // of a broken one. Instead: read out only the valid (non-null) component
        // references, clear the array completely, and rebuild it from just
        // those - fully deterministic, no ambiguity about call counts.
        private static bool RemoveMissingScripts(GameObject go)
        {
            // Read via the native (non-serialized) API first - accurate regardless
            // of any serialization snapshotting, and doesn't commit to a
            // SerializedObject view before we know whether a prefab disconnect
            // needs to happen first.
            Component[] existing = go.GetComponents<Component>();
            bool foundMissing = false;
            for (int i = 0; i < existing.Length; i++)
            {
                if (existing[i] == null)
                {
                    foundMissing = true;
                    break;
                }
            }

            if (!foundMissing)
            {
                return false;
            }

            // Diagnosed via WulframPrefabCheck: every object where an earlier
            // attempt at this fix silently failed to persist was
            // PrefabType.PrefabInstance - Unity's (pre-2018.3) prefab override
            // system was reverting the raw SerializedObject structural edit on
            // save. The one case that DID work was already
            // PrefabType.DisconnectedPrefabInstance. Disconnecting before editing
            // makes every instance behave like that working case.
            // Trade-off, stated plainly: these Cargo objects lose their live link
            // to the source prefab - future edits to the prefab asset won't
            // propagate to them anymore. Acceptable here since these are static
            // level placements, not something actively maintained via the prefab,
            // and the prefab relationship was already partially broken (one
            // instance was already disconnected before we touched anything).
            //
            // This must happen BEFORE creating any SerializedObject/SerializedProperty
            // for this GameObject - doing it after would leave those referencing a
            // stale pre-disconnect snapshot (exactly the bug in the first attempt
            // at this fix).
            if (PrefabUtility.GetPrefabType(go) == PrefabType.PrefabInstance)
            {
                PrefabUtility.DisconnectPrefabInstance(go);
            }

            // Re-read live components now, post-disconnect, then build the
            // SerializedObject fresh from the current state.
            existing = go.GetComponents<Component>();
            System.Collections.Generic.List<Object> keep = new System.Collections.Generic.List<Object>();
            for (int i = 0; i < existing.Length; i++)
            {
                if (existing[i] != null)
                {
                    keep.Add(existing[i]);
                }
            }

            SerializedObject so = new SerializedObject(go);
            SerializedProperty components = so.FindProperty("m_Component");

            components.ClearArray();
            for (int i = 0; i < keep.Count; i++)
            {
                components.InsertArrayElementAtIndex(i);
                SerializedProperty newElement = components.GetArrayElementAtIndex(i).FindPropertyRelative("component");
                newElement.objectReferenceValue = keep[i];
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(go);
            EditorSceneManager.MarkSceneDirty(go.scene);
            return true;
        }
    }
}
