using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEditor.XR.Interaction.Toolkit
{
    [CustomEditor(typeof(XRRayInteractor))]
    [CanEditMultipleObjects]
    internal class XRRayInteractorEditor : Editor
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

        SerializedProperty m_MaxRaycastDistance;
        SerializedProperty m_HitDetectionType;
        SerializedProperty m_SphereCastRadius;        
        SerializedProperty m_RaycastMask;
        SerializedProperty m_RaycastTriggerInteraction;
        SerializedProperty m_HoverToSelect;
        SerializedProperty m_HoverTimeToSelect;

        SerializedProperty m_LineType;
        SerializedProperty m_EndPointDistance;
        SerializedProperty m_EndPointHeight;
        SerializedProperty m_ControlPointDistance;
        SerializedProperty m_ControlPointHeight;
        SerializedProperty m_SampleFrequency;

        SerializedProperty m_Velocity;
        SerializedProperty m_Acceleration;
        SerializedProperty m_AdditionalFlightTime;
        SerializedProperty m_ReferenceFrame;

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
            public static readonly GUIContent maxRaycastDistance = new GUIContent("Max Raycast Distance", "Max distance of ray cast. Increase this value will let you reach further.");
            public static readonly GUIContent sphereCastRadius = new GUIContent("Sphere Cast Radius", "Radius of this Interactor's ray, used for spherecasting.");
            public static readonly GUIContent raycastMask = new GUIContent("Raycast Mask", "Layer mask used for limiting raycast targets.");
            public static readonly GUIContent raycastTriggerInteraction = new GUIContent("Raycast Trigger Interaction", "Type of interaction with trigger colliders via raycast.");
            public static readonly GUIContent hoverToSelect = new GUIContent("Hover To Select", "If true, this interactor will simulate a Select event if hovered over an Interactable for some amount of time. Selection will be exited when the Interactor is no longer hovering over the Interactable.");
            public static readonly GUIContent hoverTimeToSelect = new GUIContent("Hover Time To Select", "Number of seconds for which this interactor must hover over an object to select it.");
            public static readonly GUIContent lineType = new GUIContent("Line Type", "Line type of the ray cast.");
            public static readonly GUIContent endPointDistance = new GUIContent("End Point Distance", "Increase this value distance will make the end of curve further from the start point.");
            public static readonly GUIContent controlPointDistance = new GUIContent("Control Point Distance", "Increase this value will make the peak of the curve further from the start point.");
            public static readonly GUIContent endPointHeight = new GUIContent("End Point Height", "Decrease this value will make the end of the curve drop lower relative to the start point.");
            public static readonly GUIContent controlPointHeight = new GUIContent("Control Point Height", "Increase this value will make the peak of the curve higher relative to the start point.");
            public static readonly GUIContent sampleFrequency = new GUIContent("Sample Frequency", "Gets or sets the number of sample points of the curve, should be at least 3, the higher the better quality.");
            public static readonly GUIContent velocity = new GUIContent("Velocity", "Initial velocity of the projectile. Increase this value will make the curve reach further.");
            public static readonly GUIContent acceleration = new GUIContent("Acceleration", "Gravity of the projectile in the reference frame.");
            public static readonly GUIContent additionalFlightTime = new GUIContent("Additional FlightTime", "Additional flight time after the projectile lands at the same height of the start point in the tracking space. Increase this value will make the end point drop lower in height.");
            public static readonly GUIContent referenceFrame = new GUIContent("Reference Frame", "The reference frame of the projectile. If not set it will try to find the XRRig object, if the XRRig does not exist it will use self");
            public static readonly GUIContent hitDetectionType = new GUIContent("Hit Detection Type", "The type of hit detection used to hit interactable objects.");        
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
            m_MaxRaycastDistance = serializedObject.FindProperty("m_MaxRaycastDistance");
            m_SphereCastRadius = serializedObject.FindProperty("m_SphereCastRadius");
            m_HitDetectionType = serializedObject.FindProperty("m_HitDetectionType");
            m_RaycastMask = serializedObject.FindProperty("m_RaycastMask");
            m_RaycastTriggerInteraction = serializedObject.FindProperty("m_RaycastTriggerInteraction");
            m_HoverToSelect = serializedObject.FindProperty("m_HoverToSelect");
            m_HoverTimeToSelect = serializedObject.FindProperty("m_HoverTimeToSelect");

            m_LineType = serializedObject.FindProperty("m_LineType");
            m_EndPointDistance = serializedObject.FindProperty("m_EndPointDistance");
            m_EndPointHeight = serializedObject.FindProperty("m_EndPointHeight");
            m_ControlPointDistance = serializedObject.FindProperty("m_ControlPointDistance");
            m_ControlPointHeight = serializedObject.FindProperty("m_ControlPointHeight");
            m_SampleFrequency = serializedObject.FindProperty("m_SampleFrequency");

            m_ReferenceFrame = serializedObject.FindProperty("m_ReferenceFrame");
            m_Velocity = serializedObject.FindProperty("m_Velocity");
            m_Acceleration = serializedObject.FindProperty("m_Acceleration");
            m_AdditionalFlightTime = serializedObject.FindProperty("m_AdditionalFlightTime");

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
            EditorGUILayout.PropertyField(m_ToggleSelect, Tooltips.toggleSelect);
            EditorGUILayout.PropertyField(m_HideControllerOnSelect, Tooltips.hideControllerOnSelect);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_LineType, Tooltips.lineType);

            EditorGUI.indentLevel++;
            if(m_LineType.enumValueIndex  == 0) // straight line
            {
                 EditorGUILayout.PropertyField(m_MaxRaycastDistance, Tooltips.maxRaycastDistance);
            }
            else if (m_LineType.enumValueIndex == 1) // projectile
            {
                EditorGUILayout.PropertyField(m_ReferenceFrame, Tooltips.referenceFrame);
                EditorGUILayout.PropertyField(m_Velocity, Tooltips.velocity);
                EditorGUILayout.PropertyField(m_Acceleration, Tooltips.acceleration);
                EditorGUILayout.PropertyField(m_AdditionalFlightTime, Tooltips.additionalFlightTime);
                EditorGUILayout.PropertyField(m_SampleFrequency, Tooltips.sampleFrequency);
            }

            else if (m_LineType.enumValueIndex == 2) // bezier
            {
                EditorGUILayout.PropertyField(m_EndPointDistance, Tooltips.endPointDistance);
                EditorGUILayout.PropertyField(m_EndPointHeight, Tooltips.endPointHeight);
                EditorGUILayout.PropertyField(m_ControlPointDistance, Tooltips.controlPointDistance);
                EditorGUILayout.PropertyField(m_ControlPointHeight, Tooltips.controlPointHeight);
                EditorGUILayout.PropertyField(m_SampleFrequency, Tooltips.sampleFrequency);
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_HitDetectionType, Tooltips.hitDetectionType);
            using (new EditorGUI.DisabledScope(m_HitDetectionType.enumValueIndex != (int)XRRayInteractor.HitDetectionType.SphereCast))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_SphereCastRadius, Tooltips.sphereCastRadius);
                EditorGUI.indentLevel--;
            }
        
            EditorGUILayout.Space();


            EditorGUILayout.PropertyField(m_RaycastMask, Tooltips.raycastMask);
            EditorGUILayout.PropertyField(m_RaycastTriggerInteraction, Tooltips.raycastTriggerInteraction);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_HoverToSelect, Tooltips.hoverToSelect);
            if (m_HoverToSelect.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_HoverTimeToSelect, Tooltips.hoverTimeToSelect);
                EditorGUI.indentLevel--;
            }

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

            m_ShowInteractorEvents = EditorGUILayout.Foldout(m_ShowInteractorEvents, "Interactor Events");

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
