using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.XR.Interaction
{
    /// <summary>
    /// UnityEvent that responds to changes of hover and selection by this interactor.
    /// </summary>
    [Serializable]
    public class XRInteractorEvent : UnityEvent<XRBaseInteractable> { }

    /// <summary>
    /// Interactor helper object that provides UnityEvent callbacks for an interactor.
    /// </summary>
    [AddComponentMenu("XR/Helpers/XR Interactor Event Callbacks")]
    [DisallowMultipleComponent]
    public class XRInteractorEventCallbacks : MonoBehaviour
    {
        [SerializeField, Tooltip("Called when the interactor begins hovering an interactable.")]
        XRInteractorEvent m_OnHoverEnter = new XRInteractorEvent();
        /// <summary>Gets or sets the event that is called when the interactor begins hovering over an interactable.</summary>
        public XRInteractorEvent onHoverEnter { get { return m_OnHoverEnter; } set { m_OnHoverEnter = value; } }

        [SerializeField, Tooltip("Called when the the interactor stops the hovering an interactable.")]
        XRInteractorEvent m_OnHoverExit = new XRInteractorEvent();
        /// <summary>Gets or sets the event that is called when the the interactor stops hovering over an interactable.</summary>
        public XRInteractorEvent onHoverExit { get { return m_OnHoverExit; } set { m_OnHoverExit = value; } }

        [SerializeField, Tooltip("Called when the the interactor begins selecting an interactable.")]
        XRInteractorEvent m_OnSelectEnter = new XRInteractorEvent();
        /// <summary>Gets or sets the event that is called when the the interactor begins selecting an interactable.</summary>
        public XRInteractorEvent onSelectEnter { get { return m_OnSelectEnter; } set { m_OnSelectEnter = value; } }

        [SerializeField, Tooltip("Called when the the interactor stops selecting an interactable.")]
        XRInteractorEvent m_OnSelectExit = new XRInteractorEvent();
        /// <summary>Gets or sets the event that is called when the the interactor stops selecting an interactable.</summary>
        public XRInteractorEvent onSelectExit { get { return m_OnSelectExit; } set { m_OnSelectExit = value; } }

        XRBaseInteractor m_Interactor;

        void Awake()
        {
            m_Interactor = GetComponent<XRBaseInteractor>();
            if (m_Interactor)
            {
                m_Interactor.hoverEnter     += OnHoverEnter;
                m_Interactor.hoverExit      += OnHoverExit;
                m_Interactor.selectEnter    += OnSelectEnter;
                m_Interactor.selectExit     += OnSelectExit;
            }
            else
                Debug.LogWarning("Could not find interactor for InteractorEventCallbacks helper.", this);
        }

        void Destroy()
        {
            if (m_Interactor)
            {
                m_Interactor.hoverEnter     -= OnHoverEnter;
                m_Interactor.hoverExit      -= OnHoverExit;
                m_Interactor.selectEnter    -= OnSelectEnter;
                m_Interactor.selectExit     -= OnSelectExit;
            }
        }

        void OnHoverEnter(XRBaseInteractable interactable)    { m_OnHoverEnter.Invoke(interactable); }
        void OnHoverExit(XRBaseInteractable interactable)     { m_OnHoverExit.Invoke(interactable); }
        void OnSelectEnter(XRBaseInteractable interactable)   { m_OnSelectEnter.Invoke(interactable); }
        void OnSelectExit(XRBaseInteractable interactable)    { m_OnSelectExit.Invoke(interactable); }
    }
}