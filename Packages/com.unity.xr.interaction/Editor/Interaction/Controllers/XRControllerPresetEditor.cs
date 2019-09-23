// Removed on 4/24/2018
// Needs to be re-added: https://github.com/Unity-Technologies/XR-Interaction/issues/16
/*
using UnityEngine;
using UnityEngine.XR.Interaction;

namespace UnityEditor.XR.Interaction
{
    [CustomEditor(typeof(XRControllerPreset))]
    internal class XRControllerPresetEditor : Editor
    {
        SerializedProperty m_PositionAction;
        SerializedProperty m_RotationAction;
        SerializedProperty m_SelectAction;
        SerializedProperty m_ActivateAction;
        SerializedProperty m_UIPressAction;
        SerializedProperty m_ModelPrefab;
        SerializedProperty m_AnimateModel;
        SerializedProperty m_ModelSelectTransition;
        SerializedProperty m_ModelDeSelectTransition;
        SerializedProperty m_HapticsDeviceName;

        static class Tooltips
        {
            public static readonly GUIContent positionAction = new GUIContent(
                "Position Action",
                "Input Action used to control position of controller.");

            public static readonly GUIContent rotationAction = new GUIContent(
                "Rotation Action",
                "Input Action used to control rotation of controller.");

            public static readonly GUIContent selectAction = new GUIContent(
                "Select Action",
                "Input Action used to control selection status of controller.");

            public static readonly GUIContent activateAction = new GUIContent(
                "Activate Action",
                "Input Action used to control activation status of controller.");

            public static readonly GUIContent uiPressAction = new GUIContent(
                "UI Press Action",
                "Input Action used to control press status of controller (used for communicating with UI).");

            public static readonly GUIContent modelPrefab = new GUIContent(
                "Model Prefab",
                "Controller model prefab to show.");

            public static readonly GUIContent animateModel = new GUIContent(
                "animateModel",
                "Whether this model animates in response to interaction events.");

            public static readonly GUIContent modelSelectTransition = new GUIContent(
                "modelSelectTransition",
                "The animation transition to enable when selecting.");

            public static readonly GUIContent modelDeSelectTransition = new GUIContent(
                "modelDeSelectTransition",
                "The animation transition to enable when de-selecting.");

            public static readonly GUIContent hapticsDeviceName = new GUIContent(
                "Haptics Device Name",
                "Haptics device name for playing back haptic feedback on this device.");
        }

        void OnEnable()
        {
            m_PositionAction = serializedObject.FindProperty("m_PositionAction");
            m_RotationAction = serializedObject.FindProperty("m_RotationAction");
            m_SelectAction = serializedObject.FindProperty("m_SelectAction");
            m_ActivateAction = serializedObject.FindProperty("m_ActivateAction");
            m_UIPressAction = serializedObject.FindProperty("m_UIPressAction");
            m_ModelPrefab = serializedObject.FindProperty("m_ModelPrefab");
            m_AnimateModel = serializedObject.FindProperty("m_AnimateModel");
            m_ModelSelectTransition = serializedObject.FindProperty("m_ModelSelectTransition");
            m_ModelDeSelectTransition = serializedObject.FindProperty("m_ModelDeSelectTransition");
            m_HapticsDeviceName = serializedObject.FindProperty("m_HapticsDeviceName");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_PositionAction, Tooltips.positionAction);
            EditorGUILayout.PropertyField(m_RotationAction, Tooltips.rotationAction);
            EditorGUILayout.PropertyField(m_SelectAction, Tooltips.selectAction);
            EditorGUILayout.PropertyField(m_ActivateAction, Tooltips.activateAction);
            EditorGUILayout.PropertyField(m_UIPressAction, Tooltips.uiPressAction);
            EditorGUILayout.PropertyField(m_ModelPrefab, Tooltips.modelPrefab);
            EditorGUILayout.PropertyField(m_AnimateModel, Tooltips.animateModel);
            if (m_AnimateModel.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_ModelSelectTransition, Tooltips.modelSelectTransition);
                EditorGUILayout.PropertyField(m_ModelDeSelectTransition, Tooltips.modelDeSelectTransition);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(m_HapticsDeviceName, Tooltips.hapticsDeviceName);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
*/