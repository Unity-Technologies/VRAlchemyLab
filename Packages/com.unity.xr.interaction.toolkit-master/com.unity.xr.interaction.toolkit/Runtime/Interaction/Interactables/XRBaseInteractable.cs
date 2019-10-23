using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// UnityEvent that responds to changes of hover, selection, and activation by this interactable.
    /// </summary>
    [Serializable]
    public class XRInteractableEvent : UnityEvent<XRBaseInteractor> { }

    /// <summary>
    /// Abstract base class from which all interactable behaviours derive.
    /// This class hooks into the interaction system (via XRInteractionManager) and provides base virtual methods for handling
    /// hover and selection.
    /// </summary>
    [SelectionBase]
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_Interactables)]
    public abstract class XRBaseInteractable : MonoBehaviour
    {

        /// <summary>Type of movement for an interactable</summary>
        public enum MovementType
        {
            /// <summary>In VelocityTracking mode, the Rigid Body associated with the will have velocity and angular velocity added to it such that the interactable attach point will follow the interactor attach point
            /// as this is applying forces to the RigidBody, this will appear to be a slight distance behind the visual representation of the Interactor / Controller</summary>
            VelocityTracking,
            /// <summary>In Kinematic mode the Rigid Body associated with the interactable will be moved such that the interactable attach point will match the interactor attach point
            /// as this is updating the RigidBody, this will appear a frame behind the visual representation of the Interactor / Controller </summary>
            Kinematic,
            /// <summary>In Instantaneous Mode the interactable's transform is updated such that the interactable attach point will match the interactor's attach point.
            /// as this is updating the transform directly, any rigid body attached to the GameObject that the interactable component is on will be disabled while being interacted with so
            /// that any motion will not "judder" due to the rigid body interfering with motion.</summary>
            Instantaneous,
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

        [SerializeField, Tooltip("Only interactors with this Layer Mask will interact with this interactable.")]
        LayerMask m_InteractionLayerMask = -1;
        /// <summary>Gets or sets the layer mask to use to filter interactors that can interact with this interactable.</summary>
        public LayerMask interactionLayerMask { get { return m_InteractionLayerMask; } set { m_InteractionLayerMask = value; } }

        static XRInteractionManager s_CachedInteractionManager;
        float m_CachedDistanceToInteractor = float.MaxValue;


        List<XRBaseInteractor> m_HoveringInteractors = new List<XRBaseInteractor>();
        /// <summary>Gets the list of interactors that are hovering on this interactable; </summary>
        public List<XRBaseInteractor> hoveringInteractors { get { return m_HoveringInteractors; } }

        /// <summary>Gets whether this interactable is currently being hovered.</summary>
        public bool isHovered { get; private set; }

        /// <summary>Gets whether this interactable is currently being selected.</summary>
        public bool isSelected { get; private set; }

        /// <summary>Gets cached distance to the last interactor.</summary>
        public float cachedDistanceToInteractor { get { return m_CachedDistanceToInteractor; } }
        
        XRInteractionManager m_RegisteredInteractionManager = null;

        [Header("Interactable Events")]

        [SerializeField, Tooltip("Called only when the first interactor begins hovering over this interactable.")]
        XRInteractableEvent m_OnFirstHoverEnter = new XRInteractableEvent();
        /// <summary>Gets or sets the event that is called only when the first interactor begins hovering over this interactable.</summary>
        public XRInteractableEvent onFirstHoverEnter { get { return m_OnFirstHoverEnter; } set { m_OnFirstHoverEnter = value; } }

        [SerializeField, Tooltip("Called every time when an interactor begins hovering this interactable.")]
        XRInteractableEvent m_OnHoverEnter = new XRInteractableEvent();
        /// <summary>Gets or sets the event that is called every time when an interactor begins hovering over this interactable.</summary>
        public XRInteractableEvent onHoverEnter { get { return m_OnHoverEnter; } set { m_OnHoverEnter = value; } }

        [SerializeField, Tooltip("Called every time when an interactor stops hovering over this interactable.")]
        XRInteractableEvent m_OnHoverExit = new XRInteractableEvent();
        /// <summary>Gets or sets the event that is called every time when an interactor stops hovering over this interactable.</summary>
        public XRInteractableEvent onHoverExit { get { return m_OnHoverExit; } set { m_OnHoverExit = value; } }

        [SerializeField, Tooltip("Called only when the last interactor stops hovering over this interactable.")]
        XRInteractableEvent m_OnLastHoverExit = new XRInteractableEvent();
        /// <summary>Gets or sets the event that is called only when the last interactor stops hovering over this interactable.</summary>
        public XRInteractableEvent onLastHoverExit { get { return m_OnLastHoverExit; } set { m_OnLastHoverExit = value; } }

        [SerializeField, Tooltip("Called when this interactable enters the select state.")]
        XRInteractableEvent m_OnSelectEnter = new XRInteractableEvent();
        /// <summary>Gets or sets the event that is called when an interactor begins selecting this interactable.</summary>
        public XRInteractableEvent onSelectEnter { get { return m_OnSelectEnter; } set { m_OnSelectEnter = value; } }

        [SerializeField, Tooltip("Called when this interactable exits the select state.")]
        XRInteractableEvent m_OnSelectExit = new XRInteractableEvent();
        /// <summary>Gets or sets the event that is called when an interactor stops selecting this interactable.</summary>
        public XRInteractableEvent onSelectExit { get { return m_OnSelectExit; } set { m_OnSelectExit = value; } }

        [SerializeField]
        [Tooltip("Called when an Interactor activates this selected Interactable.")]
        XRInteractableEvent m_OnActivate = new XRInteractableEvent();
        /// <summary>Gets or sets the event that is called when an Interactor activates this Interactable.</summary>
        public XRInteractableEvent onActivate { get { return m_OnActivate; } set { m_OnActivate = value; } }

        [SerializeField]
        [Tooltip("Called when an Interactor has deactivated this selected Interactable.")]
        XRInteractableEvent m_OnDeactivate = new XRInteractableEvent();
        /// <summary>Gets or sets the event that is called when an Interactor deactivates this Interactable.</summary>
        public XRInteractableEvent onDeactivate { get { return m_OnDeactivate; } set { m_OnDeactivate = value; } }

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

        bool IsOnValidLayerMask(XRBaseInteractor interactor)
        {
            return interactionLayerMask == -1 || interactor.interactionLayerMask == -1 ||
                (interactionLayerMask & interactor.interactionLayerMask) == interactor.interactionLayerMask;
        }

        /// <summary>
        /// Determines if this interactable can be hovered by a given interactor.
        /// </summary>
        /// <param name="interactor">Interactor to check for a valid hover state with.</param>
        /// <returns>True if hovering is valid this frame, False if not.</returns>
        public virtual bool IsHoverableBy(XRBaseInteractor interactor) { return IsOnValidLayerMask(interactor); }

        /// <summary>
        /// Determines if this interactable can be selected by a given interactor.
        /// </summary>
        /// <param name="interactor">Interactor to check for a valid selection with.</param>
        /// <returns>True if selection is valid this frame, False if not.</returns>
        public virtual bool IsSelectableBy(XRBaseInteractor interactor) { return IsOnValidLayerMask(interactor); }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor first initiates hovering over an interactable.</summary>
        /// <param name="interactor">Interactor that is initiating the hover.</param>
        protected internal virtual void OnHoverEnter(XRBaseInteractor interactor) 
		{
            if(m_CustomReticle)
                AttachCustomReticle(interactor);

            isHovered = true;
            m_HoveringInteractors.Add(interactor);

            if (m_HoveringInteractors.Count == 1)
                m_OnFirstHoverEnter?.Invoke(interactor);

            m_OnHoverEnter?.Invoke(interactor);
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor ends hovering over an interactable.</summary>
        /// <param name="interactor">Interactor that is ending the hover.</param>
		protected internal virtual void OnHoverExit(XRBaseInteractor interactor) 
		{
            if (m_CustomReticle)                
                RemoveCustomReticle(interactor);

			isHovered = false;
            if (m_HoveringInteractors.Contains(interactor))
                m_HoveringInteractors.Remove(interactor);

            if (m_HoveringInteractors.Count == 0)
                m_OnLastHoverExit?.Invoke(interactor);

            m_OnHoverExit?.Invoke(interactor);
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor first initiates selection of an interactable.</summary>
        /// <param name="interactor">Interactor that is initiating the selection.</param>
		protected internal virtual void OnSelectEnter(XRBaseInteractor interactor) 
		{
			isSelected = true;
            m_OnSelectEnter?.Invoke(interactor);
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor ends selection of an interactable.</summary>
        /// <param name="interactor">Interactor that is ending the selection.</param>
		protected internal virtual void OnSelectExit(XRBaseInteractor interactor) 
		{
			isSelected = false;
            m_OnSelectExit?.Invoke(interactor);
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor sends an activation event down to an interactable.</summary>
        /// <param name="interactor">Interactor that is sending the activation event.</param>
		protected internal virtual void OnActivate(XRBaseInteractor interactor)
        {
            m_OnActivate?.Invoke(interactor);
        }



        [SerializeField]
        GameObject m_CustomReticle;
        /// <summary>Gets or sets the reticle that will appear at the end of the line when it is valid.</summary>
        public GameObject customReticle { get { return m_CustomReticle; } set { m_CustomReticle = value; } }

        Dictionary<XRBaseInteractor, GameObject> m_ReticleCache = new Dictionary<XRBaseInteractor, GameObject>();


        public virtual void AttachCustomReticle(XRBaseInteractor interactor)
        {
            if (interactor == null)
                return;

            // try and find any attached reticle and swap it
            IXRCustomReticleProvider ilv = interactor.transform.GetComponent<IXRCustomReticleProvider>();
            if (ilv != null)
            {
               
                GameObject prevReticle;
                if (m_ReticleCache.TryGetValue(interactor, out prevReticle))
                {
                    Destroy(prevReticle);
                    m_ReticleCache.Remove(interactor);
                }
                if (m_CustomReticle != null)
                {
                    var rInstance = Instantiate(m_CustomReticle);
                    m_ReticleCache.Add(interactor, rInstance);
                    ilv.AttachCustomReticle(rInstance);
                }
            }
        }

        public virtual void RemoveCustomReticle(XRBaseInteractor interactor)
        {
            if (interactor == null)
                return;

            // try and find any attached reticle and swap it            
            IXRCustomReticleProvider ilv = interactor.transform.GetComponent<IXRCustomReticleProvider>();
            if (ilv != null)
            {
                GameObject reticle;
                bool setCustomReticle = false;
                if (m_ReticleCache.TryGetValue(interactor, out reticle))
                {
                    Destroy(reticle);
                    m_ReticleCache.Remove(interactor);
                    setCustomReticle = true;
                }                
                if ( setCustomReticle)
                {
                    ilv.RemoveCustomReticle();
                }
            }
        }

        protected internal virtual void OnDeactivate(XRBaseInteractor interactor)
        {
            m_OnDeactivate?.Invoke(interactor);
        }

        /// <summary>
        /// This method is called by the interaction manager to update the interactable. 
        /// Please see the interaction manager documentation for more details on update order
        /// </summary>        
        public virtual void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            return;
        }        
    }
}
