using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wulfram.EditorTools
{
    /// <summary>
    /// Writes every serialized property of every component in a scene to a text file, one line each,
    /// so two versions of a binary scene can be compared with a plain diff. Object references are
    /// written as hierarchy paths (scene objects) or asset paths, never instance IDs, so the output is
    /// stable between runs. Read-only: the scene is never saved.
    ///
    ///   Unity.exe -batchmode -quit -projectPath <path>
    ///     -executeMethod Wulfram.EditorTools.WulframSceneDump.Dump -scenePath <scene> -dumpOut <file>
    /// </summary>
    public static class WulframSceneDump
    {
        public static void Dump()
        {
            string scenePath = Arg("-scenePath");
            string outPath = Arg("-dumpOut");
            if (scenePath == null || outPath == null)
            {
                Debug.LogError("WulframSceneDump: needs -scenePath and -dumpOut");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# scene " + scenePath + " dirtyAfterOpen=" + scene.isDirty);
            GameObject[] roots = scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
            {
                Transform[] all = roots[r].GetComponentsInChildren<Transform>(true);
                for (int t = 0; t < all.Length; t++)
                {
                    DumpObject(sb, all[t].gameObject);
                }
            }
            File.WriteAllText(outPath, sb.ToString());
            Debug.Log("WulframSceneDump: wrote " + outPath + " (dirtyAfterOpen=" + scene.isDirty + ")");
        }

        private static void DumpObject(StringBuilder sb, GameObject go)
        {
            string goPath = PathOf(go.transform);
            DumpProperties(sb, goPath + " |GameObject", go);
            Component[] components = go.GetComponents<Component>();
            Dictionary<string, int> seen = new Dictionary<string, int>();
            for (int c = 0; c < components.Length; c++)
            {
                if (components[c] == null)
                {
                    sb.AppendLine(goPath + " |<missing script> #" + c);
                    continue;
                }
                string type = components[c].GetType().Name;
                int n;
                seen.TryGetValue(type, out n);
                seen[type] = n + 1;
                DumpProperties(sb, goPath + " |" + type + (n > 0 ? "#" + n : ""), components[c]);
            }
        }

        private static void DumpProperties(StringBuilder sb, string prefix, Object target)
        {
            SerializedProperty it = new SerializedObject(target).GetIterator();
            bool enter = true;
            while (it.Next(enter))
            {
                enter = it.propertyType != SerializedPropertyType.String;
                if (it.propertyPath == "m_PrefabInternal" || it.propertyPath == "m_GameObject" || it.propertyPath == "m_ObjectHideFlags")
                {
                    continue;
                }
                sb.Append(prefix).Append(' ').Append(it.propertyPath).Append(" = ").AppendLine(Value(it));
            }
        }

        private static string Value(SerializedProperty p)
        {
            CultureInfo inv = CultureInfo.InvariantCulture;
            switch (p.propertyType)
            {
                case SerializedPropertyType.Integer: return p.longValue.ToString(inv);
                case SerializedPropertyType.Boolean: return p.boolValue.ToString();
                case SerializedPropertyType.Float: return p.doubleValue.ToString("R", inv);
                case SerializedPropertyType.String: return "\"" + p.stringValue + "\"";
                case SerializedPropertyType.Enum: return p.enumValueIndex.ToString(inv);
                case SerializedPropertyType.ArraySize: return p.intValue.ToString(inv);
                case SerializedPropertyType.Character: return p.intValue.ToString(inv);
                case SerializedPropertyType.LayerMask: return p.intValue.ToString(inv);
                case SerializedPropertyType.Color: return p.colorValue.ToString("R");
                case SerializedPropertyType.Vector2: return p.vector2Value.ToString("R");
                case SerializedPropertyType.Vector3: return p.vector3Value.ToString("R");
                case SerializedPropertyType.Vector4: return p.vector4Value.ToString("R");
                case SerializedPropertyType.Quaternion: return p.quaternionValue.ToString("R");
                case SerializedPropertyType.Rect: return p.rectValue.ToString("R");
                case SerializedPropertyType.Bounds: return p.boundsValue.ToString("R");
                case SerializedPropertyType.ObjectReference:
                    Object o = p.objectReferenceValue;
                    if (o == null)
                    {
                        return p.objectReferenceInstanceIDValue == 0 ? "null" : "<unresolved>";
                    }
                    string asset = AssetDatabase.GetAssetPath(o);
                    if (!string.IsNullOrEmpty(asset))
                    {
                        return o.GetType().Name + " " + asset + ":" + o.name;
                    }
                    Component comp = o as Component;
                    GameObject g = comp != null ? comp.gameObject : o as GameObject;
                    return o.GetType().Name + " " + (g != null ? PathOf(g.transform) : o.name);
                default:
                    return "<" + p.propertyType + ">";
            }
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

        private static string Arg(string name)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name)
                {
                    return args[i + 1];
                }
            }
            return null;
        }
    }
}
