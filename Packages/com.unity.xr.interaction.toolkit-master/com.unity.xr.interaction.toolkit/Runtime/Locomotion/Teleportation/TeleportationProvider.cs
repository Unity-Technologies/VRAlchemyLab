using System;
using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit
{

    public class TeleportationProvider : LocomotionProvider
    {

        // the current teleportation request
        TeleportRequest m_CurrentRequest;
        // whether the current teleportation request is valid.
        bool m_ValidRequest = false;


        /// <summary>
        /// This function will queue a teleportation request within the provider. 
        /// </summary>
        /// <param name="teleportRequest">The teleportation request</param>
        /// <returns>true if successful.</returns>
        public bool QueueTeleportRequest(TeleportRequest teleportRequest)
        {
            m_CurrentRequest = teleportRequest;
            m_ValidRequest = true;
            return true;
        }

        /// <summary>
        /// Update function for the Teleportation Provider
        /// </summary>
        private void Update()
        {
            if(m_ValidRequest && BeginLocomotion())
            {
                var xrRig = system.xrRig;
                if (xrRig != null)
                {
                    switch (m_CurrentRequest.matchOrientation)
                    {
                        case MatchOrientation.None:
                            xrRig.MatchRigUp(m_CurrentRequest.destinationUpVector);
                            break;
                        case MatchOrientation.Camera:
                            xrRig.MatchRigUpCameraForward(m_CurrentRequest.destinationUpVector, m_CurrentRequest.destinationForwardVector);
                            break;
                        //case MatchOrientation.Rig:
                        //    xrRig.MatchRigUpRigForward(m_CurrentRequest.destinationUpVector, m_CurrentRequest.destinationForwardVector);
                        //    break;
                    }

                    Vector3 heightAdjustment = xrRig.rig.transform.up * xrRig.cameraInRigSpaceHeight;

                    Vector3 cameraDestination = m_CurrentRequest.destinationPosition + heightAdjustment;

                    xrRig.MoveCameraToWorldLocation(cameraDestination);
                }
                EndLocomotion();
                m_ValidRequest = false;
            }          
        }
    }
}
 
 