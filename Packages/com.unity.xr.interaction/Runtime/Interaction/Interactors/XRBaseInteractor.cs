using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.XR.Interaction
{
    /// <summary>
    /// Abstract base class from which all interactable behaviours derive.
    /// This class hooks into the interaction system (via XRInteractionManager) and provides base virtual methods for handling
    /// hover and selection.
    /// </summary>
    [SelectionBase]
    [DisallowMultipleComponent]
    public abstract class XRBaseInteractor : MonoBehaviour
    {
        [SerializeField]
        XRInteractionManager m_InteractionManager;
        /// <summary>Gets or sets Interaction Manager.</summary>
        public XRInteractionManager interactionManager
        {
            get { return m_InteractionManager; }
            set
            {
                m_InteractionManager = value;
                RegisterWithInteractionMananger();
            }
        }

        [SerializeField]
        LayerMask m_InteractionLayerMask = -1;
        /// <summary>Gets or sets interaction layer mask.  Only interactables with this layer mask will respond to this interactor.</summary>
        public LayerMask InteractionLayerMask { get { return m_InteractionLayerMask; } set { m_InteractionLayerMask = value; } }

        [SerializeField]
        Transform m_AttachTransform;
        /// <summary>Gets or sets the attach transform that is used as an attach point for interactables.
        /// Note: setting this will not automatically destroy the previous attach transform object.
        /// </summary>
        public Transform attachTransform { get { return m_AttachTransform; } }

        [SerializeField]
        XRBaseInteractable m_StartingSelectedInteractable = null;

        // target selected object (may by null)
        XRBaseInteractable m_SelectTarget;
        /// <summary>Gets selected interactable for this interactor.</summary>
        public XRBaseInteractable selectTarget { get { return m_SelectTarget; } }

        // Target interactables that are currently being hovered over. (may by empty)
        protected List<XRBaseInteractable> m_HoverTargets = new List<XRBaseInteractable>();

        /// <summary>Gets initial interactable that is selected by this interactor (may be null).</summary>
        public XRBaseInteractable startingSelectedInteractable { get { return m_StartingSelectedInteractable; } }

        /// <summary>Gets layer mask that is used to filter the interactables that can be interacted with.</summary>
        public LayerMask interactionLayerMask { get { return m_InteractionLayerMask; } }

        /// <summary>Event that gets called when this interactor is started.</summary>
        public event Action<XRBaseInteractor> started;

        /// <summary>Event that gets called when this interactor hovers over an interactable.</summary>
        public event Action<XRBaseInteractable> hoverEnter;

        /// <summary>Event that gets called when this interactor no longer hovers over an interactable.</summary>
        public event Action<XRBaseInteractable> hoverExit;

        /// <summary>Event that gets called when this interactor selects an interactable.</summary>
        public event Action<XRBaseInteractable> selectEnter;

        /// <summary>Event that gets called when this interactor no longer selects an interactable.</summary>
        public event Action<XRBaseInteractable> selectExit;

        XRInteractionManager m_RegisteredInteractionManager = null;

        protected virtual void Reset()
        {
            FindCreateInteractionManager();
        }

        protected virtual void Awake()
        {
            // create empty attach transform if none specified
            if (m_AttachTransform == null)
            {
                var attachGO = new GameObject(string.Format("[{0}] Attach", gameObject.name));
                if (attachGO != null)
                {
                    m_AttachTransform = attachGO.transform;
                    m_AttachTransform.SetParent(transform);
                    attachGO.transform.localPosition = Vector3.zero;
                    attachGO.transform.localRotation = Quaternion.identity;
                }
            }

            // setup interaction manager
            if (!m_InteractionManager)
                m_InteractionManager = FindObjectOfType<XRInteractionManager>();
            if (m_InteractionManager)
                RegisterWithInteractionMananger();
            else
                Debug.LogWarning("Could not find InteractionManager.", this);
        }

        void Start()
        {
            if (started != null)
                started(this);
        }

        public void GetHoverTargets(List<XRBaseInteractable> hoverTargets)
        {
            hoverTargets.Clear();
            foreach (var target in m_HoverTargets)
                hoverTargets.Add(target);
        }

        void FindCreateInteractionManager()
        {
            if (m_InteractionManager == null)
            {
                m_InteractionManager = Object.FindObjectOfType<XRInteractionManager>();
                if (m_InteractionManager == null)
                {
                    var interactionManagerGO = new GameObject("Interaction Manager", typeof(XRInteractionManager));
                    if (interactionManagerGO)
                        m_InteractionManager = interactionManagerGO.GetComponent<XRInteractionManager>();
                }
            }
        }

        void RegisterWithInteractionMananger()
        {
            if (m_InteractionManager != m_RegisteredInteractionManager)
            {
                if (m_RegisteredInteractionManager != null && m_InteractionManager != m_RegisteredInteractionManager)
                {
                    m_RegisteredInteractionManager.UnregisterInteractor(this);
                    m_RegisteredInteractionManager = null;
                }
                if (m_InteractionManager)
                {
                    m_InteractionManager.RegisterInteractor(this);
                    m_RegisteredInteractionManager = m_InteractionManager;
                }
            }
        }

        void OnDestroy()
        {
            if (m_RegisteredInteractionManager)
                m_InteractionManager.UnregisterInteractor(this);
        }

        internal void ClearHoverTargets()
        {
            m_HoverTargets.Clear();
        }

        bool IsOnValidLayerMask(XRBaseInteractable interactable)
        {
            return interactionLayerMask == -1 || interactable.interactionLayerMask == -1 ||
                (interactionLayerMask & interactable.interactionLayerMask) > 0;
        }

        /// <summary>
        /// Retrieve the list of interactables that this interactor could possibly interact with this frame.
        /// </summary>
        /// <param name="validTargets">Populated List of interactables that are valid for selection or hover.</param>
        public abstract void GetValidTargets(List<XRBaseInteractable> validTargets);

        /// <summary>Gets whether this interactor is in a state where it could hover.</summary>
        public virtual bool isHoverActive { get { return true; } }
        
        /// <summary>Gets whether this interactor is in a state where it could select.</summary>
        public virtual bool isSelectActive { get { return true; } }

        /// <summary>Determines if the interactable is valid for hover this frame.</summary>
        /// <param name="interactable">Interactable to check.</param>
        /// <returns><c>true</c> if the interactable can be hovered over this frame.</returns>
        public virtual bool CanHover(XRBaseInteractable interactable)                 { return IsOnValidLayerMask(interactable); }

        /// <summary>Determines if the interactable is valid for selection this frame.</summary>
        /// <param name="interactable">Interactable to check.</param>
        /// <returns><c>true</c> if the interactable can be selected this frame.</returns>
        public virtual bool CanSelect(XRBaseInteractable interactable)                { return IsOnValidLayerMask(interactable); }

        /// <summary>Gets if this interactor requires exclusive selection of an interactable.</summary>
        public virtual bool isSelectExclusive                                       { get { return true; } }

        /// <summary>Gets whether this interactor can override the movement type of the currently selected interactable.</summary>
        public virtual bool overrideSelectedInteractableMovement                    { get { return false; } }

        /// <summary>Gets the movement type to use when overriding the selected interactable's movement.</summary>
        public virtual XRBaseInteractable.MovementType? selectedInteractableMovementTypeOverride   { get { return null; } }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor first initiates hovering over an interactable.</summary>
        /// <param name="interactable">Interactable that is being hovered over.</param>
        protected internal virtual void OnHoverEnter(XRBaseInteractable interactable)
        {
            m_HoverTargets.Add(interactable);

            if (hoverEnter != null)
                hoverEnter(interactable);
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor ends hovering over an interactable.</summary>
        /// <param name="interactable">Interactable that is no longer hovered over.</param>
        protected internal virtual void OnHoverExit(XRBaseInteractable interactable)
        {
            Debug.Assert(m_HoverTargets.Contains(interactable));
            m_HoverTargets.Remove(interactable);

            if (hoverExit != null)
                hoverExit(interactable);
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor first initiates selection of an interactable.</summary>
        /// <param name="interactable">Interactable that is being selected.</param>
        protected internal virtual void OnSelectEnter(XRBaseInteractable interactable)
        {
            m_SelectTarget = interactable;

            if (selectEnter != null)
                selectEnter(interactable);
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor ends selection of an interactable.</summary>
        /// <param name="interactable">Interactable that is no longer selected.</param>
        protected internal virtual void OnSelectExit(XRBaseInteractable interactable)
        {
            Debug.Assert(m_SelectTarget == interactable);
            if (m_SelectTarget == interactable)
                m_SelectTarget = null;

            if (selectExit != null)
                selectExit(interactable);
        }
    }
}
