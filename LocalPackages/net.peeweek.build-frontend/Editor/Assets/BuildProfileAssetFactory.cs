using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.ProjectWindowCallback;
using System.IO;

namespace UnityEditor.VFXToolbox.ImageSequencer
{
    internal class BuildProfileAssetFactory
    {
        [MenuItem("Assets/Create/Build/Build Profile", priority = BuildFrontend.CreateAssetMenuPriority)]
        private static void MenuCreatePostProcessingProfile()
        {
            var icon = EditorGUIUtility.FindTexture("BuildProfile");

            // Unity 6 : premier paramètre = EntityId, pas int
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                default, // évite tout cast implicite int → EntityId
                ScriptableObject.CreateInstance<DoCreateBuildProfileAsset>(),
                "New BuildProfile.asset",
                icon,
                null
            );
        }

        public static BuildProfile CreateAssetAtPath(string path)
        {
            BuildProfile asset = ScriptableObject.CreateInstance<BuildProfile>();
            asset.name = Path.GetFileName(path);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }

    internal class DoCreateBuildProfileAsset : AssetCreationEndAction
    {
        public override void Action(EntityId entityId, string pathName, string resourceFile)
        {
            BuildProfile asset = BuildProfileAssetFactory.CreateAssetAtPath(pathName);
            ProjectWindowUtil.ShowCreatedAsset(asset);
        }
    }
}
