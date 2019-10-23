using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Simple Interactable Visual component that demonstrates hover or selection state with emissive tinting.
    /// Note: requires use of a shader that supports emission (such as Standard shader).
    /// </summary>
    [AddComponentMenu("XR/Helpers/XR Tint Interactable Visual")]
    [DisallowMultipleComponent]
    public class XRTintInteractableVisual : MonoBehaviour
    {
        [SerializeField, Tooltip("Tint color for interactable.")]
        Color m_TintColor = Color.yellow;
        /// <summary>Gets or sets the tint color for interactable.</summary>
        public Color tintColor { get { return m_TintColor; } set { m_TintColor = value; } }

        [SerializeField, Tooltip("Tint on hover.")]
        bool m_TintOnHover = true;
        /// <summary>Gets or sets whether this should tint on hover.</summary>
        public bool tintOnHover { get { return m_TintOnHover; } set { m_TintOnHover = value; } }

        [SerializeField, Tooltip("Tint on selection.")]
        bool m_TintOnSelection = true;
        /// <summary>Gets or sets whether this should tint on selection.</summary>
        public bool tintOnSelection { get { return m_TintOnSelection; } set { m_TintOnSelection = value; } }

        [SerializeField, Tooltip("Renderer(s) to use for tinting (will default to Renderer on GameObject if not specified).")]
        List<Renderer> m_TintRenderers = new List<Renderer>();
        /// <summary>Gets or sets the tint renderer(s).</summary>
        public List<Renderer> tintRenderers { get { return m_TintRenderers; } set { m_TintRenderers = value; } }

        XRBaseInteractable m_Interactable;

        void Awake()
        {
            m_Interactable = GetComponent<XRBaseInteractable>();
            if (m_Interactable)
            {
                m_Interactable.onHoverEnter.AddListener(OnFirstHoverEnter);
                m_Interactable.onLastHoverExit.AddListener(OnLastHoverExit);
                m_Interactable.onSelectEnter.AddListener(OnSelectEnter);
                m_Interactable.onSelectExit.AddListener(OnSelectExit);
            }
            else
                Debug.LogWarning("Could not find interactable for helper.", this);

            if (m_TintRenderers.Count == 0)
            {
                m_TintRenderers = new List<Renderer>(GetComponents<Renderer>());
                if (m_TintRenderers.Count == 0)
                    Debug.LogWarning("Could not find required Renderer component.", this);
            }
        }

        void Destroy()
        {
            if (m_Interactable)
            {
                m_Interactable.onFirstHoverEnter.RemoveListener(OnFirstHoverEnter);
                m_Interactable.onLastHoverExit.RemoveListener(OnLastHoverExit);
                m_Interactable.onSelectEnter.RemoveListener(OnSelectEnter);
                m_Interactable.onSelectExit.RemoveListener(OnSelectExit);
            }
        }

        protected virtual void SetTint(bool on)
        {
            foreach (var render in m_TintRenderers)
            {
                if (render && render.material)
                {
                    if (on)
                    {
                        Color finalColor = m_TintColor * Mathf.LinearToGammaSpace(1.0f);
                        render.material.SetColor("_EmissionColor", finalColor);
                        render.material.EnableKeyword("_EMISSION");
                    }
                    else
                        render.material.DisableKeyword("_EMISSION");
                }
            }
        }

        void OnFirstHoverEnter(XRBaseInteractor interactor)
        {
            SetTint(true);
        }
        void OnLastHoverExit(XRBaseInteractor interactor)
        {
            SetTint(false);
        }
        void OnSelectEnter(XRBaseInteractor interactor)
        {
            if (m_TintOnSelection)
                SetTint(true);
        }
        void OnSelectExit(XRBaseInteractor interactor)
        {
            if (m_TintOnSelection)
                SetTint(false);
        }
    }
}