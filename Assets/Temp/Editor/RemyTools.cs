using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class RemyTools : EditorWindow
{
    [MenuItem("Tools/Remy Tools")]
    public static void OpenWindow()
    {
        GetWindow<RemyTools>();
    }

    Shader shader;

    Dictionary<Material, List<GameObject>> matches = new();

    private void OnGUI()
    {
        shader = (Shader) EditorGUILayout.ObjectField(shader, typeof(Shader), false);

        if (GUILayout.Button("Search"))
        {
            matches?.Clear();
            var renderers = Resources.FindObjectsOfTypeAll<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                foreach(Material material in renderer.sharedMaterials)
                {
                    if (material.shader == shader)
                    {
                        if (!matches.ContainsKey(material))
                            matches.Add(material, new List<GameObject>());

                        matches[material].Add(renderer.gameObject);
                    }
                }
            }
        }

        if (matches != null && matches.Count > 0)
        {
            foreach(var match in matches)
            {
                EditorGUILayout.ObjectField(match.Key, typeof(Material), false);
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                GUILayout.BeginVertical();
                foreach(var obj in match.Value)
                {
                    EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }
    }
}
