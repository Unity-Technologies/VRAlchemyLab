using System;
using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// The result of a locomotion request
    /// </summary>
    public enum RequestResult
    {
        /// <summary>
        /// The locomotion request was successful
        /// </summary>
        Success,
        /// <summary>
        /// The locomotion request failed due to the system being currently busy
        /// </summary>
        Busy,
        /// <summary>
        /// The locomotion request failed due to an unknown error
        /// </summary>
        Error,
    }

    /// <summary>
    /// The LocomotionSystem object is used to control access to the XR Rig. This system enforces that only one
    /// Locomotion Provider can move the XR Rig at one time. This is the only place that access to an XR Rig is controlled,
    /// having multiple LocomotionSystems drive a single XR Rig is not recommended.
    /// </summary>
    public class LocomotionSystem : MonoBehaviour
    {       
        const float Timeout = 10.0f;
        
        LocomotionProvider m_CurrentExclusiveProvider = null;
        float m_TimeMadeExclusive = 0.0f;

        [SerializeField]
        [Tooltip("The timeout for exclusive access to the XR Rig.")]
        float m_Timeout = Timeout; 
        /// <summary>
        /// The timeout for exclusive access to the XR Rig
        /// </summary>
        public float timeout { get { return m_Timeout; } set { m_Timeout = value;}}
        
        [SerializeField]
        [Tooltip("The XR Rig object to provide access control to.")]
        XRRig m_XRRig;
        /// <summary>
        /// The XR Rig object to provide access control to.
        /// </summary>
        public XRRig xrRig { get { return m_XRRig; } set { m_XRRig = value;}}

        /// <summary>
        /// Is a locomotion request already being performed
        /// </summary>
        /// <returns>true if there is already a locomotion in progress</returns>
        public bool Busy
        {
            get
            {
                return m_CurrentExclusiveProvider != null;
            }
        }

        /// <summary>
        /// The RequestExclusiveOperation function will attempt to "lock" access to the XR Rig for the Locomotion Provider passed
        /// </summary>
        /// <param name="provider">The locomotion provider that is requesting access</param>
        /// <returns>A RequestResult that reflects the status of the request</returns>
        public RequestResult RequestExclusiveOperation(LocomotionProvider provider)
        {

            if(provider == null)
                return RequestResult.Error;

            if(m_CurrentExclusiveProvider == null)
            {
                m_CurrentExclusiveProvider = provider;
                m_TimeMadeExclusive = Time.time;
                return RequestResult.Success;
            }

            if(m_CurrentExclusiveProvider != provider)
            {
                return RequestResult.Busy;
            }

            return RequestResult.Error;
        }

        internal void ResetExclusivity()
        {
            m_CurrentExclusiveProvider = null;
            m_TimeMadeExclusive = 0.0f;
        }

        /// <summary>
        /// This function informs the LocomotionSystem that Exclusive Access to the XR Rig is no longer required.
        /// </summary>
        /// <param name="provider">The LocomtionProvider that is relinquishing access.</param>
        /// <returns>A RequestResult that reflects the status of the request</returns>
        public RequestResult FinishExclusiveOperation(LocomotionProvider provider)
        {
            if(provider == null || m_CurrentExclusiveProvider == null)
                return RequestResult.Error;

            if(m_CurrentExclusiveProvider == provider)
            {
                ResetExclusivity();
                return RequestResult.Success;
            }
            else
            {
                return RequestResult.Error;
            }        
        }

        /// <summary>
        /// The standard update function
        /// </summary>
        public void Update()
        {
            if (m_CurrentExclusiveProvider != null && Time.time > m_TimeMadeExclusive + m_Timeout)
            {
                ResetExclusivity();
            }
        }

        private void Awake()
        {
            if(m_XRRig == null)
                m_XRRig = Object.FindObjectOfType<XRRig>();
        }
    }
}