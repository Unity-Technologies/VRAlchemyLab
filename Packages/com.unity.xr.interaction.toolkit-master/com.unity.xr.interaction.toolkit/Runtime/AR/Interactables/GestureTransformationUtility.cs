//-----------------------------------------------------------------------
// <copyright file="GestureTransformationUtility.cs" company="Google">
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

#if AR_FOUNDATION_PRESENT

using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Interaction.Toolkit.AR
{
    /// <summary>
    /// Provides helper functions for common functionality for transforming objects in AR.
    /// </summary>
    public static class GestureTransformationUtility
    {
        /// <summary>
        /// Translation mode.
        /// </summary>
        public enum GestureTranslationMode
        {
            Horizontal,
            Vertical,
            Any,
        }

        /// <summary>
        /// Slight offset of the down ray used in GetBestPlacementPosition to ensure that the
        /// current groundingPlane is included in the hit results.
        /// </summary>
        const float k_DownRayOffset = 0.01f;

        /// <summary>
        /// Max amount (inches) to offset the screen touch in GetBestPlacementPosition.
        /// The actual amount if dependent on the angle of the camera relative.
        /// The further downward the camera is angled, the more the screen touch is offset.
        /// </summary>
        const float k_MaxScreenTouchOffset = 0.4f;

        /// <summary>
        /// In GetBestPlacementPosition, when the camera is closer than this value to the object,
        /// reduce how much the object hovers.
        /// </summary>
        const float k_HoverDistanceThreshold = 1.0f;
        
        static ARRaycastManager s_ARRaycastManager;
        static ARPlaneManager s_ARPlaneManager;
        static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

        static bool CheckDependentManagers()
        {
            if (s_ARRaycastManager == null)
            {
                s_ARRaycastManager = Object.FindObjectOfType<ARRaycastManager>();
                if (s_ARRaycastManager == null)
                {
                    Debug.LogWarning("Could not find ARRaycastManager in scene.");
                    return false;
                }
            }
            if (s_ARPlaneManager == null)
            {
                s_ARPlaneManager = Object.FindObjectOfType<ARPlaneManager>();
                if (s_ARPlaneManager == null)
                {
                    Debug.LogWarning("Could not find ARPlaneManager in scene.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Cast a ray from a point in screen space against trackables, i.e., detected features such as planes.
        /// </summary>
        /// <param name="screenPoint">The point, in device screen pixels, from which to cast.</param>
        /// <param name="hitResults">Contents are replaced with the raycast results, if successful.</param>
        /// <param name="trackableTypes">(Optional) The types of trackables to cast against.</param>
        /// <returns>True if the raycast hit a trackable in the <paramref name="trackableTypes"/></returns>
        public static bool Raycast(Vector2 screenPoint, List<ARRaycastHit> hitResults,
            TrackableType trackableTypes = TrackableType.All)
        {
            hitResults.Clear();

            if (CheckDependentManagers() && s_ARRaycastManager.Raycast(screenPoint, hitResults, trackableTypes))
                return true;
            
            // No hits or managers, try debug planes
            var sessionOrigin = Object.FindObjectOfType<ARSessionOrigin>();
            
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(screenPoint);
            if (Physics.Raycast(ray, out hit, float.MaxValue, 1 << 9))
            {
                hitResults.Add(new ARRaycastHit(
                    new XRRaycastHit(TrackableId.invalidId,
                        new Pose(hit.point, Quaternion.LookRotation(Vector3.forward, hit.normal)),
                        hit.distance, TrackableType.PlaneWithinPolygon),
                    hit.distance, sessionOrigin != null ? sessionOrigin.transform : hit.collider.transform));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Calculates the best position to place an object in AR based on screen position.
        /// Could be used for tapping a location on the screen, dragging an object, or using a fixed
        /// cursor in the center of the screen for placing and moving objects.
        ///
        /// Objects are placed along the x/z of the grounding plane. When placed on an AR plane
        /// below the grounding plane, the object will drop straight down onto it in world space.
        /// This prevents the object from being pushed deeper into the scene when moving from a
        /// higher plane to a lower plane. When moving from a lower plane to a higher plane, this
        /// function returns a new groundingPlane to replace the old one.
        /// </summary>
        /// <returns>The best placement position.</returns>
        /// <param name="currentAnchorPosition">Position of the parent anchor, i.e., where the
        /// object is before translation starts.</param>
        /// <param name="screenPos">Location on the screen in pixels to place the object at.</param>
        /// <param name="groundingPlaneHeight">The starting height of the plane that the object is
        /// being placed along.</param>
        /// <param name="hoverOffset">How much should the object hover above the groundingPlane
        /// before it has been placed.</param>
        /// <param name="maxTranslationDistance">The maximum distance that the object can be
        /// translated.</param>
        /// <param name="gestureTranslationMode">The translation mode, indicating the plane types allowed.
        /// </param>
        public static Placement GetBestPlacementPosition(
            Vector3 currentAnchorPosition,
            Vector2 screenPos,
            float groundingPlaneHeight,
            float hoverOffset,
            float maxTranslationDistance,
            GestureTranslationMode gestureTranslationMode)
        {
            Placement result = new Placement();
            if (!CheckDependentManagers())
                return result;
            
            result.UpdatedGroundingPlaneHeight = groundingPlaneHeight;

            // Get the angle between the camera and the object's down direction.
            float angle = Vector3.Angle(Camera.main.transform.forward, Vector3.down);
            angle = 90.0f - angle;

            float touchOffsetRatio = Mathf.Clamp01(angle / 90.0f);
            float screenTouchOffset = touchOffsetRatio * k_MaxScreenTouchOffset;
            screenPos.y += GestureTouchesUtility.InchesToPixels(screenTouchOffset);

            float hoverRatio = Mathf.Clamp01(angle / 45.0f);
            hoverOffset *= hoverRatio;

            float distance = (Camera.main.transform.position - currentAnchorPosition).magnitude;
            float distanceHoverRatio = Mathf.Clamp01(distance / k_HoverDistanceThreshold);
            hoverOffset *= distanceHoverRatio;

            // The best estimate of the point in the plane where the object will be placed:
            Vector3 groundingPoint;

            // Get the ray to cast into the scene from the perspective of the camera.
            if (Raycast(new Vector2(screenPos.x, screenPos.y), s_Hits, TrackableType.Planes))
            {
                var firstHit = s_Hits[0];
                var plane = s_ARPlaneManager.GetPlane(firstHit.trackableId);
                if (plane == null || IsPlaneTypeAllowed(gestureTranslationMode, plane.alignment))
                {
                    // Avoid detecting the back of existing planes.
                    if (Vector3.Dot(Camera.main.transform.position - firstHit.pose.position,
                                    firstHit.pose.rotation * Vector3.up) < 0)
                        return result;

                    // Don't allow hovering for vertical or horizontal downward facing planes.
                    if (plane == null ||
                        (plane.alignment == PlaneAlignment.Vertical ||
                        plane.alignment == PlaneAlignment.HorizontalDown ||
                        plane.alignment == PlaneAlignment.HorizontalUp))
                    {
                        groundingPoint = LimitTranslation(
                            firstHit.pose.position, currentAnchorPosition, maxTranslationDistance);

                        if (plane != null)
                        {
                            result.PlacementPlane = plane;
                            result.HasPlane = true;
                        }

                        result.HasPlacementPosition = true;
                        result.PlacementPosition = groundingPoint;
                        result.HasHoveringPosition = true;
                        result.HoveringPosition = groundingPoint;
                        result.UpdatedGroundingPlaneHeight = groundingPoint.y;
                        result.PlacementRotation = firstHit.pose.rotation;
                        return result;
                    }
                }
                else
                {
                    // Plane type not allowed.
                    return result;
                }
            }

            // Return early if the camera is pointing upwards.
            if (angle < 0f)
            {
                return result;
            }

            // If the grounding point is lower than the current grounding plane height, or if the
            // raycast did not return a hit, then we extend the grounding plane to infinity, and do
            // a new raycast into the scene from the perspective of the camera.
            Ray cameraRay = Camera.main.ScreenPointToRay(screenPos);
            Plane groundingPlane =
                new Plane(Vector3.up, new Vector3(0.0f, groundingPlaneHeight, 0.0f));

            // Find the hovering position by casting from the camera onto the grounding plane
            // and offsetting the result by the hover offset.
            float enter;
            if (groundingPlane.Raycast(cameraRay, out enter))
            {
                groundingPoint = LimitTranslation(
                    cameraRay.GetPoint(enter), currentAnchorPosition, maxTranslationDistance);

                result.HasHoveringPosition = true;
                result.HoveringPosition = groundingPoint + (Vector3.up * hoverOffset);
            }
            else
            {
                // If we can't successfully cast onto the groundingPlane, just return early.
                return result;
            }

            return result;
        }

        /// <summary>
        /// Limits the translation to the maximum distance allowed.
        /// </summary>
        /// <returns>The new target position, limited so that the object does not tranlsate more
        /// than the maximum allowed distance.</returns>
        /// <param name="desiredPosition">Desired position.</param>
        /// <param name="currentPosition">Current position.</param>
        /// <param name="maxTranslationDistance">Max translation distance.</param>
        static Vector3 LimitTranslation(Vector3 desiredPosition, Vector3 currentPosition,
                                                float maxTranslationDistance)
        {
            if ((desiredPosition - currentPosition).sqrMagnitude > Mathf.Pow(maxTranslationDistance, 2.0f))
            {
                return currentPosition + (
                    (desiredPosition - currentPosition).normalized * maxTranslationDistance);
            }

            return desiredPosition;
        }

        static bool IsPlaneTypeAllowed(GestureTranslationMode gestureTranslationMode, PlaneAlignment planeAlignment)
        {
            if (gestureTranslationMode == GestureTranslationMode.Any)
            {
                return true;
            }

            if (gestureTranslationMode == GestureTranslationMode.Horizontal &&
               (planeAlignment == PlaneAlignment.HorizontalDown ||
                planeAlignment == PlaneAlignment.HorizontalUp))
            {
                return true;
            }

            if (gestureTranslationMode == GestureTranslationMode.Vertical &&
               planeAlignment == PlaneAlignment.Vertical)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Result of the function GetBestPlacementPosition that indicates if a placementPosition
        /// was found and information about the placement position.
        /// </summary>
        public struct Placement
        {
            /// <summary>
            /// True if this Placement has a hovering position, else false;
            /// </summary>
            public bool HasHoveringPosition;
            
            /// <summary>
            /// The position that the object should be displayed at before the placement has been
            /// confirmed.
            /// </summary>
            public Vector3 HoveringPosition;

            /// <summary>
            /// True if this Placement has a placement position, else false.
            /// </summary>
            public bool HasPlacementPosition;
            
            /// <summary>
            /// The resulting position that the object should be placed at.
            /// </summary>
            public Vector3 PlacementPosition;

            /// <summary>
            /// The resulting rotation that the object should have.
            /// </summary>
            public Quaternion PlacementRotation;

            /// <summary>
            /// True if this Placement has a plane, else false.
            /// </summary>
            public bool HasPlane;

            /// <summary>
            /// The AR Plane that the object is being placed on.
            /// </summary>
            public ARPlane PlacementPlane;

            /// <summary>
            /// This is the updated groundingPlaneHeight resulting from this hit detection.
            /// </summary>
            public float UpdatedGroundingPlaneHeight;
        }
    }
}

#endif
