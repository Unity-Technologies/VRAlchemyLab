using System;
using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// The locomotion provider is the base class for various locomotion implementations.
    /// this class provides simple ways to interrogate the locomotion system for whether a locomotion can begin
    /// and simple events for hooking into a start/end locomotion.
    /// </summary>
    public abstract class LocomotionProvider : MonoBehaviour
    {

        [SerializeField]
        [Tooltip("The locomotion system that this Locomotion Provider will communicate with for exclusive access to an XR Rig")]        
        LocomotionSystem m_System;
        /// <summary>
        /// The Locomotion system that this locomotion provider will communicate with for exclusive access to an XR Rig.
        /// If one is not provided, the system will attempt to locate one during its Awake call
        /// </summary>
        public LocomotionSystem system { get { return m_System;} set { m_System = value;}}

        protected virtual void Awake()
        {
            if (!m_System)
                m_System = Object.FindObjectOfType<LocomotionSystem>();
        }

        protected bool CanBeginLocomotion()
        {
            if (m_System == null)
            {               
                return false;
            }

            return !m_System.Busy;
        }

        protected bool BeginLocomotion()
        {
            if (m_System == null)
            {                
                return false;
            }

            bool ret = (m_System.RequestExclusiveOperation(this) == RequestResult.Success);
            if (ret && startLocomotion != null)
                startLocomotion(m_System);

            return ret;            
        }

        protected bool EndLocomotion()
        {
            if (m_System == null)
            {      
                return false;
            }

            bool ret = (m_System.FinishExclusiveOperation(this) == RequestResult.Success);
            if (ret && endLocomotion != null)
                endLocomotion(m_System);

            return ret;
        }


        /// <summary>
        /// The startingLocomotion action will be called when a Locomotion Provider starts a locomotion event
        /// </summary>
        public event Action<LocomotionSystem> startLocomotion;

        /// <summary>
        /// The endingLocomotion action will be called when a Locomotion Provider stops a locomotion event
        /// </summary>
        public event Action<LocomotionSystem> endLocomotion;

    }
}
