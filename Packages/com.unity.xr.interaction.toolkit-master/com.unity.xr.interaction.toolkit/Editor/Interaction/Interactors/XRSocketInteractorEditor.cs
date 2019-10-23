using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEditor.XR.Interaction.Toolkit
{
    [CustomEditor(typeof(XRSocketInteractor))]
    internal class XRSocketInteractorEditor : Editor
    {
        SerializedProperty m_InteractionManager;
        SerializedProperty m_InteractionLayerMask;
        SerializedProperty m_AttachTransform;
        SerializedProperty m_StartingSelectedInteractable;

        SerializedProperty m_ShowInteractableHoverMeshes;
        SerializedProperty m_InteractableHoverMeshMaterial;
        SerializedProperty m_SocketActive;
        SerializedProperty m_InteractableHoverScale;

        SerializedProperty m_OnHoverEnter;
        SerializedProperty m_OnHoverExit;
        SerializedProperty m_OnSelectEnter;
        SerializedProperty m_OnSelectExit;
        bool m_ShowInteractorEvents;

        static class Tooltips
        {
            public static readonly GUIContent interactionManager = new GUIContent("Interaction Manager", "Manager to handle all interaction management (will find one if empty).");
            public static readonly GUIContent interactionLayerMask = new GUIContent("Interaction Layer Mask", "Only interactables with this Layer Mask will respond to this interactor.");
            public static readonly GUIContent attachTransform = new GUIContent("Attach Transform", "Attach Transform to use for this Interactor.  Will create empty GameObject if none set.");
            public static readonly GUIContent startingSelectedInteractable = new GUIContent("Starting Selected Interactable", "Interactable that will be selected upon start.");
            public static readonly GUIContent toggleSelect = new GUIContent("Toggle Select", "Toggle select on button press instead of hold.");

            public static readonly GUIContent showInteractableHoverMeshes = new GUIContent("Show Interactable Hover Meshes", "Show interactable's meshes at socket's attach point on hover.");
            public static readonly GUIContent interactableHoverMeshMaterial = new GUIContent("Interactable Hover Mesh Material", "Material used for rendering interactable meshes on hover (a default material will be created if none is supplied).");
            public static readonly GUIContent socketActive = new GUIContent("Socket Active", "Turn socket interaction on/off");
            public static readonly GUIContent interactableHoverScale = new GUIContent("Interactable Hover Scale", "Scale at which to render hovered interactable.");
        }

        void OnEnable()
        {
            m_InteractionManager = serializedObject.FindProperty("m_InteractionManager");
            m_InteractionLayerMask = serializedObject.FindProperty("m_InteractionLayerMask");
            m_AttachTransform = serializedObject.FindProperty("m_AttachTransform");
            m_StartingSelectedInteractable = serializedObject.FindProperty("m_StartingSelectedInteractable");

            m_ShowInteractableHoverMeshes = serializedObject.FindProperty("m_ShowInteractableHoverMeshes");
            m_InteractableHoverMeshMaterial = serializedObject.FindProperty("m_InteractableHoverMeshMaterial");
            m_SocketActive = serializedObject.FindProperty("m_SocketActive");
            m_InteractableHoverScale = serializedObject.FindProperty("m_InteractableHoverScale");

            m_OnHoverEnter = serializedObject.FindProperty("m_OnHoverEnter");
            m_OnHoverExit = serializedObject.FindProperty("m_OnHoverExit");
            m_OnSelectEnter = serializedObject.FindProperty("m_OnSelectEnter");
            m_OnSelectExit = serializedObject.FindProperty("m_OnSelectExit");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_InteractionManager, Tooltips.interactionManager);
            EditorGUILayout.PropertyField(m_InteractionLayerMask, Tooltips.interactionLayerMask);
            EditorGUILayout.PropertyField(m_AttachTransform, Tooltips.attachTransform);
            EditorGUILayout.PropertyField(m_StartingSelectedInteractable, Tooltips.startingSelectedInteractable);

            EditorGUILayout.PropertyField(m_ShowInteractableHoverMeshes, Tooltips.showInteractableHoverMeshes);
            EditorGUILayout.PropertyField(m_InteractableHoverMeshMaterial, Tooltips.interactableHoverMeshMaterial);
            EditorGUILayout.PropertyField(m_SocketActive, Tooltips.socketActive);
            EditorGUILayout.PropertyField(m_InteractableHoverScale, Tooltips.interactableHoverScale);

            m_ShowInteractorEvents = EditorGUILayout.Toggle("Show Interactor Events", m_ShowInteractorEvents);
            if (m_ShowInteractorEvents)
            {
                // UnityEvents have not yet supported Tooltips
                EditorGUILayout.PropertyField(m_OnHoverEnter);
                EditorGUILayout.PropertyField(m_OnHoverExit);
                EditorGUILayout.PropertyField(m_OnSelectEnter);
                EditorGUILayout.PropertyField(m_OnSelectExit);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
