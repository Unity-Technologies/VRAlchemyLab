using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;
using UnityEngine.XR.Interaction.Toolkit;

[VFXBinder("VR Demo/XR Ray Interactor Binder")]
public class VFXXRRayInteractorBinder : VFXBinderBase
{
    public XRRayInteractor XRRayInteractor;

    [VFXPropertyBinding("System.Boolean")]
    public ExposedProperty EnabledProperty = "IsEnabled";

    [VFXPropertyBinding("System.Boolean")]
    public ExposedProperty HitProperty = "IsHit";

    [VFXPropertyBinding("UnityEngine.Vector3")]
    public ExposedProperty TargetProperty = "TargetPosition";

    [VFXPropertyBinding("UnityEngine.Vector3")]
    public ExposedProperty TargetNormalProperty = "TargetNormal";

    Vector3 m_Position;
    Vector3 m_Normal;

    public override bool IsValid(VisualEffect component)
    {
        return XRRayInteractor != null
            && component.HasBool(EnabledProperty)
            && component.HasBool(HitProperty)
            && component.HasVector3(TargetProperty)
            && component.HasVector3(TargetNormalProperty);
    }

    public override void UpdateBinding(VisualEffect component)
    {
        int id = 0;
        bool hit = false;

        bool valid = XRRayInteractor.TryGetHitInfo(ref m_Position, ref m_Normal, ref id, ref hit);

        if(XRRayInteractor.isActiveAndEnabled)
        {
            component.SetBool(EnabledProperty, true);

            if (hit && valid)
            {
                component.SetBool(HitProperty, true);
                component.SetVector3(TargetProperty, m_Position);
                component.SetVector3(TargetNormalProperty, m_Normal);
            }
            else
                component.SetBool(HitProperty, false);
        }
        else
            component.SetBool(EnabledProperty, false);

    }
}
