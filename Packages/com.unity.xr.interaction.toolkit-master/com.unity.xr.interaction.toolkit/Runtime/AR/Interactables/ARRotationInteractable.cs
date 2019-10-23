//-----------------------------------------------------------------------
// <copyright file="RotationManipulator.cs" company="Google">
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
namespace UnityEngine.XR.Interaction.Toolkit.AR {  public class ARRotationInteractable {} }

#else

using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit.AR
{
    /// <summary>
    /// Manipulates the rotation of an object via a drag or a twist gesture.
    /// If an object is selected, then dragging along the horizontal axis
    /// or performing a twist gesture will rotate along the y-axis of the item.
    /// </summary>
    public class ARRotationInteractable : ARBaseGestureInteractable
    {
        [SerializeField, Tooltip("Rate at which to rotate object with a drag.")]
        float m_RotationRateDegreesDrag = 100.0f;
        
        /// <summary>Gets or sets the rate at which to rotate object with a drag.</summary>
        public float RotationRateDegreesDrag { 
            get { return m_RotationRateDegreesDrag; }
            set { m_RotationRateDegreesDrag = value; }
        }
        
        [SerializeField, Tooltip("Rate at which to rotate object with a twist.")]
        float m_RotationRateDegreesTwist = 2.5f;
        
        /// <summary>Gets or sets the rate at which to rotate object with a twist.</summary>
        public float RotationRateDegreesTwist { 
            get { return m_RotationRateDegreesTwist; }
            set { m_RotationRateDegreesTwist = value; }
        }

        /// <summary>
        /// Returns true if the manipulation can be started for the given Drag gesture.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        /// <returns>True if the manipulation can be started.</returns>
        protected override bool CanStartManipulationForGesture(DragGesture gesture)
        {
            if (!IsGameObjectSelected())
                return false;

            if (gesture.TargetObject != null)
                return false;

            return true;
        }

        /// <summary>
        /// Returns true if the manipulation can be started for the given Twist gesture.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        /// <returns>True if the manipulation can be started.</returns>
        protected override bool CanStartManipulationForGesture(TwistGesture gesture)
        {
            if (!IsGameObjectSelected())
                return false;

            if (gesture.TargetObject != null)
                return false;

            return true;
        }

        /// <summary>
        /// Rotates the object around the y-axis via a Drag gesture.
        /// </summary>
        /// <param name="gesture">The current drag gesture.</param>
        protected override void OnContinueManipulation(DragGesture gesture)
        {
            var forward = Camera.main.transform.forward;
            var worldToVerticalOrientedDevice = Quaternion.Inverse(Quaternion.LookRotation(forward, Vector3.up));
            var deviceToWorld = Camera.main.transform.rotation;
            var rotatedDelta = worldToVerticalOrientedDevice * deviceToWorld * gesture.Delta;

            var rotationAmount = -1.0f * (rotatedDelta.x / Screen.dpi) * m_RotationRateDegreesDrag;
            transform.Rotate(0.0f, rotationAmount, 0.0f);
        }

        /// <summary>
        /// Rotates the object around the y-axis via a Twist gesture.
        /// </summary>
        /// <param name="gesture">The current twist gesture.</param>
        protected override void OnContinueManipulation(TwistGesture gesture)
        {
            var rotationAmount = -gesture.DeltaRotation * m_RotationRateDegreesTwist;
            transform.Rotate(0.0f, rotationAmount, 0.0f);
        }
    }
}

#endif