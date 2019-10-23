using UnityEngine;
using UnityEditor;

namespace UnityEngine.XR.Interaction.Toolkit
{
    [CustomEditor(typeof(XRControllerRecording))]
    class XRControllerRecordingEditor : Editor 
    { 
        private XRControllerRecording controllerRecording;
     
    	void Awake()
    	{
    		controllerRecording = (XRControllerRecording)target;
    	}
     
    	public override void OnInspectorGUI()
    	{
            if (GUILayout.Button("Clear Recording"))
                controllerRecording.frames.Clear();

            GUILayout.Label("Frames");
    		GUILayout.BeginVertical();
            DisplayRecordingFrames();
            GUILayout.Space(5);
            GUILayout.EndVertical();
        }
     
        void DisplayRecordingFrames()
        {
            for (int i = 0; i < controllerRecording.frames.Count; i++ )
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.TextField(controllerRecording.frames[i].time.ToString(), GUILayout.Width(80));
                GUILayout.TextField(controllerRecording.frames[i].position.ToString(), GUILayout.Width(100));
                GUILayout.TextField(controllerRecording.frames[i].rotation.ToString(), GUILayout.Width(160));
                GUILayout.TextField(controllerRecording.frames[i].selectActive.ToString(), GUILayout.Width(40));
                GUILayout.TextField(controllerRecording.frames[i].pressActive.ToString(), GUILayout.Width(40));
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}