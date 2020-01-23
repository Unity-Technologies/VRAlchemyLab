using UnityEngine;
using UnityEngine.XR;
using GameplayIngredients.Actions;

public class XRSetEyeTextureResolutionScaleAction : ActionBase
{
    public float scale = 1.0f;

    public override void Execute(GameObject instigator = null)
    {
        XRSettings.eyeTextureResolutionScale = scale;
    }
}