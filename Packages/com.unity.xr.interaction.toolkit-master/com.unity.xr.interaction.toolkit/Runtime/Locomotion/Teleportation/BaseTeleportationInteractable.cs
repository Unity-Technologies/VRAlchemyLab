using System;
using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// The option of which object's orientation in the rig will be matched with the destination after teleporting.
    /// </summary>
    public enum MatchOrientation
    {
        None = 0,
        Camera = 1,
        // Rig = 2,
    }

    /// <summary>
    /// The Teleport Request that describes the result of the teleportation action. Each Teleportation Interactable must fill out a Teleport Request
    /// for each teleport action. 
    /// </summary>
    public struct TeleportRequest
    {
        /// <summary>
        /// The position in world space of the Teleportation Destination
        /// </summary>
        public Vector3 destinationPosition;
        /// <summary>
        /// The rotation in world space of the Teleportation Destination, This is used primarily for matching world rotations directly
        /// </summary>
        public Quaternion destinationRotation;
        /// <summary>
        /// This vector describes the upwards facing normal of the destination. This is used to match the Rig's up vector to the destination's up vector.
        /// </summary>
        public Vector3 destinationUpVector;
        /// <summary>
        /// This vector desecribes the forward normal of the destination. This is used to match the Rig, or Head's heading to the forward direction of the destination
        /// </summary>
        public Vector3 destinationForwardVector;
        /// <summary>
        ///  The Time (in unix epoch) of the request
        /// </summary>
        public float requestTime;
        /// <summary>
        /// The option of which object's orientation in the rig will be matched with the destination after teleportation.
        /// </summary>
        public MatchOrientation matchOrientation;
    }

    /// <summary>
    /// This is intended to be the base class for all Teleportation Interactables. This abstracts the teleport request process for specalizations of this class.
    /// </summary>
    public abstract class BaseTeleportationInteractable : XRBaseInteractable
    {
        [Header("Teleportation")]

        [SerializeField]
        [Tooltip("The teleportation provider that this Teleport interactable will communicate Teleportation Requests to.")]
        protected TeleportationProvider m_TeleportationProvider = null;
        /// <summary>
        /// The teleportation provider that this Teleport interactable will communicate Teleportation Requests to.
        /// If no teleportation provider is configured, then on awake the base teleportation interactable will attempt to find a teleportation provider to work with.
        /// </summary>
        public TeleportationProvider teleportationProvider { get { return m_TeleportationProvider; } set { m_TeleportationProvider = value; } }

        [SerializeField]
        [Tooltip("Specify which object's orientation in the rig will be matched with this Teleportation Interactable after teleporting. " +
            "Set to Camera if you wish to have the camera look at something in the forward direction of this Interactable or its specified anchor.")]
        protected MatchOrientation m_MatchOrientation = MatchOrientation.None;
        /// <summary>
        /// Specify which object's orientation in the rig will be matched with this Teleportation Interactable after teleporting.
        /// Set to Camera if you wish to have the camera look at something in the forward direction of this Interactable or its specified anchor.
        /// </summary>
        public MatchOrientation matchOrientation { get { return m_MatchOrientation; } set { m_MatchOrientation = value; } }

        public enum TeleportTrigger
        {
            OnSelectExit = 0,
            OnSelectEnter = 1,
        }

        [SerializeField]
        [Tooltip("Specify when the teleportation will be triggered. Options map to when the trigger is pressed or when it is released.")]
        protected TeleportTrigger m_TeleportTrigger = TeleportTrigger.OnSelectExit;
        /// <summary>
        /// Specify when the teleportation will be triggered.
        /// </summary>
        public TeleportTrigger teleportTrigger { get { return m_TeleportTrigger; } set { m_TeleportTrigger = value; } }


        protected override void Awake()
        {
            base.Awake();
            if(m_TeleportationProvider==null)
            {
                m_TeleportationProvider = FindObjectOfType<TeleportationProvider>();
            }
        }

        protected virtual bool GenerateTeleportRequest(XRBaseInteractor interactor, RaycastHit raycastHit, ref TeleportRequest teleportRequest)
        {
            return false;
        }


        void SendTeleportRequest(XRBaseInteractor interactor)
        {
            if (!interactor || m_TeleportationProvider == null)
                return;

            XRRayInteractor rayInt = interactor as XRRayInteractor;
            if (rayInt != null)
            {
                RaycastHit raycastHit;
                if (rayInt.GetCurrentRaycastHit(out raycastHit))
                {
                    // are we still selecting this object   
                    bool found = false;
                    foreach (Collider collider in colliders)
                    {
                        if (collider == raycastHit.collider)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        TeleportRequest tr = new TeleportRequest();
                        tr.matchOrientation = m_MatchOrientation;
                        tr.requestTime = Time.time;
                        if (GenerateTeleportRequest(interactor, raycastHit, ref tr))
                        {
                            m_TeleportationProvider.QueueTeleportRequest(tr);
                        }
                    }
                }
            }
        }

        protected internal override void OnSelectEnter(XRBaseInteractor interactor)
        {
            if (m_TeleportTrigger == TeleportTrigger.OnSelectEnter)
                SendTeleportRequest(interactor);

            base.OnSelectEnter(interactor);
        }

        protected internal override void OnSelectExit(XRBaseInteractor interactor)
        {
            if (m_TeleportTrigger == TeleportTrigger.OnSelectExit)
                SendTeleportRequest(interactor);

            base.OnSelectExit(interactor);
        }
    }
}

