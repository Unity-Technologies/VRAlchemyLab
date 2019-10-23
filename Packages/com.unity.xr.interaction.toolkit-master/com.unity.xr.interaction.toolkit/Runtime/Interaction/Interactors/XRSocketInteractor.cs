using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Interactor used for holding interactables via a socket.  This component is not designed to be attached to a controller
    /// (thus does not derive from XRBaseControllerInteractor) and instead will always attempt to select an interactable that it is
    /// hovering (though will not perform exclusive selection of that interactable).
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("XR/XR Socket Interactor")]
    public class XRSocketInteractor : XRBaseInteractor
    {
        [Header("Socket")]

        [SerializeField]
        bool m_ShowInteractableHoverMeshes = true;
        /// <summary>Gets or sets whether this socket should show a mesh at socket's attach point for interactables that it is hovering over.</summary>
        public bool showInteractableHoverMeshes { get { return m_ShowInteractableHoverMeshes; } set { m_ShowInteractableHoverMeshes = value; } }

        [SerializeField]
        Material m_InteractableHoverMeshMaterial;
        /// <summary>Gets or sets material used for rendering interactable meshes on hover (a default material will be created if none is supplied).</summary>
        Material interactableHoverMeshMaterial { get { return m_InteractableHoverMeshMaterial; } set { m_InteractableHoverMeshMaterial = value; } }

        [SerializeField]
        bool m_SocketActive = true;
        /// <summary>Gets or sets whether socket interaction is enabled.</summary>
        public bool socketActive { get { return m_SocketActive; } set { m_SocketActive = value; } }

        [SerializeField]
        float m_InteractableHoverScale = 1.0f;
        /// <summary>Gets or sets Scale at which to render hovered interactable.</summary>
        public float interactableHoverScale { get { return m_InteractableHoverScale; } set { m_InteractableHoverScale = value; } }

        [SerializeField]
        float m_RecycleDelayTime = 1.0f;
        /// <summary>Sets the amount of time the socket will refuse hovers after an object is removed.</summary>
        public float recycleDelayTime { get { return m_RecycleDelayTime; } set { m_RecycleDelayTime = value; } }
        float m_LastRemoveTime = -100.0f;


        // reusable list of valid targets
        List<XRBaseInteractable> m_ValidTargets = new List<XRBaseInteractable>();

        // reusable map of interactables to their distance squared from this interactor (used for sort)
        Dictionary<XRBaseInteractable, float> m_InteractableDistanceSqrMap = new Dictionary<XRBaseInteractable, float>();

        Dictionary<XRBaseInteractable, MeshFilter[]> m_MeshFilterCache = new Dictionary<XRBaseInteractable, MeshFilter[]>();

        // Sort comparison function used by GetValidTargets
        Comparison<XRBaseInteractable> m_InteractableSortComparison;
        int InteractableSortComparison(XRBaseInteractable x, XRBaseInteractable y)
        {
            float xDistance = m_InteractableDistanceSqrMap[x];
            float yDistance = m_InteractableDistanceSqrMap[y];
            if (xDistance > yDistance)
                return 1;
            if (xDistance < yDistance)
                return -1;
            else
                return 0;
        }

        protected override void Awake()
        {
            base.Awake();

            m_InteractableSortComparison = InteractableSortComparison;
            if (m_InteractableHoverMeshMaterial == null)
            {
                // create of default transparent shader
                var m_InteractableHoverMeshMaterial = new Material(Shader.Find("Standard"));
                if (m_InteractableHoverMeshMaterial)
                {
                    m_InteractableHoverMeshMaterial.SetFloat("_Mode", 2);
                    m_InteractableHoverMeshMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    m_InteractableHoverMeshMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    m_InteractableHoverMeshMaterial.SetInt("_ZWrite", 0);
                    m_InteractableHoverMeshMaterial.DisableKeyword("_ALPHATEST_ON");
                    m_InteractableHoverMeshMaterial.EnableKeyword("_ALPHABLEND_ON");
                    m_InteractableHoverMeshMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    m_InteractableHoverMeshMaterial.SetColor("_Color", new Color(0.0f, 0.0f, 1.0f, 0.6f));
                }
                else
                    Debug.LogWarning("Failed to create default transparent material for Socket Interactor.", this);
            }
        }

        protected internal override void OnHoverEnter(XRBaseInteractable interactable)
        {
            base.OnHoverEnter(interactable);
            MeshFilter[] interactableMeshFilters = interactable.GetComponentsInChildren<MeshFilter>();
            if(interactableMeshFilters.Length > 0)
            {
                m_MeshFilterCache.Add(interactable, interactableMeshFilters);
            }
        }
        protected internal override void OnHoverExit(XRBaseInteractable interactable)
        {
            base.OnHoverExit(interactable);
            m_MeshFilterCache.Remove(interactable);
        }

        protected internal override void OnSelectExit(XRBaseInteractable interactable)
        {
            base.OnSelectExit(interactable);
            m_LastRemoveTime = Time.time;
        }
        
        public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractor(updatePhase);

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic && m_ShowInteractableHoverMeshes && m_HoverTargets.Count > 0)
                DrawHoveredInteractables();
        }

        protected void OnTriggerEnter(Collider col)
        {
            var interactable = interactionManager.TryGetInteractableForCollider(col);
            if (interactable && !m_ValidTargets.Contains(interactable) && selectTarget != interactable)
                m_ValidTargets.Add(interactable);
        }

        protected void OnTriggerExit(Collider col)
        {
            var interactable = interactionManager.TryGetInteractableForCollider(col);
            if (interactable && m_ValidTargets.Contains(interactable) && selectTarget != interactable)
                m_ValidTargets.Remove(interactable);
        }

        Matrix4x4 GetInteractableAttachMatrix(XRGrabInteractable interactable, Vector3 scale)
        {
            var interactableLocalPosition = Vector3.zero;
            var interactableLocalRotation = Quaternion.identity;
            var interactableScale = interactable.transform.lossyScale;
            if (interactable.attachTransform != null)
            {
                interactableLocalPosition = interactable.attachTransform.localPosition;
                interactableLocalRotation = interactable.attachTransform.localRotation; 
                interactableScale = interactable.attachTransform.lossyScale;
            }

            Vector3 finalPosition = attachTransform.position - interactableLocalPosition;
            Quaternion finalRotation = attachTransform.rotation * interactableLocalRotation;
            return Matrix4x4.TRS(finalPosition, finalRotation, Vector3.Scale(scale, interactableScale));
        }

        protected virtual void DrawHoveredInteractables()
        {
            if (interactableHoverMeshMaterial == null)
                return;
            float hoveredScale = Mathf.Max(0.0f, m_InteractableHoverScale);
            for (int i = 0; i < m_HoverTargets.Count; i++)
            {
                var hoverTarget = m_HoverTargets[i] as XRGrabInteractable;
                if (hoverTarget == null || hoverTarget == selectTarget)
                    continue;

                MeshFilter[] interactableMeshFilters = null;
                if (m_MeshFilterCache.TryGetValue(hoverTarget, out interactableMeshFilters))
                {
                    if (interactableMeshFilters != null && interactableMeshFilters.Length > 0)
                    {
                        foreach (var meshFilter in interactableMeshFilters)
                        {
                            if (meshFilter != null && (Camera.main.cullingMask & (1 << meshFilter.gameObject.layer)) != 0)
                            {
                                Graphics.DrawMesh(meshFilter.sharedMesh,
                                    GetInteractableAttachMatrix(hoverTarget, meshFilter.transform.lossyScale * hoveredScale),
                                    interactableHoverMeshMaterial, gameObject.layer);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retrieve the list of interactables that this interactor could possibly interact with this frame.
        /// This list is sorted by priority (in this case distance).
        /// </summary>
        /// <param name="validTargets">Populated List of interactables that are valid for selection or hover.</param>
        public override void GetValidTargets(List<XRBaseInteractable> validTargets)
        {
            validTargets.Clear();
            m_InteractableDistanceSqrMap.Clear();

            // Calculate distance squared to interactor's attach transform and add to validTargets (which is sorted before returning)
            foreach (var interactable in m_ValidTargets)
            {
                if (interactable != selectTarget)
                {
                    m_InteractableDistanceSqrMap[interactable] = interactable.GetDistanceSqrToInteractor(this);
                    validTargets.Add(interactable);
                }
            }

            validTargets.Sort(m_InteractableSortComparison);
        }

        /// <summary>Gets whether this interactor is in a state where it could hover (always true for sockets if active).</summary>
        public override bool isHoverActive { get { return m_SocketActive; } }

        /// <summary>Gets whether this interactor is in a state where it could select (always true for sockets if active).</summary>
        public override bool isSelectActive { get { return m_SocketActive; } }

        /// <summary>Gets if this interactor requires exclusive selection of an interactable (always false for sockets).</summary>
        public override bool isSelectExclusive { get { return false; } }

        /// <summary>Gets the movement type to use when overriding the selected interactable's movement (always Kinematic for sockets).</summary>
        public override XRBaseInteractable.MovementType? selectedInteractableMovementTypeOverride
        {
            get { return XRBaseInteractable.MovementType.Kinematic; }
        }

        /// <summary>Determines if the interactable is valid for selection this frame.</summary>
        /// <param name="interactable">Interactable to check.</param>
        /// <returns><c>true</c> if the interactable can be selected this frame.</returns>
        public override bool CanSelect(XRBaseInteractable interactable)
        {
            return base.CanSelect(interactable) && (selectTarget == null || selectTarget == interactable);
        }

        public override bool CanHover(XRBaseInteractable interactable)
        {
            bool baseRet = base.CanHover(interactable);
            return baseRet && Time.time > m_LastRemoveTime + m_RecycleDelayTime;
        }
    }
}
