using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

[VFXBinder("VR Demo/VFXCameraBinder")]
public class VFXCameraBinder : VFXBinderBase
{
    public new Camera camera;
    public ExposedProperty viewProjProperty = "ViewProj";

    public override bool IsValid(VisualEffect component)
    {
        return camera != null && component != null && component.HasMatrix4x4(viewProjProperty);
    }

    public override void UpdateBinding(VisualEffect component)
    {
        Matrix4x4 pm = camera.projectionMatrix;
        Matrix4x4 wcm = camera.worldToCameraMatrix;

        component.SetMatrix4x4("ViewProj", pm * wcm);
    }

}
