using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class OpenBook : MonoBehaviour
{
    XRGrabInteractable m_InteractableBase;
    Animator m_Animator;

    const string k_AnimBookOpen = "BookOpen";
    const string k_AnimBookClose = "BookClose";
  
    bool m_Primary2DAxis;
    
    // Start is called before the first frame update
    void Start()
    {
        m_InteractableBase = GetComponent<XRGrabInteractable>();
        m_Animator = GetComponent<Animator>();
       
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Primary2DAxis)
        {
            m_Animator.SetTrigger(k_AnimBookOpen);
        }

        else 
        {
            m_Animator.SetTrigger(k_AnimBookClose);
        }
    }
}
