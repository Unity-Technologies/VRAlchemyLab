using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.XR.Interaction
{
    [Serializable]
    public class XRInteractableEvent : UnityEvent<XRBaseInteractor> { }

    /// <summary>
    /// Helper object that provides UnityEvent callbacks for an Interactable
    /// </summary>
    [AddComponentMenu("XR/Helpers/XR Interactable Event Callbacks")]
    [DisallowMultipleComponent]
    public class XRInteractableEventCallbacks : MonoBehaviour
    {
        [SerializeField, Tooltip("Called when the the Interactable enters the hover state.")]
        XRInteractableEvent m_OnHoverEnter = new XRInteractableEvent();
        /// <summary>Gets or sets the event that is called when the interactor begins hovering over an interactable.</summary>
        public XRInteractableEvent onHoverEnter { get { return m_OnHoverEnter; } set { m_OnHoverEnter = value; } }

        [SerializeField, Tooltip("Called when the the Interactable exits the hover state.")]
        XRInteractableEvent m_OnHoverExit = new XRInteractableEvent();
        /// <summary>Gets or sets the event that is called when the the interactor stops hovering over an interactable.</summary>
        public XRInteractableEvent onHoverExit { get { return m_OnHoverExit; } set { m_OnHoverExit = value; } }

        [SerializeField, Tooltip("Called when the the Interactable enters the hover state.")]
        XRInteractableEvent m_OnSelectEnter = new XRInteractableEvent();
        /// <summary>Gets or sets the event that is called when the the interactor begins selecting an interactable.</summary>
        public XRInteractableEvent onSelectEnter { get { return m_OnSelectEnter; } set { m_OnSelectEnter = value; } }

        [SerializeField, Tooltip("Called when the the Interactable exits the hover state.")]
        XRInteractableEvent m_OnSelectExit = new XRInteractableEvent();
        /// <summary>Gets or sets the event that is called when the the interactor stops selecting an interactable.</summary>
        public XRInteractableEvent onSelectExit { get { return m_OnSelectExit; } set { m_OnSelectExit = value; } }

        [SerializeField]
        [Tooltip("Called when the the Interactor activates this selected Interactable.")]
        XRInteractableEvent m_OnActivate = new XRInteractableEvent();
        /// <summary>Gets or sets the event that is called when the the interactor activates an interactable.</summary>
        public XRInteractableEvent onActivate { get { return m_OnActivate; } set { m_OnActivate = value; } }

        XRBaseInteractable m_Interactable;

        void Awake()
        {
            m_Interactable = GetComponent<XRBaseInteractable>();
            if (m_Interactable)
            {
                m_Interactable.hoverEnter     += OnHoverEnter;
                m_Interactable.hoverExit      += OnHoverExit;
                m_Interactable.selectEnter    += OnSelectEnter;
                m_Interactable.selectExit     += OnSelectExit;
                m_Interactable.activated      += OnActivate;
            }
            else
                Debug.LogWarning("Could not find interactable for helper.", this);
        }

        void Destroy()
        {
            if (m_Interactable)
            {
                m_Interactable.hoverEnter   -= OnHoverEnter;
                m_Interactable.hoverExit    -= OnHoverExit;
                m_Interactable.selectEnter  -= OnSelectEnter;
                m_Interactable.selectExit   -= OnSelectExit;
                m_Interactable.activated    -= OnActivate;
            }
        }

        void OnHoverEnter(XRBaseInteractor interactor)
        {
            if (m_Interactable.hoveringInteractors.Count == 1)
                m_OnHoverEnter.Invoke(interactor);
        }

        void OnHoverExit(XRBaseInteractor interactor)
        {
            if (m_Interactable.hoveringInteractors.Count == 0)
                m_OnHoverExit.Invoke(interactor);
        }
        void OnSelectEnter(XRBaseInteractor interactor) { m_OnSelectEnter.Invoke(interactor); }
        void OnSelectExit(XRBaseInteractor interactor)  { m_OnSelectExit.Invoke(interactor); }
        void OnActivate(XRBaseInteractor interactor)    { m_OnActivate.Invoke(interactor); }
    }
}