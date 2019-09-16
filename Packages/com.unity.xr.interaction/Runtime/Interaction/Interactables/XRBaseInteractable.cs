using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.XR.Interaction
{
    /// <summary>
    /// Abstract base class from which all interactable behaviours derive.
    /// This class hooks into the interaction system (via XRInteractionManager) and provides base virtual methods for handling
    /// hover and selection.
    /// </summary>
    [SelectionBase]
    public abstract class XRBaseInteractable : MonoBehaviour
    {
        /// <summary>Type of movement for an interactable</summary>
        public enum MovementType
        {
            /// <summary>Interactable is under full control of script.</summary>
            NonKinematic,
            /// <summary>Interactable is affected by forces, collisions and joints.</summary>
            Kinematic
        };

        [SerializeField, Tooltip("Manager to handle all interaction management (will find one if empty).")]
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

        [SerializeField, Tooltip("Colliders to use for interaction (if empty, will use any child colliders).")]
        List<Collider> m_Colliders = new List<Collider>();
        /// <summary>Gets colliders to use for interaction with this interactable.</summary>
        public IEnumerable<Collider> colliders { get { return m_Colliders; } }

        [SerializeField, Tooltip("Only Interactors with this Layer Mask will interact with this Interactable.")]
        LayerMask m_InteractionLayerMask = -1;
        /// <summary>Gets or sets the layer mask to use to filter interactors that can interact with this interactable.</summary>
        public LayerMask interactionLayerMask { get { return m_InteractionLayerMask; } set { m_InteractionLayerMask = value; } }

        static XRInteractionManager s_CachedInteractionManager;
        float m_CachedDistanceToInteractor = float.MaxValue;


        List<XRBaseInteractor> m_HoveringInteractors = new List<XRBaseInteractor>();
        /// <summary>Gets the list of Interactors that are hovering on this interactable; </summary>
        public List<XRBaseInteractor> hoveringInteractors { get { return m_HoveringInteractors; } }

        /// <summary>Gets whether this interactable is currently being hovered.</summary>
        public bool isHovered { get; private set; }

        /// <summary>Gets whether this interactable is currently being selected.</summary>
        public bool isSelected { get; private set; }

        /// <summary>Gets cached distance to the last interactor.</summary>
        public float cachedDistanceToInteractor { get { return m_CachedDistanceToInteractor; } }

        /// <summary>Event that gets called when this interactable is started.</summary>
        public event Action<XRBaseInteractable> started;

        /// <summary>Event that gets called when this interactable is hovered over.</summary>
        public event Action<XRBaseInteractor> hoverEnter;

        /// <summary>Event that gets called when this interactable is no longer hovered over.</summary>
        public event Action<XRBaseInteractor> hoverExit;

        /// <summary>Event that gets called when this interactable is selected.</summary>
        public event Action<XRBaseInteractor> selectEnter;

        /// <summary>Event that gets called when this interactable is unselected.</summary>
        public event Action<XRBaseInteractor> selectExit;

        /// <summary>Event that gets called when this interactable is activated.</summary>
        public event Action<XRBaseInteractor> activated;

        XRInteractionManager m_RegisteredInteractionManager = null;

        protected virtual void Reset()
        {
            FindCreateInteractionManager();
        }

        protected virtual void Awake()
		{
            // if we have no colliders, add children colliders
            if (m_Colliders.Count <= 0)
                m_Colliders = new List<Collider>(GetComponentsInChildren<Collider>());

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
                if (m_RegisteredInteractionManager != null)
                {
                    m_RegisteredInteractionManager.UnregisterInteractable(this);
                    m_RegisteredInteractionManager = null;
                }
                if (m_InteractionManager)
                {
                    m_InteractionManager.RegisterInteractable(this);
                    m_RegisteredInteractionManager = m_InteractionManager;
                }
            }
        }

        void OnDestroy()
        {
            if (m_RegisteredInteractionManager)
                m_InteractionManager.UnregisterInteractable(this);
        }

        /// <summary>
        /// Calculates distance squared to interactor (based on colliders).
        /// </summary>
        /// <param name="interactor">Interactor to calculate distance against.</param>
        /// <returns>Minimum distance between the interactor and this interactable's colliders.</returns>
        public float GetDistanceSqrToInteractor(XRBaseInteractor interactor)
        {
            if (!interactor)
                return float.MaxValue;

            float minDistanceSqr = float.MaxValue;
            foreach (var col in m_Colliders)
            {
                var offset = (interactor.attachTransform.position - col.transform.position);
                minDistanceSqr = Mathf.Min(offset.sqrMagnitude, minDistanceSqr);
            }
            return minDistanceSqr;
        }

        /// <summary>
        /// Determines if this interactable can be hovered by a given interactor.
        /// </summary>
        /// <param name="interactor">Interactor to check for a valid hover state with.</param>
        /// <returns>True if hovering is valid this frame, False if not.</returns>
        public virtual bool IsHoverableBy(XRBaseInteractor interactor) { return true; }

        /// <summary>
        /// Determines if this interactable can be selected by a given interactor.
        /// </summary>
        /// <param name="interactor">Interactor to check for a valid selection with.</param>
        /// <returns>True if selection is valid this frame, False if not.</returns>
        public virtual bool IsSelectableBy(XRBaseInteractor interactor) { return true; }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor first initiates hovering over an interactable.</summary>
        /// <param name="interactor">Interactor that is initiating the hover.</param>
        protected internal virtual void OnHoverEnter(XRBaseInteractor interactor) 
		{
            isHovered = true;
            m_HoveringInteractors.Add(interactor);

            if (hoverEnter != null)
                hoverEnter(interactor);
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor ends hovering over an interactable.</summary>
        /// <param name="interactor">Interactor that is ending the hover.</param>
		protected internal virtual void OnHoverExit(XRBaseInteractor interactor) 
		{
			isHovered = false;
            if (m_HoveringInteractors.Contains(interactor))
                m_HoveringInteractors.Remove(interactor);

            if (hoverExit != null)
                hoverExit(interactor);
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor first initiates selection of an interactable.</summary>
        /// <param name="interactor">Interactor that is initiating the selection.</param>
		protected internal virtual void OnSelectEnter(XRBaseInteractor interactor) 
		{
			isSelected = true;

            if (selectEnter != null)
                selectEnter(interactor);
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor ends selection of an interactable.</summary>
        /// <param name="interactor">Interactor that is ending the selection.</param>
		protected internal virtual void OnSelectExit(XRBaseInteractor interactor) 
		{
			isSelected = false;

            if (selectExit != null)
                selectExit(interactor);
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor sends an activation event down to an interactable.</summary>
        /// <param name="interactor">Interactor that is sending the activation event.</param>
		protected internal virtual void OnActivate(XRBaseInteractor interactor)
        {
            if (activated != null)
                activated(interactor);
        }
    }
}