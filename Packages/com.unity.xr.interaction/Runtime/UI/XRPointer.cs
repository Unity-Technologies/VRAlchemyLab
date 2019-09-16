using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UnityEngine.XR.Interaction.UI
{
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_XRPointer)]
    public class XRPointer : MonoBehaviour
    {
        const float k_DefaultHoverTimeToSelect = 0.5f;

        [SerializeField]
        [Tooltip("Controller used to detect press events (defaults to Controller component on Game Object).")]
        XRController m_Controller = null;

        [SerializeField]
        [Tooltip("If true, this pointer will simulate a click/press (depending on the type of UI element) " +
                "if hovered over an object for some amount of time. If a press is simulated (for a slider for example) " +
                "it will be released when the pointer is no longer hovering over the object.")]
        bool m_HoverToSelect = false;

        [SerializeField]
        [Tooltip("Number of seconds for which this pointer must hover over an object to select it.")]
        float m_HoverTimeToSelect = k_DefaultHoverTimeToSelect;

        GameObject m_CurrentHoverSelectTarget;
        float m_HoverInitialTime;

        /// <summary>
        /// This is the controller that drives the input state of this pointer.
        /// </summary>
        public XRController controller { get { return m_Controller; } set { m_Controller = value; } }

        XRUIInputModule inputModule { get; set; }

        /// <summary>
        /// If this pointer has a null input module reference, this method will try to find an XRInputModule in the scene 
        /// or create one if none exists.
        /// </summary>
        void FindOrCreateXRUIInputModule()
        {
            // We'd like to use this simpler version, but we can't be sure the enable order is properly setup.
            //EventSystem eventSystem = eventSystem.current;
            var eventSystem = Object.FindObjectOfType<EventSystem>();
            if (eventSystem == null)
                eventSystem = new GameObject("Event System", typeof(EventSystem)).GetComponent<EventSystem>();

            inputModule = eventSystem.GetComponent<XRUIInputModule>();
            if (inputModule == null)
                inputModule = eventSystem.gameObject.AddComponent<XRUIInputModule>();
        }

        void OnEnable()
        {
            FindOrCreateXRUIInputModule();

            if (controller == null)
                controller = gameObject.GetComponent<XRController>();

            m_CurrentHoverSelectTarget = null;

            inputModule.RegisterPointer(this);
        }

        void OnDisable()
        {
            inputModule.UnregisterPointer(this);

            inputModule = null;
        }

        public TrackedDeviceModel GetTrackedDeviceModel()
        {
            if (inputModule != null && enabled)
            {
                TrackedDeviceModel model;
                if (inputModule.GetTrackedDeviceModel(this, out model))
                    return model;
            }
            return new TrackedDeviceModel(-1);
        }

        public void UpdateModel(ref TrackedDeviceModel model)
        {
            if (m_Controller == null)
                return;

            model.position = m_Controller.transform.position;
            model.orientation = m_Controller.transform.rotation;
            model.select = m_Controller.uiPressInteractionState.active;

            if (m_HoverToSelect)
                UpdateHoverSelect(ref model);
        }

        void UpdateHoverSelect(ref TrackedDeviceModel model)
        {
            List<GameObject> hoverTargets = model.implementationData.hoverTargets;
            for (int i = hoverTargets.Count - 1; i >= 0; --i)
            {
                GameObject obj = hoverTargets[i];
                if (ExecuteEvents.CanHandleEvent<ISelectHandler>(obj) || ExecuteEvents.CanHandleEvent<IPointerClickHandler>(obj))
                {
                    if (m_CurrentHoverSelectTarget != obj)
                    {
                        m_CurrentHoverSelectTarget = obj;
                        m_HoverInitialTime = Time.time;
                    }

                    model.select = (Time.time - m_HoverInitialTime) > m_HoverTimeToSelect;
                    return;
                }
            }

            //Nothing targetted
            if (m_CurrentHoverSelectTarget != null)
                model.select = false;

            m_HoverInitialTime = Time.time;
            m_CurrentHoverSelectTarget = null;
        }
    }
}