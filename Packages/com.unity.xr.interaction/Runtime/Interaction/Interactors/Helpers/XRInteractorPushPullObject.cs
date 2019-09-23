// Removed on 4/24/2018
// Needs to be re-added: https://github.com/Unity-Technologies/XR-Interaction/issues/18
/*
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEngine.Experimental.Input.Controls;

namespace UnityEngine.XR.Interaction
{
    /// <summary>
    /// Interactor helper object that uses an axis control of an input device to push or pull a selected interactable.
    /// </summary>
    [AddComponentMenu("XR/Helpers/XR Interactor Push Pull Object")]
    [DisallowMultipleComponent]
    public class XRInteractorPushPullObject : MonoBehaviour
    {
        const float k_DeadzoneThreshold = 0.3f;
        const float k_MaxPushPullRange  = 10f;
        const float k_PushPullSpeed     = 5f;

        [SerializeField, Tooltip("AxisControl Action used to push or pull an Interactable attached to the Interactor.")]
        InputAction m_PushPullAction;
        /// <summary>Gets or sets axisControl Action used to push or pull an Interactable attached to the Interactor.</summary>
        public InputAction pushPullAction { get { return m_PushPullAction; } set { m_PushPullAction = value; BindPushPullAction(); } }

        [SerializeField, Tooltip("Dead zone for input on push and pull")]
        float m_DeadzoneThreshold = k_DeadzoneThreshold;
        /// <summary>Gets or sets dead zone for input on push and pull</summary>
        public float deadzoneThreshold { get { return m_DeadzoneThreshold; } set { m_DeadzoneThreshold = value; } }

        [SerializeField, Tooltip("The max value that the anchor can be extended to")]
        float m_MaxRange = k_MaxPushPullRange;
        /// <summary>Gets or sets the max value that the anchor can be extended to</summary>
        public float maxRange { get { return m_MaxRange; } set { m_MaxRange = value; } }

        [SerializeField, Tooltip("Speed at which the object will be pushed or pulled to the interactor.")]
        float m_Speed = k_PushPullSpeed;
        /// <summary>Gets or sets the speed at which the object will be pushed or pulled to the interactor.</summary>
        public float speed { get { return m_Speed; } set { m_Speed = value; } }

        [SerializeField, Tooltip("Return to previous transform upon release")]
        bool m_RevertOnRelease = true;
        /// <summary>Gets or sets whether to return to previous transform upon release</summary>
        public bool revertOnRelease { get { return m_RevertOnRelease; } set { m_RevertOnRelease = value; } }

        XRBaseInteractor m_Interactor;
        Coroutine m_RotationAnimationCoroutine;
        Vector3 m_SavedPosition;

        void Awake()
        {
            m_Interactor = GetComponent<XRBaseInteractor>();
            if (m_Interactor)
            {
                m_SavedPosition = m_Interactor.attachTransform.localPosition;
                m_Interactor.selectEnter += OnSelectEnter;
            }
        }

        private void OnDestroy()
        {
            if (m_Interactor)
                m_Interactor.selectEnter -= OnSelectEnter;
        }

        void BindPushPullAction()
        {
            if (m_PushPullAction != null)
            {
                m_PushPullAction.performed -= OnPushPullAction;
                m_PushPullAction.Disable();
                m_PushPullAction.performed += OnPushPullAction;
                m_PushPullAction.Enable();
            }
        }

        void OnEnable()
        {
            BindPushPullAction();
        }

        void OnValidate()
        {
            BindPushPullAction();
        }

        void OnDisable()
        {
            m_PushPullAction.performed -= OnPushPullAction;
            m_PushPullAction.Disable();
        }

        void OnPushPullAction(InputAction.CallbackContext context)
        {
            var axisControl = context.control as AxisControl;
            if (axisControl != null && m_Interactor && m_Interactor.selectTarget)
            {
                float absZoomAmount = Mathf.Abs(axisControl.ReadValue()) - m_DeadzoneThreshold;
                if (absZoomAmount < 0.0f)
                    return;
                float zoomAmount = absZoomAmount * -Mathf.Sign(axisControl.ReadValue());

                var currentPos = m_Interactor.attachTransform.localPosition;
                var targetPos = currentPos + Vector3.forward * zoomAmount;
                targetPos.z = Mathf.Clamp(targetPos.z, 0f, m_MaxRange);
                m_Interactor.attachTransform.localPosition = Vector3.Lerp(currentPos, targetPos, Time.unscaledDeltaTime * m_Speed);
            }
        }

        void OnSelectEnter(XRBaseInteractable interactable)
        {
            if (m_Interactor && m_RevertOnRelease)
                m_Interactor.attachTransform.localPosition = m_SavedPosition;
        }
    }
}
*/