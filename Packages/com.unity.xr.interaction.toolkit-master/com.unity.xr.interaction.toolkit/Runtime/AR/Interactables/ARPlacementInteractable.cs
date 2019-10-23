//-----------------------------------------------------------------------
// <copyright originalFile="AndyPlacementManipulator.cs" company="Google">
// <renamed file="ARPlacementInteractable.cs">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

#if !AR_FOUNDATION_PRESENT

// Stub class definition used to fool version defines that this MonoScript exists (fixed in 19.3)
namespace UnityEngine.XR.Interaction.Toolkit.AR {  public class ARPlacementInteractable {} }

#else

using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Interaction.Toolkit.AR
{

    /// <summary>
    /// UnityEvent that responds to changes of hover and selection by this interactor.
    /// </summary>
    [Serializable]
    public class ARObjectPlacedEvent : UnityEvent<ARPlacementInteractable, GameObject> { }
    
    /// <summary>
    /// Controls the placement of Andy objects via a tap gesture.
    /// </summary>
    public class ARPlacementInteractable : ARBaseGestureInteractable
    {
        /// <summary>
        /// A GameObject to place when a raycast from a user touch hits a plane.
        /// </summary>
        public GameObject PlacementPrefab;

        [SerializeField, Tooltip("Called when the this interactable places a new GameObject in the world.")]
        ARObjectPlacedEvent m_OnObjectPlaced = new ARObjectPlacedEvent();
        /// <summary>Gets or sets the event that is called when the this interactable places a new GameObject in the world.</summary>
        public ARObjectPlacedEvent onObjectPlaced { get { return m_OnObjectPlaced; } set { m_OnObjectPlaced = value; } }
        
        static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
        static GameObject s_TrackablesObject;

        /// <summary>
        /// Returns true if the manipulation can be started for the given gesture.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        /// <returns>True if the manipulation can be started.</returns>
        protected override bool CanStartManipulationForGesture(TapGesture gesture)
        {
            // Allow for test planes
            if (gesture.TargetObject == null || gesture.TargetObject.layer == 9)
                return true;

            return false;
        }

        /// <summary>
        /// Function called when the manipulation is ended.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected override void OnEndManipulation(TapGesture gesture)
        {
            if (gesture.WasCancelled)
                return;

            // If gesture is targeting an existing object we are done.
            // Allow for test planes
            if (gesture.TargetObject != null && gesture.TargetObject.layer != 9)
                return;
            
            // Raycast against the location the player touched to search for planes.
            if (GestureTransformationUtility.Raycast(gesture.StartPosition, s_Hits, TrackableType.PlaneWithinPolygon))
            {
                var hit = s_Hits[0];
                
                // Use hit pose and camera pose to check if hittest is from the
                // back of the plane, if it is, no need to create the anchor.
                if (Vector3.Dot(Camera.main.transform.position - hit.pose.position,
                        hit.pose.rotation * Vector3.up) < 0)
                    return;
                
                // Instantiate placement prefab at the hit pose.
                var placementObject = Instantiate(PlacementPrefab, hit.pose.position, hit.pose.rotation);

                // Create anchor to track reference point and set it as the parent of placementObject.
                // TODO: this should update with a reference point for better tracking.
                var anchorObject = new GameObject("PlacementAnchor");
                anchorObject.transform.position = hit.pose.position;
                anchorObject.transform.rotation = hit.pose.rotation;
                placementObject.transform.parent = anchorObject.transform;

                // Find trackables object in scene and use that as parent
                if (s_TrackablesObject == null)
                    s_TrackablesObject = GameObject.Find("Trackables");
                if (s_TrackablesObject != null)
                    anchorObject.transform.parent = s_TrackablesObject.transform;

                if (m_OnObjectPlaced != null)
                    m_OnObjectPlaced.Invoke(this, placementObject);
            }
        }
    }
}

#endif