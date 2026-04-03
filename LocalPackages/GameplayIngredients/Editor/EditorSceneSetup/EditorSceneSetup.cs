using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor.Callbacks;

namespace GameplayIngredients.Editor
{
    public class EditorSceneSetup : ScriptableObject
    {
        [MenuItem("File/Save Scene Setup As... #%&S", priority = 171)]
        static void SaveSetup()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save EditorSceneSetup",
                "New EditorSceneSetup",
                "asset",
                "Save EditorSceneSetup?"
            );

            if (!string.IsNullOrEmpty(path))
            {
                EditorSceneSetup setup = GetCurrentSetup();
                AssetDatabase.CreateAsset(setup, path);
            }
        }

        public delegate void EditorSceneSetupLoadedDelegate(EditorSceneSetup setup);
        public static event EditorSceneSetupLoadedDelegate onSetupLoaded;

        // --- UNITY 6 FIX ---
        // Signatures valides : (EntityId), (EntityId, int), (EntityId, int, int)
        [OnOpenAsset]
        static bool OnOpenAsset(UnityEngine.EntityId entityId, int line)
        {
            // Conversion officielle Unity 6
            Object obj = EditorUtility.EntityIdToObject(entityId);

            if (obj is EditorSceneSetup setup)
            {
                try
                {
                    EditorUtility.DisplayProgressBar(
                        "Loading Scenes",
                        $"Loading Scene Setup {setup.name}...",
                        1.0f
                    );

                    RestoreSetup(setup);
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }

                return true;
            }

            return false;
        }
        // --------------------

        [MenuItem("Assets/Create/Editor Scene Setup", priority = 200)]
        static void CreateAsset()
        {
            AssetFactory.CreateAssetInProjectWindow<EditorSceneSetup>(
                "SceneSet Icon",
                "New SceneSetup.asset"
            );
        }

        public int ActiveScene;
        public EditorScene[] LoadedScenes;

        [System.Serializable]
        public struct EditorScene
        {
            public SceneAsset Scene;
            public bool Loaded;
        }

        public static EditorSceneSetup GetCurrentSetup()
        {
            var scenesetups = EditorSceneManager.GetSceneManagerSetup();
            var editorSetup = CreateInstance<EditorSceneSetup>();

            editorSetup.LoadedScenes = new EditorScene[scenesetups.Length];

            for (int i = 0; i < scenesetups.Length; i++)
            {
                var setup = scenesetups[i];

                if (setup.isActive)
                    editorSetup.ActiveScene = i;

                editorSetup.LoadedScenes[i].Scene =
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(setup.path);

                editorSetup.LoadedScenes[i].Loaded = setup.isLoaded;
            }

            return editorSetup;
        }

        public static void RestoreSetup(EditorSceneSetup editorSetup)
        {
            SceneSetup[] setups = new SceneSetup[editorSetup.LoadedScenes.Length];

            for (int i = 0; i < setups.Length; i++)
            {
                setups[i] = new SceneSetup
                {
                    path = AssetDatabase.GetAssetPath(editorSetup.LoadedScenes[i].Scene),
                    isLoaded = editorSetup.LoadedScenes[i].Loaded,
                    isActive = (editorSetup.ActiveScene == i)
                };
            }

            EditorSceneManager.RestoreSceneManagerSetup(setups);

            onSetupLoaded?.Invoke(editorSetup);
        }
    }
}
