using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEditor;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>Get line points and hit point info for rendering </summary>
    interface ILineRenderable
    {        
        bool GetLinePoints(ref Vector3[] linePoints, ref int noPoints);
        bool TryGetHitInfo(ref Vector3 position, ref Vector3 normal, ref int positionInLine, ref bool isValidTarget);
    }    

    /// <summary>
    /// Interactor helper object aligns a LineRenderer with the Interactor.
    /// </summary>
    [AddComponentMenu("XR/Helpers/XR Interactor Line Visual")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LineRenderer))]
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_LineVisual)]
    public class XRInteractorLineVisual : MonoBehaviour, IXRCustomReticleProvider
    {
        const float     k_MinLineWidth          = 0.0001f;
        const float     k_MaxLineWidth          = 0.05f;

        [SerializeField]	
        [Range(k_MinLineWidth, k_MaxLineWidth)]float m_LineWidth = 0.02f;	
        /// <summary>Gets or sets the width of the line (in centimeters).</summary>	
        public float lineWidth { get { return m_LineWidth; } set { m_LineWidth = value; m_PerformSetup = true; } }

        [SerializeField]
        bool m_OverrideInteractorLineLength = true;
        /// <summary>Gets or sets the width of the line (in centimeters).</summary>	
        public bool overrideInteractorLineLength { get { return m_OverrideInteractorLineLength; } set { m_OverrideInteractorLineLength = value; } }

        [SerializeField]
        float m_LineLength = 10.0f;
        /// <summary>Gets or sets the width of the line (in centimeters).</summary>	
        public float lineLength { get { return m_LineLength; } set { m_LineLength = value; } }

        [SerializeField]	
        AnimationCurve m_WidthCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);	
        /// <summary>Gets or sets the relative width of the line from the start to the end.</summary>	
        AnimationCurve widthCurve { get { return m_WidthCurve; } set { m_WidthCurve = value; m_PerformSetup = true; } }

        [SerializeField]
        Gradient m_ValidColorGradient = new Gradient()
        {            
            colorKeys = new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f)}, 
            alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1.0f) }
        };
        /// <summary>Gets or sets the color of the line as a gradient from start to end to indicate a valid state.</summary>	
        public Gradient validColorGradient { get { return m_ValidColorGradient; } set { m_ValidColorGradient = value; } }
               
        [SerializeField]	
        Gradient m_InvalidColorGradient = new Gradient()
        {            
            colorKeys = new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.red, 1.0f)}, 
            alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1.0f) }
        };
        /// <summary>Gets or sets the color of the line as a gradient from start to end to indicate an invalid state.</summary>	
        public Gradient invalidColorGradient { get { return m_InvalidColorGradient; } set { m_InvalidColorGradient = value; } }

        [SerializeField]
        bool m_SmoothMovement = false;
        /// <summary>Gets or sets if enabled, the rendered segments will be delayed from and smoothly follow the target segments.</summary>
        public bool smoothMovement { get { return m_SmoothMovement; } set { m_SmoothMovement = value; } }

        [SerializeField]
        float m_FollowTightness = 10f;
        /// <summary>Gets or sets the speed that the rendered segments will follow the target segments.</summary>
        public float followTightness { get { return m_FollowTightness; } set { m_FollowTightness = value; } }

        [SerializeField]
        float m_SnapThresholdDistance = 10f;
        /// <summary>Gets or sets the threshold distance to snap line points when smoothMovement is enabled.</summary>
        public float snapThresholdDistance { get { return m_SnapThresholdDistance; } set { m_SnapThresholdDistance = value; } }
        
        [SerializeField]
        GameObject m_Reticle;
        /// <summary>Gets or sets the reticle that will appear at the end of the line when it is valid.</summary>
        public GameObject reticle { get { return m_Reticle; } set { m_Reticle = value; } }

        [SerializeField]
        bool m_StopLineAtFirstRaycastHit = true;
        /// <summary>Sets whether we cut the line off at the first raycast hit..</summary>
        public bool stopLineAtFirstRaycastHit { get { return m_StopLineAtFirstRaycastHit; } set { m_StopLineAtFirstRaycastHit = value; } }

        Vector3 m_ReticlePos = new Vector3(), m_ReticleNormal = new Vector3();
        int m_EndPositionInLine= 0;

        bool m_SnapCurve = true;
        bool m_PerformSetup = false;
        bool m_PreviousHit = false;
        bool m_CurrentHit = false;
        GameObject m_ReticleToUse;


        LineRenderer m_LineRenderer;

        // interface to get target point
        ILineRenderable m_LineRenderable;

        // reusable lists of target points
        Vector3[] m_TargetPoints = null;
        int m_NoTargetPoints = -1;

        // reusable lists of rendered points
        Vector3[] m_RenderPoints = null;
        int m_NoRenderPoints = -1;

        // reusable lists of rendered points to smooth movement
        Vector3[] m_PreviousRenderPoints = null;
        int m_NoPreviousRenderPoints = -1;

        Vector3[] m_ClearArray = new[] { Vector3.zero, Vector3.zero };

        void Awake()
        {
            m_LineRenderable = GetComponent<ILineRenderable>();

            if(m_Reticle != null)
                m_Reticle.SetActive(false);

            UpdateSettings();     
        }

        void OnEnable()
        {            
            m_SnapCurve = true;
            m_ReticleToUse = null;

            Reset();
        }

        void OnDisable()
        {
            if (m_LineRenderer)
                m_LineRenderer.enabled = false;
            m_ReticleToUse = null;
        }

        void Reset()
        {
            if (TryFindLineRenderer())
            {
                ClearLineRenderer();
                UpdateSettings();
            }
        }

        void ClearLineRenderer()
        {
            if (TryFindLineRenderer())
            {
                m_LineRenderer.SetPositions(m_ClearArray);
                m_LineRenderer.positionCount = 0;
            }
        }

        void Update()
        {
            if (m_PerformSetup)
            {
                UpdateSettings();
                m_PerformSetup = false;
            }
            if (m_LineRenderable == null)
            {
                m_LineRenderer.enabled = false;
                return;
            }

            UpdateLineVisual();            
        }                       

        void UpdateLineVisual()
        {
            if (m_LineRenderer == null)
                return;
          
            m_NoRenderPoints = 0;

            // get all the line sample points from the ILineRenderable interface
            if (!m_LineRenderable.GetLinePoints(ref m_TargetPoints, ref m_NoTargetPoints))
            {
                m_LineRenderer.enabled = false;
                ClearLineRenderer();
                return;
            }            

            // sanity check.
            if (m_TargetPoints == null || 
                m_TargetPoints.Length == 0 || 
                m_NoTargetPoints == 0 || 
                m_NoTargetPoints > m_TargetPoints.Length)
            {
                m_LineRenderer.enabled = false;
                ClearLineRenderer();
                return;
            }

            // make sure we have the correct sized arrays for everything.
            if(m_RenderPoints == null || m_RenderPoints.Length < m_NoTargetPoints)
            {
                m_RenderPoints = new Vector3[m_NoTargetPoints];
                m_NoRenderPoints = 0;
            }
            if(m_PreviousRenderPoints == null)
            {
                m_PreviousRenderPoints = new Vector3[m_NoTargetPoints];
                m_NoPreviousRenderPoints = 0;
            }         

            // if there is a big movement (snap turn, teleportation), snap the curve
            if (m_PreviousRenderPoints.Length != m_NoTargetPoints)
            {
                m_SnapCurve = true;
            }
            else
            {
                // compare the two endpoints of the curve. as that will have the largest delta.
                if (m_PreviousRenderPoints != null && 
                    m_NoPreviousRenderPoints > 0 && 
                    m_NoPreviousRenderPoints <= m_PreviousRenderPoints.Length &&
                    m_TargetPoints != null &&
                    m_NoTargetPoints > 0 &&
                    m_NoTargetPoints <= m_TargetPoints.Length)
                {
                    int prevPointIndex = m_NoPreviousRenderPoints - 1;
                    int currPointIndex = m_NoTargetPoints - 1;
                    if (Vector3.Distance(m_PreviousRenderPoints[prevPointIndex], m_TargetPoints[currPointIndex]) > m_SnapThresholdDistance)
                    {
                        m_SnapCurve = true;
                    }
                }
            }

            bool isValidTarget = false;
            // if the line hits, insert reticle position into the list for smoothing 
            // remove the last point in the list to keep the number of points consistent
            // we reset m_CurrentHit at the end of the code path
            if (m_LineRenderable.TryGetHitInfo(ref m_ReticlePos, ref m_ReticleNormal, ref m_EndPositionInLine, ref isValidTarget))
            {

                // end the line at the current hit point.
                if (m_EndPositionInLine <= m_NoTargetPoints && m_EndPositionInLine > 0)
                {
                    m_TargetPoints[m_EndPositionInLine] = m_ReticlePos;                   
                    m_NoTargetPoints = m_EndPositionInLine + 1;
                }

                m_CurrentHit = true;
            }
    
            if (m_SmoothMovement && (m_NoPreviousRenderPoints == m_NoTargetPoints) && !m_SnapCurve)
            {
                // smooth movement by having render points follow target points
                int maxRenderPoints = m_RenderPoints.Length;
                float length = 0;
                for (int i = 0; i < m_NoTargetPoints && m_NoRenderPoints < maxRenderPoints; i++)
                {
                    if(m_OverrideInteractorLineLength)
                    {
                        Vector3 smoothPoint = Vector3.Lerp(m_PreviousRenderPoints[i], m_TargetPoints[i], m_FollowTightness * Time.deltaTime);
                        if(m_NoRenderPoints > 0 && m_RenderPoints.Length > 0)
                        {
                            float segLength = Vector3.Distance(m_RenderPoints[m_NoRenderPoints - 1], smoothPoint);
                            length += segLength;
                            if(length > m_LineLength)
                            {
                                float delta = length - m_LineLength;
                                // re-project final point to match the desired length
                                smoothPoint = Vector3.Lerp(m_RenderPoints[m_NoRenderPoints - 1], smoothPoint, delta / segLength);
                                m_RenderPoints[m_NoRenderPoints] = smoothPoint;
                                m_NoRenderPoints++;
                                break;
                            }
                            else
                            {
                                m_RenderPoints[m_NoRenderPoints] = smoothPoint;
                                m_NoRenderPoints++;
                            }
                        }
                        else
                        {
                            m_RenderPoints[m_NoRenderPoints] = smoothPoint;
                            m_NoRenderPoints++;
                        }
                    }
                    else
                    {
                        Vector3 smoothPoint = Vector3.Lerp(m_PreviousRenderPoints[i], m_TargetPoints[i], m_FollowTightness * Time.deltaTime);
                        m_RenderPoints[m_NoRenderPoints] = smoothPoint;
                        m_NoRenderPoints++;
                    }                   
                }

            }
            else
            {
                if (m_OverrideInteractorLineLength)
                {
                    float length = 0;
                    int maxRenderPoints = m_RenderPoints.Length;
                    for (int i = 0; i < m_NoTargetPoints && m_NoRenderPoints < maxRenderPoints; i++)
                    {
                        Vector3 newPoint = m_TargetPoints[i];
                        if (m_NoRenderPoints > 0 && m_RenderPoints.Length > 0)
                        {
                            float segLength = Vector3.Distance(m_RenderPoints[m_NoRenderPoints - 1], newPoint);
                            length += segLength;
                            if (length > m_LineLength)
                            {
                                float delta = length - m_LineLength;
                                // re-project final point to match the desired length
                                Vector3 resolvedPoint = Vector3.Lerp(m_RenderPoints[m_NoRenderPoints - 1], newPoint, 1-(delta / segLength));
                                m_RenderPoints[m_NoRenderPoints] = resolvedPoint;
                                m_NoRenderPoints++;
                                break;
                            }
                            else
                            {
                                m_RenderPoints[m_NoRenderPoints] = newPoint;
                                m_NoRenderPoints++;
                            }
                        }
                        else
                        {
                            m_RenderPoints[m_NoRenderPoints] = newPoint;
                            m_NoRenderPoints++;
                        }
                    }
                }
                else
                {
                    Array.Copy(m_TargetPoints, m_RenderPoints, m_NoTargetPoints);
                    m_NoRenderPoints = m_NoTargetPoints;
                }
            }

            // when a straight line has only two points and color gradients have more than two keys,
            // interpolate points between the two points to enable better color gradient effects            
            if (isValidTarget)
            { 
                m_LineRenderer.colorGradient = m_ValidColorGradient;
                // set reticle position and show reticle
                m_ReticleToUse = m_CustomReticleAttached ? m_CustomReticle : m_Reticle;
                if (m_ReticleToUse != null)
                {
                    m_ReticleToUse.transform.position = m_ReticlePos;
                    m_ReticleToUse.transform.up = m_ReticleNormal;
                    m_ReticleToUse.SetActive(true);
                }

            }
            else
            {
                m_LineRenderer.colorGradient = m_InvalidColorGradient;
                m_ReticleToUse = m_CustomReticleAttached ? m_CustomReticle : m_Reticle;
                if (m_ReticleToUse != null)
                {
                    m_Reticle.SetActive(false);
                }
            }

            if (m_NoRenderPoints >= 2)
            {
                m_LineRenderer.enabled = true;
                m_LineRenderer.positionCount = m_NoRenderPoints;
                m_LineRenderer.SetPositions(m_RenderPoints);                
            }
            else
            {
                m_LineRenderer.enabled = false;
                ClearLineRenderer();
                return;
            }

            // update previous points            
            Array.Copy(m_RenderPoints, m_PreviousRenderPoints, m_NoRenderPoints);
            m_NoPreviousRenderPoints = m_NoRenderPoints;
            m_PreviousHit = m_CurrentHit;
            m_SnapCurve = false; 
            m_CurrentHit = false;
            m_ReticleToUse = null;
        }


        void OnValidate()
        {
            UpdateSettings();
        }

        void UpdateSettings()
        {
            if (TryFindLineRenderer())
            {
                m_LineRenderer.widthMultiplier =  Mathf.Clamp(m_LineWidth, k_MinLineWidth, k_MaxLineWidth);
                m_LineRenderer.widthCurve = m_WidthCurve;                
                m_SnapCurve = true;           
            }                       
        }
        
        bool TryFindLineRenderer()
        {
            m_LineRenderer = GetComponent<LineRenderer>();
            if (!m_LineRenderer)
            {
                Debug.LogWarning("No Line Renderer found for Interactor Line Visual.", this);
                enabled = false;
                return false;
            }
            return true;
        }

        GameObject m_CustomReticle = null;
        bool m_CustomReticleAttached = false;      

        public bool AttachCustomReticle(GameObject reticle)
        {
            if (!m_CustomReticleAttached)
            {
                if(m_Reticle != null)
                {
                    m_Reticle.SetActive(false);
                }
            }
            else
            {
                if(m_CustomReticle != null)
                {
                    m_CustomReticle.SetActive(false);
                }                
            }

            m_CustomReticle = reticle;
            if(m_CustomReticle != null)
            {
                m_CustomReticle.SetActive(true);
            }
            
            m_CustomReticleAttached = true;            
            return false;
        }

        public bool RemoveCustomReticle()
        {
            if(m_CustomReticle != null)
            {
                m_CustomReticle.SetActive(false);
            }
            if(m_Reticle != null)
            {
                m_Reticle.SetActive(true);
            }

            m_CustomReticle = null;
            m_CustomReticleAttached = false;
            return false;
        }
    }      
    
}
