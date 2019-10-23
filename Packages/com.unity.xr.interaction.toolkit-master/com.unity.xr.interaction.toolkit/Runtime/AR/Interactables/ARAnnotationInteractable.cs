#if !AR_FOUNDATION_PRESENT

// Stub class definition used to fool version defines that this MonoScript exists (fixed in 19.3)
namespace UnityEngine.XR.Interaction.Toolkit.AR {  public class ARAnnotationInteractable {} }

#else

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit.AR
{
    [Serializable]
    public class ARAnnotation
    {
        /// <summary>
        /// The visualization game object that will become active when the object is hovered over.
        /// </summary>
        public GameObject AnnotationVisualization;

        /// <summary>
        /// Maximum angle (in radians) off of FOV horizontal center to show annotation. 
        /// </summary>
        public float MaxFOVCenterOffsetAngle = 0.25f;
        
        /// <summary>
        /// Minimum range to show annotation at.
        /// </summary>
        public float MinAnnotationRange = 0.0f;
        
        /// <summary>
        /// Maximum range to show annotation at.
        /// </summary>
        public float MaxAnnotationRange = 10.0f;
    }
    
    
    public class ARAnnotationInteractable : ARBaseGestureInteractable
    {
        [SerializeField]
        List<ARAnnotation> m_Annotations = new List<ARAnnotation>();
        
        ARAnnotation m_ViewingAnnotation;
        ARAnnotation m_ViewingAnnotationDotProduct;

        void Update()
        {
            // Disable all annotations if not hovered.
            if (!isHovered)
            {
                foreach (var annotation in m_Annotations)
                {
                    annotation.AnnotationVisualization.SetActive(false);
                }
            }
            else
            {
                var fromCamera = transform.position - Camera.main.transform.position;
                float distSquare = fromCamera.sqrMagnitude;
                fromCamera.y = 0.0f;
                fromCamera.Normalize();
                float dotProd = Vector3.Dot(fromCamera, Camera.main.transform.forward);

                foreach (var annotation in m_Annotations)
                {
                    bool enableThisFrame = 
                        (Mathf.Acos(dotProd) < annotation.MaxFOVCenterOffsetAngle &&
                        distSquare >= Mathf.Pow(annotation.MinAnnotationRange, 2.0f) && 
                        distSquare < Mathf.Pow(annotation.MaxAnnotationRange, 2.0f));
                    if (annotation.AnnotationVisualization != null)
                    {
                        if (enableThisFrame && !annotation.AnnotationVisualization.activeSelf)
                            annotation.AnnotationVisualization.SetActive(true);
                        else if (!enableThisFrame && annotation.AnnotationVisualization.activeSelf)
                            annotation.AnnotationVisualization.SetActive(false);

                        // If enabled, align to camera
                        if (annotation.AnnotationVisualization.activeSelf)
                        {
                            annotation.AnnotationVisualization.transform.rotation =
                                Quaternion.LookRotation(fromCamera, transform.up);
                        }
                    }
                }
            }
        }
    }
}

#endif