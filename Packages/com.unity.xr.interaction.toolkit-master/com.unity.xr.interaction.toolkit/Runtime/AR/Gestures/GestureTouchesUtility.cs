//-----------------------------------------------------------------------
// <copyright file="GestureTouchesUtility.cs" company="Google">
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
using System.Reflection;

namespace UnityEngine.XR.Interaction.Toolkit.AR
{
    class MockTouch
    {
        static Dictionary<string, FieldInfo> fields;
        object m_Touch;

        public float deltaTime { get { return ((Touch)m_Touch).deltaTime; } set { fields["m_TimeDelta"].SetValue(m_Touch, value); } }
        public int tapCount { get { return ((Touch)m_Touch).tapCount; } set { fields["m_TapCount"].SetValue(m_Touch, value); } }
        public TouchPhase phase { get { return ((Touch)m_Touch).phase; } set { fields["m_Phase"].SetValue(m_Touch, value); } }
        public Vector2 deltaPosition { get { return ((Touch)m_Touch).deltaPosition; } set { fields["m_PositionDelta"].SetValue(m_Touch, value); } }
        public int fingerId { get { return ((Touch)m_Touch).fingerId; } set { fields["m_FingerId"].SetValue(m_Touch, value); } }
        public Vector2 position { get { return ((Touch)m_Touch).position; } set { fields["m_Position"].SetValue(m_Touch, value); } }
        public Vector2 rawPosition { get { return ((Touch)m_Touch).rawPosition; } set { fields["m_RawPosition"].SetValue(m_Touch, value); } }

        public Touch Touch { get { return (Touch)m_Touch; } }

        public MockTouch()
        {
            m_Touch = new Touch();
        }

        static MockTouch()
        {
            fields = new Dictionary<string, FieldInfo>();
            foreach (var f in typeof(Touch).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                fields.Add(f.Name, f);
        }
    }
    
    
    /// <summary>
    /// Singleton used by Gesture's and GestureRecognizer's to interact with touch input.
    ///
    /// 1. Makes it easy to find touches by fingerId.
    /// 2. Allows Gestures to Lock/Release fingerIds.
    /// 3. Wraps Input.Touches so that it works both in editor and on device.
    /// 4. Provides helper functions for converting touch coordinates
    ///    and performing raycasts based on touches.
    /// </summary>
    static class GestureTouchesUtility
    {
        const float k_EdgeThresholdInches = 0.1f;
        
        static List<Touch> s_Touches = new List<Touch>();
        internal static List<MockTouch> s_MockTouches = new List<MockTouch>();
        static HashSet<int> s_RetainedFingerIds = new HashSet<int>();

        /// <summary>
        /// Try to find a touch for a particular finger id.
        /// </summary>
        /// <param name="fingerId">The finger id to find the touch.</param>
        /// <param name="touch">The output touch.</param>
        /// <returns>True if a touch was found.</returns>
        public static bool TryFindTouch(int fingerId, out Touch touch)
        {
            for (int i = 0; i < Touches.Length; i++)
            {
                if (Touches[i].fingerId == fingerId)
                {
                    touch = Touches[i];
                    return true;
                }
            }

            touch = new Touch();
            return false;
        }

        /// <summary>
        /// Converts Pixels to Inches.
        /// </summary>
        /// <param name="pixels">The amount to convert in pixels.</param>
        /// <returns>The converted amount in inches.</returns>
        public static float PixelsToInches(float pixels)
        {
            return pixels / Screen.dpi;
        }

        /// <summary>
        /// Converts Inches to Pixels.
        /// </summary>
        /// <param name="inches">The amount to convert in inches.</param>
        /// <returns>The converted amount in pixels.</returns>
        public static float InchesToPixels(float inches)
        {
            return inches * Screen.dpi;
        }

        /// <summary>
        /// Used to determine if a touch is off the edge of the screen based on some slop.
        /// Useful to prevent accidental touches from simply holding the device from causing
        /// confusing behavior.
        /// </summary>
        /// <param name="touch">The touch to check.</param>
        /// <returns>True if the touch is off screen edge.</returns>
        public static bool IsTouchOffScreenEdge(Touch touch)
        {
            float slopPixels = InchesToPixels(k_EdgeThresholdInches);

            bool result = touch.position.x <= slopPixels;
            result |= touch.position.y <= slopPixels;
            result |= touch.position.x >= Screen.width - slopPixels;
            result |= touch.position.y >= Screen.height - slopPixels;

            return result;
        }

        /// <summary>
        /// Performs a Raycast from the camera.
        /// </summary>
        /// <param name="screenPos">The screen position to perform the raycast from.</param>
        /// <param name="result">The RaycastHit result.</param>
        /// <returns>True if an object was hit.</returns>
        public static bool RaycastFromCamera(Vector2 screenPos, out RaycastHit result)
        {
            if (Camera.main == null)
            {
                result = new RaycastHit();
                return false;
            }

            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                result = hit;
                return true;
            }

            result = hit;
            return false;
        }

        /// <summary>
        /// Locks a finger Id.
        /// </summary>
        /// <param name="fingerId">The finger id to lock.</param>
        public static void LockFingerId(int fingerId)
        {
            if (!IsFingerIdRetained(fingerId))
            {
                s_RetainedFingerIds.Add(fingerId);
            }
        }

        /// <summary>
        /// Releases a finger Id.
        /// </summary>
        /// <param name="fingerId">The finger id to release.</param>
        public static void ReleaseFingerId(int fingerId)
        {
            if (IsFingerIdRetained(fingerId))
            {
                s_RetainedFingerIds.Remove(fingerId);
            }
        }

        /// <summary>
        /// Returns true if the finger Id is retained.
        /// </summary>
        /// <param name="fingerId">The finger id to check.</param>
        /// <returns>True if the finger is retained.</returns>
        public static bool IsFingerIdRetained(int fingerId)
        {
            return s_RetainedFingerIds.Contains(fingerId);
        }

        public static Touch[] Touches
        {
            get
            {
                s_Touches.Clear();
                s_Touches.AddRange(Input.touches);
        
                foreach (var mockTouch in s_MockTouches)
                    s_Touches.Add(mockTouch.Touch);

                return s_Touches.ToArray();
            }
        }
    }
}

#endif