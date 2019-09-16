using UnityEngine;
using UnityEngine.XR.Interaction;

namespace UnityEditor.XR.Interaction
{
    [CustomEditor(typeof(SnapTurnProvider))]
    internal class SnapTurnProviderEditor : Editor
    {
        SerializedProperty m_LocomotionSystem;
        SerializedProperty m_EnablePrimaryDevice;
        SerializedProperty m_PrimaryDeviceRole;
        SerializedProperty m_EnableSecondaryDevice;
        SerializedProperty m_SecondaryDeviceRole;
        SerializedProperty m_PrimaryDeviceTurnAxis;
        SerializedProperty m_SecondaryDeviceTurnAxis;
        SerializedProperty m_TurnAxis;
        SerializedProperty m_TurnAmount;
        SerializedProperty m_Duration;
        SerializedProperty m_DeadZone;
        SerializedProperty m_ActivateTimeout;

        static class Tooltips
        {
            public static readonly GUIContent locomotionSystem = new GUIContent(
                "System",
                "The locomotion system that the snap turn provider will interface with");
            
            public static readonly GUIContent enablePrimaryDevice = new GUIContent(
                "Enable Primary Device",
                "Enables the primary device for snap turn detection");

            public static readonly GUIContent enableSecondaryDevice = new GUIContent(
                 "Enable Secondary Device",
                 "Enables the secondary device for snap turn detection");

            public static readonly GUIContent deviceRole = new GUIContent(
               "Device Role",
               "Which device to use to read data for the snap turn");

            public static readonly GUIContent deviceTurnAxis = new GUIContent(
                "Turn Input Source",
                "The Input axis to use to begin a snap turn");

            public static readonly GUIContent turnAmount = new GUIContent(
                "Turn Amount",
                "the number of degrees to turn around the Y axis when performing a right handed snap turn. This will automatically be negated for left turns.");

            public static readonly GUIContent activateTimeout = new GUIContent(
                "Activation Timeout",
                "how long between a successful snap turn does the use need to wait before being able to perform a subsequent snap turn");

            public static readonly GUIContent deadZone = new GUIContent(
                "Dead Zone",
                "Minimum distance of axis travel before performing a snap turn");
        }


        void OnEnable()
        {
            m_LocomotionSystem = serializedObject.FindProperty("m_System");
            m_EnablePrimaryDevice = serializedObject.FindProperty("m_EnablePrimaryDevice");
            m_EnableSecondaryDevice = serializedObject.FindProperty("m_EnableSecondaryDevice");

            m_PrimaryDeviceRole = serializedObject.FindProperty("m_PrimaryDeviceRole");
            m_SecondaryDeviceRole = serializedObject.FindProperty("m_SecondaryDeviceRole");

            m_PrimaryDeviceTurnAxis = serializedObject.FindProperty("m_PrimaryDeviceTurnAxis");
            m_SecondaryDeviceTurnAxis = serializedObject.FindProperty("m_SecondaryDeviceTurnAxis");

            m_TurnAmount = serializedObject.FindProperty("m_TurnAmount");          
            m_DeadZone = serializedObject.FindProperty("m_DeadZone");
            m_ActivateTimeout = serializedObject.FindProperty("m_DebounceTime");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_LocomotionSystem, Tooltips.locomotionSystem);
            EditorGUILayout.PropertyField(m_EnablePrimaryDevice, Tooltips.enablePrimaryDevice);
            if(m_EnablePrimaryDevice.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_PrimaryDeviceRole, Tooltips.deviceRole);
                EditorGUILayout.PropertyField(m_PrimaryDeviceTurnAxis, Tooltips.deviceTurnAxis);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(m_EnableSecondaryDevice, Tooltips.enableSecondaryDevice);
            if (m_EnableSecondaryDevice.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_SecondaryDeviceRole, Tooltips.deviceRole);
                EditorGUILayout.PropertyField(m_SecondaryDeviceTurnAxis, Tooltips.deviceTurnAxis);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(m_TurnAmount, Tooltips.turnAmount);
            EditorGUILayout.PropertyField(m_DeadZone, Tooltips.deadZone);
            EditorGUILayout.PropertyField(m_ActivateTimeout, Tooltips.activateTimeout);

            serializedObject.ApplyModifiedProperties();
        }
    }
}