using System.Collections;
using UnityEngine;
using UnityEditor;

namespace UnityEngine.XR.Interaction.Toolkit
{
    [CustomEditor(typeof(XRControllerRecorder))]
    class XRControllerRecorderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var controllerRecorder = (XRControllerRecorder)target;
            DrawDefaultInspector();

            // show playback controls
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                if (controllerRecorder.isRecording)
                {
                    if (GUILayout.Button("Stop Recording"))
                        controllerRecorder.isRecording = false;
                }
                else
                {
                    if (GUILayout.Button("Record Input"))
                        controllerRecorder.isRecording = true;
                }

                if (controllerRecorder.isPlaying)
                {
                    if (GUILayout.Button("Stop"))
                        controllerRecorder.isPlaying = false;
                }
                else
                {
                    if (GUILayout.Button("Play"))
                        controllerRecorder.isPlaying = true;
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.HorizontalSlider((float)controllerRecorder.currentTime, 0.0f, (float)controllerRecorder.duration);
            }
        }
    }
}
