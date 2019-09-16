using UnityEngine;
using UnityEngine.XR.Interaction;

namespace UnityEditor.XR.Interaction
{
    [CustomEditor(typeof(XRInteractorLineVisual))]
    [CanEditMultipleObjects]
    internal class XRInteractorLineVisualEditor : Editor
    {
        SerializedProperty m_LineWidth;
        SerializedProperty m_WidthCurve;
        SerializedProperty m_ValidColorGradient;
        SerializedProperty m_InvalidColorGradient;
        SerializedProperty m_SmoothMovement;
        SerializedProperty m_FollowTightness;
        SerializedProperty m_SnapThresholdDistance;
        SerializedProperty m_Reticle;
        
        static class Tooltips
        {
            public static readonly GUIContent lineWidth = new GUIContent("Line Width", "The width of the line (in centimeters).");
            public static readonly GUIContent widthCurve = new GUIContent("Width Curve", "The relative width of the line from the start to the end.");
            public static readonly GUIContent validColorGradient = new GUIContent("Valid Color Gradient", "The color of the line as a gradient from start to end to indicate a valid state.");
            public static readonly GUIContent invalidColorGradient = new GUIContent("Invalid Color Gradient", "The color of the line as a gradient from start to end to indicate an invalid state.");
            public static readonly GUIContent smoothMovement = new GUIContent("Smooth Movement", "If enabled, the rendered segments will be delayed from and smoothly follow the target segments.");
            public static readonly GUIContent followTightness = new GUIContent("Follow Tightness", "Sets the speed that the rendered segments will follow the target segments.");
            public static readonly GUIContent snapThresholdDistance = new GUIContent("Snap Threshold Distance", "Sets the threshold distance between line points at two consecutive frames to snap rendered segments to target segments.");
            public static readonly GUIContent reticle = new GUIContent("Reticle", "The reticle that will appear at the end of the valid line.");
        }

        void OnEnable()
        {
            m_LineWidth = serializedObject.FindProperty("m_LineWidth");
            m_WidthCurve = serializedObject.FindProperty("m_WidthCurve");
            m_ValidColorGradient = serializedObject.FindProperty("m_ValidColorGradient");
            m_InvalidColorGradient = serializedObject.FindProperty("m_InvalidColorGradient");
            m_SmoothMovement = serializedObject.FindProperty("m_SmoothMovement");
            m_FollowTightness = serializedObject.FindProperty("m_FollowTightness");
            m_SnapThresholdDistance = serializedObject.FindProperty("m_SnapThresholdDistance");
            m_Reticle = serializedObject.FindProperty("m_Reticle");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_LineWidth, Tooltips.lineWidth);
            EditorGUILayout.PropertyField(m_WidthCurve, Tooltips.widthCurve);
            EditorGUILayout.PropertyField(m_ValidColorGradient, Tooltips.validColorGradient);
            EditorGUILayout.PropertyField(m_InvalidColorGradient, Tooltips.invalidColorGradient);
            EditorGUILayout.PropertyField(m_Reticle, Tooltips.reticle);
            EditorGUILayout.PropertyField(m_SmoothMovement, Tooltips.smoothMovement);

            if (m_SmoothMovement.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_FollowTightness, Tooltips.followTightness);
                EditorGUILayout.PropertyField(m_SnapThresholdDistance, Tooltips.snapThresholdDistance);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

    }


}
