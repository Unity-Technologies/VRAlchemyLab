using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class VRLabLoaderEditor
{
	static VRLabLoaderEditor()
	{
		EditorApplication.delayCall += LoadScenesSafely;
	}

	private static void LoadScenesSafely()
	{
		// Find the VRLab asset by type name
		string[] guids = AssetDatabase.FindAssets("t:MonoBehaviour VRLab");
		if (guids.Length == 0)
			return;

		string path = AssetDatabase.GUIDToAssetPath(guids[0]);
		Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);

		if (asset == null)
			return;

		SerializedObject so = new SerializedObject(asset);
		SerializedProperty loadedScenes = so.FindProperty("LoadedScenes");

		if (loadedScenes == null || !loadedScenes.isArray)
			return;

		for (int i = 0; i < loadedScenes.arraySize; i++)
		{
			SerializedProperty entry = loadedScenes.GetArrayElementAtIndex(i);
			SerializedProperty sceneProp = entry.FindPropertyRelative("Scene");
			SerializedProperty loadedProp = entry.FindPropertyRelative("Loaded");

			if (sceneProp == null || loadedProp == null)
				continue;

			if (!loadedProp.boolValue)
				continue;

			string sceneGuid = sceneProp.FindPropertyRelative("guid").stringValue;
			string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);

			if (string.IsNullOrEmpty(scenePath))
				continue;

			if (IsSceneOpen(scenePath))
				continue;

			EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
		}
	}

	private static bool IsSceneOpen(string scenePath)
	{
		for (int i = 0; i < EditorSceneManager.sceneCount; i++)
		{
			var s = EditorSceneManager.GetSceneAt(i);
			if (s.path == scenePath)
				return true;
		}
		return false;
	}
}
