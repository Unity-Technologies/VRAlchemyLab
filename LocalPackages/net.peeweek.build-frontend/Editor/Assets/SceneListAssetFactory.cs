using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.ProjectWindowCallback;
using System.IO;

namespace UnityEditor.VFXToolbox.ImageSequencer
{
    internal class SceneListAssetFactory
    {
        [MenuItem("Assets/Create/Build/Scene List", priority = BuildFrontend.CreateAssetMenuPriority)]
        private static void MenuCreatePostProcessingProfile()
        {
            var icon = EditorGUIUtility.FindTexture("SceneList");

            // Use EntityId (no implicit int cast)
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                default, // EntityId default value
                ScriptableObject.CreateInstance<DoCreateSceneListAsset>(),
                "New SceneList.asset",
                icon,
                null
            );
        }

        public static SceneList CreateAssetAtPath(string path)
        {
            SceneList asset = ScriptableObject.CreateInstance<SceneList>();
            asset.name = Path.GetFileName(path);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }

    internal class DoCreateSceneListAsset : AssetCreationEndAction
    {
        public override void Action(EntityId entityId, string pathName, string resourceFile)
        {
            SceneList asset = SceneListAssetFactory.CreateAssetAtPath(pathName);
            ProjectWindowUtil.ShowCreatedAsset(asset);
        }
    }
}
