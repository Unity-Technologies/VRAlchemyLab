//-----------------------------------------------------------------------
// <copyright file="Manipulator.cs" company="Google">
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
namespace UnityEngine.XR.Interaction.Toolkit.AR {  public class ARBaseGestureInteractable {} }

#else

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.XR.Interaction.Toolkit.AR
{
    /// <summary>
    /// Base class that manipulates an object via a gesture.
    /// </summary>
    public abstract class ARBaseGestureInteractable : XRBaseInteractable
    {
        bool m_IsManipulating;

        /// <summary>
        /// Determines if this interactable can be selected by a given interactor.
        /// </summary>
        /// <param name="interactor">Interactor to check for a valid selection with.</param>
        /// <returns>True if selection is valid this frame, False if not.</returns>
        public override bool IsSelectableBy(XRBaseInteractor interactor) { return false; }

        /// <summary>
        /// Returns true if the manipulation can be started for the given gesture.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        /// <returns>True if the manipulation can be started.</returns>
        protected virtual bool CanStartManipulationForGesture(DragGesture gesture)
        {
            return false;
        }

        /// <summary>
        /// Returns true if the manipulation can be started for the given gesture.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        /// <returns>True if the manipulation can be started.</returns>
        protected virtual bool CanStartManipulationForGesture(PinchGesture gesture)
        {
            return false;
        }

        /// <summary>
        /// Returns true if the manipulation can be started for the given gesture.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        /// <returns>True if the manipulation can be started.</returns>
        protected virtual bool CanStartManipulationForGesture(TapGesture gesture)
        {
            return false;
        }

        /// <summary>
        /// Returns true if the manipulation can be started for the given gesture.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        /// <returns>True if the manipulation can be started.</returns>
        protected virtual bool CanStartManipulationForGesture(TwistGesture gesture)
        {
            return false;
        }

        /// <summary>
        /// Returns true if the manipulation can be started for the given gesture.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        /// <returns>True if the manipulation can be started.</returns>
        protected virtual bool CanStartManipulationForGesture(TwoFingerDragGesture gesture)
        {
            return false;
        }

        /// <summary>
        /// Function called when the manipulation is started.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnStartManipulation(DragGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is started.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnStartManipulation(PinchGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is started.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnStartManipulation(TapGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is started.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnStartManipulation(TwistGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is started.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnStartManipulation(TwoFingerDragGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is continued.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnContinueManipulation(DragGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is continued.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnContinueManipulation(PinchGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is continued.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnContinueManipulation(TapGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is continued.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnContinueManipulation(TwistGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is continued.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnContinueManipulation(TwoFingerDragGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is ended.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnEndManipulation(DragGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is ended.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnEndManipulation(PinchGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is ended.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnEndManipulation(TapGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is ended.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnEndManipulation(TwistGesture gesture)
        {
            // Optional override.
        }

        /// <summary>
        /// Function called when the manipulation is ended.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected virtual void OnEndManipulation(TwoFingerDragGesture gesture)
        {
            // Optional override.
        }

        static ARGestureInteractor s_GestureInteractor;
        
        protected static ARGestureInteractor gestureInteractor
        {
            get { return s_GestureInteractor; }
        }

        static bool UpdateGestureInteractor()
        {
            if (s_GestureInteractor == null)
            {
                var gestureInteractors = FindObjectsOfType<ARGestureInteractor>();
                if (gestureInteractors.Length == 0)
                {
                    Debug.LogWarning("No gesture interactor in scene.");
                    return false;
                }
                else if (gestureInteractors.Length > 1)
                {
                    Debug.LogWarning("Multiple gesture interactors in scene.  Ensure there is only one");
                }

                s_GestureInteractor = gestureInteractors[0];
            }

            return true;
        }
        
        protected virtual bool IsGameObjectSelected()
        {
            if (!UpdateGestureInteractor())
                return false;

            var selectedInteractable = s_GestureInteractor.selectTarget;
            if (selectedInteractable == null)
                return false;

            return (selectedInteractable.gameObject == gameObject);
        }

        /// <summary>
        /// Determines if this interactable can be hovered by a given interactor.
        /// </summary>
        /// <param name="interactor">Interactor to check for a valid hover state with.</param>
        /// <returns>True if hovering is valid this frame, False if not.</returns>
        public override bool IsHoverableBy(XRBaseInteractor interactor)
        {
            return (interactor is ARGestureInteractor);
        }

        /// <summary>
        /// Connect an interactor's gestures to this interactable
        /// </summary>
        protected internal void ConnectGestureInteractor()
        {
            if (!UpdateGestureInteractor())
                return;
            
            if (s_GestureInteractor.DragGestureRecognizer != null)
                s_GestureInteractor.DragGestureRecognizer.onGestureStarted += OnGestureStarted;

            if (s_GestureInteractor.PinchGestureRecognizer != null)
                s_GestureInteractor.PinchGestureRecognizer.onGestureStarted += OnGestureStarted;

            if (s_GestureInteractor.TapGestureRecognizer != null)
                s_GestureInteractor.TapGestureRecognizer.onGestureStarted += OnGestureStarted;

            if (s_GestureInteractor.TwistGestureRecognizer != null)
                s_GestureInteractor.TwistGestureRecognizer.onGestureStarted += OnGestureStarted;

            if (s_GestureInteractor.TwoFingerDragGestureRecognizer != null)
                s_GestureInteractor.TwoFingerDragGestureRecognizer.onGestureStarted += OnGestureStarted;
        }

        /// <summary>
        /// Disconnect an interactor's gestures from this interactable
        /// </summary>
        protected internal void DisconnectGestureInteractor()
        {
            if (!UpdateGestureInteractor())
                return;
            
            if (s_GestureInteractor.DragGestureRecognizer != null)
                s_GestureInteractor.DragGestureRecognizer.onGestureStarted -= OnGestureStarted;

            if (s_GestureInteractor.PinchGestureRecognizer != null)
                s_GestureInteractor.PinchGestureRecognizer.onGestureStarted -= OnGestureStarted;

            if (s_GestureInteractor.TapGestureRecognizer != null)
                s_GestureInteractor.TapGestureRecognizer.onGestureStarted -= OnGestureStarted;

            if (s_GestureInteractor.TwistGestureRecognizer != null)
                s_GestureInteractor.TwistGestureRecognizer.onGestureStarted -= OnGestureStarted;

            if (s_GestureInteractor.TwoFingerDragGestureRecognizer != null)
                s_GestureInteractor.TwoFingerDragGestureRecognizer.onGestureStarted -= OnGestureStarted;
        }

        void OnGestureStarted(DragGesture gesture)
        {
            if (m_IsManipulating)
                return;

            if (CanStartManipulationForGesture(gesture))
            {
                m_IsManipulating = true;
                gesture.onUpdated += OnUpdated;
                gesture.onFinished += OnFinished;
                OnStartManipulation(gesture);
            }
        }

        void OnGestureStarted(PinchGesture gesture)
        {
            if (m_IsManipulating)
                return;

            if (CanStartManipulationForGesture(gesture))
            {
                m_IsManipulating = true;
                gesture.onUpdated += OnUpdated;
                gesture.onFinished += OnFinished;
                OnStartManipulation(gesture);
            }
        }

        void OnGestureStarted(TapGesture gesture)
        {
            if (m_IsManipulating)
                return;

            if (CanStartManipulationForGesture(gesture))
            {
                m_IsManipulating = true;
                gesture.onUpdated += OnUpdated;
                gesture.onFinished += OnFinished;
                OnStartManipulation(gesture);
            }
        }

        void OnGestureStarted(TwistGesture gesture)
        {
            if (m_IsManipulating)
                return;

            if (CanStartManipulationForGesture(gesture))
            {
                m_IsManipulating = true;
                gesture.onUpdated += OnUpdated;
                gesture.onFinished += OnFinished;
                OnStartManipulation(gesture);
            }
        }

        void OnGestureStarted(TwoFingerDragGesture gesture)
        {
            if (m_IsManipulating)
                return;

            if (CanStartManipulationForGesture(gesture))
            {
                m_IsManipulating = true;
                gesture.onUpdated += OnUpdated;
                gesture.onFinished += OnFinished;
                OnStartManipulation(gesture);
            }
        }

        void OnUpdated(DragGesture gesture)
        {
            if (!m_IsManipulating)
                return;

            // Can only transform selected Items.
            if (!IsGameObjectSelected())
            {
                m_IsManipulating = false;
                OnEndManipulation(gesture);
                return;
            }

            OnContinueManipulation(gesture);
        }

        void OnUpdated(PinchGesture gesture)
        {
            if (!m_IsManipulating)
                return;

            // Can only transform selected Items.
            if (!IsGameObjectSelected())
            {
                m_IsManipulating = false;
                OnEndManipulation(gesture);
                return;
            }

            OnContinueManipulation(gesture);
        }

        void OnUpdated(TapGesture gesture)
        {
            if (!m_IsManipulating)
                return;

            // Can only transform selected Items.
            if (!IsGameObjectSelected())
            {
                m_IsManipulating = false;
                OnEndManipulation(gesture);
                return;
            }

            OnContinueManipulation(gesture);
        }

        void OnUpdated(TwistGesture gesture)
        {
            if (!m_IsManipulating)
                return;

            // Can only transform selected Items.
            if (!IsGameObjectSelected())
            {
                m_IsManipulating = false;
                OnEndManipulation(gesture);
                return;
            }

            OnContinueManipulation(gesture);
        }

        void OnUpdated(TwoFingerDragGesture gesture)
        {
            if (!m_IsManipulating)
                return;

            // Can only transform selected Items.
            if (!IsGameObjectSelected())
            {
                m_IsManipulating = false;
                OnEndManipulation(gesture);
                return;
            }

            OnContinueManipulation(gesture);
        }

        void OnFinished(DragGesture gesture)
        {
            m_IsManipulating = false;
            OnEndManipulation(gesture);
        }

        void OnFinished(PinchGesture gesture)
        {
            m_IsManipulating = false;
            OnEndManipulation(gesture);
        }

        void OnFinished(TapGesture gesture)
        {
            m_IsManipulating = false;
            OnEndManipulation(gesture);
        }

        void OnFinished(TwistGesture gesture)
        {
            m_IsManipulating = false;
            OnEndManipulation(gesture);
        }

        void OnFinished(TwoFingerDragGesture gesture)
        {
            m_IsManipulating = false;
            OnEndManipulation(gesture);
        }
    }
}

#endif