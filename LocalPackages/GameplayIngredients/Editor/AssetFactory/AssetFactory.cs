using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.ProjectWindowCallback;
using System;
using System.IO;

namespace GameplayIngredients.Editor
{
    public class AssetFactory
    {
        public static void CreateAssetInProjectWindow<T>(string iconName, string fileName) where T : ScriptableObject
        {
            var icon = EditorGUIUtility.FindTexture(iconName);

            var namingInstance = ScriptableObject.CreateInstance<DoCreateGenericAsset>();
            namingInstance.type = typeof(T);

            // Unity 6 : premier paramètre = EntityId, pas int
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                default, // évite le cast implicite int → EntityId
                namingInstance,
                fileName,
                icon,
                null
            );
        }

        public static ScriptableObject CreateAssetAtPath(string path, Type type)
        {
            Debug.Log("CreateAssetAtPath (" + type.Name + ")");

            ScriptableObject asset = ScriptableObject.CreateInstance(type);
            asset.name = Path.GetFileName(path);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        class DoCreateGenericAsset : AssetCreationEndAction
        {
            public Type type;

            public override void Action(EntityId entityId, string pathName, string resourceFile)
            {
                ScriptableObject asset = AssetFactory.CreateAssetAtPath(pathName, type);
                ProjectWindowUtil.ShowCreatedAsset(asset);
            }
        }
    }
}
