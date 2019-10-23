using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AR;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// The Interaction Manager acts as an intermediary between Interactors and Interactables in a scene.  
    /// It is possible to have multiple Interaction Managers, each with their own valid set of Interactors and Interactables.  
    /// Upon Awake both Interactors and Interactables register themselves with a valid Interaction Manager in the scene 
    /// (if a specific one has not already been assigned in the inspector).  Every scene must have at least one Interaction Mananger
    /// for Interactors and Interactables to be able to communicate.
    /// </summary>
    /// <remarks>
    /// Many of the methods on this class are designed to be internal such that they can be called by the abstract
    /// base classes of the Interaction system (but are not called directly).
    /// </remarks>
	[AddComponentMenu("XR/XR Interaction Manager")]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_InteractionManager)]

	public class XRInteractionManager : MonoBehaviour
	{        

        List<XRBaseInteractor> 	m_Interactors 	= new List<XRBaseInteractor>();
		List<XRBaseInteractable> m_Interactables = new List<XRBaseInteractable>();

        // internal properties for accessing Interactors and Interactables (used by XR Interaction Debugger)
        internal List<XRBaseInteractor> interactors { get { return m_Interactors; } }
        internal List<XRBaseInteractable> interactables { get { return m_Interactables; } }

        // map of all registered objects to test for colliding
        Dictionary<Collider, XRBaseInteractable> m_ColliderToInteractableMap = new Dictionary<Collider, XRBaseInteractable>();

        // reusable list of interactables for retrieving hover targets
        List<XRBaseInteractable> m_HoverTargetList = new List<XRBaseInteractable>();

        // reusable list of valid targets for and interactor
        List<XRBaseInteractable> m_InteractorValidTargets = new List<XRBaseInteractable>();

#if AR_FOUNDATION_PRESENT
        // Flag to indicate that interactables should be reconnected to gestures next frame
        bool m_GestureInteractablesNeedReconnect = false;
#endif

        protected virtual void OnEnable()
        {
            Application.onBeforeRender += OnBeforeRender;
        }

        protected virtual void OnDisable()
        {
            Application.onBeforeRender -= OnBeforeRender;
        }

        [BeforeRenderOrder(XRInteractionUpdateOrder.k_BeforeRenderOrder)]
        void OnBeforeRender()
        {
            ProcessInteractors(XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender);
            ProcessInteractables(XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender);
        }

        void Awake()
        {
            foreach (var interactor in m_Interactors)
            {
                if (interactor.startingSelectedInteractable)
                    SelectEnter(interactor, interactor.startingSelectedInteractable);
            }
        }

        void ProcessInteractors(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            foreach (var interactor in m_Interactors)
            {
                interactor.ProcessInteractor(updatePhase);
            }
        }
           
        void ProcessInteractables(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            foreach (var interactable in m_Interactables)
            {
                interactable.ProcessInteractable(updatePhase);
            }
        }

        private void LateUpdate()
        {
            ProcessInteractors(XRInteractionUpdateOrder.UpdatePhase.Late);
            ProcessInteractables(XRInteractionUpdateOrder.UpdatePhase.Late);
        }

        private void FixedUpdate()
        {
            ProcessInteractors(XRInteractionUpdateOrder.UpdatePhase.Fixed);
            ProcessInteractables(XRInteractionUpdateOrder.UpdatePhase.Fixed);
        }

        void Update()
		{
            ProcessInteractors(XRInteractionUpdateOrder.UpdatePhase.Dynamic);

            foreach (var interactor in m_Interactors)
			{
				GetValidTargets(interactor, m_InteractorValidTargets);

				ClearInteractorSelection(interactor); 
				ClearInteractorHover(interactor, m_InteractorValidTargets);
				InteractorSelectValidTargets(interactor, m_InteractorValidTargets);
				InteractorHoverValidTargets(interactor, m_InteractorValidTargets);
			}

            ProcessInteractables(XRInteractionUpdateOrder.UpdatePhase.Dynamic);

#if AR_FOUNDATION_PRESENT
            // Check if gesture interactors/interactables have been updated
            // (in which case we need to reconnect gestures).
            if (m_GestureInteractablesNeedReconnect)
            {
	            foreach (var interactable in m_Interactables)
	            {
		            var gestureInteractable = interactable as ARBaseGestureInteractable;
		            if (gestureInteractable != null)
		            {
			            gestureInteractable.DisconnectGestureInteractor();
			            gestureInteractable.ConnectGestureInteractor();
		            }
	            }

	            m_GestureInteractablesNeedReconnect = false;
            }
#endif
        }

		internal void RegisterInteractor(XRBaseInteractor interactor)
		{
			if (!m_Interactors.Contains(interactor))
			{
				m_Interactors.Add(interactor);
#if AR_FOUNDATION_PRESENT
				if (interactor is ARGestureInteractor)
					m_GestureInteractablesNeedReconnect = true;
#endif
			}

		}

      


		internal void UnregisterInteractor(XRBaseInteractor interactor)
		{
            if (m_Interactors.Contains(interactor))
            {                           
                ClearInteractorHover(interactor, null);                
                ClearInteractorSelection(interactor);                
                
                m_Interactors.Remove(interactor);
#if AR_FOUNDATION_PRESENT
				if (interactor is ARGestureInteractor)
					m_GestureInteractablesNeedReconnect = true;
#endif
			}
		}

		internal void RegisterInteractable(XRBaseInteractable interactable)
		{
            if (!m_Interactables.Contains(interactable))
            {
                m_Interactables.Add(interactable);
                
                foreach (var collider in interactable.colliders)
                {
                    if (collider != null && !m_ColliderToInteractableMap.ContainsKey(collider))
                        m_ColliderToInteractableMap.Add(collider, interactable);
                }
#if AR_FOUNDATION_PRESENT
                if (interactable is ARBaseGestureInteractable)
					m_GestureInteractablesNeedReconnect = true;
#endif
            }
		}

		internal void UnregisterInteractable(XRBaseInteractable interactable)
		{
            if (m_Interactables.Contains(interactable))
            {
                m_Interactables.Remove(interactable);

                foreach (var collider in interactable.colliders)
                {
                    if (collider != null && m_ColliderToInteractableMap.ContainsKey(collider))
                        m_ColliderToInteractableMap.Remove(collider);
                }
#if AR_FOUNDATION_PRESENT
                if (interactable is ARBaseGestureInteractable)
	                m_GestureInteractablesNeedReconnect = true;
#endif
            }
		}

		internal XRBaseInteractable TryGetInteractableForCollider(Collider collider)
		{
			XRBaseInteractable interactable;
            if (collider != null && m_ColliderToInteractableMap.TryGetValue(collider, out interactable))
                return interactable;
            
            return null;
		}

		internal List<XRBaseInteractable> GetValidTargets(XRBaseInteractor interactor, List<XRBaseInteractable> validTargets)
		{
			interactor.GetValidTargets(validTargets);

			// Remove interactables that are not being handled by this manager.
            for (int i = validTargets.Count - 1; i >= 0; --i)
            {
                if (!m_Interactables.Contains(validTargets[i]))
                    validTargets.RemoveAt(i);
            }
			return validTargets;
		}

		void ClearInteractorSelection(XRBaseInteractor interactor)
		{
            // TODO: Make sure SelectExit is called if the selectTarget of the interactor is destroyed (and write a test around this).
            if (interactor.selectTarget &&
				(!interactor.isSelectActive || !interactor.CanSelect(interactor.selectTarget) || !interactor.selectTarget.IsSelectableBy(interactor)))
                SelectExit(interactor, interactor.selectTarget);
		}

		void ClearInteractorHover(XRBaseInteractor interactor, List<XRBaseInteractable> validTargets)
		{
            interactor.GetHoverTargets(m_HoverTargetList);
            for (int i = 0; i < m_HoverTargetList.Count; i++)
            {
                var target = m_HoverTargetList[i];
                if (!interactor.isHoverActive || !interactor.CanHover(target) || !target.IsHoverableBy(interactor) || ((validTargets != null && !validTargets.Contains(target)) || validTargets == null))
                    HoverExit(interactor, target);
            }
		}

        void SelectEnter(XRBaseInteractor interactor, XRBaseInteractable interactable)
        {
            // allow new exclusive selection to take precedence over previous non-exclusive selection (useful Interactors like Sockets)
            if (interactor.isSelectExclusive)
            {
                for (int i = 0; i < m_Interactors.Count; i++)
                {
                    if (m_Interactors[i].selectTarget == interactable && !m_Interactors[i].isSelectExclusive)
                        SelectExit(m_Interactors[i], interactable);
                }
            }
            interactor.OnSelectEnter(interactable);
            interactable.OnSelectEnter(interactor);
        }

        void SelectExit(XRBaseInteractor interactor, XRBaseInteractable interactable)
        {
            interactor.OnSelectExit(interactable);
            interactable.OnSelectExit(interactor);
        }

        void HoverEnter(XRBaseInteractor interactor, XRBaseInteractable interactable)
        {
            interactor.OnHoverEnter(interactable);
            interactable.OnHoverEnter(interactor);
        }

        void HoverExit(XRBaseInteractor interactor, XRBaseInteractable interactable)
        {
            interactor.OnHoverExit(interactable);
            interactable.OnHoverExit(interactor);
        }

        void InteractorSelectValidTargets(XRBaseInteractor interactor, List<XRBaseInteractable> validTargets)
		{
			if (interactor.isSelectActive)
			{
				for (var i=0; i < validTargets.Count && interactor.isSelectActive; ++i)
				{
                    if (interactor.CanSelect(validTargets[i]) && validTargets[i].IsSelectableBy(interactor) &&
                    	interactor.selectTarget != validTargets[i])
                    {
                        SelectEnter(interactor, validTargets[i]);
                    }
				}
			}
		}

		void InteractorHoverValidTargets(XRBaseInteractor interactor, List<XRBaseInteractable> validTargets)
		{
			if (interactor.isHoverActive)
			{
				for (var i=0; i < validTargets.Count && interactor.isHoverActive; ++i)
				{
                    interactor.GetHoverTargets(m_HoverTargetList);
                    if (interactor.CanHover(validTargets[i]) && validTargets[i].IsHoverableBy(interactor) &&
                    	!m_HoverTargetList.Contains(validTargets[i]))
                    {
                        HoverEnter(interactor, validTargets[i]);
                    }
				}
			}
		}
	}
}
