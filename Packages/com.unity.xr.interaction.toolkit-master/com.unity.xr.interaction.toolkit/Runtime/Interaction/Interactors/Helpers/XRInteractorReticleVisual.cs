using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Interactor helper object draws a targeting reticlePrefab over a raycasted point in front of the Interactor.
    /// </summary>
    [AddComponentMenu("XR/Helpers/XR Interactor Reticle Visual")]
    [DisallowMultipleComponent]
    public class XRInteractorReticleVisual : MonoBehaviour
    {
        const int   k_MaxRaycastHits                = 10;
        const float k_DefaultMaxRaycastDistance     = 10;
        const float k_DefaultPrefabScalingFactor    = 1.0f;
        const float k_DefaultEndpointSmoothingTime  = 0.02f;
        const int   k_DefaultLayerMask              = -1;

        [SerializeField, Tooltip("The max distance to RayCast from this Interactor.")]
        float m_MaxRaycastDistance = k_DefaultMaxRaycastDistance;
        /// <summary>Gets or sets the max distance to RayCast from this Interactor.</summary>
        public float maxRaycastDistance { get { return m_MaxRaycastDistance; } set { m_MaxRaycastDistance = value; } }

        [SerializeField, Tooltip("Prefab to draw over Raycast destination.")]
        GameObject m_ReticlePrefab;
        /// <summary>Gets or sets prefab to draw over Raycast destination.</summary>
        public GameObject reticlePrefab { get { return m_ReticlePrefab; } set { m_ReticlePrefab = value; SetupReticlePrefab(); } }

        [SerializeField, Tooltip("Amount to scale prefab (before applying distance scaling).")]
        float m_PrefabScalingFactor = k_DefaultPrefabScalingFactor;
        /// <summary>Gets or sets amount to scale prefab (before applying distance scaling).</summary>
        public float prefabScalingFactor { get { return m_PrefabScalingFactor; } set { m_PrefabScalingFactor = value; } }

        [SerializeField, Tooltip("Whether to undo the apparent scale of the prefab by distance.")]
        bool m_UndoDistanceScaling = true;
        /// <summary>Gets or sets whether to undo the apparent scale of the prefab by distance.</summary>
        public bool undoDistanceScaling { get { return m_UndoDistanceScaling; } set { m_UndoDistanceScaling = value; } }

        [SerializeField, Tooltip("Whether to align the prefab to the raycasted surface normal.")]
        bool m_AlignPrefabWithSurfaceNormal = true;
        /// <summary>Gets or sets whether to align the prefab to the raycasted surface normal.</summary>
        public bool alignPrefabWithSurfaceNormal { get { return m_AlignPrefabWithSurfaceNormal; } set { m_AlignPrefabWithSurfaceNormal = value; } }

        [SerializeField, Tooltip("Smoothing time for endpoint.")]
        float m_EndpointSmoothingTime = k_DefaultEndpointSmoothingTime;
        /// <summary>Gets or sets smoothing time for endpoint.</summary>
        public float endpointSmoothingTime { get { return m_EndpointSmoothingTime; } set { m_EndpointSmoothingTime = value; } }

        [SerializeField, Tooltip("Draw the reticlePrefab while selecting an Interactable.")]
        bool m_DrawWhileSelecting = false;
        /// <summary>Gets or sets draw the reticlePrefab while selecting an Interactable.</summary>
        public bool drawWhileSelecting { get { return m_DrawWhileSelecting; } set { m_DrawWhileSelecting = value; } }

        [SerializeField, Tooltip("Layer mask for raycast.")]
        LayerMask m_RaycastMask = k_DefaultLayerMask;
        /// <summary>Gets or sets layer mask for raycast.</summary>
        public LayerMask raycastMask { get { return m_RaycastMask; } set { m_RaycastMask = value; } }

        GameObject m_ReticleInstance;
        XRBaseInteractor m_Interactor;
        Vector3 m_TargetEndPoint;
        Vector3 m_TargetEndNormal;

        // reusable array of raycast hits
        RaycastHit[] m_RaycastHits = new RaycastHit[k_MaxRaycastHits];
        RaycastHitComparer m_RaycastHitComparer = new RaycastHitComparer();

        XRUIPointer m_XRUIPointer;

        bool m_ReticleActive = false;
        /// <summary>Gets or sets whether the reticlePrefab is currently active.</summary>
        public bool reticleActive
        {
            get { return m_ReticleActive; }
            set { m_ReticleActive = value; if (m_ReticleInstance) m_ReticleInstance.SetActive(value); }
        }

        bool m_HasSelectedInteractable = false;
        

        void Awake()
        {
            m_Interactor = GetComponent<XRBaseInteractor>();
            if (m_Interactor)
            {
                m_Interactor.onSelectEnter.AddListener(OnSelectEnter);
                m_Interactor.onSelectExit.AddListener(OnSelectExit); 
            }
            SetupReticlePrefab();
            m_XRUIPointer = GetComponent<XRUIPointer>();
            reticleActive = false;
        }

        private void OnDestroy()
        {
            if (m_Interactor)
            {
                m_Interactor.onSelectEnter.RemoveListener(OnSelectEnter);
                m_Interactor.onSelectExit.RemoveListener(OnSelectExit);
            }
        }

        void SetupReticlePrefab()
        {
            if (m_ReticleInstance)
            {
                Destroy(m_ReticleInstance);
            }
            if (m_ReticlePrefab)
                m_ReticleInstance = GameObject.Instantiate(m_ReticlePrefab);
        }

        void Update()
        {
            if (m_Interactor != null)
            {
                if (UpdateReticleTarget())
                    ActivateReticleAtTarget();
                else
                    reticleActive = false;
            }
            else
                reticleActive = false;
        }

        bool TryGetRaycastPoint(ref Vector3 raycastPos, ref Vector3 raycastNormal)
        {
            bool raycastHit = false;

            // Raycast against physics
            int hitCount = Physics.RaycastNonAlloc(m_Interactor.attachTransform.position, m_Interactor.attachTransform.forward,
                m_RaycastHits, m_MaxRaycastDistance, m_RaycastMask);
            if (hitCount != 0)
            {
                Array.Sort(m_RaycastHits, 0, hitCount, m_RaycastHitComparer);
                raycastPos = m_RaycastHits.First().point;
                raycastNormal = m_RaycastHits.First().normal;
                raycastHit = true;
            }

            // Get last Raycast against EventSystem system with attached XRPointer (if available)
            if (m_XRUIPointer && m_XRUIPointer.enabled)
            {
                TrackedDeviceModel model = m_XRUIPointer.GetTrackedDeviceModel();
                RaycastResult lastRaycastResult = model.implementationData.lastFrameRaycast;
                if(lastRaycastResult.isValid)
                {
                    if ((lastRaycastResult.worldPosition - m_Interactor.transform.position).sqrMagnitude <
                    (raycastPos - m_Interactor.transform.position).sqrMagnitude)
                    {
                        raycastPos = lastRaycastResult.worldPosition;
                        raycastNormal = lastRaycastResult.worldNormal;
                    }
                    raycastHit = true;
                }              
            }

            return raycastHit;
        }

        bool UpdateReticleTarget()
        {
            if (m_HasSelectedInteractable && !m_DrawWhileSelecting)
                return false;

            Vector3 raycastPos = new Vector3(), raycastNormal = new Vector3();
            if (TryGetRaycastPoint(ref raycastPos, ref raycastNormal))
            {
                // Smooth target
                Vector3 velocity = new Vector3();
                m_TargetEndPoint = Vector3.SmoothDamp(m_TargetEndPoint, raycastPos, ref velocity, m_EndpointSmoothingTime);
                m_TargetEndNormal = Vector3.SmoothDamp(m_TargetEndNormal, raycastNormal, ref velocity, m_EndpointSmoothingTime);
                return true;
            }
            return false;
        }

        void ActivateReticleAtTarget()
        {
            if (m_ReticleInstance)
            {
                m_ReticleInstance.transform.position = m_TargetEndPoint;
                if (m_AlignPrefabWithSurfaceNormal)
                    m_ReticleInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, m_TargetEndNormal);
                else
                    m_ReticleInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, (m_Interactor.attachTransform.position - m_TargetEndPoint).normalized);
                float scaleFactor = m_PrefabScalingFactor;
                if (m_UndoDistanceScaling)
                    scaleFactor *= Vector3.Distance(m_Interactor.attachTransform.position, m_TargetEndPoint);
                m_ReticleInstance.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                reticleActive = true;
            }
        }

        void OnSelectEnter(XRBaseInteractable interactable)
        {
            m_HasSelectedInteractable = true;
            reticleActive = false;
        }

        void OnSelectExit(XRBaseInteractable interactable)
        {
            m_HasSelectedInteractable = false;
        }
    }
}
