using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UnityEngine.XR.Interaction.Toolkit.UI
{
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_XRUIPointer)]
    public class XRUIPointer : MonoBehaviour, ILineRenderable
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

        public bool GetLinePoints(ref Vector3[] linePoints, ref int noPoints)
        {
            if (linePoints == null)
            {
                linePoints = new Vector3[2];
            }
            if (linePoints.Length < 2)
            {
                linePoints = new Vector3[2];
            }
            linePoints[0] = transform.position;
            linePoints[1] = transform.position + (transform.forward * inputModule.maxRaycastDistance);
            noPoints = 2;
            return true;
        }

        public bool TryGetHitInfo(ref Vector3 position, ref Vector3 normal, ref int positionInLine, ref bool isValidTarget)
        {
            TrackedDeviceModel model = GetTrackedDeviceModel();
            RaycastResult lastRaycastResult = model.implementationData.lastFrameRaycast;
            if (lastRaycastResult.isValid)
            {
                position = lastRaycastResult.worldPosition;
                normal = lastRaycastResult.worldNormal;
                positionInLine = 1;
                isValidTarget = true;
                return true;
            }
            return false;
        }


        /// <summary>
        /// If this pointer has a null input module reference, this method will try to find an XRInputModule in the scene 
        /// or create one if none exists.
        /// </summary>
        void FindOrCreateXRUIInputModule()
        {
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

            foreach (Canvas canvas in GameObject.FindObjectsOfType<Canvas>())
                if (canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null)
                    canvas.worldCamera = Camera.main;
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

            //Nothing targeted
            if (m_CurrentHoverSelectTarget != null)
                model.select = false;

            m_HoverInitialTime = Time.time;
            m_CurrentHoverSelectTarget = null;
        }
    }
}