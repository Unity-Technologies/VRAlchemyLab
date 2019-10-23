using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.XR.Interaction.Toolkit.UI
{
    public struct TrackedDeviceModel
    {
        const float k_DefaultMaxRaycastDistance = 1000;

        public struct ImplementationData
        {
            /// <summary>
            /// This tracks the current GUI targets being hovered over.  Syncs up to <see cref="PointerEventData.hovered"/>.
            /// </summary>
            public List<GameObject> hoverTargets { get; set; }

            /// <summary>
            ///  Tracks the current enter/exit target being hovered over at any given moment. Syncs up to <see cref="PointerEventData.pointerEnter"/>.
            /// </summary>
            public GameObject pointerTarget { get; set; }

            /// <summary>
            /// Used to cache whether or not the current mouse button is being dragged.  See <see cref="PointerEventData.dragging"/> for more details.
            /// </summary>
            public bool isDragging { get; set; }

            /// <summary>
            /// Used to cache the last time this button was pressed.  See <see cref="PointerEventData.clickTime"/> for more details.
            /// </summary>
            public float pressedTime { get; set; }

            /// <summary>
            /// The position on the screen that this button was last pressed.  In the same scale as <see cref="MouseModel.position"/>, and caches the same value as <see cref="PointerEventData.pressPosition"/>.
            /// </summary>
            public Vector2 pressedPosition { get; set; }

            /// <summary>
            /// The Raycast data from the time it was pressed.  See <see cref="PointerEventData.pointerPressRaycast"/> for more details.
            /// </summary>
            public RaycastResult pressedRaycast { get; set; }
            
            /// <summary>
            /// The last raycast done for this model.
            /// </summary>
            public RaycastResult lastFrameRaycast { get; set; }

            /// <summary>
            /// The last gameobject pressed on that can handle press or click events.  See <see cref="PointerEventData.pointerPress"/> for more details.
            /// </summary>
            public GameObject pressedGameObject { get; set; }

            /// <summary>
            /// The last gameobject pressed on regardless of whether it can handle events or not.  See <see cref="PointerEventData.rawPointerPress"/> for more details.
            /// </summary>
            public GameObject pressedGameObjectRaw { get; set; }

            /// <summary>
            /// The gameobject currently being dragged if any.  See <see cref="PointerEventData.pointerDrag"/> for more details.
            /// </summary>
            public GameObject draggedGameObject { get; set; }

            /// <summary>
            /// Resets this object to it's default, unused state.
            /// </summary>
            public void Reset()
            {
                isDragging = false;
                pressedTime = 0.0f;
                pressedPosition = Vector2.zero;
                pressedRaycast = new RaycastResult();
                pressedGameObject = pressedGameObjectRaw = draggedGameObject = null;

                if (hoverTargets == null)
                {
                    hoverTargets = new List<GameObject>();
                }
                else
                {
                    hoverTargets.Clear();
                }
            }
        }

        internal ImplementationData implementationData { get { return m_ImplementationData; } }

        public int pointerId { get; private set; }
        
        public float maxRaycastDistance { get; set; }

        public bool select
        {
            get
            {
                return m_SelectDown;
            }
            set
            {
                if (m_SelectDown != value)
                {
                    m_SelectDown = value;
                    selectDelta |= value ? ButtonDeltaState.Pressed : ButtonDeltaState.Released;
                    changedThisFrame = true;
                }
            }
        }
        public ButtonDeltaState selectDelta { get; private set; }

        public bool changedThisFrame { get; private set; }

        public Vector3 position
        {
            get
            {
                return m_Position;
            }
            set
            {
                if (m_Position != value)
                {
                    m_Position = value;
                    changedThisFrame = true;
                }
            }
        }

        public Quaternion orientation
        {
            get
            {
                return m_Orientation;
            }
            set
            {
                if (m_Orientation != value)
                {
                    m_Orientation = value;
                    changedThisFrame = true;
                }
            }
        }

        public TrackedDeviceModel(int pointerId)
        {
            this.pointerId = pointerId;
            maxRaycastDistance = k_DefaultMaxRaycastDistance;

            m_Orientation = Quaternion.identity;
            m_Position = Vector3.zero;
            m_SelectDown = changedThisFrame = false;
            selectDelta = ButtonDeltaState.NoChange;

            m_ImplementationData = new ImplementationData();
            m_ImplementationData.Reset();
        }

        public void Reset()
        {
            m_Orientation = Quaternion.identity;
            m_Position = Vector3.zero;
            m_SelectDown = changedThisFrame = false;
            selectDelta = ButtonDeltaState.NoChange;
            m_ImplementationData.Reset();
        }

        public void OnFrameFinished()
        {
            selectDelta = ButtonDeltaState.NoChange;
            changedThisFrame = false;
        }

        public void CopyTo(TrackedDeviceEventData eventData)
        {
            eventData.ray = new Ray(m_Position, m_Orientation * Vector3.forward);
            eventData.maxDistance = maxRaycastDistance;
            // Demolish the position so we don't trigger any checks from the Graphics Raycaster.
            eventData.position = new Vector2(float.MinValue, float.MinValue);

            eventData.pointerEnter = m_ImplementationData.pointerTarget;
            eventData.dragging = m_ImplementationData.isDragging;
            eventData.clickTime = m_ImplementationData.pressedTime;
            eventData.pressPosition = m_ImplementationData.pressedPosition;
            eventData.pointerPressRaycast = m_ImplementationData.pressedRaycast;
            eventData.pointerPress = m_ImplementationData.pressedGameObject;
            eventData.rawPointerPress = m_ImplementationData.pressedGameObjectRaw;
            eventData.pointerDrag = m_ImplementationData.draggedGameObject;

            eventData.hovered.Clear();
            eventData.hovered.AddRange(m_ImplementationData.hoverTargets);
        }

        public void CopyFrom(TrackedDeviceEventData eventData)
        {
            m_ImplementationData.pointerTarget = eventData.pointerEnter;
            m_ImplementationData.isDragging = eventData.dragging;
            m_ImplementationData.pressedTime = eventData.clickTime;
            m_ImplementationData.pressedPosition = eventData.pressPosition;
            m_ImplementationData.pressedRaycast = eventData.pointerPressRaycast;
            m_ImplementationData.pressedGameObject = eventData.pointerPress;
            m_ImplementationData.pressedGameObjectRaw = eventData.rawPointerPress;
            m_ImplementationData.draggedGameObject = eventData.pointerDrag;
            m_ImplementationData.lastFrameRaycast = eventData.pointerCurrentRaycast;

            m_ImplementationData.hoverTargets.Clear();
            m_ImplementationData.hoverTargets.AddRange(eventData.hovered);
        }

        private bool m_SelectDown;
        private Vector3 m_Position;
        private Quaternion m_Orientation;

        private ImplementationData m_ImplementationData;
    }
}
