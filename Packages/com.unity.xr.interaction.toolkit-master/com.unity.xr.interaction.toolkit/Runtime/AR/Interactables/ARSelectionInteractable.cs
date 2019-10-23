//-----------------------------------------------------------------------
// <copyright file="SelectionManipulator.cs" company="Google">
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
namespace UnityEngine.XR.Interaction.Toolkit.AR {  public class ARSelectionInteractable {} }

#else

using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit.AR
{
    /// <summary>
    /// Controls the selection of an object through Tap gesture.
    /// </summary>
    public class ARSelectionInteractable : ARBaseGestureInteractable
    {
        /// <summary>
        /// The visualization game object that will become active when the object is selected.
        /// </summary>
        public GameObject m_SelectionVisualization;
        
        bool m_GestureSelected;

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        void Update()
        {
        }
        
        /// <summary>
        /// Determines if this interactable can be selected by a given interactor.
        /// </summary>
        /// <param name="interactor">Interactor to check for a valid selection with.</param>
        /// <returns>True if selection is valid this frame, False if not.</returns>
        public override bool IsSelectableBy(XRBaseInteractor interactor)
        {
            if (!(interactor is ARGestureInteractor))
                return false;
            
            return m_GestureSelected;
        }

        /// <summary>
        /// Returns true if the manipulation can be started for the given gesture.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        /// <returns>True if the manipulation can be started.</returns>
        protected override bool CanStartManipulationForGesture(TapGesture gesture)
        {
            return true;
        }

        /// <summary>
        /// Function called when the manipulation is ended.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected override void OnEndManipulation(TapGesture gesture)
        {
            if (gesture.WasCancelled)
                return;
            if (gestureInteractor == null)
                return;

            if (gesture.TargetObject == gameObject)
            {
                // Toggle selection
                m_GestureSelected = !m_GestureSelected;
            }
            else
                m_GestureSelected = false;
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor first initiates selection of an interactable.</summary>
        /// <param name="interactor">Interactor that is initiating the selection.</param>
        protected internal override void OnSelectEnter(XRBaseInteractor interactor) 
        {
            base.OnSelectEnter(interactor);
            
            if (m_SelectionVisualization != null)
                m_SelectionVisualization.SetActive(true);
        }

        /// <summary>This method is called by the interaction manager 
        /// when the interactor ends selection of an interactable.</summary>
        /// <param name="interactor">Interactor that is ending the selection.</param>
        protected internal override void OnSelectExit(XRBaseInteractor interactor) 
        {
            base.OnSelectExit(interactor);
            
            if (m_SelectionVisualization != null)
                m_SelectionVisualization.SetActive(false);
        }
    }
}

#endif