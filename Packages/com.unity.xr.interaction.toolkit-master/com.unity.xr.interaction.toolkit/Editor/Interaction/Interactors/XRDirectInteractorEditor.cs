using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEditor.XR.Interaction.Toolkit
{
    [CustomEditor(typeof(XRDirectInteractor))]
    internal class XRDirectInteractorEditor : Editor
    {
        SerializedProperty m_InteractionManager;
        SerializedProperty m_InteractionLayerMask;
        SerializedProperty m_AttachTransform;
        SerializedProperty m_StartingSelectedInteractable;
        SerializedProperty m_ToggleSelect;
        SerializedProperty m_HideControllerOnSelect;

        SerializedProperty m_PlayAudioClipOnSelectEnter;
        SerializedProperty m_AudioClipForOnSelectEnter;
        SerializedProperty m_PlayAudioClipOnSelectExit;
        SerializedProperty m_AudioClipForOnSelectExit;
        SerializedProperty m_PlayAudioClipOnHoverEnter;
        SerializedProperty m_AudioClipForOnHoverEnter;
        SerializedProperty m_PlayAudioClipOnHoverExit;
        SerializedProperty m_AudioClipForOnHoverExit;

        SerializedProperty m_PlayHapticsOnSelectEnter;
        SerializedProperty m_HapticSelectEnterIntensity;
        SerializedProperty m_HapticSelectEnterDuration;
        SerializedProperty m_PlayHapticsOnHoverEnter;
        SerializedProperty m_HapticHoverEnterIntensity;
        SerializedProperty m_HapticHoverEnterDuration;
        SerializedProperty m_PlayHapticsOnSelectExit;
        SerializedProperty m_HapticSelectExitIntensity;
        SerializedProperty m_HapticSelectExitDuration;
        SerializedProperty m_PlayHapticsOnHoverExit;
        SerializedProperty m_HapticHoverExitIntensity;
        SerializedProperty m_HapticHoverExitDuration;

        SerializedProperty m_OnHoverEnter;
        SerializedProperty m_OnHoverExit;
        SerializedProperty m_OnSelectEnter;
        SerializedProperty m_OnSelectExit;
        bool m_ShowInteractorEvents;
        bool m_ShowSoundEvents = false;
        bool m_ShowHapticEvents = false;

        static class Tooltips
        {
            public static readonly GUIContent interactionManager = new GUIContent("Interaction Manager", "Manager to handle all interaction management (will find one if empty).");
            public static readonly GUIContent interactionLayerMask = new GUIContent("Interaction Layer Mask", "Only interactables with this Layer Mask will respond to this interactor.");
            public static readonly GUIContent attachTransform = new GUIContent("Attach Transform", "Attach Transform to use for this Interactor.  Will create empty GameObject if none set.");
            public static readonly GUIContent startingSelectedInteractable = new GUIContent("Starting Selected Interactable", "Interactable that will be selected upon start.");
            public static readonly GUIContent toggleSelect = new GUIContent("Toggle Select", "Toggle select on button press instead of hold.");
            public static readonly GUIContent hideControllerOnSelect = new GUIContent("Hide Controller On Select", "Hide controller on select.");
            public static readonly GUIContent PlayAudioClipOnSelectEnter = new GUIContent("Play AudioClip On Select Enter", "Play an audio clip when the Select state is entered.");
            public static readonly GUIContent AudioClipForOnSelectEnter = new GUIContent("AudioClip To Play On Select Enter", "The audio clip to play when the Select state is entered.");
            public static readonly GUIContent PlayAudioClipOnSelectExit = new GUIContent("Play AudioClip On Select Exit", "Play an audio clip when the Select state is exited.");
            public static readonly GUIContent AudioClipForOnSelectExit = new GUIContent("AudioClip To Play On Select Exit", "The audio clip to play when the Select state is exited.");
            public static readonly GUIContent PlayAudioClipOnHoverEnter = new GUIContent("Play AudioClip On Hover Enter", "Play an audio clip when the Hover state is entered.");
            public static readonly GUIContent AudioClipForOnHoverEnter = new GUIContent("AudioClip To Play On Hover Enter", "The audio clip to play when the Hover state is entered.");
            public static readonly GUIContent PlayAudioClipOnHoverExit = new GUIContent("Play AudioClip On Hover Exit", "Play an audio clip when the Hover state is exited.");
            public static readonly GUIContent AudioClipForOnHoverExit = new GUIContent("AudioClip To Play On Hover Exit", "The audio clip to play when the Hover state is exited.");
            public static readonly GUIContent playHapticsOnSelectEnter = new GUIContent("Play Haptics On Select Enter", "Play haptics when the select state is entered.");
            public static readonly GUIContent hapticSelectEnterIntensity = new GUIContent("Haptic Select Enter Intensity", "Haptics intensity to play when the select state is entered.");
            public static readonly GUIContent hapticSelectEnterDuration = new GUIContent("Haptic Select Enter Duration", "Haptics Duration to play when the select state is entered.");
            public static readonly GUIContent playHapticsOnHoverEnter = new GUIContent("Play Haptics On Hover Enter", "Play haptics when the hover state is entered.");
            public static readonly GUIContent hapticHoverEnterIntensity = new GUIContent("Haptic Hover Enter Intensity", "Haptics intensity to play when the hover state is entered.");
            public static readonly GUIContent hapticHoverEnterDuration = new GUIContent("Haptic Hover Enter Duration", "Haptics Duration to play when the hover state is entered.");
            public static readonly GUIContent playHapticsOnSelectExit = new GUIContent("Play Haptics On Select Exit", "Play haptics when the select state is exited.");
            public static readonly GUIContent hapticSelectExitIntensity = new GUIContent("Haptic Select Exit Intensity", "Haptics intensity to play when the select state is exited.");
            public static readonly GUIContent hapticSelectExitDuration = new GUIContent("Haptic Select Exit Duration", "Haptics Duration to play when the select state is exited.");
            public static readonly GUIContent playHapticsOnHoverExit = new GUIContent("Play Haptics On Hover Exit", "Play haptics when the hover state is exited.");
            public static readonly GUIContent hapticHoverExitIntensity = new GUIContent("Haptic Hover Exit Intensity", "Haptics intensity to play when the hover state is exited.");
            public static readonly GUIContent hapticHoverExitDuration = new GUIContent("Haptic Hover Exit Duration", "Haptics Duration to play when the hover state is exited.");
        }

        void OnEnable()
        {
            m_InteractionManager = serializedObject.FindProperty("m_InteractionManager");
            m_InteractionLayerMask = serializedObject.FindProperty("m_InteractionLayerMask");
            m_AttachTransform = serializedObject.FindProperty("m_AttachTransform");
            m_StartingSelectedInteractable = serializedObject.FindProperty("m_StartingSelectedInteractable");
            m_ToggleSelect = serializedObject.FindProperty("m_ToggleSelect");
            m_HideControllerOnSelect = serializedObject.FindProperty("m_HideControllerOnSelect");
            m_PlayAudioClipOnSelectEnter = serializedObject.FindProperty("m_PlayAudioClipOnSelectEnter");
            m_AudioClipForOnSelectEnter = serializedObject.FindProperty("m_AudioClipForOnSelectEnter");
            m_PlayAudioClipOnSelectExit = serializedObject.FindProperty("m_PlayAudioClipOnSelectExit");
            m_AudioClipForOnSelectExit = serializedObject.FindProperty("m_AudioClipForOnSelectExit");
            m_PlayAudioClipOnHoverEnter = serializedObject.FindProperty("m_PlayAudioClipOnHoverEnter");
            m_AudioClipForOnHoverEnter = serializedObject.FindProperty("m_AudioClipForOnHoverEnter");
            m_PlayAudioClipOnHoverExit = serializedObject.FindProperty("m_PlayAudioClipOnHoverExit");
            m_AudioClipForOnHoverExit = serializedObject.FindProperty("m_AudioClipForOnHoverExit");
            m_PlayHapticsOnSelectEnter = serializedObject.FindProperty("m_PlayHapticsOnSelectEnter");
            m_HapticSelectEnterIntensity = serializedObject.FindProperty("m_HapticSelectEnterIntensity");
            m_HapticSelectEnterDuration = serializedObject.FindProperty("m_HapticSelectEnterDuration");
            m_PlayHapticsOnHoverEnter = serializedObject.FindProperty("m_PlayHapticsOnHoverEnter");
            m_HapticHoverEnterIntensity = serializedObject.FindProperty("m_HapticHoverEnterIntensity");
            m_HapticHoverEnterDuration = serializedObject.FindProperty("m_HapticHoverEnterDuration");
            m_PlayHapticsOnSelectExit = serializedObject.FindProperty("m_PlayHapticsOnSelectExit");
            m_HapticSelectExitIntensity = serializedObject.FindProperty("m_HapticSelectExitIntensity");
            m_HapticSelectExitDuration = serializedObject.FindProperty("m_HapticSelectExitDuration");
            m_PlayHapticsOnHoverExit = serializedObject.FindProperty("m_PlayHapticsOnHoverExit");
            m_HapticHoverExitIntensity = serializedObject.FindProperty("m_HapticHoverExitIntensity");
            m_HapticHoverExitDuration = serializedObject.FindProperty("m_HapticHoverExitDuration");

            m_OnSelectEnter = serializedObject.FindProperty("m_OnSelectEnter");
            m_OnSelectExit = serializedObject.FindProperty("m_OnSelectExit");
            m_OnHoverEnter = serializedObject.FindProperty("m_OnHoverEnter");
            m_OnHoverExit = serializedObject.FindProperty("m_OnHoverExit");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_InteractionManager, Tooltips.interactionManager);
            EditorGUILayout.PropertyField(m_InteractionLayerMask, Tooltips.interactionLayerMask);
            EditorGUILayout.PropertyField(m_AttachTransform, Tooltips.attachTransform);
            EditorGUILayout.PropertyField(m_StartingSelectedInteractable, Tooltips.startingSelectedInteractable);
            EditorGUILayout.PropertyField(m_ToggleSelect, Tooltips.toggleSelect);
            EditorGUILayout.PropertyField(m_HideControllerOnSelect, Tooltips.hideControllerOnSelect);

            EditorGUILayout.Space();

            m_ShowSoundEvents = EditorGUILayout.Foldout(m_ShowSoundEvents, "Sound Events");
            if (m_ShowSoundEvents)
            {

                EditorGUILayout.PropertyField(m_PlayAudioClipOnSelectEnter, Tooltips.PlayAudioClipOnSelectEnter);
                if (m_PlayAudioClipOnSelectEnter.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_AudioClipForOnSelectEnter, Tooltips.AudioClipForOnSelectEnter);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(m_PlayAudioClipOnSelectExit, Tooltips.PlayAudioClipOnSelectExit);
                if (m_PlayAudioClipOnSelectExit.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_AudioClipForOnSelectExit, Tooltips.AudioClipForOnSelectExit);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(m_PlayAudioClipOnHoverEnter, Tooltips.PlayAudioClipOnHoverEnter);
                if (m_PlayAudioClipOnHoverEnter.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_AudioClipForOnHoverEnter, Tooltips.AudioClipForOnHoverEnter);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(m_PlayAudioClipOnHoverExit, Tooltips.PlayAudioClipOnHoverExit);
                if (m_PlayAudioClipOnHoverExit.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_AudioClipForOnHoverExit, Tooltips.AudioClipForOnHoverExit);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.Space();

            m_ShowHapticEvents = EditorGUILayout.Foldout(m_ShowHapticEvents, "Haptic Events");
            if (m_ShowHapticEvents)
            {
                EditorGUILayout.PropertyField(m_PlayHapticsOnSelectEnter, Tooltips.playHapticsOnSelectEnter);
                if (m_PlayHapticsOnSelectEnter.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_HapticSelectEnterIntensity, Tooltips.hapticSelectEnterIntensity);
                    EditorGUILayout.PropertyField(m_HapticSelectEnterDuration, Tooltips.hapticSelectEnterDuration);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(m_PlayHapticsOnSelectExit, Tooltips.playHapticsOnSelectExit);
                if (m_PlayHapticsOnSelectExit.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_HapticSelectExitIntensity, Tooltips.hapticSelectExitIntensity);
                    EditorGUILayout.PropertyField(m_HapticSelectExitDuration, Tooltips.hapticSelectExitDuration);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(m_PlayHapticsOnHoverEnter, Tooltips.playHapticsOnHoverEnter);
                if (m_PlayHapticsOnHoverEnter.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_HapticHoverEnterIntensity, Tooltips.hapticHoverEnterIntensity);
                    EditorGUILayout.PropertyField(m_HapticHoverEnterDuration, Tooltips.hapticHoverEnterDuration);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(m_PlayHapticsOnHoverExit, Tooltips.playHapticsOnHoverExit);
                if (m_PlayHapticsOnHoverExit.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_HapticHoverExitIntensity, Tooltips.hapticHoverExitIntensity);
                    EditorGUILayout.PropertyField(m_HapticHoverExitDuration, Tooltips.hapticHoverExitDuration);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.Space();

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
