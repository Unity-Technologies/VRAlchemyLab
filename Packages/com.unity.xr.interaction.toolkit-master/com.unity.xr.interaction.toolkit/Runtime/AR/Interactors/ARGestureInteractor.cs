//-----------------------------------------------------------------------
// <copyright file="ARGestureInteractor.cs" company="Google">
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
namespace UnityEngine.XR.Interaction.Toolkit.AR {  public class ARGestureInteractor {} }

#else

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.XR.Interaction.Toolkit.AR
{
    /// <summary>
    /// The ARGestureInteractor allows the user to manipulate virtual objects (select, translate,
    /// rotate, scale and elevate) through gestures (tap, drag, twist, swipe).
    /// The ARGestureInteractor also handles the current selected object and its visualization.
    ///
    /// To enable it add one ARGestureInteractor to your scene and one ARGestureInteractabl as parent of each
    /// of your virtual objects.
    /// </summary>
    public class ARGestureInteractor : XRBaseInteractor
    {
        private static ARGestureInteractor s_Instance = null;

        private DragGestureRecognizer m_DragGestureRecognizer = new DragGestureRecognizer();
        private PinchGestureRecognizer m_PinchGestureRecognizer = new PinchGestureRecognizer();
        private TwoFingerDragGestureRecognizer m_TwoFingerDragGestureRecognizer = new TwoFingerDragGestureRecognizer();
        private TapGestureRecognizer m_TapGestureRecognizer = new TapGestureRecognizer();
        private TwistGestureRecognizer m_TwistGestureRecognizer = new TwistGestureRecognizer();

        /// <summary>
        /// Gets the ARGestureInteractor instance.
        /// </summary>
        public static ARGestureInteractor Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    var xrGestureInteractors = FindObjectsOfType<ARGestureInteractor>();
                    if (xrGestureInteractors.Length > 0)
                    {
                        s_Instance = xrGestureInteractors[0];
                    }
                    else
                    {
                        Debug.LogError("No instance of ARGestureInteractor exists in the scene.");
                    }
                }

                return s_Instance;
            }
        }

        /// <summary>
        /// Gets the Drag gesture recognizer.
        /// </summary>
        public DragGestureRecognizer DragGestureRecognizer
        {
            get
            {
                return m_DragGestureRecognizer;
            }
        }

        /// <summary>
        /// Gets the Pinch gesture recognizer.
        /// </summary>
        public PinchGestureRecognizer PinchGestureRecognizer
        {
            get
            {
                return m_PinchGestureRecognizer;
            }
        }

        /// <summary>
        /// Gets the two finger drag gesture recognizer.
        /// </summary>
        public TwoFingerDragGestureRecognizer TwoFingerDragGestureRecognizer
        {
            get
            {
                return m_TwoFingerDragGestureRecognizer;
            }
        }

        /// <summary>
        /// Gets the Tap gesture recognizer.
        /// </summary>
        public TapGestureRecognizer TapGestureRecognizer
        {
            get
            {
                return m_TapGestureRecognizer;
            }
        }

        /// <summary>
        /// Gets the Twist gesture recognizer.
        /// </summary>
        public TwistGestureRecognizer TwistGestureRecognizer
        {
            get
            {
                return m_TwistGestureRecognizer;
            }
        }
        
        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (Instance != this)
            {
                // TODO: We should support multiple ARGestureInteractors eventually
                Debug.LogWarning("Multiple instances of ARGestureInteractor detected in the scene." +
                                 " Only one instance can exist at a time. The duplicate instances" +
                                 " will be destroyed.");
                DestroyImmediate(gameObject);
                return;
            }
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            DragGestureRecognizer.Update();
            PinchGestureRecognizer.Update();
            TwoFingerDragGestureRecognizer.Update();
            TapGestureRecognizer.Update();
            TwistGestureRecognizer.Update();
        }

        float GetHorizontalFOV(Camera camera)
        {
            float vFOV = camera.fieldOfView * Mathf.Deg2Rad;
            float cameraHeight = Mathf.Tan(vFOV * .5f);
            return Mathf.Atan(cameraHeight * camera.aspect);
        }

        /// <summary>
        /// Retrieve the list of interactables that this interactor could possibly interact with this frame.
        /// </summary>
        /// <param name="validTargets">Populated List of interactables that are valid for selection or hover.</param>
        public override void GetValidTargets(List<XRBaseInteractable> validTargets)
        {
            validTargets.Clear();
            
            float hFOV = GetHorizontalFOV(Camera.main);

            foreach (var interactable in interactionManager.interactables)
            {
                // We can always interact with placement interactables.
                if (interactable is ARPlacementInteractable)
                    validTargets.Add(interactable);
                else if (interactable is ARBaseGestureInteractable)
                {
                    // Check if angle off of camera's forward axis is less than hFOV (more or less in camera frustrum).
                    // Note: this does not take size of object into consideration.
                    // Note: this will fall down when directly over/under object (we should also check for dot
                    // product with up/down.
                    Vector3 toTarget =
                        Vector3.Normalize(interactable.transform.position - Camera.main.transform.position);
                    float dotForwardToTarget = Vector3.Dot(Camera.main.transform.forward, toTarget);
                    if (Mathf.Acos(dotForwardToTarget) < hFOV)
                        validTargets.Add(interactable);
                }
            }
        }
    }
}
#endif