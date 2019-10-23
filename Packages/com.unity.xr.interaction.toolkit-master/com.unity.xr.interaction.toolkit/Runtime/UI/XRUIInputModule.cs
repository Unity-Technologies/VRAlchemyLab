using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.XR.Interaction.Toolkit.UI
{
    public class XRUIInputModule : UIInputModule
    {
        const float k_DefaultMaxTrackedDeviceRaycastDistance = 1000.0f;

        struct RegisteredPointer
        {
            public RegisteredPointer(XRUIPointer pointer, int deviceIndex)
            {
                this.pointer = pointer;
                model = new TrackedDeviceModel(deviceIndex);
            }

            public XRUIPointer pointer;
            public TrackedDeviceModel model;
        }

        struct RegisteredTouch
        {
            public RegisteredTouch(Touch touch, int deviceIndex)
            {
                touchId = touch.fingerId;
                model = new TouchModel(deviceIndex);
                isValid = true;
            }

            public bool isValid;
            public int touchId;
            public TouchModel model;

        }

        [SerializeField]
        [Tooltip("The maximum distance to raycast with tracked devices to find hit objects.")]
        float m_MaxTrackedDeviceRaycastDistance = k_DefaultMaxTrackedDeviceRaycastDistance;
        public float maxRaycastDistance { get { return m_MaxTrackedDeviceRaycastDistance;} set { m_MaxTrackedDeviceRaycastDistance = value;} }

        [SerializeField]
        [Tooltip("If true, will forward 3D tracked device data to UI elements")]
        bool m_EnableXRInput = true;
        [SerializeField]
        [Tooltip("If true, will forward 2D mouse data to UI elements")]
        bool m_EnableMouseInput = true;
        [SerializeField]
        [Tooltip("If true, will forward 2D touch data to UI elements.")]
        bool m_EnableTouchInput = true;

        MouseModel m_Mouse;
        List<RegisteredTouch> m_RegisteredTouches;
        int m_RollingPointerIndex;
        List<RegisteredPointer> m_RegisteredPointers;

        void EnsureInitialized()
        {
            if (m_RegisteredPointers == null)
                m_RegisteredPointers = new List<RegisteredPointer>();

            if (m_RegisteredTouches == null)
                m_RegisteredTouches = new List<RegisteredTouch>();
        }

        protected override void Awake()
        {
            base.Awake();
            EnsureInitialized();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Mouse = new MouseModel(m_RollingPointerIndex++);
        }

        public void RegisterPointer(XRUIPointer pointer)
        {
            EnsureInitialized();

            for (int i = 0; i < m_RegisteredPointers.Count; i++)
            {
                if (m_RegisteredPointers[i].pointer == pointer)
                    return;
            }

            m_RegisteredPointers.Add(new RegisteredPointer(pointer, m_RollingPointerIndex++));
        }

        public void UnregisterPointer(XRUIPointer pointer)
        {
            EnsureInitialized();

            for (int i = 0; i < m_RegisteredPointers.Count; i++)
            {
                if (m_RegisteredPointers[i].pointer == pointer)
                {
                    RegisteredPointer registeredPointer = m_RegisteredPointers[i];
                    registeredPointer.pointer = null;
                    m_RegisteredPointers[i] = registeredPointer;
                    return;
                }
            }
        }

        public bool GetTrackedDeviceModel(XRUIPointer pointer, out TrackedDeviceModel model)
        {
            EnsureInitialized();

            for (int i = 0; i < m_RegisteredPointers.Count; i++)
            {
                if (m_RegisteredPointers[i].pointer == pointer)
                {
                    model = m_RegisteredPointers[i].model;
                    return true;
                }
            }

            model = new TrackedDeviceModel(-1);
            return false;
        }

        protected override void DoProcess()
        {
            EnsureInitialized();

            if (m_EnableXRInput)
            {
                for (int i = 0; i < m_RegisteredPointers.Count; i++)
                {
                    RegisteredPointer registeredPointer = m_RegisteredPointers[i];

                    //Update the raycast distance in case it's changed between frames
                    registeredPointer.model.maxRaycastDistance = m_MaxTrackedDeviceRaycastDistance;

                    //If device is removed, we send a default state to unclick any UI
                    if (registeredPointer.pointer == null)
                    {
                        registeredPointer.model.Reset();
                        ProcessTrackedDevice(ref registeredPointer.model);
                        m_RegisteredPointers.RemoveAt(i--);
                    }
                    else
                    {
                        registeredPointer.pointer.UpdateModel(ref registeredPointer.model);
                        ProcessTrackedDevice(ref registeredPointer.model);
                        m_RegisteredPointers[i] = registeredPointer;
                    }
                }
            }

            if(m_EnableMouseInput)
                ProcessMouse();

            if (m_EnableTouchInput)
                ProcessTouches();
        }

        void ProcessMouse()
        {
            if(Input.mousePresent)
            {
                m_Mouse.position = Input.mousePosition;
                m_Mouse.scrollPosition = Input.mouseScrollDelta;
                m_Mouse.leftButtonPressed = Input.GetMouseButton(0);
                m_Mouse.rightButtonPressed = Input.GetMouseButton(1);
                m_Mouse.middleButtonPressed = Input.GetMouseButton(2);

                ProcessMouse(ref m_Mouse);
            }
        }

        void ProcessTouches()
        {
            Touch[] touches = Input.touches;
            for (int i = 0; i < touches.Length; i++)
            {
                Touch touch = touches[i];
                int registeredTouchIndex = -1;

                //Find if touch already exists
                for (int j = 0; j < m_RegisteredTouches.Count; j++)
                {
                    if (touch.fingerId == m_RegisteredTouches[j].touchId)
                    {
                        registeredTouchIndex = j;
                        break;
                    }
                }

                if (registeredTouchIndex < 0)
                {
                    //Not found, search empty pool
                    for (int j = 0; j < m_RegisteredTouches.Count; j++)
                    {
                        if (!m_RegisteredTouches[j].isValid)
                        {
                            //Re-use the Id
                            int pointerId = m_RegisteredTouches[j].model.pointerId;
                            m_RegisteredTouches[j] = new RegisteredTouch(touch, pointerId);
                            registeredTouchIndex = j;
                            break;
                        }
                    }

                    if (registeredTouchIndex < 0)
                    {
                        //No Empty slots, add one
                        registeredTouchIndex = m_RegisteredTouches.Count;
                        m_RegisteredTouches.Add(new RegisteredTouch(touch, m_RollingPointerIndex++));
                    }
                }

                RegisteredTouch registeredTouch = m_RegisteredTouches[registeredTouchIndex];
                registeredTouch.model.selectPhase = touch.phase;
                registeredTouch.model.position = touch.position;
                m_RegisteredTouches[registeredTouchIndex] = registeredTouch;
            }

            for (int i = 0; i < m_RegisteredTouches.Count; i++)
            {
                RegisteredTouch registeredTouch = m_RegisteredTouches[i];
                ProcessTouch(ref registeredTouch.model);
                if (registeredTouch.model.selectPhase == TouchPhase.Ended || registeredTouch.model.selectPhase == TouchPhase.Canceled)
                    registeredTouch.isValid = false;
                m_RegisteredTouches[i] = registeredTouch;
            }
        }
    }
}
