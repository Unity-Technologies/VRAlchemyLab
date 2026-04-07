using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.ProjectWindowCallback;
using System.IO;

namespace UnityEditor.VFXToolbox.ImageSequencer
{
    internal class BuildTemplateAssetFactory
    {
        [MenuItem("Assets/Create/Build/Build Template", priority = BuildFrontend.CreateAssetMenuPriority)]
        private static void MenuCreatePostProcessingProfile()
        {
            var icon = EditorGUIUtility.FindTexture("BuildTemplate");

            // Unity 6: first parameter is EntityId, not int
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                default, // default(EntityId) – évite le cast obsolète depuis un int
                ScriptableObject.CreateInstance<DoCreateBuildTemplateAsset>(),
                "New BuildTemplate.asset",
                icon,
                null
            );
        }

        public static BuildTemplate CreateAssetAtPath(string path)
        {
            BuildTemplate asset = ScriptableObject.CreateInstance<BuildTemplate>();
            asset.name = Path.GetFileName(path);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }

    internal class DoCreateBuildTemplateAsset : AssetCreationEndAction
    {
        public override void Action(EntityId entityId, string pathName, string resourceFile)
        {
            BuildTemplate asset = BuildTemplateAssetFactory.CreateAssetAtPath(pathName);
            ProjectWindowUtil.ShowCreatedAsset(asset);
        }
    }
}
