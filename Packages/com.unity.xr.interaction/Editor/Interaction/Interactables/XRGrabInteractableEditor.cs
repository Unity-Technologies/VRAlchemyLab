using UnityEngine;
using UnityEngine.XR.Interaction;

namespace UnityEditor.XR.Interaction
{
    [CustomEditor(typeof(XRGrabInteractable))]
    internal class XRGrabInteractableEditor : Editor
    {
        SerializedProperty m_AttachTransform;
        SerializedProperty m_AttachEaseInTime;
        SerializedProperty m_MovementType;
        SerializedProperty m_TrackPosition;
        SerializedProperty m_SmoothPosition;
        SerializedProperty m_SmoothPositionAmount;
        SerializedProperty m_TightenPosition;
        SerializedProperty m_TrackRotation;
        SerializedProperty m_SmoothRotation;
        SerializedProperty m_SmoothRotationAmount;
        SerializedProperty m_TightenRotation;
        SerializedProperty m_ThrowOnDetach;
        SerializedProperty m_ThrowSmoothingDuration;
        SerializedProperty m_ThrowSmoothingCurve;
        SerializedProperty m_ThrowVelocityScale;
        SerializedProperty m_ThrowAngularVelocityScale;
        SerializedProperty m_GravityOnDetach;

        static class Tooltips
        {
            public static readonly GUIContent attachTransform = new GUIContent("Attach Transform", "Attach point to use on this Interactable (will use RigidBody center if none set).");
            public static readonly GUIContent attachEaseInTime = new GUIContent("Attach Ease In Time", "Time it takes to ease in the attach (time of 0.0 indicates no easing).");
            public static readonly GUIContent movementType = new GUIContent("Movement Type", "Type of movement for RigidBody.");
            public static readonly GUIContent trackPosition = new GUIContent("Track Position", "Whether the this interactable should track the position of the interactor.");
            public static readonly GUIContent smoothPosition = new GUIContent("Smooth Position", "Apply smoothing to the follow position of the object.");
            public static readonly GUIContent smoothPositionAmount = new GUIContent("Smooth Position Amount", "Smoothing applied to the object's position when following.");
            public static readonly GUIContent tightenPosition = new GUIContent("Tighten Position", "Reduces the maximum follow position difference when using smoothing.");
            public static readonly GUIContent trackRotation = new GUIContent("Track Rotation", "Whether the this interactable should track the rotation of the interactor.");
            public static readonly GUIContent smoothRotation = new GUIContent("Smooth Rotation", "Apply smoothing to the follow rotation of the object.");
            public static readonly GUIContent smoothRotationAmount = new GUIContent("Smooth Rotation Amount", "Smoothing multiple applied to the object's rotation when following.");
            public static readonly GUIContent tightenRotation = new GUIContent("Tighten Rotation", "Reduces the maximum follow rotation difference when using smoothing.");
            public static readonly GUIContent throwOnDetach = new GUIContent("Throw On Detach", "Object inherits the interactor's velocity when released.");
            public static readonly GUIContent throwSmoothingDuration = new GUIContent("Throw Smoothing Duration", "Time period to average thrown velocity over");
            public static readonly GUIContent throwSmoothingCurve = new GUIContent("ThrowSmoothingCurve", "The curve to use to weight velocity smoothing (most recent frames to the right.");
            public static readonly GUIContent throwVelocityScale = new GUIContent("Throw Velocity Scale", "Scale the velocity used when throwing.");
            public static readonly GUIContent throwAngularVelocityScale = new GUIContent("Throw Angular Velocity Scale", "Scale the angular velocity used when throwing");
            public static readonly GUIContent gravityOnDetach = new GUIContent("Gravity On Detach", "Object has gravity when released (will still use pre-grab value if this is false).");
        }

        void OnEnable()
        {
            m_AttachTransform = serializedObject.FindProperty("m_AttachTransform");
            m_MovementType = serializedObject.FindProperty("m_MovementType");
            m_AttachEaseInTime = serializedObject.FindProperty("m_AttachEaseInTime");
            m_TrackPosition = serializedObject.FindProperty("m_TrackPosition");
            m_SmoothPosition = serializedObject.FindProperty("m_SmoothPosition");
            m_SmoothPositionAmount = serializedObject.FindProperty("m_SmoothPositionAmount");
            m_TightenPosition = serializedObject.FindProperty("m_TightenPosition");
            m_TrackRotation = serializedObject.FindProperty("m_TrackRotation");
            m_SmoothRotation = serializedObject.FindProperty("m_SmoothRotation");
            m_SmoothRotationAmount = serializedObject.FindProperty("m_SmoothRotationAmount");
            m_TightenRotation = serializedObject.FindProperty("m_TightenRotation");
            m_ThrowOnDetach = serializedObject.FindProperty("m_ThrowOnDetach");
            m_ThrowSmoothingDuration = serializedObject.FindProperty("m_ThrowSmoothingDuration");
            m_ThrowSmoothingCurve = serializedObject.FindProperty("m_ThrowSmoothingCurve");
            m_ThrowVelocityScale = serializedObject.FindProperty("m_ThrowVelocityScale");
            m_ThrowAngularVelocityScale = serializedObject.FindProperty("m_ThrowAngularVelocityScale");
            m_GravityOnDetach =  serializedObject.FindProperty("m_GravityOnDetach");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_AttachTransform, Tooltips.attachTransform);
            EditorGUILayout.PropertyField(m_AttachEaseInTime, Tooltips.attachEaseInTime);
            EditorGUILayout.PropertyField(m_MovementType, Tooltips.movementType);
            
            EditorGUILayout.PropertyField(m_TrackPosition, Tooltips.trackPosition);
            if (m_TrackPosition.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_SmoothPosition, Tooltips.smoothPosition);
                if (m_SmoothPosition.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_SmoothPositionAmount, Tooltips.smoothPositionAmount);
                    EditorGUILayout.PropertyField(m_TightenPosition, Tooltips.tightenPosition);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(m_TrackRotation, Tooltips.trackRotation);
            if (m_TrackRotation.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_SmoothRotation, Tooltips.smoothRotation);
                if (m_SmoothRotation.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_SmoothRotationAmount, Tooltips.smoothRotationAmount);
                    EditorGUILayout.PropertyField(m_TightenRotation, Tooltips.tightenRotation);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(m_ThrowOnDetach, Tooltips.throwOnDetach);
            if (m_ThrowOnDetach.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_ThrowSmoothingDuration, Tooltips.throwSmoothingDuration);
                EditorGUILayout.PropertyField(m_ThrowSmoothingCurve, Tooltips.throwSmoothingCurve);
                EditorGUILayout.PropertyField(m_ThrowVelocityScale, Tooltips.throwVelocityScale);
                EditorGUILayout.PropertyField(m_ThrowAngularVelocityScale, Tooltips.throwAngularVelocityScale);
                EditorGUILayout.PropertyField(m_GravityOnDetach, Tooltips.gravityOnDetach);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
