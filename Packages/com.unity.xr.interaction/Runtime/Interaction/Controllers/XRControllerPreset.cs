// Removed on 4/24/2018
// Needs to be re-added: https://github.com/Unity-Technologies/XR-Interaction/issues/15
/*
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace UnityEngine.XR.Interaction
{
    /// <summary>
    /// Preset ScriptableObject that overrides the behavior of a controller.
    /// It is possible to setup this object in advance to provide specific bindings and models for a controller.
    /// </summary>
    [CreateAssetMenu(menuName = "XR/XR Controller Preset")]
    public class XRControllerPreset : ScriptableObject 
    {
        [Header("Input Actions")]

        [SerializeField, Tooltip("Action used to control position of controller.")]
        InputAction m_PositionAction;
        /// <summary>Get action used to control position of controller.</summary>
        public InputAction positionAction { get { return m_PositionAction; } }

        [SerializeField, Tooltip("Action used to control rotation of controller.")]
        InputAction m_RotationAction;
        /// <summary>Get action used to control rotation of controller.</summary>
        public InputAction rotationAction { get { return m_RotationAction; } }

        [SerializeField, Tooltip("Action used to control selection status of controller.")]
        InputAction m_SelectAction;
        /// <summary>Get action used to control select status of controller.</summary>
        public InputAction selectAction { get { return m_SelectAction; } }

        [SerializeField, Tooltip("Action used to control activation status of controller.")]
        InputAction m_ActivateAction;
        /// <summary>Get action used to control activate status of controller.</summary>
        public InputAction activateAction { get { return m_ActivateAction; } }

        [SerializeField, Tooltip("Action used to control ui press status of controller (used for communicating with Canvas UI).")]
        InputAction m_UIPressAction;
        /// <summary>Get action used to control uIPress status of controller (used for communicating with Canvas UI).</summary>
        public InputAction uiPressAction { get { return m_UIPressAction; } }

        [Header("Model")]

        [SerializeField, Tooltip("Controller model prefab to show.")]
        Transform m_ModelPrefab;
        /// <summary>Get prefab to show for this controller's model.</summary>
        public Transform modelPrefab { get { return m_ModelPrefab; } }

        [SerializeField, Tooltip("Whether this model animates in response to interaction events.")]
        bool m_AnimateModel;
        /// <summary>Gets or sets whether this model animates in response to interaction events.</summary>
        public bool animateModel { get { return m_AnimateModel; } set { m_AnimateModel = value; } }

        [SerializeField, Tooltip("The animation transition to enable when selecting.")]
        string m_ModelSelectTransition;
        /// <summary>Gets or sets the animation transition to enable when selecting.</summary>
        public string modelSelectTransition { get { return m_ModelSelectTransition; } set { m_ModelSelectTransition = value; } }

        [SerializeField, Tooltip("The animation transition to enable when de-selecting.")]
        string m_ModelDeSelectTransition;
        /// <summary>Gets or sets the animation transition to enable when de-selecting.</summary>
        public string modelDeSelectTransition { get { return m_ModelDeSelectTransition; } set { m_ModelDeSelectTransition = value; } }

        [Header("Haptics")]

        [SerializeField, Tooltip("Haptics device name for playing back haptic feedback on this device.")]
        string m_HapticsDeviceName;
        /// <summary>Get haptics device name for playing back haptic feedback on this device.</summary>
        public string hapticsDeviceName { get { return m_HapticsDeviceName; } }
    }
}
*/