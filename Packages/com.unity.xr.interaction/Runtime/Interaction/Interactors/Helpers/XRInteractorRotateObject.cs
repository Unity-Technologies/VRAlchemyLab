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
    /// Interactor helper object that uses an axis control of an input device to rotate a selected interactable.
    /// </summary>
    [AddComponentMenu("XR/Helpers/XR Interactor Rotate Object")]
    [DisallowMultipleComponent]
    public class XRInteractorRotateObject : MonoBehaviour
    {
        const int   k_DefaultRotationSnapInterval   = 45;
        const float k_DefaultRotationSnapTime       = 0.1f;
        const float k_DefaultDeadzoneThreshold      = 0.3f;

        [SerializeField, Tooltip("AxisControl Action used to rotate an interactable attached to the interactor.")]
        InputAction m_RotateAction;
        /// <summary>Gets or sets the axisControl Action used to rotate an interactable attached to the interactor.</summary>
        public InputAction rotateAction { get { return m_RotateAction; } set { m_RotateAction = value; BindRotateAction(); } }

        [SerializeField, Tooltip("The user can only rotate an anchor in multiples of [m_RotationSnapInterval] while in rotation lock mode.")]
        int m_RotationSnapInterval = k_DefaultRotationSnapInterval;
        /// <summary>Gets or sets the user can only rotate an anchor in multiples of [m_RotationSnapInterval] while in rotation lock mode.</summary>
        public int rotationSnapInterval { get { return m_RotationSnapInterval; } set { m_RotationSnapInterval = value; } }

        [Range(0f, 1f)]
        [SerializeField, Tooltip("Number of seconds it takes for the anchor to snap its rotation.")]
        float m_RotationSnapTime = k_DefaultRotationSnapTime;
        /// <summary>Gets or sets the number of seconds it takes for the anchor to snap its rotation.</summary>
        public float rotationSnapTime { get { return m_RotationSnapTime; } set { m_RotationSnapTime = value; } }

        [SerializeField, Tooltip("Dead zone for input on rotation.")]
        float m_DeadzoneThreshold = k_DefaultDeadzoneThreshold;
        /// <summary>Gets or sets the dead zone for input on rotation.</summary>
        public float deadzoneThreshold { get { return m_DeadzoneThreshold; } set { m_DeadzoneThreshold = value; } }

        [SerializeField, Tooltip("Inverts the control output that is applied to the rotation.")]
        bool m_InvertRotation;
        /// <summary>Gets or sets whether to invert the control output that is applied to the rotation.</summary>
        public bool invertRotation { get { return m_InvertRotation; } set { m_InvertRotation = value; } }

        [SerializeField, Tooltip("Return to previous transform upon release.")]
        bool m_RevertOnRelease = true;
        /// <summary>Gets or sets whether to return to previous transform upon release.</summary>
        public bool revertOnRelease { get { return m_RevertOnRelease; } set { m_RevertOnRelease = value; } }

        XRBaseInteractor m_Interactor;
        Coroutine m_RotationAnimationCoroutine;
        Quaternion m_SavedRotation;

        void Awake()
        {
            m_Interactor = GetComponent<XRBaseInteractor>();
            if (m_Interactor)
            {
                m_SavedRotation = m_Interactor.attachTransform.rotation;
                m_Interactor.selectEnter += OnSelectEnter;
            }
        }

        private void OnDestroy()
        {
            if (m_Interactor)
                m_Interactor.selectEnter -= OnSelectEnter;
        }

        void BindRotateAction()
        {
            if (m_RotateAction != null)
            {
                m_RotateAction.performed -= OnRotateAction;
                m_RotateAction.Disable();
                m_RotateAction.performed += OnRotateAction;
                m_RotateAction.Enable();
            }
        }

        void OnEnable()
        {
            BindRotateAction();
        }

        void OnValidate()
        {
            BindRotateAction();
        }

        void OnDisable()
        {
            m_RotateAction.performed -= OnRotateAction;
            m_RotateAction.Disable();
        }

        void OnRotateAction(InputAction.CallbackContext context)
        {
            var axisControl = context.control as AxisControl;
            if (axisControl != null && m_Interactor && m_Interactor.selectTarget)
            {
                float absRotationAmount = Mathf.Abs(axisControl.ReadValue()) - m_DeadzoneThreshold;
                if (m_RotationSnapInterval == 0 || absRotationAmount < 0.0f)
                    return;
                float rotationAmount = absRotationAmount * Mathf.Sign(axisControl.ReadValue());
            
                if (m_InvertRotation)
                    rotationAmount *= -1;
                var targetRotationOffset = rotationAmount < 0.0f
                    ? Quaternion.Euler(Vector3.up * -m_RotationSnapInterval)
                    : Quaternion.Euler(Vector3.up * m_RotationSnapInterval);
                if (m_RotationAnimationCoroutine == null)
                    m_RotationAnimationCoroutine = StartCoroutine(AnimateRotation(m_Interactor.attachTransform, m_Interactor.attachTransform.rotation * targetRotationOffset));
            }
        }

        IEnumerator AnimateRotation(Transform anchor, Quaternion targetRotation)
        {
            var timePassed = 0f;
            var initialRotation = anchor.rotation;

            while (timePassed < m_RotationSnapTime)
            {
                yield return null;
                timePassed += Time.deltaTime;
                anchor.rotation = Quaternion.Slerp(initialRotation, targetRotation, timePassed / m_RotationSnapTime);
            }
            m_RotationAnimationCoroutine = null;
        }

        void OnSelectEnter(XRBaseInteractable interactable)
        {
            if (m_Interactor && m_RevertOnRelease)
                m_Interactor.attachTransform.rotation = m_SavedRotation;
        }
    }
}
*/