using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// UnityEvent that responds to changes of hover and selection by this interactor.
    /// </summary>
    [Serializable]    
    public class XRInteractorEvent : UnityEvent<XRBaseInteractable> { }

    /// <summary>
    /// Abstract base class from which all interactable behaviours derive.
    /// This class hooks into the interaction system (via XRInteractionManager) and provides base virtual methods for handling
    /// hover and selection.
    /// </summary>
    [SelectionBase]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_Interactors)]
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

        XRInteractionManager m_RegisteredInteractionManager = null;

        [SerializeField, Tooltip("Called when this interactor begins hovering an interactable.")]
        XRInteractorEvent m_OnHoverEnter = new XRInteractorEvent();
        /// <summary>Gets or sets the event that is called when this interactor begins hovering over an interactable.</summary>
        public XRInteractorEvent onHoverEnter { get { return m_OnHoverEnter; } set { m_OnHoverEnter = value; } }

        [SerializeField, Tooltip("Called when this interactor stops the hovering an interactable.")]
        XRInteractorEvent m_OnHoverExit = new XRInteractorEvent();
        /// <summary>Gets or sets the event that is called when this interactor stops hovering over an interactable.</summary>
        public XRInteractorEvent onHoverExit { get { return m_OnHoverExit; } set { m_OnHoverExit = value; } }

        [SerializeField, Tooltip("Called when this interactor begins selecting an interactable.")]
        XRInteractorEvent m_OnSelectEnter = new XRInteractorEvent();
        /// <summary>Gets or sets the event that is called when this interactor begins selecting an interactable.</summary>
        public XRInteractorEvent onSelectEnter { get { return m_OnSelectEnter; } set { m_OnSelectEnter = value; } }

        [SerializeField, Tooltip("Called when this interactor stops selecting an interactable.")]
        XRInteractorEvent m_OnSelectExit = new XRInteractorEvent();
        /// <summary>Gets or sets the event that is called when this interactor stops selecting an interactable.</summary>
        public XRInteractorEvent onSelectExit { get { return m_OnSelectExit; } set { m_OnSelectExit = value; } }
        
        bool m_AllowHover = true;
        public bool allowHover {  get { return m_AllowHover;  } set { m_AllowHover = value; } }
        
        bool m_AllowSelect = true;
        public bool allowSelect { get { return m_AllowSelect; } set { m_AllowSelect = value; } }

        bool m_EnableInteractions = false;
        public bool enableInteractions {  get { return m_EnableInteractions; } set { m_EnableInteractions = value; EnableInteractions(value); } }

        void EnableInteractions(bool enable)
        {
            m_AllowHover = enable;
            m_AllowSelect = enable;
        }

        bool m_RequiresRegistration = true;

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

        protected virtual void OnEnable()
        {
            if (m_RequiresRegistration)
            {
                FindCreateInteractionManager();
                if (m_RegisteredInteractionManager)
                    m_RegisteredInteractionManager.RegisterInteractor(this);
                m_RequiresRegistration = false;
            }
            EnableInteractions(true);
        }

        protected virtual void OnDisable()
        {
            if (m_RegisteredInteractionManager)
                m_RegisteredInteractionManager.UnregisterInteractor(this);

            m_RequiresRegistration = true;
        }
        void OnDestroy()
        {
            if (m_RegisteredInteractionManager)
                m_RegisteredInteractionManager.UnregisterInteractor(this);
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

            if (m_RegisteredInteractionManager != null)
                m_RequiresRegistration = false;
        }
        
        internal void ClearHoverTargets()
        {
            m_HoverTargets.Clear();
        }

        bool IsOnValidLayerMask(XRBaseInteractable interactable)
        {
            return interactionLayerMask == -1 || interactable.interactionLayerMask == -1 ||
                (interactionLayerMask & interactable.interactionLayerMask) == interactable.interactionLayerMask;
        }

        /// <summary>
        /// Retrieve the list of interactables that this interactor could possibly interact with this frame.
        /// </summary>
        /// <param name="validTargets">Populated List of interactables that are valid for selection or hover.</param>
        public abstract void GetValidTargets(List<XRBaseInteractable> validTargets);

        /// <summary>Gets whether this interactor is in a state where it could hover.</summary>
        public virtual bool isHoverActive { get { return m_AllowHover; } }
        
        /// <summary>Gets whether this interactor is in a state where it could select.</summary>
        public virtual bool isSelectActive { get { return m_AllowSelect; } }

        /// <summary>Determines if the interactable is valid for hover this frame.</summary>
        /// <param name="interactable">Interactable to check.</param>
        /// <returns><c>true</c> if the interactable can be hovered over this frame.</returns>
        public virtual bool CanHover(XRBaseInteractable interactable)                 { return m_AllowHover && IsOnValidLayerMask(interactable); }

        /// <summary>Determines if the interactable is valid for selection this frame.</summary>
        /// <param name="interactable">Interactable to check.</param>
        /// <returns><c>true</c> if the interactable can be selected this frame.</returns>
        public virtual bool CanSelect(XRBaseInteractable interactable)                { return m_AllowSelect && IsOnValidLayerMask(interactable); }

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
            m_OnHoverEnter?.Invoke(interactable);
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor ends hovering over an interactable.</summary>
        /// <param name="interactable">Interactable that is no longer hovered over.</param>
        protected internal virtual void OnHoverExit(XRBaseInteractable interactable)
        {
            Debug.Assert(m_HoverTargets.Contains(interactable));
            m_HoverTargets.Remove(interactable);
            m_OnHoverExit?.Invoke(interactable);
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor first initiates selection of an interactable.</summary>
        /// <param name="interactable">Interactable that is being selected.</param>
        protected internal virtual void OnSelectEnter(XRBaseInteractable interactable)
        {
            m_SelectTarget = interactable;
            m_OnSelectEnter?.Invoke(interactable);
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor ends selection of an interactable.</summary>
        /// <param name="interactable">Interactable that is no longer selected.</param>
        protected internal virtual void OnSelectExit(XRBaseInteractable interactable)
        {
            Debug.Assert(m_SelectTarget == interactable);
            if (m_SelectTarget == interactable)
                m_SelectTarget = null;

            m_OnSelectExit?.Invoke(interactable);
        }

        /// <summary>
        /// This method is called by the interaction manager to update the interactor. 
        /// Please see the interaction manager documentation for more details on update order
        /// </summary>        
    
        public virtual void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            return;
        }
    }
}
