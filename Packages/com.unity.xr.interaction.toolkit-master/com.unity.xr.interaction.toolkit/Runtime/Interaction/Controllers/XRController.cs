using System;
using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// XRController MonoBehaviour that interprets InputSystem events into 
    /// XR Interaction Interactor position, rotation and interaction states.
    /// </summary>
    [DisallowMultipleComponent, AddComponentMenu("XR/XR Controller")]
    public class XRController : MonoBehaviour
    {
        /// <summary>Available axes to bind interaction actions to.</summary>
        public enum InputAxes
        {
            Primary2DAxis,
            DPad,
            Trigger,
            Grip,
            IndexTouch,
            ThumbTouch,
            Secondary2DAxis,
            IndexFinger,
            MiddleFinger,
            RingFinger,
            PinkyFinger,
            CombinedTrigger
        };

        /// <summary>
        /// The update type being used by the tracked pose driver
        /// </summary>
        public enum UpdateType
        {
            /// <summary>
            /// Sample input at both update, and directly before rendering. For smooth head pose tracking, 
            /// we recommend using this value as it will provide the lowest input latency for the device. 
            /// This is the default value for the UpdateType option
            /// </summary>
            UpdateAndBeforeRender,
            /// <summary>
            /// Only sample input during the update phase of the frame.
            /// </summary>
            Update,
            /// <summary>
            /// Only sample input directly before rendering
            /// </summary>
            BeforeRender,
        }
        [Header("Tracking Configuration")]
                
        [SerializeField]
        UpdateType m_UpdateTrackingType = UpdateType.UpdateAndBeforeRender;
        /// <summary>
        /// The update type being used by the tracked pose driver
        /// </summary>
        public UpdateType updateTrackingType
        {
            get { return m_UpdateTrackingType; }
            set { m_UpdateTrackingType = value; }
        }

        bool m_EnableInputTracking = true;
        /// <summary>Gets or sets if input is enabled for this controller.</summary>
        public bool enableInputTracking
        {
            get { return m_EnableInputTracking; }
            set { m_EnableInputTracking = value; }
        }

      
        [Header("Configuration")]

        [SerializeField, Tooltip("Used to disable an input state changing in the interactor. useful for swapping to a different interactor on the same object")]
        bool m_EnableInputActions = true;
        public bool enableInputActions { get { return m_EnableInputActions; } set { m_EnableInputActions = value; } }

        [SerializeField]
        XRNode m_ControllerNode;
        /// <summary>Gets or sets the XRNode for this controller.</summary>
        public XRNode controllerNode { get { return m_ControllerNode; } set { m_ControllerNode = value; } }

        [SerializeField]
        InputAxes m_SelectUsage = InputAxes.Grip;
        /// <summary>Gets or sets the usage to use for detecting selection.</summary>
        public InputAxes selectUsage { get { return m_SelectUsage; } set { m_SelectUsage = value; } }

        [SerializeField]
        InputAxes m_ActivateUsage = InputAxes.Trigger;
        /// <summary>Gets or sets the usage to use for detecting activation.</summary>
        public InputAxes activateUsage { get { return m_ActivateUsage; } set { m_ActivateUsage = value; } }

        [SerializeField]
        InputAxes m_UIPressUsage = InputAxes.Trigger;
        /// <summary>Gets or sets the usage to use for detecting a UI press.</summary>
        public InputAxes uiPressUsage { get { return m_UIPressUsage; } set { m_UIPressUsage = value; } }

        [SerializeField]
        float m_AxisToPressThreshold = 0.1f;
        /// <summary>Gets or sets the the amount the axis needs to be pressed to trigger an interaction event.</summary>
        public float axisToPressThreshold { get { return m_AxisToPressThreshold; } set { m_AxisToPressThreshold = value; } }

        [Header("Model")]

        [SerializeField]
        Transform m_ModelPrefab;
        /// <summary>Gets or sets the model prefab to show for this controller.</summary>
        public Transform modelPrefab { get { return m_ModelPrefab; } set { m_ModelPrefab = value; } }

        [SerializeField]
        Transform m_ModelTransform;
        /// <summary>Gets or sets the model transform that is used as the parent for the controller model.
        /// Note: setting this will not automatically destroy the previous model transform object.
        /// </summary>
        public Transform modelTransform { get { return m_ModelTransform; } }

        [SerializeField]
        bool m_AnimateModel;
        /// <summary>Gets or sets whether this model animates in response to interaction events.</summary>
        public bool animateModel { get { return m_AnimateModel; } set { m_AnimateModel = value; } }

        [SerializeField]
        string m_ModelSelectTransition;
        /// <summary>Gets or sets the animation transition to enable when selecting.</summary>
        public string modelSelectTransition { get { return m_ModelSelectTransition; } set { m_ModelSelectTransition = value; } }

        [SerializeField]
        string m_ModelDeSelectTransition;
        /// <summary>Gets or sets the animation transition to enable when de-selecting.</summary>
        public string modelDeSelectTransition { get { return m_ModelDeSelectTransition; } set { m_ModelDeSelectTransition = value; } }

        /// <summary>
        /// InteractionState type to hold current state for a given interaction.
        /// </summary>
        internal struct InteractionState
        {
            /// <summary>This field is true if it is is currently on.</summary>
            public bool active;
            /// <summary>This field is true if the interaction state was activated this frame.</summary>
            public bool activatedThisFrame;
            /// <summary>This field is true if the interaction state was de-activated this frame.</summary>
            public bool deActivatedThisFrame;
        }

        internal enum InteractionTypes { select, activate, uiPress };
        InteractionState m_SelectInteractionState;
        InteractionState m_ActivateInteractionState;
        InteractionState m_UIPressInteractionState;

        /// <summary>Gets the current select interaction state.</summary>
        internal InteractionState selectInteractionState { get { return m_SelectInteractionState; } }
        /// <summary>Gets the current activate interaction state.</summary>
        internal InteractionState activateInteractionState { get { return m_ActivateInteractionState; } }
        /// <summary>Gets the current ui press interaction state.</summary>
        internal InteractionState uiPressInteractionState { get { return m_UIPressInteractionState; } }

        // Flag to indicate that setup should be (re)performed on Update.
        bool m_PerformSetup = true;

        GameObject m_ModelGO;
        bool m_HideControllerModel = false;

        /// <summary>Gets or sets whether the controller model should be hidden.</summary>
        public bool hideControllerModel
        {
            get { return m_HideControllerModel; }
            set
            {
                m_HideControllerModel = value;
                if (m_ModelGO)
                    m_ModelGO.SetActive(!m_HideControllerModel);
            }
        }

        protected virtual void OnEnable()
        {
            Application.onBeforeRender += OnBeforeRender;
        }

        protected virtual void OnDisable()
        {
            Application.onBeforeRender -= OnBeforeRender;
        }

        protected virtual void OnBeforeRender()
        {
            if (enableInputTracking &&
               (m_UpdateTrackingType == UpdateType.BeforeRender ||
                m_UpdateTrackingType == UpdateType.UpdateAndBeforeRender))
            {
                UpdateTrackingInput();
            }
        }

        protected virtual void Awake()
        {
            // create empty model transform if none specified
            if (m_ModelTransform == null)
            {
                var modelGO = new GameObject(string.Format("[{0}] Model", gameObject.name));
                if (modelGO != null)
                {
                    m_ModelTransform = modelGO.transform;
                    m_ModelTransform.SetParent(transform);
                    modelGO.transform.localPosition = Vector3.zero;
                    modelGO.transform.localRotation = Quaternion.identity;
                }
            }
        }

        void PerformSetup()
        {
            SetupModel();
        }

        void SetupModel()
        {
            if (m_ModelGO)
                Destroy(m_ModelGO);
            var model = m_ModelPrefab;
            if (model != null)
            {
                m_ModelGO = Instantiate(model).gameObject;
                m_ModelGO.transform.parent = m_ModelTransform;
                m_ModelGO.transform.localPosition = new Vector3(0f, 0f, 0f);
                m_ModelGO.transform.localRotation = Quaternion.identity;
                m_ModelGO.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                m_ModelGO.transform.gameObject.SetActive(true);
            }
        }

        bool ShouldUpdateTrackingInput()
        {
            return enableInputTracking &&
                (m_UpdateTrackingType == UpdateType.Update ||
                m_UpdateTrackingType == UpdateType.UpdateAndBeforeRender);
        }

        void Update()
        {
            if (m_PerformSetup)
            {
                PerformSetup();
                m_PerformSetup = false;
            }

            if ( enableInputTracking &&
                (m_UpdateTrackingType == UpdateType.Update ||
                m_UpdateTrackingType == UpdateType.UpdateAndBeforeRender))
            {
                UpdateTrackingInput();

            }
            if(enableInputActions)
            {
                UpdateInput();
            }   
        }

        void UpdateTrackingInput()
        {
            Vector3 devicePosition = new Vector3();
            if (InputDevices.GetDeviceAtXRNode(controllerNode).TryGetFeatureValue(CommonUsages.devicePosition, out devicePosition))
                transform.localPosition = devicePosition;

            Quaternion deviceRotation = new Quaternion();
            if (InputDevices.GetDeviceAtXRNode(controllerNode).TryGetFeatureValue(CommonUsages.deviceRotation, out deviceRotation))
                transform.localRotation = deviceRotation;
        }

        void UpdateInput()
        {
            // clear state for selection, activation and press state dependent on this frame
            m_SelectInteractionState.activatedThisFrame = m_SelectInteractionState.deActivatedThisFrame = false;
            m_ActivateInteractionState.activatedThisFrame = m_ActivateInteractionState.deActivatedThisFrame = false;
            m_UIPressInteractionState.activatedThisFrame = m_UIPressInteractionState.deActivatedThisFrame = false;

            HandleInteractionAction(controllerNode, m_SelectUsage.ToString(), ref m_SelectInteractionState);
            HandleInteractionAction(controllerNode, m_ActivateUsage.ToString(), ref m_ActivateInteractionState);
            HandleInteractionAction(controllerNode, m_UIPressUsage.ToString(), ref m_UIPressInteractionState);

            UpdateControllerModelAnimation();                     
        }

        void HandleInteractionAction(XRNode node, string usage, ref InteractionState interactionState)
        {
            float value = 0.0f;
            var inputDevice = InputDevices.GetDeviceAtXRNode(node);
            if (inputDevice.isValid && inputDevice.TryGetFeatureValue(new InputFeatureUsage<float>(usage), out value) &&
                value >= m_AxisToPressThreshold)
            {
                if (!interactionState.active)
                {
                    interactionState.activatedThisFrame = true;
                    interactionState.active = true;
                }
            }
            else
            {
                if (interactionState.active)
                {
                    interactionState.deActivatedThisFrame = true;
                    interactionState.active = false;
                }
            }
        }

        // Override the XRController's current interaction state (used for interaction state playback)
        internal void UpdateInteractionType(InteractionTypes interactionStateType, bool isInteractionStateOn)
        {
            switch (interactionStateType)
            {
                case InteractionTypes.select:
                    UpdateInteractionState(ref m_SelectInteractionState, isInteractionStateOn);
                    break;
                case InteractionTypes.activate:
                    UpdateInteractionState(ref m_ActivateInteractionState, isInteractionStateOn);
                    break;
                case InteractionTypes.uiPress:
                    UpdateInteractionState(ref m_UIPressInteractionState, isInteractionStateOn);
                    break;
            }
        }

        static void UpdateInteractionState(ref InteractionState interactionState, bool isInteractionStateOn)
        {
            bool previousActive = interactionState.active;
            interactionState.active = isInteractionStateOn;

            if (interactionState.active && !previousActive)
                interactionState.activatedThisFrame = true;
            else if (!interactionState.active && previousActive)
                interactionState.deActivatedThisFrame = true;
        }

        // Override the XRController's controller model's animation (if the prefab contains an animator)
        internal void UpdateControllerModelAnimation()
        {
            if (m_ModelGO && m_AnimateModel)
            {
                Animator animator = m_ModelGO.GetComponent<Animator>();
                if (animator)
                {
                    if (m_SelectInteractionState.activatedThisFrame)
                        animator.SetBool(modelSelectTransition, true);
                    else if (m_SelectInteractionState.deActivatedThisFrame)
                        animator.SetBool(modelDeSelectTransition, true);
                }
            }
        }

        // Override the XRController's current position and rotation (used for interaction state playback)
        internal void UpdateControllerPose(Vector3 position, Quaternion rotation)
        {
            transform.localPosition = position;
            transform.localRotation = rotation;
        }

        /// <summary>Play a haptic impulse on the controller if one is available</summary>
        /// <param name="amplitude">Amplitude (from 0.0 to 1.0) to play impulse at.</param>
        /// <param name="duration">Duration (in seconds) to play haptic impulse.</param>
        public bool SendHapticImpulse(float amplitude, float duration)
        {
            HapticCapabilities capabilities;
            if (InputDevices.GetDeviceAtXRNode(controllerNode).TryGetHapticCapabilities(out capabilities) &&
                capabilities.supportsImpulse)
            {
                return InputDevices.GetDeviceAtXRNode(controllerNode).SendHapticImpulse(0, amplitude, duration);
            }
            return false;
        }
    }
}
