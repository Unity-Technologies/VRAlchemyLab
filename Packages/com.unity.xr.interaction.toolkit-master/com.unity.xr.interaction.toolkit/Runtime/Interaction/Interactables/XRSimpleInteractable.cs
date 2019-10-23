using UnityEngine;
using System;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// This is the simplest version of an Interactable object.
    /// It simply provides a public implementation of the XRBaseInteractable. 
    /// It is intended to be used as a way to respond to OnHoverEnter/Exit and OnSelectEnter/Exit events with no underlying interaction behaviour.
    /// </summary>
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu("XR/XR Simple Interactable")]
    public class XRSimpleInteractable : XRBaseInteractable
    {


    }
}
