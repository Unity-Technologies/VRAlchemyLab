using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.UI;
using UnityEditor;

namespace UnityEngine.XR.Interaction
{
    /// <summary>Get line points and hit point info for rendering </summary>
    interface ILineRenderable
    {
        bool GetLinePoints(List<Vector3> linePoints);
        bool TryGetHitInfo(ref Vector3 position, ref Vector3 normal, ref int positionInLine, ref bool isValidTarget);
    }    

    /// <summary>
    /// Interactor helper object aligns a LineRenderer with the Interactor.
    /// </summary>
    [AddComponentMenu("XR/Helpers/XR Interactor Line Visual")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LineRenderer))]
    public class XRInteractorLineVisual : MonoBehaviour
    {
        const float     k_MinLineWidth          = 0.0001f;
        const float     k_MaxLineWidth          = 0.05f;

        [SerializeField]	
        [Range(k_MinLineWidth, k_MaxLineWidth)]float m_LineWidth = 0.02f;	
        /// <summary>Gets or sets the width of the line (in centimeters).</summary>	
        public float lineWidth { get { return m_LineWidth; } set { m_LineWidth = value; m_PerformSetup = true; } }	

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
        public Gradient validColorGradient { get { return m_ValidColorGradient; } set { m_ValidColorGradient = value; m_PerformSetup = true; } }
               
        [SerializeField]	
        Gradient m_InvalidColorGradient = new Gradient()
        {            
            colorKeys = new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.red, 1.0f)}, 
            alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1.0f) }
        };
        /// <summary>Gets or sets the color of the line as a gradient from start to end to indicate an invalid state.</summary>	
        public Gradient invalidColorGradient { get { return m_InvalidColorGradient; } set { m_InvalidColorGradient = value; m_PerformSetup = true; } }

        [SerializeField]
        bool m_SmoothMovement = false;
        /// <summary>Gets or sets if enabled, the rendered segments will be delayed from and smoothly follow the target segments.</summary>
        public bool smoothMovement { get { return m_SmoothMovement; } set { m_SmoothMovement = value; m_PerformSetup = true; } }

        [SerializeField]
        float m_FollowTightness = 10f;
        /// <summary>Gets or sets the speed that the rendered segments will follow the target segments.</summary>
        public float followTightness { get { return m_FollowTightness; } set { m_FollowTightness = value; m_PerformSetup = true; } }

        [SerializeField]
        float m_SnapThresholdDistance = 10f;
        /// <summary>Gets or sets the threshold distance to snap line points when smoothMovement is enabled.</summary>
        public float snapThresholdDistance { get { return m_SnapThresholdDistance; } set { m_SnapThresholdDistance = value; m_PerformSetup = true; } }
        
        [SerializeField]
        GameObject m_Reticle;
        /// <summary>Gets or sets the reticle that will appear at the end of the line when it is valid.</summary>
        public GameObject reticle { get { return m_Reticle; } set { m_Reticle = value; m_PerformSetup = true; } }
               
        Vector3 m_ReticlePos = new Vector3(), m_ReticleNormal = new Vector3();
        int m_EndPositionInLine= 0;

        bool m_SnapCurve = true;
        bool m_PerformSetup = false;
        bool m_PreviousHit = false;

        bool m_IsValidTarget;
               
        LineRenderer m_LineRenderer;

        // interface to get target point
        ILineRenderable m_LineRenderable;
        
        // reusable lists of target points
        List<Vector3> m_TargetPoints = new List<Vector3>();            

        // reusable lists of rendered points
        List<Vector3> m_RenderPoints = new List<Vector3>();

        // reusable lists of rendered points to smooth movement
        List<Vector3> m_PreviousRenderPoints = new List<Vector3>();

        // reusable lists of rendered points to enable color gradient
        List<Vector3> m_ExtendedRenderPoints = new List<Vector3>();
        
        void Awake()
        {
            m_LineRenderable = GetComponent<ILineRenderable>();

            if(m_Reticle != null)
                m_Reticle.SetActive(false);

            UpdateSettings();     
        }

        void OnEnable()
        {
            if (m_LineRenderer)
                m_LineRenderer.enabled = true;
            m_SnapCurve = true;
        }

        void OnDisable()
        {
            if (m_LineRenderer)
                m_LineRenderer.enabled = false;
        }

        void Reset()
        {
            if (TryFindLineRenderer())
            {
                m_LineRenderer.SetPositions(new[] { Vector3.zero, Vector3.zero });
                UpdateSettings();
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
            m_LineRenderer.enabled = true;

            // get all the line sample points from the ILineRenderable interface
            if (!m_LineRenderable.GetLinePoints(m_TargetPoints))
                return;            

            int targetPointCount = m_TargetPoints.Count;

            m_RenderPoints.Clear();

            // if there is a big movement (snap turn, teleportation), snap the curve
            if (m_PreviousRenderPoints.Count != targetPointCount)
            {
                m_SnapCurve = true;
            }
            else
            {
                for (int i = 0; i < targetPointCount; i++)
                {
                    if (Vector3.Distance(m_PreviousRenderPoints[i], m_TargetPoints[i]) > m_SnapThresholdDistance)
                    {
                        m_SnapCurve = true;
                        break;
                    }
                }
            }
                

            // if the line hits, insert reticle position into the list for smoothing 
            // remove the last point in the list to keep the number of points consistent
            if (m_LineRenderable.TryGetHitInfo(ref m_ReticlePos, ref m_ReticleNormal, ref m_EndPositionInLine, ref m_IsValidTarget))
            {
                    
                m_TargetPoints.Insert(m_EndPositionInLine, m_ReticlePos);
                m_TargetPoints.RemoveAt(m_TargetPoints.Count - 1);

                if (m_SmoothMovement && (m_PreviousRenderPoints.Count == targetPointCount) && !m_SnapCurve && m_PreviousHit)
                {
                    // smooth movement by having render points follow target points
                    for (int i = 0; i < targetPointCount; i++)
                    {
                        Vector3 smoothPoint = Vector3.Lerp(m_PreviousRenderPoints[i], m_TargetPoints[i], m_FollowTightness * Time.deltaTime);
                        m_RenderPoints.Add(smoothPoint);
                    }
                }
                else
                {
                    m_RenderPoints.AddRange(m_TargetPoints);
                }

                // update hit state to enable snap when next frame the curve does not hit
                m_PreviousHit = true;

                // set reticle position and show reticle
                if (m_Reticle != null)
                {
                    m_Reticle.transform.position = m_RenderPoints[m_EndPositionInLine];
                    m_Reticle.transform.up = m_ReticleNormal;
                    m_Reticle.SetActive(true);
                }                               

                // when a straight line has only two points and color gradients have more than two keys,
                // interpolate points between the two points to enable better color gradient effects
                int maxColorGradientCount = Mathf.Max(validColorGradient.alphaKeys.Length,
                    validColorGradient.colorKeys.Length, invalidColorGradient.alphaKeys.Length, invalidColorGradient.colorKeys.Length);

                if (m_EndPositionInLine == 1 && maxColorGradientCount > 2)
                {
                    m_ExtendedRenderPoints.Clear();
                    int newPointsCount = maxColorGradientCount * 2;
                    for (int i = 0; i < newPointsCount; i++)
                    {
                        Vector3 interPoint = Vector3.Lerp(m_RenderPoints[0], m_RenderPoints[1], (float)(i) / newPointsCount);
                        m_ExtendedRenderPoints.Add(interPoint);
                    }
                    m_LineRenderer.positionCount = newPointsCount;
                    m_LineRenderer.SetPositions(m_ExtendedRenderPoints.ToArray());
                }
                else
                {
                    // only render lines before the reticle
                    m_LineRenderer.positionCount = m_EndPositionInLine + 1;
                    for (int i = 0; i < m_LineRenderer.positionCount; i++)
                    {
                        m_LineRenderer.SetPosition(i, m_RenderPoints[i]);
                    }
                }                                           

                m_LineRenderer.colorGradient = m_IsValidTarget? m_ValidColorGradient : m_InvalidColorGradient;
                    
            }

            else // does not hit, show the invalid line
            {
                if (m_SmoothMovement && m_PreviousRenderPoints.Count == targetPointCount && !m_SnapCurve && !m_PreviousHit) // may need to deal with teleportation as well
                {
                    // smooth movement by having render points follow target points
                    for (int i = 0; i < targetPointCount; i++)
                    {
                        Vector3 smoothPoint = Vector3.Lerp(m_PreviousRenderPoints[i], m_TargetPoints[i], m_FollowTightness * Time.deltaTime);
                        m_RenderPoints.Add(smoothPoint);
                    }
                }
                else
                {
                    m_RenderPoints.AddRange(m_TargetPoints);
                }

                // update hit state to enable snap when next frame the curve does not hit
                m_PreviousHit = false;

                // do not show the reticle
                if (m_Reticle != null)
                    m_Reticle.SetActive(false);

                int maxColorGradientCount = Mathf.Max(validColorGradient.alphaKeys.Length,
                    validColorGradient.colorKeys.Length, invalidColorGradient.alphaKeys.Length, invalidColorGradient.colorKeys.Length);

                if (m_RenderPoints.Count == 2 && maxColorGradientCount > 2)
                {
                    m_ExtendedRenderPoints.Clear();
                    int newPointsCount = maxColorGradientCount * 2;
                    for (int i = 0; i < newPointsCount; i++)
                    {
                        Vector3 interPoint = Vector3.Lerp(m_RenderPoints[0], m_RenderPoints[1], (float)(i) / newPointsCount);
                        m_ExtendedRenderPoints.Add(interPoint);
                    }
                    m_LineRenderer.positionCount = newPointsCount;
                    m_LineRenderer.SetPositions(m_ExtendedRenderPoints.ToArray());
                }
                else
                {
                    m_LineRenderer.positionCount = m_RenderPoints.Count;
                    m_LineRenderer.SetPositions(m_RenderPoints.ToArray());
                }

                m_LineRenderer.colorGradient = m_InvalidColorGradient;

            }                       
            
            // update previous points
            m_PreviousRenderPoints.Clear();
            m_PreviousRenderPoints.AddRange(m_RenderPoints);

            m_SnapCurve = false;
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
    }      
    
}
